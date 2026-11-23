using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>
/// 授权通过后的主控面板：显示授权状态、挂接游戏进程、驱动覆盖层。
/// 面板本身很小，只显示状态与快捷键提示；绘制全部交给 OverlayWindow。
/// </summary>
internal sealed class EspHostForm60 : Form
{
	private readonly EspSettings settings;

	private readonly OverlayWindow overlay = new OverlayWindow();

	private readonly HotkeyWindow60 hotkey = new HotkeyWindow60();

	private readonly Label stateLabel;

	private readonly Label hintLabel;

	private readonly Timer attachTimer = new Timer();

	private EspEngine60 engine;

	private GameProcess game;

	private MemoryReader reader;

	private OffsetProfile profile;

	public EspHostForm60()
	{
		settings = EspSettings.Load(EspSettings.DefaultPath);
		Text = "Bf6Esp";
		FormBorderStyle = FormBorderStyle.None;
		ShowInTaskbar = false;
		TopMost = true;
		StartPosition = FormStartPosition.Manual;
		ClientSize = new Size(340, 62);
		BackColor = Color.FromArgb(20, 22, 27);
		ForeColor = Color.FromArgb(228, 232, 240);
		Font = new Font("Microsoft YaHei UI", 9f);
		Location = new Point(16, 16);
		stateLabel = new Label
		{
			Text = "未授权",
			Location = new Point(12, 8),
			Size = new Size(316, 22),
			ForeColor = Color.FromArgb(255, 140, 120)
		};
		hintLabel = new Label
		{
			Text = "等待游戏进程…",
			Location = new Point(12, 34),
			Size = new Size(316, 20),
			ForeColor = Color.FromArgb(150, 160, 175)
		};
		Controls.Add(stateLabel);
		Controls.Add(hintLabel);
		ContextMenuStrip menu = new ContextMenuStrip();
		menu.Items.Add("打开设置", null, delegate
		{
			OpenSettings();
		});
		menu.Items.Add("重新登录", null, delegate
		{
			Action handler = ChangeLicense60;
			if (handler != null)
			{
				handler();
			}
		});
		menu.Items.Add("退出", null, delegate
		{
			Close();
		});
		ContextMenuStrip = menu;
		hotkey.Triggered += ToggleOverlay;
		attachTimer.Interval = 1000;
		attachTimer.Tick += delegate
		{
			if (engine == null && LicenseGate60.Allowed)
			{
				ResumeEsp60();
			}
		};
		attachTimer.Start();
	}

	/// <summary>用户请求重新登录时触发。</summary>
	public event Action ChangeLicense60;

	/// <summary>刷新授权状态显示。</summary>
	public void SetLicenseState(LicenseView60 view)
	{
		if (view == null)
		{
			return;
		}
		stateLabel.Text = LicenseStateText60.Describe(view);
		if (view.State == "active")
		{
			stateLabel.ForeColor = Color.FromArgb(120, 220, 130);
		}
		else if (view.State == "network_wait" || view.State == "update_available")
		{
			stateLabel.ForeColor = Color.FromArgb(255, 205, 120);
		}
		else
		{
			stateLabel.ForeColor = Color.FromArgb(255, 140, 120);
		}
	}

	/// <summary>授权有效时启动（或恢复）ESP。</summary>
	public void ResumeEsp60()
	{
		if (engine != null)
		{
			return;
		}
		if (!LicenseGate60.Allowed)
		{
			hintLabel.Text = "未授权，覆盖层未启动。";
			return;
		}
		game = GameProcess.Attach();
		if (game == null)
		{
			hintLabel.Text = "未检测到 Battlefield 6 进程（bf6.exe）。";
			return;
		}
		profile = ResolveProfile(game);
		reader = new MemoryReader(game.Handle);
		IEntitySource fallback = new ProfileEntitySource(reader, profile, null, delegate(string message)
		{
			Log(message);
		});
		RuntimeEntitySource source = new RuntimeEntitySource(reader, new RuntimeLayout60(), fallback, delegate(string message)
		{
			Log(message);
		});
		engine = new EspEngine60(game, reader, source, settings, overlay, hotkey, delegate(string message)
		{
			Log(message);
		});
		engine.Start();
		hintLabel.Text = "运行中 · 热键 " + settings.HotkeyName + " 切换覆盖层 · 实体 " + source.Name;
		StartRuntimeLocator(source);
	}

