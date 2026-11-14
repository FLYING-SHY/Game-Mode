namespace CoH3Minimap;

internal static class NativeHudLocatorTests
{
    internal static void Run()
    {
        const long image = 0x140000000, global = image + 0x06A8CAC8,
            app = 0x200000, service = 0x210000, head = 0x220000,
            root = 0x230000, node = 0x240000, hud = 0x250000, map = 0x260000;
        long key = image + 0x06AD2890;
        var memory = new Dictionary<long, byte[]>
        {
            [global] = new byte[8], [app] = new byte[0x150], [service] = new byte[0x340],
            [head] = new byte[0x20], [root] = new byte[0x38], [node] = new byte[0x38],
            [hud] = new byte[0x9D0], [map] = new byte[0x48]
        };
        void I64(long address, int offset, long value) => BitConverter.GetBytes(value).CopyTo(memory[address], offset);
        byte[]? Read(long address, int length)
        {
            foreach (var (start, data) in memory)
            {
                long offset = address - start;
                if (offset >= 0 && offset + length <= data.Length) return data.AsSpan((int)offset, length).ToArray();
            }
            return null;
        }
        I64(global, 0, app); I64(app, 0x148, service); I64(service, 0x338, head);
        I64(head, 8, root); memory[head][0x19] = 1;
        I64(root, 0, node); I64(root, 0x10, head); I64(root, 0x20, key + 0x100);
        I64(node, 0, head); I64(node, 0x10, head); I64(node, 0x20, key);
        I64(node, 0x28, 0x12345678); // Lock structure, deliberately not the HUD pointer.
        I64(node, 0x30, hud);
        I64(hud, 0, image + 0x03EF3C00); I64(hud, 0x10, 1);
        I64(hud, 0x30, image + 0x03DEEF30); I64(hud, 0x9C8, map);
        I64(map, 0, image + 0x03DF4060); I64(map, 0x30, image + 0x03DF4000); I64(map, 0x40, hud);
        var locator = new NativeHudLocator(Read, image);
        var found = locator.Locate();
        if (found is null || found.HudAddress != hud || found.MapAddress != map || found.RegistryNodeAddress != node)
            throw new Exception("Native service tree lookup did not resolve the weak object's +30 slot.");
        I64(hud, 0x10, 0);
        if (locator.Locate() is not null) throw new Exception("Zero-reference HUD accepted.");
        I64(hud, 0x10, 1);
        I64(map, 0x40, app);
        if (locator.Locate() is not null) throw new Exception("Map with wrong HUD owner accepted.");
        I64(map, 0x40, hud);
        I64(hud, 0, image + 0x03D6DF70);
        if (locator.Locate() is not null) throw new Exception("Wrong HUD runtime type accepted.");
        I64(hud, 0, image + 0x03EF3C00);
        I64(root, 0, root);
        if (locator.Locate() is not null) throw new Exception("Cyclic registry accepted.");
        I64(root, 0, node);
        int globalReads = 0;
        var changedRoot = new NativeHudLocator((address, length) =>
        {
            var data = Read(address, length);
            if (address == global && ++globalReads == 2) return BitConverter.GetBytes(app + 0x100);
            return data;
        }, image);
        if (changedRoot.Locate() is not null) throw new Exception("Changed application root accepted.");
        Console.WriteLine("PASS: native HUD registry traversal, weak +30 object slot, RTTI pairs, owner/refcount guards, cycle rejection and root replacement. Synthetic memory only.");
    }
}
