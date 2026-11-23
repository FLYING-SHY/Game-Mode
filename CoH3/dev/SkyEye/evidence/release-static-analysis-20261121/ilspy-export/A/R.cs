using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;

namespace A
{
	internal static class R
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static ResolveEventHandler A;

			internal Assembly A(object P_0, ResolveEventArgs P_1)
			{
				if (new AssemblyName(P_1.Name).Name != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XH())
				{
					return null;
				}
				using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xh());
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
			if (R.m_A)
			{
				return;
			}
			R.m_A = true;
			AppDomain.CurrentDomain.AssemblyResolve += delegate(object P_0, ResolveEventArgs P_1)
			{
				if (new AssemblyName(P_1.Name).Name != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XH())
				{
					return (Assembly)null;
				}
				using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xh());
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
	internal static class r
	{
		private static readonly Dictionary<string, HashSet<string>> m_A;

		private static readonly Dictionary<string, string> m_A;

		public static IEnumerable<string> A => r.m_A.Keys;

		static r()
		{
			r.m_A = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			r.m_A = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CM());
			if (stream == null)
			{
				throw new InvalidOperationException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cm());
			}
			using StreamReader streamReader = new StreamReader(stream);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split('\t');
				if (array.Length == 3)
				{
					if (!r.m_A.TryGetValue(array[0], out var value))
					{
						HashSet<string> hashSet = (r.m_A[array[0]] = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
						value = hashSet;
					}
					value.Add(array[1]);
					r.m_A[array[0]] = array[2];
				}
			}
		}

		public static bool A(string P_0)
		{
			return r.m_A.ContainsKey(P_0);
		}

		public static bool A(string P_0, string P_1)
		{
			if (r.m_A.TryGetValue(P_0, out var value))
			{
				return value.Contains(P_1);
			}
			return false;
		}

		public static string a(string P_0)
		{
			if (!r.m_A.TryGetValue(P_0, out var value))
			{
				return P_0;
			}
			return value;
		}
	}
}
namespace a
{
	internal sealed class R
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
	internal static class r
	{
		internal const ulong A = 76561197960265728uL;

		internal static string A => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EM());

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
			if (text == null || !text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Em(), StringComparison.Ordinal) || text.Length < CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Em().Length + 16 || !DateTime.TryParseExact(text.Substring(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Em().Length, 16), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EN(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
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
				if (text2.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.En(), StringComparison.Ordinal))
				{
					flag = true;
				}
				int num2 = text2.IndexOf(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EO(), StringComparison.Ordinal);
				if (num2 >= 0)
				{
					string text3 = text2.Substring(num2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EO().Length);
					if (!text3.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Eo(), StringComparison.Ordinal))
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
