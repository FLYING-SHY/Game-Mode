using CoH3MemoryTool;
using System.Text.Json;
using System.Text.RegularExpressions;

// F only: links the production helper and context. No PlayerInfoReader instance, P/Invoke,
// process handle, license stub, game, GUI, or native memory is used by this fixture.
static class Program
{
    const long Image = 0x10000000, RootSlot = 0x10010000, RootType = Image + 64823200,
        HudType = Image + 65713904, OtherType = Image + 64400000,
        Singleton = 0x20000000, Root = 0x20010000, Hud = 0x20020000;
    static int checks;

    sealed class Memory
    {
        readonly Dictionary<long, byte[]> regions = new();
        internal long Fault;
        internal int FaultCount;
        internal readonly HashSet<(long, int)> Accesses = new();
        internal bool Short;
        internal int Reads, Bytes, LimitReads = int.MaxValue, LimitBytes = int.MaxValue;
        internal void Region(long address, int bytes = 576) => regions[address] = new byte[bytes];
        internal void Q(long address, long value) => Write(address, BitConverter.GetBytes(value));
        internal void U(long address, uint value) => Write(address, BitConverter.GetBytes(value));
        void Write(long address, byte[] bytes)
        {
            foreach (var pair in regions)
                if (address >= pair.Key && address - pair.Key + bytes.Length <= pair.Value.Length)
                {
                    bytes.CopyTo(pair.Value, (int)(address - pair.Key));
                    return;
                }
            throw new Exception("fixture address not mapped: " + address.ToString("x"));
        }
        internal byte[] Read(long address, int count)
        {
            if (++Reads > LimitReads || (Bytes += count) > LimitBytes) throw new Exception("fixture budget exceeded");
            Accesses.Add((address, count));
            if (address == Fault && (FaultCount == 0 || FaultCount == count))
            {
                if (Short) return new byte[Math.Max(0, count - 1)];
                throw new IOException("fixture transient read fault");
            }
            foreach (var pair in regions)
                if (address >= pair.Key && address - pair.Key + count <= pair.Value.Length)
                    return pair.Value.AsSpan((int)(address - pair.Key), count).ToArray();
            throw new IOException("fixture unreadable candidate");
        }
        internal void Node(long address, long type, long parent)
        {
            Region(address);
            Q(address, type);
            Q(address + 136, parent);
        }
        internal void RootAt(long root)
        {
            Q(Singleton + 136, root);
            Node(root, RootType, 0);
        }
        internal void ResetCounters(int readLimit = int.MaxValue, int byteLimit = int.MaxValue)
        {
            Reads = Bytes = 0;
            LimitReads = readLimit;
            LimitBytes = byteLimit;
        }
    }

