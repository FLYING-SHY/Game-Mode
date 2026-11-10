// rea decompile_all
// name: FUN_14017ca30
// address: 0x14017ca30
// body: available
// range: 0x14017ca30-0x14017cabe inclusive
// range: 0x14017cac0-0x14017cae7 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14017ca30(undefined8 *param_1)

{
  undefined8 *puVar1;
  uint uVar2;
  undefined8 *puVar3;
  undefined8 *puVar4;
  
  *param_1 = 0x143d6dd40;
  func_0x00014017d240();
  func_0x0001401bf0d0(param_1 + 0xf);
  FUN_14017cd20(param_1 + 5);
  puVar1 = param_1 + 3;
  puVar3 = puVar1;
  if (2 < *(uint *)(param_1 + 2)) {
    puVar3 = (undefined8 *)*puVar1;
  }
  uVar2 = *(uint *)((longlong)param_1 + 0x14);
  for (puVar4 = puVar3; puVar4 != puVar3 + uVar2; puVar4 = puVar4 + 1) {
    func_0x000140ef83a0(puVar4);
  }
  if (puVar3 != puVar1) {
    func_0x000140567f00(5,puVar3);
  }
  param_1[2] = 2;
  *param_1 = 0x143d99ae0;
  return;
}

