using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using B;

namespace B
{
	internal sealed class C
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
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eE() + (this.A ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ee() + this.A,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eF() + (this.a ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ef() + a,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eG() + B,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eg() + b,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eH() + (this.B ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eh() + this.A.ToString(CultureInfo.InvariantCulture),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eI() + C,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ei() + (this.b ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eJ() + c
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
			if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ej()) && !(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eK()))
			{
				if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ek())
				{
					if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
					{
						this.A = Math.Max(0.5m, Math.Min(8m, result));
					}
					return true;
				}
				if (text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eL() && text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.el() && text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eM() && text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.em() && text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eN() && text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.en() && text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eO() && text != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eo())
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
						if (c2 != 'M' || !(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eN()))
						{
							goto IL_0292;
						}
						this.B = result2 != 0;
					}
					else
					{
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.el()))
						{
							goto IL_0292;
						}
						a = Math.Max(2, Math.Min(40, result2));
					}
				}
				else
				{
					if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eO()))
					{
						goto IL_0292;
					}
					this.b = result2 != 0;
				}
				goto IL_02a7;
			}
			if (int.TryParse(text2, out var result3))
			{
				if (text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ej())
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
			if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.en()))
			{
				goto IL_0292;
			}
			C = Math.Max(2, Math.Min(40, result2));
			goto IL_02a7;
			IL_0292:
			b = Math.Max(1, Math.Min(60, result2));
			goto IL_02a7;
			IL_01d6:
			if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eo()))
			{
				goto IL_0292;
			}
			c = Math.Max(4, Math.Min(60, result2));
			goto IL_02a7;
			IL_0188:
			if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eL()))
			{
				goto IL_0292;
			}
			this.a = result2 != 0;
			goto IL_02a7;
			IL_02a7:
			return true;
			IL_01fa:
			if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.eM()))
			{
				goto IL_0292;
			}
			B = Math.Max(3, Math.Min(60, result2));
			goto IL_02a7;
		}
	}
	internal sealed class c : Exception
	{
		public c(string P_0)
			: base(P_0)
		{
		}
	}
}
namespace b
{
	internal sealed class C
	{
		public long A;

		public long a;

		public uint A;

		public uint a;
	}
	internal sealed class c
	{
		public global::A.j A = new global::A.j();

		public global::B.t A = new global::B.t();

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

		public c A()
		{
			return new c
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
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KV() + (this.A ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kv() + this.a.ToString(CultureInfo.InvariantCulture),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KW() + this.B.ToString(CultureInfo.InvariantCulture),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kw() + (this.a ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KX() + this.A,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kx() + this.A.ToString(CultureInfo.InvariantCulture),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KY() + (this.C ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ky() + (this.c ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.KZ() + (this.D ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Kz() + (this.d ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kA() + (this.E ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ka() + this.a,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kB() + B,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kb() + this.b,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kC() + this.C,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kc() + b.ToString(CultureInfo.InvariantCulture),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kD() + E,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kd() + e,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kE() + (this.b ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ke() + (this.B ? 1 : 0),
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kF() + c,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kf() + D,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kG() + d,
				CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kg() + C.ToString(CultureInfo.InvariantCulture)
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kH()))
						{
							break;
						}
						this.A = text2 != CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.By();
						return true;
					case 'C':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kh()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kI()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ki()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kJ()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kj()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kK()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kk()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kL()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.C = result != 0;
						}
						return true;
					case 'c':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kl()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							c = Math.Max(8, Math.Min(200, result));
						}
						return true;
					case 'a':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kM()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.km()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kn()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.d = result != 0;
						}
						return true;
					case 'V':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kO()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ko()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.c = result != 0;
						}
						return true;
					case 'T':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kP()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							B = Math.Max(1, Math.Min(200, result));
						}
						return true;
					case 'r':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kp()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kQ()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.E = result != 0;
						}
						return true;
					case 'V':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kq()))
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
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kr()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							E = Math.Max(10, Math.Min(144, result));
						}
						return true;
					case 's':
						if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kS()))
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
					if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kN()))
					{
						break;
					}
					if (int.TryParse(text2, out result))
					{
						this.a = result != 0;
					}
					return true;
				case 18:
					if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.kR()))
					{
						break;
					}
					if (int.TryParse(text2, out result))
					{
						this.b = Math.Max(1, Math.Min(200, result));
					}
					return true;
				case 15:
					if (!(text == CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.ks()))
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
