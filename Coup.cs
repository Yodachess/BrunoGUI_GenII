// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Un coup de la partie, dans toutes ses notations (voir LogiqueMouvements.ListeCoups)
// ├─ Classe "Coup"
// │            └─ "PositionDeDepart"   Elément de départ d'une partie commencée à partir d'un FEN (sans coup)
// └─ Classe "VueCoups"                 Vue en lecture seule d'une notation (ex : LogiqueMouvements.ListeCoupsFen)

using System;
using System.Collections;
using System.Collections.Generic;

namespace BrunoGUI_GenII
{
    public class Coup
    {
        public string Fen { get; init; } = "";          // position APRES le coup (ou position de départ)
        public string PgnIntl { get; init; } = "";      // ex : "12. Nf3+ " (le numéro n'est présent que pour les coups blancs)
        public string PgnFr { get; init; } = "";        // ex : "12. Cf3+ "
        public string Nal { get; init; } = "";          // notation algébrique longue, ex : "12. Cg1-f3+ "
        public string Uci { get; init; } = "";          // ex : "g1f3 "
        public bool EstPositionDeDepart { get; init; }  // true : pas un coup, seulement la position de départ (partie chargée depuis un FEN)
        // Partie à la pendule : temps restant de chaque camp juste après ce coup (null sans pendule). Notés par l'interface après
        // le coup (la logique ne connaît pas la pendule) ; servent à remettre la pendule à l'heure (retour arrière, "Reprendre ici")
        public TimeSpan? TempsBlancs { get; set; }
        public TimeSpan? TempsNoirs { get; set; }
        public TimeSpan? TempsReflexion { get; set; }   // partie PGN chargée : temps passé sur ce coup ([%emt], ChessBase), null si inconnu
        public string Annotation { get; set; } = "";    // "!!", "!", "!?", "?!", "?", "??" (voir Annotations), "" : aucune
        // Analyse de partie (AnalysePartie.cs) : annotation proposée par le moteur (affichée plus pâle, jusqu'à ce que le joueur
        // en choisisse une), évaluation après le coup (point de vue des Blancs), meilleur coup du moteur dans la position d'avant
        public bool AnnotationProposee { get; set; }
        public Evaluation? EvaluationApres { get; set; }
        public string? MeilleurCoup { get; set; }
        public bool MeilleurJoue { get; set; }
        public Evaluation? EvaluationMeilleur { get; set; }     // évaluation du meilleur coup du moteur (position avant le coup)
        public double? PerteAnalyse { get; set; }               // chances de gain perdues par le coup (0 : aussi bon que le meilleur)
        public string? MeilleurCoupLong { get; set; }            // le meilleur coup avec sa case de départ ("Ta8-c8")
        public string? MeilleurCoupUci { get; set; }             // le meilleur coup au format UCI ("a8c8") : flèche sur l'échiquier
        public string? VarianteMeilleure { get; set; }           // la suite prévue par le moteur après le meilleur coup ("19 ... Tac8 20. Cf3")
        public string? VarianteMeilleureUci { get; set; }        // la même au format UCI ("a8c8 g1f3 ...") : rejouée sur l'échiquier (ligne 3)
        public string? CoupJoueLong { get; set; }                // le coup lui-même avec sa case de départ, en français ("Dd8-d7")
        public string? Commentaire { get; set; }                 // texte du commentaire du coup dans le PGN chargé (null : aucun), réécrit à l'enregistrement
        // Commentaire écrit par l'analyse de partie (ex : "Sacrifice du cavalier"), remplacé à chaque analyse ; dans le PGN, un bloc
        // à part marqué [%autocom], relu comme tel (jamais confondu avec le commentaire d'un joueur ou d'un auteur)
        public string? CommentaireAuto { get; set; }
        // La partie essentielle de ce commentaire, pour la feuille de partie (sacrifice, thème tactique, mat manqué) ; null : rien
        public string? CommentaireTactique { get; set; }
        public bool VarianteLue { get; set; }                   // la variante (meilleur coup) vient du PGN chargé : réécrite à l'enregistrement
        // Appréciation qui change après ce coup (-2 à +2, JugementCoups.NiveauAppreciation ; null : pas de changement), dont le texte
        // ("Avantage Blanc") termine CommentaireAuto ; dans le fichier PGN, elle s'écrit en symbole après le coup ($18 = +-, comme
        // ChessBase) et son texte est retiré du commentaire, puis refait à la relecture (choix de Bruno, 2026-10-28)
        public int? NiveauAppreciation { get; set; }
        // La menace créée par ce coup ("e5g6", celle de « Menace : 15. Cg6 ») : flèche rouge pendant le parcours, et [%cal Re5g6] dans
        // le bloc [%autocom] du fichier PGN (choix de Bruno, 2026-10-28, comme les flèches de ChessBase)
        public string? MenaceUci { get; set; }
        // Flèches lues dans le PGN (commandes [%cal] de ChessBase ou de Lichess, hors bloc de l'analyse) : "Re7h7,Oe5g6", une lettre
        // de couleur puis le coup ; montrées pendant le parcours et réécrites à l'enregistrement, comme le commentaire du joueur
        public string? FlechesAuteur { get; set; }

