using System;
using System.Collections.Generic;
using System.IO;

namespace CoH3FowTool
{
    /// <summary>
    /// Minimal read-only PE32+ parser: section table, RVA &lt;-&gt; file-offset mapping.
    /// Enough for signature scanning and for translating a file offset back to an RVA
    /// so the same signature can be applied to a live process (base + RVA).
    /// </summary>
    internal sealed class PeImage
    {
        public readonly byte[] Data;
        public readonly ulong ImageBase;
        public readonly List<Section> Sections = new List<Section>();

        public sealed class Section
        {
            public string Name;
            public uint VirtualAddress;
            public uint VirtualSize;
            public uint PointerToRawData;
            public uint SizeOfRawData;
        }

        public PeImage(string path)
        {
            Data = File.ReadAllBytes(path);
            if (Data.Length < 0x200) throw new BadImageFormatException("file too small: " + path);

            int e_lfanew = BitConverter.ToInt32(Data, 0x3C);
            if (BitConverter.ToUInt32(Data, e_lfanew) != 0x00004550)
                throw new BadImageFormatException("missing PE signature: " + path);

            int coff = e_lfanew + 4;
            int numSections = BitConverter.ToUInt16(Data, coff + 2);
            int optSize = BitConverter.ToUInt16(Data, coff + 16);
            int opt = coff + 20;

            ushort magic = BitConverter.ToUInt16(Data, opt);
            if (magic != 0x20B) throw new BadImageFormatException("not PE32+: " + path);

            // PE32+ 的 ImageBase 是 64 位, 位于 OptionalHeader + 0x18
            ImageBase = BitConverter.ToUInt64(Data, opt + 0x18);

            int sh = opt + optSize;
            for (int i = 0; i < numSections; i++, sh += 40)
            {
                var s = new Section
                {
                    Name = System.Text.Encoding.ASCII.GetString(Data, sh, 8).TrimEnd('\0'),
                    VirtualSize = BitConverter.ToUInt32(Data, sh + 8),
                    VirtualAddress = BitConverter.ToUInt32(Data, sh + 12),
                    SizeOfRawData = BitConverter.ToUInt32(Data, sh + 16),
                    PointerToRawData = BitConverter.ToUInt32(Data, sh + 20)
                };
                Sections.Add(s);
            }
        }

        /// <summary>File offset -> RVA. Returns -1 when the offset is not backed by a section.</summary>
        public long OffsetToRva(long offset)
        {
            foreach (var s in Sections)
            {
                long start = s.PointerToRawData;
                long end = start + s.SizeOfRawData;
                if (offset >= start && offset < end)
                    return s.VirtualAddress + (offset - start);
            }
            return -1;
        }

        /// <summary>RVA -> file offset. Returns -1 when the RVA is not backed by raw data.</summary>
        public long RvaToOffset(long rva)
        {
            foreach (var s in Sections)
            {
                long start = s.VirtualAddress;
                long size = Math.Max(s.SizeOfRawData, s.VirtualSize);
                if (rva >= start && rva < start + size)
                {
                    long delta = rva - start;
                    if (delta >= s.SizeOfRawData) return -1;
                    return s.PointerToRawData + delta;
                }
            }
            return -1;
        }

        public string SectionOfRva(long rva)
        {
            foreach (var s in Sections)
            {
                long start = s.VirtualAddress;
                long size = Math.Max(s.SizeOfRawData, s.VirtualSize);
                if (rva >= start && rva < start + size) return s.Name;
            }
            return "?";
        }
    }

    /// <summary>Byte pattern with '?' wildcards, e.g. "0F B6 81 70 06 00 00 C3".</summary>
    internal sealed class Pattern
    {
        public readonly byte[] Bytes;
        public readonly byte[] Mask;   // 1 = must match, 0 = wildcard

        public Pattern(string hex)
        {
            var parts = hex.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            Bytes = new byte[parts.Length];
            Mask = new byte[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "?" || parts[i] == "??")
                {
                    Bytes[i] = 0;
                    Mask[i] = 0;
                }
                else
                {
                    Bytes[i] = Convert.ToByte(parts[i], 16);
                    Mask[i] = 1;
                }
            }
        }

        public List<long> FindAll(byte[] data)
        {
            var hits = new List<long>();
            int n = Bytes.Length;
            for (int i = 0; i + n <= data.Length; i++)
            {
                if (Mask[0] == 1 && data[i] != Bytes[0]) continue;
                bool ok = true;
                for (int j = 1; j < n; j++)
                {
                    if (Mask[j] == 1 && data[i + j] != Bytes[j]) { ok = false; break; }
                }
                if (ok) hits.Add(i);
            }
            return hits;
        }

        public override string ToString() => BitConverter.ToString(Bytes).Replace('-', ' ');
    }
}
