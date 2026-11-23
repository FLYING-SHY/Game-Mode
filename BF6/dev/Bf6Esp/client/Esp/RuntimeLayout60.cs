using System;
using System.Collections.Generic;

namespace Bf6Esp;

/// <summary>运行时布局：定位器在目标进程里找到的所有可用信息。</summary>
internal sealed class RuntimeLayout60
{
	/// <summary>实体指针数组的首地址（元素为 8 字节指针）。</summary>
	public ulong EntityListAddress;

	/// <summary>实体数量的存放地址（int）。</summary>
	public ulong EntityCountAddress;

	/// <summary>实体数量。</summary>
	public int EntityCount;

	/// <summary>视图投影矩阵地址（16 个 float）。</summary>
	public ulong MatrixAddress;

	/// <summary>true 表示矩阵按"列向量"约定解释。</summary>
	public bool UseColumnVector;

	/// <summary>本地玩家实体地址（0 = 未定位）。</summary>
	public ulong LocalPlayerAddress;

	/// <summary>本地队伍 id 的存放地址（0 = 未定位）。</summary>
	public ulong LocalTeamIdAddress;

	/// <summary>本地队伍 id（-1 = 未知）。</summary>
	public int LocalTeamId = -1;

	/// <summary>字段名 -> 相对实体首地址的偏移。</summary>
	public Dictionary<string, long> FieldOffsets = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// 字段名 -> 组件指针槽偏移（相对实体首地址）。
	/// 出现在这里的字段说明：实体地址 + 该偏移处是一个指针，真正的数据在 (指针 + FieldOffsets[字段])。
	/// 没出现的字段就是直接内联在实体里的。
	/// </summary>
	public Dictionary<string, long> FieldPointerOffsets = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

	/// <summary>过程说明（写进校准报告）。</summary>
	public List<string> Notes = new List<string>();

	public bool HasEntities
	{
		get { return EntityListAddress != 0UL && EntityCount > 0; }
	}

	public bool HasMatrix
	{
		get { return MatrixAddress != 0UL; }
	}

	/// <summary>按候选别名查找字段偏移；找不到返回 -1。</summary>
	public long FindOffset(params string[] aliases)
	{
		foreach (string alias in aliases)
		{
			long value;
			if (FieldOffsets.TryGetValue(alias, out value))
			{
				return value;
			}
		}
		return -1L;
	}

	/// <summary>按候选别名查找组件指针槽偏移；没有则返回 -1（表示字段内联在实体里）。</summary>
	public long FindPointerOffset(params string[] aliases)
	{
		if (FieldPointerOffsets == null)
		{
			return -1L;
		}
		foreach (string alias in aliases)
		{
			long value;
			if (FieldPointerOffsets.TryGetValue(alias, out value))
			{
				return value;
			}
		}
		return -1L;
	}
}

/// <summary>已提交内存区间的快速查询表。</summary>
internal sealed class AddressMap60
{
	private readonly ulong[] starts;

	private readonly ulong[] ends;

	public AddressMap60(List<ulong[]> regions)
	{
		List<ulong[]> sorted = new List<ulong[]>();
		if (regions != null)
		{
			sorted.AddRange(regions);
		}
		sorted.Sort(delegate(ulong[] a, ulong[] b)
		{
			return a[0].CompareTo(b[0]);
		});
		starts = new ulong[sorted.Count];
		ends = new ulong[sorted.Count];
		for (int i = 0; i < sorted.Count; i++)
		{
			starts[i] = sorted[i][0];
			ends[i] = sorted[i][1];
		}
	}

	public bool Contains(ulong address)
	{
		int low = 0;
		int high = starts.Length - 1;
		while (low <= high)
		{
			int mid = (low + high) / 2;
			if (address < starts[mid])
			{
				high = mid - 1;
			}
			else if (address >= ends[mid])
			{
				low = mid + 1;
			}
			else
			{
				return true;
			}
		}
		return false;
	}
}
