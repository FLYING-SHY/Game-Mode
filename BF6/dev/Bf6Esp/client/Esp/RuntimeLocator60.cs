using System;
using System.Collections.Generic;
using System.Drawing;

namespace Bf6Esp;

/// <summary>实体数组候选：模块里 {指针, 数量, 容量} 这种形态的运行时容器。</summary>
internal sealed class EntityArrayCandidate60
{
	/// <summary>容器结构体本身的地址（数量字段在 +0x08）。</summary>
	public ulong ContainerAddress;

	/// <summary>实体指针数组首地址。</summary>
	public ulong DataAddress;

	public int Count;

	public int Capacity;

	/// <summary>前若干个元素里指向已提交内存的比例有效个数。</summary>
	public int ValidElements;

	/// <summary>用血量字段校验时命中的元素个数。</summary>
	public int HealthMatches;

	public double Score;
}

/// <summary>投影矩阵候选。</summary>
internal sealed class MatrixCandidate60
{
	public ulong Address;

	public float[] Values;

	/// <summary>与视口宽高比的一致程度（越小越好）。</summary>
	public double AspectError;
}

/// <summary>
/// 运行时定位器：在目标进程里把 ESP 需要的东西都找出来。
///
/// 三条线索互相独立、互为验证：
///   1) Frostbite 反射系统 —— 明文字段名 + u16 字段偏移（RuntimeReflection60）
///   2) 实体容器 —— 模块 .data 里 {指针, 数量, 容量} 的连续指针数组
///   3) 投影矩阵 —— 内存里的 float[16]，用视口宽高比与实体投影落点率筛选
///
/// 全部只读：只用 ReadProcessMemory / VirtualQueryEx。
/// </summary>
internal static class RuntimeLocator60
{
	private const int ChunkBytes = 0x100000;

	private const long MaxHeapScanBytes = 384L * 1024L * 1024L;

	private const int MaxEntityArrayCandidates = 64;

	private const int MaxMatrixCandidates = 128;

	/// <summary>调试追踪钩子（自检时打开）。</summary>
	internal static Action<string> Trace;

	private const int MaxProbeElements = 64;

	private static readonly string[] PositionAliases = { "positionX", "positionY", "positionZ", "Position", "worldPosition", "transformPosition" };

	private static readonly string[] HealthAliases = { "CurrentHealth", "health", "Health" };

	private static readonly string[] MaxHealthAliases = { "MaxHealth", "maxHealth", "MaxHealthValue" };

	private static readonly string[] TeamAliases = { "CurrentTeamId", "TeamId", "teamId", "OwningTeam", "CurrentTeam" };

	/// <summary>定位入口；返回的布局里每一项都可能为"未知"，由调用方决定是否使用。</summary>
	internal static RuntimeLayout60 Locate(MemoryReader reader, GameProcess game, OffsetProfile profile, Action<string> log)
	{
		return Locate(reader, game, profile, log, MaxHeapScanBytes);
	}

