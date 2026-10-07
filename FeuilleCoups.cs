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
//              ├─ "CoupCliqueDroit" clic droit sur un coup (annotations)
//              └─ "DessineBande"   courbe d'évaluation verticale à droite des coups, alignée sur les lignes
// La mise en page (quel coup sur quelle ligne) vient de FeuilleDePartie.Lignes (Coup.cs, testée) ; ici, seulement le dessin
// et la souris. Le composant ne connaît ni la partie ni le parcours : le formulaire lui dit quoi montrer.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        // Couleurs de la feuille : fond blanc, une ligne sur deux gris-bleu très pâle, coup affiché en bleu (comme ChessBase ou
        // Lichess ; le jaune pâle "papier" d'avant n'a pas plu à Bruno)
        private static readonly Color CouleurPapier = Color.White;
        private static readonly Color CouleurLignePaire = Color.FromArgb(243, 246, 250);
        private static readonly Color CouleurSelection = Color.FromArgb(190, 212, 248);
        private static readonly Color CouleurSurvol = Color.FromArgb(228, 235, 245);
        private static readonly Color CouleurNumero = Color.FromArgb(120, 125, 135);
        private static readonly Color CouleurBordure = Color.FromArgb(175, 182, 195);

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
        private int LargeurCoup => Math.Max(10, (ClientSize.Width - LargeurNumero - 2 - LargeurBande) / 2);

        // ═══ Courbe d'évaluation : bande verticale à droite des coups, alignée sur les lignes (elle défile avec la feuille) ═══
        // À la façon de ChessBase (choix de Bruno) : une barre par demi-coup analysé, à la hauteur de son coup (coup blanc : moitié
        // haute de la ligne, coup noir : moitié basse), partant de la ligne centrale (0.00) ; VERTE vers la droite quand les Blancs
        // sont mieux, ROUGE vers la gauche quand ce sont les Noirs, longueur en pions jusqu'à ±4 (au-delà : barre entière),
        // JAUNE entière pour un mat. Repères discrets à ±1 et ±2 pions ; petit carré de couleur au bout d'une barre annotée
        public const int LargeurBande = 36;
        private const double PionsMaximum = 4;
        private static readonly Color CouleurFondBande = Color.FromArgb(246, 247, 249);
        private static readonly Color CouleurBlancsMieux = Color.FromArgb(46, 160, 67);
        private static readonly Color CouleurNoirsMieux = Color.FromArgb(214, 48, 49);
        private static readonly Color CouleurMat = Color.FromArgb(240, 200, 0);
        private static readonly Color CouleurLigneCentrale = Color.FromArgb(90, 95, 105);
        private static readonly Color CouleurRepere = Color.FromArgb(222, 225, 232);
        private Rectangle ZoneBande => new(ClientSize.Width - LargeurBande - 1, 1, LargeurBande, ClientSize.Height - 2);

        private float YDuDemiCoup(int ligne, bool noir) =>
            1 + ligne * HauteurLigne + AutoScrollPosition.Y + HauteurLigne * (noir ? 0.5f : 0f);

        private float DemiLargeurBande => LargeurBande / 2f - 2;
        private float LongueurPions(double pions) => (float)(Math.Clamp(pions, -PionsMaximum, PionsMaximum) / PionsMaximum) * DemiLargeurBande;

        private IEnumerable<(int Index, int Ligne, bool Noir, Evaluation Evaluation)> DemiCoupsAnalyses()
        {   // Les demi-coups analysés, dans l'ordre de la partie, avec leur ligne
            for (int ligne = 0; ligne < _lignes.Count; ligne++)
                foreach ((int? index, bool noir) in new[] { (_lignes[ligne].Blanc, false), (_lignes[ligne].Noir, true) })
                    if (index is int i && _coups[i].EvaluationApres is Evaluation evaluation)
                        yield return (i, ligne, noir, evaluation);
        }

        private void DessineBande(Graphics g)
        {
            Rectangle bande = ZoneBande;
            using (SolidBrush fond = new(CouleurFondBande))
                g.FillRectangle(fond, bande);
            Region decoupageAvant = g.Clip;
            g.SetClip(bande);
            float centre = bande.X + bande.Width / 2f;
            using (Pen repere = new(CouleurRepere))
                foreach (double pions in new[] { -2.0, -1.0, 1.0, 2.0 })
                    g.DrawLine(repere, centre + LongueurPions(pions), bande.Y, centre + LongueurPions(pions), bande.Bottom);
            float demiHauteur = HauteurLigne / 2f;
            foreach ((int index, int ligne, bool noir, Evaluation evaluation) in DemiCoupsAnalyses())
            {
                float y = YDuDemiCoup(ligne, noir);
                if (y > bande.Bottom || y + demiHauteur < bande.Y)
                    continue;
                float longueur;
                Color couleur;
                if (evaluation.MatEn is int mat)
                {
                    longueur = (mat >= 0 ? 1 : -1) * DemiLargeurBande;
                    couleur = CouleurMat;
                }
                else
                {
                    longueur = LongueurPions((evaluation.Centipions ?? 0) / 100.0);
                    couleur = longueur >= 0 ? CouleurBlancsMieux : CouleurNoirsMieux;
                }
                RectangleF barre = new(Math.Min(centre, centre + longueur), y + 1, Math.Max(1, Math.Abs(longueur)), demiHauteur - 2);
                using (SolidBrush pinceau = new(couleur))
                    g.FillRectangle(pinceau, barre);
                if (_coups[index].Annotation != "")
                {   // Coup annoté : petit carré de sa couleur au bout de la barre, cerclé de blanc
                    float bout = longueur >= 0 ? barre.Right : barre.Left;
                    RectangleF marque = new(Math.Clamp(bout - 3, bande.X + 1, bande.Right - 7), y + demiHauteur / 2 - 3, 6, 6);
                    using SolidBrush pinceau = new(CouleurAnnotation(_coups[index].Annotation));
                    using Pen bord = new(Color.White);
                    g.FillRectangle(pinceau, marque);
                    g.DrawRectangle(bord, marque.X, marque.Y, marque.Width, marque.Height);
                }
            }
            using (Pen ligneCentrale = new(CouleurLigneCentrale))
                g.DrawLine(ligneCentrale, centre, bande.Y, centre, bande.Bottom);
            // Le demi-coup affiché : cadre bleu autour de sa barre, sur toute la largeur
            int ligneSelection = LigneDuCoup(_indexSelectionne);
            if (ligneSelection >= 0)
            {
                float y = YDuDemiCoup(ligneSelection, _lignes[ligneSelection].Noir == _indexSelectionne);
                using Pen cadre = new(Color.FromArgb(40, 100, 220), 2);
                g.DrawRectangle(cadre, bande.X + 1, y, bande.Width - 2, demiHauteur);
            }
            g.Clip = decoupageAvant;
        }
        private int DemiCoupDansLaBande(Point point)
        {   // Demi-coup à cette hauteur de la bande (moitié haute de la ligne : coup blanc, basse : coup noir), -1 s'il n'y en a pas
            if (!ZoneBande.Contains(point))
                return -1;
            int y = point.Y - AutoScrollPosition.Y - 1;
            int ligne = y / HauteurLigne;
            if (ligne < 0 || ligne >= _lignes.Count)
                return -1;
            bool noir = y % HauteurLigne >= HauteurLigne / 2;
            return (noir ? _lignes[ligne].Noir : _lignes[ligne].Blanc) ?? -1;
        }

        private readonly ToolTip _infobulleBande = new() { InitialDelay = 200, ReshowDelay = 100 };
        private int _demiCoupInfobulle = -2;
        private void MetAJourInfobulleBande(Point point)
        {   // Survol de la bande : le coup et son évaluation ("25... c5??  2.73"), ou une explication s'il n'est pas analysé
            int index = DemiCoupDansLaBande(point);
            if (!ZoneBande.Contains(point))
                index = -2;
            if (index == _demiCoupInfobulle)
                return;
            _demiCoupInfobulle = index;
            string texte = index == -2 ? null
                : index < 0 || _coups[index].EvaluationApres is not Evaluation evaluation ? "Courbe d'évaluation : analyser la partie pour la voir"
                : $"{_coups[index].PgnFrNumerote}{_coups[index].Annotation}   {evaluation.Texte}";
            _infobulleBande.SetToolTip(this, texte);
        }

        private string _resultat;               // résultat écrit sous le dernier coup ("1-0", "0-1", "½-½"), null : partie en cours

        public void MetAJour(IReadOnlyList<Coup> coups, int indexSelectionne, string resultat = null)
        {   // Les coups de la partie, le coup surligné (celui de la position affichée ; -1 : aucun, ex : position de départ),
            // et le résultat de la partie ("1-0", "0-1", "1/2-1/2" ; vide ou "*" : partie en cours, rien n'est écrit)
            _coups = [.. coups];
            _lignes = FeuilleDePartie.Lignes(_coups);
            _resultat = resultat is "1-0" or "0-1" ? resultat : resultat == "1/2-1/2" ? "½-½" : null;
            _indexSelectionne = indexSelectionne;
            _indexSurvole = -1;
            AutoScrollMinSize = new Size(0, NombreLignes * HauteurLigne + 2);
            RendVisible(indexSelectionne);
            Invalidate();
        }

        private int NombreLignes => _lignes.Count + (_resultat != null ? 1 : 0);     // avec la ligne du résultat

        private int LigneDuCoup(int index) => _lignes.FindIndex(l => l.Blanc == index || l.Noir == index);

        private void RendVisible(int index)
        {   // Fait défiler la feuille pour que le coup soit visible (le dernier coup joué reste en vue, avec le résultat en dessous)
            int ligne = LigneDuCoup(index);
            if (ligne < 0)
                return;
            int lignesVisibles = ligne == _lignes.Count - 1 && _resultat != null ? 2 : 1;
            int haut = ligne * HauteurLigne, decalage = -AutoScrollPosition.Y;
            if (haut < decalage)
                AutoScrollPosition = new Point(0, haut);
            else if (haut + lignesVisibles * HauteurLigne > decalage + ClientSize.Height)
                AutoScrollPosition = new Point(0, haut + lignesVisibles * HauteurLigne - ClientSize.Height + 2);
        }

        private int CoupSous(Point point)
        {   // Place dans ListeCoups du coup sous ce point du composant, -1 s'il n'y en a pas (numéro, case vide, sous la dernière ligne)
            if (point.X >= ZoneBande.X)
                return DemiCoupDansLaBande(point);      // courbe d'évaluation : le demi-coup à cette hauteur
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
            if (_resultat != null)
            {   // Le résultat, centré et en gras sous le dernier coup
                int y = 1 + _lignes.Count * hauteur + AutoScrollPosition.Y;
                _policeResultat ??= new Font(Font, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, _resultat, _policeResultat, new Rectangle(0, y, ClientSize.Width, hauteur), ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
            DessineBande(e.Graphics);
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
        private Font _policeResultat;       // police du résultat de la partie (gras)

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
        {   // !! turquoise, ! vert, !? violet, ?! jaune ocre, ? orange, ?? rouge (? et ?? bien distincts : l'orange foncé
            // d'avant se confondait avec le rouge)
            "!!" => Color.FromArgb(0, 150, 136),
            "!" => Color.FromArgb(34, 139, 34),
            "!?" => Color.FromArgb(120, 80, 200),
            "?!" => Color.FromArgb(200, 150, 0),
            "?" => Color.FromArgb(240, 110, 0),
            "??" => Color.FromArgb(200, 0, 0),
            _ => Color.Black
        };

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MetAJourInfobulleBande(e.Location);
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
            _demiCoupInfobulle = -2;
            _infobulleBande.SetToolTip(this, null);
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
