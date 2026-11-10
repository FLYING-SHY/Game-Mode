// rea decompile_all
// name: FUN_1404b2390
// address: 0x1404b2390
// body: available
// range: 0x1404b2390-0x1404b24a1 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

int * FUN_1404b2390(longlong param_1,int *param_2)

{
  longlong lVar1;
  int iVar2;
  longlong lVar3;
  longlong lVar4;
  int *piVar5;
  longlong *plVar6;
  ulonglong uVar7;
  longlong *plVar8;
  longlong local_28;
  longlong local_20;
  
  plVar8 = (longlong *)0x0;
  *param_2 = 0;
  param_2[1] = -1;
  lVar1 = param_1 + 0x70;
  local_20 = lVar1;
  func_0x0001401ae690(lVar1);
  piVar5 = (int *)func_0x0001404ea5b0(param_1,&local_28);
  iVar2 = *piVar5;
  *param_2 = iVar2;
  uVar7 = (ulonglong)(uint)piVar5[1];
  param_2[1] = piVar5[1];
  if (((iVar2 != 0) &&
      (uVar7 < (ulonglong)(*(longlong *)(param_1 + 0x28) - *(longlong *)(param_1 + 0x20) >> 2))) &&
     (*(int *)(*(longlong *)(param_1 + 0x20) + uVar7 * 4) == iVar2)) {
    plVar8 = (longlong *)(*(longlong *)(param_1 + 8) + uVar7 * 8);
  }
  plVar6 = (longlong *)func_0x0001404ea070(&local_28);
  lVar3 = *plVar6;
  *plVar6 = 0;
  lVar4 = *plVar8;
  *plVar8 = lVar3;
  if (lVar4 != 0) {
    func_0x0001404ea3c0();
  }
  if (local_28 != 0) {
    func_0x0001404ea3c0();
  }
  lVar4 = *plVar8;
  lVar3 = lVar4 + 8;
  local_28 = lVar3;
  (*(code *)0x5bf315a)(lVar3);
  *(undefined4 *)(lVar4 + 0x1c) = 0xffffffff;
  *(undefined8 *)(lVar4 + 0x20) = *(undefined8 *)(_DAT_146ba43b8 + 0xb8);
  *(byte *)(lVar4 + 0x30) = *(byte *)(lVar4 + 0x30) | 1;
  func_0x00014134cea0(*(undefined8 *)(_DAT_146ba43b8 + 0x98));
  *(int *)(lVar4 + 0x28) = *(int *)(lVar4 + 0x28) + 1;
  (*(code *)0x5bf3174)(lVar3);
  func_0x0001401ae110(lVar1);
  return param_2;
}

