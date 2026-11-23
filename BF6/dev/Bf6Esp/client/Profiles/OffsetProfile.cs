using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bf6Esp;

/// <summary>结构体成员偏移描述：字段名 -> 十六进制偏移字符串。</summary>
[DataContract]
internal sealed class StructLayout60
{
	[DataMember(Name = "name", Order = 0)]
	public string Name;

	/// <summary>成员偏移，形如 "0x1A8"。</summary>
	[DataMember(Name = "fields", Order = 1)]
	public Dictionary<string, string> Fields;

	public long OffsetOf(string member)
	{
		long result;
		if (Fields != null && member != null && Fields.TryGetValue(member, out string text) && OffsetProfile.TryParseHex(text, out result))
		{
			return result;
		}
		return -1L;
	}
}

/// <summary>特征码描述。</summary>
[DataContract]
internal sealed class SignatureSpec60
{
	[DataMember(Name = "name", Order = 0)]
	public string Name;

	/// <summary>模式串，例如 "48 8B ?? ?? 48 85 C0"。</summary>
	[DataMember(Name = "pattern", Order = 1)]
	public string Pattern;

	/// <summary>限定扫描的节名；为空表示全模块。</summary>
	[DataMember(Name = "section", Order = 2)]
	public string Section;

	/// <summary>命中点之后的结果偏移（相对命中地址）。</summary>
	[DataMember(Name = "resultOffset", Order = 3)]
	public int ResultOffset;

	[DataMember(Name = "description", Order = 4)]
	public string Description;
}

/// <summary>签名到偏移的解析规则。</summary>
[DataContract]
internal sealed class ResolutionRule60
{
	/// <summary>要解析出的目标名（对应 Offsets 的键）。</summary>
	[DataMember(Name = "target", Order = 0)]
	public string Target;

	/// <summary>使用的签名名（对应 Signatures 的 Name）。</summary>
	[DataMember(Name = "signature", Order = 1)]
	public string Signature;

	/// <summary>
	/// 解析方式：
	///   match       —— 命中地址本身就是目标地址
	///   match+off   —— 命中地址 + Offset
	///   rel32       —— 命中地址 + Offset 处存放 int32 相对位移（x64 RIP 相对寻址）
	///   value       —— 命中地址 + Offset 处存放一个 64 位指针值，值本身就是目标
	/// </summary>
	[DataMember(Name = "mode", Order = 2)]
	public string Mode;

	[DataMember(Name = "offset", Order = 3)]
	public int Offset;

	[DataMember(Name = "note", Order = 4)]
	public string Note;
}

/// <summary>档案里记录的 PE 节表项（供签名扫描与启发式定位参考）。</summary>
[DataContract]
internal sealed class ProfileSection60
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

/// <summary>
/// 一个游戏 build 的偏移/特征码档案。
/// 档案以游戏可执行文件的 SHA-256 为键绑定到具体 build，
/// 游戏更新后旧档案不会被误用。
/// </summary>
[DataContract]
internal sealed class OffsetProfile
{
	[DataMember(Name = "gameExe", Order = 0)]
	public string GameExe;

	/// <summary>游戏 exe 的 SHA-256（大写十六进制）。</summary>
	[DataMember(Name = "buildSha256", Order = 1)]
	public string BuildSha256;

	[DataMember(Name = "moduleSize", Order = 2)]
	public long ModuleSize;

	[DataMember(Name = "updatedUtc", Order = 3)]
	public string UpdatedUtc;

	[DataMember(Name = "notes", Order = 4)]
	public string Notes;

	/// <summary>已知的绝对偏移（RVA 或绝对地址），未知时为 0。</summary>
	[DataMember(Name = "offsets", Order = 5)]
	public Dictionary<string, ulong> Offsets;

	[DataMember(Name = "structs", Order = 6)]
	public List<StructLayout60> Structs;

	[DataMember(Name = "signatures", Order = 7)]
	public List<SignatureSpec60> Signatures;

	[DataMember(Name = "enums", Order = 8)]
	public Dictionary<string, int> Enums;

	[DataMember(Name = "resolution", Order = 9)]
	public List<ResolutionRule60> Resolution;

	/// <summary>该 build 的 PE 节表（节名 + RVA + 虚拟大小 + 原始大小）。</summary>
	[DataMember(Name = "sections", Order = 10)]
	public List<ProfileSection60> Sections;

	public OffsetProfile()
	{
		Offsets = new Dictionary<string, ulong>();
		Structs = new List<StructLayout60>();
		Signatures = new List<SignatureSpec60>();
		Enums = new Dictionary<string, int>();
		Resolution = new List<ResolutionRule60>();
		Sections = new List<ProfileSection60>();
	}

	public ulong Offset(string name)
	{
		ulong value;
		if (Offsets != null && name != null && Offsets.TryGetValue(name, out value))
		{
			return value;
		}
		return 0UL;
	}

	public StructLayout60 Struct(string name)
	{
		if (Structs == null || name == null)
		{
			return null;
		}
		foreach (StructLayout60 layout in Structs)
		{
			if (string.Equals(layout.Name, name, System.StringComparison.OrdinalIgnoreCase))
			{
				return layout;
			}
		}
		return null;
	}

	public SignatureSpec60 Signature(string name)
	{
		if (Signatures == null || name == null)
		{
			return null;
		}
		foreach (SignatureSpec60 signature in Signatures)
		{
			if (string.Equals(signature.Name, name, System.StringComparison.OrdinalIgnoreCase))
			{
				return signature;
			}
		}
		return null;
	}

	public int EnumValue(string name, int fallback)
	{
		int value;
		if (Enums != null && name != null && Enums.TryGetValue(name, out value))
		{
			return value;
		}
		return fallback;
	}

	public static bool TryParseHex(string text, out long value)
	{
		value = 0L;
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		text = text.Trim();
		if (text.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
		{
			text = text.Substring(2);
		}
		return long.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out value);
	}
}
