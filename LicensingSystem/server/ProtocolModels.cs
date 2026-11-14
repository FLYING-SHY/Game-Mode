using System.Text.Json.Serialization;

namespace Licensing.Server;

public sealed record ActivationRequest
{
    [JsonPropertyName("code")] public string? Code { get; init; }
    [JsonPropertyName("device_id")] public string? DeviceId { get; init; }
    [JsonPropertyName("steam_id64")] public string? SteamId64 { get; init; }
    [JsonPropertyName("installation_id")] public string? InstallationId { get; init; }
    [JsonPropertyName("client_version")] public string? ClientVersion { get; init; }
    [JsonPropertyName("nonce")] public string? Nonce { get; init; }
    [JsonPropertyName("request_id")] public string? RequestId { get; init; }
    [JsonPropertyName("game_name")] public string? GameName { get; init; }
}

public sealed record HeartbeatRequest
{
    [JsonPropertyName("device_id")] public string? DeviceId { get; init; }
    [JsonPropertyName("steam_id64")] public string? SteamId64 { get; init; }
    [JsonPropertyName("installation_id")] public string? InstallationId { get; init; }
    [JsonPropertyName("client_version")] public string? ClientVersion { get; init; }
    [JsonPropertyName("nonce")] public string? Nonce { get; init; }
}

public sealed record LeaseResponse
{
    [JsonPropertyName("state")] public required string State { get; init; }
    [JsonPropertyName("lease_kid"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? LeaseKid { get; init; }
    [JsonPropertyName("lease_signature"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? LeaseSignature { get; init; }
    [JsonPropertyName("server_time")] public long ServerTime { get; init; }
    [JsonPropertyName("lease_until"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public long? LeaseUntil { get; init; }
    [JsonPropertyName("expires_at"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public long? ExpiresAt { get; init; }
    [JsonPropertyName("session_token"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SessionToken { get; init; }
    [JsonPropertyName("license_kind"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? LicenseKind { get; init; }
    [JsonPropertyName("message"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Message { get; init; }
    [JsonPropertyName("minimum"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Minimum { get; init; }
    [JsonPropertyName("latest"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Latest { get; init; }
    [JsonPropertyName("download_url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? DownloadUrl { get; init; }
}

public sealed record IssueRequest(string Kind, long? DurationSeconds = null, int Quantity = 1, string? Note = "");
public sealed record SuspensionRequest(bool Suspended);
public sealed record RenewRequest(long DurationSeconds);
public sealed record CodeRow(string Id, string Code, string CodeSuffix, string Kind, long? DurationSeconds,
    string Note, long CreatedAtUnix, long? ActivatedAtUnix, long? ExpiresAtUnix, string? BoundDeviceId,
    string? LastSteamId, string State, int ActiveSessionCount);
public sealed record CodeStats(int Total, int Unused, int Active, int Expired, int Suspended, int Online);
public sealed record CodePage(IReadOnlyList<CodeRow> Items, int Total, int Page, int PageSize, CodeStats Stats);
public sealed record IssuedCodes(IReadOnlyList<CodeRow> Items, int Count);
public sealed record SessionRow(string Id, string CodeId, string CodeSuffix, string DeviceId,
    string InstallationId, string SteamId, string? GameName, long CreatedAtUnix, long LastSeenAtUnix,
    long LeaseUntilUnix, long? RevokedAtUnix, string State);
public sealed record AuditRow(string Id, long OccurredAtUnix, string Action, string? CodeId, string Details);
public sealed record AuditPage(IReadOnlyList<AuditRow> Items, int Total, int Page, int PageSize);

public sealed class LicensingException(string code, string message, int status = 400) : Exception(message)
{
    public string Code { get; } = code;
    public int Status { get; } = status;
}
