namespace CoH3Minimap;

internal readonly record struct NativeFrameRenderResult(NativeMarkRenderCounts Marks, bool BackgroundLoaded,
    int MissingSelectionColor, bool CameraViewportDrawn, bool TerritoryDrawn = false, bool FogDrawn = false,
    int SquadsDrawn = 0);

internal static class NativeFrameRendering
{
    internal const int LogicalSize = 268;

    internal static NativeFrameRenderResult Draw(Graphics graphics, RectangleF target, NativeHudFrame frame, NativeAssetCatalog assets,
        bool experimentalCpuFog = false, bool showAllUnits = false)
    {
        var geometry = frame.Map.Geometry;
        if (geometry is null) return default;
        var saved = graphics.Save();
        try
        {
            graphics.SetClip(target);
            var canvas = NativeMapProjection.FitRotatedSquare(target, geometry.AngleDegrees);
            graphics.TranslateTransform(canvas.X, canvas.Y);
            graphics.ScaleTransform(canvas.Width / LogicalSize, canvas.Height / LogicalSize);
            var logical = new RectangleF(0, 0, LogicalSize, LogicalSize);
            Bitmap? terrain = assets.Resolve(frame.Map.Minimap?.AssetPath);
            if (terrain is not null)
                NativeMapRendering.Draw(graphics, NativeMapProjection.FitUniform(terrain.Size, logical), geometry.AngleDegrees,
                    terrain, null, null);
            using var territory = frame.Layers?.Territory?.ToBitmap();
            using var interactivity = frame.Layers?.Interactivity?.ToBitmap();
            using var masked = territory is not null && interactivity is not null
                ? NativeMapRendering.ApplyInteractivityMask(territory, interactivity) : null;
            if (masked is not null)
                NativeMapRendering.Draw(graphics, logical, geometry.AngleDegrees, null, masked, null);
            // Candidate only: mod=1 and no temporal history. Keep native markers above the Fog layer.
            using var fog = experimentalCpuFog && frame.Fog is { UiDisabled: false } planes
                ? NativeFogGeneration.SteadyStateBlackAlpha(planes)?.ToBitmap() : null;
            if (fog is not null)
                NativeMapRendering.Draw(graphics, logical, geometry.AngleDegrees, null, null, fog);
            var inputs = new List<NativeMarkRenderInput>();
            int missingColor = 0;
            foreach (var mark in frame.Map.Marks)
            {
                if (mark.SelectionColor is null) { missingColor++; continue; }
                frame.Icons.TryGetValue(mark.IconHandle, out var icon);
                inputs.Add(new(mark, icon?.AssetPath, mark.SelectionColor.ToColor()));
            }
            var counts = NativeMarkRendering.Draw(graphics, new NativeMapProjection(logical, geometry.AngleDegrees),
                new SizeF(geometry.PlayableWidth, geometry.PlayableHeight), inputs, path => assets.Resolve(path));
            bool cameraDrawn = false;
            if (geometry.PerspectiveBox.Count == 4)
            {
                var projection = new NativeMapProjection(logical, geometry.AngleDegrees);
                var points = new PointF[4];
                int[] order = { 0, 1, 3, 2 }; // Native PathGeometry builder 228430.
                cameraDrawn = true;
                for (int i = 0; i < order.Length; i++)
                {
                    var point = geometry.PerspectiveBox[order[i]];
                    if (!projection.TryProjectPosition(point.X, point.Y, geometry.PlayableWidth, geometry.PlayableHeight, out points[i]))
                    { cameraDrawn = false; break; }
                }
                if (cameraDrawn)
                {
                    using var stroke = new Pen(Color.White, 1);
                    graphics.DrawPolygon(stroke, points);
                }
            }
            // 全部玩家小队：画在迷雾之上（敌军全局显示核心），与 marks 同一 TryProjectPosition 变换路径。
            int squadsDrawn = 0;
            if (showAllUnits && frame.Squads is { Count: > 0 } squadList)
            {
                var squadProjection = new NativeMapProjection(logical, geometry.AngleDegrees);
                using var squadBorder = new Pen(Color.Black, 1);
                // 队伍着色（Player_GetTeam 同源）：本地队伍=蓝（含队友），其余队伍=红；
                // 队伍读不到时退回本地玩家 ID 判定（本地=蓝，其余=红）。
                uint localPlayerId = frame.LocalPlayerId ?? uint.MaxValue;
                uint localTeam = frame.LocalTeam ?? uint.MaxValue;
                foreach (var squad in squadList)
                {
                    if (!squadProjection.TryProjectPosition(squad.X, squad.Z,
                        geometry.PlayableWidth, geometry.PlayableHeight, out var point)) continue;
                    bool friendly = squad.Team is uint team && frame.LocalTeam is not null
                        ? team == localTeam
                        : squad.PlayerId == localPlayerId;
                    Color color = friendly ? Color.FromArgb(70, 130, 255) : Color.FromArgb(255, 70, 70);
                    using var fill = new SolidBrush(color);
                    Bitmap? icon = null;
                    foreach (string candidate in SquadIconPaths(squad.Name, squad.Category))
                    {
                        icon = assets.Resolve(candidate);
                        if (icon is not null) break;
                    }
                    if (icon is null)
                    {
                        // Unknown/图标彻底缺失：保持实心圆点兜底。
                        graphics.FillEllipse(fill, point.X - 4, point.Y - 4, 8, 8);
                        graphics.DrawEllipse(squadBorder, point.X - 4, point.Y - 4, 8, 8);
                    }
                    else
                    {
                        // 22px 敌我底色圆 + 16px 图标：露出约 3px 队伍色环（蓝=本队/红=敌队）。
                        graphics.FillEllipse(fill, point.X - 11, point.Y - 11, 22, 22);
                        graphics.DrawEllipse(squadBorder, point.X - 11, point.Y - 11, 22, 22);
                        float scale = 16f / Math.Max(icon.Width, icon.Height);
                        var destination = new RectangleF(point.X - icon.Width * scale / 2,
                            point.Y - icon.Height * scale / 2, icon.Width * scale, icon.Height * scale);
                        graphics.DrawImage(icon, destination);
                    }
                    squadsDrawn++;
                }
            }
            return new(counts, terrain is not null, missingColor, cameraDrawn, masked is not null, fog is not null, squadsDrawn);
        }
        finally { graphics.Restore(saved); }
    }

