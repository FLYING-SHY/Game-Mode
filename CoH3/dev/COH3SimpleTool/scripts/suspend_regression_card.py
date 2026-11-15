"""Suspend the regression test card."""
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

    body = json.dumps({"suspended": True}).encode()
    result = request("api/admin/codes/e047f47a1e7a490097022f1a73de3d6d/suspension", body, {"Content-Type": "application/json", "X-CSRF-TOKEN": csrf})
    print(result)


if __name__ == "__main__":
    main()
