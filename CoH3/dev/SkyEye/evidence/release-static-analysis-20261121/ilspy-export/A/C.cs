using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;

namespace A
{
	internal static class C
	{
		internal static bool A(string P_0)
		{
			if (string.IsNullOrEmpty(P_0) || P_0.Length < 5 || P_0.Length > 12 || P_0[0] == '0')
			{
				return false;
			}
			foreach (char c2 in P_0)
			{
				if (c2 < '0' || c2 > '9')
				{
					return false;
				}
			}
			return true;
		}

		internal static string A(byte[] P_0)
		{
			if (P_0.Length > 65536)
			{
				throw new InvalidDataException();
			}
			Encoding[] array = new Encoding[4]
			{
				new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
				new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true),
				new UnicodeEncoding(bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true),
				Encoding.GetEncoding(54936, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)
			};
			foreach (Encoding encoding in array)
			{
				string text;
				try
				{
					text = encoding.GetString(P_0).TrimStart('\ufeff');
				}
				catch (DecoderFallbackException)
				{
					continue;
				}
				if (text.IndexOf('\0') >= 0)
				{
					continue;
				}
				bool flag = false;
				string[] array2 = text.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int num2 = 0; num2 < array2.Length; num2++)
				{
					string text2 = array2[num2].Trim();
					if (text2.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.a()) || text2.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.B()))
					{
						continue;
					}
					if (text2.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.b()))
					{
						flag = text2.Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.C(), StringComparison.OrdinalIgnoreCase);
						continue;
					}
					int num3 = text2.IndexOf('=');
					if (flag && num3 >= 0 && text2.Substring(0, num3).Trim().Equals(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.c(), StringComparison.OrdinalIgnoreCase))
					{
						string text3 = text2.Substring(num3 + 1).Trim();
						if (text3.Length >= 2 && ((text3[0] == '"' && text3[text3.Length - 1] == '"') || (text3[0] == '\'' && text3[text3.Length - 1] == '\'')))
						{
							text3 = text3.Substring(1, text3.Length - 2);
						}
						return Environment.ExpandEnvironmentVariables(text3);
					}
				}
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
		}

		private static string a(string P_0)
		{
			using FileStream fileStream = new FileStream(P_0, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			byte[] array = new byte[65537];
			int num;
			int num2;
			for (num = 0; num < array.Length; num += num2)
			{
				if ((num2 = fileStream.Read(array, num, array.Length - num)) <= 0)
				{
					break;
				}
			}
			if (num > 65536)
			{
				throw new InvalidDataException();
			}
			Array.Resize(ref array, num);
			return A(array);
		}

		private static bool B(string P_0)
		{
			if (string.IsNullOrWhiteSpace(P_0) || !Path.IsPathRooted(P_0) || P_0.StartsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.d()))
			{
				return false;
			}
			return new DriveInfo(Path.GetPathRoot(P_0)).DriveType != DriveType.Network;
		}

		internal static c A(IEnumerable<string> P_0, IEnumerable<string> P_1)
		{
			c c2 = new c();
			List<string> list = new List<string>(P_0);
			foreach (string item in P_1)
			{
				try
				{
					if (!File.Exists(item))
					{
						c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.E() + item);
						continue;
					}
					string text = a(item);
					if (string.IsNullOrWhiteSpace(text))
					{
						c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.e() + item);
						continue;
					}
					list.Add(text);
					c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.F() + item);
				}
				catch (Exception ex)
				{
					c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.f() + A(ex) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.G() + item);
				}
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Stopwatch stopwatch = Stopwatch.StartNew();
			foreach (string item2 in list)
			{
				if (stopwatch.Elapsed > TimeSpan.FromSeconds(15.0))
				{
					c2.A = true;
					break;
				}
				try
				{
					if (!B(item2))
					{
						c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.g() + item2);
						continue;
					}
					string fullPath = Path.GetFullPath(item2);
					if (fullPath.TrimEnd(Path.DirectorySeparatorChar).Equals(Path.GetPathRoot(fullPath).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
					{
						c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.H() + item2);
						continue;
					}
					string text2 = fullPath.TrimEnd(Path.DirectorySeparatorChar);
					if (!hashSet.Add(text2))
					{
						continue;
					}
					c2.A++;
					if (!Directory.Exists(text2))
					{
						c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.h() + text2);
						continue;
					}
					if ((File.GetAttributes(text2) & FileAttributes.ReparsePoint) != 0)
					{
						c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.I() + text2);
						continue;
					}
					string fileName = Path.GetFileName(text2);
					if (A(fileName))
					{
						A(text2, fileName, c2);
						c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.i() + text2);
						continue;
					}
					int num = 0;
					int count = c2.A.Count;
					foreach (string item3 in Directory.EnumerateDirectories(text2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.J(), SearchOption.TopDirectoryOnly))
					{
						if (++num > 4096 || stopwatch.Elapsed > TimeSpan.FromSeconds(15.0))
						{
							c2.A = true;
							break;
						}
						string fileName2 = Path.GetFileName(item3);
						if (A(fileName2))
						{
							try
							{
								A(item3, fileName2, c2);
							}
							catch (Exception ex2)
							{
								c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.j() + A(ex2) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.G() + item3);
							}
						}
					}
					c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.K() + text2 + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.k() + (c2.A.Count - count));
				}
				catch (Exception ex3)
				{
					c2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.L() + A(ex3) + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.G() + item2);
				}
			}
			return c2;
		}

		private static void A(string P_0, string P_1, c P_2)
		{
			if ((File.GetAttributes(P_0) & FileAttributes.ReparsePoint) != 0)
			{
				P_2.A.Add(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.l() + P_0);
				return;
			}
			bool flag = A(Path.Combine(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.M()), true);
			bool flag2 = A(Path.Combine(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.m()), false) || A(Path.Combine(P_0, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.N()), false);
			if (!P_2.A.TryGetValue(P_1, out var value))
			{
				value = new b
				{
					A = P_1
				};
				P_2.A.Add(P_1, value);
			}
			value.A.Add(P_0);
			value.A |= flag || flag2;
			value.a.Add(flag ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.o() : (flag2 ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.O() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.n()));
		}

		private static bool A(string P_0, bool P_1)
		{
			try
			{
				FileAttributes attributes = File.GetAttributes(P_0);
				return (attributes & FileAttributes.ReparsePoint) == 0 && (attributes & FileAttributes.Directory) != 0 == P_1;
			}
			catch (IOException)
			{
				return false;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}

		private static string A(Exception P_0)
		{
			if (P_0 is UnauthorizedAccessException || P_0 is SecurityException)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.P();
			}
			if (P_0 is InvalidDataException)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.p();
			}
			if (P_0 is IOException)
			{
				return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Q();
			}
			return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.q();
		}

		internal static c b(string P_0)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
			if (!string.IsNullOrEmpty(folderPath))
			{
				list.Add(Path.Combine(folderPath, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.R()));
			}
			if (!string.IsNullOrEmpty(folderPath2))
			{
				list2.Add(Path.Combine(folderPath2, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.r(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.S(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.s()));
			}
			if (!string.IsNullOrWhiteSpace(P_0))
			{
				list.Add(P_0);
			}
			return A(list, list2);
		}
	}
	internal sealed class c
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<b, bool> A;

			public static Func<b, int> A;

			public static Func<b, string> A;

			internal bool A(b P_0)
			{
				return P_0.A;
			}

			internal int a(b P_0)
			{
				return P_0.A.Length;
			}

			internal string B(b P_0)
			{
				return P_0.A;
			}
		}

		public readonly Dictionary<string, b> A = new Dictionary<string, b>();

		public readonly List<string> A = new List<string>();

		public int A;

		public bool A;

		public string A
		{
			get
			{
				int num = this.A.Values.Count((b P_0) => P_0.A);
				return string.Format(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.T(), this.A.Count, num, this.A.Count - num);
			}
		}

		public IEnumerable<b> a => this.A.Values.OrderBy((b P_0) => P_0.A.Length).ThenBy((b P_0) => P_0.A, StringComparer.Ordinal);

		public string B()
		{
			StringBuilder stringBuilder = new StringBuilder(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.t());
			stringBuilder.AppendLine(this.A);
			stringBuilder.AppendLine(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.U());
			foreach (b item in a)
			{
				stringBuilder.AppendLine(item.A + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.u() + string.Join(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.V(), item.a));
				foreach (string item2 in item.A)
				{
					stringBuilder.AppendLine(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.v() + item2);
				}
			}
			stringBuilder.AppendLine(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.W());
			foreach (string item3 in this.A)
			{
				stringBuilder.AppendLine(item3);
			}
			if (this.A)
			{
				stringBuilder.AppendLine(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.w());
			}
			return stringBuilder.ToString();
		}
	}
}
namespace a
{
	internal sealed class C : Form
	{
		public w A;

