using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;
using Microsoft.Win32;
using a;
using b;

namespace B
{
	internal static class F
	{
		private struct A
		{
			public double A;

			public double a;

			public double B;
		}

		private sealed class a
		{
			public global::b.A A;

			public PointF A;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class B
		{
			public static readonly B A = new B();

			public static Func<global::b.A, string> A;

			public static Func<IGrouping<string, global::b.A>, string> A;

			public static Func<IGrouping<string, global::b.A>, string> a;

			public static Func<a, uint> A;

			public static Func<a, float> A;

			public static Func<a, float> a;

			public static Func<a, global::b.A> A;

			public static Func<a, float> B;

			public static Func<a, float> b;

			internal string A(global::b.A P_0)
			{
				return P_0.a ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}

			internal string A(IGrouping<string, global::b.A> P_0)
			{
				return P_0.Key;
			}

			internal string a(IGrouping<string, global::b.A> P_0)
			{
				return P_0.Key + ((P_0.Count() > 1) ? (CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA() + P_0.Count()) : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
			}

			internal uint A(a P_0)
			{
				return P_0.A.A;
			}

			internal float a(a P_0)
			{
				return P_0.A.Y;
			}

			internal float B(a P_0)
			{
				return P_0.A.X;
			}

			internal global::b.A b(a P_0)
			{
				return P_0.A;
			}

			internal float C(a P_0)
			{
				return P_0.A.X;
			}

			internal float c(a P_0)
			{
				return P_0.A.Y;
			}
		}

		[CompilerGenerated]
		private sealed class b
		{
			public float A;
		}

		[CompilerGenerated]
		private sealed class C
		{
			public a A;

			public b A;

			internal bool A(a P_0)
			{
				if (!((P_0.A.A.A - this.A.A.A.A) * (P_0.A.A.A - this.A.A.A.A) + (P_0.A.A.B - this.A.A.A.B) * (P_0.A.A.B - this.A.A.A.B) <= 64f))
				{
					if (Math.Abs(P_0.A.X - this.A.A.X) <= this.A.A)
					{
						return Math.Abs(P_0.A.Y - this.A.A.Y) <= this.A.A;
					}
					return false;
				}
				return true;
			}
		}

		[CompilerGenerated]
		private sealed class c
		{
			public PointF A;

			internal bool A(PointF P_0)
			{
				return Math.Abs(P_0.X - this.A.X) + Math.Abs(P_0.Y - this.A.Y) < 1f;
			}
		}

		public static float[] A(float P_0, float P_1, float P_2)
		{
			return new float[2]
			{
				Math.Max(0.01f, P_0 / P_2),
				Math.Max(0.01f, P_1 / P_2)
			};
		}

		private static A A(global::a.w P_0, float[] P_1)
		{
			double num = P_1[0];
			double num2 = P_1[1];
			double num3 = P_1[2];
			double num4 = num * (double)P_0.A[0] + num2 * (double)P_0.A[4] + num3 * (double)P_0.A[8] + (double)P_0.A[12];
			double num5 = num * (double)P_0.A[1] + num2 * (double)P_0.A[5] + num3 * (double)P_0.A[9] + (double)P_0.A[13];
			double num6 = num * (double)P_0.A[2] + num2 * (double)P_0.A[6] + num3 * (double)P_0.A[10] + (double)P_0.A[14];
			return new A
			{
				A = num4 * (double)P_0.a[0] + num5 * (double)P_0.a[4] + num6 * (double)P_0.a[8] + (double)P_0.a[12],
				a = num4 * (double)P_0.a[1] + num5 * (double)P_0.a[5] + num6 * (double)P_0.a[9] + (double)P_0.a[13],
				B = num4 * (double)P_0.a[3] + num5 * (double)P_0.a[7] + num6 * (double)P_0.a[11] + (double)P_0.a[15]
			};
		}

		private static bool A(double P_0, double P_1, ref double P_2, ref double P_3)
		{
			if (double.IsNaN(P_0) || double.IsInfinity(P_0) || double.IsNaN(P_1) || double.IsInfinity(P_1))
			{
				return false;
			}
			if (P_0 < 0.0 && P_1 < 0.0)
			{
				return false;
			}
			if (P_0 >= 0.0 && P_1 >= 0.0)
			{
				return true;
			}
			double val = P_0 / (P_0 - P_1);
			if (P_0 < 0.0)
			{
				P_2 = Math.Max(P_2, val);
			}
			else
			{
				P_3 = Math.Min(P_3, val);
			}
			return P_2 <= P_3;
		}

		public static bool A(global::a.w P_0, float[] P_1, float[] P_2, int P_3, int P_4, out PointF P_5, out PointF P_6)
		{
			P_5 = (P_6 = (P_6 = default(PointF)));
			A a2 = A(P_0, P_1);
			A a3 = A(P_0, P_2);
			double num = 0.0;
			double num2 = 1.0;
			if (!A(a2.B - 0.05, a3.B - 0.05, ref num, ref num2) || !A(a2.A + a2.B, a3.A + a3.B, ref num, ref num2) || !A(a2.B - a2.A, a3.B - a3.A, ref num, ref num2) || !A(a2.a + a2.B, a3.a + a3.B, ref num, ref num2) || !A(a2.B - a2.a, a3.B - a3.a, ref num, ref num2))
			{
				return false;
			}
			double num3 = a2.B + (a3.B - a2.B) * num;
			double num4 = a2.B + (a3.B - a2.B) * num2;
			if (num3 < 0.049999 || num4 < 0.049999)
			{
				return false;
			}
			P_5 = new PointF((float)((1.0 + (a2.A + (a3.A - a2.A) * num) / num3) * (double)P_3 / 2.0), (float)((1.0 - (a2.a + (a3.a - a2.a) * num) / num3) * (double)P_4 / 2.0));
			P_6 = new PointF((float)((1.0 + (a2.A + (a3.A - a2.A) * num2) / num4) * (double)P_3 / 2.0), (float)((1.0 - (a2.a + (a3.a - a2.a) * num2) / num4) * (double)P_4 / 2.0));
			return true;
		}

		private static global::A.y A(Color P_0, PointF P_1, PointF P_2, float P_3)
		{
			global::A.y y2 = new global::A.y();
			y2.A = P_0;
			y2.A = P_3;
			y2.A = new PointF[2] { P_1, P_2 };
			y2.A = Rectangle.FromLTRB((int)Math.Min(P_1.X, P_2.X) - 16, (int)Math.Min(P_1.Y, P_2.Y) - 16, (int)Math.Max(P_1.X, P_2.X) + 17, (int)Math.Max(P_1.Y, P_2.Y) + 17);
			return y2;
		}

		public static string A(IEnumerable<global::b.A> P_0)
		{
			return string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dt(), from grouping in (from a2 in P_0
					group a2 by a2.a ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).OrderBy((IGrouping<string, global::b.A> grouping) => grouping.Key, StringComparer.Ordinal)
				select grouping.Key + ((grouping.Count() > 1) ? (CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA() + grouping.Count()) : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()));
		}

