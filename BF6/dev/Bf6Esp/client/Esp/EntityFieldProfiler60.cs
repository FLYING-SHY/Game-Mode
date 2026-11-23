using System;
using System.Collections.Generic;

namespace Bf6Esp;

/// <summary>
/// 实体字段推断结果：在不知道字段名的情况下，靠多实体数值分布反推出的偏移。
/// </summary>
internal sealed class EntityFieldProfile60
{
	/// <summary>位置 x 分量偏移；-1 = 未找到。</summary>
	public int PositionOffsetX = -1;

	/// <summary>位置 y 分量偏移；-1 = 未找到。</summary>
	public int PositionOffsetY = -1;

	/// <summary>位置 z 分量偏移；-1 = 未找到。</summary>
	public int PositionOffsetZ = -1;

	/// <summary>位置起点偏移（等价于 x 分量）。</summary>
	public int PositionOffset
	{
		get { return PositionOffsetX; }
	}

	/// <summary>当前血量偏移（float）；-1 = 未找到。</summary>
	public int HealthOffset = -1;

	/// <summary>
	/// 当前血量所在的组件指针槽（实体侧偏移）；-1 = 血量就内联在实体里。
	/// 非 -1 时：实体地址 + 本偏移处是一个指针，真正的血量在 (指针 + HealthOffset)。
	/// </summary>
	public int HealthComponentOffset = -1;

	/// <summary>最大血量偏移（float）；-1 = 未找到。</summary>
	public int MaxHealthOffset = -1;

	/// <summary>最大血量所在的组件指针槽（实体侧偏移）；-1 = 内联。</summary>
	public int MaxHealthComponentOffset = -1;

	/// <summary>队伍 id 偏移（int）；-1 = 未找到。</summary>
	public int TeamOffset = -1;

	/// <summary>队伍 id 所在的组件指针槽（实体侧偏移）；-1 = 内联。</summary>
	public int TeamComponentOffset = -1;

	/// <summary>姓名字符串指针偏移（8 字节指针）；-1 = 未找到。</summary>
	public int NamePointerOffset = -1;

	/// <summary>姓名字符串指针所在的组件指针槽（实体侧偏移）；-1 = 内联。</summary>
	public int NameComponentOffset = -1;

	/// <summary>位置所在的组件指针槽（实体侧偏移）；-1 = 内联。</summary>
	public int PositionComponentOffset = -1;

	/// <summary>综合可信度（0 = 什么都没找到）。</summary>
	public double Score;

	/// <summary>过程说明。</summary>
	public List<string> Notes = new List<string>();

	/// <summary>扫描基准地址（实体本体或一级间接目标）。</summary>
	public ulong BaseAddress;

	public bool HasPosition
	{
		get { return PositionOffsetX >= 0 && PositionOffsetY >= 0 && PositionOffsetZ >= 0; }
	}

	public bool HasHealth
	{
		get { return HealthOffset >= 0; }
	}
}

/// <summary>
/// 实体字段统计推断器。
///
/// 思路：实体对象是同构的——同一个偏移在所有实体上装的是同一种数据。
/// 因此只要采到足够多的实体，就能靠数值分布把"血量/队伍/位置/姓名"分辨出来：
///
///   * 血量：所有实体取值都在 (0, 1000]，并存在另一个恒定槽位与其同量级（最大血量，通常 100）。
///   * 队伍：int 取值落在 [0, 64]，且不同取值个数很少（1..8）。
///   * 位置：连续三个 float，全部有限、量级合理（|v| &lt; 1e5），且跨实体有变化。
///   * 姓名：8 字节对齐处是已提交区间内的指针，且指向可打印 ASCII。
///
/// 这条路完全不依赖"成员名"，因此在 bf6.exe 这种"有偏移没名字"的构建上依然可用。
/// 全程只读：只有 ReadProcessMemory。
/// </summary>
internal static class EntityFieldProfiler60
{
	/// <summary>每个实体扫描的字节数。</summary>
	internal const int DefaultScanBytes = 0x400;

	/// <summary>最多采样多少个实体。</summary>
	internal const int DefaultMaxEntities = 32;

	/// <summary>血量取值上界（超过就不像血量）。</summary>
	private const float HealthUpperBound = 1000f;

	/// <summary>血量取值下界（比这更小的多半是指针被当成浮点数）。</summary>
	private const float HealthLowerBound = 0.5f;

