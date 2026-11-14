using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;

namespace CoH3Minimap;

// Read-only, exact-path ScenariosMP.sga v10 reader. Caller owns each returned Bitmap.
// Port of sga_extract.py and descriptor-aware rrtex_indexed.py, not the legacy resync parser.
internal static class NativeArchiveAssets
{
    private const int MaxTocBytes = 64 * 1024 * 1024;
    private const int MaxAssetBytes = 128 * 1024 * 1024;
    private const int MaxDimension = 8192;
    private static readonly ConcurrentDictionary<string, Lazy<ArchiveIndex>> Indexes = new(StringComparer.OrdinalIgnoreCase);

    internal static Bitmap? TryLoad(string gameDirectory, string nativeAssetPath)
        => TryLoad(gameDirectory, nativeAssetPath, out _);

    internal static Bitmap? TryLoad(string gameDirectory, string nativeAssetPath, out string? error)
    {
        error = null;
        try
        {
            byte[] rrtex = ReadAsset(gameDirectory, nativeAssetPath);
            return DecodeBitmap(rrtex);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or ArgumentException or
                                   NotSupportedException or OverflowException or UnauthorizedAccessException)
        {
            error = ex.Message;
            return null;
        }
    }

    // Internal seams are also used by the standalone real-archive verification harness.
    internal static byte[] ReadAsset(string gameDirectory, string nativeAssetPath)
    {
        string key = Normalize(nativeAssetPath);
        if (string.IsNullOrWhiteSpace(gameDirectory)) throw new ArgumentException("Game directory is empty.");
        string archive = Path.GetFullPath(Path.Combine(gameDirectory, "anvil", "archives", "ScenariosMP.sga"));
        var info = new FileInfo(archive);
        if (!info.Exists) throw new FileNotFoundException("ScenariosMP.sga was not found.", archive);
        // Invalidate on Steam archive replacement; keep the cache per physical archive generation.
        string generation = archive + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks;
        Lazy<ArchiveIndex> lazy = Indexes.GetOrAdd(generation,
            _ => new Lazy<ArchiveIndex>(() => ArchiveIndex.Open(archive), LazyThreadSafetyMode.ExecutionAndPublication));
        ArchiveIndex index;
        try { index = lazy.Value; }
        catch { Indexes.TryRemove(generation, out _); throw; }
        if (!index.Files.TryGetValue(key, out Entry entry))
            throw new FileNotFoundException("Exact native asset path is absent from ScenariosMP.sga: " + key);
        using var stream = OpenRead(archive);
        if (stream.Length != index.ArchiveLength) throw new InvalidDataException("Archive changed after indexing.");
        stream.Position = checked(index.DataOffset + entry.Offset);
        byte[] stored = new byte[entry.Stored];
        stream.ReadExactly(stored);
        if (entry.Verification == 4 && Crc32(stored) != entry.Crc)
            throw new InvalidDataException("SGA stored-entry CRC32 mismatch.");
        if ((entry.Flags >> 4) != 0) throw new NotSupportedException("Encrypted SGA entries are unsupported.");
        if ((entry.Flags & 15) == 0)
        {
            if (entry.Stored != entry.Raw) throw new InvalidDataException("Raw SGA entry length mismatch.");
            return stored;
        }
        // The proven SGA parser concatenates independent zlib members for compressed entries.
        using var input = new MemoryStream(stored, false);
        using var output = new MemoryStream(entry.Raw);
        while (input.Position < input.Length && output.Length < entry.Raw)
        {
            long before = input.Position;
            int outputBefore = checked((int)output.Length);
            using (var exact = new ExactReadStream(input))
            using (var zlib = new ZLibStream(exact, CompressionMode.Decompress, true))
                CopyBounded(zlib, output, entry.Raw);
            if (input.Position <= before) throw new InvalidDataException("SGA zlib member made no progress.");
            ValidateZlib(stored.AsSpan((int)before, checked((int)(input.Position - before))),
                output.GetBuffer().AsSpan(outputBefore, checked((int)output.Length - outputBefore)));
        }
        if (output.Length != entry.Raw || input.Position != input.Length)
            throw new InvalidDataException("SGA decompressed length or member boundary mismatch.");
        return output.ToArray();
    }

