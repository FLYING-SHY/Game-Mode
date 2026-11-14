using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class BarrageDisplayTracker
{
	private sealed class Item
	{
		public BarrageRecord Record;

		public DateTime LastSeen;
	}

	private readonly Dictionary<string, Item> items = new Dictionary<string, Item>();

	private string match;

	public void Clear()
	{
		items.Clear();
		match = null;
	}

	public List<BarrageRecord> Update(string key, IEnumerable<BarrageRecord> source, DateTime now, IEnumerable<uint> unknownSlots = null)
	{
		if (match != key)
		{
			Clear();
			match = key;
		}
		HashSet<uint> unknown = new HashSet<uint>(unknownSlots ?? Enumerable.Empty<uint>());
		BarrageRecord[] array = (from r in source
			group r by r.Key into g
			select g.First()).ToArray();
		HashSet<string> keys = new HashSet<string>(array.Select((BarrageRecord r) => r.Key));
		BarrageRecord[] array2 = array;
		foreach (BarrageRecord barrageRecord in array2)
		{
			items[barrageRecord.Key] = new Item
			{
				Record = barrageRecord,
				LastSeen = now
			};
		}
		string[] array3 = (from p in items
			where !keys.Contains(p.Key) && (!unknown.Contains(p.Value.Record.Slot) || now - p.Value.LastSeen >= TimeSpan.FromSeconds(3.0))
			select p.Key).ToArray();
		foreach (string key2 in array3)
		{
			items.Remove(key2);
		}
		return items.Values.Select((Item v) => v.Record).ToList();
	}
}
