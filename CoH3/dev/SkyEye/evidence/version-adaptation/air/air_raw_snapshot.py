"""One-shot read-only raw air evidence. --self-test never constructs WindowsMemory.

Live mode requires explicit --pid and --out. Fixed limits: 10 s / 2 MiB / 20k
ReadProcessMemory attempts, including failed reads. No process writes or game calls.
"""
import argparse
import collections
import ctypes
from ctypes import wintypes
import datetime
import hashlib
import json
import math
import pathlib
import re
import struct
import sys
import time

MAX_SECONDS = 10.0
MAX_BYTES = 2 * 1024 * 1024
MAX_READS = 20000
MAX_SINGLE = 65536
RESERVE_BYTES = 8192
RESERVE_READS = 64
RESERVE_SECONDS = 0.5
SENTINEL = 0xFFFFFFFF
ROLES = {"resource": 112918560, "arena": 102260400, "world": 98614988,
         "timedAbility": 64568064, "moverDynamics": 64559016}
LOITERS = {"ebps\\races\\german\\aircraft\\stuka_ju87_loiter",
           "ebps\\races\\german\\aircraft\\stuka_ju87_rocket_strafe_loiter_ger",
           "ebps\\races\\afrika_korps\\aircraft\\stuka_ju87_anti_vehicle_loiter_ak"}
RECOVERABLE = {"ebps\\races\\german\\aircraft\\stuka_ju87_strafe",
               "ebps\\races\\german\\aircraft\\stuka_ju87_combined_arms_strafe",
               "ebps\\races\\american\\aircraft\\p47_thunderbolt_air_support_center_us"}
V1 = "ebps\\gameplay\\projectiles\\races\\german\\p_artillery_off_map_rocket_v1"


def u(raw, off=0):
    return struct.unpack_from("<I", raw, off)[0]


def q(raw, off=0):
    return struct.unpack_from("<Q", raw, off)[0]


def xyz(raw, offsets):
    return [struct.unpack_from("<f", raw, off)[0] for off in offsets]


def json_floats(values):
    return [x if math.isfinite(x) else str(x) for x in values]


def valid_position(v):
    return all(math.isfinite(x) for x in v) and abs(v[0]) < 20000 and abs(v[2]) < 20000 and abs(v[1]) < 2000


def address_span(address, size):
    if not isinstance(address, int) or size < 0 or not 0x10000 <= address < 0x800000000000 or address + size > 0x800000000000:
        raise ValueError("Invalid user-mode address span")
    return address


def relative(arena, offset, size=1):
    if not 0 <= offset < SENTINEL:
        raise ValueError("Invalid/sentinel arena offset")
    return address_span(arena + offset, size)


class BudgetExceeded(RuntimeError):
    pass


class BoundedReader:
    def __init__(self, memory, clock=time.monotonic):
        self.memory, self.clock = memory, clock
        self.base, self.image_size = memory.base, memory.image_size
        self.started = clock()
        self.read_calls = self.bytes_requested = self.bytes_returned = 0
        self.finalizing = False

    def tick(self):
        limit = MAX_SECONDS if self.finalizing else MAX_SECONDS - RESERVE_SECONDS
        if self.clock() - self.started >= limit:
            raise BudgetExceeded("10 s deadline (0.5 s reserved for final stability checks)")

    def read(self, address, size):
        address_span(address, size)
        if not 0 < size <= MAX_SINGLE:
            raise ValueError("Single read must be 1..65536 bytes")
        self.tick()
        byte_limit = MAX_BYTES if self.finalizing else MAX_BYTES - RESERVE_BYTES
        read_limit = MAX_READS if self.finalizing else MAX_READS - RESERVE_READS
        if self.read_calls + 1 > read_limit or self.bytes_requested + size > byte_limit:
            raise BudgetExceeded("2 MiB / 20000 attempts budget (final-check reserve included)")
        # Charge requests before the OS call, including partial or failed attempts.
        self.read_calls += 1
        self.bytes_requested += size
        raw = self.memory.read(address, size)
        self.bytes_returned += len(raw)
        self.tick()
        if len(raw) != size:
            raise OSError("Short memory read: %d/%d bytes" % (len(raw), size))
        return raw

    def block(self, address, size):
        address_span(address, size)
        return b"".join(self.read(address + i, min(MAX_SINGLE, size - i)) for i in range(0, size, MAX_SINGLE))

    def stats(self):
        return {"readAttempts": self.read_calls, "requestedBytes": self.bytes_requested,
                "returnedBytes": self.bytes_returned, "elapsedSeconds": self.clock() - self.started}


