// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Gestion du moteur UCI ...
//      └─ Classe "MoteurUci" qui gère le moteur UCI (un objet par moteur : tout son état est dans l'objet, rien de statique)
//                      ├─ "Start"  pour démarrer le moteur
//                      ├─ "ProcOutputDataReceived"     Evènement de sortie de données du processus UCI  
//                      ├─ "EnvoieOptionsDemarrage"     Threads et Hash de BrunoGUI.ini, envoyés à la réception de "uciok"
//                      ├─ "StandardInputDataToUci"     Envoi de données de l'interface vers moteur UCI
//                      ├─ "PositionFenUci"             Position Fen courante envoyée au Moteur UCI
//                      ├─ "JeuMoteurUci"               Envoie au moteur UCI le Fen actuel
//                      ├─ "ActiveLimiteElo"            Activation de la limitation du ELO
//                      ├─ "DefinitLimiteElo"           Définition de la force ELO du moteur  
//                      ├─ "DefinitNiveau"              "Skill Level" pour jouer
//                      ├─ "AppliqueForce"              Force limitée pour jouer, pleine force pour analyser (avant chaque "go")
//                      ├─ "DefinitMultiPV"            Nombre de variantes demandées au moteur
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
        void IMoteur.Chercher(string fen, LimiteTemps limite, bool forceMaximale) => JeuMoteurUci(fen, limite, forceMaximale);
        void IMoteur.Abandonner() => AbandonneDemandeEnCours();
        bool IMoteur.EnReflexion => EnReflexion;

        public event AfficheMoteurUci? AfficheUci;
        public event AfficheDonneesBrutesUci? AfficheDonneesBrutes;
        public event AfficheCoupMoteurUci? AfficheCoupMoteur;
        public List<string> OptionsUci = [];  // Noms des options déclarées par le moteur (lignes "option name ...")
        public string DataUci { get; set; } = "";            // dernière ligne reçue du moteur, telle quelle
        public LigneUci DerniereLigne { get; private set; } = new();   // la même ligne, décodée
        public string DataVersUci { get; set; } = "";
        public string CoupAuFormatUci { get; set; } = "";
        public bool UciVersGui { get; set; }
        public int NombreLignesPV { get; set; } = 3;     // Nombre de variantes (MultiPV) demandées au moteur
        public int? NombreThreads { get; set; }          // Threads et Hash (Mo) envoyés au démarrage du moteur (null : valeur du moteur)
        public int? TailleHachageMo { get; set; }
        private bool _optionsDemarrageEnvoyees;          // Threads/Hash ne sont envoyés qu'une fois par démarrage du moteur
        public string? NomAnnonce { get; private set; }  // nom annoncé par le moteur ("id name ..."), ex : Stockfish 19

        // Force du moteur : limitée pour jouer (Elo, niveau), jamais pour analyser. Ce qui est voulu pour jouer est mémorisé, et ce
        // qui est actuellement réglé dans le moteur aussi : avant chaque "go", JeuMoteurUci n'envoie que ce qui doit changer
        private bool _limiteEloVoulue;          // UCI_LimitStrength voulu pour jouer (ActiveLimiteElo)
        private bool _limiteEloDansMoteur;      // valeur actuelle de UCI_LimitStrength dans le moteur
        public int? NiveauVoulu { get; private set; }   // "Skill Level" réglé pour jouer (null : jamais réglé, 20 chez Stockfish)
        private int? _niveauDansMoteur;
        public const int NiveauMaximal = 20;

        // Numérotation des demandes ("go") : une réponse à une demande abandonnée (retour arrière, nouvelle partie, résultat...)
        // est ignorée, ce qui permet de laisser l'interface active pendant la réflexion du moteur
        public readonly SuiviDemandesMoteur Demandes = new();
        public int NumeroDemandeDeLaLigne { get; private set; }     // demande à laquelle répond DerniereLigne
        public bool LigneAbandonnee => Demandes.EstAbandonnee(NumeroDemandeDeLaLigne);  // DerniereLigne est une réponse périmée
        public bool EnReflexion => Demandes.EnAttenteNonAbandonnee;    // le moteur réfléchit à une demande toujours valable
        public void AbandonneDemandeEnCours()
        {   // La demande en cours devient périmée : "stop" (le moteur répond tout de suite par un bestmove, qui sera ignoré)
            if (Demandes.Abandonner())
                StandardInputDataToUci("stop");
        }
        private Process? Proc;

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
            // démarrer le processus (fichier absent ou illisible : exception pour l'appelant, et pas de moteur plutôt qu'un
            // processus jamais démarré)
            try
            {
                Proc.Start();
            }
            catch
            {
                Proc.Dispose();
                Proc = null;
                throw;
            }
            // commencer à lire les sorties de données
            Proc.BeginOutputReadLine();
            // première interrogation du processus: le moteur UCI est il pret ? 
            OptionsUci.Clear();
            NomAnnonce = null;
            _optionsDemarrageEnvoyees = false;
            _limiteEloVoulue = _limiteEloDansMoteur = false;    // un moteur qui démarre a ses réglages par défaut
            NiveauVoulu = _niveauDansMoteur = null;
            Demandes.Reinitialiser();
            StandardInputDataToUci("uci");  // On demande les infos au moteur (il répond par ses options puis "uciok")
        }

        private void ProcOutputDataReceived(object? sender, DataReceivedEventArgs e)
        {   // Evènement de sortie de données du processus UCI vers l'interface pour jouer le coup du moteur UCI
            if (sender != Proc)
                return;     // ligne d'un ancien processus moteur (arrêté par un changement de moteur ou une nouvelle partie) : ignorée
            try
            {
                TraiteLigneDuMoteur(e.Data);
            }
            catch (Exception ex)
            {   // Thread du moteur : une erreur non rattrapée ici arrêterait l'application. Elle est notée, la ligne est perdue,
                // et la lecture des lignes suivantes continue (les erreurs de l'interface sont traitées par SurLeThreadInterface)
                Journal.Erreur("Ligne du moteur : " + e.Data, ex);
            }
        }
        private void TraiteLigneDuMoteur(string? ligne)
        {
            UciVersGui = true;
            if (!string.IsNullOrWhiteSpace(ligne))
            {   // La ligne est décodée une seule fois ; l'interface lit le résultat dans DerniereLigne
                DataUci = ligne;
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
                            CoupAuFormatUci = DerniereLigne.MeilleurCoup ?? "";     // (jamais null ici : AucunCoupLegal est faux)
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
        private void EnvoieOptionsDemarrage()
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
        public void StandardInputDataToUci(string Data)
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
        private void PositionFenUci(string PositionFenActuel)
        {   // Position Fen courante envoyée au Moteur UCI
            StandardInputDataToUci("position fen " + PositionFenActuel);
        }
        public void JeuMoteurUci(string FenActuel, LimiteTemps limite, bool forceMaximale = false)
        {   // Envoie au moteur UCI le Fen actuel et invitation à jouer pour le moteur UCI (temps fixe, sans limite ou pendule : voir LimiteTemps)
            // forceMaximale (analyse) : sans la limite de force réglée pour jouer
            AbandonneDemandeEnCours();      // une nouvelle demande remplace celle en cours (UCI interdit "position"/"go" pendant une recherche)
            AppliqueForce(forceMaximale);
            StandardInputDataToUci("setoption name MultiPV value " + NombreLignesPV);   // On demande le nombre de variations choisi
            PositionFenUci(FenActuel);
            Demandes.DemandeEnvoyee();      // numéro de cette demande (compté avant l'envoi du "go", dont la réponse peut arriver très vite)
            StandardInputDataToUci(limite.CommandeGo);
        }
        public void DefinitMultiPV(int nombreLignes)
        {   // Mémorise et envoie au moteur le nombre de variantes (MultiPV)
            NombreLignesPV = nombreLignes;
            StandardInputDataToUci("setoption name MultiPV value " + nombreLignes);
        }
        public void DefinitThreads(int nombreThreads)
        {   // Mémorise (renvoyé à chaque redémarrage du moteur, enregistré dans le .ini) et envoie au moteur le nombre de threads
            NombreThreads = nombreThreads;
            StandardInputDataToUci("setoption name Threads value " + nombreThreads);
        }
        public void DefinitHachage(int tailleMo)
        {   // Mémorise (renvoyé à chaque redémarrage du moteur, enregistré dans le .ini) et envoie au moteur la table de hachage (Mo)
            TailleHachageMo = tailleMo;
            StandardInputDataToUci("setoption name Hash value " + tailleMo);
        }
        public void ActiveLimiteElo()
        {   // Activation de la limitation du ELO (pour jouer : une analyse la retire le temps de sa recherche, voir AppliqueForce)
            _limiteEloVoulue = _limiteEloDansMoteur = true;
            StandardInputDataToUci("setoption name UCI_LimitStrength value true");
        }
        public void DefinitNiveau(int niveau)
        {   // "Skill Level" (0 à 20 chez Stockfish) pour jouer ; une analyse se fait toujours au niveau maximal
            NiveauVoulu = _niveauDansMoteur = niveau;
            StandardInputDataToUci("setoption name Skill Level value " + niveau);
        }
        private void AppliqueForce(bool forceMaximale)
        {   // Avant un "go" : force limitée (Elo, niveau) pour jouer, pleine force pour analyser.
            // Comportement de Stockfish (vérifié dans stockfish\src\search.cpp) :
            //  - UCI_Elo est une option à part, gardée par le moteur : "UCI_LimitStrength value false" ne fait que l'ignorer, et
            //    "value true" la réutilise sans qu'il faille la renvoyer (à chaque recherche : Skill(Skill Level, LimitStrength ? UCI_Elo : 0)).
            //    Elle n'est perdue qu'au redémarrage du moteur (retour à 1320).
            //  - Avec une force limitée (Elo, ou Skill Level < 20), Stockfish cherche au moins 4 variantes en coulisse, choisit un coup
            //    moins bon (Skill::pick_best), puis l'ÉCHANGE avec la vraie meilleure variante avant de l'afficher : la variante 1
            //    annoncée est ce coup faible, souvent très courte, et son score vaut 0 si sa recherche n'est pas allée au bout
            //    (-VALUE_INFINITE affiché VALUE_ZERO). Une analyse avec la limite donnait donc des "meilleurs coups" absurdes à 0.00.
            // Les options ne sont envoyées que si elles changent (une fois au début d'une analyse, une fois quand le moteur rejoue)
            bool limite = _limiteEloVoulue && !forceMaximale;
            if (limite != _limiteEloDansMoteur && OptionsUci.Contains("UCI_LimitStrength"))
            {
                StandardInputDataToUci("setoption name UCI_LimitStrength value " + (limite ? "true" : "false"));
                _limiteEloDansMoteur = limite;
            }
            if (NiveauVoulu is int niveauVoulu && OptionsUci.Contains("Skill Level"))
            {
                int niveau = forceMaximale ? NiveauMaximal : niveauVoulu;
                if (niveau != _niveauDansMoteur)
                {
                    StandardInputDataToUci("setoption name Skill Level value " + niveau);
                    _niveauDansMoteur = niveau;
                }
            }
        }
        public void DefinitLimiteElo(string ValeurElo)
        {   // Définition de la force ELO du moteur (default 1320 min 1320 max 3190 pour Stockfish)
            StandardInputDataToUci("setoption name UCI_Elo value " + ValeurElo);
        }
        public void SpecialeSargon()
        {   // Sinon Sargon  mouline sans fin !!
            StandardInputDataToUci("setoption name FixedDepth value 6");       
        }
        public void Quitte()
        {   // On ferme le moteur UCI (sans erreur s'il n'a jamais démarré ou s'il a déjà été arrêté, ex : par une mise à jour).
            // Certains moteurs (ex : Sargon 1978) ignorent "quit" : après une seconde d'attente, le processus est arrêté de force,
            // sinon il resterait en mémoire et verrouillerait son .exe
            if (Proc == null)
                return;
            StandardInputDataToUci("quit");
            // Dès maintenant, les dernières lignes du moteur (ex : son bestmove après un "stop") sont ignorées (sender != Proc) :
            // à la fermeture de l'application, elles arrivaient sur une fenêtre détruite (ObjectDisposedException dans le journal)
            Process enArret = Proc;
            Proc = null;
            try
            {
                if (!enArret.WaitForExit(1000))
                    enArret.Kill();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
            {   // processus déjà terminé ou inaccessible : rien à faire
                Debug.WriteLine($"[App] Arrêt du moteur : {ex.Message}");
            }
            enArret.Dispose();
        }
    }
}
