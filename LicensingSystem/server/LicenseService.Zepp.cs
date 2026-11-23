using System.Globalization;
using Microsoft.Data.Sqlite;

namespace Licensing.Server;

public sealed partial class LicenseService
{
    private bool IsZepp => options.ProductId == "ZeppPortal";

    private void InitializeZeppStorage(SqliteConnection db, SqliteTransaction tx)
    {
        if (!IsZepp) return;
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS ZeppRedemptions (
                RedemptionId TEXT PRIMARY KEY, CodeId TEXT NOT NULL UNIQUE, CodeHash TEXT NOT NULL UNIQUE,
                AccountId TEXT NOT NULL, StartsAt INTEGER NOT NULL, ExpiresAt INTEGER,
                Permanent INTEGER NOT NULL, Suspended INTEGER NOT NULL DEFAULT 0, Revoked INTEGER NOT NULL DEFAULT 0,
                CHECK((Permanent=1 AND ExpiresAt IS NULL) OR (Permanent=0 AND ExpiresAt>StartsAt))
            );
            CREATE INDEX IF NOT EXISTS IX_ZeppRedemptions_Account ON ZeppRedemptions(AccountId,StartsAt);
            CREATE TABLE IF NOT EXISTS ZeppRequests (
                RequestId TEXT PRIMARY KEY, AccountId TEXT NOT NULL, CodeHash TEXT NOT NULL,
                RedemptionId TEXT NOT NULL REFERENCES ZeppRedemptions(RedemptionId)
            );
            """;
        command.ExecuteNonQuery();
    }

    public ZeppEntitlementResponse Entitlement(ZeppEntitlementRequest request)
    {
        RequireZepp();
        var account = ZeppAccount(request.AccountId);
        using var db = Open();
        using var tx = db.BeginTransaction();
        var grants = ReadGrants(db, tx, account);
        tx.Commit();
        return new(true, options.ProductId, account, grants);
    }

    public ZeppRedemptionResponse Redeem(ZeppRedeemRequest request)
    {
        RequireZepp();
        var account = ZeppAccount(request.AccountId);
        if (request.RequestId is not { Length: 36 } || !Guid.TryParseExact(request.RequestId, "D", out var requestUuid)
            || NormalizeCode(request.Code) is not string code)
            throw new LicensingException("invalid_request", "兑换请求必须包含有界卡码及 UUID request_id。");
        var requestId = requestUuid.ToString("D");
        var hash = CodeHash(code);
        using var db = Open();
        // The write lock serializes competing accounts before the read/consume/receipt sequence.
        using var tx = db.BeginTransaction(deferred: false);
        using (var prior = db.CreateCommand())
        {
            prior.Transaction = tx;
            prior.CommandText = "SELECT AccountId,CodeHash,RedemptionId FROM ZeppRequests WHERE RequestId=$id";
            Add(prior, "$id", requestId);
            using var reader = prior.ExecuteReader();
            if (reader.Read())
            {
                if (reader.GetString(0) != account || reader.GetString(1) != hash)
                    throw new LicensingException("request_conflict", "request_id 已用于不同的兑换请求。", 409);
                var redemptionId = reader.GetString(2);
                reader.Close();
                return RedemptionResult(db, tx, account, redemptionId);
            }
        }
        using (var prior = db.CreateCommand())
        {
            prior.Transaction = tx;
            prior.CommandText = "SELECT AccountId,RedemptionId FROM ZeppRedemptions WHERE CodeHash=$hash";
            Add(prior, "$hash", hash);
            using var reader = prior.ExecuteReader();
            if (reader.Read())
            {
                if (reader.GetString(0) != account) throw new LicensingException("card_used", "此卡已绑定其他账号。", 409);
                var redemptionId = reader.GetString(1);
                reader.Close();
                RememberZeppRequest(db, tx, requestId, account, hash, redemptionId);
                return RedemptionResult(db, tx, account, redemptionId);
            }
        }
        var row = FindCode(db, tx, "c.CodeHash=$value", hash)
            ?? throw new LicensingException("invalid_card", "卡密无效。", 404);
        if (row.Suspended) throw new LicensingException("card_suspended", "卡密已停用。", 403);
        if (row.ActivatedAt is not null) throw new LicensingException("card_used", "卡密已有核销记录。", 409);
        var now = Now();
        var permanent = row.Kind == "permanent";
        if (!permanent && Convert.ToInt64(Scalar(db, tx, """
            SELECT COUNT(*) FROM ZeppRedemptions WHERE AccountId=$account AND Permanent=1
                AND Suspended=0 AND Revoked=0 AND StartsAt<=$now
            """, ("$account", account), ("$now", now)), CultureInfo.InvariantCulture) > 0)
            throw new LicensingException("permanent_member", "永久会员无需兑换时长卡。", 409);
        var end = Scalar(db, tx, "SELECT MAX(ExpiresAt) FROM ZeppRedemptions WHERE AccountId=$account AND Permanent=0", ("$account", account));
        var startsAt = permanent ? now : Math.Max(now, end is null or DBNull ? now : Convert.ToInt64(end, CultureInfo.InvariantCulture));
        if (!permanent && startsAt > PermanentExpiry - row.DurationSeconds!.Value)
            throw new LicensingException("invalid_request", "续期期限超过允许范围。");
        long? expiresAt = permanent ? null : AddDuration(startsAt, row.DurationSeconds!.Value);
        var id = Guid.NewGuid().ToString("D");
        Execute(db, tx, """
            INSERT INTO ZeppRedemptions(RedemptionId,CodeId,CodeHash,AccountId,StartsAt,ExpiresAt,Permanent)
            VALUES($id,$codeId,$hash,$account,$start,$expiry,$permanent)
            """, ("$id", id), ("$codeId", row.Id), ("$hash", hash), ("$account", account),
            ("$start", startsAt), ("$expiry", expiresAt), ("$permanent", permanent));
        Execute(db, tx, "UPDATE LicenseCodes SET ActivatedAt=$now,ExpiresAt=$expiry WHERE Id=$id",
            ("$now", now), ("$expiry", expiresAt), ("$id", row.Id));
        RememberZeppRequest(db, tx, requestId, account, hash, id);
        Audit(db, tx, "zepp_redeem", row.Id, System.Text.Json.JsonSerializer.Serialize(new { account_id = account, redemption_id = id }), now);
        return RedemptionResult(db, tx, account, id);
    }

    private ZeppRedemptionResponse RedemptionResult(SqliteConnection db, SqliteTransaction tx, string account, string id)
    {
        var grants = ReadGrants(db, tx, account);
        var receipt = grants.Single(grant => grant.RedemptionId == id);
        tx.Commit();
        return new(true, options.ProductId, account, receipt, grants);
    }
    private static void RememberZeppRequest(SqliteConnection db, SqliteTransaction tx, string requestId, string account, string hash, string id) =>
        Execute(db, tx, "INSERT INTO ZeppRequests(RequestId,AccountId,CodeHash,RedemptionId) VALUES($request,$account,$hash,$id)",
            ("$request", requestId), ("$account", account), ("$hash", hash), ("$id", id));

    private static IReadOnlyList<ZeppGrant> ReadGrants(SqliteConnection db, SqliteTransaction tx, string account)
    {
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = "SELECT RedemptionId,StartsAt,ExpiresAt,Permanent,Suspended,Revoked FROM ZeppRedemptions WHERE AccountId=$account ORDER BY StartsAt,RedemptionId";
        Add(command, "$account", account);
        using var reader = command.ExecuteReader();
        var grants = new List<ZeppGrant>();
        while (reader.Read()) grants.Add(new(reader.GetString(0), Utc(reader.GetInt64(1)),
            Number(reader, 2) is long expiry ? Utc(expiry) : null, reader.GetBoolean(3), reader.GetBoolean(4), reader.GetBoolean(5)));
        return grants;
    }

    private void RequireDeviceProduct()
    {
        if (IsZepp) throw new LicensingException("account_bound", "Zepp 使用固定 UID 核销，不使用设备绑定接口。", 403);
    }
    private void RequireZepp()
    {
        if (!IsZepp) throw new LicensingException("forbidden", "此产品不使用账号核销。", 403);
    }
    private static string ZeppAccount(string? account) => account is { Length: >= 1 and <= 32 }
        && account.All(c => c is >= '0' and <= '9') ? account
        : throw new LicensingException("invalid_request", "account_id 必须为十进制上游 UID，最长 32 字符。");
    private static string Utc(long seconds) => DateTimeOffset.FromUnixTimeSeconds(seconds)
        .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
}
