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
        private readonly Dictionary<LogiqueMouvements.TypePiece, Bitmap> ListeBitmapsPiece = []; // liste des Bitmaps pour les pièces
        private readonly Dictionary<LogiqueMouvements.TypeSymbole, Bitmap> ListeBitmapsSymbole = []; // liste des Bitmaps pour les symboles
        private readonly List<PictureBox> PictJeux = []; // les 120 cases du jeu
        private readonly List<LogiqueMouvements.TypePiece> ListeNoire = // Reine, Tour, Fou et Cavalier noirs pour promotion
            [LogiqueMouvements.TypePiece.ReineNoire, LogiqueMouvements.TypePiece.TourNoire, LogiqueMouvements.TypePiece.FouNoir, LogiqueMouvements.TypePiece.CavalierNoir];
        private readonly List<LogiqueMouvements.TypePiece> ListeBlanche =   // Reine, Tour, Fou et Cavalier blancs pour promotion
            [LogiqueMouvements.TypePiece.ReineBlanche, LogiqueMouvements.TypePiece.TourBlanche, LogiqueMouvements.TypePiece.FouBlanc, LogiqueMouvements.TypePiece.CavalierBlanc];
        private readonly List<Color> CouleurCaseOrigines = []; // Couleurs d'origine des cases
        private readonly List<int> IndiceVisuCoteNoir = [];
        private List<string> ListeParties = [];
        private List<PartieEchecsPGN> ListePartiesPGN = [];

        // les Bitmaps
        private readonly Bitmap PionBlanc = new(Properties.Resources.PionBlanc);
        private readonly Bitmap TourBlanche = new(Properties.Resources.TourBlanche);
        private readonly Bitmap CavalierBlanc = new(Properties.Resources.CavalierBlanc);
        private readonly Bitmap FouBlanc = new(Properties.Resources.FouBlanc);
        private readonly Bitmap ReineBlanche = new(Properties.Resources.ReineBlanche);
        private readonly Bitmap RoiBlanc = new(Properties.Resources.RoiBlanc);
        private readonly Bitmap PionNoir = new(Properties.Resources.PionNoir);
        private readonly Bitmap TourNoire = new(Properties.Resources.TourNoire);
        private readonly Bitmap CavalierNoir = new(Properties.Resources.CavalierNoir);
        private readonly Bitmap FouNoir = new(Properties.Resources.FouNoir);
        private readonly Bitmap ReineNoire = new(Properties.Resources.ReineNoire);
        private readonly Bitmap RoiNoir = new(Properties.Resources.RoiNoir);
        private readonly Bitmap CercleVert = new(Properties.Resources.SansPrise); // mouvement autorisé sans prise
        private readonly Bitmap CercleRouge = new(Properties.Resources.Menace); // menace pour la pièce sélectionnée
        private readonly Bitmap CercleViolet = new(Properties.Resources.Interdit); // mouvement interdit
        private readonly Bitmap CroixPriseVerte = new(Properties.Resources.AvecPrise); // mouvement autorisé avec prise
        // private readonly Bitmap CerclePrise = new(Properties.Resources.AvecPrise); // mouvement autorisé avec prise

        // les variables
        public static int NumeroDemiCoup { get; set; } = 0;
        private readonly Partie _partie = new();    // mode de la partie, qui joue quel camp (voir Partie.cs)
        public Partie PartieCourante => _partie;
        public string _dossierRacine, _dossierStockfish;
        private int _indexSource120, _forceMoteurElo, _nombreLignesPV, _tempsRestant;
        private int _dernierCoupMoteurUci;   // dernière case jouée par le moteur UCI
        private int _numeroLigne;       // Indices dans la DataGrid FeuillePartie
        private int _indexCaseSourceDernierMouvement, _indexCaseDestinationDernierMouvement;
        private bool _plateauAutorise = true;   // c'est au joueur de bouger les pièces (voir PlateauEnable et MetAJourPlateau)
        private bool _dernierCoupColore;        // les cases du dernier coup du moteur sont à montrer (masquées pendant le parcours)
        private string _caseSource, _caseDestination;
        private string _nomHumain, _joueurElo, _nomMoteur, _moteurElo, _joueurBlanc, _joueurNoir;
        private string _cheminMoteur, _nomMoteurChoisi;
        private string _bibliotheque = "rodent.bin";
        private bool _clickCaseSource, _visuSymbole, _montreDonneesBrutesUci, _montre3VariantesUci, _montreListeParties;
        private bool _visuCoteNoir;     // True quand les Noirs sont en bas de l'écran
        private bool _clavierActif, _emetUnSon, _bibliothèqueAléatoire;
        private bool _bibliothèqueActive = true;
        private int _dureeReflexionMilliSeconde = 5000;
        private Color _couleurCaseSombre, _couleurCaseClaire, _couleurCaseSource, _couleurCaseDestination;
        private LogiqueMouvements.TypePiece _selectionPromotion, _pieceSource;
        private readonly Color _violetCustom = Color.FromArgb(128, 128, 255);  // Rouge = 128, Vert = 128, Bleu = 255

        // les classes
        public LogiqueMouvements LogiqueMouvements = new();
        public MoteurUci MoteurUci = new();
        private readonly PiloteMoteur _pilote;      // ce qui est demandé au moteur (coup de partie ou analyse) : voir PiloteMoteur.cs
        public GestionPartiePgn GestionPartiePgn = new();
        public PartieForceModule maNouvellePartieForceModule = new();
        public PartieEchecsPGN PartieEnCours = new();
        public ParametresUciStockfish mesParametresUciStockfish = new();
        public ParametresDeBase mesparametresDeBase;        // mesparametresDeBase est déclarée, mais elle n’est instanciée qu'après "InitializeComponent();"
        private FenetrePartie mafenetrePartie;              // mafenetrePartie est déclarée, mais elle n’est pas encore instanciée. A instancier dans une méthode
        private AffichePgn affichePgn = new();   // affichePgn est à la fois déclarée et instanciée. Prêt à être utilisé dès le début
        private FichierPartiePgn fichierPartiePgn = new();     // idem pour fichierPartiePgn
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
            _couleurCaseSombre = Parametres.ConvertitCouleur(parametres.CaseSombre, Parametres.LichessCaseSombre);
            _couleurCaseClaire = Parametres.ConvertitCouleur(parametres.CaseClaire, Parametres.LichessCaseClaire);
            _couleurCaseSource = Parametres.ConvertitCouleur(parametres.CouleurCaseSource, Parametres.LichessCaseSource);
            _couleurCaseDestination = Parametres.ConvertitCouleur(parametres.CouleurCaseDestination, Parametres.LichessCaseDestination);
            _nomHumain = parametres.NomHumain;
            _joueurElo = parametres.EloHumain;
            _dureeReflexionMilliSeconde = parametres.DureeReflexionSeconde * 1000;
            _nomMoteur = parametres.Moteur;
            _forceMoteurElo = parametres.ForceMoteur;
            _nombreLignesPV = MoteurUci.NombreLignesPV = parametres.NombreLignesPV;
            _bibliotheque = parametres.Bibliotheque;
            LabelJoueurNoir.Text = parametres.Moteur;
            EloNoir.Text = _moteurElo = _forceMoteurElo.ToString();
            LabelJoueurBlanc.Text = _nomHumain;
            EloBlanc.Text = _joueurElo;
            // Préférences : la fenêtre "Nouvelle partie" propose les derniers choix, le curseur reprend le dernier temps de réflexion
            maNouvellePartieForceModule.ChoixCouleur = parametres.CouleurMoteur;
            maNouvellePartieForceModule.ForceMaximale = parametres.ForceMaximale;
            maNouvellePartieForceModule.ForceModule = parametres.ForceMoteur;
            maNouvellePartieForceModule.DureeReflexionSeconde = parametres.DureeReflexionSeconde;
            maNouvellePartieForceModule.NomAdversaire = parametres.NomHumain;
            TrackBarTempsReflexion.Value = Math.Clamp(parametres.DureeReflexionSeconde, TrackBarTempsReflexion.Minimum, TrackBarTempsReflexion.Maximum);
            labelTempsReflexion.Text = "[" + TrackBarTempsReflexion.Value + "]";
            // Debug pour vérifier
            Debug.WriteLine($"Paramètres chargés : Moteur = {_cheminMoteur}, Case sombre = {_couleurCaseSombre.Name}, Case claire = {_couleurCaseClaire.Name}");
            Debug.WriteLine($"Paramètres chargés : Case source = {_couleurCaseSource.Name}, Case destination = {_couleurCaseDestination.Name}");
            Debug.WriteLine($"Paramètres chargés : Biblio = {_bibliotheque}, Force = {_forceMoteurElo}, Nombre PV = {_nombreLignesPV}");
            Debug.WriteLine($"Paramètres chargés : Temps de réflexion = {_dureeReflexionMilliSeconde}");

            DateTime Aujourdhui = DateTime.Today;
            _visuCoteNoir = false;      // On commence avec la vue côté Blanc (par défaut, l'ordinateur a les Noirs : voir Partie)
            _montreDonneesBrutesUci = _clavierActif = false;
            _pilote = new PiloteMoteur(MoteurUci)
            {   // Bibliothèque d'ouvertures (si elle est active) : le coup choisi est aussi affiché dans la liste de la bibliothèque
                ChoixBibliotheque = fen => _bibliothèqueActive ? AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen)) : null
            };
            PartieEnCours.Date = Aujourdhui.ToString("yyyy.MM.dd");
            PartieEnCours.Lieu = "Maison"; PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Result = "*";
            // En-têtes PGN de départ = joueurs affichés (humain avec les Blancs, moteur avec les Noirs), repris par la fenêtre "Entête PGN"
            PartieEnCours.White = _nomHumain;
            PartieEnCours.WhiteElo = string.IsNullOrWhiteSpace(_joueurElo) ? "?" : _joueurElo;
            PartieEnCours.Black = parametres.Moteur;
            PartieEnCours.BlackElo = _forceMoteurElo.ToString();
            mesparametresDeBase = new ParametresDeBase(this);
        }

        private void BrunoInterfaceGraphique_Load(object sender, EventArgs e)
        {   // Forme Interface graphique
            // les évènements dans les classes
            LogiqueMouvements.AfficheCoupNoir += AfficheCoupNoir;
            LogiqueMouvements.AfficheCoupBlanc += AfficheCoupBlanc;
            LogiqueMouvements.AfficheInfoEchec += AfficheInfoEchec;
            LogiqueMouvements.AfficheEchecEtMat += AfficheEchecEtMat;
            LogiqueMouvements.AfficheTour += AfficheTour;
            LogiqueMouvements.AffichePromotionPion += AffichePromotionPion;
            LogiqueMouvements.DessinePiece += DessinePieceDeLaPartie;     // ignoré pendant le parcours de la partie
            LogiqueMouvements.DessineSymbole += DessineSymbole;
            MoteurUci.AfficheUci += AfficheUci;
            MoteurUci.AfficheDonneesBrutes += AfficheDonneesBrutes;
            MoteurUci.AfficheCoupMoteur += AfficheCoupMoteur;
            this.KeyPreview = true; // <-- obligatoire pour capter toutes les touches
            // Liste des pièces du jeu
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.PionBlanc, PionBlanc);        // pion blanc
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.TourBlanche, TourBlanche);    // tour blanche
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.CavalierBlanc, CavalierBlanc);// cavalier blanc
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.FouBlanc, FouBlanc);          // fou blanc
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.ReineBlanche, ReineBlanche);  // reine blanche
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.RoiBlanc, RoiBlanc);          // roi blanc
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.PionNoir, PionNoir);          // pion noir
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.TourNoire, TourNoire);        //tour noire
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.CavalierNoir, CavalierNoir);  // cavalier noir
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.FouNoir, FouNoir);            // fou noir
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.ReineNoire, ReineNoire);      // reine noire
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.RoiNoir, RoiNoir);            // roi noir
            ListeBitmapsPiece.Add(LogiqueMouvements.TypePiece.Vide, null);                  // case vide
            // Liste des symboles du jeu
            ListeBitmapsSymbole.Add(LogiqueMouvements.TypeSymbole.SymboleMenacePiece, CercleRouge); // symbole Menace
            ListeBitmapsSymbole.Add(LogiqueMouvements.TypeSymbole.SymboleMouvementSansPrise, CercleVert); // symbole Mouvement sans prise
            ListeBitmapsSymbole.Add(LogiqueMouvements.TypeSymbole.SymboleMouvementInterdit, CercleViolet); // symbole Mouvement interdit
            ListeBitmapsSymbole.Add(LogiqueMouvements.TypeSymbole.SymboleMouvementAvecPrise, CroixPriseVerte); // symbole Mouvement avec prise

            // Les couleurs des cases viennent de BrunoGUI.ini (voir le constructeur) ; le style Lichess est la valeur par défaut


            _dossierRacine = Chemins.RepertoireRacine;
            string cheminMoteurs = Chemins.MoteursUCI;
            string cheminPolyglot = Chemins.BibliothèquesPolyglot;

            _cheminMoteur = CheminStockfish;            // moteur lancé au démarrage
            _dossierStockfish = Path.Combine(_dossierRacine, "stockfish");

            Debug.WriteLine("Dossier Racine = " + _dossierRacine);
            Debug.WriteLine("Dossier Stockfish = " + _dossierStockfish);
            Debug.WriteLine("Chemin moteurs = " + cheminMoteurs);
            Debug.WriteLine("Chemin Polyglot = " + cheminPolyglot);

            InformationPourJoueur.Text = "   Bienvenue   ";

            DessineEchiquier();
            for (int i = 0; i <= 119; i++)
            {   // L'échiquier 120 cases (bordures comprises) est créé par la classe Position
                IndiceVisuCoteNoir.Add(i);      // on crée la liste de 1 à 120
            }
            IndiceVisuCoteNoir.Reverse();       // On inverse l'ordre pour avoir la liste de 120 à 1 pour la vue côté noir
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
            PartieEnCours.Black = LabelJoueurNoir.Text = _nomMoteur;
            // VarianteMoteurUci2.Text = "[INFO] Fin de la vérification de mise à jour de Stockfish...";
        }

        private void NouvellePartieStockfish_Click(object sender, EventArgs e)
        {   // Nouvelle partie contre Stockfish, avec la possibilité de régler la force du moteur et le temps de réflexion
            AbandonneReflexion();   // nouvelle partie
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            LogiqueMouvements.PartieEnCoursMat = LogiqueMouvements.PartieEnCoursPat = false;
            QuiJoue = ColorPiece.Blanc;
            NumeroDemiCoup = 0;
            DémarreStockfish();
            if (maNouvellePartieForceModule.ShowDialog() == DialogResult.OK)
            {   // Utilise les sélections faites par l'utilisateur
                string couleurMoteur = maNouvellePartieForceModule.ChoixCouleur;
                bool forceMaximale = maNouvellePartieForceModule.ForceMaximale;
                _forceMoteurElo = maNouvellePartieForceModule.ForceModule;
                _nomHumain = maNouvellePartieForceModule.NomAdversaire;     // mémorisé dans les préférences à la fermeture
                _dureeReflexionMilliSeconde = maNouvellePartieForceModule.DureeReflexionSeconde * 1000;
                TrackBarTempsReflexion.Value = maNouvellePartieForceModule.DureeReflexionSeconde;   // On met à jour la trackbar ...
                labelTempsReflexion.Text = "[" + TrackBarTempsReflexion.Value.ToString() + "]";

                MiseaZeroAffichages();
                if (forceMaximale)
                {   // Moteur à sa force Elo maximale
                    _forceMoteurElo = 3150;
                }
                MoteurUci.ActiveLimiteElo();
                MoteurUci.DefinitLimiteElo(_forceMoteurElo.ToString());
                MoteurUci.DefinitMultiPV(MoteurUci.NombreLignesPV);
                if (couleurMoteur == "Blancs")
                {   // Le moteur joue les blancs
                    PartieEnCours.White = LabelJoueurBlanc.Text = _nomMoteur;
                    PartieEnCours.WhiteElo = EloBlanc.Text = _forceMoteurElo.ToString();
                    PartieEnCours.Black = LabelJoueurNoir.Text = maNouvellePartieForceModule.NomAdversaire;
                    PartieEnCours.BlackElo = EloNoir.Text = _joueurElo;
                    CommencerPartie(Joueur.Moteur, Joueur.Humain);
                    if (_visuCoteNoir == false)
                        TourneEchiquier();      // On met la vue côté Noir
                    ParametresJoueurHumain("Noirs", "Le moteur UCI joue");      // On fait jouer le moteur côté blanc
                    JeuMoteurAvecBibliothèque(FenDepart);
                }
                else
                {   // Le moteur joue les noirs
                    PartieEnCours.White = LabelJoueurBlanc.Text = maNouvellePartieForceModule.NomAdversaire;
                    PartieEnCours.WhiteElo = EloBlanc.Text = _joueurElo;
                    PartieEnCours.Black = LabelJoueurNoir.Text = _nomMoteur;
                    PartieEnCours.BlackElo = EloNoir.Text = _forceMoteurElo.ToString();
                    if (_visuCoteNoir)
                        TourneEchiquier();
                    CommencerPartie(Joueur.Humain, Joueur.Moteur);
                    ParametresJoueurHumain("Blancs", "A vous de jouer");            // On demande à l'humain de jouer
                    PlateauEnable(true);                                            // On lui permet de bouger les pièces
                }
            }
        }
        private void CommencerPartie(Joueur blancs, Joueur noirs)      // POINT D'ENTREE POUR TOUTES LES NOUVELLES PARTIES
        {   // Début d'une nouvelle partie : qui joue les Blancs et les Noirs (humain ou moteur)
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            _partie.Commencer(blancs, noirs);
            EffaceDernierCoup();        // les cases du dernier coup de la partie précédente
            _dernierCoupMoteurUci = -1;
            _clickCaseSource = _visuSymbole = true;
            PartieEnCours.CoupsPartiePGN = PartieEnCours.Result = PartieEnCours.CompteDePLy = PartieEnCours.Ronde = "";
            PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Lieu = "Maison";
            NumeroDemiCoup = 0;
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
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
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
                    int IndexCase120 = Convert.ToInt32(CaseClick.Name[8..]); // Utilise le numéro de la PictureBox comme index
                    if (_visuCoteNoir)
                        IndexCase120 = IndiceVisuCoteNoir[IndexCase120];    // Si on regarde côté noir, il faut inverser l'index par rapport a la vue côté blanc
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
                            if (PictJeux[IndexCase120].Image != null)
                            {   // Si la case cliquée contient bien une pièce ou un pion, on va utiliser le thumbnail de la pièce comme curseur :-)
                                using (Bitmap Piece = new(PictJeux[IndexCase120].Image))
                                {   // Quand on bouge la souris, on bouge le thumbnail de la pièce comme un curseur :-)
                                    Bitmap thumbnail = (Bitmap)Piece.GetThumbnailImage(88, 88, null, IntPtr.Zero);
                                    Cursor = new Cursor(thumbnail.GetHicon());
                                }
                                DessinePiece(IndexCase120, LogiqueMouvements.TypePiece.Vide);   // On vide la case d'origine car le joueur bouge la pièce ...
                                LogiqueMouvements.DessineMouvements(_caseSource, true);
                                _clickCaseSource = false;
                            }
                        }
                        else
                        {       // Déplacement d'une pièce
                            LogiqueMouvements.Echec = false;
                            Cursor = Cursors.Default;       // On revient au curseur "normal"
                            LogiqueMouvements.EffaceSymboles(true);
                            _caseDestination = LogiqueMouvements.NomCaseAlgebrique(IndexCase120);
                            AbandonneReflexion();   // une analyse en cours porterait sur la position d'avant ce coup
                            LogiqueMouvements.ExecutionCoup(_caseSource, _caseDestination);
                            string chaineFen = LogiqueMouvements.RetourneChaineFenActuel(); // UCI : remplacer le FEN par liste de coups ?!
                            if (LogiqueMouvements.CoupValide)
                            {   // envoi de la Position Fen au moteur UCI
                                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(chaineFen));
                                if (_partie.MoteurAuTrait)      // c'est au moteur de répondre (pas après un mat ou un pat : partie terminée)
                                {
                                    JeuMoteurAvecBibliothèque(chaineFen);
                                }
                            }
                            else
                            {   // Si le coup n'est pas valide, on remet la pièce sur sa case d'origine !
                                DessinePiece(_indexSource120, _pieceSource);
                            }
                            _clickCaseSource = true;
                            EffaceDernierCoup();
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
                        PartieEnCours.Black = LabelJoueurNoir.Text = _nomMoteur;     // étiquette et en-tête PGN identiques
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
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheDonneesBrutes));
                return;     // Empêche le code suivant de s'exécuter sur le thread secondaire
            }
            else
            {
                if (MoteurUci.UciVersGui)
                {
                    if (!MoteurUci.DataUci.Contains("currmove"))    // Inutile d'afficher les currmove, il n'y rien d'intéressant ...
                        donneesBrutesUci.DonneesBrutesVue.AppendText(Environment.NewLine + "[" + _nomMoteur + "]    " + MoteurUci.DataUci);
                }
                else
                    donneesBrutesUci.DonneesBrutesVue.AppendText(Environment.NewLine + " [BrunoGUI_GenII]    " + MoteurUci.DataVersUci);
                donneesBrutesUci.DonneesBrutesVue.ScrollToCaret();  // Pour garder l'affichage dans toute la fenêtre
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
                TrackBarTempsReflexion.Enabled = true;
                if (_emetUnSon)
                {   // Son pour dire que le coup est joué
                    SoundPlayer player = new(@"C:\Windows\Media\Windows Notify.wav");
                    player.Play();
                }
                TypeDemande demande = _pilote.ReponseRecue();   // à quelle demande répond ce bestmove ?
                if (demande == TypeDemande.CoupDePartie)
                {   // Coup de la partie : on le joue (promotion comprise)
                    StatusProgramme.Text = InformationPourJoueur.Text = "A vous de jouer";
                    _caseSource = MoteurUci.CoupAuFormatUci[..2];    // CoupAuFormatUci contient le "best move" sous la forme e2e4
                    _caseDestination = MoteurUci.CoupAuFormatUci.Substring(2, 2);
                    PiloteMoteur.JouerCoupUci(MoteurUci.CoupAuFormatUci);      // (rien n'est joué si la position est déjà un mat)
                    if (_dernierCoupMoteurUci != -1)
                    {       // on redessine la case pour effacer le contour du coup précédent du Moteur UCI
                        PictJeux[_dernierCoupMoteurUci].BackColor = CouleurCaseOrigines[_dernierCoupMoteurUci];
                        if (!ParcoursEnCours)       // pendant le parcours, l'échiquier montre une position passée
                            DessinePiece(_dernierCoupMoteurUci, LogiqueMouvements.PiecesEchiquier[_dernierCoupMoteurUci]);
                    }
                    _dernierCoupMoteurUci = LogiqueMouvements.RenvoieCaseIndex120(_caseDestination);

                    if (_dernierCoupColore)
                        CouleursNormalesDernierCoup();  // le moteur a déjà joué juste avant (ex : "Ordinateur joue") : on efface son coup précédent
                    _indexCaseSourceDernierMouvement = RenvoieCaseIndex120(_caseSource);              // convertit la case source en index
                    _indexCaseDestinationDernierMouvement = RenvoieCaseIndex120(_caseDestination);    // convertit la case destination en index
                    _dernierCoupColore = true;
                    ColoreDernierCoup();        // montre les cases source et destination du coup (au retour de parcours s'il y en a un)
                    LeMoteurARépondu();      // On réautorise si le moteur a fini de réfléchir
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

        private void AfficheCoupBlanc(string coupBlanc)
        {   // Affiche le coup joué par les blancs
            // Comme c'est le coup Blanc, il faut afficher le numéro du coup
            if (LogiqueMouvements.EchecetMat == false)
            {   // ******      Traitement du numéro de demi-coup :     ******
                if (_partie.DepuisPosition)
                {   // Si on a chargé une position depuis une FEN,
                    // il faut calculer le numéro de demi-coup en fonction du nombre de coups joués
                    int demiCoupsFen = ((int)NombreCoupsJoues - 1) * 2;
                    if (QuiJoue == ColorPiece.Noir)
                        demiCoupsFen++;
                    NumeroDemiCoup = demiCoupsFen;
                }
                else
                {
                    NumeroDemiCoup = LogiqueMouvements.ListeCoupsFen.Count - 1;
                }
                // ******      Traitement du numéro de demi-coup :     ******
                PartieEnCours.CompteDePLy = (LogiqueMouvements.ListeCoupsFen.Count).ToString();
                _numeroLigne++;
                string raisonNulle = _partie.RejeuPgn ? null : LogiqueMouvements.RaisonNulle();    // répétition, 50 coups ou matériel insuffisant
                if (raisonNulle != null)
                    GestionResultat("1/2-1/2", raisonNulle);
                MetAJourCommandes();    // un coup a été joué : on peut parcourir la partie, la liste des coups, le retour arrière...
            }
            else
            {
                StatusProgramme.Text = "Partie terminée";
            }
        }

        private void AfficheCoupNoir(string coupNoir)
        {   //  Affiche le coup joué par les noirs
            // Comme c'est le coup Noir, on n'a pas besoin d'afficher le numéro du coup
            if (LogiqueMouvements.EchecetMat == false)
            {   // ******      Traitement du numéro de demi-coup :     ******
                if (_partie.DepuisPosition)
                {   // Si on a chargé une position depuis une FEN,
                    // il faut calculer le numéro de demi-coup en fonction du nombre de coups joués
                    int demiCoupsFen = ((int)NombreCoupsJoues - 1) * 2;
                    if (QuiJoue == ColorPiece.Noir)
                        demiCoupsFen++;
                    NumeroDemiCoup = demiCoupsFen;
                }
                else
                {   // Sinon, on peut simplement utiliser la taille de la liste des coups FEN pour déterminer le numéro de demi-coup
                    NumeroDemiCoup = LogiqueMouvements.ListeCoupsFen.Count - 1;
                }
                // ******      Traitement du numéro de demi-coup :     ******
                PartieEnCours.CompteDePLy = (LogiqueMouvements.ListeCoupsFen.Count).ToString();
                string raisonNulle = _partie.RejeuPgn ? null : LogiqueMouvements.RaisonNulle();    // répétition, 50 coups ou matériel insuffisant
                if (raisonNulle != null)
                    GestionResultat("1/2-1/2", raisonNulle);
                MetAJourCommandes();    // un coup a été joué : on peut parcourir la partie, la liste des coups, le retour arrière...
            }
            else
            {
                StatusProgramme.Text = "Partie terminée";
            }
        }

        private void AfficheTour(string Couleur)        // Affiche la couleur du joueur humain courant
        {   // Affiche la couleur du joueur humain courant, et active les PictureBox si c'est au tour du joueur humain
            if (_partie.EntreHumains)
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux " + Couleur + " de jouer";
            else    // active les Picturebox si c'est au tour du joueur humain
                PlateauEnable(_partie.JoueurDe(Couleur == "Blancs" ? ColorPiece.Blanc : ColorPiece.Noir) == Joueur.Humain);
        }

        private void AfficheEchecEtMat(string couleurRoiMat)   // Affiche l'échec et mat du roi de la couleur en paramètre
        {   // Affiche l'échec et mat du roi de la couleur en paramètre, et gère la fin de partie
            int indexCouleur = couleurRoiMat == "Blanc" ? 2 : 1;
            LogiqueMouvements.PartieEnCoursMat = true;      // le "#" du mat est déjà dans les notations du dernier coup (LogiqueMouvements.ExecutionCoup)
            string coupMat = LogiqueMouvements.ListeCoupsPgnFr[^1];
            int indexPoint = coupMat.IndexOf('.');  // On enlève le numéro de coup s'il existe
            if (indexPoint != -1)
            {   // Ce if n'est jamais éxecuté, mais pourrait être utile ?
                coupMat = coupMat[indexPoint..].Replace(".", "");
            }
            if (indexCouleur == 2)
            {   // Couleur du Roi mat = Blanc
                GestionResultat("0-1", " Gain Noir");
            }
            else
            {   // Couleur du Roi mat = Noir
                GestionResultat("1-0", " Gain Blanc");
            }
            InformationPourJoueur.Text = VarianteMoteurCourante.Text = "Le Roi " + couleurRoiMat + " est échec et mat";
            StatusProgramme.Text = "Partie terminée";
            Application.DoEvents();
            PlateauEnable(false);
        }

        private void AfficheInfoEchec(string infoechec)
        {   // Affiche les informations d'échec ou de pat dans l'étiquette, et si c'est un pat, on gère la fin de partie
            InformationsPartie.Text = infoechec;
            InformationsPartie.ForeColor = Color.DarkGreen;
            if (infoechec.Contains("échec") || infoechec.Contains("Pat"))
                InformationsPartie.ForeColor = Color.DarkGreen;
            if (infoechec.Contains("Pat"))
            {
                LogiqueMouvements.PartieEnCoursPat = true;
                GestionResultat("1/2-1/2", "Pat (Nulle)");
                InformationPourJoueur.Text = "Pat (Nulle)";
                PlateauEnable(false); // un des joueurs est pat : fin de la partie
            }
        }
        private void AffichePromotionPion(string Couleur)
        {   // Affiche la promotion d'un pion : on affiche les pièces disponibles pour la promotion,
            // et on attend que le joueur clique sur une pièce pour faire son choix
            _selectionPromotion = LogiqueMouvements.TypePiece.Vide;
            Promo0.Image = Couleur == "Noir" ? ReineNoire : ReineBlanche;
            Promo1.Image = Couleur == "Noir" ? TourNoire : TourBlanche;
            Promo2.Image = Couleur == "Noir" ? FouNoir : FouBlanc;
            Promo3.Image = Couleur == "Noir" ? CavalierNoir : CavalierBlanc;
            GroupPromo.Visible = true;
            while (_selectionPromotion == LogiqueMouvements.TypePiece.Vide && !IsDisposed)
            {   // Attente du clic sur une pièce : la courte pause évite d'occuper le processeur à 100 % pendant l'attente
                Application.DoEvents();
                Thread.Sleep(15);
            }
            if (!IsDisposed)
                GroupPromo.Visible = false;
        }
        private void EffaceDernierCoup()
        {   // Efface les couleurs de la case source et destination du dernier coup joué
            _dernierCoupColore = false;
            CouleursNormalesDernierCoup();
        }
        private void CouleursNormalesDernierCoup()
        {   // Remet la couleur normale des cases du dernier coup (sans oublier ce coup : voir ColoreDernierCoup)
            PictJeux[_indexCaseSourceDernierMouvement].BackColor = Outils.EstCaseClaire(_indexCaseSourceDernierMouvement) ? _couleurCaseClaire : _couleurCaseSombre;
            PictJeux[_indexCaseDestinationDernierMouvement].BackColor = Outils.EstCaseClaire(_indexCaseDestinationDernierMouvement) ? _couleurCaseClaire : _couleurCaseSombre;
        }
        private void ColoreDernierCoup()
        {   // Montre les cases source et destination du dernier coup du moteur, sauf pendant le parcours (position passée affichée)
            if (!_dernierCoupColore || ParcoursEnCours)
                return;
            PictJeux[_indexCaseSourceDernierMouvement].BackColor = _couleurCaseSource;
            PictJeux[_indexCaseDestinationDernierMouvement].BackColor = _couleurCaseDestination;
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
            PlateauEnable(false);
            MetAJourCommandes();
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des menus
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void HumainOrdinateur_Click(object sender, EventArgs e)
        {   // L'humain joue les blancs, l'ordinateur les noirs
            AbandonneReflexion();   // nouvelle partie
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            QuiJoue = ColorPiece.Blanc;
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomHumain;
            PartieEnCours.Black = LabelJoueurNoir.Text = _nomMoteur;
            PartieEnCours.WhiteElo = EloBlanc.Text = _joueurElo;
            PartieEnCours.BlackElo = EloNoir.Text = _moteurElo;
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous aurez les Blancs contre " + _nomMoteur + ". " + "\nToute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Le joueur a les Blancs, l'ordinateur les Noirs ", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                StatusProgramme.Text = ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "";    // On efface les données de la partie précédente
                CommencerPartie(Joueur.Humain, Joueur.Moteur);
                if (_visuCoteNoir)
                    TourneEchiquier();
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
                ParametresJoueurHumain("Blancs", "A vous de jouer");            // On demande à l'humain de jouer
                PlateauEnable(true);                                            // On lui permet de bouger les pièces
            }
        }
        private void OrdinateurHumain_Click(object sender, EventArgs e)
        {   // L'ordinateur joue les blancs, l'humain les noirs
            AbandonneReflexion();   // nouvelle partie
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            QuiJoue = ColorPiece.Blanc;
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomMoteur;
            PartieEnCours.Black = LabelJoueurNoir.Text = _nomHumain;
            PartieEnCours.WhiteElo = EloBlanc.Text = _moteurElo;
            PartieEnCours.BlackElo = EloNoir.Text = _joueurElo;
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous aurez les Noirs contre " + _nomMoteur + ". " + "\nToute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Le joueur a les Noirs, l'ordinateur les Blancs ", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                StatusProgramme.Text = ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "";     // On efface les données de la partie précédente
                CommencerPartie(Joueur.Moteur, Joueur.Humain);
                if (_visuCoteNoir == false)
                    TourneEchiquier();                                          // On met la vue côté Noir
                ParametresJoueurHumain("Noirs", "Le moteur UCI joue");
                NumeroDemiCoup = 0;
                JeuMoteurAvecBibliothèque(FenDepart);
            }
        }
        private void HumainContreHumain_Click(object sender, EventArgs e)
        {   // 2 joueurs humains s'affrontent, pas de moteur UCI
            AbandonneReflexion();   // nouvelle partie
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomHumain;
            PartieEnCours.Black = LabelJoueurNoir.Text = "Adversaire";
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous jouez contre votre ami/partenaire,\n" + "ou vous saisissez une partie ...\n" +
                "Toute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Jeu entre amis, ou saisie de partie", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
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
                _bibliotheque = OuvertureChoixBibliothèque.FileName;
                RécupèreBibliothèque();
            }
        }
        private void RodentIV_Click(object sender, EventArgs e)
        {   //  https://echecs-et-informatique.franceserv.com/rodent-iv.html
            EloNoir.Text = _moteurElo = "+- 3000";
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\Rodent_IV", "rodent-iv-x64.exe");
            Debug.WriteLine("Chemin Rodent IV = " + _cheminMoteur);
            DémarrageMoteur();
        }
        private void Sargon1_1978_Click(object sender, EventArgs e)
        {   // https://echecs-et-informatique.franceserv.com/sargon-1978.html
            EloNoir.Text = _moteurElo = "1678";
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\sargon1978", "sargon1978_1_01b.exe");
            Debug.WriteLine("Chemin sargon I 1978 = " + _cheminMoteur);
            DémarrageMoteur();
            MoteurUci.SpecialeSargon();         // Sinon Sargon  mouline sans fin !!!!!
        }
        public void DémarreStockfish()
        {   //  https://stockfishchess.org/
            EloNoir.Text = _moteurElo = "+- 3000";
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
            if (_partie.Blancs == Joueur.Moteur)
                LabelJoueurBlanc.Text = _nomMoteurChoisi;
            if (_partie.Noirs == Joueur.Moteur)
                LabelJoueurNoir.Text = _nomMoteurChoisi;
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
        {   // Permet de choisir la couleur des cases sombres de l'échiquier
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
            {   // On récupère la couleur choisie par l'utilisateur
                _couleurCaseSombre = CouleurDialogue.Color;
                Color Couleur;
                int index = 0;
                CouleurCaseOrigines.Clear();                        // On nettoie la liste des couleurs d'origine
                // les 120 cases du jeu (seules 64 cases sont visibles : voir la classe ClassEchiquier pour les détails )
                for (int Ligne = 0; Ligne <= 11; Ligne++)           // On parcourt les lignes de l"échiquier
                {
                    Couleur = Ligne % 2 == 0 ? _couleurCaseClaire : _couleurCaseSombre; // Couleur des cases de l'échiquier
                    for (int Colonne = 0; Colonne <= 9; Colonne++)              // On parcourt les colonnes de l'échiquier
                    {
                        PictJeux[index].BackColor = Couleur;
                        CouleurCaseOrigines.Add(Couleur);
                        index++;
                        Couleur = Couleur == _couleurCaseClaire ? _couleurCaseSombre : _couleurCaseClaire;
                    }
                }
            }
        }
        private void CaseClaire_Click(object sender, EventArgs e)
        {   // Permet à l'utilisateur de choisir la couleur des cases claires de l'échiquier
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
            {   // On récupère la couleur choisie par l'utilisateur
                _couleurCaseClaire = CouleurDialogue.Color;
                Color Couleur;
                int index = 0;
                CouleurCaseOrigines.Clear();                        // On nettoie la liste des couleurs d'origine
                // les 120 cases du jeu (seules 64 cases sont visibles : voir la classe ClassEchiquier pour les détails )
                for (int ligne = 0; ligne <= 11; ligne++)           // On parcourt les lignes de l"échiquier
                {
                    Couleur = ligne % 2 == 0 ? _couleurCaseClaire : _couleurCaseSombre; // Couleur des cases de l'échiquier
                    for (int Colonne = 0; Colonne <= 9; Colonne++)              // On parcourt les colonnes de l'échiquier
                    {
                        PictJeux[index].BackColor = Couleur;
                        CouleurCaseOrigines.Add(Couleur);
                        index++;
                        Couleur = Couleur == _couleurCaseClaire ? _couleurCaseSombre : _couleurCaseClaire;
                    }
                }
            }
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
            LancerReflexion();  // Décompte le temps de réflexion
        }
        private void InverseEchiquier_Click(object sender, EventArgs e)
        {   // Permet d'inverser la vue de l'échiquier (côté Blanc ou côté Noir) : ne change pas le droit de jouer
            TourneEchiquier();
        }
        private void OrdinateurJoue_Click(object sender, EventArgs e)
        {   // Permet de faire jouer l'ordinateur UCI, sans que ce soit son tour (pour tester une position par exemple)
            AbandonneReflexion();   // une nouvelle demande remplace la réflexion en cours
            _partie.MoteurPrendLeTrait();   // le moteur joue désormais le camp au trait, l'humain l'autre
            string Fenaenvoyer;
            if (ListeCoupsFen.Count > 0)
                Fenaenvoyer = ListeCoupsFen[^1];   // dernier FEN
            else
            {
                NumeroDemiCoup = 0;
                Fenaenvoyer = FenDepart; // position initiale
            }
            JeuMoteurAvecBibliothèque(Fenaenvoyer);
            PlateauEnable(true);
            _clickCaseSource = _visuSymbole = true;    // L'ordinateur ayant joué, c'est indispensable !
        }
        private void RetourArriere_Click(object sender, EventArgs e)
        {   // Permet de revenir en arrière d'un demi-coup (coup des blancs ou des noirs)
            if (ParcoursEnCours)
                return;             // sécurité : le bouton est grisé pendant le parcours (voir MetAJourCommandes)
            AbandonneReflexion();   // retour arrière : le moteur ne doit pas jouer sur la position annulée
            EffaceDernierCoup();
            bool etaitTerminee = _partie.Mode == ModePartie.Terminee;
            if (!_partie.AnnulerDernierCoup())      // retire le dernier 1/2 coup et rétablit la position (jamais avant la position de départ)
                _ = KryptonMessageBox.Show("Pas assez de coups joués \nPas de retour arrière possible", "Retour impossible", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            else
            {
                if (etaitTerminee)
                    EffaceResultat();   // on a annulé un coup d'une partie terminée : elle reprend
                NumeroDemiCoup = LogiqueMouvements.ListeCoupsFen.Count == 0 ? 0 : NumeroDemiCoup - 1;
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(LogiqueMouvements.RetourneChaineFenActuel()));
                InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + QuiJoue + "s";
                InformationsPartie.Text = _partie.Blancs == Joueur.Moteur ? "L'ordinateur joue les Blancs" :
                          _partie.Noirs == Joueur.Moteur ? "L'ordinateur joue les Noirs" :
                          "L'ordinateur ne joue pas cette partie";
                // Au joueur de jouer, sauf si c'est au tour du moteur (il faut alors un 2e retour arrière, ou "Ordinateur joue")
                PlateauEnable(!_partie.MoteurAuTrait);
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
            string numeroCoup = "";
            string blancs = "";
            string noirs;
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
            // Les coups joués en PGN français (sans l'éventuelle position de départ d'une partie chargée depuis un FEN)
            List<string> coupsPgnFr = LogiqueMouvements.ListeCoups.Where(c => !c.EstPositionDeDepart).Select(c => c.PgnFr).ToList();
            // Nombre de coups à traiter (sans compter le résultat s'il est à la fin)
            int nombreCoups = coupsPgnFr.Count;
            if (nombreCoups != 0)
            {
                // Vérifier si la dernière ligne est un résultat (1-0, 0-1, 1/2-1/2)
                string dernierElement = coupsPgnFr[^1].Trim();
                bool dernierElementEstResultat = (dernierElement == "1-0" || dernierElement == "0-1" || dernierElement == "1/2-1/2");
                // Si le dernier élément est un résultat, on ne le traite pas comme un coup
                if (dernierElementEstResultat)
                    nombreCoups--;  // Exclure le résultat du traitement des coups

                // Traitement des coups
                for (int i = 0; i < nombreCoups; i++)
                {
                    string coup = coupsPgnFr[i].Trim();

                    if (i % 2 == 0) // Lignes paires : coups des Blancs avec numéro
                    {
                        string[] coupBlancs = coup.Split([' '], 2);
                        numeroCoup = coupBlancs[0]; // Numéro du coup
                        blancs = coupBlancs[1].Trim(); // Coup des Blancs
                    }
                    else // Lignes impaires : coups des Noirs
                    {
                        noirs = coup; // Coup des Noirs
                        mafenetrePartie.FeuillePartie.Rows.Add(numeroCoup, blancs, noirs);
                    }
                }
                // Si la liste contient un nombre impair de coups (dernier coup blanc sans noir)
                if (nombreCoups % 2 != 0)
                {
                    mafenetrePartie.FeuillePartie.Rows.Add(numeroCoup, blancs, "");
                }
                // Ajouter le résultat de la partie s'il existe
                if (dernierElementEstResultat)
                {
                    // Ajouter une ligne avec le résultat dans la colonne des Noirs
                    mafenetrePartie.FeuillePartie.Rows.Add("", "", dernierElement);
                }
                if (mafenetrePartie.FeuillePartie.Rows.Count > 0)
                {   // Sélectionner la cellule du premier coup blanc (colonne 1, première ligne)
                    mafenetrePartie.FeuillePartie.CurrentCell = mafenetrePartie.FeuillePartie.Rows[0].Cells[1];
                    mafenetrePartie.FeuillePartie.Focus(); // Met le focus sur la DataGridView
                }
            }
        }
        public void MontrePartiesPGN_Click(object sender, EventArgs e)
        {   // Affiche ou masque la liste des parties
            _montreListeParties = !_montreListeParties;
            MontrePartiesPGN.Text = _montreListeParties ? "Masque liste parties" : "Affiche liste parties";
            if (_montreListeParties) fichierPartiePgn.Show();   // On affiche la liste des parties
            else fichierPartiePgn.Hide();                       // ou on masque la liste des parties
        }
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
            LabelJoueurBlanc.Text = PartieEnCours.White;        // On affiche les noms et ELO des joueurs qui sont dans l'entête PGN
            LabelJoueurNoir.Text = PartieEnCours.Black;
            EloBlanc.Text = PartieEnCours.WhiteElo;
            EloNoir.Text = PartieEnCours.BlackElo;
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
            _ = KryptonMessageBox.Show("      BrunoGUI GenII\n       Version 1.06\n--  Bruno COURTOIS  -- " +
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
                await maj.Installer(version);       // arrête le Stockfish en cours
                bool stockfishEnCours = string.Equals(Path.GetFullPath(_cheminMoteur), CheminStockfish, StringComparison.OrdinalIgnoreCase);
                if (stockfishEnCours)
                {   // Le moteur arrêté par l'installation est redémarré avec la nouvelle version
                    MoteurUci.Quitte();
                    MoteurUci.Start(CheminStockfish);
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
            parametres.CaseSombre = Parametres.FormatCouleur(_couleurCaseSombre);
            parametres.CaseClaire = Parametres.FormatCouleur(_couleurCaseClaire);
            parametres.CouleurCaseSource = Parametres.FormatCouleur(_couleurCaseSource);
            parametres.CouleurCaseDestination = Parametres.FormatCouleur(_couleurCaseDestination);
            parametres.NomHumain = _nomHumain;
            parametres.DureeReflexionSeconde = TrackBarTempsReflexion.Value;
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
        {   // "Reprendre la partie d'ici" : la partie est coupée à la position affichée (parcours) et reprend depuis cette position
            if (!ParcoursEnCours)
                return;     // sécurité : le bouton n'est actif que pendant le parcours (voir MetAJourCommandes)
            int index = _indexAffiche;
            int aSupprimer = LogiqueMouvements.ListeCoups.Count - 1 - index;
            bool etaitLectureSeule = PartieEnLectureSeule;
            string message = etaitLectureSeule
                ? $"La partie chargée devient votre partie à partir de la position affichée :\nvous jouez le camp au trait, {_nomMoteur} l'autre camp.\n\nLes {aSupprimer} demi-coup(s) suivant(s) seront supprimés. Continuer ?"
                : $"La partie reprend à la position affichée.\n\nLes {aSupprimer} demi-coup(s) suivant(s) seront supprimés. Continuer ?";
            if (KryptonMessageBox.Show(message, "Reprendre la partie d'ici", KryptonMessageBoxButtons.OKCancel, KryptonMessageBoxIcon.Question) != DialogResult.OK)
                return;
            AbandonneReflexion();   // la partie change
            bool etaitTerminee = _partie.Mode == ModePartie.Terminee;
            if (mafenetrePartie != null && !mafenetrePartie.IsDisposed)
                mafenetrePartie.Close();    // sa liste de coups ne correspond plus à la partie
            QuitteParcours();
            EffaceDernierCoup();
            if (_partie.ReprendreDepuis(index) == 0)
            {   // rien à supprimer : l'échiquier revient simplement à la partie
                LogiqueMouvements.DessinPieces();
                MetAJourCommandes();
                return;
            }
            _dernierCoupMoteurUci = -1;
            if (etaitTerminee || etaitLectureSeule)
                EffaceResultat();       // la partie n'a plus de résultat
            if (etaitLectureSeule)
            {   // La partie chargée devient une partie d'entraînement contre le moteur (l'humain a le camp au trait)
                bool humainBlancs = _partie.Blancs == Joueur.Humain;
                PartieEnCours.White = LabelJoueurBlanc.Text = humainBlancs ? _nomHumain : _nomMoteur;
                PartieEnCours.Black = LabelJoueurNoir.Text = humainBlancs ? _nomMoteur : _nomHumain;
                PartieEnCours.WhiteElo = EloBlanc.Text = humainBlancs ? _joueurElo : _moteurElo;
                PartieEnCours.BlackElo = EloNoir.Text = humainBlancs ? _moteurElo : _joueurElo;
                PartieEnCours.Tournoi = "Entrainement";
                PartieEnCours.Lieu = "Maison";
                PartieEnCours.Date = DateTime.Today.ToString("yyyy.MM.dd");
                PartieEnCours.Ronde = "";
            }
            PartieEnCours.CompteDePLy = LogiqueMouvements.ListeCoupsFen.Count.ToString();
            NumeroDemiCoup = _partie.DepuisPosition ? (int)Math.Round((NombreCoupsJoues - 1) * 2) : Math.Max(0, LogiqueMouvements.ListeCoupsFen.Count - 1);
            string fen = LogiqueMouvements.RetourneChaineFenActuel();
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen));
            InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + (QuiJoue == ColorPiece.Blanc ? "Blancs" : "Noirs");
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
            AbandonneReflexion();   // chargement d'une partie
            EffaceDernierCoup();
            ListeParties.Clear();    // On vide la liste des parties 
            ListePartiesPGN.Clear(); // On vide la liste des parties PGN
            if (ChargerPartiesPgn.ShowDialog() == DialogResult.OK)
            {
                string cheminFichier = ChargerPartiesPgn.FileName;
                try
                {   // Vérifie et obtient le chemin complet
                    string fullPath = Path.GetFullPath(cheminFichier);
                    Debug.WriteLine("Chemin complet du fichier : " + fullPath);

                    // Lire le contenu du fichier et l'afficher dans la console
                    string contenuFichier = File.ReadAllText(fullPath);
                    // NettoyageRapide();
                    if (fichierPartiePgn == null || fichierPartiePgn.IsDisposed)
                    {   // Traitement pour prendre en compte la fermeture par croix rouge en haut à droite ...
                        fichierPartiePgn = new FichierPartiePgn();
                        fichierPartiePgn.FormClosed += (s, args) =>
                        {   // On évite que la référence de affichePgn pointe vers un objet supprimé.
                            fichierPartiePgn = null;
                        };
                    }
                    ListeParties = FichierPartiePgn.DecodeFichierPGN(fullPath); // Récupère les parties PGN
                    foreach (string partie in ListeParties)                     // On parcourt la liste de parties, et
                    {                                                           // On met chaque partie au format PartieEchecsPGN dans ListePartiePGN
                        ListePartiesPGN.Add(FichierPartiePgn.DecodePartiePGN(partie));
                    }
                    fichierPartiePgn.NombrePartiesFichier.Text = ListePartiesPGN.Count.ToString()
                        + " partie(s) dans le fichier  " + cheminFichier[(cheminFichier.LastIndexOf('\\') + 1)..];
                    fichierPartiePgn.AfficherListeParties(ListePartiesPGN);
                    fichierPartiePgn.Show();
                    MontrePartiesPGN.Enabled = true; // Active le bouton pour masquer/afficher la liste
                    MontrePartiesPGN_Click(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Chargement Pgn : Erreur lors de la lecture du fichier : " + ex.Message);
                }
            }
            VarianteMoteurUci1.Text = VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = InformationPourJoueur.Text = "...";
            VarianteMoteurCourante.Text = ScoreMoteur.Text = EvaluationUci.Text = "...";
        }
        private void ChargePositionFen_Click(object sender, EventArgs e)
        {
            if (ChargerPositionFen.ShowDialog() != DialogResult.OK)
                return;     // annulé : la partie en cours ne change pas
            string contenuFen;
            try
            {
                contenuFen = File.ReadAllText(Path.GetFullPath(ChargerPositionFen.FileName)).Trim();
                Debug.WriteLine("Contenu du fichier FEN : " + contenuFen);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Chargement FEN : Erreur lors de la lecture du fichier : " + ex.Message);
                return;
            }
            if (contenuFen.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 6)
            {
                KryptonMessageBox.Show("Ce fichier ne contient pas une position FEN complète (6 champs).", "Chargement FEN",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }
            AbandonneReflexion();   // chargement d'une position
            QuitteParcours();       // nouvelle partie : l'échiquier suit la partie
            ListeParties.Clear();    // On vide la liste des parties
            ListePartiesPGN.Clear(); // On vide la liste des parties PGN
            ViderCoups();              // On vide la liste des coups (toutes les notations)
            InitialisationEchiquier();    // On réinitialise l'échiquier
            VarianteMoteurUci1.Text = "Fen chargé : " + contenuFen;
            // Trait, droits de roque, case en passant, compteur des 50 coups et numéro du coup : tout vient de la FEN
            LogiqueMouvements.MiseenplaceFen(contenuFen);
            AjoutePositionDeDepart(contenuFen);  // La partie commence à cette position (élément sans coup, en tête de liste) :
                                                 // le retour arrière ne remonte jamais avant
            _partie.CommencerDepuisPosition();  // l'humain joue le camp au trait, le moteur lui répond
            EffaceDernierCoup();                // les cases du dernier coup de la partie précédente
            _pilote.Abandonner();       // plus aucune demande (analyse ou coup) en cours au moteur
            _dernierCoupMoteurUci = -1;
            _clickCaseSource = _visuSymbole = true;
            PartieEnCours.CoupsPartiePGN = PartieEnCours.Result = PartieEnCours.CompteDePLy = PartieEnCours.Ronde = "";
            PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Lieu = "Maison";
            PartieEnCours.White = LabelJoueurBlanc.Text = "";
            PartieEnCours.Black = LabelJoueurNoir.Text = "";
            PartieEnCours.WhiteElo = EloBlanc.Text = "";
            PartieEnCours.BlackElo = EloNoir.Text = "";
            InformationPourJoueur.Text = "Trait aux " + (QuiJoue == ColorPiece.Blanc ? "Blancs" : "Noirs");
            // Numéro de demi-coup (à partir de 0) : NombreCoupsJoues vaut n (Blancs au trait) ou n + 0,5 (Noirs au trait) pour la FEN "... n"
            NumeroDemiCoup = (int)Math.Round((NombreCoupsJoues - 1) * 2);
            PromotionPiece = TypePiece.Vide;
            PlateauEnable(true);   // On active le plateau pour pouvoir jouer à partir de la position chargée
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(contenuFen));
            MetAJourCommandes();
        }
        private void EnregistrerPgn_Click(object sender, EventArgs e)
        {   // Enregistre la partie au format PGN
            try
            {
                string contenuPgn = GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Intl"); // On récupère la partie au format PGN
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
                            {   // Affiche la nouvelle partie
                                KryptonMessageBox.Show($"Fichier PGN :\n {contenuPgn}", "Affichage fichier PGN", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
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
            EffaceDernierCoup();    // les cases du dernier coup de la partie précédente
            Outils.MiseaZeroListes();
            Debug.WriteLine("ChargerPartieDepuisPgn / :  " + partie.White + " vs " + partie.Black + "   Résultat : " + partie.Result);
            PartieEnCours.Tournoi = partie.Tournoi;
            PartieEnCours.Lieu = partie.Lieu;
            PartieEnCours.Date = partie.Date;
            PartieEnCours.Ronde = partie.Ronde;
            PartieEnCours.White = LabelJoueurBlanc.Text = _joueurBlanc = partie.White;
            PartieEnCours.WhiteElo = EloBlanc.Text = partie.WhiteElo;
            PartieEnCours.Black = LabelJoueurNoir.Text = _joueurNoir = partie.Black;
            PartieEnCours.BlackElo = EloNoir.Text = partie.BlackElo;
            PartieEnCours.Result = InformationsPartie.Text = partie.Result;
            PartieEnCours.ECO = partie.ECO;
            PartieEnCours.CompteDePLy = partie.CompteDePLy;
            PartieEnCours.CoupsPartiePGN = partie.CoupsPartiePGN;
            InformationPourJoueur.Text = partie.Tournoi + " / ronde " + partie.Ronde;
            StatusProgramme.Text = $"{partie.White} vs {partie.Black}";
            ScoreMoteur.Text = InformationsPartie.Text = "Résultat : " + partie.Result;
            // Certains fichiers PGN n'ont pas d'espace entre le numéro et le coup, il faut l'ajouter :
            PartieEnCours.CoupsPartiePGN = PartieEnCours.CoupsPartiePGN.Replace(".", ". ");
            // On decoupe la liste de coups recue :
            string[] coupsPartie = PartieEnCours.CoupsPartiePGN.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            Debug.WriteLine($"Partie en PGN : {PartieEnCours.CoupsPartiePGN}");
            if (coupsPartie[0] != "1.")     // Tester si CoupsPartie[0] = "1." pour vérifier que c'est bien le début d'une partie ?
                _ = KryptonMessageBox.Show("Problème avec la partie \n Elle ne débute pas avec 1. ", "Problème de partie",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
            ParcoursPartie(coupsPartie);
        }
        private void ParcoursPartie(string[] suiteCoups)
        {   // Parcourt la partie coup par coup pour l'afficher sur l'échiquier et afficher le résultat à la fin
            bool _couleurTraitBlanc = true;        // Pour commencer avec les Blancs
            _partie.Commencer(Joueur.Humain, Joueur.Humain);    // rejeu des coups de la partie ; lecture seule à la fin
            VarianteMoteurCourante.Text = "";
            PartieEnCoursMat = PartieEnCoursPat = false;     // On réinitialise les indicateurs de fin de partie
            _partie.RejeuPgn = true;    // pas de nulle automatique pendant le rejeu : c'est le résultat du PGN qui compte
            try
            {
                for (int indicecoup = 0; indicecoup < suiteCoups.Length - 1; indicecoup++)  // Parcourir tous les coups de la partie
                {
                    GestionPartiePgn.DecodeCoupPartie(suiteCoups[indicecoup], _couleurTraitBlanc);
                    if (!suiteCoups[indicecoup].Contains('.'))
                    {
                        _couleurTraitBlanc = !_couleurTraitBlanc;
                    }
                }
            }
            finally
            {
                _partie.RejeuPgn = false;
            }
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
                    Console.WriteLine($"Pas de résultat défini : {PartieEnCours.Result}");
                    break;
            }
            Thread.Sleep(200);  // pause 0,2 seconde
            // La partie reste sur sa position finale ; elle est en lecture seule (parcours et analyse), et on l'affiche depuis le début
            _partie.PasserEnLectureSeule();
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
            if (_pilote.DemanderCoup(chaineFen, _dureeReflexionMilliSeconde) == ResultatDemandeCoup.CoupBibliotheque)
            {   // Coup trouvé dans la bibliothèque : il est déjà joué
                string coupChoisiTxt = _pilote.DernierCoupBibliotheque;
                VarianteMoteurUci1.Text = "Coup bibliothèque " + Path.GetFileName(_bibliotheque) + " exécuté par le moteur -> " + coupChoisiTxt;
                VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = ".....";
                Debug.WriteLine($"Coup bibliothèque exécuté : {coupChoisiTxt}");
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(LogiqueMouvements.RetourneChaineFenActuel()));
                return;
            }
            // Aucun coup dans la bibliothèque ou bibliothèque inactive : le moteur réfléchit
            LancerReflexion();  // Décompte le temps de réflexion
            if (!LogiqueMouvements.EchecetMat)
            {
                InformationPourJoueur.Text = StatusProgramme.Text = _nomMoteur + " réfléchit ...";
            }
            if (LogiqueMouvements.RenvoieCaseIndex120(_caseDestination) == _dernierCoupMoteurUci)
                _dernierCoupMoteurUci = -1;
        }

        private string AfficherCoupsBibliotheque(ulong clePosition)
        {   // Affiche les coups disponibles dans la bibliothèque pour une position donnée
            var entrees = PolyglotBibliothèque.TrouverLesEntrées(clePosition).ToList();
            CoupsBibliothèqueBox.Clear();
            // --- Titre ---
            CoupsBibliothèqueBox.SelectionAlignment = HorizontalAlignment.Center;
            CoupsBibliothèqueBox.SelectionColor = Color.Black;
            CoupsBibliothèqueBox.SelectionFont = new Font(CoupsBibliothèqueBox.Font, FontStyle.Bold);
            CoupsBibliothèqueBox.AppendText("Bibliothèque\n--------------\n");
            CoupsBibliothèqueBox.SelectionAlignment = HorizontalAlignment.Left;
            if (entrees.Count == 0)
            {
                CoupsBibliothèqueBox.SelectionFont = new Font(CoupsBibliothèqueBox.Font, FontStyle.Regular);
                CoupsBibliothèqueBox.AppendText("Aucun coup trouvé dans la bibliothèque.\n");
                return string.Empty;
            }
            // --- Choix du coup ---
            EntréePolyglot entreeChoisie;
            var rnd = new Random();

            if (_bibliothèqueAléatoire)
            {   // Choix aléatoire parmi toutes les entrées
                entreeChoisie = entrees[rnd.Next(entrees.Count)];
            }
            else
            {   // Choix parmi les meilleurs poids
                var maxPoids = entrees.Max(e => e.Poids);
                var meilleures = entrees.Where(e => e.Poids == maxPoids).ToList();
                entreeChoisie = meilleures[rnd.Next(meilleures.Count)];
            }

            string coupChoisiTxt = PolyglotBibliothèque.DecodeCoup(entreeChoisie.CoupBiblio);            // Tri des coups par poids décroissant
            entrees = [.. entrees.OrderByDescending(e => e.Poids)];
            // --- Affichage dans la liste ---
            foreach (var e in entrees)
            {
                string coupTxt = PolyglotBibliothèque.DecodeCoup(e.CoupBiblio);
                bool estChoisi = (e == entreeChoisie);
                CoupsBibliothèqueBox.SelectionFont = new Font(CoupsBibliothèqueBox.Font, estChoisi ? FontStyle.Bold : FontStyle.Regular);
                CoupsBibliothèqueBox.SelectionColor = estChoisi ? Color.Green : Color.Black; string prefixe = estChoisi ? "⭐ " : " *  ";
                CoupsBibliothèqueBox.AppendText($"{prefixe}{coupTxt} ({e.Poids})\n");
            }
            return coupChoisiTxt;
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Routines de Dessin
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void DessineEchiquier()
        {   // Dessine les cases de l'échiquier (120 cases au total, mais seules 64 sont visibles)
            Color Couleur;
            int Index = 0;
            // les 120 cases du jeu (seules 64 cases sont visibles : voir la classe LogiqueMouvements pour les détails )
            for (int Ligne = 0; Ligne <= 11; Ligne++)
            {
                Couleur = Ligne % 2 == 0 ? _couleurCaseClaire : _couleurCaseSombre;     // Couleur des cases de l'échiquier
                for (int Colonne = 0; Colonne <= 9; Colonne++)
                {
                    PictureBox Pict = new()
                    {   //  Les cases sont des PictureBox indéxées, par exemple : case a1 = PictJeux21, case h8 = PictJeux98
                        Name = "Pictjeux" + Index.ToString(),
                        BackColor = Couleur,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Size = new Size(60, 60),    // Taille des case = 60 * 60 pixels
                        Location = new Point(-39 + (Colonne * 60), 560 - (Ligne * 60)), // -39 semble OK mais à checker ?
                        Visible = Ligne > 1 & Ligne < 10 & Colonne > 0 & Colonne < 9, // On ne rend visible que les 64 cases utiles
                        Enabled = false
                    };
                    CouleurCaseOrigines.Add(Couleur);
                    Pict.BringToFront();
                    PictJeux.Add(Pict);
                    Pict.MouseDown += CaseMouseDown;
                    Plateau.Controls.Add(Pict);
                    Index++;
                    Couleur = Couleur == _couleurCaseClaire ? _couleurCaseSombre : _couleurCaseClaire;
                }
            }
        }
        private void DessinePiece(int IndexCase, LogiqueMouvements.TypePiece Piece)
        {   // Dessine une pièce sur l'échiquier avec l'indexSource120 et le type de la Piece
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(() => DessinePiece(IndexCase, Piece)));
                    return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
                }
                else
                {
                    PictJeux[IndexCase].Image = ListeBitmapsPiece[Piece];
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erreur dans DessinePiece : " + ex.Message);
                Debug.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }
        private void DessineSymbole(int IndexCase, LogiqueMouvements.TypeSymbole Symbole)
        {   // Dessine un des 4 symboles sur l'échiquier si ceux sont visibles
            if (_visuSymbole == true)
            {
                if (PictJeux[IndexCase].Image == null)                          // Si la case est vide,
                    PictJeux[IndexCase].Image = ListeBitmapsSymbole[Symbole];   // On dessine le symbole passé en paramètre
                else
                {                                                               // Si la case n'est pas vide
                    Bitmap CaseJeu = new(PictJeux[IndexCase].Image);
                    Graphics g = Graphics.FromImage(CaseJeu);
                    g.DrawImage(ListeBitmapsSymbole[Symbole], 0, 0, 100, 100);
                    PictJeux[IndexCase].Image = CaseJeu;
                }
            }
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        //  Diverses méthodes
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void TrackBarTempsReflexion_ValueChanged(object sender, EventArgs e)
        {   // Méthode appelée lorsque la valeur du TrackBar de temps de réflexion change
            _dureeReflexionMilliSeconde = TrackBarTempsReflexion.Value * 1000;
            InformationsPartie.Text = "Temps de réflexion = " + (_dureeReflexionMilliSeconde / 1000).ToString() + " secondes";
            labelTempsReflexion.Text = "(" + TrackBarTempsReflexion.Value + ")";
            Debug.WriteLine($" 3. _dureeReflexionMilliSeconde = {_dureeReflexionMilliSeconde}");
        }
        private void ActiveBibliothèque_CheckedChanged(object sender, EventArgs e)
        {   // Bascule pour activer ou non la bibliothèque
            _bibliothèqueActive = !_bibliothèqueActive;
        }
        private void ActiveAléatoire_CheckedChanged(object sender, EventArgs e)
        {   // Choisir un coup aléatoire ou le meilleur coup dans la bibliothèque
            _bibliothèqueAléatoire = !_bibliothèqueAléatoire;
        }
        private void ActiveSon_CheckedChanged(object sender, EventArgs e)
        {   // Bascule pour mettre ou enlever le son
            _emetUnSon = !_emetUnSon;
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
            LogiqueMouvements.PromotionPiece = _selectionPromotion;
            Debug.WriteLine($"[Promo0_Click] Promotion choisie : {_selectionPromotion} (PromotionPiece =  {LogiqueMouvements.PromotionPiece})");
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
            if (PictJeux.Count < 120)
                return;     // cases pas encore créées
            bool actif = ParcoursEnCours
                ? !PartieEnLectureSeule
                : _plateauAutorise && (_partie.Mode == ModePartie.EnCours || _partie.Mode == ModePartie.AucunePartie);
            for (int i = 0; i <= 119; i++)
                if (PictJeux[i].Visible)
                    PictJeux[i].Enabled = actif;
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
            // Reprendre ici : seulement pendant le parcours (la position affichée est une position passée)
            BoutonReprendreIci.Enabled = ParcoursEnCours && _partie.Mode != ModePartie.AucunePartie;
            _clavierActif = coupsJoues;     // flèches du clavier (Echap les coupe jusqu'au prochain calcul)
            AnalysePosition.Enabled = _partie.Mode != ModePartie.AucunePartie;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = enCours;
            OrdinateurJoue.Enabled = enCours;
            BoutonBalises.Enabled = SaisiePartieBouton.Enabled = !PartieEnLectureSeule;
            MetAJourPlateau();
        }
        private void TourneEchiquier()
        {   // Tourne l'échiquier de 180° pour changer le côté de visualisation
            EffaceDernierCoup();
            PictJeux.Reverse();             // On inverse les liste des PictureBox ce qui revient à faire une rotation à 180°
            Plateau.Image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Plateau.Refresh();              // On inverse le plateau
            if (ParcoursEnCours)
                DessineEchiquierDe(_positionAffichee);  // pendant le parcours : la position affichée
            else
                LogiqueMouvements.DessinPieces();       // On dessine les pièces de la partie
            _visuCoteNoir = !_visuCoteNoir;   // On inverse le flag de côté de visualisation
        }
        private void ParametresJoueurHumain(string Couleur, string Affichage)
        {   // Message au joueur humain en début de partie (sa couleur est fixée par CommencerPartie)
            InformationPourJoueur.Visible = true;
            InformationPourJoueur.Text = StatusProgramme.Text = Affichage;
        }
        private void RécupèreBibliothèque()
        {   // Récupère les informations de la bibliothèque
            var polyglot = new PolyglotBibliothèque();
            polyglot.MessageLog += msg => CoupsBibliothèque.Text = msg;
            polyglot.PolyglotBibliothèqueLecture(_bibliotheque);
            CoupsBibliothèque.SelectAll();
            CoupsBibliothèque.SelectionAlignment = HorizontalAlignment.Center;
            CoupsBibliothèque.DeselectAll();
            // Recherche de la position dans la bibliothèque à partir du FEN
            ulong clePosition = PolyglotBibliothèque.CalculeClefPolyglot(LogiqueMouvements.RetourneChaineFenActuel());
            string coupChoisiTxt = AfficherCoupsBibliotheque(clePosition);
        }
        private void MiseaZeroAffichages()
        {   // Réinitialise les affichages de la partie et du moteur
            Outils.MiseaZeroListes();
            _numeroLigne = 0;
            NumeroDemiCoup = 0;
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
        private void NePasDérangerMoteur()
        {   // Pendant la réflexion du moteur, tout reste possible : les actions qui rendent la réflexion inutile l'abandonnent
            // (AbandonneReflexion, la réponse du moteur est alors ignorée), et parcourir la partie ne modifie que l'affichage
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
            if (_dernierCoupColore)
                CouleursNormalesDernierCoup();  // le dernier coup de la partie n'a pas de sens sur une position passée
            _positionAffichee = LogiqueMouvements.PositionDepuisFen(fen);
            _indexAffiche = index;
            MetAJourCommandes();
            DessineEchiquierDe(_positionAffichee);
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen));
            InformationPourJoueur.Text = "Trait aux " + (_positionAffichee.QuiJoue == ColorPiece.Blanc ? "Blancs" : "Noirs");
            VarianteMoteurUci1.Text = index < 0 || LogiqueMouvements.ListeCoups[index].EstPositionDeDepart
                ? "   [ Position initiale ]" : $"   [ {TexteCoupJoue(index, _positionAffichee)} ]";
            MiseaZeroParcours();
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
            ColoreDernierCoup();
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(LogiqueMouvements.RetourneChaineFenActuel()));
            InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + (QuiJoue == ColorPiece.Blanc ? "Blancs" : "Noirs");
            InformationsPartie.Text = PartieEnLectureSeule ? "Fin de la partie" : "";
        }
        private void QuitteParcours()
        {   // La partie va être remplacée (nouvelle partie, chargement) : l'échiquier suivra la partie
            _positionAffichee = null;
            MetAJourCommandes();
        }
        private void DessineEchiquierDe(Position position)
        {   // Dessine directement les 64 cases d'une position (sans passer par l'événement de la partie, ignoré pendant le parcours)
            for (int ligne = 2; ligne <= 9; ligne++)
                for (int colonne = 1; colonne <= 8; colonne++)
                    DessinePiece((ligne * 10) + colonne, position.Pieces[(ligne * 10) + colonne]);
        }
        private void DessinePieceDeLaPartie(int IndexCase, LogiqueMouvements.TypePiece Piece)
        {   // Dessins demandés par la partie (coups joués...) : ignorés pendant le parcours, l'échiquier est redessiné au retour
            if (!ParcoursEnCours)
                DessinePiece(IndexCase, Piece);
        }

        private void AbandonneReflexion()
        {   // Rend périmée la réflexion en cours (partie ou analyse) : le moteur s'arrête et sa réponse sera ignorée.
            // A appeler avant toute action qui change la partie ou la position (retour arrière, résultat, nouvelle partie, chargement...)
            if (!_pilote.Abandonner())
                return;     // le moteur ne réfléchissait pas : rien à signaler
            MiseaZéroTimer();
            TrackBarTempsReflexion.Enabled = true;
            LeMoteurARépondu();
            InformationPourJoueur.Text = StatusProgramme.Text = "Réflexion du moteur interrompue";
        }
        private void LeMoteurARépondu()
        {   // Après que le moteur a répondu (ou a été interrompu) : état des commandes
            MetAJourCommandes();
        }

        private void LancerReflexion()
        {   // Lance le timer de réflexion et affiche le message de temps restant
            NePasDérangerMoteur();
            // Durée en secondes
            _tempsRestant = _dureeReflexionMilliSeconde / 1000;
            labelTempsReflexion.Text = "[" + _tempsRestant.ToString() + "]";

            timer.Interval = 1000; // 1 seconde
            timer.Tick -= Timer_Tick;
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {   // Méthode appelée à chaque tick du timer (toutes les secondes)
            _tempsRestant--;
            labelTempsReflexion.Text = "[" + _tempsRestant.ToString() + "]";
            InformationsPartie.Text = "merci de patienter " + _tempsRestant.ToString() + " seconde(s)";
            if (_tempsRestant <= 0)
            {
                timer.Stop();
                Task.Delay(3000).ContinueWith(_ =>
                {   // attend 3 secondes avant de remettre la valeur initiale
                    this.Invoke(new Action(() =>
                    {
                        labelTempsReflexion.Text = "[" + (_dureeReflexionMilliSeconde / 1000).ToString() + "]";
                    }));
                });
            }
        }
        private void MiseaZéroTimer()
        {   // Appelée après que le moteur a répondu pour remettre le timer à zéro
            timer.Stop(); // stoppe le timer
            _tempsRestant = _dureeReflexionMilliSeconde / 1000; // reset
            labelTempsReflexion.Text = "[" + _tempsRestant.ToString() + "]";
            InformationsPartie.Text = ""; // si tu veux nettoyer le message
        }


    }
}
