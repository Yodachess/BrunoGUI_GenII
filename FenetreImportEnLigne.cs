// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre « Importer mes parties en ligne » (menu Fichier) ...
//      └─ Classe "FenetreImportEnLigne" (KryptonForm construite en code, boîte de dialogue)
// Le site (Lichess ou Chess.com), le pseudo (un par site, mémorisé dans les préférences) et le nombre de parties ; le
// téléchargement lui-même est fait par le formulaire principal (ImportEnLigne)

using System;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.Langue;

namespace BrunoGUI_GenII
{
    public class FenetreImportEnLigne : KryptonForm
    {
        private readonly KryptonRadioButton _lichess = new() { Text = "Lichess", Location = new Point(110, 20), AutoSize = true };
        private readonly KryptonRadioButton _chessCom = new() { Text = "Chess.com", Location = new Point(220, 20), AutoSize = true };
        private readonly KryptonTextBox _pseudo = new() { Location = new Point(110, 56), Width = 220 };
        private readonly KryptonNumericUpDown _nombre = new()
        { Location = new Point(110, 94), Width = 70, Minimum = ImportEnLigne.MinimumParties, Maximum = ImportEnLigne.MaximumParties };
        private readonly KryptonLabel _message = new() { Location = new Point(12, 130), AutoSize = true, StateCommon = { ShortText = { Color1 = Color.Firebrick } } };
        private string _pseudoLichess, _pseudoChessCom;

        public SiteEnLigne SiteChoisi => _chessCom.Checked ? SiteEnLigne.ChessCom : SiteEnLigne.Lichess;
        public string Pseudo => _pseudo.Text.Trim();
        public string PseudoLichess => SiteChoisi == SiteEnLigne.Lichess ? Pseudo : _pseudoLichess;
        public string PseudoChessCom => SiteChoisi == SiteEnLigne.ChessCom ? Pseudo : _pseudoChessCom;
        public int Nombre => (int)_nombre.Value;

        public FenetreImportEnLigne(SiteEnLigne site, string pseudoLichess, string pseudoChessCom, int nombre)
        {
            Text = T("Importer mes parties en ligne");
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(460, 230);
            _pseudoLichess = pseudoLichess;
            _pseudoChessCom = pseudoChessCom;
            (site == SiteEnLigne.ChessCom ? _chessCom : _lichess).Checked = true;
            _pseudo.Text = site == SiteEnLigne.ChessCom ? pseudoChessCom : pseudoLichess;
            _nombre.Value = Math.Clamp(nombre, ImportEnLigne.MinimumParties, ImportEnLigne.MaximumParties);
            _lichess.CheckedChanged += (s, e) => ChangeDeSite();
            KryptonButton importer = new() { Text = T("Importer"), Location = new Point(240, 190), Size = new Size(100, 28) };
            KryptonButton annuler = new() { Text = T("Annuler"), Location = new Point(350, 190), Size = new Size(100, 28), DialogResult = DialogResult.Cancel };
            importer.Click += (s, e) =>
            {   // (pseudo refusé : la fenêtre reste ouverte et dit pourquoi)
                if (ImportEnLigne.PseudoValable(Pseudo))
                {
                    DialogResult = DialogResult.OK;
                    return;
                }
                _message.Text = T("Pseudo incorrect : de 2 à 30 lettres, chiffres, « _ » ou « - ».");
                _pseudo.Focus();
            };
            Controls.AddRange([
                Libelle(T("Site :"), 22), _lichess, _chessCom,
                Libelle(T("Pseudo :"), 58), _pseudo,
                Libelle(T("Parties :"), 96), _nombre,
                Libelle(T("Les plus récentes d'abord ; seul votre pseudo est envoyé au site."), 158),
                _message, importer, annuler]);
            AcceptButton = importer;
            CancelButton = annuler;
        }

        private void ChangeDeSite()
        {   // Chaque site garde son pseudo
            if (SiteChoisi == SiteEnLigne.Lichess)
            {
                _pseudoChessCom = _pseudo.Text.Trim();
                _pseudo.Text = _pseudoLichess;
            }
            else
            {
                _pseudoLichess = _pseudo.Text.Trim();
                _pseudo.Text = _pseudoChessCom;
            }
            _message.Text = "";
        }

        private static KryptonLabel Libelle(string texte, int y) => new() { Text = texte, Location = new Point(12, y), AutoSize = true };
    }
}
