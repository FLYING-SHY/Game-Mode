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
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using C;
using a;

namespace A
{
	internal enum L
	{
		A,
		a,
		B,
		b,
		C,
		c,
		D
	}
	internal sealed class l : IDisposable
	{
		private sealed class A
		{
			public L A;

			public long A;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class a
		{
			public static readonly a A = new a();

			public static Func<KeyValuePair<L, string>, string> A;

			internal string A(KeyValuePair<L, string> P_0)
			{
				return string.Concat(P_0.Key, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FQ(), P_0.Value);
			}
		}

		public bool A = true;

		private readonly Dictionary<L, string> m_A = new Dictionary<L, string>();

		private readonly Queue<A> m_A = new Queue<A>();

		private bool m_a;

		private bool m_B;

		private Q m_A;

		public string A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();

		private readonly string m_a;

		private static string A => AppDomain.CurrentDomain.BaseDirectory;

		private static string a => Path.Combine(l.A, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BV());

		public l(string P_0 = null)
		{
			this.m_a = P_0 ?? l.a;
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
						L result;
						if (text2 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bv())
						{
							this.A = text3 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BW();
						}
						else if (Enum.TryParse<L>(text2, out result))
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

		public string A(L P_0)
		{
			lock (this.m_A)
			{
				string value;
				return (this.m_A.TryGetValue(P_0, out value) && !string.IsNullOrWhiteSpace(value)) ? value : v.A(P_0);
			}
		}

		public string a(L P_0)
		{
			string text = A(P_0);
			if (P_0 == L.A && string.IsNullOrEmpty(text))
			{
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bw();
			}
			if (!string.IsNullOrEmpty(text))
			{
				return Path.GetFileName(text);
			}
			return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BX();
		}

		public void A(L P_0, string P_1)
		{
			if (P_1 != null)
			{
				using (new Q(P_1))
				{
				}
			}
			lock (this.m_A)
			{
				this.m_A[P_0] = P_1 ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
				B();
			}
		}

		public void B()
		{
			lock (this.m_A)
			{
				File.WriteAllLines(this.m_a, new string[1] { _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Bx() + (this.A ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BW() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BY()) }.Concat(this.m_A.Select((KeyValuePair<L, string> P_0) => string.Concat(P_0.Key, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.FQ(), P_0.Value))));
			}
		}

		public void A(L P_0, bool P_1 = false)
		{
			if (!global::a.N.A || (!this.A && !P_1) || this.m_B)
			{
				return;
			}
			if (P_0 == L.A && string.IsNullOrEmpty(A(P_0)))
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
							A = global::a.N.a
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
							if (this.m_B || !global::a.N.A || this.m_A.Count == 0)
							{
								this.m_A.Clear();
								this.m_a = false;
								break;
							}
							a2 = this.m_A.Dequeue();
						}
						if (a2.A == global::a.N.a)
						{
							try
							{
								string text = A(a2.A);
								if (!string.IsNullOrEmpty(text))
								{
									using Q q2 = new Q(text);
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
								this.A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.By();
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
			Q.A();
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
					if (this.m_B || !global::a.N.A || this.m_A.Count == 0)
					{
						this.m_A.Clear();
						this.m_a = false;
						break;
					}
					a2 = this.m_A.Dequeue();
				}
				if (a2.A != global::a.N.a)
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
					using Q q2 = new Q(text);
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
					this.A = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.By();
					this.m_A = null;
				}
			}
		}
	}
}
namespace a
{
	internal static class L
	{
		public static string A
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dW()]))
				{
					return ConfigurationManager.AppSettings[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dW()].Trim();
				}
				return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dw();
			}
		}

		public static Uri a => A(ConfigurationManager.AppSettings[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dX()] ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dx());

		internal static Uri A(string P_0)
		{
			if (!Uri.TryCreate(P_0, UriKind.Absolute, out var result) || string.IsNullOrEmpty(result.Host) || !string.IsNullOrEmpty(result.UserInfo) || !string.IsNullOrEmpty(result.Query) || !string.IsNullOrEmpty(result.Fragment) || (result.Scheme != Uri.UriSchemeHttps && (result.Scheme != Uri.UriSchemeHttp || result.Host != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dY())))
			{
				throw new ArgumentException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dy());
			}
			return new Uri(result.AbsoluteUri.TrimEnd('/') + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.cB());
		}
	}
	internal sealed class l : IDisposable
	{
		[CompilerGenerated]
		private static class A
		{
			public static Action<M> A;
		}

		private readonly object m_A = new object();

		private readonly H m_A;

		private readonly g m_A;

		private readonly CancellationTokenSource m_A = new CancellationTokenSource();

		private readonly M m_A;

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

		private readonly Action<M> m_A;

		private long m_A;

		private long m_a;

		private long m_B;

		private e m_A;

		private int m_A;

		[CompilerGenerated]
		private Action<Q> m_A;

		public M A => this.m_A;

		[SpecialName]
		[CompilerGenerated]
		public void A(Action<Q> P_0)
		{
			Action<Q> action = this.m_A;
			Action<Q> action2;
			do
			{
				action2 = action;
				Action<Q> value = (Action<Q>)Delegate.Combine(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_A, value, action2);
			}
			while ((object)action != action2);
		}

		[SpecialName]
		[CompilerGenerated]
		public void a(Action<Q> P_0)
		{
			Action<Q> action = this.m_A;
			Action<Q> action2;
			do
			{
				action2 = action;
				Action<Q> value = (Action<Q>)Delegate.Remove(action2, P_0);
				action = Interlocked.CompareExchange(ref this.m_A, value, action2);
			}
			while ((object)action != action2);
		}

		public bool A(Q P_0)
		{
			lock (this.m_A)
			{
				return !this.m_A.IsCancellationRequested && P_0.A == this.m_A;
			}
		}

		public l(H P_0 = null, g P_1 = null, M P_2 = null, Action<M> P_3 = null)
		{
			this.m_A = P_0 ?? new global::C.M();
			this.m_A = P_1 ?? new global::C.k();
			this.m_A = P_2 ?? P.a();
			this.m_A = P_3 ?? new Action<M>(P.A);
			this.m_A = this.m_A.Code ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
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
				this.m_A = (P_0 ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D()).Trim().ToUpperInvariant().Replace(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BA(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
				this.m_A.Code = (P_1 ? this.m_A : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
				this.m_A.Remember = P_1;
				this.m_A.AutoLogin = true;
				this.m_A = true;
				this.m_B = true;
				this.m_A++;
				N.B();
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
				N.B();
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
					N.B();
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
					this.m_A?.Invoke(new Q
					{
						A = P_0,
						a = P_1,
						B = ((this.m_A == null) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D() : this.m_A.a),
						A = (P_0 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dS()),
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

		private void A(o P_0)
		{
			string text = P_0.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dZ());
			string text2 = P_0.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dz());
			string text3 = P_0.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EA());
			if (!string.IsNullOrEmpty(text) && p.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ea(), text) && p.A(text2))
			{
				string text4 = text + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dQ() + text2 + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dQ() + text3;
				if (!(text4 == this.m_C))
				{
					this.m_C = text4;
					A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dU(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), text2, 0L, text, text3, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
				}
			}
		}

		private Dictionary<string, object> c()
		{
			return new Dictionary<string, object>
			{
				{
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dG(),
					this.m_A.A
				},
				{
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dg(),
					this.m_A.Installation
				},
				{
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EB(),
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ea()
				},
				{
					_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.df(),
					m.A()
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
			N.B();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			try
			{
				await this.m_A.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XP(), new Dictionary<string, object>(), text, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
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
					long num = k.A;
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
						N.B();
						lock (this.m_A)
						{
							this.m_A = false;
							this.m_A.AutoLogin = false;
						}
						flag2 = false;
						A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ds(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.du(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
						await D().ConfigureAwait(continueOnCapturedContext: false);
						C();
					}
					if (num >= this.m_a)
					{
						e e2 = this.m_A.A();
						this.m_a = k.A + 1000;
						if (!string.IsNullOrEmpty(e2.B))
						{
							if (!string.IsNullOrEmpty(this.m_a) || this.m_A != null)
							{
								await D().ConfigureAwait(continueOnCapturedContext: false);
							}
							this.m_A = e2;
							N.B();
							if (!this.m_a)
							{
								A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xj(), e2.B, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
							}
						}
						else
						{
							if (this.m_A != null && !string.IsNullOrEmpty(this.m_A.A) && this.m_A.A != e2.A)
							{
								await D().ConfigureAwait(continueOnCapturedContext: false);
								this.m_b = Guid.NewGuid().ToString();
								this.m_A = 0L;
							}
							this.m_A = e2;
						}
					}
					if (flag || num < this.m_A)
					{
						goto IL_0dcd;
					}
					o o2 = await this.m_A.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XK(), null, null, this.m_A.Token).ConfigureAwait(continueOnCapturedContext: false);
					lock (this.m_A)
					{
						if (num2 != this.m_A || this.m_A.IsCancellationRequested)
						{
							continue;
						}
						if (p.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ea(), o2.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xk())))
						{
							N.B();
							lock (this.m_A)
							{
								this.m_A = false;
							}
							A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dt(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XL(), o2.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dz()), 0L, o2.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dZ()), o2.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EA()), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
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
					if (!N.A && !this.m_a && !string.IsNullOrEmpty(this.m_a))
					{
						A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xl(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XM(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
					}
					goto IL_0f54;
					IL_0615:
					if (string.IsNullOrEmpty(this.m_a) && this.m_A == null)
					{
						this.m_b = this.m_b ?? Guid.NewGuid().ToString();
						this.m_A = c();
						this.m_A[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xm()] = value;
						this.m_A[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XN()] = this.m_A.a ?? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
						this.m_A[_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xn()] = this.m_b;
					}
					string text = this.m_A.A;
					o o3 = await this.m_A.A(string.IsNullOrEmpty(this.m_a) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xo() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XO(), string.IsNullOrEmpty(this.m_a) ? this.m_A : c(), this.m_a, this.m_A.Token).ConfigureAwait(continueOnCapturedContext: false);
					bool flag4;
					lock (this.m_A)
					{
						flag4 = num2 != this.m_A || this.m_A.IsCancellationRequested;
					}
					if (flag4)
					{
						string text2 = o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dM());
						if (!string.IsNullOrEmpty(text2))
						{
							try
							{
								await this.m_A.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XP(), new Dictionary<string, object>(), text2, CancellationToken.None).ConfigureAwait(continueOnCapturedContext: false);
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
					e e3 = this.m_A.A();
					if (!string.IsNullOrEmpty(e3.B) || e3.A != text)
					{
						string value2 = o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dM());
						if (!string.IsNullOrEmpty(value2))
						{
							this.m_a = value2;
						}
						await D().ConfigureAwait(continueOnCapturedContext: false);
						this.m_A = e3;
						this.m_a = 0L;
						this.m_A = 0L;
						continue;
					}
					bool flag5 = false;
					lock (this.m_A)
					{
						if (num2 != this.m_A || this.m_A.IsCancellationRequested)
						{
							string value3 = o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dM());
							if (!string.IsNullOrEmpty(value3))
							{
								this.m_a = value3;
							}
							continue;
						}
						if (!string.IsNullOrEmpty(o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dM())))
						{
							this.m_a = o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dM());
						}
						string text3 = o3.A;
						if (!(text3 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dk()))
						{
							if (!(text3 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dS()))
							{
								if (!(text3 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dv()))
								{
									if (!(text3 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ds()))
									{
										if (text3 == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dt())
										{
											N.B();
											lock (this.m_A)
											{
												this.m_A = false;
											}
											A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dt(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XL(), o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dz()), 0L, o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dZ()), o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EA()), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
											this.m_A = k.A + 30000;
										}
										else
										{
											N.B();
											this.m_A = false;
											this.m_A.AutoLogin = false;
											this.m_A = k.A + 30000;
											A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dT(), (o3.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xq()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xt() : ((o3.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XR()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XT() : ((o3.A == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xr()) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xs() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XS())), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
											flag5 = true;
										}
									}
									else
									{
										N.B();
										this.m_a = false;
										this.m_A = false;
										this.m_A.AutoLogin = false;
										A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ds(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.du(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
										flag5 = true;
									}
								}
								else
								{
									N.B();
									this.m_A = k.A + 15000;
									A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dv(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XQ(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
								}
							}
							else
							{
								N.B();
								this.m_a = true;
								this.m_A = k.A + 30000;
								A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dS(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
							}
						}
						else
						{
							if (!o3.a || string.IsNullOrEmpty(this.m_a) || this.m_a.Length != 64 || !N.A(o3.a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.@do()), o3.a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dP()), o3.a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dp()), o3.A, this.m_A))
							{
								throw new HttpRequestException(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xp());
							}
							this.m_B = this.m_A.A;
							this.m_a = false;
							this.m_B = o3.A + Math.Max(0L, o3.a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dp()) - o3.a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.@do())) * 1000 - 1000;
							this.m_A = k.A + 28000;
							A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dk(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), o3.a(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dp()), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), o3.A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.dN()));
							C();
							A(o3);
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
					this.m_A = k.A + 5000;
					if (!this.m_a)
					{
						A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xl(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XU(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
					}
					goto IL_0f54;
				}
				catch (Exception ex4)
				{
					if (!(ex4 is HttpRequestException) && !(ex4 is IOException) && !(ex4 is ArgumentException) && !(ex4 is InvalidOperationException) && !(ex4 is OverflowException))
					{
						N.B();
						A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xl(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xu(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
					}
					else if (!this.m_a)
					{
						A(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xl(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XV(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), 0L, _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D(), _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D());
					}
					this.m_A = k.A + 5000;
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
				N.B();
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
				N.B();
			}
		}
	}
}