    internal static Bitmap DecodeBitmap(byte[] rrtex)
    {
        Texture texture = ParseTexture(rrtex);
        ColorRgba32[] colors = new BcDecoder().DecodeRaw(texture.Blocks, texture.Width, texture.Height, CompressionFormat.Bc7);
        if (colors.Length != checked(texture.Width * texture.Height))
            throw new InvalidDataException("BC7 decoder returned an unexpected pixel count.");
        var bitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format32bppArgb);
        try
        {
            BitmapData bits = bitmap.LockBits(new Rectangle(0, 0, texture.Width, texture.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte[] row = new byte[checked(texture.Width * 4)];
                for (int y = 0; y < texture.Height; y++)
                {
                    for (int x = 0; x < texture.Width; x++)
                    {
                        ColorRgba32 c = colors[y * texture.Width + x];
                        int p = x * 4;
                        row[p] = c.b; row[p + 1] = c.g; row[p + 2] = c.r; row[p + 3] = c.a;
                    }
                    Marshal.Copy(row, 0, IntPtr.Add(bits.Scan0, y * bits.Stride), row.Length);
                }
            }
            finally { bitmap.UnlockBits(bits); }
            return bitmap;
        }
        catch { bitmap.Dispose(); throw; }
    }

    internal readonly record struct Texture(int Width, int Height, byte[] Blocks, int SegmentCount, int RawSegments);

    internal static Texture ParseTexture(byte[] blob)
    {
        if (blob.Length < 24 || !blob.AsSpan(0, 16).SequenceEqual("Relic Chunky\r\n\u001a\0"u8) || U32(blob, 16) != 4)
            throw new InvalidDataException("Expected Relic Chunky v4.");
        var chunks = new Dictionary<string, (int Start, int Length)>(StringComparer.Ordinal);
        WalkChunks(blob, 24, blob.Length, 0, chunks);
        if (!chunks.TryGetValue("TMAN", out var tm) || !chunks.TryGetValue("TDAT", out var td))
            throw new InvalidDataException("Exactly one TMAN and TDAT are required.");
        ReadOnlySpan<byte> tman = blob.AsSpan(tm.Start, tm.Length);
        ReadOnlySpan<byte> tdat = blob.AsSpan(td.Start, td.Length);
        if (tman.Length < 37 || tdat.Length < 4) throw new InvalidDataException("Truncated TMAN/TDAT.");
        uint format = U32(tman, 0);
        if (format != 6) throw new NotSupportedException($"Unverified rrtex format {format}; only observed format 6 maps to BC7.");
        int width = Size(U32(tman, 4), MaxDimension, "texture width");
        int height = Size(U32(tman, 8), MaxDimension, "texture height");
        if (width == 0 || height == 0 || (long)width * height > 16 * 1024 * 1024)
            throw new InvalidDataException("Invalid or excessive texture dimensions.");
        // Both marker 1 maps and marker 0 UI resources have been observed with one mip.
        if (tman[28] > 1 || U32(tman, 29) != 1) throw new NotSupportedException("Unverified marker or mip layout.");
        int count = Size(U32(tman, 33), 65536, "segment count");
        if (count == 0 || tman.Length != checked(37 + count * 8) || U32(tdat, 0) != format)
            throw new InvalidDataException("TMAN table size or TDAT codec mismatch.");
        int expected = checked(((width + 3) / 4) * ((height + 3) / 4) * 16);
        int decodedLength = checked(expected + 16);
        byte[] decoded = new byte[decodedLength];
        int source = 4, destination = 0, rawSegments = 0;
        for (int i = 0; i < count; i++)
        {
            int raw = Size(U32(tman, 37 + i * 8), decodedLength, "segment decoded size");
            int stored = Size(U32(tman, 41 + i * 8), decodedLength, "segment stored size");
            if (raw == 0 || stored == 0 || stored > raw || (long)source + stored > tdat.Length ||
                (long)destination + raw > decoded.Length)
                throw new InvalidDataException("Invalid TMAN segment boundary or size.");
            if (stored == raw)
            {
                tdat.Slice(source, stored).CopyTo(decoded.AsSpan(destination, raw));
                rawSegments++;
            }
            else
            {
                using var input = new MemoryStream(blob, td.Start + source, stored, false);
                using var exact = new ExactReadStream(input, bulkPrefix: true);
                using (var zlib = new ZLibStream(exact, CompressionMode.Decompress, true))
                {
                    zlib.ReadExactly(decoded.AsSpan(destination, raw));
                    if (zlib.ReadByte() != -1) throw new InvalidDataException("rrtex segment exceeds its raw length.");
                }
                if (input.Position != input.Length) throw new InvalidDataException("Unconsumed zlib segment bytes.");
                ValidateZlib(tdat.Slice(source, stored), decoded.AsSpan(destination, raw));
            }
            source += stored; destination += raw;
        }
        if (source != tdat.Length || destination != decoded.Length)
            throw new InvalidDataException("Unconsumed TDAT bytes or decoded total mismatch.");
        if (U32(decoded, 0) != 0 || U32(decoded, 4) != width || U32(decoded, 8) != height || U32(decoded, 12) != expected)
            throw new InvalidDataException("Decoded mip header mismatch.");
        return new Texture(width, height, decoded.AsSpan(16).ToArray(), count, rawSegments);
    }

