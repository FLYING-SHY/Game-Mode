using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;
using B;
using Microsoft.Win32;
using a;
using b;

namespace A
{
	internal sealed class J
	{
		public bool A = true;

		public bool a = true;

		public bool B = true;

		public int A = 25;

		public int a = 30;

		public int B = 14;

		public int b = 10;

		public decimal A = 2m;

		public J A()
		{
			return (J)MemberwiseClone();
		}

		public IEnumerable<string> a()
		{
			return new string[8]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BH() + (this.A ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bh() + (this.a ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BI() + (this.B ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bi() + this.A,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BJ() + this.a,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bj() + B,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BK() + b,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bk() + this.A.ToString(CultureInfo.InvariantCulture)
			};
		}

		public bool A(string P_0)
		{
			int num = P_0.IndexOf('=');
			if (num < 0)
			{
				return false;
			}
			string text = P_0.Substring(0, num);
			string text2 = P_0.Substring(num + 1);
			if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BL())
			{
				if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
				{
					this.A = Math.Max(0.5m, Math.Min(8m, result));
				}
				return true;
			}
			if (!new string[7]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bl(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BM(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bm(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BN(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bn(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BO(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bo()
			}.Contains(text))
			{
				return false;
			}
			if (!int.TryParse(text2, out var result2))
			{
				return true;
			}
			if (text != null)
			{
				switch (text.Length)
				{
				case 5:
					switch (text[3])
					{
					case 'O':
						if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bl())
						{
							this.A = result2 != 0;
						}
						break;
					case 'V':
						if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BM())
						{
							this.a = result2 != 0;
						}
						break;
					}
					break;
				case 9:
					if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bm())
					{
						this.B = result2 != 0;
					}
					break;
				case 10:
					if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BN())
					{
						this.A = Math.Max(0, Math.Min(100, result2));
					}
					break;
				case 14:
					if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bn())
					{
						this.a = Math.Max(0, Math.Min(100, result2));
					}
					break;
				case 7:
					if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BO())
					{
						B = Math.Max(8, Math.Min(40, result2));
					}
					break;
				case 11:
					if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bo())
					{
						b = Math.Max(6, Math.Min(24, result2));
					}
					break;
				}
			}
			return true;
		}
	}
	internal static class j
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<k, bool> A;

			public static Func<k, bool> a;

			internal bool A(k P_0)
			{
				return !P_0.A;
			}

			internal bool a(k P_0)
			{
				if (!P_0.A)
				{
					return !P_0.a;
				}
				return false;
			}
		}

		private static readonly object m_A = new object();

		private static string m_A;

		private static DateTime m_A;

		public static void A(string P_0)
		{
			lock (j.m_A)
			{
				try
				{
					if (!(j.m_A == P_0) || !(DateTime.UtcNow - j.m_A < TimeSpan.FromSeconds(10.0)))
					{
						j.m_A = P_0;
						j.m_A = DateTime.UtcNow;
						string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BP());
						if (File.Exists(text) && new FileInfo(text).Length > 262144)
						{
							File.WriteAllText(text, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
						}
						File.AppendAllText(text, DateTime.Now.ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.az()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA() + P_0 + Environment.NewLine);
					}
				}
				catch
				{
				}
			}
		}

		private static void A(List<Y> P_0, PointF P_1, PointF P_2, Color P_3, float P_4)
		{
			int num = (int)Math.Ceiling(P_4 + 4f);
			P_0.Add(new Y
			{
				A = new PointF[2] { P_1, P_2 },
				A = P_3,
				A = P_4,
				A = Rectangle.FromLTRB((int)Math.Floor(Math.Min(P_1.X, P_2.X)) - num, (int)Math.Floor(Math.Min(P_1.Y, P_2.Y)) - num, (int)Math.Ceiling(Math.Max(P_1.X, P_2.X)) + num, (int)Math.Ceiling(Math.Max(P_1.Y, P_2.Y)) + num)
			});
		}

		private static void A(List<global::a.t> P_0, PointF P_1, string P_2, Color P_3, global::a.V P_4)
		{
			Bitmap bitmap = P_4.a(new global::B.Z
			{
				a = P_2,
				A = P_3
			});
			P_0.Add(new global::a.t
			{
				A = bitmap,
				A = new Rectangle((int)(P_1.X - (float)bitmap.Width / 2f), (int)(P_1.Y - (float)bitmap.Height / 2f), bitmap.Width, bitmap.Height)
			});
		}

		private static void A(global::a.V P_0, J P_1)
		{
			P_0.A(new global::b.b
			{
				d = P_1.B
			});
		}

		public static int A(List<global::a.t> P_0, List<Y> P_1, IEnumerable<k> P_2, global::a.v P_3, J P_4, int P_5, int P_6, global::a.V P_7)
		{
			if (!P_4.A)
			{
				return 0;
			}
			A(P_7, P_4);
			int num = 0;
			foreach (k item in P_2.Where((k k2) => !k2.A))
			{
				if (item.a)
				{
					if (P_3.A(item.A, item.a + 0.15f, item.B, P_5, P_6, out var pointF))
					{
						A(P_1, new PointF(pointF.X - 9f, pointF.Y), new PointF(pointF.X + 9f, pointF.Y), item.A, (float)P_4.A);
						A(P_1, new PointF(pointF.X, pointF.Y - 9f), new PointF(pointF.X, pointF.Y + 9f), item.A, (float)P_4.A);
						A(P_0, new PointF(pointF.X, pointF.Y - 22f), item.a, item.A, P_7);
						num++;
					}
					continue;
				}
				float[][] array = g.A(item);
				int count = P_1.Count;
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					if (global::B.E.A(P_3, array[num2], array[(num2 + 1) % array.Length], P_5, P_6, out var pointF2, out var pointF3))
					{
						A(P_1, pointF2, pointF3, item.A, (float)P_4.A);
					}
				}
				float num3 = item.d + item.D / 2f;
				if (P_3.A(item.A + item.b * num3, item.a + 0.15f, item.B + item.C * num3, P_5, P_6, out var pointF4))
				{
					A(P_0, pointF4, item.a, item.A, P_7);
				}
				if (P_1.Count > count)
				{
					num++;
				}
			}
			return num;
		}

		public static void A(Graphics P_0, IEnumerable<k> P_1, global::a.v P_2, J P_3, int P_4, int P_5)
		{
			if (!P_3.A)
			{
				return;
			}
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			foreach (k item in P_1.Where((k k2) => !k2.A && !k2.a))
			{
				PointF[] array = g.A(P_2, g.A(item), P_4, P_5);
				if (array.Length >= 3)
				{
					using SolidBrush brush = new SolidBrush(Color.FromArgb(P_3.A * 255 / 100, item.A));
					P_0.FillPolygon(brush, array);
				}
			}
		}

		private static RectangleF A(int P_0, int P_1)
		{
			return new RectangleF((float)P_0 * 0.32f, (float)P_1 * 0.4f, (float)P_0 * 0.36f, (float)P_1 * 0.14f);
		}

		public static void A(List<global::a.t> P_0, List<Y> P_1, int P_2, int P_3, J P_4, global::a.V P_5)
		{
			A(P_5, P_4);
			RectangleF rectangleF = A(P_2, P_3);
			PointF[] array = new PointF[4]
			{
				new PointF(rectangleF.Left, rectangleF.Top),
				new PointF(rectangleF.Right, rectangleF.Top),
				new PointF(rectangleF.Right, rectangleF.Bottom),
				new PointF(rectangleF.Left, rectangleF.Bottom)
			};
			for (int num = 0; num < 4; num++)
			{
				A(P_1, array[num], array[(num + 1) % 4], Color.Cyan, Math.Max(2f, (float)P_4.A));
			}
			A(P_0, new PointF(rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top + rectangleF.Height / 2f), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bp(), Color.Cyan, P_5);
		}

		public static void A(Graphics P_0, int P_1, int P_2, J P_3)
		{
			using SolidBrush brush = new SolidBrush(Color.FromArgb(P_3.A * 255 / 100, Color.Cyan));
			P_0.FillRectangle(brush, A(P_1, P_2));
		}
	}
}
namespace a
{
	internal sealed class J
	{
		private readonly string m_A;

