using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CoH3MemoryTool;

internal sealed partial class MaintenanceLiveContext
{
    private bool SampleEventPrewarm(PlayerInfoReader reader, Exception openError, string match, long epoch)
    {
        bool completed = false;
        LiveProbeResult recorded = null;
        Probe(reader, openError, "event_prewarm_delay", result =>
        {
            recorded = result;
            var clock = Stopwatch.StartNew();
            string startedUtc = DateTime.UtcNow.ToString("O");
            string outcome = "exception";
            int liveMatchChecks = 0;
            Action requireLiveMatch = () =>
            {
                RequireEventContext(reader, match, epoch);
                liveMatchChecks++;
                if (!reader.IsCurrentMatch55()) throw new InvalidOperationException("Live match check failed around event prewarm; no event query may start.");
                RequireEventContext(reader, match, epoch);
            };
            try
            {
                requireLiveMatch();
                clock.Restart();
                startedUtc = DateTime.UtcNow.ToString("O");
                Console.WriteLine("event_prewarm_delay started milliseconds=" + eventPrewarmDelayMilliseconds + " utc=" + startedUtc);
                Console.Out.Flush();
                completed = EventPrewarm.Wait(eventPrewarmDelayMilliseconds, () => RequireEventContext(reader, match, epoch),
                    () => clock.ElapsedMilliseconds, Thread.Sleep, () => eventCancellation.IsCancellationRequested);
                if (eventCancellation.IsCancellationRequested) completed = false;
                if (completed) requireLiveMatch(); else RequireEventContext(reader, match, epoch);
                outcome = completed ? "elapsed" : "cancelled";
                result.Status = completed ? "observed" : "cancelled";
            }
            finally
            {
                string finishedUtc = DateTime.UtcNow.ToString("O");
                result.Evidence = new
                {
                    requestedMilliseconds = eventPrewarmDelayMilliseconds, startedUtc, finishedUtc,
                    elapsedMilliseconds = clock.ElapsedMilliseconds, initialMatch = match, initialEpoch = epoch, liveMatchChecks, outcome
                };
                result.Details.Add("Optional cooperative delay after the initial air query; no reader reopen or event query during the delay. The same pre-air match/epoch is required by the following event window. Start/end each call IsCurrentMatch55 for live world/local-identity validation; these guard camera reads are not recovery evidence. Between50ms wait slices only cached MatchKey and lease are checked.");
                Console.WriteLine("event_prewarm_delay finished outcome=" + outcome + " elapsedMilliseconds=" + clock.ElapsedMilliseconds + " utc=" + finishedUtc);
                Console.Out.Flush();
            }
        });
        return completed && recorded?.Status == "observed";
    }

    private void SampleRecovery(PlayerInfoReader reader, RecoveryProbeTrigger trigger)
    {
        var result = NewResult(reader, "same_reader_recovery");
        var clock = Stopwatch.StartNew();
        var evidence = RecoveryProbe.Run(trigger,
            () => new RecoveryProbeContext
            {
                Reader = reader, ReaderOpenAttempts = readerOpenAttempts, Match = reader?.MatchKey,
                Epoch = LicenseGate60.Epoch, Allowed = LicenseGate60.Allowed
            },
            () =>
            {
                var camera = reader.ReadMineCamera();
                return new { camera.View, camera.Projection, convention = "16 floats per matrix, raw reader ordering" };
            },
            () =>
            {
                snapshotAttempts++;
                var rows = reader.Snapshot();
                if (rows.Count == 0 || rows.Count(row => row.Relation == "自己") != 1)
                    throw new SnapshotDependencyException("Recovery Snapshot local attribution unavailable.");
                var local = rows.Single(row => row.Relation == "自己");
                return new
                {
                    rowCount = rows.Count,
                    local = new { local.Slot, local.Relation, local.IsLocalPlayer, playerAddress = local.PlayerAddress.ToString("x"), identity = local.PlayerIdentity.ToString("x") }
                };
            }, () => clock.ElapsedMilliseconds, () => DateTime.UtcNow.ToString("O"), Thread.Sleep,
            () => eventCancellation.IsCancellationRequested);
        result.Status = evidence.Status;
        result.Evidence = evidence;
        result.SampleCount = evidence.CameraAndSnapshotRecovered ? 1 : 0;
        result.ElapsedMilliseconds = clock.ElapsedMilliseconds;
        result.Details.Add("Explicit --recovery-probe only; triggered by a natural event-window ReadMineCamera diagnostic message, never an injected exception. No recognized camera error means not_triggered with zero recovery reads.");
        result.Details.Add("At most3 sequential attempts, each direct ReadMineCamera followed by Snapshot only if the camera call succeeded; stop after the first successful pair. Cooperative2000ms budget and100ms retry gap; a synchronous read may overrun. Snapshot itself performs internal camera reads not counted as directCameraAttempts.");
        result.Details.Add("ReferenceEquals verifies the original reader object; identity hashes are labels only. Reader open count must remain1. Original event match/lease epoch and Allowed are checked before/after each call, including failed calls; MatchKey is a cached guard, while successful Snapshot performs the engine's live world/local-identity checks.");
        result.Details.Add("Independent diagnostic evidence only: original event_window fail, air invalidated and exit3 remain unchanged. Map evidence is separate and does not count as camera or Snapshot recovery; production GUI retry behavior is not exercised.");
        results.Add(result);
        Console.WriteLine(result.Feature + ": " + result.Status + " cameraAttempts=" + evidence.DirectCameraAttempts + " snapshotAttempts=" + evidence.SnapshotAttempts + " stop=" + evidence.StopReason);
    }
}
