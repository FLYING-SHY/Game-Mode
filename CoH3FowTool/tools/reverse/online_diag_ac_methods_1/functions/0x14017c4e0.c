// rea decompile_all
// name: FUN_14017c4e0
// address: 0x14017c4e0
// body: available
// range: 0x14017c4e0-0x14017c595 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14017c4e0(longlong *param_1,undefined8 *param_2)

{
  undefined8 uVar1;
  undefined1 auStack_a8 [32];
  undefined8 local_88;
  undefined8 uStack_80;
  undefined4 local_78;
  undefined4 uStack_74;
  undefined4 uStack_70;
  undefined4 uStack_6c;
  undefined8 local_68;
  undefined8 uStack_60;
  undefined8 local_58;
  undefined8 uStack_50;
  undefined8 local_48;
  undefined8 uStack_40;
  undefined8 local_38;
  undefined8 uStack_30;
  undefined8 local_28;
  undefined8 uStack_20;
  ulonglong local_18;
  
  local_18 = DAT_145df0fc0 ^ (ulonglong)auStack_a8;
  if ((int)param_1[3] != 0) {
    local_78 = *(undefined4 *)(param_2 + 2);
    uStack_74 = *(undefined4 *)((longlong)param_2 + 0x14);
    uStack_70 = *(undefined4 *)(param_2 + 3);
    uStack_6c = *(undefined4 *)((longlong)param_2 + 0x1c);
    uStack_60 = 0;
    local_28 = 0;
    uStack_20 = 2;
    local_58 = 0;
    uStack_50 = 0;
    local_48 = 0;
    uStack_40 = 0;
    local_38 = 0;
    uStack_30 = 0;
    local_88 = *param_2;
    uStack_80 = param_2[1];
    local_68 = param_2[4];
    if (*(code **)(*param_1 + 0x28) == (code *)0x140ca3380) {
      uVar1 = func_0x0001403b2dc0();
      func_0x000140ca33c0(uVar1,param_1 + 3,&local_88);
    }
    else {
      (**(code **)(*param_1 + 0x28))((int)local_68,&local_88);
    }
  }
  func_0x0001422ba6e0(local_18 ^ (ulonglong)auStack_a8);
  return;
}

