#!/usr/bin/env python3
"""Index fixed historical observations. No probe import, process access, or current C# check.

Default: exclusively create the two fixed new deliverables after full validation.
--check: rebuild in memory and compare both existing deliverables, without writes.
Run with Python -B -X utf8. SHA pins are integrity bindings, not signatures.
"""
import sys
sys.dont_write_bytecode = True

import argparse
import hashlib
import json
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
EVIDENCE = "evidence/version-adaptation/"
PHASE = EVIDENCE + "lifecycle-phase/"
LIVE = EVIDENCE + "session-lifecycle-20261122/round3-live/"
INDEX = LIVE + "round3-v4-observation-index.json"
FINDINGS = LIVE + "round3-v4-findings.md"
REFERENCE = PHASE + "v3/live-round3/pid30504-paused-001.json"
REF_SHA = "ba29370662feeab744da3fa20cbf3812eb69ae5d9b2fd79fcf7f63f4591696fd"
PID, START = 30504, 134397842153993114
START_UTC = "2026-11-22T01:23:35.3993114Z"
MODULE_SHA = "8a3445292255d652c8dbe9b53f24328af159c161be89836ba54ef070946dd3ec"
MODULE_BASE, MODULE_SIZE = "0x7ff6985b0000", 120369152
UI_ROOT, ROOT_VTABLE = "0x1d2a08a61c0", "0x7ff69c384bf0"
VTABLES = {"hud": "0x7ff69c45c6b0", "minimap": "0x7ff69c42b160", "tactical": "0x7ff69c42b810"}
OLD = {"hud": "0x1d3cc5ec570", "minimap": "0x1d3cca73b30", "tactical": "0x1d3ccc60fb0"}
NEW = {"hud": "0x1d098515200", "minimap": "0x1d3b7dad490", "tactical": "0x1d2ae303150"}
SHAPES = {"hud": (12, 11, 9, 2), "minimap": (26, 25, 14, 11), "tactical": (22, 21, 15, 6)}
PROBE_PINS = {
    "phase_probe.py": "21af7198c86ea5f53b06e9df737251d81a21dd345ec243b8974beca75ec1d723",
    "v2/phase_probe_v2.py": "e2c56be1b7b49f242b4549545e03e4a50d765e5de9c9cf7b8473a9966525e7c0",
    "v3/phase_probe_v3.py": "81ee593a762933f0cf119cbc6fdcafc37cfa687ddd171aad6aa6f99dbf7da66b",
    "v4/phase_probe_v4.py": "b8cc05b3ae3ed8c162bd6119b2f7896ec23f389ab273c1337c03eac3f1dcb23f",
}
# These are capture-time pins. Never read/re-pin the current production source here.
HISTORICAL_SOURCES = {
    "client/CoH3MemoryTool/GameLayout52426.cs": ("90af6a0f98c5c3c48dd9cad8595377ee90990938753e68b142899b707c596d47", 4166),
    "client/CoH3MemoryTool/PlayerInfoReader.cs": ("39dde7a0f2bd80e2412fed3439870cf7a49a794a9aab0f812a9afee6f60c65d2", 105047),
    "evidence/version-adaptation/air/air_raw_snapshot.py": ("0438ebbe4b29bcae3b6d428e46ce2db0fcdf58c87d93f6058f0432dc9ff765d4", 40147),
}
# id, raw suffix, bytes, SHA, screenshot ids, role, lead's existing read_image finding
SPECS = (
    ("results", "results-ref-paused-001", 4085712, "76c8968954b3ff94e42cb60354a2e88f2ce6f810cecc968b76c90315e50e7889", (23, 24), "primary", "费蒙维尔结果页，02:58；前后均由 lead 亲读图确认。"),
    ("mainmenu", "mainmenu-ref-paused-001", 4255670, "6d0a41c32016e10628fbf706e48fa0cdde54799fc2f92bbba999edd022a34765", (26, 27), "primary", "主菜单；state26 是本次 probe 的 before，state27 为 after。"),
    ("second-running002", "second-running-ref-paused-002", 4989658, "9fc92f78a114175a215552fb99965ab9b681803a410cb8bcf02c124aec4e726b", (37, 38), "primary", "第二局运行；04:13/资源1531 → 04:17/资源1548，lead 亲读图。"),
    ("second-paused001", "second-paused-ref-paused-001", 4992125, "950b7254e1508bf4e5301bf18a6346897735f88daed40b8089702d674e56ff1e", (40, 41), "primary", "第二局 Esc 菜单；前后均 07:39/资源2458，lead 亲读图。"),
    ("second-running001", "second-running-ref-paused-001", 4992801, "83f6707611dde8d8d3e97d232d4e1b0a13e99271f0333ef9c79578257d7ccfa5", (35, 36), "preliminary_only", "第二局运行的先行观测；截图间隔较大，002 作为主要运行证据。"),
)
# Each PNG and its selected-window sidecar are separately fixed.
CAPTURES = {
    23: ("state-23-results-v4-before", "1b1765c97c843f4131e1552e233bf471c644959ea274b5d2da6524d202bc0d93", "ff913ab02b1fe2f28ff3d19866146f91bb9d4ed299dc431cd074ae25d8714ea2"),
    24: ("state-24-results-v4-after", "3c6a3e656a2130f0d5468b392638e4e7b38d6fc3722300202a14329bdb829f1f", "5d91a4fcf9ad56de114acc7f4efd0d95bac7daa43036f7226c80709108fe1c5a"),
    26: ("state-26-stable-after-results-close", "3f0c7c43b3e108f5bc9b2cc859bd1e245049de39bf187b35948be09c354dddb6", "490c0e4bd65c2e71e4028acd4aad8d73f200441baeb5f477225e724029a16167"),
    27: ("state-27-mainmenu-v4-after", "dc659a7a65d411b4ece65c18ae947b330d33ceaf469cc3a731415dcd244061b8", "ea6d91fc08b6d21243d21442a0cf1f4be678564af55eff467e1a471a341b88bb"),
    35: ("state-35-second-match-running-before", "7942970f6ca8f3443d78f4a843989d7dfeefc8e5efae9a244310f8269ed37d8b", "00a9c7089f82f0ca363e2761681641311aa049013118ca9048e7f31e0784acf7"),
    36: ("state-36-second-match-running-after", "620ea34bd1475906b5106ca5ee7df928d0b7fbb33c4abd3ba5d970547b114895", "7fef1a8229908a29d0c7eec8360d04de21fb8d8520c83cff5084a13c994383a5"),
    37: ("state-37-second-match-tight-before", "ea91d5a8c3ba920d5d79b9259343e8d2391c7c90506f6b309c16cbeddec3482e", "4f8118a5004640e222323d300d28f16de285cddf229afb8e5bc9dc4803c050a1"),
    38: ("state-38-second-match-tight-after", "3667dc27b52d9c98b7bbe3e9344b21f70822eb4ed921d735a208d3756f713287", "e05ef9d9cad5edc49acb0ee305ab4f214cf47b09f3a4c947330ce13c7fa375b0"),
    40: ("state-40-second-paused-tight-before", "e2726ef7135af31f09fea754f4b2e5d727bcd5a2655e5bbd348ecbe8eba97bae", "730832019e288a8fe3cac3a2b0d6d52e46fa5da35d0943b619521e196a98c3ab"),
    41: ("state-41-second-paused-tight-after", "ecc2e3ae8be1752424f0fb5f20137289fe2c20430a5afec417f3c93f8c8b6d85", "e5f68492f303c34177320503613c1529b66c82d769ffcfda83fe02bba20447a7"),
}


