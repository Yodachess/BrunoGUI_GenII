// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Journal des erreurs, sans interface graphique (testé dans Tests/Program.cs)
// └─ Classe statique "Journal" : fichier texte BrunoGUI.log à côté de l'exécutable
//              ├─ "Erreur"   une exception, avec son contexte (ce que faisait le programme) et sa pile d'appels
//              └─ "Info"     un fait utile au diagnostic (ex : préférences non enregistrées)
// Ecrire dans le journal ne lève jamais d'exception (un journal qui planterait l'application serait pire que pas de journal) ;
// au-delà de TailleMaximale, le fichier est renommé en BrunoGUI.log.old (un seul ancien journal est gardé).

using System;
using System.Diagnostics;
using System.IO;

namespace BrunoGUI_GenII
{
    public static class Journal
    {
        public const long TailleMaximale = 1024 * 1024;     // 1 Mo
        private static readonly object _verrou = new();     // le moteur écrit depuis son propre thread

        // Fichier du journal : à côté de l'exécutable (déploiement portable, comme les .ini) ; modifiable pour les tests
        public static string Chemin { get; set; } = Path.Combine(AppContext.BaseDirectory, "BrunoGUI.log");

        public static void Erreur(string contexte, Exception exception) =>
            Ecrire("ERREUR", $"{contexte} : {exception.GetType().Name} : {exception.Message}{Environment.NewLine}{exception.StackTrace}");

        public static void Info(string message) => Ecrire("INFO", message);

        private static void Ecrire(string niveau, string texte)
        {
            string ligne = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{niveau}] {texte}";
            Debug.WriteLine(ligne);
            try
            {
                lock (_verrou)
                {
                    FileInfo fichier = new(Chemin);
                    if (fichier.Exists && fichier.Length > TailleMaximale)
                        File.Move(Chemin, Chemin + ".old", overwrite: true);
                    File.AppendAllText(Chemin, ligne + Environment.NewLine);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {   // dossier protégé en écriture, disque plein... : le journal se tait, l'application continue
                Debug.WriteLine("[Journal] Non écrit : " + ex.Message);
            }
        }
    }
}
