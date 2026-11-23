using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Bf6Esp;

/// <summary>
/// 反射解析器自检（--selftest-reflect）。
///
/// 用与正式流程完全一致的「只读句柄 + ReadProcessMemory」路径，
/// 扫描一块自己摆好的合成结构（见 ReflectionFixture60），
/// 校验解析器能否把已知的字段名 -> 偏移、类型名原样还原。
/// 这验证的是解析逻辑本身，与游戏无关，因此可以在没有游戏的环境下回归。
/// </summary>
internal static class ReflectionSelfTest60
{
	internal static int Run()
	{
		ConsoleBridge60.Attach();
		ConsoleBridge60.WriteLine("=== Bf6Esp 反射解析器自检（合成夹具） ===");
		int failures = 0;

		Process current = null;
		ReflectionFixture60 fixture = null;
		try
		{
			fixture = ReflectionFixture60.Build();
			ConsoleBridge60.WriteLine("[信息] " + fixture.Describe().TrimEnd());

			current = Process.GetCurrentProcess();
			ProcessModule module = current.MainModule;
			ulong moduleBase = (ulong)module.BaseAddress.ToInt64();

			using (ProcessHandle readHandle = GameProcess.OpenReadOnly(current.Id))
			{
				if (readHandle == null || readHandle.IsInvalid)
				{
					ConsoleBridge60.WriteLine("[失败] 只读打开进程失败。");
					return 1;
				}
				MemoryReader reader = new MemoryReader(readHandle, true);
				PeImage image = PeImage.FromMemory(reader, moduleBase);
				if (image == null)
				{
					ConsoleBridge60.WriteLine("[失败] PE 头解析失败。");
					failures++;
				}

				List<ulong[]> extra = new List<ulong[]>();
				extra.Add(new ulong[] { fixture.BaseAddress, fixture.BaseAddress + fixture.Size });

				ReflectionScanResult60 result = RuntimeReflection60.Scan(reader, moduleBase, image, extra);
				foreach (string note in result.Notes)
				{
					ConsoleBridge60.WriteLine("[信息] " + note);
				}

				Dictionary<string, int> expected = ReflectionFixture60.ExpectedFields();
				foreach (KeyValuePair<string, int> pair in expected)
				{
					int offset;
					bool found = result.TryGetOffset(pair.Key, out offset);
					bool same = found && offset == pair.Value;
					ConsoleBridge60.WriteLine((same ? "[成功] " : "[失败] ") +
						"字段 " + pair.Key + "：期望 0x" + pair.Value.ToString("X", CultureInfo.InvariantCulture) +
						"，实际 " + (found ? ("0x" + offset.ToString("X", CultureInfo.InvariantCulture)) : "(未找到)") + "。");
					if (!same)
					{
						failures++;
					}
				}

				foreach (string typeName in ReflectionFixture60.ExpectedTypes())
				{
					bool found = false;
					foreach (ReflectionType60 type in result.Types)
					{
						if (string.Equals(type.Name, typeName, StringComparison.Ordinal))
						{
							found = true;
							break;
						}
					}
					ConsoleBridge60.WriteLine((found ? "[成功] " : "[失败] ") + "类型 " + typeName + "："
						+ (found ? "已还原" : "未还原") + "。");
					if (!found)
					{
						failures++;
					}
				}

				ConsoleBridge60.WriteLine("[信息] 还原类型数=" + result.Types.Count.ToString(CultureInfo.InvariantCulture) +
					"，字段条目=" + result.Fields.Count.ToString(CultureInfo.InvariantCulture) +
					"，描述符数组=" + result.DescriptorRuns.ToString(CultureInfo.InvariantCulture) +
					"，读取失败计数=" + reader.ReadFailures.ToString(CultureInfo.InvariantCulture));
			}
		}
		catch (Exception ex)
		{
			ConsoleBridge60.WriteLine("[失败] 自检异常：" + ex.Message);
			failures++;
		}
		finally
		{
			if (fixture != null)
			{
				fixture.Dispose();
			}
			if (current != null)
			{
				current.Dispose();
			}
		}

		ConsoleBridge60.WriteLine(failures == 0 ? "=== 反射自检通过 ===" : ("=== 反射自检失败项：" + failures.ToString(CultureInfo.InvariantCulture) + " ==="));
		return failures == 0 ? 0 : 1;
	}
}
