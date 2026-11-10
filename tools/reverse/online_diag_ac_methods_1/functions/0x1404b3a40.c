// rea decompile_all
// name: FUN_1404b3a40
// address: 0x1404b3a40
// body: available
// range: 0x1404b3a40-0x1404b3ac8 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 * FUN_1404b3a40(undefined8 *param_1,longlong param_2,undefined8 param_3)

{
  undefined8 *puVar1;
  
  puVar1 = param_1;
  func_0x0001404b4360(param_1,param_3);
  *puVar1 = &DAT_143d7d320;
  puVar1[0x12] = param_2;
  puVar1[0x13] = 0;
  *(undefined4 *)(puVar1 + 0x14) = 0;
  puVar1[0x15] = 0;
  puVar1[0x16] = 0;
  puVar1[0x17] = 0;
  puVar1[0x18] = 0;
  if (*(longlong *)(param_2 + 0x58) != 0) {
    func_0x0001404b43c0(*(longlong *)(*(longlong *)(*(longlong *)(param_2 + 0x58) + 0x20) + 0xd8) +
                        0xa10,param_1,0x14067e7e0);
  }
  return param_1;
}

