// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Chargement d'une position FEN ou d'une partie PGN dans la partie, sans interface graphique (testé dans Tests/Program.cs)
//  └─ Classe "ChargementPartie"
//              ├─ "ErreurFen"          contrôle d'une FEN (6 champs, 8 rangées de 8 cases, un roi par camp...) : la raison, ou null
//              ├─ "EstFenComplete"     ErreurFen == null ; "NormaliseFen" : champs séparés par un seul espace
//              ├─ "ChargerPosition"    la partie commence à une position FEN (l'humain joue le camp au trait)
//              └─ "ChargerPartiePgn"   rejoue une partie PGN (depuis sa balise FEN s'il y en a une), puis la met en lecture seule ;
//                                      les temps de pendule ([%clk]) sont notés dans chaque coup (Coup.TempsBlancs/TempsNoirs)
// Le formulaire garde les boîtes de dialogue, les messages et les textes affichés : il lit le compte rendu du chargement.

using System;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public record ResultatChargementPgn(
        bool FenIncomplete,         // la balise FEN était refusée (voir ErreurFen) : les coups ont été joués depuis la position initiale
        string CoupIllisible,       // premier coup illisible ou illégal (le rejeu s'est arrêté avant lui), null si tout est joué
        int DemiCoupsJoues);

    public static class ChargementPartie
    {
        public static bool EstFenComplete(string fen) => ErreurFen(fen) == null;

        public static string NormaliseFen(string fen) =>
            // Les 6 champs séparés par un seul espace (LitFen découpe sur les espaces)
            string.Join(' ', (fen ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

        public static string ErreurFen(string fen)
        {   // null si la FEN est utilisable, sinon la raison (affichée à l'utilisateur) : on ne la lit jamais sans ce contrôle,
            // une FEN mal formée ferait planter la lecture ou donnerait une position incohérente
            string[] champs = NormaliseFen(fen).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (champs.Length != 6)
                return $"{champs.Length} champ(s) au lieu de 6";
            string[] rangees = champs[0].Split('/');
            if (rangees.Length != 8)
                return $"{rangees.Length} rangée(s) au lieu de 8";
            int roisBlancs = 0, roisNoirs = 0;
            foreach (string rangee in rangees)
            {
                int cases = 0;
                foreach (char c in rangee)
                {
                    if (c is >= '1' and <= '8')
                        cases += c - '0';
                    else if ("pnbrqkPNBRQK".Contains(c))
                    {
                        cases++;
                        if (c == 'K') roisBlancs++;
                        if (c == 'k') roisNoirs++;
                    }
                    else
                        return $"caractère « {c} » inconnu dans la position";
                }
                if (cases != 8)
                    return $"la rangée « {rangee} » a {cases} case(s) au lieu de 8";
            }
            if (roisBlancs != 1 || roisNoirs != 1)
                return "il faut exactement un roi de chaque couleur";
            if (rangees[0].IndexOfAny(['p', 'P']) >= 0 || rangees[7].IndexOfAny(['p', 'P']) >= 0)
                return "pion sur la première ou la dernière rangée";
            if (champs[1] is not ("w" or "b"))
                return $"trait « {champs[1]} » au lieu de w ou b";
            if (champs[2] != "-" && (champs[2].Any(c => !"KQkq".Contains(c)) || champs[2].Distinct().Count() != champs[2].Length))
                return $"droits de roque « {champs[2]} » illisibles";
            if (champs[3] != "-" && !(champs[3].Length == 2 && champs[3][0] is >= 'a' and <= 'h' && champs[3][1] is '3' or '6'))
                return $"case en passant « {champs[3]} » illisible";
            if (!int.TryParse(champs[4], out int demiCoups) || demiCoups < 0)
                return $"compteur des 50 coups « {champs[4]} » illisible";
            if (!int.TryParse(champs[5], out int numero) || numero < 1)
                return $"numéro du coup « {champs[5]} » illisible";
            // Position impossible : le camp qui vient de jouer ne peut pas être resté en échec
            Position position = PositionDepuisFen(string.Join(' ', champs));
            if (CalculerSur(position, () => { QuiJoue = Adversaire(QuiJoue); return CampAuTraitEnEchec(); }))
                return "le camp qui n'a pas le trait est en échec";
            return null;
        }

        public static bool ChargerPosition(string fen, Partie partie)
        {   // La partie commence à cette position : trait, roques, en passant, 50 coups et numéro du coup viennent de la FEN.
            // Renvoie false (et rien ne change) si la FEN est mal formée (voir ErreurFen)
            if (!EstFenComplete(fen))
                return false;
            fen = NormaliseFen(fen);
            ViderCoups();
            InitialisationEchiquier();
            MiseenplaceFen(fen);
            AjoutePositionDeDepart(fen);    // élément sans coup en tête de liste : le retour arrière ne remonte jamais avant
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
                    MiseenplaceFen(NormaliseFen(pgn.Fen));
                    AjoutePositionDeDepart(NormaliseFen(pgn.Fen));
                }
                else
                    fenIncomplete = true;
            }
            bool depuisPosition = ListeCoups.Count > 0 && ListeCoups[0].EstPositionDeDepart;
            partie.Commencer(Joueur.Humain, Joueur.Humain, depuisPosition);
            partie.RejeuPgn = true;     // pas de nulle automatique pendant le rejeu : c'est le résultat du PGN qui compte
            string coupIllisible = null;
            int demiCoupsJoues = 0;
            // Temps de pendule ([%clk] après chaque coup) : chaque coup note le temps de son camp et le dernier temps connu de l'autre
            // (au départ : le temps initial de la balise TimeControl, s'il y en a une)
            Cadence cadence = Cadence.Lire(pgn.TimeControl);
            TimeSpan? tempsBlancs = cadence.EstSansPendule ? null : cadence.TempsInitial, tempsNoirs = tempsBlancs;
            try
            {
                foreach (string element in ElementsDesCoups(pgn.CoupsPartiePGN))  // numéros de coups et résultat compris (ignorés)
                {
                    if (!GestionPartiePgn.DecodeCoupPartie(element))
                    {
                        coupIllisible = element;
                        break;
                    }
                    if (GestionPartiePgn.EstNumeroOuResultat(element))
                        continue;
                    TimeSpan? temps = demiCoupsJoues < (pgn.TempsCoups?.Count ?? 0) ? pgn.TempsCoups[demiCoupsJoues] : null;
                    Coup coup = ListeCoups[^1];
                    coup.TempsReflexion = demiCoupsJoues < (pgn.TempsReflexion?.Count ?? 0) ? pgn.TempsReflexion[demiCoupsJoues] : null;
                    demiCoupsJoues++;
                    if (temps != null && coup.EstCoupBlanc)
                        tempsBlancs = temps;
                    else if (temps != null)
                        tempsNoirs = temps;
                    if (tempsBlancs != null || tempsNoirs != null)
                    {
                        coup.TempsBlancs = tempsBlancs;
                        coup.TempsNoirs = tempsNoirs;
                    }
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
