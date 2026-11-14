using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class OverlayProgram
{
	[DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();

	[STAThread]
	public static int Main(string[] args)
	{
		SetProcessDPIAware();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		if (args.Contains("--self-test"))
		{
			return SelfTest();
		}
		bool createdNew;
		using (new Mutex(initiallyOwned: true, "Local\\SkyeyeSelfHostedOverlay", out createdNew))
		{
			if (!createdNew)
			{
				MessageBox.Show("悬浮信息已经运行，请双击托盘里的信息图标打开设置。");
				return 0;
			}
			Application.Run(new OverlaySettingsForm());
		}
		return 0;
	}

	private static int SelfTest()
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		List<string> list = new List<string>();
		try
		{
			PlayerInfoRow playerInfoRow = new PlayerInfoRow();
			playerInfoRow.Name = "敌方玩家";
			playerInfoRow.Relation = "敌方";
			playerInfoRow.Group = "空军（luftwaffe）";
			playerInfoRow.Manpower = 450.9f;
			playerInfoRow.Munition = 120.2f;
			playerInfoRow.Fuel = 80.8f;
			playerInfoRow.Population = 52f;
			playerInfoRow.Race = "german";
			playerInfoRow.BuildingsKnown = true;
			PlayerInfoRow playerInfoRow2 = playerInfoRow;
			playerInfoRow2.Buildings.Add(new TrackedBuilding
			{
				Tier = 1,
				Key = "base",
				Complete = true
			});
			playerInfoRow2.Buildings.Add(new TrackedBuilding
			{
				Tier = 2,
				Key = "building",
				Complete = false
			});
			playerInfoRow2.Upgrades.AddRange(new string[3] { "upgrade\\german\\battlegroups\\luftwaffe\\luftwaffe", "upgrade\\german\\battlegroups\\luftwaffe\\luftwaffe_left_1b_fallschirmpioneers", "upgrade\\german\\battlegroups\\luftwaffe\\luftwaffe_right_1b_strafing_run_ger" });
			string[] lines = OverlayText.Lines(new PlayerInfoRow[2]
			{
				playerInfoRow2,
				new PlayerInfoRow
				{
					Name = "不应显示",
					Relation = "队友"
				}
			});
			if (lines.Length != 2 || !lines[0].Contains("人 450  弹 120  油 80") || Regex.IsMatch(lines[1], "[a-zA-Z_]") || lines[1].Contains("待识别") || lines.Any((string x) => x.Contains("不应显示")))
			{
				throw new Exception("Chinese formatting/filter mismatch");
			}
			PlayerInfoRow playerInfoRow3 = new PlayerInfoRow();
			playerInfoRow3.Slot = 9u;
			playerInfoRow3.Name = playerInfoRow2.Name;
			playerInfoRow3.Relation = "敌方";
			PlayerInfoRow playerInfoRow4 = playerInfoRow3;
			PlayerInfoRow playerInfoRow5 = new PlayerInfoRow();
			playerInfoRow5.Slot = 10u;
			playerInfoRow5.Name = "队友";
			playerInfoRow5.Relation = "队友";
			PlayerInfoRow playerInfoRow6 = playerInfoRow5;
			PlayerInfoRow[] rows = new PlayerInfoRow[3] { playerInfoRow2, playerInfoRow4, playerInfoRow6 };
			EnemySelection enemySelection = new EnemySelection();
			enemySelection.UpdateMatch("match-one");
			if (enemySelection.Filter(rows).Count() != 2)
			{
				throw new Exception("New match must default to all enemies");
			}
			enemySelection.Set(playerInfoRow2, enabled: false);
			playerInfoRow2.Manpower += 10f;
			enemySelection.UpdateMatch("match-one");
			if (enemySelection.Filter(rows).Single() != playerInfoRow4)
			{
				throw new Exception("Same-name player selection or refresh persistence failed");
			}
			enemySelection.Set(playerInfoRow4, enabled: false);
			if (OverlayText.Lines(enemySelection.Filter(rows)).Length != 0)
			{
				throw new Exception("Select-none failed");
			}
			enemySelection.UpdateMatch("match-two");
			if (enemySelection.Filter(rows).Count() != 2)
			{
				throw new Exception("New match selection reset failed");
			}
			list.Add("PASS select subset, duplicate nicknames, refresh persistence, select none, ally exclusion and new-match reset.");
			list.Add("PASS enemy-only filtering, resource order and truncation to integer, group root exclusion, compact branch labels.");
			AlertTests.Run(list);
			string path = Path.Combine(Path.GetTempPath(), "coh3-overlay-settings-" + Guid.NewGuid().ToString("N") + ".txt");
			try
			{
				OverlayPreferences overlayPreferences = new OverlayPreferences();
				overlayPreferences.FontSize = 24;
				overlayPreferences.Top = 60;
				overlayPreferences.WidthPercent = 80;
				overlayPreferences.Color = Color.Cyan;
				overlayPreferences.SkillColor = Color.Lime;
				overlayPreferences.Enabled = false;
				OverlayPreferences overlayPreferences2 = overlayPreferences;
				overlayPreferences2.Save(path);
				OverlayPreferences overlayPreferences3 = OverlayPreferences.Load(path);
				if (overlayPreferences3.FontSize != 24 || overlayPreferences3.Top != 60 || overlayPreferences3.WidthPercent != 80 || overlayPreferences3.Color.ToArgb() != Color.Cyan.ToArgb() || overlayPreferences3.SkillColor.ToArgb() != Color.Lime.ToArgb() || overlayPreferences3.Enabled)
				{
					throw new Exception("Settings round-trip mismatch");
				}
				list.Add("PASS persisted font size, color, top offset, width and enabled state; existing user settings untouched.");
			}
			finally
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			using (EnemyOverlayWindow enemyOverlayWindow = new EnemyOverlayWindow(new OverlayPreferences(), track: false))
			{
				int windowStyleForTest = enemyOverlayWindow.WindowStyleForTest;
				int num = 134742176;
				if ((windowStyleForTest & num) != num)
				{
					throw new Exception("Missing overlay styles");
				}
				enemyOverlayWindow.ClearData();
				if (enemyOverlayWindow.Visible)
				{
					throw new Exception("Stale data visible");
				}
				list.Add("PASS native layered + transparent + no-activate + tool-window flags; clear-data hides overlay.");
			}
			int[] array = new int[2] { 14, 24 };
			foreach (int num3 in array)
			{
				using Bitmap bitmap = new Bitmap(1800, 780);
				using Graphics graphics = Graphics.FromImage(bitmap);
				using Font font = new Font("Microsoft YaHei UI", num3, FontStyle.Bold);
				graphics.Clear(Color.FromArgb(38, 45, 43));
				string[] lines2 = Enumerable.Range(0, 4).SelectMany((int i) => new string[2]
				{
					lines[0].Replace("敌方玩家", "敌方 " + (i + 1)),
					lines[1] + " · 空降猎兵 · 破片炸弹"
				}).ToArray();
				int num4 = OverlayDrawing.Draw(graphics, lines2, font, Color.FromArgb(255, 225, 95), Color.FromArgb(150, 220, 255), 1800, paint: true);
				if (num4 > 780)
				{
					throw new Exception("Four-player layout overflow at " + num3);
				}
				bitmap.Save(Path.Combine(baseDirectory, "overlay10-preview-" + num3 + ".png"));
				list.Add("PASS 4-enemy layout at " + num3 + "pt; height=" + num4);
			}
			using (OverlaySettingsForm overlaySettingsForm = new OverlaySettingsForm(previewOnly: true))
			{
				using Bitmap bitmap2 = new Bitmap(overlaySettingsForm.Width, overlaySettingsForm.Height);
				overlaySettingsForm.ShowInTaskbar = false;
				overlaySettingsForm.Opacity = 0.0;
				overlaySettingsForm.Show();
				Application.DoEvents();
				overlaySettingsForm.DrawToBitmap(bitmap2, new Rectangle(0, 0, bitmap2.Width, bitmap2.Height));
				bitmap2.Save(Path.Combine(baseDirectory, "overlay10-settings-preview.png"));
				overlaySettingsForm.Close();
			}
			using (AlertSounds sounds = new AlertSounds())
			{
				using SoundSettingsForm soundSettingsForm = new SoundSettingsForm(sounds);
				using Bitmap bitmap3 = new Bitmap(soundSettingsForm.Width, soundSettingsForm.Height);
				soundSettingsForm.ShowInTaskbar = false;
				soundSettingsForm.Opacity = 0.0;
				soundSettingsForm.Show();
				Application.DoEvents();
				soundSettingsForm.DrawToBitmap(bitmap3, new Rectangle(0, 0, bitmap3.Width, bitmap3.Height));
				bitmap3.Save(Path.Combine(baseDirectory, "overlay10-sounds-preview.png"));
				soundSettingsForm.Close();
			}
			try
			{
				using PlayerInfoReader playerInfoReader = new PlayerInfoReader();
				List<PlayerInfoRow> list2 = playerInfoReader.Snapshot();
				list.Add("LIVE READ PASS: " + list2.Count + " players, " + list2.Count((PlayerInfoRow x) => x.Relation == "敌方") + " enemies; no writes.");
			}
			catch (Exception ex)
			{
				list.Add("LIVE READ UNAVAILABLE: " + ex.Message);
			}
			File.WriteAllLines(Path.Combine(baseDirectory, "overlay10-verification.txt"), list);
			return 0;
		}
		catch (Exception ex2)
		{
			list.Add("FAIL: " + ex2);
			File.WriteAllLines(Path.Combine(baseDirectory, "overlay10-verification.txt"), list);
			return 1;
		}
	}
}
