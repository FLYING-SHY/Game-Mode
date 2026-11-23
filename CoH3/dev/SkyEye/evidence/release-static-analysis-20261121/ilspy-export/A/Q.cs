using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using NAudio.Wave;
using a;

namespace A
{
	internal static class Q
	{
		private static readonly Dictionary<string, string> m_A = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cd(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CE()
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ce(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CF()
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cf(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CG()
			},
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cg(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CG()
			}
		};

		public static bool A(string P_0)
		{
			if (P_0 != null)
			{
				return Q.m_A.ContainsKey(P_0);
			}
			return false;
		}

		public static bool a(string P_0)
		{
			return P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cb();
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
			if (P_0 == null || !Q.m_A.TryGetValue(P_0, out var value))
			{
				if (!a(P_0))
				{
					return null;
				}
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CC();
			}
			return value;
		}

		public static bool C(string P_0)
		{
			if (!(P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cc()))
			{
				return P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CD();
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
	internal sealed class q : IDisposable
	{
		private static readonly object m_A = new object();

		private static readonly HashSet<q> m_A = new HashSet<q>();

		private readonly SoundPlayer m_A;

		private readonly object m_a = new object();

		private bool m_A;

		public static void A()
		{
			lock (q.m_A)
			{
				foreach (q item in q.m_A)
				{
					try
					{
						item.m_A.Stop();
					}
					catch (ObjectDisposedException)
					{
					}
					catch (InvalidOperationException)
					{
					}
				}
			}
		}

		public q(string P_0)
		{
			P_0 = Path.GetFullPath(P_0);
			if (!File.Exists(P_0))
			{
				throw new FileNotFoundException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CH(), P_0);
			}
			string extension = Path.GetExtension(P_0);
			if (extension.Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ch(), StringComparison.OrdinalIgnoreCase))
			{
				P_0 = A(P_0);
			}
			else if (!extension.Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CI(), StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ci());
			}
			this.m_A = new SoundPlayer(P_0);
			try
			{
				this.m_A.Load();
				lock (q.m_A)
				{
					q.m_A.Add(this);
				}
			}
			catch
			{
				this.m_A.Dispose();
				throw;
			}
		}

		private static string A(string P_0)
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Expected O, but got Unknown
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Expected O, but got Unknown
			string text;
			using (FileStream inputStream = File.OpenRead(P_0))
			{
				using SHA256 sHA = SHA256.Create();
				text = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CJ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
			}
			string text2 = Path.Combine(Path.GetTempPath(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cj());
			Directory.CreateDirectory(text2);
			string text3 = Path.Combine(text2, text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CI());
			if (File.Exists(text3))
			{
				return text3;
			}
			string text4 = text3 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CK() + Guid.NewGuid().ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ck()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CL();
			try
			{
				Mp3FileReader val = new Mp3FileReader(P_0);
				try
				{
					if (((Stream)(object)val).Length <= 0)
					{
						throw new InvalidOperationException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cl());
					}
					WaveFileWriter.CreateWaveFile(text4, (IWaveProvider)val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				using (SoundPlayer soundPlayer = new SoundPlayer(text4))
				{
					soundPlayer.Load();
				}
				if (!File.Exists(text3))
				{
					File.Move(text4, text3);
				}
				return text3;
			}
			finally
			{
				if (File.Exists(text4))
				{
					File.Delete(text4);
				}
			}
		}

		public void a()
		{
			A(global::a.O.a);
		}

		public void A(long P_0)
		{
			lock (this.m_a)
			{
				if (this.m_A || !global::a.O.A || global::a.O.a != P_0)
				{
					return;
				}
			}
			try
			{
				this.m_A.PlaySync();
			}
			catch (ObjectDisposedException)
			{
				lock (this.m_a)
				{
					if (!this.m_A)
					{
						throw;
					}
				}
			}
		}

		public void Dispose()
		{
			lock (this.m_a)
			{
				if (this.m_A)
				{
					return;
				}
				this.m_A = true;
				this.m_A.Stop();
				this.m_A.Dispose();
			}
			lock (q.m_A)
			{
				q.m_A.Remove(this);
			}
		}
	}
}
namespace a
{
	internal static class Q
	{
		private static readonly byte[] m_A = Encoding.UTF8.GetBytes(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EL());

		internal static string A => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ej(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EK());

		public static N a()
		{
			return A(Q.A);
		}

		internal static N A(string P_0)
		{
			try
			{
				byte[] bytes = ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ek())), Q.m_A, DataProtectionScope.CurrentUser);
				N n2 = new JavaScriptSerializer().Deserialize<N>(Encoding.UTF8.GetString(bytes));
				if (n2 != null && Guid.TryParse(n2.Installation, out var result) && result != Guid.Empty)
				{
					return n2;
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
			return new N
			{
				Installation = Guid.NewGuid().ToString(),
				Code = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()
			};
		}

		public static void A(N P_0)
		{
			A(P_0, Q.A);
		}

		internal static void A(N P_0, string P_1)
		{
			Directory.CreateDirectory(P_1);
			string text = Path.Combine(P_1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ek());
			string text2 = text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CK() + Guid.NewGuid().ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ck()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CL();
			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(P_0));
				File.WriteAllBytes(text2, ProtectedData.Protect(bytes, Q.m_A, DataProtectionScope.CurrentUser));
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
	internal static class q
	{
		public static bool A(string P_0, string P_1)
		{
			if (!Version.TryParse(P_0, out var result) || !Version.TryParse(P_1, out var result2))
			{
				throw new ArgumentException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.El());
			}
			return A(result).CompareTo(A(result2)) < 0;
		}

		private static Version A(Version P_0)
		{
			return new Version(P_0.Major, P_0.Minor, Math.Max(0, P_0.Build), Math.Max(0, P_0.Revision));
		}

		public static bool A(string P_0)
		{
			Uri uri = M.a;
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
