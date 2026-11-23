using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using a;
using b;

namespace B;

internal static class N
{
	[STAThread]
	public static int A(string[] P_0)
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		if (P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IV()))
		{
			try
			{
				using (n n2 = new n())
				{
					List<O> list = n2.k();
					List<string> list2 = new List<string>
					{
						CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Iv() + list.Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IW(),
						CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Iw()
					};
					foreach (O item in list)
					{
						list2.Add(item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IX() + item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ix() + item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IY() + item.C + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Iy() + global::A.t.A(item) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IZ() + item.A.Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Iz() + item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iA() + item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ia() + item.B + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iB() + item.b + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ib() + global::A.u.a(item) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iC() + item.A.Count);
					}
					File.WriteAllLines(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ic()), list2);
				}
				return 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ic()), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fP() + ex);
				return 1;
			}
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		if (P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iD()))
		{
			using (m m2 = new m())
			{
				m2.ShowInTaskbar = false;
				m2.Opacity = 0.0;
				m2.Show();
				Application.DoEvents();
				using (Bitmap bitmap = new Bitmap(m2.Width, m2.Height))
				{
					m2.DrawToBitmap(bitmap, new Rectangle(0, 0, m2.Width, m2.Height));
					bitmap.Save(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.id()));
				}
				m2.Close();
			}
			return 0;
		}
		Application.Run(new m());
		return 0;
	}
}
internal sealed class n : IDisposable
{
	private sealed class A
	{
		public long A;

		public long a;

		public uint A;

		public uint a;

		public string A;

		public double A;

		public bool A;

		public bool a;

		public global::A.L A;
	}

	private struct a
	{
		public IntPtr A;

		public IntPtr a;

		public uint A;

		public ushort A;

		public UIntPtr A;

		public uint a;

		public uint B;

		public uint b;
	}

	private sealed class B
	{
		public long A;

		public long a;

		public uint A;

		public uint a;

		public int A;

		public ulong A;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class b
	{
		public static readonly b A = new b();

		public static Func<O, bool> A;

		public static Func<O, bool> a;

		public static Func<O, IEnumerable<global::b.d>> A;

		public static Func<O, int> A;

		public static Func<global::b.A, bool> A;

		public static Func<global::b.A, bool> a;

		public static Func<O, IEnumerable<global::b.d>> a;

		public static Func<O, T> A;

		public static Func<T, long> A;

		public static Func<IGrouping<long, T>, long> A;

		public static Func<IGrouping<long, T>, T> A;

		public static Func<KeyValuePair<long, DateTime>, long> A;

		public static Func<KeyValuePair<long, DateTime>, DateTime> A;

		public static Func<string, bool> A;

		public static Func<string, string[]> A;

		public static Func<string[], bool> A;

		public static Func<string[], string> A;

		public static Func<string, string> A;

		public static Func<T, long> a;

		public static Func<IGrouping<long, T>, long> a;

		public static Func<IGrouping<long, T>, T> a;

		public static Func<r, ulong> A;

		public static Func<r, bool> A;

		public static Func<r, uint> A;

		internal bool A(O P_0)
		{
			return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba();
		}

		internal bool a(O P_0)
		{
			return !P_0.b;
		}

		internal IEnumerable<global::b.d> B(O P_0)
		{
			return P_0.A;
		}

		internal int b(O P_0)
		{
			return P_0.A.Count;
		}

		internal bool A(global::b.A P_0)
		{
			return P_0.A != null;
		}

		internal bool a(global::b.A P_0)
		{
			return P_0.A != null;
		}

		internal IEnumerable<global::b.d> C(O P_0)
		{
			return P_0.A;
		}

		internal T c(O P_0)
		{
			return new T
			{
				A = P_0.A,
				A = P_0.A,
				A = P_0.A,
				A = P_0.a
			};
		}

		internal long A(T P_0)
		{
			return P_0.A;
		}

		internal long A(IGrouping<long, T> P_0)
		{
			return P_0.Key;
		}

		internal T a(IGrouping<long, T> P_0)
		{
			return P_0.First();
		}

		internal long A(KeyValuePair<long, DateTime> P_0)
		{
			KeyValuePair<long, DateTime> keyValuePair = P_0;
			return keyValuePair.Key;
		}

		internal DateTime a(KeyValuePair<long, DateTime> P_0)
		{
			return P_0.Value;
		}

		internal bool A(string P_0)
		{
			return P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xf());
		}

		internal string[] a(string P_0)
		{
			return P_0.Split('\\');
		}

		internal bool A(string[] P_0)
		{
			return P_0.Length >= 5;
		}

		internal string a(string[] P_0)
		{
			return P_0[3];
		}

