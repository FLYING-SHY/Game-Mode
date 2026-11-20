using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using B;
using a;

namespace B
{
	internal static class H
	{
		private static readonly object m_A = new object();

		public static void A(string P_0, string P_1)
		{
			try
			{
				lock (H.m_A)
				{
					string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eu());
					if (File.Exists(text) && new FileInfo(text).Length > 262144)
					{
						File.WriteAllText(text, C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D());
					}
					File.AppendAllText(text, DateTime.Now.ToString(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.az()) + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + P_0 + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BA() + P_1 + Environment.NewLine);
				}
			}
			catch
			{
			}
		}
	}
	internal static class h
	{
		public const TextFormatFlags A = TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;

		public static int A(Graphics P_0, string[] P_1, Font P_2, Color P_3, Color P_4, int P_5, bool P_6, int P_7 = 0, Color[] P_8 = null, g[][] P_9 = null, double P_10 = -1.0)
		{
			using Font font = new Font(P_2.FontFamily, (P_7 > 0) ? ((float)P_7) : P_2.Size, P_2.Style);
			int num = 5;
			for (int num2 = 0; num2 < P_1.Length; num2++)
			{
				Font font2 = ((num2 % 2 == 0) ? P_2 : font);
				if (string.IsNullOrEmpty(P_1[num2]))
				{
					continue;
				}
				bool flag = P_9 != null && num2 < P_9.Length && P_9[num2].Length != 0;
				Color color = ((P_8 != null && num2 < P_8.Length) ? P_8[num2] : ((num2 % 2 == 0) ? P_3 : P_4));
				int num3 = (flag ? Y.A(P_0, P_1[num2], font2, new Rectangle(7, num, Math.Max(20, P_5 - 14), 32767), color, P_9[num2], P_6, (P_10 < 0.0) ? W.A : P_10) : TextRenderer.MeasureText(P_0, P_1[num2], font2, new Size(Math.Max(20, P_5 - 14), 32767), TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height);
				Rectangle rectangle = new Rectangle(7, num, Math.Max(20, P_5 - 14), num3);
				if (P_6 && !flag)
				{
					Point[] array = new Point[4]
					{
						new Point(-1, 0),
						new Point(1, 0),
						new Point(0, -1),
						new Point(0, 1)
					};
					foreach (Point pos in array)
					{
						Rectangle bounds = rectangle;
						bounds.Offset(pos);
						TextRenderer.DrawText(P_0, P_1[num2], font2, bounds, Color.Black, TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
					}
					TextRenderer.DrawText(P_0, P_1[num2], font2, rectangle, (P_8 != null && num2 < P_8.Length) ? P_8[num2] : ((num2 % 2 == 0) ? P_3 : P_4), TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
				}
				num += num3 + ((num2 % 2 != 1) ? 1 : 7);
			}
			return num + 3;
		}
	}
}
namespace b
{
	internal static class H
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class A
		{
			public static readonly A A = new A();

			public static Func<global::B.Z, bool> A;

			internal bool A(global::B.Z P_0)
			{
				if (!P_0.a)
				{
					return !string.IsNullOrEmpty(P_0.a);
				}
				return false;
			}
		}

		public static void A(Graphics P_0, IEnumerable<global::B.Z> P_1, global::a.y P_2, global::B.B P_3, h P_4, int P_5, int P_6, IEnumerable<global::A.s> P_7)
		{
			if (!P_4.A || P_2 == null || !P_2.A)
			{
				return;
			}
			global::B.B b2 = new global::B.B
			{
				A = false,
				a = true,
				B = P_3.B,
				b = P_3.b,
				a = P_3.a,
				B = P_3.B,
				A = P_3.A,
				C = P_3.C,
				c = P_3.c
			};
			global::a.Z.A(P_0, P_1, P_2, b2, P_5, P_6, P_7);
			GraphicsState gstate = P_0.Save();
			try
			{
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					graphicsPath.AddPolygon(P_2.A(P_5, P_6));
					P_0.SetClip(graphicsPath, CombineMode.Intersect);
				}
				P_0.SmoothingMode = SmoothingMode.AntiAlias;
				using Font font = new Font(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ba(), P_4.A, FontStyle.Bold, GraphicsUnit.Pixel);
				using StringFormat format = new StringFormat
				{
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Far
				};
				foreach (global::B.Z item in P_1.Where((global::B.Z z) => !z.a && !string.IsNullOrEmpty(z.a)))
				{
					if (!P_2.A(item.A, item.B, P_5, P_6, out var origin))
					{
						continue;
					}
					float num = (item.A ? P_3.B : P_3.a);
					origin.Y -= num / 2f + 3f;
					using GraphicsPath graphicsPath2 = new GraphicsPath();
					graphicsPath2.AddString(item.a, font.FontFamily, 1, P_4.A, origin, format);
					using Pen pen = new Pen(Color.Black, 3f)
					{
						LineJoin = LineJoin.Round
					};
					using SolidBrush brush = new SolidBrush(item.A);
					P_0.DrawPath(pen, graphicsPath2);
					P_0.FillPath(brush, graphicsPath2);
				}
			}
			finally
			{
				P_0.Restore(gstate);
			}
		}
	}
	internal sealed class h
	{
		public bool A = true;

		public int A = 12;

		public IEnumerable<string> A()
		{
			return new string[2]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LI() + (this.A ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Li() + this.A
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
			if (text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LJ() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Lj())
			{
				return false;
			}
			if (int.TryParse(P_0.Substring(num + 1), out var result))
			{
				if (text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.LJ())
				{
					this.A = result != 0;
				}
				else
				{
					this.A = Math.Max(8, Math.Min(40, result));
				}
			}
			return true;
		}
	}
}
