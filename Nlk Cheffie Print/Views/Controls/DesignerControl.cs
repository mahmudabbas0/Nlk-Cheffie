using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views.Controls
{
    public partial class DesignerControl : UserControl
    {
        private string _activeTemplateId = "kitchen";
        private Dictionary<string, SlipTemplate> _templates = new Dictionary<string, SlipTemplate>();
        private ListBox? _activeListBox;

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

            // Handle resizing events to keep receipt centered and eliminate ghosting trails
            pnlMiddleCanvas.Resize += (s, ev) =>
            {
                CenterReceiptRoll();
                pnlMiddleCanvas.Invalidate();
            };
            pnlReceiptRoll.Resize += (s, ev) => pnlReceiptRoll.Invalidate();

            SetupTemplateSelector();
            LoadDefaultTemplates();
            LoadTemplate(_activeTemplateId);
            TranslateUI();

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
            pnlReceiptRoll.Invalidate();
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
            _templates["kitchen"] = GetDefaultTemplate("kitchen");
            _templates["cashier"] = GetDefaultTemplate("cashier");
            _templates["courier"] = GetDefaultTemplate("courier");
        }

        private SlipTemplate GetDefaultTemplate(string role)
        {
            var template = new SlipTemplate();

            // Header Elements
            template.Header.Add(new TemplateElement { Type = "text", Content = "{restoran_adi}", Font = "B", Size = "2x", Align = "center" });
            
            string titleKey = role switch
            {
                "kitchen" => "receipt.kitchen_title",
                "cashier" => "receipt.cashier_title",
                "courier" => "receipt.courier_title",
                _ => "receipt.order_slip"
            };
            template.Header.Add(new TemplateElement { Type = "text", Content = "{" + titleKey + "}", Font = "B", Size = "2x", Align = "center" });
            template.Header.Add(new TemplateElement { Type = "separator" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "{restoran_adres}", Font = "A", Size = "1x", Align = "left" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "Tel: {restoran_phone}", Font = "A", Size = "1x", Align = "left" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "Table: {table_name}", Font = "A", Size = "1x", Align = "left" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "Order No: {order_no}", Font = "B", Size = "1x", Align = "right" });
            template.Header.Add(new TemplateElement { Type = "text", Content = "Date: {date}  Time: {time}", Font = "A", Size = "1x", Align = "left" });
            template.Header.Add(new TemplateElement { Type = "separator" });

            // Body Elements
            template.Body.Add(new TemplateElement { Type = "items", Align = "left" });

            // Footer Elements
            template.Footer.Add(new TemplateElement { Type = "separator" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Subtotal: {subtotal}", Font = "A", Size = "1x", Align = "left" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Tax: {tax_total}", Font = "A", Size = "1x", Align = "left" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Grand Total: {grand_total}", Font = "B", Size = "1x", Align = "left" });
            
            if (role == "courier")
            {
                template.Footer.Add(new TemplateElement { Type = "text", Content = "Delivery Address: {delivery_address}", Font = "A", Size = "1x", Align = "left" });
                template.Footer.Add(new TemplateElement { Type = "text", Content = "Customer: {customer_name} ({customer_phone})", Font = "A", Size = "1x", Align = "left" });
                template.Footer.Add(new TemplateElement { Type = "text", Content = "Payment: {payment_type}", Font = "A", Size = "1x", Align = "left" });
                template.Footer.Add(new TemplateElement { Type = "text", Content = "Note: {note}", Font = "A", Size = "1x", Align = "left" });
            }
            else
            {
                template.Footer.Add(new TemplateElement { Type = "text", Content = "Payment: {payment_type}", Font = "A", Size = "1x", Align = "left" });
            }

            template.Footer.Add(new TemplateElement { Type = "separator" });
            template.Footer.Add(new TemplateElement { Type = "qrcode", Content = "{payment_link}", Align = "center" });
            template.Footer.Add(new TemplateElement { Type = "text", Content = "Powered by Cheffie POS", Font = "A", Size = "1x", Align = "center" });

            return template;
        }

        private void LoadTemplate(string templateId)
        {
            _activeTemplateId = templateId;
            if (!_templates.TryGetValue(templateId, out var template))
            {
                template = GetDefaultTemplate(templateId);
                _templates[templateId] = template;
            }

            PopulateSectionList(lstHeader, template.Header);
            PopulateSectionList(lstBody, template.Body);
            PopulateSectionList(lstFooter, template.Footer);

            pnlReceiptRoll.Invalidate();
        }

        private void PopulateSectionList(ListBox listBox, List<TemplateElement> list)
        {
            listBox.Items.Clear();
            foreach (var el in list)
            {
                listBox.Items.Add(GetElementLabel(el));
            }
        }

        private string GetElementLabel(TemplateElement el)
        {
            string typeLabel = el.Type.ToUpper() switch
            {
                "TEXT" => LocalizationService.T("designer.items.text", "Text"),
                "SEPARATOR" => LocalizationService.T("designer.items.separator", "Separator"),
                "QRCODE" => LocalizationService.T("designer.items.qrcode", "QR Code"),
                "BARCODE" => LocalizationService.T("designer.items.barcode", "Barcode"),
                "LOGO" => LocalizationService.T("designer.items.logo", "Logo"),
                "ITEMS" => LocalizationService.T("designer.items.list", "Product List"),
                _ => el.Type
            };

            if (el.Type == "text" || el.Type == "qrcode" || el.Type == "barcode")
            {
                return $"[{typeLabel}] {el.Content}";
            }
            if (el.Type == "logo")
            {
                return $"[{typeLabel}] {Path.GetFileName(el.Path)}";
            }
            return $"[{typeLabel}]";
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

        private void cmbTemplate_SelectedIndexChanged(object sender, EventArgs e)
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

            // Draw Drop Shadow
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
            {
                g.FillRectangle(shadowBrush, new Rectangle(4, 4, w - 8, h - 8));
            }

            // Draw Paper background
            Color paperBg = Color.FromArgb(252, 252, 250);
            using (Brush paperBrush = new SolidBrush(paperBg))
            {
                // Draw paper roll polygon with zig-zag borders at top/bottom
                Point[] pts = GetReceiptPaperPolygon(w, h);
                g.FillPolygon(paperBrush, pts);
            }

            // Draw Receipt Content Preview (Simulated GDI+ rendering of template)
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
            while (x < w - 2)
            {
                pts.Add(new Point(x + zigzagSize / 2, 2 + zigzagSize));
                pts.Add(new Point(Math.Min(x + zigzagSize, w - 2), 2));
                x += zigzagSize;
            }

            pts.Add(new Point(w - 2, 2));
            pts.Add(new Point(w - 2, h - 2));

            // Bottom zigzag (right to left)
            x = w - 2;
            while (x > 2)
            {
                pts.Add(new Point(x - zigzagSize / 2, h - 2 - zigzagSize));
                pts.Add(new Point(Math.Max(x - zigzagSize, 2), h - 2));
                x -= zigzagSize;
            }

            pts.Add(new Point(2, h - 2));
            return pts.ToArray();
        }

        private int DrawElementPreview(Graphics g, TemplateElement el, int paperWidth, int yOffset, Font fnNormal, Font fnBold, Font fnHeader, Brush brush)
        {
            if (yOffset > pnlReceiptRoll.Height - 30) return yOffset;

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
                string qrText = el.Content;
                // Substitute placeholders for preview
                qrText = qrText.Replace("{restoran_adi}", ConfigManager.Current.App.RestaurantName)
                               .Replace("{restoran_adres}", ConfigManager.Current.App.RestaurantAddress)
                               .Replace("{restoran_phone}", ConfigManager.Current.App.RestaurantPhone)
                               .Replace("{table_name}", "Table Garden-1")
                               .Replace("{order_no}", "S-001")
                               .Replace("{date}", DateTime.Now.ToString("dd.MM.yyyy"))
                               .Replace("{time}", DateTime.Now.ToString("HH:mm"))
                               .Replace("{subtotal}", "100.00 TL")
                               .Replace("{tax_total}", "10.00 TL")
                               .Replace("{grand_total}", "110.00 TL")
                               .Replace("{payment_type}", "Cash")
                               .Replace("{payment_link}", "https://nlkmenu.com")
                               .Replace("{odeme_linki}", "https://nlkmenu.com");

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
                // Draw mock barcode stripes
                int barW = 100;
                int barH = 30;
                int x = (paperWidth - barW) / 2;
                for (int i = 0; i < barW; i += 4)
                {
                    int w = (i % 3 == 0) ? 1 : 2;
                    g.FillRectangle(Brushes.Black, x + i, yOffset, w, barH);
                }
                return yOffset + barH + 12;
            }

            if (el.Type == "logo")
            {
                // Draw mock logo box
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
            string text = el.Content;
            
            // Perform basic template substitutions for preview
            text = text.Replace("{restoran_adi}", ConfigManager.Current.App.RestaurantName)
                       .Replace("{restoran_adres}", ConfigManager.Current.App.RestaurantAddress)
                       .Replace("{restoran_phone}", ConfigManager.Current.App.RestaurantPhone)
                       .Replace("{table_name}", "Table Garden-1")
                       .Replace("{order_no}", "S-001")
                       .Replace("{date}", DateTime.Now.ToString("dd.MM.yyyy"))
                       .Replace("{time}", DateTime.Now.ToString("HH:mm"))
                       .Replace("{subtotal}", "100.00 TL")
                       .Replace("{tax_total}", "10.00 TL")
                       .Replace("{grand_total}", "110.00 TL")
                       .Replace("{payment_type}", "Cash");

            // Look up translation keys for texts like {receipt.kitchen_title}
            if (text.Contains("{receipt."))
            {
                string innerKey = text.Trim('{', '}');
                text = LocalizationService.T(innerKey, text);
            }

            Font fnUse = el.Font == "B" ? fnBold : fnNormal;
            if (el.Size == "2x") fnUse = fnHeader;

            SizeF size = g.MeasureString(text, fnUse);
            float drawX = margin;

            if (el.Align == "center")
            {
                drawX = (paperWidth - size.Width) / 2;
            }
            else if (el.Align == "right")
            {
                drawX = paperWidth - margin - size.Width;
            }

            g.DrawString(text, fnUse, brush, drawX, yOffset);

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
                        pnlReceiptRoll.Invalidate();
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
                    pnlReceiptRoll.Invalidate();
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
                pnlReceiptRoll.Invalidate();
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
                _templates[_activeTemplateId] = GetDefaultTemplate(_activeTemplateId);
                LoadTemplate(_activeTemplateId);
            }
        }

        private void btnSaveDesign_Click(object sender, EventArgs e)
        {
            // simulated save success
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
            pnlReceiptRoll.Left = (pnlMiddleCanvas.Width - paperWidth) / 2;
            pnlReceiptRoll.Top = 45;
            pnlReceiptRoll.Height = Math.Max(100, pnlMiddleCanvas.Height - 75); // stays top/bottom padded
            pnlReceiptRoll.ResumeLayout();
        }
    }
}
