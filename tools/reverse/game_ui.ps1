param([Parameter(Mandatory=$true)][double]$X, [Parameter(Mandatory=$true)][double]$Y)
$ErrorActionPreference = 'Stop'
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GameUiClick {
  [StructLayout(LayoutKind.Sequential)] public struct R { public int left,top,right,bottom; }
  [DllImport("user32.dll")] static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
  [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr h,out R r);
  [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] static extern bool SetCursorPos(int x,int y);
  [DllImport("user32.dll")] static extern void mouse_event(uint flags,uint x,uint y,uint data,UIntPtr extra);
  public static void Click(IntPtr h,double x,double y) {
    if (x<0 || x>1 || y<0 || y>1) throw new ArgumentOutOfRangeException();
    SetThreadDpiAwarenessContext(new IntPtr(-4));
    SetForegroundWindow(h); System.Threading.Thread.Sleep(100);
    if(GetForegroundWindow()!=h) throw new InvalidOperationException("Game is not foreground");
    R r; if(!GetWindowRect(h,out r)) throw new InvalidOperationException("Window bounds missing");
    SetCursorPos(r.left+(int)((r.right-r.left)*x),r.top+(int)((r.bottom-r.top)*y));
    mouse_event(2,0,0,0,UIntPtr.Zero); System.Threading.Thread.Sleep(60); mouse_event(4,0,0,0,UIntPtr.Zero);
  }
}
'@
$p = Get-Process -Name RelicCoH3 -ErrorAction Stop | Select-Object -First 1
[GameUiClick]::Click($p.MainWindowHandle,$X,$Y)
