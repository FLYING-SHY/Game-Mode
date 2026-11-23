using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bf6Esp;

/// <summary>校准报告里的节表条目。</summary>
[DataContract]
internal sealed class CalibrationSection60
{
	[DataMember(Name = "name", Order = 0)]
	public string Name;

	[DataMember(Name = "rva", Order = 1)]
	public long Rva;

	[DataMember(Name = "virtualSize", Order = 2)]
	public long VirtualSize;

	[DataMember(Name = "rawSize", Order = 3)]
	public long RawSize;
}

/// <summary>某个签名的扫描结果。</summary>
[DataContract]
internal sealed class CalibrationSignature60
{
	[DataMember(Name = "name", Order = 0)]
	public string Name;

	[DataMember(Name = "pattern", Order = 1)]
	public string Pattern;

	[DataMember(Name = "section", Order = 2)]
	public string Section;

	[DataMember(Name = "description", Order = 3)]
	public string Description;

	[DataMember(Name = "hits", Order = 4)]
	public int Hits;

	[DataMember(Name = "rvas", Order = 5)]
	public List<long> Rvas;
}

/// <summary>疑似投影矩阵的候选。</summary>
[DataContract]
internal sealed class CalibrationProjection60
{
	[DataMember(Name = "address", Order = 0)]
	public long Address;

	[DataMember(Name = "rva", Order = 1)]
	public long Rva;

	[DataMember(Name = "section", Order = 2)]
	public string Section;

	[DataMember(Name = "values", Order = 3)]
	public List<float> Values;
}

/// <summary>指向 typeinfo / fieldinf 的候选指针。</summary>
[DataContract]
internal sealed class CalibrationPointer60
{
	[DataMember(Name = "address", Order = 1)]
	public long Address;

	[DataMember(Name = "rva", Order = 2)]
	public long Rva;

	[DataMember(Name = "section", Order = 3)]
	public string Section;

	[DataMember(Name = "target", Order = 4)]
	public long Target;
}

/// <summary>连续候选指针构成的数组（实体列表候选）。</summary>
[DataContract]
internal sealed class CalibrationArray60
{
	[DataMember(Name = "address", Order = 0)]
	public long Address;

	[DataMember(Name = "rva", Order = 1)]
	public long Rva;

	[DataMember(Name = "section", Order = 2)]
	public string Section;

	[DataMember(Name = "count", Order = 3)]
	public int Count;
}

/// <summary>运行时反射还原出的字段（名字 -> 偏移）。</summary>
[DataContract]
internal sealed class CalibrationReflectionField60
{
	[DataMember(Name = "name", Order = 0)]
	public string Name;

	[DataMember(Name = "offset", Order = 1)]
	public int Offset;

	[DataMember(Name = "owner", Order = 2)]
	public string Owner;

	/// <summary>置信度：3 完整类型表 / 2 描述符数组 / 1 孤立记录。</summary>
	[DataMember(Name = "confidence", Order = 3)]
	public int Confidence;

	[DataMember(Name = "recordAddress", Order = 4)]
	public long RecordAddress;
}

/// <summary>运行时反射还原出的类型。</summary>
[DataContract]
internal sealed class CalibrationReflectionType60
{
	[DataMember(Name = "name", Order = 0)]
	public string Name;

	[DataMember(Name = "size", Order = 1)]
	public int Size;

	[DataMember(Name = "address", Order = 2)]
	public long Address;

	[DataMember(Name = "super", Order = 3)]
	public string Super;

	[DataMember(Name = "fields", Order = 4)]
	public List<CalibrationReflectionField60> Fields;
}

/// <summary>
/// 校准报告：把一次 attach 的全部原始发现落盘，供人工/其它代理转成偏移。
/// </summary>
[DataContract]
internal sealed class CalibrationReport60
{
	[DataMember(Name = "generatedUtc", Order = 0)]
	public string GeneratedUtc;

	[DataMember(Name = "gameExe", Order = 1)]
	public string GameExe;

	[DataMember(Name = "buildSha256", Order = 2)]
	public string BuildSha256;

	[DataMember(Name = "moduleBase", Order = 3)]
	public long ModuleBase;

	[DataMember(Name = "moduleSize", Order = 4)]
	public long ModuleSize;

	[DataMember(Name = "imageBase", Order = 5)]
	public long ImageBase;

	[DataMember(Name = "sections", Order = 6)]
	public List<CalibrationSection60> Sections;

	[DataMember(Name = "signatures", Order = 7)]
	public List<CalibrationSignature60> Signatures;

	[DataMember(Name = "projectionCandidates", Order = 8)]
	public List<CalibrationProjection60> ProjectionCandidates;

	[DataMember(Name = "typeInfoPointers", Order = 9)]
	public List<CalibrationPointer60> TypeInfoPointers;

	[DataMember(Name = "pointerArrays", Order = 10)]
	public List<CalibrationArray60> PointerArrays;

	/// <summary>本次实际解析出来的目标地址（目标名 -> 运行时绝对地址）。</summary>
	[DataMember(Name = "resolved", Order = 11)]
	public Dictionary<string, ulong> Resolved;

	/// <summary>本次使用的结构体定义（从档案复制，便于离线复核）。</summary>
	[DataMember(Name = "structs", Order = 12)]
	public List<StructLayout60> Structs;

	[DataMember(Name = "notes", Order = 13)]
	public List<string> Notes;

	/// <summary>运行时反射还原出的类型（明文名字 + 字段偏移）。</summary>
	[DataMember(Name = "reflectionTypes", Order = 14)]
	public List<CalibrationReflectionType60> ReflectionTypes;

	/// <summary>运行时反射还原出的字段（字段名 -> 偏移）。</summary>
	[DataMember(Name = "reflectionFields", Order = 15)]
	public List<CalibrationReflectionField60> ReflectionFields;

	/// <summary>反射扫描的统计摘要。</summary>
	[DataMember(Name = "reflectionSummary", Order = 16)]
	public string ReflectionSummary;

	public CalibrationReport60()
	{
		Sections = new List<CalibrationSection60>();
		Signatures = new List<CalibrationSignature60>();
		ProjectionCandidates = new List<CalibrationProjection60>();
		TypeInfoPointers = new List<CalibrationPointer60>();
		PointerArrays = new List<CalibrationArray60>();
		Resolved = new Dictionary<string, ulong>();
		Structs = new List<StructLayout60>();
		Notes = new List<string>();
		ReflectionTypes = new List<CalibrationReflectionType60>();
		ReflectionFields = new List<CalibrationReflectionField60>();
	}
}
