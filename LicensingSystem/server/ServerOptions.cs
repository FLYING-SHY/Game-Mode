using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Licensing.Server;

public record ProductOptions
{
    public required string DatabasePath { get; init; }
    public required string KeyDirectory { get; init; }
    public string ProductId { get; init; } = "CoH3FowTool";
    public string ProductName { get; init; } = "CoH3 战争先锋";
    public string CodePrefix { get; init; } = "COH3";
    public string MinimumVersion { get; init; } = "0.60.8";
    public string LatestVersion { get; init; } = "0.60.8";
    public string DownloadUrl { get; init; } = "";
    public string VersionNotes { get; init; } = "";

    internal void ValidateProduct()
    {
        if (string.IsNullOrWhiteSpace(DatabasePath) || string.IsNullOrWhiteSpace(KeyDirectory))
            throw new InvalidOperationException("数据库与独立密钥目录必须配置。");
        if (string.IsNullOrWhiteSpace(ProductId) || ProductId.Length > 100
            || ProductId != ProductId.Trim() || ProductId.Any(char.IsControl) || string.IsNullOrWhiteSpace(ProductName)
            || ProductName.Length > 200 || CodePrefix is null || !Regex.IsMatch(CodePrefix, "^[A-Za-z0-9]{1,16}$"))
            throw new InvalidOperationException("产品标识、名称或卡密前缀格式错误。");
        if (!Version.TryParse(MinimumVersion, out var minimum)
            || !Version.TryParse(LatestVersion, out var latest) || latest < minimum)
            throw new InvalidOperationException("版本配置必须有效，且最新版本不低于最低版本。");
        if (DownloadUrl is null || DownloadUrl.Length > 2048 || (DownloadUrl.Length > 0
            && (!Uri.TryCreate(DownloadUrl, UriKind.Absolute, out var download) || download.Scheme != "https")))
            throw new InvalidOperationException("下载地址必须为空或绝对 HTTPS 地址。");
    }
}

public sealed record ServerOptions : ProductOptions
{
    public string ListenUrl { get; init; } = "http://127.0.0.1:47831";
    public required string AdminToken { get; init; }
    // Additional products only. The original root product remains the default client product.
    public ProductOptions[] Products { get; init; } = [];
    public string? ZeppBackendToken { get; init; }
    public string[] AllowedHosts { get; init; } = ["127.0.0.1", "localhost", "::1"];
    public string[] TrustedProxies { get; init; } = [];
    public int ForwardLimit { get; init; } = 1;

    public static ServerOptions Load(string path)
    {
        var value = JsonSerializer.Deserialize<ServerOptions>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("授权服务配置为空。");
        value.Validate();
        return value;
    }

    public IReadOnlyList<ServerOptions> ProductConfigurations() => new[] { this with { Products = [] } }
        .Concat(Products.Select(product => this with
        {
            Products = [], ProductId = product.ProductId, ProductName = product.ProductName,
            CodePrefix = product.CodePrefix, DatabasePath = product.DatabasePath, KeyDirectory = product.KeyDirectory,
            MinimumVersion = product.MinimumVersion, LatestVersion = product.LatestVersion,
            DownloadUrl = product.DownloadUrl, VersionNotes = product.VersionNotes
        })).ToArray();

    public void Validate()
    {
        if (!Uri.TryCreate(ListenUrl, UriKind.Absolute, out var listen)
            || listen.Scheme is not ("http" or "https") || listen.AbsolutePath != "/"
            || !string.IsNullOrEmpty(listen.Query) || !string.IsNullOrEmpty(listen.UserInfo)
            || !IPAddress.TryParse(listen.Host.Trim('[', ']'), out _))
            throw new InvalidOperationException("ListenUrl 必须是明确 IP 的 HTTP/HTTPS 监听地址。");
        if (string.IsNullOrWhiteSpace(AdminToken) || AdminToken.Length < 32)
            throw new InvalidOperationException("至少 32 字符的管理员令牌必须配置。");
        if (Products is null || Products.Any(product => product is null))
            throw new InvalidOperationException("Products 必须是产品配置数组。");
        var configurations = ProductConfigurations();
        foreach (var product in configurations) product.ValidateProduct();
        if (configurations.Select(product => product.ProductId).Distinct(StringComparer.OrdinalIgnoreCase).Count() != configurations.Count)
            throw new InvalidOperationException("产品标识必须唯一；Products 只填写根产品之外的产品。");
        for (var i = 0; i < configurations.Count; i++)
        for (var j = i + 1; j < configurations.Count; j++)
        {
            var a = configurations[i]; var b = configurations[j];
            if (SamePath(a.DatabasePath, b.DatabasePath) || Within(a.KeyDirectory, b.KeyDirectory)
                || Within(b.KeyDirectory, a.KeyDirectory) || Within(a.DatabasePath, b.KeyDirectory)
                || Within(b.DatabasePath, a.KeyDirectory))
                throw new InvalidOperationException("每个产品必须使用独立数据库和互不重叠的密钥目录。");
        }
        if (configurations.Any(product => product.ProductId == "ZeppPortal")
            && (string.IsNullOrWhiteSpace(ZeppBackendToken) || ZeppBackendToken.Length < 32 || ZeppBackendToken == AdminToken))
            throw new InvalidOperationException("ZeppBackendToken 必须至少 32 字符且独立于管理员令牌。");
        if (AllowedHosts is null || AllowedHosts.Length == 0 || AllowedHosts.Any(host =>
                string.IsNullOrWhiteSpace(host) || host.Contains('*') || host.Contains('/')
                || host != host.Trim() || Uri.CheckHostName(host.Trim('[', ']')) == UriHostNameType.Unknown))
            throw new InvalidOperationException("AllowedHosts 必须列出明确主机名或 IP，且不接受通配符。");
        if (TrustedProxies is null || TrustedProxies.Any(proxy => !IPAddress.TryParse(proxy, out _))
            || ForwardLimit is < 1 or > 10)
            throw new InvalidOperationException("TrustedProxies 只接受明确 IP；ForwardLimit 范围为 1–10。");
    }

    private static string FullPath(string path) => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    private static bool SamePath(string a, string b) => FullPath(a).Equals(FullPath(b), StringComparison.OrdinalIgnoreCase);
    private static bool Within(string path, string directory) => SamePath(path, directory)
        || FullPath(path).StartsWith(FullPath(directory) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}
