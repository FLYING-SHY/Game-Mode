using System;

namespace CoH3MemoryTool;

internal static class LicenseVersion60
{
	public static bool IsOlder(string current, string minimum)
	{
		if (!Version.TryParse(current, out var result) || !Version.TryParse(minimum, out var result2))
		{
			throw new ArgumentException("Invalid version");
		}
		return Normalize(result).CompareTo(Normalize(result2)) < 0;
	}

	private static Version Normalize(Version v)
	{
		return new Version(v.Major, v.Minor, Math.Max(0, v.Build), Math.Max(0, v.Revision));
	}

	public static bool DownloadAllowed(string value)
	{
		Uri configured = LicenseConfiguration60.ServerBaseAddress;
		if (Uri.TryCreate(value, UriKind.Absolute, out var result) && string.IsNullOrEmpty(result.UserInfo))
		{
			return result.Scheme == configured.Scheme && result.Host.Equals(configured.Host, StringComparison.OrdinalIgnoreCase) && result.Port == configured.Port;
		}
		return false;
	}
}
