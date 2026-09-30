// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre d'affichage des coups et parcours de la liste ...
//  └─ Classe "FenetrePartie" qui affiche la liste des coups et les boutons
//              ├─ "FenetrePartie"
//              ├─ "MettreAJourSelection"  
//              ├─ "AfficherPositionActuelle"  
//              ├─ "BoutonDebut_Click"  
//              ├─ "BoutonGauche_Click"  
//              ├─ "BoutonDroit_Click"  
//              ├─ "BoutonFin_Click"  
//              ├─ "FermeFeuillePartie_Click"  
//              └─ "FenetrePartie_FormClosing"

using System;
using System.Drawing;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public partial class FenetrePartie : Form       // Classe pour le parcours de la feuille de partie
    {
        private int ligneActuelle = 0;      // Ligne actuelle (index)
        private int colonneActuelle = 1;    // 1: Blancs, 2: Noirs (par défaut on commence avec les Blancs)
        private EchiquierPrincipal _brunoInterfaceGraphique;
        public FenetrePartie(EchiquierPrincipal brunoInterfaceGraphique)
        {   // A l'ouverture, l'échiquier reste sur la position qu'il montre : il ne change qu'en naviguant dans la liste
            InitializeComponent();
            _brunoInterfaceGraphique = brunoInterfaceGraphique;
            FeuillePartie.ScrollBars = ScrollBars.Vertical; // Toujours afficher le défilement vertical
            FeuillePartie.Columns[1].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold | FontStyle.Italic);
            FeuillePartie.Columns[2].DefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold | FontStyle.Italic);
        }
        // Une partie commencée depuis un FEN a en tête de ListeCoups un élément "position de départ" (sans coup) : on le saute
        private static int Decalage => LogiqueMouvements.ListeCoups.Count > 0 && LogiqueMouvements.ListeCoups[0].EstPositionDeDepart ? 1 : 0;
        private static int NombreCoupsJoues => LogiqueMouvements.ListeCoups.Count - Decalage;
        // Partie commençant par un coup noir (FEN avec les Noirs au trait) : la 1re cellule des Blancs est "..." (pas un coup)
        private static int CelluleVide => FeuilleDePartie.CommenceParLesNoirs(LogiqueMouvements.ListeCoups) ? 1 : 0;
        private void MettreAJourSelection()
        {
            if (ligneActuelle >= 0 && ligneActuelle < FeuillePartie.Rows.Count)
            {   // Sélectionner la cellule correspondante à la position actuelle
                FeuillePartie.CurrentCell = FeuillePartie.Rows[ligneActuelle].Cells[colonneActuelle];
            }
            AfficherPositionActuelle();
        }
        private void AfficherPositionActuelle()
        {   // Affiche sur l'échiquier la position après le coup sélectionné (la partie n'est pas modifiée)
            int demiCoup = ligneActuelle * 2 + (colonneActuelle - 1) - CelluleVide;     // n° du coup sélectionné parmi les coups joués (à partir de 0)
            if (demiCoup >= 0 && demiCoup < NombreCoupsJoues)
                _brunoInterfaceGraphique.AfficheCoupDeLaPartie(Decalage + demiCoup);
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {
            ligneActuelle = 0;      // Aller à la première ligne
            colonneActuelle = 1 + CelluleVide;      // Premier coup : celui des Blancs, ou des Noirs si la partie commence par eux
            MettreAJourSelection();
            BoutonGauche.Enabled = BoutonDebut.Enabled = false;
            BoutonDroit.Enabled = BoutonFin.Enabled = true;
        }
        private void BoutonGauche_Click(object sender, EventArgs e)
        {
            if (ligneActuelle == 0 && colonneActuelle == 1 + CelluleVide)
                return;                // déjà sur le premier coup
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
            if (NombreCoupsJoues == 0)
                return;
            int dernier = NombreCoupsJoues - 1 + CelluleVide;     // n° de la cellule du dernier coup joué (à partir de 0)
            ligneActuelle = dernier / 2;
            colonneActuelle = dernier % 2 + 1;      // 1 : coup blanc, 2 : coup noir
            MettreAJourSelection();
            BoutonDroit.Enabled = BoutonFin.Enabled = false;
            BoutonDebut.Enabled = BoutonGauche.Enabled = true;
        }
        private void FermeFeuillePartie_Click(object sender, EventArgs e)
        {   // Fermeture de la liste de coups (bouton) : voir FenetrePartie_FormClosing
            this.Close();
        }
        private void FenetrePartie_FormClosing(object sender, FormClosingEventArgs e)
        {   // Fermeture de la fenêtre (bouton ou croix rouge) : l'échiquier revient à la position courante de la partie.
            // La partie n'ayant pas été modifiée par le parcours, rien d'autre à rétablir
            _brunoInterfaceGraphique.RetourPositionCourante();
        }
    }
}