		public static List<global::A.y> A(IEnumerable<global::b.A> P_0, global::a.w P_1, global::b.c P_2, int P_3, int P_4, Action<global::b.A, PointF> P_5)
		{
			List<global::A.y> list = new List<global::A.y>();
			if (!P_2.a)
			{
				return list;
			}
			List<a> list2 = new List<a>();
			float num = (float)P_2.A;
			float num2 = (float)P_2.A / 2f;
			foreach (global::b.A item in P_0)
			{
				e e2 = item.A;
				if (e2 != null)
				{
					if (F.A(P_1, new float[3]
					{
						item.A,
						item.a + 0.2f,
						item.B
					}, new float[3]
					{
						e2.A,
						e2.a + 0.2f,
						e2.B
					}, P_3, P_4, out var pointF, out var pointF2))
					{
						global::A.y y2 = F.A(item.A, pointF, pointF2, num);
						y2.A = true;
						y2.a = (float)P_2.a;
						y2.B = (float)P_2.B;
						list.Add(y2);
					}
					if (P_1.A(e2.A, e2.a + 0.2f, e2.B, P_3, P_4, out var pointF3))
					{
						list2.Add(new a
						{
							A = item,
							A = pointF3
						});
					}
				}
			}
			List<a> list3 = new List<a>(from a3 in list2
				orderby a3.A.A, a3.A.Y, a3.A.X
				select a3);
			float A = Math.Max(36, P_2.A + 12);
			while (list3.Count > 0)
			{
				List<a> list4 = new List<a> { list3[0] };
				list3.RemoveAt(0);
				bool flag;
				do
				{
					flag = false;
					for (int num3 = list3.Count - 1; num3 >= 0; num3--)
					{
						a A2 = list3[num3];
						if (A2.A.A == list4[0].A.A && !(A2.A.A != list4[0].A.A) && list4.Any((a a3) => (a3.A.A.A - A2.A.A.A) * (a3.A.A.A - A2.A.A.A) + (a3.A.A.B - A2.A.A.B) * (a3.A.A.B - A2.A.A.B) <= 64f || (Math.Abs(a3.A.X - A2.A.X) <= A && Math.Abs(a3.A.Y - A2.A.Y) <= A)))
						{
							list4.Add(A2);
							list3.RemoveAt(num3);
							flag = true;
						}
					}
				}
				while (flag);
				List<PointF> list5 = new List<PointF>();
				foreach (a item2 in list4)
				{
					PointF A3 = item2.A;
					if (!list5.Any((PointF pointF4) => Math.Abs(pointF4.X - A3.X) + Math.Abs(pointF4.Y - A3.Y) < 1f))
					{
						list5.Add(A3);
						list.Add(F.A(item2.A.A, new PointF(A3.X - num2, A3.Y - num2), new PointF(A3.X + num2, A3.Y + num2), num));
						list.Add(F.A(item2.A.A, new PointF(A3.X - num2, A3.Y + num2), new PointF(A3.X + num2, A3.Y - num2), num));
					}
				}
				if (P_5 != null)
				{
					global::b.A a2 = list4[0].A;
					P_5(new global::b.A
					{
						a = F.A(list4.Select((a a3) => a3.A)),
						A = a2.A,
						A = a2.A,
						A = true
					}, new PointF(list4.Average((a a3) => a3.A.X), list4.Min((a a3) => a3.A.Y) - num2 - 4f));
				}
			}
			return list;
		}
	}
	internal static class f
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<int, int> A;

