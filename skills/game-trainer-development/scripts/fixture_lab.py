#!/usr/bin/env python3
"""Offline simulated memory images only; no process or real-game access. Python 3.9+."""
import argparse
import hashlib
import json
from pathlib import Path
import struct
import tempfile

PATTERN = "48 8B 81 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 85 C9"
ORIGINAL = bytes.fromhex("0F 85 11 00 00 00")
PATCH = b"\x90" * len(ORIGINAL)


def number(value):
    return int(value, 0) if isinstance(value, str) else int(value)


def digest(data):
    return hashlib.sha256(data).hexdigest()


def scan(data, pattern):
    tokens = [None if t == "??" else int(t, 16) for t in pattern.split()]
    if not tokens:
        raise ValueError("empty pattern")
    return [i for i in range(len(data) - len(tokens) + 1)
            if all(t is None or data[i + j] == t for j, t in enumerate(tokens))]


def read(regions, address, size):
    if size < 0:
        raise ValueError("negative read")
    for base, data in regions:
        if base <= address < base + len(data):
            offset = address - base
            if offset + size > len(data):
                raise ValueError("short read")
            return bytes(data[offset:offset + size])
    raise ValueError("address outside image")


def resolve(regions, module_base, candidate):
    address = struct.unpack("<Q", read(regions, module_base + number(candidate["root_rva"]), 8))[0]
    for offset in candidate["ptr_offsets"]:
        address = struct.unpack("<Q", read(regions, address + number(offset), 8))[0]
    return address + number(candidate["health_offset"])


def patch_model():
    image = bytearray(ORIGINAL)
    saved = bytes(image)
    image[:] = PATCH
    enabled = bytes(image) == PATCH
    if image == PATCH:
        image[:] = saved
    restored = bytes(image) == ORIGINAL
    image[:] = PATCH
    image[0] = 0xCC  # A second writer changed our installed bytes.
    before = bytes(image)
    conflict = image != PATCH
    if not conflict:
        image[:] = saved
    return {"enabled": enabled, "disabled_restored": restored,
            "restore_conflict": bool(conflict), "conflict_preserved": bytes(image) == before,
            "original_hex": ORIGINAL.hex(), "installed_hex": PATCH.hex(),
            "conflicting_hex": before.hex()}


