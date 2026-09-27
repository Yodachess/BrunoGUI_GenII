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
        // Choix proposés à l'ouverture de la fenêtre (préférences du .ini, puis derniers choix), et choix validés
        public string ChoixCouleur { get; set; } = "Noirs";     // couleur jouée par le moteur
        public string NomAdversaire { get; set; } = "Bruno";    // nom du joueur humain
        public int ForceModule { get; set; } = 1850;            // Elo du moteur si pas force maximale
        public int DureeReflexionSeconde { get; set; } = 5;
        public bool ForceMaximale { get; set; } = true;

        public PartieForceModule()
        {
            InitializeComponent();
            this.VisibleChanged += (s, e) => { if (Visible) AfficheChoix(); };
        }
        private void AfficheChoix()
        {   // Met les contrôles de la fenêtre aux valeurs des propriétés
            ModuleJoueBlancs.Checked = ChoixCouleur == "Blancs";
            ModuleJoueNoirs.Checked = ChoixCouleur != "Blancs";
            ForceMoteurMaximum.Checked = ForceMaximale;
            ForceMoteurDefinie.Checked = !ForceMaximale;
            ValeurLimiteElo.Value = Math.Clamp(ForceModule, (int)ValeurLimiteElo.Minimum, (int)ValeurLimiteElo.Maximum);
            ValeurLimiteElo.Enabled = !ForceMaximale;
            TempsReflexion.Value = Math.Clamp(DureeReflexionSeconde, (int)TempsReflexion.Minimum, (int)TempsReflexion.Maximum);
            TextBoxNomAdvesaire.Text = NomAdversaire;
        }
        private void NouvellePartieForceModule_Load(object sender, EventArgs e)
        {
            AfficheChoix();
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
