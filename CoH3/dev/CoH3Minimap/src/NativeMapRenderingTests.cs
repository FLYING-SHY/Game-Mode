using System.Drawing.Imaging;

namespace CoH3Minimap;

internal static class NativeMapRenderingTests
{
    internal static void Run()
    {
        var projection = new NativeMapProjection(new RectangleF(10, 20, 100, 80), 0);
        Expect(projection, 0, 0, false, 10, 20);
        Expect(projection, 0, 0, true, 10, 100);
        Expect(projection with { AngleDegrees = 90 }, 0, 0, true, 20, 10);
        Expect(projection with { AngleDegrees = 90 }, 0, 0, false, 100, 10);
        Expect(projection with { AngleDegrees = -90 }, 0.5f, 0.5f, true, 60, 60);
        if (projection.TryProject(float.NaN, 0, true, out _) ||
            (projection with { AngleDegrees = float.PositiveInfinity }).TryProject(0, 0, true, out _))
            throw new Exception("Invalid native projection input was accepted.");
        var centered = new NativeMapProjection(new RectangleF(0, 0, 800, 600), 0);
        if (!centered.TryProjectPosition(100, -50, 400, 200, out var nativePoint) || nativePoint != new PointF(600, 450) ||
            !(centered with { AngleDegrees = 90 }).TryProjectPosition(100, -50, 400, 200, out var rotatedPoint) ||
            Math.Abs(rotatedPoint.X - 250) > 0.001 || Math.Abs(rotatedPoint.Y - 500) > 0.001 ||
            centered.TryProjectPosition(0, 0, 0, 200, out _))
            throw new Exception("Centered native MapCanvas coordinates or ancestor transform order is incorrect.");
        var fitted = NativeMapProjection.FitUniform(new Size(100, 200), new RectangleF(0, 0, 400, 400));
        if (fitted != new RectangleF(100, 0, 200, 400))
            throw new Exception("Stretch=Uniform did not preserve aspect ratio.");

        foreach (float angle in new[] { 0f, 45f, -60f, -90f, 135f })
        {
            var available = new RectangleF(10, 20, 400, 400);
            var canvas = NativeMapProjection.FitRotatedSquare(available, angle);
            var rotated = new NativeMapProjection(canvas, angle);
            foreach (var uv in new[] { new PointF(0, 0), new PointF(1, 0), new PointF(0, 1), new PointF(1, 1) })
                if (!rotated.TryProject(uv.X, uv.Y, false, out var corner) ||
                    corner.X < available.Left - 0.01f || corner.X > available.Right + 0.01f ||
                    corner.Y < available.Top - 0.01f || corner.Y > available.Bottom + 0.01f)
                    throw new Exception("A rotated native map corner was clipped by the independent window.");
        }

        using var territory = Solid(2, 2, Color.FromArgb(200, 20, 80, 120));
        using var mask = Solid(2, 2, Color.FromArgb(128, 255, 255, 255));
        using var masked = NativeMapRendering.ApplyInteractivityMask(territory, mask);
        if (Math.Abs(masked.GetPixel(0, 0).A - 100) > 1 || territory.GetPixel(0, 0).A != 200)
            throw new Exception("Interactivity alpha multiplication changed the source or gave the wrong alpha.");
        using var transparentMask = Solid(2, 2, Color.FromArgb(0, 255, 255, 255));
        using var hiddenTerritory = NativeMapRendering.ApplyInteractivityMask(territory, transparentMask);
        if (hiddenTerritory.GetPixel(1, 1).A != 0)
            throw new Exception("A transparent interactivity mask did not hide territory.");

        using var asymmetric = new Bitmap(8, 8, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(asymmetric))
        {
            graphics.Clear(Color.Blue);
            using var red = new SolidBrush(Color.Red);
            graphics.FillRectangle(red, 0, 0, 8, 4);
        }
        using var output = Solid(32, 32, Color.Black);
        using (var graphics = Graphics.FromImage(output))
            NativeMapRendering.Draw(graphics, new RectangleF(0, 0, 32, 32), 0, asymmetric, null, null);
        if (output.GetPixel(16, 6).R < 240 || output.GetPixel(16, 26).B < 240)
            throw new Exception("Terrain was incorrectly vertically flipped.");
        using (var graphics = Graphics.FromImage(output))
            NativeMapRendering.Draw(graphics, new RectangleF(0, 0, 32, 32), 0, null, asymmetric, null);
        if (output.GetPixel(16, 6).B < 240 || output.GetPixel(16, 26).R < 240)
            throw new Exception("The overlay did not use the independent ScaleY=-1 transform.");

        using var white = Solid(2, 2, Color.White);
        using var black = Solid(2, 2, Color.Black);
        using (var graphics = Graphics.FromImage(output))
            NativeMapRendering.Draw(graphics, new RectangleF(0, 0, 32, 32), 0, white, null, black);
        var fogPixel = output.GetPixel(16, 16);
        if (Math.Abs(fogPixel.R - 26) > 2 || Math.Abs(fogPixel.G - 26) > 2 || Math.Abs(fogPixel.B - 26) > 2)
            throw new Exception("Fog was not composited after terrain at 0.9 opacity.");
        using (var graphics = Graphics.FromImage(output))
        {
            graphics.Clear(Color.Magenta);
            NativeMapRendering.Draw(graphics, new RectangleF(0, 0, 32, 32), 45, null, null, null);
        }
        if (output.GetPixel(16, 16).ToArgb() != Color.Magenta.ToArgb())
            throw new Exception("Missing live layers were replaced with invented image content.");
        Console.WriteLine("PASS: native XAML transform order, independent terrain orientation, uniform fit, interactivity alpha mask, fog order/opacity. Synthetic pixels only; no game process accessed.");
    }

    private static void Expect(NativeMapProjection projection, float u, float v, bool flip, float x, float y)
    {
        if (!projection.TryProject(u, v, flip, out var point) || Math.Abs(point.X - x) > 0.001 || Math.Abs(point.Y - y) > 0.001)
            throw new Exception($"Native projection mismatch: {point} vs ({x},{y}).");
    }

    private static Bitmap Solid(int width, int height, Color color)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(color);
        return bitmap;
    }
}
