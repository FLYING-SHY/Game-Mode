using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;

namespace Bf6Esp;

/// <summary>
/// GDI+ 覆盖层绘制。所有绘制都包在 try/catch 里：
/// 目标窗口消失、位图被释放等异常都不允许冒泡打断读取线程。
/// </summary>
internal sealed class EspRenderer
{
	private readonly StringFormat center = new StringFormat
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Near
	};

	private readonly StringFormat left = new StringFormat
	{
		Alignment = StringAlignment.Near,
		LineAlignment = StringAlignment.Near
	};

	public void Render(Graphics graphics, int width, int height, IList<EntitySnapshot> entities, float[] viewProjection, EspSettings settings)
	{
		if (graphics == null || settings == null)
		{
			return;
		}
		try
		{
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			graphics.Clear(Color.Transparent);
			if (settings.ShowFovCircle)
			{
				DrawFovCircle(graphics, width, height, settings);
			}
			if (entities == null || viewProjection == null)
			{
				return;
			}
			foreach (EntitySnapshot entity in entities)
			{
				try
				{
					DrawEntity(graphics, width, height, entity, viewProjection, settings);
				}
				catch (Exception)
				{
					// 单个实体绘制失败直接跳过，绝不影响其它实体。
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static void DrawFovCircle(Graphics graphics, int width, int height, EspSettings settings)
	{
		using (Pen pen = new Pen(Color.FromArgb(70, settings.TextColor), 1f))
		{
			float radius = Math.Max(10f, settings.FovRadius);
			graphics.DrawEllipse(pen, (width / 2) - radius, (height / 2) - radius, radius * 2f, radius * 2f);
		}
	}

	private void DrawEntity(Graphics graphics, int width, int height, EntitySnapshot entity, float[] viewProjection, EspSettings settings)
	{
		if (entity.IsLocalPlayer)
		{
			return;
		}
		if (settings.OnlyEnemies && !entity.IsEnemy)
		{
			return;
		}
		if (settings.OnlyAlive && entity.MaxHealth > 0f && entity.Health <= 0f)
		{
			return;
		}
		if (settings.MaxDistance > 0f && entity.Distance > 0f && entity.Distance > settings.MaxDistance)
		{
			return;
		}
		ScreenPoint60 point;
		if (!WorldToScreen.Project(viewProjection, entity.X, entity.Y, entity.Z, width, height, out point))
		{
			// 相机背后：画一个离屏箭头（若开启）。
			if (settings.ShowOffscreenArrow)
			{
				DrawOffscreenArrow(graphics, width, height, entity, viewProjection, settings);
			}
			return;
		}
		bool inside = point.X >= 0f && point.Y >= 0f && point.X <= width && point.Y <= height;
		if (!inside)
		{
			if (settings.ShowOffscreenArrow)
			{
				DrawOffscreenArrow(graphics, width, height, entity, viewProjection, settings);
			}
			return;
		}
		Color color = PickColor(entity, settings);
		float scale = Scale(entity.Distance);
		float boxHeight = 110f * scale;
		float boxWidth = 46f * scale;
		float left = point.X - (boxWidth / 2f);
		float top = point.Y - boxHeight;
		float right = point.X + (boxWidth / 2f);
		float bottom = point.Y;
		if (settings.ShowSnapLine)
		{
			using (Pen line = new Pen(settings.SnapLineColor, 1f))
			{
				graphics.DrawLine(line, point.X, bottom, width / 2f, height);
			}
		}
		using (Pen pen = new Pen(color, Math.Max(1, settings.BoxThickness)))
		{
			if (settings.BoxStyle == "corner")
			{
				DrawCornerBox(graphics, pen, left, top, right, bottom);
			}
			else
			{
				graphics.DrawRectangle(pen, left, top, boxWidth, boxHeight);
			}
		}
		if (settings.ShowHealthBar)
		{
			DrawHealthBar(graphics, left, top, bottom, entity, settings);
		}
		if (settings.ShowDistance)
		{
			string text = entity.Distance.ToString("0", CultureInfo.InvariantCulture) + "m";
			using (Font font = new Font("Microsoft YaHei UI", Math.Max(8f, 11f * scale), FontStyle.Bold))
			using (SolidBrush brush = new SolidBrush(settings.TextColor))
			{
				graphics.DrawString(text, font, brush, point.X, bottom + 2f, center);
			}
		}
		if (settings.ShowName && !string.IsNullOrEmpty(entity.Name))
		{
			using (Font font = new Font("Microsoft YaHei UI", Math.Max(8f, 11f * scale), FontStyle.Regular))
			using (SolidBrush brush = new SolidBrush(settings.TextColor))
			{
				graphics.DrawString(entity.Name, font, brush, point.X, top - 16f, center);
			}
		}
	}

	private static Color PickColor(EntitySnapshot entity, EspSettings settings)
	{
		if (entity.TeamId < 0)
		{
			return settings.NeutralColor;
		}
		return entity.IsEnemy ? settings.EnemyColor : settings.FriendlyColor;
	}

	private static float Scale(float distance)
	{
		if (distance <= 0f)
		{
			return 1f;
		}
		float scale = 120f / (120f + distance);
		if (scale < 0.35f)
		{
			scale = 0.35f;
		}
		if (scale > 1.2f)
		{
			scale = 1.2f;
		}
		return scale;
	}

	private static void DrawCornerBox(Graphics graphics, Pen pen, float left, float top, float right, float bottom)
	{
		float width = right - left;
		float height = bottom - top;
		float cornerX = width * 0.3f;
		float cornerY = height * 0.25f;
		graphics.DrawLine(pen, left, top, left + cornerX, top);
		graphics.DrawLine(pen, left, top, left, top + cornerY);
		graphics.DrawLine(pen, right, top, right - cornerX, top);
		graphics.DrawLine(pen, right, top, right, top + cornerY);
		graphics.DrawLine(pen, left, bottom, left + cornerX, bottom);
		graphics.DrawLine(pen, left, bottom, left, bottom - cornerY);
		graphics.DrawLine(pen, right, bottom, right - cornerX, bottom);
		graphics.DrawLine(pen, right, bottom, right, bottom - cornerY);
	}

	private static void DrawHealthBar(Graphics graphics, float left, float top, float bottom, EntitySnapshot entity, EspSettings settings)
	{
		if (entity.MaxHealth <= 0f)
		{
			return;
		}
		float ratio = entity.Health / entity.MaxHealth;
		if (ratio < 0f)
		{
			ratio = 0f;
		}
		if (ratio > 1f)
		{
			ratio = 1f;
		}
		float barWidth = 4f;
		float height = bottom - top;
		float x = left - (barWidth + 3f);
		using (SolidBrush background = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
		{
			graphics.FillRectangle(background, x, top, barWidth, height);
		}
		float filled = height * ratio;
		Color color;
		if (ratio > 0.6f)
		{
			color = settings.HealthHighColor;
		}
		else if (ratio > 0.3f)
		{
			color = settings.HealthMidColor;
		}
		else
		{
			color = settings.HealthLowColor;
		}
		using (SolidBrush brush = new SolidBrush(color))
		{
			graphics.FillRectangle(brush, x, bottom - filled, barWidth, filled);
		}
	}

	private static void DrawOffscreenArrow(Graphics graphics, int width, int height, EntitySnapshot entity, float[] viewProjection, EspSettings settings)
	{
		float dx = entity.X;
		float dy = entity.Y;
		float dz = entity.Z;
		float centerX = width / 2f;
		float centerY = height / 2f;
		float angle;
		float radius;
		if (entity.Distance > 0f)
		{
			angle = (float)Math.Atan2(-(entity.Y), entity.X);
			radius = Math.Min(width, height) * 0.42f;
		}
		else
		{
			ScreenPoint60 dummy;
			if (!WorldToScreen.Project(viewProjection, dx, dy, dz, width, height, out dummy))
			{
				return;
			}
			angle = (float)Math.Atan2(dummy.Y - centerY, dummy.X - centerX);
			radius = Math.Min(width, height) * 0.42f;
		}
		float x = centerX + (float)Math.Cos(angle) * radius;
		float y = centerY + (float)Math.Sin(angle) * radius;
		float size = 9f;
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(220, settings.EnemyColor)))
		{
			PointF[] triangle = new PointF[3];
			triangle[0] = new PointF(x, y);
			triangle[1] = new PointF(x - (float)Math.Cos(angle - 0.5) * size, y - (float)Math.Sin(angle - 0.5) * size);
			triangle[2] = new PointF(x - (float)Math.Cos(angle + 0.5) * size, y - (float)Math.Sin(angle + 0.5) * size);
			graphics.FillPolygon(brush, triangle);
		}
	}
}