		public float A;

		public float a;

		public float B;

		public int A;

		public string A = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.DM();

		public bool A;

		public bool a = true;

		public float b = 2f;

		public Color A = Color.White;

		protected override bool A => true;

		protected override CreateParams a
		{
			get
			{
				CreateParams obj = base.CreateParams;
				obj.ExStyle |= 134742176;
				return obj;
			}
		}

		public C()
		{
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.TopMost = true;
			BackColor = Color.FromArgb(1, 2, 3);
			base.TransparencyKey = BackColor;
			DoubleBuffered = true;
			base.AutoScaleMode = AutoScaleMode.None;
		}

		protected override void OnPaint(PaintEventArgs P_0)
		{
			if (!O.A || this.A == null)
			{
				return;
			}
			PointF pointF;
			bool num = this.A.A(this.A, this.a + 0.5f, B, base.Width, base.Height, out pointF);
			Color foreColor = (this.A ? Color.OrangeRed : Color.Aqua);
			c.A(P_0.Graphics, this.A, this.A, this.a, B, this.A, base.Width, base.Height, this.A, b, this.a);
			if (!num)
			{
				return;
			}
			using (Pen pen = new Pen(this.A, 2f))
			{
				P_0.Graphics.DrawEllipse(pen, pointF.X - 7f, pointF.Y - 7f, 14f, 14f);
			}
			using Font font = J.A(12f, FontStyle.Bold);
			TextRenderer.DrawText(P_0.Graphics, this.A, font, new Point((int)pointF.X + 12, (int)pointF.Y - 16), foreColor, Color.Black, TextFormatFlags.NoPrefix);
		}
	}
	internal static class c
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.O, bool> A;

