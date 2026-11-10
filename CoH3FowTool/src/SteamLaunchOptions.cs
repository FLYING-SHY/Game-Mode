using System;
using System.IO;
using System.Text;

namespace CoH3FowTool
{
    /// <summary>
    /// Steam 启动项读写。
    ///
    /// Steam 把每个 app 的启动项存在
    ///   &lt;SteamRoot&gt;/userdata/&lt;accountId&gt;/config/localconfig.vdf
    /// 里, 是 Valve KeyValues 文本格式。CoH3 (appid 1677280) 的启动项键是
    ///   "LaunchOptions"
    /// 挂在 "Software/Valve/Steam/apps/1677280" 下。
    ///
    /// 这里只做最小改动: 找到 LaunchOptions 那一行, 替换它的值, 其余内容原样保留。
    /// </summary>
    internal static class SteamLaunchOptions
    {
        public const int AppId = 1677280;

        public static string[] FindSteamRoots()
        {
            var roots = new System.Collections.Generic.List<string>();

            // 优先用注册表里记录的 Steam 路径
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var p = key?.GetValue("SteamPath") as string;
                if (!string.IsNullOrEmpty(p) && Directory.Exists(p)) roots.Add(p);
            }
            catch { }

            foreach (var guess in new[]
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                @"D:\Steam",
                @"D:\SteamLibrary",
                @"D:\game\steam",
                @"E:\Steam",
                @"E:\SteamLibrary",
                @"G:\Steam",
                @"G:\SteamLibrary",
            })
            {
                if (Directory.Exists(guess) && !roots.Contains(guess)) roots.Add(guess);
            }
            return roots.ToArray();
        }

        public static string[] FindLocalConfigFiles()
        {
            var outFiles = new System.Collections.Generic.List<string>();
            var seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var root in FindSteamRoots())
            {
                string ud = Path.Combine(root, "userdata");
                if (!Directory.Exists(ud)) continue;
                foreach (var acct in Directory.GetDirectories(ud))
                {
                    string f = Path.Combine(acct, "config", "localconfig.vdf");
                    if (!File.Exists(f)) continue;
                    // 同一个文件可能通过不同盘符大小写路径找到, 用绝对路径去重
                    string key = Path.GetFullPath(f).ToLowerInvariant();
                    if (!seen.Add(key)) continue;
                    outFiles.Add(f);
                }
            }
            return outFiles.ToArray();
        }

        /// <summary>读取某个 app 块内的 LaunchOptions; 没设置时返回空串。</summary>
        public static string Read(string file, int appId)
        {
            var lines = File.ReadAllLines(file);
            string appKey = "\"" + appId + "\"";

            int blockStart = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                var t = lines[i].Trim();
                if (t != appKey) continue;
                int j = i + 1;
                while (j < lines.Length && lines[j].Trim().Length == 0) j++;
                if (j >= lines.Length || lines[j].Trim() != "{") continue;
                blockStart = j;
                break;
            }
            if (blockStart < 0) return "";

            for (int i = blockStart; i < lines.Length; i++)
            {
                var t = lines[i].Trim();
                if (t == "}") break;
                if (!t.StartsWith("\"LaunchOptions\"", StringComparison.Ordinal)) continue;
                int q1 = t.IndexOf('"');
                int q2 = t.IndexOf('"', q1 + 1);
                if (q2 < 0) continue;
                int q3 = t.IndexOf('"', q2 + 1);
                if (q3 < 0) continue;
                return t.Substring(q2 + 1, q3 - q2 - 1);
            }
            return "";
        }

        /// <summary>
        /// 写入 LaunchOptions。
        ///
        /// Steam 把每个 app 的设置放在
        ///   "Software" { "Valve" { "Steam" { "apps" { "&lt;appid&gt;" { ... } } } } }
        /// 里。设置项本身是普通文本 KV (和 LastPlayed / Playtime 同一个层级),
        /// 只有云同步相关的数据才会用 hex blob。
        ///
        /// 这里把 LaunchOptions 插到该 app 块内第一个条目前面, 其余内容原样保留。
        /// </summary>
        public static bool Write(string file, string value, int appId)
        {
            var lines = File.ReadAllLines(file);
            string appKey = "\"" + appId + "\"";

            // 找 apps 段里 "<appid>" 这个块的起止
            int blockStart = -1, blockIndent = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var t = lines[i].Trim();
                if (t != appKey) continue;
                // 后面必须紧跟 "{" 才是块的开头 (blob 形式是 "1677280"  "hex")
                int j = i + 1;
                while (j < lines.Length && lines[j].Trim().Length == 0) j++;
                if (j >= lines.Length || lines[j].Trim() != "{") continue;
                blockStart = j;
                blockIndent = lines[i].Length - lines[i].TrimStart().Length;
                break;
            }
            if (blockStart < 0) return false;

            // 若已存在 LaunchOptions, 就地替换
            for (int i = blockStart; i < lines.Length; i++)
            {
                var t = lines[i].Trim();
                if (t == "}") break;                       // 出了这个块
                if (!t.StartsWith("\"LaunchOptions\"", StringComparison.Ordinal)) continue;

                int q1 = t.IndexOf('"');
                int q2 = t.IndexOf('"', q1 + 1);
                int q3 = t.IndexOf('"', q2 + 1);
                string tail = q3 >= 0 ? t.Substring(q3 + 1) : "";
                lines[i] = t.Substring(0, q2 + 1) + value.Replace("\"", "\\\"") + "\"" + tail;
                File.WriteAllLines(file, lines, new UTF8Encoding(false));
                return true;
            }

            // 没有就插到块的第一个子项之前 (用 Tab 缩进, 与 Steam 自己写的一致)
            int insertAt = blockStart + 1;
            int childIndent = lines[blockStart].Length - lines[blockStart].TrimStart().Length;
            var newLine = new string('\t', childIndent / 4 + 1) + "\"LaunchOptions\"\t\t\"" + value + "\"";
            lines = lines.Take(insertAt).Concat(new[] { newLine }).Concat(lines.Skip(insertAt)).ToArray();
            File.WriteAllLines(file, lines, new UTF8Encoding(false));
            return true;
        }

        /// <summary>把 options 里的某个开关加上/去掉, 保留其它已有内容。</summary>
        public static string Merge(string current, string flag, bool enable)
        {
            var parts = (current ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Where(s => !s.Equals(flag, StringComparison.OrdinalIgnoreCase)).ToList();
            if (enable) parts.Add(flag);
            return string.Join(" ", parts);
        }
    }
}
