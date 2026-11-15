using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class OverlaySettingsForm : Form
{
	private struct Rectangle47
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private readonly ArmourProductionTracker armourAlerts = new ArmourProductionTracker();

	private readonly EnemySelection armourSelection = new EnemySelection();

	private readonly AlertTracker alerts = new AlertTracker();

	private readonly AlertSounds sounds = new AlertSounds();

	private readonly EnemySelection selection = new EnemySelection();

	private readonly CheckedListBox enemies = new CheckedListBox();

	private bool updatingEnemies;

	private readonly OverlayPreferences preferences = OverlayPreferences.Load();

	private readonly EnemyOverlayWindow overlay;

	private readonly MineOverlayWindow mineOverlay;

	private readonly SquadMarkerHost squadOverlay;

	private readonly MinimapMarkerWindow miniOverlay;

	private readonly AirOverlayHost airOverlay;

	private readonly System.Windows.Forms.Timer polling = new System.Windows.Forms.Timer();

	private readonly System.Windows.Forms.Timer previewAnimation = new System.Windows.Forms.Timer();

	private readonly Label status = new Label();

	private readonly Panel preview = new Preview45();

	private readonly NotifyIcon tray = new NotifyIcon();

	private PlayerInfoReader reader;

	private int pid;

	private DateTime retryAt = DateTime.MinValue;

	private List<PlayerInfoRow> rows = new List<PlayerInfoRow>();

	private bool liveRows;

	private readonly ComboBox copyKey = new ComboBox();

	private readonly ComboBox copyMods = new ComboBox();

	private readonly bool previewOnlyMode;

	private uint registeredCopyKey;

	private uint registeredCopyMods;

	private bool copyRegistered;

	private readonly System.Windows.Forms.Timer flankTimer47 = new System.Windows.Forms.Timer();

	private FlankOverlay47 flankWindow47;

	private bool flankEnabled47;

	private bool flankActive47;

	private bool flankWasDown47;

	private bool flankRegistered47;

	private bool flankRange52 = true;

	private decimal flankRangeWidth52 = 2m;

	private int flankRadius47 = 40;

	private int flankRepeat47 = 15;

	private string flankKey47 = "F9";

	private string flankSound47 = "";

	private string flankMatch47;

	private DateTime flankArmed47;

	private DateTime flankFresh47;

	private DateTime flankNextSound47;

	private float flankX47;

	private float flankY47;

	private float flankZ47;

	private long flankSource47;

	private long flankIdentity47;

	private List<SquadMarker> flankUnits47 = new List<SquadMarker>();

	private FlankResult47 flankResult47;

	private Label flankStatus47;

	private uint flankMods47;

	private uint flankCode47;

	private string licenseExpiry60 = "";

	private readonly ReadRecovery55 recovery55 = new ReadRecovery55();

	private readonly List<KeyValuePair<Toggle46, Func<bool>>> bindings46 = new List<KeyValuePair<Toggle46, Func<bool>>>();

	private readonly Stack46[] columns46 = new Stack46[3];

	private readonly CheckedListBox armourList46 = new CheckedListBox();

	private bool sync46;

	private bool armourSync46;

	private Label connection46;

	private ToolTip tips46 = new ToolTip();

	private readonly System.Windows.Forms.Timer supportTimer49 = new System.Windows.Forms.Timer();

	private readonly Dictionary<string, SupportCommand49> supportActive49 = new Dictionary<string, SupportCommand49>();

	private readonly HashSet<string> supportSeen49 = new HashSet<string>();

	private string supportMatch49;

	private bool supportBaseline49;

	public Action HostShow { get; set; }

	public Action HostHide { get; set; }

	public Action HostExit { get; set; }

	private string CopyKeyPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "copy-hotkey.txt");

	private string FlankPath47 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "flank-settings.txt");

	public event Action ChangeLicense60;

	public OverlaySettingsForm(bool previewOnly = false)
	{
		previewOnlyMode = previewOnly;
		Text = "天穹之眼 · 英雄连3  /  v0.60.8";
		base.ClientSize = new Size(Math.Min(1480, Screen.PrimaryScreen.WorkingArea.Width - 30), Math.Min(950, Screen.PrimaryScreen.WorkingArea.Height - 30));
		MinimumSize = new Size(1180, 740);
		Font = new Font("Microsoft YaHei UI", 10f);
		base.StartPosition = FormStartPosition.CenterScreen;
		overlay = new EnemyOverlayWindow(preferences, !previewOnly);
		mineOverlay = new MineOverlayWindow(preferences, !previewOnly);
		squadOverlay = new SquadMarkerHost(preferences.SquadMarkers, !previewOnly);
		miniOverlay = new MinimapMarkerWindow(preferences.Minimap, !previewOnly, preferences.Tactical51);
		airOverlay = new AirOverlayHost(preferences.SquadMarkers, !previewOnly, preferences.Tactical51);
		BuildInterface45();
		miniOverlay.Range55 = MapRange55;
		tray.Icon = Interface45.Icon;
		tray.Text = "天穹之眼 · 英雄连3";
		tray.Visible = !previewOnly;
		NotifyIcon notifyIcon = tray;
		EventHandler value = delegate
		{
			ShowSettings();
		};
		notifyIcon.DoubleClick += value;
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip
		{
			Items = 
			{
				{
					"打开设置",
					(Image)null,
					(EventHandler)delegate
					{
						ShowSettings();
					}
				},
				{
					"退出程序",
					(Image)null,
					(EventHandler)delegate
					{
						if (HostExit != null)
						{
							HostExit();
						}
						else
						{
							Close();
						}
					}
				}
			}
		};
		tray.ContextMenuStrip = contextMenuStrip;
		base.Resize += delegate
		{
			if (base.WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		};
		previewAnimation.Interval = 50;
		previewAnimation.Tick += delegate
		{
			if (base.Visible)
			{
				preview.Invalidate();
			}
		};
		previewAnimation.Start();
		polling.Interval = 1000;
		polling.Tick += delegate
		{
			Poll();
		};
		if (!previewOnly)
		{
			base.Shown += delegate
			{
				StartTracking();
			};
		}
		else
		{
			status.Text = "外观预览 · 下方是示例数据";
		}
		if (previewOnly)
		{
			PlayerInfoRow playerInfoRow = new PlayerInfoRow
			{
				Slot = 1u,
				Name = "敌方玩家甲",
				Relation = "敌方",
				Manpower = 450f,
				Munition = 120f,
				Fuel = 80f,
				Population = 52f,
				Race = "german",
				BuildingsKnown = true
			};
			playerInfoRow.Buildings.Add(new TrackedBuilding
			{
				Tier = 1,
				Key = "preview1",
				Complete = true
			});
			playerInfoRow.Buildings.Add(new TrackedBuilding
			{
				Tier = 2,
				Key = "preview2",
				Complete = false
			});
			playerInfoRow.Upgrades.AddRange(new string[3] { "upgrade\\german\\battlegroups\\luftwaffe\\luftwaffe", "upgrade\\german\\battlegroups\\luftwaffe\\luftwaffe_left_1b_fallschirmpioneers", "upgrade\\german\\battlegroups\\luftwaffe\\luftwaffe_right_1b_strafing_run_ger" });
			PlayerInfoRow playerInfoRow2 = new PlayerInfoRow
			{
				Slot = 2u,
				Name = "敌方玩家乙",
				Relation = "敌方",
				Manpower = 360f,
				Munition = 90f,
				Fuel = 50f
			};
			playerInfoRow2.Upgrades.Add("upgrade\\american\\battlegroups\\armored\\armored");
			rows = new List<PlayerInfoRow> { playerInfoRow, playerInfoRow2 };
			liveRows = true;
			selection.UpdateMatch("preview");
			selection.Set(playerInfoRow2, enabled: false);
			SyncEnemies();
		}
	}

	public void StartTracking()
	{
		if (!previewOnlyMode && LicenseGate60.Allowed)
		{
			Poll();
			polling.Start();
			if (!copyRegistered)
			{
				ApplyCopyKey();
			}
		}
	}

	private void LoadCopyKey()
	{
		try
		{
			string[] array = File.ReadAllLines(CopyKeyPath);
			if (array.Length == 2 && int.TryParse(array[0], out var result) && result >= 0 && result < 5 && copyKey.Items.Contains(array[1]))
			{
				copyMods.SelectedIndex = result;
				copyKey.SelectedItem = array[1];
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	private void ApplyCopyKey()
	{
		if (previewOnlyMode || !LicenseGate60.Allowed)
		{
			return;
		}
		uint key = (uint)(Keys)Enum.Parse(typeof(Keys), copyKey.Text);
		uint modifiers = (new uint[5] { 0u, 2u, 1u, 4u, 3u })[copyMods.SelectedIndex] | 0x4000;
		if (copyRegistered)
		{
			Native.UnregisterHotKey(base.Handle, 201);
		}
		if (!Native.RegisterHotKey(base.Handle, 201, modifiers, key))
		{
			copyRegistered = registeredCopyKey != 0 && Native.RegisterHotKey(base.Handle, 201, registeredCopyMods, registeredCopyKey);
			MessageBox.Show(this, "该快捷键已被占用，请选择其他组合。之前有效的复制快捷键会继续保留。", "复制快捷键");
			return;
		}
		copyRegistered = true;
		registeredCopyKey = key;
		registeredCopyMods = modifiers;
		try
		{
			File.WriteAllLines(CopyKeyPath, new string[2]
			{
				copyMods.SelectedIndex.ToString(),
				copyKey.Text
			});
			status.Text = "复制快捷键已保存：" + ((copyMods.SelectedIndex == 0) ? "" : (copyMods.Text + "+")) + copyKey.Text;
		}
		catch (Exception ex)
		{
			status.Text = "本次快捷键已生效，但未能保存：" + ex.Message;
		}
	}

	private void CopyEnemyInfo()
	{
		if (previewOnlyMode || !LicenseGate60.Allowed)
		{
			return;
		}
		Poll();
		if (!liveRows)
		{
			status.Text = "没有当前对局数据，未更改剪贴板。";
			return;
		}
		string text = string.Join(Environment.NewLine, OverlayText.Lines(selection.Filter(rows), preferences));
		if (text.Length == 0)
		{
			status.Text = "没有勾选敌方玩家，未更改剪贴板。";
			return;
		}
		try
		{
			Clipboard.SetText(text, TextDataFormat.UnicodeText);
			status.Text = "已复制 " + selection.Filter(rows).Count() + " 名敌方的信息，可手动粘贴。";
		}
		catch (ExternalException)
		{
			status.Text = "剪贴板正被占用，请再按一次复制快捷键。";
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (HitTest46(ref m) || FlankMessage47(ref m))
		{
			return;
		}
		if (m.Msg == 786 && m.WParam.ToInt32() == 201)
		{
			Native.GetWindowThreadProcessId(Native.GetForegroundWindow(), out var num);
			if (num == (uint)pid || num == (uint)Process.GetCurrentProcess().Id)
			{
				CopyEnemyInfo();
			}
		}
		base.WndProc(ref m);
	}

	private void ShowSettings()
	{
		if (HostShow != null)
		{
			HostShow();
			return;
		}
		Show();
		base.WindowState = FormWindowState.Normal;
		Activate();
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
		preview.Invalidate();
		SyncConsole46();
		try
		{
			if (!previewOnlyMode)
			{
				preferences.Save();
			}
		}
		catch (Exception ex)
		{
			status.Text = "设置未保存：" + ex.Message;
		}
	}

	private void DropReader()
	{
		ResetFlank47();
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
		SyncArmour46();
		liveRows = false;
		UpdateConnection45();
		updatingEnemies = true;
		enemies.Items.Clear();
		updatingEnemies = false;
		preview.Invalidate();
	}

	private void SyncEnemies()
	{
		EnemyChoice[] array = (from x in rows
			where x.Relation == "敌方"
			orderby x.Slot
			select new EnemyChoice
			{
				Player = x
			}).ToArray();
		updatingEnemies = true;
		try
		{
			bool flag = enemies.Items.Count == array.Length;
			int num = 0;
			while (flag && num < array.Length)
			{
				flag = ((EnemyChoice)enemies.Items[num]).ToString() == array[num].ToString();
				num++;
			}
			if (!flag)
			{
				enemies.Items.Clear();
				EnemyChoice[] array2 = array;
				foreach (EnemyChoice enemyChoice in array2)
				{
					enemies.Items.Add(enemyChoice, selection.Includes(enemyChoice.Player));
				}
			}
			else
			{
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					((EnemyChoice)enemies.Items[num3]).Player = array[num3].Player;
					enemies.SetItemChecked(num3, selection.Includes(array[num3].Player));
				}
			}
		}
		finally
		{
			updatingEnemies = false;
		}
		SyncArmour46();
	}

	private void RefreshSelection()
	{
		if (liveRows && LicenseGate60.Allowed)
		{
			overlay.UpdateRows(pid, DisplayRows());
			status.Text = "已勾选 " + selection.Filter(rows).Count() + " / " + rows.Count((PlayerInfoRow x) => x.Relation == "敌方") + " 名敌方 · 每秒刷新 · 本局有效";
		}
		preview.Invalidate();
	}

	private void SetAll(bool enabled)
	{
		foreach (PlayerInfoRow item in rows.Where((PlayerInfoRow x) => x.Relation == "敌方"))
		{
			selection.Set(item, enabled);
		}
		SyncEnemies();
		RefreshSelection();
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
			try
			{
				UpdateFlank47();
			}
			catch (Exception ex)
			{
				if (flankWindow47 != null)
				{
					flankWindow47.Hide();
				}
				if (flankStatus47 != null)
				{
					flankStatus47.Text = ex.Message;
				}
			}
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
			UpdateConnection45();
			SyncEnemies();
			RefreshSelection();
			foreach (GameAlert item in alerts.Update(reader.MatchKey, rows, selection))
			{
				sounds.Play(item.Kind);
				status.Text = item.Text;
			}
			foreach (GameAlert item2 in armourAlerts.Update(reader.MatchKey, rows, armourSelection))
			{
				sounds.Play(item2.Kind);
				status.Text = item2.Text;
			}
			if (sounds.LastError.Length > 0)
			{
				status.Text = sounds.LastError;
			}
			if (mineOverlay.LastError.Length > 0)
			{
				status.Text = "地雷标记：" + mineOverlay.LastError;
			}
			if (squadOverlay.LastError.Length > 0)
			{
				status.Text = "班组标记：" + squadOverlay.LastError;
			}
			if (preferences.Minimap.Enabled && miniOverlay.LastError.Length > 0)
			{
				status.Text = "小地图：" + miniOverlay.LastError;
			}
			if (airOverlay.LastError.Length > 0)
			{
				status.Text = "飞机技能：" + airOverlay.LastError;
			}
			if (preferences.UseGameColours && reader.ColourError.Length > 0)
			{
				status.Text = "玩家颜色暂未读到，使用手动配色：" + reader.ColourError;
			}
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
			SyncConsole46();
			if (stopwatch.ElapsedMilliseconds > 1000)
			{
				OverlayDiagnostics55.Record("主读取耗时", stopwatch.ElapsedMilliseconds + "ms");
			}
		}
	}

	private void PaintPreview(object sender, PaintEventArgs e)
	{
		string[] array = ((liveRows && LicenseGate60.Allowed) ? OverlayText.Lines(DisplayRows(), preferences) : new string[2] { "预览示例  人 450  弹 120  油 80  |  空军战斗群", "已解锁  左一乙：空降工兵 · 右一乙：扫射" });
		if (liveRows && array.Length == 0)
		{
			array = new string[1] { "未勾选敌方或尚无敌方数据，悬浮层已隐藏" };
		}
		using Font font = new Font("Microsoft YaHei UI", preferences.FontSize, FontStyle.Bold);
		OverlayDrawing.Draw(e.Graphics, array, font, preferences.Color, preferences.SkillColor, preview.Width, paint: true, preferences.DetailSize, liveRows ? OverlayText.Colours(DisplayRows(), preferences) : null, liveRows ? OverlayText.NumberSpans(DisplayRows(), preferences) : null);
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		if (copyRegistered)
		{
			Native.UnregisterHotKey(base.Handle, 201);
		}
		previewAnimation.Dispose();
		polling.Stop();
		polling.Dispose();
		tray.Visible = false;
		tray.Dispose();
		DropReader();
		sounds.Dispose();
		overlay.Close();
		overlay.Dispose();
		mineOverlay.Close();
		mineOverlay.Dispose();
		squadOverlay.Close();
		squadOverlay.Dispose();
		miniOverlay.Close();
		miniOverlay.Dispose();
		airOverlay.Dispose();
		base.OnFormClosed(e);
	}

	[DllImport("user32.dll")]
	private static extern short GetAsyncKeyState(int key);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr h, out Rectangle47 r);

	[DllImport("user32.dll")]
	private static extern bool ScreenToClient(IntPtr h, ref Point p);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr h, ref Point p);

	private void BuildFlank47()
	{
		try
		{
			string[] array = File.ReadAllLines(FlankPath47);
			foreach (string text in array)
			{
				int num = text.IndexOf('=');
				if (num >= 0)
				{
					string text2 = text.Substring(0, num);
					string text3 = text.Substring(num + 1);
					if (text2 == "range")
					{
						flankRange52 = text3 == "1";
					}
					if (text2 == "rangeWidth" && decimal.TryParse(text3, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
					{
						flankRangeWidth52 = Math.Max(0.5m, Math.Min(12m, result));
					}
					if (text2 == "enabled")
					{
						flankEnabled47 = text3 == "1";
					}
					if (text2 == "radius" && int.TryParse(text3, out var result2))
					{
						flankRadius47 = Math.Max(5, Math.Min(500, result2));
					}
					if (text2 == "repeat" && int.TryParse(text3, out result2))
					{
						flankRepeat47 = Math.Max(3, Math.Min(120, result2));
					}
					if (text2 == "key")
					{
						flankKey47 = text3;
					}
					if (text2 == "sound")
					{
						flankSound47 = text3;
					}
				}
			}
		}
		catch (IOException)
		{
		}
		Module46 module = Module46(2, "07", "包抄提醒");
		Toggle46(module.Body, "启用监测", () => flankEnabled47, delegate(bool v)
		{
			flankEnabled47 = v;
			if (!v)
			{
				ResetFlank47();
			}
			SaveFlank47();
		});
		Number46(module.Body, "半径(射程)", flankRadius47, 5m, 500m, delegate(decimal v)
		{
			flankRadius47 = (int)v;
			SaveFlank47();
		});
		Number46(module.Body, "重复间隔 秒", flankRepeat47, 3m, 120m, delegate(decimal v)
		{
			flankRepeat47 = (int)v;
			SaveFlank47();
		});
		Toggle46(module.Body, "显示检测圆环", () => flankRange52, delegate(bool v)
		{
			flankRange52 = v;
			SaveFlank47();
		});
		Number46(module.Body, "圆环粗细", flankRangeWidth52, 0.5m, 12m, delegate(decimal v)
		{
			flankRangeWidth52 = v;
			SaveFlank47();
		}, 1);
		TextBox key = new TextBox
		{
			Text = flankKey47,
			Width = 150,
			BackColor = Palette46.Back,
			ForeColor = Palette46.Ink,
			BorderStyle = BorderStyle.FixedSingle
		};
		module.Body.Controls.Add(key);
		Button46(module.Body, "保存标记热键", delegate
		{
			RegisterFlank47(key.Text);
			SaveFlank47();
		}, 150);
		Button46(module.Body, "选择音源", delegate
		{
			using OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "MP3 / WAV|*.mp3;*.wav"
			};
			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
			{
				using (new AudioClip(openFileDialog.FileName))
				{
				}
				flankSound47 = openFileDialog.FileName;
				SaveFlank47();
				flankStatus47.Text = Path.GetFileName(openFileDialog.FileName);
			}
		}, 145);
		Button46(module.Body, "试听", PlayFlank47, 65);
		Button46(module.Body, "重置音源", delegate
		{
			flankSound47 = "";
			SaveFlank47();
			flankStatus47.Text = "默认音源：" + Path.GetFileName(DefaultSounds61.FlankFile);
		}, 100);
		Button46(module.Body, "清除监测点", ResetFlank47, 145);
		flankStatus47 = Label46("按热键后 5 秒内左键点击单位或地面。", 390, Palette46.Muted);
		Full46(module.Body, flankStatus47, 48);
		tips46.SetToolTip(key, "如 F9、Ctrl+F9、Alt+G；与复制信息热键区分。");
		flankWindow47 = new FlankOverlay47();
		flankTimer47.Interval = 50;
		flankTimer47.Tick += delegate
		{
			TickFlank47();
		};
		if (previewOnlyMode)
		{
			return;
		}
		base.Shown += delegate
		{
			if (LicenseGate60.Allowed)
			{
				try
				{
					RegisterFlank47(flankKey47);
				}
				catch (Exception ex2)
				{
					flankStatus47.Text = ex2.Message;
				}
				flankTimer47.Start();
			}
		};
	}

	private void SaveFlank47()
	{
		if (previewOnlyMode)
		{
			return;
		}
		try
		{
			File.WriteAllLines(FlankPath47, new string[7]
			{
				"range=" + (flankRange52 ? 1 : 0),
				"rangeWidth=" + flankRangeWidth52.ToString(CultureInfo.InvariantCulture),
				"enabled=" + (flankEnabled47 ? 1 : 0),
				"radius=" + flankRadius47,
				"repeat=" + flankRepeat47,
				"key=" + flankKey47,
				"sound=" + flankSound47
			});
		}
		catch (Exception ex)
		{
			status.Text = "包抄设置未保存：" + ex.Message;
		}
	}

	private void RegisterFlank47(string text)
	{
		string[] array = text.Split('+');
		uint num = 16384u;
		foreach (string item in array.Take(array.Length - 1))
		{
			if (item.Trim().Equals("ctrl", StringComparison.OrdinalIgnoreCase))
			{
				num |= 2;
				continue;
			}
			if (item.Trim().Equals("alt", StringComparison.OrdinalIgnoreCase))
			{
				num |= 1;
				continue;
			}
			if (item.Trim().Equals("shift", StringComparison.OrdinalIgnoreCase))
			{
				num |= 4;
				continue;
			}
			throw new Exception("热键格式如 Ctrl+F9。");
		}
		if (!Enum.TryParse<Keys>(array.Last().Trim(), ignoreCase: true, out var result) || result == Keys.None || result > (Keys.F16 | Keys.F17))
		{
			throw new Exception("无效标记热键。");
		}
		if (previewOnlyMode || !LicenseGate60.Allowed)
		{
			return;
		}
		uint modifiers = flankMods47;
		uint num2 = flankCode47;
		Native.UnregisterHotKey(base.Handle, 203);
		if (!Native.RegisterHotKey(base.Handle, 203, num, (uint)result))
		{
			if (num2 != 0)
			{
				Native.RegisterHotKey(base.Handle, 203, modifiers, num2);
			}
			throw new Exception("标记热键被占用，原热键保留。");
		}
		flankRegistered47 = true;
		flankMods47 = num;
		flankCode47 = (uint)result;
		flankKey47 = text;
	}

	private bool FlankMessage47(ref Message m)
	{
		if (m.Msg != 786 || m.WParam.ToInt32() != 203)
		{
			return false;
		}
		Native.GetWindowThreadProcessId(Native.GetForegroundWindow(), out var num);
		if (LicenseGate60.Allowed && num == (uint)pid && flankEnabled47)
		{
			flankArmed47 = DateTime.UtcNow.AddSeconds(5.0);
			flankWasDown47 = (GetAsyncKeyState(1) & 0x8000) != 0;
			flankStatus47.Text = "等待左键点击：优先捕捉单位，Esc 取消。";
		}
		return true;
	}

	private void UpdateFlank47()
	{
		if (flankWindow47 == null || !flankEnabled47)
		{
			return;
		}
		if (flankMatch47 != reader.MatchKey)
		{
			ResetFlank47();
			flankMatch47 = reader.MatchKey;
		}
		foreach (PlayerInfoRow row in rows)
		{
			foreach (SquadMarkerSource markerSquad in row.MarkerSquads)
			{
				markerSquad.ColourKnown = true;
				markerSquad.Colour = (row.GameColourKnown ? row.GameColour : Color.White);
			}
		}
		reader.FlankReadMovement47 = true;
		try
		{
			flankUnits47 = reader.ReadSquadMarkers(reader.MatchKey, rows.Where((PlayerInfoRow p) => p.MarkerDataKnown).SelectMany((PlayerInfoRow p) => p.MarkerSquads));
		}
		finally
		{
			reader.FlankReadMovement47 = false;
		}
		flankFresh47 = DateTime.UtcNow;
		if (!flankActive47)
		{
			return;
		}
		if (rows.Any((PlayerInfoRow p) => !p.MarkerDataKnown))
		{
			flankStatus47.Text = "单位数据暂不完整，本次不判定。";
			return;
		}
		if (flankSource47 != 0)
		{
			SquadMarker squadMarker = flankUnits47.FirstOrDefault((SquadMarker u) => u.Source == flankSource47 && u.Identity == flankIdentity47);
			if (squadMarker == null)
			{
				ResetFlank47();
				flankStatus47.Text = "监测单位已消失，已清除。";
				return;
			}
			flankX47 = squadMarker.X;
			flankY47 = squadMarker.Y;
			flankZ47 = squadMarker.Z;
		}
		flankResult47 = FlankLogic47.Count(flankUnits47, rows, flankX47, flankZ47, flankRadius47);
		FlankResult47 flankResult = flankResult47;
		flankStatus47.Text = "范围内 友" + flankResult.Friends + " / 敌" + flankResult.Enemies + "；含终点 友" + flankResult.IncomingFriends + " / 敌" + flankResult.IncomingEnemies;
		if (flankResult.Danger && DateTime.UtcNow >= flankNextSound47)
		{
			PlayFlank47();
			flankNextSound47 = DateTime.UtcNow.AddSeconds(flankRepeat47);
		}
		if (!flankResult.Danger)
		{
			flankNextSound47 = DateTime.MinValue;
		}
	}

	private void TickFlank47()
	{
		if (!LicenseGate60.Allowed || reader == null || !flankEnabled47 || (!flankActive47 && flankArmed47 <= DateTime.UtcNow))
		{
			flankWindow47.Hide();
			return;
		}
		IntPtr foregroundWindow = Native.GetForegroundWindow();
		Native.GetWindowThreadProcessId(foregroundWindow, out var num);
		if (num != (uint)pid || !GetClientRect(foregroundWindow, out var rect) || rect.Right < 200)
		{
			flankWindow47.Hide();
			return;
		}
		bool flag = (GetAsyncKeyState(1) & 0x8000) != 0;
		try
		{
			if (flankArmed47 > DateTime.UtcNow && (GetAsyncKeyState(27) & 0x8000) != 0)
			{
				flankArmed47 = DateTime.MinValue;
			}
			MineCamera camera = reader.ReadMineCamera();
			if (flankArmed47 > DateTime.UtcNow && flag && !flankWasDown47)
			{
				flankArmed47 = DateTime.MinValue;
				Point at = Cursor.Position;
				ScreenToClient(foregroundWindow, ref at);
				if (at.X < 0 || at.Y < 0 || at.X >= rect.Right || (double)at.Y >= (double)rect.Bottom * 0.78 || DateTime.UtcNow - flankFresh47 > TimeSpan.FromSeconds(3.0))
				{
					throw new Exception("请点击主场景（避开底部面板），并等待对局数据就绪。");
				}
				var source = (from x in flankUnits47.Select(delegate(SquadMarker u)
					{
						PointF point;
						bool flag2 = camera.Project(u.X, u.Y + 0.8f, u.Z, rect.Right, rect.Bottom, out point);
						return new
						{
							Unit = u,
							Distance = (flag2 ? ((double)((point.X - (float)at.X) * (point.X - (float)at.X) + (point.Y - (float)at.Y) * (point.Y - (float)at.Y))) : double.MaxValue)
						};
					})
					orderby x.Distance
					select x).ToArray();
				var anon = source.FirstOrDefault();
				if (anon != null && anon.Distance < 1936.0)
				{
					SquadMarker unit = anon.Unit;
					flankX47 = unit.X;
					flankY47 = unit.Y;
					flankZ47 = unit.Z;
					flankSource47 = unit.Source;
					flankIdentity47 = unit.Identity;
					flankStatus47.Text = "跟随单位：" + unit.Name;
				}
				else
				{
					flankY47 = ((anon != null && anon.Distance < double.MaxValue) ? anon.Unit.Y : 0f);
					if (!FlankLogic47.Ground(camera, at, rect.Right, rect.Bottom, flankY47, out flankX47, out flankZ47))
					{
						throw new Exception("无法定位地面，请改点单位。");
					}
					flankSource47 = 0L;
					flankStatus47.Text = "固定点已标记（地形高度估算，可改点单位）。";
				}
				flankActive47 = true;
				flankNextSound47 = DateTime.MinValue;
				flankResult47 = null;
			}
			if (TacticalMapState51.Open(pid, reader.MatchKey))
			{
				flankWindow47.Hide();
				return;
			}
			if (!flankActive47 || DateTime.UtcNow - flankFresh47 > TimeSpan.FromSeconds(3.0))
			{
				flankWindow47.Hide();
				return;
			}
			Point p = Point.Empty;
			ClientToScreen(foregroundWindow, ref p);
			flankWindow47.Bounds = new Rectangle(p.X, p.Y, rect.Right, rect.Bottom);
			flankWindow47.Camera = camera;
			flankWindow47.X = flankX47;
			flankWindow47.Y = flankY47;
			flankWindow47.Z = flankZ47;
			flankWindow47.Radius = flankRadius47;
			flankWindow47.ShowRange52 = flankRange52;
			flankWindow47.RangeWidth52 = (float)flankRangeWidth52;
			flankWindow47.OwnColour52 = FlankRangeDrawing52.OwnColour(rows);
			flankWindow47.Danger = flankResult47 != null && flankResult47.Danger;
			flankWindow47.Caption = (flankWindow47.Danger ? "包抄危险" : "包抄监测") + " · 半径 " + flankRadius47;
			if (!flankWindow47.Visible)
			{
				flankWindow47.Show();
			}
			flankWindow47.Invalidate();
		}
		catch (Exception ex)
		{
			flankWindow47.Hide();
			flankStatus47.Text = ex.Message;
		}
		finally
		{
			flankWasDown47 = flag;
		}
	}

	private void PlayFlank47()
	{
		if (previewOnlyMode || !LicenseGate60.Allowed)
		{
			return;
		}
		long epoch = LicenseGate60.Epoch;
		string file = (string.IsNullOrWhiteSpace(flankSound47) ? DefaultSounds61.FlankFile : flankSound47);
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				if (LicenseGate60.Allowed && LicenseGate60.Epoch == epoch)
				{
					if (!string.IsNullOrWhiteSpace(file))
					{
						using (AudioClip audioClip = new AudioClip(file))
						{
							audioClip.PlaySync(epoch);
							return;
						}
					}
					SystemSounds.Exclamation.Play();
				}
			}
			catch (Exception)
			{
				try
				{
					BeginInvoke((Action)delegate
					{
						flankStatus47.Text = "包抄音源无法播放，请重新选择。";
					});
				}
				catch
				{
				}
			}
		});
	}

	private void ResetFlank47()
	{
		flankActive47 = false;
		flankArmed47 = DateTime.MinValue;
		flankUnits47.Clear();
		flankResult47 = null;
		if (flankWindow47 != null)
		{
			flankWindow47.Hide();
		}
		if (flankStatus47 != null)
		{
			flankStatus47.Text = "尚未设置监测点。";
		}
	}

	private void DisposeFlank47()
	{
		flankTimer47.Stop();
		flankTimer47.Dispose();
		if (flankRegistered47)
		{
			Native.UnregisterHotKey(base.Handle, 203);
		}
		if (flankWindow47 != null)
		{
			flankWindow47.Dispose();
			flankWindow47 = null;
		}
	}

	public void SetLicenseExpiry60(long unix, string kind = "")
	{
		licenseExpiry60 = FormatLicenseExpiry60(unix, kind);
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
		if (previewOnlyMode || !LicenseGate60.Allowed)
		{
			return;
		}
		supportTimer49.Start();
		flankTimer47.Start();
		if (!flankRegistered47)
		{
			try
			{
				RegisterFlank47(flankKey47);
			}
			catch
			{
			}
		}
		if (!polling.Enabled)
		{
			StartTracking();
		}
	}

	public void StopProtected60()
	{
		if (!previewOnlyMode)
		{
			polling.Stop();
			supportTimer49.Stop();
			flankTimer47.Stop();
			if (copyRegistered)
			{
				Native.UnregisterHotKey(base.Handle, 201);
				copyRegistered = false;
			}
			if (flankRegistered47)
			{
				Native.UnregisterHotKey(base.Handle, 203);
				flankRegistered47 = false;
			}
			sounds.Pause60();
			DropReader();
			AudioClip.StopAll60();
		}
	}

	private void BuildLicenseControls60(Control parent)
	{
		Button46(parent, "更换激活码", delegate
		{
			this.ChangeLicense60?.Invoke();
		}, 150);
		Button46(parent, "查看授权期限", delegate
		{
			using LicenseNotice60 licenseNotice = new LicenseNotice60("授权期限", string.IsNullOrEmpty(licenseExpiry60) ? "授权信息正在验证。" : licenseExpiry60);
			licenseNotice.ShowDialog(this);
		}, 150);
	}

	private FlankMapRange55 MapRange55()
	{
		if (!flankEnabled47 || !flankActive47 || !flankRange52 || reader == null || flankMatch47 != reader.MatchKey || DateTime.UtcNow - flankFresh47 > TimeSpan.FromSeconds(3.0))
		{
			return null;
		}
		FlankMapRange55 flankMapRange = new FlankMapRange55();
		flankMapRange.X = flankX47;
		flankMapRange.Z = flankZ47;
		flankMapRange.Radius = flankRadius47;
		flankMapRange.Width = (float)flankRangeWidth52;
		flankMapRange.Colour = FlankRangeDrawing52.OwnColour(rows);
		return flankMapRange;
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

	[DllImport("user32.dll")]
	private static extern bool ReleaseCapture();

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr h, int m, IntPtr w, IntPtr l);

	private void BuildInterface45()
	{
		base.AutoScaleMode = AutoScaleMode.None;
		Font = Interface45.Font();
		base.Icon = Interface45.Icon;
		BackColor = Palette46.Back;
		ForeColor = Palette46.Ink;
		base.FormBorderStyle = FormBorderStyle.None;
		base.Padding = new Padding(8);
		base.MaximizedBounds = Screen.FromControl(this).WorkingArea;
		Surface46 surface = new Surface46();
		surface.Dock = DockStyle.Fill;
		surface.Padding = new Padding(10);
		Surface46 surface2 = surface;
		base.Controls.Add(surface2);
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
		tableLayoutPanel.Dock = DockStyle.Fill;
		tableLayoutPanel.ColumnCount = 1;
		tableLayoutPanel.RowCount = 5;
		tableLayoutPanel.Padding = Padding.Empty;
		tableLayoutPanel.Margin = Padding.Empty;
		tableLayoutPanel.BackColor = Palette46.Back;
		TableLayoutPanel tableLayoutPanel2 = tableLayoutPanel;
		surface2.Controls.Add(tableLayoutPanel2);
		int[] array = new int[2] { 52, 50 };
		foreach (int num in array)
		{
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, num));
		}
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
		Panel panel = new Panel();
		panel.Dock = DockStyle.Fill;
		Panel panel2 = panel;
		tableLayoutPanel2.Controls.Add(panel2, 0, 0);
		Label label = new Label();
		label.Text = "◉  天穹之眼";
		label.Font = Interface45.Font(21f, FontStyle.Bold);
		label.ForeColor = Palette46.Ink;
		label.AutoSize = true;
		label.Location = new Point(4, 4);
		Label label2 = label;
		panel2.Controls.Add(label2);
		Label label3 = new Label();
		label3.Text = "TACTICAL SYSTEM  /  战术信息控制台";
		label3.Font = Interface45.Font(9f);
		label3.ForeColor = Palette46.Muted;
		label3.AutoSize = true;
		label3.Location = new Point(260, 15);
		Label label4 = label3;
		panel2.Controls.Add(label4);
		MouseEventHandler value = delegate(object s, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(base.Handle, 161, new IntPtr(2), IntPtr.Zero);
			}
		};
		panel2.MouseDown += value;
		label2.MouseDown += value;
		label4.MouseDown += value;
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
		flowLayoutPanel.Dock = DockStyle.Right;
		flowLayoutPanel.Width = 320;
		flowLayoutPanel.FlowDirection = FlowDirection.LeftToRight;
		flowLayoutPanel.WrapContents = false;
		FlowLayoutPanel flowLayoutPanel2 = flowLayoutPanel;
		panel2.Controls.Add(flowLayoutPanel2);
		connection46 = new Label
		{
			Text = "○ 等待对局",
			Width = 140,
			Height = 34,
			ForeColor = Palette46.Amber,
			TextAlign = ContentAlignment.MiddleRight
		};
		flowLayoutPanel2.Controls.Add(connection46);
		Button46(flowLayoutPanel2, "收起", delegate
		{
			Hide();
		}, 52);
		Button46(flowLayoutPanel2, "□", delegate
		{
			base.WindowState = ((base.WindowState != FormWindowState.Maximized) ? FormWindowState.Maximized : FormWindowState.Normal);
			Fit46();
		}, 42);
		Button46(flowLayoutPanel2, "×", delegate
		{
			Close();
		}, 42);
		FlowLayoutPanel flowLayoutPanel3 = new FlowLayoutPanel();
		flowLayoutPanel3.Dock = DockStyle.Fill;
		flowLayoutPanel3.WrapContents = false;
		flowLayoutPanel3.Padding = new Padding(8, 4, 0, 0);
		flowLayoutPanel3.BackColor = Palette46.Raised;
		FlowLayoutPanel flowLayoutPanel4 = flowLayoutPanel3;
		tableLayoutPanel2.Controls.Add(flowLayoutPanel4, 0, 1);
		flowLayoutPanel4.Controls.Add(Label46("快速控制", 90));
		Toggle46(flowLayoutPanel4, "顶部玩家信息", () => preferences.Enabled, delegate(bool v)
		{
			preferences.Enabled = v;
		}, 210);
		Toggle46(flowLayoutPanel4, "主画面标记", () => preferences.SquadMarkers.Enabled, delegate(bool v)
		{
			preferences.SquadMarkers.Enabled = v;
		}, 210);
		Toggle46(flowLayoutPanel4, "小地图标记", () => preferences.Minimap.Enabled, delegate(bool v)
		{
			preferences.Minimap.Enabled = v;
		}, 210);
		flowLayoutPanel4.Controls.Add(Label46("显示参数自动保存", 165, Palette46.Muted));
		TableLayoutPanel tableLayoutPanel3 = new TableLayoutPanel();
		tableLayoutPanel3.Dock = DockStyle.Fill;
		tableLayoutPanel3.ColumnCount = 3;
		tableLayoutPanel3.RowCount = 1;
		tableLayoutPanel3.Margin = new Padding(0, 10, 0, 0);
		tableLayoutPanel3.BackColor = Palette46.Back;
		TableLayoutPanel tableLayoutPanel4 = tableLayoutPanel3;
		tableLayoutPanel2.Controls.Add(tableLayoutPanel4, 0, 2);
		for (int num2 = 0; num2 < 3; num2++)
		{
			tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
			columns46[num2] = new Stack46();
			tableLayoutPanel4.Controls.Add(columns46[num2], num2, 0);
		}
		Module46 module = Module46(0, "01", "玩家情报");
		Players46(module.Body);
		Module46 module2 = Module46(1, "02", "战场标记");
		World46(module2.Body);
		Module46 module3 = Module46(2, "03", "炮击与空袭");
		Threat46(module3.Body);
		module = Module46(0, "04", "小地图与战术地图");
		Mini46(module.Body);
		module2 = Module46(1, "05", "声音提醒");
		Sounds46(module2.Body);
		module3 = Module46(2, "06", "快捷操作");
		Actions46(module3.Body);
		preview.Dock = DockStyle.Fill;
		preview.Paint += PaintPreview;
		preview.Margin = new Padding(0, 8, 8, 0);
		tableLayoutPanel2.Controls.Add(preview, 0, 3);
		status.Dock = DockStyle.Fill;
		status.ForeColor = Palette46.Muted;
		status.Font = Interface45.Font(8f);
		status.TextAlign = ContentAlignment.MiddleLeft;
		status.AutoEllipsis = true;
		tableLayoutPanel2.Controls.Add(status, 0, 4);
		base.Shown += delegate
		{
			Fit46();
		};
		base.SizeChanged += delegate
		{
			Fit46();
		};
		BuildFlank47();
		InitSupport49();
		SyncConsole46();
	}

	private void Fit46()
	{
		Stack46[] array = columns46;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Fit();
		}
	}

	private Module46 Module46(int column, string number, string title)
	{
		Module46 module = new Module46(number, title);
		columns46[column].Controls.Add(module);
		return module;
	}

	private Label Label46(string text, int width = 160, Color? colour = null)
	{
		Label label = new Label();
		label.Text = text;
		label.ForeColor = colour ?? Palette46.Ink;
		label.Width = width;
		label.Height = 30;
		label.TextAlign = ContentAlignment.MiddleLeft;
		label.Font = Interface45.Font(9f);
		label.Margin = new Padding(0, 2, 3, 2);
		label.AutoEllipsis = true;
		return label;
	}

	private Action46 Button46(Control parent, string title, Action action, int width = 130)
	{
		Action46 action2 = new Action46();
		action2.Text = title;
		action2.Width = width;
		Action46 action3 = action2;
		action3.Click += delegate
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				status.Text = ex.Message;
			}
		};
		parent.Controls.Add(action3);
		return action3;
	}

	private Toggle46 Toggle46(Control parent, string title, Func<bool> read, Action<bool> write, int width = 146)
	{
		Toggle46 b = new Toggle46
		{
			Text = title,
			Width = width,
			Checked = read()
		};
		b.CheckedChanged += delegate
		{
			if (!sync46)
			{
				write(b.Checked);
				Changed();
				SyncConsole46();
			}
		};
		bindings46.Add(new KeyValuePair<Toggle46, Func<bool>>(b, read));
		parent.Controls.Add(b);
		return b;
	}

	private void Number46(Control parent, string title, decimal value, decimal low, decimal high, Action<decimal> write, int digits = 0)
	{
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
		flowLayoutPanel.Width = 150;
		flowLayoutPanel.Height = 32;
		flowLayoutPanel.WrapContents = false;
		flowLayoutPanel.Margin = new Padding(0, 2, 4, 2);
		FlowLayoutPanel flowLayoutPanel2 = flowLayoutPanel;
		flowLayoutPanel2.Controls.Add(Label46(title, 73, Palette46.Muted));
		Stepper46 stepper = new Stepper46(low, high, value, digits);
		stepper.Margin = new Padding(0, 3, 0, 0);
		Stepper46 stepper2 = stepper;
		flowLayoutPanel2.Controls.Add(stepper2);
		stepper2.Changed = delegate(decimal v)
		{
			write(v);
			Changed();
		};
		parent.Controls.Add(flowLayoutPanel2);
	}

	private void Colour46(Control parent, string title, Func<Color> get, Action<Color> set)
	{
		Action46 b = Button46(parent, title, delegate
		{
		}, 146);
		b.Swatch = get;
		b.Click += delegate
		{
			using ColorDialog colorDialog = new ColorDialog
			{
				Color = get(),
				FullOpen = true
			};
			if (colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				set(colorDialog.Color);
				b.Invalidate();
				Changed();
			}
		};
	}

	private Fold46 Fold46(Control parent, string title, bool expanded = false)
	{
		Fold46 fold = new Fold46(title);
		parent.Controls.Add(fold);
		fold.Expanded = expanded;
		return fold;
	}

	private Fold46 Feature46(Control parent, string title, Func<bool> read, Action<bool> write)
	{
		Fold46 f = Fold46(parent, title);
		f.IsFeature = true;
		f.Header.Text = "";
		Toggle46 toggle = Toggle46(f.Header, title, read, write, 270);
		toggle.Location = new Point(4, 0);
		f.Header.SizeChanged += delegate
		{
			toggle.Width = f.Header.Width - 50;
		};
		Label label = new Label();
		label.BackColor = Palette46.Panel;
		label.Text = "⌄";
		label.ForeColor = Palette46.Light;
		label.TextAlign = ContentAlignment.MiddleCenter;
		label.Size = new Size(36, 30);
		label.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		label.Location = new Point(f.Header.Width - 40, 0);
		Label label2 = label;
		label2.Click += delegate
		{
			f.Expanded = !f.Expanded;
		};
		f.Header.Controls.Add(label2);
		return f;
	}

	private void Full46(Control parent, Control child, int height)
	{
		child.Tag = "full";
		child.Width = 390;
		child.Height = height;
		child.Margin = new Padding(0, 4, 0, 6);
		parent.Controls.Add(child);
	}

	private void Players46(Control p)
	{
		Toggle46(p, "持有单位", () => preferences.ShowUnits, delegate(bool v)
		{
			preferences.ShowUnits = v;
		});
		Toggle46(p, "战斗群技能", () => preferences.ShowSkills, delegate(bool v)
		{
			preferences.ShowSkills = v;
		});
		enemies.CheckOnClick = true;
		enemies.IntegralHeight = false;
		enemies.BackColor = Palette46.Back;
		enemies.ForeColor = Palette46.Ink;
		enemies.BorderStyle = BorderStyle.FixedSingle;
		enemies.Font = Interface45.Font(9f);
		Full46(p, enemies, 90);
		enemies.ItemCheck += delegate(object s, ItemCheckEventArgs e)
		{
			if (!updatingEnemies)
			{
				selection.Set(((EnemyChoice)enemies.Items[e.Index]).Player, e.NewValue == CheckState.Checked);
				RefreshSelection();
			}
		};
		Number46(p, "基础字号", preferences.FontSize, 9m, 36m, delegate(decimal v)
		{
			preferences.FontSize = (int)v;
		});
		Number46(p, "详情字号", preferences.DetailSize, 9m, 36m, delegate(decimal v)
		{
			preferences.DetailSize = (int)v;
		});
		Colour46(p, "基础行颜色", () => preferences.Color, delegate(Color v)
		{
			preferences.Color = v;
		});
		Colour46(p, "详情行颜色", () => preferences.SkillColor, delegate(Color v)
		{
			preferences.SkillColor = v;
		});
		Fold46 fold = Fold46(p, "信息类别：资源 / 进度 / 战斗群");
		string[] array = new string[10] { "人力", "弹药", "油料", "人口", "CP", "战斗群", "基地建造", "正在生产", "手雷状态", "250升级" };
		for (int num = 0; num < array.Length; num++)
		{
			int index = num;
			Toggle46(fold.Body, array[index], () => preferences.Info49[index], delegate(bool v)
			{
				preferences.Info49[index] = v;
			});
		}
		Fold46 fold2 = Fold46(p, "显示对象与信息位置");
		Toggle46(fold2.Body, "包括自己队友", () => preferences.ShowAllPlayers, delegate(bool v)
		{
			preferences.ShowAllPlayers = v;
		});
		Toggle46(fold2.Body, "跟随玩家颜色", () => preferences.UseGameColours, delegate(bool v)
		{
			preferences.UseGameColours = v;
		});
		Number46(fold2.Body, "顶部距离 px", preferences.Top, 0m, 300m, delegate(decimal v)
		{
			preferences.Top = (int)v;
		});
		Number46(fold2.Body, "信息宽度 %", preferences.WidthPercent, 40m, 100m, delegate(decimal v)
		{
			preferences.WidthPercent = (int)v;
		});
		Button46(fold2.Body, "全部显示", delegate
		{
			SetAll(enabled: true);
		}, 145);
		Button46(fold2.Body, "全部隐藏", delegate
		{
			SetAll(enabled: false);
		}, 145);
		Fold46 fold3 = Fold46(p, "数值渐变与闪烁");
		foreach (KeyValuePair<string, ResourceColourRule> item in preferences.Resources.Rules())
		{
			string title = ((item.Key == "manpower") ? "人力" : ((item.Key == "munition") ? "弹药" : ((item.Key == "fuel") ? "油料" : "人口")));
			ResourceColourRule r = item.Value;
			Fold46 fold4 = Fold46(fold3.Body, title);
			Toggle46(fold4.Body, "启用独立配色", () => r.Enabled, delegate(bool v)
			{
				r.Enabled = v;
			});
			Colour46(fold4.Body, "零值颜色", () => r.Zero, delegate(Color v)
			{
				r.Zero = v;
			});
			Colour46(fold4.Body, "目标颜色", () => r.Target, delegate(Color v)
			{
				r.Target = v;
			});
			Colour46(fold4.Body, "闪烁颜色", () => r.Flash, delegate(Color v)
			{
				r.Flash = v;
			});
			Number46(fold4.Body, "渐变终点", r.TargetValue, 1m, 1000000m, delegate(decimal v)
			{
				r.TargetValue = v;
			});
			Number46(fold4.Body, "闪烁阈值", r.FlashAbove, 0m, 1000000m, delegate(decimal v)
			{
				r.FlashAbove = v;
			});
			Number46(fold4.Body, "闪烁频率 Hz", r.Frequency, 0.1m, 5m, delegate(decimal v)
			{
				r.Frequency = v;
			}, 1);
		}
	}

	private void World46(Control p)
	{
		SquadMarkerPreferences s = preferences.SquadMarkers;
		Fold46 fold = Fold46(p, "班组图形与名称", expanded: true);
		Number46(fold.Body, "步兵圆环", s.InfantrySize, 8m, 200m, delegate(decimal v)
		{
			s.InfantrySize = (int)v;
		});
		Number46(fold.Body, "载具方框", s.VehicleSize, 8m, 200m, delegate(decimal v)
		{
			s.VehicleSize = (int)v;
		});
		Number46(fold.Body, "描边粗细", s.Stroke, 0.5m, 12m, delegate(decimal v)
		{
			s.Stroke = v;
		}, 1);
		Number46(fold.Body, "名称字号", s.TextSize, 8m, 36m, delegate(decimal v)
		{
			s.TextSize = (int)v;
		});
		Fold46 fold2 = Feature46(p, "移动目标连线", () => s.Movement, delegate(bool v)
		{
			s.Movement = v;
		});
		Number46(fold2.Body, "终点叉号 px", s.MoveCrossSize, 8m, 160m, delegate(decimal v)
		{
			s.MoveCrossSize = (int)v;
		});
		Number46(fold2.Body, "连线粗细 px", s.MoveWidth, 0.5m, 12m, delegate(decimal v)
		{
			s.MoveWidth = v;
		}, 1);
		Number46(fold2.Body, "虚线段长 px", s.MoveDashLength, 1m, 100m, delegate(decimal v)
		{
			s.MoveDashLength = v;
		}, 1);
		Number46(fold2.Body, "虚线间隔 px", s.MoveDashGap, 1m, 100m, delegate(decimal v)
		{
			s.MoveDashGap = v;
		}, 1);
		Fold46 fold3 = Feature46(p, "朝向与射程线", () => s.Directions, delegate(bool v)
		{
			s.Directions = v;
		});
		Toggle46(fold3.Body, "机枪自动射界", () => s.AutoSectors, delegate(bool v)
		{
			s.AutoSectors = v;
		});
		Toggle46(fold3.Body, "小队实际射程", () => s.AutoTeamRange, delegate(bool v)
		{
			s.AutoTeamRange = v;
		});
		Toggle46(fold3.Body, "载具实际射程", () => s.AutoVehicleRange, delegate(bool v)
		{
			s.AutoVehicleRange = v;
		});
		Toggle46(fold3.Body, "备用示意射界", () => s.Sectors, delegate(bool v)
		{
			s.Sectors = v;
		});
		Number46(fold3.Body, "载具自定义长", s.VehicleLength, 1m, 200m, delegate(decimal v)
		{
			s.VehicleLength = (int)v;
		});
		Number46(fold3.Body, "小队自定义长", s.TeamLength, 1m, 200m, delegate(decimal v)
		{
			s.TeamLength = (int)v;
		});
		Number46(fold3.Body, "火炮方向长度", s.ArtilleryLength, 1m, 200m, delegate(decimal v)
		{
			s.ArtilleryLength = (int)v;
		});
		Number46(fold3.Body, "方向线宽 px", s.DirectionWidth, 0.5m, 12m, delegate(decimal v)
		{
			s.DirectionWidth = v;
		}, 1);
		Number46(fold3.Body, "备用射界角度", s.SectorAngle, 0m, 180m, delegate(decimal v)
		{
			s.SectorAngle = (int)v;
		});
		Fold46 fold4 = Feature46(p, "地雷与炸药", () => preferences.ShowMines, delegate(bool v)
		{
			preferences.ShowMines = v;
		});
		Colour46(fold4.Body, "敌方颜色", () => preferences.EnemyMineColor, delegate(Color v)
		{
			preferences.EnemyMineColor = v;
		});
		Colour46(fold4.Body, "己方队友颜色", () => preferences.FriendlyMineColor, delegate(Color v)
		{
			preferences.FriendlyMineColor = v;
		});
		Number46(p, "镜头 帧/秒", s.FrameRate, 10m, 144m, delegate(decimal v)
		{
			s.FrameRate = (int)v;
		});
		Number46(p, "位置 次/秒", s.PositionRate, 5m, 144m, delegate(decimal v)
		{
			s.PositionRate = (int)v;
		});
		Button46(p, "统计开关", delegate
		{
			s.ShowMetrics = !s.ShowMetrics;
			Changed();
			status.Text = (s.ShowMetrics ? "已开启游戏内刷新统计" : "已关闭游戏内刷新统计");
		}, 80);
	}

	private void Threat46(Control p)
	{
		AirPreferences a = preferences.SquadMarkers.Air;
		ProjectilePreferences s = preferences.SquadMarkers.Projectiles;
		Toggle46(p, "弹幕技能目标", () => preferences.ShowEnemyBarrages, delegate(bool v)
		{
			preferences.ShowEnemyBarrages = v;
		});
		Number46(p, "弹幕圆环", preferences.BarrageMarkerSize, 8m, 240m, delegate(decimal v)
		{
			preferences.BarrageMarkerSize = (int)v;
		});
		Fold46 fold = Feature46(p, "逐发炮弹落点", () => s.Enabled, delegate(bool v)
		{
			s.Enabled = v;
		});
		Toggle46(fold.Body, "自己的炮弹", () => s.ShowOwn, delegate(bool v)
		{
			s.ShowOwn = v;
		});
		Toggle46(fold.Body, "队友的炮弹", () => s.ShowAllies, delegate(bool v)
		{
			s.ShowAllies = v;
		});
		Toggle46(fold.Body, "逐发范围圆环", () => s.Rings, delegate(bool v)
		{
			s.Rings = v;
		});
		Number46(fold.Body, "编号大小", s.NumberSize, 8m, 72m, delegate(decimal v)
		{
			s.NumberSize = (int)v;
		});
		Number46(fold.Body, "名称字号", s.TextSize, 8m, 36m, delegate(decimal v)
		{
			s.TextSize = (int)v;
		});
		Number46(fold.Body, "圆环线宽 px", s.RingWidth, 0.5m, 8m, delegate(decimal v)
		{
			s.RingWidth = v;
		}, 1);
		Fold46 fold2 = Feature46(p, "飞机条形区域", () => a.Enabled, delegate(bool v)
		{
			a.Enabled = v;
		});
		Toggle46(p, "主画面 V1", () => a.V1, delegate(bool v)
		{
			a.V1 = v;
		});
		Number46(p, "不透明度 %", a.Opacity, 0m, 100m, delegate(decimal v)
		{
			a.Opacity = (int)v;
		});
		Number46(fold2.Body, "主画面字号", a.TextSize, 8m, 40m, delegate(decimal v)
		{
			a.TextSize = (int)v;
		});
		Number46(fold2.Body, "区域线宽 px", a.Width, 0.5m, 8m, delegate(decimal v)
		{
			a.Width = v;
		}, 1);
		Number46(fold2.Body, "小地图字号", a.MiniTextSize, 6m, 24m, delegate(decimal v)
		{
			a.MiniTextSize = (int)v;
		});
		Number46(fold2.Body, "小图不透明%", a.MiniOpacity, 0m, 100m, delegate(decimal v)
		{
			a.MiniOpacity = (int)v;
		});
		Button46(p, "清空当前飞机矩形", delegate
		{
			squadOverlay.ClearAir();
			airOverlay.ClearAir();
			status.Text = "当前矩形已清空；新的空袭仍会正常显示。";
		}, 210);
	}

	private void Mini46(Control p)
	{
		MinimapPreferences m = preferences.Minimap;
		Toggle46(p, "战术地图标记", () => preferences.Tactical51.Enabled, delegate(bool v)
		{
			preferences.Tactical51.Enabled = v;
		});
		Number46(p, "战术字号", preferences.Tactical51.TextSize, 8m, 40m, delegate(decimal v)
		{
			preferences.Tactical51.TextSize = (int)v;
		});
		Toggle46(p, "移动连线终点", () => m.Movement, delegate(bool v)
		{
			m.Movement = v;
		});
		Toggle46(p, "弹幕落点", () => m.Barrages, delegate(bool v)
		{
			m.Barrages = v;
		});
		Toggle46(p, "V1 范围", () => preferences.SquadMarkers.Air.MiniV1, delegate(bool v)
		{
			preferences.SquadMarkers.Air.MiniV1 = v;
		});
		Number46(p, "刷新 次/秒", m.RefreshRate, 1m, 60m, delegate(decimal v)
		{
			m.RefreshRate = (int)v;
		});
		Number46(p, "步兵圆点", m.InfantrySize, 2m, 40m, delegate(decimal v)
		{
			m.InfantrySize = (int)v;
		});
		Number46(p, "载具三角", m.VehicleSize, 3m, 60m, delegate(decimal v)
		{
			m.VehicleSize = (int)v;
		});
		Fold46 fold = Fold46(p, "连线与终点尺寸");
		Number46(fold.Body, "连线粗细 px", m.MoveWidth, 0.5m, 8m, delegate(decimal v)
		{
			m.MoveWidth = v;
		}, 1);
		Number46(fold.Body, "终点叉号 px", m.MoveCrossSize, 2m, 40m, delegate(decimal v)
		{
			m.MoveCrossSize = (int)v;
		});
		Number46(fold.Body, "弹幕星号 px", m.BarrageSize, 4m, 60m, delegate(decimal v)
		{
			m.BarrageSize = (int)v;
		});
		Button46(p, "重新连接地图标记", RecoverMini45, 230);
	}

	private void Sounds46(Control p)
	{
		Toggle46(p, "启用声音提示", () => sounds.Enabled, delegate(bool v)
		{
			sounds.Enabled = v;
			sounds.Save();
		});
		p.Controls.Add(Label46("MP3 / WAV", 110, Palette46.Muted));
		foreach (AlertKind value in Enum.GetValues(typeof(AlertKind)))
		{
			AlertKind k = value;
			FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
			flowLayoutPanel.Tag = "full";
			flowLayoutPanel.Width = 390;
			flowLayoutPanel.Height = 34;
			flowLayoutPanel.WrapContents = false;
			flowLayoutPanel.Margin = new Padding(0, 1, 0, 1);
			FlowLayoutPanel flowLayoutPanel2 = flowLayoutPanel;
			p.Controls.Add(flowLayoutPanel2);
			string title = ((k == AlertKind.EnemySldFlare) ? "SLD星爆照明弹" : ((k == AlertKind.BattleGroup) ? "选择战斗群" : ((k == AlertKind.BaseComplete) ? "基地建成" : ((k == AlertKind.GrenadeReady) ? "手雷升级" : ((k == AlertKind.EnemyArmourProduction) ? "装甲生产" : ((k == AlertKind.EnemyArtilleryBarrage) ? "敌方弹幕" : "己方火炮就绪"))))));
			flowLayoutPanel2.Controls.Add(Label46(title, 110));
			Action46 choose = Button46(flowLayoutPanel2, "音源", delegate
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Title = "选择" + title + "音源",
					Filter = "MP3 / WAV|*.mp3;*.wav|所有文件|*.*"
				};
				if (openFileDialog.ShowDialog(this) == DialogResult.OK)
				{
					sounds.Set(k, openFileDialog.FileName);
					status.Text = title + "：" + sounds.DisplayFile(k);
					SyncConsole46();
				}
			}, 62);
			tips46.SetToolTip(choose, sounds.DisplayFile(k));
			Button46(flowLayoutPanel2, "试听", delegate
			{
				sounds.Play(k, preview: true);
			}, 52);
			Button46(flowLayoutPanel2, "重置", delegate
			{
				sounds.Set(k, null);
				tips46.SetToolTip(choose, sounds.DisplayFile(k));
				status.Text = title + "：" + sounds.DisplayFile(k);
			}, 52);
		}
		Fold46 fold = Fold46(p, "装甲提醒玩家（每辆均提醒）");
		armourList46.CheckOnClick = true;
		armourList46.IntegralHeight = false;
		armourList46.BorderStyle = BorderStyle.FixedSingle;
		armourList46.BackColor = Palette46.Back;
		armourList46.ForeColor = Palette46.Ink;
		Full46(fold.Body, armourList46, 100);
		armourList46.ItemCheck += delegate(object sender, ItemCheckEventArgs e)
		{
			if (!armourSync46)
			{
				armourSelection.Set(((EnemyChoice)armourList46.Items[e.Index]).Player, e.NewValue == CheckState.Checked);
			}
		};
		Button46(fold.Body, "全部开启", delegate
		{
			foreach (PlayerInfoRow item in rows.Where((PlayerInfoRow x) => x.Relation == "敌方"))
			{
				armourSelection.Set(item, enabled: true);
			}
			SyncArmour46();
		}, 145);
		Button46(fold.Body, "全部关闭", delegate
		{
			foreach (PlayerInfoRow item2 in rows.Where((PlayerInfoRow x) => x.Relation == "敌方"))
			{
				armourSelection.Set(item2, enabled: false);
			}
			SyncArmour46();
		}, 145);
	}

	private void Actions46(Control p)
	{
		BuildLicenseControls60(p);
		ComboBox comboBox = copyMods;
		ComboBoxStyle dropDownStyle = (copyKey.DropDownStyle = ComboBoxStyle.DropDownList);
		comboBox.DropDownStyle = dropDownStyle;
		copyMods.Width = 110;
		copyKey.Width = 65;
		ComboBox comboBox2 = copyMods;
		FlatStyle flatStyle = (copyKey.FlatStyle = FlatStyle.Flat);
		comboBox2.FlatStyle = flatStyle;
		ComboBox comboBox3 = copyMods;
		Color backColor = (copyKey.BackColor = Palette46.Raised);
		comboBox3.BackColor = backColor;
		ComboBox comboBox4 = copyMods;
		Color foreColor = (copyKey.ForeColor = Palette46.Ink);
		comboBox4.ForeColor = foreColor;
		copyMods.Items.AddRange(new object[5] { "无修饰键", "Ctrl", "Alt", "Shift", "Ctrl+Alt" });
		copyMods.SelectedIndex = 0;
		for (int i = 1; i <= 12; i++)
		{
			copyKey.Items.Add("F" + i);
		}
		for (char c = 'A'; c <= 'Z'; c = (char)(c + 1))
		{
			copyKey.Items.Add(c.ToString());
		}
		copyKey.SelectedItem = "F8";
		LoadCopyKey();
		ComboBox[] array = new ComboBox[2] { copyMods, copyKey };
		foreach (ComboBox comboBox5 in array)
		{
			comboBox5.DrawMode = DrawMode.OwnerDrawFixed;
			comboBox5.Font = Interface45.Font(9f);
			comboBox5.ItemHeight = 22;
			comboBox5.DrawItem += delegate(object sender, DrawItemEventArgs e)
			{
				if (e.Index >= 0)
				{
					ComboBox comboBox6 = (ComboBox)sender;
					using (SolidBrush brush = new SolidBrush(((e.State & DrawItemState.Selected) != DrawItemState.None) ? Palette46.Raised : Palette46.Back))
					{
						e.Graphics.FillRectangle(brush, e.Bounds);
					}
					TextRenderer.DrawText(e.Graphics, comboBox6.Items[e.Index].ToString(), comboBox6.Font, e.Bounds, Palette46.Ink, TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.PreserveGraphicsTranslateTransform);
				}
			};
		}
		p.Controls.Add(Label46("复制信息热键", 130));
		p.Controls.Add(copyMods);
		p.Controls.Add(copyKey);
		Button46(p, "保存热键", ApplyCopyKey, 150);
		Button46(p, "立即复制", CopyEnemyInfo, 150);
		Button46(p, "空袭显示自检（8秒）", delegate
		{
			squadOverlay.TestAir();
			airOverlay.TestAir();
			status.Text = "请返回游戏观察中央自检矩形。";
		}, 298);
		Button46(p, "收起到托盘", delegate
		{
			Hide();
		}, 298);
		Fold46 fold = Fold46(p, "操作说明");
		Full46(fold.Body, new Label
		{
			Text = "展开区就在当前模块内，不切换页面。\n拖动顶部标题栏移动窗口，拖动边缘缩放。\n双击托盘图标重新打开。\n参数自动保存；复制热键需点击保存。\n颜色跟随游戏玩家；独立数值配色优先。",
			ForeColor = Palette46.Muted,
			Font = Interface45.Font(9f)
		}, 110);
	}

	private void RecoverMini45()
	{
		miniOverlay.Recover();
		status.Text = "小地图与战术地图正在重新连接。";
	}

	private void UpdateConnection45()
	{
		SyncConsole46();
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
			if (connection46 != null)
			{
				connection46.Text = (previewOnlyMode ? "◉ 外观预览" : (GameVersion45.Unverified ? "⚠ 版本未验证" : (liveRows ? "● 对局已连接" : "○ 等待对局")));
				tips46.SetToolTip(connection46, GameVersion45.Unverified ? "当前游戏版本兼容性未验证，允许继续测试；显示异常时请联系卖家适配。" : "");
			}
		}
		finally
		{
			sync46 = false;
		}
	}

	private void SyncArmour46()
	{
		if (armourList46 == null)
		{
			return;
		}
		armourSync46 = true;
		try
		{
			PlayerInfoRow[] array = (from x in rows
				where x.Relation == "敌方"
				orderby x.Slot
				select x).ToArray();
			bool flag = armourList46.Items.Count == array.Length;
			int num = 0;
			while (flag && num < array.Length)
			{
				flag = ((EnemyChoice)armourList46.Items[num]).Player.Slot == array[num].Slot && ((EnemyChoice)armourList46.Items[num]).Player.Name == array[num].Name;
				num++;
			}
			if (!flag)
			{
				armourList46.Items.Clear();
				PlayerInfoRow[] array2 = array;
				foreach (PlayerInfoRow playerInfoRow in array2)
				{
					armourList46.Items.Add(new EnemyChoice
					{
						Player = playerInfoRow
					}, armourSelection.Includes(playerInfoRow));
				}
			}
			else
			{
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					((EnemyChoice)armourList46.Items[num3]).Player = array[num3];
					armourList46.SetItemChecked(num3, armourSelection.Includes(array[num3]));
				}
			}
		}
		finally
		{
			armourSync46 = false;
		}
	}

	private bool HitTest46(ref Message m)
	{
		if (m.Msg != 132 || base.WindowState == FormWindowState.Maximized)
		{
			return false;
		}
		Point point = PointToClient(new Point((short)(m.LParam.ToInt64() & 0xFFFF), (short)((m.LParam.ToInt64() >> 16) & 0xFFFF)));
		bool flag = point.X < 7;
		bool flag2 = point.X > base.Width - 8;
		bool flag3 = point.Y < 7;
		bool flag4 = point.Y > base.Height - 8;
		int num = ((!flag) ? ((!flag2) ? (flag3 ? 12 : (flag4 ? 15 : 0)) : (flag3 ? 14 : (flag4 ? 17 : 11))) : (flag3 ? 13 : (flag4 ? 16 : 10)));
		if (num == 0)
		{
			return false;
		}
		m.Result = new IntPtr(num);
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			supportTimer49.Stop();
			supportTimer49.Dispose();
			DisposeFlank47();
		}
		if (disposing && tips46 != null)
		{
			tips46.Dispose();
			tips46 = null;
		}
		base.Dispose(disposing);
	}

	private void InitSupport49()
	{
		supportTimer49.Interval = 150;
		supportTimer49.Tick += delegate
		{
			PollSupport49();
		};
		if (!previewOnlyMode && LicenseGate60.Allowed)
		{
			supportTimer49.Start();
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
				supportActive49.Clear();
				supportBaseline49 = false;
			}
			List<SupportCommand49> list = reader.ReadSupport49(rows);
			foreach (SupportCommand49 item in list)
			{
				if (!supportSeen49.Add(item.Key))
				{
					continue;
				}
				if (item.Flare)
				{
					if (supportBaseline49)
					{
						sounds.Play(AlertKind.EnemySldFlare);
						status.Text = "敌方" + (item.Slot + 1) + "使用了SLD星爆照明弹";
					}
				}
				else
				{
					item.Expires = DateTime.UtcNow.AddSeconds(SupportSchedule49.Seconds(item.Path));
					supportActive49[item.Key] = item;
				}
			}
			supportBaseline49 = true;
			string[] array = (from x in supportActive49
				where x.Value.Expires < DateTime.UtcNow
				select x.Key).ToArray();
			foreach (string key in array)
			{
				supportActive49.Remove(key);
			}
		}
		catch
		{
		}
	}

	private void MergeSupport49()
	{
		PollSupport49();
		foreach (PlayerInfoRow row in rows)
		{
			row.Barrages.RemoveAll((BarrageRecord b) => b.Key != null && b.Key.StartsWith("support49:"));
			foreach (SupportCommand49 item in supportActive49.Values.Where((SupportCommand49 c) => c.Slot == row.Slot))
			{
				row.Barrages.Add(new BarrageRecord
				{
					Key = "support49:" + item.Key,
					CastKey = item.Key,
					Name = item.Name,
					Player = row.Name,
					Ability = item.Path,
					Slot = row.Slot,
					X = item.X,
					Y = item.Y,
					Z = item.Z,
					Colour = row.GameColour
				});
			}
		}
	}
}
