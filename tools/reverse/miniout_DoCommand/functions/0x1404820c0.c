// rea decompile_all
// name: entry
// address: 0x1404820c0
// body: available
// range: 0x1404820c0-0x140482132 inclusive
// range: 0x140482140-0x1404821a2 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b537ee6fdad293e1c8ea50e7b50d0b6b22972a83032bca093d279ab7e770ce4
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void entry(undefined8 param_1,uint *param_2)

{
  char cVar1;
  ulonglong uVar2;
  char *pcVar3;
  int *piVar4;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_c8 [160];
  
  if (*param_2 == 0xffffffff) {
    return;
  }
  piVar4 = (int *)(*(longlong *)
                    ((ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8 +
                    0x146185e00) + (ulonglong)*param_2);
  if ((char)piVar4[0x18] == '\0') {
    uVar2 = 0;
    do {
      if (*(int *)(uVar2 + 0x144f1df58) == *piVar4) {
                    /* WARNING: Bad instruction - Truncating control flow here */
        halt_baddata();
      }
      uVar2 = uVar2 + 4;
    } while (uVar2 < 0x30);
  }
  pcVar3 = (char *)(_DAT_145df0fc0 ^ (ulonglong)auStack_c8);
  cVar1 = (char)pcVar3;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
  *pcVar3 = *pcVar3 + cVar1;
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

