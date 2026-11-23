"""Raw current-root HUD membership, with optional process-bound v1 reference hints."""
import argparse
from collections import Counter, deque
from contextlib import contextmanager
import hashlib
import json
from pathlib import Path
import re
import struct
import time
from types import ModuleType

BASE_PATH = Path(__file__).resolve().parents[1] / "phase_probe.py"
BASE_SHA256 = "21af7198c86ea5f53b06e9df737251d81a21dd345ec243b8974beca75ec1d723"
MAX_REFERENCE_BYTES, MAX_NODES = 8 * 1024 * 1024, 768
KINDS = ("hud", "minimap", "tactical")
LABELS = ("running", "paused", "results", "mainmenu", "newmatch")
# Per three groups: <=19272 attempts, <=2016 KiB, still subject to v1 global caps.
PARTITIONS = {"camera": (768, 16 * 1024, 0.4), "world": (96, 8 * 1024, 0.2),
              "hud": (5000, 560 * 1024, 1.35), "stability": (560, 88 * 1024, 0.6)}


def sha256(raw):
    return hashlib.sha256(raw).hexdigest()


def load_base():
    raw = BASE_PATH.read_bytes()
    if sha256(raw) != BASE_SHA256:
        raise ValueError("Frozen v1 probe SHA256 mismatch")
    module = ModuleType("pinned_phase_probe_v1")
    module.__file__ = str(BASE_PATH)
    exec(compile(raw, module.__file__, "exec"), module.__dict__)
    return module


p = load_base()


def same_identity(value, pid, start):
    return value.get("pid") == pid and value.get("startFiletime") == start and value.get("stillActive") is True


def parse_reference(raw, expected_sha, pid, start, metadata):
    actual = sha256(raw)
    metadata.update(sha256=actual, expectedSha256=expected_sha, bytes=len(raw))
    if not re.fullmatch("[0-9a-f]{64}", expected_sha or "") or actual != expected_sha:
        raise ValueError("Reference byte SHA256 differs from --reference-sha256")
    doc = json.loads(raw)
    module = doc["module"]
    base = int(module["base"], 16)
    metadata.update(schema=doc.get("schema"), reportedScriptSha256=doc.get("scriptSha256"),
                    reportedScriptShaIsAttestation=False, operatorLabel=doc.get("operatorLabel"),
                    pid=doc.get("pid"), expectedStartFiletime=doc.get("expectedStartFiletime"),
                    module={key: module[key] for key in ("base", "sha256", "imageSize")})
    if (doc.get("schema") != "known-hud-phase-observation-v1" or doc.get("scriptSha256") != BASE_SHA256 or
            doc.get("status") != "observed" or doc.get("operatorLabel") not in ("running", "paused") or
            doc.get("operatorLabelIsObservedPhase") is not False or doc.get("handleClosed") is not True or
            doc.get("phaseDecision") != "not_evaluated"):
        raise ValueError("Reference requires an observed running/paused v1 report from the pinned script")
    if (doc.get("pid") != pid or doc.get("expectedStartFiletime") != start or
            p.start_filetime(doc["expectedStartUtc"]) != start or
            not all(same_identity(doc[key], pid, start) for key in ("identityBefore", "identityAfter"))):
        raise ValueError("Reference PID/start identity differs from the requested target")
    if (module["sha256"] != p.GAME_SHA256 or module["imageSize"] != p.IMAGE_SIZE or
            not p.pointer(base) or base + p.IMAGE_SIZE >= p.USER_END):
        raise ValueError("Reference module identity differs from the pinned image")
    if {key: item.get("sha256") for key, item in doc["sourceManifest"].items()} != p.SOURCE_PINS:
        raise ValueError("Reference source pins differ from v1")
    samples = doc["samples"]
    if [item["scheduledOffsetSeconds"] for item in samples] != [0, 1, 3]:
        raise ValueError("Reference requires the original 0/1/3-second sample set")
    chosen = None
    for sample in reversed(samples):
        if (not sample.get("sameProcessIdentity") or
                not all(same_identity(sample[key], pid, start) for key in ("identityBefore", "identityAfter"))):
            raise ValueError("Reference sample identity differs from the requested target")
        hud = sample["hud"]
        if (chosen is None and hud.get("status") == "observed" and
                hud.get("rootVtable") == hex(base + p.ROLES["rootVtable"][1])):
            chosen = sample
    if chosen is None:
        raise ValueError("Reference has no observed known UI root")
    hud = chosen["hud"]
    root = int(hud["root"], 16)
    if not p.pointer(root):
        raise ValueError("Reference root address is invalid")
    candidates = {kind: set() for kind in KINDS}

    def add(kind, address, vtable):
        if kind in candidates and vtable == hex(base + p.ROLES[kind + "Vtable"][1]):
            value = int(address, 16)
            if p.pointer(value) and value != root:
                candidates[kind].add(value)

    for node in hud.get("selected", []):
        add(node["kind"], node["address"], node["vtable"])
    for kind, item in hud.get("maps", {}).items():
        if item.get("status") == "observed" and item.get("sourceParentChainChecks") is True:
            add(kind, item["control"], item["controlVtable"])
            for node in item["nodes"]:
                add("hud", node["address"], node["vtable"])
    hints = {kind: next(iter(values)) for kind, values in candidates.items() if len(values) == 1}
    if "hud" not in hints:
        raise ValueError("Reference has no unique observed HUD hint")
    metadata.update(sampleOffsetSeconds=chosen["scheduledOffsetSeconds"], sampleStartedUtc=chosen["startedUtc"],
                    root=hex(root), traversal=hud.get("traversal"), hints={kind: hex(value) for kind, value in hints.items()},
                    candidateCounts={kind: len(values) for kind, values in candidates.items()}, currentModuleMatch=None)
    return hints


