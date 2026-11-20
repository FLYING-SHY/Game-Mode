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
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;
using a;
using b;

namespace B;

internal static class M
{
	[STAThread]
	public static int A(string[] P_0)
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		if (P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iq()))
		{
			try
			{
				using (m m2 = new m())
				{
					List<N> list = m2.i();
					List<string> list2 = new List<string>
					{
						_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IR() + list.Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ir(),
						_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IS()
					};
					foreach (N item in list)
					{
						list2.Add(item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Is() + item.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IT() + item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.It() + item.C + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IU() + global::A.T.A(item) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iu() + item.A.Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IV() + item.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iv() + item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IW() + item.B + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iw() + item.b + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IX() + global::A.U.a(item) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ix() + item.A.Count);
					}
					File.WriteAllLines(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IY()), list2);
				}
				return 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IY()), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fl() + ex);
				return 1;
			}
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		if (P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iy()))
		{
			using (l l2 = new l())
			{
				l2.ShowInTaskbar = false;
				l2.Opacity = 0.0;
				l2.Show();
				Application.DoEvents();
				using (Bitmap bitmap = new Bitmap(l2.Width, l2.Height))
				{
					l2.DrawToBitmap(bitmap, new Rectangle(0, 0, l2.Width, l2.Height));
					bitmap.Save(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IZ()));
				}
				l2.Close();
			}
			return 0;
		}
		Application.Run(new l());
		return 0;
	}
}
internal sealed class m : IDisposable
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

		public global::A.k A;
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

		public static Func<N, bool> A;

		public static Func<N, bool> a;

		public static Func<N, IEnumerable<global::b.c>> A;

		public static Func<N, int> A;

		public static Func<Z, bool> A;

		public static Func<Z, bool> a;

		public static Func<N, IEnumerable<global::b.c>> a;

		public static Func<N, S> A;

		public static Func<S, long> A;

		public static Func<IGrouping<long, S>, long> A;

		public static Func<IGrouping<long, S>, S> A;

		public static Func<KeyValuePair<long, DateTime>, long> A;

		public static Func<KeyValuePair<long, DateTime>, DateTime> A;

		public static Func<string, bool> A;

		public static Func<string, string[]> A;

		public static Func<string[], bool> A;

		public static Func<string[], string> A;

		public static Func<string, string> A;

		public static Func<S, long> a;

		public static Func<IGrouping<long, S>, long> a;

		public static Func<IGrouping<long, S>, S> a;

		public static Func<q, ulong> A;

		public static Func<q, bool> A;

		public static Func<q, uint> A;

		internal bool A(N P_0)
		{
			return P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA();
		}

		internal bool a(N P_0)
		{
			return !P_0.b;
		}

		internal IEnumerable<global::b.c> B(N P_0)
		{
			return P_0.A;
		}

		internal int b(N P_0)
		{
			return P_0.A.Count;
		}

		internal bool A(Z P_0)
		{
			return P_0.A != null;
		}

		internal bool a(Z P_0)
		{
			return P_0.A != null;
		}

		internal IEnumerable<global::b.c> C(N P_0)
		{
			return P_0.A;
		}

		internal S c(N P_0)
		{
			return new S
			{
				A = P_0.A,
				A = P_0.A,
				A = P_0.A,
				A = P_0.a
			};
		}

		internal long A(S P_0)
		{
			return P_0.A;
		}

		internal long A(IGrouping<long, S> P_0)
		{
			return P_0.Key;
		}

		internal S a(IGrouping<long, S> P_0)
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
			return P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xB());
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
			if (P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xb())
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xC();
			}
			if (P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xc())
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fs();
			}
			return P_0;
		}

		internal long a(S P_0)
		{
			return P_0.A;
		}

		internal long B(IGrouping<long, S> P_0)
		{
			return P_0.Key;
		}

		internal S b(IGrouping<long, S> P_0)
		{
			return P_0.First();
		}

		internal ulong A(q P_0)
		{
			return P_0.A;
		}

		internal bool a(q P_0)
		{
			return P_0.B == null;
		}

		internal uint B(q P_0)
		{
			return P_0.A;
		}
	}

	[CompilerGenerated]
	private sealed class C
	{
		public uint A;

		internal bool A(N P_0)
		{
			return P_0.A == this.A;
		}
	}

	[CompilerGenerated]
	private sealed class c
	{
		public uint A;

		public m A;

		internal bool A(uint P_0)
		{
			return P_0 >= this.A;
		}

		internal bool A(q P_0)
		{
			if (P_0.B == null)
			{
				return this.A.m_A[P_0.a].A == 0;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class D
	{
		public long A;

		internal bool A(global::b.c P_0)
		{
			return P_0.A == this.A;
		}
	}

	[CompilerGenerated]
	private sealed class d
	{
		public m A;

		public uint A;

		public double A;

		public Func<global::A.i, bool> A;

		internal bool A(global::A.k P_0)
		{
			return this.A.m_A.Contains(P_0.A);
		}

		internal bool A(uint P_0)
		{
			return P_0 >= this.A;
		}

		internal bool A(global::A.i P_0)
		{
			if (P_0.A != null && !P_0.a && this.A - P_0.A >= 600.0)
			{
				return !this.A.m_A.A(P_0.A);
			}
			return false;
		}

		internal bool A(long P_0)
		{
			e e2 = new e
			{
				A = P_0
			};
			return !this.A.m_A.Values.Any(e2.A);
		}
	}

	[CompilerGenerated]
	private sealed class E
	{
		public global::A.k A;

		internal bool A(global::A.k P_0)
		{
			if (P_0.A == this.A.A && P_0.a == this.A.a)
			{
				return Math.Abs(P_0.A - this.A.A) + Math.Abs(P_0.B - this.A.B) < 20f;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class e
	{
		public long A;

		internal bool A(A P_0)
		{
			return P_0.A == this.A;
		}
	}

	[CompilerGenerated]
	private sealed class F
	{
		public long A;

		internal bool A(N P_0)
		{
			if (P_0.a == this.A)
			{
				return P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA();
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class f
	{
		public m A;

		public long A;

		public long a;

		public long B;

		internal void A(object P_0)
		{
			long num = 0L;
			long num2 = 0L;
			Stopwatch stopwatch = Stopwatch.StartNew();
			string text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
					}).Select(m.b.A.A).ToArray();
					if (array.Length != 0)
					{
						int num3 = this.A.m_A % array.Length;
						list.AddRange(array.Skip(num3).Concat(array.Take(num3)).Take(4));
						this.A.m_A = (num3 + 4) % array.Length;
					}
				}
				num = this.A.g();
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
						global::a.y y2 = this.A.b(item);
						if ((double)y2.A < (double)y2.C * 0.5 && (double)y2.a > (double)y2.c * 0.45)
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
							val = this.A.F(this.A.F(this.A.m_F + 112837064) + 136) & -16777216;
						}
						catch
						{
						}
						this.A.m_A.B(Math.Max(65536L, val));
					}
					num4 = this.A.m_A.a;
				}
				byte[] array2 = new byte[65536];
				long num5 = this.A.m_F + 65511760;
				while (num == 0L && stopwatch.ElapsedMilliseconds < 80 && num2 < 33554432)
				{
					lock (this.A.m_A)
					{
						if (this.A.m_C || this.A.m_d != a)
						{
							return;
						}
					}
					if (m.A(this.A.m_A, new IntPtr(num4), out a a2, (UIntPtr)(ulong)Marshal.SizeOf(typeof(a))) == UIntPtr.Zero)
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
							global::B.G.a(this.A.m_A, new IntPtr(num8), array2, (UIntPtr)(ulong)num9, out var uIntPtr);
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
										long key = this.A.m_A.OrderBy(m.b.A.a).First().Key;
										this.A.m_A.Remove(key);
									}
									this.A.m_A[num12] = DateTime.UtcNow;
								}
								try
								{
									global::a.y y3 = this.A.b(num12);
									if ((double)y3.A < (double)y3.C * 0.5 && (double)y3.a > (double)y3.c * 0.45)
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
							this.A.B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
						}
						else
						{
							this.A.m_a = DateTime.UtcNow.AddMilliseconds(flag ? 2000 : 100);
							this.A.B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xD();
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
						this.A.a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xd() + num.ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xv()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xE() + this.A.m_A.a.ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xv()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xe() + num2 / 1048576 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xF() + stopwatch.ElapsedMilliseconds + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + text;
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

	private long m_A;

	private ulong[] m_A = new ulong[0];

	private readonly Dictionary<uint, A> m_A = new Dictionary<uint, A>();

	private readonly global::A.F m_A = new global::A.F();

	private bool m_A;

	private readonly HashSet<string> m_A = new HashSet<string>();

	private global::a.R m_A;

	private DateTime m_A;

	private ulong? m_A;

	private readonly Dictionary<long, global::a.S> m_A = new Dictionary<long, global::a.S>();

	private bool m_a;

	private long m_a;

	private long m_B;

	private readonly global::B.c m_A = new global::B.c();

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

	public string a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

	public string B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jb();

	private long m_E;

	private long m_e = -1L;

	private bool m_c;

	private readonly Queue<long> m_A = new Queue<long>();

	private readonly HashSet<long> m_A = new HashSet<long>();

	private DateTime m_B;

	public bool D;

	public string b = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

	private readonly Process m_A;

	private readonly o m_A;

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

	private readonly T m_A = new T();

	private long m_G;

	private uint m_B;

	private long m_g;

	private DateTime m_C;

	public string C = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JC();

	[CompilerGenerated]
	private string m_c;

	internal global::a.R A
	{
		get
		{
			try
			{
				if (this.m_A == null || this.m_A.HasExited || this.m_A.StartTime != this.m_b || this.m_A.A != this.b)
				{
					return null;
				}
				B(this.b);
				if (C() != this.m_A.A)
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

	private string a => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iz());

	private string B => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iA());

	public string b
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

	private long C()
	{
		e();
		long num = F(this.m_B + 680);
		if (F(num) != this.m_F + 64683768 || A(num + 24, 1)[0] != 0)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ia());
		}
		long num2 = F(num + 8);
		if ((int)num2 == 0 || (uint)(num2 >>> 32) >= 16)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iB());
		}
		return num2;
	}

	private long A(long P_0, long P_1, uint P_2, long P_3)
	{
		if (P_2 == 0 || P_2 > 16)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ib());
		}
		for (uint num = 0u; num < P_2; num++)
		{
			uint num2 = E(P_1 + num * 4);
			if (num2 != uint.MaxValue)
			{
				long num3 = A(P_0, num2);
				if (F(num3 + 8) == P_3)
				{
					return num3;
				}
			}
		}
		throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iC());
	}

	internal IEnumerable<string> c()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<N> list = i();
		yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xf() + list.Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fj() + stopwatch.ElapsedMilliseconds + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xG() + this.b;
		foreach (N item in list)
		{
			yield return item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Is() + item.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xg() + item.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IV() + item.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xH() + item.B + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iv() + item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Iw() + item.b + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xh() + item.C + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xI() + (item.b ? item.A.ToArgb().ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xJ()) : (_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xi() + this.b)) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xj() + global::b.j.A(item) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.IU() + global::A.T.A(item) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xK() + p.A(item) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ix() + item.A.Count;
			foreach (global::b.c item2 in item.A)
			{
				item2.A = item.b;
				item2.A = item.A;
			}
		}
		if (list.Count((N P_0) => P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()) != 1)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xk());
		}
		if (list.Any((N P_0) => !P_0.b))
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xL() + this.b);
		}
		List<Z> list2 = A(this.b, list.SelectMany((N P_0) => P_0.A));
		yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xl() + list2.Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xM() + list.Sum((N P_0) => P_0.A.Count) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xm() + list2.Count((Z P_0) => P_0.A != null) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xN() + list2.Count((Z P_0) => P_0.A != null);
		foreach (Z item3 in list2.Take(16))
		{
			yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xn() + item3.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + item3.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xO() + item3.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BC() + item3.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BC() + item3.B + ((item3.A == null) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D() : (_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xo() + item3.A.c + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xP() + item3.A.c));
		}
		e();
		yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xp() + J();
		using (m m2 = new m(I()))
		{
			List<Z> list3 = m2.A(this.b, list.SelectMany((N P_0) => P_0.A));
			m2.e();
			yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xQ() + list3.Count;
		}
		global::a.y y2 = null;
		stopwatch.Restart();
		while (stopwatch.ElapsedMilliseconds < 12000 && !A(out y2))
		{
			Thread.Sleep(40);
		}
		if (y2 == null)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xq() + this.B + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + this.a);
		}
		yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xR() + stopwatch.ElapsedMilliseconds + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xr() + y2.D + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xv() + y2.d + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xS() + y2.B + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xv() + y2.b + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xs() + y2.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BC() + y2.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xT() + y2.E;
		bool flag = false;
		long num = H(this.m_c);
		if (num == 0L)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xt());
		}
		global::a.y y3 = A(num, out flag);
		yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xU() + y3.D + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xv() + y3.d + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xu() + flag;
		S[] array = list.Select((N P_0) => new S
		{
			A = P_0.A,
			A = P_0.A,
			A = P_0.A,
			A = P_0.a
		}).ToArray();
		yield return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xV() + A(list).Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xv() + A(this.b, array, false).Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xW() + A(this.b, array).Count;
	}

	public void D()
	{
		this.m_A.Clear();
		this.m_A.A();
		this.m_A = new ulong[0];
		this.m_A.Clear();
		this.m_A = true;
	}

	public List<global::A.k> A(string P_0, IEnumerable<S> P_1)
	{
		B(P_0);
		long num = this.m_G;
		long num2 = this.A(num, this.m_B);
		long num3 = this.A(num, E(num2 + 260));
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
		Dictionary<long, S> dictionary = (from s2 in P_1
			group s2 by s2.A).ToDictionary((IGrouping<long, S> grouping) => grouping.Key, (IGrouping<long, S> source) => source.First());
		List<global::A.k> list = new List<global::A.k>();
		foreach (S value6 in dictionary.Values)
		{
			long num4 = value6.A + 1016;
			uint num5 = E(num4 + 44);
			uint num6 = E(num4 + 40);
			if (num5 > 256)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ic());
			}
			if (num5 == 0)
			{
				continue;
			}
			byte[] value = this.A(this.A(num, num6), (int)(num5 * 4));
			long num7 = F(value6.A + 8);
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
					if (F(num10) != this.m_F + 64568064)
					{
						continue;
					}
					byte[] array = this.A(num10, 148);
					string text = G(BitConverter.ToInt64(array, 56));
					global::A.G g2 = global::A.f.A(text);
					if (g2 != null && BitConverter.ToUInt32(array, 12) == 1)
					{
						uint num11 = BitConverter.ToUInt32(array, 8);
						byte[] array2 = this.A(this.A(num, num11), 20);
						if (global::A.H.A(array, array2, this.m_F, num7, out var num12, out var num13, out var num14, out var num15, out var num16, g2.A) && this.A(num10, 148).SequenceEqual(array) && this.A(this.A(num, num11), 20).SequenceEqual(array2))
						{
							list.Add(new global::A.k
							{
								A = value6.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num10 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + BitConverter.ToUInt32(array, 68) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + text + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + Convert.ToBase64String(array2.Skip(4).Take(12).ToArray()),
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
			if (E(num4 + 44) != num5 || E(num4 + 40) != num6)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iD());
			}
		}
		if (this.m_A)
		{
			foreach (global::A.k item in list)
			{
				this.m_A.Add(item.A);
			}
			this.m_A = false;
		}
		list.RemoveAll((global::A.k k5) => this.m_A.Contains(k5.A));
		bool flag = this.m_A.Length == 0;
		uint A2 = E(num3 + 24);
		uint num17 = E(num3 + 8);
		uint num18 = E(num3 + 20);
		if (A2 == 0 || A2 > 65536)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.id());
		}
		byte[] value2 = a(this.A(num, num17), (int)(A2 * 8));
		byte[] value3 = a(this.A(num, num18), (int)(A2 * 4));
		if (this.m_A.Length < A2)
		{
			Array.Resize(ref this.m_A, (int)A2);
		}
		List<global::A.k> list2 = new List<global::A.k>();
		List<global::A.i> list3 = new List<global::A.i>();
		List<global::A.k> list4 = new List<global::A.k>();
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
					if (E(num24 + 104) != num22 || E(num24 + 108) != num20)
					{
						continue;
					}
					long num25 = F(num24 + 56);
					string text2 = G(num25);
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
					goto IL_069b;
				}
				catch
				{
				}
				continue;
			}
			goto IL_069b;
			IL_069b:
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
						global::A.k k2 = global::A.H.A(array4, array5, array6, this.m_F, num, value4.A, num22, num20);
						if (k2 != null && dictionary.TryGetValue(k2.A, out var value5) && E(value4.A + 104) == num22 && E(value4.A + 388) == num26 && this.A(this.A(num, num26) + 16, 16).SequenceEqual(array5.Skip(16).Take(16)))
						{
							k2.A = value5.A;
							k2.A = value5.A;
							list4.Add(k2);
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
				if (!dictionary.ContainsKey(key2) || E(value4.A + 88) != num28 || E(value4.A + 104) != num22)
				{
					continue;
				}
				if (global::a.r.A(value4.A))
				{
					global::A.k k3 = this.A(value4.A, num22, array4, value4.A, num);
					if (k3 != null)
					{
						k3.A = key2;
						k3.A = dictionary[key2].A;
						k3.A = dictionary[key2].A;
						list2.Add(k3);
					}
					continue;
				}
				if (value4.A == null && value4.A && A - value4.A < 2500.0 && (value4.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aU() || value4.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.au() || value4.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aV()))
				{
					try
					{
						uint num29 = BitConverter.ToUInt32(array4, 388);
						if (num29 != uint.MaxValue)
						{
							byte[] array7 = this.A(this.A(num, num29), 284);
							global::A.k k4 = global::A.H.A(value4.A, array4, array7, this.m_F, num, value4.A, num22, num20);
							if (k4 != null && E(value4.A + 388) == num29 && E(value4.A + 104) == num22 && this.A(this.A(num, num29) + 16, 16).SequenceEqual(array7.Skip(16).Take(16)))
							{
								value4.A = k4;
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
				if (value4.A != null && global::A.H.A(value4.A, null, num30, num31))
				{
					value4.a = true;
				}
				list3.Add(new global::A.i
				{
					A = value4.A + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num22,
					a = value4.A,
					A = key2,
					A = value4.A,
					A = value4.A,
					a = value4.a,
					A = global::b.a.A(num30, 0f, num31),
					A = num30,
					a = num31
				});
			}
			catch
			{
			}
		}
		if (E(num3 + 24) != A2 || E(num3 + 8) != num17 || E(num3 + 20) != num18)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iE());
		}
		B(P_0);
		List<global::A.k> list5 = this.m_A.A(list, list3, A);
		foreach (global::A.i item2 in list3.Where((global::A.i i2) => i2.A != null && !i2.a && A - i2.A >= 600.0 && !this.m_A.A(i2.A)))
		{
			global::A.k A3 = item2.A;
			if (!list5.Any((global::A.k k5) => k5.A == A3.A && k5.a == A3.a && Math.Abs(k5.A - A3.A) + Math.Abs(k5.B - A3.B) < 20f))
			{
				list5.Add(A3);
			}
		}
		list5.AddRange(list4);
		list5.AddRange(list2);
		long[] array8 = this.m_A.Keys.Where(delegate(long num32)
		{
			e e2 = new e();
			e2.A = num32;
			return !this.m_A.Values.Any(e2.A);
		}).ToArray();
		foreach (long key3 in array8)
		{
			this.m_A.Remove(key3);
		}
		return list5;
	}

	private long A(long P_0, long P_1, uint P_2, uint P_3, bool P_4)
	{
		if (P_2 == 0 || P_1 == 0L || P_3 >= E(P_1 + 24) || P_3 > 1000000)
		{
			return 0L;
		}
		long num = A(P_0, E(P_1 + 20));
		long num2 = A(P_0, E(P_1 + 8));
		if (E(num + P_3 * 4) != P_2)
		{
			return 0L;
		}
		uint num3 = E(num2 + P_3 * 8);
		if (num3 == uint.MaxValue)
		{
			return 0L;
		}
		long num4 = A(P_0, num3);
		if (E(num + P_3 * 4) != P_2 || E(num2 + P_3 * 8) != num3)
		{
			return 0L;
		}
		if (!P_4 && (E(num4 + 104) != P_2 || E(num4 + 108) != P_3 || (E(num4 + 160) & 0x4000) != 0))
		{
			return 0L;
		}
		return num4;
	}

	private bool A(long P_0, byte[] P_1, int P_2, out float P_3, out float P_4, out float P_5)
	{
		if (global::A.S.A(P_1, P_2, out P_3, out P_4, out P_5))
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
		long num4 = A(P_0, E(this.m_F + 98614988));
		uint num5 = E(num4 + ((b2 == 3) ? 260 : 256));
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
		return global::A.S.A(P_3, P_4, P_5);
	}

	private void A(long P_0, long P_1, string P_2, N P_3, bool P_4 = false)
	{
		if ((P_3.a != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz() && !P_4) || !global::A.S.A(P_2))
		{
			return;
		}
		long num = A(P_1, 103319626L);
		if (num == 0L || e(num + 108) <= 0f)
		{
			return;
		}
		uint num2 = E(P_1 + 104);
		uint num3 = E(P_1 + 108);
		uint num4 = E(P_1 + 88);
		long num5 = A(P_1, 103319637L);
		if (num5 == 0L)
		{
			return;
		}
		uint num6 = E(num5 + 76);
		if (num6 == uint.MaxValue)
		{
			return;
		}
		long num7 = A(P_0, num6);
		uint num8 = E(num7 + 44);
		uint num9 = E(num7 + 40);
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
			List<global::A.s> list = new List<global::A.s>();
			for (int num10 = 0; num10 < num8; num10++)
			{
				uint num11 = BitConverter.ToUInt32(value, num10 * 4);
				if (num11 == uint.MaxValue)
				{
					continue;
				}
				long num12 = A(P_0, num11);
				if (F(num12) != this.m_F + 64568064)
				{
					continue;
				}
				byte[] array = A(num12, 164);
				string text = G(BitConverter.ToInt64(array, 56));
				if (!global::A.R.A(P_2, text) || !global::A.S.A(text, array))
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
				List<global::A.s> list2 = new List<global::A.s>();
				for (int num16 = 0; num16 < num14; num16++)
				{
					if (A(P_0, array2, num16, out var num17, out var num18, out var num19))
					{
						string text2 = P_1 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num2 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num12 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + text;
						list2.Add(new global::A.s
						{
							A = text2 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num16,
							a = text2,
							B = global::b.j.A(global::A.R.a(P_2)),
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
				if (BitConverter.ToInt64(array3, 0) != this.m_F + 64568064 || BitConverter.ToInt64(array3, 56) != BitConverter.ToInt64(array, 56) || BitConverter.ToUInt32(array3, 8) != num13 || BitConverter.ToUInt32(array3, 12) != num14 || !global::A.S.A(text, array3))
				{
					throw new InvalidOperationException();
				}
				if (!A(num15, array2.Length).SequenceEqual(array2))
				{
					throw new InvalidOperationException();
				}
				list.AddRange(list2);
			}
			if (E(P_1 + 104) != num2 || E(P_1 + 108) != num3 || E(P_1 + 88) != num4 || E(num5 + 76) != num6 || E(num7 + 44) != num8 || E(num7 + 40) != num9)
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

	private void A(Z P_0, global::b.c P_1)
	{
		bool flag = P_1.A.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bV()) && (P_1.A.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ie()) || P_1.A.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iF()));
		bool flag2 = P_1.A.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.@if()) || flag;
		bool flag3 = P_1.A.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iG());
		if (!P_0.A && !flag2 && !flag3)
		{
			return;
		}
		foreach (global::b.B item in P_1.A)
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
				if (num2 == 0L || e(num2 + 108) <= 0f)
				{
					continue;
				}
				long num3 = 0L;
				long num4 = A(num, 103319697L);
				if (num4 != 0L)
				{
					uint num5 = E(num4 + 16);
					if (num5 != uint.MaxValue)
					{
						num3 = A(this.m_G, num5);
					}
				}
				if (num3 == 0L)
				{
					long num6 = A(num, 103319632L);
					if (num6 != 0L && E(num6 + 20) != 0 && E(num6 + 20) < 32)
					{
						long num7 = A(this.m_G, E(A(this.m_G, E(num6 + 16))));
						uint num8 = E(num7 + 240);
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
				string text = G(F(num3 + 184));
				bool flag4 = P_1.A.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ie()) || P_1.A.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iF());
				bool flag5 = text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ig()) || text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iH());
				bool flag6 = text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ih()) || (flag4 && text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iI()));
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
					if (num9 == 0L || E(num9 + 40) == 0)
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
				float num13 = e(num3 + 404);
				if (num13 <= 0f || num13 > 500f)
				{
					continue;
				}
				global::A.w w2 = new global::A.w
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
					a = global::A.X.A(P_1.A, text),
					C = (flag || !flag2 || (A(num3 + 438, 1)[0] & 4) != 0)
				};
				if (flag5 && (flag2 || flag3))
				{
					try
					{
						long num14 = F(num3 + 312);
						byte[] value2 = A(num14 + 280, 28);
						global::A.X.A(w2, BitConverter.ToSingle(value2, 0), BitConverter.ToSingle(value2, 4), BitConverter.ToSingle(value2, 24));
					}
					catch
					{
					}
				}
				P_0.A = w2;
				break;
				end_IL_0088:;
			}
			catch
			{
			}
		}
	}

	private void A(long P_0, long P_1, string P_2, N P_3)
	{
		if (!(P_3.a != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()) && global::a.h.A(P_2) != null)
		{
			long num = A(P_1, 103319626L);
			if (num != 0L && !(e(num + 108) <= 0f))
			{
				global::b.c c2 = new global::b.c
				{
					A = P_1,
					a = F(P_1 + 104),
					A = P_2,
					A = P_3.A
				};
				c2.A.Add(new global::b.B
				{
					A = P_1,
					a = P_3.A,
					A = E(P_1 + 96),
					a = E(P_1 + 124)
				});
				P_3.A.Add(c2);
			}
		}
	}

	private void A(List<N> P_0, long P_1)
	{
		N[] array = P_0.Where((N n2) => n2.a == P_1 && n2.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()).ToArray();
		if (array.Length != 1)
		{
			d();
			return;
		}
		if (DateTime.UtcNow >= this.m_A)
		{
			this.m_A = global::a.q.A(global::a.q.A, this.m_b);
			this.m_A = DateTime.UtcNow.AddSeconds(this.m_A.HasValue ? 10 : 2);
		}
		array[0].A = true;
		array[0].A = this.m_A;
		this.m_A = new global::a.R
		{
			A = this.m_A.Id,
			A = this.m_b,
			A = this.b,
			A = array[0].A,
			A = P_1,
			A = this.m_A
		};
	}

	private void d()
	{
		this.m_A = null;
		this.m_A = null;
		this.m_A = DateTime.MinValue;
	}

	private global::A.k A(long P_0, uint P_1, byte[] P_2, string P_3, long P_4)
	{
		if (!global::a.r.A(P_3))
		{
			return null;
		}
		if (!this.m_A.TryGetValue(P_0, out var value) || value.A != P_1)
		{
			Dictionary<long, global::a.S> dictionary = this.m_A;
			long key = P_0;
			global::a.S obj = new global::a.S
			{
				A = P_1
			};
			global::a.S s2 = obj;
			dictionary[key] = obj;
			value = s2;
		}
		uint num = BitConverter.ToUInt32(P_2, 388);
		if (num == uint.MaxValue)
		{
			return null;
		}
		byte[] array = A(A(P_4, num), 284);
		if (BitConverter.ToInt64(array, 0) != this.m_F + 64559016 || BitConverter.ToUInt32(array, 36) + P_4 != P_0 || BitConverter.ToUInt32(array, 64) != 1)
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
		if (num9 != 0L && E(num9 + 20) != 0 && E(num9 + 20) < 32)
		{
			long num10 = A(P_4, E(A(P_4, E(num9 + 16))));
			uint num11 = E(num10 + 240);
			if (num11 != uint.MaxValue)
			{
				long num12 = A(P_4, num11);
				global::a.s.A(G(F(num12 + 184)), out num7, out num8);
			}
		}
		string text = (P_3.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ii()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ij() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iJ());
		global::a.r.A(value, b2 == 4, num4, num5, num6, num2, num3, BitConverter.ToSingle(array, 272), BitConverter.ToSingle(array, 280), num7, num8, P_0 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + P_1, text);
		return value.A;
	}

	private void E()
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
				long num = global::a.X.A(File.ReadAllText(this.a), this.m_A.Id, this.m_b.ToUniversalTime().Ticks, this.m_F);
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
			File.WriteAllText(this.a, this.m_A.Id + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bq() + this.m_b.ToUniversalTime().Ticks + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bq() + this.m_F + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bq() + P_0);
			this.m_a = P_0;
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	public global::a.v e()
	{
		try
		{
			if (this.m_B == 0L || F(this.m_B) != this.m_F + 64684536)
			{
				this.m_B = 0L;
				long num = F(F(this.m_F + 102260680) + 184);
				long num2 = F(num);
				long num3 = F(num + 8);
				if (num3 < num2 || num3 - num2 > 8192)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iK());
				}
				for (long num4 = num2; num4 < num3; num4 += 8)
				{
					long num5 = F(num4);
					if (num5 < 65576)
					{
						continue;
					}
					try
					{
						if (F(num5 - 40) == this.m_F + 64684536)
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
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ik());
			}
			long num6 = F(this.m_B + 416);
			byte[] value = A(num6, 816);
			global::a.v v2 = new global::a.v
			{
				A = new float[16],
				a = new float[16]
			};
			for (int num7 = 0; num7 < 16; num7++)
			{
				v2.A[num7] = BitConverter.ToSingle(value, 88 + num7 * 4);
				v2.a[num7] = BitConverter.ToSingle(value, 752 + num7 * 4);
				if (float.IsNaN(v2.A[num7]) || float.IsInfinity(v2.A[num7]) || float.IsNaN(v2.a[num7]) || float.IsInfinity(v2.a[num7]))
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iL());
				}
			}
			if ((double)Math.Abs(v2.A[15] - 1f) > 0.01 || v2.a[0] <= 0f || v2.a[5] <= 0f || (double)Math.Abs(v2.a[11] - 1f) > 0.01)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.il());
			}
			for (int num8 = 0; num8 < 3; num8++)
			{
				float num9 = v2.A[12 + num8];
				for (int num10 = 0; num10 < 3; num10++)
				{
					num9 += BitConverter.ToSingle(value, 72 + num10 * 4) * v2.A[num10 * 4 + num8];
				}
				if ((double)Math.Abs(num9) > 0.1)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iM());
				}
			}
			if (F(this.m_B + 416) != num6)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.im());
			}
			return v2;
		}
		catch
		{
			this.m_B = 0L;
			throw;
		}
	}

	[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
	private static extern UIntPtr A(o P_0, IntPtr P_1, out a P_2, UIntPtr P_3);

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
				this.B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iN();
				this.a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
		if (F(P_0 + 112) != num2 || (E(P_0 + 124) & 0x3FF) != num)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.@in());
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
					num4 = F(num4 + (((num3 & 0x40) != 0) ? 48 : (((num3 & 0x20) != 0) ? 56 : 40)));
				}
				else if ((num3 & 0x100) != 0)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iO());
				}
				return num4;
			}
		}
		return 0L;
	}

	private int a(long P_0)
	{
		long num = F(this.m_F + P_0);
		return BitConverter.ToUInt16(A(num + 184, 2), 0);
	}

	private float a(byte[] P_0, int P_1)
	{
		long num = A(P_0, P_1);
		if (num < 65536)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.io());
		}
		float num2 = e(num + 40);
		if (num2 < 8f || num2 > 20000f)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iP());
		}
		return num2;
	}

	private global::a.x B(long P_0)
	{
		switch (F(P_0) - this.m_F)
		{
		case 64432864L:
		{
			if (A(P_0 + 248, 1)[0] != 0)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ip());
			}
			byte[] array3 = A(P_0 + 216, 32);
			byte[] array4 = A(P_0 + 216, 32);
			if (!global::A.t.A(array3, array4) || A(P_0 + 248, 1)[0] != 0)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ip());
			}
			return global::a.x.A(array3);
		}
		case 64431008L:
		{
			byte[] array = A(P_0, 128);
			byte[] array2 = A(P_0, array);
			int num10 = a(111878264L);
			long num11 = A(array2, num10);
			if (num11 == 0L)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iQ());
			}
			return global::a.x.A(A(num11 + 40, 32));
		}
		case 64815280L:
		{
			float num5 = e(P_0 + 208);
			float num6 = e(P_0 + 212);
			float num7 = e(P_0 + 216);
			double num8 = Math.Cos((double)num7 * Math.PI / 180.0);
			double num9 = Math.Sin((double)num7 * Math.PI / 180.0);
			return new global::a.x
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
			float num = e(P_0 + 208);
			float num2 = e(P_0 + 212);
			float num3 = e(P_0 + 216);
			float num4 = e(P_0 + 220);
			return new global::a.x
			{
				A = num3,
				b = num4,
				C = num * (1f - num3),
				c = num2 * (1f - num4)
			};
		}
		case 64430720L:
			return global::a.x.A(e(P_0 + 208), e(P_0 + 212));
		default:
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iq());
		}
	}

	private global::a.y b(long P_0)
	{
		if (P_0 < 65536 || F(P_0) != this.m_F + 65511760)
		{
			throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iR());
		}
		byte[] array = A(P_0, 480);
		byte[] array2 = A(P_0, array);
		float num = a(array2, a(111989640L));
		float num2 = a(array2, a(111989680L));
		long num3 = BitConverter.ToInt64(array, 472);
		if (num3 < 65536 || F(num3 + 136) != P_0)
		{
			throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ir());
		}
		byte[] value = A(num3, 344);
		float num4 = BitConverter.ToSingle(value, 296);
		float num5 = BitConverter.ToSingle(value, 300);
		if (num4 < 10f || num5 < 10f || num4 > 4000f || num5 > 4000f)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iS());
		}
		global::a.x x2 = global::a.x.A;
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
				throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.@is());
			}
			byte[] array3 = A(num6, 344);
			long num10 = BitConverter.ToInt64(array3, 0) - this.m_F;
			if (num10 == 65713904)
			{
				flag = true;
			}
			long num11 = A(A(num6, array3), 40);
			if (flag3 && num11 == 0L)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iT());
			}
			flag3 = false;
			if (num11 != 0L)
			{
				x2 = x2.A(B(num11));
			}
			float num12 = BitConverter.ToSingle(array3, 168);
			float num13 = BitConverter.ToSingle(array3, 172);
			if (!global::b.a.A(num12, 0f, num13))
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.it());
			}
			x2 = x2.A(global::a.x.A(num12, num13));
			long num14 = BitConverter.ToInt64(array3, 136);
			if (F(num6 + 136) != num14)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iU());
			}
			if (num14 == 0L)
			{
				flag2 = num10 == 64823200;
				num8 = BitConverter.ToSingle(array3, 296);
				num9 = BitConverter.ToSingle(array3, 300);
			}
			num6 = num14;
		}
		if (!flag || !flag2)
		{
			throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iu());
		}
		if (num8 < 200f || num9 < 200f || num8 > 20000f || num9 > 20000f)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iV());
		}
		if (F(P_0 + 472) != num3 || F(P_0) != this.m_F + 65511760)
		{
			throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iv());
		}
		global::a.y y2 = global::a.x.A(num, num2, num4, num5, num8, num9, x2);
		if (!y2.A || y2.A < 0f || y2.A > num8 || y2.a < 0f || y2.a > num9)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iW());
		}
		return y2;
	}

	public bool A(out global::a.y P_0)
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
			f();
			return false;
		}
		try
		{
			B(this.b);
			global::a.y y2 = b(num);
			B(this.b);
			lock (this.m_A)
			{
				if (this.m_C || this.m_d != num2)
				{
					return false;
				}
				this.m_a = 0;
			}
			P_0 = y2;
			this.B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
				if (ex is global::B.b || ++this.m_a >= 8)
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
				f();
			}
			return false;
		}
	}

	public void F()
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
			this.B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EX();
		}
		f();
	}

	private void f()
	{
		if (!global::a.N.A)
		{
			return;
		}
		E();
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
				string text = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
						}).Select(m.b.A.A).ToArray();
						if (array.Length != 0)
						{
							int num3 = this.m_A % array.Length;
							list.AddRange(array.Skip(num3).Concat(array.Take(num3)).Take(4));
							this.m_A = (num3 + 4) % array.Length;
						}
					}
					num = g();
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
							global::a.y y2 = b(item);
							if ((double)y2.A < (double)y2.C * 0.5 && (double)y2.a > (double)y2.c * 0.45)
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
								val = F(F(this.m_F + 112837064) + 136) & -16777216;
							}
							catch
							{
							}
							this.m_A.B(Math.Max(65536L, val));
						}
						num4 = this.m_A.a;
					}
					byte[] array2 = new byte[65536];
					long num5 = this.m_F + 65511760;
					while (num == 0L && stopwatch.ElapsedMilliseconds < 80 && num2 < 33554432)
					{
						lock (this.m_A)
						{
							if (this.m_C || this.m_d != a2)
							{
								return;
							}
						}
						if (m.A(this.m_A, new IntPtr(num4), out var a3, (UIntPtr)(ulong)Marshal.SizeOf(typeof(a))) == UIntPtr.Zero)
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
								global::B.G.a(this.m_A, new IntPtr(num8), array2, (UIntPtr)(ulong)num9, out var uIntPtr);
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
													long key = this.m_A.OrderBy(m.b.A.a).First().Key;
													this.m_A.Remove(key);
												}
												this.m_A[num12] = DateTime.UtcNow;
											}
											try
											{
												global::a.y y3 = b(num12);
												if ((double)y3.A < (double)y3.C * 0.5 && (double)y3.a > (double)y3.c * 0.45)
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
								this.B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
							}
							else
							{
								this.m_a = DateTime.UtcNow.AddMilliseconds(flag ? 2000 : 100);
								this.B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xD();
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
							this.a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xd() + num.ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xv()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xE() + this.m_A.a.ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xv()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xe() + num2 / 1048576 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xF() + stopwatch.ElapsedMilliseconds + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + text;
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

	private void G()
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
		HashSet<long> hashSet = new HashSet<long>(g(P_0));
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
					if (F(num2 + 136) == P_0)
					{
						long num3 = F(num2) - this.m_F;
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
				long num2 = F(num + 136);
				if (num2 == 0L)
				{
					if (F(num) == this.m_F + 64823200)
					{
						this.m_E = num;
						File.WriteAllText(this.B, this.m_A.Id + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bq() + this.m_b.ToUniversalTime().Ticks + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bq() + this.m_F + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bq() + num);
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

	private long g()
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
				H();
			}
		}
		if (!this.m_c)
		{
			this.m_c = true;
			try
			{
				this.m_E = global::a.X.A(File.ReadAllText(this.B), this.m_A.Id, this.m_b.ToUniversalTime().Ticks, this.m_F);
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
			long num = F(F(this.m_F + 112837064) + 136);
			if (num > 65536 && F(num) == this.m_F + 64823200 && num != this.m_E)
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
			if (this.m_E == 0L || F(this.m_E) != this.m_F + 64823200)
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
					if (F(num3) == this.m_F + 65511760)
					{
						lock (this.m_A)
						{
							this.m_A[num3] = DateTime.UtcNow;
						}
						try
						{
							global::a.y y2 = b(num3);
							if ((double)y2.A < (double)y2.C * 0.5 && (double)y2.a > (double)y2.c * 0.45)
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

	private void H()
	{
		this.m_A.Clear();
		this.m_A.Clear();
		this.m_B = DateTime.MinValue;
	}

	private void a(Z P_0, global::b.c P_1)
	{
		if (!P_0.A && !this.D)
		{
			return;
		}
		foreach (global::b.B item in P_1.A)
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
				if (num2 == 0L || e(num2 + 108) <= 0f)
				{
					continue;
				}
				uint num3 = E(num + 388);
				if (num3 == uint.MaxValue)
				{
					continue;
				}
				long num4 = A(this.m_G, num3);
				long num5 = F(num4);
				if (F(num5 + 344) != this.m_F + 13437424)
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
					global::B.d d2 = global::B.F.a(array, array2);
					if (d2 != null && E(num + 96) == item.A && E(num + 388) == num3 && E(num4 + 172) == num6 && E(num4 + 8) == 0 && E(num7 + 144) == BitConverter.ToUInt32(array2, 144) && A(num4 + 76, 16).SequenceEqual(array.Skip(76).Take(16)))
					{
						P_0.A = d2;
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
			long num = F(P_0);
			if (num < this.m_F || num > this.m_F + 268435456)
			{
				return false;
			}
			long num2 = F(num - 8);
			if (num2 < this.m_F || num2 > this.m_F + 268435456 || E(num2) != 1)
			{
				return false;
			}
			uint num3 = E(num2 + 12);
			if (num3 > 268435456)
			{
				return false;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iw());
			return global::A.t.A(A(this.m_F + num3 + 16, bytes.Length), bytes);
		}
		catch
		{
			return false;
		}
	}

	private long d(long P_0)
	{
		long num = F(P_0 + 296);
		long num2 = F(num);
		long num3 = F(num + 8);
		if (num2 < 65536 || num3 <= num2 || num3 - num2 > 32768 || (num3 - num2) % 8 != 0L)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iX());
		}
		long num4 = 0L;
		for (long num5 = num2; num5 < num3; num5 += 8)
		{
			long num6 = F(num5);
			if (num6 >= 65536 && D(num6))
			{
				if (num4 != 0L && num4 != num6)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ix());
				}
				num4 = num6;
			}
		}
		if (num4 == 0L)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iY());
		}
		if (F(num) != num2 || F(num + 8) != num3)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iy());
		}
		return num4;
	}

	private long h()
	{
		long num = F(this.m_F + 102261280);
		ulong num2 = 14695981039346656037uL;
		for (int num3 = 0; num3 < 4; num3++)
		{
			num2 ^= (byte)(3276800 >>> num3 * 8);
			num2 *= 1099511628211L;
		}
		long num4 = F(num + 664);
		if (num4 < 0 || num4 > 1048576)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iZ());
		}
		long num5 = F(num + 640) + (long)((num2 & (ulong)num4) * 16);
		long num6 = F(num + 624);
		long num7 = F(num5 + 8);
		long num8 = F(num5);
		int num9 = 0;
		while (num7 != num6 && num9++ < 128)
		{
			if (E(num7 + 16) == 3276800)
			{
				long num10 = F(num7 + 24);
				if (F(num7 + 32) - num10 != 8)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.iz());
				}
				long num11 = F(num10);
				if (E(num11 + 72) != 3276800)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JA());
				}
				return d(num11);
			}
			if (num7 == num8)
			{
				break;
			}
			num7 = F(num7 + 8);
		}
		throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ja());
	}

	private void A(List<N> P_0, long P_1, long P_2)
	{
		try
		{
			if (this.m_B == 0L || F(this.m_B) != this.m_F + 64684536)
			{
				e();
			}
			long num = F(this.m_B + 680);
			long num2 = h();
			long num3 = A(P_1, E(P_2 + 284));
			byte[] array = A(num + 24, 2);
			long num4 = F(num + ((array[0] == 0) ? 8 : 16));
			uint num5 = (uint)((ulong)num4 >> 32);
			foreach (N item in P_0)
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
						uint num8 = E(num3 + item.A * 4);
						int num9 = 0;
						if (num8 != 10000 && num8 != uint.MaxValue)
						{
							for (uint num10 = 0u; num10 < item.A; num10++)
							{
								if (E(num3 + num10 * 4) == num8)
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
				uint num11 = E(num2 + num7 + 8);
				item.A = L.A(num11);
				item.b = true;
			}
			if (!A(num + 24, 2).A(array))
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JB());
			}
			this.b = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
		}
		catch (Exception ex)
		{
			foreach (N item2 in P_0)
			{
				item2.b = false;
			}
			this.b = ex.Message;
		}
	}

	public global::a.U I()
	{
		return new global::a.U
		{
			A = this.m_A.Id,
			A = this.m_F,
			A = this.m_b,
			A = this.b,
			a = this.m_G,
			A = this.m_B
		};
	}

	internal m(global::a.U P_0)
	{
		this.m_A = Process.GetProcessById(P_0.A);
		try
		{
			this.m_b = this.m_A.StartTime;
			this.m_F = this.m_A.MainModule.BaseAddress.ToInt64();
			if (this.m_b != P_0.A || this.m_F != P_0.A)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jc());
			}
			if (!global::a.N.A(this.m_A.Id, this.m_b))
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JD());
			}
			this.m_A = global::B.G.A(1040u, false, this.m_A.Id);
			if (this.m_A.IsInvalid)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jd());
			}
			this.b = P_0.A;
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

	public m()
	{
		Process[] processesByName = Process.GetProcessesByName(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fZ());
		if (processesByName.Length != 1)
		{
			Process[] array = processesByName;
			for (int num = 0; num < array.Length; num++)
			{
				array[num].Dispose();
			}
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JE());
		}
		this.m_A = processesByName[0];
		try
		{
			this.m_b = this.m_A.StartTime;
			this.m_F = this.m_A.MainModule.BaseAddress.ToInt64();
			global::a.F.a(this.m_A.MainModule.FileName);
			if (!global::a.N.A(this.m_A.Id, this.m_b))
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JD());
			}
			this.m_A = global::B.G.A(1040u, false, this.m_A.Id);
			if (this.m_A.IsInvalid)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Je());
			}
			long num2 = this.m_F + 112918560 + 840;
			long num3 = F(num2 + 32);
			long num4 = F(num2 + 40);
			if (num4 < num3 || num4 - num3 > 6144 || (num4 - num3) % 48 != 0L)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JF());
			}
			for (long num5 = num3; num5 < num4; num5 += 48)
			{
				string key = f(num5 + 8);
				uint num6 = E(num5 + 40);
				if (num6 < 10)
				{
					this.m_A[key] = (int)num6;
				}
			}
			long num7 = this.m_F + 112918560 + 560;
			long num8 = F(num7 + 32);
			long num9 = F(num7 + 40);
			if (num9 < num8 || num9 - num8 > 12288 || (num9 - num8) % 48 != 0L)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jf());
			}
			for (long num10 = num8; num10 < num9; num10 += 48)
			{
				string text = f(num10 + 8);
				if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JG())
				{
					this.m_A = E(num10 + 40);
				}
				else if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jg())
				{
					this.m_a = E(num10 + 40);
				}
			}
			string[] array2 = new string[4]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FL(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FM(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fl(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JH()
			};
			foreach (string text2 in array2)
			{
				if (!this.m_A.ContainsKey(text2))
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jh() + text2);
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
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JI());
		}
		byte[] array = new byte[P_1];
		if (!global::B.G.a(this.m_A, new IntPtr(P_0), array, (UIntPtr)(ulong)P_1, out var uIntPtr) || uIntPtr.ToUInt64() != (ulong)P_1)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ji());
		}
		return array;
	}

	private uint E(long P_0)
	{
		return BitConverter.ToUInt32(A(P_0, 4), 0);
	}

	private float e(long P_0)
	{
		float result = BitConverter.ToSingle(A(P_0, 4), 0);
		if (float.IsNaN(result) || float.IsInfinity(result))
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JJ());
		}
		return result;
	}

	private long F(long P_0)
	{
		return BitConverter.ToInt64(A(P_0, 8), 0);
	}

	private long A(long P_0, uint P_1)
	{
		if (P_1 == uint.MaxValue)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jj());
		}
		return checked(P_0 + P_1);
	}

	private string f(long P_0)
	{
		long num = F(P_0 + 16);
		long num2 = F(P_0 + 24);
		if (num < 0 || num > 1024 || num2 < num)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JK());
		}
		if (num == 0L)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
		}
		return Encoding.UTF8.GetString(A((num2 < 16) ? P_0 : F(P_0), (int)num));
	}

	private string A(long P_0, long P_1, uint P_2)
	{
		long num = P_1 + 1640;
		if (E(num + 4) != uint.MaxValue)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jk() + P_2;
		}
		uint num2 = E(num + 12);
		if (num2 == 0 || num2 > 256)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jk() + P_2;
		}
		string text = Encoding.Unicode.GetString(A(A(P_0, E(num + 8)), checked((int)num2 * 2)));
		char[] trimChars = new char[1];
		return text.TrimEnd(trimChars);
	}

	private float A(byte[] P_0, string P_1)
	{
		float result = BitConverter.ToSingle(P_0, this.m_A[P_1] * 4);
		if (float.IsNaN(result) || float.IsInfinity(result))
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JL());
		}
		return result;
	}

	private long A(long P_0, long P_1)
	{
		int num = BitConverter.ToUInt16(A(P_0 + 76, 2), 0);
		int num2 = A(this.m_F + P_1 + num * 128, 1)[0];
		if (num2 == 255)
		{
			return 0L;
		}
		long num3 = F(this.m_F + 102260448) + E(P_0 + 64) + num2 * 24;
		if ((A(num3 + 16, 1)[0] & 1) == 0)
		{
			return 0L;
		}
		uint num4 = E(num3);
		long num5 = F(num3 + 8);
		if (num5 == 0L)
		{
			return 0L;
		}
		switch (num4)
		{
		case 8u:
			return num5;
		default:
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jl());
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
			return F(this.m_F + 102260400 + num4 * 16) + (uint)num5;
		}
	}

	private string G(long P_0)
	{
		if (!this.m_A.TryGetValue(P_0, out var value))
		{
			value = f(P_0 + 40);
			if (this.m_A.Count > 10000)
			{
				this.m_A.Clear();
			}
			this.m_A[P_0] = value;
		}
		return value;
	}

	private void A(long P_0, global::A.P P_1)
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
		uint num3 = E(num2 + 76);
		if (num3 == uint.MaxValue)
		{
			return;
		}
		long num4 = A(P_0, num3);
		uint num5 = E(num4 + 4);
		uint num6 = E(num4);
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
		if (E(num4 + 4) != num5 || E(num4) != num6)
		{
			return;
		}
		foreach (KeyValuePair<uint, int> item in dictionary)
		{
			P_1.A[item.Key] = item.Value;
		}
		P_1.a = true;
	}

	private void A(long P_0, long P_1, N P_2)
	{
		long num = A(P_0, E(P_1 + 1792));
		uint num2 = E(num + 40);
		uint num3 = E(num + 36);
		if (num2 > 4096)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JM());
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
			if (A(P_0, E(num6 + 88)) != P_1)
			{
				continue;
			}
			A(P_0, num6, P_2, hashSet);
			string text = G(F(num6 + 56));
			A(P_0, num6, text, P_2);
			A(num6, text, P_2);
			if (global::a.i.A(text))
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
			if (text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jm()))
			{
				string[] array = text.Split('\\');
				if (array.Length >= 3)
				{
					P_2.b = array[2];
				}
				A(P_0, num6, P_2, hashSet2, false);
				int num7 = global::A.T.A(text);
				if (num7 == 0)
				{
					continue;
				}
				long num8 = A(num6, 103319626L);
				long num9 = A(num6, 103319627L);
				if (num8 == 0L || num9 == 0L)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JN());
				}
				if (e(num8 + 108) <= 0f)
				{
					continue;
				}
				P_2.A.Add(new global::b.J
				{
					A = num7,
					A = num6 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + E(num6 + 96),
					A = (E(num9 + 40) == 0)
				});
			}
			if (P_2.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA() && global::A.P.a(text))
			{
				long num10 = A(num6, 103319626L);
				if (num10 != 0L && !(e(num10 + 108) <= 0f))
				{
					global::A.P p2 = new global::A.P
					{
						A = num6 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + E(num6 + 96),
						a = global::A.P.B(text),
						A = num6,
						A = global::A.P.A(text)
					};
					A(P_0, p2);
					P_2.A.Add(p2);
				}
			}
		}
		if (E(num + 40) != num2 || E(num + 36) != num3)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JM());
		}
		P_2.d = true;
		P_2.c = true;
	}

	public List<N> i()
	{
		if (this.m_A.HasExited || this.m_A.StartTime != this.m_b)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jn());
		}
		long num = C();
		uint num2 = (uint)num;
		uint A = (uint)((ulong)num >> 32);
		if (num2 == 0 || A >= 16)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JO());
		}
		long num3 = F(this.m_F + 102260400);
		uint num4 = E(this.m_F + 98614988);
		long num5 = this.A(num3, num4);
		long num6 = this.A(num3, E(num5 + 252));
		uint num7 = E(num6 + 16);
		uint num8 = E(num6 + 12);
		if (num7 == 0 || num7 > 16)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jo());
		}
		long num9 = this.A(num3, num8);
		long num10 = this.A(num3, E(num5 + 284));
		long num11 = this.A(num3, num9, num7, num);
		string text = this.m_A.Id + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + this.m_b.Ticks + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num5 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num11 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num2;
		if (this.b != text)
		{
			this.m_A.Clear();
		}
		uint num12 = E(num10 + A * 4);
		List<N> list = new List<N>();
		for (uint num13 = 0u; num13 < num7; num13++)
		{
			long num14 = this.A(num3, E(num9 + num13 * 4));
			uint num15 = E(num14 + 12);
			if (num15 >= 16)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JP());
			}
			N n2 = new N();
			n2.A = num14;
			n2.a = F(num14 + 8);
			n2.A = num15;
			n2.a = E(num10 + num15 * 4);
			n2.A = this.A(num3, num14, num15);
			n2.a = ((num15 == A) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA() : ((n2.a == 10000 || num12 == 10000) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dk() : ((n2.a == 10002 || num12 == 10002) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz() : ((n2.a == num12) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()))));
			byte[] array = this.A(num14 + 1704, 40);
			n2.A = this.A(array, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FL());
			n2.a = this.A(array, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FM());
			n2.B = this.A(array, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fl());
			n2.b = this.A(array, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JH());
			n2.C = e(num14 + 1256) + e(num14 + 1304);
			if (n2.C < 0f || n2.C > 10000f)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jp());
			}
			uint num16 = E(num14 + 788);
			uint num17 = E(num14 + 784);
			if (num16 > 1024)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JQ());
			}
			if (num16 != 0)
			{
				long num18 = this.A(num3, num17);
				for (uint num19 = 0u; num19 < num16; num19++)
				{
					long num20 = num18 + num19 * 104;
					if (this.A(num20 + 16, 1)[0] == 0)
					{
						n2.A.Add(f(F(num20) + 40));
					}
				}
				if (E(num14 + 788) != num16 || E(num14 + 784) != num17)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JQ());
				}
			}
			string[] array2 = (from P_0 in n2.A
				where P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xB())
				select P_0.Split('\\') into P_0
				where P_0.Length >= 5
				select P_0[3]).Distinct().ToArray();
			n2.B = ((array2.Length == 0) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jq() : string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cd(), array2.Select(delegate(string P_0)
			{
				if (P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xb())
				{
					return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xC();
				}
				return (P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.xc()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fs() : P_0;
			})));
			this.A(num3, num14, n2);
			list.Add(n2);
		}
		if (E(this.m_F + 98614988) != num4 || F(this.m_F + 102260400) != num3 || E(num6 + 16) != num7 || E(num6 + 12) != num8 || C() != num || F(num11 + 8) != num)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JR());
		}
		if (!list.Any((N P_0) => P_0.A == A))
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jr());
		}
		this.A(list, num3, num5);
		a(text);
		this.b = text;
		a(num3, num4);
		this.A(list, num);
		return list;
	}

	public void Dispose()
	{
		d();
		G();
		if (this.m_A != null)
		{
			this.m_A.Dispose();
		}
		if (this.m_A != null)
		{
			this.m_A.Dispose();
		}
	}

	private void A(long P_0, long P_1, N P_2, HashSet<long> P_3, bool P_4 = false)
	{
		try
		{
			long num = A(P_1, 103319626L);
			if (num == 0L || e(num + 108) <= 0f)
			{
				return;
			}
			long num2 = (P_4 ? A(A(P_0, E(P_1 + 124)), 103319618L) : A(P_1, 103319633L));
			if (num2 == 0L)
			{
				return;
			}
			int num3 = (P_4 ? 16 : 52);
			uint num4 = E(num2 + num3);
			if (num4 == uint.MaxValue)
			{
				return;
			}
			long num5 = A(P_0, num4);
			if (!P_3.Add(num5))
			{
				return;
			}
			uint num6 = E(num5 + 52);
			uint num7 = E(num5 + 48);
			if (num6 > 128)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JS());
			}
			if (num6 == 0)
			{
				return;
			}
			long num8 = A(P_0, num7);
			byte[] value = A(num8, 192);
			uint num9 = BitConverter.ToUInt32(value, 184);
			long num10 = A(P_0, num9);
			P p2 = null;
			P p3 = null;
			float? num11 = null;
			if (!P_4 && F(num10) == this.m_F + 64584880)
			{
				long num12 = F(num10 + 16);
				string text = G(num12);
				if (!text.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.br(), StringComparison.Ordinal))
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Js());
				}
				p2 = new P(text, BitConverter.ToSingle(value, 48), BitConverter.ToSingle(value, 52), num5 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + BitConverter.ToUInt32(value, 4));
				if (F(num10 + 16) != num12 || F(num10) != this.m_F + 64584880)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JT());
				}
			}
			long num13 = this.m_F + (P_4 ? 64557040 : 64557304);
			if (F(num10) == num13)
			{
				long num14 = F(num10 + 16);
				string text2 = G(num14);
				if (!P_4 && global::a.f.A(text2))
				{
					float num15 = BitConverter.ToSingle(value, 48);
					float num16 = BitConverter.ToSingle(value, 52);
					global::a.f.A(num15);
					global::a.f.A(num16);
					if (num16 <= 0f || num15 > num16)
					{
						throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jt());
					}
					num11 = num15;
				}
				if (P_4 && global::a.i.a(text2))
				{
					p3 = new P(text2, BitConverter.ToSingle(value, 48), BitConverter.ToSingle(value, 52), P_1 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + E(P_1 + 96));
				}
				if (F(num10 + 16) != num14 || F(num10) != num13)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JU());
				}
			}
			if (E(num2 + num3) != num4 || E(num5 + 52) != num6 || E(num5 + 48) != num7 || E(num8 + 184) != num9 || F(num8) != BitConverter.ToInt64(value, 0))
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JS());
			}
			if (p2 != null)
			{
				P_2.a.Add(p2);
			}
			if (p3 != null)
			{
				P_2.A.Add(p3);
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
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
		}
		long num2 = A(num, 103319637L);
		if (num2 == 0L)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
		}
		uint num3 = E(num2 + 76);
		if (num3 == uint.MaxValue)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
		}
		long num4 = A(P_0, num3);
		uint num5 = E(num4 + 44);
		uint num6 = E(num4 + 40);
		if (num5 == 0 || num5 > 64 || num6 == uint.MaxValue)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
			if (F(num9) != this.m_F + 64568064)
			{
				continue;
			}
			byte[] array = A(num9, 164);
			if (global::A.S.A(array))
			{
				string text = G(BitConverter.ToInt64(array, 56));
				if (text.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ju(), StringComparison.Ordinal) && (text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JV()) || text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jv()) || text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JW())))
				{
					return num9 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + text;
				}
			}
		}
		return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
		string text = global::A.p.b(P_3);
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
					uint num4 = E(num3 + 124);
					value = global::b.j.A((num4 != uint.MaxValue) ? G(F(A(P_0, num4) + 56)) : global::A.R.a(G(F(num3 + 56))));
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
		string text2 = (P_3.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jw()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jb() : (P_3.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bv()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jB() : (P_3.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JX()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ja() : (P_3.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jx()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jA() : (P_3.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JY()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jz() : (P_3.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Jy()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JZ() : null))))));
		if (text2 != null)
		{
			return global::b.j.A(text2);
		}
		return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jC();
	}

	public List<q> A(string P_0, IEnumerable<S> P_1, bool P_2 = false)
	{
		B(P_0);
		long num = this.m_G;
		long num2 = this.A(num, this.m_B);
		long num3 = this.A(num, E(num2 + 260));
		if (this.m_f != num3)
		{
			this.m_f = num3;
			this.m_a = new ulong[0];
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.a();
		}
		uint A = E(num3 + 24);
		uint num4 = E(num3 + 8);
		uint num5 = E(num3 + 20);
		if (A == 0 || A > 65536)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jc());
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
		Dictionary<long, S> dictionary = (from s2 in P_1
			group s2 by s2.A).ToDictionary((IGrouping<long, S> grouping) => grouping.Key, (IGrouping<long, S> source) => source.First());
		List<q> list = new List<q>();
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
					string text = G(num12);
					if (string.IsNullOrEmpty(text))
					{
						continue;
					}
					if (text.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jD(), StringComparison.Ordinal) && text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jd()))
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
				if (num16 == 0L || F(num16) != this.m_F + 65307976)
				{
					continue;
				}
				byte[] array3 = this.A(num16, 208);
				uint num17 = BitConverter.ToUInt32(array3, 56);
				if (num17 == uint.MaxValue || !dictionary.TryGetValue(this.A(num, num17), out var value5))
				{
					continue;
				}
				string text2 = G(BitConverter.ToInt64(array3, 64));
				string text3 = null;
				bool flag = false;
				string text4 = null;
				if (text3 == null && text4 == null && !R.A(text2))
				{
					continue;
				}
				byte[] array4 = this.A(num15, 80);
				if (R.A(array2, array4, array3, this.m_F, num, num13, num9, num7, out var q2, R.a(text2) || global::A.p.B(text2) || text4 != null, text3 != null, flag) && E(num13 + 104) == num9 && E(num13 + 108) == num7 && F(num13 + 56) == value4.a && E(num13 + 388) == num14 && this.A(num15 + 16, 16).SequenceEqual(array4.Skip(16).Take(16)) && (value4.A == 0L || value4.A == q2.A))
				{
					value4.A = q2.A;
					if (flag)
					{
						q2.A |= 9223372036854775808uL;
					}
					q2.B = value5.A;
					q2.A = value5.A;
					q2.a = text2;
					q2.B = text3;
					q2.A = text3 ?? text4 ?? this.A(num, num3, array3, text2);
					list.Add(q2);
				}
			}
			catch
			{
			}
		}
		if (E(num3 + 24) != A || E(num3 + 8) != num4 || E(num3 + 20) != num5 || this.A(num, E(num2 + 260)) != num3)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jE());
		}
		B(P_0);
		double num18 = (double)Stopwatch.GetTimestamp() * 1000.0 / (double)Stopwatch.Frequency;
		HashSet<ulong> hashSet = new HashSet<ulong>(from q3 in list
			where q3.B == null && this.m_A[q3.a].A == 0
			select q3.A);
		foreach (ulong item in hashSet)
		{
			this.m_A.A(item);
		}
		foreach (T.A item2 in this.m_A.A)
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
		foreach (q item3 in from q3 in list
			where q3.B == null
			orderby q3.A
			select q3)
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

	public bool J()
	{
		try
		{
			if (string.IsNullOrEmpty(this.b) || this.m_A.HasExited || this.m_A.StartTime != this.m_b)
			{
				return false;
			}
			B(this.b);
			long num = C();
			uint num2 = (uint)num;
			if (num2 == 0 || (uint)(num >>> 32) >= 16)
			{
				return false;
			}
			long num3 = A(this.m_G, this.m_B);
			long num4 = A(this.m_G, E(num3 + 252));
			long num5 = A(this.m_G, A(this.m_G, E(num4 + 12)), E(num4 + 16), num);
			return this.b == this.m_A.Id + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + this.m_b.Ticks + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num3 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num5 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num2;
		}
		catch
		{
			return false;
		}
	}

	private void A(long P_0, long P_1, long P_2, string P_3, N P_4)
	{
		if ((!(P_4.a != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()) || !(P_4.a != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bA()) || !(P_4.a != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL())) && global::b.a.A(P_3))
		{
			global::b.c c2 = P_4.A.FirstOrDefault((global::b.c c3) => c3.A == P_2);
			if (c2 == null)
			{
				c2 = new global::b.c
				{
					A = P_2,
					a = F(P_2 + 104),
					A = P_3,
					A = P_4.A
				};
				P_4.A.Add(c2);
			}
			c2.A.Add(new global::b.B
			{
				A = P_1,
				a = A(P_0, E(P_1 + 88)),
				A = E(P_1 + 96),
				a = E(P_1 + 124)
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
		if (this.m_A.HasExited || this.b != P_0 || this.m_G == 0L || F(this.m_F + 102260400) != this.m_G || E(this.m_F + 98614988) != this.m_B)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.je());
		}
	}

	public List<Z> A(string P_0, IEnumerable<global::b.c> P_1)
	{
		B(P_0);
		List<Z> list = new List<Z>();
		foreach (global::b.c item in P_1)
		{
			if (!item.A)
			{
				continue;
			}
			try
			{
				if (F(item.A + 104) != item.a)
				{
					continue;
				}
				List<float[]> list2 = new List<float[]>();
				foreach (global::b.B item2 in item.A)
				{
					try
					{
						byte[] value = A(item2.A, 128);
						if (BitConverter.ToUInt32(value, 96) != item2.A || BitConverter.ToUInt32(value, 124) != item2.a || A(this.m_G, BitConverter.ToUInt32(value, 88)) != item2.a)
						{
							continue;
						}
						long num = A(item2.A, 103319626L);
						if (num != 0L && !(e(num + 108) <= 0f))
						{
							float num2 = BitConverter.ToSingle(value, 44);
							float num3 = BitConverter.ToSingle(value, 48);
							float num4 = BitConverter.ToSingle(value, 52);
							if (global::b.a.A(num2, num3, num4))
							{
								list2.Add(new float[3] { num2, num3, num4 });
							}
						}
					}
					catch
					{
					}
				}
				if (F(item.A + 104) == item.a)
				{
					Z z2 = global::b.a.A(item, list2);
					if (z2 != null)
					{
						A(z2, item);
						a(z2, item);
						list.Add(z2);
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

	public List<global::b.f> A(IEnumerable<N> P_0)
	{
		List<global::b.f> list = new List<global::b.f>();
		foreach (N item in P_0)
		{
			if (item.a != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz())
			{
				continue;
			}
			long num = item.A + 1016;
			uint num2 = E(num + 44);
			uint num3 = E(num + 40);
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
					if (F(num6) != this.m_F + 64568064)
					{
						continue;
					}
					byte[] array = A(num6, 164);
					if (!global::b.F.A(array, this.m_F, item.a))
					{
						continue;
					}
					string text = G(BitConverter.ToInt64(array, 56));
					string text2 = global::b.F.A(text);
					if (text2 != null)
					{
						uint num7 = BitConverter.ToUInt32(array, 12);
						uint num8 = BitConverter.ToUInt32(array, 8);
						if (num7 != 0 && num7 <= 16 && num8 != uint.MaxValue && A(this.m_G, A(A(this.m_G, num8), (int)(num7 * 20)), 0, out var num9, out var num10, out var num11) && A(num6, 164).SequenceEqual(array))
						{
							list.Add(new global::b.f
							{
								A = item.a + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + num6 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + BitConverter.ToUInt32(array, 68),
								A = item.A,
								a = text,
								B = text2,
								A = (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.hn()),
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
			if (E(num + 44) != num2 || E(num + 40) != num3)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jF());
			}
		}
		return list;
	}

	private static bool A(uint P_0)
	{
		return (P_0 & 0x20) != 0;
	}

	private IEnumerable<long> g(long P_0)
	{
		HashSet<long> hashSet = new HashSet<long>();
		int[] array = new int[2] { 200, 472 };
		foreach (int num2 in array)
		{
			try
			{
				long num3 = F(P_0 + num2);
				if (num3 > 65536 && num3 != P_0 && F(num3 + 136) == P_0)
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
			long num4 = F(P_0 + 504);
			if (num4 > 65536)
			{
				uint num5 = E(num4 + 124);
				long num6 = F(num4 + 112);
				if (num5 <= 512 && num6 > 65536)
				{
					byte[] value = A(num6, (int)(num5 * 8));
					for (int num7 = 0; num7 < num5; num7++)
					{
						long num8 = BitConverter.ToInt64(value, num7 * 8);
						try
						{
							if (num8 > 65536 && F(num8 + 136) == P_0)
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

	private long H(long P_0)
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
			if (F(num) == this.m_F + 65713904)
			{
				break;
			}
			num = F(num + 136);
		}
		if (num == 0L || F(num) != this.m_F + 65713904)
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
				long num4 = F(num3);
				if (num4 == this.m_F + 65513472)
				{
					return num3;
				}
				if (num4 == this.m_F + 65511760)
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

	private global::a.y A(long P_0, out bool P_1)
	{
		P_1 = false;
		if (P_0 < 65536 || F(P_0) != this.m_F + 65513472)
		{
			throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jf());
		}
		byte[] array = A(P_0, 552);
		byte[] array2 = A(P_0, array);
		float num = a(array2, a(111989640L));
		float num2 = a(array2, a(111989680L));
		long num3 = BitConverter.ToInt64(array, 528);
		if (num3 < 65536)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jG());
		}
		byte[] value = A(num3, 344);
		float num4 = BitConverter.ToSingle(value, 296);
		float num5 = BitConverter.ToSingle(value, 300);
		if (num4 < 10f || num5 < 10f || num4 > 20000f || num5 > 20000f)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jg());
		}
		global::a.x x2 = global::a.x.A;
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
				throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jH());
			}
			byte[] array3 = A(num6, 344);
			long num9 = BitConverter.ToInt64(array3, 0) - this.m_F;
			if (num6 == P_0)
			{
				flag = true;
			}
			if (num9 == 65713904)
			{
				flag2 = true;
			}
			flag4 &= A(BitConverter.ToUInt32(array3, 192));
			long num10 = A(A(num6, array3), 40);
			if (num6 == num3 && num10 == 0L)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jh());
			}
			if (num10 != 0L)
			{
				x2 = x2.A(B(num10));
			}
			float num11 = BitConverter.ToSingle(array3, 168);
			float num12 = BitConverter.ToSingle(array3, 172);
			if (!global::b.a.A(num11, 0f, num12))
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jI());
			}
			x2 = x2.A(global::a.x.A(num11, num12));
			long num13 = BitConverter.ToInt64(array3, 136);
			if (F(num6 + 136) != num13)
			{
				throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ji());
			}
			if (num13 == 0L)
			{
				flag3 = num9 == 64823200;
				num7 = BitConverter.ToSingle(array3, 296);
				num8 = BitConverter.ToSingle(array3, 300);
			}
			num6 = num13;
		}
		if (!flag || !flag2 || !flag3)
		{
			throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jJ());
		}
		if (num7 < 200f || num8 < 200f || num7 > 20000f || num8 > 20000f)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jj());
		}
		if (F(P_0 + 528) != num3 || F(P_0) != this.m_F + 65513472)
		{
			throw new global::B.b(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jK());
		}
		global::a.y y2 = global::a.x.A(num, num2, num4, num5, num7, num8, x2);
		if (!y2.A || y2.A < 0f || y2.A > num7 || y2.a < 0f || y2.a > num8)
		{
			throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jk());
		}
		P_1 = flag4;
		return y2;
	}

	public bool a(out global::a.y P_0)
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
			B(this.b);
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
				this.C = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jL();
				result = false;
			}
			else if (DateTime.UtcNow < this.m_C)
			{
				result = false;
			}
			else
			{
				this.m_C = DateTime.UtcNow.AddMilliseconds(500.0);
				num = H(num3);
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
				this.C = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.JC();
				result = false;
			}
			goto end_IL_0038;
			IL_0126:
			bool flag;
			global::a.y y3 = A(num, out flag);
			B(this.b);
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
			global::b.I.A(this.m_A.Id, this.b, flag);
			this.C = (flag ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jl());
			if (!flag)
			{
				result = false;
			}
			else
			{
				P_0 = y3;
				result = true;
			}
			end_IL_0038:;
		}
		catch (Exception ex)
		{
			this.C = ex.Message;
			if (ex is global::B.b)
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

	private void A(long P_0, long P_1, N P_2, HashSet<long> P_3)
	{
		uint num = E(P_1 + 124);
		if (num == uint.MaxValue)
		{
			return;
		}
		long num2 = A(P_1, 103319626L);
		if (num2 == 0L || e(num2 + 108) <= 0f)
		{
			return;
		}
		long num3 = A(P_0, num);
		long item = F(num3 + 104);
		string text = G(F(num3 + 56));
		if (text.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.br()) && !text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jM()) && !text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jm()) && !text.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jN()))
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

	private void A(long P_0, string P_1, N P_2)
	{
		if (!global::a.G.a(P_1))
		{
			return;
		}
		long num = A(P_0, 103319626L);
		if (num != 0L && !(e(num + 108) <= 0f))
		{
			long num2 = A(P_0, 103319627L);
			if (num2 == 0L || E(num2 + 40) == 0)
			{
				P_2.A.Add(new global::a.w
				{
					A = P_0,
					A = P_1,
					a = P_2.a,
					A = global::a.G.A(P_1),
					A = e(P_0 + 44),
					a = e(P_0 + 48),
					B = e(P_0 + 52)
				});
			}
		}
	}
}
