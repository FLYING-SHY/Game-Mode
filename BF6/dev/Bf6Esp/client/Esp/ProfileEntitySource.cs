using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bf6Esp;

/// <summary>
/// 用档案里的 Offsets / Structs 直接读游戏内存的实体来源。
///
/// 档案偏移语义（运行时绝对地址，模块基址 + RVA）：
///   EntityList          实体指针数组首地址，每个元素 8 字节
///   EntityCount         指向 int 的地址
///   LocalPlayer         指向本地玩家实体指针的地址
///   LocalTeamId         指向 int 的地址
///   ViewProjectionMatrix 指向 16 个 float 的矩阵
///   PlayerNameBuffer    指向本地玩家名字缓冲
///
/// Structs（相对实体首地址的成员偏移，十六进制字符串）：
///   Transform       PositionX / PositionY / PositionZ
///   HealthComponent CurrentHealth / MaxHealth
///   Entity          TeamId / NamePointer
/// </summary>
internal class ProfileEntitySource : IEntitySource
{
	protected readonly MemoryReader reader;

	protected readonly OffsetProfile profile;

	protected readonly Action<string> log;

	/// <summary>可选的运行时解析结果（校准模式产出），优先级高于档案偏移。</summary>
	protected readonly Dictionary<string, ulong> resolved;

	public virtual string Name
	{
		get { return "档案"; }
	}

	public ProfileEntitySource(MemoryReader reader, OffsetProfile profile, Dictionary<string, ulong> resolved, Action<string> log)
	{
		this.reader = reader;
		this.profile = profile;
		this.resolved = resolved;
		this.log = log ?? delegate(string message)
		{
		};
	}

	/// <summary>取某个目标的运行时地址；0 表示未知。</summary>
	protected virtual ulong Resolve(string name)
	{
		ulong value;
		if (resolved != null && resolved.TryGetValue(name, out value) && value != 0UL)
		{
			return value;
		}
		if (profile == null)
		{
			return 0UL;
		}
		return profile.Offset(name);
	}

	/// <summary>取结构体成员偏移；-1 表示未知。</summary>
	protected long Member(string structName, string member)
	{
		if (profile == null)
		{
			return -1L;
		}
		StructLayout60 layout = profile.Struct(structName);
		if (layout == null)
		{
			return -1L;
		}
		return layout.OffsetOf(member);
	}

	public virtual bool TryGetViewProjection(out float[] matrix)
	{
		matrix = null;
		ulong address = Resolve("ViewProjectionMatrix");
		if (address == 0UL)
		{
			return false;
		}
		float[] values = new float[16];
		for (int i = 0; i < values.Length; i++)
		{
			if (!reader.TryRead(address + (ulong)(i * 4), out values[i]))
			{
				return false;
			}
		}
		matrix = values;
		return true;
	}

	public virtual bool TryGetLocalTeam(out int teamId)
	{
		teamId = 0;
		ulong address = Resolve("LocalTeamId");
		if (address == 0UL)
		{
			return false;
		}
		return reader.TryRead(address, out teamId);
	}

	public virtual bool TryGetLocalPlayerAddress(out ulong address)
	{
		address = 0UL;
		ulong pointer = Resolve("LocalPlayer");
		if (pointer == 0UL)
		{
			return false;
		}
		return reader.TryRead(pointer, out address) && address != 0UL;
	}

	public virtual bool TryGetLocalPosition(out float x, out float y, out float z)
	{
		x = 0f;
		y = 0f;
		z = 0f;
		ulong address;
		if (!TryGetLocalPlayerAddress(out address))
		{
			return false;
		}
		return TryGetPosition(address, out x, out y, out z);
	}

	public virtual bool TryGetEntityCount(out int count)
	{
		count = 0;
		ulong address = Resolve("EntityCount");
		if (address == 0UL)
		{
			return false;
		}
		return reader.TryRead(address, out count);
	}

	public virtual bool TryGetEntityAddress(int index, out ulong address)
	{
		address = 0UL;
		ulong array = Resolve("EntityList");
		if (array == 0UL || index < 0)
		{
			return false;
		}
		if (!reader.TryRead(array + (ulong)(index * 8), out address))
		{
			return false;
		}
		return address != 0UL;
	}

	public virtual bool TryGetPosition(ulong address, out float x, out float y, out float z)
	{
		x = 0f;
		y = 0f;
		z = 0f;
		if (address == 0UL)
		{
			return false;
		}
		long offsetX = Member("Transform", "PositionX");
		long offsetY = Member("Transform", "PositionY");
		long offsetZ = Member("Transform", "PositionZ");
		if (offsetX < 0L || offsetY < 0L || offsetZ < 0L)
		{
			return false;
		}
		return reader.TryRead(address + (ulong)offsetX, out x)
			&& reader.TryRead(address + (ulong)offsetY, out y)
			&& reader.TryRead(address + (ulong)offsetZ, out z);
	}

	public virtual bool TryGetHealth(ulong address, out float current, out float max)
	{
		current = 0f;
		max = 0f;
		if (address == 0UL)
		{
			return false;
		}
		long offsetCurrent = Member("HealthComponent", "CurrentHealth");
		long offsetMax = Member("HealthComponent", "MaxHealth");
		if (offsetCurrent < 0L || offsetMax < 0L)
		{
			return false;
		}
		return reader.TryRead(address + (ulong)offsetCurrent, out current)
			&& reader.TryRead(address + (ulong)offsetMax, out max);
	}

	public virtual bool TryGetTeam(ulong address, out int teamId)
	{
		teamId = -1;
		if (address == 0UL)
		{
			return false;
		}
		long offset = Member("Entity", "TeamId");
		if (offset < 0L)
		{
			return false;
		}
		return reader.TryRead(address + (ulong)offset, out teamId);
	}

	public virtual bool TryGetName(ulong address, out string name)
	{
		name = "";
		if (address == 0UL)
		{
			return false;
		}
		long offset = Member("Entity", "NamePointer");
		if (offset < 0L)
		{
			return false;
		}
		ulong buffer = address + (ulong)offset;
		ulong pointer;
		if (reader.TryRead(buffer, out pointer) && pointer != 0UL)
		{
			string indirect = ReadString(pointer, 64);
			if (indirect.Length > 0)
			{
				name = indirect;
				return true;
			}
		}
		string inline = ReadString(buffer, 64);
		if (inline.Length > 0)
		{
			name = inline;
			return true;
		}
		return false;
	}

	/// <summary>读取一段以 NUL 结尾的 UTF-8 文本。</summary>
	protected string ReadString(ulong address, int maxLength)
	{
		byte[] buffer = reader.ReadBytes(address, maxLength);
		if (buffer == null)
		{
			return "";
		}
		int length = 0;
		while (length < buffer.Length && buffer[length] != 0)
		{
			length++;
		}
		if (length == 0)
		{
			return "";
		}
		string text = Encoding.UTF8.GetString(buffer, 0, length);
		text = text.Trim();
		if (text.Length == 0)
		{
			return "";
		}
		foreach (char c in text)
		{
			if (char.IsControl(c))
			{
				return "";
			}
		}
		return text;
	}

	/// <summary>把地址写进日志用的十六进制文本。</summary>
	protected static string Hex(ulong value)
	{
		return "0x" + value.ToString("X", CultureInfo.InvariantCulture);
	}
}
