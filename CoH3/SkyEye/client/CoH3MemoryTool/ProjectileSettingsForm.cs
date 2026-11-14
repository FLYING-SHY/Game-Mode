using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class ProjectileSettingsForm : Form
{
	public ProjectileSettingsForm(ProjectilePreferences p, Action changed)
	{
		Text = "逐发炮弹落点";
		base.ClientSize = new Size(510, 335);
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
		CheckBox enabled = new CheckBox
		{
			Text = "显示敌方每发炮弹的落点编号及单位简称",
			AutoSize = true,
			Checked = p.Enabled
		};
		flowLayoutPanel.Controls.Add(enabled);
		enabled.CheckedChanged += delegate
		{
			p.Enabled = enabled.Checked;
			changed();
		};
		CheckBox rings = new CheckBox
		{
			Text = "显示该发炮弹的爆炸范围圆环",
			AutoSize = true,
			Checked = p.Rings
		};
		flowLayoutPanel.Controls.Add(rings);
		rings.CheckedChanged += delegate
		{
			p.Rings = rings.Checked;
			changed();
		};
		decimal value = p.NumberSize;
		Action<decimal> changed2 = delegate(decimal n)
		{
			p.NumberSize = (int)n;
			changed();
		};
		Add(flowLayoutPanel, "落点编号数字大小", value, 8m, 72m, 0, changed2);
		Add(flowLayoutPanel, "单位简称文字大小", p.TextSize, 8m, 36m, 0, delegate(decimal n)
		{
			p.TextSize = (int)n;
			changed();
		});
		Add(flowLayoutPanel, "范围圆环粗细（像素）", p.RingWidth, 0.5m, 8m, 1, delegate(decimal n)
		{
			p.RingWidth = n;
			changed();
		});
		flowLayoutPanel.Controls.Add(new Label
		{
			Width = 475,
			Height = 105,
			Text = "每个单位每轮弹幕从1编号，炮弹抵达后清除。\n颜色跟随玩家，名称沿用“单位简称.tsv”。\n圆环按弹体实际读取的外半径绘制，不是必死圈。\n原有弹幕技能目标标记保留；本功能仅绘制在主画面。"
		});
	}

	private static void Add(Control panel, string label, decimal value, decimal min, decimal max, int digits, Action<decimal> changed)
	{
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
		flowLayoutPanel.Width = 475;
		flowLayoutPanel.Height = 35;
		FlowLayoutPanel flowLayoutPanel2 = flowLayoutPanel;
		panel.Controls.Add(flowLayoutPanel2);
		flowLayoutPanel2.Controls.Add(new Label
		{
			Text = label,
			Width = 310,
			Padding = new Padding(0, 5, 0, 0)
		});
		NumericUpDown input = new NumericUpDown
		{
			Minimum = min,
			Maximum = max,
			Value = value,
			DecimalPlaces = digits,
			Increment = ((digits == 0) ? 1m : 0.5m),
			Width = 90
		};
		flowLayoutPanel2.Controls.Add(input);
		input.ValueChanged += delegate
		{
			changed(input.Value);
		};
	}
}
