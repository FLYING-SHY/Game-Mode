using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class AirOverlayWindow : Form
{
	private struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private const int MessageFrame = 33508;

	private string diagnosticState = "";

	private readonly HashSet<string> diagnosticZones = new HashSet<string>();

	private long testUntil;

	private readonly bool track;

	private volatile SquadMarkerPreferences settings;

	private volatile MarkerScene scene;

	private PrecisionFrameClock clock;

	private readonly Stopwatch time = Stopwatch.StartNew();

	private readonly AlphaSurface surface = new AlphaSurface();

	private PlayerInfoReader reader;

	private string match;

	private List<AirZone> zones = new List<AirZone>();

	private double nextRead;

	private double nextMap;

	private MinimapFrame minimap;

	private MinimapFrame tacticalMap51;

	private volatile TacticalMapPreferences51 tactical51;

	private Font font;

	private Font miniFont;

	private int size;

	private int miniSize;

	private int errors;

	public volatile string LastError = "";

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

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr w, out Rect r);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr w, ref Point p);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr w);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr w, IntPtr after, int x, int y, int cx, int cy, uint flags);

	private void Trace(string message)
	{
		try
		{
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "air-diagnostics.log");
			if (File.Exists(text) && new FileInfo(text).Length > 262144)
			{
				File.WriteAllText(text, "日志达到上限，重新记录。\r\n");
			}
			File.AppendAllText(text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + message + Environment.NewLine);
		}
		catch
		{
		}
	}

	private void TraceState(string message)
	{
		if (!(diagnosticState == message))
		{
			diagnosticState = message;
			Trace(message);
		}
	}

	public void TestAir()
	{
		Interlocked.Exchange(ref testUntil, DateTime.UtcNow.AddSeconds(8.0).Ticks);
		if (clock != null)
		{
			clock.RequestNow();
		}
	}

	public AirOverlayWindow(SquadMarkerPreferences p, bool track = true, TacticalMapPreferences51 tactical = null)
	{
		settings = p;
		tactical51 = CopyTactical51(tactical);
		this.track = track;
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.Manual;
		base.AutoScaleMode = AutoScaleMode.None;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (track)
		{
			clock = new PrecisionFrameClock(base.Handle, 33508, 5);
		}
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		if (clock != null)
		{
			clock.Dispose();
			clock = null;
		}
		base.OnHandleDestroyed(e);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 33508)
		{
			try
			{
				Frame();
				return;
			}
			finally
			{
				if (clock != null)
				{
					clock.Complete();
				}
			}
		}
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

	protected override void OnPaintBackground(PaintEventArgs e)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
	}

	private static TacticalMapPreferences51 CopyTactical51(TacticalMapPreferences51 p)
	{
		TacticalMapPreferences51 tacticalMapPreferences = new TacticalMapPreferences51();
		tacticalMapPreferences.Enabled = p?.Enabled ?? false;
		tacticalMapPreferences.TextSize = p?.TextSize ?? 12;
		return tacticalMapPreferences;
	}

	public void ApplySettings(SquadMarkerPreferences p, TacticalMapPreferences51 tactical = null)
	{
		settings = p;
		tactical51 = CopyTactical51(tactical);
		if (!p.Air.Enabled && base.IsHandleCreated)
		{
			try
			{
				BeginInvoke(new Action(ClearAirForSwitch));
			}
			catch (InvalidOperationException)
			{
			}
		}
		if (clock != null)
		{
			clock.RequestNow();
		}
	}

	public void ClearAir()
	{
		if (base.IsHandleCreated)
		{
			try
			{
				BeginInvoke(new Action(ClearAirForSwitch));
			}
			catch (InvalidOperationException)
			{
			}
		}
	}

	private void ClearAirForSwitch()
	{
		Interlocked.Exchange(ref testUntil, 0L);
		zones.Clear();
		nextRead = 0.0;
		if (reader != null)
		{
			reader.ResetAirZones();
		}
		if (surface.Graphics != null)
		{
			surface.Graphics.Clear(Color.Transparent);
		}
		if (base.Visible)
		{
			Hide();
		}
	}

	public void Publish(MarkerScene p)
	{
		scene = p;
		if (clock != null)
		{
			clock.RequestNow();
		}
	}

	private void Pause()
	{
		if (base.Visible)
		{
			Hide();
		}
		if (clock != null)
		{
			clock.Rate = 5;
		}
	}

	private void Drop()
	{
		if (reader != null)
		{
			reader.Dispose();
		}
		reader = null;
		match = null;
		zones.Clear();
		minimap = null;
		tacticalMap51 = null;
		nextRead = (nextMap = 0.0);
	}

	private void Fonts(AirPreferences p)
	{
		if (font == null || size != p.TextSize)
		{
			if (font != null)
			{
				font.Dispose();
			}
			font = new Font("Microsoft YaHei UI", p.TextSize, FontStyle.Bold);
			size = p.TextSize;
		}
		if (miniFont == null || miniSize != p.MiniTextSize)
		{
			if (miniFont != null)
			{
				miniFont.Dispose();
			}
			miniFont = new Font("Microsoft YaHei UI", p.MiniTextSize, FontStyle.Bold);
			miniSize = p.MiniTextSize;
		}
	}

	private void Frame()
	{
		MarkerScene markerScene = scene;
		AirPreferences air = settings.Air;
		if (!LicenseGate60.Allowed)
		{
			Drop();
			if (surface.Graphics != null)
			{
				surface.Graphics.Clear(Color.Transparent);
			}
			Pause();
			return;
		}
		if (markerScene == null)
		{
			Drop();
			Pause();
			return;
		}
		if ((!air.Enabled && !air.V1 && !air.MiniV1) || DateTime.UtcNow - markerScene.Updated > TimeSpan.FromSeconds(3.0))
		{
			Pause();
			return;
		}
		IntPtr foregroundWindow = Native.GetForegroundWindow();
		Native.GetWindowThreadProcessId(foregroundWindow, out var pid);
		if (pid != markerScene.Context.Pid || IsIconic(foregroundWindow))
		{
			Pause();
			return;
		}
		Point p = Point.Empty;
		if (!GetClientRect(foregroundWindow, out var r) || r.Right < 200 || r.Bottom < 100 || !ClientToScreen(foregroundWindow, ref p))
		{
			Pause();
			return;
		}
		try
		{
			if (reader == null || match != markerScene.Context.Match)
			{
				Drop();
				reader = new PlayerInfoReader(markerScene.Context);
				match = markerScene.Context.Match;
				Trace("连接对局 " + match + " 敌方槽位=" + string.Join(",", markerScene.Players.Select((ProjectilePlayer x) => x.Slot)));
				diagnosticZones.Clear();
			}
			if (clock != null)
			{
				clock.Rate = settings.FrameRate;
			}
			double totalMilliseconds = time.Elapsed.TotalMilliseconds;
			if (totalMilliseconds >= nextRead)
			{
				zones = reader.ReadAirZones(match, markerScene.Players ?? new ProjectilePlayer[0]);
				HashSet<string> hashSet = new HashSet<string>(zones.Select((AirZone z) => z.Key));
				foreach (AirZone item in zones.Where((AirZone z) => !diagnosticZones.Contains(z.Key)))
				{
					Trace("识别 " + item.Name + " 槽位=" + item.Slot + " 来源=" + (item.Key.StartsWith("flight:") ? "飞机补读" : "技能或火箭") + " 目标=" + item.X + "," + item.Y + "," + item.Z);
				}
				foreach (string item2 in diagnosticZones.Except(hashSet))
				{
					Trace("清除 " + item2);
				}
				diagnosticZones.Clear();
				foreach (string item3 in hashSet)
				{
					diagnosticZones.Add(item3);
				}
				nextRead += 33.333333333333336;
				if (nextRead <= totalMilliseconds)
				{
					nextRead = totalMilliseconds + 33.333333333333336;
				}
			}
			if (zones.Count == 0 && DateTime.UtcNow.Ticks >= Interlocked.Read(ref testUntil))
			{
				LastError = "";
				errors = 0;
				if (base.Visible)
				{
					Hide();
				}
				return;
			}
			if (air.MiniV1 && zones.Any((AirZone z) => z.Rocket) && totalMilliseconds >= nextMap)
			{
				reader.TryReadMinimap(out minimap);
				if (tactical51.Enabled)
				{
					reader.TryReadTactical51(out tacticalMap51);
				}
				else
				{
					tacticalMap51 = null;
				}
				nextMap = totalMilliseconds + 100.0;
			}
			Fonts(air);
			surface.Ensure(r.Right, r.Bottom);
			surface.Graphics.Clear(Color.Transparent);
			if (!TacticalMapState51.Open(markerScene.Context.Pid, markerScene.Context.Match))
			{
				MineCamera camera = reader.ReadMineCamera();
				AirDrawing.World(surface.Graphics, zones.Where((AirZone z) => z.Rocket), camera, air, r.Right, r.Bottom, font);
				AirRender42.Fill(surface.Graphics, zones, camera, air, r.Right, r.Bottom);
				if (DateTime.UtcNow.Ticks < Interlocked.Read(ref testUntil))
				{
					AirRender42.TestFill(surface.Graphics, r.Right, r.Bottom, air);
				}
			}
			AirDrawing.Minimap(surface.Graphics, zones, minimap, air, r.Right, r.Bottom, miniFont);
			if (tactical51.Enabled)
			{
				AirDrawing.Minimap(surface.Graphics, zones, tacticalMap51, air, r.Right, r.Bottom, miniFont);
			}
			if (!base.Visible)
			{
				Show();
			}
			surface.Present(base.Handle, p);
			SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u);
			TraceState("绘制正常");
			LastError = "";
			errors = 0;
		}
		catch (Exception ex)
		{
			TraceState("失败：" + ex.Message);
			LastError = ex.Message;
			zones.Clear();
			minimap = null;
			if (++errors >= 3)
			{
				Drop();
			}
			if (base.Visible)
			{
				Hide();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (clock != null)
			{
				clock.Dispose();
				clock = null;
			}
			Drop();
			surface.Dispose();
			if (font != null)
			{
				font.Dispose();
			}
			if (miniFont != null)
			{
				miniFont.Dispose();
			}
		}
		base.Dispose(disposing);
	}
}