        public void EffaceAnalyse()
        {   // Au lancement d'une analyse de partie (choix de Bruno, 2026-10-21) : plus rien de l'analyse précédente, ou de celle du PGN
            // chargé ; sinon une analyse interrompue laissait un mélange (fin analysée de nouveau, début de l'ancienne). Restent
            // l'annotation et le commentaire du joueur (ou de l'auteur du PGN)
            EvaluationApres = EvaluationMeilleur = null;
            MeilleurCoup = MeilleurCoupLong = MeilleurCoupUci = VarianteMeilleure = VarianteMeilleureUci = CommentaireAuto = CommentaireTactique = null;
            MeilleurJoue = VarianteLue = false;
            PerteAnalyse = null;
            NiveauAppreciation = null;
            MenaceUci = null;
            if (AnnotationProposee)
                Annotation = "";
            AnnotationProposee = false;
        }

        public static Coup PositionDeDepart(string fen) => new() { Fen = fen, EstPositionDeDepart = true };

        // Après un coup blanc, ce sont les Noirs qui ont le trait (2e champ de la FEN de la position après le coup)
        public bool EstCoupBlanc => !EstPositionDeDepart && Fen.Split(' ')[1] == "b";
        // Numéro du coup complet : 12 pour "12. Cf3" comme pour "12... Fe7" (le 6e champ de la FEN augmente après chaque coup noir)
        public int NumeroDuCoup => int.Parse(Fen.Split(' ')[5]) - (EstCoupBlanc ? 0 : 1);
        // Le coup en français avec son numéro, blanc ou noir : "23. Ce6", "23... Fe7"
        public string PgnFrNumerote => EstCoupBlanc ? PgnFr.Trim() : $"{NumeroDuCoup}... {PgnFr.Trim()}";
        public string PgnFrSansNumero
        {
            get
            {
                string texte = PgnFr.Trim();
                return EstCoupBlanc && texte.Contains(' ') ? texte[(texte.IndexOf(' ') + 1)..] : texte;
            }
        }
    }

    public static class Annotations
    {   // Annotations d'un coup (symboles du PGN) : saisies par clic droit sur la feuille, écrites après le coup ("Ng5?!"),
        // relues depuis le coup lui-même ou depuis les codes $1 à $6 (Fritz, ChessBase)
        public static readonly string[] Toutes = ["!!", "!", "!?", "?!", "?", "??"];

        public static string DepuisNag(int nag) => nag switch
        {   // Codes NAG du PGN : $1 = !, $2 = ?, $3 = !!, $4 = ??, $5 = !?, $6 = ?! (les autres, ex : $14 "léger avantage blanc", sont ignorés)
            1 => "!", 2 => "?", 3 => "!!", 4 => "??", 5 => "!?", 6 => "?!", _ => ""
        };

