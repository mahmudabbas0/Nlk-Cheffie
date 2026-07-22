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

        private NotifyIcon? _notifyIcon;
        private ContextMenuStrip? _trayMenu;
        private ToolStripMenuItem? _statusMenuItem;
        private bool _isWsConnected = false;
        private string? _lastPrinterError = null;
        private bool _isExiting = false;

        public static event Action? OrderListChanged;

        public MainForm()
        {
            InitializeComponent();
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            LocalizationService.LanguageChanged += TranslateUI;
            this.DoubleBuffered = true;
            InitializeTrayIcon();
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

            _ = ProductExtrasCatalog.EnsureLoadedAsync();

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

            if (isCancel)
            {
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    PrintQueueWorker.Cancel(jobId);
                }
                return;
            }

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
                _isWsConnected = isConnected;
                UpdateTrayStatus();

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
                if (isError)
                {
                    _lastPrinterError = message;
                    UpdateTrayStatus();
                    _notifyIcon?.ShowBalloonTip(5000, LocalizationService.T("printers.error_title", "Yazıcı Hatası"), message, ToolTipIcon.Warning);
                }
                else
                {
                    if (_lastPrinterError != null)
                    {
                        _lastPrinterError = null;
                        UpdateTrayStatus();
                    }
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

        private void OnUpdateAvailable(string latestVersion, string downloadUrl, string sha256, bool isMandatory)
        {
            if (this.IsDisposed) return;

            this.Invoke(new Action(() =>
            {
                lblUpdateBanner.Text = $"{LocalizationService.T("updater.update_available_title")} (v{latestVersion})";
                lblUpdateBanner.Visible = true;

                string msg = LocalizationService.T("updater.update_available_msg").Replace("{version}", latestVersion);

                var dialogResult = MessageBox.Show(
                    msg,
                    LocalizationService.T("updater.update_available_title"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                if (dialogResult == DialogResult.Yes)
                {
                    // Trigger download background worker in UI thread or another thread
                    TriggerUpdateDownload(downloadUrl, sha256);
                }
            }));
        }

        private void TriggerUpdateDownload(string downloadUrl, string sha256)
        {
            // Simple visual prompt or direct start
            _ = Task.Run(async () =>
            {
                await _updater!.DownloadAndInstallUpdateAsync(downloadUrl, sha256);
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
            // Apply RightToLeft layout dynamically for Arabic
            bool isRtl = LocalizationService.CurrentLanguage.ToLower() == "ar";
            this.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = isRtl;

            this.Text = LocalizationService.T("tray.tooltip");
            lblBrand.Text = "CHEFFIE POS BRIDGE";
            
            btnNavOrders.Text = " " + LocalizationService.T("tabs.orders");
            btnNavPrinters.Text = " " + LocalizationService.T("tabs.printers");
            btnNavDesigner.Text = " " + LocalizationService.T("tabs.designer");

            // Propagate translation to loaded child controls if active
            _ordersControl?.TranslateUI();
            _printersControl?.TranslateUI();
            _designerControl?.TranslateUI();

            TranslateTrayMenu();

            this.PerformLayout();
            this.Refresh();
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
                    btn.BackColor = Color.FromArgb(24, ThemeManager.ColorAccent);
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

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = LocalizationService.T("tray.tooltip", "Cheffie POS Bridge");
            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += (s, e) => ShowMainForm();

            _trayMenu = new ContextMenuStrip();
            _trayMenu.BackColor = ThemeManager.ColorCard;
            _trayMenu.ForeColor = ThemeManager.ColorText;
            _trayMenu.ShowImageMargin = false;
            _trayMenu.ShowItemToolTips = true;
            _trayMenu.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable());

            _statusMenuItem = new ToolStripMenuItem(LocalizationService.T("tray.status_disconnected", "Durum: Bağlı Değil"))
            {
                Enabled = false,
                Font = ThemeManager.FontBodyBold
            };

            var showItem = new ToolStripMenuItem(LocalizationService.T("tray.show", "Göster"));
            showItem.Click += (s, e) => ShowMainForm();

            var settingsItem = new ToolStripMenuItem(LocalizationService.T("settings.title", "Ayarlar"));
            settingsItem.Click += (s, e) => btnSettings_Click(this, EventArgs.Empty);

            var printerStatusItem = new ToolStripMenuItem(LocalizationService.T("tray.printer_status", "Yazıcı Durumu"));
            printerStatusItem.Click += (s, e) => ShowPrinterStatus();

            var quitItem = new ToolStripMenuItem(LocalizationService.T("tray.quit", "Çıkış"));
            quitItem.Click += (s, e) => ExitApplication();

            _trayMenu.Items.Add(_statusMenuItem);
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add(showItem);
            _trayMenu.Items.Add(settingsItem);
            _trayMenu.Items.Add(printerStatusItem);
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add(quitItem);

            _notifyIcon.ContextMenuStrip = _trayMenu;

            UpdateTrayStatus();
        }

        private void UpdateTrayStatus()
        {
            if (_notifyIcon == null || _statusMenuItem == null) return;

            Color iconColor;
            string statusText;

            if (!_isWsConnected)
            {
                iconColor = Color.FromArgb(244, 67, 54); // Red
                statusText = $"{LocalizationService.T("tray.status", "Durum: ")}{LocalizationService.T("tray.status_disconnected", "Bağlı Değil")}";
                _statusMenuItem.ToolTipText = null;
            }
            else if (!string.IsNullOrEmpty(_lastPrinterError))
            {
                iconColor = Color.FromArgb(255, 152, 0); // Orange/Yellow
                string displayErr = _lastPrinterError;
                if (displayErr.Length > 40)
                {
                    displayErr = displayErr.Substring(0, 37) + "...";
                }
                statusText = $"{LocalizationService.T("tray.printer_error", "Yazıcı Hatası")}: {displayErr}";
                _statusMenuItem.ToolTipText = _lastPrinterError;
            }
            else
            {
                iconColor = Color.FromArgb(76, 175, 80); // Green
                statusText = $"{LocalizationService.T("tray.status", "Durum: ")}{LocalizationService.T("tray.status_connected", "Bağlı")}";
                _statusMenuItem.ToolTipText = null;
            }

            _statusMenuItem.Text = statusText;

            // Generate circle icon dynamically
            using (var bmp = new Bitmap(16, 16))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var brush = new SolidBrush(iconColor))
                {
                    g.FillEllipse(brush, 0, 0, 16, 16);
                }
                var oldIcon = _notifyIcon.Icon;
                _notifyIcon.Icon = Icon.FromHandle(bmp.GetHicon());
                if (oldIcon != null)
                {
                    DestroyIcon(oldIcon.Handle);
                    oldIcon.Dispose();
                }
            }
        }

        private void TranslateTrayMenu()
        {
            if (_notifyIcon == null || _trayMenu == null) return;

            _notifyIcon.Text = LocalizationService.T("tray.tooltip", "Cheffie POS Bridge");

            // Re-fetch translations for all menu items
            _trayMenu.Items[2].Text = LocalizationService.T("tray.show", "Göster");
            _trayMenu.Items[3].Text = LocalizationService.T("settings.title", "Ayarlar");
            _trayMenu.Items[4].Text = LocalizationService.T("tray.printer_status", "Yazıcı Durumu");
            _trayMenu.Items[6].Text = LocalizationService.T("tray.quit", "Çıkış");

            UpdateTrayStatus();
        }

        private void ShowMainForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        private void ShowPrinterStatus()
        {
            var printers = ConfigManager.Current.Printers;
            string kitchen = string.IsNullOrEmpty(printers.Kitchen) ? LocalizationService.T("printers.not_assigned", "Atanmadı") : printers.Kitchen;
            string cashier = string.IsNullOrEmpty(printers.Cashier) ? LocalizationService.T("printers.not_assigned", "Atanmadı") : printers.Cashier;
            string courier = string.IsNullOrEmpty(printers.Courier) ? LocalizationService.T("printers.not_assigned", "Atanmadı") : printers.Courier;

            string msg = $"{LocalizationService.T("settings.kitchen", "Mutfak")}: {kitchen}\n" +
                         $"{LocalizationService.T("settings.cashier", "Kasa")}: {cashier}\n" +
                         $"{LocalizationService.T("settings.courier", "Kurye")}: {courier}\n\n" +
                         $"{LocalizationService.T("tray.last_error", "Son Hata")}: {(_lastPrinterError ?? LocalizationService.T("tray.no_error", "Hata Yok"))}";

            MessageBox.Show(msg, LocalizationService.T("tray.printer_status", "Yazıcı Durumu"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExitApplication()
        {
            _isExiting = true;
            Application.Exit();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_isExiting && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                return;
            }

            StopBackgroundServices();
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            base.OnFormClosing(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LocalizationService.LanguageChanged -= TranslateUI;
            base.OnHandleDestroyed(e);
        }
    }

    public class CustomColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => ThemeManager.ColorCard;
        public override Color MenuBorder => Color.FromArgb(40, Color.White);
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => Color.FromArgb(60, ThemeManager.ColorAccent); // Subtle orange hover highlight
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, ThemeManager.ColorAccent);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, ThemeManager.ColorAccent);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(80, ThemeManager.ColorAccent);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(80, ThemeManager.ColorAccent);
        public override Color ImageMarginGradientBegin => ThemeManager.ColorCard;
        public override Color ImageMarginGradientMiddle => ThemeManager.ColorCard;
        public override Color ImageMarginGradientEnd => ThemeManager.ColorCard;
    }
}
