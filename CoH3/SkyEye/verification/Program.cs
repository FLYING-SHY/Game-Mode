using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CoH3MemoryTool;

internal static class Program
{
    private static readonly Stopwatch elapsed = Stopwatch.StartNew();
    private static readonly List<object> events = new List<object>();
    private static string sandbox;

    private static int Main(string[] args)
    {
        try { return Run(args).GetAwaiter().GetResult(); }
        catch (Exception ex)
        {
            Record("FAIL", ex.GetType().Name);
            SaveReport(false);
            Console.Error.WriteLine("FAIL " + ex.GetType().Name + ": " + ex.Message);
            return 1;
        }
    }

    private static async Task<int> Run(string[] args)
    {
        Dictionary<string, string> options = Parse(args);
        string mode = Value(options, "mode", "selfcheck");
        string allowedRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "sandbox"));
        sandbox = Path.GetFullPath(Value(options, "sandbox", Path.Combine(allowedRoot, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8))));
        Require(sandbox.StartsWith(allowedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "Sandbox must be a child of verification/sandbox.");
        Directory.CreateDirectory(sandbox);
        Record("START", mode);
        Require(Environment.Is64BitProcess, "Verifier must run as x64.");
        if (mode == "selfcheck") SelfCheck();
        else if (mode == "ui") HardenedChecks.Ui(sandbox);
        else if (mode == "audio") HardenedChecks.Audio(sandbox);
        else
        {
            Uri address = LicenseConfiguration60.ParseServerBaseAddress(Value(options, "base-url", LicenseConfiguration60.ServerBaseAddress.AbsoluteUri));
            Require(options.ContainsKey("code-file"), "Supply --code-file with an isolated test card file.");
            string code = File.ReadAllText(options["code-file"]).Trim();
            Require(code.Length > 0 && code.Length < 1024, "Test card file must contain one nonempty code.");
            string kind = Value(options, "expected-kind", "permanent");
            if (mode == "protocol") await Protocol(address, code, kind);
            else if (mode == "coordinator" || mode == "network" || mode == "observe")
                await Coordinator(address, code, kind, mode, Value(options, "expected-state", "login_required"), int.Parse(Value(options, "timeout-seconds", "100")));
            else throw new ArgumentException("Unknown mode: " + mode);
        }
        Record("PASS", mode);
        SaveReport(true);
        return 0;
    }

    private static void SelfCheck()
    {
        SignatureChecks.Run();
        Require(LicenseConfiguration60.ProductId == "CoH3FowTool", "Default product must match the contract.");
        Require(LicenseVersion60.DownloadAllowed("http://127.0.0.1:47835/download/client.exe"), "Configured loopback download host must be allowed.");
        Require(!LicenseVersion60.DownloadAllowed("https://unrelated.example/download/client.exe"), "Unrelated download hosts must be rejected.");
        Require(!LicenseVersion60.DownloadAllowed("http://127.0.0.1:47832/download/client.exe"), "Different download ports must be rejected.");
        Require(!LicenseVersion60.DownloadAllowed("http://user@127.0.0.1:47835/file"), "Download userinfo must be rejected.");
        Require(LicenseConfiguration60.ParseServerBaseAddress("https://licensing.example").AbsoluteUri == "https://licensing.example/", "HTTPS base URL normalization failed.");
        foreach (string invalid in new[] { "http://licensing.example/", "file:///tmp/license", "https://user@licensing.example/", "https://licensing.example/?token=x", "https://licensing.example/#x" })
        {
            bool rejected = false;
            try { LicenseConfiguration60.ParseServerBaseAddress(invalid); }
            catch (ArgumentException) { rejected = true; }
            Require(rejected, "Invalid server base URL accepted.");
        }
        Record("PASS", "configuration and same-origin download policy");
        Require(LicenseDevice60.Fingerprint("CoH3FowTool", " 00112233-4455-6677-8899-AABBCCDDEEFF ") == "428300FDED0D955C71A7C695C8256512E69D5272F81AD2D95F4E3B9362F15EA0", "MachineGuid product hash fixture mismatch.");
        Require(Regex.IsMatch(LicenseDevice60.GetWindowsDeviceId(), "\\A[0-9A-F]{64}\\z"), "Real Windows device fingerprint format mismatch.");
        Require(LicenseDevice60.Fingerprint("OtherProduct", "00112233-4455-6677-8899-aabbccddeeff") != "428300FDED0D955C71A7C695C8256512E69D5272F81AD2D95F4E3B9362F15EA0", "Product hash spaces must differ.");
        Record("PASS", "reference fingerprint fixture and real MachineGuid provider");
        string store = Path.Combine(sandbox, "credentials");
        LicenseCredentials60 credentials = LicenseStore60.Load(store);
        Require(credentials.Code == "" && Guid.TryParse(credentials.Installation, out _), "Fresh sandbox must not load existing credentials.");
        credentials.Code = "SANDBOX-ROUNDTRIP-ONLY";
        LicenseStore60.Save(credentials, store);
        LicenseCredentials60 restored = LicenseStore60.Load(store);
        Require(restored.Code == credentials.Code && restored.Installation == credentials.Installation, "DPAPI sandbox credentials must round-trip.");
        Require(!System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(Path.Combine(store, "credentials.dat"))).Contains(credentials.Code), "Sandbox credentials must be encrypted.");
        Require(LicenseStore60.DirectoryPath.EndsWith(Path.Combine("Skyeye", "SelfHostedLicensing")), "Production store namespace must be isolated.");
        Record("PASS", "isolated DPAPI store round-trip");
        GameIdentity60 identity = Identity("76561198000000001");
        long now = LicenseClock60.Milliseconds;
        LicenseGate60.Revoke();
        Require(!LicenseGate60.Allowed, "Gate must start closed.");
        Require(!LicenseGate60.Grant(100, 161, 200, now, identity), "Lease beyond 60 seconds accepted.");
        Require(!LicenseGate60.Grant(100, 150, 149, now, identity), "Expiry before lease accepted.");
        Require(!LicenseGate60.Grant(100, 100, 200, now, identity), "Empty lease accepted.");
        Require(!LicenseGate60.Grant(100, 160, 200, now - 60000, identity), "Already elapsed lease accepted.");
        Require(LicenseGate60.Grant(100, 160, 253370764800, now, identity), "Valid permanent response short lease rejected.");
        Require(LicenseGate60.Allowed && LicenseGate60.Matches(identity.Pid, identity.Started), "Gate did not bind the current game process.");
        Require(!LicenseGate60.Matches(identity.Pid + 1, identity.Started), "Gate accepted another process.");
        long epoch = LicenseGate60.Epoch;
        LicenseGate60.Revoke();
        Require(!LicenseGate60.Allowed && LicenseGate60.Epoch > epoch, "Revocation must close and invalidate the gate.");
        Require(OverlaySettingsForm.FormatLicenseExpiry60(253370764800, "permanent") == "授权期限：永久", "Permanent UI must not display year 9999.");
        Require(OverlaySettingsForm.FormatLicenseExpiry60(1900000000, "timed").StartsWith("授权到期："), "Timed UI must display its deadline.");
        Record("PASS", "real gate lease limits, identity, revocation and permanent UI formatter");
        Assembly client = typeof(LicenseApi60).Assembly;
        Require(client.GetName().Name == "天穹之眼-自有授权版-v0.60.8", "Own-service output name mismatch.");
        foreach (string resource in new[] { "CoH3.InterfaceIcon", "names-zh-hans.tsv", "CoH3.BarrageCatalogue.tsv", "CoH3.Audio.NAudio.dll" })
            Require(client.GetManifestResourceNames().Contains(resource), "Missing logical resource: " + resource);
        using (Stream stream = client.GetManifestResourceStream("CoH3.Audio.NAudio.dll")) Require(stream.Length > 0, "Embedded NAudio is empty.");
        MethodInfo releaseCapture = typeof(ActivationForm60).GetMethod("ReleaseCapture60", BindingFlags.NonPublic | BindingFlags.Static);
        Require(releaseCapture.GetCustomAttribute<DllImportAttribute>().EntryPoint == "ReleaseCapture", "ReleaseCapture import entry point mismatch.");
        using (LicenseApi60 api = new LicenseApi60())
        {
            bool rejected = false;
            try { api.Call("api/v2/activate", new Dictionary<string, object>(), null, CancellationToken.None).GetAwaiter().GetResult(); }
            catch (ArgumentException) { rejected = true; }
            Require(rejected, "Non-v1 route accepted.");
        }
        Record("PASS", "real assembly, embedded resources, exact PInvoke fix and fixed route allowlist");
    }

    private static async Task Protocol(Uri address, string code, string kind)
    {
        using (LicenseApi60 api = new LicenseApi60(address))
        {
            LicenseReply60 version = await api.Call("api/v1/version", null, null, CancellationToken.None);
            Require(!LicenseVersion60.IsOlder("0.60.8", version.Text("minimum")), "Test server requires a newer client.");
            Dictionary<string, object> payload = Payload(code, Guid.NewGuid().ToString(), LicenseDevice60.GetWindowsDeviceId());
            string token = null;
            try
            {
                LicenseReply60 first = await api.Call("api/v1/activate", payload, null, CancellationToken.None);
                Active(first, kind);
                token = first.Text("session_token");
                Record("PASS", "real API activation and lease response");
                LicenseReply60 retry = await api.Call("api/v1/activate", payload, null, CancellationToken.None);
                Active(retry, kind);
                Require(retry.Text("session_token") == token && retry.Number("lease_until") == first.Number("lease_until"), "Same-request retry must preserve token and lease.");
                Record("PASS", "same-request activation idempotency");
                Dictionary<string, object> conflict = new Dictionary<string, object>(payload) { ["steam_id64"] = "76561198000000002" };
                Require((await api.Call("api/v1/activate", conflict, null, CancellationToken.None)).State == "request_conflict", "Changed payload under the same request ID must conflict.");
                Dictionary<string, object> concurrent = new Dictionary<string, object>(payload) { ["request_id"] = Guid.NewGuid().ToString() };
                Require((await api.Call("api/v1/activate", concurrent, null, CancellationToken.None)).State == "in_use", "Concurrent activation must be rejected.");
                Dictionary<string, object> heartbeat = HeartbeatPayload(payload);
                Active(await api.Call("api/v1/heartbeat", heartbeat, token, CancellationToken.None), kind);
                await api.Call("api/v1/logout", new Dictionary<string, object>(), token, CancellationToken.None);
                token = null;
                Record("PASS", "request conflict, concurrent session, authenticated heartbeat and logout");
                Dictionary<string, object> otherDevice = new Dictionary<string, object>(payload)
                {
                    ["request_id"] = Guid.NewGuid().ToString(),
                    ["device_id"] = new string('A', 64)
                };
                LicenseReply60 denied = await api.Call("api/v1/activate", otherDevice, null, CancellationToken.None);
                Require(new[] { "identity_changed", "revoked", "blocked" }.Contains(denied.State), "Previously bound card must reject a different Windows device.");
                Dictionary<string, object> again = new Dictionary<string, object>(payload) { ["request_id"] = Guid.NewGuid().ToString() };
                LicenseReply60 reactivated = await api.Call("api/v1/activate", again, null, CancellationToken.None);
                Active(reactivated, kind);
                token = reactivated.Text("session_token");
                Require(reactivated.Number("expires_at") == first.Number("expires_at"), "Reactivation must not restart the card lifetime.");
                Record("PASS", "single-device binding and lifetime preserved across logout/reactivation");
            }
            finally
            {
                if (!string.IsNullOrEmpty(token)) await api.Call("api/v1/logout", new Dictionary<string, object>(), token, CancellationToken.None);
            }
        }
    }

    private static async Task Coordinator(Uri address, string code, string kind, string mode, string expectedState, int timeoutSeconds)
    {
        string store = Path.Combine(sandbox, "credentials");
        LicenseCredentials60 credentials = LicenseStore60.Load(store);
        credentials.Code = "";
        credentials.AutoLogin = false;
        FakeIdentity provider = new FakeIdentity();
        ObservedApi api = new ObservedApi(new LicenseApi60(address));
        LicenseView60 latest = null;
        using (LicenseCoordinator60 coordinator = new LicenseCoordinator60(api, provider, credentials, value => LicenseStore60.Save(value, store)))
        {
            coordinator.Changed += view => { Volatile.Write(ref latest, view); Record("STATE", view.State); };
            try
            {
                coordinator.Submit(code, remember: false);
                coordinator.Start();
                await Until(() => Volatile.Read(ref latest)?.State == "active" && LicenseGate60.Allowed, 15000, "real coordinator activation");
                Require(latest.LicenseKind == kind, "Coordinator must propagate license_kind.");
                Record("PASS", "real coordinator active gate and license kind");
                long activatedAt = api.ActivatedAt;
                if (mode == "network")
                {
                    api.Offline = true;
                    await Until(() => api.OfflineAttempts > 0, 36000, "real scheduled heartbeat during network fault");
                    Require(LicenseGate60.Allowed, "Original lease should survive the first network failure before its deadline.");
                    await Until(() => !LicenseGate60.Allowed, 40000, "actual monotonic lease expiry during network failure");
                    Require(LicenseClock60.Milliseconds - activatedAt >= 57000, "Network lease test ended before the original real deadline.");
                    Require(latest.State == "network_wait", "Network failure must report network_wait.");
                    Record("PASS", "real 28-second attempt and real 60-second lease expiry without offline permanent grant");
                    api.Offline = false;
                }
                else if (mode == "observe")
                {
                    Record("READY", "apply external admin change; waiting for " + expectedState);
                    await Until(() => Volatile.Read(ref latest)?.State == expectedState && !LicenseGate60.Allowed, timeoutSeconds * 1000, "external state transition: " + expectedState);
                    Record("PASS", "external state transition closes actual gate: " + expectedState);
                }
                else
                {
                    await Until(() => api.HeartbeatCount > 0, 36000, "real scheduled heartbeat");
                    Require(api.FirstHeartbeatAt - activatedAt >= 27000 && api.FirstHeartbeatAt - activatedAt < 36000, "Heartbeat cadence differs from 28 seconds.");
                    Require(LicenseGate60.Allowed, "Heartbeat must renew the real gate.");
                    Record("PASS", "real 28-second heartbeat, mandatory device_id and disabled collection flags");
                    int beforeResume = api.HeartbeatCount;
                    coordinator.RevalidateAfterResume();
                    Require(!LicenseGate60.Allowed, "Resume must immediately revoke until revalidated.");
                    await Until(() => api.HeartbeatCount > beforeResume && LicenseGate60.Allowed, 10000, "resume heartbeat revalidation");
                    int beforeIdentity = api.LogoutCount;
                    Volatile.Write(ref provider.Current, Identity("76561198000000002"));
                    await Until(() => api.LogoutCount > beforeIdentity && api.ActivationCount > 1 && LicenseGate60.Allowed, 12000, "current-game identity replacement");
                    int beforePause = api.LogoutCount;
                    coordinator.PauseForLogin();
                    Require(!LicenseGate60.Allowed, "Pause must immediately close the gate.");
                    await Until(() => api.LogoutCount > beforePause, 10000, "pause logout");
                    Record("PASS", "resume revalidation, current-game identity change and pause/logout");
                }
            }
            finally
            {
                api.Offline = false;
                await coordinator.CloseAsync();
                Require(!LicenseGate60.Allowed, "Close must revoke the real gate.");
            }
        }
        LicenseCredentials60 stored = LicenseStore60.Load(store);
        Require(!stored.Remember && stored.Code == "", "Remember=false must not persist a test card.");
    }

    private static Dictionary<string, object> Payload(string code, string requestId, string device)
    {
        return new Dictionary<string, object>
        {
            ["code"] = code, ["device_id"] = device, ["steam_id64"] = "76561198000000001",
            ["installation_id"] = Guid.NewGuid().ToString(), ["client_version"] = "0.60.8",
            ["request_id"] = requestId, ["game_name"] = "Sandbox Current Game"
        };
    }

    private static Dictionary<string, object> HeartbeatPayload(Dictionary<string, object> activation)
    {
        return activation.Where(pair => !new[] { "code", "request_id", "game_name" }.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private static void Active(LicenseReply60 reply, string kind)
    {
        Require(reply.State == "active", "Expected active; received " + reply.State);
        Require(reply.SignatureVerified, "Active API reply must carry the verified signature marker.");
        Require(Regex.IsMatch(reply.Text("session_token"), "\\A[0-9a-fA-F]{64}\\z"), "Session token format mismatch.");
        long remaining = Math.Min(reply.Number("lease_until"), reply.Number("expires_at")) - reply.Number("server_time");
        Require(remaining > 0 && remaining <= 60 && reply.Number("expires_at") >= reply.Number("lease_until"), "Lease response violates the short-lease contract.");
        Require(reply.Text("license_kind") == kind, "License kind mismatch.");
    }

    private static GameIdentity60 Identity(string steamId) => new GameIdentity60 { Pid = 424242, Started = new DateTime(2026, 11, 13, 0, 0, 0, DateTimeKind.Utc), SteamId = steamId, Name = "Sandbox Current Game" };

    private static async Task Until(Func<bool> condition, int milliseconds, string label)
    {
        Stopwatch wait = Stopwatch.StartNew();
        while (!condition())
        {
            if (wait.ElapsedMilliseconds > milliseconds) throw new Exception("Timed out: " + label);
            await Task.Delay(100);
        }
    }

    private static Dictionary<string, string> Parse(string[] args)
    {
        Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i += 2)
        {
            Require(args[i].StartsWith("--") && i + 1 < args.Length, "Arguments must use --name value pairs.");
            result.Add(args[i].Substring(2), args[i + 1]);
        }
        return result;
    }
    private static string Value(Dictionary<string, string> options, string name, string fallback) => options.TryGetValue(name, out string value) ? value : fallback;
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    private static void Record(string type, string detail)
    {
        lock (events)
        {
            events.Add(new { elapsedMs = elapsed.ElapsedMilliseconds, type, detail });
            Console.WriteLine(type + " [" + elapsed.ElapsedMilliseconds + "ms] " + detail);
        }
    }
    private static void SaveReport(bool passed)
    {
        if (sandbox == null || !Directory.Exists(sandbox)) return;
        lock (events) File.WriteAllText(Path.Combine(sandbox, "result.json"), new JavaScriptSerializer().Serialize(new { passed, clientAssembly = typeof(LicenseApi60).Assembly.GetName().Name, process64Bit = Environment.Is64BitProcess, elapsedMs = elapsed.ElapsedMilliseconds, events }));
    }

    private sealed class FakeIdentity : IGameIdentityProvider60
    {
        public GameIdentity60 Current = Identity("76561198000000001");
        public GameIdentity60 Read() => Volatile.Read(ref Current);
    }

    private sealed class ObservedApi : ILicenseApi60
    {
        private readonly LicenseApi60 actual;
        public volatile bool Offline;
        public int OfflineAttempts;
        public int HeartbeatCount;
        public int ActivationCount;
        public int LogoutCount;
        public long ActivatedAt;
        public long FirstHeartbeatAt;
        public ObservedApi(LicenseApi60 actual) { this.actual = actual; }
        public async Task<LicenseReply60> Call(string path, Dictionary<string, object> payload, string token, CancellationToken cancel)
        {
            if (Offline) { Interlocked.Increment(ref OfflineAttempts); throw new HttpRequestException("Verifier transport fault."); }
            if (path == "api/v1/activate" || path == "api/v1/heartbeat")
            {
                string[] expected = path.EndsWith("activate") ? new[] { "client_version", "code", "device_id", "game_name", "installation_id", "request_id", "steam_id64" } : new[] { "client_version", "device_id", "installation_id", "steam_id64" };
                Require(payload.Keys.OrderBy(key => key).SequenceEqual(expected.OrderBy(key => key)), "Unexpected identity payload fields; extra account collection must remain disabled.");
                Require((string)payload["device_id"] == LicenseDevice60.GetWindowsDeviceId(), "Coordinator must always send the real product device hash.");
                Record("HTTP", path + " keys=" + string.Join(",", payload.Keys.OrderBy(key => key)));
            }
            LicenseReply60 reply = await actual.Call(path, payload, token, cancel);
            if (path == "api/v1/version")
            {
                // Only capability flags are forced on; HTTP still reaches the real server.
                return new LicenseReply60(new Dictionary<string, object>
                {
                    ["minimum"] = reply.Text("minimum"), ["latest"] = reply.Text("latest"),
                    ["download_url"] = reply.Text("download_url"), ["notes"] = reply.Text("notes"),
                    ["connection_info"] = 1, ["steam_accounts_info"] = 1, ["qq_accounts_info"] = 1
                }, reply.RequestStarted);
            }
            if (path == "api/v1/activate")
            {
                if (reply.State == "active") Interlocked.CompareExchange(ref ActivatedAt, reply.RequestStarted, 0);
                Interlocked.Increment(ref ActivationCount);
            }
            if (path == "api/v1/heartbeat")
            {
                Interlocked.CompareExchange(ref FirstHeartbeatAt, reply.RequestStarted, 0);
                Interlocked.Increment(ref HeartbeatCount);
            }
            if (path == "api/v1/logout") Interlocked.Increment(ref LogoutCount);
            Record("REPLY", path + " state=" + reply.State);
            return reply;
        }
        public void Dispose() => actual.Dispose();
    }
}
