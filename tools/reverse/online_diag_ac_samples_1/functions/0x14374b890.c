// rea decompile_all
// name: FUN_14374b890
// address: 0x14374b890
// body: available
// range: 0x14374b890-0x14374bab6 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Ghidra Listing reports a terminal call at 0x14374b954 with no resolved callee; its no-return basis is unknown.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14374b890(longlong param_1,uint param_2,longlong param_3)

{
  uint uVar1;
  code *pcVar2;
  char cVar3;
  longlong lVar4;
  longlong *plVar5;
  longlong lVar6;
  longlong lVar7;
  longlong lVar8;
  undefined8 uVar9;
  int iVar10;
  int iVar11;
  longlong unaff_GS_OFFSET;
  undefined4 extraout_XMM0_Da;
  undefined1 auStack_88 [32];
  uint local_68;
  longlong local_60;
  undefined4 local_58;
  undefined4 local_54;
  uint local_50;
  undefined4 local_4c;
  int local_48;
  int iStack_44;
  ulonglong local_40;
  
  local_40 = DAT_145df0fc0 ^ (ulonglong)auStack_88;
  iVar10 = *(int *)(param_3 + 8);
  local_68 = param_2;
  lVar4 = func_0x0001401e3f90(*DAT_146b02cf0 + 0x1930);
  if (lVar4 == 0) {
    plVar5 = &DAT_14464da78;
  }
  else {
    plVar5 = (longlong *)(lVar4 + 0x18);
  }
  if (((*plVar5 != -1) && (*(int *)(param_1 + 0x558) != iVar10)) &&
     ((*(char *)(param_3 + 0x692) == '\0' || (*(char *)(param_3 + 0x692) == '\x02')))) {
    lVar4 = *(longlong *)(DAT_146a88858 + 0x18);
    func_0x000140327b30(extraout_XMM0_Da,lVar4 + -0x50);
    func_0x000140327bd0(lVar4 + -0x48,&local_50,lVar4 + -0x50);
    if (CONCAT44(iStack_44,local_48) == 0) {
      (*(code *)0x5bf4e38)("invalid unordered_map<K, T> key");
      pcVar2 = (code *)swi(3);
      (*pcVar2)();
      return;
    }
    local_50 = param_2;
    local_4c = func_0x000140aacbe0(CONCAT44(iStack_44,local_48) + 0x18,0xc);
    iVar10 = 0;
    iVar11 = 0;
    uVar1 = *(uint *)((ulonglong)DAT_145e0becc + 0x100 +
                     *(longlong *)
                      (&DAT_146185e00 +
                      (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8));
    if (uVar1 == 0xffffffff) {
      lVar4 = 0;
    }
    else {
      lVar4 = *(longlong *)
               (&DAT_146185e00 +
               (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8) +
              (ulonglong)uVar1;
    }
    lVar6 = func_0x0001401943a0(lVar4 + 0x708);
    lVar7 = func_0x0001401943a0(lVar4 + 0x708);
    uVar1 = *(uint *)(lVar4 + 0x70c);
    for (; lVar6 != lVar7 + (ulonglong)uVar1 * 4; lVar6 = lVar6 + 4) {
      lVar4 = func_0x0001401943a0(lVar6);
      cVar3 = func_0x00014374c180(param_3,lVar4);
      if (cVar3 != '\0') {
        iVar11 = iVar11 + 1;
      }
      lVar4 = lVar4 + 0x100;
      lVar8 = func_0x0001401943a0(lVar4);
      local_60 = 0;
      func_0x00014042fa40(lVar4,&local_60);
      for (; lVar8 != local_60; lVar8 = lVar8 + 4) {
        uVar9 = func_0x0001401943a0(lVar8);
        cVar3 = func_0x00014374c0b0(param_3,uVar9);
        if (cVar3 != '\0') {
          iVar10 = iVar10 + 1;
        }
      }
    }
    local_58 = *(undefined4 *)(DAT_146a97ae0 + 4);
    local_54 = 0;
    LOCK();
    local_60 = (ulonglong)local_68 << 0x20;
    UNLOCK();
    local_48 = iVar10;
    iStack_44 = iVar11;
    func_0x0001433ffd2c((ulonglong)*(uint *)(param_3 + 0xc) * 0x50 + 0x28 + param_1,&local_50);
    FUN_14374b088(param_1);
  }
  func_0x0001422ba6e0(local_40 ^ (ulonglong)auStack_88);
  return;
}

