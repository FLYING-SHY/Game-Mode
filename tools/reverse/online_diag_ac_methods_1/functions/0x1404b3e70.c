// rea decompile_all
// name: FUN_1404b3e70
// address: 0x1404b3e70
// body: available
// range: 0x1404b3e70-0x1404b3ff0 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_1404b3e70(undefined8 *param_1,undefined8 param_2)

{
  undefined8 uVar1;
  undefined8 *puVar2;
  undefined4 *puVar3;
  undefined8 uVar4;
  undefined8 *local_28;
  undefined1 local_20 [8];
  undefined8 local_18;
  undefined8 *local_10;
  
  *param_1 = 0x143dd8d48;
  param_1[1] = 0;
  param_1[2] = 0;
  param_1[3] = 0;
  *(undefined4 *)(param_1 + 4) = 0;
  *(undefined4 *)((longlong)param_1 + 0x24) = 0xffffffff;
  param_1[5] = 0;
  param_1[6] = 0;
  param_1[7] = 0;
  param_1[8] = 0;
  param_1[9] = 0;
  param_1[10] = 0;
  param_1[0xb] = 0;
  param_1[0xc] = 0;
  param_1[0xd] = 0;
  param_1[0xe] = 0;
  param_1[0xf] = 0;
  param_1[0x10] = 0;
  param_1[0x11] = 0;
  *(undefined4 *)(param_1 + 0x12) = 0;
  *(undefined4 *)((longlong)param_1 + 0x94) = 0xffffffff;
  param_1[0x13] = 0;
  param_1[0x14] = 0;
  param_1[0x15] = 0;
  param_1[0x16] = 0;
  param_1[0x17] = 0;
  param_1[0x18] = 0;
  param_1[0x19] = 0;
  *(undefined4 *)(param_1 + 0x1a) = 0xffffffff;
  param_1[0x1b] = param_2;
  local_10 = param_1;
  puVar2 = (undefined8 *)func_0x0001422ba9a8(0x90);
  *puVar2 = 0;
  puVar2[1] = 0;
  puVar2[2] = 0;
  puVar2[3] = 0;
  puVar2[4] = 0;
  puVar2[5] = 0;
  puVar2[6] = 0;
  puVar2[7] = 0;
  puVar2[8] = 0;
  puVar2[9] = 0;
  puVar2[10] = 0;
  puVar2[0xb] = 0;
  puVar2[0xc] = 0;
  puVar2[0xd] = 0;
  puVar2[0xe] = 0;
  puVar2[0xf] = 0;
  puVar2[0x10] = 0;
  puVar2[0x11] = 0;
  local_28 = puVar2;
  func_0x0001404b4360(puVar2,param_1);
  *puVar2 = &DAT_143d7d368;
  local_18 = 0;
  local_28 = puVar2;
  puVar3 = (undefined4 *)func_0x0001404b40c0(param_1,local_20,&local_28);
  *(undefined4 *)(param_1 + 0x1a) = *puVar3;
  if (local_28 == (undefined8 *)0x0) {
    uVar1 = param_1[0x1b];
    uVar4 = func_0x0001422ba9a8(200);
    local_18 = uVar4;
    func_0x0001422bddb5(uVar4,0,200);
    FUN_1404b3a40(uVar4,uVar1,param_1);
                    /* WARNING: Bad instruction - Truncating control flow here */
    halt_baddata();
  }
                    /* WARNING: Bad instruction - Truncating control flow here */
  halt_baddata();
}

