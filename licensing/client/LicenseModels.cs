namespace CoH3.Licensing;

public enum LicenseKind { Unknown, Permanent, Timed }

public sealed record LicenseStatus(
    bool CanEnable,
    string Code,
    string Message,
    LicenseKind Kind = LicenseKind.Unknown,
    DateTimeOffset? ExpiresAtUtc = null,
    string? DeviceId = null);

public sealed record LicenseClientOptions
{
    public required Uri BaseUri { get; init; }
    public required string VerificationPublicKeyPem { get; init; }
    public required string LicenseFilePath { get; init; }
}

internal sealed record ActivationRequest(string Code, string DeviceId);
internal sealed record ValidationRequest(string Certificate, string DeviceId);
internal sealed record LicenseApiResponse(string Code, string Message, string? Certificate = null);
internal sealed record VerifiedLicense(string Certificate, string LicenseId, string MachineId,
    string DeviceId, LicenseKind Kind, DateTimeOffset ActivatedAtUtc,
    DateTimeOffset? ExpiresAtUtc, DateTimeOffset IssuedAtUtc);
