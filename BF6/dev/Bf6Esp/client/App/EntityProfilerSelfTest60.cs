using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Bf6Esp;

/// <summary>
/// 实体字段统计推断器自检（--selftest-entity）。
///
/// 摆一块合成内存：24 个"实体"，按三种布局分别摆：
///   场景 A（内联连续）：实体头里直接放
///     +0x00 姓名指针（指向 ASCII）
///     +0x08 自增 id（int，会干扰队伍/血量判定）
///     +0x10 位置 x/y/z（float，跨实体大幅变化）
///     +0x30 当前血量（float，部分实体掉血）
///     +0x34 最大血量（float，恒定 100）
///     +0x40 队伍 id（int，1/2 两种）
///   场景 B（内联矩阵）：位置换成列主序 4x4 变换矩阵，平移在 [3,7,11]。
///   场景 C（组件指针）：实体头里只有姓名指针 + 两个组件指针 + 队伍 int，
///     位置在变换组件对象的行主序矩阵平移里，血量在血量组件对象里。
/// 然后用与正式流程一致的「只读句柄 + ReadProcessMemory」路径跑推断器，
/// 校验它能否在**完全不知道字段名**的前提下把偏移还原出来。
///
/// 这验证的是推断逻辑本身（level F：fixture），与真实游戏无关。
/// </summary>
internal static class EntityProfilerSelfTest60
{
	private const int EntityCount = 24;

	private const int EntityStride = 0x100;

	private const int NameFieldOffset = 0x00;

	private const int IdFieldOffset = 0x08;

	private const int PositionFieldOffset = 0x10;

	private const int MatrixFieldOffset = 0x50;

	private const int HealthFieldOffset = 0x30;

	private const int MaxHealthFieldOffset = 0x34;

	private const int TeamFieldOffset = 0x40;

	/// <summary>场景 C：变换组件指针槽（实体侧偏移）。</summary>
	private const int TransformPointerOffset = 0x08;

	/// <summary>场景 C：血量组件指针槽（实体侧偏移）。</summary>
	private const int HealthPointerOffset = 0x10;

	/// <summary>场景 C：队伍 id 内联在实体里的偏移。</summary>
	private const int ComponentTeamFieldOffset = 0x18;

	/// <summary>场景 C：变换组件对象里矩阵的起始偏移。</summary>
	private const int ComponentMatrixOffset = 0x00;

	/// <summary>场景 C：变换组件对象里平移分量的字节偏移。</summary>
	private const int ComponentTranslationOffset = 0x30;

	/// <summary>场景 C：血量组件对象里当前/最大血量的字节偏移。</summary>
	private const int ComponentHealthOffset = 0x30;

	/// <summary>夹具布局。</summary>
	private enum LayoutMode
	{
		/// <summary>位置是连续三个 float。</summary>
		Contiguous,

		/// <summary>位置在列主序 4x4 变换矩阵的平移分量里（[3,7,11]）。</summary>
		ColumnMajorMatrix,

		/// <summary>位置/血量都在组件对象里，实体只存组件指针。</summary>
		Components
	}

	/// <summary>一个场景的期望结果。</summary>
	private sealed class Expectation
	{
		public int PositionX;

		public int PositionY;

		public int PositionZ;

		public int PositionComponent = -1;

		public int Health;

		public int HealthComponent = -1;

		public int MaxHealth;

		public int MaxHealthComponent = -1;

		public int Team;

		public int TeamComponent = -1;

		public int NamePointer;

		public int NameComponent = -1;
	}

