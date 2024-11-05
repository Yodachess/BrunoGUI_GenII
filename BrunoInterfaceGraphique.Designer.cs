namespace BrunoGUI_Stockfish
{
    partial class BrunoInterfaceGraphique
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrunoInterfaceGraphique));
            this.InformationPourJoueur = new System.Windows.Forms.Label();
            this.MontreDonneesUci = new Krypton.Toolkit.KryptonButton();
            this.InverseEchiquier = new Krypton.Toolkit.KryptonButton();
            this.MenuInterfaceGraphique = new System.Windows.Forms.MenuStrip();
            this.FichierMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EnregistrerPgn = new System.Windows.Forms.ToolStripMenuItem();
            this.EnregistrerFen = new System.Windows.Forms.ToolStripMenuItem();
            this.Quitter = new System.Windows.Forms.ToolStripMenuItem();
            this.PartieMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.NouvellePartie = new System.Windows.Forms.ToolStripMenuItem();
            this.ParametresDeBase = new System.Windows.Forms.ToolStripMenuItem();
            this.ParametresAvances = new System.Windows.Forms.ToolStripMenuItem();
            this.HumainContreHumain = new System.Windows.Forms.ToolStripMenuItem();
            this.StopMoteur = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.Personnaliser = new System.Windows.Forms.ToolStripMenuItem();
            this.CaseSombre = new System.Windows.Forms.ToolStripMenuItem();
            this.CaseClaire = new System.Windows.Forms.ToolStripMenuItem();
            this.visualiserPgn = new System.Windows.Forms.ToolStripMenuItem();
            this.OptionBalises = new System.Windows.Forms.ToolStripMenuItem();
            this.PointArret = new System.Windows.Forms.ToolStripMenuItem();
            this.Apropos = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.SauvegardeFen = new System.Windows.Forms.SaveFileDialog();
            this.SauvegardeFichier = new System.Windows.Forms.SaveFileDialog();
            this.CouleurDialogue = new System.Windows.Forms.ColorDialog();
            this.GroupPromo = new System.Windows.Forms.GroupBox();
            this.Promo3 = new System.Windows.Forms.PictureBox();
            this.Promo2 = new System.Windows.Forms.PictureBox();
            this.Promo1 = new System.Windows.Forms.PictureBox();
            this.Promo0 = new System.Windows.Forms.PictureBox();
            this.BoutonBalises = new Krypton.Toolkit.KryptonButton();
            this.RetourArriere = new Krypton.Toolkit.KryptonButton();
            this.VisualisationPgn = new Krypton.Toolkit.KryptonButton();
            this.kryptonStatusStrip1 = new Krypton.Toolkit.KryptonStatusStrip();
            this.StatusProgramme = new System.Windows.Forms.ToolStripStatusLabel();
            this.EvaluationUci = new System.Windows.Forms.ToolStripStatusLabel();
            this.ScoreMoteur = new System.Windows.Forms.ToolStripStatusLabel();
            this.VarianteMoteurCourante = new System.Windows.Forms.ToolStripStatusLabel();
            this.VarianteMoteurUci1 = new System.Windows.Forms.RichTextBox();
            this.VarianteMoteurUci2 = new System.Windows.Forms.RichTextBox();
            this.VarianteMoteurUci3 = new System.Windows.Forms.RichTextBox();
            this.AnalysePosition = new Krypton.Toolkit.KryptonButton();
            this.groupeJoueurs = new System.Windows.Forms.GroupBox();
            this.LabelJoueurNoir = new System.Windows.Forms.Label();
            this.EloNoir = new System.Windows.Forms.Label();
            this.LabelJoueurBlanc = new System.Windows.Forms.Label();
            this.EloBlanc = new System.Windows.Forms.Label();
            this.InformationsPartie = new System.Windows.Forms.Label();
            this.MontreVariantesUci = new Krypton.Toolkit.KryptonButton();
            this.Decompteur = new System.Windows.Forms.Label();
            this.ChronometreTempsFixe = new System.Windows.Forms.Timer(this.components);
            this.ListeCoupsBouton = new Krypton.Toolkit.KryptonButton();
            this.Plateau = new System.Windows.Forms.PictureBox();
            this.BoutonGainBlanc = new Krypton.Toolkit.KryptonButton();
            this.BoutonGainNoir = new Krypton.Toolkit.KryptonButton();
            this.BoutonNulle = new Krypton.Toolkit.KryptonButton();
            this.MenuInterfaceGraphique.SuspendLayout();
            this.GroupPromo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Promo3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Promo2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Promo1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Promo0)).BeginInit();
            this.kryptonStatusStrip1.SuspendLayout();
            this.groupeJoueurs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Plateau)).BeginInit();
            this.SuspendLayout();
            // 
            // InformationPourJoueur
            // 
            this.InformationPourJoueur.BackColor = System.Drawing.Color.LightSteelBlue;
            this.InformationPourJoueur.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.InformationPourJoueur.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InformationPourJoueur.ForeColor = System.Drawing.Color.DarkBlue;
            this.InformationPourJoueur.Location = new System.Drawing.Point(12, 68);
            this.InformationPourJoueur.Name = "InformationPourJoueur";
            this.InformationPourJoueur.Size = new System.Drawing.Size(200, 19);
            this.InformationPourJoueur.TabIndex = 1;
            this.InformationPourJoueur.Text = "Information Pour Joueur";
            this.InformationPourJoueur.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MontreDonneesUci
            // 
            this.MontreDonneesUci.Location = new System.Drawing.Point(328, 715);
            this.MontreDonneesUci.Name = "MontreDonneesUci";
            this.MontreDonneesUci.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.MontreDonneesUci.Size = new System.Drawing.Size(120, 20);
            this.MontreDonneesUci.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.MontreDonneesUci.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.MontreDonneesUci.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            this.MontreDonneesUci.StateCommon.Border.Rounding = 20F;
            this.MontreDonneesUci.StateCommon.Border.Width = 1;
            this.MontreDonneesUci.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.MontreDonneesUci.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            this.MontreDonneesUci.TabIndex = 2;
            this.MontreDonneesUci.Values.Text = "Affiche protocole";
            this.MontreDonneesUci.Click += new System.EventHandler(this.MontreDonneesUci_Click);
            // 
            // InverseEchiquier
            // 
            this.InverseEchiquier.Location = new System.Drawing.Point(119, 689);
            this.InverseEchiquier.Name = "InverseEchiquier";
            this.InverseEchiquier.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.InverseEchiquier.Size = new System.Drawing.Size(112, 20);
            this.InverseEchiquier.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.InverseEchiquier.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.InverseEchiquier.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            this.InverseEchiquier.StateCommon.Border.Rounding = 20F;
            this.InverseEchiquier.StateCommon.Border.Width = 1;
            this.InverseEchiquier.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.InverseEchiquier.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            this.InverseEchiquier.TabIndex = 4;
            this.InverseEchiquier.Values.Text = "Tourne Echiquier";
            this.InverseEchiquier.Click += new System.EventHandler(this.InverseEchiquier_Click);
            // 
            // MenuInterfaceGraphique
            // 
            this.MenuInterfaceGraphique.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuInterfaceGraphique.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FichierMenu,
            this.PartieMenu,
            this.OptionsMenu,
            this.Apropos,
            this.toolStripMenuItem2});
            this.MenuInterfaceGraphique.Location = new System.Drawing.Point(0, 0);
            this.MenuInterfaceGraphique.Name = "MenuInterfaceGraphique";
            this.MenuInterfaceGraphique.Size = new System.Drawing.Size(540, 24);
            this.MenuInterfaceGraphique.TabIndex = 6;
            this.MenuInterfaceGraphique.Text = "Menu Interface Graphique";
            // 
            // FichierMenu
            // 
            this.FichierMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EnregistrerPgn,
            this.EnregistrerFen,
            this.Quitter});
            this.FichierMenu.Name = "FichierMenu";
            this.FichierMenu.Size = new System.Drawing.Size(54, 20);
            this.FichierMenu.Text = "Fichier";
            // 
            // EnregistrerPgn
            // 
            this.EnregistrerPgn.Image = ((System.Drawing.Image)(resources.GetObject("EnregistrerPgn.Image")));
            this.EnregistrerPgn.Name = "EnregistrerPgn";
            this.EnregistrerPgn.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.EnregistrerPgn.Size = new System.Drawing.Size(203, 22);
            this.EnregistrerPgn.Text = "Enregistrer Partie";
            this.EnregistrerPgn.Click += new System.EventHandler(this.EnregistrerPgn_Click);
            // 
            // EnregistrerFen
            // 
            this.EnregistrerFen.Image = ((System.Drawing.Image)(resources.GetObject("EnregistrerFen.Image")));
            this.EnregistrerFen.Name = "EnregistrerFen";
            this.EnregistrerFen.Size = new System.Drawing.Size(203, 22);
            this.EnregistrerFen.Text = "Enregistrer Position";
            this.EnregistrerFen.Click += new System.EventHandler(this.EnregistrerFen_Click);
            // 
            // Quitter
            // 
            this.Quitter.Image = ((System.Drawing.Image)(resources.GetObject("Quitter.Image")));
            this.Quitter.Name = "Quitter";
            this.Quitter.Size = new System.Drawing.Size(203, 22);
            this.Quitter.Text = "Quitter";
            this.Quitter.Click += new System.EventHandler(this.Quitter_Click);
            // 
            // PartieMenu
            // 
            this.PartieMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NouvellePartie,
            this.ParametresDeBase,
            this.ParametresAvances,
            this.HumainContreHumain,
            this.StopMoteur});
            this.PartieMenu.Name = "PartieMenu";
            this.PartieMenu.Size = new System.Drawing.Size(49, 20);
            this.PartieMenu.Text = "Partie";
            // 
            // NouvellePartie
            // 
            this.NouvellePartie.Image = ((System.Drawing.Image)(resources.GetObject("NouvellePartie.Image")));
            this.NouvellePartie.Name = "NouvellePartie";
            this.NouvellePartie.Size = new System.Drawing.Size(180, 22);
            this.NouvellePartie.Text = "Contre Stockfish";
            this.NouvellePartie.Click += new System.EventHandler(this.NouvellePartie_Click);
            // 
            // ParametresDeBase
            // 
            this.ParametresDeBase.Image = ((System.Drawing.Image)(resources.GetObject("ParametresDeBase.Image")));
            this.ParametresDeBase.Name = "ParametresDeBase";
            this.ParametresDeBase.Size = new System.Drawing.Size(180, 22);
            this.ParametresDeBase.Text = "Paramètres de base";
            this.ParametresDeBase.Click += new System.EventHandler(this.ParametresDeBase_Click);
            // 
            // ParametresAvances
            // 
            this.ParametresAvances.Image = ((System.Drawing.Image)(resources.GetObject("ParametresAvances.Image")));
            this.ParametresAvances.Name = "ParametresAvances";
            this.ParametresAvances.Size = new System.Drawing.Size(180, 22);
            this.ParametresAvances.Text = "Paramêtres Avancés";
            this.ParametresAvances.Click += new System.EventHandler(this.ParametresAvances_Click);
            // 
            // HumainContreHumain
            // 
            this.HumainContreHumain.Image = ((System.Drawing.Image)(resources.GetObject("HumainContreHumain.Image")));
            this.HumainContreHumain.Name = "HumainContreHumain";
            this.HumainContreHumain.Size = new System.Drawing.Size(180, 22);
            this.HumainContreHumain.Text = "Entre Humains";
            this.HumainContreHumain.Click += new System.EventHandler(this.HumainContreHumain_Click);
            // 
            // StopMoteur
            // 
            this.StopMoteur.Image = ((System.Drawing.Image)(resources.GetObject("StopMoteur.Image")));
            this.StopMoteur.Name = "StopMoteur";
            this.StopMoteur.Size = new System.Drawing.Size(180, 22);
            this.StopMoteur.Text = "Stoppe le Moteur";
            this.StopMoteur.Click += new System.EventHandler(this.StopMoteur_Click);
            // 
            // OptionsMenu
            // 
            this.OptionsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Personnaliser,
            this.visualiserPgn,
            this.OptionBalises,
            this.PointArret});
            this.OptionsMenu.Name = "OptionsMenu";
            this.OptionsMenu.Size = new System.Drawing.Size(61, 20);
            this.OptionsMenu.Text = "Options";
            // 
            // Personnaliser
            // 
            this.Personnaliser.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CaseSombre,
            this.CaseClaire});
            this.Personnaliser.Image = ((System.Drawing.Image)(resources.GetObject("Personnaliser.Image")));
            this.Personnaliser.Name = "Personnaliser";
            this.Personnaliser.Size = new System.Drawing.Size(176, 22);
            this.Personnaliser.Text = "Personnaliser";
            // 
            // CaseSombre
            // 
            this.CaseSombre.Image = ((System.Drawing.Image)(resources.GetObject("CaseSombre.Image")));
            this.CaseSombre.Name = "CaseSombre";
            this.CaseSombre.Size = new System.Drawing.Size(143, 22);
            this.CaseSombre.Text = "Case Sombre";
            this.CaseSombre.Click += new System.EventHandler(this.CaseSombre_Click);
            // 
            // CaseClaire
            // 
            this.CaseClaire.Image = ((System.Drawing.Image)(resources.GetObject("CaseClaire.Image")));
            this.CaseClaire.Name = "CaseClaire";
            this.CaseClaire.Size = new System.Drawing.Size(143, 22);
            this.CaseClaire.Text = "Case Claire";
            this.CaseClaire.Click += new System.EventHandler(this.CaseClaire_Click);
            // 
            // visualiserPgn
            // 
            this.visualiserPgn.Image = ((System.Drawing.Image)(resources.GetObject("visualiserPgn.Image")));
            this.visualiserPgn.Name = "visualiserPgn";
            this.visualiserPgn.Size = new System.Drawing.Size(176, 22);
            this.visualiserPgn.Text = "Partie PGN";
            this.visualiserPgn.Click += new System.EventHandler(this.visualiserPgn_Click);
            // 
            // OptionBalises
            // 
            this.OptionBalises.Image = ((System.Drawing.Image)(resources.GetObject("OptionBalises.Image")));
            this.OptionBalises.Name = "OptionBalises";
            this.OptionBalises.Size = new System.Drawing.Size(176, 22);
            this.OptionBalises.Text = "Saisie En-têtes PGN";
            this.OptionBalises.Click += new System.EventHandler(this.BoutonBalises_Click);
            // 
            // PointArret
            // 
            this.PointArret.Image = ((System.Drawing.Image)(resources.GetObject("PointArret.Image")));
            this.PointArret.Name = "PointArret";
            this.PointArret.Size = new System.Drawing.Size(176, 22);
            this.PointArret.Text = "Point d\'arrêt";
            this.PointArret.Click += new System.EventHandler(this.PointArret_Click);
            // 
            // Apropos
            // 
            this.Apropos.Name = "Apropos";
            this.Apropos.Size = new System.Drawing.Size(95, 20);
            this.Apropos.Text = "A propos de ...";
            this.Apropos.Click += new System.EventHandler(this.Apropos_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(22, 20);
            this.toolStripMenuItem2.Text = " ";
            // 
            // SauvegardeFen
            // 
            this.SauvegardeFen.Filter = "Fichier FEN (*.fen)|*.fen";
            // 
            // SauvegardeFichier
            // 
            this.SauvegardeFichier.Filter = "Fichier PGN (*.pgn)|*.pgn";
            // 
            // GroupPromo
            // 
            this.GroupPromo.BackColor = System.Drawing.Color.LightSteelBlue;
            this.GroupPromo.Controls.Add(this.Promo3);
            this.GroupPromo.Controls.Add(this.Promo2);
            this.GroupPromo.Controls.Add(this.Promo1);
            this.GroupPromo.Controls.Add(this.Promo0);
            this.GroupPromo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GroupPromo.Location = new System.Drawing.Point(87, 320);
            this.GroupPromo.Name = "GroupPromo";
            this.GroupPromo.Size = new System.Drawing.Size(354, 111);
            this.GroupPromo.TabIndex = 7;
            this.GroupPromo.TabStop = false;
            this.GroupPromo.Text = "Pièces pour promotion   (Sélectionnez la pièce promue)";
            this.GroupPromo.Visible = false;
            // 
            // Promo3
            // 
            this.Promo3.BackColor = System.Drawing.Color.White;
            this.Promo3.Location = new System.Drawing.Point(265, 19);
            this.Promo3.Name = "Promo3";
            this.Promo3.Size = new System.Drawing.Size(80, 80);
            this.Promo3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Promo3.TabIndex = 4;
            this.Promo3.TabStop = false;
            this.Promo3.Click += new System.EventHandler(this.Promo0_Click);
            // 
            // Promo2
            // 
            this.Promo2.BackColor = System.Drawing.Color.White;
            this.Promo2.Location = new System.Drawing.Point(179, 20);
            this.Promo2.Name = "Promo2";
            this.Promo2.Size = new System.Drawing.Size(80, 80);
            this.Promo2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Promo2.TabIndex = 3;
            this.Promo2.TabStop = false;
            this.Promo2.Click += new System.EventHandler(this.Promo0_Click);
            // 
            // Promo1
            // 
            this.Promo1.BackColor = System.Drawing.Color.White;
            this.Promo1.Location = new System.Drawing.Point(93, 19);
            this.Promo1.Name = "Promo1";
            this.Promo1.Size = new System.Drawing.Size(80, 80);
            this.Promo1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Promo1.TabIndex = 2;
            this.Promo1.TabStop = false;
            this.Promo1.Click += new System.EventHandler(this.Promo0_Click);
            // 
            // Promo0
            // 
            this.Promo0.BackColor = System.Drawing.Color.White;
            this.Promo0.Location = new System.Drawing.Point(7, 20);
            this.Promo0.Name = "Promo0";
            this.Promo0.Size = new System.Drawing.Size(80, 80);
            this.Promo0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Promo0.TabIndex = 1;
            this.Promo0.TabStop = false;
            this.Promo0.Click += new System.EventHandler(this.Promo0_Click);
            // 
            // BoutonBalises
            // 
            this.BoutonBalises.Location = new System.Drawing.Point(237, 715);
            this.BoutonBalises.Name = "BoutonBalises";
            this.BoutonBalises.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.BoutonBalises.Size = new System.Drawing.Size(85, 20);
            this.BoutonBalises.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.BoutonBalises.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.BoutonBalises.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            this.BoutonBalises.StateCommon.Border.Rounding = 20F;
            this.BoutonBalises.StateCommon.Border.Width = 1;
            this.BoutonBalises.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.BoutonBalises.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            this.BoutonBalises.TabIndex = 8;
            this.BoutonBalises.Values.Text = "Entête PGN";
            this.BoutonBalises.Click += new System.EventHandler(this.BoutonBalises_Click);
            // 
            // RetourArriere
            // 
            this.RetourArriere.Location = new System.Drawing.Point(119, 715);
            this.RetourArriere.Name = "RetourArriere";
            this.RetourArriere.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.RetourArriere.Size = new System.Drawing.Size(112, 20);
            this.RetourArriere.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.RetourArriere.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.RetourArriere.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            this.RetourArriere.StateCommon.Border.Rounding = 20F;
            this.RetourArriere.StateCommon.Border.Width = 1;
            this.RetourArriere.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.RetourArriere.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            this.RetourArriere.TabIndex = 9;
            this.RetourArriere.Values.Text = "Retour Arrière";
            this.RetourArriere.Click += new System.EventHandler(this.RetourArriere_Click);
            // 
            // VisualisationPgn
            // 
            this.VisualisationPgn.Location = new System.Drawing.Point(237, 689);
            this.VisualisationPgn.Name = "VisualisationPgn";
            this.VisualisationPgn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.VisualisationPgn.Size = new System.Drawing.Size(85, 20);
            this.VisualisationPgn.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.VisualisationPgn.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.VisualisationPgn.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            this.VisualisationPgn.StateCommon.Border.Rounding = 20F;
            this.VisualisationPgn.StateCommon.Border.Width = 1;
            this.VisualisationPgn.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.VisualisationPgn.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            this.VisualisationPgn.TabIndex = 10;
            this.VisualisationPgn.Values.Text = "Partie PGN";
            this.VisualisationPgn.Click += new System.EventHandler(this.VisualisationPgn_Click);
            // 
            // kryptonStatusStrip1
            // 
            this.kryptonStatusStrip1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.kryptonStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusProgramme,
            this.EvaluationUci,
            this.ScoreMoteur,
            this.VarianteMoteurCourante});
            this.kryptonStatusStrip1.Location = new System.Drawing.Point(0, 745);
            this.kryptonStatusStrip1.Name = "kryptonStatusStrip1";
            this.kryptonStatusStrip1.ProgressBars = null;
            this.kryptonStatusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.kryptonStatusStrip1.Size = new System.Drawing.Size(540, 26);
            this.kryptonStatusStrip1.TabIndex = 11;
            this.kryptonStatusStrip1.Text = "kryptonStatusStrip1";
            // 
            // StatusProgramme
            // 
            this.StatusProgramme.AutoSize = false;
            this.StatusProgramme.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.StatusProgramme.Name = "StatusProgramme";
            this.StatusProgramme.Size = new System.Drawing.Size(130, 21);
            this.StatusProgramme.Text = "Status du Programme";
            // 
            // EvaluationUci
            // 
            this.EvaluationUci.AutoSize = false;
            this.EvaluationUci.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EvaluationUci.Name = "EvaluationUci";
            this.EvaluationUci.Size = new System.Drawing.Size(100, 21);
            this.EvaluationUci.Text = "Evaluation";
            // 
            // ScoreMoteur
            // 
            this.ScoreMoteur.AutoSize = false;
            this.ScoreMoteur.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.ScoreMoteur.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ScoreMoteur.Name = "ScoreMoteur";
            this.ScoreMoteur.Size = new System.Drawing.Size(90, 21);
            this.ScoreMoteur.Text = "Score Moteur";
            // 
            // VarianteMoteurCourante
            // 
            this.VarianteMoteurCourante.AutoSize = false;
            this.VarianteMoteurCourante.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.VarianteMoteurCourante.Name = "VarianteMoteurCourante";
            this.VarianteMoteurCourante.Size = new System.Drawing.Size(205, 21);
            this.VarianteMoteurCourante.Text = "Variante en cours d\'examen";
            // 
            // VarianteMoteurUci1
            // 
            this.VarianteMoteurUci1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.VarianteMoteurUci1.ForeColor = System.Drawing.Color.DarkBlue;
            this.VarianteMoteurUci1.Location = new System.Drawing.Point(10, 620);
            this.VarianteMoteurUci1.Multiline = false;
            this.VarianteMoteurUci1.Name = "VarianteMoteurUci1";
            this.VarianteMoteurUci1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.VarianteMoteurUci1.Size = new System.Drawing.Size(520, 20);
            this.VarianteMoteurUci1.TabIndex = 13;
            this.VarianteMoteurUci1.Text = "Affichage Variante UCI";
            // 
            // VarianteMoteurUci2
            // 
            this.VarianteMoteurUci2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.VarianteMoteurUci2.ForeColor = System.Drawing.Color.DarkBlue;
            this.VarianteMoteurUci2.Location = new System.Drawing.Point(10, 640);
            this.VarianteMoteurUci2.Multiline = false;
            this.VarianteMoteurUci2.Name = "VarianteMoteurUci2";
            this.VarianteMoteurUci2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.VarianteMoteurUci2.Size = new System.Drawing.Size(520, 20);
            this.VarianteMoteurUci2.TabIndex = 21;
            this.VarianteMoteurUci2.Text = "Affichage Variante UCI";
            // 
            // VarianteMoteurUci3
            // 
            this.VarianteMoteurUci3.BackColor = System.Drawing.Color.LightSteelBlue;
            this.VarianteMoteurUci3.ForeColor = System.Drawing.Color.DarkBlue;
            this.VarianteMoteurUci3.Location = new System.Drawing.Point(10, 660);
            this.VarianteMoteurUci3.Multiline = false;
            this.VarianteMoteurUci3.Name = "VarianteMoteurUci3";
            this.VarianteMoteurUci3.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.VarianteMoteurUci3.Size = new System.Drawing.Size(520, 20);
            this.VarianteMoteurUci3.TabIndex = 22;
            this.VarianteMoteurUci3.Text = "Affichage Variante UCI";
            // 
            // AnalysePosition
            // 
            this.AnalysePosition.Location = new System.Drawing.Point(6, 689);
            this.AnalysePosition.Name = "AnalysePosition";
            this.AnalysePosition.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.AnalysePosition.Size = new System.Drawing.Size(110, 20);
            this.AnalysePosition.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.AnalysePosition.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.AnalysePosition.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            this.AnalysePosition.StateCommon.Border.Rounding = 20F;
            this.AnalysePosition.StateCommon.Border.Width = 1;
            this.AnalysePosition.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.AnalysePosition.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            this.AnalysePosition.TabIndex = 23;
            this.AnalysePosition.Values.Text = "Analyse Position";
            this.AnalysePosition.Click += new System.EventHandler(this.AnalysePosition_Click);
            // 
            // groupeJoueurs
            // 
            this.groupeJoueurs.BackColor = System.Drawing.Color.LightGray;
            this.groupeJoueurs.Controls.Add(this.LabelJoueurNoir);
            this.groupeJoueurs.Controls.Add(this.EloNoir);
            this.groupeJoueurs.Controls.Add(this.LabelJoueurBlanc);
            this.groupeJoueurs.Controls.Add(this.EloBlanc);
            this.groupeJoueurs.Location = new System.Drawing.Point(10, 28);
            this.groupeJoueurs.Name = "groupeJoueurs";
            this.groupeJoueurs.Size = new System.Drawing.Size(518, 37);
            this.groupeJoueurs.TabIndex = 24;
            this.groupeJoueurs.TabStop = false;
            this.groupeJoueurs.Text = "Joueurs";
            // 
            // LabelJoueurNoir
            // 
            this.LabelJoueurNoir.BackColor = System.Drawing.Color.Black;
            this.LabelJoueurNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelJoueurNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelJoueurNoir.ForeColor = System.Drawing.Color.White;
            this.LabelJoueurNoir.Location = new System.Drawing.Point(262, 14);
            this.LabelJoueurNoir.Name = "LabelJoueurNoir";
            this.LabelJoueurNoir.Size = new System.Drawing.Size(160, 20);
            this.LabelJoueurNoir.TabIndex = 3;
            this.LabelJoueurNoir.Text = "Joueur Noir";
            this.LabelJoueurNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EloNoir
            // 
            this.EloNoir.BackColor = System.Drawing.Color.Black;
            this.EloNoir.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.EloNoir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EloNoir.ForeColor = System.Drawing.Color.White;
            this.EloNoir.Location = new System.Drawing.Point(428, 14);
            this.EloNoir.Name = "EloNoir";
            this.EloNoir.Size = new System.Drawing.Size(90, 20);
            this.EloNoir.TabIndex = 2;
            this.EloNoir.Text = "Elo Noir";
            this.EloNoir.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LabelJoueurBlanc
            // 
            this.LabelJoueurBlanc.BackColor = System.Drawing.Color.White;
            this.LabelJoueurBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.LabelJoueurBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelJoueurBlanc.Location = new System.Drawing.Point(2, 14);
            this.LabelJoueurBlanc.Name = "LabelJoueurBlanc";
            this.LabelJoueurBlanc.Size = new System.Drawing.Size(160, 20);
            this.LabelJoueurBlanc.TabIndex = 1;
            this.LabelJoueurBlanc.Text = "Joueur Blanc";
            this.LabelJoueurBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EloBlanc
            // 
            this.EloBlanc.BackColor = System.Drawing.Color.White;
            this.EloBlanc.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.EloBlanc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EloBlanc.Location = new System.Drawing.Point(166, 14);
            this.EloBlanc.Name = "EloBlanc";
            this.EloBlanc.Size = new System.Drawing.Size(90, 20);
            this.EloBlanc.TabIndex = 0;
            this.EloBlanc.Text = "Elo Blanc";
            this.EloBlanc.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InformationsPartie
            // 
            this.InformationsPartie.BackColor = System.Drawing.Color.Lavender;
            this.InformationsPartie.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.InformationsPartie.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InformationsPartie.ForeColor = System.Drawing.Color.DarkViolet;
            this.InformationsPartie.Location = new System.Drawing.Point(328, 68);
            this.InformationsPartie.Name = "InformationsPartie";
            this.InformationsPartie.Size = new System.Drawing.Size(200, 19);
            this.InformationsPartie.TabIndex = 25;
            this.InformationsPartie.Text = "Informations Partie";
            this.InformationsPartie.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MontreVariantesUci
            // 
            this.MontreVariantesUci.Location = new System.Drawing.Point(328, 689);
            this.MontreVariantesUci.Name = "MontreVariantesUci";
            this.MontreVariantesUci.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.MontreVariantesUci.Size = new System.Drawing.Size(120, 20);
            this.MontreVariantesUci.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.MontreVariantesUci.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.MontreVariantesUci.StateCommon.Border.GraphicsHint = Krypton.Toolkit.PaletteGraphicsHint.AntiAlias;
            this.MontreVariantesUci.StateCommon.Border.Rounding = 20F;
            this.MontreVariantesUci.StateCommon.Border.Width = 1;
            this.MontreVariantesUci.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.MontreVariantesUci.StateCommon.Content.ShortText.Color2 = System.Drawing.Color.Black;
            this.MontreVariantesUci.TabIndex = 26;
            this.MontreVariantesUci.Values.Text = "Masque variantes";
            this.MontreVariantesUci.Click += new System.EventHandler(this.MontreVariantesUci_Click);
            // 
            // Decompteur
            // 
            this.Decompteur.BackColor = System.Drawing.Color.DarkGray;
            this.Decompteur.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Decompteur.Font = new System.Drawing.Font("Gill Sans Ultra Bold", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Decompteur.ForeColor = System.Drawing.Color.Black;
            this.Decompteur.Location = new System.Drawing.Point(218, 67);
            this.Decompteur.Name = "Decompteur";
            this.Decompteur.Size = new System.Drawing.Size(104, 20);
            this.Decompteur.TabIndex = 27;
            this.Decompteur.Text = "Chrono";
            this.Decompteur.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // ChronometreTempsFixe
            // 
            this.ChronometreTempsFixe.Tick += new System.EventHandler(this.ChronometreTempsFixe_Tick);
            // 
            // ListeCoupsBouton
            // 
            this.ListeCoupsBouton.Location = new System.Drawing.Point(6, 715);
            this.ListeCoupsBouton.Name = "ListeCoupsBouton";
            this.ListeCoupsBouton.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.ListeCoupsBouton.Size = new System.Drawing.Size(110, 20);
            this.ListeCoupsBouton.StateCommon.Border.Color1 = System.Drawing.Color.DarkBlue;
            this.ListeCoupsBouton.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ListeCoupsBouton.StateCommon.Border.Rounding = 20F;
            this.ListeCoupsBouton.StateCommon.Border.Width = 1;
            this.ListeCoupsBouton.StateCommon.Content.ShortText.Color1 = System.Drawing.Color.Black;
            this.ListeCoupsBouton.StateNormal.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ListeCoupsBouton.StateNormal.Border.Rounding = 20F;
            this.ListeCoupsBouton.StateNormal.Border.Width = 1;
            this.ListeCoupsBouton.TabIndex = 28;
            this.ListeCoupsBouton.Values.Text = "Liste des coups";
            this.ListeCoupsBouton.Click += new System.EventHandler(this.ListeCoupsBouton_Click);
            // 
            // Plateau
            // 
            this.Plateau.ErrorImage = global::BrunoGUI_Stockfish.Properties.Resources.Plateau_gris_GillSanOK;
            this.Plateau.Image = global::BrunoGUI_Stockfish.Properties.Resources.Plateau_gris_GillSanOK;
            this.Plateau.InitialImage = ((System.Drawing.Image)(resources.GetObject("Plateau.InitialImage")));
            this.Plateau.Location = new System.Drawing.Point(10, 90);
            this.Plateau.Name = "Plateau";
            this.Plateau.Size = new System.Drawing.Size(520, 520);
            this.Plateau.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.Plateau.TabIndex = 3;
            this.Plateau.TabStop = false;
            // 
            // BoutonGainBlanc
            // 
            this.BoutonGainBlanc.Location = new System.Drawing.Point(452, 686);
            this.BoutonGainBlanc.Name = "BoutonGainBlanc";
            this.BoutonGainBlanc.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.BoutonGainBlanc.Size = new System.Drawing.Size(78, 15);
            this.BoutonGainBlanc.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.BoutonGainBlanc.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.BoutonGainBlanc.StateCommon.Border.Rounding = 20F;
            this.BoutonGainBlanc.StateCommon.Border.Width = 1;
            this.BoutonGainBlanc.TabIndex = 29;
            this.BoutonGainBlanc.Values.Text = "Gain blanc";
            this.BoutonGainBlanc.Click += new System.EventHandler(this.BoutonGainBlanc_Click);
            // 
            // BoutonGainNoir
            // 
            this.BoutonGainNoir.Location = new System.Drawing.Point(452, 706);
            this.BoutonGainNoir.Name = "BoutonGainNoir";
            this.BoutonGainNoir.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.BoutonGainNoir.Size = new System.Drawing.Size(78, 15);
            this.BoutonGainNoir.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.BoutonGainNoir.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.BoutonGainNoir.StateCommon.Border.Rounding = 20F;
            this.BoutonGainNoir.StateCommon.Border.Width = 1;
            this.BoutonGainNoir.TabIndex = 30;
            this.BoutonGainNoir.Values.Text = "Gain noir";
            this.BoutonGainNoir.Click += new System.EventHandler(this.BoutonGainNoir_Click);
            // 
            // BoutonNulle
            // 
            this.BoutonNulle.Location = new System.Drawing.Point(452, 726);
            this.BoutonNulle.Name = "BoutonNulle";
            this.BoutonNulle.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.BoutonNulle.Size = new System.Drawing.Size(78, 15);
            this.BoutonNulle.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.BoutonNulle.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.BoutonNulle.StateCommon.Border.Rounding = 20F;
            this.BoutonNulle.StateCommon.Border.Width = 1;
            this.BoutonNulle.TabIndex = 31;
            this.BoutonNulle.Values.Text = "Nulle";
            this.BoutonNulle.Click += new System.EventHandler(this.BoutonNulle_Click);
            // 
            // BrunoInterfaceGraphique
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(540, 771);
            this.Controls.Add(this.BoutonNulle);
            this.Controls.Add(this.BoutonGainNoir);
            this.Controls.Add(this.BoutonGainBlanc);
            this.Controls.Add(this.ListeCoupsBouton);
            this.Controls.Add(this.Decompteur);
            this.Controls.Add(this.MontreVariantesUci);
            this.Controls.Add(this.InformationsPartie);
            this.Controls.Add(this.groupeJoueurs);
            this.Controls.Add(this.AnalysePosition);
            this.Controls.Add(this.VarianteMoteurUci3);
            this.Controls.Add(this.VarianteMoteurUci2);
            this.Controls.Add(this.VarianteMoteurUci1);
            this.Controls.Add(this.kryptonStatusStrip1);
            this.Controls.Add(this.VisualisationPgn);
            this.Controls.Add(this.RetourArriere);
            this.Controls.Add(this.BoutonBalises);
            this.Controls.Add(this.GroupPromo);
            this.Controls.Add(this.InverseEchiquier);
            this.Controls.Add(this.Plateau);
            this.Controls.Add(this.MontreDonneesUci);
            this.Controls.Add(this.InformationPourJoueur);
            this.Controls.Add(this.MenuInterfaceGraphique);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MenuInterfaceGraphique;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BrunoInterfaceGraphique";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bruno Interface Graphique";
            this.Load += new System.EventHandler(this.BrunoInterfaceGraphique_Load);
            this.MenuInterfaceGraphique.ResumeLayout(false);
            this.MenuInterfaceGraphique.PerformLayout();
            this.GroupPromo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Promo3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Promo2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Promo1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Promo0)).EndInit();
            this.kryptonStatusStrip1.ResumeLayout(false);
            this.kryptonStatusStrip1.PerformLayout();
            this.groupeJoueurs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Plateau)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.ToolStripMenuItem NouvellePartie;
        private System.Windows.Forms.ToolStripMenuItem HumainContreHumain;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem StopMoteur;
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
        private System.Windows.Forms.ToolStripMenuItem PointArret;
        private System.Windows.Forms.RichTextBox VarianteMoteurUci2;
        private System.Windows.Forms.RichTextBox VarianteMoteurUci3;
        private System.Windows.Forms.ToolStripStatusLabel EvaluationUci;
        private System.Windows.Forms.GroupBox groupeJoueurs;
        private System.Windows.Forms.Label LabelJoueurNoir;
        private System.Windows.Forms.Label LabelJoueurBlanc;
        private System.Windows.Forms.Label InformationsPartie;
        private Krypton.Toolkit.KryptonButton MontreVariantesUci;
        private System.Windows.Forms.ToolStripMenuItem ParametresAvances;
        private System.Windows.Forms.Label Decompteur;
        private System.Windows.Forms.Timer ChronometreTempsFixe;
        public Krypton.Toolkit.KryptonButton ListeCoupsBouton;
        public Krypton.Toolkit.KryptonButton RetourArriere;
        public Krypton.Toolkit.KryptonButton AnalysePosition;
        private System.Windows.Forms.ToolStripMenuItem visualiserPgn;
        private System.Windows.Forms.ToolStripMenuItem ParametresDeBase;
        private Krypton.Toolkit.KryptonButton BoutonGainBlanc;
        private Krypton.Toolkit.KryptonButton BoutonGainNoir;
        private Krypton.Toolkit.KryptonButton BoutonNulle;
        public System.Windows.Forms.Label EloNoir;
        public System.Windows.Forms.Label EloBlanc;
    }
}

