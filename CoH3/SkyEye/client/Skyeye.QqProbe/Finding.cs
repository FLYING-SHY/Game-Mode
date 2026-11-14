using System;
using System.Collections.Generic;

namespace Skyeye.QqProbe;

internal sealed class Finding
{
	public string Number;

	public readonly SortedSet<string> Paths = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

	public readonly SortedSet<string> Evidence = new SortedSet<string>(StringComparer.Ordinal);

	public bool HasQqData;
}
