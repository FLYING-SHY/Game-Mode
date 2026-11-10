// rea decompile_all
// name: FUN_140c966a0
// address: 0x140c966a0
// body: available
// range: 0x140c966a0-0x140c96759 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_140c966a0(longlong param_1,undefined4 *param_2)

{
  longlong lVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  undefined4 uVar4;
  undefined4 *puVar5;
  undefined8 *puVar6;
  
  if (DAT_146186188 != '\0') {
    (*(code *)0x5bf3a60)(param_1 + 0x10);
    lVar1 = *(longlong *)(param_1 + 0x20 + (ulonglong)(*(uint *)(param_1 + 0x18) & 1) * 8);
    (*(code *)0x5bf315a)(lVar1);
    puVar5 = (undefined4 *)func_0x0001402ac5e0(lVar1 + 8,0x10);
    puVar6 = *(undefined8 **)(lVar1 + 0x68);
    if (puVar6 == *(undefined8 **)(lVar1 + 0x70)) {
      puVar6 = (undefined8 *)func_0x000140b74ae0(lVar1 + 0x60);
    }
    else {
      *puVar6 = 0;
      puVar6[1] = 0;
      puVar6 = *(undefined8 **)(lVar1 + 0x68);
      *(undefined8 **)(lVar1 + 0x68) = puVar6 + 2;
    }
    *puVar6 = &LAB_141045700;
    puVar6[1] = puVar5;
    (*(code *)0x5bf3174)(lVar1);
    uVar2 = param_2[1];
    uVar3 = param_2[2];
    uVar4 = param_2[3];
    *puVar5 = *param_2;
    puVar5[1] = uVar2;
    puVar5[2] = uVar3;
    puVar5[3] = uVar4;
    (*(code *)0x5bf3a78)(param_1 + 0x10);
    return;
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

