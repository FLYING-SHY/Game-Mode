using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CoH3.Licensing;

internal static class ProtectedClientTests
{
    private static readonly List<string> Checks = new();

    private static async Task<int> Main(string[] args)
    {
        bool jsonOnly = args.Length > 0 && args[0] == "--json-only";
        if ((!jsonOnly && args.Length != 3) || (jsonOnly && args.Length != 2))
            throw new ArgumentException("Use --json-only OUTPUT, or PUBLIC_CONFIG CODES_FILE OUTPUT.");
        string output = Path.GetFullPath(args[^1]);
        Directory.CreateDirectory(output);
        Exception? failure = null;
        try
        {
            await DecodeErrorResponse(output);
            if (!jsonOnly) await CheckActualService(args[0], args[1], output);
        }
        catch (Exception ex) { failure = ex; }
        var assemblyPath = typeof(LicenseClient).Assembly.Location;
        var report = new
        {
            Passed = failure is null,
            Checks,
            Failure = failure?.ToString(),
            Assembly = assemblyPath,
            AssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assemblyPath))),
            Scope = jsonOnly ? "Actual protected client; synthetic JSON error response; no service or game access."
                : "Actual protected client and local Keygen service; injected device IDs; no game or physical second computer.",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        File.WriteAllText(Path.Combine(output, "protected-client-tests.json"),
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine(JsonSerializer.Serialize(report));
        return failure is null ? 0 : 1;
    }

    private static async Task DecodeErrorResponse(string output)
    {
        using var http = new HttpClient(new JsonErrorHandler());
        using var client = new LicenseClient(new LicenseClientOptions
        {
            BaseUri = new Uri("http://127.0.0.1:1/"),
            VerificationPublicKeyPem = "not-used-for-an-error-response",
            LicenseFilePath = Path.Combine(output, "json-error.lic")
        }, http, () => "JSON-FIXTURE-DEVICE");
        var result = await client.ActivateAsync("SYNTHETIC-ERROR-ONLY");
        Check("protected JSON response keeps the original error code and message",
            !result.CanEnable && result.Code == "InvalidCode" && result.Message == "fixture error response");
    }

    private static async Task CheckActualService(string configuration, string codesFile, string output)
    {
        using var deployment = JsonDocument.Parse(File.ReadAllText(configuration));
        using var codes = JsonDocument.Parse(File.ReadAllText(codesFile));
        string publicKey = deployment.RootElement.GetProperty("verificationPublicKeyPem").GetString()!;
        var endpoint = new Uri(deployment.RootElement.GetProperty("serviceUri").GetString()!);
        string identity = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("PROTECTED-" + Guid.NewGuid().ToString("N"))));
        string otherIdentity = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity + "-OTHER")));
        string permanentFile = Path.Combine(output, "permanent.lic");
        string timedFile = Path.Combine(output, "timed.lic");
        LicenseClientOptions Options(string file, string? key = null) => new()
        { BaseUri = endpoint, VerificationPublicKeyPem = key ?? publicKey, LicenseFilePath = file };
        using var onlineHandler = new CountingHandler();
        using var online = new HttpClient(onlineHandler) { Timeout = TimeSpan.FromSeconds(30) };
        using (var client = new LicenseClient(Options(permanentFile), online, () => identity))
        {
            var activated = await client.ActivateAsync(codes.RootElement.GetProperty("permanentCode").GetString()!);
            Check("protected client activates a permanent license through the actual service",
                activated.CanEnable && activated.Kind == LicenseKind.Permanent);
            string certificate = File.ReadAllText(permanentFile);
            var recovered = await client.ActivateAsync(codes.RootElement.GetProperty("permanentCode").GetString()!);
            Check("same-device retry recovers the exact signed certificate",
                recovered.CanEnable && File.ReadAllText(permanentFile) == certificate);
        }
        using var deniedHandler = new DenyHandler();
        using var offline = new HttpClient(deniedHandler);
        using (var client = new LicenseClient(Options(permanentFile), offline, () => identity))
        {
            var result = await client.ValidateStartupAsync();
            Check("permanent startup verifies the protected client license with zero HTTP requests",
                result.CanEnable && result.Kind == LicenseKind.Permanent && deniedHandler.Requests == 0);
        }
        using (var copied = new LicenseClient(Options(permanentFile), offline, () => otherIdentity))
        {
            var result = await copied.ValidateStartupAsync();
            Check("copying the signed license to a different injected device fails locally",
                !result.CanEnable && result.Code == "DeviceMismatch" && deniedHandler.Requests == 0);
        }
        using (var other = new LicenseClient(Options(Path.Combine(output, "other.lic")), online, () => otherIdentity))
        {
            var result = await other.ActivateAsync(codes.RootElement.GetProperty("permanentCode").GetString()!);
            Check("a redeemed code does not activate a second injected device", !result.CanEnable && result.Code == "AlreadyRedeemed");
        }
        using (var wrongKey = RSA.Create(2048))
        using (var changedRoot = new LicenseClient(Options(permanentFile, wrongKey.ExportSubjectPublicKeyInfoPem()), offline, () => identity))
        {
            var result = await changedRoot.ValidateStartupAsync();
            Check("a different verification key does not validate the signed license",
                !result.CanEnable && result.Code == "InvalidLicense" && deniedHandler.Requests == 0);
        }
        var clock = new SkewedClock();
        using (var client = new LicenseClient(Options(timedFile), online, () => identity, clock))
        {
            var result = await client.ActivateAsync(codes.RootElement.GetProperty("timedCode").GetString()!);
            Check("protected timed activation uses signed server time despite a skewed local wall clock",
                result.CanEnable && result.Kind == LicenseKind.Timed && result.ExpiresAtUtc is not null);
            int requests = onlineHandler.Requests;
            clock.Advance(TimeSpan.FromDays(3650));
            var expired = client.CheckCurrentLicense();
            Check("protected timed expiry uses elapsed time without periodic HTTP requests",
                !expired.CanEnable && expired.Code == "Expired" && onlineHandler.Requests == requests);
        }
        using (var fresh = new LicenseClient(Options(timedFile), online, () => identity))
        {
            int before = onlineHandler.Requests;
            var result = await fresh.ValidateStartupAsync();
            Check("each new timed startup contacts the actual service", result.CanEnable && onlineHandler.Requests == before + 1);
        }
        using (var disconnected = new LicenseClient(Options(timedFile), offline, () => identity))
        {
            var result = await disconnected.ValidateStartupAsync();
            Check("timed startup fails closed while disconnected without deleting the license",
                !result.CanEnable && result.Code == "NetworkUnavailable" && deniedHandler.Requests == 1
                && File.Exists(timedFile) && !disconnected.CheckCurrentLicense().CanEnable);
        }
    }

    private static void Check(string name, bool condition)
    {
        if (!condition) throw new InvalidOperationException(name);
        Checks.Add(name);
    }

    private sealed class JsonErrorHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            { Content = new StringContent("{\"code\":\"InvalidCode\",\"message\":\"fixture error response\",\"certificate\":null}", Encoding.UTF8, "application/json") });
    }
    private sealed class CountingHandler : DelegatingHandler
    {
        internal int Requests;
        internal CountingHandler() : base(new SocketsHttpHandler()) { }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        { Requests++; return base.SendAsync(request, token); }
    }
    private sealed class DenyHandler : HttpMessageHandler
    {
        internal int Requests;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        { Requests++; throw new HttpRequestException("Deliberate offline transport."); }
    }
    private sealed class SkewedClock : TimeProvider
    {
        private long ticks;
        public override long TimestampFrequency => TimeSpan.TicksPerSecond;
        public override long GetTimestamp() => ticks;
        public override DateTimeOffset GetUtcNow() => new(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);
        internal void Advance(TimeSpan duration) => ticks += duration.Ticks;
    }
}
