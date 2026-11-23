using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using _003CPrivateImplementationDetails_003E_007B5DBBFACD_002D3611_002D46D1_002D9D2A_002D1FE02ADBEEEB_007D;
using A;
using a;
using b;

namespace B;

internal sealed class O
{
	internal bool A;

	internal ulong? A;

	public readonly List<Q> A = new List<Q>();

	public bool a = true;

	public bool B = true;

	public readonly List<global::A.T> A = new List<global::A.T>();

	public long A;

	public long a;

	public bool b;

	public Color A;

	public uint A;

	public uint a;

	public string A;

	public string a;

	public string B;

	public float A;

	public float a;

	public float B;

	public float b;

	public bool C = true;

	public readonly List<global::b.d> A = new List<global::b.d>();

	public readonly List<global::a.x> A = new List<global::a.x>();

	public bool c;

	public readonly Dictionary<string, int> A = new Dictionary<string, int>();

	public float? A;

	public bool D = true;

	public readonly List<Q> a = new List<Q>();

	public float C;

	public string b;

	public bool d;

	public readonly List<global::b.K> A = new List<global::b.K>();

	public readonly List<global::A.p> A = new List<global::A.p>();

	public readonly List<string> A = new List<string>();
}
internal sealed class o : IDisposable
{
	private readonly IntPtr m_A;

	private readonly int m_A;

	private readonly Thread m_A;

	private readonly ManualResetEvent m_A = new ManualResetEvent(initialState: false);

	private readonly AutoResetEvent m_A = new AutoResetEvent(initialState: false);

	private int m_a;

	private int m_B;

	private int m_b;

	private long m_A;

	[CompilerGenerated]
	private bool m_A;

	public bool A
	{
		[CompilerGenerated]
		get
		{
			return this.m_A;
		}
		[CompilerGenerated]
		private set
		{
			this.m_A = flag;
		}
	}

	public int a
	{
		get
		{
			return Volatile.Read(ref this.m_a);
		}
		set
		{
			int num = Math.Max(1, Math.Min(240, val));
			if (Interlocked.Exchange(ref this.m_a, num) != num)
			{
				this.m_A.Set();
			}
		}
	}

	public long B => Interlocked.Read(ref this.m_A);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "CreateWaitableTimerEx", SetLastError = true)]
	private static extern IntPtr A(IntPtr P_0, string P_1, uint P_2, uint P_3);

	[DllImport("kernel32.dll", EntryPoint = "SetWaitableTimer", SetLastError = true)]
	private static extern bool A(IntPtr P_0, ref long P_1, int P_2, IntPtr P_3, IntPtr P_4, bool P_5);

	[DllImport("kernel32.dll", EntryPoint = "WaitForMultipleObjects")]
	private static extern uint A(uint P_0, IntPtr[] P_1, bool P_2, uint P_3);

	[DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
	private static extern bool A(IntPtr P_0);

	[DllImport("user32.dll", EntryPoint = "PostMessage", SetLastError = true)]
	private static extern bool A(IntPtr P_0, int P_1, IntPtr P_2, IntPtr P_3);

	[Conditional("SKYEYE_PIPELINE_TRACE")]
	public void b()
	{
	}

	public o(IntPtr P_0, int P_1, int P_2)
	{
		this.m_A = P_0;
		this.m_A = P_1;
		this.m_a = P_2;
		this.m_A = new Thread(D)
		{
			IsBackground = true,
			Name = CD95D04D_002DF435_002D4934_002D84FA_002D7CEBE72F9F4B.jS()
		};
		this.m_A.Start();
	}

	public void C()
	{
		if (Volatile.Read(ref this.m_b) == 0)
		{
			if (Interlocked.CompareExchange(ref this.m_B, 1, 0) != 0)
			{
				Interlocked.Increment(ref this.m_A);
			}
			else if (!A(this.m_A, this.m_A, IntPtr.Zero, IntPtr.Zero))
			{
				Interlocked.Exchange(ref this.m_B, 0);
			}
		}
	}

	public void c()
	{
		Interlocked.Exchange(ref this.m_B, 0);
	}

	private void D()
	{
		IntPtr intPtr = A(IntPtr.Zero, null, 2u, 2031619u);
		this.A = intPtr != IntPtr.Zero;
		if (intPtr == IntPtr.Zero)
		{
			intPtr = A(IntPtr.Zero, null, 0u, 2031619u);
		}
		try
		{
			double num = Stopwatch.GetTimestamp();
			IntPtr[] array = new IntPtr[3]
			{
				this.m_A.SafeWaitHandle.DangerousGetHandle(),
				intPtr,
				this.m_A.SafeWaitHandle.DangerousGetHandle()
			};
			while (!this.m_A.WaitOne(0))
			{
				double num2 = Stopwatch.GetTimestamp();
				double num3 = (double)Stopwatch.Frequency / (double)a;
				num += num3;
				if (num < num2)
				{
					num = num2 + num3;
				}
				if (intPtr == IntPtr.Zero)
				{
					if (!this.m_A.WaitOne(Math.Max(1, (int)Math.Ceiling((num - num2) * 1000.0 / (double)Stopwatch.Frequency))))
					{
						C();
						continue;
					}
					break;
				}
				long num4 = -Math.Max(1L, (long)Math.Ceiling((num - num2) * 10000000.0 / (double)Stopwatch.Frequency));
				if (!A(intPtr, ref num4, 0, IntPtr.Zero, IntPtr.Zero, false))
				{
					if (!this.m_A.WaitOne(10))
					{
						num = Stopwatch.GetTimestamp();
						continue;
					}
					break;
				}
				switch (A(3u, array, false, 1000u))
				{
				case 2u:
					num = Stopwatch.GetTimestamp();
					break;
				case 1u:
					C();
					break;
				default:
					num = Stopwatch.GetTimestamp();
					break;
				case 0u:
					return;
				}
			}
		}
		finally
		{
			if (intPtr != IntPtr.Zero)
			{
				A(intPtr);
			}
		}
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref this.m_b, 1) == 0)
		{
			this.m_A.Set();
			if (Thread.CurrentThread != this.m_A)
			{
				this.m_A.Join();
			}
			this.m_A.Dispose();
			this.m_A.Dispose();
		}
	}
}
