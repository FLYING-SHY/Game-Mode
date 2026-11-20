param()
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing,System.Windows.Forms
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class LiveRegression {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X, Y; }
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT rectangle);
    [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr hwnd, ref POINT point);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
}
'@
[LiveRegression]::SetProcessDPIAware() | Out-Null

$root = 'F:\WorkSpace\Game\CoH3\dev\COH3SimpleTool'
$outDir = 'F:\WorkSpace\Game\CoH3\output\COH3SimpleTool'
$exe = Join-Path $outDir 'COH3工具.exe'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$evid = Join-Path $root ("evidence\live-regression-" + $stamp)
New-Item -ItemType Directory -Force -Path $evid | Out-Null
$privateDir = 'F:\WorkSpace\Game\LicensingSystem\.local\hardening-v2\cards-simpletool'
$settingsFile = Join-Path $outDir 'overlay-settings.txt'
$code = (Get-Content (Join-Path $privateDir 'regression-permanent.txt') -Raw).Trim()

function Wait-MainWindow([int]$pid_, [int]$seconds) {
    foreach ($i in 1..($seconds * 2)) {
        Start-Sleep -Milliseconds 500
        $p = Get-Process -Id $pid_ -ErrorAction SilentlyContinue
        if ($p -and $p.MainWindowHandle -ne 0) { return $p }
    }
    return $null
}
function Wait-Title([int]$pid_, [string]$match, [int]$seconds) {
    foreach ($i in 1..($seconds * 2)) {
        Start-Sleep -Milliseconds 500
        $p = Get-Process -Id $pid_ -ErrorAction SilentlyContinue
        if ($p) { $p.Refresh(); if ($p.MainWindowTitle -and $p.MainWindowTitle -match $match) { return $p } }
    }
    return $null
}
function Click-Client([IntPtr]$hwnd, [int]$cx, [int]$cy) {
    $pt = New-Object LiveRegression+POINT
    $pt.X = $cx; $pt.Y = $cy
    [LiveRegression]::ClientToScreen($hwnd, [ref]$pt) | Out-Null
    [LiveRegression]::SetForegroundWindow($hwnd) | Out-Null
    Start-Sleep -Milliseconds 150
    [LiveRegression]::SetCursorPos($pt.X, $pt.Y) | Out-Null
    Start-Sleep -Milliseconds 120
    [LiveRegression]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    [LiveRegression]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 250
}
function Capture([int]$pid_, [string]$name) {
    $p = Get-Process -Id $pid_; $p.Refresh()
    $handle = $p.MainWindowHandle
    $element = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
    $texts = @()
    $kids = $element.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    for ($i = 0; $i -lt [Math]::Min($kids.Count, 200); $i++) {
        try {
            $cur = $kids.Item($i).Current
            if (-not $cur.IsPassword -and $cur.ControlType -ne [System.Windows.Automation.ControlType]::Edit -and $cur.Name) { $texts += $cur.Name }
        } catch {}
    }
    $rect = New-Object LiveRegression+RECT
    [LiveRegression]::GetWindowRect($handle, [ref]$rect) | Out-Null
    $w = $rect.Right - $rect.Left; $h = $rect.Bottom - $rect.Top
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $hdc = $g.GetHdc()
    $printed = [LiveRegression]::PrintWindow($handle, $hdc, 2)
    $g.ReleaseHdc($hdc); $g.Dispose()
    if ($printed) { $bmp.Save((Join-Path $evid ($name + '.png')), [System.Drawing.Imaging.ImageFormat]::Png) }
    $bmp.Dispose()
    return [pscustomobject]@{ name = $name; title = $p.MainWindowTitle; width = $w; height = $h; screenshot = $printed; texts = $texts }
}

# 0) fresh state
Get-Process | Where-Object { $_.Path -eq $exe } | ForEach-Object { Stop-Process -Id $_.Id -Force }
$cred = Join-Path $env:LOCALAPPDATA 'Skyeye\SelfHostedLicensing\credentials.dat'
$credBackup = Join-Path $privateDir 'credentials-pre-regression.dat'
$credExisted = Test-Path $cred
if ($credExisted) { Copy-Item $cred $credBackup -Force }
$log = @()

