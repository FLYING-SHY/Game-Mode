using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CoH3MemoryTool;

// No engine, process, UI or authorization dependencies: the same control flow runs in the isolated fixture.
internal static class RecoveryProbe
{
    internal const int MaximumAttempts = 3;
    internal const int BudgetMilliseconds = 2000;

    internal static bool IsCameraFailure(Exception exception) => exception?.Message is
        "镜头列表正在更新。" or "未找到镜头对象。" or "镜头数值无效。" or
        "镜头格式不匹配。" or "镜头正在切换。" or "镜头已切换。";

    internal static RecoveryProbeEvidence Run(RecoveryProbeTrigger trigger, Func<RecoveryProbeContext> context,
        Func<object> readCamera, Func<object> readSnapshot, Func<long> elapsedMilliseconds,
        Func<string> utcNow, Action<int> sleep, Func<bool> cancelled)
    {
        var evidence = new RecoveryProbeEvidence();
        if (trigger == null) return evidence;
        evidence.Status = "fail";
        evidence.StopReason = "attempt_cap";
        evidence.TriggeredUtc = trigger.Utc;
        evidence.TriggerError = trigger.Error;
        evidence.InitialMatch = trigger.Match;
        evidence.InitialEpoch = trigger.Epoch;
        evidence.TriggerReaderIdentity = Identity(trigger.Reader);
        evidence.ReaderOpenAttemptsAtTrigger = trigger.ReaderOpenAttempts;
        evidence.StartContext = Guard(context(), trigger);
        try
        {
            for (int i = 0; i < MaximumAttempts; i++)
            {
                if (Stop()) break;
                var attempt = new RecoveryProbeAttempt { Ordinal = i + 1 };
                evidence.Attempts.Add(attempt);
                attempt.Camera = Read(readCamera, true);
                if (attempt.Camera.Status == "invalidated") { Invalidated(); break; }
                if (attempt.Camera.Status == "observed")
                {
                    if (Stop()) break;
                    attempt.Snapshot = Read(readSnapshot, false);
                    if (attempt.Snapshot.Status == "invalidated") { Invalidated(); break; }
                    if (attempt.Snapshot.Status == "observed")
                    {
                        evidence.Status = "observed";
                        evidence.StopReason = "camera_and_snapshot_observed";
                        evidence.CameraAndSnapshotRecovered = true;
                        break;
                    }
                }
                if (i + 1 < MaximumAttempts && !Stop())
                {
                    long remaining = BudgetMilliseconds - elapsedMilliseconds();
                    if (remaining > 0) sleep((int)Math.Min(100, remaining));
                }
            }
        }
        finally
        {
            evidence.EndContext = Guard(context(), trigger);
            if (!evidence.EndContext.Valid) Invalidated();
            evidence.ElapsedMilliseconds = elapsedMilliseconds();
            evidence.OverrunMilliseconds = Math.Max(0, evidence.ElapsedMilliseconds - BudgetMilliseconds);
        }
        return evidence;

        bool Stop()
        {
            if (!Guard(context(), trigger).Valid) { Invalidated(); return true; }
            if (cancelled()) { evidence.Status = "cancelled"; evidence.StopReason = "cancelled"; return true; }
            if (elapsedMilliseconds() >= BudgetMilliseconds) { evidence.StopReason = "deadline"; return true; }
            return false;
        }

        void Invalidated()
        {
            evidence.Status = "blocked";
            evidence.StopReason = "context_changed";
            evidence.CameraAndSnapshotRecovered = false;
        }

        RecoveryProbeRead Read(Func<object> query, bool camera)
        {
            var read = new RecoveryProbeRead
            {
                StartedUtc = utcNow(), StartedMilliseconds = elapsedMilliseconds(),
                Before = Guard(context(), trigger), Status = "invalidated"
            };
            try
            {
                if (!read.Before.Valid) return read;
                read.Called = true;
                if (camera) evidence.DirectCameraAttempts++; else evidence.SnapshotAttempts++;
                read.Evidence = query();
                read.Status = "observed";
            }
            catch (Exception exception) { read.Status = "fail"; read.Error = exception.ToString(); }
            finally
            {
                read.After = Guard(context(), trigger);
                if (!read.After.Valid) read.Status = "invalidated";
                read.FinishedUtc = utcNow();
                read.FinishedMilliseconds = elapsedMilliseconds();
            }
            return read;
        }
    }

