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
using System.Runtime.InteropServices;
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
            ListePendule.DrawMode = DrawMode.OwnerDrawFixed;    // étoile dorée des cadences officielles
            ListePendule.DrawItem += PartieForceModule.DessineCadence;
            ChoisitCadence(parametres.Cadence);
            _minuteriePendule.Tick += MinuteriePendule_Tick;
            PenduleBlanc.Click += Pendule_Click;        // un clic sur une pendule : pause / reprise
            PenduleNoir.Click += Pendule_Click;
            PenduleBlanc.ReduitPourTenir = PenduleNoir.ReduitPourTenir = true;     // "1:30:00" toujours lisible en entier
            _infobulleBilan.SetToolTip(BilanAnalyse,
                "Précision : 100 % = tous les coups aussi bons que ceux du moteur (formule de Lichess).\n" +
                "Imprécision (?!), erreur (?), gaffe (??) : le coup fait perdre au moins 10, 20 ou 30 %\n" +
                "des chances de gain (une perte dans une position déjà gagnée compte peu).\n" +
                "Analyse complète : les coups douteux sont revus 10 s pour confirmer le jugement.\n" +
                "Coup critique : celui où la partie a basculé (la plus grosse perte) ; un clic l'affiche.");
            BilanAnalyse.Click += BilanAnalyse_Click;
            BoutonAnalysePartie.MouseEnter += MetAJourInfobullesAnalyse;
            BoutonAnalyseComplete.MouseEnter += MetAJourInfobullesAnalyse;
            foreach (RichTextBox ligne in new[] { VarianteMoteurUci1, VarianteMoteurUci2, VarianteMoteurUci3 })
            {   // lignes de variante 1 à 3 : centrées et en gras pour le coup regardé, son analyse et la suite prévue (voir AfficheLigneCentree)
                _policesLignes[ligne] = (ligne.Font, new Font(ligne.Font, FontStyle.Bold));
                ligne.TextChanged += LigneVariante_TextChanged;
                // Texte centré en hauteur et décalé du bord gauche (choix de Bruno) : zone de texte réglée par EM_SETRECT, qui ne vaut
                // que pour une zone multiligne (sans retour à la ligne automatique, elle reste sur une ligne)
                ligne.Multiline = true;
                ligne.WordWrap = false;
                ligne.HandleCreated += (s, e) => MargesLigne((RichTextBox)s);
                ligne.Resize += (s, e) => MargesLigne((RichTextBox)s);
                if (ligne.IsHandleCreated)
                    MargesLigne(ligne);
            }
            FeuilleDesCoups.CoupClique += FeuilleDesCoups_CoupClique;           // clic sur un coup de la feuille : sa position
            FeuilleDesCoups.CoupCliqueDroit += FeuilleDesCoups_CoupCliqueDroit; // clic droit : annotations (!!, !, !?, ?!, ?, ??)
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
               {   // On note dans le journal, on ne bloque pas le démarrage si la MAJ échoue (hors ligne, GitHub indisponible,
                   // fichier verrouillé... : les causes possibles sont trop variées pour être listées, d'où le catch général)
                   Journal.Info("Pas de mise à jour automatique de Stockfish : " + ex.Message);
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
            {   // un clic sur l'échiquier ne doit jamais arrêter la partie : l'erreur est notée dans le journal pour être corrigée
                Journal.Erreur("Clic sur une case de l'échiquier", ex);
            }
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
                // Dernière case de la barre d'état : le dernier coup qui reste (celui annulé y était encore)
                Coup dernierCoup = LogiqueMouvements.ListeCoups.LastOrDefault(c => !c.EstPositionDeDepart);
                VarianteMoteurCourante.Text = dernierCoup != null ? "Coup joué : " + dernierCoup.PgnFrNumerote : "Position de départ";
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
        // Version de l'application, lue dans l'assembly (<Version> du .csproj) : "1.16" (le mineur sur deux chiffres : "1.05")
        public static string VersionAffichee
        {
            get
            {
                Version version = typeof(EchiquierPrincipal).Assembly.GetName().Version ?? new Version(0, 0);
                return $"{version.Major}.{version.Minor:00}";
            }
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
            // Version 1.12 = coup du moteur affiché dans le cadre vert, pause par clic sur une pendule, cadence FIDE
            //                (90 + 30 min, +30 s, movestogo), temps %clk et %emt relus et affichés au parcours, PlyCount juste,
            //                fichiers PGN aux encodages mélangés lus ligne par ligne
            // Version 1.13 = feuille de partie intégrée à droite de l'échiquier (composant maison, couleur papier, clic sur un coup
            //                pour l'afficher), annotations !! ! !? ?! ? ?? (clic droit, en couleur, écrites et relues dans le PGN,
            //                codes $1 à $6 compris), coup de bibliothèque dans la barre d'état, bouton "Setup position" (à venir)
            // Version 1.14 = analyse de partie (rapide 3 s par position, ou complète avec les coups douteux revus 10 s), de la fin vers
            //                le début comme ChessBase, toujours à pleine force ; annotations proposées, précision en % et bilan,
            //                coup critique (clic), flèches du coup joué et du meilleur coup, évaluation et meilleure suite au parcours,
            //                PGN enregistré avec %eval et la meilleure variante après les erreurs
            // Version 1.15 = courbe d'évaluation verticale le long de la feuille (barres façon ChessBase), annotations pendant
            //                l'analyse, cadre "Bilan partie", tournoi au-dessus de la feuille, résultat sur la feuille, feuille
            //                sur fond blanc, cadences KO / FIDE (étoile dorée), PGN en CRLF ajouté sans réécrire le fichier,
            //                bibliothèque aléatoire par défaut, plantage pendant le parcours d'une analyse corrigé
            // Version 1.16 = saisie d'une position à la main, annotations « ! » (seul bon coup) et « !! » (avec sacrifice), PGN relu
            //                avec commentaires, évaluations et variantes (meilleur coup des coups annotés), marque [%auto] des
            //                annotations de l'analyse, lignes de variantes plus grandes et centrées, aide complétée
            // (le numéro de version vient du .csproj, <Version> : il n'est plus écrit ici)
            _ = KryptonMessageBox.Show("      BrunoGUI GenII\n       Version " + VersionAffichee + "\n--  Bruno COURTOIS  -- " +
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
            {   // On gère les messages (ex: "Déjà à jour" ou "Pas de connexion") ; causes trop variées pour être listées
                Journal.Info("Mise à jour de Stockfish non faite : " + ex.Message);
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
                Journal.Info("Préférences non enregistrées : " + ex.Message);
            }
        }
        private void KryptonQuitter_Click(object sender, EventArgs e)
        {   // Note : Envoie l’événement FormClosing puis l’événement FormClosed (après la fermeture complète)
            // Et détruit les contrôles du formulaire
            Close();
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
            // Pendant la pause (clic sur une pendule) et l'analyse de la partie, on ne joue pas : l'échiquier est bloqué
            bool actif = !_pauseJoueur && _analyseDePartie == null && (ParcoursEnCours
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
            groupParcoursPartie.Enabled = coupsJoues;
            // Reprendre ici : pendant le parcours (position passée), ou sur une partie PGN en lecture seule (y compris sa position finale)
            BoutonReprendreIci.Enabled = (ParcoursEnCours && _partie.Mode != ModePartie.AucunePartie) || PartieEnLectureSeule;
            _clavierActif = coupsJoues;     // flèches du clavier (Echap les coupe jusqu'au prochain calcul)
            AnalysePosition.Enabled = _partie.Mode != ModePartie.AucunePartie;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = enCours;
            OrdinateurJoue.Enabled = enCours;
            BoutonBalises.Enabled = SaisiePartieBouton.Enabled = !PartieEnLectureSeule;
            BoutonSaisiePosition.Enabled = _analyseDePartie == null;      // (pas pendant une analyse de partie)
            MetAJourPlateau();
            MetAJourFeuille();      // (appelée partout où la partie ou le parcours change : la feuille les suit d'ici)
        }
        private void MetAJourFeuille()
        {   // Feuille de partie à droite de l'échiquier : tous les coups, et le coup de la position affichée surligné
            // (pendant le parcours, le coup regardé ; sinon le dernier coup joué ; rien pour la position de départ)
            // Partie finie : son résultat sous le dernier coup, comme sur une feuille papier
            int indexAffiche = ParcoursEnCours ? _indexAffiche : LogiqueMouvements.ListeCoups.Count - 1;
            FeuilleDesCoups.MetAJour(LogiqueMouvements.ListeCoups, indexAffiche, PartieEnCours.Result);
            AfficheTournoi();
        }
        private void AfficheTournoi()
        {   // Au-dessus de la feuille, à droite des joueurs : le tournoi (balise Event, sur deux lignes), puis la ronde et la date
            // (choix de Bruno : le lieu ne tient pas ; infobulle : le tout, lieu compris)
            static string Valeur(string balise) => string.IsNullOrWhiteSpace(balise) || balise.Trim() is "?" or "-" ? "" : balise.Trim();
            string date = Valeur(PartieEnCours.Date);
            string[] morceaux = date.Split('.');            // "2026.04.19" -> "19/04/2026" (les parties inconnues "??" sont omises)
            if (morceaux.Length == 3)
                date = string.Join("/", morceaux.Reverse().Where(m => m.All(char.IsDigit) && m != ""));
            string ronde = Valeur(PartieEnCours.Ronde);
            string details = string.Join("  ·  ", new[] { ronde != "" ? "Ronde " + ronde : "", date }.Where(t => t != ""));
            string tournoi = Valeur(PartieEnCours.Tournoi), lieu = Valeur(PartieEnCours.Lieu);
            if (LabelTournoi.Text == tournoi && LabelDetailsTournoi.Text == details)
                return;
            LabelTournoi.Text = tournoi;
            LabelDetailsTournoi.Text = details;
            string complet = string.Join("\n", new[] { tournoi, lieu, details }.Where(t => t != ""));
            _infobulleBilan.SetToolTip(LabelTournoi, complet);
            _infobulleBilan.SetToolTip(LabelDetailsTournoi, complet);
        }
        private void FeuilleDesCoups_CoupClique(int index)
        {   // Clic sur un coup de la feuille : on affiche la position après ce coup (le dernier coup ramène à la partie)
            AfficheCoupDeLaPartie(index);
        }
        private void FeuilleDesCoups_CoupCliqueDroit(int index, Point position)
        {   // Clic droit sur un coup de la feuille : menu des annotations (!!, !, !?, ?!, ?, ??), l'annotation actuelle cochée.
            // L'annotation est rangée dans le coup (Coup.Annotation) et écrite dans le PGN
            Coup coup = LogiqueMouvements.ListeCoups[index];
            ContextMenuStrip menu = new();
            foreach (string annotation in Annotations.Toutes)
                menu.Items.Add(ElementAnnotation(coup, annotation, $"{annotation}\t{Annotations.Nom(annotation)}"));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(ElementAnnotation(coup, "", Annotations.Nom("")));
            menu.Closed += (s, e) => BeginInvoke(new Action(menu.Dispose));     // libéré après le traitement du clic
            menu.Show(FeuilleDesCoups, position);
        }
        private Font _policeMenuAnnotations;    // créée une seule fois (une police par élément et par clic droit ne serait jamais libérée)
        private ToolStripMenuItem ElementAnnotation(Coup coup, string annotation, string texte)
        {
            _policeMenuAnnotations ??= new Font(Font, FontStyle.Bold);
            ToolStripMenuItem element = new(texte)
            {
                Checked = coup.Annotation == annotation,
                ForeColor = FeuilleCoups.CouleurAnnotation(annotation),
                Font = _policeMenuAnnotations
            };
            element.Click += (s, e) =>
            {
                coup.Annotation = annotation;
                coup.AnnotationProposee = false;    // choisie par le joueur : plus une proposition de l'analyse
                MetAJourFeuille();
            };
            return element;
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



    }
}
