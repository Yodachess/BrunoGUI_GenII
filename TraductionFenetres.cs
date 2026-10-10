// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Traduction des textes posés dans le designer (menus, boutons, cadres, étiquettes) ...
//      └─ Classe statique "TraductionFenetres"
//                      └─ "Traduit"    Tous les textes d'une fenêtre et de ses contrôles, à l'ouverture (après InitializeComponent)
// Les fichiers *.Designer.cs restent en français : chaque texte est remplacé par sa traduction (Langue.T) quand la fenêtre est
// créée. Un texte sans traduction (ex : "-:--", ou un texte remplacé ensuite par le code) reste tel quel.

using System.Windows.Forms;
using Krypton.Toolkit;

namespace BrunoGUI_GenII
{
    public static class TraductionFenetres
    {
        public static void Traduit(Control controle)
        {   // La fenêtre (son titre) puis tous ses contrôles, récursivement ; rien à faire en français
            if (Langue.EstFrancais)
                return;
            TraduitControle(controle);
        }

        private static void TraduitControle(Control controle)
        {
            if (!string.IsNullOrWhiteSpace(controle.Text))
                controle.Text = Langue.T(controle.Text);
            switch (controle)
            {
                case KryptonGroupBox cadre:
                    cadre.Values.Heading = Langue.T(cadre.Values.Heading);
                    break;
                case ToolStrip barre:       // menus et barre d'état
                    TraduitElements(barre.Items);
                    break;
            }
            foreach (Control enfant in controle.Controls)
                TraduitControle(enfant);
        }

        private static void TraduitElements(ToolStripItemCollection elements)
        {
            foreach (ToolStripItem element in elements)
            {
                if (!string.IsNullOrWhiteSpace(element.Text))
                    element.Text = Langue.T(element.Text);
                if (!string.IsNullOrWhiteSpace(element.ToolTipText))
                    element.ToolTipText = Langue.T(element.ToolTipText);
                if (element is ToolStripDropDownItem menu)
                    TraduitElements(menu.DropDownItems);
            }
        }
    }
}
