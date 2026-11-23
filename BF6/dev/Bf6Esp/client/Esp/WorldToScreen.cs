using System;

namespace Bf6Esp;

/// <summary>投影结果：屏幕像素坐标 + 深度（除数 clip.w）。</summary>
internal struct ScreenPoint60
{
	public float X;

	public float Y;

	public float Depth;
}

/// <summary>
/// 世界坐标 -> 屏幕像素。
///
/// 矩阵按行主序存储：m[row*4+col]。Frostbite / D3D 约定是"行向量右乘" v' = v * M：
///   clip.x = x*m[0] + y*m[4] + z*m[8] + m[12]
///   clip.y = x*m[1] + y*m[5] + z*m[9] + m[13]
///   clip.z = x*m[2] + y*m[6] + z*m[10] + m[14]
///   clip.w = x*m[3] + y*m[7] + z*m[11] + m[15]     <- 透视除数（FOV 无关的深度）
///
/// 若目标程序把矩阵按"列向量右乘"（OpenGL 风格）存放，则 w 应当是 row3 · p
/// （m[12]x + m[13]y + m[14]z + m[15]）。本类按矩阵自身的特征自动判断约定，
/// 也允许外部强制指定（UseColumnVector）。
/// </summary>
internal static class WorldToScreen
{
	/// <summary>true 表示按 OpenGL 列向量约定解释矩阵。</summary>
	public static bool UseColumnVector;

	/// <summary>投影并返回屏幕坐标；仅当矩阵不可用或点在相机背后时返回 false。</summary>
	public static bool Project(float[] m, float x, float y, float z, int width, int height, out ScreenPoint60 point)
	{
		point = default(ScreenPoint60);
		if (m == null || m.Length < 16 || width <= 0 || height <= 0)
		{
			return false;
		}
		float clipX;
		float clipY;
		float clipZ;
		float clipW;
		if (UseColumnVector || (!LooksLikeFrostbiteLayout(m) && LooksLikeColumnVectorLayout(m)))
		{
			clipX = m[0] * x + m[1] * y + m[2] * z + m[3];
			clipY = m[4] * x + m[5] * y + m[6] * z + m[7];
			clipZ = m[8] * x + m[9] * y + m[10] * z + m[11];
			clipW = m[12] * x + m[13] * y + m[14] * z + m[15];
		}
		else
		{
			clipX = x * m[0] + y * m[4] + z * m[8] + m[12];
			clipY = x * m[1] + y * m[5] + z * m[9] + m[13];
			clipZ = x * m[2] + y * m[6] + z * m[10] + m[14];
			clipW = x * m[3] + y * m[7] + z * m[11] + m[15];
		}
		if (float.IsNaN(clipX) || float.IsNaN(clipY) || float.IsNaN(clipZ) || float.IsNaN(clipW))
		{
			return false;
		}
		if (clipW <= 0.0001f)
		{
			return false;
		}
		float ndcX = clipX / clipW;
		float ndcY = clipY / clipW;
		point.X = (ndcX * 0.5f + 0.5f) * width;
		point.Y = (0.5f - ndcY * 0.5f) * height;
		point.Depth = clipW;
		return true;
	}

	/// <summary>投影并要求结果落在视口内。</summary>
	public static bool TryProject(float[] m, float x, float y, float z, int width, int height, out ScreenPoint60 point)
	{
		if (!Project(m, x, y, z, width, height, out point))
		{
			return false;
		}
		if (point.X < 0f || point.Y < 0f || point.X > width || point.Y > height)
		{
			return false;
		}
		return true;
	}

	/// <summary>Frostbite/D3D 特征：m[11]≈1、m[14]≈±1、m[15]≈0、m[12]/m[13]≈0。</summary>
	public static bool LooksLikeFrostbiteLayout(float[] m)
	{
		if (m == null || m.Length < 16)
		{
			return false;
		}
		return Math.Abs(m[11] - 1f) < 0.01f
			&& Math.Abs(m[15]) < 0.001f
			&& Math.Abs(Math.Abs(m[14]) - 1f) < 0.01f
			&& Math.Abs(m[12]) < 0.001f
			&& Math.Abs(m[13]) < 0.001f;
	}

	/// <summary>OpenGL 特征：m[3]≈1、m[10]≈1、m[15]≈0、m[12]/m[13]≈0。</summary>
	public static bool LooksLikeColumnVectorLayout(float[] m)
	{
		if (m == null || m.Length < 16)
		{
			return false;
		}
		return Math.Abs(m[3] - 1f) < 0.01f
			&& Math.Abs(m[15]) < 0.001f
			&& Math.Abs(Math.Abs(m[10]) - 1f) < 0.01f
			&& Math.Abs(m[12]) < 0.001f
			&& Math.Abs(m[13]) < 0.001f;
	}
}
