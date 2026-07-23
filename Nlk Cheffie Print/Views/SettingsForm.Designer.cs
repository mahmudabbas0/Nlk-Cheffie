namespace Nlk_Cheffie_Print.Views
{
    partial class SettingsForm
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
            this.pnlGeneralCard = new System.Windows.Forms.Panel();
            this.cmbLanguage = new Nlk_Cheffie_Print.Core.FlatComboBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.cmbAutoPrintRole = new Nlk_Cheffie_Print.Core.FlatComboBox();
            this.lblAutoPrintRole = new System.Windows.Forms.Label();
            this.chkAutoPrint = new Nlk_Cheffie_Print.Core.FlatCheckBox();
            this.chkGraphicMode = new Nlk_Cheffie_Print.Core.FlatCheckBox();
            this.chkDryRun = new Nlk_Cheffie_Print.Core.FlatCheckBox();
            this.chkPopupNotifications = new Nlk_Cheffie_Print.Core.FlatCheckBox();
            this.chkOrderSound = new Nlk_Cheffie_Print.Core.FlatCheckBox();
            this.chkPrinterBuzzer = new Nlk_Cheffie_Print.Core.FlatCheckBox();
            this.lblGeneralTitle = new System.Windows.Forms.Label();
            this.pnlConnectionCard = new System.Windows.Forms.Panel();
            this.btnResetConnection = new System.Windows.Forms.Button();
            this.txtApiUrl = new System.Windows.Forms.TextBox();
            this.lblServerUrl = new System.Windows.Forms.Label();
            this.lblConnectionTitle = new System.Windows.Forms.Label();
            this.pnlAppCard = new System.Windows.Forms.Panel();
            this.btnQuit = new System.Windows.Forms.Button();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.pnlGeneralCard.SuspendLayout();
            this.pnlConnectionCard.SuspendLayout();
            this.pnlAppCard.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkPopupNotifications
            // 
            this.chkPopupNotifications.AutoSize = true;
            this.chkPopupNotifications.Location = new System.Drawing.Point(15, 122);
            this.chkPopupNotifications.Name = "chkPopupNotifications";
            this.chkPopupNotifications.Size = new System.Drawing.Size(240, 19);
            this.chkPopupNotifications.TabIndex = 4;
            this.chkPopupNotifications.Text = "Yeni sipariş pop-up bildirimi göster";
            this.chkPopupNotifications.UseVisualStyleBackColor = true;
            // 
            // chkOrderSound
            // 
            this.chkOrderSound.AutoSize = true;
            this.chkOrderSound.Location = new System.Drawing.Point(15, 149);
            this.chkOrderSound.Name = "chkOrderSound";
            this.chkOrderSound.Size = new System.Drawing.Size(220, 19);
            this.chkOrderSound.TabIndex = 5;
            this.chkOrderSound.Text = "Sipariş gelince sesli ikaz çal";
            this.chkOrderSound.UseVisualStyleBackColor = true;
            // 
            // chkPrinterBuzzer
            // 
            this.chkPrinterBuzzer.AutoSize = true;
            this.chkPrinterBuzzer.Location = new System.Drawing.Point(15, 176);
            this.chkPrinterBuzzer.Name = "chkPrinterBuzzer";
            this.chkPrinterBuzzer.Size = new System.Drawing.Size(250, 19);
            this.chkPrinterBuzzer.TabIndex = 6;
            this.chkPrinterBuzzer.Text = "Yazıcı zili / bip sesini çal (Buzzer)";
            this.chkPrinterBuzzer.UseVisualStyleBackColor = true;
            // 
            // pnlGeneralCard
            // 
            this.pnlGeneralCard.Controls.Add(this.chkPrinterBuzzer);
            this.pnlGeneralCard.Controls.Add(this.chkOrderSound);
            this.pnlGeneralCard.Controls.Add(this.chkPopupNotifications);
            this.pnlGeneralCard.Controls.Add(this.cmbLanguage);
            this.pnlGeneralCard.Controls.Add(this.lblLanguage);
            this.pnlGeneralCard.Controls.Add(this.cmbAutoPrintRole);
            this.pnlGeneralCard.Controls.Add(this.lblAutoPrintRole);
            this.pnlGeneralCard.Controls.Add(this.chkAutoPrint);
            this.pnlGeneralCard.Controls.Add(this.chkGraphicMode);
            this.pnlGeneralCard.Controls.Add(this.chkDryRun);
            this.pnlGeneralCard.Controls.Add(this.lblGeneralTitle);
            this.pnlGeneralCard.Location = new System.Drawing.Point(20, 20);
            this.pnlGeneralCard.Name = "pnlGeneralCard";
            this.pnlGeneralCard.Size = new System.Drawing.Size(460, 260);
            this.pnlGeneralCard.TabIndex = 0;
            this.pnlGeneralCard.Tag = "Card";
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Location = new System.Drawing.Point(250, 220);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(190, 23);
            this.cmbLanguage.TabIndex = 8;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblLanguage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblLanguage.Location = new System.Drawing.Point(250, 200);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(100, 15);
            this.lblLanguage.TabIndex = 7;
            this.lblLanguage.Text = "Language / Dil:";
            // 
            // cmbAutoPrintRole
            // 
            this.cmbAutoPrintRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAutoPrintRole.FormattingEnabled = true;
            this.cmbAutoPrintRole.Location = new System.Drawing.Point(15, 220);
            this.cmbAutoPrintRole.Name = "cmbAutoPrintRole";
            this.cmbAutoPrintRole.Size = new System.Drawing.Size(190, 23);
            this.cmbAutoPrintRole.TabIndex = 7;
            // 
            // lblAutoPrintRole
            // 
            this.lblAutoPrintRole.AutoSize = true;
            this.lblAutoPrintRole.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblAutoPrintRole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblAutoPrintRole.Location = new System.Drawing.Point(15, 200);
            this.lblAutoPrintRole.Name = "lblAutoPrintRole";
            this.lblAutoPrintRole.Size = new System.Drawing.Size(105, 15);
            this.lblAutoPrintRole.TabIndex = 6;
            this.lblAutoPrintRole.Text = "Otomatik Fiş Tipi:";
            // 
            // chkAutoPrint
            // 
            this.chkAutoPrint.AutoSize = true;
            this.chkAutoPrint.Location = new System.Drawing.Point(15, 95);
            this.chkAutoPrint.Name = "chkAutoPrint";
            this.chkAutoPrint.Size = new System.Drawing.Size(225, 19);
            this.chkAutoPrint.TabIndex = 3;
            this.chkAutoPrint.Text = "Otomatik fatura yazdır (yeni sipariş gelince)";
            this.chkAutoPrint.UseVisualStyleBackColor = true;
            // 
            // chkGraphicMode
            // 
            this.chkGraphicMode.AutoSize = true;
            this.chkGraphicMode.Location = new System.Drawing.Point(15, 68);
            this.chkGraphicMode.Name = "chkGraphicMode";
            this.chkGraphicMode.Size = new System.Drawing.Size(262, 19);
            this.chkGraphicMode.TabIndex = 2;
            this.chkGraphicMode.Text = "Grafik mod (bitmap baskı, Türkçe karakter sorunsuz)";
            this.chkGraphicMode.UseVisualStyleBackColor = true;
            // 
            // chkDryRun
            // 
            this.chkDryRun.AutoSize = true;
            this.chkDryRun.Location = new System.Drawing.Point(15, 41);
            this.chkDryRun.Name = "chkDryRun";
            this.chkDryRun.Size = new System.Drawing.Size(271, 19);
            this.chkDryRun.TabIndex = 1;
            this.chkDryRun.Text = "Deneme modu (fişleri kuyruğa yaz, gerçek yazıcıya gönderme)";
            this.chkDryRun.UseVisualStyleBackColor = true;
            // 
            // lblGeneralTitle
            // 
            this.lblGeneralTitle.AutoSize = true;
            this.lblGeneralTitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblGeneralTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblGeneralTitle.Location = new System.Drawing.Point(15, 15);
            this.lblGeneralTitle.Name = "lblGeneralTitle";
            this.lblGeneralTitle.Size = new System.Drawing.Size(43, 15);
            this.lblGeneralTitle.TabIndex = 0;
            this.lblGeneralTitle.Tag = "Title";
            this.lblGeneralTitle.Text = "GENEL";
            // 
            // pnlConnectionCard
            // 
            this.pnlConnectionCard.Controls.Add(this.btnResetConnection);
            this.pnlConnectionCard.Controls.Add(this.txtApiUrl);
            this.pnlConnectionCard.Controls.Add(this.lblServerUrl);
            this.pnlConnectionCard.Controls.Add(this.lblConnectionTitle);
            this.pnlConnectionCard.Location = new System.Drawing.Point(20, 290);
            this.pnlConnectionCard.Name = "pnlConnectionCard";
            this.pnlConnectionCard.Size = new System.Drawing.Size(460, 130);
            this.pnlConnectionCard.TabIndex = 1;
            this.pnlConnectionCard.Tag = "Card";
            // 
            // btnResetConnection
            // 
            this.btnResetConnection.Location = new System.Drawing.Point(15, 85);
            this.btnResetConnection.Name = "btnResetConnection";
            this.btnResetConnection.Size = new System.Drawing.Size(230, 32);
            this.btnResetConnection.TabIndex = 3;
            this.btnResetConnection.Tag = "Danger";
            this.btnResetConnection.Text = "Restoran Bağlantısını Sıfırla";
            this.btnResetConnection.UseVisualStyleBackColor = true;
            this.btnResetConnection.Click += new System.EventHandler(this.btnResetConnection_Click);
            // 
            // txtApiUrl
            // 
            this.txtApiUrl.Location = new System.Drawing.Point(135, 47);
            this.txtApiUrl.Name = "txtApiUrl";
            this.txtApiUrl.Size = new System.Drawing.Size(305, 23);
            this.txtApiUrl.TabIndex = 2;
            // 
            // lblServerUrl
            // 
            this.lblServerUrl.AutoSize = true;
            this.lblServerUrl.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblServerUrl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblServerUrl.Location = new System.Drawing.Point(15, 50);
            this.lblServerUrl.Name = "lblServerUrl";
            this.lblServerUrl.Size = new System.Drawing.Size(96, 15);
            this.lblServerUrl.TabIndex = 1;
            this.lblServerUrl.Text = "Sunucu API URL:";
            // 
            // lblConnectionTitle
            // 
            this.lblConnectionTitle.AutoSize = true;
            this.lblConnectionTitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblConnectionTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblConnectionTitle.Location = new System.Drawing.Point(15, 15);
            this.lblConnectionTitle.Name = "lblConnectionTitle";
            this.lblConnectionTitle.Size = new System.Drawing.Size(65, 15);
            this.lblConnectionTitle.TabIndex = 0;
            this.lblConnectionTitle.Tag = "Title";
            this.lblConnectionTitle.Text = "BAĞLANTI";
            // 
            // pnlAppCard
            // 
            this.pnlAppCard.Controls.Add(this.btnQuit);
            this.pnlAppCard.Controls.Add(this.lblAppTitle);
            this.pnlAppCard.Location = new System.Drawing.Point(20, 430);
            this.pnlAppCard.Name = "pnlAppCard";
            this.pnlAppCard.Size = new System.Drawing.Size(460, 90);
            this.pnlAppCard.TabIndex = 2;
            this.pnlAppCard.Tag = "Card";
            // 
            // btnQuit
            // 
            this.btnQuit.Location = new System.Drawing.Point(15, 45);
            this.btnQuit.Name = "btnQuit";
            this.btnQuit.Size = new System.Drawing.Size(150, 32);
            this.btnQuit.TabIndex = 1;
            this.btnQuit.Tag = "Secondary";
            this.btnQuit.Text = "Uygulamadan Çık";
            this.btnQuit.UseVisualStyleBackColor = true;
            this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
            // 
            // lblAppTitle
            // 
            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblAppTitle.Location = new System.Drawing.Point(15, 15);
            this.lblAppTitle.Name = "lblAppTitle";
            this.lblAppTitle.Size = new System.Drawing.Size(73, 15);
            this.lblAppTitle.TabIndex = 0;
            this.lblAppTitle.Tag = "Title";
            this.lblAppTitle.Text = "UYGULAMA";
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.btnCancel);
            this.pnlActions.Controls.Add(this.btnOk);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActions.Location = new System.Drawing.Point(0, 535);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new System.Drawing.Size(500, 50);
            this.pnlActions.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(270, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "Secondary";
            this.btnCancel.Text = "İptal";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(385, 8);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(100, 32);
            this.btnOk.TabIndex = 0;
            this.btnOk.Tag = "Primary";
            this.btnOk.Text = "Tamam";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 585);
            this.Controls.Add(this.pnlActions);
            this.Controls.Add(this.pnlAppCard);
            this.Controls.Add(this.pnlConnectionCard);
            this.Controls.Add(this.pnlGeneralCard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ayarlar";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.pnlGeneralCard.ResumeLayout(false);
            this.pnlGeneralCard.PerformLayout();
            this.pnlConnectionCard.ResumeLayout(false);
            this.pnlConnectionCard.PerformLayout();
            this.pnlAppCard.ResumeLayout(false);
            this.pnlAppCard.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlGeneralCard;
        private System.Windows.Forms.Label lblGeneralTitle;
        private Nlk_Cheffie_Print.Core.FlatCheckBox chkDryRun;
        private Nlk_Cheffie_Print.Core.FlatCheckBox chkGraphicMode;
        private Nlk_Cheffie_Print.Core.FlatCheckBox chkAutoPrint;
        private Nlk_Cheffie_Print.Core.FlatCheckBox chkPopupNotifications;
        private Nlk_Cheffie_Print.Core.FlatCheckBox chkOrderSound;
        private Nlk_Cheffie_Print.Core.FlatCheckBox chkPrinterBuzzer;
        private System.Windows.Forms.Label lblAutoPrintRole;
        private Nlk_Cheffie_Print.Core.FlatComboBox cmbAutoPrintRole;
        private System.Windows.Forms.Label lblLanguage;
        private Nlk_Cheffie_Print.Core.FlatComboBox cmbLanguage;
        private System.Windows.Forms.Panel pnlConnectionCard;
        private System.Windows.Forms.Label lblConnectionTitle;
        private System.Windows.Forms.Label lblServerUrl;
        private System.Windows.Forms.TextBox txtApiUrl;
        private System.Windows.Forms.Button btnResetConnection;
        private System.Windows.Forms.Panel pnlAppCard;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Button btnQuit;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
    }
}
