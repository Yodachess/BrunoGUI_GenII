// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre d'édition des paramètres base de Stockfish ...
// └─ Classe "ParametresDeBase" 
//              ├─ "ParametresDeBase"     (Init)
//              ├─ "BaseBoutonOk_Click"
//              ├─ "BaseBoutonAnnuler_Click"  
//              └─ "ParametresDeBase_FormClosing"

using System;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public partial class ParametresDeBase : Form
    {
        private readonly EchiquierPrincipal interfaceGraphique;     // Stocke la référence de la classe principale
        public ParametresDeBase(EchiquierPrincipal brunoigInstance)
        {
            InitializeComponent();
            interfaceGraphique = brunoigInstance;
            this.FormClosing += ParametresDeBase_FormClosing;   // Gestion du click sur la croix rouge en haut à droite ...
            this.VisibleChanged += ParametresDeBase_VisibleChanged;
        }
        private void ParametresDeBase_VisibleChanged(object sender, EventArgs e)
        {   // A chaque affichage, on montre le nombre de variantes actuellement demandé au moteur
            if (Visible)
                baseMultipvNumerique.Value = Math.Clamp(MoteurUci.NombreLignesPV, (int)baseMultipvNumerique.Minimum, (int)baseMultipvNumerique.Maximum);
        }
        private void BaseBoutonOk_Click(object sender, EventArgs e)
        {
            PartieForceModule maNouvellePartieForceModule = new();
            MoteurUci.DefinitLimiteElo(baseEloNumerique.Value.ToString());
            MoteurUci.StandardInputDataToUci("setoption name Threads value " + (int)baseThreadsNumerique.Value);
            MoteurUci.DefinitMultiPV((int)baseMultipvNumerique.Value);
            maNouvellePartieForceModule.DureeReflexionSeconde = interfaceGraphique.TrackBarTempsReflexion.Value = ((int)baseReflexionNumerique.Value);
            if (interfaceGraphique.OrdinateurJoueNoir)
                interfaceGraphique.PartieEnCours.BlackElo = interfaceGraphique.EloNoir.Text = baseEloNumerique.Value.ToString();
            else
                interfaceGraphique.PartieEnCours.WhiteElo = interfaceGraphique.EloBlanc.Text = baseEloNumerique.Value.ToString();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
        private void BaseBoutonAnnuler_Click(object sender, EventArgs e)
        {   // Le formulaire ne se ferme jamais, même si l’utilisateur clique sur la croix. Il est simplement caché.
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }
        private void ParametresDeBase_FormClosing(object sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true; // Annule la fermeture de la fenêtre
            this.Hide();     // Masque la fenêtre
        }
    }
}
