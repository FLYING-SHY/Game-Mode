using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using _003CPrivateImplementationDetails_003E_007B39A8E59D_002D61FC_002D4FB4_002D8036_002DA8A4A41E5D63_007D;
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
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ea() + (this.A ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eB() + this.A,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eb() + (this.a ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eC() + a,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ec() + B,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eD() + b,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ed() + (this.B ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eE() + this.A.ToString(CultureInfo.InvariantCulture),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ee() + C,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eF() + (this.b ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ef() + c
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
			if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eG()) && !(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eg()))
			{
				if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eH())
				{
					if (decimal.TryParse(text2, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
					{
						this.A = Math.Max(0.5m, Math.Min(8m, result));
					}
					return true;
				}
				if (text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eh() && text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eI() && text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ei() && text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eJ() && text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ej() && text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eK() && text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ek() && text != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eL())
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
						if (c2 != 'M' || !(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ej()))
						{
							goto IL_0292;
						}
						this.B = result2 != 0;
					}
					else
					{
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eI()))
						{
							goto IL_0292;
						}
						a = Math.Max(2, Math.Min(40, result2));
					}
				}
				else
				{
					if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ek()))
					{
						goto IL_0292;
					}
					this.b = result2 != 0;
				}
				goto IL_02a7;
			}
			if (int.TryParse(text2, out var result3))
			{
				if (text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eG())
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
			if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eK()))
			{
				goto IL_0292;
			}
			C = Math.Max(2, Math.Min(40, result2));
			goto IL_02a7;
			IL_0292:
			b = Math.Max(1, Math.Min(60, result2));
			goto IL_02a7;
			IL_01d6:
			if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eL()))
			{
				goto IL_0292;
			}
			c = Math.Max(4, Math.Min(60, result2));
			goto IL_02a7;
			IL_0188:
			if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.eh()))
			{
				goto IL_0292;
			}
			this.a = result2 != 0;
			goto IL_02a7;
			IL_02a7:
			return true;
			IL_01fa:
			if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ei()))
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
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kq() + (this.A ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KR() + this.a.ToString(CultureInfo.InvariantCulture),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kr() + this.B.ToString(CultureInfo.InvariantCulture),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KS() + (this.a ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ks() + this.A,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KT() + this.A.ToString(CultureInfo.InvariantCulture),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kt() + (this.C ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KU() + (this.c ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ku() + (this.D ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KV() + (this.d ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kv() + (this.E ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KW() + this.a,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kw() + B,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KX() + this.b,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kx() + this.C,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KY() + b.ToString(CultureInfo.InvariantCulture),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Ky() + E,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.KZ() + e,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.Kz() + (this.b ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kA() + (this.B ? 1 : 0),
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ka() + c,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kB() + D,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kb() + d,
				_3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kC() + C.ToString(CultureInfo.InvariantCulture)
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kc()))
						{
							break;
						}
						this.A = text2 != _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.BY();
						return true;
					case 'C':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kD()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kd()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kE()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ke()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kF()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kf()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kG()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kg()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.C = result != 0;
						}
						return true;
					case 'c':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kH()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							c = Math.Max(8, Math.Min(200, result));
						}
						return true;
					case 'a':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kh()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kI()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kJ()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.d = result != 0;
						}
						return true;
					case 'V':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kj()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kK()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.c = result != 0;
						}
						return true;
					case 'T':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kk()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							B = Math.Max(1, Math.Min(200, result));
						}
						return true;
					case 'r':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kL()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kl()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							this.E = result != 0;
						}
						return true;
					case 'V':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kM()))
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
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kN()))
						{
							break;
						}
						if (int.TryParse(text2, out result))
						{
							E = Math.Max(10, Math.Min(144, result));
						}
						return true;
					case 's':
						if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kn()))
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
					if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.ki()))
					{
						break;
					}
					if (int.TryParse(text2, out result))
					{
						this.a = result != 0;
					}
					return true;
				case 18:
					if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.km()))
					{
						break;
					}
					if (int.TryParse(text2, out result))
					{
						this.b = Math.Max(1, Math.Min(200, result));
					}
					return true;
				case 15:
					if (!(text == _3FEA2BAD_002D15F9_002D4056_002D9078_002D73D0FDA796DC.kO()))
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
