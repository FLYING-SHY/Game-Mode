using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class SquadMarkerHost : IDisposable
{
	private readonly SquadMarkerPreferences preferences;

	private readonly Thread thread;

	private SquadMarkerWindow window;

	private volatile string error = "";

	private int disposed;

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
				return "标记绘制尚未启动。";
			}
			return error;
		}
	}

	public string Statistics
	{
		get
		{
			if (window != null)
			{
				return window.Statistics;
			}
			return "尚无绘制测量。";
		}
	}

	public SquadMarkerHost(SquadMarkerPreferences p, bool track = true)
	{
		SquadMarkerHost squadMarkerHost = this;
		preferences = p;
		if (!track)
		{
			window = new SquadMarkerWindow(p, track: false);
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
					squadMarkerHost.window = new SquadMarkerWindow(p.Copy());
					_ = squadMarkerHost.window.Handle;
					flag = true;
					ready.Set();
					Application.Run();
				}
				catch (Exception ex)
				{
					squadMarkerHost.error = ex.Message;
					if (!flag)
					{
						ready.Set();
					}
				}
				finally
				{
					if (squadMarkerHost.window != null)
					{
						squadMarkerHost.window.Dispose();
					}
				}
			})
			{
				IsBackground = true,
				Name = "CoH3 independent marker renderer"
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
			window.ApplySettings(preferences.Copy());
		}
	}

	public void UpdateRows(PlayerInfoReader r, int gamePid, IEnumerable<PlayerInfoRow> rows)
	{
		if (window == null || Volatile.Read(ref disposed) != 0)
		{
			return;
		}
		PlayerInfoRow[] source = rows.Where((PlayerInfoRow p) => p.Relation == "敌方").ToArray();
		List<SquadMarkerSource> list = new List<SquadMarkerSource>();
		foreach (PlayerInfoRow item in source.Where((PlayerInfoRow p) => p.MarkerDataKnown))
		{
			foreach (SquadMarkerSource markerSquad in item.MarkerSquads)
			{
				SquadMarkerSource squadMarkerSource = new SquadMarkerSource();
				squadMarkerSource.Squad = markerSquad.Squad;
				squadMarkerSource.Identity = markerSquad.Identity;
				squadMarkerSource.Path = markerSquad.Path;
				squadMarkerSource.Slot = markerSquad.Slot;
				squadMarkerSource.ColourKnown = item.GameColourKnown;
				squadMarkerSource.Colour = item.GameColour;
				SquadMarkerSource squadMarkerSource2 = squadMarkerSource;
				foreach (SquadMarkerMember member in markerSquad.Members)
				{
					squadMarkerSource2.Members.Add(new SquadMarkerMember
					{
						Entity = member.Entity,
						Owner = member.Owner,
						Generation = member.Generation,
						SquadOffset = member.SquadOffset
					});
				}
				list.Add(squadMarkerSource2);
			}
		}
		string warning = (source.Any((PlayerInfoRow p) => !p.MarkerDataKnown) ? "部分班组数据正在更新。" : (source.Any((PlayerInfoRow p) => p.MarkerSquads.Count > 0 && !p.GameColourKnown) ? "部分玩家颜色尚未读到。" : ""));
		window.PublishScene(new MarkerScene
		{
			Context = r.MarkerContext(),
			Sources = list.ToArray(),
			ProjectilePlayers = (from p in rows
				where p.Relation == "敌方" || p.Relation == "自己" || p.Relation == "队友"
				select new ProjectilePlayer
				{
					Address = p.PlayerAddress,
					Slot = p.Slot,
					Relation = p.Relation,
					Colour = (p.GameColourKnown ? p.GameColour : Color.Silver)
				}).ToArray(),
			Players = source.Select((PlayerInfoRow p) => new ProjectilePlayer
			{
				Address = p.PlayerAddress,
				Slot = p.Slot,
				Colour = (p.GameColourKnown ? p.GameColour : Color.Red)
			}).ToArray(),
			Updated = DateTime.UtcNow,
			Warning = warning
		});
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
			window.PublishScene(null);
		}
	}

	public void Close()
	{
		Dispose();
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
