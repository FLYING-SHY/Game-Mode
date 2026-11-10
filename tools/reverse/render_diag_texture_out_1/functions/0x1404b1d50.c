// rea decompile_all
// name: entry
// address: 0x1404b1d50
// body: available
// range: 0x1404b1d50-0x1404b2074 inclusive
// range: 0x142759b22-0x142759b57 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 * entry(undefined8 *param_1,undefined8 param_2)

{
  undefined8 uVar1;
  longlong lVar2;
  undefined8 *puVar3;
  longlong local_28;
  undefined8 local_20;
  undefined8 *local_18;
  
  *param_1 = 0;
  param_1[1] = 0;
  param_1[2] = 0;
  param_1[3] = 0;
  param_1[4] = 0;
  param_1[5] = 0;
  param_1[6] = 0;
  param_1[7] = 0;
  param_1[8] = 0;
  param_1[9] = 0;
  param_1[10] = 0;
  param_1[0xb] = 0;
  param_1[0xc] = 0;
  param_1[0xd] = 0;
  param_1[0xe] = 0;
  param_1[0xf] = 0;
  param_1[0x10] = 0;
  param_1[0x11] = 0;
  param_1[0x12] = 0;
  param_1[0x13] = 0;
  param_1[0x14] = 0;
  param_1[0x15] = 0;
  param_1[0x16] = 0;
  param_1[0x17] = 0;
  param_1[0x18] = 0;
  *(undefined4 *)(param_1 + 0x19) = 0x40400000;
  *(undefined4 *)((longlong)param_1 + 0xcc) = 0x3f800000;
  *(undefined4 *)(param_1 + 0x1a) = 0x44000000;
  *(undefined4 *)((longlong)param_1 + 0xd4) = 0x44000000;
  *(undefined4 *)(param_1 + 0x1b) = 0x43430000;
  *(undefined4 *)((longlong)param_1 + 0xdc) = 0x43430000;
  *(undefined1 *)(param_1 + 0x1c) = 0;
  param_1[0x1d] = 0;
  param_1[0x1e] = 0;
  param_1[0x1f] = 0;
  param_1[0x20] = 0;
  param_1[0x21] = 0;
  param_1[0x22] = 0;
  param_1[0x23] = 0;
  param_1[0x24] = 0;
  param_1[0x25] = 0xffffffffffffffff;
  param_1[0x26] = 0;
  param_1[0x27] = 0;
  local_18 = param_1;
  FUN_1404b1390(param_1 + 0x28);
  param_1[0x40] = 0;
  *(undefined1 *)(param_1 + 0x41) = 0;
  *(undefined8 *)((longlong)param_1 + 0x20c) = 0;
  param_1[0x43] = 0;
  param_1[0x44] = param_2;
  func_0x0001402bb1e0(param_1 + 0x45);
  FUN_1404b2230(param_1 + 0xb1);
  local_20 = param_1[0x44];
  local_28 = param_1[0xb1];
  FUN_1404b2080(param_1 + 0xb2,&local_28,&local_20);
  local_28 = param_1[0x44];
  local_20 = param_1[0xb1];
  FUN_1404b2080(param_1 + 0xb3,&local_20,&local_28);
  param_1[0xb4] = 0;
  param_1[0xb5] = 0;
  param_1[0xb6] = 0;
  param_1[0xb7] = 0;
  param_1[0xb8] = 0;
  param_1[0xb9] = 0;
  param_1[0xba] = 0;
  *(undefined1 *)(param_1 + 0xbb) = 1;
  *(undefined4 *)((longlong)param_1 + 0x5dc) = 0;
  *(undefined2 *)(param_1 + 0xbc) = 0;
  *(undefined1 *)((longlong)param_1 + 0x5e2) = 0;
  *(undefined2 *)(param_1 + 0xbd) = 1;
  param_1[0xbf] = 0;
  param_1[0xc0] = 0;
  param_1[0xc1] = 0;
  param_1[0xc2] = 0;
  param_1[0xc3] = 0;
  param_1[0xbe] = &DAT_143d7fc10;
  param_1[0xc4] = &DAT_143d7fba8;
  *(undefined1 *)(param_1 + 0xc5) = 0;
  *(undefined1 *)(param_1 + 200) = 0;
  (*(code *)0x5bf31c6)(param_1 + 199);
  (*(code *)0x5bf31da)(param_1 + 0xc6);
  func_0x0001412f5e90(param_1 + 7);
  puVar3 = (undefined8 *)FUN_1404b2280(&local_28);
  uVar1 = *puVar3;
  *puVar3 = 0;
  lVar2 = param_1[0xb4];
  param_1[0xb4] = uVar1;
  if (lVar2 != 0) {
    func_0x0001414136b0(lVar2);
    func_0x0001422ba700(lVar2,0x38);
  }
  lVar2 = local_28;
  if (local_28 != 0) {
    func_0x0001414136b0(local_28);
    func_0x0001422ba700(lVar2,0x38);
  }
  puVar3 = (undefined8 *)FUN_1404b22e0(&local_28);
  uVar1 = *puVar3;
  *puVar3 = 0;
  lVar2 = param_1[0x40];
  param_1[0x40] = uVar1;
  if (lVar2 != 0) {
    func_0x000140278b70();
  }
  if (local_28 != 0) {
    func_0x000140278b70();
  }
  return param_1;
}

