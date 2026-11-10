using System.Net;
using System.Text.Json;

namespace CoH3.Licensing.Server;

public sealed record ServerOptions
{
    public string ListenUrl { get; init; } = "http://127.0.0.1:47831";
    public required string DatabasePath { get; init; }
    public required string AdminToken { get; init; }
    public required string KeygenBaseUrl { get; init; }
    public string KeygenHost { get; init; } = "api.keygen.localhost";
    public required string KeygenToken { get; init; }
    public required string PermanentPolicyId { get; init; }
    public required string TimedPolicyId { get; init; }
    public required string VerificationPublicKeyPem { get; init; }

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
            || string.IsNullOrWhiteSpace(PermanentPolicyId) || string.IsNullOrWhiteSpace(TimedPolicyId)
            || string.IsNullOrWhiteSpace(VerificationPublicKeyPem))
            throw new InvalidOperationException("服务端凭据、策略或账户公钥未配置。");
    }
}
