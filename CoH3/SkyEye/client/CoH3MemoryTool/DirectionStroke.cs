using System.Drawing;

namespace CoH3MemoryTool;

internal sealed class DirectionStroke
{
	public bool Dashed;

	public float Width;

	public float DashLength;

	public float DashGap;

	public Color Colour;

	public PointF[] Points;

	public Rectangle Bounds;
}
