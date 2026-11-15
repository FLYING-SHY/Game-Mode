using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CoH3MemoryTool;

internal sealed class LeaseSignature60
{
	private readonly string product;
	private readonly string device;
	private readonly string steam;
	private readonly string installation;
	private readonly string nonce;
	private readonly string bearer;
	private readonly bool activation;

	internal LeaseSignature60(Dictionary<string, object> request, string token, bool isActivation)
	{
		product = LicenseConfiguration60.ProductId;
		device = Get(request, "device_id").ToUpperInvariant();
		steam = Get(request, "steam_id64");
		if (!Guid.TryParse(Get(request, "installation_id"), out var id)) throw new ArgumentException("Invalid lease installation");
		installation = id.ToString("D").ToLowerInvariant();
		nonce = Get(request, "nonce");
		bearer = token;
		activation = isActivation;
		if (!Regex.IsMatch(device, "\\A[0-9A-F]{64}\\z") || !Regex.IsMatch(steam, "\\A[0-9]{17}\\z") || !Regex.IsMatch(nonce, "\\A[0-9a-f]{32}\\z")) throw new ArgumentException("Invalid lease request identity");
	}

	private static string Get(Dictionary<string, object> values, string key) => values.TryGetValue(key, out var value) ? Convert.ToString(value, CultureInfo.InvariantCulture) : "";

	internal static string Hash(byte[] value)
	{
		using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(value)).Replace("-", "").ToLowerInvariant();
	}

	internal static string NewNonce()
	{
		byte[] bytes = new byte[16];
		using (var random = RandomNumberGenerator.Create()) random.GetBytes(bytes);
		return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
	}

	[System.Reflection.Obfuscation(Feature = "virtualization", Exclude = false)]
	internal bool Verify(LicenseReply60 reply, string modulus, string exponent, string kid)
	{
		try
		{
			if (reply.State != "active" || !Regex.IsMatch(kid ?? "", "\\A[0-9a-f]{16}\\z") || reply.Text("lease_kid") != kid) return false;
			byte[] n = Convert.FromBase64String(modulus), e = Convert.FromBase64String(exponent);
			if (n.Length != 384 || (n[0] & 128) == 0 || e.Length == 0 || Hash(n).Substring(0, 16) != kid) return false;
			string token = reply.Text("session_token");
			if (token.Length == 0 && !activation) token = bearer;
			if (!Regex.IsMatch(token ?? "", "\\A[0-9a-fA-F]{64}\\z")) return false;
			string kind = reply.Text("license_kind");
			if (kind != "permanent" && kind != "timed") return false;
			string[] times = new string[3];
			string[] names = { "server_time", "lease_until", "expires_at" };
			for (int i = 0; i < names.Length; i++)
			{
				string raw = reply.Text(names[i]);
				if (!long.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var time) || raw != time.ToString(CultureInfo.InvariantCulture)) return false;
				times[i] = raw;
			}
			string canonical = string.Join("\n", new[] { "skyeye-lease-v2", product, "active", device, steam, installation, nonce, Hash(Encoding.UTF8.GetBytes(token)), times[0], times[1], times[2], kind, kid });
			byte[] signature = Convert.FromBase64String(reply.Text("lease_signature"));
			if (signature.Length != 384) return false;
			using (var rsa = new RSACryptoServiceProvider())
			{
				rsa.PersistKeyInCsp = false;
				rsa.ImportParameters(new RSAParameters { Modulus = n, Exponent = e });
				return rsa.VerifyData(Encoding.UTF8.GetBytes(canonical), CryptoConfig.MapNameToOID("SHA256"), signature);
			}
		}
		catch (Exception ex) when (ex is FormatException || ex is CryptographicException || ex is ArgumentException)
		{
			return false;
		}
	}
}