def require(condition, message):
    if not condition:
        raise ValueError(message)


def digest(raw):
    return hashlib.sha256(raw).hexdigest()


def encoded(value):
    return (json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False) + "\n").encode("utf-8")


def checked_file(path, sha, size=None):
    raw = (ROOT / path).read_bytes()
    require(digest(raw) == sha, "fixed SHA mismatch: " + path)
    require(size is None or len(raw) == size, "fixed size mismatch: " + path)
    return raw, {"path": path, "sha256": sha, "bytes": len(raw)}


def utc(value):
    return datetime.fromisoformat(value.replace("Z", "+00:00")).astimezone(timezone.utc)


def same_path(left, right):
    return left.replace("\\", "/").lower() == str(right).replace("\\", "/").lower()


def validate_common(doc, version):
    require(doc["schema"] == "known-hud-phase-observation-" + version, "raw schema")
    require((doc["pid"], doc["expectedStartFiletime"], doc["expectedStartUtc"]) == (PID, START, START_UTC), "raw identity")
    for key in ("identityBefore", "identityAfter"):
        ident = doc[key]
        require((ident["pid"], ident["startFiletime"]) == (PID, START) and ident["stillActive"] is True, "identity attestation")
    require(doc["status"] == "observed" and doc["handleClosed"] is True and doc["accessMask"] == "0x410", "raw read-only completion")
    require(doc["sourceBindingVerified"] is True, "raw source binding")
    require(doc["phaseDecision"] == "not_evaluated" and doc["productAcceptance"] is False and doc["operatorLabelIsObservedPhase"] is False, "raw nonclaims")
    module = doc["module"]
    require((module["sha256"], module["base"], module["imageSize"], module["fileBytes"]) == (MODULE_SHA, MODULE_BASE, MODULE_SIZE, 108391096), "raw module")
    require(module["fileStableDuringHash"] is True, "module hash stability")
    manifest = doc["sourceManifest"]
    require(set(manifest) == set(HISTORICAL_SOURCES), "historical source paths")
    for path, (sha, size) in HISTORICAL_SOURCES.items():
        require((manifest[path]["sha256"], manifest[path]["bytes"]) == (sha, size), "historical source pin: " + path)
        require(same_path(manifest[path]["path"], ROOT / path), "historical source path: " + path)
    att = doc["executionAttestation"]
    require(doc["scriptSha256"] == PROBE_PINS[version + "/phase_probe_" + version + ".py"], "reported wrapper hash")
    require(att["wrapper"]["sha256"] == doc["scriptSha256"], "wrapper attestation")
    if version == "v4":
        require(att["schema"] == "phase-probe-execution-attestation-v4", "v4 attestation schema")
        require(att["overrides"]["measurementFunctionsChanged"] is False, "frozen measurement functions")
        require(att["boundaries"] == {"phaseDecision": "not_evaluated", "productAcceptance": False, "operatorLabelIsObservedPhase": False, "productSnapshotCalled": False}, "v4 attestation boundaries")
        att = att["frozenV3LoaderAttestation"]
    require(att["wrapper"]["sha256"] == PROBE_PINS["v3/phase_probe_v3.py"], "frozen v3 loader")
    require(att["measurementEngine"]["sha256"] == PROBE_PINS["v2/phase_probe_v2.py"] and att["baseProbe"]["sha256"] == PROBE_PINS["phase_probe.py"], "frozen engine chain")
    require(att["sourceBinding"]["effectiveSourceLoaderPins"] == {k: v[0] for k, v in HISTORICAL_SOURCES.items()}, "historical loader pins")
    require(att["sourceBinding"]["sourceFilesExecuted"] is False and att["sourceBinding"]["loaderCodeUnchanged"] is True, "source binding boundary")
    for key in ("productSnapshotCalled", "productCSharpExecuted", "productBuildCorrespondenceEstablished", "phasePredicateImplemented", "productAcceptance"):
        require(att["boundaries"][key] is False, "v3 boundary: " + key)
    require([s["scheduledOffsetSeconds"] for s in doc["samples"]] == [0, 1, 3], "sample schedule")
    caps = doc["caps"]
    require((caps["seconds"], caps["requestedBytes"], caps["readAttempts"], caps["singleReadBytes"]) == (10.0, 2097152, 20000, 65536), "fixed total caps")
    require(doc["deadlineOverrunSeconds"] == 0 and doc["counters"]["elapsedSeconds"] <= caps["seconds"], "observation deadline")


