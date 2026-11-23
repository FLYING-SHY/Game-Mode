using System;
using System.Collections.Generic;
using System.IO;

namespace Bf6Esp;

/// <summary>PE 节表项。</summary>
internal sealed class PeSection
{
	public string Name;

	public uint Rva;

	public uint VirtualSize;

	public uint RawSize;

	public uint RawOffset;

	/// <summary>该节在内存中的实际占用长度（虚拟大小优先，0 时退回原始大小）。</summary>
	public uint SizeInMemory
	{
		get { return (VirtualSize != 0U) ? VirtualSize : RawSize; }
	}

	public override string ToString()
	{
		return Name + " rva=0x" + Rva.ToString("X") + " vsize=0x" + VirtualSize.ToString("X") + " raw=0x" + RawSize.ToString("X");
	}
}

/// <summary>
/// 解析远端模块（或磁盘文件）的 PE 头与节表。
/// 优先从进程内存读取；头部页读不到时回退到磁盘上的同名文件。
/// </summary>
internal sealed class PeImage
{
	private const int HeaderProbeSize = 0x1000;

	public ulong ImageBase { get; private set; }

	public ushort Machine { get; private set; }

	public bool IsPE32Plus { get; private set; }

	public uint SizeOfImage { get; private set; }

	public ushort SectionCount { get; private set; }

	public List<PeSection> Sections { get; private set; }

	private PeImage()
	{
		Sections = new List<PeSection>();
	}

	/// <summary>从目标进程内存解析模块头。</summary>
	public static PeImage FromMemory(MemoryReader reader, ulong moduleBase)
	{
		byte[] header = reader.ReadBytes(moduleBase, HeaderProbeSize);
		PeImage image = Parse(header, true);
		if (image == null)
		{
			return null;
		}
		return image;
	}

	/// <summary>从磁盘上的可执行文件解析模块头（内存头读不到时的回退路径）。</summary>
	public static PeImage FromFile(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return null;
		}
		try
		{
			byte[] header;
			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				int size = (int)Math.Min(HeaderProbeSize, stream.Length);
				header = new byte[size];
				int done = 0;
				while (done < size)
				{
					int read = stream.Read(header, done, size - done);
					if (read <= 0)
					{
						break;
					}
					done += read;
				}
				if (done < size)
				{
					return null;
				}
			}
			return Parse(header, false);
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
	}

	private static PeImage Parse(byte[] header, bool fromMemory)
	{
		if (header == null || header.Length < 0x100)
		{
			return null;
		}
		if (header[0] != 0x4D || header[1] != 0x5A)
		{
			return null;
		}
		int lfanew = BitConverter.ToInt32(header, 0x3C);
		if (lfanew <= 0 || lfanew + 24 >= header.Length)
		{
			return null;
		}
		if (header[lfanew] != 0x50 || header[lfanew + 1] != 0x45 || header[lfanew + 2] != 0x00 || header[lfanew + 3] != 0x00)
		{
			return null;
		}
		int coff = lfanew + 4;
		ushort machine = BitConverter.ToUInt16(header, coff);
		ushort sectionCount = BitConverter.ToUInt16(header, coff + 2);
		ushort optionalSize = BitConverter.ToUInt16(header, coff + 16);
		int optional = coff + 20;
		if (optional + 64 > header.Length)
		{
			return null;
		}
		ushort magic = BitConverter.ToUInt16(header, optional);
		bool plus = (magic == 0x20B);
		if (!plus && magic != 0x10B)
		{
			return null;
		}
		PeImage image = new PeImage
		{
			Machine = machine,
			IsPE32Plus = plus,
			SectionCount = sectionCount,
			ImageBase = plus ? BitConverter.ToUInt64(header, optional + 24) : (ulong)BitConverter.ToUInt32(header, optional + 28),
			SizeOfImage = BitConverter.ToUInt32(header, optional + 56)
		};
		int sectionTable = optional + optionalSize;
		for (int i = 0; i < sectionCount; i++)
		{
			int entry = sectionTable + i * 40;
			if (entry + 40 > header.Length)
			{
				break;
			}
			string name = ReadName(header, entry);
			PeSection section = new PeSection
			{
				Name = name,
				VirtualSize = BitConverter.ToUInt32(header, entry + 8),
				Rva = BitConverter.ToUInt32(header, entry + 12),
				RawSize = BitConverter.ToUInt32(header, entry + 16),
				RawOffset = BitConverter.ToUInt32(header, entry + 20)
			};
			image.Sections.Add(section);
		}
		return image;
	}

	private static string ReadName(byte[] buffer, int offset)
	{
		int length = 0;
		while (length < 8 && buffer[offset + length] != 0)
		{
			length++;
		}
		return System.Text.Encoding.ASCII.GetString(buffer, offset, length);
	}

	public PeSection FindSection(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (PeSection section in Sections)
		{
			if (string.Equals(section.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				return section;
			}
		}
		return null;
	}

	public bool TryGetSection(string name, out PeSection section)
	{
		section = FindSection(name);
		return section != null;
	}

	/// <summary>节表文本形式，用于校准报告。</summary>
	public string DescribeSections()
	{
		System.Text.StringBuilder builder = new System.Text.StringBuilder();
		foreach (PeSection section in Sections)
		{
			builder.AppendLine(section.ToString());
		}
		return builder.ToString();
	}
}