class WindowsMemory:
    """Only QUERY_INFORMATION | VM_READ; never requests write/operation/thread access."""
    def __init__(self, pid):
        if sys.platform != "win32" or ctypes.sizeof(ctypes.c_void_p) != 8:
            raise RuntimeError("Live mode requires 64-bit Python on Windows")
        self.handle = None
        self.kernel = ctypes.WinDLL("kernel32", use_last_error=True)
        self.kernel.OpenProcess.argtypes = [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD]
        self.kernel.OpenProcess.restype = wintypes.HANDLE
        self.kernel.CloseHandle.argtypes = [wintypes.HANDLE]
        self.kernel.CloseHandle.restype = wintypes.BOOL
        self.kernel.ReadProcessMemory.argtypes = [wintypes.HANDLE, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_size_t, ctypes.POINTER(ctypes.c_size_t)]
        self.kernel.ReadProcessMemory.restype = wintypes.BOOL
        self.handle = self.kernel.OpenProcess(0x410, False, pid)
        if not self.handle:
            raise ctypes.WinError(ctypes.get_last_error())
        try:
            psapi = ctypes.WinDLL("psapi", use_last_error=True)
            psapi.EnumProcessModulesEx.argtypes = [wintypes.HANDLE, ctypes.POINTER(wintypes.HMODULE), wintypes.DWORD, ctypes.POINTER(wintypes.DWORD), wintypes.DWORD]
            psapi.EnumProcessModulesEx.restype = wintypes.BOOL
            modules = (wintypes.HMODULE * 2048)()
            needed = wintypes.DWORD()
            if not psapi.EnumProcessModulesEx(self.handle, modules, ctypes.sizeof(modules), ctypes.byref(needed), 3):
                raise ctypes.WinError(ctypes.get_last_error())
            if not 0 < needed.value <= ctypes.sizeof(modules):
                raise ValueError("Module list empty or exceeds fixed 2048-module buffer")
            self.base = int(modules[0])
            psapi.GetModuleFileNameExW.argtypes = [wintypes.HANDLE, wintypes.HMODULE, wintypes.LPWSTR, wintypes.DWORD]
            psapi.GetModuleFileNameExW.restype = wintypes.DWORD
            name = ctypes.create_unicode_buffer(32768)
            length = psapi.GetModuleFileNameExW(self.handle, modules[0], name, len(name))
            if not 0 < length < len(name) - 1:
                raise ValueError("Module path absent or truncated")
            self.module_path = name.value
            if pathlib.Path(self.module_path).name.lower() != "reliccoh3.exe":
                raise ValueError("Selected PID main module is not RelicCoH3.exe")

            class ModuleInfo(ctypes.Structure):
                _fields_ = [("base", ctypes.c_void_p), ("size", wintypes.DWORD), ("entry", ctypes.c_void_p)]

            psapi.GetModuleInformation.argtypes = [wintypes.HANDLE, wintypes.HMODULE, ctypes.POINTER(ModuleInfo), wintypes.DWORD]
            psapi.GetModuleInformation.restype = wintypes.BOOL
            info = ModuleInfo()
            if not psapi.GetModuleInformation(self.handle, modules[0], ctypes.byref(info), ctypes.sizeof(info)):
                raise ctypes.WinError(ctypes.get_last_error())
            self.image_size = info.size
            address_span(self.base, self.image_size)
        except Exception:
            self.close()
            raise

    def read(self, address, size):
        buffer = ctypes.create_string_buffer(size)
        done = ctypes.c_size_t()
        if not self.kernel.ReadProcessMemory(self.handle, address, buffer, size, ctypes.byref(done)) or done.value != size:
            raise OSError("ReadProcessMemory(%#x,%d) error=%d actual=%d" % (address, size, ctypes.get_last_error(), done.value))
        return buffer.raw

    def close(self):
        if self.handle:
            self.kernel.CloseHandle(self.handle)
            self.handle = None


def load_sources():
    root = pathlib.Path(__file__).resolve().parents[3]
    client = root / "client" / "CoH3MemoryTool"
    files = {name: client / (name + ".cs") for name in
             ("GameLayout52426", "PlayerInfoReader", "AirLogic", "AirCatalogue", "Loiter49", "SquadMarkerLogic", "BarrageLogic")}
    files["live_probe"] = root / "evidence" / "version-adaptation" / "live_probe.py"
    texts = {name: path.read_text(encoding="utf-8-sig") for name, path in files.items()}
    mapping = {int(old): int(new, 16) for old, new in re.findall(r"\{\s*(\d+)\s*,\s*(0x[0-9A-Fa-f]+)\s*\}", texts["GameLayout52426"])}
    rvas = {role: mapping[old] for role, old in ROLES.items()}  # no legacy/delta fallback
    catalogue, aircraft = {}, set()
    for call in re.findall(r'^\s*Add\(("[^;]+)\);', texts["AirCatalogue"], re.MULTILINE):
        strings = [s.replace("\\\\", "\\") for s in re.findall(r'"([^"\n]*)"', call)]
        if len(strings) < 3:
            raise ValueError("Unexpected AirCatalogue.Add source format")
        catalogue["abilities\\races\\" + strings[0]] = {"name": strings[1], "point": False}
        aircraft.update(strings[2:])
    for path in re.findall(r'list\["([^"]+)"\]\.Point\s*=\s*true', texts["AirCatalogue"]):
        catalogue[path.replace("\\\\", "\\")]["point"] = True
    if not catalogue:
        raise ValueError("No catalogue entries parsed from current source")
    manifest = {name: {"path": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()} for name, path in files.items()}
    return rvas, catalogue, aircraft, manifest


