using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;
using a;
using b;

namespace B
{
	internal sealed class J
	{
		public readonly global::b.i A = new global::b.i();

		public readonly C A = new C();

		public readonly global::b.c A = new global::b.c();

		public readonly y A = new y();

		public bool A = true;

		public bool a = true;

		public Color A = Color.Red;

		public Color a = Color.Lime;

		public int A = 44;

		public bool B = true;

		public bool b = true;

		public bool C = true;

		public bool c;

		public int a = 14;

		public int B = 14;

		public int b = 8;

		public int C = 94;

		public Color B = Color.FromArgb(255, 225, 95);

		public Color b = Color.FromArgb(150, 220, 255);

		public bool D = true;

		public readonly bool[] A = Enumerable.Repeat(element: true, 10).ToArray();

		public static readonly string[] A = new string[10]
		{
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fo(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FP(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fp(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FQ(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fq(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FR(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fr(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FS(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fs(),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FT()
		};

		private static string A => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ey());

		public static J A(string P_0 = null)
		{
			J j2 = new J();
			try
			{
				string[] array = File.ReadAllLines(P_0 ?? J.A);
				foreach (string text in array)
				{
					if (j2.A.A(text) || j2.B(text) || j2.A.A(text) || j2.A.A(text) || j2.A.A(text))
					{
						continue;
					}
					string[] array2 = text.Split('=');
					if (array2.Length != 2 || !int.TryParse(array2[1], out var result))
					{
						continue;
					}
					string text2 = array2[0];
					if (text2 == null)
					{
						continue;
					}
					switch (text2.Length)
					{
					case 11:
						switch (text2[0])
						{
						case 'b':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eZ())
							{
								j2.A = Math.Max(8, Math.Min(240, result));
							}
							break;
						case 'g':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ez())
							{
								j2.A = result != 0;
							}
							break;
						}
						break;
					case 5:
						switch (text2[0])
						{
						case 'm':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fa())
							{
								j2.a = result != 0;
							}
							break;
						case 'u':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FB())
							{
								j2.b = result != 0;
							}
							break;
						case 'w':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fb())
							{
								j2.C = Math.Max(40, Math.Min(100, result));
							}
							break;
						case 'c':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FC())
							{
								j2.B = Color.FromArgb(255, Color.FromArgb(result));
							}
							break;
						}
						break;
					case 10:
						switch (text2[0])
						{
						case 'a':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FE())
							{
								j2.c = result != 0;
							}
							break;
						case 'd':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fe())
							{
								j2.a = Math.Max(9, Math.Min(36, result));
							}
							break;
						case 's':
							if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FF())
							{
								j2.b = Color.FromArgb(255, Color.FromArgb(result));
							}
							break;
						}
						break;
					case 13:
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FA())
						{
							j2.B = result != 0;
						}
						break;
					case 14:
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fc())
						{
							j2.A = Color.FromArgb(255, Color.FromArgb(result));
						}
						break;
					case 17:
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FD())
						{
							j2.a = Color.FromArgb(255, Color.FromArgb(result));
						}
						break;
					case 6:
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fd())
						{
							j2.C = result != 0;
						}
						break;
					case 4:
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ff())
						{
							j2.B = Math.Max(9, Math.Min(36, result));
						}
						break;
					case 3:
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FG())
						{
							j2.b = Math.Max(0, Math.Min(300, result));
						}
						break;
					case 7:
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BW())
						{
							j2.D = result != 0;
						}
						break;
					}
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			return j2;
		}

		public void a(string P_0 = null)
		{
			File.WriteAllLines(P_0 ?? J.A, new string[16]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fg() + this.A,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FH() + (this.B ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fh() + (this.A ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FI() + (this.a ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fi() + this.A.ToArgb(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FJ() + this.a.ToArgb(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fj() + (this.b ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FK() + (this.C ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fk() + (c ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FL() + this.a,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fl() + this.B,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FM() + this.b,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fm() + C,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FN() + this.B.ToArgb(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fn() + b.ToArgb(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BY() + (D ? 1 : 0)
			}.Concat(a()).Concat(this.A.a()).Concat(this.A.a())
				.Concat(this.A.A())
				.Concat(this.A.A()));
		}

		public bool B(string P_0)
		{
			if (!P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FO()))
			{
				return false;
			}
			string[] array = P_0.Split('=');
			if (array.Length != 2)
			{
				return true;
			}
			int num = Array.IndexOf(J.A, array[0].Substring(5));
			if (num >= 0 && (array[1] == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By() || array[1] == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bw()))
			{
				this.A[num] = array[1] == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bw();
			}
			return true;
		}

		public IEnumerable<string> a()
		{
			return J.A.Select((string P_0, int P_1) => CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FO() + P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ft() + (this.A[P_1] ? 1 : 0));
		}

		[CompilerGenerated]
		private string A(string P_0, int P_1)
		{
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FO() + P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ft() + (this.A[P_1] ? 1 : 0);
		}
	}
	internal static class j
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<string, bool> A;

			public static Func<O, bool> A;

			internal bool A(string P_0)
			{
				return P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FZ());
			}

			internal bool A(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public string[] A;

			public Func<int, IEnumerable<string>> A;

			internal IEnumerable<string> A(int P_0)
			{
				return new string[2]
				{
					this.A[0].Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fv(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xa() + (P_0 + 1)),
					this.A[1] + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xB()
				};
			}
		}

		[DllImport("user32.dll", EntryPoint = "SetProcessDPIAware")]
		private static extern bool A();

		[STAThread]
		public static int A(string[] P_0)
		{
			A();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			if (P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FU()))
			{
				return a();
			}
			bool createdNew;
			using (new Mutex(initiallyOwned: true, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fu(), out createdNew))
			{
				if (!createdNew)
				{
					MessageBox.Show(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FV());
					return 0;
				}
				Application.Run(new K());
			}
			return 0;
		}

		private static int a()
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			List<string> list = new List<string>();
			try
			{
				O o2 = new O
				{
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fv(),
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA(),
					B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FW(),
					A = 450.9f,
					B = 120.2f,
					a = 80.8f,
					C = 52f,
					b = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fw(),
					d = true
				};
				o2.A.Add(new global::b.K
				{
					A = 1,
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FX(),
					A = true
				});
				o2.A.Add(new global::b.K
				{
					A = 2,
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fx(),
					A = false
				});
				o2.A.AddRange(new string[3]
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bC(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FY(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fy()
				});
				string[] A = k.B(new O[2]
				{
					o2,
					new O
					{
						A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FZ(),
						a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl()
					}
				});
				if (A.Length != 2 || !A[0].Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fz()) || Regex.IsMatch(A[1], CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fA()) || A[1].Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fa()) || A.Any((string P_0) => P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FZ())))
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fB());
				}
				O o3 = new O
				{
					A = 9u,
					A = o2.A,
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
				};
				O o4 = new O
				{
					A = 10u,
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl(),
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl()
				};
				O[] array = new O[3] { o2, o3, o4 };
				global::a.A obj = new global::a.A();
				obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fb());
				if (obj.A(array).Count() != 2)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fC());
				}
				obj.A(o2, false);
				o2.A += 10f;
				obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fb());
				if (obj.A(array).Single() != o3)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fc());
				}
				obj.A(o3, false);
				if (k.B(obj.A(array)).Length != 0)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fD());
				}
				obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fd());
				if (obj.A(array).Count() != 2)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fE());
				}
				list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fe());
				list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fF());
				global::A.m.A(list);
				string text = Path.Combine(Path.GetTempPath(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ff() + Guid.NewGuid().ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ck()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fG());
				try
				{
					J obj2 = new J();
					obj2.B = 24;
					obj2.b = 60;
					obj2.C = 80;
					obj2.B = Color.Cyan;
					obj2.b = Color.Lime;
					obj2.D = false;
					obj2.a(text);
					J j2 = J.A(text);
					if (j2.B != 24 || j2.b != 60 || j2.C != 80 || j2.B.ToArgb() != Color.Cyan.ToArgb() || j2.b.ToArgb() != Color.Lime.ToArgb() || j2.D)
					{
						throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fg());
					}
					list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fH());
				}
				finally
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
				}
				using (global::A.z z2 = new global::A.z(new J(), false))
				{
					int num = z2.B;
					int num2 = 134742176;
					if ((num & num2) != num2)
					{
						throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fh());
					}
					z2.B();
					if (z2.Visible)
					{
						throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fI());
					}
					list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fi());
				}
				int[] array2 = new int[2] { 14, 24 };
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					int num4 = array2[num3];
					using Bitmap bitmap = new Bitmap(1800, 780);
					using Graphics graphics = Graphics.FromImage(bitmap);
					using Font font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), num4, FontStyle.Bold);
					graphics.Clear(Color.FromArgb(38, 45, 43));
					string[] array3 = Enumerable.Range(0, 4).SelectMany((int P_0) => new string[2]
					{
						A[0].Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fv(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xa() + (P_0 + 1)),
						A[1] + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xB()
					}).ToArray();
					int num5 = i.A(graphics, array3, font, Color.FromArgb(255, 225, 95), Color.FromArgb(150, 220, 255), 1800, true);
					if (num5 > 780)
					{
						throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fJ() + num4);
					}
					bitmap.Save(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fj() + num4 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fK()));
					list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fk() + num4 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fL() + num5);
				}
				using (K k2 = new K(true))
				{
					using Bitmap bitmap2 = new Bitmap(k2.Width, k2.Height);
					k2.ShowInTaskbar = false;
					k2.Opacity = 0.0;
					k2.Show();
					Application.DoEvents();
					k2.DrawToBitmap(bitmap2, new Rectangle(0, 0, bitmap2.Width, bitmap2.Height));
					bitmap2.Save(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fl()));
					k2.Close();
				}
				using (global::A.M m2 = new global::A.M())
				{
					using z z3 = new z(m2);
					using Bitmap bitmap3 = new Bitmap(z3.Width, z3.Height);
					z3.ShowInTaskbar = false;
					z3.Opacity = 0.0;
					z3.Show();
					Application.DoEvents();
					z3.DrawToBitmap(bitmap3, new Rectangle(0, 0, bitmap3.Width, bitmap3.Height));
					bitmap3.Save(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fM()));
					z3.Close();
				}
				try
				{
					using n n2 = new n();
					List<O> list2 = n2.k();
					list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fm() + list2.Count + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fN() + list2.Count((O P_0) => P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fn());
				}
				catch (Exception ex)
				{
					list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fO() + ex.Message);
				}
				File.WriteAllLines(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fo()), list);
				return 0;
			}
			catch (Exception ex2)
			{
				list.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fP() + ex2);
				File.WriteAllLines(Path.Combine(baseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fo()), list);
				return 1;
			}
		}
	}
}
namespace b
{
	internal static class J
	{
		private sealed class A
		{
			public int A;

