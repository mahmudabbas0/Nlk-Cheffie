namespace Nlk_Cheffie_Print.Views
{
    partial class LoginForm
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
            this.pnlCard = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnBrowserLogin = new System.Windows.Forms.Button();
            this.lblOrManual = new System.Windows.Forms.Label();
            this.lblTokenLabel = new System.Windows.Forms.Label();
            this.pnlTokenWrapper = new System.Windows.Forms.Panel();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.btnTogglePassword = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlCard.SuspendLayout();
            this.pnlTokenWrapper.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlCard
            // 
            this.pnlCard.Controls.Add(this.lblSubtitle);
            this.pnlCard.Controls.Add(this.lblTitle);
            this.pnlCard.Controls.Add(this.btnBrowserLogin);
            this.pnlCard.Controls.Add(this.lblOrManual);
            this.pnlCard.Controls.Add(this.lblTokenLabel);
            this.pnlCard.Controls.Add(this.pnlTokenWrapper);
            this.pnlCard.Controls.Add(this.btnConnect);
            this.pnlCard.Location = new System.Drawing.Point(150, 60);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(500, 400);
            this.pnlCard.TabIndex = 0;
            this.pnlCard.Tag = "Card";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblSubtitle.Location = new System.Drawing.Point(50, 22);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(400, 18);
            this.lblSubtitle.TabIndex = 0;
            this.lblSubtitle.Text = "Nlk Cheffie Print";
            this.lblSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.lblTitle.Location = new System.Drawing.Point(50, 44);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(400, 30);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Cloud Connection";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnBrowserLogin
            // 
            this.btnBrowserLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnBrowserLogin.FlatAppearance.BorderSize = 0;
            this.btnBrowserLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowserLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnBrowserLogin.ForeColor = System.Drawing.Color.White;
            this.btnBrowserLogin.Location = new System.Drawing.Point(50, 95);
            this.btnBrowserLogin.Name = "btnBrowserLogin";
            this.btnBrowserLogin.Size = new System.Drawing.Size(400, 45);
            this.btnBrowserLogin.TabIndex = 2;
            this.btnBrowserLogin.Tag = "Success";
            this.btnBrowserLogin.Text = "Login with Browser";
            this.btnBrowserLogin.UseVisualStyleBackColor = false;
            this.btnBrowserLogin.Click += new System.EventHandler(this.btnBrowserLogin_Click);
            // 
            // lblOrManual
            // 
            this.lblOrManual.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblOrManual.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblOrManual.Location = new System.Drawing.Point(50, 155);
            this.lblOrManual.Name = "lblOrManual";
            this.lblOrManual.Size = new System.Drawing.Size(400, 20);
            this.lblOrManual.TabIndex = 3;
            this.lblOrManual.Text = "or enter token manually";
            this.lblOrManual.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTokenLabel
            // 
            this.lblTokenLabel.AutoSize = true;
            this.lblTokenLabel.Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold);
            this.lblTokenLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(142)))), ((int)(((byte)(147)))));
            this.lblTokenLabel.Location = new System.Drawing.Point(50, 195);
            this.lblTokenLabel.Name = "lblTokenLabel";
            this.lblTokenLabel.Size = new System.Drawing.Size(88, 15);
            this.lblTokenLabel.TabIndex = 4;
            this.lblTokenLabel.Text = "DEVICE TOKEN";
            // 
            // pnlTokenWrapper
            // 
            this.pnlTokenWrapper.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.pnlTokenWrapper.Controls.Add(this.txtToken);
            this.pnlTokenWrapper.Controls.Add(this.btnTogglePassword);
            this.pnlTokenWrapper.Location = new System.Drawing.Point(50, 218);
            this.pnlTokenWrapper.Name = "pnlTokenWrapper";
            this.pnlTokenWrapper.Size = new System.Drawing.Size(400, 45);
            this.pnlTokenWrapper.TabIndex = 5;
            // 
            // txtToken
            // 
            this.txtToken.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(30)))));
            this.txtToken.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtToken.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtToken.ForeColor = System.Drawing.Color.White;
            this.txtToken.Location = new System.Drawing.Point(12, 13);
            this.txtToken.Name = "txtToken";
            this.txtToken.PasswordChar = '●';
            this.txtToken.Size = new System.Drawing.Size(335, 20);
            this.txtToken.TabIndex = 0;
            // 
            // btnTogglePassword
            // 
            this.btnTogglePassword.BackColor = System.Drawing.Color.Transparent;
            this.btnTogglePassword.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnTogglePassword.FlatAppearance.BorderSize = 0;
            this.btnTogglePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTogglePassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnTogglePassword.ForeColor = System.Drawing.Color.Gray;
            this.btnTogglePassword.Location = new System.Drawing.Point(355, 0);
            this.btnTogglePassword.Name = "btnTogglePassword";
            this.btnTogglePassword.Size = new System.Drawing.Size(45, 45);
            this.btnTogglePassword.TabIndex = 1;
            this.btnTogglePassword.Text = "👁";
            this.btnTogglePassword.UseVisualStyleBackColor = false;
            this.btnTogglePassword.Click += new System.EventHandler(this.btnTogglePassword_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(152)))), ((int)(((byte)(0)))));
            this.btnConnect.FlatAppearance.BorderSize = 0;
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnConnect.ForeColor = System.Drawing.Color.Black;
            this.btnConnect.Location = new System.Drawing.Point(50, 300);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(400, 45);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Tag = "Primary";
            this.btnConnect.Text = "CONNECT TO SYSTEM";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(620, 20);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(150, 25);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Tag = "Status";
            this.lblStatus.Text = "SYSTEM OFFLINE";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlCard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nlk Cheffie Print - Authorization";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.pnlCard.ResumeLayout(false);
            this.pnlCard.PerformLayout();
            this.pnlTokenWrapper.ResumeLayout(false);
            this.pnlTokenWrapper.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlCard;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnBrowserLogin;
        private System.Windows.Forms.Label lblOrManual;
        private System.Windows.Forms.Label lblTokenLabel;
        private System.Windows.Forms.Panel pnlTokenWrapper;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Button btnTogglePassword;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblStatus;
    }
}
