// rea decompile_all
// name: entry
// address: 0x140ceed80
// body: available
// range: 0x140ceed80-0x140ceee2a inclusive
// range: 0x142925436-0x1429254dc inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void entry(longlong param_1)

{
  uint uVar1;
  longlong lVar2;
  ulonglong uVar3;
  uint *puVar4;
  longlong lVar5;
  longlong unaff_GS_OFFSET;
  undefined8 local_res8;
  
  puVar4 = (uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
  lVar2 = *(longlong *)(&DAT_146185e00 + (ulonglong)*puVar4 * 8);
  uVar1 = *(uint *)((ulonglong)DAT_145e0becc + 0x44 + lVar2);
  if ((((*(int *)(param_1 + 0x560) < (int)uVar1) &&
       (*(uint *)(param_1 + 0x560) = uVar1, 0x7f < uVar1)) &&
      (lVar2 = func_0x00014018c920((ulonglong)*(uint *)((ulonglong)DAT_145e0becc + 0xfc + lVar2) +
                                   lVar2,param_1 + 0x558), lVar2 != 0)) &&
     (((*(char *)(lVar2 + 0x680) == '\0' && (*(char *)(lVar2 + 0x691) == '\t')) &&
      ((FUN_140ceee30(param_1,uVar1), *(char *)(param_1 + 0x564) != '\0' && ((uVar1 & 7) == 0))))))
  {
    lVar2 = *(longlong *)(&DAT_146185e00 + (ulonglong)*puVar4 * 8);
    uVar3 = (ulonglong)(uVar1 >> 3 & 0xf);
    lVar5 = (ulonglong)*(uint *)((ulonglong)DAT_145e0becc + 0xfc + lVar2) + lVar2;
    if (uVar3 < *(uint *)(lVar5 + 0x10)) {
      local_res8 = 0x98ee5700e985ada2;
      lVar2 = func_0x00014026d480(lVar2 + (ulonglong)DAT_145e0becc + 0x68,&local_res8);
      if (lVar2 == 0) {
        lVar2 = func_0x0001401943a0(*(longlong *)(&DAT_146185e00 + (ulonglong)*puVar4 * 8) +
                                    (ulonglong)*(uint *)(lVar5 + 0xc) + uVar3 * 4);
        if (*(int *)(lVar2 + 8) == *(int *)(param_1 + 0x558)) {
          FUN_14374bab8();
        }
        else {
          FUN_14374b890(param_1,uVar1);
        }
      }
    }
  }
  return;
}