		internal string B(string P_0)
		{
			if (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xG())
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xg();
			}
			if (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xH())
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FW();
			}
			return P_0;
		}

		internal long a(T P_0)
		{
			return P_0.A;
		}

		internal long B(IGrouping<long, T> P_0)
		{
			return P_0.Key;
		}

		internal T b(IGrouping<long, T> P_0)
		{
			return P_0.First();
		}

		internal ulong A(r P_0)
		{
			return P_0.A;
		}

		internal bool a(r P_0)
		{
			return P_0.B == null;
		}

		internal uint B(r P_0)
		{
			return P_0.A;
		}
	}

	[CompilerGenerated]
	private sealed class C
	{
		public n A;

		public long A;

		public long a;

		public long B;

		internal void A(object P_0)
		{
			long num = 0L;
			long num2 = 0L;
			Stopwatch stopwatch = Stopwatch.StartNew();
			string text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			bool flag = false;
			try
			{
				List<long> list = new List<long>();
				lock (this.A.m_A)
				{
					if (this.A != 0L)
					{
						list.Add(this.A);
					}
					long[] array = this.A.m_A.Where(delegate(KeyValuePair<long, DateTime> keyValuePair2)
					{
						DateTime utcNow = DateTime.UtcNow;
						KeyValuePair<long, DateTime> keyValuePair = keyValuePair2;
						if (utcNow - keyValuePair.Value < TimeSpan.FromSeconds(60.0))
						{
							keyValuePair = keyValuePair2;
							return keyValuePair.Key != this.A;
						}
						return false;
					}).Select(n.b.A.A).ToArray();
					if (array.Length != 0)
					{
						int num3 = this.A.m_A % array.Length;
						list.AddRange(array.Skip(num3).Concat(array.Take(num3)).Take(4));
						this.A.m_A = (num3 + 4) % array.Length;
					}
				}
				num = this.A.H();
				foreach (long item in list)
				{
					if (num != 0L)
					{
						break;
					}
					lock (this.A.m_A)
					{
						if (this.A.m_C || this.A.m_d != a)
						{
							return;
						}
					}
					try
					{
						global::a.z z2 = this.A.b(item);
						if ((double)z2.A < (double)z2.C * 0.5 && (double)z2.a > (double)z2.c * 0.45)
						{
							num = item;
							break;
						}
					}
					catch
					{
					}
				}
				long num4;
				lock (this.A.m_A)
				{
					if (this.A.m_C || this.A.m_d != a)
					{
						return;
					}
					if (!this.A.m_A.A)
					{
						long val = 0L;
						try
						{
							val = this.A.G(this.A.G(this.A.m_F + this.A.E(112837064L)) + 136) & -16777216;
						}
						catch
						{
						}
						this.A.m_A.B(Math.Max(65536L, val));
					}
					num4 = this.A.m_A.a;
				}
				byte[] array2 = new byte[65536];
				long num5 = this.A.m_F + this.A.E(65511760L);
				while (num == 0L && stopwatch.ElapsedMilliseconds < 80 && num2 < 33554432)
				{
					lock (this.A.m_A)
					{
						if (this.A.m_C || this.A.m_d != a)
						{
							return;
						}
					}
					if (n.A(this.A.m_A, new IntPtr(num4), out a a2, (UIntPtr)(ulong)Marshal.SizeOf(typeof(a))) == UIntPtr.Zero)
					{
						lock (this.A.m_A)
						{
							if (this.A.m_d != a)
							{
								return;
							}
							flag = this.A.m_A.C();
							if (flag)
							{
								if (this.A.m_B)
								{
									this.A.m_B = false;
									flag = false;
								}
								else
								{
									this.A.m_B = true;
								}
							}
							num4 = this.A.m_A.a;
						}
						break;
					}
					long num6 = Math.Max(num4, a2.A.ToInt64());
					long num7 = checked(a2.A.ToInt64() + (long)a2.A.ToUInt64());
					if (num7 <= num4)
					{
						break;
					}
					if (a2.a == 4096 && a2.b == 131072 && (a2.B & 0x101) == 0 && (a2.B & 0xEE) != 0 && (!this.A.m_B || a2.A.ToUInt64() <= 4194304))
					{
						long num8;
						for (num8 = num6; num8 < num7; num8 += array2.Length)
						{
							if (num != 0L)
							{
								break;
							}
							if (stopwatch.ElapsedMilliseconds >= 80)
							{
								break;
							}
							if (num2 >= 33554432)
							{
								break;
							}
							lock (this.A.m_A)
							{
								if (this.A.m_C || this.A.m_d != a)
								{
									return;
								}
							}
							int num9 = (int)Math.Min(array2.Length, num7 - num8);
							global::B.H.a(this.A.m_A, new IntPtr(num8), array2, (UIntPtr)(ulong)num9, out var uIntPtr);
							int num10 = (int)Math.Min((ulong)num9, uIntPtr.ToUInt64());
							num2 += num9;
							if (num10 < 8)
							{
								continue;
							}
							for (int num11 = 0; num11 + 8 <= num10; num11 += 8)
							{
								if (array2[num11] != (byte)num5 || BitConverter.ToInt64(array2, num11) != num5)
								{
									continue;
								}
								long num12 = num8 + num11;
								lock (this.A.m_A)
								{
									if (this.A.m_d != a)
									{
										return;
									}
									if (this.A.m_A.Count >= 64)
									{
										long key = this.A.m_A.OrderBy(n.b.A.a).First().Key;
										this.A.m_A.Remove(key);
									}
									this.A.m_A[num12] = DateTime.UtcNow;
								}
								try
								{
									global::a.z z3 = this.A.b(num12);
									if ((double)z3.A < (double)z3.C * 0.5 && (double)z3.a > (double)z3.c * 0.45)
									{
										num = num12;
										break;
									}
								}
								catch
								{
								}
							}
						}
						num4 = Math.Min(num8, num7);
					}
					else
					{
						num4 = num7;
					}
					lock (this.A.m_A)
					{
						if (this.A.m_d != a)
						{
							return;
						}
						flag = this.A.m_A.b(num4);
						if (flag)
						{
							if (this.A.m_B)
							{
								this.A.m_B = false;
								flag = false;
							}
							else
							{
								this.A.m_B = true;
							}
							num4 = this.A.m_A.a;
						}
					}
					if (flag)
					{
						break;
					}
				}
				lock (this.A.m_A)
				{
					if (!this.A.m_C && this.A.m_d == a)
					{
						this.A.m_c = num;
						if (num != 0L)
						{
							this.A.m_D = num;
							this.A.m_a = 0;
							this.A.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
						}
						else
						{
							this.A.m_a = DateTime.UtcNow.AddMilliseconds(flag ? 2000 : 100);
							this.A.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xh();
						}
					}
				}
				if (num != 0L)
				{
					this.A.A(num);
					this.A.c(num);
				}
			}
			catch (Exception ex)
			{
				text = ex.Message;
			}
			finally
			{
				lock (this.A.m_A)
				{
					if (this.A.m_d == a)
					{
						this.A.a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xI() + num.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xi() + this.A.m_A.a.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xJ() + num2 / 1048576 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xj() + stopwatch.ElapsedMilliseconds + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + text;
					}
					if (this.A.m_C == B)
					{
						this.A.m_b = false;
					}
				}
			}
		}

		internal bool A(KeyValuePair<long, DateTime> P_0)
		{
			DateTime utcNow = DateTime.UtcNow;
			KeyValuePair<long, DateTime> keyValuePair = P_0;
			if (utcNow - keyValuePair.Value < TimeSpan.FromSeconds(60.0))
			{
				keyValuePair = P_0;
				return keyValuePair.Key != this.A;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class c
	{
		public uint A;

		internal bool A(O P_0)
		{
			return P_0.A == this.A;
		}
	}

	[CompilerGenerated]
	private sealed class D
	{
		public uint A;

		public n A;

		internal bool A(uint P_0)
		{
			return P_0 >= this.A;
		}

		internal bool A(r P_0)
		{
			if (P_0.B == null)
			{
				return this.A.m_A[P_0.a].A == 0;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class d
	{
		public long A;

		internal bool A(global::b.d P_0)
		{
			return P_0.A == this.A;
		}
	}

	[CompilerGenerated]
	private sealed class E
	{
		public n A;

		public uint A;

		public double A;

		public Func<global::A.J, bool> A;

		internal bool A(global::A.L P_0)
		{
			return this.A.m_A.Contains(P_0.A);
		}

		internal bool A(uint P_0)
		{
			return P_0 >= this.A;
		}

		internal bool A(global::A.J P_0)
		{
			if (P_0.A != null && !P_0.a && this.A - P_0.A >= 600.0)
			{
				return !this.A.m_A.A(P_0.A);
			}
			return false;
		}

		internal bool A(long P_0)
		{
			F f2 = new F
			{
				A = P_0
			};
			return !this.A.m_A.Values.Any(f2.A);
		}
	}

	[CompilerGenerated]
	private sealed class e
	{
		public global::A.L A;

		internal bool A(global::A.L P_0)
		{
			if (P_0.A == this.A.A && P_0.a == this.A.a)
			{
				return Math.Abs(P_0.A - this.A.A) + Math.Abs(P_0.B - this.A.B) < 20f;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class F
	{
		public long A;

		internal bool A(A P_0)
		{
			return P_0.A == this.A;
		}
	}

	[CompilerGenerated]
	private sealed class f
	{
		public long A;

		internal bool A(O P_0)
		{
			if (P_0.a == this.A)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba();
			}
			return false;
		}
	}

	private long m_A;

	private ulong[] m_A = new ulong[0];

	private readonly Dictionary<uint, A> m_A = new Dictionary<uint, A>();

	private readonly global::A.F m_A = new global::A.F();

	private bool m_A;

	private readonly HashSet<string> m_A = new HashSet<string>();

	private global::a.S m_A;

	private DateTime m_A;

	private ulong? m_A;

	private readonly Dictionary<long, global::a.T> m_A = new Dictionary<long, global::a.T>();

	private bool m_a;

	private long m_a;

	private long m_B;

	private readonly global::B.d m_A = new global::B.d();

	private long m_b;

	private long m_C;

	private bool m_B = true;

	private int m_A;

	private readonly Dictionary<long, DateTime> m_A = new Dictionary<long, DateTime>();

	private readonly object m_A = new object();

	private long m_c;

	private long m_D;

	private long m_d;

	private string m_A;

	private bool m_b;

	private bool m_C;

	private DateTime m_a = DateTime.MinValue;

	private int m_a;

	public string a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

	public string B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JG();

	private long m_E;

	private long m_e = -1L;

	private bool m_c;

	private readonly Queue<long> m_A = new Queue<long>();

	private readonly HashSet<long> m_A = new HashSet<long>();

	private DateTime m_B;

	public bool D;

	public string b = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

	private readonly bool m_d;

	private readonly global::a.G.a m_A;

	[CompilerGenerated]
	private readonly global::a.G.A m_A;

	private readonly Process m_A;

	private readonly p m_A;

	private readonly long m_F;

	private readonly DateTime m_b;

	private readonly Dictionary<string, int> m_A = new Dictionary<string, int>();

	private uint m_A = uint.MaxValue;

	private uint m_a = uint.MaxValue;

	private readonly Dictionary<long, string> m_A = new Dictionary<long, string>();

	private long m_f;

	private ulong[] m_a = new ulong[0];

	private readonly Dictionary<uint, B> m_A = new Dictionary<uint, B>();

	private readonly Dictionary<ulong, string> m_A = new Dictionary<ulong, string>();

	private readonly U m_A = new U();

	private long m_G;

	private uint m_B;

	private long m_g;

	private DateTime m_C;

	public string C = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jg();

	[CompilerGenerated]
	private string m_c;

	internal global::a.G.A A
	{
		[CompilerGenerated]
		get
		{
			return this.m_A;
		}
	}

	internal global::a.S a
	{
		get
		{
			try
			{
				if (this.m_A == null || this.m_A.HasExited || this.m_A.StartTime != this.m_b || this.m_A.A != this.C)
				{
					return null;
				}
				B(this.C);
				if (c() != this.m_A.A)
				{
					return null;
				}
				return this.m_A;
			}
			catch
			{
				return null;
			}
		}
	}

	private string B => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iE());

	private string b => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ie());

	public string C
	{
		[CompilerGenerated]
		get
		{
			return this.m_c;
		}
		[CompilerGenerated]
		private set
		{
			this.m_c = text;
		}
	}

	private long c()
	{
		F();
		long num = G(this.m_B + 680);
		if (G(num) != this.m_F + E(64683768L) || A(num + 24, 1)[0] != 0)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iF());
		}
		long num2 = G(num + 8);
		if ((int)num2 == 0 || (uint)(num2 >>> 32) >= 16)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.@if());
		}
		return num2;
	}

	private long A(long P_0, long P_1, uint P_2, long P_3)
	{
		if (P_2 == 0 || P_2 > 16)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iG());
		}
		for (uint num = 0u; num < P_2; num++)
		{
			uint num2 = F(P_1 + num * 4);
			if (num2 != uint.MaxValue)
			{
				long num3 = A(P_0, num2);
				if (G(num3 + 8) == P_3)
				{
					return num3;
				}
			}
		}
		throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ig());
	}

	internal IEnumerable<string> D()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<O> list = k();
		yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xK() + this.A.ToString() + (this.m_d ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xL() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xk()) + list.Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fN() + stopwatch.ElapsedMilliseconds + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xl() + this.C;
		foreach (O item in list)
		{
			yield return item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IX() + item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xM() + item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Iz() + item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xm() + item.B + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iA() + item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iB() + item.b + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xN() + item.C + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xn() + (item.b ? item.A.ToArgb().ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xo()) : (CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xO() + this.b)) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xP() + global::b.k.A(item) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Iy() + global::A.t.A(item) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xp() + q.A(item) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iC() + item.A.Count;
			foreach (global::b.d item2 in item.A)
			{
				item2.A = item.b;
				item2.A = item.A;
			}
		}
		if (list.Count((O P_0) => P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()) != 1)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xQ());
		}
		if (list.Any((O P_0) => !P_0.b))
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xq() + this.b);
		}
		List<global::b.A> list2 = A(this.C, list.SelectMany((O P_0) => P_0.A));
		yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xR() + list2.Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xr() + list.Sum((O P_0) => P_0.A.Count) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xS() + list2.Count((global::b.A P_0) => P_0.A != null) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xs() + list2.Count((global::b.A P_0) => P_0.A != null);
		foreach (global::b.A item3 in list2.Take(16))
		{
			yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xT() + item3.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + item3.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xt() + item3.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bc() + item3.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bc() + item3.B + ((item3.A == null) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D() : (CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xU() + item3.A.c + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xu() + item3.A.c));
		}
		F();
		yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xV() + L();
		using (n n2 = new n(i(), this.m_d))
		{
			List<global::b.A> list3 = n2.A(this.C, list.SelectMany((O P_0) => P_0.A));
			n2.F();
			yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xv() + list3.Count;
		}
		global::a.z z2 = null;
		stopwatch.Restart();
		while (stopwatch.ElapsedMilliseconds < 12000 && !A(out z2))
		{
			Thread.Sleep(40);
		}
		if (z2 == null)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xW() + this.B + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + this.a);
		}
		yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xw() + stopwatch.ElapsedMilliseconds + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xX() + z2.D + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA() + z2.d + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xx() + z2.B + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA() + z2.b + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xY() + z2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bc() + z2.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xy() + z2.E;
		bool flag = false;
		long num = I(this.m_c);
		if (num == 0L)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xZ());
		}
		global::a.z z3 = A(num, out flag);
		yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xz() + z3.D + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA() + z3.d + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YA() + flag;
		T[] array = list.Select((O P_0) => new T
		{
			A = P_0.A,
			A = P_0.A,
			A = P_0.A,
			A = P_0.a
		}).ToArray();
		yield return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ya() + A(list).Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.YB() + A(this.C, array, false).Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yb() + A(this.C, array).Count;
	}

	public void d()
	{
		this.m_A.Clear();
		this.m_A.A();
		this.m_A = new ulong[0];
		this.m_A.Clear();
		this.m_A = true;
	}

	public List<global::A.L> A(string P_0, IEnumerable<T> P_1)
	{
		B(P_0);
		long num = this.m_G;
		long num2 = this.A(num, this.m_B);
		long num3 = this.A(num, F(num2 + 260));
		double A = (double)Stopwatch.GetTimestamp() * 1000.0 / (double)Stopwatch.Frequency;
		if (this.m_A != num3)
		{
			this.m_A = num3;
			this.m_A = new ulong[0];
			this.m_A.Clear();
			this.m_A.A();
			this.m_A.Clear();
			this.m_A.Clear();
		}
		Dictionary<long, T> dictionary = (from t2 in P_1
			group t2 by t2.A).ToDictionary((IGrouping<long, T> grouping) => grouping.Key, (IGrouping<long, T> source) => source.First());
		List<global::A.L> list = new List<global::A.L>();
		foreach (T value6 in dictionary.Values)
		{
			long num4 = value6.A + 1016;
			uint num5 = F(num4 + 44);
			uint num6 = F(num4 + 40);
			if (num5 > 256)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iH());
			}
			if (num5 == 0)
			{
				continue;
			}
			byte[] value = this.A(this.A(num, num6), (int)(num5 * 4));
			long num7 = G(value6.A + 8);
			for (int num8 = 0; num8 < num5; num8++)
			{
				try
				{
					uint num9 = BitConverter.ToUInt32(value, num8 * 4);
					if (num9 == uint.MaxValue)
					{
						continue;
					}
					long num10 = this.A(num, num9);
					if (G(num10) != this.m_F + E(64568064L))
					{
						continue;
					}
					byte[] array = this.A(num10, 148);
					string text = H(BitConverter.ToInt64(array, 56));
					global::A.G g2 = global::A.f.A(text);
					if (g2 != null && BitConverter.ToUInt32(array, 12) == 1)
					{
						uint num11 = BitConverter.ToUInt32(array, 8);
						byte[] array2 = this.A(this.A(num, num11), 20);
						if (global::A.h.A(array, array2, this.m_F, num7, out var num12, out var num13, out var num14, out var num15, out var num16, g2.A, E) && this.A(num10, 148).SequenceEqual(array) && this.A(this.A(num, num11), 20).SequenceEqual(array2))
						{
							list.Add(new global::A.L
							{
								A = value6.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num10 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + BitConverter.ToUInt32(array, 68) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + Convert.ToBase64String(array2.Skip(4).Take(12).ToArray()),
								a = text,
								A = value6.A,
								A = value6.A,
								A = value6.A,
								A = num12,
								a = num13,
								B = num14,
								b = num15,
								C = num16
							});
						}
					}
				}
				catch
				{
				}
			}
			if (F(num4 + 44) != num5 || F(num4 + 40) != num6)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ih());
			}
		}
		if (this.m_A)
		{
			foreach (global::A.L item in list)
			{
				this.m_A.Add(item.A);
			}
			this.m_A = false;
		}
		list.RemoveAll((global::A.L l5) => this.m_A.Contains(l5.A));
		bool flag = this.m_A.Length == 0;
		uint A2 = F(num3 + 24);
		uint num17 = F(num3 + 8);
		uint num18 = F(num3 + 20);
		if (A2 == 0 || A2 > 65536)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iI());
		}
		byte[] value2 = a(this.A(num, num17), (int)(A2 * 8));
		byte[] value3 = a(this.A(num, num18), (int)(A2 * 4));
		if (this.m_A.Length < A2)
		{
			Array.Resize(ref this.m_A, (int)A2);
		}
		List<global::A.L> list2 = new List<global::A.L>();
		List<global::A.J> list3 = new List<global::A.J>();
		List<global::A.L> list4 = new List<global::A.L>();
		uint[] array3 = this.m_A.Keys.Where((uint num32) => num32 >= A2).ToArray();
		foreach (uint key in array3)
		{
			this.m_A.Remove(key);
		}
		for (uint num20 = 0u; num20 < A2; num20++)
		{
			uint num21 = BitConverter.ToUInt32(value2, (int)(num20 * 8));
			uint num22 = BitConverter.ToUInt32(value3, (int)(num20 * 4));
			ulong num23 = ((ulong)num22 << 32) | num21;
			if (num21 == uint.MaxValue || num22 == 0)
			{
				this.m_A[num20] = num23;
				this.m_A.Remove(num20);
				continue;
			}
			if (this.m_A[num20] != num23)
			{
				this.m_A.Remove(num20);
				try
				{
					long num24 = this.A(num, num21);
					if (F(num24 + 104) != num22 || F(num24 + 108) != num20)
					{
						continue;
					}
					long num25 = G(num24 + 56);
					string text2 = H(num25);
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					if (global::A.f.B(text2) || global::A.f.a(text2))
					{
						this.m_A[num20] = new A
						{
							A = num24,
							A = num22,
							a = num20,
							a = num25,
							A = text2,
							A = A,
							A = !flag
						};
					}
					this.m_A[num20] = num23;
					goto IL_06ad;
				}
				catch
				{
				}
				continue;
			}
			goto IL_06ad;
			IL_06ad:
			if (!this.m_A.TryGetValue(num20, out var value4))
			{
				continue;
			}
			try
			{
				byte[] array4 = this.A(value4.A, 392);
				if (BitConverter.ToUInt32(array4, 104) != num22 || BitConverter.ToUInt32(array4, 108) != num20 || BitConverter.ToInt64(array4, 56) != value4.a)
				{
					continue;
				}
				if (global::A.f.a(value4.A))
				{
					uint num26 = BitConverter.ToUInt32(array4, 388);
					if (num26 == uint.MaxValue)
					{
						continue;
					}
					long num27 = this.A(value4.A, 103319634L);
					if (num27 != 0L)
					{
						byte[] array5 = this.A(this.A(num, num26), 68);
						byte[] array6 = this.A(num27, 108);
						global::A.L l2 = global::A.h.A(array4, array5, array6, this.m_F, num, value4.A, num22, num20, E);
						if (l2 != null && dictionary.TryGetValue(l2.A, out var value5) && F(value4.A + 104) == num22 && F(value4.A + 388) == num26 && this.A(this.A(num, num26) + 16, 16).SequenceEqual(array5.Skip(16).Take(16)))
						{
							l2.A = value5.A;
							l2.A = value5.A;
							list4.Add(l2);
						}
					}
					continue;
				}
				uint num28 = BitConverter.ToUInt32(array4, 88);
				if (num28 == uint.MaxValue || (BitConverter.ToUInt32(array4, 160) & 0x4000) != 0)
				{
					continue;
				}
				long key2 = this.A(num, num28);
				if (!dictionary.ContainsKey(key2) || F(value4.A + 88) != num28 || F(value4.A + 104) != num22)
				{
					continue;
				}
				if (global::a.s.A(value4.A))
				{
					global::A.L l3 = this.A(value4.A, num22, array4, value4.A, num);
					if (l3 != null)
					{
						l3.A = key2;
						l3.A = dictionary[key2].A;
						l3.A = dictionary[key2].A;
						list2.Add(l3);
					}
					continue;
				}
				if (value4.A == null && value4.A && A - value4.A < 2500.0 && (value4.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.au() || value4.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aV() || value4.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.av()))
				{
					try
					{
						uint num29 = BitConverter.ToUInt32(array4, 388);
						if (num29 != uint.MaxValue)
						{
							global::A.H h2 = K();
							byte[] array7 = this.A(this.A(num, num29), global::A.h.A(h2));
							global::A.L l4 = global::A.h.A(value4.A, array4, array7, this.m_F, num, value4.A, num22, num20, E, h2);
							if (l4 != null && F(value4.A + 388) == num29 && F(value4.A + 104) == num22 && this.A(this.A(num, num29) + 16, 16).SequenceEqual(array7.Skip(16).Take(16)))
							{
								value4.A = l4;
								value4.A.A = key2;
								value4.A.A = dictionary[key2].A;
								value4.A.A = dictionary[key2].A;
							}
						}
					}
					catch
					{
					}
				}
				float num30 = BitConverter.ToSingle(array4, 44);
				float num31 = BitConverter.ToSingle(array4, 52);
				if (value4.A != null && global::A.h.A(value4.A, null, num30, num31))
				{
					value4.a = true;
				}
				list3.Add(new global::A.J
				{
					A = value4.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num22,
					a = value4.A,
					A = key2,
					A = value4.A,
					A = value4.A,
					a = value4.a,
					A = global::b.b.A(num30, 0f, num31),
					A = num30,
					a = num31
				});
			}
			catch
			{
			}
		}
		if (F(num3 + 24) != A2 || F(num3 + 8) != num17 || F(num3 + 20) != num18)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ii());
		}
		B(P_0);
		List<global::A.L> list5 = this.m_A.A(list, list3, A);
		foreach (global::A.J item2 in list3.Where((global::A.J j2) => j2.A != null && !j2.a && A - j2.A >= 600.0 && !this.m_A.A(j2.A)))
		{
			global::A.L A3 = item2.A;
			if (!list5.Any((global::A.L l5) => l5.A == A3.A && l5.a == A3.a && Math.Abs(l5.A - A3.A) + Math.Abs(l5.B - A3.B) < 20f))
			{
				list5.Add(A3);
			}
		}
		list5.AddRange(list4);
		list5.AddRange(list2);
		long[] array8 = this.m_A.Keys.Where(delegate(long num32)
		{
			F f2 = new F();
			f2.A = num32;
			return !this.m_A.Values.Any(f2.A);
		}).ToArray();
		foreach (long key3 in array8)
		{
			this.m_A.Remove(key3);
		}
		return list5;
	}

	private long A(long P_0, long P_1, uint P_2, uint P_3, bool P_4)
	{
		if (P_2 == 0 || P_1 == 0L || P_3 >= F(P_1 + 24) || P_3 > 1000000)
		{
			return 0L;
		}
		long num = A(P_0, F(P_1 + 20));
		long num2 = A(P_0, F(P_1 + 8));
		if (F(num + P_3 * 4) != P_2)
		{
			return 0L;
		}
		uint num3 = F(num2 + P_3 * 8);
		if (num3 == uint.MaxValue)
		{
			return 0L;
		}
		long num4 = A(P_0, num3);
		if (F(num + P_3 * 4) != P_2 || F(num2 + P_3 * 8) != num3)
		{
			return 0L;
		}
		if (!P_4 && (F(num4 + 104) != P_2 || F(num4 + 108) != P_3 || (F(num4 + 160) & 0x4000) != 0))
		{
			return 0L;
		}
		return num4;
	}

	private bool A(long P_0, byte[] P_1, int P_2, out float P_3, out float P_4, out float P_5)
	{
		if (global::A.s.A(P_1, P_2, out P_3, out P_4, out P_5))
		{
			return true;
		}
		int num = P_2 * 20;
		byte b2 = P_1[num];
		if (b2 != 3 && b2 != 4)
		{
			return false;
		}
		uint num2 = BitConverter.ToUInt32(P_1, num + 4);
		uint num3 = BitConverter.ToUInt32(P_1, num + 8);
		long num4 = A(P_0, F(this.m_F + E(98614988L)));
		uint num5 = F(num4 + ((b2 == 3) ? 260 : 256));
		if (num5 == uint.MaxValue)
		{
			return false;
		}
		long num6 = A(P_0, A(P_0, num5), num2, num3, b2 == 4);
		if (num6 == 0L)
		{
			return false;
		}
		byte[] value = A(num6 + 44, 12);
		P_3 = BitConverter.ToSingle(value, 0);
		P_4 = BitConverter.ToSingle(value, 4);
		P_5 = BitConverter.ToSingle(value, 8);
		return global::A.s.A(P_3, P_4, P_5);
	}

	private void A(long P_0, long P_1, string P_2, O P_3, bool P_4 = false)
	{
		if ((P_3.a != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() && !P_4) || !global::A.s.A(P_2))
		{
			return;
		}
		long num = A(P_1, 103319626L);
		if (num == 0L || f(num + 108) <= 0f)
		{
			return;
		}
		uint num2 = F(P_1 + 104);
		uint num3 = F(P_1 + 108);
		uint num4 = F(P_1 + 88);
		long num5 = A(P_1, 103319637L);
		if (num5 == 0L)
		{
			return;
		}
		uint num6 = F(num5 + 76);
		if (num6 == uint.MaxValue)
		{
			return;
		}
		long num7 = A(P_0, num6);
		uint num8 = F(num7 + 44);
		uint num9 = F(num7 + 40);
		switch (num8)
		{
		default:
			throw new InvalidOperationException();
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 17u:
		case 18u:
		case 19u:
		case 20u:
		case 21u:
		case 22u:
		case 23u:
		case 24u:
		case 25u:
		case 26u:
		case 27u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
		case 32u:
		case 33u:
		case 34u:
		case 35u:
		case 36u:
		case 37u:
		case 38u:
		case 39u:
		case 40u:
		case 41u:
		case 42u:
		case 43u:
		case 44u:
		case 45u:
		case 46u:
		case 47u:
		case 48u:
		case 49u:
		case 50u:
		case 51u:
		case 52u:
		case 53u:
		case 54u:
		case 55u:
		case 56u:
		case 57u:
		case 58u:
		case 59u:
		case 60u:
		case 61u:
		case 62u:
		case 63u:
		case 64u:
		{
			if (num9 == uint.MaxValue)
			{
				throw new InvalidOperationException();
			}
			byte[] value = A(A(P_0, num9), (int)(num8 * 4));
			List<global::A.T> list = new List<global::A.T>();
			for (int num10 = 0; num10 < num8; num10++)
			{
				uint num11 = BitConverter.ToUInt32(value, num10 * 4);
				if (num11 == uint.MaxValue)
				{
					continue;
				}
				long num12 = A(P_0, num11);
				if (G(num12) != this.m_F + E(64568064L))
				{
					continue;
				}
				byte[] array = A(num12, 164);
				string text = H(BitConverter.ToInt64(array, 56));
				if (!global::A.r.A(P_2, text) || !global::A.s.A(text, array))
				{
					continue;
				}
				uint num13 = BitConverter.ToUInt32(array, 8);
				uint num14 = BitConverter.ToUInt32(array, 12);
				if (num14 == 0)
				{
					continue;
				}
				if (num14 > 64 || num13 == uint.MaxValue)
				{
					throw new InvalidOperationException();
				}
				long num15 = A(P_0, num13);
				byte[] array2 = A(num15, (int)(num14 * 20));
				List<global::A.T> list2 = new List<global::A.T>();
				for (int num16 = 0; num16 < num14; num16++)
				{
					if (A(P_0, array2, num16, out var num17, out var num18, out var num19))
					{
						string text2 = P_1 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num12 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + text;
						list2.Add(new global::A.T
						{
							A = text2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num16,
							a = text2,
							B = global::b.k.A(global::A.r.a(P_2)),
							C = text,
							b = P_3.A,
							A = P_3.A,
							A = num17,
							a = num18,
							B = num19,
							A = (P_3.b ? P_3.A : Color.Red)
						});
					}
				}
				byte[] array3 = A(num12, 164);
				if (BitConverter.ToInt64(array3, 0) != this.m_F + E(64568064L) || BitConverter.ToInt64(array3, 56) != BitConverter.ToInt64(array, 56) || BitConverter.ToUInt32(array3, 8) != num13 || BitConverter.ToUInt32(array3, 12) != num14 || !global::A.s.A(text, array3))
				{
					throw new InvalidOperationException();
				}
				if (!A(num15, array2.Length).SequenceEqual(array2))
				{
					throw new InvalidOperationException();
				}
				list.AddRange(list2);
			}
			if (F(P_1 + 104) != num2 || F(P_1 + 108) != num3 || F(P_1 + 88) != num4 || F(num5 + 76) != num6 || F(num7 + 44) != num8 || F(num7 + 40) != num9)
			{
				throw new InvalidOperationException();
			}
			P_3.A.AddRange(list);
			break;
		}
		case 0u:
			break;
		}
	}

	private void A(global::b.A P_0, global::b.d P_1)
	{
		bool flag = P_1.A.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bv()) && (P_1.A.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iJ()) || P_1.A.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ij()));
		bool flag2 = P_1.A.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iK()) || flag;
		bool flag3 = P_1.A.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ik());
		if (!P_0.A && !flag2 && !flag3)
		{
			return;
		}
		foreach (global::b.C item in P_1.A)
		{
			try
			{
				long num = item.A;
				byte[] value = A(num, 128);
				if (BitConverter.ToUInt32(value, 96) != item.A || BitConverter.ToUInt32(value, 124) != item.a || A(this.m_G, BitConverter.ToUInt32(value, 88)) != item.a)
				{
					continue;
				}
				long num2 = A(num, 103319626L);
				if (num2 == 0L || f(num2 + 108) <= 0f)
				{
					continue;
				}
				long num3 = 0L;
				long num4 = A(num, 103319697L);
				if (num4 != 0L)
				{
					uint num5 = F(num4 + 16);
					if (num5 != uint.MaxValue)
					{
						num3 = A(this.m_G, num5);
					}
				}
				if (num3 == 0L)
				{
					long num6 = A(num, 103319632L);
					if (num6 != 0L && F(num6 + 20) != 0 && F(num6 + 20) < 32)
					{
						long num7 = A(this.m_G, F(A(this.m_G, F(num6 + 16))));
						uint num8 = F(num7 + 240);
						if (num8 != uint.MaxValue)
						{
							num3 = A(this.m_G, num8);
						}
					}
				}
				if (num3 == 0L)
				{
					continue;
				}
				string text = H(G(num3 + 184));
				bool flag4 = P_1.A.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iJ()) || P_1.A.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ij());
				bool flag5 = text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iL()) || text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.il());
				bool flag6 = text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iM()) || (flag4 && text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.im()));
				if (flag4 && !flag6)
				{
					continue;
				}
				if (!flag3 && !flag)
				{
					goto IL_02a7;
				}
				if (flag5 || flag6)
				{
					long num9 = A(num, 103319627L);
					if (num9 == 0L || F(num9 + 40) == 0)
					{
						goto IL_02a7;
					}
				}
				goto end_IL_0088;
				IL_02a7:
				float num10 = BitConverter.ToSingle(value, 32);
				float num11 = BitConverter.ToSingle(value, 40);
				float num12 = (float)Math.Sqrt(num10 * num10 + num11 * num11);
				if (float.IsNaN(num12) || (double)num12 < 0.5 || (double)num12 > 1.5)
				{
					continue;
				}
				float num13 = f(num3 + 404);
				if (num13 <= 0f || num13 > 500f)
				{
					continue;
				}
				global::A.X x2 = new global::A.X
				{
					A = BitConverter.ToSingle(value, 44),
					a = BitConverter.ToSingle(value, 48),
					B = BitConverter.ToSingle(value, 52),
					b = num10 / num12,
					C = num11 / num12,
					c = num13,
					A = flag2,
					B = flag5,
					b = flag6,
					a = global::A.x.A(P_1.A, text),
					C = (flag || !flag2 || (A(num3 + 438, 1)[0] & 4) != 0)
				};
				if (flag5 && (flag2 || flag3))
				{
					try
					{
						long num14 = G(num3 + 312);
						byte[] value2 = A(num14 + 280, 28);
						global::A.x.A(x2, BitConverter.ToSingle(value2, 0), BitConverter.ToSingle(value2, 4), BitConverter.ToSingle(value2, 24));
					}
					catch
					{
					}
				}
				P_0.A = x2;
				break;
				end_IL_0088:;
			}
			catch
			{
			}
		}
	}

	private void A(long P_0, long P_1, string P_2, O P_3)
	{
		if (!(P_3.a != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()) && global::a.i.A(P_2) != null)
		{
			long num = A(P_1, 103319626L);
			if (num != 0L && !(f(num + 108) <= 0f))
			{
				global::b.d d2 = new global::b.d
				{
					A = P_1,
					a = G(P_1 + 104),
					A = P_2,
					A = P_3.A
				};
				d2.A.Add(new global::b.C
				{
					A = P_1,
					a = P_3.A,
					A = F(P_1 + 96),
					a = F(P_1 + 124)
				});
				P_3.A.Add(d2);
			}
		}
	}

	private void A(List<O> P_0, long P_1)
	{
		O[] array = P_0.Where((O o2) => o2.a == P_1 && o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()).ToArray();
		if (array.Length != 1)
		{
			E();
			return;
		}
		if (DateTime.UtcNow >= this.m_A)
		{
			this.m_A = global::a.r.A(global::a.r.A, this.m_b);
			this.m_A = DateTime.UtcNow.AddSeconds(this.m_A.HasValue ? 10 : 2);
		}
		array[0].A = true;
		array[0].A = this.m_A;
		this.m_A = new global::a.S
		{
			A = this.m_A.Id,
			A = this.m_b,
			A = this.C,
			A = array[0].A,
			A = P_1,
			A = this.m_A
		};
	}

	private void E()
	{
		this.m_A = null;
		this.m_A = null;
		this.m_A = DateTime.MinValue;
	}

	private global::A.L A(long P_0, uint P_1, byte[] P_2, string P_3, long P_4)
	{
		if (!global::a.s.A(P_3))
		{
			return null;
		}
		if (!this.m_A.TryGetValue(P_0, out var value) || value.A != P_1)
		{
			Dictionary<long, global::a.T> dictionary = this.m_A;
			long key = P_0;
			global::a.T obj = new global::a.T
			{
				A = P_1
			};
			global::a.T t2 = obj;
			dictionary[key] = obj;
			value = t2;
		}
		uint num = BitConverter.ToUInt32(P_2, 388);
		if (num == uint.MaxValue)
		{
			return null;
		}
		global::A.H h2 = K();
		byte[] array = A(A(P_4, num), global::A.h.A(h2));
		if (BitConverter.ToInt64(array, 0) != this.m_F + E(64559016L) || BitConverter.ToUInt32(array, 36) + P_4 != P_0 || BitConverter.ToUInt32(array, 64) != 1)
		{
			return null;
		}
		float num2 = BitConverter.ToSingle(P_2, 44);
		float num3 = BitConverter.ToSingle(P_2, 52);
		byte b2 = array[16];
		if ((b2 != 2 && b2 != 4) || !A(P_4, array.Skip(16).Take(20).ToArray(), 0, out var num4, out var num5, out var num6))
		{
			return null;
		}
		float num7 = 0f;
		float num8 = 0f;
		long num9 = A(P_0, 103319632L);
		if (num9 != 0L && F(num9 + 20) != 0 && F(num9 + 20) < 32)
		{
			long num10 = A(P_4, F(A(P_4, F(num9 + 16))));
			uint num11 = F(num10 + 240);
			if (num11 != uint.MaxValue)
			{
				long num12 = A(P_4, num11);
				global::a.t.A(H(G(num12 + 184)), out num7, out num8);
			}
		}
		string text = (P_3.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iN()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iO() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.@in());
		global::A.h.A(array, h2, out var num13, out var _, out var num15);
		global::a.s.A(value, b2 == 4, num4, num5, num6, num2, num3, num13, num15, num7, num8, P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + P_1, text);
		return value.A;
	}

	private void e()
	{
		lock (this.m_A)
		{
			if (this.m_a || this.m_C)
			{
				return;
			}
			this.m_a = true;
			try
			{
				long num = global::a.Y.A(File.ReadAllText(this.B), this.m_A.Id, this.m_b.ToUniversalTime().Ticks, this.m_F);
				if (num != 0L && this.m_D == 0L)
				{
					this.m_D = num;
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}

	private void A(long P_0)
	{
		if (P_0 == 0L || P_0 == this.m_a)
		{
			return;
		}
		try
		{
			File.WriteAllText(this.B, this.m_A.Id + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + this.m_b.ToUniversalTime().Ticks + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + this.m_F + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + P_0);
			this.m_a = P_0;
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	public global::a.w F()
	{
		try
		{
			if (this.m_B == 0L || G(this.m_B) != this.m_F + E(64684536L))
			{
				this.m_B = 0L;
				long num = G(G(this.m_F + E(102260680L)) + 184);
				long num2 = G(num);
				long num3 = G(num + 8);
				if (num3 < num2 || num3 - num2 > 8192)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.io());
				}
				for (long num4 = num2; num4 < num3; num4 += 8)
				{
					long num5 = G(num4);
					if (num5 < 65576)
					{
						continue;
					}
					try
					{
						if (G(num5 - 40) == this.m_F + E(64684536L))
						{
							this.m_B = num5 - 40;
							break;
						}
					}
					catch
					{
					}
				}
			}
			if (this.m_B == 0L)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iP());
			}
			long num6 = G(this.m_B + 416);
			byte[] value = A(num6, 816);
			global::a.w w2 = new global::a.w
			{
				A = new float[16],
				a = new float[16]
			};
			for (int num7 = 0; num7 < 16; num7++)
			{
				w2.A[num7] = BitConverter.ToSingle(value, 88 + num7 * 4);
				w2.a[num7] = BitConverter.ToSingle(value, 752 + num7 * 4);
				if (float.IsNaN(w2.A[num7]) || float.IsInfinity(w2.A[num7]) || float.IsNaN(w2.a[num7]) || float.IsInfinity(w2.a[num7]))
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ip());
				}
			}
			if ((double)Math.Abs(w2.A[15] - 1f) > 0.01 || w2.a[0] <= 0f || w2.a[5] <= 0f || (double)Math.Abs(w2.a[11] - 1f) > 0.01)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iQ());
			}
			for (int num8 = 0; num8 < 3; num8++)
			{
				float num9 = w2.A[12 + num8];
				for (int num10 = 0; num10 < 3; num10++)
				{
					num9 += BitConverter.ToSingle(value, 72 + num10 * 4) * w2.A[num10 * 4 + num8];
				}
				if ((double)Math.Abs(num9) > 0.1)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iq());
				}
			}
			if (G(this.m_B + 416) != num6)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iR());
			}
			return w2;
		}
		catch
		{
			this.m_B = 0L;
			throw;
		}
	}

	[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
	private static extern UIntPtr A(p P_0, IntPtr P_1, out a P_2, UIntPtr P_3);

	private void a(string P_0)
	{
		lock (this.m_A)
		{
			if (!(this.m_A == P_0))
			{
				this.m_A = P_0;
				this.m_d++;
				this.m_g = 0L;
				this.m_C = DateTime.MinValue;
				if (this.m_c != 0L)
				{
					this.m_D = this.m_c;
				}
				this.m_c = 0L;
				this.m_a = DateTime.MinValue;
				this.m_a = 0;
				this.m_A.b();
				this.m_B = true;
				this.m_A = 0;
				this.m_A.Clear();
				this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ir();
				this.a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
		}
	}

	private byte[] A(long P_0, byte[] P_1)
	{
		int num = BitConverter.ToInt32(P_1, 124) & 0x3FF;
		long num2 = BitConverter.ToInt64(P_1, 112);
		if (num == 0)
		{
			return new byte[0];
		}
		byte[] result = A(num2, num * 16);
		if (G(P_0 + 112) != num2 || (F(P_0 + 124) & 0x3FF) != num)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iS());
		}
		return result;
	}

	private long A(byte[] P_0, int P_1)
	{
		for (int num = 0; num < P_0.Length; num += 16)
		{
			int num2 = BitConverter.ToUInt16(P_0, num + 2);
			if (num2 > P_1)
			{
				return 0L;
			}
			if (num2 == P_1)
			{
				int num3 = BitConverter.ToUInt16(P_0, num);
				long num4 = BitConverter.ToInt64(P_0, num + 8);
				if (num4 == 0L)
				{
					return 0L;
				}
				if ((num3 & 0x70) != 0)
				{
					num4 = G(num4 + (((num3 & 0x40) != 0) ? 48 : (((num3 & 0x20) != 0) ? 56 : 40)));
				}
				else if ((num3 & 0x100) != 0)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.@is());
				}
				return num4;
			}
		}
		return 0L;
	}

	private int a(long P_0)
	{
		long num = G(this.m_F + E(P_0));
		return BitConverter.ToUInt16(A(num + 184, 2), 0);
	}

	private float a(byte[] P_0, int P_1)
	{
		long num = A(P_0, P_1);
		if (num < 65536)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iT());
		}
		float num2 = f(num + 40);
		if (num2 < 8f || num2 > 20000f)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.it());
		}
		return num2;
	}

	private global::a.y B(long P_0)
	{
		switch (e(G(P_0) - this.m_F))
		{
		case 64432864L:
		{
			if (A(P_0 + 248, 1)[0] != 0)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iU());
			}
			byte[] array3 = A(P_0 + 216, 32);
			byte[] array4 = A(P_0 + 216, 32);
			if (!global::A.U.A(array3, array4) || A(P_0 + 248, 1)[0] != 0)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iU());
			}
			return global::a.y.A(array3);
		}
		case 64431008L:
		{
			byte[] array = A(P_0, 128);
			byte[] array2 = A(P_0, array);
			int num10 = a(111878264L);
			long num11 = A(array2, num10);
			if (num11 == 0L)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iu());
			}
			return global::a.y.A(A(num11 + 40, 32));
		}
		case 64815280L:
		{
			float num5 = f(P_0 + 208);
			float num6 = f(P_0 + 212);
			float num7 = f(P_0 + 216);
			double num8 = Math.Cos((double)num7 * Math.PI / 180.0);
			double num9 = Math.Sin((double)num7 * Math.PI / 180.0);
			return new global::a.y
			{
				A = num8,
				a = 0.0 - num9,
				B = num9,
				b = num8,
				C = (double)num5 * (1.0 - num8) + (double)num6 * num9,
				c = (double)num6 * (1.0 - num8) - (double)num5 * num9
			};
		}
		case 64815656L:
		{
			float num = f(P_0 + 208);
			float num2 = f(P_0 + 212);
			float num3 = f(P_0 + 216);
			float num4 = f(P_0 + 220);
			return new global::a.y
			{
				A = num3,
				b = num4,
				C = num * (1f - num3),
				c = num2 * (1f - num4)
			};
		}
		case 64430720L:
			return global::a.y.A(f(P_0 + 208), f(P_0 + 212));
		default:
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iV());
		}
	}

	private global::a.z b(long P_0)
	{
		if (P_0 < 65536 || G(P_0) != this.m_F + E(65511760L))
		{
			throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iv());
		}
		byte[] array = A(P_0, 480);
		byte[] array2 = A(P_0, array);
		float num = a(array2, a(111989640L));
		float num2 = a(array2, a(111989680L));
		long num3 = BitConverter.ToInt64(array, 472);
		if (num3 < 65536 || G(num3 + 136) != P_0)
		{
			throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iW());
		}
		byte[] value = A(num3, 344);
		float num4 = BitConverter.ToSingle(value, 296);
		float num5 = BitConverter.ToSingle(value, 300);
		if (num4 < 10f || num5 < 10f || num4 > 4000f || num5 > 4000f)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iw());
		}
		global::a.y y2 = global::a.y.A;
		long num6 = num3;
		int num7 = 0;
		HashSet<long> hashSet = new HashSet<long>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		float num8 = 0f;
		float num9 = 0f;
		while (num6 != 0L)
		{
			if (!hashSet.Add(num6) || num7++ >= 64)
			{
				throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iX());
			}
			byte[] array3 = A(num6, 344);
			long num10 = BitConverter.ToInt64(array3, 0) - this.m_F;
			if (num10 == E(65713904L))
			{
				flag = true;
			}
			long num11 = A(A(num6, array3), 40);
			if (flag3 && num11 == 0L)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ix());
			}
			flag3 = false;
			if (num11 != 0L)
			{
				y2 = y2.A(B(num11));
			}
			float num12 = BitConverter.ToSingle(array3, 168);
			float num13 = BitConverter.ToSingle(array3, 172);
			if (!global::b.b.A(num12, 0f, num13))
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iY());
			}
			y2 = y2.A(global::a.y.A(num12, num13));
			long num14 = BitConverter.ToInt64(array3, 136);
			if (G(num6 + 136) != num14)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iy());
			}
			if (num14 == 0L)
			{
				flag2 = num10 == E(64823200L);
				num8 = BitConverter.ToSingle(array3, 296);
				num9 = BitConverter.ToSingle(array3, 300);
			}
			num6 = num14;
		}
		if (!flag || !flag2)
		{
			throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iZ());
		}
		if (num8 < 200f || num9 < 200f || num8 > 20000f || num9 > 20000f)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.iz());
		}
		if (G(P_0 + 472) != num3 || G(P_0) != this.m_F + E(65511760L))
		{
			throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JA());
		}
		global::a.z z2 = global::a.y.A(num, num2, num4, num5, num8, num9, y2);
		if (!z2.A || z2.A < 0f || z2.A > num8 || z2.a < 0f || z2.a > num9)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ja());
		}
		return z2;
	}

	public bool A(out global::a.z P_0)
	{
		P_0 = null;
		long num;
		long num2;
		lock (this.m_A)
		{
			num = this.m_c;
			num2 = this.m_d;
		}
		if (num == 0L)
		{
			G();
			return false;
		}
		try
		{
			B(this.C);
			global::a.z z2 = b(num);
			B(this.C);
			lock (this.m_A)
			{
				if (this.m_C || this.m_d != num2)
				{
					return false;
				}
				this.m_a = 0;
			}
			P_0 = z2;
			this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			return true;
		}
		catch (Exception ex)
		{
			this.B = ex.Message;
			bool flag = false;
			lock (this.m_A)
			{
				if (this.m_d != num2)
				{
					return false;
				}
				if (ex is global::B.c || ++this.m_a >= 8)
				{
					if (this.m_c == num)
					{
						this.m_c = 0L;
					}
					flag = true;
				}
			}
			if (flag)
			{
				G();
			}
			return false;
		}
	}

	public void f()
	{
		lock (this.m_A)
		{
			if (this.m_C)
			{
				return;
			}
			this.m_d++;
			this.m_c = 0L;
			this.m_g = 0L;
			this.m_C = DateTime.MinValue;
			this.m_a = 0;
			this.m_a = DateTime.MinValue;
			this.m_A.b();
			this.m_B = true;
			this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ea();
		}
		G();
	}

	private void G()
	{
		if (!global::a.O.A)
		{
			return;
		}
		e();
		long a2;
		long B;
		long A;
		lock (this.m_A)
		{
			if (this.m_C || this.m_b || string.IsNullOrEmpty(this.m_A) || DateTime.UtcNow < this.m_a)
			{
				return;
			}
			this.m_b = true;
			a2 = this.m_d;
			B = (this.m_C = ++this.m_b);
			A = this.m_D;
			this.m_a = DateTime.UtcNow.AddMilliseconds(100.0);
		}
		try
		{
			if (ThreadPool.QueueUserWorkItem(delegate
			{
				long num = 0L;
				long num2 = 0L;
				Stopwatch stopwatch = Stopwatch.StartNew();
				string text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				bool flag = false;
				try
				{
					List<long> list = new List<long>();
					lock (this.m_A)
					{
						if (A != 0L)
						{
							list.Add(A);
						}
						long[] array = this.m_A.Where(delegate(KeyValuePair<long, DateTime> keyValuePair2)
						{
							DateTime utcNow = DateTime.UtcNow;
							KeyValuePair<long, DateTime> keyValuePair = keyValuePair2;
							if (utcNow - keyValuePair.Value < TimeSpan.FromSeconds(60.0))
							{
								keyValuePair = keyValuePair2;
								return keyValuePair.Key != A;
							}
							return false;
						}).Select(n.b.A.A).ToArray();
						if (array.Length != 0)
						{
							int num3 = this.m_A % array.Length;
							list.AddRange(array.Skip(num3).Concat(array.Take(num3)).Take(4));
							this.m_A = (num3 + 4) % array.Length;
						}
					}
					num = H();
					foreach (long item in list)
					{
						if (num != 0L)
						{
							break;
						}
						lock (this.m_A)
						{
							if (this.m_C || this.m_d != a2)
							{
								return;
							}
						}
						try
						{
							global::a.z z2 = b(item);
							if ((double)z2.A < (double)z2.C * 0.5 && (double)z2.a > (double)z2.c * 0.45)
							{
								num = item;
								break;
							}
						}
						catch
						{
						}
					}
					long num4;
					lock (this.m_A)
					{
						if (this.m_C || this.m_d != a2)
						{
							return;
						}
						if (!this.m_A.A)
						{
							long val = 0L;
							try
							{
								val = G(G(this.m_F + E(112837064L)) + 136) & -16777216;
							}
							catch
							{
							}
							this.m_A.B(Math.Max(65536L, val));
						}
						num4 = this.m_A.a;
					}
					byte[] array2 = new byte[65536];
					long num5 = this.m_F + E(65511760L);
					while (num == 0L && stopwatch.ElapsedMilliseconds < 80 && num2 < 33554432)
					{
						lock (this.m_A)
						{
							if (this.m_C || this.m_d != a2)
							{
								return;
							}
						}
						if (n.A(this.m_A, new IntPtr(num4), out var a3, (UIntPtr)(ulong)Marshal.SizeOf(typeof(a))) == UIntPtr.Zero)
						{
							lock (this.m_A)
							{
								if (this.m_d != a2)
								{
									return;
								}
								flag = this.m_A.C();
								if (flag)
								{
									if (this.m_B)
									{
										this.m_B = false;
										flag = false;
									}
									else
									{
										this.m_B = true;
									}
								}
								num4 = this.m_A.a;
							}
							break;
						}
						long num6 = Math.Max(num4, a3.A.ToInt64());
						long num7 = checked(a3.A.ToInt64() + (long)a3.A.ToUInt64());
						if (num7 <= num4)
						{
							break;
						}
						if (a3.a == 4096 && a3.b == 131072 && (a3.B & 0x101) == 0 && (a3.B & 0xEE) != 0 && (!this.m_B || a3.A.ToUInt64() <= 4194304))
						{
							long num8;
							for (num8 = num6; num8 < num7; num8 += array2.Length)
							{
								if (num != 0L)
								{
									break;
								}
								if (stopwatch.ElapsedMilliseconds >= 80)
								{
									break;
								}
								if (num2 >= 33554432)
								{
									break;
								}
								lock (this.m_A)
								{
									if (this.m_C || this.m_d != a2)
									{
										return;
									}
								}
								int num9 = (int)Math.Min(array2.Length, num7 - num8);
								global::B.H.a(this.m_A, new IntPtr(num8), array2, (UIntPtr)(ulong)num9, out var uIntPtr);
								int num10 = (int)Math.Min((ulong)num9, uIntPtr.ToUInt64());
								num2 += num9;
								if (num10 >= 8)
								{
									for (int num11 = 0; num11 + 8 <= num10; num11 += 8)
									{
										if (array2[num11] == (byte)num5 && BitConverter.ToInt64(array2, num11) == num5)
										{
											long num12 = num8 + num11;
											lock (this.m_A)
											{
												if (this.m_d != a2)
												{
													return;
												}
												if (this.m_A.Count >= 64)
												{
													long key = this.m_A.OrderBy(n.b.A.a).First().Key;
													this.m_A.Remove(key);
												}
												this.m_A[num12] = DateTime.UtcNow;
											}
											try
											{
												global::a.z z3 = b(num12);
												if ((double)z3.A < (double)z3.C * 0.5 && (double)z3.a > (double)z3.c * 0.45)
												{
													num = num12;
													break;
												}
											}
											catch
											{
											}
										}
									}
								}
							}
							num4 = Math.Min(num8, num7);
						}
						else
						{
							num4 = num7;
						}
						lock (this.m_A)
						{
							if (this.m_d != a2)
							{
								return;
							}
							flag = this.m_A.b(num4);
							if (flag)
							{
								if (this.m_B)
								{
									this.m_B = false;
									flag = false;
								}
								else
								{
									this.m_B = true;
								}
								num4 = this.m_A.a;
							}
						}
						if (flag)
						{
							break;
						}
					}
					lock (this.m_A)
					{
						if (!this.m_C && this.m_d == a2)
						{
							this.m_c = num;
							if (num != 0L)
							{
								this.m_D = num;
								this.m_a = 0;
								this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
							}
							else
							{
								this.m_a = DateTime.UtcNow.AddMilliseconds(flag ? 2000 : 100);
								this.B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xh();
							}
						}
					}
					if (num != 0L)
					{
						this.A(num);
						c(num);
					}
				}
				catch (Exception ex)
				{
					text = ex.Message;
				}
				finally
				{
					lock (this.m_A)
					{
						if (this.m_d == a2)
						{
							this.a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xI() + num.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xi() + this.m_A.a.ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xJ() + num2 / 1048576 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xj() + stopwatch.ElapsedMilliseconds + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + text;
						}
						if (this.m_C == B)
						{
							this.m_b = false;
						}
					}
				}
			}))
			{
				return;
			}
			lock (this.m_A)
			{
				if (this.m_C == B)
				{
					this.m_b = false;
				}
			}
		}
		catch
		{
			lock (this.m_A)
			{
				if (this.m_C == B)
				{
					this.m_b = false;
				}
			}
		}
	}

	private void g()
	{
		lock (this.m_A)
		{
			this.m_C = true;
			this.m_d++;
			this.m_c = (this.m_D = 0L);
		}
	}

	private IEnumerable<long> C(long P_0)
	{
		HashSet<long> hashSet = new HashSet<long>(h(P_0));
		try
		{
			byte[] array = A(P_0, 576);
			for (int num = 144; num + 8 <= array.Length; num += 8)
			{
				long num2 = BitConverter.ToInt64(array, num);
				if (num2 < 65536 || num2 > 140737488355327L || num2 % 8 != 0L || num2 == P_0)
				{
					continue;
				}
				try
				{
					if (G(num2 + 136) == P_0)
					{
						long num3 = G(num2) - this.m_F;
						if (num3 >= 64307200 && num3 < 96444416)
						{
							hashSet.Add(num2);
						}
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return hashSet;
	}

	private void c(long P_0)
	{
		try
		{
			long num = P_0;
			HashSet<long> hashSet = new HashSet<long>();
			while (num != 0L && hashSet.Count < 64 && hashSet.Add(num))
			{
				long num2 = G(num + 136);
				if (num2 == 0L)
				{
					if (G(num) == this.m_F + E(64823200L))
					{
						this.m_E = num;
						File.WriteAllText(this.b, this.m_A.Id + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + this.m_b.ToUniversalTime().Ticks + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + this.m_F + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bR() + num);
					}
					break;
				}
				num = num2;
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch
		{
		}
	}

	private long H()
	{
		lock (this.m_A)
		{
			if (this.m_C)
			{
				return 0L;
			}
			if (this.m_e != this.m_d)
			{
				this.m_e = this.m_d;
				h();
			}
		}
		if (!this.m_c)
		{
			this.m_c = true;
			try
			{
				this.m_E = global::a.Y.A(File.ReadAllText(this.b), this.m_A.Id, this.m_b.ToUniversalTime().Ticks, this.m_F);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
		try
		{
			long num = G(G(this.m_F + E(112837064L)) + 136);
			if (num > 65536 && G(num) == this.m_F + E(64823200L) && num != this.m_E)
			{
				this.m_E = num;
				this.m_A.Clear();
				this.m_A.Clear();
			}
			if (this.m_E == 0L)
			{
				long num2;
				lock (this.m_A)
				{
					num2 = this.m_D;
				}
				if (num2 != 0L)
				{
					c(num2);
				}
			}
			if (this.m_E == 0L || G(this.m_E) != this.m_F + E(64823200L))
			{
				return 0L;
			}
			if (this.m_A.Count == 0)
			{
				if (DateTime.UtcNow < this.m_B)
				{
					return 0L;
				}
				this.m_A.Clear();
				this.m_A.Enqueue(this.m_E);
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (this.m_A.Count > 0 && stopwatch.ElapsedMilliseconds < 25 && this.m_A.Count < 6000)
			{
				long num3 = this.m_A.Dequeue();
				if (!this.m_A.Add(num3))
				{
					continue;
				}
				try
				{
					if (G(num3) == this.m_F + E(65511760L))
					{
						lock (this.m_A)
						{
							this.m_A[num3] = DateTime.UtcNow;
						}
						try
						{
							global::a.z z2 = b(num3);
							if ((double)z2.A < (double)z2.C * 0.5 && (double)z2.a > (double)z2.c * 0.45)
							{
								this.m_A.Clear();
								this.m_B = DateTime.UtcNow.AddSeconds(1.0);
								return num3;
							}
						}
						catch
						{
						}
						continue;
					}
					foreach (long item in C(num3))
					{
						this.m_A.Enqueue(item);
					}
				}
				catch
				{
				}
			}
			if (this.m_A.Count == 0 || this.m_A.Count >= 6000)
			{
				this.m_A.Clear();
				this.m_B = DateTime.UtcNow.AddMilliseconds(500.0);
			}
			return 0L;
		}
		catch
		{
			this.m_A.Clear();
			this.m_E = 0L;
			return 0L;
		}
	}

	private void h()
	{
		this.m_A.Clear();
		this.m_A.Clear();
		this.m_B = DateTime.MinValue;
	}

	private void a(global::b.A P_0, global::b.d P_1)
	{
		if (!P_0.A && !this.D)
		{
			return;
		}
		foreach (global::b.C item in P_1.A)
		{
			try
			{
				long num = item.A;
				byte[] value = A(num, 128);
				if (BitConverter.ToUInt32(value, 96) != item.A || BitConverter.ToUInt32(value, 124) != item.a || A(this.m_G, BitConverter.ToUInt32(value, 88)) != item.a)
				{
					continue;
				}
				long num2 = A(num, 103319626L);
				if (num2 == 0L || f(num2 + 108) <= 0f)
				{
					continue;
				}
				uint num3 = F(num + 388);
				if (num3 == uint.MaxValue)
				{
					continue;
				}
				long num4 = A(this.m_G, num3);
				long num5 = G(num4);
				if (G(num5 + 344) != this.m_F + E(13437424L))
				{
					continue;
				}
				byte[] array = A(num4, 180);
				if (BitConverter.ToUInt32(array, 64) != 0)
				{
					continue;
				}
				uint num6 = BitConverter.ToUInt32(array, 172);
				if (num6 != uint.MaxValue)
				{
					long num7 = A(this.m_G, num6);
					byte[] array2 = A(num7, 148);
					global::B.e e2 = global::B.G.a(array, array2);
					if (e2 != null && F(num + 96) == item.A && F(num + 388) == num3 && F(num4 + 172) == num6 && F(num4 + 8) == 0 && F(num7 + 144) == BitConverter.ToUInt32(array2, 144) && A(num4 + 76, 16).SequenceEqual(array.Skip(76).Take(16)))
					{
						P_0.A = e2;
						break;
					}
				}
			}
			catch
			{
			}
		}
	}

	private bool D(long P_0)
	{
		try
		{
			long num = G(P_0);
			if (num < this.m_F || num > this.m_F + 268435456)
			{
				return false;
			}
			long num2 = G(num - 8);
			if (num2 < this.m_F || num2 > this.m_F + 268435456 || F(num2) != 1)
			{
				return false;
			}
			uint num3 = F(num2 + 12);
			if (num3 > 268435456)
			{
				return false;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JB());
			return global::A.U.A(A(this.m_F + num3 + 16, bytes.Length), bytes);
		}
		catch
		{
			return false;
		}
	}

	private long d(long P_0)
	{
		long num = G(P_0 + 296);
		long num2 = G(num);
		long num3 = G(num + 8);
		if (num2 < 65536 || num3 <= num2 || num3 - num2 > 32768 || (num3 - num2) % 8 != 0L)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jb());
		}
		long num4 = 0L;
		for (long num5 = num2; num5 < num3; num5 += 8)
		{
			long num6 = G(num5);
			if (num6 >= 65536 && D(num6))
			{
				if (num4 != 0L && num4 != num6)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JC());
				}
				num4 = num6;
			}
		}
		if (num4 == 0L)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jc());
		}
		if (G(num) != num2 || G(num + 8) != num3)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JD());
		}
		return num4;
	}

	private long I()
	{
		long num = G(this.m_F + E(102261280L));
		ulong num2 = 14695981039346656037uL;
		for (int num3 = 0; num3 < 4; num3++)
		{
			num2 ^= (byte)(3276800 >>> num3 * 8);
			num2 *= 1099511628211L;
		}
		long num4 = G(num + 664);
		if (num4 < 0 || num4 > 1048576)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jd());
		}
		long num5 = G(num + 640) + (long)((num2 & (ulong)num4) * 16);
		long num6 = G(num + 624);
		long num7 = G(num5 + 8);
		long num8 = G(num5);
		int num9 = 0;
		while (num7 != num6 && num9++ < 128)
		{
			if (F(num7 + 16) == 3276800)
			{
				long num10 = G(num7 + 24);
				if (G(num7 + 32) - num10 != 8)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JE());
				}
				long num11 = G(num10);
				if (F(num11 + 72) != 3276800)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Je());
				}
				return d(num11);
			}
			if (num7 == num8)
			{
				break;
			}
			num7 = G(num7 + 8);
		}
		throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JF());
	}

	private void A(List<O> P_0, long P_1, long P_2)
	{
		try
		{
			if (this.m_B == 0L || G(this.m_B) != this.m_F + E(64684536L))
			{
				F();
			}
			long num = G(this.m_B + 680);
			long num2 = I();
			long num3 = A(P_1, F(P_2 + 284));
			byte[] array = A(num + 24, 2);
			long num4 = G(num + ((array[0] == 0) ? 8 : 16));
			uint num5 = (uint)((ulong)num4 >> 32);
			foreach (O item in P_0)
			{
				int num6 = 3;
				if ((int)item.a != 0 && (int)num4 != 0 && num5 < 16)
				{
					num6 = A(num3 + 80 + item.A * 16 + num5, 1)[0];
				}
				int num7 = 77512;
				if (array[1] != 0)
				{
					if (num6 == 1 || num6 == 2)
					{
						uint num8 = F(num3 + item.A * 4);
						int num9 = 0;
						if (num8 != 10000 && num8 != uint.MaxValue)
						{
							for (uint num10 = 0u; num10 < item.A; num10++)
							{
								if (F(num3 + num10 * 4) == num8)
								{
									num9++;
								}
							}
						}
						if (num9 < 4)
						{
							num7 = ((num6 == 1) ? 77384 : 77448) + 16 * num9;
						}
					}
				}
				else if ((int)num4 != 0 && (int)num4 == (int)item.a)
				{
					num7 = 77352;
				}
				else
				{
					switch (num6)
					{
					case 1:
						num7 = 77336;
						break;
					case 2:
						num7 = 77368;
						break;
					}
				}
				uint num11 = F(num2 + num7 + 8);
				item.A = M.A(num11);
				item.b = true;
			}
			if (!A(num + 24, 2).A(array))
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jf());
			}
			this.b = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}
		catch (Exception ex)
		{
			foreach (O item2 in P_0)
			{
				item2.b = false;
			}
			this.b = ex.Message;
		}
	}

	public global::a.V i()
	{
		return new global::a.V
		{
			A = this.m_A.Id,
			A = this.m_F,
			A = this.m_b,
			A = this.C,
			a = this.m_G,
			A = this.m_B
		};
	}

	internal n(global::a.V P_0)
		: this(P_0, false)
	{
	}

	private n(global::a.V P_0, bool P_1)
	{
		this.m_d = P_1;
		this.m_A = Process.GetProcessById(P_0.A);
		try
		{
			this.m_b = this.m_A.StartTime;
			this.m_F = this.m_A.MainModule.BaseAddress.ToInt64();
			if (this.m_b != P_0.A || this.m_F != P_0.A)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JH());
			}
			this.m_A = global::a.G.A(this.m_A.MainModule.FileName, out var a2);
			this.m_A = a2;
			global::a.G.A(this.A, P_1);
			j();
			if (!global::a.O.A(this.m_A.Id, this.m_b))
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jh());
			}
			this.m_A = global::B.H.A(1040u, false, this.m_A.Id);
			if (this.m_A.IsInvalid)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JI());
			}
			this.C = P_0.A;
			a(P_0.A);
			a(P_0.a, P_0.A);
			B(P_0.A);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private bool J()
	{
		if (this.m_A == global::a.G.a.B)
		{
			return true;
		}
		if (this.m_A == global::a.G.a.a)
		{
			return false;
		}
		if (this.m_A == global::a.G.a.A)
		{
			return this.m_d;
		}
		return false;
	}

	private void j()
	{
		if (J())
		{
			global::a.f.A();
		}
	}

	private global::A.H K()
	{
		if (J())
		{
			return global::A.H.a;
		}
		return global::A.H.A;
	}

	private long E(long P_0)
	{
		if (J())
		{
			return global::a.f.A(P_0);
		}
		return P_0;
	}

	private long e(long P_0)
	{
		if (J())
		{
			return global::a.f.a(P_0);
		}
		return P_0;
	}

	public n()
		: this(false)
	{
	}

	private n(bool P_0)
	{
		this.m_d = P_0;
		Process[] processesByName = Process.GetProcessesByName(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gc());
		if (processesByName.Length != 1)
		{
			Process[] array = processesByName;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].Dispose();
			}
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ji());
		}
		this.m_A = processesByName[0];
		try
		{
			this.m_b = this.m_A.StartTime;
			this.m_F = this.m_A.MainModule.BaseAddress.ToInt64();
			this.m_A = global::a.G.A(this.m_A.MainModule.FileName, out var a2);
			this.m_A = a2;
			global::a.G.A(this.A, P_0);
			j();
			if (!global::a.O.A(this.m_A.Id, this.m_b))
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jh());
			}
			this.m_A = global::B.H.A(1040u, false, this.m_A.Id);
			if (this.m_A.IsInvalid)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JJ());
			}
			long num2 = this.m_F + E(112918560L) + 840;
			long num3 = G(num2 + 32);
			long num4 = G(num2 + 40);
			if (num4 < num3 || num4 - num3 > 6144 || (num4 - num3) % 48 != 0L)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jj());
			}
			for (long num5 = num3; num5 < num4; num5 += 48)
			{
				string key = g(num5 + 8);
				uint num6 = F(num5 + 40);
				if (num6 < 10)
				{
					this.m_A[key] = (int)num6;
				}
			}
			long num7 = this.m_F + E(112918560L) + 560;
			long num8 = G(num7 + 32);
			long num9 = G(num7 + 40);
			if (num9 < num8 || num9 - num8 > 12288 || (num9 - num8) % 48 != 0L)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JK());
			}
			for (long num10 = num8; num10 < num9; num10 += 48)
			{
				string text = g(num10 + 8);
				if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jk())
				{
					this.m_A = F(num10 + 40);
				}
				else if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JL())
				{
					this.m_a = F(num10 + 40);
				}
			}
			string[] array2 = new string[4]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fo(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fp(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FP(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jl()
			};
			foreach (string text2 in array2)
			{
				if (!this.m_A.ContainsKey(text2))
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JM() + text2);
				}
			}
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private byte[] A(long P_0, int P_1)
	{
		if (P_0 < 65536 || P_1 < 0 || P_1 > 65536)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jm());
		}
		byte[] array = new byte[P_1];
		if (!global::B.H.a(this.m_A, new IntPtr(P_0), array, (UIntPtr)(ulong)P_1, out var uIntPtr) || uIntPtr.ToUInt64() != (ulong)P_1)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JN());
		}
		return array;
	}

	private uint F(long P_0)
	{
		return BitConverter.ToUInt32(A(P_0, 4), 0);
	}

	private float f(long P_0)
	{
		float result = BitConverter.ToSingle(A(P_0, 4), 0);
		if (float.IsNaN(result) || float.IsInfinity(result))
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jn());
		}
		return result;
	}

	private long G(long P_0)
	{
		return BitConverter.ToInt64(A(P_0, 8), 0);
	}

	private long A(long P_0, uint P_1)
	{
		if (P_1 == uint.MaxValue)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JO());
		}
		return checked(P_0 + P_1);
	}

	private string g(long P_0)
	{
		long num = G(P_0 + 16);
		long num2 = G(P_0 + 24);
		if (num < 0 || num > 1024 || num2 < num)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jo());
		}
		if (num == 0L)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}
		return Encoding.UTF8.GetString(A((num2 < 16) ? P_0 : G(P_0), (int)num));
	}

	private string A(long P_0, long P_1, uint P_2)
	{
		long num = P_1 + 1640;
		if (F(num + 4) != uint.MaxValue)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JP() + P_2;
		}
		uint num2 = F(num + 12);
		if (num2 == 0 || num2 > 256)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JP() + P_2;
		}
		string text = Encoding.Unicode.GetString(A(A(P_0, F(num + 8)), checked((int)num2 * 2)));
		char[] trimChars = new char[1];
		return text.TrimEnd(trimChars);
	}

	private float A(byte[] P_0, string P_1)
	{
		float result = BitConverter.ToSingle(P_0, this.m_A[P_1] * 4);
		if (float.IsNaN(result) || float.IsInfinity(result))
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jp());
		}
		return result;
	}

	private long A(long P_0, long P_1)
	{
		int num = BitConverter.ToUInt16(A(P_0 + 76, 2), 0);
		int num2 = A(this.m_F + E(P_1) + num * 128, 1)[0];
		if (num2 == 255)
		{
			return 0L;
		}
		long num3 = G(this.m_F + E(102260448L)) + F(P_0 + 64) + num2 * 24;
		if ((A(num3 + 16, 1)[0] & 1) == 0)
		{
			return 0L;
		}
		uint num4 = F(num3);
		long num5 = G(num3 + 8);
		if (num5 == 0L)
		{
			return 0L;
		}
		switch (num4)
		{
		case 8u:
			return num5;
		default:
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JQ());
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
			return G(this.m_F + E(102260400L) + num4 * 16) + (uint)num5;
		}
	}

	private string H(long P_0)
	{
		if (!this.m_A.TryGetValue(P_0, out var value))
		{
			value = g(P_0 + 40);
			if (this.m_A.Count > 10000)
			{
				this.m_A.Clear();
			}
			this.m_A[P_0] = value;
		}
		return value;
	}

	private void A(long P_0, global::A.p P_1)
	{
		uint num = (P_1.A ? this.m_a : this.m_A);
		if (num == uint.MaxValue)
		{
			return;
		}
		long num2 = A(P_1.A, 103319637L);
		if (num2 == 0L)
		{
			return;
		}
		uint num3 = F(num2 + 76);
		if (num3 == uint.MaxValue)
		{
			return;
		}
		long num4 = A(P_0, num3);
		uint num5 = F(num4 + 4);
		uint num6 = F(num4);
		if (num5 > 128)
		{
			return;
		}
		Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
		if (num5 != 0)
		{
			byte[] value = A(A(P_0, num6), (int)(num5 * 28));
			for (int num7 = 0; num7 < num5; num7++)
			{
				int num8 = num7 * 28;
				uint num9 = BitConverter.ToUInt32(value, num8);
				int num10 = BitConverter.ToInt32(value, num8 + 4);
				int num11 = BitConverter.ToInt32(value, num8 + 8);
				if (num9 == num)
				{
					if (num10 < 0 || num10 > 100000 || num11 < 0 || num11 > 100000)
					{
						return;
					}
					dictionary[num9] = num10;
				}
			}
		}
		if (F(num4 + 4) != num5 || F(num4) != num6)
		{
			return;
		}
		foreach (KeyValuePair<uint, int> item in dictionary)
		{
			P_1.A[item.Key] = item.Value;
		}
		P_1.a = true;
	}

	private void A(long P_0, long P_1, O P_2)
	{
		long num = A(P_0, F(P_1 + 1792));
		uint num2 = F(num + 40);
		uint num3 = F(num + 36);
		if (num2 > 4096)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jq());
		}
		if (num2 == 0)
		{
			P_2.c = true;
			P_2.d = true;
			return;
		}
		HashSet<long> hashSet = new HashSet<long>();
		HashSet<long> hashSet2 = new HashSet<long>();
		byte[] value = A(A(P_0, num3), checked((int)num2 * 4));
		for (int num4 = 0; num4 < num2; num4++)
		{
			uint num5 = BitConverter.ToUInt32(value, num4 * 4);
			if (num5 == uint.MaxValue)
			{
				continue;
			}
			long num6 = A(P_0, num5);
			if (A(P_0, F(num6 + 88)) != P_1)
			{
				continue;
			}
			A(P_0, num6, P_2, hashSet);
			string text = H(G(num6 + 56));
			A(P_0, num6, text, P_2);
			A(num6, text, P_2);
			if (global::a.j.A(text))
			{
				A(P_0, num6, P_2, hashSet2, true);
			}
			try
			{
				A(P_0, num6, text, P_2, false);
			}
			catch
			{
				P_2.B = false;
			}
			if (text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JR()))
			{
				string[] array = text.Split('\\');
				if (array.Length >= 3)
				{
					P_2.b = array[2];
				}
				A(P_0, num6, P_2, hashSet2, false);
				int num7 = global::A.t.A(text);
				if (num7 == 0)
				{
					continue;
				}
				long num8 = A(num6, 103319626L);
				long num9 = A(num6, 103319627L);
				if (num8 == 0L || num9 == 0L)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jr());
				}
				if (f(num8 + 108) <= 0f)
				{
					continue;
				}
				P_2.A.Add(new global::b.K
				{
					A = num7,
					A = num6 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + F(num6 + 96),
					A = (F(num9 + 40) == 0)
				});
			}
			if (P_2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba() && global::A.p.a(text))
			{
				long num10 = A(num6, 103319626L);
				if (num10 != 0L && !(f(num10 + 108) <= 0f))
				{
					global::A.p p2 = new global::A.p
					{
						A = num6 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + F(num6 + 96),
						a = global::A.p.B(text),
						A = num6,
						A = global::A.p.A(text)
					};
					A(P_0, p2);
					P_2.A.Add(p2);
				}
			}
		}
		if (F(num + 40) != num2 || F(num + 36) != num3)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jq());
		}
		P_2.d = true;
		P_2.c = true;
	}

	public List<O> k()
	{
		if (this.m_A.HasExited || this.m_A.StartTime != this.m_b)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JS());
		}
		long num = c();
		uint num2 = (uint)num;
		uint A = (uint)((ulong)num >> 32);
		if (num2 == 0 || A >= 16)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Js());
		}
		long num3 = G(this.m_F + E(102260400L));
		uint num4 = F(this.m_F + E(98614988L));
		long num5 = this.A(num3, num4);
		long num6 = this.A(num3, F(num5 + 252));
		uint num7 = F(num6 + 16);
		uint num8 = F(num6 + 12);
		if (num7 == 0 || num7 > 16)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JT());
		}
		long num9 = this.A(num3, num8);
		long num10 = this.A(num3, F(num5 + 284));
		long num11 = this.A(num3, num9, num7, num);
		string text = this.m_A.Id + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + this.m_b.Ticks + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num5 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num11 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num2;
		if (this.C != text)
		{
			this.m_A.Clear();
		}
		uint num12 = F(num10 + A * 4);
		List<O> list = new List<O>();
		for (uint num13 = 0u; num13 < num7; num13++)
		{
			long num14 = this.A(num3, F(num9 + num13 * 4));
			uint num15 = F(num14 + 12);
			if (num15 >= 16)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jt());
			}
			O o2 = new O();
			o2.A = num14;
			o2.a = G(num14 + 8);
			o2.A = num15;
			o2.a = F(num10 + num15 * 4);
			o2.A = this.A(num3, num14, num15);
			o2.a = ((num15 == A) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba() : ((o2.a == 10000 || num12 == 10000) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DL() : ((o2.a == 10002 || num12 == 10002) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() : ((o2.a == num12) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))));
			byte[] array = this.A(num14 + 1704, 40);
			o2.A = this.A(array, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fo());
			o2.a = this.A(array, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fp());
			o2.B = this.A(array, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FP());
			o2.b = this.A(array, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jl());
			o2.C = f(num14 + 1256) + f(num14 + 1304);
			if (o2.C < 0f || o2.C > 10000f)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JU());
			}
			uint num16 = F(num14 + 788);
			uint num17 = F(num14 + 784);
			if (num16 > 1024)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ju());
			}
			if (num16 != 0)
			{
				long num18 = this.A(num3, num17);
				for (uint num19 = 0u; num19 < num16; num19++)
				{
					long num20 = num18 + num19 * 104;
					if (this.A(num20 + 16, 1)[0] == 0)
					{
						o2.A.Add(g(G(num20) + 40));
					}
				}
				if (F(num14 + 788) != num16 || F(num14 + 784) != num17)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ju());
				}
			}
			string[] array2 = (from P_0 in o2.A
				where P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xf())
				select P_0.Split('\\') into P_0
				where P_0.Length >= 5
				select P_0[3]).Distinct().ToArray();
			o2.B = ((array2.Length == 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JV() : string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cE(), array2.Select(delegate(string P_0)
			{
				if (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xG())
				{
					return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xg();
				}
				return (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xH()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FW() : P_0;
			})));
			this.A(num3, num14, o2);
			list.Add(o2);
		}
		if (F(this.m_F + E(98614988L)) != num4 || G(this.m_F + E(102260400L)) != num3 || F(num6 + 16) != num7 || F(num6 + 12) != num8 || c() != num || G(num11 + 8) != num)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jv());
		}
		if (!list.Any((O P_0) => P_0.A == A))
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JW());
		}
		this.A(list, num3, num5);
		a(text);
		this.C = text;
		a(num3, num4);
		this.A(list, num);
		return list;
	}

	public void Dispose()
	{
		E();
		g();
		if (this.m_A != null)
		{
			this.m_A.Dispose();
		}
		if (this.m_A != null)
		{
			this.m_A.Dispose();
		}
	}

	private void A(long P_0, long P_1, O P_2, HashSet<long> P_3, bool P_4 = false)
	{
		try
		{
			long num = A(P_1, 103319626L);
			if (num == 0L || f(num + 108) <= 0f)
			{
				return;
			}
			long num2 = (P_4 ? A(A(P_0, F(P_1 + 124)), 103319618L) : A(P_1, 103319633L));
			if (num2 == 0L)
			{
				return;
			}
			int num3 = (P_4 ? 16 : 52);
			uint num4 = F(num2 + num3);
			if (num4 == uint.MaxValue)
			{
				return;
			}
			long num5 = A(P_0, num4);
			if (!P_3.Add(num5))
			{
				return;
			}
			uint num6 = F(num5 + 52);
			uint num7 = F(num5 + 48);
			if (num6 > 128)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jw());
			}
			if (num6 == 0)
			{
				return;
			}
			long num8 = A(P_0, num7);
			byte[] value = A(num8, 192);
			uint num9 = BitConverter.ToUInt32(value, 184);
			long num10 = A(P_0, num9);
			Q q2 = null;
			Q q3 = null;
			float? num11 = null;
			if (!P_4 && G(num10) == this.m_F + E(64584880L))
			{
				long num12 = G(num10 + 16);
				string text = H(num12);
				if (!text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bS(), StringComparison.Ordinal))
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JX());
				}
				q2 = new Q(text, BitConverter.ToSingle(value, 48), BitConverter.ToSingle(value, 52), num5 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + BitConverter.ToUInt32(value, 4));
				if (G(num10 + 16) != num12 || G(num10) != this.m_F + E(64584880L))
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jx());
				}
			}
			long num13 = this.m_F + E(P_4 ? 64557040 : 64557304);
			if (G(num10) == num13)
			{
				long num14 = G(num10 + 16);
				string text2 = H(num14);
				if (!P_4 && global::a.g.A(text2))
				{
					float num15 = BitConverter.ToSingle(value, 48);
					float num16 = BitConverter.ToSingle(value, 52);
					global::a.g.A(num15);
					global::a.g.A(num16);
					if (num16 <= 0f || num15 > num16)
					{
						throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JY());
					}
					num11 = num15;
				}
				if (P_4 && global::a.j.a(text2))
				{
					q3 = new Q(text2, BitConverter.ToSingle(value, 48), BitConverter.ToSingle(value, 52), P_1 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + F(P_1 + 96));
				}
				if (G(num10 + 16) != num14 || G(num10) != num13)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jy());
				}
			}
			if (F(num2 + num3) != num4 || F(num5 + 52) != num6 || F(num5 + 48) != num7 || F(num8 + 184) != num9 || G(num8) != BitConverter.ToInt64(value, 0))
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jw());
			}
			if (q2 != null)
			{
				P_2.a.Add(q2);
			}
			if (q3 != null)
			{
				P_2.A.Add(q3);
			}
			if (num11.HasValue)
			{
				P_2.A = (P_2.A.HasValue ? new float?(Math.Min(P_2.A.Value, num11.Value)) : num11);
			}
		}
		catch (Exception)
		{
			if (P_4)
			{
				P_2.a = false;
			}
			else
			{
				P_2.D = false;
			}
		}
	}

	private string A(long P_0, long P_1, ulong P_2)
	{
		long num = A(P_0, P_1, (uint)(P_2 >> 32), (uint)P_2, false);
		if (num == 0L)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}
		long num2 = A(num, 103319637L);
		if (num2 == 0L)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}
		uint num3 = F(num2 + 76);
		if (num3 == uint.MaxValue)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}
		long num4 = A(P_0, num3);
		uint num5 = F(num4 + 44);
		uint num6 = F(num4 + 40);
		if (num5 == 0 || num5 > 64 || num6 == uint.MaxValue)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}
		byte[] value = A(A(P_0, num6), (int)(num5 * 4));
		for (int num7 = 0; num7 < num5; num7++)
		{
			uint num8 = BitConverter.ToUInt32(value, num7 * 4);
			if (num8 == uint.MaxValue)
			{
				continue;
			}
			long num9 = A(P_0, num8);
			if (G(num9) != this.m_F + E(64568064L))
			{
				continue;
			}
			byte[] array = A(num9, 164);
			if (global::A.s.A(array))
			{
				string text = H(BitConverter.ToInt64(array, 56));
				if (text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.JZ(), StringComparison.Ordinal) && (text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jz()) || text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jA()) || text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ja())))
				{
					return num9 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + text;
				}
			}
		}
		return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
	}

	private byte[] a(long P_0, int P_1)
	{
		if (P_1 <= 65536)
		{
			return A(P_0, P_1);
		}
		byte[] array = new byte[P_1];
		for (int num = 0; num < P_1; num += 65536)
		{
			byte[] array2 = A(P_0 + num, Math.Min(65536, P_1 - num));
			Buffer.BlockCopy(array2, 0, array, num, array2.Length);
		}
		return array;
	}

	private string A(long P_0, long P_1, byte[] P_2, string P_3)
	{
		string text = global::A.Q.b(P_3);
		if (text != null)
		{
			return text;
		}
		if (P_2[16] == 3)
		{
			uint num = BitConverter.ToUInt32(P_2, 20);
			uint num2 = BitConverter.ToUInt32(P_2, 24);
			ulong key = ((ulong)num << 32) | num2;
			if (this.m_A.TryGetValue(key, out var value))
			{
				return value;
			}
			try
			{
				long num3 = A(P_0, P_1, num, num2, false);
				if (num3 != 0L)
				{
					uint num4 = F(num3 + 124);
					value = global::b.k.A((num4 != uint.MaxValue) ? H(G(A(P_0, num4) + 56)) : global::A.r.a(H(G(num3 + 56))));
					if (!string.IsNullOrWhiteSpace(value))
					{
						if (this.m_A.Count > 4096)
						{
							this.m_A.Clear();
						}
						this.m_A[key] = value;
						return value;
					}
				}
			}
			catch
			{
			}
		}
		string text2 = (P_3.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jB()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jG() : (P_3.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bW()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jf() : (P_3.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jb()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jF() : (P_3.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jC()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.je() : (P_3.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jc()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jE() : (P_3.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jD()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jd() : null))))));
		if (text2 != null)
		{
			return global::b.k.A(text2);
		}
		return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jg();
	}

	public List<r> A(string P_0, IEnumerable<T> P_1, bool P_2 = false)
	{
		B(P_0);
		long num = this.m_G;
		long num2 = this.A(num, this.m_B);
		long num3 = this.A(num, F(num2 + 260));
		if (this.m_f != num3)
		{
			this.m_f = num3;
			this.m_a = new ulong[0];
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.a();
		}
		uint A = F(num3 + 24);
		uint num4 = F(num3 + 8);
		uint num5 = F(num3 + 20);
		if (A == 0 || A > 65536)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jH());
		}
		byte[] value;
		byte[] value2;
		checked
		{
			value = a(this.A(num, num4), (int)A * 8);
			value2 = a(this.A(num, num5), (int)A * 4);
		}
		if (this.m_a.Length < A)
		{
			Array.Resize(ref this.m_a, (int)A);
		}
		Dictionary<long, T> dictionary = (from t2 in P_1
			group t2 by t2.A).ToDictionary((IGrouping<long, T> grouping) => grouping.Key, (IGrouping<long, T> source) => source.First());
		List<r> list = new List<r>();
		uint[] array = this.m_A.Keys.Where((uint num19) => num19 >= A).ToArray();
		foreach (uint key in array)
		{
			this.m_A.Remove(key);
		}
		for (uint num7 = 0u; num7 < A; num7++)
		{
			uint num8 = BitConverter.ToUInt32(value, (int)(num7 * 8));
			uint num9 = BitConverter.ToUInt32(value2, (int)(num7 * 4));
			ulong num10 = ((ulong)num9 << 32) | num8;
			if (num8 == uint.MaxValue || num9 == 0)
			{
				this.m_A.Remove(num7);
				this.m_a[num7] = num10;
				continue;
			}
			if (this.m_a[num7] != num10)
			{
				this.m_A.Remove(num7);
				try
				{
					long num11 = this.A(num, num8);
					byte[] value3 = this.A(num11, 128);
					if (BitConverter.ToUInt32(value3, 104) != num9 || BitConverter.ToUInt32(value3, 108) != num7)
					{
						continue;
					}
					long num12 = BitConverter.ToInt64(value3, 56);
					string text = H(num12);
					if (string.IsNullOrEmpty(text))
					{
						continue;
					}
					if (text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jh(), StringComparison.Ordinal) && text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jI()))
					{
						this.m_A[num7] = new B
						{
							A = num11,
							A = num9,
							a = num7,
							a = num12
						};
					}
					this.m_a[num7] = num10;
					goto IL_030a;
				}
				catch
				{
				}
				continue;
			}
			goto IL_030a;
			IL_030a:
			if (!this.m_A.TryGetValue(num7, out var value4))
			{
				continue;
			}
			try
			{
				long num13 = value4.A;
				byte[] array2 = this.A(num13, 392);
				uint num14 = BitConverter.ToUInt32(array2, 388);
				if (num14 == uint.MaxValue)
				{
					continue;
				}
				long num15 = this.A(num, num14);
				long num16 = this.A(num13, 103319634L);
				if (num16 == 0L || G(num16) != this.m_F + E(65307976L))
				{
					continue;
				}
				byte[] array3 = this.A(num16, 208);
				uint num17 = BitConverter.ToUInt32(array3, 56);
				if (num17 == uint.MaxValue || !dictionary.TryGetValue(this.A(num, num17), out var value5))
				{
					continue;
				}
				string text2 = H(BitConverter.ToInt64(array3, 64));
				string text3 = null;
				bool flag = false;
				string text4 = null;
				if (text3 == null && text4 == null && !S.A(text2))
				{
					continue;
				}
				byte[] array4 = this.A(num15, 80);
				if (S.A(array2, array4, array3, this.m_F, num, num13, num9, num7, out var r2, S.a(text2) || global::A.Q.B(text2) || text4 != null, text3 != null, flag, E) && F(num13 + 104) == num9 && F(num13 + 108) == num7 && G(num13 + 56) == value4.a && F(num13 + 388) == num14 && this.A(num15 + 16, 16).SequenceEqual(array4.Skip(16).Take(16)) && (value4.A == 0L || value4.A == r2.A))
				{
					value4.A = r2.A;
					if (flag)
					{
						r2.A |= 9223372036854775808uL;
					}
					r2.B = value5.A;
					r2.A = value5.A;
					r2.a = text2;
					r2.B = text3;
					r2.A = text3 ?? text4 ?? this.A(num, num3, array3, text2);
					list.Add(r2);
				}
			}
			catch
			{
			}
		}
		if (F(num3 + 24) != A || F(num3 + 8) != num4 || F(num3 + 20) != num5 || this.A(num, F(num2 + 260)) != num3)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ji());
		}
		B(P_0);
		double num18 = (double)Stopwatch.GetTimestamp() * 1000.0 / (double)Stopwatch.Frequency;
		HashSet<ulong> hashSet = new HashSet<ulong>(from r3 in list
			where r3.B == null && this.m_A[r3.a].A == 0
			select r3.A);
		foreach (ulong item in hashSet)
		{
			this.m_A.A(item);
		}
		foreach (U.A item2 in this.m_A.A)
		{
			if (!(num18 < item2.a) || hashSet.Contains(item2.A))
			{
				try
				{
					this.m_A.A(item2.A, this.A(num, num3, item2.A));
					item2.a = num18 + 100.0;
				}
				catch
				{
				}
			}
		}
		foreach (r item3 in from r3 in list
			where r3.B == null
			orderby r3.A
			select r3)
		{
			B b2 = this.m_A[item3.a];
			if (b2.A == 0)
			{
				b2.A = this.m_A.A(item3.A, num18);
			}
			item3.A = b2.A;
		}
		this.m_A.A(num18);
		return list;
	}

	public bool L()
	{
		try
		{
			if (string.IsNullOrEmpty(this.C) || this.m_A.HasExited || this.m_A.StartTime != this.m_b)
			{
				return false;
			}
			B(this.C);
			long num = c();
			uint num2 = (uint)num;
			if (num2 == 0 || (uint)(num >>> 32) >= 16)
			{
				return false;
			}
			long num3 = A(this.m_G, this.m_B);
			long num4 = A(this.m_G, F(num3 + 252));
			long num5 = A(this.m_G, A(this.m_G, F(num4 + 12)), F(num4 + 16), num);
			return this.C == this.m_A.Id + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + this.m_b.Ticks + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num3 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num5 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num2;
		}
		catch
		{
			return false;
		}
	}

	private void A(long P_0, long P_1, long P_2, string P_3, O P_4)
	{
		if ((!(P_4.a != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()) || !(P_4.a != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()) || !(P_4.a != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl())) && global::b.b.A(P_3))
		{
			global::b.d d2 = P_4.A.FirstOrDefault((global::b.d d3) => d3.A == P_2);
			if (d2 == null)
			{
				d2 = new global::b.d
				{
					A = P_2,
					a = G(P_2 + 104),
					A = P_3,
					A = P_4.A
				};
				P_4.A.Add(d2);
			}
			d2.A.Add(new global::b.C
			{
				A = P_1,
				a = A(P_0, F(P_1 + 88)),
				A = F(P_1 + 96),
				a = F(P_1 + 124)
			});
		}
	}

	private void a(long P_0, uint P_1)
	{
		this.m_G = P_0;
		this.m_B = P_1;
	}

	private void B(string P_0)
	{
		if (this.m_A.HasExited || this.C != P_0 || this.m_G == 0L || G(this.m_F + E(102260400L)) != this.m_G || F(this.m_F + E(98614988L)) != this.m_B)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jJ());
		}
	}

	public List<global::b.A> A(string P_0, IEnumerable<global::b.d> P_1)
	{
		B(P_0);
		List<global::b.A> list = new List<global::b.A>();
		foreach (global::b.d item in P_1)
		{
			if (!item.A)
			{
				continue;
			}
			try
			{
				if (G(item.A + 104) != item.a)
				{
					continue;
				}
				List<float[]> list2 = new List<float[]>();
				foreach (global::b.C item2 in item.A)
				{
					try
					{
						byte[] value = A(item2.A, 128);
						if (BitConverter.ToUInt32(value, 96) != item2.A || BitConverter.ToUInt32(value, 124) != item2.a || A(this.m_G, BitConverter.ToUInt32(value, 88)) != item2.a)
						{
							continue;
						}
						long num = A(item2.A, 103319626L);
						if (num != 0L && !(f(num + 108) <= 0f))
						{
							float num2 = BitConverter.ToSingle(value, 44);
							float num3 = BitConverter.ToSingle(value, 48);
							float num4 = BitConverter.ToSingle(value, 52);
							if (global::b.b.A(num2, num3, num4))
							{
								list2.Add(new float[3] { num2, num3, num4 });
							}
						}
					}
					catch
					{
					}
				}
				if (G(item.A + 104) == item.a)
				{
					global::b.A a2 = global::b.b.A(item, list2);
					if (a2 != null)
					{
						A(a2, item);
						a(a2, item);
						list.Add(a2);
					}
				}
			}
			catch
			{
			}
		}
		B(P_0);
		return list;
	}

	public List<global::b.g> A(IEnumerable<O> P_0)
	{
		List<global::b.g> list = new List<global::b.g>();
		foreach (O item in P_0)
		{
			if (item.a != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA())
			{
				continue;
			}
			long num = item.A + 1016;
			uint num2 = F(num + 44);
			uint num3 = F(num + 40);
			if (num2 > 256)
			{
				continue;
			}
			byte[] value = ((num2 == 0) ? new byte[0] : A(A(this.m_G, num3), (int)(num2 * 4)));
			for (int num4 = 0; num4 < num2; num4++)
			{
				try
				{
					uint num5 = BitConverter.ToUInt32(value, num4 * 4);
					if (num5 == uint.MaxValue)
					{
						continue;
					}
					long num6 = A(this.m_G, num5);
					if (G(num6) != this.m_F + E(64568064L))
					{
						continue;
					}
					byte[] array = A(num6, 164);
					if (!global::b.G.A(array, this.m_F, item.a, E))
					{
						continue;
					}
					string text = H(BitConverter.ToInt64(array, 56));
					string text2 = global::b.G.A(text);
					if (text2 != null)
					{
						uint num7 = BitConverter.ToUInt32(array, 12);
						uint num8 = BitConverter.ToUInt32(array, 8);
						if (num7 != 0 && num7 <= 16 && num8 != uint.MaxValue && A(this.m_G, A(A(this.m_G, num8), (int)(num7 * 20)), 0, out var num9, out var num10, out var num11) && A(num6, 164).SequenceEqual(array))
						{
							list.Add(new global::b.g
							{
								A = item.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + num6 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + BitConverter.ToUInt32(array, 68),
								A = item.A,
								a = text,
								B = text2,
								A = (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hR()),
								A = num9,
								a = num10,
								B = num11
							});
						}
					}
				}
				catch
				{
				}
			}
			if (F(num + 44) != num2 || F(num + 40) != num3)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jj());
			}
		}
		return list;
	}

	private static bool A(uint P_0)
	{
		return (P_0 & 0x20) != 0;
	}

	private IEnumerable<long> h(long P_0)
	{
		HashSet<long> hashSet = new HashSet<long>();
		int[] array = new int[2] { 200, 472 };
		foreach (int num2 in array)
		{
			try
			{
				long num3 = G(P_0 + num2);
				if (num3 > 65536 && num3 != P_0 && G(num3 + 136) == P_0)
				{
					hashSet.Add(num3);
				}
			}
			catch
			{
			}
		}
		try
		{
			long num4 = G(P_0 + 504);
			if (num4 > 65536)
			{
				uint num5 = F(num4 + 124);
				long num6 = G(num4 + 112);
				if (num5 <= 512 && num6 > 65536)
				{
					byte[] value = A(num6, (int)(num5 * 8));
					for (int num7 = 0; num7 < num5; num7++)
					{
						long num8 = BitConverter.ToInt64(value, num7 * 8);
						try
						{
							if (num8 > 65536 && G(num8 + 136) == P_0)
							{
								hashSet.Add(num8);
							}
						}
						catch
						{
						}
					}
				}
			}
		}
		catch
		{
		}
		return hashSet;
	}

	private long I(long P_0)
	{
		long num = P_0;
		HashSet<long> hashSet = new HashSet<long>();
		for (int num2 = 0; num2 < 64; num2++)
		{
			if (num == 0L)
			{
				break;
			}
			if (!hashSet.Add(num))
			{
				return 0L;
			}
			if (G(num) == this.m_F + E(65713904L))
			{
				break;
			}
			num = G(num + 136);
		}
		if (num == 0L || G(num) != this.m_F + E(65713904L))
		{
			return 0L;
		}
		Queue<long> queue = new Queue<long>();
		queue.Enqueue(num);
		hashSet.Clear();
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (queue.Count > 0 && hashSet.Count < 2048 && stopwatch.ElapsedMilliseconds < 60)
		{
			long num3 = queue.Dequeue();
			if (!hashSet.Add(num3))
			{
				continue;
			}
			try
			{
				long num4 = G(num3);
				if (num4 == this.m_F + E(65513472L))
				{
					return num3;
				}
				if (num4 == this.m_F + E(65511760L))
				{
					continue;
				}
				foreach (long item in C(num3))
				{
					queue.Enqueue(item);
				}
			}
			catch
			{
			}
		}
		return 0L;
	}

	private global::a.z A(long P_0, out bool P_1)
	{
		P_1 = false;
		if (P_0 < 65536 || G(P_0) != this.m_F + E(65513472L))
		{
			throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jK());
		}
		byte[] array = A(P_0, 552);
		byte[] array2 = A(P_0, array);
		float num = a(array2, a(111989640L));
		float num2 = a(array2, a(111989680L));
		long num3 = BitConverter.ToInt64(array, 528);
		if (num3 < 65536)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jk());
		}
		byte[] value = A(num3, 344);
		float num4 = BitConverter.ToSingle(value, 296);
		float num5 = BitConverter.ToSingle(value, 300);
		if (num4 < 10f || num5 < 10f || num4 > 20000f || num5 > 20000f)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jL());
		}
		global::a.y y2 = global::a.y.A;
		long num6 = num3;
		HashSet<long> hashSet = new HashSet<long>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		float num7 = 0f;
		float num8 = 0f;
		while (num6 != 0L)
		{
			if (!hashSet.Add(num6) || hashSet.Count > 64)
			{
				throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jl());
			}
			byte[] array3 = A(num6, 344);
			long num9 = BitConverter.ToInt64(array3, 0) - this.m_F;
			if (num6 == P_0)
			{
				flag = true;
			}
			if (num9 == E(65713904L))
			{
				flag2 = true;
			}
			flag4 &= A(BitConverter.ToUInt32(array3, 192));
			long num10 = A(A(num6, array3), 40);
			if (num6 == num3 && num10 == 0L)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jM());
			}
			if (num10 != 0L)
			{
				y2 = y2.A(B(num10));
			}
			float num11 = BitConverter.ToSingle(array3, 168);
			float num12 = BitConverter.ToSingle(array3, 172);
			if (!global::b.b.A(num11, 0f, num12))
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jm());
			}
			y2 = y2.A(global::a.y.A(num11, num12));
			long num13 = BitConverter.ToInt64(array3, 136);
			if (G(num6 + 136) != num13)
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jN());
			}
			if (num13 == 0L)
			{
				flag3 = num9 == E(64823200L);
				num7 = BitConverter.ToSingle(array3, 296);
				num8 = BitConverter.ToSingle(array3, 300);
			}
			num6 = num13;
		}
		if (!flag || !flag2 || !flag3)
		{
			throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jn());
		}
		if (num7 < 200f || num8 < 200f || num7 > 20000f || num8 > 20000f)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jO());
		}
		if (G(P_0 + 528) != num3 || G(P_0) != this.m_F + E(65513472L))
		{
			throw new global::B.c(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jo());
		}
		global::a.z z2 = global::a.y.A(num, num2, num4, num5, num7, num8, y2);
		if (!z2.A || z2.A < 0f || z2.A > num7 || z2.a < 0f || z2.a > num8)
		{
			throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jP());
		}
		P_1 = flag4;
		return z2;
	}

	public bool a(out global::a.z P_0)
	{
		P_0 = null;
		long num;
		long num2;
		long num3;
		lock (this.m_A)
		{
			num = this.m_g;
			num2 = this.m_d;
			num3 = this.m_c;
		}
		bool result;
		try
		{
			B(this.C);
			if (num != 0L)
			{
				goto IL_0126;
			}
			if (num3 == 0L)
			{
				A(out var _);
				lock (this.m_A)
				{
					num3 = this.m_c;
				}
			}
			if (num3 == 0L)
			{
				this.C = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jp();
				result = false;
			}
			else if (DateTime.UtcNow < this.m_C)
			{
				result = false;
			}
			else
			{
				this.m_C = DateTime.UtcNow.AddMilliseconds(500.0);
				num = I(num3);
				if (num != 0L)
				{
					lock (this.m_A)
					{
						if (!this.m_C && this.m_d == num2)
						{
							this.m_g = num;
							goto end_IL_00f1;
						}
						result = false;
						goto end_IL_0038;
						end_IL_00f1:;
					}
					goto IL_0126;
				}
				this.C = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Jg();
				result = false;
			}
			goto end_IL_0038;
			IL_0126:
			bool flag;
			global::a.z z3 = A(num, out flag);
			B(this.C);
			lock (this.m_A)
			{
				if (!this.m_C && this.m_d == num2)
				{
					goto end_IL_0147;
				}
				result = false;
				goto end_IL_0038;
				end_IL_0147:;
			}
			global::b.J.A(this.m_A.Id, this.C, flag);
			this.C = (flag ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jQ());
			if (!flag)
			{
				result = false;
			}
			else
			{
				P_0 = z3;
				result = true;
			}
			end_IL_0038:;
		}
		catch (Exception ex)
		{
			this.C = ex.Message;
			if (ex is global::B.c)
			{
				lock (this.m_A)
				{
					if (this.m_d == num2)
					{
						this.m_g = 0L;
					}
				}
			}
			result = false;
		}
		return result;
	}

	private void A(long P_0, long P_1, O P_2, HashSet<long> P_3)
	{
		uint num = F(P_1 + 124);
		if (num == uint.MaxValue)
		{
			return;
		}
		long num2 = A(P_1, 103319626L);
		if (num2 == 0L || f(num2 + 108) <= 0f)
		{
			return;
		}
		long num3 = A(P_0, num);
		long item = G(num3 + 104);
		string text = H(G(num3 + 56));
		if (text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bS()) && !text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jq()) && !text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jR()) && !text.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jr()))
		{
			try
			{
				A(P_0, P_1, num3, text, P_2);
			}
			catch
			{
				P_2.C = false;
			}
			if (!P_3.Contains(item))
			{
				P_3.Add(item);
				P_2.A.TryGetValue(text, out var value);
				P_2.A[text] = value + 1;
			}
		}
	}

	private void A(long P_0, string P_1, O P_2)
	{
		if (!global::a.H.a(P_1))
		{
			return;
		}
		long num = A(P_0, 103319626L);
		if (num != 0L && !(f(num + 108) <= 0f))
		{
			long num2 = A(P_0, 103319627L);
			if (num2 == 0L || F(num2 + 40) == 0)
			{
				P_2.A.Add(new global::a.x
				{
					A = P_0,
					A = P_1,
					a = P_2.a,
					A = global::a.H.A(P_1),
					A = f(P_0 + 44),
					a = f(P_0 + 48),
					B = f(P_0 + 52)
				});
			}
		}
	}
}
