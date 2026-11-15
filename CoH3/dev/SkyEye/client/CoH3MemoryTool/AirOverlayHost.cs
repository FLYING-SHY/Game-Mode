using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class AirOverlayHost : IDisposable
{
	private readonly SquadMarkerPreferences preferences;

	private readonly TacticalMapPreferences51 tactical51;

	private readonly Thread thread;

	private AirOverlayWindow window;

	private int disposed;

	private volatile string error = "";

	public string LastError
	{
		get
		{
			if (error.Length <= 0)
			{
				if (window != null)
				{
					return window.LastError;
				}
				return "";
			}
			return error;
		}
	}

	public AirOverlayHost(SquadMarkerPreferences p, bool track = true, TacticalMapPreferences51 tactical = null)
	{
		AirOverlayHost airOverlayHost = this;
		preferences = p;
		tactical51 = tactical ?? new TacticalMapPreferences51
		{
			Enabled = false
		};
		if (!track)
		{
			window = new AirOverlayWindow(p.Copy(), track: false, tactical51);
			return;
		}
		ManualResetEvent ready = new ManualResetEvent(initialState: false);
		try
		{
			thread = new Thread((ThreadStart)delegate
			{
				bool flag = false;
				try
				{
					airOverlayHost.window = new AirOverlayWindow(p.Copy(), track: true, airOverlayHost.tactical51);
					_ = airOverlayHost.window.Handle;
					flag = true;
					ready.Set();
					Application.Run();
				}
				catch (Exception ex)
				{
					airOverlayHost.error = ex.Message;
					if (!flag)
					{
						ready.Set();
					}
				}
				finally
				{
					if (airOverlayHost.window != null)
					{
						airOverlayHost.window.Dispose();
					}
				}
			})
			{
				IsBackground = true,
				Name = "CoH3 airstrike alpha renderer"
			};
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			ready.WaitOne();
		}
		finally
		{
			if (ready != null)
			{
				((IDisposable)ready).Dispose();
			}
		}
	}

	public void ApplyAppearance()
	{
		if (window != null && Volatile.Read(ref disposed) == 0)
		{
			window.ApplySettings(preferences.Copy(), tactical51);
		}
	}

	public void UpdateRows(PlayerInfoReader reader, int gamePid, IEnumerable<PlayerInfoRow> rows)
	{
		if (window != null && Volatile.Read(ref disposed) == 0)
		{
			window.Publish(new MarkerScene
			{
				Context = reader.MarkerContext(),
				Players = (from p in rows
					where p.Relation == "敌方"
					select new ProjectilePlayer
					{
						Address = p.PlayerAddress,
						Slot = p.Slot,
						Colour = (p.GameColourKnown ? p.GameColour : Color.Red)
					}).ToArray(),
				Updated = DateTime.UtcNow
			});
		}
	}

	public void TestAir()
	{
		if (window != null)
		{
			window.TestAir();
		}
	}

	public void ClearAir()
	{
		if (window != null)
		{
			window.ClearAir();
		}
	}

	public void ClearData()
	{
		if (window != null && Volatile.Read(ref disposed) == 0)
		{
			window.Publish(null);
		}
	}

	public void Dispose()
	{
		if (Interlocked.Exchange(ref disposed, 1) != 0 || window == null)
		{
			return;
		}
		if (thread == null)
		{
			window.Dispose();
			return;
		}
		try
		{
			window.BeginInvoke((Action)delegate
			{
				window.Close();
				Application.ExitThread();
			});
		}
		catch (InvalidOperationException)
		{
		}
		if (Thread.CurrentThread != thread)
		{
			thread.Join();
		}
	}
}
