using System;
using System.Drawing;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>激活界面：输入卡密、显示授权状态。</summary>
internal sealed class ActivationForm60 : Form
{
	private readonly TextBox codeBox;

	private readonly CheckBox rememberBox;

	private readonly Label stateLabel;

	private readonly Label nicknameLabel;

	/// <summary>用户点击"激活"时触发：参数为（卡密，是否记住）。</summary>
	public event Action<string, bool> ActivateRequested;

	public ActivationForm60(LicenseCredentials60 credentials)
	{
		Text = "Bf6Esp 激活";
		ClientSize = new Size(470, 300);
		StartPosition = FormStartPosition.CenterScreen;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		BackColor = Color.FromArgb(24, 26, 31);
		ForeColor = Color.FromArgb(228, 232, 240);
		Font = new Font("Microsoft YaHei UI", 9f);
		Label title = new Label
		{
			Text = "Battlefield 6 ESP 授权",
			Font = new Font("Microsoft YaHei UI", 15f, FontStyle.Bold),
			ForeColor = Color.White,
			Bounds = new Rectangle(24, 18, 420, 34)
		};
		Label tip = new Label
		{
			Text = "请输入卡密完成激活。未授权时覆盖层不会启动。",
			Bounds = new Rectangle(24, 56, 420, 24)
		};
		codeBox = new TextBox
		{
			Bounds = new Rectangle(24, 92, 422, 28),
			CharacterCasing = CharacterCasing.Upper,
			Text = (credentials == null) ? "" : (credentials.Code ?? "")
		};
		rememberBox = new CheckBox
		{
			Text = "记住卡密（下次自动登录）",
			Checked = (credentials != null) && credentials.Remember,
			Bounds = new Rectangle(24, 128, 260, 24),
			ForeColor = Color.FromArgb(228, 232, 240)
		};
		stateLabel = new Label
		{
			Text = "未授权",
			Bounds = new Rectangle(24, 164, 422, 44),
			ForeColor = Color.FromArgb(255, 196, 92, 92)
		};
		nicknameLabel = new Label
		{
			Text = "",
			Bounds = new Rectangle(24, 208, 422, 24),
			ForeColor = Color.FromArgb(160, 170, 185)
		};
		Button activate = new Button
		{
			Text = "激活",
			Bounds = new Rectangle(24, 244, 120, 34),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(56, 122, 236),
			ForeColor = Color.White
		};
		activate.FlatAppearance.BorderSize = 0;
		activate.Click += delegate
		{
			string code = (codeBox.Text ?? "").Trim();
			Action<string, bool> handler = ActivateRequested;
			if (handler != null)
			{
				handler(code, rememberBox.Checked);
			}
		};
		Button quit = new Button
		{
			Text = "退出",
			Bounds = new Rectangle(326, 244, 120, 34),
			FlatStyle = FlatStyle.Flat,
			BackColor = Color.FromArgb(52, 56, 64),
			ForeColor = Color.White
		};
		quit.FlatAppearance.BorderSize = 0;
		quit.Click += delegate
		{
			Close();
		};
		Controls.Add(title);
		Controls.Add(tip);
		Controls.Add(codeBox);
		Controls.Add(rememberBox);
		Controls.Add(stateLabel);
		Controls.Add(nicknameLabel);
		Controls.Add(activate);
		Controls.Add(quit);
	}

	/// <summary>刷新授权状态显示。</summary>
	public void UpdateState(LicenseView60 view)
	{
		if (view == null)
		{
			return;
		}
		stateLabel.Text = LicenseStateText60.Describe(view);
		switch (view.State)
		{
			case "active":
				stateLabel.ForeColor = Color.FromArgb(120, 220, 130);
				break;
			case "network_wait":
			case "update_available":
				stateLabel.ForeColor = Color.FromArgb(255, 205, 120);
				break;
			default:
				stateLabel.ForeColor = Color.FromArgb(255, 140, 120);
				break;
		}
		nicknameLabel.Text = string.IsNullOrEmpty(view.Nickname) ? "" : ("当前 Steam 账号：" + view.Nickname);
	}
}
