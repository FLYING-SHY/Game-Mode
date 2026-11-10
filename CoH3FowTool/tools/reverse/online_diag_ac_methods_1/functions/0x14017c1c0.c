// rea decompile_all
// name: FUN_14017c1c0
// address: 0x14017c1c0
// body: available
// range: 0x14017c1c0-0x14017c23e inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 *
FUN_14017c1c0(undefined8 *param_1,longlong *param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 uVar1;
  undefined1 local_res10 [8];
  
  param_1[1] = 0;
  *param_1 = 0x143d6dd00;
  param_1[2] = param_3;
  param_1[3] = param_4;
  param_1[4] = 0;
  param_2 = (longlong *)*param_2;
  if (param_2 != (longlong *)0x0) {
    if (*(code **)(*param_2 + 8) == FUN_14017c070) {
      uVar1 = FUN_14017c070();
    }
    else {
      uVar1 = (**(code **)(*param_2 + 8))(param_2,local_res10);
    }
    FUN_14017ceb0(param_1 + 4,uVar1);
    func_0x00014017d1b0(local_res10);
  }
  return param_1;
}

