using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;

namespace Licensing.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var config = Environment.GetEnvironmentVariable("LICENSING_SERVER_CONFIG")
            ?? Path.Combine(AppContext.BaseDirectory, "server.local.json");
        var options = ServerOptions.Load(config);
        if (args.Length > 0 && args[0] == "--export-lease-public-key")
        {
            if (args.Length != 2) throw new ArgumentException("Usage: --export-lease-public-key OUTPUT_JSON");
            var output = Path.GetFullPath(args[1]);
            var keyDirectory = Path.GetFullPath(options.KeyDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (output.Equals(keyDirectory, StringComparison.OrdinalIgnoreCase)
                || output.StartsWith(keyDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || output.Equals(Path.GetFullPath(options.DatabasePath), StringComparison.OrdinalIgnoreCase)
                || output.Equals(Path.GetFullPath(config), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Public export must use a separate output path.");
            using var signer = new LeaseSigner(options);
            using var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await System.Text.Json.JsonSerializer.SerializeAsync(stream, signer.ExportPublicKey(),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            return;
        }
        await Build(options, args).RunAsync();
    }

    public static WebApplication Build(ServerOptions options, string[]? args = null, TimeProvider? timeProvider = null)
    {
        options.Validate();
        var builder = WebApplication.CreateBuilder(args ?? []);
        builder.WebHost.UseUrls(options.ListenUrl);
        builder.WebHost.ConfigureKestrel(server => server.Limits.MaxRequestBodySize = 64 * 1024);
        // Hosting diagnostics include query strings, which may contain an administrator's code search.
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.HttpLogging", LogLevel.Warning);
        builder.Services.Configure<RouteHandlerOptions>(routing => routing.ThrowOnBadRequest = true);
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<TimeProvider>(timeProvider ?? TimeProvider.System);
        builder.Services.AddSingleton<LeaseSigner>();
        builder.Services.AddSingleton<LicenseService>();
        Directory.CreateDirectory(Path.GetFullPath(options.KeyDirectory));
        var protection = builder.Services.AddDataProtection()
            .SetApplicationName("Licensing.Native.v1")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(options.KeyDirectory)));
        if (OperatingSystem.IsWindows()) protection.ProtectKeysWithDpapi();
        builder.Services.AddAuthentication("admin").AddCookie("admin", cookie =>
        {
            cookie.Cookie.Name = "Licensing.Admin";
            cookie.Cookie.HttpOnly = true;
            cookie.Cookie.SameSite = SameSiteMode.Strict;
            cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            cookie.ExpireTimeSpan = TimeSpan.FromHours(4);
            cookie.SlidingExpiration = false;
            cookie.Events.OnRedirectToLogin = async context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "unauthenticated", message = "请先登录管理后台。" });
                }
                else context.Response.Redirect("/admin/login");
            };
            cookie.Events.OnRedirectToAccessDenied = async context =>
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { error = "forbidden", message = "管理员认证失败。" });
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddAntiforgery(settings =>
        {
            settings.HeaderName = "X-CSRF-TOKEN";
            settings.Cookie.Name = "Licensing.Csrf";
            settings.Cookie.HttpOnly = true;
            settings.Cookie.SameSite = SameSiteMode.Strict;
            settings.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
        builder.Services.AddRateLimiter(limits =>
        {
            limits.RejectionStatusCode = 429;
            limits.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "local",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
        });
        // Prevent ambient ASPNETCORE_FORWARDEDHEADERS_ENABLED from enabling blanket trust.
        builder.Configuration["ForwardedHeaders_Enabled"] = "false";
        var app = builder.Build();
        if (options.TrustedProxies.Length > 0)
        {
            var forwarded = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                ForwardLimit = options.ForwardLimit
            };
            forwarded.KnownProxies.Clear();
            forwarded.KnownIPNetworks.Clear();
            foreach (var proxy in options.TrustedProxies) forwarded.KnownProxies.Add(IPAddress.Parse(proxy));
            app.UseForwardedHeaders(forwarded);
        }
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers.XContentTypeOptions = "nosniff";
            context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; script-src 'self'; style-src 'self'; connect-src 'self'; form-action 'self'; frame-ancestors 'none'; base-uri 'none'";
            if (!options.AllowedHosts.Contains(context.Request.Host.Host.Trim('[', ']'), StringComparer.OrdinalIgnoreCase))
            {
                await Error(context, 400, "invalid_host", "主机名未配置。");
                return;
            }
            var remote = context.Connection.RemoteIpAddress;
            if (!context.Request.IsHttps && remote is not null && !IPAddress.IsLoopback(remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4() : remote))
            {
                await Error(context, 400, "https_required", "远程连接必须使用 HTTPS。");
                return;
            }
            try { await next(); }
            catch (LicensingException ex) { await Error(context, ex.Status, ex.Code, ex.Message); }
            catch (BadHttpRequestException)
            { await Error(context, 400, "invalid_request", "请求格式无效。"); }
            catch (SqliteException ex)
            {
                app.Logger.LogWarning("License storage operation failed ({Code})", ex.SqliteErrorCode);
                await Error(context, 503, "storage_unavailable", "授权记录存储暂时繁忙，请重试。");
            }
            catch (Exception ex) when (ex is CryptographicException or FormatException or System.Text.Json.JsonException)
            {
                app.Logger.LogWarning("License data integrity check failed ({Type})", ex.GetType().Name);
                await Error(context, 503, "integrity_error", "授权记录完整性校验失败，请联系管理员。");
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
            catch (Exception ex)
            {
                app.Logger.LogError("License request failed ({Type})", ex.GetType().Name);
                await Error(context, 500, "internal_error", "服务处理异常，请稍后重试。");
            }
        });
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/health", (LicenseService service) =>
        {
            service.Probe();
            return Results.Json(new { status = "ready", framework = "ASP.NET Core", mode = "native-leases" });
        });
        app.MapGet("/api/v1/version", (LicenseService service) => Results.Json(service.VersionInfo()));
        app.MapPost("/api/v1/activate", (ActivationRequest request, LicenseService service) => Results.Json(service.Activate(request)));
        app.MapPost("/api/v1/heartbeat", (HeartbeatRequest request, HttpContext context, LicenseService service) =>
            Results.Json(service.Heartbeat(Bearer(context), request)));
        app.MapPost("/api/v1/logout", (HttpContext context, LicenseService service) =>
        {
            service.Logout(Bearer(context));
            return Results.Json(new { ok = true });
        });

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
            catch (AntiforgeryValidationException) { return Results.Json(new { error = "invalid_csrf", message = "请求验证失败。" }, statusCode: 400); }
            if (!context.Request.HasFormContentType)
                return Results.Json(new { error = "invalid_request", message = "登录请求必须使用表单。" }, statusCode: 400);
            var form = await context.Request.ReadFormAsync();
            var actual = SHA256.HashData(Encoding.UTF8.GetBytes(form["token"].ToString()));
            var expected = SHA256.HashData(Encoding.UTF8.GetBytes(options.AdminToken));
            if (!CryptographicOperations.FixedTimeEquals(actual, expected))
                return Results.Json(new { error = "invalid_credentials", message = "管理员令牌错误。" }, statusCode: 401);
            await context.SignInAsync("admin", new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "owner")], "admin")));
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
                try { await invocation.HttpContext.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(invocation.HttpContext); }
                catch (AntiforgeryValidationException)
                { return Results.Json(new { error = "invalid_csrf", message = "请求验证失败，请刷新管理页。" }, statusCode: 400); }
            }
            return await next(invocation);
        });
        admin.MapGet("/settings", () => Results.Json(new
        {
            productId = options.ProductId, productName = options.ProductName, codePrefix = options.CodePrefix.ToUpperInvariant(),
            leaseSeconds = LicenseService.LeaseSeconds, heartbeatSeconds = LicenseService.HeartbeatSeconds
        }));
        admin.MapGet("/codes", (string? search, string? status, int? page, int? pageSize, LicenseService service) =>
            Results.Json(service.List(search, status, page ?? 1, pageSize ?? 25)));
        admin.MapPost("/codes", (IssueRequest request, LicenseService service) => Results.Json(service.Issue(request)));
        admin.MapPost("/codes/{id}/suspension", (string id, SuspensionRequest request, LicenseService service) =>
        { service.Suspend(id, request.Suspended); return Results.Json(new { ok = true }); });
        admin.MapPost("/codes/{id}/renew", (string id, RenewRequest request, LicenseService service) =>
        { service.Renew(id, request.DurationSeconds); return Results.Json(new { ok = true }); });
        admin.MapPost("/codes/{id}/unbind", (string id, LicenseService service) =>
        { service.Unbind(id); return Results.Json(new { ok = true }); });
        admin.MapPost("/codes/{id}/kick", (string id, LicenseService service) =>
        { service.Kick(id); return Results.Json(new { ok = true }); });
        admin.MapGet("/codes/export", (string? search, string? status, LicenseService service) =>
            Results.File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(service.Export(search, status))).ToArray(),
                "text/csv; charset=utf-8", "license-codes.csv"));
        admin.MapGet("/sessions", (string? codeId, bool? activeOnly, LicenseService service) =>
            Results.Json(new { items = service.Sessions(codeId, activeOnly ?? true) }));
        admin.MapGet("/audit", (int? page, int? pageSize, LicenseService service) =>
            Results.Json(service.GetAudit(page ?? 1, pageSize ?? 50)));
        admin.MapPost("/logout", async (HttpContext context) =>
        { await context.SignOutAsync("admin"); return Results.Json(new { ok = true }); });
        // Validate storage during startup, rather than on the first activation.
        app.Services.GetRequiredService<LicenseService>().Probe();
        return app;
    }

    private static string? Bearer(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? header[7..].Trim() : null;
    }
    private static string Asset(string name)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("Licensing.Server." + name)
            ?? throw new InvalidOperationException("管理资源缺失。");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    private static async Task Error(HttpContext context, int status, string code, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { error = code, message });
    }
}
