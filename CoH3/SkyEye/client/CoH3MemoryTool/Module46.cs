using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Module46 : Panel
{
	public readonly FlowLayoutPanel Body;

	private readonly string title;

	private readonly string code;

	private bool layout;

	public Module46(string number, string name)
	{
		title = name;
		code = number;
		BackColor = Palette46.Panel;
		base.Width = 420;
		base.Margin = new Padding(0, 0, 0, 12);
		DoubleBuffered = true;
		Body = new FlowLayoutPanel
		{
			Location = new Point(15, 48),
			Width = 390,
			Height = 1,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			WrapContents = true,
			FlowDirection = FlowDirection.LeftToRight,
			BackColor = Palette46.Panel,
			Margin = Padding.Empty
		};
		base.Controls.Add(Body);
		FlowLayoutPanel body = Body;
		LayoutEventHandler value = delegate
		{
			Fit();
		};
		body.Layout += value;
		base.SizeChanged += delegate
		{
			Fit();
		};
	}

	public void Fit()
	{
		if (layout)
		{
			return;
		}
		layout = true;
		try
		{
			Body.MaximumSize = new Size(Math.Max(280, base.Width - 30), 0);
			Body.MinimumSize = new Size(Math.Max(280, base.Width - 30), 0);
			foreach (Control control in Body.Controls)
			{
				if (control.Tag as string == "full")
				{
					if (control is Fold46 fold)
					{
						fold.FitWidth(Body.MinimumSize.Width);
					}
					else
					{
						control.Width = Body.MinimumSize.Width;
					}
				}
			}
			Body.PerformLayout();
			base.Height = Body.Height + 63;
		}
		finally
		{
			layout = false;
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		Palette46.Frame(graphics, base.ClientRectangle, Palette46.Line);
		using (Pen pen = new Pen(Palette46.Line))
		{
			graphics.DrawLine(pen, 1, 38, base.Width - 3, 38);
			graphics.DrawLine(pen, 53, 10, 53, 29);
		}
		using (Font font = Interface45.Font(9f))
		{
			TextRenderer.DrawText(graphics, code, font, new Point(16, 12), Palette46.Muted, TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
		}
		using (Font font2 = Interface45.Font(11f, FontStyle.Bold))
		{
			TextRenderer.DrawText(graphics, title, font2, new Point(67, 9), Palette46.Ink, TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
		}
		using (Pen pen2 = new Pen(Palette46.Light))
		{
			graphics.DrawLine(pen2, base.Width - 42, 15, base.Width - 26, 15);
			graphics.DrawLine(pen2, base.Width - 42, 19, base.Width - 32, 19);
		}
		using Pen pen3 = new Pen(Color.FromArgb(28, 50, 66));
		graphics.DrawLine(pen3, 10, base.Height - 5, base.Width - 17, base.Height - 5);
	}
}
