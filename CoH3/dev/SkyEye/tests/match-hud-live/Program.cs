using CoH3MemoryTool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

internal static class Program
{
    const int PinnedPid = 30504, Polls = 40, IntervalMs = 250, MaxReads = 12000;
    const long MaxRequested = 8L * 1024 * 1024, MaxOutput = 32L * 1024 * 1024;
    const string StartPin = "2026-11-22T01:23:35.3993114Z";
    const string ModulePin = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec";
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static readonly FieldInfo WalkingField = typeof(MatchHudGuard).GetField("walking", Private);
    static readonly Type WalkingType = WalkingField.FieldType;
    static readonly FieldInfo WalkAddress = WalkingType.GetField("Address", Private);
    static readonly FieldInfo WalkCursor = WalkingType.GetField("Cursor", Private);
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 };

    static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Region
        {
            internal IntPtr Base, AllocationBase;
            internal uint AllocationProtect;
            internal ushort PartitionId;
            internal UIntPtr Size;
            internal uint State, Protect, Type;
        }
        [DllImport("kernel32.dll", SetLastError = true)] internal static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadProcessMemory(IntPtr handle, IntPtr address, [Out] byte[] buffer, UIntPtr count, out UIntPtr actual);
        [DllImport("kernel32.dll", SetLastError = true)] internal static extern UIntPtr VirtualQueryEx(IntPtr handle, IntPtr address, out Region info, UIntPtr size);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool GetExitCodeProcess(IntPtr handle, out uint code);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] internal static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")] internal static extern void SetLastError(uint error);
    }

    sealed class Output : IDisposable
    {
        readonly FileStream journal;
        internal long Bytes, PendingEstimate;
        internal bool JournalComplete = true;
        internal readonly string Directory;
        internal Output(string directory)
        {
            Directory = directory;
            if (System.IO.Directory.Exists(directory) || File.Exists(directory)) throw new IOException("output already exists");
            string scope = Path.Combine(SourcePins.Repo, "CoH3/dev/SkyEye/tests/match-hud-live") + Path.DirectorySeparatorChar;
            if (!directory.StartsWith(scope, StringComparison.OrdinalIgnoreCase)) throw new IOException("output must be a new directory within match-hud-live");
            for (string parent = Path.GetDirectoryName(directory); parent != null && parent.Length >= scope.Length - 1; parent = Path.GetDirectoryName(parent))
                if (System.IO.Directory.Exists(parent) && (File.GetAttributes(parent) & FileAttributes.ReparsePoint) != 0) throw new IOException("output parent is a reparse point");
            System.IO.Directory.CreateDirectory(directory);
            journal = new FileStream(Path.Combine(directory, "journal.jsonl"), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        }
        internal bool Room(long additional) { return Bytes + PendingEstimate + additional <= MaxOutput - 1024 * 1024; }
        internal void Journal(object record)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Json.Serialize(record) + "\n");
                if (Bytes + bytes.Length > MaxOutput - 1024 * 1024) throw new IOException("output_cap_before_record; journal is partial");
                journal.Write(bytes, 0, bytes.Length); Bytes += bytes.Length;
            }
            catch { JournalComplete = false; throw; }
        }
        internal void Flush() { journal.Flush(); PendingEstimate = 0; }
        internal void Summary(object record)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(Json.Serialize(record));
            if (Bytes + bytes.Length > MaxOutput) throw new IOException("output_cap_before_summary");
            using (var file = new FileStream(Path.Combine(Directory, "summary.json"), FileMode.CreateNew, FileAccess.Write, FileShare.Read)) file.Write(bytes, 0, bytes.Length);
            Bytes += bytes.Length;
        }
        public void Dispose() { journal.Dispose(); }
    }

    sealed class ReadAttempt
    {
        internal Dictionary<string, object> Row = new Dictionary<string, object>();
        internal byte[] Buffer;
        internal int Returned;
        internal object Record()
        {
            Row["rawHex"] = Buffer == null || Returned <= 0 ? "" : BitConverter.ToString(Buffer, 0, Returned).Replace("-", "").ToLowerInvariant();
            return Row;
        }
    }

    sealed class Reader
    {
        internal IntPtr Handle;
        internal MatchHudGuard Guard;
        internal int Poll;
        internal long Attempts, BudgetedReads, Requested, Actual, RpmIssued, RpmFailed, PrecheckFailures;
        internal string StopReason;
        internal readonly List<ReadAttempt> Logs = new List<ReadAttempt>();
        int flushed;
        readonly Stopwatch global;
        readonly Output output;
        internal Reader(IntPtr handle, Stopwatch global, Output output) { Handle = handle; this.global = global; this.output = output; }
        internal byte[] Read(long address, int count)
        {
            var timer = Stopwatch.StartNew();
            var item = new ReadAttempt(); Logs.Add(item);
            var row = item.Row;
            row["event"] = "read_delegate"; row["poll"] = Poll; row["attempt"] = ++Attempts;
            row["address"] = Hex(address); row["requested"] = count; row["actual"] = 0UL;
            row["rpmIssued"] = false; row["nativeOK"] = null; row["nativeError"] = null;
            row["utc"] = DateTime.UtcNow.ToString("o"); row["vq"] = new List<object>();
            object walking = Guard == null ? null : WalkingField.GetValue(Guard);
            row["walking"] = walking == null ? null : Hex((long)WalkAddress.GetValue(walking));
            row["walkingCursor"] = walking == null ? (object)null : WalkCursor.GetValue(walking);
            string reason = null;
            try
            {
                if (StopReason != null) throw new IOException(StopReason);
                if (global.Elapsed.TotalSeconds >= 30) { StopReason = "global_30s_cap"; throw new IOException(StopReason); }
                if (BudgetedReads >= MaxReads || count > 0 && Requested + count > MaxRequested) { StopReason = "global_read_or_request_cap"; throw new IOException(StopReason); }
                long estimate = 16384L + Math.Max(0L, count) * 2;
                if (!output.Room(estimate)) { StopReason = "output_32MiB_reservation_cap"; throw new IOException(StopReason); }
                output.PendingEstimate += estimate;
                BudgetedReads++; if (count > 0) Requested += count;
                if (!Native.GetExitCodeProcess(Handle, out uint code)) { StopReason = "process_status_failure"; throw new IOException(StopReason + ":" + Marshal.GetLastWin32Error()); }
                if (code != 259) { StopReason = "process_exited"; throw new IOException(StopReason); }
                if (address < 65536 || count <= 0 || count > 65536 || address >= 140737488355328L || count > 140737488355328L - address)
                { PrecheckFailures++; throw new IOException("precheck_address_length_or_overflow"); }
                long end = address + count, cursor = address;
                int regions = 0;
                var query = Stopwatch.StartNew();
                while (cursor < end)
                {
                    if (++regions > 32) { PrecheckFailures++; throw new IOException("precheck_region_count_cap"); }
                    Native.SetLastError(0);
                    UIntPtr queried = Native.VirtualQueryEx(Handle, new IntPtr(cursor), out Native.Region region, new UIntPtr((uint)Marshal.SizeOf(typeof(Native.Region))));
                    int error = Marshal.GetLastWin32Error();
                    ((List<object>)row["vq"]).Add(new { address = Hex(cursor), returned = queried.ToUInt64(), error, regionBase = Hex(region.Base.ToInt64()), bytes = region.Size.ToUInt64(), region.State, region.Protect, region.Type });
                    ulong size = region.Size.ToUInt64(); long begin = region.Base.ToInt64(); uint permission = region.Protect & 255;
                    if (queried.ToUInt64() != (ulong)Marshal.SizeOf(typeof(Native.Region)) || size == 0 || size > long.MaxValue || begin < 0 || begin > long.MaxValue - (long)size)
                    { PrecheckFailures++; throw new IOException("precheck_vq_failed_or_invalid_region"); }
                    long regionEnd = begin + (long)size;
                    bool readable = permission == 2 || permission == 4 || permission == 8 || permission == 0x20 || permission == 0x40 || permission == 0x80;
                    if (region.State != 0x1000 || (region.Protect & 0x100) != 0 || !readable || begin > cursor || regionEnd <= cursor)
                    { PrecheckFailures++; throw new IOException("precheck_region_not_readable"); }
                    cursor = Math.Min(end, regionEnd);
                }
                row["vqMs"] = query.Elapsed.TotalMilliseconds;
                item.Buffer = new byte[count]; Native.SetLastError(0);
                var rpm = Stopwatch.StartNew(); RpmIssued++; row["rpmIssued"] = true;
                bool ok = Native.ReadProcessMemory(Handle, new IntPtr(address), item.Buffer, new UIntPtr((uint)count), out UIntPtr actual);
                int nativeError = Marshal.GetLastWin32Error(); rpm.Stop(); ulong returned = actual.ToUInt64();
                row["nativeOK"] = ok; row["nativeError"] = nativeError; row["actual"] = returned; row["rpmMs"] = rpm.Elapsed.TotalMilliseconds;
                item.Returned = (int)Math.Min((ulong)count, returned); Actual += item.Returned;
                if (!ok || returned != (ulong)count) { RpmFailed++; throw new IOException("rpm_failed_or_short_read"); }
                return item.Buffer;
            }
            catch (Exception error) { reason = error.Message; row["originalException"] = error.GetType().FullName; throw; }
            finally { timer.Stop(); row["originalReason"] = reason; row["delegateMs"] = timer.Elapsed.TotalMilliseconds; }
        }
        internal object Stats() { return new { logicalAttempts = Attempts, budgetedReads = BudgetedReads, requestedBytes = Requested, actualBytes = Actual, rpmIssued = RpmIssued, rpmFailedOrShort = RpmFailed, precheckFailures = PrecheckFailures, StopReason }; }
        internal void BeginPoll(int poll) { Poll = poll; Logs.Clear(); flushed = 0; }
        internal void FlushLogs(Output target) { while (flushed < Logs.Count) { target.Journal(Logs[flushed].Record()); flushed++; } target.Flush(); }
        internal object RawType(MatchHudContext token, long image, long rootRva, long hudRva)
        {
            ReadAttempt root = Logs.LastOrDefault(x => x.Row["address"].Equals(Hex(token.Root)) && x.Returned >= 8 && (bool?)x.Row["nativeOK"] == true);
            ReadAttempt hud = Logs.LastOrDefault(x => x.Row["address"].Equals(Hex(token.Hud)) && x.Returned >= 8 && (bool?)x.Row["nativeOK"] == true);
            long rootType = root == null ? 0 : BitConverter.ToInt64(root.Buffer, 0), hudType = hud == null ? 0 : BitConverter.ToInt64(hud.Buffer, 0);
            return new { rootType = Hex(rootType), hudType = Hex(hudType), expectedRootType = Hex(image + rootRva), expectedHudType = Hex(image + hudRva), rootArithmeticMatch = rootType == image + rootRva, hudArithmeticMatch = hudType == image + hudRva, source = "this_poll_exact_read_journal" };
        }
    }

    static string Hex(long value) { return "0x" + unchecked((ulong)value).ToString("x", CultureInfo.InvariantCulture); }
    static string Hash(string path)
    {
        using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
    }
    static object Sources(bool check)
    {
        var list = new List<object>();
        long sourceBytes = 0;
        foreach (string[] pin in SourcePins.Files)
        {
            long size = new FileInfo(pin[0]).Length;
            sourceBytes += size;
            if (size > 16L * 1024 * 1024 || sourceBytes > 32L * 1024 * 1024) throw new IOException("source_32MiB_cap");
            string actual = Hash(pin[0]); bool match = actual.Equals(pin[1], StringComparison.OrdinalIgnoreCase);
            list.Add(new { path = pin[0], expectedSha256 = pin[1], actualSha256 = actual, match });
            if (check && !match) throw new IOException("source_pin_changed: " + pin[0]);
        }
        return list;
    }
    static object GuardStats(MatchHudGuard guard)
    {
        object Value(string name) { return typeof(MatchHudGuard).GetField(name, Private).GetValue(guard); }
        int Count(string name) { object value = Value(name); return (int)value.GetType().GetProperty("Count").GetValue(value, null); }
        object walking = Value("walking"), bound = Value("bound");
        return new { pending = Count("pending"), seen = Count("seen"), candidates = Count("candidates"), walkRoot = Hex((long)Value("walkRoot")), walkReadFault = Value("walkReadFault"), walkLimited = Value("walkLimited"), walking = walking == null ? null : Hex((long)WalkAddress.GetValue(walking)), cursor = walking == null ? (object)null : WalkCursor.GetValue(walking), hasBound = bound != null, readOnlyReflection = true };
    }
    static object Identity(Process process, long expectedStart, out long image)
    {
        var watch = Stopwatch.StartNew(); process.Refresh();
        if (process.HasExited) throw new EndOfStreamException("process_exited");
        long start = process.StartTime.ToUniversalTime().Ticks;
        using (ProcessModule module = process.MainModule)
        {
            image = module.BaseAddress.ToInt64(); string path = module.FileName, sha = Hash(path);
            if (start != expectedStart || process.Id != PinnedPid || !Path.GetFileName(path).Equals("RelicCoH3.exe", StringComparison.OrdinalIgnoreCase) || !sha.Equals(ModulePin, StringComparison.OrdinalIgnoreCase)) throw new IOException("process_identity_pin_changed");
            if (image < 65536 || SourcePins.RootSlotRva >= module.ModuleMemorySize || SourcePins.RootTypeRva >= module.ModuleMemorySize || SourcePins.HudTypeRva >= module.ModuleMemorySize) throw new IOException("module_base_or_layout_outside_image");
            return new { pid = process.Id, startUtc = process.StartTime.ToUniversalTime().ToString("o"), modulePath = path, moduleBase = Hex(image), imageBytes = module.ModuleMemorySize, moduleSha256 = sha, identityAndHashMs = watch.Elapsed.TotalMilliseconds };
        }
    }
    static Dictionary<string, string> Arguments(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (args.Length != 10) throw new ArgumentException("explicit --repo --pid --start-utc --module-sha256 --out pairs required");
        for (int i = 0; i < args.Length; i += 2) map.Add(args[i], args[i + 1]);
        if (map.Count != 5 || !map.ContainsKey("--out") || map["--pid"] != PinnedPid.ToString() || map["--start-utc"] != StartPin || !map["--module-sha256"].Equals(ModulePin, StringComparison.OrdinalIgnoreCase) || !Path.GetFullPath(map["--repo"]).TrimEnd('\\', '/').Equals(SourcePins.Repo.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("only compiled PID/start/module/workspace pins are accepted");
        return map;
    }

    public static int Main(string[] args)
    {
        Output output = null; Reader reader = null; Process process = null; IntPtr handle = IntPtr.Zero;
        var global = Stopwatch.StartNew(); var rounds = new List<object>();
        string stop = "not_started", fatal = null; int exit = 0, readyCount = 0, afterFirstReady = 0;
        bool firstReady = false, closeAttempted = false, closeOK = false, openAttempted = false; int closeError = 0;
        object sourcesBefore = null, sourcesAfter = null, identityFirst = null;
        string executable = Assembly.GetExecutingAssembly().Location, executableHash = Hash(executable);
        try
        {
            var options = Arguments(args); output = new Output(Path.GetFullPath(options["--out"]));
            sourcesBefore = Sources(true);
            output.Journal(new { @event = "start", schema = "production-hud-qualification-only-v1", productAcceptance = false, productGUI = "not_run", ReaderConstructed = false, classification = "qualification-only", executable, executableHash, sourcesBefore, generatedPinsPath = SourcePins.GeneratedPath, generatedPinsSha256 = Hash(SourcePins.GeneratedPath), rawReferencePath = SourcePins.RawReference, rawReferenceSha256 = SourcePins.RawReferenceSha256, rawArithmeticCrosscheck = true, layout = new { SourcePins.RootSlotRva, SourcePins.RootTypeRva, SourcePins.HudTypeRva }, accessMask = "0x410", bounds = new { polls = Polls, intervalMs = IntervalMs, seconds = 30, reads = MaxReads, requestedBytes = MaxRequested, outputBytes = MaxOutput, helperReads = 256, helperBytes = 131072, helperMs = 12 }, journalEncoding = "UTF8 JSONL; rawHex actual returned bytes; flush outside Resolve" });
            try { process = Process.GetProcessById(PinnedPid); }
            catch (ArgumentException) { throw new EndOfStreamException("process_exited_before_open"); }
            long expectedStart = DateTime.ParseExact(StartPin, "o", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal).Ticks;
            identityFirst = Identity(process, expectedStart, out long image);
            openAttempted = true;
            Native.SetLastError(0); handle = Native.OpenProcess(0x410, false, PinnedPid); int openError = Marshal.GetLastWin32Error();
            output.Journal(new { @event = "open_process", accessMask = "0x410", ok = handle != IntPtr.Zero, error = openError, identityFirst });
            if (handle == IntPtr.Zero) throw new IOException("OpenProcess_failed:" + openError);
            reader = new Reader(handle, global, output);
            MatchHudGuard guard = new MatchHudGuard(reader.Read, checked(image + SourcePins.RootSlotRva), checked(image + SourcePins.RootTypeRva), checked(image + SourcePins.HudTypeRva), image);
            reader.Guard = guard;
            string world = "diagnostic-qualification-only:" + PinnedPid + ":" + expectedStart + ":" + Hex(image);
            long lastResolveStart = -IntervalMs;
            for (int poll = 0; poll < Polls; poll++)
            {
                if (global.Elapsed.TotalSeconds >= 30) { stop = "global_30s_cap"; break; }
                object identity = Identity(process, expectedStart, out long currentImage);
                if (currentImage != image) throw new IOException("module_base_changed");
                long scheduled = lastResolveStart + IntervalMs;
                long delay = scheduled - global.ElapsedMilliseconds;
                if (delay > 0) Thread.Sleep((int)delay);
                if (global.Elapsed.TotalSeconds >= 30) { stop = "global_30s_cap"; break; }
                reader.BeginPoll(poll); object before = GuardStats(guard);
                string outcome = null, message = null; MatchHudContext token = null;
                long attemptBefore = reader.Attempts, rpmBefore = reader.RpmIssued, bytesBefore = reader.Requested;
                bool cached = firstReady;
                long actualResolveStart = global.ElapsedMilliseconds;
                long sincePreviousResolveMs = lastResolveStart < 0 ? 0 : actualResolveStart - lastResolveStart;
                lastResolveStart = actualResolveStart;
                var watch = Stopwatch.StartNew();
                try { token = guard.Resolve(world); outcome = "Ready"; }
                catch (MatchHudUnavailableException error) { outcome = error.Outcome.ToString(); message = error.Message; }
                watch.Stop(); object raw = token == null ? null : reader.RawType(token, image, SourcePins.RootTypeRva, SourcePins.HudTypeRva);
                var result = new { @event = "resolve", poll, scheduledEarliestMs = scheduled, actualResolveStartMs = actualResolveStart, sincePreviousResolveMs, actualEndMs = global.Elapsed.TotalMilliseconds, identity, diagnosticWorld = world, productEpoch = (object)null, diagnosticSoftwareEpoch = token == null ? (object)null : token.Epoch, root = token == null ? null : Hex(token.Root), hud = token == null ? null : Hex(token.Hud), rawTypeCrosscheck = raw, cachedAfterFirstReady = cached, outcome, message, resolveMs = watch.Elapsed.TotalMilliseconds, attempted = reader.Attempts - attemptBefore, rpmIssued = reader.RpmIssued - rpmBefore, requestedBytes = reader.Requested - bytesBefore, before, after = GuardStats(guard), counters = reader.Stats() };
                var flush = Stopwatch.StartNew(); reader.FlushLogs(output); flush.Stop();
                output.Journal(result); output.Journal(new { @event = "journal_flush_cost", poll, ms = flush.Elapsed.TotalMilliseconds }); output.Flush(); rounds.Add(result);
                if (token != null) { readyCount++; if (!firstReady) firstReady = true; }
                if (cached) afterFirstReady++;
                if (reader.StopReason != null) { stop = reader.StopReason; break; }
                if (afterFirstReady >= 10) { stop = "first_ready_plus_10_cached_attempts"; break; }
                stop = "40_poll_cap";
            }
        }
        catch (EndOfStreamException) { stop = "process_exited"; }
        catch (ArgumentException error) { fatal = error.ToString(); stop = "argument_or_process_failure"; exit = 2; }
        catch (Exception error) { fatal = error.ToString(); stop = "identity_or_program_failure"; exit = 2; }
        finally
        {
            if (handle != IntPtr.Zero) { closeAttempted = true; Native.SetLastError(0); closeOK = Native.CloseHandle(handle); closeError = Marshal.GetLastWin32Error(); if (!closeOK) { exit = 2; fatal = (fatal ?? "") + "; CloseHandle_failed:" + closeError; } }
            if (process != null) process.Dispose();
            if (output != null && reader != null && output.JournalComplete)
            {
                try { reader.FlushLogs(output); }
                catch (Exception error) { output.JournalComplete = false; exit = 2; fatal = (fatal ?? "") + "; final_journal_drain_failure:" + error; }
            }
            try { sourcesAfter = Sources(false); Sources(true); } catch (Exception error) { exit = 2; fatal = (fatal ?? "") + "; source_after_failure:" + error; }
            if (output != null)
            {
                try
                {
                    output.Summary(new { schema = "production-hud-qualification-only-v1", status = exit == 0 ? (readyCount > 0 ? "qualification_ready_observed" : "qualification_not_ready_within_bounds") : "identity_or_program_failure", exitCode = exit, stopReason = stop, fatal, productAcceptance = false, productGUI = "not_run", ReaderConstructed = false, classification = "qualification-only", productEpoch = (object)null, diagnosticWorldIsProductWorld = false, readyCount, afterFirstReady, rounds, counters = reader == null ? null : reader.Stats(), elapsedMs = global.Elapsed.TotalMilliseconds, sourcesBefore, sourcesAfter, executable, executableHash, executableHashAfter = Hash(executable), journalComplete = output.JournalComplete, capturePartial = !output.JournalComplete || stop.Contains("global_") || stop.Contains("output_") || stop == "process_exited" || exit != 0, openAttempted, nativeRpmIssued = reader == null ? 0 : reader.RpmIssued, closeAttempted, closeOK, closeError, outputBytesBeforeSummary = output.Bytes, costNotes = "Resolve includes VQ/native/delegate/reflection overhead. Hex/JSON/file flush runs after Resolve. 12ms bounds issued reads cooperatively; no hard native preemption." });
                }
                catch (Exception error) { exit = 2; Console.Error.WriteLine("summary_write_failure; partial journal: " + error); }
                output.Dispose();
            }
            Console.WriteLine(Json.Serialize(new { exitCode = exit, stopReason = stop, readyCount, productAcceptance = false, nativeRun = openAttempted, nativeRpmIssued = reader == null ? 0 : reader.RpmIssued, output = output == null ? null : output.Directory, fatal }));
        }
        return exit;
    }
}
