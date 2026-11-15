namespace CoH3Minimap;

static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            ApplicationConfiguration.Initialize();
            if (args.Contains("--selftest")) return SmokeTests.Run();
            if (args.Contains("--native-probe")) return NativeHudProbe.Run(args);
            Application.Run(new NativeMinimapForm(experimentalCpuFog: args.Contains("--experimental-cpu-fog"),
                showAllUnits: args.Contains("--show-all-units")));
            return 0;
        }
        catch (System.Exception ex)
        {
            System.Console.Error.WriteLine($"FATAL: {ex}");
            return 1;
        }
    }
}
