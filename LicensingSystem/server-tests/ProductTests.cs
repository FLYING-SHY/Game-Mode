using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Licensing.Server;

internal static class ProductTests
{
    public static async Task Run(string parent, Action<bool, string> assert)
    {
        var root = Path.Combine(parent, "products");
        Directory.CreateDirectory(root);
        var config = Path.Combine(root, "config.json");
        await File.WriteAllTextAsync(config, JsonSerializer.Serialize(new
        {
            ListenUrl = "http://127.0.0.1:0", DatabasePath = Path.Combine(root, "coh3.db"),
            KeyDirectory = Path.Combine(root, "coh3-keys"), AdminToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            ZeppBackendToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            Products = new[] { new { ProductId = "ZeppPortal", ProductName = "Zepp 网站", CodePrefix = "ZEPP",
                DatabasePath = Path.Combine(root, "zepp.db"), KeyDirectory = Path.Combine(root, "zepp-keys") } }
        }));
        var options = ServerOptions.Load(config);
        var clock = new ManualTimeProvider(DateTimeOffset.Parse("2026-11-01T00:00:00Z"));
        await using var app = Licensing.Server.Program.Build(options, ["--Logging:LogLevel:Default", "Warning"], clock);
        await app.StartAsync();
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() })
            { BaseAddress = new Uri(app.Urls.Single()) };
        var loginHtml = await client.GetStringAsync("/admin/login");
        var csrf = WebUtility.HtmlDecode(Regex.Match(loginHtml, "name=__RequestVerificationToken value=\"([^\"]+)\"").Groups[1].Value);
        using var login = await client.PostAsync("/admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
            { ["token"] = options.AdminToken, ["__RequestVerificationToken"] = csrf }));
        if (login.StatusCode != HttpStatusCode.Redirect) throw new InvalidOperationException("Product fixture login failed.");
        var settings = await client.GetFromJsonAsync<JsonElement>("/api/admin/settings?productId=ZeppPortal");
        assert(settings.TryGetProperty("products", out var products) && products.GetArrayLength() == 2
            && settings.GetProperty("productId").GetString() == "ZeppPortal", "product selection resolves independent Zepp settings while retaining default product");
        using var entitlementRequest = new HttpRequestMessage(HttpMethod.Post, "/internal/zepp/entitlement")
        {
            Content = JsonContent.Create(new { account_id = "123456789" })
        };
        entitlementRequest.Headers.Authorization = new("Bearer", options.ZeppBackendToken);
        using var entitlementResponse = await client.SendAsync(entitlementRequest);
        assert(entitlementResponse.StatusCode == HttpStatusCode.OK,
            "loopback backend token can query explicit empty Zepp entitlement");
        var entitlement = await entitlementResponse.Content.ReadFromJsonAsync<JsonElement>();
        assert(entitlement.GetProperty("success").GetBoolean() && entitlement.GetProperty("product_id").GetString() == "ZeppPortal"
            && entitlement.GetProperty("account_id").GetString() == "123456789" && entitlement.GetProperty("grants").GetArrayLength() == 0,
            "empty Zepp entitlement uses strict snake_case response contract");
        var adminHtml = await client.GetStringAsync("/admin");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", WebUtility.HtmlDecode(Regex.Match(adminHtml, "name=\\\"csrf-token\\\" content=\\\"([^\\\"]+)\\\"").Groups[1].Value));
        async Task<JsonElement> Post(string route, object body, HttpStatusCode expected = HttpStatusCode.OK, string? bearer = null, HttpClient? target = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, route) { Content = JsonContent.Create(body) };
            if (bearer is not null) request.Headers.Authorization = new("Bearer", bearer);
            using var response = await (target ?? client).SendAsync(request);
            if (response.StatusCode != expected) throw new InvalidOperationException($"HTTP {(int)response.StatusCode} instead of {(int)expected}: {route}");
            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }
        async Task<JsonElement> Issue(string kind, long? duration = null, string product = "ZeppPortal") =>
            (await Post("/api/admin/codes?productId=" + product, new { kind, durationSeconds = duration, quantity = 1 })).GetProperty("items")[0];
        Task<JsonElement> Redeem(JsonElement card, string account = "123456789", string? requestId = null, HttpStatusCode expected = HttpStatusCode.OK) =>
            Post("/internal/zepp/redeem", new { account_id = account, code = card.GetProperty("code").GetString(), request_id = requestId ?? Guid.NewGuid().ToString() }, expected, options.ZeppBackendToken);
        var first = await Issue("timed", 604800);
        var firstRequest = Guid.NewGuid().ToString();
        var redeemed = await Redeem(first, requestId: firstRequest);
        var receipt = redeemed.GetProperty("receipt");
        assert(receipt.GetProperty("starts_at").GetString() == "2026-11-01T00:00:00Z"
            && receipt.GetProperty("expires_at").GetString() == "2026-11-08T00:00:00Z"
            && !receipt.GetProperty("permanent").GetBoolean() && redeemed.GetProperty("grants").GetArrayLength() == 1,
            "first timed Zepp redemption begins now and returns its fixed seven-day receipt");
        clock.Advance(3600);
        var retried = await Redeem(first, requestId: firstRequest);
        assert(retried.GetProperty("receipt").GetRawText() == receipt.GetRawText(), "same request remains idempotent beyond the old sixty-second device lease");
        var sameCard = await Redeem(first);
        assert(sameCard.GetProperty("receipt").GetRawText() == receipt.GetRawText() && sameCard.GetProperty("grants").GetArrayLength() == 1,
            "same card and UID with a new request retain the original receipt without extra time");
        assert((await Redeem(first, "987654321", expected: HttpStatusCode.Conflict)).GetProperty("code").GetString() == "card_used",
            "another UID cannot consume an already redeemed card");
        assert((await Redeem(first, "987654321", firstRequest, HttpStatusCode.Conflict)).GetProperty("code").GetString() == "request_conflict",
            "same request with another UID conflicts");
        var second = await Issue("timed", 172800);
        assert((await Redeem(second, requestId: firstRequest, expected: HttpStatusCode.Conflict)).GetProperty("code").GetString() == "request_conflict",
            "same request with another card conflicts without consuming it");
        var renewed = await Redeem(second);
        var secondReceipt = renewed.GetProperty("receipt");
        assert(secondReceipt.GetProperty("starts_at").GetString() == "2026-11-08T00:00:00Z"
            && secondReceipt.GetProperty("expires_at").GetString() == "2026-11-10T00:00:00Z",
            "second timed card starts at the already scheduled endpoint rather than redemption time");
        await Post("/api/admin/codes/" + first.GetProperty("id").GetString() + "/suspension?productId=ZeppPortal", new { suspended = true });
        var suspended = await Redeem(first, requestId: firstRequest);
        assert(suspended.GetProperty("receipt").GetProperty("suspended").GetBoolean()
            && suspended.GetProperty("grants")[1].GetProperty("starts_at").GetString() == "2026-11-08T00:00:00Z",
            "suspending one card updates its historical receipt without advancing the following card");
        clock.Advance(600);
        await Post("/api/admin/codes/" + first.GetProperty("id").GetString() + "/suspension?productId=ZeppPortal", new { suspended = false });
        var restored = await Redeem(first, requestId: firstRequest);
        assert(!restored.GetProperty("receipt").GetProperty("suspended").GetBoolean()
            && restored.GetProperty("receipt").GetProperty("expires_at").GetString() == "2026-11-08T00:00:00Z",
            "restoring a suspended card retains original expiry without compensation");
        await Post("/api/admin/codes/" + first.GetProperty("id").GetString() + "/renew?productId=ZeppPortal", new { durationSeconds = 86400 });
        var adminRenewed = await Redeem(first, requestId: firstRequest);
        assert(adminRenewed.GetProperty("receipt").GetProperty("expires_at").GetString() == "2026-11-09T00:00:00Z"
            && adminRenewed.GetProperty("grants")[1].GetProperty("starts_at").GetString() == "2026-11-08T00:00:00Z",
            "admin renewal changes only its grant while the following card keeps its fixed start");
        await Post("/api/admin/codes/" + first.GetProperty("id").GetString() + "/delete?productId=ZeppPortal", new { });
        var deleted = await Redeem(first, requestId: firstRequest);
        assert(deleted.GetProperty("receipt").GetProperty("revoked").GetBoolean()
            && deleted.GetProperty("receipt").GetProperty("redemption_id").GetString() == receipt.GetProperty("redemption_id").GetString(),
            "deleting a used card preserves its revoked unique historical receipt");
        var deletedRetry = await Redeem(first);
        assert(deletedRetry.GetProperty("receipt").GetProperty("revoked").GetBoolean()
            && deletedRetry.GetProperty("grants").GetArrayLength() == 2,
            "same UID retry after physical deletion cannot recreate or lengthen entitlement");
        var permanent = await Issue("permanent");
        var permanentResult = await Redeem(permanent);
        var permanentReceipt = permanentResult.GetProperty("receipt");
        assert(permanentReceipt.GetProperty("permanent").GetBoolean()
            && permanentReceipt.GetProperty("expires_at").ValueKind == JsonValueKind.Null
            && permanentReceipt.GetProperty("starts_at").GetString() == "2026-11-01T01:10:00Z",
            "timed member obtains permanent entitlement immediately with null expiry");
        var spared = await Issue("timed", 3600);
        assert((await Redeem(spared, expected: HttpStatusCode.Conflict)).GetProperty("code").GetString() == "permanent_member",
            "active permanent membership leaves a timed card unconsumed");
        var sparedResult = await Redeem(spared, "555555555");
        assert(sparedResult.GetProperty("grants").GetArrayLength() == 1, "another account can still consume the timed card spared for a permanent member");
        var forbiddenUnbind = await Post("/api/admin/codes/" + permanent.GetProperty("id").GetString() + "/unbind?productId=ZeppPortal", new { }, HttpStatusCode.Forbidden);
        assert(forbiddenUnbind.GetProperty("error").GetString() == "account_bound", "Zepp admin unbind cannot create an account reassignment bypass");
        var forbiddenKick = await Post("/api/admin/codes/" + permanent.GetProperty("id").GetString() + "/kick?productId=ZeppPortal", new { }, HttpStatusCode.Forbidden);
        assert(forbiddenKick.GetProperty("error").GetString() == "account_bound", "Zepp admin kick rejects the device-only mutation");
        var scheduledPage = await client.GetFromJsonAsync<JsonElement>("/api/admin/codes?productId=ZeppPortal&search=" + second.GetProperty("id").GetString());
        var scheduledCard = scheduledPage.GetProperty("items")[0];
        assert(scheduledCard.GetProperty("accountId").GetString() == "123456789"
            && scheduledCard.GetProperty("state").GetString() == "scheduled"
            && DateTimeOffset.FromUnixTimeSeconds(scheduledCard.GetProperty("startsAtUnix").GetInt64()).ToString("u") == "2026-11-08 00:00:00Z",
            "Zepp admin list exposes the bound UID and future fixed interval as scheduled");
        var zeppCsv = await client.GetStringAsync("/api/admin/codes/export?productId=ZeppPortal");
        assert(zeppCsv.Contains("accountId,startsAtUnix") && zeppCsv.Contains("123456789"), "Zepp CSV exports the selected account-bound card metadata");
        var coh3Card = await Issue("timed", 7200, "CoH3FowTool");
        var coh3Id = coh3Card.GetProperty("id").GetString();
        assert((await Redeem(coh3Card, expected: HttpStatusCode.NotFound)).GetProperty("code").GetString() == "invalid_card", "a CoH3 card never grants Zepp entitlement");
        var rootSettings = await client.GetFromJsonAsync<JsonElement>("/api/admin/settings");
        assert(rootSettings.GetProperty("productId").GetString() == "CoH3FowTool", "unscoped admin calls retain the original default product");
        using (var unknown = await client.GetAsync("/api/admin/codes?productId=missing"))
            assert(unknown.StatusCode == HttpStatusCode.BadRequest, "unknown product fails rather than falling back to a card pool");
        using (var duplicate = await client.GetAsync("/api/admin/codes?productId=CoH3FowTool&productId=ZeppPortal"))
            assert(duplicate.StatusCode == HttpStatusCode.BadRequest, "ambiguous product query fails closed");
        var coh3Page = await client.GetFromJsonAsync<JsonElement>("/api/admin/codes?productId=CoH3FowTool");
        assert(coh3Page.GetProperty("items").GetArrayLength() == 1
            && coh3Page.GetProperty("items")[0].GetProperty("id").GetString() == coh3Id, "CoH3 reads exclude every Zepp card");
        var scopedZeppPage = await client.GetFromJsonAsync<JsonElement>("/api/admin/codes?productId=ZeppPortal&search=" + coh3Id);
        assert(scopedZeppPage.GetProperty("total").GetInt32() == 0, "Zepp reads exclude CoH3 card IDs");
        foreach (var mutation in new[] { "suspension", "renew", "unbind", "kick", "delete" })
        {
            var result = await Post("/api/admin/codes/" + second.GetProperty("id").GetString() + "/" + mutation + "?productId=CoH3FowTool",
                new { suspended = true, durationSeconds = 60 }, HttpStatusCode.NotFound);
            assert(result.GetProperty("error").GetString() == "not_found", "cross-product " + mutation + " does not affect the foreign card");
        }
        await Post("/api/admin/codes/delete-batch?productId=CoH3FowTool", new { ids = new[] { coh3Id, second.GetProperty("id").GetString() } }, HttpStatusCode.NotFound);
        var stillCoH3 = await client.GetFromJsonAsync<JsonElement>("/api/admin/codes?productId=CoH3FowTool");
        assert(stillCoH3.GetProperty("total").GetInt32() == 1, "mixed-product batch deletion is atomic and preserves valid selected-product cards");
        var coh3Csv = await client.GetStringAsync("/api/admin/codes/export?productId=CoH3FowTool");
        assert(coh3Csv.Contains(coh3Card.GetProperty("code").GetString()!) && !coh3Csv.Contains(second.GetProperty("code").GetString()!), "CoH3 CSV does not export any Zepp card");
        var zeppAudit = await client.GetFromJsonAsync<JsonElement>("/api/admin/audit?productId=ZeppPortal");
        assert(!zeppAudit.GetProperty("items").EnumerateArray().Any(item => item.GetProperty("codeId").GetString() == coh3Id), "Zepp audit events stay scoped to their own database");
        var zeppSessions = await client.GetFromJsonAsync<JsonElement>("/api/admin/sessions?productId=ZeppPortal");
        assert(zeppSessions.GetProperty("items").GetArrayLength() == 0, "Zepp has no device sessions");
        var rootVersion = await client.GetFromJsonAsync<JsonElement>("/api/v1/version?productId=ZeppPortal");
        assert(rootVersion.GetProperty("product_id").GetString() == "CoH3FowTool", "legacy client routes ignore admin product selection and retain CoH3 identity");
        var deviceAttempt = await Post("/api/v1/activate?productId=ZeppPortal", new ActivationRequest
        {
            Code = second.GetProperty("code").GetString(), RequestId = Guid.NewGuid().ToString(), DeviceId = new string('a', 64),
            SteamId64 = "76561198000000001", InstallationId = Guid.NewGuid().ToString(), ClientVersion = "0.60.8",
            Nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant()
        });
        assert(deviceAttempt.GetProperty("state").GetString() == "revoked", "a Zepp card cannot activate via the default game device route");
        foreach (var token in new string?[] { null, options.AdminToken, "incorrect-backend-token" })
        {
            var denied = await Post("/internal/zepp/entitlement", new { account_id = "123456789" }, HttpStatusCode.Forbidden, token);
            assert(denied.GetProperty("success").ValueKind == JsonValueKind.False && denied.GetProperty("code").GetString() == "forbidden",
                "internal endpoint denies missing, admin-only or incorrect Bearer tokens");
        }
        foreach (var uid in new[] { "", "abc", "123 456", new string('1', 33), "１２３" })
        {
            var invalid = await Post("/internal/zepp/entitlement", new { account_id = uid }, HttpStatusCode.BadRequest, options.ZeppBackendToken);
            assert(invalid.GetProperty("code").GetString() == "invalid_request", "internal entitlement rejects malformed decimal UID");
        }
        var invalidRequest = await Post("/internal/zepp/redeem", new { account_id = "123456789", code = "x", request_id = "bad-uuid" }, HttpStatusCode.BadRequest, options.ZeppBackendToken);
        assert(invalidRequest.GetProperty("code").GetString() == "invalid_request", "redemption requires a canonical UUID");
        var hugeCode = await Post("/internal/zepp/redeem", new { account_id = "123456789", code = new string('x', 257), request_id = Guid.NewGuid().ToString() }, HttpStatusCode.BadRequest, options.ZeppBackendToken);
        assert(hugeCode.GetProperty("code").GetString() == "invalid_request", "redemption bounds code size before lookup");
        var raceCard = await Issue("timed", 60);
        async Task<(HttpStatusCode Status, JsonElement Body)> Race(string uid)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/zepp/redeem")
            { Content = JsonContent.Create(new { account_id = uid, code = raceCard.GetProperty("code").GetString(), request_id = Guid.NewGuid().ToString() }) };
            request.Headers.Authorization = new("Bearer", options.ZeppBackendToken);
            using var response = await client.SendAsync(request);
            return (response.StatusCode, await response.Content.ReadFromJsonAsync<JsonElement>());
        }
        var raced = await Task.WhenAll(Race("111111111"), Race("222222222"));
        assert(raced.Count(result => result.Status == HttpStatusCode.OK) == 1 && raced.Count(result => result.Status == HttpStatusCode.Conflict) == 1,
            "concurrent cross-UID redemption has exactly one transaction winner");
        var raceWinner = raced.Single(result => result.Status == HttpStatusCode.OK).Body;
        assert(raceWinner.GetProperty("grants").GetArrayLength() == 1, "concurrent winner receives one grant only");
        var inactive = await Issue("timed", 60);
        await Post("/api/admin/codes/" + inactive.GetProperty("id").GetString() + "/suspension?productId=ZeppPortal", new { suspended = true });
        assert((await Redeem(inactive, "333333333", expected: HttpStatusCode.Forbidden)).GetProperty("code").GetString() == "card_suspended", "suspended unused card cannot be consumed");
        await Post("/api/admin/codes/" + inactive.GetProperty("id").GetString() + "/suspension?productId=ZeppPortal", new { suspended = false });
        var inactiveRequest = Guid.NewGuid().ToString();
        var shortGrant = await Redeem(inactive, "333333333", inactiveRequest);
        clock.Advance(61);
        var expiredRetry = await Redeem(inactive, "333333333", inactiveRequest);
        assert(shortGrant.GetProperty("receipt").GetRawText() == expiredRetry.GetProperty("receipt").GetRawText(), "expired-card retry returns the old interval rather than starting again");
        var afterExpiry = await Issue("timed", 120);
        var afterExpiryResult = await Redeem(afterExpiry, "333333333");
        assert(afterExpiryResult.GetProperty("receipt").GetProperty("starts_at").GetString() == "2026-11-01T01:11:01Z", "renewal after expiration begins now rather than the past scheduled end");
        var faultCard = await Issue("timed", 600);
        var faultRequest = Guid.NewGuid().ToString();
        using (var db = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=" + options.Products[0].DatabasePath))
        {
            db.Open();
            using var command = db.CreateCommand();
            command.CommandText = "CREATE TRIGGER FailZeppRequest BEFORE INSERT ON ZeppRequests BEGIN SELECT RAISE(ABORT,'injected-receipt-failure'); END;";
            command.ExecuteNonQuery();
            var faultResult = await Redeem(faultCard, "444444444", faultRequest, HttpStatusCode.ServiceUnavailable);
            assert(faultResult.GetProperty("success").ValueKind == JsonValueKind.False, "storage fault returns an internal failure response rather than a false entitlement denial");
            var afterFault = await Post("/internal/zepp/entitlement", new { account_id = "444444444" }, bearer: options.ZeppBackendToken);
            assert(afterFault.GetProperty("grants").GetArrayLength() == 0, "failed receipt insertion rolls back the grant in the same transaction");
            var faultCardPage = await client.GetFromJsonAsync<JsonElement>("/api/admin/codes?productId=ZeppPortal&search=" + faultCard.GetProperty("id").GetString());
            assert(faultCardPage.GetProperty("items")[0].GetProperty("state").GetString() == "unused", "failed receipt insertion also rolls back card consumption");
            command.CommandText = "DROP TRIGGER FailZeppRequest";
            command.ExecuteNonQuery();
        }
        var recoveredFault = await Redeem(faultCard, "444444444", faultRequest);
        assert(recoveredFault.GetProperty("grants").GetArrayLength() == 1, "same request succeeds exactly once after storage recovery");
        await app.StopAsync();
        await using var restarted = Licensing.Server.Program.Build(options, ["--Logging:LogLevel:Default", "Warning"], clock);
        await restarted.StartAsync();
        using var restartClient = new HttpClient { BaseAddress = new Uri(restarted.Urls.Single()) };
        var durable = await Post("/internal/zepp/redeem", new { account_id = "123456789", code = first.GetProperty("code").GetString(), request_id = firstRequest }, bearer: options.ZeppBackendToken, target: restartClient);
        assert(durable.GetProperty("receipt").GetProperty("redemption_id").GetString() == receipt.GetProperty("redemption_id").GetString()
            && durable.GetProperty("receipt").GetProperty("revoked").GetBoolean(), "restart retains the same durable receipt even for a deleted card");
        var durableEntitlement = await Post("/internal/zepp/entitlement", new { account_id = "123456789" }, bearer: options.ZeppBackendToken, target: restartClient);
        assert(durableEntitlement.GetProperty("grants").GetArrayLength() == 3
            && durableEntitlement.GetProperty("grants").EnumerateArray().Any(grant => grant.GetProperty("permanent").GetBoolean()), "restart restores permanent and historical timed grants without extra issuance");
        await restarted.StopAsync();
        await VerifyConfigurationAndPeer(options, root, clock, assert);
    }

    private static async Task VerifyConfigurationAndPeer(ServerOptions options, string root, TimeProvider clock, Action<bool, string> assert)
    {
        void Invalid(ServerOptions candidate, string message)
        {
            var rejected = false;
            try { candidate.Validate(); } catch (InvalidOperationException) { rejected = true; }
            assert(rejected, message);
        }
        Invalid(options with { ZeppBackendToken = options.AdminToken }, "configuration requires a backend token distinct from the admin token");
        Invalid(options with { ZeppBackendToken = null }, "Zepp configuration requires its own backend token");
        Invalid(options with { Products = [options.Products[0] with { DatabasePath = options.DatabasePath }] }, "two products cannot share a database path");
        Invalid(options with { Products = [options.Products[0] with { KeyDirectory = Path.Combine(options.KeyDirectory, "nested") }] }, "two products cannot share nested keyring directories");
        Invalid(options with { Products = [options.Products[0] with { ProductId = "coh3fowtool" }] }, "product identity collisions are rejected case-insensitively");
        var oldOnly = options with { Products = [], ZeppBackendToken = null };
        oldOnly.Validate();
        assert(oldOnly.ProductConfigurations().Count == 1, "legacy root-only configuration remains valid without new token settings");
        var zeppOptions = options.ProductConfigurations().Single(product => product.ProductId == "ZeppPortal") with { ListenUrl = "http://127.0.0.1:0" };
        var databaseRejected = false;
        try
        {
            await using var incompatible = Licensing.Server.Program.Build(zeppOptions with { DatabasePath = options.DatabasePath }, ["--Logging:LogLevel:Default", "Warning"], clock);
        }
        catch (InvalidOperationException) { databaseRejected = true; }
        assert(databaseRejected, "opening a CoH3-owned database as Zepp fails the persisted product guard");
        using (var db = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=" + options.DatabasePath))
        {
            db.Open();
            using var command = db.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('ZeppRedemptions','ZeppRequests')";
            assert(Convert.ToInt64(command.ExecuteScalar()) == 0, "rejected Zepp startup does not add Zepp tables to the CoH3 database");
        }
        await using (var zepp = Licensing.Server.Program.Build(zeppOptions, ["--Logging:LogLevel:Default", "Warning"], clock))
        {
            await zepp.StartAsync();
            using var client = new HttpClient { BaseAddress = new Uri(zepp.Urls.Single()) };
            foreach (var route in new[] { "activate", "heartbeat", "logout" })
            {
                using var denied = await client.PostAsJsonAsync("/api/v1/" + route, new { });
                var body = await denied.Content.ReadFromJsonAsync<JsonElement>();
                assert(denied.StatusCode == HttpStatusCode.Forbidden && body.GetProperty("error").GetString() == "account_bound", "Zepp-only host closes legacy device " + route + " route");
            }
            await zepp.StopAsync();
        }
        var address = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));
        var peerOptions = options with
        {
            ListenUrl = "http://" + address + ":0", AllowedHosts = [address.ToString()], TrustedProxies = [address.ToString()],
            DatabasePath = Path.Combine(root, "peer-coh3.db"), KeyDirectory = Path.Combine(root, "peer-coh3-keys"),
            Products = [options.Products[0] with { DatabasePath = Path.Combine(root, "peer-zepp.db"), KeyDirectory = Path.Combine(root, "peer-zepp-keys") }]
        };
        await using var peerApp = Licensing.Server.Program.Build(peerOptions, ["--Logging:LogLevel:Default", "Warning"], clock);
        await peerApp.StartAsync();
        using var peerClient = new HttpClient(new HttpClientHandler { UseProxy = false }) { BaseAddress = new Uri(peerApp.Urls.Single()) };
        using var forged = new HttpRequestMessage(HttpMethod.Post, "/internal/zepp/entitlement") { Content = JsonContent.Create(new { account_id = "123456789" }) };
        forged.Headers.Authorization = new("Bearer", options.ZeppBackendToken);
        forged.Headers.Add("X-Forwarded-For", "127.0.0.1");
        using var response = await peerClient.SendAsync(forged);
        var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
        assert(response.StatusCode == HttpStatusCode.Forbidden && responseBody.GetProperty("code").GetString() == "forbidden", "trusted non-loopback socket peer with forged loopback XFF is denied before forwarded headers");
        await peerApp.StopAsync();
    }
}
