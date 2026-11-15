using Microsoft.Win32.SafeHandles;

namespace CoH3MemoryTool;

internal sealed class ProcHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	public ProcHandle()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		return Native.CloseHandle(handle);
	}
}
