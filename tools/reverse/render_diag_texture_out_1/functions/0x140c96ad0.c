// rea decompile_all
// name: FUN_140c96ad0
// address: 0x140c96ad0
// body: available
// range: 0x140c96ad0-0x140c96cd7 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_140c96ad0(undefined8 param_1,longlong param_2,longlong param_3,undefined8 param_4)

{
  undefined8 *puVar1;
  undefined1 auStack_108 [32];
  short local_e8;
  short local_e0;
  undefined8 local_d8;
  undefined8 local_c8;
  undefined8 uStack_c0;
  ushort local_b8;
  ushort local_b6;
  undefined8 local_b0;
  undefined8 uStack_a8;
  undefined8 local_a0;
  undefined8 uStack_98;
  undefined1 local_90 [32];
  ulonglong local_70;
  
  local_70 = DAT_145df0fc0 ^ (ulonglong)auStack_108;
  func_0x0001402dfb90(param_3,&local_b8);
  local_c8._0_4_ = (float)*(undefined8 *)(param_2 + 8);
  local_c8._4_4_ = (float)((ulonglong)*(undefined8 *)(param_2 + 8) >> 0x20);
  local_c8 = CONCAT44(((float)*(ushort *)(param_3 + 0x10) * local_c8._4_4_) /
                      *(float *)(*(longlong *)(DAT_146185f18 + 0x100) + 0xa0),
                      ((float)*(ushort *)(param_3 + 8) * (float)local_c8) /
                      *(float *)(*(longlong *)(DAT_146185f18 + 0x100) + 0x9c));
  uStack_a8 = local_c8;
  local_a0 = *(undefined8 *)(param_2 + 0x10);
  uStack_98 = *(undefined8 *)(param_2 + 0x18);
  local_b0 = CONCAT44((float)local_b6,(float)local_b8);
  func_0x000140284310(local_90,8,4,0x1401aa1e0);
  func_0x0001403532d0(&local_b0,local_90);
  puVar1 = (undefined8 *)func_0x000142dafcf0(&local_c8,0x1447333b0);
  local_b0 = *puVar1;
  uStack_a8 = puVar1[1];
  puVar1 = (undefined8 *)func_0x000142dafcf0(&local_c8,0x1447333b0);
  local_e0 = *(short *)(param_3 + 0x10) + -1;
  local_c8 = *puVar1;
  uStack_c0 = puVar1[1];
  local_e8 = *(short *)(param_3 + 8) + -1;
  local_d8 = param_4;
  FUN_140c96ce0();
  func_0x0001422ba6e0(local_70 ^ (ulonglong)auStack_108);
  return;
}

