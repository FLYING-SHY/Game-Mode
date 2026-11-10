// rea decompile_all
// name: FUN_140c96e60
// address: 0x140c96e60
// body: available
// range: 0x140c96e60-0x140c96eba inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_140c96e60(undefined8 param_1,longlong *param_2)

{
  int *piVar1;
  int iVar2;
  longlong lVar3;
  longlong *plVar4;
  undefined8 uStack_28;
  undefined8 *local_20;
  longlong alStack_18 [2];
  
  if (*param_2 == 0) {
    local_20 = (undefined8 *)func_0x0001422ba9a8();
    *local_20 = 0;
    local_20[1] = 0;
    local_20[2] = 0;
    local_20[3] = 0;
    local_20[4] = 0;
    local_20[5] = 0;
    local_20[6] = 0;
    local_20[7] = 0;
    local_20[8] = 0;
    local_20 = (undefined8 *)func_0x000140c97330(local_20);
    uStack_28 = param_1;
    plVar4 = (longlong *)FUN_140c96f30(alStack_18,&uStack_28,&local_20);
    lVar3 = *plVar4;
    if (lVar3 != 0) {
      LOCK();
      *(int *)(lVar3 + 0x10) = *(int *)(lVar3 + 0x10) + 1;
      UNLOCK();
    }
    if (*param_2 != 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    *param_2 = lVar3;
    if (alStack_18[0] != 0) {
      LOCK();
      piVar1 = (int *)(alStack_18[0] + 0x10);
      iVar2 = *piVar1;
      *piVar1 = *piVar1 + -1;
      UNLOCK();
      if (iVar2 == 1) {
        func_0x000140452990();
      }
    }
    if (local_20 != (undefined8 *)0x0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
  }
  return;
}

