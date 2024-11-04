namespace BrunoGUI_Stockfish
{
    partial class PartieForceModule
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PartieForceModule));
            this.GroupeNoirsBlancs = new Krypton.Toolkit.KryptonGroupBox();
            this.LabelChoixCouleur = new Krypton.Toolkit.KryptonLabel();
            this.ModuleJoueNoirs = new Krypton.Toolkit.KryptonRadioButton();
            this.ModuleJoueBlancs = new Krypton.Toolkit.KryptonRadioButton();
            this.GroupeForceMoteur = new Krypton.Toolkit.KryptonGroupBox();
            this.LimitesEloPossibles = new Krypton.Toolkit.KryptonLabel();
            this.ValeurLimiteElo = new Krypton.Toolkit.KryptonNumericUpDown();
            this.ForceMoteurDefinie = new Krypton.Toolkit.KryptonRadioButton();
            this.ForceMoteurMaximum = new Krypton.Toolkit.KryptonRadioButton();
            this.ForceMoteurOk = new Krypton.Toolkit.KryptonButton();
            this.ForceMoteurAnnuler = new Krypton.Toolkit.KryptonButton();
            this.GroupeTempsRefflexion = new Krypton.Toolkit.KryptonGroupBox();
            this.TempsReflexion = new Krypton.Toolkit.KryptonNumericUpDown();
            this.LabelDureeReflexion = new Krypton.Toolkit.KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)(this.GroupeNoirsBlancs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupeNoirsBlancs.Panel)).BeginInit();
            this.GroupeNoirsBlancs.Panel.SuspendLayout();
            this.GroupeNoirsBlancs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GroupeForceMoteur)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupeForceMoteur.Panel)).BeginInit();
            this.GroupeForceMoteur.Panel.SuspendLayout();
            this.GroupeForceMoteur.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GroupeTempsRefflexion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupeTempsRefflexion.Panel)).BeginInit();
            this.GroupeTempsRefflexion.Panel.SuspendLayout();
            this.GroupeTempsRefflexion.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupeNoirsBlancs
            // 
            this.GroupeNoirsBlancs.Location = new System.Drawing.Point(13, 0);
            this.GroupeNoirsBlancs.Name = "GroupeNoirsBlancs";
            this.GroupeNoirsBlancs.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            // 
            // GroupeNoirsBlancs.Panel
            // 
            this.GroupeNoirsBlancs.Panel.Controls.Add(this.LabelChoixCouleur);
            this.GroupeNoirsBlancs.Panel.Controls.Add(this.ModuleJoueNoirs);
            this.GroupeNoirsBlancs.Panel.Controls.Add(this.ModuleJoueBlancs);
            this.GroupeNoirsBlancs.Size = new System.Drawing.Size(305, 60);
            this.GroupeNoirsBlancs.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.GroupeNoirsBlancs.StateCommon.Border.Rounding = 20F;
            this.GroupeNoirsBlancs.StateCommon.Border.Width = 1;
            this.GroupeNoirsBlancs.TabIndex = 0;
            this.GroupeNoirsBlancs.ToolTipValues.Description = "Sélectionnez la couleur avec laquelle le module va jouer";
            this.GroupeNoirsBlancs.ToolTipValues.EnableToolTips = true;
            this.GroupeNoirsBlancs.ToolTipValues.Heading = "";
            this.GroupeNoirsBlancs.Values.Heading = "     Couleur Moteur";
            // 
            // LabelChoixCouleur
            // 
            this.LabelChoixCouleur.AutoSize = false;
            this.LabelChoixCouleur.Location = new System.Drawing.Point(4, 3);
            this.LabelChoixCouleur.Name = "LabelChoixCouleur";
            this.LabelChoixCouleur.Size = new System.Drawing.Size(144, 25);
            this.LabelChoixCouleur.TabIndex = 2;
            this.LabelChoixCouleur.Values.Text = "Le module joue avec les";
            // 
            // ModuleJoueNoirs
            // 
            this.ModuleJoueNoirs.AutoSize = false;
            this.ModuleJoueNoirs.Checked = true;
            this.ModuleJoueNoirs.Location = new System.Drawing.Point(229, 3);
            this.ModuleJoueNoirs.Name = "ModuleJoueNoirs";
            this.ModuleJoueNoirs.Size = new System.Drawing.Size(68, 27);
            this.ModuleJoueNoirs.TabIndex = 1;
            this.ModuleJoueNoirs.Values.Text = "Noirs";
            // 
            // ModuleJoueBlancs
            // 
            this.ModuleJoueBlancs.AutoSize = false;
            this.ModuleJoueBlancs.Location = new System.Drawing.Point(154, 3);
            this.ModuleJoueBlancs.Name = "ModuleJoueBlancs";
            this.ModuleJoueBlancs.Size = new System.Drawing.Size(94, 27);
            this.ModuleJoueBlancs.TabIndex = 0;
            this.ModuleJoueBlancs.Values.Text = "Blancs";
            // 
            // GroupeForceMoteur
            // 
            this.GroupeForceMoteur.Location = new System.Drawing.Point(12, 59);
            this.GroupeForceMoteur.Name = "GroupeForceMoteur";
            this.GroupeForceMoteur.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            // 
            // GroupeForceMoteur.Panel
            // 
            this.GroupeForceMoteur.Panel.Controls.Add(this.LimitesEloPossibles);
            this.GroupeForceMoteur.Panel.Controls.Add(this.ValeurLimiteElo);
            this.GroupeForceMoteur.Panel.Controls.Add(this.ForceMoteurDefinie);
            this.GroupeForceMoteur.Panel.Controls.Add(this.ForceMoteurMaximum);
            this.GroupeForceMoteur.Size = new System.Drawing.Size(305, 110);
            this.GroupeForceMoteur.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.GroupeForceMoteur.StateCommon.Border.Rounding = 20F;
            this.GroupeForceMoteur.StateCommon.Border.Width = 1;
            this.GroupeForceMoteur.TabIndex = 1;
            this.GroupeForceMoteur.ToolTipValues.Description = "Sélectionnez la force du moteur  en  classemnt ELO";
            this.GroupeForceMoteur.ToolTipValues.EnableToolTips = true;
            this.GroupeForceMoteur.ToolTipValues.Heading = "";
            this.GroupeForceMoteur.Values.Heading = "     Force Moteur";
            // 
            // LimitesEloPossibles
            // 
            this.LimitesEloPossibles.AutoSize = false;
            this.LimitesEloPossibles.Location = new System.Drawing.Point(35, 56);
            this.LimitesEloPossibles.Name = "LimitesEloPossibles";
            this.LimitesEloPossibles.Size = new System.Drawing.Size(259, 25);
            this.LimitesEloPossibles.TabIndex = 3;
            this.LimitesEloPossibles.Values.Text = "ELO Minimum : 1320    ELO Maximum : 3190";
            // 
            // ValeurLimiteElo
            // 
            this.ValeurLimiteElo.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.ValeurLimiteElo.Location = new System.Drawing.Point(207, 30);
            this.ValeurLimiteElo.Maximum = new decimal(new int[] {
            3150,
            0,
            0,
            0});
            this.ValeurLimiteElo.Minimum = new decimal(new int[] {
            1320,
            0,
            0,
            0});
            this.ValeurLimiteElo.Name = "ValeurLimiteElo";
            this.ValeurLimiteElo.Size = new System.Drawing.Size(75, 22);
            this.ValeurLimiteElo.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.ValeurLimiteElo.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ValeurLimiteElo.TabIndex = 2;
            this.ValeurLimiteElo.Value = new decimal(new int[] {
            1320,
            0,
            0,
            0});
            // 
            // ForceMoteurDefinie
            // 
            this.ForceMoteurDefinie.Location = new System.Drawing.Point(35, 30);
            this.ForceMoteurDefinie.Name = "ForceMoteurDefinie";
            this.ForceMoteurDefinie.Size = new System.Drawing.Size(165, 20);
            this.ForceMoteurDefinie.TabIndex = 1;
            this.ForceMoteurDefinie.Values.Text = "Limite Force du moteur à : ";
            // 
            // ForceMoteurMaximum
            // 
            this.ForceMoteurMaximum.Checked = true;
            this.ForceMoteurMaximum.Location = new System.Drawing.Point(35, 3);
            this.ForceMoteurMaximum.Name = "ForceMoteurMaximum";
            this.ForceMoteurMaximum.Size = new System.Drawing.Size(170, 20);
            this.ForceMoteurMaximum.TabIndex = 0;
            this.ForceMoteurMaximum.Values.Text = "Force du moteur maximum";
            // 
            // ForceMoteurOk
            // 
            this.ForceMoteurOk.Location = new System.Drawing.Point(129, 232);
            this.ForceMoteurOk.Name = "ForceMoteurOk";
            this.ForceMoteurOk.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.ForceMoteurOk.Size = new System.Drawing.Size(90, 25);
            this.ForceMoteurOk.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.ForceMoteurOk.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ForceMoteurOk.StateCommon.Border.Rounding = 20F;
            this.ForceMoteurOk.StateCommon.Border.Width = 3;
            this.ForceMoteurOk.TabIndex = 2;
            this.ForceMoteurOk.Values.Text = "OK";
            // 
            // ForceMoteurAnnuler
            // 
            this.ForceMoteurAnnuler.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ForceMoteurAnnuler.Location = new System.Drawing.Point(228, 232);
            this.ForceMoteurAnnuler.Name = "ForceMoteurAnnuler";
            this.ForceMoteurAnnuler.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.ForceMoteurAnnuler.Size = new System.Drawing.Size(90, 25);
            this.ForceMoteurAnnuler.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.ForceMoteurAnnuler.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ForceMoteurAnnuler.StateCommon.Border.Rounding = 20F;
            this.ForceMoteurAnnuler.StateCommon.Border.Width = 3;
            this.ForceMoteurAnnuler.TabIndex = 3;
            this.ForceMoteurAnnuler.Values.Text = "Annuler";
            // 
            // GroupeTempsRefflexion
            // 
            this.GroupeTempsRefflexion.Location = new System.Drawing.Point(13, 168);
            this.GroupeTempsRefflexion.Name = "GroupeTempsRefflexion";
            this.GroupeTempsRefflexion.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            // 
            // GroupeTempsRefflexion.Panel
            // 
            this.GroupeTempsRefflexion.Panel.Controls.Add(this.TempsReflexion);
            this.GroupeTempsRefflexion.Panel.Controls.Add(this.LabelDureeReflexion);
            this.GroupeTempsRefflexion.Size = new System.Drawing.Size(305, 58);
            this.GroupeTempsRefflexion.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.GroupeTempsRefflexion.StateCommon.Border.Rounding = 20F;
            this.GroupeTempsRefflexion.StateCommon.Border.Width = 1;
            this.GroupeTempsRefflexion.TabIndex = 4;
            this.GroupeTempsRefflexion.ToolTipValues.Description = "Sélectionnez la durée de réflexion du moteur en secondes";
            this.GroupeTempsRefflexion.ToolTipValues.EnableToolTips = true;
            this.GroupeTempsRefflexion.ToolTipValues.Heading = "";
            this.GroupeTempsRefflexion.Values.Heading = "     Durée Réflexion";
            // 
            // TempsReflexion
            // 
            this.TempsReflexion.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TempsReflexion.Location = new System.Drawing.Point(206, 0);
            this.TempsReflexion.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.TempsReflexion.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.TempsReflexion.Name = "TempsReflexion";
            this.TempsReflexion.Size = new System.Drawing.Size(76, 22);
            this.TempsReflexion.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.TempsReflexion.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.TempsReflexion.TabIndex = 1;
            this.TempsReflexion.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // LabelDureeReflexion
            // 
            this.LabelDureeReflexion.AutoSize = false;
            this.LabelDureeReflexion.Location = new System.Drawing.Point(8, 0);
            this.LabelDureeReflexion.Name = "LabelDureeReflexion";
            this.LabelDureeReflexion.Size = new System.Drawing.Size(196, 25);
            this.LabelDureeReflexion.TabIndex = 0;
            this.LabelDureeReflexion.Values.Text = "Durée réflexion (en secondes) :";
            // 
            // PartieForceModule
            // 
            this.AcceptButton = this.ForceMoteurOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ForceMoteurAnnuler;
            this.ClientSize = new System.Drawing.Size(334, 269);
            this.Controls.Add(this.GroupeTempsRefflexion);
            this.Controls.Add(this.ForceMoteurAnnuler);
            this.Controls.Add(this.ForceMoteurOk);
            this.Controls.Add(this.GroupeForceMoteur);
            this.Controls.Add(this.GroupeNoirsBlancs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PartieForceModule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nouvelle Partie et Force Module";
            this.Load += new System.EventHandler(this.NouvellePartieForceModule_Load);
            ((System.ComponentModel.ISupportInitialize)(this.GroupeNoirsBlancs.Panel)).EndInit();
            this.GroupeNoirsBlancs.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GroupeNoirsBlancs)).EndInit();
            this.GroupeNoirsBlancs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GroupeForceMoteur.Panel)).EndInit();
            this.GroupeForceMoteur.Panel.ResumeLayout(false);
            this.GroupeForceMoteur.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GroupeForceMoteur)).EndInit();
            this.GroupeForceMoteur.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GroupeTempsRefflexion.Panel)).EndInit();
            this.GroupeTempsRefflexion.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GroupeTempsRefflexion)).EndInit();
            this.GroupeTempsRefflexion.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Krypton.Toolkit.KryptonGroupBox GroupeNoirsBlancs;
        private Krypton.Toolkit.KryptonRadioButton ModuleJoueNoirs;
        private Krypton.Toolkit.KryptonRadioButton ModuleJoueBlancs;
        private Krypton.Toolkit.KryptonLabel LabelChoixCouleur;
        private Krypton.Toolkit.KryptonGroupBox GroupeForceMoteur;
        private Krypton.Toolkit.KryptonNumericUpDown ValeurLimiteElo;
        private Krypton.Toolkit.KryptonRadioButton ForceMoteurDefinie;
        private Krypton.Toolkit.KryptonRadioButton ForceMoteurMaximum;
        private Krypton.Toolkit.KryptonLabel LimitesEloPossibles;
        private Krypton.Toolkit.KryptonButton ForceMoteurOk;
        private Krypton.Toolkit.KryptonButton ForceMoteurAnnuler;
        private Krypton.Toolkit.KryptonGroupBox GroupeTempsRefflexion;
        private Krypton.Toolkit.KryptonNumericUpDown TempsReflexion;
        private Krypton.Toolkit.KryptonLabel LabelDureeReflexion;
    }
}