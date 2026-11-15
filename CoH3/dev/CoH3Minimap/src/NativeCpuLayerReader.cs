using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CoH3Minimap;

internal sealed record NativeCpuImage(int Width, int Height, byte[] Bgra)
{
    // CPU format 0 is packed BGRA. 4D7180/5B27A0 swap R/B only when uploading to GPU RGBA.
    internal Bitmap ToBitmap()
    {
        var image = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        try
        {
            var bits = image.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < Height; y++)
                    Marshal.Copy(Bgra, y * Width * 4, IntPtr.Add(bits.Scan0, y * bits.Stride), Width * 4);
            }
            finally { image.UnlockBits(bits); }
            return image;
        }
        catch { image.Dispose(); throw; }
    }
}

internal sealed record NativeCpuLayers(NativeCpuImage? Territory, NativeCpuImage? Interactivity);

// Only reads initialized CPU images. It does not obtain GPU pointers or invoke producers.
internal sealed class NativeCpuLayerReader
{
    private readonly Func<long, int, byte[]?> _read;
    private readonly long _module;
    internal NativeCpuLayerReader(GameReader game) : this((address, length) => game.ReadBytes(new IntPtr(address), length), game.ModuleBase.ToInt64()) { }
    internal NativeCpuLayerReader(Func<long, int, byte[]?> read, long moduleBase) { _read = read; _module = moduleBase; }

    internal NativeCpuLayers Read() => new(ReadTerritory(), ReadInteractivity());

    private NativeCpuImage? ReadTerritory()
    {
        long root = Pointer(_module + 0x6B02D78);
        var control = Bytes(root, 0x50);
        var ready = Bytes(_module + 0x6B02D80, 2);
        if (control is null || ready is null || ready[0] != 1 || ready[1] != 1 || control[0] != 0) return null;
        ulong next = BitConverter.ToUInt64(control, 0x48);
        if (next > 1) return null;
        long imageAddress = root + 0x98 + (long)((next + 1) & 1) * 0x38;
        var image = ReadImage(imageAddress);
        var after = Bytes(root, 0x50);
        if (image is null || after is null || after[0] != 0 || BitConverter.ToUInt64(after, 0x48) != next ||
            root != Pointer(_module + 0x6B02D78) || !ready.AsSpan().SequenceEqual(Bytes(_module + 0x6B02D80, 2))) return null;
        return image;
    }

    private NativeCpuImage? ReadInteractivity()
    {
        long root = Pointer(_module + 0x6B02D88);
        var image = ReadImage(root);
        return root == Pointer(_module + 0x6B02D88) ? image : null;
    }

    private NativeCpuImage? ReadImage(long address)
    {
        var header = Bytes(address, 0x30);
        if (header is null || BitConverter.ToUInt32(header, 0x10) != 0) return null;
        uint width = BitConverter.ToUInt32(header, 0x14), height = BitConverter.ToUInt32(header, 0x18);
        uint total = BitConverter.ToUInt32(header, 0x1C);
        if (width == 0 || height == 0 || width > 4096 || height > 4096 ||
            (ulong)width * height * 4 != total || total > 16 * 1024 * 1024 ||
            BitConverter.ToUInt64(header, 0x28) != (ulong)width * height) return null;
        long pixels = BitConverter.ToInt64(header, 0x20);
        var bytes = Bytes(pixels, (int)total);
        var after = Bytes(address, 0x30);
        if (bytes is null || after is null || !header.AsSpan().SequenceEqual(after)) return null;
        return new((int)width, (int)height, bytes);
    }

    private long Pointer(long address)
    {
        var bytes = Bytes(address, 8);
        long value = bytes is null ? 0 : BitConverter.ToInt64(bytes);
        return value >= 0x10000 && value < 0x0000800000000000 ? value : 0;
    }

    private byte[]? Bytes(long address, int length)
    {
        if (address < 0x10000 || address >= 0x0000800000000000 || length <= 0) return null;
        var bytes = _read(address, length);
        return bytes?.Length == length ? bytes : null;
    }
}