	internal static int Run()
	{
		ConsoleBridge60.Attach();
		ConsoleBridge60.WriteLine("=== Bf6Esp 实体字段统计推断自检（合成夹具） ===");
		int failures = 0;

		Expectation scenarioA = new Expectation
		{
			PositionX = PositionFieldOffset,
			PositionY = PositionFieldOffset + 4,
			PositionZ = PositionFieldOffset + 8,
			Health = HealthFieldOffset,
			MaxHealth = MaxHealthFieldOffset,
			Team = TeamFieldOffset,
			NamePointer = NameFieldOffset
		};
		Expectation scenarioB = new Expectation
		{
			PositionX = MatrixFieldOffset + 12,
			PositionY = MatrixFieldOffset + 28,
			PositionZ = MatrixFieldOffset + 44,
			Health = HealthFieldOffset,
			MaxHealth = MaxHealthFieldOffset,
			Team = TeamFieldOffset,
			NamePointer = NameFieldOffset
		};
		Expectation scenarioC = new Expectation
		{
			PositionX = ComponentTranslationOffset,
			PositionY = ComponentTranslationOffset + 4,
			PositionZ = ComponentTranslationOffset + 8,
			PositionComponent = TransformPointerOffset,
			Health = ComponentHealthOffset,
			HealthComponent = HealthPointerOffset,
			MaxHealth = ComponentHealthOffset + 4,
			MaxHealthComponent = HealthPointerOffset,
			Team = ComponentTeamFieldOffset,
			NamePointer = NameFieldOffset
		};

		failures += RunScenario(LayoutMode.Contiguous, "A：位置为连续三个 float（内联）", scenarioA);
		failures += RunScenario(LayoutMode.ColumnMajorMatrix, "B：位置在列主序 4x4 变换矩阵内（内联）", scenarioB);
		failures += RunScenario(LayoutMode.Components, "C：位置与血量都在组件对象里（实体只存指针）", scenarioC);

		ConsoleBridge60.WriteLine(failures == 0 ? "=== 实体字段推断自检通过 ===" :
			("=== 实体字段推断自检失败项：" + failures.ToString(CultureInfo.InvariantCulture) + " ==="));
		return failures == 0 ? 0 : 1;
	}

	/// <summary>跑一个布局场景。</summary>
	private static int RunScenario(LayoutMode mode, string label, Expectation expected)
	{
		int failures = 0;
		ConsoleBridge60.WriteLine("--- 场景 " + label + " ---");

		Process current = null;
		IntPtr block = IntPtr.Zero;
		try
		{
			current = Process.GetCurrentProcess();
			ulong baseAddress = BuildFixture(mode, out block);

			using (ProcessHandle readHandle = GameProcess.OpenReadOnly(current.Id))
			{
				if (readHandle == null || readHandle.IsInvalid)
				{
					ConsoleBridge60.WriteLine("[失败] 只读打开进程失败。");
					return 1;
				}
				MemoryReader reader = new MemoryReader(readHandle, true);

				List<ulong[]> regions = ProcessRegions60.Enumerate(readHandle);
				AddressMap60 map = new AddressMap60(regions);

				ulong arrayAddress = baseAddress + (ulong)ArrayAreaOffset;
				EntityFieldProfile60 profile = EntityFieldProfiler60.Profile(reader, arrayAddress, EntityCount, map,
					EntityStride, EntityCount);

				foreach (string note in profile.Notes)
				{
					ConsoleBridge60.WriteLine("[信息] " + note);
				}
				ConsoleBridge60.WriteLine("[信息] 推断结果：位置 0x" + profile.PositionOffsetX.ToString("X", CultureInfo.InvariantCulture) +
					"/0x" + profile.PositionOffsetY.ToString("X", CultureInfo.InvariantCulture) +
					"/0x" + profile.PositionOffsetZ.ToString("X", CultureInfo.InvariantCulture) +
					"，位置组件槽 0x" + profile.PositionComponentOffset.ToString("X", CultureInfo.InvariantCulture) +
					"，血量 0x" + profile.HealthOffset.ToString("X", CultureInfo.InvariantCulture) +
					"，血量组件槽 0x" + profile.HealthComponentOffset.ToString("X", CultureInfo.InvariantCulture) +
					"，最大血量 0x" + profile.MaxHealthOffset.ToString("X", CultureInfo.InvariantCulture) +
					"，队伍 0x" + profile.TeamOffset.ToString("X", CultureInfo.InvariantCulture) +
					"，姓名指针 0x" + profile.NamePointerOffset.ToString("X", CultureInfo.InvariantCulture) +
					"，评分 " + profile.Score.ToString("0.0", CultureInfo.InvariantCulture) + "。");

				failures += Check("位置 x", profile.PositionOffsetX, expected.PositionX);
				failures += Check("位置 y", profile.PositionOffsetY, expected.PositionY);
				failures += Check("位置 z", profile.PositionOffsetZ, expected.PositionZ);
				failures += Check("位置组件指针槽", profile.PositionComponentOffset, expected.PositionComponent);
				failures += Check("当前血量", profile.HealthOffset, expected.Health);
				failures += Check("血量组件指针槽", profile.HealthComponentOffset, expected.HealthComponent);
				failures += Check("最大血量", profile.MaxHealthOffset, expected.MaxHealth);
				failures += Check("最大血量组件指针槽", profile.MaxHealthComponentOffset, expected.MaxHealthComponent);
				failures += Check("队伍", profile.TeamOffset, expected.Team);
				failures += Check("队伍组件指针槽", profile.TeamComponentOffset, expected.TeamComponent);
				failures += Check("姓名指针", profile.NamePointerOffset, expected.NamePointer);
				failures += Check("姓名组件指针槽", profile.NameComponentOffset, expected.NameComponent);
			}
		}
		catch (Exception ex)
		{
			ConsoleBridge60.WriteLine("[失败] 自检异常：" + ex.Message);
			failures++;
		}
		finally
		{
			if (block != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(block);
			}
			if (current != null)
			{
				current.Dispose();
			}
		}
		return failures;
	}

