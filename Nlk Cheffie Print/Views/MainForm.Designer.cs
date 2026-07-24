namespace Nlk_Cheffie_Print.Views
{
    partial class MainForm
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
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.lblRestaurantName = new System.Windows.Forms.Label();
            this.btnNavDesigner = new System.Windows.Forms.Button();
            this.btnNavPrinters = new System.Windows.Forms.Button();
            this.btnNavOrders = new System.Windows.Forms.Button();
            this.lblBrand = new System.Windows.Forms.Label();
            this.pnlTopBar = new System.Windows.Forms.Panel();
            this.btnSettings = new System.Windows.Forms.Button();
            this.lblUpdateBanner = new System.Windows.Forms.Label();
            this.pnlContainer = new System.Windows.Forms.Panel();
            this.pnlSidebar.SuspendLayout();
            this.pnlTopBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.Controls.Add(this.lblRestaurantName);
            this.pnlSidebar.Controls.Add(this.btnNavDesigner);
            this.pnlSidebar.Controls.Add(this.btnNavPrinters);
            this.pnlSidebar.Controls.Add(this.btnNavOrders);
            this.pnlSidebar.Controls.Add(this.lblBrand);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(200, 560);
            this.pnlSidebar.TabIndex = 0;
            this.pnlSidebar.Tag = "Card";
            // 
            // lblRestaurantName
            // 
            this.lblRestaurantName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRestaurantName.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblRestaurantName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblRestaurantName.Location = new System.Drawing.Point(12, 510);
            this.lblRestaurantName.Name = "lblRestaurantName";
            this.lblRestaurantName.Size = new System.Drawing.Size(175, 40);
            this.lblRestaurantName.TabIndex = 4;
            this.lblRestaurantName.Tag = "Muted";
            this.lblRestaurantName.Text = "NLK Soft Restoran";
            this.lblRestaurantName.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // btnNavDesigner
            // 
            this.btnNavDesigner.Location = new System.Drawing.Point(0, 190);
            this.btnNavDesigner.Name = "btnNavDesigner";
            this.btnNavDesigner.Size = new System.Drawing.Size(200, 45);
            this.btnNavDesigner.TabIndex = 3;
            this.btnNavDesigner.Tag = "Nav";
            this.btnNavDesigner.Text = "Fiş Tasarımı";
            this.btnNavDesigner.UseVisualStyleBackColor = true;
            this.btnNavDesigner.Click += new System.EventHandler(this.btnNavDesigner_Click);
            // 
            // btnNavPrinters
            // 
            this.btnNavPrinters.Location = new System.Drawing.Point(0, 140);
            this.btnNavPrinters.Name = "btnNavPrinters";
            this.btnNavPrinters.Size = new System.Drawing.Size(200, 45);
            this.btnNavPrinters.TabIndex = 2;
            this.btnNavPrinters.Tag = "Nav";
            this.btnNavPrinters.Text = "Yazıcılar";
            this.btnNavPrinters.UseVisualStyleBackColor = true;
            this.btnNavPrinters.Click += new System.EventHandler(this.btnNavPrinters_Click);
            // 
            // btnNavOrders
            // 
            this.btnNavOrders.Location = new System.Drawing.Point(0, 90);
            this.btnNavOrders.Name = "btnNavOrders";
            this.btnNavOrders.Size = new System.Drawing.Size(200, 45);
            this.btnNavOrders.TabIndex = 1;
            this.btnNavOrders.Tag = "Nav";
            this.btnNavOrders.Text = "Siparişler";
            this.btnNavOrders.UseVisualStyleBackColor = true;
            this.btnNavOrders.Click += new System.EventHandler(this.btnNavOrders_Click);
            // 
            // lblBrand
            // 
            this.lblBrand.AutoSize = true;
            this.lblBrand.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBrand.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblBrand.Location = new System.Drawing.Point(12, 20);
            this.lblBrand.Name = "lblBrand";
            this.lblBrand.Size = new System.Drawing.Size(150, 19);
            this.lblBrand.TabIndex = 0;
            this.lblBrand.Tag = "Title";
            this.lblBrand.Text = "CHEFFIE POS BRIDGE";
            // 
            // pnlTopBar
            // 
            this.pnlTopBar.Controls.Add(this.btnSettings);
            this.pnlTopBar.Controls.Add(this.lblUpdateBanner);
            this.pnlTopBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTopBar.Location = new System.Drawing.Point(200, 0);
            this.pnlTopBar.Name = "pnlTopBar";
            this.pnlTopBar.Size = new System.Drawing.Size(660, 50);
            this.pnlTopBar.TabIndex = 1;
            this.pnlTopBar.Tag = "Card";
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Location = new System.Drawing.Point(615, 10);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(32, 32);
            this.btnSettings.TabIndex = 1;
            this.btnSettings.Text = "";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // lblUpdateBanner
            // 
            this.lblUpdateBanner.AutoSize = true;
            this.lblUpdateBanner.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblUpdateBanner.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.lblUpdateBanner.Location = new System.Drawing.Point(15, 18);
            this.lblUpdateBanner.Name = "lblUpdateBanner";
            this.lblUpdateBanner.Size = new System.Drawing.Size(161, 13);
            this.lblUpdateBanner.TabIndex = 0;
            this.lblUpdateBanner.Text = "Yeni Sürüm Güncellemesi Hazır";
            this.lblUpdateBanner.Visible = false;
            // 
            // pnlContainer
            // 
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.Location = new System.Drawing.Point(200, 50);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(660, 510);
            this.pnlContainer.TabIndex = 2;
            this.pnlContainer.Tag = "Background";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 720);
            this.Controls.Add(this.pnlContainer);
            this.Controls.Add(this.pnlTopBar);
            this.Controls.Add(this.pnlSidebar);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nlk Cheffie Print";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSidebar.PerformLayout();
            this.pnlTopBar.ResumeLayout(false);
            this.pnlTopBar.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblBrand;
        private System.Windows.Forms.Button btnNavOrders;
        private System.Windows.Forms.Button btnNavPrinters;
        private System.Windows.Forms.Button btnNavDesigner;
        private System.Windows.Forms.Panel pnlTopBar;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Label lblUpdateBanner;
        private System.Windows.Forms.Panel pnlContainer;
        private System.Windows.Forms.Label lblRestaurantName;
    }
}
