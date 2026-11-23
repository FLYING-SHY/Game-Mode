using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CoH3MemoryTool;

internal sealed partial class MaintenanceLiveContext
{
    private int readerOpenAttempts;
    private int snapshotAttempts;
    private string snapshotCapturedUtc;

    internal const bool SampleInfantryMovement = true;

    internal static void BindMarkerColours(IEnumerable<PlayerInfoRow> players)
    {
        foreach (var row in players)
            foreach (var source in row.MarkerSquads)
            {
                source.ColourKnown = row.GameColourKnown;
                source.Colour = row.GameColour;
            }
    }

    private void Sample()
    {
        results.Add(new LiveProbeResult { Feature = "authorization", Status = "pass", Details = new List<string> { "Real coordinator lease allowed; no synthetic grant." } });
        PlayerInfoReader reader = null;
        Exception openError = null;
        readerOpenAttempts++;
        try { reader = PlayerInfoReader.OpenForMaintenanceDiagnostics(); }
        catch (Exception exception) { openError = exception; }
        try
        {
            List<PlayerInfoRow> rows = null;
            Probe(reader, openError, "snapshot/resources", result =>
            {
                snapshotAttempts++;
                rows = reader.Snapshot();
                snapshotCapturedUtc = DateTime.UtcNow.ToString("O");
                result.Evidence = rows.Select(row => new { row.Slot, row.Relation, row.IsLocalPlayer, identity = row.PlayerIdentity.ToString("x"), row.Manpower, row.Munition, row.Fuel, row.CP, row.Population, row.Race }).ToArray();
                if (rows.Count == 0 || rows.Count(row => row.Relation == "自己") != 1) throw new InvalidOperationException("No valid live snapshot/local player; enter an active match.");
                result.SampleCount = rows.Count;
                result.Details.Add("Resources are raw game values. Snapshot captured once; downstream items share this timestamp, not atomic cross-feature state.");
            });
            var capturedRows = rows;
            Func<List<PlayerInfoRow>> requireRows = () =>
            {
                if (capturedRows == null || capturedRows.Count == 0 || capturedRows.Count(row => row.Relation == "自己") != 1) throw new SnapshotDependencyException("Single snapshot unavailable or local attribution invalid; no retry per feature.");
                return capturedRows;
            };
            Probe(reader, openError, "units", result =>
            {
                var players = requireRows();
                result.Evidence = players.Select(row => new { row.Slot, row.UnitsKnown, units = row.Units.ToArray() }).ToArray();
                if (players.Any(row => !row.UnitsKnown)) throw new InvalidOperationException("Unit data unavailable.");
                Count(result, players.Sum(row => row.Units.Values.Sum()));
            });
            Probe(reader, openError, "buildings", result =>
            {
                var players = requireRows();
                result.Evidence = players.Select(row => new { row.Slot, row.BuildingsKnown, items = row.Buildings.ToArray() }).ToArray();
                if (players.Any(row => !row.BuildingsKnown)) throw new InvalidOperationException("Building data unavailable.");
                Count(result, players.Sum(row => row.Buildings.Count));
            });
            Probe(reader, openError, "production", result =>
            {
                var players = requireRows();
                result.Evidence = players.Select(row => new { row.Slot, row.ProductionKnown, row.Kwk38Known, row.GrenadeRemainingTicks, tickRatePerSecond = 8, grenadeRemainingSeconds = row.GrenadeRemainingTicks.HasValue ? (int?)GrenadeUpgrade.SecondsRemaining(row.GrenadeRemainingTicks.Value) : null, jobs = row.Producing.Concat(row.Kwk38Upgrading).Select(job => new { job.Path, job.Key, job.RemainingTicks, job.TotalTicks, job.RemainingSeconds }).ToArray() }).ToArray();
                if (players.Any(row => !row.ProductionKnown || !row.Kwk38Known)) throw new InvalidOperationException("Production data unavailable.");
                Count(result, players.Sum(row => row.Producing.Count + row.Kwk38Upgrading.Count + (row.GrenadeRemainingTicks.HasValue ? 1 : 0)));
                result.Details.Add("Tick-to-second conversion inherited from GrenadeUpgrade: ceil(ticks/8); visual timer agreement not established.");
            });
            Probe(reader, openError, "upgrades", result =>
            {
                var players = requireRows(); result.Evidence = players.Select(row => new { row.Slot, items = row.Upgrades.ToArray() }).ToArray(); Count(result, players.Sum(row => row.Upgrades.Count));
                result.Details.Add("No independent UpgradesKnown flag exists; empty lists mean sample_absent, not absence proven.");
            });
            Probe(reader, openError, "colours", result =>
            {
                var players = requireRows(); result.Evidence = players.Select(row => new { row.Slot, row.Relation, row.GameColourKnown, argb = row.GameColour.ToArgb().ToString("x8") }).ToArray();
                if (players.Any(row => !row.GameColourKnown)) throw new InvalidOperationException("Colour data unavailable: " + reader.ColourError);
                Count(result, players.Count);
            });
            Probe(reader, openError, "camera", result =>
            {
                var camera = reader.ReadMineCamera(); result.Evidence = new { camera.View, camera.Projection, convention = "16 floats per matrix, raw reader ordering" }; Count(result, 1);
            });
            Probe(reader, openError, "squad_markers", result =>
            {
                var players = requireRows();
                if (players.Any(row => !row.MarkerDataKnown)) throw new InvalidOperationException("Marker source data unavailable.");
                // Snapshot sources deliberately omit render colours; production hosts bind them before reading.
                BindMarkerColours(players);
                bool previousMovement = reader.FlankReadMovement47;
                List<SquadMarker> items;
                reader.FlankReadMovement47 = SampleInfantryMovement;
                try { items = reader.ReadSquadMarkers(reader.MatchKey, players.SelectMany(row => row.MarkerSquads)); }
                finally { reader.FlankReadMovement47 = previousMovement; }
                result.Details.Add("Bound source colours from the shared snapshot, matching SquadMarkerHost; sourceCount=" + players.Sum(row => row.MarkerSquads.Count));
                result.Details.Add("Sampling flags: FlankReadMovement47=" + SampleInfantryMovement + "; previous=" + previousMovement + "; restored in finally. This opts infantry into the same movement path used by UpdateFlank47.");
                result.Details.Add("Direction/Movement null means no optional result from this read, not event absence or verified layout. Regular marker hosts bind colour and capture Context/membership; UpdateFlank47 is the only production assignment of a reader sampling flag.");
                result.Evidence = items.Select(item => new { item.Slot, item.Name, item.Path, item.X, item.Y, item.Z, item.Direction, item.Movement }).ToArray(); Count(result, items.Count);
            });
            Probe(reader, openError, "support", result =>
            {
                var items = reader.ReadSupport49(requireRows()); result.Evidence = items.Select(item => new { item.Slot, item.Name, item.Path, item.X, item.Y, item.Z, expiresUtc = item.Expires.ToUniversalTime().ToString("O"), item.Flare }).ToArray(); Count(result, items.Count);
            });
            Probe(reader, openError, "projectiles", result =>
            {
                var items = reader.ReadProjectileImpacts(reader.MatchKey, Players(requireRows())); result.Evidence = items.Select(item => new { item.Slot, item.Id, item.Index, item.Name, item.Weapon, item.Label, item.X, item.Y, item.Z, item.Radius }).ToArray(); Count(result, items.Count);
            });
            // Fix the event context before the initial air scan, including any optional prewarm delay.
            string eventMatch = reader?.MatchKey;
            long eventEpoch = LicenseGate60.Epoch;
            Probe(reader, openError, "air", result =>
            {
                var items = reader.ReadAirZones(reader.MatchKey, Players(requireRows())); result.Evidence = items.Select(item => new { item.Slot, item.Name, item.X, item.Y, item.Z, item.DX, item.DZ, item.HalfWidth, item.Length, item.Radius, item.Rocket, item.Point }).ToArray(); Count(result, items.Count);
            });
            RecoveryProbeTrigger recoveryTrigger = null;
            if (eventSeconds > 0)
            {
                if (eventPrewarmDelayMilliseconds == 0 || SampleEventPrewarm(reader, openError, eventMatch, eventEpoch))
                    recoveryTrigger = SampleEvents(reader, openError, eventMatch, eventEpoch);
                else
                    results.Add(new LiveProbeResult
                    {
                        Feature = "event_window", Status = eventCancellation.IsCancellationRequested ? "cancelled" : "blocked",
                        Details = new List<string> { "Prewarm delay did not complete in the original match/lease epoch; no event read attempted." }
                    });
            }
            if (recoveryProbe) SampleRecovery(reader, recoveryTrigger);
            // Last queries: Dispose below cancels asynchronous discovery immediately after its bounded polling window.
            if (eventSeconds > 0 && eventCancellation.IsCancellationRequested)
            {
                foreach (string feature in new[] { "minimap", "tactical" })
                    results.Add(new LiveProbeResult { Feature = feature, Status = "not_run", Details = new List<string> { "Event window cancelled; skip optional map discovery and dispose reader." } });
            }
            else ProbeMaps(reader, openError, capturedRows != null && capturedRows.Count > 0 && capturedRows.Count(row => row.Relation == "自己") == 1);
            results.Add(new LiveProbeResult { Feature = "VerifyLive58", Status = "not_run", Details = new List<string> { "Aggregate iterator repeats Snapshot and contains its own 12-second minimap loop. Disabled in bounded runner; use independent feature evidence above." } });
        }
        finally { reader?.Dispose(); }
    }

