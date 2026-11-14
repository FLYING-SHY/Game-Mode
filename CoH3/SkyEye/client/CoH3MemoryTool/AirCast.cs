using System.Collections.Generic;

namespace CoH3MemoryTool;

internal sealed class AirCast
{
	public AirZone Zone;

	public AirDefinition Definition;

	public double Born;

	public double LastSeen;

	public bool Seen;

	public readonly HashSet<string> Aircraft = new HashSet<string>();
}
