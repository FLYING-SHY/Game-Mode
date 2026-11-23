using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>
/// 透明覆盖层窗口。
///
/// 特性：
///   - 无边框、不进任务栏、置顶、鼠标穿透、不抢焦点；
///   - 用 UpdateLayeredWindow 做逐像素 alpha（透明黑底）；
///   - 始终跟随前台游戏窗口的矩形，取不到时退回主屏幕。
/// </summary>
internal sealed class OverlayWindow : Form
{
	private const int WS_EX_LAYERED = 0x00080000;

	private const int WS_EX_TRANSPARENT = 0x00000020;

	private const int WS_EX_TOOLWINDOW = 0x00000080;

	private const int WS_EX_NOACTIVATE = 0x08000000;

	private const int ULW_ALPHA = 0x00000002;

	private const int SWP_NOSIZE = 0x0001;

	private const int SWP_NOZORDER = 0x0004;

	private const int SWP_NOACTIVATE = 0x0010;

	private bool visible;

	public OverlayWindow()
	{
		FormBorderStyle = FormBorderStyle.None;
		ShowInTaskbar = false;
		TopMost = true;
		StartPosition = FormStartPosition.Manual;
		Location = new Point(0, 0);
		Size = new Size(1, 1);
		BackColor = Color.Black;
		// ControlStyles 只能控制 WinForms 自己的样式位；WS_EX_* 属于扩展样式，
		// 在 CreateParams 里追加（见下方重写）。
		SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.OptimizedDoubleBuffer, true);
	}

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams cp = base.CreateParams;
			cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
			return cp;
		}
	}

	protected override bool ShowWithoutActivation
	{
		get { return true; }
	}

	/// <summary>确保窗口已创建并显示（不会抢焦点）。</summary>
	public void EnsureVisible()
	{
		if (visible && IsHandleCreated)
		{
			return;
		}
		if (!IsHandleCreated)
		{
			IntPtr handle = Handle;
			GC.KeepAlive(handle);
		}
		if (!Visible)
		{
			Show();
		}
		visible = true;
	}

	/// <summary>把一张带 alpha 的位图贴到屏幕指定位置。</summary>
	public void Render(Bitmap bitmap, Point origin)
	{
		if (bitmap == null || !IsHandleCreated)
		{
			return;
		}
		IntPtr screenDc = IntPtr.Zero;
		IntPtr memDc = IntPtr.Zero;
		IntPtr hBitmap = IntPtr.Zero;
		IntPtr oldBitmap = IntPtr.Zero;
		try
		{
			screenDc = GetDC(IntPtr.Zero);
			if (screenDc == IntPtr.Zero)
			{
				return;
			}
			memDc = CreateCompatibleDC(screenDc);
			if (memDc == IntPtr.Zero)
			{
				return;
			}
			hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
			oldBitmap = SelectObject(memDc, hBitmap);
			NativeSize size = new NativeSize
			{
				Width = bitmap.Width,
				Height = bitmap.Height
			};
			NativePoint destination = new NativePoint
			{
				X = origin.X,
				Y = origin.Y
			};
			NativePoint source = new NativePoint
			{
				X = 0,
				Y = 0
			};
			BlendFunction blend = new BlendFunction
			{
				BlendOp = 0,
				BlendFlags = 0,
				SourceConstantAlpha = 255,
				AlphaFormat = 1
			};
			UpdateLayeredWindow(Handle, screenDc, ref destination, ref size, memDc, ref source, 0, ref blend, ULW_ALPHA);
		}
		catch (ExternalException)
		{
		}
		catch (ArgumentException)
		{
		}
		finally
		{
			if (memDc != IntPtr.Zero && oldBitmap != IntPtr.Zero)
			{
				SelectObject(memDc, oldBitmap);
			}
			if (hBitmap != IntPtr.Zero)
			{
				DeleteObject(hBitmap);
			}
			if (memDc != IntPtr.Zero)
			{
				DeleteDC(memDc);
			}
			if (screenDc != IntPtr.Zero)
			{
				ReleaseDC(IntPtr.Zero, screenDc);
			}
		}
	}

	/// <summary>清空覆盖层（贴一张全透明图）。</summary>
	public void Clear()
	{
		if (!IsHandleCreated)
		{
			return;
		}
		using (Bitmap empty = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
		{
			Render(empty, new Point(0, 0));
		}
	}

	/// <summary>取前台窗口矩形；若前台窗口不属于指定进程则返回 false。</summary>
	public static bool TryGetForegroundRect(int processId, out Rectangle rect)
	{
		rect = Rectangle.Empty;
		IntPtr window = GetForegroundWindow();
		if (window == IntPtr.Zero)
		{
			return false;
		}
		uint owner;
		GetWindowThreadProcessId(window, out owner);
		if (processId > 0 && (int)owner != processId)
		{
			return false;
		}
		NativeRect bounds;
		if (!GetWindowRect(window, out bounds))
		{
			return false;
		}
		int width = bounds.Right - bounds.Left;
		int height = bounds.Bottom - bounds.Top;
		if (width <= 0 || height <= 0)
		{
			return false;
		}
		rect = new Rectangle(bounds.Left, bounds.Top, width, height);
		return true;
	}

	/// <summary>主屏幕矩形（覆盖层兜底位置）。</summary>
	public static Rectangle PrimaryScreenRect()
	{
		Screen screen = Screen.PrimaryScreen;
		if (screen == null)
		{
			return new Rectangle(0, 0, 1920, 1080);
		}
		return screen.Bounds;
	}

	protected override void SetVisibleCore(bool value)
	{
		base.SetVisibleCore(value);
		visible = value;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeRect
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct NativePoint
	{
		public int X;

		public int Y;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeSize
	{
		public int Width;

		public int Height;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct BlendFunction
	{
		public byte BlendOp;

		public byte BlendFlags;

		public byte SourceConstantAlpha;

		public byte AlphaFormat;
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref NativePoint pptDst, ref NativeSize psize, IntPtr hdcSrc, ref NativePoint pptSrc, int crKey, ref BlendFunction pblend, int dwFlags);

	[DllImport("user32.dll")]
	private static extern IntPtr GetDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

	[DllImport("gdi32.dll")]
	private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteDC(IntPtr hdc);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
}