    private static ProjectilePlayer[] Players(IEnumerable<PlayerInfoRow> rows) => rows.Select(row => new ProjectilePlayer { Address = row.PlayerAddress, Slot = row.Slot, Colour = row.GameColour, Relation = row.Relation }).ToArray();

    internal static void Count(LiveProbeResult result, int count)
    {
        result.SampleCount = count;
        result.Status = count == 0 ? "sample_absent" : "observed";
    }

    private void Probe(PlayerInfoReader reader, Exception openError, string feature, Action<LiveProbeResult> query)
    {
        var result = NewResult(reader, feature);
        var clock = Stopwatch.StartNew();
        long epoch = LicenseGate60.Epoch;
        try
        {
            if (reader == null) throw new SnapshotDependencyException("Reader initialization failed: " + openError);
            if (!LicenseGate60.Allowed) throw new InvalidOperationException("Live read unavailable: authorization lease not allowed.");
            query(result);
            if (!LicenseGate60.Allowed || LicenseGate60.Epoch != epoch) throw new InvalidOperationException("Authorization lease changed during query; sample invalidated.");
            if (result.Status == "pending") result.Status = "observed";
        }
        catch (SnapshotDependencyException exception) { result.Status = "blocked"; result.Error = exception.ToString(); }
        catch (Exception exception) { result.Status = "fail"; result.Error = exception.ToString(); }
        result.ElapsedMilliseconds = clock.ElapsedMilliseconds;
        results.Add(result);
        Console.WriteLine(result.Feature + ": " + result.Status + " samples=" + result.SampleCount + (result.Error == null ? "" : " " + result.Error));
    }

