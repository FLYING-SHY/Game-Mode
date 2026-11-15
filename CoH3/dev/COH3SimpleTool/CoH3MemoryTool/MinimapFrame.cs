using System;
using System.Drawing;

namespace CoH3MemoryTool;

internal sealed class MinimapFrame
{
	public bool NativeTransform;

	public MinimapAffine WorldToUi;

	public float CentreX;

	public float CentreY;

	public float PixelWidth;

	public float PixelHeight;

	public float RootWidth;

	public float RootHeight;

	public float WorldWidth;

	public float WorldHeight;

	public float Angle;

	public bool Valid
	{
		get
		{
			if (WorldWidth >= 8f && WorldHeight >= 8f && PixelWidth >= 10f && PixelHeight >= 10f && RootWidth > 0f)
			{
				return RootHeight > 0f;
			}
			return false;
		}
	}

	public PointF Point(float x, float z, int width, int height)
	{
		if (NativeTransform)
		{
			PointF pointF = WorldToUi.Point(x, z);
			return new PointF(pointF.X * (float)width / RootWidth, pointF.Y * (float)height / RootHeight);
		}
		double num = (double)Angle * Math.PI / 180.0;
		double num2 = x / WorldWidth * PixelWidth;
		double num3 = (0f - z) / WorldHeight * PixelHeight;
		return new PointF((float)(((double)CentreX + num2 * Math.Cos(num) - num3 * Math.Sin(num)) * (double)width / (double)RootWidth), (float)(((double)CentreY + num2 * Math.Sin(num) + num3 * Math.Cos(num)) * (double)height / (double)RootHeight));
	}

	public bool Project(float x, float z, int width, int height, out PointF point)
	{
		point = default(PointF);
		if (!Valid || !SquadMarkerLogic.ValidPosition(x, 0f, z) || Math.Abs(x) > WorldWidth / 2f || Math.Abs(z) > WorldHeight / 2f)
		{
			return false;
		}
		point = Point(x, z, width, height);
		return true;
	}

	public PointF[] Corners(int width, int height)
	{
		return new PointF[4]
		{
			Point((0f - WorldWidth) / 2f, (0f - WorldHeight) / 2f, width, height),
			Point(WorldWidth / 2f, (0f - WorldHeight) / 2f, width, height),
			Point(WorldWidth / 2f, WorldHeight / 2f, width, height),
			Point((0f - WorldWidth) / 2f, WorldHeight / 2f, width, height)
		};
	}
}
