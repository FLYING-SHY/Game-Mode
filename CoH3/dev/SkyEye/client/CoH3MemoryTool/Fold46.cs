using System;
using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class Fold46 : FlowLayoutPanel
{
	public readonly FlowLayoutPanel Body;

	public readonly Action46 Header;

	public bool IsFeature;

	private readonly string caption;

	public bool Expanded
	{
		get
		{
			return Body.Visible;
		}
		set
		{
			Body.Visible = value;
			Header.Text = (IsFeature ? "" : (caption + (value ? "  −" : "  ＋")));
			PerformLayout();
		}
	}

	public void FitWidth(int width)
	{
		MinimumSize = new Size(width, 0);
		MaximumSize = new Size(width, 0);
		Header.Width = width;
		Body.MinimumSize = new Size(width, 0);
		Body.MaximumSize = new Size(width, 0);
		foreach (Control control in Body.Controls)
		{
			if (control.Tag as string == "full")
			{
				if (control is Fold46 fold)
				{
					fold.FitWidth(width);
				}
				else
				{
					control.Width = width;
				}
			}
		}
		PerformLayout();
	}

	public Fold46(string name)
	{
		caption = name;
		base.Tag = "full";
		base.Width = 390;
		base.FlowDirection = FlowDirection.TopDown;
		base.WrapContents = false;
		AutoSize = true;
		AutoSizeMode = AutoSizeMode.GrowAndShrink;
		base.Margin = new Padding(0, 3, 0, 3);
		Header = new Action46
		{
			Name = name,
			Text = name + "  ＋",
			Width = 390,
			Margin = Padding.Empty
		};
		Body = new FlowLayoutPanel
		{
			Visible = false,
			Width = 390,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			WrapContents = true,
			Margin = new Padding(0, 6, 0, 4)
		};
		base.Controls.Add(Header);
		base.Controls.Add(Body);
		Action46 header = Header;
		EventHandler value = delegate
		{
			Expanded = !Expanded;
		};
		header.Click += value;
		base.SizeChanged += delegate
		{
			Header.Width = base.Width;
			Body.MinimumSize = new Size(base.Width, 0);
			Body.MaximumSize = new Size(base.Width, 0);
		};
	}
}
