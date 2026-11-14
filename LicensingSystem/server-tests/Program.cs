using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Licensing.Server;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using ServerProgram = Licensing.Server.Program;

// Real ASP.NET Core HTTP + persistent SQLite. Only the public TimeProvider seam is controlled.
// No external configuration, legacy client, Keygen service, or synthetic HTTP responses are required.
var parent = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(AppContext.BaseDirectory, "test-runs");
var root = Path.Combine(parent, DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..8]);
Directory.CreateDirectory(root);
var clock = new ManualTimeProvider(DateTimeOffset.UtcNow);
var options = new ServerOptions
{
    ListenUrl = "http://127.0.0.1:0", DatabasePath = Path.Combine(root, "native.db"),
    KeyDirectory = Path.Combine(root, "keys"), AdminToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32))
};
var app = ServerProgram.Build(options, ["--Logging:LogLevel:Default", "Warning"], clock);
var passed = new List<string>();
var adminCookies = new CookieContainer();
// Connect assigns both clients immediately after every successful host startup.
HttpClient anonymous = null!;
HttpClient admin = null!;
void Assert(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException("FAIL: " + name);
    passed.Add(name);
    Console.WriteLine("PASS: " + name);
}
void Connect()
{
    anonymous?.Dispose(); admin?.Dispose();
    var address = new Uri(app.Urls.Single());
    anonymous = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = address };
    admin = new HttpClient(new HttpClientHandler { CookieContainer = adminCookies, AllowAutoRedirect = false }) { BaseAddress = address };
}
async Task<JsonElement> Post(string route, object body, string? token = null, bool authenticated = false, HttpStatusCode expected = HttpStatusCode.OK)
{
    using var request = new HttpRequestMessage(HttpMethod.Post, route) { Content = JsonContent.Create(body) };
    if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    using var response = await (authenticated ? admin! : anonymous!).SendAsync(request);
    if (response.StatusCode != expected) throw new InvalidOperationException($"HTTP {(int)response.StatusCode} instead of {(int)expected}: {route}");
    return await response.Content.ReadFromJsonAsync<JsonElement>();
}
async Task<JsonElement> Get(string route) => await admin!.GetFromJsonAsync<JsonElement>(route);
async Task<JsonElement> Issue(string kind, long? duration = null, string note = "")
{
    var response = await Post("/api/admin/codes", new { kind, durationSeconds = duration, quantity = 1, note }, authenticated: true);
    return response.GetProperty("items")[0];
}
async Task<JsonElement> Card(string id)
{
    var response = await Get("/api/admin/codes?search=" + Uri.EscapeDataString(id));
    return response.GetProperty("items").EnumerateArray().Single(item => item.GetProperty("id").GetString() == id);
}
static string Text(JsonElement value, string property) => value.GetProperty(property).GetString()!;
static long Number(JsonElement value, string property) => value.GetProperty(property).GetInt64();
static string State(JsonElement value) => Text(value, "state");
static string NewNonce() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
static ActivationRequest Activation(string code, string request, char device = 'a') => new()
{
    Code = code, RequestId = request, DeviceId = new string(device, 64), SteamId64 = "76561198000000001",
    InstallationId = "d08cff4e-9ed0-4e95-a124-cc4f60f4174f", Nonce = NewNonce(), ClientVersion = "0.60.8", GameName = "integration-game"
};
static HeartbeatRequest Heartbeat(ActivationRequest request) => new()
{
    DeviceId = request.DeviceId, SteamId64 = request.SteamId64, InstallationId = request.InstallationId, ClientVersion = request.ClientVersion, Nonce = NewNonce()
};
bool Verify(JsonElement response, ActivationRequest context, string? bearer = null, LeasePublicKey? pinned = null)
{
    var pub = pinned ?? app.Services.GetRequiredService<LeaseSigner>().ExportPublicKey();
    if (!response.TryGetProperty("lease_kid", out var kid) || kid.GetString() != pub.Kid
        || !response.TryGetProperty("lease_signature", out var signature) || State(response) != "active") return false;
    var effective = response.TryGetProperty("session_token", out var tokenValue) ? tokenValue.GetString()! : bearer!;
    var canonical = string.Join("\n", new[] {
        "skyeye-lease-v2", options.ProductId, "active", context.DeviceId!.ToUpperInvariant(), context.SteamId64!,
        Guid.Parse(context.InstallationId!).ToString("D").ToLowerInvariant(), context.Nonce!,
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(effective))).ToLowerInvariant(),
        Number(response, "server_time").ToString(System.Globalization.CultureInfo.InvariantCulture),
        Number(response, "lease_until").ToString(System.Globalization.CultureInfo.InvariantCulture),
        Number(response, "expires_at").ToString(System.Globalization.CultureInfo.InvariantCulture),
        Text(response, "license_kind"), pub.Kid });
    using var rsa = RSA.Create();
    rsa.ImportParameters(new RSAParameters { Modulus = Convert.FromBase64String(pub.Modulus), Exponent = Convert.FromBase64String(pub.Exponent) });
    return rsa.VerifyData(Encoding.UTF8.GetBytes(canonical), Convert.FromBase64String(signature.GetString()!), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
}
static JsonElement Change(JsonElement source, string property, object value)
{
    var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(source.GetRawText())!;
    fields[property] = JsonSerializer.SerializeToElement(value);
    return JsonSerializer.SerializeToElement(fields);
}
async Task RefreshCsrf()
{
    var html = await admin!.GetStringAsync("/admin");
    var token = Regex.Match(html, "name=\"csrf-token\" content=\"([^\"]+)\"").Groups[1].Value;
    if (token.Length == 0) throw new InvalidOperationException("Rendered admin CSRF token missing.");
    admin.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
    admin.DefaultRequestHeaders.Add("X-CSRF-TOKEN", WebUtility.HtmlDecode(token));
}

