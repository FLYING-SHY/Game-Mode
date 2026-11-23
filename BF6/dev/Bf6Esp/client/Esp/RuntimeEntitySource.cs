using System;
using System.Collections.Generic;

namespace Bf6Esp;

/// <summary>
/// 运行时实体来源：优先使用定位器在目标进程里现找的布局，
/// 找不到的项回落到档案（OffsetProfile）里烘焙好的偏移。
/// </summary>
internal sealed class RuntimeEntitySource : IEntitySource
{
	private RuntimeLayout60 layout;

	private readonly IEntitySource fallback;

	private readonly MemoryReader reader;

	private readonly Action<string> log;

	public RuntimeEntitySource(MemoryReader reader, RuntimeLayout60 layout, IEntitySource fallback, Action<string> log)
	{
		this.reader = reader;
		this.layout = layout;
		this.fallback = fallback;
		this.log = log ?? delegate(string message)
		{
		};
	}

	/// <summary>定位器跑完后把新布局灌进来（覆盖层下一帧即生效）。</summary>
	public void ApplyLayout(RuntimeLayout60 located)
	{
		if (located == null)
		{
			return;
		}
		layout = located;
	}

	public string Name
	{
		get { return "运行时"; }
	}

	// ---------------- 基础取值 ----------------

	private bool TryReadFloat(ulong address, out float value)
	{
		value = 0f;
		return reader != null && reader.TryRead(address, out value);
	}

	private bool TryReadInt(ulong address, out int value)
	{
		value = 0;
		return reader != null && reader.TryRead(address, out value);
	}

	/// <summary>取字段偏移：先运行时反射，再档案结构体。</summary>
	private static readonly string[] PositionXAliases = { "positionX" };

	private static readonly string[] PositionYAliases = { "positionY" };

	private static readonly string[] PositionZAliases = { "positionZ" };

	private static readonly string[] PositionVectorAliases = { "Position", "worldPosition", "transformPosition" };

	private static readonly string[] HealthAliases = { "CurrentHealth", "health" };

	private static readonly string[] MaxHealthAliases = { "MaxHealth", "maxHealth" };

	private static readonly string[] TeamAliases = { "CurrentTeamId", "TeamId", "teamId", "OwningTeam" };

	private static readonly string[] NameAliases = { "PlayerNamePointer", "NamePointer", "playerName" };

	/// <summary>
	/// 解析字段的基址与偏移。
	/// 字段可能直接内联在实体里，也可能住在组件对象后面（实体里某个槽存着组件指针）。
	/// </summary>
	private bool ResolveField(ulong address, string[] aliases, out ulong baseAddress, out long offset)
	{
		baseAddress = address;
		offset = -1L;
		if (layout == null)
		{
			return false;
		}
		long inner = layout.FindOffset(aliases);
		if (inner < 0L)
		{
			return false;
		}
		long slot = layout.FindPointerOffset(aliases);
		if (slot < 0L)
		{
			offset = inner;
			return true;
		}
		ulong target;
		if (!TryReadU64(address + (ulong)slot, out target) || target == 0UL)
		{
			return false;
		}
		baseAddress = target;
		offset = inner;
		return true;
	}

	// ---------------- IEntitySource ----------------

	public bool TryGetViewProjection(out float[] matrix)
	{
		matrix = null;
		if (layout != null && layout.HasMatrix)
		{
			float[] values = new float[16];
			bool ok = true;
			for (int i = 0; i < values.Length; i++)
			{
				if (!TryReadFloat(layout.MatrixAddress + (ulong)(i * 4), out values[i]))
				{
					ok = false;
					break;
				}
			}
			if (ok)
			{
				matrix = values;
				WorldToScreen.UseColumnVector = layout.UseColumnVector;
				return true;
			}
		}
		return fallback != null && fallback.TryGetViewProjection(out matrix);
	}

	public bool TryGetLocalTeam(out int teamId)
	{
		teamId = -1;
		if (layout != null && layout.LocalTeamIdAddress != 0UL)
		{
			if (TryReadInt(layout.LocalTeamIdAddress, out teamId))
			{
				return true;
			}
		}
		if (layout != null && layout.LocalTeamId >= 0)
		{
			teamId = layout.LocalTeamId;
			return true;
		}
		return fallback != null && fallback.TryGetLocalTeam(out teamId);
	}

	public bool TryGetLocalPlayerAddress(out ulong address)
	{
		address = 0UL;
		if (layout != null && layout.LocalPlayerAddress != 0UL)
		{
			address = layout.LocalPlayerAddress;
			return true;
		}
		return fallback != null && fallback.TryGetLocalPlayerAddress(out address);
	}

	public bool TryGetLocalPosition(out float x, out float y, out float z)
	{
		x = 0f;
		y = 0f;
		z = 0f;
		ulong address;
		if (TryGetLocalPlayerAddress(out address) && TryGetPosition(address, out x, out y, out z))
		{
			return true;
		}
		return fallback != null && fallback.TryGetLocalPosition(out x, out y, out z);
	}

