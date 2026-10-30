// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Analyse de partie (suite de AnalysePartie.cs) : les bons coups
//  ├─ "BonCoup"                                   « ! » ou « !! » (seul bon coup, sacrifice correct, coup difficile)
//  ├─ "DifficultesAMesurer", "EnregistreDifficulte"   profondeur où un moteur sans mémoire trouve le coup
//  ├─ "SeulBonCoup", "EstFuite", "EstReprise"       ce qui exclut un « ! » (coup forcé, fuite évidente, reprise)
//  └─ "MatManque"                                 « Mat en 8 manqué »

using System;
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public partial class AnalyseDePartie
    {
        private string? MatManque(int k, int indexCoup, double sens, bool meilleurJoue)
        {   // "Mat en 8 manqué" (étape B) : le joueur avait un mat forcé avant son coup, et ne l'a plus après (s'il le
            // garde, même plus long, rien). Le coup à jouer est déjà dans la 2e ligne ("— meilleur : Dh4xg5 (M8)") et dans la variante
            // du PGN ; laisser un mat à l'adversaire est dit par l'appréciation ("Mat en 8 pour les Blancs")
            if (meilleurJoue || EstCoupDeBibliotheque(indexCoup) || _positions[k].Evaluation?.MatEn is not int mat || sens * mat <= 0)
                return null;
            PositionAnalysee apres = _positions[k + 1];
            bool matGarde = apres.Evaluation?.MatEn is int matApres ? sens * matApres > 0 : apres.ChancesFinDePartie is double fin && sens * fin > 0;
            return matGarde ? null : Langue.T("Mat en {0} manqué", Math.Abs(mat));
        }

        private (string Annotation, DetailSacrifice? Sacrifice) BonCoup(int k, int indexCoup, double sens)
        {   // Le meilleur coup a été joué : "!", "!!" ou rien, et le sacrifice qui lui vaut son annotation (commentaire de l'analyse).
            // Difficulté inconnue (pas de 2e moteur) : "!" au seul bon coup (l'alternative est une erreur), "!!" s'il sacrifie.
            // Difficulté mesurée (octobre 2026) : un sacrifice correct a "!", et "!!" s'il était difficile à trouver
            // (13. Cd5!! : une pièce, trouvé à la profondeur 12 ; il n'est pas "le seul bon coup", une alternative restait à +0.11) ;
            // sans sacrifice, "!" seulement au seul bon coup difficile à trouver (21. Th3!) ; un seul bon coup naturel n'a rien
            // (29. gxf3, 15... O-O, 47. Rd4)
            if (_positions[k].ProfondeurDecouverte is not int profondeur)
            {
                if (!SeulBonCoup(k, indexCoup, sens))
                    return ("", null);
                DetailSacrifice? sacrificeSeul = Sacrifice(k);
                return (sacrificeSeul != null ? "!!" : "!", sacrificeSeul);
            }
            bool difficile = profondeur >= JugementCoups.ProfondeurCoupDifficile;
            if (SacrificeCorrect(k, indexCoup, sens) is DetailSacrifice sacrifice)
            {   // (un mat forcé était déjà là : lâcher du matériel en route n'est remarquable que si c'était difficile, 34... Rg8)
                bool matDejaForce = _positions[k].Evaluation?.MatEn is int mat && sens * mat > 0;
                return difficile ? ("!!", sacrifice) : matDejaForce ? ("", null) : ("!", sacrifice);
            }
            return (difficile && SeulBonCoup(k, indexCoup, sens) ? "!" : "", null);
        }

        private DetailSacrifice? SacrificeCorrect(int k, int indexCoup, double sens)
        {   // Le meilleur coup (déjà vérifié) sacrifie, et le joueur n'est pas perdu après lui. Dans une position déjà gagnée, donner du
            // matériel pour conclure n'est remarquable que si c'est le seul coup qui gagne (25... Cfe4!! après la gaffe 25. Ff5??), si le
            // sacrifice ouvre le roque adverse ou si la suite mate (la combinaison qui conclut, 2026-10-28 :
            // 21. Fxh6! de Carlsen–Ernst, que ChessBase annote et que nous taisions ; rendre du matériel pour simplifier reste muet)
            // (ni quand l'adversaire garde un avantage décisif, -2.00 ou pire : la pièce lâchée par un camp déjà perdant n'a rien de
            // brillant, 15... Cd5 de Beliavsky–Larsen à -3.32 laissait une tour déjà perdue, faux « !! » du 2026-10-28)
            if (_positions[k + 1].Chances is not double apres
                || sens * apres < -JugementCoups.ChancesDeGain(new Evaluation(Evaluation.SeuilGain, null)))
                return null;
            bool conclut = DetruitLeRempart(k, indexCoup) || (_positions[k + 1].Evaluation?.MatEn is int mat && sens * mat > 0);
            if (sens * apres > JugementCoups.LimitePartieDecidee && !conclut && !SeulBonCoup(k, indexCoup, sens))
                return null;
            return Sacrifice(k);
        }

        public IEnumerable<(int IndexPosition, string Fen, string CoupUci)> DifficultesAMesurer()
        {   // Meilleurs coups joués, seuls bons coups ou sacrifices, dont on ne sait pas encore s'ils étaient difficiles à trouver :
            // l'interface les fait mesurer par un 2e moteur (EnregistreDifficulte). Positions avant et après déjà analysées ; jamais un
            // coup de la bibliothèque
            for (int k = 0; k < _indexCoups.Count; k++)
            {
                PositionAnalysee avant = _positions[k];
                int indexCoup = _indexCoups[k];
                if (avant.DifficulteMesuree || avant.MeilleurCoupUci == null || EstCoupDeBibliotheque(indexCoup)
                    || avant.MeilleurCoupUci != _coups[indexCoup].Uci.Trim() || _positions[k + 1].Chances == null)
                    continue;
                double sens = avant.AuTrait == ColorPiece.Blanc ? 1 : -1;
                if (SeulBonCoup(k, indexCoup, sens) || SacrificeCorrect(k, indexCoup, sens) != null)
                    yield return (k, avant.Fen, avant.MeilleurCoupUci);
            }
        }

        public void EnregistreDifficulte(int indexPosition, int? profondeurDecouverte)
        {   // Résultat de la mesure (null : échec, la règle sans difficulté s'applique) ; une position n'est mesurée qu'une fois
            _positions[indexPosition].ProfondeurDecouverte = profondeurDecouverte;
            _positions[indexPosition].DifficulteMesuree = true;
        }

        private bool SeulBonCoup(int k, int indexCoup, double sens)
        {   // "!" (bon coup) : le meilleur coup a été joué ET c'était le seul bon : la 2e variante du moteur aurait fait
            // perdre au moins JugementCoups.SeuilSeulBonCoup de chances de gain (une erreur). Exclus : un coup
            // forcé (un seul coup légal) et une reprise immédiate (on reprend sur la case où l'adversaire vient de prendre : rien de remarquable)
            // Pas non plus quand le joueur reste perdu après le coup (la seule défense qui perd moins dans une position perdue) : ses
            // chances sous -JugementCoups.LimitePartieDecidee. Le seul coup qui GARDE une partie gagnée a son "!" (25... Cfe4 après
            // 25. Ff5?? : -3.82, l'alternative ne donnait que -0.56 ; avant octobre 2026, la limite valait dans les deux sens)
            PositionAnalysee avant = _positions[k];
            if (avant.EvaluationSeconde is not Evaluation seconde || avant.Chances is not double chances || avant.NombreCoupsLegaux <= 1)
                return false;
            if (_positions[k + 1].Chances is not double apres || sens * apres < -JugementCoups.LimitePartieDecidee)
                return false;
            // (une prise sur la case de l'adversaire qui SACRIFIE n'est pas une reprise : 21... gxh6 22. Txh6+!!, la tour se donne)
            if (k > 0 && EstReprise(k, indexCoup) && Sacrifice(k) == null)
                return false;
            if (EstFuite(k, indexCoup))
                return false;
            return sens * (chances - JugementCoups.ChancesDeGain(seconde)) >= JugementCoups.SeuilSeulBonCoup;
        }

        private bool EstFuite(int k, int indexCoup)
        {   // Retraite évidente, pas un "!" : la pièce jouée (plus qu'un pion) était attaquée par une pièce adverse de moindre valeur,
            // elle n'avait qu'UNE case sûre, et c'est là qu'elle va (ex : Caro-Kann, 8. h5 Fh7 : le fou attaqué par le pion n'a que h7).
            // Avec plusieurs cases sûres, trouver la seule bonne est un vrai choix, qui garde son "!" (ex : Cg6) ;
            // aller sur une case attaquée n'est pas une fuite (c'est un sacrifice)
            string joue = _coups[indexCoup].Uci.Trim();
            if (joue.Length < 4)
                return false;
            string depart = joue[..2], arrivee = joue.Substring(2, 2);
            Position avant = PositionDepuisFen(_positions[k].Fen);
            int valeur = JugementCoups.Valeur(avant.Pieces[RenvoieCaseIndex120(depart)]);
            if (valeur <= 1)
                return false;       // pion (ou roi, valeur 0) : jamais une fuite au sens de cette règle
            // attaques adverses sur la case de départ : coups légaux de l'adversaire, en lui donnant le trait sur une copie
            Position adverse = AvecLeTrait(avant, Adversaire(avant.QuiJoue));
            bool menacee = Regles.CoupsLegaux(adverse).Any(c => c.Destination == depart && JugementCoups.Valeur(adverse.Pieces[RenvoieCaseIndex120(c.Source)]) < valeur);
            if (!menacee)
                return false;
            List<string> casesSures = [.. Regles.CoupsLegaux(avant).Where(c => c.Source == depart).Select(c => c.Destination)
                                          .Where(destination => CaseSure(avant, depart, destination, valeur))];
            return casesSures.Count == 1 && casesSures[0] == arrivee;
        }

        private static bool CaseSure(Position avant, string depart, string destination, int valeur)
        {   // Après le coup (sur une copie), la pièce n'y est attaquée par aucune pièce adverse de moindre valeur, et, si elle y est
            // attaquée, elle y est défendue (une pièce de même valeur qui la prend sera reprise : simple échange)
            Position p = avant.Copier();
            int indexDestination = RenvoieCaseIndex120(destination);
            ColorPiece camp = p.QuiJoue;
            Regles.SimuleCoup(p, RenvoieCaseIndex120(depart), indexDestination);
            p.QuiJoue = Adversaire(camp);
            List<int> attaquants = [.. Regles.CoupsLegaux(p).Where(c => c.Destination == destination)
                                                         .Select(c => JugementCoups.Valeur(p.Pieces[RenvoieCaseIndex120(c.Source)]))];
            if (attaquants.Any(v => v < valeur))
                return false;
            if (attaquants.Count == 0)
                return true;
            // Défendue ? On met un pion adverse à sa place : une pièce du camp qui peut le prendre défend la case
            p.Pieces[indexDestination] = camp == ColorPiece.Blanc ? TypePiece.PionNoir : TypePiece.PionBlanc;
            p.QuiJoue = camp;
            return Regles.CoupsLegaux(p).Any(c => c.Destination == destination);
        }

        private static Position AvecLeTrait(Position position, ColorPiece camp)
        {   // Une copie de la position, ce camp au trait (pour voir ce qu'il pourrait jouer)
            Position copie = position.Copier();
            copie.QuiJoue = camp;
            return copie;
        }

        private static bool EstDefendue(Position position, string prise)
        {   // Après cette prise du camp au trait (sur une copie), l'adversaire peut-il reprendre sur la case d'arrivée ?
            Position copie = position.Copier();
            ChargementPartie.JoueCoupUci(copie, prise);
            string arrivee = prise.Substring(2, 2);
            return Regles.CoupsLegaux(copie).Any(reprise => reprise.Destination == arrivee);
        }

        private bool EstReprise(int k, int indexCoup)
        {   // Le coup joué arrive sur la case où l'adversaire vient de prendre une pièce (ex : 3... Fxe4 4. Dxe4)
            string joue = _coups[indexCoup].Uci.Trim(), precedent = _coups[_indexCoups[k - 1]].Uci.Trim();
            if (joue.Length < 4 || precedent.Length < 4 || joue.Substring(2, 2) != precedent.Substring(2, 2))
                return false;
            List<TypePiece> piecesAvantPrecedent = PositionDepuisFen(_positions[k - 1].Fen).Pieces;     // avant le coup de l'adversaire
            return piecesAvantPrecedent[RenvoieCaseIndex120(precedent.Substring(2, 2))] != TypePiece.Vide;
        }

        private static string SansSymboles(string coup) => coup.TrimEnd('+', '#', '!', '?');
    }
}
