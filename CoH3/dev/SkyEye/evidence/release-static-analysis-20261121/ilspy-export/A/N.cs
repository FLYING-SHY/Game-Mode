using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using Microsoft.Win32;
using a;
using b;

namespace A
{
	internal sealed class N
	{
		[CompilerGenerated]
		private sealed class A
		{
			public HashSet<string> A;

			public HashSet<string> a;

			public N A;

			internal bool A(string P_0)
			{
				return !this.A.Contains(P_0);
			}

			internal bool a(string P_0)
			{
				return !this.a.Contains(P_0);
			}

			internal bool B(string P_0)
			{
				return !this.A.m_A.Contains(P_0);
			}
		}

		[CompilerGenerated]
		private sealed class a
		{
			public global::B.O A;

			public Func<KeyValuePair<string, uint>, bool> A;

			internal bool A(KeyValuePair<string, uint> P_0)
			{
				return P_0.Value == this.A.A;
			}
		}

		private readonly HashSet<string> m_A = new HashSet<string>();

		private readonly Dictionary<string, uint> m_A = new Dictionary<string, uint>();

		private readonly HashSet<uint> m_A = new HashSet<uint>();

		private string m_A;

		private bool m_A;

		private readonly Dictionary<uint, string> m_A = new Dictionary<uint, string>();

		private readonly HashSet<string> m_a = new HashSet<string>();

		private readonly Dictionary<string, int> m_A = new Dictionary<string, int>();

		private readonly Dictionary<string, Dictionary<uint, int>> m_A = new Dictionary<string, Dictionary<uint, int>>();

		public void A()
		{
			this.m_A = false;
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
			this.m_a.Clear();
			this.m_A.Clear();
			this.m_A.Clear();
		}

		public List<global::a.e> A(string P_0, List<global::B.O> P_1, global::a.A P_2)
		{
			if (this.m_A != P_0)
			{
				this.A();
				this.m_A = P_0;
			}
			List<global::a.e> list = new List<global::a.e>();
			HashSet<string> A = new HashSet<string>();
			HashSet<string> a2 = new HashSet<string>();
			HashSet<string> hashSet = new HashSet<string>();
			foreach (global::B.O A2 in P_1)
			{
				if (A2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bA())
				{
					if (!A2.B)
					{
						foreach (KeyValuePair<string, uint> item in this.m_A.Where((KeyValuePair<string, uint> keyValuePair) => keyValuePair.Value == A2.A))
						{
							hashSet.Add(item.Key);
						}
					}
					foreach (T item2 in A2.A)
					{
						string text = item2.a ?? item2.A;
						this.m_A[text] = A2.A;
						hashSet.Add(text);
						if (this.m_A.Add(text) && this.m_A && P_2.a(A2))
						{
							list.Add(new global::a.e
							{
								A = l.D,
								A = A2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + item2.B + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bM()
							});
						}
					}
					if (global::a.g.a(A2) && this.m_A.Add(A2.A) && this.m_A && P_2.a(A2))
					{
						list.Add(new global::a.e
						{
							A = l.C,
							A = A2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bm()
						});
					}
					string text2 = u.A(A2);
					string value;
					bool flag = this.m_A.TryGetValue(A2.A, out value);
					if (this.m_A && flag && text2.Length > 0 && text2 != value && P_2.a(A2))
					{
						list.Add(new global::a.e
						{
							A = l.a,
							A = A2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bN() + u.a(A2)
						});
					}
					this.m_A[A2.A] = text2;
					foreach (global::b.K item3 in A2.A)
					{
						string text3 = A2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.aY() + item3.A;
						A.Add(text3);
						if (!item3.A)
						{
							this.m_A.Remove(text3);
							continue;
						}
						this.m_A.TryGetValue(text3, out var value2);
						this.m_A[text3] = value2 + 1;
						if (!this.m_A)
						{
							this.m_a.Add(text3);
						}
						else if (value2 + 1 >= 2 && this.m_a.Add(text3) && P_2.a(A2))
						{
							list.Add(new global::a.e
							{
								A = l.B,
								A = A2.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ba() + t.A(item3.A) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bn()
							});
						}
					}
				}
				if (!(A2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba()))
				{
					continue;
				}
				foreach (p item4 in A2.A)
				{
					a2.Add(item4.A);
					if (!item4.a)
					{
						this.m_A.Remove(item4.A);
						continue;
					}
					if (this.m_A && this.m_A.TryGetValue(item4.A, out var value3))
					{
						foreach (KeyValuePair<uint, int> item5 in value3)
						{
							if (item5.Value > 0 && (!item4.A.TryGetValue(item5.Key, out var value4) || value4 <= 0))
							{
								list.Add(new global::a.e
								{
									A = l.b,
									A = item4.A
								});
							}
						}
					}
					this.m_A[item4.A] = new Dictionary<uint, int>(item4.A);
				}
			}
			string[] array = this.m_A.Keys.Where((string item) => !A.Contains(item)).ToArray();
			foreach (string key in array)
			{
				this.m_A.Remove(key);
			}
			array = this.m_A.Keys.Where((string item) => !a2.Contains(item)).ToArray();
			foreach (string key2 in array)
			{
				this.m_A.Remove(key2);
			}
			this.m_A.IntersectWith(hashSet);
			array = this.m_A.Keys.Where((string item) => !this.m_A.Contains(item)).ToArray();
			foreach (string key3 in array)
			{
				this.m_A.Remove(key3);
			}
			this.m_A = true;
			return list;
		}
	}
	internal sealed class n : IDisposable
	{
		private struct A
		{
			public uint A;

