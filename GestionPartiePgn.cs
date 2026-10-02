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
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public class PartieEchecsPGN
    {   // Format de chaque partie qui se trouve dans ListeParties
        public string Tournoi { get; set; }
        public string Lieu { get; set; }
        public string Date { get; set; }
        public string Ronde { get; set; }
        public string White { get; set; }
        public string Black { get; set; }
        public string Result { get; set; }
        public string ECO { get; set; }
        public string WhiteElo { get; set; }
        public string BlackElo { get; set; }
        public string CompteDePLy { get; set; }
        public string CoupsPartiePGN { get; set; }
        public string Fen { get; set; }             // position de départ ([SetUp "1"] [FEN "..."]) ; vide : position initiale
        public string TimeControl { get; set; }     // cadence ([TimeControl "180+2"], en secondes + bonus) ; vide : sans pendule
    }

    public class GestionPartiePgn
    {   // Spécification détaillée du format PGN = https://fr.wikipedia.org/wiki/Portable_Game_Notation
        public static string RetourneContenuPgn(PartieEchecsPGN partieEnCours, string localisation)
        {   // Met au format Pgn la partieEnCours pour visualisation et sauvegarde ... 
            int comptepartiel = 0;
            string contenuPgn = "";
            // Partie commençant par un coup noir (départ FEN, Noirs au trait) : le PGN exige "n... coup"
            Coup premier = ListeCoups.FirstOrDefault(c => !c.EstPositionDeDepart);
            if (premier != null && !premier.EstCoupBlanc)
                contenuPgn = premier.NumeroDuCoup + "... ";
            for (int i = 0; i < ListeCoupsPgnIntl.Count; i++)   // Création du contenu du fichier en lignes de 80 caractères
            {   // Il faut des lignes <= 80 caractères, mais n'aller à la ligne que si c'est un espace
                comptepartiel += ListeCoupsPgnIntl[i].Length;
                if (localisation == "Fr")
                    contenuPgn += ListeCoupsPgnFr[i];
                else
                    contenuPgn += ListeCoupsPgnIntl[i];
                if (comptepartiel >= 74)
                {   // Si plus de 80 caractères, il faut découper  (attention aux coups comme Cfxe6+)
                    contenuPgn += " \n";        // Rajout des sauts de ligne
                    comptepartiel = 0;          // Ligne suivante
                }
            }
            contenuPgn = contenuPgn + " " + ResultatPgn(partieEnCours);   // Rajout du résultat à la fin de la partie
            contenuPgn = RetourneEntetePgn(partieEnCours) + contenuPgn;
            return contenuPgn;
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
            private void InitializeComponents()
            {
                // Création des champs de saisie
                // Les valeurs ne sont recopiées dans la partie qu'à l'enregistrement (SauveBalises_Click) : "Quitter" n'en garde aucune
                tournamentCase = CreationBalisesTextBox("Tournoi :", 20, 10);
                lieuCase = CreationBalisesTextBox("Lieu :     ", 20, 40);
                dateCase = CreationBalisesTextBox("Date :     ", 20, 70);
                rondeCase = CreationBalisesTextBox("Ronde :    ", 20, 100);
                blancsCase = CreationBalisesTextBox("Blancs :   ", 20, 130);
                noirsCase = CreationBalisesTextBox("Noirs :    ", 20, 160);
                resultCase = CreationBalisesTextBox("Résultat :", 20, 190);
                ecoCase = CreationBalisesTextBox("ECO :     ", 20, 220);
                whiteeloCase = CreationBalisesTextBox("ELO Blancs :", 20, 250);
                blackeloCase = CreationBalisesTextBox("ELO Noirs :", 20, 280);
                plycountCase = CreationBalisesTextBox("Demi coups :", 20, 310);
                cadenceCase = CreationBalisesTextBox("Cadence :  ", 20, 340);      // TimeControl : "180+2" = 3 min + 2 s par coup
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
                    Text = "Enregistrer En-têtes", // ✅ Utilise simplement `Text`
                    Location = new Point(20, 380),
                    Width = 110
                };
                sauveEnTete.Click += SauveBalises_Click;
                this.Controls.Add(sauveEnTete);

                annulerEnTete = new KryptonButton
                {
                    Text = "Quitter En-Têtes",
                    Location = new Point(135, 380),
                    Width = 110
                };
                annulerEnTete.Click += AnnulerBalises_Click;
                this.Controls.Add(annulerEnTete);

                // Configuration de la fenêtre
                this.Text = "Saisie des en-têtes de parties";
                this.Size = new Size(280, 450);
            }
            private KryptonTextBox CreationBalisesTextBox(string labelText, int x, int y)
            {
                KryptonLabel label = new()
                {
                    Text = labelText,
                    Location = new Point(x, y),
                };
                label.StateNormal.ShortText.Font = new Font("Arial", 10); // ✅ Police du label
                this.Controls.Add(label);

                KryptonTextBox textBox = new()
                {
                    Location = new Point(x + label.Width + 5, y),
                    Width = 120,
                };
                textBox.StateCommon.Border.DrawBorders = PaletteDrawBorders.All; // ✅ Bordure
                textBox.StateCommon.Content.Font = new Font("Arial", 10, FontStyle.Bold | FontStyle.Italic); // ✅ Police du texte
                this.Controls.Add(textBox);

                return textBox;
            }
            private void SauveBalises_Click(object sender, EventArgs e)
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
            private void AnnulerBalises_Click(object sender, EventArgs e)
            {   // Fermer sans enregistrer
                this.Close();
            }
        }
    }
}








