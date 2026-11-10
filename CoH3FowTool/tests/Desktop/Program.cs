using System.Drawing;
using System.Text.Json;
using CoH3FowTool.Gui;

internal static class DesktopTests
{
    private static readonly List<string> Passed = new();
    private static Exception? Failure;
    private static string OutputDirectory = "";

    [STAThread]
    private static int Main(string[] args)
    {
        OutputDirectory = Path.GetFullPath(args.Length > 0 ? args[0] : "artifacts");
        Directory.CreateDirectory(OutputDirectory);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var controller = new FixtureController();
        using var form = new MainForm(controller);
        form.Shown += async (_, _) =>
        {
            try { await CheckForm(form, controller); }
            catch (Exception ex) { Failure = ex; }
            finally { form.Close(); }
        };
        Application.Run(form);
        var report = new
        {
            Passed = Failure is null,
            Checks = Passed,
            Failure = Failure?.ToString(),
            Scope = "Synthetic WinForms controller only; no game, network, or license service access.",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        File.WriteAllText(Path.Combine(OutputDirectory, "desktop-tests.json"),
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine(JsonSerializer.Serialize(report));
        return Failure is null ? 0 : 1;
    }

    private static async Task CheckForm(MainForm form, FixtureController controller)
    {
        Button Button(string name) => (Button)form.Controls.Find(name, true).Single();
        TextBox TextBox(string name) => (TextBox)form.Controls.Find(name, true).Single();
        var enable = Button("EnableDisplay");
        var status = Button("CheckStatus");
        var disable = Button("DisableDisplay");
        var restore = Button("RestoreOriginal");
        var activate = Button("ActivateLicense");
        var log = TextBox("OperationLog");
        await Until(() => controller.Initializations == 1 && status.Enabled);
        Check("initialization checks licensing once and never dispatches a game action", controller.Actions.Count == 0);
        Check("unlicensed enable disabled; status/disable/restore enabled", !enable.Enabled && status.Enabled && disable.Enabled && restore.Enabled);
        Check("activation accepts only a nonempty code", !activate.Enabled);
        Capture(form, "unlicensed.png");

        TextBox("GamePath").Text = @"Z:\FixtureOnly\RelicCoH3.exe";
        foreach (var pair in new[] { (status, ToolAction.Status), (disable, ToolAction.Disable), (restore, ToolAction.Restore) })
        {
            int count = controller.Actions.Count;
            pair.Item1.PerformClick();
            await Until(() => status.Enabled && controller.Actions.Count > count);
            Check("unlicensed recovery dispatch: " + pair.Item2,
                controller.Actions.Last() == pair.Item2 && controller.LastPath == @"Z:\FixtureOnly\RelicCoH3.exe");
        }

        TextBox("ActivationCode").Text = "SYNTHETIC-FIXTURE-CODE";
        Check("nonempty activation code enables activation", activate.Enabled);
        activate.PerformClick();
        await Until(() => enable.Enabled);
        Check("activation enables GUI only after controller acceptance", controller.Activations == 1);
        Check("successful activation clears the input", TextBox("ActivationCode").Text.Length == 0);
        enable.PerformClick();
        await Until(() => status.Enabled && controller.Actions.Last() == ToolAction.Enable);
        Check("enable dispatch after valid authorization", controller.Actions.Last() == ToolAction.Enable);
        Capture(form, "activated.png");

        controller.Current = new LicenseView(true, "限时授权 · 可用", "合成可信状态与本机时钟偏差测试", DateTimeOffset.UtcNow.AddMinutes(-5), "fixture-device");
        await Task.Delay(1100);
        Check("trusted license status is not overridden by the local wall clock", enable.Enabled && status.Enabled);
        controller.Current = new LicenseView(false, "授权已到期", "合成可信到期状态", DateTimeOffset.UtcNow.AddMinutes(-5), "fixture-device");
        await Until(() => !enable.Enabled && status.Enabled);
        Check("timer prevents new enabling after trusted expiry without network", disable.Enabled && restore.Enabled);
        Check("timer uses in-memory snapshots", controller.Initializations == 1 && controller.Activations == 1);

        controller.Current = new LicenseView(true, "永久授权 · 已激活", "合成授权；不是发行许可证", null, "fixture-device");
        await Until(() => enable.Enabled);
        controller.ThrowNextAction = true;
        status.PerformClick();
        await Until(() => status.Enabled && log.Text.Contains("fixture action failed"));
        Check("operation exceptions recover controls without overwriting license", enable.Enabled && disable.Enabled && restore.Enabled);

        controller.PendingAction = new TaskCompletionSource<ToolResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        int before = controller.Actions.Count;
        disable.PerformClick();
        await Until(() => controller.Actions.Count > before && !status.Enabled);
        form.Close();
        Check("user close waits for a running action", !form.IsDisposed && !status.Enabled);
        controller.PendingAction.SetResult(new ToolResult(0, "合成动作完成"));
        await Until(() => status.Enabled);
        Check("busy completion restores all available controls", enable.Enabled && disable.Enabled && restore.Enabled);
        form.Size = form.MinimumSize;
        await Task.Delay(100);
        Capture(form, "minimum-window.png");
    }

    private static void Check(string name, bool success)
    {
        if (!success) throw new InvalidOperationException(name);
        Passed.Add(name);
    }

    private static async Task Until(Func<bool> condition)
    {
        var timeout = DateTime.UtcNow.AddSeconds(8);
        while (!condition())
        {
            if (DateTime.UtcNow > timeout) throw new TimeoutException("GUI condition did not settle.");
            await Task.Delay(20);
        }
    }

    private static void Capture(Form form, string file)
    {
        using var image = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(image, new Rectangle(Point.Empty, form.Size));
        image.Save(Path.Combine(OutputDirectory, file), System.Drawing.Imaging.ImageFormat.Png);
    }

    private sealed class FixtureController : IDesktopController
    {
        internal LicenseView Current = new(false, "尚未激活", "合成界面测试；不访问游戏与授权后端。", null, "fixture-device");
        internal int Initializations;
        internal int Activations;
        internal List<ToolAction> Actions = new();
        internal string LastPath = "";
        internal bool ThrowNextAction;
        internal TaskCompletionSource<ToolResult>? PendingAction;
        public string DefaultGamePath => @"Z:\FixtureOnly\RelicCoH3.exe";
        public LicenseView CurrentLicense => Current;
        public Task<LicenseView> InitializeAsync() { Initializations++; return Task.FromResult(Current); }
        public Task<LicenseView> ActivateAsync(string code)
        {
            Activations++;
            Current = new(true, "永久授权 · 已激活", "合成授权；不是发行许可证。", null, "fixture-device");
            return Task.FromResult(Current);
        }
        public Task<ToolResult> ExecuteAsync(ToolAction action, string gamePath)
        {
            LastPath = gamePath;
            Actions.Add(action);
            if (ThrowNextAction) { ThrowNextAction = false; throw new InvalidOperationException("fixture action failed"); }
            return PendingAction?.Task ?? Task.FromResult(new ToolResult(0, "合成动作记录；没有执行游戏操作。"));
        }
    }
}
