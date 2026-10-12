// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre de suivi de l'analyse de partie (choix de Bruno : la barre sous la feuille était trop discrète) ...
//      └─ Classe "FenetreAnalyse" (KryptonForm construite en code, non bloquante, ouverte et fermée par EchiquierPrincipal)
//                      ├─ "MontreEtape"    Nouvelle position demandée au moteur : position et progression, temps restant
//                      ├─ "MontreScore"    Score et meilleure suite du moteur, au fil de sa réflexion
//                      └─ événement "Interrompre"
// Disposition compacte (choix de Bruno) : « Position après 25... c5 (12/54)   Score : 0.35 — Léger avantage Blanc (⩲) » sur une ligne, la meilleure suite,
// la barre, puis le temps restant à gauche du bouton Interrompre. La croix ne fait que fermer la fenêtre : l'analyse continue
// (le bouton « Interrompre » sous la feuille reste disponible).

using System;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.Langue;

namespace BrunoGUI_GenII
{
    public class FenetreAnalyse : KryptonForm
    {
        // Polices partagées (une fenêtre par analyse : des polices créées à chaque fois ne seraient jamais libérées)
        private static readonly Font PoliceNormale = new("Segoe UI", 9.5F), PoliceGrasse = new("Segoe UI", 10F, FontStyle.Bold);
        private const int Largeur = 600;     // la 1re ligne va jusqu'à l'appréciation : « Score : 0.35 — Léger avantage Blanc (⩲) »
        private readonly Label _entete = Ligne(10, 22, PoliceGrasse);           // position, progression et score
        private readonly Label _suite = Ligne(34, 38, PoliceNormale);           // meilleure suite (deux lignes au plus)
        private readonly ProgressBar _barre = new() { Location = new Point(12, 76), Size = new Size(Largeur, 14) };
        private readonly Label _temps = Ligne(100, 28, PoliceNormale, Largeur - 130);
        private string _position = "", _score = "";
        public event Action? Interrompre;
        protected override bool ShowWithoutActivation => true;      // la fenêtre principale garde le clavier (flèches du parcours)

        public FenetreAnalyse(bool complete)
        {
            Text = complete ? T("Analyse complète de la partie") : T("Analyse rapide de la partie");
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            ClientSize = new Size(Largeur + 24, 136);
            _temps.TextAlign = ContentAlignment.MiddleLeft;     // à la hauteur du bouton
            _entete.ForeColor = Color.DarkGreen;
            KryptonButton interrompre = new() { Text = T("Interrompre"), Location = new Point(Largeur + 12 - 118, 100), Size = new Size(118, 28) };
            interrompre.Click += (s, e) => Interrompre?.Invoke();
            Controls.AddRange([_entete, _suite, _barre, _temps, interrompre]);
        }

        private static Label Ligne(int y, int hauteur, Font police, int largeur = Largeur) => new()
        {
            Location = new Point(12, y), Size = new Size(largeur, hauteur), AutoEllipsis = true, BackColor = Color.Transparent, Font = police
        };

        public void MontreEtape(string position, string progression, int faites, int total, string tempsRestant)
        {   // Le moteur commence une nouvelle position : son score et sa suite seront affichés au fil de sa réflexion
            _position = $"{position} ({progression})";
            _score = T("Score : …");
            _entete.Text = _position + "   " + _score;
            _suite.Text = "";
            _temps.Text = tempsRestant;
            _barre.Maximum = Math.Max(1, total);
            _barre.Value = Math.Clamp(faites, 0, _barre.Maximum);
        }

        public void MontreScore(string score, string suite)
        {
            _score = score;
            _entete.Text = _position + "   " + _score;
            _suite.Text = suite;
        }
    }
}
