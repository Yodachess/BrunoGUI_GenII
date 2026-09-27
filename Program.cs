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
using Krypton.Toolkit;

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
            Parametres parametres = new();
            parametres.ChargerDepuisIni(System.IO.Path.Combine(Chemins.RepertoireRacine, "BrunoGUI.ini"));
            ConfigureKrypton(parametres.Palette);
            Application.Run(new EchiquierPrincipal());
        }

        private static void ConfigureKrypton(string palette)
        {   // Textes des boutons des boîtes de message en français (Krypton 95 les affiche en anglais par défaut)
            var textes = KryptonManager.Strings.GeneralStrings;
            textes.OK = "O&K";
            textes.Cancel = "&Annuler";
            textes.Yes = "&Oui";
            textes.No = "&Non";
            textes.Abort = "A&bandonner";
            textes.Retry = "&Réessayer";
            textes.Ignore = "&Ignorer";
            textes.Close = "&Fermer";
            textes.Today = "Au&jourd'hui";
            textes.Help = "Ai&de";
            textes.Continue = "&Continuer";
            textes.TryAgain = "Réessa&yer";

            // Palette de toute l'application (clé "Palette" de BrunoGUI.ini, ex : Microsoft365Silver) ;
            // absente ou inconnue : palette par défaut de Krypton 95 (Microsoft365Blue)
            if (string.IsNullOrWhiteSpace(palette))
                return;
            if (Enum.TryParse(palette.Trim(), true, out PaletteMode mode) && mode != PaletteMode.Custom && mode != PaletteMode.Global)
                _ = new KryptonManager { GlobalPaletteMode = mode };
            else
                System.Diagnostics.Debug.WriteLine($"[INFO] Palette inconnue dans BrunoGUI.ini : '{palette}' (palette par défaut conservée)");
        }
    }
}
