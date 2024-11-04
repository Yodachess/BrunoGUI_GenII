namespace BrunoGUI_Stockfish
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
            this.GroupParametreDeBase = new Krypton.Toolkit.KryptonGroupBox();
            this.baseReflexionNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            this.baseMultipvNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            this.baseThreadsNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            this.baseEloNumerique = new Krypton.Toolkit.KryptonNumericUpDown();
            this.LabelBaseReflexion = new Krypton.Toolkit.KryptonLabel();
            this.LabelBaseMultiPv = new Krypton.Toolkit.KryptonLabel();
            this.LabelBaseThreads = new Krypton.Toolkit.KryptonLabel();
            this.LabelBaseELo = new Krypton.Toolkit.KryptonLabel();
            this.BaseBoutonOk = new Krypton.Toolkit.KryptonButton();
            this.BaseBoutonAnnuler = new Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.GroupParametreDeBase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GroupParametreDeBase.Panel)).BeginInit();
            this.GroupParametreDeBase.Panel.SuspendLayout();
            this.GroupParametreDeBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupParametreDeBase
            // 
            this.GroupParametreDeBase.CausesValidation = false;
            this.GroupParametreDeBase.Location = new System.Drawing.Point(12, 12);
            this.GroupParametreDeBase.Name = "GroupParametreDeBase";
            // 
            // GroupParametreDeBase.Panel
            // 
            this.GroupParametreDeBase.Panel.Controls.Add(this.baseReflexionNumerique);
            this.GroupParametreDeBase.Panel.Controls.Add(this.baseMultipvNumerique);
            this.GroupParametreDeBase.Panel.Controls.Add(this.baseThreadsNumerique);
            this.GroupParametreDeBase.Panel.Controls.Add(this.baseEloNumerique);
            this.GroupParametreDeBase.Panel.Controls.Add(this.LabelBaseReflexion);
            this.GroupParametreDeBase.Panel.Controls.Add(this.LabelBaseMultiPv);
            this.GroupParametreDeBase.Panel.Controls.Add(this.LabelBaseThreads);
            this.GroupParametreDeBase.Panel.Controls.Add(this.LabelBaseELo);
            this.GroupParametreDeBase.Size = new System.Drawing.Size(340, 170);
            this.GroupParametreDeBase.TabIndex = 0;
            this.GroupParametreDeBase.Values.Heading = "  Paramètres de Base (ELO, Thread, MultiPV, Réflexion)";
            // 
            // baseReflexionNumerique
            // 
            this.baseReflexionNumerique.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.baseReflexionNumerique.Location = new System.Drawing.Point(200, 110);
            this.baseReflexionNumerique.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.baseReflexionNumerique.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.baseReflexionNumerique.Name = "baseReflexionNumerique";
            this.baseReflexionNumerique.Size = new System.Drawing.Size(120, 22);
            this.baseReflexionNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.baseReflexionNumerique.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.baseReflexionNumerique.TabIndex = 7;
            this.baseReflexionNumerique.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // baseMultipvNumerique
            // 
            this.baseMultipvNumerique.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.baseMultipvNumerique.Location = new System.Drawing.Point(200, 80);
            this.baseMultipvNumerique.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.baseMultipvNumerique.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.baseMultipvNumerique.Name = "baseMultipvNumerique";
            this.baseMultipvNumerique.Size = new System.Drawing.Size(120, 22);
            this.baseMultipvNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.baseMultipvNumerique.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.baseMultipvNumerique.TabIndex = 6;
            this.baseMultipvNumerique.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // baseThreadsNumerique
            // 
            this.baseThreadsNumerique.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.baseThreadsNumerique.Location = new System.Drawing.Point(200, 50);
            this.baseThreadsNumerique.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.baseThreadsNumerique.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.baseThreadsNumerique.Name = "baseThreadsNumerique";
            this.baseThreadsNumerique.Size = new System.Drawing.Size(120, 22);
            this.baseThreadsNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.baseThreadsNumerique.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.baseThreadsNumerique.TabIndex = 5;
            this.baseThreadsNumerique.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // baseEloNumerique
            // 
            this.baseEloNumerique.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.baseEloNumerique.Location = new System.Drawing.Point(200, 20);
            this.baseEloNumerique.Maximum = new decimal(new int[] {
            3190,
            0,
            0,
            0});
            this.baseEloNumerique.Minimum = new decimal(new int[] {
            1320,
            0,
            0,
            0});
            this.baseEloNumerique.Name = "baseEloNumerique";
            this.baseEloNumerique.Size = new System.Drawing.Size(120, 22);
            this.baseEloNumerique.StateCommon.Border.Color1 = System.Drawing.Color.Black;
            this.baseEloNumerique.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.baseEloNumerique.TabIndex = 4;
            this.baseEloNumerique.Value = new decimal(new int[] {
            1958,
            0,
            0,
            0});
            // 
            // LabelBaseReflexion
            // 
            this.LabelBaseReflexion.AutoSize = false;
            this.LabelBaseReflexion.Location = new System.Drawing.Point(20, 110);
            this.LabelBaseReflexion.Name = "LabelBaseReflexion";
            this.LabelBaseReflexion.Size = new System.Drawing.Size(170, 20);
            this.LabelBaseReflexion.TabIndex = 3;
            this.LabelBaseReflexion.Values.Text = "Réflexion (en secondes) : ";
            // 
            // LabelBaseMultiPv
            // 
            this.LabelBaseMultiPv.AutoSize = false;
            this.LabelBaseMultiPv.Location = new System.Drawing.Point(20, 80);
            this.LabelBaseMultiPv.Name = "LabelBaseMultiPv";
            this.LabelBaseMultiPv.Size = new System.Drawing.Size(170, 20);
            this.LabelBaseMultiPv.TabIndex = 2;
            this.LabelBaseMultiPv.Values.Text = "Nombre variations (1 à 256) : ";
            // 
            // LabelBaseThreads
            // 
            this.LabelBaseThreads.AutoSize = false;
            this.LabelBaseThreads.Location = new System.Drawing.Point(20, 50);
            this.LabelBaseThreads.Name = "LabelBaseThreads";
            this.LabelBaseThreads.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.LabelBaseThreads.Size = new System.Drawing.Size(170, 20);
            this.LabelBaseThreads.TabIndex = 1;
            this.LabelBaseThreads.Values.Text = "Nombre Threads (1 à 1024) : ";
            // 
            // LabelBaseELo
            // 
            this.LabelBaseELo.AutoSize = false;
            this.LabelBaseELo.Location = new System.Drawing.Point(20, 20);
            this.LabelBaseELo.Name = "LabelBaseELo";
            this.LabelBaseELo.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.LabelBaseELo.Size = new System.Drawing.Size(170, 20);
            this.LabelBaseELo.TabIndex = 0;
            this.LabelBaseELo.Values.Text = "Force ELO (1320 à 3190) : ";
            // 
            // BaseBoutonOk
            // 
            this.BaseBoutonOk.Location = new System.Drawing.Point(148, 189);
            this.BaseBoutonOk.Name = "BaseBoutonOk";
            this.BaseBoutonOk.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.BaseBoutonOk.Size = new System.Drawing.Size(90, 25);
            this.BaseBoutonOk.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.BaseBoutonOk.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.BaseBoutonOk.StateCommon.Border.Rounding = 20F;
            this.BaseBoutonOk.StateCommon.Border.Width = 3;
            this.BaseBoutonOk.TabIndex = 1;
            this.BaseBoutonOk.Values.Text = "OK";
            this.BaseBoutonOk.Click += new System.EventHandler(this.BaseBoutonOk_Click);
            // 
            // BaseBoutonAnnuler
            // 
            this.BaseBoutonAnnuler.Location = new System.Drawing.Point(245, 189);
            this.BaseBoutonAnnuler.Name = "BaseBoutonAnnuler";
            this.BaseBoutonAnnuler.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365BlueDarkMode;
            this.BaseBoutonAnnuler.Size = new System.Drawing.Size(90, 25);
            this.BaseBoutonAnnuler.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.BaseBoutonAnnuler.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.BaseBoutonAnnuler.StateCommon.Border.Rounding = 20F;
            this.BaseBoutonAnnuler.StateCommon.Border.Width = 3;
            this.BaseBoutonAnnuler.TabIndex = 2;
            this.BaseBoutonAnnuler.Values.Text = "Annuler";
            this.BaseBoutonAnnuler.Click += new System.EventHandler(this.BaseBoutonAnnuler_Click);
            // 
            // ParametresDeBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(370, 232);
            this.Controls.Add(this.BaseBoutonAnnuler);
            this.Controls.Add(this.BaseBoutonOk);
            this.Controls.Add(this.GroupParametreDeBase);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ParametresDeBase";
            this.Text = "Parametres de base du moteur UCI";
            ((System.ComponentModel.ISupportInitialize)(this.GroupParametreDeBase.Panel)).EndInit();
            this.GroupParametreDeBase.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.GroupParametreDeBase)).EndInit();
            this.GroupParametreDeBase.ResumeLayout(false);
            this.ResumeLayout(false);

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