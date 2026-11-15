using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal static class AlertTests
{
	private static void Check(bool ok, string msg)
	{
		if (!ok)
		{
			throw new Exception(msg);
		}
	}

	public static void Run(List<string> log)
	{
		PlayerInfoRow playerInfoRow = new PlayerInfoRow();
		playerInfoRow.Slot = 2u;
		playerInfoRow.Name = "测试敌方";
		playerInfoRow.Relation = "敌方";
		PlayerInfoRow playerInfoRow2 = playerInfoRow;
		PlayerInfoRow playerInfoRow3 = new PlayerInfoRow();
		playerInfoRow3.Slot = 3u;
		playerInfoRow3.Name = "自己";
		playerInfoRow3.Relation = "自己";
		PlayerInfoRow playerInfoRow4 = playerInfoRow3;
		List<PlayerInfoRow> list = new List<PlayerInfoRow>();
		list.Add(playerInfoRow2);
		list.Add(playerInfoRow4);
		List<PlayerInfoRow> rows = list;
		EnemySelection enemySelection = new EnemySelection();
		enemySelection.UpdateMatch("test");
		AlertTracker alertTracker = new AlertTracker();
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "initial baseline");
		playerInfoRow2.Upgrades.Add("upgrade\\german\\battlegroups\\luftwaffe\\luftwaffe");
		Check(alertTracker.Update("test", rows, enemySelection).Count((GameAlert x) => x.Kind == AlertKind.BattleGroup) == 1, "group selection alert");
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "group repeated");
		TrackedBuilding trackedBuilding = new TrackedBuilding();
		trackedBuilding.Tier = 2;
		trackedBuilding.Key = "one";
		trackedBuilding.Complete = false;
		TrackedBuilding trackedBuilding2 = trackedBuilding;
		playerInfoRow2.Buildings.Add(trackedBuilding2);
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "construction alert too early");
		trackedBuilding2.Complete = true;
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "construction debounce");
		Check(alertTracker.Update("test", rows, enemySelection).Count((GameAlert x) => x.Kind == AlertKind.BaseComplete) == 1, "construction completion");
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "construction repeated");
		enemySelection.Set(playerInfoRow2, enabled: false);
		playerInfoRow2.Buildings.Add(new TrackedBuilding
		{
			Tier = 3,
			Key = "two",
			Complete = true
		});
		alertTracker.Update("test", rows, enemySelection);
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "unselected enemy alerted");
		ArtilleryState artilleryState = new ArtilleryState();
		artilleryState.Key = "bishop";
		artilleryState.Name = "主教自行火炮";
		artilleryState.CooldownsKnown = true;
		ArtilleryState artilleryState2 = artilleryState;
		artilleryState2.Cooldowns[17u] = 360;
		playerInfoRow4.Artillery.Add(artilleryState2);
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "active cooldown alerted");
		artilleryState2.Cooldowns[17u] = 4;
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "countdown alerted");
		artilleryState2.Cooldowns.Clear();
		Check(alertTracker.Update("test", rows, enemySelection).Count((GameAlert x) => x.Kind == AlertKind.ArtilleryReady) == 1, "cooldown completion");
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "cooldown repeated");
		artilleryState2.Cooldowns[17u] = 100;
		alertTracker.Update("test", rows, enemySelection);
		playerInfoRow4.Artillery.Clear();
		Check(alertTracker.Update("test", rows, enemySelection).Count == 0, "destroyed unit alerted");
		Check(alertTracker.Update("new match", rows, enemySelection).Count == 0, "match reset alerted");
		using (AlertSounds alertSounds = new AlertSounds())
		{
			foreach (AlertKind value in Enum.GetValues(typeof(AlertKind)))
			{
				if (!string.IsNullOrEmpty(alertSounds.FileFor(value)))
				{
					using (new AudioClip(alertSounds.FileFor(value)))
					{
					}
				}
			}
		}
		log.Add("PASS alert baseline, battlegroup once, construction completion debounce, selected-enemy filtering, actual cooldown disappearance once, destroyed-unit suppression, match reset, three WAV files loaded.");
	}
}
