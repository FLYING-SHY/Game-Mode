namespace CoH3Minimap;

using System.Text;

internal static class NativeFrameRenderingTests
{
    internal static void Run()
    {
        using var assets = new NativeAssetCatalog();
        if (assets.Resolve("data:scenarios\\multiplayer\\twin_beach_2p_mkii\\twin_beach_2p_mkii_mm_handmade.rrtex") is not { Width: 2048, Height: 2048 })
            throw new Exception("The verified current-map asset was not embedded under its native path.");
        if (assets.Resolve("icons/common/resources/symbols/mm_victory_point") is null ||
            assets.Resolve("data:scenarios/../../elsewhere") is not null)
            throw new Exception("Native icon lookup or resource-path bounds failed.");
        var geometry = new NativeMapGeometry(0, 384, 384, new PointF[]
        {
            new(-96, -96), new(96, -96), new(-96, 96), new(96, 96)
        }, 1, 1, 1);
        var map = new NativeMapSample(0, 0, 0, 0, 0, 0, Array.Empty<NativeMarkSample>(), geometry);
        var frame = new NativeHudFrame(DateTimeOffset.Now, new(0, 0, 0, 0, 0, 0), map,
            new Dictionary<ulong, NativeIconReference?>());
        using var bitmap = new Bitmap(268, 268);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Black);
        var result = NativeFrameRendering.Draw(graphics, new RectangleF(0, 0, 268, 268), frame, assets);
        if (!result.CameraViewportDrawn || bitmap.GetPixel(134, 67).R < 240 || bitmap.GetPixel(134, 134).R > 10)
            throw new Exception("PerspectiveBox order must be 0,1,3,2 and must not draw crossing diagonals.");

        var fog = new NativeFogPlanes(8, 4, 2, 0, 1, false, new byte[4], new byte[4], new byte[4], UiDisabled: false);
        var marker = new NativeMarkSample(0, "SquadMarkModel", 0, 0, 0, 0, 64, 1, 0, 0, 1,
            new NativeSelectionColor(0, 1, 0, 1, 1));
        var fogFrame = frame with { Map = map with { Marks = new[] { marker } }, Fog = fog };
        NativeFrameRenderResult DrawFogFrame(NativeHudFrame value, bool enabled)
        {
            graphics.Clear(Color.White);
            return NativeFrameRendering.Draw(graphics, new RectangleF(0, 0, 268, 268), value, assets, enabled);
        }
        var off = DrawFogFrame(fogFrame, false);
        if (off.FogDrawn || bitmap.GetPixel(134, 134).R != 255)
            throw new Exception("CPU Fog must remain opt-in.");
        var on = DrawFogFrame(fogFrame, true);
        if (!on.FogDrawn || bitmap.GetPixel(134, 134).R is < 20 or > 30 ||
            bitmap.GetPixel(134, 67).R < 240 || bitmap.GetPixel(134, 89).G < 240 ||
            !on.CameraViewportDrawn || on.Marks.Drawn != 1 || on.Marks != off.Marks)
            throw new Exception("CPU Fog must darken the background once and keep the camera and marks above it.");
        foreach (var skipped in new NativeFogPlanes?[] { null, fog with { UiDisabled = true }, fog with { UiDisabled = null }, fog with { Width = 7 } })
        {
            var resultWithoutFog = DrawFogFrame(frame with { Fog = skipped }, true);
            if (resultWithoutFog.FogDrawn || bitmap.GetPixel(134, 134).R != 255)
                throw new Exception("Disabled, unknown, unavailable or invalid Fog must not render.");
        }

        var cache = new byte[0x1C];
        byte[]? Read(long _, int length) => length == cache.Length ? cache.ToArray() : null;
        foreach (float degrees in new[] { 0f, -90f })
        {
            BitConverter.GetBytes(1UL).CopyTo(cache, 0);
            BitConverter.GetBytes(degrees).CopyTo(cache, 0x18);
            if (NativeMarkAngle.Read(Read, 0x10000)?.Degrees != degrees)
                throw new Exception("Zero and negative cached mark angles are valid.");
        }
        foreach (ulong epoch in new[] { ulong.MaxValue, ulong.MaxValue - 1 })
        {
            BitConverter.GetBytes(epoch).CopyTo(cache, 0);
            if (NativeMarkAngle.Read(Read, 0x10000) is not null)
                throw new Exception("Uninitialized and invalidated angle caches must be skipped.");
        }
        Console.WriteLine("PASS: embedded native map/icons, bounded resource lookup, noncrossing PerspectiveBox, opt-in Fog gating/opacity/marker and viewport layering, and angle-cache sentinels. Synthetic pixels only.");

