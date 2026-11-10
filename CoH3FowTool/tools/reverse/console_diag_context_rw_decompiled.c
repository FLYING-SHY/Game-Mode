
ulonglong FUN_1401884a0(void)

{
  bool bVar1;
  ulonglong in_RAX;
  longlong lVar2;
  char cVar3;
  int iVar4;
  
  if ((DAT_145e0becc == -1) || (DAT_146b02cf0 == (longlong *)0x0)) goto LAB_14018854e;
  iVar4 = *(int *)(*(longlong *)(*DAT_146b02cf0 + 0x2b20) + 0x2f0);
  lVar2 = FUN_1403b4ba0(DAT_146b02cf0);
  cVar3 = (char)lVar2;
  lVar2 = FUN_1402de560();
  if ((lVar2 == 0) || (*(char *)(lVar2 + 0x680) == '\0')) {
    bVar1 = false;
    if (lVar2 != 0) goto LAB_14018851d;
LAB_140188532:
    in_RAX = 0;
  }
  else {
    bVar1 = true;
LAB_14018851d:
    if ((*(char *)(lVar2 + 0x691) != '\x05') && (1 < (byte)(*(char *)(lVar2 + 0x691) - 7U)))
    goto LAB_140188532;
    in_RAX = 1;
  }
  if ((((bVar1) || ((char)in_RAX != '\0')) || (iVar4 != 1)) || (cVar3 != '\0')) {
    return 1;
  }
LAB_14018854e:
  return in_RAX & 0xffffffffffffff00;
}

