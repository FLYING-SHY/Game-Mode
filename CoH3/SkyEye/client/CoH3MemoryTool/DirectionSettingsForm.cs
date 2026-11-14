using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class DirectionSettingsForm : Form
{
	public DirectionSettingsForm(SquadMarkerPreferences p, Action changed)
	{
		Text = "朝向与射界";
		base.ClientSize = new Size(620, 550);
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
			Text = "显示敌方载具和小队武器方向线",
			Checked = p.Directions,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(on);
		on.CheckedChanged += delegate
		{
			p.Directions = on.Checked;
			changed();
		};
		CheckBox auto = new CheckBox
		{
			Text = "小队武器默认使用实际射程（火炮除外）",
			Checked = p.AutoTeamRange,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(auto);
		auto.CheckedChanged += delegate
		{
			p.AutoTeamRange = auto.Checked;
			changed();
		};
		CheckBox vehicleAuto = new CheckBox
		{
			Text = "载具默认使用当前主武器射程（自行火炮除外）",
			Checked = p.AutoVehicleRange,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(vehicleAuto);
		vehicleAuto.CheckedChanged += delegate
		{
			p.AutoVehicleRange = vehicleAuto.Checked;
			changed();
		};
		decimal value = p.VehicleLength;
		Action<decimal> changed2 = delegate(decimal n)
		{
			p.VehicleLength = (int)n;
			changed();
		};
		Add(flowLayoutPanel, "载具自定义长度（关闭实际射程后生效）", value, 1m, 200m, changed2);
		Add(flowLayoutPanel, "小队武器自定义长度（关闭实际射程后生效）", p.TeamLength, 1m, 200m, delegate(decimal n)
		{
			p.TeamLength = (int)n;
			changed();
		});
		Add(flowLayoutPanel, "火炮及自行火炮方向线长度（游戏距离单位）", p.ArtilleryLength, 1m, 200m, delegate(decimal n)
		{
			p.ArtilleryLength = (int)n;
			changed();
		});
		Add(flowLayoutPanel, "线条粗度（像素）", p.DirectionWidth, 0.5m, 12m, delegate(decimal n)
		{
			p.DirectionWidth = n;
			changed();
		});
		CheckBox autoSector = new CheckBox
		{
			Text = "自动绘制机枪射界（读取当前武器角度及射程）",
			Checked = p.AutoSectors,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(autoSector);
		autoSector.CheckedChanged += delegate
		{
			p.AutoSectors = autoSector.Checked;
			changed();
		};
		CheckBox sector = new CheckBox
		{
			Text = "关闭自动射界或读取失败时，使用手动示意扇形",
			Checked = p.Sectors,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(sector);
		sector.CheckedChanged += delegate
		{
			p.Sectors = sector.Checked;
			changed();
		};
		Add(flowLayoutPanel, "备用示意扇形总角度（0 为不绘制）", p.SectorAngle, 0m, 180m, delegate(decimal n)
		{
			p.SectorAngle = (int)n;
			changed();
		});
		string text = "需要架设的机枪只在架设完成后绘制；颜色与所属玩家一致。";
		flowLayoutPanel.Controls.Add(new Label
		{
			Width = 580,
			Height = 100,
			Text = text + "\n自动射界按当前武器分别读取，不会把所有机枪套用同一角度。\n扇形表示武器的水平覆盖范围，未扣除建筑、地形和视线遮挡。\n载具方向线表示车身朝向；火炮长度独立设置。"
		});
	}

	private static void Add(Control parent, string text, decimal value, decimal min, decimal max, Action<decimal> changed)
	{
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
		flowLayoutPanel.Width = 585;
		flowLayoutPanel.Height = 32;
		flowLayoutPanel.WrapContents = false;
		FlowLayoutPanel flowLayoutPanel2 = flowLayoutPanel;
		parent.Controls.Add(flowLayoutPanel2);
		flowLayoutPanel2.Controls.Add(new Label
		{
			Text = text,
			Width = 450,
			Padding = new Padding(0, 5, 0, 0)
		});
		NumericUpDown n = new NumericUpDown
		{
			Minimum = min,
			Maximum = max,
			Value = value,
			DecimalPlaces = ((min < 1m) ? 1 : 0),
			Increment = ((min < 1m) ? 0.5m : 1m),
			Width = 90
		};
		flowLayoutPanel2.Controls.Add(n);
		n.ValueChanged += delegate
		{
			changed(n.Value);
		};
	}
}
