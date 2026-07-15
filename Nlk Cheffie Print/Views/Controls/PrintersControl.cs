using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Models;

namespace Nlk_Cheffie_Print.Views.Controls
{
    public partial class PrintersControl : UserControl
    {
        public PrintersControl()
        {
            InitializeComponent();
            LocalizationService.LanguageChanged += TranslateUI;
            this.DoubleBuffered = true;
        }

        private void PrintersControl_Load(object sender, EventArgs e)
        {
            ThemeManager.ApplyTheme(this);
            pnlPrintersCard.BackColor = ThemeManager.ColorCard;
            pnlRolesCard.BackColor = ThemeManager.ColorCard;
            lstPrinters.BackColor = ThemeManager.ColorFieldBg;
            lstPrinters.ForeColor = ThemeManager.ColorText;

            TranslateUI();
            LoadPrintersFromConfig();
        }

        public void TranslateUI()
        {
            lblTitle.Text = LocalizationService.T("printers.title");
            lblSubtitle.Text = LocalizationService.T("printers.subtitle");
            lblRolesTitle.Text = LocalizationService.T("printers.roles");
            lblRoleKitchen.Text = LocalizationService.T("printers.role_kitchen").ToUpper();
            lblRoleCashier.Text = LocalizationService.T("printers.role_cashier").ToUpper();
            lblRoleCourier.Text = LocalizationService.T("printers.role_courier").ToUpper();

            btnScanUsb.Text = LocalizationService.T("printers.scan_usb");
            btnAddIp.Text = LocalizationService.T("printers.add_ip");
            btnRemove.Text = LocalizationService.T("printers.remove");

            btnTestKitchen.Text = LocalizationService.T("settings.kitchen").ToUpper() + " TEST";
            btnTestCashier.Text = LocalizationService.T("settings.cashier").ToUpper() + " TEST";
            btnTestCourier.Text = LocalizationService.T("settings.courier").ToUpper() + " TEST";
            btnSaveRoles.Text = LocalizationService.T("printers.save_roles_btn");

            RefreshPrinterCombos();
        }

        private void LoadPrintersFromConfig()
        {
            lstPrinters.Items.Clear();
            
            var printers = ConfigManager.Current.Printers.Available;
            if (printers.Count == 0)
            {
                // Add some default mock printers if none loaded
                printers.Add(new PrinterInfo { Name = "XP-80C (Thermal/RAW)", Type = "usb", Id = "USB001" });
                printers.Add(new PrinterInfo { Name = "POS-58 (Thermal/RAW)", Type = "usb", Id = "USB002" });
                printers.Add(new PrinterInfo { Name = "Epson TM-T88VI", Type = "network", Id = "192.168.1.200", Port = 9100 });
                ConfigManager.Save();
            }

            foreach (var p in printers)
            {
                string display = $"{p.Name} [{p.Id}]";
                lstPrinters.Items.Add(display);
            }

            RefreshPrinterCombos();
        }

        private void RefreshPrinterCombos()
        {
            var printers = ConfigManager.Current.Printers.Available;

            // Kitchen combo
            string? prevKitchen = cmbKitchen.SelectedValue as string ?? ConfigManager.Current.Printers.Kitchen;
            PopulateCombo(cmbKitchen, printers, prevKitchen);

            // Cashier combo
            string? prevCashier = cmbCashier.SelectedValue as string ?? ConfigManager.Current.Printers.Cashier;
            PopulateCombo(cmbCashier, printers, prevCashier);

            // Courier combo
            string? prevCourier = cmbCourier.SelectedValue as string ?? ConfigManager.Current.Printers.Courier;
            PopulateCombo(cmbCourier, printers, prevCourier);
        }