    private static LiveProbeResult NewResult(PlayerInfoReader reader, string feature) => new LiveProbeResult { Feature = feature, Status = "pending", BuildStatus = reader?.BuildStatus.ToString(), StartedUtc = DateTime.UtcNow.ToString("O") };

    private void ProbeMaps(PlayerInfoReader reader, Exception openError, bool snapshotAvailable)
    {
        var mini = NewResult(reader, "minimap");
        var tactical = NewResult(reader, "tactical");
        var clock = Stopwatch.StartNew();
        long epoch = LicenseGate60.Epoch;
        int attempts = 0;
        if (reader == null || !snapshotAvailable)
        {
            foreach (var result in new[] { mini, tactical }) { result.Status = "blocked"; result.Error = "Reader/snapshot dependency unavailable: " + openError; }
        }
        else
        {
            BoundedPoll.Until(() =>
            {
                foreach (var result in new[] { mini, tactical })
                {
                    if (clock.ElapsedMilliseconds >= 5000) break;
                    if (result.Status != "pending") continue;
                    try
                    {
                        if (!LicenseGate60.Allowed || LicenseGate60.Epoch != epoch) throw new InvalidOperationException("Authorization changed during map discovery.");
                        MinimapFrame frame;
                        bool found = result == mini ? reader.TryReadMinimap(out frame) : reader.TryReadTactical51(out frame);
                        if (found)
                        {
                            if (frame == null || !frame.Valid) throw new InvalidOperationException("Map method returned invalid frame.");
                            result.Evidence = new { frame.CentreX, frame.CentreY, frame.PixelWidth, frame.PixelHeight, frame.RootWidth, frame.RootHeight, frame.WorldWidth, frame.WorldHeight, frame.Angle, frame.NativeTransform, frame.WorldToUi };
                            Count(result, 1);
                        }
                    }
                    catch (Exception exception) { result.Status = "fail"; result.Error = exception.ToString(); }
                }
                return mini.Status != "pending" && tactical.Status != "pending";
            }, () => clock.ElapsedMilliseconds, Thread.Sleep, out attempts);
            foreach (var result in new[] { mini, tactical })
            {
                if (result.Status == "pending")
                {
                    result.Status = result == tactical && (reader.TacticalStatus51 ?? "").Contains("未打开") ? "sample_absent" : "discovery_timeout";
                    if (result.Status == "sample_absent") result.SampleCount = 0;
                    result.Error = "Bounded discovery ended; not a layout-invalid conclusion.";
                }
                if (!LicenseGate60.Allowed || LicenseGate60.Epoch != epoch) { result.Status = "fail"; result.Error = "Authorization changed; map evidence invalidated."; }
            }
        }
        foreach (var result in new[] { mini, tactical })
        {
            result.ElapsedMilliseconds = clock.ElapsedMilliseconds;
            result.Details.Add("sharedBudgetMilliseconds=5000; maxPollingRounds=101; pollingRounds=" + attempts + "; reader disposed after map probes; cancellation is cooperative.");
            result.Details.Add("minimapStatus=" + reader?.MinimapStatus + "; minimapDiagnostic=" + reader?.MinimapDiagnostic + "; tacticalStatus=" + reader?.TacticalStatus51);
            results.Add(result);
        }
    }
}

internal sealed class SnapshotDependencyException : Exception
{
    internal SnapshotDependencyException(string message) : base(message) { }
}
