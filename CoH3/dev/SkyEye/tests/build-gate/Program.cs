using CoH3MemoryTool;
using Status = CoH3MemoryTool.GameVersion45.CheckStatus;

internal static class Program
{
    private static int checks;

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL " + name);
        checks++;
        Console.WriteLine("PASS " + name);
    }

    private static bool Permits(Status status, bool maintenance)
    {
        try { GameVersion45.RequireVerified(status, maintenance); return true; }
        catch (InvalidOperationException) { return false; }
    }

    private static int Main(string[] args)
    {
        string root = Path.GetFullPath(args[0]);
        string temporary = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "fixture-files-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            Check(GameVersion45.Status == Status.NotChecked, "initial not checked");
            Check(GameVersion45.SupportedHash == "2406b3ee580bfc983b0aea93a31c26f9a6c98f10f706c5cec6a13ab5e3a22cde", "verified hash unchanged");
            Check(GameVersion45.SupportedHash52426 == "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec", "new build hash registered");
            Check(GameVersion45.MatchesHash(GameVersion45.SupportedHash.ToUpperInvariant()), "hash match case insensitive");
            Check(GameVersion45.MatchesHash(GameVersion45.SupportedHash52426.ToUpperInvariant()), "new build hash match case insensitive");
            Check(!GameVersion45.MatchesHash(null) && !GameVersion45.MatchesHash("") && !GameVersion45.MatchesHash(new string('0', 64)), "unknown hashes rejected");
            Check(GameVersion45.BuildOf(GameVersion45.SupportedHash) == GameVersion45.GameBuild.Build2406, "legacy fingerprint maps to legacy build");
            Check(GameVersion45.BuildOf(GameVersion45.SupportedHash52426) == GameVersion45.GameBuild.Build52426, "new fingerprint maps to new build");
            Check(GameVersion45.BuildOf(new string('0', 64)) == GameVersion45.GameBuild.Unknown, "unknown fingerprint maps to unknown build");
            foreach (Status status in Enum.GetValues<Status>())
            {
                Check(Permits(status, false) == (status == Status.Verified), "default gate " + status);
#if MAINTENANCE_DIAGNOSTICS
                Check(Permits(status, true), "explicit maintenance gate " + status);
#else
                Check(Permits(status, true) == (status == Status.Verified), "release ignores maintenance argument " + status);
#endif
            }
            string fixture = Path.Combine(temporary, "unknown-build.bin");
            File.WriteAllText(fixture, "offline fixture, not a game executable");
            Check(GameVersion45.Assess(fixture) == Status.FingerprintMismatch, "unknown build assess");
            Check(GameVersion45.Assess(fixture) == Status.FingerprintMismatch, "cached unknown build remains blocked");
            Status instanceResult = GameVersion45.Assess(fixture);
            Check(GameVersion45.Assess(Path.Combine(temporary, "missing.bin")) == Status.ReadFailed, "missing build read fails closed");
            Check(instanceResult == Status.FingerprintMismatch && !Permits(instanceResult, false), "instance assessment independent of global status");
            Check(GameVersion45.Assess(null) == Status.ReadFailed, "null path read fails closed");
            using (FileStream locked = new FileStream(fixture, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Check(GameVersion45.Assess(fixture) == Status.ReadFailed, "locked build read fails closed");
            Check(GameVersion45.Assess(fixture) == Status.FingerprintMismatch, "read failure recovery re-assesses");

            string reader = File.ReadAllText(Path.Combine(root, "PlayerInfoReader.cs"));
            string identity = File.ReadAllText(Path.Combine(root, "GameIdentityProvider60.cs"));
            Check(reader.Contains("public PlayerInfoReader() : this(false)") && reader.Contains("internal PlayerInfoReader(MarkerReadContext context) : this(context, false)"), "normal constructors force standard mode");
            foreach (string signature in new[] { "private PlayerInfoReader(MarkerReadContext context, bool maintenanceDiagnostics)", "private PlayerInfoReader(bool maintenanceDiagnostics)" })
            {
                int start = reader.IndexOf(signature, StringComparison.Ordinal);
                int assess = reader.IndexOf("BuildStatus = GameVersion45.Assess(", start, StringComparison.Ordinal);
                int gate = reader.IndexOf("GameVersion45.RequireVerified(BuildStatus, maintenanceDiagnostics);", start, StringComparison.Ordinal);
                int license = reader.IndexOf("LicenseGate60.Matches(", start, StringComparison.Ordinal);
                int open = reader.IndexOf("Native.OpenProcess(", start, StringComparison.Ordinal);
                Check(start >= 0 && start < assess && assess < gate && gate < license && license < open, "assessment and gates precede native handle: " + signature);
            }
            int factory = reader.IndexOf("internal static PlayerInfoReader OpenForMaintenanceDiagnostics()", StringComparison.Ordinal);
            int guard = reader.LastIndexOf("#if MAINTENANCE_DIAGNOSTICS", factory, StringComparison.Ordinal);
            int end = reader.IndexOf("#endif", guard, StringComparison.Ordinal);
            Check(guard >= 0 && factory > guard && end > factory && reader.IndexOf("OpenForMaintenanceDiagnostics(MarkerReadContext context)", factory, StringComparison.Ordinal) < end, "maintenance factories compilation isolated");
            Check(reader.Contains("new PlayerInfoReader(MarkerContext(), maintenanceDiagnostics)"), "diagnostic child connection preserves explicit mode");
            Check(reader.Contains("MAINTENANCE DIAGNOSTICS (not build verification)"), "diagnostic output never claims build verified");
            Check(reader.Contains("GameVersion45.GameBuild.Build52426") && reader.Contains("GameLayout52426.Resolve(legacy)"), "reader selects the layout of the detected build");
            Check(reader.Contains("GameVersion45.GameBuild.Build2406") && reader.Contains("GameVersion45.GameBuild.Build52426"), "reader dispatches every registered build explicitly");
            Check(!reader.Contains("if (maintenanceDiagnostics) return AirMoverShape.Candidate52426_184;"), "mover shape follows the detected build, not the diagnostic flag");
            Check(!reader.Contains("GameLayout52426.RequireCandidate("), "fingerprint decision belongs to GameVersion45 alone");
            Check(identity.Contains("|| GameVersion45.Status == GameVersion45.CheckStatus.ReadFailed"), "identity retries failed assessment without blocking log identity");
            Check(!reader.Contains("GetEnvironmentVariable") && !File.ReadAllText(Path.Combine(root, "GameVersion45.cs")).Contains("GetEnvironmentVariable"), "no environment variable bypass");
            if (args.Length > 1)
            {
                var assembly = System.Reflection.Assembly.LoadFile(Path.GetFullPath(args[1]));
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
                var compiledReader = assembly.GetType("CoH3MemoryTool.PlayerInfoReader", true);
                var factories = compiledReader.GetMethods(flags).Where(method => method.Name == "OpenForMaintenanceDiagnostics").ToArray();
#if MAINTENANCE_DIAGNOSTICS
                Check(factories.Length == 2, "compiled maintenance binary has explicit factories");
#else
                Check(factories.Length == 0, "compiled release binary has no maintenance factories");
#endif
                var version = assembly.GetType("CoH3MemoryTool.GameVersion45", true);
                Check((string)version.GetField("SupportedHash52426", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null) == "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec", "compiled binary registers the new build hash");
                var layout = assembly.GetType("CoH3MemoryTool.GameLayout52426", false);
                Check(layout != null, "compiled binary ships the 52426 layout table");
                var moverEnum = assembly.GetType("CoH3MemoryTool.AirMoverShape", false);
                Check(moverEnum != null && Enum.GetNames(moverEnum).Contains("Candidate52426_184"), "compiled binary ships the candidate mover shape");
                var statusType = version.GetNestedType("CheckStatus", System.Reflection.BindingFlags.NonPublic);
                var require = version.GetMethod("RequireVerified", flags);
                foreach (Status status in Enum.GetValues<Status>())
                {
                    foreach (bool maintenance in new[] { false, true })
                    {
                        bool allowed;
                        try { require.Invoke(null, new[] { Enum.Parse(statusType, status.ToString()), (object)maintenance }); allowed = true; }
                        catch (System.Reflection.TargetInvocationException exception) when (exception.InnerException is InvalidOperationException) { allowed = false; }
#if MAINTENANCE_DIAGNOSTICS
                        bool expected = maintenance || status == Status.Verified;
#else
                        bool expected = status == Status.Verified;
#endif
                        Check(allowed == expected, "compiled binary gate " + status + " maintenance=" + maintenance);
                    }
                }
            }
            Console.WriteLine("PASS total=" + checks + "; fixture only; no game launch or attach");
            return 0;
        }
        finally
        {
            // The exact directory was created above beneath this fixture's own build output.
            Directory.Delete(temporary, true);
        }
    }
}
