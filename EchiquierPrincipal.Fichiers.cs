// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// EchiquierPrincipal (fichier partiel) : Fichiers : ouverture et enregistrement PGN et FEN, saisie de position, chargement d'une partie PGN

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.GestionPartiePgn;
using static BrunoGUI_GenII.LogiqueMouvements;
using static BrunoGUI_GenII.Parametres;

namespace BrunoGUI_GenII
{
    public partial class EchiquierPrincipal
    {
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion de fichiers (ouverture/sauvegarde)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐

        /* Séquence : L'utilisateur clique sur 
        "mainform.ChargePartiesPgn_Click"
                ├─ On décode le fichier choisi "ListeParties = fichierPartiePgn.DecodeFichierPGN"
                ├─ On parcourt "ListeParties" pour les décoder avec "fichierPartiePgn.DecodePartiePGN"
                └─ On affiche la liste "fichierPartiePgn.AfficherListeParties(ListePartiesPGN)" 
         L'utilisateur double-clique sur une partie de la liste
                ├─ "mainForm.MontrePartiesPGN_Click"
                ├─ "mainForm.ChargerPartieDepuisPgn(partie)"
                └─ "mainForm.ParcoursPartie(coupsPartie)
        */
        private void ChargePartiesPgn_Click(object sender, EventArgs e)
        {   // --- Affiche la boîte de dialogue et traite le fichier PGN sélectionné  ---
            // Ouvrir un fichier ne change pas la partie en cours (ni la réflexion du moteur) : seul le choix d'une partie
            // dans la liste la remplace (ChargerPartieDepuisPgn). "Annuler" ne change donc rien
            if (ChargerPartiesPgn.ShowDialog() != DialogResult.OK)
                return;
            string cheminFichier = ChargerPartiesPgn.FileName;
            try
            {
                string fullPath = Path.GetFullPath(cheminFichier);
                Debug.WriteLine("Chemin complet du fichier : " + fullPath);
                ListeParties = FichierPartiePgn.DecodeFichierPGN(fullPath); // Récupère les parties PGN
                ListePartiesPGN.Clear();
                foreach (string partie in ListeParties)                     // On met chaque partie au format PartieEchecsPGN dans ListePartiePGN
                    ListePartiesPGN.Add(FichierPartiePgn.DecodePartiePGN(partie));
                fichierPartiePgn.NombrePartiesFichier.Text = ListePartiesPGN.Count.ToString()
                    + " partie(s) dans le fichier  " + Path.GetFileName(cheminFichier);
                fichierPartiePgn.AfficherListeParties(ListePartiesPGN);
                fichierPartiePgn.Show();
                fichierPartiePgn.BringToFront();
                MontrePartiesPGN.Enabled = true; // Active le bouton pour masquer/afficher la liste
            }
            catch (Exception ex)
            {   // fichier illisible ou contenu inattendu (erreurs variées : lecture, décodage) : expliqué et noté dans le journal
                Journal.Erreur("Ouverture du fichier PGN " + ChargerPartiesPgn.FileName, ex);
                KryptonMessageBox.Show("Impossible de lire ce fichier PGN :\n" + ex.Message, "Ouvrir fichier PGN",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            }
        }
        private void ChargePositionFen_Click(object sender, EventArgs e)
        {
            if (ChargerPositionFen.ShowDialog() != DialogResult.OK)
                return;     // annulé : la partie en cours ne change pas
            string[] positions;     // une position par ligne (un fichier peut en contenir plusieurs : on charge la première)
            try
            {
                positions = File.ReadAllLines(Path.GetFullPath(ChargerPositionFen.FileName)).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
            {
                Journal.Erreur("Lecture du fichier FEN " + ChargerPositionFen.FileName, ex);
                KryptonMessageBox.Show("Lecture impossible : " + ex.Message, "Chargement FEN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }
            string contenuFen = positions.Length > 0 ? ChargementPartie.NormaliseFen(positions[0]) : "";
            string erreur = ChargementPartie.ErreurFen(contenuFen);
            if (erreur != null)
            {   // FEN mal formée : on ne la lit pas (elle ferait planter la lecture ou donnerait une position incohérente)
                KryptonMessageBox.Show($"Position FEN refusée : {erreur}.\n\n{contenuFen}", "Chargement FEN",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }
            ChargeFenDansLaPartie(contenuFen, "Fen chargé : ", positions.Length > 1 ? $"   Fichier de {positions.Length} positions : la première est chargée" : "...");
        }
        private void BoutonSaisiePosition_Click(object sender, EventArgs e)
        {   // Saisie d'une position à la main (fenêtre SaisiePosition), en partant de la position affichée ; OK : elle est jouée
            // comme une FEN chargée (nouvelle partie, l'humain au trait) ; Annuler ne change rien
            string fenAffichee = LogiqueMouvements.CalculerSur(_positionAffichee ?? LogiqueMouvements.PositionActuelle, LogiqueMouvements.RetourneChaineFenActuel);
            using SaisiePosition saisie = new(fenAffichee, _vue.ImagePiece, _vue.CaseClaire, _vue.CaseSombre, _vue.CoteNoir);
            if (saisie.ShowDialog(this) == DialogResult.OK)
                ChargeFenDansLaPartie(saisie.FenSaisie, "Position saisie : ", "...");
        }
        private void ChargeFenDansLaPartie(string contenuFen, string libelle, string detail)
        {   // Une position (FEN déjà contrôlée par ChargementPartie.ErreurFen) devient une nouvelle partie : fichier FEN ou saisie
            AbandonneReflexion();   // chargement d'une position
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            ListeParties.Clear();    // On vide la liste des parties
            ListePartiesPGN.Clear(); // On vide la liste des parties PGN
            ChargementPartie.ChargerPosition(contenuFen, _partie);     // l'humain joue le camp au trait, le moteur lui répond
            VarianteMoteurUci1.Text = libelle + contenuFen;
            _vue.EffaceDernierCoup();                // les cases du dernier coup de la partie précédente
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            _clickCaseSource = _visuSymbole = true;
            PartieEnCours.CoupsPartiePGN = PartieEnCours.Result = PartieEnCours.CompteDePLy = PartieEnCours.Ronde = "";
            PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Lieu = "Maison";
            AfficheJoueurs("", "", "", "");     // position chargée : ce n'est la partie ni de l'humain ni du moteur, noms vides
            NouvellePendule();                  // la partie qui commence à cette position suit la cadence choisie
            InformationPourJoueur.Text = "Trait aux " + NomCamp(QuiJoue);
            PlateauEnable(true);   // On active le plateau pour pouvoir jouer à partir de la position chargée
            AfficheCoupsBibliotheque(contenuFen);
            // Le cadre vert est court : les détails du chargement vont dans les lignes de variantes 2 et 3 (inutilisées à ce moment)
            InformationsPartie.Text = "Position chargée";
            VarianteMoteurUci2.Text = detail;
            VarianteMoteurUci3.Text = _pendule != null ? $"   Pendule {_pendule.Cadence.Nom} : elle démarre au premier coup" : "...";
            MetAJourCommandes();
        }
        private void EnregistrerPgn_Click(object sender, EventArgs e)
        {   // Enregistre la partie au format PGN
            try
            {
                // La partie au format PGN, avec les temps de la pendule après chaque coup (seulement dans le fichier, pas à l'affichage)
                // Fins de ligne Windows (CRLF) dans le fichier : certains logiciels n'affichent pas un simple "\n" comme un retour à la ligne
                string contenuPgn = GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Intl", pourFichier: true).Replace("\n", "\r\n");
                // Ecriture du fichier PGN (Partie complète + en-tête)
                {
                    SauvegardeFichier.OverwritePrompt = false;      // Permet d'éviter l'affichage de 2 boites de dialogue si le fichier choisi existe...
                    DialogResult Reponse = SauvegardeFichier.ShowDialog();  // l'utilisateur doit rentrer le nom du fichier PGN
                    if (Reponse == DialogResult.OK)                         // On ne sauvegarde que si l'utilisateur est d'accord
                    {
                        string cheminPgn = SauvegardeFichier.FileName;
                        if (File.Exists(cheminPgn))                         // Si le fichier existe déjà
                        {   // On demande à l'utilisateur s'il veut écraser le fichier ou ajouter la partie
                            DialogResult resultat = KryptonMessageBox.Show("ATTENTION, le fichier " + Path.GetFileName(cheminPgn) + " existe déjà. \nCliquer Oui pour ajouter la partie à la fin." +
                               "\nCliquer Non pour écraser le fichier existant.\nCancel pour afficher le fichier PGN.",
                               "Fichier existant", KryptonMessageBoxButtons.YesNoCancel, KryptonMessageBoxIcon.Warning);
                            if (resultat == DialogResult.No)
                            {   // Écrase le fichier existant avec la nouvelle partie
                                File.WriteAllText(cheminPgn, contenuPgn);
                                InformationPourJoueur.Text = "La partie est écrite dans le fichier " + Path.GetFileName(cheminPgn);
                            }
                            else if (resultat == DialogResult.Yes)
                            {   // Ajoute la nouvelle partie à la fin du fichier existant, sans réécrire ce qu'il contient (relu puis réécrit
                                // en UTF-8, un fichier aux lignes Latin-1 aurait perdu ses accents)
                                File.AppendAllText(cheminPgn, "\r\n\r\n" + contenuPgn);
                                InformationPourJoueur.Text = "La partie est ajoutée dans le fichier " + Path.GetFileName(cheminPgn);
                            }
                            else if (resultat == DialogResult.Cancel)
                            {   // Affiche la nouvelle partie (sans les temps de la pendule : ils ne vont que dans le fichier)
                                KryptonMessageBox.Show($"Fichier PGN :\n {GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Intl")}", "Affichage fichier PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                            }
                        }
                        else
                        {   // Si le fichier n'existe pas, écrire simplement la nouvelle partie
                            File.WriteAllText(cheminPgn, contenuPgn);               // Ecriture du fichier au format PGN
                        }
                    }
                }
            }
            catch (Exception ex)
            {   // écriture impossible (dossier protégé, disque plein, fichier verrouillé...) : expliqué et noté dans le journal
                Journal.Erreur("Enregistrement du fichier PGN", ex);
                KryptonMessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur méthode Enregistrer PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                Debug.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }
        private void EnregistrerFen_Click(object sender, EventArgs e)
        {   // Ecriture du fichier FEN (position courante)
            {
                DialogResult Reponse = SauvegardeFen.ShowDialog();      // l'utilisateur doit rentrer le nom du fichier FEN
                if (Reponse == DialogResult.OK)                         // On ne sauvegarde que si l'utilisateur est d'accord
                {
                    string CheminFen = SauvegardeFen.FileName;
                    File.WriteAllText(CheminFen, LogiqueMouvements.RetourneChaineFenActuel());  // Ecriture du fichier au format FEN
                }
            }
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des parties PGN (sélection/lecture)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        public void ChargerPartieDepuisPgn(PartieEchecsPGN partie)
        {   // --- Charge UNE partie depuis un fichier PGN lorsque'on double-clique ---
            AbandonneReflexion();
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            _vue.EffaceDernierCoup();    // les cases du dernier coup de la partie précédente
            SupprimePendule();      // partie PGN : pas de pendule
            Debug.WriteLine("ChargerPartieDepuisPgn / :  " + partie.White + " vs " + partie.Black + "   Résultat : " + partie.Result);
            PartieEnCours.Tournoi = partie.Tournoi;
            PartieEnCours.Lieu = partie.Lieu;
            PartieEnCours.Date = partie.Date;
            PartieEnCours.Ronde = partie.Ronde;
            AfficheJoueurs(partie.White, partie.WhiteElo, partie.Black, partie.BlackElo);
            PartieEnCours.Result = InformationsPartie.Text = partie.Result;
            PartieEnCours.ECO = partie.ECO;
            PartieEnCours.CompteDePLy = partie.CompteDePLy;
            PartieEnCours.CoupsPartiePGN = partie.CoupsPartiePGN;
            PartieEnCours.TimeControl = partie.TimeControl;
            InformationPourJoueur.Text = partie.Tournoi + " / ronde " + partie.Ronde;
            StatusProgramme.Text = $"{partie.White} vs {partie.Black}";
            ScoreMoteur.Text = InformationsPartie.Text = "Résultat : " + partie.Result;
            VarianteMoteurCourante.Text = "";
            Debug.WriteLine($"Partie en PGN : {partie.CoupsPartiePGN}");
            // Rejeu des coups (depuis la balise FEN s'il y en a une) ; la partie finit en lecture seule
            ResultatChargementPgn chargement = ChargementPartie.ChargerPartiePgn(partie, _partie);
            PartieEnCours.CompteDePLy = chargement.DemiCoupsJoues.ToString();   // PlyCount : demi-coups réellement rejoués (la balise du fichier peut manquer ou être fausse)
            if (chargement.FenIncomplete)
                KryptonMessageBox.Show($"La position de départ de cette partie (balise FEN) est refusée : {ChargementPartie.ErreurFen(partie.Fen)}.\n" +
                    "Les coups sont joués depuis la position initiale.",
                    "Partie PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            if (chargement.CoupIllisible != null)
                KryptonMessageBox.Show($"Coup illisible ou illégal : « {chargement.CoupIllisible} » (demi-coup n° {chargement.DemiCoupsJoues + 1}).\n" +
                    "La partie est chargée jusqu'au coup précédent.", "Partie PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            AfficheResultatPartiePgn();
        }
        private void AfficheResultatPartiePgn()
        {   // Partie PGN rejouée : son résultat est affiché, et l'échiquier montre la partie depuis le début
            switch (PartieEnCours.Result)       // Et on ajoute le résultat
            {
                case "1-0":
                    InformationsPartie.Text = "Résultat : 1-0 Gain Blanc";
                    break;
                case "0-1":
                    InformationsPartie.Text = "Résultat : 0-1 Gain Noir";
                    break;
                case "1/2-1/2":
                    InformationsPartie.Text = "Résultat : 1/2-1/2 Nulle";
                    break;
                case "*":
                    InformationsPartie.Text = "Résultat : * Indéterminé";
                    break;
                default:
                    Debug.WriteLine($"Pas de résultat défini : {PartieEnCours.Result}");
                    break;
            }
            // La partie reste sur sa position finale ; elle est en lecture seule (parcours et analyse), et on l'affiche depuis le début
            PlateauEnable(false);
            MetAJourCommandes();
            string resultat = InformationsPartie.Text;
            AfficheCoupDeLaPartie(IndexPremierePosition);
            InformationsPartie.Text = resultat;     // on garde le résultat de la partie affiché
        }
    }
}