    private static void WalkChunks(byte[] blob, int start, int end, int depth,
        Dictionary<string, (int Start, int Length)> chunks)
    {
        if (depth > 16) throw new InvalidDataException("Excessive Chunky nesting.");
        int p = start;
        while (p < end)
        {
            if (end - p < 20) throw new InvalidDataException("Truncated 20-byte Chunky header.");
            bool folder = blob.AsSpan(p, 4).SequenceEqual("FOLD"u8);
            if (!folder && !blob.AsSpan(p, 4).SequenceEqual("DATA"u8))
                throw new InvalidDataException("Invalid Chunky type; resynchronization is intentionally disabled.");
            int payloadLength = Size(U32(blob, p + 12), MaxAssetBytes, "chunk payload");
            int descriptor = Size(U32(blob, p + 16), MaxAssetBytes, "chunk descriptor");
            long bodyLong = (long)p + 20 + descriptor;
            long limitLong = bodyLong + payloadLength;
            if (limitLong > end) throw new InvalidDataException("Chunk exceeds parent.");
            int body = (int)bodyLong, limit = (int)limitLong;
            string name = Encoding.ASCII.GetString(blob, p + 4, 4);
            if (name is "TMAN" or "TDAT")
            {
                if (folder || !chunks.TryAdd(name, (body, payloadLength)))
                    throw new InvalidDataException("Duplicate or folder-valued " + name + ".");
            }
            if (folder) WalkChunks(blob, body, limit, depth + 1, chunks);
            p = limit;
        }
    }

