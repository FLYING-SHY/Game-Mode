// rea decompile_all
// name: FUN_1407fced0
// address: 0x1407fced0
// body: available
// range: 0x1407fced0-0x1407fcf62 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


longlong * FUN_1407fced0(uint *param_1,longlong *param_2,longlong *param_3,longlong *param_4)

{
  uint uVar1;
  longlong lVar2;
  longlong lVar3;
  longlong lVar4;
  longlong lVar5;
  longlong unaff_GS_OFFSET;
  
  lVar2 = *param_3;
  lVar3 = *param_4;
  if (lVar2 != lVar3) {
    uVar1 = *param_1;
    lVar4 = *(longlong *)
             (&DAT_146185e00 +
             (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8);
    lVar5 = ((ulonglong)param_1[1] - lVar3) + lVar4 + (ulonglong)uVar1;
    func_0x0001422bddaf(lVar2,lVar3,lVar5);
    param_1[1] = ((int)lVar2 - (int)(lVar4 + (ulonglong)uVar1)) + (int)lVar5;
  }
  *param_2 = *param_3;
  return param_2;
}

