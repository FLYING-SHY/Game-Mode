using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;
using A;
using B;

namespace B
{
	internal sealed class B
	{
		public bool A = true;

		public int A = 10;

		public bool a = true;

		public bool B = true;

		public bool b = true;

		public int a = 6;

		public int B = 10;

		public int b = 10;

		public int C = 8;

		public int c = 12;

		public decimal A = 1m;

		public IEnumerable<string> A()
		{
			return new string[11]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ea() + (this.A ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eB() + this.A,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eb() + (this.a ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eC() + a,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ec() + B,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eD() + b,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ed() + (this.B ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eE() + this.A.ToString(CultureInfo.InvariantCulture),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ee() + C,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eF() + (this.b ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ef() + c
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
			int result2;
			if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eG()) && !(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eg()))
			{
				if (text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eH())
				{
					if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
					{
						this.A = Math.Max(0.5m, Math.Min(8m, result));
					}
					return true;
				}
				if (text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eh() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eI() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ei() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eJ() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ej() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eK() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ek() && text != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eL())
				{
					return false;
				}
				if (!int.TryParse(text2, out result2))
				{
					return true;
				}
				if (text == null)
				{
					goto IL_0292;
				}
				switch (text.Length)
				{
				case 12:
					break;
				case 9:
					goto IL_0188;
				case 15:
					goto IL_01d6;
				case 13:
					goto IL_01e8;
				case 11:
					goto IL_01fa;
				default:
					goto IL_0292;
				}
				char c2 = text[4];
				if (c2 != 'B')
				{
					if (c2 != 'I')
					{
						if (c2 != 'M' || !(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ej()))
						{
							goto IL_0292;
						}
						this.B = result2 != 0;
					}
					else
					{
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eI()))
						{
							goto IL_0292;
						}
						a = Math.Max(2, Math.Min(40, result2));
					}
				}
				else
				{
					if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ek()))
					{
						goto IL_0292;
					}
					this.b = result2 != 0;
				}
				goto IL_02a7;
			}
			if (int.TryParse(text2, out var result3))
			{
				if (text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eG())
				{
					this.A = result3 != 0;
				}
				else
				{
					this.A = Math.Max(6, Math.Min(30, result3));
				}
			}
			return true;
			IL_01e8:
			if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eK()))
			{
				goto IL_0292;
			}
			C = Math.Max(2, Math.Min(40, result2));
			goto IL_02a7;
			IL_0292:
			b = Math.Max(1, Math.Min(60, result2));
			goto IL_02a7;
			IL_01d6:
			if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eL()))
			{
				goto IL_0292;
			}
			c = Math.Max(4, Math.Min(60, result2));
			goto IL_02a7;
			IL_0188:
			if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.eh()))
			{
				goto IL_0292;
			}
			this.a = result2 != 0;
			goto IL_02a7;
			IL_02a7:
			return true;
			IL_01fa:
			if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ei()))
			{
				goto IL_0292;
			}
			B = Math.Max(3, Math.Min(60, result2));
			goto IL_02a7;
		}
	}
	internal sealed class b : Exception
	{
		public b(string P_0)
			: base(P_0)
		{
		}
	}
}
namespace b
{
	internal sealed class B
	{
		public long A;

		public long a;

		public uint A;

		public uint a;
	}
	internal sealed class b
	{
		public global::A.J A = new global::A.J();

		public global::B.s A = new global::B.s();

		public bool A = true;

		public bool a = true;

		public int A = 24;

		public decimal A = 2m;

		public decimal a = 8m;

		public decimal B = 5m;

		public bool B = true;

		public bool b;

		public bool C = true;

		public bool c = true;

		public bool D = true;

		public bool d = true;

		public bool E;

		public int a = 20;

		public int B = 45;

		public int b = 25;

		public int C;

		public decimal b = 2m;

		public int c = 40;

		public int D = 48;

		public int d = 12;

		public int E = 60;

		public int e = 20;

		public decimal C = 2m;

		public b A()
		{
			return new b
			{
				A = this.A,
				A = this.A.A(),
				A = this.A.B(),
				B = this.B,
				b = this.b,
				a = this.a,
				A = this.A,
				A = this.A,
				a = this.a,
				B = this.B,
				C = this.C,
				c = this.c,
				D = this.D,
				d = this.d,
				E = this.E,
				a = this.a,
				B = B,
				b = this.b,
				C = this.C,
				b = b,
				c = c,
				D = D,
				d = d,
				E = E,
				e = e,
				C = C
			};
		}

