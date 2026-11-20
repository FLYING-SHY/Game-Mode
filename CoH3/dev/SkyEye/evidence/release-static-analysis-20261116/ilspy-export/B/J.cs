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
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using B;
using a;
using b;

namespace B
{
	internal sealed class J : Form
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

			public static Func<N, bool> A;

			public static Func<N, uint> A;

			public static Func<N, global::A.y> A;

			public static Func<N, bool> a;

			public static Func<N, bool> B;

			public static Func<N, bool> b;

			public static Func<N, IEnumerable<global::b.c>> A;

			public static Func<N, bool> C;

			public static Func<A<Z, double>, double> A;

			public static Action A;

			public static Func<N, bool> c;

			public static Func<N, bool> D;

			public static DrawItemEventHandler A;

			public static Func<N, bool> d;

			public static Func<N, uint> a;

			public static Func<KeyValuePair<string, global::b.f>, bool> A;

			public static Func<KeyValuePair<string, global::b.f>, string> A;

			public static Predicate<global::A.s> A;

			internal bool A(N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}

			internal uint a(N P_0)
			{
				return P_0.A;
			}

			internal global::A.y B(N P_0)
			{
				return new global::A.y
				{
					A = P_0
				};
			}

			internal bool b(N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}

			internal bool C(N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}

			internal bool c(N P_0)
			{
				return P_0.C;
			}

			internal IEnumerable<global::b.c> D(N P_0)
			{
				return P_0.A;
			}

			internal bool d(N P_0)
			{
				return !P_0.C;
			}

			internal double A(A<Z, double> P_0)
			{
				return P_0.a;
			}

			internal void A()
			{
			}

			internal bool E(N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}

			internal bool e(N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}

