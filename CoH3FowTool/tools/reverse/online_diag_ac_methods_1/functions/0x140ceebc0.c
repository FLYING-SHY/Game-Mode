// rea decompile_all
// name: FUN_140ceebc0
// address: 0x140ceebc0
// body: available
// range: 0x140ceebc0-0x140ceed72 inclusive
// range: 0x14292531c-0x142925434 inclusive
// non_contiguous: true
// provider: Ghidra 12.1.4 (ghidra)
// analysis_profile: 9b6938e062ee9f64f4ba8c6e1627fa211ec816b27e44f6f2a4542b02cee2d143
// limitations: The session serves operations only after Ghidra reports default auto-analysis complete; incomplete analysis does not expose partial results.; The imported Program and temporary project are ephemeral and deleted on close. Annotation operations edit session database metadata; original executable bytes are never written.; Cancelling an active Ghidra request terminates its ephemeral database and discards session annotations. The selected target remains open; the next provider query imports the original artifact again.; Default-space addresses use lowercase 0x-prefixed hexadecimal; other address spaces use <percent-encoded-space>:0x<hex>.; Ghidra Listing reports a terminal call at 0x142925329 with no resolved callee; its no-return basis is unknown.; Windows Ghidra P0 accepts native x86, x86-64, ARMNT (Thumb-2) and ARM64 PE applications and DLLs, and explicitly selected raw 6502 images; managed, hostile, sensitive, and mutable-path targets are unsupported.; The Windows bridge uses authenticated IPv4 loopback because Node path-based IPC does not expose Java AF_UNIX sockets; the endpoint file contains no bearer token.; Windows sessions require the matching packaged Windows x64 native addon, local NTFS targets and runtimes, and Windows 10 or later. Native handles enforce path admission, private DACLs, and Job Object ownership automatically.; Pseudocode is Ghidra decompiler output, not original source and not text-equivalent to Hopper output.; External functions and functions without an analyzable body return null; other decompiler failures remain explicit.
// pseudocode: provider-specific; not original source


undefined8 *
FUN_140ceebc0(longlong *param_1,undefined2 *param_2,undefined4 *param_3,undefined2 *param_4,
             undefined2 *param_5,undefined2 *param_6)

{
  ulonglong uVar1;
  undefined2 *puVar2;
  undefined8 *puVar3;
  undefined2 uVar4;
  undefined2 uVar5;
  undefined4 uVar6;
  longlong lVar7;
  undefined2 *puVar8;
  code *pcVar9;
  longlong lVar10;
  ulonglong uVar11;
  undefined8 uVar12;
  undefined8 *puVar13;
  undefined8 *puVar14;
  undefined8 *puVar15;
  longlong lVar16;
  ulonglong uVar17;
  undefined2 *puVar18;
  longlong *local_60;
  undefined8 local_58;
  ulonglong local_50;
  undefined8 *local_48;
  undefined8 *local_40;
  
  lVar7 = *param_1;
  lVar16 = ((longlong)param_2 - lVar7) / 6 + ((longlong)param_2 - lVar7 >> 0x3f);
  lVar10 = (param_1[1] - lVar7) / 0x18;
  if (lVar10 != 0xaaaaaaaaaaaaaaa) {
    uVar1 = lVar10 + 1;
    uVar11 = (param_1[2] - lVar7) / 0x18;
    uVar17 = 0xaaaaaaaaaaaaaaa;
    if ((uVar11 <= 0xaaaaaaaaaaaaaaa - (uVar11 >> 1)) &&
       (uVar17 = (uVar11 >> 1) + uVar11, uVar17 < uVar1)) {
      uVar17 = uVar1;
    }
    uVar12 = func_0x0001402aa7c0(uVar17);
    puVar13 = (undefined8 *)func_0x0001402aa7a0(uVar12);
    puVar3 = puVar13 + ((lVar16 >> 2) - (lVar16 >> 0x3f)) * 3;
    puVar14 = puVar3 + 3;
    uVar4 = *param_6;
    uVar5 = *param_5;
    uVar6 = *param_3;
    *(undefined2 *)((longlong)puVar3 + 0xc) = *param_4;
    *puVar3 = &DAT_143df1ba0;
    *(undefined4 *)(puVar3 + 1) = uVar6;
    *(undefined2 *)((longlong)puVar3 + 0xe) = uVar5;
    *(undefined2 *)(puVar3 + 2) = uVar4;
    puVar8 = (undefined2 *)param_1[1];
    puVar18 = (undefined2 *)*param_1;
    puVar15 = puVar13;
    local_60 = param_1;
    local_50 = uVar17;
    local_48 = puVar3;
    local_40 = puVar14;
    if (param_2 == puVar8) {
      for (; puVar18 != puVar8; puVar18 = puVar18 + 0xc) {
        *puVar15 = &DAT_143df1ba0;
        *(undefined4 *)(puVar15 + 1) = *(undefined4 *)(puVar18 + 4);
        *(undefined2 *)((longlong)puVar15 + 0xc) = puVar18[6];
        *(undefined2 *)((longlong)puVar15 + 0xe) = puVar18[7];
        *(undefined2 *)(puVar15 + 2) = puVar18[8];
        puVar15 = puVar15 + 3;
      }
    }
    else {
      for (; puVar18 != param_2; puVar18 = puVar18 + 0xc) {
        *puVar15 = &DAT_143df1ba0;
        *(undefined4 *)(puVar15 + 1) = *(undefined4 *)(puVar18 + 4);
        *(undefined2 *)((longlong)puVar15 + 0xc) = puVar18[6];
        *(undefined2 *)((longlong)puVar15 + 0xe) = puVar18[7];
        *(undefined2 *)(puVar15 + 2) = puVar18[8];
        puVar15 = puVar15 + 3;
      }
      func_0x0001411afd30(puVar15,puVar15);
      puVar8 = (undefined2 *)param_1[1];
      puVar15 = puVar14;
      local_48 = puVar13;
      if (param_2 != puVar8) {
        puVar18 = param_2 + 6;
        do {
          *puVar15 = &DAT_143df1ba0;
          *(undefined4 *)(puVar15 + 1) = *(undefined4 *)(puVar18 + -2);
          *(undefined2 *)((longlong)puVar15 + 0xc) = *puVar18;
          *(undefined2 *)((longlong)puVar15 + 0xe) = puVar18[1];
          *(undefined2 *)(puVar15 + 2) = puVar18[2];
          puVar15 = puVar15 + 3;
          puVar2 = puVar18 + 6;
          puVar18 = puVar18 + 0xc;
        } while (puVar2 != puVar8);
      }
    }
    func_0x0001411afd30(puVar15,puVar15);
    local_58 = 0;
    if (*param_1 != 0) {
      func_0x0001411afd30(*param_1,param_1[1]);
      func_0x00014031b310(*param_1,((param_1[2] - *param_1) / 0x18) * 0x18);
    }
    *param_1 = (longlong)puVar13;
    param_1[1] = (longlong)(puVar13 + uVar1 * 3);
    param_1[2] = (longlong)(puVar13 + uVar17 * 3);
    FUN_140cef060(&local_60);
    return puVar3;
  }
  (*(code *)0x5bf4de0)("vector too long");
  pcVar9 = (code *)swi(3);
  puVar14 = (undefined8 *)(*pcVar9)();
  return puVar14;
}

