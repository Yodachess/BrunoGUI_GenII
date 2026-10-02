// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Un coup de la partie, dans toutes ses notations (voir LogiqueMouvements.ListeCoups)
// ├─ Classe "Coup"
// │            └─ "PositionDeDepart"   Elément de départ d'une partie commencée à partir d'un FEN (sans coup)
// └─ Classe "VueCoups"                 Vue en lecture seule d'une notation (ex : LogiqueMouvements.ListeCoupsFen)

using System;
using System.Collections;
using System.Collections.Generic;

namespace BrunoGUI_GenII
{
    public class Coup
    {
        public string Fen { get; init; } = "";          // position APRES le coup (ou position de départ)
        public string PgnIntl { get; init; } = "";      // ex : "12. Nf3+ " (le numéro n'est présent que pour les coups blancs)
        public string PgnFr { get; init; } = "";        // ex : "12. Cf3+ "
        public string Nal { get; init; } = "";          // notation algébrique longue, ex : "12. Cg1-f3+ "
        public string Uci { get; init; } = "";          // ex : "g1f3 "
        public bool EstPositionDeDepart { get; init; }  // true : pas un coup, seulement la position de départ (partie chargée depuis un FEN)
        // Partie à la pendule : temps restant de chaque camp juste après ce coup (null sans pendule). Notés par l'interface après
        // le coup (la logique ne connaît pas la pendule) ; servent à remettre la pendule à l'heure (retour arrière, "Reprendre ici")
        public TimeSpan? TempsBlancs { get; set; }
        public TimeSpan? TempsNoirs { get; set; }

        public static Coup PositionDeDepart(string fen) => new() { Fen = fen, EstPositionDeDepart = true };

        // Après un coup blanc, ce sont les Noirs qui ont le trait (2e champ de la FEN de la position après le coup)
        public bool EstCoupBlanc => !EstPositionDeDepart && Fen.Split(' ')[1] == "b";
        // Numéro du coup complet : 12 pour "12. Cf3" comme pour "12... Fe7" (le 6e champ de la FEN augmente après chaque coup noir)
        public int NumeroDuCoup => int.Parse(Fen.Split(' ')[5]) - (EstCoupBlanc ? 0 : 1);
        public string PgnFrSansNumero
        {
            get
            {
                string texte = PgnFr.Trim();
                return EstCoupBlanc && texte.Contains(' ') ? texte[(texte.IndexOf(' ') + 1)..] : texte;
            }
        }
    }

    public static class FeuilleDePartie
    {   // Lignes de la feuille de partie (fenêtre "Liste des coups") : numéro, coup blanc, coup noir.
        // Une partie commencée depuis un FEN avec les Noirs au trait a une première ligne "n. | ... | coup noir"
        public static List<string[]> Lignes(IEnumerable<Coup> coups)
        {
            List<string[]> lignes = [];
            foreach (Coup coup in coups)
            {
                if (coup.EstPositionDeDepart)
                    continue;
                if (coup.EstCoupBlanc)
                    lignes.Add([coup.NumeroDuCoup + ".", coup.PgnFrSansNumero, ""]);
                else if (lignes.Count > 0 && lignes[^1][2] == "")
                    lignes[^1][2] = coup.PgnFrSansNumero;
                else
                    lignes.Add([coup.NumeroDuCoup + ".", "...", coup.PgnFrSansNumero]);
            }
            return lignes;
        }
        public static bool CommenceParLesNoirs(IEnumerable<Coup> coups)
        {   // Le premier coup joué est-il un coup noir ? (la feuille commence alors par "...")
            foreach (Coup coup in coups)
                if (!coup.EstPositionDeDepart)
                    return !coup.EstCoupBlanc;
            return false;
        }
    }

    public class VueCoups(IReadOnlyList<Coup> coups, Func<Coup, string> notation) : IReadOnlyList<string>
    {   // Une notation de tous les coups, en lecture seule : les coups s'ajoutent et se retirent uniquement
        // par LogiqueMouvements (AjouteCoup, RetireDernierCoup, ViderCoups), ce qui garde toutes les notations alignées
        public string this[int index] => notation(coups[index]);
        public int Count => coups.Count;
        public IEnumerator<string> GetEnumerator()
        {
            foreach (Coup coup in coups)
                yield return notation(coup);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
