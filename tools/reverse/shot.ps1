param(
  [string]$Out = "C:\Users\21642\Documents\deepseek-harness\default-workspace\CoH3FowTool\tools\reverse\shot.png",
  [int]$ProcId = 0
)
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public struct RECT { public int Left, Top, Right, Bottom; }
public class W {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern int GetWindowTextLength(IntPtr h);
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, System.Text.StringBuilder s, int n);
}
"@
$p = if ($ProcId -gt 0) { Get-Process -Id $ProcId } else { Get-Process -Name RelicCoH3 -ErrorAction SilentlyContinue | Select-Object -First 1 }
if (-not $p) { Write-Output "no process"; exit 1 }
$h = $p.MainWindowHandle
$sb = New-Object System.Text.StringBuilder 512
[void][W]::GetWindowText($h, $sb, 512)
Write-Output ("hwnd=0x{0:X}  title='{1}'" -f $h.ToInt64(), $sb.ToString())
Write-Output ("foreground=0x{0:X}  isGame={1}" -f ([W]::GetForegroundWindow()).ToInt64(), ([W]::GetForegroundWindow() -eq $h))
$r = New-Object RECT
[void][W]::GetWindowRect($h, [ref]$r)
$w = $r.Right - $r.Left; $ht = $r.Bottom - $r.Top
Write-Output ("rect={0},{1} {2}x{3}" -f $r.Left, $r.Top, $w, $ht)
if ($w -le 0 -or $ht -le 0) { Write-Output "bad rect"; exit 1 }
$bmp = New-Object System.Drawing.Bitmap $w, $ht
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($r.Left, $r.Top, 0, 0, (New-Object System.Drawing.Size $w, $ht))
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output "saved $Out"
