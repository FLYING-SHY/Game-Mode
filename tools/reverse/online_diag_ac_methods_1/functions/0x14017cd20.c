// rea decompile_all
// name: FUN_14017cd20
// address: 0x14017cd20
// body: available
// range: 0x14017cd20-0x14017cd65 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Ghidra Listing reports a terminal call at 0x14017cd61 to 0x14017ce20 FUN_14017ce20 (external=false, thunk=false, hasNoReturn=false); fallthrough is excluded by Ghidra's flow model. The reason for the terminal flow is not established by this observation.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_14017cd20(longlong param_1)

{
  longlong lVar1;
  
  lVar1 = FUN_14017cd70();
  if ((lVar1 != 0) &&
     ((_DAT_146ba4424 != *(float *)(param_1 + 0x24) || (*(int *)(param_1 + 0x20) != _DAT_146ba4420))
     )) {
    func_0x000140a6aed0(*(undefined8 *)(lVar1 + 0x38),*(undefined8 *)(param_1 + 0x20));
  }
  FUN_14017ce20(param_1);
  return;
}

