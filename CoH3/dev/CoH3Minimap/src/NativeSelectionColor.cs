namespace CoH3Minimap;

// SelectionColor is a distinct lazy property from payload RawColor.
// A448C0/A44920 reads inline RGBA floats at Mark+1D0. State is Mark+1B8.
internal sealed record NativeSelectionColor(float R, float G, float B, float A, byte CacheState)
{
    internal Color ToColor() => Color.FromArgb(Channel(A), Channel(R), Channel(G), Channel(B));
    private static int Channel(float value) => (int)Math.Clamp(MathF.Round(value * 255), 0, 255);

    internal static NativeSelectionColor? Read(Func<long, int, byte[]?> read, long mark)
    {
        var value = read(mark + 0x1B8, 0x28);
        if (value is not { Length: 0x28 } || value[0] is not (1 or 2)) return null;
        float r = BitConverter.ToSingle(value, 0x18), g = BitConverter.ToSingle(value, 0x1C),
            b = BitConverter.ToSingle(value, 0x20), a = BitConverter.ToSingle(value, 0x24);
        if (new[] { r, g, b, a }.Any(v => !float.IsFinite(v) || v < 0 || v > 1)) return null;
        var after = read(mark + 0x1B8, 0x28);
        if (after is not { Length: 0x28 } || !value.AsSpan().SequenceEqual(after)) return null;
        return new(r, g, b, a, value[0]);
    }
}
