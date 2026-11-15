using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class MineOverlayWindow : Form
{
	private struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private readonly OverlayPreferences prefs;

	private readonly Timer timer = new Timer();

	private readonly Font labelFont = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold);

	private readonly BarrageDisplayTracker barrageTracker = new BarrageDisplayTracker();

	private List<BarrageRecord> barrages = new List<BarrageRecord>();

	private PlayerInfoReader reader;

	private List<MineRecord> mines = new List<MineRecord>();

	private MineCamera camera;

	private int pid;

	private DateTime updated;

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

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr w, out Rect r);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr w, ref Point p);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr w);

	public MineOverlayWindow(OverlayPreferences preferences, bool track = true)
	{
		prefs = preferences;
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.Manual;
		base.AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(1, 2, 3);
		base.TransparencyKey = BackColor;
		DoubleBuffered = true;
		timer.Interval = 33;
		Timer obj = timer;
		EventHandler value = delegate
		{
			RefreshFrame();
		};
		obj.Tick += value;
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

	public void UpdateRows(PlayerInfoReader r, int gamePid, IEnumerable<PlayerInfoRow> rows)
	{
		reader = r;
		pid = gamePid;
		mines = (from x in rows.SelectMany((PlayerInfoRow x) => x.Mines)
			group x by x.Entity into x
			select x.First()).ToList();
		foreach (PlayerInfoRow row in rows)
		{
			foreach (BarrageRecord barrage in row.Barrages)
			{
				barrage.Colour = (row.GameColourKnown ? row.GameColour : Color.Red);
			}
		}
		barrages = barrageTracker.Update(r.MatchKey, rows.SelectMany((PlayerInfoRow x) => x.Barrages), DateTime.UtcNow, from x in rows
			where !x.BarragesKnown
			select x.Slot);
		updated = DateTime.UtcNow;
	}

	public void ClearData()
	{
		reader = null;
		camera = null;
		mines.Clear();
		barrages.Clear();
		barrageTracker.Clear();
		Hide();
	}

	private void RefreshFrame()
	{
		if (!LicenseGate60.Allowed || reader == null || ((!prefs.ShowMines || mines.Count == 0) && (!prefs.ShowEnemyBarrages || barrages.Count == 0)) || DateTime.UtcNow - updated > TimeSpan.FromSeconds(3.0))
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
		if (!GetClientRect(foregroundWindow, out var r) || !ClientToScreen(foregroundWindow, ref p))
		{
			Hide();
			return;
		}
		if (TacticalMapState51.Open(pid, reader.MatchKey))
		{
			Hide();
			return;
		}
		try
		{
			camera = reader.ReadMineCamera();
			LastError = "";
			base.Bounds = new Rectangle(p.X, p.Y, r.Right, r.Bottom);
			if (!base.Visible)
			{
				Show();
			}
			Invalidate();
		}
		catch (Exception ex)
		{
			LastError = ex.Message;
			Hide();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (!LicenseGate60.Allowed || camera == null)
		{
			return;
		}
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (MineRecord mine in mines)
		{
			if (!prefs.ShowMines)
			{
				break;
			}
			if (!camera.Project(mine.X, mine.Y + 0.1f, mine.Z, base.ClientSize.Width, base.ClientSize.Height, out var point))
			{
				continue;
			}
			Color color = ((mine.Relation == "敌方") ? prefs.EnemyMineColor : ((mine.Relation == "自己" || mine.Relation == "队友") ? prefs.FriendlyMineColor : Color.Silver));
			using (Pen pen = new Pen(Color.Black, 5f))
			{
				using Pen pen2 = new Pen(color, 3f);
				e.Graphics.DrawEllipse(pen, point.X - 8f, point.Y - 8f, 16f, 16f);
				e.Graphics.DrawEllipse(pen2, point.X - 8f, point.Y - 8f, 16f, 16f);
				e.Graphics.DrawLine(pen2, point.X - 4f, point.Y, point.X + 4f, point.Y);
				e.Graphics.DrawLine(pen2, point.X, point.Y - 4f, point.X, point.Y + 4f);
			}
			string text = ((mine.Relation == "敌方") ? "敌" : ((mine.Relation == "中立") ? "中立" : "友")) + (mine.Demolition ? "炸药" : "雷");
			TextRenderer.DrawText(e.Graphics, text, labelFont, new Point((int)point.X + 11, (int)point.Y - 9), color, Color.Black, TextFormatFlags.NoPadding);
		}
		if (!prefs.ShowEnemyBarrages)
		{
			return;
		}
		foreach (BarrageRecord barrage in barrages)
		{
			if (!camera.Project(barrage.X, barrage.Y + 0.2f, barrage.Z, base.ClientSize.Width, base.ClientSize.Height, out var point2))
			{
				continue;
			}
			float num = (float)prefs.BarrageMarkerSize / 2f;
			float num2 = num * 30f / 22f;
			using (Pen pen3 = new Pen(Color.Black, 5f))
			{
				using Pen pen4 = new Pen(barrage.Colour, 3f);
				e.Graphics.DrawEllipse(pen3, point2.X - num, point2.Y - num, num * 2f, num * 2f);
				e.Graphics.DrawEllipse(pen4, point2.X - num, point2.Y - num, num * 2f, num * 2f);
				e.Graphics.DrawLine(pen4, point2.X - num2, point2.Y, point2.X + num2, point2.Y);
				e.Graphics.DrawLine(pen4, point2.X, point2.Y - num2, point2.X, point2.Y + num2);
			}
			TextRenderer.DrawText(e.Graphics, barrage.Name, labelFont, new Point((int)(point2.X + num + 5f), (int)point2.Y - 10), barrage.Colour, Color.Black, TextFormatFlags.NoPadding);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			timer.Dispose();
			labelFont.Dispose();
		}
		base.Dispose(disposing);
	}
}