			internal int A(int P_0)
			{
				return P_0;
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public Size A;

			public int A;

			public PointF A;

			public int a;

			public Rectangle A;

			public Func<int, bool> A;

			public Func<int, int> A;

			public Func<int, int> a;

			public Func<Rectangle, bool> A;

			internal bool A(int P_0)
			{
				if (P_0 >= 4)
				{
					return P_0 + this.A.Height <= this.A - 4;
				}
				return false;
			}

			internal int a(int P_0)
			{
				return (!((float)(P_0 + this.A.Height) <= this.A.Y)) ? 1 : 0;
			}

			internal int B(int P_0)
			{
				return Math.Abs(P_0 - this.a);
			}

			internal bool A(Rectangle P_0)
			{
				return Rectangle.Inflate(P_0, 2, 2).IntersectsWith(this.A);
			}
		}

		public static Rectangle A(Size P_0, PointF P_1, int P_2, int P_3, IEnumerable<Rectangle> P_4)
		{
			int num = Math.Max(4, Math.Min(Math.Max(4, P_2 - P_0.Width - 4), (int)Math.Round(P_1.X - (float)P_0.Width / 2f)));
			int a2 = Math.Max(4, Math.Min(Math.Max(4, P_3 - P_0.Height - 4), (int)Math.Round(P_1.Y - (float)P_0.Height)));
			Rectangle[] array = P_4.ToArray();
			List<int> list = new List<int> { a2 };
			Rectangle[] array2 = array;
			for (int num2 = 0; num2 < array2.Length; num2++)
			{
				Rectangle rectangle = array2[num2];
				list.Add(rectangle.Top - P_0.Height - 4);
				list.Add(rectangle.Bottom + 4);
			}
			Rectangle A = default(Rectangle);
			foreach (int item in (from num3 in list.Where((int num3) => num3 >= 4 && num3 + P_0.Height <= P_3 - 4).Distinct()
				orderby (!((float)(num3 + P_0.Height) <= P_1.Y)) ? 1 : 0, Math.Abs(num3 - a2)
				select num3).ThenBy((int result) => result))
			{
				ref Rectangle reference = ref A;
				reference = new Rectangle(num, item, P_0.Width, P_0.Height);
				if (!array.Any((Rectangle rect) => Rectangle.Inflate(rect, 2, 2).IntersectsWith(A)))
				{
					return A;
				}
			}
			return new Rectangle(num, a2, P_0.Width, P_0.Height);
		}
	}
}
namespace b
{
	internal static class F
	{
		private sealed class A
		{
			private readonly string m_A;