def audit_journal(doc):
    rows, counters = doc["readJournal"], doc["counters"]
    requested = returned = 0
    failed = 0
    for ordinal, row in enumerate(rows, 1):
        require(row["ordinal"] == ordinal and 0 < row["length"] <= doc["caps"]["singleReadBytes"], "journal ordinal/length")
        actual = row.get("actualBytes", 0)
        require(row["status"] in ("read", "failed"), "journal status")
        if row["status"] == "read":
            require(actual == row["length"], "journal short read")
        else:
            require(actual == 0 and "actual=0" in row["error"]["message"], "failed-read accounting")
            failed += 1
        if "rawHex" in row:
            require(len(bytes.fromhex(row["rawHex"])) == actual, "journal rawHex size")
        requested += row["length"]
        returned += actual
    require((len(rows), requested, returned) == tuple(counters[k] for k in ("readAttempts", "requestedBytes", "returnedBytes")), "journal totals")
    require(len(rows) <= doc["caps"]["readAttempts"] and requested <= doc["caps"]["requestedBytes"], "total read budget")
    require(all(doc["samples"][-1]["counters"][k] == counters[k] for k in ("readAttempts", "requestedBytes", "returnedBytes")), "final cumulative counters")
    groups, previous, prev_requested, prev_returned = [], 0, 0, 0
    for sample in doc["samples"]:
        end = sample["counters"]["readAttempts"]
        group = rows[previous:end]
        require(len(group) == end - previous > 0, "sample journal boundary")
        req, ret = sum(r["length"] for r in group), sum(r.get("actualBytes", 0) for r in group)
        require((req, ret) == (sample["counters"]["requestedBytes"] - prev_requested, sample["counters"]["returnedBytes"] - prev_returned), "sample byte counters")
        require(all(utc(sample["startedUtc"]) <= utc(r["utc"]) <= utc(sample["finishedUtc"]) for r in group), "sample journal timestamp span")
        groups.append(group)
        previous, prev_requested, prev_returned = end, sample["counters"]["requestedBytes"], sample["counters"]["returnedBytes"]
    require(previous == len(rows), "unassigned journal rows")
    return groups, {"entries": len(rows), "requestedBytes": requested, "returnedBytes": returned, "failedZeroReturnReads": failed, "cumulativeSampleBoundaries": [s["counters"]["readAttempts"] for s in doc["samples"]], "totalsReconcile": True, "readSpanCoverageVerified": True, "largeReadBodiesPresentInJournal": False, "limitation": "body576/vectorElements rows record address/size/result, usually not body bytes; parsed node values and elements hashes are report fields, not independently re-decoded journal bytes"}


def covered(rows, field, address, length):
    address = int(address, 16) if isinstance(address, str) else address
    return any(r["field"] == field and r["status"] == "read" and int(r["address"], 16) <= address and address + length <= int(r["address"], 16) + r["actualBytes"] for r in rows)


def topology(trace):
    return {"nodes": [{k: n[k] for k in ("address", "vtable", "parent136")} for n in trace["nodes"]], "edges": trace["edges"], "templateField": trace.get("templateField")}


