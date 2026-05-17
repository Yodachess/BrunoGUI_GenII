// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using BrunoGUI_GenII;

public class MiseAJourStockfish
{
    private readonly string _cheminComplet;
    private readonly string _repertoire;
    private readonly string _repertoireSauvegarde;
    private static readonly HttpClient client = new();

    public MiseAJourStockfish(string cheminMoteurOuDossier)
    {   // Si le chemin passé est un dossier, on ajoute "stockfish.exe" au bout
        if (Directory.Exists(cheminMoteurOuDossier))
        {
            _cheminComplet = Path.Combine(cheminMoteurOuDossier, "stockfish.exe");
        }
        else
        {
            _cheminComplet = Path.GetFullPath(cheminMoteurOuDossier);
        }
        _repertoire = Path.GetDirectoryName(_cheminComplet);
        _repertoireSauvegarde = _cheminComplet + ".old";
        Debug.WriteLine($"[MAJ] Cible corrigée: {_cheminComplet}");
        if (!client.DefaultRequestHeaders.Contains("User-Agent"))
            client.DefaultRequestHeaders.Add("User-Agent", "Stockfish-Updater-CSharp");
    }
    public async Task ExecuterMiseAJour()
    {
        bool miseAJourReussie = false;
        Debug.WriteLine("[MAJ] Début du processus de mise à jour.");
        try
        {   // 1. ARRÊT PRÉVENTIF (On évite le verrouillage avant même de commencer)
            Debug.WriteLine("[MAJ] Étape 0: Arrêt préventif des instances de Stockfish...");
            ArreterProcessusStockfish();
            await Task.Delay(1000); // Pause pour laisser l'OS respirer

            // 2. VÉRIFICATION VERSION
            Debug.WriteLine("[MAJ] Étape 1: Récupération de la version locale...");
            string versionLocale = await ObtenirVersionLocale();
            Debug.WriteLine($"[MAJ] Version locale détectée: {versionLocale}");

            var (newTag, downloadUrl) = await VerifierNouvelleVersionGitHub(versionLocale);
            if (newTag == null)
            {
                Debug.WriteLine("[MAJ] Aucun nouveau tag trouvé. Fin.");
                throw new Exception("Vous avez déjà la dernière version.");
            }
            Debug.WriteLine($"[MAJ] Nouvelle version disponible: {newTag}");

            // 3. TÉLÉCHARGEMENT
            Debug.WriteLine("[MAJ] Étape 2: Téléchargement du ZIP...");
            string tempZip = Path.Combine(Path.GetTempPath(), "sf_update.zip");
            await TelechargerFichier(downloadUrl, tempZip);
            Debug.WriteLine($"[MAJ] ZIP téléchargé dans: {tempZip}");

            // 4. EXTRACTION TEMP
            Debug.WriteLine("[MAJ] Étape 3: Extraction vers le dossier Temp...");
            string tempExe = Path.Combine(Path.GetTempPath(), "stockfish_new.exe");
            ExtraireVersTemp(tempZip, tempExe);
            Debug.WriteLine($"[MAJ] EXE extrait dans: {tempExe}");

            // 5. REMPLACEMENT (Le moment où ça cassait avant)
            Debug.WriteLine("[MAJ] Étape 4: Nettoyage final avant remplacement...");

            // On retue le processus (car ObtenirVersionLocale a pu en relancer un !)
            ArreterProcessusStockfish();
            Debug.WriteLine("[MAJ] Pause de 1500ms pour libération finale des handles...");
            await Task.Delay(1500);

            // Diagnostic de verrouillage JUSTE avant l'écriture
            Debug.WriteLine("[MAJ] Étape 5: Vérification ultime du verrouillage...");
            VerifierVerrouillageFichier(_cheminComplet);

            // Droits et Backup
            Debug.WriteLine("[MAJ] Étape 6: Forçage des accès et création du backup...");
            ForcerAccesFichier(_cheminComplet);

            if (File.Exists(_repertoireSauvegarde))
            {
                Debug.WriteLine("[MAJ] Nettoyage de l'ancien backup...");
                ForcerAccesFichier(_repertoireSauvegarde);
                File.Delete(_repertoireSauvegarde);
            }

            if (File.Exists(_cheminComplet))
            {
                Debug.WriteLine("[MAJ] Déplacement du fichier actuel vers .old...");
                // File.Move est souvent plus efficace que CopierAvecRetry pour libérer le nom de fichier
                File.Move(_cheminComplet, _repertoireSauvegarde);
                Debug.WriteLine("[MAJ] Backup (.old) créé avec succès.");
            }

            // Installation
            Debug.WriteLine("[MAJ] Étape 7: Copie du nouveau binaire vers la cible...");
            CopierAvecRetry(tempExe, _cheminComplet, true);
            Debug.WriteLine("[MAJ] Nouveau binaire installé.");

            // 6. SANTÉ
            Debug.WriteLine("[MAJ] Étape 8: Vérification de santé (UCI)...");
            if (await VerifierSanteMoteur())
            {
                Debug.WriteLine("[MAJ] Santé OK. Nettoyage final...");
                miseAJourReussie = true;
                try
                {
                    if (File.Exists(tempZip)) File.Delete(tempZip);
                    if (File.Exists(tempExe)) File.Delete(tempExe);
                    if (File.Exists(_repertoireSauvegarde)) File.Delete(_repertoireSauvegarde);
                }
                catch { /* Optionnel */ }
                Debug.WriteLine("[MAJ] Mise à jour terminée avec succès !");
            }
            else
            {
                Debug.WriteLine("[MAJ] ERREUR: Le moteur ne répond pas à UCI.");
                throw new Exception("Le nouveau moteur ne démarre pas (Erreur UCI).");
            }
        }
        catch (Exception ex)
        {   // Si le flag est à vrai, on ignore l'erreur de communication car le fichier est déjà remplacé
            if (miseAJourReussie)
            {
                Debug.WriteLine($"[MAJ] Info : Erreur de flux post-installation ignorée : {ex.Message}");
            }
            else
            {
                Debug.WriteLine($"[MAJ] CRASH DURANT L'INSTALLATION: {ex.Message}");
                RestaurerBackup();
                throw;
            }
        }
    }
    private static void VerifierVerrouillageFichier(string chemin)
    {
        if (!File.Exists(chemin))
        {
            Debug.WriteLine($"[DEBUG] VerifierVerrouillage: Le fichier n'existe pas encore ({chemin}).");
            return;
        }
        try
        {
            using (FileStream stream = new(chemin, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Debug.WriteLine($"[DEBUG] VerifierVerrouillage: Succès, le fichier {Path.GetFileName(chemin)} est LIBRE.");
            }
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"[DEBUG] VerifierVerrouillage: ÉCHEC ! Le fichier est VERROUILLÉ. \nDétail: {ex.Message}");
            throw new Exception($"Le fichier {Path.GetFileName(chemin)} est verrouillé par un autre processus.");
        }
    }

