using System;
using System.Runtime.InteropServices;

namespace Bf6Esp;

/// <summary>
/// 进程内存相关的 P/Invoke 声明。
/// 这里只声明只读所需的最小 API 集合：打开只读句柄、读取进程内存、枚举模块。
/// 刻意不声明 WriteProcessMemory / VirtualAllocEx / CreateRemoteThread 等写入型 API，
/// 因为本工具的设计前提是"外部只读"，见 docs/ARCHITECTURE.md。
/// </summary>
internal static class NativeMemory
{
	/// <summary>PROCESS_VM_READ：只读内存。</summary>
	internal const uint ProcessVmRead = 0x0010;

	/// <summary>PROCESS_QUERY_LIMITED_INFORMATION：查询进程基本信息所需的最少权限。</summary>
	internal const uint ProcessQueryLimitedInformation = 0x1000;

	internal const uint Th32csSnapModule = 0x00000008;

	internal const uint Th32csSnapModule32 = 0x00000010;

	internal const uint InvalidHandleValue = 0xFFFFFFFF;

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct ModuleEntry32
	{
		public uint Size;

		public uint ModuleId;

		public uint ProcessId;

		public uint GlobalUsage;

		public uint ProcessUsage;

		public IntPtr BaseAddress;

		public uint BaseSize;

		public IntPtr ModuleHandle;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string ModuleName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string ExePath;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool Module32First(IntPtr snapshot, ref ModuleEntry32 entry);

	[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool Module32Next(IntPtr snapshot, ref ModuleEntry32 entry);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool CloseHandle(IntPtr handle);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool ReadProcessMemory(IntPtr handle, IntPtr address, [Out] byte[] buffer, UIntPtr size, out UIntPtr bytesRead);

	/// <summary>MEMORY_BASIC_INFORMATION（64 位布局）。</summary>
	[StructLayout(LayoutKind.Sequential)]
	internal struct MemoryBasicInformation
	{
		public IntPtr BaseAddress;

		public IntPtr AllocationBase;

		public uint AllocationProtect;

		/// <summary>SIZE_T RegionSize。</summary>
		public IntPtr RegionSize;

		public uint State;

		public uint Protect;

		public uint Type;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern IntPtr VirtualQueryEx(IntPtr handle, IntPtr address, out MemoryBasicInformation buffer, IntPtr length);
}
