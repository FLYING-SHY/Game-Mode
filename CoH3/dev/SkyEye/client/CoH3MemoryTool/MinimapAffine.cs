using System;
using System.Drawing;

namespace CoH3MemoryTool;

internal struct MinimapAffine
{
	public double XX;

	public double XY;

	public double YX;

	public double YY;

	public double TX;

	public double TY;

	public static MinimapAffine Identity => new MinimapAffine
	{
		XX = 1.0,
		YY = 1.0
	};

	public bool Valid
	{
		get
		{
			double[] array = new double[6] { XX, XY, YX, YY, TX, TY };
			foreach (double num in array)
			{
				if (double.IsNaN(num) || double.IsInfinity(num) || Math.Abs(num) > 1000000.0)
				{
					return false;
				}
			}
			return Math.Abs(XX * YY - XY * YX) > 1E-09;
		}
	}

	public static MinimapAffine Translation(double x, double y)
	{
		return new MinimapAffine
		{
			XX = 1.0,
			YY = 1.0,
			TX = x,
			TY = y
		};
	}

	public MinimapAffine Then(MinimapAffine b)
	{
		return new MinimapAffine
		{
			XX = b.XX * XX + b.XY * YX,
			XY = b.XX * XY + b.XY * YY,
			YX = b.YX * XX + b.YY * YX,
			YY = b.YX * XY + b.YY * YY,
			TX = b.XX * TX + b.XY * TY + b.TX,
			TY = b.YX * TX + b.YY * TY + b.TY
		};
	}

	public PointF Point(double x, double y)
	{
		return new PointF((float)(XX * x + XY * y + TX), (float)(YX * x + YY * y + TY));
	}

	public static MinimapAffine Native(byte[] data)
	{
		uint num = BitConverter.ToUInt32(data, 28);
		switch (num)
		{
		default:
			throw new Exception("小地图界面矩阵尚未就绪。");
		case 0u:
			return Identity;
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		{
			MinimapAffine result = Identity;
			if ((num & 4) != 0)
			{
				result = new MinimapAffine
				{
					XX = BitConverter.ToSingle(data, 0),
					YX = BitConverter.ToSingle(data, 4),
					XY = BitConverter.ToSingle(data, 8),
					YY = BitConverter.ToSingle(data, 12),
					TX = BitConverter.ToSingle(data, 16),
					TY = BitConverter.ToSingle(data, 20)
				};
			}
			else
			{
				if ((num & 2) != 0)
				{
					result.XX = BitConverter.ToSingle(data, 0);
					result.YY = BitConverter.ToSingle(data, 12);
				}
				if ((num & 1) != 0)
				{
					result.TX = BitConverter.ToSingle(data, 16);
					result.TY = BitConverter.ToSingle(data, 20);
				}
			}
			if (!result.Valid)
			{
				throw new Exception("小地图界面矩阵正在变化。");
			}
			return result;
		}
		}
	}

	public static MinimapFrame Frame(float ww, float wh, float pixelsX, float pixelsY, float rootX, float rootY, MinimapAffine localToUi)
	{
		MinimapAffine minimapAffine = new MinimapAffine
		{
			XX = pixelsX / ww,
			YY = pixelsY / wh,
			TX = pixelsX / 2f,
			TY = pixelsY / 2f
		};
		MinimapAffine worldToUi = minimapAffine.Then(localToUi);
		if (!worldToUi.Valid)
		{
			throw new Exception("小地图投影无效。");
		}
		PointF pointF = worldToUi.Point(0.0, 0.0);
		MinimapFrame minimapFrame = new MinimapFrame();
		minimapFrame.NativeTransform = true;
		minimapFrame.WorldToUi = worldToUi;
		minimapFrame.WorldWidth = ww;
		minimapFrame.WorldHeight = wh;
		minimapFrame.RootWidth = rootX;
		minimapFrame.RootHeight = rootY;
		minimapFrame.CentreX = pointF.X;
		minimapFrame.CentreY = pointF.Y;
		minimapFrame.PixelWidth = (float)(Math.Sqrt(worldToUi.XX * worldToUi.XX + worldToUi.YX * worldToUi.YX) * (double)ww);
		minimapFrame.PixelHeight = (float)(Math.Sqrt(worldToUi.XY * worldToUi.XY + worldToUi.YY * worldToUi.YY) * (double)wh);
		minimapFrame.Angle = (float)(Math.Atan2(worldToUi.YX, worldToUi.XX) * 180.0 / Math.PI);
		return minimapFrame;
	}
}
