"""Dump recent audit with timestamps."""
from __future__ import annotations

import datetime as dt
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

    audit = json.loads(request("api/admin/audit?page=1&pageSize=24", None, {"X-CSRF-TOKEN": csrf}))
    rows = []
    for item in audit.get("items", []):
        t = dt.datetime.fromtimestamp(item.get("occurredAtUnix", 0)).strftime("%m-%d %H:%M:%S")
        rows.append({"t": t, "action": item.get("action"), "codeId": str(item.get("codeId"))[:8], "details": item.get("details")})
    print(json.dumps(rows, ensure_ascii=True, default=str))


if __name__ == "__main__":
    main()