    private static void ForcerAccesFichier(string chemin)
    {
        if (!File.Exists(chemin)) return;
        try
        {
            Debug.WriteLine($"[DEBUG] ForcerAcces: Normalisation des attributs pour {chemin}");
            File.SetAttributes(chemin, FileAttributes.Normal);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DEBUG] ForcerAcces: Impossible de changer les attributs: {ex.Message}");
        }
    }

    private static void CopierAvecRetry(string source, string dest, bool overwrite)
    {
        Debug.WriteLine($"[DEBUG] CopierAvecRetry: {Path.GetFileName(source)} -> {Path.GetFileName(dest)}");
        if (Directory.Exists(dest))
        {
            Debug.WriteLine("[DEBUG] CopierAvecRetry: La destination est un répertoire ! Suppression...");
            Directory.Delete(dest, true);
        }

        int attempts = 5;
        while (attempts > 0)
        {
            try
            {
                if (File.Exists(dest)) File.SetAttributes(dest, FileAttributes.Normal);
                File.Copy(source, dest, overwrite);
                return;
            }
            catch (IOException ex)
            {
                attempts--;
                Debug.WriteLine($"[DEBUG] CopierAvecRetry: Échec (Tentatives restantes: {attempts}). Erreur: {ex.Message}");
                if (attempts == 0) throw;
                Thread.Sleep(2000);
            }
        }
    }

    public async Task<string> ObtenirVersionLocale()
    {
        Debug.WriteLine($"[DEBUG] ObtenirVersionLocale: Vérification du fichier à {_cheminComplet}");

        if (!File.Exists(_cheminComplet))
        {
            Debug.WriteLine("[DEBUG] ObtenirVersionLocale: Fichier introuvable.");
            return "sf_0";
        }

        try
        {
            var info = new ProcessStartInfo
            {
                FileName = _cheminComplet,
                Arguments = "uci",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Debug.WriteLine("[DEBUG] ObtenirVersionLocale: Démarrage du processus Stockfish...");
            using var p = Process.Start(info);

            if (p == null)
            {
                Debug.WriteLine("[DEBUG] ObtenirVersionLocale: Échec du démarrage du processus (p est null).");
                return "sf_unknown";
            }

            // On lit plusieurs lignes car l'ID n'est pas forcément sur la première
            string line;
            while ((line = await p.StandardOutput.ReadLineAsync()) != null)
            {
                Debug.WriteLine($"[DEBUG] Stockfish Output: {line}");

                if (line.StartsWith("id name Stockfish"))
                {
                    // Extrait juste le nombre à la fin, ex: "id name Stockfish 17" -> "17"
                    var versionNumber = line.Split(' ').LastOrDefault();
                    Debug.WriteLine($"[DEBUG] ObtenirVersionLocale: Version trouvée : {versionNumber}");

                    p.Kill();
                    return "sf_" + versionNumber;
                }
            }

            Debug.WriteLine("[DEBUG] ObtenirVersionLocale: Fin de la sortie sans trouver 'id name Stockfish'.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DEBUG] ObtenirVersionLocale: Erreur critique: {ex.Message}");
            Debug.WriteLine($"[DEBUG] StackTrace: {ex.StackTrace}");
        }

        return "sf_unknown";
    }
    private static void ExtraireVersTemp(string cheminZip, string cheminExeCible)
    {
        using (ZipArchive archive = ZipFile.OpenRead(cheminZip))
        {
            var exeEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            if (exeEntry == null) throw new Exception("EXE non trouvé dans le ZIP.");

            if (File.Exists(cheminExeCible))
            {
                File.SetAttributes(cheminExeCible, FileAttributes.Normal);
                File.Delete(cheminExeCible);
            }
            exeEntry.ExtractToFile(cheminExeCible);
        }
    }

    private void ArreterProcessusStockfish()
    {
        string name = Path.GetFileNameWithoutExtension(_cheminComplet);
        Debug.WriteLine($"[DEBUG] ArreterProcessus: Recherche de '{name}'...");
        foreach (var p in Process.GetProcessesByName(name))
        {
            try
            {
                Debug.WriteLine($"[DEBUG] ArreterProcessus: Kill du processus ID {p.Id}");
                p.Kill();
                p.WaitForExit(2000); // CRUCIAL : attend que Windows libère le fichier
                if (!p.WaitForExit(3000))
                {
                    Debug.WriteLine("[DEBUG] ArreterProcessus: Le processus résiste, appel à TaskKill...");
                    Process.Start(new ProcessStartInfo("taskkill", $"/F /IM {name}.exe /T") { CreateNoWindow = true });
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[DEBUG] ArreterProcessus: Exception: {ex.Message}"); }
        }
    }

    private void RestaurerBackup()
    {
        if (File.Exists(_repertoireSauvegarde))
        {
            try
            {
                Debug.WriteLine("[DEBUG] RestaurerBackup: Tentative de restauration du fichier .old...");
                ForcerAccesFichier(_cheminComplet);
                File.Copy(_repertoireSauvegarde, _cheminComplet, true);
            }
            catch (Exception ex) { Debug.WriteLine($"[DEBUG] RestaurerBackup: ÉCHEC: {ex.Message}"); }
        }
    }

    private async Task<bool> VerifierSanteMoteur()
    {
        try
        {
            var info = new ProcessStartInfo
            {
                FileName = _cheminComplet,
                Arguments = "uci",
                RedirectStandardOutput = true,
                RedirectStandardInput = true, // Ajouté pour pouvoir envoyer "quit" proprement
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _repertoire
            };

            using var p = Process.Start(info);
            if (p == null) return false;

            // On lance la lecture de la première ligne
            var readTask = p.StandardOutput.ReadLineAsync();

            // On attend soit la réponse, soit un timeout de 5 secondes
            if (await Task.WhenAny(readTask, Task.Delay(5000)) == readTask)
            {
                string line = await readTask;
                Debug.WriteLine($"[DEBUG] VerifierSante: Réponse UCI reçue: {line}");

                // --- FERMETURE PROPRE POUR ÉVITER LE CRASH DU CANAL ---
                try
                {
                    // On tente de dire gentiment à Stockfish de s'arrêter
                    await p.StandardInput.WriteLineAsync("quit");
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DEBUG] VerifierSante: Erreur lors du quit (ignorée): {ex.Message}");
                }

                if (!p.HasExited) p.Kill();

                return line != null && line.Contains("Stockfish");
            }

            Debug.WriteLine("[DEBUG] VerifierSante: Timeout (5s) sans réponse UCI.");
            if (!p.HasExited) p.Kill();
            return false;
        }
        catch (IOException ex) when (ex.Message.Contains("canal") || ex.Message.Contains("pipe"))
        {
            // On capture l'erreur de canal fermé car elle arrive quand le processus se coupe
            // mais cela ne veut pas dire que le moteur est "malade".
            Debug.WriteLine($"[DEBUG] VerifierSante: Canal fermé pendant la lecture (attendu).");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DEBUG] VerifierSante: Exception critique: {ex.Message}");
            return false;
        }
    }
    private async Task<(string tag, string url)> VerifierNouvelleVersionGitHub(string tagCourant)
    {
        string apiUrl = "https://api.github.com/repos/official-stockfish/Stockfish/releases/latest";
        var response = await client.GetStringAsync(apiUrl);
        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;
        string latestTag = root.GetProperty("tag_name").GetString();

        if (latestTag.Equals(tagCourant, StringComparison.OrdinalIgnoreCase)) return (null, null);
        Debug.WriteLine($"[DEBUG] Comparaison: Local={tagCourant} | GitHub={latestTag}");
        string arch = GetBestArchitectureSuffix();
        var asset = root.GetProperty("assets").EnumerateArray()
            .FirstOrDefault(a => a.GetProperty("name").GetString().ToLower().Contains("windows") &&
                                 a.GetProperty("name").GetString().ToLower().Contains(arch));
        Debug.WriteLine($"[DEBUG] Architecture détectée = " + arch);
        if (asset.ValueKind == JsonValueKind.Undefined)
            throw new Exception("Architecture non trouvée sur GitHub.");

        return (latestTag, asset.GetProperty("browser_download_url").GetString());
    }

    private static string GetBestArchitectureSuffix()
    {
        if (Bmi2.IsSupported && Avx2.IsSupported) return "x86-64-bmi2";
        if (Avx2.IsSupported) return "x86-64-avx2";
        if (Sse42.IsSupported) return "x86-64-modern";
        return "x86-64";
    }

    private static async Task TelechargerFichier(string url, string dest)
    {
        var data = await client.GetByteArrayAsync(url);
        await File.WriteAllBytesAsync(dest, data);
    }
}