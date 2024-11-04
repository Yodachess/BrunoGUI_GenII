namespace BrunoGUI_Stockfish
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
            this.DonneesBrutesVue = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // DonneesBrutesVue
            // 
            this.DonneesBrutesVue.BackColor = System.Drawing.Color.LightSteelBlue;
            this.DonneesBrutesVue.ForeColor = System.Drawing.Color.DarkBlue;
            this.DonneesBrutesVue.Location = new System.Drawing.Point(0, 4);
            this.DonneesBrutesVue.Name = "DonneesBrutesVue";
            this.DonneesBrutesVue.Size = new System.Drawing.Size(1218, 337);
            this.DonneesBrutesVue.TabIndex = 0;
            this.DonneesBrutesVue.Text = "";
            // 
            // DonneesBrutesUci
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1220, 342);
            this.Controls.Add(this.DonneesBrutesVue);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DonneesBrutesUci";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Donnees Brutes protocole Uci";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox DonneesBrutesVue;
    }
}