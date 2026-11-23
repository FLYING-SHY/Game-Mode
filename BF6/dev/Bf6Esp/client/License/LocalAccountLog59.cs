using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Bf6Esp;

/// <summary>
/// 从 Frostbite 本地日志里取 Steam 账号（"Steam account number is [7656119...]"）。
///
/// 说明：BF6 的日志落盘位置尚未由静态分析确认，这里列出多个候选目录，
/// 命中任意一个即可；找不到时返回 null，由 SteamIdentity60 兜底。
/// </summary>
internal static class LocalAccountLog59
{
	internal const ulong IndividualBase = 76561197960265728UL;

	private const string Marker = "Steam account number is [";

	internal static bool IsIndividual(ulong id)
	{
		return id > IndividualBase && (id - IndividualBase) <= uint.MaxValue;
	}

	/// <summary>候选日志目录。</summary>
	internal static string[] CandidateDirectories()
	{
		string documents = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		return new string[]
		{
			Path.Combine(documents, "Documents", "Battlefield 6"),
			Path.Combine(documents, "Documents", "My Games", "Battlefield 6"),
			Path.Combine(documents, "Saved Games", "Battlefield 6"),
			Path.Combine(documents, "OneDrive", "Documents", "Battlefield 6")
		};
	}

	/// <summary>扫描候选目录中最近修改的日志，取出 Steam 账号。</summary>
	internal static ulong? Read(DateTime processStarted)
	{
		foreach (string directory in CandidateDirectories())
		{
			if (!Directory.Exists(directory))
			{
				continue;
			}
			string[] files;
			try
			{
				files = Directory.GetFiles(directory, "*.log", SearchOption.AllDirectories);
			}
			catch (IOException)
			{
				continue;
			}
			catch (UnauthorizedAccessException)
			{
				continue;
			}
			DateTime newest = DateTime.MinValue;
			string newestFile = "";
			foreach (string file in files)
			{
				try
				{
					DateTime written = File.GetLastWriteTime(file);
					if (written > newest && written >= processStarted.AddMinutes(-5.0))
					{
						newest = written;
						newestFile = file;
					}
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
			if (string.IsNullOrEmpty(newestFile))
			{
				continue;
			}
			ulong? parsed = ReadFile(newestFile);
			if (parsed.HasValue)
			{
				return parsed;
			}
		}
		return null;
	}

	private static ulong? ReadFile(string path)
	{
		try
		{
			string text;
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
			{
				using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true))
				{
					text = reader.ReadToEnd();
				}
			}
			return Parse(text);
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
	}

	/// <summary>从日志文本里解析 Steam 账号。</summary>
	internal static ulong? Parse(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		int index = text.IndexOf(Marker, StringComparison.Ordinal);
		while (index >= 0)
		{
			int start = index + Marker.Length;
			int end = text.IndexOf(']', start);
			if (end < 0)
			{
				return null;
			}
			string digits = text.Substring(start, end - start);
			ulong value;
			if (digits.Length == 17 && ulong.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out value) && IsIndividual(value))
			{
				return value;
			}
			index = text.IndexOf(Marker, end, StringComparison.Ordinal);
		}
		return null;
	}
}
