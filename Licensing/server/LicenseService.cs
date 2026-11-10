using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Licensing.Server;

public sealed record IssueRequest(string Kind, int? Days = null, string? ProductId = null);
public sealed record IssuedCode(string Id, string Code, string Kind, int? Days, string ProductId);
public sealed record CodeSummary(string Id, string CodeHint, string Kind, int? Days, string LicenseId,
    string? DeviceId, DateTimeOffset? ActivatedAtUtc, DateTimeOffset? ExpiresAtUtc, bool Suspended,
    DateTimeOffset CreatedAtUtc, string ProductId);
public sealed class LicensingException(string code, string message, int status = 400) : Exception(message)
{
    public string Code { get; } = code;
    public int Status { get; } = status;
}

public sealed class LicenseService
{
    private readonly ServerOptions options;
    private readonly KeygenGateway keygen;
    private readonly string connectionString;

    public LicenseService(ServerOptions options, KeygenGateway keygen)
    {
        this.options = options;
        this.keygen = keygen;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(options.DatabasePath))!);
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(options.DatabasePath), Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 45, Pooling = false
        }.ToString();
        using var db = Open();
        using var command = db.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS Codes (
                Id TEXT PRIMARY KEY, CodeHash TEXT NOT NULL UNIQUE, CodeHint TEXT NOT NULL,
                ProductId TEXT NOT NULL,
                Kind TEXT NOT NULL CHECK(Kind IN ('permanent','timed')), Days INTEGER,
                LicenseId TEXT NOT NULL UNIQUE, DeviceId TEXT, MachineId TEXT,
                ActivatedAt TEXT, ExpiresAt TEXT, Certificate TEXT, Suspended INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                CHECK ((Kind='permanent' AND Days IS NULL) OR (Kind='timed' AND Days BETWEEN 1 AND 36500))
            );
            """;
        command.ExecuteNonQuery();
        EnsureProductColumn(db, options.ResolveProduct(null).Id);
    }

    // 旧版数据库没有 ProductId 列：补列并回填默认产品，历史授权码保持可用。
    private static void EnsureProductColumn(SqliteConnection db, string defaultProductId)
    {
        var exists = false;
        using (var probe = db.CreateCommand())
        {
            probe.CommandText = "PRAGMA table_info(Codes)";
            using var reader = probe.ExecuteReader();
            while (reader.Read()) if (reader.GetString(1) == "ProductId") { exists = true; break; }
        }
        if (exists) return;
        using (var alter = db.CreateCommand())
        {
            alter.CommandText = "ALTER TABLE Codes ADD COLUMN ProductId TEXT";
            alter.ExecuteNonQuery();
        }
        using var fill = db.CreateCommand();
        fill.CommandText = "UPDATE Codes SET ProductId=$product WHERE ProductId IS NULL";
        Add(fill, "$product", defaultProductId);
        fill.ExecuteNonQuery();
    }

    public async Task<IssuedCode> IssueAsync(IssueRequest request, CancellationToken ct)
    {
        if (request.Kind is not ("permanent" or "timed")
            || (request.Kind == "permanent" && request.Days is not null)
            || (request.Kind == "timed" && request.Days is not (>= 1 and <= 36500)))
            throw new LicensingException("InvalidIssue", "请选择永久授权，或 1–36500 天的限时授权。");
        var product = options.ResolveProduct(request.ProductId);
        var license = await keygen.CreateLicenseAsync(product, request.Kind, request.Days, ct);
        var id = Guid.NewGuid().ToString("N");
        var code = product.EffectiveCodePrefix + "-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(20));
        using var db = Open();
        using var command = db.CreateCommand();
        command.CommandText = "INSERT INTO Codes(Id,CodeHash,CodeHint,ProductId,Kind,Days,LicenseId,CreatedAt) VALUES($id,$hash,$hint,$product,$kind,$days,$license,$created)";
        Add(command, "$id", id); Add(command, "$hash", HashCode(product.Id, code)); Add(command, "$hint", code[^8..]);
        Add(command, "$product", product.Id);
        Add(command, "$kind", request.Kind); Add(command, "$days", request.Days); Add(command, "$license", license.Id);
        Add(command, "$created", Format(DateTimeOffset.UtcNow));
        await command.ExecuteNonQueryAsync(ct);
        return new(id, code, request.Kind, request.Days, product.Id);
    }

    internal async Task<LicenseApiResponse> ActivateAsync(ActivationRequest request, CancellationToken ct)
    {
        CheckDevice(request.DeviceId);
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Length > 256)
            throw new LicensingException("InvalidCode", "授权码无效。");
        var product = options.ResolveProduct(request.ProductId);
        using var db = Open();
        // IMMEDIATE writer transaction serializes redemption across threads and server processes sharing this database.
        using var tx = db.BeginTransaction(deferred: false);
        var row = Find(db, tx, "CodeHash", HashCode(product.Id, request.Code.Trim().ToUpperInvariant()));
        if (row is null) throw new LicensingException("InvalidCode", "授权码不存在。");
        if (row.DeviceId is not null && row.DeviceId != request.DeviceId)
            throw new LicensingException("AlreadyRedeemed", "此码已由其他设备成功兑换。", 409);
        if (row.Kind == "permanent" && row.Certificate is not null)
        {
            tx.Commit();
            return new("Valid", "已恢复本机永久激活结果。", row.Certificate);
        }
        var license = await keygen.GetLicenseAsync(row.LicenseId, product.Id, ct);
        CheckLicense(license, row);
        var machines = await keygen.GetMachinesAsync(row.LicenseId, ct);
        if (machines.Count > 1) throw new LicensingException("BackendStateConflict", "服务端设备数量异常，请联系管理员。", 503);
        var machine = machines.SingleOrDefault();
        if (machine is null)
            machine = await keygen.CreateMachineAsync(row.LicenseId, request.DeviceId, product.Id, ct);
        if (machine.Fingerprint != request.DeviceId)
            throw new LicensingException("AlreadyRedeemed", "此码已由其他设备成功兑换。", 409);
        // Remote machine.created is durable even if this process dies or the successful response is lost.
        // This avoids Keygen FROM_FIRST_ACTIVATION's asynchronous expiry update.
        var activated = machine.CreatedAt.ToUniversalTime();
        var expires = row.Kind == "timed" ? activated.AddDays(row.Days!.Value) : (DateTimeOffset?)null;
        if (expires is not null && license.Expiry is null)
            await keygen.SetExpiryAsync(row.LicenseId, expires.Value, ct);
        else if (license.Expiry != expires)
            throw new LicensingException("BackendStateConflict", "Keygen 与兑换期限记录不一致，请联系管理员。", 503);
        await keygen.ValidateAsync(row.LicenseId, request.DeviceId, ct);
        var certificate = await keygen.CheckoutAsync(machine.Id, ct);
        var verified = MachineFile.Verify(certificate, options.VerificationPublicKeyPem, request.DeviceId, product.Id);
        CheckGrant(verified, row, machine.Id, expires);
        using var update = db.CreateCommand();
        update.Transaction = tx;
        update.CommandText = "UPDATE Codes SET DeviceId=$device,MachineId=$machine,ActivatedAt=$activated,ExpiresAt=$expiry,Certificate=$certificate WHERE Id=$id";
        Add(update, "$device", request.DeviceId); Add(update, "$machine", machine.Id);
        Add(update, "$activated", Format(activated)); Add(update, "$expiry", Format(expires));
        Add(update, "$certificate", certificate); Add(update, "$id", row.Id);
        await update.ExecuteNonQueryAsync(ct);
        tx.Commit();
        return new("Valid", "本机激活成功。", certificate);
    }

    internal async Task<LicenseApiResponse> ValidateAsync(ValidationRequest request, CancellationToken ct)
    {
        CheckDevice(request.DeviceId);
        var product = options.ResolveProduct(request.ProductId);
        var existing = MachineFile.Verify(request.Certificate, options.VerificationPublicKeyPem, request.DeviceId, product.Id);
        using var db = Open();
        var row = Find(db, null, "LicenseId", existing.LicenseId)
            ?? throw new LicensingException("InvalidLicense", "服务端不存在此兑换记录。", 403);
        if (row.DeviceId != request.DeviceId || row.MachineId != existing.MachineId)
            throw new LicensingException("DeviceMismatch", "服务端设备绑定不匹配。", 403);
        var rowProduct = string.IsNullOrWhiteSpace(row.ProductId) ? product.Id : row.ProductId;
        if (!string.Equals(rowProduct, product.Id, StringComparison.OrdinalIgnoreCase))
            throw new LicensingException("InvalidLicense", "授权记录所属产品不匹配。", 403);
        if (row.Kind == "permanent") return new("Valid", "永久授权无需再次联网。", row.Certificate);
        var license = await keygen.GetLicenseAsync(row.LicenseId, rowProduct, ct);
        CheckLicense(license, row);
        if (license.Expiry != row.ExpiresAt)
            throw new LicensingException("BackendStateConflict", "Keygen 与兑换期限记录不一致。", 503);
        await keygen.ValidateAsync(row.LicenseId, request.DeviceId, ct);
        var certificate = await keygen.CheckoutAsync(existing.MachineId, ct);
        var verified = MachineFile.Verify(certificate, options.VerificationPublicKeyPem, request.DeviceId, product.Id);
        CheckGrant(verified, row, existing.MachineId, row.ExpiresAt);
        return new("Valid", "限时授权已联网确认。", certificate);
    }

    public IReadOnlyList<CodeSummary> List()
    {
        using var db = Open();
        using var command = db.CreateCommand();
        command.CommandText = "SELECT Id,CodeHint,ProductId,Kind,Days,LicenseId,DeviceId,MachineId,ActivatedAt,ExpiresAt,Certificate,Suspended,CreatedAt FROM Codes ORDER BY CreatedAt DESC LIMIT 1000";
        using var reader = command.ExecuteReader();
        var result = new List<CodeSummary>();
        while (reader.Read()) result.Add(Read(reader).Summary);
        return result;
    }

    public async Task SuspendAsync(string id, bool suspend, CancellationToken ct)
    {
        using var db = Open();
        using var tx = db.BeginTransaction(deferred: false);
        var row = Find(db, tx, "Id", id) ?? throw new LicensingException("NotFound", "授权记录不存在。", 404);
        if (row.Kind != "timed") throw new LicensingException("PermanentIrrevocable", "永久签名许可不提供远程撤回；此入口仅管理限时授权。");
        await keygen.SuspendAsync(row.LicenseId, suspend, ct);
        using var command = db.CreateCommand();
        command.Transaction = tx;
        command.CommandText = "UPDATE Codes SET Suspended=$suspended WHERE Id=$id";
        Add(command, "$suspended", suspend ? 1 : 0); Add(command, "$id", id);
        await command.ExecuteNonQueryAsync(ct);
        tx.Commit();
    }

    private static void CheckLicense(KeygenLicense license, Row row)
    {
        if (license.Suspended || row.Suspended) throw new LicensingException("Suspended", "限时授权已被管理员停用。", 403);
        if (license.Kind != row.Kind || license.Days != row.Days)
            throw new LicensingException("BackendStateConflict", "Keygen 签名元数据与发码记录不一致。", 503);
    }
    private static void CheckGrant(VerifiedLicense grant, Row row, string machineId, DateTimeOffset? expires)
    {
        if (grant.LicenseId != row.LicenseId || grant.MachineId != machineId
            || grant.Kind != (row.Kind == "permanent" ? LicenseKind.Permanent : LicenseKind.Timed)
            || grant.ExpiresAtUtc != expires)
            throw new LicensingException("BackendStateConflict", "Keygen 机器许可证与兑换记录不一致。", 503);
    }
    private static void CheckDevice(string device)
    {
        if (string.IsNullOrEmpty(device) || device.Length != 64 || device.Any(ch => ch is not (>= '0' and <= '9' or >= 'A' and <= 'F')))
            throw new LicensingException("InvalidDevice", "设备标识格式错误。");
    }
    private SqliteConnection Open() { var db = new SqliteConnection(connectionString); db.Open(); return db; }
    // 授权码哈希按产品隔离：不同产品的授权码空间互不冲突。默认产品沿用历史哈希前缀，旧码继续有效。
    private static string HashCode(string productId, string code) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(productId + "|code-v1|" + code)));
    private static void Add(SqliteCommand command, string name, object? value) => command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    private static string? Format(DateTimeOffset? value) => value?.ToUniversalTime().ToString("O");
    private static DateTimeOffset? Date(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : DateTimeOffset.Parse(reader.GetString(index), CultureInfo.InvariantCulture);
    private static string? Text(SqliteDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetString(index);
    private static Row? Find(SqliteConnection db, SqliteTransaction? tx, string column, string value)
    {
        if (column is not ("Id" or "CodeHash" or "LicenseId")) throw new ArgumentException(nameof(column));
        using var command = db.CreateCommand(); command.Transaction = tx;
        command.CommandText = $"SELECT Id,CodeHint,ProductId,Kind,Days,LicenseId,DeviceId,MachineId,ActivatedAt,ExpiresAt,Certificate,Suspended,CreatedAt FROM Codes WHERE {column}=$value";
        Add(command, "$value", value);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Read(reader) : null;
    }
    private static Row Read(SqliteDataReader reader) => new(reader.GetString(0), reader.GetString(1),
        reader.IsDBNull(2) ? string.Empty : reader.GetString(2), reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetInt32(4), reader.GetString(5), Text(reader, 6), Text(reader, 7),
        Date(reader, 8), Date(reader, 9), Text(reader, 10), reader.GetBoolean(11), Date(reader, 12)!.Value);
    private sealed record Row(string Id, string Hint, string ProductId, string Kind, int? Days, string LicenseId, string? DeviceId,
        string? MachineId, DateTimeOffset? ActivatedAt, DateTimeOffset? ExpiresAt, string? Certificate, bool Suspended, DateTimeOffset CreatedAt)
    {
        internal CodeSummary Summary => new(Id, Hint, Kind, Days, LicenseId, DeviceId, ActivatedAt, ExpiresAt, Suspended, CreatedAt, ProductId);
    }
}
