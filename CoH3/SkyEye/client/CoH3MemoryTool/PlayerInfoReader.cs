using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CoH3MemoryTool;

internal sealed class PlayerInfoReader : IDisposable
{
	private sealed class AirCandidate
	{
		public long Entity;

		public long Blueprint;

		public uint Id;

		public uint Index;

		public string Path;

		public double Born;

		public bool NewFlight;

		public bool Finished;

		public AirZone Recovered;
	}

	private struct MiniMemoryRegion
	{
		public IntPtr Base;

		public IntPtr AllocationBase;

		public uint AllocationProtect;

		public ushort PartitionId;

		public UIntPtr Size;

		public uint State;

		public uint Protect;

		public uint Type;
	}

	private sealed class ShellCandidate
	{
		public long Entity;

		public long Blueprint;

		public uint Id;

		public uint Index;

		public int Number;

		public ulong SourceKey;
	}

	private long airRegistry;

	private ulong[] airSeen = new ulong[0];

	private readonly Dictionary<uint, AirCandidate> airCandidates = new Dictionary<uint, AirCandidate>();

	private readonly AirCastTracker airTracker = new AirCastTracker();

	private bool suppressRetainedAir;

	private readonly HashSet<string> ignoredAir = new HashSet<string>();

	private LocalPlayerAccount59 localAccount59;

	private DateTime accountNext59;

	private ulong? steamAccount59;

	private readonly Dictionary<long, LoiterState49> loiterStates49 = new Dictionary<long, LoiterState49>();

	private bool miniCacheRead52;

	private long savedMiniHint52;

	private long cameraCore;

	private readonly MiniScanProgress44 miniProgress = new MiniScanProgress44();

	private long miniScanSerial;

	private long miniActiveScan;

	private bool miniSmallPass = true;

	private int miniCandidateCursor;

	private readonly Dictionary<long, DateTime> miniCandidates = new Dictionary<long, DateTime>();

	private readonly object miniGate = new object();

	private long miniControl;

	private long miniHint;

	private long miniEpoch;

	private string miniMatch;

	private bool miniScanning;

	private bool miniDisposed;

	private DateTime miniRetry = DateTime.MinValue;

	private int miniFailures;

	public string MinimapDiagnostic = "";

	public string MinimapStatus = "小地图定位中…";

	private long miniRoot53;

	private long treeEpoch53 = -1L;

	private bool miniRootLoaded53;

	private readonly Queue<long> miniNodes53 = new Queue<long>();

	private readonly HashSet<long> miniSeen53 = new HashSet<long>();

	private DateTime miniTreeRetry53;

	public bool FlankReadMovement47;

	public string ColourError = "";

	private readonly Process process;

	private readonly ProcHandle handle;

	private readonly long image;

	private readonly DateTime started;

	private readonly Dictionary<string, int> resources = new Dictionary<string, int>();

	private uint barrageTimer = uint.MaxValue;

	private uint stukaBarrageTimer = uint.MaxValue;

	private readonly Dictionary<long, string> blueprintNames = new Dictionary<long, string>();

	private long shellRegistry;

	private ulong[] shellSeen = new ulong[0];

	private readonly Dictionary<uint, ShellCandidate> shellCandidates = new Dictionary<uint, ShellCandidate>();

	private readonly Dictionary<ulong, string> shellSourceNames = new Dictionary<ulong, string>();

	private readonly ProjectileSequenceTracker shellSequence = new ProjectileSequenceTracker();

	private long markerArena;

	private uint markerWorldOffset;

	private long tacticalControl51;

	private DateTime tacticalRetry51;

	public string TacticalStatus51 = "战术地图定位中…";

	internal LocalPlayerAccount59 LocalAccount59
	{
		get
		{
			try
			{
				if (localAccount59 == null || process.HasExited || process.StartTime != started || localAccount59.Match != MatchKey)
				{
					return null;
				}
				CheckMarkerWorld(MatchKey);
				if (LocalIdentity58() != localAccount59.PlayerIdentity)
				{
					return null;
				}
				return localAccount59;
			}
			catch
			{
				return null;
			}
		}
	}

