using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace CoH3MemoryTool;

internal sealed class GameIdentityProvider60 : IGameIdentityProvider60
{
	private int checkedPid;

	private DateTime checkedStart;

	public GameIdentity60 Read()
	{
		Process[] processesByName = Process.GetProcessesByName("RelicCoH3");
		try
		{
			if (processesByName.Length != 1)
			{
				GameIdentity60 gameIdentity = new GameIdentity60();
				gameIdentity.Error = ((processesByName.Length == 0) ? "请先启动游戏，并停留在大厅。" : "检测到多个游戏进程，请保留一个。");
				return gameIdentity;
			}
			Process process = processesByName[0];
			DateTime startTime = process.StartTime;
			if (checkedPid != process.Id || checkedStart != startTime)
			{
				GameVersion45.Assess(process.MainModule.FileName);
				checkedPid = process.Id;
				checkedStart = startTime;
			}
			string defaultPath = LocalAccountLog59.DefaultPath;
			DateTime lastWriteTime = File.GetLastWriteTime(defaultPath);
			string text;
			using (FileStream fileStream = new FileStream(defaultPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
			{
				byte[] array = new byte[Math.Min(65536, (int)Math.Min(fileStream.Length, 2147483647L))];
				int i;
				int num;
				for (i = 0; i < array.Length; i += num)
				{
					if ((num = fileStream.Read(array, i, array.Length - i)) <= 0)
					{
						break;
					}
				}
				text = Encoding.UTF8.GetString(array, 0, i).TrimStart('\ufeff');
				if (fileStream.Length > i)
				{
					int num2 = text.LastIndexOf('\n');
					if (num2 < 0)
					{
						GameIdentity60 gameIdentity2 = new GameIdentity60();
						gameIdentity2.Error = "游戏账号正在准备，请稍候。";
						return gameIdentity2;
					}
					text = text.Substring(0, num2 + 1);
				}
			}
			ulong? num3 = LocalAccountLog59.Parse(text, startTime, lastWriteTime);
			if (!num3.HasValue || process.HasExited || process.StartTime != startTime || File.GetLastWriteTime(defaultPath) < startTime)
			{
				GameIdentity60 gameIdentity3 = new GameIdentity60();
				gameIdentity3.Error = "尚未读到本次游戏账号，请稍候；必要时重启游戏。";
				return gameIdentity3;
			}
			string name = ParseName(text);
			GameIdentity60 gameIdentity4 = new GameIdentity60();
			gameIdentity4.Pid = process.Id;
			gameIdentity4.Started = startTime;
			gameIdentity4.SteamId = num3.Value.ToString(CultureInfo.InvariantCulture);
			gameIdentity4.Name = name;
			return gameIdentity4;
		}
		catch (UnauthorizedAccessException)
		{
			GameIdentity60 gameIdentity5 = new GameIdentity60();
			gameIdentity5.Error = "无法读取游戏身份，请使用与游戏相同的运行权限。";
			return gameIdentity5;
		}
		catch (Win32Exception)
		{
			GameIdentity60 gameIdentity6 = new GameIdentity60();
			gameIdentity6.Error = "暂时无法确认游戏进程，请稍候。";
			return gameIdentity6;
		}
		catch (IOException)
		{
			GameIdentity60 gameIdentity7 = new GameIdentity60();
			gameIdentity7.Error = "正在等待游戏启动记录。";
			return gameIdentity7;
		}
		catch (InvalidOperationException)
		{
			GameIdentity60 gameIdentity8 = new GameIdentity60();
			gameIdentity8.Error = "游戏进程已变化，正在重新识别。";
			return gameIdentity8;
		}
		finally
		{
			Process[] array2 = processesByName;
			foreach (Process process2 in array2)
			{
				process2.Dispose();
			}
		}
	}

	internal static string ParseName(string head)
	{
		using (StringReader stringReader = new StringReader(head))
		{
			string text;
			while ((text = stringReader.ReadLine()) != null)
			{
				int num = text.IndexOf(": Current Steam alias/persona is [", StringComparison.Ordinal);
				if (num < 0 || !text.EndsWith("]", StringComparison.Ordinal))
				{
					continue;
				}
				string text2 = text.Substring(num + ": Current Steam alias/persona is [".Length, text.Length - num - ": Current Steam alias/persona is [".Length - 1);
				if (text2.Length > 128)
				{
					return "";
				}
				string text3 = text2;
				foreach (char c in text3)
				{
					if (c < ' ' || c == '\u007f')
					{
						return "";
					}
				}
				return text2;
			}
		}
		return "";
	}
}