try
{
    var exportConfig = Path.Combine(root, "isolated-export-config.json");
    var publicExportPath = Path.Combine(root, "lease-public.json");
    await File.WriteAllTextAsync(exportConfig, JsonSerializer.Serialize(options));
    var priorConfig = Environment.GetEnvironmentVariable("LICENSING_SERVER_CONFIG");
    try
    {
        Environment.SetEnvironmentVariable("LICENSING_SERVER_CONFIG", exportConfig);
        await ServerProgram.Main(["--export-lease-public-key", publicExportPath]);
        var exportedPublic = JsonSerializer.Deserialize<LeasePublicKey>(await File.ReadAllTextAsync(publicExportPath));
        Assert(exportedPublic == app.Services.GetRequiredService<LeaseSigner>().ExportPublicKey(), "offline CLI exports configured signing public key without starting server");
        var originalExport = await File.ReadAllBytesAsync(publicExportPath);
        var overwriteRejected = false;
        try { await ServerProgram.Main(["--export-lease-public-key", publicExportPath]); }
        catch (IOException) { overwriteRejected = true; }
        var retainedExport = await File.ReadAllBytesAsync(publicExportPath);
        Assert(overwriteRejected && originalExport.SequenceEqual(retainedExport), "offline CLI preserves existing export destination");
    }
    finally { Environment.SetEnvironmentVariable("LICENSING_SERVER_CONFIG", priorConfig); }
    var concurrentOptions = options with { KeyDirectory = Path.Combine(root, "concurrent-signing-keys") };
    var concurrentKeys = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
    {
        using var signer = new LeaseSigner(concurrentOptions);
        return signer.ExportPublicKey();
    })));
    Assert(concurrentKeys.All(k => k == concurrentKeys[0]), "concurrent key initialization publishes one persistent RSA key");
    Assert(!Directory.EnumerateFiles(concurrentOptions.KeyDirectory, "*.tmp").Any(), "key initialization removes temporary private key files");
    var brokenKeyDirectory = Path.Combine(root, "broken-signing-keys");
    Directory.CreateDirectory(brokenKeyDirectory);
    var brokenKeyPath = Path.Combine(brokenKeyDirectory, LeaseSigner.KeyFileName);
    await File.WriteAllTextAsync(brokenKeyPath, "invalid-key-fixture");
    var invalidKeyRejected = false;
    try { using var signer = new LeaseSigner(options with { KeyDirectory = brokenKeyDirectory }); }
    catch (CryptographicException) { invalidKeyRejected = true; }
    Assert(invalidKeyRejected && await File.ReadAllTextAsync(brokenKeyPath) == "invalid-key-fixture", "invalid persisted key fails closed without replacement");
    await app.StartAsync();
    Connect();
    var health = await anonymous!.GetFromJsonAsync<JsonElement>("/health");
    Assert(Text(health, "mode") == "native-leases", "health verifies native storage");
    var version = await anonymous!.GetFromJsonAsync<JsonElement>("/api/v1/version");
    Assert(Text(version, "minimum") == "0.60.8" && Text(version, "latest") == "0.60.8"
        && Text(version, "product_id") == "CoH3FowTool", "version uses fixed snake_case contract");
    Assert(Number(version, "connection_info") == 0 && Number(version, "steam_accounts_info") == 0
        && Number(version, "qq_accounts_info") == 0, "version keeps all collection flags disabled");
    foreach (var route in new[] { "settings", "codes", "codes/export", "sessions", "audit" })
        Assert((await anonymous.GetAsync("/api/admin/" + route)).StatusCode == HttpStatusCode.Unauthorized, route + " requires administrator cookie");
    Assert((await anonymous.GetAsync("/admin")).StatusCode == HttpStatusCode.Redirect, "admin page requires login");
    using (var badHost = new HttpRequestMessage(HttpMethod.Get, "/api/v1/version"))
    {
        badHost.Headers.Host = "unlisted.example";
        Assert((await anonymous.SendAsync(badHost)).StatusCode == HttpStatusCode.BadRequest, "unlisted Host header rejected");
    }
    using (var forwarded = new HttpRequestMessage(HttpMethod.Get, "/api/v1/version"))
    {
        forwarded.Headers.Add("X-Forwarded-For", "198.51.100.1"); forwarded.Headers.Add("X-Forwarded-Proto", "http");
        Assert((await anonymous.SendAsync(forwarded)).StatusCode == HttpStatusCode.OK, "unconfigured forwarded headers are ignored");
    }
    var loginHtml = await admin!.GetStringAsync("/admin/login");
    var loginCsrf = WebUtility.HtmlDecode(Regex.Match(loginHtml, "name=__RequestVerificationToken value=\"([^\"]+)\"").Groups[1].Value);
    var noCsrf = await admin.PostAsync("/admin/login", new FormUrlEncodedContent(new Dictionary<string, string> { ["token"] = options.AdminToken }));
    Assert(noCsrf.StatusCode == HttpStatusCode.BadRequest, "login rejects absent CSRF");
    var wrongLogin = await admin.PostAsync("/admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["token"] = "wrong", ["__RequestVerificationToken"] = loginCsrf }));
    Assert(wrongLogin.StatusCode == HttpStatusCode.Unauthorized, "wrong administrator credential rejected");
    var login = await admin.PostAsync("/admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
        { ["token"] = options.AdminToken, ["__RequestVerificationToken"] = loginCsrf }));
    Assert(login.StatusCode == HttpStatusCode.Redirect, "administrator form login succeeds");
    var missingCsrf = await Post("/api/admin/codes", new { kind = "permanent" }, authenticated: true, expected: HttpStatusCode.BadRequest);
    Assert(Text(missingCsrf, "error") == "invalid_csrf", "management write requires CSRF even with cookie");
    await RefreshCsrf();
    Assert((await admin.GetAsync("/admin/app.js")).IsSuccessStatusCode && (await admin.GetAsync("/admin/app.css")).IsSuccessStatusCode,
        "embedded management resources served after login");
    var settings = await Get("/api/admin/settings");
    Assert(Text(settings, "productId") == "CoH3FowTool" && Number(settings, "leaseSeconds") == 60 && Number(settings, "heartbeatSeconds") == 28
        && !settings.TryGetProperty("adminToken", out _) && !settings.TryGetProperty("keyDirectory", out _), "settings expose protocol metadata without credentials");
    var invalidIssue = await Post("/api/admin/codes", new { kind = "timed", durationSeconds = 0, quantity = 1 }, authenticated: true, expected: HttpStatusCode.BadRequest);
    Assert(invalidIssue.TryGetProperty("error", out _) && invalidIssue.TryGetProperty("message", out _), "invalid issue has management error object");
    var excessive = await Post("/api/admin/codes", new { kind = "permanent", quantity = 501 }, authenticated: true, expected: HttpStatusCode.BadRequest);
    Assert(Text(excessive, "error") == "invalid_issue", "issue quantity bounded at 500");

    const string formulaNote = "=SUM(1,1),\"quoted\"\nline";
    var batch = await Post("/api/admin/codes", new { kind = "timed", durationSeconds = 120, quantity = 2, note = formulaNote }, authenticated: true);
    var card = batch.GetProperty("items")[0];
    var cardId = Text(card, "id"); var code = Text(card, "code");
    Assert(Number(batch, "count") == 2 && batch.GetProperty("items").GetArrayLength() == 2
        && code != Text(batch.GetProperty("items")[1], "code"), "batch issue returns unique complete codes");
    Assert(State(card) == "unused" && card.GetProperty("activatedAtUnix").ValueKind == JsonValueKind.Null
        && card.GetProperty("expiresAtUnix").ValueKind == JsonValueKind.Null, "unredeemed timed code has nullable activation and expiry");
    var invalidInput = Activation(code, "first");
    foreach (var nonce in new string?[] { null, "", new string('a', 31), new string('a', 33), new string('A', 32), new string('g', 32), new string('0', 31) + "\n" })
    {
        var invalid = await Post("/api/v1/activate", invalidInput with { Nonce = nonce }, expected: HttpStatusCode.BadRequest);
        Assert(Text(invalid, "error") == "invalid_nonce", "invalid activation nonce rejected: " + (nonce is null ? "null" : nonce.Length + "/" + (nonce.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f') ? "hex" : "invalid")));
    }
    Assert(State(await Card(cardId)) == "unused" && (await Get($"/api/admin/sessions?codeId={cardId}&activeOnly=false")).GetProperty("items").GetArrayLength() == 0,
        "invalid nonce leaves activation and sessions untouched");
    clock.Advance(30);
    var input = Activation(code, "first");
    var first = await Post("/api/v1/activate", input);
    var firstTime = Number(first, "server_time"); var firstExpiry = Number(first, "expires_at"); var token = Text(first, "session_token");
    Assert(State(first) == "active" && firstTime == Number(card, "createdAtUnix") + 30 && firstExpiry == firstTime + 120,
        "timed validity starts at first successful activation");
    Assert(Number(first, "lease_until") == firstTime + 60 && Text(first, "license_kind") == "timed"
        && token.Length == 64 && token.All(char.IsAsciiHexDigit), "activation emits active short lease and 64 hexadecimal token");
    Assert(first.TryGetProperty("lease_kid", out _) && first.TryGetProperty("lease_signature", out _), "active activation includes mandatory signature");
    Assert(Verify(first, input), "activation signature verifies independently against contract canonical form");
    Assert(Verify(first, input with { DeviceId = input.DeviceId!.ToUpperInvariant(), InstallationId = input.InstallationId!.ToUpperInvariant() }), "identity case normalization agrees with signature");
    var publicKey = app.Services.GetRequiredService<LeaseSigner>().ExportPublicKey();
    var exportedFields = JsonSerializer.SerializeToElement(publicKey);
    Assert(exportedFields.EnumerateObject().Select(p => p.Name).Order().SequenceEqual(new[] { "exponent", "kid", "modulus" })
        && Convert.FromBase64String(publicKey.Modulus).Length == 384 && publicKey.Kid == Convert.ToHexString(SHA256.HashData(Convert.FromBase64String(publicKey.Modulus))).ToLowerInvariant()[..16],
        "offline public export contains only modulus exponent and derived kid for RSA3072");
    Assert(!Verify(first, input with { Nonce = NewNonce() }), "captured activation signature fails across fresh nonce");
    Assert(!Verify(first, input with { DeviceId = new string('b', 64) }), "signature binds request device");
    Assert(!Verify(first, input with { SteamId64 = "76561198000000002" }), "signature binds request Steam identity");
    Assert(!Verify(first, input with { InstallationId = Guid.NewGuid().ToString("D") }), "signature binds request installation");
    foreach (var field in new[] { "server_time", "lease_until", "expires_at" })
        Assert(!Verify(Change(first, field, Number(first, field) + 1), input), "signature rejects altered " + field);
    Assert(!Verify(Change(first, "license_kind", "permanent"), input), "signature rejects altered license kind");
    Assert(!Verify(Change(first, "session_token", new string('0', 64)), input), "signature rejects altered token");
    Assert(!Verify(Change(first, "lease_kid", "0000000000000000"), input), "signature rejects wrong kid");
    using (var wrongKey = RSA.Create(3072))
    {
        var parameters = wrongKey.ExportParameters(false);
        Assert(!Verify(first, input, pinned: publicKey with { Modulus = Convert.ToBase64String(parameters.Modulus!) }), "signature rejects wrong pinned public key");
    }
    Assert(!Verify(Change(first, "state", "revoked"), input), "signature grants no altered non-active state");
    clock.Advance(10);
    var retryInput = input with { Nonce = NewNonce() };
    var retry = await Post("/api/v1/activate", retryInput);
    Assert(State(retry) == "active" && Text(retry, "session_token") == token && Number(retry, "server_time") == firstTime + 10
        && Number(retry, "lease_until") == Number(first, "lease_until"), "idempotent retry updates server time without extending lease");
    Assert(State(await Post("/api/v1/activate", input with { GameName = "other-game" })) == "request_conflict",
        "same request id with different payload conflicts");
    Assert(State(await Post("/api/v1/activate", input with { DeviceId = new string('b', 64) })) == "request_conflict",
        "same request id with different identity conflicts");
    Assert(State(await Post("/api/v1/activate", input with { RequestId = "new-login" })) == "in_use",
        "new request cannot replace a live session");
    Assert(Verify(retry, retryInput) && !Verify(first, retryInput) && Text(retry, "lease_signature") != Text(first, "lease_signature"),
        "stable activation request id retries are freshly signed for independent nonce");
    var beforeBadHeartbeat = (await Get($"/api/admin/sessions?codeId={cardId}")).GetProperty("items")[0];
    foreach (var nonce in new string?[] { null, "", new string('A', 32), new string('0', 31) })
    {
        var invalid = await Post("/api/v1/heartbeat", Heartbeat(input) with { Nonce = nonce }, token, expected: HttpStatusCode.BadRequest);
        Assert(Text(invalid, "error") == "invalid_nonce", "invalid heartbeat nonce rejected: " + (nonce is null ? "null" : nonce.Length.ToString()));
    }
    var afterBadHeartbeat = (await Get($"/api/admin/sessions?codeId={cardId}")).GetProperty("items")[0];
    Assert(beforeBadHeartbeat.GetRawText() == afterBadHeartbeat.GetRawText(), "invalid nonce leaves heartbeat lease and session untouched");
    var beatRequest = Heartbeat(input);
    var beat = await Post("/api/v1/heartbeat", beatRequest, token);
    var beatContext = input with { Nonce = beatRequest.Nonce };
    Assert(Verify(beat, beatContext, token) && !Verify(beat, beatContext with { Nonce = NewNonce() }, token), "heartbeat signature verifies and rejects replay nonce");
    var noToken = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(beat.GetRawText())!;
    noToken.Remove("session_token");
    Assert(Verify(JsonSerializer.SerializeToElement(noToken), beatContext, token)
        && !Verify(JsonSerializer.SerializeToElement(noToken), beatContext, new string('0', 64)), "heartbeat canonical token falls back to actual bearer when response omits token");
    Assert(State(beat) == "active" && Number(beat, "lease_until") == firstTime + 70 && Number(beat, "expires_at") == firstExpiry,
        "heartbeat renews lease without resetting card expiry");
    var bound = await Card(cardId);
    Assert(Text(bound, "boundDeviceId") == new string('A', 64) && Number(bound, "activeSessionCount") == 1,
        "lowercase device fingerprint normalizes and binds once");
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(input) with { SteamId64 = "76561198000000002" }, token)) == "identity_changed",
        "heartbeat identity change revokes its session");
    var relog = await Post("/api/v1/activate", input with { RequestId = "relog" });
    var relogToken = Text(relog, "session_token");
    Assert(State(relog) == "active" && Number(relog, "expires_at") == firstExpiry, "new session retains first activation expiry");
    Assert((await Post("/api/v1/logout", new { }, relogToken)).GetProperty("ok").GetBoolean(), "logout returns JSON acknowledgement");
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(input), relogToken)) == "revoked", "logout immediately invalidates bearer token");
    Assert(State(await Post("/api/v1/activate", input with { RequestId = "relog" })) == "revoked", "logged out request id cannot revive its session");
    Assert(State(await Post("/api/v1/activate", input with { RequestId = "wrong-device", DeviceId = new string('b', 64) })) == "identity_changed",
        "released session preserves device binding");
    var next = await Post("/api/v1/activate", input with { RequestId = "before-renew" });
    await Post($"/api/admin/codes/{cardId}/renew", new { durationSeconds = 120 }, authenticated: true);
    var renewed = await Card(cardId);
    Assert(Number(renewed, "expiresAtUnix") == firstExpiry + 120 && Text(renewed, "boundDeviceId") == new string('A', 64),
        "renew extends expiry and preserves binding");
    await Post($"/api/admin/codes/{cardId}/unbind", new { }, authenticated: true);
    var unbound = await Card(cardId);
    Assert(unbound.GetProperty("boundDeviceId").ValueKind == JsonValueKind.Null && Number(unbound, "activatedAtUnix") == firstTime
        && Number(unbound, "expiresAtUnix") == firstExpiry + 120, "unbind preserves activation and expiry clocks");
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(input), Text(next, "session_token"))) == "revoked", "unbind revokes current session");
    var reboundInput = input with { RequestId = "rebound", DeviceId = new string('B', 64) };
    var rebound = await Post("/api/v1/activate", reboundInput);
    Assert(State(rebound) == "active" && Number(rebound, "expires_at") == firstExpiry + 120, "new device binds after unbind without a fresh duration");
    await Post($"/api/admin/codes/{cardId}/kick", new { }, authenticated: true);
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(reboundInput), Text(rebound, "session_token"))) == "revoked", "administrator kick immediately revokes session");
    var history = await Get($"/api/admin/sessions?codeId={cardId}&activeOnly=false");
    Assert(history.GetProperty("items").GetArrayLength() >= 3 && history.GetProperty("items").EnumerateArray().All(row =>
        !row.TryGetProperty("token", out _) && !row.TryGetProperty("sessionToken", out _) && !row.TryGetProperty("tokenHash", out _)),
        "session history exposes no token or token hash");
    Assert((await Get($"/api/admin/sessions?codeId={cardId}")).GetProperty("items").GetArrayLength() == 0, "active sessions exclude revoked history");

    var permanent = await Issue("permanent"); var permanentId = Text(permanent, "id");
    var permanentInput = Activation(Text(permanent, "code"), "permanent");
    var perm = await Post("/api/v1/activate", permanentInput);
    Assert(Text(perm, "license_kind") == "permanent" && Number(perm, "expires_at") == 253370764800
        && Number(perm, "lease_until") - Number(perm, "server_time") == 60, "permanent code still receives only a 60-second lease");
    Assert((await Card(permanentId)).GetProperty("expiresAtUnix").ValueKind == JsonValueKind.Null, "permanent management expiry remains null");
    await Post($"/api/admin/codes/{permanentId}/suspension", new { suspended = true }, authenticated: true);
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(permanentInput), Text(perm, "session_token"))) == "blocked", "permanent code can also be suspended");
    await Post($"/api/admin/codes/{permanentId}/suspension", new { suspended = false }, authenticated: true);
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(permanentInput), Text(perm, "session_token"))) != "active", "restoring code does not revive revoked session");
    var permanentRetry = permanentInput with { RequestId = "permanent-restored" };
    var restored = await Post("/api/v1/activate", permanentRetry);
    Assert(State(restored) == "active", "restored code permits a fresh login");
    clock.Advance(61);
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(permanentRetry), Text(restored, "session_token"))) == "revoked", "expired permanent lease cannot heartbeat back to life");
    Assert(State(await Post("/api/v1/activate", permanentRetry)) == "revoked", "expired request id cannot mint a new lease");
    clock.Advance(-2);
    Assert(State(await Post("/api/v1/activate", permanentRetry)) == "revoked", "observed expired request stays revoked across clock rollback");
    clock.Advance(2);
    Assert(State(await Post("/api/v1/activate", permanentInput with { RequestId = "permanent-fresh" })) == "active", "new request works after abnormal-exit lease timeout");

    var shortCard = await Issue("timed", 2);
    var shortInput = Activation(Text(shortCard, "code"), "short");
    var shortLease = await Post("/api/v1/activate", shortInput);
    Assert(Number(shortLease, "lease_until") == Number(shortLease, "expires_at")
        && Number(shortLease, "lease_until") - Number(shortLease, "server_time") == 2, "lease never exceeds short card expiry");
    clock.Advance(2);
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(shortInput), Text(shortLease, "session_token"))) == "expired", "timed lease stops at card expiry");
    Assert(State(await Post("/api/v1/activate", shortInput with { RequestId = "expired-new" })) == "expired", "new request cannot restart an expired timed card");
    var unused = await Issue("timed", 10);
    await Post($"/api/admin/codes/{Text(unused, "id")}/renew", new { durationSeconds = 5 }, authenticated: true);
    var unusedLease = await Post("/api/v1/activate", Activation(Text(unused, "code"), "unused-renew"));
    Assert(Number(unusedLease, "expires_at") - Number(unusedLease, "server_time") == 15, "renewing unused card increases duration before first activation");
    Assert(State(await Post("/api/v1/activate", Activation("INVALID", "invalid"))) == "revoked", "invalid code is an ordinary revoked state");
    Assert(State(await Post("/api/v1/activate", new { code, nonce = NewNonce() })) == "revoked", "missing activation identity is an ordinary business rejection");
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(input), "invalid")) == "revoked", "invalid bearer is an ordinary revoked state");
    Assert(State(await Post("/api/v1/activate", permanentInput with { RequestId = "old-version", ClientVersion = "0.59.0" })) == "update_required", "minimum version gate returns business state");
    using (var malformed = await anonymous.PostAsync("/api/v1/activate", new StringContent("{", Encoding.UTF8, "application/json")))
        Assert(malformed.StatusCode == HttpStatusCode.BadRequest && (await malformed.Content.ReadAsStringAsync()).Contains("invalid_request"), "malformed JSON produces an error object");

    var parallel = await Issue("permanent"); var parallelCode = Text(parallel, "code");
    var contenders = await Task.WhenAll(Enumerable.Range(0, 8).Select(index => Post("/api/v1/activate", Activation(parallelCode, "parallel-" + index))));
    Assert(contenders.Count(item => State(item) == "active") == 1 && contenders.Count(item => State(item) == "in_use") == 7,
        "eight concurrent HTTP logins produce exactly one winner");
    var replacedCard = await Issue("permanent");
    var replacedId = Text(replacedCard, "id");
    var replacedInput = Activation(Text(replacedCard, "code"), "unobserved-expiry");
    await Post("/api/v1/activate", replacedInput);
    clock.Advance(61);
    var replacement = await Post("/api/v1/activate", replacedInput with { RequestId = "replacement" });
    clock.Advance(-2);
    Assert(State(replacement) == "active" && State(await Post("/api/v1/activate", replacedInput)) == "revoked",
        "replacement login permanently retires unobserved expired request");
    Assert((await Get($"/api/admin/sessions?codeId={replacedId}")).GetProperty("items").GetArrayLength() == 1,
        "clock rollback after replacement preserves exactly one effective session");
    clock.Advance(2);
    var retryCard = await Issue("permanent");
    var parallelInput = Activation(Text(retryCard, "code"), "parallel-idempotent");
    var sameRequests = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Post("/api/v1/activate", parallelInput)));
    Assert(sameRequests.All(item => State(item) == "active") && sameRequests.Select(item => Text(item, "session_token")).Distinct().Count() == 1,
        "eight concurrent identical retries recover the same session");
    var paged = await Get("/api/admin/codes?page=1&pageSize=1");
    Assert(paged.GetProperty("items").GetArrayLength() == 1 && Number(paged, "total") > 1 && Number(paged, "pageSize") == 1,
        "management codes return pagination envelope");
    var unusedRows = await Get("/api/admin/codes?status=unused");
    Assert(unusedRows.GetProperty("items").EnumerateArray().All(item => State(item) == "unused"), "management status filter matches code state");
    var exported = await admin.GetStringAsync("/api/admin/codes/export?search=" + Uri.EscapeDataString(cardId));
    Assert(exported.Contains(code) && exported.Contains("\"'=SUM(1,1),\"\"quoted\"\"\nline\""), "CSV includes full code and neutralizes formulas while escaping quotes and newlines");
    var audit = await Get("/api/admin/audit?pageSize=500");
    var auditText = audit.GetRawText();
    Assert(Number(audit, "total") > 0 && !auditText.Contains(code) && !auditText.Contains(token), "audit has pagination and excludes sensitive plaintext");
    var invalidPage = await Post($"/api/admin/codes/missing/kick", new { }, authenticated: true, expected: HttpStatusCode.NotFound);
    Assert(Text(invalidPage, "error") == "not_found", "unknown management code returns error object");

    var rollbackCard = await Issue("permanent");
    var rollbackInput = Activation(Text(rollbackCard, "code"), "clock-heartbeat");
    var rollbackSession = await Post("/api/v1/activate", rollbackInput);
    clock.Advance(-100);
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(rollbackInput), Text(rollbackSession, "session_token"))) == "revoked",
        "large server clock rollback revokes heartbeat as a business state");
    clock.Advance(100);
    Assert(State(await Post("/api/v1/heartbeat", Heartbeat(rollbackInput), Text(rollbackSession, "session_token"))) == "revoked",
        "clock correction does not revive a session revoked by clock rollback");

    var durable = await Issue("permanent"); var durableCode = Text(durable, "code");
    var durableInput = Activation(durableCode, "durable");
    var durableLease = await Post("/api/v1/activate", durableInput);
    var durableToken = Text(durableLease, "session_token");
    clock.Advance(-100);
    var rollback = await Post("/api/v1/activate", durableInput);
    Assert(State(rollback) == "active" && Number(rollback, "lease_until") - Number(rollback, "server_time") <= 60,
        "clock rollback still limits response lease to 60 seconds");
    clock.Advance(100);
    var persisted = Encoding.UTF8.GetString(await File.ReadAllBytesAsync(options.DatabasePath));
    if (File.Exists(options.DatabasePath + "-wal")) persisted += Encoding.UTF8.GetString(await File.ReadAllBytesAsync(options.DatabasePath + "-wal"));
    Assert(!persisted.Contains(durableCode) && !persisted.Contains(durableToken), "SQLite and WAL do not store complete code or bearer plaintext");
    await app.StopAsync(); await app.DisposeAsync();
    app = ServerProgram.Build(options, ["--Logging:LogLevel:Default", "Warning"], clock);
    await app.StartAsync(); Connect(); await RefreshCsrf();
    Assert(app.Services.GetRequiredService<LeaseSigner>().ExportPublicKey() == publicKey, "restart preserves exact signing modulus exponent and kid");
    Assert(Verify(first, input, pinned: publicKey), "pre-restart signature remains verifiable with pinned public key");
    var recovered = await Card(Text(durable, "id"));
    Assert(Text(recovered, "code") == durableCode, "persistent keys recover full administrator code after restart");
    var recoveredLease = await Post("/api/v1/activate", durableInput);
    Assert(Verify(recoveredLease, durableInput, pinned: publicKey), "post-restart active response verifies using original public pin");
    Assert(Text(recoveredLease, "session_token") == durableToken && Number(recoveredLease, "lease_until") == Number(durableLease, "lease_until"),
        "restart preserves encrypted idempotent token and original lease");
    Assert((await Get("/api/admin/settings")).TryGetProperty("productId", out _), "persistent administrator cookie survives restart");
    Assert((await Post("/api/admin/logout", new { }, authenticated: true)).GetProperty("ok").GetBoolean(), "management logout returns JSON acknowledgement");
    Assert((await admin.GetAsync("/api/admin/settings")).StatusCode == HttpStatusCode.Unauthorized, "management logout clears administrator session");

    var oldPath = Path.Combine(root, "old-sentinel.db");
    using (var db = new SqliteConnection("Data Source=" + oldPath + ";Pooling=False"))
    {
        db.Open(); using var command = db.CreateCommand();
        command.CommandText = "CREATE TABLE Codes(Id TEXT PRIMARY KEY); INSERT INTO Codes VALUES('sentinel');";
        command.ExecuteNonQuery();
    }
    var oldHash = SHA256.HashData(await File.ReadAllBytesAsync(oldPath));
    var oldRejected = false;
    try
    {
        var wrong = ServerProgram.Build(options with { DatabasePath = oldPath, KeyDirectory = Path.Combine(root, "old-keys") }, ["--Logging:LogLevel:Default", "Warning"], clock);
        await wrong.DisposeAsync();
    }
    catch (InvalidOperationException error) when (error.Message.Contains("旧版")) { oldRejected = true; }
    Assert(oldRejected, "native server refuses a legacy database");
    var oldHashAfterRejection = SHA256.HashData(await File.ReadAllBytesAsync(oldPath));
    Assert(oldHash.SequenceEqual(oldHashAfterRejection), "refusing legacy database leaves its bytes unchanged");
    var report = new { mode = "native-leases-real-http-and-sqlite", passed = passed.Count, checks = passed,
        targetFramework = AppContext.TargetFrameworkName, runtimeVersion = Environment.Version.ToString(),
        frameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
        processArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
        completedAtUtc = DateTimeOffset.UtcNow, testDirectory = root };
    await File.WriteAllTextAsync(Path.Combine(root, "results.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine($"RESULT: {passed.Count} passed; real HTTP + SQLite + persisted protection keys; report {root}");
    return 0;
}
finally
{
    anonymous?.Dispose(); admin?.Dispose();
    await app.StopAsync(); await app.DisposeAsync();
}

sealed class ManualTimeProvider(DateTimeOffset initial) : TimeProvider
{
    private long seconds = initial.ToUnixTimeSeconds();
    public override DateTimeOffset GetUtcNow() => DateTimeOffset.FromUnixTimeSeconds(Interlocked.Read(ref seconds));
    public void Advance(long amount) => Interlocked.Add(ref seconds, amount);
}
