using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class ControlBarForm : Form
{
	private readonly AlertTracker alerts = new AlertTracker();

	private readonly ArmourProductionTracker armourAlerts = new ArmourProductionTracker();

	private readonly EnemySelection selection = new EnemySelection();

	private readonly EnemySelection armourSelection = new EnemySelection();

	private readonly OverlayPreferences preferences = OverlayPreferences.Load();

	private readonly EnemyOverlayWindow overlay;

	private readonly MineOverlayWindow mineOverlay;

	private readonly SquadMarkerHost squadOverlay;

	private readonly MinimapMarkerWindow miniOverlay;

	private readonly AirOverlayHost airOverlay;

	private readonly System.Windows.Forms.Timer polling = new System.Windows.Forms.Timer();

	private readonly System.Windows.Forms.Timer supportTimer49 = new System.Windows.Forms.Timer();

	private readonly HashSet<string> supportSeen49 = new HashSet<string>();

	private string supportMatch49;

	private bool supportBaseline49;

	private PlayerInfoReader reader;

	private int pid;

	private DateTime retryAt = DateTime.MinValue;

	private List<PlayerInfoRow> rows = new List<PlayerInfoRow>();

	private bool liveRows;

	private readonly ReadRecovery55 recovery55 = new ReadRecovery55();

	private readonly List<KeyValuePair<Toggle46, Func<bool>>> bindings46 = new List<KeyValuePair<Toggle46, Func<bool>>>();

	private bool sync46;

	private Label connection46;

	private readonly Label status = new Label();

	private readonly ToolTip tips46 = new ToolTip();

	private string licenseExpiry60 = "";

	public event Action ChangeLicense60;

	public ControlBarForm()
	{
		Text = "COH3工具";
		base.ClientSize = new Size(824, 118);
		base.StartPosition = FormStartPosition.CenterScreen;
		base.FormBorderStyle = FormBorderStyle.None;
		base.MaximizeBox = false;
		TopMost = true;
		ShowInTaskbar = true;
		Font = Interface45.Font();
		base.Icon = Interface45.Icon;
		BackColor = Palette46.Back;
		ForeColor = Palette46.Ink;
		overlay = new EnemyOverlayWindow(preferences, track: true);
		mineOverlay = new MineOverlayWindow(preferences, track: true);
		squadOverlay = new SquadMarkerHost(preferences.SquadMarkers, track: true);
		miniOverlay = new MinimapMarkerWindow(preferences.Minimap, track: true, preferences.Tactical51);
		airOverlay = new AirOverlayHost(preferences.SquadMarkers, track: true, preferences.Tactical51);
		miniOverlay.Range55 = delegate
		{
			return null;
		};
		BuildInterface45();
		polling.Interval = 1000;
		polling.Tick += delegate
		{
			Poll();
		};
		supportTimer49.Interval = 150;
		supportTimer49.Tick += delegate
		{
			PollSupport49();
		};
		base.Shown += delegate
		{
			StartTracking();
		};
	}

	[DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
	private static extern bool ReleaseCapture60();

	[DllImport("user32.dll", EntryPoint = "SendMessageW")]
	private static extern IntPtr SendMessage60(IntPtr h, int m, IntPtr w, IntPtr l);

	private void BuildInterface45()
	{
		base.AutoScaleMode = AutoScaleMode.None;
		Label label = new Label
		{
			Text = "COH3工具",
			Font = Interface45.Font(11f, FontStyle.Bold),
			ForeColor = Palette46.Ink,
			Bounds = new Rectangle(14, 10, 170, 26)
		};
		base.Controls.Add(label);
		connection46 = new Label
		{
			Text = "○ 等待对局",
			Font = Interface45.Font(9f),
			ForeColor = Palette46.Muted,
			TextAlign = ContentAlignment.MiddleRight,
			Bounds = new Rectangle(388, 12, 180, 22)
		};
		base.Controls.Add(connection46);
		Button46(576, "退出", delegate
		{
			this.ChangeLicense60?.Invoke();
		}, 70);
		Button46(650, "—", delegate
		{
			base.WindowState = FormWindowState.Minimized;
		}, 34);
		Button46(686, "×", delegate
		{
			Close();
		}, 34);
		Toggle46 toggle = Toggle46(14, "顶部玩家信息", () => preferences.Enabled, delegate(bool v)
		{
			preferences.Enabled = v;
		});
		Toggle46 toggle2 = Toggle46(228, "主画面标记", () => preferences.SquadMarkers.Enabled, delegate(bool v)
		{
			preferences.SquadMarkers.Enabled = v;
		});
		Toggle46 toggle3 = Toggle46(442, "小地图标记", () => preferences.Minimap.Enabled, delegate(bool v)
		{
			preferences.Minimap.Enabled = v;
		});
		Label label3 = new Label
		{
			Text = "参数自动保存",
			Font = Interface45.Font(8f),
			ForeColor = Palette46.Muted,
			Bounds = new Rectangle(664, 60, 130, 20)
		};
		base.Controls.Add(label3);
		status.Font = Interface45.Font(8f);
		status.ForeColor = Palette46.Muted;
		status.Bounds = new Rectangle(16, 88, 792, 20);
		status.AutoEllipsis = true;
		base.Controls.Add(status);
		MouseEventHandler value = delegate(object s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture60();
				SendMessage60(base.Handle, 161, new IntPtr(2), IntPtr.Zero);
			}
		};
		base.MouseDown += value;
		label.MouseDown += value;
		connection46.MouseDown += value;
		Paint += delegate(object s, PaintEventArgs e)
		{
			using Pen pen = new Pen(Palette46.Line);
			e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
			e.Graphics.DrawLine(pen, 0, 44, base.Width - 1, 44);
		};
		SyncConsole46();
	}

	private Toggle46 Toggle46(int x, string text, Func<bool> get, Action<bool> set)
	{
		Toggle46 toggle = new Toggle46();
		toggle.Text = text;
		toggle.Checked = get();
		toggle.SetBounds(x, 52, 205, 30);
		toggle.CheckedChanged += delegate
		{
			if (!sync46)
			{
				set(toggle.Checked);
				Changed();
			}
		};
		base.Controls.Add(toggle);
		bindings46.Add(new KeyValuePair<Toggle46, Func<bool>>(toggle, get));
		return toggle;
	}

	private void Button46(int x, string text, EventHandler onClick, int width)
	{
		Button button = new Button();
		button.Text = text;
		button.FlatStyle = FlatStyle.Flat;
		button.FlatAppearance.BorderColor = Palette46.Line;
		button.FlatAppearance.MouseOverBackColor = Palette46.Raised;
		button.BackColor = Palette46.Panel;
		button.ForeColor = Palette46.Ink;
		button.Font = Interface45.Font(9f);
		button.SetBounds(x, 8, width, 28);
		button.Cursor = Cursors.Hand;
		button.Click += onClick;
		base.Controls.Add(button);
	}

	public void SetLicenseExpiry60(long unix, string kind = "")
	{
		licenseExpiry60 = FormatLicenseExpiry60(unix, kind);
		UpdateConnection46();
	}

	internal static string FormatLicenseExpiry60(long unix, string kind)
	{
		if (kind == "permanent")
		{
			return "授权期限：永久";
		}
		try
		{
			return "授权到期：" + new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unix).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
		}
		catch (ArgumentOutOfRangeException)
		{
			return "";
		}
	}

	public void ResumeProtected60()
	{
		if (!LicenseGate60.Allowed)
		{
			return;
		}
		supportTimer49.Start();
		if (!polling.Enabled)
		{
			StartTracking();
		}
	}

	public void StopProtected60()
	{
		polling.Stop();
		supportTimer49.Stop();
		DropReader();
	}

	public void StartTracking()
	{
		if (LicenseGate60.Allowed)
		{
			Poll();
			polling.Start();
		}
	}

	private IEnumerable<PlayerInfoRow> DisplayRows()
	{
		return rows.Where((PlayerInfoRow x) => selection.Includes(x) || (preferences.ShowAllPlayers && (x.Relation == "自己" || x.Relation == "队友")));
	}

	private void Changed()
	{
		if (liveRows && LicenseGate60.Allowed)
		{
			overlay.UpdateRows(pid, DisplayRows());
		}
		overlay.ApplyAppearance();
		squadOverlay.ApplyAppearance();
		miniOverlay.ApplyAppearance();
		airOverlay.ApplyAppearance();
		try
		{
			preferences.Save();
		}
		catch (Exception ex)
		{
			status.Text = "设置未保存：" + ex.Message;
		}
	}

	private void DropReader()
	{
		airOverlay.ClearData();
		miniOverlay.ClearData();
		squadOverlay.ClearData();
		mineOverlay.ClearData();
		if (reader != null)
		{
			reader.Dispose();
		}
		reader = null;
		pid = 0;
		alerts.Reset();
		armourAlerts.Reconnect();
		overlay.ClearData();
		rows.Clear();
		liveRows = false;
		SyncConsole46();
	}

	private void Poll()
	{
		if (!LicenseGate60.Allowed)
		{
			StopProtected60();
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			if (reader == null)
			{
				if (DateTime.UtcNow < retryAt)
				{
					return;
				}
				retryAt = DateTime.UtcNow.AddSeconds(1.0);
				Process[] processesByName = Process.GetProcessesByName("RelicCoH3");
				try
				{
					if (processesByName.Length != 1)
					{
						throw new Exception("等待启动游戏（自动重试）");
					}
					pid = processesByName[0].Id;
				}
				finally
				{
					Process[] array = processesByName;
					foreach (Process process in array)
					{
						process.Dispose();
					}
				}
				reader = new PlayerInfoReader();
			}
			rows = reader.Snapshot();
			recovery55.Success();
			polling.Interval = 1000;
			Layer55("地雷标记", delegate
			{
				mineOverlay.UpdateRows(reader, pid, rows);
			});
			Layer55("战场标记", delegate
			{
				squadOverlay.UpdateRows(reader, pid, rows);
			});
			Layer55("地图标记", delegate
			{
				miniOverlay.UpdateRows(reader, pid, rows);
			});
			Layer55("空袭标记", delegate
			{
				airOverlay.UpdateRows(reader, pid, rows);
			});
			selection.UpdateMatch(reader.MatchKey);
			armourSelection.UpdateMatch(reader.MatchKey);
			liveRows = true;
			overlay.UpdateRows(pid, DisplayRows());
			foreach (GameAlert item in alerts.Update(reader.MatchKey, rows, selection))
			{
				status.Text = item.Text;
			}
			foreach (GameAlert item2 in armourAlerts.Update(reader.MatchKey, rows, armourSelection))
			{
				status.Text = item2.Text;
			}
			if (preferences.UseGameColours && reader.ColourError.Length > 0)
			{
				status.Text = "玩家颜色暂未读到，使用手动配色：" + reader.ColourError;
			}
			SyncConsole46();
		}
		catch (Exception ex2)
		{
			bool sameMatch = reader != null && liveRows && reader.IsCurrentMatch55();
			if (recovery55.Retry(sameMatch, DateTime.UtcNow))
			{
				polling.Interval = 100;
				OverlayDiagnostics55.Record("快速重试", ex2.Message);
				status.Text = "数据更新中，正在快速重试：" + ex2.Message;
			}
			else
			{
				OverlayDiagnostics55.Record("重新连接", ex2.Message);
				DropReader();
				polling.Interval = 1000;
				retryAt = DateTime.UtcNow.AddSeconds(1.0);
				status.Text = ex2.Message + "；正在重新连接。";
			}
		}
		finally
		{
			if (stopwatch.ElapsedMilliseconds > 1000)
			{
				OverlayDiagnostics55.Record("主读取耗时", stopwatch.ElapsedMilliseconds + "ms");
			}
		}
	}

	private void PollSupport49()
	{
		if (!LicenseGate60.Allowed || reader == null || !liveRows)
		{
			return;
		}
		try
		{
			if (supportMatch49 != reader.MatchKey)
			{
				supportMatch49 = reader.MatchKey;
				supportSeen49.Clear();
				supportBaseline49 = false;
			}
			List<SupportCommand49> list = reader.ReadSupport49(rows);
			foreach (SupportCommand49 item in list)
			{
				if (supportSeen49.Add(item.Key) && item.Flare && supportBaseline49)
				{
					status.Text = "敌方" + (item.Slot + 1) + "使用了SLD星爆照明弹";
				}
			}
			supportBaseline49 = true;
		}
		catch
		{
		}
	}

	private void Layer55(string name, Action action)
	{
		try
		{
			action();
		}
		catch (Exception ex)
		{
			OverlayDiagnostics55.Record("图层读取失败", name + " " + ex.Message);
			status.Text = name + "：" + ex.Message;
		}
	}

	private void SyncConsole46()
	{
		if (sync46)
		{
			return;
		}
		sync46 = true;
		try
		{
			foreach (KeyValuePair<Toggle46, Func<bool>> item in bindings46)
			{
				if (!item.Key.IsDisposed)
				{
					item.Key.Checked = item.Value();
				}
			}
			UpdateConnection46();
		}
		finally
		{
			sync46 = false;
		}
	}

	private void UpdateConnection46()
	{
		if (connection46 == null)
		{
			return;
		}
		(string label, string detail) = GameVersion45.Status switch
		{
			GameVersion45.CheckStatus.FingerprintMismatch => ("⚠ 待适配", "游戏主程序指纹与已验证基线不一致，结构依赖功能已暂停。"),
			GameVersion45.CheckStatus.ReadFailed => ("⚠ 版本读取失败", "读取游戏主程序失败，未能验证版本。"),
			_ => (liveRows ? "● 对局已连接" : "○ 等待对局", "")
		};
		connection46.Text = label;
		connection46.ForeColor = (liveRows ? Palette46.Ink : Palette46.Muted);
		tips46.SetToolTip(connection46, detail.Length != 0 && licenseExpiry60.Length != 0
			? detail + "\n" + licenseExpiry60
			: detail + licenseExpiry60);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			supportTimer49.Stop();
			supportTimer49.Dispose();
			polling.Dispose();
			tips46.Dispose();
		}
		base.Dispose(disposing);
	}
}
