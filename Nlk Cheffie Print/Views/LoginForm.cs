using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Core.Net;

namespace Nlk_Cheffie_Print.Views
{
    public partial class LoginForm : Form
    {
        private bool _isPasswordHidden = true;
        private LocalLoginServer? _loginServer;

        public LoginForm()
        {
            InitializeComponent();
            LocalizationService.LanguageChanged += OnLanguageChanged;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            TranslateUI();
            
            // Populate token if already exists
            if (ConfigManager.Current?.App != null)
            {
                txtToken.Text = ConfigManager.Current.App.DeviceToken;
            }
        }

        private void OnLanguageChanged()
        {
            TranslateUI();
        }

        private bool _isTokenFocused = false;

        private void ApplyTheme()
        {
            ThemeManager.ApplyTheme(this);
            
            pnlCard.BackColor = ThemeManager.ColorCard;
            pnlTokenWrapper.BackColor = ThemeManager.ColorFieldBg;
            txtToken.BackColor = ThemeManager.ColorFieldBg;
            
            lblStatus.BackColor = Color.FromArgb(35, 239, 68, 68);
            lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
            
            // Draw clean card border
            pnlCard.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                var rect = pnlCard.ClientRectangle;
                using (var pen = new Pen(Color.FromArgb(45, 45, 50), 1))
                {
                    g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };

            // Clean border for token field
            txtToken.Enter += (s, ev) => { _isTokenFocused = true; pnlTokenWrapper.Invalidate(); };
            txtToken.Leave += (s, ev) => { _isTokenFocused = false; pnlTokenWrapper.Invalidate(); };
            pnlTokenWrapper.Paint += (s, ev) =>
            {
                var g = ev.Graphics;
                var rect = pnlTokenWrapper.ClientRectangle;
                Color borderCol = _isTokenFocused ? Color.FromArgb(255, 152, 0) : Color.FromArgb(55, 55, 60);
                using (var pen = new Pen(borderCol, 1f))
                {
                    g.DrawRectangle(pen, 0, 0, rect.Width - 1, rect.Height - 1);
                }
            };

            // Vector eye icon for btnTogglePassword
            btnTogglePassword.Text = "";
            btnTogglePassword.Paint += (s, pe) =>
            {
                var g = pe.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(ThemeManager.ColorFieldBg);

                var btn = (Button)s!;
                Point ptMouse = btn.PointToClient(Cursor.Position);
                bool isHover = btn.ClientRectangle.Contains(ptMouse);
                Color iconColor = isHover ? Color.FromArgb(255, 152, 0) : Color.FromArgb(140, 140, 145);

                int cx = btn.Width / 2;
                int cy = btn.Height / 2;

                using (var pen = new Pen(iconColor, 1.8f))
                using (var brush = new SolidBrush(iconColor))
                {
                    g.DrawArc(pen, cx - 10, cy - 7, 20, 14, 200, 140);
                    g.DrawArc(pen, cx - 10, cy - 7, 20, 14, 20, 140);
                    g.FillEllipse(brush, cx - 3, cy - 3, 6, 6);

                    if (_isPasswordHidden)
                    {
                        using (var slashPen = new Pen(Color.FromArgb(239, 68, 68), 2f))
                        {
                            g.DrawLine(slashPen, cx - 8, cy + 6, cx + 8, cy - 6);
                        }
                    }
                }
            };
            btnTogglePassword.MouseEnter += (s, e) => btnTogglePassword.Invalidate();
            btnTogglePassword.MouseLeave += (s, e) => btnTogglePassword.Invalidate();

            // Center the card panel on the form
            pnlCard.Location = new Point(
                (this.ClientSize.Width - pnlCard.Width) / 2,
                (this.ClientSize.Height - pnlCard.Height) / 2
            );
        }

        private void TranslateUI()
        {
            // Apply RightToLeft layout dynamically for Arabic
            bool isRtl = LocalizationService.CurrentLanguage.ToLower() == "ar";
            this.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = isRtl;

            this.Text = LocalizationService.T("tray.tooltip") + " - " + LocalizationService.T("tabs.login");
            lblTitle.Text = LocalizationService.T("login.cloud_connection");
            btnBrowserLogin.Text = LocalizationService.T("login.browser_login");
            lblOrManual.Text = LocalizationService.T("login.or_manual");
            lblTokenLabel.Text = LocalizationService.T("login.device_token");
            txtToken.PlaceholderText = LocalizationService.T("login.device_token_placeholder");
            btnConnect.Text = LocalizationService.T("login.connect");
            
            lblStatus.Text = LocalizationService.T("login.offline");
        }

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            _isPasswordHidden = !_isPasswordHidden;
            txtToken.PasswordChar = _isPasswordHidden ? '●' : '\0';
            btnTogglePassword.Invalidate();
        }

