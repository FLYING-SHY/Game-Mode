// rea decompile_all
// name: FUN_141045480
// address: 0x141045480
// body: available
// range: 0x141045480-0x1410454d5 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined1
FUN_141045480(undefined1 *param_1,undefined8 *param_2,undefined8 param_3,undefined8 param_4)

{
  undefined1 uVar1;
  code *pcVar2;
  
  LOCK();
  *param_1 = 1;
  UNLOCK();
  pcVar2 = *(code **)*param_2;
  if (pcVar2 == FUN_1410454e0) {
    func_0x0001402ce910();
  }
  else {
    (*pcVar2)(param_2,0,pcVar2,param_4,param_1);
  }
  (*_DAT_143d55bf8)(param_2);
  LOCK();
  uVar1 = *param_1;
  *param_1 = 0;
  UNLOCK();
  return uVar1;
}

