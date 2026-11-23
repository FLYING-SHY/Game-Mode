using System;
using System.Collections.Generic;

namespace CoH3MemoryTool;

// Layout for game build 5.1.52426.0 (fingerprint 8a34...d3ec).
// Keys name the legacy reader roles; values are individually re-located RVAs, not a delta.
// The table is compiled into every build but only selected when GameVersion45 detects this
// build, so an unknown or older fingerprint can never reach these addresses.
internal static class GameLayout52426
{
	// The single build this table describes; GameVersion45 owns the fingerprint decision.
	internal const string BuildHash = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec";

	private static readonly Dictionary<long, long> candidates = new Dictionary<long, long>
	{
		{ 112918560, 0x6BB1ED0 }, // resource singleton / getter and live name/index table
		{ 98614988, 0x5E0DECC }, // world offset / RIP read and live world
		{ 102260400, 0x6187E40 }, // arena table / static resolver and live arena
		{ 102260448, 0x6187E70 }, // TLS-indexed component pool array (index 0 snapshot)
		{ 102260680, 0x6187F58 }, // GameUICore list owner
		{ 102261280, 0x61881B0 }, // config registry / key 0x320000
		{ 112837064, 0x6B9E078 }, // UI singleton / GameUIAppWindow
		{ 64684536, 0x3DB1068 }, // GameUICore, not CameraEntity
		{ 64683768, 0x3DB0D68 }, // TeamColourManager
		{ 64823200, 0x3DD4BF0 }, // GameUIAppWindow
		{ 65511760, 0x3E7B160 }, // MapControl
		{ 65513472, 0x3E7B810 }, // TacticalMapControl
		{ 65713904, 0x3EAC6B0 }, // AnvilRTSHUDPage
		{ 64430720, 0x3D73280 }, // TranslateTransform
		{ 64431008, 0x3D733A0 }, // MatrixTransform
		{ 64432864, 0x3D73AE0 }, // TransformGroup
		{ 64815280, 0x3DD2D00 }, // RotateTransform
		{ 64815656, 0x3DD2E78 }, // ScaleTransform
		{ 111878264, 0x6AB4018 }, // Matrix property pointer, wrapper +32; ID 69
		{ 111989640, 0x6ACF328 }, // PlayableWidth pointer; ID 874
		{ 111989680, 0x6ACF350 }, // PlayableHeight pointer; ID 875
		{ 64584880, 0x3D98BF0 }, // SpawnProductionItem / RTTI and live production chain
		{ 64568064, 0x3D94A40 }, // TimedAbility / RTTI, constructor, live mortar barrage
		{ 65313128, 0x3E4AA38 }, // ProjectileDynamics / static RTTI candidate; event fields pending
		{ 65307976, 0x3E49618 }, // ProjectileExt / RTTI and live mortar projectile chain
		{ 64559016, 0x3D926E0 }, // MoverDynamics / RTTI and live loiter instance
		{ 13437424, 0xCD0F10 }, // MoverDynamics vtable slot +344 / mode resolver
		{ 64557040, 0x3D92058 }, // SquadUpgradeProductionItem
		{ 64557304, 0x3D92160 }, // EntityUpgradeProductionItem
		{ 103319618, 0x628A7E2 }, // SimInventoryItemExt
		{ 103319626, 0x628A7EA }, // HealthExt
		{ 103319627, 0x628A7EB }, // ConstructionExt
		{ 103319632, 0x628A7F0 }, // CombatExt
		{ 103319633, 0x628A7F1 }, // ProductionQueueExt
		{ 103319634, 0x628A7F2 }, // no active sample yet; lookup ID only
		{ 103319637, 0x628A7F5 }, // AbilityExt
		{ 103319697, 0x628A831 }, // WeaponExt
	};

	// Eagerly check reader and pure-decoder roles before per-object catches can hide a missing key.
	private static readonly long[] requiredDependencies = new long[]
	{
		112918560, 98614988, 102260400, 102260448, 102260680, 102261280, 112837064,
		64684536, 64683768, 64823200, 65511760, 65513472, 65713904,
		64430720, 64431008, 64432864, 64815280, 64815656,
		111878264, 111989640, 111989680,
		64584880, 64568064, 65313128, 65307976, 64559016, 13437424, 64557040, 64557304,
		103319618, 103319626, 103319627, 103319632, 103319633, 103319634, 103319637, 103319697
	};

	internal static void ValidateDependencies()
	{
		foreach (long legacy in requiredDependencies) Resolve(legacy);
	}

	internal static long Resolve(long legacy)
	{
		if (!candidates.TryGetValue(legacy, out long candidate))
			throw new InvalidOperationException("新版布局依赖尚待定位: 0x" + legacy.ToString("X"));
		return candidate;
	}

	internal static long LegacyTransform(long candidate)
	{
		foreach (long legacy in new long[] { 64430720, 64431008, 64432864, 64815280, 64815656 })
			if (candidates[legacy] == candidate) return legacy;
		throw new InvalidOperationException("新版界面变换类型尚待验证: 0x" + candidate.ToString("X"));
	}
}
