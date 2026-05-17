// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre d'édition pour nouvellle partie contre Stockfish ...
//      └─ Classe "PartieForceModule" 
//                      ├─ "PartieForceModule"     (Init)
//                      ├─ "NouvellePartieForceModule_Load"
//                      ├─ "ForceMoteurMaximum_CheckedChanged"  
//                      ├─ "ForceMoteurDefinie_CheckedChanged"  
//                      ├─ "ForceMoteurOk_Click"  
//                      └─ "ForceMoteurAnnuler_Click"

using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace BrunoGUI_GenII
{
    public partial class PartieForceModule : Form
    {
        public string ChoixCouleur { get; set; }
        public string NomAdversaire { get; set; }
        public int ForceModule { get; set; }
        public int DureeReflexionSeconde { get; set; }
        public bool ForceMaximale { get; set; }

        public PartieForceModule()
        {
            InitializeComponent();
        }
        private void NouvellePartieForceModule_Load(object sender, EventArgs e)
        {
            ModuleJoueNoirs.Checked = true;     // Les Noirs par défaut
            ForceMoteurMaximum.Checked = true;  // Force Maximum par défaut
            DureeReflexionSeconde = 2;
            ValeurLimiteElo.Value = 1850;       // Valeur par défaut du ELO si pas max
            NomAdversaire = "Bruno";
            TextBoxNomAdvesaire.Text = NomAdversaire;
            ValeurLimiteElo.Enabled = false;    // Désactiver le NumericUpDown par défaut
            TempsReflexion.Enabled = true;

            // Méthodes séparées pour la gestion des événements
            ForceMoteurMaximum.CheckedChanged += ForceMoteurMaximum_CheckedChanged;
            ForceMoteurDefinie.CheckedChanged += ForceMoteurDefinie_CheckedChanged;
            ForceMoteurOk.Click += ForceMoteurOk_Click;
            ForceMoteurAnnuler.Click += ForceMoteurAnnuler_Click;
        }
        private void ForceMoteurMaximum_CheckedChanged(object sender, EventArgs e)
        {
            ValeurLimiteElo.Enabled = !ForceMoteurMaximum.Checked;
        }
        private void ForceMoteurDefinie_CheckedChanged(object sender, EventArgs e)
        {
            ValeurLimiteElo.Enabled = ForceMoteurDefinie.Checked;
        }
        private void ForceMoteurOk_Click(object sender, EventArgs e)
        {
            ChoixCouleur = ModuleJoueBlancs.Checked ? "Blancs" : "Noirs";
            NomAdversaire = TextBoxNomAdvesaire.Text;
            ForceMaximale = ForceMoteurMaximum.Checked;
            ForceModule = (int)ValeurLimiteElo.Value;
            DureeReflexionSeconde = ((int)TempsReflexion.Value);
            Debug.WriteLine($"ForceMoteurOk_Click / Durée Réflexion secondes =  {DureeReflexionSeconde}");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void ForceMoteurAnnuler_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