    private static string Normalize(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > 1024) throw new ArgumentException("Invalid native asset path.");
        string key = path.Replace('/', '\\');
        if (key.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) key = key[5..];
        if (!key.StartsWith("scenarios\\multiplayer\\", StringComparison.OrdinalIgnoreCase) ||
            !key.EndsWith(".rrtex", StringComparison.OrdinalIgnoreCase) || key.IndexOfAny([':', '\0']) >= 0)
            throw new ArgumentException("Only exact multiplayer scenario rrtex paths are accepted.");
        foreach (string part in key.Split('\\'))
            if (part.Length == 0 || part is "." or ".." || part.IndexOfAny(['<', '>', '"', '|', '?', '*']) >= 0)
                throw new ArgumentException("Invalid native asset path component.");
        return key;
    }

    private readonly record struct Entry(long Offset, int Stored, int Raw, byte Verification, byte Flags, uint Crc);

    private sealed class ArchiveIndex
    {
        internal readonly Dictionary<string, Entry> Files = new(StringComparer.OrdinalIgnoreCase);
        internal long DataOffset;
        internal long ArchiveLength;

        internal static ArchiveIndex Open(string path)
        {
            using var stream = OpenRead(path);
            byte[] header = new byte[0x1ac]; stream.ReadExactly(header);
            if (!header.AsSpan(0, 8).SequenceEqual("_ARCHIVE"u8) || U32(header, 8) != 10)
                throw new InvalidDataException("Expected SGA v10 archive.");
            long tocOffset = checked((long)BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(140, 8)));
            int tocLength = Size(U32(header, 148), MaxTocBytes, "SGA TOC");
            long dataOffset = checked((long)BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(152, 8)));
            long dataLength = U32(header, 160);
            if (tocOffset < header.Length || tocLength < 44 || tocOffset > stream.Length - tocLength ||
                dataOffset < header.Length || dataLength > stream.Length - dataOffset ||
                (dataOffset < tocOffset + tocLength && tocOffset < dataOffset + dataLength))
                throw new InvalidDataException("Invalid SGA header boundaries.");
            byte[] toc = new byte[tocLength]; stream.Position = tocOffset; stream.ReadExactly(toc);
            int driveOffset = Size(U32(toc, 0), tocLength, "drive table offset");
            int driveCount = Size(U32(toc, 4), 1024, "drive count");
            int folderOffset = Size(U32(toc, 8), tocLength, "folder table offset");
            int folderCount = Size(U32(toc, 12), 1000000, "folder count");
            int fileOffset = Size(U32(toc, 16), tocLength, "file table offset");
            int fileCount = Size(U32(toc, 20), 1000000, "file count");
            int nameOffset = Size(U32(toc, 24), tocLength, "name table offset");
            int nameLength = Size(U32(toc, 28), tocLength, "name table length");
            CheckTable(driveOffset, driveCount, 148, tocLength);
            CheckTable(folderOffset, folderCount, 20, tocLength);
            CheckTable(fileOffset, fileCount, 30, tocLength);
            CheckTable(nameOffset, nameLength, 1, tocLength);
            if (driveCount != 1 || folderCount == 0) throw new NotSupportedException("Expected one SGA drive and a root folder.");
            var names = new Dictionary<uint, string>();
            var utf8 = new UTF8Encoding(false, true);
            for (int p = 0; p < nameLength;)
            {
                int zero = Array.IndexOf(toc, (byte)0, nameOffset + p, nameLength - p);
                if (zero < 0) throw new InvalidDataException("Unterminated SGA name.");
                names.Add((uint)p, utf8.GetString(toc, nameOffset + p, zero - nameOffset - p));
                p = zero - nameOffset + 1;
            }
            string Name(uint offset) => names.TryGetValue(offset, out string? value) ? value :
                throw new InvalidDataException("SGA name offset is not a string boundary.");
            var index = new ArchiveIndex { DataOffset = dataOffset, ArchiveLength = stream.Length };
            var visited = new HashSet<int>();
            void Visit(int folder, int depth)
            {
                if (depth > 64 || !visited.Add(folder)) throw new InvalidDataException("Cyclic/deep SGA folder tree.");
                int at = folderOffset + folder * 20;
                string prefix = folder == 0 ? "" : Name(U32(toc, at)) + "\\";
                int firstFolder = Size(U32(toc, at + 4), folderCount, "first child folder");
                int endFolder = Size(U32(toc, at + 8), folderCount, "end child folder");
                int firstFile = Size(U32(toc, at + 12), fileCount, "first file");
                int endFile = Size(U32(toc, at + 16), fileCount, "end file");
                if (firstFolder > endFolder || firstFile > endFile) throw new InvalidDataException("Invalid SGA folder range.");
                for (int i = firstFile; i < endFile; i++)
                {
                    int f = fileOffset + i * 30;
                    string key = prefix + Name(U32(toc, f));
                    if (!key.EndsWith(".rrtex", StringComparison.OrdinalIgnoreCase) ||
                        !key.StartsWith("scenarios\\multiplayer\\", StringComparison.OrdinalIgnoreCase)) continue;
                    key = Normalize(key);
                    long relative = checked((long)BinaryPrimitives.ReadUInt64LittleEndian(toc.AsSpan(f + 8, 8)));
                    int stored = Size(U32(toc, f + 16), MaxAssetBytes, "SGA entry stored size");
                    int raw = Size(U32(toc, f + 20), MaxAssetBytes, "SGA entry decoded size");
                    if (stored == 0 || raw == 0 || relative > dataLength - stored)
                        throw new InvalidDataException("SGA file exceeds archive bounds.");
                    var entry = new Entry(relative, stored, raw, toc[f + 24], toc[f + 25], U32(toc, f + 26));
                    if (!index.Files.TryAdd(key, entry)) throw new InvalidDataException("Duplicate SGA native asset path.");
                }
                for (int i = firstFolder; i < endFolder; i++) Visit(i, depth + 1);
            }
            Visit(0, 0);
            return index;
        }
    }

    private static FileStream OpenRead(string path) => new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
    private static uint U32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset, 4));
    private static int Size(uint value, int max, string label) => value <= max ? (int)value :
        throw new InvalidDataException(label + " exceeds the supported bound.");
    private static void CheckTable(int offset, int count, int stride, int total)
    {
        if (offset < 44 || (long)offset + (long)count * stride > total)
            throw new InvalidDataException("SGA TOC table exceeds its boundary.");
    }
    private static void CopyBounded(Stream source, Stream output, int max)
    {
        byte[] buffer = new byte[65536];
        int read;
        while ((read = source.Read(buffer)) != 0)
        {
            if (output.Length + read > max) throw new InvalidDataException("Decompressed SGA exceeds declared size.");
            output.Write(buffer, 0, read);
        }
    }
    private static uint Crc32(ReadOnlySpan<byte> bytes)
    {
        uint crc = 0xffffffff;
        foreach (byte b in bytes)
        {
            crc ^= b;
            for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ (0xedb88320u & (0u - (crc & 1)));
        }
        return ~crc;
    }

    private static void ValidateZlib(ReadOnlySpan<byte> encoded, ReadOnlySpan<byte> decoded)
    {
        if (encoded.Length < 6 || (encoded[0] & 15) != 8 || (encoded[0] >> 4) > 7 ||
            ((encoded[0] << 8) | encoded[1]) % 31 != 0 || (encoded[1] & 32) != 0)
            throw new InvalidDataException("Invalid zlib header or preset dictionary.");
        uint a = 1, b = 0;
        for (int start = 0; start < decoded.Length; start += 5552)
        {
            int end = Math.Min(start + 5552, decoded.Length);
            for (int i = start; i < end; i++) { a += decoded[i]; b += a; }
            a %= 65521; b %= 65521;
        }
        if (BinaryPrimitives.ReadUInt32BigEndian(encoded[^4..]) != ((b << 16) | a))
            throw new InvalidDataException("Zlib Adler32 mismatch or truncated trailer.");
    }

    // For indexed rrtex segments, bulk-read only before the final 64 bytes. The tail is
    // read bytewise so native zlib read-ahead cannot hide trailing bytes. Unknown-length
    // concatenated SGA members use bytewise reads throughout to preserve member boundaries.
    private sealed class ExactReadStream(Stream source, bool bulkPrefix = false) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        private int Limit(int count) => Math.Min(count, bulkPrefix ? (int)Math.Max(1, source.Length - source.Position - 64) : 1);
        public override int Read(byte[] buffer, int offset, int count) => source.Read(buffer, offset, Limit(count));
        public override int Read(Span<byte> buffer) => source.Read(buffer[..Limit(buffer.Length)]);
        public override int ReadByte() => source.ReadByte();
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
