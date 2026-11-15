namespace CoH3MemoryTool;

internal sealed class EnemyChoice
{
	public PlayerInfoRow Player;

	public override string ToString()
	{
		return "敌方" + (Player.Slot + 1) + " · " + Player.Name + " · " + ChineseNames.Group(Player);
	}
}
