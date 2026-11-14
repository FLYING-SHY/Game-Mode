using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class SquadMarkerWindow : Form
{
	private struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	internal const int FrameMessage = 33494;

	private volatile SquadMarkerPreferences settings;

	private volatile MarkerScene scene;

	private readonly bool track;

	private PrecisionFrameClock pump;

	private PlayerInfoReader reader;

	private string activeMatch;

	private MarkerScene current;

	private List<SquadMarker> markers = new List<SquadMarker>();

	private List<MarkerDrawCommand> commands = new List<MarkerDrawCommand>();

	private long airTestUntil;

	private double nextAirRead;

	private List<AirZone> airZones = new List<AirZone>();

	private readonly MarkerSpriteCache airSprites = new MarkerSpriteCache();

	private string airVisibleState = "";

	private List<DirectionStroke> directionPaths = new List<DirectionStroke>();

	private List<ProjectileImpact> shellImpacts = new List<ProjectileImpact>();

	private readonly ProjectileSpriteCache shellSprites = new ProjectileSpriteCache();

	private double nextShellRead;

	private string shellWarning = "";

	private readonly MarkerSpriteCache sprites = new MarkerSpriteCache();

	private readonly Font metricFont = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold);

	private readonly Stopwatch clock = Stopwatch.StartNew();

	private double nextPositionRead;

	private double cameraReadMs;

	private double unitReadMs;

	private double cameraSampled;

	private double reportStarted;

	private double lastPaint;

	private double paintSum;

	private double ageSum;

	private int painted;

	private int positionReads;

	private int appliedPositionRate;

	private int failedFrames;

	private readonly List<double> intervals = new List<double>();

	private bool drawnMetrics;

	private string stats = "返回游戏后测量：标记绘制帧率、帧间隔、采样到绘制耗时。";

	public volatile string LastError = "";

	public volatile string Statistics = "返回游戏后开始测量。";

	public long PresentedFramesForTest;

	private int layerError66;

	public long PresentedFrameCount65 => Interlocked.Read(ref PresentedFramesForTest);

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

	public int WindowStyleForTest => CreateParams.ExStyle;

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr w, out Rect r);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr w, ref Point p);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr w);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr w, IntPtr after, int x, int y, int cx, int cy, uint flags);

	public void TestAir()
	{
		Interlocked.Exchange(ref airTestUntil, DateTime.UtcNow.AddSeconds(8.0).Ticks);
		if (pump != null)
		{
			pump.RequestNow();
		}
	}

	internal void RestoreLayer66()
	{
		if (SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u))
		{
			if (layerError66 != 0)
			{
				OverlayDiagnostics55.Record("主画面层级恢复", "恢复成功");
			}
			layerError66 = 0;
			return;
		}
		int num = Math.Max(1, Marshal.GetLastWin32Error());
		if (num != layerError66)
		{
			OverlayDiagnostics55.Record("主画面层级恢复失败", "Win32=" + num);
		}
		layerError66 = num;
	}

	public SquadMarkerWindow(SquadMarkerPreferences p, bool track = true)
	{
		settings = p.Copy();
		this.track = track;
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.Manual;
		base.AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(1, 2, 3);
		base.TransparencyKey = BackColor;
		DoubleBuffered = true;
	}

	[Conditional("SKYEYE_PIPELINE_TRACE")]
	public void TraceClock()
	{
		PrecisionFrameClock precisionFrameClock = pump;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (track)
		{
			pump = new PrecisionFrameClock(base.Handle, 33494, 5);
		}
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		if (pump != null)
		{
			pump.Dispose();
			pump = null;
		}
		base.OnHandleDestroyed(e);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 33494)
		{
			try
			{
				Frame();
				return;
			}
			finally
			{
				if (pump != null)
				{
					pump.Complete();
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

	public void ApplySettings(SquadMarkerPreferences p)
	{
		settings = p;
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
		if (pump != null)
		{
			pump.RequestNow();
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
		Interlocked.Exchange(ref airTestUntil, 0L);
		airZones.Clear();
		nextAirRead = 0.0;
		if (reader != null)
		{
			reader.ResetAirZones();
		}
		commands.Clear();
		directionPaths.Clear();
		if (base.Visible)
		{
			Hide();
		}
		Invalidate();
	}

	public void PublishScene(MarkerScene next)
	{
		scene = next;
		if (pump != null)
		{
			pump.RequestNow();
		}
	}

	private void Pause(bool idle = true)
	{
		if (base.Visible)
		{
			Hide();
		}
		if (pump != null)
		{
			pump.Rate = (idle ? 5 : settings.FrameRate);
		}
		lastPaint = 0.0;
		painted = (positionReads = 0);
		paintSum = (ageSum = 0.0);
		intervals.Clear();
		reportStarted = clock.Elapsed.TotalMilliseconds;
	}

	private void DropConnection()
	{
		if (reader != null)
		{
			reader.Dispose();
		}
		reader = null;
		activeMatch = null;
		markers.Clear();
		commands.Clear();
		directionPaths.Clear();
		shellImpacts.Clear();
		airZones.Clear();
		airSprites.Reset();
		nextAirRead = 0.0;
		sprites.Reset();
		nextPositionRead = (nextShellRead = 0.0);
		shellWarning = "";
		failedFrames = 0;
	}

	private MineCamera FreshCamera()
	{
		Exception ex = null;
		for (int i = 0; i < 3; i++)
		{
			try
			{
				return reader.ReadMineCamera();
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
		}
		throw ex;
	}

	private void Frame()
	{
		SquadMarkerPreferences p = settings;
		MarkerScene markerScene = scene;
		if (!LicenseGate60.Allowed)
		{
			DropConnection();
			Pause();
			return;
		}
		if (markerScene == null)
		{
			DropConnection();
			Pause();
			return;
		}
		if ((!p.Enabled && !p.Projectiles.AnyEnabled && !p.Air.Enabled && DateTime.UtcNow.Ticks > Interlocked.Read(ref airTestUntil)) || DateTime.UtcNow - markerScene.Updated > TimeSpan.FromSeconds(3.0))
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
		if (TacticalMapState51.Open(markerScene.Context.Pid, markerScene.Context.Match))
		{
			Pause(idle: false);
			return;
		}
		Point p2 = default(Point);
		if (!GetClientRect(foregroundWindow, out var r) || r.Right < 200 || r.Bottom < 100 || !ClientToScreen(foregroundWindow, ref p2))
		{
			Pause();
			return;
		}
		try
		{
			if (reader == null || activeMatch != markerScene.Context.Match)
			{
				DropConnection();
				reader = new PlayerInfoReader(markerScene.Context);
				activeMatch = markerScene.Context.Match;
			}
			current = markerScene;
			if (pump != null)
			{
				pump.Rate = p.FrameRate;
			}
			sprites.Configure(p);
			if (appliedPositionRate != p.PositionRate)
			{
				appliedPositionRate = p.PositionRate;
				nextPositionRead = 0.0;
			}
			if (!p.Enabled)
			{
				markers.Clear();
				nextPositionRead = 0.0;
			}
			else if (clock.Elapsed.TotalMilliseconds >= nextPositionRead)
			{
				double totalMilliseconds = clock.Elapsed.TotalMilliseconds;
				markers = reader.ReadSquadMarkers(activeMatch, current.Sources);
				unitReadMs = clock.Elapsed.TotalMilliseconds - totalMilliseconds;
				positionReads++;
				double num = 1000.0 / (double)p.PositionRate;
				nextPositionRead += num;
				if (nextPositionRead <= totalMilliseconds)
				{
					nextPositionRead = totalMilliseconds + num;
				}
			}
			if (!p.Projectiles.AnyEnabled)
			{
				shellImpacts.Clear();
				nextShellRead = 0.0;
				shellWarning = "";
			}
			else if (clock.Elapsed.TotalMilliseconds >= nextShellRead)
			{
				double totalMilliseconds2 = clock.Elapsed.TotalMilliseconds;
				try
				{
					shellImpacts = reader.ReadProjectileImpacts(activeMatch, (current.ProjectilePlayers ?? current.Players ?? new ProjectilePlayer[0]).Where((ProjectilePlayer shotPlayer49) => shotPlayer49.Relation == null || shotPlayer49.Relation == "敌方" || (shotPlayer49.Relation == "自己" && p.Projectiles.ShowOwn) || (shotPlayer49.Relation == "队友" && p.Projectiles.ShowAllies)), p.Projectiles.InfantryOn);
					shellWarning = "";
				}
				catch (Exception ex)
				{
					shellImpacts.Clear();
					shellWarning = ex.Message;
				}
				nextShellRead += 33.333333333333336;
				if (nextShellRead <= totalMilliseconds2)
				{
					nextShellRead = totalMilliseconds2 + 33.333333333333336;
				}
			}
			if (!p.Air.Enabled)
			{
				airZones.Clear();
				nextAirRead = 0.0;
			}
			else if (clock.Elapsed.TotalMilliseconds >= nextAirRead)
			{
				try
				{
					airZones = reader.ReadAirZones(activeMatch, current.Players ?? new ProjectilePlayer[0]);
				}
				catch (Exception ex2)
				{
					airZones.Clear();
					AirRender42.Log("边框读取失败：" + ex2.Message);
				}
				nextAirRead = clock.Elapsed.TotalMilliseconds + 33.333333333333336;
			}
			double totalMilliseconds3 = clock.Elapsed.TotalMilliseconds;
			MineCamera mineCamera = FreshCamera();
			cameraSampled = clock.Elapsed.TotalMilliseconds;
			cameraReadMs = cameraSampled - totalMilliseconds3;
			List<DirectionStroke> list = DirectionDrawing.Build(markers, mineCamera, p, r.Right, r.Bottom);
			List<MarkerDrawCommand> fresh = new List<MarkerDrawCommand>();
			foreach (SquadMarker marker in markers)
			{
				if (mineCamera.Project(marker.X, marker.Y + (marker.TextOnly ? 3f : (marker.Vehicle ? 1.2f : 0.8f)), marker.Z, r.Right, r.Bottom, out var point))
				{
					Bitmap bitmap = sprites.Get(marker);
					fresh.Add(new MarkerDrawCommand
					{
						Image = bitmap,
						Bounds = new Rectangle((int)Math.Round(point.X - (float)bitmap.Width / 2f), (int)Math.Round(point.Y - (float)bitmap.Height / 2f), bitmap.Width, bitmap.Height)
					});
				}
			}
			List<Rectangle> movementLabelBounds = new List<Rectangle>();
			list.AddRange(MovementDrawing.Build(markers, mineCamera, p, r.Right, r.Bottom, delegate(SquadMarker marker, PointF anchor)
			{
				Bitmap bitmap2 = sprites.MovementLabel(marker);
				Rectangle rectangle2 = MovementLabelLayout.Place(bitmap2.Size, anchor, r.Right, r.Bottom, movementLabelBounds);
				movementLabelBounds.Add(rectangle2);
				fresh.Add(new MarkerDrawCommand
				{
					Image = bitmap2,
					Bounds = rectangle2
				});
			}));
			list.AddRange(ProjectileDrawing.Rings(shellImpacts, mineCamera, p.Projectiles, r.Right, r.Bottom));
			ProjectileDrawing.AddNumbers(fresh, shellImpacts, mineCamera, p.Projectiles, r.Right, r.Bottom, shellSprites);
			int num2 = AirRender42.Add(fresh, list, airZones, mineCamera, p.Air, r.Right, r.Bottom, airSprites);
			string text = "空袭边框图层：识别=" + airZones.Count + " 可见=" + num2;
			if (text != airVisibleState)
			{
				airVisibleState = text;
				AirRender42.Log(text);
			}
			if (DateTime.UtcNow.Ticks < Interlocked.Read(ref airTestUntil))
			{
				AirRender42.Test(fresh, list, r.Right, r.Bottom, p.Air, airSprites);
			}
			if (fresh.Count == 0 && list.Count == 0 && !p.ShowMetrics)
			{
				commands = fresh;
				directionPaths = list;
				LastError = ((shellWarning.Length > 0) ? shellWarning : current.Warning);
				failedFrames = 0;
				Pause(idle: false);
				return;
			}
			Rectangle rect = MetricsRect(r.Right, r.Bottom);
			using Region region = new Region();
			region.MakeEmpty();
			foreach (DirectionStroke directionPath in directionPaths)
			{
				region.Union(directionPath.Bounds);
			}
			foreach (DirectionStroke item in list)
			{
				region.Union(item.Bounds);
			}
			directionPaths = list;
			foreach (MarkerDrawCommand command in commands)
			{
				region.Union(command.Bounds);
			}
			foreach (MarkerDrawCommand item2 in fresh)
			{
				region.Union(item2.Bounds);
			}
			if (p.ShowMetrics || drawnMetrics)
			{
				region.Union(rect);
			}
			drawnMetrics = p.ShowMetrics;
			commands = fresh;
			Rectangle rectangle = new Rectangle(p2.X, p2.Y, r.Right, r.Bottom);
			bool flag = base.Bounds != rectangle;
			bool flag2 = !base.Visible;
			if (flag)
			{
				base.Bounds = rectangle;
			}
			LastError = ((shellWarning.Length > 0) ? shellWarning : current.Warning);
			if (flag2)
			{
				Show();
			}
			RestoreLayer66();
			if (flag || flag2)
			{
				Invalidate();
			}
			else
			{
				Invalidate(region);
			}
			Update();
			failedFrames = 0;
		}
		catch (Exception ex3)
		{
			LastError = ex3.Message;
			failedFrames++;
			if (reader == null)
			{
				Pause();
				return;
			}
			if (failedFrames >= 5 || ex3.Message.Contains("对局已切换"))
			{
				commands.Clear();
				if (base.Visible)
				{
					Hide();
				}
			}
			if (pump != null)
			{
				pump.Rate = p.FrameRate;
			}
		}
	}

	private static Rectangle MetricsRect(int width, int height)
	{
		return new Rectangle(Math.Max(0, width - 555), Math.Max(0, height - 330), 545, 95);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		double totalMilliseconds = clock.Elapsed.TotalMilliseconds;
		base.OnPaint(e);
		if (!LicenseGate60.Allowed)
		{
			return;
		}
		DirectionDrawing.Draw(e.Graphics, directionPaths, settings.DirectionWidth);
		foreach (MarkerDrawCommand command in commands)
		{
			if (command.Bounds.IntersectsWith(e.ClipRectangle))
			{
				e.Graphics.DrawImageUnscaled(command.Image, command.Bounds.Location);
			}
		}
		if (settings.ShowMetrics)
		{
			Rectangle rectangle = MetricsRect(base.ClientSize.Width, base.ClientSize.Height);
			e.Graphics.FillRectangle(Brushes.Black, rectangle);
			e.Graphics.DrawString(stats, metricFont, Brushes.White, rectangle);
		}
		if (commands.Count != 0 || directionPaths.Count != 0 || settings.ShowMetrics)
		{
			Interlocked.Increment(ref PresentedFramesForTest);
			double totalMilliseconds2 = clock.Elapsed.TotalMilliseconds;
			if (lastPaint > 0.0)
			{
				intervals.Add(totalMilliseconds2 - lastPaint);
			}
			lastPaint = totalMilliseconds2;
			painted++;
			paintSum += totalMilliseconds2 - totalMilliseconds;
			ageSum += Math.Max(0.0, totalMilliseconds2 - cameraSampled);
			if (totalMilliseconds2 - reportStarted >= 1000.0)
			{
				intervals.Sort();
				double num = ((intervals.Count == 0) ? 0.0 : intervals[Math.Min(intervals.Count - 1, (int)((double)intervals.Count * 0.95))]);
				stats = string.Format("标记 {0:F1}/{1} 帧/秒 · 帧间隔 P95 {2:F1} ms\n位置 {7:F1}/{8} 次/秒 · 镜头 {3:F2} ms · 单位 {4:F2} ms\n采样→绘制 {5:F2} ms · 绘图 {6:F2} ms\n（绘制统计，不含游戏渲染及显示器延迟）", (double)(painted * 1000) / (totalMilliseconds2 - reportStarted), settings.FrameRate, num, cameraReadMs, unitReadMs, ageSum / (double)painted, paintSum / (double)painted, (double)(positionReads * 1000) / (totalMilliseconds2 - reportStarted), settings.PositionRate);
				Statistics = stats;
				reportStarted = totalMilliseconds2;
				painted = (positionReads = 0);
				paintSum = (ageSum = 0.0);
				intervals.Clear();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (pump != null)
			{
				pump.Dispose();
				pump = null;
			}
			DropConnection();
			sprites.Dispose();
			airSprites.Dispose();
			shellSprites.Dispose();
			metricFont.Dispose();
		}
		base.Dispose(disposing);
	}
}
