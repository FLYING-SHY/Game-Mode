using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>覆盖层设置对话框。</summary>
internal sealed class SettingsForm : Form
{
	private readonly EspSettings settings;

	private readonly CheckBox onlyEnemies;

	private readonly CheckBox onlyAlive;

	private readonly CheckBox showHealthBar;

	private readonly CheckBox showDistance;

	private readonly CheckBox showName;

	private readonly CheckBox showSnapLine;

	private readonly CheckBox showFovCircle;

	private readonly CheckBox showOffscreenArrow;

	private readonly ComboBox boxStyle;

	private readonly ComboBox hotkey;

	private readonly TrackBar thickness;

	private readonly TrackBar maxDistance;

	private readonly TrackBar fovRadius;

	private readonly Label thicknessLabel;

	private readonly Label maxDistanceLabel;

	private readonly Label fovRadiusLabel;

	private readonly Button enemyColor;

	private readonly Button friendlyColor;

	private readonly Button neutralColor;

	private readonly Button textColor;

	private readonly Button snapLineColor;

	/// <summary>确定后的设置副本。</summary>
	public EspSettings Result { get; private set; }

	public SettingsForm(EspSettings source)
	{
		settings = (source == null) ? new EspSettings() : source.Clone();
		Result = settings.Clone();
		Text = "Bf6Esp 设置";
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;
		ClientSize = new Size(470, 470);
		Font = new Font("Microsoft YaHei UI", 9f);

		GroupBox groupDisplay = new GroupBox
		{
			Text = "显示选项",
			Location = new Point(12, 10),
			Size = new Size(446, 150)
		};
		onlyEnemies = MakeCheck(groupDisplay, "仅显示敌人", 14, 24);
		onlyAlive = MakeCheck(groupDisplay, "仅显示存活目标", 150, 24);
		showHealthBar = MakeCheck(groupDisplay, "显示血条", 300, 24);
		showDistance = MakeCheck(groupDisplay, "显示距离", 14, 54);
		showName = MakeCheck(groupDisplay, "显示名字", 150, 54);
		showSnapLine = MakeCheck(groupDisplay, "显示下拉线", 300, 54);
		showFovCircle = MakeCheck(groupDisplay, "显示 FOV 圈", 14, 84);
		showOffscreenArrow = MakeCheck(groupDisplay, "显示离屏箭头", 150, 84);
		Controls.Add(groupDisplay);

		GroupBox groupParams = new GroupBox
		{
			Text = "参数",
			Location = new Point(12, 168),
			Size = new Size(446, 150)
		};
		Label styleLabel = new Label
		{
			Text = "方框样式",
			Location = new Point(16, 28),
			AutoSize = true
		};
		boxStyle = new ComboBox
		{
			Location = new Point(96, 24),
			Size = new Size(120, 24),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		boxStyle.Items.Add("完整矩形");
		boxStyle.Items.Add("四角折线");
		groupParams.Controls.Add(styleLabel);
		groupParams.Controls.Add(boxStyle);

		thicknessLabel = new Label
		{
			Location = new Point(240, 28),
			AutoSize = true
		};
		thickness = new TrackBar
		{
			Location = new Point(16, 58),
			Size = new Size(300, 40),
			Minimum = 1,
			Maximum = 8,
			TickStyle = TickStyle.None
		};
		groupParams.Controls.Add(thickness);
		groupParams.Controls.Add(new Label { Text = "框线粗细", Location = new Point(16, 44), AutoSize = true });
		groupParams.Controls.Add(thicknessLabel);

		maxDistanceLabel = new Label
		{
			Location = new Point(360, 28),
			AutoSize = true
		};
		maxDistance = new TrackBar
		{
			Location = new Point(16, 108),
			Size = new Size(300, 40),
			Minimum = 10,
			Maximum = 2000,
			TickStyle = TickStyle.None
		};
		groupParams.Controls.Add(maxDistance);
		groupParams.Controls.Add(new Label { Text = "最大距离（米）", Location = new Point(16, 94), AutoSize = true });
		groupParams.Controls.Add(maxDistanceLabel);

		fovRadiusLabel = new Label
		{
			Location = new Point(360, 62),
			AutoSize = true
		};
		fovRadius = new TrackBar
		{
			Location = new Point(330, 80),
			Size = new Size(110, 40),
			Minimum = 20,
			Maximum = 600,
			TickStyle = TickStyle.None
		};
		groupParams.Controls.Add(fovRadius);
		groupParams.Controls.Add(fovRadiusLabel);
		Controls.Add(groupParams);

		GroupBox groupColors = new GroupBox
		{
			Text = "颜色",
			Location = new Point(12, 326),
			Size = new Size(446, 96)
		};
		enemyColor = MakeColorButton(groupColors, "敌人", 14, 24);
		friendlyColor = MakeColorButton(groupColors, "友方", 100, 24);
		neutralColor = MakeColorButton(groupColors, "中立", 186, 24);
		textColor = MakeColorButton(groupColors, "文字", 272, 24);
		snapLineColor = MakeColorButton(groupColors, "下拉线", 14, 54);
		Controls.Add(groupColors);

		GroupBox groupHotkey = new GroupBox
		{
			Text = "全局热键（切换覆盖层）",
			Location = new Point(12, 430),
			Size = new Size(300, 32)
		};
		hotkey = new ComboBox
		{
			Location = new Point(120, 18),
			Size = new Size(160, 24),
			DropDownStyle = ComboBoxStyle.DropDownList
		};
		hotkey.Items.AddRange(new object[] { "End", "F1", "Insert", "Home", "Pause", "Delete" });
		groupHotkey.Controls.Add(hotkey);
		Controls.Add(groupHotkey);

		Button ok = new Button
		{
			Text = "保存",
			Location = new Point(330, 430),
			Size = new Size(124, 30)
		};
		ok.Click += delegate
		{
			Collect();
			Result = settings.Clone();
			DialogResult = DialogResult.OK;
			Close();
		};
		Button cancel = new Button
		{
			Text = "取消",
			Location = new Point(330, 464),
			Size = new Size(124, 30)
		};
		cancel.Click += delegate
		{
			DialogResult = DialogResult.Cancel;
			Close();
		};
		Button reset = new Button
		{
			Text = "恢复默认",
			Location = new Point(12, 464),
			Size = new Size(100, 30)
		};
		reset.Click += delegate
		{
			EspSettings defaults = new EspSettings();
			settings.OverlayEnabled = defaults.OverlayEnabled;
			settings.BoxStyle = defaults.BoxStyle;
			settings.BoxThickness = defaults.BoxThickness;
			settings.MaxDistance = defaults.MaxDistance;
			settings.OnlyEnemies = defaults.OnlyEnemies;
			settings.OnlyAlive = defaults.OnlyAlive;
			settings.ShowHealthBar = defaults.ShowHealthBar;
			settings.ShowDistance = defaults.ShowDistance;
			settings.ShowName = defaults.ShowName;
			settings.ShowSnapLine = defaults.ShowSnapLine;
			settings.ShowFovCircle = defaults.ShowFovCircle;
			settings.FovRadius = defaults.FovRadius;
			settings.ShowOffscreenArrow = defaults.ShowOffscreenArrow;
			settings.EnemyColor = defaults.EnemyColor;
			settings.FriendlyColor = defaults.FriendlyColor;
			settings.NeutralColor = defaults.NeutralColor;
			settings.TextColor = defaults.TextColor;
			settings.SnapLineColor = defaults.SnapLineColor;
			settings.HealthHighColor = defaults.HealthHighColor;
			settings.HealthMidColor = defaults.HealthMidColor;
			settings.HealthLowColor = defaults.HealthLowColor;
			settings.HotkeyName = defaults.HotkeyName;
			Apply();
		};
		Controls.Add(ok);
		Controls.Add(cancel);
		Controls.Add(reset);
		ClientSize = new Size(470, 506);

		thickness.ValueChanged += delegate { UpdateLabels(); };
		maxDistance.ValueChanged += delegate { UpdateLabels(); };
		fovRadius.ValueChanged += delegate { UpdateLabels(); };
		enemyColor.Click += delegate
		{
			Color picked;
			if (PickColor(enemyColor, settings.EnemyColor, out picked))
			{
				settings.EnemyColor = picked;
			}
		};
		friendlyColor.Click += delegate
		{
			Color picked;
			if (PickColor(friendlyColor, settings.FriendlyColor, out picked))
			{
				settings.FriendlyColor = picked;
			}
		};
		neutralColor.Click += delegate
		{
			Color picked;
			if (PickColor(neutralColor, settings.NeutralColor, out picked))
			{
				settings.NeutralColor = picked;
			}
		};
		textColor.Click += delegate
		{
			Color picked;
			if (PickColor(textColor, settings.TextColor, out picked))
			{
				settings.TextColor = picked;
			}
		};
		snapLineColor.Click += delegate
		{
			Color picked;
			if (PickColor(snapLineColor, settings.SnapLineColor, out picked))
			{
				settings.SnapLineColor = picked;
			}
		};

		Apply();
	}

	private static CheckBox MakeCheck(Control parent, string text, int x, int y)
	{
		CheckBox box = new CheckBox
		{
			Text = text,
			Location = new Point(x, y),
			AutoSize = true
		};
		parent.Controls.Add(box);
		return box;
	}

	private static Button MakeColorButton(Control parent, string text, int x, int y)
	{
		Button button = new Button
		{
			Text = text,
			Location = new Point(x, y),
			Size = new Size(78, 26),
			UseVisualStyleBackColor = false
		};
		parent.Controls.Add(button);
		return button;
	}

	private void Apply()
	{
		onlyEnemies.Checked = settings.OnlyEnemies;
		onlyAlive.Checked = settings.OnlyAlive;
		showHealthBar.Checked = settings.ShowHealthBar;
		showDistance.Checked = settings.ShowDistance;
		showName.Checked = settings.ShowName;
		showSnapLine.Checked = settings.ShowSnapLine;
		showFovCircle.Checked = settings.ShowFovCircle;
		showOffscreenArrow.Checked = settings.ShowOffscreenArrow;
		boxStyle.SelectedIndex = (settings.BoxStyle == "corner") ? 1 : 0;
		if (boxStyle.SelectedIndex < 0)
		{
			boxStyle.SelectedIndex = 0;
		}
		int hotkeyIndex = hotkey.Items.IndexOf(settings.HotkeyName);
		if (hotkeyIndex < 0)
		{
			hotkeyIndex = 0;
		}
		hotkey.SelectedIndex = hotkeyIndex;
		thickness.Value = Clamp(settings.BoxThickness, thickness.Minimum, thickness.Maximum);
		maxDistance.Value = Clamp((int)Math.Round(settings.MaxDistance), maxDistance.Minimum, maxDistance.Maximum);
		fovRadius.Value = Clamp(settings.FovRadius, fovRadius.Minimum, fovRadius.Maximum);
		enemyColor.BackColor = settings.EnemyColor;
		friendlyColor.BackColor = settings.FriendlyColor;
		neutralColor.BackColor = settings.NeutralColor;
		textColor.BackColor = settings.TextColor;
		snapLineColor.BackColor = settings.SnapLineColor;
		UpdateLabels();
	}

	private void UpdateLabels()
	{
		thicknessLabel.Text = "粗细 " + thickness.Value;
		maxDistanceLabel.Text = "最大 " + maxDistance.Value + " m";
		fovRadiusLabel.Text = "半径 " + fovRadius.Value + " px";
	}

	private void Collect()
	{
		settings.OnlyEnemies = onlyEnemies.Checked;
		settings.OnlyAlive = onlyAlive.Checked;
		settings.ShowHealthBar = showHealthBar.Checked;
		settings.ShowDistance = showDistance.Checked;
		settings.ShowName = showName.Checked;
		settings.ShowSnapLine = showSnapLine.Checked;
		settings.ShowFovCircle = showFovCircle.Checked;
		settings.ShowOffscreenArrow = showOffscreenArrow.Checked;
		settings.BoxStyle = (boxStyle.SelectedIndex == 1) ? "corner" : "full";
		settings.BoxThickness = thickness.Value;
		settings.MaxDistance = maxDistance.Value;
		settings.FovRadius = fovRadius.Value;
		settings.HotkeyName = Convert.ToString(hotkey.SelectedItem, CultureInfo.InvariantCulture);
	}

	/// <summary>弹出取色器；返回 false 表示用户取消。</summary>
	private static bool PickColor(Button button, Color current, out Color picked)
	{
		picked = current;
		using (ColorDialog dialog = new ColorDialog())
		{
			dialog.Color = current;
			dialog.FullOpen = true;
			if (dialog.ShowDialog() != DialogResult.OK)
			{
				return false;
			}
			picked = dialog.Color;
			button.BackColor = picked;
			return true;
		}
	}

	private static int Clamp(int value, int min, int max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}
}
