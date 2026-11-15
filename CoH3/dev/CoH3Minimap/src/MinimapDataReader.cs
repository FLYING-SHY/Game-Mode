namespace CoH3Minimap;

/// <summary>Kind of world object drawn on the minimap.</summary>
public enum EntityKind
{
    Unit = 0,       // skinned mesh / animator -> mobile entity
    Structure = 1,  // static mesh / mesh collection -> building or prop
}

/// <summary>Single entity snapshot (read-only).</summary>
public readonly record struct EntityData(
    IntPtr SourceAddress,   // GameObject address
    float WorldX,           // world X
    float WorldY,           // world Y
    int TeamId,             // GameObject+0x6C (owner/team id)
    EntityKind Kind,        // unit or structure
    uint VtableRva          // vtable RVA (for diagnostics)
);

/// <summary>Static tactical marker from the game's own minimap marker table (D+0x588).</summary>
public readonly record struct MarkerData(
    int Id,
    float WorldX,
    float WorldY,
    float Radius,
    uint Style
);

/// <summary>Map bounds info.</summary>
public readonly record struct MapInfo(
    float OriginX,  // W+0x9C
    float OriginY,  // W+0xA0
    float SizeX,    // W+0xA8 (u16->float)
    float SizeY,    // W+0xAA (u16->float)
    bool IsValid
)
{
    public readonly (float u, float v) WorldToUV(float wx, float wy)
    {
        if (!IsValid || SizeX <= 0 || SizeY <= 0) return (0, 0);
        return ((wx - OriginX) / SizeX, (wy - OriginY) / SizeY);
    }

    public readonly (float px, float py) WorldToPixel(float wx, float wy, int width, int height)
    {
        var (u, v) = WorldToUV(wx, wy);
        return (u * width, v * height);
    }
}

/// <summary>Per-frame snapshot.</summary>
public readonly record struct GameSnapshot(
    long TimestampMs,
    MapInfo Map,
    IReadOnlyList<EntityData> Entities,
    IReadOnlyList<MarkerData> Markers,
    string DataSource
);

/// <summary>
/// Reads minimap data from game memory. Read-only: the process is opened with
/// PROCESS_VM_READ | PROCESS_QUERY_INFORMATION and never written.
/// </summary>
public sealed class MinimapDataReader : IDisposable
{
    // build 26550104 RVAs (2026-11-11 update)
    private const uint RVA_ROOT_SLOT = 0x6185FC8;
    private const uint RVA_GAMEOBJECT_VTABLE = 0x3D6DF70;

    // FNV-1a 64 component type hashes (class name -> hash)
    private const ulong HashTransform = 0x87F84BF5F3786796;
    private const ulong HashSkinnedMesh = 0x0AA81EEFCDF835A5;
    private const ulong HashAnimator = 0xAB613D8CD877B9D3;
    private const ulong HashStaticMesh = 0x951B74B7FFEE57ED;
    private const ulong HashMeshCollection = 0x48780771530A248F;

    private readonly GameReader _reader;
    private readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();

    private ulong _moduleBase;
    private ulong _gameObjectVtableVa;

    private readonly List<(long Base, int Size)> _scanRegions = new();
    private readonly List<(long Address, EntityKind Kind)> _objects = new();
    private long _lastObjectRefreshMs = -100000;
    private long _lastFullRegionScanMs = -100000;

    private const int ObjectRefreshMs = 3000;      // re-enumerate GameObjects every 3 s
    private const int HeavyRegionScanMs = 60000;   // full heap discovery at most once a minute
    private const int MaxObjects = 6000;

    private System.Threading.CancellationToken _token;

    public MinimapDataReader(GameReader reader)
    {
        _reader = reader;
        _moduleBase = (ulong)reader.ModuleBase;
        _gameObjectVtableVa = _moduleBase + RVA_GAMEOBJECT_VTABLE;
    }

    public bool IsConnected => _reader.ProcessHandle != IntPtr.Zero;

