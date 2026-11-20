using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;

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
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jS() + (this.A ? 1 : 0),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.js() + (this.a ? 1 : 0),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jT() + (this.B ? 1 : 0),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jt() + (this.b ? 1 : 0),
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jU() + this.A,
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ju() + this.a,
			C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jV() + this.A.ToString(CultureInfo.InvariantCulture)
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
					if (c2 != 'N' || !(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jw()))
					{
						break;
					}
					goto IL_0188;
				}
				if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jW()))
				{
					break;
				}
				this.a = text2 != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BY();
				return true;
			}
			case 9:
			{
				char c2 = text[5];
				if (c2 != 'S')
				{
					if (c2 != 'T' || !(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jy()))
					{
						break;
					}
					if (int.TryParse(text2, out result2))
					{
						this.a = Math.Max(8, Math.Min(36, result2));
					}
					return true;
				}
				if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jY()))
				{
					break;
				}
				goto IL_0188;
			}
			case 8:
				if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jv()))
				{
					break;
				}
				this.A = text2 != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BY();
				return true;
			case 12:
				if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jX()))
				{
					break;
				}
				if (int.TryParse(text2, out result2))
				{
					this.B = result2 != 0;
				}
				return true;
			case 10:
				if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jx()))
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
					if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.jZ()))
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
