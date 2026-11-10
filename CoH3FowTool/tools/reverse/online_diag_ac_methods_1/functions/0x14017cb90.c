// rea decompile_all
// name: FUN_14017cb90
// address: 0x14017cb90
// body: available
// range: 0x14017cb90-0x14017cbf5 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined8 * FUN_14017cb90(undefined8 *param_1)

{
  param_1[2] = 3;
  param_1[1] = 0;
  *param_1 = 0;
  *(undefined4 *)(param_1 + 3) = 0;
  *(undefined4 *)((longlong)param_1 + 0x1c) = 0x3f800000;
  *(undefined4 *)(param_1 + 4) = 1;
  *(undefined8 *)((longlong)param_1 + 0x24) = _DAT_14464a4d8;
  *(undefined4 *)((longlong)param_1 + 0x2c) = 0;
  param_1[6] = _DAT_146ba4420;
  param_1[7] = 0;
  param_1[8] = 0;
  param_1[9] = 0;
  *(undefined4 *)(param_1 + 10) = 0;
  *(undefined4 *)((longlong)param_1 + 0x54) = 0x3f800000;
  *(undefined4 *)(param_1 + 0xb) = 0x1000101;
  *(undefined4 *)((longlong)param_1 + 0x5c) = 0xffffffff;
  *(undefined1 *)(param_1 + 0xc) = 0;
  return param_1;
}

