using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;

namespace Licensing.Server;

public sealed class LicenseService
{
    public const int LeaseSeconds = 60;
    public const int HeartbeatSeconds = 28;
    public const long PermanentExpiry = 253370764800; // 9999-01-01 UTC, below DateTime.MaxValue.
    private const long MaximumDuration = 3153600000; // 100 years.
    private readonly ServerOptions options;
    private readonly TimeProvider clock;
    private readonly LeaseSigner signer;
    private readonly IDataProtector codes;
    private readonly IDataProtector tokens;
    private readonly string connectionString;
    private const string CodeColumns = "c.Id,c.CodeCipher,c.CodeSuffix,c.Kind,c.DurationSeconds,c.Note,c.CreatedAt,c.ActivatedAt,c.ExpiresAt,c.BoundDeviceId,c.LastSteamId,c.Suspended";

    public LicenseService(ServerOptions options, IDataProtectionProvider protection, TimeProvider clock, LeaseSigner signer)
    {
        this.options = options;
        this.clock = clock;
        this.signer = signer;
        codes = protection.CreateProtector("Licensing.Native.Code.v1", options.ProductId);
        tokens = protection.CreateProtector("Licensing.Native.SessionToken.v1", options.ProductId);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(options.DatabasePath))!);
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(options.DatabasePath), Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 30, Pooling = false
        }.ToString();
        using var db = Open();
        using (var probe = db.CreateCommand())
        {
            probe.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
            using var reader = probe.ExecuteReader();
            while (reader.Read())
                if (reader.GetString(0) is not ("LicenseCodes" or "LeaseSessions" or "ActivationRequests" or "AuditEvents" or "NativeMetadata"))
                    throw new InvalidOperationException("数据库含旧版或其他系统表；请配置独立的新数据库路径。");
        }
        using var command = db.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS NativeMetadata (Name TEXT PRIMARY KEY, Value TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS LicenseCodes (
                Id TEXT PRIMARY KEY, ProductId TEXT NOT NULL, CodeHash TEXT NOT NULL UNIQUE,
                CodeCipher TEXT NOT NULL, CodeSuffix TEXT NOT NULL,
                Kind TEXT NOT NULL CHECK(Kind IN ('permanent','timed')), DurationSeconds INTEGER,
                Note TEXT NOT NULL, CreatedAt INTEGER NOT NULL, ActivatedAt INTEGER, ExpiresAt INTEGER,
                BoundDeviceId TEXT, LastSteamId TEXT, Suspended INTEGER NOT NULL DEFAULT 0,
                CHECK((Kind='permanent' AND DurationSeconds IS NULL AND ExpiresAt IS NULL)
                    OR (Kind='timed' AND DurationSeconds>0))
            );
            CREATE TABLE IF NOT EXISTS LeaseSessions (
                Id TEXT PRIMARY KEY, CodeId TEXT NOT NULL REFERENCES LicenseCodes(Id),
                TokenHash TEXT NOT NULL UNIQUE, TokenCipher TEXT NOT NULL,
                DeviceId TEXT NOT NULL, InstallationId TEXT NOT NULL, SteamId TEXT NOT NULL,
                GameName TEXT, ClientVersion TEXT NOT NULL, CreatedAt INTEGER NOT NULL,
                LastSeenAt INTEGER NOT NULL, LeaseUntil INTEGER NOT NULL, RevokedAt INTEGER, RevocationState TEXT,
                CHECK(LeaseUntil>CreatedAt)
            );
            CREATE INDEX IF NOT EXISTS IX_LeaseSessions_CodeId ON LeaseSessions(CodeId,RevokedAt,LeaseUntil);
            CREATE TABLE IF NOT EXISTS ActivationRequests (
                RequestId TEXT PRIMARY KEY, PayloadHash TEXT NOT NULL,
                SessionId TEXT NOT NULL REFERENCES LeaseSessions(Id)
            );
            CREATE TABLE IF NOT EXISTS AuditEvents (
                Id TEXT PRIMARY KEY, OccurredAt INTEGER NOT NULL, Action TEXT NOT NULL,
                CodeId TEXT, Details TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_AuditEvents_Time ON AuditEvents(OccurredAt);
            """;
        command.ExecuteNonQuery();
        using var tx = db.BeginTransaction(deferred: false);
        var product = Scalar(db, tx, "SELECT Value FROM NativeMetadata WHERE Name='ProductId'") as string;
        if (product is not null && product != options.ProductId)
            throw new InvalidOperationException("此数据库已属于另一个产品，请使用独立数据库。");
        Execute(db, tx, "INSERT OR IGNORE INTO NativeMetadata(Name,Value) VALUES('ProductId',$product)", ("$product", options.ProductId));
        tx.Commit();
    }

    public object VersionInfo() => new
    {
        minimum = options.MinimumVersion, latest = options.LatestVersion,
        download_url = options.DownloadUrl, notes = options.VersionNotes,
        connection_info = 0, steam_accounts_info = 0, qq_accounts_info = 0, product_id = options.ProductId
    };

    public LeaseResponse Activate(ActivationRequest request)
    {
        LeaseSigner.ValidateNonce(request.Nonce);
        var code = NormalizeCode(request.Code);
        var device = NormalizeDevice(request.DeviceId);
        var steam = NormalizeSteam(request.SteamId64);
        var installation = NormalizeInstallation(request.InstallationId);
        var requestId = Identity(request.RequestId, 128);
        var version = Identity(request.ClientVersion, 64);
        var game = (request.GameName ?? "").Trim();
        if (code is null || device is null || steam is null || installation is null || requestId is null
            || version is null || game.Length > 128 || game.Any(char.IsControl))
            return Denied("revoked", Now(), "激活请求字段无效。");
        var hash = CodeHash(code);
        var payload = Hash(JsonSerializer.Serialize(new[] { hash, device, steam, installation, version, game }));
        using var db = Open();
        using var tx = db.BeginTransaction(deferred: false);
        var now = Now();
        var prior = FindRequest(db, tx, requestId);
        if (prior is not null && prior.Value.PayloadHash != payload)
            return Denied("request_conflict", now, "request_id 已用于其他激活负载。");
        if (Outdated(version)) return UpdateRequired(now);
        if (prior is not null)
        {
            var session = FindSession(db, tx, "s.Id=$value", prior.Value.SessionId)!;
            var rejection = CheckSession(session, now);
            if (rejection is not null) return RejectSession(db, tx, session, rejection, now);
            tx.Commit();
            return signer.Sign(Active(session.Code, session.LeaseUntil, tokens.Unprotect(session.TokenCipher), now),
                device, steam, installation, request.Nonce!, tokens.Unprotect(session.TokenCipher));
        }
        var row = FindCode(db, tx, "c.CodeHash=$value", hash);
        if (row is null) return Denied("revoked", now, "卡密无效。");
        if (row.Suspended) return Denied("blocked", now);
        if (row.ExpiresAt is not null && row.ExpiresAt <= now) return Denied("expired", now);
        // Retire timed-out predecessors atomically with their replacement, including after clock rollback.
        Execute(db, tx, """
            UPDATE LeaseSessions SET RevokedAt=$now,RevocationState='revoked'
            WHERE CodeId=$id AND RevokedAt IS NULL AND LeaseUntil<=$now
            """, ("$now", now), ("$id", row.Id));
        if (Convert.ToInt64(Scalar(db, tx,
                "SELECT COUNT(*) FROM LeaseSessions WHERE CodeId=$id AND RevokedAt IS NULL AND LeaseUntil>$now",
                ("$id", row.Id), ("$now", now)), CultureInfo.InvariantCulture) > 0)
            return Denied("in_use", now);
        if (row.BoundDeviceId is not null && row.BoundDeviceId != device)
            return Denied("identity_changed", now, "卡密已绑定其他设备。");
        var activated = row.ActivatedAt ?? now;
        var expiry = row.Kind == "permanent" ? (long?)null : row.ExpiresAt ?? AddDuration(now, row.DurationSeconds!.Value);
        var lease = Math.Min(now + LeaseSeconds, expiry ?? PermanentExpiry);
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var sessionId = Guid.NewGuid().ToString("N");
        Execute(db, tx, """
            UPDATE LicenseCodes SET ActivatedAt=$activated,ExpiresAt=$expiry,BoundDeviceId=$device,LastSteamId=$steam WHERE Id=$id;
            """, ("$activated", activated), ("$expiry", expiry), ("$device", device), ("$steam", steam), ("$id", row.Id));
        Execute(db, tx, """
            INSERT INTO LeaseSessions(Id,CodeId,TokenHash,TokenCipher,DeviceId,InstallationId,SteamId,GameName,ClientVersion,CreatedAt,LastSeenAt,LeaseUntil)
            VALUES($id,$code,$hash,$cipher,$device,$installation,$steam,$game,$version,$now,$now,$lease)
            """, ("$id", sessionId), ("$code", row.Id), ("$hash", TokenHash(token)), ("$cipher", tokens.Protect(token)),
            ("$device", device), ("$installation", installation), ("$steam", steam), ("$game", game.Length == 0 ? null : game),
            ("$version", version), ("$now", now), ("$lease", lease));
        Execute(db, tx, "INSERT INTO ActivationRequests(RequestId,PayloadHash,SessionId) VALUES($id,$payload,$session)",
            ("$id", requestId), ("$payload", payload), ("$session", sessionId));
        Audit(db, tx, "activate", row.Id, "{}", now);
        tx.Commit();
        return signer.Sign(Active(row with { ActivatedAt = activated, ExpiresAt = expiry, BoundDeviceId = device }, lease, token, now),
            device, steam, installation, request.Nonce!, token);
    }

    public LeaseResponse Heartbeat(string? token, HeartbeatRequest request)
    {
        LeaseSigner.ValidateNonce(request.Nonce);
        if (!ValidToken(token)) return Denied("revoked", Now());
        using var db = Open();
        using var tx = db.BeginTransaction(deferred: false);
        var now = Now();
        var session = FindSession(db, tx, "s.TokenHash=$value", TokenHash(token!));
        if (session is null) return Denied("revoked", now);
        var rejection = CheckSession(session, now);
        if (rejection is not null) return RejectSession(db, tx, session, rejection, now);
        // A large wall-clock correction must not violate LeaseUntil>CreatedAt or revive on correction.
        if (now + LeaseSeconds <= session.CreatedAt)
        {
            RevokeSession(db, tx, session.Id, "revoked", now);
            Audit(db, tx, "clock_rollback", session.Code.Id, "{}", now);
            tx.Commit();
            return Denied("revoked", now);
        }
        var device = NormalizeDevice(request.DeviceId);
        var installation = NormalizeInstallation(request.InstallationId);
        var steam = NormalizeSteam(request.SteamId64);
        var version = Identity(request.ClientVersion, 64);
        if (device != session.DeviceId || installation != session.InstallationId || steam != session.SteamId)
        {
            RevokeSession(db, tx, session.Id, "identity_changed", now);
            Audit(db, tx, "identity_changed", session.Code.Id, "{}", now);
            tx.Commit();
            return Denied("identity_changed", now);
        }
        if (version is null || Outdated(version)) return UpdateRequired(now);
        var lease = Math.Min(now + LeaseSeconds, session.Code.ExpiresAt ?? PermanentExpiry);
        Execute(db, tx, "UPDATE LeaseSessions SET LastSeenAt=$now,LeaseUntil=$lease,ClientVersion=$version WHERE Id=$id",
            ("$now", now), ("$lease", lease), ("$version", version), ("$id", session.Id));
        tx.Commit();
        return signer.Sign(Active(session.Code, lease, token!, now), device!, steam!, installation!, request.Nonce!, token!);
    }

    private static string? NormalizeInstallation(string? value) => Guid.TryParse(value, out var id) ? id.ToString("D").ToLowerInvariant() : null;
    private static string? NormalizeSteam(string? value) => value is { Length: >= 1 and <= 20 }
        && value.All(c => c is >= '0' and <= '9') && ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out _) ? value : null;

    public void Logout(string? token)
    {
        if (!ValidToken(token)) return;
        using var db = Open();
        using var tx = db.BeginTransaction(deferred: false);
        var now = Now();
        var session = FindSession(db, tx, "s.TokenHash=$value", TokenHash(token!));
        if (session is not null && session.RevokedAt is null)
        {
            RevokeSession(db, tx, session.Id, "revoked", now);
            Audit(db, tx, "logout", session.Code.Id, "{}", now);
        }
        tx.Commit();
    }

    public IssuedCodes Issue(IssueRequest request)
    {
        if (request.Kind is not ("permanent" or "timed") || request.Quantity is < 1 or > 500
            || (request.Kind == "permanent" && request.DurationSeconds is not null)
            || (request.Kind == "timed" && request.DurationSeconds is not (>= 1 and <= MaximumDuration))
            || (request.Note?.Length ?? 0) > 2000)
            throw new LicensingException("invalid_issue", "请选择永久或有效秒数的限时授权；数量范围为 1–500。");
        var now = Now();
        using var db = Open();
        using var tx = db.BeginTransaction(deferred: false);
        var result = new List<CodeRow>(request.Quantity);
        for (var i = 0; i < request.Quantity; i++)
        {
            var id = Guid.NewGuid().ToString("N");
            var code = options.CodePrefix.ToUpperInvariant() + "-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(20));
            var suffix = code[^8..];
            Execute(db, tx, """
                INSERT INTO LicenseCodes(Id,ProductId,CodeHash,CodeCipher,CodeSuffix,Kind,DurationSeconds,Note,CreatedAt)
                VALUES($id,$product,$hash,$cipher,$suffix,$kind,$duration,$note,$now)
                """, ("$id", id), ("$product", options.ProductId), ("$hash", CodeHash(code)), ("$cipher", codes.Protect(code)),
                ("$suffix", suffix), ("$kind", request.Kind), ("$duration", request.DurationSeconds),
                ("$note", request.Note ?? ""), ("$now", now));
            Audit(db, tx, "issue", id, JsonSerializer.Serialize(new { kind = request.Kind, durationSeconds = request.DurationSeconds }), now);
            result.Add(new(id, code, suffix, request.Kind, request.DurationSeconds, request.Note ?? "", now, null, null, null, null, "unused", 0));
        }
        tx.Commit();
        return new(result, result.Count);
    }

    public CodePage List(string? search = null, string? status = null, int page = 1, int pageSize = 25)
    {
        ValidatePaging(page, pageSize);
        ValidateStatus(status);
        using var db = Open();
        using var tx = db.BeginTransaction();
        var now = Now();
        var all = ReadCodes(db, tx, now);
        var stats = new CodeStats(all.Count, all.Count(row => row.State == "unused"), all.Count(row => row.State == "active"),
            all.Count(row => row.State == "expired"), all.Count(row => row.State == "suspended"), all.Count(row => row.ActiveSessionCount > 0));
        var filtered = Filter(all, search, status).ToArray();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        tx.Commit();
        return new(items, filtered.Length, page, pageSize, stats);
    }

    public string Export(string? search = null, string? status = null)
    {
        ValidateStatus(status);
        using var db = Open();
        using var tx = db.BeginTransaction();
        var rows = Filter(ReadCodes(db, tx, Now()), search, status);
        var text = new StringBuilder("id,code,codeSuffix,kind,durationSeconds,note,createdAtUnix,activatedAtUnix,expiresAtUnix,boundDeviceId,lastSteamId,state,activeSessionCount\r\n");
        foreach (var row in rows)
            text.AppendJoin(',', new object?[] { row.Id, row.Code, row.CodeSuffix, row.Kind, row.DurationSeconds, row.Note,
                row.CreatedAtUnix, row.ActivatedAtUnix, row.ExpiresAtUnix, row.BoundDeviceId, row.LastSteamId, row.State,
                row.ActiveSessionCount }.Select(Csv)).Append("\r\n");
        Audit(db, tx, "export", null, "{}", Now());
        tx.Commit();
        return text.ToString();
    }

    public void Suspend(string id, bool suspended)
    {
        MutateCode(id, "suspension", JsonSerializer.Serialize(new { suspended }), (db, tx, row, now) =>
        {
            Execute(db, tx, "UPDATE LicenseCodes SET Suspended=$suspended WHERE Id=$id", ("$suspended", suspended ? 1 : 0), ("$id", id));
            if (suspended) RevokeCode(db, tx, id, "blocked", now);
        });
    }

    public void Renew(string id, long durationSeconds)
    {
        if (durationSeconds is < 1 or > MaximumDuration)
            throw new LicensingException("invalid_duration", "续期秒数必须在 1–3153600000 之间。");
        MutateCode(id, "renew", JsonSerializer.Serialize(new { durationSeconds }), (db, tx, row, now) =>
        {
            if (row.Kind != "timed") throw new LicensingException("invalid_kind", "永久卡无需续期。");
            var duration = checked(row.DurationSeconds!.Value + durationSeconds);
            if (duration > MaximumDuration) throw new LicensingException("invalid_duration", "累计授权时长超过 100 年。");
            var expiry = row.ExpiresAt is null ? (long?)null : AddDuration(Math.Max(now, row.ExpiresAt.Value), durationSeconds);
            Execute(db, tx, "UPDATE LicenseCodes SET DurationSeconds=$duration,ExpiresAt=$expiry WHERE Id=$id",
                ("$duration", duration), ("$expiry", expiry), ("$id", id));
        });
    }

    public void Unbind(string id) => MutateCode(id, "unbind", "{}", (db, tx, row, now) =>
    {
        Execute(db, tx, "UPDATE LicenseCodes SET BoundDeviceId=NULL WHERE Id=$id", ("$id", id));
        RevokeCode(db, tx, id, "revoked", now);
    });

    public void Kick(string id) => MutateCode(id, "kick", "{}", (db, tx, row, now) => RevokeCode(db, tx, id, "revoked", now));

    public IReadOnlyList<SessionRow> Sessions(string? codeId = null, bool activeOnly = true)
    {
        var now = Now();
        using var db = Open();
        using var command = db.CreateCommand();
        command.CommandText = """
            SELECT s.Id,s.CodeId,c.CodeSuffix,s.DeviceId,s.InstallationId,s.SteamId,s.GameName,
                s.CreatedAt,s.LastSeenAt,s.LeaseUntil,s.RevokedAt,s.RevocationState,c.Suspended,c.ExpiresAt
            FROM LeaseSessions s JOIN LicenseCodes c ON c.Id=s.CodeId
            WHERE ($code IS NULL OR s.CodeId=$code)
                AND ($active=0 OR (s.RevokedAt IS NULL AND s.LeaseUntil>$now AND c.Suspended=0 AND (c.ExpiresAt IS NULL OR c.ExpiresAt>$now)))
            ORDER BY s.CreatedAt DESC,s.Id DESC
            """;
        Add(command, "$code", string.IsNullOrWhiteSpace(codeId) ? null : codeId);
        Add(command, "$active", activeOnly ? 1 : 0); Add(command, "$now", now);
        using var reader = command.ExecuteReader();
        var result = new List<SessionRow>();
        while (reader.Read())
        {
            var revoked = Number(reader, 10);
            var state = revoked is not null ? Text(reader, 11) ?? "revoked"
                : reader.GetBoolean(12) ? "blocked" : Number(reader, 13) is long expiry && expiry <= now ? "expired"
                : reader.GetInt64(9) <= now ? "expired" : "active";
            result.Add(new(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                reader.GetString(5), Text(reader, 6), reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), revoked, state));
        }
        return result;
    }

    public AuditPage GetAudit(int page = 1, int pageSize = 50)
    {
        ValidatePaging(page, pageSize);
        using var db = Open();
        using var tx = db.BeginTransaction();
        var total = Convert.ToInt32(Scalar(db, tx, "SELECT COUNT(*) FROM AuditEvents"), CultureInfo.InvariantCulture);
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = "SELECT Id,OccurredAt,Action,CodeId,Details FROM AuditEvents ORDER BY OccurredAt DESC,Id DESC LIMIT $limit OFFSET $offset";
        Add(command, "$limit", pageSize); Add(command, "$offset", (page - 1) * pageSize);
        var result = new List<AuditRow>();
        using (var reader = command.ExecuteReader())
            while (reader.Read()) result.Add(new(reader.GetString(0), reader.GetInt64(1), reader.GetString(2), Text(reader, 3), reader.GetString(4)));
        tx.Commit();
        return new(result, total, page, pageSize);
    }

    public void Probe()
    {
        using var db = Open();
        using var command = db.CreateCommand(); command.CommandText = "SELECT 1"; command.ExecuteScalar();
    }

    private void MutateCode(string id, string action, string details, Action<SqliteConnection, SqliteTransaction, CodeRecord, long> mutation)
    {
        using var db = Open();
        using var tx = db.BeginTransaction(deferred: false);
        var row = FindCode(db, tx, "c.Id=$value", id) ?? throw new LicensingException("not_found", "卡密记录不存在。", 404);
        var now = Now();
        mutation(db, tx, row, now);
        Audit(db, tx, action, id, details, now);
        tx.Commit();
    }

    private List<CodeRow> ReadCodes(SqliteConnection db, SqliteTransaction tx, long now)
    {
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = $"""
            SELECT {CodeColumns},(SELECT COUNT(*) FROM LeaseSessions s WHERE s.CodeId=c.Id
                AND s.RevokedAt IS NULL AND s.LeaseUntil>$now AND c.Suspended=0 AND (c.ExpiresAt IS NULL OR c.ExpiresAt>$now))
            FROM LicenseCodes c ORDER BY c.CreatedAt DESC,c.Id DESC
            """;
        Add(command, "$now", now);
        using var reader = command.ExecuteReader();
        var result = new List<CodeRow>();
        while (reader.Read())
        {
            var row = ReadCode(reader);
            result.Add(new(row.Id, codes.Unprotect(row.CodeCipher), row.CodeSuffix, row.Kind, row.DurationSeconds,
                row.Note, row.CreatedAt, row.ActivatedAt, row.ExpiresAt, row.BoundDeviceId, row.LastSteamId,
                CodeState(row, now), reader.GetInt32(12)));
        }
        return result;
    }

    private static IEnumerable<CodeRow> Filter(IEnumerable<CodeRow> rows, string? search, string? status)
    {
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
            rows = status == "online" ? rows.Where(row => row.ActiveSessionCount > 0) : rows.Where(row => row.State == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            rows = rows.Where(row => new[] { row.Id, row.Code, row.Note, row.BoundDeviceId, row.LastSteamId }
                .Any(value => value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true));
        }
        return rows;
    }

    private CodeRecord? FindCode(SqliteConnection db, SqliteTransaction tx, string predicate, string value)
    {
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = $"SELECT {CodeColumns} FROM LicenseCodes c WHERE {predicate}";
        Add(command, "$value", value);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadCode(reader) : null;
    }

    private static (string PayloadHash, string SessionId)? FindRequest(SqliteConnection db, SqliteTransaction tx, string requestId)
    {
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = "SELECT PayloadHash,SessionId FROM ActivationRequests WHERE RequestId=$id";
        Add(command, "$id", requestId);
        using var reader = command.ExecuteReader();
        return reader.Read() ? (reader.GetString(0), reader.GetString(1)) : null;
    }

    private SessionRecord? FindSession(SqliteConnection db, SqliteTransaction tx, string predicate, string value)
    {
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = $"""
            SELECT {CodeColumns},s.Id,s.TokenCipher,s.DeviceId,s.InstallationId,s.SteamId,s.LeaseUntil,s.RevokedAt,s.RevocationState,s.CreatedAt
            FROM LeaseSessions s JOIN LicenseCodes c ON c.Id=s.CodeId WHERE {predicate}
            """;
        Add(command, "$value", value);
        using var reader = command.ExecuteReader();
        return reader.Read() ? new(ReadCode(reader), reader.GetString(12), reader.GetString(13), reader.GetString(14),
            reader.GetString(15), reader.GetString(16), reader.GetInt64(17), Number(reader, 18), Text(reader, 19), reader.GetInt64(20)) : null;
    }

    private static LeaseResponse RejectSession(SqliteConnection db, SqliteTransaction tx, SessionRecord session, string state, long now)
    {
        // Once the server observes a terminal session, a clock rollback or renewal cannot revive it.
        if (session.RevokedAt is null) RevokeSession(db, tx, session.Id, state, now);
        tx.Commit();
        return Denied(state, now);
    }

    private static string? CheckSession(SessionRecord session, long now)
    {
        if (session.Code.Suspended) return "blocked";
        if (session.Code.ExpiresAt is long expiry && expiry <= now) return "expired";
        if (session.RevokedAt is not null) return session.RevocationState ?? "revoked";
        if (session.Code.BoundDeviceId != session.DeviceId) return "identity_changed";
        return session.LeaseUntil <= now ? "revoked" : null;
    }

    private static CodeRecord ReadCode(SqliteDataReader reader) => new(reader.GetString(0), reader.GetString(1),
        reader.GetString(2), reader.GetString(3), Number(reader, 4), reader.GetString(5), reader.GetInt64(6),
        Number(reader, 7), Number(reader, 8), Text(reader, 9), Text(reader, 10), reader.GetBoolean(11));
    private sealed record CodeRecord(string Id, string CodeCipher, string CodeSuffix, string Kind, long? DurationSeconds,
        string Note, long CreatedAt, long? ActivatedAt, long? ExpiresAt, string? BoundDeviceId, string? LastSteamId, bool Suspended);
    private sealed record SessionRecord(CodeRecord Code, string Id, string TokenCipher, string DeviceId, string InstallationId,
        string SteamId, long LeaseUntil, long? RevokedAt, string? RevocationState, long CreatedAt);

    private static LeaseResponse Active(CodeRecord code, long lease, string token, long now) => new()
    {
        State = "active", ServerTime = now, LeaseUntil = Math.Min(lease, now + LeaseSeconds), ExpiresAt = code.ExpiresAt ?? PermanentExpiry,
        SessionToken = token, LicenseKind = code.Kind
    };
    private static LeaseResponse Denied(string state, long now, string? message = null) => new() { State = state, ServerTime = now, Message = message };
    private LeaseResponse UpdateRequired(long now) => new()
    {
        State = "update_required", ServerTime = now, Minimum = options.MinimumVersion,
        Latest = options.LatestVersion, DownloadUrl = options.DownloadUrl
    };
    private bool Outdated(string version) => !Version.TryParse(version, out var actual) || actual < Version.Parse(options.MinimumVersion);
    private static string CodeState(CodeRecord row, long now) => row.Suspended ? "suspended"
        : row.ExpiresAt is long expiry && expiry <= now ? "expired" : row.ActivatedAt is null ? "unused" : "active";
    private long Now() => clock.GetUtcNow().ToUnixTimeSeconds();
    private string CodeHash(string code) => Hash(options.ProductId + "|code-v1|" + code);
    private string TokenHash(string token) => Hash(options.ProductId + "|session-v1|" + token.ToLowerInvariant());
    private static string Hash(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
    private static bool ValidToken(string? token) => token?.Length == 64 && token.All(char.IsAsciiHexDigit);
    private static string? NormalizeDevice(string? device) => device?.Length == 64 && device.All(char.IsAsciiHexDigit) ? device.ToUpperInvariant() : null;
    private static string? NormalizeCode(string? code) => string.IsNullOrWhiteSpace(code) || code.Length > 256 ? null : code.Trim().ToUpperInvariant();
    private static string? Identity(string? value, int max) => string.IsNullOrWhiteSpace(value) || value.Length > max
        || value.Any(char.IsControl) ? null : value.Trim();
    private static long AddDuration(long start, long duration)
    {
        if (duration > PermanentExpiry - start) throw new LicensingException("invalid_duration", "期限超过协议允许的时间范围。");
        return checked(start + duration);
    }
    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1 || page > 1000000 || pageSize is < 1 or > 500)
            throw new LicensingException("invalid_paging", "page 必须大于零，pageSize 范围为 1–500。");
    }
    private static void ValidateStatus(string? status)
    {
        if (!string.IsNullOrEmpty(status) && status is not ("all" or "unused" or "active" or "expired" or "suspended" or "online"))
            throw new LicensingException("invalid_status", "状态筛选值无效。");
    }
    private static string Csv(object? value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        var trimmed = text.TrimStart();
        if (trimmed.Length > 0 && (trimmed[0] is '=' or '+' or '-' or '@' || text[0] is '\t' or '\r' or '\n')) text = "'" + text;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
    private static string? Text(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetString(index);
    private static long? Number(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetInt64(index);
    private SqliteConnection Open()
    {
        var db = new SqliteConnection(connectionString);
        db.Open();
        using var command = db.CreateCommand(); command.CommandText = "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=30000;"; command.ExecuteNonQuery();
        return db;
    }
    private static void Add(SqliteCommand command, string name, object? value) => command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    private static void Execute(SqliteConnection db, SqliteTransaction tx, string sql, params (string Name, object? Value)[] parameters)
    {
        using var command = db.CreateCommand(); command.Transaction = tx; command.CommandText = sql;
        foreach (var item in parameters) Add(command, item.Name, item.Value);
        command.ExecuteNonQuery();
    }
    private static object? Scalar(SqliteConnection db, SqliteTransaction tx, string sql, params (string Name, object? Value)[] parameters)
    {
        using var command = db.CreateCommand(); command.Transaction = tx; command.CommandText = sql;
        foreach (var item in parameters) Add(command, item.Name, item.Value);
        return command.ExecuteScalar();
    }
    private static void Audit(SqliteConnection db, SqliteTransaction tx, string action, string? codeId, string details, long now) =>
        Execute(db, tx, "INSERT INTO AuditEvents(Id,OccurredAt,Action,CodeId,Details) VALUES($id,$now,$action,$code,$details)",
            ("$id", Guid.NewGuid().ToString("N")), ("$now", now), ("$action", action), ("$code", codeId), ("$details", details));
    private static void RevokeSession(SqliteConnection db, SqliteTransaction tx, string id, string state, long now) =>
        Execute(db, tx, "UPDATE LeaseSessions SET RevokedAt=$now,RevocationState=$state WHERE Id=$id AND RevokedAt IS NULL",
            ("$now", now), ("$state", state), ("$id", id));
    private static void RevokeCode(SqliteConnection db, SqliteTransaction tx, string id, string state, long now) =>
        Execute(db, tx, "UPDATE LeaseSessions SET RevokedAt=$now,RevocationState=$state WHERE CodeId=$id AND RevokedAt IS NULL",
            ("$now", now), ("$state", state), ("$id", id));
}
