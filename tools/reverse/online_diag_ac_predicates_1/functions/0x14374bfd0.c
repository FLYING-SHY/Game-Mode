// rea decompile_all
// name: FUN_14374bfd0
// address: 0x14374bfd0
// body: available
// range: 0x14374bfd0-0x14374c0a3 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


/* WARNING: Globals starting with '_' overlap smaller symbols at the same address */

void FUN_14374bfd0(longlong param_1)

{
  longlong lVar1;
  undefined1 auStack_78 [32];
  undefined4 local_58;
  longlong local_50;
  undefined8 ******local_48;
  undefined8 ******local_38 [3];
  ulonglong local_20;
  ulonglong local_18;
  
  local_18 = DAT_145df0fc0 ^ (ulonglong)auStack_78;
  if ((param_1 != 0) && (lVar1 = *(longlong *)(param_1 + 0x38), *(int *)(lVar1 + 0x48) != 0x130000))
  {
    func_0x000142481d14(local_38);
    local_48 = local_38;
    if (0xf < local_20) {
      local_48 = local_38[0];
    }
    local_50 = lVar1 + 0x28;
    if (0xf < *(ulonglong *)(lVar1 + 0x40)) {
      local_50 = *(longlong *)local_50;
    }
    local_58 = *(undefined4 *)(lVar1 + 0x48);
    func_0x000142478c18("E:\\anvil_BuildSystem_stable\\engine\\source\\runtime\\essence\\public\\essence/propertysystem/PropertyBagGroup.h"
                        ,0xac,
                        "SIM -- Attempting to query bag not associated with this group type.\nGroup is of type %d.  Query is for type %d.  Group name: \'%s\', Lua callstack:\n%s"
                        ,0x130000);
    _DAT_00000000 = 0;
    func_0x000140311020(local_38);
  }
  func_0x0001422ba6e0(local_18 ^ (ulonglong)auStack_78);
  return;
}

