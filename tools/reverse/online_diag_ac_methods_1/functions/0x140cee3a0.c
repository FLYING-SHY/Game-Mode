// rea decompile_all
// name: FUN_140cee3a0
// address: 0x140cee3a0
// body: available
// range: 0x140cee3a0-0x140ceeb41 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Ghidra Listing reports a terminal call at 0x140ceea29 with no resolved callee; its no-return basis is unknown.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_140cee3a0(longlong param_1)

{
  int *piVar1;
  int iVar2;
  code *pcVar3;
  longlong lVar4;
  longlong *plVar5;
  char cVar6;
  undefined1 uVar7;
  undefined4 uVar8;
  undefined8 uVar9;
  longlong lVar10;
  longlong *plVar11;
  longlong lVar12;
  longlong *plVar13;
  longlong *plVar14;
  longlong *plVar15;
  undefined1 auStack_88 [32];
  longlong *local_68;
  longlong local_60;
  longlong *local_58;
  longlong *local_50;
  longlong lStack_48;
  longlong *local_40;
  longlong local_38;
  ulonglong local_30;
  
  local_30 = DAT_145df0fc0 ^ (ulonglong)auStack_88;
  local_60 = param_1;
  if (*(longlong *)(param_1 + 0xa8) == _DAT_146a8cbe8) goto LAB_140ceeb18;
  *(longlong *)(param_1 + 0xa8) = _DAT_146a8cbe8;
  if (*(char *)(param_1 + 0xb0) == '\0') {
LAB_140ceeacb:
    if ((*(longlong *)(param_1 + 0xb8) == 0) || (cVar6 = FUN_140ceeb50(), cVar6 == '\0')) {
      uVar7 = 0;
    }
    else {
      uVar7 = 1;
    }
    *(undefined1 *)(param_1 + 0xb1) = uVar7;
  }
  else {
    local_58 = (longlong *)0x0;
    func_0x00014020c9a0(param_1 + 0x88,&local_58);
    plVar5 = local_58;
    if (local_58 != (longlong *)0x0) {
      if (*(code **)(*local_58 + 0x68) == (code *)&SUB_1403249e0) {
        lVar12 = local_58[0xb];
        if (lVar12 != _DAT_146a8cbe8) {
          local_58[0xb] = _DAT_146a8cbe8;
          local_40 = (longlong *)0x0;
          if ((char)local_58[0xc] != '\0') {
            uVar9 = func_0x00014020c9a0(local_58 + 7,&local_38);
            func_0x000140228ca0(&local_40,uVar9);
            if (local_38 != 0) {
              LOCK();
              piVar1 = (int *)(local_38 + 0x10);
              iVar2 = *piVar1;
              *piVar1 = *piVar1 + -1;
              UNLOCK();
              if (iVar2 == 1) {
                func_0x000140452990();
              }
            }
            plVar11 = local_40;
            if (local_40 != (longlong *)0x0) {
              pcVar3 = *(code **)(*local_40 + 0x68);
              if (pcVar3 == (code *)&SUB_1403249e0) {
                cVar6 = func_0x0001403249e0(local_40);
              }
              else if (pcVar3 == (code *)0x14151e2b0) {
                if (local_40[0x15] != _DAT_146a8cbe8) {
                  local_40[0x15] = _DAT_146a8cbe8;
                  if ((char)local_40[0x16] != '\0') {
                    local_68 = (longlong *)0x0;
                    func_0x00014020c9a0(local_40 + 0x11,&local_68);
                    plVar15 = local_68;
                    if (local_68 != (longlong *)0x0) {
                      if (*(code **)(*local_68 + 0x68) == (code *)&SUB_1403249e0) {
                        cVar6 = func_0x0001403249e0();
                      }
                      else {
                        cVar6 = (**(code **)(*local_68 + 0x68))(local_68);
                      }
                      if (cVar6 != '\0') {
                        LOCK();
                        plVar13 = plVar15 + 2;
                        lVar10 = *plVar13;
                        *(int *)plVar13 = (int)*plVar13 + -1;
                        UNLOCK();
                        if ((int)lVar10 == 1) {
                          func_0x000140452990(plVar15);
                        }
                        goto LAB_140cee545;
                      }
                    }
                    *(undefined1 *)((longlong)plVar11 + 0xb1) = 0;
                    if (plVar15 != (longlong *)0x0) {
                      LOCK();
                      plVar11 = plVar15 + 2;
                      lVar10 = *plVar11;
                      *(int *)plVar11 = (int)*plVar11 + -1;
                      UNLOCK();
                      if ((int)lVar10 == 1) {
                        func_0x000140452990(plVar15);
                      }
                    }
                    cVar6 = '\0';
                    param_1 = local_60;
                    goto LAB_140cee713;
                  }
LAB_140cee545:
                  if ((plVar11[0x17] == 0) || (cVar6 = func_0x00014151e250(), cVar6 == '\0')) {
LAB_140cee58b:
                    uVar7 = 0;
                  }
                  else {
                    uVar7 = 1;
                  }
LAB_140cee58d:
                  *(undefined1 *)((longlong)plVar11 + 0xb1) = uVar7;
                }
LAB_140cee593:
                cVar6 = *(char *)((longlong)plVar11 + 0xb1);
                param_1 = local_60;
              }
              else if (pcVar3 == (code *)0x140a3e410) {
                if (local_40[0x15] == _DAT_146a8cbe8) goto LAB_140cee593;
                local_40[0x15] = _DAT_146a8cbe8;
                if ((char)local_40[0x16] == '\0') {
LAB_140cee62b:
                  plVar15 = (longlong *)plVar11[0x17];
                  if (plVar15 != (longlong *)0x0) {
                    if (*(code **)(*plVar15 + 0x20) == (code *)0x1405571b0) {
                      if ((char)plVar15[0x11] == '\0') {
                        func_0x000140a48500(plVar15);
                        *(undefined1 *)(plVar15 + 0x11) = 1;
                      }
                      lVar10 = func_0x0001405579d0(*(undefined8 *)(DAT_146185f18 + 0xb8));
                      cVar6 = '\0';
                      if (lVar10 != 0) {
                        lVar4 = *(longlong *)(lVar10 + 0xc0);
                        if (lVar4 != plVar15[10]) {
                          if (lVar4 == 0) {
                            func_0x000140a48500(plVar15);
                          }
                          plVar15[10] = lVar4;
                          func_0x000140a49080(plVar15,lVar10 + 0x10);
                        }
                        (**(code **)(*plVar15 + 0x50))(plVar15);
                        if ((char)plVar15[0x15] != '\0') {
                          func_0x000140a48c10(plVar15);
                        }
                        cVar6 = '\x01';
                      }
                    }
                    else {
                      cVar6 = (**(code **)(*plVar15 + 0x20))(plVar15);
                    }
                    if (cVar6 != '\0') {
                      uVar7 = 1;
                      goto LAB_140cee58d;
                    }
                  }
                  goto LAB_140cee58b;
                }
                local_68 = (longlong *)0x0;
                func_0x00014020c9a0(local_40 + 0x11,&local_68);
                plVar15 = local_68;
                if (local_68 != (longlong *)0x0) {
                  if (*(code **)(*local_68 + 0x68) == (code *)&SUB_1403249e0) {
                    cVar6 = func_0x0001403249e0();
                  }
                  else {
                    cVar6 = (**(code **)(*local_68 + 0x68))(local_68);
                  }
                  if (cVar6 != '\0') {
                    LOCK();
                    plVar13 = plVar15 + 2;
                    lVar10 = *plVar13;
                    *(int *)plVar13 = (int)*plVar13 + -1;
                    UNLOCK();
                    if ((int)lVar10 == 1) {
                      func_0x000140452990(plVar15);
                    }
                    goto LAB_140cee62b;
                  }
                }
                *(undefined1 *)((longlong)plVar11 + 0xb1) = 0;
                if (plVar15 != (longlong *)0x0) {
                  LOCK();
                  plVar11 = plVar15 + 2;
                  lVar10 = *plVar11;
                  *(int *)plVar11 = (int)*plVar11 + -1;
                  UNLOCK();
                  if ((int)lVar10 == 1) {
                    func_0x000140452990(plVar15);
                  }
                }
                cVar6 = '\0';
                param_1 = local_60;
              }
              else {
                cVar6 = (*pcVar3)(local_40);
              }
LAB_140cee713:
              if (cVar6 != '\0') goto LAB_140cee741;
            }
            *(undefined1 *)((longlong)plVar5 + 0x61) = 0;
            if (local_40 != (longlong *)0x0) {
              LOCK();
              plVar11 = local_40 + 2;
              lVar12 = *plVar11;
              *(int *)plVar11 = (int)*plVar11 + -1;
              UNLOCK();
              if ((int)lVar12 == 1) {
                func_0x000140452990(local_40);
              }
            }
            cVar6 = '\0';
            goto LAB_140ceeaad;
          }
LAB_140cee741:
          pcVar3 = *(code **)(*plVar5 + 0x80);
          if (pcVar3 == (code *)0x140323f70) {
            if (lVar12 < plVar5[0x17]) {
LAB_140cee7ba:
              (**(code **)(*plVar5 + 0x90))(plVar5,&local_40,lVar12);
              cVar6 = '\x01';
            }
            else {
              local_50 = (longlong *)0x0;
              lStack_48 = 0;
              func_0x0001404f9050(plVar5 + 0x13,&local_50);
              if ((local_50 == (longlong *)0x0) ||
                 (cVar6 = (**(code **)(*local_50 + 0x20))(), cVar6 == '\0')) {
                plVar5[0x16] = 0;
              }
              else {
                lVar10 = (**(code **)(*local_50 + 8))(local_50,plVar5 + 0x15);
                plVar5[0x16] = lVar10;
                if (lVar10 != 0) {
LAB_140cee7ac:
                  if (lStack_48 != 0) {
                    func_0x0001403a9cb0();
                  }
                  goto LAB_140cee7ba;
                }
              }
              if (lStack_48 != 0) {
                func_0x0001403a9cb0();
              }
LAB_140cee7e9:
              cVar6 = '\0';
            }
          }
          else {
            if (pcVar3 == (code *)0x140323ef0) {
              if (lVar12 < plVar5[0x17]) goto LAB_140cee7ba;
              local_50 = (longlong *)0x0;
              lStack_48 = 0;
              func_0x0001404f9050(plVar5 + 0x13,&local_50);
              if ((local_50 == (longlong *)0x0) ||
                 (cVar6 = (**(code **)(*local_50 + 0x20))(), cVar6 == '\0')) {
                plVar5[0x16] = 0;
              }
              else {
                lVar10 = (**(code **)(*local_50 + 8))(local_50,plVar5 + 0x15);
                plVar5[0x16] = lVar10;
                if (lVar10 != 0) goto LAB_140cee7ac;
              }
              if (lStack_48 != 0) {
                func_0x0001403a9cb0();
                cVar6 = '\0';
                goto LAB_140ceea5d;
              }
              goto LAB_140cee7e9;
            }
            if (pcVar3 == (code *)0x140323b50) {
              if (lVar12 < plVar5[0x17]) {
LAB_140cee8e9:
                pcVar3 = *(code **)(*plVar5 + 0x90);
                if (pcVar3 == (code *)0x1406ce960) {
                  plVar11 = (longlong *)func_0x0001406ceac0(DAT_146185e80,1,0x8754a2e7d2341eb,0);
                  if (plVar11 != (longlong *)0x0) {
                    plVar15 = (longlong *)plVar5[0x6d];
                    plVar13 = (longlong *)plVar5[0x6e];
                    local_68 = plVar13;
                    if (plVar15 != plVar13) {
                      plVar14 = plVar15 + 1;
                      do {
                        if (*(code **)(*plVar11 + 0x10) == (code *)0x141519040) {
                          local_38 = plVar5[0x15];
                          func_0x0001438fb4bc(plVar11 + 0xc,&local_50,&local_38);
                          if (local_50 == (longlong *)plVar11[0xd]) {
                            uVar8 = 2;
                          }
                          else {
                            lVar10 = local_50[4];
                            for (lVar12 = local_50[3]; lVar12 != lVar10; lVar12 = lVar12 + 0x88) {
                              cVar6 = func_0x0001414f80c0(lVar12,plVar14);
                              if (cVar6 != '\0') {
                                if (*(longlong **)(lVar12 + 0x80) == (longlong *)0x0) {
                                  (*(code *)0x5bf56da)();
                                  pcVar3 = (code *)swi(3);
                                  (*pcVar3)();
                                  return;
                                }
                                cVar6 = (**(code **)(**(longlong **)(lVar12 + 0x80) + 0x10))();
                                if (cVar6 == '\0') {
                                  uVar8 = *(undefined4 *)(lVar12 + 0x40);
                                  plVar13 = local_68;
                                  goto LAB_140ceea0e;
                                }
                              }
                            }
                            uVar8 = 2;
                            plVar13 = local_68;
                          }
                        }
                        else {
                          uVar8 = (**(code **)(*plVar11 + 0x10))(plVar11,plVar5[0x15],plVar14);
                        }
LAB_140ceea0e:
                        *(undefined4 *)plVar15 = uVar8;
                        plVar15 = plVar15 + 0xc;
                        plVar14 = plVar14 + 0xc;
                      } while (plVar15 != plVar13);
                    }
                  }
                }
                else {
                  if (pcVar3 == (code *)0x1401da830) {
                    func_0x0001401da840();
                    cVar6 = '\x01';
                    goto LAB_140ceea5d;
                  }
                  (*pcVar3)(plVar5,&local_40,lVar12);
                }
                cVar6 = '\x01';
              }
              else {
                local_50 = (longlong *)0x0;
                lStack_48 = 0;
                func_0x0001404f9050(plVar5 + 0x13,&local_50);
                if ((local_50 == (longlong *)0x0) ||
                   (cVar6 = (**(code **)(*local_50 + 0x20))(), cVar6 == '\0')) {
                  plVar5[0x16] = 0;
                }
                else {
                  lVar10 = (**(code **)(*local_50 + 8))(local_50,plVar5 + 0x15);
                  plVar5[0x16] = lVar10;
                  if (lVar10 != 0) {
                    if (lStack_48 != 0) {
                      func_0x0001403a9cb0();
                    }
                    goto LAB_140cee8e9;
                  }
                }
                if (lStack_48 == 0) goto LAB_140cee7e9;
                func_0x0001403a9cb0();
                cVar6 = '\0';
              }
            }
            else {
              cVar6 = (*pcVar3)(plVar5,&local_40,lVar12);
            }
          }
LAB_140ceea5d:
          *(char *)((longlong)plVar5 + 0x61) = cVar6;
          if (cVar6 != '\0') {
            for (plVar11 = (longlong *)plVar5[0xf]; plVar11 != (longlong *)0x0;
                plVar11 = (longlong *)plVar11[5]) {
              (**(code **)(*plVar11 + 8))(plVar11);
            }
          }
          param_1 = local_60;
          if (local_40 != (longlong *)0x0) {
            LOCK();
            plVar11 = local_40 + 2;
            lVar12 = *plVar11;
            *(int *)plVar11 = (int)*plVar11 + -1;
            UNLOCK();
            if ((int)lVar12 == 1) {
              func_0x000140452990();
              param_1 = local_60;
            }
          }
        }
        cVar6 = *(char *)((longlong)plVar5 + 0x61);
      }
      else {
        cVar6 = (**(code **)(*local_58 + 0x68))(local_58);
      }
LAB_140ceeaad:
      if (cVar6 != '\0') {
        if (plVar5 != (longlong *)0x0) {
          LOCK();
          plVar11 = plVar5 + 2;
          lVar12 = *plVar11;
          *(int *)plVar11 = (int)*plVar11 + -1;
          UNLOCK();
          if ((int)lVar12 == 1) {
            func_0x000140452990(plVar5);
          }
        }
        goto LAB_140ceeacb;
      }
    }
    *(undefined1 *)(param_1 + 0xb1) = 0;
    if (plVar5 != (longlong *)0x0) {
      LOCK();
      plVar11 = plVar5 + 2;
      lVar12 = *plVar11;
      *(int *)plVar11 = (int)*plVar11 + -1;
      UNLOCK();
      if ((int)lVar12 == 1) {
        func_0x000140452990(plVar5);
      }
    }
  }
LAB_140ceeb18:
  func_0x0001422ba6e0(local_30 ^ (ulonglong)auStack_88);
  return;
}

