"""Look up the regression card by suffix and dump its admin record (no complete code)."""
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
    suffix = sys.argv[2]
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

    result = json.loads(request("api/admin/codes?search=" + suffix, None, {"X-CSRF-TOKEN": csrf}))
    items = result.get("items", [])
    out = []
    for item in items:
        out.append({k: v for k, v in item.items() if k not in {"code", "codeHash"}})
    print(json.dumps({"count": len(items), "codes": out}, ensure_ascii=True, default=str))


if __name__ == "__main__":
    main()
