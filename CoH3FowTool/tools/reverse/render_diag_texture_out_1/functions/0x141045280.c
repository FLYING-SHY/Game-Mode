// rea decompile_all
// name: FUN_141045280
// address: 0x141045280
// body: available
// range: 0x141045280-0x141045342 inclusive
// non_contiguous: false
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


longlong * FUN_141045280(longlong *param_1,longlong *param_2)

{
  longlong *plVar1;
  uint uVar2;
  longlong lVar3;
  longlong lVar4;
  longlong *plVar5;
  longlong *plVar6;
  longlong lVar7;
  
  if ((param_1 != param_2) && (plVar1 = param_1 + 1, plVar1 != param_2)) {
    lVar7 = 8;
    plVar6 = plVar1;
    do {
      lVar3 = *plVar6;
      uVar2 = *(uint *)(*param_1 + 0x4c);
      if ((*(uint *)(lVar3 + 0x4c) < uVar2) ||
         ((plVar5 = plVar6, *(uint *)(lVar3 + 0x4c) == uVar2 &&
          (*(uint *)(lVar3 + 0x50) < *(uint *)(*param_1 + 0x50))))) {
        func_0x0001422bddaf(plVar1,param_1,lVar7);
        *param_1 = lVar3;
      }
      else {
        while( true ) {
          lVar4 = plVar5[-1];
          if ((*(uint *)(lVar4 + 0x4c) <= *(uint *)(lVar3 + 0x4c)) &&
             ((*(uint *)(lVar3 + 0x4c) != *(uint *)(lVar4 + 0x4c) ||
              (*(uint *)(lVar4 + 0x50) <= *(uint *)(lVar3 + 0x50))))) break;
          *plVar5 = lVar4;
          plVar5 = plVar5 + -1;
        }
        *plVar5 = lVar3;
      }
      plVar6 = plVar6 + 1;
      lVar7 = lVar7 + 8;
    } while (plVar6 != param_2);
  }
  return param_2;
}

