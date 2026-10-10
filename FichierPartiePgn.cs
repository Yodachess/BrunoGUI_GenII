// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Gestion de fichier de partie au format PGN ...
//      └─ Classe "FichierPartiePgn" qui gère le fichier de partie
//                      ├─ "FichierPartiePgn"                   Init  
//                      ├─ "DecodeFichierPGN"                   Décodage du fichier      
//                      ├─ "DecodePartiePGN"                    Décodage de la partie     
//                      ├─ "SupprimeCommentaires"               Supprime les commentaires dans le pgn             
//                      ├─ "AfficherListeParties"               Affichage du tableau de parties               
//                      └─ "TableauPartiesPgn_CellDoubleClick"  Sélection de la partie

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using Krypton.Toolkit;

namespace BrunoGUI_GenII
{
    public static class ParseurPgn
    {   // Cette classe utilise une machine à états pour extraire proprement la section des coups
        // d'une partie PGN, en ignorant les en-têtes, les commentaires et les variantes.
        private enum Etat
        {
            EnTetes,
            Coups,
            Commentaire,
            Variante
        }
        public const string MarqueTemps = "%clk=";     // garderTemps : "{[%clk 0:02:51]}" devient le mot "%clk=0:02:51"
        public const string MarqueReflexion = "%emt="; // "{[%emt 0:30:11]}" (temps passé sur le coup, ChessBase) devient "%emt=0:30:11"
        public const string MarqueEvaluation = "%eval=";   // "{[%eval -0.60]}" devient "%eval=-0.60" ("#-3" pour un mat)
        public const string MarqueCommentaire = "%com=";   // texte d'un commentaire (sans ses [%...]), en base 64 (il contient des espaces)
        public const string MarqueVariante = "%var=";      // une variante "( ... )" de la partie principale, en base 64
        public const string MarqueAuto = "%auto=";         // "{[%auto]}" : annotation du coup posée par l'analyse de partie de BrunoGUI

        public static string ExtraireCoups(string pgn, bool garderTemps = false)
        {   // Cette méthode parcourt le PGN caractère par caractère et utilise une machine à états
            // pour déterminer si elle se trouve dans les en-têtes, les coups, les commentaires ou les variantes.
            // Les variantes peuvent être imbriquées "( ... ( ... ) ... )" : on compte la profondeur, sinon la fin d'une variante
            // intérieure ferait reprendre la variante extérieure comme si c'étaient des coups de la partie.
            // garderTemps : chaque commentaire de la partie principale est gardé, juste après son coup, sous la forme de mots :
            // temps de pendule "%clk=h:mm:ss", temps de réflexion "%emt=", évaluation "%eval=", et son texte "%com=" ;
            // chaque variante de la partie principale (avec ses sous-variantes, sans ses commentaires) devient un mot "%var="
            StringBuilder sb = new();
            StringBuilder commentaire = new();
            StringBuilder variante = new();     // variante de la partie principale en cours de lecture
            bool dansCommentaire = false;       // { ... }
            bool dansCommentaireLigne = false;  // ; ... jusqu'à la fin de la ligne
            int profondeurVariante = 0;
            for (int i = 0; i < pgn.Length; i++)
            {
                char c = pgn[i];
                if (dansCommentaire)
                {
                    if (c != '}')
                    {
                        commentaire.Append(c);
                        continue;
                    }
                    dansCommentaire = false;
                    if (garderTemps && profondeurVariante == 0)
                        AjouteCommentaire(sb, commentaire.ToString());
                    commentaire.Clear();
                    continue;
                }
                if (dansCommentaireLigne)
                {
                    if (c == '\n') { dansCommentaireLigne = false; sb.Append(' '); }
                    continue;
                }
                if (c == '{') { dansCommentaire = true; sb.Append(' '); continue; }   // espace : les coups de part et d'autre restent séparés
                if (c == ';') { dansCommentaireLigne = true; continue; }
                if (c == '(')
                {
                    if (profondeurVariante > 0)
                        variante.Append(c);
                    else
                        variante.Clear();
                    profondeurVariante++;
                    sb.Append(' ');
                    continue;
                }
                if (c == ')')
                {
                    if (profondeurVariante == 0)
                        continue;
                    profondeurVariante--;
                    if (profondeurVariante > 0)
                        variante.Append(c);
                    else if (garderTemps && variante.ToString().Trim() != "")
                        sb.Append(' ').Append(MarqueVariante).Append(EnBase64(variante.ToString().Trim())).Append(' ');
                    continue;
                }
                if (profondeurVariante > 0)
                {
                    variante.Append(c);
                    continue;
                }
                if (c == '[')
                {   // "enlève" les balises [Nom "valeur"]
                    while (i < pgn.Length && pgn[i] != ']')
                        i++;
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static void AjouteCommentaire(StringBuilder sb, string commentaire)
        {   // Un commentaire de la partie principale : ses commandes [%clk], [%emt], [%eval], puis son texte s'il en reste.
            // (Sauts de ligne retirés : ChessBase coupe parfois le temps en fin de ligne, ex : "[%emt 0:⏎00:47]")
            string texte = commentaire.Replace("\r", "").Replace("\n", " ");
            foreach (Match temps in Regex.Matches(texte.Replace(" ", ""), @"%(clk|emt)(\d+:\d{1,2}:\d{1,2}(?:\.\d+)?)"))
                sb.Append(' ').Append(temps.Groups[1].Value == "clk" ? MarqueTemps : MarqueReflexion).Append(temps.Groups[2].Value).Append(' ');
            if (Regex.IsMatch(texte, @"\[%auto\]"))
                sb.Append(' ').Append(MarqueAuto).Append("1 ");
            Match evaluation = Regex.Match(texte, @"\[%eval\s+(#?-?\d+(?:\.\d+)?)");
            if (evaluation.Success)
                sb.Append(' ').Append(MarqueEvaluation).Append(evaluation.Groups[1].Value).Append(' ');
            string libre = Regex.Replace(Regex.Replace(texte, @"\[%[^\]]*\]", " "), @"\s+", " ").Trim();
            if (libre != "")
                sb.Append(' ').Append(MarqueCommentaire).Append(EnBase64(libre)).Append(' ');
        }

        private static string EnBase64(string texte) => Convert.ToBase64String(Encoding.UTF8.GetBytes(texte));
        public static string DepuisBase64(string texte)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(texte)); }
            catch (FormatException) { return ""; }
        }

