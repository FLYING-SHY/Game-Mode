$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$source=Join-Path $root 'verification\bin\Release\net48'
$target=Join-Path $root 'verification\bin\ReactorTrial\net48'
$ev=Join-Path $root 'evidence\reactor-client-trial'
New-Item -ItemType Directory -Force -Path $target,$ev | Out-Null
Copy-Item -LiteralPath $source -Destination (Split-Path -Parent $target) -Recurse -Force
$client='天穹之眼-自有授权版-v0.60.8.exe'
Copy-Item -LiteralPath (Join-Path $root ('tools\reactor\trial\20261113-144033-423-c4bc273d\'+$client)) -Destination (Join-Path $target $client) -Force
Add-Type @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
public static class ReactorWindowProbe {
 public delegate bool Callback(IntPtr h, IntPtr p);
 [DllImport("user32.dll")] static extern bool EnumWindows(Callback cb,IntPtr p);
 [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr h,Callback cb,IntPtr p);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr h,StringBuilder s,int n);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr h,StringBuilder s,int n);
 public static string[] Read(uint wanted) {
  var rows=new List<string>();
  Callback child=(h,p)=>{var s=new StringBuilder(4096);GetWindowText(h,s,s.Capacity);if(s.Length>0)rows.Add("child: "+s);return true;};
  EnumWindows((h,p)=>{uint pid;GetWindowThreadProcessId(h,out pid);if(pid==wanted){var s=new StringBuilder(4096);var c=new StringBuilder(512);GetWindowText(h,s,s.Capacity);GetClassName(h,c,c.Capacity);rows.Add("window: "+h+" class="+c+" text="+s);EnumChildWindows(h,child,IntPtr.Zero);}return true;},IntPtr.Zero);
  return rows.ToArray();
 }
}
'@
$stamp=Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$jobs=@()
foreach($mode in @('selfcheck','ui','audio')) {
 $sandbox=Join-Path $root ('verification\sandbox\reactor-'+$stamp+'-'+$mode)
 $stdout=Join-Path $ev ($mode+'-stdout.log')
 $stderr=Join-Path $ev ($mode+'-stderr.log')
 $info=New-Object Diagnostics.ProcessStartInfo
 $info.FileName=Join-Path $target 'SkyEye.Integration.Verification.exe'
 $info.Arguments='--mode '+$mode+' --sandbox "'+$sandbox+'"'
 $info.WorkingDirectory=$target
 $info.UseShellExecute=$false
 $info.RedirectStandardOutput=$true
 $info.RedirectStandardError=$true
 $proc=New-Object Diagnostics.Process
 $proc.StartInfo=$info
 $null=$proc.Start()
 $outTask=$proc.StandardOutput.ReadToEndAsync()
 $errTask=$proc.StandardError.ReadToEndAsync()
 $jobs+=@{Mode=$mode;Process=$proc;Start=Get-Date;Sandbox=$sandbox;Stdout=$stdout;Stderr=$stderr;OutTask=$outTask;ErrTask=$errTask}
}
$rows=@()
foreach($item in $jobs) {
 $proc=$item.Process
 $remaining=[Math]::Max(0,30000-((Get-Date)-$item.Start).TotalMilliseconds)
 $exited=$proc.WaitForExit([int]$remaining)
 $windows=@()
 if(-not $exited) { $windows=@([ReactorWindowProbe]::Read([uint32]$proc.Id)); Stop-Process -Id $proc.Id -Force; $proc.WaitForExit(5000) | Out-Null }
 [IO.File]::WriteAllText($item.Stdout,$item.OutTask.GetAwaiter().GetResult(),[Text.UTF8Encoding]::new($false))
 [IO.File]::WriteAllText($item.Stderr,$item.ErrTask.GetAwaiter().GetResult(),[Text.UTF8Encoding]::new($false))
 $record=[ordered]@{Mode=$item.Mode;PID=$proc.Id;TimeoutMs=30000;TimedOut=(-not $exited);ExitCode=$proc.ExitCode;Windows=$windows;Sandbox=$item.Sandbox;Stdout=$item.Stdout;Stderr=$item.Stderr;Passed=($exited -and $proc.ExitCode -eq 0)}
 $rows+=$record
 foreach($report in @('result.json','ui-result.json','audio-result.json')) { $p=Join-Path $item.Sandbox $report; if(Test-Path -LiteralPath $p){Copy-Item -LiteralPath $p -Destination (Join-Path $ev ($item.Mode+'-'+$report)) -Force} }
}
$rows | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $ev 'runtime-results.json') -Encoding UTF8
$rows | ConvertTo-Json -Depth 5
