using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>授权相关的提示窗（纯 WinForms，BF6 工具线自有样式）。</summary>
internal sealed class LicenseNotice60 : Form
{
	public LicenseNotice60(string title, string message, string button = "确认")
	{
		Text = title;
		base.ClientSize = new Size(480, 220);
		base.StartPosition = FormStartPosition.CenterParent;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.BackColor = Color.FromArgb(24, 26, 31);
		base.ForeColor = Color.FromArgb(228, 232, 240);
		base.Font = new Font("Microsoft YaHei UI", 9f);
		Label head = new Label
		{
			Text = title,
			Font = new Font("Microsoft YaHei UI", 14f, FontStyle.Bold),
			ForeColor = Color.White,
			Bounds = new Rectangle(24, 20, 430, 34)
		};
		Label body = new Label
		{
			Text = message,
			Bounds = new Rectangle(24, 72, 430, 76)
		};
		Button confirm = new Button
		{
			Text = button,
			Bounds = new Rectangle(348, 158, 100, 32),
			DialogResult = DialogResult.OK,
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(56, 122, 236),
			ForeColor = Color.White
		};
		confirm.FlatAppearance.BorderSize = 0;
		base.Controls.Add(head);
		base.Controls.Add(body);
		base.Controls.Add(confirm);
		base.AcceptButton = confirm;
		base.CancelButton = confirm;
	}
}
