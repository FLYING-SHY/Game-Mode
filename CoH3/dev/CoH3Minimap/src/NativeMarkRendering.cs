using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CoH3Minimap;

// SelectionColor is explicit: NativeMarkSample.RawColor is the separate Color property.
internal sealed record NativeMarkRenderInput(NativeMarkSample Mark, string? IconPath, Color SelectionColor);
internal enum NativeMarkDrawStatus { Drawn, MissingAsset, Unsupported, Invalid }
internal readonly record struct NativeMarkRenderCounts(int Drawn, int MissingAssets, int Unsupported, int Invalid);

/// <summary>
/// Base Marks templates from mapcontrol.xaml:590-883, build 26550104.
/// No capture spinner, selection animation, team inference, or generated substitute icons.
/// Bitmap assets are borrowed from the caller. This class never disposes them.
/// </summary>
internal static class NativeMarkRendering
{
    internal const string TeamWeaponFallback = "/WPFGUI;component/icons/common/units/symbols/team_weapon_minimap_icon.png";
    internal const string VehicleFallback = "/WPFGUI;component/icons/common/units/symbols/vehicle_light_symbol.png";
    internal const string EmplacementFallback = "/WPFGUI;component/icons/common/units/symbols/emplacement_symbol.png";

    internal static NativeMarkRenderCounts Draw(Graphics graphics, NativeMapProjection projection,
        SizeF playableSize, IReadOnlyList<NativeMarkRenderInput> marks, Func<string, Bitmap?> bitmapResolver)
    {
        int drawn = 0, missing = 0, unsupported = 0, invalid = 0;
        foreach (var input in marks)
        {
            TryDraw(graphics, projection, playableSize, input, bitmapResolver, out var status);
            switch (status)
            {
                case NativeMarkDrawStatus.Drawn: drawn++; break;
                case NativeMarkDrawStatus.MissingAsset: missing++; break;
                case NativeMarkDrawStatus.Unsupported: unsupported++; break;
                case NativeMarkDrawStatus.Invalid: invalid++; break;
            }
        }
        return new(drawn, missing, unsupported, invalid);
    }

    internal static bool TryDraw(Graphics graphics, NativeMapProjection projection, SizeF playableSize,
        NativeMarkRenderInput input, Func<string, Bitmap?> bitmapResolver, out NativeMarkDrawStatus status)
    {
        status = NativeMarkDrawStatus.Invalid;
        var mark = input.Mark;
        bool squad = mark.ModelType == "SquadMarkModel";
        bool structure = mark.ModelType == "StructureMarkModel";
        bool icon = mark.ModelType is "CustomPointMarkModel" or "CustomStructureMarkModel" or
            "CustomSquadMarkModel" or "AnvilStrategicPointMarkModel" or "AnvilManpowerPointMarkModel" or
            "AnvilVictoryPointMarkModel" or "TeamWeaponMarkModel" or "VehicleMarkModel" or "EmplacementMarkModel";
        if ((!squad && !structure && !icon) || mark.RawAngleMode > 2)
        {
            status = NativeMarkDrawStatus.Unsupported;
            return false;
        }
        if (!TryProjectPosition(projection, playableSize, mark.PositionX, mark.PositionY, out var center)) return false;

        // The Squad template is an 8px ellipse and has no Angle/Scale binding.
        float markerAngle = 0;
        if (!squad && !TryResolveCachedOrRawAngle(mark, out markerAngle))
        {
            status = mark.RawAngleMode == 1 ? NativeMarkDrawStatus.Unsupported : NativeMarkDrawStatus.Invalid;
            return false;
        }
        float size = squad ? 8 : mark.RawScale * 32f;
        if (!float.IsFinite(size) || size <= 0) return false;
        float totalAngle = projection.AngleDegrees - markerAngle;
        if (!float.IsFinite(totalAngle)) return false;

        Bitmap? bitmap = null;
        if (icon)
        {
            string? path = string.IsNullOrEmpty(input.IconPath) ? mark.ModelType switch
            {
                "TeamWeaponMarkModel" => TeamWeaponFallback,
                "VehicleMarkModel" => VehicleFallback,
                "EmplacementMarkModel" => EmplacementFallback,
                _ => null
            } : input.IconPath;
            // XAML falls back only when Icon is empty, not when its asset is missing.
            if (string.IsNullOrEmpty(path) || (bitmap = bitmapResolver(path)) is null)
            {
                status = NativeMarkDrawStatus.MissingAsset;
                return false;
            }
        }

        var saved = graphics.Save();
        try
        {
            graphics.SetClip(projection.Viewport, CombineMode.Intersect);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            if (bitmap is not null)
            {
                float width = size, height = size;
                if (mark.ModelType == "EmplacementMarkModel")
                {
                    // Image defaults to Stretch=Uniform; ShaderBrush rectangles use Fill.
                    float fit = size / Math.Max(bitmap.Width, bitmap.Height);
                    width = bitmap.Width * fit;
                    height = bitmap.Height * fit;
                }
                DrawTintedImage(graphics, bitmap, center, width, height, totalAngle, input.SelectionColor);
            }
            else
            {
                graphics.TranslateTransform(center.X, center.Y);
                if (!squad) graphics.RotateTransform(totalAngle);
                using var fill = new SolidBrush(input.SelectionColor);
                using var stroke = new Pen(Color.Black, 1);
                float half = size / 2;
                if (squad)
                {
                    graphics.FillEllipse(fill, -half, -half, size, size);
                    graphics.DrawEllipse(stroke, -half + 0.5f, -half + 0.5f, size - 1, size - 1);
                }
                else
                {
                    graphics.FillRectangle(fill, -half, -half, size, size);
                    graphics.DrawRectangle(stroke, -half + 0.5f, -half + 0.5f, Math.Max(0, size - 1), Math.Max(0, size - 1));
                }
            }
        }
        finally { graphics.Restore(saved); }
        status = NativeMarkDrawStatus.Drawn;
        return true;
    }

