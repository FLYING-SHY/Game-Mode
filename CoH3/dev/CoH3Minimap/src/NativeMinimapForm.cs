using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace CoH3Minimap;

internal sealed class NativeMinimapForm : Form
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WH_MOUSE_LL = 14;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_RBUTTONUP = 0x0205;

    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandle(string? lpModuleName);

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)] private struct POINT { public int X; public int Y; }

    private static NativeMinimapForm? _clickThroughOwner;
    private IntPtr _mouseHook;
    private readonly LowLevelMouseProc _hookProc; // 防GC回收委托

    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 100 };
    private readonly CancellationTokenSource _stop = new();
    private readonly NativeAssetCatalog _assets = new();
    private readonly object _gate = new();
    private readonly ToolStripMenuItem _experimentalFog = new("实验 CPU 迷雾（mod=1，无时间混合）") { CheckOnClick = true };
    private readonly ToolStripMenuItem _showAllUnits = new("显示全部单位（迷雾上敌军）") { CheckOnClick = true };
    private readonly ToolStripMenuItem _clickThrough = new("鼠标穿透（小地图上右键解除）") { CheckOnClick = true };
    private readonly Func<CancellationToken, NativeHudFrame?>? _captureOverride;
    private Thread? _worker;
    private NativeHudFrame? _frame;
    private string _status = "等候游戏原生 HUD 初始化…";
    private Point? _drag;
    private bool _disposed;
    internal bool WorkerStopped => _worker is null || !_worker.IsAlive;

    internal NativeMinimapForm(Func<CancellationToken, NativeHudFrame?>? captureOverride = null, bool experimentalCpuFog = false,
        bool showAllUnits = false)
    {
        _captureOverride = captureOverride;
        _hookProc = LowLevelMouseHook;
        Text = "CoH3 原生小地图";
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true; ShowInTaskbar = true; DoubleBuffered = true;
        AutoScaleMode = AutoScaleMode.None;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(40, 40); ClientSize = new Size(400, 448);
        var menu = new ContextMenuStrip();
        menu.Items.Add("268 像素", null, (_, _) => ClientSize = new Size(276, 324));
        menu.Items.Add("400 像素", null, (_, _) => ClientSize = new Size(400, 448));
        menu.Items.Add("600 像素", null, (_, _) => ClientSize = new Size(600, 648));
        _experimentalFog.CheckedChanged += (_, _) =>
        {
            Text = _experimentalFog.Checked ? "CoH3 原生小地图 · CPU Fog 实验" : "CoH3 原生小地图";
            Invalidate();
        };
        _experimentalFog.Checked = experimentalCpuFog;
        _showAllUnits.Checked = showAllUnits;
        _showAllUnits.CheckedChanged += (_, _) => Invalidate();
        _clickThrough.CheckedChanged += (_, _) => ApplyClickThrough(_clickThrough.Checked);
        menu.Items.Add(_experimentalFog);
        menu.Items.Add(_showAllUnits);
        menu.Items.Add(_clickThrough);
        menu.Items.Add("关闭", null, (_, _) => Close());
        ContextMenuStrip = menu;
        MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) _drag = e.Location; };
        MouseMove += (_, e) => { if (_drag is { } anchor && e.Button == MouseButtons.Left) Location += new Size(e.X - anchor.X, e.Y - anchor.Y); };
        MouseUp += (_, _) => _drag = null;
        _timer.Tick += (_, _) => Invalidate();
        Shown += (_, _) =>
        {
            _worker = new Thread(CaptureLoop) { IsBackground = true, Name = "NativeHudReadOnly" };
            _worker.Start(); _timer.Start();
        };
    }

    /// <summary>开启：WS_EX_LAYERED|WS_EX_TRANSPARENT 让全部鼠标消息穿过窗口；WH_MOUSE_LL 全局钩子在小地图区域内拦下右键并解除穿透。</summary>
    private void ApplyClickThrough(bool enable)
    {
        if (IsHandleCreated)
        {
            int style = GetWindowLong(Handle, GWL_EXSTYLE);
            int desired = enable ? style | WS_EX_LAYERED | WS_EX_TRANSPARENT
                : style & ~(WS_EX_LAYERED | WS_EX_TRANSPARENT);
            if (desired != style) SetWindowLong(Handle, GWL_EXSTYLE, desired);
        }
        if (enable && _mouseHook == IntPtr.Zero)
        {
            _clickThroughOwner = this;
            _mouseHook = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, GetModuleHandle(null), 0);
        }
        else if (!enable && _mouseHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;
            _clickThroughOwner = null;
        }
        Text = enable ? Text + " · 穿透" : Text.Replace(" · 穿透", "");
        Invalidate();
    }

    private static IntPtr LowLevelMouseHook(int nCode, IntPtr wParam, IntPtr lParam)
    {
        var owner = _clickThroughOwner;
        if (nCode >= 0 && owner is not null && !owner.IsDisposed &&
            (wParam == (IntPtr)WM_RBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONUP))
        {
            GetCursorPos(out var cursor);
            var bounds = owner.RectangleToScreen(owner.ClientRectangle);
            if (bounds.Contains(cursor.X, cursor.Y))
            {
                if (wParam == (IntPtr)WM_RBUTTONDOWN)
                    owner.BeginInvoke(() => owner._clickThrough.Checked = false); // 解除穿透并弹菜单
                return (IntPtr)1; // 吞掉本次右键，避免穿透给游戏发移动/取消指令
            }
        }
        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private void CaptureLoop()
    {
        NativeFrameSource? source = null;
        var token = _stop.Token;
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    NativeHudFrame? frame;
                    if (_captureOverride is not null) frame = _captureOverride(token);
                    else
                    {
                        source ??= NativeFrameSource.Attach();
                        frame = source?.Capture(token);
                        if (frame is not null && source is not null)
                            _assets.PrepareMap(source.GameDirectory, frame.Map.Minimap?.AssetPath);
                    }
                    lock (_gate)
                    {
                        _frame = frame;
                        _status = frame is null ? "等候游戏或原生 HUD；对象切换时重试…" : "";
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    source?.Dispose(); source = null;
                    lock (_gate) { _frame = null; _status = ex.Message; }
                    if (token.WaitHandle.WaitOne(1000)) break;
                }
                if (token.WaitHandle.WaitOne(100)) break;
            }
        }
        finally { source?.Dispose(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.FromArgb(22, 25, 28));
        g.SmoothingMode = SmoothingMode.AntiAlias;
        NativeHudFrame? frame; string status;
        lock (_gate) { frame = _frame; status = _status; }
        int side = Math.Max(1, Math.Min(ClientSize.Width - 8, ClientSize.Height - 56));
        var target = new RectangleF((ClientSize.Width - side) / 2f, 4, side, side);
        string detail = "外部只读 · 领土 / 迷雾 / 视口尚未完整接入";
        try
        {
            if (frame?.Map.Geometry is not null)
            {
                var result = NativeFrameRendering.Draw(g, target, frame, _assets, _experimentalFog.Checked, _showAllUnits.Checked);
                string fogStatus = frame.Fog?.UiDisabled switch
                {
                    true => "迷雾：原生关闭",
                    false => result.FogDrawn ? "迷雾：CPU实验，无时间混合" :
                        _experimentalFog.Checked ? "迷雾：CPU候选待更新" : "迷雾：材质待核对",
                    _ => "迷雾：等候缓存"
                };
                detail = "外部只读" + (result.TerritoryDrawn ? " · 领土" : " · 领土待更新") +
                    (result.CameraViewportDrawn ? " / 视口" : "") + " · " + fogStatus;
                status = $"标记 {result.Marks.Drawn}/{frame.Map.Marks.Count} · 缺图 {result.Marks.MissingAssets} · 未支持 {result.Marks.Unsupported}";
                if (_showAllUnits.Checked)
                    status = $"单位 {result.SquadsDrawn}/{frame.Squads?.Count ?? 0}" + (frame.LocalTeam is uint lt ? $" · 本队{lt}" : "") + " · " + status;
                if (!result.BackgroundLoaded) status = "当前地图底图暂未就绪 · " + status;
                if (result.MissingSelectionColor > 0) status += $" · 颜色待更新 {result.MissingSelectionColor}";
                if (frame.MatchState?.IsGameOverSpectatingMap == true) status = "赛后查看地图 · " + status;
                if ((DateTimeOffset.Now - frame.CapturedAt).TotalSeconds > 2) status = "采样已过期 · " + status;
            }
            else if (frame is not null) status = "已找到原生模型，等候地图尺寸缓存…";
        }
        catch (Exception ex) { status = "绘制重试：" + ex.Message; }
        using var border = new Pen(Color.FromArgb(80, 86, 92));
        g.DrawRectangle(border, target.X, target.Y, target.Width, target.Height);
        int top = Math.Min(ClientSize.Height - 44, (int)target.Bottom + 4);
        TextRenderer.DrawText(g, status, Font, new Rectangle(6, top, ClientSize.Width - 12, 20), Color.Gainsboro,
            TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, detail, Font, new Rectangle(6, top + 20, ClientSize.Width - 12, 20), Color.DarkGray,
            TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            _disposed = true;
            if (_mouseHook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHook);
                _mouseHook = IntPtr.Zero;
                _clickThroughOwner = null;
            }
            _timer.Stop(); _timer.Dispose(); _stop.Cancel(); _assets.Dispose();
            ContextMenuStrip?.Dispose();
        }
        base.Dispose(disposing);
    }
}
