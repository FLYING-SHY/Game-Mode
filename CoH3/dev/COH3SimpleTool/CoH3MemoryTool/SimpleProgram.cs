using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal static class SimpleProgram
{
	[DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();

	[STAThread]
	public static int Main(string[] args)
	{
		SetProcessDPIAware();
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		bool createdNew;
		using (new Mutex(initiallyOwned: true, "Local\\COH3SimpleToolControls", out createdNew))
		{
			if (!createdNew)
			{
				MessageBox.Show("COH3工具已在运行，请先退出已有窗口。", "COH3工具");
				return 1;
			}
			Application.Run(new SimpleLicenseApp());
		}
		return 0;
	}
}