def create(directory):
    directory.mkdir(parents=True, exist_ok=True)
    manifest = {"schema": 1, "model": "offline-simulated-native64", "pointer_size": 8,
                "byte_order": "little", "pattern": PATTERN, "sessions": []}
    for build, root, link, field, module_base in [
            ("A", 0x180, 0x20, 0x38, 0x140000000),
            ("B", 0x240, 0x30, 0x58, 0x180000000)]:
        for session in range(2):
            sid = f"{build}{session}"
            heap_base = 0x200000000 + (ord(build) - 65) * 0x1000000 + session * 0x200000
            manager, actor = heap_base + 0x100, heap_base + 0x400 + session * 0x80
            module = bytearray(b"\xCC" * 0x400)
            module[:4] = b"SIM\0"  # Deliberately not a loadable PE or ELF.
            struct.pack_into("<Q", module, root, manager)
            code = b"\x48\x8B\x81" + struct.pack("<i", field)
            code += b"\x48\x8B\x0D" + struct.pack("<i", root - (0x40 + 14))
            code += b"\x48\x85\xC9"
            module[0x40:0x40 + len(code)] = code
            module[0x80:0x86] = ORIGINAL
            entry = {"id": sid, "build": build, "module_base": module_base,
                     "heap_base": heap_base, "module_size": len(module), "heap_size": 0x800,
                     "pointer_root": {"rva": root, "address": module_base + root},
                     "field_layout": {"ptr_offsets": [link], "health_offset": field},
                     "health_address": actor + field, "decoy_addresses": [heap_base + 0x600, heap_base + 0x620],
                     "expected_values": [100, 73, 91], "frames": [], "aob": {}}
            for mode in ("unique", "zero", "multi"):
                variant = bytearray(module)
                if mode == "zero":
                    variant[0x40] = 0x49
                if mode == "multi":
                    variant[0xA0:0xA0 + len(code)] = code
                    struct.pack_into("<i", variant, 0xA0 + 10, root - (0xA0 + 14))
                name = f"{sid}.{mode}.module.bin"
                (directory / name).write_bytes(variant)
                entry["aob"][mode] = {"file": name, "sha256": digest(variant)}
            for frame, health in enumerate(entry["expected_values"]):
                heap = bytearray(0x800)
                struct.pack_into("<Q", heap, 0x100 + link, actor)
                struct.pack_into("<i", heap, actor - heap_base + field, health)
                struct.pack_into("<i", heap, 0x600, 100)  # Constant decoy.
                struct.pack_into("<i", heap, 0x620, [100, 91, 73][frame])
                name = f"{sid}.frame{frame}.heap.bin"
                (directory / name).write_bytes(heap)
                entry["frames"].append({"file": name, "sha256": digest(heap)})
            manifest["sessions"].append(entry)
    (directory / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    return manifest


def load_image(directory, spec):
    name = spec["file"]
    if Path(name).name != name or "/" in name or "\\" in name:
        raise ValueError("image filename must be local basename")
    data = (directory / name).read_bytes()
    if digest(data) != spec["sha256"]:
        raise ValueError("image sha256 mismatch: " + name)
    return data


def verify(directory, result):
    manifest = json.loads((directory / "manifest.json").read_text(encoding="utf-8"))
    if result.get("schema") != 1 or not isinstance(result.get("candidates"), list):
        raise ValueError("expected schema=1 and candidates array")
    reports = []
    for candidate in result["candidates"]:
        sessions = [s for s in manifest["sessions"] if s["build"] == candidate["build"]]
        required = {s["id"] for s in sessions}
        selected = candidate["sessions"]
        report = {"build": candidate["build"], "passed": False, "sessions": []}
        try:
            if len(required) < 2 or len(selected) != len(set(selected)) or set(selected) != required:
                raise ValueError("select all distinct sessions for this build (at least two)")
            for session in sessions:
                module = load_image(directory, session["aob"]["unique"])
                values, decoys = [], [[], []]
                address = None
                for frame in session["frames"]:
                    heap = load_image(directory, frame)
                    regions = [(session["module_base"], module), (session["heap_base"], heap)]
                    address = resolve(regions, session["module_base"], candidate)
                    values.append(struct.unpack("<i", read(regions, address, 4))[0])
                    for i, decoy in enumerate(session["decoy_addresses"]):
                        decoys[i].append(struct.unpack("<i", read(regions, decoy, 4))[0])
                observed = candidate.get("resolved_addresses", {}).get(session["id"], address)
                ok = (number(observed) == address == session["health_address"]
                      and values == session["expected_values"] and values not in decoys)
                report["sessions"].append({"id": session["id"], "address": address,
                                           "values": values, "decoy_values": decoys, "passed": ok})
            report["passed"] = all(s["passed"] for s in report["sessions"])
        except (KeyError, ValueError, TypeError, struct.error) as error:
            report["error"] = str(error)
        reports.append(report)
    aob = []
    for session in manifest["sessions"]:
        for mode, expected in (("unique", 1), ("zero", 0), ("multi", 2)):
            data = load_image(directory, session["aob"][mode])
            hits = scan(data, PATTERN)
            roots = [session["module_base"] + hit + 14 + struct.unpack_from("<i", data, hit + 10)[0]
                     for hit in hits]
            aob.append({"session": session["id"], "mode": mode, "count": len(hits),
                        "status": "unique" if len(hits) == 1 else "zero" if not hits else "multiple",
                        "decoded_roots": roots, "accepted": len(hits) == 1,
                        "passed": len(hits) == expected and all(r == session["pointer_root"]["address"] for r in roots)})
    patch = patch_model()
    builds = [c["build"] for c in result["candidates"]]
    passed = (sorted(builds) == ["A", "B"] and all(r["passed"] for r in reports)
              and all(r["passed"] for r in aob) and all(patch[k] for k in
                  ("enabled", "disabled_restored", "restore_conflict", "conflict_preserved")))
    return {"schema": 1, "model": manifest["model"], "passed": passed,
            "candidates": reports, "aob": aob, "patch_model": patch}


def oracle_candidate(manifest):
    # Test oracle only. Reading the manifest is not reverse engineering.
    return {"schema": 1, "candidates": [
        {"build": build, "root_rva": s["pointer_root"]["rva"], **s["field_layout"],
         "sessions": [x["id"] for x in manifest["sessions"] if x["build"] == build]}
        for build, s in ((b, next(x for x in manifest["sessions"] if x["build"] == b)) for b in ("A", "B"))]}


def self_test():
    checks = {}
    with tempfile.TemporaryDirectory(prefix="fixture-lab-") as temp:
        directory = Path(temp)
        manifest = create(directory)
        good = oracle_candidate(manifest)
        report = verify(directory, good)
        checks["two_builds_two_sessions_decoys"] = report["passed"]
        checks["aob_0_1_multiple_rip"] = all(x["passed"] for x in report["aob"])
        checks["normal_enable_disable"] = report["patch_model"]["enabled"] and report["patch_model"]["disabled_restored"]
        checks["restore_conflict"] = report["patch_model"]["restore_conflict"] and report["patch_model"]["conflict_preserved"]
        checks["last_byte_boundary"] = read([(10, b"ab")], 11, 1) == b"b"
        for name, address, size in (("short_read", 11, 2), ("outside_image", 12, 1), ("negative_size", 10, -1)):
            try:
                read([(10, b"ab")], address, size)
                checks[name] = False
            except ValueError:
                checks[name] = True
        stale = json.loads(json.dumps(good))
        address = manifest["sessions"][0]["health_address"]
        stale["candidates"][0]["resolved_addresses"] = {"A0": address, "A1": address}
        checks["stale_session"] = not verify(directory, stale)["passed"]
        bad = json.loads(json.dumps(good))
        bad["candidates"][0]["health_offset"] = 0x200  # Points at constant decoy in A0.
        checks["decoy_rejected"] = not verify(directory, bad)["passed"]
        checks["deterministic"] = manifest == create(directory)
    return {"passed": all(checks.values()), "checks": checks}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("self-test")
    make = sub.add_parser("create")
    make.add_argument("--output", type=Path, required=True)
    check = sub.add_parser("verify")
    check.add_argument("--directory", type=Path, required=True)
    check.add_argument("--result", type=Path, required=True, help="candidate JSON input; report goes to stdout")
    args = parser.parse_args()
    try:
        if args.command == "create":
            manifest = create(args.output)
            output = {"model": manifest["model"], "sessions": len(manifest["sessions"]), "created": True}
        elif args.command == "verify":
            output = verify(args.directory, json.loads(args.result.read_text(encoding="utf-8-sig")))
        else:
            output = self_test()
    except (OSError, ValueError, KeyError, TypeError, struct.error) as error:
        output = {"passed": False, "error": str(error)}
    print(json.dumps(output, indent=2))
    return 0 if output.get("passed", output.get("created", False)) else 1


if __name__ == "__main__":
    raise SystemExit(main())
