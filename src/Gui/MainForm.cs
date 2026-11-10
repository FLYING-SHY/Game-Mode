#nullable enable

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoH3FowTool.Gui
{
    internal sealed class MainForm : Form
    {
        private readonly IDesktopController _controller;
        private readonly TextBox _gamePath = new TextBox();
        private readonly Button _browse = new Button();
        private readonly Label _licenseSummary = new Label();
        private readonly Label _licenseMetadata = new Label();
        private readonly TextBox _licenseDetail = new TextBox();
        private readonly TextBox _activationCode = new TextBox();
        private readonly Button _activate = new Button();
        private readonly Button _status = new Button();
        private readonly Button _enable = new Button();
        private readonly Button _disable = new Button();
        private readonly Button _restore = new Button();
        private readonly Label _actionHint = new Label();
        private readonly TextBox _log = new TextBox();
        private readonly Label _activity = new Label();
        private readonly ProgressBar _progress = new ProgressBar();
        private readonly System.Windows.Forms.Timer _licenseTimer;
        private LicenseView? _license;
        private bool _authorizationReady;
        private bool _busy;
        private bool _started;

        public MainForm(IDesktopController controller)
        {
            ArgumentNullException.ThrowIfNull(controller);
            _controller = controller;

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Microsoft YaHei UI", 9F);
            Text = "CoH3FowTool · 显示工具";
            ClientSize = new Size(900, 660);
            MinimumSize = new Size(860, 620);
            StartPosition = FormStartPosition.CenterScreen;
            ShowIcon = false;

            BuildLayout();
            _gamePath.Text = controller.DefaultGamePath;
            _licenseSummary.Text = "正在读取授权";
            _licenseMetadata.Text = "到期时间：—";
            _licenseDetail.Text = "启动时检查本机授权，完成后即可操作。";
            _activity.Text = "准备检查授权…";
            UpdateButtons();

            _browse.Click += BrowseGame;
            _activate.Click += async (_, _) => await ActivateAsync();
            _activationCode.TextChanged += (_, _) => UpdateButtons();
            _activationCode.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                if (_activate.Enabled) await ActivateAsync();
            };
            _status.Click += async (_, _) => await ExecuteAsync(ToolAction.Status, "检查状态");
            _enable.Click += async (_, _) => await ExecuteAsync(ToolAction.Enable, "开启显示");
            _disable.Click += async (_, _) => await ExecuteAsync(ToolAction.Disable, "关闭显示");
            _restore.Click += async (_, _) => await ExecuteAsync(ToolAction.Restore, "恢复原状");

            _licenseTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _licenseTimer.Tick += (_, _) => RefreshLicense();
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_started) return;
            _started = true;
            await RunOperationAsync("授权检查", async () =>
            {
                var view = await Task.Run(() => _controller.InitializeAsync());
                if (IsDisposed || Disposing) return;
                _authorizationReady = true;
                ShowLicense(view);
                AppendLog("授权检查：" + view.Summary + Environment.NewLine + view.Detail);
                _activity.Text = "授权检查完成。点击“检查状态”读取游戏状态。";
            }, updatesLicense: true);
            if (!IsDisposed && !Disposing) _licenseTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // The controller owns the operation lifetime; avoid interrupting a running request.
            if (_busy && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                _activity.Text = "操作进行中，请等待完成后关闭窗口。";
                AppendLog("操作进行中，请等待完成后关闭窗口。");
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _licenseTimer?.Dispose();
            base.Dispose(disposing);
        }

        private void BuildLayout()
        {
            SuspendLayout();
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(18)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 4; i++) root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            var title = new Label
            {
                Text = "CoH3 显示工具",
                Font = new Font(Font, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };
            root.Controls.Add(title, 0, 0);
            root.Controls.Add(BuildGamePath(), 0, 1);
            root.Controls.Add(BuildLicense(), 0, 2);
            root.Controls.Add(BuildActions(), 0, 3);

            var logGroup = new GroupBox
            {
                Text = "操作日志（可选中复制 / Ctrl+A 全选）",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 8)
            };
            _log.Name = "OperationLog";
            _log.Dock = DockStyle.Fill;
            _log.Multiline = true;
            _log.ReadOnly = true;
            _log.WordWrap = false;
            _log.ScrollBars = ScrollBars.Both;
            _log.BackColor = SystemColors.Window;
            _log.ShortcutsEnabled = true;
            logGroup.Controls.Add(_log);
            root.Controls.Add(logGroup, 0, 4);

            var footer = NewGrid(2, 1);
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            _activity.Name = "ActivityStatus";
            _activity.Dock = DockStyle.Fill;
            _activity.AutoSize = true;
            _activity.Margin = new Padding(0, 3, 8, 0);
            _progress.Dock = DockStyle.Fill;
            _progress.Height = 16;
            _progress.Style = ProgressBarStyle.Marquee;
            _progress.MarqueeAnimationSpeed = 35;
            _progress.Visible = false;
            footer.Controls.Add(_activity, 0, 0);
            footer.Controls.Add(_progress, 1, 0);
            root.Controls.Add(footer, 0, 5);
            ResumeLayout(true);
        }

        private GroupBox BuildGamePath()
        {
            var grid = NewGrid(2, 2);
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            _gamePath.Name = "GamePath";
            _gamePath.AccessibleName = "游戏主程序完整路径";
            _gamePath.Dock = DockStyle.Fill;
            _gamePath.Margin = new Padding(0, 3, 8, 3);
            ConfigureButton(_browse, "BrowseGame", "选择文件…");
            grid.Controls.Add(_gamePath, 0, 0);
            grid.Controls.Add(_browse, 1, 0);
            var hint = new Label
            {
                Text = "选择 RelicCoH3.exe，或直接粘贴游戏主程序的完整路径。",
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 0, 0)
            };
            grid.Controls.Add(hint, 0, 1);
            grid.SetColumnSpan(hint, 2);
            return InGroup("游戏路径", grid);
        }

        private GroupBox BuildLicense()
        {
            var grid = NewGrid(1, 4);
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _licenseSummary.Name = "LicenseSummary";
            _licenseSummary.AutoSize = true;
            _licenseSummary.Dock = DockStyle.Fill;
            _licenseSummary.Font = new Font(Font, FontStyle.Bold);
            _licenseSummary.Margin = new Padding(0, 0, 0, 4);
            grid.Controls.Add(_licenseSummary, 0, 0);
            _licenseMetadata.Name = "LicenseMetadata";
            _licenseMetadata.AutoSize = true;
            _licenseMetadata.Dock = DockStyle.Fill;
            _licenseMetadata.Margin = new Padding(0, 0, 0, 4);
            grid.Controls.Add(_licenseMetadata, 0, 1);
            _licenseDetail.Name = "LicenseDetail";
            _licenseDetail.AccessibleName = "授权详细信息";
            _licenseDetail.Dock = DockStyle.Fill;
            _licenseDetail.Multiline = true;
            _licenseDetail.ReadOnly = true;
            _licenseDetail.ScrollBars = ScrollBars.Vertical;
            _licenseDetail.BorderStyle = BorderStyle.None;
            _licenseDetail.BackColor = SystemColors.Control;
            _licenseDetail.Height = 38;
            _licenseDetail.Margin = new Padding(0, 0, 0, 8);
            grid.Controls.Add(_licenseDetail, 0, 2);

            var activation = NewGrid(3, 1);
            activation.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            activation.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            activation.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            var caption = new Label
            {
                Text = "授权码",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 0, 10, 0)
            };
            _activationCode.Name = "ActivationCode";
            _activationCode.AccessibleName = "授权码";
            _activationCode.PlaceholderText = "输入授权码后点击激活";
            _activationCode.Dock = DockStyle.Fill;
            _activationCode.Margin = new Padding(0, 3, 8, 3);
            ConfigureButton(_activate, "ActivateLicense", "激活授权");
            activation.Controls.Add(caption, 0, 0);
            activation.Controls.Add(_activationCode, 1, 0);
            activation.Controls.Add(_activate, 2, 0);
            grid.Controls.Add(activation, 0, 3);
            return InGroup("本机授权", grid);
        }

        private GroupBox BuildActions()
        {
            var grid = NewGrid(4, 2);
            for (int i = 0; i < 4; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            ConfigureButton(_status, "CheckStatus", "检查状态");
            ConfigureButton(_enable, "EnableDisplay", "开启显示");
            ConfigureButton(_disable, "DisableDisplay", "关闭显示");
            ConfigureButton(_restore, "RestoreOriginal", "恢复原状");
            grid.Controls.Add(_status, 0, 0);
            grid.Controls.Add(_enable, 1, 0);
            grid.Controls.Add(_disable, 2, 0);
            grid.Controls.Add(_restore, 3, 0);
            _actionHint.Name = "ActionAvailability";
            _actionHint.AutoSize = true;
            _actionHint.Dock = DockStyle.Fill;
            _actionHint.Margin = new Padding(0, 6, 0, 0);
            grid.Controls.Add(_actionHint, 0, 1);
            grid.SetColumnSpan(_actionHint, 4);
            return InGroup("操作", grid);
        }

        private static TableLayoutPanel NewGrid(int columns, int rows)
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = columns,
                RowCount = rows,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                Margin = Padding.Empty
            };
            for (int row = 0; row < rows; row++) grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            return grid;
        }

        private static GroupBox InGroup(string title, Control content)
        {
            var group = new GroupBox
            {
                Text = title,
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 0, 10)
            };
            group.Controls.Add(content);
            return group;
        }

        private static void ConfigureButton(Button button, string name, string text)
        {
            button.Name = name;
            button.Text = text;
            button.Dock = DockStyle.None;
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            button.AutoSize = false;
            button.Size = new Size(100, 32);
            button.MinimumSize = new Size(0, 32);
            button.Margin = new Padding(3, 0, 3, 0);
            button.UseVisualStyleBackColor = true;
        }

        private void BrowseGame(object? sender, EventArgs e)
        {
            try
            {
                using var dialog = new OpenFileDialog
                {
                    Title = "选择游戏主程序",
                    Filter = "游戏主程序 (RelicCoH3.exe)|RelicCoH3.exe|可执行文件 (*.exe)|*.exe",
                    FileName = "RelicCoH3.exe",
                    CheckFileExists = true,
                    Multiselect = false,
                    RestoreDirectory = true
                };
                if (dialog.ShowDialog(this) == DialogResult.OK) _gamePath.Text = dialog.FileName;
            }
            catch (Exception ex)
            {
                AppendLog("选择文件失败：" + DescribeException(ex));
                _activity.Text = "选择文件失败，请查看日志。";
            }
        }

        private async Task ActivateAsync()
        {
            string code = _activationCode.Text.Trim();
            if (code.Length == 0 || _busy) return;
            await RunOperationAsync("激活授权", async () =>
            {
                var view = await Task.Run(() => _controller.ActivateAsync(code));
                if (IsDisposed || Disposing) return;
                _authorizationReady = true;
                ShowLicense(view);
                if (CanEnableNow) _activationCode.Clear();
                AppendLog("激活结果：" + view.Summary + Environment.NewLine + view.Detail);
                _activity.Text = CanEnableNow ? "授权已更新，可开启显示。" : "授权已更新，请查看授权详情。";
            }, updatesLicense: true);
        }

        private async Task ExecuteAsync(ToolAction action, string title)
        {
            if (_busy) return;
            RefreshLicense();
            if (action == ToolAction.Enable && !CanEnableNow)
            {
                AppendLog("开启显示未执行：请先取得有效授权。" + Environment.NewLine + _licenseDetail.Text);
                return;
            }
            string path = _gamePath.Text.Trim();
            await RunOperationAsync(title, async () =>
            {
                var result = await Task.Run(() => _controller.ExecuteAsync(action, path));
                if (IsDisposed || Disposing) return;
                AppendLog(title + "（退出码 " + result.ExitCode + "）" + Environment.NewLine + result.Text);
                _activity.Text = result.ExitCode == 0 ? title + "完成。" : title + "未完成，请查看日志。";
            });
        }

        private async Task RunOperationAsync(string title, Func<Task> operation, bool updatesLicense = false)
        {
            if (_busy || IsDisposed || Disposing) return;
            _busy = true;
            UpdateButtons();
            _activity.Text = title + "…";
            _progress.Visible = true;
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing) return;
                string detail = DescribeException(ex);
                AppendLog(title + "失败：" + detail);
                _activity.Text = title + "失败，请查看日志。";
                if (updatesLicense && !_authorizationReady)
                {
                    _licenseSummary.Text = "授权检查未完成";
                    _licenseDetail.Text = detail;
                }
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    _busy = false;
                    _progress.Visible = false;
                    RefreshLicense();
                    UpdateButtons();
                }
            }
        }

        // The licensing client evaluates expiry from signed server time and a monotonic clock.
        // ExpiresAt is display data, not a second decision based on the local wall clock.
        private bool CanEnableNow => _authorizationReady && _license is { CanEnable: true };

        private void RefreshLicense()
        {
            if (!_authorizationReady || IsDisposed || Disposing) return;
            try
            {
                ShowLicense(_controller.CurrentLicense);
            }
            catch (Exception ex)
            {
                _license = null;
                _licenseSummary.Text = "授权状态读取失败";
                _licenseDetail.Text = DescribeException(ex);
                UpdateButtons();
            }
        }

        private void ShowLicense(LicenseView view)
        {
            _license = view;
            _licenseSummary.Text = view.Summary;
            string expiration = view.ExpiresAt.HasValue
                ? view.ExpiresAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz") + "（本地时间）"
                : view.CanEnable ? "永久授权" : "—";
            string device = string.IsNullOrWhiteSpace(view.DeviceId) ? ""
                : "  ·  设备诊断：" + (view.DeviceId.Length > 12 ? view.DeviceId[..12] + "…" : view.DeviceId);
            _licenseMetadata.Text = "到期时间：" + expiration + device;
            _licenseDetail.Text = view.Detail;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            _gamePath.Enabled = !_busy;
            _browse.Enabled = !_busy;
            _activationCode.Enabled = !_busy;
            _activate.Enabled = !_busy && !string.IsNullOrWhiteSpace(_activationCode.Text);
            _status.Enabled = !_busy;
            _disable.Enabled = !_busy;
            _restore.Enabled = !_busy;
            _enable.Enabled = !_busy && CanEnableNow;
            _actionHint.Text = CanEnableNow
                ? "授权可用；各项操作的实际结果请查看下方日志。"
                : "开启显示需有效授权；检查状态、关闭显示、恢复原状仍可操作。";
        }

        private void AppendLog(string text)
        {
            _log.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text.TrimEnd()
                + Environment.NewLine + Environment.NewLine);
            _log.SelectionStart = _log.TextLength;
            _log.SelectionLength = 0;
            _log.ScrollToCaret();
        }

        private static string DescribeException(Exception exception)
        {
            if (exception is AggregateException aggregate)
            {
                var messages = aggregate.Flatten().InnerExceptions;
                return string.Join(Environment.NewLine, System.Linq.Enumerable.Select(messages, ex => ex.Message));
            }
            return exception.GetBaseException().Message;
        }
    }
}
