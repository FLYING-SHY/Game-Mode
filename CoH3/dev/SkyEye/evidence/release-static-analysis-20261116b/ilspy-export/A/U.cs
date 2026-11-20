using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
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
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CZ());
			if (stream == null)
			{
				throw new InvalidOperationException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Cz());
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
			return (P_0 ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()).Replace('\\', '/').ToLowerInvariant();
		}

		public static bool a(string P_0)
		{
			return U.m_A.ContainsKey(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cA() + A(P_0));
		}

		public static bool B(string P_0)
		{
			string text = A(P_0);
			string[] array = text.Split('/');
			if (array.Length >= 5)
			{
				if (!U.m_A.ContainsKey(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ca() + text))
				{
					return U.m_A.ContainsKey(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ca() + array[3] + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cB() + Regex.Replace(array.Last(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cb(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()));
				}
				return true;
			}
			return false;
		}

		public static string A(global::B.N P_0)
		{
			return string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bq(), from result in P_0.A.Where(a)
				orderby result
				select result);
		}

		public static string a(global::B.N P_0)
		{
			List<string> list = new List<string>();
			foreach (string item in P_0.A)
			{
				string[] array = A(item).Split('/');
				if (array.Length >= 5 && !(array[2] != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cC()) && U.m_A.TryGetValue(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cc() + array[1] + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cB() + array[3], out var value))
				{
					list.Add(value.EndsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cD()) ? value.Substring(0, value.Length - 3) : value);
				}
			}
			if (list.Count != 0)
			{
				return string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cd(), list.Distinct());
			}
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cE();
		}

		public static string b(string P_0)
		{
			string text = A(P_0);
			if (U.m_A.TryGetValue(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ce() + text, out var value))
			{
				return value;
			}
			string[] array = text.Split('/');
			string text2 = array.Last();
			string text3 = ((array.Length > 3) ? array[3] : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
			if (U.m_A.TryGetValue(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cF() + text3 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cB() + text2, out value))
			{
				return value;
			}
			string text4 = Regex.Replace(text2, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cb(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
			if (U.m_A.TryGetValue(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cF() + text3 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cB() + text4, out value))
			{
				return value;
			}
			Match match = Regex.Match(text2, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cf());
			if (match.Success)
			{
				return ((match.Groups[1].Value == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cG()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cH() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cg()) + match.Groups[2].Value + ((match.Groups[3].Value == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ch()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cJ() : ((match.Groups[3].Value == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cI()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ci() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D())) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cj();
			}
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cK();
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
