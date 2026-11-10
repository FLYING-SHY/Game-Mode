// rea decompile_all
// name: FUN_1406107a0
// address: 0x1406107a0
// body: available
// range: 0x1406107a0-0x140610896 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

undefined1 FUN_1406107a0(float *param_1,float *param_2,float *param_3)

{
  float fVar1;
  float fVar2;
  float fVar3;
  float fVar4;
  float fVar5;
  
  fVar1 = param_2[4];
  fVar2 = param_2[5];
  fVar5 = fVar1 * param_1[1] + param_2[3] * *param_1 + fVar2 * param_1[2];
  if (_DAT_144feab20 <= ABS(fVar5)) {
    fVar3 = param_2[1];
    fVar4 = param_2[2];
    fVar5 = (fVar3 * param_1[1] + *param_2 * *param_1 + fVar4 * param_1[2] + param_1[3]) *
            (-1.0 / fVar5);
    if (0.0 <= fVar5) {
      *param_3 = param_2[3] * fVar5 + *param_2;
      param_3[1] = fVar1 * fVar5 + fVar3;
      param_3[2] = fVar2 * fVar5 + fVar4;
      return 1;
    }
  }
  return 0;
}

