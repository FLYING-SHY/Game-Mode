using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class SquadMarkerSettingsForm : Form
{
	public SquadMarkerSettingsForm(SquadMarkerPreferences p, Action changed, Func<string> statistics = null)
	{
		Text = "敌军班组标记";
		base.ClientSize = new Size(720, 565);
		MinimumSize = new Size(720, 565);
		Font = new Font("Microsoft YaHei UI", 10f);
		base.StartPosition = FormStartPosition.CenterParent;
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			ColumnCount = 1,
			RowCount = 6
		};
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		base.Controls.Add(tableLayoutPanel);
		tableLayoutPanel.Controls.Add(new Label
		{
			Dock = DockStyle.Fill,
			Text = "每个敌方班组显示一个圆环，载具显示空心方框；图形中央显示单位简称。\n图形和文字始终跟随该玩家在游戏里的颜色。\n大小单位为屏幕像素，文字为字号。刷新率为目标值。"
		}, 0, 0);
		CheckBox enabled = new CheckBox
		{
			Text = "启用敌军班组标记",
			Checked = p.Enabled,
			AutoSize = true
		};
		tableLayoutPanel.Controls.Add(enabled, 0, 1);
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 4,
			RowCount = 2
		};
		int[] array = new int[4] { 120, 150, 150, 170 };
		foreach (int num in array)
		{
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, num));
		}
		tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 2);
		string[] array2 = new string[4] { "步兵圆环直径", "载具方框边长", "描边粗细", "名称字号" };
		decimal[] array3 = new decimal[4] { p.InfantrySize, p.VehicleSize, p.Stroke, p.TextSize };
		Panel preview = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(38, 45, 43)
		};
		tableLayoutPanel.Controls.Add(preview, 0, 5);
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 3);
		flowLayoutPanel.Controls.Add(new Label
		{
			Text = "镜头跟随（帧/秒）",
			AutoSize = true,
			Padding = new Padding(0, 5, 0, 0)
		});
		NumericUpDown frames = new NumericUpDown
		{
			Minimum = 10m,
			Maximum = 144m,
			Value = p.FrameRate,
			Width = 62
		};
		flowLayoutPanel.Controls.Add(frames);
		frames.ValueChanged += delegate
		{
			p.FrameRate = (int)frames.Value;
			changed();
		};
		flowLayoutPanel.Controls.Add(new Label
		{
			Text = "位置刷新（次/秒）",
			AutoSize = true,
			Padding = new Padding(10, 5, 0, 0)
		});
		NumericUpDown positions = new NumericUpDown
		{
			Minimum = 5m,
			Maximum = 144m,
			Value = p.PositionRate,
			Width = 62
		};
		flowLayoutPanel.Controls.Add(positions);
		positions.ValueChanged += delegate
		{
			p.PositionRate = (int)positions.Value;
			changed();
		};
		FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false
		};
		tableLayoutPanel.Controls.Add(flowLayoutPanel2, 0, 4);
		CheckBox showStats = new CheckBox
		{
			Text = "在游戏右侧显示刷新统计",
			Checked = p.ShowMetrics,
			AutoSize = true
		};
		flowLayoutPanel2.Controls.Add(showStats);
		showStats.CheckedChanged += delegate
		{
			p.ShowMetrics = showStats.Checked;
			changed();
		};
		flowLayoutPanel2.Controls.Add(new Label
		{
			AutoSize = true,
			Text = "建议先设镜头 144、位置 30；小地图保持自己的低刷新率。"
		});
		flowLayoutPanel2.Controls.Add(new Label
		{
			AutoSize = true,
			MaximumSize = new Size(610, 0),
			Text = ((statistics == null) ? "返回游戏后可查看实测绘制帧率、帧间隔和采样到绘制耗时。" : statistics())
		});
		enabled.CheckedChanged += delegate
		{
			p.Enabled = enabled.Checked;
			changed();
			preview.Invalidate();
		};
		for (int num2 = 0; num2 < 4; num2++)
		{
			int field = num2;
			tableLayoutPanel2.Controls.Add(new Label
			{
				Text = array2[num2],
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, num2, 0);
			NumericUpDown value = new NumericUpDown
			{
				Minimum = ((num2 == 2) ? 0.5m : 8m),
				Maximum = num2 switch
				{
					3 => 36, 
					2 => 12, 
					_ => 200, 
				},
				DecimalPlaces = ((num2 == 2) ? 1 : 0),
				Increment = ((num2 == 2) ? 0.5m : 1m),
				Value = array3[num2],
				Width = 95
			};
			tableLayoutPanel2.Controls.Add(value, num2, 1);
			value.ValueChanged += delegate
			{
				if (field == 0)
				{
					p.InfantrySize = (int)value.Value;
				}
				else if (field == 1)
				{
					p.VehicleSize = (int)value.Value;
				}
				else if (field == 2)
				{
					p.Stroke = value.Value;
				}
				else
				{
					p.TextSize = (int)value.Value;
				}
				changed();
				preview.Invalidate();
			};
		}
		preview.Paint += delegate(object sender, PaintEventArgs e)
		{
			using Font font = new Font("Microsoft YaHei UI", p.TextSize, FontStyle.Bold);
			SquadMarkerDrawing.Draw(e.Graphics, new PointF((float)preview.Width * 0.28f, (float)preview.Height * 0.62f), vehicle: false, UnitNames.Name("sbps\\races\\american\\infantry\\riflemen_us"), Color.FromArgb(235, 65, 65), p, font);
			SquadMarkerDrawing.Draw(e.Graphics, new PointF((float)preview.Width * 0.73f, (float)preview.Height * 0.62f), vehicle: true, UnitNames.Name("sbps\\races\\american\\vehicles\\sherman_us"), Color.FromArgb(160, 100, 255), p, font);
		};
	}
}
