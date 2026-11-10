// rea decompile_all
// name: FUN_140cee1b0
// address: 0x140cee1b0
// body: available
// range: 0x140cee1b0-0x140cee2c1 inclusive
// range: 0x1429251ba-0x1429252e9 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_140cee1b0(longlong param_1,longlong *param_2,undefined8 param_3,undefined8 param_4)

{
  int *piVar1;
  longlong *plVar2;
  longlong lVar3;
  char cVar4;
  undefined8 *puVar5;
  int iVar6;
  undefined1 auStack_f8 [32];
  undefined8 local_d8;
  int local_c8 [2];
  longlong local_c0;
  undefined1 local_b8 [32];
  undefined8 local_98;
  undefined8 local_90;
  longlong local_78;
  undefined *local_70;
  undefined8 uStack_68;
  undefined8 local_60;
  undefined8 uStack_58;
  ulonglong local_50;
  
  local_50 = DAT_145df0fc0 ^ (ulonglong)auStack_f8;
  iVar6 = 0;
  local_c8[0] = 0;
  if (*(longlong *)(param_1 + 0x140) != 0) {
    func_0x0001403b68f0();
  }
  local_70 = (undefined *)0x0;
  uStack_68 = 0;
  local_60 = 0;
  uStack_58 = 0;
  if (*(code **)(*param_2 + 0x28) == FUN_140cee2d0) {
    if (param_2[6] == 0) {
      FUN_140cee330(&local_98);
      puVar5 = &local_98;
    }
    else {
      puVar5 = (undefined8 *)FUN_140cee330(local_b8,param_2[6] + 8);
    }
    local_70 = &DAT_143df1b80;
    uStack_68 = puVar5[1];
    local_60 = puVar5[2];
    uStack_58 = puVar5[3];
  }
  else {
    (**(code **)(*param_2 + 0x28))(param_2,&local_70);
  }
  local_98 = 0;
  local_90 = 0;
  while (cVar4 = FUN_140cee360(&local_70,&local_98), cVar4 != '\0') {
    plVar2 = *(longlong **)(param_1 + 0x100);
    if ((plVar2 == (longlong *)0x0) ||
       (cVar4 = (**(code **)(*plVar2 + 0x10))(plVar2,&local_98,local_90), cVar4 != '\0')) {
      if (iVar6 < *(int *)(param_1 + 0x80)) {
        lVar3 = *(longlong *)
                 (*(longlong *)(*(longlong *)(param_1 + 0x78) + 0x28) + (longlong)iVar6 * 8);
        *(undefined8 *)(lVar3 + 0xb0) = local_90;
        *(undefined8 *)(lVar3 + 0xb8) = _DAT_146a8cbe8;
        func_0x0001403257b0(lVar3,param_3,param_4);
        local_c8[0] = iVar6;
      }
      else {
        local_78 = 0;
        local_d8 = local_90;
        func_0x000142b60bf0(param_1,&local_78,param_2,&local_98);
        local_c0 = param_1;
        func_0x000142b1fb0c(param_4,&local_c0,local_c8,&local_78);
        if (local_78 != 0) {
          LOCK();
          piVar1 = (int *)(local_78 + 0x10);
          iVar6 = *piVar1;
          *piVar1 = *piVar1 + -1;
          UNLOCK();
          if (iVar6 == 1) {
            func_0x000140452990();
          }
        }
      }
      iVar6 = local_c8[0] + 1;
      local_c8[0] = iVar6;
    }
  }
  local_c8[0] = *(int *)(param_1 + 0x80);
  while (local_c8[0] = local_c8[0] + -1, iVar6 <= local_c8[0]) {
    local_78 = param_1;
    func_0x000142b1feb4(param_4,&local_78,local_c8);
  }
  if (iVar6 != *(int *)(param_1 + 0x80)) {
    local_c8[0] = 1;
    local_78 = param_1;
    func_0x000142b1f8b4(param_4,&local_78,local_c8);
  }
  func_0x0001422ba6e0(local_50 ^ (ulonglong)auStack_f8);
  return;
}

