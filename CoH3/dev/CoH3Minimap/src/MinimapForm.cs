namespace CoH3Minimap;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

/// <summary>
/// Topmost borderless minimap window. Draggable, resizable, optional click-through.
/// Double-buffered GDI+ rendering, 10Hz refresh.
/// </summary>
public sealed class MinimapForm : Form
{
    private readonly Timer _timer;
    private readonly System.Threading.CancellationTokenSource _cts = new();
    private System.Threading.Thread? _scanThread;
    private GameSnapshot? _lastSnapshot;
    private readonly Func<System.Threading.CancellationToken, GameSnapshot?>? _captureOverride;
    private string? _scanError;
    internal bool WorkerStopped => _scanThread is null || !_scanThread.IsAlive;
    private readonly object _snapshotLock = new();
    private Bitmap? _terrainBase;
    private Bitmap? _objectiveIcon;
    private bool _dragging;
    private Point _dragStart;
    private bool _clickThrough;
    private bool _showUnits = true;
    private bool _showStructures = true;
    private bool _showMarkers = true;

    private new const int DefaultSize = 400;
    private const int UpdateIntervalMs = 100;

    private static readonly Color BgColor = Color.FromArgb(30, 30, 35);
    private static readonly Color GridColor = Color.FromArgb(50, 50, 60);

    public MinimapForm() : this(null) { }

    internal MinimapForm(Func<System.Threading.CancellationToken, GameSnapshot?>? captureOverride)
    {
        _captureOverride = captureOverride;
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(40, 40);
        Size = new Size(DefaultSize, DefaultSize + 24);

        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        // Load terrain base if exists
        var terrainPath = Path.Combine(AppContext.BaseDirectory, "minimap.png");
        if (File.Exists(terrainPath))
        {
            try { _terrainBase = new Bitmap(terrainPath); } catch { }
        }

        var objectivePath = Path.Combine(AppContext.BaseDirectory, "icons", "tacticalmap", "objective_background.png");
        if (File.Exists(objectivePath))
        {
            try { _objectiveIcon = new Bitmap(objectivePath); } catch { }
        }

        _timer = new Timer { Interval = UpdateIntervalMs };
        _timer.Tick += (_, _) => UpdateFrame();
        _timer.Start();

        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;

        var menu = new ContextMenuStrip();
        var showUnits = new ToolStripMenuItem("Show units") { Checked = true, CheckOnClick = true };
        var showStructures = new ToolStripMenuItem("Show structures") { Checked = true, CheckOnClick = true };
        var showMarkers = new ToolStripMenuItem("Show capture points") { Checked = true, CheckOnClick = true };
        showUnits.CheckedChanged += (_, _) => Invalidate();
        showStructures.CheckedChanged += (_, _) => Invalidate();
        showMarkers.CheckedChanged += (_, _) => Invalidate();
        menu.Items.Add(showUnits);
        menu.Items.Add(showStructures);
        menu.Items.Add(showMarkers);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Click Through On/Off", null, (_, _) => ToggleClickThrough());
        menu.Items.Add("Size 300", null, (_, _) => ResizeTo(300));
        menu.Items.Add("Size 400", null, (_, _) => ResizeTo(400));
        menu.Items.Add("Size 600", null, (_, _) => ResizeTo(600));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => Close());
        ContextMenuStrip = menu;
        _showUnits = showUnits.Checked;
        _showStructures = showStructures.Checked;
        _showMarkers = showMarkers.Checked;
        showUnits.CheckedChanged += (_, _) => _showUnits = showUnits.Checked;
        showStructures.CheckedChanged += (_, _) => _showStructures = showStructures.Checked;
        showMarkers.CheckedChanged += (_, _) => _showMarkers = showMarkers.Checked;

