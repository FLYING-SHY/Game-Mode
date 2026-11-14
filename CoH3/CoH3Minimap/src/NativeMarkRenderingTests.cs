using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CoH3Minimap;

internal static class NativeMarkRenderingTests
{
    internal static void Run()
    {
        CheckProjectionAndAngles();
        CheckShapeTemplates();
        CheckIconsAndCounts();
        CheckOrientationAndTint();
        Console.WriteLine("PASS: native Marks templates, centred/flipped/rotated positions, roundf angle mode2, combined icon orientation, explicit tint, official empty-Icon fallbacks, missing/unsupported counts, asset ownership and clipping. Synthetic data only; no game process accessed.");
    }

    private static NativeMarkRenderInput Input(string kind, float scale = 0.5f, float x = 0, float y = 0,
        uint mode = 0, float angle = 0, string? icon = null, Color? color = null) =>
        new(new NativeMarkSample(0, kind, 0, 0, 0, x, y, scale, 0xFFFF00FF, angle, mode), icon, color ?? Color.Lime);

    private static void CheckProjectionAndAngles()
    {
        var projection = new NativeMapProjection(new RectangleF(10, 20, 100, 80), 0);
        ExpectPosition(projection, 50, 40, 85, 40);
        ExpectPosition(projection with { AngleDegrees = 90 }, 50, 40, 80, 85);
        ExpectPosition(projection with { AngleDegrees = -90 }, 50, 40, 40, 35);
        if (NativeMarkRendering.TryProjectPosition(projection, new SizeF(0, 160), 0, 0, out _) ||
            NativeMarkRendering.TryProjectPosition(projection, new SizeF(200, 160), float.NaN, 0, out _))
            throw new Exception("Invalid playable dimensions/positions were accepted.");
        ExpectAngle(0, float.NaN, 0);
        ExpectAngle(2, 0, -90);
        ExpectAngle(2, MathF.PI / 2, 0);
        ExpectAngle(2, -MathF.PI / 2, -180);
        float halfStep = 0.5f / 114.59156036376953f;
        ExpectAngle(2, halfStep, -89.5f);
        ExpectAngle(2, -halfStep, -90.5f);
        if (NativeMarkRendering.TryResolveAngle(1, 0, out _) || NativeMarkRendering.TryResolveAngle(99, 0, out _) ||
            NativeMarkRendering.TryResolveAngle(2, float.PositiveInfinity, out _))
            throw new Exception("Unknown/camera-dependent or non-finite angle was accepted.");
    }

    private static void ExpectPosition(NativeMapProjection projection, float x, float y, float px, float py)
    {
        if (!NativeMarkRendering.TryProjectPosition(projection, new SizeF(200, 160), x, y, out var p) ||
            Math.Abs(p.X - px) > 0.001f || Math.Abs(p.Y - py) > 0.001f)
            throw new Exception($"Centred MapCanvas projection mismatch: {p} vs ({px},{py}).");
    }

    private static void ExpectAngle(uint mode, float raw, float expected)
    {
        if (!NativeMarkRendering.TryResolveAngle(mode, raw, out float actual) || Math.Abs(actual - expected) > 0.001f)
            throw new Exception($"Native roundf angle mismatch: mode={mode}, raw={raw}, actual={actual}, expected={expected}.");
    }

