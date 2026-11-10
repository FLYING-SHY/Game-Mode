#!/usr/bin/env python3
"""Transform saved REA JSON into an independent, readable evidence artifact."""
import json
import struct
import sys
from pathlib import Path
root = Path(__file__).resolve().parent
source = root / (sys.argv[1] if len(sys.argv) > 1 else 'console_diag_tickgate_rea.json')
obj = json.loads(source.read_text(encoding='utf-8-sig'))
print('ROOT_KEYS', list(obj))

def normalized(value):
    if isinstance(value, dict):
        if 'normalized_result' in value:
            return value['normalized_result']
        for nested in value.values():
            found = normalized(nested)
            if found is not None:
                return found
    elif isinstance(value, list):
        for nested in value:
            found = normalized(nested)
            if found is not None:
                return found
    return None

value = normalized(obj)
print('NORMALIZED_TYPE', type(value).__name__)
if value is not None:
    dst = root / (source.name.removesuffix('_rea.json') + '_decompiled.c')
    dst.write_text(value if isinstance(value, str) else json.dumps(value, indent=2, ensure_ascii=False), encoding='utf-8')
    print('EXPORTED', dst)
site, target = 0xC964EB, 0x6A888A0
patch = b'\x8a\x05' + struct.pack('<i', target-(site+6)) + b'\x90'*6
print('SITE_RVA', hex(site), 'PATCH', patch.hex(' '))
