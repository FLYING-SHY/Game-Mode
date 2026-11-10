
/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void entry(longlong param_1)

{
  longlong lVar1;
  undefined8 uVar2;
  undefined8 uVar3;
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
  undefined1 local_68 [64];
  ulonglong local_28;
  
  local_28 = (ulonglong)auStack_148 ^ 0x2b992ddfa232;
  lVar1 = func_0x0001405f8590();
  if (lVar1 != 0) {
    uVar2 = FUN_140187eb0(0x146a887a8);
    uVar3 = FUN_140187ee0(0x146a887a8);
    FUN_140c96650(_DAT_146b02cc0,(char)uVar3);
    if ((char)uVar2 != '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    if ((char)uVar3 != '\0') {
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
    local_e8 = &DAT_145417968;
    local_b0 = &local_e8;
    local_128 = &DAT_145417930;
    local_120 = *(undefined8 *)(lVar1 + 8);
    local_118 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x8c);
    local_114 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
    local_f0 = &local_128;
    (*(code *)0x5bf8cb2)(local_68,0x40,"FOWTextureUpdate",0x3f);
    func_0x0001401f32b0(local_68,&local_128,&local_e8);
    func_0x0001401f5ea0(&local_128);
    func_0x0001401f5ea0(&local_e8);
  }
  func_0x0001422ba6e0(local_28 ^ (ulonglong)auStack_148);
  return;
}

