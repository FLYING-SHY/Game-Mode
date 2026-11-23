using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bf6Esp;

/// <summary>
/// 只读通道自检（--selftest）。
/// 不需要游戏在运行：用与正式流程完全相同的「只读句柄 + ReadProcessMemory」路径，
/// 读取自身进程的主模块 PE 头，并把一段已知内容的托管缓冲区读回来做逐字节比对。
/// 目的是验证「读取通道」本身可用，而不是验证游戏偏移。
/// </summary>
internal static class SelfTestRunner60
{
	internal static int Run()
	{
		ConsoleBridge60.Attach();
		ConsoleBridge60.WriteLine("=== Bf6Esp 只读通道自检 ===");
		int failures = 0;

		Process current = null;
		try
		{
			current = Process.GetCurrentProcess();
			ProcessModule module = current.MainModule;
			ulong moduleBase = (ulong)module.BaseAddress.ToInt64();
			uint moduleSize = (uint)module.ModuleMemorySize;
			ConsoleBridge60.WriteLine("[信息] 目标进程：自身 (pid " + current.Id + ")");
			ConsoleBridge60.WriteLine("[信息] 主模块：" + module.FileName + " 基址=0x" + moduleBase.ToString("X") + " 大小=" + moduleSize + " bytes");

			using (ProcessHandle readHandle = GameProcess.OpenReadOnly(current.Id))
			{
				if (readHandle == null || readHandle.IsInvalid)
				{
					ConsoleBridge60.WriteLine("[失败] 只读打开进程失败（OpenProcess 返回无效句柄）。");
					return 1;
				}
				ConsoleBridge60.WriteLine("[成功] 只读句柄已取得（仅 PROCESS_VM_READ | PROCESS_QUERY_LIMITED_INFORMATION）。");

				MemoryReader reader = new MemoryReader(readHandle, true);

				// 1) DOS 头
				byte[] head = reader.ReadBytes(moduleBase, 64);
				bool mz = head != null && head.Length >= 2 && head[0] == 0x4D && head[1] == 0x5A;
				ConsoleBridge60.WriteLine((mz ? "[成功] " : "[失败] ") + "DOS 头魔数校验（MZ）。");
				if (!mz)
				{
					failures++;
				}

				// 2) PE 头与节表
				PeImage image = PeImage.FromMemory(reader, moduleBase);
				if (image == null)
				{
					ConsoleBridge60.WriteLine("[失败] PE 头解析失败。");
					failures++;
				}
				else
				{
					ConsoleBridge60.WriteLine("[成功] PE 头解析：ImageBase=0x" + image.ImageBase.ToString("X") +
						" SizeOfImage=0x" + image.SizeOfImage.ToString("X") +
						" 节数=" + image.SectionCount + (image.IsPE32Plus ? "（PE32+）" : "（PE32）"));
					foreach (PeSection section in image.Sections)
					{
						ConsoleBridge60.WriteLine("         节 " + section);
					}
				}

				// 3) 已知内容往返：把一段带特征图案的托管缓冲区用 ReadProcessMemory 读回来比对
				byte[] probe = new byte[256];
				for (int i = 0; i < probe.Length; i++)
				{
					probe[i] = (byte)((i * 7) + 3);
				}
				GCHandle pinned = GCHandle.Alloc(probe, GCHandleType.Pinned);
				try
				{
					ulong address = (ulong)(long)pinned.AddrOfPinnedObject();
					byte[] readBack = reader.ReadBytes(address, probe.Length);
					bool same = readBack != null && readBack.Length == probe.Length;
					if (same)
					{
						for (int i = 0; i < probe.Length; i++)
						{
							if (readBack[i] != probe[i])
							{
								same = false;
								break;
							}
						}
					}
					ConsoleBridge60.WriteLine((same ? "[成功] " : "[失败] ") + "已知内容往返比对（地址 0x" + address.ToString("X") + "，256 字节）。");
					if (!same)
					{
						failures++;
					}
				}
				finally
				{
					pinned.Free();
				}

				ConsoleBridge60.WriteLine("[信息] 读取失败计数：" + reader.ReadFailures);
			}
		}
		catch (Exception ex)
		{
			ConsoleBridge60.WriteLine("[失败] 自检异常：" + ex.Message);
			failures++;
		}
		finally
		{
			if (current != null)
			{
				current.Dispose();
			}
		}

		ConsoleBridge60.WriteLine(failures == 0 ? "=== 自检通过：只读通道可用 ===" : ("=== 自检失败项：" + failures + " ==="));
		return failures == 0 ? 0 : 1;
	}
}
