using System.Runtime.InteropServices;

namespace CoH3Minimap;

/// <summary>
/// 只读游戏内存访问器。绝不写入，绝不注入。
/// </summary>
public sealed class GameReader : IDisposable
{
    private const uint PROCESS_VM_READ = 0x0010;
    private const uint PROCESS_QUERY_INFORMATION = 0x0400;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    private static extern int GetLastError();

    [DllImport("kernel32.dll")]
    private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll")]
    private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

    [DllImport("kernel32.dll")]
    private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct MODULEENTRY32
    {
        public uint dwSize;
        public uint th32ModuleID;
        public uint th32ProcessID;
        public uint GlblcntUsage;
        public uint ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string szModule;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szExePath;
    }

    [DllImport("kernel32.dll")]
    private static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    private int _disposed;
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);
    public bool IsAlive => _disposed == 0 && GetExitCodeProcess(ProcessHandle, out uint code) && code == 259;
    public IntPtr ProcessHandle { get; }
    public int ProcessId { get; }
    public IntPtr ModuleBase { get; }
    public string? ModulePath { get; }

    private GameReader(IntPtr handle, int pid, IntPtr baseAddr, string? modulePath)
    {
        ProcessHandle = handle;
        ProcessId = pid;
        ModuleBase = baseAddr;
        ModulePath = modulePath;
    }

    /// <summary>附加到正在运行的 RelicCoH3.exe（兼容入口，内部走完整版 AttachFull）。</summary>
    public static GameReader? Attach() => AttachFull();

    /// <summary>读取指定地址的原始字节。失败返回 null。</summary>
    public byte[]? ReadBytes(IntPtr address, int size)
    {
        var buffer = new byte[size];
        if (ReadProcessMemory(ProcessHandle, address, buffer, (IntPtr)size, out var read) && read.ToInt64() == size)
            return buffer;
        return null;
    }

    /// <summary>读取 u64。失败返回 null。</summary>
    public ulong? ReadU64(IntPtr address)
    {
        var bytes = ReadBytes(address, 8);
        return bytes is null ? null : BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>读取 u32。失败返回 null。</summary>
    public uint? ReadU32(IntPtr address)
    {
        var bytes = ReadBytes(address, 4);
        return bytes is null ? null : BitConverter.ToUInt32(bytes, 0);
    }

    /// <summary>读取 u16。失败返回 null。</summary>
    public ushort? ReadU16(IntPtr address)
    {
        var bytes = ReadBytes(address, 2);
        return bytes is null ? null : BitConverter.ToUInt16(bytes, 0);
    }

    /// <summary>读取 u8。失败返回 null。</summary>
    public byte? ReadU8(IntPtr address)
    {
        var bytes = ReadBytes(address, 1);
        return bytes is null ? null : bytes[0];
    }

    /// <summary>读取 f32。失败返回 null。</summary>
    public float? ReadF32(IntPtr address)
    {
        var bytes = ReadBytes(address, 4);
        return bytes is null ? null : BitConverter.ToSingle(bytes, 0);
    }

    /// <summary>读取指针并做合理性校验。</summary>
    public IntPtr? ReadPointer(IntPtr address)
    {
        var val = ReadU64(address);
        if (val is null) return null;
        var ptr = new IntPtr((long)val.Value);
        if (!IsValidPointer(ptr)) return null;
        return ptr;
    }

    /// <summary>读取模块基址+偏移处的指针。</summary>
    public IntPtr? ReadPointerAt(uint rva)
    {
        return ReadPointer(new IntPtr(ModuleBase.ToInt64() + rva));
    }

    /// <summary>读取模块基址+偏移处的字节。</summary>
    public byte[]? ReadBytesAt(uint rva, int size)
    {
        return ReadBytes(new IntPtr(ModuleBase.ToInt64() + rva), size);
    }

    public static bool IsValidPointer(IntPtr ptr)
    {
        long v = ptr.ToInt64();
        return v >= 0x10000 && v < 0x800000000000 && (v & 7) == 0;
    }

    /// <summary>
    /// 枚举进程的已提交私有可读内存区域。返回 (基址, 大小, 保护属性, 类型)。
    /// </summary>
    public List<(long Base, int Size, uint Protect, uint Type)> EnumerateMemoryRegions()
    {
        var result = new List<(long, int, uint, uint)>();
        long addr = 0;
        const uint MEM_COMMIT = 0x1000;

        while (addr < 0x7FFFFFFFFFFF)
        {
            if (!VirtualQueryEx(ProcessHandle, new IntPtr(addr), out var mbi, (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()))
                break;

            long regionBase = mbi.BaseAddress.ToInt64();
            long regionSize = mbi.RegionSize.ToInt64();
            if (regionSize <= 0) break;

            if (mbi.State == MEM_COMMIT)
            {
                result.Add((regionBase, (int)regionSize, mbi.Protect, mbi.Type));
            }

            addr = regionBase + regionSize;
        }

        return result;
    }

    /// <summary>
    /// 附加到正在运行的 RelicCoH3.exe（使用 Toolhelp32 获取模块，兼容无 Modules 权限场景）。
    /// </summary>
    public static GameReader? AttachFull()
    {
        var proc = System.Diagnostics.Process.GetProcessesByName("RelicCoH3").FirstOrDefault();
        if (proc is null) return null;

        var handle = OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, proc.Id);
        if (handle == IntPtr.Zero) return null;

        // 优先 Toolhelp32（稳定），失败退回 Process.Modules
        IntPtr baseAddr = IntPtr.Zero;
        string? modulePath = null;

        const uint TH32CS_SNAPMODULE = 0x8;
        const uint TH32CS_SNAPMODULE32 = 0x10;
        var snap = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)proc.Id);
        if (snap != IntPtr.Zero && snap != new IntPtr(-1))
        {
            var me = new MODULEENTRY32 { dwSize = (uint)Marshal.SizeOf<MODULEENTRY32>() };
            if (Module32First(snap, ref me))
            {
                do
                {
                    if (me.szModule.Equals("RelicCoH3.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        baseAddr = me.modBaseAddr;
                        modulePath = me.szExePath;
                        break;
                    }
                } while (Module32Next(snap, ref me));
            }
            CloseHandle(snap);
        }

        if (baseAddr == IntPtr.Zero)
        {
            foreach (System.Diagnostics.ProcessModule mod in proc.Modules)
            {
                if (mod.ModuleName.Equals("RelicCoH3.exe", StringComparison.OrdinalIgnoreCase))
                {
                    baseAddr = mod.BaseAddress;
                    modulePath = mod.FileName;
                    break;
                }
            }
        }

        if (baseAddr == IntPtr.Zero)
        {
            CloseHandle(handle);
            return null;
        }

        return new GameReader(handle, proc.Id, baseAddr, modulePath);
    }

    public void Dispose()
    {
        if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 0 && ProcessHandle != IntPtr.Zero)
            CloseHandle(ProcessHandle);
    }
}
