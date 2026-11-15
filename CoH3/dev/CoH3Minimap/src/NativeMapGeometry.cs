namespace CoH3Minimap;

// Original cached UI values, not inferred simulation/world coordinates.
// Angle BC5B70 -> +100; Width BC6210 -> +310; Height BC6280 -> +2D8.
// PerspectiveBox DBF950 -> +190/+1A0. Lazy cache epochs are +18 from storage.
internal sealed record NativeMapGeometry(float AngleDegrees, float PlayableWidth, float PlayableHeight,
    IReadOnlyList<PointF> PerspectiveBox, ulong AngleEpoch, ulong WidthEpoch, ulong HeightEpoch)
{
    internal static NativeMapGeometry? FromModelHeader(byte[] header)
    {
        if (header.Length < 0x314) return null;
        float angle = BitConverter.ToSingle(header, 0x100), width = BitConverter.ToSingle(header, 0x310),
            height = BitConverter.ToSingle(header, 0x2D8);
        ulong ae = BitConverter.ToUInt64(header, 0xE8), we = BitConverter.ToUInt64(header, 0x2F8),
            he = BitConverter.ToUInt64(header, 0x2C0);
        if (!float.IsFinite(angle) || !float.IsFinite(width) || !float.IsFinite(height) ||
            width <= 0 || height <= 0 || ae >= ulong.MaxValue - 1 || we >= ulong.MaxValue - 1 || he >= ulong.MaxValue - 1) return null;
        var corners = new List<PointF>();
        for (int offset = 0x190; offset < 0x1B0; offset += 8)
        {
            float x = BitConverter.ToSingle(header, offset), y = BitConverter.ToSingle(header, offset + 4);
            if (!float.IsFinite(x) || !float.IsFinite(y)) { corners.Clear(); break; }
            corners.Add(new PointF(x, y));
        }
        if (corners.Distinct().Count() < 2) corners.Clear();
        return new(angle, width, height, corners, ae, we, he);
    }

    internal static bool SameCachedValues(byte[] before, byte[] after)
    {
        foreach (var (offset, length) in new[] { (0xE8, 8), (0x100, 4), (0x2F8, 8), (0x310, 4),
            (0x2C0, 8), (0x2D8, 4), (0x178, 8), (0x190, 32) })
            if (!before.AsSpan(offset, length).SequenceEqual(after.AsSpan(offset, length))) return false;
        return true;
    }
}
