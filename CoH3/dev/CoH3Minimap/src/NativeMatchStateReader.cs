namespace CoH3Minimap;

/// <summary>
/// A cached UI property, not a determination that a match is running or that its simulation advances.
/// Native registration 1478DA2/1478DB5 selects getter 13F88C0 for IsGameOverSpectatingMap.
/// The getter reads HUD+1098 and its lazy epoch at HUD+1080; this reader never evaluates it.
/// </summary>
internal sealed record NativeMatchState(bool IsGameOverSpectatingMap, ulong CacheEpoch, ulong? GlobalEpoch)
{
    public bool SameEpoch => GlobalEpoch == CacheEpoch;
}

internal sealed class NativeMatchStateReader
{
    private readonly Func<long, int, byte[]?> _read;
    private readonly long _moduleBase;

    internal NativeMatchStateReader(GameReader reader) : this(
        (address, length) => reader.ReadBytes(new IntPtr(address), length), reader.ModuleBase.ToInt64()) { }

    internal NativeMatchStateReader(Func<long, int, byte[]?> read, long moduleBase)
    {
        _read = read;
        _moduleBase = moduleBase;
    }

    internal NativeMatchState? Read(NativeHudLocation location)
    {
        // Only the Anvil HUD type used by the verified game HUD binding is admitted here.
        if (location.HudVtableRva != 0x03EF3C00 || location.HudAddress < 0x10000 ||
            location.HudAddress >= 0x800000000000 || (location.HudAddress & 7) != 0) return null;
        var before = _read(location.HudAddress + 0x1068, 0x31);
        if (before is not { Length: 0x31 }) return null;
        ulong epoch = BitConverter.ToUInt64(before, 0x18);
        byte value = before[0x30];
        if (epoch == ulong.MaxValue || epoch == ulong.MaxValue - 1 || value > 1) return null;
        var global = _read(_moduleBase + 0x06A8CCA0, 8);
        var after = _read(location.HudAddress + 0x1068, 0x31);
        if (after is not { Length: 0x31 } || !before.AsSpan().SequenceEqual(after)) return null;
        return new(value != 0, epoch, global is { Length: 8 } ? BitConverter.ToUInt64(global) : null);
    }
}
