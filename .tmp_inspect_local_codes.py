import sqlite3, json, datetime, os

db = r"F:\WorkSpace\Game\LicensingSystem\.local\hardening-v2\data\leases.db"
print("db exists:", os.path.exists(db), "size:", os.path.getsize(db))
con = sqlite3.connect(f"file:{db}?mode=ro", uri=True)
con.row_factory = sqlite3.Row
cur = con.cursor()

rows = cur.execute("""
    SELECT c.Id, c.CodeHash, c.CodeSuffix, c.Kind, c.DurationSeconds, c.Note, c.CreatedAt,
           c.CodeCipher
    FROM LicenseCodes c ORDER BY c.CreatedAt
""").fetchall()
print("LicenseCodes rows:", len(rows))

def ts(x):
    return datetime.datetime.utcfromtimestamp(x).strftime("%Y-%m-%d %H:%M:%S")

for r in rows:
    cipher = r["CodeCipher"]
    if isinstance(cipher, (bytes, bytearray)):
        preview = "BYTES:" + bytes(cipher[:12]).hex()
        clen = len(cipher)
    else:
        s = str(cipher)
        preview = s[:24]
        clen = len(s)
    print(f"{r['Id'][:8]} created={ts(r['CreatedAt'])} kind={r['Kind']} dur={r['DurationSeconds']} suffix={r['CodeSuffix']} cipherLen={clen} cipher={preview!r} note={r['Note']!r}")

print("--- sessions referencing codes ---")
sess = cur.execute("SELECT CodeId, COUNT(*) c, MAX(LastSeenAt) last FROM LeaseSessions GROUP BY CodeId").fetchall()
for s in sess:
    print(s["CodeId"][:8], s["c"], ts(s["last"]) if s["last"] else None)

print("--- activation requests ---")
try:
    ar = cur.execute("SELECT COUNT(*) FROM ActivationRequests").fetchone()
    print("ActivationRequests:", ar[0])
except Exception as e:
    print("ActivationRequests err:", e)

print("--- tables ---")
for t in cur.execute("SELECT name FROM sqlite_master WHERE type='table'").fetchall():
    print(" ", t[0], cur.execute(f"SELECT COUNT(*) FROM {t[0]}").fetchone()[0])
con.close()
