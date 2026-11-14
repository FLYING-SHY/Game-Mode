using System;
using System.Drawing;

namespace CoH3MemoryTool;

internal sealed class MineCamera
{
	public float[] View;

	public float[] Projection;

	public bool Project(float x, float y, float z, int width, int height, out PointF point)
	{
		float num = x * View[0] + y * View[4] + z * View[8] + View[12];
		float num2 = x * View[1] + y * View[5] + z * View[9] + View[13];
		float num3 = x * View[2] + y * View[6] + z * View[10] + View[14];
		float num4 = num * Projection[0] + num2 * Projection[4] + num3 * Projection[8] + Projection[12];
		float num5 = num * Projection[1] + num2 * Projection[5] + num3 * Projection[9] + Projection[13];
		float num6 = num * Projection[3] + num2 * Projection[7] + num3 * Projection[11] + Projection[15];
		point = default(PointF);
		if (num6 < 0.05f)
		{
			return false;
		}
		float num7 = num4 / num6;
		float num8 = num5 / num6;
		if (float.IsNaN(num7) || float.IsNaN(num8) || Math.Abs(num7) > 1f || Math.Abs(num8) > 1f)
		{
			return false;
		}
		point = new PointF((num7 + 1f) * (float)width / 2f, (1f - num8) * (float)height / 2f);
		return true;
	}
}
