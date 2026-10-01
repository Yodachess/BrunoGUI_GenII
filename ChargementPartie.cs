// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Chargement d'une position FEN ou d'une partie PGN dans la partie, sans interface graphique (testé dans Tests/Program.cs)
//  └─ Classe "ChargementPartie"
//              ├─ "EstFenComplete"     une FEN doit avoir ses 6 champs
//              ├─ "ChargerPosition"    la partie commence à une position FEN (l'humain joue le camp au trait)
//              └─ "ChargerPartiePgn"   rejoue une partie PGN (depuis sa balise FEN s'il y en a une), puis la met en lecture seule
// Le formulaire garde les boîtes de dialogue, les messages et les textes affichés : il lit le compte rendu du chargement.

using System;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public record ResultatChargementPgn(
        bool FenIncomplete,         // la balise FEN était incomplète : les coups ont été joués depuis la position initiale
        string CoupIllisible,       // premier coup illisible ou illégal (le rejeu s'est arrêté avant lui), null si tout est joué
        int DemiCoupsJoues);

    public static class ChargementPartie
    {
        public static bool EstFenComplete(string fen) =>
            !string.IsNullOrWhiteSpace(fen) && fen.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 6;

        public static bool ChargerPosition(string fen, Partie partie)
        {   // La partie commence à cette position : trait, roques, en passant, 50 coups et numéro du coup viennent de la FEN.
            // Renvoie false (et rien ne change) si la FEN est incomplète
            if (!EstFenComplete(fen))
                return false;
            fen = fen.Trim();
            ViderCoups();
            InitialisationEchiquier();
            MiseenplaceFen(fen);
            AjoutePositionDeDepart(fen);    // élément sans coup en tête de liste : le retour arrière ne remonte jamais avant
            PromotionPiece = TypePiece.Vide;
            partie.CommencerDepuisPosition();   // l'humain joue le camp au trait, le moteur lui répond
            return true;
        }

        public static ResultatChargementPgn ChargerPartiePgn(PartieEchecsPGN pgn, Partie partie)
        {   // Rejoue la partie coup par coup (chaque coup pour le camp au trait de la position) ; au premier coup illisible
            // ou illégal, le rejeu s'arrête. La partie reste sur sa position finale, en lecture seule
            Outils.MiseaZeroListes();
            bool fenIncomplete = false;
            if (!string.IsNullOrWhiteSpace(pgn.Fen))
            {   // Partie commençant à une position ([SetUp "1"] [FEN "..."]) : les coups sont joués depuis cette position
                if (EstFenComplete(pgn.Fen))
                {
                    MiseenplaceFen(pgn.Fen.Trim());
                    AjoutePositionDeDepart(pgn.Fen.Trim());
                }
                else
                    fenIncomplete = true;
            }
            bool depuisPosition = ListeCoups.Count > 0 && ListeCoups[0].EstPositionDeDepart;
            partie.Commencer(Joueur.Humain, Joueur.Humain, depuisPosition);
            partie.RejeuPgn = true;     // pas de nulle automatique pendant le rejeu : c'est le résultat du PGN qui compte
            string coupIllisible = null;
            int demiCoupsJoues = 0;
            try
            {
                foreach (string element in ElementsDesCoups(pgn.CoupsPartiePGN))  // numéros de coups et résultat compris (ignorés)
                {
                    if (!GestionPartiePgn.DecodeCoupPartie(element))
                    {
                        coupIllisible = element;
                        break;
                    }
                    if (!GestionPartiePgn.EstNumeroOuResultat(element))
                        demiCoupsJoues++;
                }
            }
            finally
            {
                partie.RejeuPgn = false;
            }
            partie.PasserEnLectureSeule();
            return new ResultatChargementPgn(fenIncomplete, coupIllisible, demiCoupsJoues);
        }

        public static string[] ElementsDesCoups(string coupsPgn) =>
            // Certains fichiers PGN n'ont pas d'espace entre le numéro et le coup ("12.Cf3") : on l'ajoute
            (coupsPgn ?? "").Replace(".", ". ").Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
    }
}
