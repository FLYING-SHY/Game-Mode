"""Bounded raw lifecycle observations; no phase classification or product writes.

Only explicit PID/start identity, one QUERY_INFORMATION|VM_READ handle, and known
source-derived roots/fields. --help is offline; phase_probe_selftest.py uses fakes.
"""
import argparse
from collections import deque
from contextlib import contextmanager
import ctypes as c
from ctypes import wintypes as w
import datetime as dt
import hashlib
import json
import math
from pathlib import Path
import re
import struct
import sys
import time

MAX_SECONDS, MAX_BYTES, MAX_READS = 10.0, 2 * 1024 * 1024, 20000
RESERVE_SECONDS, RESERVE_BYTES, RESERVE_READS = 0.5, 65536, 768
MAX_SINGLE, MAX_NODES, MAX_PARENTS, MAX_CHILDREN = 65536, 384, 64, 512
GAME_SHA256 = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec"
IMAGE_SIZE = 120369152
SENTINEL, USER_END = 0xffffffff, 0x800000000000
FILETIME_EPOCH = dt.datetime(1601, 1, 1, tzinfo=dt.timezone.utc)
ROLES = {
    "arena": (102260400, 0x6187e40), "world": (98614988, 0x5e0decc),
    "cameraOwner": (102260680, 0x6187f58), "uiSingleton": (112837064, 0x6b9e078),
    "cameraCoreVtable": (64684536, 0x3db1068),
    "controllerVtable": (64683768, 0x3db0d68), "rootVtable": (64823200, 0x3dd4bf0),
    "minimapVtable": (65511760, 0x3e7b160), "tacticalVtable": (65513472, 0x3e7b810),
    "hudVtable": (65713904, 0x3eac6b0),
}
SOURCE_PINS = {
    "client/CoH3MemoryTool/GameLayout52426.cs": "77c44effbf42d9f6300cfd41e753712bed85e551b487f4db1a779df528dc8d92",
    "client/CoH3MemoryTool/PlayerInfoReader.cs": "bbb359cd59409065d8b4d00ace784243ae5e99816611bc11617e7c9ed8adba9d",
    "evidence/version-adaptation/air/air_raw_snapshot.py": "0438ebbe4b29bcae3b6d428e46ce2db0fcdf58c87d93f6058f0432dc9ff765d4",
}


def utc_now():
    return dt.datetime.now(dt.timezone.utc).isoformat(timespec="microseconds").replace("+00:00", "Z")


def start_filetime(text):
    match = re.fullmatch(r"(\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d)(?:\.(\d{1,7}))?(?:Z|\+00:00)", text)
    if not match:
        raise ValueError("expected-start-utc must be explicit UTC with at most 7 fractional digits")
    value = dt.datetime.strptime(match[1], "%Y-%m-%dT%H:%M:%S").replace(tzinfo=dt.timezone.utc)
    delta = value - FILETIME_EPOCH
    return (delta.days * 86400 + delta.seconds) * 10000000 + int((match[2] or "").ljust(7, "0"))


