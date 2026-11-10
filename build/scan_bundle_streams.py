#!/usr/bin/env python3
"""Scan a .NET single-file bundle for embedded deflate streams that decode to
PE images, then compare them against candidate assemblies.

The compressed streams can start at ANY byte offset; the anchor is that the
first two DECOMPRESSED bytes of a managed assembly are 'MZ'. Uncompressed
assemblies are also checked via a plain byte search.
"""
import argparse
import hashlib
import zlib
from pathlib import Path

MZ = b"MZ"
MAX_OUTPUT = 80 * 1024 * 1024


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest().upper()


def scan_streams(data: bytes):
    n = len(data)
    view = memoryview(data)
    for i in range(n - 16):
        try:
            d = zlib.decompressobj(-15)
            head = d.decompress(view[i:i + 256], 2)
            if len(head) < 2 or head != MZ:
                continue
            body = head + d.decompress(d.unconsumed_tail + view[i + 256:], MAX_OUTPUT)
            if not d.eof:
                continue
        except zlib.error:
            continue
        yield i, body


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--bundle", required=True)
    parser.add_argument("--candidates", nargs="+", required=True,
                        help="DLL paths to look for inside the bundle")
    parser.add_argument("--expect-absent", nargs="*", default=[],
                        help="candidate labels expected NOT to be embedded")
    args = parser.parse_args()

    data = Path(args.bundle).read_bytes()
    cand = {}
    for p in args.candidates:
        path = Path(p)
        cand[f"{path.parent.name}/{path.name}"] = path.read_bytes()
    cand_sha = {label: sha256(b) for label, b in cand.items()}
    print(f"bundle: {len(data):,} bytes  sha256={sha256(data)}", flush=True)

    found = {}
    streams = 0
    for offset, body in scan_streams(data):
        streams += 1
        digest = sha256(body)
        print(f"stream @0x{offset:X}: {len(body):,} bytes PE sha256={digest}", flush=True)
        for label, want in cand_sha.items():
            if digest == want:
                found[label] = (offset, digest)
    print(f"valid MZ streams found: {streams}", flush=True)

    ok = True
    for label, blob in cand.items():
        raw = data.find(blob)
        absent_ok = label in args.expect_absent
        if label in found:
            off, digest = found[label]
            state = "MATCH" if not absent_ok else "UNEXPECTED"
            ok &= not absent_ok
            print(f"{state}  {label}: deflate stream @0x{off:X} sha256={digest}"
                  f"{' (also raw bytes)' if raw >= 0 else ''}")
        else:
            state = "ABSENT" if absent_ok else "MISSING"
            ok &= absent_ok
            print(f"{state} {label}: no stream matches sha256={cand_sha[label]}"
                  f"{' — but raw bytes present!' if raw >= 0 else ''}")

    print("verdict:", "as expected" if ok else "UNEXPECTED composition")


if __name__ == "__main__":
    main()
