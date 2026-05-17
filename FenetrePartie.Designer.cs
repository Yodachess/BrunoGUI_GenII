// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

namespace BrunoGUI_GenII
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
            FeuillePartie = new System.Windows.Forms.DataGridView();
            NumeroLigneColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            BlancsColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            NoirsColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            FermeFeuillePartie = new Krypton.Toolkit.KryptonButton();
            LblJoueurBlanc = new System.Windows.Forms.Label();
            LblEloBlanc = new System.Windows.Forms.Label();
            LblJoueurNoir = new System.Windows.Forms.Label();
            LblEloNoir = new System.Windows.Forms.Label();
            gourpeBoutons = new System.Windows.Forms.GroupBox();
            BoutonFin = new System.Windows.Forms.Button();
            BoutonGauche = new System.Windows.Forms.Button();
            BoutonDroit = new System.Windows.Forms.Button();
            BoutonDebut = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)FeuillePartie).BeginInit();
            gourpeBoutons.SuspendLayout();
            SuspendLayout();
            // 
            // FeuillePartie
            // 
            FeuillePartie.AllowUserToAddRows = false;
            FeuillePartie.AllowUserToResizeColumns = false;
            FeuillePartie.BackgroundColor = System.Drawing.Color.LightSteelBlue;
            FeuillePartie.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            FeuillePartie.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            FeuillePartie.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { NumeroLigneColumn1, BlancsColumn2, NoirsColumn3 });
            FeuillePartie.Location = new System.Drawing.Point(12, 69);
            FeuillePartie.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            FeuillePartie.Name = "FeuillePartie";
            FeuillePartie.RowHeadersVisible = false;
            FeuillePartie.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            FeuillePartie.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            FeuillePartie.Size = new System.Drawing.Size(248, 608);
            FeuillePartie.TabIndex = 0;
            // 
            // NumeroLigneColumn1
            // 
            NumeroLigneColumn1.Frozen = true;
            NumeroLigneColumn1.HeaderText = "N° coup";
            NumeroLigneColumn1.Name = "NumeroLigneColumn1";
            NumeroLigneColumn1.Width = 50;
            // 
            // BlancsColumn2
            // 
            BlancsColumn2.Frozen = true;
            BlancsColumn2.HeaderText = "Blancs";
            BlancsColumn2.Name = "BlancsColumn2";
            BlancsColumn2.Width = 75;
            // 
            // NoirsColumn3
            // 
            NoirsColumn3.Frozen = true;
            NoirsColumn3.HeaderText = "Noirs";
            NoirsColumn3.Name = "NoirsColumn3";
            NoirsColumn3.Width = 75;
            // 
            // FermeFeuillePartie
            // 
            FermeFeuillePartie.Location = new System.Drawing.Point(79, 749);
            FermeFeuillePartie.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            FermeFeuillePartie.Name = "FermeFeuillePartie";
            FermeFeuillePartie.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            FermeFeuillePartie.Size = new System.Drawing.Size(105, 29);
            FermeFeuillePartie.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            FermeFeuillePartie.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            FermeFeuillePartie.StateCommon.Border.Rounding = 20F;
            FermeFeuillePartie.StateCommon.Border.Width = 3;
            FermeFeuillePartie.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            FermeFeuillePartie.TabIndex = 1;
            FermeFeuillePartie.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            FermeFeuillePartie.Values.Text = "Fermer";
            FermeFeuillePartie.Click += FermeFeuillePartie_Click;
            // 
            // LblJoueurBlanc
            // 
            LblJoueurBlanc.BackColor = System.Drawing.Color.White;
            LblJoueurBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblJoueurBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblJoueurBlanc.Location = new System.Drawing.Point(12, 38);
            LblJoueurBlanc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            LblJoueurBlanc.Name = "LblJoueurBlanc";
            LblJoueurBlanc.Size = new System.Drawing.Size(149, 23);
            LblJoueurBlanc.TabIndex = 3;
            LblJoueurBlanc.Text = "Joueur Blanc";
            LblJoueurBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblEloBlanc
            // 
            LblEloBlanc.BackColor = System.Drawing.Color.White;
            LblEloBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblEloBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblEloBlanc.Location = new System.Drawing.Point(172, 38);
            LblEloBlanc.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            LblEloBlanc.Name = "LblEloBlanc";
            LblEloBlanc.Size = new System.Drawing.Size(88, 23);
            LblEloBlanc.TabIndex = 2;
            LblEloBlanc.Text = "Elo Blanc";
            LblEloBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblJoueurNoir
            // 
            LblJoueurNoir.BackColor = System.Drawing.Color.Black;
            LblJoueurNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblJoueurNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblJoueurNoir.ForeColor = System.Drawing.Color.White;
            LblJoueurNoir.Location = new System.Drawing.Point(12, 12);
            LblJoueurNoir.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            LblJoueurNoir.Name = "LblJoueurNoir";
            LblJoueurNoir.Size = new System.Drawing.Size(149, 23);
            LblJoueurNoir.TabIndex = 5;
            LblJoueurNoir.Text = "Joueur Noir";
            LblJoueurNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblEloNoir
            // 
            LblEloNoir.BackColor = System.Drawing.Color.Black;
            LblEloNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LblEloNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LblEloNoir.ForeColor = System.Drawing.Color.White;
            LblEloNoir.Location = new System.Drawing.Point(172, 12);
            LblEloNoir.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            LblEloNoir.Name = "LblEloNoir";
            LblEloNoir.Size = new System.Drawing.Size(88, 23);
            LblEloNoir.TabIndex = 4;
            LblEloNoir.Text = "Elo Noir";
            LblEloNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gourpeBoutons
            // 
            gourpeBoutons.BackColor = System.Drawing.Color.LightGray;
            gourpeBoutons.Controls.Add(BoutonFin);
            gourpeBoutons.Controls.Add(BoutonGauche);
            gourpeBoutons.Controls.Add(BoutonDroit);
            gourpeBoutons.Controls.Add(BoutonDebut);
            gourpeBoutons.Location = new System.Drawing.Point(37, 685);
            gourpeBoutons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gourpeBoutons.Name = "gourpeBoutons";
            gourpeBoutons.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gourpeBoutons.Size = new System.Drawing.Size(191, 57);
            gourpeBoutons.TabIndex = 6;
            gourpeBoutons.TabStop = false;
            gourpeBoutons.Text = "Parcours de la partie";
            // 
            // BoutonFin
            // 
            BoutonFin.BackColor = System.Drawing.Color.Silver;
            BoutonFin.FlatAppearance.BorderColor = System.Drawing.Color.White;
            BoutonFin.FlatAppearance.BorderSize = 3;
            BoutonFin.Image = (System.Drawing.Image)resources.GetObject("BoutonFin.Image");
            BoutonFin.Location = new System.Drawing.Point(141, 15);
            BoutonFin.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BoutonFin.Name = "BoutonFin";
            BoutonFin.Size = new System.Drawing.Size(37, 35);
            BoutonFin.TabIndex = 3;
            BoutonFin.UseVisualStyleBackColor = true;
            BoutonFin.Click += BoutonFin_Click;
            // 
            // BoutonGauche
            // 
            BoutonGauche.BackColor = System.Drawing.Color.Silver;
            BoutonGauche.FlatAppearance.BorderColor = System.Drawing.Color.White;
            BoutonGauche.FlatAppearance.BorderSize = 3;
            BoutonGauche.Image = (System.Drawing.Image)resources.GetObject("BoutonGauche.Image");
            BoutonGauche.Location = new System.Drawing.Point(52, 15);
            BoutonGauche.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BoutonGauche.Name = "BoutonGauche";
            BoutonGauche.Size = new System.Drawing.Size(37, 35);
            BoutonGauche.TabIndex = 2;
            BoutonGauche.UseVisualStyleBackColor = true;
            BoutonGauche.Click += BoutonGauche_Click;
            // 
            // BoutonDroit
            // 
            BoutonDroit.BackColor = System.Drawing.Color.Silver;
            BoutonDroit.FlatAppearance.BorderColor = System.Drawing.Color.White;
            BoutonDroit.FlatAppearance.BorderSize = 3;
            BoutonDroit.Image = (System.Drawing.Image)resources.GetObject("BoutonDroit.Image");
            BoutonDroit.Location = new System.Drawing.Point(97, 15);
            BoutonDroit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BoutonDroit.Name = "BoutonDroit";
            BoutonDroit.Size = new System.Drawing.Size(37, 35);
            BoutonDroit.TabIndex = 1;
            BoutonDroit.UseVisualStyleBackColor = true;
            BoutonDroit.Click += BoutonDroit_Click;
            // 
            // BoutonDebut
            // 
            BoutonDebut.BackColor = System.Drawing.Color.Silver;
            BoutonDebut.FlatAppearance.BorderColor = System.Drawing.Color.White;
            BoutonDebut.FlatAppearance.BorderSize = 3;
            BoutonDebut.Image = (System.Drawing.Image)resources.GetObject("BoutonDebut.Image");
            BoutonDebut.Location = new System.Drawing.Point(8, 15);
            BoutonDebut.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BoutonDebut.Name = "BoutonDebut";
            BoutonDebut.Size = new System.Drawing.Size(37, 35);
            BoutonDebut.TabIndex = 0;
            BoutonDebut.UseVisualStyleBackColor = true;
            BoutonDebut.Click += BoutonDebut_Click;
            // 
            // FenetrePartie
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(271, 782);
            Controls.Add(gourpeBoutons);
            Controls.Add(LblJoueurNoir);
            Controls.Add(LblEloNoir);
            Controls.Add(LblJoueurBlanc);
            Controls.Add(LblEloBlanc);
            Controls.Add(FermeFeuillePartie);
            Controls.Add(FeuillePartie);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "FenetrePartie";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Fenetre Partie";
            FormClosing += FenetrePartie_FormClosing;
            ((System.ComponentModel.ISupportInitialize)FeuillePartie).EndInit();
            gourpeBoutons.ResumeLayout(false);
            ResumeLayout(false);

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