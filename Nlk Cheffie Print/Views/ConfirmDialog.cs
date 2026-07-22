using System;
using System.Drawing;
using System.Windows.Forms;
using Nlk_Cheffie_Print.Core;

namespace Nlk_Cheffie_Print.Views
{
    public class ConfirmDialog : Form
    {
        private Panel pnlTitleBar;
        private Label lblTitle;
        private Label lblMessage;
        private Button btnYes;
        private Button btnNo;
        private Panel pnlContent;
        private FlowLayoutPanel flowButtons;

        public ConfirmDialog(string title, string message)
        {
            // Form Setup
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(380, 160);
            this.BackColor = ThemeManager.ColorBackground;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;

            // Apply RightToLeft layout dynamically if language is Arabic
            bool isRtl = LocalizationService.CurrentLanguage.ToLower() == "ar";
            this.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;
            this.RightToLeftLayout = isRtl;

            // Title Bar
            pnlTitleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = ThemeManager.ColorCard
            };

            lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 12, 0),
                Font = ThemeManager.FontBodyBold,
                ForeColor = ThemeManager.ColorAccent
            };
            pnlTitleBar.Controls.Add(lblTitle);

            // Content Panel
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeManager.ColorBackground,
                Padding = new Padding(20, 16, 20, 12)
            };

            lblMessage = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                Font = ThemeManager.FontBody,
                ForeColor = ThemeManager.ColorText,
                TextAlign = ContentAlignment.TopLeft
            };
            pnlContent.Controls.Add(lblMessage);

            // Action Panel (Buttons)
            Panel pnlButtons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            flowButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = isRtl ? FlowDirection.LeftToRight : FlowDirection.RightToLeft,
                BackColor = Color.Transparent
            };

            btnNo = new Button
            {
                Text = LocalizationService.T("designer.no", "Hayır"),
                DialogResult = DialogResult.No,
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontBodyBold,
                BackColor = ThemeManager.ColorCard,
                ForeColor = ThemeManager.ColorTextMuted,
                Cursor = Cursors.Hand,
                Margin = new Padding(6, 0, 6, 0)
            };
            btnNo.FlatAppearance.BorderSize = 0;
            btnNo.FlatAppearance.MouseOverBackColor = Color.FromArgb(44, 44, 46);

            btnYes = new Button
            {
                Text = LocalizationService.T("designer.yes", "Evet"),
                DialogResult = DialogResult.Yes,
                Width = 90,
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeManager.FontBodyBold,
                BackColor = ThemeManager.ColorAccent,
                ForeColor = Color.Black,
                Cursor = Cursors.Hand,
                Margin = new Padding(6, 0, 6, 0)
            };
            btnYes.FlatAppearance.BorderSize = 0;
            btnYes.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 180, 0);

            flowButtons.Controls.Add(btnNo);
            flowButtons.Controls.Add(btnYes);
            pnlButtons.Controls.Add(flowButtons);
            pnlContent.Controls.Add(pnlButtons);

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlTitleBar);

            // Border Glow Painting
            this.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.ColorBorder, 1.5f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };
        }

        public static DialogResult Show(IWin32Window owner, string title, string message)
        {
            using (var dlg = new ConfirmDialog(title, message))
            {
                return dlg.ShowDialog(owner);
            }
        }
    }
}