    private static string Identity(object reader) => reader == null ? null : RuntimeHelpers.GetHashCode(reader).ToString("x8");

    private static RecoveryProbeGuard Guard(RecoveryProbeContext current, RecoveryProbeTrigger trigger) => new RecoveryProbeGuard
    {
        ReaderIdentity = Identity(current.Reader), SameReader = trigger.Reader != null && ReferenceEquals(current.Reader, trigger.Reader),
        ReaderOpenAttempts = current.ReaderOpenAttempts, Match = current.Match, Epoch = current.Epoch, Allowed = current.Allowed,
        Valid = trigger.Reader != null && ReferenceEquals(current.Reader, trigger.Reader) && trigger.ReaderOpenAttempts == 1 &&
            current.ReaderOpenAttempts == 1 && !string.IsNullOrEmpty(trigger.Match) && current.Match == trigger.Match &&
            current.Allowed && current.Epoch == trigger.Epoch
    };
}

internal sealed class RecoveryProbeTrigger
{
    internal object Reader;
    internal int ReaderOpenAttempts;
    internal string Match;
    internal long Epoch;
    internal string Utc;
    internal string Error;
}

internal sealed class RecoveryProbeContext
{
    internal object Reader;
    internal int ReaderOpenAttempts;
    internal string Match;
    internal long Epoch;
    internal bool Allowed;
}

internal sealed class RecoveryProbeEvidence
{
    public string Status { get; set; } = "not_triggered";
    public string StopReason { get; set; } = "no_natural_camera_error_in_event_window";
    public string TriggeredUtc { get; set; }
    public string TriggerError { get; set; }
    public string TriggerReaderIdentity { get; set; }
    public int ReaderOpenAttemptsAtTrigger { get; set; }
    public string InitialMatch { get; set; }
    public long? InitialEpoch { get; set; }
    public int MaximumAttempts => RecoveryProbe.MaximumAttempts;
    public int BudgetMilliseconds => RecoveryProbe.BudgetMilliseconds;
    public int DirectCameraAttempts { get; set; }
    public int SnapshotAttempts { get; set; }
    public bool CameraAndSnapshotRecovered { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public long OverrunMilliseconds { get; set; }
    public RecoveryProbeGuard StartContext { get; set; }
    public RecoveryProbeGuard EndContext { get; set; }
    public List<RecoveryProbeAttempt> Attempts { get; set; } = new List<RecoveryProbeAttempt>();
}

internal sealed class RecoveryProbeAttempt
{
    public int Ordinal { get; set; }
    public RecoveryProbeRead Camera { get; set; }
    public RecoveryProbeRead Snapshot { get; set; }
}

internal sealed class RecoveryProbeRead
{
    public bool Called { get; set; }
    public string Status { get; set; }
    public string StartedUtc { get; set; }
    public string FinishedUtc { get; set; }
    public long StartedMilliseconds { get; set; }
    public long FinishedMilliseconds { get; set; }
    public RecoveryProbeGuard Before { get; set; }
    public RecoveryProbeGuard After { get; set; }
    public object Evidence { get; set; }
    public string Error { get; set; }
}

internal sealed class RecoveryProbeGuard
{
    public string ReaderIdentity { get; set; }
    public bool SameReader { get; set; }
    public int ReaderOpenAttempts { get; set; }
    public string Match { get; set; }
    public long Epoch { get; set; }
    public bool Allowed { get; set; }
    public bool Valid { get; set; }
}
