// rea decompile_all
// name: FUN_140610530
// address: 0x140610530
// body: available
// range: 0x140610530-0x1406105d0 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_140610530(longlong *param_1,longlong *param_2)

{
  longlong lVar1;
  undefined8 uVar2;
  longlong lVar3;
  longlong lVar4;
  
  lVar4 = (param_1[1] - *param_1) / 6 + (param_1[1] - *param_1 >> 0x3f);
  uVar2 = FUN_1406105e0(*param_2);
  lVar3 = func_0x0001402aa7a0(uVar2);
  func_0x0001422bddaf(lVar3,*param_1,param_1[1] - *param_1);
  lVar1 = *param_2;
  if (*param_1 == 0) {
    *param_1 = lVar3;
    param_1[1] = lVar3 + ((lVar4 >> 1) - (lVar4 >> 0x3f)) * 0xc;
    param_1[2] = lVar3 + lVar1 * 0xc;
    return;
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

