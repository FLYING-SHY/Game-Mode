using System.Drawing;
using System.Runtime.InteropServices;

namespace A
{
	internal sealed class L
	{
		public string A;

		public string a;

		public long A;

		public uint A;

		public Color A;

		public float A;

		public float a;

		public float B;

		public float b;

		public float C;

		public float c;

		public float D;

		public float d;

		public float E;

		public bool A;

		public bool a;
	}
	internal enum l
	{
		A,
		a,
		B,
		b,
		C,
		c,
		D
	}
}
namespace a
{
	internal static class L
	{
		public static bool A(this string[] P_0, string P_1)
		{
			for (int num = 0; num < P_0.Length; num++)
			{
				if (P_0[num] == P_1)
				{
					return true;
				}
			}
			return false;
		}
	}
	internal static class l
	{
		public static long A => checked((long)a());

		[DllImport("kernel32.dll", EntryPoint = "GetTickCount64")]
		private static extern ulong a();
	}
}
