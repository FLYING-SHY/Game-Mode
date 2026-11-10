using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Licensing.Server;

public sealed class KeygenApiException(string code, string detail, int status) : Exception(detail)
{
    public string Code { get; } = code;
    public int Status { get; } = status;
}

public sealed record KeygenLicense(string Id, bool Suspended, DateTimeOffset? Expiry, string Kind, int? Days);
public sealed record KeygenMachine(string Id, string Fingerprint, DateTimeOffset CreatedAt);

// The sole framework integration seam: actual JSON:API calls, never an in-memory licensing fallback.
public sealed class KeygenGateway(HttpClient http, ServerOptions options)
{
    public async Task<KeygenLicense> CreateLicenseAsync(ProductOptions product, string kind, int? days, CancellationToken ct)
    {
        // 同时写入通用键（product/kind/days）与历史键（coh3App/coh3Kind/coh3Days），保证旧版客户端仍能校验新签发的证书。
        using var json = await Send(HttpMethod.Post, "licenses", new
        {
            data = new
            {
                type = "licenses",
                attributes = new
                {
                    metadata = new
                    {
                        product = product.Id, kind, days,
                        coh3App = product.Id, coh3Kind = kind, coh3Days = days
                    }
                },
                relationships = new { policy = new { data = new { type = "policies", id = kind == "permanent" ? product.PermanentPolicyId : product.TimedPolicyId } } }
            }
        }, ct);
        return ReadLicense(json.RootElement.GetProperty("data"), product.Id);
    }

    public async Task<KeygenLicense> GetLicenseAsync(string id, string productId, CancellationToken ct)
    {
        using var json = await Send(HttpMethod.Get, "licenses/" + Uri.EscapeDataString(id), null, ct);
        return ReadLicense(json.RootElement.GetProperty("data"), productId);
    }

    public async Task SetExpiryAsync(string id, DateTimeOffset expiry, CancellationToken ct)
    {
        using var json = await Send(HttpMethod.Patch, "licenses/" + Uri.EscapeDataString(id),
            new { data = new { type = "licenses", id, attributes = new { expiry = expiry.ToUniversalTime().ToString("O") } } }, ct);
    }

    public async Task<IReadOnlyList<KeygenMachine>> GetMachinesAsync(string licenseId, CancellationToken ct)
    {
        using var json = await Send(HttpMethod.Get, "machines?license=" + Uri.EscapeDataString(licenseId) + "&limit=100", null, ct);
        return json.RootElement.GetProperty("data").EnumerateArray().Select(ReadMachine).ToArray();
    }

    public async Task<KeygenMachine> CreateMachineAsync(string licenseId, string deviceId, string productId, CancellationToken ct)
    {
        using var json = await Send(HttpMethod.Post, "machines", new
        {
            data = new
            {
                type = "machines",
                attributes = new { fingerprint = deviceId, platform = "Windows", name = productId },
                relationships = new { license = new { data = new { type = "licenses", id = licenseId } } }
            }
        }, ct);
        return ReadMachine(json.RootElement.GetProperty("data"));
    }

    public async Task ValidateAsync(string licenseId, string deviceId, CancellationToken ct)
    {
        using var json = await Send(HttpMethod.Post, "licenses/" + Uri.EscapeDataString(licenseId) + "/actions/validate",
            new { meta = new { scope = new { fingerprint = deviceId } } }, ct);
        var meta = json.RootElement.GetProperty("meta");
        if (!meta.GetProperty("valid").GetBoolean())
            throw new KeygenApiException(meta.GetProperty("code").GetString() ?? "INVALID",
                "Keygen 许可证验证未通过。", 403);
    }

    public async Task<string> CheckoutAsync(string machineId, CancellationToken ct)
    {
        // An empty query value is Keygen's documented nullable integer, not 0 (minimum non-null TTL is 3600).
        using var json = await Send(HttpMethod.Post, "machines/" + Uri.EscapeDataString(machineId)
            + "/actions/check-out?ttl=&include=license&algorithm=base64%2Brsa-sha256", null, ct);
        return json.RootElement.GetProperty("data").GetProperty("attributes").GetProperty("certificate").GetString()
            ?? throw new InvalidOperationException("Keygen 未返回机器许可证。");
    }

