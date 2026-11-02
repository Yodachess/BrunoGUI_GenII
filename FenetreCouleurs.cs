// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre « Couleurs des cases » (menu Affichage)
//      └─ Classe "FenetreCouleurs" (KryptonForm construite en code, boîte de dialogue)
// Chaque choix (pastille, ou « Autre… » : la fenêtre de Windows) est aussitôt montré sur le vrai échiquier par "apercu" ;
// le formulaire remet les anciennes couleurs si la fenêtre n'est pas validée. La fenêtre de Windows ne dit rien de la couleur
// en cours de choix : d'où les pastilles, pour voir tout de suite

using System;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.Langue;

namespace BrunoGUI_GenII
{
    public class FenetreCouleurs : KryptonForm
    {
        // Lichess, bois, vert, bleu, gris...
        private static readonly string[] PastillesClaires = ["#F0D9B5", "#F5E6C8", "#EEEED2", "#DEE3E6", "#E8E8E8", "#FFFFFF", "#FFFACD", "#E6D5B8"];
        private static readonly string[] PastillesSombres = ["#B58863", "#8B5A2B", "#769656", "#8CA2AD", "#7D7D7D", "#4B7399", "#A0522D", "#6E4E7E"];

        private readonly Action<Color, Color> _apercu;
        private readonly Panel _actuelleClaire = new() { Location = new Point(10, 24), Size = new Size(28, 22), BorderStyle = BorderStyle.Fixed3D };
        private readonly Panel _actuelleSombre = new() { Location = new Point(10, 68), Size = new Size(28, 22), BorderStyle = BorderStyle.Fixed3D };
        private Color _claire, _sombre;

        public FenetreCouleurs(Color claire, Color sombre, Action<Color, Color> apercu)
        {
            _claire = claire;
            _sombre = sombre;
            _apercu = apercu;
            Text = T("Couleurs des cases");
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(327, 133);
            _actuelleClaire.BackColor = claire;
            _actuelleSombre.BackColor = sombre;
            KryptonButton ok = new() { Text = "OK", Location = new Point(157, 100), Size = new Size(75, 26), DialogResult = DialogResult.OK };
            KryptonButton annuler = new() { Text = T("Annuler"), Location = new Point(242, 100), Size = new Size(75, 26), DialogResult = DialogResult.Cancel };
            Controls.AddRange([
                new KryptonLabel { Text = T("Cases claires"), Location = new Point(8, 3), AutoSize = true },
                new KryptonLabel { Text = T("Cases sombres"), Location = new Point(8, 47), AutoSize = true },
                _actuelleClaire, _actuelleSombre, ok, annuler]);
            AjouteLigne(24, PastillesClaires, () => _claire, ChoisitClaire);
            AjouteLigne(68, PastillesSombres, () => _sombre, ChoisitSombre);
            AcceptButton = ok;
            CancelButton = annuler;
        }

        private void AjouteLigne(int y, string[] couleurs, Func<Color> actuelle, Action<Color> choisit)
        {   // Les pastilles d'un type de case, puis le bouton de la fenêtre de Windows
            for (int k = 0; k < couleurs.Length; k++)
            {
                Color couleur = ColorTranslator.FromHtml(couleurs[k]);
                Panel pastille = new()
                {
                    Location = new Point(44 + k * 23, y),
                    Size = new Size(21, 22),
                    BackColor = couleur,
                    BorderStyle = BorderStyle.FixedSingle,
                    Cursor = Cursors.Hand
                };
                pastille.Click += (s, e) => choisit(couleur);
                Controls.Add(pastille);
            }
            KryptonButton autre = new() { Text = T("Autre…"), Location = new Point(232, y - 2), Size = new Size(85, 26) };
            autre.Click += (s, e) =>
            {
                using ColorDialog dialogue = new() { Color = actuelle(), FullOpen = true };
                if (dialogue.ShowDialog(this) == DialogResult.OK)
                    choisit(dialogue.Color);
            };
            Controls.Add(autre);
        }

        private void ChoisitClaire(Color couleur)
        {
            _claire = couleur;
            _actuelleClaire.BackColor = couleur;
            _apercu(_claire, _sombre);
        }

        private void ChoisitSombre(Color couleur)
        {
            _sombre = couleur;
            _actuelleSombre.BackColor = couleur;
            _apercu(_claire, _sombre);
        }
    }
}
