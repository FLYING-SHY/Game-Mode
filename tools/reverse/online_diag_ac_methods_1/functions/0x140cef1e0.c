// rea decompile_all
// name: FUN_140cef1e0
// address: 0x140cef1e0
// body: available
// range: 0x140cef1e0-0x140cef294 inclusive
// range: 0x1429255a4-0x1429255b1 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


longlong * FUN_140cef1e0(longlong *param_1,undefined8 param_2,undefined8 param_3)

{
  longlong lVar1;
  longlong lVar2;
  undefined8 uVar3;
  longlong lVar4;
  longlong lVar5;
  longlong lVar6;
  
  uVar3 = func_0x0001422ba9a8(0x90);
  lVar4 = FUN_140cef2a0(uVar3,param_2,param_3);
  *param_1 = 0;
  param_1[1] = 0;
  lVar1 = lVar4 + 0x10;
  *param_1 = lVar1;
  param_1[1] = lVar4;
  if ((lVar1 != 0) &&
     ((*(longlong *)(lVar4 + 0x20) == 0 || (*(int *)(*(longlong *)(lVar4 + 0x20) + 8) == 0)))) {
    if (lVar4 != 0) {
      LOCK();
      *(int *)(lVar4 + 8) = *(int *)(lVar4 + 8) + 1;
      UNLOCK();
    }
    lVar2 = param_1[1];
    lVar5 = 0;
    lVar6 = 0;
    if (lVar2 != 0) {
      LOCK();
      *(int *)(lVar2 + 0xc) = *(int *)(lVar2 + 0xc) + 1;
      UNLOCK();
      lVar5 = lVar2;
      lVar6 = lVar1;
    }
    *(longlong *)(lVar4 + 0x18) = lVar6;
    lVar1 = *(longlong *)(lVar4 + 0x20);
    *(longlong *)(lVar4 + 0x20) = lVar5;
    if (lVar1 != 0) {
      func_0x0001405094b0(lVar1);
    }
    if (lVar2 != 0) {
      func_0x0001403a9cb0(lVar2);
    }
  }
  return param_1;
}

