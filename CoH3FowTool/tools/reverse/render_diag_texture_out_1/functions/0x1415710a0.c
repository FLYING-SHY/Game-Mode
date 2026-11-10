// rea decompile_all
// name: FUN_1415710a0
// address: 0x1415710a0
// body: available
// range: 0x1415710a0-0x141571235 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


longlong * FUN_1415710a0(longlong param_1,int param_2,uint param_3,uint param_4,uint *param_5)

{
  longlong lVar1;
  longlong *plVar2;
  longlong *plVar3;
  uint uVar4;
  longlong *plVar5;
  longlong *plVar6;
  ushort local_res8 [4];
  undefined1 local_38 [16];
  
  lVar1 = *(longlong *)(param_1 + 0x78);
  plVar5 = (longlong *)0x0;
  if ((*(byte *)(param_1 + 8) & 8) == 0) {
LAB_141571180:
    plVar2 = plVar5;
    plVar6 = plVar5;
    if (param_3 != 0) {
      do {
        plVar2 = *(longlong **)(param_1 + 0x80);
        if (lVar1 == 0) {
          return (longlong *)0x25;
        }
        uVar4 = 0x103;
        if (plVar2 != (longlong *)0x0) {
          uVar4 = 0;
        }
        plVar3 = plVar5;
        if (((uVar4 | param_4) & 1) == 0) {
          plVar3 = plVar2;
        }
        if ((plVar3 != (longlong *)0x0) && (*plVar2 != *(longlong *)(lVar1 + 8))) {
          return (longlong *)0x23;
        }
        plVar2 = (longlong *)func_0x000141579290(lVar1,plVar3,(int)plVar6 + param_2);
        if ((int)plVar2 != 0) {
          return plVar2;
        }
        if ((param_4 & 0x10) == 0) {
          uVar4 = *(uint *)(lVar1 + 0x50);
        }
        else {
          uVar4 = *(uint *)(lVar1 + 0x54);
        }
        *param_5 = uVar4;
        uVar4 = (int)plVar6 + 1;
        plVar6 = (longlong *)(ulonglong)uVar4;
        param_5 = param_5 + 1;
      } while (uVar4 < param_3);
    }
  }
  else {
    if ((param_4 & 0x10) == 0) {
      if (*(short *)(param_1 + 0x15a) == 0) goto LAB_141571180;
      if (param_3 != 0) {
        do {
          (**(code **)(*(longlong *)(param_1 + 0x2c0) + 0xe0))
                    (param_1,0,(int)plVar5 + param_2,local_38,local_res8);
          uVar4 = (int)plVar5 + 1;
          plVar5 = (longlong *)(ulonglong)uVar4;
          *param_5 = (uint)local_res8[0];
          param_5 = param_5 + 1;
        } while (uVar4 < param_3);
      }
    }
    else {
      if (*(char *)(param_1 + 400) == '\0') goto LAB_141571180;
      if (param_3 != 0) {
        do {
          (**(code **)(*(longlong *)(param_1 + 0x2c0) + 0xe0))
                    (param_1,1,(int)plVar5 + param_2,local_38,local_res8);
          uVar4 = (int)plVar5 + 1;
          plVar5 = (longlong *)(ulonglong)uVar4;
          *param_5 = (uint)local_res8[0];
          param_5 = param_5 + 1;
        } while (uVar4 < param_3);
        return (longlong *)0x0;
      }
    }
    plVar2 = (longlong *)0x0;
  }
  return plVar2;
}

