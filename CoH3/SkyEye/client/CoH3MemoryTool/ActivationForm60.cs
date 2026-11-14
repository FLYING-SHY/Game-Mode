using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class ActivationForm60 : Form
{
	private readonly TextBox code = new TextBox();

	private readonly Toggle46 remember = new Toggle46();

	private readonly Label information = new Label();

	private readonly Action46 activate = new Action46();

	public event Action<string, bool> ActivateRequested;

	public ActivationForm60(LicenseCredentials60 saved)
	{
		Text = "天穹之眼 · 激活";
		base.ClientSize = new Size(690, 470);
		MinimumSize = base.Size;
		MaximumSize = base.Size;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.None;
		BackColor = Palette46.Back;
		ForeColor = Palette46.Ink;
		Font = Interface45.Font();
		base.Icon = Interface45.Icon;
		Surface46 surface = new Surface46
		{
			Dock = DockStyle.Fill
		};
		base.Controls.Add(surface);
		Label label = new Label
		{
			Text = "◉  天穹之眼",
			Font = Interface45.Font(23f, FontStyle.Bold),
			ForeColor = Palette46.Ink,
			Bounds = new Rectangle(30, 26, 440, 47)
		};
		surface.Controls.Add(label);
		Action46 action = new Action46
		{
			Text = "×",
			Bounds = new Rectangle(618, 28, 40, 30)
		};
		EventHandler value = delegate
		{
			Close();
		};
		action.Click += value;
		surface.Controls.Add(action);
		surface.Controls.Add(new Label
		{
			Text = "TACTICAL SYSTEM  /  授权登录",
			Font = Interface45.Font(9f),
			ForeColor = Palette46.Muted,
			Bounds = new Rectangle(34, 83, 550, 28)
		});
		Panel panel = new Panel
		{
			Bounds = new Rectangle(32, 126, 626, 236),
			BackColor = Palette46.Panel
		};
		surface.Controls.Add(panel);
		panel.Paint += delegate(object s, PaintEventArgs e)
		{
			Palette46.Frame(e.Graphics, panel.ClientRectangle, Palette46.Line);
		};
		panel.Controls.Add(new Label
		{
			Text = "激活码",
			ForeColor = Palette46.Muted,
			Bounds = new Rectangle(22, 19, 560, 25)
		});
		code.Text = saved.Code ?? "";
		code.UseSystemPasswordChar = true;
		code.BackColor = Palette46.Back;
		code.ForeColor = Palette46.Ink;
		code.Font = Interface45.Font(12f);
		code.BorderStyle = BorderStyle.FixedSingle;
		code.SetBounds(22, 52, 580, 33);
		code.MaxLength = 80;
		panel.Controls.Add(code);
		remember.Text = "记住激活码";
		remember.Checked = saved.Remember;
		remember.SetBounds(20, 99, 220, 30);
		panel.Controls.Add(remember);
		activate.Text = "激活并进入";
		activate.Primary = true;
		activate.SetBounds(418, 99, 184, 36);
		panel.Controls.Add(activate);
		activate.Click += delegate
		{
			Submit();
		};
		information.SetBounds(22, 151, 580, 70);
		information.ForeColor = Palette46.Amber;
		information.Text = "请先启动游戏并停留在大厅。";
		panel.Controls.Add(information);
		surface.Controls.Add(new Label
		{
			Text = "首次成功激活后开始计时 · 同一张激活码仅限一台电脑同时使用",
			ForeColor = Palette46.Muted,
			Font = Interface45.Font(8f),
			Bounds = new Rectangle(34, 384, 620, 28)
		});
		surface.Controls.Add(new Label
		{
			Text = "激活及使用期间会记录设备序列号和连接IP，用于授权与异常使用管理。",
			ForeColor = Palette46.Muted,
			Font = Interface45.Font(8f),
			Bounds = new Rectangle(34, 414, 620, 36)
		});
		code.KeyDown += delegate(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				Submit();
				e.SuppressKeyPress = true;
			}
		};
		MouseEventHandler value2 = delegate(object s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture60();
				SendMessage60(base.Handle, 161, new IntPtr(2), IntPtr.Zero);
			}
		};
		label.MouseDown += value2;
	}

	[DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
	private static extern bool ReleaseCapture60();

	[DllImport("user32.dll", EntryPoint = "SendMessageW")]
	private static extern IntPtr SendMessage60(IntPtr h, int m, IntPtr w, IntPtr l);

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