	private static int Check(string label, int actual, int expected)
	{
		bool same = actual == expected;
		ConsoleBridge60.WriteLine((same ? "[成功] " : "[失败] ") + label + "：期望 0x" +
			expected.ToString("X", CultureInfo.InvariantCulture) + "，实际 " +
			(actual < 0 ? "(未找到)" : ("0x" + actual.ToString("X", CultureInfo.InvariantCulture))) + "。");
		return same ? 0 : 1;
	}

	/// <summary>往合成块里写一个 float（.NET Framework 没有 Marshal.WriteSingle）。</summary>
	private static void WriteSingle(ulong address, int offset, float value)
	{
		byte[] blob = BitConverter.GetBytes(value);
		Marshal.Copy(blob, 0, new IntPtr((long)address + offset), blob.Length);
	}

	/// <summary>名字区起始偏移。</summary>
	private const int NameAreaOffset = 0x100;

	/// <summary>实体区起始偏移。</summary>
	private const int EntityAreaOffset = 0x10000;

	/// <summary>变换组件区起始偏移。</summary>
	private const int TransformAreaOffset = 0x20000;

	/// <summary>血量组件区起始偏移。</summary>
	private const int HealthAreaOffset = 0x30000;

	/// <summary>实体指针数组起始偏移。</summary>
	private const int ArrayAreaOffset = 0x40000;

