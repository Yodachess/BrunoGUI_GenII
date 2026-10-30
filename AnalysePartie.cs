// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Analyse de toute une partie par le moteur, sans interface graphique (testée dans Tests/Scenario.cs)
//  ├─ Classe "JugementCoups"    (JugementCoups.cs) : chances de gain, perte d'un coup, annotation ?! ? ?? (seuils de Lichess)
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
// La classe AnalyseDePartie est répartie en fichiers partiels : AnalysePartie.BonsCoups.cs (« ! », « !! », difficulté),
// AnalysePartie.Tactique.cs (sacrifices, thèmes tactiques, matériel), AnalysePartie.PiegesMenaces.cs (pièges et menaces).

using System;
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
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
        // Piège : une prise tentante, pas jouée, qui perdait ("La tour g6 est imprenable : 22. Fxg6? fxg6 …")
        public bool PiegeCherche { get; set; }
        public string? TextePiege { get; set; }
        public string? CasePiege { get; set; }     // case de la pièce imprenable (annoncée seulement la 1re fois : AppliqueAuxCoups)
        // Menace du joueur qui vient de jouer le coup menant à cette position : son coup suivant si l'adversaire
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
        // "Sacrifice du cavalier" : sans la case ni "offert" (« (offert en c8) » ne servait à rien)
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

    public partial class AnalyseDePartie
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
            // annoté, ni compté dans le bilan ni retenu comme coup critique (comme Lichess : la théorie n'est pas jugée)
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
            // rapide qu'un temps fixe, qui était trop long pour les positions faciles (mesuré sur les deux parties de référence, octobre
            // 2026 : 40 à 73 % de temps en moins à la profondeur 20, mêmes « ! », « !! », « ?? » et coup critique qu'à 10 s fixes).
            // Moins profond pour la théorie (position entre deux coups de la bibliothèque : ces coups ne sont pas jugés) ; plus
            // profond pour le 2e passage de l'analyse complète (profondeur 24, plafond 30 s : 13 à 28 % de temps en moins, mêmes
            // annotations importantes). Essayé puis retiré : moins profond quand la position suivante est déjà décidée ; c'est
            // justement la position d'avant une gaffe, où la perte doit être bien mesurée (le coup critique de la partie de référence
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
            // Partie déjà décidée, et toujours décidée dans le même sens après le coup : pas de ?!, ? ni ?? (comme ChessBase,
            // 2026-10-28 : 20... Cg8?! de Carlsen–Ernst, -5.81 → -9.79, n'apprenait rien). La perte compte toujours dans la
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
                // (pas l'appréciation après "Mat en 8 manqué" : elle ferait double emploi ; le commentaire d'un coup
                // qui n'est pas le meilleur ne peut être que celui du mat manqué, le sacrifice étant celui du meilleur coup)
                bool matManque = jugement.Commentaire != null && !jugement.MeilleurJoue;
                // (piège : seulement la 1re fois qu'une pièce est imprenable, pas au coup suivant du même camp s'il l'est encore :
                // la tour g6 de la partie de référence l'était cinq coups de suite)
                int k = _indexCoups.IndexOf(jugement.IndexCoup);
                string? piege = k >= 2 && _positions[k].CasePiege != null && _positions[k - 2].CasePiege == _positions[k].CasePiege
                    ? null : _positions[k].TextePiege;
                // (une erreur ? ou ?? : sa réfutation au lieu de la menace qu'elle crée : « Menace : 28. Fxg7 » sur
                // 27. Fd4?? semblait bon pour les Blancs, et rien ne disait pourquoi le coup perdait)
                // (pas de réfutation après un mat manqué : le coup n'est pas puni, il laisse passer le mat, déjà dit)
                bool erreur = jugement.Perte >= JugementCoups.SeuilErreur && !jugement.MeilleurJoue;
                bool refutation = erreur && jugement.Commentaire == null;
                string? parade = CommentaireParade(k, jugement);
                string? menace = !refutation && !erreur && k + 1 < _positions.Count ? CommentaireMenace(k + 1) : null;
                string? refutationOuMenace = refutation ? CommentaireRefutation(k, parade) : menace;
                // Deux événements au plus par coup (2026-10-28 : « Sacrifice du cavalier. Pare la menace 15... Cd5.
                // Menace : 16. Cxh8 », c'était trop), dans l'ordre d'importance : sacrifice, thème ou mat manqué, puis réfutation ou
                // menace, puis parade, puis piège ; l'ouverture, la finale et l'appréciation, courtes, restent
                HashSet<string> gardes = [.. new[] { jugement.Commentaire, refutationOuMenace, parade, piege }.OfType<string>().Take(EvenementsParCoup)];
                string? Garde(string? evenement) => evenement != null && gardes.Contains(evenement) ? evenement : null;
                coup.MenaceUci = Garde(menace) != null ? _positions[k + 1].MenaceUci : null;       // (sa flèche rouge)
                // (pas d'appréciation non plus après une réfutation : « Réfutation : 12. Cxe5. Gain Blanc », le gain allait de soi,
                // 2026-10-29)
                bool sansAppreciation = matManque || (refutation && Garde(refutationOuMenace) != null);
                string?[] textes = [ouverture.GetValueOrDefault(jugement.IndexCoup), Garde(jugement.Commentaire), Garde(parade),
                                    Garde(refutationOuMenace), Garde(piege),
                                    finales.GetValueOrDefault(jugement.IndexCoup), sansAppreciation ? null : appreciations.GetValueOrDefault(jugement.IndexCoup)];
                coup.CommentaireAuto = textes.Any(t => t != null) ? string.Join(". ", textes.OfType<string>()) : null;
                // (le niveau du changement d'appréciation : son symbole dans le fichier PGN, voir Coup.NiveauAppreciation ; aucun
                // sans son texte, sinon la relecture du PGN le referait)
                coup.NiveauAppreciation = !sansAppreciation && niveaux.TryGetValue(jugement.IndexCoup, out var appreciation) ? appreciation.Niveau : null;
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

        public string TexteBilan() =>
            // Le bilan affiché sous la feuille et écrit dans le PGN : "Blancs — précision 87 %" puis "   1 imprécision, 0 erreur,
            // 2 gaffes", pour chaque camp (explications dans l'infobulle du bilan)
            TexteBilan(Langue.T("Blancs"), Bilan(ColorPiece.Blanc)) + "\n" + TexteBilan(Langue.T("Noirs"), Bilan(ColorPiece.Noir));
        private static string TexteBilan(string camp, BilanCamp bilan) =>
            Langue.T("{0} — précision {1} %", camp, bilan.Precision) + "\n   " + Pluriel(bilan.Imprecisions, Langue.T("imprécision"), Langue.T("imprécisions"))
            + ", " + Pluriel(bilan.Erreurs, Langue.T("erreur"), Langue.T("erreurs")) + ", " + Pluriel(bilan.Gaffes, Langue.T("gaffe"), Langue.T("gaffes"));
        private static string Pluriel(int nombre, string singulier, string pluriel) =>
            // Pluriel au-delà de 1 en français ("0 erreur"), dès que le nombre n'est pas 1 en anglais ("0 mistakes")
            $"{nombre} {(nombre > 1 || (nombre == 0 && !Langue.EstFrancais) ? pluriel : singulier)}";

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
