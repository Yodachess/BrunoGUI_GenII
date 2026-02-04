// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII est développé par Bruno COURTOIS.  Copyright © 2025 █
// █ BrunoGUI_GenII est gratuit, sauf s'il est utilisé commercialement  █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
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
using ComponentFactory.Krypton.Toolkit;
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
        public bool OrdinateurJoueNoir, OrdinateurJoueBlanc;
        public string _dossierRacine;
        private int _indexSource120, _forceMoteurElo, _nombreLignesPV, _tempsRestant;
        private int _dernierCoupMoteurUci;   // dernière case jouée par le moteur UCI
        private int _numeroLigne;    // Indices dans la DataGrid FeuillePartie
        private int _indexCaseSourceDernierMouvement, _indexCaseDestinationDernierMouvement;
        private string _caseSource, _caseDestination, _couleurHumain;
        private string? _nomHumain, _joueurElo, _moteurElo, _joueurBlanc, _joueurNoir;
        private string? _cheminMoteur, _moteurChoisi, _variationMoteur, _meilleureSuite, _scoreCourant, _evaluationCourante;
        private string[] _donneesUci;        // Données en provenance du Moteur UCI
        private string? _bibliotheque = "rodent.bin";
        private bool _clickCaseSource, _visuSymbole, _montreDonneesBrutesUci, _montre3VariantesUci, _analyseEnCours, _montreListeParties, _partieTerminee;
        private bool _humain;   // True pour simuler 2 joueurs humains et False pour jouer contre le moteur UCI
        private bool _visuCoteNoir;          // True quand les Noirs sont en bas de l'écran
        private bool _clavierActif, _emetUnSon, _bibliothèqueAléatoire = false;
        private bool _bibliothèqueActive = true;
        private int _indexFenCoupActuel = 0; // Indice du coup affiché
        private int _dureeReflexionMilliSeconde = 5000;
        private Color _couleurCaseSombre, _couleurCaseClaire, _couleurCaseSource, _couleurCaseDestination;
        private LogiqueMouvements.TypePiece _selectionPromotion, _pieceSource;
        private Color _violetCustom = Color.FromArgb(128, 128, 255);  // Rouge = 128, Vert = 128, Bleu = 255
        public static PartieEchecsPGN PartieCourante = new();

        // les classes
        public LogiqueMouvements LogiqueMouvements = new();
        public MoteurUci MoteurUci = new();
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

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public EchiquierPrincipal()
        {
            InitializeComponent();

            parametres = new Parametres(); // valeurs par défaut
            parametres.ChargerDepuisIni("BrunoGUI.ini");    // Chargement du fichier des paramètres
            // Mise à jour des variables à partir des données du fichier
            _couleurCaseSombre = Color.FromName(parametres.CaseSombre);
            _couleurCaseClaire = Color.FromName(parametres.CaseClaire);
            _couleurCaseSource = Color.FromName(parametres.CouleurCaseSource);
            _couleurCaseDestination = Color.FromName(parametres.CouleurCaseDestination);
            _nomHumain = parametres.NomHumain;
            _dureeReflexionMilliSeconde = parametres.DureeReflexionSeconde * 1000;
            // Debug.WriteLine($" 1. _dureeReflexionMilliSeconde = {_dureeReflexionMilliSeconde}");
            _forceMoteurElo = parametres.ForceMoteur;
            _nombreLignesPV = parametres.NombreLignesPV;
            _bibliotheque = parametres.Bibliotheque;
            LabelJoueurNoir.Text = _cheminMoteur = parametres.Moteur;
            EloNoir.Text = _moteurElo = _forceMoteurElo.ToString();
            // Debug pour vérifier
            Debug.WriteLine($"Paramètres chargés : Moteur = {_cheminMoteur}, Case sombre = {_couleurCaseSombre.Name}, Case claire = {_couleurCaseClaire.Name}");
            Debug.WriteLine($"Paramètres chargés : Case source = {_couleurCaseSource.Name}, Case destination = {_couleurCaseDestination.Name}");
            Debug.WriteLine($"Paramètres chargés : Biblio = {_bibliotheque}, Force = {_forceMoteurElo}, Nombre PV = {_nombreLignesPV}");
            Debug.WriteLine($"Paramètres chargés : Temps de réflexion = {_dureeReflexionMilliSeconde}");

            OrdinateurJoueNoir = true;
            DateTime Aujourdhui = DateTime.Today;
            _visuCoteNoir = OrdinateurJoueBlanc = false; // On commence avec la vue côté Blanc, l'odinateur a les Noirs
            _montreDonneesBrutesUci = _analyseEnCours = _partieTerminee = _clavierActif = false;
            groupParcoursPartie.Enabled = RetourArriere.Enabled = AnalysePosition.Enabled = false;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = ListeCoupsBouton.Enabled = false;
            PartieEnCours.Date = Aujourdhui.ToString("yyyy.MM.dd");
            PartieEnCours.Lieu = "Maison"; PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.WhiteElo = PartieEnCours.BlackElo = "?"; PartieEnCours.Result = "*";
            mesparametresDeBase = new ParametresDeBase(this);
        }

        private void BrunoInterfaceGraphique_Load(object sender, EventArgs e)   // Forme Interface graphique
        {
            // les évènements dans les classes
            LogiqueMouvements.AfficheCoupNoir += AfficheCoupNoir;
            LogiqueMouvements.AfficheCoupBlanc += AfficheCoupBlanc;
            LogiqueMouvements.AfficheInfoEchec += AfficheInfoEchec;
            LogiqueMouvements.AfficheEchecEtMat += AfficheEchecEtMat;
            LogiqueMouvements.AfficheTour += AfficheTour;
            LogiqueMouvements.AffichePromotionPion += AffichePromotionPion;
            LogiqueMouvements.DessinePiece += DessinePiece;
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

            // _couleurCaseSombre = Color.CornflowerBlue;   // Couleurs cases noires par défaut
            // _couleurCaseClaire = Color.AliceBlue;        // Couleurs cases blanches par défaut
            
            _couleurCaseSombre = Color.FromArgb(181, 136, 99);  // Couleur style Lichess
            _couleurCaseClaire = Color.FromArgb(240, 217, 181); // Couleur style Lichess
            _couleurCaseSource = Color.FromArgb(134, 166, 108);
            _couleurCaseDestination = Color.FromArgb(196, 200, 127);
            

            _dossierRacine = Chemins.RepertoireRacine;
            string cheminMoteurs = Chemins.MoteursUCI;
            string cheminPolyglot = Chemins.BibliothèquesPolyglot;

            Debug.WriteLine("Dossier Racine = " + _dossierRacine);
            Debug.WriteLine("Chemin moteurs = " + cheminMoteurs);
            Debug.WriteLine("Chemin Polyglot = " + cheminPolyglot);

            InformationPourJoueur.Text = _dossierRacine;

            _moteurChoisi = Path.Combine(_dossierRacine, "stockfish", "stockfish17-windows-x86-64-avx2.exe");
            DessineEchiquier();
            for (int i = 0; i <= 119; i++)
            {   // On place des bords sur tout l'échiquier
                LogiqueMouvements.PiecesEchiquier.Add(LogiqueMouvements.TypePiece.Bordure);
                IndiceVisuCoteNoir.Add(i);      // et on crée la liste de 1 à 120
            }
            IndiceVisuCoteNoir.Reverse();       // On inverse l'ordre pour avoir la liste de 120 à 1 pour la vue côté noir
            LogiqueMouvements.InitialisationEchiquier();
            RécupèreBibliothèque();
            MiseaZeroAffichages();
            QuiJoue = ColorPiece.Blanc;
            this.ActiveControl = Plateau;       // Met le focus sur le plateau pour éviter le Bug des radiobutton "Résultat"
            _humain = false;                     // L'opposant est l'ordinateur, à mettre à true pour simuler 2 joueurs humains
            Debug.WriteLine("chemin Load = " + _moteurChoisi);
            Task.Run(() => MoteurUci.Start(_moteurChoisi)); // Démarre le moteur "de façon asynchrone" pour ne pas bloquer l'UI
        }

        private void NouvellePartieStockfish_Click(object sender, EventArgs e)
        {
            _humain = _analyseEnCours = _partieTerminee = _clavierActif = false;
            groupParcoursPartie.Enabled = RetourArriere.Enabled = AnalysePosition.Enabled = false;
            QuiJoue = ColorPiece.Blanc;
            _indexFenCoupActuel = 0;     // On est au début
            NumeroDemiCoup = 0;
            DémarreStockfish();
            if (maNouvellePartieForceModule.ShowDialog() == DialogResult.OK)
            {   // Utilise les sélections faites par l'utilisateur
                string couleurMoteur = maNouvellePartieForceModule.ChoixCouleur;
                bool forceMaximale = maNouvellePartieForceModule.ForceMaximale;
                _forceMoteurElo = maNouvellePartieForceModule.ForceModule;
                _dureeReflexionMilliSeconde = maNouvellePartieForceModule.DureeReflexionSeconde * 1000;
                TrackBarTempsReflexion.Value = maNouvellePartieForceModule.DureeReflexionSeconde;   // On met à jour la trackbar ...
                labelTempsReflexion.Text = "[" + TrackBarTempsReflexion.Value.ToString() + "]";

                Debug.WriteLine($" 2. _dureeReflexionMilliSeconde = {_dureeReflexionMilliSeconde} / DureeReflexionSeconde = {maNouvellePartieForceModule.DureeReflexionSeconde}");
                MiseaZeroAffichages();
                if (forceMaximale)
                {   // Moteur à sa force Elo maximale
                    _forceMoteurElo = 3150;
                }
                MoteurUci.ActiveLimiteElo();
                MoteurUci.DefinitLimiteElo(_forceMoteurElo.ToString());
                MoteurUci.StandardInputDataToUci("setoption name MultiPV value " + mesParametresUciStockfish.MultiPV);
                if (couleurMoteur == "Blancs")
                {   // Le moteur joue les blancs
                    OrdinateurJoueNoir = false;
                    _couleurHumain = "Blancs";
                    PartieEnCours.White = LabelJoueurBlanc.Text = _moteurChoisi;
                    PartieEnCours.WhiteElo = EloBlanc.Text = _forceMoteurElo.ToString();
                    PartieEnCours.Black = LabelJoueurNoir.Text = maNouvellePartieForceModule.NomAdversaire;
                    PartieEnCours.BlackElo = EloNoir.Text = _joueurElo;
                    CommencerPartie();
                    if (_visuCoteNoir == false)
                        TourneEchiquier();      // On met la vue côté Noir
                    ParametresJoueurHumain("Noirs", "Le moteur UCI joue");      // On fait jouer le moteur côté blanc
                    JeuMoteurAvecBibliothèque(FenDepart);
                }
                else
                {   // Le moteur joue les noirs
                    OrdinateurJoueNoir = true;
                    _couleurHumain = "Noirs";
                    PartieEnCours.White = LabelJoueurBlanc.Text = maNouvellePartieForceModule.NomAdversaire;
                    PartieEnCours.WhiteElo = EloBlanc.Text = _joueurElo;
                    PartieEnCours.Black = LabelJoueurNoir.Text = _moteurChoisi;
                    PartieEnCours.BlackElo = EloNoir.Text = _forceMoteurElo.ToString();
                    if (_visuCoteNoir)
                        TourneEchiquier();
                    CommencerPartie();
                    ParametresJoueurHumain("Blancs", "A vous de jouer");            // On demande à l'humain de jouer
                    PlateauEnable(true);                                            // On lui permet de bouger les pièces
                }
            }
        }
        private void CommencerPartie()              // Début d'une nouvelle partie
        {
            _analyseEnCours = _partieTerminee = _clavierActif = false;
            groupParcoursPartie.Enabled = RetourArriere.Enabled = AnalysePosition.Enabled = false;
            _dernierCoupMoteurUci = -1;
            _clickCaseSource = _visuSymbole = true;
            PartieEnCours.CoupsPartiePGN = "";
            NumeroDemiCoup = 0;
            MiseaZeroAffichages();
            MiseaZéroTimer();
            // DémarrageMoteur();
            _couleurHumain = VarianteMoteurUci1.Text = string.Empty;
            if (_humain == false)
            {
                InformationPourJoueur.Visible = true;
                InformationPourJoueur.Text = "Pour commencer une partie, cliquer \n sur Partie et choisissez votre couleur";
            }
            else
            {
                PartieEnCours.White = PartieEnCours.Black = "_humain";
                PlateauEnable(true);
                InformationPourJoueur.Visible = true;
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux Blancs de jouer";
            }
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion du click de la souris
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void CaseMouseDown(object sender, MouseEventArgs e)
        {   // Le joueur sélectionne la case source ou destination avec la souris
            try
            {
                if (sender is PictureBox CaseClick)
                {
                    int IndexCase120 = Convert.ToInt32(CaseClick.Name[8..]); // Utilise le numéro de la PictureBox comme index
                    if (_visuCoteNoir)
                        IndexCase120 = IndiceVisuCoteNoir[IndexCase120];    // Si on regarde côté noir, il faut inverser l'index par rapport a la vue côté blanc
                    if (_couleurHumain == string.Empty && _humain == false)
                        KryptonMessageBox.Show("Veuillez choisir votre couleur\n(Menu Partie / Nouvelle Partie)", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                            LogiqueMouvements.ExecutionCoup(_caseSource, _caseDestination);
                            string chaineFen = LogiqueMouvements.RetourneChaineFenActuel();     // UCI : remplacer le FEN par liste de coups ?!

                            if (LogiqueMouvements.CoupValide)
                            {       // envoi de la Position Fen au moteur UCI
                                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(chaineFen));
                                if (_humain == false)
                                {
                                    JeuMoteurAvecBibliothèque(chaineFen);
                                }
                            }
                            else
                            {
                                DessinePiece(_indexSource120, _pieceSource);  // Si le coup n'est pas valide, on remet la pièce sur sa case d'origine !
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
        private void AfficheUci()               // Analyse et affiche les informations du moteur UCI
        {   // ATTENTION : MALGRE LA PRESENCE DU PROTOCOLE UCI, LES MOTEURS ONT DES REPONSES DIFFERENTES !!?? (voir case "info", par ex)
            string numeroVarianteNomBox = "";
            string varianteExaminee = "";
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheUci));
                return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
            }
            else
                _donneesUci = MoteurUci.DataUci.Split(' ');                      // Découpage des informations du moteur UCI
            if (string.IsNullOrWhiteSpace(_donneesUci[0]) == false & _donneesUci.Length >= 2)
            {   // true si la chaine est " ", "\n", null, ""
                _donneesUci = MoteurUci.DataUci.Trim().Split(' ');
                switch (_donneesUci[0])     // identifier le premier mot
                {
                    case "\n":              // Analyse réponse moteur UCI
                    case " ":
                        break;
                    case "bestmove":        // **** le moteur UCI propose le meilleur coup ! ****
                        VarianteMoteurCourante.Text = "Coup joué : " + _donneesUci[1] +
                            (_donneesUci.Length > 3 ? "   (Conseil : " + _donneesUci[3] + ")" : "");
                        break;
                    case "id":
                        {
                            switch (_donneesUci[1])
                            {
                                case "name":    // Récupération du nom du moteur
                                    _moteurChoisi = MoteurUci.DataUci.Substring(8);
                                    _moteurChoisi = _moteurChoisi.Substring(0, Math.Min(20, _moteurChoisi.Length));
                                    break;

                                case "author":  // Récupération de l'auteur
                                    VarianteMoteurUci3.Text = "     Auteur(s) du moteur " + _moteurChoisi + " = " + MoteurUci.DataUci.Substring(10);
                                    break;
                            }
                        }
                        break;
                    case "info":                // **** Infos de réflexion moteur ****
                        {   // Parcours des données Uci
                            for (int ucindex = 1; (ucindex < _donneesUci.Length); ucindex++) // Recherche des informations sur la chaine _donneesUci
                                switch (_donneesUci[ucindex])
                                {
                                    case "book":
                                        VarianteMoteurUci1.Invoke(new Action(() =>
                                        {
                                            VarianteMoteurUci1.Text = "    Le moteur est dans sa bibliothèque d'ouvertures";
                                        }));
                                        break;

                                    case "multipv":
                                        numeroVarianteNomBox = "VarianteMoteurUci" + _donneesUci[ucindex + 1];
                                        break;

                                    case "cp":
                                        _scoreCourant = (Decimal.Parse(_donneesUci[ucindex + 1]) / 100).ToString("N2", CultureInfo.InvariantCulture);
                                        if (numeroVarianteNomBox == "VarianteMoteurUci1")
                                        {   // On affiche seulement le score de la meilleure variante
                                            ScoreMoteur.Text = "Score : " + _scoreCourant;
                                            AfficheEvaluation(_scoreCourant);
                                        }
                                        break;

                                    case "mate":
                                        string nombreCoupsMat = "MAT en " + Math.Abs(int.Parse(_donneesUci[ucindex + 1]));
                                        _scoreCourant = "M" + Math.Abs(int.Parse(_donneesUci[ucindex + 1]));
                                        if (numeroVarianteNomBox == "VarianteMoteurUci1")
                                        {   // On affiche le Mat seulement si c'est la meilleure variante
                                            InformationPourJoueur.Text = ScoreMoteur.Text = nombreCoupsMat;
                                        }
                                        break;

                                    case "pv":          // Affichage de la variation principlale
                                        int position = MoteurUci.DataUci.IndexOf(" pv ");
                                        _variationMoteur = MoteurUci.DataUci.Substring(position + 3);
                                        if (_variationMoteur.Length > 60)
                                            _variationMoteur = _variationMoteur.Substring(0, 60);     // On limite la longueur de la variation, pour rester dans le label
                                        _variationMoteur = Outils.AlgebriqueVersPgn(_variationMoteur, NumeroDemiCoup);  // Elle est en algébrique long, il la faut en PGN Fr ...
                                        varianteExaminee = string.Join(" ", _variationMoteur.Split(' ').Take(3));
                                        if (numeroVarianteNomBox != "")     // Par exemple Sargon n'a pas de multipv ?
                                        {
                                            RichTextBox numeroVarianteBox = Controls.Find(numeroVarianteNomBox, true).FirstOrDefault() as RichTextBox;
                                            numeroVarianteBox?.Invoke(new Action(() =>    // Si numeroVarianteBox n'est pas nul
                                            {
                                                numeroVarianteBox.Text = " " + numeroVarianteNomBox[^1] + ". (" + varianteExaminee + ") █[ " +
                                                _scoreCourant + " ]█  " + "[ " + _variationMoteur + " ]";
                                                Debug.WriteLine($"Variation : {numeroVarianteBox.Name}, / {_variationMoteur}");
                                            }));
                                        }
                                        else
                                        {                                       // Pour ceux qui n'ont qu'une variante principale (Sargon, ...) ?!
                                            VarianteMoteurUci1.Invoke(new Action(() =>
                                            {  // On n'utilise que la Box VarianteMoteurUci1 ...
                                                VarianteMoteurUci1.Text = " 1. (" + varianteExaminee + ") █[ " + _scoreCourant + " ]█  " + "[ " + _variationMoteur + " ]";
                                                VarianteMoteurUci2.Text = "... " +  _moteurChoisi + " n'affiche qu'une variante ..."; VarianteMoteurUci3.Text = "...";
                                                Debug.WriteLine($"Seulement une variante !!! : {VarianteMoteurUci1.Text}");
                                            }));
                                        }
                                        break;
                                }
                        }
                        break;
                }
            }
        }

        private string AfficheEvaluation(string _scoreCourant)
        {
            // if (_analyseEnCours) return "";
            Debug.WriteLine($"_scoreCourant : {_scoreCourant}");
            string resultat = "";
            _scoreCourant = _scoreCourant?.Trim();
            // Cas mat : "Mx"
            if (!string.IsNullOrEmpty(_scoreCourant) &&
                _scoreCourant.StartsWith("M", StringComparison.OrdinalIgnoreCase))
            {
                string number = _scoreCourant.Substring(1);

                int.TryParse(number, out int mateIn); // si ça rate, mateIn = 0, pas grave

                resultat = QuiJoue switch
                {
                    ColorPiece.Blanc => "Gain Blanc",
                    ColorPiece.Noir => "Gain Noir",
                    _ => "Gain (mat)" // fallback théorique
                };

                EvaluationUci.Text = _evaluationCourante = resultat;
                return resultat;
            }
            if (!decimal.TryParse(_scoreCourant, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal score))
            {   // Valeur invalide → on affiche quelque chose de neutre
                EvaluationUci.Text = _evaluationCourante = "Éval indisponible";
                return _evaluationCourante;
            }
            if (OrdinateurJoueNoir) score = -score;
            resultat = score switch
            {   // Entre -0.5 +0.5, c'est égal / de 0.5 à 2.5 c'est Avantage / supérieur à 2.5 c'est Gain
                >= 2.5m => "Gain Blanc",
                > 0.5m => "Avantage Blanc",
                <= -2.5m => "Gain Noir",
                < -0.5m => "Avantage Noir",
                _ => "Égal"
            };
            EvaluationUci.Text = _evaluationCourante = resultat;
            return resultat;
        }

        private void AfficheDonneesBrutes()
        {
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
                        donneesBrutesUci.DonneesBrutesVue.AppendText(Environment.NewLine + "[" + _moteurChoisi + "]    " + MoteurUci.DataUci);
                }
                else
                    donneesBrutesUci.DonneesBrutesVue.AppendText(Environment.NewLine + " [BrunoGUI_GenII]    " + MoteurUci.DataVersUci);
                donneesBrutesUci.DonneesBrutesVue.ScrollToCaret();  // Pour garder l'affichage dans toute la fenêtre
            }
        }

        private void AfficheCoupMoteur()        // Le moteur UCI joue son meilleur coup
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(AfficheCoupMoteur));
                return; // Empêche l'exécution du reste de la méthode sur le thread d'origine
            }
            else
            {
                TrackBarTempsReflexion.Enabled = true;
                if (_emetUnSon)
                {   // Son pour dire que le coup est joué
                    SoundPlayer player = new(@"C:\Windows\Media\Windows Notify.wav");
                    player.Play();
                }
                if (!_analyseEnCours)
                {   // Si ce n'est pas une analyse ...
                    StatusProgramme.Text = InformationPourJoueur.Text = "A vous de jouer";
                    _caseSource = MoteurUci.CoupAuFormatUci.Substring(0, 2);    // CoupAuFormatUci contient le "best move" sous la forme e2e4
                    _caseDestination = MoteurUci.CoupAuFormatUci.Substring(2, 2);

                    // *******Traitement promotion *********
                    if (MoteurUci.CoupAuFormatUci.Length >= 5)
                    {   // Gestion de la promotion : 5ème caractère de l'UCI (index 4)
                        char promo = char.ToLower(MoteurUci.CoupAuFormatUci[4]);
                        char destRank = _caseDestination[1];        // '1'..'8'
                        bool isWhitePromotion = destRank == '8';    // promotion en 8 => blanc
                        switch (promo)
                        {
                            case 'q': PromotionPiece = isWhitePromotion ? TypePiece.ReineBlanche : TypePiece.ReineNoire; break;
                            case 'r': PromotionPiece = isWhitePromotion ? TypePiece.TourBlanche : TypePiece.TourNoire; break;
                            case 'b': PromotionPiece = isWhitePromotion ? TypePiece.FouBlanc : TypePiece.FouNoir; break;
                            case 'n': PromotionPiece = isWhitePromotion ? TypePiece.CavalierBlanc : TypePiece.CavalierNoir; break;
                            default: PromotionPiece = TypePiece.Vide; break;
                        }
                        LogiqueMouvements.BloquerChoixPromo = true;
                    }
                    else
                    {   // Pas de promotion dans le UCI : assurer une valeur neutre
                        PromotionPiece = TypePiece.Vide;
                    }
                    // *******Traitement promotion *********

                    if (LogiqueMouvements.EchecetMat == false)  // Note : Si c'est Mat, on n"execute pas de coup
                    {
                        LogiqueMouvements.ExecutionCoup(_caseSource, _caseDestination);   // Exécute un coup du moteur UCI
                    }
                    if (_dernierCoupMoteurUci != -1)
                    {       // on redessine la case pour effacer le contour du coup précédent du Moteur UCI
                        PictJeux[_dernierCoupMoteurUci].BackColor = CouleurCaseOrigines[_dernierCoupMoteurUci];
                        DessinePiece(_dernierCoupMoteurUci, LogiqueMouvements.PiecesEchiquier[_dernierCoupMoteurUci]);
                    }
                    _dernierCoupMoteurUci = LogiqueMouvements.RenvoieCaseIndex120(_caseDestination);
                    // *******Traitement promotion *********
                    LogiqueMouvements.BloquerChoixPromo = false;
                    // *******Traitement promotion *********
                    _indexCaseSourceDernierMouvement = RenvoieCaseIndex120(_caseSource);              // convertit la case source en index
                    _indexCaseDestinationDernierMouvement = RenvoieCaseIndex120(_caseDestination);    // convertit la case destination en index
                    PictJeux[_indexCaseSourceDernierMouvement].BackColor = _couleurCaseSource;            // montre la case source du dernier coup
                    PictJeux[_indexCaseDestinationDernierMouvement].BackColor = _couleurCaseDestination;  // montre la case destination du dernier coup
                    LeMoteurARépondu();      // On réautorise si le moteur a fini de réfléchir
                    if (LogiqueMouvements.EchecetMat == false)
                        BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = true;
                }
                else
                {   // c'est une analyse, on affiche la meilleure variante
                    {
                        string[] meilleureVariante = VarianteMoteurUci1.Text.Split(['[', ']'], StringSplitOptions.RemoveEmptyEntries);
                        string debutVariante = Regex.Match(meilleureVariante[0], @"\((.*?)\)").Groups[1].Value;
                        _meilleureSuite = debutVariante + " Evaluation --- " + meilleureVariante[1] + "(" + _evaluationCourante + ")" + " ---\n" + meilleureVariante[3];
                        ScoreMoteur.Text = _evaluationCourante = "Score = " + meilleureVariante[1];
                        AfficheEvaluation(meilleureVariante[1]);
                        VarianteMoteurCourante.Text = InformationsPartie.Text = "Coup suggéré : " + debutVariante;
                        InformationPourJoueur.Text = StatusProgramme.Text = "Analyse terminée ... ";
                        _ = KryptonMessageBox.Show("La meilleure suite est : " + debutVariante +
                                            "\n Evaluation --- " + meilleureVariante[1] + " --- " + "(" + AfficheEvaluation(meilleureVariante[1]) + ")" +
                                            "\n" + meilleureVariante[3], "Analyse Moteur " + " (" + _dureeReflexionMilliSeconde / 1000 + " sec.)"
                                            + " par " + _moteurChoisi, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    RetourArriere.Enabled = AnalysePosition.Enabled = groupParcoursPartie.Enabled = true;      // On réautorise si le moteur a fini de réfléchir
                    _analyseEnCours = false;
                }
            }
        }

        private void AfficheCoupBlanc(string coupBlanc) // Affiche le coup joué par les blancs
        {                                               // Comme c'est le coup Blanc, il faut afficher le numéro du coup
            if (LogiqueMouvements.EchecetMat == false)
            {
                if (LogiqueMouvements.Echec)
                    coupBlanc += "+";                   // S'il y a echec, on le signale en ajoutant un "+" après le coup
                NumeroDemiCoup = LogiqueMouvements.ListeCoupsFen.Count - 1;     // DEBUG 28/5/24
                PartieEnCours.CompteDePLy = (LogiqueMouvements.ListeCoupsFen.Count).ToString();
                _numeroLigne++;
                if (LogiqueMouvements.TripleRepetition())
                {
                    PartieNulle_Repetition();
                }
                RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = true;
            }
            else
            {
                StatusProgramme.Text = "Partie terminée";
            }
        }

        private void AfficheCoupNoir(string coupNoir)       //  Affiche le coup joué par les noirs
        {                                                   // Comme c'est le coup Noir, on n'a pas besoin d'afficher le numéro du coup
            if (LogiqueMouvements.EchecetMat == false)
            {
                if (LogiqueMouvements.Echec)
                    coupNoir += "+";                    // S'il y a echec, signalé en ajoutant un "+" après le coup
                NumeroDemiCoup = LogiqueMouvements.ListeCoupsFen.Count - 1;     // DEBUG 28/5/24
                PartieEnCours.CompteDePLy = (LogiqueMouvements.ListeCoupsFen.Count).ToString();
                if (LogiqueMouvements.TripleRepetition())
                {
                    PartieNulle_Repetition();
                }
                RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = true;
            }
            else
            {
                StatusProgramme.Text = "Partie terminée";
            }
        }

        private void AfficheTour(string Couleur)        // Affiche la couleur du joueur humain courant
        {
            if (_humain)
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux " + Couleur + " de jouer";
            else
                PlateauEnable(Couleur == _couleurHumain); // active les Picturebox si c'est au tour du joueur humain
        }

        private void AfficheEchecEtMat(string couleurRoiMat)   // Affiche l'échec et mat du roi de la couleur en paramètre
        {
            int indexCouleur = couleurRoiMat == "Blanc" ? 2 : 1;
            LogiqueMouvements.ListeCoupsPgnIntl[^1] = (LogiqueMouvements.ListeCoupsPgnIntl[^1].ToString()).Replace("+", "#");
            LogiqueMouvements.ListeCoupsPgnFr[^1] = (LogiqueMouvements.ListeCoupsPgnFr[^1].ToString()).Replace("+", "#");
            LogiqueMouvements.ListeCoupsNal[^1] = (LogiqueMouvements.ListeCoupsNal[^1].ToString()).Replace("+", "#");
            string coupMat = (LogiqueMouvements.ListeCoupsPgnFr[LogiqueMouvements.ListeCoupsPgnIntl.Count - 1].ToString()).Replace("+", "#");
            int indexPoint = coupMat.IndexOf('.');  // On enlève le numéro de coup s'il existe
            if (indexPoint != -1)
            {   // Ce if n'est jamais éxecuté, mais pourrait être utile ?
                coupMat = coupMat.Substring(indexPoint).Replace(".", "");
            }
            if (indexCouleur == 2)
            {   // Couleur du Roi mat = Blanc
                GestionResultat("0-1", " Gain Noir");
            }
            else
            {   // Couleur du Roi mat = Noir
                GestionResultat("1-0", " Gain Blanc");
            }
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;     // Le résultat est déjà défini
            RetourArriere.Visible = false;
            InformationPourJoueur.Text = VarianteMoteurCourante.Text = "Le Roi " + couleurRoiMat + " est échec et mat";
            StatusProgramme.Text = "Partie terminée";
            Application.DoEvents();
            PlateauEnable(false);
        }

        private void AfficheInfoEchec(string infoechec)    // Affiche le texte dans l'étiquette
        {
            InformationsPartie.Text = infoechec;
            InformationsPartie.ForeColor = Color.DarkGreen;
            if (infoechec.Contains("échec") || infoechec.Contains("Pat"))
                InformationsPartie.ForeColor = Color.DarkGreen;
            if (infoechec.Contains("Pat"))
            {
                GestionResultat("1/2-1/2", "Pat (Nulle)");
                PlateauEnable(false); // un des joueurs est pat : fin de la partie
            }
        }
        private void AffichePromotionPion(string Couleur)        // Promotion d'un pion
        {
            _selectionPromotion = LogiqueMouvements.TypePiece.Vide;
            Promo0.Image = Couleur == "Noir" ? ReineNoire : ReineBlanche;
            Promo1.Image = Couleur == "Noir" ? TourNoire : TourBlanche;
            Promo2.Image = Couleur == "Noir" ? FouNoir : FouBlanc;
            Promo3.Image = Couleur == "Noir" ? CavalierNoir : CavalierBlanc;
            GroupPromo.Visible = true;
            while (_selectionPromotion == LogiqueMouvements.TypePiece.Vide)
                Application.DoEvents();
            GroupPromo.Visible = false;
        }
        private void EffaceDernierCoup()
        {
            PictJeux[_indexCaseSourceDernierMouvement].BackColor = Outils.EstCaseClaire(_indexCaseSourceDernierMouvement) ? _couleurCaseClaire : _couleurCaseSombre;
            PictJeux[_indexCaseDestinationDernierMouvement].BackColor = Outils.EstCaseClaire(_indexCaseDestinationDernierMouvement) ? _couleurCaseClaire : _couleurCaseSombre;
        }

        private void BoutonGainBlanc_Click(object sender, EventArgs e)
        {
            GestionResultat("1-0", " Gain Blanc");
        }

        private void BoutonGainNoir_Click(object sender, EventArgs e)
        {
            GestionResultat("0-1", " Gain Noir");
        }

        private void BoutonNulle_Click(object sender, EventArgs e)
        {
            GestionResultat("1/2-1/2", " Nulle");
        }

        private void PartieNulle_Repetition()
        {
            GestionResultat("1/2-1/2", "Nulle par répétition");
        }

        private void GestionResultat(string resultat, string vainqueur)
        {
            StopMoteur_Click(null, EventArgs.Empty);    // Au cas où le moteur tourne encore ?!
            LogiqueMouvements.ListeCoupsPgnIntl.Add(resultat);
            LogiqueMouvements.ListeCoupsPgnFr.Add(resultat);
            LogiqueMouvements.ListeCoupsNal.Add(resultat);
            PartieEnCours.Result = EvaluationUci.Text = resultat;
            ScoreMoteur.Text = vainqueur;
            InformationsPartie.Text = resultat + "  (" + vainqueur + ")";
            StatusProgramme.Text = "Partie terminée";
            _partieTerminee = KryptonApropos.Enabled = true;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = RetourArriere.Enabled = false;
            PlateauEnable(false);
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des menus
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void HumainOrdinateur_Click(object sender, EventArgs e)
        {
            _couleurHumain = "Blancs";
            QuiJoue = ColorPiece.Blanc;
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomHumain;
            PartieEnCours.Black = LabelJoueurNoir.Text = _moteurChoisi;
            PartieEnCours.BlackElo = _moteurElo;
            OrdinateurJoueNoir = true;
            _humain = OrdinateurJoueBlanc = _analyseEnCours = _partieTerminee = AnalysePosition.Enabled = groupParcoursPartie.Enabled = false;
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous aurez les Blancs contre " + _moteurChoisi + ". " + "\nToute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Le joueur a les Blancs, l'ordinateur les Noirs ", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                if (_visuCoteNoir)
                    TourneEchiquier();
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
                StatusProgramme.Text = ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "";    // On efface les données de la partie précédente
                CommencerPartie();
                ParametresJoueurHumain("Blancs", "A vous de jouer");            // On demande à l'humain de jouer
                PlateauEnable(true);                                            // On lui permet de bouger les pièces
            }
        }
        private void OrdinateurHumain_Click(object sender, EventArgs e)
        {
            _couleurHumain = "Noirs";
            QuiJoue = ColorPiece.Blanc;
            PartieEnCours.White = LabelJoueurBlanc.Text = _moteurChoisi;
            PartieEnCours.Black = LabelJoueurNoir.Text = _nomHumain;
            PartieEnCours.WhiteElo = _moteurElo;
            OrdinateurJoueBlanc = true;
            _humain = OrdinateurJoueBlanc = _analyseEnCours = _partieTerminee = AnalysePosition.Enabled = groupParcoursPartie.Enabled = false;
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous aurez les Noirs contre " + _moteurChoisi + ". " + "\nToute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Le joueur a les Noirs, l'ordinateur les Blancs ", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                StatusProgramme.Text = ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "";     // On efface les données de la partie précédente
                CommencerPartie();
                if (_visuCoteNoir == false)
                    TourneEchiquier();                                          // On met la vue côté Noir
                ParametresJoueurHumain("Noirs", "Le moteur UCI joue");
                NumeroDemiCoup = 0;
                JeuMoteurAvecBibliothèque(FenDepart);
            }
        }
        private void HumainContreHumain_Click(object sender, EventArgs e)
        {
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomHumain;
            PartieEnCours.Black = "Adversaire";
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous jouez contre votre ami/partenaire,\n" + "ou vous saisissez une partie ...\n" +
                "Toute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Jeu entre amis, ou saisie de partie", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                _humain = OrdinateurJoue.Enabled = true;
                OrdinateurJoueBlanc = OrdinateurJoueNoir = false;
                StatusProgramme.Text = "_humain contre _humain";
                InformationsPartie.Text = " Bruno vous souhaite une bonne partie !";
                MoteurUci.ActiveLimiteElo();        // Préparation du moteur en cas de demande d'analyse
                MoteurUci.DefinitLimiteElo("3190");
                MoteurUci.StandardInputDataToUci("setoption name MultiPV value 3");
                maNouvellePartieForceModule.DureeReflexionSeconde = 10;
                CommencerPartie();
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
            _moteurChoisi = "Rodent IV ";
            EloNoir.Text = "+- 3000";
            Directory.SetCurrentDirectory(Chemins.MoteursUCI + @"\Rodent_IV");
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\Rodent_IV", "rodent-iv-x64.exe");
            Debug.WriteLine("Chemin Rodent IV = " + _cheminMoteur);
            DémarrageMoteur();
        }
        private void Sargon1_1978_Click(object sender, EventArgs e)
        {   // https://echecs-et-informatique.franceserv.com/sargon-1978.html
            _moteurChoisi = "Sargon I 1978";
            EloNoir.Text = "1678";
            Directory.SetCurrentDirectory(Chemins.MoteursUCI + @"\sargon1978");
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\sargon1978", "sargon1978_1_01b.exe");
            Debug.WriteLine("Chemin sargon I 1978 = " + _cheminMoteur);
            DémarrageMoteur();
            MoteurUci.SpecialeSargon();         // Sinon Sargon  mouline sans fin !!!!!
        }
        public void DémarreStockfish()
        {   //  https://stockfishchess.org/
            _moteurChoisi = "Stockfish 17 ";
            // EloNoir.Text = "+- 3000";
            Directory.SetCurrentDirectory(Application.StartupPath);
            _cheminMoteur = Path.Combine(_dossierRacine, "stockfish", "stockfish17-windows-x86-64-avx2.exe");
            Debug.WriteLine("Chemin Stockfish = " + _cheminMoteur);
            DémarrageMoteur();
        }
        private void DémarrageMoteur()
        {
            StopMoteur_Click(null, EventArgs.Empty);    // On arrête le précédent moteur
            MoteurUci.Quitte();
            Debug.WriteLine("Chemin DémarrageMoteur = " + _cheminMoteur);
            MoteurUci.Start(_cheminMoteur); // on démarre le nouveau moteur Uci
            _moteurChoisi = Path.GetFileNameWithoutExtension(_cheminMoteur);
            Debug.WriteLine("Moteur = " + _moteurChoisi);
            if (OrdinateurJoueBlanc)
                LabelJoueurBlanc.Text = _moteurChoisi;
            if (OrdinateurJoueNoir)
                LabelJoueurNoir.Text = _moteurChoisi;
        }
        private void ParametresDeBase_Click(object sender, EventArgs e)
        {
            mesparametresDeBase.Show();
        }
        private void ParametresAvances_Click(object sender, EventArgs e)
        {
            mesParametresUciStockfish.Show();
        }
        private void StopMoteur_Click(object sender, EventArgs e)
        {   // Arrête le moteur UCI (utilise si réflexion infinie)
            timer.Stop();
            MoteurUci.StandardInputDataToUci("stop");
            InformationPourJoueur.Text = "Arrêt réflexion Moteur ";
        }

        private void CaseSombre_Click(object sender, EventArgs e)
        {
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
            {
                _couleurCaseSombre = CouleurDialogue.Color;          // On récupère la couleur choisie par l'utilisateur
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
        {
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
            {
                _couleurCaseClaire = CouleurDialogue.Color;          // On récupère la couleur choisie par l'utilisateur
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
        {
            HumainContreHumain_Click(sender, e);
        }
        private void AnalysePosition_Click(object sender, EventArgs e)
        {
            InformationPourJoueur.Text = StatusProgramme.Text = "Analyse de la position ...";
            _analyseEnCours = true;
            if (NumeroDemiCoup <= LogiqueMouvements.ListeCoupsFen.Count - 1)            // on empêche d'analyser sur une case au-delà de la partie ...
            {
                string Fenaenvoyer = LogiqueMouvements.ListeCoupsFen[NumeroDemiCoup];   // Récupère le FEN (position)
                RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = false; // Il faut empêcher le retour si le moteur réfléchit
                BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = groupParcoursPartie.Enabled = false;
                LancerReflexion();  // Décompte le temps de réflexion
                MoteurUci.JeuMoteurUci(Fenaenvoyer, _dureeReflexionMilliSeconde);     // On fait jouer le moteur, avec le temps de réflexion choisi
                Debug.WriteLine($"Durée Réflexion (analyse postion) =  {_dureeReflexionMilliSeconde}");
            }
        }
        private void InverseEchiquier_Click(object sender, EventArgs e)
        {
            PlateauEnable(true);
            TourneEchiquier();
        }
        private void OrdinateurJoue_Click(object sender, EventArgs e)
        {
            if (QuiJoue == ColorPiece.Blanc)
            {
                _couleurHumain = "Noirs";
                OrdinateurJoueBlanc = true;
            }
            else
            {
                _couleurHumain = "Blancs";
                OrdinateurJoueNoir = true;
            }
            _humain = groupParcoursPartie.Enabled = false;
            string Fenaenvoyer;
            if (ListeCoupsFen.Count > 0)
                Fenaenvoyer = ListeCoupsFen[^1];   // dernier FEN
            else
            {
                NumeroDemiCoup = 0;
                Fenaenvoyer = FenDepart; // position initiale
            }
            Debug.WriteLine($"Dernier coup PGN : {(ListeCoupsPgnIntl.Count > 0 ? ListeCoupsPgnIntl[^1] : "aucun")}, Demi coup : {NumeroDemiCoup}");
            JeuMoteurAvecBibliothèque(Fenaenvoyer);
            PlateauEnable(true);
            _clickCaseSource = _visuSymbole = true;    // L'ordinateur ayant joué, c'est indispensable !
        }
        private void RetourArriere_Click(object sender, EventArgs e)
        {
            EffaceDernierCoup();
            if (LogiqueMouvements.ListeCoupsFen.Count == 0)
                _ = KryptonMessageBox.Show("Pas assez de coups joués \nPas de retour arrière possible", "Retour impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                Outils.ChangerDeCoté();
                NombreCoupsJoues -= Convert.ToSingle(0.5);      // On décrémente d'un demi-coup
                NumeroDemiCoup--;
                LogiqueMouvements.ListeCoupsFen.RemoveAt(LogiqueMouvements.ListeCoupsFen.Count - 1);    // On supprime le dernier 1/2 coup
                LogiqueMouvements.ListeCoupsPgnIntl.RemoveAt(LogiqueMouvements.ListeCoupsPgnIntl.Count - 1);        // pour les 5 listes
                LogiqueMouvements.ListeCoupsPgnFr.RemoveAt(LogiqueMouvements.ListeCoupsPgnFr.Count - 1);
                LogiqueMouvements.ListeCoupsNal.RemoveAt(LogiqueMouvements.ListeCoupsNal.Count - 1);
                LogiqueMouvements.ListeCoupsUci.RemoveAt(LogiqueMouvements.ListeCoupsUci.Count - 1);
                if (LogiqueMouvements.ListeCoupsFen.Count == 0)
                {
                    LogiqueMouvements.MiseenplaceFen(FenDepart);
                    _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
                    NumeroDemiCoup = 0;
                    PetitRoqueBlancPossible = PetitRoqueNoirPossible = GrandRoqueBlancPossible = GrandRoqueNoirPossible = true;
                }
                else
                {
                    // Réactivation du roque si besoin ...  (champ 3 du FEN, mais indice 2 du Split)
                    string[] FenPrecedent = LogiqueMouvements.ListeCoupsFen[^1].Split(' ');  // Récupère le dernier FEN découpé
                    if ((FenPrecedent[2]).Contains('-'))
                    {   // Aucun roque possible
                        StatutRoque = FlagEnableRoque.AucunRoque;
                        PetitRoqueNoirPossible = PetitRoqueBlancPossible = GrandRoqueNoirPossible = GrandRoqueBlancPossible = false;
                    }
                    else if ((FenPrecedent[2]).Contains('K'))
                    {   // Petit roque blanc possible
                        PetitRoqueBlancPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueBlanc;
                    }
                    else if ((FenPrecedent[2]).Contains('Q'))
                    {   // Grand roque blanc possible
                        GrandRoqueBlancPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueBlanc;
                    }
                    else if ((FenPrecedent[2]).Contains('k'))
                    {   // Petit roque noir possible
                        PetitRoqueNoirPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueNoir;
                    }
                    else if ((FenPrecedent[2]).Contains('q'))
                    {   // Grand roque noir possible
                        GrandRoqueNoirPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueNoir;
                    }
                    string Fenaenvoyer = LogiqueMouvements.ListeCoupsFen[^1];    // Récupère le dernier FEN (position)
                    LogiqueMouvements.MiseenplaceFen(Fenaenvoyer);              // et on l'affiche sur l'échiquier
                    _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(Fenaenvoyer));
                }
                InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + QuiJoue + "s";
                InformationsPartie.Text = OrdinateurJoueBlanc ? "L'ordinateur joue les Blancs" :
                          OrdinateurJoueNoir ? "L'ordinateur joue les Noirs" :
                          "L'ordinateur ne joue pas cette partie";
                OrdinateurJoue.Enabled = true;
            }
        }
        private void ListeCoupsBouton_Click(object sender, EventArgs e)
        {
            string numeroCoup = "";
            string blancs = "";
            string noirs = "";
            PlateauEnable(false);   // Blocage du plateau car je ne veux pas autoriser de jouer pendant le parcours de la partie ....
            RetourArriere.Enabled = groupParcoursPartie.Enabled = false;
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
            // Nombre de coups à traiter (sans compter le résultat s'il est à la fin)
            int nombreCoups = LogiqueMouvements.ListeCoupsPgnFr.Count;
            if (nombreCoups != 0)
            {
                // Vérifier si la dernière ligne est un résultat (1-0, 0-1, 1/2-1/2)
                string dernierElement = LogiqueMouvements.ListeCoupsPgnFr[^1].Trim();
                bool dernierElementEstResultat = (dernierElement == "1-0" || dernierElement == "0-1" || dernierElement == "1/2-1/2");
                // Si le dernier élément est un résultat, on ne le traite pas comme un coup
                if (dernierElementEstResultat)
                    nombreCoups--;  // Exclure le résultat du traitement des coups

                // Traitement des coups
                for (int i = 0; i < nombreCoups; i++)
                {
                    string coup = LogiqueMouvements.ListeCoupsPgnFr[i].Trim();

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
            Debug.WriteLine($"Nombre coups de la liste Pgn : {nombreCoups}, Coup valide : {CoupValide}");                           // 01/02  DEBUG
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
        {
            _montre3VariantesUci = !_montre3VariantesUci;       // Affiche ou masque les 3 variantes UCI à chaque clic
            MontreVariantesUci.Text = _montre3VariantesUci ? "Affiche variantes UCI" : "Masque variantes UCI";
            VarianteMoteurUci1.Visible = VarianteMoteurUci2.Visible = VarianteMoteurUci3.Visible = !_montre3VariantesUci;
        }
        private void MontreDonneesUci_Click(object sender, EventArgs e)
        {
            _montreDonneesBrutesUci = !_montreDonneesBrutesUci;       // Affiche ou masque les données UCI à chaque clic
            MontreDonneesUci.Text = _montreDonneesBrutesUci ? "Masque protocole UCI" : "Affiche protocole UCI";
            if (_montreDonneesBrutesUci) donneesBrutesUci.Show();    // On affiche les données brutes UCI
            else donneesBrutesUci.Hide();                       // On masque les données brutes UCI
            donneesBrutesUci.DonneesBrutesVue.ScrollToCaret();      // Pour garder l'affichage dans toute la fenêtre
        }
        private void AideDocumentation_Click(object sender, EventArgs e)
        {
            var fenetreAide = new FenetreAide();
            fenetreAide.ShowDialog();
        }
        private void Apropos_Click(object sender, EventArgs e)
        {   // Option de menu "A propos"
            _ = KryptonMessageBox.Show("      BrunoGUI GenII\n       Version 0.76\n--  Bruno COURTOIS  -- \n Copyright © 2025", "A propos de",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void KryptonApropos_Click(object sender, EventArgs e)
        {   // Bouton "A propos"
            Apropos_Click(sender, e);
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        //  Fermeture Programme
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void EchiquierPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (KryptonMessageBox.Show("Quitter l'application ?", "Confirmer",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true; // Annule la fermeture
                return;
            }
            StopMoteur_Click(sender, e);
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
        {
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
            {
                MessageBox.Show("Aucun coup à afficher.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_indexFenCoupActuel > 0)
            {   // On recule seulement si on n'est pas déjà au tout début
                NumeroDemiCoup--;
                _indexFenCoupActuel--;
                string fen = ListeCoupsFen[_indexFenCoupActuel];
                LogiqueMouvements.MiseenplaceFen(fen);
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen));
                if (_indexFenCoupActuel > 0)
                {
                    string couleurQuiJoue = fen.Split(' ')[1];
                    string texteCouleur = couleurQuiJoue == "w" ? "Coup noir" : "Coup blanc";
                    VarianteMoteurUci1.Text = ($"   [ {texteCouleur} : {LogiqueMouvements.ListeCoupsNal[_indexFenCoupActuel]} ]");
                }
                else
                    VarianteMoteurUci1.Text = "Position initiale";
            }
            else
            {
                KryptonMessageBox.Show("Vous êtes au début de la partie.", "Début de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
            {
                MessageBox.Show("Aucun coup à afficher.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_indexFenCoupActuel < ListeCoupsFen.Count - 1)
            {   // On avance seulement si on n'est pas déjà au dernier coup
                NumeroDemiCoup++;
                _indexFenCoupActuel++;
                string fen = ListeCoupsFen[_indexFenCoupActuel];
                LogiqueMouvements.MiseenplaceFen(fen);
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen));
                string couleurQuiJoue = fen.Split(' ')[1];
                string texteCouleur = couleurQuiJoue == "w" ? "Coup noir" : "Coup blanc";
                VarianteMoteurUci1.Text = ($"   [ {texteCouleur} : {LogiqueMouvements.ListeCoupsNal[_indexFenCoupActuel]} ]");
            }
            else
            {
                if (LogiqueMouvements.EchecetMat)
                {
                    KryptonMessageBox.Show("Il y a échec et mat.", "Terminé : échec et mat",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    KryptonMessageBox.Show("Vous êtes à la fin de la partie.", "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
                return;
            LogiqueMouvements.MiseenplaceFen(FenDepart);
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
            NumeroDemiCoup = 0;
            _indexFenCoupActuel = 0;
            VarianteMoteurUci1.Text = "Début de partie";
        }
        private void BoutonFin_Click(object sender, EventArgs e)
        {
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
                return;
            NumeroDemiCoup = ListeCoupsFen.Count - 1;
            _indexFenCoupActuel = ListeCoupsFen.Count - 1;
            string fen = ListeCoupsFen[_indexFenCoupActuel];
            LogiqueMouvements.MiseenplaceFen(fen);
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen));
            string couleurQuiJoue = fen.Split(' ')[1];
            string texteCouleur = couleurQuiJoue == "w" ? "Coup noir" : "Coup blanc";
            VarianteMoteurUci1.Text = ($"   [ {texteCouleur} : {LogiqueMouvements.ListeCoupsNal[_indexFenCoupActuel]} ]");
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
                    ListeParties = fichierPartiePgn.DecodeFichierPGN(fullPath); // Récupère les parties PGN
                    foreach (string partie in ListeParties)                     // On parcourt la liste de parties, et
                    {                                                           // On met chaque partie au format PartieEchecsPGN dans ListePartiePGN
                        ListePartiesPGN.Add(fichierPartiePgn.DecodePartiePGN(partie));
                    }
                    fichierPartiePgn.NombrePartiesFichier.Text = ListePartiesPGN.Count.ToString()
                        + " partie(s) dans le fichier  " + cheminFichier.Substring(cheminFichier.LastIndexOf('\\') + 1); ;
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
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des parties PGN (sélection/lecture)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        public void ChargerPartieDepuisPgn(PartieEchecsPGN partie)
        {   // --- Charge UNE partie depuis un fichier PGN lorsque'on double-clique ---
            // Note: Méli-mélo entre "PartieCourante" et "PartieEnCours"  !!  A clarifier / résoudre ?!
            PartieCourante.CoupsPartiePGN = "";
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
            PartieCourante.CoupsPartiePGN = partie.CoupsPartiePGN;
            InformationPourJoueur.Text = partie.Tournoi + " / ronde " + partie.Ronde;
            StatusProgramme.Text = $"{partie.White} vs {partie.Black}";
            ScoreMoteur.Text = InformationsPartie.Text = "Résultat : " + partie.Result;
            Debug.WriteLine($"Résultat : {partie.Result}");
            PartieCourante.CoupsPartiePGN = PartieCourante.CoupsPartiePGN.Replace(".", ". "); // Certains fichiers PGN n'ont pas d'espace entre le numéro et le coup, il faut l'ajouter
            string[] coupsPartie = PartieCourante.CoupsPartiePGN.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);  // On decoupe la liste de coups recue

            if (coupsPartie[0] != "1.")     // Tester si CoupsPartie[0] = "1." pour vérifier que c'est bien le début d'une partie ?
                _ = KryptonMessageBox.Show("Problème avec la partie \n Elle ne débute pas avec 1. ", "Problème de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ParcoursPartie(coupsPartie);
        }
        private void ParcoursPartie(string[] suiteCoups)
        {
            bool _couleurTraitBlanc = true;        // Pour commencer avec les Blancs
            groupParcoursPartie.Enabled = _clavierActif = true;
            RetourArriere.Enabled = OrdinateurJoue.Enabled = BoutonBalises.Enabled = SaisiePartieBouton.Enabled = RetourArriere.Enabled = false;
            VarianteMoteurCourante.Text = "";

            for (int indicecoup = 0; indicecoup < suiteCoups.Length - 1; indicecoup++)         // Parcourir tous les coups de la partie
            {
                GestionPartiePgn.DecodeCoupPartie(suiteCoups[indicecoup], _couleurTraitBlanc);
                if (!suiteCoups[indicecoup].Contains('.'))
                {
                    _couleurTraitBlanc = !_couleurTraitBlanc;
                }
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
            Thread.Sleep(500);  // pause 0,5 seconde
            LogiqueMouvements.MiseenplaceFen(ListeCoupsFen[0]);
            NumeroDemiCoup = _indexFenCoupActuel = 0;       // Remise à zéro des indices de parcours
            BoutonBalises.Enabled = OrdinateurJoue.Enabled = false;
        }
        private void EnregistrerPgn_Click(object sender, EventArgs e)
        {
            try
            {
                string contenuPgn = GestionPartiePgn.RetourneContenuPgn(PartieEnCours, "Intl");       // On récupère la partie au format PGN
                // Ecriture du fichier PGN (Partie complète + en-tête)
                {
                    SauvegardeFichier.OverwritePrompt = false;      // Permet d'éviter l'affichage de 2 boites de dialogue si le fichier choisi existe...
                    DialogResult Reponse = SauvegardeFichier.ShowDialog();      // l'utilisateur doit rentrer le nom du fichier PGN
                    if (Reponse == DialogResult.OK)                             // On ne sauvegarde que si l'utilisateur est d'accord
                    {
                        string cheminPgn = SauvegardeFichier.FileName;
                        if (File.Exists(cheminPgn))                         // Si le fichier existe déjà
                        {   // On demande à l'utilisateur s'il veut écraser le fichier ou ajouter la partie
                            DialogResult resultat = KryptonMessageBox.Show("ATTENTION, le fichier " + Path.GetFileName(cheminPgn) + " existe déjà. \nCliquer Oui pour ajouter la partie à la fin." +
                               "\nCliquer Non pour écraser le fichier existant.\nCancel pour afficher le fichier PGN.", "Fichier existant", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
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
                                KryptonMessageBox.Show($"Fichier PGN :\n {contenuPgn}", "Affichage fichier PGN", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                KryptonMessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur méthode Enregistrer PGN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Debug.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }
        private void EnregistrerFen_Click(object sender, EventArgs e)
        {
            {               // Ecriture du fichier FEN (position courante)
                DialogResult Reponse = SauvegardeFen.ShowDialog();      // l'utilisateur doit rentrer le nom du fichier FEN
                if (Reponse == DialogResult.OK)                         // On ne sauvegarde que si l'utilisateur est d'accord
                {
                    string CheminFen = SauvegardeFen.FileName;
                    File.WriteAllText(CheminFen, LogiqueMouvements.RetourneChaineFenActuel());      // Ecriture du fichier au format FEN
                }
            }
        }
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        //  Bibliothèque d'ouvertures
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void JeuMoteurAvecBibliothèque(string chaineFen)
        {   // Lancement moteur avec recherche préalable dans la bibliothèque
            if (_bibliothèqueActive)
            {   // Recherche de la position dans la bibliothèque à partir du FEN
                ulong clePosition = PolyglotBibliothèque.CalculeClefPolyglot(chaineFen);
                string coupChoisiTxt = AfficherCoupsBibliotheque(clePosition);

                if (!string.IsNullOrEmpty(coupChoisiTxt))
                {   // Coup trouvé dans la bibliothèque → on le joue directement
                    string nomBiblio = Path.GetFileName(_bibliotheque);   // Evite d'afficher le chemin complet
                    string caseSource = coupChoisiTxt[..2];
                    string caseDestination = coupChoisiTxt.Substring(2, 2);
                    ExecutionCoup(caseSource, caseDestination);
                    VarianteMoteurUci1.Text = "Coup bibliothèque " + nomBiblio + " exécuté par le moteur -> " + coupChoisiTxt;
                    VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = ".....";
                    Debug.WriteLine($"Coup bibliothèque exécuté (CaseMoveDown) : {coupChoisiTxt}");
                    _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(LogiqueMouvements.RetourneChaineFenActuel()));
                    return; // On sort ici pour ne pas lancer le moteur après
                }
            }

            // Aucun coup dans la bibliothèque ou bibliothèque inactive → on lance le moteur UCI
            LancerReflexion();  // Décompte le temps de réflexion
            MoteurUci.JeuMoteurUci(chaineFen, _dureeReflexionMilliSeconde);
            if (!LogiqueMouvements.EchecetMat)
            {
                InformationPourJoueur.Text = StatusProgramme.Text = _moteurChoisi + " réfléchit ...";
                AnalysePosition.Enabled = OrdinateurJoue.Enabled = RetourArriere.Enabled = false;
                groupParcoursPartie.Enabled = TrackBarTempsReflexion.Enabled = false;
            }
            if (LogiqueMouvements.RenvoieCaseIndex120(_caseDestination) == _dernierCoupMoteurUci)
                _dernierCoupMoteurUci = -1;
        }

        private string AfficherCoupsBibliotheque(ulong clePosition)
        {
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
        {
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
        {
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
        {
            _bibliothèqueAléatoire = !_bibliothèqueAléatoire;
        }
        private void ActiveSon_CheckedChanged(object sender, EventArgs e)
        {   // Bascule pour mettre ou enlever le son
            _emetUnSon = !_emetUnSon;
        }
        private void Promo0_Click(object sender, EventArgs e)
        {   //  Gestion de la promotion de Pion
            PictureBox Promotion = (PictureBox)sender;
            int indexSelect = Convert.ToInt32(Promotion.Name.Substring(5));
            _selectionPromotion = LogiqueMouvements.QuiJoue == LogiqueMouvements.ColorPiece.Blanc ? ListeBlanche[indexSelect] : ListeNoire[indexSelect];
            LogiqueMouvements.PromotionPiece = _selectionPromotion;
        }
        public void PlateauEnable(bool statut)                 // Active ou désactive les cases du plateau de jeu
        {
            if (!(statut && _partieTerminee))
            {
                for (int i = 0; i <= 119; i++)
                    if (PictJeux[i].Visible)
                        PictJeux[i].Enabled = statut;
            }
        }
        private void TourneEchiquier()
        {
            EffaceDernierCoup();
            PictJeux.Reverse();             // On inverse les liste des PictureBox ce qui revient à faire une rotation à 180°
            Plateau.Image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Plateau.Refresh();              // On inverse le plateau
            LogiqueMouvements.DessinPieces();       // On dessine les pièces
            _visuCoteNoir = !_visuCoteNoir;   // On inverse le flag de côté de visualisation
        }
        private void ParametresJoueurHumain(string Couleur, string Affichage)
        {   // Paramètres selon joueur humain noir ou blanc
            _couleurHumain = Couleur;
            if (_humain == false)
                InformationPourJoueur.Text = StatusProgramme.Text = "(Vous avez les " + Couleur + ")";
            else
            {
                InformationPourJoueur.Visible = true;
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux Blancs de jouer";
            }
            InformationPourJoueur.Visible = true;
            InformationPourJoueur.Text = StatusProgramme.Text = Affichage;
        }
        private void RécupèreBibliothèque()
        {
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
        {
            Outils.MiseaZeroListes();
            _numeroLigne = 0;
            NumeroDemiCoup = 0;
            VarianteMoteurUci1.Text = VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = InformationPourJoueur.Text = "...";
            VarianteMoteurCourante.Text = ScoreMoteur.Text = EvaluationUci.Text = "...";
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = true;
        }
        private void NePasDérangerMoteur()
        {
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;
            SaisiePartieBouton.Enabled = AnalysePosition.Enabled = OrdinateurJoue.Enabled = RetourArriere.Enabled = ListeCoupsBouton.Enabled = false;
        }
        private void LeMoteurARépondu()
        {
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = true;
            AnalysePosition.Enabled = OrdinateurJoue.Enabled = RetourArriere.Enabled = ListeCoupsBouton.Enabled = true;
        }

        private void LancerReflexion()
        {
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
        {
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
        {
            timer.Stop(); // stoppe le timer
            _tempsRestant = _dureeReflexionMilliSeconde / 1000; // reset
            labelTempsReflexion.Text = "[" + _tempsRestant.ToString() + "]";
            InformationsPartie.Text = ""; // si tu veux nettoyer le message
        }
    }
}