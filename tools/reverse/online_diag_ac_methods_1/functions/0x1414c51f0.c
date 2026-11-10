// rea decompile_all
// name: FUN_1414c51f0
// address: 0x1414c51f0
// body: available
// range: 0x1414c51f0-0x1414c547b inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_1414c51f0(longlong param_1)

{
  undefined4 *puVar1;
  undefined4 uVar2;
  undefined4 uVar3;
  ulonglong uVar4;
  longlong lVar5;
  char cVar6;
  int iVar7;
  longlong *plVar8;
  longlong *plVar9;
  undefined8 uVar10;
  longlong *plVar11;
  uint uVar12;
  int iVar13;
  undefined1 auStack_138 [32];
  undefined1 local_118;
  undefined4 local_117;
  undefined4 uStack_113;
  int local_10c;
  longlong *local_108;
  undefined8 *local_100;
  undefined8 local_f8;
  undefined4 uStack_f0;
  undefined4 uStack_ec;
  undefined1 local_e8 [32];
  ulonglong local_c8 [2];
  undefined1 local_b8 [128];
  ulonglong local_38;
  
  local_38 = DAT_145df0fc0 ^ (ulonglong)auStack_138;
  plVar8 = (longlong *)(*_DAT_143d56540)(0x145dfe350);
  plVar8 = (longlong *)*plVar8;
  if (plVar8 != (longlong *)0x0) {
    iVar7 = (**(code **)(*plVar8 + 0x90))(plVar8);
    iVar13 = 0;
    if (0 < iVar7) {
      do {
        (**(code **)(*plVar8 + 0x98))(plVar8,local_c8,iVar13);
        for (plVar11 = *(longlong **)(param_1 + 0x60); plVar11 != *(longlong **)(param_1 + 0x68);
            plVar11 = plVar11 + 1) {
          local_117 = *(undefined4 *)(*plVar11 + 0x80);
          uStack_113 = CONCAT13(1,(undefined3)uStack_113);
          uVar12 = uStack_113 & 0xff800001 | 0x800001;
          uVar4 = CONCAT44(uStack_113,local_117);
          uStack_113 = uVar12;
          if ((uVar4 & 0xff800001ffffffff | 0x80000100000000) == local_c8[0]) goto LAB_1414c5440;
        }
        plVar9 = (longlong *)func_0x000143828afc(&local_100);
        plVar11 = (longlong *)*plVar9;
        *plVar9 = 0;
        local_108 = plVar11;
        if (local_100 != (undefined8 *)0x0) {
          (**(code **)*local_100)(local_100,1);
        }
        local_f8 = local_c8[0];
        uVar4 = local_f8;
        uStack_f0 = 2;
        local_f8._0_4_ = (undefined4)local_c8[0];
        local_f8._4_4_ = (undefined4)(local_c8[0] >> 0x20);
        *(undefined4 *)(plVar11 + 0x10) = (undefined4)local_f8;
        *(undefined4 *)((longlong)plVar11 + 0x84) = local_f8._4_4_;
        *(undefined4 *)(plVar11 + 0x11) = 2;
        *(undefined4 *)((longlong)plVar11 + 0x8c) = uStack_ec;
        local_f8 = uVar4;
        uVar10 = (**(code **)(*plVar8 + 0xa0))(plVar8,local_c8[0]);
        uVar10 = func_0x000140481a50(local_e8,uVar10);
        func_0x0001403064f0(plVar11 + 0x12,uVar10);
        func_0x000140311290(local_e8);
        *(undefined4 *)(plVar11 + 0x16) = 2;
        *(undefined4 *)(plVar11 + 0x17) = 0x3f;
        (**(code **)(*plVar11 + 8))(plVar11);
        func_0x000140802680(local_b8,0x1447f0ee8,plVar11[0x10]);
        func_0x000140af8100(&local_118,0);
        cVar6 = func_0x0001404a5fd0(*(undefined8 *)(_DAT_146b031e8 + 8),local_b8,&local_10c);
        if ((cVar6 != '\0') && (local_10c == 1)) {
          puVar1 = *(undefined4 **)(param_1 + 0x98);
          if (puVar1 == *(undefined4 **)(param_1 + 0xa0)) {
            func_0x000140a05840(param_1 + 0x90,puVar1,plVar11 + 0x10);
          }
          else {
            uVar2 = *(undefined4 *)((longlong)plVar11 + 0x84);
            lVar5 = plVar11[0x11];
            uVar3 = *(undefined4 *)((longlong)plVar11 + 0x8c);
            *puVar1 = (int)plVar11[0x10];
            puVar1[1] = uVar2;
            puVar1[2] = (int)lVar5;
            puVar1[3] = uVar3;
            *(longlong *)(param_1 + 0x98) = *(longlong *)(param_1 + 0x98) + 0x10;
          }
        }
        plVar9 = *(longlong **)(param_1 + 0x68);
        if (plVar9 == *(longlong **)(param_1 + 0x70)) {
          func_0x0001413e58c0(param_1 + 0x60,plVar9,&local_108);
        }
        else {
          local_108 = (longlong *)0x0;
          *plVar9 = (longlong)plVar11;
          *(longlong *)(param_1 + 0x68) = *(longlong *)(param_1 + 0x68) + 8;
        }
        plVar11 = local_108;
        func_0x0001404a5ed0(*(undefined8 *)(_DAT_146b031e8 + 8),0x143d76528);
        if (plVar11 != (longlong *)0x0) {
          (**(code **)*plVar11)(plVar11,1);
        }
LAB_1414c5440:
        iVar13 = iVar13 + 1;
      } while (iVar13 < iVar7);
    }
  }
  func_0x0001422ba6e0(local_38 ^ (ulonglong)auStack_138);
  return;
}

