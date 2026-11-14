using System;

namespace CoH3MemoryTool;

internal static class Loiter49
{
	public static bool Supported(string path)
	{
		if (!(path == "ebps\\races\\german\\aircraft\\stuka_ju87_loiter") && !(path == "ebps\\races\\german\\aircraft\\stuka_ju87_rocket_strafe_loiter_ger"))
		{
			return path == "ebps\\races\\afrika_korps\\aircraft\\stuka_ju87_anti_vehicle_loiter_ak";
		}
		return true;
	}

	public static void Observe(LoiterState49 state, bool unitTarget, float x, float y, float z, float px, float pz, float headingX, float headingZ, float range, float halfWidth, string key, string name)
	{
		if (state.Zone != null && AirLogic.Passed(state.Zone, null, px, pz))
		{
			state.Zone = null;
		}
		if (!unitTarget && !state.Acquired)
		{
			return;
		}
		if (!unitTarget && y > state.Ground + 8f)
		{
			state.Next = true;
			return;
		}
		if (state.Zone != null && !state.Next && y <= state.Ground + 3f)
		{
			double num = Math.Sqrt(headingX * headingX + headingZ * headingZ);
			if (num > 0.9 && num < 1.1)
			{
				state.Zone.X = x;
				state.Zone.Y = y;
				state.Zone.Z = z;
				state.Zone.DX = (float)((double)headingX / num);
				state.Zone.DZ = (float)((double)headingZ / num);
			}
		}
		if ((unitTarget && !state.Acquired) || (!unitTarget && state.Next && y <= state.Ground + 3f))
		{
			double num2 = Math.Sqrt(headingX * headingX + headingZ * headingZ);
			if (!(num2 < 0.9) && !(num2 > 1.1) && !(range < 1f) && !(range > 200f) && !(halfWidth <= 0f) && !(halfWidth > 100f))
			{
				float dX = (float)((double)headingX / num2);
				float dZ = (float)((double)headingZ / num2);
				state.Acquired = true;
				state.Next = false;
				state.Ground = y;
				state.Pass++;
				state.Zone = new AirZone
				{
					Key = "loiter:" + key + ":" + state.Pass,
					Name = name,
					X = x,
					Y = y,
					Z = z,
					DX = dX,
					DZ = dZ,
					Length = range,
					HalfWidth = halfWidth
				};
			}
		}
	}
}
