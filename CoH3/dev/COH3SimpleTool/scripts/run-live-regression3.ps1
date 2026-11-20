param()
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing,System.Windows.Forms
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Text;
public static class LiveReg3 {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X, Y; }
    public delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc cb, IntPtr lParam);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern int GetWindowTextW(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder text, int count);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT rectangle);
    [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr hwnd, ref POINT point);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
}
'@
[LiveReg3]::SetProcessDPIAware() | Out-Null

$root = 'F:\WorkSpace\Game\CoH3\dev\COH3SimpleTool'
$exe = 'F:\WorkSpace\Game\CoH3\output\COH3SimpleTool\COH3工具.exe'
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$evid = Join-Path $root ("evidence\live-regression3-" + $stamp)
New-Item -ItemType Directory -Force -Path $evid | Out-Null
$privateDir = 'F:\WorkSpace\Game\LicensingSystem\.local\hardening-v2\cards-simpletool'
$code = (Get-Content (Join-Path $privateDir 'regression-permanent.txt') -Raw).Trim()
$log = @()

if (-not (Get-Process RelicCoH3 -ErrorAction SilentlyContinue)) { throw 'RelicCoH3 not running' }

function Find-AppWindow([int]$procId, [int]$seconds, [scriptblock]$predicate) {
    foreach ($i in 1..($seconds * 4)) {
        Start-Sleep -Milliseconds 250
        $found = [IntPtr]::Zero
        $cb = [LiveReg3+EnumProc]{ param($hwnd, $lparam)
            $wpid = 0
            [LiveReg3]::GetWindowThreadProcessId($hwnd, [ref]$wpid) | Out-Null
            if ($wpid -eq $script:procId -and [LiveReg3]::IsWindowVisible($hwnd)) {
                $sb = New-Object System.Text.StringBuilder 256
                [LiveReg3]::GetWindowTextW($hwnd, $sb, 256) | Out-Null
                $t = $sb.ToString()
                if ($t.Length -gt 0 -and ($null -eq $script:pred -or (& $script:pred $t))) { $script:found = $hwnd; return $false }
            }
            return $true
        }
        $script:procId = $procId; $script:pred = $predicate; $script:found = [IntPtr]::Zero
        [LiveReg3]::EnumWindows($cb, [IntPtr]::Zero) | Out-Null
        if ($script:found -ne [IntPtr]::Zero) { return $script:found }
    }
    return [IntPtr]::Zero
}
function Test-AnyWindow([int]$procId, [scriptblock]$predicate) {
    $script:procId = $procId; $script:pred = $predicate; $script:found = [IntPtr]::Zero
    $cb = [LiveReg3+EnumProc]{ param($hwnd, $lparam)
        $wpid = 0
        [LiveReg3]::GetWindowThreadProcessId($hwnd, [ref]$wpid) | Out-Null
        if ($wpid -eq $script:procId -and [LiveReg3]::IsWindowVisible($hwnd)) {
            $sb = New-Object System.Text.StringBuilder 256
            [LiveReg3]::GetWindowTextW($hwnd, $sb, 256) | Out-Null
            $t = $sb.ToString()
            if ($t.Length -gt 0 -and (& $script:pred $t)) { $script:found = $hwnd; return $false }
        }
        return $true
    }
    [LiveReg3]::EnumWindows($cb, [IntPtr]::Zero) | Out-Null
    return ($script:found -ne [IntPtr]::Zero)
}
function Wait-NoWindow([int]$procId, [int]$seconds, [scriptblock]$predicate) {
    foreach ($i in 1..($seconds * 4)) {
        Start-Sleep -Milliseconds 250
        if (-not (Test-AnyWindow $procId $predicate)) { return $true }
    }
    return $false
}
function Click-Client([IntPtr]$hwnd, [int]$cx, [int]$cy) {
    $pt = New-Object LiveReg3+POINT
    $pt.X = $cx; $pt.Y = $cy
    [LiveReg3]::ClientToScreen($hwnd, [ref]$pt) | Out-Null
    [LiveReg3]::SetForegroundWindow($hwnd) | Out-Null
    Start-Sleep -Milliseconds 200
    [LiveReg3]::SetCursorPos($pt.X, $pt.Y) | Out-Null
    Start-Sleep -Milliseconds 150
    [LiveReg3]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    [LiveReg3]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 300
}
function Capture-Hwnd([IntPtr]$hwnd, [string]$name) {
    $sb = New-Object System.Text.StringBuilder 256
    [LiveReg3]::GetWindowTextW($hwnd, $sb, 256) | Out-Null
    $title = $sb.ToString()
    $element = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
    $texts = @()
    $kids = $element.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    for ($i = 0; $i -lt [Math]::Min($kids.Count, 200); $i++) {
        try {
            $cur = $kids.Item($i).Current
            if (-not $cur.IsPassword -and $cur.ControlType -ne [System.Windows.Automation.ControlType]::Edit -and $cur.Name) { $texts += $cur.Name }
        } catch {}
    }
    $rect = New-Object LiveReg3+RECT
    [LiveReg3]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
    $w = $rect.Right - $rect.Left; $h = $rect.Bottom - $rect.Top
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $hdc = $g.GetHdc()
    $printed = [LiveReg3]::PrintWindow($hwnd, $hdc, 2)
    $g.ReleaseHdc($hdc); $g.Dispose()
    if ($printed) { $bmp.Save((Join-Path $evid ($name + '.png')), [System.Drawing.Imaging.ImageFormat]::Png) }
    $bmp.Dispose()
    $script:log += [pscustomobject]@{ name = $name; title = $title; width = $w; height = $h; screenshot = $printed; texts = $texts }
}

