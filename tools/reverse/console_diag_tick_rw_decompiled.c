
void entry(longlong param_1)

{
  code *pcVar1;
  longlong lVar2;
  undefined8 uVar3;
  undefined8 uVar4;
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
  undefined8 local_68 [8];
  ulonglong local_28;
  
  local_28 = DAT_145df0fc0 ^ (ulonglong)auStack_148;
  lVar2 = func_0x0001405f8590();
  if (lVar2 != 0) {
    uVar3 = FUN_140187eb0(0x146a887a8);
    uVar4 = FUN_140187ee0(0x146a887a8);
    FUN_140c96650(DAT_146b02cc0,(char)uVar4);
    if ((char)uVar3 == '\0') {
      if ((char)uVar4 == '\0') {
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
        local_120 = *(undefined8 *)(lVar2 + 8);
        local_118 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x8c);
        local_114 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
        local_f0 = &local_128;
        (*(code *)0x5bf8cb2)(local_68,0x40,"FOWTextureUpdate",0x3f);
        FUN_1401f32b0(local_68,(longlong *)&local_128,(longlong *)&local_e8);
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
      local_120 = *(undefined8 *)(lVar2 + 8);
      local_118 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x8c);
      local_114 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
      local_f0 = &local_128;
      (*(code *)0x5bf8cb2)(&local_e8,0x40,"FOWTextureUpdateRevealExplored",0x3f);
      FUN_1401f32b0(&local_e8,(longlong *)&local_128,(longlong *)&local_a8);
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

