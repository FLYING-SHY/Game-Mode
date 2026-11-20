using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;

namespace B;

internal sealed class S
{
	public long A;

	public uint A;

	public string A;

	public Color A;
}
internal sealed class s
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

	public bool A(q P_0)
	{
		return this.B;
	}

	public s B()
	{
		return new s
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
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jS() + (this.A ? 1 : 0),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.js() + (this.a ? 1 : 0),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jT() + (this.B ? 1 : 0),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jt() + (this.b ? 1 : 0),
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jU() + this.A,
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ju() + this.a,
			_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jV() + this.A.ToString(CultureInfo.InvariantCulture)
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
					if (c2 != 'N' || !(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jw()))
					{
						break;
					}
					goto IL_0188;
				}
				if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jW()))
				{
					break;
				}
				this.a = text2 != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BY();
				return true;
			}
			case 9:
			{
				char c2 = text[5];
				if (c2 != 'S')
				{
					if (c2 != 'T' || !(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jy()))
					{
						break;
					}
					if (int.TryParse(text2, out result2))
					{
						this.a = Math.Max(8, Math.Min(36, result2));
					}
					return true;
				}
				if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jY()))
				{
					break;
				}
				goto IL_0188;
			}
			case 8:
				if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jv()))
				{
					break;
				}
				this.A = text2 != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BY();
				return true;
			case 12:
				if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jX()))
				{
					break;
				}
				if (int.TryParse(text2, out result2))
				{
					this.B = result2 != 0;
				}
				return true;
			case 10:
				if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jx()))
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
					if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.jZ()))
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
