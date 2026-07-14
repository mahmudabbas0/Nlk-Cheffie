namespace Nlk_Cheffie_Print.Views
{
    partial class EditItemForm
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
            this.lblContent = new System.Windows.Forms.Label();
            this.txtContent = new System.Windows.Forms.TextBox();
            this.lblAlign = new System.Windows.Forms.Label();
            this.cmbAlign = new System.Windows.Forms.ComboBox();
            this.lblSize = new System.Windows.Forms.Label();
            this.cmbSize = new System.Windows.Forms.ComboBox();
            this.lblWeight = new System.Windows.Forms.Label();
            this.cmbFont = new System.Windows.Forms.ComboBox();
            this.lblFamily = new System.Windows.Forms.Label();
            this.cmbFamily = new System.Windows.Forms.ComboBox();
            this.pnlItemsSettings = new System.Windows.Forms.Panel();
            this.cmbShowTax = new System.Windows.Forms.ComboBox();
            this.lblShowTax = new System.Windows.Forms.Label();
            this.cmbShowNotes = new System.Windows.Forms.ComboBox();
            this.lblShowNotes = new System.Windows.Forms.Label();
            this.cmbShowDetails = new System.Windows.Forms.ComboBox();
            this.lblShowDetails = new System.Windows.Forms.Label();
            this.txtCurrencySymbol = new System.Windows.Forms.TextBox();
            this.lblCurrencySymbol = new System.Windows.Forms.Label();
            this.cmbRightAlignPrice = new System.Windows.Forms.ComboBox();
            this.lblRightAlignPrice = new System.Windows.Forms.Label();
            this.cmbShowPrice = new System.Windows.Forms.ComboBox();
            this.lblShowPrice = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.flpTokens = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTokensTitle = new System.Windows.Forms.Label();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.pnlItemsSettings.SuspendLayout();
            this.pnlActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblContent
            // 
            this.lblContent.AutoSize = true;
            this.lblContent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblContent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblContent.Location = new System.Drawing.Point(20, 20);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(43, 15);
            this.lblContent.TabIndex = 0;
            this.lblContent.Text = "İçerik:";
            // 
            // txtContent
            // 
            this.txtContent.Location = new System.Drawing.Point(20, 40);
            this.txtContent.Name = "txtContent";
            this.txtContent.Size = new System.Drawing.Size(320, 23);
            this.txtContent.TabIndex = 1;
            // 
            // lblAlign
            // 
            this.lblAlign.AutoSize = true;
            this.lblAlign.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblAlign.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblAlign.Location = new System.Drawing.Point(20, 80);
            this.lblAlign.Name = "lblAlign";
            this.lblAlign.Size = new System.Drawing.Size(60, 15);
            this.lblAlign.TabIndex = 2;
            this.lblAlign.Text = "Hizalama:";
            // 
            // cmbAlign
            // 
            this.cmbAlign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAlign.FormattingEnabled = true;
            this.cmbAlign.Location = new System.Drawing.Point(20, 100);
            this.cmbAlign.Name = "cmbAlign";
            this.cmbAlign.Size = new System.Drawing.Size(140, 23);
            this.cmbAlign.TabIndex = 3;
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblSize.Location = new System.Drawing.Point(180, 80);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(48, 15);
            this.lblSize.TabIndex = 4;
            this.lblSize.Text = "Boyut:";
            // 
            // cmbSize
            // 
            this.cmbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSize.FormattingEnabled = true;
            this.cmbSize.Location = new System.Drawing.Point(180, 100);
            this.cmbSize.Name = "cmbSize";
            this.cmbSize.Size = new System.Drawing.Size(140, 23);
            this.cmbSize.TabIndex = 5;
            // 
            // lblWeight
            // 
            this.lblWeight.AutoSize = true;
            this.lblWeight.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblWeight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblWeight.Location = new System.Drawing.Point(20, 140);
            this.lblWeight.Name = "lblWeight";
            this.lblWeight.Size = new System.Drawing.Size(53, 15);
            this.lblWeight.TabIndex = 6;
            this.lblWeight.Text = "Kalınlık:";
            // 
            // cmbFont
            // 
            this.cmbFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFont.FormattingEnabled = true;
            this.cmbFont.Location = new System.Drawing.Point(20, 160);
            this.cmbFont.Name = "cmbFont";
            this.cmbFont.Size = new System.Drawing.Size(140, 23);
            this.cmbFont.TabIndex = 7;
            // 
            // lblFamily
            // 
            this.lblFamily.AutoSize = true;
            this.lblFamily.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblFamily.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblFamily.Location = new System.Drawing.Point(180, 140);
            this.lblFamily.Name = "lblFamily";
            this.lblFamily.Size = new System.Drawing.Size(73, 15);
            this.lblFamily.TabIndex = 8;
            this.lblFamily.Text = "Font Ailesi:";
            // 
            // cmbFamily
            // 
            this.cmbFamily.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFamily.FormattingEnabled = true;
            this.cmbFamily.Location = new System.Drawing.Point(180, 160);
            this.cmbFamily.Name = "cmbFamily";
            this.cmbFamily.Size = new System.Drawing.Size(140, 23);
            this.cmbFamily.TabIndex = 9;
            // 
            // pnlItemsSettings
            // 
            this.pnlItemsSettings.Controls.Add(this.cmbShowTax);
            this.pnlItemsSettings.Controls.Add(this.lblShowTax);
            this.pnlItemsSettings.Controls.Add(this.cmbShowNotes);
            this.pnlItemsSettings.Controls.Add(this.lblShowNotes);
            this.pnlItemsSettings.Controls.Add(this.cmbShowDetails);
            this.pnlItemsSettings.Controls.Add(this.lblShowDetails);
            this.pnlItemsSettings.Controls.Add(this.txtCurrencySymbol);
            this.pnlItemsSettings.Controls.Add(this.lblCurrencySymbol);
            this.pnlItemsSettings.Controls.Add(this.cmbRightAlignPrice);
            this.pnlItemsSettings.Controls.Add(this.lblRightAlignPrice);
            this.pnlItemsSettings.Controls.Add(this.cmbShowPrice);
            this.pnlItemsSettings.Controls.Add(this.lblShowPrice);
            this.pnlItemsSettings.Location = new System.Drawing.Point(20, 200);
            this.pnlItemsSettings.Name = "pnlItemsSettings";
            this.pnlItemsSettings.Size = new System.Drawing.Size(460, 185);
            this.pnlItemsSettings.TabIndex = 10;
            // 
            // cmbShowTax
            // 
            this.cmbShowTax.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbShowTax.FormattingEnabled = true;
            this.cmbShowTax.Location = new System.Drawing.Point(310, 140);
            this.cmbShowTax.Name = "cmbShowTax";
            this.cmbShowTax.Size = new System.Drawing.Size(130, 23);
            this.cmbShowTax.TabIndex = 11;
            // 
            // lblShowTax
            // 
            this.lblShowTax.AutoSize = true;
            this.lblShowTax.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblShowTax.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblShowTax.Location = new System.Drawing.Point(310, 120);
            this.lblShowTax.Name = "lblShowTax";
            this.lblShowTax.Size = new System.Drawing.Size(68, 15);
            this.lblShowTax.TabIndex = 10;
            this.lblShowTax.Text = "KDV Göster:";
            // 
            // cmbShowNotes
            // 
            this.cmbShowNotes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbShowNotes.FormattingEnabled = true;
            this.cmbShowNotes.Location = new System.Drawing.Point(160, 140);
            this.cmbShowNotes.Name = "cmbShowNotes";
            this.cmbShowNotes.Size = new System.Drawing.Size(130, 23);
            this.cmbShowNotes.TabIndex = 9;
            // 
            // lblShowNotes
            // 
            this.lblShowNotes.AutoSize = true;
            this.lblShowNotes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblShowNotes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblShowNotes.Location = new System.Drawing.Point(160, 120);
            this.lblShowNotes.Name = "lblShowNotes";
            this.lblShowNotes.Size = new System.Drawing.Size(123, 15);
            this.lblShowNotes.TabIndex = 8;
            this.lblShowNotes.Text = "Açıklamaları Göster:";
            // 
            // cmbShowDetails
            // 
            this.cmbShowDetails.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbShowDetails.FormattingEnabled = true;
            this.cmbShowDetails.Location = new System.Drawing.Point(10, 140);
            this.cmbShowDetails.Name = "cmbShowDetails";
            this.cmbShowDetails.Size = new System.Drawing.Size(130, 23);
            this.cmbShowDetails.TabIndex = 7;
            // 
            // lblShowDetails
            // 
            this.lblShowDetails.AutoSize = true;
            this.lblShowDetails.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblShowDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblShowDetails.Location = new System.Drawing.Point(10, 120);
            this.lblShowDetails.Name = "lblShowDetails";
            this.lblShowDetails.Size = new System.Drawing.Size(107, 15);
            this.lblShowDetails.TabIndex = 6;
            this.lblShowDetails.Text = "Ayrıntıları Göster:";
            // 
            // txtCurrencySymbol
            // 
            this.txtCurrencySymbol.Location = new System.Drawing.Point(310, 80);
            this.txtCurrencySymbol.Name = "txtCurrencySymbol";
            this.txtCurrencySymbol.Size = new System.Drawing.Size(130, 23);
            this.txtCurrencySymbol.TabIndex = 5;
            // 
            // lblCurrencySymbol
            // 
            this.lblCurrencySymbol.AutoSize = true;
            this.lblCurrencySymbol.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCurrencySymbol.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblCurrencySymbol.Location = new System.Drawing.Point(310, 60);
            this.lblCurrencySymbol.Name = "lblCurrencySymbol";
            this.lblCurrencySymbol.Size = new System.Drawing.Size(117, 15);
            this.lblCurrencySymbol.TabIndex = 4;
            this.lblCurrencySymbol.Text = "Para Birimi Simgesi:";
            // 
            // cmbRightAlignPrice
            // 
            this.cmbRightAlignPrice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRightAlignPrice.FormattingEnabled = true;
            this.cmbRightAlignPrice.Location = new System.Drawing.Point(160, 80);
            this.cmbRightAlignPrice.Name = "cmbRightAlignPrice";
            this.cmbRightAlignPrice.Size = new System.Drawing.Size(130, 23);
            this.cmbRightAlignPrice.TabIndex = 3;
            // 
            // lblRightAlignPrice
            // 
            this.lblRightAlignPrice.AutoSize = true;
            this.lblRightAlignPrice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblRightAlignPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblRightAlignPrice.Location = new System.Drawing.Point(160, 60);
            this.lblRightAlignPrice.Name = "lblRightAlignPrice";
            this.lblRightAlignPrice.Size = new System.Drawing.Size(114, 15);
            this.lblRightAlignPrice.TabIndex = 2;
            this.lblRightAlignPrice.Text = "Fiyatı Sağa Yasla:";
            // 
            // cmbShowPrice
            // 
            this.cmbShowPrice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbShowPrice.FormattingEnabled = true;
            this.cmbShowPrice.Location = new System.Drawing.Point(10, 80);
            this.cmbShowPrice.Name = "cmbShowPrice";
            this.cmbShowPrice.Size = new System.Drawing.Size(130, 23);
            this.cmbShowPrice.TabIndex = 1;
            // 
            // lblShowPrice
            // 
            this.lblShowPrice.AutoSize = true;
            this.lblShowPrice.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblShowPrice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblShowPrice.Location = new System.Drawing.Point(10, 60);
            this.lblShowPrice.Name = "lblShowPrice";
            this.lblShowPrice.Size = new System.Drawing.Size(76, 15);
            this.lblShowPrice.TabIndex = 0;
            this.lblShowPrice.Text = "Fiyat Göster:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(350, 40);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(110, 24);
            this.btnBrowse.TabIndex = 11;
            this.btnBrowse.Tag = "Secondary";
            this.btnBrowse.Text = " Dosya Seç";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // flpTokens
            // 
            this.flpTokens.Location = new System.Drawing.Point(20, 220);
            this.flpTokens.Name = "flpTokens";
            this.flpTokens.Size = new System.Drawing.Size(460, 160);
            this.flpTokens.TabIndex = 12;
            // 
            // lblTokensTitle
            // 
            this.lblTokensTitle.AutoSize = true;
            this.lblTokensTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTokensTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblTokensTitle.Location = new System.Drawing.Point(20, 200);
            this.lblTokensTitle.Name = "lblTokensTitle";
            this.lblTokensTitle.Size = new System.Drawing.Size(89, 15);
            this.lblTokensTitle.TabIndex = 13;
            this.lblTokensTitle.Text = "Değişken Ekle:";
            // 
            // pnlActions
            // 
            this.pnlActions.Controls.Add(this.btnCancel);
            this.pnlActions.Controls.Add(this.btnSave);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActions.Location = new System.Drawing.Point(0, 405);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Size = new System.Drawing.Size(500, 60);
            this.pnlActions.TabIndex = 14;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(270, 13);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 32);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Tag = "Secondary";
            this.btnCancel.Text = "İptal";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(385, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 32);
            this.btnSave.TabIndex = 0;
            this.btnSave.Tag = "Primary";
            this.btnSave.Text = "Kaydet";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // EditItemForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 465);
            this.Controls.Add(this.pnlActions);
            this.Controls.Add(this.lblTokensTitle);
            this.Controls.Add(this.flpTokens);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.pnlItemsSettings);
            this.Controls.Add(this.cmbFamily);
            this.Controls.Add(this.lblFamily);
            this.Controls.Add(this.cmbFont);
            this.Controls.Add(this.lblWeight);
            this.Controls.Add(this.cmbSize);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.cmbAlign);
            this.Controls.Add(this.lblAlign);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.lblContent);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditItemForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Öğeyi Düzenle";
            this.Load += new System.EventHandler(this.EditItemForm_Load);
            this.pnlItemsSettings.ResumeLayout(false);
            this.pnlItemsSettings.PerformLayout();
            this.pnlActions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Label lblAlign;
        private System.Windows.Forms.ComboBox cmbAlign;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.ComboBox cmbSize;
        private System.Windows.Forms.Label lblWeight;
        private System.Windows.Forms.ComboBox cmbFont;
        private System.Windows.Forms.Label lblFamily;
        private System.Windows.Forms.ComboBox cmbFamily;
        private System.Windows.Forms.Panel pnlItemsSettings;
        private System.Windows.Forms.Label lblShowPrice;
        private System.Windows.Forms.ComboBox cmbShowPrice;
        private System.Windows.Forms.Label lblRightAlignPrice;
        private System.Windows.Forms.ComboBox cmbRightAlignPrice;
        private System.Windows.Forms.Label lblCurrencySymbol;
        private System.Windows.Forms.TextBox txtCurrencySymbol;
        private System.Windows.Forms.Label lblShowDetails;
        private System.Windows.Forms.ComboBox cmbShowDetails;
        private System.Windows.Forms.Label lblShowNotes;
        private System.Windows.Forms.ComboBox cmbShowNotes;
        private System.Windows.Forms.Label lblShowTax;
        private System.Windows.Forms.ComboBox cmbShowTax;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.FlowLayoutPanel flpTokens;
        private System.Windows.Forms.Label lblTokensTitle;
        private System.Windows.Forms.Panel pnlActions;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
    }
}
