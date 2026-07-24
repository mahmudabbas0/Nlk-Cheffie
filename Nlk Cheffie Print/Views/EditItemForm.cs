using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views
{
    public partial class EditItemForm : Form
    {
        private TemplateElement _element;
        private PictureBox? picLogoPreview;
        private Label? lblLogoInfo;

        public EditItemForm(TemplateElement element)
        {
            InitializeComponent();
            _element = element;
        }

        private void EditItemForm_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
            TranslateUI();
            
            pnlItemsSettings.BackColor = ThemeManager.ColorCard;
            
            SetupLogoPreviewControls();
            PopulateDropdowns();
            LoadElementData();
            AdjustVisiblePanels();
        }

        private void SetupLogoPreviewControls()
        {
            picLogoPreview = new PictureBox
            {
                Location = new Point(20, 160),
                Size = new Size(580, 290),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(32, 32, 36),
                Visible = false
            };

            lblLogoInfo = new Label
            {
                Location = new Point(20, 460),
                Size = new Size(580, 24),
                Font = ThemeManager.FontSmall,
                ForeColor = ThemeManager.ColorTextMuted,
                Visible = false
            };

            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.BackColor = ThemeManager.ColorAccent;
            btnBrowse.ForeColor = Color.Black;
            btnBrowse.Font = ThemeManager.FontBodyBold;
            btnBrowse.Cursor = Cursors.Hand;
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.FlatAppearance.MouseOverBackColor = ThemeManager.ColorAccentHover;
            btnBrowse.FlatAppearance.MouseDownBackColor = ThemeManager.ColorAccentPressed;

            btnRemoveLogo.FlatStyle = FlatStyle.Flat;
            btnRemoveLogo.BackColor = ThemeManager.ColorDanger;
            btnRemoveLogo.ForeColor = Color.White;
            btnRemoveLogo.Font = ThemeManager.FontBodyBold;
            btnRemoveLogo.Cursor = Cursors.Hand;
            btnRemoveLogo.FlatAppearance.BorderSize = 0;
            btnRemoveLogo.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 113, 113);
            btnRemoveLogo.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 38, 38);

            this.Controls.Add(picLogoPreview);
            this.Controls.Add(lblLogoInfo);

            txtContent.TextChanged += (s, e) =>
            {
                if (_element.Type == "logo")
                    UpdateLogoPreview(txtContent.Text);
            };
        }

        private void UpdateLogoPreview(string path)
        {
            if (_element.Type != "logo" || picLogoPreview == null || lblLogoInfo == null) return;

            string trimmedPath = (path ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(trimmedPath) && File.Exists(trimmedPath))
            {
                try
                {
                    var fileInfo = new FileInfo(trimmedPath);
                    long maxSizeBytes = 2 * 1024 * 1024; // 2MB
                    if (fileInfo.Length > maxSizeBytes)
                    {
                        picLogoPreview.Image?.Dispose();
                        picLogoPreview.Image = null;
                        double sizeMb = (double)fileInfo.Length / (1024 * 1024);
                        string errFormat = LocalizationService.T("designer.labels.logo_size_error", "⚠️ Dosya boyutu 2MB sınırını aşıyor ({size:F1} MB). Lütfen daha küçük bir resim seçiniz.");
                        lblLogoInfo.Text = errFormat.Replace("{size:F1}", sizeMb.ToString("F1"));
                        lblLogoInfo.ForeColor = Color.FromArgb(231, 76, 60);
                        return;
                    }

                    using (var stream = new FileStream(trimmedPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var tempImg = Image.FromStream(stream))
                        {
                            picLogoPreview.Image?.Dispose();
                            picLogoPreview.Image = new Bitmap(tempImg);
                            
                            double ratio = (double)tempImg.Width / tempImg.Height;
                            string loadedFormat = LocalizationService.T("designer.labels.logo_loaded", "✓ Logo Yüklendi: {w} × {h} px  (En/Boy Oranı Korunuyor: {ratio})");
                            lblLogoInfo.Text = loadedFormat.Replace("{w}", tempImg.Width.ToString())
                                                            .Replace("{h}", tempImg.Height.ToString())
                                                            .Replace("{ratio}", ratio.ToString("F2"));
                            lblLogoInfo.ForeColor = Color.FromArgb(46, 204, 113);
                            return;
                        }
                    }
                }
                catch
                {
                    // Exception reading image file
                }
            }

            picLogoPreview.Image?.Dispose();
            picLogoPreview.Image = null;
            if (string.IsNullOrWhiteSpace(trimmedPath))
            {
                lblLogoInfo.Text = LocalizationService.T("designer.labels.logo_select_msg", "Lütfen basılacak logo resim dosyasını (.png, .jpg) seçiniz.");
                lblLogoInfo.ForeColor = ThemeManager.ColorTextMuted;
            }
            else
            {
                lblLogoInfo.Text = LocalizationService.T("designer.labels.logo_not_found", "⚠️ Dosya bulunamadı veya resim formatı geçersiz.");
                lblLogoInfo.ForeColor = Color.FromArgb(231, 76, 60);
            }
        }

        private void TranslateUI()
        {
            this.Text = LocalizationService.T("designer.dialogs.edit_title");
            
            lblContent.Text = _element.Type == "logo" 
                ? LocalizationService.T("designer.dialogs.path") 
                : LocalizationService.T("designer.dialogs.content");
                
            lblAlign.Text = LocalizationService.T("designer.dialogs.align", "Alignment:");
            lblSize.Text = LocalizationService.T("designer.dialogs.size", "Size:");
            lblTransform.Text = LocalizationService.T("designer.dialogs.text_case", "Text Case:");
            lblWeight.Text = LocalizationService.T("designer.dialogs.weight", "Thickness:");
            lblFamily.Text = LocalizationService.T("designer.dialogs.family", "Font Family:");
            
            lblShowPrice.Text = LocalizationService.T("designer.dialogs.show_price");
            lblRightAlignPrice.Text = LocalizationService.T("designer.dialogs.right_align_price");
            lblCurrencySymbol.Text = LocalizationService.T("designer.dialogs.currency_symbol");
            lblShowDetails.Text = LocalizationService.T("designer.dialogs.show_customizations");
            lblShowNotes.Text = LocalizationService.T("designer.dialogs.show_notes");
            lblShowTax.Text = LocalizationService.T("designer.dialogs.show_tax");
            
            lblTokensTitle.Text = LocalizationService.T("designer.dialogs.add_var");
            btnBrowse.Text = LocalizationService.T("designer.dialogs.browse");
            btnRemoveLogo.Text = LocalizationService.T("designer.dialogs.remove_logo", " Görseli Kaldır");
            
            btnSave.Text = LocalizationService.T("designer.dialogs.save");
            btnCancel.Text = LocalizationService.T("settings.cancel");
        }

        private void PopulateDropdowns()
        {
            // Alignment dropdown
            cmbAlign.Items.Clear();
            cmbAlign.DisplayMember = "Value";
            cmbAlign.ValueMember = "Key";
            cmbAlign.Items.Add(new KeyValuePair<string, string>("left", LocalizationService.T("designer.align.left", "Left")));
            cmbAlign.Items.Add(new KeyValuePair<string, string>("center", LocalizationService.T("designer.align.center", "Center")));
            cmbAlign.Items.Add(new KeyValuePair<string, string>("right", LocalizationService.T("designer.align.right", "Right")));

            // Size dropdown
            cmbSize.Items.Clear();
            cmbSize.DisplayMember = "Value";
            cmbSize.ValueMember = "Key";
            cmbSize.Items.Add(new KeyValuePair<string, string>("1x", LocalizationService.T("designer.sizes.normal", "Normal")));
            cmbSize.Items.Add(new KeyValuePair<string, string>("1.5x", LocalizationService.T("designer.sizes.medium", "Medium")));
            cmbSize.Items.Add(new KeyValuePair<string, string>("2x", LocalizationService.T("designer.sizes.large", "Large")));
            cmbSize.Items.Add(new KeyValuePair<string, string>("3x", LocalizationService.T("designer.sizes.xlarge", "Extra Large")));

            // Text Transform / Case dropdown
            cmbTransform.Items.Clear();
            cmbTransform.DisplayMember = "Value";
            cmbTransform.ValueMember = "Key";
            cmbTransform.Items.Add(new KeyValuePair<string, string>("default", LocalizationService.T("designer.transforms.default", "Normal")));
            cmbTransform.Items.Add(new KeyValuePair<string, string>("uppercase", LocalizationService.T("designer.transforms.uppercase", "UPPERCASE")));
            cmbTransform.Items.Add(new KeyValuePair<string, string>("lowercase", LocalizationService.T("designer.transforms.lowercase", "lowercase")));
            cmbTransform.Items.Add(new KeyValuePair<string, string>("titlecase", LocalizationService.T("designer.transforms.titlecase", "Title Case")));

            // Font Weight dropdown
            cmbFont.Items.Clear();
            cmbFont.DisplayMember = "Value";
            cmbFont.ValueMember = "Key";
            cmbFont.Items.Add(new KeyValuePair<string, string>("A", LocalizationService.T("designer.thickness.normal", "Thin")));
            cmbFont.Items.Add(new KeyValuePair<string, string>("B", LocalizationService.T("designer.thickness.bold", "Bold")));

            // Font Family dropdown
            cmbFamily.Items.Clear();
            cmbFamily.DisplayMember = "Value";
            cmbFamily.ValueMember = "Key";
            cmbFamily.Items.Add(new KeyValuePair<string, string>("default", LocalizationService.T("designer.fonts.default")));
            cmbFamily.Items.Add(new KeyValuePair<string, string>("arial", LocalizationService.T("designer.fonts.arial")));
            cmbFamily.Items.Add(new KeyValuePair<string, string>("mono", LocalizationService.T("designer.fonts.mono")));
            cmbFamily.Items.Add(new KeyValuePair<string, string>("sans", LocalizationService.T("designer.fonts.sans", "Sans-Serif")));

            // Yes/No dropdown helper for item settings
            PopulateYesNoCombo(cmbShowPrice);
            PopulateYesNoCombo(cmbRightAlignPrice);
            PopulateYesNoCombo(cmbShowDetails);
            PopulateYesNoCombo(cmbShowNotes);
            PopulateYesNoCombo(cmbShowTax);
        }

        private void PopulateYesNoCombo(ComboBox combo)
        {
            combo.Items.Clear();
            combo.DisplayMember = "Value";
            combo.ValueMember = "Key";
            combo.Items.Add(new KeyValuePair<bool, string>(true, LocalizationService.T("designer.yes")));
            combo.Items.Add(new KeyValuePair<bool, string>(false, LocalizationService.T("designer.no")));
        }

        private static string ToFriendlyDisplay(string content)
        {
            if (string.IsNullOrEmpty(content)) return "";

            string text = content;

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
                text = text.Replace(kvp.Key, kvp.Value);
            }

            // 2. Replace Dynamic Data Variables with localized bracketed text
            var varReplacements = new Dictionary<string, string>
            {
                { "{restoran_adi}", "{" + LocalizationService.T("designer.vars.restaurant_name", "Restoran Adı") + "}" },
                { "{restoran_name}", "{" + LocalizationService.T("designer.vars.restaurant_name", "Restoran Adı") + "}" },
                { "{restaurant_name}", "{" + LocalizationService.T("designer.vars.restaurant_name", "Restoran Adı") + "}" },

                { "{restoran_adres}", "{" + LocalizationService.T("designer.vars.restaurant_address", "Restoran Adresi") + "}" },
                { "{restoran_address}", "{" + LocalizationService.T("designer.vars.restaurant_address", "Restoran Adresi") + "}" },
                { "{restaurant_address}", "{" + LocalizationService.T("designer.vars.restaurant_address", "Restoran Adresi") + "}" },

                { "{restoran_telefon}", "{" + LocalizationService.T("designer.vars.restaurant_phone", "Restoran Tel") + "}" },
                { "{restoran_phone}", "{" + LocalizationService.T("designer.vars.restaurant_phone", "Restoran Tel") + "}" },
                { "{restaurant_phone}", "{" + LocalizationService.T("designer.vars.restaurant_phone", "Restoran Tel") + "}" },

                { "{order_no}", "{" + LocalizationService.T("designer.vars.order_no", "Sipariş No") + "}" },
                { "{order_number}", "{" + LocalizationService.T("designer.vars.order_no", "Sipariş No") + "}" },
                { "{siparis_no}", "{" + LocalizationService.T("designer.vars.order_no", "Sipariş No") + "}" },

                { "{table_name}", "{" + LocalizationService.T("designer.vars.table_name", "Masa Adı") + "}" },
                { "{masa_adi}", "{" + LocalizationService.T("designer.vars.table_name", "Masa Adı") + "}" },

                { "{date}", "{" + LocalizationService.T("designer.vars.date", "Tarih") + "}" },
                { "{tarih}", "{" + LocalizationService.T("designer.vars.date", "Tarih") + "}" },

                { "{time}", "{" + LocalizationService.T("designer.vars.time", "Saat") + "}" },
                { "{saat}", "{" + LocalizationService.T("designer.vars.time", "Saat") + "}" },

                { "{customer_name}", "{" + LocalizationService.T("designer.vars.customer_name", "Müşteri Adı") + "}" },
                { "{musteri_adi}", "{" + LocalizationService.T("designer.vars.customer_name", "Müşteri Adı") + "}" },

                { "{customer_phone}", "{" + LocalizationService.T("designer.vars.customer_phone", "Müşteri Tel") + "}" },
                { "{musteri_telefon}", "{" + LocalizationService.T("designer.vars.customer_phone", "Müşteri Tel") + "}" },

                { "{delivery_address}", "{" + LocalizationService.T("designer.vars.delivery_address", "Teslimat Adresi") + "}" },
                { "{teslimat_adresi}", "{" + LocalizationService.T("designer.vars.delivery_address", "Teslimat Adresi") + "}" },

                { "{payment_type}", "{" + LocalizationService.T("designer.vars.payment_type", "Ödeme Tipi") + "}" },
                { "{odeme_tipi}", "{" + LocalizationService.T("designer.vars.payment_type", "Ödeme Tipi") + "}" },

                { "{note}", "{" + LocalizationService.T("designer.vars.note", "Sipariş Notu") + "}" },
                { "{ek_not}", "{" + LocalizationService.T("designer.vars.note", "Sipariş Notu") + "}" },

                { "{subtotal}", "{" + LocalizationService.T("designer.vars.subtotal", "Ara Toplam") + "}" },
                { "{ara_toplam}", "{" + LocalizationService.T("designer.vars.subtotal", "Ara Toplam") + "}" },

                { "{tax_total}", "{" + LocalizationService.T("designer.vars.tax_total", "KDV Toplamı") + "}" },
                { "{kdv_toplam}", "{" + LocalizationService.T("designer.vars.tax_total", "KDV Toplamı") + "}" },

                { "{grand_total}", "{" + LocalizationService.T("designer.vars.grand_total", "Genel Toplam") + "}" },
                { "{toplam_tutar}", "{" + LocalizationService.T("designer.vars.grand_total", "Genel Toplam") + "}" },

                { "{ekstra_toplam}", "{" + LocalizationService.T("designer.vars.extra_total", "Ekstra Toplam") + "}" },
                { "{extra_total}", "{" + LocalizationService.T("designer.vars.extra_total", "Ekstra Toplam") + "}" }
            };

            foreach (var kvp in varReplacements)
            {
                text = text.Replace(kvp.Key, kvp.Value);
            }

            return text;
        }

        private void LoadElementData()
        {
            txtContent.Text = _element.Type == "logo" ? _element.Path : ToFriendlyDisplay(_element.Content);
            
            SelectComboValue(cmbAlign, _element.Align);
            SelectComboValue(cmbSize, _element.Size);
            SelectComboValue(cmbTransform, _element.TextCase);
            SelectComboValue(cmbFont, _element.Font);
            SelectComboValue(cmbFamily, _element.Family);

            if (_element.Type == "items")
            {
                SelectComboValueBool(cmbShowPrice, _element.ShowPrice);
                SelectComboValueBool(cmbRightAlignPrice, _element.RightAlignPrice);
                txtCurrencySymbol.Text = _element.CurrencySymbol;
                SelectComboValueBool(cmbShowDetails, _element.ShowCustomizations);
                SelectComboValueBool(cmbShowNotes, _element.ShowNotes);
                SelectComboValueBool(cmbShowTax, _element.ShowTax);
            }
        }

        private void SelectComboValue(ComboBox combo, string val)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is KeyValuePair<string, string> kvp)
                {
                    if (kvp.Key == val)
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private void SelectComboValueBool(ComboBox combo, bool val)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (combo.Items[i] is KeyValuePair<bool, string> kvp)
                {
                    if (kvp.Key == val)
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private void AdjustVisiblePanels()
        {
            string t = _element.Type;

            // Hide or show controls dynamically
            lblContent.Visible = (t == "text" || t == "qrcode" || t == "barcode" || t == "logo");
            txtContent.Visible = lblContent.Visible;
            btnBrowse.Visible = (t == "logo");
            btnRemoveLogo.Visible = (t == "logo");
            
            lblAlign.Visible = (t != "separator");
            cmbAlign.Visible = lblAlign.Visible;

            lblSize.Visible = (t == "text" || t == "logo" || t == "qrcode" || t == "barcode");
            cmbSize.Visible = lblSize.Visible;
            
            lblTransform.Visible = (t == "text");
            cmbTransform.Visible = lblTransform.Visible;

            lblWeight.Visible = (t == "text");
            cmbFont.Visible = lblWeight.Visible;

            lblFamily.Visible = (t == "text" || t == "barcode");
            cmbFamily.Visible = lblFamily.Visible;

            pnlItemsSettings.Visible = (t == "items");
            
            lblTokensTitle.Visible = (t == "text");
            flpTokens.Visible = (t == "text");

            if (t == "logo")
            {
                lblContent.Text = LocalizationService.T("designer.labels.logo_path", "Logo Dosya Yolu:");
                lblSize.Text = LocalizationService.T("designer.labels.logo_size", "Logo Boyutu:");
                if (picLogoPreview != null) picLogoPreview.Visible = true;
                if (lblLogoInfo != null) lblLogoInfo.Visible = true;
                UpdateLogoPreview(txtContent.Text);
            }
            else
            {
                lblContent.Text = LocalizationService.T("designer.labels.content", "İçerik:");
                lblSize.Text = LocalizationService.T("designer.labels.size", "Boyut:");
                if (picLogoPreview != null) picLogoPreview.Visible = false;
                if (lblLogoInfo != null) lblLogoInfo.Visible = false;
            }

            if (t == "text")
            {
                PopulateTokens();
            }
        }

        private void PopulateTokens()
        {
            flpTokens.Controls.Clear();

            var tokenDefs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("{restoran_adi}", LocalizationService.T("designer.vars.restaurant_name", "Restoran Adı")),
                new KeyValuePair<string, string>("{restoran_adres}", LocalizationService.T("designer.vars.restaurant_address", "Restoran Adresi")),
                new KeyValuePair<string, string>("{restoran_telefon}", LocalizationService.T("designer.vars.restaurant_phone", "Restoran Tel")),
                new KeyValuePair<string, string>("{order_no}", LocalizationService.T("designer.vars.order_no", "Sipariş No")),
                new KeyValuePair<string, string>("{table_name}", LocalizationService.T("designer.vars.table_name", "Masa Adı")),
                new KeyValuePair<string, string>("{date}", LocalizationService.T("designer.vars.date", "Tarih")),
                new KeyValuePair<string, string>("{time}", LocalizationService.T("designer.vars.time", "Saat")),
                new KeyValuePair<string, string>("{customer_name}", LocalizationService.T("designer.vars.customer_name", "Müşteri Adı")),
                new KeyValuePair<string, string>("{customer_phone}", LocalizationService.T("designer.vars.customer_phone", "Müşteri Tel")),
                new KeyValuePair<string, string>("{delivery_address}", LocalizationService.T("designer.vars.delivery_address", "Teslimat Adresi")),
                new KeyValuePair<string, string>("{payment_type}", LocalizationService.T("designer.vars.payment_type", "Ödeme Tipi")),
                new KeyValuePair<string, string>("{note}", LocalizationService.T("designer.vars.note", "Sipariş Notu")),
                new KeyValuePair<string, string>("{subtotal}", LocalizationService.T("designer.vars.subtotal", "Ara Toplam")),
                new KeyValuePair<string, string>("{tax_total}", LocalizationService.T("designer.vars.tax_total", "KDV Toplamı")),
                new KeyValuePair<string, string>("{grand_total}", LocalizationService.T("designer.vars.grand_total", "Genel Toplam"))
            };

            foreach (var tok in tokenDefs)
            {
                Button btn = new Button
                {
                    Text = tok.Value,
                    Width = 104,
                    Height = 26,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ThemeManager.ColorFieldBg,
                    ForeColor = ThemeManager.ColorText,
                    Font = ThemeManager.FontSmall,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = ThemeManager.ColorBorder;
                
                string code = tok.Key;
                btn.Click += (s, e) => InsertToken(code);
                
                flpTokens.Controls.Add(btn);
            }
        }

        private void InsertToken(string code)
        {
            int selectionStart = txtContent.SelectionStart;
            txtContent.Text = txtContent.Text.Insert(selectionStart, code);
            txtContent.SelectionStart = selectionStart + code.Length;
            txtContent.Focus();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = LocalizationService.T("designer.dialogs.logo_browse_title");
                string filter = LocalizationService.T("designer.dialogs.logo_browse_filter");
                if (string.IsNullOrWhiteSpace(filter) || !filter.Contains("|"))
                {
                    filter = "Resim Dosyaları (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Tüm Dosyalar (*.*)|*.*";
                }
                ofd.Filter = filter;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtContent.Text = ofd.FileName;
                    UpdateLogoPreview(ofd.FileName);
                }
            }
        }

        private void btnRemoveLogo_Click(object sender, EventArgs e)
        {
            txtContent.Text = "";
            UpdateLogoPreview("");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save modified values to element
            if (_element.Type == "logo")
            {
                string path = txtContent.Text.Trim();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Length > 2 * 1024 * 1024)
                    {
                        double sizeMb = (double)fileInfo.Length / (1024 * 1024);
                        string errFormat = LocalizationService.T("designer.labels.logo_size_error", "⚠️ Dosya boyutu 2MB sınırını aşıyor ({size:F1} MB). Lütfen daha küçük bir resim seçiniz.");
                        MessageBox.Show(
                            errFormat.Replace("{size:F1}", sizeMb.ToString("F1")),
                            LocalizationService.T("designer.dialogs.error_title", "Hata"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }
                }
                _element.Path = path;
            }
            else
            {
                _element.Content = txtContent.Text.Trim();
            }

            if (cmbAlign.SelectedItem != null)
                _element.Align = ((KeyValuePair<string, string>)cmbAlign.SelectedItem).Key;

            if (cmbSize.SelectedItem != null)
                _element.Size = ((KeyValuePair<string, string>)cmbSize.SelectedItem).Key;

            if (cmbTransform.SelectedItem != null)
                _element.TextCase = ((KeyValuePair<string, string>)cmbTransform.SelectedItem).Key;

            if (cmbFont.SelectedItem != null)
                _element.Font = ((KeyValuePair<string, string>)cmbFont.SelectedItem).Key;

            if (cmbFamily.SelectedItem != null)
                _element.Family = ((KeyValuePair<string, string>)cmbFamily.SelectedItem).Key;

            if (_element.Type == "items")
            {
                if (cmbShowPrice.SelectedItem != null)
                    _element.ShowPrice = ((KeyValuePair<bool, string>)cmbShowPrice.SelectedItem).Key;

                if (cmbRightAlignPrice.SelectedItem != null)
                    _element.RightAlignPrice = ((KeyValuePair<bool, string>)cmbRightAlignPrice.SelectedItem).Key;

                _element.CurrencySymbol = txtCurrencySymbol.Text.Trim();

                if (cmbShowDetails.SelectedItem != null)
                    _element.ShowCustomizations = ((KeyValuePair<bool, string>)cmbShowDetails.SelectedItem).Key;

                if (cmbShowNotes.SelectedItem != null)
                    _element.ShowNotes = ((KeyValuePair<bool, string>)cmbShowNotes.SelectedItem).Key;

                if (cmbShowTax.SelectedItem != null)
                    _element.ShowTax = ((KeyValuePair<bool, string>)cmbShowTax.SelectedItem).Key;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
