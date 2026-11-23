using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Bf6Esp;

/// <summary>
/// 偏移档案的读写与匹配。
/// 档案以 SHA-256 绑定到具体游戏 build；匹配不上时给出"版本不匹配"。
/// </summary>
internal static class ProfileStore
{
	/// <summary>
	/// 定位 profiles 目录：优先取可执行文件同级目录，其次向上查找。
	/// 只认"里面确实有 *.json 档案"的目录 —— 开发树里 client\Profiles 是源码目录，
	/// 在大小写不敏感的 Windows 上会被同名匹配挡住。
	/// </summary>
	internal static string LocateProfilesDirectory()
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		string fallback = null;
		string candidate = Path.Combine(baseDirectory, "profiles");
		if (Directory.Exists(candidate))
		{
			fallback = candidate;
			if (ContainsProfile(candidate))
			{
				return candidate;
			}
		}
		DirectoryInfo info = new DirectoryInfo(baseDirectory);
		for (int i = 0; i < 8 && info != null; i++)
		{
			string path = Path.Combine(info.FullName, "profiles");
			if (Directory.Exists(path))
			{
				if (ContainsProfile(path))
				{
					return path;
				}
				if (fallback == null)
				{
					fallback = path;
				}
			}
			info = info.Parent;
		}
		return fallback;
	}

	/// <summary>目录里是否至少有一个 json 档案。</summary>
	private static bool ContainsProfile(string directory)
	{
		try
		{
			return Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly).Length > 0;
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

	/// <summary>计算文件的 SHA-256（大写十六进制）。</summary>
	internal static string ComputeSha256(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return "";
		}
		try
		{
			using (SHA256 algorithm = SHA256.Create())
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				byte[] hash = algorithm.ComputeHash(stream);
				StringBuilder builder = new StringBuilder(hash.Length * 2);
				foreach (byte value in hash)
				{
					builder.Append(value.ToString("X2", CultureInfo.InvariantCulture));
				}
				return builder.ToString();
			}
		}
		catch (IOException)
		{
			return "";
		}
		catch (UnauthorizedAccessException)
		{
			return "";
		}
	}

	/// <summary>读取目录下所有 *.json 档案。</summary>
	internal static List<OffsetProfile> LoadAll(string directory)
	{
		List<OffsetProfile> result = new List<OffsetProfile>();
		if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
		{
			return result;
		}
		string[] files;
		try
		{
			files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
		}
		catch (IOException)
		{
			return result;
		}
		Array.Sort(files, StringComparer.OrdinalIgnoreCase);
		foreach (string file in files)
		{
			OffsetProfile profile = Load(file);
			if (profile != null)
			{
				result.Add(profile);
			}
		}
		return result;
	}

	/// <summary>读取单个档案；失败返回 null。</summary>
	internal static OffsetProfile Load(string file)
	{
		try
		{
			string text = File.ReadAllText(file, Encoding.UTF8);
			return JsonText60.Deserialize<OffsetProfile>(text);
		}
		catch (SerializationException)
		{
			return null;
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
		catch (XmlException)
		{
			return null;
		}
	}

	/// <summary>写出档案（UTF-8）。</summary>
	internal static bool Save(OffsetProfile profile, string file)
	{
		if (profile == null || string.IsNullOrEmpty(file))
		{
			return false;
		}
		try
		{
			string directory = Path.GetDirectoryName(file);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			string text = JsonText60.Serialize(profile);
			File.WriteAllText(file, text, new UTF8Encoding(false));
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
		catch (SerializationException)
		{
			return false;
		}
	}

	/// <summary>按 SHA-256 选出匹配的档案。</summary>
	internal static OffsetProfile MatchByHash(List<OffsetProfile> profiles, string sha256)
	{
		if (profiles == null || string.IsNullOrEmpty(sha256))
		{
			return null;
		}
		foreach (OffsetProfile profile in profiles)
		{
			if (!string.IsNullOrEmpty(profile.BuildSha256) && string.Equals(profile.BuildSha256, sha256, StringComparison.OrdinalIgnoreCase))
			{
				return profile;
			}
		}
		return null;
	}

	/// <summary>
	/// 特征码回退解析：档案的 SHA-256 与当前 build 不一致时，仍尝试用签名把偏移解析出来。
	/// 返回 null 表示没有任何签名可以解析出目标。
	/// </summary>
	internal static bool TryResolveBySignature(OffsetProfile profile, MemoryReader reader, ulong moduleBase, PeImage image, out Dictionary<string, ulong> resolved, out List<string> log)
	{
		resolved = new Dictionary<string, ulong>();
		log = new List<string>();
		if (profile == null || reader == null || image == null || profile.Resolution == null)
		{
			return false;
		}
		foreach (ResolutionRule60 rule in profile.Resolution)
		{
			if (rule == null || string.IsNullOrEmpty(rule.Target) || string.IsNullOrEmpty(rule.Signature))
			{
				continue;
			}
			SignatureSpec60 spec = profile.Signature(rule.Signature);
			if (spec == null || string.IsNullOrEmpty(spec.Pattern))
			{
				log.Add(rule.Target + ": 缺少签名 " + rule.Signature);
				continue;
			}
			SignatureScanner scanner = SignatureScanner.Parse(spec.Pattern);
			if (scanner == null)
			{
				log.Add(rule.Target + ": 模式串非法 " + spec.Pattern);
				continue;
			}
			long scanned;
			List<SignatureHit> hits = scanner.Scan(reader, moduleBase, image, spec.Section, out scanned);
			if (hits.Count == 0)
			{
				log.Add(rule.Target + ": 签名 " + spec.Name + " 0 命中");
				continue;
			}
			if (hits.Count > 1)
			{
				log.Add(rule.Target + ": 签名 " + spec.Name + " " + hits.Count + " 命中，取第一个");
			}
			SignatureHit hit = hits[0];
			ulong value = 0UL;
			bool ok = false;
			string mode = rule.Mode ?? "match";
			if (mode == "match")
			{
				value = hit.Address;
				ok = true;
			}
			else if (mode == "match+off")
			{
				value = hit.Address + (ulong)(long)rule.Offset;
				ok = true;
			}
			else if (mode == "rel32")
			{
				int displacement;
				if (reader.TryRead(hit.Address + (ulong)(long)rule.Offset, out displacement))
				{
					value = hit.Address + (ulong)(long)rule.Offset + 4UL + (ulong)(long)displacement;
					ok = true;
				}
			}
			else if (mode == "value")
			{
				ulong pointer;
				if (reader.TryRead(hit.Address + (ulong)(long)rule.Offset, out pointer) && pointer != 0UL)
				{
					value = pointer;
					ok = true;
				}
			}
			if (!ok)
			{
				log.Add(rule.Target + ": 签名命中但解析失败（mode=" + mode + "）");
				continue;
			}
			resolved[rule.Target] = value;
			log.Add(rule.Target + ": 由 " + spec.Name + " 解析为 0x" + value.ToString("X"));
		}
		return resolved.Count > 0;
	}
}
