using System;
using System.Linq;

namespace CoH3MemoryTool;

internal static class ProductionText
{
	public static string Summary(PlayerInfoRow row)
	{
		if (!row.ProductionKnown || row.Producing.Count == 0)
		{
			return "";
		}
		return "正在造：" + string.Join(" · ", from x in row.Producing.OrderBy((ProductionJob x) => UnitNames.Name(x.Path)).ThenBy((ProductionJob x) => x.Key, StringComparer.Ordinal)
			select UnitNames.Name(x.Path) + "（" + ((x.RemainingSeconds == 0) ? "待出场" : (x.RemainingSeconds + "秒")) + "）");
	}
}
