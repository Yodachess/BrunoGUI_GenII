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
using ComponentFactory.Krypton.Toolkit;

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
        public static string ExtraireCoups(string pgn)
        {   // Cette méthode parcourt le PGN caractère par caractère et utilise une machine à états
            // pour déterminer si elle se trouve dans les en-têtes, les coups, les commentaires ou les variantes.
            StringBuilder sb = new();
            bool dansCommentaires = false;
            bool dansVariante = false;
            for (int i = 0; i < pgn.Length; i++)
            {
                char c = pgn[i];
                // "enlève" les crochets des balises
                if (c == '[')
                {
                    while (i < pgn.Length && pgn[i] != ']')
                        i++;
                    continue;
                }
                // commentaires
                if (c == '{') { dansCommentaires = true; continue; }
                if (c == '}') { dansCommentaires = false; continue; }
                if (dansCommentaires) continue;
                // variantes
                if (c == '(') { dansVariante = true; continue; }
                if (c == ')') { dansVariante = false; continue; }
                if (dansVariante) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }
    }

    public partial class FichierPartiePgn : Form
    {
        public FichierPartiePgn()
        {
            InitializeComponent();
        }
        public static List<string> DecodeFichierPGN(string fichierPgn)
        {   // --- On découpe le fichier PGN pour obtenir la liste des parties contenues dans le fichier. ---
            List<string> listeParties = [];

            using (StreamReader lecteur = new(fichierPgn, Encoding.UTF8))
            {   // Note : StreamReader attend le chemin d'accès au fichier, pas le contenu du fichier.
                string ligne;
                StringBuilder partieCourante = new();

                while ((ligne = lecteur.ReadLine()) != null)
                {
                    ligne = ligne.Replace("\r", "").Replace("?", "").Replace("!", "").Replace("..", "");    // Tentaive de nettoyage
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
                    default:
                        break;
                }   // On se limite aux balises obligatoires + ECO + ELO + CompteDePLy, il en existe beaucoup d'autres
            }

            // --- EXTRACTION PROPRE VIA STATE MACHINE ---
            string sectionCoups = ParseurPgn.ExtraireCoups(pgn);

            sectionCoups = Regex.Replace(sectionCoups, @"\s+", " ").Trim();
            sectionCoups = sectionCoups.Replace("]", "");
            string[] tokens = sectionCoups.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<string> coupsPropres = [];

            foreach (var t in tokens)
            {
                string c = t;
                if (c == "1-0" || c == "0-1" || c == "1/2-1/2" || c == "*")
                {
                    PartiePGN.Result = c;   // On met à jour le résultat de la partie à partir de la section des coups,
                    continue;               // au cas où il serait différent de celui indiqué dans les balises
                }                           // (ce qui arrive parfois dans les fichiers PGN)
                if (Regex.IsMatch(c, @"^\d+\.$"))
                    continue;
                if (c.Contains('$'))
                    continue;
                if (c.Length < 2)
                    continue;
                coupsPropres.Add(c);
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
        private void TableauPartiesPgn_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {   // Charge la partie sélectionnée par le double-click
            if (e.RowIndex >= 0)
            {   // Utilise 'Tag' pour récupérer l'objet complet
                var partie = (PartieEchecsPGN)TableauPartiesPgn.Rows[e.RowIndex].Tag;
                if (partie != null && partie.CoupsPartiePGN != null)
                {   // Récupére la fenêtre principale
                    var mainForm = (EchiquierPrincipal)Application.OpenForms["EchiquierPrincipal"];
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
                    KryptonMessageBox.Show("La partie sélectionnée ne contient pas de coups", "Pas de coups dans la partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Debug.WriteLine("Erreur : La partie = null !? (sans doute vide ...)");
                }
            }
        }

        private void InitializeComponent()
        {
            NombrePartiesFichier = new Label();
            lblDoubleClick = new Label();
            TableauPartiesPgn = new DataGridView();
            gridJoueurBlanc = new DataGridViewTextBoxColumn();
            gridEloBlanc = new DataGridViewTextBoxColumn();
            gridJoueurNoir = new DataGridViewTextBoxColumn();
            gridEloNoir = new DataGridViewTextBoxColumn();
            gridResultat = new DataGridViewTextBoxColumn();
            gridNombreCoups = new DataGridViewTextBoxColumn();
            gridCodeEco = new DataGridViewTextBoxColumn();
            gridTournoi = new DataGridViewTextBoxColumn();
            gridRonde = new DataGridViewTextBoxColumn();
            gridSite = new DataGridViewTextBoxColumn();
            gridDate = new DataGridViewTextBoxColumn();
            ((ISupportInitialize)TableauPartiesPgn).BeginInit();
            SuspendLayout();
            // 
            // NombrePartiesFichier
            // 
            NombrePartiesFichier.BackColor = Color.LightGreen;
            NombrePartiesFichier.Location = new Point(0, 0);
            NombrePartiesFichier.Name = "NombrePartiesFichier";
            NombrePartiesFichier.Size = new Size(563, 30);
            NombrePartiesFichier.TabIndex = 0;
            NombrePartiesFichier.Text = "Nombre de parties au format PGN";
            NombrePartiesFichier.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDoubleClick
            // 
            lblDoubleClick.BackColor = Color.Lime;
            lblDoubleClick.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDoubleClick.Location = new Point(557, 0);
            lblDoubleClick.Name = "lblDoubleClick";
            lblDoubleClick.Size = new Size(379, 30);
            lblDoubleClick.TabIndex = 1;
            lblDoubleClick.Text = "Double-cliquez sur la partie que vous voulez consulter";
            lblDoubleClick.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TableauPartiesPgn
            // 
            TableauPartiesPgn.BackgroundColor = Color.Silver;
            TableauPartiesPgn.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            TableauPartiesPgn.Columns.AddRange([gridJoueurBlanc, gridEloBlanc, gridJoueurNoir, gridEloNoir, gridResultat, gridNombreCoups, gridCodeEco, gridTournoi, gridRonde, gridSite, gridDate]);
            TableauPartiesPgn.Location = new Point(0, 33);
            TableauPartiesPgn.Name = "TableauPartiesPgn";
            TableauPartiesPgn.Size = new Size(933, 529);
            TableauPartiesPgn.TabIndex = 2;
            TableauPartiesPgn.CellDoubleClick += TableauPartiesPgn_CellDoubleClick;
            // 
            // gridJoueurBlanc
            // 
            gridJoueurBlanc.HeaderText = "Blancs";
            gridJoueurBlanc.Name = "gridJoueurBlanc";
            // 
            // gridEloBlanc
            // 
            gridEloBlanc.HeaderText = "ELO ";
            gridEloBlanc.Name = "gridEloBlanc";
            gridEloBlanc.Width = 50;
            // 
            // gridJoueurNoir
            // 
            gridJoueurNoir.HeaderText = "Noirs";
            gridJoueurNoir.Name = "gridJoueurNoir";
            // 
            // gridEloNoir
            // 
            gridEloNoir.HeaderText = "ELO";
            gridEloNoir.Name = "gridEloNoir";
            gridEloNoir.Width = 50;
            // 
            // gridResultat
            // 
            gridResultat.HeaderText = "Résultat";
            gridResultat.Name = "gridResultat";
            gridResultat.Width = 50;
            // 
            // gridNombreCoups
            // 
            gridNombreCoups.HeaderText = "Nbre coups";
            gridNombreCoups.Name = "gridNombreCoups";
            gridNombreCoups.Width = 50;
            // 
            // gridCodeEco
            // 
            gridCodeEco.HeaderText = "ECO";
            gridCodeEco.Name = "gridCodeEco";
            gridCodeEco.Width = 50;
            // 
            // gridTournoi
            // 
            gridTournoi.HeaderText = "Tournoi";
            gridTournoi.Name = "gridTournoi";
            gridTournoi.Width = 200;
            // 
            // gridRonde
            // 
            gridRonde.HeaderText = "Ronde";
            gridRonde.Name = "gridRonde";
            gridRonde.Width = 50;
            // 
            // gridSite
            // 
            gridSite.HeaderText = "Site";
            gridSite.Name = "gridSite";
            // 
            // gridDate
            // 
            gridDate.HeaderText = "Date";
            gridDate.Name = "gridDate";
            // 
            // FichierPartiePgn
            // 
            ClientSize = new Size(934, 561);
            Controls.Add(TableauPartiesPgn);
            Controls.Add(lblDoubleClick);
            Controls.Add(NombrePartiesFichier);
            Name = "FichierPartiePgn";
            Text = "Fichier de partie(s) au format PGN";
            ((ISupportInitialize)TableauPartiesPgn).EndInit();
            ResumeLayout(false);

        }
        public Label NombrePartiesFichier;
        private Label lblDoubleClick;
        private DataGridViewTextBoxColumn gridJoueurBlanc;
        private DataGridViewTextBoxColumn gridEloBlanc;
        private DataGridViewTextBoxColumn gridJoueurNoir;
        private DataGridViewTextBoxColumn gridEloNoir;
        private DataGridViewTextBoxColumn gridResultat;
        private DataGridViewTextBoxColumn gridNombreCoups;
        private DataGridViewTextBoxColumn gridCodeEco;
        private DataGridViewTextBoxColumn gridTournoi;
        private DataGridViewTextBoxColumn gridRonde;
        private DataGridViewTextBoxColumn gridSite;
        private DataGridViewTextBoxColumn gridDate;
        private DataGridView TableauPartiesPgn;
    }
}
