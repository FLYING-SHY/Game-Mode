using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using CoH3MemoryTool;

internal static partial class Program
{
    private const string P47Aircraft = "ebps\\races\\american\\aircraft\\p47_thunderbolt_air_support_center_us";
    private const string P47Ability = "abilities\\races\\american\\aircraft\\strafing_run_p47_us";
    private static object capturedAirEvidence;
    private static bool Near(float a, float b, float tolerance = 0.00001f) => Math.Abs(a - b) <= tolerance;
    private static long Hex(JsonElement value) => long.Parse(value.GetString().Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    private static void TestAirMoverShape()
    {
        Check(AirLogic.MoverReadSize() == 284, "mover default extent remains legacy 284");
        Check(Equals(typeof(AirLogic).GetMethod(nameof(AirLogic.RecoverStrafe)).GetParameters().Last().DefaultValue, AirMoverShape.Legacy284), "RecoverStrafe optional shape defaults to legacy in both configurations");
        var legacy = Actor(Map, 64559016, 1);
        F(legacy, 96, 0); F(legacy, 100, 1);
        AirLogic.MoverDirection(legacy, AirMoverShape.Legacy284, out float x, out float y, out float z);
        Check(x == 1 && y == 0 && z == 0, "legacy direction ignores conflicting compact fields even with mapped vtable");
        bool invalidThrows = false;
        try { AirLogic.MoverReadSize((AirMoverShape)int.MaxValue); }
        catch (ArgumentOutOfRangeException) { invalidThrows = true; }
        Check(invalidThrows, "unknown mover shape fails instead of choosing by buffer length or RVA");
        // Every build ships both shapes: the running client picks the one that belongs to the
        // fingerprint it detected, so the candidate shape must exist in the client build too.
        Check(Enum.GetNames<AirMoverShape>().SequenceEqual(new[] { "Legacy284", "Candidate52426_184" }), "every build exposes two mover shapes");
        Check(AirLogic.MoverReadSize(AirMoverShape.Candidate52426_184) == 184, "candidate allocation extent is exactly 184");
        F(legacy, 96, 0.6f); F(legacy, 100, 0.8f);
        var body = Body(); F(body, 48, 30);
        var compact = legacy.Take(184).ToArray();
        var recovered = AirLogic.RecoverStrafe(P47Aircraft, body, compact, Image, Arena, Entity, Id, Index, Map, AirMoverShape.Candidate52426_184);
        Check(compact.Length == 184 && recovered != null && Near(recovered.DX, 0.6f) && Near(recovered.DZ, 0.8f), "exact 184-byte compact buffer recovers with explicit shape");
        var defaultZone = AirLogic.RecoverStrafe(P47Aircraft, body, legacy, Image, Arena, Entity, Id, Index, Map);
        Check(defaultZone != null && defaultZone.DX == 1 && defaultZone.DZ == 0, "default parser still uses legacy offsets");
        Check(AirLogic.RecoverStrafe(P47Aircraft, body, compact, Image, Arena, Entity, Id, Index, Map) == null, "mapper alone does not enable compact shape");
        Check(typeof(GameLayout52426).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Length > 0, "every build ships the candidate layout roles");
    }

    private static void TestCapturedAir(string directory)
    {
        var inputEvidence = new List<object>();
        JsonElement Load(string name)
        {
            string path = Path.GetFullPath(Path.Combine(directory, name));
            byte[] file = File.ReadAllBytes(path);
            using var doc = JsonDocument.Parse(file);
            var root = doc.RootElement.Clone(); var snapshot = root.GetProperty("snapshot");
            Check(snapshot.GetProperty("errors").GetArrayLength() == 0 && snapshot.GetProperty("stableRootsAndDescriptors").GetBoolean() && snapshot.GetProperty("stableEntries").GetBoolean() && snapshot.GetProperty("scanComplete").EnumerateObject().All(p => p.Value.GetBoolean()), name + " retained snapshot is complete and stable");
            inputEvidence.Add(new { path, sha256 = Convert.ToHexString(SHA256.HashData(file)), timestampUtc = root.GetProperty("timestampUtc").GetString(), moduleBase = root.GetProperty("moduleBase").GetString(), arena = snapshot.GetProperty("roots").GetProperty("arena").GetString() });
            return root;
        }
        var raw04 = Load("live-raw-04-p47-middle.json");
        var raw05 = Load("live-raw-05-p47-late.json");
        var raw07 = Load("live-raw-07-p47-early.json");
        var raw08 = Load("live-raw-08-p47-middle.json");
        var raw09 = Load("live-raw-09-p47-late.json");
        JsonElement Plane(JsonElement root, string entity) => root.GetProperty("snapshot").GetProperty("airEntities").EnumerateArray().Single(p => p.GetProperty("entity").GetString() == entity);
        var plane04 = Plane(raw04, "0x1fd7445fe00"); var plane05 = Plane(raw05, "0x1fd7445fe00");
        var plane08 = Plane(raw08, "0x1fd74467a00");
        Check(plane04.GetProperty("generation").GetUInt32() == plane05.GetProperty("generation").GetUInt32() && plane04.GetProperty("actor").GetProperty("address").GetString() == plane05.GetProperty("actor").GetProperty("address").GetString(), "raw04 and raw05 retain the same aircraft generation and actor");
        Check(!raw09.GetProperty("snapshot").GetProperty("airEntities").EnumerateArray().Any(p => p.GetProperty("blueprint").GetString() == P47Aircraft), "raw09 is an absent-aircraft negative sample; actor is optional");
        var cast07 = raw07.GetProperty("snapshot").GetProperty("timedAbilities").EnumerateArray().Single(a => a.GetProperty("blueprint").GetString() == P47Ability);
        var castBytes = Convert.FromHexString(cast07.GetProperty("raw148Hex").GetString());
        var targetBytes = Convert.FromHexString(cast07.GetProperty("target").GetProperty("raw20Hex").GetString());
        var player07 = raw07.GetProperty("snapshot").GetProperty("players").EnumerateArray().Single(p => p.GetProperty("address").GetString() == cast07.GetProperty("player").GetString());
        Check(AirLogic.Cast(castBytes, targetBytes, Hex(raw07.GetProperty("moduleBase")), Hex(player07.GetProperty("identity")), out _, out _, out _, out float castDX, out float castDZ, resolveRva: Map), "raw07 unmodified native timed cast decodes with explicit mapper");
        var body04 = Convert.FromHexString(plane04.GetProperty("raw392Hex").GetString());
        var body05 = Convert.FromHexString(plane05.GetProperty("raw392Hex").GetString());
        var actor04 = Convert.FromHexString(plane04.GetProperty("actor").GetProperty("rawHex").GetString());
        var actor08 = Convert.FromHexString(plane08.GetProperty("actor").GetProperty("rawHex").GetString());
        float motionX = BitConverter.ToSingle(body05, 44) - BitConverter.ToSingle(body04, 44);
        float motionZ = BitConverter.ToSingle(body05, 52) - BitConverter.ToSingle(body04, 52);
        double motionLength = Math.Sqrt((double)motionX * motionX + (double)motionZ * motionZ);
        double oldLength = Math.Sqrt(Math.Pow(BitConverter.ToSingle(actor04, 96), 2) + Math.Pow(BitConverter.ToSingle(actor04, 100), 2));
        double newLength = Math.Sqrt(Math.Pow(BitConverter.ToSingle(actor08, 96), 2) + Math.Pow(BitConverter.ToSingle(actor08, 100), 2));
        Check(Near((float)(BitConverter.ToSingle(actor04, 96) / oldLength), (float)(motionX / motionLength)) && Near((float)(BitConverter.ToSingle(actor04, 100) / oldLength), (float)(motionZ / motionLength)), "raw04 compact pair agrees with independent same-plane raw04-to-05 displacement");
        Check(Near((float)(BitConverter.ToSingle(actor08, 96) / newLength), castDX) && Near((float)(BitConverter.ToSingle(actor08, 100) / newLength), castDZ), "raw08 compact pair agrees with separate native raw07 cast direction");
        Check(Math.Abs(BitConverter.ToSingle(actor04, 96) / oldLength * castDX + BitConverter.ToSingle(actor04, 100) / oldLength * castDZ) < 0.02, "two captured passes exercise approximately orthogonal headings");

        foreach (var sample in new[] { (Name: "raw04", Root: raw04, Plane: plane04, Positive: true), (Name: "raw05", Root: raw05, Plane: plane05, Positive: false), (Name: "raw08", Root: raw08, Plane: plane08, Positive: true) })
        {
            byte[] body = Convert.FromHexString(sample.Plane.GetProperty("raw392Hex").GetString());
            byte[] capturedActor = Convert.FromHexString(sample.Plane.GetProperty("actor").GetProperty("rawHex").GetString());
            byte[] actor = capturedActor.Take(184).ToArray();
            long image = Hex(sample.Root.GetProperty("moduleBase")), arena = Hex(sample.Root.GetProperty("snapshot").GetProperty("roots").GetProperty("arena")), entity = Hex(sample.Plane.GetProperty("entity"));
            uint generation = sample.Plane.GetProperty("generation").GetUInt32(), index = sample.Plane.GetProperty("registryIndex").GetUInt32();
            Check(body.Length == 392 && capturedActor.Length == 284 && actor.Length == 184 && capturedActor.AsSpan(0, 184).SequenceEqual(actor), sample.Name + " replay uses unmodified 392-byte body and exact 184-byte actor prefix");
            Check(BitConverter.ToUInt32(actor, 36) + arena == entity && BitConverter.ToInt64(actor, 0) == image + Map(64559016), sample.Name + " independent saved arena/module roots agree with actor backref and vtable");
            Check(AirLogic.RecoverStrafe(P47Aircraft, body, capturedActor, image, arena, entity, generation, index, Map) == null, sample.Name + " full captured tail fails legacy direction despite candidate RVA mapper");
            AirZone Run(byte[] a, byte[] b = null, uint? id = null, uint? idx = null, Func<long, long> mapper = null) => AirLogic.RecoverStrafe(P47Aircraft, b ?? body, a, image, arena, entity, id ?? generation, idx ?? index, mapper ?? Map, AirMoverShape.Candidate52426_184);
            var zone = Run(actor);
            if (!sample.Positive)
            {
                float targetY = BitConverter.ToSingle(actor, 24);
                Check(zone == null && BitConverter.ToSingle(body, 48) - targetY < 8, "raw05 compact replay retains the original above-target rejection");
                var counterfactual = (byte[])body.Clone(); F(counterfactual, 48, targetY + 16);
                Check(Run(actor, counterfactual) != null, "raw05 counterfactual clone changing only body Y isolates height rejection; not a captured positive");
                continue;
            }
            double length = Math.Sqrt(Math.Pow(BitConverter.ToSingle(actor, 96), 2) + Math.Pow(BitConverter.ToSingle(actor, 100), 2));
            Check(zone != null && zone.X == BitConverter.ToSingle(actor, 20) && zone.Y == BitConverter.ToSingle(actor, 24) && zone.Z == BitConverter.ToSingle(actor, 28) && Near(zone.DX, (float)(BitConverter.ToSingle(actor, 96) / length)) && Near(zone.DZ, (float)(BitConverter.ToSingle(actor, 100) / length)) && zone.Length == 45 && zone.HalfWidth == 5 && zone.Key == "flight:" + entity + ":" + generation, sample.Name + " exact 184-byte real buffer recovers native P47 geometry and identity");
            Check(Run(capturedActor)?.DX == zone.DX && Run(capturedActor)?.DZ == zone.DZ, sample.Name + " explicit compact shape ignores captured bytes beyond 184");
            int mapperCalls = 0;
            long Trace(long key) { mapperCalls++; return Map(key); }
            Check(Run(actor.Take(183).ToArray(), mapper: Trace) == null && mapperCalls == 0, sample.Name + " 183-byte actor rejected before mapper");
            Check(Run(null, mapper: Trace) == null && mapperCalls == 0, sample.Name + " absent actor rejected before mapper");
            Check(Run(actor, body.Take(391).ToArray(), mapper: Trace) == null && mapperCalls == 0, sample.Name + " 391-byte body rejected before mapper");
            Check(Run(actor, id: generation + 1) == null && Run(actor, idx: index + 1) == null, sample.Name + " wrong generation and registry index independently rejected");
            var wrong = (byte[])actor.Clone(); Q(wrong, 0, image + 0x7FFF0000);
            Check(Run(wrong) == null, sample.Name + " wrong mover type rejected with compact shape");
            wrong = (byte[])actor.Clone(); U(wrong, 36, BitConverter.ToUInt32(wrong, 36) + 4);
            Check(Run(wrong) == null, sample.Name + " wrong arena-relative backref rejected");
            wrong = (byte[])actor.Clone(); U(wrong, 64, 0);
            Check(Run(wrong) == null, sample.Name + " wrong mover state rejected");
            wrong = (byte[])actor.Clone(); wrong[16] = 1;
            Check(Run(wrong) == null, sample.Name + " wrong target mode rejected");
            var destroyed = (byte[])body.Clone(); U(destroyed, 160, BitConverter.ToUInt32(destroyed, 160) | 0x4000);
            Check(Run(actor, destroyed) == null, sample.Name + " destroyed entity rejected");
            foreach (int offset in new[] { 96, 100 })
                foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                {
                    wrong = (byte[])actor.Clone(); F(wrong, offset, value);
                    Check(Run(wrong) == null, sample.Name + " non-finite compact heading rejected at " + offset + " " + value);
                }
            wrong = (byte[])actor.Clone(); F(wrong, 96, 0); F(wrong, 100, 0);
            Check(Run(wrong) == null, sample.Name + " zero compact heading rejected");
            wrong = (byte[])actor.Clone(); F(wrong, 20, float.NaN);
            Check(Run(wrong) == null, sample.Name + " non-finite target rejected");
            var failure = new MapperFailure(); Exception caught = null;
            try { Run(actor, mapper: _ => throw failure); } catch (Exception ex) { caught = ex; }
            Check(ReferenceEquals(caught, failure), sample.Name + " compact parser preserves direct mapper exception identity");
            AirLogic.MoverDirection(actor, AirMoverShape.Candidate52426_184, out float dx, out float dy, out float dz);
            var loiter = new LoiterState49();
            Loiter49.Observe(loiter, true, zone.X, zone.Y, zone.Z, BitConverter.ToSingle(body, 44), BitConverter.ToSingle(body, 52), dx, dz, 45, 5, "offline-p47-direction", "fixture only");
            Check(dy == 0 && loiter.Zone != null && Near(loiter.Zone.DX, zone.DX) && Near(loiter.Zone.DZ, zone.DZ), sample.Name + " shared compact direction feeds pure Loiter49 with synthetic acquisition inputs; not a native loiter sample");
        }
        capturedAirEvidence = new { inputs = inputEvidence, aircraft = new[] { "0x1fd7445fe00", "0x1fd74467a00" }, rawActorBytes = 184, bodyBytes = 392, capturedTailExcluded = true, independentSavedRoots = true, approximatelyOrthogonalNativeCast = true, nativeLoiterSample = false, limitation = "F only. Original captured bytes replayed offline; candidate positive/rejection cases run in every configuration because the layout table ships in every build. Loiter call uses synthetic acquisition/range inputs. No reader, timing, enemy UI, camera recovery or live game validation." };
    }
}