	/// <summary>定位入口（可指定堆扫描预算；自检用小预算，避免扫描自身大堆）。</summary>
	internal static RuntimeLayout60 Locate(MemoryReader reader, GameProcess game, OffsetProfile profile, Action<string> log, long heapBudget)
	{
		RuntimeLayout60 layout = new RuntimeLayout60();
		if (reader == null || game == null)
		{
			layout.Notes.Add("没有可用的进程句柄。");
			return layout;
		}
		Action<string> write = log ?? delegate(string message)
		{
		};
		ulong moduleBase = game.ModuleBase;
		PeImage image = PeImage.FromMemory(reader, moduleBase);
		if (image == null)
		{
			layout.Notes.Add("无法解析模块 PE 头。");
			return layout;
		}
		List<ulong[]> regions = ProcessRegions60.Enumerate(game.Handle);
		AddressMap60 map = new AddressMap60(regions);
		layout.Notes.Add("已提交可读区间 " + regions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " 个。");

		// 1) 反射字段偏移 -------------------------------------------------
		System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
		ReflectionScanResult60 reflect = RuntimeReflection60.Scan(reader, moduleBase, image, regions);
		write("[定位] 反射扫描完成，用时 " + watch.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ms。");
		foreach (KeyValuePair<string, ReflectionField60> pair in reflect.Best)
		{
			layout.FieldOffsets[pair.Key] = (long)pair.Value.Offset;
		}
		layout.Notes.Add("反射还原：类型 " + reflect.Types.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			" 个，字段名 " + reflect.Best.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " 个。");

		// 2) 实体容器候选 -------------------------------------------------
		watch = System.Diagnostics.Stopwatch.StartNew();
		List<EntityArrayCandidate60> arrays = FindEntityArrays(reader, moduleBase, image, map);
		write("[定位] 实体容器扫描完成，用时 " + watch.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ms。");
		layout.Notes.Add("实体容器候选 " + arrays.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " 个。");
		EntityArrayCandidate60 bestArray = SelectEntityArray(reader, arrays, layout, map);
		if (bestArray != null)
		{
			layout.EntityListAddress = bestArray.DataAddress;
			layout.EntityCountAddress = bestArray.ContainerAddress + 8UL;
			layout.EntityCount = bestArray.Count;
			layout.Notes.Add("实体容器：数组 0x" + bestArray.DataAddress.ToString("X", System.Globalization.CultureInfo.InvariantCulture) +
				"，数量 " + bestArray.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) +
				"，有效元素 " + bestArray.ValidElements.ToString(System.Globalization.CultureInfo.InvariantCulture) +
				"，血量命中 " + bestArray.HealthMatches.ToString(System.Globalization.CultureInfo.InvariantCulture) + "。");
		}

		// 2.5) 字段统计推断 ------------------------------------------------
		// bf6.exe 里"有成员偏移、没有成员名"，所以按名取偏移在活进程里也可能落空。
		// 这里补一条完全不看名字的路：实体对象同构，靠多实体数值分布反推字段偏移。
		EntityFieldProfile60 entityProfile = null;
		if (bestArray != null)
		{
			watch = System.Diagnostics.Stopwatch.StartNew();
			entityProfile = EntityFieldProfiler60.Profile(reader, bestArray.DataAddress, bestArray.Count, map,
				EntityFieldProfiler60.DefaultScanBytes, EntityFieldProfiler60.DefaultMaxEntities);
			write("[定位] 字段推断完成，用时 " + watch.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ms。");
			MergeProfile(layout, entityProfile);
		}

		// 3) 投影矩阵候选 -------------------------------------------------
		watch = System.Diagnostics.Stopwatch.StartNew();
		List<MatrixCandidate60> matrices = FindMatrixCandidates(reader, moduleBase, image, regions, heapBudget);
		write("[定位] 矩阵扫描完成，用时 " + watch.ElapsedMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ms。");
		layout.Notes.Add("投影矩阵候选 " + matrices.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " 个。");
		MatrixCandidate60 bestMatrix = SelectMatrix(reader, matrices, layout, bestArray);
		if (bestMatrix != null)
		{
			layout.MatrixAddress = bestMatrix.Address;
			layout.UseColumnVector = bestMatrix.AspectError < 0.0;
			layout.Notes.Add("投影矩阵：0x" + bestMatrix.Address.ToString("X", System.Globalization.CultureInfo.InvariantCulture) +
				"，宽高比误差 " + bestMatrix.AspectError.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + "。");
		}

		// 4) 本地玩家 / 本地队伍 --------------------------------------------
		LocateLocalPlayer(reader, layout, bestArray);
		write("[定位] 实体数组=" + (layout.HasEntities ? "有" : "无") + "，矩阵=" + (layout.HasMatrix ? "有" : "无") +
			"，字段=" + layout.FieldOffsets.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
		return layout;
	}

	// ------------------------------------------------------------------
	// 实体容器
	// ------------------------------------------------------------------
	private static List<EntityArrayCandidate60> FindEntityArrays(MemoryReader reader, ulong moduleBase, PeImage image, AddressMap60 map)
	{
		List<EntityArrayCandidate60> result = new List<EntityArrayCandidate60>();
		List<PeSection> sections = new List<PeSection>();
		PeSection data = image.FindSection(".data");
		if (data != null)
		{
			sections.Add(data);
		}
		foreach (PeSection section in sections)
		{
			int size = (int)Math.Min(section.SizeInMemory, (uint)0x1000000);
			if (size <= 0)
			{
				continue;
			}
			byte[] buffer = new byte[size];
			int done = 0;
			while (done < size)
			{
				int chunk = Math.Min(ChunkBytes, size - done);
				if (!reader.ReadBytes(moduleBase + section.Rva + (ulong)done, buffer, done, chunk))
				{
					break;
				}
				done += chunk;
			}
			for (int i = 0; i + 16 <= done; i += 8)
			{
				ulong pointer = BitConverter.ToUInt64(buffer, i);
				uint count = BitConverter.ToUInt32(buffer, i + 8);
				uint capacity = BitConverter.ToUInt32(buffer, i + 12);
				if (pointer == 0UL || (pointer & 7UL) != 0UL)
				{
					continue;
				}
				if (count < 1U || count > 8192U || capacity < count || capacity > 1048576U)
				{
					continue;
				}
				if (!map.Contains(pointer))
				{
					continue;
				}
				int valid = CountValidElements(reader, pointer, (int)count, map);
				if (valid < 4)
				{
					continue;
				}
				EntityArrayCandidate60 candidate = new EntityArrayCandidate60
				{
					ContainerAddress = moduleBase + section.Rva + (ulong)i,
					DataAddress = pointer,
					Count = (int)count,
					Capacity = (int)capacity,
					ValidElements = valid,
					Score = (double)valid / Math.Max(1, count)
				};
				result.Add(candidate);
			}
		}
		result.Sort(delegate(EntityArrayCandidate60 a, EntityArrayCandidate60 b)
		{
			return b.Score.CompareTo(a.Score);
		});
		if (result.Count > MaxEntityArrayCandidates)
		{
			result.RemoveRange(MaxEntityArrayCandidates, result.Count - MaxEntityArrayCandidates);
		}
		return result;
	}

	private static int CountValidElements(MemoryReader reader, ulong arrayAddress, int count, AddressMap60 map)
	{
		int probe = Math.Min(count, MaxProbeElements);
		byte[] buffer = reader.ReadBytes(arrayAddress, probe * 8);
		if (buffer == null)
		{
			return 0;
		}
		int valid = 0;
		for (int i = 0; i < probe; i++)
		{
			ulong element = BitConverter.ToUInt64(buffer, i * 8);
			if (element != 0UL && map.Contains(element))
			{
				valid++;
			}
		}
		return valid;
	}

	/// <summary>用反射出来的血量字段给实体容器打分；反射没命中时改用字段统计推断打分。</summary>
	private static EntityArrayCandidate60 SelectEntityArray(MemoryReader reader, List<EntityArrayCandidate60> candidates, RuntimeLayout60 layout, AddressMap60 map)
	{
		EntityArrayCandidate60 best = null;
		long healthOffset = layout.FindOffset(HealthAliases);
		long maxHealthOffset = layout.FindOffset(MaxHealthAliases);
		foreach (EntityArrayCandidate60 candidate in candidates)
		{
			candidate.HealthMatches = 0;
			if (healthOffset >= 0L)
			{
				int probe = Math.Min(candidate.Count, MaxProbeElements);
				byte[] buffer = reader.ReadBytes(candidate.DataAddress, probe * 8);
				if (buffer != null)
				{
					for (int i = 0; i < probe; i++)
					{
						ulong element = BitConverter.ToUInt64(buffer, i * 8);
						if (element == 0UL)
						{
							continue;
						}
						float health;
						if (!reader.TryRead(element + (ulong)healthOffset, out health))
						{
							continue;
						}
						if (health <= 0f || health > 100000f)
						{
							continue;
						}
						if (maxHealthOffset >= 0L)
						{
							float max;
							if (!reader.TryRead(element + (ulong)maxHealthOffset, out max))
							{
								continue;
							}
							if (max <= 0f || max < health || max > 100000f)
							{
								continue;
							}
						}
						candidate.HealthMatches++;
					}
				}
			}
			double score = (double)candidate.HealthMatches * 100.0 + candidate.Score * 10.0 + Math.Min(candidate.Count, 512) / 512.0;
			candidate.Score = score;
			if (best == null || candidate.Score > best.Score)
			{
				best = candidate;
			}
		}

		// 反射没给出血量偏移时，用字段统计推断给候选容器打分：
		// 真正的实体数组，其元素在同一个偏移上一定有一致的血量/位置语义。
		if (healthOffset < 0L && candidates.Count > 0)
		{
			foreach (EntityArrayCandidate60 candidate in candidates)
			{
				EntityFieldProfile60 probe = EntityFieldProfiler60.Profile(reader, candidate.DataAddress, candidate.Count, map,
					0x200, 16);
				candidate.Score += probe.Score * 5.0;
			}
			best = null;
			foreach (EntityArrayCandidate60 candidate in candidates)
			{
				if (best == null || candidate.Score > best.Score)
				{
					best = candidate;
				}
			}
		}
		return best;
	}

	/// <summary>
	/// 把统计推断出的偏移并进布局；反射已经给出的项优先，不覆盖。
	/// </summary>
	private static void MergeProfile(RuntimeLayout60 layout, EntityFieldProfile60 profile)
	{
		if (layout == null || profile == null)
		{
			return;
		}
		if (profile.HasPosition)
		{
			SetIfAbsent(layout, "positionX", profile.PositionOffsetX);
			SetIfAbsent(layout, "positionY", profile.PositionOffsetY);
			SetIfAbsent(layout, "positionZ", profile.PositionOffsetZ);
			if (profile.PositionComponentOffset >= 0)
			{
				SetPointerIfAbsent(layout, "positionX", profile.PositionComponentOffset);
				SetPointerIfAbsent(layout, "positionY", profile.PositionComponentOffset);
				SetPointerIfAbsent(layout, "positionZ", profile.PositionComponentOffset);
			}
		}
		if (profile.HealthOffset >= 0)
		{
			SetIfAbsent(layout, "CurrentHealth", profile.HealthOffset);
			if (profile.HealthComponentOffset >= 0)
			{
				SetPointerIfAbsent(layout, "CurrentHealth", profile.HealthComponentOffset);
			}
		}
		if (profile.MaxHealthOffset >= 0)
		{
			SetIfAbsent(layout, "MaxHealth", profile.MaxHealthOffset);
			if (profile.MaxHealthComponentOffset >= 0)
			{
				SetPointerIfAbsent(layout, "MaxHealth", profile.MaxHealthComponentOffset);
			}
		}
		if (profile.TeamOffset >= 0)
		{
			SetIfAbsent(layout, "CurrentTeamId", profile.TeamOffset);
			if (profile.TeamComponentOffset >= 0)
			{
				SetPointerIfAbsent(layout, "CurrentTeamId", profile.TeamComponentOffset);
			}
		}
		if (profile.NamePointerOffset >= 0)
		{
			SetIfAbsent(layout, "PlayerNamePointer", profile.NamePointerOffset);
			if (profile.NameComponentOffset >= 0)
			{
				SetPointerIfAbsent(layout, "PlayerNamePointer", profile.NameComponentOffset);
			}
		}
		layout.Notes.Add("字段推断：位置 x/y/z " +
			profile.PositionOffsetX.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" +
			profile.PositionOffsetY.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/" +
			profile.PositionOffsetZ.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			"，血量 " + profile.HealthOffset.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			"，最大血量 " + profile.MaxHealthOffset.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			"，队伍 " + profile.TeamOffset.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			"，姓名指针 " + profile.NamePointerOffset.ToString(System.Globalization.CultureInfo.InvariantCulture) +
			"，评分 " + profile.Score.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "。");
		foreach (string note in profile.Notes)
		{
			layout.Notes.Add("[推断] " + note);
		}
	}

	private static void SetIfAbsent(RuntimeLayout60 layout, string key, long value)
	{
		if (string.IsNullOrEmpty(key) || value < 0L)
		{
			return;
		}
		if (!layout.FieldOffsets.ContainsKey(key))
		{
			layout.FieldOffsets[key] = value;
		}
	}

	/// <summary>记录"该字段住在组件指针后面"的指针槽偏移。</summary>
	private static void SetPointerIfAbsent(RuntimeLayout60 layout, string key, long value)
	{
		if (string.IsNullOrEmpty(key) || value < 0L)
		{
			return;
		}
		if (!layout.FieldPointerOffsets.ContainsKey(key))
		{
			layout.FieldPointerOffsets[key] = value;
		}
	}

	// ------------------------------------------------------------------
	// 投影矩阵
	// ------------------------------------------------------------------
	private static List<MatrixCandidate60> FindMatrixCandidates(MemoryReader reader, ulong moduleBase, PeImage image, List<ulong[]> regions, long heapBudget)
	{
		List<MatrixCandidate60> result = new List<MatrixCandidate60>();
		List<ulong[]> targets = new List<ulong[]>();
		string[] names = { ".data", ".rdata", "ctr", ".rodata" };
		foreach (string name in names)
		{
			PeSection section = image.FindSection(name);
			if (section == null)
			{
				continue;
			}
			targets.Add(new ulong[] { moduleBase + section.Rva, moduleBase + section.Rva + section.SizeInMemory });
		}
		long heapBudgetLeft = heapBudget > 0L ? heapBudget : 0L;
		foreach (ulong[] region in regions)
		{
			if (heapBudgetLeft <= 0L)
			{
				break;
			}
			ulong size = region[1] - region[0];
			if (size < 0x10000UL)
			{
				continue;
			}
			if (size > (ulong)heapBudgetLeft)
			{
				size = (ulong)heapBudgetLeft;
			}
			targets.Add(new ulong[] { region[0], region[0] + size });
			heapBudgetLeft -= (long)size;
		}
		foreach (ulong[] target in targets)
		{
			ScanRegionForMatrices(reader, target[0], target[1] - target[0], result);
		}
		return result;
	}

	private static void ScanRegionForMatrices(MemoryReader reader, ulong start, ulong length, List<MatrixCandidate60> result)
	{
		if (length < 64UL)
		{
			return;
		}
		if (Trace != null)
		{
			Trace("[矩阵] 区域 0x" + start.ToString("X", System.Globalization.CultureInfo.InvariantCulture) +
				"，长度 " + length.ToString(System.Globalization.CultureInfo.InvariantCulture));
		}
		ulong done = 0UL;
		const int Overlap = 64;
		while (done < length)
		{
			int chunk = (int)Math.Min((ulong)ChunkBytes, length - done);
			byte[] buffer = reader.ReadBytes(start + done, chunk);
			if (buffer == null)
			{
				break;
			}
			int limit = buffer.Length - 64;
			for (int i = 0; i <= limit; i += 4)
			{
				float m15 = BitConverter.ToSingle(buffer, i + 60);
				if (m15 > 0.0001f || m15 < -0.0001f)
				{
					continue;
				}
				float m0 = BitConverter.ToSingle(buffer, i);
				float m5 = BitConverter.ToSingle(buffer, i + 20);
				if (!IsScale(m0) || !IsScale(m5))
				{
					continue;
				}
				float m10 = BitConverter.ToSingle(buffer, i + 40);
				float m11 = BitConverter.ToSingle(buffer, i + 44);
				float m14 = BitConverter.ToSingle(buffer, i + 56);
				if (!LooksPerspective(m10, m11, m14))
				{
					continue;
				}
				float[] values = new float[16];
				for (int k = 0; k < 16; k++)
				{
					values[k] = BitConverter.ToSingle(buffer, i + (k * 4));
				}
				result.Add(new MatrixCandidate60
				{
					Address = start + done + (ulong)i,
					Values = values,
					AspectError = 0.0
				});
				if (result.Count >= MaxMatrixCandidates * 8)
				{
					return;
				}
			}
			// 步进必须保证严格前进：末块可能只有 Overlap 大小，此时直接消费整块。
			ulong step = (ulong)chunk > (ulong)Overlap ? (ulong)chunk - (ulong)Overlap : (ulong)chunk;
			done += step;
			if (done + (ulong)Overlap > length)
			{
				break;
			}
		}
	}

	private static bool IsScale(float value)
	{
		if (float.IsNaN(value) || float.IsInfinity(value))
		{
			return false;
		}
		float magnitude = Math.Abs(value);
		return magnitude > 0.02f && magnitude < 50f;
	}

	private static bool LooksPerspective(float m10, float m11, float m14)
	{
		return Math.Abs(m11 - 1f) < 0.2f || Math.Abs(m11 + 1f) < 0.2f
			|| Math.Abs(m10 - 1f) < 0.2f || Math.Abs(m10 + 1f) < 0.2f
			|| Math.Abs(m14 - 1f) < 0.2f || Math.Abs(m14 + 1f) < 0.2f;
	}

	/// <summary>用视口宽高比 + 实体投影落点率挑出最可信的矩阵。</summary>
	private static MatrixCandidate60 SelectMatrix(MemoryReader reader, List<MatrixCandidate60> candidates, RuntimeLayout60 layout, EntityArrayCandidate60 array)
	{
		if (candidates.Count == 0)
		{
			return null;
		}
		int width = 1920;
		int height = 1080;
		Rectangle rect;
		if (OverlayWindow.TryGetForegroundRect(0, out rect) && rect.Width > 0 && rect.Height > 0)
		{
			width = rect.Width;
			height = rect.Height;
		}
		double aspect = (double)width / Math.Max(1, height);
		List<float[]> positions = new List<float[]>();
		if (array != null)
		{
			positions = CollectPositions(reader, array, layout);
		}
		MatrixCandidate60 best = null;
		double bestScore = double.MinValue;
		foreach (MatrixCandidate60 candidate in candidates)
		{
			float ratio = Math.Abs(candidate.Values[5]) / Math.Max(0.0001f, Math.Abs(candidate.Values[0]));
			double errorA = Math.Abs(Math.Log(Math.Max(0.0001, ratio) / aspect));
			double inverse = Math.Abs(Math.Log(Math.Max(0.0001, 1.0 / ratio) / aspect));
			candidate.AspectError = Math.Min(errorA, inverse);
			double score = -candidate.AspectError * 10.0;
			if (positions.Count > 0)
			{
				int hits = 0;
				foreach (float[] position in positions)
				{
					ScreenPoint60 point;
					if (WorldToScreen.Project(candidate.Values, position[0], position[1], position[2], width, height, out point))
					{
						if (point.X >= -width && point.X <= width * 2 && point.Y >= -height && point.Y <= height * 2)
						{
							hits++;
						}
					}
				}
				score += (double)hits / positions.Count * 100.0;
			}
			if (score > bestScore)
			{
				bestScore = score;
				best = candidate;
			}
		}
		return best;
	}

	private static List<float[]> CollectPositions(MemoryReader reader, EntityArrayCandidate60 array, RuntimeLayout60 layout)
	{
		List<float[]> result = new List<float[]>();
		long offsetX = layout.FindOffset("positionX");
		long offsetY = layout.FindOffset("positionY");
		long offsetZ = layout.FindOffset("positionZ");
		long vectorOffset = layout.FindOffset("Position", "worldPosition", "transformPosition");
		if (offsetX < 0L || offsetY < 0L || offsetZ < 0L)
		{
			if (vectorOffset < 0L)
			{
				return result;
			}
			offsetX = vectorOffset;
			offsetY = vectorOffset + 4;
			offsetZ = vectorOffset + 8;
		}
		int probe = Math.Min(array.Count, MaxProbeElements);
		byte[] buffer = reader.ReadBytes(array.DataAddress, probe * 8);
		if (buffer == null)
		{
			return result;
		}
		for (int i = 0; i < probe; i++)
		{
			ulong element = BitConverter.ToUInt64(buffer, i * 8);
			if (element == 0UL)
			{
				continue;
			}
			float x;
			float y;
			float z;
			if (!reader.TryRead(element + (ulong)offsetX, out x) || !reader.TryRead(element + (ulong)offsetY, out y) || !reader.TryRead(element + (ulong)offsetZ, out z))
			{
				continue;
			}
			if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
			{
				continue;
			}
			if (Math.Abs(x) > 100000f || Math.Abs(y) > 100000f || Math.Abs(z) > 100000f)
			{
				continue;
			}
			result.Add(new float[] { x, y, z });
		}
		return result;
	}

	// ------------------------------------------------------------------
	// 本地玩家
	// ------------------------------------------------------------------
	private static void LocateLocalPlayer(MemoryReader reader, RuntimeLayout60 layout, EntityArrayCandidate60 array)
	{
		if (array == null)
		{
			return;
		}
		long teamOffset = layout.FindOffset(TeamAliases);
		int probe = Math.Min(array.Count, MaxProbeElements);
		byte[] buffer = reader.ReadBytes(array.DataAddress, probe * 8);
		if (buffer == null)
		{
			return;
		}
		// 没有队伍字段时无法判断本地玩家，留给上层按"最近实体"处理。
		if (teamOffset < 0L)
		{
			layout.Notes.Add("未找到队伍字段偏移，无法判定本地玩家。");
			return;
		}
		ulong first = 0UL;
		int firstTeam = int.MinValue;
		for (int i = 0; i < probe; i++)
		{
			ulong element = BitConverter.ToUInt64(buffer, i * 8);
			if (element == 0UL)
			{
				continue;
			}
			int team;
			if (!reader.TryRead(element + (ulong)teamOffset, out team))
			{
				continue;
			}
			if (first == 0UL)
			{
				first = element;
				firstTeam = team;
			}
		}
		if (first != 0UL)
		{
			layout.LocalPlayerAddress = first;
			layout.LocalTeamId = firstTeam;
			layout.Notes.Add("本地玩家候选 0x" + first.ToString("X", System.Globalization.CultureInfo.InvariantCulture) +
				"，队伍 " + firstTeam.ToString(System.Globalization.CultureInfo.InvariantCulture) + "（启发式，可能不准）。");
		}
	}
}
