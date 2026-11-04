// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// « Et si j'avais joué un autre coup ? » : clic droit sur un coup de la feuille
//      └─ Classe partielle "EchiquierPrincipal"
// L'échiquier montre la position d'avant ce coup (parcours) et accepte une suite de coups des deux camps, jouée sur une copie
// (EssaiDeCoup). Après chaque coup, l'analyse de position part d'elle-même (AnalysePosition_Click) ; le coup suivant l'interrompt, et
// sa fin ne montre pas de boîte de message, qui serait à fermer à chaque coup. La partie n'est jamais touchée :
// tout déplacement dans la partie (Fin, Préc, Suiv, clic sur la feuille) met fin à l'essai. La suite s'écrit au fur et à mesure dans la
// feuille, en commentaire du coup remplacé (CommentaireDeLaFeuille, rien dans le coup) ; elle n'y reste que si le joueur le confirme à la fin

using System;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.Langue;

namespace BrunoGUI_GenII
{
    public partial class EchiquierPrincipal
    {
        private EssaiDeCoup? _essai;
        private int _indexCoupEssaye;       // place dans ListeCoups du coup remplacé par l'essai
        // (pas pendant l'analyse de la partie, ni pendant que le moteur cherche son coup, que l'essai abandonnerait sans le redemander)
        private bool EssaiPossible => _analyseDePartie == null && _pilote.Demande != TypeDemande.CoupDePartie;

        private void CommenceEssai(int index)
        {   // Montre la position d'avant le coup n° index de ListeCoups et attend le coup à essayer à sa place
            Coup coup = LogiqueMouvements.ListeCoups[index];
            AfficheCoupDeLaPartie(index - 1);
            if (_positionAffichee is not Position position)
                return;
            _essai = new EssaiDeCoup(position, coup);
            _indexCoupEssaye = index;
            _coupARejouer = null;       // (la meilleure suite du coup précédent ne se rejoue pas pendant l'essai)
            VarianteMoteurUci3.Cursor = Cursors.Default;
            _clickCaseSource = true;
            _vue.EffaceFleches();
            MiseaZeroVariantes();
            string joue = Notation(coup.PgnFrNumerote.Trim());
            AfficheLigneCentree(VarianteMoteurUci1, T("Et si vous aviez joué autre chose que {0} ? Jouez votre coup sur l'échiquier.", joue));
            InformationsPartie.Text = T("Essai : jouez la suite, Fin pour revenir");
            MetAJourCommandes();
        }
        private void TermineEssai(bool proposeDeGarder = true)
        {   // L'échiquier sera redessiné par le déplacement qui met fin à l'essai. proposeDeGarder : false si la partie va être remplacée
            AnnuleSelectionPiece();
            if (_essai is not EssaiDeCoup essai)
                return;
            bool garder = false;
            if (proposeDeGarder && essai.AGarder)
            {   // (moteur arrêté d'abord : sa réponse pendant la question ouvrirait la boîte de message de fin d'analyse)
                AbandonneReflexion();
                garder = KryptonMessageBox.Show(T("Garder cette suite en commentaire de {0} ?\n\n{1}", Notation(essai.CoupDeLaPartie.PgnFrNumerote.Trim()), essai.TexteCommentaire()),
                    T("Et si j'avais joué un autre coup ?"), KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question) == DialogResult.Yes;
            }
            _essai = null;
            if (garder)     // (le texte de la feuille : les phrases de l'analyse y restent, un commentaire du joueur étant affiché seul)
                essai.CoupDeLaPartie.Commentaire = Annotations.NettoieCommentaire(essai.CommentaireAvec(FeuilleDePartie.CommentaireEssentiel(essai.CoupDeLaPartie)));
            MetAJourFeuille();
        }
        private string? CommentaireDeLaFeuille(Coup coup)
        {   // Le commentaire écrit sous un coup, avec la suite en cours d'essai sous le coup qu'elle remplace
            string? commentaire = FeuilleDePartie.CommentaireEssentiel(coup);
            return _essai is { AGarder: true } essai && essai.CoupDeLaPartie == coup ? essai.CommentaireAvec(commentaire) : commentaire;
        }
        private void CaseCliqueeEssai(int indexCase)
        {   // Même geste que pour un coup de la partie : un clic prend la pièce, un autre la pose
            if (_essai == null)
                return;
            string nomCase = LogiqueMouvements.NomCaseAlgebrique(indexCase);
            if (_clickCaseSource)
            {
                if (!_essai.PieceJouable(nomCase) || _vue.ImageCase(indexCase) is not Image imagePiece)
                    return;
                _indexSource120 = indexCase;
                _caseSource = nomCase;
                _pieceSource = _essai.Courante.Pieces[indexCase];
                _vue.MetCurseurPiece(imagePiece);
                _vue.DessinePiece(indexCase, LogiqueMouvements.TypePiece.Vide);
                LogiqueMouvements.DessineMouvements(_caseSource, true, _essai.Courante);
                _clickCaseSource = false;
                return;
            }
            _clickCaseSource = true;
            _vue.LibereCurseurPiece();
            LogiqueMouvements.EffaceSymboles(false);    // (dessins ignorés pendant le parcours : la position est redessinée ici)
            LogiqueMouvements.TypePiece promotion = LogiqueMouvements.TypePiece.Vide;
            if (Regles.EstLegal(_essai.Courante, _caseSource, nomCase) && Regles.EstPromotion(_essai.Courante, _indexSource120, indexCase))
                promotion = AffichePromotionPion(_essai.Courante.QuiJoue);
            if (!_essai.Jouer(_caseSource, nomCase, promotion))
            {   // coup illégal : la pièce revient, et on peut en essayer un autre
                _vue.DessinePosition(_essai.Courante);
                return;
            }
            _positionAffichee = _essai.Courante;
            _vue.DessinePosition(_essai.Courante);
            AfficheCoupsBibliotheque(Regles.Fen(_essai.Courante));
            MetAJourCommandes();
            AnalysePosition_Click(null, EventArgs.Empty);
        }
        private void AfficheResultatEssai()
        {   // Fin de l'analyse d'une position de l'essai : la suite essayée et ce que la partie a donné (3e ligne de variante, la 1re montre la meilleure suite)
            if (_essai == null || _essai.NombreCoups == 0)
                return;
            AfficheLigneCentree(VarianteMoteurUci3, T("Et si {0} ?", Notation(_essai.TexteSuite())) + " " + _essai.TexteComparaison());
            InformationsPartie.Text = T("Essai : jouez la suite, Fin pour revenir");
            MetAJourFeuille();      // (l'évaluation s'ajoute à la suite écrite dans la feuille)
        }
    }
}
