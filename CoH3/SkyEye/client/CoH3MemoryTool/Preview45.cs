using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Preview45 : Panel
{
	public Preview45()
	{
		DoubleBuffered = true;
		BackColor = Interface45.Night;
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		base.OnPaintBackground(e);
		using (Pen pen = new Pen(Color.FromArgb(29, 47, 64)))
		{
			for (int i = 0; i < base.Width; i += 40)
			{
				e.Graphics.DrawLine(pen, i, 0, i, base.Height);
			}
			for (int j = 0; j < base.Height; j += 40)
			{
				e.Graphics.DrawLine(pen, 0, j, base.Width, j);
			}
		}
		using Pen pen2 = new Pen(Color.FromArgb(72, 111, 135));
		e.Graphics.DrawLine(pen2, 12, 12, 32, 12);
		e.Graphics.DrawLine(pen2, 12, 12, 12, 32);
		e.Graphics.DrawLine(pen2, base.Width - 13, base.Height - 13, base.Width - 33, base.Height - 13);
		e.Graphics.DrawLine(pen2, base.Width - 13, base.Height - 13, base.Width - 13, base.Height - 33);
	}
}