	/// <summary>恒定的"最大血量"不得小于这个值，否则多半是矩阵基向量里的 1.0。</summary>
	private const float HealthConstantFloor = 10f;

	/// <summary>队伍取值上界。</summary>
	private const int TeamUpperBound = 64;

	/// <summary>位置分量绝对值上界。</summary>
	private const float PositionAbsLimit = 100000f;

	/// <summary>
	/// 对一个实体数组做字段推断。
	/// </summary>
	/// <param name="reader">只读内存读取器。</param>
	/// <param name="arrayAddress">实体指针数组首地址。</param>
	/// <param name="entityCount">数组里的实体个数（会被采样上限截断）。</param>
	/// <param name="map">已提交内存区间表，用于判断指针是否指向有效内存。</param>
	/// <param name="scanBytes">每个实体扫描的字节数。</param>
	/// <param name="maxEntities">最多采样实体数。</param>
	internal static EntityFieldProfile60 Profile(MemoryReader reader, ulong arrayAddress, int entityCount,
		AddressMap60 map, int scanBytes, int maxEntities)
	{
		EntityFieldProfile60 profile = new EntityFieldProfile60();
		if (reader == null || arrayAddress == 0UL || entityCount <= 0)
		{
			profile.Notes.Add("实体数组地址或数量无效。");
			return profile;
		}
		if (scanBytes <= 0)
		{
			scanBytes = DefaultScanBytes;
		}
		if (maxEntities <= 0)
		{
			maxEntities = DefaultMaxEntities;
		}

		// ---- 采样实体指针 -------------------------------------------------
		int probe = Math.Min(entityCount, maxEntities);
		byte[] pointerBuffer = reader.ReadBytes(arrayAddress, probe * 8);
		if (pointerBuffer == null)
		{
			profile.Notes.Add("读取实体指针数组失败。");
			return profile;
		}
		List<ulong> entities = new List<ulong>();
		for (int i = 0; i < probe; i++)
		{
			ulong entity = BitConverter.ToUInt64(pointerBuffer, i * 8);
			if (entity == 0UL)
			{
				continue;
			}
			if (map != null && !map.Contains(entity))
			{
				continue;
			}
			if (!entities.Contains(entity))
			{
				entities.Add(entity);
			}
		}
		profile.Notes.Add("采样实体 " + entities.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) + " 个。");
		if (entities.Count == 0)
		{
			return profile;
		}

		// ---- 扫描三种寻址方式 ------------------------------------------------
		// Frostbite 的实体指针既可能直接指向数据，也可能指向一层 handle，
		// 还可能是一个"组件指针槽"（实体里第 p 个 8 字节槽指向变换/血量组件）。
		// 三种都试一遍，按得分合并。
		EntityFieldProfile60 direct = ProfileBase(reader, entities, null, map, scanBytes);
		EntityFieldProfile60 indirect = ProfileBase(reader, entities, true, map, scanBytes);

