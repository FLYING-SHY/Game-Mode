using System.Text.Json.Serialization;

namespace Licensing.Server;

public sealed record ZeppEntitlementRequest([property: JsonPropertyName("account_id")] string? AccountId);
public sealed record ZeppRedeemRequest(
    [property: JsonPropertyName("account_id")] string? AccountId,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("request_id")] string? RequestId);
public sealed record ZeppGrant(
    [property: JsonPropertyName("redemption_id")] string RedemptionId,
    [property: JsonPropertyName("starts_at")] string StartsAt,
    [property: JsonPropertyName("expires_at")] string? ExpiresAt,
    [property: JsonPropertyName("permanent")] bool Permanent,
    [property: JsonPropertyName("suspended")] bool Suspended,
    [property: JsonPropertyName("revoked")] bool Revoked);
public sealed record ZeppEntitlementResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("product_id")] string ProductId,
    [property: JsonPropertyName("account_id")] string AccountId,
    [property: JsonPropertyName("grants")] IReadOnlyList<ZeppGrant> Grants);
public sealed record ZeppRedemptionResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("product_id")] string ProductId,
    [property: JsonPropertyName("account_id")] string AccountId,
    [property: JsonPropertyName("receipt")] ZeppGrant Receipt,
    [property: JsonPropertyName("grants")] IReadOnlyList<ZeppGrant> Grants);
