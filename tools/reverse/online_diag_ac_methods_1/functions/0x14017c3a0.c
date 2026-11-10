// rea decompile_all
// name: FUN_14017c3a0
// address: 0x14017c3a0
// body: available
// range: 0x14017c3a0-0x14017c473 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Ghidra Listing reports a terminal call at 0x14017c44e to 0x14017c4e0 FUN_14017c4e0 (external=false, thunk=false, hasNoReturn=false); fallthrough is excluded by Ghidra's flow model. The reason for the terminal flow is not established by this observation.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14017c3a0(longlong param_1,longlong *param_2)

{
  code *pcVar1;
  undefined8 *puVar2;
  longlong *plVar3;
  longlong lVar4;
  longlong *plVar5;
  float fVar6;
  undefined8 local_res8;
  
  puVar2 = (undefined8 *)FUN_14017c480(param_1 + 0x18);
  local_res8 = *puVar2;
  plVar3 = (longlong *)func_0x000140424b40(param_1,&local_res8);
  plVar5 = (longlong *)0x0;
  if (plVar3 != (longlong *)0x0) {
    pcVar1 = *(code **)(*plVar3 + 0x18);
    if (pcVar1 == (code *)0x140180a70) {
      fVar6 = *(float *)(plVar3 + 0x12);
    }
    else if (pcVar1 == (code *)0x14017f840) {
      fVar6 = *(float *)(plVar3 + 0x1b);
      if (fVar6 <= 0.0) {
        fVar6 = 0.0;
      }
    }
    else {
      fVar6 = (float)(*pcVar1)(plVar3);
    }
    if (0.0 < fVar6) {
      *param_2 = plVar3[0x17];
      plVar5 = plVar3;
    }
  }
  func_0x00014017f4c0(param_1 + 0x18,plVar5,param_2);
  lVar4 = func_0x0001401943a0(param_1 + 8);
  FUN_14017c4e0(*(undefined8 *)(lVar4 + 0x80),param_2);
  return;
}

