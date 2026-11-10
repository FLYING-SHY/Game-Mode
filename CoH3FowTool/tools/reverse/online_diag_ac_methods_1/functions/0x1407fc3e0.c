// rea decompile_all
// name: FUN_1407fc3e0
// address: 0x1407fc3e0
// body: available
// range: 0x1407fc3e0-0x1407fc44b inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


ulonglong FUN_1407fc3e0(void)

{
  uint uVar1;
  longlong unaff_GS_OFFSET;
  
  if (*(int *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x30) < DAT_146bd07bc) {
    func_0x0001422baddc(&DAT_146bd07bc);
    if (DAT_146bd07bc == -1) {
      uVar1 = func_0x0001403aacd0(0,"AntiCheatWorldService",0x15);
      DAT_146b5bee8 = (ulonglong)uVar1;
      func_0x0001422bad70(&DAT_146bd07bc);
    }
  }
  return DAT_146b5bee8;
}

