using System;

namespace CoH3MemoryTool;

internal static class BarrageLogic
{
	public static bool Supported(string path)
	{
		return BarrageCatalogue.Supported(path);
	}

	public static bool Running(byte[] a)
	{
		if (a == null || a.Length < 164)
		{
			return false;
		}
		if (a[76] == 2)
		{
			return false;
		}
		float num = BitConverter.ToSingle(a, 72);
		float num2 = BitConverter.ToSingle(a, 152);
		if (float.IsNaN(num) || float.IsInfinity(num) || float.IsNaN(num2) || float.IsInfinity(num2))
		{
			return false;
		}
		if (!(num > 0f) && !(num2 > 0f) && BitConverter.ToInt16(a, 160) <= 0 && (a[144] & 4) == 0)
		{
			return (a[80] & 8) != 0;
		}
		return true;
	}

	public static bool Active(string ability, byte[] raw)
	{
		if (!AttackCatalogue67.EarlyBarrage(ability))
		{
			return Running(raw);
		}
		return AttackCatalogue67.Assigned(raw);
	}

	public static bool Valid(float x, float y, float z)
	{
		return SquadMarkerLogic.ValidPosition(x, y, z);
	}

	public static bool Position(byte[] targets, int index, out float x, out float y, out float z)
	{
		x = (y = (z = 0f));
		int num = index * 20;
		if (index < 0 || targets == null || num > targets.Length - 20)
		{
			return false;
		}
		if (targets[num] != 1 && targets[num] != 2)
		{
			return false;
		}
		x = BitConverter.ToSingle(targets, num + 4);
		y = BitConverter.ToSingle(targets, num + 8);
		z = BitConverter.ToSingle(targets, num + 12);
		return Valid(x, y, z);
	}

	public static string Key(long entity, uint generation, long instance, int target)
	{
		return entity + ":" + generation + ":" + instance + ":" + target;
	}
}
