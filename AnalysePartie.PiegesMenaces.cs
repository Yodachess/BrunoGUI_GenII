// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Analyse de partie (suite de AnalysePartie.cs) : pièges et menaces, cherchés par le 2e moteur (MoteurAuxiliaire.Mesure)
//  ├─ "PiegesAChercher", "EnregistrePiege"     la prise tentante non jouée qui perdait (« La tour g6 est imprenable… »)
//  ├─ "MenacesAChercher", "EnregistreMenace"   ce que le joueur jouerait après un coup nul (« Menace : 15. Cxe7+ »)
//  └─ "CommentaireMenace", "CommentaireParade", "CommentaireRefutation"   leurs commentaires

using System;
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public partial class AnalyseDePartie
    {
        public List<(int IndexPosition, string Fen, string PriseUci)> PiegesAChercher()
        {   // Pour chaque coup jugeable, la prise la plus tentante (au moins SacrificeMinimum de matériel gagné à
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
                    // prend pas (perdu de moins, le piège reste instructif : 28. Fxg7 de la partie de référence, à -4.5, « le fou f3 est
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
            // fait perdre au moins une erreur (SeuilErreur) par rapport au meilleur coup : "La tour g6 est imprenable par le fou : 22.
            // Fxg6? fxg6 23. g4 Tc8 (-5.57)" (partie Parkhov–Bruzon : pourquoi 22. g4 et pas 22. Fxg6 ?)
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
            string cible = position.Pieces[RenvoieCaseIndex120(caseCible)] switch
            {
                TypePiece.ReineBlanche or TypePiece.ReineNoire => Langue.T("La dame {0}", caseCible),
                TypePiece.TourBlanche or TypePiece.TourNoire => Langue.T("La tour {0}", caseCible),
                TypePiece.FouBlanc or TypePiece.FouNoir => Langue.T("Le fou {0}", caseCible),
                TypePiece.CavalierBlanc or TypePiece.CavalierNoir => Langue.T("Le cavalier {0}", caseCible),
                _ => Langue.T("Le pion {0}", caseCible)
            };
            // (la pièce qui prend est nommée : « Le cavalier e5 est imprenable » sur 12. Cxe5, qui le prenait, semblait faux ;
            // c'est 12. dxe5? qui perdait, 2026-10-29)
            string preneur = position.Pieces[RenvoieCaseIndex120(priseUci[..2])] switch
            {
                TypePiece.ReineBlanche or TypePiece.ReineNoire => Langue.T("la dame"),
                TypePiece.TourBlanche or TypePiece.TourNoire => Langue.T("la tour"),
                TypePiece.FouBlanc or TypePiece.FouNoir => Langue.T("le fou"),
                TypePiece.CavalierBlanc or TypePiece.CavalierNoir => Langue.T("le cavalier"),
                TypePiece.RoiBlanc or TypePiece.RoiNoir => Langue.T("le roi"),
                _ => Langue.T("le pion")
            };
            avant.TextePiege = Langue.T("{0} est imprenable par {1} : {2}", cible, preneur, variante);
        }

        public List<(int IndexPosition, string FenCoupNul)> MenacesAChercher()
        {   // Pour chaque position analysée après un coup, la même position avec le trait rendu au joueur qui vient
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
            // (2026-10-28)
            bool calmeTresForte = gainEnChances && actuelle.Centipions is int cpAvant && menace.Centipions is int cpApres
                                  && sens * (cpApres - cpAvant) >= JugementCoups.MenaceCalmeMinimum;
            if (!concrete && !calmeTresForte)
                return;
            // Pas une menace créée par le coup : une pièce que le joueur pouvait déjà gagner avant de jouer (15... Cd5 de
            // Beliavsky–Larsen, « Menace : 16... fxg6 » : le cavalier g6 était déjà en prise, 2026-10-28)
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
    }
}
