namespace CoH3Minimap;

internal static class NativeFogGenerationTests
{
    internal static void Run()
    {
        var p0 = new byte[8]; var p1 = new byte[8]; var p2 = new byte[8];
        void Bit(byte[] bytes, int n) => bytes[n >> 3] |= (byte)(1 << (n & 7));
        Bit(p0, 0); Bit(p1, 0); Bit(p2, 0); // Plane0 wins over both others.
        Bit(p1, 1); Bit(p2, 1);            // Plane1 wins over plane2.
        Bit(p2, 2); Bit(p0, 7); Bit(p0, 31); Bit(p0, 32);
        var source = new NativeFogPlanes(16, 4, 2, 0, 0, false, p0, p1, p2);
        var alpha = NativeFogGeneration.Generate(source) ?? throw new Exception("Fog generation rejected valid planes.");
        const int width = 18;
        byte At(int x, int y) => alpha[(y + 1) * width + x + 1];
        Require(At(0, 0) == 0 && At(1, 0) == 128 && At(2, 0) == 224 && At(3, 0) == 128,
            "Native fog priority and fallback");
        Require(At(7, 0) == 0 && At(15, 1) == 0 && At(0, 2) == 0 && At(0, 3) == 128,
            "LSB-first indexing, 32-bit boundary and no generation Y flip");
        Require(alpha[0] == 128 && alpha[^1] == 128, "One-pixel native border");
        var hidden = NativeFogGeneration.Generate(source with { RawConfiguration = 1 })!;
        Require(hidden[0] == 255 && hidden[width + 4] == 255 && hidden[width + 1] == 0,
            "Hide-unexplored configuration affects only fallback");
        Require(NativeFogGeneration.Generate(source with { Plane1 = Array.Empty<byte>() }) is null &&
            NativeFogGeneration.Generate(source with { Width = -1 }) is null &&
            NativeFogGeneration.Generate(source with { Width = 8, Height = 1, Plane0 = new byte[1], Plane1 = new byte[1], Plane2 = new byte[1] }) is null &&
            NativeFogGeneration.Generate(source with { Width = 9, Height = 8, Plane0 = new byte[9], Plane1 = new byte[9], Plane2 = new byte[9] }) is null,
            "Malformed planes and partial row/uint32 rejected");

        var impulse = new byte[25]; impulse[12] = 255;
        var blurred = NativeFogGeneration.Blur(impulse, 5, 5, 128);
        Require(blurred[12] == 28 && blurred[6] == 28 && blurred[0] == 128 && blurred[^1] == 128,
            "Single equal-weight 3x3 pass and forced border");
        var constant = Enumerable.Repeat((byte)128, 25).ToArray();
        Require(NativeFogGeneration.Blur(constant, 5, 5, 128).All(value => value == 128), "Fog steady-state constant");
        var image = NativeFogGeneration.SteadyStateBlackAlpha(source)!;
        Require(image.Width == 18 && image.Height == 6 && image.Bgra.Where((_, i) => i % 4 != 3).All(value => value == 0),
            "Black alpha surface with original bordered dimensions");
        Require(p0[0] == 129 && p1[0] == 3 && p2[0] == 7, "Source planes stay unchanged");
        Console.WriteLine("PASS: extracted fog shader bit order, priority, fallback, border and one-pass steady-state blur. UI material selection and temporal animation remain separate checks.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
