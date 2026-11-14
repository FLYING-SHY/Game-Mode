using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Surface46 : Panel
{
	public Surface46()
	{
		DoubleBuffered = true;
		BackColor = Palette46.Back;
	}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		base.OnPaintBackground(e);
		using (Pen pen = new Pen(Color.FromArgb(14, 29, 41)))
		{
			for (int i = 20; i < base.Width; i += 48)
			{
				e.Graphics.DrawLine(pen, i, 0, i, base.Height);
			}
			for (int j = 20; j < base.Height; j += 48)
			{
				e.Graphics.DrawLine(pen, 0, j, base.Width, j);
			}
		}
		Palette46.Frame(e.Graphics, new Rectangle(0, 0, base.Width, base.Height), Palette46.Line);
		using Pen pen2 = new Pen(Palette46.Light);
		e.Graphics.DrawLine(pen2, 12, 4, 70, 4);
		e.Graphics.DrawLine(pen2, base.Width - 12, base.Height - 5, base.Width - 70, base.Height - 5);
	}
}