def validate_trace(trace, kind, rows, prefix, positive):
    control = trace["control"]
    require(covered(rows, prefix + "ui.body576", control, 576), "trace body read span")
    if not positive:
        require(trace["termination"] == "control_vtable_changed" and trace["firstPassPath"] is False and not trace["nodes"] and not trace["edges"], "old-address type guard")
        require(trace["controlVtable"] != VTABLES[kind], "old-address vtable mismatch")
        return
    nodes, edges = trace["nodes"], trace["edges"]
    require(trace["termination"] == "current_root_reached" and trace["firstPassPath"] is True, "positive path termination")
    require(trace["controlVtable"] == VTABLES[kind] and sum(n["address"] == control and n["vtable"] == VTABLES[kind] for n in nodes) == 1, "positive control type")
    require(nodes[0]["address"] == (control if kind == "hud" else trace["templateField"]["pointer"]), "control/template path origin")
    require((nodes[-1]["address"], nodes[-1]["vtable"], nodes[-1]["parent136"]) == (UI_ROOT, ROOT_VTABLE, "0x0"), "current root end node")
    require(trace["hudAncestorSeen"] is True and trace["controlSeen"] is True and any(n["vtable"] == VTABLES["hud"] for n in nodes), "HUD ancestry")
    require(len(edges) == len(nodes) - 1, "complete selected path")
    for node in nodes:
        require(node["bit20Set"] is bool(int(node["flags192"], 16) & 0x20), "flags 0x20 decoding")
        require(covered(rows, prefix + "ui.body576", node["address"], 576), "node body read span")
    require(trace["allChainBit20Set"] is all(n["bit20Set"] for n in nodes), "chain bit20")
    for i, edge in enumerate(edges):
        child, parent, witness = nodes[i], nodes[i + 1], edge["witness"]
        require((edge["child"], edge["parent"], edge["childParent136"], child["parent136"]) == (child["address"], parent["address"], parent["address"], parent["address"]), "bidirectional edge endpoints")
        require(edge["downwardMembership"] == "listed", "selected edge membership")
        require(int.from_bytes(bytes.fromhex(witness["pointerRawHex"]), "little") == int(child["address"], 16) and len(witness["pointerRawHex"]) == 16, "witness child pointer")
        if "inlineOffset" in witness:
            offset = witness["inlineOffset"]
            require(144 <= offset <= 568 and offset % 8 == 0, "inline witness offset")
            require(covered(rows, prefix + "ui.body576", int(parent["address"], 16) + offset, 8), "inline witness read span")
        else:
            vector, index = witness["vector"], witness["index"]
            require(0 <= index < vector["count124"] <= 512 and len(vector["elementsSha256"]) == 64, "vector witness bounds")
            require(covered(rows, prefix + "ui.vectorHeader112", int(vector["address"], 16) + 112, 16), "vector header read span")
            require(covered(rows, prefix + "ui.vectorElements", vector["data112"], vector["count124"] * 8), "vector elements read span")
    if kind == "minimap":
        require(trace["templateField"]["offset"] == 472 and trace["templateParentMatchesControl"] is True, "minimap template relation")
    if kind == "tactical":
        require(trace["templateField"]["offset"] == 528, "tactical template field")
    counts = (len(nodes), len(edges), sum("inlineOffset" in e["witness"] for e in edges), sum("vector" in e["witness"] for e in edges))
    require(counts == SHAPES[kind], "selected path shape")


def validate_reference_binding(doc, reference):
    ref = doc["reference"]
    require(ref["sha256"] == ref["expectedSha256"] == REF_SHA and ref["bytes"] == 5031347, "genuine reference bytes")
    require(same_path(ref["path"], ROOT / REFERENCE), "reference path")
    require(ref["parser"] == "strict-genuine-v3-reference-v4" and ref["schema"] == reference["schema"], "strict v3 reference parser")
    require(ref["hintsOnly"] is True and ref["reportedScriptShaIsAttestation"] is False and ref["currentModuleMatch"] is True and ref["referenceRootIsCurrentRootRequirement"] is False, "reference hint boundaries")
    require((ref["pid"], ref["expectedStartFiletime"], ref["reportedScriptSha256"]) == (PID, START, PROBE_PINS["v3/phase_probe_v3.py"]), "reference identity")
    require(ref["module"] == {"base": MODULE_BASE, "sha256": MODULE_SHA, "imageSize": MODULE_SIZE}, "reference module equality")
    require(ref["hints"] == OLD and ref["candidateCounts"] == {k: 1 for k in OLD}, "reference addresses")
    require([s["scheduledOffsetSeconds"] for s in ref["samples"]] == [0, 1, 3] and all(s["validatedProofKinds"] == ["hud", "minimap", "tactical"] for s in ref["samples"]), "all reference samples validated")
    for stored, original in zip(ref["samples"], reference["samples"]):
        require(stored["root"] == original["hud"]["root"] and stored["traversal"] == original["hud"]["traversal"] and stored["currentTreeCoverage"] == original["hud"]["currentTreeCoverage"], "reference historical metadata")