class SampleLimit(p.SliceExceeded):
    pass


class Reader(p.BoundedReader):
    def __init__(self, memory, clock, started):
        super().__init__(memory, clock, started)
        self.partition_limit = None

    @contextmanager
    def partition(self, label):
        calls, size, seconds = PARTITIONS[label]
        self.partition_limit = (label, self.calls + calls, self.requested + size, self.clock() + seconds)
        try:
            yield
        finally:
            self.partition_limit = None

    def read(self, address, size, field):
        if self.partition_limit:
            label, calls, byte_limit, deadline = self.partition_limit
            self.tick()
            if self.calls + 1 > calls or self.requested + size > byte_limit or self.clock() >= deadline:
                raise SampleLimit(label + " partition: attempts/bytes/cooperative deadline")
        return super().read(address, size, field)


class KnownTree:
    """Fresh per sample; the second pass uses a separate cache for actual re-reads."""
    def __init__(self, reader, prefix):
        self.r, self.prefix = reader, prefix
        self.bodies, self.parents, self.vtables, self.lists, self.vectors = {}, {}, {}, {}, {}
        self.anomalies, self.hits, self.rejected = [], Counter(), Counter()
        self.vt = {kind: reader.base + p.ROLES[kind + "Vtable"][1] for kind in (*KINDS, "root")}

    def cached(self, table, address, label, action):
        if address in table:
            self.hits[label] += 1
        else:
            try:
                table[address] = action()
            except (OSError, ValueError) as error:
                self.anomalies.append({"address": hex(address), "field": self.prefix + label, "error": p.failure(error)})
                table[address] = None
        return table[address]

    def body(self, address):
        return self.cached(self.bodies, address, "body576", lambda: self.r.read(address, 576, self.prefix + "body576"))

    def parent(self, address):
        if self.bodies.get(address) is not None:
            return p.unpack(self.bodies[address], "Q", 136)
        return self.cached(self.parents, address, "childParent136", lambda: self.r.q(address + 136, self.prefix + "childParent136"))

    def vtable(self, address):
        if self.bodies.get(address) is not None:
            return p.unpack(self.bodies[address], "Q")
        return self.cached(self.vtables, address, "childVtable", lambda: self.r.q(address, self.prefix + "childVtable"))

    def vector(self, address):
        def read_vector():
            header = self.r.read(address + 112, 16, self.prefix + "vectorHeader112")
            data, count = p.unpack(header, "Q"), p.unpack(header, "I", 12)
            if count > p.MAX_CHILDREN:
                self.anomalies.append({"address": hex(address), "field": self.prefix + "vectorCount124",
                                       "data112": hex(data), "count124": count, "reason": "child_count_limit"})
                return None
            raw = self.r.read(data, count * 8, self.prefix + "vectorElements") if count else b""
            return {"address": hex(address), "data112": hex(data), "count124": count, "elementsSha256": sha256(raw)}, raw
        return self.cached(self.vectors, address, "vector", read_vector)

    def children_fields(self, address):
        def read_fields():
            body = self.body(address)
            if body is None:
                return None
            result = {"candidates": {}, "vectorComplete": True, "vectorPointer504": hex(p.unpack(body, "Q", 504))}
            for offset in range(144, 576, 8):
                value = p.unpack(body, "Q", offset)
                if p.pointer(value) and value != address:
                    result["candidates"].setdefault(value, {"inlineOffset": offset, "pointerRawHex": body[offset:offset + 8].hex()})
            vector = p.unpack(body, "Q", 504)
            if vector:
                if not p.pointer(vector) or vector == address:
                    result["vectorComplete"] = False
                    self.anomalies.append({"address": hex(address), "field": self.prefix + "vectorPointer504",
                                           "value": hex(vector), "reason": "invalid_or_self_vector_pointer"})
                else:
                    observed = self.vector(vector)
                    if observed is None:
                        result["vectorComplete"] = False
                    else:
                        meta, raw = observed
                        result["vector"] = meta
                        for index, (value,) in enumerate(struct.iter_unpack("<Q", raw)):
                            if p.pointer(value) and value != address:
                                result["candidates"].setdefault(value, {"vector": meta, "index": index,
                                                                         "pointerRawHex": raw[index * 8:index * 8 + 8].hex()})
            return result
        return self.cached(self.lists, address, "childrenFields", read_fields)

    def admit(self, child, parent):
        reverse = self.parent(child)
        if reverse != parent:
            self.rejected["parent_unreadable" if reverse is None else "parent_mismatch"] += 1
            return False
        vtable = self.vtable(child)
        if vtable is None or not 64307200 <= vtable - self.r.base < min(96444416, p.IMAGE_SIZE):
            self.rejected["vtable_unreadable" if vtable is None else "vtable_range"] += 1
            return False
        return True

    def trace(self, kind, control, root, out):
        out.update(nodes=[], edges=[], termination="pending", firstPassPath=False)
        body = self.body(control)
        if body is None:
            out["termination"] = "control_read_failed"
            return
        out.update(control=hex(control), controlVtable=hex(p.unpack(body, "Q")))
        if p.unpack(body, "Q") != self.vt[kind]:
            out["termination"] = "control_vtable_changed"
            return
        node = control
        if kind != "hud":
            offset = 472 if kind == "minimap" else 528
            node = p.unpack(body, "Q", offset)
            out["templateField"] = {"offset": offset, "pointer": hex(node)}
        seen = set()
        for _ in range(p.MAX_PARENTS):
            if node in seen:
                out["termination"] = "parent_cycle"
                return
            if not p.pointer(node):
                out["termination"] = "invalid_parent_pointer"
                return
            seen.add(node)
            body = self.body(node)
            if body is None:
                out["termination"] = "node_read_failed"
                return
            vtable, parent, flags = p.unpack(body, "Q"), p.unpack(body, "Q", 136), p.unpack(body, "I", 192)
            out["nodes"].append({"address": hex(node), "vtable": hex(vtable), "parent136": hex(parent),
                                  "flags192": hex(flags), "bit20Set": bool(flags & 0x20)})
            if not 64307200 <= vtable - self.r.base < min(96444416, p.IMAGE_SIZE):
                out["termination"] = "ancestor_vtable_range"
                return
            if node == root:
                out["termination"] = "current_root_reached" if vtable == self.vt["root"] and parent == 0 else "root_identity_changed"
                break
            if not parent:
                out["termination"] = "different_terminal_root"
                break
            if not p.pointer(parent):
                out["termination"] = "invalid_parent_pointer"
                return
            fields = self.children_fields(parent)
            witness = fields["candidates"].get(node) if fields else None
            status = "listed" if witness is not None else ("not_listed_in_read_fields" if fields and fields["vectorComplete"] else "unknown")
            out["edges"].append({"child": hex(node), "parent": hex(parent), "childParent136": hex(parent),
                                  "downwardMembership": status, "witness": witness})
            node = parent
        else:
            out["termination"] = "parent_limit"
        out["hudAncestorSeen"] = any(item["vtable"] == hex(self.vt["hud"]) for item in out["nodes"])
        out["controlSeen"] = any(item["address"] == hex(control) for item in out["nodes"])
        out["allChainBit20Set"] = all(item["bit20Set"] for item in out["nodes"])
        out["firstPassPath"] = (out["termination"] == "current_root_reached" and out["hudAncestorSeen"] and out["controlSeen"] and
                                all(item["downwardMembership"] == "listed" for item in out["edges"]))
        if kind == "minimap":
            out["templateParentMatchesControl"] = bool(out["nodes"]) and out["nodes"][0]["parent136"] == hex(control)
            out["firstPassPath"] &= out["templateParentMatchesControl"]

    def walk(self, root, select, out):
        queue, enqueued = deque([root]), {root}
        attempted, expanded, pending, reason = 0, 0, None, "queue_exhausted"
        try:
            while queue and attempted < MAX_NODES:
                node = pending = queue.popleft()
                attempted += 1
                body = self.body(node)
                if body is not None:
                    vtable = p.unpack(body, "Q")
                    for kind in KINDS:
                        if vtable == self.vt[kind]:
                            select(kind, node)
                            break
                    fields = self.children_fields(node)
                    for child in sorted(fields["candidates"]):
                        if child not in enqueued and self.admit(child, node):
                            enqueued.add(child)
                            queue.append(child)
                    expanded += 1
                pending = None
            if queue:
                reason = "node_limit"
        except (SampleLimit, p.BudgetExceeded) as error:
            reason = "partition_limit" if isinstance(error, SampleLimit) else "global_budget"
            out["error"] = p.failure(error)
            if isinstance(error, p.BudgetExceeded):
                raise
        finally:
            out.update(stopReason=reason, attemptedNodes=attempted, expandedNodes=expanded, queueRemaining=len(queue),
                       unfinishedNode=hex(pending) if pending is not None else None, rejectedCandidates=dict(self.rejected),
                       queueExhausted=reason == "queue_exhausted", fullProcessSearch=False)
            out["knownGraphComplete"] = out["queueExhausted"] and not self.anomalies