        public static string Nom(string annotation) => annotation switch
        {
            "!!" => Langue.T("Coup brillant"), "!" => Langue.T("Bon coup"), "!?" => Langue.T("Coup intéressant"),
            "?!" => Langue.T("Coup douteux"), "?" => Langue.T("Mauvais coup"), "??" => Langue.T("Gaffe"), _ => Langue.T("Aucune annotation")
        };

        public static string NomCourt(string annotation) => annotation switch
        {   // Pour la feuille de partie et la 2e ligne sous l'échiquier, sans le mot « coup » qui prenait de la place (choix de
            // Bruno) ; le menu des annotations garde les noms complets
            "!!" => Langue.T("Brillant"), "!" => Langue.T("Bon"), "!?" => Langue.T("Intéressant"),
            "?!" => Langue.T("Douteux"), "?" => Langue.T("Mauvais"), "??" => Langue.T("Gaffe"), _ => Nom(annotation)
        };

        public static (string Coup, string Annotation) Separe(string coupPgn)
        {   // "Ng5?!" -> ("Ng5", "?!") ; une suite de ! et ? qui n'est pas une annotation connue (ex : "!!!") est retirée sans être gardée
            int fin = coupPgn.Length;
            while (fin > 0 && coupPgn[fin - 1] is '!' or '?')
                fin--;
            string annotation = coupPgn[fin..];
            return (coupPgn[..fin], Array.IndexOf(Toutes, annotation) >= 0 ? annotation : "");
        }
    }

    // Une ligne de la feuille de partie : numéro du coup, et place dans ListeCoups du coup blanc et du coup noir
    // (null : case vide, ex : "3. | ... | Cf6" pour une partie commencée par un coup noir, ou le coup noir pas encore joué) ;
    // ou une ligne de commentaire (Commentaire non null), placée sous le coup qu'elle commente (Commente : sa place)
    public record LigneFeuille(int Numero, int? Blanc, int? Noir, string? Commentaire = null, int? Commente = null)
    {
        public bool EstCommentaire => Commentaire != null;
    }

    public static class FeuilleDePartie
    {   // Mise en page de la feuille de partie (composant FeuilleCoups, à droite de l'échiquier) : une ligne par coup complet ;
        // avec "commentaire" (place dans la liste -> texte, ou null), une ligne de commentaire sous chaque coup commenté, à la façon
        // de Lichess et d'Europe Échecs (choix de Bruno) : après un coup blanc commenté, le coup noir repart sur sa propre ligne
        // ("13. | ... | Cxd5")
        public static List<LigneFeuille> Lignes(IReadOnlyList<Coup> coups, Func<int, string?>? commentaire = null)
        {
            List<LigneFeuille> lignes = [];
            for (int i = 0; i < coups.Count; i++)
            {
                Coup coup = coups[i];
                if (coup.EstPositionDeDepart)
                    continue;
                if (coup.EstCoupBlanc)
                    lignes.Add(new(coup.NumeroDuCoup, i, null));
                else if (lignes.Count > 0 && !lignes[^1].EstCommentaire && lignes[^1].Noir == null && lignes[^1].Numero == coup.NumeroDuCoup)
                    lignes[^1] = lignes[^1] with { Noir = i };
                else
                    lignes.Add(new(coup.NumeroDuCoup, null, i));
                if (commentaire?.Invoke(i) is { Length: > 0 } texte)
                    lignes.Add(new(coup.NumeroDuCoup, null, null, texte, i));
            }
            return lignes;
        }

        private const int EcartPionsNegligeable = 50;      // en centipions

        public static bool EcartNegligeable(Coup coup)
        {   // Le coup joué ne perd presque rien par rapport au meilleur : petit écart en chances de gain ET en pions. Dans une partie
            // déjà décidée, les chances ne bougent presque plus : 20. Fd2? de Smirin–Grischuk, -7.06 → -10.66, passait pour négligeable
            // (feuille « correct », 2e ligne « — moteur : », 3e « Suite du moteur »)
            bool ecartEnPionsPetit = coup.EvaluationMeilleur is Evaluation meilleure && coup.EvaluationApres is Evaluation apres
                && meilleure.MatEn == apres.MatEn && Math.Abs((meilleure.Centipions ?? 0) - (apres.Centipions ?? 0)) < EcartPionsNegligeable;
            return coup.PerteAnalyse is double perte && perte < JugementCoups.SeuilImprecision && ecartEnPionsPetit;
        }

