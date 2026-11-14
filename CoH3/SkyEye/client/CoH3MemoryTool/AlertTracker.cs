using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class AlertTracker
{
	private readonly HashSet<string> enemyBarrages = new HashSet<string>();

	private readonly Dictionary<string, uint> barrageOwners = new Dictionary<string, uint>();

	private readonly HashSet<uint> grenades = new HashSet<uint>();

	private string match;

	private bool initialized;

	private readonly Dictionary<uint, string> groups = new Dictionary<uint, string>();

	private readonly HashSet<string> completed = new HashSet<string>();

	private readonly Dictionary<string, int> confirmations = new Dictionary<string, int>();

	private readonly Dictionary<string, Dictionary<uint, int>> artillery = new Dictionary<string, Dictionary<uint, int>>();

	public void Reset()
	{
		initialized = false;
		enemyBarrages.Clear();
		barrageOwners.Clear();
		grenades.Clear();
		groups.Clear();
		completed.Clear();
		confirmations.Clear();
		artillery.Clear();
	}

	public List<GameAlert> Update(string key, List<PlayerInfoRow> rows, EnemySelection selection)
	{
		if (match != key)
		{
			Reset();
			match = key;
		}
		List<GameAlert> list = new List<GameAlert>();
		HashSet<string> currentBuildingKeys = new HashSet<string>();
		HashSet<string> currentArtillery = new HashSet<string>();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (PlayerInfoRow p in rows)
		{
			if (p.Relation == "敌方")
			{
				if (!p.BarragesKnown)
				{
					foreach (KeyValuePair<string, uint> item in barrageOwners.Where((KeyValuePair<string, uint> x) => x.Value == p.Slot))
					{
						hashSet.Add(item.Key);
					}
				}
				foreach (BarrageRecord barrage in p.Barrages)
				{
					string text = barrage.CastKey ?? barrage.Key;
					barrageOwners[text] = p.Slot;
					hashSet.Add(text);
					if (enemyBarrages.Add(text) && initialized && selection.Includes(p))
					{
						list.Add(new GameAlert
						{
							Kind = AlertKind.EnemyArtilleryBarrage,
							Text = p.Name + " " + barrage.Name + " 发射弹幕"
						});
					}
				}
				if (GrenadeUpgrade.Completed(p) && grenades.Add(p.Slot) && initialized && selection.Includes(p))
				{
					list.Add(new GameAlert
					{
						Kind = AlertKind.GrenadeReady,
						Text = p.Name + " 手雷升级完成（有雷）"
					});
				}
				string text2 = ChineseNames.GroupKey(p);
				string value;
				bool flag = groups.TryGetValue(p.Slot, out value);
				if (initialized && flag && text2.Length > 0 && text2 != value && selection.Includes(p))
				{
					list.Add(new GameAlert
					{
						Kind = AlertKind.BattleGroup,
						Text = p.Name + " 选择了 " + ChineseNames.Group(p)
					});
				}
				groups[p.Slot] = text2;
				foreach (TrackedBuilding building in p.Buildings)
				{
					string text3 = p.Slot + ":" + building.Key;
					currentBuildingKeys.Add(text3);
					if (!building.Complete)
					{
						confirmations.Remove(text3);
						continue;
					}
					confirmations.TryGetValue(text3, out var value2);
					confirmations[text3] = value2 + 1;
					if (!initialized)
					{
						completed.Add(text3);
					}
					else if (value2 + 1 >= 2 && completed.Add(text3) && selection.Includes(p))
					{
						list.Add(new GameAlert
						{
							Kind = AlertKind.BaseComplete,
							Text = p.Name + " " + BuildingNames.TierName(building.Tier) + "已建成"
						});
					}
				}
			}
			if (!(p.Relation == "自己"))
			{
				continue;
			}
			foreach (ArtilleryState item2 in p.Artillery)
			{
				currentArtillery.Add(item2.Key);
				if (!item2.CooldownsKnown)
				{
					artillery.Remove(item2.Key);
					continue;
				}
				if (initialized && artillery.TryGetValue(item2.Key, out var value3))
				{
					foreach (KeyValuePair<uint, int> item3 in value3)
					{
						if (item3.Value > 0 && (!item2.Cooldowns.TryGetValue(item3.Key, out var value4) || value4 <= 0))
						{
							list.Add(new GameAlert
							{
								Kind = AlertKind.ArtilleryReady,
								Text = item2.ReadyMessage
							});
						}
					}
				}
				artillery[item2.Key] = new Dictionary<uint, int>(item2.Cooldowns);
			}
		}
		string[] array = confirmations.Keys.Where((string x) => !currentBuildingKeys.Contains(x)).ToArray();
		foreach (string key2 in array)
		{
			confirmations.Remove(key2);
		}
		string[] array2 = artillery.Keys.Where((string x) => !currentArtillery.Contains(x)).ToArray();
		foreach (string key3 in array2)
		{
			artillery.Remove(key3);
		}
		enemyBarrages.IntersectWith(hashSet);
		string[] array3 = barrageOwners.Keys.Where((string x) => !enemyBarrages.Contains(x)).ToArray();
		foreach (string key4 in array3)
		{
			barrageOwners.Remove(key4);
		}
		initialized = true;
		return list;
	}
}