def structure(trace):
    return (trace.get("templateField"),
            [(x["address"], x["vtable"], x["parent136"]) for x in trace["nodes"]],
            [(x["child"], x["parent"], x["downwardMembership"], x["witness"]) for x in trace["edges"]])


class Observation(p.Observation):
    def __init__(self, reader, hints):
        super().__init__(reader)
        self.hints, self.ui, self.ui_root = hints, None, None

    def observe(self, name, action):
        if name in ("camera", "world"):
            with self.r.partition(name):
                out = super().observe(name, action)
            if out.get("error", {}).get("type") == "SampleLimit":
                out["status"] = "partition_limit"
            return out
        return super().observe(name, action)

    def read_root(self, read):
        singleton = read(self.r.base + p.ROLES["uiSingleton"][1], "ui.singletonRoot")
        if not p.pointer(singleton):
            raise ValueError("Known UI singleton pointer invalid: %#x" % singleton)
        root = read(singleton + 136, "ui.rootPointer136")
        if not p.pointer(root):
            raise ValueError("Known UI root pointer invalid: %#x" % root)
        vtable = read(root, "ui.rootVtable")
        if vtable != self.vt["rootVtable"]:
            raise ValueError("Known UI root vtable mismatch: %#x" % vtable)
        return singleton, root, vtable

    def hud(self, out):
        self.ui = out
        tree, proofs = KnownTree(self.r, "ui."), {}
        traversal = {"stopReason": "not_started", "knownGraphComplete": False}
        out.update(proofs=[], anomalies=tree.anomalies, traversal=traversal, currentTreeMatches={},
                   phaseDecision="not_evaluated", completeLayoutValidationPerformed=False, csharpTryReadExecuted=False,
                   wholeTreeStabilityChecked=False, coverageIsPhaseEvidence=False)

        def inspect(kind, address, origin):
            key = kind, address
            if key in proofs:
                proofs[key]["origins"].append(origin)
                return
            entry = {"kind": kind, "address": hex(address), "origins": [origin], "currentRootReachability": "unknown", "initial": {}}
            proofs[key] = entry
            out["proofs"].append(entry)
            try:
                tree.trace(kind, address, self.ui_root[1], entry["initial"])
            except (SampleLimit, p.BudgetExceeded) as error:
                entry["initial"].update(termination="read_limit", error=p.failure(error))
                raise

        def select(kind, address):
            if kind not in out["currentTreeMatches"]:
                out["currentTreeMatches"][kind] = hex(address)
                inspect(kind, address, "current_root_walk")

        try:
            with self.r.partition("hud"):
                self.ui_root = self.read_root(self.q)
                out.update(zip(("singleton", "root", "rootVtable"), map(hex, self.ui_root)))
                for kind, address in self.hints.items():
                    inspect(kind, address, "reference_address")
                tree.walk(self.ui_root[1], select, traversal)
        except SampleLimit as error:
            traversal.update(stopReason="partition_limit", error=p.failure(error))
        except p.BudgetExceeded as error:
            traversal.update(stopReason="global_budget", error=p.failure(error))
            raise
        finally:
            out["cacheHits"] = dict(tree.hits)
            out["currentTreeCoverage"] = "known_graph_complete" if traversal["knownGraphComplete"] else "unknown"

    def invalidate_paths(self):
        if self.ui:
            for proof in self.ui["proofs"]:
                proof["currentRootReachability"] = "unknown"

    def recheck_ui(self):
        out = self.ui
        tree = KnownTree(self.r, "verify.ui.")
        out["verification"] = verification = {"status": "pending", "anomalies": tree.anomalies}
        if self.ui_root is None:
            verification.update(status="unknown", reason="initial_root_unavailable")
            return
        try:
            current = self.read_root(lambda address, field: self.r.q(address, "verify." + field))
            verification.update(singleton=hex(current[0]), root=hex(current[1]), rootVtable=hex(current[2]), rootStable=current == self.ui_root)
            if not verification["rootStable"]:
                verification.update(status="unknown", reason="root_changed_between_passes")
                return
            for proof in out["proofs"]:
                initial, repeated = proof["initial"], {}
                proof["recheck"] = repeated
                try:
                    tree.trace(proof["kind"], int(proof["address"], 16), current[1], repeated)
                except (SampleLimit, p.BudgetExceeded) as error:
                    repeated.update(termination="read_limit", error=p.failure(error))
                    raise
                proof["structureStable"] = structure(initial) == structure(repeated)
                if proof["structureStable"]:
                    proof["flags192Stable"] = [x["flags192"] for x in initial["nodes"]] == [x["flags192"] for x in repeated["nodes"]]
                if initial["firstPassPath"] and repeated["firstPassPath"] and proof["structureStable"]:
                    proof["currentRootReachability"] = "bidirectional_path_observed"
            after = self.read_root(lambda address, field: self.r.q(address, "verify.after." + field))
            verification.update(rootStable=after == current, status="checked" if after == current else "unknown",
                                finalSingleton=hex(after[0]), finalRoot=hex(after[1]), finalRootVtable=hex(after[2]))
        except (SampleLimit, p.BudgetExceeded, OSError, ValueError) as error:
            verification.update(status="unknown", error=p.failure(error))
        finally:
            verification["cacheHits"] = dict(tree.hits)
            if verification["status"] != "checked":
                self.invalidate_paths()

    def final_checks(self):
        # The watch list here covers camera/world and the current UI root, not the whole BFS.
        with self.r.partition("stability"):
            result = super().final_checks()
            self.r.finalizing = True
            try:
                self.recheck_ui()
            finally:
                self.r.finalizing = False
        return result

    def sample(self, scheduled_offset, schedule_start=None):
        result = super().sample(scheduled_offset, schedule_start)
        result["sameProcessIdentity"] &= (result["identityBefore"]["pid"] == result["identityAfter"]["pid"] == self.r.memory.pid and
                                           result["identityBefore"]["stillActive"])
        if not result["sameProcessIdentity"]:
            self.invalidate_paths()
        return result