		EntityFieldProfile60 best = direct;
		if (indirect != null && (best == null || indirect.Score > best.Score))
		{
			best = indirect;
		}
		if (best == null)
		{
			return profile;
		}
		EntityFieldProfile60 components = ProfileComponents(reader, entities, map, scanBytes);
		if (components != null)
		{
			MergeMissing(best, components);
		}
		best.Notes.AddRange(profile.Notes);
		return best;
	}

	/// <summary>把组件寻址找到、而内联寻址没找到的字段补进结果。</summary>
	private static void MergeMissing(EntityFieldProfile60 target, EntityFieldProfile60 extra)
	{
		if (target.HealthOffset < 0 && extra.HealthOffset >= 0)
		{
			target.HealthOffset = extra.HealthOffset;
			target.HealthComponentOffset = extra.HealthComponentOffset;
			target.Score += 40.0;
			target.Notes.Add("血量取自组件指针 +0x" + extra.HealthComponentOffset.ToString("X", System.Globalization.CultureInfo.InvariantCulture) + "。");
		}
		if (target.MaxHealthOffset < 0 && extra.MaxHealthOffset >= 0)
		{
			target.MaxHealthOffset = extra.MaxHealthOffset;
			target.MaxHealthComponentOffset = extra.MaxHealthComponentOffset;
			target.Score += 10.0;
		}
		if (target.TeamOffset < 0 && extra.TeamOffset >= 0)
		{
			target.TeamOffset = extra.TeamOffset;
			target.TeamComponentOffset = extra.TeamComponentOffset;
			target.Score += 15.0;
			target.Notes.Add("队伍取自组件指针 +0x" + extra.TeamComponentOffset.ToString("X", System.Globalization.CultureInfo.InvariantCulture) + "。");
		}
		if (!target.HasPosition && extra.HasPosition)
		{
			target.PositionOffsetX = extra.PositionOffsetX;
			target.PositionOffsetY = extra.PositionOffsetY;
			target.PositionOffsetZ = extra.PositionOffsetZ;
			target.PositionComponentOffset = extra.PositionComponentOffset;
			target.Score += 30.0;
			target.Notes.Add("位置取自组件指针 +0x" + extra.PositionComponentOffset.ToString("X", System.Globalization.CultureInfo.InvariantCulture) + "。");
		}
		if (target.NamePointerOffset < 0 && extra.NamePointerOffset >= 0)
		{
			target.NamePointerOffset = extra.NamePointerOffset;
			target.NameComponentOffset = extra.NameComponentOffset;
			target.Score += 5.0;
		}
	}

	/// <summary>
	/// 组件指针寻址：遍历实体头部前 0x100 字节里的每个 8 字节槽，
	/// 把"槽里的指针"当成组件对象，逐槽做一次完整的字段分析，
	/// 位置/血量/队伍/姓名各自挑最好的那个槽。
	/// </summary>
	private static EntityFieldProfile60 ProfileComponents(MemoryReader reader, List<ulong> entities, AddressMap60 map, int scanBytes)
	{
		EntityFieldProfile60 result = new EntityFieldProfile60();
		int slots = Math.Min(scanBytes, 0x100) / 8;
		if (slots <= 0)
		{
			return null;
		}

		int healthSlot = -1;
		EntityFieldProfile60 healthSub = null;
		int maxSlot = -1;
		EntityFieldProfile60 maxSub = null;
		int teamSlot = -1;
		EntityFieldProfile60 teamSub = null;
		int positionSlot = -1;
		EntityFieldProfile60 positionSub = null;
		int nameSlot = -1;
		EntityFieldProfile60 nameSub = null;
		double bestHealth = 0.0;
		double bestMax = 0.0;
		double bestTeam = 0.0;
		double bestPosition = 0.0;
		double bestName = 0.0;

		for (int slot = 0; slot < slots; slot++)
		{
			List<ulong> targets = new List<ulong>();
			for (int i = 0; i < entities.Count; i++)
			{
				ulong hop;
				if (!reader.TryRead(entities[i] + (ulong)(slot * 8), out hop) || hop == 0UL)
				{
					continue;
				}
				if (map != null && !map.Contains(hop))
				{
					continue;
				}
				targets.Add(hop);
			}
			// 至少要有一半实体在这个槽上给出有效指针。
			if (targets.Count < 4 || targets.Count * 2 < entities.Count)
			{
				continue;
			}
			EntityFieldProfile60 sub = AnalyzeAt(reader, targets, map, scanBytes);
			if (sub == null)
			{
				continue;
			}
			int byteOffset = slot * 8;

			if (sub.HealthOffset >= 0)
			{
				double score = 20.0;
				if (sub.MaxHealthOffset >= 0)
				{
					score += 10.0;
				}
				if (score > bestHealth)
				{
					bestHealth = score;
					healthSlot = byteOffset;
					healthSub = sub;
				}
			}
			if (sub.MaxHealthOffset >= 0 && sub.MaxHealthOffset != sub.HealthOffset)
			{
				double score = 10.0;
				if (score > bestMax)
				{
					bestMax = score;
					maxSlot = byteOffset;
					maxSub = sub;
				}
			}
			if (sub.TeamOffset >= 0 && 15.0 > bestTeam)
			{
				bestTeam = 15.0;
				teamSlot = byteOffset;
				teamSub = sub;
			}
			if (sub.HasPosition && 30.0 > bestPosition)
			{
				bestPosition = 30.0;
				positionSlot = byteOffset;
				positionSub = sub;
			}
			if (sub.NamePointerOffset >= 0 && 5.0 > bestName)
			{
				bestName = 5.0;
				nameSlot = byteOffset;
				nameSub = sub;
			}
		}

		if (healthSub != null && healthSlot >= 0)
		{
			result.HealthOffset = healthSub.HealthOffset;
			result.HealthComponentOffset = healthSlot;
			if (healthSub.MaxHealthOffset >= 0)
			{
				result.MaxHealthOffset = healthSub.MaxHealthOffset;
				result.MaxHealthComponentOffset = healthSlot;
			}
		}
		if (maxSub != null && maxSlot >= 0 && result.MaxHealthOffset < 0)
		{
			result.MaxHealthOffset = maxSub.MaxHealthOffset;
			result.MaxHealthComponentOffset = maxSlot;
		}
		if (teamSub != null && teamSlot >= 0)
		{
			result.TeamOffset = teamSub.TeamOffset;
			result.TeamComponentOffset = teamSlot;
		}
		if (positionSub != null && positionSlot >= 0)
		{
			result.PositionOffsetX = positionSub.PositionOffsetX;
			result.PositionOffsetY = positionSub.PositionOffsetY;
			result.PositionOffsetZ = positionSub.PositionOffsetZ;
			result.PositionComponentOffset = positionSlot;
		}
		if (nameSub != null && nameSlot >= 0)
		{
			result.NamePointerOffset = nameSub.NamePointerOffset;
			result.NameComponentOffset = nameSlot;
		}
		if (result.HealthOffset < 0 && result.TeamOffset < 0 && !result.HasPosition && result.NamePointerOffset < 0)
		{
			return null;
		}
		return result;
	}

	/// <summary>对一组"数据地址"做字段分析（地址已经解析好）。</summary>
	private static EntityFieldProfile60 AnalyzeAt(MemoryReader reader, List<ulong> dataAddresses, AddressMap60 map, int scanBytes)
	{
		int usable = dataAddresses == null ? 0 : dataAddresses.Count;
		if (usable == 0)
		{
			return null;
		}
		int slotCount = scanBytes / 4;
		float[][] values = new float[usable][];
		int used = 0;
		for (int i = 0; i < usable; i++)
		{
			byte[] blob = reader.ReadBytes(dataAddresses[i], scanBytes);
			if (blob == null || blob.Length < scanBytes)
			{
				continue;
			}
			float[] row = new float[slotCount];
			for (int k = 0; k < slotCount; k++)
			{
				row[k] = BitConverter.ToSingle(blob, k * 4);
			}
			values[used++] = row;
		}
		if (used == 0)
		{
			return null;
		}
		return AnalyzeValues(reader, values, used, slotCount, dataAddresses, map, scanBytes);
	}

	/// <summary>从已读出的数值矩阵里推断字段偏移。</summary>
	private static EntityFieldProfile60 AnalyzeValues(MemoryReader reader, float[][] values, int used, int slotCount,
		List<ulong> dataAddresses, AddressMap60 map, int scanBytes)
	{
		EntityFieldProfile60 profile = new EntityFieldProfile60();

		// ---- 血量 / 最大血量 ------------------------------------------------
		List<int> constantSlots = FindConstantSlots(values, used, slotCount);
		List<int> healthSlots = FindHealthSlots(values, used, slotCount);
		PickHealth(profile, values, used, healthSlots, constantSlots);

		// ---- 队伍 ----------------------------------------------------------
		profile.TeamOffset = FindTeamOffset(values, used, slotCount);

		// ---- 位置 ----------------------------------------------------------
		// 先试 4x4 变换矩阵（判据严格：齐次分量与基向量都要像矩阵），
		// 再退回"连续三个 float"的通用判据——矩阵布局下连续三元组很容易误判。
		int matrixX;
		int matrixY;
		int matrixZ;
		if (FindPositionInMatrix(values, used, slotCount, profile.TeamOffset, profile.HealthOffset,
			out matrixX, out matrixY, out matrixZ) >= 0)
		{
			profile.PositionOffsetX = matrixX;
			profile.PositionOffsetY = matrixY;
			profile.PositionOffsetZ = matrixZ;
			profile.Notes.Add("位置取自 4x4 变换矩阵的平移分量。");
		}
		else
		{
			int contiguous = FindPositionOffset(values, used, slotCount, profile.TeamOffset, profile.HealthOffset);
			if (contiguous >= 0)
			{
				profile.PositionOffsetX = contiguous;
				profile.PositionOffsetY = contiguous + 4;
				profile.PositionOffsetZ = contiguous + 8;
			}
		}

		// ---- 姓名指针 -------------------------------------------------------
		profile.NamePointerOffset = FindNamePointerOffset(reader, dataAddresses, used, map, scanBytes);

		// ---- 综合评分 -------------------------------------------------------
		profile.Score = 0.0;
		if (profile.HasHealth)
		{
			profile.Score += 40.0;
		}
		if (profile.HasPosition)
		{
			profile.Score += 30.0;
		}
		if (profile.TeamOffset >= 0)
		{
			profile.Score += 15.0;
		}
		if (profile.MaxHealthOffset >= 0)
		{
			profile.Score += 10.0;
		}
		if (profile.NamePointerOffset >= 0)
		{
			profile.Score += 5.0;
		}
		return profile;
	}

	/// <summary>
	/// 以某种寻址方式扫描所有实体，求该方式下的字段偏移。
	/// </summary>
	/// <param name="reader">只读内存读取器。</param>
	/// <param name="entities">实体指针列表。</param>
	/// <param name="indirect">true = 先读实体首指针再取数据；false = 直接读实体本体。</param>
	/// <param name="map">已提交内存区间表。</param>
	/// <param name="scanBytes">每个实体扫描的字节数。</param>
	private static EntityFieldProfile60 ProfileBase(MemoryReader reader, List<ulong> entities, bool? indirect, AddressMap60 map, int scanBytes)
	{
		EntityFieldProfile60 profile = new EntityFieldProfile60();
		// 先算每个实体的数据地址。
		List<ulong> dataAddresses = new List<ulong>();
		foreach (ulong entity in entities)
		{
			ulong target = entity;
			if (indirect.HasValue && indirect.Value)
			{
				ulong hop;
				if (!reader.TryRead(entity, out hop) || hop == 0UL)
				{
					continue;
				}
				target = hop;
			}
			dataAddresses.Add(target);
		}
		int usable = dataAddresses.Count;
		if (usable == 0)
		{
			return profile;
		}
		EntityFieldProfile60 analyzed = AnalyzeAt(reader, dataAddresses, map, scanBytes);
		if (analyzed == null)
		{
			return profile;
		}
		analyzed.BaseAddress = dataAddresses[0];
		analyzed.Notes.Add("寻址方式：" + (indirect.HasValue && indirect.Value ? "一级间接" : "直接") +
			"，基准 0x" + analyzed.BaseAddress.ToString("X", System.Globalization.CultureInfo.InvariantCulture) + "。");
		return analyzed;
	}

	/// <summary>找出在所有实体上取值恒定的偏移（典型是最大血量 = 100）。</summary>
	private static List<int> FindConstantSlots(float[][] values, int usable, int slotCount)
	{
		List<int> result = new List<int>();
		for (int slot = 0; slot < slotCount; slot++)
		{
			float first = values[0][slot];
			if (float.IsNaN(first) || float.IsInfinity(first))
			{
				continue;
			}
			bool constant = true;
			for (int i = 1; i < usable; i++)
			{
				float v = values[i][slot];
				if (float.IsNaN(v) || v != first)
				{
					constant = false;
					break;
				}
			}
			if (constant)
			{
				result.Add(slot);
			}
		}
		return result;
	}

	/// <summary>找出"像血量"的偏移：全部实体取值都在 [0.5, 1000]。</summary>
	private static List<int> FindHealthSlots(float[][] values, int usable, int slotCount)
	{
		List<int> result = new List<int>();
		for (int slot = 0; slot < slotCount; slot++)
		{
			bool ok = true;
			for (int i = 0; i < usable; i++)
			{
				float v = values[i][slot];
				// 下界排除"指针被当成浮点数"（denormal 量级 1e-41）。
				if (float.IsNaN(v) || float.IsInfinity(v) || v < HealthLowerBound || v > HealthUpperBound)
				{
					ok = false;
					break;
				}
			}
			if (ok)
			{
				result.Add(slot);
			}
		}
		return result;
	}

	/// <summary>
	/// 在候选槽位里挑出"当前血量"与"最大血量"：
	/// 血量槽必须在 (0,1000]；最大血量槽在所有实体上恒定，且与血量同量级。
	/// </summary>
	private static void PickHealth(EntityFieldProfile60 profile, float[][] values, int usable,
		List<int> healthSlots, List<int> constantSlots)
	{
		if (healthSlots.Count == 0)
		{
			return;
		}
		int bestHealth = -1;
		int bestMax = -1;
		double bestScore = double.MinValue;

		foreach (int healthSlot in healthSlots)
		{
			// 当前血量在所有实体上的取值集合。
			float healthValue = values[0][healthSlot];
			bool allEqualHealth = true;
			for (int i = 1; i < usable; i++)
			{
				if (values[i][healthSlot] != healthValue)
				{
					allEqualHealth = false;
					break;
				}
			}
			foreach (int constantSlot in constantSlots)
			{
				float constantValue = values[0][constantSlot];
				// 下界 10：4x4 变换矩阵的基向量恒为 1.0，若不排除会被当成"最大血量"。
				if (constantValue < HealthConstantFloor || constantValue > HealthUpperBound)
				{
					continue;
				}
				// 同一个槽位当血量和最大血量没有意义（无法区分谁是谁）。
				if (constantSlot == healthSlot)
				{
					continue;
				}
				// 当前血量不得超过最大血量。
				bool within = true;
				for (int i = 0; i < usable; i++)
				{
					if (values[i][healthSlot] > constantValue + 0.001f)
					{
						within = false;
						break;
					}
				}
				if (!within)
				{
					continue;
				}
				double score = 0.0;
				if (allEqualHealth)
				{
					// 满血状态下 current == max，是强信号。
					if (Math.Abs(healthValue - constantValue) < 0.001f)
					{
						// 但"两个恒定槽相等"也可能只是两个矩阵基向量（如 1.0 与 1.0），
						// 权重必须低于"血量在变、另一个槽恒定"这种真血量特征。
						score += 25.0;
					}
					else
					{
						// 血量恒定却不等于最大值——不是当前血量。
						continue;
					}
				}
				else
				{
					score += 20.0;
				}
				// 血量值越接近常见上限（100）越可信。
				score += Math.Min(constantValue, 100f) / 100.0 * 5.0;
				// 相邻布局（float current; float max;）是 Frostbite 的常见排布。
				if (constantSlot == healthSlot + 1)
				{
					score += 8.0;
				}
				else if (constantSlot == healthSlot + 2)
				{
					score += 4.0;
				}
				else if (healthSlot == constantSlot + 1)
				{
					score += 2.0;
				}
				if (score > bestScore)
				{
					bestScore = score;
					bestHealth = healthSlot * 4;
					bestMax = constantSlot * 4;
				}
			}
		}

		// 没有配到恒定最大值槽时，退化成"取众数占比高的血量候选"。
		// 判据：多数实体取同一个值（真实血量里大部分人满血），
		// 且这个值不小于 HealthConstantFloor —— 否则多半是指针/自增 id 之类的槽。
		if (bestHealth < 0)
		{
			int dominantOffset;
			if (TryPickDominantHealth(values, usable, healthSlots, out dominantOffset))
			{
				bestHealth = dominantOffset;
				profile.Notes.Add("未找到与血量同量级的恒定槽，按众数占比推定当前血量。");
			}
			else
			{
				profile.Notes.Add("未找到可信的血量槽（众数占比不足或取值过小）。");
				return;
			}
		}
		profile.HealthOffset = bestHealth;
		profile.MaxHealthOffset = bestMax;
	}

	/// <summary>
	/// 兜底选血量槽：要求该槽在多数实体上取同一个值（众数占比 ≥ 60%），
	/// 且众数值不小于 HealthConstantFloor。
	/// </summary>
	private static bool TryPickDominantHealth(float[][] values, int usable, List<int> healthSlots, out int offset)
	{
		offset = -1;
		if (healthSlots == null || healthSlots.Count == 0 || usable <= 0)
		{
			return false;
		}
		double bestRatio = 0.0;
		foreach (int slot in healthSlots)
		{
			HashSet<float> seen = new HashSet<float>();
			int bestCount = 0;
			float bestValue = 0f;
			for (int i = 0; i < usable; i++)
			{
				float v = values[i][slot];
				if (!seen.Add(v))
				{
					continue;
				}
				int count = 0;
				for (int k = 0; k < usable; k++)
				{
					if (values[k][slot] == v)
					{
						count++;
					}
				}
				if (count > bestCount)
				{
					bestCount = count;
					bestValue = v;
				}
			}
			if (bestCount * 5 < usable * 3 || bestValue < HealthConstantFloor)
			{
				continue;
			}
			double ratio = (double)bestCount / usable;
			if (ratio > bestRatio)
			{
				bestRatio = ratio;
				offset = slot * 4;
			}
		}
		return offset >= 0;
	}

	/// <summary>找出队伍 id 偏移：int 取值小、种类少。</summary>
	private static int FindTeamOffset(float[][] values, int usable, int slotCount)
	{
		int best = -1;
		int bestDistinct = int.MaxValue;
		for (int slot = 0; slot < slotCount; slot++)
		{
			HashSet<int> distinct = new HashSet<int>();
			bool ok = true;
			for (int i = 0; i < usable; i++)
			{
				int v = BitConverter.ToInt32(BitConverter.GetBytes(values[i][slot]), 0);
				if (v < 0 || v > TeamUpperBound)
				{
					ok = false;
					break;
				}
				distinct.Add(v);
			}
			if (!ok || distinct.Count == 0 || distinct.Count > 8)
			{
				continue;
			}
			// 全零的槽位更像填充，不当队伍；队伍全队一致且非零也可以接受。
			if (distinct.Count == 1 && distinct.Contains(0))
			{
				continue;
			}
			if (distinct.Count < bestDistinct)
			{
				bestDistinct = distinct.Count;
				best = slot * 4;
			}
		}
		return best;
	}

	/// <summary>
	/// 找出位置三元组：连续三个 float，全部有限且量级合理，跨实体有变化。
	/// </summary>
	private static int FindPositionOffset(float[][] values, int usable, int slotCount, int teamOffset, int healthOffset)
	{
		int teamSlot = teamOffset >= 0 ? teamOffset / 4 : -1;
		int healthSlot = healthOffset >= 0 ? healthOffset / 4 : -1;
		int best = -1;
		double bestScore = double.MinValue;
		for (int slot = 0; slot + 2 < slotCount; slot++)
		{
			if (slot == teamSlot || slot == healthSlot)
			{
				continue;
			}
			double score = 0.0;
			bool ok = true;
			int varying = 0;
			for (int axis = 0; axis < 3; axis++)
			{
				float min = float.MaxValue;
				float max = float.MinValue;
				for (int i = 0; i < usable; i++)
				{
					float v = values[i][slot + axis];
					if (float.IsNaN(v) || float.IsInfinity(v) || Math.Abs(v) > PositionAbsLimit)
					{
						ok = false;
						break;
					}
					if (v < min)
					{
						min = v;
					}
					if (v > max)
					{
						max = v;
					}
				}
				if (!ok)
				{
					break;
				}
				float span = max - min;
				if (span > 0.01f)
				{
					varying++;
					score += Math.Min(span, 10000.0);
				}
			}
			if (!ok || varying < 2)
			{
				continue;
			}
			// 位置分量至少有两个在变化才算位置；数值跨度越大越可信。
			score = score / 1000.0 + varying;
			if (score > bestScore)
			{
				bestScore = score;
				best = slot * 4;
			}
		}
		return best;
	}

	/// <summary>
	/// 位置不在连续三个 float 里，而是被塞进了 4x4 变换矩阵的平移分量：
	///   * 行主序：平移在第 4 行（索引 12,13,14），基向量在 (0,1,2)/(4,5,6)/(8,9,10)。
	///   * 列主序：平移在第 4 列（索引 3,7,11），基向量在 (0,4,8)/(1,5,9)/(2,6,10)。
	/// 返回 x 分量偏移（-1 表示没找到），并通过 out 参数给出 y/z 分量。
	/// </summary>
	private static int FindPositionInMatrix(float[][] values, int usable, int slotCount, int teamOffset, int healthOffset,
		out int offsetX, out int offsetY, out int offsetZ)
	{
		offsetX = -1;
		offsetY = -1;
		offsetZ = -1;
		int best = -1;
		double bestScore = double.MinValue;

		for (int baseSlot = 0; baseSlot + 15 < slotCount; baseSlot += 4)
		{
			// 只有"整块确实像变换矩阵"时才采信，避免把任意 float 数组误判成矩阵。
			if (!LooksLikeTransform(values, usable, baseSlot))
			{
				continue;
			}

			// 行主序：平移 [12,13,14]（连续三个 float）。
			double rowScore = ScoreTranslation(values, usable, baseSlot + 12, 1);
			if (rowScore > 0.0)
			{
				if (rowScore > bestScore)
				{
					bestScore = rowScore;
					best = (baseSlot + 12) * 4;
					offsetX = (baseSlot + 12) * 4;
					offsetY = (baseSlot + 13) * 4;
					offsetZ = (baseSlot + 14) * 4;
				}
			}

			// 列主序：平移 [3,7,11]（每隔一个 float）。
			double columnScore = ScoreTranslation(values, usable, baseSlot + 3, 4);
			if (columnScore > 0.0)
			{
				if (columnScore > bestScore)
				{
					bestScore = columnScore;
					best = (baseSlot + 3) * 4;
					offsetX = (baseSlot + 3) * 4;
					offsetY = (baseSlot + 7) * 4;
					offsetZ = (baseSlot + 11) * 4;
				}
			}
		}
		return best;
	}

	/// <summary>
	/// 给一组候选平移分量打分：三个分量必须都有限、量级合理，且跨实体至少两轴有变化。
	/// 返回 0 表示不是位置。
	/// </summary>
	private static double ScoreTranslation(float[][] values, int usable, int startSlot, int stride)
	{
		if (startSlot < 0 || startSlot + 2 * stride >= 4096)
		{
			return 0.0;
		}
		double score = 0.0;
		int varying = 0;
		for (int axis = 0; axis < 3; axis++)
		{
			int slot = startSlot + axis * stride;
			float min = float.MaxValue;
			float max = float.MinValue;
			for (int i = 0; i < usable; i++)
			{
				if (slot >= values[i].Length)
				{
					return 0.0;
				}
				float v = values[i][slot];
				if (float.IsNaN(v) || float.IsInfinity(v) || Math.Abs(v) > PositionAbsLimit)
				{
					return 0.0;
				}
				if (v < min)
				{
					min = v;
				}
				if (v > max)
				{
					max = v;
				}
			}
			float span = max - min;
			if (span > 0.01f)
			{
				varying++;
				score += Math.Min(span, 10000.0);
			}
		}
		if (varying < 2)
		{
			return 0.0;
		}
		return score / 1000.0 + varying;
	}

	/// <summary>
	/// 判断这 16 个 float 像不像一个变换矩阵：全部有限、齐次分量约为 1、基向量有界。
	/// </summary>
	private static bool LooksLikeTransform(float[][] values, int usable, int baseSlot)
	{
		for (int i = 0; i < usable; i++)
		{
			if (baseSlot + 15 >= values[i].Length)
			{
				return false;
			}
			float w = values[i][baseSlot + 15];
			if (float.IsNaN(w) || float.IsInfinity(w) || Math.Abs(w - 1.0f) > 0.5f)
			{
				return false;
			}
			for (int k = 0; k < 16; k++)
			{
				float v = values[i][baseSlot + k];
				if (float.IsNaN(v) || float.IsInfinity(v))
				{
					return false;
				}
			}
			// 基向量元素有界（旋转+缩放不会离谱）。
			float m00 = values[i][baseSlot + 0];
			float m11 = values[i][baseSlot + 5];
			float m22 = values[i][baseSlot + 10];
			if (Math.Abs(m00) > 1000f || Math.Abs(m11) > 1000f || Math.Abs(m22) > 1000f)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>找出姓名字符串指针偏移：8 字节对齐处是指向可打印 ASCII 的有效指针。</summary>
	private static int FindNamePointerOffset(MemoryReader reader, List<ulong> entities, int usable, AddressMap60 map, int scanBytes)
	{
		int best = -1;
		int bestHits = 0;
		for (int offset = 0; offset + 8 <= scanBytes; offset += 8)
		{
			int hits = 0;
			for (int i = 0; i < usable; i++)
			{
				ulong target;
				if (!reader.TryRead(entities[i] + (ulong)offset, out target) || target == 0UL)
				{
					continue;
				}
				if (map != null && !map.Contains(target))
				{
					continue;
				}
				byte[] blob = reader.ReadBytes(target, 32);
				if (blob == null)
				{
					continue;
				}
				int length = 0;
				while (length < blob.Length && blob[length] != 0)
				{
					if (blob[length] < 0x20 || blob[length] > 0x7E)
					{
						break;
					}
					length++;
				}
				if (length >= 2)
				{
					hits++;
				}
			}
			if (hits > bestHits)
			{
				bestHits = hits;
				best = offset;
			}
		}
		// 至少要有一半实体能读出名字才算数。
		if (best >= 0 && bestHits * 2 < usable)
		{
			return -1;
		}
		return best;
	}
}
