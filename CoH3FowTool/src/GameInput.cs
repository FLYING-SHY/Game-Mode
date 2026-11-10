using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CoH3FowTool
{
    /// <summary>Checked Win32 keyboard input. API acceptance does not prove game execution.</summary>
    internal static class GameInput
    {
        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_UNICODE = 0x0004;
        const uint KEYEVENTF_SCANCODE = 0x0008;
        const ushort SC_CONTROL = 0x1D, SC_SHIFT = 0x2A, SC_ALT = 0x38;
        const ushort SC_GRAVE = 0x29, SC_RETURN = 0x1C;

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        // MOUSEINPUT is the largest member of the native union, even when only
        // keyboard input is sent. Omitting it shrinks INPUT from 40 to 32 bytes
        // on x64 and makes SendInput fail with ERROR_INVALID_PARAMETER (87).
        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx, dy;
            public uint mouseData, dwFlags, time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk, wScan;
            public uint dwFlags, time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL, wParamH;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hwnd, int command);
        [DllImport("user32.dll")] static extern bool IsIconic(IntPtr hwnd);
        [DllImport("user32.dll")] static extern bool IsWindow(IntPtr hwnd);
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr pid);
        [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")] static extern bool AttachThreadInput(uint from, uint to, bool attach);
        [DllImport("user32.dll")] static extern IntPtr GetKeyboardLayout(uint thread);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScanExW(char ch, IntPtr layout);
        [DllImport("user32.dll")] static extern uint MapVirtualKeyExW(uint code, uint mapType, IntPtr layout);
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int key);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr LoadKeyboardLayoutW(string id, uint flags);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessageTimeoutW(IntPtr hwnd, uint msg, UIntPtr wp, IntPtr lp,
            uint flags, uint timeout, out UIntPtr result);

        // An active Chinese IME can turn physical Enter into VK_PROCESSKEY and
        // consume it. Keep the target window on an English layout for the whole
        // command (including Enter), then restore its previous layout.
        public static IDisposable UseEnglishKeyboard(IntPtr hwnd) => new KeyboardLayoutScope(hwnd);

        sealed class KeyboardLayoutScope : IDisposable
        {
            readonly IntPtr _hwnd, _previous;
            bool _changed;
            public KeyboardLayoutScope(IntPtr hwnd)
            {
                RequireForeground(hwnd);
                RequireNoHeldModifiers();
                _hwnd = hwnd;
                _previous = GetKeyboardLayout(GetWindowThreadProcessId(hwnd, IntPtr.Zero));
                if ((_previous.ToInt64() & 0xFFFF) == 0x409) return;
                IntPtr english = LoadKeyboardLayoutW("00000409", 0);
                if (english == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error(), "LoadKeyboardLayout failed");
                _changed = true;
                try
                {
                    ChangeLayout(english);
                    if ((GetKeyboardLayout(GetWindowThreadProcessId(hwnd, IntPtr.Zero)).ToInt64() & 0xFFFF) != 0x409)
                        throw new InvalidOperationException("目标窗口未切换至英文键盘布局，已停止输入命令。");
                }
                catch { Dispose(); throw; }
            }
            void ChangeLayout(IntPtr layout)
            {
                if (SendMessageTimeoutW(_hwnd, 0x50, UIntPtr.Zero, layout, 3, 1500, out _) == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "WM_INPUTLANGCHANGEREQUEST failed");
                Thread.Sleep(100);
            }
            public void Dispose()
            {
                if (!_changed) return;
                _changed = false;
                if (IsWindow(_hwnd))
                {
                    try { ChangeLayout(_previous); }
                    catch (Exception ex) { Console.WriteLine("键盘布局恢复失败: " + ex.Message); }
                }
            }
        }

        public static int NativeInputSize => Marshal.SizeOf<INPUT>();
        public static bool IsForeground(IntPtr hwnd) => hwnd != IntPtr.Zero && GetForegroundWindow() == hwnd;

        public static bool TryGetGameWindow(out IntPtr hwnd, out int pid)
        {
            hwnd = IntPtr.Zero;
            pid = 0;
            var procs = Process.GetProcessesByName("RelicCoH3");
            try
            {
                foreach (var p in procs)
                {
                    p.Refresh();
                    if (p.MainWindowHandle == IntPtr.Zero) continue;
                    hwnd = p.MainWindowHandle;
                    pid = p.Id;
                    return true;
                }
            }
            finally { foreach (var p in procs) p.Dispose(); }
            return false;
        }

        public static bool Focus(IntPtr hwnd)
        {
            if (!IsWindow(hwnd)) return false;
            if (IsIconic(hwnd)) ShowWindow(hwnd, 9);
            uint ours = GetCurrentThreadId();
            uint foreground = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
            bool attached = foreground != 0 && foreground != ours && AttachThreadInput(ours, foreground, true);
            try { SetForegroundWindow(hwnd); }
            finally { if (attached) AttachThreadInput(ours, foreground, false); }
            for (int i = 0; i < 10; i++)
            {
                if (IsForeground(hwnd)) return true;
                Thread.Sleep(30);
            }
            return false;
        }

        static void RequireForeground(IntPtr hwnd)
        {
            if (!IsWindow(hwnd) || !IsForeground(hwnd))
                throw new InvalidOperationException("目标窗口不在前台，已停止发送按键。");
        }

        static INPUT Scan(ushort scan, bool up = false)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                U = new InputUnion { ki = new KEYBDINPUT
                {
                    wScan = (ushort)(scan & 0xFF),
                    dwFlags = KEYEVENTF_SCANCODE | (up ? KEYEVENTF_KEYUP : 0u)
                        | ((scan & 0xFF00) != 0 ? KEYEVENTF_EXTENDEDKEY : 0u)
                } }
            };
        }

        static void SendChecked(params INPUT[] inputs)
        {
            int size = NativeInputSize;
            int expected = IntPtr.Size == 8 ? 40 : 28;
            if (size != expected)
                throw new InvalidOperationException($"INPUT ABI mismatch: {size}, expected {expected}");
            Marshal.SetLastPInvokeError(0);
            uint sent = SendInput((uint)inputs.Length, inputs, size);
            if (sent != inputs.Length)
            {
                int error = Marshal.GetLastPInvokeError();
                throw new Win32Exception(error,
                    $"SendInput accepted {sent}/{inputs.Length} events (INPUT={size}, Win32={error}).");
            }
        }

        static void Release(params ushort[] scans)
        {
            // Always release our own pressed keys, even if focus changes mid-chord.
            foreach (ushort scan in scans)
            {
                try { SendChecked(Scan(scan, true)); } catch { }
            }
        }

        static void Tap(IntPtr hwnd, ushort scan)
        {
            RequireForeground(hwnd);
            SendChecked(Scan(scan));
            try { Thread.Sleep(45); }
            finally { SendChecked(Scan(scan, true)); }
            Thread.Sleep(20);
        }

        public static void RequireNoHeldModifiers()
        {
            foreach (int vk in new[] { 0x10, 0x11, 0x12, 0x5B, 0x5C })
                if ((GetAsyncKeyState(vk) & 0x8000) != 0)
                    throw new InvalidOperationException("检测到键盘修饰键仍被按住，请松开后重试。");
        }

        public static void SendConsoleHotkey(IntPtr hwnd)
        {
            RequireForeground(hwnd);
            RequireNoHeldModifiers();
            try
            {
                SendChecked(Scan(SC_CONTROL), Scan(SC_SHIFT));
                Thread.Sleep(70);
                Tap(hwnd, SC_GRAVE);
            }
            finally { Release(SC_SHIFT, SC_CONTROL); }
        }

        /// <summary>Use physical scan codes for ASCII; Unicode packets for other text.</summary>
        public static void SendText(IntPtr hwnd, string text)
        {
            RequireForeground(hwnd);
            RequireNoHeldModifiers();
            IntPtr layout = GetKeyboardLayout(GetWindowThreadProcessId(hwnd, IntPtr.Zero));
            foreach (char ch in text)
            {
                RequireForeground(hwnd);
                if (ch > 127)
                {
                    var down = new INPUT { type = INPUT_KEYBOARD,
                        U = new InputUnion { ki = new KEYBDINPUT { wScan = ch, dwFlags = KEYEVENTF_UNICODE } } };
                    var up = down;
                    up.U.ki.dwFlags |= KEYEVENTF_KEYUP;
                    SendChecked(down, up);
                    continue;
                }
                short mapped = VkKeyScanExW(ch, layout);
                if (mapped == -1)
                    throw new InvalidOperationException($"字符 U+{(int)ch:X4} 在目标键盘布局中没有按键映射。");
                int modifiers = (mapped >> 8) & 0xFF;
                uint scan = MapVirtualKeyExW((uint)(mapped & 0xFF), 4, layout);
                if (scan == 0 || (modifiers & ~7) != 0)
                    throw new InvalidOperationException($"字符 U+{(int)ch:X4} 的扫描码映射失败。");
                try
                {
                    if ((modifiers & 1) != 0) SendChecked(Scan(SC_SHIFT));
                    if ((modifiers & 2) != 0) SendChecked(Scan(SC_CONTROL));
                    if ((modifiers & 4) != 0) SendChecked(Scan(SC_ALT));
                    if (modifiers != 0) Thread.Sleep(35);
                    Tap(hwnd, (ushort)scan);
                }
                finally
                {
                    if ((modifiers & 4) != 0) Release(SC_ALT);
                    if ((modifiers & 2) != 0) Release(SC_CONTROL);
                    if ((modifiers & 1) != 0) Release(SC_SHIFT);
                }
            }
        }

        public static void SendEnter(IntPtr hwnd) => Tap(hwnd, SC_RETURN);
    }
}
