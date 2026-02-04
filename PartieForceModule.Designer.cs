namespace BrunoGUI_GenII
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
            GroupeNoirsBlancs = new Krypton.Toolkit.KryptonGroupBox();
            LabelChoixCouleur = new Krypton.Toolkit.KryptonLabel();
            ModuleJoueNoirs = new Krypton.Toolkit.KryptonRadioButton();
            ModuleJoueBlancs = new Krypton.Toolkit.KryptonRadioButton();
            GroupeForceMoteur = new Krypton.Toolkit.KryptonGroupBox();
            LimitesEloPossibles = new Krypton.Toolkit.KryptonLabel();
            ValeurLimiteElo = new Krypton.Toolkit.KryptonNumericUpDown();
            ForceMoteurDefinie = new Krypton.Toolkit.KryptonRadioButton();
            ForceMoteurMaximum = new Krypton.Toolkit.KryptonRadioButton();
            ForceMoteurOk = new Krypton.Toolkit.KryptonButton();
            ForceMoteurAnnuler = new Krypton.Toolkit.KryptonButton();
            GroupeTempsRefflexion = new Krypton.Toolkit.KryptonGroupBox();
            TempsReflexion = new Krypton.Toolkit.KryptonNumericUpDown();
            LabelDureeReflexion = new Krypton.Toolkit.KryptonLabel();
            kryptonGroupBox1 = new Krypton.Toolkit.KryptonGroupBox();
            TextBoxNomAdvesaire = new System.Windows.Forms.RichTextBox();
            LabelAdversaire = new Krypton.Toolkit.KryptonLabel();
            ((System.ComponentModel.ISupportInitialize)GroupeNoirsBlancs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)GroupeNoirsBlancs.Panel).BeginInit();
            GroupeNoirsBlancs.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GroupeForceMoteur).BeginInit();
            ((System.ComponentModel.ISupportInitialize)GroupeForceMoteur.Panel).BeginInit();
            GroupeForceMoteur.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GroupeTempsRefflexion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)GroupeTempsRefflexion.Panel).BeginInit();
            GroupeTempsRefflexion.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonGroupBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)kryptonGroupBox1.Panel).BeginInit();
            kryptonGroupBox1.Panel.SuspendLayout();
            SuspendLayout();
            // 
            // GroupeNoirsBlancs
            // 
            GroupeNoirsBlancs.Location = new System.Drawing.Point(15, 3);
            GroupeNoirsBlancs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            GroupeNoirsBlancs.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            // 
            // 
            // 
            GroupeNoirsBlancs.Panel.Controls.Add(LabelChoixCouleur);
            GroupeNoirsBlancs.Panel.Controls.Add(ModuleJoueNoirs);
            GroupeNoirsBlancs.Panel.Controls.Add(ModuleJoueBlancs);
            GroupeNoirsBlancs.Size = new System.Drawing.Size(356, 69);
            GroupeNoirsBlancs.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            GroupeNoirsBlancs.StateCommon.Border.Rounding = 20F;
            GroupeNoirsBlancs.StateCommon.Border.Width = 1;
            GroupeNoirsBlancs.TabIndex = 0;
            GroupeNoirsBlancs.ToolTipValues.Description = "Sélectionnez la couleur avec laquelle le module va jouer";
            GroupeNoirsBlancs.ToolTipValues.EnableToolTips = true;
            GroupeNoirsBlancs.ToolTipValues.Heading = "";
            GroupeNoirsBlancs.Values.Heading = "     Couleur Moteur";
            // 
            // LabelChoixCouleur
            // 
            LabelChoixCouleur.AutoSize = false;
            LabelChoixCouleur.Location = new System.Drawing.Point(5, 3);
            LabelChoixCouleur.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LabelChoixCouleur.Name = "LabelChoixCouleur";
            LabelChoixCouleur.Size = new System.Drawing.Size(168, 29);
            LabelChoixCouleur.TabIndex = 2;
            LabelChoixCouleur.Values.Text = "Le module joue avec les";
            // 
            // ModuleJoueNoirs
            // 
            ModuleJoueNoirs.AutoSize = false;
            ModuleJoueNoirs.Checked = true;
            ModuleJoueNoirs.Location = new System.Drawing.Point(267, 3);
            ModuleJoueNoirs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ModuleJoueNoirs.Name = "ModuleJoueNoirs";
            ModuleJoueNoirs.Size = new System.Drawing.Size(79, 31);
            ModuleJoueNoirs.TabIndex = 1;
            ModuleJoueNoirs.Values.Text = "Noirs";
            // 
            // ModuleJoueBlancs
            // 
            ModuleJoueBlancs.AutoSize = false;
            ModuleJoueBlancs.Location = new System.Drawing.Point(180, 3);
            ModuleJoueBlancs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ModuleJoueBlancs.Name = "ModuleJoueBlancs";
            ModuleJoueBlancs.Size = new System.Drawing.Size(110, 31);
            ModuleJoueBlancs.TabIndex = 0;
            ModuleJoueBlancs.Values.Text = "Blancs";
            // 
            // GroupeForceMoteur
            // 
            GroupeForceMoteur.Location = new System.Drawing.Point(15, 137);
            GroupeForceMoteur.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            GroupeForceMoteur.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            // 
            // 
            // 
            GroupeForceMoteur.Panel.Controls.Add(LimitesEloPossibles);
            GroupeForceMoteur.Panel.Controls.Add(ValeurLimiteElo);
            GroupeForceMoteur.Panel.Controls.Add(ForceMoteurDefinie);
            GroupeForceMoteur.Panel.Controls.Add(ForceMoteurMaximum);
            GroupeForceMoteur.Size = new System.Drawing.Size(356, 127);
            GroupeForceMoteur.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            GroupeForceMoteur.StateCommon.Border.Rounding = 20F;
            GroupeForceMoteur.StateCommon.Border.Width = 1;
            GroupeForceMoteur.TabIndex = 1;
            GroupeForceMoteur.ToolTipValues.Description = "Sélectionnez la force du moteur en classement ELO";
            GroupeForceMoteur.ToolTipValues.EnableToolTips = true;
            GroupeForceMoteur.ToolTipValues.Heading = "";
            GroupeForceMoteur.Values.Heading = "     Force Moteur";
            // 
            // LimitesEloPossibles
            // 
            LimitesEloPossibles.AutoSize = false;
            LimitesEloPossibles.Location = new System.Drawing.Point(41, 65);
            LimitesEloPossibles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LimitesEloPossibles.Name = "LimitesEloPossibles";
            LimitesEloPossibles.Size = new System.Drawing.Size(302, 29);
            LimitesEloPossibles.TabIndex = 3;
            LimitesEloPossibles.Values.Text = "ELO Minimum : 1320    ELO Maximum : 3190";
            // 
            // ValeurLimiteElo
            // 
            ValeurLimiteElo.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            ValeurLimiteElo.Location = new System.Drawing.Point(241, 35);
            ValeurLimiteElo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ValeurLimiteElo.Maximum = new decimal(new int[] { 3150, 0, 0, 0 });
            ValeurLimiteElo.Minimum = new decimal(new int[] { 1320, 0, 0, 0 });
            ValeurLimiteElo.Name = "ValeurLimiteElo";
            ValeurLimiteElo.Size = new System.Drawing.Size(88, 22);
            ValeurLimiteElo.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            ValeurLimiteElo.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ValeurLimiteElo.TabIndex = 2;
            ValeurLimiteElo.Value = new decimal(new int[] { 1320, 0, 0, 0 });
            // 
            // ForceMoteurDefinie
            // 
            ForceMoteurDefinie.Location = new System.Drawing.Point(41, 35);
            ForceMoteurDefinie.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ForceMoteurDefinie.Name = "ForceMoteurDefinie";
            ForceMoteurDefinie.Size = new System.Drawing.Size(165, 20);
            ForceMoteurDefinie.TabIndex = 1;
            ForceMoteurDefinie.Values.Text = "Limite Force du moteur à : ";
            // 
            // ForceMoteurMaximum
            // 
            ForceMoteurMaximum.Checked = true;
            ForceMoteurMaximum.Location = new System.Drawing.Point(41, 3);
            ForceMoteurMaximum.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ForceMoteurMaximum.Name = "ForceMoteurMaximum";
            ForceMoteurMaximum.Size = new System.Drawing.Size(170, 20);
            ForceMoteurMaximum.TabIndex = 0;
            ForceMoteurMaximum.Values.Text = "Force du moteur maximum";
            // 
            // ForceMoteurOk
            // 
            ForceMoteurOk.Location = new System.Drawing.Point(150, 333);
            ForceMoteurOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ForceMoteurOk.Name = "ForceMoteurOk";
            ForceMoteurOk.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            ForceMoteurOk.Size = new System.Drawing.Size(105, 29);
            ForceMoteurOk.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            ForceMoteurOk.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ForceMoteurOk.StateCommon.Border.Rounding = 20F;
            ForceMoteurOk.StateCommon.Border.Width = 3;
            ForceMoteurOk.TabIndex = 2;
            ForceMoteurOk.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            ForceMoteurOk.Values.Text = "OK";
            // 
            // ForceMoteurAnnuler
            // 
            ForceMoteurAnnuler.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            ForceMoteurAnnuler.Location = new System.Drawing.Point(266, 333);
            ForceMoteurAnnuler.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ForceMoteurAnnuler.Name = "ForceMoteurAnnuler";
            ForceMoteurAnnuler.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            ForceMoteurAnnuler.Size = new System.Drawing.Size(105, 29);
            ForceMoteurAnnuler.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            ForceMoteurAnnuler.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ForceMoteurAnnuler.StateCommon.Border.Rounding = 20F;
            ForceMoteurAnnuler.StateCommon.Border.Width = 3;
            ForceMoteurAnnuler.TabIndex = 3;
            ForceMoteurAnnuler.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            ForceMoteurAnnuler.Values.Text = "Annuler";
            // 
            // GroupeTempsRefflexion
            // 
            GroupeTempsRefflexion.Location = new System.Drawing.Point(14, 260);
            GroupeTempsRefflexion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            GroupeTempsRefflexion.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            // 
            // 
            // 
            GroupeTempsRefflexion.Panel.Controls.Add(TempsReflexion);
            GroupeTempsRefflexion.Panel.Controls.Add(LabelDureeReflexion);
            GroupeTempsRefflexion.Size = new System.Drawing.Size(356, 67);
            GroupeTempsRefflexion.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            GroupeTempsRefflexion.StateCommon.Border.Rounding = 20F;
            GroupeTempsRefflexion.StateCommon.Border.Width = 1;
            GroupeTempsRefflexion.TabIndex = 4;
            GroupeTempsRefflexion.ToolTipValues.Description = "Sélectionnez la durée de réflexion du moteur en secondes";
            GroupeTempsRefflexion.ToolTipValues.EnableToolTips = true;
            GroupeTempsRefflexion.ToolTipValues.Heading = "";
            GroupeTempsRefflexion.Values.Heading = "     Durée Réflexion";
            // 
            // TempsReflexion
            // 
            TempsReflexion.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            TempsReflexion.Location = new System.Drawing.Point(240, 0);
            TempsReflexion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TempsReflexion.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            TempsReflexion.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            TempsReflexion.Name = "TempsReflexion";
            TempsReflexion.Size = new System.Drawing.Size(89, 22);
            TempsReflexion.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            TempsReflexion.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            TempsReflexion.TabIndex = 1;
            TempsReflexion.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // LabelDureeReflexion
            // 
            LabelDureeReflexion.AutoSize = false;
            LabelDureeReflexion.Location = new System.Drawing.Point(9, 0);
            LabelDureeReflexion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LabelDureeReflexion.Name = "LabelDureeReflexion";
            LabelDureeReflexion.Size = new System.Drawing.Size(229, 29);
            LabelDureeReflexion.TabIndex = 0;
            LabelDureeReflexion.Values.Text = "Durée réflexion (en secondes) :";
            // 
            // kryptonGroupBox1
            // 
            kryptonGroupBox1.Location = new System.Drawing.Point(14, 67);
            kryptonGroupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            kryptonGroupBox1.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            // 
            // 
            // 
            kryptonGroupBox1.Panel.Controls.Add(TextBoxNomAdvesaire);
            kryptonGroupBox1.Panel.Controls.Add(LabelAdversaire);
            kryptonGroupBox1.Size = new System.Drawing.Size(356, 63);
            kryptonGroupBox1.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            kryptonGroupBox1.StateCommon.Border.Rounding = 20F;
            kryptonGroupBox1.StateCommon.Border.Width = 1;
            kryptonGroupBox1.TabIndex = 5;
            kryptonGroupBox1.ToolTipValues.Description = "Entrez le nom de l'adversaire de Stockfish";
            kryptonGroupBox1.ToolTipValues.EnableToolTips = true;
            kryptonGroupBox1.ToolTipValues.Heading = "";
            kryptonGroupBox1.Values.Heading = "    Adversaire";
            // 
            // TextBoxNomAdvesaire
            // 
            TextBoxNomAdvesaire.Location = new System.Drawing.Point(163, 3);
            TextBoxNomAdvesaire.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TextBoxNomAdvesaire.Name = "TextBoxNomAdvesaire";
            TextBoxNomAdvesaire.Size = new System.Drawing.Size(165, 22);
            TextBoxNomAdvesaire.TabIndex = 1;
            TextBoxNomAdvesaire.Text = "";
            // 
            // LabelAdversaire
            // 
            LabelAdversaire.Location = new System.Drawing.Point(9, 3);
            LabelAdversaire.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LabelAdversaire.Name = "LabelAdversaire";
            LabelAdversaire.Size = new System.Drawing.Size(135, 20);
            LabelAdversaire.TabIndex = 0;
            LabelAdversaire.Values.Text = "Adversaire du moteur : ";
            // 
            // PartieForceModule
            // 
            AcceptButton = ForceMoteurOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Silver;
            CancelButton = ForceMoteurAnnuler;
            ClientSize = new System.Drawing.Size(390, 376);
            Controls.Add(kryptonGroupBox1);
            Controls.Add(GroupeTempsRefflexion);
            Controls.Add(ForceMoteurAnnuler);
            Controls.Add(ForceMoteurOk);
            Controls.Add(GroupeForceMoteur);
            Controls.Add(GroupeNoirsBlancs);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "PartieForceModule";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Nouvelle Partie et Force Module";
            Load += NouvellePartieForceModule_Load;
            ((System.ComponentModel.ISupportInitialize)GroupeNoirsBlancs.Panel).EndInit();
            GroupeNoirsBlancs.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GroupeNoirsBlancs).EndInit();
            ((System.ComponentModel.ISupportInitialize)GroupeForceMoteur.Panel).EndInit();
            GroupeForceMoteur.Panel.ResumeLayout(false);
            GroupeForceMoteur.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)GroupeForceMoteur).EndInit();
            ((System.ComponentModel.ISupportInitialize)GroupeTempsRefflexion.Panel).EndInit();
            GroupeTempsRefflexion.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GroupeTempsRefflexion).EndInit();
            ((System.ComponentModel.ISupportInitialize)kryptonGroupBox1.Panel).EndInit();
            kryptonGroupBox1.Panel.ResumeLayout(false);
            kryptonGroupBox1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kryptonGroupBox1).EndInit();
            ResumeLayout(false);

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
        private Krypton.Toolkit.KryptonGroupBox kryptonGroupBox1;
        private Krypton.Toolkit.KryptonLabel LabelAdversaire;
        private System.Windows.Forms.RichTextBox TextBoxNomAdvesaire;
    }
}