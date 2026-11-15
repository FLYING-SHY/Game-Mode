namespace CoH3Minimap;

// Mark Angle getter BC5940/BC5982 -> inline float +250; lazy storage +220.
// Read the UI's naturally computed mode-1 result, never invoke camera callbacks.
internal sealed record NativeMarkAngle(float Degrees, ulong CacheEpoch)
{
    internal static NativeMarkAngle? Read(Func<long, int, byte[]?> read, long mark)
    {
        var bytes = read(mark + 0x238, 0x1C);
        if (bytes is not { Length: 0x1C }) return null;
        ulong epoch = BitConverter.ToUInt64(bytes, 0);
        float angle = BitConverter.ToSingle(bytes, 0x18);
        if (epoch >= ulong.MaxValue - 1 || !float.IsFinite(angle)) return null;
        var after = read(mark + 0x238, 0x1C);
        if (after is not { Length: 0x1C } || !bytes.AsSpan().SequenceEqual(after)) return null;
        return new(angle, epoch);
    }
}
