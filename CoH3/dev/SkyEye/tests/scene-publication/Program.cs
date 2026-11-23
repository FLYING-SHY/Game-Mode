using System.Text.Json;
using CoH3MemoryTool;

internal static class Program
{
    private sealed class Frame
    {
        public string Match;
        public string Content;
    }

    private static readonly List<string> passed = new();

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAIL " + name);
        passed.Add(name);
    }

    private static void Wait(ManualResetEventSlim signal)
    {
        if (!signal.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("fixture signal timeout");
    }

    private static T Finish<T>(Task<T> task)
    {
        if (!task.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("fixture task timeout");
        return task.GetAwaiter().GetResult();
    }

    private static void LateFrame()
    {
        var scenes = new ScenePublication<Frame>();
        scenes.Publish(new Frame { Match = "same-addresses", Content = "old" });
        using var sampled = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        var presented = new List<string>();
        Task<bool> worker = Task.Run(() =>
        {
            Frame frame = scenes.Capture(out long token);
            sampled.Set();
            Wait(resume);
            return scenes.TryPresent(token, () => presented.Add(frame.Content));
        });
        try
        {
            Wait(sampled);
            scenes.Publish(null);
            scenes.Publish(new Frame { Match = "same-addresses", Content = "new" });
        }
        finally { resume.Set(); }
        Check(!Finish(worker) && presented.Count == 0, "in-flight old frame rejected after clear and same-key new scene");
        Frame fresh = scenes.Capture(out long newToken);
        Check(fresh.Match == "same-addresses" && scenes.TryPresent(newToken, () => presented.Add(fresh.Content)), "new generation can present with reused match key");
        Check(presented.SequenceEqual(new[] { "new" }), "no old content reaches presentation");
    }

    private static void AtomicPresentation()
    {
        var scenes = new ScenePublication<Frame>();
        scenes.Publish(new Frame { Match = "A" });
        scenes.Capture(out long token);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var attemptingClear = new ManualResetEventSlim();
        using var cleared = new ManualResetEventSlim();
        Task<bool> commit = Task.Run(() => scenes.TryPresent(token, () =>
        {
            entered.Set();
            Wait(release);
        }));
        Task<bool> clear = null;
        try
        {
            Wait(entered);
            clear = Task.Run(() =>
            {
                attemptingClear.Set();
                scenes.Publish(null);
                cleared.Set();
                return true;
            });
            Wait(attemptingClear);
            Check(!cleared.IsSet, "revocation waits for a presentation already in progress");
        }
        finally { release.Set(); }
        Check(Finish(commit) && Finish(clear), "commit and revocation both complete without deadlock");
        bool lateAction = false;
        Check(!scenes.TryPresent(token, () => lateAction = true) && !lateAction, "no presentation can start after clear returned");
    }

    private static void Publication()
    {
        var scenes = new ScenePublication<Frame>();
        Check(scenes.Capture(out long initial) == null, "starts detached");
        Check(!scenes.TryPresent(initial, () => throw new Exception("unexpected draw")), "detached presentation rejected");
        var first = new Frame { Match = "A", Content = "first" };
        scenes.Publish(first);
        Check(ReferenceEquals(scenes.Capture(out long token), first), "capture returns a complete published scene");
        scenes.Publish(new Frame { Match = "A", Content = "refresh" });
        Check(scenes.Capture(out long refreshed).Content == "refresh" && refreshed == token, "ordinary refresh keeps the connection generation");
        Check(scenes.TryPresent(token, () => { }), "ordinary refresh permits a frame already in progress");
        scenes.Publish(null);
        Check(scenes.Capture(out long revoked) == null && revoked != token, "clear advances generation and removes scene");
        scenes.Publish(null);
        Check(scenes.Capture(out long repeated) == null && repeated != revoked, "repeated clear revokes independently");
        scenes.Publish(new Frame { Match = "B" });
        scenes.Capture(out long newToken);
        Check(!scenes.TryPresent(token, () => throw new Exception("unexpected old draw")), "old generation stays revoked after publishing B");
        Check(scenes.TryPresent(newToken, () =>
        {
            Check(scenes.TryPresent(newToken, () => { }), "paint callback can reenter the presentation gate");
        }), "presentation callback commits");
        bool propagated = false;
        try { scenes.TryPresent(newToken, () => throw new InvalidOperationException("draw failure")); }
        catch (InvalidOperationException ex) { propagated = ex.Message == "draw failure"; }
        Check(propagated && scenes.TryPresent(newToken, () => { }), "callback failure propagates and releases gate");
    }

    private static void Integration(string repo)
    {
        string sky = Path.Combine(repo, "CoH3/dev/SkyEye/client/CoH3MemoryTool");
        string simple = Path.Combine(repo, "CoH3/dev/COH3SimpleTool/CoH3MemoryTool");
        foreach (string name in new[] { "ScenePublication.cs", "AirOverlayWindow.cs", "SquadMarkerWindow.cs" })
            Check(File.ReadAllBytes(Path.Combine(sky, name)).SequenceEqual(File.ReadAllBytes(Path.Combine(simple, name))), "mirror equality " + name);
        foreach (string root in new[] { sky, simple })
        {
            foreach (string name in new[] { "AirOverlayWindow.cs", "SquadMarkerWindow.cs" })
            {
                string source = File.ReadAllText(Path.Combine(root, name));
                Check(source.Contains("scene.Capture(out long generation)") && source.Contains("connectionGeneration != generation") && source.Contains("scene.TryPresent(generation, delegate"), "capture, reconnect and final commit wired " + root + "/" + name);
            }
            Check(File.ReadAllText(Path.Combine(root, "SquadMarkerWindow.cs")).Contains("scene.TryPresent(paintedGeneration, delegate { PaintScene(e); });"), "queued paint uses the same generation gate " + root);
        }
    }

    private static int Main(string[] args)
    {
        Publication();
        LateFrame();
        AtomicPresentation();
        Integration(Path.GetFullPath(args[0]));
        string report = JsonSerializer.Serialize(new
        {
            status = "PASS_fixture_only", passed = passed.Count, checks = passed,
            real_game = "not_run", gui = "not_run", native_reader = "not_linked",
            limitation = "Executes production ScenePublication and deterministic concurrency; window wiring checked from source. Does not prove result-page activity detection."
        }, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "fixture-report.json"), report + Environment.NewLine);
        Console.WriteLine("PASS_fixture_only checks=" + passed.Count + "; real_game=not_run; gui=not_run");
        return 0;
    }
}
