using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class ProjectileSequenceTracker
{
	public sealed class State
	{
		public ulong Source;

		public string Cast;

		public bool Active;

		public int Count;

		public double LastShot;

		public double NextCheck;
	}

	private readonly Dictionary<ulong, State> sources = new Dictionary<ulong, State>();

	public IEnumerable<State> States => sources.Values.ToArray();

	public State Get(ulong source)
	{
		if (!sources.TryGetValue(source, out var value))
		{
			value = (sources[source] = new State
			{
				Source = source
			});
		}
		return value;
	}

	public void Observe(ulong source, string cast)
	{
		State state = Get(source);
		if (!string.IsNullOrEmpty(cast))
		{
			if (!state.Active || state.Cast != cast)
			{
				state.Count = 0;
			}
			state.Active = true;
			state.Cast = cast;
		}
		else
		{
			state.Active = false;
		}
	}

	public int Next(ulong source, double now)
	{
		State state = Get(source);
		if (!state.Active && now - state.LastShot > 12000.0)
		{
			state.Count = 0;
		}
		state.LastShot = now;
		return ++state.Count;
	}

	public void Trim(double now)
	{
		ulong[] array = (from p in sources
			where !p.Value.Active && now - p.Value.LastShot > 120000.0
			select p.Key).ToArray();
		foreach (ulong key in array)
		{
			sources.Remove(key);
		}
	}

	public void Clear()
	{
		sources.Clear();
	}
}
