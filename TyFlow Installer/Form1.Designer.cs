namespace TyFlow_Installer
{
    partial class TyFlow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TyFlow));
            this.ExtractBtn = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.Install_Btn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // ExtractBtn
            // 
            resources.ApplyResources(this.ExtractBtn, "ExtractBtn");
            this.ExtractBtn.Name = "ExtractBtn";
            this.ExtractBtn.UseVisualStyleBackColor = true;
            this.ExtractBtn.Click += new System.EventHandler(this.ExtractBtn_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Image = global::TyFlow_Installer.Properties.Resources.tyflow_logo_white;
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // Install_Btn
            // 
            resources.ApplyResources(this.Install_Btn, "Install_Btn");
            this.Install_Btn.Name = "Install_Btn";
            this.Install_Btn.UseVisualStyleBackColor = true;
            this.Install_Btn.Click += new System.EventHandler(this.Install_Btn_Click);
            // 
            // TyFlow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Desktop;
            this.Controls.Add(this.Install_Btn);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExtractBtn);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TyFlow";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ExtractBtn;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button Install_Btn;
    }
}

