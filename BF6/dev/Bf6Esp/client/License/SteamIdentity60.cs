using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Bf6Esp;

/// <summary>
/// 本机 Steam 账号探测：优先读注册表 Steam\Users，其次解析 config\loginusers.vdf，
/// 都不行时用机器指纹派生一个稳定的 17 位占位 ID（形状合法，仅用于单机自用场景）。
/// </summary>
internal sealed class SteamIdentity60
{
	/// <summary>SteamID64 的个人账号基址。</summary>
	internal const ulong IndividualBase = 76561197960265728UL;

	public string SteamId64 = "";

	public string PersonaName = "";

	public bool Ok
	{
		get { return SteamId64.Length == 17; }
	}

	/// <summary>读取本机 Steam 身份。</summary>
	internal static SteamIdentity60 Read()
	{
		SteamIdentity60 result = new SteamIdentity60();
		if (TryReadRegistry(result))
		{
			return result;
		}
		if (TryReadLoginUsers(result))
		{
			return result;
		}
		result.SteamId64 = DeriveFallbackId();
		result.PersonaName = "本机";
		return result;
	}

	private static bool TryReadRegistry(SteamIdentity60 result)
	{
		try
		{
			using (RegistryKey steam = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
			{
				if (steam == null)
				{
					return false;
				}
				using (RegistryKey users = steam.OpenSubKey("Users"))
				{
					if (users == null)
					{
						return false;
					}
					string[] names = users.GetSubKeyNames();
					uint bestAccount = 0U;
					int bestRecent = -1;
					string bestName = "";
					foreach (string name in names)
					{
						uint account;
						if (!uint.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out account))
						{
							continue;
						}
						string persona = "";
						int recent = 0;
						using (RegistryKey user = users.OpenSubKey(name))
						{
							if (user != null)
							{
								persona = Convert.ToString(user.GetValue("PersonaName"), CultureInfo.InvariantCulture);
								if (string.IsNullOrEmpty(persona))
								{
									persona = Convert.ToString(user.GetValue("AccountName"), CultureInfo.InvariantCulture);
								}
								object value = user.GetValue("MostRecent");
								if (value is int)
								{
									recent = (int)value;
								}
							}
						}
						if (recent > bestRecent)
						{
							bestRecent = recent;
							bestAccount = account;
							bestName = persona ?? "";
						}
					}
					if (bestRecent < 0)
					{
						return false;
					}
					result.SteamId64 = (IndividualBase + bestAccount).ToString(CultureInfo.InvariantCulture);
					result.PersonaName = string.IsNullOrEmpty(bestName) ? "Steam 用户" : bestName;
					return true;
				}
			}
		}
		catch (System.Security.SecurityException)
		{
			return false;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
	}

	private static bool TryReadLoginUsers(SteamIdentity60 result)
	{
		try
		{
			string steamPath;
			using (RegistryKey steam = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
			{
				if (steam == null)
				{
					return false;
				}
				steamPath = steam.GetValue("SteamPath") as string;
			}
			if (string.IsNullOrEmpty(steamPath))
			{
				return false;
			}
			string file = Path.Combine(steamPath, "config", "loginusers.vdf");
			if (!File.Exists(file))
			{
				return false;
			}
			byte[] data;
			using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
			{
				data = new byte[stream.Length];
				int done = 0;
				while (done < data.Length)
				{
					int read = stream.Read(data, done, data.Length - done);
					if (read <= 0)
					{
						break;
					}
					done += read;
				}
			}
			ulong steamId;
			string persona;
			if (!TryReadVdfUInt64(data, "SteamID", out steamId) || steamId < IndividualBase)
			{
				return false;
			}
			if (!TryReadVdfString(data, "PersonaName", out persona))
			{
				persona = "Steam 用户";
			}
			result.SteamId64 = steamId.ToString(CultureInfo.InvariantCulture);
			result.PersonaName = persona;
			return true;
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

	/// <summary>解析二进制 VDF 中的 64 位整数键（例如 "SteamID"）。</summary>
	private static bool TryReadVdfUInt64(byte[] data, string key, out ulong value)
	{
		value = 0UL;
		if (data == null || string.IsNullOrEmpty(key))
		{
			return false;
		}
		byte[] needle = Encoding.ASCII.GetBytes(key);
		int index = IndexOf(data, needle, 0);
		while (index >= 0)
		{
			int pos = index + needle.Length;
			if (pos < data.Length && data[pos] == 0)
			{
				pos++;
				if (pos < data.Length)
				{
					byte type = data[pos++];
					if (type == 0x08 && pos + 8 <= data.Length)
					{
						value = BitConverter.ToUInt64(data, pos);
						return true;
					}
					if (type == 0x02 && pos + 4 <= data.Length)
					{
						value = BitConverter.ToUInt32(data, pos);
						return true;
					}
					if (type == 0x06)
					{
						string text = ReadCString(data, pos);
						ulong parsed;
						if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out parsed))
						{
							value = parsed;
							return true;
						}
					}
				}
			}
			index = IndexOf(data, needle, index + 1);
		}
		return false;
	}

	/// <summary>解析二进制 VDF 中的字符串键（例如 "PersonaName"）。</summary>
	private static bool TryReadVdfString(byte[] data, string key, out string value)
	{
		value = "";
		if (data == null || string.IsNullOrEmpty(key))
		{
			return false;
		}
		byte[] needle = Encoding.ASCII.GetBytes(key);
		int index = IndexOf(data, needle, 0);
		while (index >= 0)
		{
			int pos = index + needle.Length;
			if (pos < data.Length && data[pos] == 0)
			{
				pos++;
				if (pos < data.Length && data[pos] == 0x06)
				{
					pos++;
					value = ReadCString(data, pos);
					if (value.Length > 0)
					{
						return true;
					}
				}
			}
			index = IndexOf(data, needle, index + 1);
		}
		return false;
	}

	private static string ReadCString(byte[] data, int offset)
	{
		int end = offset;
		while (end < data.Length && data[end] != 0)
		{
			end++;
		}
		return Encoding.UTF8.GetString(data, offset, end - offset);
	}

	private static int IndexOf(byte[] data, byte[] needle, int start)
	{
		if (data == null || needle == null || needle.Length == 0)
		{
			return -1;
		}
		for (int i = Math.Max(0, start); i <= data.Length - needle.Length; i++)
		{
			bool hit = true;
			for (int j = 0; j < needle.Length; j++)
			{
				if (data[i + j] != needle[j])
				{
					hit = false;
					break;
				}
			}
			if (hit)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>用机器指纹派生一个形状合法的占位 SteamID64（仅在读不到 Steam 时使用）。</summary>
	private static string DeriveFallbackId()
	{
		string material;
		try
		{
			material = LicenseDevice60.GetWindowsDeviceId();
		}
		catch (PlatformNotSupportedException)
		{
			material = Environment.MachineName;
		}
		byte[] hash;
		using (SHA256 algorithm = SHA256.Create())
		{
			hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(material));
		}
		ulong part = BitConverter.ToUInt64(hash, 0) % 4000000000UL;
		return (IndividualBase + part).ToString(CultureInfo.InvariantCulture);
	}
}
