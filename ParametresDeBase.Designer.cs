namespace BrunoGUI_GenII
{
    partial class ParametresDeBase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParametresDeBase));
            GroupParametreDeBase = new Krypton.Toolkit.KryptonGroupBox();
            baseReflexionNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            baseMultipvNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            baseThreadsNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            baseEloNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            LabelBaseReflexion = new Krypton.Toolkit.KryptonLabel();
            LabelBaseMultiPv = new Krypton.Toolkit.KryptonLabel();
            LabelBaseThreads = new Krypton.Toolkit.KryptonLabel();
            LabelBaseELo = new Krypton.Toolkit.KryptonLabel();
            BaseBoutonOk = new Krypton.Toolkit.KryptonButton();
            BaseBoutonAnnuler = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)GroupParametreDeBase).BeginInit();
            ((System.ComponentModel.ISupportInitialize)GroupParametreDeBase.Panel).BeginInit();
            GroupParametreDeBase.Panel.SuspendLayout();
            SuspendLayout();
            // 
            // GroupParametreDeBase
            // 
            GroupParametreDeBase.CausesValidation = false;
            GroupParametreDeBase.Location = new System.Drawing.Point(14, 14);
            GroupParametreDeBase.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            // 
            // 
            // 
            GroupParametreDeBase.Panel.Controls.Add(baseReflexionNumerique);
            GroupParametreDeBase.Panel.Controls.Add(baseMultipvNumerique);
            GroupParametreDeBase.Panel.Controls.Add(baseThreadsNumerique);
            GroupParametreDeBase.Panel.Controls.Add(baseEloNumerique);
            GroupParametreDeBase.Panel.Controls.Add(LabelBaseReflexion);
            GroupParametreDeBase.Panel.Controls.Add(LabelBaseMultiPv);
            GroupParametreDeBase.Panel.Controls.Add(LabelBaseThreads);
            GroupParametreDeBase.Panel.Controls.Add(LabelBaseELo);
            GroupParametreDeBase.Size = new System.Drawing.Size(397, 196);
            GroupParametreDeBase.StateCommon.Back.Color1 = System.Drawing.Color.Silver;
            GroupParametreDeBase.TabIndex = 0;
            GroupParametreDeBase.Values.Heading = "  Paramètres de Base (ELO, Thread, MultiPV, Réflexion)";
            // 
            // baseReflexionNumerique
            // 
            baseReflexionNumerique.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            baseReflexionNumerique.Location = new System.Drawing.Point(233, 127);
            baseReflexionNumerique.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            baseReflexionNumerique.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            baseReflexionNumerique.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            baseReflexionNumerique.Name = "baseReflexionNumerique";
            baseReflexionNumerique.Size = new System.Drawing.Size(140, 22);
            baseReflexionNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            baseReflexionNumerique.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            baseReflexionNumerique.TabIndex = 7;
            baseReflexionNumerique.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // baseMultipvNumerique
            // 
            baseMultipvNumerique.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            baseMultipvNumerique.Location = new System.Drawing.Point(233, 92);
            baseMultipvNumerique.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            baseMultipvNumerique.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            baseMultipvNumerique.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            baseMultipvNumerique.Name = "baseMultipvNumerique";
            baseMultipvNumerique.Size = new System.Drawing.Size(140, 22);
            baseMultipvNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            baseMultipvNumerique.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            baseMultipvNumerique.TabIndex = 6;
            baseMultipvNumerique.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // baseThreadsNumerique
            // 
            baseThreadsNumerique.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            baseThreadsNumerique.Location = new System.Drawing.Point(233, 58);
            baseThreadsNumerique.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            baseThreadsNumerique.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            baseThreadsNumerique.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            baseThreadsNumerique.Name = "baseThreadsNumerique";
            baseThreadsNumerique.Size = new System.Drawing.Size(140, 22);
            baseThreadsNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            baseThreadsNumerique.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            baseThreadsNumerique.TabIndex = 5;
            baseThreadsNumerique.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // baseEloNumerique
            // 
            baseEloNumerique.Increment = new decimal(new int[] { 1, 0, 0, 0 });
            baseEloNumerique.Location = new System.Drawing.Point(233, 23);
            baseEloNumerique.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            baseEloNumerique.Maximum = new decimal(new int[] { 3190, 0, 0, 0 });
            baseEloNumerique.Minimum = new decimal(new int[] { 1320, 0, 0, 0 });
            baseEloNumerique.Name = "baseEloNumerique";
            baseEloNumerique.Size = new System.Drawing.Size(140, 22);
            baseEloNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            baseEloNumerique.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            baseEloNumerique.TabIndex = 4;
            baseEloNumerique.Value = new decimal(new int[] { 1958, 0, 0, 0 });
            // 
            // LabelBaseReflexion
            // 
            LabelBaseReflexion.AutoSize = false;
            LabelBaseReflexion.Location = new System.Drawing.Point(23, 127);
            LabelBaseReflexion.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LabelBaseReflexion.Name = "LabelBaseReflexion";
            LabelBaseReflexion.Size = new System.Drawing.Size(198, 23);
            LabelBaseReflexion.TabIndex = 3;
            LabelBaseReflexion.Values.Text = "Réflexion (0 à 600 secondes) : ";
            // 
            // LabelBaseMultiPv
            // 
            LabelBaseMultiPv.AutoSize = false;
            LabelBaseMultiPv.Location = new System.Drawing.Point(23, 92);
            LabelBaseMultiPv.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LabelBaseMultiPv.Name = "LabelBaseMultiPv";
            LabelBaseMultiPv.Size = new System.Drawing.Size(198, 23);
            LabelBaseMultiPv.TabIndex = 2;
            LabelBaseMultiPv.Values.Text = "Nombre variations (1 à 3) : ";
            // 
            // LabelBaseThreads
            // 
            LabelBaseThreads.AutoSize = false;
            LabelBaseThreads.Location = new System.Drawing.Point(23, 58);
            LabelBaseThreads.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LabelBaseThreads.Name = "LabelBaseThreads";
            LabelBaseThreads.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            LabelBaseThreads.Size = new System.Drawing.Size(198, 23);
            LabelBaseThreads.TabIndex = 1;
            LabelBaseThreads.Values.Text = "Nombre Threads (1 à 64) : ";
            // 
            // LabelBaseELo
            // 
            LabelBaseELo.AutoSize = false;
            LabelBaseELo.Location = new System.Drawing.Point(23, 23);
            LabelBaseELo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            LabelBaseELo.Name = "LabelBaseELo";
            LabelBaseELo.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            LabelBaseELo.Size = new System.Drawing.Size(198, 23);
            LabelBaseELo.StateCommon.LongText.Color1 = System.Drawing.Color.Gray;
            LabelBaseELo.TabIndex = 0;
            LabelBaseELo.Values.Text = "Force ELO (1320 à 3190) : ";
            // 
            // BaseBoutonOk
            // 
            BaseBoutonOk.Location = new System.Drawing.Point(173, 218);
            BaseBoutonOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BaseBoutonOk.Name = "BaseBoutonOk";
            BaseBoutonOk.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BaseBoutonOk.Size = new System.Drawing.Size(105, 29);
            BaseBoutonOk.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            BaseBoutonOk.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BaseBoutonOk.StateCommon.Border.Rounding = 20F;
            BaseBoutonOk.StateCommon.Border.Width = 3;
            BaseBoutonOk.TabIndex = 1;
            BaseBoutonOk.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BaseBoutonOk.Values.Text = "OK";
            BaseBoutonOk.Click += BaseBoutonOk_Click;
            // 
            // BaseBoutonAnnuler
            // 
            BaseBoutonAnnuler.Location = new System.Drawing.Point(286, 218);
            BaseBoutonAnnuler.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BaseBoutonAnnuler.Name = "BaseBoutonAnnuler";
            BaseBoutonAnnuler.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            BaseBoutonAnnuler.Size = new System.Drawing.Size(105, 29);
            BaseBoutonAnnuler.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            BaseBoutonAnnuler.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            BaseBoutonAnnuler.StateCommon.Border.Rounding = 20F;
            BaseBoutonAnnuler.StateCommon.Border.Width = 3;
            BaseBoutonAnnuler.TabIndex = 2;
            BaseBoutonAnnuler.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            BaseBoutonAnnuler.Values.Text = "Annuler";
            BaseBoutonAnnuler.Click += BaseBoutonAnnuler_Click;
            // 
            // ParametresDeBase
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Silver;
            ClientSize = new System.Drawing.Size(432, 268);
            Controls.Add(BaseBoutonAnnuler);
            Controls.Add(BaseBoutonOk);
            Controls.Add(GroupParametreDeBase);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ParametresDeBase";
            Text = "Parametres de base du moteur UCI";
            ((System.ComponentModel.ISupportInitialize)GroupParametreDeBase.Panel).EndInit();
            GroupParametreDeBase.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GroupParametreDeBase).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private Krypton.Toolkit.KryptonGroupBox GroupParametreDeBase;
        private Krypton.Toolkit.KryptonLabel LabelBaseReflexion;
        private Krypton.Toolkit.KryptonLabel LabelBaseMultiPv;
        private Krypton.Toolkit.KryptonLabel LabelBaseThreads;
        private Krypton.Toolkit.KryptonLabel LabelBaseELo;
        private Krypton.Toolkit.KryptonNumericUpDown baseReflexionNumerique;
        private Krypton.Toolkit.KryptonNumericUpDown baseMultipvNumerique;
        private Krypton.Toolkit.KryptonNumericUpDown baseThreadsNumerique;
        private Krypton.Toolkit.KryptonNumericUpDown baseEloNumerique;
        private Krypton.Toolkit.KryptonButton BaseBoutonOk;
        private Krypton.Toolkit.KryptonButton BaseBoutonAnnuler;
    }
}