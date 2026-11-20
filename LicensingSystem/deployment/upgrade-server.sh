#!/usr/bin/env bash
# 就地升级；显式传入映射时才重新加密迁移卡密。保留数据库、配置和签名私钥。
# bash upgrade-server.sh SERVER_ZIP [LEGACY_CODE_MAP_JSON]
set -euo pipefail
umask 077

APP_DIR=/opt/licensing
DOTNET=/opt/dotnet/dotnet
CONFIG=/opt/licensing/server.local.json
ZIP=${1:-/root/licensing-upgrade/SkyEye-HardenedV2-Server.zip}
MAP=${2:-}

as_user() {
    runuser -u licensing -- env DOTNET_ROOT=/opt/dotnet LICENSING_SERVER_CONFIG="$CONFIG" "$@"
}

test "$(id -u)" -eq 0
for cmd in python3 runuser systemctl tar curl; do command -v "$cmd" >/dev/null; done
for file in "$ZIP" "$CONFIG" "$DOTNET" "$APP_DIR/data/leases.db"; do
    test -f "$file" || { echo "Missing required file: $file" >&2; exit 1; }
done
if [ -n "$MAP" ] && [ ! -f "$MAP" ]; then
    echo "Missing migration map: $MAP" >&2; exit 1
fi
STAGE=$(mktemp -d /opt/licensing-upgrade.XXXXXXXX)
CHANGED=0
cleanup() {
    result=$?
    trap - EXIT
    if [ "$result" -ne 0 ]; then
        if [ "$CHANGED" -eq 1 ]; then
            echo "Upgrade failed; restoring previous program files." >&2
            systemctl stop licensing || true
            tar -xzf "$STAGE/previous-program.tar.gz" -C "$APP_DIR" || true
            systemctl start licensing || true
        fi
        echo "Upgrade stopped. Diagnostic files retained in $STAGE" >&2
    else
        case "$(realpath "$STAGE")" in /opt/licensing-upgrade.*) rm -rf -- "$STAGE" ;; esac
    fi
    exit "$result"
}
trap cleanup EXIT

echo '1/5 Validate archive before stopping the service'
python3 - "$ZIP" "$STAGE/new" <<'PY'
import hashlib, json, stat, sys, zipfile
from pathlib import PurePosixPath
with zipfile.ZipFile(sys.argv[1]) as archive:
    entries = archive.infolist()
    names = [item.filename for item in entries]
    if len(names) != len(set(names)):
        raise SystemExit('Duplicate archive entries')
    for item in entries:
        path = PurePosixPath(item.filename)
        if (path.is_absolute() or '..' in path.parts or '\\' in item.filename
                or ':' in item.filename or not path.parts
                or path.parts[0].lower() in {'data', 'server.local.json', 'server.json'}
                or stat.S_ISLNK(item.external_attr >> 16)):
            raise SystemExit('Unexpected archive path: ' + item.filename)
    manifest = json.loads(archive.read('PACKAGE-MANIFEST.json'))
    files = {item['path']: item for item in manifest['files']}
    if len(files) != len(manifest['files']):
        raise SystemExit('Duplicate manifest entries')
    payload = {item.filename for item in entries if not item.is_dir()} - {'PACKAGE-MANIFEST.json'}
    if set(files) != payload or 'Licensing.Server.dll' not in files:
        raise SystemExit('Manifest does not match archive contents')
    for name, item in files.items():
        content = archive.read(name)
        if len(content) != item['bytes'] or hashlib.sha256(content).hexdigest() != item['sha256']:
            raise SystemExit('Archive checksum mismatch: ' + name)
    archive.extractall(sys.argv[2])
print('Archive checksums verified')
PY

echo '2/5 Back up database, keys, configuration and previous program'
/usr/local/bin/licensing-backup.sh
tar -czf "$STAGE/previous-program.tar.gz" --exclude=./data --exclude=./server.local.json -C "$APP_DIR" .

echo '3/5 Stop service and replace verified program files'
CHANGED=1
systemctl stop licensing
cp -a "$STAGE/new/." "$APP_DIR/"
chown -R licensing:licensing "$APP_DIR"
chmod 700 "$APP_DIR/data" "$APP_DIR/data/keys"
chmod 600 "$APP_DIR/data/leases.db" "$APP_DIR/data/keys/"*
chmod 640 "$CONFIG"

echo '4/5 Run legacy re-encryption only when a migration map was supplied'
if [ -n "$MAP" ]; then
    # The licensing user cannot traverse /root; copy an uploaded map to its private data directory.
    if [ "$(realpath "$MAP")" != "$APP_DIR/data/legacy-code-map.json" ]; then
        install -o licensing -g licensing -m 600 "$MAP" "$APP_DIR/data/legacy-code-map.json"
    fi
    MAP="$APP_DIR/data/legacy-code-map.json"
    chown licensing:licensing "$MAP"
    chmod 600 "$MAP"
    REPORT=$(as_user "$DOTNET" "$APP_DIR/Licensing.Server.dll" --reprotect-codes "$MAP")
    printf '%s\n' "$REPORT"
    REPORT="$REPORT" python3 - <<'PY'
import os, re
if not re.search(r'^repaired=\d+ alreadyReadable=\d+ missing=0 mismatched=0 total=\d+$', os.environ['REPORT'], re.M):
    raise SystemExit('Migration requires review: missing or mismatched codes')
PY
else
    echo 'No migration map supplied; existing license records left unchanged.'
fi

echo '5/5 Start service and verify health over loopback and public HTTPS'
systemctl start licensing
for attempt in $(seq 1 30); do
    if curl --connect-timeout 2 --max-time 3 -fsS http://127.0.0.1:47835/health >/dev/null 2>&1; then break; fi
    sleep 1
done
curl --connect-timeout 3 --max-time 10 -fsS http://127.0.0.1:47835/health
echo
curl --connect-timeout 5 --max-time 15 -fsS https://124.220.147.56/health
echo
systemctl is-active --quiet licensing
echo 'Upgrade completed. Open https://124.220.147.56/admin to verify the code list.'
