// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// EchiquierPrincipal (fichier partiel) : Parcours de la partie : boutons et clavier, position affichée, analyse du coup regardé, flèches, lignes de variantes

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.GestionPartiePgn;
using static BrunoGUI_GenII.LogiqueMouvements;
using static BrunoGUI_GenII.Parametres;

namespace BrunoGUI_GenII
{
    public partial class EchiquierPrincipal
    {
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des boutons et flèches pour parcours de partie
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void BoutonPrecedent_Click(object sender, EventArgs e)
        {   // On recule d'un demi-coup dans l'affichage (la partie n'est pas modifiée)
            if (LogiqueMouvements.ListeCoups.Count == 0)
            {
                KryptonMessageBox.Show("Aucun coup à afficher.", "Info", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            int indexActuel = ParcoursEnCours ? _indexAffiche : LogiqueMouvements.ListeCoups.Count - 1;
            if (indexActuel <= IndexPremierePosition)
                KryptonMessageBox.Show("Vous êtes au début de la partie.", "Début de partie", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            else
                AfficheCoupDeLaPartie(indexActuel - 1);
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {   // On avance d'un demi-coup dans l'affichage ; après le dernier coup, on revient à la position courante de la partie
            if (ParcoursEnCours)
                AfficheCoupDeLaPartie(_indexAffiche + 1);
            else if (LogiqueMouvements.EchecetMat)
                KryptonMessageBox.Show("Il y a échec et mat.", "Terminé : échec et mat", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            else
                KryptonMessageBox.Show("Vous êtes à la fin de la partie.", "Fin de partie", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {   // On affiche la position initiale de la partie (ou la position FEN de départ)
            if (LogiqueMouvements.ListeCoups.Count > 0)
                AfficheCoupDeLaPartie(IndexPremierePosition);
        }
        private void BoutonFin_Click(object sender, EventArgs e)
        {   // On revient à la position courante de la partie
            RetourPositionCourante();
        }
        private void BoutonReprendreIci_Click(object sender, EventArgs e)
        {   // "Reprendre la partie d'ici" : la partie est coupée à la position affichée (parcours) et reprend depuis cette position.
            // Une partie PGN en lecture seule peut aussi être reprise à sa position finale (rien n'est supprimé)
            bool etaitLectureSeule = PartieEnLectureSeule;
            if (!ParcoursEnCours && !etaitLectureSeule)
                return;     // sécurité : voir MetAJourCommandes
            int index = ParcoursEnCours ? _indexAffiche : LogiqueMouvements.ListeCoups.Count - 1;
            int aSupprimer = LogiqueMouvements.ListeCoups.Count - 1 - index;
            Position positionReprise = _positionAffichee ?? LogiqueMouvements.PositionActuelle;
            if (LogiqueMouvements.CalculerSur(positionReprise, () => !LogiqueMouvements.ResteCoupsValidesJouables()))
            {   // mat ou pat : il n'y a rien à jouer depuis cette position
                KryptonMessageBox.Show("Cette position est terminée (mat ou pat) : choisissez une position antérieure avec Préc.",
                    "Reprendre la partie d'ici", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            string suppression = aSupprimer > 0 ? $"\n\nLes {aSupprimer} demi-coup(s) suivant(s) seront supprimés." : "";
            string message = etaitLectureSeule
                ? $"La partie chargée devient votre partie à partir de la position affichée :\nvous jouez le camp au trait, {_nomMoteur} l'autre camp.{suppression}\n\nContinuer ?"
                : $"La partie reprend à la position affichée.{suppression}\n\nContinuer ?";
            if (KryptonMessageBox.Show(message, "Reprendre la partie d'ici", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Question) != DialogResult.OK)
                return;
            AbandonneReflexion();   // la partie change
            bool etaitTerminee = _partie.Mode == ModePartie.Terminee;
            QuitteParcours();
            _vue.EffaceDernierCoup();
            if (_partie.ReprendreDepuis(index) == 0 && !etaitLectureSeule)
            {   // rien à supprimer : l'échiquier revient simplement à la partie
                LogiqueMouvements.DessinPieces();
                MetAJourCommandes();
                return;
            }
            if (etaitTerminee || etaitLectureSeule)
                EffaceResultat();       // la partie n'a plus de résultat
            if (etaitLectureSeule)
            {   // La partie chargée devient une partie d'entraînement contre le moteur (l'humain a le camp au trait)
                AfficheJoueursDeLaPartie();
                PartieEnCours.Tournoi = "Entrainement";
                PartieEnCours.Lieu = "Maison";
                PartieEnCours.Date = DateTime.Today.ToString("yyyy.MM.dd");
                PartieEnCours.Ronde = "";
            }
            _pauseJoueur = false;       // reprendre la partie met fin à une pause
            if (etaitLectureSeule)
                NouvellePendule();      // partie d'entraînement : la cadence choisie, qui part au prochain coup
            else
                _pendule?.RestaurerDepuis(LogiqueMouvements.ListeCoups, QuiJoue);   // temps notés après le coup où l'on reprend
            AffichePendules();
            PartieEnCours.CompteDePLy = LogiqueMouvements.ListeCoupsFen.Count.ToString();
            string fen = LogiqueMouvements.RetourneChaineFenActuel();
            AfficheCoupsBibliotheque(fen);
            InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + NomCamp(QuiJoue);
            InformationsPartie.Text = "Partie reprise";
            if (_partie.MoteurAuTrait)
            {   // C'est au moteur de jouer à partir de cette position
                PlateauEnable(false);
                MetAJourCommandes();
                JeuMoteurAvecBibliotheque(fen);
            }
            else
            {
                PlateauEnable(true);
                MetAJourCommandes();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {   // Gestion des flèches pour parcourir la partie
            /*
            ProcessCmdKey est une méthode spéciale de la classe Form (et aussi de Control).
            Elle est appelée avant que Windows envoie la touche à un contrôle précis (bouton, textbox, etc.).
            🧠 En clair :
            Quand tu appuies sur une touche :
            Windows la détecte,
            Avant de la donner à un contrôle, WinForms appelle Form.ProcessCmdKey(...),
            Si tu retournes true, tu dis :
            → “je l’ai gérée, inutile de la transmettre ailleurs”,
            Si tu retournes base.ProcessCmdKey(...),
            → “je ne m’en occupe pas, laisse WinForms la gérer normalement”.
            */
            if (!_clavierActif)
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Right: BoutonSuivant_Click(null, EventArgs.Empty); return true;
                case Keys.Left: BoutonPrecedent_Click(null, EventArgs.Empty); return true;
                case Keys.Home:
                case Keys.PageUp: BoutonDebut_Click(null, EventArgs.Empty); return true;
                case Keys.End:
                case Keys.PageDown: BoutonFin_Click(null, EventArgs.Empty); return true;
                case Keys.Escape:
                    this.ActiveControl = null;
                    _clavierActif = false; // désactive clavier
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ═══ Position affichée (parcours de la partie) ═══
        // L'échiquier montre soit la partie (LogiqueMouvements.PositionActuelle), soit une position passée (_positionAffichee).
        // Parcourir ne modifie jamais la partie : le moteur peut jouer pendant le parcours, et l'humain rejoue après être revenu
        // à la position courante (clic sur l'échiquier ou "Fin").
        private Position _positionAffichee;     // null : l'échiquier montre la partie
        private int _indexAffiche;              // index dans ListeCoups de la position affichée (-1 : position initiale FenDepart)
        private bool ParcoursEnCours => _positionAffichee != null;
        private static int IndexPremierePosition =>     // -1 : position initiale ; 0 : partie commencée depuis un FEN
            LogiqueMouvements.ListeCoups.Count > 0 && LogiqueMouvements.ListeCoups[0].EstPositionDeDepart ? 0 : -1;
        private Position PositionDesVariantes =>        // position sur laquelle sont convertis les coups du moteur (variantes, conseil)
            _pilote.AnalyseEnCours ? _pilote.PositionAnalysee : LogiqueMouvements.PositionActuelle;

        public void AfficheCoupDeLaPartie(int index)
        {   // Affiche la position après le coup n° index de ListeCoups (-1 : position initiale), sans modifier la partie.
            // Au-delà du dernier coup : retour à la position courante
            int dernier = LogiqueMouvements.ListeCoups.Count - 1;
            if (index >= dernier)
            {
                if (ParcoursEnCours)
                    RetourPositionCourante();
                else
                    AfficheDernierCoupSiPartieFinie();      // déjà à la position courante : clic sur le dernier coup de la feuille
                return;
            }
            index = Math.Max(index, IndexPremierePosition);
            if (_pilote.AnalyseEnCours && _analyseDePartie == null)
                AbandonneReflexion();       // l'analyse portait sur la position affichée jusqu'ici (la réflexion du moteur pour son coup continue ;
                                            // l'analyse de la partie aussi : on peut parcourir la partie pendant qu'elle avance)
            string fen = index < 0 ? FenDepart : LogiqueMouvements.ListeCoups[index].Fen;
            _vue.DernierCoupMasque = true;  // le dernier coup de la partie n'a pas de sens sur une position passée
            Position position = LogiqueMouvements.PositionDepuisFen(fen);   // (variable locale : la suite ne dépend pas du champ)
            _positionAffichee = position;
            _indexAffiche = index;
            MetAJourCommandes();
            _vue.DessinePosition(position);
            AfficheCoupsBibliotheque(fen);
            InformationPourJoueur.Text = "Trait aux " + NomCamp(position.QuiJoue);
            MiseaZeroParcours();
            AfficheTextesDuCoup(index, position);
            AffichePendules();      // partie sans pendule en cours (ex : PGN chargé) : temps notés à cette position
            if (!PartieEnLectureSeule)
                InformationsPartie.Text = "Parcours : Fin ou clic pour revenir";
        }
        private void AfficheTextesDuCoup(int index, Position positionApres)
        {   // Le coup regardé (1re ligne de variante et dernière case de la barre d'état) et, s'il a été analysé, son analyse
            // (lignes 2 et 3) et ses flèches. Pendant le parcours, et pour le dernier coup d'une partie finie (AfficheDernierCoupSiPartieFinie)
            bool positionInitiale = index < 0 || LogiqueMouvements.ListeCoups[index].EstPositionDeDepart;
            AfficheLigneCentree(VarianteMoteurUci1, positionInitiale
                ? "[ Position initiale ]" : $"[ {TexteCoupJoue(index, positionApres)} ]"
                  + (LogiqueMouvements.ListeCoups[index].TempsReflexion is TimeSpan reflexion ? $"   (réflexion : {TexteDuree(reflexion)})" : ""));
            VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = "...";
            VarianteMoteurCourante.Text = positionInitiale ? "Position initiale" : TexteCoupJoue(index, positionApres);
            if (!positionInitiale)
            {   // Coup analysé (analyse de partie, ou [%eval] et variante du PGN chargé) : son évaluation et le meilleur coup sur la
                // 2e ligne, la meilleure suite sur la 3e. Commentaire du PGN : sur une ligne restée libre (2e, puis 3e), jamais à la
                // suite du coup regardé (choix de Bruno) ; il est aussi, en entier, dans l'infobulle des trois lignes
                Coup coup = LogiqueMouvements.ListeCoups[index];
                string commentaire = string.IsNullOrWhiteSpace(coup.Commentaire) ? null : $"« {coup.Commentaire} »";
                foreach (RichTextBox ligne in new[] { VarianteMoteurUci1, VarianteMoteurUci2, VarianteMoteurUci3 })
                    _infobulleBilan.SetToolTip(ligne, commentaire != null ? "Commentaire : " + commentaire : null);
                if (coup.EvaluationApres != null || coup.MeilleurCoup != null)
                {   // Le texte le plus complet qui tient sur la ligne : sinon sans le nom de l'annotation, puis sans le symbole (-+)
                    int largeur = VarianteMoteurUci2.ClientSize.Width - 14;
                    string texte = new[] { TexteAnalyseDuCoup(coup, true, true), TexteAnalyseDuCoup(coup, false, true), TexteAnalyseDuCoup(coup, false, false) }
                        .FirstOrDefault(t => TextRenderer.MeasureText(t, _policesLignes[VarianteMoteurUci2].Grasse).Width <= largeur)
                        ?? TexteAnalyseDuCoup(coup, false, false);
                    AfficheLigneCentree(VarianteMoteurUci2, texte);
                }
                else if (commentaire != null)
                {
                    AfficheLigneCentree(VarianteMoteurUci2, commentaire);
                    commentaire = null;
                }
                if (!string.IsNullOrEmpty(coup.VarianteMeilleure))
                {   // ligne 3 : la suite prévue ; "meilleure" seulement si l'écart avec le coup joué compte (sinon c'est juste une
                    // autre possibilité, remarque de Bruno)
                    string libelle = coup.MeilleurJoue ? "Suite prévue : "
                        : coup.PerteAnalyse < JugementCoups.SeuilImprecision ? "Suite du moteur : " : "Meilleure suite : ";
                    AfficheLigneCentree(VarianteMoteurUci3, libelle + coup.VarianteMeilleure);
                }
                else if (commentaire != null)
                    AfficheLigneCentree(VarianteMoteurUci3, commentaire);
            }
            else
                foreach (RichTextBox ligne in new[] { VarianteMoteurUci1, VarianteMoteurUci2, VarianteMoteurUci3 })
                    _infobulleBilan.SetToolTip(ligne, null);
            MontreFlechesDuCoup(index);
        }
        private void AfficheDernierCoupSiPartieFinie()
        {   // Retour à la position courante : le dernier coup d'une partie qui ne continue plus (PGN chargé, partie terminée) est montré
            // comme pendant le parcours (son analyse, ses flèches) ; sinon les flèches sont effacées (elles resteraient après le coup suivant)
            int dernier = LogiqueMouvements.ListeCoups.Count - 1;
            if ((PartieEnLectureSeule || _partie.Mode == ModePartie.Terminee) && _analyseDePartie == null     // (pendant l'analyse : ses lignes)
                && dernier >= 0 && !LogiqueMouvements.ListeCoups[dernier].EstPositionDeDepart)
                AfficheTextesDuCoup(dernier, LogiqueMouvements.PositionActuelle);
            else
            {
                _vue.EffaceFleches();
                foreach (RichTextBox ligne in new[] { VarianteMoteurUci1, VarianteMoteurUci2, VarianteMoteurUci3 })
                    _infobulleBilan.SetToolTip(ligne, null);    // (commentaire du coup regardé pendant le parcours)
            }
        }
        // Flèches d'un coup analysé : le meilleur coup du moteur en vert, comme le coup joué s'il était ce meilleur coup
        private static readonly Color CouleurFlecheMeilleurCoup = Color.FromArgb(21, 120, 27);
        private void MontreFlechesDuCoup(int index)
        {   // Flèche du coup joué, sur tout coup regardé (elle montre ce qui a été joué) : de la couleur de son annotation s'il en a une,
            // verte sinon (choix de Bruno : une seule couleur hors annotations). Coup annoté (analysé) : aussi la flèche verte du meilleur coup du
            // moteur s'il en est un autre (choix de Bruno : pas sur un coup sans annotation, où l'écart est négligeable).
            // Les deux partent de la position d'avant le coup (l'échiquier montre celle d'après) ; rien pendant l'analyse elle-même
            _vue.EffaceFleches();
            if (_analyseDePartie != null || index < 0 || index >= LogiqueMouvements.ListeCoups.Count)
                return;
            Coup coup = LogiqueMouvements.ListeCoups[index];
            string joue = coup.Uci.Trim();
            if (coup.EstPositionDeDepart || joue.Length < 4)
                return;
            List<(int, int, Color)> fleches = [];
            Color couleurJoue = coup.Annotation != "" ? FeuilleCoups.CouleurAnnotation(coup.Annotation) : CouleurFlecheMeilleurCoup;
            fleches.Add((RenvoieCaseIndex120(joue[..2]), RenvoieCaseIndex120(joue[2..4]), couleurJoue));
            // (meilleur coup : de l'analyse de partie, ou 1re variante du PGN chargé)
            if (coup.Annotation != "" && !coup.MeilleurJoue && coup.MeilleurCoupUci is { Length: >= 4 } meilleur)
                fleches.Add((RenvoieCaseIndex120(meilleur[..2]), RenvoieCaseIndex120(meilleur[2..4]), CouleurFlecheMeilleurCoup));
            if (fleches.TrueForAll(f => f.Item1 > 0 && f.Item2 > 0))
                _vue.MontreFleches([.. fleches]);
        }
        private static string TexteAnalyseDuCoup(Coup coup, bool nomAnnotation, bool symbole)
        {   // Ex : "-9.05 (-+)  joué Dh2 ?? [Gaffe]  — meilleur : Rd3 (0.00)" : l'évaluation après le coup, le coup joué et son jugement
            // ("?? [Gaffe]", choix de Bruno), puis le meilleur coup avec SON évaluation. Si le coup joué ne perd presque rien (ex : mat
            // en 3 au lieu de 2), le meilleur n'est qu'une préférence : "— moteur : Rd3 (0.00), écart négligeable" (Bruno trouvait
            // "meilleur" inadapté). Coups en notation longue, avec la case de départ : "Dd8-d7", "Ta8-c8". Court pour tenir sur la
            // ligne en police 10 (sans "Analyse :", deux espaces) ; nomAnnotation et symbole à false raccourcissent encore.
            // Sans évaluation (PGN chargé sans [%eval]) : "Joué Fc8-e6 ?? [Gaffe]  — meilleur : Fc8-b7"
            string texte = (coup.EvaluationApres is Evaluation evaluation ? evaluation.Texte + (symbole ? $" ({evaluation.Symbole})" : "") + "  joué " : "Joué ")
                + (coup.CoupJoueLong ?? coup.PgnFrSansNumero)
                + (coup.Annotation != "" ? " " + coup.Annotation + (nomAnnotation ? $" [{Annotations.Nom(coup.Annotation)}]" : "") : "");
            if (coup.MeilleurJoue)
                return texte + "  — meilleur coup du moteur";
            if (coup.MeilleurCoup == null)
                return texte;
            string meilleur = (coup.MeilleurCoupLong ?? coup.MeilleurCoup) + (coup.EvaluationMeilleur is Evaluation e ? $" ({e.Texte})" : "");
            return texte + (coup.PerteAnalyse < JugementCoups.SeuilImprecision
                ? $"  — moteur : {meilleur}, écart négligeable" : $"  — meilleur : {meilleur}");
        }

        // Les lignes de variante 1 et 2 servent aussi aux variantes du moteur (à gauche, police normale) : pendant le parcours, le coup
        // regardé (ligne 1) et son analyse (ligne 2) y sont centrés et en gras (AfficheLigneCentree) ; tout autre texte écrit ensuite
        // remet la ligne en forme normale (LigneVariante_TextChanged)
        private bool _ecritureCentree;
        private readonly Dictionary<RichTextBox, (Font Normale, Font Grasse)> _policesLignes = [];

        [StructLayout(LayoutKind.Sequential)]
        private struct Rectangle32 { public int Gauche, Haut, Droite, Bas; }
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr fenetre, int message, IntPtr wParam, ref Rectangle32 zone);
        private const int EM_SETRECT = 0xB3;
        private static void MargesLigne(RichTextBox ligne)
        {   // Zone du texte d'une ligne de variante : 6 px à gauche (jamais collé au bord), centrée en hauteur
            int hauteurTexte = TextRenderer.MeasureText("Ag", ligne.Font).Height;
            Rectangle32 zone = new()
            {
                Gauche = 6, Haut = Math.Max(0, (ligne.ClientSize.Height - hauteurTexte) / 2),
                Droite = Math.Max(7, ligne.ClientSize.Width - 4), Bas = ligne.ClientSize.Height
            };
            SendMessage(ligne.Handle, EM_SETRECT, IntPtr.Zero, ref zone);
        }
        private void AfficheLigneCentree(RichTextBox ligne, string texte)
        {   // Texte centré en gras, toujours dans la police de la ligne (Bruno ne veut pas de police réduite)
            _ecritureCentree = true;
            ligne.Text = texte;
            ligne.SelectAll();
            ligne.SelectionAlignment = HorizontalAlignment.Center;
            ligne.SelectionFont = _policesLignes[ligne].Grasse;
            ligne.DeselectAll();
            _ecritureCentree = false;
        }
        private void LigneVariante_TextChanged(object sender, EventArgs e)
        {   // Un autre texte que le coup regardé ou son analyse : alignement à gauche et police normale
            if (_ecritureCentree || sender is not RichTextBox ligne)
                return;
            ligne.SelectAll();
            ligne.SelectionAlignment = HorizontalAlignment.Left;
            ligne.SelectionFont = _policesLignes[ligne].Normale;
            ligne.DeselectAll();
        }
        private static string TexteDuree(TimeSpan duree) =>
            // Temps de réflexion d'un coup ([%emt]) : "13 s", "30 min 11 s", "1 h 05 min"
            duree.TotalHours >= 1 ? $"{(int)duree.TotalHours} h {duree.Minutes:00} min"
            : duree.TotalMinutes >= 1 ? $"{duree.Minutes} min {duree.Seconds:00} s"
            : $"{duree.Seconds} s";
        private static string TexteCoupJoue(int index, Position positionApres)
        {   // Ex : "Coup blanc : 12. Cf3" ou "Coup noir : 12... Fe7" (coup n° index, qui a mené à positionApres)
            string coup = LogiqueMouvements.ListeCoupsNal[index];
            coup = coup.Contains('.') ? coup.Split('.').Last().Trim() : coup.Trim();
            int numero = (int)Math.Floor(positionApres.NombreCoupsJoues - 0.5f);   // numéro du coup qui vient d'être joué
            return positionApres.QuiJoue == ColorPiece.Blanc ? $"Coup noir : {numero}... {coup}" : $"Coup blanc : {numero}. {coup}";
        }
        public void RetourPositionCourante()
        {   // Fin du parcours : l'échiquier montre de nouveau la partie
            if (!ParcoursEnCours)
                return;
            _positionAffichee = null;
            MetAJourCommandes();
            LogiqueMouvements.DessinPieces();
            _vue.DernierCoupMasque = false;     // le dernier coup du moteur est de nouveau coloré
            AfficheDernierCoupSiPartieFinie();
            AfficheCoupsBibliotheque(LogiqueMouvements.RetourneChaineFenActuel());
            InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + NomCamp(QuiJoue);
            InformationsPartie.Text = PartieEnLectureSeule ? "Fin de la partie" : "";
            AffichePendules();
        }
        private void QuitteParcours()
        {   // La partie va être remplacée (nouvelle partie, chargement, "Reprendre ici") : l'échiquier suivra la partie
            _positionAffichee = null;
            _vue.DernierCoupMasque = false;
            _vue.EffaceFleches();
            EffaceBilan();              // le bilan de l'analyse portait sur l'ancienne partie
            MetAJourCommandes();
        }
        private void DessinePieceDeLaPartie(int IndexCase, LogiqueMouvements.TypePiece Piece)
        {   // Dessins demandés par la partie (coups joués...) : ignorés pendant le parcours, l'échiquier est redessiné au retour
            if (!ParcoursEnCours)
                _vue.DessinePiece(IndexCase, Piece);
        }
    }
}
