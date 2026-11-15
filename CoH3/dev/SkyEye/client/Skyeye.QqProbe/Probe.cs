using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;

namespace Skyeye.QqProbe;

internal static class Probe
{
	internal static bool NumberLike(string value)
	{
		if (string.IsNullOrEmpty(value) || value.Length < 5 || value.Length > 12 || value[0] == '0')
		{
			return false;
		}
		foreach (char c in value)
		{
			if (c < '0' || c > '9')
			{
				return false;
			}
		}
		return true;
	}

	internal static string ConfiguredPath(byte[] bytes)
	{
		if (bytes.Length > 65536)
		{
			throw new InvalidDataException();
		}
		Encoding[] array = new Encoding[4]
		{
			new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
			new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true),
			new UnicodeEncoding(bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true),
			Encoding.GetEncoding(54936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)
		};
		Encoding[] array2 = array;
		foreach (Encoding encoding in array2)
		{
			string text;
			try
			{
				text = encoding.GetString(bytes).TrimStart('\ufeff');
			}
			catch (DecoderFallbackException)
			{
				continue;
			}
			if (text.IndexOf('\0') >= 0)
			{
				continue;
			}
			bool flag = false;
			string[] array3 = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text2 in array3)
			{
				string text3 = text2.Trim();
				if (text3.StartsWith(";") || text3.StartsWith("#"))
				{
					continue;
				}
				if (text3.StartsWith("["))
				{
					flag = text3.Equals("[UserDataSet]", StringComparison.OrdinalIgnoreCase);
					continue;
				}
				int num = text3.IndexOf('=');
				if (flag && num >= 0 && text3.Substring(0, num).Trim().Equals("UserDataSavePath", StringComparison.OrdinalIgnoreCase))
				{
					string text4 = text3.Substring(num + 1).Trim();
					if (text4.Length >= 2 && ((text4[0] == '"' && text4[text4.Length - 1] == '"') || (text4[0] == '\'' && text4[text4.Length - 1] == '\'')))
					{
						text4 = text4.Substring(1, text4.Length - 2);
					}
					return Environment.ExpandEnvironmentVariables(text4);
				}
			}
		}
		return "";
	}

	private static string ReadConfig(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		byte[] array = new byte[65537];
		int i;
		int num;
		for (i = 0; i < array.Length; i += num)
		{
			if ((num = fileStream.Read(array, i, array.Length - i)) <= 0)
			{
				break;
			}
		}
		if (i > 65536)
		{
			throw new InvalidDataException();
		}
		Array.Resize(ref array, i);
		return ConfiguredPath(array);
	}

	private static bool LocalPath(string path)
	{
		if (string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) || path.StartsWith("\\\\"))
		{
			return false;
		}
		return new DriveInfo(Path.GetPathRoot(path)).DriveType != DriveType.Network;
	}

	internal static ProbeResult Scan(IEnumerable<string> roots, IEnumerable<string> configs)
	{
		ProbeResult probeResult = new ProbeResult();
		List<string> list = new List<string>(roots);
		foreach (string config in configs)
		{
			try
			{
				if (!File.Exists(config))
				{
					probeResult.Notes.Add("配置不存在：" + config);
					continue;
				}
				string text = ReadConfig(config);
				if (string.IsNullOrWhiteSpace(text))
				{
					probeResult.Notes.Add("配置未提供有效数据位置：" + config);
					continue;
				}
				list.Add(text);
				probeResult.Notes.Add("已读取数据位置配置：" + config);
			}
			catch (Exception e)
			{
				probeResult.Notes.Add("配置未读取（" + Error(e) + "）：" + config);
			}
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Stopwatch stopwatch = Stopwatch.StartNew();
		foreach (string item in list)
		{
			if (stopwatch.Elapsed > TimeSpan.FromSeconds(15.0))
			{
				probeResult.Limited = true;
				break;
			}
			try
			{
				if (!LocalPath(item))
				{
					probeResult.Notes.Add("跳过非本机绝对路径：" + item);
					continue;
				}
				string fullPath = Path.GetFullPath(item);
				if (fullPath.TrimEnd(Path.DirectorySeparatorChar).Equals(Path.GetPathRoot(fullPath).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
				{
					probeResult.Notes.Add("请选择 QQ 数据目录，不选择整个磁盘：" + item);
					continue;
				}
				string text2 = fullPath.TrimEnd(Path.DirectorySeparatorChar);
				if (!hashSet.Add(text2))
				{
					continue;
				}
				probeResult.RootsChecked++;
				if (!Directory.Exists(text2))
				{
					probeResult.Notes.Add("数据目录不存在或不可访问：" + text2);
					continue;
				}
				if ((File.GetAttributes(text2) & FileAttributes.ReparsePoint) != 0)
				{
					probeResult.Notes.Add("跳过目录链接，请选择其实际位置：" + text2);
					continue;
				}
				string fileName = Path.GetFileName(text2);
				if (NumberLike(fileName))
				{
					Inspect(text2, fileName, probeResult);
					probeResult.Notes.Add("已检查所选账号目录：" + text2);
					continue;
				}
				int num = 0;
				int count = probeResult.Accounts.Count;
				foreach (string item2 in Directory.EnumerateDirectories(text2, "*", SearchOption.TopDirectoryOnly))
				{
					if (++num > 4096 || stopwatch.Elapsed > TimeSpan.FromSeconds(15.0))
					{
						probeResult.Limited = true;
						break;
					}
					string fileName2 = Path.GetFileName(item2);
					if (NumberLike(fileName2))
					{
						try
						{
							Inspect(item2, fileName2, probeResult);
						}
						catch (Exception e2)
						{
							probeResult.Notes.Add("账号目录跳过（" + Error(e2) + "）：" + item2);
						}
					}
				}
				probeResult.Notes.Add("已检查：" + text2 + "；新增号码 " + (probeResult.Accounts.Count - count));
			}
			catch (Exception e3)
			{
				probeResult.Notes.Add("数据位置未检查（" + Error(e3) + "）：" + item);
			}
		}
		return probeResult;
	}

	private static void Inspect(string path, string number, ProbeResult result)
	{
		if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
		{
			result.Notes.Add("跳过账号目录链接：" + path);
			return;
		}
		bool flag = Marker(Path.Combine(path, "nt_qq"), directory: true);
		bool flag2 = Marker(Path.Combine(path, "Msg3.0.db"), directory: false) || Marker(Path.Combine(path, "Msg2.0.db"), directory: false);
		if (!result.Accounts.TryGetValue(number, out var value))
		{
			Finding finding = new Finding();
			finding.Number = number;
			value = finding;
			result.Accounts.Add(number, value);
		}
		value.Paths.Add(path);
		value.HasQqData |= flag || flag2;
		value.Evidence.Add(flag ? "QQNT 目录" : (flag2 ? "旧版 QQ 目录" : "数字目录（待核对）"));
	}

	private static bool Marker(string path, bool directory)
	{
		try
		{
			FileAttributes attributes = File.GetAttributes(path);
			return (attributes & FileAttributes.ReparsePoint) == 0 && (attributes & FileAttributes.Directory) != 0 == directory;
		}
		catch (IOException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	private static string Error(Exception e)
	{
		if (e is UnauthorizedAccessException || e is SecurityException)
		{
			return "没有读取权限";
		}
		if (e is InvalidDataException)
		{
			return "配置过大或损坏";
		}
		if (e is IOException)
		{
			return "文件正被占用或位置不可用";
		}
		return "路径或格式不支持";
	}

	internal static ProbeResult CurrentUser(string custom)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
		if (!string.IsNullOrEmpty(folderPath))
		{
			list.Add(Path.Combine(folderPath, "Tencent Files"));
		}
		if (!string.IsNullOrEmpty(folderPath2))
		{
			list2.Add(Path.Combine(folderPath2, "Tencent", "QQ", "UserDataInfo.ini"));
		}
		if (!string.IsNullOrWhiteSpace(custom))
		{
			list.Add(custom);
		}
		return Scan(list, list2);
	}
}
