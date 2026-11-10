// rea decompile_all
// name: FUN_14060f900
// address: 0x14060f900
// body: available
// range: 0x14060f900-0x14061032d inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14060f900(longlong *param_1,undefined8 param_2,undefined8 param_3,byte *param_4,
                  longlong param_5)

{
  byte bVar1;
  undefined2 uVar2;
  char *pcVar3;
  longlong *plVar4;
  bool bVar5;
  char cVar6;
  char cVar7;
  undefined4 uVar8;
  uint uVar9;
  undefined4 uVar10;
  undefined8 uVar11;
  longlong lVar12;
  float *pfVar13;
  undefined8 uVar14;
  ulonglong *puVar15;
  longlong lVar16;
  longlong lVar17;
  undefined8 *puVar18;
  ulonglong uVar19;
  longlong lVar20;
  undefined1 uVar21;
  char cVar22;
  longlong lVar23;
  undefined1 auStack_488 [32];
  char local_468;
  char local_467;
  undefined8 local_460;
  undefined8 uStack_458;
  undefined8 local_450;
  undefined8 local_448;
  float local_440;
  float local_43c;
  ulonglong local_438;
  undefined8 uStack_430;
  undefined8 local_428;
  undefined1 local_420 [8];
  undefined4 local_418;
  undefined4 local_414;
  float local_410;
  float local_40c;
  undefined1 local_408 [8];
  undefined8 local_400;
  undefined1 local_3f8;
  undefined1 local_3f0 [24];
  undefined1 local_3d8 [728];
  float local_100;
  ulonglong local_78;
  
  local_78 = DAT_145df0fc0 ^ (ulonglong)auStack_488;
  local_448 = param_5;
  if (param_1[0xe] != 0) {
    uVar8 = func_0x000142ad0210();
    func_0x0001402ee380(1,uVar8);
    (**(code **)(*(longlong *)param_1[0xe] + 0x40))();
    func_0x00014060df30();
    lVar20 = *(longlong *)(param_1[0xe] + 0x10);
    uVar9 = (**(code **)(**(longlong **)(lVar20 + 8) + 0x10))();
    local_448 = CONCAT44(local_448._4_4_,(float)uVar9);
    uVar9 = (**(code **)(**(longlong **)(lVar20 + 8) + 0x18))();
    local_448 = CONCAT44((float)uVar9,(undefined4)local_448);
    func_0x0001402f4040(lVar20 + 0xf0,&local_448);
    func_0x0001402ee320(1);
    goto LAB_1406102f5;
  }
  uVar8 = *(undefined4 *)(param_4 + 0xc);
  if (*(longlong *)(param_1[0xf] + 0x10) == 0) goto LAB_1406102f5;
  uVar10 = func_0x000142ad0210();
  func_0x0001402ee380(1,uVar10);
  if ((*(char *)(param_5 + 0x70) == '\0') || ((char)param_1[1] == '\0')) {
    cVar7 = '\0';
    local_467 = '\0';
    lVar20 = *(longlong *)(param_1[0xf] + 0x10);
    if (*(int *)((longlong)param_1 + 0xac) != 0) {
      *(undefined4 *)((longlong)param_1 + 0xac) = 2;
    }
    lVar12 = FUN_140610330(lVar20);
    func_0x0001435d6cac(lVar12 + 0x18);
  }
  else {
    cVar7 = '\x01';
    local_467 = '\x01';
    lVar20 = *(longlong *)(param_1[0xf] + 0x10);
    if ((*(char *)((longlong)param_1 + 0x99) == '\0') && (*(int *)((longlong)param_1 + 0xac) != 0))
    {
      cVar6 = (**(code **)(*(longlong *)param_1[0xc] + 0x48))();
      uVar10 = 2;
      if (cVar6 != '\0') goto LAB_14060fa54;
    }
    else {
LAB_14060fa54:
      uVar10 = 1;
    }
    *(undefined4 *)((longlong)param_1 + 0xac) = uVar10;
    local_438 = param_1[0x17];
    local_460 = 0x1402be4c0;
    uVar11 = func_0x000140b3f020(local_420,&local_460,&local_438);
    lVar12 = FUN_140610330(lVar20);
    func_0x000140e71a10(lVar12 + 0x18,uVar11);
  }
  if ((*(char *)((longlong)param_1 + 0x99) == '\0') && (cVar7 != '\0')) {
    uVar11 = FUN_140610330(lVar20);
    func_0x00014060deb0(uVar11);
  }
  else {
    uVar11 = FUN_140610330(lVar20);
    func_0x00014060deb0(uVar11);
  }
  uVar11 = FUN_140610330(lVar20);
  func_0x00014060ded0(uVar11);
  bVar1 = *param_4;
  cVar6 = func_0x00014060de50(lVar20);
  if (cVar6 != '\0') {
    lVar12 = func_0x00014060def0(lVar20);
    *(byte *)(lVar12 + 0x1a) = ~bVar1 & 1;
    func_0x000142ad07d8(lVar20 + 0x98);
    cVar6 = func_0x00014060de30(lVar20);
    if (cVar6 != '\0') {
      lVar12 = func_0x00014060df10(lVar20);
      *(byte *)(lVar12 + 0x60) = ~bVar1 & 1;
    }
  }
  func_0x00014060de90(lVar20 + 0x18,param_3,lVar20 + 0x98);
  func_0x0001402fb500(local_3d8,param_2);
  uVar9 = (**(code **)(**(longlong **)(lVar20 + 8) + 0x28))();
  local_440 = (float)uVar9;
  uVar9 = (**(code **)(**(longlong **)(lVar20 + 8) + 0x30))();
  local_43c = (float)uVar9;
  bVar5 = false;
  cVar6 = func_0x00014060de70(lVar20);
  if (cVar6 != '\0') {
    uVar11 = func_0x00014060df00(lVar20);
    pfVar13 = (float *)func_0x000142ad0800(local_3d8);
    uVar14 = func_0x000142ad07e0(local_3d8);
    FUN_140610340(local_3f0,pfVar13,uVar14);
    cVar6 = func_0x00014060df80(uVar11,local_3f0,&local_460);
    if (cVar6 != '\0') {
      if (400.0 < ((float)local_460 - *pfVar13) * ((float)local_460 - *pfVar13) +
                  (local_460._4_4_ - pfVar13[1]) * (local_460._4_4_ - pfVar13[1]) +
                  ((float)uStack_458 - pfVar13[2]) * ((float)uStack_458 - pfVar13[2])) {
        bVar5 = true;
      }
    }
    puVar15 = (ulonglong *)func_0x000142ad07f0(local_3d8);
    local_438 = *puVar15 ^ 0x8000000080000000;
    uStack_430 = CONCAT44(uStack_430._4_4_,(int)puVar15[1]) ^ 0x80000000;
    FUN_140610340(local_420,pfVar13,&local_438);
    if ((!bVar5) && (cVar6 = func_0x00014060df80(uVar11,local_420,&local_460), cVar6 != '\0')) {
      if (400.0 < ((float)local_460 - *pfVar13) * ((float)local_460 - *pfVar13) +
                  (local_460._4_4_ - pfVar13[1]) * (local_460._4_4_ - pfVar13[1]) +
                  ((float)uStack_458 - pfVar13[2]) * ((float)uStack_458 - pfVar13[2])) {
        bVar5 = true;
      }
    }
  }
  if ((local_100 < 1.0) && (bVar5)) {
    func_0x000140fa8070(local_3d8);
  }
  if ((local_440 != 0.0) && (local_43c != 0.0)) {
    func_0x0001402bbb40(local_3d8);
  }
  local_468 = func_0x000140310a40("force_inactive_dxdevice");
  if (local_468 == '\0') {
    if (cVar7 == '\0') {
      lVar12 = func_0x00014060df20(lVar20);
      *(undefined8 *)(lVar12 + 0x140) = 0;
    }
    else {
      func_0x000140e41360(param_1);
      func_0x000140ebe320(param_1,local_448);
      lVar12 = param_1[0xd];
      lVar17 = *(longlong *)(*param_1 + 0x50);
      cVar6 = func_0x000140f91570(lVar20);
      lVar23 = 0;
      if (cVar6 != '\0') {
        uVar11 = func_0x000140ffd210(lVar20);
        local_460 = 0;
        uStack_458 = 0;
        local_450 = 0;
        func_0x000140c93690(*(undefined8 *)(lVar17 + 0x588),&local_460,lVar20 + 0x1c8);
        func_0x000140c9eb90(&local_460,uVar11);
        func_0x0001435d6c48(&local_460);
      }
      local_438 = 0;
      uStack_430 = 0;
      local_428 = 0;
      local_460 = lVar20;
      func_0x000140c5f4a0(&local_438,&local_460);
      if (*(int *)((longlong)param_1 + 0xac) == 1) {
        local_460 = *(longlong *)(param_1[0xc] + 0x10);
        func_0x000140c5f4a0(&local_438,&local_460);
      }
      if (param_1[4] != 0) {
        local_460 = param_1[4];
        func_0x000140c5f4a0(&local_438,&local_460);
      }
      func_0x0001402db570(param_1,&local_438,cVar7);
      if (*(char *)(lVar17 + 0x628) != '\0') {
        uVar11 = func_0x000141108230(lVar20);
        func_0x00014053bd10(uVar11,lVar17 + 0x5f0);
        *(undefined1 *)(lVar17 + 0x628) = 0;
      }
      cVar7 = func_0x000140f91590(lVar20);
      if (cVar7 != '\0') {
        lVar16 = func_0x00014060df20(lVar20);
        uVar2 = *(undefined2 *)(lVar17 + 0x5e8);
        cVar6 = (char)uVar2;
        *(undefined2 *)(lVar16 + 0x148) = uVar2;
        cVar22 = (char)((ushort)uVar2 >> 8);
        *(longlong *)(lVar16 + 0x140) = lVar17 + 0x5b0;
        cVar7 = func_0x000140f91530(lVar20);
        if (cVar7 != '\0') {
          cVar7 = func_0x0001402f7440(lVar16);
          if ((((cVar7 == '\0') || (cVar6 == '\0')) || (cVar22 != '\0')) ||
             ((*(char *)(lVar17 + 0x5e1) == '\0' && (*(char *)(lVar17 + 0x5e2) == '\0')))) {
            uVar21 = 0;
          }
          else {
            uVar21 = 1;
          }
          lVar16 = func_0x000140ffd1f0(lVar20);
          *(undefined1 *)(lVar16 + 0x3b4) = uVar21;
        }
      }
      cVar7 = func_0x000140f915b0(lVar17 + 0xe8);
      if (cVar7 == '\0') {
        pcVar3 = *(char **)(lVar17 + 0xe8);
        if (*pcVar3 == '\0') {
          func_0x000142f29ec8(lVar12);
          *(undefined1 *)(lVar12 + 0x62) = 1;
          *(undefined1 *)(param_1[0xc] + 0x51) = 1;
          func_0x0001407c3dc0(param_1[0xc],0);
          func_0x0001435d74e0(lVar17 + 0xe8);
        }
        else {
          func_0x0001435d7180(param_1,pcVar3 + 8);
          *(char *)(lVar12 + 0x62) = pcVar3[0x30];
          *(char *)(param_1[0xc] + 0x51) = pcVar3[0x30];
          func_0x0001435d74e0(lVar17 + 0xe8);
        }
      }
      else {
        cVar7 = func_0x0001407c3d90(lVar12);
        if (cVar7 == '\0') {
          func_0x000142f29ec8(lVar12);
          *(undefined1 *)(lVar12 + 0x62) = 1;
          *(undefined1 *)(param_1[0xc] + 0x51) = 1;
          func_0x0001407c3dc0(param_1[0xc],0);
        }
      }
      uVar11 = func_0x000140ffd220(lVar20);
      uVar9 = func_0x000140549d10(lVar17 + 0x98);
      if (uVar9 != 0) {
        uVar19 = (ulonglong)uVar9;
        do {
          func_0x000142e74678(uVar11,*(longlong *)(lVar17 + 0x98) + lVar23);
          lVar23 = lVar23 + 0x1c;
          uVar19 = uVar19 - 1;
        } while (uVar19 != 0);
      }
      func_0x0001402e35d0(lVar17 + 0x98);
      cVar7 = func_0x000140243950(lVar17 + 0x130);
      if ((cVar7 != '\0') && (cVar7 = func_0x0001435d696c(lVar20), cVar7 != '\0')) {
        uVar11 = func_0x0001435d6960(lVar20);
        func_0x000142e74150(uVar11,lVar17 + 0x130);
      }
      func_0x000142ad0fb0(&local_438);
    }
  }
  cVar7 = local_468;
  cVar6 = func_0x00014060df70(param_1 + 0x10);
  if ((cVar6 == '\0') && (*(longlong *)(*(longlong *)param_1[0x10] + 0x10) != param_1[0xf])) {
    if ((*(char *)((longlong)param_1 + 0x99) == '\0') &&
       (cVar6 = func_0x000142e779bc(lVar20), cVar6 != '\0')) {
      uVar11 = func_0x000142e779b0(lVar20);
      func_0x000142e8ecd4(uVar11);
    }
  }
  else {
    func_0x0001402f2fe0(lVar20,local_3d8,uVar8);
  }
  if (((cVar7 == '\0') && (*(char *)(local_448 + 0x70) != '\0')) && (local_467 != '\0')) {
    lVar12 = *(longlong *)(*param_1 + 0x50);
    if ((((longlong *)param_1[0xc] != (longlong *)0x0) &&
        (cVar7 = (**(code **)(*(longlong *)param_1[0xc] + 0x48))(), cVar7 != '\0')) &&
       (*(longlong **)(*(longlong *)param_1[0x10] + 0x10) == (longlong *)param_1[0xc])) {
      (**(code **)(*(longlong *)param_1[0xc] + 0x40))();
    }
    plVar4 = (longlong *)param_1[0xd];
    lVar17 = plVar4[2];
    cVar7 = func_0x000140f91550(lVar17);
    if (((cVar7 != '\0') && (lVar17 = func_0x000140ffd200(lVar17), lVar17 != 0)) &&
       (cVar7 = (**(code **)(*plVar4 + 0x48))(plVar4), cVar7 != '\0')) {
      func_0x0001412c1750(local_420);
      local_3f8 = *(undefined1 *)((longlong)plVar4 + 0x62);
      puVar18 = (undefined8 *)(lVar12 + 0x118);
      cVar7 = func_0x0001405391a0(puVar18);
      if ((cVar7 != '\0') && (cVar7 = func_0x0001402466b0(*puVar18), cVar7 == '\0')) {
        func_0x000142e7952c(local_420);
      }
      puVar18 = (undefined8 *)(lVar12 + 0x120);
      cVar7 = func_0x0001405391a0(puVar18);
      if ((cVar7 != '\0') && (cVar7 = func_0x0001402466b0(*puVar18), cVar7 == '\0')) {
        func_0x000142e794e4(local_408);
        local_400 = *(undefined8 *)(lVar12 + 0x128);
      }
      lVar12 = *(longlong *)(lVar12 + 0x218);
      if (lVar12 != 0) {
        local_418 = *(undefined4 *)(lVar12 + 0x9c);
        local_414 = *(undefined4 *)(lVar12 + 0xa0);
        local_410 = (float)*(ushort *)(lVar12 + 0xa8);
        local_40c = (float)*(ushort *)(lVar12 + 0xaa);
      }
      func_0x000142e79898(lVar17,local_420);
      func_0x000142e79604(local_420);
    }
    if ((((longlong *)param_1[0xd] != (longlong *)0x0) &&
        (cVar7 = (**(code **)(*(longlong *)param_1[0xd] + 0x48))(), cVar7 != '\0')) &&
       (*(longlong **)(*(longlong *)param_1[0x10] + 0x10) == (longlong *)param_1[0xd])) {
      (**(code **)(*(longlong *)param_1[0xd] + 0x40))();
    }
    plVar4 = param_1 + 2;
    if (((param_1[4] != 0) && (cVar7 = func_0x0001435d71b8(plVar4), cVar7 != '\0')) &&
       (*(longlong **)(*(longlong *)param_1[0x10] + 0x10) == plVar4)) {
      func_0x000143708960(plVar4);
    }
    cVar7 = func_0x00014060df70(param_1 + 0x10);
    if (cVar7 == '\0') {
      func_0x000140b7c7f0(param_1 + 0x10,&local_460,*(undefined8 *)param_1[0x10]);
    }
  }
  func_0x00014060ddb0();
  func_0x00014060df30();
  func_0x0001402f4040(lVar20 + 0xf0,&local_440);
  func_0x0001402ee320(1);
LAB_1406102f5:
  func_0x0001422ba6e0(local_78 ^ (ulonglong)auStack_488);
  return;
}

