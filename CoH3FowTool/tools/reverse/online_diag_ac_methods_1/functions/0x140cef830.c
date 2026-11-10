// rea decompile_all
// name: FUN_140cef830
// address: 0x140cef830
// body: available
// range: 0x140cef830-0x140cef8bf inclusive
// range: 0x1429256b8-0x1429256d5 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_140cef830(undefined8 *param_1)

{
  int *piVar1;
  int iVar2;
  longlong lVar3;
  uint uVar4;
  uint uVar5;
  ulonglong uVar6;
  
  *param_1 = &PTR_FUN_143df1e90;
  uVar6 = 0;
  uVar4 = 0;
  if (*(int *)(param_1 + 6) != 0) {
    do {
      lVar3 = *(longlong *)(param_1[5] + uVar6 * 8);
      if (lVar3 != 0) {
        LOCK();
        piVar1 = (int *)(lVar3 + 0x10);
        iVar2 = *piVar1;
        *piVar1 = *piVar1 + -1;
        UNLOCK();
        if (iVar2 == 1) {
          func_0x000140452990();
        }
      }
      uVar5 = (int)uVar6 + 1;
      uVar6 = (ulonglong)uVar5;
      uVar4 = *(uint *)(param_1 + 6);
    } while (uVar5 < uVar4);
  }
  if (_DAT_146c2a498 == (code *)0x0) {
    (*(code *)0x5bf8d44)();
  }
  else if (_DAT_146c2a498 == (code *)0x140451860) {
    func_0x000140451870(param_1[5],param_1[5],(ulonglong)uVar4 << 3);
  }
  else {
    (*_DAT_146c2a498)();
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

