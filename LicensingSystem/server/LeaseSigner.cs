using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Licensing.Server;

public sealed record LeasePublicKey(
    [property: JsonPropertyName("modulus")] string Modulus,
    [property: JsonPropertyName("exponent")] string Exponent,
    [property: JsonPropertyName("kid")] string Kid);

/// <summary>Server-owned persistent RSA key. Public export is an offline operation only.</summary>
public sealed class LeaseSigner : IDisposable
{
    public const string KeyFileName = "lease-signing-key.pk8.pem";
    private readonly RSA key;
    private readonly string productId;
    private readonly object sync = new();
    private readonly LeasePublicKey publicKey;

    public LeaseSigner(ServerOptions options)
    {
        productId = options.ProductId;
        var directory = Path.GetFullPath(options.KeyDirectory);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, KeyFileName);
        if (!File.Exists(path))
        {
            // Publish a complete key atomically. Parallel initializers keep the first key.
            var temporary = Path.Combine(directory, ".lease-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using var generated = RSA.Create(3072);
                File.WriteAllText(temporary, generated.ExportPkcs8PrivateKeyPem(), new UTF8Encoding(false));
                if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(temporary, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                try { File.Move(temporary, path, overwrite: false); }
                catch (IOException) when (File.Exists(path)) { }
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        key = RSA.Create();
        try
        {
            var pem = File.ReadAllText(path);
            if (!pem.StartsWith("-----BEGIN PRIVATE KEY-----", StringComparison.Ordinal))
                throw new CryptographicException("Lease signing key must be PKCS8 PEM.");
            key.ImportFromPem(pem);
            if (key.KeySize != 3072) throw new CryptographicException("Lease signing key must be RSA-3072.");
            var parameters = key.ExportParameters(false);
            publicKey = new(Convert.ToBase64String(parameters.Modulus!), Convert.ToBase64String(parameters.Exponent!),
                Convert.ToHexString(SHA256.HashData(parameters.Modulus!)).ToLowerInvariant()[..16]);
        }
        catch { key.Dispose(); throw; }
    }

    public LeasePublicKey ExportPublicKey() => publicKey;

    public static void ValidateNonce(string? nonce)
    {
        if (nonce is null || nonce.Length != 32 || nonce.Any(c => c is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
            throw new LicensingException("invalid_nonce", "nonce 必须为 32 位小写十六进制。", 400);
    }

    public LeaseResponse Sign(LeaseResponse response, string device, string steam, string installation, string nonce, string effectiveToken)
    {
        if (response.State != "active") return response;
        ValidateNonce(nonce);
        if (response.LeaseUntil is null || response.ExpiresAt is null || response.LicenseKind is not ("permanent" or "timed")
            || string.IsNullOrEmpty(effectiveToken))
            throw new CryptographicException("Active lease signing context is incomplete.");
        var message = string.Join("\n", new[]
        {
            "skyeye-lease-v2", productId, "active", device, steam, installation, nonce,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(effectiveToken))).ToLowerInvariant(),
            response.ServerTime.ToString(CultureInfo.InvariantCulture), response.LeaseUntil.Value.ToString(CultureInfo.InvariantCulture),
            response.ExpiresAt.Value.ToString(CultureInfo.InvariantCulture), response.LicenseKind, publicKey.Kid
        });
        byte[] signature;
        lock (sync) signature = key.SignData(Encoding.UTF8.GetBytes(message), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return response with { LeaseKid = publicKey.Kid, LeaseSignature = Convert.ToBase64String(signature) };
    }

    public void Dispose() => key.Dispose();
}
