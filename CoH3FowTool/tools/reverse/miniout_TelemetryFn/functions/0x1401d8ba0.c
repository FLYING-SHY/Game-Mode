// rea decompile_all
// name: entry
// address: 0x1401d8ba0
// body: available
// range: 0x1401d8ba0-0x1401d93d4 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b537ee6fdad293e1c8ea50e7b50d0b6b22972a83032bca093d279ab7e770ce4
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Removing unreachable block (ram,0x0001426c4619) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void entry(longlong param_1,longlong *param_2,longlong param_3,longlong *param_4,undefined8 param_5,
          undefined8 param_6,undefined4 *param_7)

{
  longlong *plVar1;
  longlong *plVar2;
  char *pcVar3;
  ulonglong uVar4;
  char cVar5;
  longlong lVar6;
  undefined8 uVar7;
  longlong lVar8;
  longlong lVar9;
  undefined8 *puVar10;
  longlong lVar11;
  longlong *plVar12;
  undefined *puVar13;
  undefined8 ***pppuVar14;
  ulonglong uVar15;
  undefined1 auStackY_1c8 [32];
  undefined4 *puVar16;
  undefined8 uVar17;
  undefined4 *in_stack_fffffffffffffe68;
  undefined1 local_188;
  int local_180;
  int local_17c;
  undefined8 local_178;
  longlong *local_170;
  longlong *local_168;
  longlong local_160;
  longlong local_158;
  char *local_150;
  ulonglong local_148;
  longlong local_138;
  undefined8 local_130;
  undefined1 local_128 [32];
  undefined1 local_108 [32];
  undefined4 local_e8 [8];
  undefined1 local_c8 [32];
  undefined1 local_a8 [32];
  undefined1 local_88 [32];
  undefined8 **local_68 [3];
  ulonglong local_50;
  ulonglong local_48;
  
  local_48 = _DAT_145df0fc0 ^ (ulonglong)auStackY_1c8;
  if (*(longlong *)(*_DAT_146b02c78 + 0x120) != 0x1414fc080) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  plVar1 = (longlong *)_DAT_146b02c78[0x2e8];
  if (*(longlong *)(*plVar1 + 0x28) != 0x140d62560) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  local_168 = param_2;
  local_160 = param_3;
  local_158 = param_1;
  lVar6 = func_0x00014019edd0();
  lVar6 = lVar6 + 0xd68;
  local_148 = lVar6;
  func_0x0001409b7680(lVar6);
  if ((ulonglong)*(uint *)((longlong)plVar1 + 500) <= (ulonglong)(plVar1[0x11] - plVar1[0x10] >> 3))
  {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  local_150 = (char *)func_0x0001422ba9a8(0x58);
  local_170 = (longlong *)func_0x000140d62630(local_150,"match_start");
  puVar10 = (undefined8 *)plVar1[0x11];
  if (puVar10 == (undefined8 *)plVar1[0x12]) {
    func_0x0001403ef410(plVar1 + 0x10,puVar10,&local_170);
  }
  else {
    *puVar10 = local_170;
    plVar1[0x11] = plVar1[0x11] + 8;
  }
  plVar1 = local_170;
  (*_DAT_143d54e48)(lVar6);
  lVar11 = local_160;
  plVar2 = local_168;
  lVar6 = *(longlong *)(_DAT_146a8ca18 + 0x1c0);
  local_170 = (longlong *)0x0;
  if (lVar6 != 0) {
    local_170 = *(longlong **)(lVar6 + 0x148);
  }
  uVar17 = 0;
  puVar16 = param_7;
  func_0x0001408017f0(local_158,local_168,plVar1,local_170);
  func_0x000140800b60(plVar2,&local_180,*param_7);
  func_0x0001408020e0(plVar1,&DAT_143ede79c,local_180);
  puVar13 = &DAT_143d98b60;
  if (*(char *)(*plVar2 + 0x2bac) != '\0') {
    puVar13 = &DAT_143d98a9c;
  }
  uVar15 = (ulonglong)puVar16 & 0xffffffffffffff00;
  func_0x000140328b40(plVar1,"is_host",puVar13,&DAT_143d98b5c);
  uVar7 = func_0x000140371d80(0x146a8ca30);
  func_0x000140802800(plVar1,"party_id",uVar7);
  cVar5 = func_0x000140802ef0(param_7);
  puVar13 = &DAT_143d98a9c;
  if (cVar5 == '\0') {
    puVar13 = &DAT_143d98b60;
  }
  uVar15 = uVar15 & 0xffffffffffffff00;
  func_0x000140328b40(plVar1,"is_ai",puVar13,&DAT_143d98b5c);
  cVar5 = func_0x0001401e3ec0();
  if (cVar5 != '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  uVar15 = uVar15 & 0xffffffffffffff00;
  func_0x000140328b40(plVar1,"campaign_initiated_skirmish_or_mission",&DAT_143d98b60,&DAT_143d98b5c)
  ;
  lVar8 = func_0x00014024a520(*(undefined8 *)(*plVar2 + 0x2b78));
  if ((lVar8 != 0) && ((*(uint *)(lVar8 + 0x30) >> 0x13 & 1) != 0)) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  func_0x0001408020e0(plVar1,"game_difficulty",*(undefined4 *)*plVar2);
  func_0x0001408020e0(plVar1,"player_count",*(undefined4 *)(*plVar2 + 0x18));
  uVar7 = func_0x000140248900(plVar2);
  func_0x0001408020e0(plVar1,"player_count_human",uVar7);
  func_0x0001408020e0(plVar1,"game_type",*(undefined1 *)(_DAT_146a97ae0 + 1));
  lVar8 = local_158;
  func_0x000140802800(plVar1,"map_load_time",*(undefined8 *)(*(longlong *)(local_158 + 8) + 0xe10));
  if (0xf < *(ulonglong *)(*(longlong *)(lVar8 + 8) + 0xa68)) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  func_0x000140802250(plVar1,"battle_server",*(longlong *)(lVar8 + 8) + 0xa50,0x100);
  puVar13 = &DAT_143d98b60;
  if (*(char *)(*(longlong *)(lVar8 + 8) + 0xa70) != '\0') {
    puVar13 = &DAT_143d98a9c;
  }
  uVar15 = uVar15 & 0xffffffffffffff00;
  func_0x000140328b40(plVar1,"is_default_battle_server",puVar13,&DAT_143d98b5c);
  lVar9 = func_0x0001401e3f90(*plVar2 + 0x1930,*param_7);
  if (lVar9 == 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  func_0x000140800bb0();
  func_0x000140955b30();
  func_0x0001408020e0(plVar1,"map_x_dimension",*(undefined4 *)(*(longlong *)(lVar8 + 8) + 0xa00));
  puVar13 = (undefined *)(ulonglong)*(uint *)(*(longlong *)(lVar8 + 8) + 0xa04);
  func_0x0001408020e0(plVar1,"map_y_dimension");
  plVar12 = local_170;
  if ((lVar6 != 0) && (local_170 != (longlong *)0x0)) {
    puVar13 = &DAT_143d98b60;
    if (*(char *)(*local_170 + 0x38) != '\0') {
      puVar13 = &DAT_143d98a9c;
    }
    uVar15 = uVar15 & 0xffffffffffffff00;
    func_0x000140328b40(plVar1,"used_cheats",puVar13,&DAT_143d98b5c);
    puVar13 = &DAT_143d98b60;
    if (*(char *)(*plVar12 + 0x39) != '\0') {
      puVar13 = &DAT_143d98a9c;
    }
    uVar15 = uVar15 & 0xffffffffffffff00;
    param_7 = (undefined4 *)&DAT_143d98b5c;
    func_0x000140328b40(plVar1,"debug_used");
  }
  func_0x00014030dcf0(local_68,&DAT_143dd9010);
  plVar12 = (longlong *)0x0;
  if (param_4 != (longlong *)0x0) {
    do {
      if ((longlong *)0x10 < plVar12) break;
      func_0x00014053ccb0(local_e8,*(undefined4 *)(lVar11 + (longlong)plVar12 * 4));
      FUN_1401d82d0((longlong)local_108,plVar12,(longlong)puVar13,(ulonglong)param_7,uVar15,uVar17,
                    in_stack_fffffffffffffe68);
      uVar7 = func_0x0001401d9520(local_88,&DAT_143d80cfc,local_108);
      uVar7 = func_0x00014032aaa0(local_a8,uVar7,&DAT_143d6f76c);
      param_7 = local_e8;
      func_0x0001401d9ae0(local_128,local_188,uVar7);
      puVar10 = (undefined8 *)func_0x00014032aaa0(local_c8,local_128,&DAT_143d80d00);
      puVar13 = (undefined *)puVar10[2];
      if (0xf < (ulonglong)puVar10[3]) {
        puVar10 = (undefined8 *)*puVar10;
      }
      func_0x000140328d20(local_68,puVar10);
      func_0x000140311020(local_c8);
      func_0x000140311020(local_128);
      func_0x000140311020(local_a8);
      func_0x000140311020(local_88);
      func_0x000140311020(local_108);
      func_0x000140311020(local_e8);
      if (plVar12 != (longlong *)((longlong)param_4 + -1)) {
        puVar13 = (undefined *)0x1;
        func_0x000140328d20(local_68,&DAT_143d80de0);
      }
      plVar12 = (longlong *)((longlong)plVar12 + 1);
    } while (plVar12 < param_4);
  }
  func_0x000140328d20(local_68,&DAT_143dd9014,1);
  pppuVar14 = local_68;
  if (0xf < local_50) {
    pppuVar14 = (undefined8 ***)local_68[0];
  }
  func_0x000140802250(plVar1,"team_human_count",pppuVar14,0x100);
  if (*(int *)(*(longlong *)(*plVar2 + 0x2b20) + 8) == 1) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  func_0x0001406e6480(plVar1);
  if (*(int *)(*(longlong *)(*plVar2 + 0x2b20) + 0x2f0) == 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  uVar15 = 0;
  local_148 = *(ulonglong *)(*plVar2 + 0x18);
  if (local_148 != 0) {
    lVar6 = 0;
    do {
      lVar11 = *(longlong *)(*local_168 + 0x10);
      if (lVar11 == 0) {
        lVar11 = *local_168 + 0x28;
      }
      local_150 = (char *)(lVar11 + lVar6);
      if (*(longlong *)(*_DAT_146b02c78 + 0x120) != 0x1414fc080) {
                    /* WARNING: Bad instruction - Truncating control flow here */
        halt_baddata();
      }
      plVar1 = (longlong *)_DAT_146b02c78[0x2e8];
      if (*(longlong *)(*plVar1 + 0x28) != 0x140d62560) {
                    /* WARNING: Bad instruction - Truncating control flow here */
        halt_baddata();
      }
      lVar11 = func_0x00014019edd0();
      lVar11 = lVar11 + 0xd68;
      local_138 = lVar11;
      func_0x0001409b7680(lVar11);
      if ((ulonglong)*(uint *)((longlong)plVar1 + 500) <=
          (ulonglong)(plVar1[0x11] - plVar1[0x10] >> 3)) {
                    /* WARNING: Bad instruction - Truncating control flow here */
        halt_baddata();
      }
      local_130 = func_0x0001422ba9a8(0x58);
      local_178 = func_0x000140d62630(local_130,"matchstart_player");
      puVar10 = (undefined8 *)plVar1[0x11];
      if (puVar10 == (undefined8 *)plVar1[0x12]) {
        func_0x0001403ef410(plVar1 + 0x10,puVar10,&local_178);
      }
      else {
        *puVar10 = local_178;
        plVar1[0x11] = plVar1[0x11] + 8;
      }
      uVar17 = local_178;
      (*_DAT_143d54e48)(lVar11);
      uVar4 = local_148;
      pcVar3 = local_150;
      local_180 = *(int *)(local_150 + 0x38);
      local_17c = 0;
      if (local_180 != 0) {
        local_17c = local_180 + -1000;
      }
      func_0x0001408017f0(local_158,local_168,uVar17,local_170);
      func_0x0001408020e0(uVar17,&DAT_143d8ae98,uVar15 & 0xffffffff);
      func_0x0001408020e0(uVar17,&DAT_143ede79c,*(undefined4 *)(pcVar3 + 0x30));
      uVar7 = func_0x000140e15a50(pcVar3 + 0x3c);
      func_0x0001408020e0(uVar17,&DAT_143de7400,uVar7);
      puVar13 = &DAT_143d98a9c;
      if (*pcVar3 != '\0') {
        puVar13 = &DAT_143d98b60;
      }
      func_0x000140328b40(uVar17,"is_ai",puVar13,&DAT_143d98b5c);
      func_0x0001408020e0(uVar17,"ai_difficulty_id",*(undefined4 *)(pcVar3 + 0x44));
      func_0x0001408020e0(uVar17,"elorating",*(undefined4 *)(pcVar3 + 0xb0));
      func_0x0001406e6480(uVar17);
      uVar15 = uVar15 + 1;
      lVar6 = lVar6 + 400;
    } while (uVar15 < uVar4);
  }
  func_0x000140311020(local_68);
  func_0x0001422ba6e0(local_48 ^ (ulonglong)auStackY_1c8);
  return;
}

