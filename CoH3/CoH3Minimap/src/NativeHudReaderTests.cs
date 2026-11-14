namespace CoH3Minimap;

internal static class NativeHudReaderTests
{
    internal static void Run()
    {
        const long image = 0x140000000, map = 0x200000, collection = 0x210000,
            entries = 0x220000, mark = 0x230000, payload = 0x240000;
        var memory = new Dictionary<long, byte[]>
        {
            [map] = new byte[0x430], [collection] = new byte[0x160],
            [entries] = new byte[0x20], [mark] = new byte[0xD0], [payload] = new byte[0x28]
        };
        void I64(long region, int offset, long value) => BitConverter.GetBytes(value).CopyTo(memory[region], offset);
        void F32(long region, int offset, float value) => BitConverter.GetBytes(value).CopyTo(memory[region], offset);
        byte[]? Read(long address, int length)
        {
            foreach (var (start, data) in memory)
            {
                long offset = address - start;
                if (offset >= 0 && offset + length <= data.Length)
                    return data.AsSpan((int)offset, length).ToArray();
            }
            return null;
        }
        I64(map, 0, image + 0x03DF4298);
        I64(map, 0x428, collection);
        I64(collection, 0x150, entries);
        I64(collection, 0x158, entries + 0x20);
        I64(entries, 0x18, mark);
        I64(mark, 0, image + 0x03DA1750);
        I64(mark, 0xC0, payload);
        I64(mark, 0xC8, 42);
        I64(payload, 0x08, 0x1122334455667788);
        F32(payload, 0x10, 75.563f);
        F32(payload, 0x14, -65.881f);
        F32(payload, 0x18, 2.5f);
        BitConverter.GetBytes(0xA1B2C3D4u).CopyTo(memory[payload], 0x1C);
        F32(payload, 0x20, 12.5f);
        BitConverter.GetBytes(0xAABBCC03u).CopyTo(memory[payload], 0x24);
        var reader = new NativeHudReader(Read, image);
        var sample = reader.ReadMapModel(map);
        if (sample is null || sample.Marks.Count != 1 || sample.CollectionEntryCount != 1 || sample.RejectedEntryCount != 0)
            throw new Exception("Native Marks collection fixture was not read from the confirmed layout.");
        var value = sample.Marks[0];
        if (value.IconHandle != 0x1122334455667788 || value.PositionX != 75.563f || value.PositionY != -65.881f ||
            value.RawColor != 0xA1B2C3D4 || value.RawScale != 2.5f || value.RawAngle != 12.5f || value.RawAngleMode != 3)
            throw new Exception("Native UI payload values were reinterpreted or read at the wrong offsets.");
        I64(map, 0, image + 0x03D6DF70); // A GameObject must not masquerade as a MapModel.
        if (reader.ReadMapModel(map) is not null) throw new Exception("Visual model accepted as native MapModel.");
        I64(map, 0, image + 0x03DF4298);
        I64(collection, 0x158, entries + 0x21);
        if (reader.ReadMapModel(map) is not null) throw new Exception("Misaligned collection bounds accepted.");
        I64(collection, 0x158, entries + 0x20);
        F32(payload, 0x10, float.NaN);
        sample = reader.ReadMapModel(map);
        if (sample is null || sample.Marks.Count != 0 || sample.RejectedEntryCount != 1)
            throw new Exception("Invalid native Position was not rejected and counted.");
        F32(payload, 0x10, 75.563f);
        int boundReads = 0;
        var changedList = new NativeHudReader((address, length) =>
        {
            var data = Read(address, length);
            if (address == collection + 0x150 && ++boundReads == 2 && data is not null) data[8] ^= 0x20;
            return data;
        }, image);
        if (changedList.ReadMapModel(map) is not null) throw new Exception("Replaced Marks list published as a stable frame.");
        var missingPayload = new NativeHudReader((address, length) => address == payload ? null : Read(address, length), image);
        sample = missingPayload.ReadMapModel(map);
        if (sample is null || sample.Marks.Count != 0 || sample.RejectedEntryCount != 1)
            throw new Exception("Unreadable native payload was not rejected.");
        Console.WriteLine("PASS: native Marks offsets, raw Position preservation, model-type checks, bounds, rejected records and list replacement. Synthetic memory only; no live-object claim.");
    }
}
