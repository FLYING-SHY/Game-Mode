// rea decompile_all
// name: FUN_1410458e0
// address: 0x1410458e0
// body: available
// range: 0x1410458e0-0x141045984 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_1410458e0(longlong param_1,undefined8 param_2)

{
  int iVar1;
  undefined8 uVar2;
  undefined8 *puVar3;
  int iVar4;
  longlong *plVar5;
  undefined1 local_28 [16];
  
  plVar5 = (longlong *)(param_1 + 0x20);
  func_0x00014031fbd0(param_2,0x143e3b4f0,&DAT_143d76528,plVar5);
  func_0x00014031eee0(param_2,0x143e3b500,1,param_1 + 0x40);
  uVar2 = func_0x00014030c030();
  if (0xf < *(ulonglong *)(param_1 + 0x38)) {
    plVar5 = (longlong *)*plVar5;
  }
  puVar3 = (undefined8 *)func_0x00014030c0a0(uVar2,local_28,plVar5);
  iVar1 = *(int *)(param_1 + 0x40) + -1;
  *(undefined8 *)(param_1 + 0x18) = *puVar3;
  iVar4 = 1;
  if ((iVar1 < 1) || (iVar4 = iVar1, iVar1 < 5)) {
    *(int *)(param_1 + 0x40) = iVar4;
  }
  else {
    *(undefined4 *)(param_1 + 0x40) = 4;
  }
  return;
}

