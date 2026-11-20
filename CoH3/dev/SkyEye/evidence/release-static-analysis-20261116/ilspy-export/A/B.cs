using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;

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
	internal sealed class B
	{
		public float A;

		public float a;

		public float B;

		public float b;

		public Color A;
	}
	internal sealed class b : Form
	{
		public v A;

		public float A;

		public float a;

		public float B;

		public int A;

		public string A = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Dl();

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

		public b()
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
			if (!N.A || this.A == null)
			{
				return;
			}
			PointF pointF;
			bool num = this.A.A(this.A, this.a + 0.5f, B, base.Width, base.Height, out pointF);
			Color foreColor = (this.A ? Color.OrangeRed : Color.Aqua);
			C.A(P_0.Graphics, this.A, this.A, this.a, B, this.A, base.Width, base.Height, this.A, b, this.a);
			if (!num)
			{
				return;
			}
			using (Pen pen = new Pen(this.A, 2f))
			{
				P_0.Graphics.DrawEllipse(pen, pointF.X - 7f, pointF.Y - 7f, 14f, 14f);
			}
			using Font font = I.A(12f, FontStyle.Bold);
			TextRenderer.DrawText(P_0.Graphics, this.A, font, new Point((int)pointF.X + 12, (int)pointF.Y - 16), foreColor, Color.Black, TextFormatFlags.NoPrefix);
		}
	}
}
