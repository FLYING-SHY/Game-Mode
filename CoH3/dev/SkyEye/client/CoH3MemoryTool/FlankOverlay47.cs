using System.Drawing;
using System.Windows.Forms;

namespace CoH3MemoryTool;

internal sealed class FlankOverlay47 : Form
{
	public MineCamera Camera;

	public float X;

	public float Y;

	public float Z;

	public int Radius;

	public string Caption = "包抄监测";

	public bool Danger;

	public bool ShowRange52 = true;

	public float RangeWidth52 = 2f;

	public Color OwnColour52 = Color.White;

	protected override bool ShowWithoutActivation => true;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 134742176;
			return createParams;
		}
	}

	public FlankOverlay47()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.TopMost = true;
		BackColor = Color.FromArgb(1, 2, 3);
		base.TransparencyKey = BackColor;
		DoubleBuffered = true;
		base.AutoScaleMode = AutoScaleMode.None;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (!LicenseGate60.Allowed || Camera == null)
		{
			return;
		}
		PointF point;
		bool flag = Camera.Project(X, Y + 0.5f, Z, base.Width, base.Height, out point);
		Color foreColor = (Danger ? Color.OrangeRed : Color.Aqua);
		FlankRangeDrawing52.Draw(e.Graphics, Camera, X, Y, Z, Radius, base.Width, base.Height, OwnColour52, RangeWidth52, ShowRange52);
		if (!flag)
		{
			return;
		}
		using (Pen pen = new Pen(OwnColour52, 2f))
		{
			e.Graphics.DrawEllipse(pen, point.X - 7f, point.Y - 7f, 14f, 14f);
		}
		using Font font = Interface45.Font(12f, FontStyle.Bold);
		TextRenderer.DrawText(e.Graphics, Caption, font, new Point((int)point.X + 12, (int)point.Y - 16), foreColor, Color.Black, TextFormatFlags.NoPrefix);
	}
}