			public int A;

			public int a;

			public ushort A;

			public ushort a;

			public uint a;

			public uint B;

			public int B;

			public int b;

			public uint b;

			public uint C;
		}

		private struct a
		{
			public byte A;

			public byte a;

			public byte B;

			public byte b;
		}

		private IntPtr m_A;

		private IntPtr m_a;

		private IntPtr m_B;

		private IntPtr m_b;

		[CompilerGenerated]
		private Bitmap m_A;

		[CompilerGenerated]
		private Graphics m_A;

		public Bitmap A
		{
			[CompilerGenerated]
			get
			{
				return this.m_A;
			}
			[CompilerGenerated]
			private set
			{
				this.m_A = bitmap;
			}
		}

		public Graphics a
		{
			[CompilerGenerated]
			get
			{
				return this.m_A;
			}
			[CompilerGenerated]
			private set
			{
				this.m_A = graphics;
			}
		}

		[DllImport("user32.dll", EntryPoint = "GetDC")]
		private static extern IntPtr A(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		private static extern int A(IntPtr P_0, IntPtr P_1);

		[DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC")]
		private static extern IntPtr a(IntPtr P_0);

		[DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
		private static extern bool B(IntPtr P_0);

		[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection")]
		private static extern IntPtr A(IntPtr P_0, ref A P_1, uint P_2, out IntPtr P_3, IntPtr P_4, uint P_5);

		[DllImport("gdi32.dll", EntryPoint = "SelectObject")]
		private static extern IntPtr a(IntPtr P_0, IntPtr P_1);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		private static extern bool b(IntPtr P_0);

		[DllImport("user32.dll", EntryPoint = "UpdateLayeredWindow", SetLastError = true)]
		private static extern bool A(IntPtr P_0, IntPtr P_1, ref Point P_2, ref Size P_3, IntPtr P_4, ref Point P_5, int P_6, ref a P_7, int P_8);

		public void A(int P_0, int P_1)
		{
			if (this.A != null && this.A.Width == P_0 && this.A.Height == P_1)
			{
				return;
			}
			B();
			IntPtr intPtr = A(IntPtr.Zero);
			try
			{
				this.m_A = a(intPtr);
				A a2 = new A
				{
					A = (uint)Marshal.SizeOf(typeof(A)),
					A = P_0,
					a = -P_1,
					A = 1,
					a = 32
				};
				this.m_a = A(intPtr, ref a2, 0u, out this.m_b, IntPtr.Zero, 0u);
				if (this.m_A == IntPtr.Zero || this.m_a == IntPtr.Zero)
				{
					throw new Exception(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.bO());
				}
				this.m_B = a(this.m_A, this.m_a);
				this.A = new Bitmap(P_0, P_1, P_0 * 4, PixelFormat.Format32bppPArgb, this.m_b);
				this.a = Graphics.FromImage(this.A);
			}
			catch
			{
				B();
				throw;
			}
			finally
			{
				A(IntPtr.Zero, intPtr);
			}
		}

		public void A(IntPtr P_0, Point P_1)
		{
			this.a.Flush(FlushIntention.Sync);
			Size size = this.A.Size;
			Point empty = Point.Empty;
			a a2 = new a
			{
				B = byte.MaxValue,
				b = 1
			};
			if (!A(P_0, IntPtr.Zero, ref P_1, ref size, this.m_A, ref empty, 0, ref a2, 2))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		private void B()
		{
			if (this.a != null)
			{
				this.a.Dispose();
				this.a = null;
			}
			if (this.A != null)
			{
				this.A.Dispose();
				this.A = null;
			}
			if (this.m_A != IntPtr.Zero && this.m_B != IntPtr.Zero)
			{
				a(this.m_A, this.m_B);
			}
			if (this.m_a != IntPtr.Zero)
			{
				b(this.m_a);
			}
			if (this.m_A != IntPtr.Zero)
			{
				B(this.m_A);
			}
			this.m_A = (this.m_a = (this.m_B = (this.m_b = IntPtr.Zero)));
		}

		public void Dispose()
		{
			B();
		}
	}
}
namespace a
{
	internal sealed class N
	{
		public string Installation;

		public string Code;

		public bool Remember = true;

		public bool AutoLogin = true;
	}
	internal static class n
	{
		public static string A()
		{
			using RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EF());
			string text = registryKey?.GetValue(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ef()) as string;
			if (string.IsNullOrWhiteSpace(text))
			{
				throw new PlatformNotSupportedException(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.EG());
			}
			return A(M.A, text);
		}

		internal static string A(string P_0, string P_1)
		{
			using SHA256 sHA = SHA256.Create();
			return BitConverter.ToString(sHA.ComputeHash(Encoding.UTF8.GetBytes(P_0 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Eg() + P_1.Trim().ToLowerInvariant()))).Replace(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.CJ(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D());
		}
	}
}
