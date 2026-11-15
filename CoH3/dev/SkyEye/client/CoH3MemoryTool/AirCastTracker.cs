using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3MemoryTool;

internal sealed class AirCastTracker
{
	private readonly Dictionary<string, AirCast> casts = new Dictionary<string, AirCast>();

	private readonly Dictionary<string, AirCast> claimed = new Dictionary<string, AirCast>();

	private HashSet<string> present = new HashSet<string>();

	private readonly HashSet<string> ended = new HashSet<string>();

	public void Clear()
	{
		casts.Clear();
		claimed.Clear();
		present.Clear();
		ended.Clear();
	}

	public bool Claims(string plane)
	{
		return claimed.ContainsKey(plane);
	}

	public List<AirZone> Update(IEnumerable<AirZone> observed, IEnumerable<AirPlane> planes, double now)
	{
		AirZone[] array = observed.ToArray();
		HashSet<string> hashSet = new HashSet<string>(array.Select((AirZone a) => a.Key));
		Dictionary<string, AirPlane> flying = planes.ToDictionary((AirPlane airPlane) => airPlane.Key);
		AirZone[] array2 = array;
		foreach (AirZone airZone in array2)
		{
			if (!ended.Contains(airZone.Key))
			{
				if (!casts.TryGetValue(airZone.Key, out var value))
				{
					AirCast airCast = new AirCast();
					airCast.Zone = airZone;
					airCast.Definition = AirCatalogue.Find(airZone.Name);
					airCast.Born = now;
					value = airCast;
					casts[airZone.Key] = value;
				}
				value.LastSeen = now;
			}
		}
		present = hashSet;
		foreach (AirPlane p in flying.Values.OrderBy((AirPlane airPlane) => airPlane.Born))
		{
			if (claimed.TryGetValue(p.Key, out var value2) && casts.Values.Contains(value2))
			{
				continue;
			}
			AirCast airCast2 = (from a in casts.Values
				where a.Zone.Owner == p.Owner && p.Born >= a.Born - 300.0 && p.Born <= a.Born + 6000.0 && a.Aircraft.Count < a.Definition.Planes && a.Definition.Aircraft.Any((string n) => p.Path.EndsWith("\\" + n, StringComparison.Ordinal))
				orderby a.Born descending
				select a).FirstOrDefault();
			if (airCast2 != null)
			{
				airCast2.Aircraft.Add(p.Key);
				airCast2.Seen = true;
				claimed[p.Key] = airCast2;
			}
		}
		string[] array3 = casts.Keys.ToArray();
		foreach (string text in array3)
		{
			AirCast c = casts[text];
			bool flag = c.Aircraft.Any((string k) => flying.ContainsKey(k) && !flying[k].Finished && (!flying[k].PositionKnown || !AirLogic.Passed(c.Zone, c.Definition, flying[k].X, flying[k].Z)));
			bool flag2 = c.Aircraft.Count >= c.Definition.Planes || now - c.Born > 6000.0;
			if ((c.Seen && !flag && flag2) || (!c.Seen && now - c.Born > 6000.0))
			{
				casts.Remove(text);
				ended.Add(text);
			}
		}
		string[] array4 = claimed.Keys.Where((string k) => !flying.ContainsKey(k)).ToArray();
		foreach (string key in array4)
		{
			claimed.Remove(key);
		}
		return casts.Values.Select(delegate(AirCast airCast3)
		{
			AirZone zone = airCast3.Zone;
			return new AirZone
			{
				Key = zone.Key,
				Name = airCast3.Definition.Name,
				Owner = zone.Owner,
				Slot = zone.Slot,
				Colour = zone.Colour,
				X = zone.X - zone.DZ * airCast3.Definition.Lateral,
				Y = zone.Y,
				Z = zone.Z + zone.DX * airCast3.Definition.Lateral,
				DX = zone.DX,
				DZ = zone.DZ,
				HalfWidth = airCast3.Definition.HalfWidth,
				Length = airCast3.Definition.Length,
				Offset = airCast3.Definition.Offset,
				Point = airCast3.Definition.Point
			};
		}).ToList();
	}
}
