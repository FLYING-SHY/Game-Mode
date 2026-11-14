using System.Text;

namespace CoH3Minimap;

/// <summary>迷雾下敌军小队的全局只读位置采样；Team 为 Player_GetTeam 同源队伍号（可缺省）。</summary>
internal readonly record struct NativeSquadPosition(float X, float Z, uint PlayerId, string Name = "", string Category = "Unknown", uint? Team = null);

/// <summary>
/// build 26550104 只读 Squad 枚举链（Python 实测锚定，逐字段 QA 复现）：
/// segBase = U64[B+0x6185EB0]；sessOff = U32[B+0x5E0BECC]；
/// World = segBase + U32[segBase+sessOff+0x100]；
/// count = U32[World+0xC]；handleOff = U32[World+0x08]；validOff = U32[World+0x14]；
/// handle=U32[segBase+handleOff+8*i] → squad=segBase+handle（qword 槽）；
/// 坐标 [squad+0x2C]=x / [squad+0x34]=z；player=segBase+U32[squad+0x58]，playerID=U32[player+8]。
/// SBP 类型链（静态+实测双验证，见 native_fow_20261111/squad_blueprint_20261112 报告）：
/// SBP=U64[squad+0x38]；标签 U32[SBP+0x48]==0x2E0000；name=MSVC string @SBP+0x8；
/// tlsIdx=U32[B+0x5DF38D0]（运行时读）；T=U64(U64[SBP+0x128]+8*tlsIdx)；typeIds=u32 vector [T+0x18..T+0x20)；
/// BPDB=B+0x6BB0020（不解引用）；map=U64[BPDB+0x138]，条目步长 0x30：name 串@+8、u32 id@+0x28。
/// 任何失败或越界一律按无类型处理，绝不写内存。
/// </summary>
internal sealed class NativeSquadReader
{
    private const long SegmentBaseRva = 0x6185EB0;
    private const long SessionOffsetRva = 0x5E0BECC;
    private const long TlsIndexRva = 0x5DF38D0;
    private const long BlueprintDatabaseRva = 0x6BB0020;
    private const int MaxSquads = 4096;
    private const int MaxTypeEntries = 4096;
    private const int MaxNameBytes = 128;
    private const uint SbpTag = 0x2E0000;
    private const uint MaxSegmentOffset = 0x10000000;
    // 判定优先级（任一命中即归属）：sniper > teamweapon > armor > artillery > airplane > vehicle > building > infantry。
    internal static readonly string[] CategoryPriority =
        { "sniper", "teamweapon", "armor", "artillery", "airplane", "vehicle", "building", "emplacement", "infantry" };

    private readonly Func<long, int, byte[]?> _read;
    private readonly long _moduleBase;
    // SBP 地址 → (name, category) 进程内缓存；同型共享，tlsIdx 每次 Capture 重读。
    private readonly Dictionary<long, (string Name, string Category)> _sbpCache = new();

    internal NativeSquadReader(GameReader reader) : this(
        (address, length) => reader.ReadBytes(new IntPtr(address), length), reader.ModuleBase.ToInt64()) { }

    internal NativeSquadReader(Func<long, int, byte[]?> read, long moduleBase)
    {
        _read = read;
        _moduleBase = moduleBase;
    }

