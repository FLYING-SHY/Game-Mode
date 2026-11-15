using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace CoH3MemoryTool;

internal static class SteamAccounts71
{
	private sealed class Parser
	{
		private readonly string text;

		private int at;

		private int tokens;

		private bool quoted;

		private bool Brace(string value, string brace)
		{
			if (!quoted)
			{
				return value == brace;
			}
			return false;
		}

		public Parser(string value)
		{
			text = value ?? "";
			if (text.Length > 2097152)
			{
				throw new FormatException();
			}
		}

		private string Token()
		{
			quoted = false;
			if (++tokens > 200000)
			{
				throw new FormatException();
			}
			while (at < text.Length)
			{
				if (char.IsWhiteSpace(text[at]) || text[at] == '\ufeff')
				{
					at++;
					continue;
				}
				if (text[at] != '/' || at + 1 >= text.Length || text[at + 1] != '/')
				{
					break;
				}
				while (at < text.Length && text[at] != '\n')
				{
					at++;
				}
			}
			if (at == text.Length)
			{
				return null;
			}
			char c = text[at++];
			if (c == '{' || c == '}')
			{
				return c.ToString();
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (c == '"')
			{
				quoted = true;
				bool flag = false;
				while (at < text.Length)
				{
					c = text[at++];
					switch (c)
					{
					case '"':
						break;
					case '\\':
						if (at < text.Length)
						{
							char c2 = text[at++];
							switch (c2)
							{
							case 'n':
								c = '\n';
								break;
							case 'r':
								c = '\r';
								break;
							case 't':
								c = '\t';
								break;
							case '"':
							case '\\':
								c = c2;
								break;
							default:
								stringBuilder.Append('\\');
								c = c2;
								break;
							}
						}
						goto IL_01f4;
					default:
						goto IL_01f4;
					}
					flag = true;
					break;
					IL_01f4:
					stringBuilder.Append(c);
					if (stringBuilder.Length > 4096)
					{
						throw new FormatException();
					}
				}
				if (!flag)
				{
					throw new FormatException();
				}
			}
			else
			{
				stringBuilder.Append(c);
				while (at < text.Length && !char.IsWhiteSpace(text[at]) && text[at] != '{' && text[at] != '}')
				{
					stringBuilder.Append(text[at++]);
					if (stringBuilder.Length > 4096)
					{
						throw new FormatException();
					}
				}
			}
			return stringBuilder.ToString();
		}

		private void Skip(int depth)
		{
			if (depth > 32)
			{
				throw new FormatException();
			}
			string text;
			while (!Brace(text = Token(), "}"))
			{
				if (text == null || Brace(text, "{"))
				{
					throw new FormatException();
				}
				string text2 = Token();
				if (Brace(text2, "{"))
				{
					Skip(depth + 1);
				}
				else if (text2 == null || Brace(text2, "}"))
				{
					throw new FormatException();
				}
			}
		}

		private string Account()
		{
			string result = "";
			string text;
			while (!Brace(text = Token(), "}"))
			{
				if (text == null || Brace(text, "{"))
				{
					throw new FormatException();
				}
				string text2 = Token();
				if (Brace(text2, "{"))
				{
					Skip(1);
					continue;
				}
				if (text2 == null || Brace(text2, "}"))
				{
					throw new FormatException();
				}
				if (string.Equals(text, "PersonaName", StringComparison.OrdinalIgnoreCase))
				{
					result = CleanName(text2);
				}
			}
			return result;
		}

		public List<Dictionary<string, object>> Read()
		{
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>(StringComparer.Ordinal);
			string text;
			while ((text = Token()) != null)
			{
				if (Brace(text, "{") || Brace(text, "}"))
				{
					throw new FormatException();
				}
				string text2 = Token();
				if (text2 == null || Brace(text2, "}"))
				{
					throw new FormatException();
				}
				if (!Brace(text2, "{"))
				{
					continue;
				}
				if (!string.Equals(text, "users", StringComparison.OrdinalIgnoreCase))
				{
					Skip(1);
					continue;
				}
				while (!Brace(text = Token(), "}"))
				{
					if (text == null || Brace(text, "{"))
					{
						throw new FormatException();
					}
					text2 = Token();
					if (text2 == null || Brace(text2, "}"))
					{
						throw new FormatException();
					}
					if (Brace(text2, "{"))
					{
						if (ValidId(text))
						{
							sortedDictionary[text] = Account();
						}
						else
						{
							Skip(1);
						}
					}
				}
			}
			List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
			foreach (KeyValuePair<string, string> item in sortedDictionary)
			{
				list.Add(new Dictionary<string, object>
				{
					{ "steam_id64", item.Key },
					{ "nickname", item.Value }
				});
			}
			return list;
		}
	}

	private const int FileLimit = 2097152;

	private const int BatchSize = 24;

	private static readonly object gate = new object();

	private static List<Dictionary<string, object>> accounts = new List<Dictionary<string, object>>();

	private static DateTime nextRead;

	private static int cursor;

	internal static bool ValidId(string value)
	{
		if (value == null || value.Length != 17)
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
		if (ulong.TryParse(value, out var result) && result > 76561197960265728L)
		{
			return result <= 76561202255233023L;
		}
		return false;
	}

	internal static string CleanName(string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = value ?? "";
		foreach (char c in text)
		{
			if (!char.IsControl(c))
			{
				stringBuilder.Append(c);
			}
		}
		if (stringBuilder.Length > 128)
		{
			stringBuilder.Length = 128;
			if (char.IsHighSurrogate(stringBuilder[127]))
			{
				stringBuilder.Length = 127;
			}
		}
		return stringBuilder.ToString();
	}

	internal static List<Dictionary<string, object>> Parse(string text)
	{
		return new Parser(text).Read();
	}

	private static string ReadFile(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		if (fileStream.Length > 2097152)
		{
			return "";
		}
		byte[] array = new byte[2097153];
		int i;
		int num;
		for (i = 0; i < array.Length; i += num)
		{
			if ((num = fileStream.Read(array, i, array.Length - i)) <= 0)
			{
				break;
			}
		}
		if (i > 2097152)
		{
			return "";
		}
		return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(array, 0, i);
	}

	private static List<Dictionary<string, object>> Local()
	{
		List<string> list = new List<string>();
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
			if (registryKey != null)
			{
				list.Add(registryKey.GetValue("SteamPath") as string);
			}
		}
		catch
		{
		}
		list.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam"));
		list.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam"));
		foreach (string item in list)
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				continue;
			}
			try
			{
				string path = Path.Combine(item, "config", "loginusers.vdf");
				if (File.Exists(path))
				{
					return Parse(ReadFile(path));
				}
			}
			catch
			{
			}
		}
		return new List<Dictionary<string, object>>();
	}

	internal static object[] Batch(List<Dictionary<string, object>> source, ref int offset)
	{
		if (source.Count == 0)
		{
			offset = 0;
			return new object[0];
		}
		offset %= source.Count;
		int num = Math.Min(24, source.Count - offset);
		object[] array = new object[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = source[offset + i];
		}
		offset = (offset + num) % source.Count;
		return array;
	}

	public static void Append(Dictionary<string, object> payload)
	{
		lock (gate)
		{
			if (DateTime.UtcNow >= nextRead)
			{
				nextRead = DateTime.UtcNow.AddMinutes(1.0);
				accounts = Local();
			}
			object[] array = Batch(accounts, ref cursor);
			if (array.Length > 0)
			{
				payload["steam_accounts"] = array;
			}
		}
	}
}
