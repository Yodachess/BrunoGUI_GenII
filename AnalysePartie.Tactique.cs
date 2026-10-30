// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Analyse de partie (suite de AnalysePartie.cs) : sacrifices et thèmes tactiques
//  ├─ "Sacrifice"                                         le meilleur coup sacrifie-t-il du matériel, et lequel (« !! »)
//  ├─ "DetruitLeRempart", "AttaqueDouble", "AttaqueALaDecouverte"   thèmes tactiques du coup joué
//  └─ "PiecesEnPrise", "PrisesGagnantes", "Materiel"…              calculs de matériel sur une copie de la position

using System;
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public partial class AnalyseDePartie
    {
        private bool DetruitLeRempart(int k, int indexCoup)
        {   // Le coup (un sacrifice) prend un pion de l'abri du roi adverse roqué : roi sur sa première rangée, côté roi (f, g, h) ou
            // côté dame (a, b, c), et pion sur l'une des trois colonnes autour du roi, une ou deux rangées devant lui (15. Cxg7!)
            string joue = _coups[indexCoup].Uci.Trim();
            if (joue.Length < 4)
                return false;
            Position avant = PositionDepuisFen(_positions[k].Fen);
            ColorPiece camp = _positions[k].AuTrait;
            TypePiece pionAdverse = camp == ColorPiece.Blanc ? TypePiece.PionNoir : TypePiece.PionBlanc;
            TypePiece roiAdverse = camp == ColorPiece.Blanc ? TypePiece.RoiNoir : TypePiece.RoiBlanc;
            if (avant.Pieces[RenvoieCaseIndex120(joue.Substring(2, 2))] != pionAdverse)
                return false;
            int roi = avant.Pieces.IndexOf(roiAdverse);
            if (roi < 21)
                return false;
            int colonneRoi = roi % 10, rangeeRoi = roi / 10 - 1;       // colonne 1 (a) à 8 (h), rangée 1 à 8
            int premiere = camp == ColorPiece.Blanc ? 8 : 1, sens = camp == ColorPiece.Blanc ? -1 : 1;
            if (rangeeRoi != premiere || colonneRoi is 4 or 5)
                return false;           // roi au centre (d, e) ou sorti de sa rangée : pas un roque à démolir
            // l'adversaire a vraiment roqué plus tôt (2026-10-23 : un roi allé à pied en g8, 11... Rf8 12... Rg8,
            // n'a pas de "rempart du roque") ; une partie commencée à une position ne dit pas comment le roi y est venu : il compte
            bool roque = _coups.Take(indexCoup).Any(c => !c.EstPositionDeDepart && c.EstCoupBlanc != (camp == ColorPiece.Blanc)
                                                         && c.PgnIntl.Contains("O-O"));
            if (!roque && !(_coups.Count > 0 && _coups[0].EstPositionDeDepart))
                return false;
            int colonnePion = joue[2] - 'a' + 1, rangeePion = joue[3] - '0';
            return Math.Abs(colonnePion - colonneRoi) <= 1 && (rangeePion == premiere + sens || rangeePion == premiere + 2 * sens);
        }

        private string? AttaqueDouble(int k, int indexCoup)
        {   // Fourchette / attaque double : le meilleur coup (joué) attaque au moins deux cibles (le roi par un échec, ou une pièce
            // qu'il gagnerait à prendre : plus forte que lui ou mal défendue), la variante du moteur la fait prendre une cible au coup
            // suivant de cette pièce, et le joueur y gagne au moins SacrificeMinimum points (pas un pion) APRÈS le coup : ce que le
            // coup prend lui-même ne vient pas de l'attaque double (2026-10-25 : 14. Dxh8+ Re7 15. Dxh7, une tour
            // prise puis un pion, était une « attaque double de la dame »)
            PositionAnalysee avant = _positions[k];
            string[] variante = (avant.VarianteMeilleureUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (variante.Length < 3 || variante.Any(c => c.Length < 4) || variante[0] != _coups[indexCoup].Uci.Trim())
                return null;
            string arrivee = variante[0].Substring(2, 2);
            ColorPiece camp = avant.AuTrait;
            Position p = PositionDepuisFen(avant.Fen);      // (une position à part : la variante y est jouée)
            TypePiece piece = p.Pieces[RenvoieCaseIndex120(variante[0][..2])];
            int valeur = JugementCoups.Valeur(piece);
            ChargementPartie.JoueCoupUci(p, variante[0]);
            int depart = Materiel(p, camp);
            bool echec = Regles.CampAuTraitEnEchec(p);
            // Cibles : les prises que la pièce pourrait faire si elle rejouait (le trait lui est rendu sur une copie)
            Position rejoue = AvecLeTrait(p, camp);
            List<string> cibles = [];
            foreach ((string source, string destination) in Regles.CoupsLegaux(rejoue).Where(c => c.Source == arrivee))
            {
                int cible = JugementCoups.Valeur(rejoue.Pieces[RenvoieCaseIndex120(destination)]);
                if (cible > 0 && cible - (EstDefendue(rejoue, source + destination) ? valeur : 0) > 0)
                    cibles.Add(destination);
            }
            if (cibles.Count + (echec ? 1 : 0) < 2)
                return null;
            // La variante : réponse de l'adversaire, puis la pièce prend l'une des cibles ; on suit les reprises éventuelles
            if (variante[2][..2] != arrivee || !cibles.Contains(variante[2].Substring(2, 2))
                || GainDeLaSuite(p, variante, camp) - depart < JugementCoups.SacrificeMinimum)
                return null;
            return piece switch
            {
                TypePiece.CavalierBlanc or TypePiece.CavalierNoir => Langue.T("Fourchette du cavalier"),
                TypePiece.PionBlanc or TypePiece.PionNoir => Langue.T("Fourchette du pion"),
                TypePiece.ReineBlanche or TypePiece.ReineNoire => Langue.T("Attaque double de la dame"),
                TypePiece.TourBlanche or TypePiece.TourNoire => Langue.T("Attaque double de la tour"),
                TypePiece.FouBlanc or TypePiece.FouNoir => Langue.T("Attaque double du fou"),
                _ => Langue.T("Attaque double du roi")
            };
        }

        private static int GainDeLaSuite(Position apresLeCoup, string[] variante, ColorPiece camp)
        {   // Matériel du joueur (moins celui de l'adversaire) au bout de la suite de la variante : la réponse de l'adversaire
            // (variante[1]), le coup suivant du joueur (variante[2]), puis les reprises éventuelles. La position est modifiée
            ChargementPartie.JoueCoupUci(apresLeCoup, variante[1]);
            int i = 2;
            ChargementPartie.JoueCoupUci(apresLeCoup, variante[i]);
            while (i + 1 < variante.Length && i < JugementCoups.DemiCoupsSacrifice && EstUnePrise(apresLeCoup, variante[i + 1]))
                ChargementPartie.JoueCoupUci(apresLeCoup, variante[++i]);
            int materiel = Materiel(apresLeCoup, camp);
            if (i + 1 >= variante.Length)       // variante arrêtée après une prise : le camp au trait reprend ce qu'il peut
                materiel += apresLeCoup.QuiJoue == camp ? GainImmediat(apresLeCoup) : -GainImmediat(apresLeCoup);
            return materiel;
        }

        private string? AttaqueALaDecouverte(int k, int indexCoup)
        {   // Échec double, échec à la découverte, attaque à la découverte (2026-10-26). La pièce jouée démasque
            // une pièce à longue portée du joueur (fou, tour, dame) : celle-ci attaque maintenant, à travers la case quittée, une
            // cible qu'elle n'atteignait pas.
            // - le roi adverse attaqué par deux pièces : « Échec double » ;
            // - le roi attaqué par la pièce démasquée seulement : « Échec à la découverte » ;
            // - une pièce que la pièce démasquée gagnerait à prendre : « Attaque à la découverte », seulement si la variante du moteur
            //   prend au coup suivant (la pièce démasquée sa cible, ou la pièce jouée l'une des siennes) et gagne au moins
            //   SacrificeMinimum après le coup (comme l'attaque double : ce que le coup prend lui-même ne compte pas)
            string joue = _coups[indexCoup].Uci.Trim();
            if (joue.Length < 4)
                return null;
            ColorPiece camp = _positions[k].AuTrait;
            Position p = PositionDepuisFen(_positions[k].Fen);
            int source = RenvoieCaseIndex120(joue[..2]), arrivee = RenvoieCaseIndex120(joue.Substring(2, 2));
            TypePiece roiAdverse = camp == ColorPiece.Blanc ? TypePiece.RoiNoir : TypePiece.RoiBlanc;
            // Les pièces à longue portée du joueur, autres que celle qui bouge, et les cases qu'elles attaquaient avant le coup
            List<(string Case, HashSet<string> Cibles)> longues = [];
            for (int i = 21; i <= 98; i++)
                if (i != source && Regles.CouleurCase(p, i) == camp && p.Pieces[i] is TypePiece.FouBlanc or TypePiece.FouNoir
                        or TypePiece.TourBlanche or TypePiece.TourNoire or TypePiece.ReineBlanche or TypePiece.ReineNoire)
                    longues.Add((NomCaseAlgebrique(i), CasesPrises(p, NomCaseAlgebrique(i))));
            ChargementPartie.JoueCoupUci(p, joue);
            int depart = Materiel(p, camp);
            int roi = p.Pieces.IndexOf(roiAdverse);
            if (roi < 21)
                return null;
            // Les cibles démasquées : atteintes maintenant à travers la case quittée, et pas avant
            List<(int Piece, int Cible)> demasquees = [];
            foreach ((string caseLongue, HashSet<string> avant) in longues)
            {
                int indexLongue = RenvoieCaseIndex120(caseLongue);
                foreach (string cible in CasesPrises(p, caseLongue).Except(avant))
                    if (EntreLesDeux(indexLongue, source, RenvoieCaseIndex120(cible)))
                        demasquees.Add((indexLongue, RenvoieCaseIndex120(cible)));
            }
            if (demasquees.Count == 0)
                return null;
            int attaquantsDuRoi = Regles.CasesPiecesMenacantes(p, NomCaseAlgebrique(roi)).Count;
            if (demasquees.Any(d => d.Cible == roi))
                return attaquantsDuRoi >= 2 ? Langue.T("Échec double") : Langue.T("Échec à la découverte");
            // Attaque à la découverte : une cible que la pièce démasquée gagnerait à prendre, prise par la variante au coup suivant
            string[] variante = (_positions[k].VarianteMeilleureUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (variante.Length < 3 || variante.Any(c => c.Length < 4) || variante[0] != joue)
                return null;
            Position rejoue = AvecLeTrait(p, camp);
            bool gagnante = demasquees.Any(d =>
                JugementCoups.Valeur(p.Pieces[d.Cible]) - (EstDefendue(rejoue, NomCaseAlgebrique(d.Piece) + NomCaseAlgebrique(d.Cible))
                    ? JugementCoups.Valeur(p.Pieces[d.Piece]) : 0) > 0);
            if (!gagnante)
                return null;
            string suivant = variante[2];
            bool priseDemasquee = demasquees.Any(d => suivant == NomCaseAlgebrique(d.Piece) + NomCaseAlgebrique(d.Cible));
            bool priseDeLaPieceJouee = suivant[..2] == NomCaseAlgebrique(arrivee) && CasesPrises(p, NomCaseAlgebrique(arrivee)).Contains(suivant.Substring(2, 2));
            if (!priseDemasquee && !priseDeLaPieceJouee)
                return null;
            return GainDeLaSuite(p, variante, camp) - depart >= JugementCoups.SacrificeMinimum ? Langue.T("Attaque à la découverte") : null;
        }

        private static HashSet<string> CasesPrises(Position p, string nomCase) =>
            // Les cases des pièces adverses que la pièce de cette case pourrait prendre (sans tenir compte du trait ni des clouages)
            [.. Regles.Mouvements(p, nomCase).Where(m => m.StartsWith('x')).Select(m => m.Substring(1, 2))];

        private static bool EntreLesDeux(int piece, int milieu, int cible)
        {   // La case du milieu est-elle sur la ligne droite qui va de la pièce à la cible, entre les deux (indices du tableau de 120 cases) ?
            foreach (int pas in new[] { 1, 10, 9, 11 })
                foreach (int sens in new[] { pas, -pas })
                {
                    int d = piece + sens;
                    for (int n = 0; n < 7 && SurLePlateau(d); n++, d += sens)
                    {
                        if (d == cible)
                            break;
                        if (d == milieu)
                        {   // le milieu est sur cette direction : la cible doit être plus loin, sur la même direction
                            for (int e = d + sens, m = n + 1; m < 7 && SurLePlateau(e); m++, e += sens)
                                if (e == cible)
                                    return true;
                            break;
                        }
                    }
                }
            return false;
        }

        // (les colonnes 0 et 9 du tableau de 120 cases sont la bordure : sans ce test, une ligne passerait d'une rangée à l'autre)
        private static bool SurLePlateau(int index) => index >= 21 && index <= 98 && index % 10 is not (0 or 9);

        private DetailSacrifice? Sacrifice(int k)
        {   // Le meilleur coup sacrifie du matériel (null : non), et lequel. La variante du moteur (qui commence par ce coup) est jouée sur une copie : à la
            // PREMIÈRE position calme après la réponse de l'adversaire (le demi-coup suivant de la variante n'est pas une prise, ou la
            // variante s'arrête), dans les DemiCoupsSacrifice premiers demi-coups, le joueur a au moins SacrificeMinimum points de
            // matériel de moins qu'avant son coup. Au milieu d'une suite d'échanges, le matériel ne veut rien dire (27... Fxf3 28. Fxg7
            // Fxg2 29. Rxg2 Rxg7 : à égalité au bout) ; et un sacrifice plus loin dans la variante est celui d'un AUTRE coup
            // (12. The1 b4 13. Cd5 exd5 : c'est 13. Cd5 qui sacrifie).
            // Sacrifice OFFERT (refusé dans la variante, parce que l'accepter perd) : juste après le coup, l'adversaire peut gagner au
            // moins SacrificeMinimum points en prenant (GainImmediat) ; ex : 25... Cfe4!! laisse la tour c8 au fou f5, et la variante
            // du moteur continue par 26. c3. Seulement pour une pièce NOUVELLEMENT en prise (case qui ne l'était pas
            // après le coup précédent du joueur, ou qui l'est davantage) : sinon 16. Dh4, qui laisse le cavalier g7 offert par
            // 15. Cxg7, serait un nouveau sacrifice ; avant 25... Cfe4, c'était la tour g6 qui était en prise (au fou d3), pas c8
            PositionAnalysee avant = _positions[k];
            string[] variante = (avant.VarianteMeilleureUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (variante.Length < 1 || variante[0].Length < 4)
                return null;
            ColorPiece camp = avant.AuTrait;
            Dictionary<string, int> dejaEnPrise = k > 0 ? PiecesEnPrise(PositionDepuisFen(_positions[k - 1].Fen)) : [];
            Position p = PositionDepuisFen(avant.Fen);      // (une position à part : la variante y est jouée)
            int depart = Materiel(p, camp);
            // la 1re pièce du joueur prise par l'adversaire dans la variante ; Qualite : une tour prise par une pièce légère sur une
            // case défendue (elle sera reprise, même après un coup intermédiaire : 25... Cfe4 26. Fxc8 b3 … Fxc8)
            (TypePiece Piece, string Case, bool Qualite)? perdue = null;
            DetailSacrifice? offert = null;     // pièce laissée en prise ; accepté si la variante la fait prendre, sinon offert
            HashSet<string> jouesEnsuite = [];  // cases d'arrivée des coups suivants du joueur dans la variante
            for (int i = 0; i < Math.Min(variante.Length, JugementCoups.DemiCoupsSacrifice + 1); i++)
            {
                if (variante[i].Length < 4)
                    return null;
                string arrivee = variante[i].Substring(2, 2);
                if (i % 2 == 0 && i >= 2)
                    jouesEnsuite.Add(arrivee);
                if (i % 2 == 1 && perdue == null && p.Pieces[RenvoieCaseIndex120(arrivee)] is TypePiece prise && prise != TypePiece.Vide)
                {
                    // la pièce prise y a été amenée par un coup suivant du joueur : c'est le sacrifice de CE coup-là (12. Fh6+ Rg8
                    // 13. Fxf7+ Rxf7 14. De6# : le fou c4 est sacrifié par 13. Fxf7+, pas par 12. Fh6+, qui n'est pas prenable)
                    if (jouesEnsuite.Contains(arrivee))
                        return offert;
                    bool legere = JugementCoups.Valeur(p.Pieces[RenvoieCaseIndex120(variante[i][..2])]) == 3;
                    perdue = (prise, arrivee, JugementCoups.Valeur(prise) == 5 && legere && EstDefendue(p, variante[i]));
                }
                ChargementPartie.JoueCoupUci(p, variante[i]);
                if (i == 0 && PiecesEnPrise(p).Where(e => e.Value > dejaEnPrise.GetValueOrDefault(e.Key)).OrderByDescending(e => e.Value)
                                              .FirstOrDefault() is { Value: > 0 } offerte
                    && Materiel(p, camp) - offerte.Value <= depart - JugementCoups.SacrificeMinimum)
                {   // offert : la pièce laissée en prise, ou la qualité si c'est une tour défendue que prendrait une pièce légère
                    TypePiece piece = p.Pieces[RenvoieCaseIndex120(offerte.Key)];
                    offert = new DetailSacrifice(piece, offerte.Key, true, JugementCoups.Valeur(piece) == 5 && offerte.Value < 5);
                }
                bool fin = i + 1 >= variante.Length;
                if (i >= 1 && (fin || !EstUnePrise(p, variante[i + 1])))
                {   // Variante arrêtée au milieu d'un échange (25... Tfc8 26. Txc8+ et rien après) : le camp au trait reprend ce qu'il peut
                    int materiel = Materiel(p, camp);
                    if (fin)
                        materiel += p.QuiJoue == camp ? GainImmediat(p) : -GainImmediat(p);
                    int perte = depart - materiel;
                    if (perte < JugementCoups.SacrificeMinimum)
                        return offert;
                    TypePiece pieceDonnee = perdue?.Piece ?? TypePiece.Vide;
                    return new DetailSacrifice(pieceDonnee, perdue?.Case ?? "", false,
                                               JugementCoups.Valeur(pieceDonnee) == 5 && (perte < 5 || perdue?.Qualite == true));
                }
            }
            return offert;
        }

        private static int GainImmediat(Position position) => PiecesEnPrise(position).Values.DefaultIfEmpty(0).Max();

        private static Dictionary<string, int> PiecesEnPrise(Position position)
        {   // Pour chaque case où le camp au trait gagne du matériel en prenant tout de suite, ce qu'il gagne au mieux, reprise
            // comprise (pièce défendue : il perd celle qui a pris). Calcul simple (pas de suite d'échanges au-delà de la reprise) :
            // il suffit à voir une pièce laissée en prise
            Dictionary<string, int> enPrise = [];
            foreach ((string _, string destination, int gain) in PrisesGagnantes(position))
                if (gain > enPrise.GetValueOrDefault(destination))
                    enPrise[destination] = gain;
            return enPrise;
        }

        private static List<(string Source, string Destination, int Gain)> PrisesGagnantes(Position position)
        {   // Les prises du camp au trait qui gagnent du matériel à première vue (voir PiecesEnPrise)
            List<(string, string, int)> prises = [];
            foreach ((string source, string destination) in Regles.CoupsLegaux(position))
            {
                int cible = JugementCoups.Valeur(position.Pieces[RenvoieCaseIndex120(destination)]);
                if (cible == 0)
                    continue;
                int preneur = JugementCoups.Valeur(position.Pieces[RenvoieCaseIndex120(source)]);
                int gain = cible - (EstDefendue(position, source + destination) ? preneur : 0);
                if (gain > 0)
                    prises.Add((source, destination, gain));
            }
            return prises;
        }

        private static bool EstUnePrise(Position position, string coupUci)
        {   // La case d'arrivée est occupée, ou un pion change de colonne (prise en passant)
            if (coupUci.Length < 4)
                return false;
            TypePiece piece = position.Pieces[RenvoieCaseIndex120(coupUci[..2])];
            return position.Pieces[RenvoieCaseIndex120(coupUci.Substring(2, 2))] != TypePiece.Vide
                || (piece is TypePiece.PionBlanc or TypePiece.PionNoir && coupUci[0] != coupUci[2]);
        }

        private static bool DonneEchec(Position position, string coupUci)
        {   // Le coup (joué sur une copie) met-il le roi adverse en échec ?
            Position copie = position.Copier();
            ChargementPartie.JoueCoupUci(copie, coupUci);
            return Regles.CampAuTraitEnEchec(copie);
        }

        private static int Materiel(Position position, ColorPiece camp)
        {   // Matériel du camp moins celui de l'adversaire ; pièces blanches : valeurs impaires
            int bilan = 0;
            for (int i = 21; i <= 98; i++)
            {
                TypePiece piece = position.Pieces[i];
                if (piece is TypePiece.Vide or TypePiece.Bordure)
                    continue;
                bool blanche = (int)piece % 2 == 1;
                bilan += (blanche == (camp == ColorPiece.Blanc) ? 1 : -1) * JugementCoups.Valeur(piece);
            }
            return bilan;
        }
    }
}
