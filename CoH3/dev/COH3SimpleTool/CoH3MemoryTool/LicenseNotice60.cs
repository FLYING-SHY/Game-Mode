using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class LicenseNotice60 : Form
{
	public LicenseNotice60(string title, string message, string button = "确认")
	{
		Text = title;
		base.ClientSize = new Size(480, 232);
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		BackColor = Palette46.Back;
		ForeColor = Palette46.Ink;
		Font = Interface45.Font();
		base.Icon = Interface45.Icon;
		Controls.Add(new Label
		{
			Text = title,
			Font = Interface45.Font(15f, FontStyle.Bold),
			ForeColor = Palette46.Ink,
			Bounds = new Rectangle(26, 23, 428, 36)
		});
		Controls.Add(new Label
		{
			Text = message,
			ForeColor = Palette46.Muted,
			Bounds = new Rectangle(26, 72, 428, 70)
		});
		Button confirm = new Button
		{
			Text = button,
			FlatStyle = FlatStyle.Flat,
			BackColor = Palette46.Panel,
			ForeColor = Palette46.Ink,
			Font = Interface45.Font(10f),
			Bounds = new Rectangle(330, 168, 124, 36),
			Cursor = Cursors.Hand
		};
		confirm.FlatAppearance.BorderColor = Palette46.Line;
		confirm.FlatAppearance.MouseOverBackColor = Palette46.Raised;
		Controls.Add(confirm);
		confirm.Click += delegate
		{
			DialogResult = DialogResult.OK;
			Close();
		};
		Paint += delegate(object s, PaintEventArgs e)
		{
			using Pen pen = new Pen(Palette46.Line);
			e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
		};
		base.KeyPreview = true;
		KeyDown += delegate(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Escape)
			{
				confirm.PerformClick();
			}
		};
	}
}
