using System;
using System.Collections.Generic;
using System.IO;

namespace CoH3MemoryTool;

internal static class Adaptation58
{
	public static int Verify()
	{
		List<string> list = new List<string>();
		try
		{
			using (PlayerInfoReader playerInfoReader = new PlayerInfoReader())
			{
				list.AddRange(playerInfoReader.VerifyLive58());
			}
			File.WriteAllLines("adapt58-live-verification.txt", list);
			return 0;
		}
		catch (Exception ex)
		{
			list.Add("FAIL " + ex);
			File.WriteAllLines("adapt58-live-verification.txt", list);
			return 1;
		}
	}
}
