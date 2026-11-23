using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using B;
using a;

namespace B
{
	internal static class H
	{
		public struct A
		{
			public IntPtr A;

			public IntPtr a;

			public uint A;

			public UIntPtr A;

			public uint a;

			public uint B;

			public uint b;
		}

		[DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
		public static extern p A(uint P_0, bool P_1, int P_2);

		[DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
		public static extern bool A(IntPtr P_0);

		[DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
		private static extern bool A(p P_0, IntPtr P_1, byte[] P_2, UIntPtr P_3, out UIntPtr P_4);

		public static bool a(p P_0, IntPtr P_1, byte[] P_2, UIntPtr P_3, out UIntPtr P_4)
		{
			if (!global::a.O.A)
			{
				P_4 = UIntPtr.Zero;
				return false;
			}
			return A(P_0, P_1, P_2, P_3, out P_4);
		}

		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx", SetLastError = true)]
		public static extern UIntPtr A(p P_0, IntPtr P_1, out A P_2, UIntPtr P_3);

		[DllImport("user32.dll", EntryPoint = "RegisterHotKey", SetLastError = true)]
		public static extern bool A(IntPtr P_0, int P_1, uint P_2, uint P_3);

		[DllImport("user32.dll", EntryPoint = "UnregisterHotKey")]
		public static extern bool A(IntPtr P_0, int P_1);

		[DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
		public static extern IntPtr A();

		[DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
		public static extern uint A(IntPtr P_0, out uint P_1);
	}
	internal sealed class h
	{
		public int A;

		public int a;

		public double A;

		public Y A;
	}
}
namespace b
{
	internal static class H
	{
		public static double A(string P_0)
		{
			if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.Ll()))
			{
				if (!P_0.Contains(CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.LM()))
				{
					return 60.0;
				}
				return 13.5;
			}
			return 34.0;
		}
	}
	internal sealed class h : Panel
	{
		public h()
		{
			DoubleBuffered = true;
			BackColor = global::B.L.A;
		}

		protected override void OnPaintBackground(PaintEventArgs P_0)
		{
			base.OnPaintBackground(P_0);
			using (Pen pen = new Pen(Color.FromArgb(14, 29, 41)))
			{
				for (int n = 20; n < base.Width; n += 48)
				{
					P_0.Graphics.DrawLine(pen, n, 0, n, base.Height);
				}
				for (int num = 20; num < base.Height; num += 48)
				{
					P_0.Graphics.DrawLine(pen, 0, num, base.Width, num);
				}
			}
			global::B.L.A(P_0.Graphics, new Rectangle(0, 0, base.Width, base.Height), global::B.L.c);
			using Pen pen2 = new Pen(global::B.L.D);
			P_0.Graphics.DrawLine(pen2, 12, 4, 70, 4);
			P_0.Graphics.DrawLine(pen2, base.Width - 12, base.Height - 5, base.Width - 70, base.Height - 5);
		}
	}
}