        public static Evaluation? LitEvaluation(string texte)
        {   // "0.35", "-9.05" (pions, point de vue des Blancs) ou "#3", "#-3" (mat) : le format [%eval] de Lichess et ChessBase
            if (texte.StartsWith('#'))
                return int.TryParse(texte[1..], out int mat) ? new Evaluation(null, mat) : null;
            return decimal.TryParse(texte, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal pions)
                ? new Evaluation((int)Math.Round(pions * 100), null) : null;
        }
    }

    public partial class FichierPartiePgn : Form
    {
        public FichierPartiePgn()
        {
            InitializeComponent();
            TraductionFenetres.Traduit(this);      // textes du designer dans la langue choisie (voir Langue.cs)
            FormClosing += (s, e) =>
            {   // Croix rouge : la fenêtre est seulement masquée (la fenêtre principale la réaffiche avec "Affiche liste parties")
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    Hide();
                }
            };
        }
        public static string LireTextePgn(string fichierPgn)
        {   // Beaucoup de fichiers PGN sont en Latin-1 (ISO-8859-1), pas en UTF-8, et certains mélangent les deux (parties de
            // plusieurs logiciels mises bout à bout). Chaque LIGNE est donc lue en UTF-8 strict si elle en est, sinon en Latin-1 :
            // relire tout le fichier en Latin-1 pour quelques lignes abîmerait les accents de toutes les autres ("Ã©"), et ferait
            // de la marque UTF-8 du début (BOM) un faux premier coup "ï»¿". Les marques UTF-8 (BOM) sont retirées partout
            return DecodeLignes(File.ReadAllBytes(fichierPgn));
        }
        public static string DecodeLignes(byte[] octets)
        {
            UTF8Encoding utf8Strict = new(false, true);
            StringBuilder texte = new();
            int debut = 0;
            for (int i = 0; i <= octets.Length; i++)
            {
                if (i < octets.Length && octets[i] != '\n')
                    continue;
                int longueur = i - debut + (i < octets.Length ? 1 : 0);     // la ligne avec son "\n"
                try
                {
                    texte.Append(utf8Strict.GetString(octets, debut, longueur));
                }
                catch (DecoderFallbackException)
                {
                    texte.Append(Encoding.Latin1.GetString(octets, debut, longueur));
                }
                debut = i + 1;
            }
            return texte.Replace("﻿", "").ToString();
        }
        public static List<string> DecodeFichierPGN(string fichierPgn)
        {   // --- On découpe le fichier PGN pour obtenir la liste des parties contenues dans le fichier. ---
            List<string> listeParties = [];

            using (StringReader lecteur = new(LireTextePgn(fichierPgn)))
            {
                string? ligne;
                StringBuilder partieCourante = new();

                while ((ligne = lecteur.ReadLine()) != null)
                {
                    ligne = ligne.Replace("\r", "");
                    if (!ligne.TrimStart().StartsWith('['))
                    {   // Lignes de coups seulement : on retire les ".." de "12..." (les annotations !, ?, !?... sont gardées :
                        // DecodePartiePGN les sépare des coups) ; les en-têtes restent intacts (ex : date "2024.??.??")
                        ligne = ligne.Replace("..", "");
                    }
                    if (ligne.StartsWith("[Event ")) // Avec un espace à la fin de Event, pour ne pas confondre avec le Tag EventDate ...
                    {
                        // Commencer une nouvelle partie
                        if (partieCourante.Length > 0)
                        {   // Ajouter la partie précédente à la liste si elle existe
                            listeParties.Add(partieCourante.ToString().Trim()); // Enlever l'espace final éventuel
                        }
                        // Réinitialiser partieCourante pour une nouvelle partie
                        partieCourante.Clear();
                    }
                    // Ajouter la ligne avec un saut de ligne explicite
                    partieCourante.AppendLine(ligne);
                }

                if (partieCourante.Length > 0)
                {   // Ajouter la dernière partie à la liste des parties
                    listeParties.Add(partieCourante.ToString().Trim());
                }
                Debug.WriteLine($"Nb parties décodées = {listeParties.Count}");
                /*
                foreach (var partie in listeParties)
                {   // Affichage des parties pour vérification
                    // Debug.WriteLine($"\nPartie décodée \n" + partie);
                }
                */
            }
            return listeParties;
        }
        public static PartieEchecsPGN DecodePartiePGN(string pgn)
        {   // --- On recoit UNE partie au format pgn avec balises et on remplit la structure PartieEchecsPgn ---
            List<string> balises = [];
            PartieEchecsPGN PartiePGN = new();
            // Debug.WriteLine($"pgn avant nettoyage : \n{pgn}");
            Regex regex = new(@"\[(.*?)\]");                      // Utilisation d'une expression régulière pour extraire les balises [ et ]
            MatchCollection correspondances = regex.Matches(pgn);       // Recherche de toutes les correspondances
            foreach (Match correspondance in correspondances)           // Ajout des balises trouvées à la liste "balises"
            {
                balises.Add(correspondance.Groups[1].Value);
            }
            foreach (var balise in balises)
            {   // Ce code suppose le format usuel pour les balises dans la partie PGN, c'est-à-dire que le nom de la balise est avant
                // le premier guillemet et que la valeur de la balise est entre les guillemets. 
                string[] parts = balise.Split('"');                 // Divise chaque balise en deux parties en utilisant le caractère ". 
                string NomBalise = parts[0].Trim();                 // Extrait le nom de la balise et le nettoie de tout espace indésirable.
                string ValeurBalise = parts.Length > 1 ? parts[1].Trim() : "";      // Extrait la valeur de la balise, si elle existe,
                                                                                    // et la nettoie également de tout espace indésirable.
                switch (NomBalise)
                {   // Met à jour les propriétés de la partie en fonction de la balise (je gère les 7 obligatoires + ECO + ELO + CompteDePLy)
                    case "Event":   //  Balise obligatoire
                        PartiePGN.Tournoi = ValeurBalise;
                        break;
                    case "Site":   //  Balise obligatoire
                        PartiePGN.Lieu = ValeurBalise;
                        break;
                    case "Date":   //  Balise obligatoire
                        PartiePGN.Date = ValeurBalise;
                        break;
                    case "Round":   //  Balise obligatoire
                        PartiePGN.Ronde = ValeurBalise;
                        break;
                    case "White":   //  Balise obligatoire
                        PartiePGN.White = ValeurBalise;
                        break;
                    case "Black":   //  Balise obligatoire
                        PartiePGN.Black = ValeurBalise;
                        break;
                    case "Result":   //  Balise obligatoire
                        PartiePGN.Result = ValeurBalise;
                        break;
                    case "ECO":
                        PartiePGN.ECO = ValeurBalise;
                        break;
                    case "WhiteElo":
                        PartiePGN.WhiteElo = ValeurBalise;
                        break;
                    case "BlackElo":
                        PartiePGN.BlackElo = ValeurBalise;
                        break;
                    case "PlyCount":   //  Balise qui m'intéresse
                        PartiePGN.CompteDePLy = ValeurBalise;
                        break;
                    case "FEN":         // partie qui commence à une position (avec [SetUp "1"])
                        PartiePGN.Fen = ValeurBalise;
                        break;
                    case "TimeControl": // cadence de la pendule (ex : "180+2")
                        PartiePGN.TimeControl = ValeurBalise;
                        break;
                    default:
                        break;
                }   // On se limite aux balises obligatoires + ECO + ELO + CompteDePLy, il en existe beaucoup d'autres
            }

            // --- EXTRACTION PROPRE VIA STATE MACHINE ---
            string sectionCoups = ParseurPgn.ExtraireCoups(pgn, garderTemps: true);

            sectionCoups = Regex.Replace(sectionCoups, @"\s+", " ").Trim();
            sectionCoups = sectionCoups.Replace("]", "");
            string[] tokens = sectionCoups.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<string> coupsPropres = [];

            PartiePGN.TempsCoups = [];
            PartiePGN.TempsReflexion = [];
            PartiePGN.Annotations = [];
            PartiePGN.Evaluations = [];
            PartiePGN.Commentaires = [];
            PartiePGN.Variantes = [];
            PartiePGN.AnnotationsAuto = [];
            foreach (var t in tokens)
            {
                string c = t;
                if (c.StartsWith(ParseurPgn.MarqueEvaluation) || c.StartsWith(ParseurPgn.MarqueCommentaire) || c.StartsWith(ParseurPgn.MarqueVariante)
                    || c.StartsWith(ParseurPgn.MarqueAuto))
                {   // Évaluation, commentaire, variante ou marque [%auto] du coup qui précède (avant le 1er coup : commentaire de la
                    // partie, ignoré) ; plusieurs commentaires se suivent, seule la 1re variante compte (c'est l'alternative au coup joué)
                    if (coupsPropres.Count == 0)
                        continue;
                    if (c.StartsWith(ParseurPgn.MarqueAuto))
                        PartiePGN.AnnotationsAuto[^1] = true;
                    else if (c.StartsWith(ParseurPgn.MarqueEvaluation))
                        PartiePGN.Evaluations[^1] = ParseurPgn.LitEvaluation(c[ParseurPgn.MarqueEvaluation.Length..]);
                    else if (c.StartsWith(ParseurPgn.MarqueCommentaire))
                    {
                        string texte = ParseurPgn.DepuisBase64(c[ParseurPgn.MarqueCommentaire.Length..]);
                        PartiePGN.Commentaires[^1] = PartiePGN.Commentaires[^1] == null ? texte : PartiePGN.Commentaires[^1] + " " + texte;
                    }
                    else
                        PartiePGN.Variantes[^1] ??= ParseurPgn.DepuisBase64(c[ParseurPgn.MarqueVariante.Length..]);
                    continue;
                }
                if (c.StartsWith(ParseurPgn.MarqueTemps) || c.StartsWith(ParseurPgn.MarqueReflexion))
                {   // Temps de pendule ([%clk h:mm:ss]) ou temps de réflexion ([%emt h:mm:ss]) du coup qui précède :
                    // un élément de TempsCoups et de TempsReflexion par coup gardé
                    bool pendule = c.StartsWith(ParseurPgn.MarqueTemps);
                    List<TimeSpan?> liste = pendule ? PartiePGN.TempsCoups : PartiePGN.TempsReflexion;
                    if (liste.Count > 0 && TimeSpan.TryParse(c[ParseurPgn.MarqueTemps.Length..], System.Globalization.CultureInfo.InvariantCulture, out TimeSpan temps))
                        liste[^1] = temps;
                    continue;
                }
                if (c == "1-0" || c == "0-1" || c == "1/2-1/2" || c == "*")
                {
                    PartiePGN.Result = c;   // On met à jour le résultat de la partie à partir de la section des coups,
                    continue;               // au cas où il serait différent de celui indiqué dans les balises
                }                           // (ce qui arrive parfois dans les fichiers PGN)
                if (Regex.IsMatch(c, @"^\d+\.+$"))      // numéro de coup : "12." ou "12..." (coup noir après un commentaire)
                    continue;
                if (c.StartsWith('$') || c.Trim('!', '?').Length == 0)
                {   // Annotation du coup qui précède : code NAG ($1 à $6 ; les autres, ex : $14, sont ignorés) ou symbole isolé ("e4 !")
                    string annotation = c.StartsWith('$') ? (int.TryParse(c[1..], out int nag) ? Annotations.DepuisNag(nag) : "")
                                                          : Annotations.Separe("x" + c).Annotation;
                    if (annotation != "" && PartiePGN.Annotations.Count > 0 && PartiePGN.Annotations[^1] == "")
                        PartiePGN.Annotations[^1] = annotation;
                    continue;
                }
                if (c.Contains('$'))
                    continue;
                var (coupSeul, annotationDuCoup) = Annotations.Separe(c);     // "Ng5?!" : le coup "Ng5", l'annotation "?!"
                if (coupSeul.Length < 2)
                    continue;
                coupsPropres.Add(coupSeul);
                PartiePGN.TempsCoups.Add(null);
                PartiePGN.TempsReflexion.Add(null);
                PartiePGN.Annotations.Add(annotationDuCoup);
                PartiePGN.Evaluations.Add(null);
                PartiePGN.Commentaires.Add(null);
                PartiePGN.Variantes.Add(null);
                PartiePGN.AnnotationsAuto.Add(false);
            }

            string final = string.Join(" ", coupsPropres);
            PartiePGN.CoupsPartiePGN = AjouteNumerosCoups(final);
            // Ajout du résultat à la fin de la liste de coups, car on l'aviat supprimé dans la boucle de nettoyage
            PartiePGN.CoupsPartiePGN = PartiePGN.CoupsPartiePGN + " " + PartiePGN.Result;
            
            Debug.WriteLine($"PGN nettoyé = {PartiePGN.CoupsPartiePGN}");
            return PartiePGN;
        }

