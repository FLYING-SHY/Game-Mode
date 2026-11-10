// rea decompile_all
// name: FUN_14374bab8
// address: 0x14374bab8
// body: available
// range: 0x14374bab8-0x14374bf54 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Ghidra Listing reports a terminal call at 0x14374bf54 with no resolved callee; its no-return basis is unknown.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14374bab8(longlong param_1,uint param_2)

{
  undefined8 *puVar1;
  uint *puVar2;
  code *pcVar3;
  char cVar4;
  undefined4 uVar5;
  uint uVar6;
  undefined8 uVar7;
  longlong *plVar8;
  longlong lVar9;
  longlong lVar10;
  longlong lVar11;
  longlong lVar12;
  longlong lVar13;
  int iVar14;
  ulonglong uVar15;
  ulonglong uVar16;
  ulonglong uVar17;
  uint uVar18;
  longlong unaff_GS_OFFSET;
  float extraout_XMM0_Da;
  undefined4 extraout_XMM0_Db;
  float fVar19;
  undefined1 auVar21 [16];
  undefined1 auStack_c8 [32];
  uint local_a8;
  undefined8 local_a0;
  longlong *local_98;
  ulonglong local_90;
  uint local_88;
  float local_84;
  uint local_80;
  uint uStack_7c;
  longlong local_78;
  uint local_70;
  longlong local_68;
  undefined8 local_60;
  longlong *local_58;
  longlong local_50;
  undefined4 local_48;
  undefined4 local_44;
  ulonglong local_40;
  undefined1 auVar20 [16];
  
  local_40 = DAT_145df0fc0 ^ (ulonglong)auStack_c8;
  lVar10 = **(longlong **)(unaff_GS_OFFSET + 0x58);
  local_78 = param_1;
  local_70 = param_2;
  local_68 = lVar10;
  local_60 = func_0x00014018c920((ulonglong)
                                 *(uint *)((ulonglong)DAT_145e0becc + 0xfc +
                                          *(longlong *)
                                           (&DAT_146185e00 + (ulonglong)*(uint *)(lVar10 + 0x40) * 8
                                           )) +
                                 *(longlong *)
                                  (&DAT_146185e00 + (ulonglong)*(uint *)(lVar10 + 0x40) * 8),
                                 param_1 + 0x558);
  lVar9 = *(longlong *)(DAT_146a88858 + 0x18);
  uVar7 = func_0x000140327b30(DAT_146a88858,lVar9 + -0x50);
  func_0x000140327bd0(lVar9 + -0x48,&local_88,lVar9 + -0x50,uVar7);
  uVar15 = 0;
  if (CONCAT44(uStack_7c,local_80) != 0) {
    uVar5 = func_0x000140aacbe0(CONCAT44(uStack_7c,local_80) + 0x18,0);
    auVar21._4_4_ = extraout_XMM0_Db;
    auVar21._0_4_ = extraout_XMM0_Da;
    iVar14 = (int)extraout_XMM0_Da;
    fVar19 = extraout_XMM0_Da;
    if ((iVar14 != -0x80000000) && ((float)iVar14 != extraout_XMM0_Da)) {
      auVar21._8_4_ = extraout_XMM0_Db;
      auVar21._12_4_ = extraout_XMM0_Db;
      auVar20._8_8_ = auVar21._8_8_;
      auVar20._4_4_ = extraout_XMM0_Da;
      auVar20._0_4_ = extraout_XMM0_Da;
      uVar6 = movmskps(uVar5,auVar20);
      iVar14 = iVar14 - (uVar6 & 1);
      fVar19 = (float)iVar14;
    }
    lVar9 = *(longlong *)(DAT_146a88858 + 0x18);
    uVar7 = func_0x000140327b30(iVar14,lVar9 + -0x50);
    func_0x000140327bd0(lVar9 + -0x48,&local_88,lVar9 + -0x50,uVar7);
    if (CONCAT44(uStack_7c,local_80) != 0) {
      local_84 = (float)func_0x000140aacbe0(CONCAT44(uStack_7c,local_80) + 0x18,0xc);
      if (local_84 <= fVar19) {
        local_84 = fVar19;
      }
      local_a0 = (ulonglong)local_a0._4_4_ << 0x20;
      local_a8 = 0;
      uVar6 = *(uint *)((ulonglong)DAT_145e0becc + 0x100 +
                       *(longlong *)(&DAT_146185e00 + (ulonglong)*(uint *)(lVar10 + 0x40) * 8));
      local_90 = uVar15;
      if (uVar6 != 0xffffffff) {
        local_90 = *(longlong *)(&DAT_146185e00 + (ulonglong)*(uint *)(lVar10 + 0x40) * 8) +
                   (ulonglong)uVar6;
      }
      lVar10 = local_90 + 0x708;
      local_88 = param_2;
      plVar8 = (longlong *)func_0x0001401943a0(lVar10);
      local_98 = plVar8;
      lVar9 = func_0x0001401943a0(lVar10);
      uVar7 = local_60;
      local_58 = (longlong *)(lVar9 + (ulonglong)*(uint *)(lVar10 + 4) * 4);
      while (lVar10 = local_78, uVar6 = (uint)uVar15, plVar8 != local_58) {
        lVar10 = func_0x0001401943a0(plVar8);
        lVar10 = lVar10 + 0x100;
        lVar11 = func_0x0001401943a0(lVar10);
        local_a0 = 0;
        func_0x00014042fa40(lVar10,&local_a0);
        lVar9 = local_78;
        lVar10 = local_a0;
        for (; lVar11 != lVar10; lVar11 = lVar11 + 4) {
          lVar12 = func_0x0001401943a0(lVar11);
          lVar13 = func_0x0001405daad0(**(undefined8 **)(*(longlong *)(lVar9 + 8) + 0xa8),
                                       *(undefined8 *)(lVar12 + 0x88));
          if (((lVar13 != 0) && (*(short *)(lVar13 + 0x6a) < 1)) &&
             (*(char *)(lVar13 + 0x69) == '\0')) {
            uVar15 = (ulonglong)((int)uVar15 + 1);
            cVar4 = FUN_14374c0b0(uVar7,lVar12);
            if (cVar4 == '\0') {
              local_50 = *(longlong *)(lVar9 + 0x558);
              plVar8 = *(longlong **)(lVar9 + 0x548);
              local_48 = 0x800;
              if (plVar8 == *(longlong **)(lVar9 + 0x550)) {
                func_0x000142b371b0(lVar9 + 0x540,plVar8,&local_50);
              }
              else {
                *plVar8 = local_50;
                *(undefined4 *)(plVar8 + 1) = 0x800;
                *(longlong *)(lVar9 + 0x548) = *(longlong *)(lVar9 + 0x548) + 0xc;
              }
            }
          }
        }
        local_a0 = CONCAT44(local_a0._4_4_,(int)uVar15);
        plVar8 = (longlong *)((longlong)local_98 + 4);
        local_98 = plVar8;
      }
      uVar16 = 0;
      local_98 = (longlong *)0x0;
      puVar1 = *(undefined8 **)(*(longlong *)(local_78 + 8) + 0xb8);
      func_0x000140557bf0(&local_98,*puVar1,puVar1[1]);
      uVar15 = local_90;
      uStack_7c = local_a8;
      if ((local_98 != (longlong *)puVar1[1]) && (lVar9 = *local_98, lVar9 != 0)) {
        lVar11 = *(longlong *)(&DAT_146185e00 + (ulonglong)*(uint *)(local_68 + 0x40) * 8);
        uVar6 = *(uint *)((ulonglong)DAT_145e0becc + 0x104 + lVar11);
        uVar17 = uVar16;
        uVar18 = local_a8;
        if (*(longlong *)(lVar9 + 0x18) == *(longlong *)(lVar9 + 0x10)) {
          uVar6 = (uint)local_a0;
        }
        else {
          do {
            lVar10 = *(longlong *)(lVar9 + 0x40);
            if ((*(char *)(uVar16 + 0xc + lVar10) == '\0') &&
               ((*(byte *)(uVar17 + *(longlong *)(lVar9 + 0x58)) & 1) != 0)) {
              local_68 = *(longlong *)(*(longlong *)(lVar9 + 0x88) + uVar17 * 8);
              uVar7 = func_0x00014038f9e0((ulonglong)uVar6 + lVar11,&local_68);
              cVar4 = FUN_14374bfd0(uVar7);
              if (cVar4 == '\0') {
                local_90 = *(ulonglong *)(uVar16 + lVar10);
                uVar18 = uVar18 + 1;
                lVar10 = func_0x000140390510(uVar15,&local_90);
                if ((lVar10 == 0) || (cVar4 = FUN_14374c180(local_60,lVar10), cVar4 == '\0')) {
                  local_48 = 0x1000;
                  local_50 = *(longlong *)(local_78 + 0x558);
                  plVar8 = *(longlong **)(local_78 + 0x548);
                  if (plVar8 == *(longlong **)(local_78 + 0x550)) {
                    func_0x000142b371b0(local_78 + 0x540,plVar8,&local_50);
                  }
                  else {
                    *plVar8 = local_50;
                    *(undefined4 *)(plVar8 + 1) = 0x1000;
                    *(longlong *)(local_78 + 0x548) = *(longlong *)(local_78 + 0x548) + 0xc;
                  }
                }
              }
            }
            uVar17 = uVar17 + 1;
            uVar16 = uVar16 + 0x28;
          } while (uVar17 < (ulonglong)(*(longlong *)(lVar9 + 0x18) - *(longlong *)(lVar9 + 0x10)));
          lVar10 = local_78;
          uStack_7c = uVar18;
          uVar6 = (uint)local_a0;
        }
      }
      local_48 = *(undefined4 *)(DAT_146a97ae0 + 4);
      local_44 = 0;
      LOCK();
      local_50 = (ulonglong)local_70 << 0x20;
      UNLOCK();
      puVar2 = *(uint **)(lVar10 + 0x530);
      local_80 = uVar6;
      if (puVar2 == *(uint **)(lVar10 + 0x538)) {
        func_0x000142ade42c(lVar10 + 0x528,puVar2,&local_88);
      }
      else {
        *puVar2 = local_70;
        puVar2[1] = (uint)local_84;
        puVar2[2] = uVar6;
        puVar2[3] = uStack_7c;
        *(longlong *)(lVar10 + 0x530) = *(longlong *)(lVar10 + 0x530) + 0x10;
      }
      func_0x0001422ba6e0(local_40 ^ (ulonglong)auStack_c8);
      return;
    }
  }
  (*(code *)0x5bf4e38)("invalid unordered_map<K, T> key");
  pcVar3 = (code *)swi(3);
  (*pcVar3)();
  return;
}

