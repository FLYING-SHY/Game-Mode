using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class MinimapMarkerWindow : Form
{
	private struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private readonly bool tracking;

	private readonly MinimapPreferences prefs;

	private readonly Timer timer = new Timer();

	private readonly BarrageDisplayTracker barrageTracker = new BarrageDisplayTracker();

	private List<BarrageRecord> barrages = new List<BarrageRecord>();

	private PlayerInfoReader reader;

	private string match;

	private int pid;

	private DateTime updated;

	private SquadMarkerSource[] sources = new SquadMarkerSource[0];

	private List<SquadMarker> markers = new List<SquadMarker>();

	private MinimapFrame frame;

	private MinimapFrame tacticalFrame51;

	private readonly TacticalMapPreferences51 tactical51;

	private string lastMiniLog = "";

	private DateTime nextMiniLog;

	public Func<FlankMapRange55> Range55;

	public string LastError = "";

	protected override bool ShowWithoutActivation => true;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 134742176;
			return createParams;
		}
	}

	public int IntervalForTest => timer.Interval;

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr w, out Rect r);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr w, ref Point p);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr w);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr w, IntPtr after, int x, int y, int cx, int cy, uint flags);

	private void Trace(string state, string detail = "")
	{
		if (state == lastMiniLog && DateTime.UtcNow < nextMiniLog)
		{
			return;
		}
		lastMiniLog = state;
		nextMiniLog = DateTime.UtcNow.AddSeconds(10.0);
		try
		{
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "minimap-diagnostics.log");
			if (File.Exists(text) && new FileInfo(text).Length > 262144)
			{
				File.WriteAllText(text, "");
			}
			File.AppendAllText(text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + state + " " + detail + Environment.NewLine);
		}
		catch
		{
		}
	}

	public MinimapMarkerWindow(MinimapPreferences p, bool track = true, TacticalMapPreferences51 tactical = null)
	{
		tracking = track;
		prefs = p;
		tactical51 = tactical ?? new TacticalMapPreferences51
		{
			Enabled = false
		};
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.Manual;
		base.AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(1, 2, 3);
		base.TransparencyKey = BackColor;
		DoubleBuffered = true;
		ApplyAppearance();
		timer.Tick += delegate
		{
			Frame();
		};
		if (track)
		{
			timer.Start();
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 132)
		{
			m.Result = new IntPtr(-1);
		}
		else if (m.Msg == 33)
		{
			m.Result = new IntPtr(3);
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	public void Recover()
	{
		frame = null;
		tacticalFrame51 = null;
		markers.Clear();
		if (reader != null)
		{
			reader.RecoverMinimap();
		}
		Trace("手动重连");
		if (tracking)
		{
			timer.Start();
		}
		Hide();
		LastError = "正在重新连接小地图…";
	}

	public void ApplyAppearance()
	{
		if (tracking && !timer.Enabled)
		{
			timer.Start();
		}
		timer.Interval = Math.Max(1, 1000 / prefs.RefreshRate);
		if (!prefs.Enabled && !tactical51.Enabled)
		{
			Hide();
		}
		Invalidate();
	}

	public void UpdateRows(PlayerInfoReader r, int gamePid, IEnumerable<PlayerInfoRow> rows)
	{
		if (reader != r || match != r.MatchKey)
		{
			frame = null;
			tacticalFrame51 = null;
			markers.Clear();
			barrages.Clear();
			barrageTracker.Clear();
		}
		reader = r;
		match = r.MatchKey;
		pid = gamePid;
		PlayerInfoRow[] array = rows.Where((PlayerInfoRow p) => p.Relation == "敌方").ToArray();
		PlayerInfoRow[] array2 = array;
		foreach (PlayerInfoRow playerInfoRow in array2)
		{
			foreach (BarrageRecord barrage in playerInfoRow.Barrages)
			{
				barrage.Colour = (playerInfoRow.GameColourKnown ? playerInfoRow.GameColour : Color.Red);
			}
		}
		barrages = barrageTracker.Update(match, array.SelectMany((PlayerInfoRow p) => p.Barrages), DateTime.UtcNow, from p in array
			where !p.BarragesKnown
			select p.Slot);
		PlayerInfoRow[] array3 = array.Where((PlayerInfoRow p) => p.MarkerDataKnown).ToArray();
		PlayerInfoRow[] array4 = array3;
		foreach (PlayerInfoRow playerInfoRow2 in array4)
		{
			foreach (SquadMarkerSource markerSquad in playerInfoRow2.MarkerSquads)
			{
				markerSquad.ColourKnown = playerInfoRow2.GameColourKnown;
				markerSquad.Colour = playerInfoRow2.GameColour;
			}
		}
		sources = array3.SelectMany((PlayerInfoRow p) => p.MarkerSquads).ToArray();
		updated = DateTime.UtcNow;
	}

	public void ClearData()
	{
		reader = null;
		frame = null;
		tacticalFrame51 = null;
		sources = new SquadMarkerSource[0];
		markers.Clear();
		barrages.Clear();
		barrageTracker.Clear();
		LastError = "";
		Hide();
	}

	private void Frame()
	{
		if (!LicenseGate60.Allowed || (!prefs.Enabled && !tactical51.Enabled) || reader == null || DateTime.UtcNow - updated > TimeSpan.FromSeconds(3.0))
		{
			Hide();
			return;
		}
		IntPtr foregroundWindow = Native.GetForegroundWindow();
		Native.GetWindowThreadProcessId(foregroundWindow, out var num);
		if (num != pid || IsIconic(foregroundWindow))
		{
			Hide();
			return;
		}
		Point p = default(Point);
		if (!GetClientRect(foregroundWindow, out var r) || r.Right < 200 || r.Bottom < 100 || !ClientToScreen(foregroundWindow, ref p))
		{
			Hide();
			return;
		}
		try
		{
			frame = null;
			tacticalFrame51 = null;
			bool flag = prefs.Enabled && reader.TryReadMinimap(out frame);
			bool flag2 = tactical51.Enabled && reader.TryReadTactical51(out tacticalFrame51);
			if (!flag && !flag2)
			{
				LastError = (prefs.Enabled ? reader.MinimapStatus : reader.TacticalStatus51);
				Trace("地图定位中", LastError + " " + reader.MinimapDiagnostic);
				Hide();
				return;
			}
			markers = reader.ReadSquadMarkers(match, sources);
			LastError = "";
			base.Bounds = new Rectangle(p.X, p.Y, r.Right, r.Bottom);
			if (markers.Count == 0 && (!prefs.Barrages || barrages.Count == 0) && (Range55 == null || Range55() == null))
			{
				Trace("没有可绘制敌军");
				Hide();
				return;
			}
			Trace("小地图绘制", "单位=" + markers.Count + " 弹幕=" + barrages.Count + " " + reader.MinimapDiagnostic);
			if (!base.Visible)
			{
				Show();
			}
			SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u);
			Invalidate();
		}
		catch (Exception ex)
		{
			frame = null;
			tacticalFrame51 = null;
			markers.Clear();
			LastError = ex.Message;
			Trace("小地图读取失败", ex.Message);
			Hide();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (LicenseGate60.Allowed)
		{
			MinimapMarkerDrawing.Draw(e.Graphics, markers, frame, prefs, base.ClientSize.Width, base.ClientSize.Height, barrages);
			TacticalMapDrawing51.Draw(e.Graphics, markers, tacticalFrame51, prefs, tactical51, base.ClientSize.Width, base.ClientSize.Height, barrages);
			FlankMapRange55 r = ((Range55 == null) ? null : Range55());
			if (prefs.Enabled)
			{
				FlankMapDrawing55.Draw(e.Graphics, r, frame, base.ClientSize.Width, base.ClientSize.Height);
			}
			if (tactical51.Enabled)
			{
				FlankMapDrawing55.Draw(e.Graphics, r, tacticalFrame51, base.ClientSize.Width, base.ClientSize.Height);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			timer.Dispose();
		}
		base.Dispose(disposing);
	}
}
