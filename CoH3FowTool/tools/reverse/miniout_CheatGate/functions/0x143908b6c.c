// rea decompile_all
// name: entry
// address: 0x143908b6c
// body: available
// range: 0x143908b6c-0x143908bc3 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b537ee6fdad293e1c8ea50e7b50d0b6b22972a83032bca093d279ab7e770ce4
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

bool entry(longlong param_1)

{
  longlong lVar1;
  longlong unaff_GS_OFFSET;
  bool bVar2;
  
  lVar1 = *(longlong *)
           ((ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8 + 0x146185e00);
  lVar1 = func_0x00014018c920((ulonglong)*(uint *)((ulonglong)_DAT_145e0becc + 0xfc + lVar1) + lVar1
                              ,param_1 + 8);
  bVar2 = false;
  if (lVar1 != 0) {
    bVar2 = *(char *)(lVar1 + 0x680) == '\0';
  }
  return bVar2;
}

