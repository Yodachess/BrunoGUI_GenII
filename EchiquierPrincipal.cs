// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Informations reflexion moteur - Temps de reflexion - Réglage force moteur Stockfish
// Gestion par menus - Sauvegarde PGN - Affichage Score - Personnalisation couleurs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.GestionPartiePgn;
using static BrunoGUI_GenII.LogiqueMouvements;
using static BrunoGUI_GenII.Parametres;

namespace BrunoGUI_GenII
{
    public partial class EchiquierPrincipal : Form
    {
        // Les listes
        private readonly List<LogiqueMouvements.TypePiece> ListeNoire = // Reine, Tour, Fou et Cavalier noirs pour promotion
            [LogiqueMouvements.TypePiece.ReineNoire, LogiqueMouvements.TypePiece.TourNoire, LogiqueMouvements.TypePiece.FouNoir, LogiqueMouvements.TypePiece.CavalierNoir];
        private readonly List<LogiqueMouvements.TypePiece> ListeBlanche =   // Reine, Tour, Fou et Cavalier blancs pour promotion
            [LogiqueMouvements.TypePiece.ReineBlanche, LogiqueMouvements.TypePiece.TourBlanche, LogiqueMouvements.TypePiece.FouBlanc, LogiqueMouvements.TypePiece.CavalierBlanc];
        private List<string> ListeParties = [];
        private List<PartieEchecsPGN> ListePartiesPGN = [];
        private readonly VueEchiquier _vue;     // l'affichage de l'échiquier : cases, pièces, couleurs, inversion (voir VueEchiquier.cs)

        // les variables
        private readonly Partie _partie = new();    // mode de la partie, qui joue quel camp (voir Partie.cs)
        public Partie PartieCourante => _partie;
        private int _indexSource120, _forceMoteurElo;
        private bool _plateauAutorise = true;   // c'est au joueur de bouger les pièces (voir PlateauEnable et MetAJourPlateau)
        private string _caseSource, _caseDestination;
        private string _nomHumain, _joueurElo, _nomMoteur, _moteurElo;
        private bool _nomsHumainMoteur;     // les joueurs affichés sont l'humain et le moteur (voir AfficheJoueursDeLaPartie)
        private string _cheminMoteur, _nomMoteurChoisi;
        private string _bibliotheque = "rodent.bin";
        private bool _clickCaseSource, _visuSymbole, _montreDonneesBrutesUci, _montre3VariantesUci;
        private bool _clavierActif, _emetUnSon, _bibliothèqueAléatoire;
        private bool _bibliothèqueActive = true;
        private int _dureeReflexionMilliSeconde = 5000;
        private LogiqueMouvements.TypePiece _selectionPromotion, _pieceSource;

        // les classes
        public LogiqueMouvements LogiqueMouvements = new();
        public readonly MoteurUci MoteurUci = new();    // le moteur UCI en cours (un seul objet, gardé quand on change de moteur)
        private readonly PiloteMoteur _pilote;      // ce qui est demandé au moteur (coup de partie ou analyse) : voir PiloteMoteur.cs
        public GestionPartiePgn GestionPartiePgn = new();
        public PartieForceModule maNouvellePartieForceModule = new();
        public PartieEchecsPGN PartieEnCours = new();
        public ParametresUciStockfish mesParametresUciStockfish;    // créée dans le constructeur (elle règle MoteurUci)
        public ParametresDeBase mesparametresDeBase;        // mesparametresDeBase est déclarée, mais elle n’est instanciée qu'après "InitializeComponent();"
        private FenetrePartie mafenetrePartie;              // mafenetrePartie est déclarée, mais elle n’est pas encore instanciée. A instancier dans une méthode
        private AffichePgn affichePgn = new();   // affichePgn est à la fois déclarée et instanciée. Prêt à être utilisé dès le début
        private readonly FichierPartiePgn fichierPartiePgn = new();     // liste des parties d'un fichier PGN (masquée, jamais détruite)
        private readonly DonneesBrutesUci donneesBrutesUci = new();
        private readonly Parametres parametres;
        private static readonly System.Windows.Forms.Timer timer1 = new();
        private System.Windows.Forms.Timer timer = timer1;

        public EchiquierPrincipal()
        {
            InitializeComponent();
            InformationsPartie.AutoEllipsis = true;     // message trop long pour le cadre : "…" et texte complet au survol de la souris

            parametres =Parametres.Charger(Chemins.RepertoireRacine);   // BrunoGUI.ini puis préférences personnelles (à côté de l'exécutable)
            MoteurUci.NombreThreads = parametres.NombreCoeursThread;    // envoyés au moteur à son démarrage (voir MoteurUci)
            MoteurUci.TailleHachageMo = parametres.TailleHachageMo;
            // Mise à jour des variables à partir des données du fichier
            _vue = new VueEchiquier(Plateau, this,      // vue côté Blancs au départ ; couleurs du .ini (style Lichess par défaut)
                Parametres.ConvertitCouleur(parametres.CaseClaire, Parametres.LichessCaseClaire),
                Parametres.ConvertitCouleur(parametres.CaseSombre, Parametres.LichessCaseSombre),
                Parametres.ConvertitCouleur(parametres.CouleurCaseSource, Parametres.LichessCaseSource),
                Parametres.ConvertitCouleur(parametres.CouleurCaseDestination, Parametres.LichessCaseDestination));
            _nomHumain = parametres.NomHumain;
            _joueurElo = parametres.EloHumain;
            _dureeReflexionMilliSeconde = parametres.DureeReflexionSeconde * 1000;
            _nomMoteur = parametres.Moteur;
            _forceMoteurElo = parametres.ForceMoteur;
            MoteurUci.NombreLignesPV = parametres.NombreLignesPV;
            _bibliotheque = parametres.Bibliotheque;
            _moteurElo = _forceMoteurElo.ToString();
            // Préférences : la fenêtre "Nouvelle partie" propose les derniers choix, le curseur reprend le dernier temps de réflexion
            maNouvellePartieForceModule.ChoixCouleur = parametres.CouleurMoteur;
            maNouvellePartieForceModule.ForceMaximale = parametres.ForceMaximale;
            maNouvellePartieForceModule.ForceModule = parametres.ForceMoteur;
            maNouvellePartieForceModule.DureeReflexionSeconde = parametres.DureeReflexionSeconde;
            maNouvellePartieForceModule.NomAdversaire = parametres.NomHumain;
            TempsReflexionSecondes.Value = Math.Clamp(parametres.DureeReflexionSeconde, (int)TempsReflexionSecondes.Minimum, (int)TempsReflexionSecondes.Maximum);
            foreach (Cadence cadence in Cadence.Proposees)
                ListePendule.Items.Add(cadence);
            ChoisitCadence(parametres.Cadence);
            _minuteriePendule.Tick += MinuteriePendule_Tick;
            PenduleBlanc.Click += Pendule_Click;        // un clic sur une pendule : pause / reprise
            PenduleNoir.Click += Pendule_Click;
            PenduleBlanc.ReduitPourTenir = PenduleNoir.ReduitPourTenir = true;     // "1:30:00" toujours lisible en entier
            AffichePendules();      // pas de pendule au départ : "-:--"
            // Debug pour vérifier
            Debug.WriteLine($"Paramètres chargés : Biblio = {_bibliotheque}, Force = {_forceMoteurElo}, Nombre PV = {MoteurUci.NombreLignesPV}");
            Debug.WriteLine($"Paramètres chargés : Temps de réflexion = {_dureeReflexionMilliSeconde}");

            DateTime Aujourdhui = DateTime.Today;
            _montreDonneesBrutesUci = _clavierActif = false;
            _pilote = new PiloteMoteur(MoteurUci)
            {   // Bibliothèque d'ouvertures (si elle est active) : le coup choisi est aussi affiché dans la liste de la bibliothèque
                ChoixBibliotheque = fen => _bibliothèqueActive ? ChoisirCoupBibliotheque(fen) : null
            };
            PartieEnCours.Date = Aujourdhui.ToString("yyyy.MM.dd");
            PartieEnCours.Lieu = "Maison"; PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Result = "*";
            AfficheJoueursDeLaPartie();     // avant toute partie : l'humain avec les Blancs, le moteur avec les Noirs (étiquettes et en-tête PGN)
            mesparametresDeBase = new ParametresDeBase(this);
            mesParametresUciStockfish = new ParametresUciStockfish(MoteurUci);
            fichierPartiePgn.VisibleChanged += (s, e) => MetAJourBoutonListeParties();   // bouton "Affiche/Masque liste parties"
            // Les options suivent l'état initial des cases à cocher du designer (le son était inversé : case cochée, son coupé)
            _emetUnSon = ActiveSon.Checked;
            _bibliothèqueActive = ActiveBibliothèque.Checked;
            _bibliothèqueAléatoire = ActiveAléatoire.Checked;
        }

        private void BrunoInterfaceGraphique_Load(object sender, EventArgs e)
        {   // Forme Interface graphique
            // les évènements dans les classes
            LogiqueMouvements.AfficheCoupNoir += CoupJoue;
            LogiqueMouvements.AfficheCoupBlanc += CoupJoue;
            LogiqueMouvements.AfficheInfoEchec += AfficheInfoEchec;
            LogiqueMouvements.AfficheEchecEtMat += AfficheEchecEtMat;
            LogiqueMouvements.AffichePat += AffichePat;
            LogiqueMouvements.AfficheTour += AfficheTour;
            LogiqueMouvements.ChoixPromotion = AffichePromotionPion;
            LogiqueMouvements.DessinePiece += DessinePieceDeLaPartie;     // ignoré pendant le parcours de la partie
            LogiqueMouvements.DessineSymbole += DessineSymbole;
            MoteurUci.AfficheUci += AfficheUci;
            MoteurUci.AfficheDonneesBrutes += AfficheDonneesBrutes;
            MoteurUci.AfficheCoupMoteur += AfficheCoupMoteur;
            this.KeyPreview = true; // <-- obligatoire pour capter toutes les touches
            // Les images des pièces et les couleurs des cases sont dans VueEchiquier (couleurs du .ini, voir le constructeur)

            _cheminMoteur = CheminStockfish;            // moteur lancé au démarrage
            Debug.WriteLine("Dossier Racine = " + Chemins.RepertoireRacine);
            Debug.WriteLine("Chemin moteurs = " + Chemins.MoteursUCI);
            Debug.WriteLine("Chemin Polyglot = " + Chemins.BibliothèquesPolyglot);

            InformationPourJoueur.Text = "   Bienvenue   ";

            _vue.CreerCases(CaseMouseDown);     // les 120 cases (64 visibles), indexées comme le tableau "mailbox"
            LogiqueMouvements.InitialisationEchiquier();
            RécupèreBibliothèque();
            MiseaZeroAffichages();
            QuiJoue = ColorPiece.Blanc;
            this.ActiveControl = Plateau;       // Met le focus sur le plateau pour éviter le Bug des radiobutton "Résultat"
            MetAJourCommandes();                // aucune partie : seuls les menus sont utiles

            ActiverMenus(false);    // On désactive les menus après la mise à jour
            VarianteMoteurUci2.Text = "     ---       [INFO] Vérification initiale de mise à jour de Stockfish...      ---";
            _ = Task.Run(async () =>            // MISE A JOUR STOCKFISH SI ELLE EXISTE
           {   // Vérification de la mise à jour de Stockfish, puis lancement du moteur
                
               try
               {   // A. Vérification (au plus tous les 30 jours) et, après accord, installation ; on ATTEND qu'elle finisse
                   await VerificationAutomatiqueMiseAJour();
               }
               catch (Exception ex)
               {   // On log juste, on ne bloque pas le démarrage si la MAJ échoue (ex: hors ligne)
                   Debug.WriteLine("[INFO] Pas de mise à jour effectuée : " + ex.Message);
                   // VarianteMoteurUci1.Text = "[INFO] Pas de mise à jour effectuée : ";
               }
                    // B. MAINTENANT, on démarre le moteur. 
                    // Le fichier est libre, remplacé et prêt.
               Debug.WriteLine("chemin Load = " + _cheminMoteur);

               MoteurUci.Start(_cheminMoteur);
               ActiverMenus(true);    // On réactive les menus après la mise à jour
           });
            Debug.WriteLine("Moteur = " + _nomMoteur);
            // VarianteMoteurUci2.Text = "[INFO] Fin de la vérification de mise à jour de Stockfish...";
        }

