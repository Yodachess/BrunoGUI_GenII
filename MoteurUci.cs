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
        public static List<string> OptionsUci = [];  // Liste pour stocker les options du moteur UCi
        public static string DataUci { get; set; }
        public static string DataVersUci { get; set; }
        public static string CoupAuFormatUci { get; set; }
        public static string FichierMoteurUci { get; set; }
        public static string AuteurMoteur { get; set; }
        public static bool LimiteElo { get; set; }
        public static bool UciVersGui { get; set; }
        public static int NombreLignesPV { get; set; } = 3;     // Nombre de variantes (MultiPV) demandées au moteur
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
            StandardInputDataToUci("uci");  // On demande les infos au moteur
        }

        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {   // Evènement de sortie de données du processus UCI vers l'interface pour jouer le coup du moteur UCI
            UciVersGui = true; 
            if (string.IsNullOrWhiteSpace(e.Data) == false)   // true si la chaine est " ", "\n", null, ""
            {   // extraire les mots de l'instruction
                DataUci = e.Data;
                AfficheUci();

                string[] DataTableau = DataUci.Split(' ');
                AfficheDonneesBrutes();

                switch (DataTableau[0])         // Analyse réponse moteur UCI 
                {   // identifier le premier mot 
                    case "\n":
                    case " ":
                        break;
                    case "readyok": // le moteur UCI est prêt à jouer
                        // position de départ ( Fen correspondant à un début de partie )
                        PositionFenUci(LogiqueMouvements.FenDepart);
                        break;
                    case "bestmove": // le moteur UCI propose le meilleur coup
                                     //  CRASH : tester si DataTableau[1] = (none), alors il y a MAT, il ne faut pas AfficherCoupMoteur
                        if (DataTableau[1] != "(none)" && DataTableau[1] != "0000") // Un moteur au - retourne 0000 en cas de Mat ou Pat
                        {                               
                            CoupAuFormatUci = DataTableau[1];
                            AfficheCoupMoteur();
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
                    case "option":
                        if (DataTableau[2] == "UCI_LimitStrength")  // il est possible de régler la force ELO
                        {
                            ActiveLimiteElo();
                            LimiteElo = true;
                        }
                        break;
                }
            }
            UciVersGui = false;
        }
        public static void StandardInputDataToUci(string Data)
        {   // Envoi de données de l'interface vers moteur UCI
            Debug.WriteLine($"[App] {Data}");
            UciVersGui = false;
            DataVersUci = Data;
            AfficheDonneesBrutes();
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
