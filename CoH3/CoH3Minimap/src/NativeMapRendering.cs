using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CoH3Minimap;

/// <summary>
/// The transforms declared in the extracted hudpage.xaml and mapcontrol.xaml.
/// Image inputs are normalized; UI mark positions use the centered MapCanvas formula.
/// MapCanvas local coordinates are statically verified; physical UI scale still needs comparison.
/// </summary>
internal readonly record struct NativeMapProjection(RectangleF Viewport, float AngleDegrees)
{
    internal bool TryProject(float u, float v, bool flipY, out PointF point)
    {
        point = default;
        if (!float.IsFinite(u) || !float.IsFinite(v) || !float.IsFinite(AngleDegrees) ||
            !float.IsFinite(Viewport.X) || !float.IsFinite(Viewport.Y) ||
            !float.IsFinite(Viewport.Width) || !float.IsFinite(Viewport.Height) ||
            Viewport.Width <= 0 || Viewport.Height <= 0) return false;

        return TryTransform((u - 0.5) * Viewport.Width, (v - 0.5) * Viewport.Height, flipY, out point);
    }

    // MapCanvas::ArrangeOverride 52A140: preserve native float32 operation order.
    // Position already belongs to the centered native map coordinate system.
    internal bool TryProjectPosition(float x, float y, float playableWidth, float playableHeight, out PointF point)
    {
        point = default;
        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(playableWidth) ||
            !float.IsFinite(playableHeight) || playableWidth <= 0 || playableHeight <= 0 ||
            !float.IsFinite(Viewport.Width) || !float.IsFinite(Viewport.Height) ||
            Viewport.Width <= 0 || Viewport.Height <= 0) return false;
        float anchorX = (x + playableWidth * 0.5f) * (Viewport.Width / playableWidth);
        float anchorY = (y + playableHeight * 0.5f) * (Viewport.Height / playableHeight);
        return TryTransform(anchorX - Viewport.Width * 0.5, anchorY - Viewport.Height * 0.5, true, out point);
    }

    private bool TryTransform(double x, double y, bool flipY, out PointF point)
    {
        point = default;
        if (!float.IsFinite(AngleDegrees) || !float.IsFinite(Viewport.X) || !float.IsFinite(Viewport.Y)) return false;
        if (flipY) y = -y;
        double radians = AngleDegrees * Math.PI / 180;
        double c = Math.Cos(radians), s = Math.Sin(radians);
        float px = (float)(Viewport.X + Viewport.Width / 2 + c * x - s * y);
        float py = (float)(Viewport.Y + Viewport.Height / 2 + s * x + c * y);
        if (!float.IsFinite(px) || !float.IsFinite(py)) return false;
        point = new PointF(px, py);
        return true;
    }

    internal PointF[] ImageCorners(bool flipY)
    {
        if (!TryProject(0, 0, flipY, out var topLeft) ||
            !TryProject(1, 0, flipY, out var topRight) ||
            !TryProject(0, 1, flipY, out var bottomLeft))
            throw new ArgumentException("Invalid native map projection.");
        return new[] { topLeft, topRight, bottomLeft };
    }

    // Fit the rotated canvas inside the independent window without cutting off its corners.
    internal static RectangleF FitRotatedSquare(RectangleF available, float angleDegrees)
    {
        if (!float.IsFinite(angleDegrees) || !float.IsFinite(available.X) || !float.IsFinite(available.Y) ||
            !float.IsFinite(available.Width) || !float.IsFinite(available.Height) || available.Width <= 0 || available.Height <= 0)
            throw new ArgumentException("Invalid rotated native map bounds.");
        double radians = angleDegrees * Math.PI / 180;
        float extent = (float)(Math.Abs(Math.Cos(radians)) + Math.Abs(Math.Sin(radians)));
        float side = Math.Min(available.Width, available.Height) / extent;
        return new RectangleF(available.X + (available.Width - side) / 2,
            available.Y + (available.Height - side) / 2, side, side);
    }

    internal static RectangleF FitUniform(Size source, RectangleF available)
    {
        if (source.Width <= 0 || source.Height <= 0 || available.Width <= 0 || available.Height <= 0 ||
            !float.IsFinite(available.X) || !float.IsFinite(available.Y) ||
            !float.IsFinite(available.Width) || !float.IsFinite(available.Height))
            throw new ArgumentException("Invalid native map image dimensions.");
        float scale = Math.Min(available.Width / source.Width, available.Height / source.Height);
        float width = source.Width * scale, height = source.Height * scale;
        return new RectangleF(available.X + (available.Width - width) / 2,
            available.Y + (available.Height - height) / 2, width, height);
    }
}

