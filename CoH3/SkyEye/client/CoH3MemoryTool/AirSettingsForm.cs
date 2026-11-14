using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class AirSettingsForm : Form
{
	public AirSettingsForm(AirPreferences p, Action changed)
	{
		Text = "飞机条形技能与 V1";
		base.ClientSize = new Size(555, 475);
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
		bool enabled = p.Enabled;
		Action<bool> changed2 = delegate(bool v)
		{
			p.Enabled = v;
			changed();
		};
		Toggle(flowLayoutPanel, "显示敌方条形扫射、投弹区域及技能名称", enabled, changed2);
		Toggle(flowLayoutPanel, "主画面显示 V1 落点与爆炸范围圆环", p.V1, delegate(bool v)
		{
			p.V1 = v;
			changed();
		});
		Toggle(flowLayoutPanel, "小地图显示 V1 半透明范围和中央文字", p.MiniV1, delegate(bool v)
		{
			p.MiniV1 = v;
			changed();
		});
		Add(flowLayoutPanel, "飞机矩形不透明度（0%全透明）", p.Opacity, 0m, 100m, 0, delegate(decimal n)
		{
			p.Opacity = (int)n;
			changed();
		});
		Add(flowLayoutPanel, "小地图 V1 圆形不透明度", p.MiniOpacity, 0m, 100m, 0, delegate(decimal n)
		{
			p.MiniOpacity = (int)n;
			changed();
		});
		Add(flowLayoutPanel, "主画面技能名称字号", p.TextSize, 8m, 40m, 0, delegate(decimal n)
		{
			p.TextSize = (int)n;
			changed();
		});
		Add(flowLayoutPanel, "小地图 V1 字号", p.MiniTextSize, 6m, 24m, 0, delegate(decimal n)
		{
			p.MiniTextSize = (int)n;
			changed();
		});
		Add(flowLayoutPanel, "主画面边线／圆环粗细", p.Width, 0.5m, 8m, 1, delegate(decimal n)
		{
			p.Width = n;
			changed();
		});
		flowLayoutPanel.Controls.Add(new Label
		{
			Width = 515,
			Height = 125,
			Text = "全部标识颜色跟随施放玩家。圆形巡航技能不重复绘制。\n矩形按本版本技能的瞄准区域和方向绘制，不是必死区；\n爆炸散布、伤害衰减和遮挡会影响实际伤害。\nV1 读取已生成火箭的实际目标与爆炸外半径，落地即清除。\n请在施放技能前开启；中途开启可能漏掉已结束的技能指令。"
		});
	}

	private static void Toggle(Control panel, string text, bool value, Action<bool> changed)
	{
		CheckBox c = new CheckBox
		{
			Text = text,
			Checked = value,
			AutoSize = true
		};
		panel.Controls.Add(c);
		c.CheckedChanged += delegate
		{
			changed(c.Checked);
		};
	}

	private static void Add(Control panel, string text, decimal value, decimal min, decimal max, int digits, Action<decimal> changed)
	{
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
		flowLayoutPanel.Width = 515;
		flowLayoutPanel.Height = 35;
		FlowLayoutPanel flowLayoutPanel2 = flowLayoutPanel;
		panel.Controls.Add(flowLayoutPanel2);
		flowLayoutPanel2.Controls.Add(new Label
		{
			Text = text,
			Width = 355,
			Padding = new Padding(0, 5, 0, 0)
		});
		NumericUpDown n = new NumericUpDown
		{
			Minimum = min,
			Maximum = max,
			Value = value,
			DecimalPlaces = digits,
			Increment = ((digits == 0) ? 1m : 0.5m),
			Width = 100
		};
		flowLayoutPanel2.Controls.Add(n);
		n.ValueChanged += delegate
		{
			changed(n.Value);
		};
	}
}
