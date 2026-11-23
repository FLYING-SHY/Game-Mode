param([Parameter(Mandatory=$true)][int]$TargetPid,[string]$Name,[switch]$Invoke,[switch]$Click)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
if($Click){
 Add-Type @'
using System; using System.Runtime.InteropServices;
public static class UiMouse {
 [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint flags,uint x,uint y,uint data,UIntPtr extra);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr hwnd);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
 [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint from,uint to,bool attach);
 [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
}
'@
 [UiMouse]::SetProcessDPIAware() | Out-Null
}
$root=[System.Windows.Automation.AutomationElement]::RootElement
$cond=New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ProcessIdProperty,$TargetPid)
$windows=$root.FindAll([System.Windows.Automation.TreeScope]::Children,$cond)
$records=@()
foreach($window in $windows){
 $all=$window.FindAll([System.Windows.Automation.TreeScope]::Descendants,[System.Windows.Automation.Condition]::TrueCondition)
 $elements=@($window); for($i=0;$i -lt [Math]::Min($all.Count,400);$i++){ $elements+=$all.Item($i) }
 foreach($el in $elements){
  $cur=$el.Current
  # Exclude text entry controls: this probe never reads typed credentials or values.
  if($cur.ControlType -eq [System.Windows.Automation.ControlType]::Edit){continue}
  $record=[ordered]@{name=$cur.Name;type=$cur.ControlType.ProgrammaticName;enabled=$cur.IsEnabled;offscreen=$cur.IsOffscreen;rect=$cur.BoundingRectangle.ToString()}
  $records+=$record
  if($Click -and $Name -and $cur.Name -ceq $Name){
   if(-not $cur.IsEnabled -or $cur.IsOffscreen){throw 'Selected control is not available; no mouse input sent'}
    $targetHandle=[IntPtr]$window.Current.NativeWindowHandle
    [uint32]$ownerPid=0
    [UiMouse]::GetWindowThreadProcessId($targetHandle,[ref]$ownerPid) | Out-Null
    if($ownerPid -ne $TargetPid){throw 'Selected window owner changed; no mouse input sent'}
    [UiMouse]::SetForegroundWindow($targetHandle) | Out-Null
    if([UiMouse]::GetForegroundWindow() -ne $targetHandle){
     [uint32]$foregroundPid=0
     $foregroundThread=[UiMouse]::GetWindowThreadProcessId([UiMouse]::GetForegroundWindow(),[ref]$foregroundPid)
     $currentThread=[UiMouse]::GetCurrentThreadId()
     $attached=$foregroundThread -ne 0 -and $foregroundThread -ne $currentThread -and [UiMouse]::AttachThreadInput($currentThread,$foregroundThread,$true)
     try { [UiMouse]::BringWindowToTop($targetHandle) | Out-Null; [UiMouse]::SetForegroundWindow($targetHandle) | Out-Null }
     finally { if($attached){[UiMouse]::AttachThreadInput($currentThread,$foregroundThread,$false) | Out-Null} }
    }
    Start-Sleep -Milliseconds 150
    if([UiMouse]::GetForegroundWindow() -ne $targetHandle){throw 'Selected control panel did not acquire foreground; no mouse input sent'}
    $r=$cur.BoundingRectangle
   [UiMouse]::SetCursorPos([int]($r.Left+$r.Width/2),[int]($r.Top+$r.Height/2)) | Out-Null
   Start-Sleep -Milliseconds 120
    if([UiMouse]::GetForegroundWindow() -ne $targetHandle){throw 'Selected control panel lost foreground; no mouse input sent'}
    [UiMouse]::mouse_event(2,0,0,0,[UIntPtr]::Zero)
    try { Start-Sleep -Milliseconds 40 } finally { [UiMouse]::mouse_event(4,0,0,0,[UIntPtr]::Zero) }
   $record.action='foreground_checked_click'; $record.utc=[DateTime]::UtcNow.ToString('o'); $record.windowHandle=$targetHandle.ToInt64(); $Click=$false
  }
  if($Invoke -and $Name -and $cur.Name -ceq $Name){
   $pat=$null
   if($el.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern,[ref]$pat)){$pat.Invoke(); $record.action='invoked'}
   elseif($el.TryGetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern,[ref]$pat)){$record.before=$pat.Current.ToggleState.ToString();$pat.Toggle();$record.action='toggled'}
   else{throw "Control has no invoke/toggle pattern: $Name"}
   $Invoke=$false
  }
 }
}
if($Name){ $records | Where-Object { $_.name -ceq $Name } | ConvertTo-Json -Depth 5 } else { $records | ConvertTo-Json -Depth 5 }
