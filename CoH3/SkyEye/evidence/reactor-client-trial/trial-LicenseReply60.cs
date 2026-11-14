using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using NkSJSQ9aFrymjgfGPSw;
using WMXjPLTjQRLwQF9rQH;
using iZi9ysUsDKRJ6xFvQy;
using wqxojeRn0UCkY7t1HAJ;
using z2KxDjqVacbiWYiXsU;

namespace CoH3MemoryTool;

internal sealed class LicenseReply60
{
	private readonly Dictionary<string, object> data;

	public readonly long RequestStarted;

	public string State => Text(TZHq4URoSLru6L5UYD.oN75WAx6E(0x6FFE28A3 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_e581728154924db3b05673e321bf5c72));

	public bool SignatureVerified { get; private set; }

	internal void VerifyActive(LeaseSignature60 context)
	{
		scrTL5XAG5AQ8BJp2n.LJvDgJev2X();
		object[] array = new object[2] { this, context };
		int[] array2 = new int[2];
		try
		{
			hoMuqZRhpg6UCtZcEnM.ehWtVS8XY6(116, array, array2, null);
		}
		finally
		{
		}
	}

	public LicenseReply60(Dictionary<string, object> values, long started)
	{
		eypx9YKVY4l9aDN52a.heKJ7hrhy();
		base..ctor();
		data = new Dictionary<string, object>(values);
		RequestStarted = started;
	}

	public string Text(string key)
	{
		if (!data.TryGetValue(key, out var value))
		{
			return "";
		}
		return Convert.ToString(value, CultureInfo.InvariantCulture);
	}

	public long Number(string key)
	{
		if (!long.TryParse(Text(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return 0L;
		}
		return result;
	}

	internal static object _058889B666F546D2(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		return (object)new HttpRequestException((string)P_1.GikcAEOwEG());
	}

	internal static long _50C4C565FF57CAA5(object P_0, long P_1)
	{
		((LicenseReply60)P_0).SignatureVerified = (byte)P_1 != 0;
		return 0L;
	}

	internal static long _F639ADB0E8C6FFDE(object P_0, tmTrov97NfFp7sH21t1 P_1, tmTrov97NfFp7sH21t1 P_2, tmTrov97NfFp7sH21t1 P_3, tmTrov97NfFp7sH21t1 P_4)
	{
		return ((LeaseSignature60)P_0).Verify((LicenseReply60)P_1.GikcAEOwEG(), (string)P_2.GikcAEOwEG(), (string)P_3.GikcAEOwEG(), (string)P_4.GikcAEOwEG()) ? 1L : 0L;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
