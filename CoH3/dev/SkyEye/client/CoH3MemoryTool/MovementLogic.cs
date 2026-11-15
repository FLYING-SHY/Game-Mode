using System;

namespace CoH3MemoryTool;

internal static class MovementLogic
{
	public static bool Active(byte[] actor, byte[] state)
	{
		uint num = BitConverter.ToUInt32(actor, 64);
		uint num2 = BitConverter.ToUInt32(state, 144);
		if (num == 0 && BitConverter.ToUInt32(actor, 8) == 0 && actor[16] != 5 && actor[76] == 2)
		{
			if (num2 != 2 && num2 != 3)
			{
				return num2 == 4;
			}
			return true;
		}
		return false;
	}

	public static MovementData Decode(byte[] actor, byte[] state)
	{
		if (!Active(actor, state))
		{
			return null;
		}
		float x = BitConverter.ToSingle(actor, 80);
		float y = BitConverter.ToSingle(actor, 84);
		float z = BitConverter.ToSingle(actor, 88);
		if (!SquadMarkerLogic.ValidPosition(x, y, z))
		{
			return null;
		}
		MovementData movementData = new MovementData();
		movementData.X = x;
		movementData.Y = y;
		movementData.Z = z;
		return movementData;
	}
}
