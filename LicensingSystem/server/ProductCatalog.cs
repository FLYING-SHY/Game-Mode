using Microsoft.AspNetCore.DataProtection;

namespace Licensing.Server;

/// <summary>Resolves every management operation to one product's database and protection keys.</summary>
public sealed class ProductCatalog : IDisposable
{
    public sealed record Entry(ServerOptions Options, LicenseService Service)
    {
        public bool AccountBound => Options.ProductId == "ZeppPortal";
        public object Metadata() => new { productId = Options.ProductId, productName = Options.ProductName,
            codePrefix = Options.CodePrefix.ToUpperInvariant(), accountBound = AccountBound };
    }

    private readonly Dictionary<string, Entry> entries = new(StringComparer.Ordinal);
    private readonly List<IDisposable> owned = [];
    public Entry Default { get; }
    public IEnumerable<Entry> All => entries.Values;

    public ProductCatalog(ServerOptions options, LicenseService legacy, TimeProvider clock)
    {
        Default = new(options.ProductConfigurations()[0], legacy);
        entries.Add(options.ProductId, Default);
        try
        {
            foreach (var product in options.ProductConfigurations().Skip(1))
            {
                var services = new ServiceCollection();
                var protection = services.AddDataProtection().SetApplicationName("Licensing.Native.v1")
                    .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(product.KeyDirectory)));
                if (OperatingSystem.IsWindows()) protection.ProtectKeysWithDpapi();
                var provider = services.BuildServiceProvider(); owned.Add(provider);
                var signer = new LeaseSigner(product); owned.Add(signer);
                entries.Add(product.ProductId, new(product,
                    new LicenseService(product, provider.GetRequiredService<IDataProtectionProvider>(), clock, signer)));
            }
        }
        catch { Dispose(); throw; }
    }

    public Entry Get(string? productId) => string.IsNullOrEmpty(productId) ? Default
        : entries.TryGetValue(productId, out var entry) ? entry
        : throw new LicensingException("invalid_product", "所选产品不存在。", 400);

    public Entry Selected(HttpContext context)
    {
        var values = context.Request.Query["productId"];
        if (values.Count > 1 || values.Count == 1 && string.IsNullOrWhiteSpace(values[0]))
            throw new LicensingException("invalid_product", "请选择明确产品。", 400);
        return Get(values.Count == 0 ? null : values[0]);
    }

    public void Dispose()
    {
        for (var i = owned.Count - 1; i >= 0; i--) owned[i].Dispose();
        owned.Clear();
    }
}
