import sys, re, numpy as np
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parent))
import p2_hash_attack as HA

def to_mat(lst):
    L = max(len(s) for s in lst) + 1
    m = np.zeros((len(lst), L), dtype=np.uint8)
    for i, s in enumerate(lst):
        m[i, :len(s)] = np.frombuffer(s, dtype=np.uint8)
    return m

names = [b"HealthComponent", b"TeamId", b"Health", b"ClientPlayerManager"]
m = to_mat(names)
std = HA.crc32_table(0xEDB88320, True)
cast = HA.crc32_table(0x82F63B78, True)
koop = HA.crc32_table(0x04C11DB7, False)

fams = {
 "crc32_refl": lambda m: HA.crc32_vec(m, std, 0xFFFFFFFF, 0xFFFFFFFF, True),
 "crc32_refl_init0": lambda m: HA.crc32_vec(m, std, 0, 0, True),
 "crc32c": lambda m: HA.crc32_vec(m, cast, 0xFFFFFFFF, 0xFFFFFFFF, True),
 "fnv1a32": HA.fnv1a32_vec,
 "fnv1a64lo": lambda m: (HA.fnv1a64_vec(m) & np.uint64(0xFFFFFFFF)).astype(np.uint32),
 "djb2": lambda m: HA.simple_poly(m, 33, 5381),
 "sdbm": lambda m: HA.simple_poly(m, 65599, 0),
 "elf": lambda m: HA.simple_poly(m, 16, 0, 0),
 "poly31_7": lambda m: HA.simple_poly(m, 31, 0, 7),
 "jenkins": HA.jenkins_oat,
 "murmur3": HA.murmur3_32_vec,
}
for k, f in fams.items():
    h = f(m)
    print(f"{k:20s}", [f"0x{int(v):08X}" for v in h])
