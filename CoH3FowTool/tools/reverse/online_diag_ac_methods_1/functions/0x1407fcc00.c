// rea decompile_all
// name: FUN_1407fcc00
// address: 0x1407fcc00
// body: available
// range: 0x1407fcc00-0x1407fcd8a inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Control flow encountered bad instruction data */

void FUN_1407fcc00(undefined8 param_1,longlong param_2)

{
  longlong lVar1;
  ulonglong uVar2;
  ushort uVar3;
  byte bVar4;
  int iVar5;
  longlong *plVar6;
  ulonglong uVar7;
  uint uVar8;
  ulonglong uVar9;
  longlong unaff_GS_OFFSET;
  undefined1 auStack_68 [40];
  byte local_40 [4];
  undefined1 local_3c [4];
  undefined4 local_38;
  ulonglong local_30;
  
  local_30 = DAT_145df0fc0 ^ (ulonglong)auStack_68;
  uVar7 = (ulonglong)DAT_145e0becc;
  local_40[0] = 0;
  lVar1 = *(longlong *)
           (&DAT_146185e00 +
           (ulonglong)*(uint *)(**(longlong **)(unaff_GS_OFFSET + 0x58) + 0x40) * 8);
  func_0x000140664130(param_1,local_40,1);
  bVar4 = local_40[0];
  if ((local_40[0] & 0xc0) == 0x40) {
    bVar4 = local_40[0] & 0x3f;
    uVar8 = local_40[0] & 0x3f;
    if (0xff < uVar8) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    func_0x000140482e30(param_2,(ulonglong)*(uint *)(param_2 + 4) + (ulonglong)uVar8);
    if (bVar4 != 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
  }
  else {
    if ((local_40[0] & 0xc0) != 0) {
                    /* WARNING: Bad instruction - Truncating control flow here */
      halt_baddata();
    }
    func_0x000140664130(param_1,local_40,1);
    uVar3 = CONCAT11(bVar4,local_40[0]);
    func_0x000140664130(param_1,local_40,1);
    uVar8 = (uint)local_40[0];
    func_0x000140664130(param_1,local_40,1);
    uVar8 = (uint)local_40[0] | uVar8 << 8 | (uint)uVar3 << 0x10;
    uVar2 = (ulonglong)(bVar4 >> 4);
    uVar9 = uVar2 & 0xffffffffffffff03;
    iVar5 = func_0x0001407fd350(uVar2 & 3);
    plVar6 = (longlong *)
             func_0x0001406c5310(**(undefined8 **)(uVar7 + lVar1 + 0xe0),uVar9 & 0xff,
                                 (uVar8 & 0xfffffff) + iVar5);
    if (plVar6 != (longlong *)0x0) {
      lVar1 = plVar6[1];
      local_3c[0] = (**(code **)(*plVar6 + 0x18))(plVar6);
      local_38 = (int)lVar1;
      func_0x000140482d90(param_2,local_3c);
    }
  }
  func_0x0001422ba6e0(local_30 ^ (ulonglong)auStack_68);
  return;
}

