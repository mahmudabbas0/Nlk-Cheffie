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
            
            PopulateDropdowns();
            LoadElementData();
            AdjustVisiblePanels();
        }

        private void TranslateUI()
        {
            this.Text = LocalizationService.T("designer.dialogs.edit_title");
            
            lblContent.Text = _element.Type == "logo" 
                ? LocalizationService.T("designer.dialogs.path") 
                : LocalizationService.T("designer.dialogs.content");
                
            lblAlign.Text = LocalizationService.T("designer.dialogs.align");
            lblSize.Text = LocalizationService.T("designer.dialogs.size");
            lblWeight.Text = LocalizationService.T("designer.dialogs.weight");
            lblFamily.Text = LocalizationService.T("designer.dialogs.family");
            
            lblShowPrice.Text = LocalizationService.T("designer.dialogs.show_price");
            lblRightAlignPrice.Text = LocalizationService.T("designer.dialogs.right_align_price");
            lblCurrencySymbol.Text = LocalizationService.T("designer.dialogs.currency_symbol");
            lblShowDetails.Text = LocalizationService.T("designer.dialogs.show_customizations");
            lblShowNotes.Text = LocalizationService.T("designer.dialogs.show_notes");
            lblShowTax.Text = LocalizationService.T("designer.dialogs.show_tax");
            
            lblTokensTitle.Text = LocalizationService.T("designer.dialogs.add_var");
            btnBrowse.Text = LocalizationService.T("designer.dialogs.browse");
            
            btnSave.Text = LocalizationService.T("designer.dialogs.save");
            btnCancel.Text = LocalizationService.T("settings.cancel");
        }

        private void PopulateDropdowns()
        {
            // Alignment dropdown
            cmbAlign.Items.Clear();
            cmbAlign.DisplayMember = "Value";
            cmbAlign.ValueMember = "Key";
            cmbAlign.Items.Add(new KeyValuePair<string, string>("left", LocalizationService.T("designer.align.left")));
            cmbAlign.Items.Add(new KeyValuePair<string, string>("center", LocalizationService.T("designer.align.center")));
            cmbAlign.Items.Add(new KeyValuePair<string, string>("right", LocalizationService.T("designer.align.right")));

            // Size dropdown
            cmbSize.Items.Clear();
            cmbSize.DisplayMember = "Value";
            cmbSize.ValueMember = "Key";
            cmbSize.Items.Add(new KeyValuePair<string, string>("1x", LocalizationService.T("designer.sizes.normal")));
            cmbSize.Items.Add(new KeyValuePair<string, string>("2x", LocalizationService.T("designer.sizes.large")));

            // Font Weight dropdown
            cmbFont.Items.Clear();
            cmbFont.DisplayMember = "Value";
            cmbFont.ValueMember = "Key";
            cmbFont.Items.Add(new KeyValuePair<string, string>("A", LocalizationService.T("designer.thickness.normal")));
            cmbFont.Items.Add(new KeyValuePair<string, string>("B", LocalizationService.T("designer.thickness.bold")));

            // Font Family dropdown
            cmbFamily.Items.Clear();
            cmbFamily.DisplayMember = "Value";
            cmbFamily.ValueMember = "Key";
            cmbFamily.Items.Add(new KeyValuePair<string, string>("default", LocalizationService.T("designer.fonts.default")));
            cmbFamily.Items.Add(new KeyValuePair<string, string>("arial", LocalizationService.T("designer.fonts.arial")));
            cmbFamily.Items.Add(new KeyValuePair<string, string>("mono", LocalizationService.T("designer.fonts.mono")));

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

        private void LoadElementData()
        {
            txtContent.Text = _element.Type == "logo" ? _element.Path : _element.Content;
            
            SelectComboValue(cmbAlign, _element.Align);
            SelectComboValue(cmbSize, _element.Size);
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
            
            lblAlign.Visible = (t != "separator");
            cmbAlign.Visible = lblAlign.Visible;

            lblSize.Visible = (t == "text");
            cmbSize.Visible = lblSize.Visible;
            
            lblWeight.Visible = (t == "text");
            cmbFont.Visible = lblWeight.Visible;

            lblFamily.Visible = (t == "text" || t == "barcode");
            cmbFamily.Visible = lblFamily.Visible;

            pnlItemsSettings.Visible = (t == "items");
            
            lblTokensTitle.Visible = (t == "text");
            flpTokens.Visible = (t == "text");

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
                new KeyValuePair<string, string>("{restoran_adi}", "Restoran"),
                new KeyValuePair<string, string>("{restoran_adres}", "Adres"),
                new KeyValuePair<string, string>("{restoran_telefon}", "Telefon"),
                new KeyValuePair<string, string>("{order_no}", "Sipariş No"),
                new KeyValuePair<string, string>("{table_name}", "Masa/Paket"),
                new KeyValuePair<string, string>("{date}", "Tarih"),
                new KeyValuePair<string, string>("{time}", "Saat"),
                new KeyValuePair<string, string>("{customer_name}", "Müşteri"),
                new KeyValuePair<string, string>("{customer_phone}", "Müşteri Tel"),
                new KeyValuePair<string, string>("{delivery_address}", "Teslimat"),
                new KeyValuePair<string, string>("{payment_type}", "Ödeme"),
                new KeyValuePair<string, string>("{note}", "Not"),
                new KeyValuePair<string, string>("{subtotal}", "Ara Toplam"),
                new KeyValuePair<string, string>("{tax_total}", "KDV"),
                new KeyValuePair<string, string>("{grand_total}", "Toplam")
            };

            foreach (var tok in tokenDefs)
            {
                Button btn = new Button
                {
                    Text = tok.Value,
                    Width = 80,
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
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Save modified values to element
            if (_element.Type == "logo")
            {
                _element.Path = txtContent.Text.Trim();
            }
            else
            {
                _element.Content = txtContent.Text.Trim();
            }

            if (cmbAlign.SelectedItem != null)
                _element.Align = ((KeyValuePair<string, string>)cmbAlign.SelectedItem).Key;

            if (cmbSize.SelectedItem != null)
                _element.Size = ((KeyValuePair<string, string>)cmbSize.SelectedItem).Key;

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