        // 只读 Squad 枚举链：合成内存逐字段复现 Python 实测链路。
        const long moduleBase = 0x10000, segBase = 0x200000, world = 0x200080;
        var memory = new Dictionary<long, byte[]>();
        void Put(long address, byte[] bytes) => memory[address] = bytes;
        Put(moduleBase + 0x6185EB0, BitConverter.GetBytes(segBase));
        Put(moduleBase + 0x5E0BECC, BitConverter.GetBytes(0x50u));
        Put(segBase + 0x50 + 0x100, BitConverter.GetBytes((uint)(world - segBase)));
        var worldHeader = new byte[0x18];
        BitConverter.GetBytes(0x1000u).CopyTo(worldHeader, 0x08);
        BitConverter.GetBytes(2u).CopyTo(worldHeader, 0x0C);
        BitConverter.GetBytes(0x2000u).CopyTo(worldHeader, 0x14);
        Put(world, worldHeader);
        Put(segBase + 0x1000, BitConverter.GetBytes(0x3000u).Concat(new byte[4]).ToArray()
            .Concat(BitConverter.GetBytes(0x4000u)).Concat(new byte[4]).ToArray());
        Put(segBase + 0x2000, BitConverter.GetBytes(1u).Concat(BitConverter.GetBytes(0u)).ToArray());
        var squad = new byte[0x60];
        BitConverter.GetBytes(10.5f).CopyTo(squad, 0x2C);
        BitConverter.GetBytes(-3.25f).CopyTo(squad, 0x34);
        BitConverter.GetBytes(0x5000u).CopyTo(squad, 0x58);
        BitConverter.GetBytes(segBase + 0x6000).CopyTo(squad, 0x38); // SBP = U64[squad+0x38]（绝对地址）
        Put(segBase + 0x3000, squad);
        Put(segBase + 0x5000 + 8, BitConverter.GetBytes(1000u).Concat(BitConverter.GetBytes(0u)).ToArray());
        // session：+0x08 本地玩家 ID、+0x0C 本地 slot、+0x11C 队伍表偏移；队伍表 8 项（slot5 与 slot0 异队）。
        Put(segBase + 0x50 + 0x08, BitConverter.GetBytes(1005u));
        Put(segBase + 0x50 + 0x0C, BitConverter.GetBytes(5u));
        Put(segBase + 0x50 + 0x11C, BitConverter.GetBytes(0xF000u));
        Put(segBase + 0xF000, Enumerable.Range(0, 8).Select(i => BitConverter.GetBytes(i is 0 or 5 ? 1u : 0u)).SelectMany(b => b).ToArray());

        // SBP 类型链：标签 0x2E0000、SSO 名、tlsIdx=44、typeIds 向量、BPDB 名→ID 表。
        const uint infantryId = 7, armorId = 9;
        const long sbp = segBase + 0x6000, typesArray = segBase + 0x7000, typesT = segBase + 0x8000,
            typeIdsData = segBase + 0x8100, bpdbMap = segBase + 0x9000, bpdbEntries = segBase + 0xA000;
        var sbpBody = new byte[0x130];
        Encoding.ASCII.GetBytes("pioneer_ger").CopyTo(sbpBody, 0x08);
        BitConverter.GetBytes(11L).CopyTo(sbpBody, 0x18); // size
        BitConverter.GetBytes(15L).CopyTo(sbpBody, 0x20); // cap ≤ 0xF → 内联
        BitConverter.GetBytes(0x2E0000u).CopyTo(sbpBody, 0x48); // 标签
        BitConverter.GetBytes(typesArray).CopyTo(sbpBody, 0x128);
        Put(sbp, sbpBody);
        const uint tlsIdx = 44;
        Put(moduleBase + 0x5DF38D0, BitConverter.GetBytes(tlsIdx));
        var tlsSlots = new byte[0x168];
        BitConverter.GetBytes(typesT).CopyTo(tlsSlots, (int)(8 * tlsIdx));
        Put(typesArray, tlsSlots);
        var tHeader = new byte[0x28];
        BitConverter.GetBytes(typeIdsData).CopyTo(tHeader, 0x18);
        BitConverter.GetBytes(typeIdsData + 8).CopyTo(tHeader, 0x20);
        Put(typesT, tHeader);
        Put(typeIdsData, BitConverter.GetBytes(infantryId).Concat(BitConverter.GetBytes(3u)).ToArray());
        Put(moduleBase + 0x6BB0020 + 0x138, BitConverter.GetBytes(bpdbMap));
        var mapHeader = new byte[0x28];
        BitConverter.GetBytes(bpdbEntries).CopyTo(mapHeader, 0x18);
        BitConverter.GetBytes(bpdbEntries + 2 * 0x30).CopyTo(mapHeader, 0x20);
        Put(bpdbMap, mapHeader);
        byte[] TypeEntry(string label, uint id)
        {
            var entry = new byte[0x30];
            Encoding.ASCII.GetBytes(label).CopyTo(entry, 0x08);
            BitConverter.GetBytes((long)label.Length).CopyTo(entry, 0x18);
            BitConverter.GetBytes(15L).CopyTo(entry, 0x20);
            BitConverter.GetBytes(id).CopyTo(entry, 0x28);
            return entry;
        }
        Put(bpdbEntries, TypeEntry("infantry", infantryId).Concat(TypeEntry("armor", armorId)).ToArray());
        byte[]? SquadRead(long address, int length)
        {
            foreach (var (baseAddress, bytes) in memory)
                if (address >= baseAddress && address + length <= baseAddress + bytes.Length)
                    return bytes[(int)(address - baseAddress)..(int)(address - baseAddress + length)];
            return null;
        }
        var reader = new NativeSquadReader(SquadRead, moduleBase);
        var listed = reader.Read();
        if (listed.Count != 1 || listed[0] is not { X: 10.5f, Z: -3.25f, PlayerId: 1000, Name: "pioneer_ger", Category: "infantry", Team: 1 })
            throw new Exception("Squad chain must enumerate the single valid entry with exact fields, SBP type, and slot-0 team.");
        if (reader.ReadLocalPlayerId() != 1005 || reader.ReadLocalTeam() != 1)
            throw new Exception("Local player ID (session+0x08) and local team (team table slot5) must both resolve.");

