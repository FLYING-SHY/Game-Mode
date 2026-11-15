using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CoH3MemoryTool;

internal sealed class PrecisionFrameClock : IDisposable
{
	private readonly IntPtr window;

	private readonly int message;

	private readonly Thread thread;

	private readonly ManualResetEvent stop = new ManualResetEvent(initialState: false);

	private readonly AutoResetEvent changed = new AutoResetEvent(initialState: false);

	private int rate;

	private int pending;

	private int disposed;

	private long skipped;

	public bool HighResolution { get; private set; }

	public int Rate
	{
		get
		{
			return Volatile.Read(ref rate);
		}
		set
		{
			int num = Math.Max(1, Math.Min(240, value));
			if (Interlocked.Exchange(ref rate, num) != num)
			{
				changed.Set();
			}
		}
	}

	public long Skipped => Interlocked.Read(ref skipped);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr CreateWaitableTimerEx(IntPtr attrs, string name, uint flags, uint access);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetWaitableTimer(IntPtr timer, ref long due, int period, IntPtr callback, IntPtr arg, bool resume);

	[DllImport("kernel32.dll")]
	private static extern uint WaitForMultipleObjects(uint count, IntPtr[] handles, bool all, uint milliseconds);

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(IntPtr h);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool PostMessage(IntPtr h, int msg, IntPtr w, IntPtr l);

	[Conditional("SKYEYE_PIPELINE_TRACE")]
	public void TraceStatus()
	{
	}

	public PrecisionFrameClock(IntPtr target, int msg, int initialRate)
	{
		window = target;
		message = msg;
		rate = initialRate;
		thread = new Thread(Run)
		{
			IsBackground = true,
			Name = "CoH3 marker frame clock"
		};
		thread.Start();
	}

	public void RequestNow()
	{
		if (Volatile.Read(ref disposed) == 0)
		{
			if (Interlocked.CompareExchange(ref pending, 1, 0) != 0)
			{
				Interlocked.Increment(ref skipped);
			}
			else if (!PostMessage(window, message, IntPtr.Zero, IntPtr.Zero))
			{
				Interlocked.Exchange(ref pending, 0);
			}
		}
	}

	public void Complete()
	{
		Interlocked.Exchange(ref pending, 0);
	}

	private void Run()
	{
		IntPtr intPtr = CreateWaitableTimerEx(IntPtr.Zero, null, 2u, 2031619u);
		HighResolution = intPtr != IntPtr.Zero;
		if (intPtr == IntPtr.Zero)
		{
			intPtr = CreateWaitableTimerEx(IntPtr.Zero, null, 0u, 2031619u);
		}
		try
		{
			double num = Stopwatch.GetTimestamp();
			IntPtr[] handles = new IntPtr[3]
			{
				stop.SafeWaitHandle.DangerousGetHandle(),
				intPtr,
				changed.SafeWaitHandle.DangerousGetHandle()
			};
			while (!stop.WaitOne(0))
			{
				double num2 = Stopwatch.GetTimestamp();
				double num3 = (double)Stopwatch.Frequency / (double)Rate;
				num += num3;
				if (num < num2)
				{
					num = num2 + num3;
				}
				if (intPtr == IntPtr.Zero)
				{
					if (!stop.WaitOne(Math.Max(1, (int)Math.Ceiling((num - num2) * 1000.0 / (double)Stopwatch.Frequency))))
					{
						RequestNow();
						continue;
					}
					break;
				}
				long due = -Math.Max(1L, (long)Math.Ceiling((num - num2) * 10000000.0 / (double)Stopwatch.Frequency));
				if (!SetWaitableTimer(intPtr, ref due, 0, IntPtr.Zero, IntPtr.Zero, resume: false))
				{
					if (!stop.WaitOne(10))
					{
						num = Stopwatch.GetTimestamp();
						continue;
					}
					break;
				}
				switch (WaitForMultipleObjects(3u, handles, all: false, 1000u))
				{
				case 2u:
					num = Stopwatch.GetTimestamp();
					break;
				case 1u:
					RequestNow();
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
				CloseHandle(intPtr);
			}
		}
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref disposed, 1) == 0)
		{
			stop.Set();
			if (Thread.CurrentThread != thread)
			{
				thread.Join();
			}
			stop.Dispose();
			changed.Dispose();
		}
	}
}
