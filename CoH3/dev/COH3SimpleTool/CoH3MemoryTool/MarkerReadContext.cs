using System;

namespace CoH3MemoryTool;

internal sealed class MarkerReadContext
{
	public int Pid;

	public long Image;

	public long Arena;

	public uint WorldOffset;

	public DateTime Started;

	public string Match;
}
