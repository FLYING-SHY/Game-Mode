using System;

namespace CoH3MemoryTool;

internal sealed class ReadRecovery55
{
	private int failures;

	private DateTime first;

	public void Success()
	{
		failures = 0;
		first = DateTime.MinValue;
	}

	public bool Retry(bool sameMatch, DateTime now)
	{
		if (!sameMatch)
		{
			Success();
			return false;
		}
		if (failures++ == 0)
		{
			first = now;
		}
		if (failures <= 8)
		{
			return now - first < TimeSpan.FromSeconds(2.0);
		}
		return false;
	}
}
