using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Views
{
    public partial class SettingsForm : Form
    {
        public bool ConnectionResetTriggered { get; private set; } = false;
        private string _originalLanguage = "tr";

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
            pnlGeneralCard.BackColor = ThemeManager.ColorCard;
            pnlConnectionCard.BackColor = ThemeManager.ColorCard;
            pnlAppCard.BackColor = ThemeManager.ColorCard;

            PopulateDropdowns();
            LoadConfigData();

            _originalLanguage = LocalizationService.CurrentLanguage;

            TranslateUI();
        }

        private void TranslateUI()
        {
            this.Text = LocalizationService.T("settings.title");
            lblGeneralTitle.Text = LocalizationService.T("settings.general").ToUpper();
            lblConnectionTitle.Text = LocalizationService.T("settings.connection").ToUpper();
            lblAppTitle.Text = LocalizationService.T("settings.app").ToUpper();

            chkDryRun.Text = LocalizationService.T("settings.dry_run");
            chkGraphicMode.Text = LocalizationService.T("settings.graphic_mode");
            chkAutoPrint.Text = LocalizationService.T("settings.auto_print");
            lblAutoPrintRole.Text = LocalizationService.T("settings.auto_print_role");
            lblLanguage.Text = LocalizationService.T("settings.language");

            lblServerUrl.Text = LocalizationService.T("settings.server_url");
            btnResetConnection.Text = LocalizationService.T("settings.reset_connection");
            btnQuit.Text = LocalizationService.T("settings.quit");

            btnOk.Text = LocalizationService.T("settings.ok");
            btnCancel.Text = LocalizationService.T("settings.cancel");

            // Re-populate auto print role combo with translations
            int selectedRoleIdx = cmbAutoPrintRole.SelectedIndex;
            cmbAutoPrintRole.Items.Clear();
            cmbAutoPrintRole.Items.Add(new KeyValuePair<string, string>("kitchen", LocalizationService.T("settings.kitchen")));
            cmbAutoPrintRole.Items.Add(new KeyValuePair<string, string>("cashier", LocalizationService.T("settings.cashier")));
            cmbAutoPrintRole.Items.Add(new KeyValuePair<string, string>("courier", LocalizationService.T("settings.courier")));
            cmbAutoPrintRole.SelectedIndex = selectedRoleIdx >= 0 ? selectedRoleIdx : 0;
        }

        private void PopulateDropdowns()
        {
            // Auto Print Role combo
            cmbAutoPrintRole.DisplayMember = "Value";
            cmbAutoPrintRole.ValueMember = "Key";
            cmbAutoPrintRole.Items.Add(new KeyValuePair<string, string>("kitchen", LocalizationService.T("settings.kitchen")));
            cmbAutoPrintRole.Items.Add(new KeyValuePair<string, string>("cashier", LocalizationService.T("settings.cashier")));
            cmbAutoPrintRole.Items.Add(new KeyValuePair<string, string>("courier", LocalizationService.T("settings.courier")));

            // Language combo
            cmbLanguage.DisplayMember = "Value";
            cmbLanguage.ValueMember = "Key";
            foreach (var lang in LocalizationService.GetAvailableLanguages())
            {
                cmbLanguage.Items.Add(lang);
            }
        }

        private void LoadConfigData()
        {
            var app = ConfigManager.Current.App;
            chkDryRun.Checked = app.DryRun;
            chkGraphicMode.Checked = app.GraphicMode;
            chkAutoPrint.Checked = app.AutoPrintEnabled;
            txtApiUrl.Text = app.ApiBaseUrl;

            // Select active role
            for (int i = 0; i < cmbAutoPrintRole.Items.Count; i++)
            {
                if (cmbAutoPrintRole.Items[i] is KeyValuePair<string, string> kvp)
                {
                    if (kvp.Key == app.AutoPrintRole)
                    {
                        cmbAutoPrintRole.SelectedIndex = i;
                        break;
                    }
                }
            }
            if (cmbAutoPrintRole.SelectedIndex < 0 && cmbAutoPrintRole.Items.Count > 0)
                cmbAutoPrintRole.SelectedIndex = 0;

            // Select active language
            for (int i = 0; i < cmbLanguage.Items.Count; i++)
            {
                if (cmbLanguage.Items[i] is KeyValuePair<string, string> kvp)
                {
                    if (kvp.Key == app.Language)
                    {
                        cmbLanguage.SelectedIndex = i;
                        break;
                    }
                }
            }
            if (cmbLanguage.SelectedIndex < 0 && cmbLanguage.Items.Count > 0)
                cmbLanguage.SelectedIndex = 0;
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLanguage.SelectedItem == null) return;
            
            var selectedKvp = (KeyValuePair<string, string>)cmbLanguage.SelectedItem;
            string newLang = selectedKvp.Key;
            
            // Instantly apply translation to the service (which fires UI updates)
            LocalizationService.CurrentLanguage = newLang;
            TranslateUI();
        }

        private void btnResetConnection_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(
                LocalizationService.T("settings.reset_confirm_msg"),
                LocalizationService.T("settings.reset_confirm_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmResult == DialogResult.Yes)
            {
                // Clear active profile settings
                ConfigManager.Current.App.DeviceToken = "";
                ConfigManager.Current.App.RestaurantName = "";
                ConfigManager.Current.App.RestaurantSlug = "";
                ConfigManager.Current.App.RestaurantAddress = "";
                ConfigManager.Current.App.RestaurantPhone = "";
                ConfigManager.Current.App.RestaurantId = "";
                ConfigManager.Save();

                MessageBox.Show(
                    LocalizationService.T("settings.reset_success_msg"),
                    LocalizationService.T("settings.reset_success_title"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ConnectionResetTriggered = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string apiUrl = txtApiUrl.Text.Trim();
            if (!ConfigManager.IsSecureApiUrl(apiUrl))
            {
                MessageBox.Show("Sunucu API URL adresi HTTPS olmalıdır.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var app = ConfigManager.Current.App;
            app.DryRun = chkDryRun.Checked;
            app.GraphicMode = chkGraphicMode.Checked;
            app.AutoPrintEnabled = chkAutoPrint.Checked;
            app.ApiBaseUrl = apiUrl;

            if (cmbAutoPrintRole.SelectedItem != null)
                app.AutoPrintRole = ((KeyValuePair<string, string>)cmbAutoPrintRole.SelectedItem).Key;

            if (cmbLanguage.SelectedItem != null)
                app.Language = ((KeyValuePair<string, string>)cmbLanguage.SelectedItem).Key;

            ConfigManager.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (DialogResult != DialogResult.OK)
            {
                LocalizationService.CurrentLanguage = _originalLanguage;
            }
        }
    }
}
