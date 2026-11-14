namespace CoH3Minimap;

// FogOfWarGenerateCS and the non-temporal FogOfWarBlurCS, extracted from the
// shipped DXBC. These functions reproduce shader math; they do not establish
// which UI material consumes the result or reproduce C3's GPU frame history.
internal static class NativeFogGeneration
{
    internal static byte[]? Generate(NativeFogPlanes planes)
    {
        long bits = (long)planes.Width * planes.Height;
        if (planes.Width <= 0 || planes.Height <= 0 || planes.Width > 4096 || planes.Height > 4096 ||
            (planes.Width & 7) != 0 || (bits & 31) != 0 || planes.Plane0.Length != bits / 8 ||
            planes.Plane1.Length != bits / 8 || planes.Plane2.Length != bits / 8) return null;
        int width = planes.Width + 2, height = planes.Height + 2;
        byte fallback = planes.RawConfiguration != 0 ? (byte)255 : (byte)128;
        var alpha = new byte[checked(width * height)];
        Array.Fill(alpha, fallback);
        for (int y = 0; y < planes.Height; y++)
            for (int x = 0; x < planes.Width; x++)
            {
                int linear = y * planes.Width + x;
                int index = linear >> 3, mask = 1 << (linear & 7);
                alpha[(y + 1) * width + x + 1] =
                    (planes.Plane0[index] & mask) != 0 ? (byte)0 :
                    (planes.Plane1[index] & mask) != 0 ? (byte)128 :
                    (planes.Plane2[index] & mask) != 0 ? (byte)224 : fallback;
            }
        return alpha;
    }

    internal static byte[] Blur(byte[] generated, int width, int height, byte fallback)
    {
        if (width < 3 || height < 3 || generated.Length != checked(width * height))
            throw new ArgumentException("Invalid native fog texture dimensions.");
        var result = new byte[generated.Length];
        Array.Fill(result, fallback);
        for (int y = 1; y < height - 1; y++)
            for (int x = 1; x < width - 1; x++)
            {
                int sum = 0;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++) sum += generated[(y + dy) * width + x + dx];
                // R8 UNORM output; denominator nine cannot produce half-integer ties.
                result[y * width + x] = (byte)((sum + 4) / 9);
            }
        return result;
    }

    internal static NativeCpuImage? SteadyStateBlackAlpha(NativeFogPlanes planes)
    {
        var generated = Generate(planes);
        if (generated is null) return null;
        int width = planes.Width + 2, height = planes.Height + 2;
        var alpha = Blur(generated, width, height, planes.RawConfiguration != 0 ? (byte)255 : (byte)128);
        var bgra = new byte[checked(alpha.Length * 4)];
        for (int i = 0; i < alpha.Length; i++) bgra[i * 4 + 3] = alpha[i];
        return new NativeCpuImage(width, height, bgra);
    }
}
