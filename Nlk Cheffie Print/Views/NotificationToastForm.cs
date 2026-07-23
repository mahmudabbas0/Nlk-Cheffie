using System;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Views
{
    public class NotificationToastForm : Form
    {
        private System.Windows.Forms.Timer _closeTimer;
        private Label _lblTitle;
        private Label _lblDetails;
        private Label _lblTotal;
        private Button _btnView;
        private Button _btnClose;
        private Action? _onViewClicked;

        public NotificationToastForm(JsonElement orderData, Action? onViewClicked = null)
        {
            _onViewClicked = onViewClicked;
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Size = new Size(370, 125);
            this.BackColor = Color.FromArgb(24, 24, 26);
            this.DoubleBuffered = true;

            // Custom border paint with left accent strip and dark background
            this.Paint += (s, e) =>
            {
                using (var p = new Pen(Color.FromArgb(255, 152, 0), 2))
                {
                    e.Graphics.DrawRectangle(p, 1, 1, Width - 2, Height - 2);
                }
                using (var pLeft = new Pen(Color.FromArgb(255, 152, 0), 6))
                {
                    e.Graphics.DrawLine(pLeft, 3, 3, 3, Height - 3);
                }
            };

            // Title - Fully Localized with English default fallback
            _lblTitle = new Label
            {
                Text = "🔔 " + LocalizationService.T("notifications.new_order_title", "New Order Received!"),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 152, 0),
                Location = new Point(16, 12),
                AutoSize = true
            };

            // Parse order details
            string orderNo = GetOrderProp(orderData, "order_number", "id", "-");
            string table = GetOrderProp(orderData, "table_name", "table", "-");
            string total = GetOrderProp(orderData, "total_amount", "total", "0.00");

            // Details line
            _lblDetails = new Label
            {
                Text = $"{LocalizationService.T("orders.detail.order_no", "Order No")}: #{orderNo}  •  {LocalizationService.T("orders.detail.table", "Table")}: {table}",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(220, 220, 220),
                Location = new Point(16, 38),
                Size = new Size(335, 20)
            };

            // Total amount line
            _lblTotal = new Label
            {
                Text = $"{LocalizationService.T("designer.vars.grand_total", "Grand total")}: {total} TL",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 62),
                AutoSize = true
            };

            // Modern View Button - Generous width (115px), smooth orange hover
            _btnView = new Button
            {
                Text = LocalizationService.T("notifications.view", "View Order"),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(115, 30),
                Location = new Point(240, 80),
                Cursor = Cursors.Hand
            };
            _btnView.FlatAppearance.BorderSize = 0;
            _btnView.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 175, 45);
            _btnView.FlatAppearance.MouseDownBackColor = Color.FromArgb(230, 135, 0);
            _btnView.Click += (s, e) =>
            {
                _onViewClicked?.Invoke();
                this.Close();
            };

            // Close X Button
            _btnClose = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 160, 160),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(26, 26),
                Location = new Point(338, 6),
                Cursor = Cursors.Hand
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 45, 48);
            _btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(_lblTitle);
            this.Controls.Add(_lblDetails);
            this.Controls.Add(_lblTotal);
            this.Controls.Add(_btnView);
            this.Controls.Add(_btnClose);

            // Auto close timer (6 seconds)
            _closeTimer = new System.Windows.Forms.Timer { Interval = 6000 };
            _closeTimer.Tick += (s, e) =>
            {
                _closeTimer.Stop();
                this.Close();
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            PositionAtBottomRight();
            _closeTimer.Start();
        }

        private void PositionAtBottomRight()
        {
            Rectangle workingArea = Screen.PrimaryScreen?.WorkingArea ?? SystemInformation.WorkingArea;
            int x = workingArea.Right - this.Width - 16;
            int y = workingArea.Bottom - this.Height - 16;
            this.Location = new Point(x, y);
        }

        private string GetOrderProp(JsonElement el, string key1, string key2, string fallback)
        {
            if (el.ValueKind == JsonValueKind.Object)
            {
                if (el.TryGetProperty(key1, out var p1) && p1.ValueKind != JsonValueKind.Null)
                    return p1.ToString();
                if (el.TryGetProperty(key2, out var p2) && p2.ValueKind != JsonValueKind.Null)
                    return p2.ToString();
            }
            return fallback;
        }

        public static void ShowToast(JsonElement orderData, Action? onViewClicked = null)
        {
            try
            {
                var toast = new NotificationToastForm(orderData, onViewClicked);
                toast.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show toast: {ex.Message}");
            }
        }
    }
}
