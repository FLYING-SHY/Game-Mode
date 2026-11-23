using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Bf6Esp;

/// <summary>
/// 反射解析器的合成夹具（自测用）。
///
/// 在一块自己分配的非托管内存里，按 Frostbite/MEC 的布局手工摆出：
///   * 一条 TypeInfo 链表（节点 + info 头 + FieldInfo 数组）；
///   * 一段独立的字段描述数组（用于验证兜底扫描路径）。
/// 夹具里的名字与偏移都是已知值，因此解析器必须能把它们原样还原出来，
/// 否则说明解析逻辑本身有问题（而不是游戏数据的问题）。
/// </summary>
internal sealed class ReflectionFixture60
{
	internal const int BlockSize = 0x2000;

	// 字符串区
	private const int StrHealthType = 0x0100;

	private const int StrHealthComponent = 0x0140;

	private const int StrMaxHealth = 0x0170;

	private const int StrSoldierType = 0x0190;

	private const int StrTeamId = 0x01C0;

	private const int StrPosX = 0x01E0;

	private const int StrPosY = 0x01F0;

	private const int StrPosZ = 0x0200;

	private const int StrHealthComp = 0x0210;

	// 结构区
	private const int NodeA = 0x0400;

	private const int NodeB = 0x0410;

	private const int InfoHealth = 0x0420;

	private const int InfoSoldier = 0x0460;

	private const int FieldsHealth = 0x04A0;

	private const int FieldsSoldier = 0x04E0;

	private const int DescriptorRun = 0x0500;

	private readonly IntPtr block;

	private readonly byte[] data;

	private ReflectionFixture60()
	{
		block = Marshal.AllocHGlobal(BlockSize);
		data = new byte[BlockSize];
	}

	public ulong BaseAddress
	{
		get { return (ulong)(long)block; }
	}

	public ulong Size
	{
		get { return BlockSize; }
	}

	/// <summary>构造夹具并返回。</summary>
	public static ReflectionFixture60 Build()
	{
		ReflectionFixture60 fixture = new ReflectionFixture60();
		fixture.Populate();
		fixture.Flush();
		return fixture;
	}

	/// <summary>期望的字段名 -> 偏移。</summary>
	public static Dictionary<string, int> ExpectedFields()
	{
		Dictionary<string, int> expected = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		expected["CurrentHealth"] = 0x30;
		expected["MaxHealth"] = 0x34;
		expected["CurrentTeamId"] = 0x40;
		expected["positionX"] = 0x10;
		expected["positionY"] = 0x14;
		expected["positionZ"] = 0x18;
		expected["healthComponent"] = 0x28;
		return expected;
	}

	/// <summary>期望被还原的类型名。</summary>
	public static string[] ExpectedTypes()
	{
		return new string[] { "ClientSoldierHealthComponent", "ClientSoldierEntity" };
	}

	public void Dispose()
	{
		if (block != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(block);
		}
	}

