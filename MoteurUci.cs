// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Gestion du moteur UCI ...
//      └─ Classe "MoteurUci" qui gère le moteur UCI
//                      ├─ "Start"  pour démarrer le moteur
//                      ├─ "ProcOutputDataReceived"     Evènement de sortie de données du processus UCI  
//                      ├─ "EnvoieOptionsDemarrage"     Threads et Hash de BrunoGUI.ini, envoyés à la réception de "uciok"
//                      ├─ "StandardInputDataToUci"     Envoi de données de l'interface vers moteur UCI
//                      ├─ "PositionFenUci"             Position Fen courante envoyée au Moteur UCI
//                      ├─ "JeuMoteurUci"               Envoie au moteur UCI le Fen actuel
//                      ├─ "ActiveLimiteElo"            Activation de la limitation du ELO
//                      ├─ "DefinitLimiteElo"           Définition de la force ELO du moteur  
//                      ├─ "DefinitMultiPV"             Nombre de variantes demandées au moteur
//                      ├─ "DefinitThreads" / "DefinitHachage"   Threads et table de hachage (mémorisés pour les redémarrages)
//                      ├─ "SpecialeSargon"            Profondeur = 6 sinon boucle infinie ...  
//                      └─ "Quitte"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BrunoGUI_GenII
{
    public delegate void AfficheMoteurUci();
    public delegate void AfficheCoupMoteurUci();
    public delegate void AfficheDonneesBrutesUci();
    public class MoteurUci : IMoteur
    {
        // Le moteur vu par PiloteMoteur (qui ne connaît que IMoteur : un faux moteur le remplace dans les tests)
        void IMoteur.Chercher(string fen, int dureeMilliSecondes) => JeuMoteurUci(fen, dureeMilliSecondes);
        void IMoteur.Abandonner() => AbandonneDemandeEnCours();
        bool IMoteur.EnReflexion => EnReflexion;

        public static event AfficheMoteurUci AfficheUci;
        public static event AfficheDonneesBrutesUci  AfficheDonneesBrutes;
        public static event AfficheCoupMoteurUci AfficheCoupMoteur;
        public static List<string> OptionsUci = [];  // Noms des options déclarées par le moteur (lignes "option name ...")
        public static string DataUci { get; set; }              // dernière ligne reçue du moteur, telle quelle
        public static LigneUci DerniereLigne { get; private set; } = new();   // la même ligne, décodée
        public static string DataVersUci { get; set; }
        public static string CoupAuFormatUci { get; set; }
        public static bool UciVersGui { get; set; }
        public static int NombreLignesPV { get; set; } = 3;     // Nombre de variantes (MultiPV) demandées au moteur
        public static int? NombreThreads { get; set; }          // Threads et Hash (Mo) envoyés au démarrage du moteur (null : valeur du moteur)
        public static int? TailleHachageMo { get; set; }
        private static bool _optionsDemarrageEnvoyees;          // Threads/Hash ne sont envoyés qu'une fois par démarrage du moteur
        public static string NomAnnonce { get; private set; }  // nom annoncé par le moteur ("id name ..."), ex : Stockfish 19

        // Numérotation des demandes ("go") : une réponse à une demande abandonnée (retour arrière, nouvelle partie, résultat...)
        // est ignorée, ce qui permet de laisser l'interface active pendant la réflexion du moteur
        public static readonly SuiviDemandesMoteur Demandes = new();
        public static int NumeroDemandeDeLaLigne { get; private set; }     // demande à laquelle répond DerniereLigne
        public static bool LigneAbandonnee => Demandes.EstAbandonnee(NumeroDemandeDeLaLigne);  // DerniereLigne est une réponse périmée
        public static bool EnReflexion => Demandes.EnAttenteNonAbandonnee;    // le moteur réfléchit à une demande toujours valable
        public static void AbandonneDemandeEnCours()
        {   // La demande en cours devient périmée : "stop" (le moteur répond tout de suite par un bestmove, qui sera ignoré)
            if (Demandes.Abandonner())
                StandardInputDataToUci("stop");
        }
        private static Process Proc;

        public void Start(string fichierMoteurUci)
        {   // Démarrage du moteur Uci dont le chemin est passé en paramêtre
            string cheminComplet = Path.GetFullPath(fichierMoteurUci);
            string repertoireMoteur = Path.GetDirectoryName(cheminComplet)!;

            Proc = new Process();

            Proc.StartInfo.FileName = cheminComplet;
            Proc.StartInfo.WorkingDirectory = repertoireMoteur;
            
            Proc.StartInfo.UseShellExecute = false;
            Proc.StartInfo.RedirectStandardOutput = true;
            Proc.StartInfo.RedirectStandardInput = true;
            Proc.StartInfo.CreateNoWindow = true;
            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            // gestionnaire d'événement de sortie de données
            Proc.OutputDataReceived += ProcOutputDataReceived;
            // démarrer le processus
            Proc.Start();
            // commencer à lire les sorties de données
            Proc.BeginOutputReadLine();
            // première interrogation du processus: le moteur UCI est il pret ? 
            OptionsUci.Clear();
            NomAnnonce = null;
            _optionsDemarrageEnvoyees = false;
            Demandes.Reinitialiser();
            StandardInputDataToUci("uci");  // On demande les infos au moteur (il répond par ses options puis "uciok")
        }

        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {   // Evènement de sortie de données du processus UCI vers l'interface pour jouer le coup du moteur UCI
            if (sender != Proc)
                return;     // ligne d'un ancien processus moteur (arrêté par un changement de moteur ou une nouvelle partie) : ignorée
            UciVersGui = true;
            if (string.IsNullOrWhiteSpace(e.Data) == false)   // true si la chaine est " ", "\n", null, ""
            {   // La ligne est décodée une seule fois ; l'interface lit le résultat dans DerniereLigne
                DataUci = e.Data;
                DerniereLigne = LigneUci.Analyser(DataUci);
                // Demande à laquelle répond la ligne : un "bestmove" termine la demande en cours
                NumeroDemandeDeLaLigne = DerniereLigne.Commande == "bestmove" ? Demandes.ReponseRecue() : Demandes.NumeroEnCours;
                AfficheUci?.Invoke();
                AfficheDonneesBrutes?.Invoke();

                switch (DerniereLigne.Commande)         // Analyse réponse moteur UCI
                {
                    case "readyok": // le moteur UCI est prêt à jouer
                        // position de départ ( Fen correspondant à un début de partie )
                        PositionFenUci(LogiqueMouvements.FenDepart);
                        break;
                    case "bestmove": // le moteur UCI propose le meilleur coup
                        // Un moteur retourne "(none)" ou "0000" en cas de mat ou de pat : aucun coup à jouer.
                        // Ce cas est traité par l'interface (AfficheUci, sur son thread), pas ici sur le thread du moteur
                        if (!DerniereLigne.AucunCoupLegal && !LigneAbandonnee)     // réponse périmée : ignorée
                        {
                            CoupAuFormatUci = DerniereLigne.MeilleurCoup;
                            AfficheCoupMoteur?.Invoke();
                        }
                        break;
                    case "id":
                        if (DerniereLigne.NomMoteur != null)
                            NomAnnonce = DerniereLigne.NomMoteur;
                        break;
                    case "uciok":   // le moteur a fini de déclarer ses options
                        EnvoieOptionsDemarrage();
                        break;
                    case "option":
                        if (DerniereLigne.NomOption != null && !OptionsUci.Contains(DerniereLigne.NomOption))
                            OptionsUci.Add(DerniereLigne.NomOption);
                        if (DerniereLigne.NomOption == "UCI_LimitStrength")  // il est possible de régler la force ELO
                            ActiveLimiteElo();
                        break;
                }
            }
            UciVersGui = false;
        }
        private static void EnvoieOptionsDemarrage()
        {   // Threads et Hash de BrunoGUI.ini (réglages de Stockfish), envoyés une seule fois et seulement si le moteur déclare ces options
            // (un nouveau "uci", par exemple depuis la fenêtre des paramètres, ne doit pas écraser les réglages faits entre-temps).
            // Uniquement à Stockfish : d'autres moteurs se comportent autrement avec plusieurs threads
            // (ex : Rodent IV ignore alors MultiPV et n'affiche plus qu'une variante) ; ils gardent leurs propres réglages.
            if (_optionsDemarrageEnvoyees)
                return;
            _optionsDemarrageEnvoyees = true;
            if (NomAnnonce?.StartsWith("Stockfish", StringComparison.OrdinalIgnoreCase) != true)
                return;
            if (NombreThreads is int threads && OptionsUci.Contains("Threads"))
                StandardInputDataToUci("setoption name Threads value " + threads);
            if (TailleHachageMo is int hachage && OptionsUci.Contains("Hash"))
                StandardInputDataToUci("setoption name Hash value " + hachage);
        }
        public static void StandardInputDataToUci(string Data)
        {   // Envoi de données de l'interface vers moteur UCI
            Debug.WriteLine($"[App] {Data}");
            UciVersGui = false;
            DataVersUci = Data;
            AfficheDonneesBrutes?.Invoke();
            try
            {
                Proc?.StandardInput.Write(Data + Environment.NewLine);   // pas de moteur démarré : commande ignorée
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
            {   // Le moteur a été arrêté (ex : pendant une mise à jour) : commande ignorée plutôt que plantage
                Debug.WriteLine($"[App] Commande non envoyée, moteur arrêté : {ex.Message}");
            }
        }
        private static void PositionFenUci(string PositionFenActuel)
        {   // Position Fen courante envoyée au Moteur UCI
            StandardInputDataToUci("position fen " + PositionFenActuel);
        }
        public const int ReflexionInfinie = -1;     // durée à passer à JeuMoteurUci pour une réflexion sans limite (jusqu'à "stop")

        public static void JeuMoteurUci(string FenActuel, int Duree)
        {   // Envoie au moteur UCI le Fen actuel et invitation à jouer pour le moteur UCI (Duree en millisecondes, ou ReflexionInfinie)
            AbandonneDemandeEnCours();      // une nouvelle demande remplace celle en cours (UCI interdit "position"/"go" pendant une recherche)
            StandardInputDataToUci("setoption name MultiPV value " + NombreLignesPV);   // On demande le nombre de variations choisi
            PositionFenUci(FenActuel);
            Demandes.DemandeEnvoyee();      // numéro de cette demande (compté avant l'envoi du "go", dont la réponse peut arriver très vite)
            if (Duree == ReflexionInfinie)
                StandardInputDataToUci("go infinite");      // Réflexion sans limite, jusqu'à l'envoi de "stop"
            else
                StandardInputDataToUci("go movetime " + Duree.ToString());
        }
        public static void DefinitMultiPV(int nombreLignes)
        {   // Mémorise et envoie au moteur le nombre de variantes (MultiPV)
            NombreLignesPV = nombreLignes;
            StandardInputDataToUci("setoption name MultiPV value " + nombreLignes);
        }
        public static void DefinitThreads(int nombreThreads)
        {   // Mémorise (renvoyé à chaque redémarrage du moteur, enregistré dans le .ini) et envoie au moteur le nombre de threads
            NombreThreads = nombreThreads;
            StandardInputDataToUci("setoption name Threads value " + nombreThreads);
        }
        public static void DefinitHachage(int tailleMo)
        {   // Mémorise (renvoyé à chaque redémarrage du moteur, enregistré dans le .ini) et envoie au moteur la table de hachage (Mo)
            TailleHachageMo = tailleMo;
            StandardInputDataToUci("setoption name Hash value " + tailleMo);
        }
        public static void ActiveLimiteElo()
        {   // Activation de la limitation du ELO
            StandardInputDataToUci("setoption name UCI_LimitStrength value true");
        }
        public static void DefinitLimiteElo(string ValeurElo)
        {   // Définition de la force ELO du moteur (default 1320 min 1320 max 3190 pour Stockfish)
            StandardInputDataToUci("setoption name UCI_Elo value " + ValeurElo);
        }
        public static void SpecialeSargon()
        {   // Sinon Sargon  mouline sans fin !!
            StandardInputDataToUci("setoption name FixedDepth value 6");       
        }
        public static void Quitte()
        {   // On ferme le moteur UCI (sans erreur s'il n'a jamais démarré ou s'il a déjà été arrêté, ex : par une mise à jour).
            // Certains moteurs (ex : Sargon 1978) ignorent "quit" : après une seconde d'attente, le processus est arrêté de force,
            // sinon il resterait en mémoire et verrouillerait son .exe
            if (Proc == null)
                return;
            StandardInputDataToUci("quit");
            try
            {
                if (!Proc.WaitForExit(1000))
                    Proc.Kill();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
            {   // processus déjà terminé ou inaccessible : rien à faire
                Debug.WriteLine($"[App] Arrêt du moteur : {ex.Message}");
            }
            Proc.Dispose();
            Proc = null;
        }
    }
}
