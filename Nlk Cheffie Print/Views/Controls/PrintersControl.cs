using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;
using Nlk_Cheffie_Print.Core.Printer;
using Nlk_Cheffie_Print.Core.Workers;
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

            lstPrinters.DrawMode = DrawMode.OwnerDrawFixed;
            lstPrinters.ItemHeight = 36;
            lstPrinters.DrawItem += lstPrinters_DrawItem;

            TranslateUI();
            LoadPrintersFromConfig();
        }

        private void lstPrinters_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstPrinters.Items.Count) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            Color bgColor = isSelected ? Color.FromArgb(45, ThemeManager.ColorAccent) : ThemeManager.ColorFieldBg;
            Color textColor = isSelected ? ThemeManager.ColorAccent : ThemeManager.ColorText;

            using (var bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            if (isSelected)
            {
                using (var indicatorBrush = new SolidBrush(ThemeManager.ColorAccent))
                {
                    e.Graphics.FillRectangle(indicatorBrush, new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height));
                }
            }

            string text = lstPrinters.Items[e.Index]?.ToString() ?? "";
            Rectangle textRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y, e.Bounds.Width - 16, e.Bounds.Height);

            TextRenderer.DrawText(e.Graphics, text, ThemeManager.FontBodyBold, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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
            btnProfile.Text = LocalizationService.CurrentLanguage == "tr" ? "Yazıcı Profili" : 
                             (LocalizationService.CurrentLanguage == "de" ? "Druckerprofil" : 
                             (LocalizationService.CurrentLanguage == "ar" ? "ملف تعريف الطابعة" : 
                             (LocalizationService.CurrentLanguage == "es" ? "Perfil de impresora" : 
                             (LocalizationService.CurrentLanguage == "fr" ? "Profil d'imprimante" : "Printer Profile"))));

            string testText = LocalizationService.CurrentLanguage == "tr" ? "TEST" : 
                             (LocalizationService.CurrentLanguage == "ar" ? "تجربة" : 
                             (LocalizationService.CurrentLanguage == "es" ? "PRUEBA" : 
                             (LocalizationService.CurrentLanguage == "fr" ? "TEST" : "TEST")));

            btnTestKitchen.Text = LocalizationService.T("settings.kitchen").ToUpper() + " " + testText;
            btnTestCashier.Text = LocalizationService.T("settings.cashier").ToUpper() + " " + testText;
            btnTestCourier.Text = LocalizationService.T("settings.courier").ToUpper() + " " + testText;
            btnSaveRoles.Text = LocalizationService.T("printers.save_roles_btn");

            RefreshPrinterCombos();
        }

        private void LoadPrintersFromConfig()
        {
            RefreshInstalledPrinters(showResult: false);
        }

        private void RefreshInstalledPrinters(bool showResult)
        {
            ConfigManager.Current.Printers.Available ??= new List<PrinterInfo>();
            var printers = ConfigManager.Current.Printers.Available;
            var previousRoleIds = new Dictionary<string, string>
            {
                ["kitchen"] = ConfigManager.Current.Printers.Kitchen,
                ["cashier"] = ConfigManager.Current.Printers.Cashier,
                ["courier"] = ConfigManager.Current.Printers.Courier
            };

            var networkPrinters = printers
                .Where(p => p != null && (string.Equals(p.Type, "network", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Type, "ip", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var windowsPrinters = PrinterSettings.InstalledPrinters
                .Cast<string>()
                .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                .Select(name =>
                {
                    var existing = printers.FirstOrDefault(p => p != null &&
                        (string.Equals(p.Id, name, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)));
                    var detected = new PrinterInfo { Name = name, Id = name, Type = "win32" };
                    detected.Profile = existing?.Profile ?? PrinterProfileResolver.InferProfileId(detected);
                    return detected;
                })
                .ToList();

            var migratedRoleIds = new Dictionary<string, string>();
            foreach (var role in previousRoleIds)
            {
                string selectedId = role.Value;
                var previous = printers.FirstOrDefault(p => p != null && string.Equals(p.Id, selectedId, StringComparison.OrdinalIgnoreCase));
                string previousName = previous?.Name ?? string.Empty;
                string normalizedName = previousName.Replace(" (Thermal/RAW)", string.Empty, StringComparison.OrdinalIgnoreCase);

                var installed = windowsPrinters.FirstOrDefault(p =>
                    string.Equals(p.Name, previousName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

                migratedRoleIds[role.Key] = installed?.Id ?? selectedId;
            }

            printers.Clear();
            printers.AddRange(windowsPrinters);
            printers.AddRange(networkPrinters.Where(network => !printers.Any(p =>
                p != null && string.Equals(p.Id, network.Id, StringComparison.OrdinalIgnoreCase))));

            ConfigManager.Current.Printers.Kitchen = migratedRoleIds["kitchen"];
            ConfigManager.Current.Printers.Cashier = migratedRoleIds["cashier"];
            ConfigManager.Current.Printers.Courier = migratedRoleIds["courier"];
            ConfigManager.Save();

            lstPrinters.Items.Clear();
            foreach (var printer in printers)
            {
                var profile = PrinterProfileResolver.Resolve(printer);
                string name = string.Equals(printer.Type, "win32", StringComparison.OrdinalIgnoreCase)
                    ? printer.Name
                    : $"{printer.Name} [{printer.Id}]";
                lstPrinters.Items.Add($"{name} — {profile.DisplayName}");
            }

            RefreshPrinterCombos();

            if (showResult)
            {
                MessageBox.Show(
                    LocalizationService.T("printers.scan_found").Replace("{count}", windowsPrinters.Count.ToString()),
                    LocalizationService.T("printers.scan_completed"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
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
            try
            {
                RefreshInstalledPrinters(showResult: true);
            }
            finally
            {
                btnScanUsb.Enabled = true;
                btnScanUsb.Text = LocalizationService.T("printers.scan_usb");
            }
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
                    Port = 9100,
                    Profile = PrinterProfileResolver.EscPos80
                };

                ConfigManager.Current.Printers.Available.Add(newPrinter);
                ConfigManager.Save();

                lstPrinters.Items.Add($"{newPrinter.Name} [{newPrinter.Id}] — {PrinterProfileResolver.Resolve(newPrinter).DisplayName}");
                RefreshPrinterCombos();
            }
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            int index = lstPrinters.SelectedIndex;
            var printers = ConfigManager.Current.Printers.Available;
            if (index < 0 || printers == null || index >= printers.Count)
            {
                MessageBox.Show("Lütfen önce listeden bir yazıcı seçin.", "Yazıcı Profili", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var printer = printers[index];
            if (printer == null)
            {
                MessageBox.Show("Seçili yazıcı kaydı geçersiz.", "Yazıcı Profili", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var menu = new ContextMenuStrip();
            AddProfileMenuItem(menu, printer, PrinterProfileResolver.EscPos58, index);
            AddProfileMenuItem(menu, printer, PrinterProfileResolver.EscPos80, index);
            if (string.Equals(printer.Type, "win32", StringComparison.OrdinalIgnoreCase) || string.Equals(printer.Type, "win32_gdi", StringComparison.OrdinalIgnoreCase))
            {
                AddProfileMenuItem(menu, printer, PrinterProfileResolver.WindowsGdi, index);
            }
            menu.Show(btnProfile, new Point(0, btnProfile.Height));
        }

        private void AddProfileMenuItem(ContextMenuStrip menu, PrinterInfo printer, string profileId, int selectedIndex)
        {
            var profile = PrinterProfileResolver.Resolve(new PrinterInfo { Profile = profileId });
            var item = new ToolStripMenuItem(profile.DisplayName)
            {
                Checked = PrinterProfileResolver.Resolve(printer).Id == profileId
            };
            item.Click += (_, _) =>
            {
                // Defer execution after menu has completely closed to avoid native WndProc access errors
                BeginInvoke(new Action(() =>
                {
                    try
                    {
                        printer.Profile = profileId;
                        ConfigManager.Save();

                        if (!IsDisposed)
                        {
                            RefreshInstalledPrinters(showResult: false);
                            if (selectedIndex >= 0 && selectedIndex < lstPrinters.Items.Count)
                            {
                                lstPrinters.SelectedIndex = selectedIndex;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Printer profile update failed: {ex}");
                        MessageBox.Show(
                            $"Yazıcı profili güncellenemedi: {ex.Message}",
                            "Yazıcı Profili",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }));
            };
            menu.Items.Add(item);
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

            switch (role)
            {
                case "kitchen":
                    ConfigManager.Current.Printers.Kitchen = selectedId;
                    break;
                case "cashier":
                    ConfigManager.Current.Printers.Cashier = selectedId;
                    break;
                case "courier":
                    ConfigManager.Current.Printers.Courier = selectedId;
                    break;
            }

            ConfigManager.Save();

            string testSlip = $"{{\"restaurant_info\":{{\"name\":{JsonSerializer.Serialize(ConfigManager.Current.App.RestaurantName)},\"address\":{JsonSerializer.Serialize(ConfigManager.Current.App.RestaurantAddress)},\"phone\":{JsonSerializer.Serialize(ConfigManager.Current.App.RestaurantPhone)}}},\"order_info\":{{\"order_number\":\"TEST-001\",\"table_name\":\"Test Masa\",\"date\":\"{DateTime.Now:dd.MM.yyyy}\",\"time\":\"{DateTime.Now:HH:mm}\",\"payment_method\":\"Nakit\"}},\"payment_info\":{{\"subtotal\":\"100.00\",\"tax\":\"10.00\",\"total\":\"110.00\"}},\"items\":[{{\"quantity\":1,\"name\":\"Test Ürünü\",\"line_total\":\"100.00\"}}]}}";
            using var document = JsonDocument.Parse(testSlip);
            PrintQueueWorker.Enqueue(new PrintJob
            {
                Role = role,
                Data = document.RootElement.Clone(),
                JobId = $"test-{role}-{Guid.NewGuid():N}",
                ForcePrint = true,
                SkipOrderLog = true
            });

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
