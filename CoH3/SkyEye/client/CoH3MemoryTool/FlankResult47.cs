namespace CoH3MemoryTool;

internal sealed class FlankResult47
{
	public int Friends;

	public int Enemies;

	public int IncomingFriends;

	public int IncomingEnemies;

	public bool Danger
	{
		get
		{
			if (Enemies <= Friends)
			{
				return IncomingEnemies > IncomingFriends;
			}
			return true;
		}
	}
}
