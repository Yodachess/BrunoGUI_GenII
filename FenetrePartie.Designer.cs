namespace BrunoGUI_Stockfish
{
    partial class FenetrePartie
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FenetrePartie));
            this.FeuillePartie = new System.Windows.Forms.DataGridView();
            this.FermeFeuillePartie = new Krypton.Toolkit.KryptonButton();
            this.LblJoueurBlanc = new System.Windows.Forms.Label();
            this.LblEloBlanc = new System.Windows.Forms.Label();
            this.LblJoueurNoir = new System.Windows.Forms.Label();
            this.LblEloNoir = new System.Windows.Forms.Label();
            this.gourpeBoutons = new System.Windows.Forms.GroupBox();
            this.BoutonFin = new System.Windows.Forms.Button();
            this.BoutonGauche = new System.Windows.Forms.Button();
            this.BoutonDroit = new System.Windows.Forms.Button();
            this.BoutonDebut = new System.Windows.Forms.Button();
            this.NumeroLigneColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BlancsColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NoirsColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.FeuillePartie)).BeginInit();
            this.gourpeBoutons.SuspendLayout();
            this.SuspendLayout();
            // 
            // FeuillePartie
            // 
            this.FeuillePartie.AllowUserToAddRows = false;
            this.FeuillePartie.AllowUserToResizeColumns = false;
            this.FeuillePartie.BackgroundColor = System.Drawing.Color.LightSteelBlue;
            this.FeuillePartie.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.FeuillePartie.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FeuillePartie.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NumeroLigneColumn1,
            this.BlancsColumn2,
            this.NoirsColumn3});
            this.FeuillePartie.Location = new System.Drawing.Point(10, 60);
            this.FeuillePartie.Name = "FeuillePartie";
            this.FeuillePartie.RowHeadersVisible = false;
            this.FeuillePartie.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.FeuillePartie.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FeuillePartie.Size = new System.Drawing.Size(213, 527);
            this.FeuillePartie.TabIndex = 0;
            // 
            // FermeFeuillePartie
            // 
            this.FermeFeuillePartie.Location = new System.Drawing.Point(68, 649);
            this.FermeFeuillePartie.Name = "FermeFeuillePartie";
            this.FermeFeuillePartie.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.FermeFeuillePartie.Size = new System.Drawing.Size(90, 25);
            this.FermeFeuillePartie.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.FermeFeuillePartie.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.FermeFeuillePartie.StateCommon.Border.Rounding = 20F;
            this.FermeFeuillePartie.StateCommon.Border.Width = 3;
            this.FermeFeuillePartie.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.FermeFeuillePartie.TabIndex = 1;
            this.FermeFeuillePartie.Values.Text = "Fermer";
            this.FermeFeuillePartie.Click += new System.EventHandler(this.FermeFeuillePartie_Click);
            // 
            // LblJoueurBlanc
            // 
            this.LblJoueurBlanc.BackColor = System.Drawing.Color.White;
            this.LblJoueurBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblJoueurBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblJoueurBlanc.Location = new System.Drawing.Point(10, 33);
            this.LblJoueurBlanc.Name = "LblJoueurBlanc";
            this.LblJoueurBlanc.Size = new System.Drawing.Size(128, 20);
            this.LblJoueurBlanc.TabIndex = 3;
            this.LblJoueurBlanc.Text = "Joueur Blanc";
            this.LblJoueurBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblEloBlanc
            // 
            this.LblEloBlanc.BackColor = System.Drawing.Color.White;
            this.LblEloBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblEloBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblEloBlanc.Location = new System.Drawing.Point(147, 33);
            this.LblEloBlanc.Name = "LblEloBlanc";
            this.LblEloBlanc.Size = new System.Drawing.Size(75, 20);
            this.LblEloBlanc.TabIndex = 2;
            this.LblEloBlanc.Text = "Elo Blanc";
            this.LblEloBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblJoueurNoir
            // 
            this.LblJoueurNoir.BackColor = System.Drawing.Color.Black;
            this.LblJoueurNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblJoueurNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblJoueurNoir.ForeColor = System.Drawing.Color.White;
            this.LblJoueurNoir.Location = new System.Drawing.Point(10, 10);
            this.LblJoueurNoir.Name = "LblJoueurNoir";
            this.LblJoueurNoir.Size = new System.Drawing.Size(128, 20);
            this.LblJoueurNoir.TabIndex = 5;
            this.LblJoueurNoir.Text = "Joueur Noir";
            this.LblJoueurNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblEloNoir
            // 
            this.LblEloNoir.BackColor = System.Drawing.Color.Black;
            this.LblEloNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LblEloNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblEloNoir.ForeColor = System.Drawing.Color.White;
            this.LblEloNoir.Location = new System.Drawing.Point(147, 10);
            this.LblEloNoir.Name = "LblEloNoir";
            this.LblEloNoir.Size = new System.Drawing.Size(75, 20);
            this.LblEloNoir.TabIndex = 4;
            this.LblEloNoir.Text = "Elo Noir";
            this.LblEloNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gourpeBoutons
            // 
            this.gourpeBoutons.BackColor = System.Drawing.Color.LightGray;
            this.gourpeBoutons.Controls.Add(this.BoutonFin);
            this.gourpeBoutons.Controls.Add(this.BoutonGauche);
            this.gourpeBoutons.Controls.Add(this.BoutonDroit);
            this.gourpeBoutons.Controls.Add(this.BoutonDebut);
            this.gourpeBoutons.Location = new System.Drawing.Point(32, 594);
            this.gourpeBoutons.Name = "gourpeBoutons";
            this.gourpeBoutons.Size = new System.Drawing.Size(164, 49);
            this.gourpeBoutons.TabIndex = 6;
            this.gourpeBoutons.TabStop = false;
            this.gourpeBoutons.Text = "Parcours de la partie";
            // 
            // BoutonFin
            // 
            this.BoutonFin.BackColor = System.Drawing.Color.Silver;
            this.BoutonFin.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.BoutonFin.FlatAppearance.BorderSize = 3;
            this.BoutonFin.Image = ((System.Drawing.Image)(resources.GetObject("BoutonFin.Image")));
            this.BoutonFin.Location = new System.Drawing.Point(121, 13);
            this.BoutonFin.Name = "BoutonFin";
            this.BoutonFin.Size = new System.Drawing.Size(32, 30);
            this.BoutonFin.TabIndex = 3;
            this.BoutonFin.UseVisualStyleBackColor = true;
            this.BoutonFin.Click += new System.EventHandler(this.BoutonFin_Click);
            // 
            // BoutonGauche
            // 
            this.BoutonGauche.BackColor = System.Drawing.Color.Silver;
            this.BoutonGauche.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.BoutonGauche.FlatAppearance.BorderSize = 3;
            this.BoutonGauche.Image = ((System.Drawing.Image)(resources.GetObject("BoutonGauche.Image")));
            this.BoutonGauche.Location = new System.Drawing.Point(45, 13);
            this.BoutonGauche.Name = "BoutonGauche";
            this.BoutonGauche.Size = new System.Drawing.Size(32, 30);
            this.BoutonGauche.TabIndex = 2;
            this.BoutonGauche.UseVisualStyleBackColor = true;
            this.BoutonGauche.Click += new System.EventHandler(this.BoutonGauche_Click);
            // 
            // BoutonDroit
            // 
            this.BoutonDroit.BackColor = System.Drawing.Color.Silver;
            this.BoutonDroit.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.BoutonDroit.FlatAppearance.BorderSize = 3;
            this.BoutonDroit.Image = ((System.Drawing.Image)(resources.GetObject("BoutonDroit.Image")));
            this.BoutonDroit.Location = new System.Drawing.Point(83, 13);
            this.BoutonDroit.Name = "BoutonDroit";
            this.BoutonDroit.Size = new System.Drawing.Size(32, 30);
            this.BoutonDroit.TabIndex = 1;
            this.BoutonDroit.UseVisualStyleBackColor = true;
            this.BoutonDroit.Click += new System.EventHandler(this.BoutonDroit_Click);
            // 
            // BoutonDebut
            // 
            this.BoutonDebut.BackColor = System.Drawing.Color.Silver;
            this.BoutonDebut.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.BoutonDebut.FlatAppearance.BorderSize = 3;
            this.BoutonDebut.Image = ((System.Drawing.Image)(resources.GetObject("BoutonDebut.Image")));
            this.BoutonDebut.Location = new System.Drawing.Point(7, 13);
            this.BoutonDebut.Name = "BoutonDebut";
            this.BoutonDebut.Size = new System.Drawing.Size(32, 30);
            this.BoutonDebut.TabIndex = 0;
            this.BoutonDebut.UseVisualStyleBackColor = true;
            this.BoutonDebut.Click += new System.EventHandler(this.BoutonDebut_Click);
            // 
            // NumeroLigneColumn1
            // 
            this.NumeroLigneColumn1.Frozen = true;
            this.NumeroLigneColumn1.HeaderText = "N° coup";
            this.NumeroLigneColumn1.Name = "NumeroLigneColumn1";
            this.NumeroLigneColumn1.Width = 50;
            // 
            // BlancsColumn2
            // 
            this.BlancsColumn2.Frozen = true;
            this.BlancsColumn2.HeaderText = "Blancs";
            this.BlancsColumn2.Name = "BlancsColumn2";
            this.BlancsColumn2.Width = 75;
            // 
            // NoirsColumn3
            // 
            this.NoirsColumn3.Frozen = true;
            this.NoirsColumn3.HeaderText = "Noirs";
            this.NoirsColumn3.Name = "NoirsColumn3";
            this.NoirsColumn3.Width = 75;
            // 
            // FenetrePartie
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(232, 678);
            this.Controls.Add(this.gourpeBoutons);
            this.Controls.Add(this.LblJoueurNoir);
            this.Controls.Add(this.LblEloNoir);
            this.Controls.Add(this.LblJoueurBlanc);
            this.Controls.Add(this.LblEloBlanc);
            this.Controls.Add(this.FermeFeuillePartie);
            this.Controls.Add(this.FeuillePartie);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FenetrePartie";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Fenetre Partie";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FenetrePartie_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.FeuillePartie)).EndInit();
            this.gourpeBoutons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.DataGridView FeuillePartie;
        private Krypton.Toolkit.KryptonButton FermeFeuillePartie;
        public System.Windows.Forms.Label LblJoueurBlanc;
        public System.Windows.Forms.Label LblEloBlanc;
        public System.Windows.Forms.Label LblJoueurNoir;
        public System.Windows.Forms.Label LblEloNoir;
        private System.Windows.Forms.GroupBox gourpeBoutons;
        private System.Windows.Forms.Button BoutonDebut;
        private System.Windows.Forms.Button BoutonFin;
        private System.Windows.Forms.Button BoutonGauche;
        private System.Windows.Forms.Button BoutonDroit;
        private System.Windows.Forms.DataGridViewTextBoxColumn NumeroLigneColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn BlancsColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn NoirsColumn3;
    }
}