// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ OutilsBruno est développé par Bruno COURTOIS.  Copyright © 2021/2024 █  
// █ OutilsBruno est gratuit, sauf s'il est utilisé commercialement       █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
// Module ajouté à la structure de base pour gérer les parties et balises PGN

using System;
using System.Drawing;
using System.Windows.Forms;
using static BrunoGUI_Stockfish.LogiqueMouvements;

namespace BrunoGUI_Stockfish
{
    public class GestionPartiePgn       // Spécification détaillée du format PGN = https://fr.wikipedia.org/wiki/Portable_Game_Notation
    {
        public class PartieEchecPGN
        {   // Format de chaque partie qui se trouve dans ListeParties
            public string Tournament { get; set; }
            public string Lieu { get; set; }
            public string Date { get; set; }
            public string Round { get; set; }
            public string White { get; set; }
            public string Black { get; set; }
            public string Result { get; set; }
            public string ECO { get; set; }
            public string WhiteElo { get; set; }
            public string BlackElo { get; set; }
            public string PlyCount { get; set; }
            public string CoupsPartiePGN { get; set; }
        }
        public string RetourneContenuPgn(PartieEchecPGN PartieEnCours, string localisation)
        {
            int comptepartiel = 0;
            string contenuPgn = "";
            for (int i = 0; i < LogiqueMouvements.ListeCoupsPgn.Count; i++)     // Création du contenu du fichier en lignes de 80 caractères
            {       // Il faut des lignes <= 80 caractères, mais n'aller à la ligne que si c'est un espace
                comptepartiel = comptepartiel + LogiqueMouvements.ListeCoupsPgn[i].Length;
                if (localisation == "Fr")
                    contenuPgn = contenuPgn + LogiqueMouvements.ListeCoupsPgnFr[i];
                else
                    contenuPgn = contenuPgn + LogiqueMouvements.ListeCoupsPgn[i];
                if (comptepartiel >= 74)
                {   // Si plus de 80 caractères, il faut découper  (attention aux coups comme Cfxe6+)
                    contenuPgn += " \n";        // Rajout des sauts de ligne
                    comptepartiel = 0;          // Ligne suivante
                }
            }
            // Ajout de l'en-tête complet respectant le format PGN (mes Balises optionnelles préférées)
            contenuPgn = "[PlyCount \"" + PartieEnCours.PlyCount + "\"]\n\n" + contenuPgn;  // Nombre de 1/2 coups
            contenuPgn = "[BlackElo \"" + PartieEnCours.BlackElo + "\"]\n" + contenuPgn;    // Elo Noirs
            contenuPgn = "[WhiteElo \"" + PartieEnCours.WhiteElo + "\"]\n" + contenuPgn;    // Elo Blancs
            contenuPgn = "[ECO \"" + PartieEnCours.ECO + "\"]\n" + contenuPgn;              // Code ECO (ouverture) de la partie 

            // Ajout de l'en-tête complet respectant le format PGN (Balises obligatoires)
            contenuPgn = "[Result \"" + PartieEnCours.Result + "\"]\n" + contenuPgn;        // Résultat Partie
            contenuPgn = "[Black \"" + PartieEnCours.Black + "\"]\n" + contenuPgn;            // Date
            contenuPgn = "[White \"" + PartieEnCours.White + "\"]\n" + contenuPgn;            // Date
            contenuPgn = "[Date \"" + PartieEnCours.Date + "\"]\n" + contenuPgn;            // Date
            contenuPgn = "[Round \"" + PartieEnCours.Round + "\"]\n" + contenuPgn;          // Numéro de la Ronde
            contenuPgn = "[Site \"" + PartieEnCours.Lieu + "\"]\n" + contenuPgn;            // Lieu de la Partie
            contenuPgn = "[Event \"" + PartieEnCours.Tournament + "\"]\n" + contenuPgn;     // Nom du Tournoi
            return contenuPgn;
        }
        public string AlgebriqueVersPgn(string varianteBrute, int numeroDemiCoup)     // Retourne les coups sous la forme x. Db6 (format PGN en fait)
        {
            varianteBrute = varianteBrute.TrimStart();
            string[] variantePgnDecoupe = varianteBrute.Split(' ');     // Découpage des coups de la variante
            string stockeFen = LogiqueMouvements.RetourneChaineFenActuel();     // Récupérer le FEN actuel pour le remettre à la fin ? Obligé si on bouge les pièces !!
            string coupExamine = "";
            varianteBrute = "";
            if (LogiqueMouvements.QuiJoue == ColorPiece.Noir)                               //la PV commence par le coup Noir
                varianteBrute = ((numeroDemiCoup / 2) + 1).ToString() + " ...";     // On met le numéro du coup Noir
            for (int i = 0; i < variantePgnDecoupe.Length; i++)
            {
                if (variantePgnDecoupe[i] != "")    // Pour blinder le code (Au cas ou la découpe donne un élément vide)
                {
                    if (variantePgnDecoupe[i].Length >= 4)   // Le coup doit comporter source et destination, sinon crash ci dessous ...
                    {
                        string source = variantePgnDecoupe[i].Substring(0, 2);
                        string destination = variantePgnDecoupe[i].Substring(2, 2);
                        coupExamine = LogiqueMouvements.CoupNotationAlgebriquePGN(source, destination);    // On récupère le coup sous la forme b8d7 au format PGN comme Cd7
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

        public class SaisieBalises : Form
        {
            private PartieEchecPGN partieBalises;
            private TextBox tournamentCase;
            private TextBox lieuCase;
            private TextBox dateCase;
            private TextBox rondeCase;
            private TextBox blancsCase;
            private TextBox noirsCase;
            private TextBox resultCase;
            private TextBox ecoCase;
            private TextBox whiteeloCase;
            private TextBox blackeloCase;
            private TextBox plycountCase;

            private Button SauveEnTete;
            private Button AnnulerEnTete;

            public SaisieBalises(PartieEchecPGN partie)
            {
                partieBalises = partie;
                InitializeComponents();
                this.BackColor = Color.LightSteelBlue;
            }
            private void InitializeComponents()
            {
                tournamentCase = CreationBalisesTextBox("Tournoi :", 20, 10, value => partieBalises.Tournament = value);
                lieuCase = CreationBalisesTextBox("Lieu :", 20, 40, value => partieBalises.Lieu = value);
                dateCase = CreationBalisesTextBox("Date :", 20, 70, value => partieBalises.Date = value);
                rondeCase = CreationBalisesTextBox("Ronde :", 20, 100, value => partieBalises.Round = value);
                blancsCase = CreationBalisesTextBox("Blancs :", 20, 130, value => partieBalises.White = value);
                noirsCase = CreationBalisesTextBox("Noirs :", 20, 160, value => partieBalises.Black = value);
                resultCase = CreationBalisesTextBox("Résultat :", 20, 190, value => partieBalises.Result = value);
                ecoCase = CreationBalisesTextBox("ECO :", 20, 220, value => partieBalises.ECO = value);
                whiteeloCase = CreationBalisesTextBox("ELO BLancs :", 20, 250, value => partieBalises.WhiteElo = value);
                blackeloCase = CreationBalisesTextBox("ELO Noirs :", 20, 280, value => partieBalises.BlackElo = value);
                plycountCase = CreationBalisesTextBox("Demi coups :", 20, 310, value => partieBalises.PlyCount = value);

                // Pré-remplir les champs avec les valeurs actuelles de la partie en cours
                tournamentCase.Text = partieBalises.Tournament;
                lieuCase.Text = partieBalises.Lieu;
                dateCase.Text = partieBalises.Date;
                rondeCase.Text = partieBalises.Round;
                blancsCase.Text = partieBalises.White;
                noirsCase.Text = partieBalises.Black;
                resultCase.Text = partieBalises.Result;
                ecoCase.Text = partieBalises.ECO;
                whiteeloCase.Text = partieBalises.WhiteElo;
                blackeloCase.Text = partieBalises.BlackElo;
                plycountCase.Text = partieBalises.PlyCount;

                SauveEnTete = new Button
                {
                    Text = "Enregistrer En-têtes",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(20, 350),
                    Width = 110
                };
                SauveEnTete.Click += SauveBalises_Click;
                this.Controls.Add(SauveEnTete);

                AnnulerEnTete = new Button
                {
                    Text = "Quitter En-Têtes",
                    Location = new Point(130, 350),
                    Width = 110
                };
                AnnulerEnTete.Click += SauveBalises_Click;
                this.Controls.Add(AnnulerEnTete);

                // Configuration de la fenêtre
                this.Text = "Saisie des en-têtes de parties";
                this.Size = new Size(280, 420);
            }
            private TextBox CreationBalisesTextBox(string labelText, int x, int y, Action<string> updateProperty)
            {
                Label label = new Label
                {
                    Text = labelText,
                    Location = new Point(x, y),
                    Font = new Font("Arial", 10) //, FontStyle.Bold);
                };
                this.Controls.Add(label);

                TextBox textBox = new TextBox
                {
                    Location = new Point(x + label.Width + 5, y),
                    Width = 120,
                    BorderStyle = BorderStyle.Fixed3D,
                    Font = new Font("Arial", 10, FontStyle.Bold | FontStyle.Italic)
                };
                // Utilisation de la gestionnaire d'événements TextChanged pour mettre à jour la propriété
                textBox.TextChanged += (sender, e) => updateProperty(textBox.Text);
                this.Controls.Add(textBox);
                return textBox;
            }
            private void SauveBalises_Click(object sender, EventArgs e)
            {   // Le bouton "Enregistrer" a été cliqué, on met à jour les valeurs de la partie en cours
                partieBalises.Tournament = tournamentCase.Text;
                partieBalises.Lieu = lieuCase.Text;
                partieBalises.Date = dateCase.Text;
                partieBalises.Round = rondeCase.Text;
                partieBalises.White = blancsCase.Text;
                partieBalises.Black = noirsCase.Text;
                partieBalises.Result = resultCase.Text;
                partieBalises.ECO = ecoCase.Text;
                partieBalises.WhiteElo = whiteeloCase.Text;
                partieBalises.BlackElo = blackeloCase.Text;
                partieBalises.PlyCount = plycountCase.Text;
                // Fermer la fenêtre de saisie
                this.Close();
            }
        }
    }
}




