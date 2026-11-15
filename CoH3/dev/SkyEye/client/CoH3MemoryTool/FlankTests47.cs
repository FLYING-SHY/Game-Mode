using System;

namespace CoH3MemoryTool;

internal static class FlankTests47
{
	public static void Run()
	{
		PlayerInfoRow[] players = new PlayerInfoRow[4]
		{
			new PlayerInfoRow
			{
				Slot = 1u,
				Relation = "自己"
			},
			new PlayerInfoRow
			{
				Slot = 2u,
				Relation = "队友"
			},
			new PlayerInfoRow
			{
				Slot = 3u,
				Relation = "敌方"
			},
			new PlayerInfoRow
			{
				Slot = 4u,
				Relation = "敌方"
			}
		};
		SquadMarker[] array = new SquadMarker[4]
		{
			new SquadMarker
			{
				Slot = 1u
			},
			new SquadMarker
			{
				Slot = 3u
			},
			new SquadMarker
			{
				Slot = 3u
			},
			new SquadMarker
			{
				Slot = 4u,
				X = 100f,
				Movement = new MovementData
				{
					X = 0f,
					Z = 0f
				}
			}
		};
		FlankResult47 flankResult = FlankLogic47.Count(array, players, 0f, 0f, 40.0);
		if (flankResult.Friends != 1 || flankResult.Enemies != 1 || flankResult.IncomingEnemies != 2 || !flankResult.Danger)
		{
			throw new Exception("Flank unique-player or goal detection failed");
		}
		FlankResult47 flankResult2 = FlankLogic47.Count(new SquadMarker[3]
		{
			array[0],
			array[1],
			new SquadMarker
			{
				Slot = 2u
			}
		}, players, 0f, 0f, 40.0);
		if (flankResult2.Danger)
		{
			throw new Exception("Friendly support ignored");
		}
		MineCamera mineCamera = new MineCamera();
		mineCamera.View = new float[16]
		{
			1f, 0f, 0f, 0f, 0f, 0.5f, 0.866f, 0f, 0f, -0.866f,
			0.5f, 0f, 0f, -5f, 100f, 1f
		};
		mineCamera.Projection = new float[16]
		{
			1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f,
			1f, 1f, 0f, 0f, 0f, 0f
		};
		MineCamera mineCamera2 = mineCamera;
		if (!mineCamera2.Project(2f, 0f, 3f, 1920, 1080, out var point) || !FlankLogic47.Ground(mineCamera2, point, 1920, 1080, 0f, out var x, out var z) || (double)Math.Abs(x - 2f) > 0.001 || (double)Math.Abs(z - 3f) > 0.001)
		{
			throw new Exception("Point projection roundtrip failed");
		}
	}
}
