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
//                      ├─ "OptionsCourantes"  
//                      ├─ "ActiveLimiteElo"            Activation de la limitation du ELO
//                      ├─ "DefinitLimiteElo"           Définition de la force ELO du moteur  
//                      ├─ "DefinitMultiPV"             Nombre de variantes demandées au moteur
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
    public class MoteurUci
    {
        public static event AfficheMoteurUci AfficheUci;
        public static event AfficheDonneesBrutesUci  AfficheDonneesBrutes;
        public static event AfficheCoupMoteurUci AfficheCoupMoteur;
        public static List<string> OptionsUci = [];  // Noms des options déclarées par le moteur (lignes "option name ...")
        public static string DataUci { get; set; }              // dernière ligne reçue du moteur, telle quelle
        public static LigneUci DerniereLigne { get; private set; } = new();   // la même ligne, décodée
        public static string DataVersUci { get; set; }
        public static string CoupAuFormatUci { get; set; }
        public static string FichierMoteurUci { get; set; }
        public static string AuteurMoteur { get; set; }
        public static bool LimiteElo { get; set; }
        public static bool UciVersGui { get; set; }
        public static int NombreLignesPV { get; set; } = 3;     // Nombre de variantes (MultiPV) demandées au moteur
        public static int? NombreThreads { get; set; }          // Threads et Hash (Mo) envoyés au démarrage du moteur (null : valeur du moteur)
        public static int? TailleHachageMo { get; set; }
        private static bool _optionsDemarrageEnvoyees;          // Threads/Hash ne sont envoyés qu'une fois par démarrage du moteur
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
            LogiqueMouvements.StatutMoteurUci = true;
            OptionsUci.Clear();
            _optionsDemarrageEnvoyees = false;
            StandardInputDataToUci("uci");  // On demande les infos au moteur (il répond par ses options puis "uciok")
        }

        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {   // Evènement de sortie de données du processus UCI vers l'interface pour jouer le coup du moteur UCI
            UciVersGui = true; 
            if (string.IsNullOrWhiteSpace(e.Data) == false)   // true si la chaine est " ", "\n", null, ""
            {   // La ligne est décodée une seule fois ; l'interface lit le résultat dans DerniereLigne
                DataUci = e.Data;
                DerniereLigne = LigneUci.Analyser(DataUci);
                AfficheUci?.Invoke();
                AfficheDonneesBrutes?.Invoke();

                switch (DerniereLigne.Commande)         // Analyse réponse moteur UCI
                {
                    case "readyok": // le moteur UCI est prêt à jouer
                        // position de départ ( Fen correspondant à un début de partie )
                        PositionFenUci(LogiqueMouvements.FenDepart);
                        break;
                    case "bestmove": // le moteur UCI propose le meilleur coup
                        if (!DerniereLigne.AucunCoupLegal) // Un moteur retourne "(none)" ou "0000" en cas de Mat ou Pat
                        {
                            CoupAuFormatUci = DerniereLigne.MeilleurCoup;
                            AfficheCoupMoteur?.Invoke();
                        }
                        else
                        {   // Si le moteur répond "bestmove (none)", c'est MAT ou PAT , alors il ne faut pas AfficherCoupMoteur
                            if (LogiqueMouvements.Echec)
                            {   // Roi en échec => MAT
                                LogiqueMouvements.EchecetMat = true;
                            }
                            else
                            {   // Roi pas en échec => PAT
                                LogiqueMouvements.Pat = true;
                            }
                        }
                        break;
                    case "uciok":   // le moteur a fini de déclarer ses options
                        EnvoieOptionsDemarrage();
                        break;
                    case "option":
                        if (DerniereLigne.NomOption != null && !OptionsUci.Contains(DerniereLigne.NomOption))
                            OptionsUci.Add(DerniereLigne.NomOption);
                        if (DerniereLigne.NomOption == "UCI_LimitStrength")  // il est possible de régler la force ELO
                        {
                            ActiveLimiteElo();
                            LimiteElo = true;
                        }
                        break;
                }
            }
            UciVersGui = false;
        }
        private static void EnvoieOptionsDemarrage()
        {   // Threads et Hash de BrunoGUI.ini, envoyés une seule fois et seulement si le moteur déclare ces options
            // (un nouveau "uci", par exemple depuis la fenêtre des paramètres, ne doit pas écraser les réglages faits entre-temps)
            if (_optionsDemarrageEnvoyees)
                return;
            _optionsDemarrageEnvoyees = true;
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
            Proc.StandardInput.Write(Data + Environment.NewLine);
        }
        private static void PositionFenUci(string PositionFenActuel)
        {   // Position Fen courante envoyée au Moteur UCI
            StandardInputDataToUci("position fen " + PositionFenActuel);
        }
        public static void JeuMoteurUci(string FenActuel, int Duree)
        {   // Envoie au moteur UCI le Fen actuel et invitation à jouer pour le moteur UCI
            StandardInputDataToUci("setoption name MultiPV value " + NombreLignesPV);   // On demande le nombre de variations choisi
            PositionFenUci(FenActuel);
            if (Duree == 9999)
                StandardInputDataToUci("go infinite");      // Réflexion sans limite, jusqu'à l'envoi de "stop"
            else
                StandardInputDataToUci("go movetime " + Duree.ToString());
        }
        public static void OptionsCourantes()
        {   // Envoi au moteur UCI les options courantes
            StandardInputDataToUci("setoption name Ponder value true");     // Réfléchit sur le temps de l'adversaire
            StandardInputDataToUci("setoption name Verbose value true");
            StandardInputDataToUci("setoption name Ownbook value true");
            StandardInputDataToUci("setoption name VerboseBook value true");
            StandardInputDataToUci("setoption name MultiPV value " + NombreLignesPV);
        }
        public static void DefinitMultiPV(int nombreLignes)
        {   // Mémorise et envoie au moteur le nombre de variantes (MultiPV)
            NombreLignesPV = nombreLignes;
            StandardInputDataToUci("setoption name MultiPV value " + nombreLignes);
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
        {   // On ferme le moteur UCI
            StandardInputDataToUci("quit");
            LogiqueMouvements.StatutMoteurUci = false;
            LimiteElo = false;
            Proc.Dispose();
        }
    }
}
