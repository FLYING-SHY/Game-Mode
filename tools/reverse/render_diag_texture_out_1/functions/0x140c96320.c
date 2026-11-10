// rea decompile_all
// name: FUN_140c96320
// address: 0x140c96320
// body: available
// range: 0x140c96320-0x140c96498 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */
/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_140c96320(longlong param_1)

{
  code *pcVar1;
  char cVar2;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_f8 [32];
  undefined8 local_d8;
  undefined4 local_d0;
  undefined4 local_cc;
  longlong local_c8;
  undefined8 *local_a0;
  longlong local_98;
  longlong local_90;
  longlong *local_60;
  undefined1 local_58 [64];
  ulonglong local_18;
  
  local_18 = DAT_145df0fc0 ^ (ulonglong)auStack_f8;
  LOCK();
  *(undefined1 *)(param_1 + 0x168) = 0;
  UNLOCK();
  if ((DAT_146b02cd0 != '\0') && (_DAT_146b02cc8 != 0)) {
    cVar2 = func_0x0001402488a0();
    if (cVar2 != '\0') {
      local_90 = param_1 + 0x170;
      LOCK();
      *(undefined1 *)(param_1 + 0x180) = 0;
      UNLOCK();
      local_98 = 0x145417968;
      local_60 = &local_98;
      local_d8 = 0x1454178f8;
      local_d0 = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x8c);
      local_cc = *(undefined4 *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40);
      local_a0 = &local_d8;
      local_c8 = param_1;
      (*_DAT_143d56238)(local_58,0x40,0x143de6940,0x3f);
      func_0x0001401f32b0(local_58,&local_d8,&local_98);
      func_0x0001401f5ea0(&local_d8);
      if (local_60 != (longlong *)0x0) {
        pcVar1 = *(code **)(*local_60 + 0x20);
        if (pcVar1 == (code *)0x1403b6430) {
          if (local_60 != &local_98) {
                    /* WARNING: Bad instruction - Truncating control flow here */
            halt_baddata();
          }
        }
        else if ((pcVar1 == (code *)0x1402f0800) || (pcVar1 == (code *)0x1401f3680)) {
          if (local_60 != &local_98) {
                    /* WARNING: Bad instruction - Truncating control flow here */
            halt_baddata();
          }
        }
        else {
          (*pcVar1)();
        }
      }
    }
  }
  func_0x0001422ba6e0(local_18 ^ (ulonglong)auStack_f8);
  return;
}

