using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using CoH3MemoryTool;

// Isolated BCL fixture: links only GameLayout52426, never PlayerInfoReader/PInvoke/UI.
static class Program
{
    private static int checks;
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        checks++;
    }

    private static HashSet<long> Dependencies(string root)
    {
        string reader = File.ReadAllText(Path.Combine(root, "PlayerInfoReader.cs"));
        var keys = new HashSet<long>();
        void Add(string source, string pattern, int group = 1)
        {
            foreach (Match match in Regex.Matches(source, pattern))
                foreach (Match number in Regex.Matches(match.Groups[group].Value, @"\d+"))
                    keys.Add(long.Parse(number.Value));
        }
        Add(reader, @"BuildRva45\(([^()]*)\)"); // includes vehicle ternary; variables covered below
        Add(reader, @"Component\([^\r\n;]*?,\s*(\d+)L?\)");
        Add(reader, @"MiniPropertyId\((\d+)L?\)");
        foreach (Match match in Regex.Matches(reader, @"case\s+(\d+)L?:"))
            if (long.Parse(match.Groups[1].Value) > 1000000) keys.Add(long.Parse(match.Groups[1].Value));
        foreach (string helper in Directory.GetFiles(root, "*.cs"))
        {
            string source = File.ReadAllText(helper);
            if (!source.Contains("resolveRva?.Invoke")) continue;
            Add(source, @"resolveRva\?\.Invoke\((\d+)\)");
            // Detect future direct exact guards that bypass the mapper. The only image range bound
            // is in the reader, not these decoders; helpers have no permitted direct image constants.
            Check(!Regex.IsMatch(source, @"\bimage\s*\+\s*\d+"), helper + " has a direct image RVA guard");
        }
        return keys;
    }

    public static int Main(string[] args)
    {
        string repo = Path.GetFullPath(args[0]);
        string sky = Path.Combine(repo, "CoH3/dev/SkyEye/client/CoH3MemoryTool");
        string simple = Path.Combine(repo, "CoH3/dev/COH3SimpleTool/CoH3MemoryTool");
        foreach (string file in new[] { "GameLayout52426.cs", "PlayerInfoReader.cs", "GameVersion45.cs", "AirLogic.cs" })
            Check(File.ReadAllBytes(Path.Combine(sky, file)).SequenceEqual(File.ReadAllBytes(Path.Combine(simple, file))), "mirror mismatch: " + file);
        foreach (string root in new[] { sky, simple })
        {
            string source = File.ReadAllText(Path.Combine(root, "PlayerInfoReader.cs"));
            string block = @"GameVersion45\.RequireVerified\(BuildStatus, maintenanceDiagnostics\);\s*ValidateLayout\(\);\s*if \(!LicenseGate60\.Matches\([^{}]*\{[^{}]*\}\s*handle = Native\.OpenProcess";
            Check(Regex.Matches(source, block, RegexOptions.Singleline).Count == 2, "Both constructors must preflight after hash and before licensing/open");
            Check(source.Contains("GameVersion45.GameBuild.Build52426") && source.Contains("GameVersion45.GameBuild.Build2406"), "reader dispatches every registered build: " + root);
            Check(source.Contains("GameLayout52426.Resolve(legacy)") && source.Contains("GameLayout52426.LegacyTransform(actual)"), "reader routes legacy roles through the selected table: " + root);
            Check(!source.Contains("RequireCandidate"), "fingerprint decision belongs to GameVersion45: " + root);
        }
        Type type = typeof(GameLayout52426);
        // The layout table ships in every build now: the fingerprint decides which table is used,
        // so both the client build and the maintenance build must expose the same 37 roles.
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var required = (long[])type.GetField("requiredDependencies", flags).GetValue(null);
        var candidates = (Dictionary<long, long>)type.GetField("candidates", flags).GetValue(null);
        Check(required.Length == 37 && required.Distinct().Count() == 37, "37 unique required roles");
        Check(required.ToHashSet().SetEquals(candidates.Keys), "candidate/required key sets differ");
        foreach (string root in new[] { sky, simple })
        {
            HashSet<long> scanned = Dependencies(root);
            Check(scanned.Count == 37, "reader/helper scanner must collect all 37 current roles");
            Check(scanned.SetEquals(required), "reader/helper source dependency drift");
            // Mutation demonstrates that adding a future decoder role without declaring it is detected.
            scanned.Add(123456789);
            Check(!scanned.IsSubsetOf(required), "future helper dependency drift fixture");
        }
        GameLayout52426.ValidateDependencies();
        Check(true, "complete layout preflight passes");
        foreach (long key in required)
        {
            long value = candidates[key];
            candidates.Remove(key);
            try
            {
                bool thrown = false;
                try { GameLayout52426.ValidateDependencies(); }
                catch (InvalidOperationException error)
                {
                    thrown = true;
                    Check(error.Message.Contains("0x" + key.ToString("X")), "missing role error must identify key");
                }
                Check(thrown, "missing role was swallowed: " + key);
            }
            finally { candidates.Add(key, value); }
        }
        GameLayout52426.ValidateDependencies();
        Check(candidates.Count == 37, "all candidate mutations restored");
        byte[] assembly = File.ReadAllBytes(Assembly.GetExecutingAssembly().Location);
        string layoutSource = File.ReadAllText(Path.Combine(sky, "GameLayout52426.cs"));
        string candidateHash = Regex.Match(layoutSource, "BuildHash = \"([a-f0-9]{64})\"").Groups[1].Value;
        Check(candidateHash.Length == 64, "candidate hash source probe");
        Check(assembly.AsSpan().IndexOf(System.Text.Encoding.Unicode.GetBytes(candidateHash)) >= 0, "every build ships the candidate layout table");
#if MAINTENANCE_DIAGNOSTICS
        string mode = "Maintenance";
#else
        string mode = "Release";
#endif
        Console.WriteLine(JsonSerializer.Serialize(new { status = "PASS_fixture_only", mode, checks, real_game = "not_run", ui = "not_run", native_process_open = "not_linked" }));
        return 0;
    }
}
