using System.Text;

namespace CoH3Minimap;

internal sealed record NativeMinimapReference(string? AssetPath, string RawBytesHex,
    ulong CacheEpoch, ulong? GlobalEpoch, bool SameEpoch);

internal static class NativeMinimapReader
{
    // Minimap lazy storage +108, epoch +120, boxed UI reference +138,
    // std::string +140 (length +150, capacity +158). Original bytes verified.
    internal static NativeMinimapReference? Read(byte[] header, Func<long, int, byte[]?> read, long map, long moduleBase)
    {
        if (header.Length < 0x160) return null;
        ulong epoch = BitConverter.ToUInt64(header, 0x120), length = BitConverter.ToUInt64(header, 0x150),
            capacity = BitConverter.ToUInt64(header, 0x158);
        if (epoch == ulong.MaxValue || length > 259 || length > capacity || (capacity <= 15 && length > 15)) return null;
        long text = capacity <= 15 ? map + 0x140 : BitConverter.ToInt64(header, 0x140);
        if (text < 0x10000 || text >= 0x800000000000) return null;
        var data = capacity <= 15 ? header.AsSpan(0x140, (int)length + 1).ToArray() : read(text, (int)length + 1);
        if (data is null || data.Length != (int)length + 1 || data[^1] != 0 || data.AsSpan(0, (int)length).Contains((byte)0)) return null;
        var again = read(text, (int)length + 1);
        if (again is null || !data.AsSpan().SequenceEqual(again)) return null;
        var globalBytes = read(moduleBase + 0x06A8CCA0, 8);
        ulong? global = globalBytes is { Length: 8 } ? BitConverter.ToUInt64(globalBytes) : null;
        string? path;
        try { path = new UTF8Encoding(false, true).GetString(data, 0, (int)length); }
        catch (DecoderFallbackException) { path = null; }
        return new(path, Convert.ToHexString(data.AsSpan(0, (int)length)), epoch, global, global == epoch);
    }
}
