namespace Nlk_Cheffie_Print.Views.Controls
{
    partial class DesignerControl
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
            this.pnlLeftSidebar = new System.Windows.Forms.Panel();
            this.btnSaveDesign = new System.Windows.Forms.Button();
            this.btnResetTemplate = new System.Windows.Forms.Button();
            this.btnAddFooter = new System.Windows.Forms.Button();
            this.lstFooter = new System.Windows.Forms.ListBox();
            this.lblFooterSection = new System.Windows.Forms.Label();
            this.btnAddBody = new System.Windows.Forms.Button();
            this.lstBody = new System.Windows.Forms.ListBox();
            this.lblBodySection = new System.Windows.Forms.Label();
            this.btnAddHeader = new System.Windows.Forms.Button();
            this.lstHeader = new System.Windows.Forms.ListBox();
            this.lblHeaderSection = new System.Windows.Forms.Label();
            this.lblLeftTitle = new System.Windows.Forms.Label();
            this.pnlRightSidebar = new System.Windows.Forms.Panel();
            this.btnDeleteItem = new System.Windows.Forms.Button();
            this.btnEditItem = new System.Windows.Forms.Button();
            this.lblItemActions = new System.Windows.Forms.Label();
            this.cmbTemplate = new System.Windows.Forms.ComboBox();
            this.lblTemplateSelect = new System.Windows.Forms.Label();
            this.pnlMiddleCanvas = new System.Windows.Forms.Panel();
            this.pnlReceiptRoll = new System.Windows.Forms.Panel();
            this.lblCanvasTitle = new System.Windows.Forms.Label();
            this.pnlLeftSidebar.SuspendLayout();
            this.pnlRightSidebar.SuspendLayout();
            this.pnlMiddleCanvas.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeftSidebar
            // 
            this.pnlLeftSidebar.Controls.Add(this.btnSaveDesign);
            this.pnlLeftSidebar.Controls.Add(this.btnResetTemplate);
            this.pnlLeftSidebar.Controls.Add(this.btnAddFooter);
            this.pnlLeftSidebar.Controls.Add(this.lstFooter);
            this.pnlLeftSidebar.Controls.Add(this.lblFooterSection);
            this.pnlLeftSidebar.Controls.Add(this.btnAddBody);
            this.pnlLeftSidebar.Controls.Add(this.lstBody);
            this.pnlLeftSidebar.Controls.Add(this.lblBodySection);
            this.pnlLeftSidebar.Controls.Add(this.btnAddHeader);
            this.pnlLeftSidebar.Controls.Add(this.lstHeader);
            this.pnlLeftSidebar.Controls.Add(this.lblHeaderSection);
            this.pnlLeftSidebar.Controls.Add(this.lblLeftTitle);
            this.pnlLeftSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeftSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftSidebar.Name = "pnlLeftSidebar";
            this.pnlLeftSidebar.Size = new System.Drawing.Size(260, 500);
            this.pnlLeftSidebar.TabIndex = 0;
            // 
            // btnSaveDesign
            // 
            this.btnSaveDesign.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveDesign.Location = new System.Drawing.Point(15, 450);
            this.btnSaveDesign.Name = "btnSaveDesign";
            this.btnSaveDesign.Size = new System.Drawing.Size(230, 35);
            this.btnSaveDesign.TabIndex = 11;
            this.btnSaveDesign.Tag = "Primary";
            this.btnSaveDesign.Text = "Tasarımı Kaydet";
            this.btnSaveDesign.UseVisualStyleBackColor = true;
            this.btnSaveDesign.Click += new System.EventHandler(this.btnSaveDesign_Click);
            // 
            // btnResetTemplate
            // 
            this.btnResetTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnResetTemplate.Location = new System.Drawing.Point(15, 410);
            this.btnResetTemplate.Name = "btnResetTemplate";
            this.btnResetTemplate.Size = new System.Drawing.Size(230, 32);
            this.btnResetTemplate.TabIndex = 10;
            this.btnResetTemplate.Tag = "Danger";
            this.btnResetTemplate.Text = "Varsayılana Döndür";
            this.btnResetTemplate.UseVisualStyleBackColor = true;
            this.btnResetTemplate.Click += new System.EventHandler(this.btnResetTemplate_Click);
            // 
            // btnAddFooter
            // 
            this.btnAddFooter.Location = new System.Drawing.Point(15, 365);
            this.btnAddFooter.Name = "btnAddFooter";
            this.btnAddFooter.Size = new System.Drawing.Size(230, 25);
            this.btnAddFooter.TabIndex = 9;
            this.btnAddFooter.Tag = "Secondary";
            this.btnAddFooter.Text = "+ Add Item to Footer";
            this.btnAddFooter.UseVisualStyleBackColor = true;
            this.btnAddFooter.Click += new System.EventHandler(this.btnAddFooter_Click);
            // 
            // lstFooter
            // 
            this.lstFooter.Location = new System.Drawing.Point(15, 295);
            this.lstFooter.Name = "lstFooter";
            this.lstFooter.Size = new System.Drawing.Size(230, 64);
            this.lstFooter.TabIndex = 8;
            this.lstFooter.SelectedIndexChanged += new System.EventHandler(this.lstFooter_SelectedIndexChanged);
            this.lstFooter.DoubleClick += new System.EventHandler(this.lstFooter_DoubleClick);
            // 
            // lblFooterSection
            // 
            this.lblFooterSection.AutoSize = true;
            this.lblFooterSection.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblFooterSection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblFooterSection.Location = new System.Drawing.Point(15, 275);
            this.lblFooterSection.Name = "lblFooterSection";
            this.lblFooterSection.Size = new System.Drawing.Size(86, 15);
            this.lblFooterSection.TabIndex = 7;
            this.lblFooterSection.Tag = "Muted";
            this.lblFooterSection.Text = "Footer Section";
            // 
            // btnAddBody
            // 
            this.btnAddBody.Location = new System.Drawing.Point(15, 240);
            this.btnAddBody.Name = "btnAddBody";
            this.btnAddBody.Size = new System.Drawing.Size(230, 25);
            this.btnAddBody.TabIndex = 6;
            this.btnAddBody.Tag = "Secondary";
            this.btnAddBody.Text = "+ Add Item to Body";
            this.btnAddBody.UseVisualStyleBackColor = true;
            this.btnAddBody.Click += new System.EventHandler(this.btnAddBody_Click);
            // 
            // lstBody
            // 
            this.lstBody.Location = new System.Drawing.Point(15, 170);
            this.lstBody.Name = "lstBody";
            this.lstBody.Size = new System.Drawing.Size(230, 64);
            this.lstBody.TabIndex = 5;
            this.lstBody.SelectedIndexChanged += new System.EventHandler(this.lstBody_SelectedIndexChanged);
            this.lstBody.DoubleClick += new System.EventHandler(this.lstBody_DoubleClick);
            // 
            // lblBodySection
            // 
            this.lblBodySection.AutoSize = true;
            this.lblBodySection.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblBodySection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblBodySection.Location = new System.Drawing.Point(15, 150);
            this.lblBodySection.Name = "lblBodySection";
            this.lblBodySection.Size = new System.Drawing.Size(78, 15);
            this.lblBodySection.TabIndex = 4;
            this.lblBodySection.Tag = "Muted";
            this.lblBodySection.Text = "Body Section";
            // 
            // btnAddHeader
            // 
            this.btnAddHeader.Location = new System.Drawing.Point(15, 115);
            this.btnAddHeader.Name = "btnAddHeader";
            this.btnAddHeader.Size = new System.Drawing.Size(230, 25);
            this.btnAddHeader.TabIndex = 3;
            this.btnAddHeader.Tag = "Secondary";
            this.btnAddHeader.Text = "+ Add Item to Header";
            this.btnAddHeader.UseVisualStyleBackColor = true;
            this.btnAddHeader.Click += new System.EventHandler(this.btnAddHeader_Click);
            // 
            // lstHeader
            // 
            this.lstHeader.Location = new System.Drawing.Point(15, 45);
            this.lstHeader.Name = "lstHeader";
            this.lstHeader.Size = new System.Drawing.Size(230, 64);
            this.lstHeader.TabIndex = 2;
            this.lstHeader.SelectedIndexChanged += new System.EventHandler(this.lstHeader_SelectedIndexChanged);
            this.lstHeader.DoubleClick += new System.EventHandler(this.lstHeader_DoubleClick);
            // 
            // lblHeaderSection
            // 
            this.lblHeaderSection.AutoSize = true;
            this.lblHeaderSection.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblHeaderSection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblHeaderSection.Location = new System.Drawing.Point(15, 25);
            this.lblHeaderSection.Name = "lblHeaderSection";
            this.lblHeaderSection.Size = new System.Drawing.Size(89, 15);
            this.lblHeaderSection.TabIndex = 1;
            this.lblHeaderSection.Tag = "Muted";
            this.lblHeaderSection.Text = "Header Section";
            // 
            // lblLeftTitle
            // 
            this.lblLeftTitle.AutoSize = true;
            this.lblLeftTitle.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblLeftTitle.ForeColor = System.Drawing.Color.White;
            this.lblLeftTitle.Location = new System.Drawing.Point(15, 5);
            this.lblLeftTitle.Name = "lblLeftTitle";
            this.lblLeftTitle.Size = new System.Drawing.Size(127, 17);
            this.lblLeftTitle.TabIndex = 0;
            this.lblLeftTitle.Tag = "Header";
            this.lblLeftTitle.Text = "TASARIM BLOKLARI";
            // 
            // pnlRightSidebar
            // 
            this.pnlRightSidebar.Controls.Add(this.btnDeleteItem);
            this.pnlRightSidebar.Controls.Add(this.btnEditItem);
            this.pnlRightSidebar.Controls.Add(this.lblItemActions);
            this.pnlRightSidebar.Controls.Add(this.cmbTemplate);
            this.pnlRightSidebar.Controls.Add(this.lblTemplateSelect);
            this.pnlRightSidebar.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRightSidebar.Location = new System.Drawing.Point(580, 0);
            this.pnlRightSidebar.Name = "pnlRightSidebar";
            this.pnlRightSidebar.Size = new System.Drawing.Size(220, 500);
            this.pnlRightSidebar.TabIndex = 1;
            // 
            // btnDeleteItem
            // 
            this.btnDeleteItem.Location = new System.Drawing.Point(15, 170);
            this.btnDeleteItem.Name = "btnDeleteItem";
            this.btnDeleteItem.Size = new System.Drawing.Size(190, 32);
            this.btnDeleteItem.TabIndex = 4;
            this.btnDeleteItem.Tag = "Danger";
            this.btnDeleteItem.Text = "Seçili Öğeyi Sil";
            this.btnDeleteItem.UseVisualStyleBackColor = true;
            this.btnDeleteItem.Click += new System.EventHandler(this.btnDeleteItem_Click);
            // 
            // btnEditItem
            // 
            this.btnEditItem.Location = new System.Drawing.Point(15, 130);
            this.btnEditItem.Name = "btnEditItem";
            this.btnEditItem.Size = new System.Drawing.Size(190, 32);
            this.btnEditItem.TabIndex = 3;
            this.btnEditItem.Tag = "Secondary";
            this.btnEditItem.Text = "Seçili Öğeyi Düzenle";
            this.btnEditItem.UseVisualStyleBackColor = true;
            this.btnEditItem.Click += new System.EventHandler(this.btnEditItem_Click);
            // 
            // lblItemActions
            // 
            this.lblItemActions.AutoSize = true;
            this.lblItemActions.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblItemActions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblItemActions.Location = new System.Drawing.Point(15, 105);
            this.lblItemActions.Name = "lblItemActions";
            this.lblItemActions.Size = new System.Drawing.Size(94, 15);
            this.lblItemActions.TabIndex = 2;
            this.lblItemActions.Tag = "Title";
            this.lblItemActions.Text = "ÖĞE İŞLEMLERİ";
            // 
            // cmbTemplate
            // 
            this.cmbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTemplate.FormattingEnabled = true;
            this.cmbTemplate.Location = new System.Drawing.Point(15, 45);
            this.cmbTemplate.Name = "cmbTemplate";
            this.cmbTemplate.Size = new System.Drawing.Size(190, 23);
            this.cmbTemplate.TabIndex = 1;
            this.cmbTemplate.SelectedIndexChanged += new System.EventHandler(this.cmbTemplate_SelectedIndexChanged);
            // 
            // lblTemplateSelect
            // 
            this.lblTemplateSelect.AutoSize = true;
            this.lblTemplateSelect.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblTemplateSelect.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblTemplateSelect.Location = new System.Drawing.Point(15, 25);
            this.lblTemplateSelect.Name = "lblTemplateSelect";
            this.lblTemplateSelect.Size = new System.Drawing.Size(95, 15);
            this.lblTemplateSelect.TabIndex = 0;
            this.lblTemplateSelect.Tag = "Title";
            this.lblTemplateSelect.Text = "ŞABLON SEÇİMİ";
            // 
            // pnlMiddleCanvas
            // 
            this.pnlMiddleCanvas.Controls.Add(this.pnlReceiptRoll);
            this.pnlMiddleCanvas.Controls.Add(this.lblCanvasTitle);
            this.pnlMiddleCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMiddleCanvas.Location = new System.Drawing.Point(260, 0);
            this.pnlMiddleCanvas.Name = "pnlMiddleCanvas";
            this.pnlMiddleCanvas.Size = new System.Drawing.Size(320, 500);
            this.pnlMiddleCanvas.TabIndex = 2;
            this.pnlMiddleCanvas.Tag = "Background";
            this.pnlMiddleCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlMiddleCanvas_Paint);
            // 
            // pnlReceiptRoll
            // 
            this.pnlReceiptRoll.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlReceiptRoll.Location = new System.Drawing.Point(30, 45);
            this.pnlReceiptRoll.Name = "pnlReceiptRoll";
            this.pnlReceiptRoll.Size = new System.Drawing.Size(260, 440);
            this.pnlReceiptRoll.TabIndex = 1;
            this.pnlReceiptRoll.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlReceiptRoll_Paint);
            // 
            // lblCanvasTitle
            // 
            this.lblCanvasTitle.AutoSize = true;
            this.lblCanvasTitle.Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold);
            this.lblCanvasTitle.ForeColor = System.Drawing.Color.White;
            this.lblCanvasTitle.Location = new System.Drawing.Point(20, 15);
            this.lblCanvasTitle.Name = "lblCanvasTitle";
            this.lblCanvasTitle.Size = new System.Drawing.Size(95, 17);
            this.lblCanvasTitle.TabIndex = 0;
            this.lblCanvasTitle.Tag = "Header";
            this.lblCanvasTitle.Text = "FİŞ ÖNİZLEME";
            // 
            // DesignerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMiddleCanvas);
            this.Controls.Add(this.pnlRightSidebar);
            this.Controls.Add(this.pnlLeftSidebar);
            this.Name = "DesignerControl";
            this.Size = new System.Drawing.Size(800, 500);
            this.Load += new System.EventHandler(this.DesignerControl_Load);
            this.pnlLeftSidebar.ResumeLayout(false);
            this.pnlLeftSidebar.PerformLayout();
            this.pnlRightSidebar.ResumeLayout(false);
            this.pnlRightSidebar.PerformLayout();
            this.pnlMiddleCanvas.ResumeLayout(false);
            this.pnlMiddleCanvas.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLeftSidebar;
        private System.Windows.Forms.Label lblLeftTitle;
        private System.Windows.Forms.Label lblHeaderSection;
        private System.Windows.Forms.ListBox lstHeader;
        private System.Windows.Forms.Button btnAddHeader;
        private System.Windows.Forms.Label lblBodySection;
        private System.Windows.Forms.ListBox lstBody;
        private System.Windows.Forms.Button btnAddBody;
        private System.Windows.Forms.Label lblFooterSection;
        private System.Windows.Forms.ListBox lstFooter;
        private System.Windows.Forms.Button btnAddFooter;
        private System.Windows.Forms.Button btnResetTemplate;
        private System.Windows.Forms.Button btnSaveDesign;
        private System.Windows.Forms.Panel pnlRightSidebar;
        private System.Windows.Forms.Label lblTemplateSelect;
        private System.Windows.Forms.ComboBox cmbTemplate;
        private System.Windows.Forms.Label lblItemActions;
        private System.Windows.Forms.Button btnEditItem;
        private System.Windows.Forms.Button btnDeleteItem;
        private System.Windows.Forms.Panel pnlMiddleCanvas;
        private System.Windows.Forms.Label lblCanvasTitle;
        private System.Windows.Forms.Panel pnlReceiptRoll;
    }
}
