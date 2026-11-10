// rea decompile_all
// name: FUN_1404b6140
// address: 0x1404b6140
// body: available
// range: 0x1404b6140-0x1404b61ff inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_1404b6140(undefined8 param_1,undefined8 *param_2)

{
  code *pcVar1;
  longlong lVar2;
  
  if (param_2 == (undefined8 *)0x0) {
    return;
  }
  pcVar1 = *(code **)*param_2;
  if (pcVar1 == FUN_141513950) {
    func_0x0001404b4440(param_2);
  }
  else if (pcVar1 == (code *)0x141330620) {
    lVar2 = param_2[6];
    if (lVar2 != 0) {
      func_0x00014031b310(lVar2,param_2[8] - lVar2 & 0xfffffffffffffff8);
      param_2[6] = 0;
      param_2[7] = 0;
      param_2[8] = 0;
    }
    func_0x000141330690();
    func_0x00014031b310(param_2[4],0x70);
  }
  else if (pcVar1 != (code *)0x14150db90) {
    (*pcVar1)(param_2,1);
    return;
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

