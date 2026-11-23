using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Bf6Esp;

/// <summary>地址区间集合（用于判断某个指针是否落在合法范围内）。</summary>
internal sealed class AddressRegions60
{
	private readonly List<ulong[]> ranges = new List<ulong[]>();

	public void Add(ulong low, ulong high)
	{
		if (high > low)
		{
			ranges.Add(new ulong[] { low, high });
		}
	}

	public bool Contains(ulong address)
	{
		for (int i = 0; i < ranges.Count; i++)
		{
			ulong[] range = ranges[i];
			if (address >= range[0] && address < range[1])
			{
				return true;
			}
		}
		return false;
	}

	public int Count
	{
		get { return ranges.Count; }
	}
}

/// <summary>已加载到本地内存的模块节（避免对同一区域反复做 ReadProcessMemory）。</summary>
internal sealed class LoadedRegion60
{
	public string Name;

	public ulong Va;

	public byte[] Data;
}

/// <summary>
/// 远端内存视图：优先从已加载的节缓冲里取数据，取不到才回落到 ReadProcessMemory。
/// 全部只读。
/// </summary>
internal sealed class MemoryView60
{
	private readonly MemoryReader reader;

	private readonly List<LoadedRegion60> regions = new List<LoadedRegion60>();

	private readonly Dictionary<ulong, string> stringCache = new Dictionary<ulong, string>();

	/// <summary>已发生的字符串读取次数（命中缓存的不计）。</summary>
	internal long StringReads;

	/// <summary>字符串读取上限；超过后停止向外读，避免在超大堆上拖死。</summary>
	internal long StringReadLimit = 400000L;

	/// <summary>候选起点（指向名字的指针）上限。</summary>
	internal long CandidateLimit = 400000L;

	internal bool BudgetExhausted;

	internal MemoryView60(MemoryReader reader)
	{
		this.reader = reader;
	}

