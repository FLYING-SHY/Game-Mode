using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using CoH3.Licensing;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;

namespace CoH3.Licensing.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var config = Environment.GetEnvironmentVariable("COH3_LICENSING_CONFIG")
            ?? Path.Combine(AppContext.BaseDirectory, "server.local.json");
        var app = Build(ServerOptions.Load(config), args);
        await app.RunAsync();
    }

    public static WebApplication Build(ServerOptions options, string[]? args = null)
    {
        options.Validate();
        var builder = WebApplication.CreateBuilder(args ?? []);
        builder.WebHost.UseUrls(options.ListenUrl);
        builder.WebHost.ConfigureKestrel(server => server.Limits.MaxRequestBodySize = 256 * 1024);
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(20) });
        builder.Services.AddSingleton<KeygenGateway>();
        builder.Services.AddSingleton<LicenseService>();
        var cookieKeys = builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(options.DatabasePath))!, "admin-keys")));
        if (OperatingSystem.IsWindows()) cookieKeys.ProtectKeysWithDpapi();
        builder.Services.AddAuthentication("admin").AddCookie("admin", cookie =>
        {
            cookie.Cookie.Name = "CoH3.Licensing.Admin";
            cookie.Cookie.HttpOnly = true;
            cookie.Cookie.SameSite = SameSiteMode.Strict;
            cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            cookie.ExpireTimeSpan = TimeSpan.FromHours(4);
            cookie.SlidingExpiration = false;
            cookie.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api")) context.Response.StatusCode = 401;
                else context.Response.Redirect("/admin/login");
                return Task.CompletedTask;
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddAntiforgery(settings =>
        {
            settings.HeaderName = "X-CSRF-TOKEN";
            settings.Cookie.SameSite = SameSiteMode.Strict;
            settings.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
        builder.Services.AddRateLimiter(limits =>
        {
            limits.RejectionStatusCode = 429;
            limits.AddPolicy("public", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "local",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            limits.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "local",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
        var app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; script-src 'self'; style-src 'self'; connect-src 'self'; form-action 'self'; frame-ancestors 'none'; base-uri 'none'";
            // Defeat DNS rebinding on the local administration listener.
            if (!IPAddress.TryParse(context.Request.Host.Host, out var host) || !IPAddress.IsLoopback(host))
            { context.Response.StatusCode = 400; return; }
            try { await next(); }
            catch (LicensingException ex) { await Error(context, ex.Status, ex.Code, ex.Message); }
            catch (KeygenApiException ex)
            {
                var code = ex.Code switch
                {
                    "SUSPENDED" => "Suspended", "EXPIRED" => "Expired",
                    "FINGERPRINT_SCOPE_MISMATCH" or "NO_MACHINE" => "DeviceMismatch",
                    _ => "BackendUnavailable"
                };
                await Error(context, code == "BackendUnavailable" ? 503 : 403, code,
                    code switch { "Suspended" => "限时授权已被停用。", "Expired" => "限时授权已到期。",
                        "DeviceMismatch" => "设备绑定验证失败。", _ => "授权框架返回异常，请联系管理员。" });
                app.Logger.LogWarning("Keygen request failed ({Code}, HTTP {Status})", ex.Code, ex.Status);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            { await Error(context, 503, "BackendUnavailable", "授权服务暂时连接不到 Keygen；请稍后重试。"); }
            catch (Exception ex) when (ex is CryptographicException or FormatException or System.Text.Json.JsonException
                or KeyNotFoundException or InvalidOperationException or DeviceMismatchException)
            {
                app.Logger.LogWarning("License integrity/configuration check failed: {Type}", ex.GetType().Name);
                await Error(context, 403, "InvalidLicense", "许可证、设备或后端配置验证失败。");
            }
            catch (SqliteException ex)
            {
                app.Logger.LogWarning("License database operation failed ({Code})", ex.SqliteErrorCode);
                await Error(context, 503, "StorageUnavailable", "授权记录存储暂时繁忙，请重试。");
            }
        });
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/health", async (KeygenGateway keygen, CancellationToken ct) =>
        {
            await keygen.ProbeAsync(ct);
            return Results.Json(new { status = "ready", framework = "Keygen CE", mode = "real-api" });
        });
        app.MapPost("/api/licenses/activate", async (ActivationRequest request, LicenseService service, CancellationToken ct)
            => Results.Json(await service.ActivateAsync(request, ct))).RequireRateLimiting("public");
        app.MapPost("/api/licenses/validate", async (ValidationRequest request, LicenseService service, CancellationToken ct)
            => Results.Json(await service.ValidateAsync(request, ct))).RequireRateLimiting("public");

        app.MapGet("/", () => Results.Redirect("/admin/login"));
        app.MapGet("/admin/login", (HttpContext context, IAntiforgery csrf) =>
        {
            var token = WebUtility.HtmlEncode(csrf.GetAndStoreTokens(context).RequestToken!);
            return Results.Content("<!doctype html><html lang=zh-CN><meta charset=utf-8><title>授权管理员登录</title>"
                + "<h1>授权管理员登录</h1><form method=post action=/admin/login>"
                + "<label>管理员令牌 <input type=password name=token required autocomplete=current-password></label>"
                + $"<input type=hidden name=__RequestVerificationToken value=\"{token}\"><button>登录</button></form></html>", "text/html; charset=utf-8");
        });
        app.MapPost("/admin/login", async (HttpContext context, IAntiforgery csrf) =>
        {
            try { await csrf.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { return Results.StatusCode(400); }
            var form = await context.Request.ReadFormAsync();
            var actual = SHA256.HashData(Encoding.UTF8.GetBytes(form["token"].ToString()));
            var expected = SHA256.HashData(Encoding.UTF8.GetBytes(options.AdminToken));
            if (!CryptographicOperations.FixedTimeEquals(actual, expected)) return Results.Unauthorized();
            await context.SignInAsync("admin", new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "owner")], "admin")));
            return Results.Redirect("/admin");
        }).RequireRateLimiting("login");
        app.MapGet("/admin", (HttpContext context, IAntiforgery csrf) => Results.Content(
            Asset("admin.html").Replace("__CSRF__", WebUtility.HtmlEncode(csrf.GetAndStoreTokens(context).RequestToken!)),
            "text/html; charset=utf-8")).RequireAuthorization();
        app.MapGet("/admin/app.js", () => Results.Content(Asset("admin.js"), "text/javascript; charset=utf-8")).RequireAuthorization();
        app.MapGet("/admin/app.css", () => Results.Content(Asset("admin.css"), "text/css; charset=utf-8")).RequireAuthorization();
        var admin = app.MapGroup("/api/admin").RequireAuthorization();
        admin.AddEndpointFilter(async (invocation, next) =>
        {
            if (HttpMethods.IsPost(invocation.HttpContext.Request.Method))
            {
                try { await invocation.HttpContext.RequestServices.GetRequiredService<IAntiforgery>()
                    .ValidateRequestAsync(invocation.HttpContext); }
                catch (AntiforgeryValidationException) { return Results.StatusCode(400); }
            }
            return await next(invocation);
        });
        admin.MapGet("/codes", (LicenseService service) => Results.Json(service.List()));
        admin.MapPost("/codes", async (IssueRequest request, LicenseService service, CancellationToken ct)
            => Results.Json(await service.IssueAsync(request, ct)));
        admin.MapPost("/codes/{id}/suspension", async (string id, SuspensionRequest request, LicenseService service, CancellationToken ct) =>
        { await service.SuspendAsync(id, request.Suspended, ct); return Results.Ok(); });
        admin.MapPost("/logout", async (HttpContext context) => { await context.SignOutAsync("admin"); return Results.Ok(); });
        return app;
    }

    private static string Asset(string name)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("CoH3.Licensing.Server." + name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    private static async Task Error(HttpContext context, int status, string code, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new LicenseApiResponse(code, message));
    }
    public sealed record SuspensionRequest(bool Suspended);
}
