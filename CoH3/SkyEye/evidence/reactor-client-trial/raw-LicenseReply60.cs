using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Reflection;

namespace CoH3MemoryTool;

internal sealed class LicenseReply60
{
	private readonly Dictionary<string, object> data;

	public readonly long RequestStarted;

	public string State => Text("state");

	public bool SignatureVerified { get; private set; }

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	internal void VerifyActive(LeaseSignature60 context)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (context == null || !context.Verify(this, "yN6tFcbXVdx1crnUS1aLFZpxc08bUctCwq3xGcNVovzc/Ukrd+DeWBBFWqNEKeYW4tefrV7AEbICYyKVsSJ3v1I3t2r+PAYZ0pHxayWD0yE3zS75uYBrQKqZVuOmSIaYNbNwnMgdCI9D7lx8Qd3sEu3AuPjs11OanWuFb2e6HiydJ7ury9ova2bOR7sC1g5NeF60I1vJHXHRIc0rJdkREB3Io2NiWBJhiO92tzp1VfE1cDK0jdx/yrZ/dL7ZSzEVCooC9rEPE3Onhjn35inQ/RVmqZ8r6rq6zWOO4UCQOvjR+jCnycOZsr/oTA5oycu7AAkHJdDdwCnwuaXKLxpc6XH7OhZFRbUIqfMPaRcJTbWr4TWtwYH8sm1ez5ZgiOu+LcFrHnEf5nOI9j3diYlbeFBEhYCowG7U+D5xXRfhq6pCive/Y0VYFv8HXVFU5BM6tmd1jZpLrbHirkk4ZUAE+63zjqXzcJr3eA7zXDHktCu10wFFL6Y9GKEeFq/X9IjV", "AQAB", "8507f94ed17eb438"))
		{
			throw new HttpRequestException("授权签名验证失败");
		}
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
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
