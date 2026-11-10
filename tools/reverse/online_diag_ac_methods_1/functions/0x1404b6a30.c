// rea decompile_all
// name: FUN_1404b6a30
// address: 0x1404b6a30
// body: available
// range: 0x1404b6a30-0x1404b6c1c inclusive
// range: 0x14275a592-0x14275a609 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_1404b6a30(longlong param_1)

{
  char cVar1;
  undefined4 uVar2;
  longlong *plVar3;
  longlong lVar4;
  longlong lVar5;
  longlong *plVar6;
  longlong lVar7;
  undefined1 auStack_38 [32];
  longlong local_18;
  ulonglong local_10;
  
  local_10 = DAT_145df0fc0 ^ (ulonglong)auStack_38;
  lVar7 = 0;
  local_18 = 0;
  cVar1 = func_0x000140514f20(DAT_146185d60,&local_18,
                              "config/vehicle_customization_weapon_hit_handler_configuration.rdo");
  if ((cVar1 != '\0') && (local_18 != 0)) {
    plVar3 = (longlong *)func_0x000140514ee0(&local_18);
    if (*(code **)(*plVar3 + 0x18) == (code *)0x140516310) {
      plVar6 = plVar3 + 6;
      if (0xf < (ulonglong)plVar3[9]) {
        plVar6 = (longlong *)*plVar6;
      }
      lVar4 = func_0x0001405163c0(DAT_146185e88,plVar6);
      lVar5 = lVar7;
      if (lVar4 != 0) {
        lVar5 = func_0x000140516390(lVar4,0x72d4bf705b849aa3);
      }
    }
    else {
      lVar5 = (**(code **)(*plVar3 + 0x18))(plVar3,0x72d4bf705b849aa3);
    }
    lVar4 = lVar7;
    if (lVar5 != 0) {
      lVar4 = lVar5;
    }
    if (*(char *)(param_1 + 0x118) == '\0') {
      FUN_1404b6c30();
      *(undefined1 *)(param_1 + 0x118) = 1;
    }
    else {
      func_0x0001435a0148(param_1 + 0xa8,lVar4);
    }
  }
  cVar1 = func_0x000140514f20(DAT_146185d60,&local_18,
                              "config/vehicle_customization_weathering_configuration.rdo");
  if ((cVar1 != '\0') && (local_18 != 0)) {
    plVar3 = (longlong *)func_0x000140514ee0(&local_18);
    if (*(code **)(*plVar3 + 0x18) == (code *)0x140516310) {
      plVar6 = plVar3 + 6;
      if (0xf < (ulonglong)plVar3[9]) {
        plVar6 = (longlong *)*plVar6;
      }
      lVar4 = func_0x0001405163c0(DAT_146185e88,plVar6);
      lVar5 = lVar7;
      if (lVar4 != 0) {
        lVar5 = func_0x000140516390(lVar4,0x310e4692d762da82);
      }
    }
    else {
      lVar5 = (**(code **)(*plVar3 + 0x18))(plVar3,0x310e4692d762da82);
    }
    if (lVar5 != 0) {
      lVar7 = lVar5;
    }
    if (*(char *)(param_1 + 0x148) == '\0') {
      *(undefined8 *)(param_1 + 0x120) = 0x143d6e458;
      *(undefined8 *)(param_1 + 0x128) = *(undefined8 *)(lVar7 + 8);
      *(undefined8 *)(param_1 + 0x130) = *(undefined8 *)(lVar7 + 0x10);
      *(undefined1 *)(param_1 + 0x138) = *(undefined1 *)(lVar7 + 0x18);
      *(undefined ***)(param_1 + 0x120) = &PTR_FUN_143d7d8b0;
      *(undefined4 *)(param_1 + 0x140) = *(undefined4 *)(lVar7 + 0x20);
      uVar2 = *(undefined4 *)(lVar7 + 0x24);
      *(undefined1 *)(param_1 + 0x148) = 1;
    }
    else {
      *(undefined8 *)(param_1 + 0x128) = *(undefined8 *)(lVar7 + 8);
      *(undefined8 *)(param_1 + 0x130) = *(undefined8 *)(lVar7 + 0x10);
      *(undefined1 *)(param_1 + 0x138) = *(undefined1 *)(lVar7 + 0x18);
      *(undefined4 *)(param_1 + 0x140) = *(undefined4 *)(lVar7 + 0x20);
      uVar2 = *(undefined4 *)(lVar7 + 0x24);
    }
    *(undefined4 *)(param_1 + 0x144) = uVar2;
  }
  if (local_18 != 0) {
    func_0x0001401ffe70(&local_18);
  }
  func_0x0001422ba6e0(local_10 ^ (ulonglong)auStack_38);
  return;
}

