using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Bf6Esp;

/// <summary>
/// 校准模式：只读挂接游戏进程，把模块信息、签名命中、通用启发式发现全部落盘。
/// 这里不产生任何偏移结论，只产出原始素材（raw findings），供人工/其它代理确认。
/// </summary>
internal static class CalibrationRunner60
{
	private const int ChunkSize = 0x100000;

	private const int MaxProjectionRecords = 512;

	private const int MaxPointerRecords = 4096;

	private const int MaxArrayRecords = 512;

	private const int MaxRvaPerSignature = 64;

	/// <summary>执行校准；返回进程退出码。</summary>
	internal static int Run(string outputPath)
	{
		ConsoleBridge60.Attach();
		ConsoleBridge60.WriteLine("=== Bf6Esp 校准模式（外部只读） ===");
		GameProcess game = GameProcess.Attach();
		if (game == null)
		{
			ConsoleBridge60.WriteLine("[错误] 未检测到 Battlefield 6 进程（bf6.exe / bf6Trial.exe），请先启动游戏后重试。");
			return 3;
		}
		using (game)
		{
			ConsoleBridge60.WriteLine("[信息] 进程 PID=" + game.ProcessId.ToString(CultureInfo.InvariantCulture) + " 启动时间=" + game.Started.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("[信息] 模块基址=0x" + game.ModuleBase.ToString("X") + " 模块大小=0x" + game.ModuleSize.ToString("X"));
			MemoryReader reader = new MemoryReader(game.Handle, true);
			if (!reader.IsValid)
			{
				ConsoleBridge60.WriteLine("[错误] 无法以只读方式打开进程句柄（权限不足或进程已退出）。");
				return 4;
			}
			Stopwatch watch = Stopwatch.StartNew();
			CalibrationReport60 report = new CalibrationReport60
			{
				GeneratedUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
				GameExe = game.ExePath,
				ModuleBase = (long)game.ModuleBase,
				ModuleSize = game.ModuleSize
			};
			try
			{
				report.BuildSha256 = ProfileStore.ComputeSha256(game.ExePath);
			}
			catch (IOException)
			{
				report.BuildSha256 = string.Empty;
			}
			catch (UnauthorizedAccessException)
			{
				report.BuildSha256 = string.Empty;
			}
			ConsoleBridge60.WriteLine("[信息] on-disk SHA-256 = " + (string.IsNullOrEmpty(report.BuildSha256) ? "(读取失败)" : report.BuildSha256));
			PeImage image = PeImage.FromMemory(reader, game.ModuleBase);
			if (image == null)
			{
				image = PeImage.FromFile(game.ExePath);
				report.Notes.Add("内存中的 PE 头读取失败，节表来自磁盘文件。");
			}
			if (image == null)
			{
				ConsoleBridge60.WriteLine("[错误] 无法解析 PE 头（内存与磁盘都失败）。");
				return 5;
			}
			report.ImageBase = (long)image.ImageBase;
			foreach (PeSection section in image.Sections)
			{
				report.Sections.Add(new CalibrationSection60
				{
					Name = section.Name,
					Rva = section.Rva,
					VirtualSize = section.VirtualSize,
					RawSize = section.RawSize
				});
			}
			ConsoleBridge60.WriteLine("[信息] PE 节表（" + image.Sections.Count.ToString(CultureInfo.InvariantCulture) + " 节）：");
			foreach (PeSection section in image.Sections)
			{
				ConsoleBridge60.WriteLine("        " + section.ToString());
			}
			// 1) 档案签名扫描
			string profilesDirectory = ProfileStore.LocateProfilesDirectory();
			List<OffsetProfile> profiles = ProfileStore.LoadAll(profilesDirectory);
			ConsoleBridge60.WriteLine("[信息] 档案目录=" + profilesDirectory + "，档案数量=" + profiles.Count.ToString(CultureInfo.InvariantCulture));
			OffsetProfile active = ProfileStore.MatchByHash(profiles, report.BuildSha256);
			if (active == null && profiles.Count > 0)
			{
				active = profiles[0];
			}
			if (active != null)
			{
				report.Structs = active.Structs ?? new List<StructLayout60>();
				Dictionary<string, ulong> resolved;
				List<string> resolutionLog;
				if (ProfileStore.TryResolveBySignature(active, reader, game.ModuleBase, image, out resolved, out resolutionLog))
				{
					foreach (KeyValuePair<string, ulong> pair in resolved)
					{
						report.Resolved[pair.Key] = pair.Value;
					}
				}
				foreach (string line in resolutionLog)
				{
					report.Notes.Add("[解析] " + line);
				}
			}
			ScanSignatures(reader, game.ModuleBase, image, profiles, report);
			// 2) 通用启发式
			ScanHeuristics(reader, game.ModuleBase, image, report);
			// 3) 运行时反射解析（明文字段名 -> 偏移）
			ScanRuntimeReflection(reader, game.Handle, game.ModuleBase, image, report);
			// 4) 运行时布局定位（实体容器 + 投影矩阵 + 本地玩家）
			ScanRuntimeLayout(reader, game, image, report);
			watch.Stop();
			report.Notes.Add("耗时 " + watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + " ms；读取失败计数 " + reader.ReadFailures.ToString(CultureInfo.InvariantCulture) + "。");
			report.Notes.Add("本报告只包含原始发现，未确认任何偏移；结论需人工/静态分析复核后再写入档案。");
			string path = ResolveOutputPath(outputPath);
			bool saved = Save(report, path);
			ConsoleBridge60.WriteLine("=== 校准摘要 ===");
			ConsoleBridge60.WriteLine("  模块基址        : 0x" + game.ModuleBase.ToString("X"));
			ConsoleBridge60.WriteLine("  模块大小        : 0x" + game.ModuleSize.ToString("X"));
			ConsoleBridge60.WriteLine("  PE 节数量       : " + image.Sections.Count.ToString(CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("  签名命中条目    : " + report.Signatures.Count.ToString(CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("  投影矩阵候选    : " + report.ProjectionCandidates.Count.ToString(CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("  TypeInfo 指针   : " + report.TypeInfoPointers.Count.ToString(CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("  指针数组候选    : " + report.PointerArrays.Count.ToString(CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("  已解析目标      : " + report.Resolved.Count.ToString(CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("  报告路径        : " + path + (saved ? string.Empty : "（写入失败）"));
			ConsoleBridge60.WriteLine("=== 校准结束 ===");
			return saved ? 0 : 6;
		}
	}

	/// <summary>扫描档案里配置的所有签名，记录命中数与 RVA。</summary>
	internal static void ScanSignatures(MemoryReader reader, ulong moduleBase, PeImage image, List<OffsetProfile> profiles, CalibrationReport60 report)
	{
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (OffsetProfile profile in profiles)
		{
			if (profile == null || profile.Signatures == null)
			{
				continue;
			}
			foreach (SignatureSpec60 spec in profile.Signatures)
			{
				if (spec == null || string.IsNullOrEmpty(spec.Name) || string.IsNullOrEmpty(spec.Pattern))
				{
					continue;
				}
				string key = spec.Name + "|" + spec.Pattern;
				if (!seen.Add(key))
				{
					continue;
				}
				CalibrationSignature60 entry = new CalibrationSignature60
				{
					Name = spec.Name,
					Pattern = spec.Pattern,
					Section = spec.Section,
					Description = spec.Description,
					Rvas = new List<long>()
				};
				SignatureScanner scanner = SignatureScanner.Parse(spec.Pattern);
				if (scanner == null)
				{
					entry.Hits = 0;
					report.Signatures.Add(entry);
					report.Notes.Add("[签名] " + spec.Name + "：模式串尚未填写/非法（" + spec.Pattern + "），跳过。");
					ConsoleBridge60.WriteLine("  [签名] " + spec.Name + "：模式串无效，跳过。");
					continue;
				}
				long scanned;
				List<SignatureHit> hits = scanner.Scan(reader, moduleBase, image, spec.Section, out scanned);
				entry.Hits = hits.Count;
				foreach (SignatureHit hit in hits)
				{
					if (entry.Rvas.Count < MaxRvaPerSignature)
					{
						entry.Rvas.Add((long)hit.Rva);
					}
				}
				report.Signatures.Add(entry);
				ConsoleBridge60.WriteLine("  [签名] " + spec.Name + "：命中 " + hits.Count.ToString(CultureInfo.InvariantCulture) + " 次" + (hits.Count > 0 ? ("，首个 RVA=0x" + hits[0].Rva.ToString("X")) : string.Empty));
			}
		}
	}

	/// <summary>
	/// 运行时反射解析：把 Frostbite 类型系统里的明文字段名与偏移还原出来。
	/// 名字白名单除了模块映像，还包括目标进程里已提交的可读区间（引擎可能把类型名分配在堆上）。
	/// </summary>
	private static void ScanRuntimeReflection(MemoryReader reader, ProcessHandle handle, ulong moduleBase, PeImage image, CalibrationReport60 report)
	{
		List<ulong[]> regions = new List<ulong[]>();
		try
		{
			regions = ProcessRegions60.Enumerate(handle);
		}
		catch (Exception)
		{
			regions = new List<ulong[]>();
		}
		ConsoleBridge60.WriteLine("  [反射] 目标可读区间 " + regions.Count.ToString(CultureInfo.InvariantCulture) + " 个（作为名字白名单）");
		ReflectionScanResult60 result = RuntimeReflection60.Scan(reader, moduleBase, image, regions);
		foreach (string note in result.Notes)
		{
			report.Notes.Add("[反射] " + note);
		}
		report.ReflectionSummary = "types=" + result.Types.Count.ToString(CultureInfo.InvariantCulture) +
			" fields=" + result.Fields.Count.ToString(CultureInfo.InvariantCulture) +
			" namePointers=" + result.NamePointers.ToString(CultureInfo.InvariantCulture) +
			" descriptorRuns=" + result.DescriptorRuns.ToString(CultureInfo.InvariantCulture);
		foreach (ReflectionType60 type in result.Types)
		{
			CalibrationReflectionType60 entry = new CalibrationReflectionType60
			{
				Name = type.Name,
				Size = type.Size,
				Address = (long)type.Address,
				Super = type.Super,
				Fields = new List<CalibrationReflectionField60>()
			};
			foreach (ReflectionField60 field in type.Fields)
			{
				entry.Fields.Add(ToReportField(field));
			}
			report.ReflectionTypes.Add(entry);
		}
		foreach (KeyValuePair<string, ReflectionField60> pair in result.Best)
		{
			report.ReflectionFields.Add(ToReportField(pair.Value));
		}
		ConsoleBridge60.WriteLine("  [反射] 还原类型 " + result.Types.Count.ToString(CultureInfo.InvariantCulture) +
			" 个，唯一字段名 " + result.Best.Count.ToString(CultureInfo.InvariantCulture) + " 个");
		PrintInterestingFields(result);
	}

	private static CalibrationReflectionField60 ToReportField(ReflectionField60 field)
	{
		return new CalibrationReflectionField60
		{
			Name = field.Name,
			Offset = field.Offset,
			Owner = field.Owner,
			Confidence = field.Confidence,
			RecordAddress = (long)field.RecordAddress
		};
	}

	/// <summary>把跟 ESP 直接相关的字段名打出来，方便人工核对。</summary>
	private static void PrintInterestingFields(ReflectionScanResult60 result)
	{
		string[] wanted = { "CurrentHealth", "MaxHealth", "CurrentTeamId", "health", "position", "Position", "transform", "Transform", "name", "Name", "TeamId", "teamId" };
		foreach (string key in wanted)
		{
			ReflectionField60 field;
			if (!result.Best.TryGetValue(key, out field))
			{
				continue;
			}
			ConsoleBridge60.WriteLine("  [反射] " + field.Name + " -> 0x" + field.Offset.ToString("X", CultureInfo.InvariantCulture) +
				(field.Owner != null ? ("  类型=" + field.Owner) : string.Empty) +
				"  置信度=" + field.Confidence.ToString(CultureInfo.InvariantCulture));
		}
	}

	/// <summary>
	/// 运行时布局定位：实体指针数组、投影矩阵、本地玩家/队伍。
	/// 结果写进 report.Resolved（绝对地址），结构体成员偏移写进 report.Structs。
	/// </summary>
	private static void ScanRuntimeLayout(MemoryReader reader, GameProcess game, PeImage image, CalibrationReport60 report)
	{
		ConsoleBridge60.WriteLine("  [布局] 运行时定位器启动（反射字段 + 实体容器 + 投影矩阵）…");
		RuntimeLayout60 layout = RuntimeLocator60.Locate(reader, game, null, delegate(string message)
		{
			ConsoleBridge60.WriteLine("  [布局] " + message);
		});
		foreach (string note in layout.Notes)
		{
			report.Notes.Add("[布局] " + note);
			ConsoleBridge60.WriteLine("  [布局] " + note);
		}
		if (layout.EntityListAddress != 0UL)
		{
			report.Resolved["EntityList"] = layout.EntityListAddress;
		}
		if (layout.EntityCountAddress != 0UL)
		{
			report.Resolved["EntityCount"] = layout.EntityCountAddress;
		}
		if (layout.MatrixAddress != 0UL)
		{
			report.Resolved["ViewProjectionMatrix"] = layout.MatrixAddress;
		}
		if (layout.LocalPlayerAddress != 0UL)
		{
			report.Resolved["LocalPlayer"] = layout.LocalPlayerAddress;
		}
		if (layout.LocalTeamIdAddress != 0UL)
		{
			report.Resolved["LocalTeamId"] = layout.LocalTeamIdAddress;
		}
		StructLayout60 transform = new StructLayout60
		{
			Name = "Transform",
			Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		};
		StructLayout60 health = new StructLayout60
		{
			Name = "HealthComponent",
			Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		};
		StructLayout60 entity = new StructLayout60
		{
			Name = "Entity",
			Fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		};
		foreach (KeyValuePair<string, long> pair in layout.FieldOffsets)
		{
			string hex = "0x" + pair.Value.ToString("X", CultureInfo.InvariantCulture);
			if (pair.Key.IndexOf("osition", StringComparison.OrdinalIgnoreCase) >= 0 || pair.Key.IndexOf("ositionX", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				transform.Fields[pair.Key] = hex;
			}
			else if (pair.Key.IndexOf("ealth", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				health.Fields[pair.Key] = hex;
			}
			else if (pair.Key.IndexOf("eam", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				entity.Fields[pair.Key] = hex;
			}
		}
		if (transform.Fields.Count > 0)
		{
			report.Structs.Add(transform);
		}
		if (health.Fields.Count > 0)
		{
			report.Structs.Add(health);
		}
		if (entity.Fields.Count > 0)
		{
			report.Structs.Add(entity);
		}
	}

	/// <summary>通用启发式：投影矩阵候选 + TypeInfo 指针 + 指针数组。</summary>
	private static void ScanHeuristics(MemoryReader reader, ulong moduleBase, PeImage image, CalibrationReport60 report)
	{
		PeSection typeinfo = image.FindSection("typeinfo");
		PeSection fieldinf = image.FindSection("fieldinf");
		ulong typeinfoLo = 0UL;
		ulong typeinfoHi = 0UL;
		ulong fieldinfLo = 0UL;
		ulong fieldinfHi = 0UL;
		if (typeinfo != null)
		{
			typeinfoLo = moduleBase + typeinfo.Rva;
			typeinfoHi = typeinfoLo + typeinfo.SizeInMemory;
		}
		if (fieldinf != null)
		{
			fieldinfLo = moduleBase + fieldinf.Rva;
			fieldinfHi = fieldinfLo + fieldinf.SizeInMemory;
		}
		if (typeinfoHi == 0UL && fieldinfHi == 0UL)
		{
			report.Notes.Add("[启发式] 模块内没有 typeinfo / fieldinf 节，跳过指针扫描。");
			return;
		}
		string[] targets = { ".data", ".rdata" };
		foreach (string name in targets)
		{
			PeSection section = image.FindSection(name);
			if (section == null)
			{
				continue;
			}
			byte[] data = ReadSection(reader, moduleBase, section);
			if (data == null)
			{
				report.Notes.Add("[启发式] 节 " + name + " 读取失败，跳过。");
				continue;
			}
			int before = report.ProjectionCandidates.Count;
			ScanProjectionCandidates(data, moduleBase + section.Rva, section, report);
			ConsoleBridge60.WriteLine("  [启发式] " + name + " 投影矩阵候选 +" + (report.ProjectionCandidates.Count - before).ToString(CultureInfo.InvariantCulture));
			int pointerBefore = report.TypeInfoPointers.Count;
			int arrayBefore = report.PointerArrays.Count;
			ScanPointers(data, moduleBase + section.Rva, section, report, typeinfoLo, typeinfoHi, fieldinfLo, fieldinfHi);
			ConsoleBridge60.WriteLine("  [启发式] " + name + " TypeInfo 指针 +" + (report.TypeInfoPointers.Count - pointerBefore).ToString(CultureInfo.InvariantCulture) + "，指针数组 +" + (report.PointerArrays.Count - arrayBefore).ToString(CultureInfo.InvariantCulture));
		}
	}

	private static byte[] ReadSection(MemoryReader reader, ulong moduleBase, PeSection section)
	{
		uint size = section.SizeInMemory;
		if (size == 0U)
		{
			return null;
		}
		byte[] data = new byte[size];
		int done = 0;
		while (done < data.Length)
		{
			int chunk = Math.Min(ChunkSize, data.Length - done);
			if (!reader.ReadBytes(moduleBase + section.Rva + (ulong)done, data, done, chunk))
			{
				return null;
			}
			done += chunk;
		}
		return data;
	}

	/// <summary>
	/// 投影矩阵启发式：|m[10]| ∈ [0.5,3.0]、|m[11]| ∈ [0.5,3.0]、m[14] ≈ ±1、m[15] == 0。
	/// 矩阵按 16 个 float、行优先（m[row*4+col]）解读。
	/// </summary>
	private static void ScanProjectionCandidates(byte[] data, ulong sectionVa, PeSection section, CalibrationReport60 report)
	{
		for (int i = 0; i + 64 <= data.Length; i += 4)
		{
			float m15 = BitConverter.ToSingle(data, i + 60);
			if (!Near(m15, 0f, 0.001f))
			{
				continue;
			}
			float m10 = BitConverter.ToSingle(data, i + 40);
			if (!AbsInRange(m10, 0.5f, 3.0f))
			{
				continue;
			}
			float m11 = BitConverter.ToSingle(data, i + 44);
			if (!AbsInRange(m11, 0.5f, 3.0f))
			{
				continue;
			}
			float m14 = BitConverter.ToSingle(data, i + 56);
			if (!Near(Math.Abs(m14), 1f, 0.1f))
			{
				continue;
			}
			if (report.ProjectionCandidates.Count >= MaxProjectionRecords)
			{
				report.Notes.Add("[启发式] 投影矩阵候选超过 " + MaxProjectionRecords.ToString(CultureInfo.InvariantCulture) + " 条，已截断。");
				return;
			}
			List<float> values = new List<float>(16);
			for (int k = 0; k < 16; k++)
			{
				values.Add(BitConverter.ToSingle(data, i + k * 4));
			}
			report.ProjectionCandidates.Add(new CalibrationProjection60
			{
				Address = (long)(sectionVa + (ulong)i),
				Rva = (long)(section.Rva + (ulong)i),
				Section = section.Name,
				Values = values
			});
		}
	}

	/// <summary>TypeInfo 指针扫描：目标落在 typeinfo / fieldinf 节内的 64 位指针，以及它们的连续数组。</summary>
	private static void ScanPointers(byte[] data, ulong sectionVa, PeSection section, CalibrationReport60 report, ulong typeinfoLo, ulong typeinfoHi, ulong fieldinfLo, ulong fieldinfHi)
	{
		int runStart = -1;
		int runCount = 0;
		for (int i = 0; i + 8 <= data.Length; i += 8)
		{
			ulong value = BitConverter.ToUInt64(data, i);
			bool inRange = ((value >= typeinfoLo) && (value < typeinfoHi)) || ((value >= fieldinfLo) && (value < fieldinfHi));
			if (!inRange)
			{
				if (runCount >= 2 && report.PointerArrays.Count < MaxArrayRecords)
				{
					report.PointerArrays.Add(new CalibrationArray60
					{
						Address = (long)(sectionVa + (ulong)runStart),
						Rva = (long)(section.Rva + (ulong)runStart),
						Section = section.Name,
						Count = runCount
					});
				}
				runStart = -1;
				runCount = 0;
				continue;
			}
			if (runStart < 0)
			{
				runStart = i;
			}
			runCount++;
			if (report.TypeInfoPointers.Count < MaxPointerRecords)
			{
				report.TypeInfoPointers.Add(new CalibrationPointer60
				{
					Address = (long)(sectionVa + (ulong)i),
					Rva = (long)(section.Rva + (ulong)i),
					Section = section.Name,
					Target = (long)value
				});
			}
		}
		if (runCount >= 2 && report.PointerArrays.Count < MaxArrayRecords)
		{
			report.PointerArrays.Add(new CalibrationArray60
			{
				Address = (long)(sectionVa + (ulong)runStart),
				Rva = (long)(section.Rva + (ulong)runStart),
				Section = section.Name,
				Count = runCount
			});
		}
	}

	private static bool Near(float value, float target, float tolerance)
	{
		return Math.Abs(value - target) <= tolerance;
	}

	private static bool AbsInRange(float value, float low, float high)
	{
		float absolute = Math.Abs(value);
		return absolute >= low && absolute <= high;
	}

	private static string ResolveOutputPath(string outputPath)
	{
		if (!string.IsNullOrEmpty(outputPath))
		{
			string full = Path.GetFullPath(outputPath);
			string directory = Path.GetDirectoryName(full);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			return full;
		}
		string profiles = ProfileStore.LocateProfilesDirectory();
		if (!Directory.Exists(profiles))
		{
			Directory.CreateDirectory(profiles);
		}
		string stamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
		return Path.Combine(profiles, "calibration-" + stamp + ".json");
	}

	private static bool Save(CalibrationReport60 report, string path)
	{
		try
		{
			string text = JsonText60.Serialize(report);
			File.WriteAllText(path, text, new UTF8Encoding(false));
			return true;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
		catch (SerializationException)
		{
			return false;
		}
	}
}
