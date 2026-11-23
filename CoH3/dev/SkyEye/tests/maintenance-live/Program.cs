using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class MaintenanceLiveProgram
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "--self-test") return RunnerSelfTests.Run();
        if (args.Length == 1 && args[0] == "--event-self-test") return EventPollingSelfTests.Run();
        if (args.Length == 1 && args[0] == "--air-event-self-test") return AirEventSelfTests.Run();
        if (args.Length == 1 && args[0] == "--recovery-self-test") return RecoveryProbeSelfTests.Run();
        string output = null;
        int waitSeconds = 120;
        int eventSeconds = 0;
        int eventPrewarmDelayMilliseconds = 0;
        bool airEvents = false;
        bool recoveryProbe = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--out" && i + 1 < args.Length) output = args[++i];
            else if (args[i] == "--wait-seconds" && i + 1 < args.Length && int.TryParse(args[++i], out int seconds) && seconds >= 1 && seconds <= 600) waitSeconds = seconds;
            else if (args[i] == "--event-seconds" && i + 1 < args.Length && EventPolling.ParseSeconds(args[++i], out int duration)) eventSeconds = duration;
            else if (args[i] == "--air-events") airEvents = true;
            else if (args[i] == "--recovery-probe") recoveryProbe = true;
            else if (args[i] == "--event-prewarm-delay-ms" && i + 1 < args.Length && EventPrewarm.ParseMilliseconds(args[++i], out int delay)) eventPrewarmDelayMilliseconds = delay;
            else { Console.Error.WriteLine("Usage: SkyEye.Maintenance.Live.exe --out REPORT_DIRECTORY [--wait-seconds 120] [--event-seconds 1..60] [--air-events] [--recovery-probe] [--event-prewarm-delay-ms 0..5000]"); return 2; }
        }
        if (airEvents && eventSeconds == 0) { Console.Error.WriteLine("--air-events requires --event-seconds 1..60."); return 2; }
        if (recoveryProbe && eventSeconds == 0) { Console.Error.WriteLine("--recovery-probe requires --event-seconds 1..60."); return 2; }
        if (eventPrewarmDelayMilliseconds > 0 && eventSeconds == 0) { Console.Error.WriteLine("--event-prewarm-delay-ms requires --event-seconds 1..60."); return 2; }
        if (string.IsNullOrWhiteSpace(output)) { Console.Error.WriteLine("--out REPORT_DIRECTORY is required. Maintenance-only executable; not the production client."); return 2; }
        try
        {
            Directory.CreateDirectory(Path.GetFullPath(output));
            AudioDependency.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var context = new MaintenanceLiveContext(Path.GetFullPath(output), waitSeconds, eventSeconds, airEvents, recoveryProbe, eventPrewarmDelayMilliseconds))
            {
                ConsoleCancelEventHandler cancel = (sender, eventArgs) => { eventArgs.Cancel = true; context.CancelEventSampling(); };
                if (eventSeconds > 0) Console.CancelKeyPress += cancel;
                try { Application.Run(context); return context.ExitCode; }
                finally { if (eventSeconds > 0) Console.CancelKeyPress -= cancel; }
            }
        }
        catch (Exception exception) { Console.Error.WriteLine(exception); return 4; }
    }
}

