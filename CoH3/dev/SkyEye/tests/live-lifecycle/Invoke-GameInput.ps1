param(
    [Parameter(Mandatory=$true)][int]$TargetProcessId,
    [Parameter(Mandatory=$true)][string]$ExpectedStartUtc,
    [Parameter(Mandatory=$true)][ValidateSet('Click','Key')][string]$Action,
    [double]$X = 0,
    [double]$Y = 0,
    [int]$VirtualKey = 0,
    [Parameter(Mandatory=$true)][string]$Output
)
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $Output) { throw 'Input evidence output already exists.' }
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class CoH3UiInput {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X, Y; }
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hwnd, int command);
    [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint from, uint to, bool attach);
    [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr hwnd, out RECT rectangle);
    [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr hwnd, ref POINT point);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] public static extern void keybd_event(byte key, byte scan, uint flags, UIntPtr extra);
}
'@
[void][CoH3UiInput]::SetProcessDPIAware()
$target = Get-Process -Id $TargetProcessId
if ($target.ProcessName -ne 'RelicCoH3') { throw 'Selected process is not RelicCoH3.' }
$started = $target.StartTime.ToUniversalTime().ToString('O')
if ($started -ne $ExpectedStartUtc) { throw ('Process start identity changed: ' + $started) }
$window = $target.MainWindowHandle
if ($window -eq [IntPtr]::Zero) { throw 'Game has no main window.' }
$rect = New-Object CoH3UiInput+RECT
if (-not [CoH3UiInput]::GetClientRect($window, [ref]$rect)) { throw 'GetClientRect failed.' }
$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top
if ($width -le 0 -or $height -le 0) { throw 'Game client rectangle is empty.' }
if ([CoH3UiInput]::IsIconic($window)) { [void][CoH3UiInput]::ShowWindowAsync($window, 9) }
$focusMethod = 'SetForegroundWindow'
[void][CoH3UiInput]::SetForegroundWindow($window)
$foreground = [CoH3UiInput]::GetForegroundWindow()
[uint32]$foregroundProcess = 0
$foregroundThread = [CoH3UiInput]::GetWindowThreadProcessId($foreground, [ref]$foregroundProcess)
if ($foregroundProcess -ne $TargetProcessId) {
    $callingThread = [CoH3UiInput]::GetCurrentThreadId()
    $attached = $false
    if ($foregroundThread -ne 0 -and $foregroundThread -ne $callingThread) {
        $attached = [CoH3UiInput]::AttachThreadInput($callingThread, $foregroundThread, $true)
    }
    try {
        [void][CoH3UiInput]::BringWindowToTop($window)
        [void][CoH3UiInput]::SetForegroundWindow($window)
        $focusMethod = 'AttachThreadInput/BringWindowToTop/SetForegroundWindow'
    } finally {
        if ($attached) { [void][CoH3UiInput]::AttachThreadInput($callingThread, $foregroundThread, $false) }
    }
    $foreground = [CoH3UiInput]::GetForegroundWindow()
    [void][CoH3UiInput]::GetWindowThreadProcessId($foreground, [ref]$foregroundProcess)
}
if ($foregroundProcess -ne $TargetProcessId) { throw ('Game did not obtain foreground; no input sent. foregroundPid=' + $foregroundProcess) }
$point = New-Object CoH3UiInput+POINT
if ($Action -eq 'Click') {
    if ($X -lt 0 -or $X -ge 2560 -or $Y -lt 0 -or $Y -ge 1440) { throw 'Click is outside the 2560x1440 reference image.' }
    $point.X = [int]($X * $width / 2560)
    $point.Y = [int]($Y * $height / 1440)
    if (-not [CoH3UiInput]::ClientToScreen($window, [ref]$point)) { throw 'ClientToScreen failed.' }
    if (-not [CoH3UiInput]::SetCursorPos($point.X, $point.Y)) { throw 'SetCursorPos failed.' }
    [CoH3UiInput]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    [CoH3UiInput]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
} else {
    if ($VirtualKey -lt 1 -or $VirtualKey -gt 254) { throw 'VirtualKey is outside 1..254.' }
    [CoH3UiInput]::keybd_event([byte]$VirtualKey, 0, 0, [UIntPtr]::Zero)
    [CoH3UiInput]::keybd_event([byte]$VirtualKey, 0, 2, [UIntPtr]::Zero)
}
$record = [ordered]@{
    utc=[datetime]::UtcNow.ToString('O');pid=$TargetProcessId;startTimeUtc=$started;hwnd=$window.ToInt64()
    executable=$target.Path;action=$Action;referenceX=$X;referenceY=$Y;virtualKey=$VirtualKey
    clientWidth=$width;clientHeight=$height;screenX=$point.X;screenY=$point.Y
    foregroundPid=$foregroundProcess;focusMethod=$focusMethod;inputSent=$true;resultingGameState='not_evaluated'
}
$jsonText = $record | ConvertTo-Json -Depth 4
[IO.File]::WriteAllText([IO.Path]::GetFullPath($Output), $jsonText, (New-Object Text.UTF8Encoding($false)))
$jsonText