        // 堆串（cap>0xF）与坏 SBP（标签不符→Unknown）。
        const long heapSbp = segBase + 0xB000, heapString = segBase + 0xC000;
        var heapBody = new byte[0x130];
        BitConverter.GetBytes(heapString).CopyTo(heapBody, 0x08);
        BitConverter.GetBytes(19L).CopyTo(heapBody, 0x18);
        BitConverter.GetBytes(31L).CopyTo(heapBody, 0x20);
        BitConverter.GetBytes(0x2E0000u).CopyTo(heapBody, 0x48);
        BitConverter.GetBytes(typesArray).CopyTo(heapBody, 0x128);
        Put(heapSbp, heapBody);
        Put(heapString, Encoding.ASCII.GetBytes("panzergrenadier_ger"));
        var heapSquad = (byte[])squad.Clone();
        BitConverter.GetBytes(heapSbp).CopyTo(heapSquad, 0x38);
        Put(segBase + 0x4000, heapSquad);
        var badSbpBuffer = new byte[0x130]; // 无 0x2E0000 标签 → Unknown
        const long badSbp = segBase + 0xD000;
        Put(badSbp, badSbpBuffer);
        var badSquad = (byte[])squad.Clone();
        BitConverter.GetBytes(badSbp).CopyTo(badSquad, 0x38);
        Put(segBase + 0xE000, badSquad);
        // 扩为 3 个条目以同时覆盖堆串与坏 SBP。
        BitConverter.GetBytes(3u).CopyTo(worldHeader, 0x0C);
        Put(world, worldHeader);
        Put(segBase + 0x1000, BitConverter.GetBytes(0x3000u).Concat(new byte[4])
            .Concat(BitConverter.GetBytes(0x4000u)).Concat(new byte[4])
            .Concat(BitConverter.GetBytes(0xE000u)).Concat(new byte[4]).ToArray());
        Put(segBase + 0x2000, BitConverter.GetBytes(1u).Concat(BitConverter.GetBytes(1u)).Concat(BitConverter.GetBytes(1u)).ToArray());
        var listed3 = new NativeSquadReader(SquadRead, moduleBase).Read();
        if (listed3.Count != 3 || listed3[1] is not { Name: "panzergrenadier_ger", Category: "infantry" } ||
            listed3[2] is not { Name: "", Category: "Unknown" })
            throw new Exception("Heap-string SBP and tag-mismatch SBP must resolve name/type/Unknown correctly.");
        if (new NativeSquadReader((_, _) => null, moduleBase).Read().Count != 0)
            throw new Exception("Unreadable memory must yield an empty squad list.");
        var iconCandidates = NativeFrameRendering.SquadIconPaths("stug_iii_sdkfz_142_ger", "armor").ToArray();
        if (iconCandidates.Length < 4 || !iconCandidates[0].EndsWith("stug_iii_sdkfz_142_ger") ||
            !iconCandidates.Any(c => c.EndsWith("unit_german_stug_iii_sdkfz_142_symbol")) ||
            !iconCandidates.Any(c => c.EndsWith("stug_iii_sdkfz_142")) ||
            !iconCandidates[^1].EndsWith("tank_icon"))
            throw new Exception("Icon candidates must try raw name, faction-prefixed symbol variants, stripped name, then category fallback.");
        var infantryCandidates = NativeFrameRendering.SquadIconPaths("pioneer_ger", "infantry").ToArray();
        if (!infantryCandidates[0].EndsWith("pioneer_ger") ||
            !infantryCandidates.Any(c => c.EndsWith("unit_german_pioneer_symbol")) ||
            !infantryCandidates[^1].EndsWith("grenadier_ger"))
            throw new Exception("Infantry candidates must include raw, faction-prefixed, and faction-generic infantry icon.");
        var plainInfantry = NativeFrameRendering.SquadIconPaths("base_defense", "infantry").ToArray();
        if (!plainInfantry[^1].EndsWith("attachment_infantry_uk_symbol"))
            throw new Exception("Factionless infantry must fall back to a generic infantry icon.");

