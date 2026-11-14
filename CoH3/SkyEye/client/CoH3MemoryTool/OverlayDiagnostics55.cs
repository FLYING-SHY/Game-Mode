using System;
using System.IO;

namespace CoH3MemoryTool;

internal static class OverlayDiagnostics55
{
	private static readonly object gate = new object();

	public static void Record(string state, string detail)
	{
		try
		{
			lock (gate)
			{
				string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "overlay-diagnostics.log");
				if (File.Exists(text) && new FileInfo(text).Length > 262144)
				{
					File.WriteAllText(text, "");
				}
				File.AppendAllText(text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + state + " " + detail + Environment.NewLine);
			}
		}
		catch
		{
		}
	}
}