class Snapshot:
    def __init__(self, reader, rvas, catalogue, aircraft):
        self.r, self.rvas, self.catalogue, self.aircraft = reader, rvas, catalogue, aircraft
        self.watch = []
        self.names = {}
        self.coverage = collections.Counter()
        self.result = {"players": [], "timedAbilities": [], "airEntities": [], "errors": [],
                       "scanComplete": {"players": False, "timedAbilities": False, "entityRegistry": False}}

    def checkpoint(self, label, address, size):
        raw = self.r.read(address, size)
        self.watch.append((label, address, raw))
        return raw

    def error(self, location, ex):
        self.coverage["errors"] += 1
        if len(self.result["errors"]) < 100:
            self.result["errors"].append({"location": location, "error": str(ex)})

    def string(self, address):
        address_span(address, 32)
        if address not in self.names:
            raw = self.r.read(address, 32)
            size, cap = q(raw, 16), q(raw, 24)
            if size > 1024 or cap < size:
                raise ValueError("String dimensions exceed source 1024-byte bound")
            value = (raw[:size] if cap < 16 else self.r.read(q(raw), size)) if size else b""
            if self.r.read(address, 32) != raw:
                raise ValueError("String descriptor changed")
            self.names[address] = value.decode("utf-8", errors="strict")
        return self.names[address]

    def name(self, blueprint):
        address_span(blueprint, 72)
        return self.string(blueprint + 40)

    def vtable(self, raw, role):
        vt = q(raw)
        return {"address": hex(vt), "rva": hex(vt - self.r.base) if self.r.base <= vt < self.r.base + self.r.image_size else None,
                "expectedRva": hex(self.rvas[role]), "matches": vt == self.r.base + self.rvas[role]}

    def name_vector(self, kind, offset, limit):
        addr = self.r.base + self.rvas["resource"] + offset + 32
        raw = self.checkpoint(kind + ".beginEnd", addr, 16)
        begin, end = q(raw), q(raw, 8)
        if end < begin or end - begin > limit or (end - begin) % 48:
            raise ValueError("Invalid bounded name vector: " + kind)
        rows = []
        for p in range(begin, end, 48):
            self.r.tick()
            rows.append({"name": self.string(p + 8), "index": u(self.r.read(p + 40, 4))})
        return {"begin": hex(begin), "end": hex(end), "count": len(rows), "rows": rows}

    def abilities(self, arena, player):
        vector = player["_address"] + 1016
        header = self.checkpoint("abilities@" + player["address"] + ".dataCount", vector + 40, 8)
        data, count = u(header), u(header, 4)
        player["abilityVector"] = {"dataOffset40": hex(data), "count44": count}
        if count > 256:
            raise ValueError("Timed ability count exceeds source bound 256")
        slots = self.r.read(relative(arena, data, count * 4), count * 4) if count else b""
        for i in range(count):
            self.r.tick()
            off = u(slots, i * 4)
            if off == SENTINEL:
                self.coverage["sentinelAbilitySlots"] += 1
                continue
            rec = {"player": player["address"], "slot": player["slot"], "vectorIndex": i, "offset": hex(off)}
            self.result["timedAbilities"].append(rec)
            try:
                address = relative(arena, off, 148)
                raw = self.r.read(address, 148)
                rec.update(address=hex(address), raw148Hex=raw.hex(), vtable=self.vtable(raw, "timedAbility"))
                rec["fields"] = {"targetOffset8": hex(u(raw, 8)), "targetCount12": u(raw, 12), "type20": raw[20],
                                 "identity24": hex(q(raw, 24)), "direction40": json_floats(xyz(raw, (40, 44, 48))),
                                 "blueprint56": hex(q(raw, 56)), "instance68": u(raw, 68), "phase76": raw[76], "flags144": raw[144]}
                try:
                    rec["blueprint"] = self.name(q(raw, 56))
                except (OSError, ValueError) as ex:
                    self.error("ability.blueprint@" + hex(address), ex)
                definition = self.catalogue.get(rec.get("blueprint"))
                gates = {"timedVtable": rec["vtable"]["matches"], "cataloguedAbility": definition is not None,
                         "oneTarget": u(raw, 12) == 1, "phaseNot2": raw[76] != 2, "type6": raw[20] == 6,
                         "identityMatchesPlayer": q(raw, 24) == player["_identity"], "flag144Bit0": bool(raw[144] & 1)}
                rec["gates"] = gates
                rec["catalogueDefinition"] = definition
                if u(raw, 12) == 1:
                    target_addr = relative(arena, u(raw, 8), 20)
                    targets = self.r.read(target_addr, 20)
                    rec["target"] = {"address": hex(target_addr), "raw20Hex": targets.hex(), "kind0": targets[0],
                                     "position4": json_floats(xyz(targets, (4, 8, 12)))}
                    gates["targetKind1or2"] = targets[0] in (1, 2)
                    gates["validTargetPosition"] = valid_position(xyz(targets, (4, 8, 12)))
                    gates["targetStable"] = self.r.read(target_addr, 20) == targets
                else:
                    rec["targetSkipped"] = "Source only follows targetOffset8 when targetCount12 == 1"
                direction = xyz(raw, (40, 44, 48))
                norm = math.hypot(direction[0], direction[2])
                # C# tests abs(Y)>0.02 (rather than Y finite); preserve that exact gate.
                gates["directionOrPoint"] = bool(definition and definition["point"]) or (math.isfinite(norm) and 0.95 <= norm <= 1.05 and not abs(direction[1]) > 0.02)
                rec["directionAllFinite"] = all(math.isfinite(x) for x in direction)
                gates["raw148Stable"] = self.r.read(address, 148) == raw
                rec["rawCastCandidate"] = all(gates.values())
                rec["failedGates"] = [k for k, v in gates.items() if not v]
                self.coverage["abilitySlotsRead"] += 1
            except (OSError, ValueError) as ex:
                rec["error"] = str(ex)
                rec["rawCastCandidate"] = False
                self.error("ability@" + hex(off), ex)

    def plane(self, arena, entity, index, generation, header, path, players):
        rec = {"entity": hex(entity), "registryIndex": index, "generation": generation, "blueprint": path,
               "cataloguedAircraft": path in LOITERS or (path.startswith("ebps\\races\\") and "\\aircraft\\" in path and path.rsplit("\\", 1)[-1] in self.aircraft),
               "p47Name": "p47" in path.lower(), "reconName": "recon" in path.lower(), "v1": path == V1}
        self.result["airEntities"].append(rec)
        raw = self.r.read(entity, 392)
        owner, actor_off = u(raw, 88), u(raw, 388)
        rec.update(raw392Hex=raw.hex(), entityVtable=hex(q(raw)), blueprintPointer=hex(q(raw, 56)),
                   ownerOffset88=hex(owner), ownerPlayer=players.get(arena + owner), flags160=hex(u(raw, 160)),
                   position44=json_floats(xyz(raw, (44, 48, 52))), actorOffset388=hex(actor_off))
        gates = {"registryIdentity": u(raw, 104) == generation and u(raw, 108) == index and q(raw, 56) == q(header, 56),
                 "notDestroyed": not u(raw, 160) & 0x4000, "ownerPresent": owner != SENTINEL,
                 "ownerInPlayers": arena + owner in players, "actorPresent": actor_off != SENTINEL,
                 "recoverableStrafePath": path in RECOVERABLE}
        rec["recoveryRawGates"] = gates
        if actor_off != SENTINEL:
            actor_addr = relative(arena, actor_off, 68 if path == V1 else 284)
            motion = self.r.read(actor_addr, 68 if path == V1 else 284)
            rec["actor"] = {"address": hex(actor_addr), "rawHex": motion.hex(), "vtable": self.vtable(motion, "moverDynamics"),
                            "mode16": motion[16], "position20": json_floats(xyz(motion, (20, 24, 28))),
                            "backref36": hex(u(motion, 36)), "state64": u(motion, 64)}
            if path != V1:
                heading = xyz(motion, (272, 276, 280))
                magnitude = math.hypot(heading[0], heading[2])
                height = xyz(raw, (48,))[0]
                ground = xyz(motion, (24,))[0]
                rec["actor"]["heading272"] = json_floats(heading)
                gates.update(moverVtable=rec["actor"]["vtable"]["matches"], actorBackref=arena + u(motion, 36) == entity,
                             actorState1=u(motion, 64) == 1, actorMode2=motion[16] == 2,
                             validActorPosition=valid_position(xyz(motion, (20, 24, 28))),
                             unitHeading=math.isfinite(magnitude) and 0.95 <= magnitude <= 1.01,
                             flatHeading=math.isfinite(heading[1]) and abs(heading[1]) <= 0.1,
                             aboveTarget=math.isfinite(height) and height - ground >= 8,
                             actorTargetStable=self.r.read(actor_addr + 16, 16) == motion[16:32])
        tail = self.r.read(entity + 88, 24)
        gates["identityAndOwnerStable"] = (u(tail) == owner and u(tail, 16) == generation and u(tail, 20) == index
                                           and q(self.r.read(entity + 56, 8)) == q(raw, 56)
                                           and u(self.r.read(entity + 388, 4)) == actor_off)
        rec["failedRecoveryRawGates"] = [k for k, v in gates.items() if not v]
        rec["rawRecoveryCandidate"] = all(gates.values()) and path != V1
        rec["temporalAndEnemyGatesEvaluated"] = False

    def collect(self):
        r, out = self.r, self.result
        try:
            if any(rva + 1024 > r.image_size for rva in self.rvas.values()):
                raise ValueError("Current layout RVA outside selected main module")
            arena = q(self.checkpoint("root.arena", r.base + self.rvas["arena"], 8))
            world_offset = u(self.checkpoint("root.worldOffset", r.base + self.rvas["world"], 4))
            address_span(arena, 1)
            world = relative(arena, world_offset, 264)
            out["roots"] = {"arena": hex(arena), "worldOffset": hex(world_offset), "world": hex(world),
                            "resourceStaticAddress": hex(r.base + self.rvas["resource"])}
            # Capture descriptors early so a later budget/read failure still gets final checks.
            world_vectors = self.checkpoint("world.playerAndEntityOffsets", world + 252, 12)
            pvec = relative(arena, u(world_vectors), 20)
            registry = relative(arena, u(world_vectors, 8), 28)
            ph = self.checkpoint("players.dataCount", pvec + 12, 8)
            pdata, pcount = u(ph), u(ph, 4)
            if not 0 < pcount <= 16:
                raise ValueError("Player count outside established live_probe bound 1..16")
            slots = self.checkpoint("players.offsets", relative(arena, pdata, pcount * 4), pcount * 4)
            rh = self.checkpoint("registry.dataIdsCount", registry + 8, 20)
            offsets, ids, count = u(rh), u(rh, 12), u(rh, 16)
            out["registry"] = {"address": hex(registry), "offsets8": hex(offsets), "ids20": hex(ids), "count24": count}
            if not 0 < count <= 65536:
                raise ValueError("Entity registry count outside source bound 1..65536")
            # Validate both full table spans before reading either table.
            offsets_addr = relative(arena, offsets, count * 8)
            ids_addr = relative(arena, ids, count * 4)
            for kind, offset, limit in (("resources", 840, 6144), ("timers", 560, 12288)):
                try:
                    out[kind] = self.name_vector(kind, offset, limit)
                except (OSError, ValueError) as ex:
                    self.error(kind, ex)
            players = {}
            for i in range(pcount):
                player = relative(arena, u(slots, i * 4), 1064)
                identity_raw = self.checkpoint("player@" + hex(player) + ".identity", player + 8, 8)
                identity = q(identity_raw)
                row = {"address": hex(player), "identity": hex(identity), "slot": u(identity_raw, 4),
                       "_address": player, "_identity": identity}
                out["players"].append(row)
                players[player] = {"address": hex(player), "identity": hex(identity), "slot": row["slot"]}
            out["scanComplete"]["players"] = True
            ability_errors_before = self.coverage["errors"]
            for row in out["players"]:
                try:
                    self.abilities(arena, row)
                except (OSError, ValueError) as ex:
                    self.error("abilities@" + row["address"], ex)
            out["scanComplete"]["timedAbilities"] = self.coverage["errors"] == ability_errors_before
            table = r.block(offsets_addr, count * 8)
            generations = r.block(ids_addr, count * 4)
            registry_errors_before = self.coverage["errors"]
            for i in range(count):
                r.tick()
                self.coverage["registrySlotsScanned"] += 1
                offset, generation = u(table, i * 8), u(generations, i * 4)
                if offset == SENTINEL or generation == 0:
                    continue
                self.coverage["registryLiveSlots"] += 1
                try:
                    entity = relative(arena, offset, 112)
                    header = r.read(entity, 112)
                    if u(header, 104) != generation or u(header, 108) != i:
                        self.coverage["entityIdentityMismatch"] += 1
                        continue
                    path = self.name(q(header, 56))
                    self.coverage["entityHeadersRead"] += 1
                    if path == V1 or (path.startswith("ebps\\") and ("\\aircraft\\" in path or "p47" in path.lower() or "recon" in path.lower())):
                        self.plane(arena, entity, i, generation, header, path, players)
                except (OSError, ValueError) as ex:
                    self.error("registry[%d]" % i, ex)
            out["scanComplete"]["entityRegistry"] = self.coverage["errors"] == registry_errors_before and not self.coverage["entityIdentityMismatch"]
        except BudgetExceeded as ex:
            out["stopReason"] = str(ex)
        except (OSError, ValueError) as ex:
            self.error("snapshot", ex)
        finally:
            r.finalizing = True
            checks = []
            for label, address, expected in self.watch:
                check = {"label": label, "address": hex(address), "beforeHex": expected.hex()}
                checks.append(check)
                try:
                    after = r.read(address, len(expected))
                    check.update(afterHex=after.hex(), stable=after == expected)
                except (BudgetExceeded, OSError, ValueError) as ex:
                    check.update(stable=None, error=str(ex))
                    if isinstance(ex, BudgetExceeded):
                        break
            out["stabilityChecks"] = checks
            out["stableRootsAndDescriptors"] = bool(self.watch) and len(checks) == len(self.watch) and all(c.get("stable") is True for c in checks)
            for row in out["players"]:
                row.pop("_address", None)
                row.pop("_identity", None)
        out["coverage"] = dict(self.coverage)
        complete = all(out["scanComplete"].values()) and not self.coverage["errors"]
        out["stableEntries"] = (all(rec.get("gates", {}).get("raw148Stable") is True and rec["gates"].get("targetStable", True)
                                    for rec in out["timedAbilities"])
                                and all(all(rec.get("recoveryRawGates", {}).get(key) is True
                                            for key in ("registryIdentity", "identityAndOwnerStable"))
                                        and rec["recoveryRawGates"].get("actorTargetStable", True)
                                        for rec in out["airEntities"]))
        stable = out["stableRootsAndDescriptors"] and out["stableEntries"]
        out["status"] = ("inconclusive_budget_exhausted" if "stopReason" in out else
                         "unstable_or_unchecked_snapshot" if not stable and not self.coverage["errors"] else
                         "complete_raw_candidate_snapshot" if complete and stable else "inconclusive_partial_snapshot")
        out["absenceEvidenceCompleteWithinThisSnapshot"] = complete and stable
        return out


