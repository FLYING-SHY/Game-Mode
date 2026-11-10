// rea decompile_all
// name: FUN_1407fc080
// address: 0x1407fc080
// body: available
// range: 0x1407fc080-0x1407fc141 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

longlong * FUN_1407fc080(longlong *param_1,longlong param_2,longlong param_3,int *param_4)

{
  int iVar1;
  ulonglong uVar2;
  longlong lVar3;
  ulonglong uVar4;
  ulonglong uVar5;
  longlong lVar6;
  
  uVar5 = param_3 - param_2 >> 4;
  lVar3 = param_2;
  if (0 < (longlong)uVar5) {
    iVar1 = *param_4;
    lVar6 = param_2;
    do {
      uVar4 = uVar5 >> 1;
      lVar3 = uVar4 * 0x10;
      if (*(int *)(lVar3 + lVar6) < iVar1) {
        uVar4 = uVar5 + (-1 - uVar4);
        param_2 = lVar3 + lVar6 + 0x10;
      }
      else {
        param_2 = lVar6;
        if (*(int *)(lVar3 + lVar6) <= iVar1) {
          uVar4 = lVar3 >> 4;
          while (uVar2 = uVar4, 0 < (longlong)uVar2) {
            uVar4 = uVar2 >> 1;
            if (*(int *)(param_2 + uVar4 * 0x10) < iVar1) {
              param_2 = param_2 + uVar4 * 0x10 + 0x10;
              uVar4 = uVar2 + (-1 - uVar4);
            }
          }
          lVar3 = lVar3 + lVar6 + 0x10;
          if (0 < (longlong)((uVar5 * 0x10 - lVar3) + lVar6) >> 4) {
                    /* WARNING: Bad instruction - Truncating control flow here */
            halt_baddata();
          }
          break;
        }
      }
      uVar5 = uVar4;
      lVar3 = param_2;
      lVar6 = param_2;
    } while (0 < (longlong)uVar4);
  }
  *param_1 = param_2;
  param_1[1] = lVar3;
  return param_1;
}