    /// <summary>Read display object: D = u64(root+0x50) (deref, verified at runtime).</summary>
    public IntPtr? GetDisplayObject()
    {
        var root = _reader.ReadPointerAt(RVA_ROOT_SLOT);
        if (root is null) return null;
        long rl = root.Value.ToInt64();
        if (rl < 0x10000 || rl > 0x800000000000) return null;
        var d = _reader.ReadPointer(new IntPtr(rl + 0x50));
        if (d is null) return null;
        long dl = d.Value.ToInt64();
        if (dl < 0x10000 || dl > 0x800000000000) return null;
        return new IntPtr(dl);
    }

    /// <summary>Read map bounds: W = u64(D+0x218) then origin/size from W.</summary>
    public MapInfo ReadMapInfo(IntPtr display)
    {
        var world = _reader.ReadPointer(new IntPtr(display.ToInt64() + 0x218));
        if (world is null) return default;

        var originX = _reader.ReadF32(new IntPtr(world.Value.ToInt64() + 0x9C));
        var originY = _reader.ReadF32(new IntPtr(world.Value.ToInt64() + 0xA0));
        var sizeX = _reader.ReadU16(new IntPtr(world.Value.ToInt64() + 0xA8));
        var sizeY = _reader.ReadU16(new IntPtr(world.Value.ToInt64() + 0xAA));

        if (originX is null || originY is null || sizeX is null || sizeY is null)
            return default;

        return new MapInfo(originX.Value, originY.Value, sizeX.Value, sizeY.Value,
            float.IsFinite(originX.Value) && float.IsFinite(originY.Value) && sizeX.Value > 0 && sizeY.Value > 0);
    }

    /// <summary>
    /// Enumerate the game's GameObjects and read their TransformComponent.
    /// World position = map centre + local (x, z); verified 99.9% in-bounds.
    /// </summary>
    public List<EntityData> ReadEntities(IntPtr display, MapInfo map)
    {
        var result = new List<EntityData>(512);
        if (!map.IsValid) return result;

        long now = _clock.ElapsedMilliseconds;
        bool needRefresh = _objects.Count == 0 || (now - _lastObjectRefreshMs) > ObjectRefreshMs;

        if (needRefresh)
        {
            RefreshObjectList(now);
            _lastObjectRefreshMs = now;
        }

        float cx = map.OriginX + map.SizeX / 2f;
        float cy = map.OriginY + map.SizeY / 2f;

        foreach (var (addr, kind) in _objects)
        {
            _token.ThrowIfCancellationRequested();
            var ent = ReadObject(new IntPtr(addr), kind, map, cx, cy);
            if (ent is not null) result.Add(ent.Value);
            if (result.Count >= MaxObjects) break;
        }
        return result;
    }

    /// <summary>Refresh the GameObject address list.</summary>
    private void RefreshObjectList(long now)
    {
        _objects.Clear();

        bool heavyScan = _scanRegions.Count == 0 || (now - _lastFullRegionScanMs) > HeavyRegionScanMs;
        if (heavyScan)
        {
            _scanRegions.Clear();
            foreach (var (regionBase, regionSize, protect, type) in _reader.EnumerateMemoryRegions())
            {
                if ((type & 0x20000) == 0) continue;
                if ((protect & 0x04) == 0 && (protect & 0x02) == 0 && (protect & 0x20) == 0 && (protect & 0x40) == 0) continue;
                if (regionSize <= 0x1000 || regionSize > 0x4000000) continue;
                if (regionBase < 0x100000000 || regionBase >= 0x7F0000000000) continue;
                if (ScanForGameObjects(regionBase, regionSize, recordRegion: true))
                {
                    _scanRegions.Add((regionBase, regionSize));
                }
                if (_objects.Count >= MaxObjects) break;
            }
            _lastFullRegionScanMs = now;
        }
        else
        {
            foreach (var (regionBase, regionSize) in _scanRegions)
            {
                _token.ThrowIfCancellationRequested();
                ScanForGameObjects(regionBase, regionSize, recordRegion: false);
                if (_objects.Count >= MaxObjects) break;
            }
        }
    }

