using System;
using System.IO;
using System.Reflection;

namespace CoH3MemoryTool;

internal static class AudioDependency
{
	private static bool installed;

	public static void Initialize()
	{
		if (installed)
		{
			return;
		}
		installed = true;
		AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs args)
		{
			if (new AssemblyName(args.Name).Name != "NAudio")
			{
				return (Assembly)null;
			}
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CoH3.Audio.NAudio.dll");
			if (stream != null)
			{
				using MemoryStream memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				return Assembly.Load(memoryStream.ToArray());
			}
			return (Assembly)null;
		};
	}
}
