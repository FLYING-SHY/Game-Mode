using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
using a;

namespace C;

internal sealed class k : g
{
	private int m_A;

	private DateTime m_A;

	public e A()
	{
		Process[] processesByName = Process.GetProcessesByName(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.fZ());
		try
		{
			if (processesByName.Length != 1)
			{
				return new e
				{
					B = ((processesByName.Length == 0) ? _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XF() : _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xe())
				};
			}
			Process process = processesByName[0];
			DateTime startTime = process.StartTime;
			if (this.m_A != process.Id || this.m_A != startTime)
			{
				F.a(process.MainModule.FileName);
				this.m_A = process.Id;
				this.m_A = startTime;
			}
			string path = q.A;
			DateTime lastWriteTime = File.GetLastWriteTime(path);
			string text;
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
			{
				byte[] array = new byte[Math.Min(65536, (int)Math.Min(fileStream.Length, 2147483647L))];
				int i;
				int num;
				for (i = 0; i < array.Length; i += num)
				{
					if ((num = fileStream.Read(array, i, array.Length - i)) <= 0)
					{
						break;
					}
				}
				text = Encoding.UTF8.GetString(array, 0, i).TrimStart('\ufeff');
				if (fileStream.Length > i)
				{
					int num2 = text.LastIndexOf('\n');
					if (num2 < 0)
					{
						return new e
						{
							B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xf()
						};
					}
					text = text.Substring(0, num2 + 1);
				}
			}
			ulong? num3 = q.A(text, startTime, lastWriteTime);
			if (!num3.HasValue || process.HasExited || process.StartTime != startTime || File.GetLastWriteTime(path) < startTime)
			{
				return new e
				{
					B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XG()
				};
			}
			string a = A(text);
			return new e
			{
				A = process.Id,
				A = startTime,
				A = num3.Value.ToString(CultureInfo.InvariantCulture),
				a = a
			};
		}
		catch (UnauthorizedAccessException)
		{
			return new e
			{
				B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xg()
			};
		}
		catch (Win32Exception)
		{
			return new e
			{
				B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XH()
			};
		}
		catch (IOException)
		{
			return new e
			{
				B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xh()
			};
		}
		catch (InvalidOperationException)
		{
			return new e
			{
				B = _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.XI()
			};
		}
		finally
		{
			Process[] array2 = processesByName;
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j].Dispose();
			}
		}
	}

	internal static string A(string P_0)
	{
		using (StringReader stringReader = new StringReader(P_0))
		{
			string text;
			while ((text = stringReader.ReadLine()) != null)
			{
				int num = text.IndexOf(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xi(), StringComparison.Ordinal);
				if (num < 0 || !text.EndsWith(_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.EL(), StringComparison.Ordinal))
				{
					continue;
				}
				string text2 = text.Substring(num + _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xi().Length, text.Length - num - _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Xi().Length - 1);
				if (text2.Length > 128)
				{
					return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
				}
				string text3 = text2;
				foreach (char c in text3)
				{
					if (c < ' ' || c == '\u007f')
					{
						return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
					}
				}
				return text2;
			}
		}
		return _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.D();
	}
}
