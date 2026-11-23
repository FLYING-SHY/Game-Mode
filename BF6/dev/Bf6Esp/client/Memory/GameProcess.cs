using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Bf6Esp;

/// <summary>
/// 定位 Battlefield 6 客户端进程，并取得主模块（bf6.exe）的基址与大小。
/// 只使用只读手段：进程枚举 + Toolhelp 模块快照。
/// </summary>
internal sealed class GameProcess : IDisposable
{
	/// <summary>可接受的进程名（不含扩展名）：正式版与试玩版。</summary>
	internal static readonly string[] ProcessNames = { "bf6", "bf6Trial" };

	private ProcessHandle handle;

	public bool IsRunning { get; private set; }

	public int ProcessId { get; private set; }

	public ulong ModuleBase { get; private set; }

	public uint ModuleSize { get; private set; }

	public string ExePath { get; private set; }

	public DateTime Started { get; private set; }

	public string DisplayName { get; private set; }

	private GameProcess()
	{
	}

	/// <summary>附加到正在运行的游戏进程；未运行或附加失败时返回 null。</summary>
	internal static GameProcess Attach()
	{
		Process[] processes = null;
		try
		{
			foreach (string name in ProcessNames)
			{
				processes = Process.GetProcessesByName(name);
				if (processes.Length > 0)
				{
					GameProcess result = Attach(processes[0], name);
					if (result != null)
					{
						return result;
					}
				}
				processes = null;
			}
		}
		catch (InvalidOperationException)
		{
		}
		catch (System.ComponentModel.Win32Exception)
		{
		}
		finally
		{
			DisposeProcesses(processes);
		}
		return null;
	}

	private static GameProcess Attach(Process process, string name)
	{
		try
		{
			string path = process.MainModule.FileName;
			DateTime started = process.StartTime;
			ulong moduleBase = 0UL;
			uint moduleSize = 0U;
			if (!TryReadModule(process.Id, path, out moduleBase, out moduleSize))
			{
				return null;
			}
			GameProcess gameProcess = new GameProcess
			{
				IsRunning = true,
				ProcessId = process.Id,
				ModuleBase = moduleBase,
				ModuleSize = moduleSize,
				ExePath = path,
				Started = started,
				DisplayName = Path.GetFileName(path)
			};
			gameProcess.handle = OpenReadOnly(process.Id);
			return gameProcess;
		}
		catch (InvalidOperationException)
		{
			return null;
		}
		catch (System.ComponentModel.Win32Exception)
		{
			return null;
		}
	}

	/// <summary>用 Toolhelp 模块快照取主模块基址与大小（失败时回退到 Process.MainModule）。</summary>
	private static bool TryReadModule(int processId, string exePath, out ulong moduleBase, out uint moduleSize)
	{
		moduleBase = 0UL;
		moduleSize = 0U;
		IntPtr snapshot = NativeMemory.CreateToolhelp32Snapshot(NativeMemory.Th32csSnapModule | NativeMemory.Th32csSnapModule32, (uint)processId);
		if (snapshot == new IntPtr(-1))
		{
			return false;
		}
		try
		{
			NativeMemory.ModuleEntry32 entry = new NativeMemory.ModuleEntry32();
			entry.Size = (uint)MarshalSize();
			ulong fallbackBase = 0UL;
			uint fallbackSize = 0U;
			if (NativeMemory.Module32First(snapshot, ref entry))
			{
				do
				{
					ulong candidate = (ulong)entry.BaseAddress.ToInt64();
					uint size = entry.BaseSize;
					if (!string.IsNullOrEmpty(exePath) && string.Equals(entry.ExePath, exePath, StringComparison.OrdinalIgnoreCase))
					{
						moduleBase = candidate;
						moduleSize = size;
						return true;
					}
					if (size > fallbackSize)
					{
						fallbackBase = candidate;
						fallbackSize = size;
					}
				}
				while (NativeMemory.Module32Next(snapshot, ref entry));
			}
			if (fallbackSize > 0U)
			{
				moduleBase = fallbackBase;
				moduleSize = fallbackSize;
				return true;
			}
		}
		finally
		{
			NativeMemory.CloseHandle(snapshot);
		}
		return false;
	}

	private static int MarshalSize()
	{
		return System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMemory.ModuleEntry32));
	}

	/// <summary>以只读方式打开自身进程（自检用；不是游戏进程，仅用于验证读取/定位代码路径）。</summary>
	internal static GameProcess OpenSelf()
	{
		try
		{
			Process current = Process.GetCurrentProcess();
			ProcessModule module = current.MainModule;
			GameProcess result = new GameProcess
			{
				IsRunning = true,
				ProcessId = current.Id,
				ModuleBase = (ulong)module.BaseAddress.ToInt64(),
				ModuleSize = (uint)module.ModuleMemorySize,
				ExePath = module.FileName,
				Started = current.StartTime,
				DisplayName = Path.GetFileName(module.FileName)
			};
			result.handle = OpenReadOnly(current.Id);
			return result;
		}
		catch (InvalidOperationException)
		{
			return null;
		}
		catch (System.ComponentModel.Win32Exception)
		{
			return null;
		}
	}

	/// <summary>只读方式打开进程句柄；只申请 PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION。</summary>
	internal static ProcessHandle OpenReadOnly(int processId)
	{
		IntPtr raw = NativeMemory.OpenProcess(NativeMemory.ProcessVmRead | NativeMemory.ProcessQueryLimitedInformation, false, processId);
		if (raw == IntPtr.Zero)
		{
			return null;
		}
		ProcessHandle result = new ProcessHandle();
		result.Attach(raw);
		return result;
	}

	internal bool IsHandleValid
	{
		get { return handle != null && !handle.IsInvalid && !handle.IsClosed; }
	}

	internal ProcessHandle Handle
	{
		get { return handle; }
	}

	public void Dispose()
	{
		if (handle != null)
		{
			handle.Dispose();
			handle = null;
		}
	}

	private static void DisposeProcesses(Process[] processes)
	{
		if (processes == null)
		{
			return;
		}
		foreach (Process process in processes)
		{
			process.Dispose();
		}
	}
}