		public IEnumerable<string> a()
		{
			return new string[24]
			{
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kq() + (this.A ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KR() + this.a.ToString(CultureInfo.InvariantCulture),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kr() + this.B.ToString(CultureInfo.InvariantCulture),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KS() + (this.a ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ks() + this.A,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KT() + this.A.ToString(CultureInfo.InvariantCulture),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kt() + (this.C ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KU() + (this.c ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ku() + (this.D ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KV() + (this.d ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kv() + (this.E ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KW() + this.a,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kw() + B,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KX() + this.b,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kx() + this.C,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KY() + b.ToString(CultureInfo.InvariantCulture),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ky() + E,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.KZ() + e,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Kz() + (this.b ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kA() + (this.B ? 1 : 0),
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ka() + c,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kB() + D,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kb() + d,
				C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kC() + C.ToString(CultureInfo.InvariantCulture)
			}.Concat(this.A.b()).Concat(this.A.a());
		}

		public bool A(string P_0)
		{
			if (this.A.A(P_0) || this.A.A(P_0))
			{
				return true;
			}
			int num = P_0.IndexOf('=');
			if (num < 0)
			{
				return false;
			}
			string text = P_0.Substring(0, num);
			string text2 = P_0.Substring(num + 1);
			if (text != null)
			{
				int result;
				switch (text.Length)
				{
				case 9:
					switch (text[4])
					{
					case 'c':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kc()))
						{
							break;
						}
						this.A = text2 != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BY();
						return true;
					case 'C':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kD()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.A = Math.Max(8, Math.Min(160, result));
						}
						return true;
					case 'W':
					{
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kd()))
						{
							break;
						}
						if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result2))
						{
							this.A = Math.Max(0.5m, Math.Min(12m, result2));
						}
						return true;
					}
					case 'd':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kE()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							d = Math.Max(8, Math.Min(36, result));
						}
						return true;
					}
					break;
				case 14:
					switch (text[9])
					{
					case 'e':
					{
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ke()))
						{
							break;
						}
						if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result4))
						{
							this.a = Math.Max(1m, Math.Min(100m, result4));
						}
						return true;
					}
					case 'A':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kF()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.C = Math.Max(0, Math.Min(180, result));
						}
						return true;
					case 'W':
					{
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kf()))
						{
							break;
						}
						if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result3))
						{
							b = Math.Max(0.5m, Math.Min(12m, result3));
						}
						return true;
					}
					}
					break;
				case 11:
					switch (text[8])
					{
					case 'G':
					{
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kG()))
						{
							break;
						}
						if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result6))
						{
							this.B = Math.Max(1m, Math.Min(100m, result6));
						}
						return true;
					}
					case 'n':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kg()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.C = result != 0;
						}
						return true;
					case 'c':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kH()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							c = Math.Max(8, Math.Min(200, result));
						}
						return true;
					case 'a':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kh()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							D = Math.Max(8, Math.Min(200, result));
						}
						return true;
					case 'o':
					{
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kI()))
						{
							break;
						}
						if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result5))
						{
							C = Math.Max(0.5m, Math.Min(12m, result5));
						}
						return true;
					}
					}
					break;
				case 20:
					switch (text[9])
					{
					case 'A':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kJ()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.d = result != 0;
						}
						return true;
					case 'V':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kj()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.D = result != 0;
						}
						return true;
					}
					break;
				case 13:
					switch (text[9])
					{
					case 'A':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kK()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.c = result != 0;
						}
						return true;
					case 'T':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kk()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							B = Math.Max(1, Math.Min(200, result));
						}
						return true;
					case 'r':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kL()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.b = result != 0;
						}
						return true;
					}
					break;
				case 16:
					switch (text[9])
					{
					case 'S':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kl()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.E = result != 0;
						}
						return true;
					case 'V':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kM()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.a = Math.Max(1, Math.Min(200, result));
						}
						return true;
					}
					break;
				case 12:
					switch (text[0])
					{
					case 'm':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kN()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							E = Math.Max(10, Math.Min(144, result));
						}
						return true;
					case 's':
						if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kn()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.B = result != 0;
						}
						return true;
					}
					break;
				case 10:
					if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.ki()))
					{
						break;
					}
					if (int.TryParse(text2, out result))
					{
						this.a = result != 0;
					}
					return true;
				case 18:
					if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.km()))
					{
						break;
					}
					if (int.TryParse(text2, out result))
					{
						this.b = Math.Max(1, Math.Min(200, result));
					}
					return true;
				case 15:
					if (!(text == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.kO()))
					{
						break;
					}
					if (int.TryParse(text2, out result))
					{
						e = Math.Max(5, Math.Min(144, result));
					}
					return true;
				}
			}
			return false;
		}
	}
}
