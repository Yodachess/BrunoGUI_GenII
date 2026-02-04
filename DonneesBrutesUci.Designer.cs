namespace BrunoGUI_GenII
{
    partial class DonneesBrutesUci
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DonneesBrutesUci));
            DonneesBrutesVue = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // DonneesBrutesVue
            // 
            DonneesBrutesVue.BackColor = System.Drawing.Color.Silver;
            DonneesBrutesVue.ForeColor = System.Drawing.Color.DarkBlue;
            DonneesBrutesVue.Location = new System.Drawing.Point(0, 5);
            DonneesBrutesVue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            DonneesBrutesVue.Name = "DonneesBrutesVue";
            DonneesBrutesVue.Size = new System.Drawing.Size(1420, 388);
            DonneesBrutesVue.TabIndex = 0;
            DonneesBrutesVue.Text = "";
            // 
            // DonneesBrutesUci
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ActiveCaption;
            ClientSize = new System.Drawing.Size(1423, 395);
            Controls.Add(DonneesBrutesVue);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "DonneesBrutesUci";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Donnees Brutes protocole Uci";
            ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox DonneesBrutesVue;
    }
}