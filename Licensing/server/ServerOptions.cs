using System.Net;
using System.Text.Json;

namespace Licensing.Server;

/// <summary>
/// 一个产品（游戏修改器）在授权系统中的注册信息：独立的产品标识、独立的 Keygen 策略与独立的授权码前缀。
/// </summary>
public sealed record ProductOptions
{
    public required string Id { get; init; }
    public string DisplayName { get; init; } = "";
    public required string PermanentPolicyId { get; init; }
    public required string TimedPolicyId { get; init; }
    public string CodePrefix { get; init; } = "";

    /// <summary>授权码前缀；未显式配置时由产品 id 派生（大写字母数字前 4 位）。</summary>
    public string EffectiveCodePrefix => string.IsNullOrWhiteSpace(CodePrefix)
        ? DeriveCodePrefix(Id) : CodePrefix.Trim();

    public static string DeriveCodePrefix(string id)
    {
        var text = new string(id.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return text.Length == 0 ? "LIC" : text[..Math.Min(4, text.Length)];
    }
}

public sealed record ServerOptions
{
    public string ListenUrl { get; init; } = "http://127.0.0.1:47831";
    public required string DatabasePath { get; init; }
    public required string AdminToken { get; init; }
    public required string KeygenBaseUrl { get; init; }
    public string KeygenHost { get; init; } = "api.keygen.localhost";
    public required string KeygenToken { get; init; }
    public required string VerificationPublicKeyPem { get; init; }

    /// <summary>旧版单产品配置：永久策略 id。产品清单为空时作为默认产品的策略使用。</summary>
    public string PermanentPolicyId { get; init; } = "";

    /// <summary>旧版单产品配置：限时策略 id。产品清单为空时作为默认产品的策略使用。</summary>
    public string TimedPolicyId { get; init; } = "";

    /// <summary>请求未携带产品标识时使用的默认产品。</summary>
    public string DefaultProductId { get; init; } = ProductIds.Default;

    /// <summary>产品清单；为空时回落到旧版单产品配置。</summary>
    public IReadOnlyList<ProductOptions> Products { get; init; } = Array.Empty<ProductOptions>();

    /// <summary>归一化后的产品清单（兼容旧版单产品配置）。</summary>
    public IReadOnlyList<ProductOptions> Catalog
    {
        get
        {
            if (Products.Count > 0) return Products;
            var id = string.IsNullOrWhiteSpace(DefaultProductId) ? ProductIds.Default : DefaultProductId.Trim();
            return new[]
            {
                new ProductOptions
                {
                    Id = id,
                    DisplayName = id,
                    PermanentPolicyId = PermanentPolicyId,
                    TimedPolicyId = TimedPolicyId
                }
            };
        }
    }

    /// <summary>按产品标识解析产品；未指定时回落到默认产品。</summary>
    public ProductOptions ResolveProduct(string? id)
    {
        var catalog = Catalog;
        if (string.IsNullOrWhiteSpace(id))
        {
            var fallback = catalog.FirstOrDefault(p =>
                string.Equals(p.Id, DefaultProductId, StringComparison.OrdinalIgnoreCase));
            return fallback ?? catalog[0];
        }
        var match = catalog.FirstOrDefault(p => string.Equals(p.Id, id.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match is null) throw new LicensingException("UnknownProduct", "未知的产品标识。", 400);
        return match;
    }

    public static ServerOptions Load(string path)
    {
        var value = JsonSerializer.Deserialize<ServerOptions>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("授权服务配置为空。");
        value.Validate();
        return value;
    }

    public void Validate()
    {
        var listen = new Uri(ListenUrl);
        if (listen.Scheme != "http" || !IPAddress.TryParse(listen.Host, out var ip)
            || !IPAddress.IsLoopback(ip) || listen.AbsolutePath != "/")
            throw new InvalidOperationException("管理和授权适配服务只允许绑定 loopback IP。");
        var backend = new Uri(KeygenBaseUrl);
        if ((backend.Scheme != "https" && !(backend.Scheme == "http" && backend.IsLoopback))
            || !KeygenBaseUrl.EndsWith('/'))
            throw new InvalidOperationException("Keygen 地址需 HTTPS 或本机 HTTP，并以 / 结尾。");
        if (AdminToken.Length < 32 || string.IsNullOrWhiteSpace(KeygenToken)
            || string.IsNullOrWhiteSpace(VerificationPublicKeyPem))
            throw new InvalidOperationException("服务端凭据或账户公钥未配置。");
        var catalog = Catalog;
        if (catalog.Count == 0) throw new InvalidOperationException("至少需要配置一个授权产品。");
        foreach (var product in catalog)
        {
            if (string.IsNullOrWhiteSpace(product.Id)
                || string.IsNullOrWhiteSpace(product.PermanentPolicyId)
                || string.IsNullOrWhiteSpace(product.TimedPolicyId))
                throw new InvalidOperationException("产品 " + product.Id + " 缺少永久或限时策略 id。");
        }
        var ids = catalog.Select(p => p.Id).ToArray();
        if (ids.Distinct(StringComparer.OrdinalIgnoreCase).Count() != ids.Length)
            throw new InvalidOperationException("产品标识重复。");
    }
}
