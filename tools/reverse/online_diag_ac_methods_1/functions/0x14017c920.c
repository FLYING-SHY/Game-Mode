// rea decompile_all
// name: FUN_14017c920
// address: 0x14017c920
// body: available
// range: 0x14017c920-0x14017c9b0 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_14017c920(undefined8 param_1,longlong param_2,ulonglong param_3)

{
  ulonglong uVar1;
  ulonglong *puVar2;
  ulonglong *puVar3;
  ulonglong *puVar4;
  ulonglong uVar5;
  ulonglong *puVar6;
  
  FUN_14017c9c0(param_3,0);
  if (*(longlong *)(param_2 + 0x10) != 0x14082cc80) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  puVar3 = (ulonglong *)0x0;
  puVar2 = *(ulonglong **)(param_2 + 0x20);
  uVar5 = *(longlong *)(param_2 + 0x60) + 0xefU & ~(*(longlong *)(param_2 + 0x60) - 1U);
  puVar4 = puVar3;
  puVar6 = puVar3;
  if ((ulonglong *)(ulonglong)*(uint *)(param_2 + 0x2c) != (ulonglong *)0x0) {
    do {
      uVar1 = *puVar2;
      puVar4 = puVar3;
      if ((uVar1 == 0) ||
         ((uVar1 <= param_3 && (puVar4 = puVar2, param_3 < *(uint *)(param_2 + 0x28) + uVar1))))
      break;
      puVar6 = (ulonglong *)((longlong)puVar6 + 1);
      puVar2 = puVar2 + 2;
      puVar4 = (ulonglong *)0x0;
    } while (puVar6 < (ulonglong *)(ulonglong)*(uint *)(param_2 + 0x2c));
  }
  *(int *)(puVar4 + 1) = (int)puVar4[1] - (int)uVar5;
  *(longlong *)(param_2 + 0x68) = *(longlong *)(param_2 + 0x68) - uVar5;
  return;
}

