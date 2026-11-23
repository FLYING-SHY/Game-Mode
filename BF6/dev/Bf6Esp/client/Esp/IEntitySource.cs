namespace Bf6Esp;

/// <summary>
/// 实体数据来源抽象。
///
/// Frostbite 4 的 ECS 布局还在逆向中，因此读取器只依赖这个接口：
///   - ProfileEntitySource      —— 用档案里的 Offsets/Structs 直接读；
///   - CalibrationEntitySource  —— 用校准模式产出的解析结果读。
/// 任何一项读不到时返回 false，由读取器记录日志并跳过，绝不猜值。
/// </summary>
internal interface IEntitySource
{
	/// <summary>来源名称（用于日志）。</summary>
	string Name { get; }

	/// <summary>取视图投影矩阵（16 个 float，行主序 m[row*4+col]）。</summary>
	bool TryGetViewProjection(out float[] matrix);

	/// <summary>取本地玩家队伍 id。</summary>
	bool TryGetLocalTeam(out int teamId);

	/// <summary>取本地玩家实体地址。</summary>
	bool TryGetLocalPlayerAddress(out ulong address);

	/// <summary>取本地玩家坐标（用于计算距离）。</summary>
	bool TryGetLocalPosition(out float x, out float y, out float z);

	/// <summary>取实体数量。</summary>
	bool TryGetEntityCount(out int count);

	/// <summary>取第 index 个实体地址。</summary>
	bool TryGetEntityAddress(int index, out ulong address);

	/// <summary>取实体坐标。</summary>
	bool TryGetPosition(ulong address, out float x, out float y, out float z);

	/// <summary>取实体血量。</summary>
	bool TryGetHealth(ulong address, out float current, out float max);

	/// <summary>取实体名字。</summary>
	bool TryGetName(ulong address, out string name);

	/// <summary>取实体队伍 id。</summary>
	bool TryGetTeam(ulong address, out int teamId);
}
