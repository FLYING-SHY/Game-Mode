using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class SoundSettingsForm : Form
{
	public SoundSettingsForm(AlertSounds sounds)
	{
		SoundSettingsForm owner = this;
		Text = "自定义提示音";
		base.ClientSize = new Size(790, 480);
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
			Text = "启用声音提示",
			Checked = sounds.Enabled,
			AutoSize = true
		};
		flowLayoutPanel.Controls.Add(enabled);
		enabled.CheckedChanged += delegate
		{
			sounds.Enabled = enabled.Checked;
			sounds.Save();
		};
		flowLayoutPanel.Controls.Add(new Label
		{
			Text = "装甲提示使用独立的玩家勾选；其他敌方提示跟随显示勾选。支持 MP3 / WAV。",
			AutoSize = true,
			Margin = new Padding(0, 8, 0, 8)
		});
		foreach (AlertKind value in Enum.GetValues(typeof(AlertKind)))
		{
			AlertKind k = value;
			FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
			{
				Width = 710,
				Height = 52,
				WrapContents = false
			};
			flowLayoutPanel.Controls.Add(flowLayoutPanel2);
			flowLayoutPanel2.Controls.Add(new Label
			{
				Text = ((k == AlertKind.EnemyArtilleryBarrage) ? "敌方火炮弹幕" : ((k == AlertKind.BattleGroup) ? "选择战斗群" : ((k == AlertKind.BaseComplete) ? "基地建成" : ((k == AlertKind.EnemyArmourProduction) ? "敌方装甲开造" : ((k == AlertKind.GrenadeReady) ? "敌方手雷升级" : "己方弹幕就绪"))))),
				Width = 112,
				Height = 38,
				Padding = new Padding(0, 8, 0, 0)
			});
			Button button = new Button
			{
				Text = "选择文件",
				AutoSize = true
			};
			Button button2 = new Button
			{
				Text = "试听",
				AutoSize = true
			};
			Button button3 = new Button
			{
				Text = "恢复默认",
				AutoSize = true
			};
			Label name = new Label
			{
				Text = sounds.DisplayFile(k),
				AutoSize = true,
				Padding = new Padding(0, 8, 0, 0)
			};
			flowLayoutPanel2.Controls.Add(button);
			flowLayoutPanel2.Controls.Add(button2);
			flowLayoutPanel2.Controls.Add(button3);
			flowLayoutPanel2.Controls.Add(name);
			button.Click += delegate
			{
				using OpenFileDialog openFileDialog = new OpenFileDialog
				{
					Filter = "音频文件（MP3、WAV）|*.mp3;*.wav|MP3 音频|*.mp3|WAV 音频|*.wav",
					Title = "选择提示音"
				};
				if (openFileDialog.ShowDialog(owner) == DialogResult.OK)
				{
					try
					{
						sounds.Set(k, openFileDialog.FileName);
						name.Text = Path.GetFileName(openFileDialog.FileName);
						return;
					}
					catch (Exception ex)
					{
						MessageBox.Show(owner, "音源无法加载：\n" + ex.Message, "提示音设置");
						return;
					}
				}
			};
			button2.Click += delegate
			{
				sounds.Play(k, preview: true);
			};
			button3.Click += delegate
			{
				try
				{
					sounds.Set(k, null);
					name.Text = sounds.DisplayFile(k);
				}
				catch (Exception)
				{
					MessageBox.Show(owner, "提示音设置未能保存。", "提示音设置");
				}
			};
		}
	}
}
