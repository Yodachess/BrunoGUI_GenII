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
        private readonly MoteurUci MoteurUci;                       // le moteur réglé par cette fenêtre
        public ParametresDeBase(EchiquierPrincipal brunoigInstance)
        {
            InitializeComponent();
            TraductionFenetres.Traduit(this);      // textes du designer dans la langue choisie (voir Langue.cs)
            interfaceGraphique = brunoigInstance;
            MoteurUci = brunoigInstance.MoteurUci;
            this.FormClosing += ParametresDeBase_FormClosing;   // Gestion du click sur la croix rouge en haut à droite ...
            this.VisibleChanged += ParametresDeBase_VisibleChanged;
        }
        private void ParametresDeBase_VisibleChanged(object? sender, EventArgs e)
        {   // A chaque affichage, on montre les réglages actuels (variantes, threads, temps de réflexion)
            if (!Visible)
                return;
            baseMultipvNumerique.Value = Math.Clamp(MoteurUci.NombreLignesPV, (int)baseMultipvNumerique.Minimum, (int)baseMultipvNumerique.Maximum);
            if (MoteurUci.NombreThreads is int threads)
                baseThreadsNumerique.Value = Math.Clamp(threads, (int)baseThreadsNumerique.Minimum, (int)baseThreadsNumerique.Maximum);
            baseReflexionNumerique.Value = Math.Clamp(interfaceGraphique.TempsReflexionSecondes.Value, baseReflexionNumerique.Minimum, baseReflexionNumerique.Maximum);
            // Le moteur réglé est dans le titre, et l'Elo prend SA plage (2026-10-23 : Rodent IV va de 800 à 2800,
            // Stockfish de 1320 à 3190) ; un moteur qui ne règle pas sa force par l'Elo (Sargon) a le champ grisé
            Text = Langue.T("Paramètres du moteur — {0}", MoteurUci.NomAnnonce ?? Langue.T("moteur en cours"));
            if (MoteurUci.PlageElo is (int minimum, int maximum))
            {
                baseEloNumerique.Minimum = minimum;
                baseEloNumerique.Maximum = maximum;
                baseEloNumerique.Enabled = true;
                LabelBaseELo.Values.Text = Langue.T("Force Elo ({0} à {1}) : ", minimum, maximum);
            }
            else
            {
                baseEloNumerique.Enabled = false;
                LabelBaseELo.Values.Text = Langue.T("Force Elo : sans objet pour ce moteur");
            }
            if (MoteurUci.EloVoulu is int elo)     // (pleine force : la dernière valeur reste affichée)
                baseEloNumerique.Value = Math.Clamp(elo, (int)baseEloNumerique.Minimum, (int)baseEloNumerique.Maximum);
            _eloAffiche = baseEloNumerique.Value;
        }
        private decimal _eloAffiche;    // l'Elo montré à l'ouverture de la fenêtre
        private void BaseBoutonOk_Click(object? sender, EventArgs e)
        {
            MoteurUci.DefinitThreads((int)baseThreadsNumerique.Value);
            MoteurUci.DefinitMultiPV((int)baseMultipvNumerique.Value);
            var champ = interfaceGraphique.TempsReflexionSecondes;
            champ.Value = Math.Clamp(baseReflexionNumerique.Value, champ.Minimum, champ.Maximum);   // met aussi à jour le temps de réflexion
            // L'Elo n'est envoyé que s'il a été changé (ou s'il était déjà limité) : un moteur en pleine force y restait sinon limité,
            // alors qu'on n'avait ouvert la fenêtre que pour les threads ou les variantes
            if (baseEloNumerique.Enabled && (baseEloNumerique.Value != _eloAffiche || MoteurUci.EloVoulu != null))
                interfaceGraphique.DefinitEloMoteur((int)baseEloNumerique.Value);    // force du moteur, et Elo affiché au(x) camp(s) qu'il joue
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }
        private void BaseBoutonAnnuler_Click(object? sender, EventArgs e)
        {   // Le formulaire ne se ferme jamais, même si l’utilisateur clique sur la croix. Il est simplement caché.
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
        }
        private void ParametresDeBase_FormClosing(object? sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true; // Annule la fermeture de la fenêtre
            this.Hide();     // Masque la fenêtre
        }
    }
}