    /// <summary>Scan a memory region for GameObject vtable pointers; classify by component set.</summary>
    private bool ScanForGameObjects(long regionBase, long regionSize, bool recordRegion)
    {
        bool hit = false;
        const int chunk = 0x400000;
        for (long off = 0; off < regionSize; off += chunk)
        {
            _token.ThrowIfCancellationRequested();
            int len = (int)Math.Min(chunk, regionSize - off);
            var data = _reader.ReadBytes(new IntPtr(regionBase + off), len);
            if (data is null || data.Length < 0x40) continue;

            for (int i = 0; i + 0x40 <= data.Length; i += 8)
            {
                ulong vt = BitConverter.ToUInt64(data, i);
                if (vt != _gameObjectVtableVa) continue;
                hit = true;
                var kind = ClassifyObject(new IntPtr(regionBase + off + i));
                if (kind is not null)
                {
                    _objects.Add((regionBase + off + i, kind.Value));
                    if (_objects.Count >= MaxObjects) return hit;
                }
            }
        }
        return hit;
    }

    /// <summary>Read the component table and decide what kind of object this is.</summary>
    private EntityKind? ClassifyObject(IntPtr obj)
    {
        var header = _reader.ReadBytes(new IntPtr(obj.ToInt64() + 0x30), 16);
        if (header is null || header.Length < 16) return null;
        long begin = BitConverter.ToInt64(header, 0);
        long end = BitConverter.ToInt64(header, 8);
        if (begin <= 0x10000 || end <= begin) return null;
        long count = (end - begin) / 16;
        if (count <= 0 || count > 96) return null;

        var table = _reader.ReadBytes(new IntPtr(begin), (int)(count * 16));
        if (table is null || table.Length < count * 16) return null;

        bool hasTransform = false, hasSkinned = false, hasAnimator = false;
        bool hasStatic = false, hasMeshColl = false;
        for (int i = 0; i < count; i++)
        {
            ulong hash = BitConverter.ToUInt64(table, i * 16);
            if (hash == HashTransform) hasTransform = true;
            else if (hash == HashSkinnedMesh) hasSkinned = true;
            else if (hash == HashAnimator) hasAnimator = true;
            else if (hash == HashStaticMesh) hasStatic = true;
            else if (hash == HashMeshCollection) hasMeshColl = true;
        }
        if (!hasTransform) return null;
        if (hasSkinned || hasAnimator) return EntityKind.Unit;
        if (hasStatic || hasMeshColl) return EntityKind.Structure;
        return null;
    }

    /// <summary>Read one GameObject's transform and owner id.</summary>
    private EntityData? ReadObject(IntPtr obj, EntityKind kind, MapInfo map, float cx, float cy)
    {
        long a = obj.ToInt64();
        var header = _reader.ReadBytes(new IntPtr(a + 0x30), 16);
        if (header is null || header.Length < 16) return null;
        long begin = BitConverter.ToInt64(header, 0);
        long end = BitConverter.ToInt64(header, 8);
        if (begin <= 0x10000 || end <= begin) return null;
        long count = (end - begin) / 16;
        if (count <= 0 || count > 96) return null;

        var table = _reader.ReadBytes(new IntPtr(begin), (int)(count * 16));
        if (table is null || table.Length < count * 16) return null;

        long transform = 0;
        for (int i = 0; i < count; i++)
        {
            if (BitConverter.ToUInt64(table, i * 16) == HashTransform)
            {
                transform = BitConverter.ToInt64(table, i * 16 + 8);
                break;
            }
        }
        if (transform == 0) return null;

        // TransformComponent layout: +0x90 quaternion, +0xA0 = local position (x, height, z).
        var pos = _reader.ReadBytes(new IntPtr(transform + 0xA0), 12);
        if (pos is null || pos.Length < 12) return null;
        float lx = BitConverter.ToSingle(pos, 0);
        float lz = BitConverter.ToSingle(pos, 8);
        if (!float.IsFinite(lx) || !float.IsFinite(lz)) return null;
        if (lx == 0f && lz == 0f) return null; // unplaced template

        float wx = cx + lx;
        float wy = cy + lz;
        if (wx < map.OriginX || wx > map.OriginX + map.SizeX) return null;
        if (wy < map.OriginY || wy > map.OriginY + map.SizeY) return null;

        var teamBytes = _reader.ReadBytes(new IntPtr(a + 0x6C), 4);
        int team = teamBytes is not null && teamBytes.Length == 4 ? BitConverter.ToInt32(teamBytes, 0) : 0;

        var vtPtr = _reader.ReadPointer(obj);
        uint vtRva = vtPtr is not null ? (uint)(vtPtr.Value.ToInt64() - (long)_moduleBase) : 0;

        return new EntityData(obj, wx, wy, team, kind, vtRva);
    }

