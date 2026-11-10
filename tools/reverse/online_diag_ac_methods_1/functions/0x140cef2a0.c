// rea decompile_all
// name: FUN_140cef2a0
// address: 0x140cef2a0
// body: available
// range: 0x140cef2a0-0x140cef329 inclusive
// range: 0x1429255b2-0x14292561a inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 * FUN_140cef2a0(undefined8 *param_1,undefined8 *param_2,undefined8 *param_3)

{
  longlong lVar1;
  undefined8 *puVar2;
  undefined8 *local_res10;
  
  *param_1 = 0;
  param_1[1] = 0;
  *(undefined4 *)(param_1 + 1) = 1;
  *(undefined4 *)((longlong)param_1 + 0xc) = 1;
  *param_1 = &PTR_FUN_143df1be8;
  local_res10 = (undefined8 *)*param_3;
  *param_3 = 0;
  FUN_140cefb70(param_1 + 2,*param_2,&local_res10);
  puVar2 = local_res10;
  param_1[2] = &PTR_LAB_143df1c10;
  param_1[5] = &DAT_143df1bc8;
  if (local_res10 != (undefined8 *)0x0) {
    if (*(code **)*local_res10 == (code *)0x1413ee1c0) {
      lVar1 = local_res10[7];
      if (lVar1 != 0) {
        func_0x00014031b310(lVar1,local_res10[9] - lVar1 & 0xfffffffffffffff0);
        puVar2[7] = 0;
        puVar2[8] = 0;
        puVar2[9] = 0;
      }
      if (puVar2[2] != 0) {
        func_0x0001405094b0();
      }
      func_0x0001422ba700(puVar2,0x50);
    }
    else {
      (**(code **)*local_res10)(local_res10,1);
    }
  }
  return param_1;
}

