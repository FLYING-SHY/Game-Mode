using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
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

		public string A => this.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bt();

		public static bool A(string P_0)
		{
			return string.Equals(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bU(), StringComparison.OrdinalIgnoreCase);
		}

		public static bool a(string P_0)
		{
			if (!A(P_0))
			{
				if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bu()) && (P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bS()) || P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bV())))
				{
					if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bg()) && !P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bv()))
					{
						return P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bW());
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
				if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bg()))
				{
					if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bv()))
					{
						if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bw()))
						{
							if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bX()))
							{
								if (!P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bx()))
								{
									return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bY();
								}
								return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.by();
							}
							return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bZ();
						}
						return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bz();
					}
					return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CA();
				}
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bH();
			}
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ca();
		}
	}
	internal static class p
	{
		private static readonly Dictionary<string, string> m_A = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CD(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cd()
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CE(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ce()
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CF(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cf()
			},
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CG(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cf()
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
			return P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CB();
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
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cb();
			}
			return value;
		}

		public static bool C(string P_0)
		{
			if (!(P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CC()))
			{
				return P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cc();
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
		private static readonly byte[] m_A = Encoding.UTF8.GetBytes(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Eh());

		internal static string A => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EG(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Eg());

		public static M a()
		{
			return A(P.A);
		}

		internal static M A(string P_0)
		{
			try
			{
				byte[] bytes = ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EH())), P.m_A, DataProtectionScope.CurrentUser);
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
				Code = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()
			};
		}

		public static void A(M P_0)
		{
			A(P_0, P.A);
		}

		internal static void A(M P_0, string P_1)
		{
			Directory.CreateDirectory(P_1);
			string text = Path.Combine(P_1, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EH());
			string text2 = text + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cj() + Guid.NewGuid().ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CK()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ck();
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
				throw new ArgumentException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EI());
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