	public void Add(string name, ulong va, byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			return;
		}
		LoadedRegion60 region = new LoadedRegion60 { Name = name, Va = va, Data = data };
		regions.Add(region);
	}

	public int RegionCount
	{
		get { return regions.Count; }
	}

	public IList<LoadedRegion60> Regions
	{
		get { return regions; }
	}

	public bool TryRead(ulong address, int size, out byte[] data)
	{
		data = null;
		if (size <= 0)
		{
			return false;
		}
		for (int i = 0; i < regions.Count; i++)
		{
			LoadedRegion60 region = regions[i];
			ulong end = region.Va + (ulong)region.Data.Length;
			if (address >= region.Va && address + (ulong)size <= end)
			{
				data = new byte[size];
				Buffer.BlockCopy(region.Data, (int)(address - region.Va), data, 0, size);
				return true;
			}
		}
		byte[] buffer = new byte[size];
		if (reader == null || !reader.ReadBytes(address, buffer, 0, size))
		{
			return false;
		}
		data = buffer;
		return true;
	}

	public bool TryReadUInt64(ulong address, out ulong value)
	{
		value = 0UL;
		byte[] data;
		if (!TryRead(address, 8, out data))
		{
			return false;
		}
		value = BitConverter.ToUInt64(data, 0);
		return true;
	}

	public bool TryReadUInt16(ulong address, out ushort value)
	{
		value = 0;
		byte[] data;
		if (!TryRead(address, 2, out data))
		{
			return false;
		}
		value = BitConverter.ToUInt16(data, 0);
		return true;
	}

	/// <summary>地址是否落在已缓存（已读到内存）的区间里。</summary>
	public bool IsCached(ulong address, int size)
	{
		if (size <= 0)
		{
			return false;
		}
		for (int i = 0; i < regions.Count; i++)
		{
			LoadedRegion60 region = regions[i];
			ulong end = region.Va + (ulong)region.Data.Length;
			if (address >= region.Va && address + (ulong)size <= end)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>读取 ASCII 字符串（遇 NUL 结束）。带缓存与读取预算。</summary>
	public bool TryReadString(ulong address, int maxLength, out string value)
	{
		value = null;
		if (address == 0UL || maxLength <= 0)
		{
			return false;
		}
		string cached;
		if (stringCache.TryGetValue(address, out cached))
		{
			value = cached;
			return cached != null;
		}
		if (StringReads >= StringReadLimit)
		{
			BudgetExhausted = true;
			stringCache[address] = null;
			return false;
		}
		StringReads++;
		byte[] data = null;
		for (int i = 0; i < regions.Count; i++)
		{
			LoadedRegion60 region = regions[i];
			ulong end = region.Va + (ulong)region.Data.Length;
			if (address >= region.Va && address < end)
			{
				int start = (int)(address - region.Va);
				int available = region.Data.Length - start;
				int length = Math.Min(maxLength, available);
				data = new byte[length];
				Buffer.BlockCopy(region.Data, start, data, 0, length);
				break;
			}
		}
		if (data == null)
		{
			data = new byte[maxLength];
			if (reader == null || !reader.ReadBytes(address, data, 0, maxLength))
			{
				stringCache[address] = null;
				return false;
			}
		}
		int textLength = 0;
		while (textLength < data.Length && data[textLength] != 0)
		{
			textLength++;
		}
		if (textLength == 0)
		{
			stringCache[address] = null;
			return false;
		}
		string text = Encoding.ASCII.GetString(data, 0, textLength);
		stringCache[address] = text;
		value = text;
		return true;
	}
}

/// <summary>反射字段：名字 + 在宿主结构中的偏移。</summary>
internal sealed class ReflectionField60
{
	public string Name;

	public int Offset;

	/// <summary>宿主类型名（Pass A 的描述符数组通常拿不到类型名，为空）。</summary>
	public string Owner;

	/// <summary>置信度：3 = 完整类型表；2 = 描述符数组；1 = 孤立记录。</summary>
	public int Confidence;

	public ulong RecordAddress;

	public override string ToString()
	{
		return Name + "@0x" + Offset.ToString("X") + (string.IsNullOrEmpty(Owner) ? string.Empty : (" (" + Owner + ")"));
	}
}

/// <summary>反射类型：名字 + 尺寸 + 字段表 + 父类型名。</summary>
internal sealed class ReflectionType60
{
	public string Name;

	public int Size;

	public ulong Address;

	public string Super;

	public readonly List<ReflectionField60> Fields = new List<ReflectionField60>();
}

/// <summary>反射扫描结果。</summary>
internal sealed class ReflectionScanResult60
{
	public readonly List<ReflectionType60> Types = new List<ReflectionType60>();

	public readonly List<ReflectionField60> Fields = new List<ReflectionField60>();

	public readonly Dictionary<string, ReflectionField60> Best = new Dictionary<string, ReflectionField60>(StringComparer.OrdinalIgnoreCase);

	public int NamePointers;

	public int TypeCandidates;

	public int DescriptorRuns;

	public readonly List<string> Notes = new List<string>();

	public void AddField(ReflectionField60 field)
	{
		if (field == null || string.IsNullOrEmpty(field.Name))
		{
			return;
		}
		Fields.Add(field);
		ReflectionField60 current;
		if (!Best.TryGetValue(field.Name, out current) || field.Confidence > current.Confidence)
		{
			Best[field.Name] = field;
		}
	}

	public bool TryGetOffset(string fieldName, out int offset)
	{
		offset = 0;
		ReflectionField60 field;
		if (fieldName != null && Best.TryGetValue(fieldName, out field))
		{
			offset = field.Offset;
			return true;
		}
		return false;
	}
}

/// <summary>
/// Frostbite 运行时反射解析器（只读）。
///
/// 思路：Frostbite 的类型系统会把"类型名 / 字段名 / 字段偏移"以结构体的形式放在
/// 进程内存里。这里不依赖任何静态哈希，而是在运行时把符合 Frostbite 布局的记录找出来：
///
///   Pass B（MEC 式完整类型表）：
///     TypeInfo 节点 : +0x00 info*  +0x08 next*
///     info 结构     : +0x00 name* +0x08 u16 flags +0x0A u16 size +0x10 module*
///                     +0x18 u8 align +0x1A u16 fieldCount
///                     class: +0x20 super* +0x30 fieldArray*
///                     struct: +0x28 fieldArray*
///     FieldInfo     : 0x18 字节 = +0x00 name* +0x08 u16 flags +0x0A u16 offset +0x10 type*
///
///   Pass A（描述符数组兜底）：即使类型头布局不同，字段描述数组通常是
///     {name*, flags, offset(u16), type*} 的定长记录流。找到连续 >= 3 条的记录流即可
///     得到"字段名 -> 偏移"映射，用于血量 / 阵营 / 位置等常用字段。
///
/// 全部读取都走只读通道，不写入、不注入。
/// </summary>
internal static class RuntimeReflection60
{
	internal const int MaxIdentifierLength = 96;

	internal const int MaxFieldOffset = 0x2000;

	internal const int MaxTypes = 2048;

	internal const int MaxFields = 8192;

	private const int FieldInfoSize = 0x18;

	private static readonly int[] RunStrides = { 0x18, 0x10, 0x20 };

	private static readonly int[] OffsetPositions = { 0x0A, 0x08, 0x0C };

	/// <summary>执行扫描。</summary>
	/// <param name="reader">只读内存读取器。</param>
	/// <param name="moduleBase">目标模块基址。</param>
	/// <param name="image">目标模块 PE 视图（可为 null）。</param>
	/// <param name="extraNameRegions">额外的"名字字符串"允许区间（堆里的反射名字等），每项为 [lo, hi)。</param>
	internal static ReflectionScanResult60 Scan(MemoryReader reader, ulong moduleBase, PeImage image, IList<ulong[]> extraNameRegions)
	{
		ReflectionScanResult60 result = new ReflectionScanResult60();
		MemoryView60 view = new MemoryView60(reader);
		AddressRegions60 nameRegions = new AddressRegions60();

		if (image != null)
		{
			foreach (PeSection section in image.Sections)
			{
				ulong low = moduleBase + section.Rva;
				ulong size = section.SizeInMemory;
				if (size == 0UL)
				{
					continue;
				}
				nameRegions.Add(low, low + size);
				if (section.Name == ".rdata" || section.Name == ".data" || section.Name == "typeinfo" || section.Name == "fieldinf")
				{
					byte[] data = ReadSection(reader, low, size);
					if (data != null)
					{
						view.Add(section.Name, low, data);
					}
				}
			}
		}

		if (extraNameRegions != null)
		{
			ulong extraBudget = 0x800000UL;
			foreach (ulong[] region in extraNameRegions)
			{
				if (region == null || region.Length < 2 || region[1] <= region[0])
				{
					continue;
				}
				nameRegions.Add(region[0], region[1]);
				ulong span = region[1] - region[0];
				if (span > 0x100000UL)
				{
					span = 0x100000UL;
				}
				if (span > extraBudget)
				{
					span = extraBudget;
				}
				extraBudget -= span;
				if (span == 0UL)
				{
					continue;
				}
				byte[] data = ReadSection(reader, region[0], span);
				if (data != null)
				{
					view.Add("extra", region[0], data);
				}
				if (extraBudget == 0UL)
				{
					break;
				}
			}
		}

		HashSet<ulong> namePointers = CollectNamePointers(view, nameRegions, result);
		ScanTypeInfos(view, nameRegions, result);
		ScanDescriptorRuns(view, namePointers, result);

		if (view.BudgetExhausted)
		{
			result.Notes.Add("扫描预算用尽：字符串读取 " +
				view.StringReads.ToString(System.Globalization.CultureInfo.InvariantCulture) + " 次，结果为部分覆盖。");
		}

		result.Notes.Add("名字指针 " + result.NamePointers.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			" 个；类型候选 " + result.TypeCandidates.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			" 个；完整类型 " + result.Types.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			" 个；描述符数组 " + result.DescriptorRuns.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			" 段；字段 " + result.Fields.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " 条。");
		return result;
	}

	private static byte[] ReadSection(MemoryReader reader, ulong low, ulong size)
	{
		if (reader == null || size == 0UL || size > 0x40000000UL)
		{
			return null;
		}
		byte[] buffer = new byte[(int)size];
		int done = 0;
		while (done < buffer.Length)
		{
			int chunk = Math.Min(0x100000, buffer.Length - done);
			if (!reader.ReadBytes(low + (ulong)done, buffer, done, chunk))
			{
				return null;
			}
			done += chunk;
		}
		return buffer;
	}

	/// <summary>判断一个字符串是不是像样的标识符（类型名 / 字段名）。</summary>
	internal static bool IsIdentifier(string text)
	{
		if (string.IsNullOrEmpty(text) || text.Length > MaxIdentifierLength)
		{
			return false;
		}
		char first = text[0];
		if (!((first >= 'A' && first <= 'Z') || (first >= 'a' && first <= 'z') || first == '_' || first == '$'))
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			bool ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') ||
				c == '_' || c == '$' || c == ':' || c == '.' || c == '<' || c == '>' || c == '[' || c == ']' || c == ' ';
			if (!ok)
			{
				return false;
			}
		}
		return true;
	}

	private static HashSet<ulong> CollectNamePointers(MemoryView60 view, AddressRegions60 nameRegions, ReflectionScanResult60 result)
	{
		HashSet<ulong> pointers = new HashSet<ulong>();
		foreach (LoadedRegion60 region in view.Regions)
		{
			byte[] data = region.Data;
			for (int offset = 0; offset + 8 <= data.Length; offset += 8)
			{
				ulong value = BitConverter.ToUInt64(data, offset);
				if (value == 0UL || !nameRegions.Contains(value))
				{
					continue;
				}
				if (pointers.Contains(value))
				{
					continue;
				}
				string text;
				if (view.TryReadString(value, MaxIdentifierLength, out text) && IsIdentifier(text))
				{
					pointers.Add(value);
				}
			}
		}
		result.NamePointers = pointers.Count;
		return pointers;
	}

	/// <summary>Pass B：MEC 式 TypeInfo 链表。</summary>
	private static void ScanTypeInfos(MemoryView60 view, AddressRegions60 nameRegions, ReflectionScanResult60 result)
	{
		HashSet<ulong> visited = new HashSet<ulong>();
		long candidates = 0L;
		foreach (LoadedRegion60 region in view.Regions)
		{
			byte[] data = region.Data;
			for (int offset = 0; offset + 8 <= data.Length; offset += 8)
			{
				ulong value = BitConverter.ToUInt64(data, offset);
				if (value == 0UL || !nameRegions.Contains(value) || visited.Contains(value))
				{
					continue;
				}
				candidates++;
				if (candidates > view.CandidateLimit)
				{
					view.BudgetExhausted = true;
					return;
				}
				result.TypeCandidates++;
				ReflectionType60 type = TryReadTypeInfo(view, value, nameRegions, result);
				if (type == null)
				{
					continue;
				}
				visited.Add(value);
				if (result.Types.Count >= MaxTypes)
				{
					result.Notes.Add("类型数量达到上限 " + MaxTypes.ToString(System.Globalization.CultureInfo.InvariantCulture) + "，停止扩展。");
					return;
				}
				result.Types.Add(type);
				foreach (ReflectionField60 field in type.Fields)
				{
					result.AddField(field);
				}
			}
		}
	}

	private static ReflectionType60 TryReadTypeInfo(MemoryView60 view, ulong infoAddress, AddressRegions60 nameRegions, ReflectionScanResult60 result)
	{
		byte[] head;
		if (!view.TryRead(infoAddress, 0x38, out head))
		{
			return null;
		}
		ulong namePointer = BitConverter.ToUInt64(head, 0x00);
		if (namePointer == 0UL || !nameRegions.Contains(namePointer))
		{
			return null;
		}
		string typeName;
		if (!view.TryReadString(namePointer, MaxIdentifierLength, out typeName) || !IsIdentifier(typeName))
		{
			return null;
		}
		ushort flags = BitConverter.ToUInt16(head, 0x08);
		ushort size = BitConverter.ToUInt16(head, 0x0A);
		ushort fieldCount = BitConverter.ToUInt16(head, 0x1A);
		if (fieldCount > 1024)
		{
			return null;
		}
		ulong fieldArray = BitConverter.ToUInt64(head, 0x30);
		if (!nameRegions.Contains(fieldArray))
		{
			fieldArray = BitConverter.ToUInt64(head, 0x28);
		}
		if (fieldCount > 0 && !nameRegions.Contains(fieldArray))
		{
			return null;
		}
		ReflectionType60 type = new ReflectionType60
		{
			Name = typeName,
			Size = size,
			Address = infoAddress,
			Super = null
		};
		ulong superPointer = BitConverter.ToUInt64(head, 0x20);
		if (superPointer != 0UL && nameRegions.Contains(superPointer))
		{
			string superName;
			if (view.TryReadString(superPointer, MaxIdentifierLength, out superName) && IsIdentifier(superName))
			{
				type.Super = superName;
			}
		}
		if (fieldCount == 0)
		{
			// 没有字段表的类型仍然有用（纯类型名），但可信度低，仅记录名字。
			return type;
		}
		int count = Math.Min((int)fieldCount, 64);
		if (!view.IsCached(fieldArray, count * FieldInfoSize))
		{
			return type;
		}
		byte[] records;
		if (!view.TryRead(fieldArray, count * FieldInfoSize, out records))
		{
			return type;
		}
		int accepted = 0;
		for (int i = 0; i < count; i++)
		{
			int baseOffset = i * FieldInfoSize;
			ulong fieldNamePointer = BitConverter.ToUInt64(records, baseOffset);
			if (fieldNamePointer == 0UL || !nameRegions.Contains(fieldNamePointer))
			{
				continue;
			}
			string fieldName;
			if (!view.TryReadString(fieldNamePointer, MaxIdentifierLength, out fieldName) || !IsIdentifier(fieldName))
			{
				continue;
			}
			ushort fieldOffset = BitConverter.ToUInt16(records, baseOffset + 0x0A);
			if (fieldOffset > MaxFieldOffset)
			{
				continue;
			}
			type.Fields.Add(new ReflectionField60
			{
				Name = fieldName,
				Offset = fieldOffset,
				Owner = typeName,
				Confidence = 3,
				RecordAddress = fieldArray + (ulong)baseOffset
			});
			accepted++;
		}
		if (accepted == 0 && type.Fields.Count == 0)
		{
			return null;
		}
		return type;
	}

	/// <summary>Pass A：描述符数组兜底扫描。</summary>
	private static void ScanDescriptorRuns(MemoryView60 view, HashSet<ulong> namePointers, ReflectionScanResult60 result)
	{
		if (namePointers.Count == 0)
		{
			return;
		}
		long candidates = 0L;
		foreach (LoadedRegion60 region in view.Regions)
		{
			byte[] data = region.Data;
			for (int offset = 0; offset + 8 <= data.Length; offset += 8)
			{
				ulong value = BitConverter.ToUInt64(data, offset);
				if (!namePointers.Contains(value))
				{
					continue;
				}
				candidates++;
				if (candidates > view.CandidateLimit)
				{
					view.BudgetExhausted = true;
					return;
				}
				ulong recordAddress = region.Va + (ulong)offset;
				foreach (int stride in RunStrides)
				{
					int position = PickOffsetPosition(view, data, offset, stride);
					if (position < 0)
					{
						continue;
					}
					List<ReflectionField60> run = new List<ReflectionField60>();
					int cursor = offset;
					while (cursor + stride + 8 <= data.Length && run.Count < 64)
					{
						ulong pointer = BitConverter.ToUInt64(data, cursor);
						if (!namePointers.Contains(pointer))
						{
							break;
						}
						string fieldName;
						if (!view.TryReadString(pointer, MaxIdentifierLength, out fieldName) || !IsIdentifier(fieldName))
						{
							break;
						}
						ushort fieldOffset = BitConverter.ToUInt16(data, cursor + position);
						if (fieldOffset > MaxFieldOffset)
						{
							break;
						}
						run.Add(new ReflectionField60
						{
							Name = fieldName,
							Offset = fieldOffset,
							Owner = null,
							Confidence = 2,
							RecordAddress = recordAddress + (ulong)(cursor - offset)
						});
						cursor += stride;
					}
					if (run.Count >= 3)
					{
						result.DescriptorRuns++;
						foreach (ReflectionField60 field in run)
						{
							if (result.Fields.Count >= MaxFields)
							{
								break;
							}
							result.AddField(field);
						}
						break;
					}
				}
			}
		}
	}

	/// <summary>为一个候选记录挑出"偏移字段"所在的 u16 位置；挑不出返回 -1。</summary>
	private static int PickOffsetPosition(MemoryView60 view, byte[] data, int offset, int stride)
	{
		foreach (int position in OffsetPositions)
		{
			if (offset + position + 2 > data.Length)
			{
				continue;
			}
			ushort value = BitConverter.ToUInt16(data, offset + position);
			if (value > MaxFieldOffset)
			{
				continue;
			}
			// 要求相邻记录在同一位置也是小值，避免把 flags 之类的字段当成偏移。
			if (offset + stride + position + 2 <= data.Length)
			{
				ushort next = BitConverter.ToUInt16(data, offset + stride + position);
				if (next > MaxFieldOffset)
				{
					continue;
				}
			}
			return position;
		}
		return -1;
	}
}
