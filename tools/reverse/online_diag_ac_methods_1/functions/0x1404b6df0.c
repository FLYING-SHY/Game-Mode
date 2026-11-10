// rea decompile_all
// name: FUN_1404b6df0
// address: 0x1404b6df0
// body: available
// range: 0x1404b6df0-0x1404b6efc inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_1404b6df0(longlong param_1)

{
  longlong lVar1;
  
  if (*(char *)(param_1 + 0x148) != '\0') {
    FUN_1404b6f10(param_1 + 0x120,0);
  }
  if (*(char *)(param_1 + 0x118) != '\0') {
    FUN_1404b6f50(param_1 + 0xa8,0);
  }
  if (*(longlong *)(param_1 + 0x88) != 0) {
    func_0x000140fdd030(*(longlong *)(param_1 + 0x88),*(undefined8 *)(param_1 + 0x90));
    func_0x00014031b310(*(longlong *)(param_1 + 0x88),
                        ((*(longlong *)(param_1 + 0x98) - *(longlong *)(param_1 + 0x88)) / 0x18) *
                        0x18);
    *(undefined8 *)(param_1 + 0x88) = 0;
    *(undefined8 *)(param_1 + 0x90) = 0;
    *(undefined8 *)(param_1 + 0x98) = 0;
  }
  func_0x0001422bac50(param_1 + 0x28,0x18,4,0x1411bc7a0);
  lVar1 = *(longlong *)(param_1 + 0x10);
  if (lVar1 != 0) {
    func_0x00014031b310(lVar1,((*(longlong *)(param_1 + 0x20) - lVar1) / 0x18) * 0x18);
    *(undefined8 *)(param_1 + 0x10) = 0;
    *(undefined8 *)(param_1 + 0x18) = 0;
    *(undefined8 *)(param_1 + 0x20) = 0;
  }
  return;
}

