namespace Nlk_Cheffie_Print.Views.Controls
{
    partial class PrintersControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pnlPrintersCard = new System.Windows.Forms.Panel();
            this.lstPrinters = new System.Windows.Forms.ListBox();
            this.pnlPrinterActions = new System.Windows.Forms.Panel();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnProfile = new System.Windows.Forms.Button();
            this.btnAddIp = new System.Windows.Forms.Button();
            this.btnScanUsb = new System.Windows.Forms.Button();
            this.pnlRolesCard = new System.Windows.Forms.Panel();
            this.btnSaveRoles = new System.Windows.Forms.Button();
            this.btnTestCourier = new System.Windows.Forms.Button();
            this.btnTestCashier = new System.Windows.Forms.Button();
            this.btnTestKitchen = new System.Windows.Forms.Button();
            this.cmbCourier = new Nlk_Cheffie_Print.Core.FlatComboBox();
            this.lblRoleCourier = new System.Windows.Forms.Label();
            this.cmbCashier = new Nlk_Cheffie_Print.Core.FlatComboBox();
            this.lblRoleCashier = new System.Windows.Forms.Label();
            this.cmbKitchen = new Nlk_Cheffie_Print.Core.FlatComboBox();
            this.lblRoleKitchen = new System.Windows.Forms.Label();
            this.lblRolesTitle = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlPrintersCard.SuspendLayout();
            this.pnlPrinterActions.SuspendLayout();
            this.pnlRolesCard.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(800, 55);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5f);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblSubtitle.Location = new System.Drawing.Point(540, 19);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(240, 15);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Tag = "Muted";
            this.lblSubtitle.Text = "Add USB and IP printers, assign roles.";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 11.5f, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(183, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Tag = "Header";
            this.lblTitle.Text = "YAZICILAR VE ROLLER";
            // 
            // pnlPrintersCard
            // 
            this.pnlPrintersCard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlPrintersCard.Controls.Add(this.lstPrinters);
            this.pnlPrintersCard.Controls.Add(this.pnlPrinterActions);
            this.pnlPrintersCard.Location = new System.Drawing.Point(15, 65);
            this.pnlPrintersCard.Name = "pnlPrintersCard";
            this.pnlPrintersCard.Size = new System.Drawing.Size(770, 200);
            this.pnlPrintersCard.TabIndex = 1;
            this.pnlPrintersCard.Tag = "Card";
            // 
            // lstPrinters
            // 
            this.lstPrinters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstPrinters.Location = new System.Drawing.Point(0, 45);
            this.lstPrinters.Name = "lstPrinters";
            this.lstPrinters.Size = new System.Drawing.Size(770, 155);
            this.lstPrinters.TabIndex = 1;
            // 
            // pnlPrinterActions
            // 
            this.pnlPrinterActions.Controls.Add(this.btnRemove);
            this.pnlPrinterActions.Controls.Add(this.btnProfile);
            this.pnlPrinterActions.Controls.Add(this.btnAddIp);
            this.pnlPrinterActions.Controls.Add(this.btnScanUsb);
            this.pnlPrinterActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPrinterActions.Location = new System.Drawing.Point(0, 0);
            this.pnlPrinterActions.Name = "pnlPrinterActions";
            this.pnlPrinterActions.Size = new System.Drawing.Size(770, 45);
            this.pnlPrinterActions.TabIndex = 0;
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(235, 8);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(100, 30);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Tag = "Danger";
            this.btnRemove.Text = " Sil";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnProfile
            // 
            this.btnProfile.Location = new System.Drawing.Point(345, 8);
            this.btnProfile.Name = "btnProfile";
            this.btnProfile.Size = new System.Drawing.Size(145, 30);
            this.btnProfile.TabIndex = 3;
            this.btnProfile.Tag = "Secondary";
            this.btnProfile.Text = "Yazıcı Profili";
            this.btnProfile.UseVisualStyleBackColor = true;
            this.btnProfile.Click += new System.EventHandler(this.btnProfile_Click);
            // 
            // btnAddIp
            // 
            this.btnAddIp.Location = new System.Drawing.Point(125, 8);
            this.btnAddIp.Name = "btnAddIp";
            this.btnAddIp.Size = new System.Drawing.Size(100, 30);
            this.btnAddIp.TabIndex = 1;
            this.btnAddIp.Tag = "Secondary";
            this.btnAddIp.Text = " IP Ekle";
            this.btnAddIp.UseVisualStyleBackColor = true;
            this.btnAddIp.Click += new System.EventHandler(this.btnAddIp_Click);
            // 
            // btnScanUsb
            // 
            this.btnScanUsb.Location = new System.Drawing.Point(15, 8);
            this.btnScanUsb.Name = "btnScanUsb";
            this.btnScanUsb.Size = new System.Drawing.Size(100, 30);
            this.btnScanUsb.TabIndex = 0;
            this.btnScanUsb.Tag = "Primary";
            this.btnScanUsb.Text = " USB Tara";
            this.btnScanUsb.UseVisualStyleBackColor = true;
            this.btnScanUsb.Click += new System.EventHandler(this.btnScanUsb_Click);
            // 
            // pnlRolesCard
            // 
            this.pnlRolesCard.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlRolesCard.Controls.Add(this.btnSaveRoles);
            this.pnlRolesCard.Controls.Add(this.btnTestCourier);
            this.pnlRolesCard.Controls.Add(this.btnTestCashier);
            this.pnlRolesCard.Controls.Add(this.btnTestKitchen);
            this.pnlRolesCard.Controls.Add(this.cmbCourier);
            this.pnlRolesCard.Controls.Add(this.lblRoleCourier);
            this.pnlRolesCard.Controls.Add(this.cmbCashier);
            this.pnlRolesCard.Controls.Add(this.lblRoleCashier);
            this.pnlRolesCard.Controls.Add(this.cmbKitchen);
            this.pnlRolesCard.Controls.Add(this.lblRoleKitchen);
            this.pnlRolesCard.Controls.Add(this.lblRolesTitle);
            this.pnlRolesCard.Location = new System.Drawing.Point(15, 280);
            this.pnlRolesCard.Name = "pnlRolesCard";
            this.pnlRolesCard.Size = new System.Drawing.Size(770, 205);
            this.pnlRolesCard.TabIndex = 2;
            this.pnlRolesCard.Tag = "Card";
            // 
            // btnSaveRoles
            // 
            this.btnSaveRoles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveRoles.Location = new System.Drawing.Point(535, 150);
            this.btnSaveRoles.Name = "btnSaveRoles";
            this.btnSaveRoles.Size = new System.Drawing.Size(220, 40);
            this.btnSaveRoles.TabIndex = 10;
            this.btnSaveRoles.Tag = "Primary";
            this.btnSaveRoles.Text = "ROL EŞLEMELERİNİ KAYDET";
            this.btnSaveRoles.UseVisualStyleBackColor = true;
            this.btnSaveRoles.Click += new System.EventHandler(this.btnSaveRoles_Click);
            // 
            // btnTestCourier
            // 
            this.btnTestCourier.Location = new System.Drawing.Point(355, 150);
            this.btnTestCourier.Name = "btnTestCourier";
            this.btnTestCourier.Size = new System.Drawing.Size(150, 35);
            this.btnTestCourier.TabIndex = 9;
            this.btnTestCourier.Tag = "Secondary";
            this.btnTestCourier.Text = "KURYE TEST";
            this.btnTestCourier.UseVisualStyleBackColor = true;
            this.btnTestCourier.Click += new System.EventHandler(this.btnTestCourier_Click);
            // 
            // btnTestCashier
            // 
            this.btnTestCashier.Location = new System.Drawing.Point(185, 150);
            this.btnTestCashier.Name = "btnTestCashier";
            this.btnTestCashier.Size = new System.Drawing.Size(150, 35);
            this.btnTestCashier.TabIndex = 8;
            this.btnTestCashier.Tag = "Secondary";
            this.btnTestCashier.Text = "KASA TEST";
            this.btnTestCashier.UseVisualStyleBackColor = true;
            this.btnTestCashier.Click += new System.EventHandler(this.btnTestCashier_Click);
            // 
            // btnTestKitchen
            // 
            this.btnTestKitchen.Location = new System.Drawing.Point(15, 150);
            this.btnTestKitchen.Name = "btnTestKitchen";
            this.btnTestKitchen.Size = new System.Drawing.Size(150, 35);
            this.btnTestKitchen.TabIndex = 7;
            this.btnTestKitchen.Tag = "Secondary";
            this.btnTestKitchen.Text = "MUTFAK TEST";
            this.btnTestKitchen.UseVisualStyleBackColor = true;
            this.btnTestKitchen.Click += new System.EventHandler(this.btnTestKitchen_Click);
            // 
            // cmbCourier
            // 
            this.cmbCourier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCourier.FormattingEnabled = true;
            this.cmbCourier.Location = new System.Drawing.Point(535, 95);
            this.cmbCourier.Name = "cmbCourier";
            this.cmbCourier.Size = new System.Drawing.Size(220, 23);
            this.cmbCourier.TabIndex = 6;
            // 
            // lblRoleCourier
            // 
            this.lblRoleCourier.AutoSize = true;
            this.lblRoleCourier.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblRoleCourier.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblRoleCourier.Location = new System.Drawing.Point(535, 70);
            this.lblRoleCourier.Name = "lblRoleCourier";
            this.lblRoleCourier.Size = new System.Drawing.Size(117, 15);
            this.lblRoleCourier.TabIndex = 5;
            this.lblRoleCourier.Text = "KURYE YAZICISI:";
            // 
            // cmbCashier
            // 
            this.cmbCashier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCashier.FormattingEnabled = true;
            this.cmbCashier.Location = new System.Drawing.Point(275, 95);
            this.cmbCashier.Name = "cmbCashier";
            this.cmbCashier.Size = new System.Drawing.Size(220, 23);
            this.cmbCashier.TabIndex = 4;
            // 
            // lblRoleCashier
            // 
            this.lblRoleCashier.AutoSize = true;
            this.lblRoleCashier.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblRoleCashier.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblRoleCashier.Location = new System.Drawing.Point(275, 70);
            this.lblRoleCashier.Name = "lblRoleCashier";
            this.lblRoleCashier.Size = new System.Drawing.Size(117, 15);
            this.lblRoleCashier.TabIndex = 3;
            this.lblRoleCashier.Text = "KASA YAZICISI:";
            // 
            // cmbKitchen
            // 
            this.cmbKitchen.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKitchen.FormattingEnabled = true;
            this.cmbKitchen.Location = new System.Drawing.Point(15, 95);
            this.cmbKitchen.Name = "cmbKitchen";
            this.cmbKitchen.Size = new System.Drawing.Size(220, 23);
            this.cmbKitchen.TabIndex = 2;
            // 
            // lblRoleKitchen
            // 
            this.lblRoleKitchen.AutoSize = true;
            this.lblRoleKitchen.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblRoleKitchen.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblRoleKitchen.Location = new System.Drawing.Point(15, 70);
            this.lblRoleKitchen.Name = "lblRoleKitchen";
            this.lblRoleKitchen.Size = new System.Drawing.Size(127, 15);
            this.lblRoleKitchen.TabIndex = 1;
            this.lblRoleKitchen.Text = "MUTFAK YAZICISI:";
            // 
            // lblRolesTitle
            // 
            this.lblRolesTitle.AutoSize = true;
            this.lblRolesTitle.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblRolesTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblRolesTitle.Location = new System.Drawing.Point(15, 15);
            this.lblRolesTitle.Name = "lblRolesTitle";
            this.lblRolesTitle.Size = new System.Drawing.Size(107, 15);
            this.lblRolesTitle.TabIndex = 0;
            this.lblRolesTitle.Text = "ROL ATAMALARI";
            // 
            // PrintersControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlRolesCard);
            this.Controls.Add(this.pnlPrintersCard);
            this.Controls.Add(this.pnlHeader);
            this.Name = "PrintersControl";
            this.Size = new System.Drawing.Size(800, 500);
            this.Load += new System.EventHandler(this.PrintersControl_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlPrintersCard.ResumeLayout(false);
            this.pnlPrinterActions.ResumeLayout(false);
            this.pnlRolesCard.ResumeLayout(false);
            this.pnlRolesCard.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel pnlPrintersCard;
        private System.Windows.Forms.Panel pnlPrinterActions;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnProfile;
        private System.Windows.Forms.Button btnAddIp;
        private System.Windows.Forms.Button btnScanUsb;
        private System.Windows.Forms.ListBox lstPrinters;
        private System.Windows.Forms.Panel pnlRolesCard;
        private System.Windows.Forms.Label lblRolesTitle;
        private System.Windows.Forms.Label lblRoleKitchen;
        private Nlk_Cheffie_Print.Core.FlatComboBox cmbKitchen;
        private Nlk_Cheffie_Print.Core.FlatComboBox cmbCourier;
        private System.Windows.Forms.Label lblRoleCourier;
        private Nlk_Cheffie_Print.Core.FlatComboBox cmbCashier;
        private System.Windows.Forms.Label lblRoleCashier;
        private System.Windows.Forms.Button btnSaveRoles;
        private System.Windows.Forms.Button btnTestCourier;
        private System.Windows.Forms.Button btnTestCashier;
        private System.Windows.Forms.Button btnTestKitchen;
    }
}
