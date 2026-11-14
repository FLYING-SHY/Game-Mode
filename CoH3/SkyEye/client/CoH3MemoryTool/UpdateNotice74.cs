using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class UpdateNotice74 : Form
{
	private bool mayClose;

	private LicenseView60 current;

	private readonly bool requiredUpdate;

	private Label heading;

	private TextBox notes;

	public UpdateNotice74(LicenseView60 view, bool required)
	{
		current = view;
		requiredUpdate = required;
		Text = (required ? "软件需要更新" : "发现新版本");
		base.ClientSize = new Size(590, 410);
		MinimumSize = base.Size;
		MaximumSize = base.Size;
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		base.ControlBox = false;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		BackColor = Palette46.Back;
		ForeColor = Palette46.Ink;
		Font = Interface45.Font();
		base.Icon = Interface45.Icon;
		heading = new Label
		{
			Text = (required ? "请更新软件 · " : "发现新版本 ") + view.Latest,
			Font = Interface45.Font(17f, FontStyle.Bold),
			ForeColor = Palette46.Light,
			Bounds = new Rectangle(24, 20, 542, 42)
		};
		base.Controls.Add(heading);
		base.Controls.Add(new Label
		{
			Text = (required ? "当前版本低于最低允许版本。" : "当前功能继续运行。此窗口打开期间不能修改软件设置。"),
			Bounds = new Rectangle(24, 70, 542, 42)
		});
		notes = new TextBox
		{
			Text = (view.Notes ?? ""),
			ReadOnly = true,
			Multiline = true,
			ScrollBars = ScrollBars.Vertical,
			BackColor = Palette46.Panel,
			ForeColor = Palette46.Ink,
			Bounds = new Rectangle(24, 120, 542, 210),
			TabStop = false
		};
		base.Controls.Add(notes);
		Button button = new Button
		{
			Text = "更新",
			Bounds = new Rectangle(420, 350, 146, 36)
		};
		base.Controls.Add(button);
		base.AcceptButton = button;
		button.Click += delegate
		{
			if (LicenseVersion60.DownloadAllowed(current.DownloadUrl))
			{
				try
				{
					Process.Start(new ProcessStartInfo(current.DownloadUrl)
					{
						UseShellExecute = true
					});
				}
				catch (Win32Exception)
				{
					MessageBox.Show(this, "无法打开下载地址，请联系卖家。", "更新");
					return;
				}
			}
			Dismiss();
		};
		base.FormClosing += delegate(object sender, FormClosingEventArgs e)
		{
			if (!mayClose && e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
			}
		};
	}

	public void RefreshView(LicenseView60 view)
	{
		current = view;
		heading.Text = (requiredUpdate ? "请更新软件 · " : "发现新版本 ") + view.Latest;
		notes.Text = view.Notes ?? "";
	}

	public void Dismiss()
	{
		mayClose = true;
		Close();
	}
}
