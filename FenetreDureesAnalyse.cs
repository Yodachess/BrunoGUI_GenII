// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre « Durées de l'analyse de partie » (menu Options) ...
//      └─ Classe "FenetreDureesAnalyse" (KryptonForm construite en code, boîte de dialogue)
// Secondes par position pour l'analyse rapide (1er passage) et pour la vérification des coups douteux (2e passage de l'analyse
// complète) ; réglables, 10 et 30 s par défaut, mémorisées dans les préférences

using System;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.Langue;

namespace BrunoGUI_GenII
{
    public class FenetreDureesAnalyse : KryptonForm
    {
        public const int Minimum = 1, Maximum = 600;      // secondes par position
        private readonly KryptonNumericUpDown _rapide = Champ(28);
        private readonly KryptonNumericUpDown _complete = Champ(66);
        public int DureeRapide => (int)_rapide.Value;
        public int DureeComplete => (int)_complete.Value;

        public FenetreDureesAnalyse(int dureeRapide, int dureeComplete)
        {
            Text = T("Durées de l'analyse de partie");
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(470, 180);
            _rapide.Value = Math.Clamp(dureeRapide, Minimum, Maximum);
            _complete.Value = Math.Clamp(dureeComplete, Minimum, Maximum);
            KryptonButton ok = new() { Text = "OK", Location = new Point(250, 140), Size = new Size(100, 28), DialogResult = DialogResult.OK };
            KryptonButton annuler = new() { Text = T("Annuler"), Location = new Point(360, 140), Size = new Size(100, 28), DialogResult = DialogResult.Cancel };
            Controls.AddRange([
                Libelle(T("Analyse rapide : secondes par position"), 30),
                _rapide,
                Libelle(T("Analyse complète : secondes par position revue (coups douteux)"), 68),
                _complete,
                Libelle(T("Une partie de 40 coups compte environ 80 positions."), 106),
                ok, annuler]);
            AcceptButton = ok;
            CancelButton = annuler;
        }

        private static KryptonLabel Libelle(string texte, int y) => new() { Text = texte, Location = new Point(12, y), AutoSize = true };
        private static KryptonNumericUpDown Champ(int y) => new() { Location = new Point(390, y), Width = 70, Minimum = Minimum, Maximum = Maximum };
    }
}
