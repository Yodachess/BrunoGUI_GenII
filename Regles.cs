// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Les règles du jeu sur une position donnée : déplacements, légalité, échec, mat et pat, notation, lecture et écriture FEN.
// Chaque fonction travaille sur la Position qu'on lui passe : aucun état statique, aucun affichage, rien sur la partie
// (LogiqueMouvements.PositionActuelle n'est jamais lue ici). On peut donc s'en servir sur n'importe quelle position, y compris
// sur un autre thread. LogiqueMouvements garde la position de la partie, la liste des coups et les événements d'affichage,
// et appelle ces fonctions.
// Les déplacements d'une case sont rendus comme des noms de cases : "e4" (case vide) ou "xe5" (prise).

using System;
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    // Ce que Jouer a fait du coup : pièce promue (Vide : pas de promotion), prise en passant, et si un second pion pouvait
    // faire la même prise en passant (la notation précise alors la colonne de départ)
    public readonly record struct ResultatCoup(TypePiece PiecePromue, bool EnPassant, bool SecondPion);

    public static class Regles
    {
        private static readonly Dictionary<FlagMouvementRoque, string> CasesTraverseesRoi = new()
        {
            { FlagMouvementRoque.PetitRoqueBlanc, "f1" },
            { FlagMouvementRoque.GrandRoqueBlanc, "d1" },
            { FlagMouvementRoque.PetitRoqueNoir, "f8" },
            { FlagMouvementRoque.GrandRoqueNoir, "d8" }
        };
        private static readonly string[] LettresFen = ["k", "K", "q", "Q", "b", "B", "r", "R", "n", "N", "p", "P"];  // index = TypePiece
        private static readonly int[] DeplacementsCavalier = [+8, +12, +19, +21, -8, -12, -19, -21];
        private static readonly int[] DeplacementsFou = [+9, -9, +11, -11];
        private static readonly int[] DeplacementsTour = [+1, -1, +10, -10];

        // ═══ FEN ═══

        public static Position LireFen(string fen)
        {   // Une nouvelle position lue dans la FEN (déjà contrôlée : voir ChargementPartie.ErreurFen)
            Position position = new();
            LireFenDans(position, fen);
            return position;
        }

        public static void LireFenDans(Position p, string fen)
        {   // Lit la FEN dans la position (Echec et EchecetMat, absents de la FEN, ne sont pas touchés)
            string[] champs = fen.Split(' ');
            p.QuiJoue = champs[1] == "w" ? ColorPiece.Blanc : ColorPiece.Noir;
            // numéro du coup : x si c'est aux Blancs de jouer, x + 0.5 si c'est aux Noirs
            p.NombreCoupsJoues = p.QuiJoue == ColorPiece.Blanc ? int.Parse(champs[5]) : int.Parse(champs[5]) + 0.5f;
            p.IndexCaseEnPassant = champs[3] == "-" ? 0 : RenvoieCaseIndex120(champs[3]);
            p.PetitRoqueBlancPossible = champs[2].Contains('K');
            p.GrandRoqueBlancPossible = champs[2].Contains('Q');
            p.PetitRoqueNoirPossible = champs[2].Contains('k');
            p.GrandRoqueNoirPossible = champs[2].Contains('q');
            if (champs.Length > 4 && int.TryParse(champs[4], out int demiCoupsSansPrise))
                p.SansPrise = demiCoupsSansPrise;
            string[] rangees = champs[0].Split('/');
            Array.Reverse(rangees);
            for (int r = 0; r < 8; r++)
            {
                int colonne = 0;
                foreach (char c in rangees[r])
                {
                    if (char.IsDigit(c))
                    {
                        for (int vide = 1; vide <= (int)char.GetNumericValue(c); vide++)
                            p.Pieces[20 + r * 10 + ++colonne] = TypePiece.Vide;
                        continue;
                    }
                    int index = Array.IndexOf(LettresFen, c.ToString());
                    colonne++;
                    if (index >= 0)
                        p.Pieces[20 + r * 10 + colonne] = (TypePiece)index;
                }
            }
        }

        public static string Fen(Position p)
        {   // La FEN de la position
            List<string> rangees = [];
            for (int i = 90; i >= 20; i -= 10)
            {
                string rangee = "";
                int vides = 0;
                for (int j = 1; j <= 8; j++)
                {
                    if (p.Pieces[i + j] == TypePiece.Vide)
                    {
                        vides++;
                        continue;
                    }
                    if (vides > 0)
                        rangee += vides.ToString();
                    vides = 0;
                    rangee += LettresFen[(int)p.Pieces[i + j]];
                }
                if (vides > 0)
                    rangee += vides.ToString();
                rangees.Add(rangee);
            }
            string roques = (p.PetitRoqueBlancPossible ? "K" : "") + (p.GrandRoqueBlancPossible ? "Q" : "")
                          + (p.PetitRoqueNoirPossible ? "k" : "") + (p.GrandRoqueNoirPossible ? "q" : "");
            return string.Join("/", rangees) + " " + (p.QuiJoue == ColorPiece.Blanc ? "w" : "b") + " " + (roques == "" ? "-" : roques)
                 + " " + (p.IndexCaseEnPassant != 0 ? NomCaseAlgebrique(p.IndexCaseEnPassant) : "-") + " " + p.SansPrise
                 + " " + Math.Truncate(p.NombreCoupsJoues).ToString();
        }

        // ═══ Couleurs des cases ═══

        public static ColorPiece CouleurCase(Position p, int index)
        {   // Couleur de la pièce sur la case (Vide, Blanc, Noir, ou BordPlateau hors de l'échiquier)
            if (index < 0 || index >= p.Pieces.Count || p.Pieces[index] == TypePiece.Bordure)
                return ColorPiece.BordPlateau;
            if (p.Pieces[index] == TypePiece.Vide)
                return ColorPiece.Vide;
            return (int)p.Pieces[index] % 2 == 0 ? ColorPiece.Noir : ColorPiece.Blanc;    // pièces noires paires, blanches impaires
        }

        public static ColorPiece CouleurAdversaireCase(Position p, int index)
        {   // Couleur adverse de la pièce sur la case (une case vide ou hors de l'échiquier rend sa propre couleur)
            ColorPiece couleur = CouleurCase(p, index);
            return couleur is ColorPiece.Blanc or ColorPiece.Noir ? Adversaire(couleur) : couleur;
        }

        // ═══ Déplacements (sans vérifier les échecs) ═══

        public static List<string> Mouvements(Position p, string nomCase)
        {   // Tous les déplacements de la pièce de la case, sans vérifier l'échec ("e4" ou "xe5") ; aucun pour une case vide
            int index = RenvoieCaseIndex120(nomCase);
            if (index < 0)
                return [];
            switch (p.Pieces[index])
            {
                case TypePiece.ReineBlanche:
                case TypePiece.ReineNoire:
                    return MouvementsReine(p, index);
                case TypePiece.RoiBlanc:
                case TypePiece.RoiNoir:
                    return MouvementsRoi(p, index);
                case TypePiece.FouBlanc:
                case TypePiece.FouNoir:
                    return MouvementsGlissants(p, index, DeplacementsFou);
                case TypePiece.TourBlanche:
                case TypePiece.TourNoire:
                    return MouvementsGlissants(p, index, DeplacementsTour);
                case TypePiece.CavalierBlanc:
                case TypePiece.CavalierNoir:
                    List<string> sauts = [];
                    foreach (int saut in DeplacementsCavalier)
                        AjouteCase(p, index, index + saut, sauts, true);
                    return sauts;
                case TypePiece.PionBlanc:
                case TypePiece.PionNoir:
                    return MouvementsPion(p, index);
                default:
                    return [];
            }
        }

        private static List<string> MouvementsReine(Position p, int index)
        {   // Fou + tour (sert aussi au roi, d'une seule case)
            List<string> mouvements = MouvementsGlissants(p, index, DeplacementsFou);
            mouvements.AddRange(MouvementsGlissants(p, index, DeplacementsTour));
            return mouvements;
        }

        private static List<string> MouvementsGlissants(Position p, int index, int[] directions)
        {
            List<string> mouvements = [];
            foreach (int direction in directions)
                AjouteDirection(p, index, direction, mouvements);
            return mouvements;
        }

        private static void AjouteDirection(Position p, int source, int direction, List<string> mouvements)
        {   // Cases libres dans la direction, puis la prise éventuelle (le roi s'arrête après une case)
            int suivante = source + direction;
            while (p.Pieces[suivante] == TypePiece.Vide)
            {
                mouvements.Add(NomCaseAlgebrique(suivante));
                if (p.Pieces[source] == TypePiece.RoiBlanc || p.Pieces[source] == TypePiece.RoiNoir)
                    return;
                suivante += direction;
            }
            if (CouleurCase(p, suivante) == CouleurAdversaireCase(p, source))
                mouvements.Add("x" + NomCaseAlgebrique(suivante));
        }

        private static void AjouteCase(Position p, int source, int destination, List<string> mouvements, bool avecPrise)
        {   // Une case d'arrivée du cavalier ou du pion
            if (p.Pieces[destination] == TypePiece.Bordure)
                return;
            if (p.Pieces[destination] == TypePiece.Vide)
                mouvements.Add(NomCaseAlgebrique(destination));
            if (avecPrise && CouleurCase(p, destination) == CouleurAdversaireCase(p, source))
                mouvements.Add("x" + NomCaseAlgebrique(destination));
        }

        private static List<string> MouvementsRoi(Position p, int index)
        {   // Les 8 cases autour, plus les roques (droit au roque, tour dans son coin, cases libres ; l'échec est vu par EstLegal)
            List<string> mouvements = MouvementsReine(p, index);
            bool roiBlancDepart = index == 25 && p.Pieces[index] == TypePiece.RoiBlanc;
            bool roiNoirDepart = index == 95 && p.Pieces[index] == TypePiece.RoiNoir;
            if (!roiBlancDepart && !roiNoirDepart)
                return mouvements;
            TypePiece tour = roiBlancDepart ? TypePiece.TourBlanche : TypePiece.TourNoire;
            if (roiBlancDepart ? p.PetitRoqueBlancPossible : p.PetitRoqueNoirPossible)
                if (p.Pieces[index + 3] == tour && p.Pieces[index + 1] == TypePiece.Vide && p.Pieces[index + 2] == TypePiece.Vide)
                    AjouteDirection(p, index, +2, mouvements);
            if (roiBlancDepart ? p.GrandRoqueBlancPossible : p.GrandRoqueNoirPossible)
                if (p.Pieces[index - 4] == tour && p.Pieces[index - 1] == TypePiece.Vide && p.Pieces[index - 2] == TypePiece.Vide && p.Pieces[index - 3] == TypePiece.Vide)
                    AjouteDirection(p, index, -2, mouvements);
            return mouvements;
        }

        private static List<string> MouvementsPion(Position p, int index)
        {   // Avance d'une case, de deux depuis la rangée de départ, prises (et prise en passant)
            List<string> mouvements = [];
            int sens = p.Pieces[index] == TypePiece.PionBlanc ? 10 : -10;
            int rangeeDepart = p.Pieces[index] == TypePiece.PionBlanc ? 3 : 8;
            AjouteCase(p, index, index + sens, mouvements, false);
            if (index / 10 == rangeeDepart && p.Pieces[index + sens] == TypePiece.Vide)
                AjouteCase(p, index, index + 2 * sens, mouvements, false);
            AjoutePrise(p, index, index + sens - Math.Sign(sens), mouvements);      // (blanc : +9 puis +11 ; noir : -9 puis -11)
            AjoutePrise(p, index, index + sens + Math.Sign(sens), mouvements);
            return mouvements;
        }

        private static void AjoutePrise(Position p, int source, int destination, List<string> mouvements)
        {   // Prise d'un pion ; la prise en passant seulement pour un pion du camp au trait
            if (CouleurCase(p, destination) == CouleurAdversaireCase(p, source))
                mouvements.Add("x" + NomCaseAlgebrique(destination));
            if (destination == p.IndexCaseEnPassant && p.QuiJoue == CouleurCase(p, source))
                mouvements.Add("x" + NomCaseAlgebrique(destination));
        }

        public static List<string> CasesPiecesMenacantes(Position p, string nomCase)
        {   // Les cases des pièces qui peuvent prendre sur cette case : pièces adverses à celle qui l'occupe, ou, si elle est vide,
            // pièces adverses au camp au trait (cases traversées par le roi qui roque)
            List<string> menaces = [];
            int index = RenvoieCaseIndex120(nomCase);
            if (index < 0)
                return menaces;
            ColorPiece couleurMenace = p.Pieces[index] == TypePiece.Vide ? Adversaire(p.QuiJoue) : CouleurAdversaireCase(p, index);
            for (int i = 21; i <= 98; i++)
            {
                if (CouleurCase(p, i) != couleurMenace)
                    continue;
                string caseMenace = NomCaseAlgebrique(i);
                if (Mouvements(p, caseMenace).Any(m => m.StartsWith('x') && m.Substring(1, 2) == nomCase))
                    menaces.Add(caseMenace);
            }
            return menaces;
        }

        // ═══ Échec, coups légaux, mat et pat ═══

        public static bool RoiAttaque(Position p, ColorPiece couleurDuRoi)
        {   // Le roi de cette couleur est-il attaqué ? (faux s'il n'y en a pas)
            int index = p.Pieces.IndexOf(couleurDuRoi == ColorPiece.Blanc ? TypePiece.RoiBlanc : TypePiece.RoiNoir);
            return index >= 0 && CasesPiecesMenacantes(p, NomCaseAlgebrique(index)).Count > 0;
        }

        public static bool CampAuTraitEnEchec(Position p) => RoiAttaque(p, p.QuiJoue);

        public static bool EstLegal(Position p, string source, string destination)
        {   // Le coup (de la pièce du camp au trait) est-il légal : déplacement possible, roque sans échec ni case traversée
            // contrôlée, et le roi du joueur pas en échec après le coup
            int indexSource = RenvoieCaseIndex120(source);
            int indexDestination = RenvoieCaseIndex120(destination);
            if (indexSource < 0 || indexDestination < 0 || CouleurCase(p, indexSource) != p.QuiJoue)
                return false;
            if (!Mouvements(p, source).Any(m => m.Contains(destination)))
                return false;
            FlagMouvementRoque roque = TestSiRoque(p, indexSource, indexDestination);
            if (roque != FlagMouvementRoque.PasDeRoque && (RoiAttaque(p, p.QuiJoue) || !EstLegal(p, source, CasesTraverseesRoi[roque])))
                return false;
            Position copie = p.Copier();
            SimuleCoup(copie, indexSource, indexDestination);
            return !RoiAttaque(copie, p.QuiJoue);
        }

        public static List<string> MouvementsValides(Position p, string nomCase)
            => [.. Mouvements(p, nomCase).Where(m => EstLegal(p, nomCase, m[^2..]))];

        public static List<(string Source, string Destination)> CoupsLegaux(Position p)
        {   // Tous les coups légaux du camp au trait (une promotion n'apparaît qu'une fois, quelle que soit la pièce choisie)
            List<(string Source, string Destination)> coups = [];
            for (int i = 21; i <= 98; i++)
            {
                if (CouleurCase(p, i) != p.QuiJoue)
                    continue;
                string source = NomCaseAlgebrique(i);
                foreach (string mouvement in MouvementsValides(p, source))
                    if (!coups.Contains((source, mouvement[^2..])))
                        coups.Add((source, mouvement[^2..]));
            }
            return coups;
        }

        public static bool ResteCoupsJouables(Position p)
        {   // Le camp au trait a-t-il au moins un coup légal ? (sinon : mat s'il est en échec, pat sinon)
            for (int i = 21; i <= 98; i++)
                if (CouleurCase(p, i) == p.QuiJoue && MouvementsValides(p, NomCaseAlgebrique(i)).Count > 0)
                    return true;
            return false;
        }

        public static bool EstMat(Position p) => CampAuTraitEnEchec(p) && !ResteCoupsJouables(p);

        // ═══ Jouer un coup ═══

        public static FlagMouvementRoque TestSiRoque(Position p, int source, int destination)
        {   // Le déplacement est-il un roque (roi sur sa case de départ qui va deux cases à côté) ?
            if (source == 25 && p.Pieces[source] == TypePiece.RoiBlanc)
            {
                if (destination == 27) return FlagMouvementRoque.PetitRoqueBlanc;
                if (destination == 23) return FlagMouvementRoque.GrandRoqueBlanc;
            }
            if (source == 95 && p.Pieces[source] == TypePiece.RoiNoir)
            {
                if (destination == 97) return FlagMouvementRoque.PetitRoqueNoir;
                if (destination == 93) return FlagMouvementRoque.GrandRoqueNoir;
            }
            return FlagMouvementRoque.PasDeRoque;
        }

        private static bool EstPriseEnPassant(Position p, int source, int destination)
            => p.IndexCaseEnPassant == destination && (p.Pieces[source] == TypePiece.PionBlanc || p.Pieces[source] == TypePiece.PionNoir);

        public static bool EstPromotion(Position p, int source, int destination)
            => (p.Pieces[source] == TypePiece.PionBlanc || p.Pieces[source] == TypePiece.PionNoir)
               && ((destination >= 21 && destination <= 28) || (destination >= 91 && destination <= 98));

        private static void Deplace(Position p, int source, int destination)
        {
            p.Pieces[destination] = p.Pieces[source];
            p.Pieces[source] = TypePiece.Vide;
        }

        public static void SimuleCoup(Position p, int source, int destination)
        {   // Déplace les pièces seulement (avec la tour du roque et le pion pris en passant), sans changer le trait, les roques,
            // la case en passant ni les compteurs, et sans promotion
            FlagMouvementRoque roque = TestSiRoque(p, source, destination);
            if (EstPriseEnPassant(p, source, destination))
                p.Pieces[destination + (p.Pieces[source] == TypePiece.PionBlanc ? -10 : +10)] = TypePiece.Vide;
            Deplace(p, source, destination);
            switch (roque)
            {
                case FlagMouvementRoque.PetitRoqueBlanc: Deplace(p, 28, 26); break;
                case FlagMouvementRoque.GrandRoqueBlanc: Deplace(p, 21, 24); break;
                case FlagMouvementRoque.PetitRoqueNoir: Deplace(p, 98, 96); break;
                case FlagMouvementRoque.GrandRoqueNoir: Deplace(p, 91, 94); break;
            }
        }

        public static ResultatCoup Jouer(Position p, string source, string destination, TypePiece promotion = TypePiece.Vide)
        {   // Joue le coup (déjà vérifié par EstLegal) : pièces, roques, case en passant, compteur des 50 coups, trait et numéro du
            // coup. promotion : la pièce choisie pour un pion qui arrive au bout (prise dans la couleur du pion ; Vide : une dame).
            // Echec et EchecetMat ne sont pas calculés ici
            int indexSource = RenvoieCaseIndex120(source);
            int indexDestination = RenvoieCaseIndex120(destination);
            TypePiece piecePromue = TypePiece.Vide;
            bool enPassant = false, secondPion = false;
            FlagMouvementRoque roque = TestSiRoque(p, indexSource, indexDestination);
            if (roque != FlagMouvementRoque.PasDeRoque)
                Roque(p, roque);
            else
            {
                TypePiece piece = p.Pieces[indexSource];
                bool pion = piece == TypePiece.PionBlanc || piece == TypePiece.PionNoir;
                if (EstPriseEnPassant(p, indexSource, indexDestination))
                {
                    secondPion = PrendEnPassant(p, indexSource);
                    enPassant = true;
                }
                p.IndexCaseEnPassant = pion && Math.Abs(indexSource - indexDestination) == 20 ? (indexSource + indexDestination) / 2 : 0;
                p.SansPrise = pion || p.Pieces[indexDestination] != TypePiece.Vide ? 0 : p.SansPrise + 1;
                // un roi qui bouge perd ses deux roques ; une tour qui quitte son coin, ou y est prise, celui de son côté
                if (piece == TypePiece.RoiBlanc)
                    p.PetitRoqueBlancPossible = p.GrandRoqueBlancPossible = false;
                if (piece == TypePiece.RoiNoir)
                    p.PetitRoqueNoirPossible = p.GrandRoqueNoirPossible = false;
                if (indexSource == 21 || indexDestination == 21) p.GrandRoqueBlancPossible = false;   // a1
                if (indexSource == 28 || indexDestination == 28) p.PetitRoqueBlancPossible = false;   // h1
                if (indexSource == 91 || indexDestination == 91) p.GrandRoqueNoirPossible = false;    // a8
                if (indexSource == 98 || indexDestination == 98) p.PetitRoqueNoirPossible = false;    // h8
                bool promotionDuPion = EstPromotion(p, indexSource, indexDestination);
                Deplace(p, indexSource, indexDestination);
                if (promotionDuPion)
                {
                    piecePromue = PieceDeLaCouleur(promotion, piece == TypePiece.PionBlanc ? ColorPiece.Blanc : ColorPiece.Noir);
                    p.Pieces[indexDestination] = piecePromue;
                }
            }
            p.QuiJoue = Adversaire(p.QuiJoue);
            p.NombreCoupsJoues += 0.5f;
            return new ResultatCoup(piecePromue, enPassant, secondPion);
        }

        private static void Roque(Position p, FlagMouvementRoque roque)
        {   // Déplace le roi et la tour ; le camp perd ses deux roques
            p.IndexCaseEnPassant = 0;
            p.SansPrise++;
            switch (roque)
            {
                case FlagMouvementRoque.PetitRoqueBlanc:
                    p.PetitRoqueBlancPossible = p.GrandRoqueBlancPossible = false;
                    Deplace(p, 25, 27); Deplace(p, 28, 26);
                    break;
                case FlagMouvementRoque.GrandRoqueBlanc:
                    p.PetitRoqueBlancPossible = p.GrandRoqueBlancPossible = false;
                    Deplace(p, 25, 23); Deplace(p, 21, 24);
                    break;
                case FlagMouvementRoque.PetitRoqueNoir:
                    p.PetitRoqueNoirPossible = p.GrandRoqueNoirPossible = false;
                    Deplace(p, 95, 97); Deplace(p, 98, 96);
                    break;
                case FlagMouvementRoque.GrandRoqueNoir:
                    p.PetitRoqueNoirPossible = p.GrandRoqueNoirPossible = false;
                    Deplace(p, 95, 93); Deplace(p, 91, 94);
                    break;
            }
        }

        private static bool PrendEnPassant(Position p, int source)
        {   // Retire le pion pris en passant ; renvoie vrai si un second pion pouvait faire la même prise
            int colonne = source % 10;
            int rangee = (source / 10) - 1;
            bool secondPion = false;
            int pionPris = p.Pieces[source] == TypePiece.PionBlanc ? p.IndexCaseEnPassant - 10 : p.IndexCaseEnPassant + 10;
            if (colonne < 7)
            {
                if (rangee == 4) secondPion = p.Pieces[source + 2] == TypePiece.PionNoir;
                if (rangee == 5) secondPion = p.Pieces[source + 2] == TypePiece.PionBlanc;
            }
            if (!secondPion && colonne > 2)
            {
                if (rangee == 4) secondPion = p.Pieces[source - 2] == TypePiece.PionNoir;
                if (rangee == 5) secondPion = p.Pieces[source - 2] == TypePiece.PionBlanc;
            }
            p.Pieces[pionPris] = TypePiece.Vide;
            p.SansPrise = 0;
            p.IndexCaseEnPassant = 0;
            return secondPion;
        }

        // ═══ Notation ═══

        public static (string Coup, bool Echec) Notation(Position p, string source, string destination)
        {   // Le coup en notation algébrique française, sans "+" ni "#" ("Cgxe2", "exd5", "e8=" pour une promotion, "O-O"), et si
            // le roi adverse est en échec après le coup (la promotion n'est pas comptée : la pièce promue n'est pas connue ici)
            int indexSource = RenvoieCaseIndex120(source);
            int indexDestination = RenvoieCaseIndex120(destination);
            TypePiece piece = p.Pieces[indexSource];
            string prise = p.Pieces[indexDestination] == TypePiece.Vide ? "" : "x";
            string coup = LettrePiece(piece) + prise + destination;
            switch (piece)
            {
                case TypePiece.CavalierBlanc:
                case TypePiece.CavalierNoir:
                case TypePiece.TourBlanche:
                case TypePiece.TourNoire:
                case TypePiece.FouBlanc:
                case TypePiece.FouNoir:
                case TypePiece.ReineBlanche:
                case TypePiece.ReineNoire:
                    coup = LettrePiece(piece) + LeveeAmbiguite(p, indexSource, indexDestination) + prise + destination;
                    break;
                case TypePiece.PionBlanc:
                case TypePiece.PionNoir:
                    coup = prise == "" ? destination : source[..1] + "x" + destination;
                    if (indexDestination < 29 || indexDestination > 90)
                        coup += "=";
                    break;
                case TypePiece.RoiBlanc:
                case TypePiece.RoiNoir:
                    coup = (source + destination) switch
                    {
                        "e1g1" or "e8g8" => "O-O",
                        "e1c1" or "e8c8" => "O-O-O",
                        _ => coup
                    };
                    break;
            }
            Position copie = p.Copier();
            SimuleCoup(copie, indexSource, indexDestination);
            return (coup, RoiAttaque(copie, Adversaire(p.QuiJoue)));
        }

        private static string LettrePiece(TypePiece piece) => piece switch
        {
            TypePiece.TourBlanche or TypePiece.TourNoire => "T",
            TypePiece.CavalierBlanc or TypePiece.CavalierNoir => "C",
            TypePiece.FouBlanc or TypePiece.FouNoir => "F",
            TypePiece.ReineBlanche or TypePiece.ReineNoire => "D",
            TypePiece.RoiBlanc or TypePiece.RoiNoir => "R",
            _ => ""
        };

        private static string LeveeAmbiguite(Position p, int indexSource, int indexDestination)
        {   // Règle PGN : si d'autres pièces identiques peuvent jouer LÉGALEMENT sur la même case (une pièce clouée ne compte pas),
            // la colonne de départ ; si l'une d'elles est sur la même colonne, la rangée ; si les deux, la case entière ; "" sinon
            string source = NomCaseAlgebrique(indexSource);
            string destination = NomCaseAlgebrique(indexDestination);
            bool ambiguite = false, memeColonne = false, memeRangee = false;
            for (int i = 21; i <= 98; i++)
            {
                if (i == indexSource || p.Pieces[i] != p.Pieces[indexSource])
                    continue;
                string autre = NomCaseAlgebrique(i);
                if (!EstLegal(p, autre, destination))
                    continue;
                ambiguite = true;
                memeColonne |= autre[0] == source[0];
                memeRangee |= autre[1] == source[1];
            }
            if (!ambiguite)
                return "";
            if (!memeColonne)
                return source[..1];
            if (!memeRangee)
                return source.Substring(1, 1);
            return source;
        }

        // ═══ Matériel ═══

        public static bool MaterielInsuffisant(Position p)
        {   // Aucun mat possible : il ne reste que les rois, plus au plus un fou ou un cavalier, ou seulement des fous tous sur des
            // cases de même couleur
            int pieces = 0, cavaliers = 0, fousCasesClaires = 0, fousCasesSombres = 0;
            for (int i = 21; i <= 98; i++)
            {
                switch (p.Pieces[i])
                {
                    case TypePiece.Vide:
                    case TypePiece.Bordure:
                    case TypePiece.RoiBlanc:
                    case TypePiece.RoiNoir:
                        break;
                    case TypePiece.CavalierBlanc:
                    case TypePiece.CavalierNoir:
                        cavaliers++; pieces++;
                        break;
                    case TypePiece.FouBlanc:
                    case TypePiece.FouNoir:
                        if (Outils.EstCaseClaire(i)) fousCasesClaires++; else fousCasesSombres++;
                        pieces++;
                        break;
                    default:
                        return false;   // pion, tour ou dame : le mat reste possible
                }
            }
            return pieces <= 1 || (cavaliers == 0 && (fousCasesClaires == 0 || fousCasesSombres == 0));
        }

        public static bool PeutMater(Position p, ColorPiece camp)
        {   // Règle simplifiée des serveurs en ligne : roi seul, ou roi et un seul fou ou cavalier = pas de mat (pendule)
            int piecesLegeres = 0;
            for (int i = 21; i <= 98; i++)
            {
                if (CouleurCase(p, i) != camp)
                    continue;
                switch (p.Pieces[i])
                {
                    case TypePiece.RoiBlanc:
                    case TypePiece.RoiNoir:
                        break;
                    case TypePiece.CavalierBlanc:
                    case TypePiece.CavalierNoir:
                    case TypePiece.FouBlanc:
                    case TypePiece.FouNoir:
                        piecesLegeres++;
                        break;
                    default:
                        return true;    // pion, tour ou dame
                }
            }
            return piecesLegeres >= 2;
        }
    }
}
