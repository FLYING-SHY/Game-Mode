// rea decompile_all
// name: FUN_1407fc1c0
// address: 0x1407fc1c0
// body: available
// range: 0x1407fc1c0-0x1407fc2b9 inclusive
// range: 0x1407fc2c0-0x1407fc2f4 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_1407fc1c0(undefined8 param_1)

{
  undefined8 *puVar1;
  longlong *plVar2;
  longlong lVar3;
  longlong lVar4;
  char cVar5;
  longlong *plVar6;
  longlong lVar7;
  undefined1 auStack_68 [64];
  ulonglong local_28;
  
  local_28 = DAT_145df0fc0 ^ (ulonglong)auStack_68;
  puVar1 = *(undefined8 **)(DAT_146185f18 + 0xb8);
  plVar6 = (longlong *)*puVar1;
  plVar2 = (longlong *)puVar1[1];
  while ((plVar6 != plVar2 && (cVar5 = FUN_1407fc310(), cVar5 == '\0'))) {
    plVar6 = plVar6 + 1;
  }
  if ((plVar6 != (longlong *)puVar1[1]) && (lVar3 = *plVar6, lVar3 != 0)) {
    lVar4 = *(longlong *)(lVar3 + 0x18);
    for (lVar7 = *(longlong *)(lVar3 + 0x10); lVar7 != lVar4; lVar7 = lVar7 + 0x18) {
      func_0x0001407fe920(param_1,lVar7);
    }
    if (*(longlong *)(lVar3 + 0x10) != *(longlong *)(lVar3 + 0x18)) {
      func_0x0001411afd30();
      *(undefined8 *)(lVar3 + 0x18) = *(undefined8 *)(lVar3 + 0x10);
    }
    if (*(longlong *)(lVar3 + 0x528) != *(longlong *)(lVar3 + 0x530)) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    if (*(longlong *)(lVar3 + 0x528) != *(longlong *)(lVar3 + 0x530)) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    if (*(longlong *)(lVar3 + 0x540) != *(longlong *)(lVar3 + 0x548)) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    if (*(longlong *)(lVar3 + 0x540) != *(longlong *)(lVar3 + 0x548)) {
      *(longlong *)(lVar3 + 0x548) = *(longlong *)(lVar3 + 0x540);
    }
  }
  func_0x0001422ba6e0(local_28 ^ (ulonglong)auStack_68);
  return;
}

