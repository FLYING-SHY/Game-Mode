// rea decompile_all
// name: FUN_1404b6970
// address: 0x1404b6970
// body: available
// range: 0x1404b6970-0x1404b6a23 inclusive
// range: 0x14275a508-0x14275a590 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_1404b6970(longlong param_1)

{
  longlong lVar1;
  
  lVar1 = *(longlong *)(param_1 + 0x540);
  if (lVar1 != 0) {
    func_0x00014031b310(lVar1,((*(longlong *)(param_1 + 0x550) - lVar1) / 0xc) * 0xc);
    *(undefined8 *)(param_1 + 0x540) = 0;
    *(undefined8 *)(param_1 + 0x548) = 0;
    *(undefined8 *)(param_1 + 0x550) = 0;
  }
  lVar1 = *(longlong *)(param_1 + 0x528);
  if (lVar1 != 0) {
    func_0x00014031b310(lVar1,*(longlong *)(param_1 + 0x538) - lVar1 & 0xfffffffffffffff0);
    *(undefined8 *)(param_1 + 0x528) = 0;
    *(undefined8 *)(param_1 + 0x530) = 0;
    *(undefined8 *)(param_1 + 0x538) = 0;
  }
  func_0x0001422bac50(param_1 + 0x28,0x50,0x10,0x141185eb0);
  if (*(longlong *)(param_1 + 0x10) != 0) {
    func_0x0001411afd30(*(longlong *)(param_1 + 0x10),*(undefined8 *)(param_1 + 0x18));
    func_0x00014031b310(*(longlong *)(param_1 + 0x10),
                        ((*(longlong *)(param_1 + 0x20) - *(longlong *)(param_1 + 0x10)) / 0x18) *
                        0x18);
    *(undefined8 *)(param_1 + 0x10) = 0;
    *(undefined8 *)(param_1 + 0x18) = 0;
    *(undefined8 *)(param_1 + 0x20) = 0;
  }
  return;
}

