param(
    [Parameter(Mandatory=$true)][int]$TargetProcessId,
    [Parameter(Mandatory=$true)][string]$OutputPrefix
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes,System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class RecoveryWindowCapture {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT rectangle);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
}
'@
$target = Get-Process -Id $TargetProcessId
$target.Refresh()
$handle = $target.MainWindowHandle
if ($handle -eq [IntPtr]::Zero) { throw 'Selected process has no main window.' }
if (Test-Path -LiteralPath ($OutputPrefix + '.json')) { throw 'Capture output already exists.' }
$window = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
$elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
$nodes = @()
for ($i=0; $i -lt [Math]::Min($elements.Count,1500); $i++) {
    try {
        $current = $elements.Item($i).Current
        $name = $current.Name
        if ($current.ControlType -eq [System.Windows.Automation.ControlType]::Edit -or $current.IsPassword) {
            $name = '[editable or password content omitted]'
        } else {
            $name = [regex]::Replace($name,'\b\d{7,}\b','[numeric identity omitted]')
            $name = [regex]::Replace($name,'\b[A-Za-z0-9_-]{24,}\b','[long identifier omitted]')
        }
        $nodes += [pscustomobject]@{
            Index=$i; Name=$name; AutomationId=$current.AutomationId;
            ControlType=$current.ControlType.ProgrammaticName; ClassName=$current.ClassName;
            IsEnabled=$current.IsEnabled; IsOffscreen=$current.IsOffscreen;
            Rectangle=[pscustomobject]@{X=$current.BoundingRectangle.X;Y=$current.BoundingRectangle.Y;Width=$current.BoundingRectangle.Width;Height=$current.BoundingRectangle.Height}
        }
    } catch {
        $nodes += [pscustomobject]@{Index=$i;ReadError=$_.Exception.GetType().FullName}
    }
}
$rectangle = New-Object RecoveryWindowCapture+RECT
if (-not [RecoveryWindowCapture]::GetWindowRect($handle,[ref]$rectangle)) { throw 'GetWindowRect failed.' }
$width = $rectangle.Right-$rectangle.Left
$height = $rectangle.Bottom-$rectangle.Top
if ($width -le 0 -or $height -le 0) { throw 'Window rectangle has no positive area.' }
$bitmap = New-Object System.Drawing.Bitmap($width,$height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$hdc = $graphics.GetHdc()
try { $printed = [RecoveryWindowCapture]::PrintWindow($handle,$hdc,2) }
finally { $graphics.ReleaseHdc($hdc) }
if ($printed) { $bitmap.Save($OutputPrefix+'.png',[System.Drawing.Imaging.ImageFormat]::Png) }
$graphics.Dispose()
$bitmap.Dispose()
$record = [pscustomobject]@{
    CapturedAt=(Get-Date).ToString('o'); ProcessId=$TargetProcessId; Executable=$target.Path;
    WindowHandle=$handle.ToInt64(); WindowTitle=$target.MainWindowTitle;
    IsVisible=[RecoveryWindowCapture]::IsWindowVisible($handle); Width=$width;Height=$height;
    ScreenshotSucceeded=$printed; ScreenshotPath=$(if($printed){$OutputPrefix+'.png'}else{$null});
    TotalDescendants=$elements.Count; CapturedNodes=$nodes.Count;
    Privacy='Edit/password values and long identifiers are omitted from the accessibility export; screenshot remains a local selected-window capture.';
    Nodes=$nodes
}
$record | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath ($OutputPrefix+'.json') -Encoding UTF8
[pscustomobject]@{ProcessId=$TargetProcessId;WindowTitle=$target.MainWindowTitle;Width=$width;Height=$height;Nodes=$nodes.Count;ScreenshotSucceeded=$printed;OutputPrefix=$OutputPrefix} | ConvertTo-Json -Compress
