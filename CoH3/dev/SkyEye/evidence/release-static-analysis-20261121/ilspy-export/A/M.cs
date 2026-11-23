using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using C;
using a;
using b;

namespace A
{
	internal sealed class M : IDisposable
	{
		private sealed class A
		{
			public l A;

			public long A;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<KeyValuePair<l, string>, string> A;

			internal string A(KeyValuePair<l, string> P_0)
			{
				return string.Concat(P_0.Key, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ft(), P_0.Value);
			}
		}

		public bool A = true;

		private readonly Dictionary<l, string> m_A = new Dictionary<l, string>();

		private readonly Queue<A> m_A = new Queue<A>();

		private bool m_a;

		private bool m_B;

		private q m_A;

		public string A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();

		private readonly string m_a;

		private static string A => AppDomain.CurrentDomain.BaseDirectory;

		private static string a => Path.Combine(M.A, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bv());

		public M(string P_0 = null)
		{
			this.m_a = P_0 ?? M.a;
			try
			{
				string[] array = File.ReadAllLines(this.m_a);
				foreach (string text in array)
				{
					int num2 = text.IndexOf('=');
					if (num2 >= 0)
					{
						string text2 = text.Substring(0, num2);
						string text3 = text.Substring(num2 + 1);
						l result;
						if (text2 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BW())
						{
							this.A = text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bw();
						}
						else if (Enum.TryParse<l>(text2, out result))
						{
							this.m_A[result] = text3;
						}
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

		public string A(l P_0)
		{
			lock (this.m_A)
			{
				string value;
				return (this.m_A.TryGetValue(P_0, out value) && !string.IsNullOrWhiteSpace(value)) ? value : W.A(P_0);
			}
		}

		public string a(l P_0)
		{
			string text = A(P_0);
			if (P_0 == l.A && string.IsNullOrEmpty(text))
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BX();
			}
			if (!string.IsNullOrEmpty(text))
			{
				return Path.GetFileName(text);
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bx();
		}

		public void A(l P_0, string P_1)
		{
			if (P_1 != null)
			{
				using (new q(P_1))
				{
				}
			}
			lock (this.m_A)
			{
				this.m_A[P_0] = P_1 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				B();
			}
		}

		public void B()
		{
			lock (this.m_A)
			{
				File.WriteAllLines(this.m_a, new string[1] { CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BY() + (this.A ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bw() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By()) }.Concat(this.m_A.Select((KeyValuePair<l, string> P_0) => string.Concat(P_0.Key, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ft(), P_0.Value))));
			}
		}

		public void A(l P_0, bool P_1 = false)
		{
			if (!global::a.O.A || (!this.A && !P_1) || this.m_B)
			{
				return;
			}
			if (P_0 == l.A && string.IsNullOrEmpty(A(P_0)))
			{
				SystemSounds.Asterisk.Play();
			}
			else
			{
				if (string.IsNullOrEmpty(A(P_0)))
				{
					return;
				}
				lock (this.m_A)
				{
					if (this.m_A.Count < 16)
					{
						this.m_A.Enqueue(new A
						{
							A = P_0,
							A = global::a.O.a
						});
					}
					if (this.m_a)
					{
						return;
					}
					this.m_a = true;
				}
				ThreadPool.QueueUserWorkItem(delegate
				{
					while (true)
					{
						A a2;
						lock (this.m_A)
						{
							if (this.m_B || !global::a.O.A || this.m_A.Count == 0)
							{
								this.m_A.Clear();
								this.m_a = false;
								break;
							}
							a2 = this.m_A.Dequeue();
						}
						if (a2.A == global::a.O.a)
						{
							try
							{
								string text = A(a2.A);
								if (!string.IsNullOrEmpty(text))
								{
									using q q2 = new q(text);
									lock (this.m_A)
									{
										if (this.m_B)
										{
											this.m_a = false;
											break;
										}
										this.m_A = q2;
									}
									q2.A(a2.A);
									lock (this.m_A)
									{
										this.m_A = null;
									}
								}
							}
							catch (Exception)
							{
								this.A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BZ();
								this.m_A = null;
							}
						}
					}
				});
			}
		}

		public void b()
		{
			lock (this.m_A)
			{
				this.m_A.Clear();
			}
			q.A();
		}

		public void Dispose()
		{
			lock (this.m_A)
			{
				this.m_B = true;
				this.m_A.Clear();
			}
			if (this.m_A != null)
			{
				try
				{
					this.m_A.Dispose();
				}
				catch
				{
				}
			}
		}

		[CompilerGenerated]
		private void A(object P_0)
		{
			while (true)
			{
				A a2;
				lock (this.m_A)
				{
					if (this.m_B || !global::a.O.A || this.m_A.Count == 0)
					{
						this.m_A.Clear();
						this.m_a = false;
						break;
					}
					a2 = this.m_A.Dequeue();
				}
				if (a2.A != global::a.O.a)
				{
					continue;
				}
				try
				{
					string text = A(a2.A);
					if (string.IsNullOrEmpty(text))
					{
						continue;
					}
					using q q2 = new q(text);
					lock (this.m_A)
					{
						if (this.m_B)
						{
							this.m_a = false;
							break;
						}
						this.m_A = q2;
					}
					q2.A(a2.A);
					lock (this.m_A)
					{
						this.m_A = null;
					}
				}
				catch (Exception)
				{
					this.A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.BZ();
					this.m_A = null;
				}
			}
		}
	}
	internal static class m
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::a.e, bool> A;

			public static Func<global::a.e, bool> a;

			public static Func<global::a.e, bool> B;

			internal bool A(global::a.e P_0)
			{
				return P_0.A == l.a;
			}

			internal bool a(global::a.e P_0)
			{
				return P_0.A == l.B;
			}

			internal bool B(global::a.e P_0)
			{
				return P_0.A == l.b;
			}
		}

		private static void A(bool P_0, string P_1)
		{
			if (!P_0)
			{
				throw new Exception(P_1);
			}
		}

		public static void A(List<string> P_0)
		{
			global::B.O o2 = new global::B.O
			{
				A = 2u,
				A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Bz(),
				a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA()
			};
			global::B.O o3 = new global::B.O
			{
				A = 3u,
				A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba(),
				a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()
			};
			List<global::B.O> list = new List<global::B.O> { o2, o3 };
			global::a.A a2 = new global::a.A();
			a2.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB());
			N obj = new N();
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bb());
			o2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bC());
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count((global::a.e e2) => e2.A == l.a) == 1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bc());
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bD());
			global::b.K k2 = new global::b.K
			{
				A = 2,
				A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bd(),
				A = false
			};
			o2.A.Add(k2);
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bE());
			k2.A = true;
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.be());
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count((global::a.e e2) => e2.A == l.B) == 1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bF());
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bf());
			a2.A(o2, false);
			o2.A.Add(new global::b.K
			{
				A = 3,
				A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bG(),
				A = true
			});
			obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2);
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bg());
			p p2 = new p
			{
				A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bH(),
				a = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bh(),
				a = true
			};
			p2.A[17u] = 360;
			o3.A.Add(p2);
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bI());
			p2.A[17u] = 4;
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bi());
			p2.A.Clear();
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count((global::a.e e2) => e2.A == l.b) == 1, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bJ());
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bj());
			p2.A[17u] = 100;
			obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2);
			o3.A.Clear();
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bB(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bK());
			A(obj.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bk(), list, a2).Count == 0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bL());
			using (M m2 = new M())
			{
				foreach (l value in Enum.GetValues(typeof(l)))
				{
					if (!string.IsNullOrEmpty(m2.A(value)))
					{
						using (new q(m2.A(value)))
						{
						}
					}
				}
			}
			P_0.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bl());
		}
	}
}
namespace a
{
	internal static class M
	{
		public static string A
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dz()]))
				{
					return ConfigurationManager.AppSettings[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dz()].Trim();
				}
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EA();
			}
		}

		public static Uri a => A(ConfigurationManager.AppSettings[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ea()] ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EB());

		internal static Uri A(string P_0)
		{
			if (!Uri.TryCreate(P_0, UriKind.Absolute, out var result) || string.IsNullOrEmpty(result.Host) || !string.IsNullOrEmpty(result.UserInfo) || !string.IsNullOrEmpty(result.Query) || !string.IsNullOrEmpty(result.Fragment) || (result.Scheme != Uri.UriSchemeHttps && (result.Scheme != Uri.UriSchemeHttp || result.Host != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Eb())))
			{
				throw new ArgumentException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EC());
			}
			return new Uri(result.AbsoluteUri.TrimEnd('/') + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cb());
		}
	}
	internal sealed class m : IDisposable
	{
		[CompilerGenerated]
		private static class A
		{
			public static Action<N> A;
		}

		private readonly object m_A = new object();

		private readonly I m_A;

		private readonly h m_A;

		private readonly CancellationTokenSource m_A = new CancellationTokenSource();

		private readonly N m_A;

		private Task m_A;

		private string m_A;

		private string m_a;

		private string m_B;

		private string m_b;

		private string m_C;

		private Dictionary<string, object> m_A;

		private bool m_A;

		private bool m_a;

		private bool m_B;

		private readonly Action<N> m_A;

		private long m_A;

		private long m_a;

		private long m_B;

		private F m_A;

		private int m_A;

		[CompilerGenerated]
		private Action<R> m_A;

		public N A => this.m_A;

		[SpecialName]
		[CompilerGenerated]
		public void A(Action<R> P_0)
		{
			Action<R> action = this.m_A;
			Action<R> action2;
			do
			{
				action2 = action;
				Action<R> value = (Action<R>)Delegate.Combine(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_A, value, action2);
			}
			while ((object)action != action2);
		}

		[SpecialName]
		[CompilerGenerated]
		public void a(Action<R> P_0)
		{
			Action<R> action = this.m_A;
			Action<R> action2;
			do
			{
				action2 = action;
				Action<R> value = (Action<R>)Delegate.Remove(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_A, value, action2);
			}
			while ((object)action != action2);
		}

		public bool A(R P_0)
		{
			lock (this.m_A)
			{
				return !this.m_A.IsCancellationRequested && P_0.A == this.m_A;
			}
		}

		public m(I P_0 = null, h P_1 = null, N P_2 = null, Action<N> P_3 = null)
		{
			this.m_A = P_0 ?? new global::C.O();
			this.m_A = P_1 ?? new global::C.l();
			this.m_A = P_2 ?? Q.a();
			this.m_A = P_3 ?? new Action<N>(Q.A);
			this.m_A = this.m_A.Code ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
			this.m_A = this.m_A.AutoLogin && !string.IsNullOrEmpty(this.m_A);
		}

		public void a()
		{
			if (this.m_A == null)
			{
				this.m_A = Task.Run((Func<Task>)d);
			}
		}

		public void A(string P_0, bool P_1)
		{
			lock (this.m_A)
			{
				if (this.m_A.IsCancellationRequested)
				{
					return;
				}
				this.m_A = (P_0 ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D()).Trim().ToUpperInvariant().Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
				this.m_A.Code = (P_1 ? this.m_A : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
				this.m_A.Remember = P_1;
				this.m_A.AutoLogin = true;
				this.m_A = true;
				this.m_B = true;
				this.m_A++;
				O.B();
			}
			C();
		}

		public void B()
		{
			lock (this.m_A)
			{
				this.m_A = false;
				this.m_B = true;
				this.m_A++;
				this.m_A.AutoLogin = false;
				O.B();
			}
			C();
		}

		public void b()
		{
			lock (this.m_A)
			{
				if (!this.m_A.IsCancellationRequested)
				{
					this.m_A++;
					this.m_A = 0L;
					this.m_a = 0L;
					O.B();
				}
			}
		}

		private void C()
		{
			lock (this.m_A)
			{
				try
				{
					this.m_A(this.m_A);
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (CryptographicException)
				{
				}
			}
		}

		private void A(string P_0, string P_1 = "", string P_2 = "", long P_3 = 0L, string P_4 = "", string P_5 = "", string P_6 = "")
		{
			lock (this.m_A)
			{
				if (!this.m_A.IsCancellationRequested)
				{
					this.m_A?.Invoke(new R
					{
						A = P_0,
						a = P_1,
						B = ((this.m_A == null) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D() : this.m_A.a),
						A = (P_0 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dv()),
						b = P_2,
						A = P_3,
						D = P_6,
						C = P_4,
						c = P_5,
						A = this.m_A
					});
				}
			}
		}

		private void A(p P_0)
		{
			string text = P_0.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ec());
			string text2 = P_0.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ED());
			string text3 = P_0.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ed());
			if (!string.IsNullOrEmpty(text) && q.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EE(), text) && q.A(text2))
			{
				string text4 = text + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dt() + text2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dt() + text3;
				if (!(text4 == this.m_C))
				{
					this.m_C = text4;
					A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dx(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), text2, 0L, text, text3, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
				}
			}
		}

		private Dictionary<string, object> c()
		{
			return new Dictionary<string, object>
			{
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dj(),
					this.m_A.A
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dK(),
					this.m_A.Installation
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ee(),
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EE()
				},
				{
					CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dJ(),
					n.A()
				}
			};
		}

		private async Task D()
		{
			string text = this.m_a;
			this.m_a = null;
			this.m_B = null;
			this.m_A = null;
			this.m_b = Guid.NewGuid().ToString();
			this.m_B = 0L;
			O.B();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			try
			{
				await this.m_A.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xt(), new Dictionary<string, object>(), text, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception ex)
			{
				if (!(ex is HttpRequestException) && !(ex is TaskCanceledException) && !(ex is ArgumentException) && !(ex is InvalidOperationException))
				{
					throw;
				}
			}
		}

		private async Task d()
		{
			bool flag = false;
			while (!this.m_A.IsCancellationRequested)
			{
				try
				{
					long num = l.A;
					int num2;
					bool flag2;
					string value;
					bool flag3;
					lock (this.m_A)
					{
						num2 = this.m_A;
						flag2 = this.m_A;
						value = this.m_A;
						flag3 = this.m_B;
						this.m_B = false;
					}
					if (flag3)
					{
						await D().ConfigureAwait(continueOnCapturedContext: false);
						this.m_a = false;
						this.m_A = 0L;
						this.m_b = Guid.NewGuid().ToString();
						flag = false;
					}
					if (this.m_B > 0 && num >= this.m_B && !this.m_a)
					{
						O.B();
						lock (this.m_A)
						{
							this.m_A = false;
							this.m_A.AutoLogin = false;
						}
						flag2 = false;
						A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dW(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dY(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
						await D().ConfigureAwait(continueOnCapturedContext: false);
						C();
					}
					if (num >= this.m_a)
					{
						F f2 = this.m_A.A();
						this.m_a = l.A + 1000;
						if (!string.IsNullOrEmpty(f2.B))
						{
							if (!string.IsNullOrEmpty(this.m_a) || this.m_A != null)
							{
								await D().ConfigureAwait(continueOnCapturedContext: false);
							}
							this.m_A = f2;
							O.B();
							if (!this.m_a)
							{
								A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XO(), f2.B, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
							}
						}
						else
						{
							if (this.m_A != null && !string.IsNullOrEmpty(this.m_A.A) && this.m_A.A != f2.A)
							{
								await D().ConfigureAwait(continueOnCapturedContext: false);
								this.m_b = Guid.NewGuid().ToString();
								this.m_A = 0L;
							}
							this.m_A = f2;
						}
					}
					if (flag || num < this.m_A)
					{
						goto IL_0dcd;
					}
					p p2 = await this.m_A.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xo(), null, null, this.m_A.Token).ConfigureAwait(continueOnCapturedContext: false);
					lock (this.m_A)
					{
						if (num2 != this.m_A || this.m_A.IsCancellationRequested)
						{
							continue;
						}
						if (q.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EE(), p2.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XP())))
						{
							O.B();
							lock (this.m_A)
							{
								this.m_A = false;
							}
							A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dX(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xp(), p2.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ED()), 0L, p2.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ec()), p2.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ed()), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
							this.m_A = num + 30000;
						}
						else
						{
							flag = true;
							this.m_A = 0L;
						}
						goto IL_0dcd;
					}
					IL_05ce:
					if (!O.A && !this.m_a && !string.IsNullOrEmpty(this.m_a))
					{
						A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XQ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xq(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
					}
					goto IL_0f54;
					IL_0615:
					if (string.IsNullOrEmpty(this.m_a) && this.m_A == null)
					{
						this.m_b = this.m_b ?? Guid.NewGuid().ToString();
						this.m_A = c();
						this.m_A[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XR()] = value;
						this.m_A[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xr()] = this.m_A.a ?? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
						this.m_A[CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XS()] = this.m_b;
					}
					string text = this.m_A.A;
					p p3 = await this.m_A.A(string.IsNullOrEmpty(this.m_a) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XT() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xs(), string.IsNullOrEmpty(this.m_a) ? this.m_A : c(), this.m_a, this.m_A.Token).ConfigureAwait(continueOnCapturedContext: false);
					bool flag4;
					lock (this.m_A)
					{
						flag4 = num2 != this.m_A || this.m_A.IsCancellationRequested;
					}
					if (flag4)
					{
						string text2 = p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dp());
						if (!string.IsNullOrEmpty(text2))
						{
							try
							{
								await this.m_A.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xt(), new Dictionary<string, object>(), text2, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
							}
							catch (HttpRequestException)
							{
							}
							catch (OperationCanceledException)
							{
							}
						}
						continue;
					}
					F f3 = this.m_A.A();
					if (!string.IsNullOrEmpty(f3.B) || f3.A != text)
					{
						string value2 = p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dp());
						if (!string.IsNullOrEmpty(value2))
						{
							this.m_a = value2;
						}
						await D().ConfigureAwait(continueOnCapturedContext: false);
						this.m_A = f3;
						this.m_a = 0L;
						this.m_A = 0L;
						continue;
					}
					bool flag5 = false;
					lock (this.m_A)
					{
						if (num2 != this.m_A || this.m_A.IsCancellationRequested)
						{
							string value3 = p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dp());
							if (!string.IsNullOrEmpty(value3))
							{
								this.m_a = value3;
							}
							continue;
						}
						if (!string.IsNullOrEmpty(p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dp())))
						{
							this.m_a = p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dp());
						}
						string text3 = p3.A;
						if (!(text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dO()))
						{
							if (!(text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dv()))
							{
								if (!(text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dZ()))
								{
									if (!(text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dW()))
									{
										if (text3 == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dX())
										{
											O.B();
											lock (this.m_A)
											{
												this.m_A = false;
											}
											A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dX(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xp(), p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ED()), 0L, p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ec()), p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ed()), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
											this.m_A = l.A + 30000;
										}
										else
										{
											O.B();
											this.m_A = false;
											this.m_A.AutoLogin = false;
											this.m_A = l.A + 30000;
											A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dw(), (p3.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XV()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XY() : ((p3.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xv()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xx() : ((p3.A == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XW()) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XX() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xw())), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
											flag5 = true;
										}
									}
									else
									{
										O.B();
										this.m_a = false;
										this.m_A = false;
										this.m_A.AutoLogin = false;
										A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dW(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dY(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
										flag5 = true;
									}
								}
								else
								{
									O.B();
									this.m_A = l.A + 15000;
									A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dZ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xu(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
								}
							}
							else
							{
								O.B();
								this.m_a = true;
								this.m_A = l.A + 30000;
								A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dv(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
							}
						}
						else
						{
							if (!p3.a || string.IsNullOrEmpty(this.m_a) || this.m_a.Length != 64 || !O.A(p3.a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dS()), p3.a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ds()), p3.a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dT()), p3.A, this.m_A))
							{
								throw new HttpRequestException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XU());
							}
							this.m_B = this.m_A.A;
							this.m_a = false;
							this.m_B = p3.A + Math.Max(0L, p3.a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dT()) - p3.a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dS())) * 1000 - 1000;
							this.m_A = l.A + 28000;
							A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dO(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), p3.a(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dT()), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), p3.A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.dq()));
							C();
							A(p3);
						}
					}
					if (flag5)
					{
						await D().ConfigureAwait(continueOnCapturedContext: false);
						C();
					}
					goto IL_05ce;
					IL_0dcd:
					if (!flag || !flag2 || this.m_A == null || !string.IsNullOrEmpty(this.m_A.B) || num < this.m_A)
					{
						goto IL_05ce;
					}
					lock (this.m_A)
					{
						if (num2 != this.m_A || this.m_A.IsCancellationRequested)
						{
							continue;
						}
						goto IL_0615;
					}
				}
				catch (OperationCanceledException)
				{
					if (this.m_A.IsCancellationRequested)
					{
						break;
					}
					this.m_A = l.A + 5000;
					if (!this.m_a)
					{
						A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XQ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xy(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
					}
					goto IL_0f54;
				}
				catch (Exception ex4)
				{
					if (!(ex4 is HttpRequestException) && !(ex4 is IOException) && !(ex4 is ArgumentException) && !(ex4 is InvalidOperationException) && !(ex4 is OverflowException))
					{
						O.B();
						A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XQ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XZ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
					}
					else if (!this.m_a)
					{
						A(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XQ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xz(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), 0L, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
					}
					this.m_A = l.A + 5000;
					goto IL_0f54;
				}
				IL_0f54:
				try
				{
					await Task.Delay(250, this.m_A.Token).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
		}

		public async Task E()
		{
			lock (this.m_A)
			{
				this.m_A.Cancel();
				this.m_A++;
				O.B();
			}
			if (this.m_A != null)
			{
				try
				{
					await this.m_A.ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (OperationCanceledException)
				{
				}
			}
			try
			{
				await D().ConfigureAwait(continueOnCapturedContext: false);
			}
			finally
			{
				this.m_A.Dispose();
			}
		}

		public void Dispose()
		{
			lock (this.m_A)
			{
				this.m_A.Cancel();
				this.m_A++;
				O.B();
			}
		}
	}
}
