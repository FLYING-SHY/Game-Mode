using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace CoH3MemoryTool;

internal static class GameVersion45
{
	// Legacy baseline build, kept for players who have not updated yet.
	public const string SupportedHash = "2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde";

	// 5.1.52426.0: every layout role is re-located per build, see GameLayout52426.
	public const string SupportedHash52426 = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec";

	private static readonly object gate = new object();

	private static string cacheKey;

	private static CheckStatus cachedStatus = CheckStatus.NotChecked;

	private static GameBuild cachedBuild = GameBuild.Unknown;

	internal enum CheckStatus
	{
		NotChecked,
		Verified,
		FingerprintMismatch,
		ReadFailed
	}

	// Each verified fingerprint selects its own layout table; mixing them yields wrong reads.
	internal enum GameBuild
	{
		Unknown,
		Build2406,
		Build52426
	}

	private static int checkStatus;

	private static int detectedBuild;

	internal static CheckStatus Status => (CheckStatus)Volatile.Read(ref checkStatus);

	internal static GameBuild CurrentBuild => (GameBuild)Volatile.Read(ref detectedBuild);

	internal static bool MatchesHash(string hash)
	{
		return BuildOf(hash) != GameBuild.Unknown;
	}

	internal static GameBuild BuildOf(string hash)
	{
		if (string.Equals(hash, SupportedHash, StringComparison.OrdinalIgnoreCase))
		{
			return GameBuild.Build2406;
		}
		if (string.Equals(hash, SupportedHash52426, StringComparison.OrdinalIgnoreCase))
		{
			return GameBuild.Build52426;
		}
		return GameBuild.Unknown;
	}

	internal static void RequireVerified(CheckStatus status, bool maintenanceDiagnostics = false)
	{
#if MAINTENANCE_DIAGNOSTICS
		// Only a separately compiled maintenance reader may inspect unverified layouts.
		if (maintenanceDiagnostics)
		{
			return;
		}
#endif
		if (status != CheckStatus.Verified)
		{
			throw new InvalidOperationException(status == CheckStatus.FingerprintMismatch
				? "游戏版本未验证，结构依赖功能已暂停。请等待版本适配。"
				: "游戏版本验证未完成，结构依赖功能已暂停。请检查游戏主程序读取权限。");
		}
	}

	public static CheckStatus Assess(string path)
	{
		return Assess(path, out _);
	}

	// The caller keeps the detected build so that every layout lookup of that process
	// uses the table that belongs to the fingerprint which was really observed.
	internal static CheckStatus Assess(string path, out GameBuild build)
	{
		lock (gate)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(path);
				string text = fileInfo.FullName + "|" + fileInfo.Length + "|" + fileInfo.LastWriteTimeUtc.Ticks;
				if (text == cacheKey)
				{
					build = cachedBuild;
					return cachedStatus;
				}
				string hash;
				using (FileStream inputStream = File.OpenRead(path))
				{
					using SHA256 sHA = SHA256.Create();
					hash = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
				}
				GameBuild detected = BuildOf(hash);
				Volatile.Write(ref detectedBuild, (int)detected);
				CheckStatus status = detected != GameBuild.Unknown ? CheckStatus.Verified : CheckStatus.FingerprintMismatch;
				Volatile.Write(ref checkStatus, (int)status);
				cacheKey = text;
				cachedStatus = status;
				cachedBuild = detected;
				build = detected;
			}
			catch
			{
				Volatile.Write(ref checkStatus, (int)CheckStatus.ReadFailed);
				Volatile.Write(ref detectedBuild, (int)GameBuild.Unknown);
				cacheKey = null;
				cachedStatus = CheckStatus.ReadFailed;
				cachedBuild = GameBuild.Unknown;
				build = GameBuild.Unknown;
			}
			return Status;
		}
	}
}
