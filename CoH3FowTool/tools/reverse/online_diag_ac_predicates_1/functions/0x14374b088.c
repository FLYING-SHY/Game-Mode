// rea decompile_all
// name: FUN_14374b088
// address: 0x14374b088
// body: available
// range: 0x14374b088-0x14374b880 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14374b088(longlong param_1)

{
  longlong **pplVar1;
  uint uVar2;
  uint uVar3;
  ulonglong uVar4;
  uint *puVar5;
  int iVar6;
  undefined4 uVar7;
  longlong lVar8;
  longlong lVar9;
  longlong lVar10;
  longlong lVar11;
  undefined8 *puVar12;
  longlong *plVar13;
  ulonglong *puVar14;
  ulonglong *puVar15;
  ulonglong uVar16;
  longlong **pplVar17;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_5c8 [32];
  uint *local_5a8;
  ulonglong local_5a0;
  longlong *local_598;
  longlong *plStack_590;
  longlong *local_588;
  longlong *plStack_580;
  longlong *local_578;
  longlong *local_570;
  longlong *plStack_568;
  longlong *local_560;
  longlong *plStack_558;
  longlong *local_550;
  longlong *local_548;
  longlong *plStack_540;
  longlong *local_538;
  longlong *plStack_530;
  longlong *local_528;
  longlong *local_520;
  longlong *plStack_518;
  longlong *local_510;
  longlong *plStack_508;
  longlong *local_500;
  longlong *local_4f8;
  longlong *local_4f0;
  ulonglong local_4e8;
  undefined4 local_4e0;
  ulonglong local_4dc;
  undefined4 local_4d4;
  ulonglong local_4d0;
  undefined4 local_4c8;
  ulonglong local_4c4;
  undefined4 local_4bc;
  ulonglong local_4b8;
  undefined4 local_4b0;
  ulonglong local_4ac;
  undefined4 local_4a4;
  ulonglong local_4a0;
  undefined4 local_498;
  ulonglong local_494;
  undefined4 local_48c;
  ulonglong local_488;
  undefined4 local_480;
  longlong local_47c;
  undefined4 local_474;
  undefined4 local_470;
  longlong local_46c;
  undefined4 local_464;
  undefined4 local_460;
  undefined1 local_458 [1056];
  ulonglong local_38;
  
  local_38 = DAT_145df0fc0 ^ (ulonglong)auStack_5c8;
  local_4f8 = DAT_146b02cf0;
  lVar10 = *(longlong *)
            (&DAT_146185e00 +
            (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8);
  uVar2 = *(uint *)((ulonglong)DAT_145e0becc + 0xfc + lVar10);
  local_4f0 = (longlong *)(**(code **)(*DAT_146b02c78 + 0x110))();
  lVar10 = (ulonglong)uVar2 + lVar10;
  lVar8 = func_0x0001401943a0(lVar10 + 0xc);
  lVar9 = func_0x0001401943a0(lVar10 + 0xc);
  uVar2 = *(uint *)(lVar10 + 0x10);
  do {
    if (lVar8 == lVar9 + (ulonglong)uVar2 * 4) {
      func_0x0001422ba6e0(local_38 ^ (ulonglong)auStack_5c8);
      return;
    }
    lVar10 = func_0x0001401943a0(lVar8);
    uVar4 = *(ulonglong *)(lVar10 + 8);
    if ((int)uVar4 != *(int *)(param_1 + 0x558)) {
      uVar16 = (uVar4 >> 0x20) * 0x50;
      pplVar17 = (longlong **)(param_1 + 0x28 + uVar16);
      local_5a0 = uVar16;
      lVar11 = func_0x0001401e3f90(*local_4f8 + 0x1930,uVar4 & 0xffffffff);
      if (lVar11 == 0) {
        puVar12 = &DAT_14464da78;
      }
      else {
        puVar12 = (undefined8 *)(lVar11 + 0x18);
      }
      local_5a8 = (uint *)*puVar12;
      if (local_5a8 != (uint *)0xffffffffffffffff) {
        func_0x000140b97a10(local_458);
        iVar6 = (**(code **)(*local_4f0 + 0x20))(local_4f0,local_5a8,local_458);
        if (iVar6 == 2) {
          local_598 = (longlong *)0x0;
          plStack_590 = (longlong *)0x0;
          local_588 = (longlong *)0x0;
          plStack_580 = (longlong *)0x0;
          local_578 = (longlong *)0x0;
          local_598 = (longlong *)func_0x0001422ba9a8(0x10);
          local_598[1] = 0;
          *local_598 = (longlong)&local_598;
          pplVar1 = pplVar17 + 5;
          if (pplVar1 != &local_598) {
            func_0x0001409f94c0(pplVar1);
            plVar13 = *pplVar1;
            *pplVar1 = local_598;
            if (local_598 != (longlong *)0x0) {
              *local_598 = (longlong)pplVar1;
            }
            if (plVar13 != (longlong *)0x0) {
              *plVar13 = (longlong)&local_598;
            }
            pplVar17[6] = plStack_590;
            pplVar17[7] = local_588;
            pplVar17[8] = plStack_580;
            pplVar17[9] = local_578;
            plStack_590 = (longlong *)0x0;
            local_588 = (longlong *)0x0;
            plStack_580 = (longlong *)0x0;
            local_578 = (longlong *)0x0;
            local_598 = plVar13;
          }
          func_0x0001409f9490(&local_598);
          local_570 = (longlong *)0x0;
          plStack_568 = (longlong *)0x0;
          local_560 = (longlong *)0x0;
          plStack_558 = (longlong *)0x0;
          local_550 = (longlong *)0x0;
          local_570 = (longlong *)func_0x0001422ba9a8(0x10);
          local_570[1] = 0;
          *local_570 = (longlong)&local_570;
          if (pplVar17 != &local_570) {
            func_0x0001409f94c0(pplVar17);
            plVar13 = *pplVar17;
            *pplVar17 = local_570;
            if (local_570 != (longlong *)0x0) {
              *local_570 = (longlong)pplVar17;
            }
            if (plVar13 != (longlong *)0x0) {
              *plVar13 = (longlong)&local_570;
            }
            pplVar17[1] = plStack_568;
            pplVar17[2] = local_560;
            pplVar17[3] = plStack_558;
            pplVar17[4] = local_550;
            plStack_568 = (longlong *)0x0;
            local_560 = (longlong *)0x0;
            plStack_558 = (longlong *)0x0;
            local_550 = (longlong *)0x0;
            local_570 = plVar13;
          }
          func_0x0001409f9490(&local_570);
        }
        else {
          if (*(char *)(lVar10 + 0x692) != '\0') {
            if (*(char *)(lVar10 + 0x692) == '\x02') {
              local_4e0 = 2;
              puVar14 = *(ulonglong **)(param_1 + 0x548);
              local_4e8 = uVar4;
              if (puVar14 == *(ulonglong **)(param_1 + 0x550)) {
                func_0x000142b371b0(param_1 + 0x540,puVar14,&local_4e8);
              }
              else {
                *puVar14 = uVar4;
                *(undefined4 *)(puVar14 + 1) = 2;
                *(longlong *)(param_1 + 0x548) = *(longlong *)(param_1 + 0x548) + 0xc;
              }
            }
            else {
              local_548 = (longlong *)0x0;
              plStack_540 = (longlong *)0x0;
              local_538 = (longlong *)0x0;
              plStack_530 = (longlong *)0x0;
              local_528 = (longlong *)0x0;
              local_548 = (longlong *)func_0x0001422ba9a8(0x10);
              local_548[1] = 0;
              *local_548 = (longlong)&local_548;
              pplVar1 = pplVar17 + 5;
              if (pplVar1 != &local_548) {
                while (pplVar17[9] != (longlong *)0x0) {
                  plVar13 = (longlong *)((longlong)pplVar17[9] + -1);
                  pplVar17[9] = plVar13;
                  if (plVar13 == (longlong *)0x0) {
                    pplVar17[8] = (longlong *)0x0;
                  }
                }
                if (pplVar17[6] != (longlong *)0x0) {
                  plVar13 = pplVar17[7];
                  while (0 < (longlong)plVar13) {
                    plVar13 = (longlong *)((longlong)plVar13 + -1);
                    if (pplVar17[6][(longlong)plVar13] != 0) {
                      func_0x00014031b310(pplVar17[6][(longlong)plVar13],0x10);
                    }
                  }
                  func_0x00014031b310(pplVar17[6],(longlong)pplVar17[7] << 3);
                  uVar16 = local_5a0;
                }
                plVar13 = *pplVar1;
                *pplVar1 = local_548;
                if (local_548 != (longlong *)0x0) {
                  *local_548 = (longlong)pplVar1;
                }
                if (plVar13 != (longlong *)0x0) {
                  *plVar13 = (longlong)&local_548;
                }
                pplVar17[6] = plStack_540;
                pplVar17[7] = local_538;
                pplVar17[8] = plStack_530;
                pplVar17[9] = local_528;
                plStack_540 = (longlong *)0x0;
                local_538 = (longlong *)0x0;
                plStack_530 = (longlong *)0x0;
                local_528 = (longlong *)0x0;
                local_548 = plVar13;
              }
              func_0x0001409f9490(&local_548);
              local_520 = (longlong *)0x0;
              plStack_518 = (longlong *)0x0;
              local_510 = (longlong *)0x0;
              plStack_508 = (longlong *)0x0;
              local_500 = (longlong *)0x0;
              local_520 = (longlong *)func_0x0001422ba9a8(0x10);
              local_520[1] = 0;
              *local_520 = (longlong)&local_520;
              if (pplVar17 != &local_520) {
                while (pplVar17[4] != (longlong *)0x0) {
                  plVar13 = (longlong *)((longlong)pplVar17[4] + -1);
                  pplVar17[4] = plVar13;
                  if (plVar13 == (longlong *)0x0) {
                    pplVar17[3] = (longlong *)0x0;
                  }
                }
                if (pplVar17[1] != (longlong *)0x0) {
                  plVar13 = pplVar17[2];
                  while (0 < (longlong)plVar13) {
                    plVar13 = (longlong *)((longlong)plVar13 + -1);
                    if (pplVar17[1][(longlong)plVar13] != 0) {
                      func_0x00014031b310(pplVar17[1][(longlong)plVar13],0x10);
                    }
                  }
                  func_0x00014031b310(pplVar17[1],(longlong)pplVar17[2] << 3);
                }
                plVar13 = *pplVar17;
                *pplVar17 = local_520;
                if (local_520 != (longlong *)0x0) {
                  *local_520 = (longlong)pplVar17;
                }
                if (plVar13 != (longlong *)0x0) {
                  *plVar13 = (longlong)&local_520;
                }
                pplVar17[1] = plStack_518;
                pplVar17[2] = local_510;
                pplVar17[3] = plStack_508;
                pplVar17[4] = local_500;
                plStack_518 = (longlong *)0x0;
                local_510 = (longlong *)0x0;
                plStack_508 = (longlong *)0x0;
                local_500 = (longlong *)0x0;
                local_520 = plVar13;
              }
              func_0x0001409f9490(&local_520);
            }
          }
          lVar10 = *(longlong *)(uVar16 + 0x70 + param_1);
          plVar13 = pplVar17[4];
          if (lVar10 == 0) {
            if (0x5a0 < (ulonglong)((longlong)plVar13 << 7)) {
              local_4d4 = 0x400;
              puVar14 = *(ulonglong **)(param_1 + 0x548);
              local_4dc = uVar4;
              if (puVar14 == *(ulonglong **)(param_1 + 0x550)) {
                puVar15 = &local_4dc;
                goto LAB_14374b6c6;
              }
              *puVar14 = uVar4;
              *(undefined4 *)(puVar14 + 1) = 0x400;
LAB_14374b5d9:
              *(longlong *)(param_1 + 0x548) = *(longlong *)(param_1 + 0x548) + 0xc;
            }
          }
          else if (plVar13 != (longlong *)0x0) {
            local_5a0 = *(ulonglong *)(uVar16 + 0x68 + param_1);
            puVar5 = *(uint **)(*(longlong *)(uVar16 + 0x58 + param_1) +
                               (*(longlong *)(uVar16 + 0x60 + param_1) - 1U & local_5a0) * 8);
            local_5a8 = (uint *)pplVar17[1][(longlong)pplVar17[2] - 1U & (ulonglong)pplVar17[3]];
            uVar3 = *local_5a8;
            if ((int)uVar3 < (int)*puVar5) {
              pplVar17[4] = (longlong *)((longlong)plVar13 + -1);
              if ((longlong *)((longlong)plVar13 + -1) == (longlong *)0x0) {
                pplVar17[3] = (longlong *)0x0;
              }
              else {
                pplVar17[3] = (longlong *)((longlong)pplVar17[3] + 1);
              }
              local_4c8 = 8;
              puVar14 = *(ulonglong **)(param_1 + 0x548);
              local_4d0 = uVar4;
              if (puVar14 != *(ulonglong **)(param_1 + 0x550)) {
LAB_14374b66d:
                *puVar14 = uVar4;
                *(undefined4 *)(puVar14 + 1) = 8;
                goto LAB_14374b5d9;
              }
              puVar15 = &local_4d0;
            }
            else {
              if ((int)uVar3 <= (int)*puVar5) {
                local_474 = *(undefined4 *)(DAT_146a97ae0 + 4);
                local_470 = 0;
                LOCK();
                local_47c = (ulonglong)uVar3 << 0x20;
                UNLOCK();
                local_464 = *(undefined4 *)(DAT_146a97ae0 + 4);
                local_460 = 0;
                LOCK();
                local_46c = (ulonglong)*puVar5 << 0x20;
                UNLOCK();
                if ((float)local_5a8[1] < (float)puVar5[1]) {
                  local_4b0 = 0x80;
                  puVar14 = *(ulonglong **)(param_1 + 0x548);
                  local_4b8 = uVar4;
                  if (puVar14 == *(ulonglong **)(param_1 + 0x550)) {
                    func_0x000142b371b0(param_1 + 0x540,puVar14,&local_4b8,0);
                  }
                  else {
                    *puVar14 = uVar4;
                    *(undefined4 *)(puVar14 + 1) = 0x80;
                    *(longlong *)(param_1 + 0x548) = *(longlong *)(param_1 + 0x548) + 0xc;
                  }
                }
                if (puVar5[2] < local_5a8[2]) {
                  uVar7 = 0x100;
                  local_4a4 = 0x100;
                  puVar14 = *(ulonglong **)(param_1 + 0x548);
                  local_4ac = uVar4;
                  if (puVar14 == *(ulonglong **)(param_1 + 0x550)) {
                    puVar15 = &local_4ac;
LAB_14374b7a6:
                    func_0x000142b371b0(param_1 + 0x540,puVar14,puVar15);
                  }
                  else {
LAB_14374b795:
                    *puVar14 = uVar4;
                    *(undefined4 *)(puVar14 + 1) = uVar7;
                    *(longlong *)(param_1 + 0x548) = *(longlong *)(param_1 + 0x548) + 0xc;
                  }
                }
                else if (local_5a8[2] < puVar5[2]) {
                  uVar7 = 0x800;
                  local_498 = 0x800;
                  puVar14 = *(ulonglong **)(param_1 + 0x548);
                  local_4a0 = uVar4;
                  if (puVar14 != *(ulonglong **)(param_1 + 0x550)) goto LAB_14374b795;
                  puVar15 = &local_4a0;
                  goto LAB_14374b7a6;
                }
                if (puVar5[3] < local_5a8[3]) {
                  uVar7 = 0x200;
                  local_48c = 0x200;
                  puVar14 = *(ulonglong **)(param_1 + 0x548);
                  local_494 = uVar4;
                  if (puVar14 == *(ulonglong **)(param_1 + 0x550)) {
                    puVar15 = &local_494;
LAB_14374b80d:
                    func_0x000142b371b0(param_1 + 0x540,puVar14,puVar15);
                  }
                  else {
LAB_14374b7fc:
                    *puVar14 = uVar4;
                    *(undefined4 *)(puVar14 + 1) = uVar7;
                    *(longlong *)(param_1 + 0x548) = *(longlong *)(param_1 + 0x548) + 0xc;
                  }
                }
                else if (local_5a8[3] < puVar5[3]) {
                  uVar7 = 0x1000;
                  local_480 = 0x1000;
                  puVar14 = *(ulonglong **)(param_1 + 0x548);
                  local_488 = uVar4;
                  if (puVar14 != *(ulonglong **)(param_1 + 0x550)) goto LAB_14374b7fc;
                  puVar15 = &local_488;
                  goto LAB_14374b80d;
                }
                plVar13 = (longlong *)(uVar16 + 0x70 + param_1);
                *plVar13 = *plVar13 + -1;
                if (*plVar13 == 0) {
                  *(undefined8 *)(uVar16 + 0x68 + param_1) = 0;
                }
                else {
                  plVar13 = (longlong *)(uVar16 + 0x68 + param_1);
                  *plVar13 = *plVar13 + 1;
                }
                pplVar1 = pplVar17 + 4;
                *pplVar1 = (longlong *)((longlong)*pplVar1 + -1);
                if (*pplVar1 == (longlong *)0x0) {
                  pplVar17[3] = (longlong *)0x0;
                }
                else {
                  pplVar17[3] = (longlong *)((longlong)pplVar17[3] + 1);
                }
                goto LAB_14374b83f;
              }
              lVar10 = lVar10 + -1;
              *(longlong *)(uVar16 + 0x70 + param_1) = lVar10;
              if (lVar10 == 0) {
                *(undefined8 *)(uVar16 + 0x68 + param_1) = 0;
              }
              else {
                *(ulonglong *)(uVar16 + 0x68 + param_1) = local_5a0 + 1;
              }
              local_4bc = 8;
              puVar14 = *(ulonglong **)(param_1 + 0x548);
              local_4c4 = uVar4;
              if (puVar14 != *(ulonglong **)(param_1 + 0x550)) goto LAB_14374b66d;
              puVar15 = &local_4c4;
            }
LAB_14374b6c6:
            func_0x000142b371b0(param_1 + 0x540,puVar14,puVar15);
          }
        }
LAB_14374b83f:
        func_0x000140b97140(local_458);
      }
    }
    lVar8 = lVar8 + 4;
  } while( true );
}

