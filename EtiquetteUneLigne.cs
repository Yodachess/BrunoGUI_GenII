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
// et se termine par "…" s'il est trop long.

using System.Drawing;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public class EtiquetteUneLigne : Label
    {
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
            TextRenderer.DrawText(e.Graphics, Text, Font, zone, ForeColor, alignement | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
