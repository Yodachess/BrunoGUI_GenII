namespace BrunoGUI_Stockfish
{
    partial class AffichePgn
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AffichePgn));
            this.ZoneAffichage = new Krypton.Toolkit.KryptonRichTextBox();
            this.AffichePgnIntl = new Krypton.Toolkit.KryptonButton();
            this.ListeNalAfficheNal = new Krypton.Toolkit.KryptonButton();
            this.ListeFenAffichePgn = new Krypton.Toolkit.KryptonButton();
            this.MasqueAffichePgn = new Krypton.Toolkit.KryptonButton();
            this.AffichePgnFr = new Krypton.Toolkit.KryptonButton();
            this.SuspendLayout();
            // 
            // ZoneAffichage
            // 
            this.ZoneAffichage.CueHint.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ZoneAffichage.CueHint.CueHintText = "Affichage de la partie en cours au format PGN";
            this.ZoneAffichage.Dock = System.Windows.Forms.DockStyle.Top;
            this.ZoneAffichage.Location = new System.Drawing.Point(0, 0);
            this.ZoneAffichage.Name = "ZoneAffichage";
            this.ZoneAffichage.Size = new System.Drawing.Size(529, 494);
            this.ZoneAffichage.StateCommon.Back.Color1 = System.Drawing.Color.LightSteelBlue;
            this.ZoneAffichage.StateCommon.Border.Color1 = System.Drawing.Color.DimGray;
            this.ZoneAffichage.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ZoneAffichage.StateCommon.Border.Width = 2;
            this.ZoneAffichage.StateCommon.Content.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ZoneAffichage.TabIndex = 0;
            this.ZoneAffichage.Text = "Zone Affichage de la partie";
            // 
            // AffichePgnIntl
            // 
            this.AffichePgnIntl.Location = new System.Drawing.Point(12, 500);
            this.AffichePgnIntl.Name = "AffichePgnIntl";
            this.AffichePgnIntl.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.AffichePgnIntl.Size = new System.Drawing.Size(80, 25);
            this.AffichePgnIntl.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.AffichePgnIntl.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.AffichePgnIntl.StateCommon.Border.Rounding = 20F;
            this.AffichePgnIntl.StateCommon.Border.Width = 3;
            this.AffichePgnIntl.TabIndex = 1;
            this.AffichePgnIntl.Values.Text = "PGN Intl.";
            this.AffichePgnIntl.Click += new System.EventHandler(this.AffichePgnIntl_Click);
            // 
            // ListeNalAfficheNal
            // 
            this.ListeNalAfficheNal.Location = new System.Drawing.Point(184, 500);
            this.ListeNalAfficheNal.Name = "ListeNalAfficheNal";
            this.ListeNalAfficheNal.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.ListeNalAfficheNal.Size = new System.Drawing.Size(110, 25);
            this.ListeNalAfficheNal.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.ListeNalAfficheNal.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ListeNalAfficheNal.StateCommon.Border.Rounding = 20F;
            this.ListeNalAfficheNal.StateCommon.Border.Width = 3;
            this.ListeNalAfficheNal.TabIndex = 2;
            this.ListeNalAfficheNal.Values.Text = "Algébrique ";
            this.ListeNalAfficheNal.Click += new System.EventHandler(this.ListeNalAfficheNal_Click);
            // 
            // ListeFenAffichePgn
            // 
            this.ListeFenAffichePgn.Location = new System.Drawing.Point(300, 500);
            this.ListeFenAffichePgn.Name = "ListeFenAffichePgn";
            this.ListeFenAffichePgn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.ListeFenAffichePgn.Size = new System.Drawing.Size(120, 25);
            this.ListeFenAffichePgn.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.ListeFenAffichePgn.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.ListeFenAffichePgn.StateCommon.Border.Rounding = 20F;
            this.ListeFenAffichePgn.StateCommon.Border.Width = 3;
            this.ListeFenAffichePgn.TabIndex = 3;
            this.ListeFenAffichePgn.Values.Text = "Liste FEN";
            this.ListeFenAffichePgn.Click += new System.EventHandler(this.ListeFenAffichePgn_Click);
            // 
            // MasqueAffichePgn
            // 
            this.MasqueAffichePgn.Location = new System.Drawing.Point(426, 500);
            this.MasqueAffichePgn.Name = "MasqueAffichePgn";
            this.MasqueAffichePgn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.MasqueAffichePgn.Size = new System.Drawing.Size(90, 25);
            this.MasqueAffichePgn.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.MasqueAffichePgn.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.MasqueAffichePgn.StateCommon.Border.Rounding = 20F;
            this.MasqueAffichePgn.StateCommon.Border.Width = 3;
            this.MasqueAffichePgn.TabIndex = 4;
            this.MasqueAffichePgn.Values.Text = "Femer";
            this.MasqueAffichePgn.Click += new System.EventHandler(this.MasqueAffichePgn_Click);
            // 
            // AffichePgnFr
            // 
            this.AffichePgnFr.Location = new System.Drawing.Point(98, 500);
            this.AffichePgnFr.Name = "AffichePgnFr";
            this.AffichePgnFr.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverLightMode;
            this.AffichePgnFr.Size = new System.Drawing.Size(80, 25);
            this.AffichePgnFr.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            this.AffichePgnFr.StateCommon.Border.DrawBorders = ((Krypton.Toolkit.PaletteDrawBorders)((((Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom) 
            | Krypton.Toolkit.PaletteDrawBorders.Left) 
            | Krypton.Toolkit.PaletteDrawBorders.Right)));
            this.AffichePgnFr.StateCommon.Border.Rounding = 20F;
            this.AffichePgnFr.StateCommon.Border.Width = 3;
            this.AffichePgnFr.TabIndex = 5;
            this.AffichePgnFr.Values.Text = "PGN Fr.";
            this.AffichePgnFr.Click += new System.EventHandler(this.AffichePgnFr_Click);
            // 
            // AffichePgn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 532);
            this.Controls.Add(this.AffichePgnFr);
            this.Controls.Add(this.MasqueAffichePgn);
            this.Controls.Add(this.ListeFenAffichePgn);
            this.Controls.Add(this.ListeNalAfficheNal);
            this.Controls.Add(this.AffichePgnIntl);
            this.Controls.Add(this.ZoneAffichage);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AffichePgn";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Affichage liste coups de la partie";
            this.ResumeLayout(false);

        }

        #endregion

        private Krypton.Toolkit.KryptonRichTextBox ZoneAffichage;
        private Krypton.Toolkit.KryptonButton AffichePgnIntl;
        private Krypton.Toolkit.KryptonButton ListeNalAfficheNal;
        private Krypton.Toolkit.KryptonButton ListeFenAffichePgn;
        private Krypton.Toolkit.KryptonButton MasqueAffichePgn;
        private Krypton.Toolkit.KryptonButton AffichePgnFr;
    }
}