def summarize(doc, expected_controls, has_old):
    groups, journal = audit_journal(doc)
    samples, positive_traces, old_reads, node_entries, edge_entries, inline, vector = [], 0, [], 0, 0, 0, 0
    for sample, rows in zip(doc["samples"], groups):
        hud, summaries = sample["hud"], []
        for identity in (sample["identityBefore"], sample["identityAfter"]):
            require((identity["pid"], identity["startFiletime"], identity["stillActive"]) == (PID, START, True), "sample process identity")
        require(sample["sameProcessIdentity"] is True, "sample identity recheck")
        require(hud["root"] == UI_ROOT and hud["rootVtable"] == ROOT_VTABLE and hud["verification"]["rootStable"] is True, "observed/rechecked UI root")
        require(hud["currentTreeCoverage"] == "unknown" and hud["traversal"]["knownGraphComplete"] is False and hud["traversal"]["fullProcessSearch"] is False, "unknown traversal coverage")
        require(hud["phaseDecision"] == "not_evaluated" and all(hud[k] is False for k in ("completeLayoutValidationPerformed", "csharpTryReadExecuted", "wholeTreeStabilityChecked", "coverageIsPhaseEvidence")), "HUD nonclaims")
        require(hud["currentTreeMatches"] == expected_controls, "current tree matches")
        expected_pairs = {(k, v) for k, v in expected_controls.items()} | ({(k, v) for k, v in OLD.items()} if has_old else set())
        proofs = hud["proofs"]
        require(len(proofs) == len(expected_pairs) and {(p["kind"], p["address"]) for p in proofs} == expected_pairs, "proof set/uniqueness")
        for proof in proofs:
            kind, address = proof["kind"], proof["address"]
            positive = address == expected_controls.get(kind)
            require(proof["origins"] == (["current_root_walk"] if positive else ["reference_address"]), "proof origin")
            for pass_name, prefix in (("initial", ""), ("recheck", "verify.")):
                trace = proof[pass_name]
                require(trace["control"] == address, "proof address consistency")
                validate_trace(trace, kind, rows, prefix, positive)
                if positive:
                    positive_traces += 1
                    node_entries += len(trace["nodes"])
                    edge_entries += len(trace["edges"])
                    inline += sum("inlineOffset" in e["witness"] for e in trace["edges"])
                    vector += sum("vector" in e["witness"] for e in trace["edges"])
                else:
                    matching = [r for r in rows if r["field"] == prefix + "ui.body576" and r["address"] == address and r["length"] == r.get("actualBytes") == 576 and r["status"] == "read"]
                    require(len(matching) == 1, "one old-control body read per pass/sample")
                    old_reads.append({"sample": sample["scheduledOffsetSeconds"], "kind": kind, "pass": pass_name, "ordinal": matching[0]["ordinal"], "address": address, "bytes": 576, "reportedHeadQword": trace["controlVtable"]})
            first, repeated = proof["initial"], proof["recheck"]
            require(proof["structureStable"] is True and topology(first) == topology(repeated), "two-pass selected structure")
            changes = [{"address": n["address"], "before": n["flags192"], "after": r["flags192"], "xor": hex(int(n["flags192"], 16) ^ int(r["flags192"], 16)), "mask0x20Changed": bool((int(n["flags192"], 16) ^ int(r["flags192"], 16)) & 0x20)} for n, r in zip(first["nodes"], repeated["nodes"]) if n["flags192"] != r["flags192"]]
            require(proof["flags192Stable"] is (not changes), "reported flag stability")
            require(proof["currentRootReachability"] == ("bidirectional_path_observed" if positive else "unknown"), "reachability scope")
            summaries.append({"kind": kind, "address": address, "origins": proof["origins"], "currentRootReachability": proof["currentRootReachability"], "termination": first["termination"], "firstPassPath": first["firstPassPath"], "structureStable": True, "flags192Stable": proof["flags192Stable"], "emptyTraceEqualityOnly": not positive, "nodeCount": len(first["nodes"]), "edgeCount": len(first["edges"]), "topologySha256": digest(encoded(topology(first))), "controlFlagsInitial": next(n["flags192"] for n in first["nodes"] if n["address"] == address) if positive else None, "controlFlagsRecheck": next(n["flags192"] for n in repeated["nodes"] if n["address"] == address) if positive else None, "allChainBit20Initial": first.get("allChainBit20Set"), "allChainBit20Recheck": repeated.get("allChainBit20Set"), "templateField": first.get("templateField"), "flagsChanges": changes})
        world = {k: v for k, v in sample["world"].items() if k not in ("status", "startedUtc", "finishedUtc")}
        require(world["uniqueObservedLocalPlayer"] is True and world["matchKeyCandidateIsProductSnapshot"] is False, "world observation boundary")
        require(sample["camera"]["finite"] is True and sample["camera"]["sourceMatrixChecks"] is True, "camera observation checks")
        ui_rows = [r for r in rows if r["field"].startswith("ui.")]
        hud_cap = doc["caps"]["perSamplePartitions"]["hud"]
        require(len(ui_rows) <= hud_cap["readAttempts"] and sum(r["length"] for r in ui_rows) <= hud_cap["requestedBytes"], "HUD read partition")
        stability = sample["stability"]
        require(stability["checked"] == stability["checkpointCount"] and not stability["failures"] and stability["stableWorldRootsAfterRead"] is True, "selected checkpoint recheck")
        samples.append({"scheduledOffsetSeconds": sample["scheduledOffsetSeconds"], "startedUtc": sample["startedUtc"], "finishedUtc": sample["finishedUtc"], "world": world, "cameraCore": sample["camera"]["core"], "controller": sample["camera"]["controller"], "root": hud["root"], "rootVtable": hud["rootVtable"], "currentTreeCoverage": "unknown", "currentTreeMatches": hud["currentTreeMatches"], "traversal": hud["traversal"], "anomalyCounts": dict(Counter(a.get("reason", a.get("error", {}).get("type", "unknown")) for a in hud["anomalies"])), "verificationAnomalyCount": len(hud["verification"]["anomalies"]), "proofs": summaries, "journal": {"firstOrdinal": rows[0]["ordinal"], "lastOrdinal": rows[-1]["ordinal"], "readAttempts": len(rows), "requestedBytes": sum(r["length"] for r in rows), "returnedBytes": sum(r.get("actualBytes", 0) for r in rows), "hudReadAttempts": len(ui_rows), "hudRequestedBytes": sum(r["length"] for r in ui_rows)}, "stability": stability})
    require(len(old_reads) == (18 if has_old else 0), "18 old-control reads")
    journal.update({"positiveTracePasses": positive_traces, "positiveNodeEntries": node_entries, "positiveEdges": edge_entries, "inlineWitnesses": inline, "vectorWitnesses": vector, "oldBody576ReadCount": len(old_reads), "oldBodyReads": old_reads})
    return {"operatorLabel": doc["operatorLabel"], "operatorLabelIsObservedPhase": False, "phaseDecision": "not_evaluated", "productAcceptance": False, "status": "observed", "handleClosed": True, "sourceBindingVerified": True, "startedUtc": doc["startedUtc"], "finishedUtc": doc["finishedUtc"], "counters": doc["counters"], "deadlineOverrunSeconds": doc["deadlineOverrunSeconds"], "caps": doc["caps"], "journalAudit": journal, "samples": samples}


