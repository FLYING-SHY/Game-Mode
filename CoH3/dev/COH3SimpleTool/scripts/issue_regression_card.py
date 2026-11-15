"""Issue one permanent regression card for COH3SimpleTool live activation.

Complete code is written to the private file and placed on the clipboard only;
it is never printed to stdout/conversation.
"""
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
    config_path = Path(sys.argv[1])
    out_path = Path(sys.argv[2])
    config = json.loads(config_path.read_text(encoding="utf-8-sig"))
    base = str(config["ListenUrl"]).rstrip("/") + "/"
    if urllib.parse.urlsplit(base).hostname not in {"127.0.0.1", "localhost", "::1"}:
        raise SystemExit("restricted to loopback")
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

    def request(path: str, data: bytes | None = None, headers: dict | None = None) -> str:
        req = urllib.request.Request(urllib.parse.urljoin(base, path), data=data, headers=headers or {})
        with opener.open(req, timeout=15) as response:
            return response.read().decode("utf-8-sig")

    login = request("admin/login")
    token_field = re.search(r'name=__RequestVerificationToken\s+value="([^"]+)"', login)
    if not token_field:
        raise RuntimeError("anti-forgery field missing")
    form = urllib.parse.urlencode({
        "token": config["AdminToken"],
        "__RequestVerificationToken": html.unescape(token_field.group(1)),
    }).encode()
    page = request("admin/login", form, {"Content-Type": "application/x-www-form-urlencoded"})
    csrf_match = re.search(r'<meta\s+name="csrf-token"\s+content="([^"]+)"', page)
    if not csrf_match:
        raise RuntimeError("admin auth failed")
    csrf = html.unescape(csrf_match.group(1))

    body = json.dumps({
        "kind": "permanent",
        "quantity": 1,
        "note": "COH3SimpleTool live regression 20261115",
    }).encode()
    result = json.loads(request("api/admin/codes", body, {"Content-Type": "application/json", "X-CSRF-TOKEN": csrf}))
    item = result["items"][0]
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text(item["code"] + "\n", encoding="ascii")
    print(json.dumps({
        "id": item["id"],
        "codeSuffix": item["codeSuffix"],
        "kind": item.get("kind"),
        "savedTo": str(out_path),
        "note": "complete code saved to private file only",
    }, ensure_ascii=True))


if __name__ == "__main__":
    main()
