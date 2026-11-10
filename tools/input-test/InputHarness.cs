using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using CoH3FowTool;

internal static class InputHarness
{
    const uint WM_KEYDOWN = 0x100, WM_CHAR = 0x102, WM_CLOSE = 0x10, WM_DESTROY = 2;
    static readonly StringBuilder Received = new();
    static readonly WndProc Callback = WindowProc;
    static int Enters, Chords;
    static Exception Failure;

    delegate IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct WNDCLASS
    {
        public uint style;
        public WndProc proc;
        public int clsExtra, wndExtra;
        public IntPtr instance, icon, cursor, background;
        public string menu, name;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public UIntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public int x, y;
        public uint lPrivate;
    }
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] static extern ushort RegisterClassW(ref WNDCLASS wc);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] static extern IntPtr CreateWindowExW(uint ex, string cls, string title, uint style, int x, int y, int w, int h, IntPtr parent, IntPtr menu, IntPtr inst, IntPtr param);
    [DllImport("user32.dll")] static extern IntPtr DefWindowProcW(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] static extern bool DestroyWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern void PostQuitMessage(int code);
    [DllImport("user32.dll")] static extern bool PostMessageW(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);
    [DllImport("user32.dll")] static extern int GetMessageW(out MSG msg, IntPtr hwnd, uint min, uint max);
    [DllImport("user32.dll")] static extern bool TranslateMessage(ref MSG msg);
    [DllImport("user32.dll")] static extern IntPtr DispatchMessageW(ref MSG msg);
    [DllImport("user32.dll")] static extern IntPtr SetFocus(IntPtr hwnd);
    [DllImport("user32.dll")] static extern short GetKeyState(int key);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] static extern IntPtr GetModuleHandleW(string name);

    static IntPtr WindowProc(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp)
    {
        if (msg == WM_KEYDOWN || msg == 0x104 || msg == 0x290)
            Console.WriteLine($"key message=0x{msg:X} vk=0x{wp.ToInt64():X} lparam=0x{lp.ToInt64():X}");
        if (msg == WM_KEYDOWN)
        {
            if (wp.ToInt64() == 13) Enters++;
            if (wp.ToInt64() == 0xC0 && GetKeyState(0x11) < 0 && GetKeyState(0x10) < 0) Chords++;
        }
        if (msg == WM_CHAR && wp.ToInt64() >= 32) Received.Append((char)wp.ToInt32());
        if (msg == WM_CLOSE) { DestroyWindow(hwnd); return IntPtr.Zero; }
        if (msg == WM_DESTROY) { PostQuitMessage(0); return IntPtr.Zero; }
        return DefWindowProcW(hwnd, msg, wp, lp);
    }

    static int Main(string[] args)
    {
        try
        {
            Console.WriteLine($"INPUT size={GameInput.NativeInputSize}, expected=40");
            if (args.Length > 0)
            {
                if (!GameInput.TryGetGameWindow(out var game, out int pid)) throw new Exception("Game window not found");
                if (!GameInput.Focus(game)) throw new Exception("Game focus failed");
                Thread.Sleep(300);
                using var keyboard = GameInput.UseEnglishKeyboard(game);
                if (args[0] == "game-hotkey") GameInput.SendConsoleHotkey(game);
                else if (args[0] == "game-type") GameInput.SendText(game, args.Length > 1 ? args[1] : "cheat_revealall");
                else if (args[0] == "game-enter") GameInput.SendEnter(game);
                else throw new Exception("Unknown diagnostic stage");
                Console.WriteLine($"Input API accepted stage={args[0]} PID={pid}; game execution not yet checked.");
                return 0;
            }
            var wc = new WNDCLASS { proc = Callback, instance = GetModuleHandleW(null), name = "CoH3InputIntegrationTest" };
            if (RegisterClassW(ref wc) == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            IntPtr hwnd = CreateWindowExW(0, wc.name, "CoH3 tool keyboard integration test", 0x10CF0000,
                150, 150, 700, 220, IntPtr.Zero, IntPtr.Zero, wc.instance, IntPtr.Zero);
            if (hwnd == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            GameInput.Focus(hwnd);
            SetFocus(hwnd);
            var sender = Task.Run(() =>
            {
                try
                {
                    Thread.Sleep(400);
                    if (!GameInput.Focus(hwnd)) throw new Exception("Test window focus failed");
                    using var keyboard = GameInput.UseEnglishKeyboard(hwnd);
                    GameInput.SendConsoleHotkey(hwnd);
                    Thread.Sleep(150);
                    GameInput.SendText(hwnd, "cheat_revealall");
                    GameInput.SendEnter(hwnd);
                    Thread.Sleep(200);
                }
                catch (Exception ex) { Failure = ex; }
                finally { PostMessageW(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero); }
            });
            while (GetMessageW(out MSG msg, IntPtr.Zero, 0, 0) > 0)
            {
                TranslateMessage(ref msg);
                DispatchMessageW(ref msg);
            }
            sender.GetAwaiter().GetResult();
            if (Failure != null) throw Failure;
            string text = Received.ToString();
            Console.WriteLine($"Received text={text}, Enter keydowns={Enters}, Ctrl+Shift+Grave keydowns={Chords}");
            bool ok = GameInput.NativeInputSize == 40 && text == "cheat_revealall" && Enters == 1 && Chords == 1;
            Console.WriteLine(ok ? "INPUT INTEGRATION PASS" : "INPUT INTEGRATION FAIL");
            return ok ? 0 : 1;
        }
        catch (Exception ex) { Console.WriteLine(ex); return 1; }
    }
}