def capture_record(number):
    stem, meta_sha, image_sha = CAPTURES[number]
    meta_raw, meta_file = checked_file(LIVE + stem + ".json", meta_sha)
    image_raw, image_file = checked_file(LIVE + stem + ".png", image_sha)
    meta = json.loads(meta_raw.decode("utf-8-sig"))
    require(meta["ProcessId"] == PID and meta["WindowHandle"] == 1379546 and meta["ScreenshotSucceeded"] is True and meta["IsVisible"] is True, "selected-window capture identity/result")
    require((meta["Width"], meta["Height"]) == (2560, 1440) and same_path(meta["ScreenshotPath"], ROOT / image_file["path"]), "capture dimensions/path")
    require(meta["Privacy"].endswith("local selected-window capture."), "capture type provenance")
    require(image_raw[:8] == b"\x89PNG\r\n\x1a\n" and image_raw[12:16] == b"IHDR" and (int.from_bytes(image_raw[16:20], "big"), int.from_bytes(image_raw[20:24], "big")) == (2560, 1440), "PNG header dimensions")
    return {"sidecar": meta_file, "image": image_file, "capturedAt": meta["CapturedAt"], "processId": meta["ProcessId"], "windowHandle": meta["WindowHandle"], "captureType": "selected-window", "compositedDesktop": False, "externalOverlayCompositionProven": False}


def positive_hud(sample):
    return next(p["initial"] for p in sample["hud"]["proofs"] if p["kind"] == "hud" and p["initial"]["firstPassPath"])


def build_index():
    probes = [checked_file(PHASE + path, sha)[1] for path, sha in PROBE_PINS.items()]
    ref_raw, ref_file = checked_file(REFERENCE, REF_SHA, 5031347)
    reference = json.loads(ref_raw)
    validate_common(reference, "v3")
    ref_summary = summarize(reference, OLD, False)
    observations, raw_docs, common_attestation = [], {}, None
    for ident, suffix, size, sha, capture_ids, role, ground_truth in SPECS:
        raw, raw_file = checked_file(PHASE + "v4/live-round3/pid30504-" + suffix + ".json", sha, size)
        doc = json.loads(raw)
        validate_common(doc, "v4")
        validate_reference_binding(doc, reference)
        if common_attestation is None:
            common_attestation = doc["executionAttestation"]
        require(doc["executionAttestation"] == common_attestation and doc["sourceManifest"] == reference["sourceManifest"] and doc["module"] == reference["module"] and doc["roles"] == reference["roles"], "cross-report historical provenance")
        controls = NEW if ident.startswith("second-") else {}
        summary = summarize(doc, controls, True)
        before, after = [capture_record(n) for n in capture_ids]
        require(utc(before["capturedAt"]) <= utc(doc["startedUtc"]) <= utc(doc["finishedUtc"]) <= utc(after["capturedAt"]), "before/probe/after timestamp order")
        for sample in doc["samples"]:
            require(utc(doc["startedUtc"]) <= utc(sample["startedUtc"]) <= utc(sample["finishedUtc"]) <= utc(doc["finishedUtc"]), "invocation sample time order")
        observations.append({"id": ident, "role": role, "raw": raw_file, "screenshots": {"before": before, "after": after, "beforeToAfterSeconds": round((utc(after["capturedAt"]) - utc(before["capturedAt"])).total_seconds(), 6), "beforeToInvocationSeconds": round((utc(doc["startedUtc"]) - utc(before["capturedAt"])).total_seconds(), 6), "invocationFinishToAfterSeconds": round((utc(after["capturedAt"]) - utc(doc["finishedUtc"])).total_seconds(), 6)}, "groundTruth": {"source": "lead read_image attestation supplied to phase-probe-current", "contentRevalidatedByGenerator": False, "observation": ground_truth}, **summary})
        raw_docs[ident] = doc
    indexed = {o["id"]: o for o in observations}
    baseline_world = ref_summary["samples"][0]["world"]
    require(all(s["world"] == baseline_world for s in ref_summary["samples"]), "first-match reference world stability")
    reused = ["results", "second-running001", "second-running002", "second-paused001"]
    require(all(s["world"] == baseline_world for ident in reused for s in indexed[ident]["samples"]), "cross-match world/local/key reuse")
    old_hud = positive_hud(reference["samples"][0])
    old_ancestors = topology(old_hud)["nodes"][1:]
    for ident in ("second-running001", "second-running002", "second-paused001"):
        for sample in raw_docs[ident]["samples"]:
            current = positive_hud(sample)
            require(current["control"] != old_hud["control"] and topology(current)["nodes"][1:] == old_ancestors and len(old_ancestors) == 11, "changed HUD / reused 11 ancestors")
            for proof in sample["hud"]["proofs"]:
                if proof["initial"]["firstPassPath"]:
                    baseline = next(p for p in raw_docs["second-running002"]["samples"][0]["hud"]["proofs"] if p["kind"] == proof["kind"] and p["initial"]["firstPassPath"])
                    require(topology(proof["initial"]) == topology(baseline["initial"]), "second-match selected topology reuse")
    current_hud = positive_hud(raw_docs["second-running002"]["samples"][0])
    for edge in (old_hud["edges"][0], current_hud["edges"][0]):
        require(edge["parent"] == "0x1d2a061eb10" and edge["witness"]["inlineOffset"] == 200, "shared parent+200 witness")
    for ident in ("second-running001", "second-running002", "second-paused001"):
        for s in indexed[ident]["samples"]:
            for p in s["proofs"]:
                if p["firstPassPath"]:
                    require(p["allChainBit20Initial"] is (p["kind"] != "tactical") and p["allChainBit20Recheck"] is (p["kind"] != "tactical"), "observed HUD/minimap bit20 pattern")
                    require(all(c["mask0x20Changed"] is False for c in p["flagsChanges"]), "observed non-0x20 flag changes")
    script_raw = Path(__file__).read_bytes()
    return {"schema": "coh3-round3-v4-historical-observation-index-v1", "generator": {"path": str(Path(__file__).resolve().relative_to(ROOT)).replace("\\", "/"), "sha256": digest(script_raw), "bytes": len(script_raw), "mode": "offline fixed-input reconstruction", "frozenProbesImportedOrExecuted": False, "processOrGuiAccess": False, "currentProductionSourceReadOrRepinned": False, "hashesAreSignatures": False}, "identity": {"pid": PID, "expectedStartFiletime": START, "expectedStartUtc": START_UTC}, "module": reference["module"], "historicalSourceManifest": reference["sourceManifest"], "executionAttestation": common_attestation, "frozenProbeFiles": probes, "genuineReference": {"raw": ref_file, "hintsOnly": True, "historicalRootNotRequiredToEqualCurrentRoot": True, "summary": ref_summary}, "observations": observations, "crossMatch": {"comparedTo": REFERENCE, "exactWorldLocalAndMatchKeyReuse": True, "reusingObservations": reused, "world": baseline_world, "uiRootAddressReused": UI_ROOT, "oldHud": OLD["hud"], "newHud": NEW["hud"], "sameHudAddressReuseObserved": False, "hudAncestorsReused": old_ancestors, "reusedAncestorCount": 11, "oldHudFirstEdge": old_hud["edges"][0], "newHudFirstEdge": current_hud["edges"][0], "sharedWitnessSlot": "0x1d2a061ebd8", "inference": "world/local/key/root equality alone does not identify a new match; independent lifecycle invalidation remains a product design requirement, not implemented by this index"}, "boundaries": {"phaseDecision": "not_evaluated", "productAcceptance": False, "productSnapshotCalled": False, "productBuildCorrespondenceEstablished": False, "productGuiAcceptance": "not_run", "currentTreeCoverage": "unknown", "wholeTreeAbsenceProven": False, "oldObjectLifetimeOrDestructorProven": False, "oldParentOrFlagsDecodedAfterTypeMismatch": False, "rawFlagsStabilityIsPausePredicate": False, "externalOverlayCompositionProven": False}}


