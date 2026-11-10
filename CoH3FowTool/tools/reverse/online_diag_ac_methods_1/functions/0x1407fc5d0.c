// rea decompile_all
// name: FUN_1407fc5d0
// address: 0x1407fc5d0
// body: available
// range: 0x1407fc5d0-0x1407fcaf2 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Removing unreachable block (ram,0x0001407fcada) */
/* WARNING: Removing unreachable block (ram,0x000142803be7) */

void FUN_1407fc5d0(uint *param_1,longlong param_2)

{
  uint *puVar1;
  char cVar2;
  undefined8 *puVar3;
  longlong lVar4;
  longlong lVar5;
  uint uVar6;
  int iVar7;
  uint uVar8;
  ulonglong uVar9;
  uint *puVar10;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_568 [32];
  byte *local_548;
  uint local_538;
  undefined8 local_534;
  int local_52c;
  uint local_528;
  undefined8 *local_520;
  undefined8 local_518;
  undefined4 local_510;
  uint *local_508;
  byte local_500;
  byte local_4ff;
  undefined1 local_4fe;
  char local_4fd;
  ushort local_4fc [2];
  undefined8 *local_4f8;
  ulonglong uStack_4f0;
  uint local_4e8 [4];
  undefined4 local_4d8;
  undefined4 local_4d4;
  undefined1 local_4d0;
  undefined2 local_4cc;
  undefined8 local_4c8;
  undefined8 local_4b8;
  undefined4 local_4b0;
  undefined2 local_4ac;
  undefined8 local_2a0;
  undefined8 local_298;
  undefined8 local_290;
  undefined2 local_288;
  undefined1 local_286;
  undefined8 local_280;
  undefined8 local_278;
  undefined4 local_270;
  undefined2 local_26c;
  undefined8 local_60;
  undefined8 local_58;
  undefined8 local_50;
  undefined2 local_48;
  undefined1 local_46;
  undefined8 local_40;
  ulonglong local_38;
  
  local_38 = DAT_145df0fc0 ^ (ulonglong)auStack_568;
  local_510 = 0;
  local_4fc[0] = 0;
  local_508 = param_1;
  func_0x0001422bddb5(&local_4b8,0,0x240);
  local_4b0 = 0;
  local_4ac = 0;
  local_4b8 = 0x143d7c6d0;
  local_2a0 = 0;
  local_298 = 0;
  local_290 = 0;
  local_288 = 0;
  local_286 = 0;
  local_280 = 0;
  FUN_1407fcbd0(&local_4b8,param_2);
  local_4f8 = &local_4b8;
  uStack_4f0 = 0;
  func_0x000140c9a790(&local_4f8,local_4fc,1);
  if (local_4fc[0] < 2) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  func_0x0001404819c0(&local_4b8);
  func_0x0001422bddb5(&local_278,0,0x240);
  local_270 = 0;
  local_26c = 0;
  local_278 = 0x143d7c6d0;
  local_60 = 0;
  local_58 = 0;
  local_50 = 0;
  local_48 = 0;
  local_46 = 0;
  local_40 = 0;
  local_548 = (byte *)CONCAT44(local_548._4_4_,1);
  local_4fe = 0;
  func_0x000140480cd0(&local_278,param_2 + 2,(ulonglong)local_4fc[0] - 2);
  local_520 = &local_278;
  local_518 = 0;
  func_0x000140664130(&local_520,&local_4fe,1);
  local_4ff = 0;
  func_0x000140664130(&local_520,&local_4ff,1);
  if ((char)local_4ff < '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  uVar6 = local_4ff & 0x7f;
  if (uVar6 == 0x7f) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  iVar7 = uVar6 + 1000;
  local_4d8 = 0;
  local_4d4 = 0xffffffff;
  local_4d0 = 0xff;
  local_4cc = 0;
  local_4c8 = 0;
  func_0x0001406d5a50(&local_520,&local_4d8,0x18);
  local_4e8[0] = 0xffffffff;
  local_4e8[1] = 0;
  local_4e8[2] = 0;
  FUN_1407fcc00(&local_520,local_4e8);
  local_500 = 0;
  local_528 = 0;
  if (iVar7 != 0) {
    local_528 = uVar6;
  }
  local_548 = &local_500;
  local_52c = iVar7;
  FUN_1407fcb00(param_1,&local_4fe,&local_52c,local_4e8);
  local_510 = 1;
  puVar10 = (uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
  lVar5 = *(longlong *)(&DAT_146185e00 + (ulonglong)*puVar10 * 8);
  lVar4 = func_0x00014037e2c0((ulonglong)*(uint *)((ulonglong)DAT_145e0becc + 0xfc + lVar5) + lVar5,
                              iVar7);
  if ((lVar4 == 0) || (*(char *)(lVar4 + 0x692) != '\x01')) {
    *(undefined1 *)((ulonglong)*param_1 + 0x60 + lVar5) = 0;
  }
  else {
    *(undefined1 *)((ulonglong)*param_1 + 0x60 + lVar5) = 1;
  }
  local_4fd = -1;
  func_0x000140664130(&local_520,&local_4fd,1);
  cVar2 = local_4fd;
  if (local_4fd != -1) {
    local_500 = 0;
    func_0x000140664130(&local_520,&local_500,1);
    uVar6 = (uint)local_500;
    if ((char)local_500 < '\0') {
      func_0x000140664130(&local_520,&local_500,1);
      uVar6 = (uint)local_500 | (uVar6 & 0x7f) << 8;
    }
    local_4f8 = (undefined8 *)0x0;
    uStack_4f0 = 0;
    FUN_1407fcd90(&local_4f8,uVar6);
    puVar3 = local_4f8;
    func_0x000140664130(&local_520,local_4f8,uVar6);
    lVar5 = *(longlong *)(&DAT_146185e00 + (ulonglong)*puVar10 * 8);
    lVar4 = (ulonglong)*param_1 + lVar5;
    local_538 = 0xffffffff;
    local_534 = 0;
    FUN_1407fce90(&local_538);
    iVar7 = (int)uStack_4f0;
    if ((int)uStack_4f0 == 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    uVar9 = uStack_4f0 & 0xffffffff;
    if (local_534 >> 0x20 < uVar9) {
      if (local_538 != 0xffffffff) {
        func_0x000140567f00(0,lVar5 + (ulonglong)local_538);
      }
      local_538 = 0xffffffff;
      local_534 = local_534 & 0xffffffff;
      func_0x000140482a60(&local_538,uVar9);
    }
    uVar6 = local_538;
    local_52c = iVar7;
    if (local_538 == 0xffffffff) {
      lVar5 = 0;
    }
    else {
      lVar5 = (ulonglong)local_538 + *(longlong *)(&DAT_146185e00 + (ulonglong)*puVar10 * 8);
    }
    func_0x0001422bdd9d(lVar5,puVar3,uVar9);
    func_0x000140567f00(5,puVar3);
    local_4f8 = (undefined8 *)0x0;
    uStack_4f0 = 0;
    *(char *)(lVar4 + 0x48) = cVar2;
    puVar1 = (uint *)(lVar4 + 0x3c);
    uVar8 = uVar6;
    if (puVar1 != &local_538) {
      func_0x000140194370(puVar1);
      uVar8 = *puVar1;
      *puVar1 = uVar6;
      *(int *)(lVar4 + 0x40) = local_52c;
      *(undefined4 *)(lVar4 + 0x44) = local_534._4_4_;
    }
    if (uVar8 != 0xffffffff) {
      func_0x000140567f00(0,(ulonglong)uVar8 +
                            *(longlong *)(&DAT_146185e00 + (ulonglong)*puVar10 * 8));
    }
  }
  lVar5 = *(longlong *)(&DAT_146185e00 + (ulonglong)*puVar10 * 8);
  lVar4 = (ulonglong)*param_1 + lVar5;
  *(undefined4 *)(lVar4 + 8) = local_4d8;
  *(undefined4 *)(lVar4 + 0xc) = local_4d4;
  *(undefined1 *)(lVar4 + 0x10) = local_4d0;
  *(undefined1 *)(lVar4 + 0x14) = (undefined1)local_4cc;
  *(undefined1 *)(lVar4 + 0x15) = local_4cc._1_1_;
  *(undefined8 *)(lVar4 + 0x18) = local_4c8;
  FUN_1407fcba0(local_4e8);
  if (local_4e8[0] != 0xffffffff) {
    func_0x000140567f00(0,lVar5 + (ulonglong)local_4e8[0]);
  }
  func_0x0001404819c0(&local_278);
  func_0x0001422ba6e0(local_38 ^ (ulonglong)auStack_568);
  return;
}