        private void PopulateCombo(ComboBox combo, List<PrinterInfo> printers, string selectedId)
        {
            combo.DataSource = null;
            var list = new List<KeyValuePair<string, string>>();
            list.Add(new KeyValuePair<string, string>("", $"- {LocalizationService.T("printers.select")} -"));

            foreach (var p in printers)
            {
                list.Add(new KeyValuePair<string, string>(p.Id, p.Name));
            }

            combo.DisplayMember = "Value";
            combo.ValueMember = "Key";
            combo.DataSource = list;

            // Select active item
            if (!string.IsNullOrEmpty(selectedId))
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    if (combo.Items[i] is KeyValuePair<string, string> kvp)
                    {
                        if (kvp.Key == selectedId)
                        {
                            combo.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                combo.SelectedIndex = 0;
            }
        }

        private void btnScanUsb_Click(object sender, EventArgs e)
        {
            btnScanUsb.Enabled = false;
            btnScanUsb.Text = LocalizationService.T("printers.scanning");

            // Mock timer to simulate scanning
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, ev) =>
            {
                timer.Stop();
                timer.Dispose();

                // Add scanned printer
                string scannedId = $"USB00{ConfigManager.Current.Printers.Available.Count + 1}";
                var newPrinter = new PrinterInfo { Name = $"New Thermal Printer ({scannedId})", Type = "usb", Id = scannedId };
                ConfigManager.Current.Printers.Available.Add(newPrinter);
                ConfigManager.Save();

                lstPrinters.Items.Add($"{newPrinter.Name} [{newPrinter.Id}]");
                RefreshPrinterCombos();

                btnScanUsb.Enabled = true;
                btnScanUsb.Text = LocalizationService.T("printers.scan_usb");

                MessageBox.Show(
                    LocalizationService.T("printers.scan_found").Replace("{count}", "1"),
                    LocalizationService.T("printers.scan_completed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            };
            timer.Start();
        }

        private void btnAddIp_Click(object sender, EventArgs e)
        {
            // Simple prompt dialog simulation
            string ip = Microsoft.VisualBasic.Interaction.InputBox(
                LocalizationService.T("printers.ip_dialog_msg"),
                LocalizationService.T("printers.ip_dialog_title"),
                "192.168.1.100"
            );

            if (!string.IsNullOrEmpty(ip.Trim()))
            {
                var newPrinter = new PrinterInfo
                {
                    Name = $"{LocalizationService.T("printers.network_printer")} ({ip})",
                    Type = "network",
                    Id = ip.Trim(),
                    Port = 9100
                };

                ConfigManager.Current.Printers.Available.Add(newPrinter);
                ConfigManager.Save();

                lstPrinters.Items.Add($"{newPrinter.Name} [{newPrinter.Id}]");
                RefreshPrinterCombos();
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int index = lstPrinters.SelectedIndex;
            if (index < 0) return;

            var confirmResult = MessageBox.Show(
                LocalizationService.T("designer.dialogs.delete_confirm_msg"),
                LocalizationService.T("designer.dialogs.delete_confirm_title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult == DialogResult.Yes)
            {
                ConfigManager.Current.Printers.Available.RemoveAt(index);
                ConfigManager.Save();
                
                lstPrinters.Items.RemoveAt(index);
                RefreshPrinterCombos();
            }
        }

        private void btnSaveRoles_Click(object sender, EventArgs e)
        {
            ConfigManager.Current.Printers.Kitchen = cmbKitchen.SelectedValue?.ToString() ?? "";
            ConfigManager.Current.Printers.Cashier = cmbCashier.SelectedValue?.ToString() ?? "";
            ConfigManager.Current.Printers.Courier = cmbCourier.SelectedValue?.ToString() ?? "";

            ConfigManager.Save();

            MessageBox.Show(
                LocalizationService.T("printers.save_roles_success"),
                LocalizationService.T("printers.save_roles_success"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void btnTestKitchen_Click(object sender, EventArgs e)
        {
            TriggerTestPrint("kitchen");
        }

        private void btnTestCashier_Click(object sender, EventArgs e)
        {
            TriggerTestPrint("cashier");
        }

        private void btnTestCourier_Click(object sender, EventArgs e)
        {
            TriggerTestPrint("courier");
        }

        private void TriggerTestPrint(string role)
        {
            string selectedId = role switch
            {
                "kitchen" => cmbKitchen.SelectedValue?.ToString() ?? "",
                "cashier" => cmbCashier.SelectedValue?.ToString() ?? "",
                "courier" => cmbCourier.SelectedValue?.ToString() ?? "",
                _ => ""
            };

            if (string.IsNullOrEmpty(selectedId))
            {
                string roleTitle = role switch
                {
                    "kitchen" => LocalizationService.T("settings.kitchen"),
                    "cashier" => LocalizationService.T("settings.cashier"),
                    "courier" => LocalizationService.T("settings.courier"),
                    _ => role
                };

                MessageBox.Show(
                    LocalizationService.T("qrm.dialogs.no_printer").Replace("{role}", roleTitle),
                    LocalizationService.T("qrm.dialogs.warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            MessageBox.Show(
                LocalizationService.T("qrm.dialogs.print_queued").Replace("{role}", role.ToUpper()),
                LocalizationService.T("qrm.dialogs.info"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            LocalizationService.LanguageChanged -= TranslateUI;
            base.OnHandleDestroyed(e);
        }
    }
}
