// rea decompile_all
// name: entry
// address: 0x140c964a0
// body: available
// range: 0x140c964a0-0x140c96604 inclusive
// range: 0x14290efe2-0x14290f147 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void entry(longlong param_1)

{
  code *pcVar1;
  char cVar2;
  char cVar3;
  longlong lVar4;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_148 [32];
  undefined *local_128;
  undefined8 local_120;
  undefined4 local_118;
  undefined4 local_114;
  undefined **local_f0;
  undefined *local_e8;
  longlong local_e0;
  undefined **local_b0;
  undefined *local_a8;
  longlong local_a0;
  undefined **local_70;
  undefined1 local_68 [64];
  ulonglong local_28;
  
  local_28 = DAT_145df0fc0 ^ (ulonglong)auStack_148;
  lVar4 = func_0x0001405f8590();
  if (lVar4 != 0) {
    cVar2 = FUN_140187eb0(&DAT_146a887a8);
    cVar3 = FUN_140187ee0(&DAT_146a887a8);
    FUN_140c96650(DAT_146b02cc0,cVar3);
    if (cVar2 == '\0') {
      if (cVar3 == '\0') {
        LOCK();
        *(undefined1 *)(param_1 + 0x128) = 1;
        UNLOCK();
        FUN_140c96610(DAT_146b02cc0,1);
        local_e0 = param_1 + 0x130;
        LOCK();
        *(undefined1 *)(param_1 + 0x140) = 0;
        UNLOCK();
        local_e8 = &DAT_145417968;
        local_b0 = &local_e8;
        local_128 = &DAT_145417930;
        local_120 = *(undefined8 *)(lVar4 + 8);
        local_118 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x8c);
        local_114 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
        local_f0 = &local_128;
        (*(code *)0x5bf8cb2)(local_68,0x40,"FOWTextureUpdate",0x3f);
        FUN_1401f32b0(local_68,&local_128,&local_e8);
        func_0x0001401f5ea0(&local_128);
        func_0x0001401f5ea0(&local_e8);
      }
      else if (*(char *)(DAT_146b02cc0 + 0x40) != '\0') {
        LOCK();
        *(undefined1 *)(param_1 + 0x128) = 1;
        UNLOCK();
        FUN_140c96610(DAT_146b02cc0,0);
      }
    }
    else {
      LOCK();
      *(undefined1 *)(param_1 + 0x128) = 1;
      UNLOCK();
      FUN_140c96610(DAT_146b02cc0,1);
      local_a0 = param_1 + 0x130;
      LOCK();
      *(undefined1 *)(param_1 + 0x140) = 0;
      UNLOCK();
      local_a8 = &DAT_145417968;
      local_70 = &local_a8;
      local_128 = &DAT_145556848;
      local_120 = *(undefined8 *)(lVar4 + 8);
      local_118 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x8c);
      local_114 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
      local_f0 = &local_128;
      (*(code *)0x5bf8cb2)(&local_e8,0x40,"FOWTextureUpdateRevealExplored",0x3f);
      FUN_1401f32b0(&local_e8,&local_128,&local_a8);
      func_0x0001401f5ea0(&local_128);
      if (local_70 != (undefined **)0x0) {
        pcVar1 = *(code **)(*local_70 + 0x20);
        if (pcVar1 == FUN_1403b6430) {
          if (local_70 != &local_a8) {
            FUN_1403b6860(local_70,0x38);
          }
        }
        else if ((pcVar1 == FUN_1402f0800) || (pcVar1 == FUN_1401f3680)) {
          if (local_70 != &local_a8) {
            FUN_1403b6860(local_70,0x10);
          }
        }
        else {
          (*pcVar1)();
        }
      }
    }
  }
  func_0x0001422ba6e0(local_28 ^ (ulonglong)auStack_148);
  return;
}

