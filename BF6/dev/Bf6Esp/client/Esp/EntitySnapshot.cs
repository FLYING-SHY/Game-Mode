namespace Bf6Esp;

/// <summary>单个实体的只读快照：覆盖层每帧只消费这个结构，不再碰游戏内存。</summary>
internal sealed class EntitySnapshot
{
	/// <summary>实体对象地址。</summary>
	public ulong Address;

	public float X;

	public float Y;

	public float Z;

	/// <summary>与本地玩家的距离（米）。</summary>
	public float Distance;

	/// <summary>队伍 id。</summary>
	public int TeamId;

	/// <summary>当前血量。</summary>
	public float Health;

	/// <summary>最大血量。</summary>
	public float MaxHealth;

	/// <summary>玩家名（读不到时为空）。</summary>
	public string Name;

	/// <summary>是否本地玩家。</summary>
	public bool IsLocalPlayer;

	/// <summary>是否敌人（与本地玩家不同队）。</summary>
	public bool IsEnemy;
}
