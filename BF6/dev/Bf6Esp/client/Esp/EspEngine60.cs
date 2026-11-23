using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>
/// 覆盖层引擎：定时读取实体、投影、绘制并贴到覆盖层窗口上。
/// 所有读取都走只读 MemoryReader，未授权时立即停止。
/// </summary>
internal sealed class EspEngine60 : IDisposable
{
	private readonly GameProcess game;

	private readonly MemoryReader reader;

	private readonly IEntitySource source;

	private readonly PlayerReader playerReader;

	private readonly EspSettings settings;

	private readonly OverlayWindow overlay;

	private readonly HotkeyWindow60 hotkey;

	private readonly Action<string> log;

	private readonly EspRenderer renderer = new EspRenderer();

	private readonly Timer timer = new Timer();

	private Bitmap bitmap;

	private bool running;

	public EspEngine60(GameProcess game, MemoryReader reader, IEntitySource source, EspSettings settings, OverlayWindow overlay, HotkeyWindow60 hotkey, Action<string> log)
	{
		this.game = game;
		this.reader = reader;
		this.source = source;
		this.settings = settings;
		this.overlay = overlay;
		this.hotkey = hotkey;
		this.log = log ?? delegate(string message)
		{
		};
		playerReader = new PlayerReader(source, this.log);
		timer.Interval = 16;
		timer.Tick += delegate { Tick(); };
	}

	/// <summary>热键回调：切换覆盖层显示状态。</summary>
	public void ToggleOverlay()
	{
		settings.OverlayEnabled = !settings.OverlayEnabled;
		log("[热键] 覆盖层已" + (settings.OverlayEnabled ? "显示" : "隐藏") + "。");
	}

	public void Start()
	{
		if (running)
		{
			return;
		}
		running = true;
		if (hotkey != null && !hotkey.Register(settings.HotkeyName))
		{
			log("[热键] 注册失败（可能被其它程序占用）：" + settings.HotkeyName);
		}
		timer.Start();
	}

	public void Stop()
	{
		running = false;
		timer.Stop();
		try
		{
			if (overlay != null)
			{
				overlay.Clear();
			}
		}
		catch (Exception)
		{
		}
	}

	private void Tick()
	{
		if (!running)
		{
			return;
		}
		try
		{
			if (!LicenseGate60.Allowed)
			{
				overlay.Clear();
				return;
			}
			if (!settings.OverlayEnabled)
			{
				overlay.Clear();
				return;
			}
			if (reader == null || !reader.IsValid)
			{
				overlay.Clear();
				return;
			}
			Rectangle rect;
			if (!OverlayWindow.TryGetForegroundRect(game.ProcessId, out rect))
			{
				rect = OverlayWindow.PrimaryScreenRect();
			}
			float[] viewProjection;
			if (!source.TryGetViewProjection(out viewProjection))
			{
				overlay.Clear();
				return;
			}
			List<EntitySnapshot> entities = playerReader.Read();
			EnsureBitmap(rect.Width, rect.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				renderer.Render(graphics, rect.Width, rect.Height, entities, viewProjection, settings);
			}
			overlay.EnsureVisible();
			overlay.Render(bitmap, rect.Location);
		}
		catch (Exception ex)
		{
			log("[绘制] 异常：" + ex.Message);
			try
			{
				overlay.Clear();
			}
			catch (Exception)
			{
			}
		}
	}

	private void EnsureBitmap(int width, int height)
	{
		if (bitmap != null && bitmap.Width == width && bitmap.Height == height)
		{
			return;
		}
		if (bitmap != null)
		{
			bitmap.Dispose();
			bitmap = null;
		}
		bitmap = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
	}

	public void Dispose()
	{
		Stop();
		timer.Dispose();
		if (bitmap != null)
		{
			bitmap.Dispose();
			bitmap = null;
		}
	}
}
