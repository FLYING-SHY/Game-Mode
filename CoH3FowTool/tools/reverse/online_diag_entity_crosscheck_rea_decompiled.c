
void entry(longlong param_1,undefined8 param_2)

{
  uint uVar1;
  bool bVar2;
  ulonglong uVar3;
  longlong lVar4;
  undefined1 uVar5;
  longlong unaff_GS_OFFSET;
  undefined8 local_res10;
  
  local_res10 = param_2;
  uVar3 = FUN_1401884a0();
  if (((char)uVar3 == '\0') || (DAT_146a888a1 == '\0')) {
    bVar2 = false;
  }
  else {
    bVar2 = true;
  }
  lVar4 = FUN_1401e97f0((longlong *)(param_1 + 0x28),param_2);
  if (lVar4 != 0) {
    uVar1 = *(uint *)(lVar4 + 0x48);
    if (((uVar1 & 4) == 0) || ((!bVar2 && ((uVar1 & 0x10) == 0)))) {
      bVar2 = false;
    }
    else {
      bVar2 = true;
    }
    if (((uVar1 & 2) != 0) || (bVar2)) {
      uVar5 = 0;
    }
    else {
      uVar5 = 1;
    }
    lVar4 = func_0x00014038f9e0((ulonglong)
                                *(uint *)((ulonglong)DAT_145e0becc + 0x104 +
                                         *(longlong *)
                                          (&DAT_146185e00 +
                                          (ulonglong)
                                          *(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40)
                                          * 8)) +
                                *(longlong *)
                                 (&DAT_146185e00 +
                                 (ulonglong)
                                 *(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8),
                                &local_res10);
    if ((lVar4 != 0) && (*(int *)(lVar4 + 0x88) != 0)) {
      lVar4 = func_0x0001405daad0(**(undefined8 **)(*(longlong *)(param_1 + 8) + 0xa8),
                                  *(undefined8 *)(lVar4 + 0x88));
      if (lVar4 != 0) {
        func_0x0001401ea840(lVar4,2,uVar5);
      }
    }
  }
  return;
}