    static Memory Basic()
    {
        var m = new Memory();
        m.Region(RootSlot, 8);
        m.Region(Singleton, 144);
        m.Q(RootSlot, Singleton);
        m.RootAt(Root);
        m.Node(Hud, HudType, Root);
        m.Q(Root + 496, Hud); // Observed inline496 witness; not just legacy 200/472.
        return m;
    }
    static MatchHudGuard Guard(Memory m, int reads = 256, int bytes = 131072) => new(m.Read, RootSlot, RootType, HudType, Image, reads, bytes, 60000);
    static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        checks++;
    }
    static MatchHudUnavailableException Unknown(Action action, MatchHudOutcome? outcome = null)
    {
        try { action(); }
        catch (MatchHudUnavailableException error)
        {
            Check(outcome == null || error.Outcome == outcome, "outcome mismatch: " + error.Outcome + " / " + outcome);
            return error;
        }
        throw new Exception("expected unavailable outcome");
    }
    static MatchHudContext Resolve(MatchHudGuard guard, string world = "same-world", int turns = 100)
    {
        for (int i = 0; i < turns; i++)
        {
            try { return guard.Resolve(world); }
            catch (MatchHudUnavailableException error) when (error.Outcome == MatchHudOutcome.Resolving) { }
        }
        throw new Exception("fixture did not resolve within bounded turns");
    }

    static void StablePauseAndShortReads()
    {
        Memory m = Basic();
        MatchHudGuard guard = Guard(m);
        MatchHudContext first = Resolve(guard);
        string key = MatchHudContext.ProductKey("same-world", first);
        Check(ReferenceEquals(first, guard.Resolve("same-world")), "normal poll must preserve token");
        m.U(Hud + 192, 0); // Flags never decide match semantics, including pause.
        Check(ReferenceEquals(first, guard.Resolve("same-world")), "pause flags must preserve epoch");
        guard.Verify(first);
        m.Fault = Hud;
        m.Short = true;
        Unknown(() => guard.Verify(first), MatchHudOutcome.ReadFault);
        Check(!first.IsRetired, "short read must not retire an identity");
        m.Fault = 0;
        Check(ReferenceEquals(first, guard.Resolve("same-world")), "short-read recovery must not mint epoch");
        Check(key == MatchHudContext.ProductKey("same-world", first), "key stable across pause and read recovery");
        m.Fault = Root;
        Unknown(() => guard.Verify(first), MatchHudOutcome.ReadFault);
        m.Fault = 0;
        guard.Verify(first);
        Check(first.Epoch > 0, "minted token has epoch");
    }

    static void RetireRediscoverAndStrictClone()
    {
        Memory m = Basic();
        MatchHudGuard main = Guard(m);
        MatchHudContext old = Resolve(main);
        string oldKey = MatchHudContext.ProductKey("same-world", old);
        var context = new MarkerReadContext { Pid = 1, Image = Image, Arena = 0x70000000, WorldOffset = 4, Started = DateTime.UnixEpoch, Match = oldKey, Hud = old };
        MatchHudGuard clone = Guard(m);
        clone.BindExpected(context.Hud);
        clone.Verify(old);
        Check(context.Hud.Epoch == old.Epoch, "clone inherits parent epoch");
        m.Q(Hud, 0x10203a0c6038230);
        Unknown(() => main.Resolve("same-world"), MatchHudOutcome.Retired);
        Check(old.IsRetired, "retirement latches on shared expected token");
        long fresh = Hud + 0x10000;
        m.Node(fresh, HudType, Root);
        m.Q(Root + 496, fresh);
        MatchHudContext second = Resolve(main);
        Check(second.Root == old.Root && second.World == old.World, "models second round same world/root");
        Check(second.Hud == fresh && second.Epoch != old.Epoch, "fresh HUD gets new epoch");
        Check(MatchHudContext.ProductKey("same-world", second) != oldKey, "second round product key must change");
        Unknown(() => clone.Verify(old), MatchHudOutcome.Retired);
        Unknown(() => clone.Resolve("same-world"), MatchHudOutcome.Retired);
        // No rediscovery is called by production clone; explicit Verify never adopts fresh HUD.
        m.Q(Hud, HudType); // Address reuse cannot revive the old shared token.
        Unknown(() => clone.Verify(old), MatchHudOutcome.Retired);
        Unknown(() => MatchHudContext.ProductKey("same-world", old), MatchHudOutcome.Retired);
        MatchHudContext anotherReader = Resolve(Guard(m));
        Check(anotherReader.Epoch != second.Epoch, "new reader gets global epoch even with same live HUD address");
        m.Q(fresh, 0);
        Unknown(() => main.Resolve("same-world"), MatchHudOutcome.Retired);
        m.Q(fresh, HudType);
        MatchHudContext reused = Resolve(main);
        Check(reused.Hud == fresh && reused.Epoch != second.Epoch, "retired same address gets a new epoch");
    }

    static void CurrentRootAndIncompleteProofs()
    {
        Memory m = Basic();
        MatchHudGuard guard = Guard(m);
        MatchHudContext token = Resolve(guard);
        m.Q(Root + 496, 0); // Missing witness is unknown, never a whole-tree negative.
        Unknown(() => guard.Verify(token), MatchHudOutcome.Resolving);
        Check(!token.IsRetired, "missing child edge is not proof of retirement");
        m.Q(Root + 496, Hud);
        guard.Verify(token);
        long root2 = Root + 0x40000;
        m.RootAt(root2);
        Unknown(() => guard.Verify(token), MatchHudOutcome.Retired);
        Check(token.IsRetired, "valid different current root retires old token");
        Unknown(() => Guard(m).Resolve("same-world"), MatchHudOutcome.Resolving);
        m.Q(root2 + 496, Hud);
        m.Q(Hud + 136, root2);
        MatchHudContext replacement = Resolve(Guard(m));
        Check(replacement.Root == root2, "current-root rediscovery uses live singleton");
        m.Q(Singleton + 136, 0);
        Unknown(() => Guard(m).Resolve("same-world"), MatchHudOutcome.Resolving);
    }

    static void VectorAmbiguityAndReadFault()
    {
        Memory m = Basic();
        long container = 0x30000000, data = container + 0x1000, hud2 = Hud + 0x10000;
        m.Region(container); m.Region(data, 16);
        m.Q(Root + 496, 0); m.Q(Root + 504, container);
        m.Q(container + 112, data); m.U(container + 124, 1); m.Q(data, Hud);
        MatchHudContext token = Resolve(Guard(m));
        Check(token.Hud == Hud, "vector504/112/124 gives positive membership");
        m.Node(hud2, HudType, Root); m.Q(data + 8, hud2); m.U(container + 124, 2);
        Unknown(() => Guard(m).Resolve("same-world"), MatchHudOutcome.Ambiguous);
        m.U(container + 124, 513); // Refuse oversized vectors without claiming absence.
        Unknown(() => Guard(m).Resolve("same-world"), MatchHudOutcome.Resolving);
        m.U(container + 124, 1); m.Fault = data; m.Short = true;
        MatchHudGuard partial = Guard(m);
        Unknown(() => partial.Resolve("same-world"), MatchHudOutcome.ReadFault);
        m.Fault = 0;
        Check(Resolve(partial).Hud == Hud, "partial tree retry can later find positive path");
    }

    static void DifferentDepthAmbiguityAndAncestorIdentity()
    {
        Memory m = Basic();
        m.Q(Root + 496, 0);
        long b1 = 0x60000000, b2 = b1 + 0x1000, deeper = b1 + 0x2000, h2 = Hud + 0x10000;
        m.Node(b1, OtherType, Root); m.Node(b2, OtherType, Root); m.Node(deeper, OtherType, b2);
        m.Node(h2, HudType, deeper); m.Q(Hud + 136, b1);
        m.Q(Root + 144, b1); m.Q(Root + 152, b2);
        m.Q(b1 + 496, Hud); m.Q(b2 + 496, deeper); m.Q(deeper + 496, h2);
        // First branch is observed before the deeper second branch. No first-HUD adoption.
        MatchHudGuard guard = Guard(m, 48, 32768);
        bool ambiguous = false;
        for (int i = 0; i < 20 && !ambiguous; i++)
        {
            m.ResetCounters(48, 32768);
            try { guard.Resolve("world"); throw new Exception("adopted before checking other branch"); }
            catch (MatchHudUnavailableException e)
            {
                Check(e.Outcome is MatchHudOutcome.Resolving or MatchHudOutcome.Ambiguous, "different-depth candidate stays unresolved");
                ambiguous = e.Outcome == MatchHudOutcome.Ambiguous;
            }
        }
        Check(ambiguous, "different-depth dual HUD is ambiguous after known frontier");
        m.Q(deeper + 496, 0); m.Q(h2, 0); m.ResetCounters();
        MatchHudContext token = Resolve(Guard(m), "world");
        m.Q(b1, 7); // A parent pointer and child witness alone are not a UI identity.
        MatchHudGuard strict = Guard(m); strict.BindExpected(token);
        Unknown(() => strict.Verify(token), MatchHudOutcome.Resolving);
        m.Q(b1, OtherType); m.Q(Root + 136, b2);
        Unknown(() => strict.Verify(token), MatchHudOutcome.Resolving);
        m.Q(Root + 136, 0); m.Q(Root, OtherType);
        Unknown(() => strict.Verify(token), MatchHudOutcome.Resolving);
    }

    static void ReadFaultDoesNotStarveOtherBranches()
    {
        Memory m = Basic(); m.Q(Root + 496, 0);
        long faulty = 0x61000000, other = faulty + 0x1000;
        m.Node(faulty, OtherType, Root); m.Node(other, OtherType, Root);
        m.Q(Root + 144, faulty); m.Q(Root + 152, other);
        m.Q(other + 496, Hud); m.Q(Hud + 136, other);
        m.Fault = faulty; m.FaultCount = 576;
        MatchHudGuard guard = Guard(m);
        Unknown(() => guard.Resolve("world"), MatchHudOutcome.ReadFault);
        Unknown(() => guard.Resolve("world"), MatchHudOutcome.ReadFault);
        Check(m.Accesses.Contains((other, 576)), "faulty Prepare is queued behind other branches");
        m.Fault = 0;
        MatchHudContext token = null;
        for (int i = 0; i < 10 && token == null; i++)
        {
            try { token = guard.Resolve("world"); }
            catch (MatchHudUnavailableException error) { Check(error.Outcome is MatchHudOutcome.ReadFault or MatchHudOutcome.Resolving, "incomplete pass remains typed unknown"); }
        }
        Check(token != null && token.Hud == Hud, "recovered branch resolves after a fresh positive pass");
        Memory budget = Basic(); MatchHudGuard tiny = Guard(budget, 1, 8);
        Unknown(() => tiny.Resolve("world"), MatchHudOutcome.Resolving);
        Check(budget.Reads == 1 && budget.Bytes == 8, "current-root reads share the discovery budget");
    }

    static void BoundedProgressAndAncestorLimit()
    {
        Memory m = Basic();
        m.Q(Root + 496, 0);
        var branches = new List<long>();
        for (int i = 0; i < 40; i++)
        {
            long branch = 0x40000000 + i * 0x1000;
            branches.Add(branch);
            m.Node(branch, OtherType, Root);
            m.Q(Root + 144 + i * 8, branch);
        }
        m.Q(branches[^1] + 496, Hud); m.Q(Hud + 136, branches[^1]);
        MatchHudGuard guard = Guard(m, 24, 8192);
        MatchHudContext found = null;
        int rounds = 0;
        for (; rounds < 100 && found == null; rounds++)
        {
            m.ResetCounters(24, 8192);
            try { found = guard.Resolve("same-world"); }
            catch (MatchHudUnavailableException e) { Check(e.Outcome == MatchHudOutcome.Resolving, "budget is unknown, not retired"); }
            Check(m.Reads <= 24 && m.Bytes <= 8192, "explicit budgets bound each turn");
        }
        Check(found != null && rounds > 1, "BFS cursor survives budget turns");
        Memory empty = Basic(); empty.Q(Root + 496, 0);
        Unknown(() => Guard(empty).Resolve("world"), MatchHudOutcome.Resolving);
        Memory longChain = Basic();
        long parent = Hud;
        for (int i = 0; i < 65; i++)
        {
            long next = 0x50000000 + i * 0x1000;
            longChain.Node(next, OtherType, 0);
            longChain.Q(parent + 136, next); longChain.Q(next + 496, parent);
            parent = next;
        }
        longChain.Q(parent + 136, Root); longChain.Q(Root + 496, parent);
        MatchHudContext expected = new("world", Root, Hud, 999999);
        MatchHudGuard strict = Guard(longChain, 1000, 1048576); strict.BindExpected(expected);
        Unknown(() => strict.Verify(expected), MatchHudOutcome.Resolving);
    }

    static string Method(string source, string signature, string next)
    {
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Check(start >= 0, "source seam exists: " + signature);
        int end = source.IndexOf(next, start + signature.Length, StringComparison.Ordinal);
        Check(end > start, "source seam end exists: " + next);
        return source[start..end];
    }
    static void SourceSeams(string repo)
    {
        string sky = Path.Combine(repo, "CoH3/dev/SkyEye/client/CoH3MemoryTool");
        string simple = Path.Combine(repo, "CoH3/dev/COH3SimpleTool/CoH3MemoryTool");
        foreach (string file in new[] { "PlayerInfoReader.cs", "MarkerReadContext.cs", "MatchHudContext.cs" })
            Check(File.ReadAllBytes(Path.Combine(sky, file)).SequenceEqual(File.ReadAllBytes(Path.Combine(simple, file))), "production mirror mismatch: " + file);
        foreach (string root in new[] { sky, simple })
        {
            string s = File.ReadAllText(Path.Combine(root, "PlayerInfoReader.cs"));
            string snapshot = Method(s, "public List<PlayerInfoRow> Snapshot()", "public void Dispose()");
            Check(snapshot.IndexOf("SnapshotHud55(worldKey)") < snapshot.IndexOf("if (MatchKey != text)"), "full product key before blueprint cache comparison");
            Check(snapshot.IndexOf("CheckMatchHud55(snapshotHud)") < snapshot.IndexOf("MatchKey = text"), "end validation before key commit");
            Check(snapshot.Contains("ProductMatchKey55(worldKey, snapshotHud)"), "snapshot uses shared product key");
            string current = Method(s, "public bool IsCurrentMatch55()", "private void TrackSquadMember");
            Check(current.Contains("ProductMatchKey55(worldKey, matchHud55)") && !current.Contains("SnapshotHud55") && !current.Contains(".Resolve(world"), "strict current check never rediscovers");
            Check(current.Contains("catch (MatchHudUnavailableException) { throw; }"), "same-match recovery propagates qualification fault");
            string context = Method(s, "public MarkerReadContext MarkerContext()", "#if MAINTENANCE_DIAGNOSTICS");
            Check(context.Contains("CheckMarkerWorld(MatchKey)") && context.Contains("markerReadContext.Hud = matchHud55"), "context strict validation and expected token");
            string clone = Method(s, "private PlayerInfoReader(MarkerReadContext context, bool maintenanceDiagnostics)", "private bool UsesCandidateLayout()");
            Check(clone.Contains("BindExpected(context.Hud)") && clone.Contains("CurrentWorldMatchKey55()") && !clone.Contains(".Resolve(world"), "clone inherits token and validates real world key");
            Check(Regex.Matches(s, @"\.Resolve\(world\)").Count == 1, "one main-reader mint/discovery seam");
            string support = Method(s, "public List<SupportCommand49> ReadSupport49", "private static bool TacticalVisible51");
            Check(Regex.Matches(support, @"if \(UsesCandidateLayout\(\)\) CheckMarkerWorld\(match\)").Count == 2, "new-build support start/end qualification; legacy unchanged");
            Check(s.Contains("miniEpoch == epoch && miniMatch == scanMatch && MatchKey == scanMatch && object.ReferenceEquals(matchHud55, scanHud)"), "async mini checks epoch/key/token before write");
            Check(s.Contains("flag = hud == null || num4 == hud.Hud;") && s.Contains("flag2 = hud == null || num4 == hud.Hud;"), "both map chains use expected HUD address");
            Check(s.Contains("num4 == hud.Root"), "map chain requires expected root");
            string camera = Method(s, "public MineCamera ReadMineCamera()", "private MineCamera ReadMineCameraCore55()");
            Check(camera.Contains("CheckMatchHud55()"), "public camera outlet strict gate");
            Check(s.Contains("MatchHudContext.ProductKey(world, hud, UsesCandidateLayout())"), "legacy product-key branch explicit");
            string order = @"GameVersion45\.RequireVerified\(BuildStatus, maintenanceDiagnostics\);\s*ValidateLayout\(\);\s*if \(!LicenseGate60\.Matches\([^{}]*\{[^{}]*\}\s*handle = Native\.OpenProcess";
            Check(Regex.Matches(s, order, RegexOptions.Singleline).Count == 2, "both constructors preserve build/layout/license/open ordering");
        }
        Check(MatchHudContext.ProductKey("legacy-world", null, false) == "legacy-world", "legacy key unchanged without HUD");
        Unknown(() => MatchHudContext.ProductKey("world", null), MatchHudOutcome.Resolving);
        MatchHudGuard missing = Guard(Basic());
        Unknown(() => missing.BindExpected(null), MatchHudOutcome.Resolving);
        Check(typeof(MatchHudUnavailableException).BaseType == typeof(InvalidOperationException) && typeof(MatchHudUnavailableException).IsSealed, "UI typed exception contract");
    }

    public static int Main(string[] args)
    {
        StablePauseAndShortReads();
        RetireRediscoverAndStrictClone();
        CurrentRootAndIncompleteProofs();
        VectorAmbiguityAndReadFault();
        DifferentDepthAmbiguityAndAncestorIdentity();
        ReadFaultDoesNotStarveOtherBranches();
        BoundedProgressAndAncestorLimit();
        SourceSeams(Path.GetFullPath(args[0]));
        Console.WriteLine(JsonSerializer.Serialize(new { status = "PASS_fixture_only", checks, real_game = "not_run", native_process_open = "not_linked", license = "not_linked", production_helper = "linked", production_reader = "source_seams_only" }));
        return 0;
    }
}
