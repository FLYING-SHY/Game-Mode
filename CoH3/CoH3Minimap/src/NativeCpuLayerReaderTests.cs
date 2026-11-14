namespace CoH3Minimap;

internal static class NativeCpuLayerReaderTests
{
    internal static void Run()
    {
        const long module = 0x140000000, root = 0x300000, mask = 0x330000;
        const long pixels0 = 0x400000, pixels1 = 0x410000, maskPixels = 0x420000;
        var memory = new Dictionary<long, byte[]>();
        Action<long>? onRead = null;
        byte[] globals = memory[module + 0x6B02D78] = new byte[0x18];
        Put64(globals, 0, root); globals[8] = globals[9] = 1; Put64(globals, 0x10, mask);
        byte[] control = memory[root] = new byte[0x108];
        byte[] maskHeader = memory[mask] = new byte[0x30];
        Header(control, 0x98, pixels0); Header(control, 0xD0, pixels1); Header(maskHeader, 0, maskPixels);
        byte[] red = memory[pixels0] = Enumerable.Range(0, 4).SelectMany(_ => new byte[] { 10, 20, 230, 128 }).ToArray();
        byte[] blue = memory[pixels1] = Enumerable.Range(0, 4).SelectMany(_ => new byte[] { 230, 20, 10, 255 }).ToArray();
        memory[maskPixels] = Enumerable.Range(0, 4).SelectMany(_ => new byte[] { 255, 255, 255, 128 }).ToArray();
        byte[]? Read(long address, int length)
        {
            onRead?.Invoke(address);
            foreach (var region in memory)
                if (address >= region.Key && address - region.Key <= region.Value.Length - length)
                    return region.Value.AsSpan((int)(address - region.Key), length).ToArray();
            return null;
        }
        var reader = new NativeCpuLayerReader(Read, module);
        var layers = reader.Read();
        if (layers.Territory is null || layers.Interactivity is null || !layers.Territory.Bgra.AsSpan().SequenceEqual(blue))
            throw new Exception("Territory reader did not select the last enqueued slot.");
        using (var bitmap = layers.Territory.ToBitmap())
            if (bitmap.GetPixel(0, 0) != Color.FromArgb(255, 10, 20, 230))
                throw new Exception("Native packed BGRA CPU pixels had their red and blue channels swapped.");
        Put64(control, 0x48, 1);
        layers = reader.Read();
        if (layers.Territory is null || !layers.Territory.Bgra.AsSpan().SequenceEqual(red))
            throw new Exception("Territory read used a fixed slot instead of the last enqueue.");
        using (var bitmap = layers.Territory.ToBitmap())
            if (bitmap.GetPixel(0, 0).A != 128 || bitmap.GetPixel(0, 0).R != 230)
                throw new Exception("CPU territory alpha or red channel was lost.");
        control[0] = 1;
        if (reader.Read().Territory is not null) throw new Exception("Dirty territory was accepted.");
        control[0] = 0; globals[9] = 0;
        if (reader.Read().Territory is not null) throw new Exception("Uninitialized CPU territory images were accepted.");
        globals[9] = 1;
        BitConverter.GetBytes(4u).CopyTo(control, 0x98 + 0x10);
        if (reader.Read().Territory is not null) throw new Exception("An unverified CPU pixel format was accepted.");
        BitConverter.GetBytes(0u).CopyTo(control, 0x98 + 0x10);
        BitConverter.GetBytes(uint.MaxValue).CopyTo(control, 0x98 + 0x1C);
        if (reader.Read().Territory is not null) throw new Exception("An invalid pixel allocation length was accepted.");
        Header(control, 0x98, pixels0);
        onRead = address => { if (address == pixels0) Put64(control, 0x48, 0); };
        if (reader.Read().Territory is not null) throw new Exception("A territory slot change during a snapshot was accepted.");
        onRead = address => { if (address == maskPixels) Put64(globals, 0x10, 0); };
        if (reader.Read().Interactivity is not null) throw new Exception("An interactivity root change during a snapshot was accepted.");
        Console.WriteLine("PASS: native CPU layer packed BGRA/alpha, enqueue slot, initialization, dirty/format/bounds and snapshot replacement guards. Synthetic memory only.");
    }

    private static void Header(byte[] bytes, int offset, long pixels)
    {
        BitConverter.GetBytes(0u).CopyTo(bytes, offset + 0x10);
        BitConverter.GetBytes(2u).CopyTo(bytes, offset + 0x14);
        BitConverter.GetBytes(2u).CopyTo(bytes, offset + 0x18);
        BitConverter.GetBytes(16u).CopyTo(bytes, offset + 0x1C);
        Put64(bytes, offset + 0x20, pixels); Put64(bytes, offset + 0x28, 4);
    }
    private static void Put64(byte[] bytes, int offset, long value) => BitConverter.GetBytes(value).CopyTo(bytes, offset);
}
