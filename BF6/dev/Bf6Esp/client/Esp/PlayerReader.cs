using System;
using System.Collections.Generic;

namespace Bf6Esp;

/// <summary>
/// 实体读取器：把实体列表翻译成一份 <see cref="EntitySnapshot"/> 列表。
///
/// Frostbite 4 的 ECS 布局还在逆向中，所以这里不写死任何布局：
/// 一切字段都通过 <see cref="IEntitySource"/> 取，取不到就跳过该实体并记日志，
/// 绝不猜测、绝不写入游戏内存。
/// </summary>
internal sealed class PlayerReader
{
	private readonly IEntitySource source;

	private readonly Action<string> log;

	private int skipped;

	private int lastCount;

	public PlayerReader(IEntitySource source, Action<string> log)
	{
		this.source = source;
		this.log = log ?? delegate(string message)
		{
		};
	}

	/// <summary>上一帧读到的实体数量。</summary>
	public int LastCount
	{
		get { return lastCount; }
	}

	/// <summary>累计跳过的实体数量（字段未知 / 读失败）。</summary>
	public int Skipped
	{
		get { return skipped; }
	}

	public List<EntitySnapshot> Read()
	{
		List<EntitySnapshot> result = new List<EntitySnapshot>();
		lastCount = 0;
		int count;
		if (!source.TryGetEntityCount(out count))
		{
			log("[读取] 实体数量未知（档案未定位 EntityCount），本帧跳过。来源：" + source.Name);
			return result;
		}
		if (count <= 0 || count > 8192)
		{
			log("[读取] 实体数量异常：" + count + "，本帧跳过。");
			return result;
		}
		lastCount = count;
		int localTeam;
		bool teamKnown = source.TryGetLocalTeam(out localTeam);
		ulong localAddress;
		bool localKnown = source.TryGetLocalPlayerAddress(out localAddress);
		float localX;
		float localY;
		float localZ;
		bool localPositionKnown = source.TryGetLocalPosition(out localX, out localY, out localZ);
		for (int i = 0; i < count; i++)
		{
			ulong address;
			if (!source.TryGetEntityAddress(i, out address) || address == 0UL)
			{
				skipped++;
				continue;
			}
			float x;
			float y;
			float z;
			if (!source.TryGetPosition(address, out x, out y, out z))
			{
				skipped++;
				continue;
			}
			EntitySnapshot snapshot = new EntitySnapshot
			{
				Address = address,
				X = x,
				Y = y,
				Z = z,
				Distance = localPositionKnown ? Distance(localX, localY, localZ, x, y, z) : 0f,
				TeamId = -1,
				Health = 0f,
				MaxHealth = 0f,
				Name = "",
				IsLocalPlayer = localKnown && address == localAddress,
				IsEnemy = false
			};
			int teamId;
			if (source.TryGetTeam(address, out teamId))
			{
				snapshot.TeamId = teamId;
			}
			float current;
			float max;
			if (source.TryGetHealth(address, out current, out max))
			{
				snapshot.Health = current;
				snapshot.MaxHealth = max;
			}
			string name;
			if (source.TryGetName(address, out name))
			{
				snapshot.Name = name;
			}
			snapshot.IsEnemy = teamKnown && snapshot.TeamId >= 0 && snapshot.TeamId != localTeam;
			result.Add(snapshot);
		}
		return result;
	}

	private static float Distance(float ax, float ay, float az, float bx, float by, float bz)
	{
		float dx = ax - bx;
		float dy = ay - by;
		float dz = az - bz;
		return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
	}
}
