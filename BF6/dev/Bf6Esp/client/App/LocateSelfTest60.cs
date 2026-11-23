using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bf6Esp;

/// <summary>
/// 定位器自检（--selftest-locate）。
///
/// 用自身进程跑一遍完整的运行时定位流程（区间枚举 -> 反射扫描 -> 实体容器候选 ->
/// 投影矩阵候选），验证这条代码路径在真实内存上不会崩、能读出结构化结果。
/// 它不证明游戏偏移正确，只证明定位器可用。
/// </summary>
internal static class LocateSelfTest60
{
	internal static int Run()
	{
		ConsoleBridge60.Attach();
		ConsoleBridge60.WriteLine("=== Bf6Esp 运行时定位器自检 ===");
		int failures = 0;
		GameProcess self = GameProcess.OpenSelf();
		if (self == null || self.Handle == null)
		{
			ConsoleBridge60.WriteLine("[失败] 无法以只读方式打开自身进程。");
			return 1;
		}
		using (self)
		{
			MemoryReader reader = new MemoryReader(self.Handle, true);
			ConsoleBridge60.WriteLine("[信息] 自身主模块基址=0x" + self.ModuleBase.ToString("X", CultureInfo.InvariantCulture));
			PeImage image = PeImage.FromMemory(reader, self.ModuleBase);
			if (image == null)
			{
				ConsoleBridge60.WriteLine("[失败] PE 头解析失败。");
				failures++;
			}
			else
			{
				ConsoleBridge60.WriteLine("[成功] 节表解析：" + image.SectionCount.ToString(CultureInfo.InvariantCulture) + " 节。");
			}
			List<ulong[]> regions = ProcessRegions60.Enumerate(self.Handle);
			if (regions.Count <= 0)
			{
				ConsoleBridge60.WriteLine("[失败] VirtualQueryEx 没有枚举到任何已提交区间。");
				failures++;
			}
			else
			{
				ConsoleBridge60.WriteLine("[成功] 已提交可读区间 " + regions.Count.ToString(CultureInfo.InvariantCulture) + " 个。");
			}
			try
			{
				// 堆扫描预算压到 16MB：自检只需要验证代码路径，不需要扫完整个堆。
				RuntimeLocator60.Trace = delegate(string message)
				{
					ConsoleBridge60.WriteLine("  " + message);
				};
				RuntimeLayout60 layout = RuntimeLocator60.Locate(reader, self, null, delegate(string message)
				{
					ConsoleBridge60.WriteLine("  [定位] " + message);
				}, 16L * 1024L * 1024L);
				foreach (string note in layout.Notes)
				{
					ConsoleBridge60.WriteLine("  [定位] " + note);
				}
				ConsoleBridge60.WriteLine("[成功] 定位流程完成：字段 " + layout.FieldOffsets.Count.ToString(CultureInfo.InvariantCulture) +
					" 个，实体数组=" + (layout.HasEntities ? "有" : "无") + "，矩阵=" + (layout.HasMatrix ? "有" : "无") + "。");
			}
			catch (Exception ex)
			{
				ConsoleBridge60.WriteLine("[失败] 定位流程异常：" + ex.Message);
				failures++;
			}
		}
		ConsoleBridge60.WriteLine(failures == 0 ? "=== 定位器自检通过 ===" : ("=== 定位器自检失败项：" + failures + " ==="));
		return failures == 0 ? 0 : 1;
	}
}
