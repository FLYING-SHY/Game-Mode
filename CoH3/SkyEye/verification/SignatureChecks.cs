using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CoH3MemoryTool;

internal static class SignatureChecks
{
    private static void Require(bool value, string label) { if (!value) throw new Exception(label); Console.WriteLine("PASS " + label); }
    internal static void Run()
    {
        using (var rsa = new RSACng(3072))
        {
            RSAParameters publicKey = rsa.ExportParameters(false);
            string modulus = Convert.ToBase64String(publicKey.Modulus), exponent = Convert.ToBase64String(publicKey.Exponent);
            string kid = LeaseSignature60.Hash(publicKey.Modulus).Substring(0, 16);
            var request = new Dictionary<string, object> { ["device_id"] = new string('a', 64), ["steam_id64"] = "76561198000000001", ["installation_id"] = Guid.NewGuid().ToString("D"), ["nonce"] = LeaseSignature60.NewNonce() };
            string token = new string('b', 64);
            var body = new Dictionary<string, object> { ["state"] = "active", ["server_time"] = 100L, ["lease_until"] = 160L, ["expires_at"] = 1000L, ["license_kind"] = "timed", ["session_token"] = token, ["lease_kid"] = kid };
            string canonical = string.Join("\n", new[] { "skyeye-lease-v2", LicenseConfiguration60.ProductId, "active", ((string)request["device_id"]).ToUpperInvariant(), (string)request["steam_id64"], (string)request["installation_id"], (string)request["nonce"], LeaseSignature60.Hash(Encoding.UTF8.GetBytes(token)), "100", "160", "1000", "timed", kid });
            Require(canonical.Split('\n').Length == 13 && !canonical.EndsWith("\n"), "13 canonical LF lines without trailing LF");
            body["lease_signature"] = Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(canonical), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            var context = new LeaseSignature60(request, null, true);
            Func<Dictionary<string, object>, bool> verify = values => context.Verify(new LicenseReply60(values, 0), modulus, exponent, kid);
            Require(verify(body), "RSA3072 SHA256 PKCS1 activation signature");
            var heartbeat = new Dictionary<string, object>(body); heartbeat.Remove("session_token");
            Require(new LeaseSignature60(request, token, false).Verify(new LicenseReply60(heartbeat, 0), modulus, exponent, kid), "heartbeat effective Bearer token fallback");
            Require(!new LeaseSignature60(request, new string('c', 64), false).Verify(new LicenseReply60(heartbeat, 0), modulus, exponent, kid), "reject altered heartbeat Bearer token");
            var otherProduct = new Dictionary<string, object>(body) { ["lease_signature"] = Convert.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(canonical.Replace("\n" + LicenseConfiguration60.ProductId + "\n", "\nOtherProduct\n")), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)) };
            Require(!verify(otherProduct), "reject signature for different product");
            Require(!context.Verify(new LicenseReply60(heartbeat, 0), modulus, exponent, kid), "activation requires response token");
            foreach (var change in new Dictionary<string, object> { ["lease_signature"] = "", ["lease_kid"] = new string('0', 16), ["server_time"] = 101L, ["lease_until"] = 159L, ["expires_at"] = 1001L, ["license_kind"] = "permanent", ["session_token"] = new string('c', 64) })
            {
                var altered = new Dictionary<string, object>(body) { [change.Key] = change.Value };
                Require(!verify(altered), "reject altered " + change.Key);
            }
            var flipped = new Dictionary<string, object>(body);
            byte[] bad = Convert.FromBase64String((string)body["lease_signature"]); bad[20] ^= 1;
            flipped["lease_signature"] = Convert.ToBase64String(bad);
            Require(!verify(flipped), "reject corrupt signature bytes");
            foreach (var change in new Dictionary<string, object> { ["device_id"] = new string('d', 64), ["steam_id64"] = "76561198000000002", ["installation_id"] = Guid.NewGuid().ToString("D"), ["nonce"] = LeaseSignature60.NewNonce() })
            {
                var altered = new Dictionary<string, object>(request) { [change.Key] = change.Value };
                Require(!new LeaseSignature60(altered, null, true).Verify(new LicenseReply60(body, 0), modulus, exponent, kid), "reject changed request " + change.Key);
            }
            using (var other = new RSACng(3072))
            {
                var otherPublic = other.ExportParameters(false);
                Require(!context.Verify(new LicenseReply60(body, 0), Convert.ToBase64String(otherPublic.Modulus), exponent, kid), "reject wrong RSA key");
                var wrongSigned = new Dictionary<string, object>(body) { ["lease_signature"] = Convert.ToBase64String(other.SignData(Encoding.UTF8.GetBytes(canonical), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)) };
                Require(!verify(wrongSigned), "reject signature made by wrong key");
            }
            Require(!context.Verify(new LicenseReply60(body, 0), "", "", ""), "empty production pin fails closed");
            Require(!new LicenseReply60(body, 0).SignatureVerified, "constructed reply never receives verification marker");
            var copied = new LicenseReply60(body, 0); body["lease_until"] = 999L;
            Require(copied.Number("lease_until") == 160, "reply fields are immutable snapshots");
        }
        RealSend().GetAwaiter().GetResult();
        CoordinatorMarker().GetAwaiter().GetResult();
    }

    private sealed class SyntheticIdentity : IGameIdentityProvider60
    {
        public GameIdentity60 Read() => new GameIdentity60 { Pid = 424242, Started = new DateTime(2026, 11, 13, 0, 0, 0, DateTimeKind.Utc), SteamId = "76561198000000001", Name = "Signature Test" };
    }

    private sealed class UnsignedApi : ILicenseApi60
    {
        public Task<LicenseReply60> Call(string path, Dictionary<string, object> payload, string token, CancellationToken cancel)
        {
            var body = path.EndsWith("version") ? new Dictionary<string, object> { ["minimum"] = "0.60.8" } : new Dictionary<string, object> { ["state"] = "active", ["session_token"] = new string('b', 64), ["server_time"] = 100L, ["lease_until"] = 160L, ["expires_at"] = 1000L, ["license_kind"] = "timed" };
            return Task.FromResult(new LicenseReply60(body, LicenseClock60.Milliseconds));
        }
        public void Dispose() { }
    }

    private static async Task CoordinatorMarker()
    {
        LicenseGate60.Revoke();
        var observed = new TaskCompletionSource<bool>();
        using (var coordinator = new LicenseCoordinator60(new UnsignedApi(), new SyntheticIdentity(), new LicenseCredentials60 { Installation = Guid.NewGuid().ToString("D") }, value => { }))
        {
            coordinator.Changed += view => { if (view.State == "active") observed.TrySetResult(false); if (view.State == "network_wait") observed.TrySetResult(true); };
            coordinator.Submit("SYNTHETIC-TEST-ONLY", false); coordinator.Start();
            try
            {
                Task completed = await Task.WhenAny(observed.Task, Task.Delay(5000));
                Require(completed == observed.Task && await observed.Task && !LicenseGate60.Allowed, "real coordinator rejects unverified active before GateGrant");
            }
            finally { await coordinator.CloseAsync(); }
        }
    }

    private static async Task RealSend()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0); probe.Start(); int port = ((IPEndPoint)probe.LocalEndpoint).Port; probe.Stop();
        using (var server = new HttpListener())
        {
            string address = "http://127.0.0.1:" + port.ToString(CultureInfo.InvariantCulture) + "/";
            server.Prefixes.Add(address); server.Start();
            string previous = null;
            Task responses = Task.Run(async () => {
                for (int i = 0; i < 2; i++)
                {
                    var incoming = await server.GetContextAsync();
                    string text; using (var reader = new System.IO.StreamReader(incoming.Request.InputStream)) text = await reader.ReadToEndAsync();
                    var data = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(text);
                    string nonce = (string)data["nonce"];
                    Require(System.Text.RegularExpressions.Regex.IsMatch(nonce, "\\A[0-9a-f]{32}\\z") && nonce != previous, "real HTTP send fresh lowercase nonce"); previous = nonce;
                    byte[] bytes = Encoding.UTF8.GetBytes("{\"state\":\"active\",\"session_token\":\"" + new string('b', 64) + "\"}");
                    incoming.Response.ContentType = "application/json"; incoming.Response.ContentLength64 = bytes.Length;
                    await incoming.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length); incoming.Response.Close();
                }
            });
            using (var api = new LicenseApi60(new Uri(address)))
            {
                var payload = new Dictionary<string, object> { ["device_id"] = new string('A', 64), ["steam_id64"] = "76561198000000001", ["installation_id"] = Guid.NewGuid().ToString("D"), ["request_id"] = Guid.NewGuid().ToString("D") };
                foreach (string route in new[] { "api/v1/activate", "api/v1/heartbeat" })
                {
                    bool rejected = false;
                    try { await api.Call(route, payload, new string('b', 64), CancellationToken.None); } catch (HttpRequestException) { rejected = true; }
                    Require(rejected, "real HTTP unsigned active rejected " + route);
                    Require(!payload.ContainsKey("nonce"), "send does not mutate stable request payload");
                }
            }
            await responses;
        }
    }
}
