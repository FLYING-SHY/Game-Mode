#!/usr/bin/env python3
"""Frozen v3 loader/v2 measurement with strictly validated genuine v3 address hints."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import time
from types import ModuleType

HERE = Path(__file__).resolve().parent
V3_PATH = HERE.parent / "v3/phase_probe_v3.py"
V3_SHA256 = "81ee593a762933f0cf119cbc6fdcafc37cfa687ddd171aad6aa6f99dbf7da66b"
SCHEMA = "known-hud-phase-observation-v4"
SOURCE_BYTES = {"client/CoH3MemoryTool/GameLayout52426.cs": 4166,
                "client/CoH3MemoryTool/PlayerInfoReader.cs": 105047,
                "evidence/version-adaptation/air/air_raw_snapshot.py": 40147}


def sha256(raw):
    return hashlib.sha256(raw).hexdigest()


def require(condition, field, reason):
    if not condition:
        raise ValueError("v3 reference: %s: %s" % (field, reason))


def equal(actual, expected, field):
    # Serialized comparison also distinguishes false/0 and true/1 recursively.
    require(json.dumps(actual, sort_keys=True, allow_nan=False) ==
            json.dumps(expected, sort_keys=True, allow_nan=False), field, "differs from the frozen expected value")


def object_at(parent, key, field):
    value = parent.get(key)
    require(type(value) is dict, field, "required object missing or malformed")
    return value


def array_at(parent, key, field):
    value = parent.get(key)
    require(type(value) is list, field, "required array missing or malformed")
    return value


def integer(value, low, high, field):
    require(type(value) is int and low <= value <= high, field, "integer outside the permitted range")
    return value


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        require(key not in result, key, "duplicate JSON key")
        result[key] = value
    return result


def invalid_constant(value):
    raise ValueError("v3 reference: non-finite JSON constant: " + value)


class V3ReferenceParser:
    """A reference supplies addresses only; frozen Observation performs all current reads."""
    def __init__(self, engine):
        self.engine, self.p = engine, engine.p
        self.attestation = engine.v3_attestation
        binding = self.attestation["sourceBinding"]
        self.manifest = {key: {"path": str(Path(binding["sourceRoot"]) / key),
                               "sha256": digest, "bytes": SOURCE_BYTES[key]}
                         for key, digest in binding["effectiveSourceLoaderPins"].items()}

    def hex_number(self, value, field, pointer=True):
        require(type(value) is str and re.fullmatch(r"0x(?:0|[1-9a-f][0-9a-f]*)", value) is not None,
                field, "canonical lower-case hex required")
        number = int(value, 16)
        if pointer:
            require(self.p.pointer(number), field, "invalid aligned user pointer")
        return number

    def identity(self, value, pid, start, field):
        require(type(value) is dict, field, "identity object missing")
        equal(value.get("pid"), pid, field + ".pid")
        equal(value.get("startFiletime"), start, field + ".startFiletime")
        equal(self.p.start_filetime(value.get("startUtc", "")), start, field + ".startUtc (100 ns)")
        equal(value.get("stillActive"), True, field + ".stillActive")
        equal(value.get("exitCode"), 259, field + ".exitCode")

    def witness(self, witness, child, field, reads, prefix):
        require(type(witness) is dict, field, "listed edge has no witness object")
        equal(witness.get("pointerRawHex"), child.to_bytes(8, "little").hex(), field + ".pointerRawHex")
        if "inlineOffset" in witness:
            equal(sorted(witness), ["inlineOffset", "pointerRawHex"], field + ".keys")
            offset = integer(witness["inlineOffset"], 144, 568, field + ".inlineOffset")
            require(offset % 8 == 0, field + ".inlineOffset", "not a frozen inline child slot")
            return
        equal(sorted(witness), ["index", "pointerRawHex", "vector"], field + ".keys")
        vector = object_at(witness, "vector", field + ".vector")
        equal(sorted(vector), ["address", "count124", "data112", "elementsSha256"], field + ".vector.keys")
        address = self.hex_number(vector.get("address"), field + ".vector.address")
        data = self.hex_number(vector.get("data112"), field + ".vector.data112")
        count = integer(vector.get("count124"), 1, self.p.MAX_CHILDREN, field + ".vector.count124")
        integer(witness.get("index"), 0, count - 1, field + ".index")
        self.p.span(data, count * 8)
        require(type(vector.get("elementsSha256")) is str and
                re.fullmatch("[0-9a-f]{64}", vector["elementsSha256"]) is not None,
                field + ".vector.elementsSha256", "lower-case SHA256 required")
        for entry in ((prefix + "vectorHeader112", address + 112, 16),
                      (prefix + "vectorElements", data, count * 8)):
            require(entry in reads, field + ".vector", "matching successful raw journal read missing: %r" % (entry,))

    def trace(self, trace, kind, control, root, base, field, reads, prefix):
        require(type(trace) is dict, field, "initial/recheck trace missing")
        require("error" not in trace, field, "trace includes an error")
        for key, expected in (("firstPassPath", True), ("termination", "current_root_reached"),
                              ("control", hex(control)), ("controlVtable", hex(base + self.p.ROLES[kind + "Vtable"][1])),
                              ("hudAncestorSeen", True), ("controlSeen", True)):
            equal(trace.get(key), expected, field + "." + key)
        nodes = array_at(trace, "nodes", field + ".nodes")
        edges = array_at(trace, "edges", field + ".edges")
        require(2 <= len(nodes) <= self.p.MAX_PARENTS, field + ".nodes", "path length outside frozen parent bound")
        equal(len(edges), len(nodes) - 1, field + ".edges.length")
        addresses, vtables, parents, flags = [], [], [], []
        for index, node in enumerate(nodes):
            where = field + ".nodes[%d]" % index
            require(type(node) is dict, where, "node object missing")
            address = self.hex_number(node.get("address"), where + ".address")
            vtable = self.hex_number(node.get("vtable"), where + ".vtable")
            parent = self.hex_number(node.get("parent136"), where + ".parent136", pointer=False)
            require(parent == 0 or self.p.pointer(parent), where + ".parent136", "invalid parent pointer")
            flag = self.hex_number(node.get("flags192"), where + ".flags192", pointer=False)
            require(0 <= flag <= 0xffffffff, where + ".flags192", "not uint32")
            equal(node.get("bit20Set"), bool(flag & 0x20), where + ".bit20Set")
            require(64307200 <= vtable - base < min(96444416, self.p.IMAGE_SIZE), where + ".vtable", "outside frozen ancestor range")
            require((prefix + "body576", address, 576) in reads, where, "matching successful body576 journal read missing")
            addresses.append(address)
            vtables.append(vtable)
            parents.append(parent)
            flags.append(flag)
        require(len(set(addresses)) == len(addresses), field + ".nodes", "cycle or repeated node")
        equal(addresses[-1], root, field + ".terminalRoot")
        equal(vtables[-1], base + self.p.ROLES["rootVtable"][1], field + ".terminalRootVtable")
        equal(parents, addresses[1:] + [0], field + ".parent136 chain")
        require(control in addresses, field + ".controlSeen", "control absent from actual nodes")
        equal(vtables[addresses.index(control)], base + self.p.ROLES[kind + "Vtable"][1], field + ".control node vtable")
        hud_nodes = [address for address, vtable in zip(addresses, vtables)
                     if vtable == base + self.p.ROLES["hudVtable"][1]]
        require(len(hud_nodes) == 1, field + ".hudAncestorSeen", "requires exactly one actual HUD ancestor")
        equal(trace.get("allChainBit20Set"), all(flag & 0x20 for flag in flags), field + ".allChainBit20Set")
        if kind == "hud":
            equal(addresses[0], control, field + ".firstNode")
            require("templateField" not in trace, field + ".templateField", "HUD has no map template field")
        else:
            equal(trace.get("templateField"), {"offset": 472 if kind == "minimap" else 528,
                                               "pointer": hex(addresses[0])}, field + ".templateField")
            if kind == "minimap":
                equal(parents[0], control, field + ".template parent")
                equal(trace.get("templateParentMatchesControl"), True, field + ".templateParentMatchesControl")
        for index, edge in enumerate(edges):
            where = field + ".edges[%d]" % index
            require(type(edge) is dict, where, "edge object missing")
            for key, expected in (("child", hex(addresses[index])), ("parent", hex(addresses[index + 1])),
                                  ("childParent136", hex(addresses[index + 1])), ("downwardMembership", "listed")):
                equal(edge.get(key), expected, where + "." + key)
            self.witness(edge.get("witness"), addresses[index], where + ".witness", reads, prefix)
        return hud_nodes[0], flags

    def journal(self, doc):
        entries = array_at(doc, "readJournal", "readJournal")
        require(0 < len(entries) <= self.p.MAX_READS, "readJournal", "empty or exceeds frozen global read bound")
        requested = returned = 0
        for index, entry in enumerate(entries):
            where = "readJournal[%d]" % index
            require(type(entry) is dict, where, "journal entry missing")
            equal(entry.get("ordinal"), index + 1, where + ".ordinal")
            size = integer(entry.get("length"), 1, self.p.MAX_SINGLE, where + ".length")
            address = self.hex_number(entry.get("address"), where + ".address", pointer=False)
            self.p.span(address, size)
            require(entry.get("status") in ("read", "failed"), where + ".status", "not a raw read outcome")
            actual = entry.get("actualBytes", 0)
            integer(actual, 0, size, where + ".actualBytes")
            if entry["status"] == "read":
                equal(actual, size, where + ".successful read length")
            requested += size
            returned += actual
        require(requested <= self.p.MAX_BYTES, "readJournal", "exceeds frozen byte budget")
        counters = object_at(doc, "counters", "counters")
        for key, expected in (("readAttempts", len(entries)), ("requestedBytes", requested), ("returnedBytes", returned)):
            equal(counters.get(key), expected, "counters." + key)
        return entries

    def __call__(self, raw, expected_sha, pid, start, metadata):
        actual = sha256(raw)
        metadata.update(sha256=actual, expectedSha256=expected_sha, bytes=len(raw), hintsOnly=True,
                        reportedScriptShaIsAttestation=False, parser="strict-genuine-v3-reference-v4")
        require(len(raw) < self.engine.MAX_REFERENCE_BYTES, "bytes", "reference must be smaller than 8 MiB")
        require(type(expected_sha) is str and re.fullmatch("[0-9a-f]{64}", expected_sha) is not None and actual == expected_sha,
                "sha256", "byte SHA256 differs from --reference-sha256")
        try:
            doc = json.loads(raw, object_pairs_hook=unique_object, parse_constant=invalid_constant)
            require(type(doc) is dict, "document", "root object required")
            return self.parse(doc, pid, start, metadata)
        except (KeyError, TypeError, AttributeError, IndexError, RecursionError) as error:
            raise ValueError("v3 reference: malformed or incomplete field: %s" % error) from error

    def parse(self, doc, pid, start, metadata):
        for key, expected in (("schema", "known-hud-phase-observation-v3"), ("scriptSha256", V3_SHA256),
                              ("status", "observed"), ("handleClosed", True), ("sourceBindingVerified", True),
                              ("phaseDecision", "not_evaluated"), ("operatorLabelIsObservedPhase", False),
                              ("productAcceptance", False), ("accessMask", "0x410")):
            equal(doc.get(key), expected, key)
        require(doc.get("operatorLabel") in ("running", "paused"), "operatorLabel", "requires running/paused operator label")
        equal(doc.get("executionAttestation"), self.attestation, "executionAttestation (all three frozen scripts/sourceBinding)")
        equal(doc.get("sourceManifest"), self.manifest, "sourceManifest (fixed paths/SHA/bytes)")
        equal(doc.get("frozenEngineReport"), {"schema": "known-hud-phase-observation-v2",
                                             "scriptSha256": self.attestation["measurementEngine"]["sha256"],
                                             "reportedDiskShaMatchesExecutedSource": True}, "frozenEngineReport")
        equal(doc.get("baseProbe"), {key: self.attestation["baseProbe"][key] for key in ("path", "sha256")}, "baseProbe")
        equal(doc.get("roles"), {key: {"legacyRva": hex(old), "candidateRva": hex(value)}
                                 for key, (old, value) in self.p.ROLES.items()}, "roles")
        equal(doc.get("pid"), pid, "pid")
        equal(doc.get("expectedStartFiletime"), start, "expectedStartFiletime")
        equal(self.p.start_filetime(doc.get("expectedStartUtc", "")), start, "expectedStartUtc (100 ns)")
        for key in ("identityBefore", "identityAfter"):
            self.identity(doc.get(key), pid, start, key)
        module = object_at(doc, "module", "module")
        base = self.hex_number(module.get("base"), "module.base")
        equal(module.get("sha256"), self.p.GAME_SHA256, "module.sha256")
        equal(module.get("imageSize"), self.p.IMAGE_SIZE, "module.imageSize")
        equal(module.get("fileStableDuringHash"), True, "module.fileStableDuringHash")
        integer(module.get("fileBytes"), 1, 256 * 1024 * 1024, "module.fileBytes")
        require(base + self.p.IMAGE_SIZE < self.p.USER_END, "module.base", "image exceeds user address range")
        metadata.update(schema=doc["schema"], reportedScriptSha256=doc["scriptSha256"], operatorLabel=doc["operatorLabel"],
                        pid=pid, expectedStartFiletime=start, module={key: module[key] for key in ("base", "sha256", "imageSize")},
                        currentModuleMatch=None, referenceRootIsCurrentRootRequirement=False)
        samples = array_at(doc, "samples", "samples")
        equal([sample.get("scheduledOffsetSeconds") for sample in samples], [0, 1, 3], "original 0/1/3-second sample set")
        entries = self.journal(doc)
        hints, sample_details, previous_reads = None, [], 0
        for index, sample in enumerate(samples):
            where = "samples[%d]" % index
            equal(sample.get("sameProcessIdentity"), True, where + ".sameProcessIdentity")
            for key in ("identityBefore", "identityAfter"):
                self.identity(sample.get(key), pid, start, where + "." + key)
            end = integer(object_at(sample, "counters", where + ".counters").get("readAttempts"),
                          previous_reads + 1, len(entries), where + ".counters.readAttempts")
            reads = {(entry["field"], int(entry["address"], 16), entry["length"])
                     for entry in entries[previous_reads:end] if entry["status"] == "read"}
            previous_reads = end
            hud = object_at(sample, "hud", where + ".hud")
            equal(hud.get("status"), "observed", where + ".hud.status")
            equal(hud.get("phaseDecision"), "not_evaluated", where + ".hud.phaseDecision")
            root = self.hex_number(hud.get("root"), where + ".hud.root")
            singleton = self.hex_number(hud.get("singleton"), where + ".hud.singleton")
            equal(hud.get("rootVtable"), hex(base + self.p.ROLES["rootVtable"][1]), where + ".hud.rootVtable")
            verification = object_at(hud, "verification", where + ".hud.verification")
            for key, expected in (("status", "checked"), ("rootStable", True), ("root", hex(root)),
                                  ("finalRoot", hex(root)), ("singleton", hex(singleton)), ("finalSingleton", hex(singleton)),
                                  ("rootVtable", hud["rootVtable"]), ("finalRootVtable", hud["rootVtable"])):
                equal(verification.get(key), expected, where + ".hud.verification." + key)
            proofs = array_at(hud, "proofs", where + ".hud.proofs")
            require(1 <= len(proofs) <= len(self.engine.KINDS), where + ".hud.proofs", "missing proofs or duplicate/ambiguous hint set")
            current, hud_ancestors = {}, []
            for proof in proofs:
                require(type(proof) is dict, where + ".hud.proofs", "proof object missing")
                kind = proof.get("kind")
                require(type(kind) is str and kind in self.engine.KINDS, where + ".kind", "unknown kind")
                field = where + ".hud.proofs." + kind
                require(kind not in current, field, "duplicate/ambiguous hint kind")
                control = self.hex_number(proof.get("address"), field + ".address")
                require(control != root and control not in current.values(), field + ".address", "root or shared/ambiguous control address")
                equal(proof.get("currentRootReachability"), "bidirectional_path_observed", field + ".currentRootReachability")
                equal(proof.get("structureStable"), True, field + ".structureStable")
                initial = object_at(proof, "initial", field + ".initial")
                repeated = object_at(proof, "recheck", field + ".recheck")
                first_hud, first_flags = self.trace(initial, kind, control, root, base, field + ".initial", reads, "ui.")
                last_hud, last_flags = self.trace(repeated, kind, control, root, base, field + ".recheck", reads, "verify.ui.")
                equal(self.engine.structure(initial), self.engine.structure(repeated), field + ".actual two-pass structure")
                equal(proof.get("flags192Stable"), first_flags == last_flags, field + ".flags192Stable")
                current[kind] = control
                hud_ancestors.extend((first_hud, last_hud))
            require("hud" in current, where + ".hud.proofs", "unique HUD hint required")
            equal(hud_ancestors, [current["hud"]] * len(hud_ancestors), where + ".actual HUD ancestor addresses")
            if hints is not None:
                equal(current, hints, where + ".hints")
            hints = current
            sample_details.append({"scheduledOffsetSeconds": sample["scheduledOffsetSeconds"], "root": hex(root),
                                   "currentTreeCoverage": hud.get("currentTreeCoverage"), "traversal": hud.get("traversal"),
                                   "validatedProofKinds": sorted(current)})
        equal(previous_reads, len(entries), "final sample journal boundary")
        metadata.update(hints={kind: hex(value) for kind, value in hints.items()},
                        candidateCounts={kind: int(kind in hints) for kind in self.engine.KINDS},
                        sampleOffsetSeconds=3, sampleStartedUtc=samples[-1].get("startedUtc"),
                        root=sample_details[-1]["root"], samples=sample_details,
                        hintPolicy="one unique HUD required; optional kinds must have complete two-pass proofs in all three samples")
        return hints


def load_engine():
    raw = V3_PATH.read_bytes()
    if sha256(raw) != V3_SHA256:
        raise ValueError("Frozen v3 wrapper SHA256 mismatch")
    wrapper = ModuleType("frozen_phase_probe_v3_for_v4")
    wrapper.__file__ = str(V3_PATH)
    exec(compile(raw, str(V3_PATH), "exec"), wrapper.__dict__)
    engine = wrapper.load_engine()
    engine.parse_reference = V3ReferenceParser(engine)
    attestation = {"schema": "phase-probe-execution-attestation-v4",
                   "wrapper": {"path": str(Path(__file__).resolve()), "sha256": sha256(Path(__file__).read_bytes()),
                               "digestMeaning": "wrapper disk bytes at load; self-reported, not a signature"},
                   "frozenV3LoaderAttestation": engine.v3_attestation,
                   "overrides": {"referenceParser": "strict genuine pinned v3 reports; addresses only; no v1 conversion",
                                 "reportAdapter": "v4 provenance replaces the v3 JSON sink",
                                 "measurementFunctionsChanged": False,
                                 "currentModuleComparison": "unchanged v2 base/SHA/imageSize equality after module attestation",
                                 "currentRootPolicy": "reference root is historical metadata; read and recheck the current root"},
                   "boundaries": {"phaseDecision": "not_evaluated", "productAcceptance": False,
                                  "operatorLabelIsObservedPhase": False, "productSnapshotCalled": False}}
    engine.json = ReportJson(attestation)
    engine.v4_attestation = attestation
    return engine


class ReportJson:
    def __init__(self, attestation):
        self.attestation = attestation

    def __getattr__(self, name):
        return getattr(json, name)

    def dump(self, report, destination, **kwargs):
        loader = self.attestation["frozenV3LoaderAttestation"]
        report["frozenEngineReport"] = {"schema": report["schema"], "scriptSha256": report.get("scriptSha256"),
            "reportedDiskShaMatchesExecutedSource": report.get("scriptSha256") == loader["measurementEngine"]["sha256"]
            if "scriptSha256" in report else None}
        report.update(schema=SCHEMA, scriptSha256=self.attestation["wrapper"]["sha256"],
                      executionAttestation=self.attestation,
                      sourceBindingVerified=report.get("sourceManifest") ==
                      self._manifest(loader["sourceBinding"]))
        return json.dump(report, destination, **kwargs)

    @staticmethod
    def _manifest(binding):
        return {key: {"path": str(Path(binding["sourceRoot"]) / key), "sha256": digest, "bytes": SOURCE_BYTES[key]}
                for key, digest in binding["effectiveSourceLoaderPins"].items()}


def capture(args, memory_factory=None, clock=time.monotonic, sleeper=time.sleep):
    output = Path(args.out).resolve()
    if not output.is_relative_to(HERE):
        raise ValueError("v4 output must remain below %s" % HERE)
    engine = load_engine()
    factory = engine.p.WindowsMemory if memory_factory is None else memory_factory
    return engine.capture(args, memory_factory=factory, clock=clock, sleeper=sleeper)


def offline_attestation():
    engine = load_engine()
    return {"schema": "phase-probe-source-check-v4", "runMode": "offline_source_verification_only",
            "nativeOrGameCallsMade": False, "realSamples": 0, "phaseDecision": "not_evaluated",
            "productAcceptance": False, "sourceManifest": engine.p.load_sources(),
            "executionAttestation": engine.v4_attestation}


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--offline-attest", action="store_true", help="Source-only verification; no process access")
    parser.add_argument("--pid", type=int)
    parser.add_argument("--expected-start-utc", help="Exact UTC, up to seven fractional digits")
    parser.add_argument("--label", choices=("running", "paused", "results", "mainmenu", "newmatch"))
    parser.add_argument("--out", help="New exclusive JSON below v4")
    parser.add_argument("--reference", help="Genuine pinned v3 running/paused JSON from this process/module")
    parser.add_argument("--reference-sha256", help="Exact lower-case reference byte SHA256")
    args = parser.parse_args(argv)
    if args.offline_attest:
        if any(value is not None for key, value in vars(args).items() if key != "offline_attest"):
            parser.error("--offline-attest is a standalone source-only check")
        print(json.dumps(offline_attestation(), ensure_ascii=False, indent=2))
        return 0
    if any(getattr(args, key) is None for key in ("pid", "expected_start_utc", "label", "out")):
        parser.error("live invocation requires --pid, --expected-start-utc, --label and --out")
    report = capture(args)
    print(json.dumps({"output": str(Path(args.out).resolve()), "schema": report["schema"], "status": report["status"],
                      "samples": len(report["samples"]), "phaseDecision": report["phaseDecision"],
                      "counters": report.get("counters"), "handleClosed": report["handleClosed"],
                      "sourceBindingVerified": report["sourceBindingVerified"]}, ensure_ascii=False))
    return 0 if report["status"] == "observed" else 3


if __name__ == "__main__":
    raise SystemExit(main())
