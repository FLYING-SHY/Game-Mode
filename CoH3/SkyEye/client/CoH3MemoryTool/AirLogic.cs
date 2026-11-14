using System;

namespace CoH3MemoryTool;

internal static class AirLogic
{
	public static bool Passed(AirZone z, AirDefinition definition, float x, float y)
	{
		if (z.Point || (definition != null && definition.Point) || !SquadMarkerLogic.ValidPosition(x, 0f, y))
		{
			return false;
		}
		float num = definition?.Length ?? z.Length;
		float num2 = definition?.Offset ?? z.Offset;
		return (x - z.X) * z.DX + (y - z.Z) * z.DZ > num2 + num + 10f;
	}

	public static AirZone RecoverStrafe(string path, byte[] body, byte[] actor, long image, long arena, long entity, uint id, uint index)
	{
		if (body == null || body.Length < 392 || actor == null || actor.Length < 284 || BitConverter.ToUInt32(body, 104) != id || BitConverter.ToUInt32(body, 108) != index || (BitConverter.ToUInt32(body, 160) & 0x4000) != 0)
		{
			return null;
		}
		if (BitConverter.ToInt64(actor, 0) != image + 64559016 || BitConverter.ToUInt32(actor, 36) + arena != entity || BitConverter.ToUInt32(actor, 64) != 1 || actor[16] != 2)
		{
			return null;
		}
		float num = BitConverter.ToSingle(actor, 20);
		float num2 = BitConverter.ToSingle(actor, 24);
		float num3 = BitConverter.ToSingle(actor, 28);
		float num4 = BitConverter.ToSingle(actor, 272);
		float num5 = BitConverter.ToSingle(actor, 276);
		float num6 = BitConverter.ToSingle(actor, 280);
		float num7 = BitConverter.ToSingle(body, 48);
		double num8 = Math.Sqrt(num4 * num4 + num6 * num6);
		if (!SquadMarkerLogic.ValidPosition(num, num2, num3) || double.IsNaN(num8) || double.IsInfinity(num8) || num8 < 0.95 || num8 > 1.01 || float.IsNaN(num5) || float.IsInfinity(num5) || (double)Math.Abs(num5) > 0.1 || float.IsNaN(num7) || float.IsInfinity(num7) || num7 - num2 < 8f)
		{
			return null;
		}
		bool flag = path == "ebps\\races\\german\\aircraft\\stuka_ju87_strafe";
		bool flag2 = path == "ebps\\races\\german\\aircraft\\stuka_ju87_combined_arms_strafe";
		bool flag3 = path == "ebps\\races\\american\\aircraft\\p47_thunderbolt_air_support_center_us";
		if (!flag && !flag2 && !flag3)
		{
			return null;
		}
		num4 = (float)((double)num4 / num8);
		num6 = (float)((double)num6 / num8);
		AirDefinition airDefinition = AirCatalogue.Find(flag3 ? "abilities\\races\\american\\aircraft\\strafing_run_p47_us" : (flag ? "abilities\\races\\german\\battlegroups\\luftwaffe\\luftwaffe_right_1b_strafing_run_ger" : "abilities\\races\\afrika_korps\\battlegroups\\italian_combined_arms\\left\\italian_combined_arms_vehicle_strafe_ak"));
		float num9 = (flag ? 2.5f : 0f);
		AirZone airZone = new AirZone();
		airZone.Key = "flight:" + entity + ":" + id;
		airZone.Name = airDefinition.Name;
		airZone.X = num + num4 * num9;
		airZone.Y = num2;
		airZone.Z = num3 + num6 * num9;
		airZone.DX = num4;
		airZone.DZ = num6;
		airZone.HalfWidth = airDefinition.HalfWidth;
		airZone.Length = airDefinition.Length;
		airZone.Offset = airDefinition.Offset;
		return airZone;
	}

	public static bool Cast(byte[] raw, byte[] targets, long image, long identity, out float x, out float y, out float z, out float dx, out float dz, bool point = false)
	{
		x = (y = (z = (dx = (dz = 0f))));
		if (raw == null || raw.Length < 148 || targets == null || targets.Length != 20 || BitConverter.ToInt64(raw, 0) != image + 64568064 || raw[76] == 2 || raw[20] != 6 || BitConverter.ToInt64(raw, 24) != identity || (raw[144] & 1) == 0)
		{
			return false;
		}
		if (!BarrageLogic.Position(targets, 0, out x, out y, out z))
		{
			return false;
		}
		if (point)
		{
			dx = 0f;
			dz = 1f;
			return true;
		}
		float num = BitConverter.ToSingle(raw, 40);
		float value = BitConverter.ToSingle(raw, 44);
		float num2 = BitConverter.ToSingle(raw, 48);
		double num3 = Math.Sqrt(num * num + num2 * num2);
		if (double.IsNaN(num3) || double.IsInfinity(num3) || num3 < 0.95 || num3 > 1.05 || (double)Math.Abs(value) > 0.02)
		{
			return false;
		}
		dx = (float)((double)num / num3);
		dz = (float)((double)num2 / num3);
		return true;
	}

	public static AirZone Rocket(byte[] body, byte[] motion, byte[] ext, long image, long arena, long entity, uint id, uint index)
	{
		if (body.Length < 392 || motion.Length < 68 || ext.Length < 108 || BitConverter.ToUInt32(body, 104) != id || BitConverter.ToUInt32(body, 108) != index || (BitConverter.ToUInt32(body, 160) & 0x4000) != 0)
		{
			return null;
		}
		if (BitConverter.ToInt64(motion, 0) != image + 65313128 || BitConverter.ToUInt32(motion, 36) + arena != entity || BitConverter.ToUInt32(motion, 64) != 3 || BitConverter.ToInt64(ext, 0) != image + 65307976 || BitConverter.ToUInt32(ext, 8) + arena != entity || (motion[16] != 1 && motion[16] != 2))
		{
			return null;
		}
		float num = BitConverter.ToSingle(motion, 20);
		float num2 = BitConverter.ToSingle(motion, 24);
		float num3 = BitConverter.ToSingle(motion, 28);
		float num4 = BitConverter.ToSingle(ext, 104);
		if (!SquadMarkerLogic.ValidPosition(num, num2, num3) || float.IsNaN(num4) || float.IsInfinity(num4) || num4 <= 0f || num4 > 100f || ProjectileLogic.Arrived(BitConverter.ToSingle(body, 44), BitConverter.ToSingle(body, 48), BitConverter.ToSingle(body, 52), num, num2, num3))
		{
			return null;
		}
		uint num5 = BitConverter.ToUInt32(ext, 56);
		if (num5 == uint.MaxValue)
		{
			return null;
		}
		AirZone airZone = new AirZone();
		airZone.Key = entity + ":" + id;
		airZone.Name = "V1";
		airZone.Owner = arena + num5;
		airZone.Rocket = true;
		airZone.X = num;
		airZone.Y = num2;
		airZone.Z = num3;
		airZone.Radius = num4;
		return airZone;
	}
}
