using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class PlayerInfoProgram
{
	[STAThread]
	public static int Main(string[] args)
	{
		string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		if (args.Contains("--verify"))
		{
			try
			{
				using (PlayerInfoReader playerInfoReader = new PlayerInfoReader())
				{
					List<PlayerInfoRow> list = playerInfoReader.Snapshot();
					List<string> list2 = new List<string>();
					list2.Add("PASS: read-only handle 0x410; supported hash; named resource registry; " + list.Count + " players.");
					list2.Add("No game memory writes or remote calls.");
					List<string> list3 = list2;
					foreach (PlayerInfoRow item in list)
					{
						list3.Add(item.Relation + " slot=" + item.Slot + " team=" + item.Team + " population=" + item.Population + " buildings=" + BuildingNames.Summary(item) + " artillery=" + item.Artillery.Count + " MP=" + item.Manpower + " fuel=" + item.Fuel + " munition=" + item.Munition + " CP=" + item.CP + " group=" + ChineseNames.Group(item) + " upgrades=" + item.Upgrades.Count);
					}
					File.WriteAllLines(Path.Combine(baseDirectory, "player-info-verification.txt"), list3);
				}
				return 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(baseDirectory, "player-info-verification.txt"), "FAIL: " + ex);
				return 1;
			}
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		if (args.Contains("--preview"))
		{
			using (PlayerInfoForm playerInfoForm = new PlayerInfoForm())
			{
				playerInfoForm.ShowInTaskbar = false;
				playerInfoForm.Opacity = 0.0;
				playerInfoForm.Show();
				Application.DoEvents();
				using (Bitmap bitmap = new Bitmap(playerInfoForm.Width, playerInfoForm.Height))
				{
					playerInfoForm.DrawToBitmap(bitmap, new Rectangle(0, 0, playerInfoForm.Width, playerInfoForm.Height));
					bitmap.Save(Path.Combine(baseDirectory, "player-info-preview.png"));
				}
				playerInfoForm.Close();
			}
			return 0;
		}
		Application.Run(new PlayerInfoForm());
		return 0;
	}
}
