// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    partial class FichierPartiePgn
    {
        private void InitializeComponent()
        {
            NombrePartiesFichier = new Label();
            lblDoubleClick = new Label();
            TableauPartiesPgn = new DataGridView();
            gridJoueurBlanc = new DataGridViewTextBoxColumn();
            gridEloBlanc = new DataGridViewTextBoxColumn();
            gridJoueurNoir = new DataGridViewTextBoxColumn();
            gridEloNoir = new DataGridViewTextBoxColumn();
            gridResultat = new DataGridViewTextBoxColumn();
            gridNombreCoups = new DataGridViewTextBoxColumn();
            gridCodeEco = new DataGridViewTextBoxColumn();
            gridTournoi = new DataGridViewTextBoxColumn();
            gridRonde = new DataGridViewTextBoxColumn();
            gridSite = new DataGridViewTextBoxColumn();
            gridDate = new DataGridViewTextBoxColumn();
            ((ISupportInitialize)TableauPartiesPgn).BeginInit();
            SuspendLayout();
            // 
            // NombrePartiesFichier
            // 
            NombrePartiesFichier.BackColor = Color.LightGreen;
            NombrePartiesFichier.Location = new Point(0, 0);
            NombrePartiesFichier.Name = "NombrePartiesFichier";
            NombrePartiesFichier.Size = new Size(563, 30);
            NombrePartiesFichier.TabIndex = 0;
            NombrePartiesFichier.Text = "Nombre de parties au format PGN";
            NombrePartiesFichier.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDoubleClick
            // 
            lblDoubleClick.BackColor = Color.Lime;
            lblDoubleClick.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDoubleClick.Location = new Point(557, 0);
            lblDoubleClick.Name = "lblDoubleClick";
            lblDoubleClick.Size = new Size(379, 30);
            lblDoubleClick.TabIndex = 1;
            lblDoubleClick.Text = "Double-cliquez sur la partie que vous voulez consulter";
            lblDoubleClick.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TableauPartiesPgn
            // 
            TableauPartiesPgn.BackgroundColor = Color.Silver;
            TableauPartiesPgn.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            TableauPartiesPgn.Columns.AddRange([gridJoueurBlanc, gridEloBlanc, gridJoueurNoir, gridEloNoir, gridResultat, gridNombreCoups, gridCodeEco, gridTournoi, gridRonde, gridSite, gridDate]);
            TableauPartiesPgn.Location = new Point(0, 33);
            TableauPartiesPgn.Name = "TableauPartiesPgn";
            TableauPartiesPgn.Size = new Size(933, 529);
            TableauPartiesPgn.TabIndex = 2;
            TableauPartiesPgn.CellDoubleClick += TableauPartiesPgn_CellDoubleClick;
            // 
            // gridJoueurBlanc
            // 
            gridJoueurBlanc.HeaderText = "Blancs";
            gridJoueurBlanc.Name = "gridJoueurBlanc";
            // 
            // gridEloBlanc
            // 
            gridEloBlanc.HeaderText = "ELO ";
            gridEloBlanc.Name = "gridEloBlanc";
            gridEloBlanc.Width = 50;
            // 
            // gridJoueurNoir
            // 
            gridJoueurNoir.HeaderText = "Noirs";
            gridJoueurNoir.Name = "gridJoueurNoir";
            // 
            // gridEloNoir
            // 
            gridEloNoir.HeaderText = "ELO";
            gridEloNoir.Name = "gridEloNoir";
            gridEloNoir.Width = 50;
            // 
            // gridResultat
            // 
            gridResultat.HeaderText = "Résultat";
            gridResultat.Name = "gridResultat";
            gridResultat.Width = 50;
            // 
            // gridNombreCoups
            // 
            gridNombreCoups.HeaderText = "Nbre coups";
            gridNombreCoups.Name = "gridNombreCoups";
            gridNombreCoups.Width = 50;
            // 
            // gridCodeEco
            // 
            gridCodeEco.HeaderText = "ECO";
            gridCodeEco.Name = "gridCodeEco";
            gridCodeEco.Width = 50;
            // 
            // gridTournoi
            // 
            gridTournoi.HeaderText = "Tournoi";
            gridTournoi.Name = "gridTournoi";
            gridTournoi.Width = 200;
            // 
            // gridRonde
            // 
            gridRonde.HeaderText = "Ronde";
            gridRonde.Name = "gridRonde";
            gridRonde.Width = 50;
            // 
            // gridSite
            // 
            gridSite.HeaderText = "Site";
            gridSite.Name = "gridSite";
            // 
            // gridDate
            // 
            gridDate.HeaderText = "Date";
            gridDate.Name = "gridDate";
            // 
            // FichierPartiePgn
            // 
            ClientSize = new Size(934, 561);
            Controls.Add(TableauPartiesPgn);
            Controls.Add(lblDoubleClick);
            Controls.Add(NombrePartiesFichier);
            Name = "FichierPartiePgn";
            Text = "Fichier de partie(s) au format PGN";
            ((ISupportInitialize)TableauPartiesPgn).EndInit();
            ResumeLayout(false);

        }
        public Label NombrePartiesFichier;
        private Label lblDoubleClick;
        private DataGridViewTextBoxColumn gridJoueurBlanc;
        private DataGridViewTextBoxColumn gridEloBlanc;
        private DataGridViewTextBoxColumn gridJoueurNoir;
        private DataGridViewTextBoxColumn gridEloNoir;
        private DataGridViewTextBoxColumn gridResultat;
        private DataGridViewTextBoxColumn gridNombreCoups;
        private DataGridViewTextBoxColumn gridCodeEco;
        private DataGridViewTextBoxColumn gridTournoi;
        private DataGridViewTextBoxColumn gridRonde;
        private DataGridViewTextBoxColumn gridSite;
        private DataGridViewTextBoxColumn gridDate;
        private DataGridView TableauPartiesPgn;
    }
}
