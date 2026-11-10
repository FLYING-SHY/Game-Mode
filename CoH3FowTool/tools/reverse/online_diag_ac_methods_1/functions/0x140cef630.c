// rea decompile_all
// name: FUN_140cef630
// address: 0x140cef630
// body: available
// range: 0x140cef630-0x140cef7bb inclusive
// range: 0x142925664-0x1429256b5 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 *
FUN_140cef630(undefined8 *param_1,longlong param_2,undefined8 *param_3,longlong param_4,
             undefined8 param_5)

{
  longlong *plVar1;
  int *piVar2;
  int iVar3;
  char cVar4;
  longlong *plVar5;
  undefined8 uVar6;
  undefined8 *puVar7;
  longlong local_res18;
  undefined8 *local_res20;
  
  func_0x000140cb40a0();
  *param_1 = &DAT_143df1ed0;
  param_1[6] = &DAT_143df1e30;
  param_1[7] = &DAT_143df1da0;
  param_1[8] = &DAT_143df1db8;
  param_1[9] = &DAT_143df1dd8;
  puVar7 = param_1 + 10;
  local_res20 = puVar7;
  func_0x000140452c50();
  *puVar7 = &DAT_143df1df8;
  plVar1 = puVar7 + 5;
  *plVar1 = 0;
  puVar7[6] = 0;
  plVar5 = (longlong *)FUN_140cef7c0(&local_res18,4);
  if (plVar1 != plVar5) {
    if (*plVar1 != 0) {
      LOCK();
      piVar2 = (int *)(*plVar1 + 0x10);
      iVar3 = *piVar2;
      *piVar2 = *piVar2 + -1;
      UNLOCK();
      if (iVar3 == 1) {
        func_0x000140452990();
      }
      *plVar1 = 0;
    }
    *plVar1 = *plVar5;
    *plVar5 = 0;
  }
  if (local_res18 != 0) {
    LOCK();
    piVar2 = (int *)(local_res18 + 0x10);
    iVar3 = *piVar2;
    *piVar2 = *piVar2 + -1;
    UNLOCK();
    if (iVar3 == 1) {
      func_0x000140452990();
    }
  }
  func_0x000140210c30(param_1 + 0x11,param_2);
  param_1[0x15] = 0xffffffffffffffff;
  *(bool *)(param_1 + 0x16) = param_2 != 0;
  *(undefined1 *)((longlong)param_1 + 0xb1) = 0;
  uVar6 = *param_3;
  *param_3 = 0;
  func_0x000140cf01c0(param_1 + 0x17,uVar6);
  param_1[0x20] = 0;
  puVar7 = *(undefined8 **)(param_4 + 0x38);
  if (puVar7 != (undefined8 *)0x0) {
    uVar6 = (**(code **)*puVar7)(puVar7,param_1 + 0x19);
    param_1[0x20] = uVar6;
  }
  func_0x0001401c32b0(param_1 + 0x21,param_5);
  param_1[0x2a] = 0;
  param_1[0x2b] = 0;
  param_1[0x2c] = 0;
  param_1[0x2d] = 0;
  param_1[0x2e] = 0;
  param_1[0x2f] = 0;
  cVar4 = func_0x000140cf0630(param_1);
  if (cVar4 != '\0') {
    func_0x000140cf02a0(param_1,param_1[0x17]);
  }
  return param_1;
}

