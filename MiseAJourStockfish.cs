// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Mise à jour de Stockfish depuis GitHub (https://github.com/official-stockfish/Stockfish/releases)
//  └─ Classe "MiseAJourStockfish"
//              ├─ "RechercherNouvelleVersion"  compare la version installée à la dernière publiée (rien n'est arrêté ni téléchargé)
//              ├─ "Installer"                  télécharge, remplace stockfish.exe (sauvegarde .old, restaurée en cas d'échec)
//              └─ "ChoisirArchive"             archive Windows adaptée au processeur (testé)
// Depuis Stockfish 19, chaque publication ne contient qu'un binaire "universal" par processeur (x86-64, arm64) :
// il détecte lui-même les instructions disponibles (plus de choix avx2 / bmi2 à faire ici).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrunoGUI_GenII
{
    public class MiseAJourStockfish
    {
        private const string UrlDerniereVersion = "https://api.github.com/repos/official-stockfish/Stockfish/releases/latest";
        private static readonly HttpClient Client = CreerClient();
        private readonly string _cheminExe;         // stockfish.exe du dossier de sortie (à côté de BrunoGUI)
        private readonly string _cheminSauvegarde;  // l'ancienne version, gardée jusqu'à ce que la nouvelle ait démarré

        public record VersionStockfish(string Tag, string Url, long TailleOctets);  // ex : sf_19, lien du zip, taille du zip

        public MiseAJourStockfish(string cheminExe)
        {
            _cheminExe = Path.GetFullPath(cheminExe);
            _cheminSauvegarde = _cheminExe + ".old";
        }
        private static HttpClient CreerClient()
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "BrunoGUI_GenII");    // exigé par l'API GitHub
            return client;
        }

        public async Task<VersionStockfish?> RechercherNouvelleVersion()
        {   // Compare la version installée à la dernière version publiée sur GitHub, sans rien arrêter ni télécharger.
            // Retourne null si Stockfish est déjà à jour (exception en cas d'erreur, ex : pas de connexion)
            string versionLocale = await ObtenirVersionLocale();
            using JsonDocument document = JsonDocument.Parse(await Client.GetStringAsync(UrlDerniereVersion));
            JsonElement publication = document.RootElement;
            string tag = publication.GetProperty("tag_name").GetString() ?? "";
            Debug.WriteLine($"[MAJ] Installée : {versionLocale}, publiée : {tag}");
            if (string.Equals(tag, versionLocale, StringComparison.OrdinalIgnoreCase))
                return null;
            var archives = publication.GetProperty("assets").EnumerateArray()
                .Select(a => (Nom: a.GetProperty("name").GetString() ?? "", Url: a.GetProperty("browser_download_url").GetString() ?? "", Taille: a.GetProperty("size").GetInt64()))
                .ToList();
            string nomChoisi = ChoisirArchive(archives.Select(a => a.Nom), RuntimeInformation.OSArchitecture)
                ?? throw new Exception("Aucune archive Windows adaptée à ce processeur dans la publication " + tag + ".");
            var archive = archives.First(a => a.Nom == nomChoisi);
            return new VersionStockfish(tag, archive.Url, archive.Taille);
        }

        public static string? ChoisirArchive(IEnumerable<string> nomsArchives, Architecture processeur)
        {   // Archive Windows à télécharger : la version ARM sur un processeur ARM, sinon la version x86-64 ("universal" de préférence,
            // en attendant d'éventuelles anciennes publications aux noms plus détaillés) ; null si aucune ne convient
            List<string> windows = nomsArchives.Where(n => n.Contains("windows", StringComparison.OrdinalIgnoreCase)).ToList();
            string? Premiere(string morceau) =>
                windows.Where(n => n.Contains(morceau, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(n => n.Contains("universal", StringComparison.OrdinalIgnoreCase))
                       .FirstOrDefault();
            if (processeur == Architecture.Arm64 && Premiere("arm64") is string arm)
                return arm;
            return Premiere("x86-64");      // (sur ARM sans version ARM : la version x86-64, émulée par Windows)
        }

        public async Task Installer(VersionStockfish version)
        {   // Télécharge et installe la version trouvée par RechercherNouvelleVersion ; en cas d'échec, l'ancienne version est remise
            // et une exception est levée. ATTENTION : arrête le Stockfish lancé depuis ce fichier (le moteur doit être redémarré ensuite)
            string zip = Path.Combine(Path.GetTempPath(), "brunogui_stockfish.zip");
            string nouvelExe = Path.Combine(Path.GetTempPath(), "brunogui_stockfish_nouveau.exe");
            try
            {
                await File.WriteAllBytesAsync(zip, await Client.GetByteArrayAsync(version.Url));
                ExtraireExe(zip, nouvelExe);
                await Task.Run(() =>
                {   // (hors du thread de l'interface : les nouvelles tentatives ne la figent pas)
                    ArreterProcessusStockfish();    // seulement après le téléchargement : le moteur reste disponible pendant ce temps
                    if (File.Exists(_cheminExe))
                        Reessayer(() => File.Move(_cheminExe, _cheminSauvegarde, overwrite: true));
                    Reessayer(() => File.Copy(nouvelExe, _cheminExe, overwrite: true));
                });
                if (!await DemarreCorrectement())
                    throw new Exception("La nouvelle version de Stockfish ne démarre pas.");
                Supprimer(_cheminSauvegarde);
            }
            catch
            {   // Echec (téléchargement, fichier verrouillé, nouvelle version qui ne démarre pas) : l'ancienne version est remise
                if (File.Exists(_cheminSauvegarde))
                    Reessayer(() => File.Copy(_cheminSauvegarde, _cheminExe, overwrite: true));
                throw;
            }
            finally
            {
                Supprimer(zip);
                Supprimer(nouvelExe);
            }
        }

        private async Task<string> ObtenirVersionLocale()
        {   // "sf_19" d'après la ligne "id name Stockfish 19" ; "sf_0" si Stockfish est absent, "sf_inconnue" s'il ne répond pas
            if (!File.Exists(_cheminExe))
                return "sf_0";
            string sortie = await LireSortieUci(_cheminExe);
            string? ligne = sortie.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith("id name Stockfish"));
            return ligne == null ? "sf_inconnue" : "sf_" + ligne.Split(' ').Last();
        }
        private async Task<bool> DemarreCorrectement() => (await LireSortieUci(_cheminExe)).Contains("uciok");

        private static async Task<string> LireSortieUci(string exe)
        {   // Lance "stockfish uci" (il répond par son nom, ses options, puis "uciok" et s'arrête) ; 5 secondes au plus
            ProcessStartInfo infos = new(exe, "uci")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(exe)
            };
            using Process? processus = Process.Start(infos);
            if (processus == null)
                return "";
            Task<string> lecture = processus.StandardOutput.ReadToEndAsync();
            if (await Task.WhenAny(lecture, Task.Delay(5000)) != lecture)
            {   // pas de réponse : on n'attend pas plus
                try { processus.Kill(); } catch (InvalidOperationException) { }
                return "";
            }
            return await lecture;
        }

        private static void ExtraireExe(string zip, string destination)
        {   // Le zip contient un dossier "stockfish" avec l'exe, les sources et la documentation : on ne garde que l'exe
            using ZipArchive archive = ZipFile.OpenRead(zip);
            ZipArchiveEntry exe = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                ?? throw new Exception("Aucun fichier .exe dans l'archive téléchargée.");
            exe.ExtractToFile(destination, overwrite: true);
        }

        private void ArreterProcessusStockfish()
        {   // Arrête seulement le Stockfish lancé depuis CE fichier (pas celui d'un autre logiciel), pour pouvoir le remplacer
            foreach (Process processus in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_cheminExe)))
            {
                try
                {
                    if (string.Equals(processus.MainModule?.FileName, _cheminExe, StringComparison.OrdinalIgnoreCase))
                    {
                        processus.Kill();
                        processus.WaitForExit(3000);
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                {   // processus déjà terminé, ou d'un autre utilisateur
                    Debug.WriteLine("[MAJ] Arrêt de Stockfish : " + ex.Message);
                }
                finally
                {
                    processus.Dispose();
                }
            }
        }

        private static void Reessayer(Action operationFichier)
        {   // Le fichier peut rester verrouillé quelques instants (antivirus, synchronisation OneDrive...) : 5 essais, 1 s d'écart
            for (int essai = 1; ; essai++)
            {
                try
                {
                    operationFichier();
                    return;
                }
                catch (IOException) when (essai < 5)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
        private static void Supprimer(string fichier)
        {
            try
            {
                if (File.Exists(fichier))
                    File.Delete(fichier);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {   // fichier temporaire ou sauvegarde : sans importance s'il reste
                Debug.WriteLine("[MAJ] Non supprimé : " + fichier);
            }
        }
    }
}
