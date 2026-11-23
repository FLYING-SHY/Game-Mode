using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using b;

namespace A
{
	internal static class U
	{
		public static bool A(byte[] P_0, byte[] P_1)
		{
			if (P_0.Length != P_1.Length)
			{
				return false;
			}
			for (int num = 0; num < P_0.Length; num++)
			{
				if (P_0[num] != P_1[num])
				{
					return false;
				}
			}
			return true;
		}
	}
	internal static class u
	{
		[CompilerGenerated]
		private static class A
		{
			public static Func<string, bool> A;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<string, string> A;

			internal string A(string P_0)
			{
				return P_0;
			}
		}

		private static readonly Dictionary<string, string> m_A = A();

		private static Dictionary<string, string> A()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Cz());
			if (stream == null)
			{
				throw new InvalidOperationException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cA());
			}
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split('\t');
				if (array.Length == 2)
				{
					dictionary[array[0]] = array[1];
				}
			}
			return dictionary;
		}

		private static string A(string P_0)
		{
			return (P_0 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).Replace('\\', '/').ToLowerInvariant();
		}

		public static bool a(string P_0)
		{
			return u.m_A.ContainsKey(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ca() + A(P_0));
		}

		public static bool B(string P_0)
		{
			string text = A(P_0);
			string[] array = text.Split('/');
			if (array.Length >= 5)
			{
				if (!u.m_A.ContainsKey(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cB() + text))
				{
					return u.m_A.ContainsKey(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cB() + array[3] + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cb() + Regex.Replace(array.Last(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cC(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()));
				}
				return true;
			}
			return false;
		}

		public static string A(global::B.O P_0)
		{
			return string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR(), from result in P_0.A.Where(a)
				orderby result
				select result);
		}

		public static string a(global::B.O P_0)
		{
			List<string> list = new List<string>();
			foreach (string item in P_0.A)
			{
				string[] array = A(item).Split('/');
				if (array.Length >= 5 && !(array[2] != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cc()) && u.m_A.TryGetValue(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cD() + array[1] + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cb() + array[3], out var value))
				{
					list.Add(value.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cd()) ? value.Substring(0, value.Length - 3) : value);
				}
			}
			if (list.Count != 0)
			{
				return string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cE(), list.Distinct());
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ce();
		}

		public static string b(string P_0)
		{
			string text = A(P_0);
			if (u.m_A.TryGetValue(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cF() + text, out var value))
			{
				return value;
			}
			string[] array = text.Split('/');
			string text2 = array.Last();
			string text3 = ((array.Length > 3) ? array[3] : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
			if (u.m_A.TryGetValue(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cf() + text3 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cb() + text2, out value))
			{
				return value;
			}
			string text4 = Regex.Replace(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cC(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
			if (u.m_A.TryGetValue(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cf() + text3 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cb() + text4, out value))
			{
				return value;
			}
			Match match = Regex.Match(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cG());
			if (match.Success)
			{
				return ((match.Groups[1].Value == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cg()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ch() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cH()) + match.Groups[2].Value + ((match.Groups[3].Value == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cI()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cj() : ((match.Groups[3].Value == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ci()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cJ() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D())) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cK();
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ck();
		}
	}
}
namespace a
{
	internal static class U
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::b.A, bool> A;

			internal bool A(global::b.A P_0)
			{
				if (P_0.A && !P_0.a)
				{
					return !string.IsNullOrEmpty(P_0.a);
				}
				return false;
			}
		}

		public static void A(Graphics P_0, IEnumerable<global::b.A> P_1, z P_2, global::B.C P_3, int P_4, int P_5)
		{
			if (!P_3.A)
			{
				return;
			}
			using StringFormat format = new StringFormat
			{
				Alignment = StringAlignment.Center,
				LineAlignment = StringAlignment.Far
			};
			using FontFamily family = new FontFamily(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB());
			foreach (global::b.A item in P_1.Where((global::b.A a2) => a2.A && !a2.a && !string.IsNullOrEmpty(a2.a)))
			{
				if (!P_2.A(item.A, item.B, P_4, P_5, out var origin))
				{
					continue;
				}
				origin.Y -= (float)P_3.B / 2f + 3f;
				using GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddString(item.a, family, 1, P_3.A, origin, format);
				using Pen pen = new Pen(Color.Black, 2.5f)
				{
					LineJoin = LineJoin.Round
				};
				using SolidBrush brush = new SolidBrush(item.A);
				P_0.DrawPath(pen, graphicsPath);
				P_0.FillPath(brush, graphicsPath);
			}
		}
	}
	internal sealed class u
	{
		public Bitmap A;

		public Rectangle A;
	}
}
