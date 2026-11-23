using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;

namespace B;

internal sealed class T
{
	public long A;

	public uint A;

	public string A;

	public Color A;
}
internal sealed class t
{
	public bool A = true;

	public bool a = true;

	public bool B = true;

	public bool b = true;

	public int A = 16;

	public int a = 12;

	public decimal A = 1.5m;

	public bool A => false;

	public bool a
	{
		get
		{
			if (!this.B)
			{
				return this.A;
			}
			return true;
		}
	}

	public bool A(r P_0)
	{
		return this.B;
	}

	public t B()
	{
		return new t
		{
			A = this.A,
			a = this.a,
			B = this.B,
			b = this.b,
			A = this.A,
			a = this.a,
			A = this.A
		};
	}

	public IEnumerable<string> b()
	{
		return new List<string>
		{
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jw() + (this.A ? 1 : 0),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jX() + (this.a ? 1 : 0),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jx() + (this.B ? 1 : 0),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jY() + (this.b ? 1 : 0),
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jy() + this.A,
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jZ() + this.a,
			CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jz() + this.A.ToString(CultureInfo.InvariantCulture)
		};
	}

	public bool A(string P_0)
	{
		int num = P_0.IndexOf('=');
		if (num < 0)
		{
			return false;
		}
		string text = P_0.Substring(0, num);
		string text2 = P_0.Substring(num + 1);
		if (text != null)
		{
			int result2;
			switch (text.Length)
			{
			case 11:
			{
				char c2 = text[5];
				if (c2 != 'A')
				{
					if (c2 != 'N' || !(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KB()))
					{
						break;
					}
					goto IL_0188;
				}
				if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ka()))
				{
					break;
				}
				this.a = text2 != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By();
				return true;
			}
			case 9:
			{
				char c2 = text[5];
				if (c2 != 'S')
				{
					if (c2 != 'T' || !(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KD()))
					{
						break;
					}
					if (int.TryParse(text2, out result2))
					{
						this.a = Math.Max(8, Math.Min(36, result2));
					}
					return true;
				}
				if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kc()))
				{
					break;
				}
				goto IL_0188;
			}
			case 8:
				if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KA()))
				{
					break;
				}
				this.A = text2 != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By();
				return true;
			case 12:
				if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kb()))
				{
					break;
				}
				if (int.TryParse(text2, out result2))
				{
					this.B = result2 != 0;
				}
				return true;
			case 10:
				if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KC()))
				{
					break;
				}
				if (int.TryParse(text2, out result2))
				{
					this.b = result2 != 0;
				}
				return true;
			case 14:
				{
					if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kd()))
					{
						break;
					}
					if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
					{
						this.A = Math.Max(0.5m, Math.Min(8m, result));
					}
					return true;
				}
				IL_0188:
				if (int.TryParse(text2, out result2))
				{
					this.A = Math.Max(8, Math.Min(72, result2));
				}
				return true;
			}
		}
		return false;
	}
}
