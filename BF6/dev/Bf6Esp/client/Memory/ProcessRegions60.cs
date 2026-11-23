using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Bf6Esp;

/// <summary>
/// 只读地枚举目标进程的可读内存区间（VirtualQueryEx）。
/// 用途：把"反射名字字符串所在的堆区间"也纳入解析器的名字白名单，
/// 否则只扫模块映像会漏掉引擎在堆上分配的类型名。
/// 不写入、不修改任何内存。
/// </summary>
internal static class ProcessRegions60
{
	private const uint MemCommit = 0x1000;

	private const int MaxRegions = 8192;

	private const ulong MaxTotalBytes = 512UL * 1024UL * 1024UL;

	private const ulong MinRegionSize = 0x10000UL;

	/// <summary>
	/// 枚举可读区间；返回的每一项是 [起始, 结束) 地址对。
	/// </summary>
	internal static List<ulong[]> Enumerate(ProcessHandle handle)
	{
		List<ulong[]> regions = new List<ulong[]>();
		if (handle == null || handle.IsInvalid)
		{
			return regions;
		}
		ulong address = 0UL;
		ulong limit = 0x00007FFFFFFFFFFFUL;
		ulong total = 0UL;
		NativeMemory.MemoryBasicInformation info;
		while (address < limit && regions.Count < MaxRegions && total < MaxTotalBytes)
		{
			IntPtr returned = NativeMemory.VirtualQueryEx(handle.DangerousGetHandle(), new IntPtr((long)address), out info, new IntPtr((long)MarshalSize()));
			if (returned == IntPtr.Zero)
			{
				break;
			}
			ulong baseAddress = (ulong)(long)info.BaseAddress.ToInt64();
			ulong size = (ulong)(long)info.RegionSize.ToInt64();
			if (size == 0UL)
			{
				break;
			}
			bool readable = info.State == MemCommit && (info.Protect & 0x04U) != 0U;
			if (readable && size >= MinRegionSize)
			{
				regions.Add(new ulong[] { baseAddress, baseAddress + size });
				total += size;
			}
			ulong next = baseAddress + size;
			if (next <= address)
			{
				break;
			}
			address = next;
		}
		return regions;
	}

	private static int MarshalSize()
	{
		return Marshal.SizeOf(typeof(NativeMemory.MemoryBasicInformation));
	}
}