	public bool TryGetEntityCount(out int count)
	{
		count = 0;
		if (layout != null && layout.HasEntities)
		{
			if (layout.EntityCountAddress != 0UL && TryReadInt(layout.EntityCountAddress, out count) && count > 0 && count <= 8192)
			{
				return true;
			}
			count = layout.EntityCount;
			return true;
		}
		return fallback != null && fallback.TryGetEntityCount(out count);
	}

	public bool TryGetEntityAddress(int index, out ulong address)
	{
		address = 0UL;
		if (layout != null && layout.HasEntities && index >= 0)
		{
			ulong value;
			if (TryReadU64(layout.EntityListAddress + (ulong)(index * 8), out value) && value != 0UL)
			{
				address = value;
				return true;
			}
		}
		return fallback != null && fallback.TryGetEntityAddress(index, out address);
	}

	private bool TryReadU64(ulong address, out ulong value)
	{
		value = 0UL;
		return reader != null && reader.TryRead(address, out value);
	}

	public bool TryGetPosition(ulong address, out float x, out float y, out float z)
	{
		x = 0f;
		y = 0f;
		z = 0f;
		if (address == 0UL)
		{
			return false;
		}
		ulong baseX;
		long offsetX;
		ulong baseY;
		long offsetY;
		ulong baseZ;
		long offsetZ;
		if (ResolveField(address, PositionXAliases, out baseX, out offsetX)
			&& ResolveField(address, PositionYAliases, out baseY, out offsetY)
			&& ResolveField(address, PositionZAliases, out baseZ, out offsetZ)
			&& TryReadFloat(baseX + (ulong)offsetX, out x)
			&& TryReadFloat(baseY + (ulong)offsetY, out y)
			&& TryReadFloat(baseZ + (ulong)offsetZ, out z))
		{
			return true;
		}
		// 位置是"一个向量起点"而不是三个独立分量的布局。
		ulong baseVector;
		long offsetVector;
		if (ResolveField(address, PositionVectorAliases, out baseVector, out offsetVector)
			&& TryReadFloat(baseVector + (ulong)offsetVector, out x)
			&& TryReadFloat(baseVector + (ulong)(offsetVector + 4), out y)
			&& TryReadFloat(baseVector + (ulong)(offsetVector + 8), out z))
		{
			return true;
		}
		return fallback != null && fallback.TryGetPosition(address, out x, out y, out z);
	}

	public bool TryGetHealth(ulong address, out float current, out float max)
	{
		current = 0f;
		max = 0f;
		if (address == 0UL)
		{
			return false;
		}
		ulong baseCurrent;
		long offsetCurrent;
		if (!ResolveField(address, HealthAliases, out baseCurrent, out offsetCurrent))
		{
			return fallback != null && fallback.TryGetHealth(address, out current, out max);
		}
		if (!TryReadFloat(baseCurrent + (ulong)offsetCurrent, out current))
		{
			return fallback != null && fallback.TryGetHealth(address, out current, out max);
		}
		ulong baseMax;
		long offsetMax;
		if (ResolveField(address, MaxHealthAliases, out baseMax, out offsetMax)
			&& TryReadFloat(baseMax + (ulong)offsetMax, out max))
		{
			return true;
		}
		// 没有最大血量时用当前血量兜底（血量条按满血画）。
		max = current;
		return true;
	}

	public bool TryGetTeam(ulong address, out int teamId)
	{
		teamId = -1;
		if (address == 0UL)
		{
			return false;
		}
		ulong baseAddress;
		long offset;
		if (!ResolveField(address, TeamAliases, out baseAddress, out offset))
		{
			return fallback != null && fallback.TryGetTeam(address, out teamId);
		}
		return TryReadInt(baseAddress + (ulong)offset, out teamId);
	}

	public bool TryGetName(ulong address, out string name)
	{
		name = "";
		if (address == 0UL)
		{
			return false;
		}
		// 运行时推断出的姓名字符串指针（实体头 + PlayerNamePointer 偏移处是指向 ASCII 的指针）。
		ulong baseAddress;
		long offset;
		if (ResolveField(address, NameAliases, out baseAddress, out offset))
		{
			ulong text;
			if (TryReadU64(baseAddress + (ulong)offset, out text) && text != 0UL)
			{
				string value = ReadAnsiString(text, 64);
				if (!string.IsNullOrEmpty(value))
				{
					name = value;
					return true;
				}
			}
		}
		if (fallback != null && fallback.TryGetName(address, out name))
		{
			return true;
		}
		return false;
	}

	/// <summary>读取一段 ANSI 字符串（遇到 NUL 或非可打印字符即停）。</summary>
	private string ReadAnsiString(ulong address, int maxLength)
	{
		if (reader == null || address == 0UL || maxLength <= 0)
		{
			return "";
		}
		byte[] blob = reader.ReadBytes(address, maxLength);
		if (blob == null)
		{
			return "";
		}
		System.Text.StringBuilder builder = new System.Text.StringBuilder();
		for (int i = 0; i < blob.Length; i++)
		{
			byte b = blob[i];
			if (b == 0)
			{
				break;
			}
			if (b < 0x20 || b > 0x7E)
			{
				return "";
			}
			builder.Append((char)b);
		}
		return builder.ToString();
	}
}
