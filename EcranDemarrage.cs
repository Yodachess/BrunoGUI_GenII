// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Windows.Forms;
using System.Drawing;

namespace BrunoGUI_GenII
{
    public partial class EcranDemarrage : Form
    {
        Timer horlogeDeFondu = new();       // Gère l'effet fondu (fade)
        Timer horlogeDeProgression = new();   // Gère la progression de la barre
        bool disparition = false;            // Indique si on est en train de disparaÃ®tre
        ProgressBar barreProgression;
        int valeurProgression = 0;

        public EcranDemarrage()
        {   // Style général de la fenètre
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Opacity = 0; // Démarre invisible
            this.ClientSize = new Size(400, 250);
            this.TopMost = true; // Toujours au-dessus
            // Logo
            this.BackgroundImage = Image.FromFile(System.IO.Path.Combine(Chemins.RepertoireRacine, "Bruno_NB.jpg"));   // à côté de l'exécutable
            this.BackgroundImageLayout = ImageLayout.Stretch;
            // Barre de progression
            barreProgression = new ProgressBar()
            {
                Style = ProgressBarStyle.Continuous,
                Dock = DockStyle.Bottom,
                Height = 20,
                ForeColor = Color.LimeGreen
            };
            this.Controls.Add(barreProgression);
            // Configuration des timers
            horlogeDeFondu.Interval = 30;               // Vitesse du fondu
            horlogeDeFondu.Tick += HorlogeDeFondu_Tick;
            horlogeDeProgression.Interval = 50;           // Vitesse de progression
            horlogeDeProgression.Tick += HorlogeDeProgression_Tick;
        }
        private void HorlogeDeFondu_Tick(object sender, EventArgs e)
        {
            if (!disparition)
            {   // Effet de fondu entrant
                if (this.Opacity < 1)
                    this.Opacity += 0.05;
                else
                    horlogeDeProgression.Start(); // Lance la progression quand visible
            }
            else
            {   // Effet de fondu sortant
                if (this.Opacity > 0)
                    this.Opacity -= 0.05;
                else
                {
                    horlogeDeFondu.Stop();
                    this.Close();
                }
            }
        }
        private void HorlogeDeProgression_Tick(object sender, EventArgs e)
        {
            valeurProgression += 2;
            if (valeurProgression <= 100)
                barreProgression.Value = valeurProgression;
            else
            {
                horlogeDeProgression.Stop();
                disparition = true; // Quand fini â†’ lance la disparition
            }
        }
        public void Demarrer()
        {   // Démarre le splash screen (fondu + progression)
            horlogeDeFondu.Start();
        }
        protected override void OnPaint(PaintEventArgs e)
        {   // Dessine le texte au-dessus du logo
            base.OnPaint(e);
            using Font font = new("Segoe UI", 24, FontStyle.Bold);
            using Brush brush = new SolidBrush(Color.White);
            StringFormat sf = new()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near  // haut
            };
            e.Graphics.DrawString("BrunoGUI_GenII", font, brush, this.ClientRectangle, sf);
        }
    }
}