/// <summary>
/// Draws only supplied layers. This class does not obtain live game textures.
/// hudpage.xaml:2314-2342: terrain, Stretch=Uniform, rotate Map.Angle.
/// mapcontrol.xaml:300-320: territory masked by interactivity, then fog at 0.9;
/// both overlays use ScaleY=-1 followed by the same rotation about the centre.
/// </summary>
internal static class NativeMapRendering
{
    internal static void Draw(Graphics graphics, RectangleF viewport, float angleDegrees,
        Bitmap? terrain, Bitmap? maskedTerritory, Bitmap? fog)
    {
        var projection = new NativeMapProjection(viewport, angleDegrees);
        var saved = graphics.Save();
        try
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            if (terrain is not null) DrawLayer(graphics, terrain, projection.ImageCorners(false), 1);
            if (maskedTerritory is not null) DrawLayer(graphics, maskedTerritory, projection.ImageCorners(true), 1);
            if (fog is not null) DrawLayer(graphics, fog, projection.ImageCorners(true), 0.9f);
        }
        finally { graphics.Restore(saved); }
    }

    private static void DrawLayer(Graphics graphics, Bitmap bitmap, PointF[] corners, float opacity)
    {
        using var attributes = new ImageAttributes();
        var matrix = new ColorMatrix { Matrix33 = opacity };
        attributes.SetColorMatrix(matrix);
        attributes.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(bitmap, corners, new RectangleF(0, 0, bitmap.Width, bitmap.Height),
            GraphicsUnit.Pixel, attributes);
    }

    // XAML OpacityMask uses the brush's alpha. RGB intensity alone is not a mask.
    // Return value is caller-owned; the source images remain untouched.
    internal static Bitmap ApplyInteractivityMask(Bitmap territory, Bitmap interactivity)
    {
        using var mask = new Bitmap(territory.Width, territory.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(mask))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImage(interactivity, new Rectangle(0, 0, mask.Width, mask.Height));
        }
        var result = new Bitmap(territory.Width, territory.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(result))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(territory, 0, 0);
        }
        var rectangle = new Rectangle(0, 0, result.Width, result.Height);
        var pixels = result.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        BitmapData? alpha = null;
        try
        {
            alpha = mask.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var row = new byte[result.Width * 4];
            var maskRow = new byte[mask.Width * 4];
            for (int y = 0; y < result.Height; y++)
            {
                var address = IntPtr.Add(pixels.Scan0, y * pixels.Stride);
                Marshal.Copy(address, row, 0, row.Length);
                Marshal.Copy(IntPtr.Add(alpha.Scan0, y * alpha.Stride), maskRow, 0, maskRow.Length);
                for (int i = 3; i < row.Length; i += 4)
                    row[i] = (byte)((row[i] * maskRow[i] + 127) / 255);
                Marshal.Copy(row, 0, address, row.Length);
            }
        }
        catch
        {
            result.UnlockBits(pixels);
            result.Dispose();
            throw;
        }
        finally
        {
            if (alpha is not null) mask.UnlockBits(alpha);
        }
        result.UnlockBits(pixels);
        return result;
    }
}
