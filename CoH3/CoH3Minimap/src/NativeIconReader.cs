using System.Text;

namespace CoH3Minimap;

internal sealed record NativeIconReference(ulong Handle, string? AssetPath, string RawBytesHex);

// Evidence: native_hud_20261111/icon_lookup/icon_registry_layout.json.
// Reads the native handle registry directly; does not evaluate lazy UI properties.
internal sealed class NativeIconReader
{
    private readonly Func<long, int, byte[]?> _read;
    private readonly long _global;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal NativeIconReader(GameReader reader) : this(
        (address, length) => reader.ReadBytes(new IntPtr(address), length), reader.ModuleBase.ToInt64()) { }

    internal NativeIconReader(Func<long, int, byte[]?> read, long moduleBase)
    {
        _read = read;
        _global = moduleBase + 0x06B57788;
    }

    private static bool Pointer(long value) => value >= 0x10000 && value < 0x800000000000 && (value & 7) == 0;
    private long ReadPointer(long address)
    {
        var bytes = _read(address, 8);
        return bytes is { Length: 8 } ? BitConverter.ToInt64(bytes) : 0;
    }

    internal NativeIconReference? Resolve(ulong handle)
    {
        if (handle == 0) return new(handle, "", "");
        long registry = ReadPointer(_global);
        if (!Pointer(registry)) return null;
        var header = _read(registry, 0x48);
        if (header is not { Length: 0x48 }) return null;
        long head = BitConverter.ToInt64(header, 0x10), buckets = BitConverter.ToInt64(header, 0x20);
        ulong mask = BitConverter.ToUInt64(header, 0x38), count = BitConverter.ToUInt64(header, 0x40);
        if (!Pointer(head) || !Pointer(buckets) || mask >= 1_048_576 || count != mask + 1 || (count & (count - 1)) != 0) return null;
        long bucket = buckets + (long)(handle & mask) * 0x10;
        var limits = _read(bucket, 0x10);
        if (limits is not { Length: 0x10 }) return null;
        long stop = BitConverter.ToInt64(limits), node = BitConverter.ToInt64(limits, 8);
        var visited = new HashSet<long>();
        NativeIconReference? result = node == head ? new(handle, "", "") : null;
        for (int index = 0; node != head && index < 256; index++)
        {
            if (!Pointer(node) || !visited.Add(node)) return null;
            var entry = _read(node, 0x20);
            if (entry is not { Length: 0x20 }) return null;
            if (BitConverter.ToUInt64(entry, 0x10) == handle)
            {
                long text = BitConverter.ToInt64(entry, 0x18);
                byte[]? bytes = ReadCString(text);
                var after = _read(node + 0x10, 0x10);
                if (bytes is null || after is null || !entry.AsSpan(0x10, 0x10).SequenceEqual(after)) return null;
                string? asset;
                try { asset = StrictUtf8.GetString(bytes); }
                catch (DecoderFallbackException) { asset = null; }
                result = new(handle, asset, Convert.ToHexString(bytes));
                break;
            }
            if (node == stop) { result = new(handle, "", ""); break; }
            node = BitConverter.ToInt64(entry, 8);
        }
        var headerAfter = _read(registry, 0x48);
        var limitsAfter = _read(bucket, 0x10);
        if (ReadPointer(_global) != registry || headerAfter is not { Length: 0x48 } ||
            !header.AsSpan(0x10).SequenceEqual(headerAfter.AsSpan(0x10)) ||
            limitsAfter is null || !limits.AsSpan().SequenceEqual(limitsAfter)) return null;
        return result;
    }

    private byte[]? ReadCString(long address)
    {
        if (address < 0x10000 || address >= 0x800000000000) return null;
        var result = new List<byte>();
        for (int offset = 0; offset < 1024;)
        {
            int length = Math.Min(32, Math.Min(1024 - offset, 0x1000 - (int)((address + offset) & 0xFFF)));
            var chunk = _read(address + offset, length);
            if (chunk is null || chunk.Length != length) return null;
            int end = Array.IndexOf(chunk, (byte)0);
            if (end >= 0) { result.AddRange(chunk.Take(end)); return result.ToArray(); }
            result.AddRange(chunk);
            offset += length;
        }
        return null;
    }
}
