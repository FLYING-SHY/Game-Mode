using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using B;
using a;
using b;

namespace A
{
	internal static class H
	{
		public static bool A(k P_0, G P_1, float P_2, float P_3)
		{
			if (P_0.a || (P_1 != null && P_1.A) || !global::b.a.A(P_2, 0f, P_3))
			{
				return false;
			}
			float num = P_1?.a ?? P_0.D;
			float num2 = P_1?.B ?? P_0.d;
			return (P_2 - P_0.A) * P_0.b + (P_3 - P_0.B) * P_0.C > num2 + num + 10f;
		}

		public static k A(string P_0, byte[] P_1, byte[] P_2, long P_3, long P_4, long P_5, uint P_6, uint P_7)
		{
			if (P_1 == null || P_1.Length < 392 || P_2 == null || P_2.Length < 284 || BitConverter.ToUInt32(P_1, 104) != P_6 || BitConverter.ToUInt32(P_1, 108) != P_7 || (BitConverter.ToUInt32(P_1, 160) & 0x4000) != 0)
			{
				return null;
			}
			if (BitConverter.ToInt64(P_2, 0) != P_3 + 64559016 || BitConverter.ToUInt32(P_2, 36) + P_4 != P_5 || BitConverter.ToUInt32(P_2, 64) != 1 || P_2[16] != 2)
			{
				return null;
			}
			float num = BitConverter.ToSingle(P_2, 20);
			float num2 = BitConverter.ToSingle(P_2, 24);
			float num3 = BitConverter.ToSingle(P_2, 28);
			float num4 = BitConverter.ToSingle(P_2, 272);
			float value = BitConverter.ToSingle(P_2, 276);
			float num5 = BitConverter.ToSingle(P_2, 280);
			float num6 = BitConverter.ToSingle(P_1, 48);
			double num7 = Math.Sqrt(num4 * num4 + num5 * num5);
			if (!global::b.a.A(num, num2, num3) || double.IsNaN(num7) || double.IsInfinity(num7) || num7 < 0.95 || num7 > 1.01 || float.IsNaN(value) || float.IsInfinity(value) || (double)Math.Abs(value) > 0.1 || float.IsNaN(num6) || float.IsInfinity(num6) || num6 - num2 < 8f)
			{
				return null;
			}
			bool flag = P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aU();
			bool flag2 = P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.au();
			bool flag3 = P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aV();
			if (!flag && !flag2 && !flag3)
			{
				return null;
			}
			num4 = (float)((double)num4 / num7);
			num5 = (float)((double)num5 / num7);
			G g2 = f.A(flag3 ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aw() : (flag ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aW() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.av()));
			float num8 = (flag ? 2.5f : 0f);
			return new k
			{
				A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aX() + P_5 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + P_6,
				a = g2.a,
				A = num + num4 * num8,
				a = num2,
				B = num3 + num5 * num8,
				b = num4,
				C = num5,
				c = g2.A,
				D = g2.a,
				d = g2.B
			};
		}

		public static bool A(byte[] P_0, byte[] P_1, long P_2, long P_3, out float P_4, out float P_5, out float P_6, out float P_7, out float P_8, bool P_9 = false)
		{
			P_4 = (P_5 = (P_6 = (P_7 = (P_8 = 0f))));
			if (P_0 == null || P_0.Length < 148 || P_1 == null || P_1.Length != 20 || BitConverter.ToInt64(P_0, 0) != P_2 + 64568064 || P_0[76] == 2 || P_0[20] != 6 || BitConverter.ToInt64(P_0, 24) != P_3 || (P_0[144] & 1) == 0)
			{
				return false;
			}
			if (!S.A(P_1, 0, out P_4, out P_5, out P_6))
			{
				return false;
			}
			if (P_9)
			{
				P_7 = 0f;
				P_8 = 1f;
				return true;
			}
			float num = BitConverter.ToSingle(P_0, 40);
			float value = BitConverter.ToSingle(P_0, 44);
			float num2 = BitConverter.ToSingle(P_0, 48);
			double num3 = Math.Sqrt(num * num + num2 * num2);
			if (double.IsNaN(num3) || double.IsInfinity(num3) || num3 < 0.95 || num3 > 1.05 || (double)Math.Abs(value) > 0.02)
			{
				return false;
			}
			P_7 = (float)((double)num / num3);
			P_8 = (float)((double)num2 / num3);
			return true;
		}

		public static k A(byte[] P_0, byte[] P_1, byte[] P_2, long P_3, long P_4, long P_5, uint P_6, uint P_7)
		{
			if (P_0.Length < 392 || P_1.Length < 68 || P_2.Length < 108 || BitConverter.ToUInt32(P_0, 104) != P_6 || BitConverter.ToUInt32(P_0, 108) != P_7 || (BitConverter.ToUInt32(P_0, 160) & 0x4000) != 0)
			{
				return null;
			}
			if (BitConverter.ToInt64(P_1, 0) != P_3 + 65313128 || BitConverter.ToUInt32(P_1, 36) + P_4 != P_5 || BitConverter.ToUInt32(P_1, 64) != 3 || BitConverter.ToInt64(P_2, 0) != P_3 + 65307976 || BitConverter.ToUInt32(P_2, 8) + P_4 != P_5 || (P_1[16] != 1 && P_1[16] != 2))
			{
				return null;
			}
			float num = BitConverter.ToSingle(P_1, 20);
			float num2 = BitConverter.ToSingle(P_1, 24);
			float num3 = BitConverter.ToSingle(P_1, 28);
			float num4 = BitConverter.ToSingle(P_2, 104);
			if (!global::b.a.A(num, num2, num3) || float.IsNaN(num4) || float.IsInfinity(num4) || num4 <= 0f || num4 > 100f || global::B.R.A(BitConverter.ToSingle(P_0, 44), BitConverter.ToSingle(P_0, 48), BitConverter.ToSingle(P_0, 52), num, num2, num3))
			{
				return null;
			}
			uint num5 = BitConverter.ToUInt32(P_2, 56);
			if (num5 == uint.MaxValue)
			{
				return null;
			}
			return new k
			{
				A = P_5 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ax() + P_6,
				a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.at(),
				A = P_4 + num5,
				A = true,
				A = num,
				a = num2,
				B = num3,
				E = num4
			};
		}
	}
	internal sealed class h : IDisposable
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.N, bool> A;