    private const string UnitIconPrefix = "/WPFGUI;component/icons/common/units/symbols/";
    private static readonly string[] FactionSuffixes = { "_ger", "_us", "_uk", "_ak", "_sov", "_dak" };

    /// <summary>SBP 名 → (阵营图标前缀, 阵营通用步兵图标)；dak/ger 同属 german 系。</summary>
    private static (string? Prefix, string? Infantry) FactionOf(string name) => name.Split('_')[^1] switch
    {
        "ger" or "dak" => ("unit_german_", "grenadier_ger"),
        "us" => ("unit_aef_", "riflemen_us"),
        "uk" => ("unit_british_", "unit_british_tommy_with_tommy_symbol"),
        "sov" => ("unit_soviet_", "unit_soviet_conscript_01_symbol"),
        "ak" => ("unit_west_german_", "unit_west_german_volksgrenadiers_symbol"),
        _ => (null, "attachment_infantry_uk_symbol")
    };

    /// <summary>
    /// 图标候选顺序：①SBP 名原样 ②名+symbol ③阵营前缀变体（unit_/vehicle_×阵营×去后缀×_symbol）
    /// ④去阵营后缀 ⑤类别 fallback（infantry 按阵营通用步兵图，Unknown 交由圆点兜底）。
    /// </summary>
    internal static IEnumerable<string> SquadIconPaths(string name, string category)
    {
        if (name.Length > 0) yield return UnitIconPrefix + name;
        string stripped = name;
        foreach (string suffix in FactionSuffixes)
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                stripped = name[..^suffix.Length];
                break;
            }
        if (name.Length > 0) yield return UnitIconPrefix + name + "_symbol";
        var (prefix, infantryIcon) = FactionOf(name);
        if (prefix is not null && stripped.Length > 0)
        {
            yield return UnitIconPrefix + prefix + stripped + "_symbol";
            yield return UnitIconPrefix + prefix.Replace("unit_", "vehicle_") + stripped + "_symbol";
        }
        if (stripped.Length > 0 && stripped != name) yield return UnitIconPrefix + stripped;
        if (stripped.Length > 0) yield return UnitIconPrefix + stripped + "_symbol";
        switch (category)
        {
            case "armor": yield return UnitIconPrefix + "tank_icon"; break;
            case "vehicle": yield return UnitIconPrefix + "vehicle_light_symbol"; break;
            case "artillery": yield return UnitIconPrefix + "artillery"; break;
            case "teamweapon": yield return UnitIconPrefix + "team_weapon_minimap_icon"; break;
            case "sniper": yield return UnitIconPrefix + "sniper_ger"; break;
            case "airplane":
                yield return UnitIconPrefix + "airplane";
                yield return UnitIconPrefix + "vehicle_light_symbol";
                break;
            case "emplacement":
            case "building": yield return UnitIconPrefix + "emplacement_symbol"; break;
            case "infantry": yield return UnitIconPrefix + infantryIcon; break;
        }
    }
}
