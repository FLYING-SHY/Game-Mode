using System;
using System.Collections.Generic;
using System.Linq;

namespace CoH3FowTool
{
    /// <summary>One byte-level edit inside a located region.</summary>
    internal sealed class Edit
    {
        public int Offset;          // offset relative to the located region start
        public byte[] Bytes;        // replacement bytes

        public Edit(int offset, string hex)
        {
            Offset = offset;
            Bytes = hex.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => Convert.ToByte(s, 16)).ToArray();
        }
    }

    internal enum SiteState
    {
        Original,
        Patched,
        NotFound,
        Ambiguous,
        BadSection,
        VersionMismatch
    }

    /// <summary>
    /// 一个补丁点 = 一个"稳定锚点 + 相对偏移的改写"。
    ///
    /// 为什么不直接拿被改的那几条指令当签名: 打上补丁之后签名本身就消失了,
    /// 无法区分"已打好"和"版本不对"。所以锚点选在补丁点附近、不受补丁影响、
    /// 且在整个 exe 里只出现一次的字节串, 再用固定的相对偏移去改目标。
    /// 这样同一套代码既能定位原始状态, 也能在打完补丁后复检。
    ///
    /// 另外每个补丁点都记着"这个版本下应该长什么样"的原始字节 (ExpectedOriginal),
    /// 用来识别游戏更新后二进制变了 —— 那种情况下宁可不动, 也不能盲改。
    /// </summary>
    internal sealed class PatchSite
    {
        public string Name;
        public string Why;
        public string AnchorHex;
        public long AnchorToTarget;      // 目标区域相对锚点起始的偏移
        public int TargetLength;         // 目标区域长度
        public string ExpectedOriginalHex;   // 本版本下的原始字节 (可空)
        public List<Edit> Edits = new List<Edit>();

        public Pattern Anchor;
        public SiteState State = SiteState.NotFound;
        public string Error;
        public long AnchorOffset = -1;   // 文件偏移
        public long TargetOffset = -1;   // 文件偏移
        public long Rva = -1;            // 目标区域的 RVA

        public PatchSite(string name, string anchorHex, long anchorToTarget, int targetLength, string why, params Edit[] edits)
            : this(name, anchorHex, anchorToTarget, targetLength, null, why, edits)
        {
        }

        public PatchSite(string name, string anchorHex, long anchorToTarget, int targetLength, string expectedOriginalHex, string why, params Edit[] edits)
        {
            Name = name;
            AnchorHex = anchorHex;
            AnchorToTarget = anchorToTarget;
            TargetLength = targetLength;
            ExpectedOriginalHex = expectedOriginalHex;
            Why = why;
            Anchor = new Pattern(anchorHex);
            Edits.AddRange(edits);
        }

        /// <summary>这个补丁点在当前状态下能不能用 (原版 / 已补 都行)。</summary>
        public bool IsUsable => State == SiteState.Original || State == SiteState.Patched;

        public byte[] OriginalBytes(byte[] data)
        {
            var res = new byte[TargetLength];
            Array.Copy(data, TargetOffset, res, 0, TargetLength);
            return res;
        }

        public byte[] DesiredBytes(byte[] data)
        {
            var res = OriginalBytes(data);
            foreach (var e in Edits)
            {
                if (e.Offset < 0 || e.Offset + e.Bytes.Length > res.Length)
                    throw new InvalidOperationException(Name + ": edit out of range");
                Array.Copy(e.Bytes, 0, res, e.Offset, e.Bytes.Length);
            }
            return res;
        }

        public byte[] ExpectedOriginalBytes()
        {
            if (string.IsNullOrEmpty(ExpectedOriginalHex)) return null;
            return ExpectedOriginalHex.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => Convert.ToByte(s, 16)).ToArray();
        }

        public bool Locate(PeImage pe)
        {
            Error = null;
            State = SiteState.NotFound;
            Rva = -1;
            AnchorOffset = -1;
            TargetOffset = -1;

            var hits = Anchor.FindAll(pe.Data);
            if (hits.Count == 0)
            {
                Error = "锚点未命中 (游戏版本已更新?)";
                return false;
            }
            if (hits.Count > 1)
            {
                State = SiteState.Ambiguous;
                Error = $"锚点重复 {hits.Count} 次";
                return false;
            }

            AnchorOffset = hits[0];
            TargetOffset = AnchorOffset + AnchorToTarget;
            if (TargetOffset < 0 || TargetOffset + TargetLength > pe.Data.Length)
            {
                State = SiteState.BadSection;
                Error = "目标偏移越界";
                return false;
            }

            Rva = pe.OffsetToRva(TargetOffset);
            if (Rva < 0)
            {
                State = SiteState.BadSection;
                Error = "目标不在映射段内";
                return false;
            }
            if (pe.SectionOfRva(Rva) != ".text")
            {
                State = SiteState.BadSection;
                Error = "目标不在 .text 内 (" + pe.SectionOfRva(Rva) + ")";
                return false;
            }

            var cur = OriginalBytes(pe.Data);
            var want = DesiredBytes(pe.Data);
            if (cur.SequenceEqual(want))
            {
                State = SiteState.Patched;
                return true;
            }

            var expected = ExpectedOriginalBytes();
            if (expected != null && expected.Length == TargetLength && !cur.SequenceEqual(expected))
            {
                State = SiteState.VersionMismatch;
                Error = "目标字节与已知版本不符 (期望 " + ExpectedOriginalHex + ", 实际 " +
                        BitConverter.ToString(cur).Replace('-', ' ') + ")";
                return true;
            }

            State = SiteState.Original;
            return true;
        }
    }

    internal static class PatchCatalog
    {
        public const string ExeName = "RelicCoH3.exe";
        public const string VerifiedBuild = "26443338";

        // Local presentation readers. Each branch skips a UI reveal flag when
        // the match-mode gate is false. NOP preserves the flag and all normal
        // update queues; it does not change the simulation or telemetry.
        public static List<PatchSite> Build() => new List<PatchSite>
        {
            new PatchSite("TerrainFrame",
                "20 5B C3 B0 01 EB F6 CC CC CC CC CC CC CC CC CC 40 53 48 83 EC 20 48 8B D9 E8 B2 05 00 00 84 C0",
                32, 2, "74 09", "每帧地形显示读取保留 UI 开关", new Edit(0, "90 90")),
            new PatchSite("EntityDisplay",
                "8B DA 48 8B F1 E8 A1 ED F9 FF 84 C0",
                12, 2, "74 0D", "实体显示读取保留 UI 开关", new Edit(0, "90 90")),
            new PatchSite("VisibilityA",
                "84 BA 00 00 00 E8 BA 0F 00 00 84 C0",
                12, 2, "74 09", "本地可见性读取 A", new Edit(0, "90 90")),
            new PatchSite("VisibilityB",
                "8B F2 48 8B E9 E8 60 05 00 00 84 C0",
                12, 2, "74 0D", "本地可见性读取 B", new Edit(0, "90 90")),
            new PatchSite("VisibilityC",
                "41 C6 04 24 00 E8 BF 01 00 00 84 C0",
                12, 2, "74 0D", "本地可见性读取 C", new Edit(0, "90 90")),
            new PatchSite("VisibilityD",
                "84 88 00 00 00 E8 7C E6 FF FF 84 C0",
                12, 2, "74 09", "本地可见性读取 D", new Edit(0, "90 90")),
        };

        // Kept only to recognize and restore the earlier version's edits.
        // BuildLegacy is never applied by the repaired reveal/apply commands.

        /// <summary>旧版五处修改的识别表，仅用于恢复原始代码；默认显示路径不应用此表。</summary>
        public static List<PatchSite> BuildLegacy()
        {
            // CheatGate: 门函数尾部, 锚点止于被改的 17 字节之前
            const string gateAnchor = "49 03 C8 E8 75 3D 88 FC 4C 8B D0";

            // cheat_revealall handler: call 门 + test al,al, 紧跟其后就是条件跳转
            // 锚点必须止于 84 C0 (test al,al), 不能把 74 7D 包进来, 否则打完补丁锚点就消失了
            const string revealAnchor = "40 53 48 83 EC 30 4C 8B D9 E8 62 F9 FF FF 84 C0";

            // DoCommand 可信来源校验, 锚点止于 41 80 78 60 00 之后的条件跳转之前
            const string trustAnchor = "4D 8B 84 C1 00 5E 18 06 4C 03 C1 41 80 78 60 00";

            // 遥测读取点之后的 lea rdx,"used_cheats" + mov rcx + call SetBoolStat
            // 锚点整体在被改字节 (0x1D8581..0x1D8584) 之后, 打完补丁依然能复检。
            const string usedCheatsAnchor = "C6 44 24 20 00 4C 8D 0D CB FB BB 03 48 8D 15 C0 98 D1 03 49 8B CE E8 A0 FB 14 00";

            // 同上, 对应 "debug_used"
            const string debugUsedAnchor = "C6 44 24 20 00 4C 8D 0D A2 FB BB 03 48 8D 15 B7 98 D1 03 49 8B CE E8 77 FB 14 00";

            return new List<PatchSite>
            {
                new PatchSite(
                    "CheatGate",
                    gateAnchor,
                    0x0B, 17,
                    "33 C0 4D 85 D2 74 0A 41 38 82 80 06 00 00 0F 94 C0",
                    "作弊运行时门 -> 恒返回 1 (解锁全部 cheat_* 命令)",
                    new Edit(0, "B8 01 00 00 00 90 90 90 90 90 90 90 90 90 90 90 90")),

                new PatchSite(
                    "RevealAllGate",
                    revealAnchor,
                    0x10, 2,
                    "74 7D",
                    "cheat_revealall: 去掉 je 分支 (nop nop), 揭示逻辑必定执行",
                    new Edit(0, "90 90")),

                new PatchSite(
                    "DoCommandTrust",
                    trustAnchor,
                    0x10, 2,
                    "74 0E",
                    "DoCommand 黑名单判定: je -> nop, 无条件走执行路径 (跳过 12 项黑名单)",
                    new Edit(0, "90 90")),

                new PatchSite(
                    "TelemetryUsedCheats",
                    usedCheatsAnchor,
                    -4, 4,
                    "4D 0F 45 C4",
                    "旧版 used_cheats 修改的兼容还原点",
                    new Edit(0, "90 90 90 90")),

                new PatchSite(
                    "TelemetryDebugUsed",
                    debugUsedAnchor,
                    -4, 4,
                    "4D 0F 45 C4",
                    "旧版 debug_used 修改的兼容还原点",
                    new Edit(0, "90 90 90 90")),
            };
        }
    }
}
