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

	private static int unverified;

	public static bool Unverified => Volatile.Read(ref unverified) != 0;

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
				Volatile.Write(ref unverified, (!MatchesHash(hash)) ? 1 : 0);
				cacheKey = text;
			}
			catch
			{
				Volatile.Write(ref unverified, 1);
				cacheKey = null;
			}
		}
	}
}
