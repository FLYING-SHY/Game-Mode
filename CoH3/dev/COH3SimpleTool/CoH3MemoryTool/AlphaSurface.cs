using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CoH3MemoryTool;

internal sealed class AlphaSurface : IDisposable
{
	private struct Header
	{
		public uint Size;

		public int Width;

		public int Height;

		public ushort Planes;

		public ushort BitCount;

		public uint Compression;

		public uint SizeImage;

		public int XPels;

		public int YPels;

		public uint Used;

		public uint Important;
	}

	private struct Blend
	{
		public byte Op;

		public byte Flags;

		public byte Alpha;

		public byte Format;
	}

	private IntPtr dc;

	private IntPtr dib;

	private IntPtr old;

	private IntPtr bits;

	public Bitmap Image { get; private set; }

	public Graphics Graphics { get; private set; }

	[DllImport("user32.dll")]
	private static extern IntPtr GetDC(IntPtr w);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(IntPtr w, IntPtr dc);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleDC(IntPtr dc);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteDC(IntPtr dc);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateDIBSection(IntPtr dc, ref Header info, uint usage, out IntPtr bits, IntPtr section, uint offset);

	[DllImport("gdi32.dll")]
	private static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr obj);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool UpdateLayeredWindow(IntPtr window, IntPtr screen, ref Point destination, ref Size size, IntPtr source, ref Point origin, int key, ref Blend blend, int flags);

	public void Ensure(int width, int height)
	{
		if (Image != null && Image.Width == width && Image.Height == height)
		{
			return;
		}
		Clear();
		IntPtr dC = GetDC(IntPtr.Zero);
		try
		{
			dc = CreateCompatibleDC(dC);
			Header info = new Header
			{
				Size = (uint)Marshal.SizeOf(typeof(Header)),
				Width = width,
				Height = -height,
				Planes = 1,
				BitCount = 32
			};
			dib = CreateDIBSection(dC, ref info, 0u, out bits, IntPtr.Zero, 0u);
			if (dc == IntPtr.Zero || dib == IntPtr.Zero)
			{
				throw new Exception("无法创建半透明绘制层。");
			}
			old = SelectObject(dc, dib);
			Image = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, bits);
			Graphics = Graphics.FromImage(Image);
		}
		catch
		{
			Clear();
			throw;
		}
		finally
		{
			ReleaseDC(IntPtr.Zero, dC);
		}
	}

	public void Present(IntPtr window, Point destination)
	{
		Graphics.Flush(FlushIntention.Sync);
		Size size = Image.Size;
		Point origin = Point.Empty;
		Blend blend = new Blend
		{
			Alpha = byte.MaxValue,
			Format = 1
		};
		if (!UpdateLayeredWindow(window, IntPtr.Zero, ref destination, ref size, dc, ref origin, 0, ref blend, 2))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}

	private void Clear()
	{
		if (Graphics != null)
		{
			Graphics.Dispose();
			Graphics = null;
		}
		if (Image != null)
		{
			Image.Dispose();
			Image = null;
		}
		if (dc != IntPtr.Zero && old != IntPtr.Zero)
		{
			SelectObject(dc, old);
		}
		if (dib != IntPtr.Zero)
		{
			DeleteObject(dib);
		}
		if (dc != IntPtr.Zero)
		{
			DeleteDC(dc);
		}
		dc = (dib = (old = (bits = IntPtr.Zero)));
	}

	public void Dispose()
	{
		Clear();
	}
}
