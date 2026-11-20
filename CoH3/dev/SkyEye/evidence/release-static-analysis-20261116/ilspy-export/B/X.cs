using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using _003CPrivateImplementationDetails_003E_007B3EEC50DA_002D68B4_002D4495_002DA655_002D37D59591F8B2_007D;

namespace B;

internal sealed class X
{
	public bool A = true;

	public Color A = Color.White;

	public Color a = Color.Gold;

	public Color B = Color.Red;

	public decimal A = 100m;

	public decimal a = 100m;

	public decimal B = 1m;

	public Color A(double P_0, double P_1)
	{
		if (double.IsNaN(P_0) || double.IsInfinity(P_0))
		{
			return this.A;
		}
		if (P_0 > (double)a)
		{
			if ((long)Math.Floor(Math.Max(0.0, P_1) * (double)B * 2.0) % 2 != 0L)
			{
				return this.B;
			}
			return this.a;
		}
		double num = Math.Max(0.0, Math.Min(1.0, P_0 / (double)Math.Max(1m, this.A)));
		return Color.FromArgb(255, A(this.A.R, this.a.R, num), A(this.A.G, this.a.G, num), A(this.A.B, this.a.B, num));
	}

	private static int A(int P_0, int P_1, double P_2)
	{
		return (int)Math.Round((double)P_0 + (double)(P_1 - P_0) * P_2);
	}

	public string A()
	{
		return string.Join(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.bq(), this.A ? C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BW() : C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BY(), this.A.ToArgb().ToString(), this.a.ToArgb().ToString(), this.B.ToArgb().ToString(), this.A.ToString(CultureInfo.InvariantCulture), a.ToString(CultureInfo.InvariantCulture), B.ToString(CultureInfo.InvariantCulture));
	}

	public void A(string P_0)
	{
		string[] array = P_0.Split('|');
		if (array.Length == 7 && (!(array[0] != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BY()) || !(array[0] != C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BW())) && int.TryParse(array[1], out var result) && int.TryParse(array[2], out var result2) && int.TryParse(array[3], out var result3) && decimal.TryParse(array[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var result4) && decimal.TryParse(array[5], NumberStyles.Number, CultureInfo.InvariantCulture, out var result5) && decimal.TryParse(array[6], NumberStyles.Number, CultureInfo.InvariantCulture, out var result6) && !(result4 < 1m) && !(result4 > 1000000m) && !(result5 < 0m) && !(result5 > 1000000m) && !(result6 < 0.1m) && !(result6 > 5m))
		{
			this.A = array[0] == C4318269_002D39F2_002D4549_002D851A_002D418926B36435.BW();
			this.A = Color.FromArgb(255, Color.FromArgb(result));
			this.a = Color.FromArgb(255, Color.FromArgb(result2));
			this.B = Color.FromArgb(255, Color.FromArgb(result3));
			this.A = result4;
			a = result5;
			B = result6;
		}
	}
}
internal sealed class x
{
	[Serializable]
	[CompilerGenerated]
	private sealed class A
	{
		public static readonly A A = new A();

		public static Func<KeyValuePair<string, X>, string> A;

		internal string A(KeyValuePair<string, X> P_0)
		{
			string text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ki();
			KeyValuePair<string, X> keyValuePair = P_0;
			string key = keyValuePair.Key;
			string text2 = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FQ();
			keyValuePair = P_0;
			return text + key + text2 + keyValuePair.Value.A();
		}
	}

	public readonly X A = new X
	{
		A = 1000m,
		a = 1000m
	};

	public readonly X a = new X
	{
		A = 100m,
		a = 100m
	};

	public readonly X B = new X
	{
		A = 100m,
		a = 100m
	};

	public readonly X b = new X
	{
		A = 80m,
		a = 80m
	};

	public IEnumerable<KeyValuePair<string, X>> A()
	{
		yield return new KeyValuePair<string, X>(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FL(), this.A);
		yield return new KeyValuePair<string, X>(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Fl(), this.a);
		yield return new KeyValuePair<string, X>(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FM(), B);
		yield return new KeyValuePair<string, X>(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Fm(), b);
	}

	public bool A(string P_0)
	{
		if (!P_0.StartsWith(C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ki(), StringComparison.Ordinal))
		{
			return false;
		}
		int num = P_0.IndexOf('=');
		if (num < 0)
		{
			return true;
		}
		string text = P_0.Substring(9, num - 9);
		foreach (KeyValuePair<string, X> item in A())
		{
			if (item.Key == text)
			{
				item.Value.A(P_0.Substring(num + 1));
			}
		}
		return true;
	}

	public IEnumerable<string> a()
	{
		return A().Select(delegate(KeyValuePair<string, X> P_0)
		{
			string text = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.Ki();
			KeyValuePair<string, X> keyValuePair = P_0;
			string key = keyValuePair.Key;
			string text2 = C4318269_002D39F2_002D4549_002D851A_002D418926B36435.FQ();
			keyValuePair = P_0;
			return text + key + text2 + keyValuePair.Value.A();
		});
	}
}
