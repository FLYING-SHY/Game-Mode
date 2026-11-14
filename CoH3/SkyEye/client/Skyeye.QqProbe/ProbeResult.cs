using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skyeye.QqProbe;

internal sealed class ProbeResult
{
	public readonly Dictionary<string, Finding> Accounts = new Dictionary<string, Finding>();

	public readonly List<string> Notes = new List<string>();

	public int RootsChecked;

	public bool Limited;

	public string Summary
	{
		get
		{
			int num = Accounts.Values.Count((Finding a) => a.HasQqData);
			return $"找到 {Accounts.Count} 个号码：{num} 个带 QQ 数据目录，{Accounts.Count - num} 个仅有数字目录。";
		}
	}

	public IEnumerable<Finding> Ordered => Accounts.Values.OrderBy((Finding a) => a.Number.Length).ThenBy((Finding a) => a.Number, StringComparer.Ordinal);

	public string Report()
	{
		StringBuilder stringBuilder = new StringBuilder("QQ 本地账号检测 · 测试版 0.1.0\r\n");
		stringBuilder.AppendLine(Summary);
		stringBuilder.AppendLine("结果仅表示本机目录线索，不证明归属、登录状态或账号完整性。\r\n");
		foreach (Finding item in Ordered)
		{
			stringBuilder.AppendLine(item.Number + " | " + string.Join("、", item.Evidence));
			foreach (string path in item.Paths)
			{
				stringBuilder.AppendLine("  " + path);
			}
		}
		stringBuilder.AppendLine("\r\n检测位置与状态：");
		foreach (string note in Notes)
		{
			stringBuilder.AppendLine(note);
		}
		if (Limited)
		{
			stringBuilder.AppendLine("检测触及数量或时间上限，结果可能不完整。");
		}
		return stringBuilder.ToString();
	}
}