        private void NouvellePartieStockfish_Click(object sender, EventArgs e)
        {   // Nouvelle partie contre Stockfish, avec la possibilité de régler la force du moteur et le temps de réflexion.
            // Rien ne change avant la validation : "Annuler" laisse la partie en cours intacte (et le moteur continue à réfléchir)
            if (maNouvellePartieForceModule.ShowDialog() == DialogResult.OK)
            {   // Utilise les sélections faites par l'utilisateur
                AbandonneReflexion();   // nouvelle partie
                QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
                DémarreStockfish();
                ColorPiece couleurMoteur = maNouvellePartieForceModule.ChoixCouleur;
                bool forceMaximale = maNouvellePartieForceModule.ForceMaximale;
                _forceMoteurElo = maNouvellePartieForceModule.ForceModule;
                _nomHumain = maNouvellePartieForceModule.NomAdversaire;     // mémorisé dans les préférences à la fermeture
                _dureeReflexionMilliSeconde = maNouvellePartieForceModule.DureeReflexionSeconde * 1000;
                TempsReflexionSecondes.Value = Math.Clamp(maNouvellePartieForceModule.DureeReflexionSeconde, (int)TempsReflexionSecondes.Minimum, (int)TempsReflexionSecondes.Maximum);
                ChoisitCadence(maNouvellePartieForceModule.ChoixCadence);     // la pendule de cette partie (et des suivantes)

                MiseaZeroAffichages();
                if (forceMaximale)
                {   // Moteur à sa force Elo maximale
                    _forceMoteurElo = 3150;
                }
                _moteurElo = _forceMoteurElo.ToString();
                MoteurUci.ActiveLimiteElo();
                MoteurUci.DefinitLimiteElo(_moteurElo);
                MoteurUci.DefinitMultiPV(MoteurUci.NombreLignesPV);
                if (couleurMoteur == ColorPiece.Blanc)
                {   // Le moteur joue les blancs
                    CommencerPartie(Joueur.Moteur, Joueur.Humain);
                    AfficheJoueursDeLaPartie();
                    if (!_vue.CoteNoir)
                        TourneEchiquier();      // On met la vue côté Noir
                    ParametresJoueurHumain("Le moteur UCI joue");      // On fait jouer le moteur côté blanc
                    JeuMoteurAvecBibliothèque(FenDepart);
                }
                else
                {   // Le moteur joue les noirs
                    if (_vue.CoteNoir)
                        TourneEchiquier();
                    CommencerPartie(Joueur.Humain, Joueur.Moteur);
                    AfficheJoueursDeLaPartie();
                    ParametresJoueurHumain("A vous de jouer");            // On demande à l'humain de jouer
                    PlateauEnable(true);                                            // On lui permet de bouger les pièces
                }
            }
        }
        private void CommencerPartie(Joueur blancs, Joueur noirs)      // POINT D'ENTREE POUR TOUTES LES NOUVELLES PARTIES
        {   // Début d'une nouvelle partie : qui joue les Blancs et les Noirs (humain ou moteur)
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            _partie.Commencer(blancs, noirs);
            _vue.EffaceDernierCoup();   // les cases du dernier coup de la partie précédente
            _clickCaseSource = _visuSymbole = true;
            PartieEnCours.CoupsPartiePGN = PartieEnCours.Result = PartieEnCours.CompteDePLy = PartieEnCours.Ronde = "";
            PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Lieu = "Maison";
            MiseaZeroAffichages();
            MiseaZéroTimer();
            VarianteMoteurUci1.Text = string.Empty;
            if (!_partie.EntreHumains)
                InformationPourJoueur.Visible = true;     // le message est donné ensuite par ParametresJoueurHumain
            else
            {   // (les noms des joueurs sont fixés par HumainContreHumain_Click)
                PlateauEnable(true);
                InformationPourJoueur.Visible = true;
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux Blancs de jouer";
            }
            AfficheCoupsBibliotheque(FenDepart);
            NouvellePendule();          // cadence choisie (Sans pendule : temps fixe par coup, comme avant)
            MetAJourCommandes();
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion du click de la souris
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void CaseMouseDown(object sender, MouseEventArgs e)
        {   // Le joueur sélectionne la case source ou destination avec la souris
            try
            {
                if (PartieEnLectureSeule)
                    return;     // partie PGN chargée : on ne joue pas de coup
                if (ParcoursEnCours)
                {   // On regardait un coup passé : le clic ramène à la position courante de la partie (il ne joue pas de coup)
                    RetourPositionCourante();
                    return;
                }
                if (sender is PictureBox CaseClick)
                {
                    int IndexCase120 = _vue.IndexDeLaCase(CaseClick);    // (tient compte de la vue côté Noirs)
                    if (_partie.Mode == ModePartie.AucunePartie)     // au lancement, aucune partie choisie : pas de coup
                        KryptonMessageBox.Show("Veuillez choisir une partie :\n\n" +
                            "   •  Stockfish : jouer contre Stockfish (force réglable)\n" +
                            "   •  Nouvelle Partie : jouer contre le moteur choisi, ou entre amis", "Aucune partie en cours", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    else
                    {
                        if (_clickCaseSource)     // Permet de savoir si c'est la sélection de la pièce ou le déplacement
                        {        // Sélection d'une pièce
                            _indexSource120 = IndexCase120;
                            _caseSource = LogiqueMouvements.NomCaseAlgebrique(_indexSource120);
                            _pieceSource = LogiqueMouvements.PiecesEchiquier[_indexSource120];
                            if (_vue.ImageCase(IndexCase120) != null)
                            {   // Si la case cliquée contient bien une pièce ou un pion, on va utiliser le thumbnail de la pièce comme curseur :-)
                                _vue.MetCurseurPiece(_vue.ImageCase(IndexCase120));     // la pièce suit la souris
                                _vue.DessinePiece(IndexCase120, LogiqueMouvements.TypePiece.Vide);   // On vide la case d'origine car le joueur bouge la pièce ...
                                LogiqueMouvements.DessineMouvements(_caseSource, true);
                                _clickCaseSource = false;
                            }
                        }
                        else
                        {       // Déplacement d'une pièce
                            LogiqueMouvements.Echec = false;
                            _vue.LibereCurseurPiece();      // On revient au curseur "normal"
                            LogiqueMouvements.EffaceSymboles(true);
                            _caseDestination = LogiqueMouvements.NomCaseAlgebrique(IndexCase120);
                            AbandonneReflexion();   // une analyse en cours porterait sur la position d'avant ce coup
                            LogiqueMouvements.ExecutionCoup(_caseSource, _caseDestination);
                            string chaineFen = LogiqueMouvements.RetourneChaineFenActuel(); // UCI : remplacer le FEN par liste de coups ?!
                            if (LogiqueMouvements.CoupValide)
                            {   // envoi de la Position Fen au moteur UCI
                                AfficheCoupsBibliotheque(chaineFen);
                                if (_partie.MoteurAuTrait)      // c'est au moteur de répondre (pas après un mat ou un pat : partie terminée)
                                {
                                    JeuMoteurAvecBibliothèque(chaineFen);
                                }
                            }
                            else
                            {   // Si le coup n'est pas valide, on remet la pièce sur sa case d'origine !
                                _vue.DessinePiece(_indexSource120, _pieceSource);
                            }
                            _clickCaseSource = true;
                            _vue.EffaceDernierCoup();
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Erreur : sender n'est pas une PictureBox.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erreur dans CaseMoveDown : " + ex.Message);
                Debug.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Procédures d'affichage diverses
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void AfficheUci()   // Affiche les informations du moteur UCI (la ligne est déjà décodée dans MoteurUci.DerniereLigne)
        {   // ATTENTION : MALGRE LA PRESENCE DU PROTOCOLE UCI, LES MOTEURS ONT DES REPONSES DIFFERENTES !!?? (voir case "info", par ex)
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheUci));
                return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
            }
            LigneUci ligne = MoteurUci.DerniereLigne;
            if ((ligne.Commande == "info" || ligne.Commande == "bestmove") && MoteurUci.LigneAbandonnee)
                return;     // réponse à une demande abandonnée (retour arrière, nouvelle partie...) : on n'affiche rien
            switch (ligne.Commande)     // identifier le premier mot
            {
                case "bestmove":        // **** le moteur UCI propose le meilleur coup ! ****
                    if (ligne.AucunCoupLegal)
                    {   // Cas particulier : le moteur retourne "bestmove (none)" ou "bestmove 0000" => partie terminée (mat ou pat).
                        // Le camp au trait est-il en échec ? (calculé ici, sur la position affichée, pas sur un état d'échec éventuellement ancien)
                        bool mat = LogiqueMouvements.CampAuTraitEnEchec();
                        if (mat)
                            LogiqueMouvements.EchecetMat = true;
                        VarianteMoteurCourante.Text = mat ? "Aucun coup légal : échec et mat" : "Aucun coup légal : pat";
                        _pilote.ReponseRecue();     // la demande est terminée, sans coup à jouer (AfficheCoupMoteur n'est pas appelé)
                        break;
                    }
                    VarianteMoteurCourante.Text = "Coup joué : " + Outils.VarianteUciVersPgn(ligne.MeilleurCoup, LogiqueMouvements.DemiCoupAvant(PositionDesVariantes), false, PositionDesVariantes) +
                        (ligne.CoupConseil != null ? "   (Conseil : " + Outils.VarianteUciVersPgn(ligne.MeilleurCoup + " " + ligne.CoupConseil, LogiqueMouvements.DemiCoupAvant(PositionDesVariantes), true, PositionDesVariantes) + ")" : "");  // Le conseil (ponder) se joue après le coup du moteur
                    break;
                case "id":
                    if (ligne.NomMoteur != null)
                    {   // Récupération du nom du moteur (limité à 20 caractères pour l'affichage)
                        _nomMoteur = ligne.NomMoteur[..Math.Min(20, ligne.NomMoteur.Length)];
                        // Le nom va au(x) camp(s) joué(s) par le moteur, jamais à un joueur humain
                        // (ex : partie PGN chargée, ou moteur qui joue les Blancs et redémarre après une mise à jour)
                        AfficheMoteurDansLaPartie();
                    }
                    if (ligne.AuteurMoteur != null)     // Récupération de l'auteur
                        VarianteMoteurUci3.Text = "     Auteur(s) du moteur " + _nomMoteur + " = " + ligne.AuteurMoteur;
                    break;
                case "info":            // **** Infos de réflexion moteur ****
                    AfficheInfoMoteur(ligne);
                    break;
            }
        }

        private void AfficheInfoMoteur(LigneUci ligne)
        {   // Affiche le score et la variante d'une ligne "info" du moteur (décodées par _pilote.Lignes, du point de vue des Blancs)
            if (ligne.DansBibliotheque)
                VarianteMoteurUci1.Text = "    Le moteur est dans sa bibliothèque d'ouvertures";

            LigneAnalyse ligneAnalyse = _pilote.Lignes.Ajouter(ligne, PositionDesVariantes);
            if (ligneAnalyse == null)
                return;     // ni score ni variante (ex : "info depth 12")
            // On affiche seulement le score de la meilleure variante (un moteur sans MultiPV, comme Sargon, n'a que celle-là)
            if (ligneAnalyse.Numero == 1 && ligneAnalyse.Evaluation is Evaluation evaluation)
            {
                EvaluationUci.Text = evaluation.Appreciation;
                if (evaluation.EstUnMat)
                {
                    ScoreMoteur.Text = "MAT en " + Math.Abs(evaluation.MatEn.Value);
                    InformationPourJoueur.Text = evaluation.TexteMat;      // "MAT en 3 pour les Blancs"
                }
                else
                    ScoreMoteur.Text = "Score : " + evaluation.Texte;
            }
            if (ligne.Variante == null)
                return;

            // Affichage de la variante
            string texteVariante = ligneAnalyse.Symbole + " (" + ligneAnalyse.Debut + ") █[ " + ligneAnalyse.TexteScore + " ]█  " + "[ " + ligneAnalyse.VariantePgn + " ]";
            if (ligne.NumeroVariante is int numeroVariante)
            {   // Une zone d'affichage par variante (VarianteMoteurUci1, 2, 3) ; les variantes au-delà ne sont pas affichées
                if (Controls.Find("VarianteMoteurUci" + numeroVariante, true).FirstOrDefault() is RichTextBox zoneVariante)
                    zoneVariante.Text = " " + texteVariante;
            }
            else
            {   // Pour ceux qui n'ont qu'une variante principale (Sargon, ...) : on n'utilise que la zone VarianteMoteurUci1
                VarianteMoteurUci1.Text = texteVariante;
                VarianteMoteurUci2.Text = "... " + _nomMoteurChoisi + " n'affiche qu'une variante ..."; VarianteMoteurUci3.Text = "...";
            }
        }

        private void AfficheDonneesBrutes()
        {   // Affiche les données brutes du moteur UCI, pour les curieux qui veulent voir ce qui se passe "sous le capot" :-)
            // Le texte est pris tout de suite (la ligne suivante remplacera DataUci) ; l'affichage est envoyé à l'interface
            // sans l'attendre (BeginInvoke), pour ne pas ralentir la lecture des lignes du moteur
            string texte;
            if (MoteurUci.UciVersGui)
            {
                if (MoteurUci.DataUci.Contains("currmove"))     // Inutile d'afficher les currmove, il n'y rien d'intéressant ...
                    return;
                texte = "[" + _nomMoteur + "]    " + MoteurUci.DataUci;
            }
            else
                texte = " [BrunoGUI_GenII]    " + MoteurUci.DataVersUci;
            if (!InvokeRequired)
                donneesBrutesUci.AjouteLigne(texte);
            else if (IsHandleCreated && !IsDisposed)
            {
                try
                {
                    BeginInvoke(new MethodInvoker(() => donneesBrutesUci.AjouteLigne(texte)));
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
                {   // fenêtre en cours de fermeture : la ligne n'est plus affichée
                    Debug.WriteLine("AfficheDonneesBrutes : " + ex.Message);
                }
            }
        }

        private void AfficheCoupMoteur()
        {   // Le moteur UCI joue son meilleur coup
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheCoupMoteur));
                return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
            }
            else
            {
                if (MoteurUci.LigneAbandonnee)
                    return;     // la demande a été abandonnée entre-temps (vérifié ici, sur le thread de l'interface) : coup ignoré
                MiseaZéroTimer();
                if (_emetUnSon)
                {   // Son pour dire que le coup est joué (son système par défaut si le fichier de Windows est absent)
                    try
                    {
                        SoundPlayer player = new(@"C:\Windows\Media\Windows Notify.wav");
                        player.Play();
                    }
                    catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException)
                    {
                        SystemSounds.Asterisk.Play();
                    }
                }
                TypeDemande demande = _pilote.ReponseRecue();   // à quelle demande répond ce bestmove ?
                if (demande == TypeDemande.CoupDePartie)
                {   // Coup de la partie : on le joue (promotion comprise)
                    StatusProgramme.Text = InformationPourJoueur.Text = "A vous de jouer";
                    _caseSource = MoteurUci.CoupAuFormatUci[..2];    // CoupAuFormatUci contient le "best move" sous la forme e2e4
                    _caseDestination = MoteurUci.CoupAuFormatUci.Substring(2, 2);
                    PiloteMoteur.JouerCoupUci(MoteurUci.CoupAuFormatUci);      // (rien n'est joué si la position est déjà un mat)
                    // Cases de départ et d'arrivée du coup colorées (pendant le parcours : seulement au retour à la partie)
                    _vue.MontreDernierCoup(RenvoieCaseIndex120(_caseSource), RenvoieCaseIndex120(_caseDestination));
                    LeMoteurARépondu();      // On réautorise si le moteur a fini de réfléchir
                    AfficheCoupDuMoteur();
                    if (ParcoursEnCours)
                    {   // Le coup est joué dans la partie, mais l'affichage reste sur la position passée que l'utilisateur regarde
                        StatusProgramme.Text = "Le moteur a joué";
                        InformationsPartie.Text = "Le moteur a joué : Fin pour revenir";
                    }
                }
                else if (demande == TypeDemande.Analyse)
                {   // c'est une analyse : on affiche la meilleure variante (mémorisée par _pilote.Lignes, pas relue dans le texte affiché)
                    LigneAnalyse meilleure = _pilote.Lignes.Meilleure;
                    InformationPourJoueur.Text = StatusProgramme.Text = "Analyse terminée ... ";
                    string titre = "Analyse Moteur (" + _dureeReflexionMilliSeconde / 1000 + " sec.) par " + _nomMoteur;
                    if (meilleure?.VariantePgn == null)
                        _ = KryptonMessageBox.Show("Le moteur n'a donné aucune variante.", titre, KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    else
                    {
                        string appreciation = meilleure.Evaluation?.Appreciation ?? "évaluation inconnue";
                        ScoreMoteur.Text = "Score = " + meilleure.TexteScore;
                        EvaluationUci.Text = appreciation;
                        VarianteMoteurCourante.Text = InformationsPartie.Text = "Coup suggéré : " + meilleure.Debut;
                        _ = KryptonMessageBox.Show("La meilleure suite est : " + meilleure.Debut +
                                            "\n Evaluation --- " + meilleure.TexteScore + " --- (" + appreciation + ")" +
                                            "\n" + meilleure.VariantePgn, titre, KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    }
                    MetAJourCommandes();
                }
                // (aucune demande en cours : bestmove après un "stop" sans demande, ignoré)
            }
        }

        private void AfficheCoupDuMoteur()
        {   // Le moteur vient de jouer (réflexion ou bibliothèque) : son coup dans le cadre vert, ex : "Coup joué : 23. Ce6".
            // Pas si la partie vient de finir (le cadre montre alors le résultat)
            if (_partie.EnCours && LogiqueMouvements.ListeCoups.Count > 0 && !LogiqueMouvements.ListeCoups[^1].EstPositionDeDepart)
                InformationsPartie.Text = "Coup joué : " + LogiqueMouvements.ListeCoups[^1].PgnFrNumerote;
        }
        private void CoupJoue(string coup)
        {   // Un coup (blanc ou noir) vient d'être joué dans la partie (événements AfficheCoupBlanc et AfficheCoupNoir)
            if (LogiqueMouvements.EchecetMat)
            {
                StatusProgramme.Text = "Partie terminée";
                return;
            }
            PartieEnCours.CompteDePLy = LogiqueMouvements.ListeCoupsFen.Count.ToString();
            if (_pendule?.CampQuiDecompte == null)
                _pendule?.Demarrer(QuiJoue);    // premier coup de la partie : la pendule part pour l'adversaire (rien n'est décompté avant)
            else if (!_pendule.CoupJoue(LogiqueMouvements.ListeCoups[^1].NumeroDuCoup) && _pendule.TempsEcoule() is ColorPiece campSansTemps)
            {   // Coup joué alors que le temps était déjà écoulé (entre deux tics de la minuterie) : la partie est perdue au temps
                PerteAuTemps(campSansTemps);
                return;
            }
            if (_pendule != null)
                _pendule.NoteTemps(LogiqueMouvements.ListeCoups[^1]);   // pour le retour arrière et "Reprendre ici"
            AffichePendules();
            string raisonNulle = _partie.RejeuPgn ? null : LogiqueMouvements.RaisonNulle();    // répétition, 50 coups ou matériel insuffisant
            if (raisonNulle != null)
                GestionResultat("1/2-1/2", raisonNulle);
            MetAJourCommandes();    // un coup a été joué : on peut parcourir la partie, la liste des coups, le retour arrière...
        }

        private void AfficheTour(ColorPiece couleur)
        {   // Affiche le camp au trait entre humains ; sinon active les cases si c'est au tour du joueur humain
            if (_partie.EntreHumains)
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux " + NomCamp(couleur) + " de jouer";
            else
                PlateauEnable(_partie.JoueurDe(couleur) == Joueur.Humain);
        }

        private void AfficheEchecEtMat(ColorPiece couleurMatee)
        {   // Affiche l'échec et mat du camp en paramètre, et gère la fin de partie
            // (le "#" du mat est déjà dans les notations du dernier coup : voir LogiqueMouvements.ExecutionCoup)
            if (couleurMatee == ColorPiece.Blanc)
                GestionResultat("0-1", " Gain Noir");
            else
                GestionResultat("1-0", " Gain Blanc");
            InformationPourJoueur.Text = VarianteMoteurCourante.Text = "Le Roi " + NomCouleur(couleurMatee) + " est échec et mat";
            StatusProgramme.Text = "Partie terminée";
            Application.DoEvents();
            PlateauEnable(false);
        }

        private void AfficheInfoEchec(string infoechec)
        {   // Affiche les informations d'échec ou de pat dans l'étiquette (le pat lui-même est traité par AffichePat)
            InformationsPartie.Text = infoechec;
            InformationsPartie.ForeColor = Color.DarkGreen;
        }

        private void AffichePat(ColorPiece couleurPat)
        {   // Un des joueurs est pat : fin de la partie
            GestionResultat("1/2-1/2", "Pat (Nulle)");
            InformationPourJoueur.Text = "Pat (Nulle)";
            PlateauEnable(false);
        }
        private LogiqueMouvements.TypePiece AffichePromotionPion(LogiqueMouvements.ColorPiece couleur)
        {   // Promotion d'un pion de l'humain : on affiche les pièces disponibles pour la promotion, on attend que le joueur
            // clique sur une pièce, et on renvoie son choix (Vide si la fenêtre se ferme : la logique promeut alors en dame)
            _selectionPromotion = LogiqueMouvements.TypePiece.Vide;
            List<LogiqueMouvements.TypePiece> pieces = couleur == LogiqueMouvements.ColorPiece.Noir ? ListeNoire : ListeBlanche;    // dame, tour, fou, cavalier
            Promo0.Image = _vue.ImagePiece(pieces[0]);
            Promo1.Image = _vue.ImagePiece(pieces[1]);
            Promo2.Image = _vue.ImagePiece(pieces[2]);
            Promo3.Image = _vue.ImagePiece(pieces[3]);
            GroupPromo.Visible = true;
            // Pendant le choix, tout le reste est inactif (menus, boutons, échiquier, flèches du clavier) : une nouvelle partie,
            // un chargement... lancés au milieu de ce coup corromperaient la partie. L'état de chaque contrôle est rétabli ensuite
            Dictionary<Control, bool> etatsAvantChoix = [];
            foreach (Control controle in Controls)
                if (controle != GroupPromo)
                {
                    etatsAvantChoix[controle] = controle.Enabled;
                    controle.Enabled = false;
                }
            bool clavierAvantChoix = _clavierActif;
            _clavierActif = false;
            try
            {
                while (_selectionPromotion == LogiqueMouvements.TypePiece.Vide && !IsDisposed)
                {   // Attente du clic sur une pièce : la courte pause évite d'occuper le processeur à 100 % pendant l'attente
                    Application.DoEvents();
                    Thread.Sleep(15);
                }
            }
            finally
            {
                if (!IsDisposed)
                {
                    GroupPromo.Visible = false;
                    foreach (var (controle, actif) in etatsAvantChoix)
                        controle.Enabled = actif;
                    _clavierActif = clavierAvantChoix;
                }
            }
            return _selectionPromotion;
        }

        private void BoutonGainBlanc_Click(object sender, EventArgs e)
        {   // Si un des joueurs abandonne, c'est la règle de l'abandon
            GestionResultat("1-0", " Gain Blanc");
        }
        private void BoutonGainNoir_Click(object sender, EventArgs e)
        {   // Si un des joueurs abandonne, c'est la règle de l'abandon
            GestionResultat("0-1", " Gain Noir");
        }
        private void BoutonNulle_Click(object sender, EventArgs e)
        {   // Si un des joueurs propose la nulle et que l'autre accepte, c'est la règle de la nulle par accord mutuel
            GestionResultat("1/2-1/2", " Nulle");
        }
        private void GestionResultat(string resultat, string vainqueur)
        {   // Fin de partie : on affiche le résultat et le vainqueur, on désactive les boutons de gain/nulle,
            AbandonneReflexion();   // résultat déclaré : le coup en cours de réflexion ne doit pas être joué
            // on empêche de bouger les pièces, on affiche le résultat dans les données de la partie
            StopMoteur_Click(null, EventArgs.Empty);    // Au cas où le moteur tourne encore ?!
            PartieEnCours.Result = EvaluationUci.Text = resultat;
            ScoreMoteur.Text = vainqueur;
            InformationsPartie.Text = resultat + "  (" + vainqueur + ")";
            StatusProgramme.Text = "Partie terminée";
            _partie.Terminer();     // le retour arrière reste possible pour reprendre la partie (Partie.AnnulerDernierCoup)
            _pauseJoueur = false;   // (ex : abandon déclaré pendant une pause)
            _pendule?.Arreter();    // les temps restent affichés
            AffichePendules();
            PlateauEnable(false);
            MetAJourCommandes();
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des menus
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void HumainOrdinateur_Click(object sender, EventArgs e)
        {   // L'humain joue les blancs, l'ordinateur les noirs.
            // On demande d'abord confirmation (la partie est remise à zéro) : "Annuler" laisse la partie en cours intacte
            string confirmation = "Vous aurez les Blancs contre " + _nomMoteur + ". " + "\nToute position précédente sera effacée,\n confirmez avec OK, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Le joueur a les Blancs, l'ordinateur les Noirs ", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                AbandonneReflexion();   // nouvelle partie
                QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
                StatusProgramme.Text = ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "";    // On efface les données de la partie précédente
                CommencerPartie(Joueur.Humain, Joueur.Moteur);
                AfficheJoueursDeLaPartie();
                if (_vue.CoteNoir)
                    TourneEchiquier();
                AfficheCoupsBibliotheque(FenDepart);
                ParametresJoueurHumain("A vous de jouer");            // On demande à l'humain de jouer
                PlateauEnable(true);                                            // On lui permet de bouger les pièces
            }
        }
        private void OrdinateurHumain_Click(object sender, EventArgs e)
        {   // L'ordinateur joue les blancs, l'humain les noirs.
            // On demande d'abord confirmation (la partie est remise à zéro) : "Annuler" laisse la partie en cours intacte
            string confirmation = "Vous aurez les Noirs contre " + _nomMoteur + ". " + "\nToute position précédente sera effacée,\n confirmez avec OK, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Le joueur a les Noirs, l'ordinateur les Blancs ", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                AbandonneReflexion();   // nouvelle partie
                QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
                StatusProgramme.Text = ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "";     // On efface les données de la partie précédente
                CommencerPartie(Joueur.Moteur, Joueur.Humain);
                AfficheJoueursDeLaPartie();
                if (!_vue.CoteNoir)
                    TourneEchiquier();                                          // On met la vue côté Noir
                ParametresJoueurHumain("Le moteur UCI joue");
                JeuMoteurAvecBibliothèque(FenDepart);
            }
        }
        private void HumainContreHumain_Click(object sender, EventArgs e)
        {   // 2 joueurs humains s'affrontent, pas de moteur UCI.
            // On demande d'abord confirmation (la partie est remise à zéro) : "Annuler" laisse la partie en cours intacte
            string confirmation = "Vous jouez contre votre ami/partenaire,\n" + "ou vous saisissez une partie ...\n" +
                "Toute position précédente sera effacée,\n confirmez avec OK, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Jeu entre amis, ou saisie de partie", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                AbandonneReflexion();   // nouvelle partie
                QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
                AfficheJoueurs(_nomHumain, _joueurElo, "Adversaire", "");
                StatusProgramme.Text = "Humain contre humain";
                InformationsPartie.Text = " Bruno vous souhaite une bonne partie !";
                MoteurUci.ActiveLimiteElo();        // Préparation du moteur en cas de demande d'analyse
                MoteurUci.DefinitLimiteElo("3190");
                MoteurUci.DefinitMultiPV(MoteurUci.NombreLignesPV);
                maNouvellePartieForceModule.DureeReflexionSeconde = 10;
                CommencerPartie(Joueur.Humain, Joueur.Humain);
            }
        }
        private void SelectionAutreMoteur_Click(object sender, EventArgs e)
        {   // l'utilisateur doit sélectionner le répertoire et fichier du moteur UCI
            DialogResult Reponse = OuvertureChoixMoteur.ShowDialog();
            if (Reponse == DialogResult.OK)     // On ne sauvegarde que si l'utilisateur a choisi un fichier
            {
                _cheminMoteur = OuvertureChoixMoteur.FileName;         // Récupère le chemin du moteur UCI
                Debug.WriteLine("Chemin Sélection Moteur = " + _cheminMoteur);
                DémarrageMoteur();
            }
        }
        private void SelectionBibliothèque_Click(object sender, EventArgs e)
        {   // l'utilisateur doit sélectionner le répertoire et fichier de la bibliothèque d'ouvertures
            DialogResult Reponse = OuvertureChoixBibliothèque.ShowDialog();
            if (Reponse == DialogResult.OK)     // l'utilisateur doit sélectionner le répertoire et fichier
            {
                string precedente = _bibliotheque;
                _bibliotheque = OuvertureChoixBibliothèque.FileName;
                if (!RécupèreBibliothèque())
                    _bibliotheque = precedente;     // fichier illisible : on garde la bibliothèque précédente (et son nom dans les préférences)
            }
        }
        private void RodentIV_Click(object sender, EventArgs e)
        {   //  https://echecs-et-informatique.franceserv.com/rodent-iv.html
            _moteurElo = "+- 3000";
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\Rodent_IV", "rodent-iv-x64.exe");
            Debug.WriteLine("Chemin Rodent IV = " + _cheminMoteur);
            DémarrageMoteur();
        }
        private void Sargon1_1978_Click(object sender, EventArgs e)
        {   // https://echecs-et-informatique.franceserv.com/sargon-1978.html
            _moteurElo = "1678";
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\sargon1978", "sargon1978_1_01b.exe");
            Debug.WriteLine("Chemin sargon I 1978 = " + _cheminMoteur);
            DémarrageMoteur();
            MoteurUci.SpecialeSargon();         // Sinon Sargon  mouline sans fin !!!!!
        }
        public void DémarreStockfish()
        {   //  https://stockfishchess.org/
            _moteurElo = "+- 3000";
            _cheminMoteur = CheminStockfish;
            Debug.WriteLine("Chemin Stockfish = " + _cheminMoteur);
            DémarrageMoteur();
        }
        private void DémarrageMoteur()
        {   // Arrête le moteur UCI s'il est déjà en cours d'exécution, pour éviter les conflits
            AbandonneReflexion();   // changement de moteur
            // (le dossier de travail du moteur est celui de son .exe : voir MoteurUci.Start)
            MoteurUci.Quitte();
            MoteurUci.Start(_cheminMoteur); // on démarre le nouveau moteur Uci
            _nomMoteurChoisi = Path.GetFileNameWithoutExtension(_cheminMoteur);
            Debug.WriteLine("Moteur = " + _nomMoteurChoisi);
            _nomMoteur = _nomMoteurChoisi;      // en attendant le nom annoncé par le moteur ("id name", voir AfficheUci)
            AfficheMoteurDansLaPartie();
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Pendule (voir Pendule.cs)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private Cadence _cadence = Cadence.SansPendule;     // cadence choisie : elle vaut pour la PROCHAINE nouvelle partie
        private Pendule _pendule;                           // pendule de la partie en cours (null : sans pendule, temps fixe par coup)
        private readonly Stopwatch _chrono = Stopwatch.StartNew();      // heure de la pendule (précise, indépendante des tics)
        private readonly System.Windows.Forms.Timer _minuteriePendule = new() { Interval = 100 };   // affichage et chute du drapeau

        private bool _pauseJoueur;      // pause demandée par un clic sur une pendule (à distinguer de la pause pendant une analyse)

        private void Pendule_Click(object sender, EventArgs e)
        {   // Un clic sur l'une des deux pendules met la partie en pause, un autre la reprend (comme le bouton d'une vraie pendule).
            // Sans pendule, ou hors d'une partie en cours, le clic ne fait rien
            if (_pendule == null || !_partie.EnCours)
                return;
            if (_pauseJoueur)
            {
                FinPause();
                InformationsPartie.Text = "Partie reprise";
                if (_partie.MoteurAuTrait && _pilote.Demande == TypeDemande.Aucune)
                    JeuMoteurAvecBibliothèque(LogiqueMouvements.RetourneChaineFenActuel());     // sa réflexion avait été interrompue
            }
            else if (_pendule.Tourne)
            {
                AbandonneReflexion();   // le moteur s'arrête de réfléchir (sinon il jouerait pendant la pause) ; il recommencera à la reprise
                _pendule.Pause();
                _pauseJoueur = true;
                InformationsPartie.Text = "En pause (clic pour reprendre)";     // le cadre vert est court : ~30 caractères
                MetAJourCommandes();
            }
            AffichePendules();
        }
        private void FinPause()
        {   // Fin de la pause du joueur (reprise, ou partie qui change : nouvelle partie, résultat, retour arrière...)
            if (!_pauseJoueur)
                return;
            _pauseJoueur = false;
            _pendule?.Reprendre();
            MetAJourCommandes();
        }

        private void ChoisitCadence(Cadence cadence)
        {   // Sélectionne la cadence dans la liste (ajoutée si elle n'y est pas, ex : valeur écrite à la main dans le .ini)
            _cadence = cadence;     // avant la sélection : pas de message "à la prochaine partie"
            maNouvellePartieForceModule.ChoixCadence = cadence;
            if (!ListePendule.Items.Contains(cadence))
                ListePendule.Items.Add(cadence);
            ListePendule.SelectedItem = cadence;
        }
        private void ListePendule_SelectedIndexChanged(object sender, EventArgs e)
        {   // Nouvelle cadence : pour la partie suivante (la partie en cours garde la sienne)
            if (ListePendule.SelectedItem is not Cadence cadence || cadence == _cadence)
                return;
            _cadence = cadence;
            maNouvellePartieForceModule.ChoixCadence = cadence;
            InformationsPartie.Text = "Pendule " + cadence.Nom + " : à la prochaine partie";
        }
        private void NouvellePendule()
        {   // Début d'une partie : pendule de la cadence choisie, temps complets affichés. Elle ne démarre qu'au premier coup
            // (voir CoupJoue), comme sur les serveurs : on peut regarder la position avant que le temps file
            _pauseJoueur = false;
            _pendule = _cadence.EstSansPendule ? null : new Pendule(_cadence, () => _chrono.Elapsed);
            PartieEnCours.TimeControl = _pendule?.Cadence.TimeControl ?? "";     // balise PGN [TimeControl] (absente sans pendule)
            if (_pendule != null)
                _minuteriePendule.Start();
            else
                _minuteriePendule.Stop();
            AffichePendules();
        }
        private void SupprimePendule()
        {   // Partie sans pendule (ex : partie PGN chargée)
            _pauseJoueur = false;
            _pendule = null;
            _minuteriePendule.Stop();
            AffichePendules();
        }
        private void MinuteriePendule_Tick(object sender, EventArgs e)
        {   // Tous les dixièmes de seconde : reprise après une analyse, chute du drapeau, affichage
            if (_pendule == null)
                return;
            if (_pendule.EnPause && !_pilote.AnalyseEnCours && !_pauseJoueur)
            {   // L'analyse est finie (ou abandonnée) : la pendule repart ; si le moteur devait jouer, l'analyse a interrompu
                // sa réflexion : on lui redemande son coup (sinon son temps s'écoulerait sans qu'il réfléchisse)
                _pendule.Reprendre();
                if (_partie.MoteurAuTrait && _pilote.Demande == TypeDemande.Aucune)
                    JeuMoteurAvecBibliothèque(LogiqueMouvements.RetourneChaineFenActuel());
            }
            // (pendant le choix d'une promotion, le coup n'est pas fini : la chute du drapeau est traitée juste après, par CoupJoue)
            if (_partie.EnCours && !GroupPromo.Visible && _pendule.TempsEcoule() is ColorPiece campSansTemps)
                PerteAuTemps(campSansTemps);
            AffichePendules();
        }
        private void PerteAuTemps(ColorPiece campSansTemps)
        {   // Le temps du camp est écoulé : il perd, sauf si l'adversaire n'a pas de quoi mater (nulle)
            ColorPiece adversaire = Adversaire(campSansTemps);
            string message = "Temps écoulé pour les " + NomCamp(campSansTemps);
            if (LogiqueMouvements.PeutMater(adversaire))
                GestionResultat(adversaire == ColorPiece.Blanc ? "1-0" : "0-1", " Gain " + NomCouleur(adversaire) + " (temps)");
            else
                GestionResultat("1/2-1/2", "Nulle (temps écoulé, matériel insuffisant)");
            InformationPourJoueur.Text = VarianteMoteurCourante.Text = message;
        }
        private void AffichePendules()
        {   // Les deux pendules, toujours affichées : "-:--" sans pendule ; le camp qui décompte sur fond vert, en rouge sous 10 secondes
            PenduleBlanc.Cursor = PenduleNoir.Cursor = _pendule != null ? Cursors.Hand : Cursors.Default;   // cliquables : pause / reprise
            if (_pendule == null)
            {   // Pas de pendule en cours : temps notés dans la partie (ex : PGN chargé avec des [%clk]) à la position affichée
                var (tempsBlancs, tempsNoirs) = TempsDeLaPositionAffichee();
                PenduleBlanc.Text = tempsBlancs is TimeSpan blancs ? Pendule.Texte(blancs) : "-:--";
                PenduleNoir.Text = tempsNoirs is TimeSpan noirs ? Pendule.Texte(noirs) : "-:--";
                PenduleBlanc.BackColor = PenduleNoir.ForeColor = Color.White;
                PenduleNoir.BackColor = PenduleBlanc.ForeColor = Color.Black;
                return;
            }
            AffichePendule(PenduleBlanc, ColorPiece.Blanc, Color.White, Color.Black);
            AffichePendule(PenduleNoir, ColorPiece.Noir, Color.Black, Color.White);
        }
        private (TimeSpan? Blancs, TimeSpan? Noirs) TempsDeLaPositionAffichee()
        {   // Temps notés dans le coup de la position affichée (parcours) ou du dernier coup ; position de départ : temps initial
            // de la cadence (balise TimeControl). Rien si la partie n'a aucun temps noté
            List<Coup> coups = [.. LogiqueMouvements.ListeCoups];
            if (!coups.Any(c => c.TempsBlancs != null || c.TempsNoirs != null))
                return (null, null);
            int index = ParcoursEnCours ? _indexAffiche : coups.Count - 1;
            if (index >= 0 && index < coups.Count && !coups[index].EstPositionDeDepart)
                return (coups[index].TempsBlancs, coups[index].TempsNoirs);
            Cadence cadence = Cadence.Lire(PartieEnCours.TimeControl);
            return cadence.EstSansPendule ? (null, null) : (cadence.TempsInitial, cadence.TempsInitial);
        }
        private void AffichePendule(Label affichage, ColorPiece camp, Color fond, Color texte)
        {
            TimeSpan restant = _pendule.TempsRestant(camp);
            bool decompte = _pendule.Tourne && _pendule.CampQuiDecompte == camp;
            affichage.Text = Pendule.Texte(restant);    // ("1:30:00" : la pendule réduit sa police pour qu'il tienne, voir ReduitPourTenir)
            affichage.BackColor = _pauseJoueur ? Color.Silver : decompte ? Color.LightGreen : fond;     // gris : partie en pause
            affichage.ForeColor = restant < TimeSpan.FromSeconds(10) ? Color.Red : decompte ? Color.Black : texte;
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Joueurs affichés (noms et Elo)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void AfficheJoueurs(string blancs, string eloBlancs, string noirs, string eloNoirs)
        {   // Point de passage unique : les étiquettes et l'en-tête PGN de la partie sont toujours écrits ensemble.
            // Noms donnés directement (position FEN, partie PGN, en-tête saisi, humain contre humain) : le moteur n'y touche plus
            PartieEnCours.White = blancs;
            PartieEnCours.WhiteElo = eloBlancs;
            PartieEnCours.Black = noirs;
            PartieEnCours.BlackElo = eloNoirs;
            LabelJoueurBlanc.Text = TexteJoueur(blancs, eloBlancs);
            LabelJoueurNoir.Text = TexteJoueur(noirs, eloNoirs);
            _nomsHumainMoteur = false;
        }
        private static string TexteJoueur(string nom, string elo) =>
            // Une seule étiquette par camp : "[1767] Bruno", "[?] Bruno" si l'Elo est inconnu ; rien sans nom (position FEN chargée)
            string.IsNullOrWhiteSpace(nom) ? "" : $"[{(string.IsNullOrWhiteSpace(elo) ? "?" : elo.Trim())}] {nom}";
        private (string Nom, string Elo) IdentiteDe(Joueur joueur) =>
            joueur == Joueur.Moteur ? (_nomMoteur, _moteurElo) : (_nomHumain, _joueurElo);
        private void AfficheJoueursDeLaPartie()
        {   // Nom et Elo de l'humain ou du moteur, selon qui joue chaque camp (_partie.Blancs / _partie.Noirs)
            var (blancs, eloBlancs) = IdentiteDe(_partie.Blancs);
            var (noirs, eloNoirs) = IdentiteDe(_partie.Noirs);
            AfficheJoueurs(blancs, eloBlancs, noirs, eloNoirs);
            _nomsHumainMoteur = true;
        }
        private void AfficheMoteurDansLaPartie()
        {   // Le nom ou l'Elo du moteur a changé : seul le ou les camps qu'il joue changent, et seulement si les noms affichés
            // sont ceux de l'humain et du moteur (pas après un chargement FEN ou PGN, ni un en-tête saisi)
            if (!_nomsHumainMoteur)
                return;
            bool blancs = _partie.Blancs == Joueur.Moteur, noirs = _partie.Noirs == Joueur.Moteur;
            AfficheJoueurs(blancs ? _nomMoteur : PartieEnCours.White, blancs ? _moteurElo : PartieEnCours.WhiteElo,
                           noirs ? _nomMoteur : PartieEnCours.Black, noirs ? _moteurElo : PartieEnCours.BlackElo);
            _nomsHumainMoteur = true;
        }
        public void DefinitEloMoteur(string elo)
        {   // Elo du moteur réglé dans les paramètres de base
            _moteurElo = elo;
            AfficheMoteurDansLaPartie();
        }
        private void ParametresDeBase_Click(object sender, EventArgs e)
        {   // Affiche les paramètres de base du moteur UCI
            mesparametresDeBase.Show();
        }
        private void ParametresAvances_Click(object sender, EventArgs e)
        {   // Affiche les paramètres avancés du moteur UCI
            mesParametresUciStockfish.Show();
        }
        private void StopMoteur_Click(object sender, EventArgs e)
        {   // Arrête le moteur UCI (utilise si réflexion infinie)
            timer.Stop();
            MoteurUci.StandardInputDataToUci("stop");
            InformationPourJoueur.Text = "Arrêt réflexion Moteur ";
        }

        private void CaseSombre_Click(object sender, EventArgs e)
        {   // Permet de choisir la couleur des cases sombres de l'échiquier (enregistrée dans les préférences à la fermeture)
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
                _vue.ChangeCouleurs(_vue.CaseClaire, CouleurDialogue.Color);
        }
        private void CaseClaire_Click(object sender, EventArgs e)
        {   // Permet de choisir la couleur des cases claires de l'échiquier (enregistrée dans les préférences à la fermeture)
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
                _vue.ChangeCouleurs(CouleurDialogue.Color, _vue.CaseSombre);
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des boutons
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void SaisiePartieBouton_Click(object sender, EventArgs e)
        {   // Permet de saisir une partie en cours, ou terminée, pour l'analyser ou la faire rejouer
            HumainContreHumain_Click(sender, e);
        }
        private void AnalysePosition_Click(object sender, EventArgs e)
        {   // Analyse la position affichée : la position courante de la partie, ou le coup passé que l'on regarde (parcours)
            AbandonneReflexion();   // une nouvelle analyse remplace la réflexion en cours
            Position position = _positionAffichee ?? LogiqueMouvements.PositionActuelle;
            if (LogiqueMouvements.CalculerSur(position, () => !LogiqueMouvements.ResteCoupsValidesJouables()))
            {   // Plus aucun coup jouable : mat ou pat, rien à analyser
                bool mat = LogiqueMouvements.CalculerSur(position, LogiqueMouvements.CampAuTraitEnEchec);
                MiseaZeroVariantes();
                InformationPourJoueur.Text = "Analyse inutile ...";
                InformationsPartie.Text = "La position est terminée ...";
                KryptonMessageBox.Show(mat ? "La position est un mat." : "La position est un pat.", "Analyse inutile");
                return;
            }
            InformationPourJoueur.Text = StatusProgramme.Text = "Analyse de la position ...";
            _pilote.DemanderAnalyse(position, _dureeReflexionMilliSeconde);   // les variantes du moteur seront converties sur cette position
            _pendule?.Pause();  // l'analyse est une aide : la pendule s'arrête pendant ce temps (voir MinuteriePendule_Tick)
            AffichePendules();
            LancerReflexion();  // Décompte le temps de réflexion
        }
        private void InverseEchiquier_Click(object sender, EventArgs e)
        {   // Permet d'inverser la vue de l'échiquier (côté Blanc ou côté Noir) : ne change pas le droit de jouer
            TourneEchiquier();
        }
        private void OrdinateurJoue_Click(object sender, EventArgs e)
        {   // Permet de faire jouer l'ordinateur UCI, sans que ce soit son tour (pour tester une position par exemple)
            AbandonneReflexion();   // une nouvelle demande remplace la réflexion en cours
            FinPause();                     // faire jouer le moteur met fin à une pause
            _partie.MoteurPrendLeTrait();   // le moteur joue désormais le camp au trait, l'humain l'autre
            if (_pendule != null && _pendule.CampQuiDecompte == null && _partie.EnCours && ListeCoups.Any(c => !c.EstPositionDeDepart))
                _pendule.Demarrer(QuiJoue);     // pendule arrêtée par un retour arrière : elle repart pour le moteur
            JeuMoteurAvecBibliothèque(ListeCoupsFen.Count > 0 ? ListeCoupsFen[^1] : FenDepart);   // dernière position, ou position initiale
            // Plateau bloqué tant que le moteur réfléchit ; libre si son coup de bibliothèque est déjà joué
            PlateauEnable(!_partie.MoteurAuTrait);
            _clickCaseSource = _visuSymbole = true;    // L'ordinateur ayant joué, c'est indispensable !
        }
        private void RetourArriere_Click(object sender, EventArgs e)
        {   // Permet de revenir en arrière d'un demi-coup (coup des blancs ou des noirs)
            if (ParcoursEnCours)
                return;             // sécurité : le bouton est grisé pendant le parcours (voir MetAJourCommandes)
            AbandonneReflexion();   // retour arrière : le moteur ne doit pas jouer sur la position annulée
            _vue.EffaceDernierCoup();
            bool etaitTerminee = _partie.Mode == ModePartie.Terminee;
            if (!_partie.AnnulerDernierCoup())      // retire le dernier 1/2 coup et rétablit la position (jamais avant la position de départ)
                _ = KryptonMessageBox.Show("Pas assez de coups joués \nPas de retour arrière possible", "Retour impossible", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            else
            {
                if (etaitTerminee)
                    EffaceResultat();   // on a annulé un coup d'une partie terminée : elle reprend
                // Pendule : temps d'avant le coup annulé. Si c'est au moteur de jouer, elle attend ("Ordinateur joue", ou un
                // 2e retour arrière) : sinon son temps s'écoulerait alors que personne ne lui demande de jouer
                _pauseJoueur = false;   // le retour arrière met fin à une pause
                _pendule?.RestaurerDepuis(LogiqueMouvements.ListeCoups, QuiJoue);
                if (_partie.MoteurAuTrait)
                    _pendule?.Arreter();
                AffichePendules();
                AfficheCoupsBibliotheque(LogiqueMouvements.RetourneChaineFenActuel());
                InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + NomCamp(QuiJoue);
                InformationsPartie.Text = _partie.Blancs == Joueur.Moteur ? "L'ordinateur joue les Blancs" :
                          _partie.Noirs == Joueur.Moteur ? "L'ordinateur joue les Noirs" :
                          "L'ordinateur ne joue pas cette partie";
                // Un demi-coup par clic (choix de Bruno) : au joueur de jouer, sauf si c'est au tour du moteur (il faut alors
                // un 2e retour arrière, ou "Ordinateur joue") ; le message le dit, l'échiquier bloqué ne doit pas surprendre
                PlateauEnable(!_partie.MoteurAuTrait);
                VarianteMoteurUci2.Text = _partie.MoteurAuTrait
                    ? $"   Au tour de {_nomMoteur} : encore « Retour arrière » pour revenir à votre coup, ou « Ordinateur joue »" : "...";
            }
            MetAJourCommandes();
        }
        private void EffaceResultat()
        {   // Une partie terminée reprend (retour arrière après un mat, un pat, une nulle ou un abandon) : le résultat est effacé
            PartieEnCours.Result = "";
            ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "...";
        }
        private void ListeCoupsBouton_Click(object sender, EventArgs e)
        {   // Affiche la liste des coups joués dans une fenêtre dédiée
            // Plus de blocage : parcourir la liste ne modifie que la position affichée (voir AfficheCoupDeLaPartie)
            // Si la fenêtre n'existe pas ou est déjà fermée, la créer
            if (mafenetrePartie == null || mafenetrePartie.IsDisposed)
            {
                mafenetrePartie = new FenetrePartie(this);
                mafenetrePartie.Show();
            }
            else
            {   // La fenêtre est déjà ouverte, lui redonner le focus
                mafenetrePartie.FeuillePartie.Rows.Clear();
                mafenetrePartie.BringToFront();
                mafenetrePartie.Focus();
            }
            mafenetrePartie.LblJoueurBlanc.Text = PartieEnCours.White;
            mafenetrePartie.LblJoueurNoir.Text = PartieEnCours.Black;
            mafenetrePartie.LblEloBlanc.Text = PartieEnCours.WhiteElo;
            mafenetrePartie.LblEloNoir.Text = PartieEnCours.BlackElo;
            // Une ligne par coup complet (une partie FEN peut commencer par un coup noir : "n. | ... | coup")
            List<string[]> lignes = FeuilleDePartie.Lignes(LogiqueMouvements.ListeCoups);
            if (lignes.Count != 0)
            {
                foreach (string[] ligne in lignes)
                    mafenetrePartie.FeuillePartie.Rows.Add(ligne[0], ligne[1], ligne[2]);
                if (mafenetrePartie.FeuillePartie.Rows.Count > 0)
                {   // Sélectionner la cellule du premier coup (colonne des Blancs, ou des Noirs si la partie commence par un coup noir)
                    int colonne = FeuilleDePartie.CommenceParLesNoirs(LogiqueMouvements.ListeCoups) ? 2 : 1;
                    mafenetrePartie.FeuillePartie.CurrentCell = mafenetrePartie.FeuillePartie.Rows[0].Cells[colonne];
                    mafenetrePartie.FeuillePartie.Focus(); // Met le focus sur la DataGridView
                }
            }
        }
        public void MontrePartiesPGN_Click(object sender, EventArgs e)
        {   // Affiche ou masque la liste des parties (la fenêtre n'est jamais détruite : voir FichierPartiePgn_FormClosing).
            // Le texte du bouton suit l'état réel de la fenêtre (MetAJourBoutonListeParties, sur VisibleChanged)
            if (fichierPartiePgn.Visible)
                fichierPartiePgn.Hide();
            else
                fichierPartiePgn.Show();
        }
        private void MetAJourBoutonListeParties() =>
            MontrePartiesPGN.Text = fichierPartiePgn.Visible ? "Masque liste parties" : "Affiche liste parties";
        private void VisualisationPgn_Click(object sender, EventArgs e)
        {   // Bouton pour voir la partie en PGN
            if (affichePgn == null || affichePgn.IsDisposed)
            {   // Traitement pour prendre en compte la fermeture par croix rouge en haut à droite ...
                affichePgn = new AffichePgn();
                affichePgn.FormClosed += (s, args) =>
                {   // On évite que la référence de affichePgn pointe vers un objet supprimé.
                    affichePgn = null;  // S'assure que la référence est libérée à la fermeture
                };
            }
            affichePgn.Show();
            string contenuPgnIntl = GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Intl");
            string contenuPgnFr = GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Fr");
            affichePgn.AffichePgnDansZone(contenuPgnIntl, contenuPgnFr);
        }
        private void VisualiserPgn_Click(object sender, EventArgs e)
        {   // Option de menu  pour voir la partie en PGN
            VisualisationPgn_Click(sender, e);
        }
        private void BoutonBalises_Click(object sender, EventArgs e)
        {   // Ouvre la fenêtre de l'en-tête PGN
            SaisieBalises SaisieBalises = new(PartieEnCours);
            SaisieBalises.ShowDialog();
            // On affiche les noms et Elo des joueurs qui sont dans l'en-tête PGN
            AfficheJoueurs(PartieEnCours.White, PartieEnCours.WhiteElo, PartieEnCours.Black, PartieEnCours.BlackElo);
        }
        private void MontreVariantesUci_Click(object sender, EventArgs e)
        {   // Affiche ou masque les 3 variantes UCI (info multiPV) à chaque clic
            _montre3VariantesUci = !_montre3VariantesUci;
            MontreVariantesUci.Text = _montre3VariantesUci ? "Affiche variantes UCI" : "Masque variantes UCI";
            VarianteMoteurUci1.Visible = VarianteMoteurUci2.Visible = VarianteMoteurUci3.Visible = !_montre3VariantesUci;
        }
        private void MontreDonneesUci_Click(object sender, EventArgs e)
        {   // Affiche ou masque les données brutes UCI (info, bestmove, etc.) à chaque clic
            _montreDonneesBrutesUci = !_montreDonneesBrutesUci;
            MontreDonneesUci.Text = _montreDonneesBrutesUci ? "Masque protocole UCI" : "Affiche protocole UCI";
            if (_montreDonneesBrutesUci) donneesBrutesUci.Show();   // On affiche les données brutes UCI
            else donneesBrutesUci.Hide();                           // On masque les données brutes UCI
            donneesBrutesUci.DonneesBrutesVue.ScrollToCaret();      // Pour garder l'affichage dans toute la fenêtre
        }
        private void AideDocumentation_Click(object sender, EventArgs e)
        {   // Ouvre la fenêtre d'aide et documentation
            var fenetreAide = new FenetreAide();
            fenetreAide.ShowDialog();
        }
        private void Apropos_Click(object sender, EventArgs e)
        {   // Option de menu "A propos"
            // Version 1.01 = gestion des fichiers réseaux neuronaux dans même répertoire que le moteur UCI (Stockfish NNUE)
            // Version 1.02 = corrections des règles (roque, prise en passant, promotion, 50 coups, lecture FEN), classe Position,
            //                MultiPV réglable, mise à jour automatique compatible Stockfish 19 (binaire "universal"), projet de tests (perft)
            // Version 1.03 = notation PGN des coups ambigus (écriture et relecture), décodage UCI (signe du mat, moteurs sans MultiPV),
            //                couleurs lues dans le .ini (#hexadécimal, style Lichess par défaut), Threads et Hash envoyés au moteur,
            //                lancement depuis un raccourci, tests automatiques et publication sur GitHub
            // Version 1.04 = préférences enregistrées (BrunoGUI.preferences.ini), liste unique des coups, nulles automatiques
            //                (50 coups, matériel insuffisant, répétition FIDE), mise à jour de Stockfish tous les 30 jours avec accord,
            //                Krypton 95 seul (boutons en français, palette choisie dans le .ini), corrections de fiabilité
            // Version 1.05 = boutons actifs pendant la réflexion du moteur (demandes numérotées), parcours de la partie pendant le jeu
            //                (position affichée séparée), retour arrière après la fin de partie, scores du point de vue des Blancs,
            //                classe Partie et commandes actives calculées en un seul endroit, écran de démarrage de 1 s
            // Version 1.06 = "Reprendre ici" (reprendre une partie, même terminée ou chargée en PGN, depuis une position passée),
            //                pilotage du moteur et calcul des évaluations/variantes hors du formulaire (testés), mat affiché avec
            //                le camp qui mate, variantes en coups entiers, arrêt garanti du moteur (Sargon), fin d'analyse fiabilisée
            // Version 1.10 = revue de code (annuler une nouvelle partie, PGN : variantes, SetUp/FEN, Latin-1...), formulaire allégé
            //                (affichage de l'échiquier, chargement FEN/PGN et bibliothèque sortis et testés), fenêtre du protocole
            //                plus rapide, mise à jour de Stockfish simplifiée (ARM), promotion passée en paramètre, couleurs typées,
            //                noms et Elo des joueurs affichés en un seul endroit (Elo au bon camp), MoteurUci sans statique
            // Version 1.11 = pendule (cadences 3+2 à 30 min, temps du moteur en go wtime/btime, perte au temps, retour arrière et
            //                "Reprendre ici" avec les temps, balise TimeControl et %clk dans le PGN enregistré), joueurs « [Elo] Nom »
            //                sans cadre, barre de réflexion, contrôle complet des FEN (plus de plantage), promotion en paramètre
            _ = KryptonMessageBox.Show("      BrunoGUI GenII\n       Version 1.11\n--  Bruno COURTOIS  -- " +
                                                                    "\n Copyright © 2026", "A propos de",
                KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
        }
        private void KryptonApropos_Click(object sender, EventArgs e)
        {   // Bouton "A propos"
            Apropos_Click(sender, e);
        }
        private string CheminStockfish => Path.Combine(Chemins.RepertoireRacine, "stockfish", "stockfish.exe");

        private async Task VerificationAutomatiqueMiseAJour()
        {   // Au démarrage : au plus tous les VerificationMiseAJourJours jours (BrunoGUI.ini), téléchargement seulement après accord.
            // Exception si la vérification échoue (ex : hors ligne) : la date n'est pas enregistrée, on réessaiera au prochain démarrage
            if (!parametres.VerificationMiseAJourDue(DateTime.Today))
            {
                Debug.WriteLine($"[MAJ] Pas de vérification automatique (dernière : {parametres.DerniereVerificationMiseAJour:yyyy-MM-dd}).");
                return;
            }
            MiseAJourStockfish maj = new(CheminStockfish);
            var version = await maj.RechercherNouvelleVersion();
            parametres.DerniereVerificationMiseAJour = DateTime.Today;     // enregistrée dans les préférences à la fermeture
            if (version != null && DemandeInstallation(version))
                await maj.Installer(version);       // le moteur n'est pas encore démarré à ce stade
        }
        private bool DemandeInstallation(MiseAJourStockfish.VersionStockfish version)
        {   // Demande l'accord avant de télécharger (question posée sur le thread de l'interface)
            string question = $"Une nouvelle version de Stockfish est disponible : {version.Tag.Replace("sf_", "Stockfish ")} " +
                              $"({version.TailleOctets / 1_000_000} Mo).\n\nLa télécharger et l'installer maintenant ?";
            DialogResult Demander() => KryptonMessageBox.Show(question, "Mise à jour de Stockfish", KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question);
            DialogResult reponse = InvokeRequired ? (DialogResult)Invoke(new Func<DialogResult>(Demander)) : Demander();
            return reponse == DialogResult.Yes;
        }
        private async void BtnMiseAJour_Click(object sender, EventArgs e)
        {   // 1. On prépare l'UI
            BtnMiseAJour.Enabled = false;
            Cursor = Cursors.WaitCursor;
            VarianteMoteurUci2.Text = "Vérification de la version courante de Stockfish...";
            try
            {   // 2. Recherche, puis installation après accord
                MiseAJourStockfish maj = new(CheminStockfish);
                var version = await maj.RechercherNouvelleVersion();
                parametres.DerniereVerificationMiseAJour = DateTime.Today;
                if (version == null)
                {
                    VarianteMoteurUci2.Text = "Stockfish est à jour !";
                    KryptonMessageBox.Show("Vous avez déjà la dernière version.", "Stockfish", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    return;
                }
                if (!DemandeInstallation(version))
                {
                    VarianteMoteurUci2.Text = "Prêt";
                    return;
                }
                VarianteMoteurUci2.Text = "Téléchargement et installation de Stockfish...";
                bool stockfishEnCours = string.Equals(Path.GetFullPath(_cheminMoteur), CheminStockfish, StringComparison.OrdinalIgnoreCase);
                if (stockfishEnCours)
                    AbandonneReflexion();   // l'installation arrête Stockfish : sa réflexion en cours n'aurait jamais de réponse
                await maj.Installer(version);       // arrête le Stockfish en cours
                if (stockfishEnCours)
                {   // Le moteur arrêté par l'installation est redémarré avec la nouvelle version
                    MoteurUci.Quitte();
                    MoteurUci.Start(CheminStockfish);
                    if (_partie.MoteurAuTrait)      // il devait jouer : on lui redemande son coup
                        JeuMoteurAvecBibliothèque(LogiqueMouvements.RetourneChaineFenActuel());
                }
                VarianteMoteurUci2.Text = "Stockfish est à jour !";
                KryptonMessageBox.Show($"{version.Tag.Replace("sf_", "Stockfish ")} est installé." +
                                       (stockfishEnCours ? "\nSes réglages de force seront appliqués à la prochaine nouvelle partie." : ""),
                                       "Stockfish", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            }
            catch (Exception ex)
            {   // On gère les messages (ex: "Déjà à jour" ou "Pas de connexion")
                VarianteMoteurUci2.Text = "Prêt";
                KryptonMessageBox.Show(ex.Message, "Mise à jour", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            }
            finally
            {
                BtnMiseAJour.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        //  Fermeture Programme
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void EchiquierPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {   // Demande de confirmation avant de quitter l'application
            if (KryptonMessageBox.Show("Quitter l'application ?", "Confirmer",
                KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true; // Annule la fermeture
                return;
            }
            StopMoteur_Click(sender, e);
            SauvePreferences();
            MoteurUci.Quitte();     // arrête le processus du moteur (de force s'il ignore "quit", comme Sargon)
        }
        private void SauvePreferences()
        {   // Enregistre les réglages faits dans l'interface dans BrunoGUI.preferences.ini (à côté de l'exécutable)
            parametres.CaseSombre = Parametres.FormatCouleur(_vue.CaseSombre);
            parametres.CaseClaire = Parametres.FormatCouleur(_vue.CaseClaire);
            parametres.CouleurCaseSource = Parametres.FormatCouleur(_vue.CaseSource);
            parametres.CouleurCaseDestination = Parametres.FormatCouleur(_vue.CaseDestination);
            parametres.NomHumain = _nomHumain;
            parametres.DureeReflexionSeconde = (int)TempsReflexionSecondes.Value;
            parametres.Cadence = _cadence;
            parametres.ForceMoteur = maNouvellePartieForceModule.ForceModule;       // l'Elo choisi, même si la dernière partie était en force maximale
            parametres.ForceMaximale = maNouvellePartieForceModule.ForceMaximale;
            parametres.CouleurMoteur = maNouvellePartieForceModule.ChoixCouleur;
            parametres.NombreLignesPV = MoteurUci.NombreLignesPV;
            parametres.NombreCoeursThread = MoteurUci.NombreThreads ?? parametres.NombreCoeursThread;
            parametres.TailleHachageMo = MoteurUci.TailleHachageMo;
            // Bibliothèque : juste le nom si elle est dans le dossier des bibliothèques fournies, sinon le chemin complet
            parametres.Bibliotheque = string.Equals(Path.GetDirectoryName(Path.GetFullPath(Path.Combine(Chemins.BibliothèquesPolyglot, _bibliotheque))),
                                                    Path.GetFullPath(Chemins.BibliothèquesPolyglot), StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileName(_bibliotheque) : _bibliotheque;
            try
            {
                parametres.SauverPreferences(Path.Combine(Chemins.RepertoireRacine, Parametres.FichierPreferences));
            }
            catch (Exception ex)
            {   // Par exemple si l'application est installée dans un dossier protégé en écriture : on ne bloque pas la fermeture
                Debug.WriteLine("[INFO] Préférences non enregistrées : " + ex.Message);
            }
        }
        private void KryptonQuitter_Click(object sender, EventArgs e)
        {   // Note : Envoie l’événement FormClosing puis l’événement FormClosed (après la fermeture complète)
            // Et détruit les contrôles du formulaire
            Close();
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des boutons et flèches pour parcours de partie
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void BoutonPrecedent_Click(object sender, EventArgs e)
        {   // On recule d'un demi-coup dans l'affichage (la partie n'est pas modifiée)
            if (LogiqueMouvements.ListeCoups.Count == 0)
            {
                KryptonMessageBox.Show("Aucun coup à afficher.", "Info", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            int indexActuel = ParcoursEnCours ? _indexAffiche : LogiqueMouvements.ListeCoups.Count - 1;
            if (indexActuel <= IndexPremierePosition)
                KryptonMessageBox.Show("Vous êtes au début de la partie.", "Début de partie", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            else
                AfficheCoupDeLaPartie(indexActuel - 1);
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {   // On avance d'un demi-coup dans l'affichage ; après le dernier coup, on revient à la position courante de la partie
            if (ParcoursEnCours)
                AfficheCoupDeLaPartie(_indexAffiche + 1);
            else if (LogiqueMouvements.EchecetMat)
                KryptonMessageBox.Show("Il y a échec et mat.", "Terminé : échec et mat", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            else
                KryptonMessageBox.Show("Vous êtes à la fin de la partie.", "Fin de partie", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {   // On affiche la position initiale de la partie (ou la position FEN de départ)
            if (LogiqueMouvements.ListeCoups.Count > 0)
                AfficheCoupDeLaPartie(IndexPremierePosition);
        }
        private void BoutonFin_Click(object sender, EventArgs e)
        {   // On revient à la position courante de la partie
            RetourPositionCourante();
        }
        private void BoutonReprendreIci_Click(object sender, EventArgs e)
        {   // "Reprendre la partie d'ici" : la partie est coupée à la position affichée (parcours) et reprend depuis cette position.
            // Une partie PGN en lecture seule peut aussi être reprise à sa position finale (rien n'est supprimé)
            bool etaitLectureSeule = PartieEnLectureSeule;
            if (!ParcoursEnCours && !etaitLectureSeule)
                return;     // sécurité : voir MetAJourCommandes
            int index = ParcoursEnCours ? _indexAffiche : LogiqueMouvements.ListeCoups.Count - 1;
            int aSupprimer = LogiqueMouvements.ListeCoups.Count - 1 - index;
            Position positionReprise = _positionAffichee ?? LogiqueMouvements.PositionActuelle;
            if (LogiqueMouvements.CalculerSur(positionReprise, () => !LogiqueMouvements.ResteCoupsValidesJouables()))
            {   // mat ou pat : il n'y a rien à jouer depuis cette position
                KryptonMessageBox.Show("Cette position est terminée (mat ou pat) : choisissez une position antérieure avec Préc.",
                    "Reprendre la partie d'ici", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            string suppression = aSupprimer > 0 ? $"\n\nLes {aSupprimer} demi-coup(s) suivant(s) seront supprimés." : "";
            string message = etaitLectureSeule
                ? $"La partie chargée devient votre partie à partir de la position affichée :\nvous jouez le camp au trait, {_nomMoteur} l'autre camp.{suppression}\n\nContinuer ?"
                : $"La partie reprend à la position affichée.{suppression}\n\nContinuer ?";
            if (KryptonMessageBox.Show(message, "Reprendre la partie d'ici", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Question) != DialogResult.OK)
                return;
            AbandonneReflexion();   // la partie change
            bool etaitTerminee = _partie.Mode == ModePartie.Terminee;
            if (mafenetrePartie != null && !mafenetrePartie.IsDisposed)
                mafenetrePartie.Close();    // sa liste de coups ne correspond plus à la partie
            QuitteParcours();
            _vue.EffaceDernierCoup();
            if (_partie.ReprendreDepuis(index) == 0 && !etaitLectureSeule)
            {   // rien à supprimer : l'échiquier revient simplement à la partie
                LogiqueMouvements.DessinPieces();
                MetAJourCommandes();
                return;
            }
            if (etaitTerminee || etaitLectureSeule)
                EffaceResultat();       // la partie n'a plus de résultat
            if (etaitLectureSeule)
            {   // La partie chargée devient une partie d'entraînement contre le moteur (l'humain a le camp au trait)
                AfficheJoueursDeLaPartie();
                PartieEnCours.Tournoi = "Entrainement";
                PartieEnCours.Lieu = "Maison";
                PartieEnCours.Date = DateTime.Today.ToString("yyyy.MM.dd");
                PartieEnCours.Ronde = "";
            }
            _pauseJoueur = false;       // reprendre la partie met fin à une pause
            if (etaitLectureSeule)
                NouvellePendule();      // partie d'entraînement : la cadence choisie, qui part au prochain coup
            else
                _pendule?.RestaurerDepuis(LogiqueMouvements.ListeCoups, QuiJoue);   // temps notés après le coup où l'on reprend
            AffichePendules();
            PartieEnCours.CompteDePLy = LogiqueMouvements.ListeCoupsFen.Count.ToString();
            string fen = LogiqueMouvements.RetourneChaineFenActuel();
            AfficheCoupsBibliotheque(fen);
            InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + NomCamp(QuiJoue);
            InformationsPartie.Text = "Partie reprise";
            if (_partie.MoteurAuTrait)
            {   // C'est au moteur de jouer à partir de cette position
                PlateauEnable(false);
                MetAJourCommandes();
                JeuMoteurAvecBibliothèque(fen);
            }
            else
            {
                PlateauEnable(true);
                MetAJourCommandes();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {   // Gestion des flèches pour parcourir la partie
            /*
            ProcessCmdKey est une méthode spéciale de la classe Form (et aussi de Control).
            Elle est appelée avant que Windows envoie la touche à un contrôle précis (bouton, textbox, etc.).
            🧠 En clair :
            Quand tu appuies sur une touche :
            Windows la détecte,
            Avant de la donner à un contrôle, WinForms appelle Form.ProcessCmdKey(...),
            Si tu retournes true, tu dis :
            → “je l’ai gérée, inutile de la transmettre ailleurs”,
            Si tu retournes base.ProcessCmdKey(...),
            → “je ne m’en occupe pas, laisse WinForms la gérer normalement”.
            */
            if (!_clavierActif)
                return base.ProcessCmdKey(ref msg, keyData);

            switch (keyData)
            {
                case Keys.Right: BoutonSuivant_Click(null, EventArgs.Empty); return true;
                case Keys.Left: BoutonPrecedent_Click(null, EventArgs.Empty); return true;
                case Keys.Home:
                case Keys.PageUp: BoutonDebut_Click(null, EventArgs.Empty); return true;
                case Keys.End:
                case Keys.PageDown: BoutonFin_Click(null, EventArgs.Empty); return true;
                case Keys.Escape:
                    this.ActiveControl = null;
                    _clavierActif = false; // désactive clavier
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion de fichiers (ouverture/sauvegarde)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐

        /* Séquence : L'utilisateur clique sur 
        "mainform.ChargePartiesPgn_Click"
                ├─ On décode le fichier choisi "ListeParties = fichierPartiePgn.DecodeFichierPGN"
                ├─ On parcourt "ListeParties" pour les décoder avec "fichierPartiePgn.DecodePartiePGN"
                └─ On affiche la liste "fichierPartiePgn.AfficherListeParties(ListePartiesPGN)" 
         L'utilisateur double-clique sur une partie de la liste
                ├─ "mainForm.MontrePartiesPGN_Click"
                ├─ "mainForm.ChargerPartieDepuisPgn(partie)"
                └─ "mainForm.ParcoursPartie(coupsPartie)
        */
        private void ChargePartiesPgn_Click(object sender, EventArgs e)
        {   // --- Affiche la boîte de dialogue et traite le fichier PGN sélectionné  ---
            // Ouvrir un fichier ne change pas la partie en cours (ni la réflexion du moteur) : seul le choix d'une partie
            // dans la liste la remplace (ChargerPartieDepuisPgn). "Annuler" ne change donc rien
            if (ChargerPartiesPgn.ShowDialog() != DialogResult.OK)
                return;
            string cheminFichier = ChargerPartiesPgn.FileName;
            try
            {
                string fullPath = Path.GetFullPath(cheminFichier);
                Debug.WriteLine("Chemin complet du fichier : " + fullPath);
                ListeParties = FichierPartiePgn.DecodeFichierPGN(fullPath); // Récupère les parties PGN
                ListePartiesPGN.Clear();
                foreach (string partie in ListeParties)                     // On met chaque partie au format PartieEchecsPGN dans ListePartiePGN
                    ListePartiesPGN.Add(FichierPartiePgn.DecodePartiePGN(partie));
                fichierPartiePgn.NombrePartiesFichier.Text = ListePartiesPGN.Count.ToString()
                    + " partie(s) dans le fichier  " + Path.GetFileName(cheminFichier);
                fichierPartiePgn.AfficherListeParties(ListePartiesPGN);
                fichierPartiePgn.Show();
                fichierPartiePgn.BringToFront();
                MontrePartiesPGN.Enabled = true; // Active le bouton pour masquer/afficher la liste
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Chargement Pgn : Erreur lors de la lecture du fichier : " + ex.Message);
                KryptonMessageBox.Show("Impossible de lire ce fichier PGN :\n" + ex.Message, "Ouvrir fichier PGN",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            }
        }
        private void ChargePositionFen_Click(object sender, EventArgs e)
        {
            if (ChargerPositionFen.ShowDialog() != DialogResult.OK)
                return;     // annulé : la partie en cours ne change pas
            string[] positions;     // une position par ligne (un fichier peut en contenir plusieurs : on charge la première)
            try
            {
                positions = File.ReadAllLines(Path.GetFullPath(ChargerPositionFen.FileName)).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show("Lecture impossible : " + ex.Message, "Chargement FEN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }
            string contenuFen = positions.Length > 0 ? ChargementPartie.NormaliseFen(positions[0]) : "";
            string erreur = ChargementPartie.ErreurFen(contenuFen);
            if (erreur != null)
            {   // FEN mal formée : on ne la lit pas (elle ferait planter la lecture ou donnerait une position incohérente)
                KryptonMessageBox.Show($"Position FEN refusée : {erreur}.\n\n{contenuFen}", "Chargement FEN",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }
            AbandonneReflexion();   // chargement d'une position
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            ListeParties.Clear();    // On vide la liste des parties
            ListePartiesPGN.Clear(); // On vide la liste des parties PGN
            ChargementPartie.ChargerPosition(contenuFen, _partie);     // l'humain joue le camp au trait, le moteur lui répond
            VarianteMoteurUci1.Text = "Fen chargé : " + contenuFen;
            _vue.EffaceDernierCoup();                // les cases du dernier coup de la partie précédente
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            _clickCaseSource = _visuSymbole = true;
            PartieEnCours.CoupsPartiePGN = PartieEnCours.Result = PartieEnCours.CompteDePLy = PartieEnCours.Ronde = "";
            PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Lieu = "Maison";
            AfficheJoueurs("", "", "", "");     // position chargée : ce n'est la partie ni de l'humain ni du moteur, noms vides
            NouvellePendule();                  // la partie qui commence à cette position suit la cadence choisie
            InformationPourJoueur.Text = "Trait aux " + NomCamp(QuiJoue);
            PlateauEnable(true);   // On active le plateau pour pouvoir jouer à partir de la position chargée
            AfficheCoupsBibliotheque(contenuFen);
            // Le cadre vert est court : les détails du chargement vont dans les lignes de variantes 2 et 3 (inutilisées à ce moment)
            InformationsPartie.Text = "Position chargée";
            VarianteMoteurUci2.Text = positions.Length > 1 ? $"   Fichier de {positions.Length} positions : la première est chargée" : "...";
            VarianteMoteurUci3.Text = _pendule != null ? $"   Pendule {_pendule.Cadence.Nom} : elle démarre au premier coup" : "...";
            MetAJourCommandes();
        }
        private void EnregistrerPgn_Click(object sender, EventArgs e)
        {   // Enregistre la partie au format PGN
            try
            {
                // La partie au format PGN, avec les temps de la pendule après chaque coup (seulement dans le fichier, pas à l'affichage)
                string contenuPgn = GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Intl", avecTemps: true);
                // Ecriture du fichier PGN (Partie complète + en-tête)
                {
                    SauvegardeFichier.OverwritePrompt = false;      // Permet d'éviter l'affichage de 2 boites de dialogue si le fichier choisi existe...
                    DialogResult Reponse = SauvegardeFichier.ShowDialog();  // l'utilisateur doit rentrer le nom du fichier PGN
                    if (Reponse == DialogResult.OK)                         // On ne sauvegarde que si l'utilisateur est d'accord
                    {
                        string cheminPgn = SauvegardeFichier.FileName;
                        if (File.Exists(cheminPgn))                         // Si le fichier existe déjà
                        {   // On demande à l'utilisateur s'il veut écraser le fichier ou ajouter la partie
                            DialogResult resultat = KryptonMessageBox.Show("ATTENTION, le fichier " + Path.GetFileName(cheminPgn) + " existe déjà. \nCliquer Oui pour ajouter la partie à la fin." +
                               "\nCliquer Non pour écraser le fichier existant.\nCancel pour afficher le fichier PGN.",
                               "Fichier existant", KryptonMessageBoxButtons.YesNoCancel, KryptonMessageBoxIcon.Warning);
                            if (resultat == DialogResult.No)
                            {   // Écrase le fichier existant avec la nouvelle partie
                                File.WriteAllText(cheminPgn, contenuPgn);
                                InformationPourJoueur.Text = "La partie est écrite dans le fichier " + Path.GetFileName(cheminPgn);
                            }
                            else if (resultat == DialogResult.Yes)
                            {   // Ajoute la nouvelle partie à la fin du fichier existant
                                string contenuExistant = File.ReadAllText(cheminPgn);
                                contenuExistant += "\n\n" + contenuPgn;
                                File.WriteAllText(cheminPgn, contenuExistant);
                                InformationPourJoueur.Text = "La partie est ajoutée dans le fichier " + Path.GetFileName(cheminPgn);
                            }
                            else if (resultat == DialogResult.Cancel)
                            {   // Affiche la nouvelle partie (sans les temps de la pendule : ils ne vont que dans le fichier)
                                KryptonMessageBox.Show($"Fichier PGN :\n {GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Intl")}", "Affichage fichier PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                            }
                        }
                        else
                        {   // Si le fichier n'existe pas, écrire simplement la nouvelle partie
                            File.WriteAllText(cheminPgn, contenuPgn);               // Ecriture du fichier au format PGN
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                KryptonMessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur méthode Enregistrer PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                Debug.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }
        private void EnregistrerFen_Click(object sender, EventArgs e)
        {   // Ecriture du fichier FEN (position courante)
            {
                DialogResult Reponse = SauvegardeFen.ShowDialog();      // l'utilisateur doit rentrer le nom du fichier FEN
                if (Reponse == DialogResult.OK)                         // On ne sauvegarde que si l'utilisateur est d'accord
                {
                    string CheminFen = SauvegardeFen.FileName;
                    File.WriteAllText(CheminFen, LogiqueMouvements.RetourneChaineFenActuel());  // Ecriture du fichier au format FEN
                }
            }
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des parties PGN (sélection/lecture)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        public void ChargerPartieDepuisPgn(PartieEchecsPGN partie)
        {   // --- Charge UNE partie depuis un fichier PGN lorsque'on double-clique ---
            AbandonneReflexion();
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            _vue.EffaceDernierCoup();    // les cases du dernier coup de la partie précédente
            SupprimePendule();      // partie PGN : pas de pendule
            Debug.WriteLine("ChargerPartieDepuisPgn / :  " + partie.White + " vs " + partie.Black + "   Résultat : " + partie.Result);
            PartieEnCours.Tournoi = partie.Tournoi;
            PartieEnCours.Lieu = partie.Lieu;
            PartieEnCours.Date = partie.Date;
            PartieEnCours.Ronde = partie.Ronde;
            AfficheJoueurs(partie.White, partie.WhiteElo, partie.Black, partie.BlackElo);
            PartieEnCours.Result = InformationsPartie.Text = partie.Result;
            PartieEnCours.ECO = partie.ECO;
            PartieEnCours.CompteDePLy = partie.CompteDePLy;
            PartieEnCours.CoupsPartiePGN = partie.CoupsPartiePGN;
            PartieEnCours.TimeControl = partie.TimeControl;
            InformationPourJoueur.Text = partie.Tournoi + " / ronde " + partie.Ronde;
            StatusProgramme.Text = $"{partie.White} vs {partie.Black}";
            ScoreMoteur.Text = InformationsPartie.Text = "Résultat : " + partie.Result;
            VarianteMoteurCourante.Text = "";
            Debug.WriteLine($"Partie en PGN : {partie.CoupsPartiePGN}");
            // Rejeu des coups (depuis la balise FEN s'il y en a une) ; la partie finit en lecture seule
            ResultatChargementPgn chargement = ChargementPartie.ChargerPartiePgn(partie, _partie);
            PartieEnCours.CompteDePLy = chargement.DemiCoupsJoues.ToString();   // PlyCount : demi-coups réellement rejoués (la balise du fichier peut manquer ou être fausse)
            if (chargement.FenIncomplete)
                KryptonMessageBox.Show($"La position de départ de cette partie (balise FEN) est refusée : {ChargementPartie.ErreurFen(partie.Fen)}.\n" +
                    "Les coups sont joués depuis la position initiale.",
                    "Partie PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            if (chargement.CoupIllisible != null)
                KryptonMessageBox.Show($"Coup illisible ou illégal : « {chargement.CoupIllisible} » (demi-coup n° {chargement.DemiCoupsJoues + 1}).\n" +
                    "La partie est chargée jusqu'au coup précédent.", "Partie PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
            AfficheResultatPartiePgn();
        }
        private void AfficheResultatPartiePgn()
        {   // Partie PGN rejouée : son résultat est affiché, et l'échiquier montre la partie depuis le début
            switch (PartieEnCours.Result)       // Et on ajoute le résultat
            {
                case "1-0":
                    InformationsPartie.Text = "Résultat : 1-0 Gain Blanc";
                    break;
                case "0-1":
                    InformationsPartie.Text = "Résultat : 0-1 Gain Noir";
                    break;
                case "1/2-1/2":
                    InformationsPartie.Text = "Résultat : 1/2-1/2 Nulle";
                    break;
                case "*":
                    InformationsPartie.Text = "Résultat : * Indéterminé";
                    break;
                default:
                    Debug.WriteLine($"Pas de résultat défini : {PartieEnCours.Result}");
                    break;
            }
            // La partie reste sur sa position finale ; elle est en lecture seule (parcours et analyse), et on l'affiche depuis le début
            PlateauEnable(false);
            MetAJourCommandes();
            string resultat = InformationsPartie.Text;
            AfficheCoupDeLaPartie(IndexPremierePosition);
            InformationsPartie.Text = resultat;     // on garde le résultat de la partie affiché
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        //  Bibliothèque d'ouvertures
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void JeuMoteurAvecBibliothèque(string chaineFen)
        {   // Coup du moteur pour la partie : bibliothèque d'ouvertures d'abord (voir _pilote.ChoixBibliotheque), sinon réflexion du moteur
            // Avec une pendule, le moteur reçoit les temps restants et gère son temps ; sinon, un temps fixe par coup
            // (numéro du coup à jouer : pour une cadence à deux périodes, le moteur sait combien de coups restent avant le contrôle)
            LimiteTemps limite = _pendule != null ? LimiteTemps.DepuisPendule(_pendule, (int)Math.Truncate(LogiqueMouvements.NombreCoupsJoues))
                                                  : LimiteTemps.Duree(_dureeReflexionMilliSeconde);
            if (_pilote.DemanderCoup(chaineFen, limite) == ResultatDemandeCoup.CoupBibliotheque)
            {   // Coup trouvé dans la bibliothèque : il est déjà joué
                string coupChoisiTxt = _pilote.DernierCoupBibliotheque;
                VarianteMoteurUci1.Text = "Coup bibliothèque " + Path.GetFileName(_bibliotheque) + " exécuté par le moteur -> " + coupChoisiTxt;
                VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = ".....";
                Debug.WriteLine($"Coup bibliothèque exécuté : {coupChoisiTxt}");
                AfficheCoupsBibliotheque(LogiqueMouvements.RetourneChaineFenActuel());
                AfficheCoupDuMoteur();
                return;
            }
            // Aucun coup dans la bibliothèque ou bibliothèque inactive : le moteur réfléchit
            if (_pendule == null)
                LancerReflexion();  // Décompte le temps de réflexion (avec une pendule, c'est elle qui décompte)
            if (!LogiqueMouvements.EchecetMat)
            {
                InformationPourJoueur.Text = StatusProgramme.Text = _nomMoteur + " réfléchit ...";
            }
        }

        // ═══ Bibliothèque d'ouvertures : affichage des coups connus, et choix du coup quand le moteur doit jouer ═══
        private static readonly Random _hasard = new();
        private Font _policeBiblioNormale, _policeBiblioGras;     // créées une seule fois (une police par ligne serait une fuite)
        private string ChoisirCoupBibliotheque(string fen)
        {   // Le moteur doit jouer : coup choisi dans la bibliothèque (PolyglotBibliothèque.ChoisirEntree), marqué ⭐ dans la liste ;
            // null s'il n'y en a pas
            List<EntréePolyglot> entrees = PolyglotBibliothèque.TrouverLesEntrées(PolyglotBibliothèque.CalculeClefPolyglot(fen)).ToList();
            EntréePolyglot choisie = PolyglotBibliothèque.ChoisirEntree(entrees, _bibliothèqueAléatoire, _hasard);
            AfficheCoupsBibliotheque(entrees, choisie);
            return choisie == null ? null : PolyglotBibliothèque.DecodeCoup(choisie.CoupBiblio);
        }
        private void AfficheCoupsBibliotheque(string fen)
        {   // Coups connus de la bibliothèque pour cette position (sans en choisir aucun)
            AfficheCoupsBibliotheque(PolyglotBibliothèque.TrouverLesEntrées(PolyglotBibliothèque.CalculeClefPolyglot(fen)).ToList(), null);
        }
        private void AfficheCoupsBibliotheque(List<EntréePolyglot> entrees, EntréePolyglot choisie)
        {   // Liste des coups, par poids décroissant ; le coup choisi (s'il y en a un) est marqué ⭐ en vert
            _policeBiblioNormale ??= new Font(CoupsBibliothèqueBox.Font, FontStyle.Regular);
            _policeBiblioGras ??= new Font(CoupsBibliothèqueBox.Font, FontStyle.Bold);
            CoupsBibliothèqueBox.Clear();
            CoupsBibliothèqueBox.SelectionAlignment = HorizontalAlignment.Center;
            CoupsBibliothèqueBox.SelectionColor = Color.Black;
            CoupsBibliothèqueBox.SelectionFont = _policeBiblioGras;
            CoupsBibliothèqueBox.AppendText("Bibliothèque\n--------------\n");
            CoupsBibliothèqueBox.SelectionAlignment = HorizontalAlignment.Left;
            if (entrees.Count == 0)
            {
                CoupsBibliothèqueBox.SelectionFont = _policeBiblioNormale;
                CoupsBibliothèqueBox.AppendText("Aucun coup trouvé dans la bibliothèque.\n");
                return;
            }
            foreach (EntréePolyglot entree in entrees.OrderByDescending(e => e.Poids))
            {
                bool estChoisie = entree == choisie;
                CoupsBibliothèqueBox.SelectionFont = estChoisie ? _policeBiblioGras : _policeBiblioNormale;
                CoupsBibliothèqueBox.SelectionColor = estChoisie ? Color.Green : Color.Black;
                CoupsBibliothèqueBox.AppendText($"{(estChoisie ? "⭐ " : " *  ")}{PolyglotBibliothèque.DecodeCoup(entree.CoupBiblio)} ({entree.Poids})\n");
            }
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Routines de Dessin
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // (cases, pièces, couleurs, inversion, curseur : voir VueEchiquier.cs)
        private void DessineSymbole(int IndexCase, LogiqueMouvements.TypeSymbole Symbole)
        {   // Symboles des mouvements possibles de la pièce sélectionnée, s'ils sont visibles
            if (_visuSymbole)
                _vue.DessineSymbole(IndexCase, Symbole);
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        //  Diverses méthodes
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void TempsReflexionSecondes_ValueChanged(object sender, EventArgs e)
        {   // Temps de réflexion (secondes) : analyses, et coups du moteur dans une partie sans pendule
            _dureeReflexionMilliSeconde = (int)TempsReflexionSecondes.Value * 1000;
            InformationsPartie.Text = "Temps de réflexion = " + (_dureeReflexionMilliSeconde / 1000).ToString() + " secondes";
        }
        private void ActiveBibliothèque_CheckedChanged(object sender, EventArgs e)
        {   // Activer ou non la bibliothèque (on lit la case : une bascule se décalerait si l'état initial différait)
            _bibliothèqueActive = ActiveBibliothèque.Checked;
        }
        private void ActiveAléatoire_CheckedChanged(object sender, EventArgs e)
        {   // Choisir un coup aléatoire ou le meilleur coup dans la bibliothèque
            _bibliothèqueAléatoire = ActiveAléatoire.Checked;
        }
        private void ActiveSon_CheckedChanged(object sender, EventArgs e)
        {   // Mettre ou enlever le son
            _emetUnSon = ActiveSon.Checked;
        }
        public void ActiverMenus(bool actif)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ActiverMenus(actif)));
                return;
            }
            MenuInterfaceGraphique.Enabled = actif;
        }
        private void Promo0_Click(object sender, EventArgs e)
        {   //  Gestion de la promotion de Pion
            PictureBox Promotion = (PictureBox)sender;
            int indexSelect = Convert.ToInt32(Promotion.Name[5..]);
            _selectionPromotion = LogiqueMouvements.QuiJoue == LogiqueMouvements.ColorPiece.Blanc ? ListeBlanche[indexSelect] : ListeNoire[indexSelect];
        }
        public void PlateauEnable(bool statut)
        {   // Est-ce au joueur de bouger les pièces ? (le plateau n'est de toute façon actif que si la partie est en cours : voir MetAJourPlateau)
            _plateauAutorise = statut;
            MetAJourPlateau();
        }
        private void MetAJourPlateau()
        {   // Pendant le parcours, les cases restent cliquables : le clic ramène à la partie (voir CaseMouseDown), sans jouer de coup.
            // Exception : partie PGN en lecture seule, où l'échiquier n'est jamais cliquable.
            // Avant toute partie, le clic affiche "Veuillez choisir votre couleur"
            if (!_vue.CasesCreees)
                return;     // cases pas encore créées
            // Pendant la pause (clic sur une pendule), on ne joue pas : l'échiquier est bloqué
            bool actif = !_pauseJoueur && (ParcoursEnCours
                ? !PartieEnLectureSeule
                : _plateauAutorise && (_partie.Mode == ModePartie.EnCours || _partie.Mode == ModePartie.AucunePartie));
            _vue.ActiverCases(actif);
        }

        // ═══ Etat de la partie et commandes actives ═══
        // Le mode de la partie (_partie.Mode, voir Partie.cs) et quelques faits (parcours, coups joués, tour du joueur) suffisent
        // à décider de toutes les commandes : MetAJourCommandes les calcule en un seul endroit. Ne pas écrire .Enabled ailleurs
        // pour ces commandes, changer l'état puis appeler MetAJourCommandes.
        private bool PartieEnLectureSeule => _partie.Mode == ModePartie.LectureSeule;

        private void MetAJourCommandes()
        {
            bool enCours = _partie.EnCours;
            bool coupsJoues = LogiqueMouvements.ListeCoups.Count > IndexPremierePosition + 1;   // au moins un coup (hors position FEN de départ)
            // Retour arrière : annule le dernier coup de la partie, donc jamais pendant le parcours (il annulerait un coup autre que celui affiché)
            RetourArriere.Enabled = (enCours || _partie.Mode == ModePartie.Terminee) && coupsJoues && !ParcoursEnCours;
            groupParcoursPartie.Enabled = ListeCoupsBouton.Enabled = coupsJoues;
            // Reprendre ici : pendant le parcours (position passée), ou sur une partie PGN en lecture seule (y compris sa position finale)
            BoutonReprendreIci.Enabled = (ParcoursEnCours && _partie.Mode != ModePartie.AucunePartie) || PartieEnLectureSeule;
            _clavierActif = coupsJoues;     // flèches du clavier (Echap les coupe jusqu'au prochain calcul)
            AnalysePosition.Enabled = _partie.Mode != ModePartie.AucunePartie;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = enCours;
            OrdinateurJoue.Enabled = enCours;
            BoutonBalises.Enabled = SaisiePartieBouton.Enabled = !PartieEnLectureSeule;
            MetAJourPlateau();
        }
        private void TourneEchiquier()
        {   // Tourne l'échiquier de 180° (vue côté Blancs / côté Noirs) et redessine la position affichée :
            // la position passée pendant le parcours, sinon la partie
            _vue.Tourner(_positionAffichee ?? LogiqueMouvements.PositionActuelle);
        }
        private void ParametresJoueurHumain(string Affichage)
        {   // Message au joueur humain en début de partie (sa couleur est fixée par CommencerPartie)
            InformationPourJoueur.Visible = true;
            InformationPourJoueur.Text = StatusProgramme.Text = Affichage;
        }
        private bool RécupèreBibliothèque()
        {   // Charge la bibliothèque _bibliotheque ; false (avec un message) si elle est introuvable ou illisible :
            // la précédente reste alors active, ou, au démarrage, le moteur joue sans bibliothèque
            var polyglot = new PolyglotBibliothèque();
            polyglot.MessageLog += msg => CoupsBibliothèque.Text = msg;
            try
            {
                polyglot.PolyglotBibliothèqueLecture(_bibliotheque);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                Debug.WriteLine("[Bibliothèque] " + ex.Message);
                if (!PolyglotBibliothèque.Disponible)
                    CoupsBibliothèque.Text = "Aucune bibliothèque d'ouvertures";
                KryptonMessageBox.Show(ex.Message + (PolyglotBibliothèque.Disponible ? "\n\nLa bibliothèque précédente reste utilisée."
                                                                                     : "\n\nLe moteur jouera sans bibliothèque d'ouvertures."),
                    "Bibliothèque d'ouvertures", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return false;
            }
            CoupsBibliothèque.SelectAll();
            CoupsBibliothèque.SelectionAlignment = HorizontalAlignment.Center;
            CoupsBibliothèque.DeselectAll();
            AfficheCoupsBibliotheque(LogiqueMouvements.RetourneChaineFenActuel());     // coups connus pour la position actuelle
            return true;
        }
        private void MiseaZeroAffichages()
        {   // Réinitialise les affichages de la partie et du moteur
            Outils.MiseaZeroListes();
            MiseaZeroVariantes();
        }
        private void MiseaZeroVariantes()
        {   // Réinitialise les affichages de variantes et d'évaluation
            VarianteMoteurUci1.Text = VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = "...";
            VarianteMoteurCourante.Text = ScoreMoteur.Text = EvaluationUci.Text = "...";
        }
        private void MiseaZeroParcours()
        {
            VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = "...";
            StatusProgramme.Text = InformationsPartie.Text = "Parcours partie";
        }
        // ═══ Position affichée (parcours de la partie) ═══
        // L'échiquier montre soit la partie (LogiqueMouvements.PositionActuelle), soit une position passée (_positionAffichee).
        // Parcourir ne modifie jamais la partie : le moteur peut jouer pendant le parcours, et l'humain rejoue après être revenu
        // à la position courante (clic sur l'échiquier ou "Fin").
        private Position _positionAffichee;     // null : l'échiquier montre la partie
        private int _indexAffiche;              // index dans ListeCoups de la position affichée (-1 : position initiale FenDepart)
        private bool ParcoursEnCours => _positionAffichee != null;
        private static int IndexPremierePosition =>     // -1 : position initiale ; 0 : partie commencée depuis un FEN
            LogiqueMouvements.ListeCoups.Count > 0 && LogiqueMouvements.ListeCoups[0].EstPositionDeDepart ? 0 : -1;
        private Position PositionDesVariantes =>        // position sur laquelle sont convertis les coups du moteur (variantes, conseil)
            _pilote.AnalyseEnCours ? _pilote.PositionAnalysee : LogiqueMouvements.PositionActuelle;

        public void AfficheCoupDeLaPartie(int index)
        {   // Affiche la position après le coup n° index de ListeCoups (-1 : position initiale), sans modifier la partie.
            // Au-delà du dernier coup : retour à la position courante
            int dernier = LogiqueMouvements.ListeCoups.Count - 1;
            if (index >= dernier)
            {
                RetourPositionCourante();
                return;
            }
            index = Math.Max(index, IndexPremierePosition);
            if (_pilote.AnalyseEnCours)
                AbandonneReflexion();       // l'analyse portait sur la position affichée jusqu'ici (la réflexion du moteur pour son coup continue)
            string fen = index < 0 ? FenDepart : LogiqueMouvements.ListeCoups[index].Fen;
            _vue.DernierCoupMasque = true;  // le dernier coup de la partie n'a pas de sens sur une position passée
            _positionAffichee = LogiqueMouvements.PositionDepuisFen(fen);
            _indexAffiche = index;
            MetAJourCommandes();
            _vue.DessinePosition(_positionAffichee);
            AfficheCoupsBibliotheque(fen);
            InformationPourJoueur.Text = "Trait aux " + NomCamp(_positionAffichee.QuiJoue);
            VarianteMoteurUci1.Text = index < 0 || LogiqueMouvements.ListeCoups[index].EstPositionDeDepart
                ? "   [ Position initiale ]" : $"   [ {TexteCoupJoue(index, _positionAffichee)} ]";
            MiseaZeroParcours();
            AffichePendules();      // partie sans pendule en cours (ex : PGN chargé) : temps notés à cette position
            if (!PartieEnLectureSeule)
                InformationsPartie.Text = "Parcours : Fin ou clic pour revenir";
        }
        private static string TexteCoupJoue(int index, Position positionApres)
        {   // Ex : "Coup blanc : 12. Cf3" ou "Coup noir : 12... Fe7" (coup n° index, qui a mené à positionApres)
            string coup = LogiqueMouvements.ListeCoupsNal[index];
            coup = coup.Contains('.') ? coup.Split('.').Last().Trim() : coup.Trim();
            int numero = (int)Math.Floor(positionApres.NombreCoupsJoues - 0.5f);   // numéro du coup qui vient d'être joué
            return positionApres.QuiJoue == ColorPiece.Blanc ? $"Coup noir : {numero}... {coup}" : $"Coup blanc : {numero}. {coup}";
        }
        public void RetourPositionCourante()
        {   // Fin du parcours : l'échiquier montre de nouveau la partie
            if (!ParcoursEnCours)
                return;
            _positionAffichee = null;
            MetAJourCommandes();
            LogiqueMouvements.DessinPieces();
            _vue.DernierCoupMasque = false;     // le dernier coup du moteur est de nouveau coloré
            AfficheCoupsBibliotheque(LogiqueMouvements.RetourneChaineFenActuel());
            InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + NomCamp(QuiJoue);
            InformationsPartie.Text = PartieEnLectureSeule ? "Fin de la partie" : "";
            AffichePendules();
        }
        private void QuitteParcours()
        {   // La partie va être remplacée (nouvelle partie, chargement) : l'échiquier suivra la partie
            _positionAffichee = null;
            _vue.DernierCoupMasque = false;
            MetAJourCommandes();
        }
        private void DessinePieceDeLaPartie(int IndexCase, LogiqueMouvements.TypePiece Piece)
        {   // Dessins demandés par la partie (coups joués...) : ignorés pendant le parcours, l'échiquier est redessiné au retour
            if (!ParcoursEnCours)
                _vue.DessinePiece(IndexCase, Piece);
        }

        private void AbandonneReflexion()
        {   // Rend périmée la réflexion en cours (partie ou analyse) : le moteur s'arrête et sa réponse sera ignorée.
            // A appeler avant toute action qui change la partie ou la position (retour arrière, résultat, nouvelle partie, chargement...)
            if (!_pilote.Abandonner())
                return;     // le moteur ne réfléchissait pas : rien à signaler
            MiseaZéroTimer();
            LeMoteurARépondu();
            InformationPourJoueur.Text = StatusProgramme.Text = "Réflexion du moteur interrompue";
        }
        private void LeMoteurARépondu()
        {   // Après que le moteur a répondu (ou a été interrompu) : état des commandes
            MetAJourCommandes();
        }

        private TimeSpan _debutReflexion;       // heure (_chrono) du début de la réflexion à temps fixe
        private void LancerReflexion()
        {   // Réflexion à temps fixe (analyse, ou coup du moteur sans pendule) : la barre se remplit pendant ce temps
            // (pendant la réflexion, tout reste possible : les actions qui la rendent inutile l'abandonnent, voir AbandonneReflexion)
            _debutReflexion = _chrono.Elapsed;
            BarreReflexion.Value = 0;
            BarreReflexion.Visible = true;
            timer.Interval = 100;
            timer.Tick -= Timer_Tick;
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {   // Tous les dixièmes de seconde : part du temps de réflexion écoulée (la barre reste pleine jusqu'à la réponse du moteur)
            double ecoule = (_chrono.Elapsed - _debutReflexion).TotalMilliseconds / Math.Max(1, _dureeReflexionMilliSeconde);
            BarreReflexion.Value = (int)Math.Round(Math.Min(1, ecoule) * BarreReflexion.Maximum);
            if (ecoule >= 1)
                timer.Stop();
        }
        private void MiseaZéroTimer()
        {   // Le moteur a répondu (ou la réflexion est abandonnée) : plus de barre
            timer.Stop();
            BarreReflexion.Visible = false;
            BarreReflexion.Value = 0;
            InformationsPartie.Text = "";
        }


    }
}
