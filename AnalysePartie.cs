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
                               string? VarianteMeilleure, double Precision);

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

        public AnalyseDePartie(IReadOnlyList<Coup> coups, bool approfondir = false, Func<string, string, bool>? coupDeBibliotheque = null)
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
        }

        private readonly HashSet<int> _coupsDeBibliotheque = [];    // place dans ListeCoups des coups connus de la bibliothèque
        public bool EstCoupDeBibliotheque(int indexCoup) => _coupsDeBibliotheque.Contains(indexCoup);

        private static PositionAnalysee NouvellePosition(string fen)
        {   // Mat ou pat : la position est jugée tout de suite (le moteur n'aurait aucun coup à proposer)
            Position position = PositionDepuisFen(fen);
            double? fin = null;
            if (CalculerSur(position, () => !ResteCoupsValidesJouables()))
                fin = !CalculerSur(position, CampAuTraitEnEchec) ? 0 : position.QuiJoue == ColorPiece.Blanc ? -1 : 1;
            return new PositionAnalysee { Fen = fen, AuTrait = position.QuiJoue, ChancesFinDePartie = fin,
                                          NombreCoupsLegaux = CalculerSur(position, () => CoupsLegaux().Count) };
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
            string annotation = EstCoupDeBibliotheque(indexCoup) ? ""
                              : meilleurJoue ? BonCoup(k, indexCoup, sens) : JugementCoups.Annotation(perte);
            return new JugementCoup(indexCoup, camp, perte, annotation, _positions[k + 1].Evaluation, meilleur, meilleurJoue,
                                    positionAvant.Evaluation, positionAvant.MeilleurCoupLong, positionAvant.VarianteMeilleure, precision);
        }

        private string BonCoup(int k, int indexCoup, double sens)
        {   // Le meilleur coup a été joué : "!", "!!" ou rien.
            // Difficulté inconnue (pas de 2e moteur) : "!" au seul bon coup (l'alternative est une erreur), "!!" s'il sacrifie.
            // Difficulté mesurée (choix de Bruno, octobre 2026) : un sacrifice correct a "!", et "!!" s'il était difficile à trouver
            // (13. Cd5!! : une pièce, trouvé à la profondeur 12 ; il n'est pas "le seul bon coup", une alternative restait à +0.11) ;
            // sans sacrifice, "!" seulement au seul bon coup difficile à trouver (21. Th3!) ; un seul bon coup naturel n'a rien
            // (29. gxf3, 15... O-O, 47. Rd4)
            if (_positions[k].ProfondeurDecouverte is not int profondeur)
                return SeulBonCoup(k, indexCoup, sens) ? (Sacrifie(k) ? "!!" : "!") : "";
            bool difficile = profondeur >= JugementCoups.ProfondeurCoupDifficile;
            if (SacrificeCorrect(k, indexCoup, sens))
                return difficile ? "!!" : "!";
            return difficile && SeulBonCoup(k, indexCoup, sens) ? "!" : "";
        }

        private bool SacrificeCorrect(int k, int indexCoup, double sens)
        {   // Le meilleur coup (déjà vérifié) sacrifie, et le joueur n'est pas perdu après lui. Dans une position déjà gagnée, donner du
            // matériel pour conclure n'est remarquable que si c'est le seul coup qui gagne (25... Cfe4!! après la gaffe 25. Ff5??)
            if (_positions[k + 1].Chances is not double apres || sens * apres < -JugementCoups.LimitePartieDecidee)
                return false;
            if (sens * apres > JugementCoups.LimitePartieDecidee && !SeulBonCoup(k, indexCoup, sens))
                return false;
            return Sacrifie(k);
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
                if (SeulBonCoup(k, indexCoup, sens) || SacrificeCorrect(k, indexCoup, sens))
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
            if (k > 0 && EstReprise(k, indexCoup))
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
            bool menacee = CalculerSur(avant, () =>
            {   // attaques adverses sur la case de départ : coups légaux de l'adversaire, en lui donnant le trait sur la copie
                QuiJoue = Adversaire(QuiJoue);
                return CoupsLegaux().Any(c => c.Destination == depart && JugementCoups.Valeur(PiecesEchiquier[RenvoieCaseIndex120(c.Source)]) < valeur);
            });
            if (!menacee)
                return false;
            List<string> casesSures = [.. CalculerSur(avant, () => CoupsLegaux().Where(c => c.Source == depart).Select(c => c.Destination).ToList())
                                          .Where(destination => CaseSure(avant, depart, destination, valeur))];
            return casesSures.Count == 1 && casesSures[0] == arrivee;
        }

        private static bool CaseSure(Position avant, string depart, string destination, int valeur)
        {   // Après le coup (sur une copie), la pièce n'y est attaquée par aucune pièce adverse de moindre valeur, et, si elle y est
            // attaquée, elle y est défendue (une pièce de même valeur qui la prend sera reprise : simple échange)
            return CalculerSur(avant, () =>
            {
                int indexDestination = RenvoieCaseIndex120(destination);
                ColorPiece camp = QuiJoue;
                SimuleCoup(RenvoieCaseIndex120(depart), indexDestination);
                QuiJoue = Adversaire(camp);
                List<int> attaquants = [.. CoupsLegaux().Where(c => c.Destination == destination)
                                                        .Select(c => JugementCoups.Valeur(PiecesEchiquier[RenvoieCaseIndex120(c.Source)]))];
                if (attaquants.Any(v => v < valeur))
                    return false;
                if (attaquants.Count == 0)
                    return true;
                // Défendue ? On met un pion adverse à sa place : une pièce du camp qui peut le prendre défend la case
                PiecesEchiquier[indexDestination] = camp == ColorPiece.Blanc ? TypePiece.PionNoir : TypePiece.PionBlanc;
                QuiJoue = camp;
                return CoupsLegaux().Any(c => c.Destination == destination);
            });
        }

        private bool Sacrifie(int k)
        {   // Le meilleur coup sacrifie du matériel. La variante du moteur (qui commence par ce coup) est jouée sur une copie : à la
            // PREMIÈRE position calme après la réponse de l'adversaire (le demi-coup suivant de la variante n'est pas une prise, ou la
            // variante s'arrête), dans les DemiCoupsSacrifice premiers demi-coups, le joueur a au moins SacrificeMinimum points de
            // matériel de moins qu'avant son coup. Au milieu d'une suite d'échanges, le matériel ne veut rien dire (27... Fxf3 28. Fxg7
            // Fxg2 29. Rxg2 Rxg7 : à égalité au bout) ; et un sacrifice plus loin dans la variante est celui d'un AUTRE coup
            // (12. The1 b4 13. Cd5 exd5 : c'est 13. Cd5 qui sacrifie)
            PositionAnalysee avant = _positions[k];
            string[] variante = (avant.VarianteMeilleureUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (variante.Length < 2)
                return false;
            ColorPiece camp = avant.AuTrait;
            return CalculerSur(PositionDepuisFen(avant.Fen), () =>
            {
                int depart = Materiel(camp);
                for (int i = 0; i < Math.Min(variante.Length, JugementCoups.DemiCoupsSacrifice + 1); i++)
                {
                    if (variante[i].Length < 4)
                        return false;
                    ChargementPartie.JoueSurLaCopie(variante[i]);
                    bool calme = i + 1 >= variante.Length || !EstUnePrise(variante[i + 1]);
                    if (i >= 1 && calme)
                        return Materiel(camp) <= depart - JugementCoups.SacrificeMinimum;
                }
                return false;
            });
        }

        private static bool EstUnePrise(string coupUci)
        {   // Sur la position actuelle (une copie) : la case d'arrivée est occupée, ou un pion change de colonne (prise en passant)
            if (coupUci.Length < 4)
                return false;
            TypePiece piece = PiecesEchiquier[RenvoieCaseIndex120(coupUci[..2])];
            return PiecesEchiquier[RenvoieCaseIndex120(coupUci.Substring(2, 2))] != TypePiece.Vide
                || (piece is TypePiece.PionBlanc or TypePiece.PionNoir && coupUci[0] != coupUci[2]);
        }

        private static int Materiel(ColorPiece camp)
        {   // Matériel du camp moins celui de l'adversaire, sur la position actuelle (une copie) ; pièces blanches : valeurs impaires
            int bilan = 0;
            for (int i = 21; i <= 98; i++)
            {
                TypePiece piece = PiecesEchiquier[i];
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

        public void AppliqueAuxCoups(IReadOnlyList<Coup> coups)
        {   // Range les résultats dans les coups jugés (même liste qu'à la construction) : évaluation, meilleur coup, et annotation
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
