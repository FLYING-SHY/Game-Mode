using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace CoH3FowTool
{
    internal readonly record struct ConsoleState(long Address, bool Open, bool Drawing, float Animation)
    {
        public override string ToString() => $"open={Open}, drawing={Drawing}, animation={Animation:F3}, object=0x{Address:X}";
    }

    /// <summary>Read-only observations for the exact binary whose layout was reversed.</summary>
    internal static class RuntimeState
    {
        const string OriginalSha256 = "21EDB5C05C33E89FB5F9D88A3FC6738817ABD80E0366F648B18ADE999CC975CA";
        const long ModeManagerSlotRva = 0x06BA4430;
        const long ConsoleVtableRva = 0x03E3A5C8;

        public static void ValidateLayout(PeImage pe)
        {
            // Normalize catalog edits so file-apply remains compatible, while
            // unrelated binary changes still invalidate these fixed layout RVAs.
            var normalized = (byte[])pe.Data.Clone();
            foreach (var site in PatchCatalog.Build().Concat(PatchCatalog.BuildLegacy()))
            {
                site.Locate(pe);
                if (!site.IsUsable) throw new InvalidOperationException("运行状态布局未匹配: " + site.Name);
                var original = site.ExpectedOriginalBytes();
                if (original == null || original.Length != site.TargetLength)
                    throw new InvalidOperationException("运行状态布局缺少原始字节: " + site.Name);
                Array.Copy(original, 0, normalized, site.TargetOffset, original.Length);
            }
            if (Convert.ToHexString(SHA256.HashData(normalized)) != OriginalSha256)
                throw new InvalidOperationException("exe 摘要与 build 26443338 的运行状态布局不匹配。");
        }

        static bool ValidPointer(long address) => address >= 0x10000 && address < 0x0000800000000000;
        static long Pointer(ProcessMemory pm, long address)
        {
            long value = BitConverter.ToInt64(pm.Read((IntPtr)address, 8));
            if (!ValidPointer(value)) throw new InvalidOperationException($"无效指针: [0x{address:X}]=0x{value:X}");
            return value;
        }

        public static bool TryConsole(ProcessMemory pm, out ConsoleState state, out string error)
        {
            state = default;
            error = null;
            try
            {
                long module = pm.ModuleBase.ToInt64();
                long manager = Pointer(pm, module + ModeManagerSlotRva);
                byte[] vector = pm.Read((IntPtr)manager, 16);
                long begin = BitConverter.ToInt64(vector, 0), end = BitConverter.ToInt64(vector, 8);
                long length = end - begin;
                if (!ValidPointer(begin) || length < 0 || length > 1024 || length % 8 != 0)
                    throw new InvalidOperationException("Console mode vector bounds are invalid.");
                for (long cursor = begin; cursor < end; cursor += 8)
                {
                    long obj = Pointer(pm, cursor);
                    if (BitConverter.ToInt64(pm.Read((IntPtr)obj, 8)) != module + ConsoleVtableRva) continue;
                    byte[] fields = pm.Read((IntPtr)(obj + 0x10), 12);
                    if (fields[0] > 1 || fields[1] > 1)
                        throw new InvalidOperationException("Console open fields are not boolean.");
                    float animation = BitConverter.ToSingle(fields, 8);
                    if (!float.IsFinite(animation)) throw new InvalidOperationException("Console animation is invalid.");
                    state = new ConsoleState(obj, fields[0] != 0, fields[1] != 0, animation);
                    return true;
                }
                error = "Console object not found in mode manager.";
                return false;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        public static ConsoleState RequireConsole(ProcessMemory pm)
        {
            if (!TryConsole(pm, out var state, out string error))
                throw new InvalidOperationException("读取控制台状态失败: " + error);
            return state;
        }

        public static ConsoleState WaitConsole(ProcessMemory pm, bool open, int timeoutMs = 2500)
        {
            var start = Environment.TickCount64;
            ConsoleState state;
            do
            {
                state = RequireConsole(pm);
                if (state.Open == open && (open ? state.Drawing : !state.Drawing)) return state;
                Thread.Sleep(50);
            } while (Environment.TickCount64 - start < timeoutMs);
            throw new InvalidOperationException($"控制台状态未达到 open={open}: {state}");
        }
    }
}
