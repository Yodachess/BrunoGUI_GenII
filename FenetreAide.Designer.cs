namespace BrunoGUI_GenII
{
    partial class FenetreAide
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
            ContenuAide = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // ContenuAide
            // 
            ContenuAide.Dock = System.Windows.Forms.DockStyle.Fill;
            ContenuAide.Location = new System.Drawing.Point(0, 0);
            ContenuAide.Name = "ContenuAide";
            ContenuAide.ReadOnly = true;
            ContenuAide.Size = new System.Drawing.Size(800, 450);
            ContenuAide.TabIndex = 0;
            ContenuAide.Text = "";
            // 
            // FenetreAide
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(ContenuAide);
            Name = "FenetreAide";
            Text = "FenetreAide";
            Load += FenetreAide_Load;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox ContenuAide;
    }
}