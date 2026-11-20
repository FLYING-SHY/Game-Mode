using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using a;

namespace C;

internal sealed class k : g
{
	private int m_A;

	private DateTime m_A;

	public e A()
	{
		Process[] processesByName = Process.GetProcessesByName(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.fZ());
		try
		{
			if (processesByName.Length != 1)
			{
				return new e
				{
					B = ((processesByName.Length == 0) ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XF() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xe())
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
							B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xf()
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
					B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XG()
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
				B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xg()
			};
		}
		catch (Win32Exception)
		{
			return new e
			{
				B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XH()
			};
		}
		catch (IOException)
		{
			return new e
			{
				B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xh()
			};
		}
		catch (InvalidOperationException)
		{
			return new e
			{
				B = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.XI()
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
				int num = text.IndexOf(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xi(), StringComparison.Ordinal);
				if (num < 0 || !text.EndsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.EL(), StringComparison.Ordinal))
				{
					continue;
				}
				string text2 = text.Substring(num + C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xi().Length, text.Length - num - C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Xi().Length - 1);
				if (text2.Length > 128)
				{
					return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
				}
				string text3 = text2;
				foreach (char c in text3)
				{
					if (c < ' ' || c == '\u007f')
					{
						return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
					}
				}
				return text2;
			}
		}
		return C4318269_002D39F2_002D4549_002D851A_002D418926B36435.D();
	}
}
