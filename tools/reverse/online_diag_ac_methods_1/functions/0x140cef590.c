// rea decompile_all
// name: FUN_140cef590
// address: 0x140cef590
// body: available
// range: 0x140cef590-0x140cef626 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 * FUN_140cef590(undefined8 *param_1)

{
  undefined1 local_88 [56];
  undefined8 local_50;
  undefined8 local_10;
  
  local_50 = 0;
  local_10 = 0;
  FUN_140cef630();
  func_0x0001401f5ea0(local_88);
  *param_1 = &DAT_143df1f80;
  param_1[6] = &DAT_143df1d40;
  param_1[7] = &DAT_143df1ce8;
  param_1[8] = &DAT_143df1d00;
  param_1[9] = &DAT_143df1d20;
  *(undefined1 *)(param_1 + 0x31) = 0;
  *(undefined8 *)((longlong)param_1 + 0x18c) = 0;
  return param_1;
}

