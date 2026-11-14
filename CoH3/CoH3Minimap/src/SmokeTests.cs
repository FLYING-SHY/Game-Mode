namespace CoH3Minimap;

internal static class SmokeTests
{
    public static int Run()
    {
        try
        {
            NativeMapRenderingTests.Run();
            NativeHudReaderTests.Run();
            NativeHudLocatorTests.Run();
            NativeIconReaderTests.Run();
            NativeMapGeometryTests.Run();
            NativeMarkRenderingTests.Run();
            NativeFrameRenderingTests.Run();
            NativeCpuLayerReaderTests.Run();
            NativeFogReaderTests.Run();
            NativeFogGenerationTests.Run();
            NativeMatchStateReaderTests.Run();
            var color = new NativeSelectionColor(1, 1, 0, 1, 2);
            var marks = new NativeMarkSample[]
            {
                new(0, "SquadMarkModel", 0, 1, 0, 0, 0, 1, 0, 0, 0, color),
                new(0, "SquadMarkModel", 0, 1, 0, float.NaN, 0, 1, 0, 0, 0, color)
            };
            var sample = new NativeHudFrame(DateTimeOffset.Now, new(0, 0, 0, 0, 0, 0),
                new NativeMapSample(0, 0, 0, 0, 2, 0, marks, new(0, 384, 384, Array.Empty<PointF>(), 1, 1, 1)),
                new Dictionary<ulong, NativeIconReference?>());

            CheckWindow("slow capture", token =>
            {
                if (token.WaitHandle.WaitOne(700)) token.ThrowIfCancellationRequested();
                return sample;
            });
            int calls = 0;
            CheckWindow("capture failure/recovery", token =>
            {
                if (Interlocked.Increment(ref calls) == 1) throw new IOException("Synthetic disconnect");
                return sample;
            });
            if (calls < 2) throw new Exception("Capture did not recover.");
            Console.WriteLine("PASS: projection, responsive UI during slow capture, failure/recovery, invalid-coordinate rendering, worker shutdown. No game process accessed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void CheckWindow(string name, Func<CancellationToken, NativeHudFrame?> capture)
    {
        using var form = new NativeMinimapForm(capture);
        int ticks = 0;
        bool painted = false;
        Exception? failure = null;
        var clock = System.Diagnostics.Stopwatch.StartNew();
        using var timer = new System.Windows.Forms.Timer { Interval = 40 };
        timer.Tick += (_, _) =>
        {
            ticks++;
            if (clock.ElapsedMilliseconds > 1400 && !painted)
            {
                try
                {
                    using var bmp = new System.Drawing.Bitmap(form.Width, form.Height);
                    form.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));
                    // The explicit synthetic SelectionColor must reach the centered native mark.
                    int side = Math.Min(form.ClientSize.Width - 8, form.ClientSize.Height - 56);
                    var color = bmp.GetPixel(form.ClientSize.Width / 2, 4 + side / 2);
                    if (color.R < 200 || color.G < 200 || color.B > 100)
                        throw new Exception("Expected synthetic midpoint was not rendered.");
                }
                catch (Exception ex) { failure = ex; }
                painted = true;
            }
            if (clock.ElapsedMilliseconds > 2100) form.Close();
        };
        timer.Start();
        Application.Run(form);
        var stop = System.Diagnostics.Stopwatch.StartNew();
        while (!form.WorkerStopped && stop.ElapsedMilliseconds < 2000)
            Thread.Sleep(10);
        if (failure is not null) throw new Exception(name, failure);
        if (ticks < 15 || !painted || !form.WorkerStopped)
            throw new Exception($"{name}: ticks={ticks}, painted={painted}, stopped={form.WorkerStopped}");
        Console.WriteLine($"PASS {name}: UI ticks={ticks}, stopped={form.WorkerStopped}");
    }
}