        // 渲染：squad 点在迷雾之上、默认关闭、队伍着色（本队蓝含队友/异队红）+ 无队伍时按本地 ID 兜底。
        NativeFrameRenderResult DrawUnits(NativeHudFrame value, bool enabled)
        {
            graphics.Clear(Color.White);
            return NativeFrameRendering.Draw(graphics, new RectangleF(0, 0, 268, 268), value, assets, true, enabled);
        }
        var unitsFrame = fogFrame with { Squads = new[] { new NativeSquadPosition(0, 0, 1000) }, LocalPlayerId = 1000, LocalTeam = 1 };
        var unitsOff = DrawUnits(unitsFrame, false);
        if (unitsOff.SquadsDrawn != 0 || bitmap.GetPixel(134, 134).R is < 20 or > 30)
            throw new Exception("Show-all-units must remain opt-in and fog must still darken the center.");
        var unitsOn = DrawUnits(unitsFrame, true);
        var center = bitmap.GetPixel(134, 134);
        if (unitsOn.SquadsDrawn != 1 || center.B < 200 || center.R > 120 || center.G > 160)
            throw new Exception("Local player (1000==1000, no team on squad) squad dot must be drawn blue via ID fallback.");
        var mateOn = DrawUnits(fogFrame with { Squads = new[] { new NativeSquadPosition(0, 0, 1007, Team: 1) }, LocalPlayerId = 1000, LocalTeam = 1 }, true);
        var mateCenter = bitmap.GetPixel(134, 134);
        if (mateOn.SquadsDrawn != 1 || mateCenter.B < 200 || mateCenter.R > 120)
            throw new Exception("A teammate (different player ID, same team) squad dot must be drawn blue.");
        var redOn = DrawUnits(fogFrame with { Squads = new[] { new NativeSquadPosition(0, 0, 1001, Team: 0) }, LocalPlayerId = 1000, LocalTeam = 1 }, true);
        var redCenter = bitmap.GetPixel(134, 134);
        if (redOn.SquadsDrawn != 1 || redCenter.R < 200 || redCenter.B > 120)
            throw new Exception("An enemy team (0 vs local 1) squad dot must be drawn red.");
        var noTeamEnemy = DrawUnits(fogFrame with { Squads = new[] { new NativeSquadPosition(0, 0, 1001) }, LocalPlayerId = 1000 }, true);
        var noTeamCenter = bitmap.GetPixel(134, 134);
        if (noTeamEnemy.SquadsDrawn != 1 || noTeamCenter.R < 200 || noTeamCenter.B > 120)
            throw new Exception("With no team data the ID fallback must still color non-local players red.");
        var iconOn = DrawUnits(fogFrame with { Squads = new[] { new NativeSquadPosition(0, 0, 1000, "stug_iii_sdkfz_142_ger", "armor", 1) }, LocalPlayerId = 1000, LocalTeam = 1 }, true);
        if (iconOn.SquadsDrawn != 1 || bitmap.GetPixel(134, 134).R < 20 && bitmap.GetPixel(134, 134).B < 60)
            throw new Exception("Armored squad with a known SBP name must render its icon above the fog without errors.");
        var noneOn = DrawUnits(fogFrame with { Squads = Array.Empty<NativeSquadPosition>() }, true);
        if (noneOn.SquadsDrawn != 0 || bitmap.GetPixel(134, 134).R is < 20 or > 30)
            throw new Exception("An empty squad list must render nothing.");
        Console.WriteLine("PASS: read-only squad enumeration chain with team table, opt-in show-all-units gating, above-fog layering, and team-aware colors. Synthetic memory only.");
    }
}
