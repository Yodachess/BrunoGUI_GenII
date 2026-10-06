// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Feuille de partie, à droite de l'échiquier (remplace la fenêtre "Liste des coups")
//  └─ Classe "FeuilleCoups" : composant dessiné à la main, une ligne "n° | coup blanc | coup noir" par coup complet
//              ├─ "MetAJour"       les coups de la partie et le coup surligné (celui de la position affichée)
//              ├─ "CoupClique"     clic gauche sur un coup : sa place dans ListeCoups
//              └─ "CoupCliqueDroit" clic droit sur un coup (annotations)
// La mise en page (quel coup sur quelle ligne) vient de FeuilleDePartie.Lignes (Coup.cs, testée) ; ici, seulement le dessin
// et la souris. Le composant ne connaît ni la partie ni le parcours : le formulaire lui dit quoi montrer.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public class FeuilleCoups : ScrollableControl
    {
        private List<Coup> _coups = [];
        private List<LigneFeuille> _lignes = [];
        private int _indexSelectionne = -1;     // coup surligné (place dans ListeCoups), -1 : aucun
        private int _indexSurvole = -1;         // coup sous la souris

        public event Action<int> CoupClique;
        public event Action<int, Point> CoupCliqueDroit;    // place du coup, position de la souris (dans le composant)

        // Couleurs d'une feuille de partie papier (jaune pâle, comme le double d'une feuille autocopiante)
        private static readonly Color CouleurPapier = Color.FromArgb(255, 250, 222);
        private static readonly Color CouleurLignePaire = Color.FromArgb(248, 240, 202);
        private static readonly Color CouleurSelection = Color.FromArgb(180, 205, 250);
        private static readonly Color CouleurSurvol = Color.FromArgb(232, 226, 190);
        private static readonly Color CouleurNumero = Color.FromArgb(140, 120, 80);
        private static readonly Color CouleurBordure = Color.FromArgb(190, 175, 130);

        public FeuilleCoups()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            AutoScroll = true;
            BackColor = CouleurPapier;
            SetStyle(ControlStyles.Selectable, true);   // peut prendre le focus (molette de la souris)
            TabStop = false;
        }

        private int HauteurLigne => Font.Height + 8;
        private int LargeurNumero => TextRenderer.MeasureText("888.", Font).Width + 4;
        private int LargeurCoup => Math.Max(10, (ClientSize.Width - LargeurNumero - 2) / 2);

        public void MetAJour(IReadOnlyList<Coup> coups, int indexSelectionne)
        {   // Les coups de la partie, et le coup surligné (celui de la position affichée ; -1 : aucun, ex : position de départ)
            _coups = [.. coups];
            _lignes = FeuilleDePartie.Lignes(_coups);
            _indexSelectionne = indexSelectionne;
            _indexSurvole = -1;
            AutoScrollMinSize = new Size(0, _lignes.Count * HauteurLigne + 2);
            RendVisible(indexSelectionne);
            Invalidate();
        }

        private int LigneDuCoup(int index) => _lignes.FindIndex(l => l.Blanc == index || l.Noir == index);

        private void RendVisible(int index)
        {   // Fait défiler la feuille pour que le coup soit visible (le dernier coup joué reste en vue)
            int ligne = LigneDuCoup(index);
            if (ligne < 0)
                return;
            int haut = ligne * HauteurLigne, decalage = -AutoScrollPosition.Y;
            if (haut < decalage)
                AutoScrollPosition = new Point(0, haut);
            else if (haut + HauteurLigne > decalage + ClientSize.Height)
                AutoScrollPosition = new Point(0, haut + HauteurLigne - ClientSize.Height + 2);
        }

        private int CoupSous(Point point)
        {   // Place dans ListeCoups du coup sous ce point du composant, -1 s'il n'y en a pas (numéro, case vide, sous la dernière ligne)
            int ligne = (point.Y - AutoScrollPosition.Y - 1) / HauteurLigne;
            if (point.Y < 1 || ligne < 0 || ligne >= _lignes.Count || point.X < LargeurNumero)
                return -1;
            int? index = point.X < LargeurNumero + LargeurCoup ? _lignes[ligne].Blanc : _lignes[ligne].Noir;
            return index ?? -1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int hauteur = HauteurLigne, largeurNumero = LargeurNumero, largeurCoup = LargeurCoup;
            int premiere = Math.Max(0, (-AutoScrollPosition.Y) / hauteur);
            for (int ligne = premiere; ligne < _lignes.Count; ligne++)
            {
                int y = 1 + ligne * hauteur + AutoScrollPosition.Y;
                if (y > ClientSize.Height)
                    break;
                if (ligne % 2 == 1)
                    using (SolidBrush fond = new(CouleurLignePaire))
                        e.Graphics.FillRectangle(fond, 1, y, ClientSize.Width - 2, hauteur);
                LigneFeuille l = _lignes[ligne];
                TextRenderer.DrawText(e.Graphics, l.Numero + ".", Font, new Rectangle(2, y, largeurNumero - 4, hauteur), CouleurNumero,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
                DessineCase(e.Graphics, l.Blanc, l.Noir != null, new Rectangle(largeurNumero, y, largeurCoup, hauteur));
                DessineCase(e.Graphics, l.Noir, false, new Rectangle(largeurNumero + largeurCoup, y, largeurCoup, hauteur));
            }
            using Pen bordure = new(CouleurBordure);
            e.Graphics.DrawRectangle(bordure, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        private void DessineCase(Graphics g, int? index, bool pointsSiVide, Rectangle zone)
        {   // Un coup (surligné s'il est affiché, éclairé sous la souris), ou "..." pour la case blanche d'une ligne commencée par un coup noir
            if (index is not int i)
            {
                if (pointsSiVide)
                    TextRenderer.DrawText(g, "...", Font, zone, CouleurNumero, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                return;
            }
            if (i == _indexSelectionne || i == _indexSurvole)
                using (SolidBrush fond = new(i == _indexSelectionne ? CouleurSelection : CouleurSurvol))
                    g.FillRectangle(fond, zone.X, zone.Y + 1, zone.Width - 2, zone.Height - 2);
            Rectangle texte = new(zone.X + 4, zone.Y, zone.Width - 6, zone.Height);
            TextFormatFlags format = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix
                                   | TextFormatFlags.NoPadding;
            string coup = _coups[i].PgnFrSansNumero;
            TextRenderer.DrawText(g, coup, Font, texte, ForeColor, format | TextFormatFlags.EndEllipsis);
            string annotation = _coups[i].Annotation;
            if (annotation != "")
            {   // L'annotation suit le coup (collée, comme dans le PGN et les autres logiciels), sur une pastille de sa couleur,
                // en blanc et gras pour être bien visible ; proposée par l'analyse de partie : pastille adoucie, tant que le joueur
                // ne l'a pas choisie
                int largeurCoup = TextRenderer.MeasureText(g, coup, Font, texte.Size, format).Width;
                _policeAnnotation ??= new Font(Font.FontFamily, Font.Size * 0.9f, FontStyle.Bold);
                Size taille = TextRenderer.MeasureText(g, annotation, _policeAnnotation, texte.Size, format);
                Rectangle pastille = new(texte.X + largeurCoup + 2, zone.Y + (zone.Height - taille.Height - 2) / 2, taille.Width + 6, taille.Height + 2);
                Color couleur = CouleurAnnotation(annotation);
                if (_coups[i].AnnotationProposee)
                    couleur = Color.FromArgb((couleur.R + CouleurPapier.R) / 2, (couleur.G + CouleurPapier.G) / 2, (couleur.B + CouleurPapier.B) / 2);
                DessinePastille(g, pastille, couleur);
                TextRenderer.DrawText(g, annotation, _policeAnnotation, pastille, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        private Font _policeAnnotation;     // police des pastilles d'annotation (créée une seule fois)

        private static void DessinePastille(Graphics g, Rectangle zone, Color couleur)
        {   // Rectangle aux coins arrondis, plein
            int rayon = Math.Min(zone.Height, 8);
            using System.Drawing.Drawing2D.GraphicsPath chemin = new();
            chemin.AddArc(zone.X, zone.Y, rayon, rayon, 180, 90);
            chemin.AddArc(zone.Right - rayon, zone.Y, rayon, rayon, 270, 90);
            chemin.AddArc(zone.Right - rayon, zone.Bottom - rayon, rayon, rayon, 0, 90);
            chemin.AddArc(zone.X, zone.Bottom - rayon, rayon, rayon, 90, 90);
            chemin.CloseFigure();
            var lissage = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (SolidBrush pinceau = new(couleur))
                g.FillPath(pinceau, chemin);
            g.SmoothingMode = lissage;
        }

        public static Color CouleurAnnotation(string annotation) => annotation switch
        {   // !! turquoise, ! vert, !? violet, ?! orange, ? orange foncé, ?? rouge
            "!!" => Color.FromArgb(0, 150, 136),
            "!" => Color.FromArgb(34, 139, 34),
            "!?" => Color.FromArgb(120, 80, 200),
            "?!" => Color.FromArgb(225, 135, 0),
            "?" => Color.FromArgb(215, 75, 30),
            "??" => Color.FromArgb(200, 0, 0),
            _ => Color.Black
        };

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int survole = CoupSous(e.Location);
            Cursor = survole >= 0 ? Cursors.Hand : Cursors.Default;
            if (survole != _indexSurvole)
            {
                _indexSurvole = survole;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_indexSurvole >= 0)
            {
                _indexSurvole = -1;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();        // pour la molette de la souris
            int index = CoupSous(e.Location);
            if (index < 0)
                return;
            if (e.Button == MouseButtons.Left)
                CoupClique?.Invoke(index);
            else if (e.Button == MouseButtons.Right)
                CoupCliqueDroit?.Invoke(index, e.Location);
        }
    }
}
