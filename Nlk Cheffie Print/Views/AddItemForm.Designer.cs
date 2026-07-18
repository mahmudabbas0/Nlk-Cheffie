namespace Nlk_Cheffie_Print.Views
{
    partial class AddItemForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitleBasic = new Label();
            flpBasic = new FlowLayoutPanel();
            btnText = new Button();
            btnSeparator = new Button();
            btnQrCode = new Button();
            btnLogo = new Button();
            btnBarcode = new Button();
            btnItems = new Button();
            lblTitleVars = new Label();
            flpVars = new FlowLayoutPanel();
            pnlButtons = new Panel();
            btnCancel = new Button();
            flpBasic.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitleBasic
            // 
            lblTitleBasic.AutoSize = true;
            lblTitleBasic.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTitleBasic.ForeColor = Color.FromArgb(255, 152, 0);
            lblTitleBasic.Location = new Point(20, 18);
            lblTitleBasic.Name = "lblTitleBasic";
            lblTitleBasic.Size = new Size(90, 17);
            lblTitleBasic.TabIndex = 0;
            lblTitleBasic.Tag = "Title";
            lblTitleBasic.Text = "Temel Öğeler";
            // 
            // flpBasic
            // 
            flpBasic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flpBasic.Controls.Add(btnText);
            flpBasic.Controls.Add(btnSeparator);
            flpBasic.Controls.Add(btnQrCode);
            flpBasic.Controls.Add(btnLogo);
            flpBasic.Controls.Add(btnBarcode);
            flpBasic.Controls.Add(btnItems);
            flpBasic.Location = new Point(20, 42);
            flpBasic.Name = "flpBasic";
            flpBasic.Size = new Size(820, 52);
            flpBasic.TabIndex = 1;
            // 
            // btnText
            // 
            btnText.Location = new Point(3, 3);
            btnText.Name = "btnText";
            btnText.Size = new Size(130, 44);
            btnText.TabIndex = 0;
            btnText.Tag = "Secondary";
            btnText.Text = "Metin";
            btnText.UseVisualStyleBackColor = true;
            btnText.Click += btnText_Click;
            // 
            // btnSeparator
            // 
            btnSeparator.Location = new Point(139, 3);
            btnSeparator.Name = "btnSeparator";
            btnSeparator.Size = new Size(130, 44);
            btnSeparator.TabIndex = 1;
            btnSeparator.Tag = "Secondary";
            btnSeparator.Text = "Ayraç";
            btnSeparator.UseVisualStyleBackColor = true;
            btnSeparator.Click += btnSeparator_Click;
            // 
            // btnQrCode
            // 
            btnQrCode.Location = new Point(275, 3);
            btnQrCode.Name = "btnQrCode";
            btnQrCode.Size = new Size(130, 44);
            btnQrCode.TabIndex = 2;
            btnQrCode.Tag = "Secondary";
            btnQrCode.Text = "QR Kodu";
            btnQrCode.UseVisualStyleBackColor = true;
            btnQrCode.Click += btnQrCode_Click;
            // 
            // btnLogo
            // 
            btnLogo.Location = new Point(411, 3);
            btnLogo.Name = "btnLogo";
            btnLogo.Size = new Size(130, 44);
            btnLogo.TabIndex = 3;
            btnLogo.Tag = "Secondary";
            btnLogo.Text = "Logo";
            btnLogo.UseVisualStyleBackColor = true;
            btnLogo.Click += btnLogo_Click;
            // 
            // btnBarcode
            // 
            btnBarcode.Location = new Point(547, 3);
            btnBarcode.Name = "btnBarcode";
            btnBarcode.Size = new Size(130, 44);
            btnBarcode.TabIndex = 4;
            btnBarcode.Tag = "Secondary";
            btnBarcode.Text = "Barkod";
            btnBarcode.UseVisualStyleBackColor = true;
            btnBarcode.Click += btnBarcode_Click;
            // 
            // btnItems
            // 
            btnItems.Location = new Point(683, 3);
            btnItems.Name = "btnItems";
            btnItems.Size = new Size(130, 44);
            btnItems.TabIndex = 5;
            btnItems.Tag = "Secondary";
            btnItems.Text = "Ürün Listesi";
            btnItems.UseVisualStyleBackColor = true;
            btnItems.Click += btnItems_Click;
            // 
            // lblTitleVars
            // 
            lblTitleVars.AutoSize = true;
            lblTitleVars.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTitleVars.ForeColor = Color.FromArgb(255, 152, 0);
            lblTitleVars.Location = new Point(20, 106);
            lblTitleVars.Name = "lblTitleVars";
            lblTitleVars.Size = new Size(143, 17);
            lblTitleVars.TabIndex = 2;
            lblTitleVars.Tag = "Title";
            lblTitleVars.Text = "Otomatik Değişkenler";
            // 
            // flpVars
            // 
            flpVars.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flpVars.AutoScroll = true;
            flpVars.Location = new Point(20, 130);
            flpVars.Name = "flpVars";
            flpVars.Padding = new Padding(0);
            flpVars.Size = new Size(820, 260);
            flpVars.TabIndex = 3;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 400);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(860, 50);
            pnlButtons.TabIndex = 4;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(745, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(95, 30);
            btnCancel.TabIndex = 0;
            btnCancel.Tag = "Secondary";
            btnCancel.Text = "İptal";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // AddItemForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(860, 450);
            Controls.Add(pnlButtons);
            Controls.Add(flpVars);
            Controls.Add(lblTitleVars);
            Controls.Add(flpBasic);
            Controls.Add(lblTitleBasic);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AddItemForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Yeni Öğe Ekle";
            Load += AddItemForm_Load;
            flpBasic.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitleBasic;
        private System.Windows.Forms.FlowLayoutPanel flpBasic;
        private System.Windows.Forms.Button btnText;
        private System.Windows.Forms.Button btnSeparator;
        private System.Windows.Forms.Button btnQrCode;
        private System.Windows.Forms.Button btnLogo;
        private System.Windows.Forms.Button btnBarcode;
        private System.Windows.Forms.Button btnItems;
        private System.Windows.Forms.Label lblTitleVars;
        private System.Windows.Forms.FlowLayoutPanel flpVars;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnCancel;
    }
}
