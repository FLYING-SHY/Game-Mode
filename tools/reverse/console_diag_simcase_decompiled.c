
void entry(longlong param_1,longlong param_2)

{
  int iVar1;
  longlong lVar2;
  longlong unaff_GS_OFFSET;
  undefined8 local_res8 [4];
  
  iVar1 = *(int *)(param_1 + 8);
  if (((iVar1 == *(int *)(param_2 + 0x24)) && (*(char *)(param_2 + 0x48) == -1)) &&
     (*(char *)(param_2 + 0x60) != '\0')) {
    local_res8[0] = 0x8000000000000010;
    func_0x0001407d4b60(local_res8,0,0,"MOD -- (CHEAT) player %d revealed fow",iVar1);
    func_0x0001434342f4();
    local_res8[0] = 0x5ff018d0c7448e70;
    lVar2 = func_0x00014026d480(*(longlong *)
                                 ((ulonglong)
                                  *(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8 +
                                 0x146185e00) + 0x100000067,local_res8);
    if (lVar2 != 0) {
      func_0x0001433a1c68(lVar2,(int *)(param_1 + 8));
    }
  }
  return;
}

