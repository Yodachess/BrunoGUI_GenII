// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// « Et si j'avais joué un autre coup ? » (sans Windows Forms, testé)
//      └─ Classe "EssaiDeCoup"
// Une suite de coups essayée à la place d'un coup de la partie (les coups des deux camps) : jouée sur une copie de la position,
// la partie n'est jamais touchée. L'affichage et l'analyse sont dans EchiquierPrincipal.Essai.cs

using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public class EssaiDeCoup(Position depart, Coup coupDeLaPartie)
    {
        public Position Courante { get; } = depart.Copier();    // d'abord la position d'avant le coup de la partie, puis après chaque coup essayé
        public Coup CoupDeLaPartie { get; } = coupDeLaPartie;
        private readonly List<Coup> _coups = [];
        public int NombreCoups => _coups.Count;
        public Evaluation? EvaluationCourante { get; set; }     // celle du moteur à la position Courante, null tant qu'il n'a pas répondu

        public bool PieceJouable(string nomCase) => Regles.MouvementsValides(Courante, nomCase).Count > 0;

        public bool Jouer(string source, string destination, TypePiece promotion = TypePiece.Vide)
        {   // false : coup illégal, rien n'est joué
            if (!Regles.EstLegal(Courante, source, destination))
                return false;
            _coups.Add(JoueEtNote(Courante, source, destination, promotion).Coup);
            EvaluationCourante = null;
            return true;
        }

        public string TexteSuite()
        {   // "14. Fc4 Cf6 15. O-O", ou "14... Cf6 15. O-O" si la suite commence par un coup noir
            if (_coups.Count == 0)
                return "";
            return string.Join(" ", _coups.Select((coup, i) => i == 0 ? coup.PgnFrNumerote : coup.PgnFr.Trim()));
        }

        public string TexteCommentaire()
        {   // La suite telle qu'elle se note dans le commentaire du coup : "Et si 14. Fc4 Cf6 15. Cf3 ⩲ (0.35)" (évaluation des Blancs, si le moteur a répondu)
            string texte = Langue.T("Et si {0}", Langue.Notation(TexteSuite()));
            return EvaluationCourante is Evaluation evaluation ? $"{texte} {evaluation.Symbole} ({evaluation.Texte})" : texte;
        }

        public string CommentaireAvec(string? commentaire) =>   // le commentaire du coup de la partie, suivi de la suite essayée
            string.IsNullOrEmpty(commentaire) ? TexteCommentaire() : $"{commentaire} {TexteCommentaire()}";

        public bool AGarder => _coups.Count > 0 && !EstLeCoupDeLaPartie;    // (rejouer le coup de la partie n'est pas une autre suite)

        public bool EstLeCoupDeLaPartie => _coups.Count == 1 && _coups[0].Uci.Trim() == CoupDeLaPartie.Uci.Trim();

        public string TexteComparaison()
        {   // Ce que la partie a donné, pour comparer avec l'évaluation de l'essai (les deux du point de vue des Blancs)
            if (EstLeCoupDeLaPartie)
                return Langue.T("C'est le coup joué dans la partie.");
            string joue = Langue.Notation(CoupDeLaPartie.PgnFrNumerote.Trim());
            return CoupDeLaPartie.EvaluationApres is Evaluation evaluation
                ? Langue.T("Dans la partie : {0} ({1}).", joue, evaluation.Texte)
                : Langue.T("Dans la partie : {0} (coup pas encore analysé).", joue);
        }
    }
}