    /// <summary>Read the game's own minimap marker table (D+0x588): capture points and buildings.</summary>
    public List<MarkerData> ReadMarkers(IntPtr display, MapInfo map)
    {
        var result = new List<MarkerData>(64);
        var cPtr = _reader.ReadPointer(new IntPtr(display.ToInt64() + 0x588));
        if (cPtr is null) return result;
        long c = cPtr.Value.ToInt64();
        if (c < 0x10000 || c > 0x800000000000) return result;

        var hdr = _reader.ReadBytes(new IntPtr(c + 0x40), 16);
        if (hdr is null || hdr.Length < 16) return result;
        long begin = BitConverter.ToInt64(hdr, 0);
        long end = BitConverter.ToInt64(hdr, 8);
        if (begin <= 0x10000 || end <= begin) return result;
        long count = (end - begin) / 0x28;
        if (count <= 0 || count > 512) return result;

        var data = _reader.ReadBytes(new IntPtr(begin), (int)(count * 0x28));
        if (data is null) return result;

        float cx = map.OriginX + map.SizeX / 2f;
        float cy = map.OriginY + map.SizeY / 2f;
        for (int i = 0; i < count; i++)
        {
            int o = i * 0x28;
            int id = BitConverter.ToInt32(data, o);
            float x = BitConverter.ToSingle(data, o + 0x10);
            float z = BitConverter.ToSingle(data, o + 0x18);
            float r = BitConverter.ToSingle(data, o + 0x1C);
            uint style = BitConverter.ToUInt32(data, o + 0x20);
            if (!float.IsFinite(x) || !float.IsFinite(z)) continue;
            float wx = cx + x, wy = cy + z;
            if (wx < map.OriginX || wx > map.OriginX + map.SizeX) continue;
            if (wy < map.OriginY || wy > map.OriginY + map.SizeY) continue;
            result.Add(new MarkerData(id, wx, wy, r, style));
        }
        return result;
    }

    /// <summary>Capture a full snapshot.</summary>
    public GameSnapshot? CaptureSnapshot(System.Threading.CancellationToken token = default)
    {
        _token = token;
        token.ThrowIfCancellationRequested();
        if (!_reader.IsAlive) throw new IOException("Game process exited.");
        var display = GetDisplayObject();
        if (display is null) return null;

        var map = ReadMapInfo(display.Value);
        if (!map.IsValid) return null;

        var entities = ReadEntities(display.Value, map);
        var markers = ReadMarkers(display.Value, map);
        int units = 0;
        foreach (var e in entities) if (e.Kind == EntityKind.Unit) units++;
        string source = entities.Count > 0 ? $"objects {entities.Count} (units {units})" : "none";

        return new GameSnapshot(
            _clock.ElapsedMilliseconds,
            map, entities, markers, source
        );
    }

    public void Dispose()
    {
        _reader.Dispose();
        _scanRegions.Clear();
        _objects.Clear();
    }
}
