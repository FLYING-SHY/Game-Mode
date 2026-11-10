// rea decompile_all
// name: FUN_1407fc450
// address: 0x1407fc450
// body: available
// range: 0x1407fc450-0x1407fc4f8 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_1407fc450(longlong param_1,undefined4 param_2)

{
  longlong *plVar1;
  longlong *plVar2;
  longlong *plVar3;
  undefined4 local_res10 [2];
  longlong *local_18 [2];
  
  local_18[0] = (longlong *)0x0;
  local_res10[0] = param_2;
  func_0x0001407fbfe0(param_1,local_18,local_res10);
  plVar2 = local_18[0];
  if (local_18[0] != *(longlong **)(param_1 + 8)) {
    plVar3 = (longlong *)local_18[0][1];
    local_18[0][1] = 0;
    if (*plVar3 != plVar3[1]) {
      plVar3[1] = *plVar3;
    }
    *(undefined4 *)(plVar3 + 3) = 0;
    plVar3[4] = 0;
    plVar1 = *(longlong **)(param_1 + 0x28);
    local_18[0] = plVar3;
    if (plVar1 == *(longlong **)(param_1 + 0x30)) {
      func_0x00014133be20(param_1 + 0x20,plVar1,local_18);
      plVar3 = local_18[0];
    }
    else {
      *plVar1 = (longlong)plVar3;
      *(longlong *)(param_1 + 0x28) = *(longlong *)(param_1 + 0x28) + 8;
      plVar3 = (longlong *)0x0;
    }
    func_0x000140d10e20(param_1,local_18,plVar2);
    if (plVar3 != (longlong *)0x0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
  }
  return;
}

