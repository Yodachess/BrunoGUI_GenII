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
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public record ResultatChargementPgn(
        bool FenIncomplete,         // la balise FEN était refusée (voir ErreurFen) : les coups ont été joués depuis la position initiale
        string? CoupIllisible,       // premier coup illisible ou illégal (le rejeu s'est arrêté avant lui), null si tout est joué
        int DemiCoupsJoues);

    public static class ChargementPartie
    {
        public static bool EstFenComplete(string fen) => ErreurFen(fen) == null;

        public static string NormaliseFen(string? fen) =>
            // Les 6 champs séparés par un seul espace (LitFen découpe sur les espaces)
            string.Join(' ', (fen ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        public static string? ErreurFen(string? fen)
        {   // null si la FEN est utilisable, sinon la raison (affichée à l'utilisateur) : on ne la lit jamais sans ce contrôle,
            // une FEN mal formée ferait planter la lecture ou donnerait une position incohérente
            string[] champs = NormaliseFen(fen).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (champs.Length != 6)
                return Langue.T("{0} champ(s) au lieu de 6", champs.Length);
            string[] rangees = champs[0].Split('/');
            if (rangees.Length != 8)
                return Langue.T("{0} rangée(s) au lieu de 8", rangees.Length);
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
                        return Langue.T("caractère « {0} » inconnu dans la position", c);
                }
                if (cases != 8)
                    return Langue.T("la rangée « {0} » a {1} case(s) au lieu de 8", rangee, cases);
            }
            if (roisBlancs != 1 || roisNoirs != 1)
                return Langue.T("il faut exactement un roi de chaque couleur");
            if (rangees[0].IndexOfAny(['p', 'P']) >= 0 || rangees[7].IndexOfAny(['p', 'P']) >= 0)
                return Langue.T("pion sur la première ou la dernière rangée");
            if (champs[1] is not ("w" or "b"))
                return Langue.T("trait « {0} » au lieu de w ou b", champs[1]);
            if (champs[2] != "-" && (champs[2].Any(c => !"KQkq".Contains(c)) || champs[2].Distinct().Count() != champs[2].Length))
                return Langue.T("droits de roque « {0} » illisibles", champs[2]);
            if (champs[3] != "-" && !(champs[3].Length == 2 && champs[3][0] is >= 'a' and <= 'h' && champs[3][1] is '3' or '6'))
                return Langue.T("case en passant « {0} » illisible", champs[3]);
            // Cohérence avec la position (une FEN collée ou lue dans un fichier ; la saisie de position le garantit déjà) : sinon un
            // roque ou une prise en passant impossibles devenaient jouables
            char PieceEn(char colonne, int rangee)
            {   // la pièce (lettre FEN) sur cette case, ' ' si elle est vide
                int c = 0;
                foreach (char x in rangees[8 - rangee])
                {
                    if (x is >= '1' and <= '8')
                        c += x - '0';
                    else if (c++ == colonne - 'a')
                        return x;
                    if (c > colonne - 'a')
                        return ' ';
                }
                return ' ';
            }
            if (champs[3] != "-")
            {   // la case est derrière un pion qui vient d'avancer de deux cases : 6e rangée et pion noir en 5e (Blancs au trait),
                // ou 3e rangée et pion blanc en 4e (Noirs au trait), cases de passage vides
                char colonne = champs[3][0];
                bool blancsAuTrait = champs[1] == "w";
                bool possible = blancsAuTrait
                    ? champs[3][1] == '6' && PieceEn(colonne, 5) == 'p' && PieceEn(colonne, 6) == ' ' && PieceEn(colonne, 7) == ' '
                    : champs[3][1] == '3' && PieceEn(colonne, 4) == 'P' && PieceEn(colonne, 3) == ' ' && PieceEn(colonne, 2) == ' ';
                if (!possible)
                    return Langue.T("case en passant « {0} » impossible : aucun pion ne vient d'avancer de deux cases", champs[3]);
            }
            if (champs[2] != "-")
                foreach (char droit in champs[2])
                {   // roi et tour sur leurs cases d'origine
                    (char roi, char tour, int rangee, char colonneTour) = droit switch
                    {
                        'K' => ('K', 'R', 1, 'h'), 'Q' => ('K', 'R', 1, 'a'), 'k' => ('k', 'r', 8, 'h'), _ => ('k', 'r', 8, 'a')
                    };
                    if (PieceEn('e', rangee) != roi || PieceEn(colonneTour, rangee) != tour)
                        return Langue.T("droit de roque « {0} » impossible : le roi ou la tour n'est plus sur sa case", droit);
                }
            if (!int.TryParse(champs[4], out int demiCoups) || demiCoups < 0)
                return Langue.T("compteur des 50 coups « {0} » illisible", champs[4]);
            if (!int.TryParse(champs[5], out int numero) || numero < 1)
                return Langue.T("numéro du coup « {0} » illisible", champs[5]);
            // Position impossible : le camp qui vient de jouer ne peut pas être resté en échec
            Position position = PositionDepuisFen(string.Join(' ', champs));
            if (Regles.RoiAttaque(position, Adversaire(position.QuiJoue)))
                return Langue.T("le camp qui n'a pas le trait est en échec");
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
            string? coupIllisible = null;
            int demiCoupsJoues = 0;
            // Temps de pendule ([%clk] après chaque coup) : chaque coup note le temps de son camp et le dernier temps connu de l'autre
            // (au départ : le temps initial de la balise TimeControl, s'il y en a une)
            Cadence cadence = Cadence.Lire(pgn.TimeControl);
            TimeSpan? tempsBlancs = cadence.EstSansPendule ? null : cadence.TempsInitial, tempsNoirs = tempsBlancs;
            Evaluation? evaluationAvant = null;     // [%eval] du coup précédent : évaluation de la position avant le coup
            try
            {
                foreach (string element in ElementsDesCoups(pgn.CoupsPartiePGN))  // numéros de coups et résultat compris (ignorés)
                {
                    string fenAvant = RetourneChaineFenActuel();
                    if (!GestionPartiePgn.DecodeCoupPartie(element))
                    {
                        coupIllisible = element;
                        break;
                    }
                    if (GestionPartiePgn.EstNumeroOuResultat(element))
                        continue;
                    TimeSpan? temps = Element(pgn.TempsCoups, demiCoupsJoues);
                    Coup coup = ListeCoups[^1];
                    coup.TempsReflexion = Element(pgn.TempsReflexion, demiCoupsJoues);
                    coup.Annotation = Element(pgn.Annotations, demiCoupsJoues) ?? "";
                    // [%auto] : annotation posée par une analyse de BrunoGUI, de nouveau "proposée" (plus pâle, remplaçable par une
                    // nouvelle analyse) ; sans cette marque, l'annotation est celle d'un joueur ou d'un auteur : jamais remplacée
                    coup.AnnotationProposee = coup.Annotation != "" && Element(pgn.AnnotationsAuto, demiCoupsJoues);
                    coup.CommentaireAuto = Element(pgn.CommentairesAuto, demiCoupsJoues);     // ([%autocom] : écrit par une analyse de BrunoGUI)
                    coup.CommentaireTactique = Element(pgn.CommentairesTactiques, demiCoupsJoues);   // ([%tac] : sa partie essentielle)
                    RangeAnalyseLue(coup, fenAvant, Element(pgn.Commentaires, demiCoupsJoues), Element(pgn.Evaluations, demiCoupsJoues),
                                    Element(pgn.Variantes, demiCoupsJoues), evaluationAvant);
                    if (Element(pgn.Appreciations, demiCoupsJoues) is int niveau)
                    {   // appréciation écrite en symbole ($18) : son texte ("Gain Blanc") revient à la fin du commentaire de l'analyse
                        string appreciation = JugementCoups.TexteAppreciation(niveau, coup.EvaluationApres ?? new Evaluation(0, null));
                        coup.NiveauAppreciation = niveau;
                        coup.CommentaireAuto = coup.CommentaireAuto == null ? appreciation : coup.CommentaireAuto + ". " + appreciation;
                    }
                    evaluationAvant = coup.EvaluationApres;
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

        private static T? Element<T>(List<T>? liste, int index) =>
            liste != null && index < liste.Count ? liste[index] : default;

        private static void RangeAnalyseLue(Coup coup, string fenAvant, string? commentaire, Evaluation? evaluation, string? variante, Evaluation? evaluationAvant)
        {   // Ce que le PGN dit du coup, rangé comme le ferait l'analyse de partie (affichage pendant le parcours, flèches,
            // réécriture à l'enregistrement) : son commentaire, son évaluation [%eval], et sa 1re variante, qui est l'alternative au
            // coup joué (ex : "16... Be6?? (16... Bb7)", ChessBase et BrunoGUI) : elle donne le meilleur coup et la meilleure suite
            coup.Commentaire = string.IsNullOrWhiteSpace(commentaire) ? null : commentaire;
            if (evaluation != null)
                coup.EvaluationApres = evaluation;
            if (evaluation != null || !string.IsNullOrWhiteSpace(variante))
                coup.CoupJoueLong = AnalyseDePartie.NotationLongue(fenAvant, coup.Uci.Trim(), coup.PgnFrSansNumero);
            if (string.IsNullOrWhiteSpace(variante))
                return;
            Position avant = PositionDepuisFen(fenAvant);
            string uci = VarianteSanVersUci(variante, avant);
            if (uci == "")
                return;     // variante illisible (ou qui ne part pas de la position d'avant le coup) : ignorée
            string premier = uci.Split(' ')[0];
            coup.VarianteLue = true;
            coup.MeilleurCoupUci = premier;
            coup.VarianteMeilleureUci = uci;
            coup.VarianteMeilleure = Outils.VarianteUciVersPgn(uci, DemiCoupAvant(avant), false, avant).Trim();
            coup.MeilleurCoup = AnalyseDePartie.PremierCoup(coup.VarianteMeilleure);
            coup.MeilleurCoupLong = AnalyseDePartie.NotationLongue(fenAvant, premier, coup.MeilleurCoup);
            coup.MeilleurJoue = premier == coup.Uci.Trim();
            coup.EvaluationMeilleur = evaluationAvant;
            if (evaluationAvant is Evaluation eAvant && coup.EvaluationApres is Evaluation eApres)
            {   // perte de chances de gain du camp qui a joué (pour "écart négligeable")
                double sens = avant.QuiJoue == ColorPiece.Blanc ? 1 : -1;
                coup.PerteAnalyse = Math.Max(0, sens * (JugementCoups.ChancesDeGain(eAvant) - JugementCoups.ChancesDeGain(eApres)));
            }
        }

        public static string VarianteSanVersUci(string variantePgn, Position position)
        {   // Une variante PGN ("16... Bb7 17. Nf3", lettres anglaises) jouée sur une copie de la position : ses coups au format UCI
            // ("c8b7 g1f3"), jusqu'au premier coup illisible ou illégal ; rien n'est modifié ni redessiné
            Position copie = position.Copier();
            List<string> coups = [];
            // (les sous-variantes "( ... )" sont retirées : seule la ligne principale de la variante est lue)
            foreach (string mot in ElementsDesCoups(FichierPartiePgn.SupprimeCommentaires(variantePgn, '(', ')')))
            {
                if (GestionPartiePgn.EstNumeroOuResultat(mot) || mot.All(char.IsDigit) || mot.StartsWith('$'))
                    continue;
                string san = Annotations.Separe(mot).Coup.TrimEnd('+', '#');
                if (san == "")
                    continue;
                string? uci = CoupSanEnUci(copie, san);
                if (uci == null)
                    break;
                coups.Add(uci);
                JoueCoupUci(copie, uci);
            }
            return string.Join(" ", coups);
        }

        private static string? CoupSanEnUci(Position position, string san)
        {   // Un coup SAN ("Nbd7", "exd5", "e8=Q", "O-O") pour le camp au trait de la position : son format UCI, ou null s'il n'est
            // pas légal
            ColorPiece camp = position.QuiJoue;
            if (san.StartsWith("0-0"))
                san = san.Replace('0', 'O');
            List<(string Source, string Destination)> legaux = Regles.CoupsLegaux(position);
            if (san.StartsWith("O-O"))
            {
                string rangee = camp == ColorPiece.Blanc ? "1" : "8";
                (string, string) roque = ("e" + rangee, (san == "O-O-O" ? "c" : "g") + rangee);
                return legaux.Contains(roque) ? roque.Item1 + roque.Item2 : null;
            }
            char promotion = '\0';
            int egal = san.IndexOf('=');
            if (egal > 0 && egal + 1 < san.Length)
            {
                promotion = san[egal + 1];
                san = san[..egal];
            }
            else if (san.Length >= 3 && char.IsLower(san[0]) && "QRBN".Contains(san[^1]))
            {   // promotion sans "=" ("e8Q")
                promotion = san[^1];
                san = san[..^1];
            }
            char lettre = "KQRBN".Contains(san[0]) ? san[0] : 'P';
            string reste = (lettre == 'P' ? san : san[1..]).Replace("x", "");
            if (reste.Length < 2)
                return null;
            string destination = reste[^2..], leveeDeDoute = reste[..^2];
            TypePiece attendue = (lettre, camp == ColorPiece.Blanc) switch
            {
                ('K', true) => TypePiece.RoiBlanc, ('K', false) => TypePiece.RoiNoir,
                ('Q', true) => TypePiece.ReineBlanche, ('Q', false) => TypePiece.ReineNoire,
                ('R', true) => TypePiece.TourBlanche, ('R', false) => TypePiece.TourNoire,
                ('B', true) => TypePiece.FouBlanc, ('B', false) => TypePiece.FouNoir,
                ('N', true) => TypePiece.CavalierBlanc, ('N', false) => TypePiece.CavalierNoir,
                (_, true) => TypePiece.PionBlanc, _ => TypePiece.PionNoir
            };
            foreach ((string source, string arrivee) in legaux)
                if (arrivee == destination && position.Pieces[RenvoieCaseIndex120(source)] == attendue && leveeDeDoute.All(source.Contains))
                {
                    bool derniereRangee = lettre == 'P' && (destination[1] == '8' || destination[1] == '1');
                    return source + destination + (derniereRangee ? char.ToLower(promotion == '\0' ? 'Q' : promotion).ToString() : "");
                }
            return null;
        }

        public static List<(Position Position, string Uci)> PositionsDeLaVariante(Position depart, string? varianteUci, int maximum)
        {   // Les positions successives d'une variante UCI ("e2e4 e7e5 ..."), jouée sur une copie de la position de départ (rien n'est
            // modifié ni redessiné) : au plus "maximum" demi-coups, arrêt au premier coup mal formé. Sert à rejouer la meilleure suite
            List<(Position, string)> positions = [];
            Position copie = depart.Copier();
            foreach (string uci in (varianteUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(maximum))
            {
                if (uci.Length < 4 || RenvoieCaseIndex120(uci[..2]) < 0 || RenvoieCaseIndex120(uci.Substring(2, 2)) < 0)
                    break;
                JoueCoupUci(copie, uci);
                positions.Add((copie.Copier(), uci));
            }
            return positions;
        }

        public static void JoueCoupUci(Position position, string uci)
        {   // Joue un coup UCI ("e7e8q") sur cette position (une copie, pour lire la suite d'une variante) : roque, prise en passant,
            // promotion, roques, case en passant, compteurs et trait (Regles.Jouer ; la légalité n'est pas vérifiée)
            TypePiece promotion = uci.Length >= 5 ? PieceDePromotion(uci[4], position.QuiJoue) : TypePiece.Vide;
            Regles.Jouer(position, uci[..2], uci.Substring(2, 2), promotion);
        }

        public static string[] ElementsDesCoups(string coupsPgn) =>
            // Certains fichiers PGN n'ont pas d'espace entre le numéro et le coup ("12.Cf3") : on l'ajoute
            (coupsPgn ?? "").Replace(".", ". ").Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
    }
}
