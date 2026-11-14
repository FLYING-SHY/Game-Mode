using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class MinimapSettingsForm : Form
{
	public MinimapSettingsForm(MinimapPreferences p, SquadMarkerPreferences shared, Action changed)
	{
		Text = "小地图敌军标记";
		base.ClientSize = new Size(650, 570);
		MinimumSize = new Size(650, 570);
		Font = new Font("Microsoft YaHei UI", 10f);
		base.StartPosition = FormStartPosition.CenterParent;
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			ColumnCount = 1,
			RowCount = 7
		};
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		base.Controls.Add(tableLayoutPanel);
		tableLayoutPanel.Controls.Add(new Label
		{
			Dock = DockStyle.Fill,
			Text = "左下角小地图：步兵班组用圆点，载具用三角形，颜色跟随游戏玩家。\n跟随当前小地图控件的完整变换，自动适配出生位置、旋转及界面缩放。\n小地图刷新率单独设置，默认每秒 10 次，不跟随主画面的刷新率。"
		}, 0, 0);
		CheckBox enabled = new CheckBox
		{
			Text = "启用小地图敌军标记",
			AutoSize = true,
			Checked = p.Enabled
		};
		tableLayoutPanel.Controls.Add(enabled, 0, 1);
		TableLayoutPanel tableLayoutPanel2 = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 3,
			RowCount = 2
		};
		for (int i = 0; i < 3; i++)
		{
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
		}
		tableLayoutPanel.Controls.Add(tableLayoutPanel2, 0, 2);
		CheckBox movement = new CheckBox
		{
			Text = "显示载具到终点的实线与叉号（不显示名称）",
			AutoSize = true,
			Checked = p.Movement
		};
		tableLayoutPanel.Controls.Add(movement, 0, 3);
		TableLayoutPanel tableLayoutPanel3 = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 2
		};
		for (int j = 0; j < 2; j++)
		{
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
		}
		tableLayoutPanel.Controls.Add(tableLayoutPanel3, 0, 4);
		tableLayoutPanel3.Controls.Add(new Label
		{
			Text = "小地图连线与叉号粗细（像素）",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, 0);
		NumericUpDown moveWidth = new NumericUpDown
		{
			Minimum = 0.5m,
			Maximum = 8m,
			DecimalPlaces = 1,
			Increment = 0.5m,
			Value = p.MoveWidth,
			Width = 100
		};
		tableLayoutPanel3.Controls.Add(moveWidth, 0, 1);
		tableLayoutPanel3.Controls.Add(new Label
		{
			Text = "小地图终点叉号大小（像素）",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 1, 0);
		NumericUpDown moveCross = new NumericUpDown
		{
			Minimum = 2m,
			Maximum = 40m,
			Value = p.MoveCrossSize,
			Width = 100
		};
		tableLayoutPanel3.Controls.Add(moveCross, 1, 1);
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			WrapContents = false
		};
		tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 5);
		CheckBox barrageOn = new CheckBox
		{
			Text = "显示敌方弹幕落点＊",
			AutoSize = true,
			Checked = p.Barrages,
			Padding = new Padding(0, 4, 0, 0)
		};
		flowLayoutPanel.Controls.Add(barrageOn);
		flowLayoutPanel.Controls.Add(new Label
		{
			Text = "星号大小（像素）",
			AutoSize = true,
			Padding = new Padding(12, 7, 0, 0)
		});
		NumericUpDown barrageSize = new NumericUpDown
		{
			Minimum = 4m,
			Maximum = 60m,
			Value = p.BarrageSize,
			Width = 80
		};
		flowLayoutPanel.Controls.Add(barrageSize);
		Panel preview = new Panel
		{
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(38, 45, 43)
		};
		tableLayoutPanel.Controls.Add(preview, 0, 6);
		enabled.CheckedChanged += delegate
		{
			p.Enabled = enabled.Checked;
			changed();
			preview.Invalidate();
		};
		movement.CheckedChanged += delegate
		{
			p.Movement = movement.Checked;
			changed();
			preview.Invalidate();
		};
		moveWidth.ValueChanged += delegate
		{
			p.MoveWidth = moveWidth.Value;
			changed();
			preview.Invalidate();
		};
		moveCross.ValueChanged += delegate
		{
			p.MoveCrossSize = (int)moveCross.Value;
			changed();
			preview.Invalidate();
		};
		barrageOn.CheckedChanged += delegate
		{
			p.Barrages = barrageOn.Checked;
			changed();
			preview.Invalidate();
		};
		barrageSize.ValueChanged += delegate
		{
			p.BarrageSize = (int)barrageSize.Value;
			changed();
			preview.Invalidate();
		};
		bool[] array = new bool[2] { false, true };
		foreach (bool flag in array)
		{
			bool v = flag;
			int column = (v ? 1 : 0);
			tableLayoutPanel2.Controls.Add(new Label
			{
				Text = (v ? "载具三角形大小（像素）" : "步兵圆点直径（像素）"),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			}, column, 0);
			NumericUpDown size = new NumericUpDown
			{
				Minimum = (v ? 3 : 2),
				Maximum = (v ? 60 : 40),
				Value = (v ? p.VehicleSize : p.InfantrySize),
				Width = 100
			};
			tableLayoutPanel2.Controls.Add(size, column, 1);
			size.ValueChanged += delegate
			{
				if (v)
				{
					p.VehicleSize = (int)size.Value;
				}
				else
				{
					p.InfantrySize = (int)size.Value;
				}
				changed();
				preview.Invalidate();
			};
		}
		tableLayoutPanel2.Controls.Add(new Label
		{
			Text = "小地图刷新（次/秒）",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		}, 2, 0);
		NumericUpDown refresh = new NumericUpDown
		{
			Minimum = 1m,
			Maximum = 60m,
			Value = p.RefreshRate,
			Width = 100
		};
		tableLayoutPanel2.Controls.Add(refresh, 2, 1);
		refresh.ValueChanged += delegate
		{
			p.RefreshRate = (int)refresh.Value;
			changed();
		};
		preview.Paint += delegate(object sender, PaintEventArgs e)
		{
			MinimapFrame frame = new MinimapFrame
			{
				CentreX = preview.Width / 2,
				CentreY = preview.Height / 2,
				PixelWidth = preview.Width - 20,
				PixelHeight = preview.Height - 20,
				RootWidth = preview.Width,
				RootHeight = preview.Height,
				WorldWidth = 200f,
				WorldHeight = 200f
			};
			SquadMarker[] markers = new SquadMarker[2]
			{
				new SquadMarker
				{
					X = -50f,
					Z = 0f,
					Colour = Color.FromArgb(255, 59, 48)
				},
				new SquadMarker
				{
					X = 50f,
					Z = -45f,
					Vehicle = true,
					Colour = Color.FromArgb(160, 100, 255),
					Movement = new MovementData
					{
						X = -10f,
						Z = 45f
					}
				}
			};
			BarrageRecord[] barrages = new BarrageRecord[1]
			{
				new BarrageRecord
				{
					X = -55f,
					Z = -45f,
					Colour = Color.Orange
				}
			};
			MinimapMarkerDrawing.Draw(e.Graphics, markers, frame, new MinimapPreferences
			{
				InfantrySize = p.InfantrySize,
				VehicleSize = p.VehicleSize,
				Movement = p.Movement,
				MoveWidth = p.MoveWidth,
				MoveCrossSize = p.MoveCrossSize,
				Barrages = p.Barrages,
				BarrageSize = p.BarrageSize
			}, preview.Width, preview.Height, barrages);
		};
	}
}
