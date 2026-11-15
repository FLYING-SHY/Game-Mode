"""Read-only admin check: recent audit events and active sessions."""
from __future__ import annotations

import html
import http.cookiejar
import json
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


def main() -> None:
    config = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
    base = str(config["ListenUrl"]).rstrip("/") + "/"
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

    def request(path: str, data: bytes | None = None, headers: dict | None = None) -> str:
        req = urllib.request.Request(urllib.parse.urljoin(base, path), data=data, headers=headers or {})
        with opener.open(req, timeout=15) as response:
            return response.read().decode("utf-8-sig")

    login = request("admin/login")
    field = re.search(r'name=__RequestVerificationToken\s+value="([^"]+)"', login)
    form = urllib.parse.urlencode({
        "token": config["AdminToken"],
        "__RequestVerificationToken": html.unescape(field.group(1)),
    }).encode()
    page = request("admin/login", form, {"Content-Type": "application/x-www-form-urlencoded"})
    csrf = html.unescape(re.search(r'<meta\s+name="csrf-token"\s+content="([^"]+)"', page).group(1))

    sessions = json.loads(request("api/admin/sessions?activeOnly=true", None, {"X-CSRF-TOKEN": csrf}))
    audit = json.loads(request("api/admin/audit?page=1&pageSize=40", None, {"X-CSRF-TOKEN": csrf}))
    items = audit.get("items", audit if isinstance(audit, list) else [])
    out = []
    for item in items:
        # keep only coarse fields; never echo codes/tokens if present
        out.append({k: item.get(k) for k in ("at", "action", "result", "codeId", "codeSuffix", "kind") if k in item})
    print(json.dumps({"activeSessions": sessions if isinstance(sessions, int) else len(sessions.get("items", [])) if isinstance(sessions, dict) else sessions,
                      "activeDetail": [{k: s.get(k) for k in ("codeId", "active", "startedAt", "lastSeenAt")} for s in (sessions.get("items", []) if isinstance(sessions, dict) else [])],
                      "audit": out}, ensure_ascii=True, default=str))


if __name__ == "__main__":
    main()
