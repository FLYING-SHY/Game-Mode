using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace CoH3FowTool
{
    internal static class Program
    {
        const string DefaultExe = @"G:\SteamLibrary\steamapps\common\Company of Heroes 3\RelicCoH3.exe";
        const string GameProcess = "RelicCoH3";

        static string _exePath = DefaultExe;

        [STAThread]
        static int Main(string[] args)
        {
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }

            var argv = new List<string>(args);
            for (int i = 0; i < argv.Count; i++)
            {
                if (argv[i] == "--exe" && i + 1 < argv.Count)
                {
                    _exePath = argv[i + 1];
                    argv.RemoveRange(i, 2);
                    break;
                }
            }

            string cmd = argv.Count > 0 ? argv[0].ToLowerInvariant() : "gui";
            switch (cmd)
            {
                case "gui":
                case "menu": return DesktopApplication.Run(_exePath);
                case "status": return CmdStatus();
                case "revert": return CmdRevert();
                case "hide": return CmdDisplay(false);
                case "file-revert": return CmdFileRevert();
                case "selftest": return CmdSelfTest();
                // The GUI is the only public path that can enable live display.
                // Do not leave older command/menu paths around the license check.
                case "apply":
                case "reveal":
                case "watch":
                case "file-apply":
                    Console.WriteLine("请在图形界面激活授权并执行开启操作。");
                    return 3;
                case "help":
                case "-h":
                case "--help": Usage(); return 0;
                default:
                    Console.WriteLine("未知命令: " + cmd);
                    Usage();
                    return 2;
            }
        }

        static void Usage()
        {
            Console.WriteLine("CoH3FowTool 2.1.0 - 中文 GUI 授权版 (build 26443338)");
            Console.WriteLine("  双击 CoH3FowTool.exe，或使用 gui 打开图形界面。");
            Console.WriteLine("  CoH3FowTool.exe [--exe <RelicCoH3.exe>] <诊断命令>");
            Console.WriteLine("    status          查看版本、代码与运行时显示状态");
            Console.WriteLine("    hide            关闭显示（无需授权）");
            Console.WriteLine("    revert          关闭显示并恢复代码（无需授权）");
            Console.WriteLine("    file-revert     从已有 .bak 恢复磁盘文件");
            Console.WriteLine("    selftest        仅在临时副本验证文件写入和完整回滚");
            Console.WriteLine("开启操作仅在 GUI 完成授权后提供。");
            Console.WriteLine("GUI 沿用 2.0 本地显示逻辑；公开匹配/天梯尚未实测。");
        }

        private static readonly object CommandLock = new();

        internal static Gui.ToolResult RunDesktopAction(Gui.ToolAction action, string gamePath)
        {
            lock (CommandLock)
            {
                string previousPath = _exePath;
                var previousOut = Console.Out;
                var previousError = Console.Error;
                using var output = new StringWriter();
                try
                {
                    Console.SetOut(output);
                    Console.SetError(output);
                    _exePath = Path.GetFullPath(gamePath.Trim().Trim('"'));
                    if (!string.Equals(Path.GetFileName(_exePath), PatchCatalog.ExeName,
                        StringComparison.OrdinalIgnoreCase))
                        throw new ArgumentException("请选择 RelicCoH3.exe 游戏主程序。");
                    int result = action switch
                    {
                        Gui.ToolAction.Status => CmdStatus(),
                        Gui.ToolAction.Enable => CmdDisplay(true),
                        Gui.ToolAction.Disable => CmdDisplay(false),
                        Gui.ToolAction.Restore => CmdRevert(),
                        _ => throw new ArgumentOutOfRangeException(nameof(action))
                    };
                    return new Gui.ToolResult(result, output.ToString());
                }
                catch (Exception ex)
                {
                    return new Gui.ToolResult(1, "操作未完成：" + ex.Message + Environment.NewLine + output);
                }
                finally
                {
                    _exePath = previousPath;
                    Console.SetOut(previousOut);
                    Console.SetError(previousError);
                }
            }
        }

        static PeImage LoadExe()
        {
            if (!File.Exists(_exePath))
                throw new FileNotFoundException("找不到游戏主程序: " + _exePath);
            return new PeImage(_exePath);
        }

        static string Hex(byte[] b) => BitConverter.ToString(b).Replace('-', ' ');

        static List<PatchSite> LocateAll(PeImage pe, bool verbose)
        {
            var sites = PatchCatalog.Build();
            foreach (var s in sites)
            {
                s.Locate(pe);
                if (!verbose) continue;

                switch (s.State)
                {
                    case SiteState.Original:
                        Console.WriteLine($"  [原始] {s.Name,-14} RVA 0x{s.Rva:X8}  file 0x{s.TargetOffset:X8}  {s.Why}");
                        break;
                    case SiteState.Patched:
                        Console.WriteLine($"  [已补] {s.Name,-14} RVA 0x{s.Rva:X8}  file 0x{s.TargetOffset:X8}  {s.Why}");
                        break;
                    case SiteState.VersionMismatch:
                        Console.WriteLine($"  [跳版] {s.Name,-14} {s.Error}");
                        break;
                    default:
                        Console.WriteLine($"  [失败] {s.Name,-14} {s.Error}");
                        break;
                }
            }
            return sites;
        }

        // ------------------------------------------------------------- status

        static int CmdStatus()
        {
            Console.WriteLine("Company of Heroes 3 - 本地迷雾显示修复版");
            Console.WriteLine("exe: " + _exePath);
            Console.WriteLine();

            PeImage pe;
            try { pe = LoadExe(); }
            catch (Exception ex) { Console.WriteLine("读取失败: " + ex.Message); return 1; }

            Console.WriteLine($"PE32+ imageBase=0x{pe.ImageBase:X}  sections={pe.Sections.Count}");
            Console.WriteLine("扫描补丁点:");
            var sites = LocateAll(pe, true);

            int ok = sites.Count(s => s.State == SiteState.Original || s.State == SiteState.Patched);
            Console.WriteLine();
            Console.WriteLine($"可用补丁点 {ok}/{sites.Count}   (验证版本 build {PatchCatalog.VerifiedBuild})");

            var proc = Process.GetProcessesByName(GameProcess);
            if (proc.Length == 0)
            {
                Console.WriteLine("游戏进程: 未运行");
            }
            else
            {
                Console.WriteLine("游戏进程: PID " + proc[0].Id);
                try
                {
                    using var pm = OpenGame();
                    Console.WriteLine($"模块基址: 0x{pm.ModuleBase.ToInt64():X}");
                    Console.WriteLine("内存补丁状态:");
                    RuntimeState.ValidateLayout(pe);
                    Console.WriteLine($"INPUT ABI: {GameInput.NativeInputSize} bytes (x64 expected 40)");
                    if (RuntimeState.TryConsole(pm, out var console, out string consoleError))
                        Console.WriteLine("控制台实际状态: " + console);
                    else
                        Console.WriteLine("控制台状态读取失败: " + consoleError);
                    try { Console.WriteLine("本地显示状态: " + FowDisplay.Read(pm)); }
                    catch (Exception ex) { Console.WriteLine("显示对象尚未就绪: " + ex.Message); }
                    foreach (var s in sites.Where(x => x.IsUsable))
                    {
                        var addr = (IntPtr)(pm.ModuleBase.ToInt64() + s.Rva);
                        var cur = pm.Read(addr, s.TargetLength);
                        var want = s.DesiredBytes(pe.Data);
                        var orig = s.ExpectedOriginalBytes();
                        string state = cur.SequenceEqual(want) ? "已打补丁"
                                     : cur.SequenceEqual(orig) ? "未打补丁"
                                     : "未知字节";
                        Console.WriteLine($"  {s.Name,-14} {state}  @0x{addr.ToInt64():X}  {Hex(cur)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("读取进程失败: " + ex.Message + " (需要管理员权限)");
                }
            }

            Console.WriteLine();
            Console.WriteLine("流程: 进入对局 -> reveal 开启本地显示；hide 关闭；revert 恢复代码。");
            return 0;
        }

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
                var snapshot = FowDisplay.SnapshotCode(pm, sites);
                DisplayState after;
                try
                {
                    if (enabled) FowDisplay.SetCode(pm, pe, sites, true);
                    FowDisplay.SetFlags(pm, enabled);
                    after = FowDisplay.Wait(pm, enabled);
                }
                catch (Exception failure)
                {
                    var failures = new List<Exception> { failure };
                    try { FowDisplay.RestoreFlags(pm, before); }
                    catch (Exception rollback) { failures.Add(rollback); }
                    try { FowDisplay.RestoreCode(pm, pe, snapshot); }
                    catch (Exception rollback) { failures.Add(rollback); }
                    throw new AggregateException("显示操作未完成，已尝试撤回本次显示修改。", failures);
                }
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
        }

        // ------------------------------------------------------------- revert

        static int CmdRevert()
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
                var failures = new List<Exception>();
                try
                {
                    FowDisplay.ClearUiFlags(pm);
                    FowDisplay.SetFlags(pm, false);
                    FowDisplay.Wait(pm, false);
                }
                catch (Exception ex)
                {
                    failures.Add(new InvalidOperationException("显示字段清理未全部完成。", ex));
                }
                // Restore each known code site even if a scene disappeared or
                // one site has changed. Report all failures after the attempts.
                foreach (var site in sites)
                {
                    try
                    {
                        FowDisplay.SetCode(pm, pe, new List<PatchSite> { site }, false);
                        Console.WriteLine($"  {site.Name,-20} 原始字节已确认");
                    }
                    catch (Exception ex) { failures.Add(new InvalidOperationException(site.Name, ex)); }
                }
                if (failures.Count > 0) throw new AggregateException("还原未全部完成，请检查各项结果。", failures);
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
        }

        // ------------------------------------------------------------- file mode

        static int CmdFileApply()
        {
            PeImage pe;
            try { pe = LoadExe(); }
            catch (Exception ex) { Console.WriteLine("读取失败: " + ex.Message); return 1; }

            if (IsTargetExeRunning())
            {
                Console.WriteLine("游戏正在运行, 请先退出 (或改用 apply 内存模式).");
                return 1;
            }

            try { RuntimeState.ValidateLayout(pe); }
            catch (Exception ex) { Console.WriteLine(ex.Message); return 1; }
            var sites = LocateAll(pe, true);
            if (sites.Any(s => !s.IsUsable)) { Console.WriteLine("显示补丁未全部匹配，已停止。"); return 1; }

            string bak = _exePath + ".bak";
            if (!File.Exists(bak))
            {
                File.Copy(_exePath, bak, false);
                Console.WriteLine("已备份: " + bak);
            }

            var data = File.ReadAllBytes(_exePath);
            int changed = 0;
            foreach (var s in sites)
            {
                var want = s.DesiredBytes(pe.Data);
                var cur = new byte[s.TargetLength];
                Array.Copy(data, s.TargetOffset, cur, 0, cur.Length);
                if (cur.SequenceEqual(want)) { Console.WriteLine($"  {s.Name,-14} 已是补丁状态"); continue; }
                Array.Copy(want, 0, data, s.TargetOffset, want.Length);
                changed++;
                Console.WriteLine($"  {s.Name,-14} 0x{s.TargetOffset:X8}  {Hex(cur)} -> {Hex(want)}");
            }

            File.WriteAllBytes(_exePath, data);
            Console.WriteLine($"写入完成, 修改 {changed} 处. 校验中...");

            var after = new PeImage(_exePath);
            bool allOk = true;
            foreach (var s in sites)
            {
                var want = s.DesiredBytes(pe.Data);
                var cur = new byte[s.TargetLength];
                Array.Copy(after.Data, s.TargetOffset, cur, 0, cur.Length);
                bool ok = cur.SequenceEqual(want);
                allOk &= ok;
                Console.WriteLine($"  {s.Name,-14} {(ok ? "OK" : "MISMATCH")}  0x{s.TargetOffset:X8}  {Hex(cur)}");
            }
            Console.WriteLine(allOk ? "校验通过: 补丁已落盘." : "校验失败, 请用 file-revert 还原.");
            return allOk ? 0 : 1;
        }

        static int CmdFileRevert()
        {
            string bak = _exePath + ".bak";
            if (!File.Exists(bak)) { Console.WriteLine("找不到备份: " + bak); return 1; }
            if (IsTargetExeRunning())
            {
                Console.WriteLine("游戏正在运行, 请先退出.");
                return 1;
            }
            File.Copy(bak, _exePath, true);
            Console.WriteLine("已从备份还原: " + _exePath);
            return 0;
        }

        // ------------------------------------------------------------- launch opts

        static int CmdLaunchOpts()
        {
            var files = SteamLaunchOptions.FindLocalConfigFiles();
            if (files.Length == 0)
            {
                Console.WriteLine("没找到 Steam 的 localconfig.vdf, 请确认 Steam 已登录过且本机装有 CoH3。");
                return 1;
            }

            Console.WriteLine("找到 " + files.Length + " 个 Steam 账号配置:");
            foreach (var f in files) Console.WriteLine("   " + f);
            Console.WriteLine();

            // 非交互模式: launch-opts --add / launch-opts --remove
            bool? scripted = null;
            foreach (var a in Environment.GetCommandLineArgs())
            {
                if (a == "--add" || a == "-add") scripted = true;
                if (a == "--remove" || a == "-remove") scripted = false;
            }

            if (scripted == null)
            {
                Console.WriteLine("操作: 1=加 -dev   2=去掉 -dev   0=取消");
                Console.Write("选择: ");
                var k = Console.ReadLine()?.Trim();
                if (k == "0") { Console.WriteLine("已取消"); return 0; }
                if (k != "1" && k != "2") { Console.WriteLine("无效选择"); return 2; }
                scripted = k == "1";
            }
            bool enable = scripted.Value;

            int okc = 0;

            if (IsSteamRunning())
            {
                Console.WriteLine("!! Steam 正在运行。Steam 会用内存里的状态回写 localconfig.vdf,");
                Console.WriteLine("!! 直接改文件很可能被覆盖掉。");
                Console.WriteLine("!! 可靠做法二选一:");
                Console.WriteLine("!!   A) 完全退出 Steam (托盘图标右键 -> 退出) 后再运行本命令, 然后重开 Steam;");
                Console.WriteLine("!!   B) 直接在 Steam 界面设置: 库 -> 右键 Company of Heroes 3 -> 属性 ->");
                Console.WriteLine("!!      通用 -> 启动选项 -> 填入  -dev");
                Console.WriteLine();
            }
            foreach (var f in files)
            {
                try
                {
                    var cur = SteamLaunchOptions.Read(f, SteamLaunchOptions.AppId);
                    var next = SteamLaunchOptions.Merge(cur, "-dev", enable);
                    if (next == cur) { Console.WriteLine($"  {f}: 无需改动 (当前: \"{cur}\")"); continue; }
                    if (SteamLaunchOptions.Write(f, next, SteamLaunchOptions.AppId))
                    {
                        okc++;
                        Console.WriteLine($"  {f}");
                        Console.WriteLine($"    \"{cur}\"  ->  \"{next}\"");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  {f}: 失败 {ex.Message}");
                }
            }

            Console.WriteLine();
            Console.WriteLine(okc > 0
                ? $"已更新 {okc} 个账号的启动项。重启 Steam 后生效。"
                : "没有改动。");
            Console.WriteLine("说明: -dev 让零售版也加载 Relic 的开发控制台与 data:dev/ 脚本。");
            return okc > 0 ? 0 : 1;
        }

        // ------------------------------------------------------------- selftest

        /// <summary>
        /// 不改动原文件的自检: 复制一份 exe 到临时目录, 在上面跑
        /// 原始扫描 -> 打补丁 -> 复检 -> 目标字节检查 -> 还原 -> 复检 全流程。
        /// </summary>
        static int CmdSelfTest()
        {
            if (!File.Exists(_exePath)) { Console.WriteLine("找不到 exe: " + _exePath); return 1; }

            string tmpDir = Path.Combine(Path.GetTempPath(), "coh3fow_selftest_" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(tmpDir);
            string tmp = Path.Combine(tmpDir, Path.GetFileName(_exePath));

            string savedExe = _exePath;
            try
            {
                Console.WriteLine("复制样本 -> " + tmp);
                File.Copy(_exePath, tmp, true);
                _exePath = tmp;

                int step = 0;
                bool ok = true;

                bool Check(string label, Func<bool> f)
                {
                    step++;
                    bool r;
                    try { r = f(); }
                    catch (Exception ex) { Console.WriteLine($"  [{step}] {label}: 异常 {ex.Message}"); ok = false; return false; }
                    Console.WriteLine($"  [{step}] {label}: {(r ? "通过" : "失败")}");
                    ok &= r;
                    return r;
                }

                Console.WriteLine("\n--- 自检开始 ---");

                Check("原版全部补丁点均可定位且为原始状态", () =>
                {
                    var pe = new PeImage(_exePath);
                    var s = PatchCatalog.Build();
                    foreach (var x in s) x.Locate(pe);
                    return s.All(x => x.State == SiteState.Original);
                });

                Check("file-apply 返回 0", () => CmdFileApply() == 0);

                Check("打完补丁后全部补丁点均为已补状态", () =>
                {
                    var pe = new PeImage(_exePath);
                    var s = PatchCatalog.Build();
                    foreach (var x in s) x.Locate(pe);
                    return s.All(x => x.State == SiteState.Patched);
                });

                Check("目标字节与预期补丁一致", () =>
                {
                    var pe = new PeImage(_exePath);
                    var s = PatchCatalog.Build();
                    foreach (var x in s) x.Locate(pe);
                    foreach (var x in s)
                    {
                        if (x.TargetOffset < 0) return false;
                        var cur = new byte[x.TargetLength];
                        Array.Copy(pe.Data, x.TargetOffset, cur, 0, cur.Length);
                        if (!cur.SequenceEqual(x.DesiredBytes(pe.Data))) return false;
                    }
                    return true;
                });

                Check("file-revert 返回 0", () => CmdFileRevert() == 0);

                Check("还原后字节与原 exe 完全一致", () =>
                {
                    using var a = File.OpenRead(savedExe);
                    using var b = File.OpenRead(tmp);
                    if (a.Length != b.Length) return false;
                    var ba = new byte[1 << 20];
                    var bb = new byte[1 << 20];
                    while (true)
                    {
                        int ra = a.Read(ba, 0, ba.Length);
                        int rb = b.Read(bb, 0, bb.Length);
                        if (ra != rb) return false;
                        if (ra == 0) return true;
                        for (int i = 0; i < ra; i++) if (ba[i] != bb[i]) return false;
                    }
                });

                Console.WriteLine("--- 自检" + (ok ? "全部通过" : "存在失败项") + " ---");
                return ok ? 0 : 1;
            }
            finally
            {
                _exePath = savedExe;
                try { Directory.Delete(tmpDir, true); } catch { }
            }
        }

        // ------------------------------------------------------------- helpers

        /// <summary>Steam 客户端是否正在运行。运行中时它会用内存状态回写 localconfig.vdf。</summary>
        static bool IsSteamRunning()
        {
            try
            {
                var names = new[] { "steam", "steamwebhelper" };
                foreach (var n in names)
                {
                    var procs = Process.GetProcessesByName(n);
                    try
                    {
                        if (procs.Length > 0) return true;
                    }
                    finally { foreach (var p in procs) p.Dispose(); }
                }
            }
            catch { }
            return false;
        }
    }
}
