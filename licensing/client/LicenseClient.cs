using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace CoH3.Licensing;

public sealed class LicenseClient : IDisposable
{
    private readonly LicenseClientOptions options;
    private readonly HttpClient http;
    private readonly Func<string> fingerprint;
    private readonly TimeProvider clock;
    private readonly SemaphoreSlim gate = new(1, 1);
    private sealed record ClientState(VerifiedLicense? Grant, long ConfirmedTimestamp, LicenseStatus Status);
    private ClientState state = new(null, 0, new(false, "NotActivated", "尚未激活，请输入授权码。"));

    public LicenseClient(LicenseClientOptions options, HttpClient httpClient,
        Func<string>? deviceFingerprint = null, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        if (!options.BaseUri.IsAbsoluteUri || (options.BaseUri.Scheme != "https"
            && !(options.BaseUri.Scheme == "http" && options.BaseUri.IsLoopback)))
            throw new ArgumentException("授权服务须使用 HTTPS 或本机 loopback HTTP。", nameof(options));
        this.options = options;
        http = httpClient;
        fingerprint = deviceFingerprint ?? GetWindowsDeviceId;
        clock = timeProvider ?? TimeProvider.System;
    }

    public async Task<LicenseStatus> ActivateAsync(string code, CancellationToken ct = default)
    {
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 256)
                return SetFailure("InvalidCode", "请输入有效授权码。");
            return await RequestAsync("api/licenses/activate",
                new ActivationRequest(code.Trim(), fingerprint()), ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsLocalFailure(ex)) { return LocalFailure(ex); }
        finally { gate.Release(); }
    }

    public async Task<LicenseStatus> ValidateStartupAsync(CancellationToken ct = default)
    {
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            SetFailure("Checking", "正在验证授权；关闭、还原与状态查看仍可使用。");
            if (string.IsNullOrWhiteSpace(options.VerificationPublicKeyPem))
                return SetFailure("ConfigurationRequired", "授权公钥尚未配置；关闭、还原与状态查看仍可使用。");
            if (!File.Exists(options.LicenseFilePath))
                return SetFailure("NotActivated", "尚未激活，请输入授权码。");
            var file = new FileInfo(options.LicenseFilePath);
            if (file.Length > 128 * 1024) throw new FormatException("许可证文件过大。");
            var certificate = await File.ReadAllTextAsync(options.LicenseFilePath, ct).ConfigureAwait(false);
            var grant = MachineFile.Verify(certificate, options.VerificationPublicKeyPem, fingerprint());
            if (grant.Kind == LicenseKind.Permanent) return Confirm(grant);
            return await RequestAsync("api/licenses/validate",
                new ValidationRequest(certificate, grant.DeviceId), ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsLocalFailure(ex)) { return LocalFailure(ex); }
        finally { gate.Release(); }
    }

    // No network calls. Timed expiry uses signed server time plus monotonic elapsed time.
    public LicenseStatus CheckCurrentLicense()
    {
        var snapshot = Volatile.Read(ref state);
        var grant = snapshot.Grant;
        if (grant is null) return snapshot.Status;
        var serverNow = grant.IssuedAtUtc + clock.GetElapsedTime(snapshot.ConfirmedTimestamp);
        if (grant.Kind == LicenseKind.Timed && serverNow >= grant.ExpiresAtUtc)
            return new(false, "Expired", "限时授权已到期；关闭、还原与状态查看仍可使用。",
                grant.Kind, grant.ExpiresAtUtc, grant.DeviceId);
        return snapshot.Status;
    }

    private async Task<LicenseStatus> RequestAsync<T>(string path, T body, CancellationToken ct)
    {
        SetFailure("Checking", "正在连接授权服务；关闭、还原与状态查看仍可使用。");
        if (string.IsNullOrWhiteSpace(options.VerificationPublicKeyPem))
            return SetFailure("ConfigurationRequired", "授权公钥尚未配置；关闭、还原与状态查看仍可使用。");
        try
        {
            using var response = await http.PostAsJsonAsync(new Uri(options.BaseUri, path), body, ct)
                .ConfigureAwait(false);
            if (response.Content.Headers.ContentLength > 256 * 1024)
                return SetFailure("InvalidResponse", "授权服务响应过大。");
            var answer = await response.Content.ReadFromJsonAsync<LicenseApiResponse>(cancellationToken: ct)
                .ConfigureAwait(false);
            if (!response.IsSuccessStatusCode || answer?.Certificate is null)
                return SetFailure(answer?.Code ?? "ServiceError", answer?.Message ?? "授权服务返回异常状态。");
            var grant = MachineFile.Verify(answer.Certificate, options.VerificationPublicKeyPem, fingerprint());
            var directory = Path.GetDirectoryName(Path.GetFullPath(options.LicenseFilePath))!;
            Directory.CreateDirectory(directory);
            var temporary = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                await File.WriteAllTextAsync(temporary, answer.Certificate, ct).ConfigureAwait(false);
                File.Move(temporary, options.LicenseFilePath, overwrite: true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return Confirm(grant);
        }
        catch (HttpRequestException) { return SetFailure("NetworkUnavailable", "连接授权服务失败；关闭、还原与状态查看仍可使用。"); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { return SetFailure("NetworkUnavailable", "授权服务连接超时；关闭、还原与状态查看仍可使用。"); }
        catch (JsonException) { return SetFailure("InvalidResponse", "授权服务响应格式错误。"); }
    }

    private LicenseStatus Confirm(VerifiedLicense grant)
    {
        var status = new LicenseStatus(true, "Valid", grant.Kind == LicenseKind.Permanent ? "永久授权已验证。" : "限时授权已联网确认。",
            grant.Kind, grant.ExpiresAtUtc, grant.DeviceId);
        Volatile.Write(ref state, new ClientState(grant, clock.GetTimestamp(), status));
        return CheckCurrentLicense();
    }

    private LicenseStatus SetFailure(string code, string message)
    {
        var status = new LicenseStatus(false, code, message, DeviceId: TryGetDeviceId());
        Volatile.Write(ref state, new ClientState(null, 0, status));
        return status;
    }

    private LicenseStatus LocalFailure(Exception ex) => ex switch
    {
        DeviceMismatchException => SetFailure("DeviceMismatch", ex.Message),
        IOException or UnauthorizedAccessException => SetFailure("StorageError", "本机许可证读取或保存失败，请检查目录权限。"),
        PlatformNotSupportedException => SetFailure("DeviceUnavailable", "设备标识读取失败。"),
        _ => SetFailure("InvalidLicense", "许可证签名、应用、种类或内容验证失败。")
    };

    private static bool IsLocalFailure(Exception ex) => ex is CryptographicException or FormatException
        or JsonException or KeyNotFoundException or InvalidOperationException or IOException
        or UnauthorizedAccessException or ArgumentException or DeviceMismatchException or PlatformNotSupportedException;
    private string? TryGetDeviceId() { try { return fingerprint(); } catch { return null; } }

    private static string GetWindowsDeviceId()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
            .OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
        var machineGuid = key?.GetValue("MachineGuid") as string;
        if (string.IsNullOrWhiteSpace(machineGuid)) throw new PlatformNotSupportedException("MachineGuid missing");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            "CoH3FowTool|device-v1|" + machineGuid.Trim().ToLowerInvariant())));
    }

    public void Dispose() => gate.Dispose(); // HttpClient lifetime belongs to the caller.
}
