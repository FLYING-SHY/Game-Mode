from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]
path = root / 'src' / 'Program.cs'
s = path.read_text(encoding='utf-8-sig')

def section(start, end, replacement):
    global s
    begin = s.index(start)
    finish = s.index(end, begin)
    s = s[:begin] + replacement + '\n\n' + s[finish:]

s = s.replace('case "reveal": return CmdReveal();', 'case "reveal": return CmdReveal();\n                case "hide": return CmdDisplay(false);')
section('        static void Usage()', '        static PeImage LoadExe()', '''        static void Usage()
        {
            Console.WriteLine("CoH3FowTool 2 - 本地显示修复版 (build 26443338)");
            Console.WriteLine("  CoH3FowTool.exe [--exe <RelicCoH3.exe>] <命令>");
            Console.WriteLine("    reveal / apply  开启本地地形与实体显示，并回读显示状态");
            Console.WriteLine("    hide            关闭本地显示开关，保留可再次开启的代码补丁");
            Console.WriteLine("    status          查看版本、六处显示补丁与运行时显示状态");
            Console.WriteLine("    revert          关闭显示，并恢复本版六处和旧版五处代码");
            Console.WriteLine("    watch           每两秒检查并开启显示，Ctrl+C 结束");
            Console.WriteLine("    selftest        在临时副本验证定位、文件写入和完整回滚");
            Console.WriteLine("    file-apply      高级：只将显示代码补丁写入磁盘，保留 .bak");
            Console.WriteLine("    file-revert     高级：从 .bak 恢复磁盘文件");
            Console.WriteLine("    launch-opts     兼容命令：--add / --remove 管理 Steam 的 -dev");
            Console.WriteLine("    menu            交互菜单 (默认)");
            Console.WriteLine("进入对局后运行 reveal；此路径无需控制台或 -dev。");
            Console.WriteLine("已实测本地遭遇战画面；公开匹配/天梯尚未实测。");
        }''')
section('        static int CmdApply(bool watch)', '        // ------------------------------------------------------------- reveal', '''        static int CmdApply(bool watch)
        {
            if (!watch) return CmdReveal();
            Console.WriteLine("每两秒检查本地显示；Ctrl+C 结束。");
            int previous = -1;
            while (true)
            {
                int result = CmdDisplay(true, false);
                if (result != previous)
                    Console.WriteLine(result == 0 ? "显示状态已开启。" : "等待游戏对局与匹配的显示布局。");
                previous = result;
                Thread.Sleep(2000);
            }
        }''')
section('        static int CmdReveal()', '        // ------------------------------------------------------------- revert', '''        static int CmdReveal() => CmdDisplay(true);

        static ProcessMemory OpenGame()
        {
            var pm = ProcessMemory.Open(GameProcess);
            try
            {
                string running = Path.GetFullPath(pm.Target.MainModule.FileName);
                if (!string.Equals(running, Path.GetFullPath(_exePath), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("运行中的游戏与 --exe 指定的文件不是同一路径。");
                return pm;
            }
            catch { pm.Dispose(); throw; }
        }

        static int CmdDisplay(bool enabled, bool verbose = true)
        {
            try
            {
                var pe = LoadExe();
                RuntimeState.ValidateLayout(pe);
                using var pm = OpenGame();
                var before = FowDisplay.Read(pm); // Require a live presentation object first.
                var sites = FowDisplay.Sites(pe);
                var legacy = PatchCatalog.BuildLegacy();
                foreach (var site in legacy)
                {
                    site.Locate(pe);
                    if (!site.IsUsable) throw new InvalidOperationException("旧版兼容点未匹配: " + site.Name);
                }
                FowDisplay.CheckCode(pm, pe, sites.Concat(legacy));
                // Earlier builds changed PCMD and telemetry. Recognize and undo
                // those bytes; the new display path never applies them.
                FowDisplay.SetCode(pm, pe, legacy, false);
                if (enabled) FowDisplay.SetCode(pm, pe, sites, true);
                FowDisplay.SetFlags(pm, enabled);
                var after = FowDisplay.Wait(pm, enabled);
                if (verbose)
                {
                    Console.WriteLine($"PID {pm.Target.Id} | 本地显示 {(enabled ? "开启" : "关闭")}");
                    Console.WriteLine("操作前: " + before);
                    Console.WriteLine("状态回读: " + after);
                    Console.WriteLine(enabled ? "六处显示代码与地形/实体开关已确认。" : "显示开关与渲染缓存均已关闭。");
                }
                return 0;
            }
            catch (Exception ex)
            {
                if (verbose) Console.WriteLine("显示操作失败: " + ex.Message);
                return 1;
            }
        }''')