# 1) launch protected exe
$proc = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -PassThru
$p = Wait-MainWindow $proc.Id 25
if (-not $p) { throw 'no main window' }
Start-Sleep -Seconds 6   # allow startup auto-login attempt
$p.Refresh()
$log += Capture $proc.Id '01-startup'

# 2) if still on login, paste activation; if control bar (auto-login), record
$phase = if ($p.MainWindowTitle -match '激活') { 'login-shown' } else { 'autologin-controlbar' }
if ($phase -eq 'login-shown') {
    $code | Set-Clipboard
    Click-Client $p.MainWindowHandle 200 63
    [System.Windows.Forms.SendKeys]::SendWait('^v')
    Start-Sleep -Milliseconds 350
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    $p2 = Wait-Title $proc.Id '^((?!激活).)*$' 30
    if ($p2) { $log += Capture $proc.Id '02-after-paste-activation' } else { $log += Capture $proc.Id '02-activation-failed' }
} else {
    $log += Capture $proc.Id '02-autologin-controlbar'
}
$p.Refresh()

# 3) toggle persistence test (only meaningful on control bar)
if ($p.MainWindowTitle -notmatch '激活') {
    $before = if (Test-Path $settingsFile) { Get-Content $settingsFile -Raw } else { '<absent>' }
    Click-Client $p.MainWindowHandle 116 67
    Start-Sleep -Milliseconds 900
    $mid = if (Test-Path $settingsFile) { Get-Content $settingsFile -Raw } else { '<absent>' }
    Click-Client $p.MainWindowHandle 116 67
    Start-Sleep -Milliseconds 700
    $after = if (Test-Path $settingsFile) { Get-Content $settingsFile -Raw } else { '<absent>' }
    $log += Capture $proc.Id '03-controlbar-toggled'
    $script:toggleResult = [pscustomobject]@{ before = $before; mid = $mid; after = $after; changed = ($before -ne $mid) -and ($mid -ne $after); restored = ($before -eq $after) }
}

# 4) exit-to-login (ChangeLicense60) then re-activate
$p.Refresh()
if ($p.MainWindowTitle -notmatch '激活') {
    Click-Client $p.MainWindowHandle 611 22
    $p3 = Wait-Title $proc.Id '激活' 15
    if ($p3) {
        $log += Capture $proc.Id '04-back-to-login'
        Start-Sleep -Milliseconds 800
        $code | Set-Clipboard
        Click-Client $p3.MainWindowHandle 200 63
        [System.Windows.Forms.SendKeys]::SendWait('^v')
        Start-Sleep -Milliseconds 350
        [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
        $p4 = Wait-Title $proc.Id '^((?!激活).)*$' 30
        if ($p4) { $log += Capture $proc.Id '05-reactivated' }
    }
}
$p.Refresh()

# 5) graceful close
$ok = $p.CloseMainWindow()
if (-not $p.WaitForExit(15)) { Stop-Process -Id $proc.Id -Force; $closed = 'forced' } else { $closed = 'graceful' }

# 6) restore user credentials
if ($credExisted) { Copy-Item $credBackup $cred -Force; $credRestored = $true } else { $credRestored = 'was-absent' }

# 7) summary (no secrets: settings file holds only flags)
[pscustomobject]@{
    stamp = $stamp
    startupPhase = $phase
    closedVia = $closed
    credentialsRestored = $credRestored
    toggle = $script:toggleResult
    captures = $log
} | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $evid 'summary.json') -Encoding UTF8
[pscustomobject]@{ stamp = $stamp; phase = $phase; closed = $closed; toggleChanged = $script:toggleResult.changed; toggleRestored = $script:toggleResult.restored; captures = ($log | ForEach-Object { $_.name + '|' + $_.title }) } | ConvertTo-Json -Compress