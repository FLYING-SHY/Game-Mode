// rea decompile_all
// name: FUN_140c96950
// address: 0x140c96950
// body: available
// range: 0x140c96950-0x140c969c5 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_140c96950(void)

{
  longlong lVar1;
  undefined4 *puVar2;
  undefined1 auStack_58 [40];
  undefined4 local_30;
  undefined4 uStack_2c;
  undefined4 *puStack_28;
  undefined8 local_20;
  ulonglong local_18;
  
  local_18 = DAT_145df0fc0 ^ (ulonglong)auStack_58;
  lVar1 = func_0x0001403b2dc0();
  if (*(char *)(lVar1 + 0x20) != '\0') {
    puVar2 = (undefined4 *)
             func_0x0001403b2e30(*(undefined8 *)
                                  (lVar1 + 0x28 + (ulonglong)*(uint *)(lVar1 + 0x24) * 8),0x20);
    local_30 = 0x393a85b8;
    puStack_28 = puVar2 + 6;
    local_20 = 0x14105a1e0;
    *puVar2 = 0x393a85b8;
    puVar2[1] = uStack_2c;
    *(undefined4 **)(puVar2 + 2) = puStack_28;
    *(undefined8 *)(puVar2 + 4) = 0x14105a1e0;
    func_0x0001422ba6e0(local_18 ^ (ulonglong)auStack_58);
    return;
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

