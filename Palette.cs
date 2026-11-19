// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Palette de l'interface : « ardoise et bleu acier » (2026-11-19). Chaque couleur a un rôle ; les fenêtres prennent leurs
// couleurs ici (EchiquierPrincipal.AppliquePalette) au lieu de les écrire chacune à sa façon. Changer une couleur ici la
// change partout. Restent à part, car elles ont un sens : couleurs des annotations, de la courbe d'évaluation, des flèches,
// de la pendule (vert : le camp qui réfléchit, rouge : moins de 10 s), Blancs sur blanc et Noirs sur noir.

using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace BrunoGUI_GenII
{
    public static class Palette
    {
        public static void Applique(Control fenetre)
        {   // Couleurs communes d'une fenêtre : tous ses boutons Krypton (les autres couleurs propres à la fenêtre sont posées par elle)
            foreach (Control controle in fenetre.Controls)
            {
                if (controle is KryptonButton bouton)
                    ColoreBouton(bouton, Bouton);
                Applique(controle);
            }
        }
        public static void ColoreBouton(KryptonButton bouton, Color fond)
        {
            bouton.StateCommon.Back.Color1 = fond;
            bouton.StateCommon.Border.Color1 = BordBouton;
            bouton.StateCommon.Content.ShortText.Color1 = bouton.StateCommon.Content.ShortText.Color2 = TexteBouton;
        }

        // Textes
        public static readonly Color Texte = Color.FromArgb(43, 48, 56);              // anthracite : texte principal
        public static readonly Color TexteSecondaire = Color.FromArgb(95, 105, 120);  // ardoise : conseil, détails
        public static readonly Color Accent = Color.FromArgb(31, 78, 128);            // bleu acier foncé : moteur, suite, informations

        // Fonds
        public static readonly Color FondInfo = Color.FromArgb(232, 235, 239);        // zones d'information (lignes, cadres, tournoi)
        public static readonly Color FondAccent = Color.FromArgb(212, 223, 236);      // zone mise en avant (message au joueur, promotion)
        public static readonly Color FondParcours = Color.FromArgb(160, 172, 188);    // cadre des boutons de parcours

        // Boutons
        public static readonly Color Bouton = Color.FromArgb(203, 216, 231);          // bleu acier clair
        public static readonly Color BoutonParcours = Color.FromArgb(166, 188, 214);  // un ton plus soutenu, même famille
        public static readonly Color BordBouton = Color.FromArgb(118, 136, 160);
        public static readonly Color TexteBouton = Color.FromArgb(25, 30, 38);
    }
}
