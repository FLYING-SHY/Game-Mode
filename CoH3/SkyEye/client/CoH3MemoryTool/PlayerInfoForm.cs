using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class PlayerInfoForm : Form
{
	private readonly DataGridView grid = new DataGridView();

	private readonly TextBox details = new TextBox();

	private readonly Label status = new Label();

	private readonly Timer timer = new Timer();

	private PlayerInfoReader reader;

	private List<PlayerInfoRow> current = new List<PlayerInfoRow>();

	private bool refreshing;

	public PlayerInfoForm(bool preview = false)
	{
		Text = "资源与战斗群 · 只读信息面板";
		base.ClientSize = new Size(1040, 610);
		MinimumSize = new Size(900, 500);
		base.StartPosition = FormStartPosition.CenterScreen;
		Font = new Font("Microsoft YaHei UI", 10f);
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 1,
			RowCount = 4
		};
		base.Controls.Add(tableLayoutPanel);
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 48f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 52f));
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			WrapContents = false
		};
		Button button = new Button
		{
			Text = "重新连接",
			AutoSize = true
		};
		EventHandler value = delegate
		{
			Connect();
		};
		button.Click += value;
		flowLayoutPanel.Controls.Add(button);
		CheckBox topmost = new CheckBox
		{
			Text = "窗口置顶",
			AutoSize = true,
			Padding = new Padding(8)
		};
		topmost.CheckedChanged += delegate
		{
			base.TopMost = topmost.Checked;
		};
		flowLayoutPanel.Controls.Add(topmost);
		status.AutoSize = true;
		status.Padding = new Padding(8);
		flowLayoutPanel.Controls.Add(status);
		tableLayoutPanel.Controls.Add(flowLayoutPanel, 0, 0);
		grid.Dock = DockStyle.Fill;
		grid.ReadOnly = true;
		grid.AllowUserToAddRows = false;
		grid.AllowUserToDeleteRows = false;
		grid.RowHeadersVisible = false;
		grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		grid.MultiSelect = false;
		grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
		string[] array = new string[7] { "关系", "玩家", "人力", "燃油", "弹药", "可用 CP", "战斗群" };
		foreach (string text in array)
		{
			int index = grid.Columns.Add(text, text);
			grid.Columns[index].SortMode = DataGridViewColumnSortMode.NotSortable;
		}
		grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		grid.ColumnHeadersHeight = 32;
		grid.RowTemplate.Height = 30;
		grid.Columns[1].FillWeight = 160f;
		grid.Columns[6].FillWeight = 180f;
		grid.SelectionChanged += delegate
		{
			if (!refreshing)
			{
				ShowDetails();
			}
		};
		tableLayoutPanel.Controls.Add(grid, 0, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Dock = DockStyle.Fill,
			Text = "点击玩家查看战斗群节点与其他已完成升级；未翻译项目显示游戏原名。",
			TextAlign = ContentAlignment.MiddleLeft
		}, 0, 2);
		details.Dock = DockStyle.Fill;
		details.Multiline = true;
		details.ReadOnly = true;
		details.ScrollBars = ScrollBars.Both;
		details.WordWrap = false;
		tableLayoutPanel.Controls.Add(details, 0, 3);
		timer.Interval = 1000;
		timer.Tick += delegate
		{
			RefreshData();
		};
		if (!preview)
		{
			base.Shown += delegate
			{
				Connect();
				timer.Start();
			};
		}
	}

	private void Connect()
	{
		timer.Stop();
		if (reader != null)
		{
			reader.Dispose();
		}
		reader = null;
		try
		{
			reader = new PlayerInfoReader();
			RefreshData();
		}
		catch (Exception ex)
		{
			Clear(ex.Message);
		}
		timer.Start();
	}

	private void Clear(string message)
	{
		refreshing = true;
		try
		{
			current.Clear();
			grid.Rows.Clear();
			details.Clear();
			status.Text = message;
		}
		finally
		{
			refreshing = false;
		}
	}

	private void RefreshData()
	{
		if (reader == null)
		{
			return;
		}
		try
		{
			List<PlayerInfoRow> list = reader.Snapshot();
			uint? num = ((grid.CurrentRow == null) ? ((uint?)null) : new uint?((uint)grid.CurrentRow.Tag));
			refreshing = true;
			current = list;
			grid.Rows.Clear();
			foreach (PlayerInfoRow item in current)
			{
				int index = grid.Rows.Add(item.Relation, item.Name, Math.Floor(item.Manpower), Math.Floor(item.Fuel), Math.Floor(item.Munition), Math.Floor(item.CP), item.Group);
				grid.Rows[index].Tag = item.Slot;
				if (item.Relation == "敌方")
				{
					grid.Rows[index].DefaultCellStyle.ForeColor = Color.Firebrick;
				}
				if (num.HasValue && num.Value == item.Slot)
				{
					grid.CurrentCell = grid.Rows[index].Cells[0];
				}
			}
			status.Text = "已读取 " + current.Count + " 名玩家 · " + DateTime.Now.ToString("HH:mm:ss") + " · 每秒刷新";
			refreshing = false;
			ShowDetails();
		}
		catch (Exception ex)
		{
			Clear(ex.Message);
		}
		finally
		{
			refreshing = false;
		}
	}

	private void ShowDetails()
	{
		if (grid.CurrentRow == null)
		{
			return;
		}
		uint slot = (uint)grid.CurrentRow.Tag;
		PlayerInfoRow playerInfoRow = current.FirstOrDefault((PlayerInfoRow x) => x.Slot == slot);
		if (playerInfoRow != null)
		{
			List<string> list = new List<string>();
			list.Add(playerInfoRow.Relation + " · " + playerInfoRow.Name + " · 队伍 " + playerInfoRow.Team);
			list.Add("战斗群：" + playerInfoRow.Group);
			list.Add("");
			list.Add("战斗群与已解锁节点：");
			List<string> list2 = list;
			string[] array = playerInfoRow.Upgrades.Where((string x) => x.Contains("\\battlegroups\\")).ToArray();
			list2.AddRange((array.Length == 0) ? new string[1] { "未读到相关记录（不据此断言玩家尚未选择）。" } : array.Select((string x) => "  " + x.Substring(x.LastIndexOf('\\') + 1).Replace('_', ' ')));
			list2.Add("");
			list2.Add("其他已完成升级：");
			list2.AddRange(from x in playerInfoRow.Upgrades.Except(array)
				select "  " + x.Substring(x.LastIndexOf('\\') + 1).Replace('_', ' '));
			string text = string.Join(Environment.NewLine, list2);
			if (details.Text != text)
			{
				details.Text = text;
			}
		}
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		timer.Stop();
		timer.Dispose();
		if (reader != null)
		{
			reader.Dispose();
		}
		base.OnFormClosed(e);
	}
}
