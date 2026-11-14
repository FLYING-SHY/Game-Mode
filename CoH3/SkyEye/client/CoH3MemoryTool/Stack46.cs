using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Stack46 : Panel
{
	private bool fitting;

	private readonly VScrollBar scroll = new VScrollBar();

	public Stack46()
	{
		Dock = DockStyle.Fill;
		AutoScroll = false;
		base.Padding = new Padding(0, 0, 8, 4);
		base.Margin = Padding.Empty;
		BackColor = Palette46.Back;
		DoubleBuffered = true;
		scroll.Dock = DockStyle.Right;
		scroll.Width = 14;
		scroll.Visible = false;
		base.Controls.Add(scroll);
		VScrollBar vScrollBar = scroll;
		EventHandler value = delegate
		{
			Position();
		};
		vScrollBar.ValueChanged += value;
		base.SizeChanged += delegate
		{
			Fit();
		};
		base.ControlAdded += delegate(object sender, ControlEventArgs e)
		{
			e.Control.SizeChanged += delegate
			{
				Fit();
			};
			Fit();
		};
		base.Layout += delegate
		{
			Fit();
		};
	}

	private int Total()
	{
		return base.Controls.OfType<Module46>().Sum((Module46 c) => c.Height + 12) + base.Padding.Bottom;
	}

	private void Position()
	{
		int num = 0;
		foreach (Module46 item in base.Controls.OfType<Module46>())
		{
			item.Location = new Point(0, num - scroll.Value);
			num += item.Height + 12;
		}
	}

	public void Fit()
	{
		if (fitting)
		{
			return;
		}
		fitting = true;
		try
		{
			for (int i = 0; i < 2; i++)
			{
				foreach (Module46 item in base.Controls.OfType<Module46>())
				{
					item.Width = Math.Max(312, base.ClientSize.Width - base.Padding.Horizontal - (scroll.Visible ? scroll.Width : 0) - 2);
					item.Fit();
				}
				scroll.Visible = Total() > base.ClientSize.Height;
			}
			scroll.Minimum = 0;
			scroll.LargeChange = Math.Max(1, base.ClientSize.Height);
			scroll.SmallChange = 36;
			scroll.Maximum = Math.Max(0, Total() - 1);
			scroll.Value = Math.Min(scroll.Value, Math.Max(0, Total() - base.ClientSize.Height));
			Position();
			scroll.BringToFront();
		}
		finally
		{
			fitting = false;
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (scroll.Visible)
		{
			scroll.Value = Math.Max(0, Math.Min(Math.Max(0, scroll.Maximum - scroll.LargeChange + 1), scroll.Value - e.Delta / 120 * 72));
		}
		base.OnMouseWheel(e);
	}
}