		private readonly string a;

		private readonly string B;

		private readonly string b;

		private readonly string C;

		private readonly string c;

		private readonly bool m_A;

		internal J(Dictionary<string, object> P_0, string P_1, bool P_2)
		{
			this.m_A = L.A;
			a = A(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.df()).ToUpperInvariant();
			B = A(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dG());
			if (!Guid.TryParse(A(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dg()), out var result))
			{
				throw new ArgumentException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dH());
			}
			b = result.ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dh()).ToLowerInvariant();
			C = A(P_0, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dI());
			c = P_1;
			this.m_A = P_2;
			if (!Regex.IsMatch(a, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.di()) || !Regex.IsMatch(B, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dJ()) || !Regex.IsMatch(C, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dj()))
			{
				throw new ArgumentException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dK());
			}
		}

		private static string A(Dictionary<string, object> P_0, string P_1)
		{
			if (!P_0.TryGetValue(P_1, out var value))
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
			}
			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}

		internal static string A(byte[] P_0)
		{
			using SHA256 sHA = SHA256.Create();
			return BitConverter.ToString(sHA.ComputeHash(P_0)).Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ci(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()).ToLowerInvariant();
		}

		internal static string A()
		{
			byte[] array = new byte[16];
			using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
			{
				randomNumberGenerator.GetBytes(array);
			}
			return BitConverter.ToString(array).Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ci(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()).ToLowerInvariant();
		}

		internal bool A(o P_0, string P_1, string P_2, string P_3)
		{
			try
			{
				if (P_0.A != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dk() || !Regex.IsMatch(P_3 ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dL()) || P_0.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dl()) != P_3)
				{
					return false;
				}
				byte[] array = Convert.FromBase64String(P_1);
				byte[] array2 = Convert.FromBase64String(P_2);
				if (array.Length != 384 || (array[0] & 0x80) == 0 || array2.Length == 0 || A(array).Substring(0, 16) != P_3)
				{
					return false;
				}
				string text = P_0.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dM());
				if (text.Length == 0 && !this.m_A)
				{
					text = c;
				}
				if (!Regex.IsMatch(text ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dm()))
				{
					return false;
				}
				string text2 = P_0.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dN());
				if (text2 != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dn() && text2 != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dO())
				{
					return false;
				}
				string[] array3 = new string[3];
				string[] array4 = new string[3]
				{
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.@do(),
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dP(),
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dp()
				};
				for (int num = 0; num < array4.Length; num++)
				{
					string text3 = P_0.A(array4[num]);
					if (!long.TryParse(text3, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var result) || text3 != result.ToString(CultureInfo.InvariantCulture))
					{
						return false;
					}
					array3[num] = text3;
				}
				string text4 = string.Join(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dQ(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dq(), this.m_A, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dk(), a, B, b, C, A(Encoding.UTF8.GetBytes(text)), array3[0], array3[1], array3[2], text2, P_3);
				byte[] array5 = Convert.FromBase64String(P_0.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dR()));
				if (array5.Length != 384)
				{
					return false;
				}
				using RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
				rSACryptoServiceProvider.PersistKeyInCsp = false;
				rSACryptoServiceProvider.ImportParameters(new RSAParameters
				{
					Modulus = array,
					Exponent = array2
				});
				return rSACryptoServiceProvider.VerifyData(Encoding.UTF8.GetBytes(text4), CryptoConfig.MapNameToOID(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dr()), array5);
			}
			catch (Exception ex) when (ex is FormatException || ex is CryptographicException || ex is ArgumentException)
			{
				return false;
			}
		}
	}
	internal sealed class j : ApplicationContext
	{
		[CompilerGenerated]
		private sealed class A
		{
			public Q A;

			public j A;

			internal void A()
			{
				this.A.A(this.A);
			}
		}

		private readonly l m_A;

		private readonly global::A.d m_A;

		private global::B.J m_A;

		private readonly Timer m_A = new Timer();

		private bool m_A;

		private bool m_a;

		private bool m_B;

		private long m_A;

		private global::b.k m_A;

		public j(l P_0 = null)
		{
			this.m_A = P_0 ?? new l();
			this.m_A = new global::A.d(this.m_A.A);
			_ = this.m_A.Handle;
			this.m_A.A(delegate(string text, bool flag)
			{
				this.m_A.A(text, flag);
			});
			this.m_A.FormClosing += A;
			SystemEvents.PowerModeChanged += A;
			this.m_A.A(delegate(Q q2)
			{
				if (!this.m_A && !this.m_A.IsDisposed)
				{
					try
					{
						this.m_A.BeginInvoke((Action)delegate
						{
							A(q2);
						});
					}
					catch (InvalidOperationException)
					{
					}
				}
			});
			this.m_A.Interval = 100;
			this.m_A.Tick += delegate
			{
				bool flag = N.A;
				if (flag != this.m_B)
				{
					this.m_B = flag;
					if (this.m_A != null)
					{
						if (flag)
						{
							this.m_A.l();
						}
						else
						{
							this.m_A.M();
						}
					}
				}
				if (!flag)
				{
					global::A.Q.A();
				}
			};
			this.m_A.Start();
			this.m_A.Show();
			this.m_A.a();
		}

		private void A()
		{
			if (this.m_A == null)
			{
				this.m_A = new global::B.J();
				this.m_A.FormClosing += A;
				this.m_A.b(delegate
				{
					this.m_A.B();
					this.m_A.M();
					this.m_A.Show();
					this.m_A.Activate();
				});
			}
		}

		private void A(Q P_0)
		{
			if (this.m_A || this.m_a || !this.m_A.A(P_0))
			{
				return;
			}
			this.m_A.A(P_0);
			if (this.m_A != null && (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dS() || P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ds() || P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dT() || P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dt()))
			{
				this.m_A.A();
			}
			if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dU())
			{
				if (!N.A || this.m_A == null)
				{
					return;
				}
				if (this.m_A == null)
				{
					using (global::b.k k2 = new global::b.k(P_0, false))
					{
						this.m_A = k2;
						try
						{
							k2.ShowDialog(this.m_A);
							return;
						}
						finally
						{
							this.m_A = null;
						}
					}
				}
				this.m_A.A(P_0);
			}
			else if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dk() || P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dS())
			{
				this.m_A = ((P_0.A > 0) ? P_0.A : this.m_A);
				A();
				if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dk())
				{
					this.m_A.A(this.m_A, P_0.D);
					this.m_A.l();
					this.m_B = true;
				}
				else
				{
					this.m_A.M();
					this.m_B = false;
				}
				if (this.m_A.Visible)
				{
					this.m_A.Hide();
					this.m_A.Show();
					this.m_A.Activate();
				}
			}
			else if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ds() || P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dt())
			{
				this.m_a = true;
				N.B();
				if (this.m_A != null)
				{
					this.m_A.M();
				}
				Form owner = ((this.m_A != null && this.m_A.Visible) ? ((Form)this.m_A) : ((Form)this.m_A));
				if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dt())
				{
					using global::b.k k3 = new global::b.k(P_0, true);
					k3.ShowDialog(owner);
				}
				else
				{
					using O o2 = new O(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.du(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.du(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dV());
					o2.ShowDialog(owner);
				}
				a();
			}
			else if (P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dT() || P_0.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dv())
			{
				if (this.m_A != null)
				{
					this.m_A.M();
					this.m_A.Hide();
				}
				this.m_A.Show();
			}
			else if (!N.A && this.m_A != null)
			{
				this.m_A.M();
			}
		}

		private void A(object P_0, FormClosingEventArgs P_1)
		{
			if (!this.m_A)
			{
				P_1.Cancel = true;
				a();
			}
		}

		private void A(object P_0, PowerModeChangedEventArgs P_1)
		{
			if (P_1.Mode == PowerModes.Resume)
			{
				this.m_A.b();
			}
		}

		private async void a()
		{
			if (this.m_A)
			{
				return;
			}
			this.m_A = true;
			if (this.m_A != null)
			{
				this.m_A.A();
			}
			SystemEvents.PowerModeChanged -= A;
			this.m_A.Stop();
			N.B();
			if (this.m_A != null)
			{
				this.m_A.M();
			}
			this.m_A.Hide();
			if (this.m_A != null)
			{
				this.m_A.Hide();
			}
			try
			{
				await this.m_A.E();
			}
			finally
			{
				if (this.m_A != null)
				{
					this.m_A.Close();
				}
				this.m_A.Close();
				this.m_A.Dispose();
				ExitThread();
			}
		}

		protected override void ExitThreadCore()
		{
			this.m_A.Dispose();
			base.ExitThreadCore();
		}

		[CompilerGenerated]
		private void A(string P_0, bool P_1)
		{
			this.m_A.A(P_0, P_1);
		}

		[CompilerGenerated]
		private void a(Q P_0)
		{
			if (this.m_A || this.m_A.IsDisposed)
			{
				return;
			}
			try
			{
				this.m_A.BeginInvoke((Action)delegate
				{
					A(P_0);
				});
			}
			catch (InvalidOperationException)
			{
			}
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			bool flag = N.A;
			if (flag != this.m_B)
			{
				this.m_B = flag;
				if (this.m_A != null)
				{
					if (flag)
					{
						this.m_A.l();
					}
					else
					{
						this.m_A.M();
					}
				}
			}
			if (!flag)
			{
				global::A.Q.A();
			}
		}

		[CompilerGenerated]
		private void B()
		{
			this.m_A.B();
			this.m_A.M();
			this.m_A.Show();
			this.m_A.Activate();
		}
	}
}
