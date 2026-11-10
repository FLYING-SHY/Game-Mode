#!/usr/bin/env python3
"""Prove which assemblies a .NET single-file bundle actually embeds.

Locates the bundle marker by diffing the bundled EXE against the staged
(non-bundled) apphost, reads the manifest offset the host would use, parses
the manifest, inflates the two managed entries, and compares SHA-256 against
the protected assemblies.
"""
import argparse
import hashlib
import json
import struct
import zlib
from pathlib import Path

DIFFERENCE_WINDOW = 8 * 1024 * 1024  # apphost prefix is compared up to this


def read_seven(data: bytes, pos: int) -> tuple[int, int]:
    result = 0
    shift = 0
    while True:
        byte = data[pos]
        pos += 1
        result |= (byte & 0x7F) << shift
        if not byte & 0x80:
            return result, pos
        shift += 7


def parse_manifest(data: bytes, header_offset: int) -> list[dict] | None:
    """Try the two plausible field orders; return entries when sane."""
    for variant in ("id-first", "count-first"):
        pos = header_offset
        try:
            major, pos = read_seven(data, pos)
            minor, pos = read_seven(data, pos)
            if not (1 <= major <= 8 and 0 <= minor <= 4):
                continue
            if variant == "id-first":
                id_len, pos = read_seven(data, pos)
                if id_len > 256:
                    continue
                pos += id_len
                count, pos = read_seven(data, pos)
            else:
                count, pos = read_seven(data, pos)
                id_len, pos = read_seven(data, pos)
                if id_len > 256:
                    continue
                pos += id_len
            if not 20 <= count <= 5000:
                continue
            entries = []
            for _ in range(count):
                offset, size, compressed = struct.unpack_from("<qqq", data, pos)
                pos += 24
                entry_type = data[pos]
                pos += 1
                name_len, pos = read_seven(data, pos)
                name = data[pos:pos + name_len].decode("utf-8", "strict")
                pos += name_len
                if not 0x400 <= offset < len(data):
                    return None
                if not 0 <= size < len(data) or not 0 <= compressed < len(data):
                    return None
                if entry_type > 5 or not name or any(c not in
                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-/" for c in name):
                    return None
                entries.append({"offset": offset, "size": size,
                                "compressed": compressed, "type": entry_type, "name": name})
            return entries
        except (IndexError, UnicodeDecodeError, struct.error):
            continue
    return None


def locate_marker(bundled: bytes, apphost: bytes) -> tuple[int, int]:
    """Return (marker_index, manifest_offset) from where the two images differ."""
    limit = min(len(apphost), len(bundled), DIFFERENCE_WINDOW)
    runs: list[tuple[int, int]] = []
    start = None
    for i in range(limit):
        if bundled[i] != apphost[i]:
            if start is None:
                start = i
        elif start is not None:
            runs.append((start, i))
            start = None
    if start is not None:
        runs.append((start, limit))
    if not runs:
        raise SystemExit("The bundled image is identical to the staged apphost; no marker patch found.")
    # The bundler patches exactly one 8-byte little-endian offset after the marker.
    for run_start, run_end in runs:
        if run_end - run_start != 8:
            continue
        value = struct.unpack_from("<q", bundled, run_start)[0]
        if 0x400 <= value < len(bundled):
            return run_start - 32, value
    detail = ", ".join(f"0x{s:X}-0x{e:X}" for s, e in runs[:8])
    raise SystemExit(f"No single 8-byte offset patch found; differing runs: {detail}")


def extract(data: bytes, entry: dict) -> bytes:
    length = entry["compressed"] or entry["size"]
    raw = data[entry["offset"]:entry["offset"] + length]
    if entry["compressed"]:
        return zlib.decompress(raw, -15)
    return raw


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--bundle", required=True)
    parser.add_argument("--apphost", required=True)
    parser.add_argument("--protected", required=True)
    args = parser.parse_args()

    bundled = Path(args.bundle).read_bytes()
    apphost = Path(args.apphost).read_bytes()
    marker_index, header_offset = locate_marker(bundled, apphost)
    entries = parse_manifest(bundled, header_offset)
    if entries is None:
        raise SystemExit("Could not parse the bundle manifest at the patched offset.")

    protected_dir = Path(args.protected)
    report = {
        "bundle": str(Path(args.bundle).resolve()),
        "markerIndex": marker_index,
        "manifestOffset": header_offset,
        "entryCount": len(entries),
        "modules": [],
    }
    ok = True
    for module in ("CoH3FowTool.dll", "Licensing.Client.dll"):
        entry = next((e for e in entries if e["name"] == module), None)
        if entry is None:
            raise SystemExit(f"Bundle manifest does not list {module}.")
        payload = extract(bundled, entry)
        bundle_sha = hashlib.sha256(payload).hexdigest().upper()
        protected_sha = hashlib.sha256(
            (protected_dir / module).read_bytes()).hexdigest().upper()
        match = bundle_sha == protected_sha
        ok &= match
        report["modules"].append({
            "module": module,
            "bundleSha256": bundle_sha,
            "protectedSha256": protected_sha,
            "bytes": len(payload),
            "match": match,
        })
    print(json.dumps(report, indent=2))
    if not ok:
        raise SystemExit("Bundle does not embed the protected assemblies.")
    print("Bundle embeds the protected assemblies.")


if __name__ == "__main__":
    main()
