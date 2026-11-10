using System;
using System.Threading.Tasks;

namespace CoH3FowTool.Gui
{
    internal enum ToolAction
    {
        Status,
        Enable,
        Disable,
        Restore
    }

    internal record ToolResult(int ExitCode, string Text);

    internal record LicenseView(
        bool CanEnable,
        string Summary,
        string Detail,
        DateTimeOffset? ExpiresAt,
        string DeviceId);

    internal interface IDesktopController
    {
        string DefaultGamePath { get; }

        // A current, thread-safe in-memory snapshot; this getter performs no network I/O.
        LicenseView CurrentLicense { get; }

        // Initialization only checks licensing. Game actions require an explicit user click.
        Task<LicenseView> InitializeAsync();
        Task<LicenseView> ActivateAsync(string code);
        Task<ToolResult> ExecuteAsync(ToolAction action, string gamePath);
    }
}
