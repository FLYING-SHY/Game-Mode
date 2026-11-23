using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CoH3MemoryTool;

internal sealed partial class MaintenanceLiveContext
{
    private static void RequireEventContext(PlayerInfoReader reader, string match, long epoch)
    {
        if (!LicenseGate60.Allowed || LicenseGate60.Epoch != epoch)
            throw new InvalidOperationException("Authorization lease changed during event window; stop and invalidate this poll.");
        if (string.IsNullOrEmpty(match) || reader.MatchKey != match)
            throw new InvalidOperationException("Match changed during event window; no cross-match sample reuse.");
    }

    internal static ProjectilePlayer[] EventPlayers(IEnumerable<PlayerInfoRow> rows, bool allSides) =>
        Players(allSides ? rows : rows.Where(row => row.Relation == "敌方"));

    private RecoveryProbeTrigger SampleEvents(PlayerInfoReader reader, Exception openError, string match, long epoch)
    {
        RecoveryProbeTrigger recoveryTrigger = null;
        Probe(reader, openError, "event_window", result =>
        {
            var clock = Stopwatch.StartNew();
            var evidence = new EventEvidence();
            var polls = new List<object>();
            var airEvidence = new AirEventEvidence();
            int rounds = 0, eventSnapshots = 0, projectileQueries = 0, airQueries = 0;
            string stopReason = "not_started";
            result.Details.Add("Actual C# Snapshot and ReadProjectileImpacts on the existing authorized maintenance reader; enemy sources only. No Python predicate or synthetic decoder replay.");
            if (airEvents) result.Details.Add("--air-events: actual C# ReadAirZones on ALL current Snapshot rows, including LOCAL and allies, for controlled local US airstrike structural evidence only. This diagnostic routing is distinct from production enemy-only UI; no authorization or engine changes.");
            result.Details.Add("Only this feature may be observed; zero decoded outputs are sample_absent, not proof of no gameplay event or valid layout. No visual/whole-feature acceptance is implied.");
            try
            {
                RequireEventContext(reader, match, epoch);
                string outcome = EventPolling.Run(eventSeconds, () =>
                {
                    long started = clock.ElapsedMilliseconds;
                    string utc = DateTime.UtcNow.ToString("O");
                    string pollStatus = "incomplete";
                    int? count = null, airCount = null;
                    try
                    {
                        RequireEventContext(reader, match, epoch);
                        snapshotAttempts++; eventSnapshots++;
                        var players = reader.Snapshot();
                        RequireEventContext(reader, match, epoch);
                        if (players.Count == 0 || players.Count(row => row.Relation == "自己") != 1)
                            throw new SnapshotDependencyException("Event snapshot/local attribution unavailable; no further event polls.");
                        BindMarkerColours(players);
                        if (eventCancellation.IsCancellationRequested) { stopReason = "cancelled"; pollStatus = "cancelled_after_snapshot"; return false; }
                        if (clock.ElapsedMilliseconds >= eventSeconds * 1000L) { stopReason = "deadline_after_snapshot"; pollStatus = "deadline_after_snapshot"; return false; }
                        RequireEventContext(reader, match, epoch);
                        projectileQueries++;
                        var impacts = reader.ReadProjectileImpacts(match, EventPlayers(players, false));
                        RequireEventContext(reader, match, epoch);
                        count = impacts.Count;
                        evidence.Add(impacts, utc, started, clock.ElapsedMilliseconds, match, epoch);
                        if (airEvents)
                        {
                            if (eventCancellation.IsCancellationRequested) { stopReason = "cancelled"; pollStatus = "cancelled_after_projectiles"; return false; }
                            if (clock.ElapsedMilliseconds >= eventSeconds * 1000L) { stopReason = "deadline_after_projectiles"; pollStatus = "deadline_after_projectiles"; return false; }
                            RequireEventContext(reader, match, epoch);
                            airQueries++;
                            var zones = reader.ReadAirZones(match, EventPlayers(players, true));
                            RequireEventContext(reader, match, epoch);
                            airCount = zones.Count;
                            airEvidence.Add(zones.Select(item => (object)new
                            {
                                item.Key, item.Owner, item.Offset, item.Slot, item.Name, item.X, item.Y, item.Z, item.DX, item.DZ,
                                item.HalfWidth, item.Length, item.Radius, item.Rocket, item.Point
                            }).ToList(), utc, started, clock.ElapsedMilliseconds, match, epoch);
                        }
                        pollStatus = "completed";
                        return true;
                    }
                    finally
                    {
                        object poll = new { ordinal = eventSnapshots, utc, startedMilliseconds = started, finishedMilliseconds = clock.ElapsedMilliseconds, status = pollStatus, decodedCount = count };
                        if (airEvents) poll = new { ordinal = eventSnapshots, utc, startedMilliseconds = started, finishedMilliseconds = clock.ElapsedMilliseconds, status = pollStatus, decodedCount = count, airDecodedCount = airCount };
                        polls.Add(poll);
                    }
                }, () => clock.ElapsedMilliseconds, Thread.Sleep, () => eventCancellation.IsCancellationRequested, out rounds);
                if (outcome != "stopped") stopReason = outcome;
                result.Status = stopReason == "cancelled" ? "cancelled" : projectileQueries == 0 ? "not_run" : evidence.TotalDecoded + airEvidence.TotalDecoded > 0 ? "observed" : "sample_absent";
            }
            catch (Exception exception)
            {
                stopReason = "exception";
                if (recoveryProbe && RecoveryProbe.IsCameraFailure(exception))
                    recoveryTrigger = new RecoveryProbeTrigger
                    {
                        Reader = reader, ReaderOpenAttempts = readerOpenAttempts, Match = match, Epoch = epoch,
                        Utc = DateTime.UtcNow.ToString("O"), Error = exception.ToString()
                    };
                throw;
            }
            finally
            {
                result.SampleCount = (int)(evidence.TotalDecoded + airEvidence.TotalDecoded);
                result.Evidence = new
                {
                    requestedSeconds = eventSeconds, budgetMilliseconds = eventSeconds * 1000,
                    elapsedMilliseconds = clock.ElapsedMilliseconds, overrunMilliseconds = Math.Max(0, clock.ElapsedMilliseconds - eventSeconds * 1000L),
                    targetCadenceMilliseconds = 100, maximumRounds = eventSeconds * 10 + 1, rounds,
                    eventSnapshotAttempts = eventSnapshots, projectileQueryAttempts = projectileQueries, stopReason,
                    initialMatch = match, initialEpoch = epoch,
                    nonzeroPolls = evidence.NonzeroPolls, decodedObservationCount = evidence.TotalDecoded,
                    retainedFrames = evidence.Frames.Count, retainedDecoded = evidence.RetainedDecoded,
                    droppedFrames = evidence.DroppedFrames, droppedDecoded = evidence.DroppedDecoded,
                    maximumRetainedFrames = EventEvidence.MaximumFrames, maximumRetainedDecoded = EventEvidence.MaximumDecoded,
                    polls, frames = evidence.Frames
                };
                if (airEvents)
                {
                    result.Evidence = new
                    {
                        projectile = result.Evidence,
                        air = new
                        {
                            scope = "all_current_snapshot_rows_diagnostic_not_production_enemy_UI",
                            status = stopReason == "exception" ? "invalidated" : stopReason == "cancelled" ? "cancelled" : airQueries == 0 ? "not_run" : airEvidence.TotalDecoded > 0 ? "observed" : "sample_absent",
                            queryAttempts = airQueries, nonzeroPolls = airEvidence.NonzeroPolls,
                            decodedObservationCount = airEvidence.TotalDecoded,
                            retainedFrames = airEvidence.Frames.Count, retainedDecoded = airEvidence.RetainedDecoded,
                            droppedFrames = airEvidence.DroppedFrames, droppedDecoded = airEvidence.DroppedDecoded,
                            maximumRetainedFrames = AirEventEvidence.MaximumFrames, maximumRetainedDecoded = AirEventEvidence.MaximumDecoded,
                            frames = airEvidence.Frames
                        }
                    };
                    result.Details.Add("Air summary/frames are separate from projectile counters; retain first/latest successful empty reads and up to126 positive frames in time order, within128 frames/256 decoded total and16 decoded per frame. Replaced empty frames and omitted positive frames count as droppedFrames; all decoded truncation/drops are exact. Zero output is sample_absent, never PASS. Aggregate observed can come from either stream; consult each stream separately.");
                    result.Details.Add("Air uses the same initial match/epoch checks before and after ReadAirZones; cancellation/deadline checked again between projectile and air reads. Earlier frames remain historical if a later read fails.");
                    Console.WriteLine("event_window airScope=all_sides_diagnostic airQueries=" + airQueries + " airDecodedObservations=" + airEvidence.TotalDecoded);
                }
                result.Details.Add("Budget and cancellation are cooperative between synchronous reads; a single OS read/Snapshot may overrun. Target start-to-start cadence is 100ms, not guaranteed under read latency; max rounds also bounds a frozen clock.");
                result.Details.Add("Decoded observations may repeat the same entity across polls and are not a unique-projectile count. Retention: at most16 decoded per nonzero poll,128 frames and256 decoded total; exact dropped counters retained. Payloads are public method outputs, not raw memory bytes.");
                result.Details.Add("Initial match/epoch stay fixed for the entire window; checked before/after Snapshot and projectile query. Lease/match errors fail the feature; earlier retained frames are historical, not proof the failing poll was valid.");
                Console.WriteLine("event_window stop=" + stopReason + " polls=" + rounds + " decodedObservations=" + evidence.TotalDecoded);
            }
        });
        return recoveryTrigger;
    }
}
