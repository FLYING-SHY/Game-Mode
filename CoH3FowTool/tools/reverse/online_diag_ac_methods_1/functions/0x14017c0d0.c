// rea decompile_all
// name: FUN_14017c0d0
// address: 0x14017c0d0
// body: available
// range: 0x14017c0d0-0x14017c1b8 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 *
FUN_14017c0d0(undefined8 *param_1,undefined8 param_2,undefined8 *param_3,undefined8 *param_4)

{
  longlong *plVar1;
  undefined8 *puVar2;
  undefined8 uVar3;
  
  uVar3 = 0;
  plVar1 = (longlong *)func_0x00014017ea40();
  if (*(code **)(*plVar1 + 0x10) == (code *)0x1401aa160) {
    puVar2 = (undefined8 *)(*_DAT_143d54500)(plVar1[7],0,0x28);
    if ((puVar2 == (undefined8 *)0x0) && ((char)plVar1[0xd] != '\0')) {
      func_0x00014247e88c();
      func_0x000142478c18(0x1447853e0,0xbd,0x144784a38,0x28);
      _DAT_00000000 = 0;
    }
  }
  else {
    puVar2 = (undefined8 *)(**(code **)(*plVar1 + 0x10))(plVar1,0x28);
  }
  if (puVar2 != (undefined8 *)0x0) {
    *puVar2 = 0;
    puVar2[1] = 0;
    puVar2[2] = 0;
    puVar2[3] = 0;
    puVar2[4] = 0;
    uVar3 = FUN_14017c1c0(puVar2,param_2,*param_3,*param_4);
  }
  *param_1 = uVar3;
  return param_1;
}

