using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views
{
    public partial class AddItemForm : Form
    {
        public TemplateElement? CreatedElement { get; private set; }

        public AddItemForm()
        {
            InitializeComponent();
        }

        private void AddItemForm_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
            TranslateUI();
            PopulateVariableButtons();
        }

        private void TranslateUI()
        {
            this.Text = LocalizationService.T("designer.dialogs.add_title");
            lblTitleBasic.Text = LocalizationService.T("designer.dialogs.basic_items");
            lblTitleVars.Text = LocalizationService.T("designer.dialogs.auto_vars");
            
            btnText.Text = " " + LocalizationService.T("designer.dialogs.text");
            btnSeparator.Text = " " + LocalizationService.T("designer.dialogs.separator");
            btnQrCode.Text = " " + LocalizationService.T("designer.dialogs.qrcode");
            btnLogo.Text = " " + LocalizationService.T("designer.dialogs.logo");
            btnBarcode.Text = " " + LocalizationService.T("designer.dialogs.barcode");
            btnItems.Text = " " + LocalizationService.T("designer.dialogs.items");
            
            btnCancel.Text = LocalizationService.T("settings.cancel");
        }

        private void PopulateVariableButtons()
        {
            flpVars.Controls.Clear();

            var varsMapping = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("{restoran_adi}", LocalizationService.T("designer.vars.restaurant_name")),
                new KeyValuePair<string, string>("{restoran_adres}", LocalizationService.T("designer.vars.restaurant_address")),
                new KeyValuePair<string, string>("{restoran_telefon}", LocalizationService.T("designer.vars.restaurant_phone")),
                new KeyValuePair<string, string>("{restoran_vergi_no}", LocalizationService.T("designer.vars.restaurant_tax")),
                new KeyValuePair<string, string>("{masa_no}", LocalizationService.T("designer.vars.table_no")),
                new KeyValuePair<string, string>("{masa_adi}", LocalizationService.T("designer.vars.table_name")),
                new KeyValuePair<string, string>("{siparis_no}", LocalizationService.T("designer.vars.order_no")),
                new KeyValuePair<string, string>("{garson_adi}", LocalizationService.T("designer.vars.waiter_name")),
                new KeyValuePair<string, string>("{tarih}", LocalizationService.T("designer.vars.date")),
                new KeyValuePair<string, string>("{saat}", LocalizationService.T("designer.vars.time")),
                new KeyValuePair<string, string>("{ara_toplam}", LocalizationService.T("designer.vars.subtotal")),
                new KeyValuePair<string, string>("{ekstra_toplam}", LocalizationService.T("designer.vars.extra_total")),
                new KeyValuePair<string, string>("{kdv_toplam}", LocalizationService.T("designer.vars.tax_total")),
                new KeyValuePair<string, string>("{toplam_tutar}", LocalizationService.T("designer.vars.grand_total")),
                new KeyValuePair<string, string>("{musteri_adi}", LocalizationService.T("designer.vars.customer_name")),
                new KeyValuePair<string, string>("{musteri_telefon}", LocalizationService.T("designer.vars.customer_phone")),
                new KeyValuePair<string, string>("{musteri_email}", LocalizationService.T("designer.vars.customer_email")),
                new KeyValuePair<string, string>("{teslimat_adresi}", LocalizationService.T("designer.vars.delivery_address")),
                new KeyValuePair<string, string>("{odeme_tipi}", LocalizationService.T("designer.vars.payment_type")),
                new KeyValuePair<string, string>("{ek_not}", LocalizationService.T("designer.vars.note")),
                new KeyValuePair<string, string>("{wifi_ag_adi}", LocalizationService.T("designer.vars.wifi_name")),
                new KeyValuePair<string, string>("{wifi_sifresi}", LocalizationService.T("designer.vars.wifi_password")),
                new KeyValuePair<string, string>("{odeme_linki}", LocalizationService.T("designer.vars.payment_link")),
                new KeyValuePair<string, string>("{slip_title}", LocalizationService.T("designer.vars.slip_title")),
                new KeyValuePair<string, string>("{siparis_durumu}", LocalizationService.T("designer.vars.order_status")),
                new KeyValuePair<string, string>("{iptal_sebebi}", LocalizationService.T("designer.vars.cancel_reason")),
                new KeyValuePair<string, string>("{iptal_saati}", LocalizationService.T("designer.vars.cancel_time"))
            };

            foreach (var kvp in varsMapping)
            {
                Button btn = new Button
                {
                    Text = kvp.Value,
                    Width = 190,
                    Height = 35,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = ThemeManager.ColorFieldBg,
                    ForeColor = ThemeManager.ColorText,
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderColor = ThemeManager.ColorBorder;
                
                string code = kvp.Key;
                btn.Click += (s, e) => SelectVariable(code);
                
                flpVars.Controls.Add(btn);
            }
        }

        private void SelectVariable(string varCode)
        {
            CreatedElement = new TemplateElement
            {
                Type = "text",
                Content = varCode,
                Align = "left",
                Font = "A",
                Size = "1x"
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnText_Click(object sender, EventArgs e)
        {
            CreatedElement = new TemplateElement
            {
                Type = "text",
                Content = LocalizationService.T("designer.dialogs.new_text_default", "Yeni metin"),
                Align = "left",
                Font = "A",
                Size = "1x"
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnSeparator_Click(object sender, EventArgs e)
        {
            CreatedElement = new TemplateElement { Type = "separator" };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnQrCode_Click(object sender, EventArgs e)
        {
            CreatedElement = new TemplateElement
            {
                Type = "qrcode",
                Content = "{odeme_linki}",
                Align = "center"
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnLogo_Click(object sender, EventArgs e)
        {
            CreatedElement = new TemplateElement
            {
                Type = "logo",
                Path = "",
                Align = "center"
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnBarcode_Click(object sender, EventArgs e)
        {
            CreatedElement = new TemplateElement
            {
                Type = "barcode",
                Content = "{siparis_no}",
                Align = "center"
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnItems_Click(object sender, EventArgs e)
        {
            CreatedElement = new TemplateElement
            {
                Type = "items",
                Align = "left"
            };
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
