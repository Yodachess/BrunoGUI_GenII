// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Moteur auxiliaire de l'analyse de partie, sans interface graphique : un 2e processus Stockfish, indépendant du moteur
// principal, interrogé de façon SYNCHRONE (à appeler hors du thread de l'interface, ex : Task.Run)
// └─ Classe "MoteurAuxiliaire"
//              ├─ "MeilleursCoupsParProfondeur"   le meilleur coup à chaque profondeur, table de hachage vidée avant
//              └─ "Mesure"                        difficulté, pièges et menaces d'une analyse de partie (commun aux deux analyses)
// Pourquoi un 2e moteur : le principal analyse la partie de la fin vers le début, sa table de hachage connaît déjà les positions
// suivantes et il "trouverait" tout de suite un coup difficile ; celui-ci repart de zéro (1 thread, petite table), comme un joueur.
// Il est lancé à la première question et arrêté par Dispose (fin de l'analyse de partie), même au milieu d'une question.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrunoGUI_GenII
{
    public sealed class MoteurAuxiliaire : IDisposable
    {
        private readonly string _chemin;
        private readonly int _threads, _hachage;
        private readonly object _verrou = new();    // une seule question à la fois
        private Process? _processus;
        private volatile bool _arrete;              // échec du démarrage ou Dispose : plus aucune question
        private int _variantes = 1;                 // MultiPV envoyé au moteur

        // threads, hachage : 1 et 16 Mo pour mesurer la difficulté (un joueur qui repart de zéro) ; ceux de l'utilisateur pour
        // l'analyse d'un fichier en arrière-plan (Analyse)
        public MoteurAuxiliaire(string chemin, int threads = 1, int hachage = 16)
        {
            _chemin = chemin;
            _threads = Math.Max(1, threads);
            _hachage = Math.Max(16, hachage);
        }

        public void NouvellePartie()
        {   // Table de hachage vidée (Analyse la garde d'une position à l'autre de la même partie, comme l'analyse de partie)
            lock (_verrou)
            {
                if (Demarre() is Process processus)
                    EnvoieSansErreur(processus, "ucinewgame");
            }
        }

        public (LigneAnalyse? Meilleure, LigneAnalyse? Seconde)? Analyse(string fen, int profondeur, int dureeMaxMs, int variantes = 2)
        {   // Analyse de la position comme l'analyse de partie ("go depth … movetime …", au moins 2 variantes : la 2e dit si le
            // meilleur coup était le seul bon) : la meilleure et la 2e variante, du point de vue des Blancs ; null si le moteur ne
            // répond pas. La table de hachage est gardée (NouvellePartie la vide)
            lock (_verrou)
            {
                if (Demarre() is not Process processus)
                    return null;
                try
                {
                    if (variantes != _variantes)
                    {
                        Envoie(processus, $"setoption name MultiPV value {variantes}");
                        _variantes = variantes;
                    }
                    Position position = LogiqueMouvements.PositionDepuisFen(fen);
                    SuiviAnalyse suivi = new();
                    Envoie(processus, "position fen " + fen);
                    Envoie(processus, $"go depth {profondeur} movetime {Math.Max(1000, dureeMaxMs)}");
                    // Sécurité : passé le délai (et une marge), "stop" (le moteur répond alors bestmove)
                    using Timer arret = new(_ => EnvoieSansErreur(processus, "stop"), null, Math.Max(1000, dureeMaxMs) + 5000, Timeout.Infinite);
                    string? ligne;
                    while ((ligne = processus.StandardOutput.ReadLine()) != null && !ligne.StartsWith("bestmove"))
                    {
                        LigneUci info = LigneUci.Analyser(ligne);
                        if (info.Profondeur != null)
                            suivi.Ajouter(info, position);
                    }
                    if (ligne != null)
                        return (suivi.Meilleure, suivi.Seconde);
                    Arrete();
                }
                catch (Exception ex) when (ex is IOException or InvalidOperationException)
                {
                    if (!_arrete)
                        Journal.Erreur("Moteur de l'analyse d'un fichier", ex);
                    Arrete();
                }
                return null;
            }
        }

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

        public async Task Mesure(AnalyseDePartie analyse, Func<bool> continuer)
        {   // Les mesures devenues possibles après une position analysée (analyse de partie et analyse d'un fichier) : profondeur où
            // les seuls bons coups sont trouvés, prises tentantes qui perdaient (pièges), menaces (coup nul). Chaque question part
            // hors du thread de l'appelant, et sa réponse est enregistrée sur ce thread (celui de l'interface pour l'analyse de
            // partie : rien ne touche l'analyse en même temps) ; arrêt dès que continuer() est faux (analyse interrompue)
            foreach ((int index, string fen, string coup) in analyse.DifficultesAMesurer().ToList())
            {   // (quelques dixièmes de seconde chacun)
                if (!continuer())
                    return;
                List<(int, string)> meilleurs = await Task.Run(() => MeilleursCoupsParProfondeur(fen, JugementCoups.ProfondeurMesureDifficulte));
                if (!continuer())
                    return;
                analyse.EnregistreDifficulte(index, JugementCoups.ProfondeurDecouverte(meilleurs, coup));
            }
            foreach ((int index, string fen, string prise) in analyse.PiegesAChercher())
            {   // la prise tentante que le joueur n'a pas jouée : que donnait-elle ? (une ou deux secondes chacune)
                if (!continuer())
                    return;
                var resultat = await Task.Run(() => Cherche(fen, prise, JugementCoups.ProfondeurPiege));
                if (!continuer())
                    return;
                analyse.EnregistrePiege(index, prise, resultat?.Centipions, resultat?.MatEn, resultat?.Variante);
            }
            foreach ((int index, string fenCoupNul) in analyse.MenacesAChercher())
            {   // la menace du coup joué : ce que son auteur jouerait si l'adversaire ne faisait rien (coup nul, environ 0,5 s)
                if (!continuer())
                    return;
                var resultat = await Task.Run(() => Cherche(fenCoupNul, null, JugementCoups.ProfondeurMenace));
                if (!continuer())
                    return;
                analyse.EnregistreMenace(index, resultat?.Centipions, resultat?.MatEn, resultat?.Variante);
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
                Envoie(processus, $"setoption name Threads value {_threads}");
                Envoie(processus, $"setoption name Hash value {_hachage}");
                _variantes = 1;
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
