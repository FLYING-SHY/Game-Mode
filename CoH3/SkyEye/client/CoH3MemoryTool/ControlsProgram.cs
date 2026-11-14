using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class ControlsProgram
{
	[DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();

	[STAThread]
	public static int Main(string[] args)
	{
		SetProcessDPIAware();
		AudioDependency.Initialize();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		bool createdNew;
		using (new Mutex(initiallyOwned: true, "Local\\SkyeyeSelfHostedControls", out createdNew))
		{
			if (!createdNew)
			{
				MessageBox.Show("软件已在运行，请退出旧版后再打开新版。", "天穹之眼 · 英雄连3");
				return 1;
			}
			Application.Run(new LicenseApplication60());
		}
		return 0;
	}
}
