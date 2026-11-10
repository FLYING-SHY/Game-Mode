// rea decompile_all
// name: FUN_1404b20f0
// address: 0x1404b20f0
// body: available
// range: 0x1404b20f0-0x1404b21a4 inclusive
// range: 0x142759b5a-0x142759b94 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 * FUN_1404b20f0(undefined8 *param_1,undefined8 param_2,undefined8 param_3)

{
  undefined8 uVar1;
  undefined8 *puVar2;
  longlong local_18;
  undefined8 *local_10;
  
  *param_1 = param_2;
  param_1[1] = param_3;
  param_1[2] = 0;
  param_1[3] = 0;
  param_1[4] = 0;
  param_1[5] = 0;
  *(undefined4 *)(param_1 + 6) = 0xbf800000;
  local_10 = param_1;
  func_0x0001403a6320(param_1 + 7);
  func_0x00014027cb60(param_1 + 0xfa);
  func_0x0001422bae68(param_1 + 0x2e3,8,0x14,0x1413ac350,0x1403eb000);
  puVar2 = (undefined8 *)FUN_1404b21b0(&local_18);
  uVar1 = *puVar2;
  *puVar2 = 0;
  puVar2 = (undefined8 *)param_1[2];
  param_1[2] = uVar1;
  if (puVar2 != (undefined8 *)0x0) {
    if (*(code **)*puVar2 == FUN_1404b1d10) {
      FUN_1404b2210();
      func_0x0001422ba700(puVar2,0x20);
    }
    else {
      (**(code **)*puVar2)(puVar2,1);
    }
  }
  if (local_18 != 0) {
    func_0x000140279b10();
  }
  return param_1;
}

