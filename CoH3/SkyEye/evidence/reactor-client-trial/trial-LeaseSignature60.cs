using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NkSJSQ9aFrymjgfGPSw;
using WMXjPLTjQRLwQF9rQH;
using WhbXv9l6YNS2HQi939e;
using iZi9ysUsDKRJ6xFvQy;
using sgSDVb9nSVjnGPvpHJA;
using wqxojeRn0UCkY7t1HAJ;
using z2KxDjqVacbiWYiXsU;

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
		eypx9YKVY4l9aDN52a.heKJ7hrhy();
		base..ctor();
		product = LicenseConfiguration60.ProductId;
		device = Get(request, TZHq4URoSLru6L5UYD.oN75WAx6E(-584009922 ^ -455678426 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_a956819404834474a2f0248632e04189)).ToUpperInvariant();
		steam = Get(request, TZHq4URoSLru6L5UYD.oN75WAx6E(0x9BCC652 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_b6443a8faf2f4b3090496c4430070a5e));
		if (!Guid.TryParse(Get(request, TZHq4URoSLru6L5UYD.oN75WAx6E(0x3F32CA64 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_a2cc80d4a253498eb99d85f57c598ef1)), out var result))
		{
			throw new ArgumentException(TZHq4URoSLru6L5UYD.oN75WAx6E(0x451F80E3 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_2040728288974083bd777e82452eeaea));
		}
		installation = result.ToString(TZHq4URoSLru6L5UYD.oN75WAx6E(0x5D65FEF2 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_92273072dbbc4e9e84275d95fa6baded)).ToLowerInvariant();
		nonce = Get(request, TZHq4URoSLru6L5UYD.oN75WAx6E(0x1CC9994E ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_ce941b4538f246769e4ae1be0c4f7646));
		bearer = token;
		activation = isActivation;
		if (!Regex.IsMatch(device, TZHq4URoSLru6L5UYD.oN75WAx6E(0x2B1C20D4 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_8bb4814f464b4932845575de28d82d4e)) || !Regex.IsMatch(steam, TZHq4URoSLru6L5UYD.oN75WAx6E(0x18223CA6 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_84749d9b8fea4366bf094545a6e18f74)) || !Regex.IsMatch(nonce, TZHq4URoSLru6L5UYD.oN75WAx6E(0x18223C84 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_84749d9b8fea4366bf094545a6e18f74)))
		{
			throw new ArgumentException(TZHq4URoSLru6L5UYD.oN75WAx6E(0x18223F68 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_84749d9b8fea4366bf094545a6e18f74));
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
		return BitConverter.ToString(sHA.ComputeHash(value)).Replace(TZHq4URoSLru6L5UYD.oN75WAx6E(0x2A52E984 ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_3fe21fc71e1f49609aa0c7de26187f08), "").ToLowerInvariant();
	}

	internal static string NewNonce()
	{
		byte[] array = new byte[16];
		using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
		{
			randomNumberGenerator.GetBytes(array);
		}
		return BitConverter.ToString(array).Replace(TZHq4URoSLru6L5UYD.oN75WAx6E(--527063993 ^ 0x1157C37A ^ <Module>{ee6854b7-61c3-4791-8177-8ab67b6d78bf}.m_e463982edfb645278511b85b840b49c4.m_04208a06264940a08da0131250fef219), "").ToLowerInvariant();
	}

	internal bool Verify(LicenseReply60 reply, string modulus, string exponent, string kid)
	{
		scrTL5XAG5AQ8BJp2n.LJvDgJev2X();
		object[] array = new object[5] { this, reply, modulus, exponent, kid };
		int[] array2 = new int[5];
		object obj;
		try
		{
			bool flag = default(bool);
			obj = hoMuqZRhpg6UCtZcEnM.ehWtVS8XY6(150, array, array2, flag);
		}
		finally
		{
		}
		return (bool)obj;
	}

	internal static object _FF976B6562219865(object P_0)
	{
		return Encoding.UTF8;
	}

	internal static object _7306A438EC0F5125(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		return Convert.FromBase64String((string)P_1.GikcAEOwEG());
	}

	internal static object _3EB56FC987519AEC(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		return CryptoConfig.MapNameToOID((string)P_1.GikcAEOwEG());
	}

	internal static object _2366B9A73FE7F32D(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		return ((long)P_0).ToString((IFormatProvider)P_1.GikcAEOwEG());
	}

	internal static object _E4C3D36FA57ADCAE(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		return ((LicenseReply60)P_0).Text((string)P_1.GikcAEOwEG());
	}

	internal static long _BFD98E8F5BE5C9F9(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		((RSA)P_0).ImportParameters((RSAParameters)P_1.GikcAEOwEG());
		return 0L;
	}

	internal static long _D575C4B76ADC9076(object P_0, tmTrov97NfFp7sH21t1[] P_1, L75Tsv9h8IG0T3W8PTf P_2)
	{
		if (P_2.mEgc3GWnyv(3) == 3)
		{
			return _C572964F5B289DCB(P_0, P_1);
		}
		return px9gGE9zC4uhZwH1lq6.n0Fx1nRt4P();
	}

	private static long _C572964F5B289DCB(object P_0, tmTrov97NfFp7sH21t1[] P_1)
	{
		long result = P_1[3].a5fcXcu7Lm();
		bool flag;
		try
		{
			flag = long.TryParse((string)P_1[0].GikcAEOwEG(), (NumberStyles)P_1[1].a5fcXcu7Lm(), (IFormatProvider)P_1[2].GikcAEOwEG(), out result);
		}
		finally
		{
			P_1[3].c2scekgZIX(result);
		}
		return flag ? 1L : 0L;
	}

	internal static long _012F9C6F0CE56D73(object P_0, tmTrov97NfFp7sH21t1 P_1, tmTrov97NfFp7sH21t1 P_2)
	{
		return Regex.IsMatch((string)P_1.GikcAEOwEG(), (string)P_2.GikcAEOwEG()) ? 1L : 0L;
	}

	internal static long _27F883C97FF20A42(object P_0, long P_1)
	{
		((RSACryptoServiceProvider)P_0).PersistKeyInCsp = (byte)P_1 != 0;
		return 0L;
	}

	internal static object _AB3CA0A6106F1873(object P_0)
	{
		return CultureInfo.InvariantCulture;
	}

	internal static long _70ACA2BF2D2664C4(object P_0, tmTrov97NfFp7sH21t1 P_1, tmTrov97NfFp7sH21t1 P_2)
	{
		return ((string)P_1.GikcAEOwEG() != (string)P_2.GikcAEOwEG()) ? 1L : 0L;
	}

	internal static object _E20AFD7493175E0C(object P_0)
	{
		return new RSACryptoServiceProvider();
	}

	internal static long _BC2B946059C93293(object P_0)
	{
		return ((string)P_0).Length;
	}

	internal static object _B28898E15E17D8B3(object P_0)
	{
		return ((LicenseReply60)P_0).State;
	}

	internal static object _68B5EB410ACBA0ED(object P_0, tmTrov97NfFp7sH21t1 P_1, tmTrov97NfFp7sH21t1 P_2)
	{
		return string.Join((string)P_1.GikcAEOwEG(), (string[])P_2.GikcAEOwEG());
	}

	internal static object _8E57457A20D64F8D(object P_0, long P_1, long P_2)
	{
		return ((string)P_0).Substring((int)P_1, (int)P_2);
	}

	internal static long _D606EF9EBA04CCF2(object P_0)
	{
		((IDisposable)P_0).Dispose();
		return 0L;
	}

	internal static object _6DAA681269B900C6(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		return ((Encoding)P_0).GetBytes((string)P_1.GikcAEOwEG());
	}

	internal static object _C8C0BD9592072CB5(object P_0, tmTrov97NfFp7sH21t1 P_1)
	{
		return Hash((byte[])P_1.GikcAEOwEG());
	}

	internal static long _9AA298A4852DC945(object P_0, tmTrov97NfFp7sH21t1 P_1, tmTrov97NfFp7sH21t1 P_2, tmTrov97NfFp7sH21t1 P_3)
	{
		return ((RSACryptoServiceProvider)P_0).VerifyData((byte[])P_1.GikcAEOwEG(), P_2.GikcAEOwEG(), (byte[])P_3.GikcAEOwEG()) ? 1L : 0L;
	}

	internal static void _BC300B85AED046E9(object P_0, object P_1)
	{
		((RSAParameters)P_0).Exponent = (byte[])P_1;
	}

	internal static void _8533FDA72B6C0F98(object P_0, object P_1)
	{
		((RSAParameters)P_0).Modulus = (byte[])P_1;
	}

	internal static object _7E2D968C726492F1(object P_0)
	{
		return ((LeaseSignature60)P_0).nonce;
	}

	internal static object _71DDFE50FCD19CCB(object P_0)
	{
		return ((LeaseSignature60)P_0).installation;
	}

	internal static object _403DAAE81C9E7C03(object P_0)
	{
		return ((LeaseSignature60)P_0).steam;
	}

	internal static object _C20E64AB1FA724D7(object P_0)
	{
		return ((LeaseSignature60)P_0).device;
	}

	internal static object _81549FF993E5AE18(object P_0)
	{
		return ((LeaseSignature60)P_0).product;
	}

	internal static object _19903EB84CA59CFE(object P_0)
	{
		return ((LeaseSignature60)P_0).bearer;
	}

	internal static long _98EA81ED16CCBD1A(object P_0)
	{
		return ((LeaseSignature60)P_0).activation ? 1L : 0L;
	}

	internal static object _BB4ED89F094F93F0(object P_0)
	{
		return P_0 as CryptographicException;
	}

	internal static object _A44BCCED50F5C013(long P_0)
	{
		return P_0;
	}

	internal static object _5FA409B3199FA7FF(object P_0)
	{
		return P_0 as FormatException;
	}

	internal static object _0A184F51F83D92EC()
	{
		return default(RSAParameters);
	}

	internal static object _461314F9D424FBB5(object P_0)
	{
		return P_0 as Exception;
	}

	internal static object _21EDEFD197F11D32(object P_0)
	{
		return P_0 as ArgumentException;
	}

	internal static object _45B5DC2901798AE6(object P_0)
	{
		return (RSAParameters)P_0;
	}

	internal static object _157C83829A411FF2(int P_0)
	{
		return new string[P_0];
	}

	internal static void _940F46C240623423(object P_0, int P_1, object P_2)
	{
		((string[])P_0)[P_1] = (string)P_2;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.1.0.7988')
