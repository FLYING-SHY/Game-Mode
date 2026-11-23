using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Bf6Esp;

/// <summary>
/// 应用外壳：负责激活界面与主控窗口的切换，并在授权失效时立刻停掉 ESP。
/// 未授权 / 过期 / 设备不匹配时一律不启动覆盖层。
/// </summary>
internal sealed class LicenseApplication60 : ApplicationContext
{
	private readonly LicenseCoordinator60 client;

	private readonly ActivationForm60 login;

	private EspHostForm60 host;

	private readonly Timer enforcement = new Timer();

	private bool closing;

	private bool terminal;

	private bool wasAllowed;

	public LicenseApplication60(LicenseCoordinator60 coordinator = null)
	{
		client = coordinator ?? new LicenseCoordinator60();
		login = new ActivationForm60(client.Credentials);
		_ = login.Handle;
		login.ActivateRequested += delegate(string code, bool remember)
		{
			client.Submit(code, remember);
		};
		login.FormClosing += Closing;
		SystemEvents.PowerModeChanged += PowerChanged60;
		client.Changed += delegate(LicenseView60 view)
		{
			if (!closing && !login.IsDisposed)
			{
				try
				{
					login.BeginInvoke((Action)delegate
					{
						Apply(view);
					});
				}
				catch (InvalidOperationException)
				{
				}
			}
		};
		enforcement.Interval = 200;
		enforcement.Tick += delegate
		{
			bool allowed = LicenseGate60.Allowed;
			if (allowed != wasAllowed)
			{
				wasAllowed = allowed;
				if (host != null)
				{
					if (allowed)
					{
						host.ResumeEsp60();
					}
					else
					{
						host.StopEsp60();
					}
				}
			}
		};
		enforcement.Start();
		login.Show();
		client.Start();
	}

	private void EnsureHost()
	{
		if (host == null)
		{
			host = new EspHostForm60();
			host.FormClosing += Closing;
			host.ChangeLicense60 += delegate
			{
				client.PauseForLogin();
				host.StopEsp60();
				login.Show();
				login.Activate();
			};
		}
	}

	private void Apply(LicenseView60 view)
	{
		if (closing || terminal || !client.IsCurrent(view))
		{
			return;
		}
		login.UpdateState(view);
		switch (view.State)
		{
			case "active":
				EnsureHost();
				host.SetLicenseState(view);
				host.ResumeEsp60();
				wasAllowed = true;
				if (login.Visible)
				{
					login.Hide();
					host.Show();
					host.Activate();
				}
				break;
			case "expired":
				Terminal("卡密已到期", "卡密已到期，请重新购买或续费。");
				break;
			case "update_required":
				Terminal("需要更新", "当前版本需要更新后才能继续使用。");
				break;
			case "blocked":
				if (host != null)
				{
					host.StopEsp60();
					host.SetLicenseState(view);
				}
				break;
			case "login_required":
			case "in_use":
				if (host != null)
				{
					host.StopEsp60();
					host.Hide();
				}
				login.Show();
				break;
			default:
				if (!LicenseGate60.Allowed && host != null)
				{
					host.StopEsp60();
					host.SetLicenseState(view);
				}
				break;
		}
	}

	private void Terminal(string title, string message)
	{
		terminal = true;
		LicenseGate60.Revoke();
		if (host != null)
		{
			host.StopEsp60();
			host.SetLicenseState(new LicenseView60 { State = "expired", Message = message });
		}
		Form owner = ((host != null && host.Visible) ? ((Form)host) : ((Form)login));
		using (LicenseNotice60 notice = new LicenseNotice60(title, message))
		{
			notice.ShowDialog(owner);
		}
		Shutdown();
	}

	private void Closing(object sender, FormClosingEventArgs e)
	{
		if (!closing)
		{
			e.Cancel = true;
			Shutdown();
		}
	}

	private void PowerChanged60(object sender, PowerModeChangedEventArgs e)
	{
		if (e.Mode == PowerModes.Resume)
		{
			client.RevalidateAfterResume();
		}
	}

	private async void Shutdown()
	{
		if (closing)
		{
			return;
		}
		closing = true;
		SystemEvents.PowerModeChanged -= PowerChanged60;
		enforcement.Stop();
		LicenseGate60.Revoke();
		if (host != null)
		{
			host.StopEsp60();
			host.Hide();
		}
		login.Hide();
		try
		{
			await client.CloseAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			if (host != null)
			{
				host.Close();
			}
			login.Close();
			enforcement.Dispose();
			ExitThread();
		}
	}

	protected override void ExitThreadCore()
	{
		client.Dispose();
		base.ExitThreadCore();
	}
}