			public static Func<global::B.N, global::B.S> A;

			internal bool A(global::B.N P_0)
			{
				return P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz();
			}

			internal global::B.S a(global::B.N P_0)
			{
				return new global::B.S
				{
					A = P_0.A,
					A = P_0.A,
					A = (P_0.b ? P_0.A : Color.Red)
				};
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public global::b.b A;

			public h A;

			public ManualResetEvent A;

			internal void A()
			{
				bool flag = false;
				try
				{
					this.A.m_A = new I(this.A.A(), true, this.A.m_A);
					_ = this.A.m_A.Handle;
					flag = true;
					this.A.Set();
					Application.Run();
				}
				catch (Exception ex)
				{
					this.A.m_A = ex.Message;
					if (!flag)
					{
						this.A.Set();
					}
				}
				finally
				{
					if (this.A.m_A != null)
					{
						this.A.m_A.Dispose();
					}
				}
			}
		}

		private readonly global::b.b m_A;

		private readonly global::b.h m_A;

		private readonly Thread m_A;

		private I m_A;

		private int m_A;

		private volatile string m_A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		public string A
		{
			get
			{
				if (this.m_A.Length <= 0)
				{
					if (this.m_A != null)
					{
						return this.m_A.A;
					}
					return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
				}
				return this.m_A;
			}
		}

		public h(global::b.b P_0, bool P_1 = true, global::b.h P_2 = null)
		{
			h A = this;
			this.m_A = P_0;
			this.m_A = P_2 ?? new global::b.h
			{
				A = false
			};
			if (!P_1)
			{
				this.m_A = new I(P_0.A(), false, this.m_A);
				return;
			}
			ManualResetEvent A2 = new ManualResetEvent(initialState: false);
			try
			{
				this.m_A = new Thread((ThreadStart)delegate
				{
					bool flag = false;
					try
					{
						A.m_A = new I(P_0.A(), true, A.m_A);
						_ = A.m_A.Handle;
						flag = true;
						A2.Set();
						Application.Run();
					}
					catch (Exception ex)
					{
						A.m_A = ex.Message;
						if (!flag)
						{
							A2.Set();
						}
					}
					finally
					{
						if (A.m_A != null)
						{
							A.m_A.Dispose();
						}
					}
				})
				{
					IsBackground = true,
					Name = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.aY()
				};
				this.m_A.SetApartmentState(ApartmentState.STA);
				this.m_A.Start();
				A2.WaitOne();
			}
			finally
			{
				if (A2 != null)
				{
					((IDisposable)A2).Dispose();
				}
			}
		}

		public void a()
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A(this.m_A.A(), this.m_A);
			}
		}

		public void A(global::B.m P_0, int P_1, IEnumerable<global::B.N> P_2)
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A(new global::a.u
				{
					A = P_0.I(),
					A = (from n2 in P_2
						where n2.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()
						select new global::B.S
						{
							A = n2.A,
							A = n2.A,
							A = (n2.b ? n2.A : Color.Red)
						}).ToArray(),
					A = DateTime.UtcNow
				});
			}
		}

		public void B()
		{
			if (this.m_A != null)
			{
				this.m_A.A();
			}
		}

		public void b()
		{
			if (this.m_A != null)
			{
				this.m_A.a();
			}
		}

		public void C()
		{
			if (this.m_A != null && Volatile.Read(ref this.m_A) == 0)
			{
				this.m_A.A((global::a.u)null);
			}
		}

		public void Dispose()
		{
			if (Interlocked.Exchange(ref this.m_A, 1) != 0 || this.m_A == null)
			{
				return;
			}
			if (this.m_A == null)
			{
				this.m_A.Dispose();
				return;
			}
			try
			{
				this.m_A.BeginInvoke((Action)delegate
				{
					this.m_A.Close();
					Application.ExitThread();
				});
			}
			catch (InvalidOperationException)
			{
			}
			if (Thread.CurrentThread != this.m_A)
			{
				this.m_A.Join();
			}
		}

		[CompilerGenerated]
		private void c()
		{
			this.m_A.Close();
			Application.ExitThread();
		}
	}
}
namespace a
{
	internal interface H : IDisposable
	{
		Task<o> A(string P_0, Dictionary<string, object> P_1, string P_2, CancellationToken P_3);
	}
	internal static class h
	{
		public static string A(string P_0)
		{
			if (string.IsNullOrEmpty(P_0) || (!P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.@as()) && !P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.br())))
			{
				return null;
			}
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (new string[7]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DW(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dw(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DX(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dx(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DY(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dy(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DZ()
			}.Contains(text))
			{
				return global::b.L.A(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Dz());
			}
			if (new string[3]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dA(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.da(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dB()
			}.Contains(text))
			{
				return global::b.L.A(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.db());
			}
			if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dC()))
			{
				return null;
			}
			return global::b.L.A(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dc());
		}
	}
}
