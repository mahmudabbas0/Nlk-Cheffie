using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nlk_Cheffie_Print.Core
{
    public static class ThemeManager
    {
        // Dark Theme Colors
        public static readonly Color ColorBackground = Color.FromArgb(15, 15, 16);        // Deep black
        public static readonly Color ColorCard = Color.FromArgb(22, 22, 24);              // Card background
        public static readonly Color ColorBorder = Color.FromArgb(44, 44, 46);             // Faint border
        public static readonly Color ColorText = Color.FromArgb(242, 242, 247);           // Bright text
        public static readonly Color ColorTextMuted = Color.FromArgb(142, 142, 147);       // Faint gray text
        
        public static readonly Color ColorAccent = Color.FromArgb(255, 152, 0);           // Amber/Orange (#FF9800)
        public static readonly Color ColorAccentHover = Color.FromArgb(255, 172, 51);      // Amber hover
        public static readonly Color ColorAccentPressed = Color.FromArgb(230, 137, 0);    // Amber pressed
        
        public static readonly Color ColorSuccess = Color.FromArgb(76, 175, 80);          // Green (#4CAF50)
        public static readonly Color ColorDanger = Color.FromArgb(239, 68, 68);           // Red (#EF4444)
        public static readonly Color ColorDangerBg = Color.FromArgb(30, 239, 68, 68);     // Red transparent bg
        
        public static readonly Color ColorFieldBg = Color.FromArgb(28, 28, 30);           // Input fields background
        
        // Font Definitions
        public static readonly Font FontTitle = new Font("Segoe UI", 11, FontStyle.Bold);
        public static readonly Font FontHeader = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Segoe UI", 9, FontStyle.Regular);
        public static readonly Font FontBodyBold = new Font("Segoe UI", 9, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 8, FontStyle.Regular);
        public static readonly Font FontSmallBold = new Font("Segoe UI", 8, FontStyle.Bold);

        public static void ApplyTheme(Control control)
        {
            if (control is Form form)
            {
                form.BackColor = ColorBackground;
                form.ForeColor = ColorText;
            }

            EnableDoubleBufferingRecursive(control);
            ApplyThemeRecursive(control);
        }

        private static void EnableDoubleBufferingRecursive(Control control)
        {
            try
            {
                typeof(Control).GetProperty("DoubleBuffered", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(control, true, null);
            }
            catch { }

            foreach (Control child in control.Controls)
            {
                EnableDoubleBufferingRecursive(child);
            }
        }

        private static void ApplyThemeRecursive(Control control)
        {
            if (control is UserControl uc)
            {
                uc.BackColor = ColorBackground;
                uc.ForeColor = ColorText;
            }

            // Apply theme colors depending on the control type
            if (control is Panel panel)
            {
                if (panel.Tag?.ToString() == "Card")
                {
                    panel.BackColor = ColorCard;
                }
                else if (panel.Tag?.ToString() == "Background")
                {
                    panel.BackColor = ColorBackground;
                }
                else
                {
                    panel.BackColor = ColorBackground; // Default all panels to background color
                }
            }
            else if (control is Label label)
            {
                label.ForeColor = label.Tag?.ToString() == "Muted" ? ColorTextMuted : ColorText;
                if (label.Tag?.ToString() == "Title")
                {
                    label.Font = FontTitle;
                    label.ForeColor = ColorAccent;
                }
                else if (label.Tag?.ToString() == "Header")
                {
                    label.Font = FontHeader;
                }
                else if (label.Tag?.ToString() == "Success")
                {
                    label.ForeColor = ColorSuccess;
                }
                else if (label.Tag?.ToString() == "Danger")
                {
                    label.ForeColor = ColorDanger;
                }
                else
                {
                    label.Font = FontBody;
                }
            }
            else if (control is Button button)
            {
                button.Font = FontBodyBold;
                button.FlatStyle = FlatStyle.Flat;
                button.Cursor = Cursors.Hand;

                if (button.Tag?.ToString() == "Primary")
                {
                    button.FlatAppearance.BorderSize = 1;
                    button.BackColor = ColorAccent;
                    button.ForeColor = Color.Black;
                    button.FlatAppearance.BorderColor = ColorAccent;
                    button.FlatAppearance.MouseOverBackColor = ColorAccentHover;
                    button.FlatAppearance.MouseDownBackColor = ColorAccentPressed;
                }
                else if (button.Tag?.ToString() == "Success")
                {
                    button.FlatAppearance.BorderSize = 1;
                    button.BackColor = ColorSuccess;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = ColorSuccess;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 190, 94);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 150, 64);
                }
                else if (button.Tag?.ToString() == "Danger")
                {
                    button.FlatAppearance.BorderSize = 1;
                    button.BackColor = Color.FromArgb(40, ColorDanger);
                    button.ForeColor = ColorDanger;
                    button.FlatAppearance.BorderColor = Color.FromArgb(80, ColorDanger);
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, ColorDanger);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, ColorDanger);
                }
                else if (button.Tag?.ToString() == "Secondary")
                {
                    button.FlatAppearance.BorderSize = 1;
                    button.BackColor = ColorFieldBg; // Distinct from ColorCard
                    button.ForeColor = ColorText;
                    button.FlatAppearance.BorderColor = ColorBorder;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 48, 52); // Premium lighter gray hover
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 22);
                }
                else if (button.Tag?.ToString() == "Nav")
                {
                    // Navigation sidebar buttons. Let MainForm control active state background, 
                    // but enforce flat presentation and hover styles here.
                    button.FlatAppearance.BorderSize = 0;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, ColorAccent);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, ColorAccent);
                }
                else
                {
                    button.FlatAppearance.BorderSize = 1;
                    button.BackColor = ColorFieldBg;
                    button.ForeColor = ColorText;
                    button.FlatAppearance.BorderColor = ColorBorder;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 48, 52);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 20, 22);
                }
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.FlatStyle = FlatStyle.Flat;
                checkBox.ForeColor = ColorText;
                checkBox.BackColor = Color.Transparent;
                checkBox.FlatAppearance.BorderColor = ColorBorder;
                checkBox.FlatAppearance.CheckedBackColor = ColorAccent;
                checkBox.FlatAppearance.MouseDownBackColor = ColorAccentPressed;
                checkBox.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, ColorAccent);
            }
            else if (control is TextBox textBox)
            {
                textBox.BackColor = ColorFieldBg;
                textBox.ForeColor = ColorText;
                textBox.Font = FontBody;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = ColorFieldBg;
                comboBox.ForeColor = ColorText;
                comboBox.Font = FontBody;
                if (comboBox is not FlatComboBox)
                {
                    comboBox.FlatStyle = FlatStyle.Flat;
                }
            }
            else if (control is DateTimePicker dateTimePicker)
            {
                dateTimePicker.BackColor = ColorFieldBg;
                dateTimePicker.ForeColor = ColorText;
                dateTimePicker.Font = FontBody;
            }
            else if (control is ListBox listBox)
            {
                listBox.BackColor = ColorFieldBg;
                listBox.ForeColor = ColorText;
                listBox.Font = FontBody;
                listBox.BorderStyle = BorderStyle.None;
            }
            else if (control is FlatListBox flatListBox)
            {
                flatListBox.BackColor = ColorFieldBg;
                flatListBox.ForeColor = ColorText;
                flatListBox.Font = FontBody;
            }
            else if (control is DataGridView dataGridView)
            {
                dataGridView.BackgroundColor = ColorBackground;
                dataGridView.GridColor = Color.FromArgb(28, 28, 30);
                dataGridView.BorderStyle = BorderStyle.None;
                dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                
                // Column headers style
                dataGridView.EnableHeadersVisualStyles = false;
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = ColorCard;
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = ColorAccent;
                dataGridView.ColumnHeadersDefaultCellStyle.Font = FontHeader;
                dataGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorCard;
                
                // Row cells style
                dataGridView.DefaultCellStyle.BackColor = ColorBackground;
                dataGridView.DefaultCellStyle.ForeColor = ColorText;
                dataGridView.DefaultCellStyle.Font = FontBody;
                dataGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(40, ColorAccent);
                dataGridView.DefaultCellStyle.SelectionForeColor = ColorText;
                
                // Alternate row
                dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 22);
                dataGridView.AlternatingRowsDefaultCellStyle.ForeColor = ColorText;

                dataGridView.RowHeadersVisible = false;
                dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView.MultiSelect = true;
                dataGridView.AllowUserToAddRows = false;
                dataGridView.AllowUserToDeleteRows = false;
                dataGridView.ReadOnly = true;
            }

            // Recursive call for children
            foreach (Control child in control.Controls)
            {
                ApplyThemeRecursive(child);
            }
        }
    }
}