			public string A;

			public bool A;

			public DateTime A;
		}

		private static volatile A m_A;

		public static void A(int P_0, string P_1, bool P_2)
		{
			J.m_A = new A
			{
				A = P_0,
				A = P_1,
				A = P_2,
				A = DateTime.UtcNow
			};
		}

		public static bool A(int P_0, string P_1)
		{
			A a2 = J.m_A;
			if (a2 != null && a2.A == P_0 && a2.A == P_1 && a2.A)
			{
				return DateTime.UtcNow - a2.A < TimeSpan.FromSeconds(2.0);
			}
			return false;
		}
	}
	internal sealed class j : Control
	{
		private bool m_A;

		[CompilerGenerated]
		private EventHandler m_A;

		public bool A
		{
			get
			{
				return this.m_A;
			}
			set
			{
				if (this.m_A != flag)
				{
					this.m_A = flag;
					Invalidate();
					if (this.m_A != null)
					{
						this.m_A(this, EventArgs.Empty);
					}
				}
			}
		}

		[SpecialName]
		[CompilerGenerated]
		public void A(EventHandler P_0)
		{
			EventHandler eventHandler = this.m_A;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value = (EventHandler)Delegate.Combine(eventHandler2, P_0);
				eventHandler = Interlocked.CompareExchange(ref this.m_A, value, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}

		[SpecialName]
		[CompilerGenerated]
		public void a(EventHandler P_0)
		{
			EventHandler eventHandler = this.m_A;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value = (EventHandler)Delegate.Remove(eventHandler2, P_0);
				eventHandler = Interlocked.CompareExchange(ref this.m_A, value, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}

		protected override void OnClick(EventArgs P_0)
		{
			this.A = !this.A;
			base.OnClick(P_0);
		}

		protected override void OnKeyDown(KeyEventArgs P_0)
		{
			if (P_0.KeyCode == Keys.Space)
			{
				OnClick(EventArgs.Empty);
				P_0.Handled = true;
				P_0.SuppressKeyPress = true;
			}
			base.OnKeyDown(P_0);
		}

		public j()
		{
			base.TabStop = true;
			base.AccessibleRole = AccessibleRole.CheckButton;
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			AutoSize = false;
			base.Size = new Size(190, 30);
			Font = global::a.J.A(9f);
			Cursor = Cursors.Hand;
			base.Margin = new Padding(0, 2, 6, 2);
			ForeColor = global::B.L.b;
			BackColor = global::B.L.a;
			SetStyle(ControlStyles.Opaque, value: true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
		}

		protected override void OnParentChanged(EventArgs P_0)
		{
			base.OnParentChanged(P_0);
			if (base.Parent != null)
			{
				BackColor = base.Parent.BackColor;
			}
			Invalidate();
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			Graphics graphics = P_0.Graphics;
			using (SolidBrush brush = new SolidBrush(BackColor))
			{
				graphics.FillRectangle(brush, base.ClientRectangle);
			}
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			Point[] points = new Point[8]
			{
				new Point(2, 10),
				new Point(7, 5),
				new Point(38, 5),
				new Point(43, 10),
				new Point(43, 20),
				new Point(38, 25),
				new Point(7, 25),
				new Point(2, 20)
			};
			using (SolidBrush brush2 = new SolidBrush(this.A ? Color.FromArgb(34, 96, 111) : Color.FromArgb(20, 34, 46)))
			{
				graphics.FillPolygon(brush2, points);
			}
			using (Pen pen = new Pen(this.A ? global::B.L.D : global::B.L.c))
			{
				graphics.DrawPolygon(pen, points);
			}
			int num = (this.A ? 28 : 6);
			using (SolidBrush brush3 = new SolidBrush(this.A ? Color.FromArgb(197, 248, 248) : global::B.L.C))
			{
				graphics.FillRectangle(brush3, num, 8, 11, 14);
			}
			TextRenderer.DrawText(graphics, Text, Font, new Rectangle(53, 0, base.Width - 54, base.Height), base.Enabled ? global::B.L.b : global::B.L.C, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
			if (Focused && ShowFocusCues)
			{
				using (Pen pen2 = new Pen(global::B.L.D))
				{
					graphics.DrawLine(pen2, 53, base.Height - 2, base.Width - 3, base.Height - 2);
				}
			}
		}
	}
}
