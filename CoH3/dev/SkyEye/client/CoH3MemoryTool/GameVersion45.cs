using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace CoH3MemoryTool;

internal static class GameVersion45
{
	public const string SupportedHash = "2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde";

	private static readonly object gate = new object();

	private static string cacheKey;

	internal enum CheckStatus
	{
		NotChecked,
		Verified,
		FingerprintMismatch,
		ReadFailed
	}

	private static int checkStatus;

	internal static CheckStatus Status => (CheckStatus)Volatile.Read(ref checkStatus);

	internal static bool MatchesHash(string hash)
	{
		return string.Equals(hash, "2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde", StringComparison.OrdinalIgnoreCase);
	}

	public static void Assess(string path)
	{
		lock (gate)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(path);
				string text = fileInfo.FullName + "|" + fileInfo.Length + "|" + fileInfo.LastWriteTimeUtc.Ticks;
				if (text == cacheKey)
				{
					return;
				}
				string hash;
				using (FileStream inputStream = File.OpenRead(path))
				{
					using SHA256 sHA = SHA256.Create();
					hash = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
				}
				Volatile.Write(ref checkStatus, (int)(MatchesHash(hash) ? CheckStatus.Verified : CheckStatus.FingerprintMismatch));
				cacheKey = text;
			}
			catch
			{
				Volatile.Write(ref checkStatus, (int)CheckStatus.ReadFailed);
				cacheKey = null;
			}
		}
	}
}
