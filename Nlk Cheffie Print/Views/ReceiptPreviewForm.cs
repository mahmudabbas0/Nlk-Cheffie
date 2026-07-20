using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Core.Net;
using Nlk_Cheffie_Print.Core.Printer;
using Nlk_Cheffie_Print.Core.Workers;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views
{
    public sealed class ReceiptPreviewForm : Form
    {
        private readonly Order _order;
        private readonly string _role;
        private JsonElement _data;
        private readonly SlipTemplate _template;

        // Custom Title Bar Dragging fields
        private bool _dragging;
        private Point _dragStart = Point.Empty;

        // Controls
        private Panel pnlTitleBar;
        private Label lblTitle;
        private Button btnCloseX;
        
        // Canvas Container
        private Panel pnlCanvas;
        
        // View 1: Detailed Text Panel
        private Panel pnlDetails = null!;
        private FlowLayoutPanel flowDetails = null!;
        
        // View 2: Visual Preview Controls
        private PictureBox picPreview;
        private FlatScrollBar scrollBar;
        private Bitmap? previewImage;
        private bool updatingPreviewLayout;
        
        // Footer & Action Buttons
        private Panel pnlFooter;
        private TableLayoutPanel tableFooter;
        private Button btnPrint;
        private Button btnToggle;
        private Button btnClose;

        public ReceiptPreviewForm(Order order)
        {
            _order = order;
            _role = ResolveRole(order.Section);
            _data = CreateReceiptData(order);
            _template = TemplateStore.Load(_role);

            // Form properties
            FormBorderStyle = FormBorderStyle.None;
            Text = $"Fiş Önizleme - {order.OrderNumber}";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 760);
            MinimumSize = new Size(420, 560);
            BackColor = ThemeManager.ColorBackground;
            ForeColor = ThemeManager.ColorText;

            // Apply RightToLeft layout dynamically for Arabic
            bool isRtl = LocalizationService.CurrentLanguage.ToLower() == "ar";
            this.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = isRtl;

            // 1. Custom Title Bar
            pnlTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = ThemeManager.ColorCard
            };
            pnlTitleBar.MouseDown += TitleBar_MouseDown;
            pnlTitleBar.MouseMove += TitleBar_MouseMove;
            pnlTitleBar.MouseUp += TitleBar_MouseUp;

            lblTitle = new Label
            {
                Text = $"{LocalizationService.T("orders.detail.dialog_title", "SİPARİŞ DETAYI")}  •  {order.OrderNumber}",
                Dock = DockStyle.Left,
                AutoSize = true,
                Padding = new Padding(16, 12, 0, 0),
                Font = ThemeManager.FontTitle,
                ForeColor = ThemeManager.ColorAccent
            };
            lblTitle.MouseDown += TitleBar_MouseDown;
            lblTitle.MouseMove += TitleBar_MouseMove;
            lblTitle.MouseUp += TitleBar_MouseUp;

            btnCloseX = new Button
            {
                Text = "✕",
                Dock = DockStyle.Right,
                Width = 44,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = ThemeManager.ColorTextMuted,
                BackColor = Color.Transparent
            };
            btnCloseX.FlatAppearance.BorderSize = 0;
            btnCloseX.FlatAppearance.MouseOverBackColor = ThemeManager.ColorDanger;
            btnCloseX.FlatAppearance.MouseDownBackColor = ThemeManager.ColorDanger;
            btnCloseX.MouseEnter += (s, e) => btnCloseX.ForeColor = Color.White;
            btnCloseX.MouseLeave += (s, e) => btnCloseX.ForeColor = ThemeManager.ColorTextMuted;
            btnCloseX.Click += (s, e) => Close();

            pnlTitleBar.Controls.Add(lblTitle);
            pnlTitleBar.Controls.Add(btnCloseX);

            // 2. Footer Panel
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 68,
                BackColor = ThemeManager.ColorCard,
                Padding = new Padding(16, 14, 16, 14)
            };

            tableFooter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            tableFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tableFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            tableFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

            btnPrint = new Button
            {
                Text = LocalizationService.T("orders.detail.reprint", "FİŞİ YAZDIR"),
                Tag = "Primary",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontBodyBold,
                BackColor = ThemeManager.ColorAccent,
                ForeColor = Color.Black,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += (s, e) => Reprint();

            btnToggle = new Button
            {
                Text = LocalizationService.T("orders.detail.show_preview", "FİŞ ÖNİZLEME"),
                Tag = "Secondary",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontBodyBold,
                BackColor = ThemeManager.ColorFieldBg,
                ForeColor = ThemeManager.ColorText,
                Margin = new Padding(8, 0, 8, 0)
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Click += (s, e) => ToggleView();

            btnClose = new Button
            {
                Text = LocalizationService.T("orders.detail.close", "KAPAT"),
                Tag = "Secondary",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontBodyBold,
                BackColor = ThemeManager.ColorFieldBg,
                ForeColor = ThemeManager.ColorText,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            tableFooter.Controls.Add(btnPrint, 0, 0);
            tableFooter.Controls.Add(btnToggle, 1, 0);
            tableFooter.Controls.Add(btnClose, 2, 0);
            pnlFooter.Controls.Add(tableFooter);

            // 3. Canvas panel for contents (with DoubleBuffering)
            pnlCanvas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.ColorBackground,
                Padding = new Padding(20)
            };
            pnlCanvas.MouseWheel += Canvas_MouseWheel;
            
            // Enable DoubleBuffer via reflection to prevent toggle flicker
            typeof(Panel).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(pnlCanvas, true);

            // Details View Setup
            BuildDetailsView();

            // Preview View Setup
            picPreview = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.White,
                Visible = false
            };
            picPreview.MouseWheel += Canvas_MouseWheel;

            scrollBar = new FlatScrollBar
            {
                Dock = DockStyle.Right,
                Width = 12,
                BackColor = ThemeManager.ColorFieldBg,
                Visible = false
            };
            scrollBar.Scroll += (s, e) =>
            {
                if (picPreview.Visible)
                {
                    LayoutPreviewImage();
                }
                else if (pnlDetails.Visible)
                {
                    flowDetails.Top = -scrollBar.Value;
                }
            };

            pnlCanvas.Controls.Add(scrollBar);
            pnlCanvas.Controls.Add(picPreview);
            pnlCanvas.Controls.Add(pnlDetails);

            Controls.Add(pnlCanvas);
            Controls.Add(pnlFooter);
            Controls.Add(pnlTitleBar);

            // Set styling recursively
            ThemeManager.ApplyTheme(this);

            LoadPreviewImage();

            // Hook canvas resize to make layout responsive
            pnlCanvas.Resize += Canvas_Resize;
            
            // Trigger initial layout
            Canvas_Resize(this, EventArgs.Empty);

            Shown += async (_, _) => await RefreshReceiptAsync();
        }

        private async Task RefreshReceiptAsync()
        {
            try
            {
                await ProductExtrasCatalog.EnsureLoadedAsync();
                OrderItemExtrasParser.RefreshOrderExtraNames(_order);
                _data = CreateReceiptData(_order);
                LoadPreviewImage();
            }
            catch
            {
                // Keep the initial receipt if refresh fails.
            }
        }

        private void Canvas_Resize(object? sender, EventArgs e)
        {
            int targetWidth = pnlCanvas.ClientSize.Width - 40 - (scrollBar.Visible ? scrollBar.Width : 0);
            if (targetWidth < 300) targetWidth = 300;

            flowDetails.SuspendLayout();
            flowDetails.Width = pnlDetails.ClientSize.Width - (scrollBar.Visible ? scrollBar.Width : 0);
            foreach (Control ctrl in flowDetails.Controls)
            {
                ctrl.Width = targetWidth;

                // Adjust child alignments inside the panels to prevent layout wrapping bugs
                if (ctrl is Panel pnl)
                {
                    foreach (Control sub in pnl.Controls)
                    {
                        if (sub is Label lbl)
                        {
                            if (lbl.TextAlign == ContentAlignment.TopRight)
                            {
                                lbl.Left = pnl.Width - lbl.Width - 12;
                            }
                            else if (lbl.Location.X >= 140) // Value label in CreateInfoRowPanel
                            {
                                lbl.Width = pnl.Width - lbl.Left - 12;
                            }
                            else if (lbl.Location.X >= 55) // Product name / Cust label in items
                            {
                                if (lbl.Font.Bold)
                                    lbl.Width = pnl.Width - lbl.Left - 120; // unit price space
                                else
                                    lbl.Width = pnl.Width - lbl.Left - 20;
                            }
                        }
                    }
                }
            }
            flowDetails.ResumeLayout(true);

            if (picPreview.Visible)
            {
                UpdateScrollRange();
            }
            else if (pnlDetails.Visible)
            {
                flowDetails.Left = (pnlDetails.ClientSize.Width - flowDetails.Width) / 2;
                UpdateDetailsScrollRange();
            }
        }

        private static bool IsEmptyValue(string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return true;
            string clean = val.Trim();
            return clean == "-" || clean == "/" || clean == "(/)" || clean == "()";
        }

        private static string TranslateStatus(string status) => status.ToLowerInvariant() switch
        {
            "pending"    => "⏳ Beklemede",
            "accepted"   => "✓ Onaylandı",
            "preparing"  => "🍳 Hazırlanıyor",
            "ready"      => "🔔 Hazır",
            "on_the_way" => "🛵 Yolda",
            "delivered"  => "✅ Teslim Edildi",
            "canceled"   => "• İptal Edildi",
            _            => status
        };

        private static string TranslatePaymentMethod(string method) => method.ToLowerInvariant() switch
        {
            "cash"   => "Nakit",
            "card"   => "Kart",
            "online" => "Online",
            _        => method
        };

        private static string TranslatePaymentStatus(string status) => status.ToLowerInvariant() switch
        {
            "pending" => "Bekliyor",
            "paid"    => "Ödendi",
            "waiting" => "Bekliyor",
            "error"   => "Hata",
            "refunded"=> "İade",
            _         => status
        };

        private static string TranslateSection(string section) => section.ToLowerInvariant() switch
        {
            "kitchen" => "Mutfak",
            "cashier" => "Kasa",
            "courier" => "Kurye",
            _         => section
        };

        private Panel CreateInfoRowPanel(string icon, string label, string value)
        {
            var pnl = new Panel
            {
                Width = 400,
                Height = 38,
                BackColor = ThemeManager.ColorCard,
                Margin = new Padding(0, 0, 0, 6),
                Tag = "Card"
            };

            pnl.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(40, Color.White), 1))
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
                }
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 10, FontStyle.Regular),
                Location = new Point(12, 10),
                Size = new Size(24, 20),
                BackColor = Color.Transparent
            };


            var lblValue = new Label
            {
                Text = value,
                Font = ThemeManager.FontBodyBold,
                ForeColor = ThemeManager.ColorText,
                Location = new Point(155, 10),
                Size = new Size(pnl.Width - 170, 20),
                BackColor = Color.Transparent,
                AutoEllipsis = true
            };

            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(new Label { Text = label, Font = ThemeManager.FontBodyBold, ForeColor = ThemeManager.ColorTextMuted, Location = new Point(40, 10), Size = new Size(110, 20), BackColor = Color.Transparent });
            pnl.Controls.Add(lblValue);

            pnl.Resize += (s, e) =>
            {
                lblValue.Width = pnl.Width - 170;
            };

            return pnl;
        }

        private static string GetItemDisplayPrice(OrderItem item)
        {
            if (item.AddedCustomizations.Count > 0 &&
                !string.IsNullOrWhiteSpace(item.UnitPrice) &&
                item.UnitPrice != "0.00")
            {
                return $"{item.UnitPrice} TL";
            }

            return string.IsNullOrEmpty(item.LineTotal) ? "TL" : $"{item.LineTotal} TL";
        }

        private void BuildDetailsView()
        {
            pnlDetails = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.ColorBackground,
                AutoScroll = false // Disable native un-themeable scrollbar
            };

            flowDetails = new FlowLayoutPanel
            {
                Dock = DockStyle.None, // Allow manual positioning for scrolling
                Location = new Point(0, 0),
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 12, 12, 12)
            };

            // 🏷️ SİPARİŞ #ORD-xxxx Title Block
            var pnlTitleBlock = new Panel
            {
                Width = 400,
                Height = 46,
                Margin = new Padding(0, 0, 0, 16)
            };
            var lblTitleIcon = new Label
            {
                Text = "🏷️",
                Font = new Font("Segoe UI Emoji", 18, FontStyle.Regular),
                Location = new Point(0, 4),
                Size = new Size(32, 38),
                BackColor = Color.Transparent
            };
            var lblTitleText = new Label
            {
                Text = $"{LocalizationService.T("orders.detail.order_header", "SİPARİŞ")} #{_order.OrderNumber}",
                Font = new Font(ThemeManager.FontTitle.FontFamily, 14, FontStyle.Bold),
                ForeColor = ThemeManager.ColorAccent,
                Location = new Point(36, 6),
                Size = new Size(360, 38),
                BackColor = Color.Transparent
            };
            pnlTitleBlock.Controls.Add(lblTitleIcon);
            pnlTitleBlock.Controls.Add(lblTitleText);
            flowDetails.Controls.Add(pnlTitleBlock);

            // Add fields dynamically (only if not empty)
            void AddRow(string icon, string labelKey, string defaultLabel, string value)
            {
                if (IsEmptyValue(value)) return;
                string label = LocalizationService.T(labelKey, defaultLabel);
                flowDetails.Controls.Add(CreateInfoRowPanel(icon, $"{label}:", value));
            }

            AddRow("🕒", "orders.detail.date", "Tarih", _order.DateTime.ToString("dd.MM.yyyy HH:mm"));
            AddRow("ℹ️", "orders.detail.section", "Bölüm", TranslateSection(_order.Section));
            AddRow("👤", "orders.detail.customer", "Müşteri", _order.CustomerName);
            AddRow("📞", "orders.detail.phone", "Telefon", _order.CustomerPhone);
            AddRow("✉️", "orders.detail.email", "E-posta", _order.CustomerEmail);
            AddRow("💳", "orders.detail.payment_type", "Ödeme Türü", TranslatePaymentMethod(_order.PaymentMethod));
            AddRow("✔️", "orders.detail.payment_status", "Ödeme Durumu", TranslatePaymentStatus(_order.PaymentStatus));
            AddRow("💲", "orders.detail.total", "Toplam", _order.TotalAmount.Replace(" TL", ""));
            AddRow("✔️", "orders.detail.status", "Durum", $"- {TranslateStatus(_order.Status)}");
            AddRow("📝", "orders.detail.note", "Sipariş Notu", _order.OrderNote);

            // ÜRÜNLER Title
            var lblItemsHeader = new Label
            {
                Text = LocalizationService.T("orders.detail.products", "ÜRÜNLER"),
                Font = new Font(ThemeManager.FontTitle.FontFamily, 12, FontStyle.Bold),
                ForeColor = ThemeManager.ColorAccent,
                AutoSize = true,
                Margin = new Padding(0, 16, 0, 8),
                BackColor = Color.Transparent
            };
            flowDetails.Controls.Add(lblItemsHeader);

            // Add Product items (with Left orange bar)
            foreach (var item in _order.Items)
            {
                var itemRow = new Panel
                {
                    Width = 400,
                    BackColor = ThemeManager.ColorCard,
                    Margin = new Padding(0, 0, 0, 8),
                    Padding = new Padding(12, 10, 12, 10),
                    Tag = "Card"
                };

                itemRow.Paint += (s, e) =>
                {
                    using (var brush = new SolidBrush(ThemeManager.ColorAccent))
                    {
                        e.Graphics.FillRectangle(brush, 0, 0, 4, itemRow.Height);
                    }
                    using (var pen = new Pen(Color.FromArgb(30, Color.White), 1))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, itemRow.Width - 1, itemRow.Height - 1);
                    }
                };

                var lblQty = new Label
                {
                    Text = $"{item.Quantity}x",
                    Font = ThemeManager.FontBodyBold,
                    ForeColor = ThemeManager.ColorAccent,
                    Location = new Point(16, 12),
                    Size = new Size(35, 20),
                    BackColor = Color.Transparent
                };

                var lblName = new Label
                {
                    Text = item.Name,
                    Font = ThemeManager.FontBodyBold,
                    ForeColor = ThemeManager.ColorText,
                    Location = new Point(55, 12),
                    Size = new Size(220, 20),
                    BackColor = Color.Transparent,
                    AutoEllipsis = true
                };

                var lblPrice = new Label
                {
                    Text = GetItemDisplayPrice(item),
                    Font = ThemeManager.FontBodyBold,
                    ForeColor = ThemeManager.ColorText,
                    Location = new Point(290, 12),
                    Size = new Size(100, 20),
                    TextAlign = ContentAlignment.TopRight,
                    BackColor = Color.Transparent
                };

                itemRow.Controls.Add(lblQty);
                itemRow.Controls.Add(lblName);
                itemRow.Controls.Add(lblPrice);

                int currentY = 32;
                if (item.AddedCustomizations.Count > 0)
                {
                    var lblCust = new Label
                    {
                        Text = $" + {LocalizationService.T("receipt.extra", "Extra")}: {string.Join(", ", item.AddedCustomizations)}",
                        Font = ThemeManager.FontSmall,
                        ForeColor = ThemeManager.ColorTextMuted,
                        Location = new Point(55, currentY),
                        Size = new Size(320, 16),
                        BackColor = Color.Transparent,
                        AutoEllipsis = true
                    };
                    itemRow.Controls.Add(lblCust);
                    currentY += 18;
                }

                if (item.RemovedCustomizations != null && item.RemovedCustomizations.Count > 0)
                {
                    var lblRem = new Label
                    {
                        Text = $" - {LocalizationService.T("receipt.remove", "Çıkart")}: {string.Join(", ", item.RemovedCustomizations)}",
                        Font = ThemeManager.FontSmall,
                        ForeColor = ThemeManager.ColorTextMuted,
                        Location = new Point(55, currentY),
                        Size = new Size(320, 16),
                        BackColor = Color.Transparent,
                        AutoEllipsis = true
                    };
                    itemRow.Controls.Add(lblRem);
                    currentY += 18;
                }

                if (!IsEmptyValue(item.Notes))
                {
                    var lblNote = new Label
                    {
                        Text = $" * {LocalizationService.T("receipt.note_prefix", "Not")}: {item.Notes}",
                        Font = ThemeManager.FontSmall,
                        ForeColor = ThemeManager.ColorAccent,
                        Location = new Point(55, currentY),
                        Size = new Size(320, 16),
                        BackColor = Color.Transparent,
                        AutoEllipsis = true
                    };
                    itemRow.Controls.Add(lblNote);
                    currentY += 18;
                }

                itemRow.Height = Math.Max(44, currentY + 8);
                flowDetails.Controls.Add(itemRow);
            }

            // 💲 FİYAT ÖZETİ Title
            var lblTotalsHeader = new Label
            {
                Text = LocalizationService.T("orders.detail.pricing_header", "FİYAT ÖZETİ"),
                Font = new Font(ThemeManager.FontTitle.FontFamily, 12, FontStyle.Bold),
                ForeColor = ThemeManager.ColorAccent,
                AutoSize = true,
                Margin = new Padding(0, 16, 0, 8),
                BackColor = Color.Transparent
            };
            flowDetails.Controls.Add(lblTotalsHeader);

            // Container panel for totals
            var totalsPanel = new Panel
            {
                Width = 400,
                Height = 110,
                BackColor = ThemeManager.ColorCard,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(12, 10, 12, 10),
                Tag = "Card"
            };
            totalsPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(30, Color.White), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, totalsPanel.Width - 1, totalsPanel.Height - 1);
                }
            };

            void AddDetailTotalRow(string label, string val, int y, bool isBold = false)
            {
                // Sanitize template tags and colons from keys (e.g. "Ara Toplam: {ara_toplam}" -> "Ara Toplam")
                string cleanLabel = label.Replace("{ara_toplam}", "").Replace("{kdv_toplam}", "").Replace("{toplam_tutar}", "").Replace("{extras_total}", "").Replace("{ekstra_toplam}", "").Trim(' ', ':');

                var lblL = new Label
                {
                    Text = cleanLabel,
                    Font = isBold ? ThemeManager.FontBodyBold : ThemeManager.FontBody,
                    ForeColor = isBold ? ThemeManager.ColorText : ThemeManager.ColorTextMuted,
                    Location = new Point(16, y),
                    Size = new Size(180, 20),
                    BackColor = Color.Transparent
                };
                var lblR = new Label
                {
                    Text = val + " TL",
                    Font = isBold ? ThemeManager.FontBodyBold : ThemeManager.FontBody,
                    ForeColor = isBold ? ThemeManager.ColorAccent : ThemeManager.ColorText,
                    Location = new Point(200, y),
                    Size = new Size(188, 20),
                    TextAlign = ContentAlignment.TopRight,
                    BackColor = Color.Transparent
                };
                totalsPanel.Controls.Add(lblL);
                totalsPanel.Controls.Add(lblR);
            }

            AddDetailTotalRow(LocalizationService.T("receipt.subtotal", "Ara Toplam"), _order.Subtotal, 12);
            AddDetailTotalRow(LocalizationService.T("receipt.extras_total", "Ekstra Toplam"), _order.ExtrasTotal, 36);
            AddDetailTotalRow(LocalizationService.T("receipt.tax", "KDV"), _order.Tax, 60);
            AddDetailTotalRow(LocalizationService.T("receipt.total", "Genel Toplam"), _order.TotalAmount.Replace(" TL", ""), 84, true);

            flowDetails.Controls.Add(totalsPanel);

            pnlDetails.Controls.Add(flowDetails);

            // Recursively hook MouseWheel scroll events so that scrolling works from anywhere inside
            void HookControlMouseWheel(Control ctrl)
            {
                ctrl.MouseWheel += Canvas_MouseWheel;
                foreach (Control child in ctrl.Controls)
                {
                    HookControlMouseWheel(child);
                }
            }
            HookControlMouseWheel(pnlDetails);
        }

        private void ToggleView()
        {
            pnlCanvas.SuspendLayout();
            if (pnlDetails.Visible)
            {
                // Show visual receipt preview
                pnlDetails.Visible = false;
                picPreview.Visible = true;
                
                btnToggle.Text = LocalizationService.T("orders.detail.show_details", "DETAYLARI GÖSTER");
                
                UpdateScrollRange();
            }
            else
            {
                // Show text details card
                picPreview.Visible = false;
                pnlDetails.Visible = true;
                
                btnToggle.Text = LocalizationService.T("orders.detail.show_preview", "FİŞ ÖNİZLEME");

                flowDetails.Top = 0;
                UpdateDetailsScrollRange();
            }
            pnlCanvas.ResumeLayout(true);
            
            // Trigger responsive width update
            Canvas_Resize(this, EventArgs.Empty);
        }

        private void LoadPreviewImage()
        {
            try
            {
                previewImage = ReceiptRenderer.RenderToBitmap(_template, _data);
                picPreview.Image = previewImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Önizleme yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateScrollRange()
        {
            if (previewImage == null || updatingPreviewLayout) return;

            updatingPreviewLayout = true;
            try
            {
                LayoutPreviewImage();
                int visibleHeight = pnlCanvas.Height - pnlCanvas.Padding.Top - pnlCanvas.Padding.Bottom;
                int totalHeight = picPreview.Height;
                int maxScroll = totalHeight - visibleHeight;

                bool shouldBeVisible = maxScroll > 0;
                if (scrollBar.Visible != shouldBeVisible)
                {
                    scrollBar.Visible = shouldBeVisible;
                    if (shouldBeVisible)
                    {
                        scrollBar.BringToFront();
                    }
                    LayoutPreviewImage();
                    totalHeight = picPreview.Height;
                    maxScroll = totalHeight - visibleHeight;
                }

                if (shouldBeVisible)
                {
                    scrollBar.Minimum = 0;
                    scrollBar.Maximum = totalHeight;
                    scrollBar.LargeChange = visibleHeight;
                    scrollBar.Value = Math.Min(scrollBar.Value, Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1));
                    LayoutPreviewImage();
                }
                else
                {
                    picPreview.Top = pnlCanvas.Padding.Top + Math.Max(0, (visibleHeight - totalHeight) / 2);
                }
            }
            finally
            {
                updatingPreviewLayout = false;
            }
        }

        private void LayoutPreviewImage()
        {
            if (previewImage == null) return;

            int availableWidth = pnlCanvas.ClientSize.Width - pnlCanvas.Padding.Left - pnlCanvas.Padding.Right;
            if (scrollBar.Visible) availableWidth -= scrollBar.Width;
            availableWidth = Math.Max(1, availableWidth);

            int width = Math.Min(previewImage.Width, availableWidth);
            int height = Math.Max(1, (int)Math.Round(previewImage.Height * (width / (double)previewImage.Width)));
            picPreview.Size = new Size(width, height);
            picPreview.Left = pnlCanvas.Padding.Left + Math.Max(0, (availableWidth - width) / 2);
            picPreview.Top = pnlCanvas.Padding.Top - (scrollBar.Visible ? scrollBar.Value : 0);
        }

        private void UpdateDetailsScrollRange()
        {
            flowDetails.PerformLayout();
            pnlDetails.PerformLayout();

            int visibleHeight = pnlDetails.ClientSize.Height;
            
            // Calculate total content height manually to bypass WinForms layout latency/AutoSize bugs
            int totalHeight = flowDetails.Padding.Top + flowDetails.Padding.Bottom;
            foreach (Control ctrl in flowDetails.Controls)
            {
                totalHeight += ctrl.Height + ctrl.Margin.Top + ctrl.Margin.Bottom;
            }

            int maxScroll = totalHeight - visibleHeight;

            bool shouldBeVisible = maxScroll > 0;
            if (scrollBar.Visible != shouldBeVisible)
            {
                scrollBar.Visible = shouldBeVisible;
                if (shouldBeVisible)
                {
                    scrollBar.BringToFront();
                }
                Canvas_Resize(this, EventArgs.Empty);
                return;
            }

            if (shouldBeVisible)
            {
                scrollBar.Minimum = 0;
                scrollBar.Maximum = totalHeight;
                scrollBar.LargeChange = visibleHeight;
            }
            else
            {
                flowDetails.Top = 0;
            }
        }

        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (!scrollBar.Visible) return;
            int val = scrollBar.Value - (e.Delta / 120) * 40;
            
            // Limit to valid scroll ranges
            int maxVal = scrollBar.Maximum - scrollBar.LargeChange + 1;
            if (val < 0) val = 0;
            if (val > maxVal) val = maxVal;

            scrollBar.Value = val;
            if (picPreview.Visible)
            {
                LayoutPreviewImage();
            }
            else if (pnlDetails.Visible)
            {
                flowDetails.Top = -val;
            }
        }

        // --- Mouse drag handlers for borderless title bar ---
        private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _dragStart = e.Location;
            }
        }

        private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point screenPoint = PointToScreen(e.Location);
                Location = new Point(screenPoint.X - _dragStart.X, screenPoint.Y - _dragStart.Y);
            }
        }

        private void TitleBar_MouseUp(object? sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(ThemeManager.ColorBorder, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                previewImage?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Reprint()
        {
            PrintQueueWorker.Enqueue(new PrintJob
            {
                Role = _role,
                Data = _data.Clone(),
                JobId = $"reprint-{_order.Id}-{Guid.NewGuid():N}",
                ForcePrint = true,
                SkipOrderLog = true
            });
            MessageBox.Show(
                LocalizationService.T("orders.detail.reprint_success_msg", "Fiş yeniden yazdırma kuyruğuna alındı."), 
                LocalizationService.T("orders.detail.reprint_success_title", "Yazıcı Kuyruğu"), 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information
            );
        }

        private static string ResolveRole(string section)
        {
            string value = section.ToLowerInvariant();
            if (value.Contains("kasa") || value.Contains("cashier")) return "cashier";
            if (value.Contains("kurye") || value.Contains("courier")) return "courier";
            return "kitchen";
        }

        private static JsonElement CreateReceiptData(Order order)
        {
            var payload = new
            {
                restaurant_info = new 
                { 
                    name = ConfigManager.Current.App.RestaurantName, 
                    address = ConfigManager.Current.App.RestaurantAddress, 
                    phone = ConfigManager.Current.App.RestaurantPhone 
                },
                order_info = new 
                { 
                    order_number = order.OrderNumber, 
                    table_name = order.TableName, 
                    waiter_name = order.WaiterName, 
                    date = order.DateTime.ToString("dd.MM.yyyy"), 
                    time = order.DateTime.ToString("HH:mm"), 
                    status = order.Status, 
                    payment_method = order.PaymentMethod, 
                    customer_name = order.CustomerName, 
                    customer_phone = order.CustomerPhone, 
                    delivery_address = order.DeliveryAddress, 
                    order_note = order.OrderNote 
                },
                payment_info = new 
                { 
                    subtotal = order.Subtotal, 
                    tax = order.Tax, 
                    extras_total = order.ExtrasTotal,
                    total = order.TotalAmount.Replace(" TL", string.Empty).Trim()
                },
                items = order.Items.ConvertAll(item => new 
                { 
                    product_id = item.ProductId,
                    quantity = item.Quantity, 
                    name = item.Name,
                    base_price = item.UnitPrice,
                    line_total = item.LineTotal, 
                    notes = item.Notes ?? "",
                    customizations = new
                    {
                        added = item.AddedCustomizations.ToArray(),
                        removed = item.RemovedCustomizations.ToArray()
                    }
                })
            };

            using var document = JsonDocument.Parse(JsonSerializer.Serialize(payload));
            return document.RootElement.Clone();
        }
    }
}
