// rea decompile_all
// name: FUN_14060f790
// address: 0x14060f790
// body: available
// range: 0x14060f790-0x14060f8f0 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_14060f790(float *param_1,float param_2,float *param_3,float param_4,longlong param_5,
                  float *param_6)

{
  float fVar1;
  float fVar2;
  char cVar3;
  undefined1 auStack_b8 [48];
  undefined8 local_88;
  float local_80;
  undefined8 local_7c;
  float local_74;
  undefined4 local_70;
  undefined8 local_6c;
  float local_64;
  ulonglong local_60;
  
  local_60 = DAT_145df0fc0 ^ (ulonglong)auStack_b8;
  local_88 = *(undefined8 *)param_1;
  local_80 = param_1[2];
  local_7c = *(undefined8 *)param_3;
  local_74 = param_3[2];
  local_70 = 0;
  local_6c = 0x3f800000;
  local_64 = param_4;
  cVar3 = FUN_1406107a0(&local_70,&local_88,param_6);
  if (cVar3 == '\0') {
    fVar1 = param_3[2];
    fVar2 = param_1[2];
    *param_6 = param_2 * *param_3 + *param_1;
    param_6[2] = param_2 * fVar1 + fVar2;
    param_6[1] = param_4;
  }
  if ((*(longlong *)(param_5 + 0x38) != 0) &&
     (param_2 < SQRT((param_6[1] - param_1[1]) * (param_6[1] - param_1[1]) +
                     (*param_6 - *param_1) * (*param_6 - *param_1) +
                     (param_6[2] - param_1[2]) * (param_6[2] - param_1[2])))) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  func_0x0001422ba6e0(local_60 ^ (ulonglong)auStack_b8);
  return;
}

