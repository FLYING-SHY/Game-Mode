using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace Bf6Esp;

/// <summary>
/// 覆盖层设置。持久化在可执行文件同目录的 overlay-settings.txt（纯文本 key=value）。
/// </summary>
internal sealed class EspSettings
{
	public bool OverlayEnabled = true;

	/// <summary>方框样式：full = 完整矩形，corner = 四角折线。</summary>
	public string BoxStyle = "full";

	public int BoxThickness = 2;

	public float MaxDistance = 500f;

	public bool OnlyEnemies = true;

	public bool OnlyAlive = true;

	public bool ShowHealthBar = true;

	public bool ShowDistance = true;

	public bool ShowName = true;

	public bool ShowSnapLine = true;

	public bool ShowFovCircle;

	public int FovRadius = 160;

	public bool ShowOffscreenArrow = true;

	public Color EnemyColor = Color.FromArgb(255, 230, 70, 70);

	public Color FriendlyColor = Color.FromArgb(255, 80, 210, 130);

	public Color NeutralColor = Color.FromArgb(255, 220, 200, 90);

	public Color TextColor = Color.FromArgb(255, 255, 255, 255);

	public Color SnapLineColor = Color.FromArgb(140, 255, 255, 255);

	public Color HealthHighColor = Color.FromArgb(255, 70, 220, 90);

	public Color HealthMidColor = Color.FromArgb(255, 240, 200, 60);

	public Color HealthLowColor = Color.FromArgb(255, 235, 70, 70);

	/// <summary>全局热键名称（End / F1 / Insert / Home / Pause / Delete）。</summary>
	public string HotkeyName = "End";

