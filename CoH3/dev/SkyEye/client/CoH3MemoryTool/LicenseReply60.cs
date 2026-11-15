using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoH3MemoryTool;

internal sealed class LicenseReply60
{
	private readonly Dictionary<string, object> data;

	public readonly long RequestStarted;

	public string State => Text("state");

	public bool SignatureVerified { get; private set; }

	[System.Reflection.Obfuscation(Feature = "virtualization", Exclude = false)]
	internal void VerifyActive(LeaseSignature60 context)
	{
		if (context == null || !context.Verify(this, PinnedLeaseKey60.Modulus, PinnedLeaseKey60.Exponent, PinnedLeaseKey60.Kid))
			throw new System.Net.Http.HttpRequestException("授权签名验证失败");
		SignatureVerified = true;
	}

	public LicenseReply60(Dictionary<string, object> values, long started)
	{
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
}
