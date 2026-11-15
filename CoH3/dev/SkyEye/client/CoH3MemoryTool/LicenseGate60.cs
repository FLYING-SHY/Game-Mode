using System;
using System.Threading;

namespace CoH3MemoryTool;

internal static class LicenseGate60
{
	private static LicenseLease60 current = new LicenseLease60();

	private static long epoch;

	public static bool Allowed
	{
		get
		{
			LicenseLease60 licenseLease = Volatile.Read(ref current);
			return licenseLease.Deadline > LicenseClock60.Milliseconds;
		}
	}

	public static long Epoch => Volatile.Read(ref current).Epoch;

	public static bool Matches(int pid, DateTime started)
	{
		LicenseLease60 licenseLease = Volatile.Read(ref current);
		if (Allowed && licenseLease.Pid == pid)
		{
			return licenseLease.Started == started;
		}
		return false;
	}

	internal static bool Grant(long server, long lease, long expires, long requestStarted, GameIdentity60 identity)
	{
		long num = Math.Min(lease, expires) - server;
		if (identity == null || num <= 0 || num > 60 || expires < lease)
		{
			return false;
		}
		long num2 = requestStarted + num * 1000 - 1000;
		if (num2 <= LicenseClock60.Milliseconds)
		{
			return false;
		}
		LicenseLease60 licenseLease = Volatile.Read(ref current);
		long num3 = ((licenseLease.Deadline > LicenseClock60.Milliseconds && licenseLease.Pid == identity.Pid && licenseLease.Started == identity.Started) ? licenseLease.Epoch : Interlocked.Increment(ref epoch));
		Volatile.Write(ref current, new LicenseLease60
		{
			Deadline = num2,
			Epoch = num3,
			Pid = identity.Pid,
			Started = identity.Started
		});
		return true;
	}

	public static void Revoke()
	{
		Volatile.Write(ref current, new LicenseLease60
		{
			Epoch = Interlocked.Increment(ref epoch)
		});
	}
}