	private string MiniHintPath52 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "minimap-location-cache.txt");

	private string MiniRootPath53 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "minimap-ui-root-cache.txt");

	public string MatchKey { get; private set; }

	private long LocalIdentity58()
	{
		ReadMineCamera();
		long num = Q(cameraCore + 680);
		if (Q(num) != image + 64683768 || Read(num + 24, 1)[0] != 0)
		{
			throw new Exception("等待可控制玩家进入对局。");
		}
		long num2 = Q(num + 8);
		if ((int)num2 == 0 || (uint)(num2 >>> 32) >= 16)
		{
			throw new Exception("本地玩家身份尚未就绪。");
		}
		return num2;
	}

	private long ResolveLocalPlayer58(long arena, long array, uint count, long identity)
	{
		if (count == 0 || count > 16)
		{
			throw new Exception("等待当前对局玩家列表。");
		}
		for (uint num = 0u; num < count; num++)
		{
			uint num2 = U(array + num * 4);
			if (num2 != uint.MaxValue)
			{
				long num3 = Resolve(arena, num2);
				if (Q(num3 + 8) == identity)
				{
					return num3;
				}
			}
		}
		throw new Exception("未在当前对局找到本地玩家。");
	}

	internal IEnumerable<string> VerifyLive58()
	{
		Stopwatch sw = Stopwatch.StartNew();
		List<PlayerInfoRow> rows = Snapshot();
		yield return "PASS supported executable; read-only snapshot " + rows.Count + " players, " + sw.ElapsedMilliseconds + "ms; match=" + MatchKey;
		foreach (PlayerInfoRow p in rows)
		{
			yield return p.Relation + " slot=" + p.Slot + " name=" + p.Name + " MP=" + p.Manpower + " ammo=" + p.Munition + " fuel=" + p.Fuel + " CP=" + p.CP + " pop=" + p.Population + " colour=" + (p.GameColourKnown ? p.GameColour.ToArgb().ToString("x8") : ("UNKNOWN " + ColourError)) + " units=" + UnitNames.Summary(p) + " buildings=" + BuildingNames.Summary(p) + " production=" + ProductionText.Summary(p) + " upgrades=" + p.Upgrades.Count;
			foreach (SquadMarkerSource markerSquad in p.MarkerSquads)
			{
				markerSquad.ColourKnown = p.GameColourKnown;
				markerSquad.Colour = p.GameColour;
			}
		}
		if (rows.Count((PlayerInfoRow playerInfoRow) => playerInfoRow.Relation == "自己") != 1)
		{
			throw new Exception("本地玩家归属验证失败。");
		}
		if (rows.Any((PlayerInfoRow playerInfoRow) => !playerInfoRow.GameColourKnown))
		{
			throw new Exception("游戏配色验证失败：" + ColourError);
		}
		List<SquadMarker> markers = ReadSquadMarkers(MatchKey, rows.SelectMany((PlayerInfoRow playerInfoRow) => playerInfoRow.MarkerSquads));
		yield return "PASS marker samples=" + markers.Count + " sources=" + rows.Sum((PlayerInfoRow playerInfoRow) => playerInfoRow.MarkerSquads.Count) + " directions=" + markers.Count((SquadMarker squadMarker) => squadMarker.Direction != null) + " goals=" + markers.Count((SquadMarker squadMarker) => squadMarker.Movement != null);
		foreach (SquadMarker m in markers.Take(16))
		{
			yield return "UNIT slot=" + m.Slot + " " + m.Name + " xyz=" + m.X + "," + m.Y + "," + m.Z + ((m.Direction == null) ? "" : (" range=" + m.Direction.Range + " arc=" + m.Direction.ArcValid));
		}
		ReadMineCamera();
		yield return "PASS camera validated; match guard=" + IsCurrentMatch55();
		using (PlayerInfoReader worker = new PlayerInfoReader(MarkerContext()))
		{
			List<SquadMarker> second = worker.ReadSquadMarkers(MatchKey, rows.SelectMany((PlayerInfoRow playerInfoRow) => playerInfoRow.MarkerSquads));
			worker.ReadMineCamera();
			yield return "PASS independent marker connection samples=" + second.Count;
		}
		MinimapFrame frame = null;
		sw.Restart();
		while (sw.ElapsedMilliseconds < 12000 && !TryReadMinimap(out frame))
		{
			Thread.Sleep(40);
		}
		if (frame == null)
		{
			throw new Exception("小地图定位未通过：" + MinimapStatus + " " + MinimapDiagnostic);
		}
		yield return "PASS minimap " + sw.ElapsedMilliseconds + "ms world=" + frame.WorldWidth + "x" + frame.WorldHeight + " pixels=" + frame.PixelWidth + "x" + frame.PixelHeight + " centre=" + frame.CentreX + "," + frame.CentreY + " angle=" + frame.Angle;
		bool visible = false;
		long tactical = FindTactical51(miniControl);
		if (tactical == 0)
		{
			throw new Exception("未找到当前界面的战术地图。");
		}
		MinimapFrame big = ReadTacticalLayout51(tactical, out visible);
		yield return "PASS tactical geometry world=" + big.WorldWidth + "x" + big.WorldHeight + " visible=" + visible;
		ProjectilePlayer[] players = rows.Select((PlayerInfoRow playerInfoRow) => new ProjectilePlayer
		{
			Address = playerInfoRow.PlayerAddress,
			Slot = playerInfoRow.Slot,
			Colour = playerInfoRow.GameColour,
			Relation = playerInfoRow.Relation
		}).ToArray();
		yield return "PASS support=" + ReadSupport49(rows).Count + " projectiles=" + ReadProjectileImpacts(MatchKey, players).Count + " air=" + ReadAirZones(MatchKey, players).Count;
	}

	public void ResetAirZones()
	{
		loiterStates49.Clear();
		airTracker.Clear();
		airSeen = new ulong[0];
		airCandidates.Clear();
		suppressRetainedAir = true;
	}

	public List<AirZone> ReadAirZones(string match, IEnumerable<ProjectilePlayer> enemyPlayers)
	{
		CheckMarkerWorld(match);
		long arena = markerArena;
		long num = Resolve(arena, markerWorldOffset);
		long num2 = Resolve(arena, U(num + 260));
		double now = (double)Stopwatch.GetTimestamp() * 1000.0 / (double)Stopwatch.Frequency;
		if (airRegistry != num2)
		{
			airRegistry = num2;
			airSeen = new ulong[0];
			airCandidates.Clear();
			airTracker.Clear();
			ignoredAir.Clear();
			loiterStates49.Clear();
		}
		Dictionary<long, ProjectilePlayer> dictionary = (from p in enemyPlayers
			group p by p.Address).ToDictionary((IGrouping<long, ProjectilePlayer> g) => g.Key, (IGrouping<long, ProjectilePlayer> g) => g.First());
		List<AirZone> list = new List<AirZone>();
		foreach (ProjectilePlayer value6 in dictionary.Values)
		{
			long num3 = value6.Address + 1016;
			uint num4 = U(num3 + 44);
			uint num5 = U(num3 + 40);
			if (num4 > 256)
			{
				throw new Exception("空袭技能列表正在变化。");
			}
			if (num4 == 0)
			{
				continue;
			}
			byte[] value = Read(Resolve(arena, num5), (int)(num4 * 4));
			long identity = Q(value6.Address + 8);
			for (int num6 = 0; num6 < num4; num6++)
			{
				try
				{
					uint num7 = BitConverter.ToUInt32(value, num6 * 4);
					if (num7 == uint.MaxValue)
					{
						continue;
					}
					long num8 = Resolve(arena, num7);
					if (Q(num8) != image + 64568064)
					{
						continue;
					}
					byte[] array = Read(num8, 148);
					string text = BlueprintName(BitConverter.ToInt64(array, 56));
					AirDefinition airDefinition = AirCatalogue.Find(text);
					if (airDefinition != null && BitConverter.ToUInt32(array, 12) == 1)
					{
						uint offset = BitConverter.ToUInt32(array, 8);
						byte[] array2 = Read(Resolve(arena, offset), 20);
						if (AirLogic.Cast(array, array2, image, identity, out var x, out var y, out var z, out var dx, out var dz, airDefinition.Point) && Read(num8, 148).SequenceEqual(array) && Read(Resolve(arena, offset), 20).SequenceEqual(array2))
						{
							list.Add(new AirZone
							{
								Key = value6.Address + ":" + num8 + ":" + BitConverter.ToUInt32(array, 68) + ":" + text + ":" + Convert.ToBase64String(array2.Skip(4).Take(12).ToArray()),
								Name = text,
								Owner = value6.Address,
								Slot = value6.Slot,
								Colour = value6.Colour,
								X = x,
								Y = y,
								Z = z,
								DX = dx,
								DZ = dz
							});
						}
					}
				}
				catch
				{
				}
			}
			if (U(num3 + 44) != num4 || U(num3 + 40) != num5)
			{
				throw new Exception("空袭技能正在切换。");
			}
		}
		if (suppressRetainedAir)
		{
			foreach (AirZone item in list)
			{
				ignoredAir.Add(item.Key);
			}
			suppressRetainedAir = false;
		}
		list.RemoveAll((AirZone c) => ignoredAir.Contains(c.Key));
		bool flag = airSeen.Length == 0;
		uint count = U(num2 + 24);
		uint num9 = U(num2 + 8);
		uint num10 = U(num2 + 20);
		if (count == 0 || count > 65536)
		{
			throw new Exception("空袭实体列表正在变化。");
		}
		byte[] value2 = ShellBlock(Resolve(arena, num9), (int)(count * 8));
		byte[] value3 = ShellBlock(Resolve(arena, num10), (int)(count * 4));
		if (airSeen.Length < count)
		{
			Array.Resize(ref airSeen, (int)count);
		}
		List<AirZone> list2 = new List<AirZone>();
		List<AirPlane> list3 = new List<AirPlane>();
		List<AirZone> list4 = new List<AirZone>();
		uint[] array3 = airCandidates.Keys.Where((uint k) => k >= count).ToArray();
		foreach (uint key in array3)
		{
			airCandidates.Remove(key);
		}
		for (uint num12 = 0u; num12 < count; num12++)
		{
			uint num13 = BitConverter.ToUInt32(value2, (int)(num12 * 8));
			uint num14 = BitConverter.ToUInt32(value3, (int)(num12 * 4));
			ulong num15 = ((ulong)num14 << 32) | num13;
			if (num13 == uint.MaxValue || num14 == 0)
			{
				airSeen[num12] = num15;
				airCandidates.Remove(num12);
				continue;
			}
			if (airSeen[num12] != num15)
			{
				airCandidates.Remove(num12);
				try
				{
					long num16 = Resolve(arena, num13);
					if (U(num16 + 104) != num14 || U(num16 + 108) != num12)
					{
						continue;
					}
					long blueprint = Q(num16 + 56);
					string text2 = BlueprintName(blueprint);
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					if (AirCatalogue.Aircraft(text2) || AirCatalogue.V1(text2))
					{
						airCandidates[num12] = new AirCandidate
						{
							Entity = num16,
							Id = num14,
							Index = num12,
							Blueprint = blueprint,
							Path = text2,
							Born = now,
							NewFlight = !flag
						};
					}
					airSeen[num12] = num15;
					goto IL_06b6;
				}
				catch
				{
				}
				continue;
			}
			goto IL_06b6;
			IL_06b6:
			if (!airCandidates.TryGetValue(num12, out var value4))
			{
				continue;
			}
			try
			{
				byte[] array4 = Read(value4.Entity, 392);
				if (BitConverter.ToUInt32(array4, 104) != num14 || BitConverter.ToUInt32(array4, 108) != num12 || BitConverter.ToInt64(array4, 56) != value4.Blueprint)
				{
					continue;
				}
				if (AirCatalogue.V1(value4.Path))
				{
					uint num17 = BitConverter.ToUInt32(array4, 388);
					if (num17 == uint.MaxValue)
					{
						continue;
					}
					long num18 = Component(value4.Entity, 103319634L);
					if (num18 != 0)
					{
						byte[] array5 = Read(Resolve(arena, num17), 68);
						byte[] ext = Read(num18, 108);
						AirZone airZone = AirLogic.Rocket(array4, array5, ext, image, arena, value4.Entity, num14, num12);
						if (airZone != null && dictionary.TryGetValue(airZone.Owner, out var value5) && U(value4.Entity + 104) == num14 && U(value4.Entity + 388) == num17 && Read(Resolve(arena, num17) + 16, 16).SequenceEqual(array5.Skip(16).Take(16)))
						{
							airZone.Slot = value5.Slot;
							airZone.Colour = value5.Colour;
							list4.Add(airZone);
						}
					}
					continue;
				}
				uint num19 = BitConverter.ToUInt32(array4, 88);
				if (num19 == uint.MaxValue || (BitConverter.ToUInt32(array4, 160) & 0x4000) != 0)
				{
					continue;
				}
				long num20 = Resolve(arena, num19);
				if (!dictionary.ContainsKey(num20) || U(value4.Entity + 88) != num19 || U(value4.Entity + 104) != num14)
				{
					continue;
				}
				if (Loiter49.Supported(value4.Path))
				{
					AirZone airZone2 = ReadLoiter49(value4.Entity, num14, array4, value4.Path, arena);
					if (airZone2 != null)
					{
						airZone2.Owner = num20;
						airZone2.Slot = dictionary[num20].Slot;
						airZone2.Colour = dictionary[num20].Colour;
						list2.Add(airZone2);
					}
					continue;
				}
				if (value4.Recovered == null && value4.NewFlight && now - value4.Born < 2500.0 && (value4.Path == "ebps\\races\\german\\aircraft\\stuka_ju87_strafe" || value4.Path == "ebps\\races\\german\\aircraft\\stuka_ju87_combined_arms_strafe" || value4.Path == "ebps\\races\\american\\aircraft\\p47_thunderbolt_air_support_center_us"))
				{
					try
					{
						uint num21 = BitConverter.ToUInt32(array4, 388);
						if (num21 != uint.MaxValue)
						{
							byte[] array6 = Read(Resolve(arena, num21), 284);
							AirZone airZone3 = AirLogic.RecoverStrafe(value4.Path, array4, array6, image, arena, value4.Entity, num14, num12);
							if (airZone3 != null && U(value4.Entity + 388) == num21 && U(value4.Entity + 104) == num14 && Read(Resolve(arena, num21) + 16, 16).SequenceEqual(array6.Skip(16).Take(16)))
							{
								value4.Recovered = airZone3;
								value4.Recovered.Owner = num20;
								value4.Recovered.Colour = dictionary[num20].Colour;
								value4.Recovered.Slot = dictionary[num20].Slot;
							}
						}
					}
					catch
					{
					}
				}
				float x2 = BitConverter.ToSingle(array4, 44);
				float num22 = BitConverter.ToSingle(array4, 52);
				if (value4.Recovered != null && AirLogic.Passed(value4.Recovered, null, x2, num22))
				{
					value4.Finished = true;
				}
				list3.Add(new AirPlane
				{
					Key = value4.Entity + ":" + num14,
					Path = value4.Path,
					Owner = num20,
					Born = value4.Born,
					Recovered = value4.Recovered,
					Finished = value4.Finished,
					PositionKnown = SquadMarkerLogic.ValidPosition(x2, 0f, num22),
					X = x2,
					Z = num22
				});
			}
			catch
			{
			}
		}
		if (U(num2 + 24) != count || U(num2 + 8) != num9 || U(num2 + 20) != num10)
		{
			throw new Exception("空袭实体列表正在切换。");
		}
		CheckMarkerWorld(match);
		List<AirZone> list5 = airTracker.Update(list, list3, now);
		foreach (AirPlane item2 in list3.Where((AirPlane p) => p.Recovered != null && !p.Finished && now - p.Born >= 600.0 && !airTracker.Claims(p.Key)))
		{
			AirZone z2 = item2.Recovered;
			if (!list5.Any((AirZone r) => r.Owner == z2.Owner && r.Name == z2.Name && Math.Abs(r.X - z2.X) + Math.Abs(r.Z - z2.Z) < 20f))
			{
				list5.Add(z2);
			}
		}
		list5.AddRange(list4);
		list5.AddRange(list2);
		long[] array7 = loiterStates49.Keys.Where((long e) => !airCandidates.Values.Any((AirCandidate c) => c.Entity == e)).ToArray();
		foreach (long key2 in array7)
		{
			loiterStates49.Remove(key2);
		}
		return list5;
	}

	private long BarrageReference(long arena, long registry, uint id, uint index, bool squad)
	{
		if (id == 0 || registry == 0 || index >= U(registry + 24) || index > 1000000)
		{
			return 0L;
		}
		long num = Resolve(arena, U(registry + 20));
		long num2 = Resolve(arena, U(registry + 8));
		if (U(num + index * 4) != id)
		{
			return 0L;
		}
		uint num3 = U(num2 + index * 8);
		if (num3 == uint.MaxValue)
		{
			return 0L;
		}
		long num4 = Resolve(arena, num3);
		if (U(num + index * 4) != id || U(num2 + index * 8) != num3)
		{
			return 0L;
		}
		if (!squad && (U(num4 + 104) != id || U(num4 + 108) != index || (U(num4 + 160) & 0x4000) != 0))
		{
			return 0L;
		}
		return num4;
	}

	private bool ReadBarragePosition(long arena, byte[] targets, int index, out float x, out float y, out float z)
	{
		if (BarrageLogic.Position(targets, index, out x, out y, out z))
		{
			return true;
		}
		int num = index * 20;
		byte b = targets[num];
		if (b != 3 && b != 4)
		{
			return false;
		}
		uint id = BitConverter.ToUInt32(targets, num + 4);
		uint index2 = BitConverter.ToUInt32(targets, num + 8);
		long num2 = Resolve(arena, U(image + 98614988));
		uint num3 = U(num2 + ((b == 3) ? 260 : 256));
		if (num3 == uint.MaxValue)
		{
			return false;
		}
		long num4 = BarrageReference(arena, Resolve(arena, num3), id, index2, b == 4);
		if (num4 == 0)
		{
			return false;
		}
		byte[] value = Read(num4 + 44, 12);
		x = BitConverter.ToSingle(value, 0);
		y = BitConverter.ToSingle(value, 4);
		z = BitConverter.ToSingle(value, 8);
		return BarrageLogic.Valid(x, y, z);
	}

	private void ReadBarrageTarget(long arena, long entity, string path, PlayerInfoRow row, bool allowSelf = false)
	{
		if ((row.Relation != "敌方" && !allowSelf) || !BarrageLogic.Supported(path))
		{
			return;
		}
		long num = Component(entity, 103319626L);
		if (num == 0 || F(num + 108) <= 0f)
		{
			return;
		}
		uint num2 = U(entity + 104);
		uint num3 = U(entity + 108);
		uint num4 = U(entity + 88);
		long num5 = Component(entity, 103319637L);
		if (num5 == 0)
		{
			return;
		}
		uint num6 = U(num5 + 76);
		if (num6 == uint.MaxValue)
		{
			return;
		}
		long num7 = Resolve(arena, num6);
		uint num8 = U(num7 + 44);
		uint num9 = U(num7 + 40);
		switch (num8)
		{
		default:
			throw new InvalidOperationException();
		case 0u:
			break;
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 17u:
		case 18u:
		case 19u:
		case 20u:
		case 21u:
		case 22u:
		case 23u:
		case 24u:
		case 25u:
		case 26u:
		case 27u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
		case 32u:
		case 33u:
		case 34u:
		case 35u:
		case 36u:
		case 37u:
		case 38u:
		case 39u:
		case 40u:
		case 41u:
		case 42u:
		case 43u:
		case 44u:
		case 45u:
		case 46u:
		case 47u:
		case 48u:
		case 49u:
		case 50u:
		case 51u:
		case 52u:
		case 53u:
		case 54u:
		case 55u:
		case 56u:
		case 57u:
		case 58u:
		case 59u:
		case 60u:
		case 61u:
		case 62u:
		case 63u:
		case 64u:
		{
			if (num9 == uint.MaxValue)
			{
				throw new InvalidOperationException();
			}
			byte[] value = Read(Resolve(arena, num9), (int)(num8 * 4));
			List<BarrageRecord> list = new List<BarrageRecord>();
			for (int i = 0; i < num8; i++)
			{
				uint num10 = BitConverter.ToUInt32(value, i * 4);
				if (num10 == uint.MaxValue)
				{
					continue;
				}
				long num11 = Resolve(arena, num10);
				if (Q(num11) != image + 64568064)
				{
					continue;
				}
				byte[] array = Read(num11, 164);
				string text = BlueprintName(BitConverter.ToInt64(array, 56));
				if (!BarrageCatalogue.Ability(path, text) || !BarrageLogic.Active(text, array))
				{
					continue;
				}
				uint num12 = BitConverter.ToUInt32(array, 8);
				uint num13 = BitConverter.ToUInt32(array, 12);
				if (num13 == 0)
				{
					continue;
				}
				if (num13 > 64 || num12 == uint.MaxValue)
				{
					throw new InvalidOperationException();
				}
				long a = Resolve(arena, num12);
				byte[] array2 = Read(a, (int)(num13 * 20));
				List<BarrageRecord> list2 = new List<BarrageRecord>();
				for (int j = 0; j < num13; j++)
				{
					if (ReadBarragePosition(arena, array2, j, out var x, out var y, out var z))
					{
						string text2 = entity + ":" + num2 + ":" + num11 + ":" + text;
						list2.Add(new BarrageRecord
						{
							Key = text2 + ":" + j,
							CastKey = text2,
							Name = UnitNames.Name(BarrageCatalogue.Squad(path)),
							Ability = text,
							Player = row.Name,
							Slot = row.Slot,
							X = x,
							Y = y,
							Z = z,
							Colour = (row.GameColourKnown ? row.GameColour : Color.Red)
						});
					}
				}
				byte[] array3 = Read(num11, 164);
				if (BitConverter.ToInt64(array3, 0) != image + 64568064 || BitConverter.ToInt64(array3, 56) != BitConverter.ToInt64(array, 56) || BitConverter.ToUInt32(array3, 8) != num12 || BitConverter.ToUInt32(array3, 12) != num13 || !BarrageLogic.Active(text, array3))
				{
					throw new InvalidOperationException();
				}
				if (!Read(a, array2.Length).SequenceEqual(array2))
				{
					throw new InvalidOperationException();
				}
				list.AddRange(list2);
			}
			if (U(entity + 104) != num2 || U(entity + 108) != num3 || U(entity + 88) != num4 || U(num5 + 76) != num6 || U(num7 + 44) != num8 || U(num7 + 40) != num9)
			{
				throw new InvalidOperationException();
			}
			row.Barrages.AddRange(list);
			break;
		}
		}
	}

	private void ReadDirection(SquadMarker marker, SquadMarkerSource source)
	{
		bool flag = source.Path.Contains("\\emplacements\\") && (source.Path.Contains("at_gun") || source.Path.Contains("anti_tank"));
		bool flag2 = source.Path.Contains("\\team_weapons\\") || flag;
		bool flag3 = source.Path.Contains("bunker");
		if (!marker.Vehicle && !flag2 && !flag3)
		{
			return;
		}
		foreach (SquadMarkerMember member in source.Members)
		{
			try
			{
				long entity = member.Entity;
				byte[] value = Read(entity, 128);
				if (BitConverter.ToUInt32(value, 96) != member.Generation || BitConverter.ToUInt32(value, 124) != member.SquadOffset || Resolve(markerArena, BitConverter.ToUInt32(value, 88)) != member.Owner)
				{
					continue;
				}
				long num = Component(entity, 103319626L);
				if (num == 0 || F(num + 108) <= 0f)
				{
					continue;
				}
				long num2 = 0L;
				long num3 = Component(entity, 103319697L);
				if (num3 != 0)
				{
					uint num4 = U(num3 + 16);
					if (num4 != uint.MaxValue)
					{
						num2 = Resolve(markerArena, num4);
					}
				}
				if (num2 == 0)
				{
					long num5 = Component(entity, 103319632L);
					if (num5 != 0 && U(num5 + 20) != 0 && U(num5 + 20) < 32)
					{
						long num6 = Resolve(markerArena, U(Resolve(markerArena, U(num5 + 16))));
						uint num7 = U(num6 + 240);
						if (num7 != uint.MaxValue)
						{
							num2 = Resolve(markerArena, num7);
						}
					}
				}
				if (num2 == 0)
				{
					continue;
				}
				string text = BlueprintName(Q(num2 + 184));
				bool flag4 = source.Path.Contains("at_gun") || source.Path.Contains("anti_tank");
				bool flag5 = text.Contains("machine_gun") || text.Contains("hmg");
				bool flag6 = text.Contains("\\anti_tank_gun\\") || (flag4 && text.Contains("\\anti_aircraft_gun\\"));
				if (flag4 && !flag6)
				{
					continue;
				}
				if (!flag3 && !flag)
				{
					goto IL_02b9;
				}
				if (flag5 || flag6)
				{
					long num8 = Component(entity, 103319627L);
					if (num8 == 0 || U(num8 + 40) == 0)
					{
						goto IL_02b9;
					}
				}
				goto end_IL_008c;
				IL_02b9:
				float num9 = BitConverter.ToSingle(value, 32);
				float num10 = BitConverter.ToSingle(value, 40);
				float num11 = (float)Math.Sqrt(num9 * num9 + num10 * num10);
				if (float.IsNaN(num11) || (double)num11 < 0.5 || (double)num11 > 1.5)
				{
					continue;
				}
				float num12 = F(num2 + 404);
				if (num12 <= 0f || num12 > 500f)
				{
					continue;
				}
				DirectionData directionData = new DirectionData();
				directionData.X = BitConverter.ToSingle(value, 44);
				directionData.Y = BitConverter.ToSingle(value, 48);
				directionData.Z = BitConverter.ToSingle(value, 52);
				directionData.DX = num9 / num11;
				directionData.DZ = num10 / num11;
				directionData.Range = num12;
				directionData.Team = flag2;
				directionData.MachineGun = flag5;
				directionData.AntiTankGun = flag6;
				directionData.Artillery = DirectionDrawing.Artillery(source.Path, text);
				directionData.Ready = flag || !flag2 || (Read(num2 + 438, 1)[0] & 4) != 0;
				DirectionData directionData2 = directionData;
				if (flag5 && (flag2 || flag3))
				{
					try
					{
						long num13 = Q(num2 + 312);
						byte[] value2 = Read(num13 + 280, 28);
						DirectionDrawing.SetArc(directionData2, BitConverter.ToSingle(value2, 0), BitConverter.ToSingle(value2, 4), BitConverter.ToSingle(value2, 24));
					}
					catch
					{
					}
				}
				marker.Direction = directionData2;
				break;
				end_IL_008c:;
			}
			catch
			{
			}
		}
	}

	private void TrackInfrastructure49(long arena, long entity, string path, PlayerInfoRow row)
	{
		if (!(row.Relation != "敌方") && Infrastructure49.Name(path) != null)
		{
			long num = Component(entity, 103319626L);
			if (num != 0 && !(F(num + 108) <= 0f))
			{
				SquadMarkerSource squadMarkerSource = new SquadMarkerSource();
				squadMarkerSource.Squad = entity;
				squadMarkerSource.Identity = Q(entity + 104);
				squadMarkerSource.Path = path;
				squadMarkerSource.Slot = row.Slot;
				SquadMarkerSource squadMarkerSource2 = squadMarkerSource;
				squadMarkerSource2.Members.Add(new SquadMarkerMember
				{
					Entity = entity,
					Owner = row.PlayerAddress,
					Generation = U(entity + 96),
					SquadOffset = U(entity + 124)
				});
				row.MarkerSquads.Add(squadMarkerSource2);
			}
		}
	}

	private void BindLocalAccount59(List<PlayerInfoRow> rows, long identity)
	{
		PlayerInfoRow[] array = rows.Where((PlayerInfoRow p) => p.PlayerIdentity == identity && p.Relation == "自己").ToArray();
		if (array.Length != 1)
		{
			ClearLocalAccount59();
			return;
		}
		if (DateTime.UtcNow >= accountNext59)
		{
			steamAccount59 = LocalAccountLog59.Read(LocalAccountLog59.DefaultPath, started);
			accountNext59 = DateTime.UtcNow.AddSeconds(steamAccount59.HasValue ? 10 : 2);
		}
		array[0].IsLocalPlayer = true;
		array[0].SteamId64 = steamAccount59;
		localAccount59 = new LocalPlayerAccount59
		{
			ProcessId = process.Id,
			ProcessStarted = started,
			Match = MatchKey,
			Slot = array[0].Slot,
			PlayerIdentity = identity,
			SteamId64 = steamAccount59
		};
	}

	private void ClearLocalAccount59()
	{
		localAccount59 = null;
		steamAccount59 = null;
		accountNext59 = DateTime.MinValue;
	}

	private AirZone ReadLoiter49(long entity, uint id, byte[] body, string path, long arena)
	{
		if (!Loiter49.Supported(path))
		{
			return null;
		}
		if (!loiterStates49.TryGetValue(entity, out var value) || value.Id != id)
		{
			value = (loiterStates49[entity] = new LoiterState49
			{
				Id = id
			});
		}
		uint num = BitConverter.ToUInt32(body, 388);
		if (num == uint.MaxValue)
		{
			return null;
		}
		byte[] array = Read(Resolve(arena, num), 284);
		if (BitConverter.ToInt64(array, 0) != image + 64559016 || BitConverter.ToUInt32(array, 36) + arena != entity || BitConverter.ToUInt32(array, 64) != 1)
		{
			return null;
		}
		float px = BitConverter.ToSingle(body, 44);
		float pz = BitConverter.ToSingle(body, 52);
		byte b = array[16];
		if ((b != 2 && b != 4) || !ReadBarragePosition(arena, array.Skip(16).Take(20).ToArray(), 0, out var x, out var y, out var z))
		{
			return null;
		}
		float length = 0f;
		float width = 0f;
		long num2 = Component(entity, 103319632L);
		if (num2 != 0 && U(num2 + 20) != 0 && U(num2 + 20) < 32)
		{
			long num3 = Resolve(arena, U(Resolve(arena, U(num2 + 16))));
			uint num4 = U(num3 + 240);
			if (num4 != uint.MaxValue)
			{
				long num5 = Resolve(arena, num4);
				LoiterWeaponCatalogue49.Shape(BlueprintName(Q(num5 + 184)), out length, out width);
			}
		}
		string name = (path.Contains("anti_vehicle") ? "斯图卡反坦克巡航" : "斯图卡巡航");
		Loiter49.Observe(value, b == 4, x, y, z, px, pz, BitConverter.ToSingle(array, 272), BitConverter.ToSingle(array, 280), length, width, entity + ":" + id, name);
		return value.Zone;
	}

	private void LoadMiniHint52()
	{
		lock (miniGate)
		{
			if (miniCacheRead52 || miniDisposed)
			{
				return;
			}
			miniCacheRead52 = true;
			try
			{
				long num = MiniHintCache52.Parse(File.ReadAllText(MiniHintPath52), process.Id, started.ToUniversalTime().Ticks, image);
				if (num != 0 && miniHint == 0)
				{
					miniHint = num;
				}
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
	}

	private void SaveMiniHint52(long found)
	{
		if (found == 0 || found == savedMiniHint52)
		{
			return;
		}
		try
		{
			File.WriteAllText(MiniHintPath52, process.Id + "|" + started.ToUniversalTime().Ticks + "|" + image + "|" + found);
			savedMiniHint52 = found;
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}

	public MineCamera ReadMineCamera()
	{
		try
		{
			if (cameraCore == 0 || Q(cameraCore) != image + 64684536)
			{
				cameraCore = 0L;
				long num = Q(Q(image + 102260680) + 184);
				long num2 = Q(num);
				long num3 = Q(num + 8);
				if (num3 < num2 || num3 - num2 > 8192)
				{
					throw new Exception("镜头列表正在更新。");
				}
				for (long num4 = num2; num4 < num3; num4 += 8)
				{
					long num5 = Q(num4);
					if (num5 < 65576)
					{
						continue;
					}
					try
					{
						if (Q(num5 - 40) == image + 64684536)
						{
							cameraCore = num5 - 40;
							break;
						}
					}
					catch
					{
					}
				}
			}
			if (cameraCore == 0)
			{
				throw new Exception("未找到镜头对象。");
			}
			long num6 = Q(cameraCore + 416);
			byte[] value = Read(num6, 816);
			MineCamera mineCamera = new MineCamera();
			mineCamera.View = new float[16];
			mineCamera.Projection = new float[16];
			MineCamera mineCamera2 = mineCamera;
			for (int i = 0; i < 16; i++)
			{
				mineCamera2.View[i] = BitConverter.ToSingle(value, 88 + i * 4);
				mineCamera2.Projection[i] = BitConverter.ToSingle(value, 752 + i * 4);
				if (float.IsNaN(mineCamera2.View[i]) || float.IsInfinity(mineCamera2.View[i]) || float.IsNaN(mineCamera2.Projection[i]) || float.IsInfinity(mineCamera2.Projection[i]))
				{
					throw new Exception("镜头数值无效。");
				}
			}
			if ((double)Math.Abs(mineCamera2.View[15] - 1f) > 0.01 || mineCamera2.Projection[0] <= 0f || mineCamera2.Projection[5] <= 0f || (double)Math.Abs(mineCamera2.Projection[11] - 1f) > 0.01)
			{
				throw new Exception("镜头格式不匹配。");
			}
			for (int j = 0; j < 3; j++)
			{
				float num7 = mineCamera2.View[12 + j];
				for (int k = 0; k < 3; k++)
				{
					num7 += BitConverter.ToSingle(value, 72 + k * 4) * mineCamera2.View[k * 4 + j];
				}
				if ((double)Math.Abs(num7) > 0.1)
				{
					throw new Exception("镜头正在切换。");
				}
			}
			if (Q(cameraCore + 416) != num6)
			{
				throw new Exception("镜头已切换。");
			}
			return mineCamera2;
		}
		catch
		{
			cameraCore = 0L;
			throw;
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern UIntPtr VirtualQueryEx(ProcHandle process, IntPtr address, out MiniMemoryRegion result, UIntPtr length);

	private void ResetMinimapForMatch(string nextMatch)
	{
		lock (miniGate)
		{
			if (!(miniMatch == nextMatch))
			{
				miniMatch = nextMatch;
				miniEpoch++;
				tacticalControl51 = 0L;
				tacticalRetry51 = DateTime.MinValue;
				if (miniControl != 0)
				{
					miniHint = miniControl;
				}
				miniControl = 0L;
				miniRetry = DateTime.MinValue;
				miniFailures = 0;
				miniProgress.Reset();
				miniSmallPass = true;
				miniCandidateCursor = 0;
				miniCandidates.Clear();
				MinimapStatus = "新对局小地图定位中…";
				MinimapDiagnostic = "";
			}
		}
	}

	private byte[] MiniProperties(long node, byte[] body)
	{
		int num = BitConverter.ToInt32(body, 124) & 0x3FF;
		long num2 = BitConverter.ToInt64(body, 112);
		if (num == 0)
		{
			return new byte[0];
		}
		byte[] result = Read(num2, num * 16);
		if (Q(node + 112) != num2 || (U(node + 124) & 0x3FF) != num)
		{
			throw new Exception("小地图界面属性正在更新。");
		}
		return result;
	}

	private long MiniProperty(byte[] data, int key)
	{
		for (int i = 0; i < data.Length; i += 16)
		{
			int num = BitConverter.ToUInt16(data, i + 2);
			if (num > key)
			{
				return 0L;
			}
			if (num == key)
			{
				int num2 = BitConverter.ToUInt16(data, i);
				long num3 = BitConverter.ToInt64(data, i + 8);
				if (num3 == 0)
				{
					return 0L;
				}
				if ((num2 & 0x70) != 0)
				{
					num3 = Q(num3 + (((num2 & 0x40) != 0) ? 48 : (((num2 & 0x20) != 0) ? 56 : 40)));
				}
				else if ((num2 & 0x100) != 0)
				{
					throw new Exception("小地图界面属性尚未求值。");
				}
				return num3;
			}
		}
		return 0L;
	}

	private int MiniPropertyId(long slot)
	{
		long num = Q(image + slot);
		return BitConverter.ToUInt16(Read(num + 184, 2), 0);
	}

	private float MiniDimension(byte[] properties, int key)
	{
		long num = MiniProperty(properties, key);
		if (num < 65536)
		{
			throw new Exception("小地图尺寸绑定尚未就绪。");
		}
		float num2 = F(num + 40);
		if (num2 < 8f || num2 > 20000f)
		{
			throw new Exception("小地图世界尺寸无效。");
		}
		return num2;
	}

	private MinimapAffine MiniTransform(long transform)
	{
		switch (Q(transform) - image)
		{
		case 64432864L:
		{
			if (Read(transform + 248, 1)[0] != 0)
			{
				throw new Exception("小地图旋转矩阵正在更新。");
			}
			byte[] array = Read(transform + 216, 32);
			byte[] b = Read(transform + 216, 32);
			if (!ByteMemory45.Equal(array, b) || Read(transform + 248, 1)[0] != 0)
			{
				throw new Exception("小地图旋转矩阵正在更新。");
			}
			return MinimapAffine.Native(array);
		}
		case 64431008L:
		{
			byte[] body = Read(transform, 128);
			byte[] data = MiniProperties(transform, body);
			int key = MiniPropertyId(111878264L);
			long num10 = MiniProperty(data, key);
			if (num10 == 0)
			{
				throw new Exception("小地图缩放矩阵尚未就绪。");
			}
			return MinimapAffine.Native(Read(num10 + 40, 32));
		}
		case 64815280L:
		{
			float num5 = F(transform + 208);
			float num6 = F(transform + 212);
			float num7 = F(transform + 216);
			double num8 = Math.Cos((double)num7 * Math.PI / 180.0);
			double num9 = Math.Sin((double)num7 * Math.PI / 180.0);
			return new MinimapAffine
			{
				XX = num8,
				XY = 0.0 - num9,
				YX = num9,
				YY = num8,
				TX = (double)num5 * (1.0 - num8) + (double)num6 * num9,
				TY = (double)num6 * (1.0 - num8) - (double)num5 * num9
			};
		}
		case 64815656L:
		{
			float num = F(transform + 208);
			float num2 = F(transform + 212);
			float num3 = F(transform + 216);
			float num4 = F(transform + 220);
			return new MinimapAffine
			{
				XX = num3,
				YY = num4,
				TX = num * (1f - num3),
				TY = num2 * (1f - num4)
			};
		}
		case 64430720L:
			return MinimapAffine.Translation(F(transform + 208), F(transform + 212));
		default:
			throw new Exception("遇到尚未验证的小地图界面变换，暂不绘制。");
		}
	}

	private MinimapFrame ReadMinimapLayout(long control)
	{
		if (control < 65536 || Q(control) != image + 65511760)
		{
			throw new MinimapRetiredControl("小地图控件已切换。");
		}
		byte[] array = Read(control, 480);
		byte[] properties = MiniProperties(control, array);
		float ww = MiniDimension(properties, MiniPropertyId(111989640L));
		float wh = MiniDimension(properties, MiniPropertyId(111989680L));
		long num = BitConverter.ToInt64(array, 472);
		if (num < 65536 || Q(num + 136) != control)
		{
			throw new MinimapRetiredControl("小地图模板尚未连接。");
		}
		byte[] value = Read(num, 344);
		float num2 = BitConverter.ToSingle(value, 296);
		float num3 = BitConverter.ToSingle(value, 300);
		if (num2 < 10f || num3 < 10f || num2 > 4000f || num3 > 4000f)
		{
			throw new Exception("小地图尚未完成布局。");
		}
		MinimapAffine localToUi = MinimapAffine.Identity;
		long num4 = num;
		int num5 = 0;
		HashSet<long> hashSet = new HashSet<long>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		float num6 = 0f;
		float num7 = 0f;
		while (num4 != 0)
		{
			if (!hashSet.Add(num4) || num5++ >= 64)
			{
				throw new MinimapRetiredControl("小地图界面层级已变化。");
			}
			byte[] array2 = Read(num4, 344);
			long num8 = BitConverter.ToInt64(array2, 0) - image;
			if (num8 == 65713904)
			{
				flag = true;
			}
			long num9 = MiniProperty(MiniProperties(num4, array2), 40);
			if (flag3 && num9 == 0)
			{
				throw new Exception("小地图实际显示矩阵尚未就绪。");
			}
			flag3 = false;
			if (num9 != 0)
			{
				localToUi = localToUi.Then(MiniTransform(num9));
			}
			float num10 = BitConverter.ToSingle(array2, 168);
			float num11 = BitConverter.ToSingle(array2, 172);
			if (!SquadMarkerLogic.ValidPosition(num10, 0f, num11))
			{
				throw new Exception("小地图界面偏移无效。");
			}
			localToUi = localToUi.Then(MinimapAffine.Translation(num10, num11));
			long num12 = BitConverter.ToInt64(array2, 136);
			if (Q(num4 + 136) != num12)
			{
				throw new Exception("小地图父控件正在变化。");
			}
			if (num12 == 0)
			{
				flag2 = num8 == 64823200;
				num6 = BitConverter.ToSingle(array2, 296);
				num7 = BitConverter.ToSingle(array2, 300);
			}
			num4 = num12;
		}
		if (!flag || !flag2)
		{
			throw new MinimapRetiredControl("小地图已离开当前对局界面。");
		}
		if (num6 < 200f || num7 < 200f || num6 > 20000f || num7 > 20000f)
		{
			throw new Exception("游戏界面尺寸尚未就绪。");
		}
		if (Q(control + 472) != num || Q(control) != image + 65511760)
		{
			throw new MinimapRetiredControl("小地图已切换，等待重新定位。");
		}
		MinimapFrame minimapFrame = MinimapAffine.Frame(ww, wh, num2, num3, num6, num7, localToUi);
		if (!minimapFrame.Valid || minimapFrame.CentreX < 0f || minimapFrame.CentreX > num6 || minimapFrame.CentreY < 0f || minimapFrame.CentreY > num7)
		{
			throw new Exception("小地图实际位置无效。");
		}
		return minimapFrame;
	}

	public bool TryReadMinimap(out MinimapFrame frame)
	{
		frame = null;
		long num;
		long num2;
		lock (miniGate)
		{
			num = miniControl;
			num2 = miniEpoch;
		}
		if (num == 0)
		{
			BeginMinimapDiscovery();
			return false;
		}
		try
		{
			CheckMarkerWorld(MatchKey);
			MinimapFrame minimapFrame = ReadMinimapLayout(num);
			CheckMarkerWorld(MatchKey);
			lock (miniGate)
			{
				if (miniDisposed || miniEpoch != num2)
				{
					return false;
				}
				miniFailures = 0;
			}
			frame = minimapFrame;
			MinimapStatus = "";
			return true;
		}
		catch (Exception ex)
		{
			MinimapStatus = ex.Message;
			bool flag = false;
			lock (miniGate)
			{
				if (miniEpoch != num2)
				{
					return false;
				}
				if (ex is MinimapRetiredControl || ++miniFailures >= 8)
				{
					if (miniControl == num)
					{
						miniControl = 0L;
					}
					flag = true;
				}
			}
			if (flag)
			{
				BeginMinimapDiscovery();
			}
			return false;
		}
	}

	public void RecoverMinimap()
	{
		lock (miniGate)
		{
			if (miniDisposed)
			{
				return;
			}
			miniEpoch++;
			miniControl = 0L;
			tacticalControl51 = 0L;
			tacticalRetry51 = DateTime.MinValue;
			miniFailures = 0;
			miniRetry = DateTime.MinValue;
			miniProgress.Reset();
			miniSmallPass = true;
			MinimapStatus = "正在重新连接小地图…";
		}
		BeginMinimapDiscovery();
	}

	private void BeginMinimapDiscovery()
	{
		if (!LicenseGate60.Allowed)
		{
			return;
		}
		LoadMiniHint52();
		long epoch;
		long token;
		long hint;
		lock (miniGate)
		{
			if (miniDisposed || miniScanning || string.IsNullOrEmpty(miniMatch) || DateTime.UtcNow < miniRetry)
			{
				return;
			}
			miniScanning = true;
			epoch = miniEpoch;
			token = (miniActiveScan = ++miniScanSerial);
			hint = miniHint;
			miniRetry = DateTime.UtcNow.AddMilliseconds(100.0);
		}
		try
		{
			if (ThreadPool.QueueUserWorkItem(delegate
			{
				long num = 0L;
				long num2 = 0L;
				Stopwatch stopwatch = Stopwatch.StartNew();
				string text = "";
				bool flag = false;
				try
				{
					List<long> list = new List<long>();
					lock (miniGate)
					{
						if (hint != 0)
						{
							list.Add(hint);
						}
						long[] array = (from x in miniCandidates
							where DateTime.UtcNow - x.Value < TimeSpan.FromSeconds(60.0) && x.Key != hint
							select x.Key).ToArray();
						if (array.Length > 0)
						{
							int num3 = miniCandidateCursor % array.Length;
							list.AddRange(array.Skip(num3).Concat(array.Take(num3)).Take(4));
							miniCandidateCursor = (num3 + 4) % array.Length;
						}
					}
					num = MiniTree53();
					foreach (long item in list)
					{
						if (num != 0)
						{
							break;
						}
						lock (miniGate)
						{
							if (miniDisposed || miniEpoch != epoch)
							{
								return;
							}
						}
						try
						{
							MinimapFrame minimapFrame = ReadMinimapLayout(item);
							if ((double)minimapFrame.CentreX < (double)minimapFrame.RootWidth * 0.5 && (double)minimapFrame.CentreY > (double)minimapFrame.RootHeight * 0.45)
							{
								num = item;
								break;
							}
						}
						catch
						{
						}
					}
					long num4;
					lock (miniGate)
					{
						if (miniDisposed || miniEpoch != epoch)
						{
							return;
						}
						if (!miniProgress.Initialized)
						{
							long val = 0L;
							try
							{
								val = Q(Q(image + 112837064) + 136) & -16777216;
							}
							catch
							{
							}
							miniProgress.Begin(Math.Max(65536L, val));
						}
						num4 = miniProgress.Cursor;
					}
					byte[] array2 = new byte[65536];
					long num5 = image + 65511760;
					while (num == 0 && stopwatch.ElapsedMilliseconds < 80 && num2 < 33554432)
					{
						lock (miniGate)
						{
							if (miniDisposed || miniEpoch != epoch)
							{
								return;
							}
						}
						if (VirtualQueryEx(handle, new IntPtr(num4), out var result, (UIntPtr)(ulong)Marshal.SizeOf(typeof(MiniMemoryRegion))) == UIntPtr.Zero)
						{
							lock (miniGate)
							{
								if (miniEpoch != epoch)
								{
									return;
								}
								flag = miniProgress.Wrap();
								if (flag)
								{
									if (miniSmallPass)
									{
										miniSmallPass = false;
										flag = false;
									}
									else
									{
										miniSmallPass = true;
									}
								}
								num4 = miniProgress.Cursor;
							}
							break;
						}
						long num6 = Math.Max(num4, result.Base.ToInt64());
						long num7 = checked(result.Base.ToInt64() + (long)result.Size.ToUInt64());
						if (num7 <= num4)
						{
							break;
						}
						if (result.State == 4096 && result.Type == 131072 && (result.Protect & 0x101) == 0 && (result.Protect & 0xEE) != 0 && (!miniSmallPass || result.Size.ToUInt64() <= 4194304))
						{
							long num8;
							for (num8 = num6; num8 < num7; num8 += array2.Length)
							{
								if (num != 0)
								{
									break;
								}
								if (stopwatch.ElapsedMilliseconds >= 80)
								{
									break;
								}
								if (num2 >= 33554432)
								{
									break;
								}
								lock (miniGate)
								{
									if (miniDisposed || miniEpoch != epoch)
									{
										return;
									}
								}
								int num9 = (int)Math.Min(array2.Length, num7 - num8);
								Native.ReadProcessMemory(handle, new IntPtr(num8), array2, (UIntPtr)(ulong)num9, out var read);
								int num10 = (int)Math.Min((ulong)num9, read.ToUInt64());
								num2 += num9;
								if (num10 >= 8)
								{
									for (int num11 = 0; num11 + 8 <= num10; num11 += 8)
									{
										if (array2[num11] == (byte)num5 && BitConverter.ToInt64(array2, num11) == num5)
										{
											long num12 = num8 + num11;
											lock (miniGate)
											{
												if (miniEpoch != epoch)
												{
													return;
												}
												if (miniCandidates.Count >= 64)
												{
													long key = miniCandidates.OrderBy((KeyValuePair<long, DateTime> x) => x.Value).First().Key;
													miniCandidates.Remove(key);
												}
												miniCandidates[num12] = DateTime.UtcNow;
											}
											try
											{
												MinimapFrame minimapFrame2 = ReadMinimapLayout(num12);
												if ((double)minimapFrame2.CentreX < (double)minimapFrame2.RootWidth * 0.5 && (double)minimapFrame2.CentreY > (double)minimapFrame2.RootHeight * 0.45)
												{
													num = num12;
													break;
												}
											}
											catch
											{
											}
										}
									}
								}
							}
							num4 = Math.Min(num8, num7);
						}
						else
						{
							num4 = num7;
						}
						lock (miniGate)
						{
							if (miniEpoch != epoch)
							{
								return;
							}
							flag = miniProgress.Advance(num4);
							if (flag)
							{
								if (miniSmallPass)
								{
									miniSmallPass = false;
									flag = false;
								}
								else
								{
									miniSmallPass = true;
								}
								num4 = miniProgress.Cursor;
							}
						}
						if (flag)
						{
							break;
						}
					}
					lock (miniGate)
					{
						if (!miniDisposed && miniEpoch == epoch)
						{
							miniControl = num;
							if (num != 0)
							{
								miniHint = num;
								miniFailures = 0;
								MinimapStatus = "";
							}
							else
							{
								miniRetry = DateTime.UtcNow.AddMilliseconds(flag ? 2000 : 100);
								MinimapStatus = "小地图定位中（自动继续搜索）…";
							}
						}
					}
					if (num != 0)
					{
						SaveMiniHint52(num);
						CacheMiniRoot53(num);
					}
				}
				catch (Exception ex)
				{
					text = ex.Message;
				}
				finally
				{
					lock (miniGate)
					{
						if (miniEpoch == epoch)
						{
							MinimapDiagnostic = "nativeControl=" + num.ToString("x") + " cursor=" + miniProgress.Cursor.ToString("x") + " batchMB=" + num2 / 1048576 + " ms=" + stopwatch.ElapsedMilliseconds + " " + text;
						}
						if (miniActiveScan == token)
						{
							miniScanning = false;
						}
					}
				}
			}))
			{
				return;
			}
			lock (miniGate)
			{
				if (miniActiveScan == token)
				{
					miniScanning = false;
				}
			}
		}
		catch
		{
			lock (miniGate)
			{
				if (miniActiveScan == token)
				{
					miniScanning = false;
				}
			}
		}
	}

	private void StopMinimapDiscovery()
	{
		lock (miniGate)
		{
			miniDisposed = true;
			miniEpoch++;
			miniControl = (miniHint = 0L);
		}
	}

	private IEnumerable<long> MiniChildren53(long node)
	{
		HashSet<long> hashSet = new HashSet<long>(TacticalChildren51(node));
		try
		{
			byte[] array = Read(node, 576);
			for (int i = 144; i + 8 <= array.Length; i += 8)
			{
				long num = BitConverter.ToInt64(array, i);
				if (num < 65536 || num > 140737488355327L || num % 8 != 0 || num == node)
				{
					continue;
				}
				try
				{
					if (Q(num + 136) == node)
					{
						long num2 = Q(num) - image;
						if (num2 >= 64307200 && num2 < 96444416)
						{
							hashSet.Add(num);
						}
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
		return hashSet;
	}

	private void CacheMiniRoot53(long control)
	{
		try
		{
			long num = control;
			HashSet<long> hashSet = new HashSet<long>();
			while (num != 0 && hashSet.Count < 64 && hashSet.Add(num))
			{
				long num2 = Q(num + 136);
				if (num2 == 0)
				{
					if (Q(num) == image + 64823200)
					{
						miniRoot53 = num;
						File.WriteAllText(MiniRootPath53, process.Id + "|" + started.ToUniversalTime().Ticks + "|" + image + "|" + num);
					}
					break;
				}
				num = num2;
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch
		{
		}
	}

	private long MiniTree53()
	{
		lock (miniGate)
		{
			if (miniDisposed)
			{
				return 0L;
			}
			if (treeEpoch53 != miniEpoch)
			{
				treeEpoch53 = miniEpoch;
				ResetMiniTree53();
			}
		}
		if (!miniRootLoaded53)
		{
			miniRootLoaded53 = true;
			try
			{
				miniRoot53 = MiniHintCache52.Parse(File.ReadAllText(MiniRootPath53), process.Id, started.ToUniversalTime().Ticks, image);
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}
		try
		{
			long num = Q(Q(image + 112837064) + 136);
			if (num > 65536 && Q(num) == image + 64823200 && num != miniRoot53)
			{
				miniRoot53 = num;
				miniNodes53.Clear();
				miniSeen53.Clear();
			}
			if (miniRoot53 == 0)
			{
				long num2;
				lock (miniGate)
				{
					num2 = miniHint;
				}
				if (num2 != 0)
				{
					CacheMiniRoot53(num2);
				}
			}
			if (miniRoot53 == 0 || Q(miniRoot53) != image + 64823200)
			{
				return 0L;
			}
			if (miniNodes53.Count == 0)
			{
				if (DateTime.UtcNow < miniTreeRetry53)
				{
					return 0L;
				}
				miniSeen53.Clear();
				miniNodes53.Enqueue(miniRoot53);
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			while (miniNodes53.Count > 0 && stopwatch.ElapsedMilliseconds < 25 && miniSeen53.Count < 6000)
			{
				long num3 = miniNodes53.Dequeue();
				if (!miniSeen53.Add(num3))
				{
					continue;
				}
				try
				{
					if (Q(num3) == image + 65511760)
					{
						lock (miniGate)
						{
							miniCandidates[num3] = DateTime.UtcNow;
						}
						try
						{
							MinimapFrame minimapFrame = ReadMinimapLayout(num3);
							if ((double)minimapFrame.CentreX < (double)minimapFrame.RootWidth * 0.5 && (double)minimapFrame.CentreY > (double)minimapFrame.RootHeight * 0.45)
							{
								miniNodes53.Clear();
								miniTreeRetry53 = DateTime.UtcNow.AddSeconds(1.0);
								return num3;
							}
						}
						catch
						{
						}
						continue;
					}
					foreach (long item in MiniChildren53(num3))
					{
						miniNodes53.Enqueue(item);
					}
				}
				catch
				{
				}
			}
			if (miniNodes53.Count == 0 || miniSeen53.Count >= 6000)
			{
				miniNodes53.Clear();
				miniTreeRetry53 = DateTime.UtcNow.AddMilliseconds(500.0);
			}
			return 0L;
		}
		catch
		{
			miniNodes53.Clear();
			miniRoot53 = 0L;
			return 0L;
		}
	}

	private void ResetMiniTree53()
	{
		miniNodes53.Clear();
		miniSeen53.Clear();
		miniTreeRetry53 = DateTime.MinValue;
	}

	private void ReadMovement(SquadMarker marker, SquadMarkerSource source)
	{
		if (!marker.Vehicle && !FlankReadMovement47)
		{
			return;
		}
		foreach (SquadMarkerMember member in source.Members)
		{
			try
			{
				long entity = member.Entity;
				byte[] value = Read(entity, 128);
				if (BitConverter.ToUInt32(value, 96) != member.Generation || BitConverter.ToUInt32(value, 124) != member.SquadOffset || Resolve(markerArena, BitConverter.ToUInt32(value, 88)) != member.Owner)
				{
					continue;
				}
				long num = Component(entity, 103319626L);
				if (num == 0 || F(num + 108) <= 0f)
				{
					continue;
				}
				uint num2 = U(entity + 388);
				if (num2 == uint.MaxValue)
				{
					continue;
				}
				long num3 = Resolve(markerArena, num2);
				long num4 = Q(num3);
				if (Q(num4 + 344) != image + 13437424)
				{
					continue;
				}
				byte[] array = Read(num3, 180);
				if (BitConverter.ToUInt32(array, 64) != 0)
				{
					continue;
				}
				uint num5 = BitConverter.ToUInt32(array, 172);
				if (num5 != uint.MaxValue)
				{
					long num6 = Resolve(markerArena, num5);
					byte[] array2 = Read(num6, 148);
					MovementData movementData = MovementLogic.Decode(array, array2);
					if (movementData != null && U(entity + 96) == member.Generation && U(entity + 388) == num2 && U(num3 + 172) == num5 && U(num3 + 8) == 0 && U(num6 + 144) == BitConverter.ToUInt32(array2, 144) && Read(num3 + 76, 16).SequenceEqual(array.Skip(76).Take(16)))
					{
						marker.Movement = movementData;
						break;
					}
				}
			}
			catch
			{
			}
		}
	}

	private bool IsUiColourConfig76(long candidate)
	{
		try
		{
			long num = Q(candidate);
			if (num < image || num > image + 268435456)
			{
				return false;
			}
			long num2 = Q(num - 8);
			if (num2 < image || num2 > image + 268435456 || U(num2) != 1)
			{
				return false;
			}
			uint num3 = U(num2 + 12);
			if (num3 > 268435456)
			{
				return false;
			}
			byte[] bytes = Encoding.ASCII.GetBytes(".?AVUIInfo@Tuning@@\0");
			return ByteMemory45.Equal(Read(image + num3 + 16, bytes.Length), bytes);
		}
		catch
		{
			return false;
		}
	}

	private long UiColourConfig76(long item)
	{
		long num = Q(item + 296);
		long num2 = Q(num);
		long num3 = Q(num + 8);
		if (num2 < 65536 || num3 <= num2 || num3 - num2 > 32768 || (num3 - num2) % 8 != 0)
		{
			throw new Exception("配色配置列表无效。");
		}
		long num4 = 0L;
		for (long num5 = num2; num5 < num3; num5 += 8)
		{
			long num6 = Q(num5);
			if (num6 >= 65536 && IsUiColourConfig76(num6))
			{
				if (num4 != 0 && num4 != num6)
				{
					throw new Exception("配色配置类型不唯一。");
				}
				num4 = num6;
			}
		}
		if (num4 == 0)
		{
			throw new Exception("未找到已验证的界面配色配置。");
		}
		if (Q(num) != num2 || Q(num + 8) != num3)
		{
			throw new Exception("配色配置列表正在切换。");
		}
		return num4;
	}

	private long ColourConfig()
	{
		long num = Q(image + 102261280);
		ulong num2 = 14695981039346656037uL;
		for (int i = 0; i < 4; i++)
		{
			num2 ^= (byte)(3276800 >>> i * 8);
			num2 *= 1099511628211L;
		}
		long num3 = Q(num + 664);
		if (num3 < 0 || num3 > 1048576)
		{
			throw new Exception("配色配置索引无效。");
		}
		long num4 = Q(num + 640) + (long)((num2 & (ulong)num3) * 16);
		long num5 = Q(num + 624);
		long num6 = Q(num4 + 8);
		long num7 = Q(num4);
		int num8 = 0;
		while (num6 != num5 && num8++ < 128)
		{
			if (U(num6 + 16) == 3276800)
			{
				long num9 = Q(num6 + 24);
				long num10 = Q(num6 + 32);
				if (num10 - num9 != 8)
				{
					throw new Exception("配色配置不唯一。");
				}
				long num11 = Q(num9);
				if (U(num11 + 72) != 3276800)
				{
					throw new Exception("配色配置类型不匹配。");
				}
				return UiColourConfig76(num11);
			}
			if (num6 == num7)
			{
				break;
			}
			num6 = Q(num6 + 8);
		}
		throw new Exception("未找到游戏配色配置。");
	}

	private void ReadPlayerColours(List<PlayerInfoRow> rows, long arena, long world)
	{
		try
		{
			if (cameraCore == 0 || Q(cameraCore) != image + 64684536)
			{
				ReadMineCamera();
			}
			long num = Q(cameraCore + 680);
			long num2 = ColourConfig();
			long num3 = Resolve(arena, U(world + 284));
			byte[] array = Read(num + 24, 2);
			long num4 = Q(num + ((array[0] == 0) ? 8 : 16));
			uint num5 = (uint)((ulong)num4 >> 32);
			foreach (PlayerInfoRow row in rows)
			{
				int num6 = 3;
				if ((int)row.PlayerIdentity != 0 && (int)num4 != 0 && num5 < 16)
				{
					num6 = Read(num3 + 80 + row.Slot * 16 + num5, 1)[0];
				}
				int num7 = 77512;
				if (array[1] != 0)
				{
					if (num6 == 1 || num6 == 2)
					{
						uint num8 = U(num3 + row.Slot * 4);
						int num9 = 0;
						if (num8 != 10000 && num8 != uint.MaxValue)
						{
							for (uint num10 = 0u; num10 < row.Slot; num10++)
							{
								if (U(num3 + num10 * 4) == num8)
								{
									num9++;
								}
							}
						}
						if (num9 < 4)
						{
							num7 = ((num6 == 1) ? 77384 : 77448) + 16 * num9;
						}
					}
				}
				else if ((int)num4 != 0 && (int)num4 == (int)row.PlayerIdentity)
				{
					num7 = 77352;
				}
				else
				{
					switch (num6)
					{
					case 1:
						num7 = 77336;
						break;
					case 2:
						num7 = 77368;
						break;
					}
				}
				uint packed = U(num2 + num7 + 8);
				row.GameColour = PlayerColour76.Decode(packed);
				row.GameColourKnown = true;
			}
			if (!Read(num + 24, 2).AsSpanEqual(array))
			{
				throw new Exception("游戏配色模式正在切换。");
			}
			ColourError = "";
		}
		catch (Exception ex)
		{
			foreach (PlayerInfoRow row2 in rows)
			{
				row2.GameColourKnown = false;
			}
			ColourError = ex.Message;
		}
	}

	public MarkerReadContext MarkerContext()
	{
		MarkerReadContext markerReadContext = new MarkerReadContext();
		markerReadContext.Pid = process.Id;
		markerReadContext.Image = image;
		markerReadContext.Started = started;
		markerReadContext.Match = MatchKey;
		markerReadContext.Arena = markerArena;
		markerReadContext.WorldOffset = markerWorldOffset;
		return markerReadContext;
	}

	internal PlayerInfoReader(MarkerReadContext context)
	{
		process = Process.GetProcessById(context.Pid);
		try
		{
			started = process.StartTime;
			image = process.MainModule.BaseAddress.ToInt64();
			if (started != context.Started || image != context.Image)
			{
				throw new Exception("游戏进程已变化。");
			}
			if (!LicenseGate60.Matches(process.Id, started))
			{
				throw new Exception("授权验证尚未完成。");
			}
			handle = Native.OpenProcess(1040u, inherit: false, process.Id);
			if (handle.IsInvalid)
			{
				throw new Exception("无法只读连接标记数据。");
			}
			MatchKey = context.Match;
			ResetMinimapForMatch(context.Match);
			CaptureMarkerWorld(context.Arena, context.WorldOffset);
			CheckMarkerWorld(context.Match);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public PlayerInfoReader()
	{
		Process[] processesByName = Process.GetProcessesByName("RelicCoH3");
		if (processesByName.Length != 1)
		{
			Process[] array = processesByName;
			foreach (Process process in array)
			{
				process.Dispose();
			}
			throw new Exception("请先启动英雄连 3。");
		}
		this.process = processesByName[0];
		try
		{
			started = this.process.StartTime;
			image = this.process.MainModule.BaseAddress.ToInt64();
			GameVersion45.Assess(this.process.MainModule.FileName);
			if (!LicenseGate60.Matches(this.process.Id, started))
			{
				throw new Exception("授权验证尚未完成。");
			}
			handle = Native.OpenProcess(1040u, inherit: false, this.process.Id);
			if (handle.IsInvalid)
			{
				throw new Exception("无法只读连接游戏。请以与游戏相同的权限运行。");
			}
			long num = image + 112918560 + 840;
			long num2 = Q(num + 32);
			long num3 = Q(num + 40);
			if (num3 < num2 || num3 - num2 > 6144 || (num3 - num2) % 48 != 0)
			{
				throw new Exception("资源名称表尚未就绪。");
			}
			for (long num4 = num2; num4 < num3; num4 += 48)
			{
				string key = StringAt(num4 + 8);
				uint num5 = U(num4 + 40);
				if (num5 < 10)
				{
					resources[key] = (int)num5;
				}
			}
			long num6 = image + 112918560 + 560;
			long num7 = Q(num6 + 32);
			long num8 = Q(num6 + 40);
			if (num8 < num7 || num8 - num7 > 12288 || (num8 - num7) % 48 != 0)
			{
				throw new Exception("技能计时名称表尚未就绪。");
			}
			for (long num9 = num7; num9 < num8; num9 += 48)
			{
				string text = StringAt(num9 + 8);
				if (text == "shared_ability_1")
				{
					barrageTimer = U(num9 + 40);
				}
				else if (text == "shared_barrage_ability")
				{
					stukaBarrageTimer = U(num9 + 40);
				}
			}
			string[] array2 = new string[4] { "manpower", "fuel", "munition", "command" };
			foreach (string text2 in array2)
			{
				if (!resources.ContainsKey(text2))
				{
					throw new Exception("未识别资源：" + text2);
				}
			}
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private byte[] Read(long a, int n)
	{
		if (a < 65536 || n < 0 || n > 65536)
		{
			throw new Exception("信息地址无效。");
		}
		byte[] array = new byte[n];
		if (!Native.ReadProcessMemory(handle, new IntPtr(a), array, (UIntPtr)(ulong)n, out var read) || read.ToUInt64() != (ulong)n)
		{
			throw new Exception("对局数据正在变化，等待重新读取。");
		}
		return array;
	}

	private uint U(long a)
	{
		return BitConverter.ToUInt32(Read(a, 4), 0);
	}

	private float F(long a)
	{
		float num = BitConverter.ToSingle(Read(a, 4), 0);
		if (float.IsNaN(num) || float.IsInfinity(num))
		{
			throw new Exception("数值正在更新。");
		}
		return num;
	}

	private long Q(long a)
	{
		return BitConverter.ToInt64(Read(a, 8), 0);
	}

	private long Resolve(long arena, uint offset)
	{
		if (offset == uint.MaxValue)
		{
			throw new Exception("对局数据尚未就绪。");
		}
		return checked(arena + offset);
	}

	private string StringAt(long a)
	{
		long num = Q(a + 16);
		long num2 = Q(a + 24);
		if (num < 0 || num > 1024 || num2 < num)
		{
			throw new Exception("文本结构不匹配。");
		}
		if (num == 0)
		{
			return "";
		}
		return Encoding.UTF8.GetString(Read((num2 < 16) ? a : Q(a), (int)num));
	}

	private string NameAt(long arena, long player, uint index)
	{
		long num = player + 1640;
		if (U(num + 4) != uint.MaxValue)
		{
			return "玩家 " + index;
		}
		uint num2 = U(num + 12);
		if (num2 == 0 || num2 > 256)
		{
			return "玩家 " + index;
		}
		string text = Encoding.Unicode.GetString(Read(Resolve(arena, U(num + 8)), checked((int)num2 * 2)));
		char[] trimChars = new char[1];
		return text.TrimEnd(trimChars);
	}

	private float Resource(byte[] data, string name)
	{
		float num = BitConverter.ToSingle(data, resources[name] * 4);
		if (float.IsNaN(num) || float.IsInfinity(num))
		{
			throw new Exception("资源数据无效。");
		}
		return num;
	}

	private long Component(long entity, long lookup)
	{
		int num = BitConverter.ToUInt16(Read(entity + 76, 2), 0);
		int num2 = Read(image + lookup + num * 128, 1)[0];
		if (num2 == 255)
		{
			return 0L;
		}
		long num3 = Q(image + 102260448) + U(entity + 64) + num2 * 24;
		if ((Read(num3 + 16, 1)[0] & 1) == 0)
		{
			return 0L;
		}
		uint num4 = U(num3);
		long num5 = Q(num3 + 8);
		if (num5 == 0)
		{
			return 0L;
		}
		switch (num4)
		{
		case 8u:
			return num5;
		default:
			throw new Exception("组件数据正在更新。");
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
			return Q(image + 102260400 + num4 * 16) + (uint)num5;
		}
	}

	private string BlueprintName(long blueprint)
	{
		if (!blueprintNames.TryGetValue(blueprint, out var value))
		{
			value = StringAt(blueprint + 40);
			if (blueprintNames.Count > 10000)
			{
				blueprintNames.Clear();
			}
			blueprintNames[blueprint] = value;
		}
		return value;
	}

	private void ReadArtillery(long arena, ArtilleryState unit)
	{
		uint num = (unit.UsesSharedBarrage ? stukaBarrageTimer : barrageTimer);
		if (num == uint.MaxValue)
		{
			return;
		}
		long num2 = Component(unit.Entity, 103319637L);
		if (num2 == 0)
		{
			return;
		}
		uint num3 = U(num2 + 76);
		if (num3 == uint.MaxValue)
		{
			return;
		}
		long num4 = Resolve(arena, num3);
		uint num5 = U(num4 + 4);
		uint num6 = U(num4);
		if (num5 > 128)
		{
			return;
		}
		Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
		if (num5 != 0)
		{
			byte[] value = Read(Resolve(arena, num6), (int)(num5 * 28));
			for (int i = 0; i < num5; i++)
			{
				int num7 = i * 28;
				uint num8 = BitConverter.ToUInt32(value, num7);
				int num9 = BitConverter.ToInt32(value, num7 + 4);
				int num10 = BitConverter.ToInt32(value, num7 + 8);
				if (num8 == num)
				{
					if (num9 < 0 || num9 > 100000 || num10 < 0 || num10 > 100000)
					{
						return;
					}
					dictionary[num8] = num9;
				}
			}
		}
		if (U(num4 + 4) != num5 || U(num4) != num6)
		{
			return;
		}
		foreach (KeyValuePair<uint, int> item in dictionary)
		{
			unit.Cooldowns[item.Key] = item.Value;
		}
		unit.CooldownsKnown = true;
	}

	private void ReadEntities(long arena, long player, PlayerInfoRow row)
	{
		long num = Resolve(arena, U(player + 1792));
		uint num2 = U(num + 40);
		uint num3 = U(num + 36);
		if (num2 > 4096)
		{
			throw new Exception("单位列表正在更新。");
		}
		if (num2 == 0)
		{
			row.UnitsKnown = true;
			row.BuildingsKnown = true;
			return;
		}
		HashSet<long> seen = new HashSet<long>();
		HashSet<long> seen2 = new HashSet<long>();
		byte[] value = Read(Resolve(arena, num3), checked((int)num2 * 4));
		for (int i = 0; i < num2; i++)
		{
			uint num4 = BitConverter.ToUInt32(value, i * 4);
			if (num4 == uint.MaxValue)
			{
				continue;
			}
			long num5 = Resolve(arena, num4);
			if (Resolve(arena, U(num5 + 88)) != player)
			{
				continue;
			}
			ReadOwnedSquad(arena, num5, row, seen);
			string text = BlueprintName(Q(num5 + 56));
			TrackInfrastructure49(arena, num5, text, row);
			ReadMine(num5, text, row);
			if (Kwk38Upgrade.IsVehicle(text))
			{
				ReadProduction(arena, num5, row, seen2, vehicle: true);
			}
			try
			{
				ReadBarrageTarget(arena, num5, text, row);
			}
			catch
			{
				row.BarragesKnown = false;
			}
			if (text.Contains("\\buildings\\production\\"))
			{
				string[] array = text.Split('\\');
				if (array.Length >= 3)
				{
					row.Race = array[2];
				}
				ReadProduction(arena, num5, row, seen2);
				int num6 = BuildingNames.Tier(text);
				if (num6 == 0)
				{
					continue;
				}
				long num7 = Component(num5, 103319626L);
				long num8 = Component(num5, 103319627L);
				if (num7 == 0 || num8 == 0)
				{
					throw new Exception("建筑数据正在更新。");
				}
				if (F(num7 + 108) <= 0f)
				{
					continue;
				}
				row.Buildings.Add(new TrackedBuilding
				{
					Tier = num6,
					Key = num5 + ":" + U(num5 + 96),
					Complete = (U(num8 + 40) == 0)
				});
			}
			if (row.Relation == "自己" && ArtilleryState.IsSupported(text))
			{
				long num9 = Component(num5, 103319626L);
				if (num9 != 0 && !(F(num9 + 108) <= 0f))
				{
					ArtilleryState artilleryState = new ArtilleryState();
					artilleryState.Key = num5 + ":" + U(num5 + 96);
					artilleryState.Name = ArtilleryState.UnitName(text);
					artilleryState.Entity = num5;
					artilleryState.UsesSharedBarrage = ArtilleryState.IsStuka(text);
					ArtilleryState artilleryState2 = artilleryState;
					ReadArtillery(arena, artilleryState2);
					row.Artillery.Add(artilleryState2);
				}
			}
		}
		if (U(num + 40) != num2 || U(num + 36) != num3)
		{
			throw new Exception("单位列表正在更新。");
		}
		row.BuildingsKnown = true;
		row.UnitsKnown = true;
	}

	public List<PlayerInfoRow> Snapshot()
	{
		if (process.HasExited || process.StartTime != started)
		{
			throw new Exception("游戏进程已结束，请重新连接。");
		}
		long num = LocalIdentity58();
		uint num2 = (uint)num;
		uint localIndex = (uint)((ulong)num >> 32);
		if (num2 == 0 || localIndex >= 16)
		{
			throw new Exception("等待进入对局。");
		}
		long num3 = Q(image + 102260400);
		uint num4 = U(image + 98614988);
		long num5 = Resolve(num3, num4);
		long num6 = Resolve(num3, U(num5 + 252));
		uint num7 = U(num6 + 16);
		uint num8 = U(num6 + 12);
		if (num7 == 0 || num7 > 16)
		{
			throw new Exception("等待玩家列表。");
		}
		long num9 = Resolve(num3, num8);
		long num10 = Resolve(num3, U(num5 + 284));
		long num11 = ResolveLocalPlayer58(num3, num9, num7, num);
		string text = process.Id + ":" + started.Ticks + ":" + num5 + ":" + num11 + ":" + num2;
		if (MatchKey != text)
		{
			blueprintNames.Clear();
		}
		uint num12 = U(num10 + localIndex * 4);
		List<PlayerInfoRow> list = new List<PlayerInfoRow>();
		for (uint num13 = 0u; num13 < num7; num13++)
		{
			long num14 = Resolve(num3, U(num9 + num13 * 4));
			uint num15 = U(num14 + 12);
			if (num15 >= 16)
			{
				throw new Exception("玩家列表正在变化。");
			}
			PlayerInfoRow playerInfoRow = new PlayerInfoRow();
			playerInfoRow.PlayerAddress = num14;
			playerInfoRow.PlayerIdentity = Q(num14 + 8);
			playerInfoRow.Slot = num15;
			playerInfoRow.Team = U(num10 + num15 * 4);
			playerInfoRow.Name = NameAt(num3, num14, num15);
			playerInfoRow.Relation = ((num15 == localIndex) ? "自己" : ((playerInfoRow.Team == 10000 || num12 == 10000) ? "中立" : ((playerInfoRow.Team == 10002 || num12 == 10002) ? "敌方" : ((playerInfoRow.Team == num12) ? "队友" : "敌方"))));
			byte[] data = Read(num14 + 1704, 40);
			playerInfoRow.Manpower = Resource(data, "manpower");
			playerInfoRow.Fuel = Resource(data, "fuel");
			playerInfoRow.Munition = Resource(data, "munition");
			playerInfoRow.CP = Resource(data, "command");
			playerInfoRow.Population = F(num14 + 1256) + F(num14 + 1304);
			if (playerInfoRow.Population < 0f || playerInfoRow.Population > 10000f)
			{
				throw new Exception("人口数据正在更新。");
			}
			uint num16 = U(num14 + 788);
			uint num17 = U(num14 + 784);
			if (num16 > 1024)
			{
				throw new Exception("升级列表正在变化。");
			}
			if (num16 != 0)
			{
				long num18 = Resolve(num3, num17);
				for (uint num19 = 0u; num19 < num16; num19++)
				{
					long num20 = num18 + num19 * 104;
					if (Read(num20 + 16, 1)[0] == 0)
					{
						playerInfoRow.Upgrades.Add(StringAt(Q(num20) + 40));
					}
				}
				if (U(num14 + 788) != num16 || U(num14 + 784) != num17)
				{
					throw new Exception("升级列表正在变化。");
				}
			}
			string[] array = (from x in playerInfoRow.Upgrades
				where x.Contains("\\battlegroups\\")
				select x.Split('\\') into x
				where x.Length >= 5
				select x[3]).Distinct().ToArray();
			playerInfoRow.Group = ((array.Length == 0) ? "未读到已选战斗群" : string.Join(" / ", array.Select((string x) => (!(x == "armored")) ? ((!(x == "luftwaffe")) ? x : "空军（luftwaffe）") : "装甲（armored）")));
			ReadEntities(num3, num14, playerInfoRow);
			list.Add(playerInfoRow);
		}
		if (U(image + 98614988) != num4 || Q(image + 102260400) != num3 || U(num6 + 16) != num7 || U(num6 + 12) != num8 || LocalIdentity58() != num || Q(num11 + 8) != num)
		{
			throw new Exception("对局已切换，等待重新读取。");
		}
		if (!list.Any((PlayerInfoRow x) => x.Slot == localIndex))
		{
			throw new Exception("未在对局中找到自己。");
		}
		ReadPlayerColours(list, num3, num5);
		ResetMinimapForMatch(text);
		MatchKey = text;
		CaptureMarkerWorld(num3, num4);
		BindLocalAccount59(list, num);
		return list;
	}

	public void Dispose()
	{
		ClearLocalAccount59();
		StopMinimapDiscovery();
		if (handle != null)
		{
			handle.Dispose();
		}
		if (process != null)
		{
			process.Dispose();
		}
	}

	private void ReadProduction(long arena, long entity, PlayerInfoRow row, HashSet<long> seen, bool vehicle = false)
	{
		try
		{
			long num = Component(entity, 103319626L);
			if (num == 0 || F(num + 108) <= 0f)
			{
				return;
			}
			long num2 = (vehicle ? Component(Resolve(arena, U(entity + 124)), 103319618L) : Component(entity, 103319633L));
			if (num2 == 0)
			{
				return;
			}
			int num3 = (vehicle ? 16 : 52);
			uint num4 = U(num2 + num3);
			if (num4 == uint.MaxValue)
			{
				return;
			}
			long num5 = Resolve(arena, num4);
			if (!seen.Add(num5))
			{
				return;
			}
			uint num6 = U(num5 + 52);
			uint num7 = U(num5 + 48);
			if (num6 > 128)
			{
				throw new Exception("生产队列正在更新。");
			}
			if (num6 == 0)
			{
				return;
			}
			long num8 = Resolve(arena, num7);
			byte[] value = Read(num8, 192);
			uint num9 = BitConverter.ToUInt32(value, 184);
			long num10 = Resolve(arena, num9);
			ProductionJob productionJob = null;
			ProductionJob productionJob2 = null;
			float? num11 = null;
			if (!vehicle && Q(num10) == image + 64584880)
			{
				long num12 = Q(num10 + 16);
				string text = BlueprintName(num12);
				if (!text.StartsWith("sbps\\races\\", StringComparison.Ordinal))
				{
					throw new Exception("生产单位名称正在更新。");
				}
				productionJob = new ProductionJob(text, BitConverter.ToSingle(value, 48), BitConverter.ToSingle(value, 52), num5 + ":" + BitConverter.ToUInt32(value, 4));
				if (Q(num10 + 16) != num12 || Q(num10) != image + 64584880)
				{
					throw new Exception("生产单位正在更新。");
				}
			}
			long num13 = image + (vehicle ? 64557040 : 64557304);
			if (Q(num10) == num13)
			{
				long num14 = Q(num10 + 16);
				string path = BlueprintName(num14);
				if (!vehicle && GrenadeUpgrade.IsResearch(path))
				{
					float num15 = BitConverter.ToSingle(value, 48);
					float num16 = BitConverter.ToSingle(value, 52);
					GrenadeUpgrade.SecondsRemaining(num15);
					GrenadeUpgrade.SecondsRemaining(num16);
					if (num16 <= 0f || num15 > num16)
					{
						throw new Exception("手雷升级进度正在更新。");
					}
					num11 = num15;
				}
				if (vehicle && Kwk38Upgrade.IsResearch(path))
				{
					productionJob2 = new ProductionJob(path, BitConverter.ToSingle(value, 48), BitConverter.ToSingle(value, 52), entity + ":" + U(entity + 96));
				}
				if (Q(num10 + 16) != num14 || Q(num10) != num13)
				{
					throw new Exception("升级项目正在更新。");
				}
			}
			if (U(num2 + num3) != num4 || U(num5 + 52) != num6 || U(num5 + 48) != num7 || U(num8 + 184) != num9 || Q(num8) != BitConverter.ToInt64(value, 0))
			{
				throw new Exception("生产队列正在更新。");
			}
			if (productionJob != null)
			{
				row.Producing.Add(productionJob);
			}
			if (productionJob2 != null)
			{
				row.Kwk38Upgrading.Add(productionJob2);
			}
			if (num11.HasValue)
			{
				row.GrenadeRemainingTicks = (row.GrenadeRemainingTicks.HasValue ? new float?(Math.Min(row.GrenadeRemainingTicks.Value, num11.Value)) : num11);
			}
		}
		catch (Exception)
		{
			if (vehicle)
			{
				row.Kwk38Known = false;
			}
			else
			{
				row.ProductionKnown = false;
			}
		}
	}

	private string ShellCast(long arena, long registry, ulong source)
	{
		long num = BarrageReference(arena, registry, (uint)(source >> 32), (uint)source, squad: false);
		if (num == 0)
		{
			return "";
		}
		long num2 = Component(num, 103319637L);
		if (num2 == 0)
		{
			return "";
		}
		uint num3 = U(num2 + 76);
		if (num3 == uint.MaxValue)
		{
			return "";
		}
		long num4 = Resolve(arena, num3);
		uint num5 = U(num4 + 44);
		uint num6 = U(num4 + 40);
		if (num5 == 0 || num5 > 64 || num6 == uint.MaxValue)
		{
			return "";
		}
		byte[] value = Read(Resolve(arena, num6), (int)(num5 * 4));
		for (int i = 0; i < num5; i++)
		{
			uint num7 = BitConverter.ToUInt32(value, i * 4);
			if (num7 == uint.MaxValue)
			{
				continue;
			}
			long num8 = Resolve(arena, num7);
			if (Q(num8) != image + 64568064)
			{
				continue;
			}
			byte[] array = Read(num8, 164);
			if (BarrageLogic.Running(array))
			{
				string text = BlueprintName(BitConverter.ToInt64(array, 56));
				if (text.StartsWith("abilities\\", StringComparison.Ordinal) && (text.Contains("barrage") || text.Contains("smoke_howitzer") || text.Contains("precision_sweep")))
				{
					return num8 + ":" + text;
				}
			}
		}
		return "";
	}

	private byte[] ShellBlock(long address, int count)
	{
		if (count <= 65536)
		{
			return Read(address, count);
		}
		byte[] array = new byte[count];
		for (int i = 0; i < count; i += 65536)
		{
			byte[] array2 = Read(address + i, Math.Min(65536, count - i));
			Buffer.BlockCopy(array2, 0, array, i, array2.Length);
		}
		return array;
	}

	private string ShellName(long arena, long registry, byte[] ext, string weapon)
	{
		string text = AttackCatalogue67.Name(weapon);
		if (text != null)
		{
			return text;
		}
		if (ext[16] == 3)
		{
			uint num = BitConverter.ToUInt32(ext, 20);
			uint num2 = BitConverter.ToUInt32(ext, 24);
			ulong key = ((ulong)num << 32) | num2;
			if (shellSourceNames.TryGetValue(key, out var value))
			{
				return value;
			}
			try
			{
				long num3 = BarrageReference(arena, registry, num, num2, squad: false);
				if (num3 != 0)
				{
					uint num4 = U(num3 + 124);
					string path = ((num4 != uint.MaxValue) ? BlueprintName(Q(Resolve(arena, num4) + 56)) : BarrageCatalogue.Squad(BlueprintName(Q(num3 + 56))));
					value = UnitNames.Name(path);
					if (!string.IsNullOrWhiteSpace(value))
					{
						if (shellSourceNames.Count > 4096)
						{
							shellSourceNames.Clear();
						}
						shellSourceNames[key] = value;
						return value;
					}
				}
			}
			catch
			{
			}
		}
		string text2 = (weapon.Contains("obice_210") ? "sbps\\races\\german\\emplacements\\howitzer_obice_210_ger" : (weapon.Contains("wespe") ? "sbps\\races\\german\\vehicles\\wespe_ger" : (weapon.Contains("stukazufuss") ? "sbps\\races\\afrika_korps\\vehicles\\halftrack_251_stuka_ak" : (weapon.Contains("nebelwerfer") ? "sbps\\races\\german\\team_weapons\\nebelwerfer_150mm_ger" : (weapon.Contains("cannone") ? "sbps\\races\\afrika_korps\\team_weapons\\howitzer_cannone_da_105_ak" : (weapon.Contains("leig") ? "sbps\\races\\afrika_korps\\team_weapons\\howitzer_75mm_leig_ak" : null))))));
		if (text2 != null)
		{
			return UnitNames.Name(text2);
		}
		return "火炮";
	}

	public List<ProjectileImpact> ReadProjectileImpacts(string match, IEnumerable<ProjectilePlayer> enemyPlayers, bool includeInfantry = false)
	{
		CheckMarkerWorld(match);
		long arena = markerArena;
		long num = Resolve(arena, markerWorldOffset);
		long num2 = Resolve(arena, U(num + 260));
		if (shellRegistry != num2)
		{
			shellRegistry = num2;
			shellSeen = new ulong[0];
			shellCandidates.Clear();
			shellSourceNames.Clear();
			shellSequence.Clear();
		}
		uint count = U(num2 + 24);
		uint num3 = U(num2 + 8);
		uint num4 = U(num2 + 20);
		if (count == 0 || count > 65536)
		{
			throw new Exception("弹体列表正在更新。");
		}
		byte[] value;
		byte[] value2;
		checked
		{
			value = ShellBlock(Resolve(arena, num3), (int)count * 8);
			value2 = ShellBlock(Resolve(arena, num4), (int)count * 4);
		}
		if (shellSeen.Length < count)
		{
			Array.Resize(ref shellSeen, (int)count);
		}
		Dictionary<long, ProjectilePlayer> dictionary = (from p in enemyPlayers
			group p by p.Address).ToDictionary((IGrouping<long, ProjectilePlayer> g) => g.Key, (IGrouping<long, ProjectilePlayer> g) => g.First());
		List<ProjectileImpact> list = new List<ProjectileImpact>();
		uint[] array = shellCandidates.Keys.Where((uint i) => i >= count).ToArray();
		foreach (uint key in array)
		{
			shellCandidates.Remove(key);
		}
		for (uint num6 = 0u; num6 < count; num6++)
		{
			uint num7 = BitConverter.ToUInt32(value, (int)(num6 * 8));
			uint num8 = BitConverter.ToUInt32(value2, (int)(num6 * 4));
			ulong num9 = ((ulong)num8 << 32) | num7;
			if (num7 == uint.MaxValue || num8 == 0)
			{
				shellCandidates.Remove(num6);
				shellSeen[num6] = num9;
				continue;
			}
			if (shellSeen[num6] != num9)
			{
				shellCandidates.Remove(num6);
				try
				{
					long num10 = Resolve(arena, num7);
					byte[] value3 = Read(num10, 128);
					if (BitConverter.ToUInt32(value3, 104) != num8 || BitConverter.ToUInt32(value3, 108) != num6)
					{
						continue;
					}
					long blueprint = BitConverter.ToInt64(value3, 56);
					string text = BlueprintName(blueprint);
					if (string.IsNullOrEmpty(text))
					{
						continue;
					}
					if (text.StartsWith("ebps\\", StringComparison.Ordinal) && text.Contains("\\projectiles\\"))
					{
						shellCandidates[num6] = new ShellCandidate
						{
							Entity = num10,
							Id = num8,
							Index = num6,
							Blueprint = blueprint
						};
					}
					shellSeen[num6] = num9;
					goto IL_0317;
				}
				catch
				{
				}
				continue;
			}
			goto IL_0317;
			IL_0317:
			if (!shellCandidates.TryGetValue(num6, out var value4))
			{
				continue;
			}
			try
			{
				long entity = value4.Entity;
				byte[] array2 = Read(entity, 392);
				uint num11 = BitConverter.ToUInt32(array2, 388);
				if (num11 == uint.MaxValue)
				{
					continue;
				}
				long num12 = Resolve(arena, num11);
				long num13 = Component(entity, 103319634L);
				if (num13 == 0 || Q(num13) != image + 65307976)
				{
					continue;
				}
				byte[] array3 = Read(num13, 208);
				uint num14 = BitConverter.ToUInt32(array3, 56);
				if (num14 == uint.MaxValue || !dictionary.TryGetValue(Resolve(arena, num14), out var value5))
				{
					continue;
				}
				string text2 = BlueprintName(BitConverter.ToInt64(array3, 64));
				string text3 = null;
				bool flag = false;
				string text4 = null;
				if (text3 == null && text4 == null && !ProjectileLogic.Weapon(text2))
				{
					continue;
				}
				byte[] array4 = Read(num12, 80);
				if (ProjectileLogic.Decode(array2, array4, array3, image, arena, entity, num8, num6, out var impact, ProjectileLogic.Mortar(text2) || AttackCatalogue67.Arc(text2) || text4 != null, text3 != null, flag) && U(entity + 104) == num8 && U(entity + 108) == num6 && Q(entity + 56) == value4.Blueprint && U(entity + 388) == num11 && Read(num12 + 16, 16).SequenceEqual(array4.Skip(16).Take(16)) && (value4.SourceKey == 0 || value4.SourceKey == impact.SourceKey))
				{
					value4.SourceKey = impact.SourceKey;
					if (flag)
					{
						impact.SourceKey |= 9223372036854775808uL;
					}
					impact.Slot = value5.Slot;
					impact.Colour = value5.Colour;
					impact.Weapon = text2;
					impact.Label = text3;
					impact.Name = text3 ?? text4 ?? ShellName(arena, num2, array3, text2);
					list.Add(impact);
				}
			}
			catch
			{
			}
		}
		if (U(num2 + 24) != count || U(num2 + 8) != num3 || U(num2 + 20) != num4 || Resolve(arena, U(num + 260)) != num2)
		{
			throw new Exception("弹体列表正在切换。");
		}
		CheckMarkerWorld(match);
		double num15 = (double)Stopwatch.GetTimestamp() * 1000.0 / (double)Stopwatch.Frequency;
		HashSet<ulong> hashSet = new HashSet<ulong>(from s in list
			where s.Label == null && shellCandidates[s.Index].Number == 0
			select s.SourceKey);
		foreach (ulong item in hashSet)
		{
			shellSequence.Get(item);
		}
		foreach (ProjectileSequenceTracker.State state in shellSequence.States)
		{
			if (!(num15 < state.NextCheck) || hashSet.Contains(state.Source))
			{
				try
				{
					shellSequence.Observe(state.Source, ShellCast(arena, num2, state.Source));
					state.NextCheck = num15 + 100.0;
				}
				catch
				{
				}
			}
		}
		foreach (ProjectileImpact item2 in from s in list
			where s.Label == null
			orderby s.Id
			select s)
		{
			ShellCandidate shellCandidate = shellCandidates[item2.Index];
			if (shellCandidate.Number == 0)
			{
				shellCandidate.Number = shellSequence.Next(item2.SourceKey, num15);
			}
			item2.Number = shellCandidate.Number;
		}
		shellSequence.Trim(num15);
		return list;
	}

	public bool IsCurrentMatch55()
	{
		try
		{
			if (string.IsNullOrEmpty(MatchKey) || process.HasExited || process.StartTime != started)
			{
				return false;
			}
			CheckMarkerWorld(MatchKey);
			long num = LocalIdentity58();
			uint num2 = (uint)num;
			if (num2 == 0 || (uint)(num >>> 32) >= 16)
			{
				return false;
			}
			long num3 = Resolve(markerArena, markerWorldOffset);
			long num4 = Resolve(markerArena, U(num3 + 252));
			long num5 = ResolveLocalPlayer58(markerArena, Resolve(markerArena, U(num4 + 12)), U(num4 + 16), num);
			return MatchKey == process.Id + ":" + started.Ticks + ":" + num3 + ":" + num5 + ":" + num2;
		}
		catch
		{
			return false;
		}
	}

	private void TrackSquadMember(long arena, long entity, long squad, string path, PlayerInfoRow row)
	{
		if ((!(row.Relation != "敌方") || !(row.Relation != "自己") || !(row.Relation != "队友")) && SquadMarkerLogic.Supported(path))
		{
			SquadMarkerSource squadMarkerSource = row.MarkerSquads.FirstOrDefault((SquadMarkerSource s) => s.Squad == squad);
			if (squadMarkerSource == null)
			{
				SquadMarkerSource squadMarkerSource2 = new SquadMarkerSource();
				squadMarkerSource2.Squad = squad;
				squadMarkerSource2.Identity = Q(squad + 104);
				squadMarkerSource2.Path = path;
				squadMarkerSource2.Slot = row.Slot;
				squadMarkerSource = squadMarkerSource2;
				row.MarkerSquads.Add(squadMarkerSource);
			}
			squadMarkerSource.Members.Add(new SquadMarkerMember
			{
				Entity = entity,
				Owner = Resolve(arena, U(entity + 88)),
				Generation = U(entity + 96),
				SquadOffset = U(entity + 124)
			});
		}
	}

	private void CaptureMarkerWorld(long arena, uint worldOffset)
	{
		markerArena = arena;
		markerWorldOffset = worldOffset;
	}

	private void CheckMarkerWorld(string match)
	{
		if (process.HasExited || MatchKey != match || markerArena == 0 || Q(image + 102260400) != markerArena || U(image + 98614988) != markerWorldOffset)
		{
			throw new Exception("对局已切换，清空班组标记。");
		}
	}

	public List<SquadMarker> ReadSquadMarkers(string match, IEnumerable<SquadMarkerSource> sources)
	{
		CheckMarkerWorld(match);
		List<SquadMarker> list = new List<SquadMarker>();
		foreach (SquadMarkerSource source in sources)
		{
			if (!source.ColourKnown)
			{
				continue;
			}
			try
			{
				if (Q(source.Squad + 104) != source.Identity)
				{
					continue;
				}
				List<float[]> list2 = new List<float[]>();
				foreach (SquadMarkerMember member in source.Members)
				{
					try
					{
						byte[] value = Read(member.Entity, 128);
						if (BitConverter.ToUInt32(value, 96) != member.Generation || BitConverter.ToUInt32(value, 124) != member.SquadOffset || Resolve(markerArena, BitConverter.ToUInt32(value, 88)) != member.Owner)
						{
							continue;
						}
						long num = Component(member.Entity, 103319626L);
						if (num != 0 && !(F(num + 108) <= 0f))
						{
							float num2 = BitConverter.ToSingle(value, 44);
							float num3 = BitConverter.ToSingle(value, 48);
							float num4 = BitConverter.ToSingle(value, 52);
							if (SquadMarkerLogic.ValidPosition(num2, num3, num4))
							{
								list2.Add(new float[3] { num2, num3, num4 });
							}
						}
					}
					catch
					{
					}
				}
				if (Q(source.Squad + 104) == source.Identity)
				{
					SquadMarker squadMarker = SquadMarkerLogic.Centre(source, list2);
					if (squadMarker != null)
					{
						ReadDirection(squadMarker, source);
						ReadMovement(squadMarker, source);
						list.Add(squadMarker);
					}
				}
			}
			catch
			{
			}
		}
		CheckMarkerWorld(match);
		return list;
	}

	public List<SupportCommand49> ReadSupport49(IEnumerable<PlayerInfoRow> rows)
	{
		List<SupportCommand49> list = new List<SupportCommand49>();
		foreach (PlayerInfoRow row in rows)
		{
			if (row.Relation != "敌方")
			{
				continue;
			}
			long num = row.PlayerAddress + 1016;
			uint num2 = U(num + 44);
			uint num3 = U(num + 40);
			if (num2 > 256)
			{
				continue;
			}
			byte[] value = ((num2 == 0) ? new byte[0] : Read(Resolve(markerArena, num3), (int)(num2 * 4)));
			for (int i = 0; i < num2; i++)
			{
				try
				{
					uint num4 = BitConverter.ToUInt32(value, i * 4);
					if (num4 == uint.MaxValue)
					{
						continue;
					}
					long num5 = Resolve(markerArena, num4);
					if (Q(num5) != image + 64568064)
					{
						continue;
					}
					byte[] array = Read(num5, 164);
					if (!SupportCatalogue49.Active(array, image, row.PlayerIdentity))
					{
						continue;
					}
					string text = BlueprintName(BitConverter.ToInt64(array, 56));
					string text2 = SupportCatalogue49.Name(text);
					if (text2 != null)
					{
						uint num6 = BitConverter.ToUInt32(array, 12);
						uint num7 = BitConverter.ToUInt32(array, 8);
						if (num6 != 0 && num6 <= 16 && num7 != uint.MaxValue && ReadBarragePosition(markerArena, Read(Resolve(markerArena, num7), (int)(num6 * 20)), 0, out var x, out var y, out var z) && Read(num5, 164).SequenceEqual(array))
						{
							list.Add(new SupportCommand49
							{
								Key = row.PlayerIdentity + ":" + num5 + ":" + BitConverter.ToUInt32(array, 68),
								Slot = row.Slot,
								Path = text,
								Name = text2,
								Flare = (text2 == "SLD星爆照明弹"),
								X = x,
								Y = y,
								Z = z
							});
						}
					}
				}
				catch
				{
				}
			}
			if (U(num + 44) != num2 || U(num + 40) != num3)
			{
				throw new Exception("战斗群技能列表正在变化。");
			}
		}
		return list;
	}

	private static bool TacticalVisible51(uint flags)
	{
		return (flags & 0x20) != 0;
	}

	private IEnumerable<long> TacticalChildren51(long node)
	{
		HashSet<long> hashSet = new HashSet<long>();
		int[] array = new int[2] { 200, 472 };
		foreach (int num in array)
		{
			try
			{
				long num2 = Q(node + num);
				if (num2 > 65536 && num2 != node && Q(num2 + 136) == node)
				{
					hashSet.Add(num2);
				}
			}
			catch
			{
			}
		}
		try
		{
			long num3 = Q(node + 504);
			if (num3 > 65536)
			{
				uint num4 = U(num3 + 124);
				long num5 = Q(num3 + 112);
				if (num4 <= 512 && num5 > 65536)
				{
					byte[] value = Read(num5, (int)(num4 * 8));
					for (int j = 0; j < num4; j++)
					{
						long num6 = BitConverter.ToInt64(value, j * 8);
						try
						{
							if (num6 > 65536 && Q(num6 + 136) == node)
							{
								hashSet.Add(num6);
							}
						}
						catch
						{
						}
					}
				}
			}
		}
		catch
		{
		}
		return hashSet;
	}

	private long FindTactical51(long mini)
	{
		long num = mini;
		HashSet<long> hashSet = new HashSet<long>();
		for (int i = 0; i < 64; i++)
		{
			if (num == 0)
			{
				break;
			}
			if (!hashSet.Add(num))
			{
				return 0L;
			}
			if (Q(num) == image + 65713904)
			{
				break;
			}
			num = Q(num + 136);
		}
		if (num == 0 || Q(num) != image + 65713904)
		{
			return 0L;
		}
		Queue<long> queue = new Queue<long>();
		queue.Enqueue(num);
		hashSet.Clear();
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (queue.Count > 0 && hashSet.Count < 2048 && stopwatch.ElapsedMilliseconds < 60)
		{
			long num2 = queue.Dequeue();
			if (!hashSet.Add(num2))
			{
				continue;
			}
			try
			{
				long num3 = Q(num2);
				if (num3 == image + 65513472)
				{
					return num2;
				}
				if (num3 == image + 65511760)
				{
					continue;
				}
				foreach (long item in MiniChildren53(num2))
				{
					queue.Enqueue(item);
				}
			}
			catch
			{
			}
		}
		return 0L;
	}

	private MinimapFrame ReadTacticalLayout51(long control, out bool visible)
	{
		visible = false;
		if (control < 65536 || Q(control) != image + 65513472)
		{
			throw new MinimapRetiredControl("战术地图控件已切换。");
		}
		byte[] array = Read(control, 552);
		byte[] properties = MiniProperties(control, array);
		float ww = MiniDimension(properties, MiniPropertyId(111989640L));
		float wh = MiniDimension(properties, MiniPropertyId(111989680L));
		long num = BitConverter.ToInt64(array, 528);
		if (num < 65536)
		{
			throw new Exception("战术地图模板尚未就绪。");
		}
		byte[] value = Read(num, 344);
		float num2 = BitConverter.ToSingle(value, 296);
		float num3 = BitConverter.ToSingle(value, 300);
		if (num2 < 10f || num3 < 10f || num2 > 20000f || num3 > 20000f)
		{
			throw new Exception("战术地图尚未完成布局。");
		}
		MinimapAffine localToUi = MinimapAffine.Identity;
		long num4 = num;
		HashSet<long> hashSet = new HashSet<long>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		float num5 = 0f;
		float num6 = 0f;
		while (num4 != 0)
		{
			if (!hashSet.Add(num4) || hashSet.Count > 64)
			{
				throw new MinimapRetiredControl("战术地图界面层级已变化。");
			}
			byte[] array2 = Read(num4, 344);
			long num7 = BitConverter.ToInt64(array2, 0) - image;
			if (num4 == control)
			{
				flag = true;
			}
			if (num7 == 65713904)
			{
				flag2 = true;
			}
			flag4 &= TacticalVisible51(BitConverter.ToUInt32(array2, 192));
			long num8 = MiniProperty(MiniProperties(num4, array2), 40);
			if (num4 == num && num8 == 0)
			{
				throw new Exception("战术地图旋转尚未就绪。");
			}
			if (num8 != 0)
			{
				localToUi = localToUi.Then(MiniTransform(num8));
			}
			float num9 = BitConverter.ToSingle(array2, 168);
			float num10 = BitConverter.ToSingle(array2, 172);
			if (!SquadMarkerLogic.ValidPosition(num9, 0f, num10))
			{
				throw new Exception("战术地图界面偏移无效。");
			}
			localToUi = localToUi.Then(MinimapAffine.Translation(num9, num10));
			long num11 = BitConverter.ToInt64(array2, 136);
			if (Q(num4 + 136) != num11)
			{
				throw new Exception("战术地图界面正在更新。");
			}
			if (num11 == 0)
			{
				flag3 = num7 == 64823200;
				num5 = BitConverter.ToSingle(array2, 296);
				num6 = BitConverter.ToSingle(array2, 300);
			}
			num4 = num11;
		}
		if (!flag || !flag2 || !flag3)
		{
			throw new MinimapRetiredControl("战术地图已离开当前对局界面。");
		}
		if (num5 < 200f || num6 < 200f || num5 > 20000f || num6 > 20000f)
		{
			throw new Exception("战术地图窗口尺寸未就绪。");
		}
		if (Q(control + 528) != num || Q(control) != image + 65513472)
		{
			throw new MinimapRetiredControl("战术地图正在切换。");
		}
		MinimapFrame minimapFrame = MinimapAffine.Frame(ww, wh, num2, num3, num5, num6, localToUi);
		if (!minimapFrame.Valid || minimapFrame.CentreX < 0f || minimapFrame.CentreX > num5 || minimapFrame.CentreY < 0f || minimapFrame.CentreY > num6)
		{
			throw new Exception("战术地图变换无效。");
		}
		visible = flag4;
		return minimapFrame;
	}

	public bool TryReadTactical51(out MinimapFrame frame)
	{
		frame = null;
		long num;
		long num2;
		long num3;
		lock (miniGate)
		{
			num = tacticalControl51;
			num2 = miniEpoch;
			num3 = miniControl;
		}
		try
		{
			CheckMarkerWorld(MatchKey);
			if (num == 0)
			{
				if (num3 == 0)
				{
					TryReadMinimap(out var _);
					lock (miniGate)
					{
						num3 = miniControl;
					}
				}
				if (num3 == 0)
				{
					TacticalStatus51 = "正在连接地图控件…";
					return false;
				}
				if (DateTime.UtcNow < tacticalRetry51)
				{
					return false;
				}
				tacticalRetry51 = DateTime.UtcNow.AddMilliseconds(500.0);
				num = FindTactical51(num3);
				if (num == 0)
				{
					TacticalStatus51 = "战术地图定位中…";
					return false;
				}
				lock (miniGate)
				{
					if (miniDisposed || miniEpoch != num2)
					{
						return false;
					}
					tacticalControl51 = num;
				}
			}
			bool visible;
			MinimapFrame minimapFrame = ReadTacticalLayout51(num, out visible);
			CheckMarkerWorld(MatchKey);
			lock (miniGate)
			{
				if (miniDisposed || miniEpoch != num2)
				{
					return false;
				}
			}
			TacticalMapState51.Publish(process.Id, MatchKey, visible);
			TacticalStatus51 = (visible ? "" : "战术地图未打开");
			if (!visible)
			{
				return false;
			}
			frame = minimapFrame;
			return true;
		}
		catch (Exception ex)
		{
			TacticalStatus51 = ex.Message;
			if (ex is MinimapRetiredControl)
			{
				lock (miniGate)
				{
					if (miniEpoch == num2)
					{
						tacticalControl51 = 0L;
					}
				}
			}
			return false;
		}
	}

	private void ReadOwnedSquad(long arena, long entity, PlayerInfoRow row, HashSet<long> seen)
	{
		uint num = U(entity + 124);
		if (num == uint.MaxValue)
		{
			return;
		}
		long num2 = Component(entity, 103319626L);
		if (num2 == 0 || F(num2 + 108) <= 0f)
		{
			return;
		}
		long num3 = Resolve(arena, num);
		long item = Q(num3 + 104);
		string text = BlueprintName(Q(num3 + 56));
		if (text.StartsWith("sbps\\races\\") && !text.Contains("\\buildings\\") && !text.Contains("\\base_defense_") && !text.Contains("\\howitzer_base_"))
		{
			try
			{
				TrackSquadMember(arena, entity, num3, text, row);
			}
			catch
			{
				row.MarkerDataKnown = false;
			}
			if (!seen.Contains(item))
			{
				seen.Add(item);
				row.Units.TryGetValue(text, out var value);
				row.Units[text] = value + 1;
			}
		}
	}

	private void ReadMine(long entity, string path, PlayerInfoRow row)
	{
		if (!HazardKind.Supported(path))
		{
			return;
		}
		long num = Component(entity, 103319626L);
		if (num != 0 && !(F(num + 108) <= 0f))
		{
			long num2 = Component(entity, 103319627L);
			if (num2 == 0 || U(num2 + 40) == 0)
			{
				row.Mines.Add(new MineRecord
				{
					Entity = entity,
					Path = path,
					Relation = row.Relation,
					Demolition = HazardKind.Demolition(path),
					X = F(entity + 44),
					Y = F(entity + 48),
					Z = F(entity + 52)
				});
			}
		}
	}
}
