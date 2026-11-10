"""Summarize a REA analyze JSON report (evidence bundle).

Usage: python rea_json_summary.py <report.json>
"""
import json, sys, collections


def walk(node, path="$", depth=0, out=None):
    if out is None:
        out = []
    if depth > 6:
        return out
    if isinstance(node, dict):
        for k, v in node.items():
            walk(v, f"{path}.{k}", depth + 1, out)
    elif isinstance(node, list):
        out.append((path, f"list[{len(node)}]"))
        for i, v in enumerate(node[:3]):
            walk(v, f"{path}[{i}]", depth + 1, out)
    else:
        s = str(node)
        if len(s) > 160:
            s = s[:160] + "..."
        out.append((path, s))
    return out


def load_report(path):
    """The report file may be prefixed with Ghidra stdout noise; find the JSON envelope."""
    with open(path, "r", encoding="utf-8", errors="replace") as f:
        text = f.read()
    try:
        return json.loads(text), "whole-file"
    except Exception:
        pass
    # try every line that starts with '{' from the end backwards
    lines = text.splitlines()
    for i in range(len(lines) - 1, -1, -1):
        s = lines[i].strip()
        if not s.startswith("{"):
            continue
        try:
            return json.loads(s), f"line {i + 1}"
        except Exception:
            continue
    # maybe it's pretty-printed: from the first '{' to the end
    k = text.find("{")
    if k >= 0:
        try:
            return json.loads(text[k:]), "tail-from-first-brace"
        except Exception as e:
            raise SystemExit(f"no JSON envelope found: {e}")
    raise SystemExit("no JSON envelope found")


def main():
    path = sys.argv[1]
    rep, where = load_report(path)
    print("json envelope at:", where)

    print("top-level keys:", list(rep.keys()))
    for key in ("evidence_id", "provider", "operation", "predicate_type", "confidence", "authority"):
        if key in rep:
            print(f"  {key}: {str(rep[key])[:200]}")

    raw = rep.get("raw_result")
    norm = rep.get("normalized_result")
    print("raw_result keys:", list(raw.keys()) if isinstance(raw, dict) else type(raw).__name__)
    print("normalized_result keys:", list(norm.keys()) if isinstance(norm, dict) else type(norm).__name__)

    # count functions / strings / symbols if present
    def count(d, names):
        res = {}
        if isinstance(d, dict):
            for n in names:
                if n in d:
                    v = d[n]
                    res[n] = len(v) if isinstance(v, (list, dict)) else v
        return res

    for label, blob in (("raw", raw), ("norm", norm)):
        c = count(blob, ["functions", "symbols", "strings", "imports", "exports", "sections", "entries", "results"])
        if c:
            print(f"{label}:", c)

    # dump the shape of the deepest interesting branches
    interesting = []
    for path_key, val in walk(rep):
        if path_key.count(".") <= 3 and isinstance(val, str) and len(val) > 40:
            interesting.append((path_key, val))
    print("\n--- sample leaves ---")
    for p, v in interesting[:40]:
        print(f"  {p} = {v}")


if __name__ == "__main__":
    main()
