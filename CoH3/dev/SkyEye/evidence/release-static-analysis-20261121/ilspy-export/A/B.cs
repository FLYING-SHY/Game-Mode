using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace A
{
	[CompilerGenerated]
	[a]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class B : Attribute
	{
		public readonly int A;

		public B(int P_0)
		{
			A = P_0;
		}
	}
	internal sealed class b
	{
		public string A;

		public readonly SortedSet<string> A = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

		public readonly SortedSet<string> a = new SortedSet<string>(StringComparer.Ordinal);

		public bool A;
	}
}
namespace a
{
	internal static class B
	{
		public static PointF[] A(b P_0, z P_1, int P_2, int P_3)
		{
			PointF[] array = new PointF[193];
			for (int num = 0; num < array.Length; num++)
			{
				double num2 = (double)num * Math.PI / 96.0;
				ref PointF reference = ref array[num];
				reference = P_1.A(P_0.A + (float)Math.Cos(num2) * P_0.B, P_0.a + (float)Math.Sin(num2) * P_0.B, P_2, P_3);
			}
			return array;
		}

		public static void A(Graphics P_0, b P_1, z P_2, int P_3, int P_4)
		{
			if (P_1 == null || P_2 == null || !P_2.A)
			{
				return;
			}
			GraphicsState gstate = P_0.Save();
			try
			{
				using (GraphicsPath graphicsPath = new GraphicsPath())
				{
					graphicsPath.AddPolygon(P_2.A(P_3, P_4));
					P_0.SetClip(graphicsPath, CombineMode.Intersect);
				}
				P_0.SmoothingMode = SmoothingMode.AntiAlias;
				using Pen pen = new Pen(P_1.A, P_1.b);
				P_0.DrawLines(pen, A(P_1, P_2, P_3, P_4));
				if (P_2.A(P_1.A, P_1.a, P_3, P_4, out var pointF))
				{
					P_0.DrawEllipse(pen, pointF.X - 2.5f, pointF.Y - 2.5f, 5f, 5f);
				}
			}
			finally
			{
				P_0.Restore(gstate);
			}
		}
	}
	internal sealed class b
	{
		public float A;

		public float a;

		public float B;

		public float b;

		public Color A;
	}
}
