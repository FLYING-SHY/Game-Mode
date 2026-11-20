using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using NAudio.Wave;
using a;

namespace A
{
	internal sealed class Q : IDisposable
	{
		private static readonly object m_A = new object();

		private static readonly HashSet<Q> m_A = new HashSet<Q>();

		private readonly SoundPlayer m_A;

		private readonly object m_a = new object();

		private bool m_A;

		public static void A()
		{
			lock (Q.m_A)
			{
				foreach (Q item in Q.m_A)
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

		public Q(string P_0)
		{
			P_0 = Path.GetFullPath(P_0);
			if (!File.Exists(P_0))
			{
				throw new FileNotFoundException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cg(), P_0);
			}
			string extension = Path.GetExtension(P_0);
			if (extension.Equals(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CH(), StringComparison.OrdinalIgnoreCase))
			{
				P_0 = A(P_0);
			}
			else if (!extension.Equals(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ch(), StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CI());
			}
			this.m_A = new SoundPlayer(P_0);
			try
			{
				this.m_A.Load();
				lock (Q.m_A)
				{
					Q.m_A.Add(this);
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
				text = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ci(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
			}
			string text2 = Path.Combine(Path.GetTempPath(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CJ());
			Directory.CreateDirectory(text2);
			string text3 = Path.Combine(text2, text + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ch());
			if (File.Exists(text3))
			{
				return text3;
			}
			string text4 = text3 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cj() + Guid.NewGuid().ToString(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CK()) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ck();
			try
			{
				Mp3FileReader val = new Mp3FileReader(P_0);
				try
				{
					if (((Stream)(object)val).Length <= 0)
					{
						throw new InvalidOperationException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CL());
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
			A(global::a.N.a);
		}

		public void A(long P_0)
		{
			lock (this.m_a)
			{
				if (this.m_A || !global::a.N.A || global::a.N.a != P_0)
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
			lock (Q.m_A)
			{
				Q.m_A.Remove(this);
			}
		}
	}
	internal static class q
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static ResolveEventHandler A;

			internal Assembly A(object P_0, ResolveEventArgs P_1)
			{
				if (new AssemblyName(P_1.Name).Name != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xc())
				{
					return null;
				}
				using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XD());
				if (stream != null)
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						return Assembly.Load(memoryStream.ToArray());
					}
				}
				return null;
			}
		}

		private static bool m_A;

		public static void A()
		{
			if (q.m_A)
			{
				return;
			}
			q.m_A = true;
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object P_0, ResolveEventArgs P_1)
			{
				if (new AssemblyName(P_1.Name).Name != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xc())
				{
					return (Assembly)null;
				}
				using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XD());
				if (stream != null)
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						stream.CopyTo(memoryStream);
						return Assembly.Load(memoryStream.ToArray());
					}
				}
				return (Assembly)null;
			};
		}
	}
}
namespace a
{
	internal sealed class Q
	{
		public string A;

		public string a;

		public string B;

		public string b;

		public string C;

		public string c;

		public long A;

		public string D;

		public bool A;

		public int A;
	}
	internal static class q
	{
		internal const ulong A = 76561197960265728uL;

		internal static string A => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ei());

		internal static bool A(ulong P_0)
		{
			if (P_0 > 76561197960265728L)
			{
				return P_0 - 76561197960265728L <= uint.MaxValue;
			}
			return false;
		}

		internal static ulong? A(string P_0, DateTime P_1, DateTime P_2)
		{
			if (P_0 == null || P_0.Length > 65536 || P_2 < P_1)
			{
				return null;
			}
			using StringReader stringReader = new StringReader(P_0);
			string text = stringReader.ReadLine();
			if (text == null || !text.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EJ(), StringComparison.Ordinal) || text.Length < C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EJ().Length + 16 || !DateTime.TryParseExact(text.Substring(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EJ().Length, 16), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ej(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
			{
				return null;
			}
			if (result < P_1.AddMinutes(-1.0) || result > P_1.AddSeconds(5.0))
			{
				return null;
			}
			bool flag = false;
			ulong? num = null;
			string text2;
			while ((text2 = stringReader.ReadLine()) != null)
			{
				if (text2.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EK(), StringComparison.Ordinal))
				{
					flag = true;
				}
				int num2 = text2.IndexOf(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ek(), StringComparison.Ordinal);
				if (num2 >= 0)
				{
					string text3 = text2.Substring(num2 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ek().Length);
					if (!text3.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EL(), StringComparison.Ordinal))
					{
						return null;
					}
					text3 = text3.Substring(0, text3.Length - 1);
					if (text3.Length != 17 || !ulong.TryParse(text3, NumberStyles.None, CultureInfo.InvariantCulture, out var result2) || !A(result2) || (num.HasValue && num.Value != result2))
					{
						return null;
					}
					num = result2;
				}
			}
			return flag ? num : ((ulong?)null);
		}

		internal static ulong? A(string P_0, DateTime P_1)
		{
			try
			{
				DateTime lastWriteTime = File.GetLastWriteTime(P_0);
				if (lastWriteTime < P_1)
				{
					return null;
				}
				using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
				int num = (int)Math.Min(fileStream.Length, 65536L);
				byte[] array = new byte[num];
				int num2;
				int num3;
				for (num2 = 0; num2 < num; num2 += num3)
				{
					if ((num3 = fileStream.Read(array, num2, num - num2)) <= 0)
					{
						break;
					}
				}
				string text = Encoding.UTF8.GetString(array, 0, num2).TrimStart('\ufeff');
				if (fileStream.Length > num2)
				{
					int num4 = text.LastIndexOf('\n');
					if (num4 < 0)
					{
						return null;
					}
					text = text.Substring(0, num4 + 1);
				}
				if (File.GetLastWriteTime(P_0) < P_1)
				{
					return null;
				}
				return A(text, P_1, lastWriteTime);
			}
			catch (IOException)
			{
				return null;
			}
			catch (UnauthorizedAccessException)
			{
				return null;
			}
		}
	}
}
