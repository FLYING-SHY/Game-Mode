using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using b;

namespace A
{
	internal sealed class P
	{
		public bool A;

		public string A;

		public string a;

		public long A;

		public bool a;

		public readonly Dictionary<uint, int> A = new Dictionary<uint, int>();

		public string A => this.a + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bt();

		public static bool A(string P_0)
		{
			return string.Equals(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bU(), StringComparison.OrdinalIgnoreCase);
		}

		public static bool a(string P_0)
		{
			if (!A(P_0))
			{
				if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bu()) && (P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bS()) || P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bV())))
				{
					if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bg()) && !P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bv()))
					{
						return P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bW());
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public static string B(string P_0)
		{
			string text = R.a(P_0);
			if (text != null)
			{
				return global::b.j.A(text);
			}
			if (!A(P_0))
			{
				if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bg()))
				{
					if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bv()))
					{
						if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bw()))
						{
							if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bX()))
							{
								if (!P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bx()))
								{
									return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bY();
								}
								return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.by();
							}
							return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bZ();
						}
						return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bz();
					}
					return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CA();
				}
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bH();
			}
			return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ca();
		}
	}
	internal static class p
	{
		private static readonly Dictionary<string, string> m_A = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CD(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cd()
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CE(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ce()
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CF(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cf()
			},
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CG(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cf()
			}
		};

		public static bool A(string P_0)
		{
			if (P_0 != null)
			{
				return p.m_A.ContainsKey(P_0);
			}
			return false;
		}

		public static bool a(string P_0)
		{
			return P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CB();
		}

		public static bool B(string P_0)
		{
			if (!A(P_0))
			{
				return a(P_0);
			}
			return true;
		}

		public static string b(string P_0)
		{
			if (P_0 == null || !p.m_A.TryGetValue(P_0, out var value))
			{
				if (!a(P_0))
				{
					return null;
				}
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cb();
			}
			return value;
		}

		public static bool C(string P_0)
		{
			if (!(P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CC()))
			{
				return P_0 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cc();
			}
			return true;
		}

		public static bool A(byte[] P_0)
		{
			if (P_0 != null && P_0.Length >= 164 && P_0[76] != 2 && (P_0[144] & 1) != 0 && BitConverter.ToUInt32(P_0, 12) != 0 && BitConverter.ToUInt32(P_0, 12) <= 64)
			{
				return BitConverter.ToUInt32(P_0, 8) != uint.MaxValue;
			}
			return false;
		}
	}
}
namespace a
{
	internal static class P
	{
		private static readonly byte[] m_A = Encoding.UTF8.GetBytes(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Eh());

		internal static string A => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EG(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Eg());

		public static M a()
		{
			return A(P.A);
		}

		internal static M A(string P_0)
		{
			try
			{
				byte[] bytes = ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EH())), P.m_A, DataProtectionScope.CurrentUser);
				M m2 = new JavaScriptSerializer().Deserialize<M>(Encoding.UTF8.GetString(bytes));
				if (m2 != null && Guid.TryParse(m2.Installation, out var result) && result != Guid.Empty)
				{
					return m2;
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (CryptographicException)
			{
			}
			catch (ArgumentException)
			{
			}
			catch (InvalidOperationException)
			{
			}
			return new M
			{
				Installation = Guid.NewGuid().ToString(),
				Code = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D()
			};
		}

		public static void A(M P_0)
		{
			A(P_0, P.A);
		}

		internal static void A(M P_0, string P_1)
		{
			Directory.CreateDirectory(P_1);
			string text = Path.Combine(P_1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EH());
			string text2 = text + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cj() + Guid.NewGuid().ToString(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CK()) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ck();
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(P_0));
				File.WriteAllBytes(text2, ProtectedData.Protect(bytes, P.m_A, DataProtectionScope.CurrentUser));
				if (File.Exists(text))
				{
					File.Replace(text2, text, null);
				}
				else
				{
					File.Move(text2, text);
				}
			}
			finally
			{
				if (File.Exists(text2))
				{
					File.Delete(text2);
				}
			}
		}
	}
	internal static class p
	{
		public static bool A(string P_0, string P_1)
		{
			if (!Version.TryParse(P_0, out var result) || !Version.TryParse(P_1, out var result2))
			{
				throw new ArgumentException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EI());
			}
			return A(result).CompareTo(A(result2)) < 0;
		}

		private static Version A(Version P_0)
		{
			return new Version(P_0.Major, P_0.Minor, Math.Max(0, P_0.Build), Math.Max(0, P_0.Revision));
		}

		public static bool A(string P_0)
		{
			Uri uri = L.a;
			if (Uri.TryCreate(P_0, UriKind.Absolute, out var result) && string.IsNullOrEmpty(result.UserInfo))
			{
				if (result.Scheme == uri.Scheme && result.Host.Equals(uri.Host, StringComparison.OrdinalIgnoreCase))
				{
					return result.Port == uri.Port;
				}
				return false;
			}
			return false;
		}
	}
}
