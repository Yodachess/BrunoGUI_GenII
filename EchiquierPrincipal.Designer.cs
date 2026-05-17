// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

namespace BrunoGUI_GenII
{
    partial class EchiquierPrincipal
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EchiquierPrincipal));
            InformationPourJoueur = new System.Windows.Forms.Label();
            MontreDonneesUci = new Krypton.Toolkit.KryptonButton();
            InverseEchiquier = new Krypton.Toolkit.KryptonButton();
            MenuInterfaceGraphique = new System.Windows.Forms.MenuStrip();
            FichierMenu = new System.Windows.Forms.ToolStripMenuItem();
            ChargePartiesPgn = new System.Windows.Forms.ToolStripMenuItem();
            ChargePositionFen = new System.Windows.Forms.ToolStripMenuItem();
            EnregistrerPgn = new System.Windows.Forms.ToolStripMenuItem();
            EnregistrerFen = new System.Windows.Forms.ToolStripMenuItem();
            Quitter = new System.Windows.Forms.ToolStripMenuItem();
            PartieMenu = new System.Windows.Forms.ToolStripMenuItem();
            NouvellePartieStockfish = new System.Windows.Forms.ToolStripMenuItem();
            ParametresDeBase = new System.Windows.Forms.ToolStripMenuItem();
            ParametresAvances = new System.Windows.Forms.ToolStripMenuItem();
            BtnMiseAJour = new System.Windows.Forms.ToolStripMenuItem();
            nouvellePartieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            HumainOrdinateur = new System.Windows.Forms.ToolStripMenuItem();
            OrdinateurHumain = new System.Windows.Forms.ToolStripMenuItem();
            HumainContreHumain = new System.Windows.Forms.ToolStripMenuItem();
            StopMoteur = new System.Windows.Forms.ToolStripMenuItem();
            moteursBibliothèquesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            SelectionAutreMoteur = new System.Windows.Forms.ToolStripMenuItem();
            SelectionBibliothèque = new System.Windows.Forms.ToolStripMenuItem();
            RodentIV = new System.Windows.Forms.ToolStripMenuItem();
            Sargon1_1978 = new System.Windows.Forms.ToolStripMenuItem();
            OptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
            Personnaliser = new System.Windows.Forms.ToolStripMenuItem();
            CaseSombre = new System.Windows.Forms.ToolStripMenuItem();
            CaseClaire = new System.Windows.Forms.ToolStripMenuItem();
            VisualiserPgn = new System.Windows.Forms.ToolStripMenuItem();
            OptionBalises = new System.Windows.Forms.ToolStripMenuItem();
            AideDocumentation = new System.Windows.Forms.ToolStripMenuItem();
            Apropos = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            SauvegardeFen = new System.Windows.Forms.SaveFileDialog();
            SauvegardeFichier = new System.Windows.Forms.SaveFileDialog();
            CouleurDialogue = new System.Windows.Forms.ColorDialog();
            GroupPromo = new System.Windows.Forms.GroupBox();
            Promo3 = new System.Windows.Forms.PictureBox();
            Promo2 = new System.Windows.Forms.PictureBox();
            Promo1 = new System.Windows.Forms.PictureBox();
            Promo0 = new System.Windows.Forms.PictureBox();
            BoutonBalises = new Krypton.Toolkit.KryptonButton();
            RetourArriere = new Krypton.Toolkit.KryptonButton();
            VisualisationPgn = new Krypton.Toolkit.KryptonButton();
            kryptonStatusStrip1 = new Krypton.Toolkit.KryptonStatusStrip();
            StatusProgramme = new System.Windows.Forms.ToolStripStatusLabel();
            EvaluationUci = new System.Windows.Forms.ToolStripStatusLabel();
            ScoreMoteur = new System.Windows.Forms.ToolStripStatusLabel();
            VarianteMoteurCourante = new System.Windows.Forms.ToolStripStatusLabel();
            VarianteMoteurUci1 = new System.Windows.Forms.RichTextBox();
            VarianteMoteurUci2 = new System.Windows.Forms.RichTextBox();
            VarianteMoteurUci3 = new System.Windows.Forms.RichTextBox();
            AnalysePosition = new Krypton.Toolkit.KryptonButton();
            groupeJoueurs = new System.Windows.Forms.GroupBox();
            LabelJoueurNoir = new System.Windows.Forms.Label();
            EloNoir = new System.Windows.Forms.Label();
            LabelJoueurBlanc = new System.Windows.Forms.Label();
            EloBlanc = new System.Windows.Forms.Label();
            InformationsPartie = new System.Windows.Forms.Label();
            MontreVariantesUci = new Krypton.Toolkit.KryptonButton();
            ListeCoupsBouton = new Krypton.Toolkit.KryptonButton();
            Plateau = new System.Windows.Forms.PictureBox();
            BoutonGainBlanc = new Krypton.Toolkit.KryptonButton();
            BoutonGainNoir = new Krypton.Toolkit.KryptonButton();
            BoutonNulle = new Krypton.Toolkit.KryptonButton();
            KryptonQuitter = new Krypton.Toolkit.KryptonButton();
            CoupsBibliothèqueBox = new System.Windows.Forms.RichTextBox();
            OuvertureChoixBibliothèque = new System.Windows.Forms.OpenFileDialog();
            OuvertureChoixMoteur = new System.Windows.Forms.OpenFileDialog();
            CoupsBibliothèque = new System.Windows.Forms.RichTextBox();
            OrdinateurJoue = new Krypton.Toolkit.KryptonButton();
            MontrePartiesPGN = new Krypton.Toolkit.KryptonButton();
            ChargerPartiesPgn = new System.Windows.Forms.OpenFileDialog();
            groupParcoursPartie = new System.Windows.Forms.GroupBox();
            BoutonFin = new Krypton.Toolkit.KryptonButton();
            BoutonDebut = new Krypton.Toolkit.KryptonButton();
            BoutonSuivant = new Krypton.Toolkit.KryptonButton();
            BoutonPrecedent = new Krypton.Toolkit.KryptonButton();
            ActiveBibliothèque = new System.Windows.Forms.CheckBox();
            ActiveSon = new System.Windows.Forms.CheckBox();
            groupBoxTempsReflexion = new System.Windows.Forms.GroupBox();
            labelTempsReflexion = new System.Windows.Forms.Label();
            TrackBarTempsReflexion = new System.Windows.Forms.TrackBar();
            KryptonApropos = new Krypton.Toolkit.KryptonButton();
            ActiveAléatoire = new System.Windows.Forms.CheckBox();
            SaisiePartieBouton = new Krypton.Toolkit.KryptonButton();
            ChargerPositionFen = new System.Windows.Forms.OpenFileDialog();
            MenuInterfaceGraphique.SuspendLayout();
            GroupPromo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Promo3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Promo2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Promo1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Promo0).BeginInit();
            kryptonStatusStrip1.SuspendLayout();
            groupeJoueurs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Plateau).BeginInit();
            groupParcoursPartie.SuspendLayout();
            groupBoxTempsReflexion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TrackBarTempsReflexion).BeginInit();
            SuspendLayout();
            // 
            // InformationPourJoueur
            // 
            InformationPourJoueur.BackColor = System.Drawing.Color.LightSteelBlue;
            InformationPourJoueur.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            InformationPourJoueur.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            InformationPourJoueur.ForeColor = System.Drawing.Color.DarkBlue;
            InformationPourJoueur.Location = new System.Drawing.Point(12, 68);
            InformationPourJoueur.Name = "InformationPourJoueur";
            InformationPourJoueur.Size = new System.Drawing.Size(254, 19);
            InformationPourJoueur.TabIndex = 1;
            InformationPourJoueur.Text = "Information Pour Joueur";
            InformationPourJoueur.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MontreDonneesUci
            // 
            MontreDonneesUci.Location = new System.Drawing.Point(542, 666);
            MontreDonneesUci.Name = "MontreDonneesUci";
            MontreDonneesUci.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            MontreDonneesUci.Size = new System.Drawing.Size(130, 25);
            MontreDonneesUci.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            MontreDonneesUci.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            MontreDonneesUci.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            MontreDonneesUci.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            MontreDonneesUci.StateCommon.Border.Rounding = 20F;
            MontreDonneesUci.StateCommon.Border.Width = 3;
            MontreDonneesUci.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            MontreDonneesUci.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            MontreDonneesUci.TabIndex = 2;
            MontreDonneesUci.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            MontreDonneesUci.Values.Text = "Affiche protocole";
            MontreDonneesUci.Click += MontreDonneesUci_Click;
            // 
            // InverseEchiquier
            // 
            InverseEchiquier.Location = new System.Drawing.Point(542, 93);
            InverseEchiquier.Name = "InverseEchiquier";
            InverseEchiquier.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            InverseEchiquier.Size = new System.Drawing.Size(130, 25);
            InverseEchiquier.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            InverseEchiquier.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            InverseEchiquier.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            InverseEchiquier.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            InverseEchiquier.StateCommon.Border.Rounding = 20F;
            InverseEchiquier.StateCommon.Border.Width = 3;
            InverseEchiquier.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            InverseEchiquier.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            InverseEchiquier.TabIndex = 4;
            InverseEchiquier.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            InverseEchiquier.Values.Text = "Tourne Echiquier";
            InverseEchiquier.Click += InverseEchiquier_Click;
            // 
            // MenuInterfaceGraphique
            // 
            MenuInterfaceGraphique.Font = new System.Drawing.Font("Segoe UI", 9F);
            MenuInterfaceGraphique.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { FichierMenu, PartieMenu, nouvellePartieToolStripMenuItem, moteursBibliothèquesToolStripMenuItem, OptionsMenu, Apropos, toolStripMenuItem2 });
            MenuInterfaceGraphique.Location = new System.Drawing.Point(0, 0);
            MenuInterfaceGraphique.Name = "MenuInterfaceGraphique";
            MenuInterfaceGraphique.Size = new System.Drawing.Size(684, 24);
            MenuInterfaceGraphique.TabIndex = 6;
            MenuInterfaceGraphique.Text = "Menu Interface Graphique";
            // 
            // FichierMenu
            // 
            FichierMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ChargePartiesPgn, ChargePositionFen, EnregistrerPgn, EnregistrerFen, Quitter });
            FichierMenu.Name = "FichierMenu";
            FichierMenu.Size = new System.Drawing.Size(54, 20);
            FichierMenu.Text = "Fichier";
            // 
            // ChargePartiesPgn
            // 
            ChargePartiesPgn.Image = (System.Drawing.Image)resources.GetObject("ChargePartiesPgn.Image");
            ChargePartiesPgn.Name = "ChargePartiesPgn";
            ChargePartiesPgn.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            ChargePartiesPgn.Size = new System.Drawing.Size(216, 22);
            ChargePartiesPgn.Text = "Ouvrir fichier PGN";
            ChargePartiesPgn.Click += ChargePartiesPgn_Click;
            // 
            // ChargePositionFen
            // 
            ChargePositionFen.Image = Properties.Resources.Fichier_FEN;
            ChargePositionFen.Name = "ChargePositionFen";
            ChargePositionFen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I;
            ChargePositionFen.Size = new System.Drawing.Size(216, 22);
            ChargePositionFen.Text = "Ouvrir fichier FEN";
            ChargePositionFen.Click += ChargePositionFen_Click;
            // 
            // EnregistrerPgn
            // 
            EnregistrerPgn.Image = (System.Drawing.Image)resources.GetObject("EnregistrerPgn.Image");
            EnregistrerPgn.Name = "EnregistrerPgn";
            EnregistrerPgn.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            EnregistrerPgn.Size = new System.Drawing.Size(216, 22);
            EnregistrerPgn.Text = "Enregistrer Partie";
            EnregistrerPgn.Click += EnregistrerPgn_Click;
            // 
            // EnregistrerFen
            // 
            EnregistrerFen.Image = (System.Drawing.Image)resources.GetObject("EnregistrerFen.Image");
            EnregistrerFen.Name = "EnregistrerFen";
            EnregistrerFen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F;
            EnregistrerFen.Size = new System.Drawing.Size(216, 22);
            EnregistrerFen.Text = "Enregistrer Position";
            EnregistrerFen.Click += EnregistrerFen_Click;
            // 
            // Quitter
            // 
            Quitter.Image = (System.Drawing.Image)resources.GetObject("Quitter.Image");
            Quitter.Name = "Quitter";
            Quitter.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4;
            Quitter.Size = new System.Drawing.Size(216, 22);
            Quitter.Text = "Quitter";
            Quitter.Click += KryptonQuitter_Click;
            // 
            // PartieMenu
            // 
            PartieMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { NouvellePartieStockfish, ParametresDeBase, ParametresAvances, BtnMiseAJour });
            PartieMenu.Name = "PartieMenu";
            PartieMenu.Size = new System.Drawing.Size(67, 20);
            PartieMenu.Text = "Stockfish";
            // 
            // NouvellePartieStockfish
            // 
            NouvellePartieStockfish.Image = (System.Drawing.Image)resources.GetObject("NouvellePartieStockfish.Image");
            NouvellePartieStockfish.Name = "NouvellePartieStockfish";
            NouvellePartieStockfish.Size = new System.Drawing.Size(242, 22);
            NouvellePartieStockfish.Text = "Nouvelle partie contre Stockfish";
            NouvellePartieStockfish.Click += NouvellePartieStockfish_Click;
            // 
            // ParametresDeBase
            // 
            ParametresDeBase.Image = (System.Drawing.Image)resources.GetObject("ParametresDeBase.Image");
            ParametresDeBase.Name = "ParametresDeBase";
            ParametresDeBase.Size = new System.Drawing.Size(242, 22);
            ParametresDeBase.Text = "Paramètres de base";
            ParametresDeBase.Click += ParametresDeBase_Click;
            // 
            // ParametresAvances
            // 
            ParametresAvances.Image = (System.Drawing.Image)resources.GetObject("ParametresAvances.Image");
            ParametresAvances.Name = "ParametresAvances";
            ParametresAvances.Size = new System.Drawing.Size(242, 22);
            ParametresAvances.Text = "Paramêtres Avancés";
            ParametresAvances.Click += ParametresAvances_Click;
            // 
            // BtnMiseAJour
            // 
            BtnMiseAJour.Name = "BtnMiseAJour";
            BtnMiseAJour.Size = new System.Drawing.Size(242, 22);
            BtnMiseAJour.Text = "Verifier mise à jour StockFish";
            BtnMiseAJour.Click += BtnMiseAJour_Click;
            // 
            // nouvellePartieToolStripMenuItem
            // 
            nouvellePartieToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { HumainOrdinateur, OrdinateurHumain, HumainContreHumain, StopMoteur });
            nouvellePartieToolStripMenuItem.Name = "nouvellePartieToolStripMenuItem";
            nouvellePartieToolStripMenuItem.Size = new System.Drawing.Size(99, 20);
            nouvellePartieToolStripMenuItem.Text = "Nouvelle Partie";
            // 
            // HumainOrdinateur
            // 
            HumainOrdinateur.Image = Properties.Resources.OrdiHumain;
            HumainOrdinateur.Name = "HumainOrdinateur";
            HumainOrdinateur.Size = new System.Drawing.Size(238, 22);
            HumainOrdinateur.Text = "Humain contre ordinateur";
            HumainOrdinateur.Click += HumainOrdinateur_Click;
            // 
            // OrdinateurHumain
            // 
            OrdinateurHumain.Image = Properties.Resources.HumainOrdi;
            OrdinateurHumain.Name = "OrdinateurHumain";
            OrdinateurHumain.Size = new System.Drawing.Size(238, 22);
            OrdinateurHumain.Text = "Ordinateur contre humain";
            OrdinateurHumain.Click += OrdinateurHumain_Click;
            // 
            // HumainContreHumain
            // 
            HumainContreHumain.Image = (System.Drawing.Image)resources.GetObject("HumainContreHumain.Image");
            HumainContreHumain.Name = "HumainContreHumain";
            HumainContreHumain.Size = new System.Drawing.Size(238, 22);
            HumainContreHumain.Text = "Entre Humains (ou saise partie)";
            HumainContreHumain.Click += SaisiePartieBouton_Click;
            // 
            // StopMoteur
            // 
            StopMoteur.Image = (System.Drawing.Image)resources.GetObject("StopMoteur.Image");
            StopMoteur.Name = "StopMoteur";
            StopMoteur.Size = new System.Drawing.Size(238, 22);
            StopMoteur.Text = "Stoppe le Moteur";
            StopMoteur.Click += StopMoteur_Click;
            // 
            // moteursBibliothèquesToolStripMenuItem
            // 
            moteursBibliothèquesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { SelectionAutreMoteur, SelectionBibliothèque, RodentIV, Sargon1_1978 });
            moteursBibliothèquesToolStripMenuItem.Name = "moteursBibliothèquesToolStripMenuItem";
            moteursBibliothèquesToolStripMenuItem.Size = new System.Drawing.Size(140, 20);
            moteursBibliothèquesToolStripMenuItem.Text = "Moteurs/Bibliothèques";
            // 
            // SelectionAutreMoteur
            // 
            SelectionAutreMoteur.Image = Properties.Resources.Ordinateur;
            SelectionAutreMoteur.Name = "SelectionAutreMoteur";
            SelectionAutreMoteur.Size = new System.Drawing.Size(233, 22);
            SelectionAutreMoteur.Text = "Sélectionnez un moteur";
            SelectionAutreMoteur.Click += SelectionAutreMoteur_Click;
            // 
            // SelectionBibliothèque
            // 
            SelectionBibliothèque.Image = (System.Drawing.Image)resources.GetObject("SelectionBibliothèque.Image");
            SelectionBibliothèque.Name = "SelectionBibliothèque";
            SelectionBibliothèque.Size = new System.Drawing.Size(233, 22);
            SelectionBibliothèque.Text = "Sélectionnez une bibliothèque";
            SelectionBibliothèque.Click += SelectionBibliothèque_Click;
            // 
            // RodentIV
            // 
            RodentIV.Image = (System.Drawing.Image)resources.GetObject("RodentIV.Image");
            RodentIV.Name = "RodentIV";
            RodentIV.Size = new System.Drawing.Size(233, 22);
            RodentIV.Text = "Rodent IV";
            RodentIV.Click += RodentIV_Click;
            // 
            // Sargon1_1978
            // 
            Sargon1_1978.Image = (System.Drawing.Image)resources.GetObject("Sargon1_1978.Image");
            Sargon1_1978.Name = "Sargon1_1978";
            Sargon1_1978.Size = new System.Drawing.Size(233, 22);
            Sargon1_1978.Text = "Sargon I 1978";
            Sargon1_1978.Click += Sargon1_1978_Click;
            // 
            // OptionsMenu
            // 
            OptionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { Personnaliser, VisualiserPgn, OptionBalises, AideDocumentation });
            OptionsMenu.Name = "OptionsMenu";
            OptionsMenu.Size = new System.Drawing.Size(61, 20);
            OptionsMenu.Text = "Options";
            // 
            // Personnaliser
            // 
            Personnaliser.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { CaseSombre, CaseClaire });
            Personnaliser.Image = (System.Drawing.Image)resources.GetObject("Personnaliser.Image");
            Personnaliser.Name = "Personnaliser";
            Personnaliser.Size = new System.Drawing.Size(192, 22);
            Personnaliser.Text = "Personnaliser";
            // 
            // CaseSombre
            // 
            CaseSombre.Image = (System.Drawing.Image)resources.GetObject("CaseSombre.Image");
            CaseSombre.Name = "CaseSombre";
            CaseSombre.Size = new System.Drawing.Size(143, 22);
            CaseSombre.Text = "Case Sombre";
            CaseSombre.Click += CaseSombre_Click;
            // 
            // CaseClaire
            // 
            CaseClaire.Image = (System.Drawing.Image)resources.GetObject("CaseClaire.Image");
            CaseClaire.Name = "CaseClaire";
            CaseClaire.Size = new System.Drawing.Size(143, 22);
            CaseClaire.Text = "Case Claire";
            CaseClaire.Click += CaseClaire_Click;
            // 
            // VisualiserPgn
            // 
            VisualiserPgn.Image = (System.Drawing.Image)resources.GetObject("VisualiserPgn.Image");
            VisualiserPgn.Name = "VisualiserPgn";
            VisualiserPgn.Size = new System.Drawing.Size(192, 22);
            VisualiserPgn.Text = "Partie PGN";
            VisualiserPgn.Click += VisualiserPgn_Click;
            // 
            // OptionBalises
            // 
            OptionBalises.Image = (System.Drawing.Image)resources.GetObject("OptionBalises.Image");
            OptionBalises.Name = "OptionBalises";
            OptionBalises.Size = new System.Drawing.Size(192, 22);
            OptionBalises.Text = "Saisie En-têtes PGN";
            OptionBalises.Click += BoutonBalises_Click;
            // 
            // AideDocumentation
            // 
            AideDocumentation.Image = (System.Drawing.Image)resources.GetObject("AideDocumentation.Image");
            AideDocumentation.Name = "AideDocumentation";
            AideDocumentation.Size = new System.Drawing.Size(192, 22);
            AideDocumentation.Text = "Aide / Documentation";
            AideDocumentation.Click += AideDocumentation_Click;
            // 
            // Apropos
            // 
            Apropos.Name = "Apropos";
            Apropos.Size = new System.Drawing.Size(95, 20);
            Apropos.Text = "A propos de ...";
            Apropos.Click += Apropos_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(22, 20);
            toolStripMenuItem2.Text = " ";
            // 
            // SauvegardeFen
            // 
            SauvegardeFen.Filter = "Fichier FEN (*.fen)|*.fen";
            // 
            // SauvegardeFichier
            // 
            SauvegardeFichier.Filter = "Fichier PGN (*.pgn)|*.pgn";
            // 
            // GroupPromo
            // 
            GroupPromo.BackColor = System.Drawing.Color.LightSteelBlue;
            GroupPromo.Controls.Add(Promo3);
            GroupPromo.Controls.Add(Promo2);
            GroupPromo.Controls.Add(Promo1);
            GroupPromo.Controls.Add(Promo0);
            GroupPromo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            GroupPromo.Location = new System.Drawing.Point(87, 320);
            GroupPromo.Name = "GroupPromo";
            GroupPromo.Size = new System.Drawing.Size(354, 111);
            GroupPromo.TabIndex = 7;
            GroupPromo.TabStop = false;
            GroupPromo.Text = "Pièces pour promotion   (Sélectionnez la pièce promue)";
            GroupPromo.Visible = false;
            // 
            // Promo3
            // 
            Promo3.BackColor = System.Drawing.Color.White;
            Promo3.Location = new System.Drawing.Point(265, 19);
            Promo3.Name = "Promo3";
            Promo3.Size = new System.Drawing.Size(80, 80);
            Promo3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            Promo3.TabIndex = 4;
            Promo3.TabStop = false;
            Promo3.Click += Promo0_Click;
            // 
            // Promo2
            // 
            Promo2.BackColor = System.Drawing.Color.White;
            Promo2.Location = new System.Drawing.Point(179, 20);
            Promo2.Name = "Promo2";
            Promo2.Size = new System.Drawing.Size(80, 80);
            Promo2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            Promo2.TabIndex = 3;
            Promo2.TabStop = false;
            Promo2.Click += Promo0_Click;
            // 
            // Promo1
            // 
            Promo1.BackColor = System.Drawing.Color.White;
            Promo1.Location = new System.Drawing.Point(93, 19);
            Promo1.Name = "Promo1";
            Promo1.Size = new System.Drawing.Size(80, 80);
            Promo1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            Promo1.TabIndex = 2;
            Promo1.TabStop = false;
            Promo1.Click += Promo0_Click;
            // 
            // Promo0
            // 
            Promo0.BackColor = System.Drawing.Color.White;
            Promo0.Location = new System.Drawing.Point(7, 20);
            Promo0.Name = "Promo0";
            Promo0.Size = new System.Drawing.Size(80, 80);
            Promo0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            Promo0.TabIndex = 1;
            Promo0.TabStop = false;
            Promo0.Click += Promo0_Click;
            // 
            // BoutonBalises
            // 
            BoutonBalises.Location = new System.Drawing.Point(540, 431);
            BoutonBalises.Name = "BoutonBalises";
            BoutonBalises.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonBalises.Size = new System.Drawing.Size(130, 25);
            BoutonBalises.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            BoutonBalises.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            BoutonBalises.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonBalises.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            BoutonBalises.StateCommon.Border.Rounding = 20F;
            BoutonBalises.StateCommon.Border.Width = 3;
            BoutonBalises.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            BoutonBalises.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            BoutonBalises.TabIndex = 8;
            BoutonBalises.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonBalises.Values.Text = "Entête PGN";
            BoutonBalises.Click += BoutonBalises_Click;
            // 
            // RetourArriere
            // 
            RetourArriere.Location = new System.Drawing.Point(542, 155);
            RetourArriere.Name = "RetourArriere";
            RetourArriere.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            RetourArriere.Size = new System.Drawing.Size(130, 25);
            RetourArriere.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            RetourArriere.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            RetourArriere.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            RetourArriere.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            RetourArriere.StateCommon.Border.Rounding = 20F;
            RetourArriere.StateCommon.Border.Width = 3;
            RetourArriere.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            RetourArriere.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            RetourArriere.TabIndex = 9;
            RetourArriere.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            RetourArriere.Values.Text = "Retour Arrière";
            RetourArriere.Click += RetourArriere_Click;
            // 
            // VisualisationPgn
            // 
            VisualisationPgn.Location = new System.Drawing.Point(540, 400);
            VisualisationPgn.Name = "VisualisationPgn";
            VisualisationPgn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            VisualisationPgn.Size = new System.Drawing.Size(130, 25);
            VisualisationPgn.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            VisualisationPgn.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            VisualisationPgn.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            VisualisationPgn.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            VisualisationPgn.StateCommon.Border.Rounding = 20F;
            VisualisationPgn.StateCommon.Border.Width = 3;
            VisualisationPgn.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            VisualisationPgn.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            VisualisationPgn.TabIndex = 10;
            VisualisationPgn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            VisualisationPgn.Values.Text = "Partie PGN";
            VisualisationPgn.Click += VisualisationPgn_Click;
            // 
            // kryptonStatusStrip1
            // 
            kryptonStatusStrip1.AutoSize = false;
            kryptonStatusStrip1.Font = new System.Drawing.Font("Segoe UI", 9F);
            kryptonStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { StatusProgramme, EvaluationUci, ScoreMoteur, VarianteMoteurCourante });
            kryptonStatusStrip1.Location = new System.Drawing.Point(0, 759);
            kryptonStatusStrip1.Name = "kryptonStatusStrip1";
            kryptonStatusStrip1.ProgressBars = null;
            kryptonStatusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            kryptonStatusStrip1.Size = new System.Drawing.Size(684, 26);
            kryptonStatusStrip1.TabIndex = 11;
            kryptonStatusStrip1.Text = "kryptonStatusStrip1";
            // 
            // StatusProgramme
            // 
            StatusProgramme.AutoSize = false;
            StatusProgramme.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            StatusProgramme.Name = "StatusProgramme";
            StatusProgramme.Size = new System.Drawing.Size(200, 21);
            StatusProgramme.Text = "Status du Programme";
            // 
            // EvaluationUci
            // 
            EvaluationUci.AutoSize = false;
            EvaluationUci.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            EvaluationUci.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            EvaluationUci.Name = "EvaluationUci";
            EvaluationUci.Size = new System.Drawing.Size(120, 21);
            EvaluationUci.Text = "Evaluation";
            // 
            // ScoreMoteur
            // 
            ScoreMoteur.AutoSize = false;
            ScoreMoteur.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            ScoreMoteur.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ScoreMoteur.Name = "ScoreMoteur";
            ScoreMoteur.Size = new System.Drawing.Size(90, 21);
            ScoreMoteur.Text = "Score Moteur";
            // 
            // VarianteMoteurCourante
            // 
            VarianteMoteurCourante.AutoSize = false;
            VarianteMoteurCourante.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom;
            VarianteMoteurCourante.Name = "VarianteMoteurCourante";
            VarianteMoteurCourante.Size = new System.Drawing.Size(260, 21);
            VarianteMoteurCourante.Text = "Variante en cours d'examen";
            // 
            // VarianteMoteurUci1
            // 
            VarianteMoteurUci1.BackColor = System.Drawing.Color.WhiteSmoke;
            VarianteMoteurUci1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            VarianteMoteurUci1.ForeColor = System.Drawing.Color.DarkGreen;
            VarianteMoteurUci1.Location = new System.Drawing.Point(10, 620);
            VarianteMoteurUci1.Multiline = false;
            VarianteMoteurUci1.Name = "VarianteMoteurUci1";
            VarianteMoteurUci1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            VarianteMoteurUci1.Size = new System.Drawing.Size(520, 20);
            VarianteMoteurUci1.TabIndex = 13;
            VarianteMoteurUci1.Text = "Affichage Variante UCI";
            // 
            // VarianteMoteurUci2
            // 
            VarianteMoteurUci2.BackColor = System.Drawing.Color.WhiteSmoke;
            VarianteMoteurUci2.ForeColor = System.Drawing.Color.DarkBlue;
            VarianteMoteurUci2.Location = new System.Drawing.Point(10, 640);
            VarianteMoteurUci2.Multiline = false;
            VarianteMoteurUci2.Name = "VarianteMoteurUci2";
            VarianteMoteurUci2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            VarianteMoteurUci2.Size = new System.Drawing.Size(520, 20);
            VarianteMoteurUci2.TabIndex = 21;
            VarianteMoteurUci2.Text = "Affichage Variante UCI";
            // 
            // VarianteMoteurUci3
            // 
            VarianteMoteurUci3.BackColor = System.Drawing.Color.WhiteSmoke;
            VarianteMoteurUci3.ForeColor = System.Drawing.Color.DarkBlue;
            VarianteMoteurUci3.Location = new System.Drawing.Point(10, 660);
            VarianteMoteurUci3.Multiline = false;
            VarianteMoteurUci3.Name = "VarianteMoteurUci3";
            VarianteMoteurUci3.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            VarianteMoteurUci3.Size = new System.Drawing.Size(520, 20);
            VarianteMoteurUci3.TabIndex = 22;
            VarianteMoteurUci3.Text = "Affichage Variante UCI";
            // 
            // AnalysePosition
            // 
            AnalysePosition.Location = new System.Drawing.Point(542, 62);
            AnalysePosition.Name = "AnalysePosition";
            AnalysePosition.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            AnalysePosition.Size = new System.Drawing.Size(130, 25);
            AnalysePosition.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            AnalysePosition.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            AnalysePosition.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            AnalysePosition.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            AnalysePosition.StateCommon.Border.Rounding = 20F;
            AnalysePosition.StateCommon.Border.Width = 3;
            AnalysePosition.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            AnalysePosition.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            AnalysePosition.TabIndex = 23;
            AnalysePosition.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            AnalysePosition.Values.Text = "Analyse Position";
            AnalysePosition.Click += AnalysePosition_Click;
            // 
            // groupeJoueurs
            // 
            groupeJoueurs.BackColor = System.Drawing.Color.LightGray;
            groupeJoueurs.Controls.Add(LabelJoueurNoir);
            groupeJoueurs.Controls.Add(EloNoir);
            groupeJoueurs.Controls.Add(LabelJoueurBlanc);
            groupeJoueurs.Controls.Add(EloBlanc);
            groupeJoueurs.Location = new System.Drawing.Point(10, 28);
            groupeJoueurs.Name = "groupeJoueurs";
            groupeJoueurs.Size = new System.Drawing.Size(518, 37);
            groupeJoueurs.TabIndex = 24;
            groupeJoueurs.TabStop = false;
            groupeJoueurs.Text = "Joueurs";
            // 
            // LabelJoueurNoir
            // 
            LabelJoueurNoir.BackColor = System.Drawing.Color.Black;
            LabelJoueurNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LabelJoueurNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LabelJoueurNoir.ForeColor = System.Drawing.Color.White;
            LabelJoueurNoir.Location = new System.Drawing.Point(262, 14);
            LabelJoueurNoir.Name = "LabelJoueurNoir";
            LabelJoueurNoir.Size = new System.Drawing.Size(160, 20);
            LabelJoueurNoir.TabIndex = 3;
            LabelJoueurNoir.Text = "Joueur Noir";
            LabelJoueurNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EloNoir
            // 
            EloNoir.BackColor = System.Drawing.Color.Black;
            EloNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            EloNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            EloNoir.ForeColor = System.Drawing.Color.White;
            EloNoir.Location = new System.Drawing.Point(428, 14);
            EloNoir.Name = "EloNoir";
            EloNoir.Size = new System.Drawing.Size(90, 20);
            EloNoir.TabIndex = 2;
            EloNoir.Text = "Elo Noir";
            EloNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelJoueurBlanc
            // 
            LabelJoueurBlanc.BackColor = System.Drawing.Color.White;
            LabelJoueurBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            LabelJoueurBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            LabelJoueurBlanc.Location = new System.Drawing.Point(2, 14);
            LabelJoueurBlanc.Name = "LabelJoueurBlanc";
            LabelJoueurBlanc.Size = new System.Drawing.Size(160, 20);
            LabelJoueurBlanc.TabIndex = 1;
            LabelJoueurBlanc.Text = "Joueur Blanc";
            LabelJoueurBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EloBlanc
            // 
            EloBlanc.BackColor = System.Drawing.Color.White;
            EloBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            EloBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            EloBlanc.Location = new System.Drawing.Point(166, 14);
            EloBlanc.Name = "EloBlanc";
            EloBlanc.Size = new System.Drawing.Size(90, 20);
            EloBlanc.TabIndex = 0;
            EloBlanc.Text = "Elo Blanc";
            EloBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InformationsPartie
            // 
            InformationsPartie.BackColor = System.Drawing.Color.WhiteSmoke;
            InformationsPartie.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            InformationsPartie.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            InformationsPartie.ForeColor = System.Drawing.Color.DarkGreen;
            InformationsPartie.Location = new System.Drawing.Point(272, 68);
            InformationsPartie.Name = "InformationsPartie";
            InformationsPartie.Size = new System.Drawing.Size(256, 19);
            InformationsPartie.TabIndex = 25;
            InformationsPartie.Text = "Informations Partie";
            InformationsPartie.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MontreVariantesUci
            // 
            MontreVariantesUci.Location = new System.Drawing.Point(542, 635);
            MontreVariantesUci.Name = "MontreVariantesUci";
            MontreVariantesUci.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            MontreVariantesUci.Size = new System.Drawing.Size(130, 25);
            MontreVariantesUci.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            MontreVariantesUci.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            MontreVariantesUci.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            MontreVariantesUci.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            MontreVariantesUci.StateCommon.Border.Rounding = 20F;
            MontreVariantesUci.StateCommon.Border.Width = 3;
            MontreVariantesUci.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            MontreVariantesUci.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            MontreVariantesUci.TabIndex = 26;
            MontreVariantesUci.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            MontreVariantesUci.Values.Text = "Masque variantes";
            MontreVariantesUci.Click += MontreVariantesUci_Click;
            // 
            // ListeCoupsBouton
            // 
            ListeCoupsBouton.Location = new System.Drawing.Point(542, 186);
            ListeCoupsBouton.Name = "ListeCoupsBouton";
            ListeCoupsBouton.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            ListeCoupsBouton.Size = new System.Drawing.Size(130, 25);
            ListeCoupsBouton.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            ListeCoupsBouton.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            ListeCoupsBouton.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ListeCoupsBouton.StateCommon.Border.Rounding = 20F;
            ListeCoupsBouton.StateCommon.Border.Width = 3;
            ListeCoupsBouton.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            ListeCoupsBouton.StateNormal.Border.Color1 = System.Drawing.Color.DarkGray;
            ListeCoupsBouton.StateNormal.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ListeCoupsBouton.StateNormal.Border.Rounding = 20F;
            ListeCoupsBouton.StateNormal.Border.Width = 3;
            ListeCoupsBouton.TabIndex = 28;
            ListeCoupsBouton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            ListeCoupsBouton.Values.Text = "Liste des coups";
            ListeCoupsBouton.Click += ListeCoupsBouton_Click;
            // 
            // Plateau
            // 
            Plateau.ErrorImage = Properties.Resources.Plateau_gris_GillSanOK;
            Plateau.Image = Properties.Resources.Plateau_gris_GillSanOK;
            Plateau.InitialImage = (System.Drawing.Image)resources.GetObject("Plateau.InitialImage");
            Plateau.Location = new System.Drawing.Point(10, 90);
            Plateau.Name = "Plateau";
            Plateau.Size = new System.Drawing.Size(520, 520);
            Plateau.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            Plateau.TabIndex = 3;
            Plateau.TabStop = false;
            // 
            // BoutonGainBlanc
            // 
            BoutonGainBlanc.Location = new System.Drawing.Point(570, 219);
            BoutonGainBlanc.Name = "BoutonGainBlanc";
            BoutonGainBlanc.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonGainBlanc.Size = new System.Drawing.Size(78, 15);
            BoutonGainBlanc.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            BoutonGainBlanc.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            BoutonGainBlanc.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonGainBlanc.StateCommon.Border.Rounding = 20F;
            BoutonGainBlanc.StateCommon.Border.Width = 1;
            BoutonGainBlanc.TabIndex = 29;
            BoutonGainBlanc.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonGainBlanc.Values.Text = "Gain blanc";
            BoutonGainBlanc.Click += BoutonGainBlanc_Click;
            // 
            // BoutonGainNoir
            // 
            BoutonGainNoir.Location = new System.Drawing.Point(570, 240);
            BoutonGainNoir.Name = "BoutonGainNoir";
            BoutonGainNoir.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonGainNoir.Size = new System.Drawing.Size(78, 15);
            BoutonGainNoir.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            BoutonGainNoir.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            BoutonGainNoir.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonGainNoir.StateCommon.Border.Rounding = 20F;
            BoutonGainNoir.StateCommon.Border.Width = 1;
            BoutonGainNoir.TabIndex = 30;
            BoutonGainNoir.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonGainNoir.Values.Text = "Gain noir";
            BoutonGainNoir.Click += BoutonGainNoir_Click;
            // 
            // BoutonNulle
            // 
            BoutonNulle.Location = new System.Drawing.Point(570, 261);
            BoutonNulle.Name = "BoutonNulle";
            BoutonNulle.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonNulle.Size = new System.Drawing.Size(78, 15);
            BoutonNulle.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            BoutonNulle.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            BoutonNulle.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonNulle.StateCommon.Border.Rounding = 20F;
            BoutonNulle.StateCommon.Border.Width = 1;
            BoutonNulle.TabIndex = 31;
            BoutonNulle.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonNulle.Values.Text = "Nulle";
            BoutonNulle.Click += BoutonNulle_Click;
            // 
            // KryptonQuitter
            // 
            KryptonQuitter.Location = new System.Drawing.Point(542, 726);
            KryptonQuitter.Name = "KryptonQuitter";
            KryptonQuitter.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            KryptonQuitter.Size = new System.Drawing.Size(130, 25);
            KryptonQuitter.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            KryptonQuitter.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            KryptonQuitter.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            KryptonQuitter.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            KryptonQuitter.StateCommon.Border.Rounding = 20F;
            KryptonQuitter.StateCommon.Border.Width = 3;
            KryptonQuitter.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            KryptonQuitter.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            KryptonQuitter.TabIndex = 32;
            KryptonQuitter.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            KryptonQuitter.Values.Text = "Quitter";
            KryptonQuitter.Click += KryptonQuitter_Click;
            // 
            // CoupsBibliothèqueBox
            // 
            CoupsBibliothèqueBox.BackColor = System.Drawing.Color.WhiteSmoke;
            CoupsBibliothèqueBox.Location = new System.Drawing.Point(542, 462);
            CoupsBibliothèqueBox.Name = "CoupsBibliothèqueBox";
            CoupsBibliothèqueBox.Size = new System.Drawing.Size(120, 164);
            CoupsBibliothèqueBox.TabIndex = 33;
            CoupsBibliothèqueBox.Text = "";
            // 
            // OuvertureChoixBibliothèque
            // 
            OuvertureChoixBibliothèque.FileName = "bibliothèque";
            OuvertureChoixBibliothèque.Filter = "Fichier BIN (*.bin)|*.bin";
            // 
            // OuvertureChoixMoteur
            // 
            OuvertureChoixMoteur.FileName = "moteur";
            OuvertureChoixMoteur.Filter = "Fichier EXE (*.exe)|*.exe";
            // 
            // CoupsBibliothèque
            // 
            CoupsBibliothèque.BackColor = System.Drawing.Color.Lavender;
            CoupsBibliothèque.ForeColor = System.Drawing.Color.DarkBlue;
            CoupsBibliothèque.Location = new System.Drawing.Point(12, 686);
            CoupsBibliothèque.Multiline = false;
            CoupsBibliothèque.Name = "CoupsBibliothèque";
            CoupsBibliothèque.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            CoupsBibliothèque.Size = new System.Drawing.Size(520, 20);
            CoupsBibliothèque.TabIndex = 34;
            CoupsBibliothèque.Text = "Bibliothèque d'ouverture sélectionnée";
            // 
            // OrdinateurJoue
            // 
            OrdinateurJoue.Location = new System.Drawing.Point(542, 124);
            OrdinateurJoue.Name = "OrdinateurJoue";
            OrdinateurJoue.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            OrdinateurJoue.Size = new System.Drawing.Size(130, 25);
            OrdinateurJoue.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            OrdinateurJoue.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            OrdinateurJoue.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            OrdinateurJoue.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            OrdinateurJoue.StateCommon.Border.Rounding = 20F;
            OrdinateurJoue.StateCommon.Border.Width = 3;
            OrdinateurJoue.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            OrdinateurJoue.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            OrdinateurJoue.TabIndex = 35;
            OrdinateurJoue.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            OrdinateurJoue.Values.Text = "Ordinateur joue";
            OrdinateurJoue.Click += OrdinateurJoue_Click;
            // 
            // MontrePartiesPGN
            // 
            MontrePartiesPGN.Location = new System.Drawing.Point(542, 282);
            MontrePartiesPGN.Name = "MontrePartiesPGN";
            MontrePartiesPGN.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            MontrePartiesPGN.Size = new System.Drawing.Size(130, 25);
            MontrePartiesPGN.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            MontrePartiesPGN.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            MontrePartiesPGN.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            MontrePartiesPGN.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            MontrePartiesPGN.StateCommon.Border.Rounding = 20F;
            MontrePartiesPGN.StateCommon.Border.Width = 3;
            MontrePartiesPGN.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            MontrePartiesPGN.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            MontrePartiesPGN.TabIndex = 36;
            MontrePartiesPGN.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            MontrePartiesPGN.Values.Text = "Affiche liste parties";
            MontrePartiesPGN.Click += MontrePartiesPGN_Click;
            // 
            // ChargerPartiesPgn
            // 
            ChargerPartiesPgn.FileName = "fichierPgn";
            ChargerPartiesPgn.Filter = "Fichier PGN (*.pgn)|*.pgn";
            // 
            // groupParcoursPartie
            // 
            groupParcoursPartie.BackColor = System.Drawing.Color.Gray;
            groupParcoursPartie.Controls.Add(BoutonFin);
            groupParcoursPartie.Controls.Add(BoutonDebut);
            groupParcoursPartie.Controls.Add(BoutonSuivant);
            groupParcoursPartie.Controls.Add(BoutonPrecedent);
            groupParcoursPartie.ForeColor = System.Drawing.SystemColors.ControlText;
            groupParcoursPartie.Location = new System.Drawing.Point(544, 313);
            groupParcoursPartie.Name = "groupParcoursPartie";
            groupParcoursPartie.Size = new System.Drawing.Size(128, 81);
            groupParcoursPartie.TabIndex = 37;
            groupParcoursPartie.TabStop = false;
            groupParcoursPartie.Text = "Parcours partie PGN";
            // 
            // BoutonFin
            // 
            BoutonFin.Location = new System.Drawing.Point(64, 49);
            BoutonFin.Name = "BoutonFin";
            BoutonFin.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonFin.Size = new System.Drawing.Size(58, 25);
            BoutonFin.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(128, 128, 255);
            BoutonFin.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonFin.StateCommon.Border.Rounding = 20F;
            BoutonFin.StateCommon.Border.Width = 3;
            BoutonFin.TabIndex = 3;
            BoutonFin.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonFin.Values.Text = "Fin";
            BoutonFin.Click += BoutonFin_Click;
            // 
            // BoutonDebut
            // 
            BoutonDebut.Location = new System.Drawing.Point(6, 49);
            BoutonDebut.Name = "BoutonDebut";
            BoutonDebut.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonDebut.Size = new System.Drawing.Size(58, 25);
            BoutonDebut.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(128, 128, 255);
            BoutonDebut.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonDebut.StateCommon.Border.Rounding = 20F;
            BoutonDebut.StateCommon.Border.Width = 3;
            BoutonDebut.TabIndex = 2;
            BoutonDebut.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonDebut.Values.Text = "Début";
            BoutonDebut.Click += BoutonDebut_Click;
            // 
            // BoutonSuivant
            // 
            BoutonSuivant.Location = new System.Drawing.Point(64, 22);
            BoutonSuivant.Name = "BoutonSuivant";
            BoutonSuivant.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonSuivant.Size = new System.Drawing.Size(56, 25);
            BoutonSuivant.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(128, 128, 255);
            BoutonSuivant.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonSuivant.StateCommon.Border.Rounding = 20F;
            BoutonSuivant.StateCommon.Border.Width = 3;
            BoutonSuivant.TabIndex = 1;
            BoutonSuivant.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonSuivant.Values.Text = "Suiv.";
            BoutonSuivant.Click += BoutonSuivant_Click;
            // 
            // BoutonPrecedent
            // 
            BoutonPrecedent.Location = new System.Drawing.Point(6, 22);
            BoutonPrecedent.Name = "BoutonPrecedent";
            BoutonPrecedent.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BoutonPrecedent.Size = new System.Drawing.Size(58, 25);
            BoutonPrecedent.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(128, 128, 255);
            BoutonPrecedent.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BoutonPrecedent.StateCommon.Border.Rounding = 20F;
            BoutonPrecedent.StateCommon.Border.Width = 3;
            BoutonPrecedent.TabIndex = 0;
            BoutonPrecedent.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BoutonPrecedent.Values.Text = "Préc.";
            BoutonPrecedent.Click += BoutonPrecedent_Click;
            // 
            // ActiveBibliothèque
            // 
            ActiveBibliothèque.BackColor = System.Drawing.Color.WhiteSmoke;
            ActiveBibliothèque.Checked = true;
            ActiveBibliothèque.CheckState = System.Windows.Forms.CheckState.Checked;
            ActiveBibliothèque.Location = new System.Drawing.Point(12, 710);
            ActiveBibliothèque.Name = "ActiveBibliothèque";
            ActiveBibliothèque.Size = new System.Drawing.Size(107, 24);
            ActiveBibliothèque.TabIndex = 38;
            ActiveBibliothèque.Text = "Bibliothèque";
            ActiveBibliothèque.UseVisualStyleBackColor = false;
            ActiveBibliothèque.CheckedChanged += ActiveBibliothèque_CheckedChanged;
            // 
            // ActiveSon
            // 
            ActiveSon.BackColor = System.Drawing.Color.WhiteSmoke;
            ActiveSon.Checked = true;
            ActiveSon.CheckState = System.Windows.Forms.CheckState.Checked;
            ActiveSon.Location = new System.Drawing.Point(125, 710);
            ActiveSon.Name = "ActiveSon";
            ActiveSon.Size = new System.Drawing.Size(49, 24);
            ActiveSon.TabIndex = 39;
            ActiveSon.Text = "Son";
            ActiveSon.UseVisualStyleBackColor = false;
            ActiveSon.CheckedChanged += ActiveSon_CheckedChanged;
            // 
            // groupBoxTempsReflexion
            // 
            groupBoxTempsReflexion.BackColor = System.Drawing.Color.WhiteSmoke;
            groupBoxTempsReflexion.Controls.Add(labelTempsReflexion);
            groupBoxTempsReflexion.Controls.Add(TrackBarTempsReflexion);
            groupBoxTempsReflexion.Location = new System.Drawing.Point(180, 710);
            groupBoxTempsReflexion.Name = "groupBoxTempsReflexion";
            groupBoxTempsReflexion.Size = new System.Drawing.Size(350, 46);
            groupBoxTempsReflexion.TabIndex = 40;
            groupBoxTempsReflexion.TabStop = false;
            groupBoxTempsReflexion.Text = "Temps de réflexion en secondes [1-600]";
            // 
            // labelTempsReflexion
            // 
            labelTempsReflexion.Font = new System.Drawing.Font("Gill Sans Ultra Bold", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelTempsReflexion.ForeColor = System.Drawing.Color.DarkGreen;
            labelTempsReflexion.Location = new System.Drawing.Point(280, 16);
            labelTempsReflexion.Name = "labelTempsReflexion";
            labelTempsReflexion.Size = new System.Drawing.Size(70, 23);
            labelTempsReflexion.TabIndex = 1;
            labelTempsReflexion.Text = "[5]";
            labelTempsReflexion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrackBarTempsReflexion
            // 
            TrackBarTempsReflexion.AutoSize = false;
            TrackBarTempsReflexion.BackColor = System.Drawing.Color.Gray;
            TrackBarTempsReflexion.Location = new System.Drawing.Point(6, 16);
            TrackBarTempsReflexion.Maximum = 600;
            TrackBarTempsReflexion.Name = "TrackBarTempsReflexion";
            TrackBarTempsReflexion.Size = new System.Drawing.Size(278, 25);
            TrackBarTempsReflexion.TabIndex = 0;
            TrackBarTempsReflexion.Value = 1;
            TrackBarTempsReflexion.ValueChanged += TrackBarTempsReflexion_ValueChanged;
            // 
            // KryptonApropos
            // 
            KryptonApropos.Location = new System.Drawing.Point(542, 697);
            KryptonApropos.Name = "KryptonApropos";
            KryptonApropos.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            KryptonApropos.Size = new System.Drawing.Size(130, 25);
            KryptonApropos.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            KryptonApropos.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            KryptonApropos.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            KryptonApropos.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            KryptonApropos.StateCommon.Border.Rounding = 20F;
            KryptonApropos.StateCommon.Border.Width = 3;
            KryptonApropos.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            KryptonApropos.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            KryptonApropos.TabIndex = 41;
            KryptonApropos.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            KryptonApropos.Values.Text = "A propos";
            KryptonApropos.Click += KryptonApropos_Click;
            // 
            // ActiveAléatoire
            // 
            ActiveAléatoire.BackColor = System.Drawing.Color.WhiteSmoke;
            ActiveAléatoire.Location = new System.Drawing.Point(10, 732);
            ActiveAléatoire.Name = "ActiveAléatoire";
            ActiveAléatoire.Size = new System.Drawing.Size(109, 24);
            ActiveAléatoire.TabIndex = 42;
            ActiveAléatoire.Text = "Aléatoire";
            ActiveAléatoire.UseVisualStyleBackColor = false;
            ActiveAléatoire.CheckedChanged += ActiveAléatoire_CheckedChanged;
            // 
            // SaisiePartieBouton
            // 
            SaisiePartieBouton.Location = new System.Drawing.Point(542, 31);
            SaisiePartieBouton.Name = "SaisiePartieBouton";
            SaisiePartieBouton.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            SaisiePartieBouton.Size = new System.Drawing.Size(130, 25);
            SaisiePartieBouton.StateCommon.Back.Color1 = System.Drawing.Color.FromArgb(192, 192, 255);
            SaisiePartieBouton.StateCommon.Border.Color1 = System.Drawing.Color.DarkGray;
            SaisiePartieBouton.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            SaisiePartieBouton.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            SaisiePartieBouton.StateCommon.Border.Rounding = 20F;
            SaisiePartieBouton.StateCommon.Border.Width = 3;
            SaisiePartieBouton.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            SaisiePartieBouton.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            SaisiePartieBouton.TabIndex = 43;
            SaisiePartieBouton.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            SaisiePartieBouton.Values.Text = "Saisie partie";
            SaisiePartieBouton.Click += SaisiePartieBouton_Click;
            // 
            // ChargerPositionFen
            // 
            ChargerPositionFen.FileName = "fichierFen";
            ChargerPositionFen.Filter = "Fichier FEN (*.fen)|*.fen";
            // 
            // EchiquierPrincipal
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            BackColor = System.Drawing.Color.LightGray;
            ClientSize = new System.Drawing.Size(684, 785);
            Controls.Add(SaisiePartieBouton);
            Controls.Add(ActiveAléatoire);
            Controls.Add(KryptonApropos);
            Controls.Add(groupBoxTempsReflexion);
            Controls.Add(ActiveSon);
            Controls.Add(ActiveBibliothèque);
            Controls.Add(groupParcoursPartie);
            Controls.Add(MontrePartiesPGN);
            Controls.Add(OrdinateurJoue);
            Controls.Add(CoupsBibliothèque);
            Controls.Add(CoupsBibliothèqueBox);
            Controls.Add(KryptonQuitter);
            Controls.Add(BoutonNulle);
            Controls.Add(BoutonGainNoir);
            Controls.Add(BoutonGainBlanc);
            Controls.Add(ListeCoupsBouton);
            Controls.Add(MontreVariantesUci);
            Controls.Add(InformationsPartie);
            Controls.Add(groupeJoueurs);
            Controls.Add(AnalysePosition);
            Controls.Add(VarianteMoteurUci3);
            Controls.Add(VarianteMoteurUci2);
            Controls.Add(VarianteMoteurUci1);
            Controls.Add(kryptonStatusStrip1);
            Controls.Add(VisualisationPgn);
            Controls.Add(RetourArriere);
            Controls.Add(BoutonBalises);
            Controls.Add(GroupPromo);
            Controls.Add(InverseEchiquier);
            Controls.Add(Plateau);
            Controls.Add(MontreDonneesUci);
            Controls.Add(InformationPourJoueur);
            Controls.Add(MenuInterfaceGraphique);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = MenuInterfaceGraphique;
            Margin = new System.Windows.Forms.Padding(2);
            Name = "EchiquierPrincipal";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Echiquier Principal";
            FormClosing += EchiquierPrincipal_FormClosing;
            Load += BrunoInterfaceGraphique_Load;
            MenuInterfaceGraphique.ResumeLayout(false);
            MenuInterfaceGraphique.PerformLayout();
            GroupPromo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Promo3).EndInit();
            ((System.ComponentModel.ISupportInitialize)Promo2).EndInit();
            ((System.ComponentModel.ISupportInitialize)Promo1).EndInit();
            ((System.ComponentModel.ISupportInitialize)Promo0).EndInit();
            kryptonStatusStrip1.ResumeLayout(false);
            kryptonStatusStrip1.PerformLayout();
            groupeJoueurs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)Plateau).EndInit();
            groupParcoursPartie.ResumeLayout(false);
            groupBoxTempsReflexion.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)TrackBarTempsReflexion).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label InformationPourJoueur;
        private Krypton.Toolkit.KryptonButton MontreDonneesUci;
        private System.Windows.Forms.PictureBox Plateau;
        private Krypton.Toolkit.KryptonButton InverseEchiquier;
        private System.Windows.Forms.MenuStrip MenuInterfaceGraphique;
        private System.Windows.Forms.ToolStripMenuItem FichierMenu;
        private System.Windows.Forms.ToolStripMenuItem Apropos;
        private System.Windows.Forms.ToolStripMenuItem Quitter;
        private System.Windows.Forms.ToolStripMenuItem EnregistrerPgn;
        private System.Windows.Forms.ToolStripMenuItem EnregistrerFen;
        private System.Windows.Forms.SaveFileDialog SauvegardeFen;
        private System.Windows.Forms.SaveFileDialog SauvegardeFichier;
        private System.Windows.Forms.ToolStripMenuItem OptionsMenu;
        private System.Windows.Forms.ToolStripMenuItem Personnaliser;
        private System.Windows.Forms.ToolStripMenuItem CaseSombre;
        private System.Windows.Forms.ToolStripMenuItem CaseClaire;
        private System.Windows.Forms.ColorDialog CouleurDialogue;
        private System.Windows.Forms.ToolStripMenuItem PartieMenu;
        private System.Windows.Forms.ToolStripMenuItem NouvellePartieStockfish;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.GroupBox GroupPromo;
        private System.Windows.Forms.PictureBox Promo2;
        private System.Windows.Forms.PictureBox Promo1;
        private System.Windows.Forms.PictureBox Promo0;
        private System.Windows.Forms.PictureBox Promo3;
        private Krypton.Toolkit.KryptonButton BoutonBalises;
        private System.Windows.Forms.ToolStripMenuItem OptionBalises;
        private Krypton.Toolkit.KryptonButton VisualisationPgn;
        private Krypton.Toolkit.KryptonStatusStrip kryptonStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusProgramme;
        private System.Windows.Forms.ToolStripStatusLabel ScoreMoteur;
        private System.Windows.Forms.ToolStripStatusLabel VarianteMoteurCourante;
        private System.Windows.Forms.RichTextBox VarianteMoteurUci1;
        private System.Windows.Forms.RichTextBox VarianteMoteurUci2;
        private System.Windows.Forms.RichTextBox VarianteMoteurUci3;
        private System.Windows.Forms.ToolStripStatusLabel EvaluationUci;
        private System.Windows.Forms.GroupBox groupeJoueurs;
        private System.Windows.Forms.Label LabelJoueurNoir;
        private System.Windows.Forms.Label LabelJoueurBlanc;
        private System.Windows.Forms.Label InformationsPartie;
        private Krypton.Toolkit.KryptonButton MontreVariantesUci;
        private System.Windows.Forms.ToolStripMenuItem ParametresAvances;
        public Krypton.Toolkit.KryptonButton ListeCoupsBouton;
        public Krypton.Toolkit.KryptonButton RetourArriere;
        public Krypton.Toolkit.KryptonButton AnalysePosition;
        private System.Windows.Forms.ToolStripMenuItem VisualiserPgn;
        private System.Windows.Forms.ToolStripMenuItem ParametresDeBase;
        private Krypton.Toolkit.KryptonButton BoutonGainBlanc;
        private Krypton.Toolkit.KryptonButton BoutonGainNoir;
        private Krypton.Toolkit.KryptonButton BoutonNulle;
        public System.Windows.Forms.Label EloNoir;
        public System.Windows.Forms.Label EloBlanc;
        private Krypton.Toolkit.KryptonButton KryptonQuitter;
        private System.Windows.Forms.RichTextBox CoupsBibliothèqueBox;
        private System.Windows.Forms.ToolStripMenuItem moteursBibliothèquesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SelectionAutreMoteur;
        private System.Windows.Forms.ToolStripMenuItem SelectionBibliothèque;
        private System.Windows.Forms.ToolStripMenuItem RodentIV;
        private System.Windows.Forms.ToolStripMenuItem Sargon1_1978;
        private System.Windows.Forms.OpenFileDialog OuvertureChoixBibliothèque;
        private System.Windows.Forms.OpenFileDialog OuvertureChoixMoteur;
        private System.Windows.Forms.RichTextBox CoupsBibliothèque;
        private System.Windows.Forms.ToolStripMenuItem nouvellePartieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HumainOrdinateur;
        private System.Windows.Forms.ToolStripMenuItem OrdinateurHumain;
        private System.Windows.Forms.ToolStripMenuItem HumainContreHumain;
        public Krypton.Toolkit.KryptonButton OrdinateurJoue;
        public Krypton.Toolkit.KryptonButton MontrePartiesPGN;
        private System.Windows.Forms.ToolStripMenuItem ChargePartiesPgn;
        private System.Windows.Forms.OpenFileDialog ChargerPartiesPgn;
        private Krypton.Toolkit.KryptonButton BoutonPrecedent;
        private Krypton.Toolkit.KryptonButton BoutonSuivant;
        private Krypton.Toolkit.KryptonButton BoutonFin;
        private Krypton.Toolkit.KryptonButton BoutonDebut;
        private System.Windows.Forms.CheckBox ActiveBibliothèque;
        private System.Windows.Forms.CheckBox ActiveSon;
        private System.Windows.Forms.GroupBox groupBoxTempsReflexion;
        private System.Windows.Forms.Label labelTempsReflexion;
        private Krypton.Toolkit.KryptonButton KryptonApropos;
        private System.Windows.Forms.CheckBox ActiveAléatoire;
        public System.Windows.Forms.TrackBar TrackBarTempsReflexion;
        public Krypton.Toolkit.KryptonButton SaisiePartieBouton;
        private System.Windows.Forms.ToolStripMenuItem AideDocumentation;
        public System.Windows.Forms.GroupBox groupParcoursPartie;
        private System.Windows.Forms.ToolStripMenuItem StopMoteur;
        private System.Windows.Forms.ToolStripMenuItem BtnMiseAJour;
        private System.Windows.Forms.OpenFileDialog ChargerPositionFen;
        private System.Windows.Forms.ToolStripMenuItem ChargePositionFen;
    }
}