class FakeMemory:
    """Sparse fake bytes; no OS/process API involved in self-test."""
    base, image_size, module_path = 0x140000000, 0x7000000, "FAKE/RelicCoH3.exe"

    def __init__(self):
        self.data = {}
        self.calls = collections.Counter()
        self.mutate = None

    def put(self, address, raw):
        self.data.update((address + i, byte) for i, byte in enumerate(raw))

    def pack(self, address, fmt, *values):
        self.put(address, struct.pack(fmt, *values))

    def read(self, address, size):
        self.calls[address] += 1
        if self.mutate:
            self.mutate(self, address, size)
        try:
            return bytes(self.data[address + i] for i in range(size))
        except KeyError as ex:
            raise OSError("Unmapped fake byte at %#x" % ex.args[0]) from ex


def fake_fixture(rvas):
    m = FakeMemory()
    arena, world, pvec, player = 0x200000000, 0x1000, 0x2000, 0x3000
    registry, offsets, ids = 0x5000, 0x6000, 0x7000
    m.pack(m.base + rvas["arena"], "<Q", arena)
    m.pack(m.base + rvas["world"], "<I", world)
    m.pack(arena + world + 252, "<III", pvec, 0, registry)
    m.pack(arena + pvec + 12, "<II", 0x2800, 1)
    m.pack(arena + 0x2800, "<I", player)
    m.pack(arena + player + 8, "<Q", 1000)
    m.pack(arena + player + 1056, "<II", 0x4000, 2)
    m.pack(arena + 0x4000, "<II", 0x8000, 0x8200)
    m.put(arena + registry, bytes(28))
    m.pack(arena + registry + 8, "<I", offsets)
    m.pack(arena + registry + 20, "<II", ids, 2)
    m.pack(arena + offsets, "<IIII", 0x10000, 0, 0x11000, 0)
    m.pack(arena + ids, "<II", 7, 8)
    # Exercise both inline and heap-backed resource/timer strings.
    for i, (off, name, index) in enumerate(((840, "fuel", 4), (560, "deploy_aircraft_p47_timer", 6))):
        row, text = 0x320000000 + i * 0x1000, 0x330000000 + i * 0x1000
        m.pack(m.base + rvas["resource"] + off + 32, "<QQ", row, row + 48)
        m.put(row, bytes(48))
        if len(name) < 16:
            m.put(row + 8, name.encode("utf-8"))
        else:
            m.pack(row + 8, "<Q", text)
            m.put(text, name.encode("utf-8"))
        m.pack(row + 24, "<QQI", len(name), max(15, len(name)), index)
    names = ["abilities\\races\\american\\aircraft\\strafing_run_p47_us",
             "abilities\\races\\american\\aircraft\\recon_l2_us",
             "ebps\\races\\american\\aircraft\\p47_thunderbolt_air_support_center_us",
             "ebps\\races\\american\\aircraft\\l2_grasshopper_recon_reveal_los_us"]
    for i, name in enumerate(names):
        bp, text = 0x300000000 + i * 0x1000, 0x310000000 + i * 0x1000
        m.put(bp + 40, bytes(32))
        m.pack(bp + 40, "<Q", text)
        m.pack(bp + 56, "<QQ", len(name), len(name))
        m.put(text, name.encode("utf-8"))
    for i in range(2):
        raw = bytearray(148)
        struct.pack_into("<QII", raw, 0, m.base + rvas["timedAbility"], 0x9000 + i * 0x100, 1)
        raw[20], raw[144] = 6, 1
        struct.pack_into("<Q", raw, 24, 1000)
        struct.pack_into("<fff", raw, 40, 1, 0, 0)
        struct.pack_into("<Q", raw, 56, 0x300000000 + i * 0x1000)
        m.put(arena + 0x8000 + i * 0x200, raw)
        target = bytearray(20)
        target[0] = 2
        struct.pack_into("<fff", target, 4, 100, 0, 200)
        m.put(arena + 0x9000 + i * 0x100, target)
        body = bytearray(392)
        struct.pack_into("<Q", body, 0, m.base + 0x123456)
        struct.pack_into("<fff", body, 44, 90, 20, 200)
        struct.pack_into("<Q", body, 56, 0x300002000 + i * 0x1000)
        struct.pack_into("<I", body, 88, player)
        struct.pack_into("<II", body, 104, 7 + i, i)
        struct.pack_into("<I", body, 388, 0x12000 + i * 0x200)
        m.put(arena + 0x10000 + i * 0x1000, body)
        motion = bytearray(284)
        struct.pack_into("<Q", motion, 0, m.base + rvas["moverDynamics"])
        motion[16] = 2
        struct.pack_into("<fff", motion, 20, 100, 0, 200)
        struct.pack_into("<I", motion, 36, 0x10000 + i * 0x1000)
        struct.pack_into("<I", motion, 64, 1)
        struct.pack_into("<fff", motion, 272, 1, 0, 0)
        m.put(arena + 0x12000 + i * 0x200, motion)
    return m


