// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Etiquette sur une seule ligne (joueurs et pendules, en haut de l'échiquier)
//  └─ Classe "EtiquetteUneLigne" : un Label qui ne va jamais à la ligne
// Un Label haut de deux lignes coupe son texte entre les mots ("[1720] Courtois," / "Bruno") : illisible.
// Ici le texte reste sur une ligne, centré en hauteur, aligné à gauche, au centre ou à droite selon TextAlign,
// et se termine par "…" s'il est trop long (ou, avec ReduitPourTenir, s'écrit en police plus petite : pendules).

using System.Drawing;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public class EtiquetteUneLigne : Label
    {
        // true : si le texte ne tient pas, la police est réduite (jusqu'à 6 points) au lieu de couper le texte par "…"
        // (pendules : "1:30:00" doit toujours se lire en entier, quelle que soit la mise à l'échelle de Windows)
        [System.ComponentModel.DefaultValue(false)]
        public bool ReduitPourTenir { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            TextFormatFlags alignement = TextAlign switch
            {
                ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter => TextFormatFlags.HorizontalCenter,
                ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
                _ => TextFormatFlags.Left
            };
            Rectangle zone = ClientRectangle;
            zone = new Rectangle(zone.Left + Padding.Left, zone.Top + Padding.Top,
                                 zone.Width - Padding.Horizontal, zone.Height - Padding.Vertical);     // marges intérieures (Padding)
            TextFormatFlags format = alignement | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
            Font police = Font;
            if (ReduitPourTenir)
            {   // Police réduite par demi-points tant que le texte dépasse (une police créée ici est libérée après le dessin)
                float taille = Font.Size;
                while (taille > 6 && TextRenderer.MeasureText(e.Graphics, Text, police, zone.Size, format | TextFormatFlags.NoPadding).Width > zone.Width)
                {
                    taille -= 0.5f;
                    if (police != Font)
                        police.Dispose();
                    police = new Font(Font.FontFamily, taille, Font.Style);
                }
                format |= TextFormatFlags.NoPadding;
            }
            TextRenderer.DrawText(e.Graphics, Text, police, zone, ForeColor, format | TextFormatFlags.EndEllipsis);
            if (police != Font)
                police.Dispose();
        }
    }
}
