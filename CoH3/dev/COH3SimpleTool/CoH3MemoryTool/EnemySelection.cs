using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class EnemySelection
{
	private string match;

	private readonly HashSet<string> excluded = new HashSet<string>();

	public static string Key(PlayerInfoRow p)
	{
		return p.Slot + "|" + p.Name;
	}

	public void UpdateMatch(string identity)
	{
		if (match != identity)
		{
			excluded.Clear();
			match = identity;
		}
	}

	public bool Includes(PlayerInfoRow p)
	{
		if (p.Relation == "敌方")
		{
			return !excluded.Contains(Key(p));
		}
		return false;
	}

	public void Set(PlayerInfoRow p, bool enabled)
	{
		if (enabled)
		{
			excluded.Remove(Key(p));
		}
		else
		{
			excluded.Add(Key(p));
		}
	}

	public IEnumerable<PlayerInfoRow> Filter(IEnumerable<PlayerInfoRow> rows)
	{
		return rows.Where(Includes);
	}
}
