using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>
/// 全局热键宿主：一个隐藏的原生窗口，用 RegisterHotKey 注册系统级热键。
/// 用来切换覆盖层显示 / 隐藏。
/// </summary>
internal sealed class HotkeyWindow60 : NativeWindow, IDisposable
{
	private const int WM_HOTKEY = 0x0312;

	private const int HotkeyId = 0xB60;

	private const uint MOD_NOREPEAT = 0x4000;

	private bool registered;

	private int currentKey;

	/// <summary>热键被按下时触发。</summary>
	public event Action Triggered;

	public HotkeyWindow60()
	{
		CreateHandle(new CreateParams
		{
			Caption = "Bf6EspHotkey",
			X = 0,
			Y = 0,
			Width = 0,
			Height = 0,
			Style = unchecked((int)0x80000000),
			ExStyle = 0
		});
	}

	/// <summary>当前已注册的热键名称。</summary>
	public string CurrentName { get; private set; }

	/// <summary>注册全局热键；成功返回 true。</summary>
	public bool Register(string name)
	{
		Unregister();
		Keys key = ResolveKey(name);
		if (key == Keys.None)
		{
			return false;
		}
		if (!RegisterHotKey(Handle, HotkeyId, MOD_NOREPEAT, (uint)key))
		{
			return false;
		}
		registered = true;
		currentKey = (int)key;
		CurrentName = name;
		return true;
	}

	public void Unregister()
	{
		if (registered)
		{
			UnregisterHotKey(Handle, HotkeyId);
			registered = false;
			currentKey = 0;
			CurrentName = null;
		}
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
		{
			Action handler = Triggered;
			if (handler != null)
			{
				try
				{
					handler();
				}
				catch (Exception)
				{
				}
			}
		}
		base.WndProc(ref m);
	}

	/// <summary>把热键名称转成 Keys。</summary>
	public static Keys ResolveKey(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return Keys.None;
		}
		switch (name.Trim().ToLowerInvariant())
		{
			case "end":
				return Keys.End;
			case "f1":
				return Keys.F1;
			case "insert":
				return Keys.Insert;
			case "home":
				return Keys.Home;
			case "pause":
				return Keys.Pause;
			case "delete":
				return Keys.Delete;
			default:
				return Keys.None;
		}
	}

	/// <summary>当前注册的热键值（未注册时为 0）。</summary>
	public int CurrentKey
	{
		get { return currentKey; }
	}

	public void Dispose()
	{
		Unregister();
		if (Handle != IntPtr.Zero)
		{
			DestroyHandle();
		}
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
