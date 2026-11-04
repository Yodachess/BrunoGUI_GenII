// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre « Commentaire » (clic droit sur un coup de la feuille) ...
//      └─ Classe "FenetreCommentaire" (KryptonForm construite en code, boîte de dialogue)
// Le texte du joueur sur un coup ; vide, il retire le commentaire. Entrée valide, Échap annule

using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.Langue;

namespace BrunoGUI_GenII
{
    public class FenetreCommentaire : KryptonForm
    {
        private readonly KryptonTextBox _texte = new() { Location = new Point(12, 12), Size = new Size(446, 90), Multiline = true };
        public string Texte => _texte.Text;

        public FenetreCommentaire(string nomDuCoup, string? commentaire)
        {
            Text = T("Commentaire de {0}", nomDuCoup);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(470, 150);
            _texte.Text = commentaire ?? "";
            KryptonLabel aide = new() { Text = T("Laissé vide, le commentaire est retiré."), Location = new Point(12, 114), AutoSize = true };
            KryptonButton ok = new() { Text = "OK", Location = new Point(280, 112), Size = new Size(85, 28), DialogResult = DialogResult.OK };
            KryptonButton annuler = new() { Text = T("Annuler"), Location = new Point(373, 112), Size = new Size(85, 28), DialogResult = DialogResult.Cancel };
            Controls.AddRange([_texte, aide, ok, annuler]);
            AcceptButton = ok;
            CancelButton = annuler;
            ActiveControl = _texte;
            Shown += (s, e) => _texte.Select(_texte.Text.Length, 0);    // le curseur à la fin du texte, pour le compléter
        }
    }
}
