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
		BackColor = Palette46.Back;
		ForeColor = Palette46.Ink;
		Font = Interface45.Font();
		base.Icon = Interface45.Icon;
		Surface46 surface = new Surface46
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(26)
		};
		base.Controls.Add(surface);
		surface.Controls.Add(new Label
		{
			Text = title,
			Font = Interface45.Font(17f, FontStyle.Bold),
			ForeColor = Palette46.Light,
			Bounds = new Rectangle(26, 23, 420, 36)
		});
		surface.Controls.Add(new Label
		{
			Text = message,
			ForeColor = Palette46.Ink,
			Bounds = new Rectangle(26, 80, 420, 66)
		});
		Action46 confirm = new Action46
		{
			Text = button,
			Primary = true,
			Bounds = new Rectangle(330, 170, 122, 36)
		};
		Action46 action = confirm;
		EventHandler value = delegate
		{
			base.DialogResult = DialogResult.OK;
			Close();
		};
		action.Click += value;
		surface.Controls.Add(confirm);
		base.KeyPreview = true;
		base.KeyDown += delegate(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Escape)
			{
				confirm.PerformClick();
			}
		};
	}
}
