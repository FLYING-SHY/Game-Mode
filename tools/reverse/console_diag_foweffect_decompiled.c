
void entry(longlong param_1,int *param_2)

{
  undefined4 uVar1;
  ulonglong uVar2;
  int local_res8 [2];
  
  local_res8[0] = *param_2;
  uVar1 = func_0x0001403aacd0(*(undefined4 *)(param_1 + 0x58),local_res8,4);
  *(longlong *)(param_1 + 0x60) = *(longlong *)(param_1 + 0x60) + 4;
  *(undefined4 *)(param_1 + 0x58) = uVar1;
  uVar2 = FUN_1406ec2e0(param_2);
  *(undefined1 *)(uVar2 + 0x6c + param_1) = 1;
  return;
}

