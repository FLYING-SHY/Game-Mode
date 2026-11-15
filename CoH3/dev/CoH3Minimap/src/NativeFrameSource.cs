using System.Security.Cryptography;

namespace CoH3Minimap;

internal sealed record NativeHudFrame(DateTimeOffset CapturedAt, NativeHudLocation Location, NativeMapSample Map,
    IReadOnlyDictionary<ulong, NativeIconReference?> Icons, NativeCpuLayers? Layers = null, NativeFogPlanes? Fog = null,
    NativeMatchState? MatchState = null, IReadOnlyList<NativeSquadPosition>? Squads = null, uint? LocalPlayerId = null,
    uint? LocalTeam = null);

internal sealed class NativeFrameSource : IDisposable
{
    private readonly GameReader _game;
    private readonly NativeHudLocator _locator;
    private readonly NativeHudReader _models;
    private readonly NativeIconReader _icons;
    private readonly NativeCpuLayerReader _layers;
    private readonly NativeFogReader _fog;
    private readonly NativeMatchStateReader _matchState;
    private readonly NativeSquadReader _squads;
    internal string GameDirectory => Path.GetDirectoryName(_game.ModulePath) ?? throw new IOException("游戏目录尚未就绪。");

    private NativeFrameSource(GameReader game)
    {
        _game = game;
        _locator = new(game); _models = new(game); _icons = new(game); _layers = new(game); _fog = new(game);
        _matchState = new(game); _squads = new(game);
    }

    internal static NativeFrameSource? Attach()
    {
        var game = GameReader.Attach();
        if (game is null) return null;
        try
        {
            string path = game.ModulePath ?? throw new IOException("游戏映像路径尚未就绪。");
            using var image = File.OpenRead(path);
            string hash = Convert.ToHexString(SHA256.HashData(image));
            if (!hash.Equals(NativeHudReader.SupportedImageSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("游戏版本未匹配，等候适配；尚未读取固定数据地址。");
            return new(game);
        }
        catch { game.Dispose(); throw; }
    }

    internal NativeHudFrame? Capture(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_game.IsAlive) throw new IOException("游戏进程已退出。");
        var location = _locator.Locate();
        if (location is null) return null;
        var map = _models.ReadMapModel(location.MapAddress);
        if (map is null) return null;
        var icons = new Dictionary<ulong, NativeIconReference?>();
        foreach (ulong handle in map.Marks.Select(mark => mark.IconHandle).Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();
            icons[handle] = _icons.Resolve(handle);
        }
        var layers = _layers.Read();
        var fog = _fog.Read();
        var matchState = _matchState.Read(location);
        IReadOnlyList<NativeSquadPosition> squadList;
        uint? localPlayerId, localTeam;
        try
        { // 失败静默空列表/空 ID，不中断帧采样。
            squadList = _squads.Read();
            localPlayerId = _squads.ReadLocalPlayerId();
            localTeam = _squads.ReadLocalTeam();
        }
        catch { squadList = Array.Empty<NativeSquadPosition>(); localPlayerId = null; localTeam = null; }
        if (_locator.Locate() != location) return null;
        return new(DateTimeOffset.Now, location, map, icons, layers, fog, matchState, squadList, localPlayerId, localTeam);
    }

    public void Dispose() => _game.Dispose();
}
