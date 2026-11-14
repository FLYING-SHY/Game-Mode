using System;

namespace CoH3MemoryTool;

internal sealed class MarkerScene
{
	public MarkerReadContext Context;

	public SquadMarkerSource[] Sources;

	public ProjectilePlayer[] Players;

	public ProjectilePlayer[] ProjectilePlayers;

	public DateTime Updated;

	public string Warning;
}
