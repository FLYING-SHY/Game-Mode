using System.Drawing;

namespace CoH3MemoryTool;

internal sealed class SquadMarker
{
	public long Source;

	public long Identity;

	public MovementData Movement;

	public DirectionData Direction;

	public string Path;

	public string Name;

	public uint Slot;

	public bool Vehicle;

	public bool TextOnly;

	public Color Colour;

	public float X;

	public float Y;

	public float Z;
}
