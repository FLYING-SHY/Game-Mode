using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class OverlayPreferences
{
	public readonly TacticalMapPreferences51 Tactical51 = new TacticalMapPreferences51();

	public readonly MinimapPreferences Minimap = new MinimapPreferences();

	public readonly SquadMarkerPreferences SquadMarkers = new SquadMarkerPreferences();

	public readonly ResourceColours Resources = new ResourceColours();

	public bool UseGameColours = true;

	public bool ShowMines = true;

	public Color EnemyMineColor = Color.Red;

	public Color FriendlyMineColor = Color.Lime;

	public int BarrageMarkerSize = 44;

	public bool ShowEnemyBarrages = true;

	public bool ShowUnits = true;

	public bool ShowSkills = true;

	public bool ShowAllPlayers;

	public int DetailSize = 14;

	public int FontSize = 14;

	public int Top = 8;

	public int WidthPercent = 94;

	public Color Color = Color.FromArgb(255, 225, 95);

	public Color SkillColor = Color.FromArgb(150, 220, 255);

	public bool Enabled = true;

	public readonly bool[] Info49 = Enumerable.Repeat(element: true, 10).ToArray();

	public static readonly string[] InfoKeys49 = new string[10] { "manpower", "munition", "fuel", "population", "cp", "group", "buildings", "production", "grenade", "kwk38" };

	private static string PathName => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "overlay-settings.txt");

	public static OverlayPreferences Load(string path = null)
	{
		OverlayPreferences overlayPreferences = new OverlayPreferences();
		try
		{
			string[] array = File.ReadAllLines(path ?? PathName);
			foreach (string text in array)
			{
				if (overlayPreferences.Tactical51.Read(text) || overlayPreferences.ReadInfo49(text) || overlayPreferences.Resources.Read(text) || overlayPreferences.SquadMarkers.Read(text) || overlayPreferences.Minimap.Read(text))
				{
					continue;
				}
				string[] array2 = text.Split('=');
				if (array2.Length == 2 && int.TryParse(array2[1], out var result))
				{
					switch (array2[0])
					{
					case "barrageSize":
						overlayPreferences.BarrageMarkerSize = Math.Max(8, Math.Min(240, result));
						break;
					case "enemyBarrages":
						overlayPreferences.ShowEnemyBarrages = result != 0;
						break;
					case "gameColours":
						overlayPreferences.UseGameColours = result != 0;
						break;
					case "mines":
						overlayPreferences.ShowMines = result != 0;
						break;
					case "enemyMineColor":
						overlayPreferences.EnemyMineColor = Color.FromArgb(255, Color.FromArgb(result));
						break;
					case "friendlyMineColor":
						overlayPreferences.FriendlyMineColor = Color.FromArgb(255, Color.FromArgb(result));
						break;
					case "units":
						overlayPreferences.ShowUnits = result != 0;
						break;
					case "skills":
						overlayPreferences.ShowSkills = result != 0;
						break;
					case "allPlayers":
						overlayPreferences.ShowAllPlayers = result != 0;
						break;
					case "detailSize":
						overlayPreferences.DetailSize = Math.Max(9, Math.Min(36, result));
						break;
					case "font":
						overlayPreferences.FontSize = Math.Max(9, Math.Min(36, result));
						break;
					case "top":
						overlayPreferences.Top = Math.Max(0, Math.Min(300, result));
						break;
					case "width":
						overlayPreferences.WidthPercent = Math.Max(40, Math.Min(100, result));
						break;
					case "color":
						overlayPreferences.Color = Color.FromArgb(255, Color.FromArgb(result));
						break;
					case "skillColor":
						overlayPreferences.SkillColor = Color.FromArgb(255, Color.FromArgb(result));
						break;
					case "enabled":
						overlayPreferences.Enabled = result != 0;
						break;
					}
				}
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		return overlayPreferences;
	}

	public void Save(string path = null)
	{
		File.WriteAllLines(path ?? PathName, new string[16]
		{
			"barrageSize=" + BarrageMarkerSize,
			"enemyBarrages=" + (ShowEnemyBarrages ? 1 : 0),
			"gameColours=" + (UseGameColours ? 1 : 0),
			"mines=" + (ShowMines ? 1 : 0),
			"enemyMineColor=" + EnemyMineColor.ToArgb(),
			"friendlyMineColor=" + FriendlyMineColor.ToArgb(),
			"units=" + (ShowUnits ? 1 : 0),
			"skills=" + (ShowSkills ? 1 : 0),
			"allPlayers=" + (ShowAllPlayers ? 1 : 0),
			"detailSize=" + DetailSize,
			"font=" + FontSize,
			"top=" + Top,
			"width=" + WidthPercent,
			"color=" + Color.ToArgb(),
			"skillColor=" + SkillColor.ToArgb(),
			"enabled=" + (Enabled ? 1 : 0)
		}.Concat(SaveInfo49()).Concat(Resources.Serialize()).Concat(SquadMarkers.Serialize())
			.Concat(Minimap.Serialize())
			.Concat(Tactical51.Serialize()));
	}

	public bool ReadInfo49(string line)
	{
		if (!line.StartsWith("info."))
		{
			return false;
		}
		string[] array = line.Split('=');
		if (array.Length != 2)
		{
			return true;
		}
		int num = Array.IndexOf(InfoKeys49, array[0].Substring(5));
		if (num >= 0 && (array[1] == "0" || array[1] == "1"))
		{
			Info49[num] = array[1] == "1";
		}
		return true;
	}

	public IEnumerable<string> SaveInfo49()
	{
		return InfoKeys49.Select((string key, int i) => "info." + key + "=" + (Info49[i] ? 1 : 0));
	}
}
