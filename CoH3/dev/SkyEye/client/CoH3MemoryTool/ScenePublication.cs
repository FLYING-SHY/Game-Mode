using System;

namespace CoH3MemoryTool;

// A clear revokes work already in progress, even if a new scene has the same MatchKey.
// Ordinary snapshot refreshes keep the generation so a renderer can finish its frame.
internal sealed class ScenePublication<T> where T : class
{
	private readonly object gate = new object();

	private T scene;

	private long generation;

	internal void Publish(T next)
	{
		lock (gate)
		{
			if (next == null) generation++;
			scene = next;
		}
	}

	internal T Capture(out long token)
	{
		lock (gate)
		{
			token = generation;
			return scene;
		}
	}

	// Hold the gate through the final presentation: once ClearData returns, an old
	// frame cannot pass a check and then become visible after the revocation.
	internal bool TryPresent(long token, Action present)
	{
		lock (gate)
		{
			if (scene == null || token != generation) return false;
			present();
			return true;
		}
	}
}
