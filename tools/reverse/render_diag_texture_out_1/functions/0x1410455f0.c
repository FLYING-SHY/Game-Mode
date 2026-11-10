// rea decompile_all
// name: FUN_1410455f0
// address: 0x1410455f0
// body: available
// range: 0x1410455f0-0x14104566f inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


ulonglong FUN_1410455f0(longlong param_1,uint param_2,undefined8 *param_3)

{
  uint uVar1;
  undefined4 uVar2;
  longlong lVar3;
  ulonglong in_RAX;
  longlong lVar4;
  ulonglong uVar5;
  
  if (*(char *)(param_1 + 0x168) != '\0') {
    in_RAX = *(longlong *)(param_1 + 0x130) - *(longlong *)(param_1 + 0x128) >> 2;
    if (param_2 < in_RAX) {
      uVar1 = *(uint *)(*(longlong *)(param_1 + 0x128) + (ulonglong)param_2 * 4);
      uVar5 = (ulonglong)uVar1;
      lVar3 = *(longlong *)(param_1 + 0x150);
      lVar4 = *(longlong *)(param_1 + 0x158) - lVar3;
      in_RAX = -(lVar4 >> 0x3f);
      if ((uVar5 < (ulonglong)(lVar4 / 0xc)) && (uVar1 != 0xffffffff)) {
        *param_3 = *(undefined8 *)(lVar3 + uVar5 * 0xc);
        uVar2 = *(undefined4 *)(lVar3 + 8 + uVar5 * 0xc);
        *(undefined4 *)(param_3 + 1) = uVar2;
        return CONCAT71((uint7)(uint3)((uint)uVar2 >> 8),1);
      }
    }
  }
  return in_RAX & 0xffffffffffffff00;
}

