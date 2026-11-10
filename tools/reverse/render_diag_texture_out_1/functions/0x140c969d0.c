// rea decompile_all
// name: FUN_140c969d0
// address: 0x140c969d0
// body: available
// range: 0x140c969d0-0x140c96a76 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_140c969d0(undefined8 param_1)

{
  char cVar1;
  int iVar2;
  longlong lVar3;
  longlong lVar4;
  undefined8 local_res10 [3];
  
  lVar3 = func_0x00014024a3d0();
  if (lVar3 == 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  local_res10[0] = 0x8000000000000010;
  func_0x0001407d4b60(local_res10,0,0,0x143de6978,*(undefined4 *)(lVar3 + 0x28));
  cVar1 = FUN_140c96aa0(param_1);
  if (cVar1 != '\0') {
    lVar4 = _DAT_146a8ca18;
    iVar2 = func_0x000140249de0();
    if (iVar2 == 3) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    if (*(longlong *)(**(longlong **)(lVar4 + 0x148) + 0xa8) != 0x1414c3d20) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    (*(longlong **)(lVar4 + 0x148))[0x21] = *(longlong *)(lVar3 + 0x20);
    FUN_140c96a80();
  }
  return;
}

