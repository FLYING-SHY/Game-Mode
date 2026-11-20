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
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using A;
using B;
using a;
using b;

namespace B
{
	internal sealed class I
	{
		public readonly global::b.h A = new global::b.h();

		public readonly B A = new B();

		public readonly global::b.b A = new global::b.b();

		public readonly x A = new x();

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
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FL(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fl(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FM(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fm(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FN(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fn(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FO(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fo(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FP(),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fp()
		};

		private static string A => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eV());

		public static I A(string P_0 = null)
		{
			I i2 = new I();
			try
			{
				string[] array = File.ReadAllLines(P_0 ?? I.A);
				foreach (string text in array)
				{
					if (i2.A.A(text) || i2.B(text) || i2.A.A(text) || i2.A.A(text) || i2.A.A(text))
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
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ev())
							{
								i2.A = Math.Max(8, Math.Min(240, result));
							}
							break;
						case 'g':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eW())
							{
								i2.A = result != 0;
							}
							break;
						}
						break;
					case 5:
						switch (text2[0])
						{
						case 'm':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eX())
							{
								i2.a = result != 0;
							}
							break;
						case 'u':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ex())
							{
								i2.b = result != 0;
							}
							break;
						case 'w':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eY())
							{
								i2.C = Math.Max(40, Math.Min(100, result));
							}
							break;
						case 'c':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ey())
							{
								i2.B = Color.FromArgb(255, Color.FromArgb(result));
							}
							break;
						}
						break;
					case 10:
						switch (text2[0])
						{
						case 'a':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fa())
							{
								i2.c = result != 0;
							}
							break;
						case 'd':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FB())
							{
								i2.a = Math.Max(9, Math.Min(36, result));
							}
							break;
						case 's':
							if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fb())
							{
								i2.b = Color.FromArgb(255, Color.FromArgb(result));
							}
							break;
						}
						break;
					case 13:
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ew())
						{
							i2.B = result != 0;
						}
						break;
					case 14:
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eZ())
						{
							i2.A = Color.FromArgb(255, Color.FromArgb(result));
						}
						break;
					case 17:
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ez())
						{
							i2.a = Color.FromArgb(255, Color.FromArgb(result));
						}
						break;
					case 6:
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FA())
						{
							i2.C = result != 0;
						}
						break;
					case 4:
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FC())
						{
							i2.B = Math.Max(9, Math.Min(36, result));
						}
						break;
					case 3:
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fc())
						{
							i2.b = Math.Max(0, Math.Min(300, result));
						}
						break;
					case 7:
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bv())
						{
							i2.D = result != 0;
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
			return i2;
		}

		public void a(string P_0 = null)
		{
			File.WriteAllLines(P_0 ?? I.A, new string[16]
			{
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FD() + this.A,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fd() + (this.B ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FE() + (this.A ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fe() + (this.a ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FF() + this.A.ToArgb(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ff() + this.a.ToArgb(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FG() + (this.b ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fg() + (this.C ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FH() + (c ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fh() + this.a,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FI() + this.B,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fi() + this.b,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FJ() + C,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fj() + this.B.ToArgb(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FK() + b.ToArgb(),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bx() + (D ? 1 : 0)
			}.Concat(a()).Concat(this.A.a()).Concat(this.A.a())
				.Concat(this.A.A())
				.Concat(this.A.A()));
		}

		public bool B(string P_0)
		{
			if (!P_0.StartsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fk()))
			{
				return false;
			}
			string[] array = P_0.Split('=');
			if (array.Length != 2)
			{
				return true;
			}
			int num = Array.IndexOf(I.A, array[0].Substring(5));
			if (num >= 0 && (array[1] == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BY() || array[1] == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BW()))
			{
				this.A[num] = array[1] == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BW();
			}
			return true;
		}

		public IEnumerable<string> a()
		{
			return I.A.Select((string P_0, int P_1) => _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fk() + P_0 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FQ() + (this.A[P_1] ? 1 : 0));
		}

		[CompilerGenerated]
		private string A(string P_0, int P_1)
		{
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fk() + P_0 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FQ() + (this.A[P_1] ? 1 : 0);
		}
	}
	internal static class i
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<string, bool> A;

			public static Func<N, bool> A;

			internal bool A(string P_0)
			{
				return P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fv());
			}

			internal bool A(N P_0)
			{
				return P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz();
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
					this.A[0].Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FS(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XW() + (P_0 + 1)),
					this.A[1] + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xw()
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
			if (P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fq()))
			{
				return a();
			}
			bool createdNew;
			using (new Mutex(initiallyOwned: true, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FR(), out createdNew))
			{
				if (!createdNew)
				{
					MessageBox.Show(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fr());
					return 0;
				}
				Application.Run(new J());
			}
			return 0;
		}

		private static int a()
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			List<string> list = new List<string>();
			try
			{
				N n2 = new N
				{
					A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FS(),
					a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz(),
					B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fs(),
					A = 450.9f,
					B = 120.2f,
					a = 80.8f,
					C = 52f,
					b = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FT(),
					d = true
				};
				n2.A.Add(new global::b.J
				{
					A = 1,
					A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ft(),
					A = true
				});
				n2.A.Add(new global::b.J
				{
					A = 2,
					A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FU(),
					A = false
				});
				n2.A.AddRange(new string[3]
				{
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.bb(),
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fu(),
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FV()
				});
				string[] A = j.B(new N[2]
				{
					n2,
					new N
					{
						A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fv(),
						a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL()
					}
				});
				if (A.Length != 2 || !A[0].Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FW()) || Regex.IsMatch(A[1], _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fw()) || A[1].Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FX()) || A.Any((string P_0) => P_0.Contains(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fv())))
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fx());
				}
				N n3 = new N
				{
					A = 9u,
					A = n2.A,
					a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()
				};
				N n4 = new N
				{
					A = 10u,
					A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL(),
					a = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.DL()
				};
				N[] array = new N[3] { n2, n3, n4 };
				global::A.z obj = new global::A.z();
				obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FY());
				if (obj.A(array).Count() != 2)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fy());
				}
				obj.A(n2, false);
				n2.A += 10f;
				obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FY());
				if (obj.A(array).Single() != n3)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FZ());
				}
				obj.A(n3, false);
				if (j.B(obj.A(array)).Length != 0)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Fz());
				}
				obj.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fA());
				if (obj.A(array).Count() != 2)
				{
					throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fa());
				}
				list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fB());
				list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fb());
				global::A.M.A(list);
				string text = Path.Combine(Path.GetTempPath(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fC() + Guid.NewGuid().ToString(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.CK()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fc());
				try
				{
					I obj2 = new I();
					obj2.B = 24;
					obj2.b = 60;
					obj2.C = 80;
					obj2.B = Color.Cyan;
					obj2.b = Color.Lime;
					obj2.D = false;
					obj2.a(text);
					I i2 = I.A(text);
					if (i2.B != 24 || i2.b != 60 || i2.C != 80 || i2.B.ToArgb() != Color.Cyan.ToArgb() || i2.b.ToArgb() != Color.Lime.ToArgb() || i2.D)
					{
						throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fD());
					}
					list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fd());
				}
				finally
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
				}
				using (global::A.Z z2 = new global::A.Z(new I(), false))
				{
					int num = z2.B;
					int num2 = 134742176;
					if ((num & num2) != num2)
					{
						throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fE());
					}
					z2.B();
					if (z2.Visible)
					{
						throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fe());
					}
					list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fF());
				}
				int[] array2 = new int[2] { 14, 24 };
				for (int num3 = 0; num3 < array2.Length; num3++)
				{
					int num4 = array2[num3];
					using Bitmap bitmap = new Bitmap(1800, 780);
					using Graphics graphics = Graphics.FromImage(bitmap);
					using Font font = new Font(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ba(), num4, FontStyle.Bold);
					graphics.Clear(Color.FromArgb(38, 45, 43));
					string[] array3 = Enumerable.Range(0, 4).SelectMany((int P_0) => new string[2]
					{
						A[0].Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FS(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XW() + (P_0 + 1)),
						A[1] + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xw()
					}).ToArray();
					int num5 = h.A(graphics, array3, font, Color.FromArgb(255, 225, 95), Color.FromArgb(150, 220, 255), 1800, true);
					if (num5 > 780)
					{
						throw new Exception(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ff() + num4);
					}
					bitmap.Save(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fG() + num4 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fg()));
					list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fH() + num4 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fh() + num5);
				}
				using (J j2 = new J(true))
				{
					using Bitmap bitmap2 = new Bitmap(j2.Width, j2.Height);
					j2.ShowInTaskbar = false;
					j2.Opacity = 0.0;
					j2.Show();
					Application.DoEvents();
					j2.DrawToBitmap(bitmap2, new Rectangle(0, 0, bitmap2.Width, bitmap2.Height));
					bitmap2.Save(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fI()));
					j2.Close();
				}
				using (global::A.l l2 = new global::A.l())
				{
					using y y2 = new y(l2);
					using Bitmap bitmap3 = new Bitmap(y2.Width, y2.Height);
					y2.ShowInTaskbar = false;
					y2.Opacity = 0.0;
					y2.Show();
					Application.DoEvents();
					y2.DrawToBitmap(bitmap3, new Rectangle(0, 0, bitmap3.Width, bitmap3.Height));
					bitmap3.Save(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fi()));
					y2.Close();
				}
				try
				{
					using m m2 = new m();
					List<N> list2 = m2.i();
					list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fJ() + list2.Count + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fj() + list2.Count((N P_0) => P_0.a == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bz()) + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fK());
				}
				catch (Exception ex)
				{
					list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fk() + ex.Message);
				}
				File.WriteAllLines(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fL()), list);
				return 0;
			}
			catch (Exception ex2)
			{
				list.Add(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fl() + ex2);
				File.WriteAllLines(Path.Combine(baseDirectory, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fL()), list);
				return 1;
			}
		}
	}
}
namespace b
{
	internal static class I
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
			I.m_A = new A
			{
				A = P_0,
				A = P_1,
				A = P_2,
				A = DateTime.UtcNow
			};
		}

		public static bool A(int P_0, string P_1)
		{
			A a2 = I.m_A;
			if (a2 != null && a2.A == P_0 && a2.A == P_1 && a2.A)
			{
				return DateTime.UtcNow - a2.A < TimeSpan.FromSeconds(2.0);
			}
			return false;
		}
	}
	internal sealed class i : Control
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

		public i()
		{
			base.TabStop = true;
			base.AccessibleRole = AccessibleRole.CheckButton;
			SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
			AutoSize = false;
			base.Size = new Size(190, 30);
			Font = global::a.I.A(9f);
			Cursor = Cursors.Hand;
			base.Margin = new Padding(0, 2, 6, 2);
			ForeColor = global::B.K.b;
			BackColor = global::B.K.a;
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
			using (Pen pen = new Pen(this.A ? global::B.K.D : global::B.K.c))
			{
				graphics.DrawPolygon(pen, points);
			}
			int num = (this.A ? 28 : 6);
			using (SolidBrush brush3 = new SolidBrush(this.A ? Color.FromArgb(197, 248, 248) : global::B.K.C))
			{
				graphics.FillRectangle(brush3, num, 8, 11, 14);
			}
			TextRenderer.DrawText(graphics, Text, Font, new Rectangle(53, 0, base.Width - 54, base.Height), base.Enabled ? global::B.K.b : global::B.K.C, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
			if (Focused && ShowFocusCues)
			{
				using (Pen pen2 = new Pen(global::B.K.D))
				{
					graphics.DrawLine(pen2, 53, base.Height - 2, base.Width - 3, base.Height - 2);
				}
			}
		}
	}
}
