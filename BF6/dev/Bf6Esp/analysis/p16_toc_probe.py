#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""p16_toc_probe.py —— 探 Data/layout.toc 的编码方式。

思路（每步都留证据）：
  1) 打印头部结构（前 64 字节 + 可能的 u32 长度字段）。
  2) 全文件找常见压缩/加密魔数（zlib / gzip / lz4 / zstd / deflate / PK / XZ）。
  3) 逐字节单字节 XOR（0x00..0xFF）在前 64KB 上试 zlib/deflate 解压。
  4) 试"跳过头部若干偏移后 zlib 解压"。
  5) 统计字节直方图熵，判断是否加密。
"""
import io
import json
import os
import zlib

TOC = r"G:\SteamLibrary\steamapps\common\Battlefield 6\Data\layout.toc"

MAGICS = {
    b"\x78\x9c": "zlib-default",
    b"\x78\x01": "zlib-no-comp",
    b"\x78\xda": "zlib-best",
    b"\x1f\x8b": "gzip",
    b"\xfd\x37\x7a\x58\x5a": "xz",
    b"\x04\x22\x4d\x18": "lz4",
    b"\x28\xb5\x2f\xfd": "zstd",
    b"PK\x03\x04": "zip",
    b"DFLT": "frostbite-dflt",
    b"chunk": "frostbite-chunk",
}


def entropy(blob):
    hist = [0] * 256
    for b in blob:
        hist[b] += 1
    n = float(len(blob))
    import math
    return -sum((c / n) * math.log(c / n, 2) for c in hist if c)


def main():
    blob = io.open(TOC, "rb").read()
    print("size:", len(blob))
    head = blob[:64]
    print("head hex:", " ".join("%02X" % b for b in head))
    print("head asc:", "".join(chr(b) if 0x20 <= b < 0x7F else "." for b in head))
    import struct
    for i in range(0, 16, 4):
        print("  u32@%d = %d (0x%08X)" % (i, struct.unpack_from("<I", blob, i)[0], struct.unpack_from("<I", blob, i)[0]))

    print("entropy(whole):", "%.3f" % entropy(blob))

    print("=== magic scan ===")
    for magic, label in MAGICS.items():
        idx = blob.find(magic)
        print("  %-18s %s" % (label, idx))

    print("=== raw zlib/deflate attempts ===")
    for skip in range(0, 64):
        for wbits in (15, -15, 47):
            try:
                out = zlib.decompress(blob[skip:], wbits)
                print("  HIT skip=%d wbits=%d out=%d" % (skip, wbits, len(out)))
                print("  head:", out[:80])
                open(r"F:\WorkSpace\Game\BF6\dev\Bf6Esp\analysis\logs\toc_dec_%d.bin" % skip, "wb").write(out)
                raise SystemExit(0)
            except zlib.error:
                pass
            except Exception:
                pass

    print("=== single-byte XOR + zlib ===")
    sample = blob[:65536]
    for key in range(256):
        x = bytes(b ^ key for b in sample)
        if x[:2] in (b"\x78\x9c", b"\x78\x01", b"\x78\xda") or x[:3] == b"\x1f\x8b\x08":
            print("  candidate xor key=0x%02X head=%s" % (key, " ".join("%02X" % b for b in x[:8])))
            try:
                out = zlib.decompress(x)
                print("    decompressed:", len(out))
            except Exception as exc:
                print("    decompress failed:", exc)

    print("=== DONE ===")


if __name__ == "__main__":
    main()
