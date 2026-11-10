// rea decompile_all
// name: FUN_140ceee30
// address: 0x140ceee30
// body: available
// range: 0x140ceee30-0x140cef052 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


void FUN_140ceee30(longlong param_1,uint param_2)

{
  uint uVar1;
  int iVar2;
  longlong lVar3;
  undefined8 *puVar4;
  longlong lVar5;
  longlong lVar6;
  longlong lVar7;
  longlong lVar8;
  short sVar9;
  short sVar10;
  short sVar11;
  longlong unaff_GS_OFFSET;
  uint local_res10 [2];
  short local_58 [2];
  short local_54 [2];
  short local_50 [4];
  longlong local_48;
  longlong local_40;
  
  if ((param_2 & 0xf) == 0) {
    lVar6 = 0;
    sVar10 = 0;
    local_50[0] = 0;
    local_54[0] = 0;
    local_58[0] = 0;
    uVar1 = *(uint *)((ulonglong)DAT_145e0becc + 0x100 +
                     *(longlong *)
                      (&DAT_146185e00 +
                      (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8));
    if (uVar1 != 0xffffffff) {
      lVar6 = *(longlong *)
               (&DAT_146185e00 +
               (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8) +
              (ulonglong)uVar1;
    }
    local_res10[0] = param_2;
    lVar5 = func_0x0001401943a0(lVar6 + 0x708);
    local_40 = lVar5 + (ulonglong)*(uint *)(lVar6 + 0x70c) * 4;
    sVar11 = sVar10;
    for (; lVar5 != local_40; lVar5 = lVar5 + 4) {
      lVar6 = func_0x0001401943a0(lVar5);
      lVar6 = lVar6 + 0x100;
      lVar7 = func_0x0001401943a0(lVar6);
      local_48 = 0;
      func_0x00014042fa40(lVar6,&local_48);
      lVar6 = local_48;
      sVar9 = local_58[0];
      for (; lVar7 != lVar6; lVar7 = lVar7 + 4) {
        lVar8 = func_0x0001401943a0(lVar7);
        lVar8 = func_0x0001405daad0(**(undefined8 **)(*(longlong *)(param_1 + 8) + 0xa8),
                                    *(undefined8 *)(lVar8 + 0x88));
        if (((lVar8 != 0) && (sVar11 = sVar11 + 1, *(short *)(lVar8 + 0x6a) < 1)) &&
           (*(char *)(lVar8 + 0x69) == '\0')) {
          sVar10 = sVar10 + 1;
          if ((*(int *)(lVar8 + 0x60) != -1) &&
             (lVar3 = *(longlong *)(*(longlong *)(lVar8 + 0x58) + 0x40),
             iVar2 = *(int *)(*(longlong *)(lVar3 + 0x18) + (longlong)*(int *)(lVar8 + 0x60) * 4),
             (*(uint *)(*(longlong *)(lVar3 + 0x70) + ((ulonglong)(longlong)iVar2 >> 5) * 4) >>
              ((byte)iVar2 & 0x1f) & 1) != 0)) {
            sVar9 = sVar9 + 1;
            local_58[0] = sVar9;
          }
        }
      }
      local_54[0] = sVar10;
      local_50[0] = sVar11;
    }
    puVar4 = *(undefined8 **)(param_1 + 0x18);
    if (puVar4 == *(undefined8 **)(param_1 + 0x20)) {
      FUN_140ceebc0(param_1 + 0x10,puVar4,local_res10,local_50,local_54,local_58);
    }
    else {
      *(uint *)(puVar4 + 1) = local_res10[0];
      *puVar4 = &DAT_143df1ba0;
      *(short *)(puVar4 + 2) = local_58[0];
      *(short *)((longlong)puVar4 + 0xc) = sVar11;
      *(short *)((longlong)puVar4 + 0xe) = sVar10;
      *(longlong *)(param_1 + 0x18) = *(longlong *)(param_1 + 0x18) + 0x18;
    }
    return;
  }
  return;
}

