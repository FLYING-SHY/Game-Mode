using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class BlackLoginForm : Form
{
	private readonly TextBox code = new TextBox();

	private readonly Toggle46 remember = new Toggle46();

	private readonly Label information = new Label();

	private readonly Button enter = new Button();

	public event Action<string, bool> ActivateRequested;

	public BlackLoginForm(LicenseCredentials60 saved)
	{
		Text = "COH3简易工具 · 激活";
		base.ClientSize = new Size(400, 212);
		base.MinimumSize = base.Size;
		base.MaximumSize = base.Size;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		BackColor = Palette46.Back;
		ForeColor = Palette46.Ink;
		Font = Interface45.Font();
		base.Icon = Interface45.Icon;
		Label label = new Label
		{
			Text = "激活码",
			ForeColor = Palette46.Muted,
			Font = Interface45.Font(9f),
			Bounds = new Rectangle(24, 20, 200, 22)
		};
		base.Controls.Add(label);
		code.Text = saved.Code ?? "";
		code.UseSystemPasswordChar = true;
		code.BackColor = Palette46.Panel;
		code.ForeColor = Palette46.Ink;
		code.Font = Interface45.Font(12f);
		code.BorderStyle = BorderStyle.None;
		code.SetBounds(30, 54, 340, 24);
		code.MaxLength = 80;
		code.GotFocus += delegate
		{
			code.Select(code.TextLength, 0);
			Invalidate();
		};
		code.LostFocus += delegate
		{
			Invalidate();
		};
		base.Controls.Add(code);
		remember.Text = "记住激活码";
		remember.Checked = (saved.Code == null) ? true : saved.Remember;
		remember.SetBounds(24, 90, 190, 30);
		base.Controls.Add(remember);
		enter.Text = "进入";
		enter.FlatStyle = FlatStyle.Flat;
		enter.FlatAppearance.BorderColor = Palette46.Line;
		enter.FlatAppearance.MouseOverBackColor = Palette46.Raised;
		enter.BackColor = Palette46.Panel;
		enter.ForeColor = Palette46.Ink;
		enter.Font = Interface45.Font(10f, FontStyle.Bold);
		enter.SetBounds(216, 90, 160, 34);
		enter.Cursor = Cursors.Hand;
		base.Controls.Add(enter);
		information.SetBounds(24, 138, 352, 48);
		information.ForeColor = Color.FromArgb(224, 98, 98);
		information.Font = Interface45.Font(9f);
		base.Controls.Add(information);
		enter.Click += delegate
		{
			Submit();
		};
		Paint += delegate(object s, PaintEventArgs e)
		{
			using Pen pen = new Pen(code.Focused ? Palette46.Light : Palette46.Line);
			e.Graphics.DrawRectangle(pen, 24, 46, 351, 33);
		};
		code.KeyDown += delegate(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				Submit();
				e.SuppressKeyPress = true;
			}
		};
	}

	private void Submit()
	{
		if (string.IsNullOrWhiteSpace(code.Text))
		{
			information.Text = "请输入激活码。";
			code.Focus();
			return;
		}
		Action<string, bool> activateRequested = this.ActivateRequested;
		if (activateRequested != null)
		{
			information.Text = "正在验证，请稍候。";
			activateRequested(code.Text, remember.Checked);
		}
	}

	public void UpdateState(LicenseView60 view)
	{
		if (!view.Silent && !string.IsNullOrEmpty(view.Message))
		{
			information.Text = view.Message;
		}
	}
}
