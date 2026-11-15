using System;

namespace CoH3MemoryTool;

internal sealed class ProductionJob
{
	public readonly string Path;

	public readonly string Key;

	public readonly float RemainingTicks;

	public readonly float TotalTicks;

	public int RemainingSeconds => GrenadeUpgrade.SecondsRemaining(RemainingTicks);

	public ProductionJob(string path, float remaining, float total, string key)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException("生产单位名称为空。");
		}
		GrenadeUpgrade.SecondsRemaining(remaining);
		GrenadeUpgrade.SecondsRemaining(total);
		if (total <= 0f || remaining > total)
		{
			throw new ArgumentOutOfRangeException("remaining", "生产进度正在更新。");
		}
		Path = path;
		RemainingTicks = remaining;
		TotalTicks = total;
		Key = key;
	}
}