    internal static bool TryProjectPosition(NativeMapProjection projection, SizeF playableSize,
        float x, float y, out PointF center)
    {
        // Use the shared MapCanvas path, including its native float32 operation order.
        return projection.TryProjectPosition(x, y, playableSize.Width, playableSize.Height, out center);
    }

    private static bool TryResolveCachedOrRawAngle(NativeMarkSample mark, out float angleDegrees)
    {
        if (mark.CachedAngle is { } cached && float.IsFinite(cached.Degrees) && cached.CacheEpoch != ulong.MaxValue)
        {
            angleDegrees = cached.Degrees;
            return true;
        }
        return TryResolveAngle(mark.RawAngleMode, mark.RawAngle, out angleDegrees);
    }

    internal static bool TryResolveAngle(uint mode, float rawAngle, out float angleDegrees)
    {
        angleDegrees = 0;
        if (mode == 0) return true;
        // 327AA0 mode 1 needs a camera/global vector absent from NativeMarkSample.
        if (mode != 2 || !float.IsFinite(rawAngle)) return false;
        // 3278F0: IAT 3D55D88=roundf; C roundf uses ties away from zero.
        angleDegrees = MathF.Round(rawAngle * 114.59156036376953f, MidpointRounding.AwayFromZero) * 0.5f - 90f;
        return float.IsFinite(angleDegrees);
    }

    private static void DrawTintedImage(Graphics graphics, Bitmap bitmap, PointF center,
        float width, float height, float angleDegrees, Color tint)
    {
        double radians = angleDegrees * Math.PI / 180;
        double cosine = Math.Cos(radians), sine = Math.Sin(radians);
        PointF Rotate(float x, float y) => new(
            (float)(center.X + cosine * x - sine * y),
            (float)(center.Y + sine * x + cosine * y));
        var corners = new[] { Rotate(-width / 2, -height / 2), Rotate(width / 2, -height / 2), Rotate(-width / 2, height / 2) };
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix
        {
            Matrix00 = tint.R / 255f, Matrix11 = tint.G / 255f,
            Matrix22 = tint.B / 255f, Matrix33 = tint.A / 255f
        });
        attributes.SetWrapMode(WrapMode.TileFlipXY);
        graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
        // The icon's F_y then R(Angle), under ancestor F_y then R(Map.Angle),
        // reduces to R(Map.Angle-Angle): no image mirror remains.
        graphics.DrawImage(bitmap, corners, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel, attributes);
    }
}
