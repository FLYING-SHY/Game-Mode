namespace CoH3Minimap;

// Coordinates and attributes below are the original UI model values. They are
// deliberately not labelled as world coordinates, military units, or team IDs.
internal sealed record NativeMarkSample(long ModelAddress, string ModelType,
    long PayloadAddress, ulong ModelEpoch, ulong IconHandle, float PositionX, float PositionY,
    float RawScale, uint RawColor, float RawAngle, uint RawAngleMode, NativeSelectionColor? SelectionColor = null,
    NativeMarkAngle? CachedAngle = null);

internal sealed record NativeMapSample(long ModelAddress, uint ModelVtableRva,
    long MarksCollectionAddress, long MapIconsAddress, int CollectionEntryCount,
    int RejectedEntryCount, IReadOnlyList<NativeMarkSample> Marks, NativeMapGeometry? Geometry = null,
    NativeMinimapReference? Minimap = null);

/// <summary>
/// build 26550104 static evidence from native_hud_20261111:
/// MapModel Marks getter F7ED00 -> +428; MapIcons -> +3F0.
/// A3F8A0: collection [+150,+158), stride20, entry+18 is MarkModel*.
/// DF2400/52A460: Position getter -> payload=[Mark+C0], floats payload+10/+14.
/// MapCanvas centered layout is statically verified. Same-scene pixel agreement is tested separately.
/// </summary>
internal sealed class NativeHudReader
{
    internal const string SupportedImageSha256 = "2406B3EE580BFC983B0AEA93A31C26F9A6C98F10F706C5CEC6A13AB5E3A22CDE";
    private readonly Func<long, int, byte[]?> _read;
    private readonly long _moduleBase;
    private const int MaxMarks = 8192;

    internal NativeHudReader(GameReader reader) : this(
        (address, length) => reader.ReadBytes(new IntPtr(address), length), reader.ModuleBase.ToInt64()) { }

    internal NativeHudReader(Func<long, int, byte[]?> read, long moduleBase)
    {
        _read = read;
        _moduleBase = moduleBase;
    }

    internal static bool IsMapVtable(uint rva) => rva is 0x03DF4298 or 0x03DF4060;
    private static bool Pointer(long value) => value >= 0x10000 && value < 0x800000000000 && (value & 7) == 0;
    private static long I64(byte[] bytes, int offset) => BitConverter.ToInt64(bytes, offset);

    internal NativeMapSample? ReadMapModel(long address)
    {
        if (!Pointer(address)) return null;
        var header = _read(address, 0x430);
        if (header is null || header.Length != 0x430) return null;
        long vtableRva = I64(header, 0) - _moduleBase;
        if (vtableRva < 0 || vtableRva > uint.MaxValue || !IsMapVtable((uint)vtableRva)) return null;
        long collection = I64(header, 0x428);
        long mapIcons = I64(header, 0x3F0);
        if (!Pointer(collection)) return null;
        var bounds = _read(collection + 0x150, 16);
        if (bounds is null || bounds.Length != 16) return null;
        long begin = I64(bounds, 0), end = I64(bounds, 8);
        long byteCount = end - begin;
        if (byteCount < 0 || byteCount > MaxMarks * 0x20L || byteCount % 0x20 != 0) return null;
        if (byteCount > 0 && (!Pointer(begin) || !Pointer(end))) return null;
        if (byteCount == 0 && begin != 0 && !Pointer(begin)) return null;
        var entries = byteCount == 0 ? Array.Empty<byte>() : _read(begin, (int)byteCount);
        if (entries is null || entries.Length != byteCount) return null;
        var marks = new List<NativeMarkSample>();
        var seen = new HashSet<long>();
        int rejected = 0;
        for (int offset = 0; offset < entries.Length; offset += 0x20)
        {
            long markAddress = I64(entries, offset + 0x18);
            if (!seen.Add(markAddress)) { rejected++; continue; }
            var mark = ReadMark(markAddress);
            if (mark is null) { rejected++; continue; }
            marks.Add(mark);
        }
        var geometry = NativeMapGeometry.FromModelHeader(header);
        var minimap = NativeMinimapReader.Read(header, _read, address, _moduleBase);
        // A cache/list replaced while reading must never be published as a frame.
        var boundsAfter = _read(collection + 0x150, 16);
        var modelAfter = _read(address, 0x430);
        if (boundsAfter is null || !bounds.AsSpan().SequenceEqual(boundsAfter) ||
            modelAfter is null || modelAfter.Length != header.Length ||
            I64(modelAfter, 0) != I64(header, 0) || I64(modelAfter, 0x428) != collection ||
            !NativeMapGeometry.SameCachedValues(header, modelAfter) ||
            !header.AsSpan(0x120, 8).SequenceEqual(modelAfter.AsSpan(0x120, 8)) ||
            !header.AsSpan(0x140, 0x20).SequenceEqual(modelAfter.AsSpan(0x140, 0x20))) return null;
        return new NativeMapSample(address, (uint)vtableRva, collection, mapIcons,
            entries.Length / 0x20, rejected, marks, geometry, minimap);
    }

    private NativeMarkSample? ReadMark(long address)
    {
        if (!Pointer(address)) return null;
        var model = _read(address, 0xD0);
        if (model is null || model.Length != 0xD0) return null;
        string? kind = (I64(model, 0) - _moduleBase) switch
        {
            0x03DA1750 => "MarkModel",
            0x03DA2D90 => "SquadMarkModelBase",
            0x03DA33D0 => "EntityMarkModelBase",
            0x04803FA0 => "CustomPointMarkModel",
            0x03E9B010 => "CustomSquadMarkModel",
            0x03DA2E40 => "SquadMarkModel",
            0x04804210 => "CustomStructureMarkModel",
            0x03DA3550 => "StructureMarkModel",
            0x03EB3CF0 => "TeamWeaponMarkModel",
            0x03E98088 => "VehicleMarkModel",
            0x0476E698 => "EmplacementMarkModel",
            0x03DA3480 => "AnvilStrategicPointMarkModel",
            0x03E9D3C0 => "AnvilVictoryPointMarkModel",
            0x03DA3DC0 => "AnvilManpowerPointMarkModel",
            _ => null
        };
        if (kind is null) return null;
        long payloadAddress = I64(model, 0xC0);
        if (!Pointer(payloadAddress)) return null;
        var payload = _read(payloadAddress, 0x28);
        if (payload is null || payload.Length != 0x28) return null;
        float x = BitConverter.ToSingle(payload, 0x10), y = BitConverter.ToSingle(payload, 0x14);
        if (!float.IsFinite(x) || !float.IsFinite(y)) return null;
        var selectionColor = NativeSelectionColor.Read(_read, address);
        var cachedAngle = NativeMarkAngle.Read(_read, address);
        var stateAfter = _read(address + 0xC0, 0x10);
        if (stateAfter is null || stateAfter.Length != 0x10 ||
            !model.AsSpan(0xC0, 0x10).SequenceEqual(stateAfter)) return null;
        return new NativeMarkSample(address, kind, payloadAddress, BitConverter.ToUInt64(model, 0xC8),
            BitConverter.ToUInt64(payload, 0x08), x, y, BitConverter.ToSingle(payload, 0x18),
            BitConverter.ToUInt32(payload, 0x1C), BitConverter.ToSingle(payload, 0x20), payload[0x24], selectionColor, cachedAngle);
    }
}