        public static string? CommentaireEssentiel(Coup coup)
        {   // Le commentaire de la feuille de partie, seulement l'essentiel (choix de Bruno) : une erreur (?!, ?, ??) et le coup qu'il
            // fallait jouer ("Gaffe. Dc6 s'imposait.", PhraseErreur), puis le sacrifice, le thème tactique ou le mat manqué ; null : rien
            List<string> phrases = [];
            // (une annotation de l'auteur du PGN sur un coup que le moteur juge bon, ou presque : il n'y a pas mieux à proposer, et
            // la feuille dit le désaccord, choix B de Bruno, 16... f5? de Smirin–Grischuk, meilleur coup pour Stockfish)
            bool coupCorrect = coup.MeilleurJoue || EcartNegligeable(coup);
            if (coup.Annotation is "?!" or "?" or "??" && !coup.AnnotationProposee && coupCorrect && (coup.MeilleurJoue || coup.PerteAnalyse != null))
                phrases.Add(Langue.T("Le moteur juge ce coup correct."));
            else if (coup.Annotation is "?!" or "?" or "??" && !coup.MeilleurJoue && !string.IsNullOrEmpty(coup.MeilleurCoup) && !coupCorrect)
                phrases.Add(PhraseErreur(coup.Annotation, coup.Fen, Langue.Notation(coup.MeilleurCoup)));
            if (!string.IsNullOrEmpty(coup.CommentaireTactique))
                phrases.Add(coup.CommentaireTactique + ".");
            return phrases.Count > 0 ? string.Join(" ", phrases) : null;
        }

        private static string PhraseErreur(string annotation, string fen, string meilleur)
        {   // Une tournure parmi trois selon le demi-coup (choix de Bruno : "Gaffe. … était meilleur." répété à chaque ligne était
            // monotone) ; la même pour un coup donné, et trois (nombre impair) pour que les erreurs successives d'un même camp, deux
            // demi-coups plus loin, changent aussi de tournure
            string[] champs = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int numero = champs.Length > 5 && int.TryParse(champs[5], out int n) ? n : 1;
            int rang = (2 * numero + (champs.Length > 1 && champs[1] == "b" ? 1 : 0)) % 3;
            return (annotation, rang) switch
            {
                ("?!", 0) => Langue.T("Douteux. {0} était meilleur.", meilleur),
                ("?!", 1) => Langue.T("{0} était plus précis.", meilleur),
                ("?!", _) => Langue.T("Mieux valait {0}.", meilleur),
                ("?", 0) => Langue.T("Mauvais. {0} était meilleur.", meilleur),
                ("?", 1) => Langue.T("Erreur : il fallait jouer {0}.", meilleur),
                ("?", _) => Langue.T("{0} était bien meilleur.", meilleur),
                (_, 0) => Langue.T("Gaffe. {0} s'imposait.", meilleur),
                (_, 1) => Langue.T("Grosse erreur : il fallait jouer {0}.", meilleur),
                _ => Langue.T("Gaffe ! Le bon coup était {0}.", meilleur)
            };
        }
    }

    public class VueCoups(IReadOnlyList<Coup> coups, Func<Coup, string> notation) : IReadOnlyList<string>
    {   // Une notation de tous les coups, en lecture seule : les coups s'ajoutent et se retirent uniquement
        // par LogiqueMouvements (AjouteCoup, RetireDernierCoup, ViderCoups), ce qui garde toutes les notations alignées
        public string this[int index] => notation(coups[index]);
        public int Count => coups.Count;
        public IEnumerator<string> GetEnumerator()
        {
            foreach (Coup coup in coups)
                yield return notation(coup);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
