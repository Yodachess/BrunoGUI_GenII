// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII est développé par Bruno COURTOIS.  Copyright © 2025 █
// █ BrunoGUI_GenII est gratuit, sauf s'il est utilisé commercialement  █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Divers outils qui encombreraient les autres fichiers ...
// └─ Classe "Chemins" qui gère les chemins d'accès
//              ├─ "RepertoireExecutable"
//              ├─ "RepertoireRacine"  
//              ├─ "BibliothèquesPolyglot"
//              └─ "MoteursUCI"
// └─ Classe "Outils"  
//              ├─ "AlgebriqueVersPgn"  
//              ├─ "EstCaseClaire"
//              ├─ "ChangerDeCoté"  
//              └─ "MiseaZeroListes"
// └─ Classe "Parametres"  
//              └─ "ChargerDepuisIni"

using System.Diagnostics;
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
            {
                // Dans un déploiement portable, le répertoire racine est celui où se trouve l'exécutable.
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
        public static string AlgebriqueVersPgn(string varianteBrute, int numeroDemiCoup)     // Retourne les coups sous la forme x. Db6 (format PGN en fait)
        {
            varianteBrute = varianteBrute.TrimStart();
            string[] variantePgnDecoupe = varianteBrute.Split(' ');         // Découpage des coups de la variante
            string stockeFen = LogiqueMouvements.RetourneChaineFenActuel(); // Récupérer le FEN actuel pour le remettre à la fin ? Obligé si on bouge les pièces !!
            string coupExamine = "";
            varianteBrute = "";
            if (LogiqueMouvements.QuiJoue == ColorPiece.Noir)                       //la PV commence par le coup Noir
                varianteBrute = ((numeroDemiCoup / 2) + 1).ToString() + " ...";     // On met le numéro du coup Noir
            for (int i = 0; i < variantePgnDecoupe.Length; i++)
            {
                if (variantePgnDecoupe[i] != "")    // Pour blinder le code (Au cas ou la découpe donne un élément vide)
                {
                    if (variantePgnDecoupe[i].Length >= 4)   // Le coup doit comporter source et destination, sinon crash ci dessous ...
                    {
                        string source = variantePgnDecoupe[i].Substring(0, 2);
                        string destination = variantePgnDecoupe[i].Substring(2, 2);
                        coupExamine = LogiqueMouvements.CoupNotationAlgebriquePGN(source, destination);    // On récupère le coup au format PGN comme Cd7
                        // *******Traitement promotion *********
                        if (variantePgnDecoupe[i].Length >= 5) // si promotion, on ajoute la pièce promue (ex : axb8=q)
                            coupExamine = coupExamine + char.ToUpper(variantePgnDecoupe[i][4]); // Note: la pièce promue doit être en majuscule 
                        // *******Traitement promotion *********
                        LogiqueMouvements.DeplacementPiece(RenvoieCaseIndex120(source), RenvoieCaseIndex120(destination), false);    // On fait le mouvement
                        ColorPiece couleurCoup = LogiqueMouvements.CouleurCase(RenvoieCaseIndex120(destination));
                        if (couleurCoup == ColorPiece.Noir)
                        {   //la PV commence par le coup Noir
                            varianteBrute = varianteBrute + " " + coupExamine;
                        }
                        if (couleurCoup == ColorPiece.Blanc)
                        {   // la PV commence par le coup Blanc
                            int numeroCoup = (numeroDemiCoup / 2) + 2;
                            if (numeroDemiCoup == 0)
                            {   // Si c'est le 1er coup Blanc, il faut mettre "1." et pas "2."
                                numeroCoup = 1;
                                numeroDemiCoup--;            // Et ajuster le numéro de 1/2 coup ... Sinon, il passe à 3 ??!!
                            }
                            varianteBrute = varianteBrute + " " + (numeroCoup) + ". " + coupExamine;
                        }
                        numeroDemiCoup++;
                    }
                }
            }
            LogiqueMouvements.MiseenplaceFen(stockeFen);        // et on réaffiche l'échiquier de départ
            return varianteBrute;
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
        public string CaseSombre { get; set; } = "CornflowerBlue";
        public string CaseClaire { get; set; } = "AliceBlue";
        public string NomHumain { get; set; } =  "Bruno";
        public string CouleurCaseSource { get; set; } = "Khaki";
        public string CouleurCaseDestination { get; set; } = "Gold";
        public int DureeReflexionSeconde { get; set; } = 3;
        public int ForceMoteur { get; set; } = 1850;
        public int NombreLignesPV { get; set; } = 3;
        public int NombreCoeursThread { get; set; } = 4;
        public string Bibliotheque { get; set; } = "rodent.bin";

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
                if (string.IsNullOrWhiteSpace(ligne) || ligne.StartsWith(";")) continue;

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