	private void WriteAscii(int offset, string text)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(text);
		Array.Copy(bytes, 0, data, offset, bytes.Length);
		data[offset + bytes.Length] = 0;
	}

	private void WriteUInt64(int offset, ulong value)
	{
		Array.Copy(BitConverter.GetBytes(value), 0, data, offset, 8);
	}

	private void WriteUInt16(int offset, ushort value)
	{
		Array.Copy(BitConverter.GetBytes(value), 0, data, offset, 2);
	}

	private void WriteByte(int offset, byte value)
	{
		data[offset] = value;
	}

	public void Flush()
	{
		Marshal.Copy(data, 0, block, BlockSize);
	}

	private ulong At(int offset)
	{
		return BaseAddress + (ulong)offset;
	}

	private void Populate()
	{
		WriteAscii(StrHealthType, "ClientSoldierHealthComponent");
		WriteAscii(StrHealthComponent, "CurrentHealth");
		WriteAscii(StrMaxHealth, "MaxHealth");
		WriteAscii(StrSoldierType, "ClientSoldierEntity");
		WriteAscii(StrTeamId, "CurrentTeamId");
		WriteAscii(StrPosX, "positionX");
		WriteAscii(StrPosY, "positionY");
		WriteAscii(StrPosZ, "positionZ");
		WriteAscii(StrHealthComp, "healthComponent");

		// TypeInfo 链表：节点A -> 节点B -> 结束
		WriteUInt64(NodeA, At(InfoHealth));
		WriteUInt64(NodeA + 8, At(NodeB));
		WriteUInt64(NodeB, At(InfoSoldier));
		WriteUInt64(NodeB + 8, 0UL);

		// info 头（health component 类型）：2 个字段
		WriteUInt64(InfoHealth, At(StrHealthType));
		WriteUInt16(InfoHealth + 0x08, 1);
		WriteUInt16(InfoHealth + 0x0A, 0x40);
		WriteUInt64(InfoHealth + 0x10, At(StrHealthType));
		WriteByte(InfoHealth + 0x18, 8);
		WriteUInt16(InfoHealth + 0x1A, 2);
		WriteUInt64(InfoHealth + 0x20, 0UL);
		WriteUInt64(InfoHealth + 0x28, 0UL);
		WriteUInt64(InfoHealth + 0x30, At(FieldsHealth));

		// info 头（soldier entity 类型）：1 个字段
		WriteUInt64(InfoSoldier, At(StrSoldierType));
		WriteUInt16(InfoSoldier + 0x08, 3);
		WriteUInt16(InfoSoldier + 0x0A, 0x120);
		WriteUInt64(InfoSoldier + 0x10, At(StrSoldierType));
		WriteByte(InfoSoldier + 0x18, 8);
		WriteUInt16(InfoSoldier + 0x1A, 1);
		WriteUInt64(InfoSoldier + 0x20, 0UL);
		WriteUInt64(InfoSoldier + 0x28, 0UL);
		WriteUInt64(InfoSoldier + 0x30, At(FieldsSoldier));

		// FieldInfo 数组：0x18 字节 = name* + u16 flags + u16 offset + type*
		WriteUInt64(FieldsHealth, At(StrHealthComponent));
		WriteUInt16(FieldsHealth + 0x08, 0);
		WriteUInt16(FieldsHealth + 0x0A, 0x30);
		WriteUInt64(FieldsHealth + 0x10, At(InfoHealth));

		WriteUInt64(FieldsHealth + 0x18, At(StrMaxHealth));
		WriteUInt16(FieldsHealth + 0x20, 0);
		WriteUInt16(FieldsHealth + 0x22, 0x34);
		WriteUInt64(FieldsHealth + 0x28, At(InfoHealth));

		WriteUInt64(FieldsSoldier, At(StrTeamId));
		WriteUInt16(FieldsSoldier + 0x08, 0);
		WriteUInt16(FieldsSoldier + 0x0A, 0x40);
		WriteUInt64(FieldsSoldier + 0x10, At(InfoHealth));

		// 独立的描述符数组（4 条记录，步长 0x18）—— 验证兜底扫描路径
		WriteUInt64(DescriptorRun, At(StrPosX));
		WriteUInt16(DescriptorRun + 0x08, 0);
		WriteUInt16(DescriptorRun + 0x0A, 0x10);
		WriteUInt64(DescriptorRun + 0x10, 0UL);

		WriteUInt64(DescriptorRun + 0x18, At(StrPosY));
		WriteUInt16(DescriptorRun + 0x20, 0);
		WriteUInt16(DescriptorRun + 0x22, 0x14);
		WriteUInt64(DescriptorRun + 0x28, 0UL);

		WriteUInt64(DescriptorRun + 0x30, At(StrPosZ));
		WriteUInt16(DescriptorRun + 0x38, 0);
		WriteUInt16(DescriptorRun + 0x3A, 0x18);
		WriteUInt64(DescriptorRun + 0x40, 0UL);

		WriteUInt64(DescriptorRun + 0x48, At(StrHealthComp));
		WriteUInt16(DescriptorRun + 0x50, 0);
		WriteUInt16(DescriptorRun + 0x52, 0x28);
		WriteUInt64(DescriptorRun + 0x58, 0UL);
	}

	/// <summary>夹具摘要文本（写进自检日志）。</summary>
	public string Describe()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("夹具基址=0x" + BaseAddress.ToString("X", CultureInfo.InvariantCulture) +
			" 大小=0x" + Size.ToString("X", CultureInfo.InvariantCulture));
		builder.AppendLine("TypeInfo 节点: 0x" + At(NodeA).ToString("X", CultureInfo.InvariantCulture) +
			" -> 0x" + At(NodeB).ToString("X", CultureInfo.InvariantCulture));
		builder.AppendLine("info 头: health=0x" + At(InfoHealth).ToString("X", CultureInfo.InvariantCulture) +
			" soldier=0x" + At(InfoSoldier).ToString("X", CultureInfo.InvariantCulture));
		builder.AppendLine("FieldInfo 数组: 0x" + At(FieldsHealth).ToString("X", CultureInfo.InvariantCulture) +
			" / 0x" + At(FieldsSoldier).ToString("X", CultureInfo.InvariantCulture));
		builder.AppendLine("描述符数组: 0x" + At(DescriptorRun).ToString("X", CultureInfo.InvariantCulture));
		return builder.ToString();
	}
}
