// rea decompile_all
// name: FUN_14374c180
// address: 0x14374c180
// body: available
// range: 0x14374c180-0x14374c327 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_14374c180(undefined8 param_1,undefined8 param_2)

{
  char cVar1;
  char cVar2;
  longlong lVar3;
  longlong lVar4;
  longlong lVar5;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_58 [32];
  undefined8 local_38;
  ulonglong local_30;
  
  local_30 = DAT_145df0fc0 ^ (ulonglong)auStack_58;
  lVar3 = func_0x000140bfc930(param_2);
  if (((lVar3 != 0) && (lVar3 = func_0x000140821e90(lVar3), lVar3 != 0)) &&
     (local_38 = *(undefined8 *)(lVar3 + 0x38), (int)local_38 != 0)) {
    lVar5 = **(longlong **)(unaff_GS_OFFSET + 0x58);
    lVar4 = func_0x00014038f9e0(*(longlong *)
                                 (&DAT_146185e00 + (ulonglong)*(uint *)(lVar5 + 0x40) * 8) +
                                (ulonglong)
                                *(uint *)((ulonglong)DAT_145e0becc + 0x104 +
                                         *(longlong *)
                                          (&DAT_146185e00 + (ulonglong)*(uint *)(lVar5 + 0x40) * 8))
                                ,&local_38);
    if ((lVar4 != 0) && (cVar1 = FUN_14374bfd0(lVar4), cVar1 == '\0')) {
      local_38 = param_1;
      cVar1 = func_0x000140b38720(*(longlong *)
                                   (&DAT_146185e00 + (ulonglong)*(uint *)(lVar5 + 0x40) * 8) +
                                  (ulonglong)
                                  *(uint *)((ulonglong)DAT_145e0becc + 0x11c +
                                           *(longlong *)
                                            (&DAT_146185e00 + (ulonglong)*(uint *)(lVar5 + 0x40) * 8
                                            )),&local_38,param_2);
      cVar2 = func_0x0001403dc680(lVar3,cVar1);
      if ((((cVar2 != '\0') &&
           ((cVar1 == '\x02' ||
            (lVar5 = func_0x0001405d6f70(param_2), *(char *)(lVar5 + 0x7be) == '\0')))) &&
          (((*(int *)(lVar4 + 0x88) != 0 &&
            (cVar1 = func_0x00014374af68(param_1,lVar4), cVar1 != '\0')) ||
           (cVar1 = func_0x000140ad2290(lVar3), cVar1 != '\0')))) &&
         (lVar3 = func_0x0001405daad0(**(undefined8 **)(DAT_146185f18 + 0xa8),
                                      *(undefined8 *)(lVar4 + 0x88)), lVar3 != 0)) {
        local_38 = CONCAT44(*(undefined4 *)(*(longlong *)(lVar3 + 0x48) + 0xa8),
                            *(undefined4 *)(*(longlong *)(lVar3 + 0x48) + 0xa0));
        func_0x0001414848e0(&local_38);
      }
    }
  }
  func_0x0001422ba6e0(local_30 ^ (ulonglong)auStack_58);
  return;
}

