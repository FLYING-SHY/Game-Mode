using System;

namespace CoH3MemoryTool;

internal static class ProjectileLogic
{
	public static bool Weapon(string path)
	{
		if (!AttackCatalogue67.Tank(path))
		{
			if (!string.IsNullOrEmpty(path) && path.StartsWith("weapon\\", StringComparison.Ordinal))
			{
				if (!path.Contains("\\heavy_artillery\\") && !path.Contains("\\light_artillery\\"))
				{
					return path.Contains("\\mortar\\");
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool Mortar(string path)
	{
		if (!string.IsNullOrEmpty(path) && path.StartsWith("weapon\\"))
		{
			if (!path.Contains("\\mortar\\"))
			{
				return path.Substring(path.LastIndexOf('\\') + 1).Contains("mortar");
			}
			return true;
		}
		return false;
	}

	public static bool Arrived(float x, float y, float z, float tx, float ty, float tz)
	{
		double num = x - tx;
		double num2 = y - ty;
		double num3 = z - tz;
		return num * num + num2 * num2 + num3 * num3 <= 0.0001;
	}

	public static bool Decode(byte[] body, byte[] actor, byte[] ext, long image, long arena, long entity, uint id, uint index, out ProjectileImpact impact, bool mortar = false, bool infantry = false, bool playerSource = false)
	{
		impact = null;
		if (body == null || body.Length < 392 || actor == null || actor.Length < 80 || ext == null || ext.Length < 108)
		{
			return false;
		}
		if (BitConverter.ToUInt32(body, 104) != id || BitConverter.ToUInt32(body, 108) != index || (BitConverter.ToUInt32(body, 160) & 0x4000) != 0)
		{
			return false;
		}
		if (BitConverter.ToInt64(actor, 0) != image + 65313128 || BitConverter.ToUInt32(actor, 36) + arena != entity || (BitConverter.ToUInt32(actor, 64) != 0 && ((!mortar && !infantry) || BitConverter.ToUInt32(actor, 64) != 3)))
		{
			return false;
		}
		if (BitConverter.ToInt64(ext, 0) != image + 65307976 || BitConverter.ToUInt32(ext, 8) + arena != entity || (actor[16] != 1 && actor[16] != 2))
		{
			return false;
		}
		float num = BitConverter.ToSingle(actor, 20);
		float num2 = BitConverter.ToSingle(actor, 24);
		float num3 = BitConverter.ToSingle(actor, 28);
		float num4 = BitConverter.ToSingle(ext, 104);
		float x = BitConverter.ToSingle(body, 44);
		float y = BitConverter.ToSingle(body, 48);
		float z = BitConverter.ToSingle(body, 52);
		if (!SquadMarkerLogic.ValidPosition(num, num2, num3) || !SquadMarkerLogic.ValidPosition(x, y, z) || float.IsNaN(num4) || float.IsInfinity(num4) || num4 < 0f || num4 > 100f || (!infantry && Arrived(x, y, z, num, num2, num3)))
		{
			return false;
		}
		uint num5 = BitConverter.ToUInt32(ext, 20);
		uint num6 = BitConverter.ToUInt32(ext, 24);
		if ((ext[16] != 3 && (!playerSource || ext[16] != 6)) || num5 == 0)
		{
			return false;
		}
		impact = new ProjectileImpact
		{
			Entity = entity,
			Id = id,
			Index = index,
			SourceKey = (((ulong)num5 << 32) | num6),
			X = num,
			Y = num2,
			Z = num3,
			Radius = num4
		};
		return true;
	}
}
