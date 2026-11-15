using System;

namespace CoH3MemoryTool;

internal sealed class MinimapRetiredControl : Exception
{
	public MinimapRetiredControl(string message)
		: base(message)
	{
	}
}
