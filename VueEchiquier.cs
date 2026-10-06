// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Affichage de l'échiquier (les 120 cases PictureBox), sans aucune règle du jeu ni état de partie
//  └─ Classe "VueEchiquier"
//              ├─ "CreerCases"             crée les 120 cases (64 visibles) sur le plateau
//              ├─ "IndexDeLaCase"          index 120 de la case cliquée (en tenant compte de l'inversion)
//              ├─ "DessinePiece", "DessineSymbole", "DessinePosition"
//              ├─ "MontreDernierCoup", "EffaceDernierCoup", "DernierCoupMasque"   cases colorées du dernier coup du moteur
//              ├─ "MontreFleches", "EffaceFleches"   flèches de couleur (analyse : coup joué, meilleur coup)
//              ├─ "Tourner"                vue côté Blancs / côté Noirs
//              ├─ "ChangeCouleurs"         couleurs des cases claires et sombres
//              ├─ "ActiverCases"           cases cliquables ou non
//              └─ "MetCurseurPiece", "LibereCurseurPiece"   la pièce suit la souris pendant un déplacement
// Le formulaire décide QUOI montrer (partie ou position passée, tour du joueur...) ; cette classe sait seulement COMMENT.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public class VueEchiquier
    {
        private readonly PictureBox _plateau;           // l'image de l'échiquier, sur laquelle sont posées les cases
        private readonly Control _proprietaireCurseur;   // la fenêtre dont le curseur devient la pièce déplacée
        private readonly List<PictureBox> _cases = [];   // les 120 cases, indexées comme le tableau "mailbox" (a1 = 21, h8 = 98)
        private readonly Dictionary<TypePiece, Bitmap> _images = new()
        {
            [TypePiece.PionBlanc] = new(Properties.Resources.PionBlanc),
            [TypePiece.TourBlanche] = new(Properties.Resources.TourBlanche),
            [TypePiece.CavalierBlanc] = new(Properties.Resources.CavalierBlanc),
            [TypePiece.FouBlanc] = new(Properties.Resources.FouBlanc),
            [TypePiece.ReineBlanche] = new(Properties.Resources.ReineBlanche),
            [TypePiece.RoiBlanc] = new(Properties.Resources.RoiBlanc),
            [TypePiece.PionNoir] = new(Properties.Resources.PionNoir),
            [TypePiece.TourNoire] = new(Properties.Resources.TourNoire),
            [TypePiece.CavalierNoir] = new(Properties.Resources.CavalierNoir),
            [TypePiece.FouNoir] = new(Properties.Resources.FouNoir),
            [TypePiece.ReineNoire] = new(Properties.Resources.ReineNoire),
            [TypePiece.RoiNoir] = new(Properties.Resources.RoiNoir),
            [TypePiece.Vide] = null
        };
        private readonly Dictionary<TypeSymbole, Bitmap> _symboles = new()
        {
            [TypeSymbole.SymboleMenacePiece] = new(Properties.Resources.Menace),               // menace pour la pièce sélectionnée
            [TypeSymbole.SymboleMouvementSansPrise] = new(Properties.Resources.SansPrise),     // mouvement autorisé sans prise
            [TypeSymbole.SymboleMouvementInterdit] = new(Properties.Resources.Interdit),       // mouvement interdit
            [TypeSymbole.SymboleMouvementAvecPrise] = new(Properties.Resources.AvecPrise)      // mouvement autorisé avec prise
        };

        public Color CaseClaire { get; private set; }
        public Color CaseSombre { get; private set; }
        public Color CaseSource { get; }            // couleurs des cases de départ et d'arrivée du dernier coup du moteur
        public Color CaseDestination { get; }
        public bool CoteNoir { get; private set; }  // true quand les Noirs sont en bas de l'écran

        public VueEchiquier(PictureBox plateau, Control proprietaireCurseur, Color claire, Color sombre, Color source, Color destination)
        {
            _plateau = plateau;
            _proprietaireCurseur = proprietaireCurseur;
            CaseClaire = claire;
            CaseSombre = sombre;
            CaseSource = source;
            CaseDestination = destination;
        }

        public Image ImagePiece(TypePiece piece) => _images[piece];
        public Image ImageCase(int index) => _cases[index].Image;

        public void CreerCases(MouseEventHandler clicSurCase)
        {   // Les 120 cases (bordures comprises, seules 64 sont visibles), nommées "Pictjeux" + index : a1 = Pictjeux21, h8 = Pictjeux98
            for (int ligne = 0; ligne <= 11; ligne++)
                for (int colonne = 0; colonne <= 9; colonne++)
                {
                    int index = ligne * 10 + colonne;
                    PictureBox caseJeu = new()
                    {
                        Name = "Pictjeux" + index,
                        BackColor = CouleurNormale(index),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Size = new Size(60, 60),    // Taille des case = 60 * 60 pixels
                        Location = new Point(-39 + (colonne * 60), 560 - (ligne * 60)),
                        Visible = ligne > 1 & ligne < 10 & colonne > 0 & colonne < 9,   // seules les 64 cases utiles sont visibles
                        Enabled = false
                    };
                    caseJeu.BringToFront();
                    caseJeu.MouseDown += clicSurCase;
                    caseJeu.Paint += DessineFlechesSurLaCase;
                    _plateau.Controls.Add(caseJeu);
                    _cases.Add(caseJeu);
                }
        }

        public int IndexDeLaCase(PictureBox caseCliquee)
        {   // Index 120 de la case cliquée : son nom donne sa place à l'écran, qu'il faut retourner si les Noirs sont en bas
            int numero = Convert.ToInt32(caseCliquee.Name[8..]);
            return CoteNoir ? 119 - numero : numero;
        }

        private Color CouleurNormale(int index) => Outils.EstCaseClaire(index) ? CaseClaire : CaseSombre;

        // ═══ Pièces et symboles ═══
        public void DessinePiece(int index, TypePiece piece)
        {   // Dessine une pièce (ou vide la case) ; peut être appelé depuis un autre thread
            try
            {
                if (_plateau.InvokeRequired)
                {
                    _plateau.Invoke(new MethodInvoker(() => DessinePiece(index, piece)));
                    return;
                }
                RemplaceImage(index, _images[piece]);       // (libère l'éventuelle image pièce + symbole)
                Application.DoEvents();
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
            {   // fenêtre en cours de fermeture : rien à dessiner
                System.Diagnostics.Debug.WriteLine("DessinePiece : " + ex.Message);
            }
        }
        public void DessinePosition(Position position)
        {   // Dessine les 64 cases d'une position
            for (int ligne = 2; ligne <= 9; ligne++)
                for (int colonne = 1; colonne <= 8; colonne++)
                    DessinePiece(ligne * 10 + colonne, position.Pieces[ligne * 10 + colonne]);
        }
        public void DessineSymbole(int index, TypeSymbole symbole)
        {   // Dessine un des 4 symboles : seul sur une case vide, par-dessus la pièce sinon
            if (_cases[index].Image == null)
                RemplaceImage(index, _symboles[symbole]);
            else
            {   // Image composée (pièce + symbole), libérée quand elle est remplacée
                Bitmap composee = new(_cases[index].Image);
                using (Graphics g = Graphics.FromImage(composee))
                    g.DrawImage(_symboles[symbole], 0, 0, 100, 100);
                _imagesComposees.Add(composee);
                RemplaceImage(index, composee);
            }
        }
        // Images composées (pièce + symbole) : à libérer quand une case change d'image
        // (les images des pièces et des symboles, partagées par toutes les cases, ne sont jamais libérées)
        private readonly HashSet<Image> _imagesComposees = [];
        private void RemplaceImage(int index, Image nouvelle)
        {
            Image ancienne = _cases[index].Image;
            _cases[index].Image = nouvelle;
            if (ancienne != null && ancienne != nouvelle && _imagesComposees.Remove(ancienne))
                ancienne.Dispose();
        }

        // ═══ Cases colorées du dernier coup du moteur ═══
        private int _source, _destination;
        private bool _dernierCoupMontre;    // un coup est à montrer
        private bool _dernierCoupMasque;    // ... mais pas maintenant (parcours : une position passée est affichée)
        public void MontreDernierCoup(int source, int destination)
        {
            EffaceDernierCoup();            // le coup précédent (ex : le moteur a joué deux fois de suite)
            _source = source;
            _destination = destination;
            _dernierCoupMontre = true;
            Colore();
        }
        public void EffaceDernierCoup()
        {
            if (_dernierCoupMontre)
                CouleursNormales();
            _dernierCoupMontre = false;
        }
        public bool DernierCoupMasque
        {   // true pendant le parcours : le dernier coup de la partie n'a pas de sens sur une position passée
            get => _dernierCoupMasque;
            set
            {
                _dernierCoupMasque = value;
                if (value) { if (_dernierCoupMontre) CouleursNormales(); }
                else Colore();
            }
        }
        private void Colore()
        {
            if (!_dernierCoupMontre || _dernierCoupMasque)
                return;
            _cases[_source].BackColor = CaseSource;
            _cases[_destination].BackColor = CaseDestination;
        }
        private void CouleursNormales()
        {
            _cases[_source].BackColor = CouleurNormale(_source);
            _cases[_destination].BackColor = CouleurNormale(_destination);
        }

        // ═══ Flèches (analyse de partie : coup joué, meilleur coup) ═══
        // Les cases sont des contrôles posés sur le plateau : aucune ne peut dessiner par-dessus ses voisines. Chaque case dessine
        // donc, après sa pièce, la partie des flèches qui la traverse (les flèches sont en coordonnées du plateau, la case les
        // décale de sa propre position, et le dessin hors de la case est coupé). L'inversion de l'échiquier est prise en compte
        // sans rien faire : la position à l'écran d'une case est lue au moment du dessin.
        private readonly List<(int Source, int Destination, Color Couleur)> _fleches = [];
        public void MontreFleches(params (int Source, int Destination, Color Couleur)[] fleches)
        {
            _fleches.Clear();
            _fleches.AddRange(fleches);
            RedessineCases();
        }
        public void EffaceFleches()
        {
            if (_fleches.Count == 0)
                return;
            _fleches.Clear();
            RedessineCases();
        }
        private void RedessineCases()
        {
            foreach (PictureBox caseJeu in _cases)
                if (caseJeu.Visible)
                    caseJeu.Invalidate();
        }
        private void DessineFlechesSurLaCase(object sender, PaintEventArgs e)
        {
            if (_fleches.Count == 0)
                return;
            PictureBox caseJeu = (PictureBox)sender;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TranslateTransform(-caseJeu.Left, -caseJeu.Top);
            foreach (var (source, destination, couleur) in _fleches)
            {
                using System.Drawing.Drawing2D.GraphicsPath fleche = Fleche(Centre(source), Centre(destination));
                using SolidBrush pinceau = new(Color.FromArgb(170, couleur));     // un peu transparente : la pièce reste visible
                e.Graphics.FillPath(pinceau, fleche);
            }
        }
        private PointF Centre(int index) => new(_cases[index].Left + _cases[index].Width / 2f, _cases[index].Top + _cases[index].Height / 2f);
        private static System.Drawing.Drawing2D.GraphicsPath Fleche(PointF depart, PointF arrivee)
        {   // Flèche pleine (corps + pointe) du centre de la case de départ vers celui de la case d'arrivée, construite
            // horizontalement puis tournée dans la bonne direction
            const float demiLargeur = 6, demiLargeurPointe = 15, longueurPointe = 22, retraitArrivee = 8;
            float dx = arrivee.X - depart.X, dy = arrivee.Y - depart.Y;
            float longueur = MathF.Sqrt(dx * dx + dy * dy) - retraitArrivee;
            float corps = Math.Max(0, longueur - longueurPointe);
            System.Drawing.Drawing2D.GraphicsPath chemin = new();
            chemin.AddPolygon(new PointF[]
            {
                new(0, -demiLargeur), new(corps, -demiLargeur), new(corps, -demiLargeurPointe), new(longueur, 0),
                new(corps, demiLargeurPointe), new(corps, demiLargeur), new(0, demiLargeur)
            });
            using System.Drawing.Drawing2D.Matrix rotation = new();
            rotation.Translate(depart.X, depart.Y);
            rotation.Rotate(MathF.Atan2(dy, dx) * 180 / MathF.PI);
            chemin.Transform(rotation);
            return chemin;
        }

        // ═══ Vue, couleurs, activation ═══
        public void Tourner(Position aDessiner)
        {   // Rotation de 180° : la liste des cases est inversée (les couleurs du dernier coup, posées sur les cases physiques,
            // sont retirées avant et remises après), puis la position est redessinée
            if (_dernierCoupMontre)
                CouleursNormales();
            _cases.Reverse();
            _plateau.Image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            _plateau.Refresh();
            CoteNoir = !CoteNoir;
            DessinePosition(aDessiner);
            Colore();
        }
        public void ChangeCouleurs(Color claire, Color sombre)
        {   // Nouvelles couleurs des cases claires et sombres (le dernier coup reste coloré)
            CaseClaire = claire;
            CaseSombre = sombre;
            for (int index = 0; index < _cases.Count; index++)
                _cases[index].BackColor = CouleurNormale(index);
            Colore();
        }
        public void ActiverCases(bool actives)
        {
            foreach (PictureBox caseJeu in _cases)
                if (caseJeu.Visible)
                    caseJeu.Enabled = actives;
        }
        public bool CasesCreees => _cases.Count == 120;

        // ═══ Curseur "pièce" pendant un déplacement : l'icône Windows et le curseur sont libérés quand on en change ═══
        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr icone);
        private Cursor _curseurPiece;
        private IntPtr _iconeCurseurPiece;
        public void MetCurseurPiece(Image piece)
        {
            LibereCurseurPiece();
            using Bitmap copie = new(piece);
            using Bitmap vignette = (Bitmap)copie.GetThumbnailImage(88, 88, null, IntPtr.Zero);
            _iconeCurseurPiece = vignette.GetHicon();
            _curseurPiece = new Cursor(_iconeCurseurPiece);
            _proprietaireCurseur.Cursor = _curseurPiece;
        }
        public void LibereCurseurPiece()
        {
            _proprietaireCurseur.Cursor = Cursors.Default;
            _curseurPiece?.Dispose();
            _curseurPiece = null;
            if (_iconeCurseurPiece != IntPtr.Zero)
                DestroyIcon(_iconeCurseurPiece);
            _iconeCurseurPiece = IntPtr.Zero;
        }
    }
}
