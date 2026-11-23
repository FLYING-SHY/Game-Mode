"""Read-only PE identity and bounded keyword survey for the installed CoH2 image.
This script never executes or modifies the input. Addresses in the report are static.
"""
import argparse
import datetime as dt
import hashlib
import json
from pathlib import Path
import re
import struct


def inspect(path, pattern):
    data = path.read_bytes()
    if data[:2] != b'MZ':
        raise ValueError('Not a DOS/PE image')
    pe = struct.unpack_from('<I', data, 0x3c)[0]
    if data[pe:pe + 4] != b'PE\0\0':
        raise ValueError('Invalid PE signature')
    machine, count, timestamp = struct.unpack_from('<HHI', data, pe + 4)
    optional_size = struct.unpack_from('<H', data, pe + 20)[0]
    optional = pe + 24
    magic = struct.unpack_from('<H', data, optional)[0]
    is64 = magic == 0x20b
    if magic not in (0x10b, 0x20b):
        raise ValueError('Unknown optional header')
    image_base = struct.unpack_from('<Q' if is64 else '<I', data, optional + (24 if is64 else 28))[0]
    size_of_image = struct.unpack_from('<I', data, optional + 56)[0]
    directory = optional + (112 if is64 else 96)
    clr_rva, clr_size = struct.unpack_from('<II', data, directory + 14 * 8)
    sections = []
    for index in range(count):
        off = optional + optional_size + index * 40
        name, virtual_size, rva, raw_size, raw_offset = struct.unpack_from('<8sIIII', data, off)
        characteristics = struct.unpack_from('<I', data, off + 36)[0]
        sections.append(dict(name=name.rstrip(b'\0').decode('ascii'), rva=rva, virtualSize=virtual_size,
                             rawOffset=raw_offset, rawSize=raw_size, characteristics=hex(characteristics)))
    def file_to_rva(offset):
        for section in sections:
            if section['rawOffset'] <= offset < section['rawOffset'] + section['rawSize']:
                return section['rva'] + offset - section['rawOffset']
        return None
    keywords = re.compile(pattern, re.I)
    hits = []
    for match in re.finditer(rb'[\x20-\x7e]{5,512}\x00', data):
        text = match.group()[:-1].decode('ascii')
        if keywords.search(text):
            rva = file_to_rva(match.start())
            hits.append(dict(text=text, fileOffset=hex(match.start()), rva=hex(rva) if rva is not None else None,
                             va=hex(image_base + rva) if rva is not None else None))
    by_rva = {int(hit['rva'], 16): hit for hit in hits if hit['rva']}
    for section in sections:
        if int(section['characteristics'], 16) & 0x20000000 == 0:
            continue
        code = data[section['rawOffset']:section['rawOffset'] + section['rawSize']]
        # Candidate x64 RIP-relative LEA and MOV references, then verify in the static engine.
        for match in re.finditer(rb'[\x48\x4c][\x8d\x8b][\x05\x0d\x15\x1d\x25\x2d\x35\x3d](....)', code, re.S):
            instruction_rva = section['rva'] + match.start()
            target_rva = instruction_rva + 7 + struct.unpack('<i', match.group(1))[0]
            if target_rva in by_rva:
                by_rva[target_rva].setdefault('candidateCodeRefs', []).append(hex(image_base + instruction_rva))
    return dict(schema='coh2-image-survey-v1', capturedUtc=dt.datetime.now(dt.timezone.utc).isoformat(),
                input=str(path.resolve()), bytes=len(data), sha256=hashlib.sha256(data).hexdigest(),
                machine=hex(machine), bits=64 if is64 else 32, clrDirectory=dict(rva=hex(clr_rva), bytes=clr_size),
                peTimestamp=hex(timestamp), imageBase=hex(image_base), sizeOfImage=hex(size_of_image),
                sections=sections, keywordPattern=pattern, matchingStrings=hits)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('input', type=Path)
    parser.add_argument('--output', required=True, type=Path)
    parser.add_argument('--pattern', default=r'(FOW|FogOfWar|fog_|reveal|setsimrate|gamespeed|cheat|Player_SetResource|Player_AddResource|populationcap)')
    args = parser.parse_args()
    report = inspect(args.input, args.pattern)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(sha256=report['sha256'], bits=report['bits'], sections=len(report['sections']),
                          matchingStrings=len(report['matchingStrings']), output=str(args.output)), ensure_ascii=False))


if __name__ == '__main__':
    main()
