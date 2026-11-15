using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class ArmourPlayersForm : Form
{
	private readonly CheckedListBox list = new CheckedListBox();

	private readonly Func<PlayerInfoRow[]> getRows;

	private readonly EnemySelection selection;

	private readonly Timer refresh = new Timer();

	private bool updating;

	public ArmourPlayersForm(Func<PlayerInfoRow[]> source, EnemySelection enabled)
	{
		getRows = source;
		selection = enabled;
		Text = "装甲生产提示 · 玩家选择";
		base.ClientSize = new Size(580, 350);
		MinimumSize = new Size(580, 350);
		Font = new Font("Microsoft YaHei UI", 10f);
		base.StartPosition = FormStartPosition.CenterParent;
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(14),
			ColumnCount = 1,
			RowCount = 3
		};
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 78f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42f));
		base.Controls.Add(tableLayoutPanel);
		tableLayoutPanel.Controls.Add(new Label
		{
			Dock = DockStyle.Fill,
			Text = "勾选的敌人每次开始生产装甲都会提醒，不限第一辆。\n只控制装甲提示音，不影响信息显示或其他提示。\n本局有效；新对局默认全选。音源在“提示音设置”中选择。"
		}, 0, 0);
		list.Dock = DockStyle.Fill;
		list.CheckOnClick = true;
		list.IntegralHeight = false;
		tableLayoutPanel.Controls.Add(list, 0, 1);
		CheckedListBox checkedListBox = list;
		ItemCheckEventHandler value = delegate(object sender, ItemCheckEventArgs e)
		{
			if (!updating)
			{
				selection.Set(((EnemyChoice)list.Items[e.Index]).Player, e.NewValue == CheckState.Checked);
			}
		};
		checkedListBox.ItemCheck += value;
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 2);
		Button button = new Button
		{
			Text = "全部开启",
			AutoSize = true
		};
		Button button2 = new Button
		{
			Text = "全部关闭",
			AutoSize = true
		};
		Button button3 = new Button
		{
			Text = "完成",
			AutoSize = true
		};
		flowLayoutPanel.Controls.AddRange(new Control[3] { button, button2, button3 });
		button.Click += delegate
		{
			SetAll(value: true);
		};
		button2.Click += delegate
		{
			SetAll(value: false);
		};
		button3.Click += delegate
		{
			Close();
		};
		RefreshPlayers();
		refresh.Interval = 1000;
		refresh.Tick += delegate
		{
			RefreshPlayers();
		};
		refresh.Start();
	}

	private void SetAll(bool value)
	{
		PlayerInfoRow[] array = getRows();
		foreach (PlayerInfoRow p in array)
		{
			selection.Set(p, value);
		}
		RefreshPlayers();
	}

	private void RefreshPlayers()
	{
		PlayerInfoRow[] array = (from p in getRows()
			where p.Relation == "敌方"
			orderby p.Slot
			select p).ToArray();
		updating = true;
		try
		{
			bool flag = list.Items.Count == array.Length;
			int num = 0;
			while (flag && num < array.Length)
			{
				PlayerInfoRow player = ((EnemyChoice)list.Items[num]).Player;
				flag = player.Slot == array[num].Slot && player.Name == array[num].Name;
				num++;
			}
			if (!flag)
			{
				list.Items.Clear();
				PlayerInfoRow[] array2 = array;
				foreach (PlayerInfoRow playerInfoRow in array2)
				{
					list.Items.Add(new EnemyChoice
					{
						Player = playerInfoRow
					}, selection.Includes(playerInfoRow));
				}
			}
			else
			{
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					((EnemyChoice)list.Items[num3]).Player = array[num3];
					list.SetItemChecked(num3, selection.Includes(array[num3]));
				}
			}
		}
		finally
		{
			updating = false;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			refresh.Dispose();
		}
		base.Dispose(disposing);
	}
}
