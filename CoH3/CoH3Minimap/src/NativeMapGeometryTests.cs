using System.Text;

namespace CoH3Minimap;

internal static class NativeMapGeometryTests
{
    internal static void Run()
    {
        const long map = 0x500000, image = 0x140000000, heap = 0x600000;
        byte[] header = new byte[0x430], external = [];
        void U64(int offset, ulong value) => BitConverter.GetBytes(value).CopyTo(header, offset);
        void F32(int offset, float value) => BitConverter.GetBytes(value).CopyTo(header, offset);
        byte[]? Read(long address, int length)
        {
            if (address >= map && address + length <= map + header.Length) return header.AsSpan((int)(address - map), length).ToArray();
            if (address == heap && length <= external.Length) return external.AsSpan(0, length).ToArray();
            if (address == image + 0x06A8CCA0 && length == 8) return BitConverter.GetBytes(42UL);
            return null;
        }
        F32(0x100, -90); F32(0x310, 384); F32(0x2D8, 384);
        U64(0xE8, 42); U64(0x2F8, 42); U64(0x2C0, 42); U64(0x178, ulong.MaxValue);
        var geometry = NativeMapGeometry.FromModelHeader(header);
        if (geometry is not { AngleDegrees: -90, PlayableWidth: 384, PlayableHeight: 384 } || geometry.PerspectiveBox.Count != 0)
            throw new Exception("Native geometry caches or uninitialized PerspectiveBox were misread.");
        var before = header.ToArray(); F32(0x310, 0);
        if (NativeMapGeometry.FromModelHeader(header) != null || NativeMapGeometry.SameCachedValues(before, header))
            throw new Exception("Uninitialized/replaced native geometry was accepted.");
        F32(0x310, 384); U64(0xE8, ulong.MaxValue);
        if (NativeMapGeometry.FromModelHeader(header) != null) throw new Exception("Uninitialized angle epoch was accepted.");
        U64(0xE8, 42); U64(0x120, 42); U64(0x150, 10); U64(0x158, 15);
        Encoding.UTF8.GetBytes("ui_minimap\0").CopyTo(header, 0x140);
        if (NativeMinimapReader.Read(header, Read, map, image) is not { AssetPath: "ui_minimap", SameEpoch: true })
            throw new Exception("Inline native Minimap string was misread.");
        const string path = "data:scenarios\\multiplayer\\fixture\\fixture_mm_handmade.rrtex";
        external = Encoding.UTF8.GetBytes(path + '\0');
        U64(0x140, heap); U64(0x150, (ulong)external.Length - 1); U64(0x158, 127);
        if (NativeMinimapReader.Read(header, Read, map, image)?.AssetPath != path)
            throw new Exception("Heap native Minimap string was misread.");
        external[^1] = 1;
        if (NativeMinimapReader.Read(header, Read, map, image) != null) throw new Exception("Unterminated native path was accepted.");
        U64(0x150, 260);
        if (NativeMinimapReader.Read(header, Read, map, image) != null) throw new Exception("Oversized native path was accepted.");
        Console.WriteLine("PASS: native map angle/dimensions/epochs, stable geometry, inline/heap Minimap strings and strict path bounds. Synthetic memory only.");
    }
}
