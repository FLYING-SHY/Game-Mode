param(
 [Parameter(Mandatory=$true)][int]$TargetProcessId,
 [Parameter(Mandatory=$true)][string]$ExpectedStartUtc,
 [Parameter(Mandatory=$true)][string]$ExpectedTitle,
 [Parameter(Mandatory=$true)][string[]]$ControlNames,
 [Parameter(Mandatory=$true)][string]$Output
)
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $Output){throw 'Action evidence already exists'}
$client=Get-Process -Id $TargetProcessId
try {
 if($client.StartTime.ToUniversalTime().ToString('o') -ne $ExpectedStartUtc){throw 'Maintenance process identity mismatch'}
 Add-Type -AssemblyName UIAutomationClient,UIAutomationTypes
 Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class ScopedMaintenanceInput {
 private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr parameter);
 [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowProc callback, IntPtr parameter);
 [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
 [DllImport("user32.dll", CharSet=CharSet.Unicode)] private static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder title, int capacity);
 public static IntPtr FindUniqueTitledWindow(uint pid, string expectedTitle) {
  var found = new System.Collections.Generic.List<IntPtr>();
  EnumWindows(delegate(IntPtr hwnd, IntPtr parameter) {
   uint owner; GetWindowThreadProcessId(hwnd, out owner);
   if(owner == pid && IsWindowVisible(hwnd)) {
    var title = new System.Text.StringBuilder(512); GetWindowText(hwnd, title, title.Capacity);
    if(title.ToString() == expectedTitle) found.Add(hwnd);
   }
   return true;
  }, IntPtr.Zero);
  if(found.Count != 1) throw new InvalidOperationException("Expected one visible titled maintenance panel");
  return found[0];
 }
 [StructLayout(LayoutKind.Sequential)] public struct POINT { public int X; public int Y; }
 [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint owner);
 [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a,uint b,bool attach);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern IntPtr WindowFromPoint(POINT point);
 [DllImport("user32.dll")] public static extern bool IsChild(IntPtr parent,IntPtr child);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint dx,uint dy,uint data,UIntPtr extra);
}
'@
 $main=[ScopedMaintenanceInput]::FindUniqueTitledWindow([uint32]$TargetProcessId,$ExpectedTitle)
 $owner=[uint32]0
 $thread=[ScopedMaintenanceInput]::GetWindowThreadProcessId([ScopedMaintenanceInput]::GetForegroundWindow(),[ref]$owner)
 $current=[ScopedMaintenanceInput]::GetCurrentThreadId()
 $attached=$thread -ne $current -and [ScopedMaintenanceInput]::AttachThreadInput($current,$thread,$true)
 try{[void][ScopedMaintenanceInput]::BringWindowToTop($main);[void][ScopedMaintenanceInput]::SetForegroundWindow($main)}finally{if($attached){[void][ScopedMaintenanceInput]::AttachThreadInput($current,$thread,$false)}}
 Start-Sleep -Milliseconds 120
 if([ScopedMaintenanceInput]::GetForegroundWindow() -ne $main){throw 'Panel did not acquire foreground; no clicks sent'}
 $actions=@()
 foreach($name in $ControlNames){
  $window=[System.Windows.Automation.AutomationElement]::FromHandle($main)
  $condition=New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty,$name)
  $elements=$window.FindAll([System.Windows.Automation.TreeScope]::Descendants,$condition)
  if($elements.Count -ne 1){throw ('Expected one named control: '+$name)}
  $target=$elements.Item(0).Current
  if(-not $target.IsEnabled -or $target.IsOffscreen){throw 'Control is not visible and enabled'}
  $point=New-Object ScopedMaintenanceInput+POINT
  $point.X=[int]($target.BoundingRectangle.X+22);$point.Y=[int]($target.BoundingRectangle.Y+$target.BoundingRectangle.Height/2)
  [void][ScopedMaintenanceInput]::SetCursorPos($point.X,$point.Y)
  $hit=[ScopedMaintenanceInput]::WindowFromPoint($point);$hitOwner=[uint32]0
  [void][ScopedMaintenanceInput]::GetWindowThreadProcessId($hit,[ref]$hitOwner)
  if($hitOwner -ne $TargetProcessId -or -not [ScopedMaintenanceInput]::IsChild($main,$hit) -or [ScopedMaintenanceInput]::GetForegroundWindow() -ne $main){throw 'Click target guard failed'}
  [ScopedMaintenanceInput]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
  try{Start-Sleep -Milliseconds 40}finally{[ScopedMaintenanceInput]::mouse_event(4,0,0,0,[UIntPtr]::Zero)}
  $actions += [pscustomobject]@{utc=[DateTime]::UtcNow.ToString('o');name=$name;childHwnd=$hit.ToInt64();x=$point.X;y=$point.Y;mechanism='foreground and owning-PID checked native mouse'}
  Start-Sleep -Milliseconds 160
 }
 [pscustomobject]@{uiPid=$TargetProcessId;uiStartUtc=$ExpectedStartUtc;mainHwnd=$main.ToInt64();actions=$actions;scope='native control actions only; settings and rendering require independent observations'}|ConvertTo-Json -Depth 5|Set-Content -LiteralPath $Output -Encoding utf8
 [pscustomobject]@{actions=$actions.Count;output=$Output}|ConvertTo-Json -Compress
} finally {$client.Dispose()}
