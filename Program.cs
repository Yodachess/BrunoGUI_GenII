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
            // Erreurs imprévues : notées dans le journal (BrunoGUI.log) ; sur le thread de l'interface, un message clair remplace la
            // boîte de plantage de .NET et l'application continue ; ailleurs (thread du moteur, tâches), .NET arrête l'application,
            // mais la cause reste dans le journal
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ErreurImprevue(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Journal.Erreur("Erreur imprévue hors de l'interface (l'application s'arrête)", e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()));
            TaskScheduler.UnobservedTaskException += (s, e) => { Journal.Erreur("Erreur dans une tâche", e.Exception); e.SetObserved(); };
            Journal.Info($"Démarrage de BrunoGUI GenII {EchiquierPrincipal.VersionAffichee}");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Préférences lues avant toute fenêtre : la langue (Options > Langue, voir Langue.cs) et la palette valent pour toutes
            Parametres parametres = Parametres.Charger(Chemins.RepertoireRacine);
            Langue.Choisir(parametres.Langue, Chemins.RepertoireRacine);
            ConfigureKrypton(parametres.Palette);
            // *** Splash Screen ***
            EcranDemarrage demarrage = new();
            demarrage.Show();
            demarrage.Demarrer();
            DateTime debut = DateTime.Now;
            while ((DateTime.Now - debut).TotalSeconds < 1)
            {   // Boucle d'événements temporaire pour permettre au splash de s'afficher
                Application.DoEvents(); // laisse le formulaire se peindre
            }
            // *** Fin du Splash ***
            Application.Run(new EchiquierPrincipal());
        }

        internal static void ErreurImprevue(Exception exception)
        {   // Erreur non prévue sur le thread de l'interface : notée, puis expliquée à l'utilisateur ; l'application continue
            // (aussi pour une action venue du thread du moteur : voir EchiquierPrincipal.SurLeThreadInterface)
            Journal.Erreur("Erreur imprévue", exception);
            try
            {
                KryptonMessageBox.Show(Langue.T("Une erreur imprévue s'est produite :\n{0}\n\nLe détail est enregistré dans le fichier BrunoGUI.log, à côté du programme.\nVous pouvez continuer ; si l'erreur se reproduit, enregistrez votre partie.", exception.Message),
                    Langue.T("Erreur"), KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Error);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ObjectDisposedException)
            {   // application en cours de fermeture : plus de fenêtre à montrer
                Journal.Erreur("Message d'erreur impossible à afficher", ex);
            }
        }

        private static void ConfigureKrypton(string palette)
        {   // Textes des boutons des boîtes de message en français (Krypton 95 les affiche en anglais par défaut : rien à faire en anglais)
            var textes = KryptonManager.Strings.GeneralStrings;
            if (Langue.EstFrancais)
            {
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
            }

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