# fresh state + credentials backup
Get-Process | Where-Object { $_.Path -eq $exe } | ForEach-Object { Stop-Process -Id $_.Id -Force }
$cred = Join-Path $env:LOCALAPPDATA 'Skyeye\SelfHostedLicensing\credentials.dat'
$credBackup = Join-Path $privateDir 'credentials-pre-regression.dat'
$credExisted = Test-Path $cred
if ($credExisted) { Copy-Item $cred $credBackup -Force }
$settingsFile = Join-Path (Split-Path $exe) 'overlay-settings.txt'

$proc = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -PassThru
Start-Sleep -Seconds 8
$h = Find-AppWindow $proc.Id 20 { $true }
if ($h -eq [IntPtr]::Zero) { throw 'no window' }
Capture-Hwnd $h '01-startup'

# toggle persistence quick check: click toggle 1, expect enabled flip in overlay-settings.txt
$toggleBefore = if (Test-Path $settingsFile) { (Select-String -Path $settingsFile -Pattern '^enabled=' | Select-Object -First 1).Line } else { 'absent' }
Click-Client $h 116 67
Start-Sleep -Milliseconds 600
$toggleMid = (Select-String -Path $settingsFile -Pattern '^enabled=' | Select-Object -First 1).Line
Click-Client $h 116 67
Start-Sleep -Milliseconds 600
$toggleAfter = (Select-String -Path $settingsFile -Pattern '^enabled=' | Select-Object -First 1).Line
$toggleOk = ($toggleMid -ne $toggleBefore) -and ($toggleAfter -eq $toggleBefore)

# click exit -> expect login window AND control bar hidden
Click-Client $h 611 22
$hLogin = Find-AppWindow $proc.Id 20 { param($t) $t -match '激活' }
$exitToLogin = ($hLogin -ne [IntPtr]::Zero)
if ($exitToLogin) { Capture-Hwnd $hLogin '02-back-to-login' }
$barHidden = Wait-NoWindow $proc.Id 10 { param($t) $t -notmatch '激活' }

# paste test card (clear prefilled user code first)
$activated = $false
if ($exitToLogin -and $barHidden) {
    Start-Sleep -Milliseconds 500
    $code | Set-Clipboard
    Click-Client $hLogin 200 63
    [System.Windows.Forms.SendKeys]::SendWait('^a')
    Start-Sleep -Milliseconds 150
    [System.Windows.Forms.SendKeys]::SendWait('^v')
    Start-Sleep -Milliseconds 400
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    $hBar = Find-AppWindow $proc.Id 40 { param($t) $t -notmatch '激活' }
    $loginGone = Wait-NoWindow $proc.Id 10 { param($t) $t -match '激活' }
    $activated = ($hBar -ne [IntPtr]::Zero) -and $loginGone
    if ($hBar -ne [IntPtr]::Zero) { Capture-Hwnd $hBar '03-activated-testcard' }
}

# graceful close via x on control bar
$closed = 'not-attempted'
if ($activated) {
    Click-Client $hBar 703 22
    if ($proc.WaitForExit(20)) { $closed = 'graceful' } else { Stop-Process -Id $proc.Id -Force; $closed = 'forced' }
} else {
    if ($proc.WaitForExit(5)) { $closed = 'closed-inactive' } else { Stop-Process -Id $proc.Id -Force; $closed = 'forced' }
}

# restore credentials
if ($credExisted) { Copy-Item $credBackup $cred -Force; $credRestored = $true } else { $credRestored = 'was-absent' }

[pscustomobject]@{ stamp = $stamp; exitToLogin = $exitToLogin; barHiddenAfterExit = $barHidden; activatedWithTestCard = $activated; closedVia = $closed; togglePersistence = @{ before = $toggleBefore; mid = $toggleMid; after = $toggleAfter; ok = $toggleOk }; credentialsRestored = $credRestored; captures = $log } | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $evid 'summary.json') -Encoding UTF8
[pscustomobject]@{ stamp = $stamp; exitToLogin = $exitToLogin; barHidden = $barHidden; activated = $activated; closed = $closed; toggleOk = $toggleOk } | ConvertTo-Json -Compress
