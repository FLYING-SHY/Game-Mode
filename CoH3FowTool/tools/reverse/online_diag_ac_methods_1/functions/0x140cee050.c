// rea decompile_all
// name: FUN_140cee050
// address: 0x140cee050
// body: available
// range: 0x140cee050-0x140cee149 inclusive
// range: 0x1429250da-0x1429251b8 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_140cee050(longlong param_1,longlong *param_2)

{
  int *piVar1;
  int iVar2;
  longlong *plVar3;
  undefined8 uVar4;
  char cVar5;
  undefined8 *puVar6;
  undefined1 auStack_c8 [32];
  undefined8 local_a8;
  longlong local_90;
  undefined1 local_88 [32];
  undefined8 local_68;
  undefined8 local_60;
  undefined *local_48;
  undefined8 uStack_40;
  undefined8 local_38;
  undefined8 uStack_30;
  ulonglong local_28;
  
  local_28 = DAT_145df0fc0 ^ (ulonglong)auStack_c8;
  if (*(longlong *)(param_1 + 0x140) != 0) {
    func_0x0001403b68f0();
  }
  local_48 = (undefined *)0x0;
  uStack_40 = 0;
  local_38 = 0;
  uStack_30 = 0;
  if (*(code **)(*param_2 + 0x28) == FUN_140cee2d0) {
    if (param_2[6] == 0) {
      FUN_140cee330(&local_68);
      puVar6 = &local_68;
    }
    else {
      puVar6 = (undefined8 *)FUN_140cee330(local_88,param_2[6] + 8);
    }
    local_48 = &DAT_143df1b80;
    uStack_40 = puVar6[1];
    local_38 = puVar6[2];
    uStack_30 = puVar6[3];
  }
  else {
    (**(code **)(*param_2 + 0x28))(param_2,&local_48);
  }
  local_68 = 0;
  local_60 = 0;
  while (cVar5 = FUN_140cee360(&local_48,&local_68), cVar5 != '\0') {
    plVar3 = *(longlong **)(param_1 + 0x100);
    if ((plVar3 == (longlong *)0x0) ||
       (cVar5 = (**(code **)(*plVar3 + 0x10))(plVar3,&local_68,local_60), cVar5 != '\0')) {
      local_a8 = local_60;
      puVar6 = (undefined8 *)func_0x000142b60bf0(param_1,&local_90,param_2,&local_68);
      uVar4 = *puVar6;
      if (*(int *)(param_1 + 0x80) == *(int *)(*(longlong *)(param_1 + 0x78) + 0x30)) {
        func_0x000142b6da44(param_1 + 0x50,*(int *)(param_1 + 0x80) + 1);
      }
      iVar2 = *(int *)(param_1 + 0x80);
      *(int *)(param_1 + 0x80) = iVar2 + 1;
      func_0x0001424da830(*(longlong *)(*(longlong *)(param_1 + 0x78) + 0x28) + (longlong)iVar2 * 8,
                          uVar4);
      *(int *)(param_1 + 0x84) = *(int *)(param_1 + 0x84) + 1;
      if (local_90 != 0) {
        LOCK();
        piVar1 = (int *)(local_90 + 0x10);
        iVar2 = *piVar1;
        *piVar1 = *piVar1 + -1;
        UNLOCK();
        if (iVar2 == 1) {
          func_0x000140452990();
        }
      }
    }
  }
  func_0x0001422ba6e0(local_28 ^ (ulonglong)auStack_c8);
  return;
}