def render_findings(index):
    lines = [
        "**Round3 v4：历史原始观测索引与复核结论**", "",
        "主记录为 results、mainmenu、紧同步 second-running002、second-paused001；second-running001 只作先行补充。索引完整固定每个 raw、真实 v3 paused 引用、冻结 v1–v4、截图 PNG/sidecar 与本生成器的 SHA256。哈希用于内容绑定，不是签名。", "",
        "PID `30504`，启动时间 `2026-11-22T01:23:35.3993114Z` / FILETIME `134397842153993114`；模块 SHA256 `" + MODULE_SHA + "`。历史 Reader SHA256 `" + HISTORICAL_SOURCES["client/CoH3MemoryTool/PlayerInfoReader.cs"][0] + "`。当前生产源码的变化不改写这些采样时 sourceManifest；生成器不读取当前 C#、不执行或导入冻结 probe。", "",
        "| 记录 | 类型 | reads | 请求/返回 bytes | 秒 | 截图前后秒 | raw SHA256 |", "|---|---|---:|---:|---:|---:|---|",
    ]
    for o in index["observations"]:
        c = o["counters"]
        link = "../../lifecycle-phase/v4/live-round3/" + Path(o["raw"]["path"]).name
        lines.append(f"| [{o['id']}](<{link}>) | {o['role']} | {c['readAttempts']} | {c['requestedBytes']}/{c['returnedBytes']} | {c['elapsedSeconds']:.3f} | {o['screenshots']['beforeToAfterSeconds']:.6f} | `{o['raw']['sha256']}` |")
    lines += ["", "所有 raw 保留 `phaseDecision=not_evaluated`、`productAcceptance=false`、`currentTreeCoverage=unknown`，句柄已关闭、身份/模块与历史 source binding 一致；各次均未超过 20000 reads / 2 MiB requested / 10 秒，总 deadline overrun 为 0。journal 连号、逐项读长/返回、累计与每样本账目、initial/recheck 分区读 span 均重算。大块 body/vector 日志通常不含原始字节，因此 parsed 节点值和 elements hash 不是从 journal 独立重解码所得。", "", "| 记录 | before（sidecar 原时区） | raw UTC 起止 | after（sidecar 原时区） | 根代理既有读图结论 |", "|---|---|---|---|---|"]
    for o in index["observations"]:
        b, a = o["screenshots"]["before"], o["screenshots"]["after"]
        bn, an = Path(b["image"]["path"]).name, Path(a["image"]["path"]).name
        lines.append(f"| {o['id']} | [{bn}](<{bn}>) {b['capturedAt']} | {o['startedUtc']} → {o['finishedUtc']} | [{an}](<{an}>) {a['capturedAt']} | {o['groundTruth']['observation']} |")
    lines += ["", "截图为 selected-window，非桌面合成图；内容结论来源是 lead 已完成的 read_image 检查，本生成器只校验文件身份、PNG/sidecar 尺寸和 before ≤ probe ≤ after 顺序。它们不构成产品覆盖层合成、产品真实 GUI 或产品 G 验收。", "", "跨局对比真实 [v3 paused 引用](<../../lifecycle-phase/v3/live-round3/pid30504-paused-001.json>)：第一局 paused、results、第二局 running/paused 的 world/local/player entries 与 matchKey 完全复用，arena `0x1d01d600000`、world `0x1d023a00000`、vector `0x1d01d61c600`、array `0x1d01ea15140`、local `0x1d026209000`，key `30504:639309074153993114:1993462513664:1993504493568:1001`。UI root 始终 `0x1d2a08a61c0`；这些字段相等不足以定义新局 epoch。主菜单则是 1 玩家/local `0x1d026207000`/lowIdentity1000。", "", "第二局当前 HUD `0x1d098515200`、minimap `0x1d3b7dad490`、tactical `0x1d2ae303150`：0/1/3 秒、initial/recheck 均有当前根双向路径，按 HUD/minimap/tactical 顺序分别 12/26/22 nodes、11/25/21 edges。每份第二局 raw 审核 18 positive traces / 360 node entries / 342 edges（228 inline + 114 vector）。HUD 换址，11 个祖先 address/vtable/parent136 完全复用；共同父 `0x1d2a061eb10` 的 `+200` 槽 `0x1d2a061ebd8` 从 `70c55eccd3010000` 变为 `00525198d0010000`。同一 HUD 地址跨局重用尚未实测。", "", "每个 v4 raw 都对第一局旧 HUD/minimap/tactical 做了 18 次完整 576-byte body 读取（3 controls × 2 passes × 3 samples），均在 `control_vtable_changed` 早停：空 nodes/edges、firstPassPath=false、reachability=unknown。其 structureStable/flags192Stable=true 只是空列表比较；未解码旧 parent/flags，不推断销毁、脱链、隐藏或对象不存在。results/mainmenu 没有当前正路径，仍保持 unknown；queue_exhausted 不等于完整覆盖。", "", "| 第二局记录 | HUD flags192Stable(0/1/3) | minimap | tactical | 0x20 变化 |", "|---|---|---|---|---|"]
    for o in index["observations"]:
        if not o["id"].startswith("second-"):
            continue
        values = {}
        for kind in ("hud", "minimap", "tactical"):
            values[kind] = "/".join("T" if next(p for p in s["proofs"] if p["kind"] == kind and p["firstPassPath"])["flags192Stable"] else "F" for s in o["samples"])
        lines.append(f"| {o['id']} | {values['hud']} | {values['minimap']} | {values['tactical']} | 无 |")
    lines += ["", "HUD/minimap 两遍链上的 mask `0x20` 均为 true，tactical 为 false；raw flags 在 running 与 paused 都会变化。paused 3s 的 minimap control 自身也从 `0x2a01c030` → `0x2a018030`，XOR `0x4000`；其他已测变化为 `0x4000` 或 `0x40004000`，均未改变 `0x20`。flags192Stable 不是独立的暂停判据或上下文保留必要条件；完整差异节点、读 ordinal、各样本截断/异常与 checkpoints 见 [JSON 索引](<round3-v4-observation-index.json>)。", "", "重算命令（仓库根）：", "", "```powershell", "python -B -X utf8 CoH3/dev/SkyEye/tests/live-lifecycle/index_round3_v4.py --check", "```", "", "[生成器](<../../../../tests/live-lifecycle/index_round3_v4.py>)默认仅以 exclusive create 新建两份固定输出；`--check` 在内存重建并逐字节比较两份既存输出，发现 drift 即失败，全程无写。product Snapshot、build correspondence、产品真实 GUI、覆盖层合成与产品 G 验收仍为 not_run/false；本索引没有实现 phase predicate 或产品 epoch/cache 修改。", ""]
    return "\n".join(lines).encode("utf-8")


