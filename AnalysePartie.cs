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
        // "!" : le meilleur coup était le seul bon, la meilleure alternative (2e variante du moteur) aurait été au moins une erreur
        public const double SeuilSeulBonCoup = SeuilErreur;

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
        public string Fen { get; init; }
        public ColorPiece AuTrait { get; init; }
        public double? ChancesFinDePartie { get; init; }    // mat ou pat : +1, -1 ou 0, sans demander au moteur
        public Evaluation? Evaluation { get; set; }         // du point de vue des Blancs (null : pas encore analysée)
        public string MeilleurCoup { get; set; }            // premier coup de la meilleure variante, en notation française ("Cf3")
        public string MeilleurCoupUci { get; set; }         // le même au format UCI ("g1f3")
        public string MeilleurCoupLong { get; set; }        // le même avec sa case de départ ("Cg1-f3")
        public string VarianteMeilleure { get; set; }       // la meilleure variante en notation française ("12. Cf3 Fe7 13. ...")
        public Evaluation? EvaluationSeconde { get; set; }  // évaluation de la 2e variante du moteur (la meilleure alternative), null : inconnue
        public int NombreCoupsLegaux { get; init; }          // 1 : coup forcé (jamais "!")
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
                               string MeilleurCoup, bool MeilleurJoue, Evaluation? EvaluationMeilleur, string MeilleurCoupLong,
                               string VarianteMeilleure, double Precision);

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

        public AnalyseDePartie(IReadOnlyList<Coup> coups, bool approfondir = false)
        {   // Les positions de la partie : celle de départ (position initiale ou FEN de départ), puis celle après chaque coup.
            // approfondir : après le 1er passage, les positions avant et après chaque coup douteux (?!, ?, ??) sont revues plus
            // longtemps (voir EnApprofondissement) : un jugement sévère mérite d'être confirmé, et le temps n'est pas perdu sur les autres
            _coups = [.. coups];
            _approfondir = approfondir;
            string fenDepart = _coups.Count > 0 && _coups[0].EstPositionDeDepart ? _coups[0].Fen : FenDepart;
            _positions.Add(NouvellePosition(fenDepart));
            for (int i = 0; i < _coups.Count; i++)
                if (!_coups[i].EstPositionDeDepart)
                {
                    _indexCoups.Add(i);
                    _positions.Add(NouvellePosition(_coups[i].Fen));
                }
        }

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
            foreach (JugementCoup jugement in Jugements().Where(j => j.Perte >= JugementCoups.SeuilImprecision).ToList())
            {
                int k = _indexCoups.IndexOf(jugement.IndexCoup);
                foreach (PositionAnalysee position in new[] { _positions[k], _positions[k + 1] })
                    position.AApprofondir = position.ChancesFinDePartie == null;   // (mat ou pat : rien à revoir)
            }
        }

        public void Enregistre(int indexPosition, LigneAnalyse meilleure, LigneAnalyse seconde = null)
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
            position.MeilleurCoupUci = (meilleure.VarianteUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            position.MeilleurCoupLong = NotationLongue(position.Fen, position.MeilleurCoupUci, position.MeilleurCoup);
        }

        public static string NotationLongue(string fen, string coupUci, string coupPgn = null)
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

        public static string PremierCoup(string variantePgn) =>
            // "12. Cf3 Fe7" -> "Cf3" ; "19 ... Fa2 20. Cf3" (Noirs au trait) -> "Fa2" : le premier mot qui contient une lettre
            // (les numéros "19", "19.", "19..." et les "..." sont sautés)
            (variantePgn ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(m => m.Any(char.IsLetter));

        public JugementCoup Jugement(int indexCoup)
        {   // Le coup n° indexCoup de ListeCoups : null s'il n'en est pas un, ou si les positions avant et après ne sont pas analysées
            int k = _indexCoups.IndexOf(indexCoup);
            if (k < 0 || _positions[k].Chances is not double avant || _positions[k + 1].Chances is not double apres)
                return null;
            PositionAnalysee positionAvant = _positions[k];
            ColorPiece camp = positionAvant.AuTrait;
            string meilleur = positionAvant.MeilleurCoup;
            // Meilleur coup joué ? Comparé au format UCI s'il est connu (sans ambiguïté), sinon en notation
            bool meilleurJoue = positionAvant.MeilleurCoupUci != null
                ? positionAvant.MeilleurCoupUci == _coups[indexCoup].Uci.Trim()
                : meilleur != null && SansSymboles(meilleur) == SansSymboles(_coups[indexCoup].PgnFrSansNumero);
            // Le meilleur coup du moteur ne perd rien (l'écart d'évaluation entre deux recherches ne serait que du bruit)
            double sens = camp == ColorPiece.Blanc ? 1 : -1;                // chances vues du camp qui joue
            double perte = meilleurJoue ? 0 : Math.Max(0, sens * (avant - apres));
            double precision = meilleurJoue ? 100 : JugementCoups.Precision(sens * avant, sens * apres);
            string annotation = meilleurJoue && SeulBonCoup(k, indexCoup, sens) ? "!" : JugementCoups.Annotation(perte);
            return new JugementCoup(indexCoup, camp, perte, annotation, _positions[k + 1].Evaluation, meilleur, meilleurJoue,
                                    positionAvant.Evaluation, positionAvant.MeilleurCoupLong, positionAvant.VarianteMeilleure, precision);
        }

        private bool SeulBonCoup(int k, int indexCoup, double sens)
        {   // "!" (bon coup, choix de Bruno) : le meilleur coup a été joué ET c'était le seul bon : la 2e variante du moteur aurait fait
            // perdre au moins JugementCoups.SeuilSeulBonCoup de chances de gain (une erreur). Exclus : un coup forcé (un seul coup
            // légal) et une reprise immédiate (on reprend sur la case où l'adversaire vient de prendre : rien de remarquable)
            PositionAnalysee avant = _positions[k];
            if (avant.EvaluationSeconde is not Evaluation seconde || avant.Chances is not double chances || avant.NombreCoupsLegaux <= 1)
                return false;
            if (k > 0 && EstReprise(k, indexCoup))
                return false;
            return sens * (chances - JugementCoups.ChancesDeGain(seconde)) >= JugementCoups.SeuilSeulBonCoup;
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

        public IEnumerable<JugementCoup> Jugements() => _indexCoups.Select(Jugement).Where(j => j != null);

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
                coup.CoupJoueLong = NotationLongue(positionAvant.Fen, coup.Uci.Trim(), coup.PgnFrSansNumero);
                if (coup.Annotation == "" || coup.AnnotationProposee)
                {
                    coup.Annotation = jugement.Annotation;
                    coup.AnnotationProposee = jugement.Annotation != "";
                }
            }
        }

        public BilanCamp Bilan(ColorPiece camp)
        {   // Imprécisions, erreurs, gaffes et précision moyenne (en %) des coups jugés du camp
            List<JugementCoup> jugements = [.. Jugements().Where(j => j.Camp == camp)];
            int precision = jugements.Count == 0 ? 0 : (int)Math.Round(jugements.Average(j => j.Precision));
            return new BilanCamp(jugements.Count(j => j.Annotation == "?!"), jugements.Count(j => j.Annotation == "?"),
                                 jugements.Count(j => j.Annotation == "??"), precision);
        }

        public JugementCoup CoupCritique() =>
            // Le moment où la partie a basculé : le coup qui a fait perdre le plus de chances de gain, s'il est au moins une erreur (?)
            // (null : aucun coup n'a vraiment changé le cours de la partie). À égalité, le premier
            Jugements().Where(j => j.Perte >= JugementCoups.SeuilErreur).OrderByDescending(j => j.Perte).ThenBy(j => j.IndexCoup).FirstOrDefault();
    }
}
