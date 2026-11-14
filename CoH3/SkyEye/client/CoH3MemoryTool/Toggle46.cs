using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Toggle46 : Control
{
	private bool check;

	public bool Checked
	{
		get
		{
			return check;
		}
		set
		{
			if (check != value)
			{
				check = value;
				Invalidate();
				if (this.CheckedChanged != null)
				{
					this.CheckedChanged(this, EventArgs.Empty);
				}
			}
		}
	}

	public event EventHandler CheckedChanged;

	protected override void OnClick(EventArgs e)
	{
		Checked = !Checked;
		base.OnClick(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Space)
		{
			OnClick(EventArgs.Empty);
			e.Handled = true;
			e.SuppressKeyPress = true;
		}
		base.OnKeyDown(e);
	}

	public Toggle46()
	{
		base.TabStop = true;
		base.AccessibleRole = AccessibleRole.CheckButton;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		AutoSize = false;
		base.Size = new Size(190, 30);
		Font = Interface45.Font(9f);
		Cursor = Cursors.Hand;
		base.Margin = new Padding(0, 2, 6, 2);
		ForeColor = Palette46.Ink;
		BackColor = Palette46.Panel;
		SetStyle(ControlStyles.Opaque, value: true);
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
	}

	protected override void OnParentChanged(EventArgs e)
	{
		base.OnParentChanged(e);
		if (base.Parent != null)
		{
			BackColor = base.Parent.BackColor;
		}
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			graphics.FillRectangle(brush, base.ClientRectangle);
		}
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		Point[] points = new Point[8]
		{
			new Point(2, 10),
			new Point(7, 5),
			new Point(38, 5),
			new Point(43, 10),
			new Point(43, 20),
			new Point(38, 25),
			new Point(7, 25),
			new Point(2, 20)
		};
		using (SolidBrush brush2 = new SolidBrush(Checked ? Color.FromArgb(34, 96, 111) : Color.FromArgb(20, 34, 46)))
		{
			graphics.FillPolygon(brush2, points);
		}
		using (Pen pen = new Pen(Checked ? Palette46.Light : Palette46.Line))
		{
			graphics.DrawPolygon(pen, points);
		}
		int num = (Checked ? 28 : 6);
		using (SolidBrush brush3 = new SolidBrush(Checked ? Color.FromArgb(197, 248, 248) : Palette46.Muted))
		{
			graphics.FillRectangle(brush3, num, 8, 11, 14);
		}
		TextRenderer.DrawText(graphics, Text, Font, new Rectangle(53, 0, base.Width - 54, base.Height), base.Enabled ? Palette46.Ink : Palette46.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
		if (Focused && ShowFocusCues)
		{
			using (Pen pen2 = new Pen(Palette46.Light))
			{
				graphics.DrawLine(pen2, 53, base.Height - 2, base.Width - 3, base.Height - 2);
			}
		}
	}
}
