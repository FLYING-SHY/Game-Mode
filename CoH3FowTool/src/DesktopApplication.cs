#nullable enable

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Licensing;
using CoH3FowTool.Gui;

namespace CoH3FowTool;

internal static class DesktopApplication
{
    internal static int Run(string preferredGamePath)
    {
        using var singleInstance = new Mutex(true, @"Local\CoH3FowTool.Desktop", out bool firstInstance);
        if (!firstInstance)
        {
            System.Windows.Forms.MessageBox.Show("图形界面已在运行，请切换到已有窗口。", "CoH3FowTool");
            return 2;
        }
        try
        {
            System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            using var controller = DesktopController.Create(preferredGamePath);
            using var form = new MainForm(controller);
            System.Windows.Forms.Application.Run(form);
            return 0;
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show("界面启动失败：" + ex.Message, "CoH3FowTool",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            return 1;
        }
        finally { singleInstance.ReleaseMutex(); }
    }
}

internal sealed class DesktopController : IDesktopController, IDisposable
{
    private readonly LicenseClient _license;
    private readonly HttpClient _http;
    private readonly SemaphoreSlim _operations = new(1, 1);
    private readonly string _configurationError;
    private volatile bool _initialized;

    internal static string UserDataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CoH3FowTool");

    /// <summary>本修改器在授权系统中的产品标识。</summary>
    internal const string ProductId = "CoH3FowTool";

    internal static DesktopController Create(string preferredGamePath)
    {
        string configurationError = "";
        var options = new LicenseClientOptions
        {
            BaseUri = new Uri("http://127.0.0.1:47831/"),
            VerificationPublicKeyPem = "",
            LicenseFilePath = Path.Combine(UserDataDirectory, "license.lic"),
            ProductId = ProductId
        };
        try
        {
            using var resource = typeof(DesktopController).Assembly.GetManifestResourceStream(
                "CoH3FowTool.LicenseDeployment.json");
            if (resource is null)
                throw new InvalidDataException("此构建尚未配置授权服务，请由发行者完成服务与公钥配置后重新构建。");
            var deployment = JsonSerializer.Deserialize<PublicDeployment>(resource,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidDataException("授权服务配置为空。");
            if (deployment.VerificationPublicKeyPem.Contains("PRIVATE KEY", StringComparison.Ordinal))
                throw new InvalidDataException("用户端配置只接受公钥。");
            using var rsa = RSA.Create();
            rsa.ImportFromPem(deployment.VerificationPublicKeyPem);
            if (rsa.KeySize < 2048) throw new InvalidDataException("授权公钥强度不足。");
            var endpoint = new Uri(deployment.ServiceUri, UriKind.Absolute);
            if (endpoint.Scheme != Uri.UriSchemeHttps &&
                !(endpoint.Scheme == Uri.UriSchemeHttp && endpoint.IsLoopback))
                throw new InvalidDataException("授权服务必须使用 HTTPS；本机验证可使用 loopback HTTP。");
            options = options with
            {
                BaseUri = endpoint,
                VerificationPublicKeyPem = deployment.VerificationPublicKeyPem,
                ProductId = string.IsNullOrWhiteSpace(deployment.ProductId) ? ProductId : deployment.ProductId.Trim()
            };
        }
        catch (Exception ex) { configurationError = ex.Message; }

        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        var client = new LicenseClient(options, http);
        return new DesktopController(client, http, GamePathSettings.Find(preferredGamePath), configurationError);
    }

    internal DesktopController(LicenseClient license, HttpClient http, string defaultGamePath,
        string configurationError = "")
    {
        _license = license;
        _http = http;
        DefaultGamePath = defaultGamePath;
        _configurationError = configurationError;
    }

    public string DefaultGamePath { get; }

    public LicenseView CurrentLicense
    {
        get
        {
            if (_configurationError.Length > 0) return Unavailable(_configurationError);
            if (!_initialized) return Unavailable("正在读取本机授权。");
            try { return ToView(_license.CheckCurrentLicense()); }
            catch (Exception ex) { return Unavailable(ex.Message); }
        }
    }

    public async Task<LicenseView> InitializeAsync()
    {
        await _operations.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_configurationError.Length > 0) return Unavailable(_configurationError);
            var result = await _license.ValidateStartupAsync().ConfigureAwait(false);
            _initialized = true;
            return ToView(result);
        }
        catch (Exception ex) { return Unavailable("授权检查未完成：" + ex.Message); }
        finally { _operations.Release(); }
    }

