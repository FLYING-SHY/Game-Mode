// rea decompile_all
// name: FUN_140cefde0
// address: 0x140cefde0
// body: available
// range: 0x140cefde0-0x140cefe9f inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_140cefde0(undefined8 *param_1,undefined8 param_2,undefined8 param_3,undefined8 param_4)

{
  undefined8 *puVar1;
  
  *param_1 = 0x143df21e0;
  param_1[3] = 0x143df21c0;
  if (*(char *)(*(longlong *)param_1[0xd] + 0x19) == '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  func_0x00014088e2c0(param_1 + 0xd,param_1 + 0xd,*(undefined8 *)(param_1[0xd] + 8),param_4,
                      *(longlong *)param_1[0xd]);
  func_0x00014031b310(param_1[0xd],0x30);
  puVar1 = (undefined8 *)param_1[9];
  if (puVar1 != (undefined8 *)0x0) {
    if (*(longlong *)*puVar1 != 0x141443480) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    if (puVar1[2] != 0) {
      func_0x0001405094b0();
    }
    func_0x0001422ba700(puVar1,0x18);
  }
  if (param_1[5] == 0) {
    if (param_1[2] != 0) {
      func_0x0001405094b0();
    }
    return;
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

