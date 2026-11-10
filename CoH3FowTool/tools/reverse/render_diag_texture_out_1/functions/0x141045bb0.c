// rea decompile_all
// name: FUN_141045bb0
// address: 0x141045bb0
// body: available
// range: 0x141045bb0-0x141045e26 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_141045bb0(void)

{
  longlong lVar1;
  undefined8 uVar2;
  longlong unaff_GS_OFFSET;
  
  if (DAT_146b46b7b == '\0') {
    DAT_146b46b7b = '\x01';
    uVar2 = FUN_141045f10();
    lVar1 = **(longlong **)(unaff_GS_OFFSET + 0x58);
    if (*(int *)(lVar1 + 0x30) < _DAT_146c20a98) {
      func_0x0001422baddc(&DAT_146c20a98);
      if (_DAT_146c20a98 == -1) {
        _DAT_146c20aa0 = uVar2;
        func_0x0001422bad70(&DAT_146c20a98);
      }
    }
    _DAT_146c20aa8 = _DAT_146af3750;
    _DAT_146af3750 = &DAT_146c20aa0;
    func_0x0001422bddc7(uVar2,0,0x145ed89a8,0x145ed8848,0);
    if (*(int *)(lVar1 + 0x30) < _DAT_146c20ab0) {
      func_0x0001422baddc(&DAT_146c20ab0);
      if (_DAT_146c20ab0 == -1) {
        _DAT_146c20ab8 = FUN_141045e30();
        _DAT_146c20ac0 = (undefined1 *)0x0;
        func_0x0001422bad70(&DAT_146c20ab0);
      }
    }
    _DAT_146c20ac0 = _DAT_146af3750;
    _DAT_146af3750 = &DAT_146c20ab8;
    uVar2 = func_0x000141047190();
    if (*(int *)(lVar1 + 0x30) < _DAT_146c20ac8) {
      func_0x0001422baddc(&DAT_146c20ac8);
      if (_DAT_146c20ac8 == -1) {
        _DAT_146c20ad0 = uVar2;
        func_0x0001422bad70(&DAT_146c20ac8);
      }
    }
    _DAT_146c20ad8 = _DAT_146af3750;
    _DAT_146af3750 = &DAT_146c20ad0;
    func_0x0001422bddc7(uVar2,0,0x145ed89a8,0x145ed8848,0);
    uVar2 = func_0x0001410460e0();
    if (*(int *)(lVar1 + 0x30) < _DAT_146c20ae0) {
      func_0x0001422baddc(&DAT_146c20ae0);
      if (_DAT_146c20ae0 == -1) {
        _DAT_146c20ae8 = uVar2;
        func_0x0001422bad70(&DAT_146c20ae0);
      }
    }
    _DAT_146c20af0 = _DAT_146af3750;
    _DAT_146af3750 = &DAT_146c20ae8;
    func_0x0001422bddc7(uVar2,0,0x145ed89a8,0x145ed8848,0);
    uVar2 = FUN_141045ff0();
    if (*(int *)(lVar1 + 0x30) < _DAT_146c20af8) {
      func_0x0001422baddc(&DAT_146c20af8);
      if (_DAT_146c20af8 == -1) {
        _DAT_146c20b00 = uVar2;
        func_0x0001422bad70(&DAT_146c20af8);
      }
    }
    _DAT_146c20b08 = _DAT_146af3750;
    _DAT_146af3750 = &DAT_146c20b00;
    func_0x0001422bddc7(uVar2,0,0x145ed89a8,0x145ed8848,0);
  }
  return;
}