    public async Task<LicenseView> ActivateAsync(string code)
    {
        await _operations.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_configurationError.Length > 0) return Unavailable(_configurationError);
            var result = await _license.ActivateAsync(code.Trim()).ConfigureAwait(false);
            _initialized = true;
            return ToView(result);
        }
        catch (Exception ex) { return Unavailable("激活未完成：" + ex.Message); }
        finally { _operations.Release(); }
    }

    public async Task<ToolResult> ExecuteAsync(ToolAction action, string gamePath)
    {
        await _operations.WaitAsync().ConfigureAwait(false);
        try
        {
            // Check immediately before dispatch as well as in the UI. Closing and
            // restoration remain available even when startup validation failed.
            var license = CurrentLicense;
            if (action == ToolAction.Enable && !license.CanEnable)
                return new ToolResult(3, "开启未执行：" + license.Summary + Environment.NewLine + license.Detail);
            var result = await Task.Run(() => Program.RunDesktopAction(action, gamePath)).ConfigureAwait(false);
            if (result.ExitCode == 0) GamePathSettings.Save(gamePath);
            return result;
        }
        finally { _operations.Release(); }
    }

    private static LicenseView ToView(LicenseStatus status) => new(status.CanEnable,
        status.CanEnable ? status.Kind == LicenseKind.Permanent ? "永久授权 · 已激活" : "限时授权 · 可用"
            : status.Code == "Expired" ? "授权已到期" : "授权未就绪",
        status.Message, status.ExpiresAtUtc, status.DeviceId ?? "");

    private static LicenseView Unavailable(string message) => new(false, "授权未就绪", message, null, "");

    public void Dispose()
    {
        _http.Dispose();
        _operations.Dispose();
    }

    private sealed class PublicDeployment
    {
        public string ServiceUri { get; set; } = "";
        public string VerificationPublicKeyPem { get; set; } = "";
        public string ProductId { get; set; } = "";
    }
}

internal static class GamePathSettings
{
    private static string SettingsPath => Path.Combine(DesktopController.UserDataDirectory, "settings.json");

    internal static string Find(string preferred)
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                using var stored = JsonDocument.Parse(File.ReadAllText(SettingsPath));
                string? path = stored.RootElement.ValueKind == JsonValueKind.Object &&
                    stored.RootElement.TryGetProperty("gamePath", out var value) && value.ValueKind == JsonValueKind.String
                    ? value.GetString() : null;
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path) &&
                    string.Equals(Path.GetFileName(path), PatchCatalog.ExeName, StringComparison.OrdinalIgnoreCase))
                    return path;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or KeyNotFoundException) { }
        if (File.Exists(preferred)) return preferred;
        var libraries = new HashSet<string>(SteamLaunchOptions.FindSteamRoots(), StringComparer.OrdinalIgnoreCase);
        foreach (string root in libraries.ToArray())
        {
            string libraryFile = Path.Combine(root, "steamapps", "libraryfolders.vdf");
            try
            {
                if (!File.Exists(libraryFile)) continue;
                foreach (Match match in Regex.Matches(File.ReadAllText(libraryFile), "\"path\"\\s*\"([^\"]+)\""))
                    libraries.Add(match.Groups[1].Value.Replace("\\\\", "\\"));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
        foreach (string root in libraries)
        {
            string candidate = Path.Combine(root, "steamapps", "common", "Company of Heroes 3", PatchCatalog.ExeName);
            if (File.Exists(candidate)) return candidate;
        }
        return preferred;
    }

    internal static void Save(string gamePath)
    {
        try
        {
            Directory.CreateDirectory(DesktopController.UserDataDirectory);
            string temporary = SettingsPath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(new { gamePath = Path.GetFullPath(gamePath.Trim().Trim('"')) }));
            File.Move(temporary, SettingsPath, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException) { }
    }
}
