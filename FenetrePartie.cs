using System;
using System.Drawing;
using System.Windows.Forms;

namespace BrunoGUI_Stockfish
{
    public partial class FenetrePartie : Form       // Classe pour le parcours de la feuille de partie
    {
        private int ligneActuelle = 0;      // Ligne actuelle (index)
        private int colonneActuelle = 1;    // 1: Blancs, 2: Noirs (par défaut on commence avec les Blancs)
        private BrunoInterfaceGraphique _brunoInterfaceGraphique;
        public FenetrePartie(BrunoInterfaceGraphique brunoInterfaceGraphique)
        {
            InitializeComponent();
            FeuillePartie.ScrollBars = ScrollBars.Vertical; // Toujours afficher le défilement vertical
            FeuillePartie.Columns[1].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold | FontStyle.Italic);
            FeuillePartie.Columns[2].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold | FontStyle.Italic);
            AfficherPositionActuelle();
            _brunoInterfaceGraphique = brunoInterfaceGraphique;
        }
        private void MettreAJourSelection()
        {
            if (ligneActuelle >= 0 && ligneActuelle < FeuillePartie.Rows.Count)
            {   // Sélectionner la cellule correspondante à la position actuelle
                FeuillePartie.CurrentCell = FeuillePartie.Rows[ligneActuelle].Cells[colonneActuelle];
            }
            AfficherPositionActuelle();
        }
        private void AfficherPositionActuelle()
        {
            int demiCoupsTotaux = LogiqueMouvements.ListeCoupsFen.Count;
            if (ligneActuelle >= 0 && (ligneActuelle * 2) < demiCoupsTotaux)
            {   // Vérifie que l'indice de la ligne est valide dans la ListeCoupsFen
                string fenActuel = "";
                if (colonneActuelle == 1)
                {   // Si on est sur la colonne des Blancs (colonneActuelle = 1)
                    if (ligneActuelle * 2 < demiCoupsTotaux)
                    {   // Vérifie que l'index pour les Blancs est valide
                        fenActuel = LogiqueMouvements.ListeCoupsFen[ligneActuelle * 2];  // Index des Blancs
                        BrunoInterfaceGraphique.NumeroDemiCoup = ligneActuelle * 2;
                    }
                }
                else if (colonneActuelle == 2)
                {   // Si on est sur la colonne des Noirs (colonneActuelle = 2)
                    if ((ligneActuelle * 2 + 1) < demiCoupsTotaux)
                    {   // Vérifie que l'index pour les Noirs est valide
                        fenActuel = LogiqueMouvements.ListeCoupsFen[ligneActuelle * 2 + 1];  // Index des Noirs
                        BrunoInterfaceGraphique.NumeroDemiCoup = ligneActuelle * 2 + 1;
                    }
                }
                // Affiche la position dans la fenêtre en fonction du FEN actuel, si trouvé
                if (!string.IsNullOrEmpty(fenActuel))
                {
                    LogiqueMouvements.MiseenplaceFen(fenActuel);
                }
            }
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {
            ligneActuelle = 0;      // Aller à la première ligne
            colonneActuelle = 1;    // Commencer avec le coup Blancs
            MettreAJourSelection();
            BoutonGauche.Enabled = BoutonDebut.Enabled = false;
            BoutonDroit.Enabled = BoutonFin.Enabled = true;
        }
        private void BoutonGauche_Click(object sender, EventArgs e)
        {
            if (colonneActuelle == 1)  // Si on est sur Blancs
            {
                if (ligneActuelle > 0)
                {
                    ligneActuelle--;        // Aller à la ligne précédente
                    colonneActuelle = 2;    // Passer à Noirs
                }
            }
            else  // Si on est sur Noirs
            {
                colonneActuelle = 1;  // Revenir à Blancs sur la même ligne
            }
            MettreAJourSelection();
            BoutonDebut.Enabled = BoutonGauche.Enabled = BoutonDroit.Enabled = BoutonFin.Enabled = true;
        }
        private void BoutonDroit_Click(object sender, EventArgs e)
        {
            if (colonneActuelle == 2)  // Si on est sur Noirs
            {
                if (ligneActuelle < FeuillePartie.Rows.Count - 1)
                {
                    ligneActuelle++;        // Aller à la ligne suivante
                    colonneActuelle = 1;    // Passer à Blancs
                }
            }
            else  // Si on est sur Blancs
            {
                if (ligneActuelle < FeuillePartie.Rows.Count - 1 || colonneActuelle != 2)
                {   // Condition ajoutée pour vérifier que si le dernier coup de la liste est un coup blanc,
                    // la colonne n'est pas modifiée pour passer à un coup noir inexistant.
                    colonneActuelle = 2;  // Aller à Noirs sur la même ligne
                }
            }
            MettreAJourSelection();
            BoutonDebut.Enabled = BoutonGauche.Enabled = BoutonDroit.Enabled = BoutonFin.Enabled = true;
        }
        private void BoutonFin_Click(object sender, EventArgs e)
        {
            if (LogiqueMouvements.ListeCoupsFen.Count % 2 == 0)
            {   // Vérifier si le dernier coup est un coup blanc (pas de colonne noire après la dernière ligne)
                ligneActuelle = (LogiqueMouvements.ListeCoupsFen.Count - 1) / 2;
                colonneActuelle = 2;  // Terminer avec le coup Noirs
            }
            else
            {
                ligneActuelle = ((LogiqueMouvements.ListeCoupsFen.Count - 1) - 1) / 2;
                colonneActuelle = 1;  // Terminer avec le coup Blancs
            }
            MettreAJourSelection();
            BoutonDroit.Enabled = BoutonFin.Enabled = false;
            BoutonDebut.Enabled = BoutonGauche.Enabled = true;
        }
        private void FermeFeuillePartie_Click(object sender, EventArgs e)
        {   // Si on ferme la liste de coups, il faut revenir à la fin de la partie ...
            LogiqueMouvements.MiseenplaceFen(LogiqueMouvements.ListeCoupsFen[LogiqueMouvements.ListeCoupsFen.Count - 1]);
            BrunoInterfaceGraphique.NumeroDemiCoup = LogiqueMouvements.ListeCoupsFen.Count - 1; // A la sortie de la liste de coups, il faut revenir à la fin de la partie
            _brunoInterfaceGraphique.PlateauEnable(true);                                       // et autoriser de jouer
            _brunoInterfaceGraphique.AnalysePosition.Enabled = _brunoInterfaceGraphique.RetourArriere.Enabled = true;
            this.Close();
        }
        private void FenetrePartie_FormClosing(object sender, FormClosingEventArgs e)
        {   // Fermeture de la fenêtre par la croix rouge en haut à droite ...
            LogiqueMouvements.MiseenplaceFen(LogiqueMouvements.ListeCoupsFen[LogiqueMouvements.ListeCoupsFen.Count - 1]);
            BrunoInterfaceGraphique.NumeroDemiCoup = LogiqueMouvements.ListeCoupsFen.Count - 1; // A la sortie de la liste de coups, il faut revenir à la fin de la partie
            _brunoInterfaceGraphique.PlateauEnable(true);                                       // et autoriser de jouer
            _brunoInterfaceGraphique.AnalysePosition.Enabled = _brunoInterfaceGraphique.RetourArriere.Enabled = true;
        }
    }
}
