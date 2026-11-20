using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using B;
using b;

namespace A
{
	internal static class U
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
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.CZ());
			if (stream == null)
			{
				throw new InvalidOperationException(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Cz());
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
			return (P_0 ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D()).Replace('\\', '/').ToLowerInvariant();
		}

		public static bool a(string P_0)
		{
			return U.m_A.ContainsKey(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cA() + A(P_0));
		}

		public static bool B(string P_0)
		{
			string text = A(P_0);
			string[] array = text.Split('/');
			if (array.Length >= 5)
			{
				if (!U.m_A.ContainsKey(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ca() + text))
				{
					return U.m_A.ContainsKey(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ca() + array[3] + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cB() + Regex.Replace(array.Last(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cb(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D()));
				}
				return true;
			}
			return false;
		}

		public static string A(global::B.N P_0)
		{
			return string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq(), from result in P_0.A.Where(a)
				orderby result
				select result);
		}

		public static string a(global::B.N P_0)
		{
			List<string> list = new List<string>();
			foreach (string item in P_0.A)
			{
				string[] array = A(item).Split('/');
				if (array.Length >= 5 && !(array[2] != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cC()) && U.m_A.TryGetValue(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cc() + array[1] + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cB() + array[3], out var value))
				{
					list.Add(value.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cD()) ? value.Substring(0, value.Length - 3) : value);
				}
			}
			if (list.Count != 0)
			{
				return string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cd(), list.Distinct());
			}
			return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cE();
		}

		public static string b(string P_0)
		{
			string text = A(P_0);
			if (U.m_A.TryGetValue(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ce() + text, out var value))
			{
				return value;
			}
			string[] array = text.Split('/');
			string text2 = array.Last();
			string text3 = ((array.Length > 3) ? array[3] : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
			if (U.m_A.TryGetValue(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cF() + text3 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cB() + text2, out value))
			{
				return value;
			}
			string text4 = Regex.Replace(text2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cb(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
			if (U.m_A.TryGetValue(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cF() + text3 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cB() + text4, out value))
			{
				return value;
			}
			Match match = Regex.Match(text2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cf());
			if (match.Success)
			{
				return ((match.Groups[1].Value == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cG()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cH() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cg()) + match.Groups[2].Value + ((match.Groups[3].Value == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ch()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cJ() : ((match.Groups[3].Value == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cI()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ci() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D())) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cj();
			}
			return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cK();
		}
	}
	internal static class u
	{
		public static bool A(this byte[] P_0, byte[] P_1)
		{
			if (P_0.Length == P_1.Length && P_0[0] == P_1[0])
			{
				return P_0[1] == P_1[1];
			}
			return false;
		}
	}
}
namespace a
{
	internal sealed class U
	{
		public int A;

		public long A;

		public long a;

		public uint A;

		public DateTime A;

		public string A;
	}
	internal sealed class u
	{
		public U A;

		public global::b.c[] A;

		public global::B.S[] A;

		public global::B.S[] a;

		public DateTime A;

		public string A;
	}
}
