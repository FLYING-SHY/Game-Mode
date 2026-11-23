using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using a;

namespace B;

internal sealed class P : Panel
{
	public P()
	{
		DoubleBuffered = true;
		BackColor = global::a.J.D;
	}

	protected override void OnPaintBackground(PaintEventArgs P_0)
	{
		base.OnPaintBackground(P_0);
		using (Pen pen = new Pen(Color.FromArgb(29, 47, 64)))
		{
			for (int num = 0; num < base.Width; num += 40)
			{
				P_0.Graphics.DrawLine(pen, num, 0, num, base.Height);
			}
			for (int num2 = 0; num2 < base.Height; num2 += 40)
			{
				P_0.Graphics.DrawLine(pen, 0, num2, base.Width, num2);
			}
		}
		using Pen pen2 = new Pen(Color.FromArgb(72, 111, 135));
		P_0.Graphics.DrawLine(pen2, 12, 12, 32, 12);
		P_0.Graphics.DrawLine(pen2, 12, 12, 12, 32);
		P_0.Graphics.DrawLine(pen2, base.Width - 13, base.Height - 13, base.Width - 33, base.Height - 13);
		P_0.Graphics.DrawLine(pen2, base.Width - 13, base.Height - 13, base.Width - 13, base.Height - 33);
	}
}
internal sealed class p : SafeHandleZeroOrMinusOneIsInvalid
{
	public p()
		: base(ownsHandle: true)
	{
	}

	protected override bool ReleaseHandle()
	{
		return H.A(handle);
	}
}