section('        static int CmdRevert()', '        // ------------------------------------------------------------- file mode', '''        static int CmdRevert()
        {
            try
            {
                var pe = LoadExe();
                RuntimeState.ValidateLayout(pe);
                using var pm = OpenGame();
                var sites = PatchCatalog.Build().Concat(PatchCatalog.BuildLegacy()).ToList();
                foreach (var site in sites)
                {
                    site.Locate(pe);
                    if (!site.IsUsable) throw new InvalidOperationException("还原点未匹配: " + site.Name);
                }
                FowDisplay.CheckCode(pm, pe, sites);
                bool hasDisplay;
                try { FowDisplay.Read(pm); hasDisplay = true; }
                catch (InvalidOperationException) { hasDisplay = false; }
                if (hasDisplay)
                {
                    FowDisplay.SetFlags(pm, false);
                    FowDisplay.Wait(pm, false);
                }
                else
                    pm.Write((IntPtr)(pm.ModuleBase.ToInt64() + 0x6A888A0), new byte[] { 0, 0 });
                FowDisplay.SetCode(pm, pe, sites, false);
                foreach (var site in sites)
                    Console.WriteLine($"  {site.Name,-20} 原始字节已确认");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("还原失败: " + ex.Message);
                return 1;
            }
        }

        static bool IsTargetExeRunning()
        {
            foreach (var process in Process.GetProcessesByName(GameProcess))
            {
                using (process)
                {
                    try
                    {
                        if (string.Equals(Path.GetFullPath(process.MainModule.FileName),
                            Path.GetFullPath(_exePath), StringComparison.OrdinalIgnoreCase)) return true;
                    }
                    catch { return true; }
                }
            }
            return false;
        }''')
s = s.replace('if (Process.GetProcessesByName(GameProcess).Length > 0)', 'if (IsTargetExeRunning())')
s = s.replace('var orig = s.OriginalBytes(pe.Data);', 'var orig = s.ExpectedOriginalBytes();')
s = s.replace('Company of Heroes 3 - 战争迷雾 / 作弊门 补丁工具', 'Company of Heroes 3 - 本地迷雾显示修复版')
s = s.replace('流程: 打补丁 -> 进游戏 -> Control+Shift+` 打开控制台 -> 输入 cheat_revealall', '流程: 进入对局 -> reveal 开启本地显示；hide 关闭；revert 恢复代码。')
needle='''                    foreach (var s in sites.Where(x => x.IsUsable))'''
s=s.replace(needle, '''                    try { Console.WriteLine("本地显示状态: " + FowDisplay.Read(pm)); }
                    catch (Exception ex) { Console.WriteLine("显示对象尚未就绪: " + ex.Message); }
''' + needle)
s=s.replace('using var pm = ProcessMemory.Open(GameProcess);', 'using var pm = OpenGame();')
# This replacement deliberately leaves the implementation inside OpenGame intact.
s=s.replace('''            var sites = LocateAll(pe, true).Where(s => s.IsUsable).ToList();
            if (sites.Count == 0) { Console.WriteLine("没有可用补丁点."); return 1; }''', '''            try { RuntimeState.ValidateLayout(pe); }
            catch (Exception ex) { Console.WriteLine(ex.Message); return 1; }
            var sites = LocateAll(pe, true);
            if (sites.Any(s => !s.IsUsable)) { Console.WriteLine("显示补丁未全部匹配，已停止。"); return 1; }''')
path.write_text(s,encoding='utf-8')

catalog = root / 'src' / 'PatchCatalog.cs'
c = catalog.read_text(encoding='utf-8-sig')
c, count = re.subn(r'        /// <summary>\n        /// 全部针对.*?        /// </summary>',
    '        /// <summary>旧版五处修改的识别表，仅用于恢复原始代码；默认显示路径不应用此表。</summary>',
    c, count=1, flags=re.S)
assert count == 1
c = c.replace('赛后遥测 used_cheats 强制为 false (不上报用过作弊)', '旧版 used_cheats 修改的兼容还原点')
c = c.replace('赛后遥测 debug_used 强制为 false (不上报用过 dev 模式)', '旧版 debug_used 修改的兼容还原点')
catalog.write_text(c,encoding='utf-8')
print('Updated display commands, path-aware file guard, full-batch validation, and legacy comments.')
