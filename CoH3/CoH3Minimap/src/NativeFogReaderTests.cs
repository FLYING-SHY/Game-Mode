namespace CoH3Minimap;

internal static class NativeFogReaderTests
{
    internal static void Run()
    {
        const long module = 0x140000000, root = 0x280000, controller = 0x300000, owner = 0x400000;
        var regions = new Dictionary<long, byte[]>();
        byte[] global = regions[module + 0x6185FC8] = BitConverter.GetBytes(root);
        byte[] rootBytes = regions[root] = new byte[0x58];
        BitConverter.GetBytes(controller).CopyTo(rootBytes, 0x50);
        byte[] header = regions[controller + 0x5B0] = new byte[0x33];
        BitConverter.GetBytes(owner).CopyTo(header, 0);
        for (int i = 0; i < 3; i++) BitConverter.GetBytes(owner + i * 8L).CopyTo(header, 8 + i * 8);
        BitConverter.GetBytes(8u).CopyTo(header, 0x20); BitConverter.GetBytes(8u).CopyTo(header, 0x24);
        header[0x28] = 3; BitConverter.GetBytes(0x11223344u).CopyTo(header, 0x2C);
        byte[] payload = regions[owner] = Enumerable.Range(0, 24).Select(i => (byte)i).ToArray();
        Action<long>? onRead = null;
        byte[]? Read(long address, int count)
        {
            if (count > 2 * 1024 * 1024) throw new Exception("Unbounded fog read.");
            onRead?.Invoke(address);
            foreach (var region in regions)
                if (address >= region.Key && address - region.Key <= region.Value.Length - count)
                    return region.Value.AsSpan((int)(address - region.Key), count).ToArray();
            return null;
        }
        const long ui = 0x500000;
        regions[module + 0x6B02D70] = BitConverter.GetBytes(ui);
        var uiBytes = regions[ui] = new byte[0x50];
        uiBytes[0x41] = 1;
        var reader = new NativeFogReader(Read, module);
        var sample = reader.Read();
        if (sample is null || sample.Width != 8 || sample.Height != 8 || sample.Mode != 3 || sample.Flags != 0x11223344 ||
            !sample.Plane0.AsSpan().SequenceEqual(payload.AsSpan(0, 8)) || !sample.Plane2.AsSpan().SequenceEqual(payload.AsSpan(16, 8)))
            throw new Exception("Native fog plane layout was not preserved.");
        if (sample.UiResourceAddress != ui || sample.UiDisabled != true)
            throw new Exception("Native fog Disabled resource selector was not read.");
        uiBytes[0x41] = 0;
        if (reader.Read()?.UiDisabled != false) throw new Exception("Native fog active resource selector was not read.");
        header[0x32] = 1;
        if (reader.Read() is not null) throw new Exception("Resizing fog memory was accepted.");
        header[0x32] = 0;
        BitConverter.GetBytes(7u).CopyTo(header, 0x20); BitConverter.GetBytes(7u).CopyTo(header, 0x24);
        if (reader.Read() is not null) throw new Exception("A partial-byte fog allocation was accepted.");
        BitConverter.GetBytes(8u).CopyTo(header, 0x20); BitConverter.GetBytes(1u).CopyTo(header, 0x24);
        BitConverter.GetBytes(owner + 1).CopyTo(header, 0x10); BitConverter.GetBytes(owner + 2).CopyTo(header, 0x18);
        if (reader.Read() is not null) throw new Exception("A partial uint32 fog plane was accepted.");
        BitConverter.GetBytes(9u).CopyTo(header, 0x20); BitConverter.GetBytes(8u).CopyTo(header, 0x24);
        BitConverter.GetBytes(owner + 9).CopyTo(header, 0x10); BitConverter.GetBytes(owner + 18).CopyTo(header, 0x18);
        regions[owner] = new byte[27];
        if (reader.Read() is not null) throw new Exception("A non-byte-aligned fog row was accepted.");
        regions[owner] = payload;
        BitConverter.GetBytes(8u).CopyTo(header, 0x20); BitConverter.GetBytes(8u).CopyTo(header, 0x24);
        BitConverter.GetBytes(owner + 8).CopyTo(header, 0x10);
        BitConverter.GetBytes(owner + 17).CopyTo(header, 0x18);
        if (reader.Read() is not null) throw new Exception("A noncontiguous fog allocation was accepted.");
        BitConverter.GetBytes(owner + 16).CopyTo(header, 0x18);
        header[0x31] = 2;
        if (reader.Read() is not null) throw new Exception("An invalid fog dirty flag was accepted.");
        header[0x31] = 0;
        onRead = address => { if (address == owner + 8) Array.Clear(rootBytes, 0x50, 8); };
        if (reader.Read() is not null) throw new Exception("A fog controller replacement was accepted.");
        Console.WriteLine("PASS: bounded native fog CPU bitplanes, resize/layout guards and root replacement. Raw planes only; GPU fog composition is not asserted.");
    }
}
