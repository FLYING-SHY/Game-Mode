using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace CoH3Minimap;

internal static class NativeHudProbe
{
    internal static int Run(string[] args)
    {
        string output = GetOption(args, "--output") ?? Path.Combine(AppContext.BaseDirectory,
            "native-hud-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") + ".json");
        int sampleCount = int.TryParse(GetOption(args, "--samples"), out int parsed) ? Math.Clamp(parsed, 1, 100) : 1;
        string? fogBytesOutput = GetOption(args, "--fog-bytes-output");
        if (fogBytesOutput is not null && sampleCount > 5)
            throw new ArgumentException("Raw fog export supports at most five samples.");
        if (fogBytesOutput is not null && Path.GetFullPath(fogBytesOutput).Equals(Path.GetFullPath(output), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Raw fog export requires a separate output file.");
        var fogByteSamples = new List<object>();
        var samples = new List<object>();
        var attempts = new List<object>();
        var clock = Stopwatch.StartNew();
        var diagnostics = new List<string>();
        int? processId = null;
        string? imagePath = null, imageSha256 = null;
        string status = "GameNotRunning";
        using (var reader = GameReader.Attach())
        {
            if (reader is not null)
            {
                processId = reader.ProcessId;
                imagePath = reader.ModulePath;
                if (imagePath is null) status = "ImagePathUnavailable";
                else
                {
                    using (var image = File.OpenRead(imagePath)) imageSha256 = Convert.ToHexString(SHA256.HashData(image));
                    if (!imageSha256.Equals(NativeHudReader.SupportedImageSha256, StringComparison.OrdinalIgnoreCase))
                        status = "UnsupportedImage";
                    else
                    {
                        var locator = new NativeHudLocator(reader);
                        var native = new NativeHudReader(reader);
                        var icons = new NativeIconReader(reader);
                        var cpu = new NativeCpuLayerReader(reader);
                        var fog = new NativeFogReader(reader);
                        var matchStateReader = new NativeMatchStateReader(reader);
                        status = "HudNotInitializedOrUnstable";
                        for (int index = 0; index < sampleCount && reader.IsAlive; index++)
                        {
                            long startedTicks = clock.ElapsedTicks;
                            bool captured = false;
                            string? discardReason = null;
                            var location = locator.Locate();
                            var map = location is null ? null : native.ReadMapModel(location.MapAddress);
                            var locationAfter = locator.Locate();
                            if (location is not null && map is not null && location == locationAfter)
                            {
                                var references = map.Marks.Select(mark => mark.IconHandle).Distinct()
                                    .Select(handle => icons.Resolve(handle)).ToArray();
                                var layers = cpu.Read();
                                var planes = fog.Read();
                                var matchState = matchStateReader.Read(location);
                                if (planes is not null)
                                    Console.WriteLine($"Fog UI selector: resource=0x{planes.UiResourceAddress:X}, disabled={planes.UiDisabled?.ToString() ?? "unknown"}.");
                                if (location == locator.Locate())
                                {
                                    captured = true;
                                    var capturedAt = DateTimeOffset.Now;
                                    samples.Add(new
                                    {
                                        AttemptIndex = index,
                                        CapturedAt = capturedAt, Location = location, Map = map, Icons = references,
                                        MatchState = matchState,
                                        Territory = ImageInfo(layers.Territory), Interactivity = ImageInfo(layers.Interactivity),
                                        FogPlanes = planes is null ? null : new
                                        {
                                            planes.Width, planes.Height, planes.Mode, planes.Flags, planes.RawConfiguration, planes.Dirty,
                                            planes.UiResourceAddress, planes.UiDisabled,
                                            ByteCountPerPlane = planes.Plane0.Length,
                                            SetBits = new[] { SetBits(planes.Plane0), SetBits(planes.Plane1), SetBits(planes.Plane2) },
                                            Sha256 = new[] { Hash(planes.Plane0), Hash(planes.Plane1), Hash(planes.Plane2) }
                                        }
                                    });
                                    if (fogBytesOutput is not null && planes is not null)
                                        fogByteSamples.Add(new
                                        {
                                            AttemptIndex = index, CapturedAt = capturedAt, Map = map,
                                            MatchState = matchState, FogPlanes = planes,
                                            Territory = layers.Territory, Interactivity = layers.Interactivity
                                        });
                                }
                                else discardReason = "HudChangedAfterLayerRead";
                            }
                            else discardReason = "NativeRootOrModelUnavailableOrChanged";
                            attempts.Add(new
                            {
                                AttemptIndex = index, StartedMonotonicTicks = startedTicks,
                                CompletedMonotonicTicks = clock.ElapsedTicks,
                                Outcome = captured ? "Captured" : "Discarded", Reason = discardReason
                            });
                            if (discardReason is not null) diagnostics.Add($"Sample {index}: {discardReason}; discarded.");
                            if (index + 1 < sampleCount) Thread.Sleep(100);
                        }
                        if (samples.Count > 0) status = "NativeModelsReadRuntimeIdentityNotYetCompared";
                    }
                }
            }
        }
        var report = new
        {
            CreatedAt = DateTimeOffset.Now, Method = "ReadOnlyNativeHudRegistry", ProbeSchemaVersion = 2,
            Status = status, ProcessId = processId, ImagePath = imagePath, ImageSha256 = imageSha256,
            RequestedSamples = sampleCount, AttemptedSamples = attempts.Count, CapturedSamples = samples.Count,
            UnattemptedSamples = sampleCount - attempts.Count,
            MonotonicClock = "Stopwatch.ElapsedTicks", MonotonicClockFrequency = Stopwatch.Frequency,
            AttemptTimingIncludes = "Native reads and diagnostic hashing; not GPU frames or simulation ticks",
            Attempts = attempts,
            GameWritten = false, GameFunctionsInvoked = false, HeapScanned = false,
            SameSceneVisualComparisonPassed = false, LevelBAccepted = false,
            ProjectionWorldOriginVerified = true, FogCompositionVerified = false,
            TeamAndVisibilityInterpretation = "NotInferred:OriginalMarksAndCachedSelectionColor",
            Samples = samples, Diagnostics = diagnostics
        };
        string fullPath = Path.GetFullPath(output);
        string? directory = Path.GetDirectoryName(fullPath);
        if (directory is not null) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
            JsonSerializer.Serialize(stream, report, new JsonSerializerOptions
            {
                WriteIndented = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
            });
        Console.WriteLine($"{status}: {samples.Count}/{sampleCount} native samples. Report: {fullPath}");
        if (fogBytesOutput is not null)
        {
            string fogPath = Path.GetFullPath(fogBytesOutput);
            string? fogDirectory = Path.GetDirectoryName(fogPath);
            if (fogDirectory is not null) Directory.CreateDirectory(fogDirectory);
            var rawReport = new
            {
                CreatedAt = DateTimeOffset.Now, FogFixtureSchemaVersion = 1, ExperimentalOnly = true,
                ProcessId = processId, ImagePath = imagePath, ImageSha256 = imageSha256,
                ProbeReport = fullPath, ProbeReportSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fullPath))),
                ByteArrayEncoding = "Base64", CpuLayerSourceFormat = "BGRA8Straight",
                RequestedSamples = sampleCount, CapturedFogSamples = fogByteSamples.Count,
                AtomicSnapshot = false, GpuFrameHistoryCaptured = false, FogCompositionVerified = false,
                SameSceneVisualComparisonPassed = false, LevelBAccepted = false,
                GameWritten = false, GameFunctionsInvoked = false, HeapScanned = false,
                Samples = fogByteSamples
            };
            using (var stream = new FileStream(fogPath, FileMode.CreateNew, FileAccess.Write))
                JsonSerializer.Serialize(stream, rawReport, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
                });
            Console.WriteLine($"Experimental raw fog: {fogByteSamples.Count} samples. Fixture: {fogPath}");
        }
        return samples.Count > 0 && (fogBytesOutput is null || fogByteSamples.Count > 0) ? 0 : 2;
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static int SetBits(byte[] bytes)
    {
        int count = 0;
        foreach (byte value in bytes) count += System.Numerics.BitOperations.PopCount((uint)value);
        return count;
    }
    private static object? ImageInfo(NativeCpuImage? image) => image is null ? null : new
    {
        image.Width, image.Height, SourceFormat = "BGRA8Straight", ByteCount = image.Bgra.Length,
        Sha256 = Hash(image.Bgra)
    };

    private static string? GetOption(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
