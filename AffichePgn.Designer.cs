// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

namespace BrunoGUI_GenII
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
            ZoneAffichage = new Krypton.Toolkit.KryptonRichTextBox();
            AffichePgnIntl = new Krypton.Toolkit.KryptonButton();
            ListeNalAfficheNal = new Krypton.Toolkit.KryptonButton();
            ListeFenAffichePgn = new Krypton.Toolkit.KryptonButton();
            MasqueAffichePgn = new Krypton.Toolkit.KryptonButton();
            AffichePgnFr = new Krypton.Toolkit.KryptonButton();
            AfficheCoupsUci = new Krypton.Toolkit.KryptonButton();
            SuspendLayout();
            // 
            // ZoneAffichage
            // 
            ZoneAffichage.CueHint.Color1 = System.Drawing.Color.FromArgb(64, 64, 64);
            ZoneAffichage.CueHint.CueHintText = "Affichage de la partie en cours au format PGN";
            ZoneAffichage.Dock = System.Windows.Forms.DockStyle.Top;
            ZoneAffichage.Location = new System.Drawing.Point(0, 0);
            ZoneAffichage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ZoneAffichage.Name = "ZoneAffichage";
            ZoneAffichage.Size = new System.Drawing.Size(764, 570);
            ZoneAffichage.StateCommon.Back.Color1 = System.Drawing.Color.LightGray;
            ZoneAffichage.StateCommon.Border.Color1 = System.Drawing.Color.DimGray;
            ZoneAffichage.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ZoneAffichage.StateCommon.Border.Width = 2;
            ZoneAffichage.StateCommon.Content.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ZoneAffichage.TabIndex = 0;
            ZoneAffichage.Text = "Zone Affichage de la partie";
            // 
            // AffichePgnIntl
            // 
            AffichePgnIntl.Location = new System.Drawing.Point(11, 576);
            AffichePgnIntl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AffichePgnIntl.Name = "AffichePgnIntl";
            AffichePgnIntl.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            AffichePgnIntl.Size = new System.Drawing.Size(90, 29);
            AffichePgnIntl.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            AffichePgnIntl.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            AffichePgnIntl.StateCommon.Border.Rounding = 20F;
            AffichePgnIntl.StateCommon.Border.Width = 3;
            AffichePgnIntl.TabIndex = 1;
            AffichePgnIntl.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            AffichePgnIntl.Values.Text = "PGN Intl.";
            AffichePgnIntl.Click += AffichePgnIntl_Click;
            // 
            // ListeNalAfficheNal
            // 
            ListeNalAfficheNal.Location = new System.Drawing.Point(207, 577);
            ListeNalAfficheNal.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ListeNalAfficheNal.Name = "ListeNalAfficheNal";
            ListeNalAfficheNal.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            ListeNalAfficheNal.Size = new System.Drawing.Size(90, 29);
            ListeNalAfficheNal.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            ListeNalAfficheNal.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ListeNalAfficheNal.StateCommon.Border.Rounding = 20F;
            ListeNalAfficheNal.StateCommon.Border.Width = 3;
            ListeNalAfficheNal.TabIndex = 2;
            ListeNalAfficheNal.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            ListeNalAfficheNal.Values.Text = "Algébrique ";
            ListeNalAfficheNal.Click += ListeNalAfficheNal_Click;
            // 
            // ListeFenAffichePgn
            // 
            ListeFenAffichePgn.Location = new System.Drawing.Point(403, 576);
            ListeFenAffichePgn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ListeFenAffichePgn.Name = "ListeFenAffichePgn";
            ListeFenAffichePgn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            ListeFenAffichePgn.Size = new System.Drawing.Size(90, 29);
            ListeFenAffichePgn.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            ListeFenAffichePgn.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            ListeFenAffichePgn.StateCommon.Border.Rounding = 20F;
            ListeFenAffichePgn.StateCommon.Border.Width = 3;
            ListeFenAffichePgn.TabIndex = 3;
            ListeFenAffichePgn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            ListeFenAffichePgn.Values.Text = "Liste FEN";
            ListeFenAffichePgn.Click += ListeFenAffichePgn_Click;
            // 
            // MasqueAffichePgn
            // 
            MasqueAffichePgn.Location = new System.Drawing.Point(661, 576);
            MasqueAffichePgn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MasqueAffichePgn.Name = "MasqueAffichePgn";
            MasqueAffichePgn.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            MasqueAffichePgn.Size = new System.Drawing.Size(90, 29);
            MasqueAffichePgn.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            MasqueAffichePgn.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            MasqueAffichePgn.StateCommon.Border.Rounding = 20F;
            MasqueAffichePgn.StateCommon.Border.Width = 3;
            MasqueAffichePgn.TabIndex = 4;
            MasqueAffichePgn.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            MasqueAffichePgn.Values.Text = "Femer";
            MasqueAffichePgn.Click += MasqueAffichePgn_Click;
            // 
            // AffichePgnFr
            // 
            AffichePgnFr.Location = new System.Drawing.Point(109, 577);
            AffichePgnFr.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AffichePgnFr.Name = "AffichePgnFr";
            AffichePgnFr.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            AffichePgnFr.Size = new System.Drawing.Size(90, 29);
            AffichePgnFr.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            AffichePgnFr.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            AffichePgnFr.StateCommon.Border.Rounding = 20F;
            AffichePgnFr.StateCommon.Border.Width = 3;
            AffichePgnFr.TabIndex = 5;
            AffichePgnFr.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            AffichePgnFr.Values.Text = "PGN Fr.";
            AffichePgnFr.Click += AffichePgnFr_Click;
            // 
            // AfficheCoupsUci
            // 
            AfficheCoupsUci.Location = new System.Drawing.Point(305, 577);
            AfficheCoupsUci.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AfficheCoupsUci.Name = "AfficheCoupsUci";
            AfficheCoupsUci.PaletteMode = Krypton.Toolkit.PaletteMode.Microsoft365SilverDarkMode;
            AfficheCoupsUci.Size = new System.Drawing.Size(90, 29);
            AfficheCoupsUci.StateCommon.Border.Color1 = System.Drawing.Color.Gray;
            AfficheCoupsUci.StateCommon.Border.DrawBorders = Krypton.Toolkit.PaletteDrawBorders.Top | Krypton.Toolkit.PaletteDrawBorders.Bottom | Krypton.Toolkit.PaletteDrawBorders.Left | Krypton.Toolkit.PaletteDrawBorders.Right;
            AfficheCoupsUci.StateCommon.Border.Rounding = 20F;
            AfficheCoupsUci.StateCommon.Border.Width = 3;
            AfficheCoupsUci.TabIndex = 6;
            AfficheCoupsUci.Values.DropDownArrowColor = System.Drawing.Color.Empty;
            AfficheCoupsUci.Values.Text = "Liste UCI";
            AfficheCoupsUci.Click += AfficheCoupsUci_Click;
            // 
            // AffichePgn
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(764, 611);
            Controls.Add(AfficheCoupsUci);
            Controls.Add(AffichePgnFr);
            Controls.Add(MasqueAffichePgn);
            Controls.Add(ListeFenAffichePgn);
            Controls.Add(ListeNalAfficheNal);
            Controls.Add(AffichePgnIntl);
            Controls.Add(ZoneAffichage);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AffichePgn";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Affichage liste coups de la partie";
            ResumeLayout(false);

        }

        #endregion

        private Krypton.Toolkit.KryptonRichTextBox ZoneAffichage;
        private Krypton.Toolkit.KryptonButton AffichePgnIntl;
        private Krypton.Toolkit.KryptonButton ListeNalAfficheNal;
        private Krypton.Toolkit.KryptonButton ListeFenAffichePgn;
        private Krypton.Toolkit.KryptonButton MasqueAffichePgn;
        private Krypton.Toolkit.KryptonButton AffichePgnFr;
        private Krypton.Toolkit.KryptonButton AfficheCoupsUci;
    }
}