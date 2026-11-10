// rea decompile_all
// name: FUN_140c964a0
// address: 0x140c964a0
// body: available
// range: 0x140c964a0-0x140c96604 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_140c964a0(longlong param_1)

{
  char cVar1;
  char cVar2;
  longlong lVar3;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_148 [32];
  undefined8 local_128;
  undefined8 local_120;
  undefined4 local_118;
  undefined4 local_114;
  undefined8 *local_f0;
  undefined8 local_e8;
  longlong local_e0;
  undefined8 *local_b0;
  undefined1 local_68 [64];
  ulonglong local_28;
  
  local_28 = DAT_145df0fc0 ^ (ulonglong)auStack_148;
  lVar3 = func_0x0001405f8590();
  if (lVar3 != 0) {
    cVar1 = func_0x000140187eb0(0x146a887a8);
    cVar2 = func_0x000140187ee0(0x146a887a8);
    FUN_140c96650(_DAT_146b02cc0,cVar2);
    if (cVar1 != '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    if (cVar2 != '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    LOCK();
    *(undefined1 *)(param_1 + 0x128) = 1;
    UNLOCK();
    FUN_140c96610(_DAT_146b02cc0,1);
    local_e0 = param_1 + 0x130;
    LOCK();
    *(undefined1 *)(param_1 + 0x140) = 0;
    UNLOCK();
    local_e8 = 0x145417968;
    local_b0 = &local_e8;
    local_128 = 0x145417930;
    local_120 = *(undefined8 *)(lVar3 + 8);
    local_118 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x8c);
    local_114 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
    local_f0 = &local_128;
    (*_DAT_143d56238)(local_68,0x40,0x143de6960,0x3f);
    func_0x0001401f32b0(local_68,&local_128,&local_e8);
    func_0x0001401f5ea0(&local_128);
    func_0x0001401f5ea0(&local_e8);
  }
  func_0x0001422ba6e0(local_28 ^ (ulonglong)auStack_148);
  return;
}