			private int m_A;

			private int m_a;

			private bool m_A;

			private bool A(string P_0, string P_1)
			{
				if (!this.m_A)
				{
					return P_0 == P_1;
				}
				return false;
			}

			public A(string P_0)
			{
				this.m_A = P_0 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				if (this.m_A.Length > 2097152)
				{
					throw new FormatException();
				}
			}

			private string A()
			{
				this.m_A = false;
				if (++this.m_a > 200000)
				{
					throw new FormatException();
				}
				while (this.m_A < this.m_A.Length)
				{
					if (char.IsWhiteSpace(this.m_A[this.m_A]) || this.m_A[this.m_A] == '\ufeff')
					{
						this.m_A++;
						continue;
					}
					if (this.m_A[this.m_A] != '/' || this.m_A + 1 >= this.m_A.Length || this.m_A[this.m_A + 1] != '/')
					{
						break;
					}
					while (this.m_A < this.m_A.Length && this.m_A[this.m_A] != '\n')
					{
						this.m_A++;
					}
				}
				if (this.m_A == this.m_A.Length)
				{
					return null;
				}
				char c2 = this.m_A[this.m_A++];
				if (c2 == '{' || c2 == '}')
				{
					return c2.ToString();
				}
				StringBuilder stringBuilder = new StringBuilder();
				if (c2 == '"')
				{
					this.m_A = true;
					bool flag = false;
					while (this.m_A < this.m_A.Length)
					{
						c2 = this.m_A[this.m_A++];
						switch (c2)
						{
						case '\\':
							if (this.m_A < this.m_A.Length)
							{
								char c3 = this.m_A[this.m_A++];
								switch (c3)
								{
								case 'n':
									c2 = '\n';
									break;
								case 'r':
									c2 = '\r';
									break;
								case 't':
									c2 = '\t';
									break;
								case '"':
								case '\\':
									c2 = c3;
									break;
								default:
									stringBuilder.Append('\\');
									c2 = c3;
									break;
								}
							}
							goto IL_0203;
						case '"':
							break;
						default:
							goto IL_0203;
						}
						flag = true;
						break;
						IL_0203:
						stringBuilder.Append(c2);
						if (stringBuilder.Length > 4096)
						{
							throw new FormatException();
						}
					}
					if (!flag)
					{
						throw new FormatException();
					}
				}
				else
				{
					stringBuilder.Append(c2);
					while (this.m_A < this.m_A.Length && !char.IsWhiteSpace(this.m_A[this.m_A]) && this.m_A[this.m_A] != '{' && this.m_A[this.m_A] != '}')
					{
						stringBuilder.Append(this.m_A[this.m_A++]);
						if (stringBuilder.Length > 4096)
						{
							throw new FormatException();
						}
					}
				}
				return stringBuilder.ToString();
			}

