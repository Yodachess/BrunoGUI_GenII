// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre de suivi de l'analyse de partie (choix de Bruno : la barre sous la feuille était trop discrète) ...
//      └─ Classe "FenetreAnalyse" (KryptonForm construite en code, non bloquante, ouverte et fermée par EchiquierPrincipal)
//                      ├─ "MontreEtape"    Nouvelle position demandée au moteur : étape, position, progression, temps restant
//                      ├─ "MontreScore"    Score et meilleure suite du moteur, au fil de sa réflexion
//                      └─ événement "Interrompre"
// La croix ne fait que fermer la fenêtre : l'analyse continue (le bouton « Interrompre » sous la feuille reste disponible).

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
        private static readonly Font PoliceNormale = new("Segoe UI", 9.5F), PoliceGrasse = new("Segoe UI", 10.5F, FontStyle.Bold);
        private readonly Label _etape = Ligne(12, 12, gras: true);
        private readonly Label _position = Ligne(12, 40);
        private readonly Label _score = Ligne(12, 64, gras: true);
        private readonly Label _suite = new()
        {   // la meilleure suite peut prendre deux lignes
            Location = new Point(12, 88), Size = new Size(436, 38), AutoEllipsis = true, BackColor = Color.Transparent,
            Font = PoliceNormale
        };
        private readonly Label _temps = Ligne(12, 130);
        private readonly ProgressBar _barre = new() { Location = new Point(12, 158), Size = new Size(436, 16) };
        public event Action? Interrompre;
        protected override bool ShowWithoutActivation => true;      // la fenêtre principale garde le clavier (flèches du parcours)

        public FenetreAnalyse(bool complete)
        {
            Text = complete ? T("Analyse complète de la partie") : T("Analyse rapide de la partie");
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            ClientSize = new Size(460, 222);
            KryptonButton interrompre = new() { Text = T("Interrompre"), Location = new Point(330, 184), Size = new Size(118, 28) };
            interrompre.Click += (s, e) => Interrompre?.Invoke();
            Controls.AddRange([_etape, _position, _score, _suite, _temps, _barre, interrompre]);
            _score.ForeColor = Color.DarkGreen;
        }

        private static Label Ligne(int x, int y, bool gras = false) => new()
        {
            Location = new Point(x, y), Size = new Size(436, 22), AutoEllipsis = true, BackColor = Color.Transparent,
            Font = gras ? PoliceGrasse : PoliceNormale
        };

        public void MontreEtape(string etape, string position, int faites, int total, string tempsRestant)
        {   // Le moteur commence une nouvelle position : son score et sa suite seront affichés au fil de sa réflexion
            _etape.Text = etape;
            _position.Text = position;
            _score.Text = T("Score : …");
            _suite.Text = "";
            _temps.Text = tempsRestant;
            _barre.Maximum = Math.Max(1, total);
            _barre.Value = Math.Clamp(faites, 0, _barre.Maximum);
        }

        public void MontreScore(string score, string suite)
        {
            _score.Text = score;
            _suite.Text = suite;
        }
    }
}
