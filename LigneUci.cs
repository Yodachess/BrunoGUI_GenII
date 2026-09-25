// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Décodage d'une ligne envoyée par le moteur UCI (une seule fois, dans MoteurUci)
// └─ Classe "LigneUci"
//              └─ "Analyser"   Découpe la ligne et remplit les propriétés selon la commande :
//                              bestmove (MeilleurCoup, CoupConseil), id (NomMoteur, AuteurMoteur), option (NomOption),
//                              info (NumeroVariante, ScoreCentipions, MatEn, Variante, DansBibliotheque)
// Protocole : https://official-stockfish.github.io/docs/stockfish-wiki/UCI-&-Commands.html
// Une propriété vaut null quand l'information est absente de la ligne.

using System;

namespace BrunoGUI_GenII
{
    public class LigneUci
    {
        public string Texte { get; private set; } = "";        // la ligne complète, sans espaces de début et de fin
        public string Commande { get; private set; } = "";     // premier mot : info, bestmove, id, option, readyok, uciok ...

        // bestmove <coup> [ponder <coup>]
        public string MeilleurCoup { get; private set; }
        public string CoupConseil { get; private set; }        // coup attendu de l'adversaire (ponder)
        public bool AucunCoupLegal => Commande == "bestmove" && (MeilleurCoup == null || MeilleurCoup == "(none)" || MeilleurCoup == "0000");

        // id name <nom> / id author <auteur>
        public string NomMoteur { get; private set; }
        public string AuteurMoteur { get; private set; }

        // option name <nom> type ...
        public string NomOption { get; private set; }

        // info ... multipv <n> score cp <x> | score mate <y> ... pv <coups>
        public bool DansBibliotheque { get; private set; }     // mot "book" (certains moteurs l'indiquent)
        public int? NumeroVariante { get; private set; }       // multipv
        public int? ScoreCentipions { get; private set; }      // score cp, du point de vue du camp au trait
        public int? MatEn { get; private set; }                // score mate, négatif si le camp au trait est maté
        public string Variante { get; private set; }           // coups UCI après "pv", séparés par des espaces

        public static LigneUci Analyser(string ligne)
        {
            LigneUci resultat = new();
            if (string.IsNullOrWhiteSpace(ligne))
                return resultat;
            resultat.Texte = ligne.Trim();
            string[] mots = resultat.Texte.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            resultat.Commande = mots[0];
            switch (resultat.Commande)
            {
                case "bestmove":
                    resultat.MeilleurCoup = MotApres(mots, 0);
                    int indexPonder = Array.IndexOf(mots, "ponder");
                    if (indexPonder > 0)
                        resultat.CoupConseil = MotApres(mots, indexPonder);
                    break;
                case "id":
                    if (mots.Length > 1 && mots[1] == "name")
                        resultat.NomMoteur = TexteApres(resultat.Texte, "id name ");
                    if (mots.Length > 1 && mots[1] == "author")
                        resultat.AuteurMoteur = TexteApres(resultat.Texte, "id author ");
                    break;
                case "option":
                    if (mots.Length > 2 && mots[1] == "name")
                    {   // Le nom peut contenir des espaces (ex : "Skill Level") : il va jusqu'au mot "type"
                        int indexType = Array.IndexOf(mots, "type");
                        int fin = indexType > 2 ? indexType : mots.Length;
                        resultat.NomOption = string.Join(" ", mots, 2, fin - 2);
                    }
                    break;
                case "info":
                    if (mots.Length > 1 && mots[1] == "string")
                        break;                  // texte libre du moteur : ses mots ne sont pas des mots-clés
                    AnalyserInfo(resultat, mots);
                    break;
            }
            return resultat;
        }

        private static void AnalyserInfo(LigneUci resultat, string[] mots)
        {
            for (int i = 1; i < mots.Length; i++)
            {
                switch (mots[i])
                {
                    case "book":
                        resultat.DansBibliotheque = true;
                        break;
                    case "multipv":
                        resultat.NumeroVariante = EntierApres(mots, i);
                        break;
                    case "cp":
                        resultat.ScoreCentipions = EntierApres(mots, i);
                        break;
                    case "mate":
                        resultat.MatEn = EntierApres(mots, i);
                        break;
                    case "pv":      // la variante occupe toute la fin de la ligne
                        if (i + 1 < mots.Length)
                            resultat.Variante = string.Join(" ", mots, i + 1, mots.Length - i - 1);
                        return;
                }
            }
        }

        private static string MotApres(string[] mots, int index) => index + 1 < mots.Length ? mots[index + 1] : null;

        private static int? EntierApres(string[] mots, int index) => int.TryParse(MotApres(mots, index), out int valeur) ? valeur : null;

        private static string TexteApres(string texte, string debut) => texte.Length > debut.Length ? texte[debut.Length..] : "";
    }
}
