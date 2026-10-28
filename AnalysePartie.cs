// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Analyse de toute une partie par le moteur, sans interface graphique (testée dans Tests/Program.cs)
//  ├─ Classe "JugementCoups"    : chances de gain d'une évaluation, perte d'un coup, annotation ?! ? ?? (seuils de Lichess)
//  ├─ Classe "PositionAnalysee" : une position de la partie, son évaluation et le meilleur coup du moteur
//  ├─ Record "JugementCoup"     : ce que le moteur pense d'un coup joué
//  └─ Classe "AnalyseDePartie"  : les positions à faire analyser une par une, puis le jugement de chaque coup et le bilan
//              ├─ "PositionSuivante"   prochaine position à demander au moteur, de la FIN vers le début (null : analyse finie)
//              ├─ "Enregistre"         résultat du moteur pour une position (sa meilleure variante)
//              ├─ "Jugement"           le coup n° i de ListeCoups : perte, précision, annotation proposée, meilleur coup, variante
//              ├─ "NotationLongue"     un coup UCI avec sa case de départ ("Dd8-d7")
//              ├─ "Bilan"              imprécisions, erreurs, gaffes et précision d'un camp
//              └─ "CoupCritique"       le coup où la partie a basculé (la plus grosse perte, au moins une erreur)
// Avec "approfondir", un 2e passage revoit plus longtemps les positions avant et après chaque coup douteux (EnApprofondissement).
// L'interface demande chaque position au moteur (PiloteMoteur.DemanderAnalyse) et enregistre sa meilleure variante.

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
        // et le joueur n'est pas perdu après le coup (ses chances au-dessus de -0,6, environ -3,8 pions : choix B de Bruno, limité au
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
        // parties de Bruno : 13. Cd5 du sacrifice à 12, 31. a4 à 10 ; reprises, roques et coups évidents de 1 à 7)
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

        // Appréciation commentée par l'analyse (étape B, choix de Bruno) : seulement quand elle change de niveau, sur 5 niveaux de
        // -2 à +2 : équilibré (moins de ±0.70, léger avantage compris), mieux (± / ∓), avantage décisif (+- / -+ ou mat). Marge
        // autour des seuils (MargeAppreciation) : une évaluation qui hésite entre 0.65 et 0.75 n'est pas commentée à chaque coup
        public const int MargeAppreciation = 15;    // en centipions : entrer dans un niveau au seuil + 15, en sortir au seuil - 15

        public static int NiveauAppreciation(Evaluation evaluation, int actuel)
        {   // Niveau après le coup, de -2 (avantage décisif noir) à +2, compte tenu du niveau actuel (marge, voir MargeAppreciation)
            if (evaluation.MatEn is int mat)
                return mat > 0 ? 2 : -2;
            int cp = evaluation.Centipions ?? 0, signe = Math.Sign(cp), valeur = Math.Abs(cp);
            int Niveau(int marge) => valeur >= Evaluation.SeuilGain + marge ? 2 : valeur >= Evaluation.SeuilAvantage + marge ? 1 : 0;
            int pourMonter = Niveau(MargeAppreciation), pourRester = Niveau(-MargeAppreciation);
            if (actuel != 0 && Math.Sign(actuel) != signe)
                return signe * pourMonter;          // l'avantage change de camp
            int niveau = Math.Abs(actuel);
            return signe * (pourMonter > niveau ? pourMonter : pourRester < niveau ? pourRester : niveau);
        }

        public static string TexteAppreciation(int niveau, Evaluation evaluation) => niveau switch
        {   // Les appréciations de base, courtes (choix de Bruno, 2026-10-21 : plutôt que "Les Blancs sont mieux")
            // ("Gain {0}" et non "Gain Blanc", déjà traduit "White wins" pour le bouton de résultat : ici, la partie n'est pas finie)
            2 => evaluation.MatEn is int mat ? Langue.T("Mat en {0} pour les Blancs", mat) : Langue.T("Gain {0}", NomCouleur(ColorPiece.Blanc)),
            1 => Langue.T("Avantage {0}", NomCouleur(ColorPiece.Blanc)),
            -1 => Langue.T("Avantage {0}", NomCouleur(ColorPiece.Noir)),
            -2 => evaluation.MatEn is int matNoir ? Langue.T("Mat en {0} pour les Noirs", -matNoir) : Langue.T("Gain {0}", NomCouleur(ColorPiece.Noir)),
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

    public sealed class PositionAnalysee
    {
        public required string Fen { get; init; }
        public ColorPiece AuTrait { get; init; }
        public double? ChancesFinDePartie { get; init; }    // mat ou pat : +1, -1 ou 0, sans demander au moteur
        public Evaluation? Evaluation { get; set; }         // du point de vue des Blancs (null : pas encore analysée)
        public string? MeilleurCoup { get; set; }           // premier coup de la meilleure variante, en notation française ("Cf3")
        public string? MeilleurCoupUci { get; set; }        // le même au format UCI ("g1f3")
        public string? MeilleurCoupLong { get; set; }       // le même avec sa case de départ ("Cg1-f3")
        public string? VarianteMeilleure { get; set; }      // la meilleure variante en notation française ("12. Cf3 Fe7 13. ...")
        public string? VarianteMeilleureUci { get; set; }   // la même au format UCI ("g1f3 f8e7 ...") : sert à voir un sacrifice ("!!")
        public Evaluation? EvaluationSeconde { get; set; }  // évaluation de la 2e variante du moteur (la meilleure alternative), null : inconnue
        public int NombreCoupsLegaux { get; init; }          // 1 : coup forcé (jamais "!")
        // Coup JOUÉ depuis cette position : profondeur à partir de laquelle un moteur sans mémoire le choisit (JugementCoups.
        // ProfondeurDecouverte) ; mesurée seulement pour un seul bon coup (DifficultesAMesurer) ; null : inconnue (règle sans difficulté)
        public int? ProfondeurDecouverte { get; set; }
        public bool DifficulteMesuree { get; set; }
        // Piège (analyse complète) : une prise tentante, pas jouée, qui perdait ("La tour g6 est imprenable : 22. Fxg6? fxg6 …")
        public bool PiegeCherche { get; set; }
        public string? TextePiege { get; set; }
        public string? CasePiege { get; set; }     // case de la pièce imprenable (annoncée seulement la 1re fois : AppliqueAuxCoups)
        // Menace (analyse complète) du joueur qui vient de jouer le coup menant à cette position : son coup suivant si l'adversaire
        // ne faisait rien (coup nul), "15. Cxf7" ; null : pas de menace sérieuse (ou pas cherchée)
        public bool MenaceCherchee { get; set; }
        public string? MenaceUci { get; set; }
        public string? TexteMenace { get; set; }   // "15. Cxf7", ou "15. Dh7#"
        public int? MenaceMatEn { get; set; }      // la menace est un mat en N (du point de vue du joueur qui menace)
        public bool Ignoree { get; set; }                   // le moteur n'a donné aucun score : position sautée (sinon l'analyse tournerait en rond)
        public bool AApprofondir { get; set; }              // 2e passage : position avant ou après un coup douteux, à revoir plus longtemps
        public bool Approfondie { get; set; }               // ... et revue (avec ou sans score : elle n'est pas redemandée)
        public bool Analysee => Evaluation != null || ChancesFinDePartie != null || Ignoree;
        public double? Chances => ChancesFinDePartie ?? (Evaluation is Evaluation e ? JugementCoups.ChancesDeGain(e) : null);
    }

    // Ce que le moteur pense du coup n° IndexCoup de ListeCoups (Perte : chances de gain perdues par le camp qui l'a joué ;
    // Precision : en %, voir JugementCoups.Precision). EvaluationMeilleur : évaluation de la position avant le coup, c'est-à-dire
    // celle du meilleur coup du moteur ; MeilleurCoupLong : avec sa case de départ ; VarianteMeilleure : la suite prévue
    public record JugementCoup(int IndexCoup, ColorPiece Camp, double Perte, string Annotation, Evaluation? EvaluationApres,
                               string? MeilleurCoup, bool MeilleurJoue, Evaluation? EvaluationMeilleur, string? MeilleurCoupLong,
                               string? VarianteMeilleure, double Precision, string? Commentaire = null);

    // Un sacrifice du meilleur coup : la pièce perdue (ou offerte) et sa case, offert (refusé dans la variante du moteur) ou
    // accepté, et s'il ne s'agit que de la qualité (une tour contre une pièce légère)
    public sealed record DetailSacrifice(TypePiece Piece, string Case, bool Offert, bool Qualite)
    {
        // "Sacrifice du cavalier" : sans la case ni "offert" (choix de Bruno, « (offert en c8) » ne servait à rien)
        public string Texte => Qualite ? Langue.T("Sacrifice de la qualité") : Piece switch
        {
            TypePiece.CavalierBlanc or TypePiece.CavalierNoir => Langue.T("Sacrifice du cavalier"),
            TypePiece.FouBlanc or TypePiece.FouNoir => Langue.T("Sacrifice du fou"),
            TypePiece.TourBlanche or TypePiece.TourNoire => Langue.T("Sacrifice de la tour"),
            TypePiece.ReineBlanche or TypePiece.ReineNoire => Langue.T("Sacrifice de la dame"),
            _ => Langue.T("Sacrifice de matériel")
        };
    }

    public record BilanCamp(int Imprecisions, int Erreurs, int Gaffes, int Precision);     // précision moyenne des coups, en %

    public class AnalyseDePartie
    {
        private readonly List<PositionAnalysee> _positions = [];    // [0] : avant le 1er coup ; [k] : après le k-ième coup joué
        private readonly List<int> _indexCoups = [];                // place dans ListeCoups du k-ième coup joué
        private readonly List<Coup> _coups;
        private readonly bool _approfondir;                         // 2e passage plus long sur les coups douteux
        private bool _premierPassageFini;

        public IReadOnlyList<PositionAnalysee> Positions => _positions;

        public int IndexDansListeCoups(int indexPosition) =>
            // Le coup de ListeCoups qui mène à cette position (pour l'afficher) ; position de départ : -1, ou 0 (élément
            // "position de départ") pour une partie commencée depuis un FEN
            indexPosition > 0 ? _indexCoups[indexPosition - 1] : _coups.Count > 0 && _coups[0].EstPositionDeDepart ? 0 : -1;

        public AnalyseDePartie(IReadOnlyList<Coup> coups, bool approfondir = false, Func<string, string, bool>? coupDeBibliotheque = null,
                               CatalogueOuvertures? ouvertures = null)
        {   // Les positions de la partie : celle de départ (position initiale ou FEN de départ), puis celle après chaque coup.
            // approfondir : après le 1er passage, les positions avant et après chaque coup douteux (?!, ?, ??) sont revues plus
            // longtemps (voir EnApprofondissement) : un jugement sévère mérite d'être confirmé, et le temps n'est pas perdu sur les autres
            // coupDeBibliotheque(fen d'avant, coup UCI) : le coup est-il dans la bibliothèque d'ouvertures ? Un tel coup n'est jamais
            // annoté, ni compté dans le bilan ni retenu comme coup critique (choix de Bruno, comme Lichess : la théorie n'est pas jugée)
            _coups = [.. coups];
            _approfondir = approfondir;
            string fenDepart = _coups.Count > 0 && _coups[0].EstPositionDeDepart ? _coups[0].Fen : FenDepart;
            _positions.Add(NouvellePosition(fenDepart));
            for (int i = 0; i < _coups.Count; i++)
                if (!_coups[i].EstPositionDeDepart)
                {
                    if (coupDeBibliotheque != null && coupDeBibliotheque(_positions[^1].Fen, _coups[i].Uci))
                        _coupsDeBibliotheque.Add(i);
                    _indexCoups.Add(i);
                    _positions.Add(NouvellePosition(_coups[i].Fen));
                }
            // ouvertures : nom de l'ouverture (code ECO) d'après la table de Lichess, au dernier coup qui y mène (CommentairesOuverture)
            if (ouvertures?.DerniereConnue(_coups) is (int indexOuverture, Ouverture ouverture))
                (IndexOuverture, Ouverture) = (indexOuverture, ouverture);
        }

        public Ouverture? Ouverture { get; }           // l'ouverture de la partie (null : aucune position de la table)
        public int IndexOuverture { get; } = -1;      // le coup (place dans ListeCoups) qui y mène

        public Dictionary<int, string> CommentairesOuverture()
        {   // Coups (place dans ListeCoups) commentés pour l'ouverture : son nom ("Défense sicilienne : variante Najdorf (B90)") au
            // dernier coup qui mène à une position de la table, et "Hors bibliothèque" au premier coup qui n'est plus dans la
            // bibliothèque chargée (seulement si les coups d'avant y étaient tous, et qu'il y en a au moins un : sans bibliothèque, ou
            // pour une partie commencée à une position inconnue de la bibliothèque, rien)
            Dictionary<int, string> textes = [];
            if (Ouverture != null && IndexOuverture >= 0 && !_coups[IndexOuverture].EstPositionDeDepart)
                textes[IndexOuverture] = Ouverture.Texte;
            int? horsBibliotheque = _indexCoups.Cast<int?>().FirstOrDefault(i => !EstCoupDeBibliotheque(i!.Value));
            if (horsBibliotheque is int sortie && _indexCoups.IndexOf(sortie) > 0)
                textes[sortie] = textes.TryGetValue(sortie, out string? nom) ? nom + ". " + Langue.T("Hors bibliothèque")
                                                                              : Langue.T("Hors bibliothèque");
            return textes;
        }

        private readonly HashSet<int> _coupsDeBibliotheque = [];    // place dans ListeCoups des coups connus de la bibliothèque
        public bool EstCoupDeBibliotheque(int indexCoup) => _coupsDeBibliotheque.Contains(indexCoup);

        private static PositionAnalysee NouvellePosition(string fen)
        {   // Mat ou pat : la position est jugée tout de suite (le moteur n'aurait aucun coup à proposer)
            Position position = PositionDepuisFen(fen);
            double? fin = null;
            if (!Regles.ResteCoupsJouables(position))
                fin = !Regles.CampAuTraitEnEchec(position) ? 0 : position.QuiJoue == ColorPiece.Blanc ? -1 : 1;
            return new PositionAnalysee { Fen = fen, AuTrait = position.QuiJoue, ChancesFinDePartie = fin,
                                          NombreCoupsLegaux = Regles.CoupsLegaux(position).Count };
        }

        public int? PositionSuivante
        {   // Prochaine position à faire analyser par le moteur (null : toutes le sont). De la FIN vers le début, comme ChessBase :
            // le moteur garde en mémoire (table de hachage) les positions des coups suivants, déjà analysées, qu'il retrouve
            // dans sa recherche : il va plus profond et voit mieux les combinaisons (le gain de la dame au coup 28 est déjà connu
            // quand il analyse le sacrifice du coup 25)
            get
            {
                int index = _positions.FindLastIndex(p => !p.Analysee);
                if (index < 0)
                {   // 2e passage (positions marquées à la fin du 1er), de la fin vers le début lui aussi
                    MarqueCoupsDouteux();
                    index = _positions.FindLastIndex(p => p.AApprofondir && !p.Approfondie);
                }
                return index < 0 ? null : index;
            }
        }
        public int ProfondeurVisee(int indexPosition)
        {   // Profondeur demandée au moteur pour cette position (la durée réglée reste un plafond, "go depth … movetime …") : plus
            // rapide qu'un temps fixe, qui était trop long pour les positions faciles (mesuré sur les deux parties de Bruno, octobre
            // 2026 : 40 à 73 % de temps en moins à la profondeur 20, mêmes « ! », « !! », « ?? » et coup critique qu'à 10 s fixes).
            // Moins profond pour la théorie (position entre deux coups de la bibliothèque : ces coups ne sont pas jugés) ; plus
            // profond pour le 2e passage de l'analyse complète (profondeur 24, plafond 30 s : 13 à 28 % de temps en moins, mêmes
            // annotations importantes). Essayé puis retiré : moins profond quand la position suivante est déjà décidée ; c'est
            // justement la position d'avant une gaffe, où la perte doit être bien mesurée (le coup critique de la partie de Bruno
            // passait de 27. Fd4?? à 28... Rxg7)
            if (EnApprofondissement)
                return JugementCoups.ProfondeurApprofondissement;
            bool coupSuivantTheorique = indexPosition < _indexCoups.Count && EstCoupDeBibliotheque(_indexCoups[indexPosition]);
            bool coupPrecedentTheorique = indexPosition == 0 || EstCoupDeBibliotheque(_indexCoups[indexPosition - 1]);
            return coupSuivantTheorique && coupPrecedentTheorique ? JugementCoups.ProfondeurTheorie : JugementCoups.ProfondeurAnalyse;
        }

        public int NombreAAnalyser => _positions.Count(p => p.ChancesFinDePartie == null);
        public int NombreAnalysees => _positions.Count(p => p.ChancesFinDePartie == null && p.Evaluation != null);
        public bool EnApprofondissement => _premierPassageFini && !Terminee;     // la position suivante est à revoir plus longtemps
        public int NombreAApprofondir => _positions.Count(p => p.AApprofondir);
        public int NombreApprofondies => _positions.Count(p => p.AApprofondir && p.Approfondie);
        public bool Terminee => PositionSuivante == null;

        private void MarqueCoupsDouteux()
        {   // Fin du 1er passage (une seule fois) : les positions avant et après chaque coup douteux sont à revoir. Celle d'avant
            // confirme le meilleur coup et son évaluation, celle d'après l'évaluation du coup joué
            if (_premierPassageFini)
                return;
            _premierPassageFini = true;
            if (!_approfondir)
                return;
            // (et les "!" et "!!" : leur 2e variante, celle qui fait d'un coup le seul bon, mérite aussi une recherche plus longue)
            HashSet<int> seulsBonsCoups = [.. DifficultesAMesurer().Select(d => _indexCoups[d.IndexPosition])];   // (difficulté pas encore mesurée)
            foreach (JugementCoup jugement in Jugements().Where(j => !EstCoupDeBibliotheque(j.IndexCoup)
                                                                     && (j.Perte >= JugementCoups.SeuilImprecision || j.Annotation is "!" or "!!"
                                                                         || seulsBonsCoups.Contains(j.IndexCoup))).ToList())
            {
                int k = _indexCoups.IndexOf(jugement.IndexCoup);
                foreach (PositionAnalysee position in new[] { _positions[k], _positions[k + 1] })
                    position.AApprofondir = position.ChancesFinDePartie == null;   // (mat ou pat : rien à revoir)
            }
        }

        public void Enregistre(int indexPosition, LigneAnalyse? meilleure, LigneAnalyse? seconde = null)
        {   // Résultat du moteur pour la position (sa meilleure variante, et la 2e s'il en a donné une : elle dit si le meilleur coup
            // était le seul bon) ; sans score, la position est sautée (ses coups ne seront pas jugés).
            // 2e passage : le nouveau résultat remplace le premier (sans score, le premier est gardé)
            PositionAnalysee position = _positions[indexPosition];
            if (_premierPassageFini && position.AApprofondir)
                position.Approfondie = true;
            if (seconde?.Evaluation is Evaluation evaluationSeconde && meilleure?.Evaluation != null)
                position.EvaluationSeconde = evaluationSeconde;
            if (meilleure?.Evaluation is not Evaluation evaluation)
            {
                if (position.Evaluation == null)
                    position.Ignoree = true;
                return;
            }
            position.Evaluation = evaluation;
            position.MeilleurCoup = PremierCoup(meilleure.VariantePgn);
            position.VarianteMeilleure = meilleure.VariantePgn;
            position.VarianteMeilleureUci = meilleure.VarianteUci;
            position.MeilleurCoupUci = (meilleure.VarianteUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            position.MeilleurCoupLong = NotationLongue(position.Fen, position.MeilleurCoupUci, position.MeilleurCoup);
        }

        public static string? NotationLongue(string fen, string? coupUci, string? coupPgn = null)
        {   // Le coup avec sa case de départ, en notation française : "Dd8-d7", "Cf3xe5", "e2-e4", "e7-e8=D", "O-O" ;
            // l'échec ou le mat ("+", "#") est repris de la notation courte du même coup s'il y en a une
            if (coupUci == null || coupUci.Length < 4)
                return null;
            List<TypePiece> pieces = PositionDepuisFen(fen).Pieces;
            string depart = coupUci[..2], arrivee = coupUci.Substring(2, 2);
            TypePiece piece = pieces[RenvoieCaseIndex120(depart)];
            bool pion = piece is TypePiece.PionBlanc or TypePiece.PionNoir;
            string texte;
            if (piece is TypePiece.RoiBlanc or TypePiece.RoiNoir && Math.Abs(depart[0] - arrivee[0]) == 2)
                texte = arrivee[0] == 'g' ? "O-O" : "O-O-O";
            else
            {
                bool prise = pieces[RenvoieCaseIndex120(arrivee)] != TypePiece.Vide || (pion && depart[0] != arrivee[0]);   // (en passant)
                texte = LettrePiece(piece) + depart + (prise ? "x" : "-") + arrivee
                      + (coupUci.Length >= 5 ? "=" + LettrePiece(PieceDePromotion(coupUci[4], ColorPiece.Blanc)) : "");
            }
            string fin = coupPgn?.TrimEnd('!', '?') ?? "";
            return texte + (fin.EndsWith('#') ? "#" : fin.EndsWith('+') ? "+" : "");
        }

        private static string LettrePiece(TypePiece piece) => piece switch
        {
            TypePiece.RoiBlanc or TypePiece.RoiNoir => "R",
            TypePiece.ReineBlanche or TypePiece.ReineNoire => "D",
            TypePiece.TourBlanche or TypePiece.TourNoire => "T",
            TypePiece.FouBlanc or TypePiece.FouNoir => "F",
            TypePiece.CavalierBlanc or TypePiece.CavalierNoir => "C",
            _ => ""
        };

        public static string? PremierCoup(string? variantePgn) =>
            // "12. Cf3 Fe7" -> "Cf3" ; "19 ... Fa2 20. Cf3" (Noirs au trait) -> "Fa2" : le premier mot qui contient une lettre
            // (les numéros "19", "19.", "19..." et les "..." sont sautés)
            (variantePgn ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(m => m.Any(char.IsLetter));

        public JugementCoup? Jugement(int indexCoup)
        {   // Le coup n° indexCoup de ListeCoups : null s'il n'en est pas un, ou si les positions avant et après ne sont pas analysées
            int k = _indexCoups.IndexOf(indexCoup);
            if (k < 0 || _positions[k].Chances is not double avant || _positions[k + 1].Chances is not double apres)
                return null;
            PositionAnalysee positionAvant = _positions[k];
            ColorPiece camp = positionAvant.AuTrait;
            string? meilleur = positionAvant.MeilleurCoup;
            // Meilleur coup joué ? Comparé au format UCI s'il est connu (sans ambiguïté), sinon en notation
            bool meilleurJoue = positionAvant.MeilleurCoupUci != null
                ? positionAvant.MeilleurCoupUci == _coups[indexCoup].Uci.Trim()
                : meilleur != null && SansSymboles(meilleur) == SansSymboles(_coups[indexCoup].PgnFrSansNumero);
            // Le meilleur coup du moteur ne perd rien (l'écart d'évaluation entre deux recherches ne serait que du bruit)
            double sens = camp == ColorPiece.Blanc ? 1 : -1;                // chances vues du camp qui joue
            double perte = meilleurJoue ? 0 : Math.Max(0, sens * (avant - apres));
            double precision = meilleurJoue ? 100 : JugementCoups.Precision(sens * avant, sens * apres);
            (string annotation, DetailSacrifice? sacrifice) = EstCoupDeBibliotheque(indexCoup) ? ("", null)
                              : meilleurJoue ? BonCoup(k, indexCoup, sens) : (JugementCoups.Annotation(perte), null);
            // Partie déjà décidée, et toujours décidée dans le même sens après le coup : pas de ?!, ? ni ?? (comme ChessBase, choix de
            // Bruno du 2026-10-28 : 20... Cg8?! de Carlsen–Ernst, -5.81 → -9.79, n'apprenait rien). La perte compte toujours dans la
            // précision et pour le coup critique
            bool decideAvant = Math.Abs(avant) >= JugementCoups.LimitePartieDecidee, decideApres = Math.Abs(apres) >= JugementCoups.LimitePartieDecidee;
            if (!meilleurJoue && decideAvant && decideApres && Math.Sign(avant) == Math.Sign(apres))
                annotation = "";
            // Commentaire : le sacrifice (et s'il détruit le rempart du roque), ou le mat manqué ; puis le thème tactique du coup
            string? texteSacrifice = sacrifice == null ? null
                : DetruitLeRempart(k, indexCoup) ? Langue.T("{0} : détruit le rempart du roque", sacrifice.Texte) : sacrifice.Texte;
            string? theme = meilleurJoue && !EstCoupDeBibliotheque(indexCoup)
                ? AttaqueALaDecouverte(k, indexCoup) ?? AttaqueDouble(k, indexCoup) : null;
            string?[] commentaires = [texteSacrifice ?? MatManque(k, indexCoup, sens, meilleurJoue), theme];
            return new JugementCoup(indexCoup, camp, perte, annotation, _positions[k + 1].Evaluation, meilleur, meilleurJoue,
                                    positionAvant.Evaluation, positionAvant.MeilleurCoupLong, positionAvant.VarianteMeilleure, precision,
                                    commentaires.Any(c => c != null) ? string.Join(". ", commentaires.OfType<string>()) : null);
        }

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
            // l'adversaire a vraiment roqué plus tôt (choix de Bruno, 2026-10-23 : un roi allé à pied en g8, 11... Rf8 12... Rg8,
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
            // coup prend lui-même ne vient pas de l'attaque double (remarque de Bruno, 2026-10-25 : 14. Dxh8+ Re7 15. Dxh7, une tour
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
        {   // Échec double, échec à la découverte, attaque à la découverte (choix de Bruno, 2026-10-26). La pièce jouée démasque
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

        private string? MatManque(int k, int indexCoup, double sens, bool meilleurJoue)
        {   // "Mat en 8 manqué" (étape B, choix de Bruno) : le joueur avait un mat forcé avant son coup, et ne l'a plus après (s'il le
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
            // Difficulté mesurée (choix de Bruno, octobre 2026) : un sacrifice correct a "!", et "!!" s'il était difficile à trouver
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
            // sacrifice ouvre le roque adverse ou si la suite mate (la combinaison qui conclut, choix de Bruno du 2026-10-28 :
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
        {   // "!" (bon coup, choix de Bruno) : le meilleur coup a été joué ET c'était le seul bon : la 2e variante du moteur aurait fait
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
            // elle n'avait qu'UNE case sûre, et c'est là qu'elle va (ex : Caro-Kann, 8. h5 Fh7 : le fou attaqué par le pion n'a que h7,
            // remarque de Bruno). Avec plusieurs cases sûres, trouver la seule bonne est un vrai choix, qui garde son "!" (ex : Cg6) ;
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

        private DetailSacrifice? Sacrifice(int k)
        {   // Le meilleur coup sacrifie du matériel (null : non), et lequel. La variante du moteur (qui commence par ce coup) est jouée sur une copie : à la
            // PREMIÈRE position calme après la réponse de l'adversaire (le demi-coup suivant de la variante n'est pas une prise, ou la
            // variante s'arrête), dans les DemiCoupsSacrifice premiers demi-coups, le joueur a au moins SacrificeMinimum points de
            // matériel de moins qu'avant son coup. Au milieu d'une suite d'échanges, le matériel ne veut rien dire (27... Fxf3 28. Fxg7
            // Fxg2 29. Rxg2 Rxg7 : à égalité au bout) ; et un sacrifice plus loin dans la variante est celui d'un AUTRE coup
            // (12. The1 b4 13. Cd5 exd5 : c'est 13. Cd5 qui sacrifie).
            // Sacrifice OFFERT (refusé dans la variante, parce que l'accepter perd) : juste après le coup, l'adversaire peut gagner au
            // moins SacrificeMinimum points en prenant (GainImmediat) ; ex : 25... Cfe4!! laisse la tour c8 au fou f5, et la variante
            // du moteur continue par 26. c3 (choix de Bruno). Seulement pour une pièce NOUVELLEMENT en prise (case qui ne l'était pas
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

        public List<(int IndexPosition, string Fen, string PriseUci)> PiegesAChercher()
        {   // Analyse complète : pour chaque coup jugeable, la prise la plus tentante (au moins SacrificeMinimum de matériel gagné à
            // première vue) que le joueur n'a pas jouée et qui n'est pas le meilleur coup ; l'interface la fait chercher par le 2e
            // moteur (EnregistrePiege). Une position sans prise tentante est notée cherchée tout de suite
            List<(int, string, string)> demandes = [];
            for (int k = 0; k < _indexCoups.Count; k++)
            {
                PositionAnalysee avant = _positions[k];
                if (avant.PiegeCherche || avant.Evaluation is not Evaluation meilleure || avant.MeilleurCoupUci == null)
                    continue;
                double sens = avant.AuTrait == ColorPiece.Blanc ? 1 : -1;
                if (meilleure.MatEn != null || Math.Abs(meilleure.Centipions ?? 0) >= JugementCoups.PiegeJoueurPerdu)
                {   // un mat est déjà là (pour l'un ou l'autre), ou la partie est décidée de beaucoup : ne pas commenter ce que le joueur ne
                    // prend pas (perdu de moins, le piège reste instructif : 28. Fxg7 de la partie de Bruno, à -4.5, « le fou f3 est
                    // imprenable » ; gagnant de plus de 6 pions, il n'apprenait rien, 26. Tg3+ à +19 de Carlsen–Ernst, choix du 2026-10-28)
                    avant.PiegeCherche = true;
                    continue;
                }
                int indexCoup = _indexCoups[k];
                string joue = _coups[indexCoup].Uci.Trim();
                (string Source, string Destination, int Gain) tentante = EstCoupDeBibliotheque(indexCoup) ? default
                    : PrisesGagnantes(PositionDepuisFen(avant.Fen))
                        .Where(p => p.Gain >= JugementCoups.SacrificeMinimum && p.Source + p.Destination is string uci
                                    && !joue.StartsWith(uci) && !avant.MeilleurCoupUci.StartsWith(uci))
                        .OrderByDescending(p => p.Gain).FirstOrDefault();
                if (tentante.Source == null)
                    avant.PiegeCherche = true;
                else
                    demandes.Add((k, avant.Fen, tentante.Source + tentante.Destination));
            }
            return demandes;
        }

        public void EnregistrePiege(int indexPosition, string priseUci, int? centipions, int? matEn, string? varianteUci)
        {   // Réponse du 2e moteur pour la prise tentante (score du point de vue du camp au trait ; rien : null). C'est un piège si elle
            // fait perdre au moins une erreur (SeuilErreur) par rapport au meilleur coup : "La tour g6 est imprenable : 22. Fxg6? fxg6
            // 23. g4 Tc8 (-5.57)" (choix de Bruno, partie Parkhov–Bruzon : pourquoi 22. g4 et pas 22. Fxg6 ?)
            PositionAnalysee avant = _positions[indexPosition];
            avant.PiegeCherche = true;
            if ((centipions == null && matEn == null) || avant.Evaluation is not Evaluation meilleure)
                return;
            double sens = avant.AuTrait == ColorPiece.Blanc ? 1 : -1;
            Evaluation prise = new(centipions is int cp ? (int)sens * cp : null, matEn is int mat ? (int)sens * mat : null);   // (point de vue des Blancs)
            // (ou au moins PiegePerteMinimum en pions : dans une position déjà perdue, les chances de gain ne bougent presque plus,
            // -4.53 → -6.67 pour 28. gxf3?, et le piège est pourtant instructif)
            bool perteEnChances = sens * (JugementCoups.ChancesDeGain(meilleure) - JugementCoups.ChancesDeGain(prise)) >= JugementCoups.SeuilErreur;
            bool perteEnPions = meilleure.Centipions is int cpMeilleur && prise.Centipions is int cpPrise && sens * (cpMeilleur - cpPrise) >= JugementCoups.PiegePerteMinimum;
            bool matPerdu = prise.MatEn is int matPrise && sens * matPrise < 0;
            if (!perteEnChances && !perteEnPions && !matPerdu)
                return;
            Position position = PositionDepuisFen(avant.Fen);
            string caseCible = priseUci.Substring(2, 2);
            string uci = string.Join(" ", (varianteUci ?? priseUci).Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(4));
            List<string> mots = [.. Outils.VarianteUciVersPgn(uci, DemiCoupAvant(position), false, position).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)];
            int premier = mots.FindIndex(m => m.Any(char.IsLetter));
            if (premier >= 0)
                mots[premier] += "?";       // la prise tentante est une erreur
            string variante = Langue.Notation(string.Join(" ", mots)) + $" ({prise.Texte})";
            avant.CasePiege = caseCible;
            avant.TextePiege = position.Pieces[RenvoieCaseIndex120(caseCible)] switch
            {
                TypePiece.ReineBlanche or TypePiece.ReineNoire => Langue.T("La dame {0} est imprenable : {1}", caseCible, variante),
                TypePiece.TourBlanche or TypePiece.TourNoire => Langue.T("La tour {0} est imprenable : {1}", caseCible, variante),
                TypePiece.FouBlanc or TypePiece.FouNoir => Langue.T("Le fou {0} est imprenable : {1}", caseCible, variante),
                TypePiece.CavalierBlanc or TypePiece.CavalierNoir => Langue.T("Le cavalier {0} est imprenable : {1}", caseCible, variante),
                _ => Langue.T("Le pion {0} est imprenable : {1}", caseCible, variante)
            };
        }

        public List<(int IndexPosition, string FenCoupNul)> MenacesAChercher()
        {   // Analyse complète : pour chaque position analysée après un coup, la même position avec le trait rendu au joueur qui vient
            // de jouer (coup nul) ; l'interface demande au 2e moteur ce qu'il y jouerait (EnregistreMenace). Pas de menace cherchée
            // après un coup de la bibliothèque, après une prise (la reprise attendue serait la "menace") ou un échec (pas de coup nul
            // possible), ni quand un mat est déjà annoncé ou la partie décidée ; une telle position est notée cherchée tout de suite
            List<(int, string)> demandes = [];
            for (int k = 1; k < _positions.Count; k++)
            {
                PositionAnalysee apres = _positions[k];
                if (apres.MenaceCherchee || apres.Evaluation is not Evaluation evaluation)
                    continue;
                int indexCoup = _indexCoups[k - 1];
                string fenAvant = _positions[k - 1].Fen;
                bool sansMenace = EstCoupDeBibliotheque(indexCoup) || evaluation.MatEn != null
                    || Math.Abs(evaluation.Centipions ?? 0) >= JugementCoups.PiegeJoueurPerdu
                    || EstUnePrise(PositionDepuisFen(fenAvant), _coups[indexCoup].Uci.Trim())
                    || Regles.CampAuTraitEnEchec(PositionDepuisFen(apres.Fen));
                if (sansMenace)
                {
                    apres.MenaceCherchee = true;
                    continue;
                }
                demandes.Add((k, FenCoupNul(apres.Fen)));
            }
            return demandes;
        }

        public static string FenCoupNul(string fen)
        {   // La même position, le trait à l'autre camp et sans case en passant (le coup nul : "et si l'adversaire ne faisait rien ?") ;
            // les Noirs passant leur tour, le coup suivant des Blancs a le numéro suivant (menace "4. Dxf7#" après 3. Dh5)
            string[] champs = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (champs.Length >= 4)
            {
                if (champs[1] == "b" && champs.Length >= 6 && int.TryParse(champs[5], out int numero))
                    champs[5] = (numero + 1).ToString();
                champs[1] = champs[1] == "w" ? "b" : "w";
                champs[3] = "-";
            }
            return string.Join(" ", champs);
        }

        public void EnregistreMenace(int indexPosition, int? centipions, int? matEn, string? varianteUci)
        {   // Réponse du 2e moteur dans la position du coup nul (score du point de vue du joueur qui menace ; rien : null). C'est une
            // menace si son coup gagne au moins une erreur en chances (SeuilErreur), ou PiegePerteMinimum en pions, par rapport à
            // l'évaluation de la position, ou s'il mate (sans mat déjà annoncé)
            PositionAnalysee apres = _positions[indexPosition];
            apres.MenaceCherchee = true;
            string? coupUci = (varianteUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if ((centipions == null && matEn == null) || coupUci == null || apres.Evaluation is not Evaluation actuelle)
                return;
            double sens = apres.AuTrait == ColorPiece.Blanc ? -1 : 1;      // le joueur qui menace n'est pas celui qui a le trait
            Evaluation menace = new(centipions is int cp ? (int)sens * cp : null, matEn is int mat ? (int)sens * mat : null);   // (point de vue des Blancs)
            bool mate = matEn is int matJoueur && matJoueur > 0;
            bool gainEnChances = sens * (JugementCoups.ChancesDeGain(menace) - JugementCoups.ChancesDeGain(actuelle)) >= JugementCoups.SeuilGaffe;
            bool gainEnPions = actuelle.Centipions is int cpActuel && menace.Centipions is int cpMenace && sens * (cpMenace - cpActuel) >= JugementCoups.MenaceGainMinimum;
            if (!mate && !gainEnChances && !gainEnPions)
                return;
            Position positionNulle = PositionDepuisFen(FenCoupNul(apres.Fen));
            // Seulement une menace concrète : une prise ou un échec (ou un mat). Le coup nul donne un temps gratuit, que le moteur
            // emploie souvent à un coup calme ("Menace : 11... Tb8", une poussée de pion) : ce n'est pas une menace
            bool concrete = mate || EstUnePrise(positionNulle, coupUci) || DonneEchec(positionNulle, coupUci);
            // ... sauf une menace calme très forte : une gaffe en chances ET au moins MenaceCalmeMinimum (3 pions) ; ChessBase écrit
            // « Menace fortement Cg6! » sur 14. Te1 de Beliavsky–Larsen, le sacrifice 15. Cg6 n'étant ni une prise ni un échec
            // (choix de Bruno, 2026-10-28)
            bool calmeTresForte = gainEnChances && actuelle.Centipions is int cpAvant && menace.Centipions is int cpApres
                                  && sens * (cpApres - cpAvant) >= JugementCoups.MenaceCalmeMinimum;
            if (!concrete && !calmeTresForte)
                return;
            // Pas une menace créée par le coup : une pièce que le joueur pouvait déjà gagner avant de jouer (15... Cd5 de
            // Beliavsky–Larsen, « Menace : 16... fxg6 » : le cavalier g6 était déjà en prise, choix de Bruno du 2026-10-28)
            if (indexPosition > 0 && coupUci.Length >= 4 && !mate
                && PiecesEnPrise(PositionDepuisFen(_positions[indexPosition - 1].Fen)).TryGetValue(coupUci.Substring(2, 2), out int dejaGagne)
                && dejaGagne >= JugementCoups.SacrificeMinimum)
                return;
            string texte = Outils.VarianteUciVersPgn(coupUci, DemiCoupAvant(positionNulle), false, positionNulle).Trim().Replace(" ... ", "... ");
            apres.MenaceUci = coupUci;
            apres.MenaceMatEn = mate ? matEn : null;
            apres.TexteMenace = Langue.Notation(texte);
        }

        private string? CommentaireMenace(int k)
        {   // La menace créée par le coup qui mène à la position k (pas celle qui existait déjà au coup précédent du même joueur)
            PositionAnalysee apres = _positions[k];
            if (apres.MenaceUci == null || apres.TexteMenace == null || (k >= 3 && _positions[k - 2].MenaceUci == apres.MenaceUci))
                return null;
            return apres.MenaceMatEn is int mat
                ? (mat == 1 ? Langue.T("Menace de mat : {0}", apres.TexteMenace) : Langue.T("Menace de mat en {0} : {1}", mat, apres.TexteMenace))
                : Langue.T("Menace : {0}", apres.TexteMenace);
        }

        private string? CommentaireRefutation(int k, string? parade)
        {   // Le coup joué depuis la position k est une erreur : le meilleur coup de l'adversaire juste après, qui la punit
            // ("Réfutation : 27... Fxf3") ; rien s'il est déjà donné par "Ne pare pas la menace …", ou si la position d'après n'est
            // pas analysée
            if (k + 1 >= _positions.Count || _positions[k + 1].MeilleurCoupUci is not string uci)
                return null;
            if (_positions[k].MenaceUci is string menace && parade != null && uci.StartsWith(menace))
                return null;
            Position apres = PositionDepuisFen(_positions[k + 1].Fen);
            string coup = Outils.VarianteUciVersPgn(uci, DemiCoupAvant(apres), false, apres).Trim().Replace(" ... ", "... ");
            return coup == "" ? null : Langue.T("Réfutation : {0}", Langue.Notation(coup));
        }

        private string? CommentaireParade(int k, JugementCoup jugement)
        {   // Le coup qui part de la position k face à une menace (créée par le coup d'avant) : "Pare la menace" seulement s'il était
            // le seul bon coup (la 2e variante du moteur perdait une erreur ; sinon presque chaque réponse "parait" une menace) ; s'il
            // perd et que la menace est justement le meilleur coup de l'adversaire ensuite, il ne l'a pas parée
            PositionAnalysee avant = _positions[k];
            if (avant.MenaceUci == null || avant.TexteMenace == null || EstCoupDeBibliotheque(jugement.IndexCoup))
                return null;
            double sens = avant.AuTrait == ColorPiece.Blanc ? 1 : -1;
            bool seulBonCoup = avant.EvaluationSeconde is Evaluation seconde && avant.Evaluation is Evaluation meilleure
                && sens * (JugementCoups.ChancesDeGain(meilleure) - JugementCoups.ChancesDeGain(seconde)) >= JugementCoups.SeuilSeulBonCoup;
            if (jugement.Perte < JugementCoups.SeuilImprecision)
                return seulBonCoup ? Langue.T("Pare la menace {0}", avant.TexteMenace) : null;
            return k + 1 < _positions.Count && _positions[k + 1].MeilleurCoupUci is string suite && suite.StartsWith(avant.MenaceUci)
                ? Langue.T("Ne pare pas la menace {0}", avant.TexteMenace) : null;
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

        private bool EstReprise(int k, int indexCoup)
        {   // Le coup joué arrive sur la case où l'adversaire vient de prendre une pièce (ex : 3... Fxe4 4. Dxe4)
            string joue = _coups[indexCoup].Uci.Trim(), precedent = _coups[_indexCoups[k - 1]].Uci.Trim();
            if (joue.Length < 4 || precedent.Length < 4 || joue.Substring(2, 2) != precedent.Substring(2, 2))
                return false;
            List<TypePiece> piecesAvantPrecedent = PositionDepuisFen(_positions[k - 1].Fen).Pieces;     // avant le coup de l'adversaire
            return piecesAvantPrecedent[RenvoieCaseIndex120(precedent.Substring(2, 2))] != TypePiece.Vide;
        }

        private static string SansSymboles(string coup) => coup.TrimEnd('+', '#', '!', '?');

        public IEnumerable<JugementCoup> Jugements() => _indexCoups.Select(Jugement).OfType<JugementCoup>();

        public Dictionary<int, string> CommentairesAppreciation() =>
            // Le texte de chaque changement d'appréciation : "Avantage Blanc", "Égal"… (voir NiveauxAppreciation)
            NiveauxAppreciation().ToDictionary(n => n.Key, n => n.Value.Texte);

        public Dictionary<int, (int Niveau, string Texte)> NiveauxAppreciation()
        {   // Coups (place dans ListeCoups) après lesquels l'appréciation change de niveau (JugementCoups.NiveauAppreciation), avec ce
            // niveau et son texte. La partie est parcourue dans l'ordre ; une position pas (encore) analysée est sautée, le niveau de
            // départ est celui de la 1re position connue (sans commentaire) ; jamais sur un coup de la bibliothèque (la théorie n'est
            // pas commentée)
            Dictionary<int, (int, string)> niveaux = [];
            int? actuel = null;
            for (int k = 0; k < _positions.Count; k++)
            {
                if (_positions[k].Evaluation is not Evaluation evaluation)
                    continue;               // (pas analysée, ou mat/pat final, jugé sans le moteur)
                int niveau = JugementCoups.NiveauAppreciation(evaluation, actuel ?? 0);
                if (actuel != null && niveau != actuel && k > 0 && !EstCoupDeBibliotheque(_indexCoups[k - 1]))
                    niveaux[_indexCoups[k - 1]] = (niveau, JugementCoups.TexteAppreciation(niveau, evaluation));
                actuel = niveau;
            }
            return niveaux;
        }

        public Dictionary<int, string> CommentairesFinale()
        {   // Coups (place dans ListeCoups) qui amènent une finale, ou une autre sorte de finale ("Finale de tours") : le type doit
            // rester le même pendant les deux demi-coups suivants (sinon, au milieu de l'échange des fous 28. Fxg7 Rxg7 29. gxf3,
            // "Finale" puis "Finale de tours" un coup plus tard) ; sans moteur, d'après les pièces de la position
            List<string?> types = [.. _positions.Select(p => JugementCoups.TypeDeFinale(PositionDepuisFen(p.Fen)))];
            Dictionary<int, string> textes = [];
            string? annoncee = null;
            for (int k = 1; k < _positions.Count; k++)
            {
                string? finale = types[k];
                bool stable = types.Skip(k + 1).Take(2).All(t => t == finale);
                if (finale != null && stable && finale != annoncee)
                {
                    textes[_indexCoups[k - 1]] = finale;
                    annoncee = finale;
                }
            }
            return textes;
        }

        public const int EvenementsParCoup = 2;     // sacrifice, thème, menace, parade, piège : au plus deux dans le commentaire d'un coup

        public void AppliqueAuxCoups(IReadOnlyList<Coup> coups)
        {
            Dictionary<int, (int Niveau, string Texte)> niveaux = NiveauxAppreciation();
            Dictionary<int, string> appreciations = niveaux.ToDictionary(n => n.Key, n => n.Value.Texte), finales = CommentairesFinale(),
                                    ouverture = CommentairesOuverture();
            // Range les résultats dans les coups jugés (même liste qu'à la construction) : évaluation, meilleur coup, et annotation
            // proposée, sauf si le joueur en a mis une lui-même (une annotation proposée par une analyse précédente est remplacée)
            foreach (JugementCoup jugement in Jugements())
            {
                Coup coup = coups[jugement.IndexCoup];
                coup.EvaluationApres = jugement.EvaluationApres;
                coup.MeilleurCoup = jugement.MeilleurCoup;
                coup.MeilleurJoue = jugement.MeilleurJoue;
                coup.EvaluationMeilleur = jugement.EvaluationMeilleur;
                coup.PerteAnalyse = jugement.Perte;
                coup.MeilleurCoupLong = jugement.MeilleurCoupLong;
                coup.VarianteMeilleure = jugement.VarianteMeilleure;
                coup.VarianteLue = false;       // la variante est maintenant celle du moteur (plus celle du PGN chargé)
                // (celui d'une analyse précédente est remplacé, ou effacé) : l'ouverture, le sacrifice, la menace parée (ou non), la
                // menace créée par le coup, le piège, la finale, puis le changement d'appréciation
                // (pas l'appréciation après "Mat en 8 manqué" : elle ferait double emploi, choix de Bruno ; le commentaire d'un coup
                // qui n'est pas le meilleur ne peut être que celui du mat manqué, le sacrifice étant celui du meilleur coup)
                bool matManque = jugement.Commentaire != null && !jugement.MeilleurJoue;
                // (piège : seulement la 1re fois qu'une pièce est imprenable, pas au coup suivant du même camp s'il l'est encore :
                // la tour g6 de la partie de Bruno l'était cinq coups de suite)
                int k = _indexCoups.IndexOf(jugement.IndexCoup);
                string? piege = k >= 2 && _positions[k].CasePiege != null && _positions[k - 2].CasePiege == _positions[k].CasePiege
                    ? null : _positions[k].TextePiege;
                // (une erreur ? ou ?? : sa réfutation au lieu de la menace qu'elle crée, remarque de Bruno : « Menace : 28. Fxg7 » sur
                // 27. Fd4?? semblait bon pour les Blancs, et rien ne disait pourquoi le coup perdait)
                // (pas de réfutation après un mat manqué : le coup n'est pas puni, il laisse passer le mat, déjà dit)
                bool erreur = jugement.Perte >= JugementCoups.SeuilErreur && !jugement.MeilleurJoue;
                bool refutation = erreur && jugement.Commentaire == null;
                string? parade = CommentaireParade(k, jugement);
                string? menace = !refutation && !erreur && k + 1 < _positions.Count ? CommentaireMenace(k + 1) : null;
                string? refutationOuMenace = refutation ? CommentaireRefutation(k, parade) : menace;
                // Deux événements au plus par coup (choix de Bruno, 2026-10-28 : « Sacrifice du cavalier. Pare la menace 15... Cd5.
                // Menace : 16. Cxh8 », c'était trop), dans l'ordre d'importance : sacrifice, thème ou mat manqué, puis réfutation ou
                // menace, puis parade, puis piège ; l'ouverture, la finale et l'appréciation, courtes, restent
                HashSet<string> gardes = [.. new[] { jugement.Commentaire, refutationOuMenace, parade, piege }.OfType<string>().Take(EvenementsParCoup)];
                string? Garde(string? evenement) => evenement != null && gardes.Contains(evenement) ? evenement : null;
                coup.MenaceUci = Garde(menace) != null ? _positions[k + 1].MenaceUci : null;       // (sa flèche rouge)
                string?[] textes = [ouverture.GetValueOrDefault(jugement.IndexCoup), Garde(jugement.Commentaire), Garde(parade),
                                    Garde(refutationOuMenace), Garde(piege),
                                    finales.GetValueOrDefault(jugement.IndexCoup), matManque ? null : appreciations.GetValueOrDefault(jugement.IndexCoup)];
                coup.CommentaireAuto = textes.Any(t => t != null) ? string.Join(". ", textes.OfType<string>()) : null;
                // (le niveau du changement d'appréciation : son symbole dans le fichier PGN, voir Coup.NiveauAppreciation)
                coup.NiveauAppreciation = !matManque && niveaux.TryGetValue(jugement.IndexCoup, out var appreciation) ? appreciation.Niveau : null;
                coup.CommentaireTactique = jugement.Commentaire;    // (sacrifice, thème, mat manqué : l'essentiel, pour la feuille)
                // (la notation NAL du coup est en lettres anglaises : la notation longue française est calculée ici)
                PositionAnalysee positionAvant = _positions[_indexCoups.IndexOf(jugement.IndexCoup)];
                coup.MeilleurCoupUci = positionAvant.MeilleurCoupUci;
                coup.VarianteMeilleureUci = positionAvant.VarianteMeilleureUci;
                coup.CoupJoueLong = NotationLongue(positionAvant.Fen, coup.Uci.Trim(), coup.PgnFrSansNumero);
                if (coup.Annotation == "" || coup.AnnotationProposee)
                {
                    coup.Annotation = jugement.Annotation;
                    coup.AnnotationProposee = jugement.Annotation != "";
                }
            }
        }

        public BilanCamp Bilan(ColorPiece camp)
        {   // Imprécisions, erreurs, gaffes et précision moyenne (en %) des coups jugés du camp, hors coups de la bibliothèque
            List<JugementCoup> jugements = [.. Jugements().Where(j => j.Camp == camp && !EstCoupDeBibliotheque(j.IndexCoup))];
            int precision = jugements.Count == 0 ? 0 : (int)Math.Round(jugements.Average(j => j.Precision));
            return new BilanCamp(jugements.Count(j => j.Annotation == "?!"), jugements.Count(j => j.Annotation == "?"),
                                 jugements.Count(j => j.Annotation == "??"), precision);
        }

        public JugementCoup? CoupCritique() =>
            // Le moment où la partie a basculé : le coup qui a fait perdre le plus de chances de gain, s'il est au moins une erreur (?)
            // (null : aucun coup n'a vraiment changé le cours de la partie). À égalité, le premier
            Jugements().Where(j => j.Perte >= JugementCoups.SeuilErreur && !EstCoupDeBibliotheque(j.IndexCoup)).OrderByDescending(j => j.Perte).ThenBy(j => j.IndexCoup).FirstOrDefault();
    }
}
