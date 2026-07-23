using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Core.Printer;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views.Controls
{
    public partial class DesignerControl : UserControl
    {
        private string _activeTemplateId = "kitchen";
        private Dictionary<string, SlipTemplate> _templates = new Dictionary<string, SlipTemplate>();
        private FlatListBox? _activeListBox;
        private FlatScrollBar scrollBar = null!;

        public DesignerControl()
        {
            InitializeComponent();
            LocalizationService.LanguageChanged += TranslateUI;
            this.DoubleBuffered = true;
        }

        private void DesignerControl_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
            pnlLeftSidebar.BackColor = ThemeManager.ColorCard;
            pnlRightSidebar.BackColor = ThemeManager.ColorCard;
            
            lstHeader.BackColor = ThemeManager.ColorFieldBg;
            lstHeader.ForeColor = ThemeManager.ColorText;
            lstBody.BackColor = ThemeManager.ColorFieldBg;
            lstBody.ForeColor = ThemeManager.ColorText;
            lstFooter.BackColor = ThemeManager.ColorFieldBg;
            lstFooter.ForeColor = ThemeManager.ColorText;

            _activeListBox = lstHeader;

            // Remove designer anchors to allow manual horizontal centering
            pnlReceiptRoll.Anchor = AnchorStyles.None;

            // Setup scrollbar dynamically
            scrollBar = new FlatScrollBar
            {
                Dock = DockStyle.Right,
                Width = 12,
                BackColor = ThemeManager.ColorFieldBg,
                Visible = false
            };
            scrollBar.Scroll += (s, ev) =>
            {
                pnlReceiptRoll.Top = 45 - scrollBar.Value;
            };
            pnlMiddleCanvas.Controls.Add(scrollBar);
            pnlMiddleCanvas.AutoScroll = false;
            pnlMiddleCanvas.MouseWheel += Canvas_MouseWheel;

            // Handle resizing events to keep receipt centered and eliminate ghosting trails
            pnlMiddleCanvas.Resize += (s, ev) =>
            {
                CenterReceiptRoll();
                pnlMiddleCanvas.Invalidate();
            };
            pnlReceiptRoll.Resize += (s, ev) => pnlReceiptRoll.Invalidate();

            SetupTemplateSelector();
            LoadDefaultTemplates();
            LoadSavedTemplates();
            LoadTemplate(_activeTemplateId);
            TranslateUI();

            // Hook up Drag and Drop reordering events
            lstHeader.ItemReordered += (oldIdx, newIdx) => ReorderElement("header", oldIdx, newIdx);
            lstBody.ItemReordered += (oldIdx, newIdx) => ReorderElement("body", oldIdx, newIdx);
            lstFooter.ItemReordered += (oldIdx, newIdx) => ReorderElement("footer", oldIdx, newIdx);

            // Run initial centering
            CenterReceiptRoll();
        }

        public void TranslateUI()
        {
            lblLeftTitle.Text = LocalizationService.T("designer.title").ToUpper();
            lblHeaderSection.Text = LocalizationService.T("designer.header_section");
            lblBodySection.Text = LocalizationService.T("designer.body_section");
            lblFooterSection.Text = LocalizationService.T("designer.footer_section");

            btnAddHeader.Text = LocalizationService.T("designer.add_header");
            btnAddBody.Text = LocalizationService.T("designer.add_body");
            btnAddFooter.Text = LocalizationService.T("designer.add_footer");

            lblTemplateSelect.Text = LocalizationService.T("designer.template_selection");
            lblItemActions.Text = LocalizationService.T("designer.item_actions");

            btnEditItem.Text = LocalizationService.T("designer.edit_item");
            btnDeleteItem.Text = LocalizationService.T("designer.delete_item");
            btnResetTemplate.Text = LocalizationService.T("designer.reset_template");
            btnSaveDesign.Text = LocalizationService.T("designer.save_design");

            lblCanvasTitle.Text = LocalizationService.T("designer.preview");

            // Update combo items without triggering event
            cmbTemplate.SelectedIndexChanged -= cmbTemplate_SelectedIndexChanged;
            int selectedIdx = cmbTemplate.SelectedIndex;
            cmbTemplate.Items.Clear();
            cmbTemplate.Items.Add(LocalizationService.T("designer.template_kitchen"));
            cmbTemplate.Items.Add(LocalizationService.T("designer.template_cashier"));
            cmbTemplate.Items.Add(LocalizationService.T("designer.template_courier"));
            cmbTemplate.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;
            cmbTemplate.SelectedIndexChanged += cmbTemplate_SelectedIndexChanged;

            // Refresh text descriptions in lists
            RefreshActiveTemplateLists();
            CenterReceiptRoll();
        }

        private void SetupTemplateSelector()
        {
            cmbTemplate.Items.Clear();
            cmbTemplate.Items.Add(LocalizationService.T("designer.template_kitchen"));
            cmbTemplate.Items.Add(LocalizationService.T("designer.template_cashier"));
            cmbTemplate.Items.Add(LocalizationService.T("designer.template_courier"));
            cmbTemplate.SelectedIndex = 0;
        }

        private void LoadDefaultTemplates()
        {
            _templates["kitchen"] = TemplateStore.GetDefaultTemplate("kitchen");
            _templates["cashier"] = TemplateStore.GetDefaultTemplate("cashier");
            _templates["courier"] = TemplateStore.GetDefaultTemplate("courier");
        }

        private void LoadSavedTemplates()
        {
            foreach (string role in new[] { "kitchen", "cashier", "courier" })
            {
                _templates[role] = TemplateStore.Load(role);
            }
        }

        private void LoadTemplate(string templateId)
        {
            _activeTemplateId = templateId;
            if (!_templates.TryGetValue(templateId, out var template))
            {
                template = TemplateStore.GetDefaultTemplate(templateId);
                _templates[templateId] = template;
            }

            PopulateSectionList(lstHeader, template.Header);
            PopulateSectionList(lstBody, template.Body);
            PopulateSectionList(lstFooter, template.Footer);

            CenterReceiptRoll();
        }

        private void PopulateSectionList(FlatListBox listBox, List<TemplateElement> list)
        {
            listBox.Items.Clear();
            foreach (var el in list)
            {
                listBox.Items.Add(GetElementLabel(el));
            }
            listBox.RefreshItemsLayout();
        }

        private string GetElementLabel(TemplateElement el)
        {
            string type = el?.Type ?? "";
            string typeLabel = type.ToUpperInvariant() switch
            {
                "TEXT" => LocalizationService.T("designer.items.text", "Metin"),
                "SEPARATOR" => LocalizationService.T("designer.items.separator", "Ayraç"),
                "QRCODE" => LocalizationService.T("designer.items.qrcode", "QR Kod"),
                "BARCODE" => LocalizationService.T("designer.items.barcode", "Barkod"),
                "LOGO" => LocalizationService.T("designer.items.logo", "Logo"),
                "ITEMS" => LocalizationService.T("designer.items.list", "Ürün Listesi"),
                _ => type
            };

            if (type == "text" || type == "qrcode" || type == "barcode")
            {
                string friendlyContent = GetFriendlyContentDisplay(el?.Content ?? "");
                return $"[{typeLabel}] {friendlyContent}";
            }
            if (type == "logo")
            {
                string logoPath = el?.Path ?? "";
                string filename = string.IsNullOrEmpty(logoPath) ? LocalizationService.T("designer.labels.no_file", "(Dosya Seçilmedi)") : Path.GetFileName(logoPath);
                return $"[{typeLabel}] {filename}";
            }
            return $"[{typeLabel}]";
        }

        private string GetFriendlyContentDisplay(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";

            string friendly = content;

            // 1. First replace UI label tokens ({L_...})
            var labelReplacements = new Dictionary<string, string>
            {
                { "{L_customer_info}", LocalizationService.T("receipt.customer_info", "Müşteri Bilgileri") },
                { "{L_customer_name}", LocalizationService.T("designer.vars.customer_name", "Müşteri Adı") },
                { "{L_customer_phone}", LocalizationService.T("designer.vars.customer_phone", "Müşteri Tel") },
                { "{L_delivery_address}", LocalizationService.T("designer.vars.delivery_address", "Teslimat Adresi") },
                { "{L_adres}", LocalizationService.T("orders.detail.address", "Adres") },
                { "{L_tel}", LocalizationService.T("orders.detail.phone", "Tel") },
                { "{L_masa}", LocalizationService.T("orders.detail.table", "Masa") },
                { "{L_siparis_no}", LocalizationService.T("orders.detail.order_no", "Sipariş No") },
                { "{L_tarih}", LocalizationService.T("orders.columns.date", "Tarih") },
                { "{L_saat}", LocalizationService.T("designer.vars.time", "Saat") },
                { "{L_ara_toplam}", LocalizationService.T("designer.vars.subtotal", "Ara Toplam") },
                { "{L_ekstra_toplam}", LocalizationService.T("designer.vars.extra_total", "Ekstra Toplam") },
                { "{L_kdv}", LocalizationService.T("designer.vars.tax_total", "KDV") },
                { "{L_total}", LocalizationService.T("designer.vars.grand_total", "Genel Toplam") },
                { "{L_afiyet_olsun}", LocalizationService.T("receipt.enjoy", "Afiyet Olsun!") }
            };

            foreach (var kvp in labelReplacements)
            {
                friendly = friendly.Replace(kvp.Key, kvp.Value);
            }

            // 2. Replace Dynamic Variable Tokens
            var varReplacements = new Dictionary<string, string>
            {
                { "{restoran_adi}", "[" + LocalizationService.T("designer.vars.restaurant_name", "Restoran Adı") + "]" },
                { "{restoran_name}", "[" + LocalizationService.T("designer.vars.restaurant_name", "Restoran Adı") + "]" },
                { "{restoran_adres}", "[" + LocalizationService.T("designer.vars.restaurant_address", "Restoran Adresi") + "]" },
                { "{restoran_address}", "[" + LocalizationService.T("designer.vars.restaurant_address", "Restoran Adresi") + "]" },
                { "{restoran_telefon}", "[" + LocalizationService.T("designer.vars.restaurant_phone", "Restoran Tel") + "]" },
                { "{restoran_phone}", "[" + LocalizationService.T("designer.vars.restaurant_phone", "Restoran Tel") + "]" },
                { "{order_no}", "[" + LocalizationService.T("designer.vars.order_no", "Sipariş No") + "]" },
                { "{order_number}", "[" + LocalizationService.T("designer.vars.order_no", "Sipariş No") + "]" },
                { "{siparis_no}", "[" + LocalizationService.T("designer.vars.order_no", "Sipariş No") + "]" },
                { "{table_name}", "[" + LocalizationService.T("designer.vars.table_name", "Masa Adı") + "]" },
                { "{masa_adi}", "[" + LocalizationService.T("designer.vars.table_name", "Masa Adı") + "]" },
                { "{date}", "[" + LocalizationService.T("designer.vars.date", "Tarih") + "]" },
                { "{tarih}", "[" + LocalizationService.T("designer.vars.date", "Tarih") + "]" },
                { "{time}", "[" + LocalizationService.T("designer.vars.time", "Saat") + "]" },
                { "{saat}", "[" + LocalizationService.T("designer.vars.time", "Saat") + "]" },
                { "{customer_name}", "[" + LocalizationService.T("designer.vars.customer_name", "Müşteri Adı") + "]" },
                { "{musteri_adi}", "[" + LocalizationService.T("designer.vars.customer_name", "Müşteri Adı") + "]" },
                { "{customer_phone}", "[" + LocalizationService.T("designer.vars.customer_phone", "Müşteri Tel") + "]" },
                { "{musteri_telefon}", "[" + LocalizationService.T("designer.vars.customer_phone", "Müşteri Tel") + "]" },
                { "{delivery_address}", "[" + LocalizationService.T("designer.vars.delivery_address", "Teslimat Adresi") + "]" },
                { "{teslimat_adresi}", "[" + LocalizationService.T("designer.vars.delivery_address", "Teslimat Adresi") + "]" },
                { "{payment_type}", "[" + LocalizationService.T("designer.vars.payment_type", "Ödeme Tipi") + "]" },
                { "{odeme_tipi}", "[" + LocalizationService.T("designer.vars.payment_type", "Ödeme Tipi") + "]" },
                { "{note}", "[" + LocalizationService.T("designer.vars.note", "Sipariş Notu") + "]" },
                { "{ek_not}", "[" + LocalizationService.T("designer.vars.note", "Sipariş Notu") + "]" },
                { "{subtotal}", "[" + LocalizationService.T("designer.vars.subtotal", "Ara Toplam") + "]" },
                { "{ara_toplam}", "[" + LocalizationService.T("designer.vars.subtotal", "Ara Toplam") + "]" },
                { "{tax_total}", "[" + LocalizationService.T("designer.vars.tax_total", "KDV Toplamı") + "]" },
                { "{kdv_toplam}", "[" + LocalizationService.T("designer.vars.tax_total", "KDV Toplamı") + "]" },
                { "{grand_total}", "[" + LocalizationService.T("designer.vars.grand_total", "Genel Toplam") + "]" },
                { "{toplam_tutar}", "[" + LocalizationService.T("designer.vars.grand_total", "Genel Toplam") + "]" },
                { "{ekstra_toplam}", "[" + LocalizationService.T("designer.vars.extra_total", "Ekstra Toplam") + "]" },
                { "{extra_total}", "[" + LocalizationService.T("designer.vars.extra_total", "Ekstra Toplam") + "]" }
            };

            foreach (var kvp in varReplacements)
            {
                friendly = friendly.Replace(kvp.Key, kvp.Value);
            }

            return friendly;
        }

        private void RefreshActiveTemplateLists()
        {
            if (_templates.TryGetValue(_activeTemplateId, out var template))
            {
                PopulateSectionList(lstHeader, template.Header);
                PopulateSectionList(lstBody, template.Body);
                PopulateSectionList(lstFooter, template.Footer);
            }
        }

        private void cmbTemplate_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string id = cmbTemplate.SelectedIndex switch
            {
                0 => "kitchen",
                1 => "cashier",
                2 => "courier",
                _ => "kitchen"
            };
            LoadTemplate(id);
        }

        private void pnlMiddleCanvas_Paint(object sender, PaintEventArgs e)
        {
            // Draw grid dots
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Color dotColor = Color.FromArgb(20, 255, 255, 255);
            using (Brush brush = new SolidBrush(dotColor))
            {
                int spacing = 20;
                float radius = 1.5f;

                for (int x = 0; x < pnlMiddleCanvas.Width; x += spacing)
                {
                    for (int y = 0; y < pnlMiddleCanvas.Height; y += spacing)
                    {
                        g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
                    }
                }
            }
        }

        private void pnlReceiptRoll_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int w = pnlReceiptRoll.Width;
            int h = pnlReceiptRoll.Height;

            // Draw Drop Shadow Polygon (follows the zig-zag exactly)
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(35, 0, 0, 0)))
            {
                Point[] shadowPts = GetReceiptPaperPolygon(w, h);
                for (int i = 0; i < shadowPts.Length; i++)
                {
                    shadowPts[i].X += 4;
                    shadowPts[i].Y += 4;
                }
                g.FillPolygon(shadowBrush, shadowPts);
            }

            // Draw Paper background
            Color paperBg = Color.FromArgb(252, 252, 250);
            using (Brush paperBrush = new SolidBrush(paperBg))
            {
                Point[] pts = GetReceiptPaperPolygon(w, h);
                g.FillPolygon(paperBrush, pts);
            }

            // Draw Paper Border (gives depth to paper edges)
            using (Pen borderPen = new Pen(Color.FromArgb(210, 210, 205), 1))
            {
                Point[] pts = GetReceiptPaperPolygon(w, h);
                g.DrawPolygon(borderPen, pts);
            }

            if (!_templates.TryGetValue(_activeTemplateId, out var template)) return;

            int yOffset = 30;
            using (Font fnNormal = new Font("Courier New", 8, FontStyle.Regular))
            using (Font fnBold = new Font("Courier New", 8, FontStyle.Bold))
            using (Font fnHeader = new Font("Courier New", 10, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                // 1. Draw Header
                foreach (var el in template.Header)
                {
                    yOffset = DrawElementPreview(g, el, w, yOffset, fnNormal, fnBold, fnHeader, textBrush);
                }

                // 2. Draw Body
                foreach (var el in template.Body)
                {
                    yOffset = DrawElementPreview(g, el, w, yOffset, fnNormal, fnBold, fnHeader, textBrush);
                }

                // 3. Draw Footer
                foreach (var el in template.Footer)
                {
                    yOffset = DrawElementPreview(g, el, w, yOffset, fnNormal, fnBold, fnHeader, textBrush);
                }
            }
        }



        private Point[] GetReceiptPaperPolygon(int w, int h)
        {
            var pts = new List<Point>();
            int zigzagSize = 6;
            int x = 2;

            // Top zigzag (left to right)
            pts.Add(new Point(2, 2));
            while (x < w - 4)
            {
                pts.Add(new Point(x + zigzagSize / 2, 2 + zigzagSize));
                pts.Add(new Point(Math.Min(x + zigzagSize, w - 4), 2));
                x += zigzagSize;
            }
            pts.Add(new Point(w - 4, 2));
            pts.Add(new Point(w - 4, h - 2));

            // Bottom zigzag (right to left)
            x = w - 4;
            while (x > 2)
            {
                pts.Add(new Point(x - zigzagSize / 2, h - 2 - zigzagSize));
                pts.Add(new Point(Math.Max(x - zigzagSize, 2), h - 2));
                x -= zigzagSize;
            }
            pts.Add(new Point(2, h - 2));
            pts.Add(new Point(2, 2));

            return pts.ToArray();
        }

        private int DrawElementPreview(Graphics g, TemplateElement el, int paperWidth, int yOffset, Font fnNormal, Font fnBold, Font fnHeader, Brush brush)
        {
            if (yOffset > pnlReceiptRoll.Height + 50) return yOffset;

            int margin = 16;
            int usableWidth = paperWidth - (margin * 2);

            if (el.Type == "separator")
            {
                using (Pen dashPen = new Pen(Color.Gray, 1))
                {
                    dashPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(dashPen, margin, yOffset + 4, paperWidth - margin, yOffset + 4);
                }
                return yOffset + 12;
            }

            if (el.Type == "qrcode")
            {
                string qrText = SubstituteForPreview(el.Content);

                if (string.IsNullOrEmpty(qrText)) qrText = "https://nlkmenu.com";

                try
                {
                    using (var qrGenerator = new QRCoder.QRCodeGenerator())
                    using (var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCoder.QRCodeGenerator.ECCLevel.Q))
                    using (var qrCode = new QRCoder.QRCode(qrCodeData))
                    {
                        using (Bitmap qrBmp = qrCode.GetGraphic(2)) // scale=2 is perfect for the 300px preview canvas
                        {
                            int x = (paperWidth - qrBmp.Width) / 2;
                            g.DrawImage(qrBmp, x, yOffset);
                            return yOffset + qrBmp.Height + 12;
                        }
                    }
                }
                catch
                {
                    int qrSize = 50;
                    int x = (paperWidth - qrSize) / 2;
                    g.DrawRectangle(Pens.Black, x, yOffset, qrSize, qrSize);
                    return yOffset + qrSize + 12;
                }
            }

            if (el.Type == "barcode")
            {
                string content = SubstituteForPreview(el.Content);
                if (string.IsNullOrWhiteSpace(content)) content = "ORD-12345";

                try
                {
                    using Bitmap barcode = ReceiptRenderer.RenderCode128Barcode(content);
                    int width = Math.Min(usableWidth, 260);
                    int height = 48;
                    int x = (paperWidth - width) / 2;
                    g.DrawImage(barcode, x, yOffset, width, height);

                    using var format = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(content, fnNormal, brush,
                        new RectangleF(margin, yOffset + height + 2, usableWidth, fnNormal.GetHeight() + 2), format);
                    return yOffset + height + (int)fnNormal.GetHeight() + 8;
                }
                catch
                {
                    g.DrawString(content, fnNormal, brush, margin, yOffset);
                    return yOffset + (int)fnNormal.GetHeight() + 4;
                }
            }

            if (el.Type == "logo")
            {
                string path = el.Path;
                if (!string.IsNullOrEmpty(path))
                {
                    string resolvedPath = ResolveLogoPath(path);
                    if (File.Exists(resolvedPath))
                    {
                        try
                        {
                            using (var img = Image.FromFile(resolvedPath))
                            {
                                int w = Math.Min(img.Width, 120);
                                int h = (int)(img.Height * ((double)w / img.Width));
                                int xImg = (paperWidth - w) / 2;
                                g.DrawImage(img, xImg, yOffset, w, h);
                                return yOffset + h + 12;
                            }
                        }
                        catch
                        {
                            // Fall through to mock box
                        }
                    }
                }

                // Draw mock logo box if file doesn't exist
                int logoW = 70;
                int logoH = 30;
                int x = (paperWidth - logoW) / 2;
                g.DrawRectangle(Pens.Gray, x, yOffset, logoW, logoH);
                g.DrawString("[LOGO]", fnNormal, Brushes.Gray, x + 15, yOffset + 8);
                return yOffset + logoH + 12;
            }

            if (el.Type == "items")
            {
                // Render sample mock product listing
                string item1 = "1x Hamburger         80.00 TL";
                string item1Ext = "  + Cheese";
                string item1Note = "  * Note: Medium-rare";
                string item2 = "2x Cola              30.00 TL";

                g.DrawString(item1, fnBold, brush, margin, yOffset);
                yOffset += 12;
                g.DrawString(item1Ext, fnNormal, Brushes.Gray, margin, yOffset);
                yOffset += 12;
                g.DrawString(item1Note, fnNormal, Brushes.Gray, margin, yOffset);
                yOffset += 12;
                g.DrawString(item2, fnNormal, brush, margin, yOffset);
                
                return yOffset + 20;
            }

            // Standard Text rendering
            string text = SubstituteForPreview(el.Content);

            // Look up translation keys for texts like {receipt.kitchen_title}
            if (text.Contains("{receipt."))
            {
                string innerKey = text.Trim('{', '}');
                text = LocalizationService.T(innerKey, text);
            }

            Font fnUse = el.Font == "B" ? fnBold : fnNormal;
            if (el.Size == "2x") fnUse = fnHeader;

            SizeF size = g.MeasureString(text, fnUse, usableWidth);
            float drawX = margin;

            if (el.Align == "center")
            {
                drawX = margin + (usableWidth - size.Width) / 2;
            }
            else if (el.Align == "right")
            {
                drawX = margin + usableWidth - size.Width;
            }

            g.DrawString(text, fnUse, brush, new RectangleF(drawX, yOffset, usableWidth, size.Height + 5));

            return yOffset + (int)size.Height + 2;
        }

        private void lstHeader_SelectedIndexChanged(object sender, EventArgs e)
        {
            _activeListBox = lstHeader;
            lstBody.ClearSelected();
            lstFooter.ClearSelected();
        }

        private void lstBody_SelectedIndexChanged(object sender, EventArgs e)
        {
            _activeListBox = lstBody;
            lstHeader.ClearSelected();
            lstFooter.ClearSelected();
        }

        private void lstFooter_SelectedIndexChanged(object sender, EventArgs e)
        {
            _activeListBox = lstFooter;
            lstHeader.ClearSelected();
            lstBody.ClearSelected();
        }

        private void btnAddHeader_Click(object sender, EventArgs e)
        {
            AddItemToSection("header");
        }

        private void btnAddBody_Click(object sender, EventArgs e)
        {
            AddItemToSection("body");
        }

        private void btnAddFooter_Click(object sender, EventArgs e)
        {
            AddItemToSection("footer");
        }

        private void AddItemToSection(string section)
        {
            using (var dlg = new AddItemForm())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var el = dlg.CreatedElement;
                    if (el != null)
                    {
                        var template = _templates[_activeTemplateId];
                        if (section == "header") template.Header.Add(el);
                        else if (section == "body") template.Body.Add(el);
                        else if (section == "footer") template.Footer.Add(el);

                        RefreshActiveTemplateLists();
                        CenterReceiptRoll();
                    }
                }
            }
        }

        private void btnEditItem_Click(object sender, EventArgs e)
        {
            EditSelectedRow();
        }

        private void lstHeader_DoubleClick(object sender, EventArgs e) => EditSelectedRow();
        private void lstBody_DoubleClick(object sender, EventArgs e) => EditSelectedRow();
        private void lstFooter_DoubleClick(object sender, EventArgs e) => EditSelectedRow();

        private void EditSelectedRow()
        {
            if (_activeListBox == null || _activeListBox.SelectedIndex < 0) return;

            int index = _activeListBox.SelectedIndex;
            var template = _templates[_activeTemplateId];
            TemplateElement? el = null;

            if (_activeListBox == lstHeader) el = template.Header[index];
            else if (_activeListBox == lstBody) el = template.Body[index];
            else if (_activeListBox == lstFooter) el = template.Footer[index];

            if (el == null) return;

            using (var dlg = new EditItemForm(el))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Values already modified inside dialog directly
                    RefreshActiveTemplateLists();
                    CenterReceiptRoll();
                }
            }
        }

        private void btnDeleteItem_Click(object sender, EventArgs e)
        {
            if (_activeListBox == null || _activeListBox.SelectedIndex < 0) return;

            var confirmResult = MessageBox.Show(
                LocalizationService.T("designer.dialogs.delete_confirm_msg"),
                LocalizationService.T("designer.dialogs.delete_confirm_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult == DialogResult.Yes)
            {
                int index = _activeListBox.SelectedIndex;
                var template = _templates[_activeTemplateId];

                if (_activeListBox == lstHeader) template.Header.RemoveAt(index);
                else if (_activeListBox == lstBody) template.Body.RemoveAt(index);
                else if (_activeListBox == lstFooter) template.Footer.RemoveAt(index);

                RefreshActiveTemplateLists();
                CenterReceiptRoll();
            }
        }

        private void btnResetTemplate_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
                LocalizationService.T("designer.dialogs.reset_confirm_msg").Replace("{role}", _activeTemplateId.ToUpper()),
                LocalizationService.T("designer.dialogs.reset_confirm_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmResult == DialogResult.Yes)
            {
                foreach (string role in new[] { "kitchen", "cashier", "courier" })
                {
                    _templates[role] = TemplateStore.GetDefaultTemplate(role);
                    TemplateStore.Save(role, _templates[role]);
                }
                LoadTemplate(_activeTemplateId);
            }
        }

        private void btnSaveDesign_Click(object sender, EventArgs e)
        {
            TemplateStore.Save(_activeTemplateId, _templates[_activeTemplateId]);
            MessageBox.Show(
                LocalizationService.T("designer.dialogs.save_success_msg"),
                LocalizationService.T("designer.dialogs.save_success_title"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LocalizationService.LanguageChanged -= TranslateUI;
            base.OnHandleDestroyed(e);
        }

        private void CenterReceiptRoll()
        {
            const int paperWidth = 300; // Fixed realistic width for 80mm receipt paper roll preview
            pnlReceiptRoll.SuspendLayout();
            pnlReceiptRoll.Width = paperWidth;
            pnlReceiptRoll.Left = Math.Max(10, (pnlMiddleCanvas.ClientSize.Width - (scrollBar != null && scrollBar.Visible ? scrollBar.Width : 0) - paperWidth) / 2);
            
            if (scrollBar != null)
            {
                int visibleHeight = pnlMiddleCanvas.Height;
                int totalHeight = CalculateReceiptHeight() + 90; // height + top/bottom padding
                int maxScroll = totalHeight - visibleHeight;

                if (maxScroll > 0)
                {
                    scrollBar.Minimum = 0;
                    scrollBar.Maximum = totalHeight;
                    scrollBar.LargeChange = visibleHeight;
                    scrollBar.Visible = true;
                }
                else
                {
                    scrollBar.Visible = false;
                    scrollBar.Value = 0;
                }

                pnlReceiptRoll.Height = totalHeight - 90;
                pnlReceiptRoll.Top = 45 - scrollBar.Value;
            }
            else
            {
                pnlReceiptRoll.Top = 45;
                pnlReceiptRoll.Height = CalculateReceiptHeight();
            }

            pnlReceiptRoll.ResumeLayout();
            pnlReceiptRoll.Invalidate(); // Force immediate redraw of template elements
        }

        private int CalculateReceiptHeight()
        {
            if (!_templates.TryGetValue(_activeTemplateId, out var template)) return 150;

            const int paperWidth = 300;
            int yOffset = 30; // initial top padding

            using (Graphics g = this.CreateGraphics())
            using (Font fnNormal = new Font("Courier New", 8, FontStyle.Regular))
            using (Font fnBold = new Font("Courier New", 8, FontStyle.Bold))
            using (Font fnHeader = new Font("Courier New", 10, FontStyle.Bold))
            {
                var allElements = new List<TemplateElement>();
                allElements.AddRange(template.Header);
                allElements.AddRange(template.Body);
                allElements.AddRange(template.Footer);

                foreach (var el in allElements)
                {
                    if (el.Type == "separator")
                    {
                        yOffset += 12;
                    }
                    else if (el.Type == "qrcode")
                    {
                        yOffset += 78;
                    }
                    else if (el.Type == "barcode")
                    {
                        yOffset += 70;
                    }
                    else if (el.Type == "logo")
                    {
                        yOffset += 42;
                    }
                    else if (el.Type == "items")
                    {
                        yOffset += 68;
                    }
                    else
                    {
                        string text = SubstituteForPreview(el.Content);

                        if (text.Contains("{receipt."))
                        {
                            string innerKey = text.Trim('{', '}');
                            text = LocalizationService.T(innerKey, text);
                        }

                        Font fnUse = el.Font == "B" ? fnBold : fnNormal;
                        if (el.Size == "2x") fnUse = fnHeader;

                        int margin = 16;
                        int usableWidth = paperWidth - (margin * 2);
                        SizeF size = g.MeasureString(text, fnUse, usableWidth);
                        yOffset += (int)size.Height + 2;
                    }
                }
            }

            return yOffset + 30; // 30px bottom padding
        }

        private string SubstituteForPreview(string content)
        {
            var ctx = ReceiptRenderer.GetMockContext();
            return ReceiptRenderer.Substitute(content, ctx);
        }

        private void ReorderElement(string section, int oldIdx, int newIdx)
        {
            var template = _templates[_activeTemplateId];
            List<TemplateElement>? list = null;
            if (section == "header") list = template.Header;
            else if (section == "body") list = template.Body;
            else if (section == "footer") list = template.Footer;

            if (list != null && oldIdx >= 0 && oldIdx < list.Count && newIdx >= 0 && newIdx < list.Count)
            {
                var el = list[oldIdx];
                list.RemoveAt(oldIdx);
                list.Insert(newIdx, el);

                // Refresh layout and preview
                CenterReceiptRoll();
            }
        }

        private void Canvas_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (scrollBar == null || !scrollBar.Visible) return;
            int val = scrollBar.Value - (e.Delta / 120) * 40;
            int maxVal = scrollBar.Maximum - scrollBar.LargeChange + 1;
            if (val < 0) val = 0;
            if (val > maxVal) val = maxVal;

            scrollBar.Value = val;
            pnlReceiptRoll.Top = 45 - val;
        }

        private static string ResolveLogoPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path) && File.Exists(path)) return path;

            // Try local AppData profile/config directories
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string slug = ConfigManager.Current.App.RestaurantSlug;
            
            if (!string.IsNullOrWhiteSpace(slug))
            {
                string profilePath = Path.Combine(appData, "nlkCheffiePrint", "profiles", slug, path);
                if (File.Exists(profilePath)) return profilePath;
            }

            string defaultConfigPath = Path.Combine(appData, "nlkCheffiePrint", "config", path);
            if (File.Exists(defaultConfigPath)) return defaultConfigPath;

            string baseConfigPath = Path.Combine(appData, "nlkCheffiePrint", path);
            if (File.Exists(baseConfigPath)) return baseConfigPath;

            string curPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            if (File.Exists(curPath)) return curPath;

            return path;
        }
    }
}
