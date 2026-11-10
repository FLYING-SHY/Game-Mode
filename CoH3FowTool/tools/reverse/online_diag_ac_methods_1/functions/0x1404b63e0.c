// rea decompile_all
// name: FUN_1404b63e0
// address: 0x1404b63e0
// body: available
// range: 0x1404b63e0-0x1404b646d inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


longlong FUN_1404b63e0(longlong param_1,uint param_2)

{
  longlong lVar1;
  
  if (*(longlong *)(param_1 + 0x40) != 0) {
    func_0x000140f25930(*(longlong *)(param_1 + 0x40),*(undefined8 *)(param_1 + 0x48));
    lVar1 = *(longlong *)(param_1 + 0x50) - *(longlong *)(param_1 + 0x40);
    lVar1 = SUB168(SEXT816(-0x7777777777777777) * SEXT816(lVar1),8) + lVar1;
    func_0x00014031b310(*(longlong *)(param_1 + 0x40),((lVar1 >> 7) - (lVar1 >> 0x3f)) * 0xf0);
    *(undefined8 *)(param_1 + 0x40) = 0;
    *(undefined8 *)(param_1 + 0x48) = 0;
    *(undefined8 *)(param_1 + 0x50) = 0;
  }
  func_0x000140915810(param_1 + 0x10);
  if ((param_2 & 1) != 0) {
    func_0x0001422ba700(param_1,0x58);
  }
  return param_1;
}

