using System;
using System.Collections.Generic;

namespace Bf6Esp;

/// <summary>一次 AOB 命中的结果。</summary>
internal sealed class SignatureHit
{
	/// <summary>相对模块基址的 RVA。</summary>
	public ulong Rva;

	/// <summary>绝对地址（模块基址 + RVA）。</summary>
	public ulong Address;

	/// <summary>命中序号（同一签名第几个命中，从 0 开始）。</summary>
	public int Index;

	public override string ToString()
	{
		return "rva=0x" + Rva.ToString("X") + " (0x" + Address.ToString("X") + ")";
	}
}

/// <summary>
/// AOB 特征扫描器。支持 "48 8B ?? 48 85 C0" 形式的模式串，
/// 其中 "??" / "?" 为通配，"4?" / "?5" 为半字节通配。
/// </summary>
internal sealed class SignatureScanner
{
	/// <summary>单节扫描上限（64MB），避免一次申请过大的缓冲区。</summary>
	private const int MaxScanBytes = 64 * 1024 * 1024;

	/// <summary>单次 ReadProcessMemory 的块大小（1MB）。</summary>
	private const int ChunkBytes = 0x100000;

	private readonly byte[] pattern;

	private readonly byte[] mask;

	private readonly int patternLength;

	private SignatureScanner(byte[] pattern, byte[] mask)
	{
		this.pattern = pattern;
		this.mask = mask;
		patternLength = pattern.Length;
	}

	/// <summary>解析模式串；格式非法时返回 null。</summary>
	public static SignatureScanner Parse(string text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		List<byte> values = new List<byte>();
		List<byte> masks = new List<byte>();
		string[] tokens = text.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string raw in tokens)
		{
			string token = raw.Trim();
			if (token.Length == 0)
			{
				continue;
			}
			if (token.Length > 2)
			{
				return null;
			}
			if (token[0] == '?')
			{
				byte low = 0;
				if (token.Length == 2 && token[1] != '?')
				{
					if (!TryHex(token[1], out low))
					{
						return null;
					}
				}
				values.Add(low);
				masks.Add(0x0F);
				continue;
			}
			byte high = 0;
			if (!TryHex(token[0], out high))
			{
				return null;
			}
			if (token.Length == 2)
			{
				if (token[1] == '?')
				{
					values.Add(high);
					masks.Add(0xF0);
					continue;
				}
				byte low = 0;
				if (!TryHex(token[1], out low))
				{
					return null;
				}
				values.Add((byte)((high << 4) | low));
				masks.Add(0xFF);
				continue;
			}
			values.Add(high);
			masks.Add(0x0F);
		}
		if (values.Count == 0)
		{
			return null;
		}
		return new SignatureScanner(values.ToArray(), masks.ToArray());
	}

	private static bool TryHex(char c, out byte value)
	{
		if (c >= '0' && c <= '9')
		{
			value = (byte)(c - '0');
			return true;
		}
		if (c >= 'A' && c <= 'F')
		{
			value = (byte)(c - 'A' + 10);
			return true;
		}
		if (c >= 'a' && c <= 'f')
		{
			value = (byte)(c - 'a' + 10);
			return true;
		}
		value = 0;
		return false;
	}

	public int Length
	{
		get { return patternLength; }
	}

	/// <summary>在给定缓冲区中扫描；返回命中的绝对地址列表。</summary>
	public List<ulong> ScanBuffer(byte[] buffer, ulong bufferBase)
	{
		List<ulong> results = new List<ulong>();
		if (buffer == null || buffer.Length < patternLength)
		{
			return results;
		}
		int limit = buffer.Length - patternLength;
		for (int i = 0; i <= limit; i++)
		{
			if (Matches(buffer, i))
			{
				results.Add(bufferBase + (ulong)i);
			}
		}
		return results;
	}

	private bool Matches(byte[] buffer, int offset)
	{
		for (int i = 0; i < patternLength; i++)
		{
			if ((buffer[offset + i] & mask[i]) != (pattern[i] & mask[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 在目标进程中扫描。sectionName 为空时扫描整个模块。
	/// 返回命中列表，同时输出实际扫描的字节数。
	/// </summary>
	public List<SignatureHit> Scan(MemoryReader reader, ulong moduleBase, PeImage image, string sectionName, out long bytesScanned)
	{
		List<SignatureHit> hits = new List<SignatureHit>();
		bytesScanned = 0L;
		if (reader == null || image == null)
		{
			return hits;
		}
		List<PeSection> targets = new List<PeSection>();
		if (string.IsNullOrEmpty(sectionName))
		{
			targets.AddRange(image.Sections);
		}
		else
		{
			PeSection section = image.FindSection(sectionName);
			if (section == null)
			{
				return hits;
			}
			targets.Add(section);
		}
		foreach (PeSection section in targets)
		{
			uint size = section.SizeInMemory;
			if (size == 0U)
			{
				continue;
			}
			int limit = (int)Math.Min(size, (uint)MaxScanBytes);
			byte[] buffer = new byte[limit];
			int done = 0;
			while (done < limit)
			{
				int chunk = Math.Min(ChunkBytes, limit - done);
				if (!reader.ReadBytes(moduleBase + section.Rva + (ulong)done, buffer, done, chunk))
				{
					break;
				}
				done += chunk;
			}
			if (done == 0)
			{
				continue;
			}
			bytesScanned += done;
			List<ulong> matches = ScanBuffer(buffer, moduleBase + section.Rva);
			foreach (ulong address in matches)
			{
				hits.Add(new SignatureHit
				{
					Address = address,
					Rva = address - moduleBase,
					Index = hits.Count
				});
			}
		}
		return hits;
	}
}
