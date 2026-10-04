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
//              ├─ "PositionSuivante"   prochaine position à demander au moteur (null : analyse finie)
//              ├─ "Enregistre"         résultat du moteur pour une position (sa meilleure variante)
//              ├─ "Jugement"           le coup n° i de ListeCoups : perte, annotation proposée, meilleur coup
//              └─ "Bilan"              imprécisions, erreurs, gaffes et perte moyenne d'un camp
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

        public static int Centipions(double chances) =>
            // Inverse de ChancesDeGain (pour la perte moyenne en centipions), borné à ±1000
            Math.Abs(chances) >= 0.999 ? Math.Sign(chances) * 1000
            : (int)Math.Clamp(Math.Round(-Math.Log(2 / (chances + 1) - 1) / 0.00368208), -1000, 1000);
    }

    public sealed class PositionAnalysee
    {
        public string Fen { get; init; }
        public ColorPiece AuTrait { get; init; }
        public double? ChancesFinDePartie { get; init; }    // mat ou pat : +1, -1 ou 0, sans demander au moteur
        public Evaluation? Evaluation { get; set; }         // du point de vue des Blancs (null : pas encore analysée)
        public string MeilleurCoup { get; set; }            // premier coup de la meilleure variante, en notation française ("Cf3")
        public bool Ignoree { get; set; }                   // le moteur n'a donné aucun score : position sautée (sinon l'analyse tournerait en rond)
        public bool Analysee => Evaluation != null || ChancesFinDePartie != null || Ignoree;
        public double? Chances => ChancesFinDePartie ?? (Evaluation is Evaluation e ? JugementCoups.ChancesDeGain(e) : null);
    }

    // Ce que le moteur pense du coup n° IndexCoup de ListeCoups (Perte : chances de gain perdues par le camp qui l'a joué)
    public record JugementCoup(int IndexCoup, ColorPiece Camp, double Perte, string Annotation, Evaluation? EvaluationApres,
                               string MeilleurCoup, bool MeilleurJoue);

    public record BilanCamp(int Imprecisions, int Erreurs, int Gaffes, int PerteMoyenne);   // perte moyenne en centipions par coup

    public class AnalyseDePartie
    {
        private readonly List<PositionAnalysee> _positions = [];    // [0] : avant le 1er coup ; [k] : après le k-ième coup joué
        private readonly List<int> _indexCoups = [];                // place dans ListeCoups du k-ième coup joué
        private readonly List<Coup> _coups;

        public IReadOnlyList<PositionAnalysee> Positions => _positions;

        public AnalyseDePartie(IReadOnlyList<Coup> coups)
        {   // Les positions de la partie : celle de départ (position initiale ou FEN de départ), puis celle après chaque coup
            _coups = [.. coups];
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
            return new PositionAnalysee { Fen = fen, AuTrait = position.QuiJoue, ChancesFinDePartie = fin };
        }

        public int? PositionSuivante
        {   // Prochaine position à faire analyser par le moteur (null : toutes le sont)
            get
            {
                int index = _positions.FindIndex(p => !p.Analysee);
                return index < 0 ? null : index;
            }
        }
        public int NombreAAnalyser => _positions.Count(p => p.ChancesFinDePartie == null);
        public int NombreAnalysees => _positions.Count(p => p.ChancesFinDePartie == null && p.Evaluation != null);
        public bool Terminee => PositionSuivante == null;

        public void Enregistre(int indexPosition, LigneAnalyse meilleure)
        {   // Résultat du moteur pour la position (sa meilleure variante) ; sans score, la position est sautée (ses coups ne seront pas jugés)
            if (meilleure?.Evaluation is not Evaluation evaluation)
            {
                _positions[indexPosition].Ignoree = true;
                return;
            }
            _positions[indexPosition].Evaluation = evaluation;
            _positions[indexPosition].MeilleurCoup = PremierCoup(meilleure.VariantePgn);
        }

        public static string PremierCoup(string variantePgn) =>
            // "12. Cf3 Fe7" -> "Cf3" ; "19 ... Fa2 20. Cf3" (Noirs au trait) -> "Fa2" : le premier mot qui contient une lettre
            // (les numéros "19", "19.", "19..." et les "..." sont sautés)
            (variantePgn ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(m => m.Any(char.IsLetter));

        public JugementCoup Jugement(int indexCoup)
        {   // Le coup n° indexCoup de ListeCoups : null s'il n'en est pas un, ou si les positions avant et après ne sont pas analysées
            int k = _indexCoups.IndexOf(indexCoup);
            if (k < 0 || _positions[k].Chances is not double avant || _positions[k + 1].Chances is not double apres)
                return null;
            ColorPiece camp = _positions[k].AuTrait;
            string meilleur = _positions[k].MeilleurCoup;
            bool meilleurJoue = meilleur != null && SansSymboles(meilleur) == SansSymboles(_coups[indexCoup].PgnFrSansNumero);
            // Le meilleur coup du moteur ne perd rien (l'écart d'évaluation entre deux recherches ne serait que du bruit)
            double perte = meilleurJoue ? 0 : Math.Max(0, camp == ColorPiece.Blanc ? avant - apres : apres - avant);
            return new JugementCoup(indexCoup, camp, perte, JugementCoups.Annotation(perte), _positions[k + 1].Evaluation, meilleur, meilleurJoue);
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
                if (coup.Annotation == "" || coup.AnnotationProposee)
                {
                    coup.Annotation = jugement.Annotation;
                    coup.AnnotationProposee = jugement.Annotation != "";
                }
            }
        }

        public BilanCamp Bilan(ColorPiece camp)
        {   // Imprécisions, erreurs, gaffes et perte moyenne (en centipions, à partir des chances de gain) des coups jugés du camp
            List<JugementCoup> jugements = [.. Jugements().Where(j => j.Camp == camp)];
            int perteMoyenne = jugements.Count == 0 ? 0 : (int)Math.Round(jugements.Average(j =>
            {
                int k = _indexCoups.IndexOf(j.IndexCoup);
                int avant = JugementCoups.Centipions(_positions[k].Chances.Value), apres = JugementCoups.Centipions(_positions[k + 1].Chances.Value);
                return j.MeilleurJoue ? 0 : Math.Max(0, camp == ColorPiece.Blanc ? avant - apres : apres - avant);
            }));
            return new BilanCamp(jugements.Count(j => j.Annotation == "?!"), jugements.Count(j => j.Annotation == "?"),
                                 jugements.Count(j => j.Annotation == "??"), perteMoyenne);
        }
    }
}