			internal bool A(global::B.O P_0)
			{
				if (P_0.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba())
				{
					return P_0.b;
				}
				return false;
			}
		}

		public static Color A(IEnumerable<global::B.O> P_0)
		{
			return P_0.FirstOrDefault((global::B.O o2) => o2.a == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ba() && o2.b)?.A ?? Color.White;
		}

		public static void A(Graphics P_0, w P_1, float P_2, float P_3, float P_4, float P_5, int P_6, int P_7, Color P_8, float P_9, bool P_10)
		{
			if (!P_10 || P_1 == null)
			{
				return;
			}
			P_0.SmoothingMode = SmoothingMode.AntiAlias;
			using Pen pen = new Pen(P_8, P_9);
			int num = 192;
			float[] array = null;
			for (int num2 = 0; num2 <= num; num2++)
			{
				double num3 = (double)num2 * Math.PI * 2.0 / (double)num;
				float[] array2 = new float[3]
				{
					P_2 + (float)Math.Cos(num3) * P_5,
					P_3 + 0.3f,
					P_4 + (float)Math.Sin(num3) * P_5
				};
				if (array != null && global::B.F.A(P_1, array, array2, P_6, P_7, out var pt, out var pt2))
				{
					P_0.DrawLine(pen, pt, pt2);
				}
				array = array2;
			}
		}
	}
}