	/// <summary>后台跑运行时定位器：把现找的实体表、矩阵、字段偏移灌进覆盖层。</summary>
	private void StartRuntimeLocator(RuntimeEntitySource source)
	{
		GameProcess attached = game;
		MemoryReader activeReader = reader;
		OffsetProfile activeProfile = profile;
		System.Threading.ThreadPool.QueueUserWorkItem(delegate		{
			try
			{
				RuntimeLayout60 layout = RuntimeLocator60.Locate(activeReader, attached, activeProfile, delegate(string message)
				{
					Log(message);
				});
				source.ApplyLayout(layout);
				foreach (string note in layout.Notes)
				{
					Log("[定位] " + note);
				}
			}
			catch (Exception ex)
			{
				Log("[定位] 失败：" + ex.Message);
			}
		});
	}

	/// <summary>授权失效时立即停掉覆盖层。</summary>
	public void StopEsp60()
	{
		if (engine != null)
		{
			engine.Stop();
			engine.Dispose();
			engine = null;
		}
		if (reader != null)
		{
			reader = null;
		}
		if (game != null)
		{
			game.Dispose();
			game = null;
		}
		try
		{
			overlay.Clear();
		}
		catch (Exception)
		{
		}
		hintLabel.Text = "覆盖层已停止。";
	}

	private void ToggleOverlay()
	{
		if (engine != null)
		{
			engine.ToggleOverlay();
			hintLabel.Text = "覆盖层已" + (settings.OverlayEnabled ? "显示" : "隐藏") + " · 热键 " + settings.HotkeyName;
		}
	}

	private void OpenSettings()
	{
		using (SettingsForm form = new SettingsForm(settings))
		{
			if (form.ShowDialog(this) == DialogResult.OK)
			{
				settings.BoxStyle = form.Result.BoxStyle;
				settings.BoxThickness = form.Result.BoxThickness;
				settings.MaxDistance = form.Result.MaxDistance;
				settings.OnlyEnemies = form.Result.OnlyEnemies;
				settings.OnlyAlive = form.Result.OnlyAlive;
				settings.ShowHealthBar = form.Result.ShowHealthBar;
				settings.ShowDistance = form.Result.ShowDistance;
				settings.ShowName = form.Result.ShowName;
				settings.ShowSnapLine = form.Result.ShowSnapLine;
				settings.ShowFovCircle = form.Result.ShowFovCircle;
				settings.FovRadius = form.Result.FovRadius;
				settings.ShowOffscreenArrow = form.Result.ShowOffscreenArrow;
				settings.EnemyColor = form.Result.EnemyColor;
				settings.FriendlyColor = form.Result.FriendlyColor;
				settings.NeutralColor = form.Result.NeutralColor;
				settings.TextColor = form.Result.TextColor;
				settings.SnapLineColor = form.Result.SnapLineColor;
				settings.HealthHighColor = form.Result.HealthHighColor;
				settings.HealthMidColor = form.Result.HealthMidColor;
				settings.HealthLowColor = form.Result.HealthLowColor;
				if (form.Result.HotkeyName != settings.HotkeyName)
				{
					settings.HotkeyName = form.Result.HotkeyName;
					if (engine != null)
					{
						hotkey.Register(settings.HotkeyName);
					}
				}
				settings.Save(EspSettings.DefaultPath);
			}
		}
	}

	/// <summary>按 SHA-256 选档案；不匹配时退回签名解析并给出提示。</summary>
	private OffsetProfile ResolveProfile(GameProcess attached)
	{
		string directory = ProfileStore.LocateProfilesDirectory();
		List<OffsetProfile> profiles = ProfileStore.LoadAll(directory);
		string sha = ProfileStore.ComputeSha256(attached.ExePath);
		OffsetProfile matched = ProfileStore.MatchByHash(profiles, sha);
		if (matched != null)
		{
			Log("[档案] 命中 build " + Short(matched.BuildSha256));
			return matched;
		}
		Log("[档案] 版本不匹配：当前 build " + Short(sha) + "，档案 " + profiles.Count + " 个。");
		if (profiles.Count > 0)
		{
			return profiles[0];
		}
		return new OffsetProfile();
	}

	private static string Short(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return "未知";
		}
		return value.Length <= 12 ? value : value.Substring(0, 12);
	}

	private static void Log(string message)
	{
		System.Diagnostics.Debug.WriteLine(message);
	}

	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		attachTimer.Stop();
		StopEsp60();
		hotkey.Dispose();
		base.OnFormClosing(e);
	}
}
