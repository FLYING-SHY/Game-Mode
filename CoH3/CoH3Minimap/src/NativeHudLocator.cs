namespace CoH3Minimap;

internal sealed record NativeHudLocation(long ApplicationAddress, long ServiceAddress,
    long RegistryNodeAddress, long HudAddress, uint HudVtableRva, long MapAddress);

/// <summary>
/// Read-only registry lookup verified statically in native_root_20261111.
/// E58DD9: app=[B+6A8CAC8], service=[app+148].
/// 242A90: service+338 is a std::map head; node+20 is TypeInfo*,
/// node+28 is a weak reference and node+30 its object pointer.
/// E3FDB2: HUD+9C8 is the cached Map. Never evaluate a game property or lock a weak reference.
/// </summary>
internal sealed class NativeHudLocator
{
    private readonly Func<long, int, byte[]?> _read;
    private readonly long _moduleBase;

    internal NativeHudLocator(GameReader reader) : this(
        (address, length) => reader.ReadBytes(new IntPtr(address), length), reader.ModuleBase.ToInt64()) { }

    internal NativeHudLocator(Func<long, int, byte[]?> read, long moduleBase)
    {
        _read = read;
        _moduleBase = moduleBase;
    }

    private static bool Pointer(long value) => value >= 0x10000 && value < 0x800000000000 && (value & 7) == 0;
    private long U64(long address)
    {
        var bytes = _read(address, 8);
        return bytes is { Length: 8 } ? BitConverter.ToInt64(bytes) : 0;
    }
    private static long I64(byte[] bytes, int offset) => BitConverter.ToInt64(bytes, offset);

    internal NativeHudLocation? Locate()
    {
        long app = U64(_moduleBase + 0x06A8CAC8);
        if (!Pointer(app)) return null;
        long service = U64(app + 0x148);
        if (!Pointer(service)) return null;
        long head = U64(service + 0x338);
        if (!Pointer(head)) return null;
        long root = U64(head + 8), node = root, targetKey = _moduleBase + 0x06AD2890;
        var visited = new HashSet<long>();
        for (int depth = 0; depth < 256 && node != head; depth++)
        {
            if (!Pointer(node) || !visited.Add(node)) return null;
            var treeNode = _read(node, 0x38);
            if (treeNode is null || treeNode.Length != 0x38 || treeNode[0x19] != 0) return null;
            long key = I64(treeNode, 0x20);
            if (key != targetKey)
            {
                node = I64(treeNode, targetKey < key ? 0 : 0x10);
                continue;
            }
            long hud = I64(treeNode, 0x30);
            if (!Pointer(hud)) return null;
            var hudHeader = _read(hud, 0x38);
            if (hudHeader is null || hudHeader.Length != 0x38 || BitConverter.ToUInt32(hudHeader, 0x10) == 0) return null;
            long hudVtableRva = I64(hudHeader, 0) - _moduleBase;
            long secondaryRva = I64(hudHeader, 0x30) - _moduleBase;
            bool expectedHud = (hudVtableRva, secondaryRva) is
                (0x03EF3C00, 0x03DEEF30) or (0x03E23640, 0x03E23458) or (0x03EF3B50, 0x03E19928);
            if (!expectedHud) return null;
            long map = U64(hud + 0x9C8);
            if (!Pointer(map)) return null;
            var mapHeader = _read(map, 0x48);
            if (mapHeader is null || mapHeader.Length != 0x48 || I64(mapHeader, 0x40) != hud) return null;
            long mapVtableRva = I64(mapHeader, 0) - _moduleBase;
            long mapSecondaryRva = I64(mapHeader, 0x30) - _moduleBase;
            if ((mapVtableRva, mapSecondaryRva) is not
                ((0x03DF4298, 0x03DF4160) or (0x03DF4060, 0x03DF4000))) return null;
            var hudAfter = _read(hud, 0x38);
            var mapAfter = _read(map, 0x48);
            if (hudAfter is null || hudAfter.Length != 0x38 ||
                BitConverter.ToUInt32(hudAfter, 0x10) == 0 ||
                I64(hudAfter, 0) != I64(hudHeader, 0) ||
                I64(hudAfter, 0x30) != I64(hudHeader, 0x30) ||
                mapAfter is null || !mapAfter.AsSpan().SequenceEqual(mapHeader) ||
                U64(_moduleBase + 0x06A8CAC8) != app || U64(app + 0x148) != service ||
                U64(service + 0x338) != head || U64(head + 8) != root ||
                U64(node + 0x20) != targetKey || U64(node + 0x30) != hud || U64(hud + 0x9C8) != map) return null;
            return new NativeHudLocation(app, service, node, hud, (uint)hudVtableRva, map);
        }
        return null;
    }
}