def self_test(rvas, catalogue, aircraft):
    tests = []

    def check(name, fn):
        fn()
        tests.append({"name": name, "status": "pass"})

    def assert_true(condition):
        if not condition:
            raise AssertionError("Self-test assertion failed")

    def run(memory):
        reader = BoundedReader(memory)
        result = Snapshot(reader, rvas, catalogue, aircraft).collect()
        assert_true(reader.read_calls <= MAX_READS and reader.bytes_requested <= MAX_BYTES)
        return result, reader

    baseline, baseline_reader = run(fake_fixture(rvas))
    check("fake native P47 raw cast + actor recovery gates", lambda: assert_true(baseline["timedAbilities"][0]["rawCastCandidate"] and baseline["airEntities"][0]["rawRecoveryCandidate"]))
    check("uncatalogued recon remains visible and rejected by catalogue", lambda: assert_true(not baseline["timedAbilities"][1]["rawCastCandidate"] and baseline["airEntities"][1]["reconName"] and not baseline["airEntities"][1]["cataloguedAircraft"]))
    check("complete stable fake snapshot", lambda: assert_true(baseline["status"] == "complete_raw_candidate_snapshot" and baseline["stableRootsAndDescriptors"]))
    check("inline resource and heap timer vectors", lambda: assert_true(baseline["resources"]["rows"] == [{"name": "fuel", "index": 4}] and baseline["timers"]["rows"] == [{"name": "deploy_aircraft_p47_timer", "index": 6}]))
    memory = fake_fixture(rvas)
    memory.pack(0x20000800C, "<I", 2)
    skipped, _ = run(memory)
    check("multi-target count never followed as one target", lambda: assert_true(not skipped["timedAbilities"][0]["rawCastCandidate"] and memory.calls[0x200009000] == 0))
    memory = fake_fixture(rvas)
    memory.pack(0x200008000, "<Q", memory.base + 0x123456)
    wrong_vtable, _ = run(memory)
    check("wrong TimedAbility vtable preserved but rejected", lambda: assert_true(not wrong_vtable["timedAbilities"][0]["rawCastCandidate"] and not wrong_vtable["timedAbilities"][0]["gates"]["timedVtable"]))
    memory = fake_fixture(rvas)
    memory.pack(0x200009004, "<f", float("nan"))
    bad_position, _ = run(memory)
    check("NaN target rejected and JSON remains finite", lambda: assert_true(not bad_position["timedAbilities"][0]["rawCastCandidate"] and bool(json.dumps(bad_position, allow_nan=False))))

    for label, addr, fmt, value in (
        ("player count 17", 0x200002010, "<I", 17),
        ("entity count 65537", 0x200005018, "<I", 65537),
        ("ability count 257", 0x200003424, "<I", 257),
        ("ability data sentinel", 0x200003420, "<I", SENTINEL),
        ("registry data sentinel", 0x200005008, "<I", SENTINEL),
        ("blueprint string length 1025", 0x300000038, "<Q", 1025),
        ("target offset sentinel", 0x200008008, "<I", SENTINEL),
    ):
        memory = fake_fixture(rvas)
        memory.pack(addr, fmt, value)
        result, _ = run(memory)
        check(label + " rejected without unbounded dereference", lambda result=result: assert_true(result["status"] == "inconclusive_partial_snapshot" and not result["absenceEvidenceCompleteWithinThisSnapshot"]))

    memory = fake_fixture(rvas)
    def move_root(m, address, size):
        if address == m.base + rvas["arena"] and m.calls[address] == 2:
            m.pack(address, "<Q", 0x210000000)
    memory.mutate = move_root
    changed, _ = run(memory)
    check("changed arena root invalidates snapshot", lambda: assert_true(changed["status"] == "unstable_or_unchecked_snapshot"))
    memory = fake_fixture(rvas)
    def move_count(m, address, size):
        if address == 0x200003420 and m.calls[address] == 2:
            m.pack(address + 4, "<I", 1)
    memory.mutate = move_count
    changed, _ = run(memory)
    check("changed ability data/count invalidates snapshot", lambda: assert_true(not changed["stableRootsAndDescriptors"]))
    memory = fake_fixture(rvas)
    def change_raw(m, address, size):
        if address == 0x200008000 and m.calls[address] == 2:
            m.pack(address + 68, "<I", 99)
    memory.mutate = change_raw
    changed, _ = run(memory)
    check("changing raw148 rejected", lambda: assert_true(not changed["timedAbilities"][0]["gates"]["raw148Stable"]))
    memory = fake_fixture(rvas)
    memory.pack(0x200010068, "<I", 99)
    changed, _ = run(memory)
    check("registry generation mismatch is incomplete evidence", lambda: assert_true(not changed["scanComplete"]["entityRegistry"] and not changed["absenceEvidenceCompleteWithinThisSnapshot"]))

    def rejected_read(label, setup, address=0x10000, size=8, expected=BudgetExceeded):
        reader = BoundedReader(FakeMemory())
        setup(reader)
        calls_before = reader.read_calls
        try:
            reader.read(address, size)
        except expected:
            assert_true(reader.read_calls == calls_before)
            tests.append({"name": label, "status": "pass"})
        else:
            raise AssertionError("Expected bounded read failure: " + label)
    rejected_read("attempt count budget", lambda r: setattr(r, "read_calls", MAX_READS - RESERVE_READS))
    rejected_read("requested-byte budget", lambda r: setattr(r, "bytes_requested", MAX_BYTES - RESERVE_BYTES))
    rejected_read("single-read 65537 rejected", lambda r: None, size=65537, expected=ValueError)
    rejected_read("noncanonical/overflow pointer rejected", lambda r: None, address=0x7FFFFFFFFFFC, expected=ValueError)
    rejected_read("null pointer rejected", lambda r: None, address=0, expected=ValueError)
    clock_value = [0.0]
    reader = BoundedReader(FakeMemory(), clock=lambda: clock_value[0])
    clock_value[0] = MAX_SECONDS - RESERVE_SECONDS
    try:
        reader.read(0x10000, 8)
    except BudgetExceeded:
        tests.append({"name": "10-second clock guard with final-check time reserve", "status": "pass"})
    else:
        raise AssertionError("Clock budget was not enforced")
    reader = BoundedReader(FakeMemory())
    try:
        reader.read(0x10000, 8)
    except OSError:
        check("failed memory requests count against both budgets", lambda: assert_true(reader.read_calls == 1 and reader.bytes_requested == 8))
    reader = BoundedReader(fake_fixture(rvas))
    reader.read_calls, reader.bytes_requested = MAX_READS - RESERVE_READS, MAX_BYTES - RESERVE_BYTES
    reader.finalizing = True
    reader.read(reader.base + rvas["arena"], 8)
    check("reserved final root check remains within total limits", lambda: assert_true(reader.read_calls == MAX_READS - RESERVE_READS + 1 and reader.bytes_requested <= MAX_BYTES))
    return {"status": "pass", "mode": "fake_bytes_only", "liveProcessOpened": False, "testCount": len(tests),
            "tests": tests, "fakeSnapshotStats": baseline_reader.stats(), "fakeSnapshot": baseline}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pid", type=int, help="Explicit RelicCoH3 process ID (required except --self-test)")
    parser.add_argument("--out", required=True, type=pathlib.Path, help="New JSON file; existing files are never overwritten")
    parser.add_argument("--self-test", action="store_true", help="Fake bytes only; never opens a process")
    args = parser.parse_args()
    if args.self_test and args.pid is not None:
        parser.error("--self-test and --pid are mutually exclusive")
    if not args.self_test and (args.pid is None or not 0 < args.pid <= 0xFFFFFFFF):
        parser.error("Live mode requires an explicit positive DWORD --pid")
    # Reserve a new output before opening a process; no append/overwrite behavior.
    with args.out.open("x", encoding="utf-8") as output:
        report = {"schema": "coh3-air-raw-snapshot/v1", "timestampUtc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
                  "verification": "candidate_raw_evidence_not_engine_or_visual_validation",
                  "limits": {"seconds": MAX_SECONDS, "requestedBytes": MAX_BYTES, "readAttempts": MAX_READS,
                             "singleReadBytes": MAX_SINGLE, "reservedFinalBytes": RESERVE_BYTES,
                             "reservedFinalReads": RESERVE_READS, "reservedFinalSeconds": RESERVE_SECONDS}}
        memory = None
        try:
            rvas, catalogue, aircraft, manifest = load_sources()
            report.update(currentRvas={k: hex(v) for k, v in rvas.items()}, sources=manifest)
            if args.self_test:
                report["selfTest"] = self_test(rvas, catalogue, aircraft)
                report["status"] = "self_test_pass"
            else:
                report.update(pid=args.pid, access="0x410 PROCESS_QUERY_INFORMATION|PROCESS_VM_READ", mode="one_shot_read_only")
                opened_at = time.monotonic()
                memory = WindowsMemory(args.pid)
                reader = BoundedReader(memory)
                reader.started = opened_at  # Include OpenProcess/module metadata time in the deadline.
                report.update(moduleBase=hex(memory.base), moduleSize=memory.image_size, modulePath=memory.module_path)
                report["snapshot"] = Snapshot(reader, rvas, catalogue, aircraft).collect()
                report["stats"] = reader.stats()
                report["status"] = report["snapshot"]["status"]
        except Exception as ex:
            report.update(status="probe_error", error="%s: %s" % (type(ex).__name__, ex))
        finally:
            if memory is not None:
                memory.close()
        json.dump(report, output, ensure_ascii=False, indent=2, allow_nan=False)
        output.write("\n")
    summary = {"out": str(args.out), "status": report["status"]}
    if "selfTest" in report:
        summary.update(testCount=report["selfTest"]["testCount"], liveProcessOpened=False)
    if "stats" in report:
        summary.update(report["stats"])
    print(json.dumps(summary, ensure_ascii=False, allow_nan=False))
    return 0 if report["status"] in ("self_test_pass", "complete_raw_candidate_snapshot") else 2


if __name__ == "__main__":
    raise SystemExit(main())