        private void btnBrowserLogin_Click(object sender, EventArgs e)
        {
            try
            {
                // Stop any previous server instance
                if (_loginServer != null)
                {
                    _loginServer.Stop();
                    _loginServer = null;
                }

                _loginServer = new LocalLoginServer();
                _loginServer.TokenReceived += OnBrowserTokenReceived;
                int port = _loginServer.Start();

                // Construct auth URL exactly as Python does:
                // https://nlkmenu.com/admin/desktop/authorize?callback=http%3A//127.0.0.1%3A<port>/callback
                string callbackUrl = $"http://127.0.0.1:{port}/callback";
                string baseUrl  = ConfigManager.Current.App.ApiBaseUrl.TrimEnd('/');
                string panelUrl = baseUrl.Replace("api.", "").Replace("/api", "");
                string authUrl  = $"{panelUrl}/admin/desktop/authorize?callback={Uri.EscapeDataString(callbackUrl)}";

                // Open the browser FIRST so the callback server is already listening
                Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

                // Then notify the user (non-blocking info)
                MessageBox.Show(
                    LocalizationService.T("login.browser_waiting_msg"),
                    LocalizationService.T("login.browser_waiting_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Tarayıcı başlatılamadı: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void OnBrowserTokenReceived(string token)
        {
            if (this.IsDisposed) return;

            // The server already sent the response and stopped itself.
            // We only need to wire the token into the UI and verify it.
            this.Invoke(new Action(async () =>
            {
                txtToken.Text = token;
                _loginServer = null; // server manages its own shutdown

                // Auto connect with the received token
                await VerifyAndConnect(token);
            }));
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string token = txtToken.Text.Trim();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show(
                    LocalizationService.T("login.missing_token_msg"),
                    LocalizationService.T("login.missing_info_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            btnConnect.Enabled = false;
            btnConnect.Text = "...";
            
            await VerifyAndConnect(token);

            btnConnect.Enabled = true;
            btnConnect.Text = LocalizationService.T("login.connect");
        }

        private async Task VerifyAndConnect(string token)
        {
            string baseUrl = ConfigManager.Current.App.ApiBaseUrl.TrimEnd('/');
            if (!ConfigManager.IsSecureApiUrl(baseUrl))
            {
                MessageBox.Show("Sunucu adresi HTTPS olmalıdır.", "Güvenli Bağlantı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string verifyUrl = $"{baseUrl}/printer/verify-token";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(12);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    client.DefaultRequestHeaders.Add("User-Agent", "CheffiePOS-PrintBridge/1.0");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(verifyUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string body = await response.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(body))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                            {
                                var restData = root.GetProperty("restaurant");
                                string id = restData.GetProperty("id").GetRawText(); // Handle int or string
                                string slug = restData.GetProperty("slug").GetString() ?? "";
                                string name = restData.GetProperty("name").GetString() ?? "";
                                string address = restData.TryGetProperty("address", out var addr) ? addr.GetString() ?? "" : "";
                                string phone = restData.TryGetProperty("phone", out var ph) ? ph.GetString() ?? "" : "";

                                // Save to config
                                var app = ConfigManager.Current.App;
                                app.DeviceToken = token;
                                app.RestaurantId = id;
                                app.RestaurantSlug = slug;
                                app.RestaurantName = name;
                                app.RestaurantAddress = address;
                                app.RestaurantPhone = phone;

                                ConfigManager.Save();

                                MessageBox.Show(
                                    LocalizationService.T("login.success_msg"),
                                    LocalizationService.T("login.success_title"),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );

                                this.DialogResult = DialogResult.OK;
                                this.Close();
                                return;
                            }
                        }
                    }

                    // Handles specific HTTP errors
                    string errMsg = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => LocalizationService.T("login.error_auth"),
                        System.Net.HttpStatusCode.Forbidden => LocalizationService.T("login.error_auth"),
                        System.Net.HttpStatusCode.NotFound => LocalizationService.T("login.error_not_found"),
                        _ => LocalizationService.T("login.error_conn")
                    };
                    throw new Exception(errMsg);
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                if (ex is HttpRequestException)
                {
                    errMsg = LocalizationService.T("login.error_conn");
                }

                MessageBox.Show(
                    errMsg,
                    LocalizationService.T("login.error_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _loginServer?.Stop();
            LocalizationService.LanguageChanged -= OnLanguageChanged;
            base.OnFormClosed(e);
        }
    }
}
