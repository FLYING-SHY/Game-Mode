// rea decompile_all
// name: FUN_1404b2a60
// address: 0x1404b2a60
// body: available
// range: 0x1404b2a60-0x1404b2dcd inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_1404b2a60(longlong *param_1)

{
  longlong lVar1;
  undefined8 *puVar2;
  
  lVar1 = param_1[0x9a];
  if (lVar1 != 0) {
    func_0x00014031b310(lVar1,param_1[0x9c] - lVar1 & 0xfffffffffffffff0);
    param_1[0x9a] = 0;
    param_1[0x9b] = 0;
    param_1[0x9c] = 0;
  }
  lVar1 = param_1[0x97];
  if (lVar1 != 0) {
    func_0x00014031b310(lVar1,param_1[0x99] - lVar1 & 0xfffffffffffffff0);
    param_1[0x97] = 0;
    param_1[0x98] = 0;
    param_1[0x99] = 0;
  }
  if (param_1[0x94] != 0) {
    func_0x000140337720(param_1[0x94],param_1[0x95]);
    func_0x00014031b310(param_1[0x94],param_1[0x96] - param_1[0x94] & 0xfffffffffffffff8);
    param_1[0x94] = 0;
    param_1[0x95] = 0;
    param_1[0x96] = 0;
  }
  func_0x000140246dc0(param_1 + 0x90);
  param_1[0x21] = (longlong)&DAT_143d76538;
  if (param_1[0x20] != 0) {
    func_0x0001404b81f0(param_1[0x20],1);
  }
  if (param_1[0x1d] != 0) {
    func_0x00014039c210(param_1[0x1d],param_1[0x1e]);
    func_0x00014031b310(param_1[0x1d],param_1[0x1f] - param_1[0x1d] & 0xfffffffffffffff8);
    param_1[0x1d] = 0;
    param_1[0x1e] = 0;
    param_1[0x1f] = 0;
  }
  func_0x000140246dc0(param_1 + 0x19);
  lVar1 = param_1[0x18];
  if (lVar1 != 0) {
    func_0x000140c210c0(lVar1);
    func_0x0001422ba700(lVar1,0x100);
  }
  if (param_1[0x17] != 0) {
    FUN_1404b2dd0();
  }
  lVar1 = param_1[0x16];
  if (lVar1 != 0) {
    func_0x00014027ac50(lVar1);
    func_0x0001422ba700(lVar1,0x2430);
  }
  lVar1 = param_1[0x15];
  if (lVar1 != 0) {
    func_0x0001413057e0(lVar1);
    func_0x0001422ba700(lVar1,0x38);
  }
  if (param_1[0x14] != 0) {
    func_0x000140278a00();
  }
  if (param_1[0x13] != 0) {
    func_0x00014149a4e0();
  }
  if (param_1[0x12] != 0) {
    FUN_1404b2e20(param_1[0x12],1);
  }
  if (param_1[0x11] != 0) {
    FUN_1404b2e90(param_1[0x11],1);
  }
  if (param_1[0x10] != 0) {
    func_0x0001404b3460(param_1[0x10],1);
  }
  lVar1 = param_1[0xf];
  if (lVar1 != 0) {
    func_0x0001413b2a00(lVar1);
    func_0x0001422ba700(lVar1,0x78);
  }
  puVar2 = (undefined8 *)param_1[0xe];
  if (puVar2 != (undefined8 *)0x0) {
    (**(code **)*puVar2)(puVar2,1);
  }
  lVar1 = param_1[0xd];
  if (lVar1 != 0) {
    func_0x00014111bf50(lVar1);
    func_0x0001422ba700(lVar1,0xb8);
  }
  lVar1 = param_1[0xc];
  if (lVar1 != 0) {
    func_0x0001413aca80(lVar1);
    func_0x0001422ba700(lVar1,0x90);
  }
  lVar1 = param_1[0xb];
  if (lVar1 != 0) {
    func_0x0001404b6f90(lVar1);
    func_0x0001422ba700(lVar1,0x90);
  }
  lVar1 = param_1[10];
  if (lVar1 != 0) {
    func_0x0001402796e0(lVar1);
    func_0x0001422ba700(lVar1,0x648);
  }
  puVar2 = (undefined8 *)param_1[9];
  if (puVar2 != (undefined8 *)0x0) {
    (**(code **)*puVar2)(puVar2,1);
  }
  lVar1 = param_1[8];
  if (lVar1 != 0) {
    func_0x000140c21050(lVar1);
    func_0x0001422ba700(lVar1,0xa8);
  }
  lVar1 = param_1[7];
  if (lVar1 != 0) {
    func_0x000140c20ed0(lVar1);
    func_0x0001422ba700(lVar1,0xd8);
  }
  lVar1 = param_1[3];
  if (lVar1 != 0) {
    func_0x00014031b310(lVar1,param_1[5] - lVar1 & 0xfffffffffffffff0);
    param_1[3] = 0;
    param_1[4] = 0;
    param_1[5] = 0;
  }
  lVar1 = *param_1;
  if (lVar1 != 0) {
    func_0x00014031b310(lVar1,param_1[2] - lVar1 & 0xfffffffffffffff0);
    *param_1 = 0;
    param_1[1] = 0;
    param_1[2] = 0;
  }
  return;
}

