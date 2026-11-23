using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Bf6Esp;

/// <summary>
/// 命令行输出桥。程序是 WinExe（没有自带控制台），
/// 这里在运行 CLI 模式时挂到父进程的控制台，保证 --probe / --calibrate 有可见输出。
/// 如果标准输出已被重定向（例如 Bf6Esp.exe --probe &gt; out.txt），则直接写标准输出句柄，
/// 并强制使用 UTF-8，避免中文提示乱码。
/// </summary>
internal static class ConsoleBridge60
{
	private const int ATTACH_PARENT_PROCESS = -1;

	private static StreamWriter consoleWriter;

	private static bool attached;

	public static void Attach()
	{
		if (attached)
		{
			return;
		}
		attached = true;
		Encoding encoding = new UTF8Encoding(false);

		// 情况一：标准输出被重定向（管道 / 文件）。
		try
		{
			if (Console.IsOutputRedirected)
			{
				Stream stdout = Console.OpenStandardOutput();
				consoleWriter = new StreamWriter(stdout, encoding);
				consoleWriter.AutoFlush = true;
				return;
			}
		}
		catch (Exception)
		{
			consoleWriter = null;
		}

		// 情况二：挂到父进程的控制台。
		try
		{
			AttachConsole(ATTACH_PARENT_PROCESS);
		}
		catch (Exception)
		{
		}
		try
		{
			// 让控制台用 UTF-8 输出，中文提示不会乱码。
			Console.OutputEncoding = encoding;
		}
		catch (Exception)
		{
		}
		try
		{
			FileStream stream = new FileStream("CONOUT$", FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
			consoleWriter = new StreamWriter(stream, encoding);
			consoleWriter.AutoFlush = true;
		}
		catch (IOException)
		{
			consoleWriter = null;
		}
		catch (UnauthorizedAccessException)
		{
			consoleWriter = null;
		}
	}

	public static void WriteLine(string text)
	{
		if (text == null)
		{
			return;
		}
		if (consoleWriter != null)
		{
			try
			{
				consoleWriter.WriteLine(text);
				return;
			}
			catch (IOException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
		}
		try
		{
			Console.WriteLine(text);
		}
		catch (IOException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool AttachConsole(int dwProcessId);
}
