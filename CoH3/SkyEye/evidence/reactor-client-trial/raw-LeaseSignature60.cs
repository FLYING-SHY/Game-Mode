using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
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
		if (!Guid.TryParse(Get(request, "installation_id"), out var result))
		{
			throw new ArgumentException("Invalid lease installation");
		}
		installation = result.ToString("D").ToLowerInvariant();
		nonce = Get(request, "nonce");
		bearer = token;
		activation = isActivation;
		if (!Regex.IsMatch(device, "\\A[0-9A-F]{64}\\z") || !Regex.IsMatch(steam, "\\A[0-9]{17}\\z") || !Regex.IsMatch(nonce, "\\A[0-9a-f]{32}\\z"))
		{
			throw new ArgumentException("Invalid lease request identity");
		}
	}

	private static string Get(Dictionary<string, object> values, string key)
	{
		if (!values.TryGetValue(key, out var value))
		{
			return "";
		}
		return Convert.ToString(value, CultureInfo.InvariantCulture);
	}

	internal static string Hash(byte[] value)
	{
		using SHA256 sHA = SHA256.Create();
		return BitConverter.ToString(sHA.ComputeHash(value)).Replace("-", "").ToLowerInvariant();
	}

	internal static string NewNonce()
	{
		byte[] array = new byte[16];
		using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
		{
			randomNumberGenerator.GetBytes(array);
		}
		return BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
	}

	[Obfuscation(Feature = "virtualization", Exclude = false)]
	internal bool Verify(LicenseReply60 reply, string modulus, string exponent, string kid)
	{
		try
		{
			if (reply.State != "active" || !Regex.IsMatch(kid ?? "", "\\A[0-9a-f]{16}\\z") || reply.Text("lease_kid") != kid)
			{
				return false;
			}
			byte[] array = Convert.FromBase64String(modulus);
			byte[] array2 = Convert.FromBase64String(exponent);
			if (array.Length != 384 || (array[0] & 0x80) == 0 || array2.Length == 0 || Hash(array).Substring(0, 16) != kid)
			{
				return false;
			}
			string text = reply.Text("session_token");
			if (text.Length == 0 && !activation)
			{
				text = bearer;
			}
			if (!Regex.IsMatch(text ?? "", "\\A[0-9a-fA-F]{64}\\z"))
			{
				return false;
			}
			string text2 = reply.Text("license_kind");
			if (text2 != "permanent" && text2 != "timed")
			{
				return false;
			}
			string[] array3 = new string[3];
			string[] array4 = new string[3] { "server_time", "lease_until", "expires_at" };
			for (int i = 0; i < array4.Length; i++)
			{
				string text3 = reply.Text(array4[i]);
				if (!long.TryParse(text3, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result) || text3 != result.ToString(CultureInfo.InvariantCulture))
				{
					return false;
				}
				array3[i] = text3;
			}
			string s = string.Join("\n", "skyeye-lease-v2", product, "active", device, steam, installation, nonce, Hash(Encoding.UTF8.GetBytes(text)), array3[0], array3[1], array3[2], text2, kid);
			byte[] array5 = Convert.FromBase64String(reply.Text("lease_signature"));
			if (array5.Length != 384)
			{
				return false;
			}
			using RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.PersistKeyInCsp = false;
			rSACryptoServiceProvider.ImportParameters(new RSAParameters
			{
				Modulus = array,
				Exponent = array2
			});
			return rSACryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(s), CryptoConfig.MapNameToOID("SHA256"), array5);
		}
		catch (Exception ex) when (ex is FormatException || ex is CryptographicException || ex is ArgumentException)
		{
			return false;
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