    public async Task SuspendAsync(string id, bool suspend, CancellationToken ct)
    {
        using var json = await Send(HttpMethod.Post, "licenses/" + Uri.EscapeDataString(id)
            + (suspend ? "/actions/suspend" : "/actions/reinstate"), null, ct);
    }

    public async Task ProbeAsync(CancellationToken ct)
    {
        foreach (var product in options.Catalog)
        foreach (var policyId in new[] { product.PermanentPolicyId, product.TimedPolicyId })
        {
            using var policy = await Send(HttpMethod.Get, "policies/" + Uri.EscapeDataString(policyId), null, ct);
            var attrs = policy.RootElement.GetProperty("data").GetProperty("attributes");
            if (!attrs.GetProperty("strict").GetBoolean() || attrs.GetProperty("maxMachines").GetInt32() != 1
                || attrs.GetProperty("duration").ValueKind != JsonValueKind.Null
                || attrs.GetProperty("authenticationStrategy").GetString() != "NONE"
                || !attrs.GetProperty("protected").GetBoolean()
                || attrs.GetProperty("overageStrategy").GetString() != "NO_OVERAGE")
                throw new InvalidOperationException("Keygen 策略需 strict、maxMachines=1、duration=null、protected、authenticationStrategy=NONE。");
        }
    }

    private async Task<JsonDocument> Send(HttpMethod method, string path, object? data, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, new Uri(new Uri(options.KeygenBaseUrl), path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.KeygenToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));
        request.Headers.Add("Keygen-Version", "1.7");
        // Local reverse proxy sets these; Host is pinned to the configured CE host.
        request.Headers.Host = options.KeygenHost;
        request.Headers.Add("X-Forwarded-Proto", "https");
        if (data is not null) request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/vnd.api+json");
        using var response = await http.SendAsync(request, ct);
        var text = await response.Content.ReadAsStringAsync(ct);
        JsonDocument json;
        try { json = JsonDocument.Parse(text); }
        catch (JsonException) { throw new KeygenApiException("INVALID_RESPONSE", "Keygen 返回非 JSON 响应。", (int)response.StatusCode); }
        if (!response.IsSuccessStatusCode)
        {
            var code = "KEYGEN_ERROR";
            if (json.RootElement.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
                code = errors[0].TryGetProperty("code", out var value) ? value.GetString() ?? code : code;
            json.Dispose();
            throw new KeygenApiException(code, "Keygen 请求失败：" + code, (int)response.StatusCode);
        }
        return json;
    }

    private static KeygenLicense ReadLicense(JsonElement data, string expectedProductId)
    {
        var attrs = data.GetProperty("attributes");
        var metadata = attrs.GetProperty("metadata");
        var application = ReadMetadata(metadata, "product", "coh3App");
        if (!string.Equals(application, expectedProductId, StringComparison.Ordinal))
            throw new InvalidOperationException("Keygen 许可证所属产品不匹配。");
        var kind = ReadMetadata(metadata, "kind", "coh3Kind");
        int? days = metadata.TryGetProperty("days", out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : metadata.TryGetProperty("coh3Days", out var legacy) && legacy.ValueKind == JsonValueKind.Number
                ? legacy.GetInt32() : null;
        return new(data.GetProperty("id").GetString()!, attrs.GetProperty("suspended").GetBoolean(),
            attrs.GetProperty("expiry").ValueKind == JsonValueKind.Null ? null : attrs.GetProperty("expiry").GetDateTimeOffset(),
            kind ?? string.Empty,
            days);
    }

    private static string? ReadMetadata(JsonElement metadata, string name, string legacyName)
    {
        if (metadata.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String) return value.GetString();
        if (metadata.TryGetProperty(legacyName, out var legacy) && legacy.ValueKind == JsonValueKind.String) return legacy.GetString();
        return null;
    }
    private static KeygenMachine ReadMachine(JsonElement data) => new(data.GetProperty("id").GetString()!,
        data.GetProperty("attributes").GetProperty("fingerprint").GetString()!,
        data.GetProperty("attributes").GetProperty("created").GetDateTimeOffset());
}
