using System;
using System.Runtime.InteropServices;

namespace CoH3MemoryTool;

internal static class Native
{
	public struct Region
	{
		public IntPtr BaseAddress;

		public IntPtr AllocationBase;

		public uint AllocationProtect;

		public UIntPtr RegionSize;

		public uint State;

		public uint Protect;

		public uint Type;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern ProcHandle OpenProcess(uint access, bool inherit, int pid);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool CloseHandle(IntPtr h);

	[DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
	private static extern bool ReadProcessMemoryRaw60(ProcHandle h, IntPtr address, byte[] buffer, UIntPtr length, out UIntPtr read);

	public static bool ReadProcessMemory(ProcHandle h, IntPtr address, byte[] buffer, UIntPtr length, out UIntPtr read)
	{
		if (!LicenseGate60.Allowed)
		{
			read = UIntPtr.Zero;
			return false;
		}
		return ReadProcessMemoryRaw60(h, address, buffer, length, out read);
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern UIntPtr VirtualQueryEx(ProcHandle h, IntPtr address, out Region region, UIntPtr size);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern bool RegisterHotKey(IntPtr h, int id, uint modifiers, uint key);

	[DllImport("user32.dll")]
	public static extern bool UnregisterHotKey(IntPtr h, int id);

	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
}
