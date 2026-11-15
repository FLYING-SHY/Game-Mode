using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class ResourceColourForm : Form
{
	private readonly Timer animation = new Timer();

	public ResourceColourForm(OverlayPreferences preferences, Action changed)
	{
		ResourceColourForm resourceColourForm = this;
		Text = "资源数值颜色与闪烁 · 所有玩家统一";
		base.ClientSize = new Size(1020, 460);
		MinimumSize = new Size(1020, 460);
		Font = new Font("Microsoft YaHei UI", 10f);
		base.StartPosition = FormStartPosition.CenterParent;
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			ColumnCount = 1,
			RowCount = 3
		};
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 86f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 235f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		base.Controls.Add(tableLayoutPanel);
		tableLayoutPanel.Controls.Add(new Label
		{
			Dock = DockStyle.Fill,
			Text = "只调整数值文字；四类资源分别设置，所有玩家共用。\n数值从 0 到“渐变终点”逐渐变色；严格超过“闪烁阈值”后，在目标色和闪烁色间切换。\n频率表示每秒完成几次“目标色 → 闪烁色 → 目标色”；取消启用即可恢复原配色。"
		}, 0, 0);
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 8,
			RowCount = 5
		};
		tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 1);
		int[] array = new int[8] { 55, 95, 115, 125, 115, 125, 115, 130 };
		foreach (int num in array)
		{
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num));
		}
		string[] array2 = new string[8] { "启用", "数值类型", "零值颜色", "渐变终点", "目标颜色", "闪烁阈值", "闪烁颜色", "频率（次/秒）" };
		foreach (string text in array2)
		{
			tableLayoutPanel2.Controls.Add(new Label
			{
				Text = text,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, tableLayoutPanel2.Controls.Count, 0);
		}
		tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 43f));
		int num2 = 1;
		foreach (KeyValuePair<string, ResourceColourRule> item in preferences.Resources.Rules())
		{
			ResourceColourRule r = item.Value;
			int row = num2++;
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
			CheckBox enabled = new CheckBox
			{
				Checked = r.Enabled,
				Dock = DockStyle.Fill
			};
			tableLayoutPanel2.Controls.Add(enabled, 0, row);
			enabled.CheckedChanged += delegate
			{
				r.Enabled = enabled.Checked;
				changed();
			};
			string text2 = ((item.Key == "manpower") ? "人力" : ((item.Key == "munition") ? "弹药" : ((item.Key == "fuel") ? "油料" : "人口")));
			tableLayoutPanel2.Controls.Add(new Label
			{
				Text = text2,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, 1, row);
			AddColour(tableLayoutPanel2, 2, row, r.Zero, delegate(Color c)
			{
				r.Zero = c;
				changed();
			});
			AddNumber(tableLayoutPanel2, 3, row, 1m, 1000000m, r.TargetValue, 0, delegate(decimal v)
			{
				r.TargetValue = v;
				changed();
			});
			AddColour(tableLayoutPanel2, 4, row, r.Target, delegate(Color c)
			{
				r.Target = c;
				changed();
			});
			AddNumber(tableLayoutPanel2, 5, row, 0m, 1000000m, r.FlashAbove, 0, delegate(decimal v)
			{
				r.FlashAbove = v;
				changed();
			});
			AddColour(tableLayoutPanel2, 6, row, r.Flash, delegate(Color c)
			{
				r.Flash = c;
				changed();
			});
			AddNumber(tableLayoutPanel2, 7, row, 0.1m, 5m, r.Frequency, 1, delegate(decimal v)
			{
				r.Frequency = v;
				changed();
			});
		}
		Panel preview = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(38, 45, 43)
		};
		tableLayoutPanel.Controls.Add(preview, 0, 2);
		preview.Paint += delegate(object sender, PaintEventArgs e)
		{
			string text3 = "预览：";
			List<NumberSpan> list = new List<NumberSpan>();
			foreach (KeyValuePair<string, ResourceColourRule> item2 in preferences.Resources.Rules())
			{
				ResourceColourRule value = item2.Value;
				string text4 = ((item2.Key == "manpower") ? "人" : ((item2.Key == "munition") ? "弹" : ((item2.Key == "fuel") ? "油" : "人口")));
				text3 = text3 + text4 + " ";
				string text5 = (value.FlashAbove + 1m).ToString(CultureInfo.InvariantCulture);
				list.Add(new NumberSpan
				{
					Start = text3.Length,
					Length = text5.Length,
					Value = (double)value.FlashAbove + 1.0,
					Rule = value
				});
				text3 = text3 + text5 + "   ";
			}
			ResourceLineDrawing.Draw(e.Graphics, text3, resourceColourForm.Font, new Rectangle(8, 8, Math.Max(20, preview.Width - 16), preview.Height - 16), Color.White, list.ToArray(), paint: true, ResourceClock.Seconds);
		};
		animation.Interval = 50;
		animation.Tick += delegate
		{
			preview.Invalidate();
		};
		animation.Start();
	}

	private void AddColour(TableLayoutPanel grid, int col, int row, Color initial, Action<Color> update)
	{
		Button button = new Button
		{
			Text = "选颜色",
			BackColor = initial,
			Dock = DockStyle.Fill
		};
		grid.Controls.Add(button, col, row);
		button.Click += delegate
		{
			using ColorDialog colorDialog = new ColorDialog
			{
				Color = button.BackColor,
				FullOpen = true
			};
			if (colorDialog.ShowDialog(this) == DialogResult.OK)
			{
				button.BackColor = colorDialog.Color;
				update(colorDialog.Color);
			}
		};
	}

	private static void AddNumber(TableLayoutPanel grid, int col, int row, decimal min, decimal max, decimal value, int decimals, Action<decimal> update)
	{
		NumericUpDown box = new NumericUpDown
		{
			Minimum = min,
			Maximum = max,
			Value = value,
			DecimalPlaces = decimals,
			Increment = ((decimals == 0) ? 1m : 0.1m),
			Dock = DockStyle.Fill,
			ThousandsSeparator = true
		};
		grid.Controls.Add(box, col, row);
		box.ValueChanged += delegate
		{
			update(box.Value);
		};
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			animation.Dispose();
		}
		base.Dispose(disposing);
	}
}
