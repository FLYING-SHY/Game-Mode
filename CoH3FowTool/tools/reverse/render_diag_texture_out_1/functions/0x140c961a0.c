// rea decompile_all
// name: FUN_140c961a0
// address: 0x140c961a0
// body: available
// range: 0x140c961a0-0x140c962c1 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_140c961a0(longlong param_1,longlong param_2)

{
  longlong lVar1;
  longlong lVar2;
  code *pcVar3;
  longlong lVar4;
  undefined4 *puVar5;
  undefined1 auStack_58 [40];
  undefined4 local_30;
  undefined4 uStack_2c;
  undefined4 *puStack_28;
  undefined8 local_20;
  ulonglong local_18;
  
  local_18 = DAT_145df0fc0 ^ (ulonglong)auStack_58;
  func_0x000140384630(param_2,0);
  lVar1 = *(longlong *)(param_2 + 0x10);
  lVar2 = **(longlong **)(param_2 + 0x20);
  pcVar3 = *(code **)(param_1 + 0x18);
  if (pcVar3 == (code *)&LAB_140c962d0) {
    FUN_140c962e0(0x146a887a8,0);
  }
  else if (pcVar3 == FUN_140c96840) {
    lVar4 = func_0x000140609190(DAT_146185f18);
    if (lVar4 != 0) {
      *(undefined1 *)(lVar4 + 0x5c) = 0;
    }
  }
  else if (pcVar3 == FUN_140c96950) {
    lVar4 = func_0x0001403b2dc0();
    if (*(char *)(lVar4 + 0x20) == '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    puVar5 = (undefined4 *)
             func_0x0001403b2e30(*(undefined8 *)
                                  (lVar4 + 0x28 + (ulonglong)*(uint *)(lVar4 + 0x24) * 8),0x20);
    local_30 = 0x393a85b8;
    puStack_28 = puVar5 + 6;
    local_20 = 0x14105a1e0;
    *puVar5 = 0x393a85b8;
    puVar5[1] = uStack_2c;
    *(undefined4 **)(puVar5 + 2) = puStack_28;
    *(undefined8 *)(puVar5 + 4) = 0x14105a1e0;
  }
  else {
    (*pcVar3)();
  }
  if ((int)((lVar1 - lVar2) + -0x10 >> 4) ==
      (int)((*(longlong *)(param_2 + 0x10) - **(longlong **)(param_2 + 0x20)) + -0x10 >> 4)) {
    func_0x0001422ba6e0(local_18 ^ (ulonglong)auStack_58);
    return;
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

