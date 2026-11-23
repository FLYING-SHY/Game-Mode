using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using a;
using b;

namespace A
{
	internal static class V
	{
		public static bool A(this byte[] P_0, byte[] P_1)
		{
			if (P_0.Length == P_1.Length && P_0[0] == P_1[0])
			{
				return P_0[1] == P_1[1];
			}
			return false;
		}
	}
	internal static class v
	{
		[DllImport("user32.dll", EntryPoint = "SetProcessDPIAware")]
		private static extern bool A();

		[STAThread]
		public static int A(string[] P_0)
		{
			A();
			R.A();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			bool createdNew;
			using (new Mutex(initiallyOwned: true, CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cL(), out createdNew))
			{
				if (!createdNew)
				{
					MessageBox.Show(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cl(), CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.cM());
					return 1;
				}
				Application.Run(new global::a.k());
			}
			return 0;
		}
	}
}
namespace a
{
	internal sealed class V
	{
		public int A;

		public long A;

		public long a;

		public uint A;

		public DateTime A;

		public string A;
	}
	internal sealed class v
	{
		public V A;

		public global::b.d[] A;

		public global::B.T[] A;

		public global::B.T[] a;

		public DateTime A;

		public string A;
	}
}
