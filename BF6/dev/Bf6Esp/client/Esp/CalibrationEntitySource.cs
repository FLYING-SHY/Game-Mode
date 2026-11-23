using System;
using System.Collections.Generic;

namespace Bf6Esp;

/// <summary>
/// 用校准模式产出的解析结果做实体来源。
/// 报告里没有的字段直接视为未知（返回 false），不会去猜。
/// </summary>
internal sealed class CalibrationEntitySource : ProfileEntitySource
{
	public override string Name
	{
		get { return "校准"; }
	}

	public CalibrationEntitySource(MemoryReader reader, CalibrationReport60 report, OffsetProfile profile, Action<string> log)
		: base(reader, Merge(profile, report), (report == null) ? null : report.Resolved, log)
	{
	}

	/// <summary>把档案的结构体定义与校准报告里解析出的地址合成一份内存档案。</summary>
	private static OffsetProfile Merge(OffsetProfile profile, CalibrationReport60 report)
	{
		OffsetProfile merged = new OffsetProfile();
		if (profile != null)
		{
			merged.GameExe = profile.GameExe;
			merged.BuildSha256 = profile.BuildSha256;
			merged.ModuleSize = profile.ModuleSize;
			merged.Notes = profile.Notes;
			merged.Enums = profile.Enums;
			if (profile.Structs != null)
			{
				merged.Structs.AddRange(profile.Structs);
			}
			if (profile.Offsets != null)
			{
				foreach (KeyValuePair<string, ulong> pair in profile.Offsets)
				{
					merged.Offsets[pair.Key] = pair.Value;
				}
			}
		}
		if (report != null)
		{
			if (report.Structs != null)
			{
				foreach (StructLayout60 layout in report.Structs)
				{
					bool exists = false;
					foreach (StructLayout60 current in merged.Structs)
					{
						if (string.Equals(current.Name, layout.Name, StringComparison.OrdinalIgnoreCase))
						{
							exists = true;
							break;
						}
					}
					if (!exists)
					{
						merged.Structs.Add(layout);
					}
				}
			}
			if (report.Resolved != null)
			{
				foreach (KeyValuePair<string, ulong> pair in report.Resolved)
				{
					merged.Offsets[pair.Key] = pair.Value;
				}
			}
		}
		return merged;
	}
}
