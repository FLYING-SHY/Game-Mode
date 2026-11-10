// rea decompile_all
// name: FUN_1414c5480
// address: 0x1414c5480
// body: available
// range: 0x1414c5480-0x1414c558c inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1414c5480(longlong *param_1)

{
  longlong *plVar1;
  longlong lVar2;
  char cVar3;
  longlong *local_18 [2];
  
  if (*(int *)(param_1[0x46] + 8) != 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  if (*(longlong *)(*param_1 + 0x508) != 0x140690280) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  local_18[0] = (longlong *)0x0;
  func_0x0001402429c0(*(undefined8 *)(_DAT_146a8ca18 + 0x148),local_18);
  plVar1 = local_18[0];
  if (local_18[0] != (longlong *)0x0) {
    if (*(longlong *)(*local_18[0] + 0x30) != 0x1414e8fd0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    cVar3 = func_0x0001402a5460(0x146a90af0,0x146a90af0);
    if (cVar3 != '\0') {
      if (plVar1 != (longlong *)0x0) {
        cVar3 = func_0x0001406903f0(plVar1);
        if (cVar3 != '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
          halt_baddata();
        }
      }
      func_0x000140690590(param_1,1);
      if (plVar1 != (longlong *)0x0) {
        cVar3 = func_0x00014068ff90(plVar1);
        if (cVar3 != '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
          halt_baddata();
        }
      }
      goto LAB_1414c5554;
    }
  }
  func_0x000140690590(param_1,1);
LAB_1414c5554:
  if (plVar1 != (longlong *)0x0) {
    LOCK();
    plVar1 = plVar1 + 2;
    lVar2 = *plVar1;
    *(int *)plVar1 = (int)*plVar1 + -1;
    UNLOCK();
    if ((int)lVar2 == 1) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
  }
  return;
}

