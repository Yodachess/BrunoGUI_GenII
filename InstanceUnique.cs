// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Un seul BrunoGUI ouvert par dossier, sans interface graphique (testé dans Tests/Scenario.cs).
// Deux fenêtres lancées du même dossier partagent ses préférences (la dernière fermée impose les siennes) et son stockfish.exe
// (sa mise à jour arrête les moteurs de l'autre). Deux installations dans des dossiers différents ne se gênent pas : elles restent permises.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace BrunoGUI_GenII
{
    public sealed class InstanceUnique : IDisposable
    {
        private Mutex? _verrou;
        public bool Seule { get; }      // false : un BrunoGUI est déjà ouvert depuis ce dossier

        public InstanceUnique(string dossier)
        {
            try
            {
                _verrou = new Mutex(true, NomDuVerrou(dossier), out bool seule);
                Seule = seule;
            }
            catch (UnauthorizedAccessException)
            {   // verrou créé par un BrunoGUI lancé en administrateur : il est donc ouvert
                Seule = false;
            }
        }

        public void Dispose()
        {   // Libère le dossier (à appeler sur le thread qui a créé l'objet) ; sans effet la deuxième fois
            if (Seule)
                _verrou?.ReleaseMutex();
            _verrou?.Dispose();
            _verrou = null;
        }

        private static string NomDuVerrou(string dossier)
        {   // Un nom de verrou ne peut pas contenir de « \ » : empreinte du chemin, sans tenir compte de la casse
            string chemin = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dossier)).ToUpperInvariant();
            return "BrunoGUI_GenII_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(chemin)));
        }
    }
}
