// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Divers outils qui encombreraient les autres fichiers ...
// └─ Classe "Chemins" qui gère les chemins d'accès
//              ├─ "RepertoireExecutable"
//              ├─ "RepertoireRacine"  
//              ├─ "BibliothèquesPolyglot"
//              └─ "MoteursUCI"
// └─ Classe "Outils"  
//              ├─ "VarianteUciVersPgn"  
//              ├─ "EstCaseClaire"
//              ├─ "ChangerDeCoté"  
//              └─ "MiseaZeroListes"
// └─ Classe "Parametres"  
//              └─ "ChargerDepuisIni"

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Windows.Forms;     // Référence nécessaire si on utilise Application.StartupPath
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public static class Chemins
    {
        public static string RepertoireExecutable
        {   // Chemin du répertoire de l'exécutable
            get
            {
                string cheminExecutable = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(cheminExecutable);
            }
        }
        public static string RepertoireRacine
        {   // Chemin du répertoire racine du projet (BrunoGUI_Echecs)
            /*
            get
            {   // Remonte de 3 niveaux : bin\Debug\net8.0-windows\ → bin\Debug\ → bin\ → BrunoGUI_Echecs\
                string repertoireExecutable = RepertoireExecutable;
                return Directory.GetParent(Directory.GetParent(Directory.GetParent(repertoireExecutable).FullName).FullName).FullName;
            }
            */
            get
            {   // Dans un déploiement portable, le répertoire racine est celui où se trouve l'exécutable.
                // On retourne simplement le chemin du dossier où l'application a démarré.
                return Application.StartupPath;

                // --- OU ---
                // Si RepertoireExecutable est défini correctement comme le chemin complet du .exe :
                // return Path.GetDirectoryName(RepertoireExecutable);
            }
        }
        public static string BibliothèquesPolyglot
        {   // Chemin du répertoire BibliothèquesPolyglot
            get
            {
                return Path.Combine(RepertoireRacine, "BibliothèquesPolyglot");
            }
        }
        public static string MoteursUCI
        {   // Chemin du répertoire Moteurs_UCI
            get
            {
                return Path.Combine(RepertoireRacine, "Moteurs_UCI");
            }
        }
    }
    public class Outils
    {
        public static string VarianteUciVersPgn(string varianteBrute, int numeroDemiCoup, bool coupConseil)
        {   // Retourne les coups dans le format PGN (Cdxe4)
            // Si coupConseil = true, seul le dernier coup de la variante est retourné (avec son numéro)
            // Les coups sont joués sur une copie de la position : la partie en cours n'est ni modifiée, ni redessinée
            TypePiece promotionEnCours = PromotionPiece;
            try
            {
                return LogiqueMouvements.CalculerSurCopie(() => ConvertitVarianteUci(varianteBrute, numeroDemiCoup, coupConseil));
            }
            finally
            {
                PromotionPiece = promotionEnCours;
            }
        }
        private static string ConvertitVarianteUci(string varianteBrute, int numeroDemiCoup, bool coupConseil)
        {   // Joue la variante sur la position actuelle (qui doit être une copie, voir VarianteUciVersPgn)
            varianteBrute = varianteBrute.TrimStart();
            string[] varianteUciDecoupe = varianteBrute.Split(' ');         // Découpage des coups de la variante
            varianteBrute = "";

            if (LogiqueMouvements.QuiJoue == ColorPiece.Noir)                       // la PV commence par le coup Noir
                varianteBrute = ((numeroDemiCoup / 2) + 1).ToString() + " ...";     // On met le numéro du coup Noir

            for (int i = 0; i < varianteUciDecoupe.Length; i++)
            {
                if (varianteUciDecoupe[i] != "")    // Pour blinder le code (Au cas ou la découpe donne un élément vide)
                {
                    if (varianteUciDecoupe[i].Length >= 4)   // Le coup doit comporter source et destination, sinon crash ci dessous ...
                    {
                        string source = varianteUciDecoupe[i][..2];
                        string destination = varianteUciDecoupe[i].Substring(2, 2);
                        if (source.Length != 2 || destination.Length != 2)      // Sécurité supplémentaire
                            continue;

                        // *******Traitement promotion *********
                        PromotionPiece = TypePiece.Vide;        // Remise à zéro de la promotion
                        if (varianteUciDecoupe[i].Length >= 5)  // si promotion, on ajoute la pièce promue (ex : axb8=q)
                        {
                            char piecePromo = char.ToLower(varianteUciDecoupe[i][4]);
                            // Déterminer la couleur de la promotion
                            bool promotionBlanche = destination[1] == '8';
                            PromotionPiece = piecePromo switch
                            {   // Mise à jour de PromotionPiece
                                'q' => promotionBlanche ? TypePiece.ReineBlanche : TypePiece.ReineNoire,
                                'r' => promotionBlanche ? TypePiece.TourBlanche : TypePiece.TourNoire,
                                'b' => promotionBlanche ? TypePiece.FouBlanc : TypePiece.FouNoir,
                                'n' => promotionBlanche ? TypePiece.CavalierBlanc : TypePiece.CavalierNoir,
                                _ => TypePiece.Vide,
                            };
                        }
                        // Génération du coup PGN AVANT déplacement
                        string coupExaminePgn = LogiqueMouvements.CoupNotationAlgebriquePGN(source, destination);
                        if (PromotionPiece != TypePiece.Vide)
                        {   // Ajouter la pièce promue au PGN
                            coupExaminePgn += CaracterePieceLocale(char.ToLower(varianteUciDecoupe[i][4]));
                        }
                        // On fait le mouvement (avec la tour du roque et le pion pris en passant)
                        int indexSource = RenvoieCaseIndex120(source);
                        int indexDestination = RenvoieCaseIndex120(destination);
                        bool doublePasPion = System.Math.Abs(indexDestination - indexSource) == 20 &&
                            (PiecesEchiquier[indexSource] == TypePiece.PionBlanc || PiecesEchiquier[indexSource] == TypePiece.PionNoir);
                        LogiqueMouvements.SimuleCoup(indexSource, indexDestination);
                        IndexCaseEnPassant = doublePasPion ? (indexSource + indexDestination) / 2 : 0;   // pour le coup suivant de la variante
                        if (PromotionPiece != TypePiece.Vide)
                        {   // IMPORTANT : remplacer le pion par la pièce promue
                            LogiqueMouvements.PiecesEchiquier[RenvoieCaseIndex120(destination)] = PromotionPiece;
                        }
                        // *******Traitement promotion *********

                        LogiqueMouvements.CalculeEchecEtMat();  
                        if (EchecetMat)     // Affichage de "+" ou "#" après le coup, selon la situation
                            coupExaminePgn += "#";
                        else if (Echec)
                        {
                            coupExaminePgn += "+";
                        }
                        ColorPiece couleurCoup = LogiqueMouvements.CouleurCase(RenvoieCaseIndex120(destination));
                        if (coupConseil)
                        {   // Un seul coup à afficher, c'est le conseil du moteur : la variante reçue est "meilleurCoup conseil",
                            // on ne garde que le dernier coup, joué sur la position obtenue après le meilleur coup
                            int numeroCoup = (numeroDemiCoup / 2) + 2;
                            if (couleurCoup == ColorPiece.Noir)
                            {   // Le conseil est un coup Noir
                                varianteBrute = (numeroCoup).ToString() + " ... " + coupExaminePgn;
                            }
                            else
                            {   // Le conseil est un coup Blanc
                                varianteBrute = (numeroCoup).ToString() + ". " + coupExaminePgn;
                            }
                        }
                        else
                        {   // Affichage de toute la variante, coup par coup
                            if (couleurCoup == ColorPiece.Noir)
                            {   //la PV commence par le coup Noir
                                varianteBrute = varianteBrute + " " + coupExaminePgn;
                            }
                            if (couleurCoup == ColorPiece.Blanc)
                            {   // la PV commence par le coup Blanc
                                int numeroCoup = (numeroDemiCoup / 2) + 2;
                                if (numeroDemiCoup == 0)
                                {   // Si c'est le 1er coup Blanc, il faut mettre "1." et pas "2."
                                    numeroCoup = 1;
                                    numeroDemiCoup--;            // Et ajuster le numéro de 1/2 coup ... Sinon, il passe à 3 ??!!
                                }
                                varianteBrute = varianteBrute + " " + (numeroCoup) + ". " + coupExaminePgn;
                            }
                            numeroDemiCoup++;
                        }
                        // Passer au joueur suivant pour le prochain coup de la variante    // DEUBG 14/05/2026
                        LogiqueMouvements.QuiJoue =
                            (LogiqueMouvements.QuiJoue == ColorPiece.Blanc)
                            ? ColorPiece.Noir
                            : ColorPiece.Blanc;
                        // IMPORTANT : reset après chaque coup
                        PromotionPiece = TypePiece.Vide;
                    }
                }
            }
            return varianteBrute;
        }

        private static char CaracterePieceLocale(char lettreInitiale)
        {
            return char.ToUpper(lettreInitiale) switch
            {
                'R' => 'T',
                'N' => 'C',
                'B' => 'F',
                'Q' => 'D',
                'K' => 'R',
                _ => '\0',
            };
        }
        public static bool EstCaseClaire(int index120)
        {   // 👉 true = case claire, false = case sombre
            int rang = (index120 / 10) - 2;   // rang 0..7 (a1 = rang 0)
            int colonne = (index120 % 10) - 1; // colonne 0..7 (a1 = col 0)
            if (rang < 0 || rang > 7 || colonne < 0 || colonne > 7)
                return false; // bordure ou hors échiquier

            // a1 = sombre → donc (rang + colonne) % 2 == 0 → sombre
            // On inverse pour avoir clair/sombre selon ton choix :
            return ((rang + colonne) % 2 != 0);
        }
        public static void ChangerDeCoté()
        {
            QuiJoue = (QuiJoue == ColorPiece.Blanc) ? ColorPiece.Noir : ColorPiece.Blanc;
        }
        public static void MiseaZeroListes()
        {
            LogiqueMouvements.InitialisationEchiquier();
            LogiqueMouvements.ListeCoupsPgnIntl.Clear();    // Mise à zéro des liste de coups PGN International
            LogiqueMouvements.ListeCoupsPgnFr.Clear();      // Mise à zéro des liste de coups PGN Francais
            LogiqueMouvements.ListeCoupsFen.Clear();        // Mise à zéro des liste de coups FEN
            LogiqueMouvements.ListeCoupsNal.Clear();        // Mise à zéro des liste de coups Notation Algébrique Longue (Itnl)
            LogiqueMouvements.ListeCoupsUci.Clear();        // Mise à zéro des liste de coups UCI (protocole moteur)
            LogiqueMouvements.EchecetMat = LogiqueMouvements.Echec = false;
        }
    }
    public class Parametres
    {
        public string Moteur { get; set; } = "stockfish.exe";
        public string SiteMoteur { get; set; } = "";
        // Couleurs par défaut (si absentes du .ini ou illisibles) : style Lichess
        public const string LichessCaseSombre = "#B58863";              // RVB 181, 136, 99
        public const string LichessCaseClaire = "#F0D9B5";              // RVB 240, 217, 181
        public const string LichessCaseSource = "#86A66C";              // RVB 134, 166, 108
        public const string LichessCaseDestination = "#C4C87F";         // RVB 196, 200, 127
        public string CaseSombre { get; set; } = LichessCaseSombre;
        public string CaseClaire { get; set; } = LichessCaseClaire;
        public string NomHumain { get; set; } =  "Bruno";
        public string EloHumain { get; set; } = "1767";
        public string CouleurCaseSource { get; set; } = LichessCaseSource;
        public string CouleurCaseDestination { get; set; } = LichessCaseDestination;
        public int DureeReflexionSeconde { get; set; } = 3;
        public int ForceMoteur { get; set; } = 1850;
        public int NombreLignesPV { get; set; } = 3;
        public int NombreCoeursThread { get; set; } = 4;
        public string Bibliotheque { get; set; } = "rodent.bin";

        public static Color ConvertitCouleur(string valeur, string parDefaut)
        {   // Couleur du .ini : un nom de couleur .NET (ex : Peru) ou un code hexadécimal (ex : #B58863)
            // Si la valeur est illisible, on utilise la couleur par défaut
            try
            {
                Color couleur = valeur.StartsWith('#') ? ColorTranslator.FromHtml(valeur) : Color.FromName(valeur);
                if (couleur.IsKnownColor || valeur.StartsWith('#'))
                    return couleur;
            }
            catch (Exception) { }
            Debug.WriteLine($"[DEBUG] Couleur illisible dans le .ini : '{valeur}', utilisation de {parDefaut}");
            return ColorTranslator.FromHtml(parDefaut);
        }

        public void ChargerDepuisIni(string chemin)
        {
            if (!File.Exists(chemin))
            {
                Debug.WriteLine($"[DEBUG] Fichier .ini introuvable : {chemin}");
                return;
            }
            Debug.WriteLine($"[DEBUG] Fichier {chemin} trouvé !");
            foreach (string ligne in File.ReadAllLines(chemin))
            {
                if (string.IsNullOrWhiteSpace(ligne) || ligne.StartsWith(';')) continue;

                var parts = ligne.Split('=', 2);
                if (parts.Length != 2) continue;

                string cle = parts[0].Trim();
                string valeur = parts[1].Trim();

                switch (cle)
                {
                    case "Moteur": Moteur = valeur; break;
                    case "Sitemoteur": SiteMoteur = valeur; break;
                    case "Casesombre": CaseSombre = valeur; break;
                    case "Caseclaire": CaseClaire = valeur; break;
                    case "CouleurCaseSource": CouleurCaseSource = valeur; break;
                    case "CouleurCaseDestination": CouleurCaseDestination = valeur; break;
                    case "NomHumain": NomHumain = valeur; break;
                    case "EloHumain": EloHumain = valeur; break;
                    case "DureereflexionSeconde": DureeReflexionSeconde = int.Parse(valeur); break;
                    case "Forcemoteur": ForceMoteur = int.Parse(valeur); break;
                    case "NombrelignesPV": NombreLignesPV = int.Parse(valeur); break;
                    case "NombreCoeursThread": NombreCoeursThread = int.Parse(valeur); break;
                    case "Bibliotheque": Bibliotheque = valeur; break;
                }
            }
        }
    }
}

