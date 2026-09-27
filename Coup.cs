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

        public static Coup PositionDeDepart(string fen) => new() { Fen = fen, EstPositionDeDepart = true };
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