			private void A(int P_0)
			{
				if (P_0 > 32)
				{
					throw new FormatException();
				}
				string text;
				while (!A(text = A(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
				{
					if (text == null || A(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()))
					{
						throw new FormatException();
					}
					string text2 = A();
					if (A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()))
					{
						A(P_0 + 1);
					}
					else if (text2 == null || A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
					{
						throw new FormatException();
					}
				}
			}

			private string a()
			{
				string result = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				string text;
				while (!A(text = A(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
				{
					if (text == null || A(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()))
					{
						throw new FormatException();
					}
					string text2 = A();
					if (A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()))
					{
						A(1);
						continue;
					}
					if (text2 == null || A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
					{
						throw new FormatException();
					}
					if (string.Equals(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YK(), StringComparison.OrdinalIgnoreCase))
					{
						result = F.a(text2);
					}
				}
				return result;
			}

			public List<Dictionary<string, object>> B()
			{
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>(StringComparer.Ordinal);
				string text;
				while ((text = A()) != null)
				{
					if (A(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()) || A(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
					{
						throw new FormatException();
					}
					string text2 = A();
					if (text2 == null || A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
					{
						throw new FormatException();
					}
					if (!A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()))
					{
						continue;
					}
					if (!string.Equals(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yk(), StringComparison.OrdinalIgnoreCase))
					{
						A(1);
						continue;
					}
					while (!A(text = A(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
					{
						if (text == null || A(text, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()))
						{
							throw new FormatException();
						}
						text2 = A();
						if (text2 == null || A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yj()))
						{
							throw new FormatException();
						}
						if (A(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YJ()))
						{
							if (F.A(text))
							{
								sortedDictionary[text] = a();
							}
							else
							{
								A(1);
							}
						}
					}
				}
				List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
				foreach (KeyValuePair<string, string> item in sortedDictionary)
				{
					list.Add(new Dictionary<string, object>
					{
						{
							CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dj(),
							item.Key
						},
						{
							CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YL(),
							item.Value
						}
					});
				}
				return list;
			}
		}

		private const int m_A = 2097152;

		private const int m_a = 24;

		private static readonly object m_A = new object();

		private static List<Dictionary<string, object>> m_A = new List<Dictionary<string, object>>();

		private static DateTime m_A;

		private static int m_B;

		internal static bool A(string P_0)
		{
			if (P_0 == null || P_0.Length != 17)
			{
				return false;
			}
			foreach (char c2 in P_0)
			{
				if (c2 < '0' || c2 > '9')
				{
					return false;
				}
			}
			if (ulong.TryParse(P_0, out var result) && result > 76561197960265728L)
			{
				return result <= 76561202255233023L;
			}
			return false;
		}

		internal static string a(string P_0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = P_0 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			foreach (char value in text)
			{
				if (!char.IsControl(value))
				{
					stringBuilder.Append(value);
				}
			}
			if (stringBuilder.Length > 128)
			{
				stringBuilder.Length = 128;
				if (char.IsHighSurrogate(stringBuilder[127]))
				{
					stringBuilder.Length = 127;
				}
			}
			return stringBuilder.ToString();
		}

		internal static List<Dictionary<string, object>> B(string P_0)
		{
			return new A(P_0).B();
		}

		private static string b(string P_0)
		{
			using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			if (fileStream.Length > 2097152)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			byte[] array = new byte[2097153];
			int n;
			int num;
			for (n = 0; n < array.Length; n += num)
			{
				if ((num = fileStream.Read(array, n, array.Length - n)) <= 0)
				{
					break;
				}
			}
			if (n > 2097152)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
			return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(array, 0, n);
		}

		private static List<Dictionary<string, object>> A()
		{
			List<string> list = new List<string>();
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ld());
				if (registryKey != null)
				{
					list.Add(registryKey.GetValue(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LE()) as string);
				}
			}
			catch
			{
			}
			list.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Le()));
			list.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Le()));
			foreach (string item in list)
			{
				if (string.IsNullOrWhiteSpace(item))
				{
					continue;
				}
				try
				{
					string text = Path.Combine(item, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LF(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lf());
					if (File.Exists(text))
					{
						return B(b(text));
					}
				}
				catch
				{
				}
			}
			return new List<Dictionary<string, object>>();
		}

		internal static object[] A(List<Dictionary<string, object>> P_0, ref int P_1)
		{
			if (P_0.Count == 0)
			{
				P_1 = 0;
				return new object[0];
			}
			P_1 %= P_0.Count;
			int num = Math.Min(24, P_0.Count - P_1);
			object[] array = new object[num];
			for (int n = 0; n < num; n++)
			{
				array[n] = P_0[P_1 + n];
			}
			P_1 = (P_1 + num) % P_0.Count;
			return array;
		}

		public static void A(Dictionary<string, object> P_0)
		{
			lock (F.m_A)
			{
				if (DateTime.UtcNow >= F.m_A)
				{
					F.m_A = DateTime.UtcNow.AddMinutes(1.0);
					F.m_A = A();
				}
				object[] array = A(F.m_A, ref F.m_B);
				if (array.Length != 0)
				{
					P_0[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LG()] = array;
				}
			}
		}
	}
	internal sealed class f : Panel
	{
		private readonly TextBox m_A = new TextBox();

		private readonly decimal m_A;

		private readonly decimal m_a;

		private readonly decimal m_B;

		private readonly int m_A;

		private decimal m_b;

		private bool m_A;

		private readonly Timer m_A = new Timer
		{
			Interval = 450
		};

		public Action<decimal> A;

		public decimal A
		{
			get
			{
				return this.m_b;
			}
			set
			{
				decimal num = decimal.Round(Math.Max(this.m_A, Math.Min(this.m_a, val)), this.m_A);
				bool flag = this.m_b != num;
				this.m_b = num;
				this.m_A = true;
				try
				{
					this.m_A.Text = num.ToString((this.m_A > 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lg() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By());
				}
				finally
				{
					this.m_A = false;
				}
				if (flag && this.A != null)
				{
					this.A(num);
				}
			}
		}

		public f(decimal P_0, decimal P_1, decimal P_2, int P_3)
		{
			this.m_A = P_0;
			this.m_a = P_1;
			this.m_A = P_3;
			this.m_B = ((P_3 > 0) ? 0.1m : 1m);
			base.Size = new Size(74, 26);
			DoubleBuffered = true;
			BackColor = global::B.L.A;
			this.m_A.BorderStyle = BorderStyle.None;
			this.m_A.BackColor = global::B.L.A;
			this.m_A.ForeColor = global::B.L.b;
			this.m_A.Font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LH(), 13f, FontStyle.Regular, GraphicsUnit.Pixel);
			this.m_A.Location = new Point(6, 5);
			this.m_A.Width = 47;
			this.m_A.TextAlign = HorizontalAlignment.Left;
			base.Controls.Add(this.m_A);
			global::A.D d2 = new global::A.D
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fx(),
				Location = new Point(55, 0),
				Size = new Size(18, 13),
				Font = global::a.J.A(7f)
			};
			global::A.D d3 = new global::A.D
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lh(),
				Location = new Point(55, 13),
				Size = new Size(18, 13),
				Font = global::a.J.A(7f)
			};
			base.Controls.Add(d2);
			base.Controls.Add(d3);
			d2.Click += delegate
			{
				this.A = this.m_b + this.m_B;
			};
			d3.Click += delegate
			{
				this.A = this.m_b - this.m_B;
			};
			this.m_A.Validated += delegate
			{
				B();
			};
			this.m_A.KeyDown += delegate(object obj, KeyEventArgs e2)
			{
				if (e2.KeyCode == Keys.Return)
				{
					B();
					e2.SuppressKeyPress = true;
				}
				if (e2.KeyCode == Keys.Up || e2.KeyCode == Keys.Down)
				{
					B();
					this.A = this.m_b + ((e2.KeyCode == Keys.Up) ? this.m_B : (-this.m_B));
					e2.SuppressKeyPress = true;
				}
			};
			this.m_A.Tick += delegate
			{
				this.m_A.Stop();
				a();
			};
			this.m_A.TextChanged += delegate
			{
				if (!this.m_A)
				{
					this.m_A.Stop();
					this.m_A.Start();
				}
			};
			this.A = P_2;
		}

		private void a()
		{
			if (!decimal.TryParse(this.m_A.Text, out var result) || result < this.m_A || result > this.m_a)
			{
				return;
			}
			result = decimal.Round(result, this.m_A);
			if (!(result == this.m_b))
			{
				this.m_b = result;
				if (this.A != null)
				{
					this.A(result);
				}
			}
		}

		protected override void Dispose(bool P_0)
		{
			if (P_0)
			{
				this.m_A.Stop();
				this.m_A.Dispose();
			}
			base.Dispose(P_0);
		}

		private void B()
		{
			this.m_A.Stop();
			if (decimal.TryParse(this.m_A.Text, out var result))
			{
				this.A = result;
			}
			else
			{
				this.m_A.Text = this.m_b.ToString((this.m_A > 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lg() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By());
			}
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			base.OnPaint(P_0);
			using Pen pen = new Pen(global::B.L.c);
			P_0.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			this.A = this.m_b + this.m_B;
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			this.A = this.m_b - this.m_B;
		}

		[CompilerGenerated]
		private void B(object P_0, EventArgs P_1)
		{
			B();
		}

		[CompilerGenerated]
		private void A(object P_0, KeyEventArgs P_1)
		{
			if (P_1.KeyCode == Keys.Return)
			{
				B();
				P_1.SuppressKeyPress = true;
			}
			if (P_1.KeyCode == Keys.Up || P_1.KeyCode == Keys.Down)
			{
				B();
				this.A = this.m_b + ((P_1.KeyCode == Keys.Up) ? this.m_B : (-this.m_B));
				P_1.SuppressKeyPress = true;
			}
		}

		[CompilerGenerated]
		private void b(object P_0, EventArgs P_1)
		{
			this.m_A.Stop();
			a();
		}

		[CompilerGenerated]
		private void C(object P_0, EventArgs P_1)
		{
			if (!this.m_A)
			{
				this.m_A.Stop();
				this.m_A.Start();
			}
		}
	}
}