        public static string AjouteNumerosCoups(string coupsSansNumeros)
        {   // Cette méthode prend une liste de coups sans numéros et ajoute les numéros de coups appropriés.
            var tokens = coupsSansNumeros.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new();
            int numero = 1;
            for (int i = 0; i < tokens.Length; i++)
            {
                string coup = tokens[i];
                if (coup == "1-0" || coup == "0-1" || coup == "1/2-1/2" || coup == "*")
                {
                    sb.Append(" " + coup);
                    break;
                }
                if (i % 2 == 0)
                {   // Coup blanc → on ajoute le numéro de coup
                    sb.Append($"{numero}. {coup}");
                }
                else
                {   // Coup noir → on ajoute juste le coup
                    sb.Append($" {coup}");
                    numero++;
                }
                if (i % 2 == 1)
                    sb.Append(' ');
            }
            return sb.ToString().Trim();
        }

        public static string SupprimeCommentaires(string chaine, char accoladeOuvrante, char accoladeFermante)
        {   /* Lorsqu'une accolade ouvrante est rencontrée, le niveau d'imbrication est augmenté de 1.
        Lorsqu'une accolade fermante est rencontrée et que le niveau d'imbrication est supérieur à 0, 
        cela signifie qu'elle correspond à une paire d'accolades imbriquées, donc le niveau d'imbrication est décrémenté de 1.
        Si une accolade fermante est rencontrée et que le niveau d'imbrication est déjà à 0, 
        cela signifie qu'elle est en dehors de toute paire d'accolades imbriquées, donc elle est conservée dans le résultat final.
        Les caractères qui ne sont pas situés entre des accolades imbriquées sont ajoutés au résultat final. */
            StringBuilder resultat = new();
            int niveauAccolade = 0;
            foreach (char caractere in chaine)
            {
                if (caractere == accoladeOuvrante)
                {
                    niveauAccolade++;
                }
                else if (caractere == accoladeFermante)
                {
                    if (niveauAccolade > 0)
                    {
                        niveauAccolade--;
                        // Ajout d'un espace pour éviter de coller deux coups après suppression
                        if (niveauAccolade == 0)
                            resultat.Append(' ');
                    }
                    else
                    {   // Cas anormal : accolade fermante sans ouvrante → on conserve
                        resultat.Append(caractere);
                    }
                }
                else if (niveauAccolade == 0)
                {
                    resultat.Append(caractere);
                }
            }
            // Sécurité : si un commentaire n'est pas refermé, on ne fait rien de plus
            // (le texte après aura été ignoré, comportement acceptable pour PGN corrompu)
            return resultat.ToString();
        }

