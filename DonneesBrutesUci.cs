// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public partial class DonneesBrutesUci : Form
    {
        // Seules les dernières lignes sont gardées : sinon la fenêtre accumule tout depuis le lancement (même masquée)
        // et chaque nouvelle ligne coûte de plus en plus cher. On retaille par paquets (au-delà de LignesMaximum + 500)
        private const int LignesMaximum = 2000;
        private int _nombreLignes;

        public DonneesBrutesUci()
        {   // Le formulaire ne se ferme jamais, même si l’utilisateur clique sur la croix. Il est simplement caché.
            InitializeComponent();
            this.FormClosing += DonneesBrutesUci_FormClosing;   // Gestion du click sur la croix rouge en haut à droite ...
        }
        public void AjouteLigne(string texte)
        {   // Ajoute une ligne du protocole (à appeler sur le thread de l'interface)
            if (IsDisposed)
                return;
            DonneesBrutesVue.AppendText(Environment.NewLine + texte);
            if (++_nombreLignes > LignesMaximum + 500)
            {
                string[] lignes = DonneesBrutesVue.Lines;
                DonneesBrutesVue.Lines = lignes[^Math.Min(LignesMaximum, lignes.Length)..];
                _nombreLignes = LignesMaximum;
            }
            if (Visible)
                DonneesBrutesVue.ScrollToCaret();   // la fenêtre montre toujours les dernières lignes
        }
        private void DonneesBrutesUci_FormClosing(object sender, FormClosingEventArgs e)
        {   // Gestion du click sur la croix rouge en haut à droite ...
            e.Cancel = true;
            this.Hide();
        }
    }
}