def filetime_utc(value):
    moment = FILETIME_EPOCH + dt.timedelta(microseconds=value // 10)
    return moment.strftime("%Y-%m-%dT%H:%M:%S.") + "%06d%dZ" % (moment.microsecond, value % 10)


def span(address, size=1):
    if not isinstance(address, int) or not 0x10000 <= address < USER_END or not 0 < size <= MAX_SINGLE or address + size > USER_END:
        raise ValueError("Invalid user address/span: address=%#x size=%d" % (address, size))
    return address


def pointer(value):
    return 0x10000 <= value < USER_END and value % 8 == 0


def relative(arena, offset):
    if not 0 <= offset < SENTINEL:
        raise ValueError("Invalid/sentinel arena offset: %#x" % offset)
    return span(arena + offset)


def unpack(raw, fmt, offset=0):
    return struct.unpack_from("<" + fmt, raw, offset)[0]


def floats(raw, offset, count):
    return list(struct.unpack_from("<%df" % count, raw, offset))


def json_floats(values):
    return [x if math.isfinite(x) else str(x) for x in values]


def failure(error):
    return {"type": type(error).__name__, "message": str(error)}


class BudgetExceeded(RuntimeError):
    pass


class SliceExceeded(RuntimeError):
    pass


class BoundedReader:
    """One read seam used by Windows memory and isolated fake-memory checks."""
    def __init__(self, memory, clock=time.monotonic, started=None):
        self.memory, self.clock = memory, clock
        self.base = memory.base
        self.started = clock() if started is None else started
        self.calls = self.requested = self.returned = 0
        self.finalizing, self.soft = False, None
        self.journal = []

    def tick(self):
        if self.clock() - self.started >= MAX_SECONDS - (0 if self.finalizing else RESERVE_SECONDS):
            raise BudgetExceeded("10-second cooperative deadline; final checks reserve 0.5 seconds")

    def read(self, address, size, field):
        span(address, size)
        self.tick()
        if self.calls + 1 > MAX_READS - (0 if self.finalizing else RESERVE_READS) or self.requested + size > MAX_BYTES - (0 if self.finalizing else RESERVE_BYTES):
            raise BudgetExceeded("2 MiB / 20000 read-attempt budget; failed attempts are charged")
        if self.soft and (self.calls + 1 > self.soft[0] or self.requested + size > self.soft[1] or self.clock() >= self.soft[2]):
            raise SliceExceeded("HUD slice: 4500 attempts / 512 KiB / 0.6 seconds")
        self.calls += 1
        self.requested += size
        entry = {"ordinal": self.calls, "field": field, "address": hex(address), "length": size,
                 "utc": utc_now(), "elapsedSeconds": self.clock() - self.started}
        self.journal.append(entry)
        try:
            raw = self.memory.read(address, size)
            self.returned += len(raw)
            entry["actualBytes"] = len(raw)
            if len(raw) != size:
                raise OSError("Short read: address=%#x requested=%d actual=%d" % (address, size, len(raw)))
            entry["status"] = "read"
            if size <= 8:
                entry["rawHex"] = raw.hex()
            self.tick()
            return raw
        except Exception as error:
            entry.update(status="failed", error=failure(error))
            raise

    def q(self, address, field):
        return unpack(self.read(address, 8, field), "Q")

    def u(self, address, field):
        return unpack(self.read(address, 4, field), "I")

    @contextmanager
    def hud_slice(self):
        self.soft = (self.calls + 4500, self.requested + 512 * 1024, self.clock() + 0.6)
        try:
            yield
        finally:
            self.soft = None

    def stats(self):
        return {"readAttempts": self.calls, "requestedBytes": self.requested,
                "returnedBytes": self.returned, "elapsedSeconds": self.clock() - self.started}


class WindowsMemory:
    """No process enumeration, VM_WRITE, VM_OPERATION, threads, calls, or UI access."""
    def __init__(self, pid, expected_start):
        if sys.platform != "win32" or c.sizeof(c.c_void_p) != 8:
            raise RuntimeError("Live capture requires 64-bit Python on Windows")
        self.handle, self.closed = None, False
        self.pid = pid
        self.k = c.WinDLL("kernel32", use_last_error=True)
        self.k.OpenProcess.argtypes, self.k.OpenProcess.restype = [w.DWORD, w.BOOL, w.DWORD], w.HANDLE
        self.k.CloseHandle.argtypes, self.k.CloseHandle.restype = [w.HANDLE], w.BOOL
        self.k.ReadProcessMemory.argtypes = [w.HANDLE, c.c_void_p, c.c_void_p, c.c_size_t, c.POINTER(c.c_size_t)]
        self.k.ReadProcessMemory.restype = w.BOOL
        self.k.GetProcessTimes.argtypes = [w.HANDLE] + [c.POINTER(w.FILETIME)] * 4
        self.k.GetProcessTimes.restype = w.BOOL
        self.k.GetExitCodeProcess.argtypes, self.k.GetExitCodeProcess.restype = [w.HANDLE, c.POINTER(w.DWORD)], w.BOOL
        self.handle = self.k.OpenProcess(0x410, False, pid)
        if not self.handle:
            raise c.WinError(c.get_last_error())
        try:
            current = self.identity()
            if current["startFiletime"] != expected_start or not current["stillActive"]:
                raise ValueError("Process identity mismatch: expectedStart=%s actual=%s" % (filetime_utc(expected_start), current))
            self.ps = c.WinDLL("psapi", use_last_error=True)
            self.ps.EnumProcessModulesEx.argtypes = [w.HANDLE, c.POINTER(w.HMODULE), w.DWORD, c.POINTER(w.DWORD), w.DWORD]
            self.ps.EnumProcessModulesEx.restype = w.BOOL
            modules, needed = (w.HMODULE * 2048)(), w.DWORD()
            if not self.ps.EnumProcessModulesEx(self.handle, modules, c.sizeof(modules), c.byref(needed), 3):
                raise c.WinError(c.get_last_error())
            if not 0 < needed.value <= c.sizeof(modules) or needed.value % c.sizeof(w.HMODULE):
                raise ValueError("Module list empty, misaligned, or exceeds 2048-module buffer")
            self.base = int(modules[0])
            self.ps.GetModuleFileNameExW.argtypes = [w.HANDLE, w.HMODULE, w.LPWSTR, w.DWORD]
            self.ps.GetModuleFileNameExW.restype = w.DWORD
            name = c.create_unicode_buffer(32768)
            length = self.ps.GetModuleFileNameExW(self.handle, modules[0], name, len(name))
            if not 0 < length < len(name) - 1:
                raise ValueError("Main module path absent or truncated")
            self.module_path = name.value
            if Path(self.module_path).name.lower() != "reliccoh3.exe":
                raise ValueError("Selected main module is not RelicCoH3.exe")

            class ModuleInfo(c.Structure):
                _fields_ = [("base", c.c_void_p), ("size", w.DWORD), ("entry", c.c_void_p)]

            self.ps.GetModuleInformation.argtypes = [w.HANDLE, w.HMODULE, c.POINTER(ModuleInfo), w.DWORD]
            self.ps.GetModuleInformation.restype = w.BOOL
            info = ModuleInfo()
            if not self.ps.GetModuleInformation(self.handle, modules[0], c.byref(info), c.sizeof(info)):
                raise c.WinError(c.get_last_error())
            self.image_size = info.size
            if self.image_size != IMAGE_SIZE or info.base != self.base or not 0x10000 <= self.base < USER_END - IMAGE_SIZE:
                raise ValueError("Main module image size/base differs from known candidate")
        except Exception:
            self.close()
            raise

    def identity(self):
        created, exited, kernel, user = (w.FILETIME() for _ in range(4))
        code = w.DWORD()
        if not self.k.GetProcessTimes(self.handle, c.byref(created), c.byref(exited), c.byref(kernel), c.byref(user)):
            raise c.WinError(c.get_last_error())
        if not self.k.GetExitCodeProcess(self.handle, c.byref(code)):
            raise c.WinError(c.get_last_error())
        value = (created.dwHighDateTime << 32) | created.dwLowDateTime
        return {"pid": self.pid, "startFiletime": value, "startUtc": filetime_utc(value),
                "exitCode": code.value, "stillActive": code.value == 259, "utc": utc_now()}

    def attest(self, tick):
        path = Path(self.module_path)
        before = path.stat()
        if not 0 < before.st_size <= 256 * 1024 * 1024:
            raise ValueError("Module file exceeds 256 MiB hashing bound")
        digest = hashlib.sha256()
        with path.open("rb") as source:
            while True:
                tick()
                block = source.read(1024 * 1024)
                if not block:
                    break
                digest.update(block)
        after = path.stat()
        value = digest.hexdigest()
        if (before.st_size, before.st_mtime_ns) != (after.st_size, after.st_mtime_ns) or value != GAME_SHA256:
            raise ValueError("Module SHA/stability mismatch: expected=%s actual=%s" % (GAME_SHA256, value))
        return {"path": str(path), "sha256": value, "fileBytes": before.st_size,
                "imageSize": self.image_size, "base": hex(self.base), "fileStableDuringHash": True}

    def read(self, address, size):
        raw, done = c.create_string_buffer(size), c.c_size_t()
        ok = self.k.ReadProcessMemory(self.handle, address, raw, size, c.byref(done))
        if not ok or done.value != size:
            raise OSError("ReadProcessMemory(%#x,%d) error=%d actual=%d" % (address, size, c.get_last_error(), done.value))
        return raw.raw

    def close(self):
        if self.handle:
            if not self.k.CloseHandle(self.handle):
                raise c.WinError(c.get_last_error())
            self.handle = None
            self.closed = True


def load_sources():
    root = Path(__file__).resolve().parents[3]
    manifest, texts = {}, {}
    for relative_path, expected in SOURCE_PINS.items():
        path = root / relative_path
        raw = path.read_bytes()
        actual = hashlib.sha256(raw).hexdigest()
        if actual != expected:
            raise ValueError("Source pin changed: %s expected=%s actual=%s" % (path, expected, actual))
        manifest[relative_path] = {"path": str(path), "sha256": actual, "bytes": len(raw)}
        texts[relative_path] = raw.decode("utf-8-sig")
    layout = texts["client/CoH3MemoryTool/GameLayout52426.cs"]
    mapping = {int(old): int(new, 16) for old, new in re.findall(r"\{\s*(\d+)\s*,\s*(0x[0-9a-fA-F]+)\s*\}", layout)}
    if GAME_SHA256 not in layout or any(mapping.get(old) != actual for old, actual in ROLES.values()):
        raise ValueError("Known candidate SHA/individual RVA roles differ from pinned source")
    return manifest


class Observation:
    def __init__(self, reader):
        self.r, self.watch = reader, []
        self.vt = {name: reader.base + actual for name, (_, actual) in ROLES.items() if name.endswith("Vtable")}

    def checkpoint(self, address, size, field, raw=None):
        raw = self.r.read(address, size, field) if raw is None else raw
        self.watch.append((address, size, field, raw))
        return raw

    def q(self, address, field):
        return unpack(self.checkpoint(address, 8, field), "Q")

    def u(self, address, field):
        return unpack(self.checkpoint(address, 4, field), "I")

    def observe(self, name, action):
        out = {"status": "pending", "startedUtc": utc_now()}
        try:
            action(out)
            if out["status"] == "pending":
                out["status"] = "observed"
        except BudgetExceeded as error:
            out.update(status="budget_exhausted", error=failure(error))
        except Exception as error:
            out.update(status="not_observed", error=failure(error))
        out["finishedUtc"] = utc_now()
        return out

    def camera(self, out):
        r = self.r
        owner = self.q(r.base + ROLES["cameraOwner"][1], "camera.owner")
        out["owner"] = hex(owner)
        vector = self.q(span(owner) + 184, "camera.vector")
        before = self.checkpoint(vector, 16, "camera.vectorBounds")
        begin, end = struct.unpack("<QQ", before)
        out.update(vector=hex(vector), begin=hex(begin), end=hex(end), candidates=[])
        if not 0 <= end - begin <= 8192 or (end - begin) % 8:
            raise ValueError("Camera vector invalid: begin=%#x end=%#x" % (begin, end))
        core = 0
        for slot in range(begin, end, 8):
            candidate = r.q(slot, "camera.vectorElement")
            if candidate < 65576 or not pointer(candidate - 40):
                continue
            item = {"element": hex(candidate), "core": hex(candidate - 40)}
            out["candidates"].append(item)
            try:
                vtable = r.q(candidate - 40, "camera.candidateVtable")
                item["vtable"] = hex(vtable)
                if vtable == self.vt["cameraCoreVtable"]:
                    core = candidate - 40
                    break
            except (BudgetExceeded, SliceExceeded):
                raise
            except Exception as error:
                item["error"] = failure(error)
        if not core:
            raise ValueError("Known GameUICore not observed in bounded source-derived vector")
        out["core"] = hex(core)
        out["coreVtable"] = hex(self.q(core, "camera.coreVtable"))
        matrix = self.q(core + 416, "camera.matrixPointer416")
        out["matrixObject"] = hex(matrix)
        body = r.read(matrix, 816, "camera.matrixBody816")
        view, projection, pose = floats(body, 88, 16), floats(body, 752, 16), floats(body, 72, 3)
        residual = [view[12 + j] + sum(pose[k] * view[k * 4 + j] for k in range(3)) for j in range(3)]
        finite = all(math.isfinite(x) for x in view + projection + pose)
        out.update(view=json_floats(view), projection=json_floats(projection), pose=json_floats(pose),
                   viewRawHex=body[88:152].hex(), projectionRawHex=body[752:816].hex(),
                   poseResidual=json_floats(residual), finite=finite,
                   sourceMatrixChecks=finite and abs(view[15] - 1) <= .01 and projection[0] > 0 and projection[5] > 0 and abs(projection[11] - 1) <= .01 and all(abs(x) <= .1 for x in residual))
        controller = self.q(core + 680, "camera.controllerPointer680")
        info = {"address": hex(controller)}
        out["controller"] = info
        info["vtable"] = hex(self.q(controller, "controller.vtable"))
        identity = self.q(controller + 8, "controller.identity8")
        mode = self.checkpoint(controller + 24, 1, "controller.byte24")[0]
        info.update(identity=hex(identity), localSlot=identity >> 32, lowIdentity=identity & SENTINEL,
                    byte24=mode, sourceLocalIdentityChecks=info["vtable"] == hex(self.vt["controllerVtable"]) and mode == 0 and (identity & SENTINEL) != 0 and identity >> 32 < 16)

    def world(self, out, camera):
        arena = self.q(self.r.base + ROLES["arena"][1], "world.arenaRoot")
        offset = self.u(self.r.base + ROLES["world"][1], "world.worldOffsetRoot")
        out.update(arena=hex(arena), worldOffset=hex(offset))
        span(arena)
        world = relative(arena, offset)
        out["world"] = hex(world)
        pvec_offset = self.u(world + 252, "world.playerVectorOffset252")
        pvec = relative(arena, pvec_offset)
        out["playerVector"] = hex(pvec)
        count = self.u(pvec + 16, "players.count16")
        array_offset = self.u(pvec + 12, "players.arrayOffset12")
        out.update(playerCount=count, playerArrayOffset=hex(array_offset), players=[])
        if not 1 <= count <= 16:
            raise ValueError("Player count outside 1..16: %d" % count)
        array = relative(arena, array_offset)
        out["playerArray"] = hex(array)
        controller = camera.get("controller", {})
        local = controller.get("identity")
        local_valid = camera.get("sourceMatrixChecks", False) and controller.get("sourceLocalIdentityChecks", False)
        out["sourceLocalIdentityChecks"] = local_valid
        local_addresses = []
        for index in range(count):
            off = self.u(array + index * 4, "players.elementOffset")
            row = {"index": index, "offset": hex(off)}
            out["players"].append(row)
            if off == SENTINEL:
                row.update(status="not_observed", error="sentinel player offset")
                continue
            address = relative(arena, off)
            identity = self.q(address + 8, "players.identity8")
            slot = identity >> 32
            row.update(status="observed", address=hex(address), identity=hex(identity), slot=slot,
                       sourceSlotRange=slot < 16, matchesObservedController=local is not None and hex(identity) == local)
            if row["matchesObservedController"]:
                local_addresses.append(address)
        out["localIdentityMatches"] = [hex(x) for x in local_addresses]
        out["uniqueObservedLocalPlayer"] = len(local_addresses) == 1
        if len(local_addresses) == 1 and local_valid:
            out["matchKeyCandidate"] = "%d:%d:%d:%d:%d" % (self.r.memory.pid, self.r.memory.identity()["startFiletime"] + 504911232000000000, world, local_addresses[0], int(local, 16) & SENTINEL)
            out["matchKeyCandidateIsProductSnapshot"] = False

    def children(self, node, body, anomalies):
        r, candidates = self.r, set()
        for offset in range(144, 576, 8):
            value = unpack(body, "Q", offset)
            if pointer(value) and value != node:
                candidates.add(value)
        vector = unpack(body, "Q", 504)
        if pointer(vector) and vector != node:
            try:
                count = r.u(vector + 124, "ui.childrenVectorCount124")
                data = r.q(vector + 112, "ui.childrenVectorData112")
                if count > MAX_CHILDREN:
                    anomalies.append({"node": hex(node), "vector": hex(vector), "count": count, "error": "child count exceeds 512; vector not followed"})
                elif count:
                    raw = r.read(data, count * 8, "ui.childrenVectorElements")
                    candidates.update(value for (value,) in struct.iter_unpack("<Q", raw) if pointer(value) and value != node)
            except (BudgetExceeded, SliceExceeded):
                raise
            except Exception as error:
                anomalies.append({"node": hex(node), "vector": hex(vector), "error": failure(error)})
        found = []
        for candidate in sorted(candidates):
            try:
                if r.q(candidate + 136, "ui.childParent136") == node:
                    vtable = r.q(candidate, "ui.childVtable")
                    if 64307200 <= vtable - r.base < min(96444416, IMAGE_SIZE):
                        found.append(candidate)
            except (BudgetExceeded, SliceExceeded):
                raise
            except Exception as error:
                anomalies.append({"node": hex(node), "candidate": hex(candidate), "error": failure(error)})
        return found

    def parent_chain(self, control, kind, out):
        offset = 472 if kind == "minimap" else 528
        control_body = self.r.read(control, 480 if kind == "minimap" else 552, kind + ".controlBody")
        self.checkpoint(control, 8, kind + ".controlVtable", control_body[:8])
        template = unpack(control_body, "Q", offset)
        self.checkpoint(control + offset, 8, kind + ".templatePointer", control_body[offset:offset + 8])
        out.update(control=hex(control), controlVtable=hex(unpack(control_body, "Q")), template=hex(template),
                   nodes=[], termination="pending", hudSeen=False, controlSeen=False, rootSeen=False,
                   completeLayoutValidationPerformed=False, csharpTryReadExecuted=False)
        if unpack(control_body, "Q") != self.vt[kind + "Vtable"]:
            raise ValueError("Map control vtable changed: %#x" % control)
        node, seen = span(template), set()
        while node:
            if node in seen:
                out["termination"] = "parent_cycle"
                raise ValueError("Parent cycle at %#x" % node)
            if len(seen) >= MAX_PARENTS:
                out["termination"] = "parent_limit"
                raise ValueError("Parent chain exceeds 64 nodes")
            seen.add(node)
            raw = self.r.read(node, 344, kind + ".parentBody344")
            vtable, parent, flags = unpack(raw, "Q"), unpack(raw, "Q", 136), unpack(raw, "I", 192)
            for pos, size, field in ((0, 8, "vtable"), (136, 8, "parent136"), (192, 4, "flags192")):
                self.checkpoint(node + pos, size, kind + ".chain." + field, raw[pos:pos + size])
            entry = {"address": hex(node), "vtable": hex(vtable), "vtableRva": hex(vtable - self.r.base),
                     "parent136": hex(parent), "flags192": hex(flags), "bit20Set": bool(flags & 0x20),
                     "translation168": json_floats(floats(raw, 168, 2)), "size296": json_floats(floats(raw, 296, 2))}
            out["nodes"].append(entry)
            out["hudSeen"] |= vtable == self.vt["hudVtable"]
            out["controlSeen"] |= node == control
            if parent == 0:
                out["rootSeen"] = vtable == self.vt["rootVtable"]
            node = parent
        out["termination"] = "null_parent"
        out["allChainBit20Set"] = all(item["bit20Set"] for item in out["nodes"])
        out["sourceParentChainChecks"] = out["hudSeen"] and out["rootSeen"] and (kind != "tactical" or out["controlSeen"])
        out["completeLayoutValidationPerformed"] = False
        out["csharpTryReadExecuted"] = False

    def hud(self, out):
        r = self.r
        out["maps"] = {kind: {"status": "not_observed", "reason": "known UI root not yet observed"} for kind in ("minimap", "tactical")}
        singleton = self.q(r.base + ROLES["uiSingleton"][1], "ui.singletonRoot")
        out["singleton"] = hex(singleton)
        root = self.q(span(singleton) + 136, "ui.rootPointer136")
        out["root"] = hex(root)
        root_vtable = self.q(root, "ui.rootVtable")
        out["rootVtable"] = hex(root_vtable)
        if root_vtable != self.vt["rootVtable"]:
            raise ValueError("Known GameUIAppWindow root vtable not observed: actual=%#x" % root_vtable)
        queue, seen, maps = deque([root]), set(), {}
        selected, anomalies = [], []
        out.update(selected=selected, anomalies=anomalies, maps={}, traversal={})
        reason = "queue_exhausted"
        try:
            with r.hud_slice():
                while queue and len(seen) < MAX_NODES:
                    node = queue.popleft()
                    if node in seen:
                        continue
                    seen.add(node)
                    try:
                        body = r.read(node, 576, "ui.nodeBody576")
                        vtable = unpack(body, "Q")
                        kind = next((name for name in ("minimap", "tactical", "hud", "root") if vtable == self.vt[name + "Vtable"]), None)
                        if kind:
                            selected.append({"kind": kind, "address": hex(node), "vtable": hex(vtable), "parent136": hex(unpack(body, "Q", 136)), "flags192": hex(unpack(body, "I", 192))})
                        if kind in ("minimap", "tactical"):
                            maps.setdefault(kind, node)
                            if len(maps) == 2:
                                reason = "first_known_map_of_each_kind_found"
                                break
                            continue
                        queue.extend(x for x in self.children(node, body, anomalies) if x not in seen)
                    except (BudgetExceeded, SliceExceeded):
                        raise
                    except Exception as error:
                        anomalies.append({"node": hex(node), "error": failure(error)})
                else:
                    if queue:
                        reason = "node_limit"
        except SliceExceeded as error:
            reason = "slice_limit"
            out["traversal"]["error"] = failure(error)
        except BudgetExceeded as error:
            reason = "global_budget"
            out.update(status="budget_exhausted", error=failure(error))
        finally:
            out["traversal"].update(visited=len(seen), queueRemaining=len(queue), stopReason=reason, maxNodes=MAX_NODES, exhaustive=False)
        for kind in ("minimap", "tactical"):
            if kind not in maps:
                out["maps"][kind] = {"status": "not_observed", "reason": "not found through bounded known UI root/children; no region scan or cached address fallback"}
            else:
                out["maps"][kind] = self.observe(kind, lambda target, kind=kind: self.parent_chain(maps[kind], kind, target))

    def final_checks(self):
        out = {"status": "pending", "checkpointCount": len(self.watch), "checked": 0, "differences": [], "failures": []}
        self.r.finalizing = True
        try:
            for address, size, field, expected in self.watch:
                try:
                    actual = self.r.read(address, size, "final." + field)
                    out["checked"] += 1
                    if actual != expected:
                        out["differences"].append({"field": field, "address": hex(address), "expectedRawHex": expected.hex(), "actualRawHex": actual.hex()})
                except BudgetExceeded:
                    raise
                except Exception as error:
                    out["failures"].append({"field": field, "address": hex(address), "length": size, "error": failure(error)})
            out["status"] = "checked"
        except BudgetExceeded as error:
            out.update(status="budget_exhausted", error=failure(error))
        finally:
            self.r.finalizing = False
        out["allObservedCheckpointsStable"] = out["status"] == "checked" and not out["differences"] and not out["failures"]
        root_names = {"world.arenaRoot", "world.worldOffsetRoot"}
        seen_roots = {field for _, _, field, _ in self.watch if field in root_names}
        root_issues = any(item["field"] in root_names for item in out["differences"] + out["failures"])
        out["stableWorldRootsAfterRead"] = seen_roots == root_names and out["status"] == "checked" and not root_issues
        return out

    def sample(self, scheduled_offset, schedule_start=None):
        now = self.r.clock()
        actual_offset = now - (self.r.started if schedule_start is None else schedule_start)
        result = {"scheduledOffsetSeconds": scheduled_offset, "actualOffsetSeconds": actual_offset,
                  "scheduleDelaySeconds": max(0.0, actual_offset - scheduled_offset),
                  "startedUtc": utc_now(), "elapsedFromInvocationSeconds": now - self.r.started}
        result["identityBefore"] = self.r.memory.identity()
        result["camera"] = self.observe("camera", self.camera)
        result["world"] = self.observe("world", lambda out: self.world(out, result["camera"]))
        result["hud"] = self.observe("hud", self.hud)
        result["stability"] = self.final_checks()
        result["identityAfter"] = self.r.memory.identity()
        result["sameProcessIdentity"] = result["identityBefore"]["startFiletime"] == result["identityAfter"]["startFiletime"] and result["identityAfter"]["stillActive"]
        result.update(finishedUtc=utc_now(), counters=self.r.stats())
        return result


def capture(args, memory_factory=WindowsMemory, clock=time.monotonic, sleeper=time.sleep):
    """Exclusive output reservation precedes source reads and any process handle."""
    output = Path(args.out)
    expected = start_filetime(args.expected_start_utc)
    if not 0 < args.pid <= 0xffffffff:
        raise ValueError("pid must be 1..4294967295")
    with output.open("x", encoding="utf-8", newline="\n") as destination:
        started, memory, reader = clock(), None, None
        report = {"schema": "known-hud-phase-observation-v1", "startedUtc": utc_now(),
                  "pid": args.pid, "expectedStartUtc": filetime_utc(expected), "expectedStartFiletime": expected,
                  "operatorLabel": args.label, "operatorLabelIsObservedPhase": False,
                  "status": "pending", "phaseDecision": "not_evaluated", "productAcceptance": False,
                  "accessMask": "0x410", "access": "PROCESS_QUERY_INFORMATION|PROCESS_VM_READ",
                  "caps": {"seconds": MAX_SECONDS, "requestedBytes": MAX_BYTES, "readAttempts": MAX_READS,
                           "singleReadBytes": MAX_SINGLE, "parents": MAX_PARENTS, "childrenPerVector": MAX_CHILDREN, "uiNodesPerSample": MAX_NODES},
                  "roles": {name: {"legacyRva": hex(old), "candidateRva": hex(actual)} for name, (old, actual) in ROLES.items()},
                  "samples": [], "handleClosed": None}
        try:
            report["sourceManifest"] = load_sources()
            report["scriptSha256"] = hashlib.sha256(Path(__file__).read_bytes()).hexdigest()
            if clock() - started >= MAX_SECONDS - RESERVE_SECONDS:
                raise BudgetExceeded("Deadline reached before process open")
            memory = memory_factory(args.pid, expected)
            reader = BoundedReader(memory, clock, started)
            report["identityBefore"] = memory.identity()
            if report["identityBefore"]["startFiletime"] != expected or not report["identityBefore"]["stillActive"]:
                raise ValueError("Process identity changed before attestation")
            report["module"] = memory.attest(reader.tick)
            # t=0/1/3 is relative to the first sample, after identity/SHA setup.
            schedule_start = clock()
            report["scheduleStartedUtc"] = utc_now()
            for offset in (0, 1, 3):
                remaining = schedule_start + offset - clock()
                if remaining > 0:
                    reader.tick()
                    if clock() + remaining - started >= MAX_SECONDS - RESERVE_SECONDS:
                        raise BudgetExceeded("Scheduled sample would exceed remaining deadline")
                    sleeper(remaining)
                reader.tick()
                current = memory.identity()
                if current["startFiletime"] != expected or not current["stillActive"]:
                    raise ValueError("Process identity changed before scheduled sample: %s" % current)
                report["samples"].append(Observation(reader).sample(offset, schedule_start))
                reader.tick()
            report["identityAfter"] = memory.identity()
            report["status"] = "observed" if report["identityAfter"]["startFiletime"] == expected and report["identityAfter"]["stillActive"] else "identity_changed"
        except Exception as error:
            report.update(status="budget_exhausted" if isinstance(error, BudgetExceeded) else "failed", error=failure(error))
        finally:
            if reader:
                report["counters"] = reader.stats()
                report["readJournal"] = reader.journal
            if memory:
                try:
                    memory.close()
                    report["handleClosed"] = memory.closed
                except Exception as error:
                    report.update(status="failed", handleCloseError=failure(error), handleClosed=False)
            report["finishedUtc"] = utc_now()
            report["elapsedSeconds"] = clock() - started
            report["deadlineOverrunSeconds"] = max(0.0, report["elapsedSeconds"] - MAX_SECONDS)
            json.dump(report, destination, ensure_ascii=False, indent=2, allow_nan=False)
            destination.write("\n")
    return report


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pid", type=int, required=True)
    parser.add_argument("--expected-start-utc", required=True, help="Exact process creation UTC (up to 7 fractional digits)")
    parser.add_argument("--out", required=True, help="New JSON path; parent must exist; never overwritten")
    parser.add_argument("--label", choices=("running", "paused", "results", "mainmenu", "newmatch"), required=True, help="Operator label only, not an inferred phase")
    args = parser.parse_args(argv)
    report = capture(args)
    print(json.dumps({"output": str(Path(args.out).resolve()), "status": report["status"], "phaseDecision": report["phaseDecision"], "samples": len(report["samples"]), "counters": report.get("counters"), "handleClosed": report["handleClosed"]}, ensure_ascii=False))
    return 0 if report["status"] == "observed" else 3


if __name__ == "__main__":
    raise SystemExit(main())
