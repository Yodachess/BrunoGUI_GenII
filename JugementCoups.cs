// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Seuils et calculs de l'analyse de partie, sans interface graphique ni moteur (voir AnalysePartie.cs)
// └─ Classe "JugementCoups" : chances de gain d'une évaluation, perte d'un coup, annotation ?! ? ?? (seuils de Lichess),
//                             appréciation, type de finale, précision ; seuils du « ! », du sacrifice, des pièges et des menaces

using System;
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public static class JugementCoups
    {
        // Perte de chances de gain (de -1 à +1) à partir de laquelle un coup est annoté (seuils de Lichess)
        public const double SeuilImprecision = 0.1, SeuilErreur = 0.2, SeuilGaffe = 0.3;
        // "!" : le meilleur coup était le seul bon, la meilleure alternative (2e variante du moteur) aurait été au moins une erreur ;
        // et le joueur n'est pas perdu après le coup (ses chances au-dessus de -0,6, environ -3,8 pions : choix B, limité au
        // camp perdu en octobre 2026).
        // Quand on sait si le coup était difficile à trouver (ProfondeurDecouverte), "!" va au seul bon coup difficile et au sacrifice
        // correct, jamais au coup naturel (reprise, roque...) : voir AnalyseDePartie.BonCoup
        public const double SeuilSeulBonCoup = SeuilErreur, LimitePartieDecidee = 0.6;
        // "!!" : un "!" qui sacrifie du matériel : dans la variante du moteur, après une réponse de l'adversaire, le joueur a au
        // moins 2 points de matériel de moins qu'avant son coup (pion 1, cavalier et fou 3, tour 5, dame 9 : une pièce contre un pion,
        // la qualité ; un échange ou un pion ne suffisent pas), compté seulement une fois les prises terminées (pas au milieu
        // d'une suite d'échanges : 27... Fxf3 28. Fxg7 Fxg2 29. Rxg2 Rxg7 n'est pas un sacrifice)
        public const int SacrificeMinimum = 2, DemiCoupsSacrifice = 6;
        // Coup difficile à trouver : un 2e moteur (table de hachage vide, 1 thread) cherche jusqu'à ProfondeurMesureDifficulte ;
        // le coup est "difficile" s'il ne devient durablement son choix qu'à partir de ProfondeurCoupDifficile (mesures sur les
        // parties de référence : 13. Cd5 du sacrifice à 12, 31. a4 à 10 ; reprises, roques et coups évidents de 1 à 7)
        public const int ProfondeurCoupDifficile = 9, ProfondeurMesureDifficulte = 14;
        // Piège : la prise tentante non jouée est cherchée par le même 2e moteur, jusqu'à cette profondeur
        public const int ProfondeurPiege = 16;
        // ... c'est un piège si elle perd une erreur en chances de gain, ou au moins 1,5 pion ; pas cherché si le joueur est perdu de 6 pions
        public const int PiegePerteMinimum = 150, PiegeJoueurPerdu = 600;     // en centipions
        // Menace (coup nul) : ce que le joueur jouerait s'il pouvait rejouer, cherché par le 2e moteur jusqu'à cette profondeur ; c'est
        // une menace si elle gagne une gaffe en chances de gain (SeuilGaffe) ou au moins MenaceGainMinimum, ou si elle mate ; pas
        // cherchée dans une partie décidée (PiegeJoueurPerdu, d'un côté ou de l'autre). Avec les seuils des pièges (une erreur,
        // 1,5 pion), il y avait une menace presque à chaque coup (un pion attaqué, une poussée) : trop bavard
        public const int ProfondeurMenace = 14;
        // Profondeurs demandées au moteur principal (AnalyseDePartie.ProfondeurVisee), la durée réglée servant de plafond
        public const int ProfondeurAnalyse = 20, ProfondeurTheorie = 12, ProfondeurApprofondissement = 24;
        public const int MenaceGainMinimum = 200;      // en centipions : environ une pièce (un pion menacé n'est pas commenté)
        public const int MenaceCalmeMinimum = 300;     // en centipions : menace qui n'est ni une prise ni un échec (avec une gaffe en chances)

        public static int? ProfondeurDecouverte(IReadOnlyList<(int Profondeur, string Coup)> meilleursCoups, string coup)
        {   // Profondeur à partir de laquelle le coup est le choix du moteur à TOUTES les profondeurs suivantes (meilleur coup à
            // chaque profondeur, dans l'ordre) ; int.MaxValue : pas son choix à la dernière ; null : aucune mesure
            if (meilleursCoups.Count == 0)
                return null;
            int? depuis = null;
            foreach ((int profondeur, string choix) in meilleursCoups)
                depuis = choix == coup ? depuis ?? profondeur : null;
            return depuis ?? int.MaxValue;
        }

        public static int Valeur(TypePiece piece) => piece switch
        {
            TypePiece.PionBlanc or TypePiece.PionNoir => 1,
            TypePiece.CavalierBlanc or TypePiece.CavalierNoir or TypePiece.FouBlanc or TypePiece.FouNoir => 3,
            TypePiece.TourBlanche or TypePiece.TourNoire => 5,
            TypePiece.ReineBlanche or TypePiece.ReineNoire => 9,
            _ => 0
        };

        public static double ChancesDeGain(Evaluation evaluation)
        {   // Chances de gain du point de vue des Blancs, de -1 (gain noir) à +1 (gain blanc), formule de Lichess :
            // perdre un pion à 0.00 compte beaucoup plus que dans une position déjà gagnée à +8
            if (evaluation.MatEn is int mat)
                return mat >= 0 ? 1 : -1;
            int cp = Math.Clamp(evaluation.Centipions ?? 0, -1000, 1000);
            return 2 / (1 + Math.Exp(-0.00368208 * cp)) - 1;
        }

        public static string Annotation(double perte) => perte switch
        {
            >= SeuilGaffe => "??",
            >= SeuilErreur => "?",
            >= SeuilImprecision => "?!",
            _ => ""
        };

        // Appréciation commentée par l'analyse (étape B) : seulement quand elle change de niveau, sur 5 niveaux de
        // -2 à +2 : équilibré (moins de ±0.70, léger avantage compris), mieux (± / ∓), avantage décisif (+- / -+ ou mat). Marge
        // autour des seuils (MargeAppreciation) : une évaluation qui hésite entre 0.65 et 0.75 n'est pas commentée à chaque coup
        public const int MargeAppreciation = 15;    // en centipions : entrer dans un niveau au seuil + 15, en sortir au seuil - 15

        public static int NiveauAppreciation(Evaluation evaluation, int actuel)
        {   // Niveau après le coup, de -3 (mat forcé pour les Noirs) à +3, compte tenu du niveau actuel (marge, voir MargeAppreciation).
            // Le mat forcé est un niveau à part (2026-10-28, ChessBase écrit « Les Blancs font mat ») : annoncé quand il
            // apparaît, puis gardé tant que l'avantage reste décisif (le moteur perd parfois le mat de vue, « #29 » puis « 15.62 » :
            // sans cela, « Gain Blanc » et « Mat en … » alterneraient)
            if (evaluation.MatEn is int mat)
                return mat > 0 ? 3 : -3;
            int cp = evaluation.Centipions ?? 0, signe = Math.Sign(cp), valeur = Math.Abs(cp);
            int Niveau(int marge) => valeur >= Evaluation.SeuilGain + marge ? 2 : valeur >= Evaluation.SeuilAvantage + marge ? 1 : 0;
            int pourMonter = Niveau(MargeAppreciation), pourRester = Niveau(-MargeAppreciation);
            if (actuel != 0 && Math.Sign(actuel) != signe)
                return signe * pourMonter;          // l'avantage change de camp
            if (Math.Abs(actuel) == 3 && pourRester == 2)
                return actuel;                      // mat annoncé, avantage toujours décisif
            int niveau = Math.Abs(actuel);
            return signe * (pourMonter > niveau ? pourMonter : pourRester < niveau ? pourRester : niveau);
        }

        public static string TexteAppreciation(int niveau, Evaluation evaluation) => niveau switch
        {   // Les appréciations de base, courtes (2026-10-21 : plutôt que "Les Blancs sont mieux")
            // ("Gain {0}" et non "Gain Blanc", déjà traduit "White wins" pour le bouton de résultat : ici, la partie n'est pas finie)
            >= 2 => evaluation.MatEn is int mat ? Langue.T("Mat en {0} pour les Blancs", mat) : Langue.T("Gain {0}", NomCouleur(ColorPiece.Blanc)),
            1 => Langue.T("Avantage {0}", NomCouleur(ColorPiece.Blanc)),
            -1 => Langue.T("Avantage {0}", NomCouleur(ColorPiece.Noir)),
            <= -2 => evaluation.MatEn is int matNoir ? Langue.T("Mat en {0} pour les Noirs", -matNoir) : Langue.T("Gain {0}", NomCouleur(ColorPiece.Noir)),
            _ => Langue.T("Égal")
        };

        public static string? TypeDeFinale(Position position)
        {   // Nom de la finale de la position (null : pas une finale) : sans dames, ou dame contre dame seules, et au plus une tour et
            // une pièce légère par camp (matériel hors pions jusqu'à 8). Finale de pions, de tours, de fous (de couleurs opposées),
            // de cavaliers, cavalier contre fou, tour contre pièce légère ; sinon "Finale"
            List<(TypePiece Piece, int Case)>[] camps = [[], []];   // [0] : Blancs, [1] : Noirs ; pièces hors rois et pions
            for (int i = 21; i <= 98; i++)
            {
                TypePiece piece = position.Pieces[i];
                if (piece is TypePiece.Vide or TypePiece.Bordure or TypePiece.RoiBlanc or TypePiece.RoiNoir or TypePiece.PionBlanc or TypePiece.PionNoir)
                    continue;
                camps[(int)piece % 2 == 1 ? 0 : 1].Add((piece, i));     // (pièces blanches : valeurs impaires)
            }
            int[] materiel = [camps[0].Sum(p => Valeur(p.Piece)), camps[1].Sum(p => Valeur(p.Piece))];
            string Signature(int camp) => string.Concat(camps[camp].Select(p => Valeur(p.Piece) switch { 9 => "D", 5 => "T", _ => p.Piece is TypePiece.FouBlanc or TypePiece.FouNoir ? "F" : "C" })
                                                                   .OrderBy(c => c));
            string blancs = Signature(0), noirs = Signature(1);
            if (blancs == "D" && noirs == "D")
                return Langue.T("Finale de dames");
            if (blancs.Contains('D') || noirs.Contains('D') || materiel[0] > 8 || materiel[1] > 8)
                return null;
            string[] paire = [blancs, noirs];
            Array.Sort(paire, StringComparer.Ordinal);
            return (paire[0], paire[1]) switch
            {
                ("", "") => Langue.T("Finale de pions"),
                ("T", "T") => Langue.T("Finale de tours"),
                ("C", "C") => Langue.T("Finale de cavaliers"),
                ("F", "F") => CouleurCase(camps[0][0].Case) == CouleurCase(camps[1][0].Case)
                              ? Langue.T("Finale de fous") : Langue.T("Finale de fous de couleurs opposées"),
                ("C", "F") => Langue.T("Finale cavalier contre fou"),
                ("F", "T") => Langue.T("Finale tour contre fou"),
                ("C", "T") => Langue.T("Finale tour contre cavalier"),
                _ => Langue.T("Finale")
            };
            static int CouleurCase(int index) => (index % 10 + index / 10) % 2;     // (cases claires et foncées du tableau 120 cases)
        }

        public static double Precision(double chancesAvant, double chancesApres)
        {   // Précision d'un coup en % (formule de Lichess), à partir des chances de gain du camp qui joue avant et après le coup :
            // 100 % s'il ne perd rien, environ 60 % pour une gaffe dans une position égale
            double gainAvant = 50 + 50 * chancesAvant, gainApres = 50 + 50 * chancesApres;     // en %
            return Math.Clamp(103.1668 * Math.Exp(-0.04354 * (gainAvant - gainApres)) - 3.1669, 0, 100);
        }
    }
}
