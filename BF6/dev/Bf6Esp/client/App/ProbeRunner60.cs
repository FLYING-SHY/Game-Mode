using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bf6Esp;

/// <summary>
/// 探测模式：快速回答三个问题——游戏在不在、档案对不对、签名命中没有。
/// 只读，不写任何游戏内存；游戏未运行时给出明确中文提示并正常退出。
/// </summary>
internal static class ProbeRunner60
{
	/// <summary>执行探测；返回进程退出码（始终为 0，探测结果只看输出文本）。</summary>
	internal static int Run()
	{
		ConsoleBridge60.Attach();
		ConsoleBridge60.WriteLine("=== Bf6Esp 探测模式（外部只读） ===");
		ConsoleBridge60.WriteLine("[信息] 授权状态：" + (LicenseGate60.Allowed
			? "已授权（覆盖层可用）"
			: "未授权（覆盖层不会启动；探测只读，不受影响）"));
		string directory = ProfileStore.LocateProfilesDirectory();
		List<OffsetProfile> profiles = ProfileStore.LoadAll(directory);
		ConsoleBridge60.WriteLine("[信息] 档案目录：" + directory + "；档案数量：" + profiles.Count.ToString(CultureInfo.InvariantCulture));
		GameProcess game = GameProcess.Attach();
		if (game == null)
		{
			ConsoleBridge60.WriteLine("[信息] 游戏进程：未运行。未检测到 Battlefield 6 进程（bf6.exe / bf6Trial.exe）。");
			ConsoleBridge60.WriteLine("[信息] 版本校验：跳过（游戏未运行，无法计算当前 build 的 SHA-256）。");
			ConsoleBridge60.WriteLine("[信息] 签名命中：跳过（游戏未运行）。");
			ConsoleBridge60.WriteLine("=== 探测结束 ===");
			return 0;
		}
		using (game)
		{
			ConsoleBridge60.WriteLine("[信息] 游戏进程：PID=" + game.ProcessId.ToString(CultureInfo.InvariantCulture) + " 模块基址=0x" + game.ModuleBase.ToString("X") + " 模块大小=0x" + game.ModuleSize.ToString("X"));
			string sha = string.Empty;
			try
			{
				sha = ProfileStore.ComputeSha256(game.ExePath);
			}
			catch (Exception)
			{
			}
			ConsoleBridge60.WriteLine("[信息] 当前 build SHA-256：" + (string.IsNullOrEmpty(sha) ? "(读取失败)" : sha));
			OffsetProfile matched = ProfileStore.MatchByHash(profiles, sha);
			if (matched != null)
			{
				ConsoleBridge60.WriteLine("[信息] 版本校验：命中档案（build " + Short(matched.BuildSha256) + "）。");
			}
			else
			{
				ConsoleBridge60.WriteLine("[警告] 版本不匹配：当前 build " + Short(sha) + " 在档案里没有对应条目，将退回签名解析。");
			}
			MemoryReader reader = new MemoryReader(game.Handle, true);
			if (!reader.IsValid)
			{
				ConsoleBridge60.WriteLine("[错误] 无法以只读方式打开进程句柄。");
				ConsoleBridge60.WriteLine("=== 探测结束 ===");
				return 0;
			}
			PeImage image = PeImage.FromMemory(reader, game.ModuleBase);
			if (image == null)
			{
				image = PeImage.FromFile(game.ExePath);
			}
			if (image == null)
			{
				ConsoleBridge60.WriteLine("[错误] 无法解析 PE 头。");
				ConsoleBridge60.WriteLine("=== 探测结束 ===");
				return 0;
			}
			ConsoleBridge60.WriteLine("[信息] PE 节数量：" + image.Sections.Count.ToString(CultureInfo.InvariantCulture) + "，镜像基址=0x" + image.ImageBase.ToString("X"));
			CalibrationReport60 report = new CalibrationReport60();
			CalibrationRunner60.ScanSignatures(reader, game.ModuleBase, image, profiles, report);
			ConsoleBridge60.WriteLine("[信息] 读取失败计数：" + reader.ReadFailures.ToString(CultureInfo.InvariantCulture));
			ConsoleBridge60.WriteLine("=== 探测结束 ===");
			return 0;
		}
	}

	private static string Short(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return "未知";
		}
		return value.Length <= 12 ? value : value.Substring(0, 12);
	}
}