    private static void CheckShapeTemplates()
    {
        using var output = Solid(64, 64, Color.Transparent);
        using var graphics = Graphics.FromImage(output);
        var projection = new NativeMapProjection(new RectangleF(0, 0, 64, 64), 0);
        Bitmap? NoAsset(string _) => throw new Exception("Solid native templates should not request an icon.");
        var squad = Input("SquadMarkModel", scale: 100, mode: 1);
        if (!NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), squad, NoAsset, out var status) ||
            status != NativeMarkDrawStatus.Drawn || output.GetPixel(32, 32).G < 240 || output.GetPixel(37, 32).A != 0 ||
            output.GetPixel(28, 28).A > 60)
            throw new Exception("Squad did not use the fixed 8px ellipse template.");
        if (output.GetPixel(28, 32).G > 180)
            throw new Exception("Squad black outline is absent.");
        graphics.Clear(Color.Transparent);
        var structure = Input("StructureMarkModel", scale: 0.5f);
        NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), structure, NoAsset, out status);
        if (status != NativeMarkDrawStatus.Drawn || output.GetPixel(32, 32).G < 240 ||
            output.GetPixel(26, 26).G < 200 || output.GetPixel(41, 32).A != 0 || output.GetPixel(24, 32).G > 180)
            throw new Exception("Structure did not use the Scale*32 square with black outline.");
        graphics.Clear(Color.Transparent);
        NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), structure with
        { Mark = structure.Mark with { RawScale = -1 } }, NoAsset, out status);
        if (status != NativeMarkDrawStatus.Invalid || output.GetPixel(32, 32).A != 0)
            throw new Exception("Invalid structure scale was rendered.");
    }

    private static void CheckIconsAndCounts()
    {
        using var icon = Solid(4, 4, Color.White);
        using var output = Solid(64, 64, Color.Transparent);
        using var graphics = Graphics.FromImage(output);
        var projection = new NativeMapProjection(new RectangleF(0, 0, 64, 64), 0);
        var requested = new List<string>();
        Bitmap? Resolve(string path) { requested.Add(path); return path == "missing" ? null : icon; }
        foreach (var pair in new[] { ("TeamWeaponMarkModel", NativeMarkRendering.TeamWeaponFallback),
            ("VehicleMarkModel", NativeMarkRendering.VehicleFallback), ("EmplacementMarkModel", NativeMarkRendering.EmplacementFallback) })
        {
            requested.Clear();
            if (!NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), Input(pair.Item1), Resolve, out _) ||
                requested.Count != 1 || requested[0] != pair.Item2)
                throw new Exception($"Wrong official empty-Icon fallback for {pair.Item1}.");
        }
        requested.Clear();
        NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), Input("VehicleMarkModel", icon: "missing"), Resolve, out var status);
        if (status != NativeMarkDrawStatus.MissingAsset || requested.Count != 1 || requested[0] != "missing")
            throw new Exception("Missing explicit Icon incorrectly fell back to a different asset.");
        string[] customKinds = { "CustomPointMarkModel", "CustomStructureMarkModel", "CustomSquadMarkModel",
            "AnvilStrategicPointMarkModel", "AnvilManpowerPointMarkModel", "AnvilVictoryPointMarkModel" };
        foreach (string kind in customKinds)
        {
            NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), Input(kind), Resolve, out status);
            if (status != NativeMarkDrawStatus.MissingAsset) throw new Exception($"Invented fallback for {kind}.");
            NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), Input(kind, icon: "provided"), Resolve, out status);
            if (status != NativeMarkDrawStatus.Drawn) throw new Exception($"Base Icon template missing for {kind}.");
        }
        var counts = NativeMarkRendering.Draw(graphics, projection, new SizeF(64, 64), new[]
        {
            Input("SquadMarkModel"), Input("StructureMarkModel"), Input("VehicleMarkModel"),
            Input("CustomPointMarkModel"), Input("MarkModel"), Input("VehicleMarkModel", mode: 1),
            Input("SquadMarkModel", mode: 99), Input("StructureMarkModel", scale: float.NaN)
        }, Resolve);
        if (counts != new NativeMarkRenderCounts(3, 1, 3, 1)) throw new Exception($"Wrong drawing status counts: {counts}.");
        if (icon.GetPixel(0, 0).ToArgb() != Color.White.ToArgb()) throw new Exception("Caller bitmap was disposed or modified.");
    }

    private static void CheckOrientationAndTint()
    {
        using var asymmetric = Solid(8, 8, Color.Blue);
        using (var g = Graphics.FromImage(asymmetric))
        { using var brush = new SolidBrush(Color.Red); g.FillRectangle(brush, 0, 0, 8, 4); }
        using var output = Solid(64, 64, Color.Transparent);
        using var graphics = Graphics.FromImage(output);
        var projection = new NativeMapProjection(new RectangleF(0, 0, 64, 64), 0);
        Bitmap? Asset(string _) => asymmetric;
        var mark = Input("CustomPointMarkModel", mode: 2, angle: MathF.PI / 2, icon: "asymmetric", color: Color.White);
        NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), mark, Asset, out _);
        if (output.GetPixel(32, 27).R < 240 || output.GetPixel(32, 37).B < 240)
            throw new Exception("The ancestor and icon Y flips did not cancel.");
        graphics.Clear(Color.Transparent);
        NativeMarkRendering.TryDraw(graphics, projection with { AngleDegrees = 90 }, new SizeF(64, 64), mark, Asset, out _);
        if (output.GetPixel(37, 32).R < 240 || output.GetPixel(27, 32).B < 240)
            throw new Exception("Map.Angle was not applied about the icon centre.");
        graphics.Clear(Color.Transparent);
        NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64), mark with
        { Mark = mark.Mark with { RawAngle = 0 } }, Asset, out _);
        if (output.GetPixel(37, 32).R < 240 || output.GetPixel(27, 32).B < 240)
            throw new Exception("Marker angle sign is not Map.Angle minus Angle.");

        using var white = Solid(8, 4, Color.White);
        graphics.Clear(Color.Transparent);
        var tint = Color.FromArgb(128, 40, 180, 80);
        NativeMarkRendering.TryDraw(graphics, projection, new SizeF(64, 64),
            Input("EmplacementMarkModel", scale: 1, color: tint), _ => white, out _);
        var pixel = output.GetPixel(32, 32);
        if (Math.Abs(pixel.A - tint.A) > 2 || Math.Abs(pixel.R - tint.R) > 2 ||
            Math.Abs(pixel.G - tint.G) > 2 || Math.Abs(pixel.B - tint.B) > 2 || output.GetPixel(32, 44).A != 0)
            throw new Exception("Explicit SelectionColor tint or Emplacement Image Stretch=Uniform was lost.");
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = SmoothingMode.None;
        var clipBefore = graphics.ClipBounds;
        NativeMarkRendering.TryDraw(graphics, new NativeMapProjection(new RectangleF(24, 24, 16, 16), 0),
            new SizeF(64, 64), Input("CustomPointMarkModel", scale: 2, icon: "provided"), _ => white, out _);
        if (output.GetPixel(20, 32).A != 0 || graphics.SmoothingMode != SmoothingMode.None || graphics.ClipBounds != clipBefore)
            throw new Exception("Viewport clipping or caller graphics state restoration failed.");
    }

    private static Bitmap Solid(int width, int height, Color color)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(color);
        return bitmap;
    }
}
