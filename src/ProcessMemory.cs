using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CoH3FowTool
{
    /// <summary>Thin wrapper over OpenProcess / ReadProcessMemory / VirtualProtectEx / WriteProcessMemory.</summary>
    internal sealed class ProcessMemory : IDisposable
    {
        const uint PROCESS_QUERY_INFORMATION = 0x0400;
        const uint PROCESS_VM_OPERATION = 0x0008;
        const uint PROCESS_VM_READ = 0x0010;
        const uint PROCESS_VM_WRITE = 0x0020;
        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_GUARD = 0x100;
        const uint PAGE_NOACCESS = 0x01;
        const uint PAGE_EXECUTE_READWRITE = 0x40;
        const uint PAGE_READWRITE = 0x04;
        const uint PAGE_EXECUTE_READ = 0x20;
        const uint PAGE_READONLY = 0x02;

        [StructLayout(LayoutKind.Sequential)]
        struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint access, bool inherit, int pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr h);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, IntPtr size, out IntPtr read);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr h, IntPtr addr, byte[] buf, IntPtr size, out IntPtr written);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr h, IntPtr addr, IntPtr size, uint newProtect, out uint oldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualQueryEx(IntPtr h, IntPtr addr, out MEMORY_BASIC_INFORMATION mbi, IntPtr len);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FlushInstructionCache(IntPtr h, IntPtr addr, IntPtr size);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr addr, IntPtr size, uint allocType, uint protect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualFree(IntPtr addr, IntPtr size, uint freeType);

        public IntPtr Handle { get; private set; }
        public Process Target { get; private set; }
        public IntPtr ModuleBase { get; private set; }
        public int ModuleSize { get; private set; }

        public static ProcessMemory Open(string processName)
        {
            var procs = Process.GetProcessesByName(processName.Replace(".exe", ""));
            if (procs.Length == 0)
                throw new InvalidOperationException("process '" + processName + "' is not running");

            var pm = new ProcessMemory { Target = procs[0] };
            pm.Handle = OpenProcess(
                PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE,
                false, pm.Target.Id);

            if (pm.Handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "OpenProcess failed (run as administrator)");

            var mod = pm.Target.MainModule;
            pm.ModuleBase = mod.BaseAddress;
            pm.ModuleSize = mod.ModuleMemorySize;
            return pm;
        }

        public byte[] Read(IntPtr addr, int size)
        {
            var buf = new byte[size];
            if (!ReadProcessMemory(Handle, addr, buf, (IntPtr)size, out var read) || read.ToInt64() != size)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "ReadProcessMemory failed at 0x" + addr.ToInt64().ToString("X"));
            return buf;
        }

        public void Write(IntPtr addr, byte[] bytes)
        {
            // Make the containing pages writable, write, restore protection.
            if (!VirtualProtectEx(Handle, addr, (IntPtr)bytes.Length, PAGE_EXECUTE_READWRITE, out uint old))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "VirtualProtectEx failed");

            try
            {
                if (!WriteProcessMemory(Handle, addr, bytes, (IntPtr)bytes.Length, out var written) ||
                    written.ToInt64() != bytes.Length)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "WriteProcessMemory failed");
                FlushInstructionCache(Handle, addr, (IntPtr)bytes.Length);
            }
            finally
            {
                VirtualProtectEx(Handle, addr, (IntPtr)bytes.Length, old, out _);
            }
        }

        /// <summary>Scan committed, readable pages of the main module for a pattern.</summary>
        public IntPtr FindPattern(Pattern pattern)
        {
            long end = ModuleBase.ToInt64() + ModuleSize;
            long addr = ModuleBase.ToInt64();

            while (addr < end)
            {
                if (VirtualQueryEx(Handle, (IntPtr)addr, out var mbi, (IntPtr)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) == IntPtr.Zero)
                    break;

                long regionBase = mbi.BaseAddress.ToInt64();
                long regionSize = mbi.RegionSize.ToInt64();
                bool usable = mbi.State == MEM_COMMIT
                              && (mbi.Protect & PAGE_GUARD) == 0
                              && mbi.Protect != PAGE_NOACCESS
                              && (mbi.Protect == PAGE_READONLY || mbi.Protect == PAGE_READWRITE
                                  || mbi.Protect == PAGE_EXECUTE_READ || mbi.Protect == PAGE_EXECUTE_READWRITE);

                if (usable && regionSize > 0 && regionSize < 256L * 1024 * 1024)
                {
                    try
                    {
                        var buf = Read((IntPtr)regionBase, (int)regionSize);
                        var hits = pattern.FindAll(buf);
                        if (hits.Count > 0)
                            return (IntPtr)(regionBase + hits[0]);
                    }
                    catch (Win32Exception)
                    {
                        // page disappeared mid-scan; skip it
                    }
                }

                addr = regionBase + Math.Max(regionSize, 0x1000);
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero) CloseHandle(Handle);
            Handle = IntPtr.Zero;
        }
    }
}