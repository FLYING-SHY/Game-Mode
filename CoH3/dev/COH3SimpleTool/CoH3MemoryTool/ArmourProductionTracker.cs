using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class ArmourProductionTracker
{
	private string match;

	private readonly HashSet<string> initialized = new HashSet<string>();

	private readonly HashSet<string> seen = new HashSet<string>();

	public void Reconnect()
	{
		initialized.Clear();
	}

	public List<GameAlert> Update(string key, IEnumerable<PlayerInfoRow> rows, EnemySelection enabled)
	{
		if (match != key)
		{
			match = key;
			initialized.Clear();
			seen.Clear();
		}
		List<GameAlert> list = new List<GameAlert>();
		foreach (PlayerInfoRow item2 in rows.Where((PlayerInfoRow p) => p.Relation == "敌方"))
		{
			string text = item2.Slot + "|" + item2.PlayerIdentity + "|" + item2.Name;
			if (!item2.ProductionKnown)
			{
				initialized.Remove(text);
				continue;
			}
			bool flag = initialized.Contains(text);
			foreach (ProductionJob item3 in item2.Producing)
			{
				if (!string.IsNullOrEmpty(item3.Key))
				{
					string item = text + "|" + item3.Key + "|" + item3.Path;
					if (seen.Add(item) && flag && enabled.Includes(item2) && ArmourUnits.Includes(item3.Path))
					{
						list.Add(new GameAlert
						{
							Kind = AlertKind.EnemyArmourProduction,
							Text = item2.Name + " 开始生产：" + UnitNames.Name(item3.Path)
						});
					}
				}
			}
			initialized.Add(text);
		}
		return list;
	}
}
