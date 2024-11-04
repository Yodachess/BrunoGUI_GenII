// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Stockfish est développé par Bruno COURTOIS.  Copyright © 2024 █  
// █ BrunoGUI_Stockfish est gratuit, sauf s'il est utilisé commercialement  █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
// Informations reflexion moteur - Temps de reflexion - Réglage force moteur
// Gestion par menus - Sauvegarde PGN - Affichage Score - Personnalisation couleurs 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BrunoGUI_Stockfish
{
    public delegate void AfficheMoteurUci();
    public delegate void AfficheCoupMoteurUci();
    public delegate void AfficheDonneesBrutesUci();
    public class MonoMoteurUci
    {
        public static event AfficheMoteurUci AfficheUci;
        public static event AfficheDonneesBrutesUci  AfficheDonneesBrutes;
        public static event AfficheCoupMoteurUci AfficheCoupMoteur;
        public static List<string> OptionsUci = new List<string>();  // Liste pour stocker les options du moteur UCi
        public static string DataUci { get; set; }
        public static string DataVersUci { get; set; }
        public static string CoupUci { get; set; }
        public static string FichierMoteurUci { get; set; }
        public static string AuteurMoteur { get; set; }
        // public static bool LimiteElo { get; set; }
        public static bool UciVersGui { get; set; }
        private static Process Proc;

        // Démarrage du moteur Uci dont le chemin est passé en paramêtre
        public void Start(string fichierMoteurUci)
        {
            var CurrentDirectory = Directory.GetCurrentDirectory();
            Proc = new Process();
            //  paramétrage de Proc.StartInfo
            Proc.StartInfo.FileName = fichierMoteurUci;
            Proc.StartInfo.UseShellExecute = false;
            Proc.StartInfo.RedirectStandardOutput = true;
            Proc.StartInfo.RedirectStandardInput = true;
            Proc.StartInfo.CreateNoWindow = true;
            Proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            // démarrer le processus
            Proc.Start();
            // gestionnaire d'événement de sortie de données
            Proc.OutputDataReceived += ProcOutputDataReceived;
            // commencer à lire les sorties de données
            Proc.BeginOutputReadLine();
            // première interrogation du processus: le moteur UCI est il pret ? 
            LogiqueMouvements.StatutMoteurUci = true;
            StandardInputDataToUci("uci");          // On demande les infos au moteur
        }

        // Evènement de sortie de données du processus UCI vers l'interface pour jouer le coup du moteur UCI
        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            UciVersGui = true; 
            if (string.IsNullOrWhiteSpace(e.Data) == false)   // true si la chaine est " ", "\n", null, ""
            {
                // extraire les mots de l'instruction
                DataUci = e.Data;
                AfficheUci();

                string[] DataTableau = DataUci.Split(' ');
                AfficheDonneesBrutes();

                switch (DataTableau[0])         // Analyse réponse moteur UCI 
                {
                    // identifier le premier mot 
                    case "\n":
                    case " ":
                        break;
                    case "option":
                        OptionsUci.Add(MonoMoteurUci.DataUci);
                        Console.WriteLine($"Option Uci : {MonoMoteurUci.DataUci}");
                        break;
                    case "bestmove": // le moteur UCI propose le meilleur coup
                        //  CRASH : tester si DataTableau[1] = (none), alors il y a MAT, il ne faut pas AfficherCoupMoteur
                        if (DataTableau[1] != "(none)")     // Si le moteur répond "bestmove (none)", c'est MAT
                        {                               // Et il ne faut pas AfficherCoupMoteur
                            CoupUci = DataTableau[1];
                            AfficheCoupMoteur();
                        }
                        else
                        {
                            LogiqueMouvements.EchecetMat = true;
                        }
                        break;
                }
            }
            UciVersGui = false;
        }
        public static void StandardInputDataToUci(string Data)
        {           // Envoi de données de l'interface vers moteur UCI
            Debug.WriteLine($"[App] {Data}");
            UciVersGui = false;
            DataVersUci = Data;
            AfficheDonneesBrutes();
            Proc.StandardInput.Write(Data + Environment.NewLine);
        }
        public static void JeuMoteurUci(string fenActuel, int duree)
        {           // Envoie au moteur UCI le Fen actuel et invitation à jouer pour le moteur UCI
            StandardInputDataToUci("position fen " + fenActuel);
            if (duree == 9999)
                StandardInputDataToUci("go movetime infinite");
            else            {
                StandardInputDataToUci("go movetime " + duree.ToString());
            }
        }
        public static void ActiveLimiteElo()
        {           // Activation de la limitation du ELO
            StandardInputDataToUci("setoption name UCI_LimitStrength value true");
        }
        public static void DefinitLimiteElo(string ValeurElo)
        {           // Définition de la force ELO du moteur (default 1320 min 1320 max 3190 pour Stockfish)
            StandardInputDataToUci("setoption name UCI_Elo value " + ValeurElo);
        }
        public void Quitte()
        {           // On ferme le moteur UCI
            StandardInputDataToUci("quit");
            LogiqueMouvements.StatutMoteurUci = false;
            Proc.Dispose();
        }
    }
}
