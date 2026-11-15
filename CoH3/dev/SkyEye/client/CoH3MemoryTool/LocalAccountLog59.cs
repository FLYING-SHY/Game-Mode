using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace CoH3MemoryTool;

internal static class LocalAccountLog59
{
	internal const ulong IndividualBase = 76561197960265728uL;

	internal static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games\\Company of Heroes 3\\warnings.log");

	internal static bool IsIndividual(ulong id)
	{
		if (id > 76561197960265728L)
		{
			return id - 76561197960265728L <= uint.MaxValue;
		}
		return false;
	}

	internal static ulong? Parse(string head, DateTime processStarted, DateTime fileWritten)
	{
		if (head == null || head.Length > 65536 || fileWritten < processStarted)
		{
			return null;
		}
		using StringReader stringReader = new StringReader(head);
		string text = stringReader.ReadLine();
		if (text == null || !text.StartsWith("RelicCoH3 started at ", StringComparison.Ordinal) || text.Length < "RelicCoH3 started at ".Length + 16 || !DateTime.TryParseExact(text.Substring("RelicCoH3 started at ".Length, 16), "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			return null;
		}
		if (result < processStarted.AddMinutes(-1.0) || result > processStarted.AddSeconds(5.0))
		{
			return null;
		}
		bool flag = false;
		ulong? num = null;
		string text2;
		while ((text2 = stringReader.ReadLine()) != null)
		{
			if (text2.EndsWith(": Steam appid is [1677280]", StringComparison.Ordinal))
			{
				flag = true;
			}
			int num2 = text2.IndexOf(": Steam account number is [", StringComparison.Ordinal);
			if (num2 >= 0)
			{
				string text3 = text2.Substring(num2 + ": Steam account number is [".Length);
				if (!text3.EndsWith("]", StringComparison.Ordinal))
				{
					return null;
				}
				text3 = text3.Substring(0, text3.Length - 1);
				if (text3.Length != 17 || !ulong.TryParse(text3, NumberStyles.None, CultureInfo.InvariantCulture, out var result2) || !IsIndividual(result2) || (num.HasValue && num.Value != result2))
				{
					return null;
				}
				num = result2;
			}
		}
		return flag ? num : ((ulong?)null);
	}

	internal static ulong? Read(string path, DateTime started)
	{
		try
		{
			DateTime lastWriteTime = File.GetLastWriteTime(path);
			if (lastWriteTime < started)
			{
				return null;
			}
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			int num = (int)Math.Min(fileStream.Length, 65536L);
			byte[] array = new byte[num];
			int i;
			int num2;
			for (i = 0; i < num; i += num2)
			{
				if ((num2 = fileStream.Read(array, i, num - i)) <= 0)
				{
					break;
				}
			}
			string text = Encoding.UTF8.GetString(array, 0, i).TrimStart('\ufeff');
			if (fileStream.Length > i)
			{
				int num3 = text.LastIndexOf('\n');
				if (num3 < 0)
				{
					return null;
				}
				text = text.Substring(0, num3 + 1);
			}
			if (File.GetLastWriteTime(path) < started)
			{
				return null;
			}
			return Parse(text, started, lastWriteTime);
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
}
