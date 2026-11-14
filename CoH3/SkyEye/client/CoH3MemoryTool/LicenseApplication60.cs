using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CoH3MemoryTool;

internal sealed class LicenseApplication60 : ApplicationContext
{
	private readonly LicenseCoordinator60 client;

	private readonly ActivationForm60 login;

	private OverlaySettingsForm overlay;

	private readonly Timer enforcement = new Timer();

	private bool closing;

	private bool terminal;

	private bool wasAllowed;

	private long expires;

	private UpdateNotice74 updateNotice;

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
		enforcement.Interval = 100;
		enforcement.Tick += delegate
		{
			bool allowed = LicenseGate60.Allowed;
			if (allowed != wasAllowed)
			{
				wasAllowed = allowed;
				if (overlay != null)
				{
					if (allowed)
					{
						overlay.ResumeProtected60();
					}
					else
					{
						overlay.StopProtected60();
					}
				}
			}
			if (!allowed)
			{
				AudioClip.StopAll60();
			}
		};
		enforcement.Start();
		login.Show();
		client.Start();
	}

	private void EnsureOverlay()
	{
		if (overlay == null)
		{
			overlay = new OverlaySettingsForm();
			overlay.FormClosing += Closing;
			overlay.ChangeLicense60 += delegate
			{
				client.PauseForLogin();
				overlay.StopProtected60();
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
		if (this.updateNotice != null && (view.State == "blocked" || view.State == "expired" || view.State == "login_required" || view.State == "update_required"))
		{
			this.updateNotice.Dismiss();
		}
		if (view.State == "update_available")
		{
			if (!LicenseGate60.Allowed || overlay == null)
			{
				return;
			}
			if (this.updateNotice == null)
			{
				using (UpdateNotice74 updateNotice = new UpdateNotice74(view, required: false))
				{
					this.updateNotice = updateNotice;
					try
					{
						updateNotice.ShowDialog(overlay);
						return;
					}
					finally
					{
						this.updateNotice = null;
					}
				}
			}
			this.updateNotice.RefreshView(view);
		}
		else if (view.State == "active" || view.State == "blocked")
		{
			expires = ((view.Expires > 0) ? view.Expires : expires);
			EnsureOverlay();
			if (view.State == "active")
			{
				overlay.SetLicenseExpiry60(expires, view.LicenseKind);
				overlay.ResumeProtected60();
				wasAllowed = true;
			}
			else
			{
				overlay.StopProtected60();
				wasAllowed = false;
			}
			if (login.Visible)
			{
				login.Hide();
				overlay.Show();
				overlay.Activate();
			}
		}
		else if (view.State == "expired" || view.State == "update_required")
		{
			terminal = true;
			LicenseGate60.Revoke();
			if (overlay != null)
			{
				overlay.StopProtected60();
			}
			Form owner = ((overlay != null && overlay.Visible) ? ((Form)overlay) : ((Form)login));
			if (view.State == "update_required")
			{
				using UpdateNotice74 updateNotice2 = new UpdateNotice74(view, required: true);
				updateNotice2.ShowDialog(owner);
			}
			else
			{
				using LicenseNotice60 licenseNotice = new LicenseNotice60("卡密已到期", "卡密已到期");
				licenseNotice.ShowDialog(owner);
			}
			Shutdown();
		}
		else if (view.State == "login_required" || view.State == "in_use")
		{
			if (overlay != null)
			{
				overlay.StopProtected60();
				overlay.Hide();
			}
			login.Show();
		}
		else if (!LicenseGate60.Allowed && overlay != null)
		{
			overlay.StopProtected60();
		}
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
		if (updateNotice != null)
		{
			updateNotice.Dismiss();
		}
		SystemEvents.PowerModeChanged -= PowerChanged60;
		enforcement.Stop();
		LicenseGate60.Revoke();
		if (overlay != null)
		{
			overlay.StopProtected60();
		}
		login.Hide();
		if (overlay != null)
		{
			overlay.Hide();
		}
		try
		{
			await client.CloseAsync();
		}
		finally
		{
			if (overlay != null)
			{
				overlay.Close();
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
