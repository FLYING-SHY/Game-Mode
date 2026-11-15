using System.Collections.Generic;
using System.Drawing;

namespace CoH3MemoryTool;

internal sealed class PlayerInfoRow
{
	internal bool IsLocalPlayer;

	internal ulong? SteamId64;

	public readonly List<ProductionJob> Kwk38Upgrading = new List<ProductionJob>();

	public bool Kwk38Known = true;

	public bool BarragesKnown = true;

	public readonly List<BarrageRecord> Barrages = new List<BarrageRecord>();

	public long PlayerAddress;

	public long PlayerIdentity;

	public bool GameColourKnown;

	public Color GameColour;

	public uint Slot;

	public uint Team;

	public string Name;

	public string Relation;

	public string Group;

	public float Manpower;

	public float Fuel;

	public float Munition;

	public float CP;

	public bool MarkerDataKnown = true;

	public readonly List<SquadMarkerSource> MarkerSquads = new List<SquadMarkerSource>();

	public readonly List<MineRecord> Mines = new List<MineRecord>();

	public bool UnitsKnown;

	public readonly Dictionary<string, int> Units = new Dictionary<string, int>();

	public float? GrenadeRemainingTicks;

	public bool ProductionKnown = true;

	public readonly List<ProductionJob> Producing = new List<ProductionJob>();

	public float Population;

	public string Race;

	public bool BuildingsKnown;

	public readonly List<TrackedBuilding> Buildings = new List<TrackedBuilding>();

	public readonly List<ArtilleryState> Artillery = new List<ArtilleryState>();

	public readonly List<string> Upgrades = new List<string>();
}