	/// <summary>设置文件路径：可执行文件同目录 overlay-settings.txt。</summary>
	public static string DefaultPath
	{
		get
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			return Path.Combine(baseDirectory, "overlay-settings.txt");
		}
	}

	public EspSettings Clone()
	{
		return (EspSettings)MemberwiseClone();
	}

	/// <summary>读取设置；文件不存在时返回默认设置。</summary>
	public static EspSettings Load(string path)
	{
		EspSettings settings = new EspSettings();
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return settings;
		}
		string[] lines;
		try
		{
			lines = File.ReadAllLines(path, Encoding.UTF8);
		}
		catch (IOException)
		{
			return settings;
		}
		catch (UnauthorizedAccessException)
		{
			return settings;
		}
		foreach (string line in lines)
		{
			if (string.IsNullOrEmpty(line))
			{
				continue;
			}
			string trimmed = line.Trim();
			if (trimmed.Length == 0 || trimmed[0] == '#' || trimmed[0] == ';')
			{
				continue;
			}
			int split = trimmed.IndexOf('=');
			if (split <= 0)
			{
				continue;
			}
			string key = trimmed.Substring(0, split).Trim();
			string value = trimmed.Substring(split + 1).Trim();
			settings.Apply(key, value);
		}
		return settings;
	}

	private void Apply(string key, string value)
	{
		switch (key.ToLowerInvariant())
		{
			case "overlay_enabled":
				OverlayEnabled = ParseBool(value, OverlayEnabled);
				break;
			case "box_style":
				BoxStyle = (value == "corner") ? "corner" : "full";
				break;
			case "box_thickness":
				BoxThickness = Clamp(ParseInt(value, BoxThickness), 1, 8);
				break;
			case "max_distance":
				MaxDistance = (float)Math.Max(0.0, ParseDouble(value, MaxDistance));
				break;
			case "only_enemies":
				OnlyEnemies = ParseBool(value, OnlyEnemies);
				break;
			case "only_alive":
				OnlyAlive = ParseBool(value, OnlyAlive);
				break;
			case "show_health_bar":
				ShowHealthBar = ParseBool(value, ShowHealthBar);
				break;
			case "show_distance":
				ShowDistance = ParseBool(value, ShowDistance);
				break;
			case "show_name":
				ShowName = ParseBool(value, ShowName);
				break;
			case "show_snap_line":
				ShowSnapLine = ParseBool(value, ShowSnapLine);
				break;
			case "show_fov_circle":
				ShowFovCircle = ParseBool(value, ShowFovCircle);
				break;
			case "fov_radius":
				FovRadius = Clamp(ParseInt(value, FovRadius), 10, 2000);
				break;
			case "show_offscreen_arrow":
				ShowOffscreenArrow = ParseBool(value, ShowOffscreenArrow);
				break;
			case "enemy_color":
				EnemyColor = ParseColor(value, EnemyColor);
				break;
			case "friendly_color":
				FriendlyColor = ParseColor(value, FriendlyColor);
				break;
			case "neutral_color":
				NeutralColor = ParseColor(value, NeutralColor);
				break;
			case "text_color":
				TextColor = ParseColor(value, TextColor);
				break;
			case "snap_line_color":
				SnapLineColor = ParseColor(value, SnapLineColor);
				break;
			case "health_high_color":
				HealthHighColor = ParseColor(value, HealthHighColor);
				break;
			case "health_mid_color":
				HealthMidColor = ParseColor(value, HealthMidColor);
				break;
			case "health_low_color":
				HealthLowColor = ParseColor(value, HealthLowColor);
				break;
			case "hotkey":
				HotkeyName = value;
				break;
		}
	}

	/// <summary>写回设置文件。</summary>
	public void Save(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		List<string> lines = new List<string>
		{
			"# Bf6Esp 覆盖层设置（key=value，# 开头为注释）",
			"overlay_enabled=" + Bool(OverlayEnabled),
			"box_style=" + BoxStyle,
			"box_thickness=" + BoxThickness.ToString(CultureInfo.InvariantCulture),
			"max_distance=" + MaxDistance.ToString("0.###", CultureInfo.InvariantCulture),
			"only_enemies=" + Bool(OnlyEnemies),
			"only_alive=" + Bool(OnlyAlive),
			"show_health_bar=" + Bool(ShowHealthBar),
			"show_distance=" + Bool(ShowDistance),
			"show_name=" + Bool(ShowName),
			"show_snap_line=" + Bool(ShowSnapLine),
			"show_fov_circle=" + Bool(ShowFovCircle),
			"fov_radius=" + FovRadius.ToString(CultureInfo.InvariantCulture),
			"show_offscreen_arrow=" + Bool(ShowOffscreenArrow),
			"enemy_color=" + ColorText(EnemyColor),
			"friendly_color=" + ColorText(FriendlyColor),
			"neutral_color=" + ColorText(NeutralColor),
			"text_color=" + ColorText(TextColor),
			"snap_line_color=" + ColorText(SnapLineColor),
			"health_high_color=" + ColorText(HealthHighColor),
			"health_mid_color=" + ColorText(HealthMidColor),
			"health_low_color=" + ColorText(HealthLowColor),
			"hotkey=" + HotkeyName
		};
		try
		{
			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			File.WriteAllLines(path, lines.ToArray(), new UTF8Encoding(false));
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	private static string Bool(bool value)
	{
		return value ? "1" : "0";
	}

	private static string ColorText(Color color)
	{
		return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
	}

	private static bool ParseBool(string value, bool fallback)
	{
		string text = (value ?? "").Trim().ToLowerInvariant();
		if (text == "1" || text == "true" || text == "yes" || text == "on")
		{
			return true;
		}
		if (text == "0" || text == "false" || text == "no" || text == "off")
		{
			return false;
		}
		return fallback;
	}

	private static int ParseInt(string value, int fallback)
	{
		int result;
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
		{
			return result;
		}
		return fallback;
	}

	private static double ParseDouble(string value, double fallback)
	{
		double result;
		if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
		{
			return result;
		}
		return fallback;
	}

	private static Color ParseColor(string value, Color fallback)
	{
		try
		{
			string text = (value ?? "").Trim();
			if (text.Length == 0)
			{
				return fallback;
			}
			if (text[0] != '#')
			{
				text = "#" + text;
			}
			Color parsed = ColorTranslator.FromHtml(text);
			return parsed;
		}
		catch (ArgumentException)
		{
			return fallback;
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
