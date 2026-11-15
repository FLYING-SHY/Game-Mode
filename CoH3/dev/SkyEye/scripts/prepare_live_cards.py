"""Issue isolated live HTTP fixtures without printing credentials or complete codes."""
from __future__ import annotations

import argparse
import datetime as dt
import html
import http.cookiejar
import json
import re
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--private-config", required=True, type=Path)
    parser.add_argument("--private-out", required=True, type=Path)
    parser.add_argument("--evidence-out", required=True, type=Path)
    args = parser.parse_args()
    config = json.loads(args.private_config.read_text(encoding="utf-8-sig"))
    base = str(config["ListenUrl"]).rstrip("/") + "/"
    parsed = urllib.parse.urlsplit(base)
    if parsed.hostname not in {"127.0.0.1", "localhost", "::1"}:
        raise SystemExit("Live fixture issuance is restricted to the configured loopback server.")
    if args.private_out.exists() or args.evidence_out.exists():
        raise SystemExit("Fixture outputs already exist; choose new output paths.")
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

    def request(path: str, data: bytes | None = None, headers: dict | None = None) -> str:
        req = urllib.request.Request(urllib.parse.urljoin(base, path), data=data, headers=headers or {})
        try:
            with opener.open(req, timeout=15) as response:
                return response.read().decode("utf-8-sig")
        except urllib.error.HTTPError as error:
            raise RuntimeError(f"Live fixture request failed with HTTP {error.code}.") from None

    login = request("admin/login")
    match = re.search(r'name=__RequestVerificationToken\s+value="([^"]+)"', login)
    if not match:
        raise RuntimeError("The login anti-forgery field was not found.")
    form = urllib.parse.urlencode({"token": config["AdminToken"], "__RequestVerificationToken": html.unescape(match.group(1))}).encode()
    page = request("admin/login", form, {"Content-Type": "application/x-www-form-urlencoded"})
    match = re.search(r'<meta\s+name="csrf-token"\s+content="([^"]+)"', page)
    if not match:
        raise RuntimeError("Authenticated administrator page was not returned.")
    csrf = html.unescape(match.group(1))
    fixtures = [
        ("protocol-permanent", "permanent", None),
        ("protocol-timed", "timed", 3600),
        ("coordinator", "permanent", None),
        ("network", "permanent", None),
        ("observe-kick", "permanent", None),
        ("observe-block", "permanent", None),
        ("observe-unbind", "timed", 3600),
        ("observe-expiry", "timed", 8),
    ]
    args.private_out.mkdir(parents=True)
    rows = []
    for name, kind, seconds in fixtures:
        body = json.dumps({"kind": kind, "durationSeconds": seconds, "quantity": 1, "note": "E2E-20261113-" + name}).encode()
        result = json.loads(request("api/admin/codes", body, {"Content-Type": "application/json", "X-CSRF-TOKEN": csrf}))
        item = result["items"][0]
        code_file = args.private_out / (name + ".txt")
        code_file.write_text(item["code"] + "\n", encoding="utf-8")
        rows.append({"name": name, "id": item["id"], "codeSuffix": item["codeSuffix"], "kind": kind, "durationSeconds": seconds, "codeFile": str(code_file)})
    evidence = {"issuedAt": dt.datetime.now(dt.timezone.utc).isoformat(), "baseUrl": base, "transport": "real HTTP administrator login + CSRF + issue API", "containsCompleteCodes": False, "fixtures": rows}
    args.evidence_out.parent.mkdir(parents=True, exist_ok=True)
    args.evidence_out.write_text(json.dumps(evidence, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"issued": len(rows), "evidence": str(args.evidence_out), "privateCodeDirectory": str(args.private_out)}, ensure_ascii=True))


if __name__ == "__main__":
    main()
