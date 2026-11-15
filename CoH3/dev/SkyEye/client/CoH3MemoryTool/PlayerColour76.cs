using System;
using System.Drawing;

namespace CoH3MemoryTool;

internal static class PlayerColour76
{
	internal static Color Decode(uint packed)
	{
		if (packed >> 24 == 0)
		{
			throw new Exception("玩家配色数据无效，等待重新读取。");
		}
		return Color.FromArgb(255, (int)((packed >> 16) & 0xFF), (int)((packed >> 8) & 0xFF), (int)(packed & 0xFF));
	}
}