        public void AfficherListeParties(List<PartieEchecsPGN> listeParties)
        {   // Affiche la liste des parties dans le DataGridView,
            // en utilisant les propriétés de chaque partie pour remplir les colonnes du tableau.
            Debug.WriteLine($"Chargement de {listeParties.Count} parties");     // Vérification du nombre de parties ajoutées
            if (listeParties == null || listeParties.Count == 0)                // Vérifier si la liste est vide
            {
                Debug.WriteLine("Aucune partie à afficher.");
                TableauPartiesPgn.DataSource = null; // Rien à afficher
                return;
            }
            TableauPartiesPgn.Rows.Clear();         // On vide explicitement toutes les lignes
            TableauPartiesPgn.DataSource = null;    // Réinitialisation complète du DataSource
            TableauPartiesPgn.RowHeadersVisible = false;    // supprime la colonne d’entêtes de lignes
            foreach (var partie in listeParties)    // Ajout manuel des données ligne par ligne (par mesure de sécurité)
            {   // Crée une nouvelle ligne avec les données de la partie
                int rowIndex = TableauPartiesPgn.Rows.Add(partie.White, partie.WhiteElo, partie.Black, partie.BlackElo, partie.Result, partie.CompteDePLy, 
                    partie.ECO, partie.Tournoi, partie.Ronde, partie.Lieu, partie.Date);
                TableauPartiesPgn.Rows[rowIndex].Tag = partie;      // Ajoute l'objet PartieEchecsPgn comme 'Tag' de la ligne
            }
            TableauPartiesPgn.Columns[0].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);  // Mise en valeur des joueurs
            TableauPartiesPgn.Columns[2].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);  // et du résultat dans le tableau
            TableauPartiesPgn.Columns[4].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);  
            TableauPartiesPgn.Refresh();        // Rafraîchir la DataGridView pour s'assurer que les données sont bien affichées
        }
        private void TableauPartiesPgn_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {   // Charge la partie sélectionnée par le double-click
            if (e.RowIndex >= 0)
            {   // Utilise 'Tag' pour récupérer l'objet complet
                var partie = TableauPartiesPgn.Rows[e.RowIndex].Tag as PartieEchecsPGN;
                if (partie != null && !string.IsNullOrWhiteSpace(partie.CoupsPartiePGN))
                {   // Récupére la fenêtre principale
                    var mainForm = Application.OpenForms["EchiquierPrincipal"] as EchiquierPrincipal;
                    if (mainForm != null)
                    {   // Charge la partie sélectionnée dans la fenêtre principale
                        Debug.WriteLine($"Chargement de la partie {partie.Tournoi} - {partie.White} vs {partie.Black}");
                        Debug.WriteLine($"Partie = {partie.CoupsPartiePGN}");
                        mainForm.MontrePartiesPGN_Click(null, EventArgs.Empty);
                        mainForm.ChargerPartieDepuisPgn(partie);
                        this.Hide();        // Masque la fenêtre FichierPartiePgn
                    }
                }
                else
                {
                    KryptonMessageBox.Show(Langue.T("La partie sélectionnée ne contient pas de coups"), Langue.T("Pas de coups dans la partie"), KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    Debug.WriteLine("Erreur : La partie = null !? (sans doute vide ...)");
                }
            }
        }
    }
}
