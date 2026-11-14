using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class EnemyOverlayWindow : Form
{
	private struct Rect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	private struct Pt
	{
		public int X;

		public int Y;
	}

	private readonly Timer follow = new Timer();

	private readonly OverlayPreferences preferences;

	private string[] lines = new string[0];

	private Color[] lineColours = new Color[0];

	private NumberSpan[][] numberSpans = new NumberSpan[0][];

	private int gamePid;

	private DateTime updated = DateTime.MinValue;

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

	public int WindowStyleForTest => GetWindowLong(base.Handle, -20);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr h, out Rect r);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(IntPtr h, ref Pt p);

	[DllImport("user32.dll")]
	private static extern bool IsIconic(IntPtr h);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr h, IntPtr insert, int x, int y, int cx, int cy, uint flags);

	[DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
	private static extern int GetWindowLong(IntPtr h, int index);

	public EnemyOverlayWindow(OverlayPreferences p, bool track = true)
	{
		preferences = p;
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		base.StartPosition = FormStartPosition.Manual;
		base.AutoScaleMode = AutoScaleMode.None;
		BackColor = Color.FromArgb(1, 2, 3);
		base.TransparencyKey = BackColor;
		DoubleBuffered = true;
		ApplyAppearance();
		follow.Interval = 50;
		Timer timer = follow;
		EventHandler value = delegate
		{
			FollowGame();
		};
		timer.Tick += value;
		if (track)
		{
			follow.Start();
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

	public void ApplyAppearance()
	{
		Font font = Font;
		Font = new Font("Microsoft YaHei UI", preferences.FontSize, FontStyle.Bold, GraphicsUnit.Point);
		if (font != null && !font.Equals(Control.DefaultFont))
		{
			font.Dispose();
		}
		Color backColor = Color.FromArgb(1, 2, 3);
		while (backColor.ToArgb() == preferences.Color.ToArgb() || backColor.ToArgb() == preferences.SkillColor.ToArgb())
		{
			backColor = Color.FromArgb(backColor.R + 1, 2, 3);
		}
		BackColor = backColor;
		base.TransparencyKey = BackColor;
		Invalidate();
	}

	public void UpdateRows(int pid, IEnumerable<PlayerInfoRow> rows)
	{
		gamePid = pid;
		PlayerInfoRow[] rows2 = rows.ToArray();
		lines = OverlayText.Lines(rows2, preferences);
		lineColours = OverlayText.Colours(rows2, preferences);
		numberSpans = OverlayText.NumberSpans(rows2, preferences);
		updated = DateTime.UtcNow;
		Invalidate();
		FollowGame();
	}

	public void ClearData()
	{
		lines = new string[0];
		updated = DateTime.MinValue;
		Hide();
	}

	private void FollowGame()
	{
		if (!LicenseGate60.Allowed || !preferences.Enabled || lines.Length == 0 || DateTime.UtcNow - updated > TimeSpan.FromSeconds(3.0))
		{
			Hide();
			return;
		}
		IntPtr foregroundWindow = Native.GetForegroundWindow();
		Native.GetWindowThreadProcessId(foregroundWindow, out var pid);
		if (pid != gamePid || IsIconic(foregroundWindow))
		{
			Hide();
			return;
		}
		Pt p = default(Pt);
		if (!GetClientRect(foregroundWindow, out var r) || !ClientToScreen(foregroundWindow, ref p) || r.Right < 200 || r.Bottom < 100)
		{
			Hide();
			return;
		}
		int num = Math.Max(100, (r.Right - r.Left) * preferences.WidthPercent / 100);
		int num2;
		using (Graphics g = CreateGraphics())
		{
			num2 = OverlayDrawing.Draw(g, lines, Font, preferences.Color, preferences.SkillColor, num, paint: false, preferences.DetailSize, lineColours, numberSpans);
		}
		int num3 = Math.Min(preferences.Top, Math.Max(0, r.Bottom - num2));
		base.Bounds = new Rectangle(p.X + (r.Right - num) / 2, p.Y + num3, num, Math.Min(num2, r.Bottom - num3));
		if (!base.Visible)
		{
			Show();
		}
		SetWindowPos(base.Handle, new IntPtr(-1), 0, 0, 0, 0, 19u);
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		OverlayDrawing.Draw(e.Graphics, lines, Font, preferences.Color, preferences.SkillColor, base.ClientSize.Width, paint: true, preferences.DetailSize, lineColours, numberSpans);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			follow.Stop();
			follow.Dispose();
		}
		base.Dispose(disposing);
	}
}
