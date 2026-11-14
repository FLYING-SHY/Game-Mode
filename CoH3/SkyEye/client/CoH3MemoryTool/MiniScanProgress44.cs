using System;

namespace CoH3MemoryTool;

internal sealed class MiniScanProgress44
{
	private bool wrapped;

	public bool Initialized { get; private set; }

	public long Cursor { get; private set; }

	public long Origin { get; private set; }

	public void Reset()
	{
		Initialized = false;
		long cursor = (Origin = 65536L);
		Cursor = cursor;
		wrapped = false;
	}

	public void Begin(long start)
	{
		long origin = (Cursor = Math.Max(65536L, start));
		Origin = origin;
		Initialized = true;
		wrapped = false;
	}

	public bool Advance(long next)
	{
		Cursor = Math.Max(65536L, next);
		if (wrapped && Cursor >= Origin)
		{
			Cursor = Origin;
			wrapped = false;
			return true;
		}
		return false;
	}

	public bool Wrap()
	{
		Cursor = 65536L;
		if (Origin == 65536)
		{
			wrapped = false;
			return true;
		}
		wrapped = true;
		return false;
	}
}
