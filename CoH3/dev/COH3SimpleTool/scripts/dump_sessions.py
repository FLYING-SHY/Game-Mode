"""Dump session history for given codeIds plus raw audit field names."""
from __future__ import annotations

import html
import http.cookiejar
import json
import re
import sys
import urllib.parse
import urllib.request
from pathlib import Path


def main() -> None:
    config = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
    code_ids = sys.argv[2:]
    base = str(config["ListenUrl"]).rstrip("/") + "/"
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

    def request(path: str, data: bytes | None = None, headers: dict | None = None) -> str:
        req = urllib.request.Request(urllib.parse.urljoin(base, path), data=data, headers=headers or {})
        with opener.open(req, timeout=15) as response:
            return response.read().decode("utf-8-sig")

    login = request("admin/login")
    field = re.search(r'name=__RequestVerificationToken\s+value="([^"]+)"', login)
    form = urllib.parse.urlencode({"token": config["AdminToken"], "__RequestVerificationToken": html.unescape(field.group(1))}).encode()
    page = request("admin/login", form, {"Content-Type": "application/x-www-form-urlencoded"})
    csrf = html.unescape(re.search(r'<meta\s+name="csrf-token"\s+content="([^"]+)"', page).group(1))
    hdr = {"X-CSRF-TOKEN": csrf}

    out = {}
    for code_id in code_ids:
        data = json.loads(request("api/admin/sessions?codeId=" + code_id, None, hdr))
        sessions = data.get("items", data if isinstance(data, list) else [])
        out[code_id] = [{k: s.get(k) for k in ("id", "active", "startedAt", "startedAtUnix", "lastSeenAt", "lastSeenAtUnix", "endedAt", "endedAtUnix", "endReason")} for s in sessions]
    audit = json.loads(request("api/admin/audit?page=1&pageSize=3", None, hdr))
    items = audit.get("items", [])
    out["auditSampleRawKeys"] = [sorted(i.keys()) for i in items[:2]]
    print(json.dumps(out, ensure_ascii=True, default=str))


if __name__ == "__main__":
    main()
