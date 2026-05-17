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

using ComponentFactory.Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }

    public class GestionPartiePgn
    {   // Spécification détaillée du format PGN = https://fr.wikipedia.org/wiki/Portable_Game_Notation
        public static string RetourneContenuPgn(PartieEchecsPGN partieEnCours, string localisation)
        {   // Met au format Pgn la partieEnCours pour visualisation et sauvegarde ... 
            int comptepartiel = 0;
            string contenuPgn = "";
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
            contenuPgn = contenuPgn + " " + partieEnCours.Result;   // Rajout du résultat à la fin de la partie
            contenuPgn = RetourneEntetePgn(partieEnCours) + contenuPgn;
            return contenuPgn;
        }
        public static string RetourneEntetePgn(PartieEchecsPGN partieEnCours)
        {   // Retourne l'en-tête de la partie au format PGN, avec les balises obligatoires et optionnelles
            string enTetePgn = "";
            // Ajout de l'en-tête complet respectant le format PGN (mes Balises optionnelles préférées)
            enTetePgn = "[PlyCount \"" + partieEnCours.CompteDePLy + "\"]\n\n" + enTetePgn;  // Nombre de 1/2 coups
            enTetePgn = "[BlackElo \"" + partieEnCours.BlackElo + "\"]\n" + enTetePgn;    // Elo Noirs
            enTetePgn = "[WhiteElo \"" + partieEnCours.WhiteElo + "\"]\n" + enTetePgn;    // Elo Blancs
            enTetePgn = "[ECO \"" + partieEnCours.ECO + "\"]\n" + enTetePgn;              // Code ECO (ouverture) de la partie 

            // Ajout de l'en-tête complet respectant le format PGN (Balises obligatoires)
            enTetePgn = "[Result \"" + partieEnCours.Result + "\"]\n" + enTetePgn;        // Résultat Partie
            enTetePgn = "[Black \"" + partieEnCours.Black + "\"]\n" + enTetePgn;          // Joueur noir
            enTetePgn = "[White \"" + partieEnCours.White + "\"]\n" + enTetePgn;          // Joueur blanc
            enTetePgn = "[Date \"" + partieEnCours.Date + "\"]\n" + enTetePgn;            // Date
            enTetePgn = "[Round \"" + partieEnCours.Ronde + "\"]\n" + enTetePgn;          // Numéro de la Ronde
            enTetePgn = "[Site \"" + partieEnCours.Lieu + "\"]\n" + enTetePgn;            // Lieu de la Partie
            enTetePgn = "[Event \"" + partieEnCours.Tournoi + "\"]\n" + enTetePgn;     // Nom du Tournoi
            return enTetePgn;
        }

        public static void DecodeCoupPartie(string coupPartie, bool couleurTraitBlanc)
        {   // Décode UN coup au format Pgn en case source / destination et execute le coup
            char dernierCaractereCoup, LeveeDeDoute;
            bool PromotionExiste = false;
            bool PriseExiste = false;
            BloquerChoixPromo = false;
            ColorPiece couleurQuiJoue;
            string CaseDestination, CaseSource;
            CaseSource = CaseDestination = "";
            TypePiece pieceQuiJoue = TypePiece.Vide;
            string CoupPGN = coupPartie;       // On récupère le coup pour pouvoir le traiter
            if (CoupPGN.Length > 0)     // Il faut s'assurer qu'il y a au moins un coup, sinon erreur "L'index se trouve en dehors des limites du tableau."
            {
                couleurQuiJoue = couleurTraitBlanc ? ColorPiece.Blanc : ColorPiece.Noir;
                dernierCaractereCoup = CoupPGN[CoupPGN.Length - 1];    // Nettoyage des signes "+" et "#" à la fin du coup qui signalent les echecs
                if (dernierCaractereCoup == '+')
                {
                    CoupPGN = CoupPGN.TrimEnd('+');     // Enlève le + dans CoupPGN pour permettre d'avoir les 2 derniers caractères comme CaseDestination
                    Echec = true;
                }
                if (dernierCaractereCoup == '#')
                {
                    CoupPGN = CoupPGN.TrimEnd('#');     // Enlève le # dans CoupPGN pour permettre d'avoir les 2 derniers caractères comme CaseDestination
                    EchecetMat = true;
                }
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
                    // CoupPGN a 5 caractères maximum, car on a enlevé les échecs au début de la méthode
                    // On enlève le "x" de la prise si il existe, ainsi que les 2 derniers caractères qui sont la destination
                    CoupPGN = CoupPGN.Replace("x", "");
                    CoupPGN = CoupPGN[..^2];
                    // Si CoupPGN.Length == 2, CoupPGN[1] est le caractère de "LeveeDeDoute", peut être une lettre ou un chiffre. Sinon c'est vide
                    if (CoupPGN.Length == 2)
                        LeveeDeDoute = CoupPGN[1];      // C'est le caractère de levée de doute, une lettre pour la colonne ou un chiffre pour la ligne
                    else
                        LeveeDeDoute = '\0';            // Il n'y a pas d'ambiguité, 

                    for (int i = 21; i <= 98; i++)
                    {
                        if (PiecesEchiquier[i] == pieceQuiJoue)     // Pour chaque Pièce trouvée, 
                        {                                       // On génère les mouvements possibles
                            List<string> Mouvements = RetourneMouvements(LogiqueMouvements.NomCaseAlgebrique(i));
                            for (int j = 0; j < Mouvements.Count; j++)
                            {                                                                               // Mouvements[x] est sous la forme c5 ou xc5 si prise
                                if (Mouvements[j][^2..] == CaseDestination)   // Au cas ou il y a prise, on prend la fin de la chaine
                                {   // Si un des mouvements est la case de destination, ce n'est pas forcément le bon Cavalier, Dame ou Tour ou Fou
                                    // Si LogiqueMouvements.NomCaseAlgebrique(i) contient LeveeDeDoute, ou que LeveeDeDoute est vide (il n'y a plus de doute)
                                    // c'est la bonne Pièce et CaseSource = LogiqueMouvements.NomCaseAlgebrique(i);
                                    if (LeveeDeDoute == '\0' || LeveeDeDoute == LogiqueMouvements.NomCaseAlgebrique(i)[0] || LeveeDeDoute == LogiqueMouvements.NomCaseAlgebrique(i)[1])
                                    {       //  Il n'y a pas d'ambiguité, 
                                        CaseSource = LogiqueMouvements.NomCaseAlgebrique(i);
                                        if (Mouvements[j].Contains('x'))
                                            PriseExiste = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (char.IsLower(CoupPGN[0]))               // COUP DE PION, car la 1ère lettre est une minuscule
                {
                    if (CoupPGN.Contains('='))              // PROMOTION
                    {   // PROMOTION  
                        PromotionExiste = true;
                        switch (CoupPGN[CoupPGN.Length - 1])
                        {
                            case 'Q':
                                LogiqueMouvements.PromotionPiece = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.ReineBlanche : TypePiece.ReineNoire;
                                break;
                            case 'R':
                                LogiqueMouvements.PromotionPiece = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.TourBlanche : TypePiece.TourNoire;
                                break;
                            case 'N':
                                LogiqueMouvements.PromotionPiece = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.CavalierBlanc : TypePiece.CavalierNoir;
                                break;
                            case 'B':
                                LogiqueMouvements.PromotionPiece = (couleurQuiJoue == ColorPiece.Blanc) ? TypePiece.FouBlanc : TypePiece.FouNoir;
                                break;
                        }
                        BloquerChoixPromo = true;   // Lors de l'execution du coup, il ne faudra pas proposer le choix de pièce promue
                        CoupPGN = CoupPGN[..^2];    // On nettoie le coup de la promotion pour l'analyse qui suit
                    }
                    if (CoupPGN.Contains('x'))              // PRISE
                    {   // PRISE
                        PriseExiste = true;
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
                        PriseExiste = false;
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
                if (dernierCaractereCoup != '.')
                {   // On change de couleur si c'est pas le numéro du coup
                    _ = couleurQuiJoue == ColorPiece.Noir ? ColorPiece.Blanc : ColorPiece.Noir;
                    string CoupNal = (CaseSource + "-" + CaseDestination);
                    PriseExiste = false;
                    PromotionExiste = false;
                    CoupNal = pieceQuiJoue + "  " + CoupNal;

                    if (string.IsNullOrWhiteSpace(CaseSource) || string.IsNullOrWhiteSpace(CaseDestination))
                    {   // Garde : s'assurer que les cases ont été déterminées correctement avant d'exécuter le coup
                        return; // interrompre l'exécution du coup, éviter index hors bornes
                    }

                    LogiqueMouvements.ExecutionCoup(CaseSource, CaseDestination);
                }
            }
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
            private KryptonButton sauveEnTete;
            private KryptonButton annulerEnTete;

            public SaisieBalises(PartieEchecsPGN partie)
            {
                partieBalises = partie;
                InitializeComponents();
                this.StartPosition = FormStartPosition.CenterScreen;
                // Appliquer la palette globale Krypton
                this.Palette = KryptonManager.CurrentGlobalPalette;
            }
            private void InitializeComponents()
            {
                // Création des champs de saisie
                tournamentCase = CreationBalisesTextBox("Tournoi :", 20, 10, value => partieBalises.Tournoi = value);
                lieuCase = CreationBalisesTextBox("Lieu :     ", 20, 40, value => partieBalises.Lieu = value);
                dateCase = CreationBalisesTextBox("Date :     ", 20, 70, value => partieBalises.Date = value);
                rondeCase = CreationBalisesTextBox("Ronde :    ", 20, 100, value => partieBalises.Ronde = value);
                blancsCase = CreationBalisesTextBox("Blancs :   ", 20, 130, value => partieBalises.White = value);
                noirsCase = CreationBalisesTextBox("Noirs :    ", 20, 160, value => partieBalises.Black = value);
                resultCase = CreationBalisesTextBox("Résultat :", 20, 190, value => partieBalises.Result = value);
                ecoCase = CreationBalisesTextBox("ECO :     ", 20, 220, value => partieBalises.ECO = value);
                whiteeloCase = CreationBalisesTextBox("ELO Blancs :", 20, 250, value => partieBalises.WhiteElo = value);
                blackeloCase = CreationBalisesTextBox("ELO Noirs :", 20, 280, value => partieBalises.BlackElo = value);
                plycountCase = CreationBalisesTextBox("Demi coups :", 20, 310, value => partieBalises.CompteDePLy = value);
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
                // Boutons
                sauveEnTete = new KryptonButton
                {
                    Text = "Enregistrer En-têtes", // ✅ Utilise simplement `Text`
                    Location = new Point(20, 350),
                    Width = 110
                };
                sauveEnTete.Click += SauveBalises_Click;
                this.Controls.Add(sauveEnTete);

                annulerEnTete = new KryptonButton
                {
                    Text = "Quitter En-Têtes",
                    Location = new Point(135, 350),
                    Width = 110
                };
                annulerEnTete.Click += AnnulerBalises_Click;
                this.Controls.Add(annulerEnTete);

                // Configuration de la fenêtre
                this.Text = "Saisie des en-têtes de parties";
                this.Size = new Size(280, 420);
            }
            private KryptonTextBox CreationBalisesTextBox(string labelText, int x, int y, Action<string> updateProperty)
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
                textBox.TextChanged += (sender, e) => updateProperty(textBox.Text);
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








