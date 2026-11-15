using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class MovementSettingsForm : Form
{
	public MovementSettingsForm(SquadMarkerPreferences p, Action changed)
	{
		Text = "移动目标与连线";
		base.ClientSize = new Size(510, 350);
		Font = new Font("Microsoft YaHei UI", 10f);
		base.StartPosition = FormStartPosition.CenterParent;
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			AutoScroll = true
		};
		base.Controls.Add(flowLayoutPanel);
		CheckBox on = new CheckBox
		{
			Text = "显示敌方载具移动终点及直线连线",
			Checked = p.Movement,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(on);
		on.CheckedChanged += delegate
		{
			p.Movement = on.Checked;
			changed();
		};
		decimal value = p.MoveCrossSize;
		Action<decimal> changed2 = delegate(decimal n)
		{
			p.MoveCrossSize = (int)n;
			changed();
		};
		Add(flowLayoutPanel, "叉号大小（像素）", value, 8m, 160m, 0, changed2);
		Add(flowLayoutPanel, "叉号与连线粗细（像素）", p.MoveWidth, 0.5m, 12m, 1, delegate(decimal n)
		{
			p.MoveWidth = n;
			changed();
		});
		Add(flowLayoutPanel, "虚线每段长度（像素）", p.MoveDashLength, 1m, 100m, 1, delegate(decimal n)
		{
			p.MoveDashLength = n;
			changed();
		});
		Add(flowLayoutPanel, "虚线间隔（越大越稀疏）", p.MoveDashGap, 1m, 100m, 1, delegate(decimal n)
		{
			p.MoveDashGap = n;
			changed();
		});
		flowLayoutPanel.Controls.Add(new Label
		{
			Width = 470,
			Height = 105,
			Text = "直线只表示单位与终点的连接，不代表实际绕路路线。\n同一玩家相邻终点合并名称；同名计数、异名换行。\n颜色跟随玩家，字号沿用班组标记设置。\n停止或到达目标后清除；需要开启“敌军班组标记”。"
		});
	}

	private static void Add(Control panel, string text, decimal value, decimal min, decimal max, int digits, Action<decimal> changed)
	{
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
		flowLayoutPanel.Width = 470;
		flowLayoutPanel.Height = 35;
		FlowLayoutPanel flowLayoutPanel2 = flowLayoutPanel;
		panel.Controls.Add(flowLayoutPanel2);
		flowLayoutPanel2.Controls.Add(new Label
		{
			Text = text,
			Width = 280,
			Padding = new Padding(0, 5, 0, 0)
		});
		NumericUpDown number = new NumericUpDown
		{
			Minimum = min,
			Maximum = max,
			Value = value,
			DecimalPlaces = digits,
			Increment = ((digits == 0) ? 1m : 0.5m),
			Width = 90
		};
		flowLayoutPanel2.Controls.Add(number);
		number.ValueChanged += delegate
		{
			changed(number.Value);
		};
	}
}
