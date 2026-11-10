using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Licensing;
using Licensing.Server;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using ServerProgram = Licensing.Server.Program;

// No Keygen emulator. This executable requires the actual CE API and actual SQLite storage.
var configPath = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("Licensing/.local/server.json");
if (!File.Exists(configPath))
{
    Console.Error.WriteLine("NOT RUN: actual Keygen CE server configuration missing. Run Licensing/deployment/Initialize-Local.ps1 first.");
    return 2;
}
var original = ServerOptions.Load(configPath);
var root = Path.Combine(Path.GetDirectoryName(configPath)!, "test-runs", DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..6]);
Directory.CreateDirectory(root);
var options = original with { ListenUrl = "http://127.0.0.1:0", DatabasePath = Path.Combine(root, "redemptions.db") };
var app = ServerProgram.Build(options);
var passed = new List<string>();
void Assert(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    passed.Add(name); Console.WriteLine("PASS: " + name);
}
string Device(string label) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("integration-device-" + label)));
var productId = options.ResolveProduct(null).Id;
LicenseClientOptions ClientOptions(Uri url, string path) => new()
{
    BaseUri = url, VerificationPublicKeyPem = options.VerificationPublicKeyPem,
    LicenseFilePath = Path.Combine(root, path + ".lic"),
    ProductId = productId
};
try
{
    await app.StartAsync();
    var baseUri = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
    using var anonymous = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = baseUri };
    var health = await anonymous.GetFromJsonAsync<JsonElement>("health");
    Assert(health.GetProperty("mode").GetString() == "real-api", "actual Keygen CE policies verified through /health");
    Assert((await anonymous.GetAsync("api/admin/codes")).StatusCode == HttpStatusCode.Unauthorized, "admin API requires authentication");
    Assert((await anonymous.GetAsync("admin")).StatusCode == HttpStatusCode.Redirect, "admin UI requires authentication");

    using var admin = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false }) { BaseAddress = baseUri };
    var login = await admin.GetStringAsync("admin/login");
    var loginCsrf = Regex.Match(login, "name=__RequestVerificationToken value=\"([^\"]+)\"").Groups[1].Value;
    Assert((await admin.PostAsync("admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
    { ["token"] = options.AdminToken }))).StatusCode == HttpStatusCode.BadRequest, "login rejects missing CSRF token");
    var signIn = await admin.PostAsync("admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
    { ["token"] = options.AdminToken, ["__RequestVerificationToken"] = WebUtility.HtmlDecode(loginCsrf) }));
    Assert(signIn.StatusCode == HttpStatusCode.Redirect, "real administrator session established");
    var adminHtml = await admin.GetStringAsync("admin");
    var csrf = Regex.Match(adminHtml, "name=\"csrf-token\" content=\"([^\"]+)\"").Groups[1].Value;
    Assert((await admin.PostAsJsonAsync("api/admin/codes", new IssueRequest("permanent"))).StatusCode == HttpStatusCode.BadRequest,
        "admin issue endpoint rejects missing CSRF token");
    admin.DefaultRequestHeaders.Add("X-CSRF-TOKEN", WebUtility.HtmlDecode(csrf));
    async Task<IssuedCode> Issue(string kind, int? days = null)
    {
        using var response = await admin.PostAsJsonAsync("api/admin/codes", new IssueRequest(kind, days));
        if (!response.IsSuccessStatusCode) throw new Exception("Issue API failed: " + await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<IssuedCode>())!;
    }
    var permanent = await Issue("permanent");
    var deviceA = Device("A"); var deviceB = Device("B");
    using var httpA = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };
    using var httpB = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };
    using var clientA = new LicenseClient(ClientOptions(baseUri, "permanent-a"), httpA, () => deviceA);
    using var clientB = new LicenseClient(ClientOptions(baseUri, "permanent-b"), httpB, () => deviceB);
    var concurrent = await Task.WhenAll(clientA.ActivateAsync(permanent.Code), clientB.ActivateAsync(permanent.Code));
    Assert(concurrent.Count(s => s.CanEnable) == 1 && concurrent.Count(s => s.Code == "AlreadyRedeemed") == 1,
        "simultaneous different-device redemption has exactly one winner (actual CE + SQLite)");
    var winnerA = concurrent[0].CanEnable;
    var winner = winnerA ? clientA : clientB;
    var winnerDevice = winnerA ? deviceA : deviceB;
    var loserDevice = winnerA ? deviceB : deviceA;
    var winnerPath = ClientOptions(baseUri, winnerA ? "permanent-a" : "permanent-b").LicenseFilePath;
    var firstCertificate = await File.ReadAllTextAsync(winnerPath);
    var retry = await winner.ActivateAsync(permanent.Code);
    Assert(retry.CanEnable && retry.Kind == LicenseKind.Permanent && retry.ExpiresAtUtc is null
        && await File.ReadAllTextAsync(winnerPath) == firstCertificate,
        "same-device retry recovers identical committed permanent certificate");
    var nativePermanent = MachineFile.Verify(firstCertificate, options.VerificationPublicKeyPem, winnerDevice, productId);
    Assert(nativePermanent.Kind == LicenseKind.Permanent, "native Keygen signature contains explicit product and permanent kind, ttl=null");
    using var wrongProductHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };
    using var wrongProduct = new LicenseClient(
        ClientOptions(baseUri, "wrong-product") with { ProductId = "OtherProduct" }, wrongProductHttp, () => loserDevice);
    Assert((await wrongProduct.ActivateAsync(permanent.Code)).Code == "UnknownProduct",
        "authorization code cannot be redeemed under a different product id");

    var copyOptions = ClientOptions(baseUri, "copied");
    await File.WriteAllTextAsync(copyOptions.LicenseFilePath, firstCertificate);
    using var copied = new LicenseClient(copyOptions, httpA, () => loserDevice);
    Assert((await copied.ValidateStartupAsync()).Code == "DeviceMismatch", "copied permanent certificate fails on another device fingerprint");
    var tampered = firstCertificate.Replace(MachineFile.Begin, "").Replace(MachineFile.End, "").Trim();
    using (var json = JsonDocument.Parse(Convert.FromBase64String(tampered)))
    {
        var signature = Convert.FromBase64String(json.RootElement.GetProperty("sig").GetString()!); signature[0] ^= 1;
        var body = JsonSerializer.Serialize(new { enc = json.RootElement.GetProperty("enc").GetString(), sig = Convert.ToBase64String(signature), alg = "base64+rsa-sha256" });
        await File.WriteAllTextAsync(copyOptions.LicenseFilePath,
            MachineFile.Begin + "\n" + Convert.ToBase64String(Encoding.UTF8.GetBytes(body)) + "\n" + MachineFile.End);
    }
    using var tamperedClient = new LicenseClient(copyOptions, httpA, () => winnerDevice);
    Assert((await tamperedClient.ValidateStartupAsync()).Code == "InvalidLicense", "tampered signature fails local verification");
    using var wrongKey = RSA.Create(2048);
    using var wrongTrust = new LicenseClient(ClientOptions(baseUri, winnerA ? "permanent-a" : "permanent-b")
        with { VerificationPublicKeyPem = wrongKey.ExportSubjectPublicKeyInfoPem() }, httpA, () => winnerDevice);
    Assert((await wrongTrust.ValidateStartupAsync()).Code == "InvalidLicense", "untrusted account public key fails local verification");

    var timed = await Issue("timed", 2);
    var timedOptions = ClientOptions(baseUri, "timed");
    var clock = new ManualTimeProvider();
    using var counted = new CountingHandler();
    using var timedHttp = new HttpClient(counted) { Timeout = TimeSpan.FromSeconds(100) };
    using var timedClient = new LicenseClient(timedOptions, timedHttp, () => deviceA, clock);
    var activated = await timedClient.ActivateAsync(timed.Code);
    Assert(activated.CanEnable && activated.Kind == LicenseKind.Timed && activated.ExpiresAtUtc is not null,
        "timed code activates with signed explicit kind and server expiry");
    var timedCertificate = await File.ReadAllTextAsync(timedOptions.LicenseFilePath);
    var nativeTimed = MachineFile.Verify(timedCertificate, options.VerificationPublicKeyPem, deviceA, productId);
    Assert(nativeTimed.ExpiresAtUtc - nativeTimed.ActivatedAtUtc == TimeSpan.FromDays(2),
        "actual Keygen license expiry starts at first machine activation, not code issue time");
    var callsBeforeCheck = counted.Count;
    clock.Advance(TimeSpan.FromDays(3));
    Assert(timedClient.CheckCurrentLicense().Code == "Expired" && counted.Count == callsBeforeCheck,
        "running timed expiry uses confirmed server time plus monotonic elapsed without network refresh");
    using var startupCount = new CountingHandler();
    using var startupHttp = new HttpClient(startupCount);
    using var restartedTimed = new LicenseClient(timedOptions, startupHttp, () => deviceA);
    Assert((await restartedTimed.ValidateStartupAsync()).CanEnable && startupCount.Count == 1,
        "new timed client startup performs exactly one online verification");
    var suspend = await admin.PostAsJsonAsync($"api/admin/codes/{timed.Id}/suspension", new { suspended = true });
    Assert(suspend.IsSuccessStatusCode, "administrator suspends actual Keygen timed license");
    Assert(restartedTimed.CheckCurrentLicense().CanEnable, "existing running confirmation stays valid until expiry without unsolicited polling");
    Assert((await restartedTimed.ValidateStartupAsync()).Code == "Suspended" && !restartedTimed.CheckCurrentLicense().CanEnable,
        "next startup sees suspension and clears previous enabled state");
    var restore = await admin.PostAsJsonAsync($"api/admin/codes/{timed.Id}/suspension", new { suspended = false });
    Assert(restore.IsSuccessStatusCode && (await restartedTimed.ValidateStartupAsync()).CanEnable,
        "administrator reinstatement restores next startup verification");
    Assert(!(await admin.PostAsJsonAsync($"api/admin/codes/{permanent.Id}/suspension", new { suspended = true })).IsSuccessStatusCode,
        "permanent remote revocation is explicitly excluded");
    var lostResponseCode = await Issue("permanent");
    using var droppingHandler = new DropFirstSuccessfulResponseHandler();
    using var droppingHttp = new HttpClient(droppingHandler);
    var lostOptions = ClientOptions(baseUri, "lost-response");
    using var recoveringClient = new LicenseClient(lostOptions, droppingHttp, () => deviceA);
    Assert((await recoveringClient.ActivateAsync(lostResponseCode.Code)).Code == "NetworkUnavailable"
        && droppingHandler.CommittedCertificate is not null && !File.Exists(lostOptions.LicenseFilePath),
        "transport fault after actual backend commit leaves client with no saved license");
    Assert((await recoveringClient.ActivateAsync(lostResponseCode.Code)).CanEnable
        && await File.ReadAllTextAsync(lostOptions.LicenseFilePath) == droppingHandler.CommittedCertificate,
        "same device recovers exact committed result after deliberately dropped real HTTP response");

    // Reproduce process death after native machine creation and before the local redemption transaction commits.
    // This seeds only actual CE state through its real API; the adapter ledger is still unbound.
    var partial = await Issue("timed", 1);
    var partialRecord = (await admin.GetFromJsonAsync<CodeSummary[]>("api/admin/codes"))!.Single(r => r.Id == partial.Id);
    using var nativeHttp = new HttpClient();
    var gateway = new KeygenGateway(nativeHttp, options);
    var durableMachine = await gateway.CreateMachineAsync(partialRecord.LicenseId, deviceA, productId, default);
    using var partialOther = new LicenseClient(ClientOptions(baseUri, "partial-other"), httpB, () => deviceB);
    Assert((await partialOther.ActivateAsync(partial.Code)).Code == "AlreadyRedeemed",
        "native binding blocks another device even before local redemption commit");
    var partialOptions = ClientOptions(baseUri, "partial-recovered");
    using var partialSame = new LicenseClient(partialOptions, httpA, () => deviceA);
    Assert((await partialSame.ActivateAsync(partial.Code)).CanEnable,
        "same device completes activation from durable native machine after pre-commit interruption");
    var recoveredNative = MachineFile.Verify(await File.ReadAllTextAsync(partialOptions.LicenseFilePath), options.VerificationPublicKeyPem, deviceA, productId);
    Assert(recoveredNative.ActivatedAtUtc == durableMachine.CreatedAt
        && recoveredNative.ExpiresAtUtc == durableMachine.CreatedAt.AddDays(1),
        "interrupted activation recovery retains original first activation time and expiry");

    Assert((await anonymous.PostAsJsonAsync("api/licenses/activate", new { code = permanent.Code })).StatusCode == HttpStatusCode.BadRequest,
        "malformed activation missing device returns an explanatory client error");
    var records = (await admin.GetFromJsonAsync<CodeSummary[]>("api/admin/codes"))!;
    Assert(records.Length == 4 && records.All(r => r.DeviceId is not null && r.ActivatedAtUtc is not null),
        "administrator views durable redemption, device and expiry records");

    await app.StopAsync(); await app.DisposeAsync();
    // The backend is now genuinely stopped. No fake HTTP response or fake backend state is used.
    using var offlineCount = new CountingHandler();
    using var offlineHttp = new HttpClient(offlineCount) { Timeout = TimeSpan.FromSeconds(2) };
    using var offlinePermanent = new LicenseClient(ClientOptions(baseUri, winnerA ? "permanent-a" : "permanent-b"), offlineHttp, () => winnerDevice);
    Assert((await offlinePermanent.ValidateStartupAsync()).CanEnable && offlineCount.Count == 0,
        "permanent restart succeeds with backend stopped and zero HTTP calls");
    using var offlineTimed = new LicenseClient(timedOptions, offlineHttp, () => deviceA);
    Assert((await offlineTimed.ValidateStartupAsync()).Code == "NetworkUnavailable" && !offlineTimed.CheckCurrentLicense().CanEnable
        && offlineCount.Count == 1, "timed restart with backend stopped blocks enable and reports network failure");

    app = ServerProgram.Build(options);
    await app.StartAsync();
    var newUri = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single());
    using var afterRestart = new LicenseClient(ClientOptions(newUri, "recovered"), httpA, () => winnerDevice);
    Assert((await afterRestart.ActivateAsync(permanent.Code)).CanEnable
        && await File.ReadAllTextAsync(ClientOptions(newUri, "recovered").LicenseFilePath) == firstCertificate,
        "adapter restart recovers original result from SQLite without consuming code again");
    using var stillOther = new LicenseClient(ClientOptions(newUri, "still-other"), httpB, () => loserDevice);
    Assert((await stillOther.ActivateAsync(permanent.Code)).Code == "AlreadyRedeemed", "device binding survives adapter process restart");
    var report = new { mode = "actual-keygen-ce-http-and-sqlite", passed = passed.Count, checks = passed,
        completedAtUtc = DateTimeOffset.UtcNow, testDirectory = root,
        pending = new[] { "physical second PC", "public deployment/TLS", "GUI/game runtime" } };
    await File.WriteAllTextAsync(Path.Combine(root, "results.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"RESULT: {passed.Count} passed; actual Keygen CE, actual loopback HTTP, persistent SQLite; report {root}");
    return 0;
}
finally { await app.StopAsync(); await app.DisposeAsync(); }

sealed class CountingHandler : DelegatingHandler
{
    private int count;
    public int Count => Volatile.Read(ref count);
    public CountingHandler() : base(new HttpClientHandler()) { }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    { Interlocked.Increment(ref count); return base.SendAsync(request, cancellationToken); }
}
sealed class DropFirstSuccessfulResponseHandler : DelegatingHandler
{
    public string? CommittedCertificate { get; private set; }
    public DropFirstSuccessfulResponseHandler() : base(new HttpClientHandler()) { }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode && CommittedCertificate is null)
        {
            var body = await response.Content.ReadFromJsonAsync<LicenseApiResponse>(cancellationToken: cancellationToken);
            CommittedCertificate = body?.Certificate ?? throw new Exception("Actual activation response contained no certificate.");
            response.Dispose();
            throw new HttpRequestException("Injected transport loss after actual backend commit; no fake server state.");
        }
        return response;
    }
}
sealed class ManualTimeProvider : TimeProvider
{
    private long ticks;
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => Volatile.Read(ref ticks);
    public void Advance(TimeSpan duration) => Interlocked.Add(ref ticks, duration.Ticks);
}
