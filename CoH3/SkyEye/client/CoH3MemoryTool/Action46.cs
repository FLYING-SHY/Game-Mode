using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Action46 : Control
{
	public Func<Color> Swatch;

	private bool hover;

	public bool Primary { get; set; }

	public void PerformClick()
	{
		if (base.Enabled)
		{
			OnClick(EventArgs.Empty);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Return)
		{
			PerformClick();
			e.Handled = true;
			e.SuppressKeyPress = true;
		}
		base.OnKeyDown(e);
	}

	public Action46()
	{
		base.TabStop = true;
		base.AccessibleRole = AccessibleRole.PushButton;
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = Palette46.Panel;
		ForeColor = Palette46.Ink;
		base.Height = 30;
		base.Width = 120;
		Font = Interface45.Font(9f);
		base.Margin = new Padding(0, 2, 8, 4);
		Cursor = Cursors.Hand;
		SetStyle(ControlStyles.Opaque, value: true);
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: false);
	}

	protected override void OnMouseEnter(EventArgs e)
	{
		hover = true;
		Invalidate();
		base.OnMouseEnter(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		hover = false;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		using (SolidBrush brush = new SolidBrush(BackColor))
		{
			graphics.FillRectangle(brush, base.ClientRectangle);
		}
		using (SolidBrush brush2 = new SolidBrush((hover || Primary) ? Palette46.Raised : Palette46.Panel))
		{
			graphics.FillPolygon(brush2, Palette46.Shape(base.Width, base.Height, 5));
		}
		Palette46.Frame(graphics, base.ClientRectangle, (hover || Focused) ? Palette46.Light : Palette46.Line);
		TextRenderer.DrawText(graphics, Text, Font, new Rectangle((base.Width >= 40) ? 8 : 0, 0, (base.Width < 40) ? base.Width : (base.Width - ((Swatch == null) ? 16 : 35)), base.Height), base.Enabled ? Palette46.Ink : Palette46.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
		if (Swatch != null)
		{
			using SolidBrush brush3 = new SolidBrush(Swatch());
			graphics.FillRectangle(brush3, base.Width - 24, 8, 13, base.Height - 16);
		}
		if (Primary)
		{
			using (SolidBrush brush4 = new SolidBrush(Palette46.Light))
			{
				graphics.FillRectangle(brush4, 0, 7, 2, base.Height - 14);
			}
		}
	}
}
