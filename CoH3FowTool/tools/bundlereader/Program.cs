using System;
using System.IO;
using System.Linq;
using System.Reflection;

// Reads a .NET single-file bundle with the SDK's own Microsoft.NET.HostModel.
// Usage: BundleReader <bundle.exe> <output-dir>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("usage: BundleReader <bundle.exe> <output-dir>");
            return 2;
        }

        var bundlePath = args[0];
        var outDir = args[1];
        Directory.CreateDirectory(outDir);

        var asm = typeof(Program).Assembly.GetReferencedAssemblies()
            .Select(a => { try { return Assembly.Load(a.FullName); } catch { return null; } })
            .FirstOrDefault(a => a?.GetName().Name == "Microsoft.NET.HostModel");
        if (asm is null)
        {
            asm = Assembly.LoadFrom(@"C:\Program Files\dotnet\sdk\9.0.315\Microsoft.NET.HostModel.dll");
        }

        foreach (var t in asm.GetExportedTypes().Where(t => t.Namespace?.Contains("Bundle") == true))
        {
            Console.WriteLine($"# type {t.FullName}");
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Console.WriteLine($"  {m}");
            }
            foreach (var p in t.GetProperties())
            {
                Console.WriteLine($"  prop {p.PropertyType.Name} {p.Name}");
            }
        }

        var manifestType = asm.GetExportedTypes().FirstOrDefault(t => t.Name == "Manifest")
            ?? throw new InvalidOperationException("No public Manifest type in Microsoft.NET.HostModel.Bundle.");
        var read = manifestType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => (m.Name is "Read" or "FromStream" or "Load" or "Parse")
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType == typeof(Stream))
            ?? throw new InvalidOperationException("Manifest has no static Read(Stream).");

        using var stream = File.OpenRead(bundlePath);
        object manifest;
        try
        {
            manifest = read.Invoke(null, new object[] { stream })!;
        }
        catch (TargetInvocationException ex)
        {
            Console.Error.WriteLine($"Manifest read failed: {ex.InnerException?.Message}");
            return 1;
        }

        Console.WriteLine($"manifest type: {manifest.GetType().FullName}");
        foreach (var p in manifest.GetType().GetProperties())
        {
            Console.WriteLine($"  {p.Name} = {p.GetValue(manifest)}");
        }

        var filesProp = manifest.GetType().GetProperties().FirstOrDefault(p => p.Name == "Files")
            ?? throw new InvalidOperationException("Manifest has no Files property.");
        var files = (System.Collections.IEnumerable)filesProp.GetValue(manifest)!;
        int count = 0;
        foreach (var file in files)
        {
            count++;
            var ft = file!.GetType();
            var name = (string)ft.GetProperty("RelativePath")!.GetValue(file)!;
            var offset = (long)ft.GetProperty("Offset")!.GetValue(file)!;
            var size = (long)ft.GetProperty("Size")!.GetValue(file)!;
            var compressed = ft.GetProperty("CompressedSize")!.GetValue(file) is long c ? c : 0L;
            var type = ft.GetProperty("Type")!.GetValue(file)!.ToString();
            Console.WriteLine($"{name}\toffset={offset}\tsize={size}\tcompressed={compressed}\ttype={type}");
        }
        Console.WriteLine($"total entries: {count}");
        return 0;
    }
}
