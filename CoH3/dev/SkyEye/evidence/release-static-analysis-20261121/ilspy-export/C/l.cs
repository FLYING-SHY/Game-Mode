using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using a;

namespace C;

internal sealed class l : h
{
	private int m_A;

	private DateTime m_A;

	public F A()
	{
		Process[] processesByName = Process.GetProcessesByName(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Gc());
		try
		{
			if (processesByName.Length != 1)
			{
				return new F
				{
					B = ((processesByName.Length == 0) ? CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xj() : CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XJ())
				};
			}
			Process process = processesByName[0];
			DateTime startTime = process.StartTime;
			if (this.m_A != process.Id || this.m_A != startTime || G.A == G.A.b)
			{
				G.B(process.MainModule.FileName);
				this.m_A = process.Id;
				this.m_A = startTime;
			}
			string path = r.A;
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
						return new F
						{
							B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XK()
						};
					}
					text = text.Substring(0, num2 + 1);
				}
			}
			ulong? num3 = r.A(text, startTime, lastWriteTime);
			if (!num3.HasValue || process.HasExited || process.StartTime != startTime || File.GetLastWriteTime(path) < startTime)
			{
				return new F
				{
					B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xk()
				};
			}
			string a = A(text);
			return new F
			{
				A = process.Id,
				A = startTime,
				A = num3.Value.ToString(CultureInfo.InvariantCulture),
				a = a
			};
		}
		catch (UnauthorizedAccessException)
		{
			return new F
			{
				B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XL()
			};
		}
		catch (Win32Exception)
		{
			return new F
			{
				B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xl()
			};
		}
		catch (IOException)
		{
			return new F
			{
				B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XM()
			};
		}
		catch (InvalidOperationException)
		{
			return new F
			{
				B = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Xm()
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
				int num = text.IndexOf(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XN(), StringComparison.Ordinal);
				if (num < 0 || !text.EndsWith(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Eo(), StringComparison.Ordinal))
				{
					continue;
				}
				string text2 = text.Substring(num + CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XN().Length, text.Length - num - CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.XN().Length - 1);
				if (text2.Length > 128)
				{
					return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
				}
				string text3 = text2;
				foreach (char c in text3)
				{
					if (c < ' ' || c == '\u007f')
					{
						return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
					}
				}
				return text2;
			}
		}
		return CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.D();
	}
}
