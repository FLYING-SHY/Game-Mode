using System;

namespace CoH3MemoryTool;

internal sealed class GameIdentity60
{
	public int Pid;

	public DateTime Started;

	public string SteamId;

	public string Name;

	public string Error;

	public string Key => Pid + ":" + Started.Ticks + ":" + SteamId;
}
