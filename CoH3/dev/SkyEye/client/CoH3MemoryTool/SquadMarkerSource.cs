using System.Collections.Generic;
using System.Drawing;

namespace CoH3MemoryTool;

internal sealed class SquadMarkerSource
{
	public long Squad;

	public long Identity;

	public string Path;

	public uint Slot;

	public bool ColourKnown;

	public Color Colour;

	public readonly List<SquadMarkerMember> Members = new List<SquadMarkerMember>();
}