def capture(args, memory_factory=p.WindowsMemory, clock=time.monotonic, sleeper=time.sleep):
    expected = p.start_filetime(args.expected_start_utc)
    if not 0 < args.pid <= 0xffffffff or args.label not in LABELS:
        raise ValueError("Explicit valid pid and label required")
    with Path(args.out).open("x", encoding="utf-8", newline="\n") as destination:
        started, memory, reader = clock(), None, None
        report = {"schema": "known-hud-phase-observation-v2", "startedUtc": p.utc_now(), "pid": args.pid,
                  "expectedStartUtc": p.filetime_utc(expected), "expectedStartFiletime": expected,
                  "operatorLabel": args.label, "operatorLabelIsObservedPhase": False,
                  "phaseDecision": "not_evaluated", "productAcceptance": False, "status": "pending",
                  "accessMask": "0x410", "handleClosed": None, "samples": [], "reference": None,
                  "baseProbe": {"path": str(BASE_PATH), "sha256": BASE_SHA256},
                  "roles": {name: {"legacyRva": hex(old), "candidateRva": hex(actual)} for name, (old, actual) in p.ROLES.items()},
                  "caps": {"seconds": p.MAX_SECONDS, "requestedBytes": p.MAX_BYTES, "readAttempts": p.MAX_READS,
                           "singleReadBytes": p.MAX_SINGLE, "uiNodesPerSample": MAX_NODES, "parents": p.MAX_PARENTS,
                           "childrenPerVector": p.MAX_CHILDREN, "perSamplePartitions": {
                               key: {"readAttempts": calls, "requestedBytes": size, "seconds": seconds}
                               for key, (calls, size, seconds) in PARTITIONS.items()}}}
        try:
            report["sourceManifest"] = p.load_sources()
            report["scriptSha256"] = sha256(Path(__file__).read_bytes())
            hints = {}
            if args.reference is not None:
                path = Path(args.reference).resolve(strict=True)
                reference = report["reference"] = {"path": str(path)}
                if not path.is_file():
                    raise ValueError("Reference must be a regular retained JSON file")
                with path.open("rb") as source:
                    raw = source.read(MAX_REFERENCE_BYTES + 1)
                if len(raw) > MAX_REFERENCE_BYTES:
                    raise ValueError("Reference exceeds 8 MiB")
                hints = parse_reference(raw, args.reference_sha256, args.pid, expected, reference)
            elif args.reference_sha256 is not None:
                raise ValueError("--reference-sha256 requires --reference")
            if clock() - started >= p.MAX_SECONDS - p.RESERVE_SECONDS:
                raise p.BudgetExceeded("Deadline reached before process open")
            memory = memory_factory(args.pid, expected)
            reader = Reader(memory, clock, started)
            report["identityBefore"] = memory.identity()
            if not same_identity(report["identityBefore"], args.pid, expected):
                raise ValueError("Process identity mismatch before attestation")
            module = report["module"] = memory.attest(reader.tick)
            if report["reference"] is not None:
                ref = report["reference"]
                ref["currentModuleMatch"] = all(module[key] == value for key, value in ref["module"].items())
                if not ref["currentModuleMatch"]:
                    raise ValueError("Reference base/SHA/image size differs from the current process module")
            schedule_start = clock()
            report["scheduleStartedUtc"] = p.utc_now()
            for offset in (0, 1, 3):
                remaining = schedule_start + offset - clock()
                if remaining > 0:
                    reader.tick()
                    if clock() + remaining - started >= p.MAX_SECONDS - p.RESERVE_SECONDS:
                        raise p.BudgetExceeded("Scheduled sample exceeds remaining deadline")
                    sleeper(remaining)
                reader.tick()
                if not same_identity(memory.identity(), args.pid, expected):
                    raise ValueError("Process identity changed before scheduled sample")
                result = Observation(reader, hints).sample(offset, schedule_start)
                report["samples"].append(result)
                if not result["sameProcessIdentity"]:
                    raise ValueError("Process identity changed during sample")
                verification_error = result["hud"].get("verification", {}).get("error", {}).get("type")
                if (any(result[name]["status"] == "budget_exhausted" for name in ("camera", "world", "hud", "stability")) or
                        verification_error == "BudgetExceeded"):
                    raise p.BudgetExceeded("Global budget stopped sampled fields; partial observations retained")
                reader.tick()
            report["identityAfter"] = memory.identity()
            report["status"] = "observed" if same_identity(report["identityAfter"], args.pid, expected) else "identity_changed"
        except Exception as error:
            report.update(status="budget_exhausted" if isinstance(error, p.BudgetExceeded) else "failed", error=p.failure(error))
        finally:
            if reader:
                report.update(counters=reader.stats(), readJournal=reader.journal)
            if memory:
                try:
                    memory.close()
                    report["handleClosed"] = memory.closed
                except Exception as error:
                    report.update(status="failed", handleCloseError=p.failure(error), handleClosed=False)
            report["finishedUtc"] = p.utc_now()
            report["elapsedSeconds"] = clock() - started
            report["deadlineOverrunSeconds"] = max(0.0, report["elapsedSeconds"] - p.MAX_SECONDS)
            json.dump(report, destination, ensure_ascii=False, indent=2, allow_nan=False)
            destination.write("\n")
    return report


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pid", type=int, required=True)
    parser.add_argument("--expected-start-utc", required=True, help="Exact UTC, up to seven fractional digits")
    parser.add_argument("--label", choices=LABELS, required=True, help="Operator label")
    parser.add_argument("--out", required=True, help="New JSON file path")
    parser.add_argument("--reference", help="Optional v1 running/paused JSON from the same process")
    parser.add_argument("--reference-sha256", help="Exact lower-case byte SHA256 required with --reference")
    args = parser.parse_args(argv)
    report = capture(args)
    print(json.dumps({"output": str(Path(args.out).resolve()), "status": report["status"], "samples": len(report["samples"]),
                      "phaseDecision": report["phaseDecision"], "counters": report.get("counters"),
                      "handleClosed": report["handleClosed"]}, ensure_ascii=False))
    return 0 if report["status"] == "observed" else 3


if __name__ == "__main__":
    raise SystemExit(main())