internal sealed partial class MaintenanceLiveContext : ApplicationContext
{
    private readonly LicenseCoordinator60 coordinator = new LicenseCoordinator60();
    private readonly ActivationForm60 login;
    private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 200 };
    private readonly Stopwatch waiting = Stopwatch.StartNew();
    private readonly List<LiveProbeResult> results = new List<LiveProbeResult>();
    private readonly string output;
    private readonly int waitSeconds;
    private readonly int eventSeconds;
    private readonly bool airEvents;
    private readonly bool recoveryProbe;
    private readonly int eventPrewarmDelayMilliseconds;
    private readonly CancellationTokenSource eventCancellation = new CancellationTokenSource();
    internal void CancelEventSampling() => eventCancellation.Cancel();
    private string licenseState = "starting";
    private bool sampling;
    private bool closing;
    public int ExitCode { get; private set; } = 3;

    internal MaintenanceLiveContext(string output, int waitSeconds, int eventSeconds = 0, bool airEvents = false, bool recoveryProbe = false, int eventPrewarmDelayMilliseconds = 0)
    {
        this.output = output;
        this.waitSeconds = waitSeconds;
        this.eventSeconds = eventSeconds;
        this.airEvents = airEvents;
        this.recoveryProbe = recoveryProbe;
        this.eventPrewarmDelayMilliseconds = eventPrewarmDelayMilliseconds;
        login = new ActivationForm60(coordinator.Credentials);
        login.Text = "MAINTENANCE LIVE - normal license login";
        _ = login.Handle;
        login.ActivateRequested += (code, remember) => coordinator.Submit(code, remember);
        login.FormClosing += async (sender, args) =>
        {
            if (closing) return;
            args.Cancel = true;
            if (sampling) { if (eventSeconds > 0) CancelEventSampling(); return; }
            results.Add(new LiveProbeResult { Feature = "authorization", Status = "fail", Error = "User closed login before sampling." });
            await Finish();
        };
        coordinator.Changed += view =>
        {
            if (closing || login.IsDisposed) return;
            try
            {
                login.BeginInvoke((Action)(() =>
                {
                    if (closing || !coordinator.IsCurrent(view)) return;
                    licenseState = view.State;
                    login.UpdateState(view);
                    Console.WriteLine("license_state=" + licenseState);
                }));
            }
            catch (InvalidOperationException) { }
        };
        timer.Tick += async (sender, args) =>
        {
            if (sampling || closing) return;
            if (eventSeconds > 0 && eventCancellation.IsCancellationRequested)
            {
                results.Add(new LiveProbeResult { Feature = "event_window", Status = "cancelled", Details = new List<string> { "Console cancellation before sampling; no event read attempted." } });
                await Finish();
                return;
            }
            if (LicenseGate60.Allowed)
            {
                sampling = true;
                timer.Stop();
                login.Hide();
                try { await Task.Run(Sample); }
                catch (Exception exception) { results.Add(new LiveProbeResult { Feature = "sampling", Status = "fail", Error = exception.ToString() }); }
                await Finish();
            }
            else if (waiting.Elapsed.TotalSeconds >= waitSeconds)
            {
                results.Add(new LiveProbeResult { Feature = "authorization", Status = "fail", Error = "AUTHORIZATION_TIMEOUT; state=" + licenseState });
                await Finish();
            }
        };
        login.Show();
        timer.Start();
        coordinator.Start();
    }

    private async Task Finish()
    {
        if (closing) return;
        closing = true;
        timer.Stop();
        ExitCode = BoundedPoll.ExitCodeFor(results);
        try { await coordinator.CloseAsync(); }
        catch (Exception exception) { ExitCode = 4; results.Add(new LiveProbeResult { Feature = "shutdown", Status = "fail", Error = exception.ToString() }); }
        try
        {
            var report = new { maintenanceOnly = true, buildVerifiedByThisRun = false, visualVerification = "not_run", utc = DateTime.UtcNow.ToString("O"), licenseState, readerOpenAttempts, snapshotAttempts, snapshotCapturedUtc, eventWindowSeconds = eventSeconds, recoveryProbeRequested = recoveryProbe, eventPrewarmDelayMilliseconds, mapDiscoveryBudgetMilliseconds = 5000, exitCode = ExitCode, results };
            var serializer = new JavaScriptSerializer { MaxJsonLength = (eventSeconds > 0 ? 4 : 1) * 1024 * 1024 };
            File.WriteAllText(Path.Combine(output, "maintenance-live.txt"), "MAINTENANCE ONLY; read success is not build/visual verification; exitCode=" + ExitCode + "\r\n" + string.Join("\r\n\r\n", results.Select(result => result.Feature + ": " + result.Status + " build=" + result.BuildStatus + " ms=" + result.ElapsedMilliseconds + " samples=" + result.SampleCount + " utc=" + result.StartedUtc + "\r\n" + string.Join("\r\n", result.Details) + "\r\n" + serializer.Serialize(result.Evidence) + "\r\n" + result.Error)), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(output, "maintenance-live.json"), serializer.Serialize(report), new UTF8Encoding(false));
        }
        catch (Exception exception) { ExitCode = 4; Console.Error.WriteLine(exception); }
        finally { coordinator.Dispose(); timer.Dispose(); login.Close(); ExitThread(); }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) eventCancellation.Dispose();
        base.Dispose(disposing);
    }
}
