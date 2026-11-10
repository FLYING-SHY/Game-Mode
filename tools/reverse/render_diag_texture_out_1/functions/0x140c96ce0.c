// rea decompile_all
// name: FUN_140c96ce0
// address: 0x140c96ce0
// body: available
// range: 0x140c96ce0-0x140c96de7 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_140c96ce0(float param_1,float param_2,float *param_3,float *param_4,ushort param_5,
                  ushort param_6,longlong *param_7)

{
  short *psVar1;
  longlong lVar2;
  ushort uVar3;
  ushort uVar4;
  longlong lVar5;
  float fVar6;
  float fVar7;
  
  fVar7 = 0.0;
  do {
    fVar6 = 0.0;
    do {
      uVar3 = (ushort)(int)((fVar7 * param_3[3] + param_3[2]) * fVar6 +
                           fVar7 * param_3[1] + *param_3);
      uVar4 = (ushort)(int)((fVar7 * param_4[3] + param_4[2]) * fVar6 +
                           fVar7 * param_4[1] + *param_4);
      if (param_6 < uVar4) {
        uVar4 = param_6;
      }
      if (param_5 < uVar3) {
        uVar3 = param_5;
      }
      lVar5 = (ulonglong)uVar4 * *(longlong *)(*param_7 + 0x30) + (ulonglong)uVar3;
      lVar2 = *(longlong *)(*param_7 + 0x28);
      if (param_7[3] == _DAT_146b8e6d8) {
        psVar1 = (short *)(lVar2 + 2 + lVar5 * 8);
        *psVar1 = *psVar1 + 1;
      }
      else if (param_7[3] == _DAT_146b8e6e0) {
        psVar1 = (short *)(lVar2 + lVar5 * 8);
        *psVar1 = *psVar1 + 1;
      }
      fVar6 = fVar6 + 1.0 / param_2;
    } while (fVar6 < 1.0);
    fVar7 = fVar7 + 1.0 / param_1;
  } while (fVar7 < 1.0);
  return;
}

