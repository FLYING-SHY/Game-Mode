using System;

namespace CoH3MemoryTool;

internal static class TacticalMapState51
{
	private sealed class State
	{
		public int Pid;

		public string Match;

		public bool Shown;

		public DateTime Updated;
	}

	private static volatile State current;

	public static void Publish(int pid, string match, bool shown)
	{
		State state = new State();
		state.Pid = pid;
		state.Match = match;
		state.Shown = shown;
		state.Updated = DateTime.UtcNow;
		current = state;
	}

	public static bool Open(int pid, string match)
	{
		State state = current;
		if (state != null && state.Pid == pid && state.Match == match && state.Shown)
		{
			return DateTime.UtcNow - state.Updated < TimeSpan.FromSeconds(2.0);
		}
		return false;
	}
}