    internal IReadOnlyList<NativeSquadPosition> Read()
    {
        var segmentBase = _read(_moduleBase + SegmentBaseRva, 8);
        if (segmentBase is null) return Array.Empty<NativeSquadPosition>();
        long segBase = BitConverter.ToInt64(segmentBase, 0);
        if (!InSegment(segBase)) return Array.Empty<NativeSquadPosition>();
        var session = _read(_moduleBase + SessionOffsetRva, 4);
        if (session is null) return Array.Empty<NativeSquadPosition>();
        uint sessOff = BitConverter.ToUInt32(session, 0);
        if (!OffsetInRange(sessOff)) return Array.Empty<NativeSquadPosition>();
        var world = _read(segBase + sessOff + 0x100, 4);
        if (world is null) return Array.Empty<NativeSquadPosition>();
        uint worldOff = BitConverter.ToUInt32(world, 0);
        if (!OffsetInRange(worldOff)) return Array.Empty<NativeSquadPosition>();
        long worldPtr = segBase + worldOff;
        var header = _read(worldPtr, 0x18);
        if (header is null) return Array.Empty<NativeSquadPosition>();
        uint handleOff = BitConverter.ToUInt32(header, 0x08);
        uint count = BitConverter.ToUInt32(header, 0x0C);
        uint validOff = BitConverter.ToUInt32(header, 0x14);
        if (count == 0 || count > MaxSquads || !OffsetInRange(handleOff) || !OffsetInRange(validOff))
            return Array.Empty<NativeSquadPosition>();
        int handleBytes = checked((int)count * 8), validBytes = checked((int)count * 4);
        var handles = _read(segBase + handleOff, handleBytes);
        var valids = _read(segBase + validOff, validBytes);
        if (handles is null || valids is null) return Array.Empty<NativeSquadPosition>();
        // 队伍表（Player_GetTeam handler rva 0xBE7A00 逐字节复现）：
        // teamTableOff = U32[segBase+sessOff+0x11C]；team = U32[segBase+teamTableOff + slot*4]（slot∈[0,8)）。
        uint? teamTableOff = ReadU32(segBase + sessOff + 0x11C);
        long teamTable = teamTableOff is not null && OffsetInRange(teamTableOff.Value)
            ? segBase + teamTableOff.Value : 0;
        // 类型名→ID 表每次 Capture 重建一次；tlsIdx 运行时重读。
        var typeMap = BuildTypeNameIdMap();
        uint? tlsIdx = ReadU32(_moduleBase + TlsIndexRva);
        var squads = new List<NativeSquadPosition>();
        for (int i = 0; i < count; i++)
        {
            if (BitConverter.ToUInt32(valids, i * 4) == 0) continue;
            long squad = segBase + BitConverter.ToUInt32(handles, i * 8);
            if (!InSegment(squad)) continue;
            var body = _read(squad, 0x60);
            if (body is null) continue;
            float x = BitConverter.ToSingle(body, 0x2C), z = BitConverter.ToSingle(body, 0x34);
            if (!float.IsFinite(x) || !float.IsFinite(z)) continue;
            uint playerRel = BitConverter.ToUInt32(body, 0x58);
            if (!OffsetInRange(playerRel)) continue;
            var player = _read(segBase + playerRel + 8, 8);
            if (player is null) continue;
            uint slot = BitConverter.ToUInt32(player, 4); // player+0x0C
            uint? team = teamTable != 0 && slot < 8 ? ReadU32(teamTable + 4L * slot) : null;
            var (name, category) = ResolveSbpInfo(BitConverter.ToInt64(body, 0x38), tlsIdx, typeMap);
            squads.Add(new(x, z, BitConverter.ToUInt32(player, 0), name, category, team));
        }
        return squads;
    }

    /// <summary>
    /// 本地玩家 ID：U32[segBase+sessOff+0x08]（+0x0C 为本地 slot，0-7）。
    /// 8 人局实测：session+0x08=1005/slot5 与本地对局一致；多人着色据此区分“我方蓝/其余红”。
    /// </summary>
    internal uint? ReadLocalPlayerId()
    {
        var segmentBase = _read(_moduleBase + SegmentBaseRva, 8);
        if (segmentBase is null) return null;
        long segBase = BitConverter.ToInt64(segmentBase, 0);
        if (!InSegment(segBase)) return null;
        var session = _read(_moduleBase + SessionOffsetRva, 4);
        if (session is null) return null;
        uint sessOff = BitConverter.ToUInt32(session, 0);
        if (!OffsetInRange(sessOff)) return null;
        return ReadU32(segBase + sessOff + 0x08);
    }

    /// <summary>
    /// 本地玩家队伍号（Player_GetTeam handler rva 0xBE7A00 同源）：
    /// localSlot = U32[segBase+sessOff+0x0C]；teamTableOff = U32[segBase+sessOff+0x11C]；
    /// localTeam = U32[segBase+teamTableOff + localSlot*4]。读不到时返回 null。
    /// </summary>
    internal uint? ReadLocalTeam()
    {
        var segmentBase = _read(_moduleBase + SegmentBaseRva, 8);
        if (segmentBase is null) return null;
        long segBase = BitConverter.ToInt64(segmentBase, 0);
        if (!InSegment(segBase)) return null;
        var session = _read(_moduleBase + SessionOffsetRva, 4);
        if (session is null) return null;
        uint sessOff = BitConverter.ToUInt32(session, 0);
        if (!OffsetInRange(sessOff)) return null;
        uint? localSlot = ReadU32(segBase + sessOff + 0x0C);
        if (localSlot is null || localSlot >= 8) return null;
        uint? teamTableOff = ReadU32(segBase + sessOff + 0x11C);
        if (teamTableOff is null || !OffsetInRange(teamTableOff.Value)) return null;
        return ReadU32(segBase + teamTableOff.Value + 4L * localSlot.Value);
    }

    private (string Name, string Category) ResolveSbpInfo(long sbp, uint? tlsIdx, Dictionary<string, uint> typeMap)
    {
        if (!InSegment(sbp)) return ("", "Unknown");
        if (_sbpCache.TryGetValue(sbp, out var cached)) return cached;
        var tag = _read(sbp + 0x48, 4);
        string name = "";
        if (tag is not null && BitConverter.ToUInt32(tag, 0) == SbpTag)
        {
            name = ReadMsvcString(sbp + 0x8) ?? "";
            var typeIds = ReadTypeIds(sbp, tlsIdx);
            string category = "Unknown";
            if (typeIds is not null && typeMap.Count > 0)
            {
                foreach (string candidate in CategoryPriority)
                {
                    if (typeMap.TryGetValue(candidate, out uint id) && typeIds.Contains(id))
                    {
                        category = candidate;
                        break;
                    }
                }
            }
            var info = (name, category);
            _sbpCache[sbp] = info;
            return info;
        }
        var unknown = (name, "Unknown");
        _sbpCache[sbp] = unknown;
        return unknown;
    }

