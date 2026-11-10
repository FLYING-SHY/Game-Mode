// rea decompile_all
// name: FUN_14374c0b0
// address: 0x14374c0b0
// body: available
// range: 0x14374c0b0-0x14374c17c inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 FUN_14374c0b0(undefined8 param_1,longlong param_2)

{
  uint uVar1;
  longlong lVar2;
  char cVar3;
  longlong lVar4;
  longlong lVar5;
  longlong unaff_GS_OFFSET;
  
  if ((*(int *)(param_2 + 0x88) == 0) || (cVar3 = func_0x00014374af68(), cVar3 == '\0')) {
    return 0;
  }
  lVar2 = *(longlong *)
           (&DAT_146185e00 +
           (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8);
  uVar1 = *(uint *)((ulonglong)DAT_145e0becc + 0x104 + lVar2);
  lVar4 = func_0x0001405daad0(**(undefined8 **)(DAT_146185f18 + 0xa8),
                              *(undefined8 *)(param_2 + 0x88));
  if (lVar4 != 0) {
    for (lVar4 = *(longlong *)(lVar4 + 0x10); lVar4 != 0; lVar4 = *(longlong *)(lVar4 + 0x10)) {
      if (((*(int *)(lVar4 + 0xf0) != 0) &&
          (lVar5 = func_0x00014038f9e0((ulonglong)uVar1 + lVar2), lVar5 != 0)) &&
         (cVar3 = func_0x00014374af68(param_1,lVar5), cVar3 == '\0')) {
        return 0;
      }
    }
  }
  return 1;
}