			internal void A(object P_0, DrawItemEventArgs P_1)
			{
				if (P_1.Index >= 0)
				{
					ComboBox comboBox = (ComboBox)P_0;
					using (SolidBrush brush = new SolidBrush(((P_1.State & DrawItemState.Selected) != DrawItemState.None) ? global::B.K.B : global::B.K.A))
					{
						P_1.Graphics.FillRectangle(brush, P_1.Bounds);
					}
					TextRenderer.DrawText(P_1.Graphics, comboBox.Items[P_1.Index].ToString(), comboBox.Font, P_1.Bounds, global::B.K.b, TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
				}
			}

			internal bool F(N P_0)
			{
				return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz();
			}

			internal uint f(N P_0)
			{
				return P_0.A;
			}

			internal bool A(KeyValuePair<string, global::b.f> P_0)
			{
				KeyValuePair<string, global::b.f> keyValuePair = P_0;
				return keyValuePair.Value.A < DateTime.UtcNow;
			}

			internal string a(KeyValuePair<string, global::b.f> P_0)
			{
				KeyValuePair<string, global::b.f> keyValuePair = P_0;
				return keyValuePair.Key;
			}

			internal bool A(global::A.s P_0)
			{
				if (P_0.A != null)
				{
					return P_0.A.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hz());
				}
				return false;
			}
		}

		[CompilerGenerated]
		private sealed class B
		{
			public J A;

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
					Filter = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XX()
				};
				if (openFileDialog.ShowDialog(this.A) == DialogResult.OK)
				{
					using (new global::A.Q(openFileDialog.FileName))
					{
					}
					this.A.m_a = openFileDialog.FileName;
					this.A.i();
					this.A.m_a.Text = Path.GetFileName(openFileDialog.FileName);
				}
			}

			internal void C()
			{
				this.A.m_a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
				this.A.i();
				this.A.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xx() + Path.GetFileName(global::A.v.A);
			}

			internal void A(object P_0, EventArgs P_1)
			{
				this.A.j();
			}

			internal void a(object P_0, EventArgs P_1)
			{
				if (global::a.N.A)
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

			public global::a.v A;

			public Point A;

			internal A<Z, double> A(Z P_0)
			{
				PointF pointF;
				bool flag = this.A.A(P_0.A, P_0.a + 0.8f, P_0.B, this.A.B, this.A.b, out pointF);
				return new A<Z, double>(P_0, flag ? ((double)((pointF.X - (float)this.A.X) * (pointF.X - (float)this.A.X) + (pointF.Y - (float)this.A.Y) * (pointF.Y - (float)this.A.Y))) : double.MaxValue);
			}
		}

		[CompilerGenerated]
		private sealed class C
		{
			public long A;

			public string A;

			public J A;

			internal void A(object P_0)
			{
				try
				{
					if (!global::a.N.A || global::a.N.a != this.A)
					{
						return;
					}
					if (!string.IsNullOrWhiteSpace(this.A))
					{
						using (global::A.Q q2 = new global::A.Q(this.A))
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
							this.A.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XY();
						});
					}
					catch
					{
					}
				}
			}

			internal void A()
			{
				this.A.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XY();
			}
		}

		[CompilerGenerated]
		private sealed class c
		{
			public Action A;

			public J A;

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
			public J A;

			public Action<bool> A;

			public global::b.i A;

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

			public J A;

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

			public J A;

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
			public global::b.i A;

			public global::a.d A;

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

			public J A;

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
			public X A;

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
			public global::b.b A;

			public J A;

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
				this.A.m_A.Text = (this.A.b ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XZ() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xy());
			}
		}

		[CompilerGenerated]
		private sealed class g
		{
			public J A;

			public s A;

			public global::A.J A;

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
				this.A.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xz();
			}
		}

		[CompilerGenerated]
		private sealed class H
		{
			public J A;

			public global::B.B A;

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

			public global::A.L A;

			public global::A.D A;

			public J A;

			internal void A()
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Title = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xA() + this.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hO(),
					Filter = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xa()
				};
				if (openFileDialog.ShowDialog(this.A) == DialogResult.OK)
				{
					this.A.m_A.A(this.A, openFileDialog.FileName);
					this.A.m_A.Text = this.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gi() + this.A.m_A.a(this.A);
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
				this.A.m_A.Text = this.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gi() + this.A.m_A.a(this.A);
			}
		}

		[CompilerGenerated]
		private sealed class I
		{
			public N A;

			public Func<global::b.f, bool> A;

			internal bool A(global::b.f P_0)
			{
				return P_0.A == this.A.A;
			}
		}

		private readonly global::A.O m_A = new global::A.O();

		private readonly global::A.z m_A = new global::A.z();

		private readonly global::A.m m_A = new global::A.m();

		private readonly global::A.l m_A = new global::A.l();

		private readonly global::A.z m_a = new global::A.z();

		private readonly CheckedListBox m_A = new CheckedListBox();

		private bool m_A;

		private readonly global::B.I m_A = global::B.I.A();

		private readonly global::A.Z m_A;

		private readonly global::a.W m_A;

		private readonly global::b.A m_A;

		private readonly global::a.z m_A;

		private readonly global::A.h m_A;

		private readonly System.Windows.Forms.Timer m_A = new System.Windows.Forms.Timer();

		private readonly System.Windows.Forms.Timer m_a = new System.Windows.Forms.Timer();

		private readonly Label m_A = new Label();

		private readonly Panel m_A = new O();

		private readonly NotifyIcon m_A = new NotifyIcon();

		private m m_A;

		private int m_A;

		private DateTime m_A = DateTime.MinValue;

		private List<N> m_A = new List<N>();

		private bool m_a;

		private readonly ComboBox m_A = new ComboBox();

		private readonly ComboBox m_a = new ComboBox();

		private readonly bool m_B;

		private uint m_A;

		private uint m_a;

		private bool m_b;

		private readonly System.Windows.Forms.Timer m_B = new System.Windows.Forms.Timer();

		private global::a.b m_A;

		private bool m_C;

		private bool m_c;

		private bool m_D;

		private bool m_d;

		private bool m_E = true;

		private decimal m_A = 2m;

		private int m_a = 40;

		private int m_B = 15;

		private string m_A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fN();

		private string m_a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();

		private string m_B;

		private DateTime m_a;

		private DateTime m_B;

		private DateTime m_b;

		private float m_A;

		private float m_a;

		private float m_B;

		private long m_A;

		private long m_a;

		private List<Z> m_A = new List<Z>();

		private global::a.c m_A;

		private Label m_a;

		private uint m_B;

		private uint m_b;

		private string m_b = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();

		private readonly v m_A = new v();

		private readonly List<KeyValuePair<global::b.i, Func<bool>>> m_A = new List<KeyValuePair<global::b.i, Func<bool>>>();

		private readonly global::b.d[] m_A = new global::b.d[3];

		private readonly CheckedListBox m_a = new CheckedListBox();

		private bool m_e;

		private bool m_F;

		private Label m_B;

		private ToolTip m_A = new ToolTip();

		private readonly System.Windows.Forms.Timer m_b = new System.Windows.Forms.Timer();

		private readonly Dictionary<string, global::b.f> m_A = new Dictionary<string, global::b.f>();

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

		private string b => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fM());

		private string C => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fm());

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

		public J(bool P_0 = false)
		{
			this.m_B = P_0;
			Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fn();
			base.ClientSize = new Size(Math.Min(1480, Screen.PrimaryScreen.WorkingArea.Width - 30), Math.Min(950, Screen.PrimaryScreen.WorkingArea.Height - 30));
			MinimumSize = new Size(1180, 740);
			Font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), 10f);
			base.StartPosition = FormStartPosition.CenterScreen;
			this.m_A = new global::A.Z(this.m_A, !P_0);
			this.m_A = new global::a.W(this.m_A, !P_0);
			this.m_A = new global::b.A(this.m_A.A, !P_0);
			this.m_A = new global::a.z(this.m_A.A, !P_0, this.m_A.A);
			this.m_A = new global::A.h(this.m_A.A, !P_0, this.m_A.A);
			n();
			this.m_A.A = m;
			this.m_A.Icon = global::a.I.A;
			this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cl();
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
						C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fO(),
						(Image)null,
						(EventHandler)delegate
						{
							e();
						}
					},
					{
						C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fo(),
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
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fP();
			}
			if (P_0)
			{
				N n2 = new N
				{
					A = 1u,
					A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fp(),
					a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz(),
					A = 450f,
					B = 120f,
					a = 80f,
					C = 52f,
					b = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FT(),
					d = true
				};
				n2.A.Add(new global::b.J
				{
					A = 1,
					A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fQ(),
					A = true
				});
				n2.A.Add(new global::b.J
				{
					A = 2,
					A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fq(),
					A = false
				});
				n2.A.AddRange(new string[3]
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bb(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Fu(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FV()
				});
				N n3 = new N
				{
					A = 2u,
					A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fR(),
					a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz(),
					A = 360f,
					B = 90f,
					a = 50f
				};
				n3.A.Add(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fr());
				this.m_A = new List<N> { n2, n3 };
				this.m_a = true;
				this.m_a.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fS());
				this.m_a.A(n3, false);
				g();
			}
		}

		public void c()
		{
			if (!this.m_B && global::a.N.A)
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
			if (this.m_B || !global::a.N.A)
			{
				return;
			}
			uint num = (uint)(Keys)Enum.Parse(typeof(Keys), this.m_A.Text);
			uint num2 = (new uint[5] { 0u, 2u, 1u, 4u, 3u })[this.m_a.SelectedIndex] | 0x4000;
			if (this.m_b)
			{
				global::B.G.A(base.Handle, 201);
			}
			if (!global::B.G.A(base.Handle, 201, num2, num))
			{
				this.m_b = this.m_A != 0 && global::B.G.A(base.Handle, 201, this.m_a, this.m_A);
				MessageBox.Show(this, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fs(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fT());
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
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ft() + ((this.m_a.SelectedIndex == 0) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D() : (this.m_a.Text + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fU())) + this.m_A.Text;
			}
			catch (Exception ex)
			{
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fu() + ex.Message;
			}
		}

		private void E()
		{
			if (this.m_B || !global::a.N.A)
			{
				return;
			}
			h();
			if (!this.m_a)
			{
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fV();
				return;
			}
			string text = string.Join(Environment.NewLine, global::B.j.B(this.m_a.A(this.m_A), this.m_A));
			if (text.Length == 0)
			{
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fv();
				return;
			}
			try
			{
				Clipboard.SetText(text, TextDataFormat.UnicodeText);
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fW() + this.m_a.A(this.m_A).Count() + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fw();
			}
			catch (ExternalException)
			{
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fX();
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
				global::B.G.A(global::B.G.A(), out var num);
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

		private IEnumerable<N> F()
		{
			return this.m_A.Where((N P_0) => this.m_a.a(P_0) || (this.m_A.c && (P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bA() || P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DL())));
		}

		private void f()
		{
			if (this.m_a && global::a.N.A)
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
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fx() + ex.Message;
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
			global::A.y[] array = (from P_0 in this.m_A
				where P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()
				orderby P_0.A
				select new global::A.y
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
					flag = ((global::A.y)this.m_A.Items[num]).ToString() == array[num].ToString();
					num++;
				}
				if (!flag)
				{
					this.m_A.Items.Clear();
					global::A.y[] array2 = array;
					foreach (global::A.y y2 in array2)
					{
						this.m_A.Items.Add(y2, this.m_a.a(y2.A));
					}
				}
				else
				{
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						((global::A.y)this.m_A.Items[num3]).A = array[num3].A;
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
			if (this.m_a && global::a.N.A)
			{
				this.m_A.A(this.m_A, F());
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fY() + this.m_a.A(this.m_A).Count() + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cd() + this.m_A.Count((N P_0) => P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fy();
			}
			this.m_A.Invalidate();
		}

		private void A(bool P_0)
		{
			foreach (N item in this.m_A.Where((N n2) => n2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
			{
				this.m_a.A(item, P_0);
			}
			g();
			H();
		}

		private void h()
		{
			if (!global::a.N.A)
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
					Process[] processesByName = Process.GetProcessesByName(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fZ());
					try
					{
						if (processesByName.Length != 1)
						{
							throw new Exception(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fz());
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
					this.m_A = new m();
				}
				this.m_A = this.m_A.i();
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
				A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GA(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ga(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GB(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gb(), delegate
				{
					this.m_A.A(this.m_A, this.m_A, this.m_A);
				});
				this.m_a.A(this.m_A.b);
				this.m_A.A(this.m_A.b);
				this.m_a = true;
				P();
				g();
				H();
				foreach (global::a.E item in this.m_A.A(this.m_A.b, this.m_A, this.m_a))
				{
					this.m_A.A(item.A, false);
					this.m_A.Text = item.A;
				}
				foreach (global::a.E item2 in this.m_A.A(this.m_A.b, this.m_A, this.m_A))
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
					this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GC() + this.m_A.A;
				}
				if (this.m_A.A.Length > 0)
				{
					this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gc() + this.m_A.A;
				}
				if (this.m_A.A.a && this.m_A.B.Length > 0)
				{
					this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GD() + this.m_A.B;
				}
				if (this.m_A.A.Length > 0)
				{
					this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gd() + this.m_A.A;
				}
				if (this.m_A.A && this.m_A.b.Length > 0)
				{
					this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GE() + this.m_A.b;
				}
			}
			catch (Exception ex2)
			{
				bool flag = this.m_A != null && this.m_a && this.m_A.J();
				if (this.m_A.A(flag, DateTime.UtcNow))
				{
					this.m_A.Interval = 100;
					global::B.H.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ge(), ex2.Message);
					this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GF() + ex2.Message;
				}
				else
				{
					global::B.H.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gf(), ex2.Message);
					G();
					this.m_A.Interval = 1000;
					this.m_A = DateTime.UtcNow.AddSeconds(1.0);
					this.m_A.Text = ex2.Message + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GG();
				}
			}
			finally
			{
				p();
				if (stopwatch.ElapsedMilliseconds > 1000)
				{
					global::B.H.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gg(), stopwatch.ElapsedMilliseconds + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GH());
				}
			}
		}

		private void A(object P_0, PaintEventArgs P_1)
		{
			string[] array = ((this.m_a && global::a.N.A) ? global::B.j.B(F(), this.m_A) : new string[2]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gh(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GI()
			});
			if (this.m_a && array.Length == 0)
			{
				array = new string[1] { C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gi() };
			}
			using Font font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), this.m_A.B, FontStyle.Bold);
			global::B.h.A(P_1.Graphics, array, font, this.m_A.B, this.m_A.b, this.m_A.Width, true, this.m_A.a, this.m_a ? global::B.j.A(F(), this.m_A) : null, this.m_a ? global::B.j.a(F(), this.m_A) : null);
		}

		protected override void OnFormClosed(FormClosedEventArgs P_0)
		{
			if (this.m_b)
			{
				global::B.G.A(base.Handle, 201);
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
						if (text2 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GJ())
						{
							this.m_E = text3 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BW();
						}
						if (text2 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gj() && decimal.TryParse(text3, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
						{
							this.m_A = Math.Max(0.5m, Math.Min(12m, result));
						}
						if (text2 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bv())
						{
							this.m_C = text3 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BW();
						}
						if (text2 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GK() && int.TryParse(text3, out var result2))
						{
							this.m_a = Math.Max(5, Math.Min(500, result2));
						}
						if (text2 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gk() && int.TryParse(text3, out result2))
						{
							this.m_B = Math.Max(3, Math.Min(120, result2));
						}
						if (text2 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GL())
						{
							this.m_A = text3;
						}
						if (text2 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gl())
						{
							this.m_a = text3;
						}
					}
				}
			}
			catch (IOException)
			{
			}
			global::B.D d2 = this.A(2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GM(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gm());
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GN(), () => this.m_C, delegate(bool P_0)
			{
				this.m_C = P_0;
				if (!P_0)
				{
					k();
				}
				i();
			}, 146);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gn(), this.m_a, 5m, 500m, delegate(decimal P_0)
			{
				this.m_a = (int)P_0;
				i();
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GO(), this.m_B, 3m, 120m, delegate(decimal P_0)
			{
				this.m_B = (int)P_0;
				i();
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Go(), () => this.m_E, delegate(bool P_0)
			{
				this.m_E = P_0;
				i();
			}, 146);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GP(), this.m_A, 0.5m, 12m, delegate(decimal P_0)
			{
				this.m_A = P_0;
				i();
			}, 1);
			TextBox A = new TextBox
			{
				Text = this.m_A,
				Width = 150,
				BackColor = global::B.K.A,
				ForeColor = global::B.K.b,
				BorderStyle = BorderStyle.FixedSingle
			};
			d2.A.Controls.Add(A);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gp(), delegate
			{
				this.A(A.Text);
				i();
			}, 150);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GQ(), delegate
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XX()
				};
				if (openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					using (new global::A.Q(openFileDialog.FileName))
					{
					}
					this.m_a = openFileDialog.FileName;
					i();
					this.m_a.Text = Path.GetFileName(openFileDialog.FileName);
				}
			}, 145);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gq(), K, 65);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GR(), delegate
			{
				this.m_a = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
				i();
				this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xx() + Path.GetFileName(global::A.v.A);
			}, 100);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gr(), k, 145);
			this.m_a = this.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GS(), 390, global::B.K.C);
			this.A(d2.A, this.m_a, 48);
			this.m_A.SetToolTip(A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gs());
			this.m_A = new global::a.b();
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
				if (global::a.N.A)
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
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GT() + (this.m_E ? 1 : 0),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gt() + this.m_A.ToString(CultureInfo.InvariantCulture),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bx() + (this.m_C ? 1 : 0),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GU() + this.m_a,
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gu() + this.m_B,
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GV() + this.m_A,
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gv() + this.m_a
				});
			}
			catch (Exception ex)
			{
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GW() + ex.Message;
			}
		}

		private void A(string P_0)
		{
			string[] array = P_0.Split('+');
			uint num = 16384u;
			foreach (string item in array.Take(array.Length - 1))
			{
				if (item.Trim().Equals(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gw(), StringComparison.OrdinalIgnoreCase))
				{
					num |= 2;
					continue;
				}
				if (item.Trim().Equals(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GX(), StringComparison.OrdinalIgnoreCase))
				{
					num |= 1;
					continue;
				}
				if (item.Trim().Equals(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gx(), StringComparison.OrdinalIgnoreCase))
				{
					num |= 4;
					continue;
				}
				throw new Exception(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GY());
			}
			if (!Enum.TryParse<Keys>(array.Last().Trim(), ignoreCase: true, out var result) || result == Keys.None || result > (Keys.F16 | Keys.F17))
			{
				throw new Exception(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gy());
			}
			if (this.m_B || !global::a.N.A)
			{
				return;
			}
			uint num2 = this.m_B;
			uint num3 = this.m_b;
			global::B.G.A(base.Handle, 203);
			if (!global::B.G.A(base.Handle, 203, num, (uint)result))
			{
				if (num3 != 0)
				{
					global::B.G.A(base.Handle, 203, num2, num3);
				}
				throw new Exception(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.GZ());
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
			global::B.G.A(global::B.G.A(), out var num);
			if (global::a.N.A && num == (uint)this.m_A && this.m_C)
			{
				this.m_a = DateTime.UtcNow.AddSeconds(5.0);
				this.m_D = (A(1) & 0x8000) != 0;
				this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gz();
			}
			return true;
		}

		private void J()
		{
			if (this.m_A == null || !this.m_C)
			{
				return;
			}
			if (this.m_B != this.m_A.b)
			{
				k();
				this.m_B = this.m_A.b;
			}
			foreach (N item in this.m_A)
			{
				foreach (global::b.c item2 in item.A)
				{
					item2.A = true;
					item2.A = (item.b ? item.A : Color.White);
				}
			}
			this.m_A.D = true;
			try
			{
				this.m_A = this.m_A.A(this.m_A.b, this.m_A.Where((N P_0) => P_0.C).SelectMany((N P_0) => P_0.A));
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
			if (this.m_A.Any((N P_0) => !P_0.C))
			{
				this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gA();
				return;
			}
			if (this.m_A != 0L)
			{
				Z z2 = this.m_A.FirstOrDefault((Z P_0) => P_0.A == this.m_A && P_0.a == this.m_a);
				if (z2 == null)
				{
					k();
					this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ga();
					return;
				}
				this.m_A = z2.A;
				this.m_a = z2.a;
				this.m_B = z2.B;
			}
			this.m_A = global::a.A.A(this.m_A, this.m_A, this.m_A, this.m_B, this.m_a);
			global::a.c c2 = this.m_A;
			this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gB() + c2.A + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gb() + c2.a + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gC() + c2.B + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gb() + c2.b;
			if (c2.A && DateTime.UtcNow >= this.m_b)
			{
				K();
				this.m_b = DateTime.UtcNow.AddSeconds(this.m_B);
			}
			if (!c2.A)
			{
				this.m_b = DateTime.MinValue;
			}
		}

		private void j()
		{
			if (!global::a.N.A || this.m_A == null || !this.m_C || (!this.m_c && this.m_a <= DateTime.UtcNow))
			{
				this.m_A.Hide();
				return;
			}
			IntPtr intPtr = global::B.G.A();
			global::B.G.A(intPtr, out var num);
			if (num != (uint)this.m_A || !global::B.J.A(intPtr, out var A) || A.B < 200)
			{
				this.m_A.Hide();
				return;
			}
			bool flag = (global::B.J.A(1) & 0x8000) != 0;
			try
			{
				if (this.m_a > DateTime.UtcNow && (global::B.J.A(27) & 0x8000) != 0)
				{
					this.m_a = DateTime.MinValue;
				}
				global::a.v A2 = this.m_A.e();
				if (this.m_a > DateTime.UtcNow && flag && !this.m_D)
				{
					this.m_a = DateTime.MinValue;
					Point A3 = Cursor.Position;
					global::B.J.A(intPtr, ref A3);
					if (A3.X < 0 || A3.Y < 0 || A3.X >= A.B || (double)A3.Y >= (double)A.b * 0.78 || DateTime.UtcNow - this.m_B > TimeSpan.FromSeconds(3.0))
					{
						throw new Exception(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gc());
					}
					A<Z, double> a2 = (from P_0 in this.m_A.Select(delegate(Z P_0)
						{
							PointF pointF;
							bool flag2 = A2.A(P_0.A, P_0.a + 0.8f, P_0.B, A.B, A.b, out pointF);
							return new A<Z, double>(P_0, flag2 ? ((double)((pointF.X - (float)A3.X) * (pointF.X - (float)A3.X) + (pointF.Y - (float)A3.Y) * (pointF.Y - (float)A3.Y))) : double.MaxValue);
						})
						orderby P_0.a
						select P_0).ToArray().FirstOrDefault();
					if (a2 != null && a2.a < 1936.0)
					{
						Z z2 = a2.A;
						this.m_A = z2.A;
						this.m_a = z2.a;
						this.m_B = z2.B;
						this.m_A = z2.A;
						this.m_a = z2.a;
						this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gD() + z2.a;
					}
					else
					{
						this.m_a = ((a2 != null && a2.a < double.MaxValue) ? a2.A.a : 0f);
						if (!global::a.A.A(A2, A3, A.B, A.b, this.m_a, out this.m_A, out this.m_B))
						{
							throw new Exception(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gd());
						}
						this.m_A = 0L;
						this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gE();
					}
					this.m_c = true;
					this.m_b = DateTime.MinValue;
					this.m_A = null;
				}
				if (global::b.I.A(this.m_A, this.m_A.b))
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
				this.m_A.A = global::a.C.A(this.m_A);
				this.m_A.A = this.m_A != null && this.m_A.A;
				this.m_A.A = (this.m_A.A ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ge() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dl()) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gF() + this.m_a;
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
			if (this.m_B || !global::a.N.A)
			{
				return;
			}
			long A = global::a.N.a;
			string A2 = (string.IsNullOrWhiteSpace(this.m_a) ? global::A.v.A : this.m_a);
			ThreadPool.QueueUserWorkItem(delegate
			{
				try
				{
					if (global::a.N.A && global::a.N.a == A)
					{
						if (!string.IsNullOrWhiteSpace(A2))
						{
							using (global::A.Q q2 = new global::A.Q(A2))
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
							this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XY();
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
				this.m_a.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gf();
			}
		}

		private void L()
		{
			this.m_B.Stop();
			this.m_B.Dispose();
			if (this.m_d)
			{
				global::B.G.A(base.Handle, 203);
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
			if (P_1 == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.dn())
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gG();
			}
			try
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gg() + new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(P_0).ToLocalTime().ToString(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ej());
			}
			catch (ArgumentOutOfRangeException)
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
			}
		}

		public void l()
		{
			if (this.m_B || !global::a.N.A)
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
					global::B.G.A(base.Handle, 201);
					this.m_b = false;
				}
				if (this.m_d)
				{
					global::B.G.A(base.Handle, 203);
					this.m_d = false;
				}
				this.m_A.b();
				G();
				global::A.Q.A();
			}
		}

		private void A(Control P_0)
		{
			A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gH(), delegate
			{
				this.m_b?.Invoke();
			}, 150);
			A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gh(), delegate
			{
				using global::a.O o2 = new global::a.O(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IA(), string.IsNullOrEmpty(this.m_b) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ia() : this.m_b, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.dV());
				o2.ShowDialog(this);
			}, 150);
		}

		private global::a.B m()
		{
			if (!this.m_C || !this.m_c || !this.m_E || this.m_A == null || this.m_B != this.m_A.b || DateTime.UtcNow - this.m_B > TimeSpan.FromSeconds(3.0))
			{
				return null;
			}
			return new global::a.B
			{
				A = this.m_A,
				a = this.m_B,
				B = this.m_a,
				b = (float)this.m_A,
				A = global::a.C.A(this.m_A)
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
				global::B.H.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gI(), P_0 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + ex.Message);
				this.m_A.Text = P_0 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gi() + ex.Message;
			}
		}

		[DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
		private static extern bool N();

		[DllImport("user32.dll", EntryPoint = "SendMessage")]
		private static extern IntPtr A(IntPtr P_0, int P_1, IntPtr P_2, IntPtr P_3);

		private void n()
		{
			base.AutoScaleMode = AutoScaleMode.None;
			Font = global::a.I.A();
			base.Icon = global::a.I.A;
			BackColor = global::B.K.A;
			ForeColor = global::B.K.b;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Padding = new Padding(8);
			base.MaximizedBounds = Screen.FromControl(this).WorkingArea;
			global::b.g g2 = new global::b.g
			{
				Dock = DockStyle.Fill,
				Padding = new Padding(10)
			};
			base.Controls.Add(g2);
			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 1,
				RowCount = 5,
				Padding = Padding.Empty,
				Margin = Padding.Empty,
				BackColor = global::B.K.A
			};
			g2.Controls.Add(tableLayoutPanel);
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
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.x(),
				Font = global::a.I.A(21f, FontStyle.Bold),
				ForeColor = global::B.K.b,
				AutoSize = true,
				Location = new Point(4, 4)
			};
			panel.Controls.Add(label);
			Label label2 = new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gJ(),
				Font = global::a.I.A(9f),
				ForeColor = global::B.K.C,
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
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gj(),
				Width = 140,
				Height = 34,
				ForeColor = global::B.K.d,
				TextAlign = ContentAlignment.MiddleRight
			};
			flowLayoutPanel.Controls.Add(this.m_B);
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gK(), delegate
			{
				Hide();
			}, 52);
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gk(), delegate
			{
				base.WindowState = ((base.WindowState != FormWindowState.Maximized) ? FormWindowState.Maximized : FormWindowState.Normal);
				O();
			}, 42);
			A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Y(), delegate
			{
				Close();
			}, 42);
			FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				WrapContents = false,
				Padding = new Padding(8, 4, 0, 0),
				BackColor = global::B.K.B
			};
			tableLayoutPanel.Controls.Add(flowLayoutPanel2, 0, 1);
			flowLayoutPanel2.Controls.Add(A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gL(), 90));
			A(flowLayoutPanel2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gl(), () => this.m_A.D, delegate(bool P_0)
			{
				this.m_A.D = P_0;
			}, 210);
			A(flowLayoutPanel2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gM(), () => this.m_A.A.B, delegate(bool P_0)
			{
				this.m_A.A.B = P_0;
			}, 210);
			A(flowLayoutPanel2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gm(), () => this.m_A.A.a, delegate(bool P_0)
			{
				this.m_A.A.a = P_0;
			}, 210);
			flowLayoutPanel2.Controls.Add(A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gN(), 165, global::B.K.C));
			TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
			{
				Dock = DockStyle.Fill,
				ColumnCount = 3,
				RowCount = 1,
				Margin = new Padding(0, 10, 0, 0),
				BackColor = global::B.K.A
			};
			tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 2);
			for (int num3 = 0; num3 < 3; num3++)
			{
				tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
				this.m_A[num3] = new global::b.d();
				tableLayoutPanel2.Controls.Add(this.m_A[num3], num3, 0);
			}
			global::B.D d2 = A(0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gn(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gO());
			a(d2.A);
			global::B.D d3 = A(1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.go(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ga());
			B(d3.A);
			global::B.D d4 = A(2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gP(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gp());
			b(d4.A);
			d2 = A(0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gQ(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gq());
			C(d2.A);
			d3 = A(1, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gR(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gr());
			c(d3.A);
			d4 = A(2, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gS(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gs());
			D(d4.A);
			this.m_A.Dock = DockStyle.Fill;
			this.m_A.Paint += A;
			this.m_A.Margin = new Padding(0, 8, 8, 0);
			tableLayoutPanel.Controls.Add(this.m_A, 0, 3);
			this.m_A.Dock = DockStyle.Fill;
			this.m_A.ForeColor = global::B.K.C;
			this.m_A.Font = global::a.I.A(8f);
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
			global::b.d[] array = this.m_A;
			for (int num = 0; num < array.Length; num++)
			{
				array[num]?.B();
			}
		}

		private global::B.D A(int P_0, string P_1, string P_2)
		{
			global::B.D d2 = new global::B.D(P_1, P_2);
			this.m_A[P_0].Controls.Add(d2);
			return d2;
		}

		private Label A(string P_0, int P_1 = 160, Color? P_2 = null)
		{
			return new Label
			{
				Text = P_0,
				ForeColor = (P_2 ?? global::B.K.b),
				Width = P_1,
				Height = 30,
				TextAlign = ContentAlignment.MiddleLeft,
				Font = global::a.I.A(9f),
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

		private global::b.i A(Control P_0, string P_1, Func<bool> P_2, Action<bool> P_3, int P_4 = 146)
		{
			global::b.i A = new global::b.i
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
			this.m_A.Add(new KeyValuePair<global::b.i, Func<bool>>(A, P_2));
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
			flowLayoutPanel.Controls.Add(A(P_1, 73, global::B.K.C));
			global::b.e e2 = new global::b.e(P_3, P_4, P_2, P_6)
			{
				Margin = new Padding(0, 3, 0, 0)
			};
			flowLayoutPanel.Controls.Add(e2);
			e2.A = delegate(decimal obj)
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

		private global::a.d A(Control P_0, string P_1, bool P_2 = false)
		{
			global::a.d d2 = new global::a.d(P_1);
			P_0.Controls.Add(d2);
			d2.A = P_2;
			return d2;
		}

		private global::a.d A(Control P_0, string P_1, Func<bool> P_2, Action<bool> P_3)
		{
			global::a.d A = this.A(P_0, P_1);
			A.A = true;
			A.A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
			global::b.i A2 = this.A(A.A, P_1, P_2, P_3, 270);
			A2.Location = new Point(4, 0);
			A.A.SizeChanged += delegate
			{
				A2.Width = A.A.Width - 50;
			};
			Label label = new Label
			{
				BackColor = global::B.K.a,
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gT(),
				ForeColor = global::B.K.D,
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
			P_1.Tag = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Do();
			P_1.Width = 390;
			P_1.Height = P_2;
			P_1.Margin = new Padding(0, 4, 0, 6);
			P_0.Controls.Add(P_1);
		}

		private void a(Control P_0)
		{
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gt(), () => this.m_A.b, delegate(bool flag)
			{
				this.m_A.b = flag;
			}, 146);
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gU(), () => this.m_A.C, delegate(bool flag)
			{
				this.m_A.C = flag;
			}, 146);
			this.m_A.CheckOnClick = true;
			this.m_A.IntegralHeight = false;
			this.m_A.BackColor = global::B.K.A;
			this.m_A.ForeColor = global::B.K.b;
			this.m_A.BorderStyle = BorderStyle.FixedSingle;
			this.m_A.Font = global::a.I.A(9f);
			this.A(P_0, this.m_A, 90);
			this.m_A.ItemCheck += delegate(object obj, ItemCheckEventArgs e2)
			{
				if (!this.m_A)
				{
					this.m_a.A(((global::A.y)this.m_A.Items[e2.Index]).A, e2.NewValue == CheckState.Checked);
					H();
				}
			};
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gu(), this.m_A.B, 9m, 36m, delegate(decimal num2)
			{
				this.m_A.B = (int)num2;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gV(), this.m_A.a, 9m, 36m, delegate(decimal num2)
			{
				this.m_A.a = (int)num2;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gv(), () => this.m_A.B, delegate(Color color)
			{
				this.m_A.B = color;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gW(), () => this.m_A.b, delegate(Color color)
			{
				this.m_A.b = color;
			});
			global::a.d d2 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gw());
			string[] array = new string[10]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gX(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gx(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gY(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gy(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gZ(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.cD(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gz(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HA(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ha(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HB()
			};
			for (int num = 0; num < array.Length; num++)
			{
				int A = num;
				this.A(d2.A, array[A], () => this.m_A.A[A], delegate(bool flag)
				{
					this.m_A.A[A] = flag;
				}, 146);
			}
			global::a.d d3 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hb());
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HC(), () => this.m_A.c, delegate(bool flag)
			{
				this.m_A.c = flag;
			}, 146);
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hc(), () => this.m_A.A, delegate(bool flag)
			{
				this.m_A.A = flag;
			}, 146);
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HD(), this.m_A.b, 0m, 300m, delegate(decimal num2)
			{
				this.m_A.b = (int)num2;
			});
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hd(), this.m_A.C, 40m, 100m, delegate(decimal num2)
			{
				this.m_A.C = (int)num2;
			});
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HE(), delegate
			{
				this.A(true);
			}, 145);
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.He(), delegate
			{
				this.A(false);
			}, 145);
			global::a.d d4 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HF());
			foreach (KeyValuePair<string, X> item in this.m_A.A.A())
			{
				string text = ((item.Key == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FL()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gX() : ((item.Key == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Fl()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gx() : ((item.Key == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FM()) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gY() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gy())));
				X A2 = item.Value;
				global::a.d d5 = this.A(d4.A, text);
				this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hf(), () => A2.A, delegate(bool flag)
				{
					A2.A = flag;
				}, 146);
				this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HG(), () => A2.A, delegate(Color color)
				{
					A2.A = color;
				});
				this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hg(), () => A2.a, delegate(Color color)
				{
					A2.a = color;
				});
				this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HH(), () => A2.B, delegate(Color color)
				{
					A2.B = color;
				});
				this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hh(), A2.A, 1m, 1000000m, delegate(decimal num2)
				{
					A2.A = num2;
				});
				this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HI(), A2.a, 0m, 1000000m, delegate(decimal num2)
				{
					A2.a = num2;
				});
				this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hi(), A2.B, 0.1m, 5m, delegate(decimal num2)
				{
					A2.B = num2;
				}, 1);
			}
		}

		private void B(Control P_0)
		{
			global::b.b A = this.m_A.A;
			global::a.d d2 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HJ(), true);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hj(), A.c, 8m, 200m, delegate(decimal num)
			{
				A.c = (int)num;
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HK(), A.D, 8m, 200m, delegate(decimal num)
			{
				A.D = (int)num;
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hk(), A.C, 0.5m, 12m, delegate(decimal num)
			{
				A.C = num;
			}, 1);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HL(), A.d, 8m, 36m, delegate(decimal num)
			{
				A.d = (int)num;
			});
			global::a.d d3 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hl(), () => A.a, delegate(bool flag)
			{
				A.a = flag;
			});
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HM(), A.A, 8m, 160m, delegate(decimal num)
			{
				A.A = (int)num;
			});
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hm(), A.A, 0.5m, 12m, delegate(decimal num)
			{
				A.A = num;
			}, 1);
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HN(), A.a, 1m, 100m, delegate(decimal num)
			{
				A.a = num;
			}, 1);
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hn(), A.B, 1m, 100m, delegate(decimal num)
			{
				A.B = num;
			}, 1);
			global::a.d d4 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HO(), () => A.C, delegate(bool flag)
			{
				A.C = flag;
			});
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ho(), () => A.d, delegate(bool flag)
			{
				A.d = flag;
			}, 146);
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HP(), () => A.c, delegate(bool flag)
			{
				A.c = flag;
			}, 146);
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hp(), () => A.D, delegate(bool flag)
			{
				A.D = flag;
			}, 146);
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HQ(), () => A.E, delegate(bool flag)
			{
				A.E = flag;
			}, 146);
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hq(), A.a, 1m, 200m, delegate(decimal num)
			{
				A.a = (int)num;
			});
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HR(), A.B, 1m, 200m, delegate(decimal num)
			{
				A.B = (int)num;
			});
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hr(), A.b, 1m, 200m, delegate(decimal num)
			{
				A.b = (int)num;
			});
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HS(), A.b, 0.5m, 12m, delegate(decimal num)
			{
				A.b = num;
			}, 1);
			this.A(d4.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hs(), A.C, 0m, 180m, delegate(decimal num)
			{
				A.C = (int)num;
			});
			global::a.d d5 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HT(), () => this.m_A.a, delegate(bool flag)
			{
				this.m_A.a = flag;
			});
			this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ht(), () => this.m_A.A, delegate(Color color)
			{
				this.m_A.A = color;
			});
			this.A(d5.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HU(), () => this.m_A.a, delegate(Color color)
			{
				this.m_A.a = color;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hu(), A.E, 10m, 144m, delegate(decimal num)
			{
				A.E = (int)num;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HV(), A.e, 5m, 144m, delegate(decimal num)
			{
				A.e = (int)num;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hv(), delegate
			{
				A.b = !A.b;
				f();
				this.m_A.Text = (A.b ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XZ() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xy());
			}, 80);
		}

		private void b(Control P_0)
		{
			global::A.J A = this.m_A.A.A;
			s A2 = this.m_A.A.A;
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HW(), () => this.m_A.B, delegate(bool flag)
			{
				this.m_A.B = flag;
			}, 146);
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hw(), this.m_A.A, 8m, 240m, delegate(decimal num)
			{
				this.m_A.A = (int)num;
			});
			global::a.d d2 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HX(), () => A2.B, delegate(bool flag)
			{
				A2.B = flag;
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hx(), () => A2.A, delegate(bool flag)
			{
				A2.A = flag;
			}, 146);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HY(), () => A2.a, delegate(bool flag)
			{
				A2.a = flag;
			}, 146);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hy(), () => A2.b, delegate(bool flag)
			{
				A2.b = flag;
			}, 146);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HZ(), A2.A, 8m, 72m, delegate(decimal num)
			{
				A2.A = (int)num;
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HL(), A2.a, 8m, 36m, delegate(decimal num)
			{
				A2.a = (int)num;
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hz(), A2.A, 0.5m, 8m, delegate(decimal num)
			{
				A2.A = num;
			}, 1);
			global::a.d d3 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hA(), () => A.A, delegate(bool flag)
			{
				A.A = flag;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ha(), () => A.a, delegate(bool flag)
			{
				A.a = flag;
			}, 146);
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hB(), A.A, 0m, 100m, delegate(decimal num)
			{
				A.A = (int)num;
			});
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hb(), A.B, 8m, 40m, delegate(decimal num)
			{
				A.B = (int)num;
			});
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hC(), A.A, 0.5m, 8m, delegate(decimal num)
			{
				A.A = num;
			}, 1);
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hc(), A.b, 6m, 24m, delegate(decimal num)
			{
				A.b = (int)num;
			});
			this.A(d3.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hD(), A.a, 0m, 100m, delegate(decimal num)
			{
				A.a = (int)num;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hd(), delegate
			{
				this.m_A.C();
				this.m_A.b();
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xz();
			}, 210);
		}

		private void C(Control P_0)
		{
			global::B.B A = this.m_A.A;
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hE(), () => this.m_A.A.A, delegate(bool flag)
			{
				this.m_A.A.A = flag;
			}, 146);
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.he(), this.m_A.A.A, 8m, 40m, delegate(decimal num)
			{
				this.m_A.A.A = (int)num;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hF(), () => A.B, delegate(bool flag)
			{
				A.B = flag;
			}, 146);
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hf(), () => A.b, delegate(bool flag)
			{
				A.b = flag;
			}, 146);
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hG(), () => this.m_A.A.A.B, delegate(bool flag)
			{
				this.m_A.A.A.B = flag;
			}, 146);
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hg(), A.b, 1m, 60m, delegate(decimal num)
			{
				A.b = (int)num;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hH(), A.a, 2m, 40m, delegate(decimal num)
			{
				A.a = (int)num;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hh(), A.B, 3m, 60m, delegate(decimal num)
			{
				A.B = (int)num;
			});
			global::a.d d2 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hI());
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Hm(), A.A, 0.5m, 8m, delegate(decimal num)
			{
				A.A = num;
			}, 1);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.HM(), A.C, 2m, 40m, delegate(decimal num)
			{
				A.C = (int)num;
			});
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hi(), A.c, 4m, 60m, delegate(decimal num)
			{
				A.c = (int)num;
			});
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hJ(), o, 230);
		}

		private void c(Control P_0)
		{
			this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hj(), () => this.m_A.A, delegate(bool flag)
			{
				this.m_A.A = flag;
				this.m_A.B();
			}, 146);
			P_0.Controls.Add(this.A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hK(), 110, global::B.K.C));
			foreach (global::A.L value in Enum.GetValues(typeof(global::A.L)))
			{
				global::A.L A = value;
				FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
				{
					Tag = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Do(),
					Width = 390,
					Height = 34,
					WrapContents = false,
					Margin = new Padding(0, 1, 0, 1)
				};
				P_0.Controls.Add(flowLayoutPanel);
				string A2 = ((A == global::A.L.A) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hn() : ((A == global::A.L.a) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hN() : ((A == global::A.L.B) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hm() : ((A == global::A.L.C) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hM() : ((A == global::A.L.c) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hl() : ((A == global::A.L.D) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hL() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hk()))))));
				flowLayoutPanel.Controls.Add(this.A(A2, 110));
				global::A.D A3 = this.A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hO(), delegate
				{
					using OpenFileDialog openFileDialog = new OpenFileDialog
					{
						Title = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xA() + A2 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hO(),
						Filter = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xa()
					};
					if (openFileDialog.ShowDialog(this) == DialogResult.OK)
					{
						this.m_A.A(A, openFileDialog.FileName);
						this.m_A.Text = A2 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gi() + this.m_A.a(A);
						p();
					}
				}, 62);
				this.m_A.SetToolTip(A3, this.m_A.a(A));
				this.A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Gq(), delegate
				{
					this.m_A.A(A, true);
				}, 52);
				this.A(flowLayoutPanel, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ho(), delegate
				{
					this.m_A.A(A, null);
					this.m_A.SetToolTip(A3, this.m_A.a(A));
					this.m_A.Text = A2 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gi() + this.m_A.a(A);
				}, 52);
			}
			global::a.d d2 = this.A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hP());
			this.m_a.CheckOnClick = true;
			this.m_a.IntegralHeight = false;
			this.m_a.BorderStyle = BorderStyle.FixedSingle;
			this.m_a.BackColor = global::B.K.A;
			this.m_a.ForeColor = global::B.K.b;
			this.A(d2.A, this.m_a, 100);
			this.m_a.ItemCheck += delegate(object obj, ItemCheckEventArgs e2)
			{
				if (!this.m_F)
				{
					this.m_A.A(((global::A.y)this.m_a.Items[e2.Index]).A, e2.NewValue == CheckState.Checked);
				}
			};
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bP(), delegate
			{
				foreach (N item in this.m_A.Where((N n2) => n2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
				{
					this.m_A.A(item, true);
				}
				Q();
			}, 145);
			this.A(d2.A, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bp(), delegate
			{
				foreach (N item2 in this.m_A.Where((N n2) => n2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
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
			Color color = (this.m_A.BackColor = global::B.K.B);
			Color backColor = color;
			comboBox3.BackColor = backColor;
			ComboBox comboBox4 = this.m_a;
			color = (this.m_A.ForeColor = global::B.K.b);
			Color foreColor = color;
			comboBox4.ForeColor = foreColor;
			this.m_a.Items.AddRange(new object[5]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hp(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hQ(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hq(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hR(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hr()
			});
			this.m_a.SelectedIndex = 0;
			for (int num = 1; num <= 12; num++)
			{
				this.m_A.Items.Add(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hS() + num);
			}
			for (char c2 = 'A'; c2 <= 'Z'; c2 = (char)(c2 + 1))
			{
				this.m_A.Items.Add(c2.ToString());
			}
			this.m_A.SelectedItem = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hs();
			D();
			ComboBox[] array = new ComboBox[2] { this.m_a, this.m_A };
			foreach (ComboBox obj in array)
			{
				obj.DrawMode = DrawMode.OwnerDrawFixed;
				obj.Font = global::a.I.A(9f);
				obj.ItemHeight = 22;
				obj.DrawItem += delegate(object obj2, DrawItemEventArgs e2)
				{
					if (e2.Index >= 0)
					{
						ComboBox comboBox5 = (ComboBox)obj2;
						using (SolidBrush brush = new SolidBrush(((e2.State & DrawItemState.Selected) != DrawItemState.None) ? global::B.K.B : global::B.K.A))
						{
							e2.Graphics.FillRectangle(brush, e2.Bounds);
						}
						TextRenderer.DrawText(e2.Graphics, comboBox5.Items[e2.Index].ToString(), comboBox5.Font, e2.Bounds, global::B.K.b, TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
					}
				};
			}
			P_0.Controls.Add(A(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hT(), 130));
			P_0.Controls.Add(this.m_a);
			P_0.Controls.Add(this.m_A);
			A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ht(), d, 150);
			A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hU(), E, 150);
			A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hu(), delegate
			{
				this.m_A.b();
				this.m_A.B();
				this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IB();
			}, 298);
			A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hV(), delegate
			{
				Hide();
			}, 298);
			global::a.d d2 = A(P_0, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hv());
			A(d2.A, new Label
			{
				Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hW(),
				ForeColor = global::B.K.C,
				Font = global::a.I.A(9f)
			}, 110);
		}

		private void o()
		{
			this.m_A.a();
			this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hw();
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
				foreach (KeyValuePair<global::b.i, Func<bool>> item in this.m_A)
				{
					if (!item.Key.IsDisposed)
					{
						item.Key.A = item.Value();
					}
				}
				if (this.m_B != null)
				{
					this.m_B.Text = (this.m_B ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hY() : (global::a.F.A ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hx() : (this.m_a ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hX() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.gj())));
					this.m_A.SetToolTip(this.m_B, global::a.F.A ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hy() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
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
				N[] array = (from P_0 in this.m_A
					where P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()
					orderby P_0.A
					select P_0).ToArray();
				bool flag = this.m_a.Items.Count == array.Length;
				int num = 0;
				while (flag && num < array.Length)
				{
					flag = ((global::A.y)this.m_a.Items[num]).A.A == array[num].A && ((global::A.y)this.m_a.Items[num]).A.A == array[num].A;
					num++;
				}
				if (!flag)
				{
					this.m_a.Items.Clear();
					N[] array2 = array;
					foreach (N n2 in array2)
					{
						this.m_a.Items.Add(new global::A.y
						{
							A = n2
						}, this.m_A.a(n2));
					}
				}
				else
				{
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						((global::A.y)this.m_a.Items[num3]).A = array[num3];
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
			if (!this.m_B && global::a.N.A)
			{
				this.m_b.Start();
			}
		}

		private void R()
		{
			if (!global::a.N.A || this.m_A == null || !this.m_a)
			{
				return;
			}
			try
			{
				if (this.m_C != this.m_A.b)
				{
					this.m_C = this.m_A.b;
					this.m_A.Clear();
					this.m_A.Clear();
					this.m_f = false;
				}
				foreach (global::b.f item in this.m_A.A(this.m_A))
				{
					if (!this.m_A.Add(item.A))
					{
						continue;
					}
					if (item.A)
					{
						if (this.m_f)
						{
							this.m_A.A(global::A.L.A, false);
							this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz() + (item.A + 1) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hZ();
						}
					}
					else
					{
						item.A = DateTime.UtcNow.AddSeconds(global::b.G.A(item.a));
						this.m_A[item.A] = item;
					}
				}
				this.m_f = true;
				string[] array = this.m_A.Where(delegate(KeyValuePair<string, global::b.f> P_0)
				{
					KeyValuePair<string, global::b.f> keyValuePair = P_0;
					return keyValuePair.Value.A < DateTime.UtcNow;
				}).Select(delegate(KeyValuePair<string, global::b.f> P_0)
				{
					KeyValuePair<string, global::b.f> keyValuePair = P_0;
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
			foreach (N A in this.m_A)
			{
				A.A.RemoveAll((global::A.s P_0) => P_0.A != null && P_0.A.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hz()));
				foreach (global::b.f item in this.m_A.Values.Where((global::b.f P_0) => P_0.A == A.A))
				{
					A.A.Add(new global::A.s
					{
						A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.hz() + item.A,
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
		private bool A(N P_0)
		{
			if (!this.m_a.a(P_0))
			{
				if (this.m_A.c)
				{
					if (!(P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bA()))
					{
						return P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DL();
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
		private bool A(Z P_0)
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
			using global::a.O o2 = new global::a.O(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IA(), string.IsNullOrEmpty(this.m_b) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ia() : this.m_b, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.dV());
			o2.ShowDialog(this);
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
				this.m_a.A(((global::A.y)this.m_A.Items[P_1.Index]).A, P_1.NewValue == CheckState.Checked);
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
				this.m_A.A(((global::A.y)this.m_a.Items[P_1.Index]).A, P_1.NewValue == CheckState.Checked);
			}
		}

		[CompilerGenerated]
		private void ac()
		{
			foreach (N item in this.m_A.Where((N P_0) => P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
			{
				this.m_A.A(item, true);
			}
			Q();
		}

		[CompilerGenerated]
		private void aD()
		{
			foreach (N item in this.m_A.Where((N P_0) => P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
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
			this.m_A.Text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IB();
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
	internal static class j
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<N, uint> A;

			public static Func<N, uint> a;

			public static Func<N, uint> B;

			public static Func<string, bool> A;

			internal uint A(N P_0)
			{
				return P_0.A;
			}

			internal uint a(N P_0)
			{
				return P_0.A;
			}

			internal uint B(N P_0)
			{
				return P_0.A;
			}

			internal bool A(string P_0)
			{
				return P_0.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xB());
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public I A;

			internal bool A(N P_0)
			{
				if (!(P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
				{
					return this.A.c;
				}
				return true;
			}

			internal IEnumerable<Color> a(N P_0)
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
			public I A;

			public Func<N, bool> A;

			internal bool A(N P_0)
			{
				if (!(P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
				{
					return this.A.c;
				}
				return true;
			}
		}

		[CompilerGenerated]
		private sealed class b
		{
			public I A;

			public Func<N, bool> A;

			internal bool A(N P_0)
			{
				if (!(P_0.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz()))
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

		public static Color[] A(IEnumerable<N> P_0, I P_1)
		{
			return (from n2 in P_0
				where n2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz() || P_1.c
				orderby n2.A
				select n2).SelectMany((N n2) => (P_1.A && n2.b) ? new Color[2] { n2.A, n2.A } : new Color[2] { P_1.B, P_1.b }).ToArray();
		}

		public static g[][] a(IEnumerable<N> P_0, I P_1)
		{
			List<g[]> list = new List<g[]>();
			foreach (N item in from n2 in P_0
				where n2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz() || P_1.c
				orderby n2.A
				select n2)
			{
				List<g> list2 = new List<g>();
				A(item, P_1, list2);
				list.Add(list2.ToArray());
				list.Add(new g[0]);
			}
			return list.ToArray();
		}

		private static string A(string P_0)
		{
			return Regex.Replace(P_0 ?? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ib(), C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA()).Trim();
		}

		public static string[] B(IEnumerable<N> P_0, I P_1 = null)
		{
			List<string> list = new List<string>();
			foreach (N item in from n2 in P_0
				where n2.a == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Bz() || (P_1 != null && P_1.c)
				orderby n2.A
				select n2)
			{
				global::A.U.a(item);
				global::A.T.A(item);
				p.A(item);
				global::a.f.A(item);
				global::a.i.A(item);
				string text = A(item.A);
				if (text.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IC()))
				{
					text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ic() + text.Substring(6);
				}
				list.Add(A(item, P_1, new List<g>()));
				List<string> list2 = new List<string>();
				foreach (string item2 in item.A.Where((string text2) => text2.Contains(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.xB())))
				{
					if (item2.Split('\\').Length >= 5 && global::A.U.B(item2))
					{
						list2.Add(global::A.U.b(item2));
					}
				}
				List<string> list3 = new List<string>();
				if (P_1 == null || P_1.C)
				{
					list3.Add(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ID() + ((list2.Count == 0) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Id() : string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DK(), list2.Distinct())));
				}
				if (P_1 != null && P_1.b)
				{
					list3.Add(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IE() + global::b.j.A(item));
				}
				list.Add(string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.dQ(), list3));
			}
			return list.ToArray();
		}

		public static string A(N P_0, I P_1, List<g> P_2)
		{
			bool[] array = ((P_1 == null) ? Enumerable.Repeat(element: true, 10).ToArray() : P_1.A);
			string text = A(P_0.A);
			if (text.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IC()))
			{
				text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ic() + text.Substring(6);
			}
			string text2 = text;
			string[] array2 = new string[5]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ie(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IF(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.If(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IG(),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ig()
			};
			double[] array3 = new double[5]
			{
				Math.Floor(P_0.A),
				Math.Floor(P_0.B),
				Math.Floor(P_0.a),
				P_0.C,
				Math.Floor(P_0.b)
			};
			X[] array4 = ((P_1 == null) ? new X[4] : new X[4]
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
					string text3 = array3[num].ToString((num == 3) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.IH() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BY(), CultureInfo.CurrentCulture);
					text2 += array2[num];
					if (num < 4 && array4[num] != null && array4[num].A)
					{
						P_2.Add(new g
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
				global::A.U.a(P_0),
				global::A.T.A(P_0),
				p.A(P_0),
				global::a.f.A(P_0),
				global::a.i.A(P_0)
			};
			for (int num2 = 0; num2 < 5; num2++)
			{
				if (array[5 + num2] && !string.IsNullOrEmpty(array5[num2]))
				{
					text2 = text2 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ih() + array5[num2];
				}
			}
			return text2;
		}
	}
}
namespace b
{
	internal sealed class J
	{
		public int A;

		public string A;

		public bool A;
	}
	internal static class j
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
				return j.A(keyValuePair.Key);
			}

			internal string A(IGrouping<string, KeyValuePair<string, int>> P_0)
			{
				return P_0.Key;
			}

			internal string a(IGrouping<string, KeyValuePair<string, int>> P_0)
			{
				return P_0.Key + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.YG() + P_0.Sum((KeyValuePair<string, int> keyValuePair) => keyValuePair.Value);
			}

			internal int a(KeyValuePair<string, int> P_0)
			{
				return P_0.Value;
			}
		}

		private static readonly Dictionary<string, string> m_A;

		static j()
		{
			j.m_A = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LK(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lk()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LL(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ll()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LM(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ll()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lm(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LN()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ln(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LO()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lo(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LO()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LP(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lp()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LQ(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lq()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LR(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lr()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LS(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ls()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LT(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lt()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LU(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lu()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LV(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lv()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LW(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lw()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LX(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lx()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LY(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ly()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LZ(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lz()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lA(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.la()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lB(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lb()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lC(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lc()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lD(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ld()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lE(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.le()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lF(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lf()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lG(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lf()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lg(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lH()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lh(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lI()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.li(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lJ()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lj(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lK()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lk(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lL()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ll(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lM()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lm(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lN()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ln(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lO()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lo(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lP()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lp(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lQ()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lq(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lQ()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lR(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lr()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lS(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ls()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lT(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lt()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lU(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lu()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lV(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lv()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lW(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lw()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lX(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lx()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lY(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ly()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lZ(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.lz()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MA(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ma()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MB(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mb()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MC(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mc()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MD(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Md()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ME(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Me()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MF(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mf()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MG(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mg()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MH(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mh()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MI(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mi()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MJ(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mj()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MK(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mc()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mk(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Md()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ML(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ml()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MM(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mm()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MN(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mn()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MO(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mo()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MP(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mp()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MQ(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lw()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mq(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MR()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mr(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MS()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ms(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MT()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mt(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MU()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mu(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MV()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mv(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MW()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mw(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mm()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MX(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mx()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MY(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.My()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MZ(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mz()
				},
				{
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.mA(),
					C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ma()
				}
			};
			try
			{
				string[] array = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.mB()));
				foreach (string text in array)
				{
					string[] array2 = text.Split('\t');
					if (array2.Length == 2 && !text.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.B()) && array2[1].Trim().Length > 0)
					{
						j.m_A[array2[0].Trim()] = array2[1].Trim();
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
			if (L.A(P_0, out var result))
			{
				return result;
			}
			string text = P_0.Substring(P_0.LastIndexOf('\\') + 1);
			if (text.Equals(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mt(), StringComparison.OrdinalIgnoreCase))
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MU();
			}
			if (text.Equals(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Mu(), StringComparison.OrdinalIgnoreCase))
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.MV();
			}
			if (!j.m_A.TryGetValue(P_0, out var value) && !j.m_A.TryGetValue(text, out value) && !K.A.TryGetValue(P_0, out value))
			{
				return text;
			}
			return value;
		}

		public static string A(global::B.N P_0)
		{
			if (!P_0.c)
			{
				return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.mb();
			}
			if (P_0.A.Count != 0)
			{
				return string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.DK(), from grouping in P_0.A.GroupBy(delegate(KeyValuePair<string, int> keyValuePair2)
					{
						KeyValuePair<string, int> keyValuePair = keyValuePair2;
						return A(keyValuePair.Key);
					})
					orderby grouping.Key
					select grouping.Key + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.YG() + grouping.Sum((KeyValuePair<string, int> keyValuePair) => keyValuePair.Value));
			}
			return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.mC();
		}
	}
}
