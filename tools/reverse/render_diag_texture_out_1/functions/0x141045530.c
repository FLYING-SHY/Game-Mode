// rea decompile_all
// name: FUN_141045530
// address: 0x141045530
// body: available
// range: 0x141045530-0x1410455ed inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

undefined8 * FUN_141045530(longlong param_1,undefined8 *param_2,undefined8 param_3)

{
  longlong *plVar1;
  char cVar2;
  longlong lVar3;
  longlong lVar4;
  undefined8 local_28;
  undefined4 local_20;
  
  plVar1 = *(longlong **)(param_1 + 0x18);
  if (*(longlong *)(*plVar1 + 0x10) != 0x1404d08e0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  (**(code **)(*(longlong *)plVar1[1] + 0x10))((longlong *)plVar1[1],param_3);
  lVar3 = func_0x0001404d0930(param_3,plVar1[2]);
  lVar4 = 0;
  if (lVar3 != 0) {
    lVar4 = lVar3 + 0x18;
  }
  (**(code **)(**(longlong **)(param_1 + 0x20) + 0x10))(*(longlong **)(param_1 + 0x20),param_3);
  if ((lVar4 != 0) && (*(int *)(param_1 + 0x10) != -1)) {
    cVar2 = FUN_1410455f0(lVar4,*(int *)(param_1 + 0x10),&local_28);
    if (cVar2 != '\0') {
      *param_2 = local_28;
      *(undefined4 *)(param_2 + 1) = local_20;
      *(undefined1 *)((longlong)param_2 + 0xc) = 1;
      return param_2;
    }
  }
  *(undefined1 *)((longlong)param_2 + 0xc) = 0;
  return param_2;
}

