// rea decompile_all
// name: FUN_14017cef0
// address: 0x14017cef0
// body: available
// range: 0x14017cef0-0x14017cffe inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Removing unreachable block (ram,0x00014017d171) */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_14017cef0(undefined8 param_1,longlong param_2,undefined8 param_3,char param_4,
                  longlong param_5)

{
  char cVar1;
  longlong *plVar2;
  char *pcVar3;
  undefined1 auStack_e8 [48];
  undefined8 local_b8;
  undefined1 local_b0 [120];
  ulonglong local_38;
  ulonglong local_30;
  
  local_30 = DAT_145df0fc0 ^ (ulonglong)auStack_e8;
  local_38 = local_38 & 0xffffffff00000000;
  plVar2 = (longlong *)func_0x000140424b40(param_2,param_5 + 8);
  if (plVar2 == (longlong *)0x0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  cVar1 = (**(code **)(*plVar2 + 0x10))(plVar2);
  if (cVar1 != param_4) {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  if (*(char *)(param_5 + 0x5b) == '\0') {
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
  if (*(int *)(param_5 + 0x10) == 0) {
    local_b8 = param_1;
    plVar2 = (longlong *)func_0x00014017bfb0(local_b0,&local_b8);
    local_38 = 0;
    if (*plVar2 == 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    FUN_14017ceb0(&local_38,plVar2);
    func_0x00014017d190(local_b0);
    if (*(char *)(param_5 + 0x5b) == '\x01') {
      if (*(int *)(param_5 + 0x20) == 1) {
        pcVar3 = (char *)(*(code *)0x5bf94d4)
                                   ((*(float *)(param_5 + 0x18) / *(float *)(param_2 + 0x90)) *
                                    _DAT_144812c80);
        *pcVar3 = *pcVar3 + (char)pcVar3;
                    /* WARNING: Bad instruction - Truncating control flow here */
        halt_baddata();
      }
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

