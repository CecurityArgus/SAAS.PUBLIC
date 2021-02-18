namespace Public_Client_Test
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.publicUri = new System.Windows.Forms.TextBox();
            this.BaseUri = new System.Windows.Forms.Label();
            this.Response = new System.Windows.Forms.RichTextBox();
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFiles = new System.Windows.Forms.TextBox();
            this.btnUpload = new System.Windows.Forms.Button();
            this.grpLogin = new System.Windows.Forms.GroupBox();
            this.grpConnection = new System.Windows.Forms.GroupBox();
            this.grpUpoad = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSiren = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbSolutionName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.grpLogin.SuspendLayout();
            this.grpConnection.SuspendLayout();
            this.grpUpoad.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Username";
            // 
            // txtUserName
            // 
            this.txtUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUserName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtUserName.Location = new System.Drawing.Point(88, 19);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(237, 20);
            this.txtUserName.TabIndex = 2;
            this.txtUserName.Text = "benoits";
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtPassword.Location = new System.Drawing.Point(88, 45);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(237, 20);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.Text = "Welcome123";
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // btnLogin
            // 
            this.btnLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogin.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnLogin.Location = new System.Drawing.Point(241, 71);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(84, 23);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Log in";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.Login_Click);
            // 
            // publicUri
            // 
            this.publicUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.publicUri.ForeColor = System.Drawing.SystemColors.ControlText;
            this.publicUri.Location = new System.Drawing.Point(88, 19);
            this.publicUri.Name = "publicUri";
            this.publicUri.Size = new System.Drawing.Size(237, 20);
            this.publicUri.TabIndex = 0;
            this.publicUri.Text = "https://app.cecurity.com/public.api/";
            // 
            // BaseUri
            // 
            this.BaseUri.AutoSize = true;
            this.BaseUri.ForeColor = System.Drawing.SystemColors.ControlText;
            this.BaseUri.Location = new System.Drawing.Point(6, 22);
            this.BaseUri.Name = "BaseUri";
            this.BaseUri.Size = new System.Drawing.Size(76, 13);
            this.BaseUri.TabIndex = 6;
            this.BaseUri.Text = "Public base uri";
            // 
            // Response
            // 
            this.Response.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Response.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Response.Location = new System.Drawing.Point(349, 12);
            this.Response.Name = "Response";
            this.Response.ReadOnly = true;
            this.Response.Size = new System.Drawing.Size(614, 476);
            this.Response.TabIndex = 8;
            this.Response.TabStop = false;
            this.Response.Text = "";
            // 
            // txtApiKey
            // 
            this.txtApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtApiKey.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtApiKey.Location = new System.Drawing.Point(88, 45);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.Size = new System.Drawing.Size(237, 20);
            this.txtApiKey.TabIndex = 1;
            this.txtApiKey.Text = "06e1104d-65ad-413f-8035-b353944287a8";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label3.Location = new System.Drawing.Point(10, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Api key";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.AutoSize = true;
            this.btnBrowse.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnBrowse.Location = new System.Drawing.Point(241, 19);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(84, 23);
            this.btnBrowse.TabIndex = 6;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFiles
            // 
            this.txtFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFiles.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtFiles.Location = new System.Drawing.Point(88, 21);
            this.txtFiles.Name = "txtFiles";
            this.txtFiles.Size = new System.Drawing.Size(147, 20);
            this.txtFiles.TabIndex = 5;
            // 
            // btnUpload
            // 
            this.btnUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpload.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnUpload.Location = new System.Drawing.Point(241, 105);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(84, 23);
            this.btnUpload.TabIndex = 9;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // grpLogin
            // 
            this.grpLogin.Controls.Add(this.label1);
            this.grpLogin.Controls.Add(this.txtUserName);
            this.grpLogin.Controls.Add(this.label2);
            this.grpLogin.Controls.Add(this.txtPassword);
            this.grpLogin.Controls.Add(this.btnLogin);
            this.grpLogin.ForeColor = System.Drawing.Color.SteelBlue;
            this.grpLogin.Location = new System.Drawing.Point(12, 98);
            this.grpLogin.Name = "grpLogin";
            this.grpLogin.Size = new System.Drawing.Size(331, 100);
            this.grpLogin.TabIndex = 14;
            this.grpLogin.TabStop = false;
            this.grpLogin.Text = "Credentials";
            // 
            // grpConnection
            // 
            this.grpConnection.Controls.Add(this.BaseUri);
            this.grpConnection.Controls.Add(this.publicUri);
            this.grpConnection.Controls.Add(this.label3);
            this.grpConnection.Controls.Add(this.txtApiKey);
            this.grpConnection.ForeColor = System.Drawing.Color.SteelBlue;
            this.grpConnection.Location = new System.Drawing.Point(12, 12);
            this.grpConnection.Name = "grpConnection";
            this.grpConnection.Size = new System.Drawing.Size(331, 80);
            this.grpConnection.TabIndex = 15;
            this.grpConnection.TabStop = false;
            this.grpConnection.Text = "Connection setings";
            // 
            // grpUpoad
            // 
            this.grpUpoad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpUpoad.Controls.Add(this.label7);
            this.grpUpoad.Controls.Add(this.txtSiren);
            this.grpUpoad.Controls.Add(this.label6);
            this.grpUpoad.Controls.Add(this.label5);
            this.grpUpoad.Controls.Add(this.cmbSolutionName);
            this.grpUpoad.Controls.Add(this.label4);
            this.grpUpoad.Controls.Add(this.txtFiles);
            this.grpUpoad.Controls.Add(this.btnBrowse);
            this.grpUpoad.Controls.Add(this.btnUpload);
            this.grpUpoad.Enabled = false;
            this.grpUpoad.ForeColor = System.Drawing.Color.SteelBlue;
            this.grpUpoad.Location = new System.Drawing.Point(12, 204);
            this.grpUpoad.Name = "grpUpoad";
            this.grpUpoad.Size = new System.Drawing.Size(331, 313);
            this.grpUpoad.TabIndex = 16;
            this.grpUpoad.TabStop = false;
            this.grpUpoad.Text = "File Upload";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(6, 131);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(319, 179);
            this.label7.TabIndex = 18;
            this.label7.Text = resources.GetString("label7.Text");
            // 
            // txtSiren
            // 
            this.txtSiren.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSiren.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtSiren.Location = new System.Drawing.Point(88, 79);
            this.txtSiren.Name = "txtSiren";
            this.txtSiren.Size = new System.Drawing.Size(237, 20);
            this.txtSiren.TabIndex = 8;
            this.txtSiren.Text = "222222222";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(10, 82);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Siren";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label5.Location = new System.Drawing.Point(10, 51);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Solution";
            // 
            // cmbSolutionName
            // 
            this.cmbSolutionName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmbSolutionName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmbSolutionName.DisplayMember = "0";
            this.cmbSolutionName.FormattingEnabled = true;
            this.cmbSolutionName.Items.AddRange(new object[] {
            "EPaie",
            "EFacture"});
            this.cmbSolutionName.Location = new System.Drawing.Point(88, 48);
            this.cmbSolutionName.Name = "cmbSolutionName";
            this.cmbSolutionName.Size = new System.Drawing.Size(237, 21);
            this.cmbSolutionName.TabIndex = 7;
            this.cmbSolutionName.Text = "EFacture";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label4.Location = new System.Drawing.Point(10, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "Files";
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnClear.Location = new System.Drawing.Point(888, 494);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 99;
            this.btnClear.Text = "Clear output";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(975, 529);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.grpUpoad);
            this.Controls.Add(this.grpConnection);
            this.Controls.Add(this.grpLogin);
            this.Controls.Add(this.Response);
            this.MinimumSize = new System.Drawing.Size(766, 367);
            this.Name = "Form1";
            this.Text = "Public Api Test";
            this.grpLogin.ResumeLayout(false);
            this.grpLogin.PerformLayout();
            this.grpConnection.ResumeLayout(false);
            this.grpConnection.PerformLayout();
            this.grpUpoad.ResumeLayout(false);
            this.grpUpoad.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox publicUri;
        private System.Windows.Forms.Label BaseUri;
        private System.Windows.Forms.RichTextBox Response;
        private System.Windows.Forms.TextBox txtApiKey;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFiles;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.GroupBox grpLogin;
        private System.Windows.Forms.GroupBox grpConnection;
        private System.Windows.Forms.GroupBox grpUpoad;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox txtSiren;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbSolutionName;
        private System.Windows.Forms.Label label7;
    }
}