        Paint += OnPaint;
    }

    private void UpdateFrame()
    {
        // Start background scan thread if not running
        if (_scanThread is null || !_scanThread.IsAlive)
        {
            _scanThread = new System.Threading.Thread(ScanLoop)
            {
                IsBackground = true,
                Name = "MinapScan"
            };
            _scanThread.Start();
        }
        Invalidate(); // Repaint with latest snapshot
    }

    private void ScanLoop()
    {
        // The worker owns the process handle; the UI never disposes it mid-read.
        MinimapDataReader? reader = null;
        var token = _cts.Token;
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    GameSnapshot? snap;
                    if (_captureOverride is not null)
                        snap = _captureOverride(token);
                    else
                    {
                        if (reader is null)
                        {
                            var game = GameReader.Attach();
                            if (game is null)
                            {
                                if (token.WaitHandle.WaitOne(2000)) break;
                                continue;
                            }
                            reader = new MinimapDataReader(game);
                        }
                        snap = reader.CaptureSnapshot(token);
                    }
                    lock (_snapshotLock)
                    {
                        _lastSnapshot = snap;
                        _scanError = null;
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    reader?.Dispose();
                    reader = null;
                    lock (_snapshotLock)
                    {
                        _lastSnapshot = null;
                        _scanError = ex.Message;
                    }
                }
                if (token.WaitHandle.WaitOne(500)) break;
            }
        }
        finally { reader?.Dispose(); }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _cts.Cancel();
            // Cancellation is checked between read chunks. Avoid blocking UI shutdown.
            _terrainBase?.Dispose();
            _terrainBase = null;
            _objectiveIcon?.Dispose();
            _objectiveIcon = null;
        }
        base.Dispose(disposing);
    }

    private GameSnapshot? GetSnapshot()
    {
        lock (_snapshotLock)
        {
            return _lastSnapshot;
        }
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        try
        {
            RenderMap(e.Graphics);
        }
        catch (Exception)
        {
            // GDI+ errors (overflow from bad coords etc) — never crash
        }
    }

    private void RenderMap(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int mapH = ClientSize.Height - 24;
        if (mapH <= 0) return;

        using var bg = new SolidBrush(BgColor);
        g.FillRectangle(bg, 0, 0, ClientSize.Width, ClientSize.Height);

        var snap = GetSnapshot();
        if (snap is null || !snap.Value.Map.IsValid)
        {
            DrawStatus(g, "Waiting for game...");
            return;
        }

        var map = snap.Value.Map;
        int w = ClientSize.Width;
        int h = mapH;

        // 1. Terrain base
        if (_terrainBase is not null)
        {
            g.DrawImage(_terrainBase, 0, 0, w, h);
        }
        else
        {
            using var grid = new Pen(GridColor, 1);
            for (int i = 1; i < 8; i++)
            {
                g.DrawLine(grid, w * i / 8, 0, w * i / 8, h);
                g.DrawLine(grid, 0, h * i / 8, w, h * i / 8);
            }
        }

        // 2. Static tactical markers (the game's own marker table)
        if (_showMarkers)
        {
            foreach (var m in snap.Value.Markers)
            {
                if (!float.IsFinite(m.WorldX) || !float.IsFinite(m.WorldY)) continue;
                var (px, py) = map.WorldToPixel(m.WorldX, m.WorldY, w, h);
                if (!float.IsFinite(px) || !float.IsFinite(py)) continue;
                if (px < -6 || px >= w + 6 || py < -6 || py >= h + 6) continue;

                bool capturePoint = m.Style == 0x249B8001;
                if (capturePoint && _objectiveIcon is not null)
                {
                    float s = 16f;
                    g.DrawImage(_objectiveIcon, px - s / 2, py - s / 2, s, s);
                    continue;
                }
                using var pen = new Pen(Color.FromArgb(capturePoint ? 235 : 130, 255, 255, 255), capturePoint ? 1.8f : 1f);
                float r = capturePoint ? 5f : 2.5f;
                if (capturePoint)
                    g.DrawEllipse(pen, px - r, py - r, r * 2, r * 2);
                else
                    g.DrawRectangle(pen, px - r, py - r, r * 2, r * 2);
            }
        }

        // 3. Entities: units clustered into squad-sized diamonds, structures as small squares
        int unitCount = 0, ownCount = 0, otherCount = 0, structureCount = 0;
        int localTeam = DominantUnitTeam(snap.Value.Entities);
        var structurePoints = new List<PointF>();
        var unitPoints = new List<(PointF P, bool Own)>();
        foreach (var ent in snap.Value.Entities)
        {
            if (!float.IsFinite(ent.WorldX) || !float.IsFinite(ent.WorldY)) continue;

            var (px, py) = map.WorldToPixel(ent.WorldX, ent.WorldY, w, h);
            if (!float.IsFinite(px) || !float.IsFinite(py)) continue;
            if (px < -5 || px >= w + 5 || py < -5 || py >= h + 5) continue;

            if (ent.Kind == EntityKind.Structure)
            {
                structurePoints.Add(new PointF(px, py));
                structureCount++;
                continue;
            }

            bool own = ent.TeamId == localTeam;
            unitPoints.Add((new PointF(px, py), own));
            unitCount++;
            if (own) ownCount++; else otherCount++;
        }

        if (_showStructures)
        {
            using var sq = new SolidBrush(Color.FromArgb(120, 195, 195, 205));
            foreach (var p in structurePoints)
                g.FillRectangle(sq, p.X - 1.2f, p.Y - 1.2f, 2.4f, 2.4f);
        }

        int drawnUnits = 0;
        if (_showUnits)
        {
            // Greedy grid cluster (cell = 10 px) so squads read as one icon instead of N dots.
            const float cell = 10f;
            var cells = new Dictionary<(int, int), (float X, float Y, int N, int Own)>();
            foreach (var (p, own) in unitPoints)
            {
                var key = ((int)MathF.Floor(p.X / cell), (int)MathF.Floor(p.Y / cell));
                cells.TryGetValue(key, out var acc);
                cells[key] = (acc.X + p.X, acc.Y + p.Y, acc.N + 1, acc.Own + (own ? 1 : 0));
            }
            drawnUnits = cells.Count;
            foreach (var kv in cells)
            {                var (sx, sy, n, ownN) = kv.Value;
                float cx = sx / n, cy = sy / n;
                bool own = ownN * 2 >= n;
                Color color = own ? Color.FromArgb(255, 255, 60) : Color.FromArgb(255, 70, 70);
                float size = 4f + MathF.Min(4f, MathF.Log2(n + 1) * 1.6f);
                var pts = new[]
                {
                    new PointF(cx, cy - size / 2),
                    new PointF(cx + size / 2, cy),
                    new PointF(cx, cy + size / 2),
                    new PointF(cx - size / 2, cy)
                };
                using (var outline = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                {
                    var big = new[]
                    {
                        new PointF(cx, cy - size / 2 - 1.2f),
                        new PointF(cx + size / 2 + 1.2f, cy),
                        new PointF(cx, cy + size / 2 + 1.2f),
                        new PointF(cx - size / 2 - 1.2f, cy)
                    };
                    g.FillPolygon(outline, big);
                }
                using var brush = new SolidBrush(color);
                g.FillPolygon(brush, pts);
            }
        }

        // 4. Status bar
        DrawStatus(g, $"models {unitCount} (own {ownCount} / other {otherCount}) > icons {drawnUnits} | struct {structureCount} | markers {snap.Value.Markers.Count} | {snap.Value.DataSource}");
    }

    private static int DominantUnitTeam(IReadOnlyList<EntityData> entities)
    {
        var counts = new Dictionary<int, int>();
        foreach (var e in entities)
        {
            if (e.Kind != EntityKind.Unit) continue;
            counts.TryGetValue(e.TeamId, out int c);
            counts[e.TeamId] = c + 1;
        }
        int best = 0, bestCount = -1;
        foreach (var kv in counts)
        {
            if (kv.Value > bestCount) { best = kv.Key; bestCount = kv.Value; }
        }
        return best;
    }

    private void DrawStatus(Graphics g, string text)
    {
        using var font = new Font("Consolas", 8F);
        using var brush = new SolidBrush(Color.FromArgb(180, 180, 190));
        g.DrawString(text, font, brush, 4, ClientSize.Height - 18);
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragging = true;
            _dragStart = e.Location;
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragging)
        {
            var delta = PointToScreen(e.Location) - (Size)new Point(_dragStart.X, _dragStart.Y);
            Location = delta;
        }
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _dragging = false;
    }

    private void ToggleClickThrough()
    {
        _clickThrough = !_clickThrough;
        int exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
        if (_clickThrough)
            SetWindowLong(Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        else
            SetWindowLong(Handle, GWL_EXSTYLE, exStyle & ~WS_EX_TRANSPARENT);
    }

    private void ResizeTo(int size)
    {
        Size = new Size(size, size + 24);
    }

    public void LoadTerrain(Bitmap terrain)
    {
        _terrainBase?.Dispose();
        _terrainBase = terrain;
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x20;
    private const int WS_EX_LAYERED = 0x80000;
}
