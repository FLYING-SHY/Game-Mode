using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Bf6Esp;

/// <summary>
/// 只读内存读取器。所有访问都走 ReadProcessMemory。
///
/// 这个类刻意不提供任何写入方法（没有 Write/WriteBytes/Patch 之类），
/// 以保证"外部只读"这一设计约束在代码层面可被静态检查。
/// </summary>
internal sealed class MemoryReader
{
	private const int MaxAttempts = 3;

	private readonly ProcessHandle handle;

	private readonly bool allowUnlicensedReads;

	private int readFailures;

	/// <summary>
	/// 普通模式：读取受授权状态约束（未授权一律失败）。
	/// </summary>
	internal MemoryReader(ProcessHandle handle)
		: this(handle, false)
	{
	}

	/// <summary>
	/// 诊断模式（--probe / --calibrate）：允许在未授权时读取，用于定位偏移。
	/// 注意：这里只放开"是否允许读"，句柄本身仍然只申请了
	/// PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION，
	/// 不存在任何写入/注入通道。
	/// </summary>
	internal MemoryReader(ProcessHandle handle, bool allowUnlicensedReads)
	{
		this.handle = handle;
		this.allowUnlicensedReads = allowUnlicensedReads;
	}

	/// <summary>累计读取失败次数（地址无效、权限不足、进程退出等）。</summary>
	public int ReadFailures
	{
		get { return Volatile.Read(ref readFailures); }
	}

	public bool IsValid
	{
		get { return handle != null && !handle.IsInvalid && !handle.IsClosed; }
	}

	/// <summary>读取一段字节到调用方缓冲区；返回是否读满 count 个字节。</summary>
	public bool ReadBytes(ulong address, byte[] buffer, int offset, int count)
	{
		if (buffer == null || count <= 0 || offset < 0 || offset + count > buffer.Length)
		{
			return false;
		}
		if (!IsValid)
		{
			Interlocked.Increment(ref readFailures);
			return false;
		}
		if (!allowUnlicensedReads && !LicenseGate60.Allowed)
		{
			Interlocked.Increment(ref readFailures);
			return false;
		}
		int done = 0;
		for (int attempt = 0; attempt < MaxAttempts && done < count; attempt++)
		{
			UIntPtr read;
			bool ok = NativeMemory.ReadProcessMemory(handle.DangerousGetHandle(), new IntPtr((long)address), buffer, new UIntPtr((uint)(count - done)), out read);
			int produced = (int)read.ToUInt64();
			if (produced > 0)
			{
				done += produced;
				address += (ulong)produced;
				offset += produced;
			}
			if (!ok && produced == 0)
			{
				break;
			}
		}
		if (done < count)
		{
			Interlocked.Increment(ref readFailures);
			return false;
		}
		return true;
	}

	/// <summary>读取一段字节；失败返回 null。</summary>
	public byte[] ReadBytes(ulong address, int count)
	{
		if (count <= 0 || count > 0x100000)
		{
			return null;
		}
		byte[] buffer = new byte[count];
		if (!ReadBytes(address, buffer, 0, count))
		{
			return null;
		}
		return buffer;
	}

	/// <summary>读取一个 POD 结构体；失败抛出 InvalidOperationException。</summary>
	public T Read<T>(ulong address) where T : struct
	{
		T value;
		if (!TryRead(address, out value))
		{
			throw new InvalidOperationException("读取内存失败: 0x" + address.ToString("X"));
		}
		return value;
	}

	/// <summary>读取一个 POD 结构体；失败返回 false。</summary>
	public bool TryRead<T>(ulong address, out T value) where T : struct
	{
		int size = Marshal.SizeOf(typeof(T));
		byte[] buffer = new byte[size];
		value = default(T);
		if (!ReadBytes(address, buffer, 0, size))
		{
			return false;
		}
		GCHandle pinned = GCHandle.Alloc(buffer, GCHandleType.Pinned);
		try
		{
			value = (T)Marshal.PtrToStructure(pinned.AddrOfPinnedObject(), typeof(T));
		}
		finally
		{
			pinned.Free();
		}
		return true;
	}

	public float ReadFloat(ulong address)
	{
		float value;
		if (!TryRead(address, out value))
		{
			throw new InvalidOperationException("读取内存失败: 0x" + address.ToString("X"));
		}
		return value;
	}

	public int ReadInt(ulong address)
	{
		int value;
		if (!TryRead(address, out value))
		{
			throw new InvalidOperationException("读取内存失败: 0x" + address.ToString("X"));
		}
		return value;
	}

	public ulong ReadUInt64(ulong address)
	{
		ulong value;
		if (!TryRead(address, out value))
		{
			throw new InvalidOperationException("读取内存失败: 0x" + address.ToString("X"));
		}
		return value;
	}
}