def payloads():
    index = build_index()
    findings = render_findings(index)
    index["findings"] = {"path": FINDINGS, "sha256": digest(findings), "bytes": len(findings)}
    return encoded(index), findings


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="validate existing fixed outputs without writing")
    args = parser.parse_args(argv)
    if not args.check:
        require(not (ROOT / INDEX).exists() and not (ROOT / FINDINGS).exists(), "exclusive-create outputs already exist; use --check")
    index_bytes, findings_bytes = payloads()
    if not args.check:
        for path, content in ((FINDINGS, findings_bytes), (INDEX, index_bytes)):
            with (ROOT / path).open("xb") as stream:
                stream.write(content)
    require((ROOT / INDEX).read_bytes() == index_bytes, "index reconstruction mismatch")
    require((ROOT / FINDINGS).read_bytes() == findings_bytes, "findings reconstruction mismatch")
    print(json.dumps({"status": "verified", "mode": "check-no-write" if args.check else "exclusive-create-and-check", "primaryObservations": 4, "preliminaryObservations": 1, "indexSha256": digest(index_bytes), "findingsSha256": digest(findings_bytes), "generatorSha256": digest(Path(__file__).read_bytes()), "nativeOrGuiCalls": 0}, ensure_ascii=False))


if __name__ == "__main__":
    main()
