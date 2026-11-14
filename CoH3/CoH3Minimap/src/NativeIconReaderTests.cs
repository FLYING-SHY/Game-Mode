using System.Text;

namespace CoH3Minimap;

internal static class NativeIconReaderTests
{
    internal static void Run()
    {
        const long image = 0x140000000, registry = 0x250000, buckets = 0x260000,
            head = 0x270000, first = 0x280000, last = 0x290000, text = 0x2A0003;
        const ulong handle = 0x123456789ABCDEF5;
        var memory = new Dictionary<long, byte[]>
        {
            [image + 0x06B57788] = new byte[8], [registry] = new byte[0x48],
            [buckets] = new byte[8 * 0x10], [first] = new byte[0x20],
            [last] = new byte[0x20], [text] = new byte[128]
        };
        void U64(long region, int offset, ulong value) => BitConverter.GetBytes(value).CopyTo(memory[region], offset);
        byte[]? Read(long address, int length)
        {
            foreach (var (start, data) in memory)
                if (address >= start && address - start + length <= data.Length)
                    return data.AsSpan((int)(address - start), length).ToArray();
            return null;
        }
        U64(image + 0x06B57788, 0, registry);
        U64(registry, 0x10, head); U64(registry, 0x20, buckets);
        U64(registry, 0x38, 7); U64(registry, 0x40, 8);
        int bucketOffset = (int)(handle & 7) * 0x10;
        U64(buckets, bucketOffset, last); U64(buckets, bucketOffset + 8, first);
        U64(first, 8, last); U64(first, 0x10, handle + 8);
        U64(last, 8, head); U64(last, 0x10, handle); U64(last, 0x18, text);
        const string asset = "/WPFGUI;component/icons/common/units/symbols/test.png";
        Encoding.UTF8.GetBytes(asset).CopyTo(memory[text], 0);
        var reader = new NativeIconReader(Read, image);
        var icon = reader.Resolve(handle);
        if (icon?.AssetPath != asset || icon.RawBytesHex != Convert.ToHexString(Encoding.UTF8.GetBytes(asset)))
            throw new Exception("Native icon collision chain failed, or stop node was not compared before terminating.");
        if (reader.Resolve(handle + 16)?.AssetPath != "") throw new Exception("Missing native icon did not preserve empty result.");
        if (reader.Resolve(0)?.AssetPath != "") throw new Exception("Zero native icon handle did not preserve empty result.");
        U64(first, 8, first);
        if (reader.Resolve(handle) is not null) throw new Exception("Cyclic native icon chain was accepted.");
        U64(first, 8, last);
        memory[text][0] = 0xFF; memory[text][1] = 0;
        icon = reader.Resolve(handle);
        if (icon is null || icon.AssetPath is not null || icon.RawBytesHex != "FF")
            throw new Exception("Unestablished icon encoding silently rewrote native bytes.");
        int globalReads = 0;
        var changed = new NativeIconReader((address, length) =>
        {
            var data = Read(address, length);
            if (address == image + 0x06B57788 && ++globalReads == 2 && data is not null) data[2] ^= 1;
            return data;
        }, image);
        if (changed.Resolve(handle) is not null) throw new Exception("Replaced icon registry was published as stable.");
        U64(registry, 0x40, 9);
        if (reader.Resolve(handle) is not null) throw new Exception("Inconsistent native icon bucket count accepted.");
        Console.WriteLine("PASS: native icon registry, bucket collisions, last-node match, empty handles, cycles, original bytes and registry replacement. Synthetic memory only.");
    }
}
