using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
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
	internal sealed class K : Form
	{
		private struct A
		{
			public int A;

			public int a;

			public int B;

			public int b;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<O, bool> A;

			public static Func<O, uint> A;

			public static Func<O, global::A.Z> A;

			public static Func<O, bool> a;

			public static Func<O, bool> B;

			public static Func<O, bool> b;

			public static Func<O, IEnumerable<global::b.d>> A;

			public static Func<O, bool> C;

			public static Func<A<global::b.A, double>, double> A;

			public static Action A;

			public static Func<O, bool> c;

			public static Func<O, bool> D;

			public static DrawItemEventHandler A;

			public static Func<O, bool> d;

			public static Func<O, uint> a;

			public static Func<KeyValuePair<string, global::b.g>, bool> A;

			public static Func<KeyValuePair<string, global::b.g>, string> A;

			public static Predicate<global::A.T> A;

			internal bool A(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal uint a(O P_0)
			{
				return P_0.A;
			}

			internal global::A.Z B(O P_0)
			{
				return new global::A.Z
				{
					A = P_0
				};
			}

			internal bool b(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal bool C(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal bool c(O P_0)
			{
				return P_0.C;
			}

			internal IEnumerable<global::b.d> D(O P_0)
			{
				return P_0.A;
			}

			internal bool d(O P_0)
			{
				return !P_0.C;
			}

			internal double A(A<global::b.A, double> P_0)
			{
				return P_0.a;
			}

			internal void A()
			{
			}

			internal bool E(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal bool e(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal void A(object P_0, DrawItemEventArgs P_1)
			{
				if (P_1.Index >= 0)
				{
					ComboBox comboBox = (ComboBox)P_0;
					using (SolidBrush brush = new SolidBrush(((P_1.State & DrawItemState.Selected) != DrawItemState.None) ? global::B.L.B : global::B.L.A))
					{
						P_1.Graphics.FillRectangle(brush, P_1.Bounds);
					}
					TextRenderer.DrawText(P_1.Graphics, comboBox.Items[P_1.Index].ToString(), comboBox.Font, P_1.Bounds, global::B.L.b, TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
				}
			}

			internal bool F(O P_0)
			{
				return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA();
			}

			internal uint f(O P_0)
			{
				return P_0.A;
			}

			internal bool A(KeyValuePair<string, global::b.g> P_0)
			{
				KeyValuePair<string, global::b.g> keyValuePair = P_0;
				return keyValuePair.Value.A < DateTime.UtcNow;
			}

			internal string a(KeyValuePair<string, global::b.g> P_0)
			{
				KeyValuePair<string, global::b.g> keyValuePair = P_0;
				return keyValuePair.Key;
			}

			internal bool A(global::A.T P_0)
			{
				if (P_0.A != null)
				{
					return P_0.A.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IE());
				}
				return false;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public K A;

			public TextBox A;

			internal bool A()
			{
				return this.A.m_C;
			}

			internal void A(bool P_0)
			{
				this.A.m_C = P_0;
				if (!P_0)
				{
					this.A.k();
				}
				this.A.i();
			}

			internal void A(decimal P_0)
			{
				this.A.m_a = (int)P_0;
				this.A.i();
			}

			internal void a(decimal P_0)
			{
				this.A.m_B = (int)P_0;
				this.A.i();
			}

			internal bool a()
			{
				return this.A.m_E;
			}

			internal void a(bool P_0)
			{
				this.A.m_E = P_0;
				this.A.i();
			}

			internal void B(decimal P_0)
			{
				this.A.m_A = P_0;
				this.A.i();
			}

			internal void B()
			{
				this.A.A(this.A.Text);
				this.A.i();
			}

			internal void b()
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xb()
				};
				if (openFileDialog.ShowDialog(this.A) == DialogResult.OK)
				{
					using (new global::A.q(openFileDialog.FileName))
					{
					}
					this.A.m_a = openFileDialog.FileName;
					this.A.i();
					this.A.m_a.Text = Path.GetFileName(openFileDialog.FileName);
				}
			}

			internal void C()
			{
				this.A.m_a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				this.A.i();
				this.A.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xC() + Path.GetFileName(global::A.W.A);
			}

			internal void A(object P_0, EventArgs P_1)
			{
				this.A.j();
			}

			internal void a(object P_0, EventArgs P_1)
			{
				if (global::a.O.A)
				{
					try
					{
						this.A.A(this.A.m_A);
					}
					catch (Exception ex)
					{
						this.A.m_a.Text = ex.Message;
					}
					this.A.m_B.Start();
				}
			}
		}

		[CompilerGenerated]
		private sealed class b
		{
			public A A;

			public global::a.w A;

			public Point A;

			internal A<global::b.A, double> A(global::b.A P_0)
			{
				PointF pointF;
				bool flag = this.A.A(P_0.A, P_0.a + 0.8f, P_0.B, this.A.B, this.A.b, out pointF);
				return new A<global::b.A, double>(P_0, flag ? ((double)((pointF.X - (float)this.A.X) * (pointF.X - (float)this.A.X) + (pointF.Y - (float)this.A.Y) * (pointF.Y - (float)this.A.Y))) : double.MaxValue);
			}
		}

		[CompilerGenerated]
		private sealed class C
		{
			public long A;

			public string A;

			public K A;

			internal void A(object P_0)
			{
				try
				{
					if (!global::a.O.A || global::a.O.a != this.A)
					{
						return;
					}
					if (!string.IsNullOrWhiteSpace(this.A))
					{
						using (global::A.q q2 = new global::A.q(this.A))
						{
							q2.A(this.A);
							return;
						}
					}
					SystemSounds.Exclamation.Play();
				}
				catch (Exception)
				{
					try
					{
						this.A.BeginInvoke((Action)delegate
						{
							this.A.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xc();
						});
					}
					catch
					{
					}
				}
			}

			internal void A()
			{
				this.A.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xc();
			}
		}

		[CompilerGenerated]
		private sealed class c
		{
			public Action A;

			public K A;

			internal void A(object P_0, EventArgs P_1)
			{
				try
				{
					this.A();
				}
				catch (Exception ex)
				{
					this.A.m_A.Text = ex.Message;
				}
			}
		}

		[CompilerGenerated]
		private sealed class D
		{
			public K A;

			public Action<bool> A;

			public global::b.j A;

			internal void A(object P_0, EventArgs P_1)
			{
				if (!this.A.m_e)
				{
					this.A(this.A.A);
					this.A.f();
					this.A.p();
				}
			}
		}

		[CompilerGenerated]
		private sealed class d
		{
			public Action<decimal> A;

			public K A;

			internal void A(decimal P_0)
			{
				this.A(P_0);
				this.A.f();
			}
		}

		[CompilerGenerated]
		private sealed class E
		{
			public Func<Color> A;

			public K A;

			public Action<Color> A;

			public global::A.D A;

			internal void A(object P_0, EventArgs P_1)
			{
				using ColorDialog colorDialog = new ColorDialog
				{
					Color = this.A(),
					FullOpen = true
				};
				if (colorDialog.ShowDialog(this.A) == DialogResult.OK)
				{
					this.A(colorDialog.Color);
					this.A.Invalidate();
					this.A.f();
				}
			}
		}

		[CompilerGenerated]
		private sealed class e
		{
			public global::b.j A;

			public global::a.E A;

			internal void A(object P_0, EventArgs P_1)
			{
				this.A.Width = this.A.A.Width - 50;
			}

			internal void a(object P_0, EventArgs P_1)
			{
				this.A.A = !this.A.A;
			}
		}

		[CompilerGenerated]
		private sealed class F
		{
			public int A;

			public K A;

			internal bool A()
			{
				return this.A.m_A.A[this.A];
			}

			internal void A(bool P_0)
			{
				this.A.m_A.A[this.A] = P_0;
			}
		}

		[CompilerGenerated]
		private sealed class f
		{
			public Y A;

			internal bool A()
			{
				return this.A.A;
			}

			internal void A(bool P_0)
			{
				this.A.A = P_0;
			}

			internal Color a()
			{
				return this.A.A;
			}

			internal void A(Color P_0)
			{
				this.A.A = P_0;
			}

			internal Color B()
			{
				return this.A.a;
			}

			internal void a(Color P_0)
			{
				this.A.a = P_0;
			}

			internal Color b()
			{
				return this.A.B;
			}

			internal void B(Color P_0)
			{
				this.A.B = P_0;
			}

			internal void A(decimal P_0)
			{
				this.A.A = P_0;
			}

			internal void a(decimal P_0)
			{
				this.A.a = P_0;
			}

			internal void B(decimal P_0)
			{
				this.A.B = P_0;
			}
		}

		[CompilerGenerated]
		private sealed class G
		{
			public global::b.c A;

			public K A;

			internal void A(decimal P_0)
			{
				this.A.c = (int)P_0;
			}

			internal void a(decimal P_0)
			{
				this.A.D = (int)P_0;
			}

			internal void B(decimal P_0)
			{
				this.A.C = P_0;
			}

			internal void b(decimal P_0)
			{
				this.A.d = (int)P_0;
			}

			internal bool A()
			{
				return this.A.a;
			}

			internal void A(bool P_0)
			{
				this.A.a = P_0;
			}

			internal void C(decimal P_0)
			{
				this.A.A = (int)P_0;
			}

			internal void c(decimal P_0)
			{
				this.A.A = P_0;
			}

			internal void D(decimal P_0)
			{
				this.A.a = P_0;
			}

			internal void d(decimal P_0)
			{
				this.A.B = P_0;
			}

			internal bool a()
			{
				return this.A.C;
			}

			internal void a(bool P_0)
			{
				this.A.C = P_0;
			}

			internal bool B()
			{
				return this.A.d;
			}

			internal void B(bool P_0)
			{
				this.A.d = P_0;
			}

			internal bool b()
			{
				return this.A.c;
			}

			internal void b(bool P_0)
			{
				this.A.c = P_0;
			}

			internal bool C()
			{
				return this.A.D;
			}

			internal void C(bool P_0)
			{
				this.A.D = P_0;
			}

			internal bool c()
			{
				return this.A.E;
			}

			internal void c(bool P_0)
			{
				this.A.E = P_0;
			}

			internal void E(decimal P_0)
			{
				this.A.a = (int)P_0;
			}

			internal void e(decimal P_0)
			{
				this.A.B = (int)P_0;
			}

			internal void F(decimal P_0)
			{
				this.A.b = (int)P_0;
			}

			internal void f(decimal P_0)
			{
				this.A.b = P_0;
			}

			internal void G(decimal P_0)
			{
				this.A.C = (int)P_0;
			}

			internal bool D()
			{
				return this.A.m_A.a;
			}

			internal void D(bool P_0)
			{
				this.A.m_A.a = P_0;
			}

			internal Color d()
			{
				return this.A.m_A.A;
			}

			internal void A(Color P_0)
			{
				this.A.m_A.A = P_0;
			}

			internal Color E()
			{
				return this.A.m_A.a;
			}

			internal void a(Color P_0)
			{
				this.A.m_A.a = P_0;
			}

			internal void g(decimal P_0)
			{
				this.A.E = (int)P_0;
			}

			internal void H(decimal P_0)
			{
				this.A.e = (int)P_0;
			}

			internal void e()
			{
				this.A.b = !this.A.b;
				this.A.f();
				this.A.m_A.Text = (this.A.b ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xd() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xD());
			}
		}

		[CompilerGenerated]
		private sealed class g
		{
			public K A;

			public t A;

			public global::A.j A;

			internal bool A()
			{
				return this.A.m_A.B;
			}

			internal void A(bool P_0)
			{
				this.A.m_A.B = P_0;
			}

			internal void A(decimal P_0)
			{
				this.A.m_A.A = (int)P_0;
			}

			internal bool a()
			{
				return this.A.B;
			}

			internal void a(bool P_0)
			{
				this.A.B = P_0;
			}

			internal bool B()
			{
				return this.A.A;
			}

			internal void B(bool P_0)
			{
				this.A.A = P_0;
			}

			internal bool b()
			{
				return this.A.a;
			}

			internal void b(bool P_0)
			{
				this.A.a = P_0;
			}

			internal bool C()
			{
				return this.A.b;
			}

			internal void C(bool P_0)
			{
				this.A.b = P_0;
			}

			internal void a(decimal P_0)
			{
				this.A.A = (int)P_0;
			}

			internal void B(decimal P_0)
			{
				this.A.a = (int)P_0;
			}

			internal void b(decimal P_0)
			{
				this.A.A = P_0;
			}

			internal bool c()
			{
				return this.A.A;
			}

			internal void c(bool P_0)
			{
				this.A.A = P_0;
			}

			internal bool D()
			{
				return this.A.a;
			}

			internal void D(bool P_0)
			{
				this.A.a = P_0;
			}

			internal void C(decimal P_0)
			{
				this.A.A = (int)P_0;
			}

			internal void c(decimal P_0)
			{
				this.A.B = (int)P_0;
			}

			internal void D(decimal P_0)
			{
				this.A.A = P_0;
			}

			internal void d(decimal P_0)
			{
				this.A.b = (int)P_0;
			}

			internal void E(decimal P_0)
			{
				this.A.a = (int)P_0;
			}

			internal void d()
			{
				this.A.m_A.C();
				this.A.m_A.b();
				this.A.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xE();
			}
		}

		[CompilerGenerated]
		private sealed class H
		{
			public K A;

			public global::B.C A;

			internal bool A()
			{
				return this.A.m_A.A.A;
			}

			internal void A(bool P_0)
			{
				this.A.m_A.A.A = P_0;
			}

			internal void A(decimal P_0)
			{
				this.A.m_A.A.A = (int)P_0;
			}

			internal bool a()
			{
				return this.A.B;
			}

			internal void a(bool P_0)
			{
				this.A.B = P_0;
			}

			internal bool B()
			{
				return this.A.b;
			}

			internal void B(bool P_0)
			{
				this.A.b = P_0;
			}

			internal bool b()
			{
				return this.A.m_A.A.A.B;
			}

			internal void b(bool P_0)
			{
				this.A.m_A.A.A.B = P_0;
			}

			internal void a(decimal P_0)
			{
				this.A.b = (int)P_0;
			}

			internal void B(decimal P_0)
			{
				this.A.a = (int)P_0;
			}

			internal void b(decimal P_0)
			{
				this.A.B = (int)P_0;
			}

			internal void C(decimal P_0)
			{
				this.A.A = P_0;
			}

			internal void c(decimal P_0)
			{
				this.A.C = (int)P_0;
			}

			internal void D(decimal P_0)
			{
				this.A.c = (int)P_0;
			}
		}

		[CompilerGenerated]
		private sealed class h
		{
			public string A;

			public global::A.l A;

			public global::A.D A;

			public K A;

			internal void A()
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Title = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xe() + this.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hr(),
					Filter = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xF()
				};
				if (openFileDialog.ShowDialog(this.A) == DialogResult.OK)
				{
					this.A.m_A.A(this.A, openFileDialog.FileName);
					this.A.m_A.Text = this.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gM() + this.A.m_A.a(this.A);
					this.A.p();
				}
			}

			internal void a()
			{
				this.A.m_A.A(this.A, true);
			}

			internal void B()
			{
				this.A.m_A.A(this.A, null);
				this.A.m_A.SetToolTip(this.A, this.A.m_A.a(this.A));
				this.A.m_A.Text = this.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gM() + this.A.m_A.a(this.A);
			}
		}

		[CompilerGenerated]
		private sealed class I
		{
			public O A;

			public Func<global::b.g, bool> A;

			internal bool A(global::b.g P_0)
			{
				return P_0.A == this.A.A;
			}
		}

		private readonly global::A.o m_A = new global::A.o();

		private readonly global::a.A m_A = new global::a.A();

		private readonly global::A.N m_A = new global::A.N();

		private readonly global::A.M m_A = new global::A.M();

		private readonly global::a.A m_a = new global::a.A();

		private readonly CheckedListBox m_A = new CheckedListBox();

		private bool m_A;

		private readonly J m_A = global::B.J.A();

		private readonly global::A.z m_A;

		private readonly global::a.X m_A;

		private readonly global::b.B m_A;

		private readonly global::B.a m_A;

		private readonly global::A.I m_A;

		private readonly System.Windows.Forms.Timer m_A = new System.Windows.Forms.Timer();

		private readonly System.Windows.Forms.Timer m_a = new System.Windows.Forms.Timer();

		private readonly Label m_A = new Label();

		private readonly Panel m_A = new P();

		private readonly NotifyIcon m_A = new NotifyIcon();

		private n m_A;

		private int m_A;

		private DateTime m_A = DateTime.MinValue;

		private List<O> m_A = new List<O>();

		private bool m_a;

		private readonly ComboBox m_A = new ComboBox();

		private readonly ComboBox m_a = new ComboBox();

		private readonly bool m_B;

		private uint m_A;

		private uint m_a;

		private bool m_b;

		private readonly System.Windows.Forms.Timer m_B = new System.Windows.Forms.Timer();

		private global::a.C m_A;

		private bool m_C;

		private bool m_c;

		private bool m_D;

		private bool m_d;

		private bool m_E = true;

		private decimal m_A = 2m;

		private int m_a = 40;

		private int m_B = 15;

		private string m_A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fq();

		private string m_a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		private string m_B;

		private DateTime m_a;

		private DateTime m_B;

		private DateTime m_b;

		private float m_A;

		private float m_a;

		private float m_B;

		private long m_A;

		private long m_a;

		private List<global::b.A> m_A = new List<global::b.A>();

		private global::a.D m_A;

		private Label m_a;

		private uint m_B;

		private uint m_b;

		private string m_b = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		private readonly w m_A = new w();

		private readonly List<KeyValuePair<global::b.j, Func<bool>>> m_A = new List<KeyValuePair<global::b.j, Func<bool>>>();

		private readonly global::b.e[] m_A = new global::b.e[3];

		private readonly CheckedListBox m_a = new CheckedListBox();

		private bool m_e;

		private bool m_F;

		private Label m_B;

		private ToolTip m_A = new ToolTip();

		private readonly System.Windows.Forms.Timer m_b = new System.Windows.Forms.Timer();

		private readonly Dictionary<string, global::b.g> m_A = new Dictionary<string, global::b.g>();

		private readonly HashSet<string> m_A = new HashSet<string>();

		private string m_C;

		private bool m_f;

		[CompilerGenerated]
		private Action m_A;

		[CompilerGenerated]
		private Action m_a;

		[CompilerGenerated]
		private Action m_B;

		[CompilerGenerated]
		private Action m_b;

		public Action A
		{
			[CompilerGenerated]
			get
			{
				return this.m_A;
			}
			[CompilerGenerated]
			set
			{
				this.m_A = action;
			}
		}

		public Action a
		{
			[CompilerGenerated]
			get
			{
				return this.m_a;
			}
			[CompilerGenerated]
			set
			{
				this.m_a = action;
			}
		}

		public Action B
		{
			[CompilerGenerated]
			get
			{
				return this.m_B;
			}
			[CompilerGenerated]
			set
			{
				this.m_B = action;
			}
		}

		private string b => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fp());

		private string C => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fQ());

		[SpecialName]
		[CompilerGenerated]
		public void b(Action P_0)
		{
			Action action = this.m_b;
			Action action2;
			do
			{
				action2 = action;
				Action value = (Action)Delegate.Combine(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_b, value, action2);
			}
			while ((object)action != action2);
		}

		[SpecialName]
		[CompilerGenerated]
		public void C(Action P_0)
		{
			Action action = this.m_b;
			Action action2;
			do
			{
				action2 = action;
				Action value = (Action)Delegate.Remove(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_b, value, action2);
			}
			while ((object)action != action2);
		}

		public K(bool P_0 = false)
		{
			this.m_B = P_0;
			Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fR();
			base.ClientSize = new Size(Math.Min(1480, Screen.PrimaryScreen.WorkingArea.Width - 30), Math.Min(950, Screen.PrimaryScreen.WorkingArea.Height - 30));
			MinimumSize = new Size(1180, 740);
			Font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), 10f);
			base.StartPosition = FormStartPosition.CenterScreen;
			this.m_A = new global::A.z(this.m_A, !P_0);
			this.m_A = new global::a.X(this.m_A, !P_0);
			this.m_A = new global::b.B(this.m_A.A, !P_0);
			this.m_A = new global::B.a(this.m_A.A, !P_0, this.m_A.A);
			this.m_A = new global::A.I(this.m_A.A, !P_0, this.m_A.A);
			n();
			this.m_A.A = m;
			this.m_A.Icon = global::a.J.A;
			this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cM();
			this.m_A.Visible = !P_0;
			NotifyIcon notifyIcon = this.m_A;
			EventHandler value = delegate
			{
				e();
			};
			notifyIcon.DoubleClick += value;
			ContextMenuStrip contextMenuStrip = new ContextMenuStrip
			{
				Items = 
				{
					{
						CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fr(),
						(Image)null,
						(EventHandler)delegate
						{
							e();
						}
					},
					{
						CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fS(),
						(Image)null,
						(EventHandler)delegate
						{
							if (this.B != null)
							{
								this.B();
							}
							else
							{
								Close();
							}
						}
					}
				}
			};
			this.m_A.ContextMenuStrip = contextMenuStrip;
			base.Resize += delegate
			{
				if (base.WindowState == FormWindowState.Minimized)
				{
					Hide();
				}
			};
			this.m_a.Interval = 50;
			this.m_a.Tick += delegate
			{
				if (base.Visible)
				{
					this.m_A.Invalidate();
				}
			};
			this.m_a.Start();
			this.m_A.Interval = 1000;
			this.m_A.Tick += delegate
			{
				h();
			};
			if (!P_0)
			{
				base.Shown += delegate
				{
					c();
				};
			}
			else
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fs();
			}
			if (P_0)
			{
				O o2 = new O
				{
					A = 1u,
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fT(),
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA(),
					A = 450f,
					B = 120f,
					a = 80f,
					C = 52f,
					b = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fw(),
					d = true
				};
				o2.A.Add(new global::b.K
				{
					A = 1,
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ft(),
					A = true
				});
				o2.A.Add(new global::b.K
				{
					A = 2,
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fU(),
					A = false
				});
				o2.A.AddRange(new string[3]
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bC(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FY(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fy()
				});
				O o3 = new O
				{
					A = 2u,
					A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fu(),
					a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA(),
					A = 360f,
					B = 90f,
					a = 50f
				};
				o3.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fV());
				this.m_A = new List<O> { o2, o3 };
				this.m_a = true;
				this.m_a.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fv());
				this.m_a.A(o3, false);
				g();
			}
		}

		public void c()
		{
			if (!this.m_B && global::a.O.A)
			{
				h();
				this.m_A.Start();
				if (!this.m_b)
				{
					d();
				}
			}
		}

		private void D()
		{
			try
			{
				string[] array = File.ReadAllLines(this.b);
				if (array.Length == 2 && int.TryParse(array[0], out var result) && result >= 0 && result < 5 && this.m_A.Items.Contains(array[1]))
				{
					this.m_a.SelectedIndex = result;
					this.m_A.SelectedItem = array[1];
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		private void d()
		{
			if (this.m_B || !global::a.O.A)
			{
				return;
			}
			uint num = (uint)(Keys)Enum.Parse(typeof(Keys), this.m_A.Text);
			uint num2 = (new uint[5] { 0u, 2u, 1u, 4u, 3u })[this.m_a.SelectedIndex] | 0x4000;
			if (this.m_b)
			{
				global::B.H.A(base.Handle, 201);
			}
			if (!global::B.H.A(base.Handle, 201, num2, num))
			{
				this.m_b = this.m_A != 0 && global::B.H.A(base.Handle, 201, this.m_a, this.m_A);
				MessageBox.Show(this, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fW(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fw());
				return;
			}
			this.m_b = true;
			this.m_A = num;
			this.m_a = num2;
			try
			{
				File.WriteAllLines(this.b, new string[2]
				{
					this.m_a.SelectedIndex.ToString(),
					this.m_A.Text
				});
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fX() + ((this.m_a.SelectedIndex == 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D() : (this.m_a.Text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fx())) + this.m_A.Text;
			}
			catch (Exception ex)
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fY() + ex.Message;
			}
		}

		private void E()
		{
			if (this.m_B || !global::a.O.A)
			{
				return;
			}
			h();
			if (!this.m_a)
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fy();
				return;
			}
			string text = string.Join(Environment.NewLine, global::B.k.B(this.m_a.A(this.m_A), this.m_A));
			if (text.Length == 0)
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fZ();
				return;
			}
			try
			{
				Clipboard.SetText(text, TextDataFormat.UnicodeText);
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.fz() + this.m_a.A(this.m_A).Count() + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GA();
			}
			catch (ExternalException)
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ga();
			}
		}

		protected override void WndProc(ref Message P_0)
		{
			if (a(ref P_0) || A(ref P_0))
			{
				return;
			}
			if (P_0.Msg == 786 && P_0.WParam.ToInt32() == 201)
			{
				global::B.H.A(global::B.H.A(), out var num);
				if (num == (uint)this.m_A || num == (uint)Process.GetCurrentProcess().Id)
				{
					E();
				}
			}
			base.WndProc(ref P_0);
		}

		private void e()
		{
			if (this.A != null)
			{
				this.A();
				return;
			}
			Show();
			base.WindowState = FormWindowState.Normal;
			Activate();
		}

		private IEnumerable<O> F()
		{
			return this.m_A.Where((O P_0) => this.m_a.a(P_0) || (this.m_A.c && (P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba() || P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl())));
		}

		private void f()
		{
			if (this.m_a && global::a.O.A)
			{
				this.m_A.A(this.m_A, F());
			}
			this.m_A.a();
			this.m_A.B();
			this.m_A.B();
			this.m_A.a();
			this.m_A.Invalidate();
			p();
			try
			{
				if (!this.m_B)
				{
					this.m_A.a(null);
				}
			}
			catch (Exception ex)
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GB() + ex.Message;
			}
		}

		private void G()
		{
			k();
			this.m_A.C();
			this.m_A.b();
			this.m_A.c();
			this.m_A.A();
			if (this.m_A != null)
			{
				this.m_A.Dispose();
			}
			this.m_A = null;
			this.m_A = 0;
			this.m_A.A();
			this.m_A.A();
			this.m_A.B();
			this.m_A.Clear();
			Q();
			this.m_a = false;
			P();
			this.m_A = true;
			this.m_A.Items.Clear();
			this.m_A = false;
			this.m_A.Invalidate();
		}

		private void g()
		{
			global::A.Z[] array = (from P_0 in this.m_A
				where P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
				orderby P_0.A
				select new global::A.Z
				{
					A = P_0
				}).ToArray();
			this.m_A = true;
			try
			{
				bool flag = this.m_A.Items.Count == array.Length;
				int num = 0;
				while (flag && num < array.Length)
				{
					flag = ((global::A.Z)this.m_A.Items[num]).ToString() == array[num].ToString();
					num++;
				}
				if (!flag)
				{
					this.m_A.Items.Clear();
					global::A.Z[] array2 = array;
					foreach (global::A.Z z2 in array2)
					{
						this.m_A.Items.Add(z2, this.m_a.a(z2.A));
					}
				}
				else
				{
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						((global::A.Z)this.m_A.Items[num3]).A = array[num3].A;
						this.m_A.SetItemChecked(num3, this.m_a.a(array[num3].A));
					}
				}
			}
			finally
			{
				this.m_A = false;
			}
			Q();
		}

		private void H()
		{
			if (this.m_a && global::a.O.A)
			{
				this.m_A.A(this.m_A, F());
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gb() + this.m_a.A(this.m_A).Count() + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cE() + this.m_A.Count((O P_0) => P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GC();
			}
			this.m_A.Invalidate();
		}

		private void A(bool P_0)
		{
			foreach (O item in this.m_A.Where((O o2) => o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
			{
				this.m_a.A(item, P_0);
			}
			g();
			H();
		}

		private void h()
		{
			if (!global::a.O.A)
			{
				M();
				return;
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				if (this.m_A == null)
				{
					if (DateTime.UtcNow < this.m_A)
					{
						return;
					}
					this.m_A = DateTime.UtcNow.AddSeconds(1.0);
					Process[] processesByName = Process.GetProcessesByName(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gc());
					try
					{
						if (processesByName.Length != 1)
						{
							throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GD());
						}
						this.m_A = processesByName[0].Id;
					}
					finally
					{
						Process[] array = processesByName;
						for (int num = 0; num < array.Length; num++)
						{
							array[num].Dispose();
						}
					}
					this.m_A = new n();
				}
				this.m_A = this.m_A.k();
				this.m_A.A();
				this.m_A.Interval = 1000;
				try
				{
					J();
				}
				catch (Exception ex)
				{
					if (this.m_A != null)
					{
						this.m_A.Hide();
					}
					if (this.m_a != null)
					{
						this.m_a.Text = ex.Message;
					}
				}
				A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gd(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GE(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ge(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GF(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				this.m_a.A(this.m_A.C);
				this.m_A.A(this.m_A.C);
				this.m_a = true;
				P();
				g();
				H();
				foreach (global::a.e item in this.m_A.A(this.m_A.C, this.m_A, this.m_a))
				{
					this.m_A.A(item.A, false);
					this.m_A.Text = item.A;
				}
				foreach (global::a.e item2 in this.m_A.A(this.m_A.C, this.m_A, this.m_A))
				{
					this.m_A.A(item2.A, false);
					this.m_A.Text = item2.A;
				}
				if (this.m_A.A.Length > 0)
				{
					this.m_A.Text = this.m_A.A;
				}
				if (this.m_A.A.Length > 0)
				{
					this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gf() + this.m_A.A;
				}
				if (this.m_A.A.Length > 0)
				{
					this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GG() + this.m_A.A;
				}
				if (this.m_A.A.a && this.m_A.B.Length > 0)
				{
					this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gg() + this.m_A.B;
				}
				if (this.m_A.A.Length > 0)
				{
					this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GH() + this.m_A.A;
				}
				if (this.m_A.A && this.m_A.b.Length > 0)
				{
					this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gh() + this.m_A.b;
				}
			}
			catch (Exception ex2)
			{
				bool flag = this.m_A != null && this.m_a && this.m_A.L();
				if (this.m_A.A(flag, DateTime.UtcNow))
				{
					this.m_A.Interval = 100;
					global::B.I.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GI(), ex2.Message);
					this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gi() + ex2.Message;
				}
				else
				{
					global::B.I.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GJ(), ex2.Message);
					G();
					this.m_A.Interval = 1000;
					this.m_A = DateTime.UtcNow.AddSeconds(1.0);
					this.m_A.Text = ex2.Message + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gj();
				}
			}
			finally
			{
				p();
				if (stopwatch.ElapsedMilliseconds > 1000)
				{
					global::B.I.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GK(), stopwatch.ElapsedMilliseconds + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gk());
				}
			}
		}

		private void A(object P_0, PaintEventArgs P_1)
		{
			string[] array = ((this.m_a && global::a.O.A) ? global::B.k.B(F(), this.m_A) : new string[2]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GL(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gl()
			});
			if (this.m_a && array.Length == 0)
			{
				array = new string[1] { CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GM() };
			}
			using Font font = new Font(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BB(), this.m_A.B, FontStyle.Bold);
			global::B.i.A(P_1.Graphics, array, font, this.m_A.B, this.m_A.b, this.m_A.Width, true, this.m_A.a, this.m_a ? global::B.k.A(F(), this.m_A) : null, this.m_a ? global::B.k.a(F(), this.m_A) : null);
		}

		protected override void OnFormClosed(FormClosedEventArgs P_0)
		{
			if (this.m_b)
			{
				global::B.H.A(base.Handle, 201);
			}
			this.m_a.Dispose();
			this.m_A.Stop();
			this.m_A.Dispose();
			this.m_A.Visible = false;
			this.m_A.Dispose();
			G();
			this.m_A.Dispose();
			this.m_A.Close();
			this.m_A.Dispose();
			this.m_A.Close();
			this.m_A.Dispose();
			this.m_A.D();
			this.m_A.Dispose();
			this.m_A.Close();
			this.m_A.Dispose();
			this.m_A.Dispose();
			base.OnFormClosed(P_0);
		}

		[DllImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
		private static extern short A(int P_0);

		[DllImport("user32.dll", EntryPoint = "GetClientRect")]
		private static extern bool A(IntPtr P_0, out A P_1);

		[DllImport("user32.dll", EntryPoint = "ScreenToClient")]
		private static extern bool A(IntPtr P_0, ref Point P_1);

		[DllImport("user32.dll", EntryPoint = "ClientToScreen")]
		private static extern bool a(IntPtr P_0, ref Point P_1);

		private void I()
		{
			try
			{
				string[] array = File.ReadAllLines(this.C);
				foreach (string text in array)
				{
					int num2 = text.IndexOf('=');
					if (num2 >= 0)
					{
						string text2 = text.Substring(0, num2);
						string text3 = text.Substring(num2 + 1);
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gm())
						{
							this.m_E = text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bw();
						}
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GN() && decimal.TryParse(text3, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
						{
							this.m_A = Math.Max(0.5m, Math.Min(12m, result));
						}
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BW())
						{
							this.m_C = text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bw();
						}
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gn() && int.TryParse(text3, out var result2))
						{
							this.m_a = Math.Max(5, Math.Min(500, result2));
						}
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GO() && int.TryParse(text3, out result2))
						{
							this.m_B = Math.Max(3, Math.Min(120, result2));
						}
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Go())
						{
							this.m_A = text3;
						}
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GP())
						{
							this.m_a = text3;
						}
					}
				}
			}
			catch (IOException)
			{
			}
			global::B.E e2 = this.A(2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gp(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GQ());
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gq(), () => this.m_C, delegate(bool P_0)
			{
				this.m_C = P_0;
				if (!P_0)
				{
					k();
				}
				i();
			}, 146);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GR(), this.m_a, 5m, 500m, delegate(decimal P_0)
			{
				this.m_a = (int)P_0;
				i();
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gr(), this.m_B, 3m, 120m, delegate(decimal P_0)
			{
				this.m_B = (int)P_0;
				i();
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GS(), () => this.m_E, delegate(bool P_0)
			{
				this.m_E = P_0;
				i();
			}, 146);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gs(), this.m_A, 0.5m, 12m, delegate(decimal P_0)
			{
				this.m_A = P_0;
				i();
			}, 1);
			TextBox A = new TextBox
			{
				Text = this.m_A,
				Width = 150,
				BackColor = global::B.L.A,
				ForeColor = global::B.L.b,
				BorderStyle = BorderStyle.FixedSingle
			};
			e2.A.Controls.Add(A);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GT(), delegate
			{
				this.A(A.Text);
				i();
			}, 150);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gt(), delegate
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xb()
				};
				if (openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					using (new global::A.q(openFileDialog.FileName))
					{
					}
					this.m_a = openFileDialog.FileName;
					i();
					this.m_a.Text = Path.GetFileName(openFileDialog.FileName);
				}
			}, 145);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GU(), K, 65);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gu(), delegate
			{
				this.m_a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				i();
				this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xC() + Path.GetFileName(global::A.W.A);
			}, 100);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GV(), k, 145);
			this.m_a = this.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gv(), 390, global::B.L.C);
			this.A(e2.A, this.m_a, 48);
			this.m_A.SetToolTip(A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GW());
			this.m_A = new global::a.C();
			this.m_B.Interval = 50;
			this.m_B.Tick += delegate
			{
				j();
			};
			if (this.m_B)
			{
				return;
			}
			base.Shown += delegate
			{
				if (global::a.O.A)
				{
					try
					{
						this.A(this.m_A);
					}
					catch (Exception ex2)
					{
						this.m_a.Text = ex2.Message;
					}
					this.m_B.Start();
				}
			};
		}

		private void i()
		{
			if (this.m_B)
			{
				return;
			}
			try
			{
				File.WriteAllLines(this.C, new string[7]
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gw() + (this.m_E ? 1 : 0),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GX() + this.m_A.ToString(CultureInfo.InvariantCulture),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BY() + (this.m_C ? 1 : 0),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gx() + this.m_a,
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GY() + this.m_B,
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gy() + this.m_A,
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GZ() + this.m_a
				});
			}
			catch (Exception ex)
			{
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gz() + ex.Message;
			}
		}

		private void A(string P_0)
		{
			string[] array = P_0.Split('+');
			uint num = 16384u;
			foreach (string item in array.Take(array.Length - 1))
			{
				if (item.Trim().Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gA(), StringComparison.OrdinalIgnoreCase))
				{
					num |= 2;
					continue;
				}
				if (item.Trim().Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ga(), StringComparison.OrdinalIgnoreCase))
				{
					num |= 1;
					continue;
				}
				if (item.Trim().Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gB(), StringComparison.OrdinalIgnoreCase))
				{
					num |= 4;
					continue;
				}
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gb());
			}
			if (!Enum.TryParse<Keys>(array.Last().Trim(), ignoreCase: true, out var result) || result == Keys.None || result > (Keys.F16 | Keys.F17))
			{
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gC());
			}
			if (this.m_B || !global::a.O.A)
			{
				return;
			}
			uint num2 = this.m_B;
			uint num3 = this.m_b;
			global::B.H.A(base.Handle, 203);
			if (!global::B.H.A(base.Handle, 203, num, (uint)result))
			{
				if (num3 != 0)
				{
					global::B.H.A(base.Handle, 203, num2, num3);
				}
				throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gc());
			}
			this.m_d = true;
			this.m_B = num;
			this.m_b = (uint)result;
			this.m_A = P_0;
		}

		private bool A(ref Message P_0)
		{
			if (P_0.Msg != 786 || P_0.WParam.ToInt32() != 203)
			{
				return false;
			}
			global::B.H.A(global::B.H.A(), out var num);
			if (global::a.O.A && num == (uint)this.m_A && this.m_C)
			{
				this.m_a = DateTime.UtcNow.AddSeconds(5.0);
				this.m_D = (A(1) & 0x8000) != 0;
				this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gD();
			}
			return true;
		}

		private void J()
		{
			if (this.m_A == null || !this.m_C)
			{
				return;
			}
			if (this.m_B != this.m_A.C)
			{
				k();
				this.m_B = this.m_A.C;
			}
			foreach (O item in this.m_A)
			{
				foreach (global::b.d item2 in item.A)
				{
					item2.A = true;
					item2.A = (item.b ? item.A : Color.White);
				}
			}
			this.m_A.D = true;
			try
			{
				this.m_A = this.m_A.A(this.m_A.C, this.m_A.Where((O P_0) => P_0.C).SelectMany((O P_0) => P_0.A));
			}
			finally
			{
				this.m_A.D = false;
			}
			this.m_B = DateTime.UtcNow;
			if (!this.m_c)
			{
				return;
			}
			if (this.m_A.Any((O P_0) => !P_0.C))
			{
				this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gd();
				return;
			}
			if (this.m_A != 0L)
			{
				global::b.A a2 = this.m_A.FirstOrDefault((global::b.A P_0) => P_0.A == this.m_A && P_0.a == this.m_a);
				if (a2 == null)
				{
					k();
					this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gE();
					return;
				}
				this.m_A = a2.A;
				this.m_a = a2.a;
				this.m_B = a2.B;
			}
			this.m_A = global::a.a.A(this.m_A, this.m_A, this.m_A, this.m_B, this.m_a);
			global::a.D d2 = this.m_A;
			this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ge() + d2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gF() + d2.a + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gf() + d2.B + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gF() + d2.b;
			if (d2.A && DateTime.UtcNow >= this.m_b)
			{
				K();
				this.m_b = DateTime.UtcNow.AddSeconds(this.m_B);
			}
			if (!d2.A)
			{
				this.m_b = DateTime.MinValue;
			}
		}

		private void j()
		{
			if (!global::a.O.A || this.m_A == null || !this.m_C || (!this.m_c && this.m_a <= DateTime.UtcNow))
			{
				this.m_A.Hide();
				return;
			}
			IntPtr intPtr = global::B.H.A();
			global::B.H.A(intPtr, out var num);
			if (num != (uint)this.m_A || !global::B.K.A(intPtr, out var A) || A.B < 200)
			{
				this.m_A.Hide();
				return;
			}
			bool flag = (global::B.K.A(1) & 0x8000) != 0;
			try
			{
				if (this.m_a > DateTime.UtcNow && (global::B.K.A(27) & 0x8000) != 0)
				{
					this.m_a = DateTime.MinValue;
				}
				global::a.w A2 = this.m_A.F();
				if (this.m_a > DateTime.UtcNow && flag && !this.m_D)
				{
					this.m_a = DateTime.MinValue;
					Point A3 = Cursor.Position;
					global::B.K.A(intPtr, ref A3);
					if (A3.X < 0 || A3.Y < 0 || A3.X >= A.B || (double)A3.Y >= (double)A.b * 0.78 || DateTime.UtcNow - this.m_B > TimeSpan.FromSeconds(3.0))
					{
						throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gG());
					}
					A<global::b.A, double> a2 = (from P_0 in this.m_A.Select(delegate(global::b.A P_0)
						{
							PointF pointF;
							bool flag2 = A2.A(P_0.A, P_0.a + 0.8f, P_0.B, A.B, A.b, out pointF);
							return new A<global::b.A, double>(P_0, flag2 ? ((double)((pointF.X - (float)A3.X) * (pointF.X - (float)A3.X) + (pointF.Y - (float)A3.Y) * (pointF.Y - (float)A3.Y))) : double.MaxValue);
						})
						orderby P_0.a
						select P_0).ToArray().FirstOrDefault();
					if (a2 != null && a2.a < 1936.0)
					{
						global::b.A a3 = a2.A;
						this.m_A = a3.A;
						this.m_a = a3.a;
						this.m_B = a3.B;
						this.m_A = a3.A;
						this.m_a = a3.a;
						this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gg() + a3.a;
					}
					else
					{
						this.m_a = ((a2 != null && a2.a < double.MaxValue) ? a2.A.a : 0f);
						if (!global::a.a.A(A2, A3, A.B, A.b, this.m_a, out this.m_A, out this.m_B))
						{
							throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gH());
						}
						this.m_A = 0L;
						this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gh();
					}
					this.m_c = true;
					this.m_b = DateTime.MinValue;
					this.m_A = null;
				}
				if (global::b.J.A(this.m_A, this.m_A.C))
				{
					this.m_A.Hide();
					return;
				}
				if (!this.m_c || DateTime.UtcNow - this.m_B > TimeSpan.FromSeconds(3.0))
				{
					this.m_A.Hide();
					return;
				}
				Point empty = Point.Empty;
				a(intPtr, ref empty);
				this.m_A.Bounds = new Rectangle(empty.X, empty.Y, A.B, A.b);
				this.m_A.A = A2;
				this.m_A.A = this.m_A;
				this.m_A.a = this.m_a;
				this.m_A.B = this.m_B;
				this.m_A.A = this.m_a;
				this.m_A.a = this.m_E;
				this.m_A.b = (float)this.m_A;
				this.m_A.A = global::a.c.A(this.m_A);
				this.m_A.A = this.m_A != null && this.m_A.A;
				this.m_A.A = (this.m_A.A ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gI() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DM()) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gi() + this.m_a;
				if (!this.m_A.Visible)
				{
					this.m_A.Show();
				}
				this.m_A.Invalidate();
			}
			catch (Exception ex)
			{
				this.m_A.Hide();
				this.m_a.Text = ex.Message;
			}
			finally
			{
				this.m_D = flag;
			}
		}

		private void K()
		{
			if (this.m_B || !global::a.O.A)
			{
				return;
			}
			long A = global::a.O.a;
			string A2 = (string.IsNullOrWhiteSpace(this.m_a) ? global::A.W.A : this.m_a);
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					if (global::a.O.A && global::a.O.a == A)
					{
						if (!string.IsNullOrWhiteSpace(A2))
						{
							using (global::A.q q2 = new global::A.q(A2))
							{
								q2.A(A);
								return;
							}
						}
						SystemSounds.Exclamation.Play();
					}
				}
				catch (Exception)
				{
					try
					{
						BeginInvoke((Action)delegate
						{
							this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xc();
						});
					}
					catch
					{
					}
				}
			});
		}

		private void k()
		{
			this.m_c = false;
			this.m_a = DateTime.MinValue;
			this.m_A.Clear();
			this.m_A = null;
			if (this.m_A != null)
			{
				this.m_A.Hide();
			}
			if (this.m_a != null)
			{
				this.m_a.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gJ();
			}
		}

		private void L()
		{
			this.m_B.Stop();
			this.m_B.Dispose();
			if (this.m_d)
			{
				global::B.H.A(base.Handle, 203);
			}
			if (this.m_A != null)
			{
				this.m_A.Dispose();
				this.m_A = null;
			}
		}

		public void A(long P_0, string P_1 = "")
		{
			this.m_b = a(P_0, P_1);
		}

		internal static string a(long P_0, string P_1)
		{
			if (P_1 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dR())
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gj();
			}
			try
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gK() + new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(P_0).ToLocalTime().ToString(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EN());
			}
			catch (ArgumentOutOfRangeException)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			}
		}

		public void l()
		{
			if (this.m_B || !global::a.O.A)
			{
				return;
			}
			this.m_b.Start();
			this.m_B.Start();
			if (!this.m_d)
			{
				try
				{
					A(this.m_A);
				}
				catch
				{
				}
			}
			if (!this.m_A.Enabled)
			{
				c();
			}
		}

		public void M()
		{
			if (!this.m_B)
			{
				this.m_A.Stop();
				this.m_b.Stop();
				this.m_B.Stop();
				if (this.m_b)
				{
					global::B.H.A(base.Handle, 201);
					this.m_b = false;
				}
				if (this.m_d)
				{
					global::B.H.A(base.Handle, 203);
					this.m_d = false;
				}
				this.m_A.b();
				G();
				global::A.q.A();
			}
		}

		private void A(Control P_0)
		{
			A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gk(), delegate
			{
				this.m_b?.Invoke();
			}, 150);
			A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gL(), delegate
			{
				using global::a.P p2 = new global::a.P(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ie(), string.IsNullOrEmpty(this.m_b) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IF() : this.m_b, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dy());
				p2.ShowDialog(this);
			}, 150);
		}

		private global::a.b m()
		{
			if (!this.m_C || !this.m_c || !this.m_E || this.m_A == null || this.m_B != this.m_A.C || DateTime.UtcNow - this.m_B > TimeSpan.FromSeconds(3.0))
			{
				return null;
			}
			return new global::a.b
			{
				A = this.m_A,
				a = this.m_B,
				B = this.m_a,
				b = (float)this.m_A,
				A = global::a.c.A(this.m_A)
			};
		}

		private void A(string P_0, Action P_1)
		{
			try
			{
				P_1();
			}
			catch (Exception ex)
			{
				global::B.I.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gl(), P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + ex.Message);
				this.m_A.Text = P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gM() + ex.Message;
			}
		}

		[DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
		private static extern bool N();

		[DllImport("user32.dll", EntryPoint = "SendMessage")]
		private static extern IntPtr A(IntPtr P_0, int P_1, IntPtr P_2, IntPtr P_3);

		private void n()
		{
			base.AutoScaleMode = AutoScaleMode.None;
			Font = global::a.J.A();
			base.Icon = global::a.J.A;
			BackColor = global::B.L.A;
			ForeColor = global::B.L.b;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Padding = new Padding(8);
			base.MaximizedBounds = Screen.FromControl(this).WorkingArea;
			global::b.h h2 = new global::b.h
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(10)
			};
			base.Controls.Add(h2);
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 5,
				Padding = Padding.Empty,
				Margin = Padding.Empty,
				BackColor = global::B.L.A
			};
			h2.Controls.Add(tableLayoutPanel);
			int[] array = new int[2] { 52, 50 };
			foreach (int num2 in array)
			{
				tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, num2));
			}
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
			Panel panel = new Panel
			{
				Dock = DockStyle.Fill
			};
			tableLayoutPanel.Controls.Add(panel, 0, 0);
			Label label = new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.x(),
				Font = global::a.J.A(21f, FontStyle.Bold),
				ForeColor = global::B.L.b,
				AutoSize = true,
				Location = new Point(4, 4)
			};
			panel.Controls.Add(label);
			Label label2 = new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gm(),
				Font = global::a.J.A(9f),
				ForeColor = global::B.L.C,
				AutoSize = true,
				Location = new Point(260, 15)
			};
			panel.Controls.Add(label2);
			MouseEventHandler value = delegate(object P_0, MouseEventArgs P_1)
			{
				if (P_1.Button == MouseButtons.Left)
				{
					N();
					A(base.Handle, 161, new IntPtr(2), IntPtr.Zero);
				}
			};
			panel.MouseDown += value;
			label.MouseDown += value;
			label2.MouseDown += value;
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Dock = DockStyle.Right,
				Width = 320,
				FlowDirection = FlowDirection.LeftToRight,
				WrapContents = false
			};
			panel.Controls.Add(flowLayoutPanel);
			this.m_B = new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gN(),
				Width = 140,
				Height = 34,
				ForeColor = global::B.L.d,
				TextAlign = ContentAlignment.MiddleRight
			};
			flowLayoutPanel.Controls.Add(this.m_B);
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gn(), delegate
			{
				Hide();
			}, 52);
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gO(), delegate
			{
				base.WindowState = ((base.WindowState != FormWindowState.Maximized) ? FormWindowState.Maximized : FormWindowState.Normal);
				O();
			}, 42);
			A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Y(), delegate
			{
				Close();
			}, 42);
			FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				WrapContents = false,
				Padding = new Padding(8, 4, 0, 0),
				BackColor = global::B.L.B
			};
			tableLayoutPanel.Controls.Add(flowLayoutPanel2, 0, 1);
			flowLayoutPanel2.Controls.Add(A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.go(), 90));
			A(flowLayoutPanel2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gP(), () => this.m_A.D, delegate(bool P_0)
			{
				this.m_A.D = P_0;
			}, 210);
			A(flowLayoutPanel2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gp(), () => this.m_A.A.B, delegate(bool P_0)
			{
				this.m_A.A.B = P_0;
			}, 210);
			A(flowLayoutPanel2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gQ(), () => this.m_A.A.a, delegate(bool P_0)
			{
				this.m_A.A.a = P_0;
			}, 210);
			flowLayoutPanel2.Controls.Add(A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gq(), 165, global::B.L.C));
			TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 3,
				RowCount = 1,
				Margin = new Padding(0, 10, 0, 0),
				BackColor = global::B.L.A
			};
			tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 2);
			for (int num3 = 0; num3 < 3; num3++)
			{
				tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
				this.m_A[num3] = new global::b.e();
				tableLayoutPanel2.Controls.Add(this.m_A[num3], num3, 0);
			}
			global::B.E e2 = A(0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gR(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gr());
			a(e2.A);
			global::B.E e3 = A(1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gS(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GE());
			B(e3.A);
			global::B.E e4 = A(2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gs(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gT());
			b(e4.A);
			e2 = A(0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gt(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gU());
			C(e2.A);
			e3 = A(1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gu(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gV());
			c(e3.A);
			e4 = A(2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gv(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gW());
			D(e4.A);
			this.m_A.Dock = DockStyle.Fill;
			this.m_A.Paint += A;
			this.m_A.Margin = new Padding(0, 8, 8, 0);
			tableLayoutPanel.Controls.Add(this.m_A, 0, 3);
			this.m_A.Dock = DockStyle.Fill;
			this.m_A.ForeColor = global::B.L.C;
			this.m_A.Font = global::a.J.A(8f);
			this.m_A.TextAlign = ContentAlignment.MiddleLeft;
			this.m_A.AutoEllipsis = true;
			tableLayoutPanel.Controls.Add(this.m_A, 0, 4);
			base.Shown += delegate
			{
				O();
			};
			base.SizeChanged += delegate
			{
				O();
			};
			I();
			q();
			p();
		}

		private void O()
		{
			global::b.e[] array = this.m_A;
			for (int num = 0; num < array.Length; num++)
			{
				array[num]?.B();
			}
		}

		private global::B.E A(int P_0, string P_1, string P_2)
		{
			global::B.E e2 = new global::B.E(P_1, P_2);
			this.m_A[P_0].Controls.Add(e2);
			return e2;
		}

		private Label A(string P_0, int P_1 = 160, Color? P_2 = null)
		{
			return new Label
			{
				Text = P_0,
				ForeColor = (P_2 ?? global::B.L.b),
				Width = P_1,
				Height = 30,
				TextAlign = ContentAlignment.MiddleLeft,
				Font = global::a.J.A(9f),
				Margin = new Padding(0, 2, 3, 2),
				AutoEllipsis = true
			};
		}

		private global::A.D A(Control P_0, string P_1, Action P_2, int P_3 = 130)
		{
			global::A.D d2 = new global::A.D
			{
				Text = P_1,
				Width = P_3
			};
			d2.Click += delegate
			{
				try
				{
					P_2();
				}
				catch (Exception ex)
				{
					this.m_A.Text = ex.Message;
				}
			};
			P_0.Controls.Add(d2);
			return d2;
		}

		private global::b.j A(Control P_0, string P_1, Func<bool> P_2, Action<bool> P_3, int P_4 = 146)
		{
			global::b.j A = new global::b.j
			{
				Text = P_1,
				Width = P_4,
				A = P_2()
			};
			A.A(delegate
			{
				if (!this.m_e)
				{
					P_3(A.A);
					f();
					p();
				}
			});
			this.m_A.Add(new KeyValuePair<global::b.j, Func<bool>>(A, P_2));
			P_0.Controls.Add(A);
			return A;
		}

		private void A(Control P_0, string P_1, decimal P_2, decimal P_3, decimal P_4, Action<decimal> P_5, int P_6 = 0)
		{
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
			{
				Width = 150,
				Height = 32,
				WrapContents = false,
				Margin = new Padding(0, 2, 4, 2)
			};
			flowLayoutPanel.Controls.Add(A(P_1, 73, global::B.L.C));
			global::b.f f2 = new global::b.f(P_3, P_4, P_2, P_6)
			{
				Margin = new Padding(0, 3, 0, 0)
			};
			flowLayoutPanel.Controls.Add(f2);
			f2.A = delegate(decimal obj)
			{
				P_5(obj);
				f();
			};
			P_0.Controls.Add(flowLayoutPanel);
		}

		private void A(Control P_0, string P_1, Func<Color> P_2, Action<Color> P_3)
		{
			global::A.D A = this.A(P_0, P_1, delegate
			{
			}, 146);
			A.A = P_2;
			A.Click += delegate
			{
				using ColorDialog colorDialog = new ColorDialog
				{
					Color = P_2(),
					FullOpen = true
				};
				if (colorDialog.ShowDialog(this) == DialogResult.OK)
				{
					P_3(colorDialog.Color);
					A.Invalidate();
					f();
				}
			};
		}

		private global::a.E A(Control P_0, string P_1, bool P_2 = false)
		{
			global::a.E e2 = new global::a.E(P_1);
			P_0.Controls.Add(e2);
			e2.A = P_2;
			return e2;
		}

		private global::a.E A(Control P_0, string P_1, Func<bool> P_2, Action<bool> P_3)
		{
			global::a.E A = this.A(P_0, P_1);
			A.A = true;
			A.A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			global::b.j A2 = this.A(A.A, P_1, P_2, P_3, 270);
			A2.Location = new Point(4, 0);
			A.A.SizeChanged += delegate
			{
				A2.Width = A.A.Width - 50;
			};
			Label label = new Label
			{
				BackColor = global::B.L.a,
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gw(),
				ForeColor = global::B.L.D,
				TextAlign = ContentAlignment.MiddleCenter,
				Size = new Size(36, 30),
				Anchor = (AnchorStyles.Top | AnchorStyles.Right),
				Location = new Point(A.A.Width - 40, 0)
			};
			label.Click += delegate
			{
				A.A = !A.A;
			};
			A.A.Controls.Add(label);
			return A;
		}

		private void A(Control P_0, Control P_1, int P_2)
		{
			P_1.Tag = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DP();
			P_1.Width = 390;
			P_1.Height = P_2;
			P_1.Margin = new Padding(0, 4, 0, 6);
			P_0.Controls.Add(P_1);
		}

		private void a(Control P_0)
		{
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gX(), () => this.m_A.b, delegate(bool flag)
			{
				this.m_A.b = flag;
			}, 146);
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gx(), () => this.m_A.C, delegate(bool flag)
			{
				this.m_A.C = flag;
			}, 146);
			this.m_A.CheckOnClick = true;
			this.m_A.IntegralHeight = false;
			this.m_A.BackColor = global::B.L.A;
			this.m_A.ForeColor = global::B.L.b;
			this.m_A.BorderStyle = BorderStyle.FixedSingle;
			this.m_A.Font = global::a.J.A(9f);
			this.A(P_0, this.m_A, 90);
			this.m_A.ItemCheck += delegate(object obj, ItemCheckEventArgs e6)
			{
				if (!this.m_A)
				{
					this.m_a.A(((global::A.Z)this.m_A.Items[e6.Index]).A, e6.NewValue == CheckState.Checked);
					H();
				}
			};
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gY(), this.m_A.B, 9m, 36m, delegate(decimal num2)
			{
				this.m_A.B = (int)num2;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gy(), this.m_A.a, 9m, 36m, delegate(decimal num2)
			{
				this.m_A.a = (int)num2;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gZ(), () => this.m_A.B, delegate(Color color)
			{
				this.m_A.B = color;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gz(), () => this.m_A.b, delegate(Color color)
			{
				this.m_A.b = color;
			});
			global::a.E e2 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HA());
			string[] array = new string[10]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ha(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HB(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hb(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HC(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hc(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cd(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HD(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hd(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HE(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.He()
			};
			for (int num = 0; num < array.Length; num++)
			{
				int A = num;
				this.A(e2.A, array[A], () => this.m_A.A[A], delegate(bool flag)
				{
					this.m_A.A[A] = flag;
				}, 146);
			}
			global::a.E e3 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HF());
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hf(), () => this.m_A.c, delegate(bool flag)
			{
				this.m_A.c = flag;
			}, 146);
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HG(), () => this.m_A.A, delegate(bool flag)
			{
				this.m_A.A = flag;
			}, 146);
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hg(), this.m_A.b, 0m, 300m, delegate(decimal num2)
			{
				this.m_A.b = (int)num2;
			});
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HH(), this.m_A.C, 40m, 100m, delegate(decimal num2)
			{
				this.m_A.C = (int)num2;
			});
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hh(), delegate
			{
				this.A(true);
			}, 145);
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HI(), delegate
			{
				this.A(false);
			}, 145);
			global::a.E e4 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hi());
			foreach (KeyValuePair<string, Y> item in this.m_A.A.A())
			{
				string text = ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fo()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ha() : ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.FP()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HB() : ((item.Key == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Fp()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hb() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HC())));
				Y A2 = item.Value;
				global::a.E e5 = this.A(e4.A, text);
				this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HJ(), () => A2.A, delegate(bool flag)
				{
					A2.A = flag;
				}, 146);
				this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hj(), () => A2.A, delegate(Color color)
				{
					A2.A = color;
				});
				this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HK(), () => A2.a, delegate(Color color)
				{
					A2.a = color;
				});
				this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hk(), () => A2.B, delegate(Color color)
				{
					A2.B = color;
				});
				this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HL(), A2.A, 1m, 1000000m, delegate(decimal num2)
				{
					A2.A = num2;
				});
				this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hl(), A2.a, 0m, 1000000m, delegate(decimal num2)
				{
					A2.a = num2;
				});
				this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HM(), A2.B, 0.1m, 5m, delegate(decimal num2)
				{
					A2.B = num2;
				}, 1);
			}
		}

		private void B(Control P_0)
		{
			global::b.c A = this.m_A.A;
			global::a.E e2 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hm(), true);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HN(), A.c, 8m, 200m, delegate(decimal num)
			{
				A.c = (int)num;
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hn(), A.D, 8m, 200m, delegate(decimal num)
			{
				A.D = (int)num;
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HO(), A.C, 0.5m, 12m, delegate(decimal num)
			{
				A.C = num;
			}, 1);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ho(), A.d, 8m, 36m, delegate(decimal num)
			{
				A.d = (int)num;
			});
			global::a.E e3 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HP(), () => A.a, delegate(bool flag)
			{
				A.a = flag;
			});
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hp(), A.A, 8m, 160m, delegate(decimal num)
			{
				A.A = (int)num;
			});
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HQ(), A.A, 0.5m, 12m, delegate(decimal num)
			{
				A.A = num;
			}, 1);
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hq(), A.a, 1m, 100m, delegate(decimal num)
			{
				A.a = num;
			}, 1);
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HR(), A.B, 1m, 100m, delegate(decimal num)
			{
				A.B = num;
			}, 1);
			global::a.E e4 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hr(), () => A.C, delegate(bool flag)
			{
				A.C = flag;
			});
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HS(), () => A.d, delegate(bool flag)
			{
				A.d = flag;
			}, 146);
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hs(), () => A.c, delegate(bool flag)
			{
				A.c = flag;
			}, 146);
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HT(), () => A.D, delegate(bool flag)
			{
				A.D = flag;
			}, 146);
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ht(), () => A.E, delegate(bool flag)
			{
				A.E = flag;
			}, 146);
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HU(), A.a, 1m, 200m, delegate(decimal num)
			{
				A.a = (int)num;
			});
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hu(), A.B, 1m, 200m, delegate(decimal num)
			{
				A.B = (int)num;
			});
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HV(), A.b, 1m, 200m, delegate(decimal num)
			{
				A.b = (int)num;
			});
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hv(), A.b, 0.5m, 12m, delegate(decimal num)
			{
				A.b = num;
			}, 1);
			this.A(e4.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HW(), A.C, 0m, 180m, delegate(decimal num)
			{
				A.C = (int)num;
			});
			global::a.E e5 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hw(), () => this.m_A.a, delegate(bool flag)
			{
				this.m_A.a = flag;
			});
			this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HX(), () => this.m_A.A, delegate(Color color)
			{
				this.m_A.A = color;
			});
			this.A(e5.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hx(), () => this.m_A.a, delegate(Color color)
			{
				this.m_A.a = color;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HY(), A.E, 10m, 144m, delegate(decimal num)
			{
				A.E = (int)num;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hy(), A.e, 5m, 144m, delegate(decimal num)
			{
				A.e = (int)num;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HZ(), delegate
			{
				A.b = !A.b;
				f();
				this.m_A.Text = (A.b ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xd() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xD());
			}, 80);
		}

		private void b(Control P_0)
		{
			global::A.j A = this.m_A.A.A;
			t A2 = this.m_A.A.A;
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hz(), () => this.m_A.B, delegate(bool flag)
			{
				this.m_A.B = flag;
			}, 146);
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hA(), this.m_A.A, 8m, 240m, delegate(decimal num)
			{
				this.m_A.A = (int)num;
			});
			global::a.E e2 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ha(), () => A2.B, delegate(bool flag)
			{
				A2.B = flag;
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hB(), () => A2.A, delegate(bool flag)
			{
				A2.A = flag;
			}, 146);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hb(), () => A2.a, delegate(bool flag)
			{
				A2.a = flag;
			}, 146);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hC(), () => A2.b, delegate(bool flag)
			{
				A2.b = flag;
			}, 146);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hc(), A2.A, 8m, 72m, delegate(decimal num)
			{
				A2.A = (int)num;
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ho(), A2.a, 8m, 36m, delegate(decimal num)
			{
				A2.a = (int)num;
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hD(), A2.A, 0.5m, 8m, delegate(decimal num)
			{
				A2.A = num;
			}, 1);
			global::a.E e3 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hd(), () => A.A, delegate(bool flag)
			{
				A.A = flag;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hE(), () => A.a, delegate(bool flag)
			{
				A.a = flag;
			}, 146);
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.he(), A.A, 0m, 100m, delegate(decimal num)
			{
				A.A = (int)num;
			});
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hF(), A.B, 8m, 40m, delegate(decimal num)
			{
				A.B = (int)num;
			});
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hf(), A.A, 0.5m, 8m, delegate(decimal num)
			{
				A.A = num;
			}, 1);
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hG(), A.b, 6m, 24m, delegate(decimal num)
			{
				A.b = (int)num;
			});
			this.A(e3.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hg(), A.a, 0m, 100m, delegate(decimal num)
			{
				A.a = (int)num;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hH(), delegate
			{
				this.m_A.C();
				this.m_A.b();
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xE();
			}, 210);
		}

		private void C(Control P_0)
		{
			global::B.C A = this.m_A.A;
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hh(), () => this.m_A.A.A, delegate(bool flag)
			{
				this.m_A.A.A = flag;
			}, 146);
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hI(), this.m_A.A.A, 8m, 40m, delegate(decimal num)
			{
				this.m_A.A.A = (int)num;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hi(), () => A.B, delegate(bool flag)
			{
				A.B = flag;
			}, 146);
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hJ(), () => A.b, delegate(bool flag)
			{
				A.b = flag;
			}, 146);
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hj(), () => this.m_A.A.A.B, delegate(bool flag)
			{
				this.m_A.A.A.B = flag;
			}, 146);
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hK(), A.b, 1m, 60m, delegate(decimal num)
			{
				A.b = (int)num;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hk(), A.a, 2m, 40m, delegate(decimal num)
			{
				A.a = (int)num;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hL(), A.B, 3m, 60m, delegate(decimal num)
			{
				A.B = (int)num;
			});
			global::a.E e2 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hl());
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.HQ(), A.A, 0.5m, 8m, delegate(decimal num)
			{
				A.A = num;
			}, 1);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Hp(), A.C, 2m, 40m, delegate(decimal num)
			{
				A.C = (int)num;
			});
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hM(), A.c, 4m, 60m, delegate(decimal num)
			{
				A.c = (int)num;
			});
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hm(), o, 230);
		}

		private void c(Control P_0)
		{
			this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hN(), () => this.m_A.A, delegate(bool flag)
			{
				this.m_A.A = flag;
				this.m_A.B();
			}, 146);
			P_0.Controls.Add(this.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hn(), 110, global::B.L.C));
			foreach (global::A.l value in Enum.GetValues(typeof(global::A.l)))
			{
				global::A.l A = value;
				FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
				{
					Tag = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DP(),
					Width = 390,
					Height = 34,
					WrapContents = false,
					Margin = new Padding(0, 1, 0, 1)
				};
				P_0.Controls.Add(flowLayoutPanel);
				string A2 = ((A == global::A.l.A) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hR() : ((A == global::A.l.a) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hq() : ((A == global::A.l.B) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hQ() : ((A == global::A.l.C) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hp() : ((A == global::A.l.c) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hP() : ((A == global::A.l.D) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ho() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hO()))))));
				flowLayoutPanel.Controls.Add(this.A(A2, 110));
				global::A.D A3 = this.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hr(), delegate
				{
					using OpenFileDialog openFileDialog = new OpenFileDialog
					{
						Title = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xe() + A2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hr(),
						Filter = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xF()
					};
					if (openFileDialog.ShowDialog(this) == DialogResult.OK)
					{
						this.m_A.A(A, openFileDialog.FileName);
						this.m_A.Text = A2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gM() + this.m_A.a(A);
						p();
					}
				}, 62);
				this.m_A.SetToolTip(A3, this.m_A.a(A));
				this.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.GU(), delegate
				{
					this.m_A.A(A, true);
				}, 52);
				this.A(flowLayoutPanel, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hS(), delegate
				{
					this.m_A.A(A, null);
					this.m_A.SetToolTip(A3, this.m_A.a(A));
					this.m_A.Text = A2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gM() + this.m_A.a(A);
				}, 52);
			}
			global::a.E e2 = this.A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hs());
			this.m_a.CheckOnClick = true;
			this.m_a.IntegralHeight = false;
			this.m_a.BorderStyle = BorderStyle.FixedSingle;
			this.m_a.BackColor = global::B.L.A;
			this.m_a.ForeColor = global::B.L.b;
			this.A(e2.A, this.m_a, 100);
			this.m_a.ItemCheck += delegate(object obj, ItemCheckEventArgs e3)
			{
				if (!this.m_F)
				{
					this.m_A.A(((global::A.Z)this.m_a.Items[e3.Index]).A, e3.NewValue == CheckState.Checked);
				}
			};
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bp(), delegate
			{
				foreach (O item in this.m_A.Where((O o2) => o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
				{
					this.m_A.A(item, true);
				}
				Q();
			}, 145);
			this.A(e2.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bQ(), delegate
			{
				foreach (O item2 in this.m_A.Where((O o2) => o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
				{
					this.m_A.A(item2, false);
				}
				Q();
			}, 145);
		}

		private void D(Control P_0)
		{
			A(P_0);
			ComboBox comboBox = this.m_a;
			ComboBoxStyle comboBoxStyle = (this.m_A.DropDownStyle = ComboBoxStyle.DropDownList);
			ComboBoxStyle dropDownStyle = comboBoxStyle;
			comboBox.DropDownStyle = dropDownStyle;
			this.m_a.Width = 110;
			this.m_A.Width = 65;
			ComboBox comboBox2 = this.m_a;
			FlatStyle flatStyle = (this.m_A.FlatStyle = FlatStyle.Flat);
			FlatStyle flatStyle3 = flatStyle;
			comboBox2.FlatStyle = flatStyle3;
			ComboBox comboBox3 = this.m_a;
			Color color = (this.m_A.BackColor = global::B.L.B);
			Color backColor = color;
			comboBox3.BackColor = backColor;
			ComboBox comboBox4 = this.m_a;
			color = (this.m_A.ForeColor = global::B.L.b);
			Color foreColor = color;
			comboBox4.ForeColor = foreColor;
			this.m_a.Items.AddRange(new object[5]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hT(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ht(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hU(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hu(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hV()
			});
			this.m_a.SelectedIndex = 0;
			for (int num = 1; num <= 12; num++)
			{
				this.m_A.Items.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hv() + num);
			}
			for (char c2 = 'A'; c2 <= 'Z'; c2 = (char)(c2 + 1))
			{
				this.m_A.Items.Add(c2.ToString());
			}
			this.m_A.SelectedItem = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hW();
			D();
			ComboBox[] array = new ComboBox[2] { this.m_a, this.m_A };
			foreach (ComboBox obj in array)
			{
				obj.DrawMode = DrawMode.OwnerDrawFixed;
				obj.Font = global::a.J.A(9f);
				obj.ItemHeight = 22;
				obj.DrawItem += delegate(object obj2, DrawItemEventArgs e3)
				{
					if (e3.Index >= 0)
					{
						ComboBox comboBox5 = (ComboBox)obj2;
						using (SolidBrush brush = new SolidBrush(((e3.State & DrawItemState.Selected) != DrawItemState.None) ? global::B.L.B : global::B.L.A))
						{
							e3.Graphics.FillRectangle(brush, e3.Bounds);
						}
						TextRenderer.DrawText(e3.Graphics, comboBox5.Items[e3.Index].ToString(), comboBox5.Font, e3.Bounds, global::B.L.b, TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
					}
				};
			}
			P_0.Controls.Add(A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hw(), 130));
			P_0.Controls.Add(this.m_a);
			P_0.Controls.Add(this.m_A);
			A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hX(), d, 150);
			A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hx(), E, 150);
			A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hY(), delegate
			{
				this.m_A.b();
				this.m_A.B();
				this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.If();
			}, 298);
			A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hy(), delegate
			{
				Hide();
			}, 298);
			global::a.E e2 = A(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hZ());
			A(e2.A, new Label
			{
				Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.hz(),
				ForeColor = global::B.L.C,
				Font = global::a.J.A(9f)
			}, 110);
		}

		private void o()
		{
			this.m_A.a();
			this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IA();
		}

		private void P()
		{
			p();
		}

		private void p()
		{
			if (this.m_e)
			{
				return;
			}
			this.m_e = true;
			try
			{
				foreach (KeyValuePair<global::b.j, Func<bool>> item in this.m_A)
				{
					if (!item.Key.IsDisposed)
					{
						item.Key.A = item.Value();
					}
				}
				if (this.m_B != null)
				{
					var (text, text2) = global::a.G.A switch
					{
						global::a.G.A.B => (CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ia(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IB()), 
						global::a.G.A.b => (CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ib(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IC()), 
						_ => (this.m_a ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ic() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.gN(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()), 
					};
					this.m_B.Text = (this.m_B ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ID() : text);
					this.m_A.SetToolTip(this.m_B, this.m_B ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D() : text2);
				}
			}
			finally
			{
				this.m_e = false;
			}
		}

		private void Q()
		{
			if (this.m_a == null)
			{
				return;
			}
			this.m_F = true;
			try
			{
				O[] array = (from P_0 in this.m_A
					where P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
					orderby P_0.A
					select P_0).ToArray();
				bool flag = this.m_a.Items.Count == array.Length;
				int num = 0;
				while (flag && num < array.Length)
				{
					flag = ((global::A.Z)this.m_a.Items[num]).A.A == array[num].A && ((global::A.Z)this.m_a.Items[num]).A.A == array[num].A;
					num++;
				}
				if (!flag)
				{
					this.m_a.Items.Clear();
					O[] array2 = array;
					foreach (O o2 in array2)
					{
						this.m_a.Items.Add(new global::A.Z
						{
							A = o2
						}, this.m_A.a(o2));
					}
				}
				else
				{
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						((global::A.Z)this.m_a.Items[num3]).A = array[num3];
						this.m_a.SetItemChecked(num3, this.m_A.a(array[num3]));
					}
				}
			}
			finally
			{
				this.m_F = false;
			}
		}

		private bool a(ref Message P_0)
		{
			if (P_0.Msg != 132 || base.WindowState == FormWindowState.Maximized)
			{
				return false;
			}
			Point point = PointToClient(new Point((short)(P_0.LParam.ToInt64() & 0xFFFF), (short)((P_0.LParam.ToInt64() >> 16) & 0xFFFF)));
			bool num = point.X < 7;
			bool flag = point.X > base.Width - 8;
			bool flag2 = point.Y < 7;
			bool flag3 = point.Y > base.Height - 8;
			int num2 = (num ? (flag2 ? 13 : (flag3 ? 16 : 10)) : (flag ? (flag2 ? 14 : (flag3 ? 17 : 11)) : (flag2 ? 12 : (flag3 ? 15 : 0))));
			if (num2 == 0)
			{
				return false;
			}
			P_0.Result = new IntPtr(num2);
			return true;
		}

		protected override void Dispose(bool P_0)
		{
			if (P_0)
			{
				this.m_b.Stop();
				this.m_b.Dispose();
				L();
			}
			if (P_0 && this.m_A != null)
			{
				this.m_A.Dispose();
				this.m_A = null;
			}
			base.Dispose(P_0);
		}

		private void q()
		{
			this.m_b.Interval = 150;
			this.m_b.Tick += delegate
			{
				R();
			};
			if (!this.m_B && global::a.O.A)
			{
				this.m_b.Start();
			}
		}

		private void R()
		{
			if (!global::a.O.A || this.m_A == null || !this.m_a)
			{
				return;
			}
			try
			{
				if (this.m_C != this.m_A.C)
				{
					this.m_C = this.m_A.C;
					this.m_A.Clear();
					this.m_A.Clear();
					this.m_f = false;
				}
				foreach (global::b.g item in this.m_A.A(this.m_A))
				{
					if (!this.m_A.Add(item.A))
					{
						continue;
					}
					if (item.A)
					{
						if (this.m_f)
						{
							this.m_A.A(global::A.l.A, false);
							this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() + (item.A + 1) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Id();
						}
					}
					else
					{
						item.A = DateTime.UtcNow.AddSeconds(global::b.H.A(item.a));
						this.m_A[item.A] = item;
					}
				}
				this.m_f = true;
				string[] array = this.m_A.Where(delegate(KeyValuePair<string, global::b.g> P_0)
				{
					KeyValuePair<string, global::b.g> keyValuePair = P_0;
					return keyValuePair.Value.A < DateTime.UtcNow;
				}).Select(delegate(KeyValuePair<string, global::b.g> P_0)
				{
					KeyValuePair<string, global::b.g> keyValuePair = P_0;
					return keyValuePair.Key;
				}).ToArray();
				foreach (string key in array)
				{
					this.m_A.Remove(key);
				}
			}
			catch
			{
			}
		}

		private void r()
		{
			R();
			foreach (O A in this.m_A)
			{
				A.A.RemoveAll((global::A.T P_0) => P_0.A != null && P_0.A.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IE()));
				foreach (global::b.g item in this.m_A.Values.Where((global::b.g P_0) => P_0.A == A.A))
				{
					A.A.Add(new global::A.T
					{
						A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IE() + item.A,
						a = item.A,
						B = item.B,
						b = A.A,
						C = item.a,
						A = A.A,
						A = item.A,
						a = item.a,
						B = item.B,
						A = A.A
					});
				}
			}
		}

		[CompilerGenerated]
		private void A(object P_0, EventArgs P_1)
		{
			e();
		}

		[CompilerGenerated]
		private void a(object P_0, EventArgs P_1)
		{
			e();
		}

		[CompilerGenerated]
		private void B(object P_0, EventArgs P_1)
		{
			if (this.B != null)
			{
				this.B();
			}
			else
			{
				Close();
			}
		}

		[CompilerGenerated]
		private void b(object P_0, EventArgs P_1)
		{
			if (base.WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		}

		[CompilerGenerated]
		private void C(object P_0, EventArgs P_1)
		{
			if (base.Visible)
			{
				this.m_A.Invalidate();
			}
		}

		[CompilerGenerated]
		private void c(object P_0, EventArgs P_1)
		{
			h();
		}

		[CompilerGenerated]
		private void D(object P_0, EventArgs P_1)
		{
			c();
		}

		[CompilerGenerated]
		private bool A(O P_0)
		{
			if (!this.m_a.a(P_0))
			{
				if (this.m_A.c)
				{
					if (!(P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()))
					{
						return P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dl();
					}
					return true;
				}
				return false;
			}
			return true;
		}

		[CompilerGenerated]
		private void S()
		{
			this.m_A.A(this.m_A, this.m_A, this.m_A);
		}

		[CompilerGenerated]
		private void s()
		{
			this.m_A.A(this.m_A, this.m_A, this.m_A);
		}

		[CompilerGenerated]
		private void T()
		{
			this.m_A.A(this.m_A, this.m_A, this.m_A);
		}

		[CompilerGenerated]
		private void t()
		{
			this.m_A.A(this.m_A, this.m_A, this.m_A);
		}

		[CompilerGenerated]
		private bool A(global::b.A P_0)
		{
			if (P_0.A == this.m_A)
			{
				return P_0.a == this.m_a;
			}
			return false;
		}

		[CompilerGenerated]
		private void U()
		{
			this.m_b?.Invoke();
		}

		[CompilerGenerated]
		private void u()
		{
			using global::a.P p2 = new global::a.P(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ie(), string.IsNullOrEmpty(this.m_b) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IF() : this.m_b, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dy());
			p2.ShowDialog(this);
		}

		[CompilerGenerated]
		private void A(object P_0, MouseEventArgs P_1)
		{
			if (P_1.Button == MouseButtons.Left)
			{
				N();
				A(base.Handle, 161, new IntPtr(2), IntPtr.Zero);
			}
		}

		[CompilerGenerated]
		private void V()
		{
			Hide();
		}

		[CompilerGenerated]
		private void v()
		{
			base.WindowState = ((base.WindowState != FormWindowState.Maximized) ? FormWindowState.Maximized : FormWindowState.Normal);
			O();
		}

		[CompilerGenerated]
		private void W()
		{
			Close();
		}

		[CompilerGenerated]
		private bool w()
		{
			return this.m_A.D;
		}

		[CompilerGenerated]
		private void a(bool P_0)
		{
			this.m_A.D = P_0;
		}

		[CompilerGenerated]
		private bool X()
		{
			return this.m_A.A.B;
		}

		[CompilerGenerated]
		private void B(bool P_0)
		{
			this.m_A.A.B = P_0;
		}

		[CompilerGenerated]
		private bool x()
		{
			return this.m_A.A.a;
		}

		[CompilerGenerated]
		private void b(bool P_0)
		{
			this.m_A.A.a = P_0;
		}

		[CompilerGenerated]
		private void d(object P_0, EventArgs P_1)
		{
			O();
		}

		[CompilerGenerated]
		private void E(object P_0, EventArgs P_1)
		{
			O();
		}

		[CompilerGenerated]
		private bool Y()
		{
			return this.m_A.b;
		}

		[CompilerGenerated]
		private void C(bool P_0)
		{
			this.m_A.b = P_0;
		}

		[CompilerGenerated]
		private bool y()
		{
			return this.m_A.C;
		}

		[CompilerGenerated]
		private void c(bool P_0)
		{
			this.m_A.C = P_0;
		}

		[CompilerGenerated]
		private void A(object P_0, ItemCheckEventArgs P_1)
		{
			if (!this.m_A)
			{
				this.m_a.A(((global::A.Z)this.m_A.Items[P_1.Index]).A, P_1.NewValue == CheckState.Checked);
				H();
			}
		}

		[CompilerGenerated]
		private void A(decimal P_0)
		{
			this.m_A.B = (int)P_0;
		}

		[CompilerGenerated]
		private void a(decimal P_0)
		{
			this.m_A.a = (int)P_0;
		}

		[CompilerGenerated]
		private Color Z()
		{
			return this.m_A.B;
		}

		[CompilerGenerated]
		private void A(Color P_0)
		{
			this.m_A.B = P_0;
		}

		[CompilerGenerated]
		private Color z()
		{
			return this.m_A.b;
		}

		[CompilerGenerated]
		private void a(Color P_0)
		{
			this.m_A.b = P_0;
		}

		[CompilerGenerated]
		private bool aA()
		{
			return this.m_A.c;
		}

		[CompilerGenerated]
		private void D(bool P_0)
		{
			this.m_A.c = P_0;
		}

		[CompilerGenerated]
		private bool aa()
		{
			return this.m_A.A;
		}

		[CompilerGenerated]
		private void d(bool P_0)
		{
			this.m_A.A = P_0;
		}

		[CompilerGenerated]
		private void B(decimal P_0)
		{
			this.m_A.b = (int)P_0;
		}

		[CompilerGenerated]
		private void b(decimal P_0)
		{
			this.m_A.C = (int)P_0;
		}

		[CompilerGenerated]
		private void aB()
		{
			A(true);
		}

		[CompilerGenerated]
		private void ab()
		{
			A(false);
		}

		[CompilerGenerated]
		private bool aC()
		{
			return this.m_A.A;
		}

		[CompilerGenerated]
		private void E(bool P_0)
		{
			this.m_A.A = P_0;
			this.m_A.B();
		}

		[CompilerGenerated]
		private void a(object P_0, ItemCheckEventArgs P_1)
		{
			if (!this.m_F)
			{
				this.m_A.A(((global::A.Z)this.m_a.Items[P_1.Index]).A, P_1.NewValue == CheckState.Checked);
			}
		}

		[CompilerGenerated]
		private void ac()
		{
			foreach (O item in this.m_A.Where((O P_0) => P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
			{
				this.m_A.A(item, true);
			}
			Q();
		}

		[CompilerGenerated]
		private void aD()
		{
			foreach (O item in this.m_A.Where((O P_0) => P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
			{
				this.m_A.A(item, false);
			}
			Q();
		}

		[CompilerGenerated]
		private void ad()
		{
			this.m_A.b();
			this.m_A.B();
			this.m_A.Text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.If();
		}

		[CompilerGenerated]
		private void aE()
		{
			Hide();
		}

		[CompilerGenerated]
		private void e(object P_0, EventArgs P_1)
		{
			R();
		}
	}
	internal static class k
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<O, uint> A;

			public static Func<O, uint> a;

			public static Func<O, uint> B;

			public static Func<string, bool> A;

			internal uint A(O P_0)
			{
				return P_0.A;
			}

			internal uint a(O P_0)
			{
				return P_0.A;
			}

			internal uint B(O P_0)
			{
				return P_0.A;
			}

			internal bool A(string P_0)
			{
				return P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xf());
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public J A;

			internal bool A(O P_0)
			{
				if (!(P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
				{
					return this.A.c;
				}
				return true;
			}

			internal IEnumerable<Color> a(O P_0)
			{
				if (this.A.A && P_0.b)
				{
					return new Color[2] { P_0.A, P_0.A };
				}
				return new Color[2]
				{
					this.A.B,
					this.A.b
				};
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public J A;

			public Func<O, bool> A;

			internal bool A(O P_0)
			{
				if (!(P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
				{
					return this.A.c;
				}
				return true;
			}
		}

		[CompilerGenerated]
		private sealed class b
		{
			public J A;

			public Func<O, bool> A;

			internal bool A(O P_0)
			{
				if (!(P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()))
				{
					if (this.A != null)
					{
						return this.A.c;
					}
					return false;
				}
				return true;
			}
		}

		public static Color[] A(IEnumerable<O> P_0, J P_1)
		{
			return (from o2 in P_0
				where o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() || P_1.c
				orderby o2.A
				select o2).SelectMany((O o2) => (P_1.A && o2.b) ? new Color[2] { o2.A, o2.A } : new Color[2] { P_1.B, P_1.b }).ToArray();
		}

		public static h[][] a(IEnumerable<O> P_0, J P_1)
		{
			List<h[]> list = new List<h[]>();
			foreach (O item in from o2 in P_0
				where o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() || P_1.c
				orderby o2.A
				select o2)
			{
				List<h> list2 = new List<h>();
				A(item, P_1, list2);
				list.Add(list2.ToArray());
				list.Add(new h[0]);
			}
			return list.ToArray();
		}

		private static string A(string P_0)
		{
			return Regex.Replace(P_0 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IG(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba()).Trim();
		}

		public static string[] B(IEnumerable<O> P_0, J P_1 = null)
		{
			List<string> list = new List<string>();
			foreach (O item in from o2 in P_0
				where o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA() || (P_1 != null && P_1.c)
				orderby o2.A
				select o2)
			{
				global::A.u.a(item);
				global::A.t.A(item);
				q.A(item);
				global::a.g.A(item);
				global::a.j.A(item);
				string text = A(item.A);
				if (text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ig()))
				{
					text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IH() + text.Substring(6);
				}
				list.Add(A(item, P_1, new List<h>()));
				List<string> list2 = new List<string>();
				foreach (string item2 in item.A.Where((string text2) => text2.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.xf())))
				{
					if (item2.Split('\\').Length >= 5 && global::A.u.B(item2))
					{
						list2.Add(global::A.u.b(item2));
					}
				}
				List<string> list3 = new List<string>();
				if (P_1 == null || P_1.C)
				{
					list3.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ih() + ((list2.Count == 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.II() : string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dk(), list2.Distinct())));
				}
				if (P_1 != null && P_1.b)
				{
					list3.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ii() + global::b.k.A(item));
				}
				list.Add(string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dt(), list3));
			}
			return list.ToArray();
		}

		public static string A(O P_0, J P_1, List<h> P_2)
		{
			bool[] array = ((P_1 == null) ? Enumerable.Repeat(element: true, 10).ToArray() : P_1.A);
			string text = A(P_0.A);
			if (text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ig()))
			{
				text = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IH() + text.Substring(6);
			}
			string text2 = text;
			string[] array2 = new string[5]
			{
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IJ(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ij(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IK(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ik(),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IL()
			};
			double[] array3 = new double[5]
			{
				Math.Floor(P_0.A),
				Math.Floor(P_0.B),
				Math.Floor(P_0.a),
				P_0.C,
				Math.Floor(P_0.b)
			};
			Y[] array4 = ((P_1 == null) ? new Y[4] : new Y[4]
			{
				P_1.A.A,
				P_1.A.a,
				P_1.A.B,
				P_1.A.b
			});
			for (int num = 0; num < 5; num++)
			{
				if (array[num])
				{
					string text3 = array3[num].ToString((num == 3) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Il() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By(), CultureInfo.CurrentCulture);
					text2 += array2[num];
					if (num < 4 && array4[num] != null && array4[num].A)
					{
						P_2.Add(new h
						{
							A = text2.Length,
							a = text3.Length,
							A = array3[num],
							A = array4[num]
						});
					}
					text2 += text3;
				}
			}
			string[] array5 = new string[5]
			{
				global::A.u.a(P_0),
				global::A.t.A(P_0),
				q.A(P_0),
				global::a.g.A(P_0),
				global::a.j.A(P_0)
			};
			for (int num2 = 0; num2 < 5; num2++)
			{
				if (array[5 + num2] && !string.IsNullOrEmpty(array5[num2]))
				{
					text2 = text2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.IM() + array5[num2];
				}
			}
			return text2;
		}
	}
}
namespace b
{
	internal sealed class K
	{
		public int A;

		public string A;

		public bool A;
	}
	internal static class k
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<KeyValuePair<string, int>, string> A;

			public static Func<IGrouping<string, KeyValuePair<string, int>>, string> A;

			public static Func<KeyValuePair<string, int>, int> A;

			public static Func<IGrouping<string, KeyValuePair<string, int>>, string> a;

			internal string A(KeyValuePair<string, int> P_0)
			{
				KeyValuePair<string, int> keyValuePair = P_0;
				return k.A(keyValuePair.Key);
			}

			internal string A(IGrouping<string, KeyValuePair<string, int>> P_0)
			{
				return P_0.Key;
			}

			internal string a(IGrouping<string, KeyValuePair<string, int>> P_0)
			{
				return P_0.Key + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yl() + P_0.Sum((KeyValuePair<string, int> keyValuePair) => keyValuePair.Value);
			}

			internal int a(KeyValuePair<string, int> P_0)
			{
				return P_0.Value;
			}
		}

		private static readonly Dictionary<string, string> m_A;

		static k()
		{
			k.m_A = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lo(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LP()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lp(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LQ()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lq(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LQ()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LR(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lr()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LS(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ls()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LT(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ls()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lt(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LU()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lu(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LV()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lv(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LW()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lw(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LX()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lx(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LY()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ly(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LZ()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Lz(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lA()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.la(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lB()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lb(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lC()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lc(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lD()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ld(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lE()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.le(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lF()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lf(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lG()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lg(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lH()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lh(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lI()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.li(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lJ()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lj(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lK()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lk(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lK()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lL(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ll()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lM(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lm()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lN(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ln()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lO(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lo()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lP(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lp()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lQ(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lq()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lR(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lr()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lS(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ls()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lT(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lt()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lU(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lu()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lV(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lu()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lv(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lW()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lw(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lX()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lx(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lY()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ly(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lZ()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lz(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MA()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ma(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MB()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mb(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MC()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mc(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MD()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Md(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ME()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Me(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MF()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mf(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MG()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mg(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MH()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mh(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MI()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mi(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MJ()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mj(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MK()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mk(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ML()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ml(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MM()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mm(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MN()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mn(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MO()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mo(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MH()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MP(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MI()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mp(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MQ()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mq(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MR()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mr(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MS()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ms(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MT()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mt(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MU()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mu(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.lB()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MV(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mv()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MW(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mw()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MX(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mx()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MY(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.My()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MZ(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mz()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mA(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ma()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mB(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MR()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mb(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mC()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mc(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mD()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.md(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mE()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.me(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mF()
				}
			};
			try
			{
				string[] array = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mf()));
				foreach (string text in array)
				{
					string[] array2 = text.Split('\t');
					if (array2.Length == 2 && !text.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.B()) && array2[1].Trim().Length > 0)
					{
						k.m_A[array2[0].Trim()] = array2[1].Trim();
					}
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		public static string A(string P_0)
		{
			if (M.A(P_0, out var result))
			{
				return result;
			}
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (text.Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MY(), StringComparison.OrdinalIgnoreCase))
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.My();
			}
			if (text.Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.MZ(), StringComparison.OrdinalIgnoreCase))
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Mz();
			}
			if (!k.m_A.TryGetValue(P_0, out var value) && !k.m_A.TryGetValue(text, out value) && !L.A.TryGetValue(P_0, out value))
			{
				return text;
			}
			return value;
		}

		public static string A(global::B.O P_0)
		{
			if (!P_0.c)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mG();
			}
			if (P_0.A.Count != 0)
			{
				return string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Dk(), from grouping in P_0.A.GroupBy(delegate(KeyValuePair<string, int> keyValuePair2)
					{
						KeyValuePair<string, int> keyValuePair = keyValuePair2;
						return A(keyValuePair.Key);
					})
					orderby grouping.Key
					select grouping.Key + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Yl() + grouping.Sum((KeyValuePair<string, int> keyValuePair) => keyValuePair.Value));
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.mg();
		}
	}
}
