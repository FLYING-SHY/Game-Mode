using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CoH3FowTool
{
    internal readonly record struct DisplayState(long Manager, long Display, bool Terrain,
        bool Entities, bool ManagerFlag, bool RenderFlag)
    {
        public bool Enabled => Terrain && Entities && ManagerFlag && RenderFlag;
        public bool Disabled => !Terrain && !Entities && !ManagerFlag && !RenderFlag;
        public override string ToString() => $"terrain={Terrain}, entities={Entities}, render={RenderFlag}, mirror={ManagerFlag}, display=0x{Display:X}";
    }

    internal static class FowDisplay
    {
        const long UiFlagsRva = 0x06A888A0;
        const long ManagerSlotRva = 0x06B02CC0;
        const long RootSlotRva = 0x06185F18;

        static long Pointer(ProcessMemory pm, long address)
        {
            long value = BitConverter.ToInt64(pm.Read((IntPtr)address, 8));
            if (value < 0x10000 || value >= 0x0000800000000000 || (value & 7) != 0)
                throw new InvalidOperationException($"显示对象尚未就绪: [0x{address:X}]=0x{value:X}");
            return value;
        }

        public static DisplayState Read(ProcessMemory pm)
        {
            long module = pm.ModuleBase.ToInt64();
            long manager = Pointer(pm, module + ManagerSlotRva);
            long root = Pointer(pm, module + RootSlotRva);
            long display = Pointer(pm, root + 0x50);
            byte[] managerData = pm.Read((IntPtr)manager, 0x48);
            byte[] displayData = pm.Read((IntPtr)display, 0x648);
            byte[] flags = pm.Read((IntPtr)(module + UiFlagsRva), 2);
            if (flags.Any(x => x > 1) || managerData[0x40] > 1 || managerData[0x41] > 1 ||
                displayData[0x5E8] > 1 || displayData[0x5E9] > 1)
                throw new InvalidOperationException("显示数据块的布尔字段未匹配。");
            if (Pointer(pm, module + ManagerSlotRva) != manager ||
                Pointer(pm, module + RootSlotRva) != root || Pointer(pm, root + 0x50) != display)
                throw new InvalidOperationException("对局正在切换，显示指针已变化。");
            return new DisplayState(manager, display, flags[0] != 0, flags[1] != 0,
                managerData[0x41] != 0, displayData[0x5E9] != 0);
        }

        public static List<PatchSite> Sites(PeImage pe)
        {
            var sites = PatchCatalog.Build();
            foreach (var site in sites)
            {
                site.Locate(pe);
                if (!site.IsUsable) throw new InvalidOperationException("显示补丁未匹配: " + site.Name);
            }
            return sites;
        }

        // Validate the entire batch before writing the first instruction.
        public static void CheckCode(ProcessMemory pm, PeImage pe, IEnumerable<PatchSite> sites)
        {
            foreach (var site in sites)
            {
                var current = pm.Read((IntPtr)(pm.ModuleBase.ToInt64() + site.Rva), site.TargetLength);
                if (!current.SequenceEqual(site.ExpectedOriginalBytes()) && !current.SequenceEqual(site.DesiredBytes(pe.Data)))
                    throw new InvalidOperationException("现场字节未匹配，已停止: " + site.Name);
            }
        }

        public static void SetCode(ProcessMemory pm, PeImage pe, List<PatchSite> sites, bool enabled)
        {
            CheckCode(pm, pe, sites);
            var changed = new List<(IntPtr Address, byte[] Before)>();
            try
            {
                foreach (var site in sites)
                {
                    var address = (IntPtr)(pm.ModuleBase.ToInt64() + site.Rva);
                    var before = pm.Read(address, site.TargetLength);
                    var desired = enabled ? site.DesiredBytes(pe.Data) : site.ExpectedOriginalBytes();
                    if (before.SequenceEqual(desired)) continue;
                    changed.Add((address, before));
                    pm.Write(address, desired);
                    if (!pm.Read(address, desired.Length).SequenceEqual(desired))
                        throw new InvalidOperationException("写入回读未匹配: " + site.Name);
                }
            }
            catch (Exception failure)
            {
                var errors = new List<Exception> { failure };
                for (int i = changed.Count - 1; i >= 0; --i)
                {
                    try { pm.Write(changed[i].Address, changed[i].Before); }
                    catch (Exception rollback) { errors.Add(rollback); }
                }
                throw new AggregateException("显示补丁操作失败，已尝试恢复本次改动。", errors);
            }
        }

        public static void SetFlags(ProcessMemory pm, bool enabled)
        {
            // The engine normally sends this value through its render queue.
            // Update both cached booleans as well so a paused scene refreshes.
            var state = Read(pm);
            long module = pm.ModuleBase.ToInt64();
            byte value = enabled ? (byte)1 : (byte)0;
            var addresses = new[] { module + UiFlagsRva, module + UiFlagsRva + 1,
                state.Manager + 0x41, state.Display + 0x5E9 };
            try
            {
                for (int i = 0; i < addresses.Length; i++)
                {
                    var current = Read(pm);
                    if (current.Manager != state.Manager || current.Display != state.Display)
                        throw new InvalidOperationException("对局正在切换，显示对象已变化。");
                    pm.Write((IntPtr)addresses[i], new[] { value });
                }
            }
            catch (Exception failure)
            {
                try { RestoreFlags(pm, state); }
                catch (Exception rollback)
                {
                    throw new AggregateException("显示字段操作失败，恢复状态也未完成。", failure, rollback);
                }
                throw;
            }
        }

        public static void ClearUiFlags(ProcessMemory pm)
            => pm.Write((IntPtr)(pm.ModuleBase.ToInt64() + UiFlagsRva), new byte[] { 0, 0 });

        public static void RestoreFlags(ProcessMemory pm, DisplayState before)
        {
            var failures = new List<Exception>();
            // Fixed module fields can be restored even if the match changed.
            try
            {
                pm.Write((IntPtr)(pm.ModuleBase.ToInt64() + UiFlagsRva),
                    new[] { before.Terrain ? (byte)1 : (byte)0, before.Entities ? (byte)1 : (byte)0 });
            }
            catch (Exception ex) { failures.Add(ex); }
            try
            {
                var current = Read(pm);
                if (current.Manager != before.Manager || current.Display != before.Display)
                    throw new InvalidOperationException("显示对象已切换，旧动态字段恢复已跳过。");
                pm.Write((IntPtr)(before.Manager + 0x41), new[] { before.ManagerFlag ? (byte)1 : (byte)0 });
                pm.Write((IntPtr)(before.Display + 0x5E9), new[] { before.RenderFlag ? (byte)1 : (byte)0 });
            }
            catch (Exception ex) { failures.Add(ex); }
            if (failures.Count > 0) throw new AggregateException("显示字段恢复未全部完成。", failures);
        }

        public static List<(PatchSite Site, byte[] Bytes)> SnapshotCode(ProcessMemory pm, List<PatchSite> sites)
            => sites.Select(site => (site, pm.Read((IntPtr)(pm.ModuleBase.ToInt64() + site.Rva), site.TargetLength))).ToList();

        public static void RestoreCode(ProcessMemory pm, PeImage pe, List<(PatchSite Site, byte[] Bytes)> snapshot)
        {
            var failures = new List<Exception>();
            foreach (var item in snapshot)
            {
                try
                {
                    CheckCode(pm, pe, new[] { item.Site });
                    var address = (IntPtr)(pm.ModuleBase.ToInt64() + item.Site.Rva);
                    pm.Write(address, item.Bytes);
                    if (!pm.Read(address, item.Bytes.Length).SequenceEqual(item.Bytes))
                        throw new InvalidOperationException("代码恢复回读未匹配: " + item.Site.Name);
                }
                catch (Exception ex) { failures.Add(ex); }
            }
            if (failures.Count > 0) throw new AggregateException("代码恢复未全部完成。", failures);
        }

        public static DisplayState Wait(ProcessMemory pm, bool enabled, int milliseconds = 2000)
        {
            var start = Environment.TickCount64;
            DisplayState state;
            do
            {
                state = Read(pm);
                if (enabled ? state.Enabled : state.Disabled)
                {
                    // Check again after normal frames had a chance to overwrite.
                    Thread.Sleep(150);
                    state = Read(pm);
                    if (enabled ? state.Enabled : state.Disabled) return state;
                }
                Thread.Sleep(50);
            } while (Environment.TickCount64 - start < milliseconds);
            throw new InvalidOperationException("显示状态回读未达到预期: " + state);
        }
    }
}