	/// <summary>摆好合成夹具，返回块首地址。</summary>
	private static ulong BuildFixture(LayoutMode mode, out IntPtr block)
	{
		int total = ArrayAreaOffset + EntityCount * 8 + 0x1000;
		block = Marshal.AllocHGlobal(total);
		for (int i = 0; i < total; i++)
		{
			Marshal.WriteByte(block, i, 0);
		}
		ulong baseAddress = (ulong)block.ToInt64();

		for (int i = 0; i < EntityCount; i++)
		{
			ulong entity = baseAddress + (ulong)(EntityAreaOffset + i * EntityStride);

			// 姓名：固定长度 ASCII
			byte[] name = System.Text.Encoding.ASCII.GetBytes("Player" + i.ToString("D2", CultureInfo.InvariantCulture) + "\0");
			Marshal.Copy(name, 0, new IntPtr((long)(baseAddress + (ulong)(NameAreaOffset + i * 32))), name.Length);
			Marshal.WriteInt64(new IntPtr((long)entity), NameFieldOffset, (long)(baseAddress + (ulong)(NameAreaOffset + i * 32)));

			float x = 1500f + i * 37f;
			float y = 2400f + i * 11f;
			float z = -800f + i * 5f;
			float health = (i == 5 || i == 9) ? 62.5f : 100f;

			if (mode == LayoutMode.Components)
			{
				// 变换组件：行主序 4x4，平移在 [12,13,14] → 字节 0x30/0x34/0x38。
				ulong transform = baseAddress + (ulong)(TransformAreaOffset + i * EntityStride);
				WriteSingle(transform, ComponentMatrixOffset + 0, 1f);
				WriteSingle(transform, ComponentMatrixOffset + 20, 1f);
				WriteSingle(transform, ComponentMatrixOffset + 40, 1f);
				WriteSingle(transform, ComponentMatrixOffset + 48, x);
				WriteSingle(transform, ComponentMatrixOffset + 52, y);
				WriteSingle(transform, ComponentMatrixOffset + 56, z);
				WriteSingle(transform, ComponentMatrixOffset + 60, 1f);
				Marshal.WriteInt64(new IntPtr((long)entity), TransformPointerOffset, (long)transform);

				// 血量组件：当前 +0x30、最大 +0x34。
				ulong healthComponent = baseAddress + (ulong)(HealthAreaOffset + i * EntityStride);
				WriteSingle(healthComponent, ComponentHealthOffset + 0, health);
				WriteSingle(healthComponent, ComponentHealthOffset + 4, 100f);
				Marshal.WriteInt64(new IntPtr((long)entity), HealthPointerOffset, (long)healthComponent);

				// 队伍内联在实体里。
				Marshal.WriteInt32(new IntPtr((long)entity), ComponentTeamFieldOffset, (i % 2) + 1);
			}
			else
			{
				// 自增 id：应当被判定为"种类太多"，不能当成队伍
				Marshal.WriteInt32(new IntPtr((long)entity), IdFieldOffset, i + 1);

				if (mode == LayoutMode.Contiguous)
				{
					WriteSingle(entity, PositionFieldOffset + 0, x);
					WriteSingle(entity, PositionFieldOffset + 4, y);
					WriteSingle(entity, PositionFieldOffset + 8, z);
				}
				else
				{
					// 列主序 4x4：索引 = 列 * 4 + 行。
					//   [0]=m00 [1]=m01 [2]=m02 [3]=tx
					//   [4]=m10 [5]=m11 [6]=m12 [7]=ty
					//   [8]=m20 [9]=m21 [10]=m22 [11]=tz
					//   [12..14]=0  [15]=1
					WriteSingle(entity, MatrixFieldOffset + 0, 1f);
					WriteSingle(entity, MatrixFieldOffset + 20, 1f);
					WriteSingle(entity, MatrixFieldOffset + 40, 1f);
					WriteSingle(entity, MatrixFieldOffset + 12, x);
					WriteSingle(entity, MatrixFieldOffset + 28, y);
					WriteSingle(entity, MatrixFieldOffset + 44, z);
					WriteSingle(entity, MatrixFieldOffset + 60, 1f);
				}

				// 血量：两个实体掉血，其余满血；最大血量恒定 100
				WriteSingle(entity, HealthFieldOffset, health);
				WriteSingle(entity, MaxHealthFieldOffset, 100f);

				// 队伍：两个阵营
				Marshal.WriteInt32(new IntPtr((long)entity), TeamFieldOffset, (i % 2) + 1);
			}

			// 指针数组
			Marshal.WriteInt64(new IntPtr((long)(baseAddress + (ulong)(ArrayAreaOffset + i * 8))), 0, (long)entity);
		}
		return baseAddress;
	}
}
