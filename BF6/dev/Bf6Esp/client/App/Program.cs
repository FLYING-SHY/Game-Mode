using System;
using System.Windows.Forms;

namespace Bf6Esp;

/// <summary>
/// 入口。默认启动图形界面（激活 + 覆盖层）；
/// --probe / --calibrate 为只读诊断模式，不启动覆盖层。
/// </summary>
internal static class Program
{
	[STAThread]
	private static int Main(string[] args)
	{
		string mode = "gui";
		string output = null;
		for (int i = 0; i < args.Length; i++)
		{
			string arg = args[i] ?? string.Empty;
			if (string.Equals(arg, "--calibrate", StringComparison.OrdinalIgnoreCase))
			{
				mode = "calibrate";
			}
			else if (string.Equals(arg, "--probe", StringComparison.OrdinalIgnoreCase))
			{
				mode = "probe";
			}
			else if (string.Equals(arg, "--selftest", StringComparison.OrdinalIgnoreCase))
			{
				mode = "selftest";
			}
			else if (string.Equals(arg, "--selftest-reflect", StringComparison.OrdinalIgnoreCase))
			{
				mode = "selftest-reflect";
			}
			else if (string.Equals(arg, "--selftest-locate", StringComparison.OrdinalIgnoreCase))
			{
				mode = "selftest-locate";
			}
			else if (string.Equals(arg, "--selftest-entity", StringComparison.OrdinalIgnoreCase))
			{
				mode = "selftest-entity";
			}
			else if (string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(arg, "/?", StringComparison.OrdinalIgnoreCase))
			{
				mode = "help";
			}
			else if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase))
			{
				if (i + 1 < args.Length)
				{
					output = args[++i];
				}
				else
				{
					ConsoleBridge60.Attach();
					ConsoleBridge60.WriteLine("[错误] --out 需要一个文件路径参数。");
					Usage();
					return 2;
				}
			}
			else if (arg.StartsWith("--out=", StringComparison.OrdinalIgnoreCase))
			{
				output = arg.Substring("--out=".Length);
			}
			else
			{
				ConsoleBridge60.Attach();
				ConsoleBridge60.WriteLine("[错误] 未知参数：" + arg);
				Usage();
				return 2;
			}
		}
		if (mode == "help")
		{
			Usage();
			return 0;
		}
		if (mode == "probe")
		{
			return ProbeRunner60.Run();
		}
		if (mode == "selftest")
		{
			return SelfTestRunner60.Run();
		}
		if (mode == "selftest-reflect")
		{
			return ReflectionSelfTest60.Run();
		}
		if (mode == "selftest-locate")
		{
			return LocateSelfTest60.Run();
		}
		if (mode == "selftest-entity")
		{
			return EntityProfilerSelfTest60.Run();
		}
		if (mode == "calibrate")
		{
			return CalibrationRunner60.Run(output);
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		try
		{
			Application.Run(new LicenseApplication60());
		}
		catch (Exception exception)
		{
			MessageBox.Show("启动失败：" + exception.Message, "Bf6Esp", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return 1;
		}
		return 0;
	}

	private static void Usage()
	{
		ConsoleBridge60.Attach();
		ConsoleBridge60.WriteLine("Bf6Esp —— Battlefield 6 外部只读 ESP");
		ConsoleBridge60.WriteLine("");
		ConsoleBridge60.WriteLine("用法：");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe                 启动图形界面（激活 + 覆盖层）");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --probe         快速状态探测：游戏是否运行、档案是否匹配、签名命中数");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --calibrate     只读校准：导出模块信息、签名命中与通用启发式发现");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --calibrate --out <file>   指定校准报告输出路径");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --selftest     只读通道自检（读自身进程 PE 头 + 已知内容往返，不需要游戏运行）");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --selftest-reflect  反射解析器自检（合成夹具，校验字段名->偏移还原）");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --selftest-locate   运行时定位器自检（对自身进程跑完整定位流程）");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --selftest-entity   实体字段推断自检（合成夹具，无字段名反推位置/血量/队伍/姓名）");
		ConsoleBridge60.WriteLine("  Bf6Esp.exe --help          显示本帮助");
		ConsoleBridge60.WriteLine("");
		ConsoleBridge60.WriteLine("退出码：0 正常 / 2 参数错误 / 3 校准时游戏未运行 / 4 无法只读打开进程 /");
		ConsoleBridge60.WriteLine("        5 PE 头解析失败 / 6 报告写入失败。--probe 恒为 0，--selftest / --selftest-reflect /");
		ConsoleBridge60.WriteLine("        --selftest-locate / --selftest-entity 失败时为 1。");
	}
}
