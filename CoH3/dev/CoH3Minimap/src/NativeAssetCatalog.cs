using System.Reflection;

namespace CoH3Minimap;

// Icons are embedded; exact scenario paths can also be read from the installed archive.
internal sealed class NativeAssetCatalog : IDisposable
{
    private readonly Assembly _assembly = typeof(NativeAssetCatalog).Assembly;
    private readonly Dictionary<string, string> _resources = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Bitmap?> _loaded = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _archiveAttempted = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();
    private bool _disposed;

    internal NativeAssetCatalog()
    {
        foreach (string name in _assembly.GetManifestResourceNames())
        {
            string normalized = name.Replace('\\', '/');
            if (normalized.StartsWith("native/", StringComparison.Ordinal))
                _resources[normalized[7..]] = name;
        }
    }

    // Run on the capture worker, never in Paint: first-time BC7 decoding can take time.
    internal void PrepareMap(string gameDirectory, string? nativePath)
    {
        string? key = Normalize(nativePath);
        if (key is null || !key.StartsWith("scenarios/", StringComparison.OrdinalIgnoreCase)) return;
        if (_resources.ContainsKey(key)) { Resolve(nativePath); return; }
        lock (_gate)
        {
            if (_disposed || !_archiveAttempted.Add(key)) return;
        }
        string rrtexPath = "data:" + key[..^4] + ".rrtex";
        var image = NativeArchiveAssets.TryLoad(gameDirectory, rrtexPath);
        Store(key, image);
    }

    internal Bitmap? Resolve(string? path)
    {
        string? key = Normalize(path);
        if (key is null) return null;
        lock (_gate)
        {
            if (_disposed) return null;
            if (_loaded.TryGetValue(key, out var cached)) return cached;
        }
        Bitmap? bitmap = null;
        if (_resources.TryGetValue(key, out string? resource))
        {
            using var stream = _assembly.GetManifestResourceStream(resource);
            if (stream is not null)
            {
                using var decoded = new Bitmap(stream);
                bitmap = new Bitmap(decoded);
            }
        }
        return Store(key, bitmap);
    }

    private Bitmap? Store(string key, Bitmap? bitmap)
    {
        lock (_gate)
        {
            if (_disposed) { bitmap?.Dispose(); return null; }
            if (_loaded.TryGetValue(key, out var cached) && cached is not null)
            { bitmap?.Dispose(); return cached; }
            _loaded[key] = bitmap;
            return bitmap;
        }
    }

    private static string? Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        string key = path.Replace('\\', '/');
        const string component = "/WPFGUI;component/";
        if (key.StartsWith(component, StringComparison.OrdinalIgnoreCase)) key = key[component.Length..];
        if (key.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) key = key[5..];
        key = key.TrimStart('/');
        if ((!key.StartsWith("icons/", StringComparison.OrdinalIgnoreCase) && !key.StartsWith("scenarios/", StringComparison.OrdinalIgnoreCase)) ||
            key.Split('/').Any(segment => segment is ".." or ".")) return null;
        if (key.EndsWith(".rrtex", StringComparison.OrdinalIgnoreCase)) key = key[..^6] + ".png";
        else if (!key.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) key += ".png";
        return key;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var image in _loaded.Values) image?.Dispose();
            _loaded.Clear();
        }
    }
}
