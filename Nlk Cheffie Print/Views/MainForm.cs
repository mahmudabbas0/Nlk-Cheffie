using System;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Core.Net;
using Nlk_Cheffie_Print.Core.Workers;
using Nlk_Cheffie_Print.Views.Controls;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views
{
    public partial class MainForm : Form
    {
        private OrdersControl? _ordersControl;
        private PrintersControl? _printersControl;
        private DesignerControl? _designerControl;
        private UserControl? _activeControl;

        private HttpOrderPoller? _poller;
        private ReverbSocketClient? _reverbClient;
        private AutoUpdater? _updater;

        public static event Action? OrderListChanged;

        public MainForm()
        {
            InitializeComponent();
            LocalizationService.LanguageChanged += TranslateUI;
            this.DoubleBuffered = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Verify authentication
            if (string.IsNullOrEmpty(ConfigManager.Current?.App?.DeviceToken))
            {
                if (!ShowLoginDialog())
                {
                    Application.Exit();
                    return;
                }
            }

            // Apply theme recursively
            ThemeManager.ApplyTheme(this);
            pnlSidebar.BackColor = ThemeManager.ColorCard;
            pnlTopBar.BackColor = ThemeManager.ColorCard;
            
            // Padding container so children controls have breathing room (not sticking to edges)
            pnlContainer.Padding = new Padding(24, 16, 24, 16);
            
            // Custom sidebar button styling
            ConfigureNavButton(btnNavOrders);
            ConfigureNavButton(btnNavPrinters);
            ConfigureNavButton(btnNavDesigner);

            // Set up status bar restaurant name
            if (ConfigManager.Current?.App != null)
            {
                lblRestaurantName.Text = ConfigManager.Current.App.RestaurantName;
            }

            TranslateUI();
            
            // Start background workers
            StartBackgroundServices();

            // Default to orders page
            ShowControl(GetOrdersControl());
            HighlightNavButton(btnNavOrders);

            // Check for updates
            _ = CheckForUpdates();
        }

        private void StartBackgroundServices()
        {
            PrintQueueWorker.Start();
            PrintQueueWorker.JobProcessed += OnPrintJobProcessed;

            var app = ConfigManager.Current.App;

            // Start HTTP API / RabbitMQ Poller
            _poller = new HttpOrderPoller();
            _poller.OrderReceived += OnOrderReceivedFromNetwork;
            _poller.Start();

            // Start WebSocket Reverb client
            _reverbClient = new ReverbSocketClient();
            _reverbClient.PrintJobReceived += OnPrintJobReceivedFromWebSocket;
            _reverbClient.ConnectionStatusChanged += OnWsConnectionStatusChanged;
            _reverbClient.Start(app.RestaurantId);
        }

        private void StopBackgroundServices()
        {
            PrintQueueWorker.JobProcessed -= OnPrintJobProcessed;
            PrintQueueWorker.Stop();

            _poller?.Stop();
            _poller = null;

            _reverbClient?.Stop();
            _reverbClient = null;
        }

        private void OnOrderReceivedFromNetwork(JsonElement order, string role)
        {
            if (this.IsDisposed) return;

            // Notify UI that log might have changed
            this.Invoke(new Action(() =>
            {
                OrderListChanged?.Invoke();
            }));

            // Auto print if enabled
            if (ConfigManager.Current.App.AutoPrintEnabled)
            {
                string oid = order.TryGetProperty("id", out var idProp) ? idProp.GetRawText() : Guid.NewGuid().ToString();
                var job = new PrintJob
                {
                    Role = role,
                    Data = order,
                    JobId = $"api-order-{oid}"
                };
                PrintQueueWorker.Enqueue(job);
            }
        }

        private void OnPrintJobReceivedFromWebSocket(JsonElement data, bool isCancel)
        {
            if (this.IsDisposed) return;

            // Extract role and slip data
            string role = data.TryGetProperty("role", out var rProp) ? rProp.GetString() ?? "kitchen" : "kitchen";
            var slipData = data.TryGetProperty("slip_data", out var sdProp) ? sdProp : data;
            string jobId = data.TryGetProperty("job_id", out var idProp) ? idProp.GetString() ?? "" : "";

            // Notify UI that order list might have changed
            this.Invoke(new Action(() =>
            {
                OrderListChanged?.Invoke();
            }));

            if (ConfigManager.Current.App.AutoPrintEnabled)
            {
                var job = new PrintJob
                {
                    Role = role,
                    Data = slipData.Clone(),
                    JobId = jobId
                };
                PrintQueueWorker.Enqueue(job);
            }
        }

        private void OnWsConnectionStatusChanged(bool isConnected)
        {
            if (this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                if (isConnected)
                {
                    lblRestaurantName.ForeColor = ThemeManager.ColorSuccess;
                }
                else
                {
                    lblRestaurantName.ForeColor = ThemeManager.ColorDanger;
                }
            }));
        }

        private void OnPrintJobProcessed(string message, bool isError)
        {
            if (this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                // Notify user via tray or status bar
                // We can use a simple tool tip or status message
                if (isError)
                {
                    MessageBox.Show(message, "Yazdırma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }));
        }

        private async Task CheckForUpdates()
        {
            string baseApi = ConfigManager.Current.App.ApiBaseUrl;
            _updater = new AutoUpdater(baseApi);
            _updater.UpdateAvailable += OnUpdateAvailable;
            
            await _updater.CheckForUpdatesAsync();
        }

        private void OnUpdateAvailable(string latestVersion, string downloadUrl, bool isMandatory)
        {
            if (this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                lblUpdateBanner.Text = $"{LocalizationService.T("updater.new_version")} (v{latestVersion})";
                lblUpdateBanner.Visible = true;

                string msg = isMandatory 
                    ? LocalizationService.T("updater.mandatory_msg") 
                    : LocalizationService.T("updater.optional_msg");

                var dialogResult = MessageBox.Show(
                    msg.Replace("{version}", latestVersion),
                    LocalizationService.T("updater.title"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                if (dialogResult == DialogResult.Yes)
                {
                    // Trigger download background worker in UI thread or another thread
                    TriggerUpdateDownload(downloadUrl);
                }
            }));
        }

        private void TriggerUpdateDownload(string downloadUrl)
        {
            // Simple visual prompt or direct start
            _ = Task.Run(async () =>
            {
                await _updater!.DownloadAndInstallUpdateAsync(downloadUrl);
            });
        }

        private bool ShowLoginDialog()
        {
            this.Hide();
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    this.Show();
                    lblRestaurantName.Text = ConfigManager.Current.App.RestaurantName;
                    return true;
                }
            }
            return false;
        }

        private void TranslateUI()
        {
            this.Text = LocalizationService.T("tray.tooltip");
            lblBrand.Text = "CHEFFIE POS BRIDGE";
            
            btnNavOrders.Text = " " + LocalizationService.T("tabs.orders");
            btnNavPrinters.Text = " " + LocalizationService.T("tabs.printers");
            btnNavDesigner.Text = " " + LocalizationService.T("tabs.designer");

            // Propagate translation to loaded child controls if active
            _ordersControl?.TranslateUI();
            _printersControl?.TranslateUI();
            _designerControl?.TranslateUI();
        }

        private void ConfigureNavButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, ThemeManager.ColorAccent);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, ThemeManager.ColorAccent);
            btn.BackColor = Color.Transparent;
            btn.ForeColor = ThemeManager.ColorText;
            btn.Font = ThemeManager.FontBody;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
        }

        private void HighlightNavButton(Button activeBtn)
        {
            Button[] btns = { btnNavOrders, btnNavPrinters, btnNavDesigner };
            foreach (var btn in btns)
            {
                if (btn == activeBtn)
                {
                    btn.BackColor = Color.FromArgb(20, Color.White);
                    btn.ForeColor = ThemeManager.ColorAccent;
                    btn.Font = ThemeManager.FontBodyBold;
                }
                else
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = ThemeManager.ColorText;
                    btn.Font = ThemeManager.FontBody;
                }
            }
        }

        private OrdersControl GetOrdersControl()
        {
            if (_ordersControl == null)
            {
                _ordersControl = new OrdersControl { Dock = DockStyle.Fill };
            }
            return _ordersControl;
        }

        private PrintersControl GetPrintersControl()
        {
            if (_printersControl == null)
            {
                _printersControl = new PrintersControl { Dock = DockStyle.Fill };
            }
            return _printersControl;
        }

        private DesignerControl GetDesignerControl()
        {
            if (_designerControl == null)
            {
                _designerControl = new DesignerControl { Dock = DockStyle.Fill };
            }
            return _designerControl;
        }

        private void ShowControl(UserControl ctrl)
        {
            if (_activeControl == ctrl) return;

            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(ctrl);
            _activeControl = ctrl;
        }

        private void btnNavOrders_Click(object sender, EventArgs e)
        {
            ShowControl(GetOrdersControl());
            HighlightNavButton(btnNavOrders);
        }

        private void btnNavPrinters_Click(object sender, EventArgs e)
        {
            ShowControl(GetPrintersControl());
            HighlightNavButton(btnNavPrinters);
        }

        private void btnNavDesigner_Click(object sender, EventArgs e)
        {
            ShowControl(GetDesignerControl());
            HighlightNavButton(btnNavDesigner);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            // Stop services before showing settings, since settings might reset connections
            StopBackgroundServices();

            using (var settings = new SettingsForm())
            {
                if (settings.ShowDialog() == DialogResult.OK)
                {
                    if (settings.ConnectionResetTriggered)
                    {
                        // Reset control caches
                        _ordersControl = null;
                        _printersControl = null;
                        _designerControl = null;
                        _activeControl = null;
                        
                        // Force login again
                        if (!ShowLoginDialog())
                        {
                            Application.Exit();
                            return;
                        }
                    }
                }
            }

            // Restart services
            StartBackgroundServices();
            ThemeManager.ApplyTheme(this);
            TranslateUI();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopBackgroundServices();
            base.OnFormClosing(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LocalizationService.LanguageChanged -= TranslateUI;
            base.OnHandleDestroyed(e);
        }
    }
}
