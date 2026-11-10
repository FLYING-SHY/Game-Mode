// rea decompile_all
// name: FUN_141045f10
// address: 0x141045f10
// body: available
// range: 0x141045f10-0x141045fe5 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined1 * FUN_141045f10(void)

{
  undefined8 uVar1;
  longlong unaff_GS_OFFSET;
  
  if (*(int *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x30) < _DAT_146b480e0) {
    func_0x0001422baddc(&DAT_146b480e0);
    if (_DAT_146b480e0 == -1) {
      _DAT_146b480f0 = 0;
      uRam0000000146b480f8 = 0;
      _DAT_146b48100 = 0;
      uRam0000000146b48108 = 0;
      _DAT_146b48110 = 0;
      uRam0000000146b48118 = 0;
      _DAT_146b48120 = 0;
      uRam0000000146b48128 = 0;
      _DAT_146b48130 = 0;
      uRam0000000146b48138 = 0;
      _DAT_146b48140 = 0;
      uRam0000000146b48148 = 0;
      uVar1 = func_0x000141483fe0();
      func_0x000140986f50(&DAT_146b480f0,0x146af3710,0x143dc5a84,uVar1);
      _DAT_146b480f0 = 0x143e07008;
      func_0x0001422bac38(0x1426230c0);
      func_0x0001422bad70(&DAT_146b480e0);
      return &DAT_146b480f0;
    }
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

