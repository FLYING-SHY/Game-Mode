namespace CoH3Minimap;

// Raw CPU bitplanes. Names, bit order and GPU generation rules are intentionally not inferred here.
internal sealed record NativeFogPlanes(int Width, int Height, byte Mode, uint Flags, byte RawConfiguration, bool Dirty,
    byte[] Plane0, byte[] Plane1, byte[] Plane2, long UiResourceAddress = 0, bool? UiDisabled = null);

internal sealed class NativeFogReader
{
    private readonly Func<long, int, byte[]?> _read;
    private readonly long _module;
    internal NativeFogReader(GameReader game)
        : this((address, length) => game.ReadBytes(new IntPtr(address), length), game.ModuleBase.ToInt64()) { }
    internal NativeFogReader(Func<long, int, byte[]?> read, long moduleBase) { _read = read; _module = moduleBase; }

    internal NativeFogPlanes? Read()
    {
        long root = Pointer(_module + 0x6185FC8), controller = Pointer(root + 0x50);
        if (controller == 0) return null;
        var header = Bytes(controller + 0x5B0, 0x33);
        if (header is null || header[0x32] != 0 || header[0x31] > 1) return null;
        uint width = BitConverter.ToUInt32(header, 0x20), height = BitConverter.ToUInt32(header, 0x24);
        ulong bits = (ulong)width * height;
        if (width == 0 || height == 0 || width > 4096 || height > 4096 || (width & 7) != 0 || (bits & 31) != 0) return null;
        int count = checked((int)(bits >> 3));
        long owner = BitConverter.ToInt64(header, 0), p0 = BitConverter.ToInt64(header, 8);
        long p1 = BitConverter.ToInt64(header, 0x10), p2 = BitConverter.ToInt64(header, 0x18);
        if (!Valid(owner) || p0 != owner || p1 != owner + count || p2 != owner + 2L * count) return null;
        var plane0 = Bytes(p0, count); var plane1 = Bytes(p1, count); var plane2 = Bytes(p2, count);
        var after = Bytes(controller + 0x5B0, 0x33);
        if (plane0 is null || plane1 is null || plane2 is null || after is null || !header.AsSpan().SequenceEqual(after) ||
            root != Pointer(_module + 0x6185FC8) || controller != Pointer(root + 0x50)) return null;
        // UI resource selection in 6A7A30 is separate from the CPU bitplanes.
        long ui = Pointer(_module + 0x6B02D70);
        var gate = Bytes(ui + 0x41, 1);
        var gateAfter = Bytes(ui + 0x41, 1);
        bool? disabled = gate is not null && gateAfter is not null && gate[0] <= 1 &&
            gate.AsSpan().SequenceEqual(gateAfter) && ui == Pointer(_module + 0x6B02D70)
            ? gate[0] != 0 : null;
        return new((int)width, (int)height, header[0x28], BitConverter.ToUInt32(header, 0x2C), header[0x30], header[0x31] != 0,
            plane0, plane1, plane2, disabled is not null ? ui : 0, disabled);
    }

    private long Pointer(long address)
    {
        var bytes = Bytes(address, 8);
        long pointer = bytes is null ? 0 : BitConverter.ToInt64(bytes);
        return Valid(pointer) ? pointer : 0;
    }
    private static bool Valid(long address) => address >= 0x10000 && address < 0x0000800000000000;
    private byte[]? Bytes(long address, int length)
    {
        if (!Valid(address) || length <= 0 || length > 2 * 1024 * 1024) return null;
        var bytes = _read(address, length);
        return bytes?.Length == length ? bytes : null;
    }
}
