using System;
using System.IO;

namespace CoH3MemoryTool;

internal static class DefaultSounds61
{
	public static string FlankFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sounds", "被包抄了.mp3");

	public static string AlertFile(AlertKind kind)
	{
		return Path.Combine(path3: kind switch
		{
			AlertKind.BaseComplete => "造本.mp3", 
			AlertKind.BattleGroup => "战斗群.mp3", 
			AlertKind.EnemyArmourProduction => "装甲生产.mp3", 
			AlertKind.EnemySldFlare => "北非海军照明弹来了.mp3", 
			AlertKind.ArtilleryReady => "火炮就绪.mp3", 
			AlertKind.EnemyArtilleryBarrage => "炮击来袭.mp3", 
			AlertKind.GrenadeReady => "手雷升级.mp3", 
			_ => throw new ArgumentOutOfRangeException("kind"), 
		}, path1: AppDomain.CurrentDomain.BaseDirectory, path2: "sounds");
	}
}
