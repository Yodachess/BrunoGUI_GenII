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
        public LogiqueMouvements.ColorPiece ChoixCouleur { get; set; } = LogiqueMouvements.ColorPiece.Noir;    // camp joué par le moteur
        public string NomAdversaire { get; set; } = "Bruno";    // nom du joueur humain
        public int ForceModule { get; set; } = 1850;            // Elo du moteur si pas force maximale
        public int DureeReflexionSeconde { get; set; } = 5;
        public bool ForceMaximale { get; set; } = true;
        public Cadence ChoixCadence { get; set; } = Cadence.SansPendule;    // pendule de la partie (sans : temps fixe par coup)

        public PartieForceModule()
        {
            InitializeComponent();
            foreach (Cadence cadence in Cadence.Proposees)
                ListePendule.Items.Add(cadence);
            this.VisibleChanged += (s, e) => { if (Visible) AfficheChoix(); };
        }
        private void ListePendule_SelectedIndexChanged(object sender, EventArgs e)
        {   // La durée de réflexion par coup ne sert que sans pendule (avec une pendule, le moteur gère son temps)
            TempsReflexion.Enabled = ListePendule.SelectedItem is not Cadence cadence || cadence.EstSansPendule;
        }
        private void AfficheChoix()
        {   // Met les contrôles de la fenêtre aux valeurs des propriétés
            ModuleJoueBlancs.Checked = ChoixCouleur == LogiqueMouvements.ColorPiece.Blanc;
            ModuleJoueNoirs.Checked = ChoixCouleur != LogiqueMouvements.ColorPiece.Blanc;
            ForceMoteurMaximum.Checked = ForceMaximale;
            ForceMoteurDefinie.Checked = !ForceMaximale;
            ValeurLimiteElo.Value = Math.Clamp(ForceModule, (int)ValeurLimiteElo.Minimum, (int)ValeurLimiteElo.Maximum);
            ValeurLimiteElo.Enabled = !ForceMaximale;
            TempsReflexion.Value = Math.Clamp(DureeReflexionSeconde, (int)TempsReflexion.Minimum, (int)TempsReflexion.Maximum);
            TextBoxNomAdvesaire.Text = NomAdversaire;
            if (!ListePendule.Items.Contains(ChoixCadence))
                ListePendule.Items.Add(ChoixCadence);       // cadence du .ini absente de la liste proposée
            ListePendule.SelectedItem = ChoixCadence;
            ListePendule_SelectedIndexChanged(ListePendule, EventArgs.Empty);
        }
        private void NouvellePartieForceModule_Load(object sender, EventArgs e)
        {
            AfficheChoix();

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
            ChoixCouleur = ModuleJoueBlancs.Checked ? LogiqueMouvements.ColorPiece.Blanc : LogiqueMouvements.ColorPiece.Noir;
            NomAdversaire = TextBoxNomAdvesaire.Text;
            ForceMaximale = ForceMoteurMaximum.Checked;
            ForceModule = (int)ValeurLimiteElo.Value;
            DureeReflexionSeconde = ((int)TempsReflexion.Value);
            if (ListePendule.SelectedItem is Cadence cadence)
                ChoixCadence = cadence;
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
