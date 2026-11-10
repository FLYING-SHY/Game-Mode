// rea decompile_all
// name: entry
// address: 0x1439091fc
// body: available
// range: 0x1439091fc-0x143909290 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b537ee6fdad293e1c8ea50e7b50d0b6b22972a83032bca093d279ab7e770ce4
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void entry(longlong param_1)

{
  longlong lVar1;
  char cVar2;
  undefined8 uVar3;
  ulonglong uVar4;
  longlong unaff_GS_OFFSET;
  undefined1 local_res10 [8];
  undefined8 local_res18;
  undefined8 local_res20;
  
  cVar2 = func_0x000143908b6c();
  if (cVar2 != '\0') {
    local_res18 = *(undefined8 *)(param_1 + 8);
    uVar4 = (ulonglong)_DAT_145e0becc;
    lVar1 = *(longlong *)
             ((ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8 + 0x146185e00
             );
    local_res20 = 0;
    uVar3 = func_0x0001432c525c(local_res10,0x83,0,&local_res18,&local_res18,&local_res20);
    func_0x0001404820c0(lVar1 + uVar4,uVar3);
    func_0x000140482770(local_res10);
  }
  return;
}

