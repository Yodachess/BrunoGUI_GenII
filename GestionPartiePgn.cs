// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Divers outils qui encombreraient les autres fichiers ...
// ├─ Classe "PartieEchecsPGN" qui décrit les balises du format PGN
// └─ Classe "GestionPartiePgn"  
//              ├─ "RetourneContenuPgn"  
//              ├─ "RetourneEntetePgn"
//              └─ "DecodeCoupPartie"
// └─ Classe "SaisieBalises" pour gestion des en-têtes PGN
//              ├─ "SaisieBalises"  
//              ├─ "InitializeComponents"  
//              ├─ "CreationBalisesTextBox"  
//              ├─ "SauveBalises_Click"
//              └─ "AnnulerBalises_Click"

using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using static BrunoGUI_GenII.Langue;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public class PartieEchecsPGN
    {   // Format de chaque partie qui se trouve dans ListeParties
        public string Tournoi { get; set; } = "";
        public string Lieu { get; set; } = "";
        public string Date { get; set; } = "";
        public string Ronde { get; set; } = "";
        public string White { get; set; } = "";
        public string Black { get; set; } = "";
        public string Result { get; set; } = "";
        public string ECO { get; set; } = "";
        public string WhiteElo { get; set; } = "";
        public string BlackElo { get; set; } = "";
        public string CompteDePLy { get; set; } = "";
        public string CoupsPartiePGN { get; set; } = "";
        public string Fen { get; set; } = "";        // position de départ ([SetUp "1"] [FEN "..."]) ; vide : position initiale
        public string TimeControl { get; set; } = "";// cadence ([TimeControl "180+2"], en secondes + bonus) ; vide : sans pendule
        public List<TimeSpan?> TempsCoups { get; set; } = [];  // temps de pendule après chaque coup ({[%clk h:mm:ss]}), dans l'ordre ; null : inconnu
        public List<TimeSpan?> TempsReflexion { get; set; } = [];  // temps passé sur chaque coup ({[%emt h:mm:ss]}, ChessBase) ; null : inconnu
        public List<string> Annotations { get; set; } = [];        // annotation de chaque coup ("!?", "??"... ; "" : aucune), dans l'ordre
        // Pour chaque coup (null : rien) : évaluation [%eval], texte de ses commentaires, et sa 1re variante "( ... )" telle qu'écrite
        public List<Evaluation?> Evaluations { get; set; } = [];
        public List<string?> Commentaires { get; set; } = [];
        public List<string?> Variantes { get; set; } = [];
        public List<bool> AnnotationsAuto { get; set; } = [];       // [%auto] : l'annotation du coup a été posée par l'analyse de partie
        public List<string?> CommentairesAuto { get; set; } = [];   // [%autocom] : texte écrit par l'analyse de partie (Coup.CommentaireAuto)
        public List<int?> Appreciations { get; set; } = [];         // appréciation lue en code NAG ($18 = +-…) : Coup.NiveauAppreciation
        // Bilan de l'analyse de partie, tel qu'affiché sous la feuille ("Blancs — précision 87 %\n   1 imprécision, …\nNoirs — …") ;
        // "" : pas d'analyse. Écrit après le dernier coup du fichier ({… [%bilan]}, comme la « valeur d'erreur » de ChessBase) et relu
        public string Bilan { get; set; } = "";
        public List<string?> FlechesAuteur { get; set; } = [];      // [%cal …] hors bloc de l'analyse : Coup.FlechesAuteur ("Re7h7,Oe5g6")
        public List<string?> FlechesMenace { get; set; } = [];      // [%cal …] du bloc [%autocom] : la flèche de la menace (Coup.MenaceUci)
        public List<string?> CommentairesTactiques { get; set; } = [];  // [%tac] : sa partie essentielle (Coup.CommentaireTactique)
    }

    public class GestionPartiePgn
    {   // Spécification détaillée du format PGN = https://fr.wikipedia.org/wiki/Portable_Game_Notation
        public static string RetourneContenuPgn(PartieEchecsPGN partieEnCours, string localisation, bool pourFichier = false)
        {   // Met au format Pgn la partieEnCours pour visualisation et sauvegarde ...
            // pourFichier : seulement dans le fichier enregistré, pas dans l'affichage, en commentaire après chaque coup,
            // l'évaluation de l'analyse de partie "[%eval 0.35]" et le temps restant de la pendule "[%clk 0:02:51]" (format de Lichess
            // et ChessBase) ; après un coup annoté ?!, ? ou ?? par l'analyse, la meilleure variante entre parenthèses (comme ChessBase)
            List<string> mots = [];
            bool apresCommentaire = false;      // un coup noir qui suit un commentaire ou une variante reprend son numéro ("1... e5")
            // Partie commençant par un coup noir (départ FEN, Noirs au trait) : le PGN exige "n... coup"
            Coup? premier = ListeCoups.FirstOrDefault(c => !c.EstPositionDeDepart);
            if (premier != null && !premier.EstCoupBlanc)
                apresCommentaire = true;
            for (int i = 0; i < ListeCoups.Count; i++)
            {
                Coup coup = ListeCoups[i];
                if (coup.EstPositionDeDepart)
                    continue;
                string texteCoup = (localisation == "Fr" ? ListeCoupsPgnFr[i] : ListeCoupsPgnIntl[i]).Trim() + coup.Annotation;  // "15. Ng5?!"
                if (!coup.EstCoupBlanc && apresCommentaire)
                    texteCoup = coup.NumeroDuCoup + "... " + texteCoup;
                apresCommentaire = false;
                List<string> commentaires = [];
                string? commentaireJoueur = string.IsNullOrWhiteSpace(coup.Commentaire) ? null : coup.Commentaire.Replace("{", "(").Replace("}", ")");
                string? texteAuto = coup.CommentaireAuto;
                if (pourFichier && coup.NiveauAppreciation is int niveau && texteAuto != null
                    && SansAppreciation(texteAuto, JugementCoups.TexteAppreciation(niveau, coup.EvaluationApres ?? new Evaluation(0, null))) is (true, var reste))
                {   // Changement d'appréciation : son symbole après le coup ("18... fxg6?? $18", comme ChessBase), à la place de son texte
                    // ("Gain Blanc"), refait à la relecture (2026-10-28)
                    texteCoup += " " + NagAppreciation(niveau);
                    texteAuto = reste;
                }
                // (flèches de l'auteur, lues dans le PGN : réécrites avec son commentaire, jamais dans le bloc de l'analyse)
                string? flechesAuteur = pourFichier && !string.IsNullOrWhiteSpace(coup.FlechesAuteur) ? $"[%cal {coup.FlechesAuteur}]" : null;
                if (!string.IsNullOrWhiteSpace(texteAuto))
                {   // Commentaire de l'analyse ("Sacrifice du cavalier") : lisible par les autres logiciels ; dans le fichier, marqué
                    // [%autocom] pour être relu comme tel, et celui du joueur va alors dans un bloc à part (chaque bloc est relu seul)
                    if (commentaireJoueur != null || flechesAuteur != null)
                        texteCoup += " {" + string.Join(" ", new[] { commentaireJoueur, flechesAuteur }.OfType<string>()) + "}";
                    commentaireJoueur = flechesAuteur = null;
                    // (et la place de sa partie essentielle, pour la feuille de partie à la relecture : "[%tac 0,21]" ; la flèche rouge
                    // de la menace, "[%cal Re5g6]", comme ChessBase)
                    int placeTactique = coup.CommentaireTactique is string tactique ? texteAuto.IndexOf(tactique) : -1;
                    commentaires.Add(texteAuto.Replace("{", "(").Replace("}", ")") + (pourFichier ? " [%autocom]" : "")
                        + (pourFichier && placeTactique >= 0 ? $" [%tac {placeTactique},{coup.CommentaireTactique!.Length}]" : "")
                        + (pourFichier && coup.MenaceUci is { Length: >= 4 } menace ? $" [%cal R{menace[..4]}]" : ""));
                }
                if (commentaireJoueur != null)      // commentaire du PGN chargé : gardé (fichier et affichage)
                    commentaires.Add(commentaireJoueur);
                if (flechesAuteur != null)
                    commentaires.Add(flechesAuteur);
                if (pourFichier && coup.AnnotationProposee && coup.Annotation != "")
                    commentaires.Add("[%auto]");    // annotation posée par l'analyse : elle redeviendra "proposée" à la relecture (remplaçable)
                TimeSpan? temps = coup.EstCoupBlanc ? coup.TempsBlancs : coup.TempsNoirs;     // pendule du camp qui vient de jouer
                if (pourFichier && coup.EvaluationApres is Evaluation evaluation)
                    commentaires.Add($"[%eval {TexteEvaluationPgn(evaluation)}]");
                if (pourFichier && temps is TimeSpan restant)
                    commentaires.Add($"[%clk {(int)restant.TotalHours}:{restant.Minutes:00}:{restant.Seconds:00}]");
                if (commentaires.Count > 0)
                {
                    texteCoup += " {" + string.Join(" ", commentaires) + "}";
                    apresCommentaire = true;
                }
                if (pourFichier && !coup.MeilleurJoue && !string.IsNullOrEmpty(coup.VarianteMeilleure)
                    && (coup.VarianteLue || coup.Annotation is "?!" or "?" or "??" || AlternativeAMontrer(coup)))
                {   // Erreur, ou coup nettement moins bon que le meilleur : ce qu'il fallait jouer, en variante courte avec le symbole de
                    // son évaluation, comme ChessBase ("(17. Qg3 $14 Kh8 18. Rd3)", 2026-10-28 ; elle partait sur 10
                    // demi-coups) ; ou variante du PGN chargé, gardée telle quelle (seule la 1re variante d'un coup est relue)
                    texteCoup += " (" + (coup.VarianteLue ? VarianteInternationale(coup.VarianteMeilleure)
                                                          : VarianteCourte(coup.VarianteMeilleure, coup.EvaluationMeilleur)) + ")";
                    apresCommentaire = true;
                }
                mots.Add(texteCoup);
            }
            if (pourFichier && !string.IsNullOrWhiteSpace(partieEnCours.Bilan) && mots.Count > 0)
                mots.Add("{" + BilanSurUneLigne(partieEnCours.Bilan) + " [%bilan]}");    // (après le dernier coup : relu comme bilan)
            mots.Add(ResultatPgn(partieEnCours));   // Rajout du résultat à la fin de la partie
            return RetourneEntetePgn(partieEnCours) + LignesDe80(string.Join(" ", mots));
        }

        public static string BilanSurUneLigne(string bilan) =>
            // Le bilan affiché (sur 4 lignes) en une phrase pour le PGN : "Blancs — précision 87 % : 1 imprécision, 0 erreur, 2 gaffes ;
            // Noirs — précision 62 % : …" (BilanDepuisLigne fait l'inverse à la relecture)
            bilan.Replace("{", "(").Replace("}", ")").Replace("\r", "").Replace("\n   ", " : ").Replace("\n", " ; ");
        public static string BilanDepuisLigne(string ligne) =>
            ligne.Replace(" ; ", "\n").Replace(" : ", "\n   ");

        public const int DemiCoupsVarianteCourte = 4;       // longueur des variantes du moteur dans le fichier PGN (ChessBase : 2 à 6)

        public static string VarianteCourte(string varianteFr, Evaluation? evaluation)
        {   // La variante du moteur limitée à DemiCoupsVarianteCourte demi-coups, en lettres anglaises, avec le symbole NAG de son
            // évaluation après son premier coup : "17... Qb6 $10 18. Qd3 c5 19. Qg3"
            List<string> mots = [];
            int demiCoups = 0;
            foreach (string mot in VarianteInternationale(varianteFr).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                bool estCoup = mot.Any(char.IsLetter);
                if (estCoup && demiCoups == DemiCoupsVarianteCourte)
                    break;
                mots.Add(mot);
                if (estCoup && ++demiCoups == 1 && evaluation is Evaluation symbole)
                    mots.Add(symbole.Nag);
            }
            while (mots.Count > 0 && !mots[^1].Any(char.IsLetter) && !mots[^1].StartsWith('$'))
                mots.RemoveAt(mots.Count - 1);      // (pas de numéro de coup seul à la fin)
            return string.Join(" ", mots);
        }

        public static bool AlternativeAMontrer(Coup coup)
        {   // Coup sans annotation mais nettement moins bon que le meilleur : ChessBase en donne l'alternative ("17. Qe2 (17. Qg3 ⩲)").
            // Au moins 0,07 de chances de gain ou un pion de perdu, dans une partie pas encore décidée de plus de 6 pions, et sans mat
            // (dans un mat ou une partie jouée d'avance, l'alternative n'apprend rien : 25. Td3 à +10 de Carlsen–Ernst)
            if (coup.MeilleurJoue || coup.Annotation != "" || coup.EvaluationMeilleur is not Evaluation meilleur
                || coup.EvaluationApres is not Evaluation apres || meilleur.MatEn != null || apres.MatEn != null
                || Math.Abs(meilleur.Centipions ?? 0) >= JugementCoups.PiegeJoueurPerdu)
                return false;
            int sens = coup.EstCoupBlanc ? 1 : -1;
            int pertePions = sens * ((meilleur.Centipions ?? 0) - (apres.Centipions ?? 0));
            return (coup.PerteAnalyse ?? 0) >= AlternativeChancesMinimum || pertePions >= AlternativePionsMinimum;
        }
        public const double AlternativeChancesMinimum = 0.07;
        public const int AlternativePionsMinimum = 100;     // en centipions

        public static string NagAppreciation(int niveau) => niveau switch
        {   // Les 5 niveaux de l'appréciation commentée (JugementCoups.NiveauAppreciation) en codes NAG : = (équilibré, léger avantage
            // compris), ± / ∓ (avantage), +- / -+ (avantage décisif ou mat)
            >= 2 => "$18", 1 => "$16", -1 => "$17", <= -2 => "$19", _ => "$10"     // (±3, mat forcé : aussi +- / -+)
        };
        public static int? NiveauDepuisNag(int nag) => nag switch
        {   // L'inverse, à la relecture ($14 / $15, « léger avantage », comptent pour l'équilibre, comme dans nos 5 niveaux ; les
            // autres codes ne sont pas des appréciations)
            18 => 2, 16 => 1, 10 or 11 or 14 or 15 => 0, 17 => -1, 19 => -2, _ => null
        };

        private static (bool Trouve, string? Reste) SansAppreciation(string texteAuto, string texteAppreciation)
        {   // Le commentaire de l'analyse sans le texte de l'appréciation, qui le termine ("Menace : … . Avantage Blanc") ; pas trouvé :
            // (false, …), le texte reste et le symbole n'est pas écrit
            if (texteAuto == texteAppreciation)
                return (true, null);
            return texteAuto.EndsWith(". " + texteAppreciation) ? (true, texteAuto[..^(texteAppreciation.Length + 2)]) : (false, null);
        }

        public static string LignesDe80(string texte)
        {   // Lignes de 80 caractères au plus, coupées entre deux mots (un commentaire ou une variante peut s'étendre sur plusieurs lignes)
            System.Text.StringBuilder resultat = new();
            int longueurLigne = 0;
            foreach (string mot in texte.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (longueurLigne > 0 && longueurLigne + 1 + mot.Length > 80)
                {
                    resultat.Append('\n');
                    longueurLigne = 0;
                }
                else if (longueurLigne > 0)
                {
                    resultat.Append(' ');
                    longueurLigne++;
                }
                resultat.Append(mot);
                longueurLigne += mot.Length;
            }
            return resultat.ToString();
        }

        public static string TexteEvaluationPgn(Evaluation evaluation) =>
            // Format [%eval] de Lichess et ChessBase : "0.35", "-9.05" (pions, point de vue des Blancs), "#3" ou "#-3" pour un mat
            evaluation.MatEn is int mat ? $"#{mat}"
            : ((evaluation.Centipions ?? 0) / 100m).ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

        public static string VarianteInternationale(string varianteFr)
        {   // Variante du moteur en notation française ("19 ... Tac8 20. Cf3") vers le PGN : lettres anglaises des pièces
            // (R→K, D→Q, T→R, F→B, C→N, y compris pour une promotion "=D") et "19..." pour un coup noir
            List<string> mots = [];
            foreach (string mot in (varianteFr ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (mot == "..." && mots.Count > 0 && mots[^1].All(char.IsDigit))
                    mots[^1] += "...";
                else
                    mots.Add(string.Concat(mot.Select(c => c switch { 'R' => 'K', 'D' => 'Q', 'T' => 'R', 'F' => 'B', 'C' => 'N', _ => c })));
            }
            return string.Join(" ", mots);
        }
        private static string ResultatPgn(PartieEchecsPGN partie) =>
            string.IsNullOrWhiteSpace(partie.Result) ? "*" : partie.Result;     // partie en cours : "*" (exigé par le format PGN)
        public static string RetourneEntetePgn(PartieEchecsPGN partieEnCours)
        {   // Retourne l'en-tête de la partie au format PGN, avec les balises obligatoires et optionnelles
            string enTetePgn = "";
            // Ajout de l'en-tête complet respectant le format PGN (mes Balises optionnelles préférées)
            enTetePgn = "[PlyCount \"" + partieEnCours.CompteDePLy + "\"]\n\n" + enTetePgn;  // Nombre de 1/2 coups
            enTetePgn = "[BlackElo \"" + partieEnCours.BlackElo + "\"]\n" + enTetePgn;    // Elo Noirs
            enTetePgn = "[WhiteElo \"" + partieEnCours.WhiteElo + "\"]\n" + enTetePgn;    // Elo Blancs
            enTetePgn = "[ECO \"" + partieEnCours.ECO + "\"]\n" + enTetePgn;              // Code ECO (ouverture) de la partie
            if (!string.IsNullOrWhiteSpace(partieEnCours.TimeControl))
                enTetePgn = "[TimeControl \"" + partieEnCours.TimeControl + "\"]\n" + enTetePgn;   // Cadence de la pendule
            // Partie commençant à une position (chargée depuis un FEN) : balises SetUp et FEN, sinon le fichier serait illisible
            if (ListeCoups.Count > 0 && ListeCoups[0].EstPositionDeDepart)
                enTetePgn = "[SetUp \"1\"]\n[FEN \"" + ListeCoups[0].Fen + "\"]\n" + enTetePgn;

            // Ajout de l'en-tête complet respectant le format PGN (Balises obligatoires)
            enTetePgn = "[Result \"" + ResultatPgn(partieEnCours) + "\"]\n" + enTetePgn;        // Résultat Partie
            enTetePgn = "[Black \"" + partieEnCours.Black + "\"]\n" + enTetePgn;          // Joueur noir
            enTetePgn = "[White \"" + partieEnCours.White + "\"]\n" + enTetePgn;          // Joueur blanc
            enTetePgn = "[Date \"" + partieEnCours.Date + "\"]\n" + enTetePgn;            // Date
            enTetePgn = "[Round \"" + partieEnCours.Ronde + "\"]\n" + enTetePgn;          // Numéro de la Ronde
            enTetePgn = "[Site \"" + partieEnCours.Lieu + "\"]\n" + enTetePgn;            // Lieu de la Partie
            enTetePgn = "[Event \"" + partieEnCours.Tournoi + "\"]\n" + enTetePgn;     // Nom du Tournoi
            return enTetePgn;
        }

        public static bool EstNumeroOuResultat(string element) =>
            element.EndsWith('.') || element is "1-0" or "0-1" or "1/2-1/2" or "*";

        public static bool DecodeCoupPartie(string coupPartie)
        {   // Décode UN coup au format Pgn en case source / destination et execute le coup, pour le camp au trait de la partie.
            // Renvoie false si le coup est illisible ou illégal (rien n'est joué) ; un numéro de coup ou un résultat est ignoré (true)
            if (string.IsNullOrWhiteSpace(coupPartie) || EstNumeroOuResultat(coupPartie))
                return true;
            try
            {
                return DecodeEtJoueCoup(coupPartie);
            }
            catch (Exception ex) when (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException || ex is FormatException)
            {   // coup trop mal formé pour être décodé (ex : "e9") : rien n'est joué
                return false;
            }
        }
        private static bool DecodeEtJoueCoup(string coupPartie)
        {
            char dernierCaractereCoup;
            TypePiece promotion = TypePiece.ReineBlanche;   // pièce de promotion (dame si le coup ne la précise pas ; couleur du pion)
            ColorPiece couleurQuiJoue;
            string CaseDestination, CaseSource;
            CaseSource = CaseDestination = "";
            TypePiece pieceQuiJoue = TypePiece.Vide;
            string CoupPGN = coupPartie;       // On récupère le coup pour pouvoir le traiter
            if (CoupPGN.StartsWith("0-0"))
                CoupPGN = CoupPGN.Replace('0', 'O');    // roque noté avec des zéros (0-0, 0-0-0) : forme officielle O-O
            if (CoupPGN.Length > 0)     // Il faut s'assurer qu'il y a au moins un coup, sinon erreur "L'index se trouve en dehors des limites du tableau."
            {
                couleurQuiJoue = QuiJoue;   // camp au trait de la partie (une partie FEN peut commencer par les Noirs)
                dernierCaractereCoup = CoupPGN[CoupPGN.Length - 1];    // Nettoyage des signes "+" et "#" à la fin du coup qui signalent les echecs
                // Enlève le "+" ou le "#" final pour avoir la case de destination dans les 2 derniers caractères
                // (l'échec et le mat sont calculés par ExecutionCoup : rien n'est forcé ici sur la position d'avant le coup)
                if (dernierCaractereCoup == '+' || dernierCaractereCoup == '#')
                    CoupPGN = CoupPGN.TrimEnd('+', '#');
                if (CoupPGN.Length < 2)
                    return false;
                if (CoupPGN.Length >= 3 && char.IsLower(CoupPGN[0]) && "QRBN".Contains(CoupPGN[^1]) && !CoupPGN.Contains('='))
                    CoupPGN = CoupPGN[..^1] + "=" + CoupPGN[^1];    // promotion sans "=" (vieux fichiers : "e8Q", "dxe8N") : "e8=Q"
                // Début du traitement du coup, il faut trouver la case de départ et de destination pour pouvoir executer le coup sur l'échiquier-
                switch (CoupPGN[0])                         // Coup de PIECE, car la 1ère lettre est une majuscule
                {                                           // On traite d'abord le Roi et le Roque, car plus simple
                    case 'K':       // Roi
                        CaseDestination = CoupPGN.Substring(CoupPGN.Length - 2, 2);     // La CaseDestination = 2 derniers caractères de CoupPGN
                        pieceQuiJoue = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.RoiBlanc : TypePiece.RoiNoir;
                        for (int i = 21; i <= 98; i++)
                            if (PiecesEchiquier[i] == pieceQuiJoue)
                            {
                                CaseSource = LogiqueMouvements.NomCaseAlgebrique(i);    // La CaseSource = Case où est le Roi qui joue
                            }
                        break;
                    case 'O':       // Roque
                        if (couleurQuiJoue == ColorPiece.Blanc)
                        {           // Grand Roque Blanc
                            if (CoupPGN == "O-O-O")             // Utiliser CoupPGN plutôt ??   DEBUG 31/01
                            {
                                CaseSource = "e1"; CaseDestination = "c1";
                            }
                            else
                            {       // Petit Roque Blanc
                                CaseSource = "e1"; CaseDestination = "g1";
                            }
                        }
                        if (couleurQuiJoue == ColorPiece.Noir)  // Utiliser CoupPGN plutôt ??   DEBUG 31/01
                        {           // Grand Roque Noir 
                            if (CoupPGN == "O-O-O")
                            {
                                CaseSource = "e8"; CaseDestination = "c8";
                            }
                            else
                            {       // Petit Roque Noir
                                CaseSource = "e8"; CaseDestination = "g8";
                            }
                        }
                        break;
                    default:
                        break;
                }

                if ("QBNR".Contains(CoupPGN[0]))            // Coup de PIECE, car la 1ère lettre est une majuscule
                {                                           // On traite les autres pièces, Dame, tour, Cavaliet et Fou
                    switch (CoupPGN[0])
                    {
                        case 'Q':       // Dame
                            CaseDestination = CoupPGN.Substring(CoupPGN.Length - 2, 2); // La CaseDestination = 2 derniers caractères de CoupPGN
                            pieceQuiJoue = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.ReineBlanche : TypePiece.ReineNoire;
                            break;
                        case 'R':       // Tour
                            CaseDestination = CoupPGN.Substring(CoupPGN.Length - 2, 2); // La CaseDestination = 2 derniers caractères de CoupPGN
                            pieceQuiJoue = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.TourBlanche : TypePiece.TourNoire;
                            break;
                        case 'N':       // Cavalier
                            CaseDestination = CoupPGN.Substring(CoupPGN.Length - 2, 2); // La CaseDestination = 2 derniers caractères de CoupPGN
                            pieceQuiJoue = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.CavalierBlanc : TypePiece.CavalierNoir;
                            break;
                        case 'B':       // Fou
                            CaseDestination = CoupPGN.Substring(CoupPGN.Length - 2, 2); // La CaseDestination = 2 derniers caractères de CoupPGN
                            pieceQuiJoue = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.FouBlanc : TypePiece.FouNoir;
                            break;
                    }
                    //      Partie commune
                    // On enlève le "x" de la prise si il existe : il reste la lettre de la pièce, la levée d'ambiguïté éventuelle
                    // (colonne, rangée ou case complète, ex : Nge2, R1a3, Qa1b2) et les 2 caractères de la destination
                    CoupPGN = CoupPGN.Replace("x", "");
                    string LeveeDeDoute = CoupPGN[1..^2];
                    // On ne retient que les coups LÉGAUX : une pièce clouée n'est pas candidate (et le PGN ne la mentionne pas)
                    foreach (var (source, destination) in LogiqueMouvements.CoupsLegaux())
                    {
                        if (destination != CaseDestination || PiecesEchiquier[RenvoieCaseIndex120(source)] != pieceQuiJoue)
                            continue;
                        if (LeveeDeDoute.All(c => source.Contains(c)))     // chaque caractère de la levée de doute correspond à la case de départ
                            CaseSource = source;
                    }
                }

                if (char.IsLower(CoupPGN[0]))               // COUP DE PION, car la 1ère lettre est une minuscule
                {
                    if (CoupPGN.Contains('='))              // PROMOTION
                    {   // PROMOTION : lettre anglaise de la pièce (Q, R, B, N), les mêmes qu'en UCI
                        promotion = LogiqueMouvements.PieceDePromotion(CoupPGN[^1], couleurQuiJoue);
                        CoupPGN = CoupPGN[..^2];    // On nettoie le coup de la promotion pour l'analyse qui suit
                    }
                    if (CoupPGN.Contains('x'))              // PRISE
                    {   // PRISE
                        CaseDestination = CoupPGN.Substring(CoupPGN.Length - 2, 2);       // La CaseDestination = 2 derniers caractères de CoupPGN
                        pieceQuiJoue = TypePiece.PionNoir;
                        if (CoupPGN[2].CompareTo(CoupPGN[0]) > 0)
                        {   // le caractère d'arrivée est après celui de départ dans l'ordre alphabétique
                            CaseSource = (couleurQuiJoue == ColorPiece.Blanc) ?
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) - 11) : // Le Pion est Blanc
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) + 9);  // Le Pion est Noir
                        }
                        else
                        {
                            CaseSource = couleurQuiJoue == ColorPiece.Blanc ?
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) - 9) :  // Le Pion est Blanc
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) + 11); // Le Pion est Noir
                        }
                        pieceQuiJoue = couleurQuiJoue == ColorPiece.Blanc ? TypePiece.PionBlanc : TypePiece.PionNoir;
                    }

                    if (!CoupPGN.Contains('x'))             // NI PROMOTION, NI PRISE
                    {   // NI PROMOTION, NI PRISE
                        CaseDestination = CoupPGN;
                        if (couleurQuiJoue == ColorPiece.Blanc)
                        {       // Le Pion est Blanc
                            pieceQuiJoue = TypePiece.PionBlanc;
                            CaseSource = PiecesEchiquier[RenvoieCaseIndex120(CaseDestination) - 10] == TypePiece.PionBlanc ?
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) - 10) : // Déplacement d'une case
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) - 20); // Déplacement de 2 cases
                        }
                        else
                        {   // Le Pion est Noir
                            pieceQuiJoue = TypePiece.PionNoir;
                            CaseSource = PiecesEchiquier[RenvoieCaseIndex120(CaseDestination) + 10] == TypePiece.PionNoir ?
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) + 10) : // Déplacement d'une case
                                LogiqueMouvements.NomCaseAlgebrique(RenvoieCaseIndex120(CaseDestination) + 20); // Déplacement de 2 cases
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(CaseSource) || string.IsNullOrWhiteSpace(CaseDestination)
                    || RenvoieCaseIndex120(CaseSource) < 0 || RenvoieCaseIndex120(CaseDestination) < 0)
                {   // Coup illisible (cases non déterminées) : rien n'est joué
                    return false;
                }
                LogiqueMouvements.ExecutionCoup(CaseSource, CaseDestination, promotion);   // (jamais de choix demandé au joueur)
                return LogiqueMouvements.CoupValide;    // false : coup illégal dans cette position
            }
            return false;
        }

    public class SaisieBalises : KryptonForm
        {
            private PartieEchecsPGN partieBalises;
            private KryptonTextBox tournamentCase;
            private KryptonTextBox lieuCase;
            private KryptonTextBox dateCase;
            private KryptonTextBox rondeCase;
            private KryptonTextBox blancsCase;
            private KryptonTextBox noirsCase;
            private KryptonTextBox resultCase;
            private KryptonTextBox ecoCase;
            private KryptonTextBox whiteeloCase;
            private KryptonTextBox blackeloCase;
            private KryptonTextBox plycountCase;
            private KryptonTextBox cadenceCase;
            private KryptonButton sauveEnTete;
            private KryptonButton annulerEnTete;

            public SaisieBalises(PartieEchecsPGN partie)
            {
                partieBalises = partie;
                InitializeComponents();
                this.StartPosition = FormStartPosition.CenterScreen;     // palette globale Krypton appliquée par défaut
            }
            [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(tournamentCase), nameof(lieuCase), nameof(dateCase), nameof(rondeCase),
                nameof(blancsCase), nameof(noirsCase), nameof(resultCase), nameof(ecoCase), nameof(whiteeloCase), nameof(blackeloCase),
                nameof(plycountCase), nameof(cadenceCase), nameof(sauveEnTete), nameof(annulerEnTete))]     // (tous créés ici)
            private void InitializeComponents()
            {
                // Création des champs de saisie
                // Les valeurs ne sont recopiées dans la partie qu'à l'enregistrement (SauveBalises_Click) : "Quitter" n'en garde aucune
                tournamentCase = CreationBalisesTextBox(T("Tournoi :"), 20, 10);
                lieuCase = CreationBalisesTextBox(T("Lieu :"), 20, 40);
                dateCase = CreationBalisesTextBox(T("Date :"), 20, 70);
                rondeCase = CreationBalisesTextBox(T("Ronde :"), 20, 100);
                blancsCase = CreationBalisesTextBox(T("Blancs :"), 20, 130);
                noirsCase = CreationBalisesTextBox(T("Noirs :"), 20, 160);
                resultCase = CreationBalisesTextBox(T("Résultat :"), 20, 190);
                ecoCase = CreationBalisesTextBox("ECO :", 20, 220);
                whiteeloCase = CreationBalisesTextBox(T("ELO Blancs :"), 20, 250);
                blackeloCase = CreationBalisesTextBox(T("ELO Noirs :"), 20, 280);
                plycountCase = CreationBalisesTextBox(T("Demi coups :"), 20, 310);
                cadenceCase = CreationBalisesTextBox(T("Cadence :"), 20, 340);      // TimeControl : "180+2" = 3 min + 2 s par coup
                // Pré-remplir les champs
                tournamentCase.Text = partieBalises.Tournoi;
                lieuCase.Text = partieBalises.Lieu;
                dateCase.Text = partieBalises.Date;
                rondeCase.Text = partieBalises.Ronde;
                blancsCase.Text = partieBalises.White;
                noirsCase.Text = partieBalises.Black;
                resultCase.Text = partieBalises.Result;
                ecoCase.Text = partieBalises.ECO;
                whiteeloCase.Text = partieBalises.WhiteElo;
                blackeloCase.Text = partieBalises.BlackElo;
                plycountCase.Text = partieBalises.CompteDePLy;
                cadenceCase.Text = partieBalises.TimeControl;
                // Boutons
                sauveEnTete = new KryptonButton
                {
                    Text = T("Enregistrer"),
                    Location = new Point(20, 380),
                    Width = 110
                };
                sauveEnTete.Click += SauveBalises_Click;
                this.Controls.Add(sauveEnTete);

                annulerEnTete = new KryptonButton
                {
                    Text = T("Quitter"),
                    Location = new Point(135, 380),
                    Width = 110
                };
                annulerEnTete.Click += AnnulerBalises_Click;
                this.Controls.Add(annulerEnTete);

                // Configuration de la fenêtre
                this.Text = T("Saisie des en-têtes de parties");
                this.Size = new Size(280, 450);
            }
            private static readonly Font PoliceLibelle = new("Arial", 10), PoliceChamp = new("Arial", 10, FontStyle.Bold | FontStyle.Italic);
            private KryptonTextBox CreationBalisesTextBox(string labelText, int x, int y)
            {
                KryptonLabel label = new()
                {
                    Text = labelText,
                    Location = new Point(x, y),
                };
                label.StateNormal.ShortText.Font = PoliceLibelle;   // (polices créées une fois : avant, deux par champ à chaque ouverture)
                this.Controls.Add(label);

                KryptonTextBox textBox = new()
                {
                    Location = new Point(x + 100, y),      // même colonne pour tous les champs, quelle que soit la langue du libellé
                    Width = 120,
                };
                textBox.StateCommon.Border.DrawBorders = PaletteDrawBorders.All; // ✅ Bordure
                textBox.StateCommon.Content.Font = PoliceChamp;
                this.Controls.Add(textBox);

                return textBox;
            }
            private void SauveBalises_Click(object? sender, EventArgs e)
            {   // Mettre à jour les valeurs de la partie
                partieBalises.Tournoi = tournamentCase.Text;
                partieBalises.Lieu = lieuCase.Text;
                partieBalises.Date = dateCase.Text;
                partieBalises.Ronde = rondeCase.Text;
                partieBalises.White = blancsCase.Text;
                partieBalises.Black = noirsCase.Text;
                partieBalises.Result = resultCase.Text;
                partieBalises.ECO = ecoCase.Text;
                partieBalises.WhiteElo = whiteeloCase.Text;
                partieBalises.BlackElo = blackeloCase.Text;
                partieBalises.CompteDePLy = plycountCase.Text;
                partieBalises.TimeControl = cadenceCase.Text.Trim();
                // Fermer la fenêtre
                this.Close();
            }
            private void AnnulerBalises_Click(object? sender, EventArgs e)
            {   // Fermer sans enregistrer
                this.Close();
            }
        }
    }
}








