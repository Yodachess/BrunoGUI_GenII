// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre "Saisie position" (bouton sous "Saisie partie") : placer les pièces à la main
// └─ Classe "SaisiePosition" : un échiquier dessiné, une palette de 12 pièces, le trait, les roques, la case en passant,
//                              le numéro du coup, la FEN (et "Coller une FEN") ; la position elle-même est dans EditeurPosition
//                              (testé). OK n'est accepté que si la position est correcte (ChargementPartie.ErreurFen) : le formulaire
//                              principal lit alors FenSaisie et la charge comme une FEN
// Clic gauche sur une case : y pose la pièce choisie (ou la retire si c'est déjà elle) ; clic droit : vide la case (pas de gomme :
// elle faisait double emploi, remarque de Bruno ; sa place sert aux messages).

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public class SaisiePosition : KryptonForm
    {
        private sealed class Zone : Panel
        {   // Panneau dessiné à la main, sans scintillement
            public Zone() { DoubleBuffered = true; ResizeRedraw = true; }
        }

        private const int TailleCase = 52, TaillePalette = 44;
        private readonly EditeurPosition _editeur;
        private readonly Func<TypePiece, Image> _image;
        private readonly Color _caseClaire, _caseSombre;
        private bool _coteNoir;
        private TypePiece _pieceChoisie = TypePiece.PionBlanc;
        private bool _miseAJour;                                   // contrôles remplis par le programme : leurs événements sont ignorés
        private string _message;                                   // message ponctuel (ex : pion refusé sur la 1re rangée)

        private readonly Zone _plateau = new() { Location = new Point(12, 12), Size = new Size(8 * TailleCase, 8 * TailleCase), Cursor = Cursors.Hand };
        private readonly List<(TypePiece Piece, Zone Case)> _palette = [];
        private readonly KryptonRadioButton _traitBlancs = new() { Text = "Blancs", Location = new Point(440, 200), Width = 80 };
        private readonly KryptonRadioButton _traitNoirs = new() { Text = "Noirs", Location = new Point(530, 200), Width = 80 };
        private readonly Dictionary<DroitRoque, KryptonCheckBox> _roques = new()
        {
            [DroitRoque.PetitBlanc] = new() { Text = "O-O blanc", Location = new Point(440, 248) },
            [DroitRoque.GrandBlanc] = new() { Text = "O-O-O blanc", Location = new Point(580, 248) },
            [DroitRoque.PetitNoir] = new() { Text = "O-O noir", Location = new Point(440, 272) },
            [DroitRoque.GrandNoir] = new() { Text = "O-O-O noir", Location = new Point(580, 272) }
        };
        private readonly KryptonComboBox _enPassant = new() { Location = new Point(560, 300), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly KryptonNumericUpDown _numero = new() { Location = new Point(560, 330), Width = 80, Minimum = 1, Maximum = 999 };
        private readonly KryptonTextBox _fen = new() { Location = new Point(56, 440), Width = 560, ReadOnly = true };
        // Messages (position correcte ou non, FEN collée...) : à droite, sous la palette, sur plusieurs lignes si besoin
        private readonly Label _etat = new()
        {
            Location = new Point(440, 132), AutoSize = true, MaximumSize = new Size(270, 48), Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            BackColor = Color.Transparent
        };

        public string FenSaisie => _editeur.Fen;   // la position saisie (lue après OK)

        public SaisiePosition(string fenDeDepart, Func<TypePiece, Image> image, Color caseClaire, Color caseSombre, bool coteNoir)
        {
            _editeur = new EditeurPosition(fenDeDepart);
            _image = image;
            _caseClaire = caseClaire;
            _caseSombre = caseSombre;
            _coteNoir = coteNoir;
            Text = "Saisie position";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(730, 512);

            _plateau.Paint += DessinePlateau;
            _plateau.MouseDown += ClicSurPlateau;
            Controls.Add(_plateau);
            CreePalette();
            Controls.Add(Libelle("Trait", 440, 182));
            Controls.AddRange([_traitBlancs, _traitNoirs]);
            _traitBlancs.CheckedChanged += (s, e) => ChangeTrait();
            Controls.Add(Libelle("Roques", 440, 228));
            foreach ((DroitRoque roque, KryptonCheckBox caseRoque) in _roques)
            {
                caseRoque.Width = 130;
                caseRoque.CheckedChanged += (s, e) =>
                {
                    if (_miseAJour) return;
                    _editeur.DefinitRoque(roque, caseRoque.Checked);
                    MetAJour();
                };
                Controls.Add(caseRoque);
            }
            Controls.Add(Libelle("En passant", 440, 302));
            _enPassant.SelectedIndexChanged += (s, e) =>
            {
                if (_miseAJour) return;
                _editeur.EnPassant = _enPassant.SelectedItem as string ?? "-";
                MetAJour();
            };
            Controls.Add(_enPassant);
            Controls.Add(Libelle("Coup n°", 440, 332));
            _numero.ValueChanged += (s, e) =>
            {
                if (_miseAJour) return;
                _editeur.NumeroCoup = (int)_numero.Value;
                MetAJour();
            };
            Controls.Add(_numero);
            Controls.Add(Bouton("Vider", 440, 364, 130, (s, e) => { _editeur.Vider(); MetAJour(); }));
            Controls.Add(Bouton("Position initiale", 580, 364, 130, (s, e) => { _editeur.PositionInitiale(); MetAJour(); }));
            Controls.Add(Bouton("Tourner", 440, 396, 130, (s, e) => { _coteNoir = !_coteNoir; _plateau.Invalidate(); }));
            Controls.Add(Bouton("Coller une FEN", 580, 396, 130, (s, e) => CollerFen()));
            Controls.Add(Libelle("FEN", 12, 442));
            Controls.Add(_fen);
            Controls.Add(Bouton("Copier", 624, 438, 86, (s, e) => Clipboard.SetText(_editeur.Fen)));
            Controls.Add(_etat);
            KryptonButton ok = Bouton("OK", 520, 474, 90, (s, e) => Valider());
            KryptonButton annuler = Bouton("Annuler", 620, 474, 90, null);
            annuler.DialogResult = DialogResult.Cancel;
            Controls.AddRange([ok, annuler]);
            AcceptButton = ok;
            CancelButton = annuler;
            MetAJour();
        }

        private static KryptonLabel Libelle(string texte, int x, int y) => new() { Text = texte, Location = new Point(x, y), AutoSize = true };

        private static KryptonButton Bouton(string texte, int x, int y, int largeur, EventHandler clic)
        {
            KryptonButton bouton = new() { Text = texte, Location = new Point(x, y), Size = new Size(largeur, 28) };
            if (clic != null)
                bouton.Click += clic;
            return bouton;
        }

        // ═══ Palette : 6 pièces blanches, 6 noires ═══
        private void CreePalette()
        {
            Controls.Add(Libelle("Pièce à poser (clic droit sur une case : la vider)", 440, 12));
            TypePiece[] blanches = [TypePiece.RoiBlanc, TypePiece.ReineBlanche, TypePiece.TourBlanche, TypePiece.FouBlanc, TypePiece.CavalierBlanc, TypePiece.PionBlanc];
            TypePiece[] noires = [TypePiece.RoiNoir, TypePiece.ReineNoire, TypePiece.TourNoire, TypePiece.FouNoir, TypePiece.CavalierNoir, TypePiece.PionNoir];
            for (int i = 0; i < 6; i++)
            {
                AjouteCasePalette(blanches[i], 440 + i * (TaillePalette + 2), 36);
                AjouteCasePalette(noires[i], 440 + i * (TaillePalette + 2), 36 + TaillePalette + 2);
            }
        }

        private void AjouteCasePalette(TypePiece piece, int x, int y)
        {
            Zone caseChoix = new() { Location = new Point(x, y), Size = new Size(TaillePalette, TaillePalette), Cursor = Cursors.Hand };
            caseChoix.Paint += (s, e) =>
            {
                bool choisie = piece == _pieceChoisie;
                e.Graphics.Clear(choisie ? Color.FromArgb(190, 212, 248) : Color.FromArgb(240, 240, 240));
                using (Pen bord = new(choisie ? Color.FromArgb(40, 100, 220) : Color.Silver, choisie ? 3 : 1))
                    e.Graphics.DrawRectangle(bord, 1, 1, TaillePalette - 3, TaillePalette - 3);
                if (_image(piece) is Image image)
                    e.Graphics.DrawImage(image, 3, 3, TaillePalette - 6, TaillePalette - 6);
            };
            caseChoix.MouseDown += (s, e) =>
            {
                _pieceChoisie = piece;
                foreach ((_, Zone autre) in _palette)
                    autre.Invalidate();
            };
            _palette.Add((piece, caseChoix));
            Controls.Add(caseChoix);
        }

        // ═══ Échiquier ═══
        private int IndexDe(int colonneEcran, int ligneEcran)
        {   // Index 120 de la case affichée à cette place (Blancs en bas, sauf vue côté Noirs)
            int colonne = _coteNoir ? 8 - colonneEcran : colonneEcran + 1;      // 1 = a
            int rangee = _coteNoir ? ligneEcran + 1 : 8 - ligneEcran;           // 1 à 8
            return (rangee + 1) * 10 + colonne;
        }

        private void DessinePlateau(object sender, PaintEventArgs e)
        {
            using Font police = new("Segoe UI", 7.5f, FontStyle.Bold);
            for (int ligne = 0; ligne < 8; ligne++)
                for (int colonne = 0; colonne < 8; colonne++)
                {
                    int index = IndexDe(colonne, ligne);
                    bool claire = (index / 10 - 1 + index % 10) % 2 == 1;      // a1 (rangée 1, colonne 1) est sombre
                    Rectangle zone = new(colonne * TailleCase, ligne * TailleCase, TailleCase, TailleCase);
                    using (SolidBrush fond = new(claire ? _caseClaire : _caseSombre))
                        e.Graphics.FillRectangle(fond, zone);
                    if (_editeur.Piece(index) is TypePiece piece && piece != TypePiece.Vide && _image(piece) is Image image)
                        e.Graphics.DrawImage(image, zone.X + 2, zone.Y + 2, TailleCase - 4, TailleCase - 4);
                    Color couleurTexte = claire ? _caseSombre : _caseClaire;
                    if (colonne == 0)       // chiffres des rangées, en haut à gauche de la 1re colonne
                        TextRenderer.DrawText(e.Graphics, (index / 10 - 1).ToString(), police, new Point(zone.X + 1, zone.Y + 1), couleurTexte);
                    if (ligne == 7)         // lettres des colonnes, en bas à droite de la dernière ligne
                        TextRenderer.DrawText(e.Graphics, ((char)('a' + index % 10 - 1)).ToString(), police,
                            new Rectangle(zone.X, zone.Y, TailleCase - 2, TailleCase - 1), couleurTexte, TextFormatFlags.Right | TextFormatFlags.Bottom);
                }
        }

        private void ClicSurPlateau(object sender, MouseEventArgs e)
        {
            int colonne = e.X / TailleCase, ligne = e.Y / TailleCase;
            if (colonne < 0 || colonne > 7 || ligne < 0 || ligne > 7)
                return;
            int index = IndexDe(colonne, ligne);
            _message = null;
            if (e.Button == MouseButtons.Right || _editeur.Piece(index) == _pieceChoisie)
                _editeur.Poser(index, TypePiece.Vide);      // clic droit, ou la même pièce déjà posée : la case est vidée
            else if (_pieceChoisie is TypePiece.PionBlanc or TypePiece.PionNoir && (index / 10 == 2 || index / 10 == 9))
                _message = "Un pion ne peut pas être sur la 1re ou la 8e rangée.";
            else
                _editeur.Poser(index, _pieceChoisie);
            MetAJour();
        }

        // ═══ Réglages, FEN, validation ═══
        private void ChangeTrait()
        {
            if (_miseAJour) return;
            _editeur.Trait = _traitBlancs.Checked ? ColorPiece.Blanc : ColorPiece.Noir;
            MetAJour();
        }

        private void CollerFen()
        {
            string texte = Clipboard.ContainsText() ? Clipboard.GetText().Trim() : "";
            string erreur = texte == "" ? "le presse-papiers ne contient pas de texte" : _editeur.ChargerFen(texte.Split('\n')[0]);
            _message = erreur != null ? $"FEN refusée : {erreur}." : "FEN collée.";
            MetAJour();
        }

        private void MetAJour()
        {   // Tous les contrôles d'après la position (sans déclencher leurs événements)
            _miseAJour = true;
            _traitBlancs.Checked = _editeur.Trait == ColorPiece.Blanc;
            _traitNoirs.Checked = _editeur.Trait == ColorPiece.Noir;
            foreach ((DroitRoque roque, KryptonCheckBox caseRoque) in _roques)
            {
                caseRoque.Enabled = _editeur.RoquePossible(roque);
                caseRoque.Checked = _editeur.Roque(roque);
            }
            _enPassant.Items.Clear();
            _enPassant.Items.Add("-");
            foreach (string caseEnPassant in _editeur.CasesEnPassantPossibles())
                _enPassant.Items.Add(caseEnPassant);
            _enPassant.SelectedItem = _editeur.EnPassant;
            _enPassant.Enabled = _enPassant.Items.Count > 1;
            _numero.Value = Math.Clamp(_editeur.NumeroCoup, (int)_numero.Minimum, (int)_numero.Maximum);
            _fen.Text = _editeur.Fen;
            string erreur = _editeur.Erreur;
            _etat.Text = _message ?? (erreur == null ? "Position correcte : OK pour la jouer." : $"Position incorrecte : {erreur}.");
            _etat.ForeColor = _message == null && erreur == null ? Color.DarkGreen : Color.Firebrick;
            _miseAJour = false;
            _plateau.Invalidate();
        }

        private void Valider()
        {   // OK : seulement si la position peut être jouée (sinon la raison s'affiche et la fenêtre reste ouverte)
            string erreur = _editeur.Erreur;
            if (erreur != null)
            {
                _message = $"Impossible de jouer cette position : {erreur}.";
                MetAJour();
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
