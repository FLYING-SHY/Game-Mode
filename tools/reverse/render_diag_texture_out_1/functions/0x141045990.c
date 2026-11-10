// rea decompile_all
// name: FUN_141045990
// address: 0x141045990
// body: available
// range: 0x141045990-0x141045a90 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined1 * FUN_141045990(void)

{
  longlong unaff_GS_OFFSET;
  
  if (*(int *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x30) < _DAT_146b37030) {
    func_0x0001422baddc(&DAT_146b37030);
    if (_DAT_146b37030 == -1) {
      _DAT_146b37040 = 0;
      uRam0000000146b37048 = 0;
      _DAT_146b37050 = 0;
      uRam0000000146b37058 = 0;
      _DAT_146b37060 = 0;
      uRam0000000146b37068 = 0;
      _DAT_146b37070 = 0;
      uRam0000000146b37078 = 0;
      _DAT_146b37080 = 0;
      uRam0000000146b37088 = 0;
      _DAT_146b37090 = 0;
      uRam0000000146b37098 = 0;
      if (DAT_146b07127 == '\0') {
        DAT_146b07127 = '\x01';
        _DAT_146b931e0 = 0x143e073f8;
        _DAT_146b931e8 = 0x145e0d1e0;
      }
      func_0x000140986f50(&DAT_146b37040,0x146adc210,0x143e3b528,0x146b93180);
      _DAT_146b37040 = 0x143e07008;
      func_0x0001422bac38(0x142615c70);
      func_0x0001422bad70(&DAT_146b37030);
      return &DAT_146b37040;
    }
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

