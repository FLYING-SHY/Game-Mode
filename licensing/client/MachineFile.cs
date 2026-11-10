using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CoH3.Licensing;

// Keygen's documented machine file format. Cryptography is provided by .NET RSA.
internal static class MachineFile
{
    internal const string Application = "CoH3FowTool";
    internal const string Begin = "-----BEGIN MACHINE FILE-----";
    internal const string End = "-----END MACHINE FILE-----";

    internal static VerifiedLicense Verify(string certificate, string publicKeyPem, string deviceId)
    {
        if (string.IsNullOrWhiteSpace(certificate) || certificate.Length > 128 * 1024 || !certificate.TrimStart().StartsWith(Begin, StringComparison.Ordinal)
            || !certificate.TrimEnd().EndsWith(End, StringComparison.Ordinal))
            throw new FormatException("许可证文件格式错误。");

        var encoded = certificate.Trim()[Begin.Length..^End.Length];
        using var outer = JsonDocument.Parse(Convert.FromBase64String(encoded));
        var root = outer.RootElement;
        if (root.GetProperty("alg").GetString() != "base64+rsa-sha256")
            throw new FormatException("许可证签名算法不受支持。");
        var enc = root.GetProperty("enc").GetString() ?? throw new FormatException("许可证内容缺失。");
        var signature = Convert.FromBase64String(root.GetProperty("sig").GetString()!);
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        if (rsa.KeySize < 2048 || !rsa.VerifyData(Encoding.UTF8.GetBytes("machine/" + enc), signature,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            throw new CryptographicException("许可证签名验证失败。");

        using var inner = JsonDocument.Parse(Convert.FromBase64String(enc));
        var body = inner.RootElement;
        var meta = body.GetProperty("meta");
        // Both modes deliberately use a perpetual snapshot; timed grants still require startup confirmation.
        if (meta.GetProperty("expiry").ValueKind != JsonValueKind.Null
            || meta.GetProperty("ttl").ValueKind != JsonValueKind.Null)
            throw new FormatException("此产品要求无快照刷新期限的机器许可证。");
        var issued = meta.GetProperty("issued").GetDateTimeOffset().ToUniversalTime();
        var machine = body.GetProperty("data");
        if (machine.GetProperty("type").GetString() != "machines")
            throw new FormatException("许可证对象类型错误。");
        var machineId = machine.GetProperty("id").GetString()!;
        var attributes = machine.GetProperty("attributes");
        var fingerprint = attributes.GetProperty("fingerprint").GetString();
        if (!string.Equals(fingerprint, deviceId, StringComparison.Ordinal))
            throw new DeviceMismatchException();
        var activated = attributes.GetProperty("created").GetDateTimeOffset().ToUniversalTime();
        var licenseId = machine.GetProperty("relationships").GetProperty("license")
            .GetProperty("data").GetProperty("id").GetString()!;
        var licenses = body.GetProperty("included").EnumerateArray()
            .Where(x => x.GetProperty("type").GetString() == "licenses"
                && x.GetProperty("id").GetString() == licenseId).ToArray();
        if (licenses.Length != 1) throw new FormatException("许可证关联缺失或重复。");
        var license = licenses[0].GetProperty("attributes");
        var metadata = license.GetProperty("metadata");
        if (metadata.GetProperty("coh3App").GetString() != Application)
            throw new FormatException("许可证不属于当前应用。");
        var kind = metadata.GetProperty("coh3Kind").GetString() switch
        {
            "permanent" => LicenseKind.Permanent,
            "timed" => LicenseKind.Timed,
            _ => throw new FormatException("许可证未包含有效的签名授权种类。")
        };
        DateTimeOffset? expiry = license.GetProperty("expiry").ValueKind == JsonValueKind.Null
            ? null : license.GetProperty("expiry").GetDateTimeOffset().ToUniversalTime();
        if ((kind == LicenseKind.Permanent && expiry is not null)
            || (kind == LicenseKind.Timed && (expiry is null || expiry <= activated)))
            throw new FormatException("签名授权种类与期限不一致。");
        if (license.GetProperty("suspended").GetBoolean())
            throw new FormatException("签名许可证处于停用状态。");
        if (string.IsNullOrWhiteSpace(machineId) || string.IsNullOrWhiteSpace(licenseId) || issued < activated)
            throw new FormatException("许可证的签名标识或时间无效。");
        return new(certificate, licenseId, machineId, deviceId, kind, activated, expiry, issued);
    }
}

internal sealed class DeviceMismatchException : Exception
{
    public DeviceMismatchException() : base("许可证与当前设备不匹配。") { }
}