    /// <summary>MSVC std::string：buffer@+0，size@+0x10，cap@+0x18；cap&gt;0xF 时 buffer 为指针。</summary>
    private string? ReadMsvcString(long address)
    {
        var header = _read(address, 0x20);
        if (header is null) return null;
        long size = BitConverter.ToInt64(header, 0x10);
        long cap = BitConverter.ToInt64(header, 0x18);
        if (size is < 0 or > MaxNameBytes) return null;
        if (cap <= 0xF)
        {
            var bytes = new byte[size];
            Array.Copy(header, bytes, size);
            return Encoding.ASCII.GetString(bytes);
        }
        long pointer = BitConverter.ToInt64(header, 0);
        if (!InSegment(pointer) || cap < size) return null;
        var heap = _read(pointer, (int)size);
        return heap is null ? null : Encoding.ASCII.GetString(heap);
    }

    private HashSet<uint>? ReadTypeIds(long sbp, uint? tlsIdx)
    {
        if (tlsIdx is null || tlsIdx >= 0x10000) return null;
        var typesSlot = _read(sbp + 0x128, 8);
        if (typesSlot is null) return null;
        long arrayPointer = BitConverter.ToInt64(typesSlot, 0);
        if (!InSegment(arrayPointer)) return null;
        var slot = _read(arrayPointer + 8L * tlsIdx.Value, 8);
        if (slot is null) return null;
        long t = BitConverter.ToInt64(slot, 0);
        if (!InSegment(t)) return null;
        var vector = _read(t, 0x28);
        if (vector is null) return null;
        long begin = BitConverter.ToInt64(vector, 0x18), end = BitConverter.ToInt64(vector, 0x20);
        long bytes = end - begin;
        if (bytes is < 0 or > MaxTypeEntries * 4L || bytes % 4 != 0) return null;
        if (bytes == 0) return new HashSet<uint>();
        if (!InSegment(begin)) return null;
        var data = _read(begin, (int)bytes);
        if (data is null) return null;
        var ids = new HashSet<uint>();
        for (int offset = 0; offset < data.Length; offset += 4) ids.Add(BitConverter.ToUInt32(data, offset));
        return ids;
    }

    /// <summary>BPDB 名→ID：map=U64[BPDB+0x138]，条目步长 0x30（name 串@+8，u32 id@+0x28），上限 4096 条。</summary>
    private Dictionary<string, uint> BuildTypeNameIdMap()
    {
        var map = new Dictionary<string, uint>();
        var pointer = _read(_moduleBase + BlueprintDatabaseRva + 0x138, 8);
        if (pointer is null) return map;
        long mapAddress = BitConverter.ToInt64(pointer, 0);
        if (!InSegment(mapAddress)) return map;
        var header = _read(mapAddress, 0x28);
        if (header is null) return map;
        long begin = BitConverter.ToInt64(header, 0x18), end = BitConverter.ToInt64(header, 0x20);
        long bytes = end - begin;
        if (bytes is < 0 or > MaxTypeEntries * 0x30L || bytes % 0x30 != 0) return map;
        if (bytes == 0) return map;
        if (!InSegment(begin)) return map;
        var entries = _read(begin, (int)bytes);
        if (entries is null) return map;
        for (int offset = 0; offset + 0x30 <= entries.Length; offset += 0x30)
        {
            string? name = ReadMsvcStringInPlace(entries, offset + 8);
            if (name is null || name.Length == 0) continue;
            map[name] = BitConverter.ToUInt32(entries, offset + 0x28);
        }
        return map;
    }

    /// <summary>在已读条目缓冲内解析 MSVC SSO/堆串（cap&gt;0xF 时按指针另读）。</summary>
    private string? ReadMsvcStringInPlace(byte[] entries, int offset)
    {
        long size = BitConverter.ToInt64(entries, offset + 0x10);
        long cap = BitConverter.ToInt64(entries, offset + 0x18);
        if (size is < 0 or > MaxNameBytes) return null;
        if (cap <= 0xF)
        {
            var bytes = new byte[size];
            Buffer.BlockCopy(entries, offset, bytes, 0, (int)size);
            return Encoding.ASCII.GetString(bytes);
        }
        long pointer = BitConverter.ToInt64(entries, offset);
        if (!InSegment(pointer) || cap < size) return null;
        var heap = _read(pointer, (int)size);
        return heap is null ? null : Encoding.ASCII.GetString(heap);
    }

    private uint? ReadU32(long address)
    {
        var bytes = _read(address, 4);
        return bytes is null ? null : BitConverter.ToUInt32(bytes, 0);
    }

    private static bool OffsetInRange(uint offset) => offset != 0 && offset < MaxSegmentOffset;
    private static bool InSegment(long value) => value >= 0x10000 && value < 0x800000000000;
}
