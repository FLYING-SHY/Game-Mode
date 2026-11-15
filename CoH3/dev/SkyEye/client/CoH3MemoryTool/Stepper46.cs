using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Stepper46 : Panel
{
	private readonly TextBox text = new TextBox();

	private readonly decimal low;

	private readonly decimal high;

	private readonly decimal step;

	private readonly int digits;

	private decimal value;

	private bool updating53;

	private readonly Timer editTimer53 = new Timer
	{
		Interval = 450
	};

	public Action<decimal> Changed;

	public decimal Value
	{
		get
		{
			return value;
		}
		set
		{
			decimal num = decimal.Round(Math.Max(low, Math.Min(high, value)), digits);
			bool flag = this.value != num;
			this.value = num;
			updating53 = true;
			try
			{
				text.Text = num.ToString((digits > 0) ? "0.0" : "0");
			}
			finally
			{
				updating53 = false;
			}
			if (flag && Changed != null)
			{
				Changed(num);
			}
		}
	}

	public Stepper46(decimal min, decimal max, decimal initial, int places)
	{
		low = min;
		high = max;
		digits = places;
		step = ((places > 0) ? 0.1m : 1m);
		base.Size = new Size(74, 26);
		DoubleBuffered = true;
		BackColor = Palette46.Back;
		text.BorderStyle = BorderStyle.None;
		text.BackColor = Palette46.Back;
		text.ForeColor = Palette46.Ink;
		text.Font = new Font("Consolas", 13f, FontStyle.Regular, GraphicsUnit.Pixel);
		text.Location = new Point(6, 5);
		text.Width = 47;
		text.TextAlign = HorizontalAlignment.Left;
		base.Controls.Add(text);
		Action46 action = new Action46
		{
			Text = "+",
			Location = new Point(55, 0),
			Size = new Size(18, 13),
			Font = Interface45.Font(7f)
		};
		Action46 action2 = new Action46
		{
			Text = "−",
			Location = new Point(55, 13),
			Size = new Size(18, 13),
			Font = Interface45.Font(7f)
		};
		base.Controls.Add(action);
		base.Controls.Add(action2);
		action.Click += delegate
		{
			Value = value + step;
		};
		action2.Click += delegate
		{
			Value = value - step;
		};
		text.Validated += delegate
		{
			Commit();
		};
		text.KeyDown += delegate(object s, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				Commit();
				e.SuppressKeyPress = true;
			}
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
			{
				Commit();
				Value = value + ((e.KeyCode == Keys.Up) ? step : (-step));
				e.SuppressKeyPress = true;
			}
		};
		editTimer53.Tick += delegate
		{
			editTimer53.Stop();
			ApplyPending53();
		};
		text.TextChanged += delegate
		{
			if (!updating53)
			{
				editTimer53.Stop();
				editTimer53.Start();
			}
		};
		Value = initial;
	}

	private void ApplyPending53()
	{
		if (!decimal.TryParse(text.Text, out var result) || result < low || result > high)
		{
			return;
		}
		result = decimal.Round(result, digits);
		if (!(result == value))
		{
			value = result;
			if (Changed != null)
			{
				Changed(result);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			editTimer53.Stop();
			editTimer53.Dispose();
		}
		base.Dispose(disposing);
	}

	private void Commit()
	{
		editTimer53.Stop();
		if (decimal.TryParse(text.Text, out var result))
		{
			Value = result;
		}
		else
		{
			text.Text = value.ToString((digits > 0) ? "0.0" : "0");
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using Pen pen = new Pen(Palette46.Line);
		e.Graphics.DrawRectangle(pen, 0, 0, base.Width - 1, base.Height - 1);
	}
}
