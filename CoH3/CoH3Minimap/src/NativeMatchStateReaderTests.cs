namespace CoH3Minimap;

internal static class NativeMatchStateReaderTests
{
    internal static void Run()
    {
        const long moduleBase = 0x140000000, hud = 0x100000;
        var location = new NativeHudLocation(0, 0, 0, hud, 0x03EF3C00, 0);
        byte[] cache = new byte[0x31];
        BitConverter.GetBytes(42UL).CopyTo(cache, 0x18);
        cache[0x30] = 1;
        var reader = new NativeMatchStateReader((address, length) => address switch
        {
            hud + 0x1068 => cache.ToArray(),
            moduleBase + 0x06A8CCA0 => BitConverter.GetBytes(43UL),
            _ => null
        }, moduleBase);
        var state = reader.Read(location);
        Check(state is { IsGameOverSpectatingMap: true, CacheEpoch: 42, GlobalEpoch: 43, SameEpoch: false },
            "Native boolean and epoch offsets must be preserved without treating stale cache as an atomic frame.");
        cache[0x30] = 0;
        Check(reader.Read(location) is { IsGameOverSpectatingMap: false }, "False must stay false, not infer a running match.");
        cache[0x30] = 2;
        Check(reader.Read(location) is null, "Invalid native boolean must be rejected.");
        cache[0x30] = 1;
        foreach (ulong epoch in new[] { ulong.MaxValue, ulong.MaxValue - 1 })
        {
            BitConverter.GetBytes(epoch).CopyTo(cache, 0x18);
            Check(reader.Read(location) is null, "Uninitialized/invalidated cache epoch must be rejected.");
        }
        BitConverter.GetBytes(42UL).CopyTo(cache, 0x18);
        int calls = 0;
        var unstable = new NativeMatchStateReader((address, length) =>
        {
            if (address != hud + 0x1068) return null;
            var data = cache.ToArray();
            if (++calls == 2) data[0x30] = 0;
            return data;
        }, moduleBase);
        Check(unstable.Read(location) is null, "Cache changes between reads must discard the hint.");
        var unavailable = new NativeMatchStateReader((_, _) => null, moduleBase);
        Check(unavailable.Read(location) is null, "Read failures must be unknown, not false.");
        var shortRead = new NativeMatchStateReader((_, _) => new byte[0x30], moduleBase);
        Check(shortRead.Read(location) is null, "Short cache reads must be rejected.");
        var noGlobal = new NativeMatchStateReader((address, _) => address == hud + 0x1068 ? cache.ToArray() : null, moduleBase);
        Check(noGlobal.Read(location) is { IsGameOverSpectatingMap: true, GlobalEpoch: null, SameEpoch: false },
            "Missing global epoch must stay unknown while preserving the stable native cached value.");
        int unexpectedReads = 0;
        var guarded = new NativeMatchStateReader((_, _) => { unexpectedReads++; return cache; }, moduleBase);
        Check(guarded.Read(location with { HudVtableRva = 0x03E23640 }) is null && unexpectedReads == 0,
            "Unverified HUD types must not be read at fixed offsets.");
        Check(guarded.Read(location with { HudAddress = 0 }) is null && unexpectedReads == 0,
            "Invalid model pointers must not trigger reads.");
        Console.WriteLine("PASS native match-state hint: exact boolean/epoch offsets, invalidation, torn/short reads, unknown types; no game accessed.");
    }

    private static void Check(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
    }
}
