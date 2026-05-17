// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace BrunoGUI_GenII
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // *** Splash Screen ***
            EcranDemarrage Démarrage = new();
            Démarrage.Show();
            Démarrage.Demarrer();
            DateTime debut = DateTime.Now;
            while ((DateTime.Now - debut).TotalSeconds < 1)
            {   // Boucle d'événements temporaire pour permettre au splash de s'afficher
                Application.DoEvents(); // laisse le formulaire se peindre
            }
            // *** Fin du Splash ***
            // Créez une instance de KryptonManager (une seule fois)
            KryptonManager kryptonManagerInstance = new()
            {
                GlobalPaletteMode = (PaletteModeManager)PaletteMode.Office2010Silver
            };
            Application.Run(new EchiquierPrincipal());
        }
    }
}
