// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Moteur auxiliaire de l'analyse de partie, sans interface graphique : un 2e processus Stockfish, indépendant du moteur
// principal, interrogé de façon SYNCHRONE (à appeler hors du thread de l'interface, ex : Task.Run)
// └─ Classe "MoteurAuxiliaire"
//              └─ "MeilleursCoupsParProfondeur"   le meilleur coup à chaque profondeur, table de hachage vidée avant
// Pourquoi un 2e moteur : le principal analyse la partie de la fin vers le début, sa table de hachage connaît déjà les positions
// suivantes et il "trouverait" tout de suite un coup difficile ; celui-ci repart de zéro (1 thread, petite table), comme un joueur.
// Il est lancé à la première question et arrêté par Dispose (fin de l'analyse de partie), même au milieu d'une question.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BrunoGUI_GenII
{
    public sealed class MoteurAuxiliaire : IDisposable
    {
        private readonly string _chemin;
        private readonly object _verrou = new();    // une seule question à la fois
        private Process? _processus;
        private volatile bool _arrete;              // échec du démarrage ou Dispose : plus aucune question

        public MoteurAuxiliaire(string chemin) => _chemin = chemin;

        public List<(int Profondeur, string Coup)> MeilleursCoupsParProfondeur(string fen, int profondeurMax, int delaiMaxMs = 15000)
        {   // Meilleur coup (1re variante) annoncé à chaque profondeur jusqu'à profondeurMax ; liste vide si le moteur ne répond pas
            List<(int, string)> resultat = [];
            lock (_verrou)
            {
                if (Demarre() is not Process processus)
                    return resultat;
                try
                {
                    Envoie(processus, "ucinewgame");
                    Envoie(processus, "position fen " + fen);
                    Envoie(processus, "go depth " + profondeurMax);
                    // Sécurité : passé le délai, "stop" (le moteur répond alors bestmove)
                    using Timer arret = new(_ => EnvoieSansErreur(processus, "stop"), null, delaiMaxMs, Timeout.Infinite);
                    string? ligne;
                    while ((ligne = processus.StandardOutput.ReadLine()) != null && !ligne.StartsWith("bestmove"))
                    {
                        LigneUci info = LigneUci.Analyser(ligne);
                        if (info.Profondeur is int profondeur && (info.NumeroVariante ?? 1) == 1
                            && (info.Variante ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries) is [string coup, ..])
                        {   // plusieurs lignes par profondeur : la dernière compte
                            if (resultat.Count > 0 && resultat[^1].Item1 == profondeur)
                                resultat[^1] = (profondeur, coup);
                            else
                                resultat.Add((profondeur, coup));
                        }
                    }
                    if (ligne == null)
                    {   // le moteur s'est arrêté (ou Dispose l'a arrêté) : résultat incomplet, inutilisable
                        resultat.Clear();
                        Arrete();
                    }
                }
                catch (Exception ex) when (ex is IOException or InvalidOperationException)
                {   // (InvalidOperationException : processus arrêté par Dispose pendant la lecture)
                    if (!_arrete)
                        Journal.Erreur("Moteur auxiliaire de l'analyse", ex);
                    resultat.Clear();
                    Arrete();
                }
            }
            return resultat;
        }

        public (int? Centipions, int? MatEn, string Variante)? Cherche(string fen, string? coupImpose, int profondeur, int delaiMaxMs = 15000)
        {   // Score (du point de vue du camp au trait) et variante du coup imposé (searchmoves ; null : le meilleur coup), à la
            // profondeur donnée ; null si le moteur ne répond pas. Sert aux pièges de l'analyse (que donne la prise tentante que le
            // joueur n'a pas jouée ?) et aux menaces (que jouerait le joueur après un coup nul ?)
            lock (_verrou)
            {
                if (Demarre() is not Process processus)
                    return null;
                try
                {
                    Envoie(processus, "ucinewgame");
                    Envoie(processus, "position fen " + fen);
                    Envoie(processus, coupImpose == null ? $"go depth {profondeur}" : $"go depth {profondeur} searchmoves {coupImpose}");
                    using Timer arret = new(_ => EnvoieSansErreur(processus, "stop"), null, delaiMaxMs, Timeout.Infinite);
                    (int?, int?, string)? resultat = null;
                    string? ligne;
                    while ((ligne = processus.StandardOutput.ReadLine()) != null && !ligne.StartsWith("bestmove"))
                    {
                        LigneUci info = LigneUci.Analyser(ligne);
                        if ((info.ScoreCentipions != null || info.MatEn != null) && info.Variante != null && (info.NumeroVariante ?? 1) == 1)
                            resultat = (info.ScoreCentipions, info.MatEn, info.Variante);
                    }
                    if (ligne != null)
                        return resultat;
                    Arrete();
                }
                catch (Exception ex) when (ex is IOException or InvalidOperationException)
                {
                    if (!_arrete)
                        Journal.Erreur("Moteur auxiliaire de l'analyse (piège)", ex);
                    Arrete();
                }
                return null;
            }
        }

        private Process? Demarre()
        {
            if (_processus != null || _arrete)
                return _processus;
            if (!File.Exists(_chemin))
            {
                Journal.Info($"Moteur auxiliaire de l'analyse introuvable : {_chemin}");
                _arrete = true;
                return null;
            }
            try
            {
                Process processus = Process.Start(new ProcessStartInfo(_chemin)
                {
                    RedirectStandardInput = true, RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_chemin) ?? ""
                }) ?? throw new InvalidOperationException("processus non lancé");
                _processus = processus;
                Envoie(processus, "uci");
                Envoie(processus, "setoption name Threads value 1");
                Envoie(processus, "setoption name Hash value 16");
                Envoie(processus, "isready");
                string? ligne;
                while ((ligne = processus.StandardOutput.ReadLine()) != null && ligne != "readyok") { }
                if (ligne == null)
                    throw new InvalidOperationException("le moteur ne répond pas");
                return processus;
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                if (!_arrete)
                    Journal.Erreur($"Démarrage du moteur auxiliaire ({_chemin})", ex);
                _arrete = true;
                Arrete();
                return null;
            }
        }

        private static void Envoie(Process processus, string commande)
        {
            processus.StandardInput.WriteLine(commande);
            processus.StandardInput.Flush();
        }

        private static void EnvoieSansErreur(Process processus, string commande)
        {
            try
            {
                Envoie(processus, commande);
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException)
            {   // le moteur vient de s'arrêter : rien à interrompre
            }
        }

        private void Arrete()
        {   // Appelable depuis n'importe quel thread : la lecture en cours (ReadLine) se termine quand le processus meurt
            Process? processus = Interlocked.Exchange(ref _processus, null);
            if (processus == null)
                return;
            try
            {
                if (!processus.HasExited)
                {
                    EnvoieSansErreur(processus, "quit");
                    if (!processus.WaitForExit(500))
                        processus.Kill();
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                Journal.Erreur("Arrêt du moteur auxiliaire", ex);
            }
            processus.Dispose();
        }

        public void Dispose()
        {   // Sans attendre la question en cours (elle renverra une liste vide)
            _arrete = true;
            Arrete();
        }
    }
}
