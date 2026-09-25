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
        public string _dossierRacine, _dossierStockfish;
        private int _indexSource120, _forceMoteurElo, _nombreLignesPV, _tempsRestant;
        private int _dernierCoupMoteurUci;   // dernière case jouée par le moteur UCI
        private int _numeroLigne;       // Indices dans la DataGrid FeuillePartie
        private int _indexCaseSourceDernierMouvement, _indexCaseDestinationDernierMouvement;
        private string _caseSource, _caseDestination, _couleurHumain;
        private string _nomHumain, _joueurElo, _nomMoteur, _moteurElo, _joueurBlanc, _joueurNoir;
        private string _cheminMoteur, _moteurChoisi, _variationMoteur, _meilleureSuite, _scoreCourant, _evaluationCourante;
        private string _bibliotheque = "rodent.bin";
        private bool _clickCaseSource, _visuSymbole, _montreDonneesBrutesUci, _montre3VariantesUci, _analyseEnCours, _montreListeParties, _partieTerminee;
        private bool _humain;           // True pour simuler 2 joueurs humains et False pour jouer contre le moteur UCI
        private bool _visuCoteNoir;     // True quand les Noirs sont en bas de l'écran
        private bool _clavierActif, _emetUnSon, _bibliothèqueAléatoire, _positionChargeeDepuisFen = false;
        private bool _bibliothèqueActive = true;
        private int _indexFenCoupActuel = 0; // Indice du coup affiché
        private int _dureeReflexionMilliSeconde = 5000;
        private Color _couleurCaseSombre, _couleurCaseClaire, _couleurCaseSource, _couleurCaseDestination;
        private LogiqueMouvements.TypePiece _selectionPromotion, _pieceSource;
        private readonly Color _violetCustom = Color.FromArgb(128, 128, 255);  // Rouge = 128, Vert = 128, Bleu = 255

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
        private static readonly System.Windows.Forms.Timer timer1 = new();
        private System.Windows.Forms.Timer timer = timer1;

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
            _joueurElo = parametres.EloHumain;
            _dureeReflexionMilliSeconde = parametres.DureeReflexionSeconde * 1000;
            _nomMoteur = parametres.Moteur;
            _forceMoteurElo = parametres.ForceMoteur;
            _nombreLignesPV = MoteurUci.NombreLignesPV = parametres.NombreLignesPV;
            _bibliotheque = parametres.Bibliotheque;
            LabelJoueurNoir.Text = _cheminMoteur = parametres.Moteur;
            EloNoir.Text = _moteurElo = _forceMoteurElo.ToString();
            LabelJoueurBlanc.Text = _nomHumain;
            EloBlanc.Text = _joueurElo;
            // Debug pour vérifier
            Debug.WriteLine($"Paramètres chargés : Moteur = {_cheminMoteur}, Case sombre = {_couleurCaseSombre.Name}, Case claire = {_couleurCaseClaire.Name}");
            Debug.WriteLine($"Paramètres chargés : Case source = {_couleurCaseSource.Name}, Case destination = {_couleurCaseDestination.Name}");
            Debug.WriteLine($"Paramètres chargés : Biblio = {_bibliotheque}, Force = {_forceMoteurElo}, Nombre PV = {_nombreLignesPV}");
            Debug.WriteLine($"Paramètres chargés : Temps de réflexion = {_dureeReflexionMilliSeconde}");

            OrdinateurJoueNoir = true;
            DateTime Aujourdhui = DateTime.Today;
            _visuCoteNoir = OrdinateurJoueBlanc = false; // On commence avec la vue côté Blanc, l'odinateur a les Noirs
            _montreDonneesBrutesUci = _analyseEnCours = _partieTerminee = _clavierActif = false;
            groupParcoursPartie.Enabled = RetourArriere.Enabled = AnalysePosition.Enabled = _positionChargeeDepuisFen = false;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = ListeCoupsBouton.Enabled = false;
            PartieEnCours.Date = Aujourdhui.ToString("yyyy.MM.dd");
            PartieEnCours.Lieu = "Maison"; PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.WhiteElo = PartieEnCours.BlackElo = "?"; PartieEnCours.Result = "*";
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

            _moteurChoisi = Path.Combine(_dossierRacine, "stockfish", "stockfish.exe");
            _dossierStockfish = Path.Combine(_dossierRacine, "stockfish");

            Debug.WriteLine("Dossier Racine = " + _dossierRacine);
            Debug.WriteLine("Dossier Stockfish = " + _dossierStockfish);
            Debug.WriteLine("Chemin moteurs = " + cheminMoteurs);
            Debug.WriteLine("Chemin Polyglot = " + cheminPolyglot);

            InformationPourJoueur.Text = _dossierRacine;

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
            _humain = false;                    // L'opposant est l'ordinateur, à mettre à true pour simuler 2 joueurs humains

            ActiverMenus(false);    // On désactive les menus après la mise à jour
            VarianteMoteurUci2.Text = "     ---       [INFO] Vérification initiale de mise à jour de Stockfish...      ---";
            _ = Task.Run(async () =>            // MISE A JOUR STOCKFISH SI ELLE EXISTE
           {   // Vérification de la mise à jour de Stockfish, puis lancement du moteur
                
               try
               {   // A. On lance la MAJ et on ATTEND qu'elle finisse
                   Debug.WriteLine("[INFO] Vérification initiale de mise à jour...");
                   // VarianteMoteurUci1.Text = "[INFO] Vérification initiale de mise à jour de Stockfish...";
                   await LancerMiseAJourAsync();
               }
               catch (Exception ex)
               {   // On log juste, on ne bloque pas le démarrage si la MAJ échoue (ex: hors ligne)
                   Debug.WriteLine("[INFO] Pas de mise à jour effectuée : " + ex.Message);
                   // VarianteMoteurUci1.Text = "[INFO] Pas de mise à jour effectuée : ";
               }
                    // B. MAINTENANT, on démarre le moteur. 
                    // Le fichier est libre, remplacé et prêt.
               Debug.WriteLine("chemin Load = " + _moteurChoisi);

               MoteurUci.Start(_moteurChoisi);
               ActiverMenus(true);    // On réactive les menus après la mise à jour
           });
            Debug.WriteLine("Moteur = " + _nomMoteur);
            PartieEnCours.Black = LabelJoueurNoir.Text = _nomMoteur;
            // VarianteMoteurUci2.Text = "[INFO] Fin de la vérification de mise à jour de Stockfish...";
        }

        private void NouvellePartieStockfish_Click(object sender, EventArgs e)
        {   // Nouvelle partie contre Stockfish, avec la possibilité de régler la force du moteur et le temps de réflexion
            _humain = _analyseEnCours = _partieTerminee = _clavierActif = _positionChargeeDepuisFen = false;
            groupParcoursPartie.Enabled = RetourArriere.Enabled = false;
            LogiqueMouvements.PartieEnCoursMat = LogiqueMouvements.PartieEnCoursPat = false;
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
                    OrdinateurJoueNoir = false;
                    _couleurHumain = "Blancs";
                    PartieEnCours.White = LabelJoueurBlanc.Text = _nomMoteur;
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
                    PartieEnCours.Black = LabelJoueurNoir.Text = _nomMoteur;
                    PartieEnCours.BlackElo = EloNoir.Text = _forceMoteurElo.ToString();
                    if (_visuCoteNoir)
                        TourneEchiquier();
                    CommencerPartie();
                    ParametresJoueurHumain("Blancs", "A vous de jouer");            // On demande à l'humain de jouer
                    PlateauEnable(true);                                            // On lui permet de bouger les pièces
                }
            }
        }
        private void CommencerPartie()      // POINT D'ENTREE POUR TOUTES LES NOUVELLES PARTIES
        {   // Début d'une nouvelle partie
            _analyseEnCours = _partieTerminee = _clavierActif = _positionChargeeDepuisFen = false;
            groupParcoursPartie.Enabled = RetourArriere.Enabled = false;
            _dernierCoupMoteurUci = -1;
            _clickCaseSource = _visuSymbole = true;
            PartieEnCours.CoupsPartiePGN = PartieEnCours.Result = PartieEnCours.CompteDePLy = PartieEnCours.Ronde = "";
            PartieEnCours.Tournoi = "Entrainement";
            PartieEnCours.Lieu = "Maison";
            NumeroDemiCoup = 0;
            MiseaZeroAffichages();
            MiseaZéroTimer();
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
                            string chaineFen = LogiqueMouvements.RetourneChaineFenActuel(); // UCI : remplacer le FEN par liste de coups ?!
                            if (LogiqueMouvements.CoupValide)
                            {   // envoi de la Position Fen au moteur UCI
                                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(chaineFen));
                                if (_humain == false)
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
            switch (ligne.Commande)     // identifier le premier mot
            {
                case "bestmove":        // **** le moteur UCI propose le meilleur coup ! ****
                    if (ligne.AucunCoupLegal)
                    {   // Cas particulier : le moteur retourne "bestmove (none)" ou "bestmove 0000" => partie terminée (mat ou pat)
                        VarianteMoteurCourante.Text = "Aucun coup légal (mat ou pat)";
                        break;
                    }
                    VarianteMoteurCourante.Text = "Coup joué : " + Outils.VarianteUciVersPgn(ligne.MeilleurCoup, NumeroDemiCoup, false) +
                        (ligne.CoupConseil != null ? "   (Conseil : " + Outils.VarianteUciVersPgn(ligne.MeilleurCoup + " " + ligne.CoupConseil, NumeroDemiCoup, true) + ")" : "");  // Le conseil (ponder) se joue après le coup du moteur
                    break;
                case "id":
                    if (ligne.NomMoteur != null)
                    {   // Récupération du nom du moteur (limité à 20 caractères pour l'affichage)
                        _nomMoteur = ligne.NomMoteur[..Math.Min(20, ligne.NomMoteur.Length)];
                        LabelJoueurNoir.Text = _nomMoteur;
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
        {   // Affiche le score et la variante d'une ligne "info" du moteur
            if (ligne.DansBibliotheque)
                VarianteMoteurUci1.Text = "    Le moteur est dans sa bibliothèque d'ouvertures";

            // Un moteur sans MultiPV (Sargon, ...) n'envoie pas de numéro de variante : sa variante unique est la meilleure
            bool meilleureVariante = (ligne.NumeroVariante ?? 1) == 1;
            if (ligne.ScoreCentipions is int centipions)
            {
                _scoreCourant = (centipions / 100m).ToString("N2", CultureInfo.InvariantCulture);
                if (meilleureVariante)
                {   // On affiche seulement le score de la meilleure variante
                    ScoreMoteur.Text = "Score : " + _scoreCourant;
                    AfficheEvaluation(_scoreCourant);
                }
            }
            if (ligne.MatEn is int matEn)
            {   // Le signe est conservé : négatif si le camp au trait se fait mater (voir AfficheEvalSymbole)
                _scoreCourant = (matEn < 0 ? "-M" : "M") + Math.Abs(matEn);
                if (meilleureVariante)
                    InformationPourJoueur.Text = ScoreMoteur.Text = "MAT en " + Math.Abs(matEn);
            }
            if (ligne.Variante == null)
                return;

            // Affichage de la variation principale
            _variationMoteur = ligne.Variante;
            if (_variationMoteur.Length > 60)
                _variationMoteur = _variationMoteur[..60];     // On limite la longueur de la variation, pour rester dans le label
            _variationMoteur = Outils.VarianteUciVersPgn(_variationMoteur, NumeroDemiCoup, false);  // Elle est en Uci, il la faut en PGN Fr ...
            string varianteExaminee = string.Join(" ", _variationMoteur.Split(' ').Take(3));
            string texteVariante = AfficheEvalSymbole(_scoreCourant) + " (" + varianteExaminee + ") █[ " + _scoreCourant + " ]█  " + "[ " + _variationMoteur + " ]";
            if (ligne.NumeroVariante is int numeroVariante)
            {   // Une zone d'affichage par variante (VarianteMoteurUci1, 2, 3) ; les variantes au-delà ne sont pas affichées
                if (Controls.Find("VarianteMoteurUci" + numeroVariante, true).FirstOrDefault() is RichTextBox zoneVariante)
                    zoneVariante.Text = " " + texteVariante;
            }
            else
            {   // Pour ceux qui n'ont qu'une variante principale (Sargon, ...) : on n'utilise que la zone VarianteMoteurUci1
                VarianteMoteurUci1.Text = texteVariante;
                VarianteMoteurUci2.Text = "... " + _moteurChoisi + " n'affiche qu'une variante ..."; VarianteMoteurUci3.Text = "...";
            }
        }

        private string AfficheEvaluation(string _scoreCourant)
        {   /*
            La règle d'or de l'UCI : Le point de vue du moteur
            Le signe du score dans le protocole UCI est toujours du point de vue du camp qui a le trait (celui qui doit jouer).
            Score positif (+) : Le moteur estime qu'il est en avantage.
            Score négatif (-) : Le moteur estime qu'il est en désavantage.
            C'est une convention relative au camp au trait, et non absolue (ce n'est pas "toujours positif pour les blancs").
            */
            string resultat;
            _scoreCourant = _scoreCourant?.Trim();
            // 1. Déterminer si le moteur parle au nom des blancs
            bool estTourBlanc = (QuiJoue == ColorPiece.Blanc);
            if (string.IsNullOrEmpty(_scoreCourant))
            {
                EvaluationUci.Text = _evaluationCourante = "Éval indisponible";
                return _evaluationCourante;
            }
            // 2. CAS DU MAT (ex: "M3", "-M2", "mate 5", "mate -5")
            if (_scoreCourant.Contains('M', StringComparison.OrdinalIgnoreCase))
            {   // On regarde si le signe '-' est présent dans la chaîne
                bool estNegatif = _scoreCourant.Contains('-');
                // Logique : 
                // Si c'est positif (+), le camp qui joue (QuiJoue) gagne.
                // Si c'est négatif (-), le camp qui joue (QuiJoue) perd.
                bool gainBlanc = (estTourBlanc && !estNegatif) || (!estTourBlanc && estNegatif);
                resultat = gainBlanc ? "Gain Blanc" : "Gain Noir";
                EvaluationUci.Text = _evaluationCourante = resultat;
            }
            else
            {   // 3. CAS DU SCORE CP
                if (!decimal.TryParse(_scoreCourant, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal score))
                {
                    EvaluationUci.Text = _evaluationCourante = "Éval indisponible";
                    return _evaluationCourante;
                }
                // Conversion en score absolu (du point de vue des Blancs)
                // Si c'est aux noirs, on inverse pour que '+' = Blanc et '-' = Noir
                decimal scoreAbsolu = estTourBlanc ? score : -score;
                resultat = scoreAbsolu switch
                {
                    >= 2.5m => "Gain Blanc (+-)",
                    > 0.5m => "Avantage Blanc (±)",
                    <= -2.5m => "Gain Noir (-+)",
                    < -0.5m => "Avantage Noir (∓)",
                    _ => "Égal (=)"
                };
            }
            EvaluationUci.Text = _evaluationCourante = resultat;
            return resultat;
        }
        private static string AfficheEvalSymbole(string scoreCourant)
        {
            scoreCourant = scoreCourant?.Trim();
            bool estTourBlanc = (QuiJoue == ColorPiece.Blanc);
            if (string.IsNullOrEmpty(scoreCourant))
                return "?";
            if (scoreCourant.Contains('M', StringComparison.OrdinalIgnoreCase))
            {   // CAS MAT
                bool estNegatif = scoreCourant.Contains('-');
                bool gainBlanc = (estTourBlanc && !estNegatif) || (!estTourBlanc && estNegatif);
                return gainBlanc ? "#+" : "#-"; // ou ce que l'on veut afficher
            }
                // CAS CP
            if (!decimal.TryParse(scoreCourant, NumberStyles.Any,
                CultureInfo.InvariantCulture, out decimal score))
                return "?";
            decimal scoreAbsolu = estTourBlanc ? score : -score;
            return scoreAbsolu switch
            {
                >= 2.5m => "+-",
                > 0.5m => "±",
                <= -2.5m => "-+",
                < -0.5m => "∓",
                _ => "="
            };
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
                TrackBarTempsReflexion.Enabled = true;
                if (_emetUnSon)
                {   // Son pour dire que le coup est joué
                    SoundPlayer player = new(@"C:\Windows\Media\Windows Notify.wav");
                    player.Play();
                }
                if (!_analyseEnCours)
                {   // Si ce n'est pas une analyse ...
                    StatusProgramme.Text = InformationPourJoueur.Text = "A vous de jouer";
                    _caseSource = MoteurUci.CoupAuFormatUci[..2];    // CoupAuFormatUci contient le "best move" sous la forme e2e4
                    _caseDestination = MoteurUci.CoupAuFormatUci.Substring(2, 2);

                    // *******Traitement promotion *********
                    if (MoteurUci.CoupAuFormatUci.Length >= 5)
                    {   // Gestion de la promotion : 5ème caractère de l'UCI (index 4)
                        char promo = char.ToLower(MoteurUci.CoupAuFormatUci[4]);
                        char rangéeDestination = _caseDestination[1];           // '1'..'8'
                        bool estPromotionBlanche = rangéeDestination == '8';    // promotion en 8 => blanc
                        LogiqueMouvements.PromotionPiece = promo switch
                        {
                            'q' => estPromotionBlanche ? TypePiece.ReineBlanche : TypePiece.ReineNoire,
                            'r' => estPromotionBlanche ? TypePiece.TourBlanche : TypePiece.TourNoire,
                            'b' => estPromotionBlanche ? TypePiece.FouBlanc : TypePiece.FouNoir,
                            'n' => estPromotionBlanche ? TypePiece.CavalierBlanc : TypePiece.CavalierNoir,
                            _ => TypePiece.Vide,
                        };
                        LogiqueMouvements.BloquerChoixPromo = true;
                    }
                    else
                    {   // Pas de promotion dans le UCI : assurer une valeur neutre
                        LogiqueMouvements.PromotionPiece = TypePiece.Vide;    //  ???
                    }

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
                                            + " par " + _nomMoteur, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    RetourArriere.Enabled = AnalysePosition.Enabled = groupParcoursPartie.Enabled = true;      // On réautorise si le moteur a fini de réfléchir
                    _analyseEnCours = false;
                }
            }
        }

        private void AfficheCoupBlanc(string coupBlanc)
        {   // Affiche le coup joué par les blancs
            // Comme c'est le coup Blanc, il faut afficher le numéro du coup
            if (LogiqueMouvements.EchecetMat == false)
            {   // ******      Traitement du numéro de demi-coup :     ******
                if (_positionChargeeDepuisFen)
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

        private void AfficheCoupNoir(string coupNoir)
        {   //  Affiche le coup joué par les noirs
            // Comme c'est le coup Noir, on n'a pas besoin d'afficher le numéro du coup
            if (LogiqueMouvements.EchecetMat == false)
            {   // ******      Traitement du numéro de demi-coup :     ******
                if (_positionChargeeDepuisFen)
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
        {   // Affiche la couleur du joueur humain courant, et active les PictureBox si c'est au tour du joueur humain
            if (_humain)
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux " + Couleur + " de jouer";
            else
                PlateauEnable(Couleur == _couleurHumain); // active les Picturebox si c'est au tour du joueur humain
        }

        private void AfficheEchecEtMat(string couleurRoiMat)   // Affiche l'échec et mat du roi de la couleur en paramètre
        {   // Affiche l'échec et mat du roi de la couleur en paramètre, et gère la fin de partie
            int indexCouleur = couleurRoiMat == "Blanc" ? 2 : 1;
            LogiqueMouvements.ListeCoupsPgnIntl[^1] = (LogiqueMouvements.ListeCoupsPgnIntl[^1].ToString()).Replace("+", "#");
            LogiqueMouvements.ListeCoupsPgnFr[^1] = (LogiqueMouvements.ListeCoupsPgnFr[^1].ToString()).Replace("+", "#");
            LogiqueMouvements.ListeCoupsNal[^1] = (LogiqueMouvements.ListeCoupsNal[^1].ToString()).Replace("+", "#");
            LogiqueMouvements.PartieEnCoursMat = true;
            string coupMat = (LogiqueMouvements.ListeCoupsPgnFr[LogiqueMouvements.ListeCoupsPgnIntl.Count - 1].ToString()).Replace("+", "#");
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
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;     // Le résultat est déjà défini
            RetourArriere.Visible = false;
            InformationPourJoueur.Text = VarianteMoteurCourante.Text = "Le Roi " + couleurRoiMat + " est échec et mat";
            StatusProgramme.Text = "Partie terminée";
            // AnalysePosition.Enabled = false;
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
                // AnalysePosition.Enabled = false;
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
            while (_selectionPromotion == LogiqueMouvements.TypePiece.Vide)
                Application.DoEvents();
            GroupPromo.Visible = false;
        }
        private void EffaceDernierCoup()
        {   // Efface les couleurs de la case source et destination du dernier coup joué
            PictJeux[_indexCaseSourceDernierMouvement].BackColor = Outils.EstCaseClaire(_indexCaseSourceDernierMouvement) ? _couleurCaseClaire : _couleurCaseSombre;
            PictJeux[_indexCaseDestinationDernierMouvement].BackColor = Outils.EstCaseClaire(_indexCaseDestinationDernierMouvement) ? _couleurCaseClaire : _couleurCaseSombre;
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
        private void PartieNulle_Repetition()
        {   // Si la position courante a déjà été atteinte 3 fois, c'est la règle de la triple répétition : partie nulle
            GestionResultat("1/2-1/2", "Nulle par répétition");
        }
        private void GestionResultat(string resultat, string vainqueur)
        {   // Fin de partie : on affiche le résultat et le vainqueur, on désactive les boutons de gain/nulle,
            // on empêche de bouger les pièces, on affiche le résultat dans les données de la partie
            StopMoteur_Click(null, EventArgs.Empty);    // Au cas où le moteur tourne encore ?!
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
        {   // L'humain joue les blancs, l'ordinateur les noirs
            _couleurHumain = "Blancs";
            QuiJoue = ColorPiece.Blanc;
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomHumain;
            PartieEnCours.Black = LabelJoueurNoir.Text = _nomMoteur;
            PartieEnCours.WhiteElo = EloBlanc.Text = _joueurElo;
            PartieEnCours.BlackElo = EloNoir.Text = _moteurElo;
            OrdinateurJoueNoir = true;
            _humain = OrdinateurJoueBlanc = _analyseEnCours = _partieTerminee = AnalysePosition.Enabled = groupParcoursPartie.Enabled = false;
            LogiqueMouvements.PartieEnCoursMat = LogiqueMouvements.PartieEnCoursPat = _positionChargeeDepuisFen = false;
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous aurez les Blancs contre " + _nomMoteur + ". " + "\nToute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = KryptonMessageBox.Show(confirmation, "Le joueur a les Blancs, l'ordinateur les Noirs ", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                StatusProgramme.Text = ScoreMoteur.Text = EvaluationUci.Text = VarianteMoteurCourante.Text = "";    // On efface les données de la partie précédente
                CommencerPartie();
                if (_visuCoteNoir)
                    TourneEchiquier();
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
                ParametresJoueurHumain("Blancs", "A vous de jouer");            // On demande à l'humain de jouer
                PlateauEnable(true);                                            // On lui permet de bouger les pièces
            }
        }
        private void OrdinateurHumain_Click(object sender, EventArgs e)
        {   // L'ordinateur joue les blancs, l'humain les noirs
            _couleurHumain = "Noirs";
            QuiJoue = ColorPiece.Blanc;
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomMoteur;
            PartieEnCours.Black = LabelJoueurNoir.Text = _nomHumain;
            PartieEnCours.WhiteElo = EloBlanc.Text = _moteurElo;
            PartieEnCours.BlackElo = EloNoir.Text = _joueurElo;
            OrdinateurJoueBlanc = true;
            _humain = OrdinateurJoueNoir = _analyseEnCours = _partieTerminee = AnalysePosition.Enabled = groupParcoursPartie.Enabled = false;
            LogiqueMouvements.PartieEnCoursMat = LogiqueMouvements.PartieEnCoursPat = _positionChargeeDepuisFen = false;
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous aurez les Noirs contre " + _nomMoteur + ". " + "\nToute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
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
        {   // 2 joueurs humains s'affrontent, pas de moteur UCI
            PartieEnCours.White = LabelJoueurBlanc.Text = _nomHumain;
            PartieEnCours.Black = "Adversaire";
            LogiqueMouvements.PartieEnCoursMat = LogiqueMouvements.PartieEnCoursPat = _positionChargeeDepuisFen = false;
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
                MoteurUci.DefinitMultiPV(MoteurUci.NombreLignesPV);
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
            EloNoir.Text = _moteurElo = "+- 3000";
            Directory.SetCurrentDirectory(Chemins.MoteursUCI + @"\Rodent_IV");
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\Rodent_IV", "rodent-iv-x64.exe");
            Debug.WriteLine("Chemin Rodent IV = " + _cheminMoteur);
            DémarrageMoteur();
        }
        private void Sargon1_1978_Click(object sender, EventArgs e)
        {   // https://echecs-et-informatique.franceserv.com/sargon-1978.html
            _moteurChoisi = "Sargon I 1978";
            EloNoir.Text = _moteurElo = "1678";
            Directory.SetCurrentDirectory(Chemins.MoteursUCI + @"\sargon1978");
            _cheminMoteur = Path.Combine(Chemins.MoteursUCI + @"\sargon1978", "sargon1978_1_01b.exe");
            Debug.WriteLine("Chemin sargon I 1978 = " + _cheminMoteur);
            DémarrageMoteur();
            MoteurUci.SpecialeSargon();         // Sinon Sargon  mouline sans fin !!!!!
        }
        public void DémarreStockfish()
        {   //  https://stockfishchess.org/
            // _moteurChoisi = "Stockfish 17 ";
            EloNoir.Text = _moteurElo = "+- 3000";
            Directory.SetCurrentDirectory(Application.StartupPath);
            _cheminMoteur = Path.Combine(_dossierRacine, "stockfish", "stockfish.exe");
            Debug.WriteLine("Chemin Stockfish = " + _cheminMoteur);
            DémarrageMoteur();
        }
        private void DémarrageMoteur()
        {   // Arrête le moteur UCI s'il est déjà en cours d'exécution, pour éviter les conflits
            MoteurUci.Quitte();
            MoteurUci.Start(_cheminMoteur); // on démarre le nouveau moteur Uci
            _moteurChoisi = Path.GetFileNameWithoutExtension(_cheminMoteur);
            Debug.WriteLine("Moteur = " + _moteurChoisi);
            if (OrdinateurJoueBlanc)
                LabelJoueurBlanc.Text = _moteurChoisi;
            if (OrdinateurJoueNoir)
                LabelJoueurNoir.Text = _moteurChoisi;
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
        {   // Permet d'analyser la position courante, même si la partie n'est pas terminée
            InformationPourJoueur.Text = StatusProgramme.Text = "Analyse de la position ...";
            _analyseEnCours = true;
            if (NumeroDemiCoup <= LogiqueMouvements.ListeCoupsFen.Count - 1)    // on empêche d'analyser au-delà de la partie ...
            {   // Si la partie se termine par MAT ou PAT, inutile de lancer l'analyse sur le dernier coup
                bool dernierCoup = NumeroDemiCoup == LogiqueMouvements.ListeCoupsFen.Count - 1;
                if (dernierCoup && (LogiqueMouvements.PartieEnCoursMat || LogiqueMouvements.PartieEnCoursPat))
                {
                    MiseaZeroVariantes();
                    InformationPourJoueur.Text = "Analyse inutile ...";
                    InformationsPartie.Text = "La partie est déjà terminée ...";
                    if (LogiqueMouvements.PartieEnCoursMat)
                        KryptonMessageBox.Show("La partie est terminée par un mat.", "Analyse inutile");
                    else
                        KryptonMessageBox.Show("La partie est terminée par un pat.", "Analyse inutile");
                    return;
                }
                string Fenaenvoyer = LogiqueMouvements.ListeCoupsFen[NumeroDemiCoup];   // Récupère le FEN (position)
                RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = false; // Il faut empêcher le retour si le moteur réfléchit
                BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = groupParcoursPartie.Enabled = false;
                LancerReflexion();  // Décompte le temps de réflexion
                MoteurUci.JeuMoteurUci(Fenaenvoyer, _dureeReflexionMilliSeconde);     // On fait jouer le moteur, avec le temps de réflexion choisi
            }
        }
        private void InverseEchiquier_Click(object sender, EventArgs e)
        {   // Permet d'inverser la vue de l'échiquier (côté Blanc ou côté Noir)
            PlateauEnable(true);
            TourneEchiquier();
        }
        private void OrdinateurJoue_Click(object sender, EventArgs e)
        {   // Permet de faire jouer l'ordinateur UCI, sans que ce soit son tour (pour tester une position par exemple)
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
            JeuMoteurAvecBibliothèque(Fenaenvoyer);
            PlateauEnable(true);
            _clickCaseSource = _visuSymbole = true;    // L'ordinateur ayant joué, c'est indispensable !
        }
        private void RetourArriere_Click(object sender, EventArgs e)
        {   // Permet de revenir en arrière d'un demi-coup (coup des blancs ou des noirs)
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
                }
                else
                {   // MiseenplaceFen rétablit aussi les droits de roque, la case en passant et le compteur des 50 coups
                    string Fenaenvoyer = LogiqueMouvements.ListeCoupsFen[^1];    // Récupère le dernier FEN (position)
                    LogiqueMouvements.MiseenplaceFen(Fenaenvoyer);              // et on l'affiche sur l'échiquier
                    _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(Fenaenvoyer));
                }
                InformationPourJoueur.Text = StatusProgramme.Text = "Trait aux " + QuiJoue + "s";
                InformationsPartie.Text = OrdinateurJoueBlanc ? "L'ordinateur joue les Blancs" :
                          OrdinateurJoueNoir ? "L'ordinateur joue les Noirs" :
                          "L'ordinateur ne joue pas cette partie";
                OrdinateurJoue.Enabled = AnalysePosition.Enabled = true;
            }
        }
        private void ListeCoupsBouton_Click(object sender, EventArgs e)
        {   // Affiche la liste des coups joués dans une fenêtre dédiée
            string numeroCoup = "";
            string blancs = "";
            string noirs;
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
            _ = KryptonMessageBox.Show("      BrunoGUI GenII\n       Version 1.02\n--  Bruno COURTOIS  -- " +
                                                                    "\n Copyright © 2026", "A propos de",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void KryptonApropos_Click(object sender, EventArgs e)
        {   // Bouton "A propos"
            Apropos_Click(sender, e);
        }
        public async Task<bool> LancerMiseAJourAsync()
        {   // Appelle la classe de mise à jour de Stockfish, qui vérifie la version actuelle
            // et télécharge la nouvelle version si besoin (retourne false si Stockfish était déjà à jour)
            MiseAJourStockfish maj = new(_moteurChoisi);
            bool misAJour = await maj.ExecuterMiseAJour();
            Debug.WriteLine(misAJour ? "Moteur mis à jour" : "Moteur déjà à jour");
            return misAJour;
        }
        private async void BtnMiseAJour_Click(object sender, EventArgs e)
        {   // 1. On prépare l'UI
            BtnMiseAJour.Enabled = false;
            Cursor = Cursors.WaitCursor;
            VarianteMoteurUci2.Text = "Vérification de la version courante de Stockfish...";
            try
            {   // 2. On appelle la méthode de mise à jour
                bool misAJour = await LancerMiseAJourAsync();
                VarianteMoteurUci2.Text = "Stockfish est à jour !";
                KryptonMessageBox.Show(misAJour ? "Mise à jour réussie." : "Vous avez déjà la dernière version.", "Stockfish", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {   // On gère les messages (ex: "Déjà à jour" ou "Pas de connexion")
                VarianteMoteurUci2.Text = "Prêt";
                KryptonMessageBox.Show(ex.Message, "Mise à jour", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        {   // On recule d'un demi-coup (si possible), en affichant le FEN correspondant
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
            {
                KryptonMessageBox.Show("Aucun coup à afficher.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_indexFenCoupActuel > 0)
            {   // On recule seulement si on n'est pas déjà au tout début
                NumeroDemiCoup--;
                _indexFenCoupActuel--;  // On recule les index
                string fen = ListeCoupsFen[_indexFenCoupActuel];
                LogiqueMouvements.MiseenplaceFen(fen);
                _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen));
                if (_indexFenCoupActuel > 0)
                {   // 1. Analyse du FEN
                    string[] partiesFen = fen.Split(' ');
                    string couleurQuiJoue = partiesFen[1];
                    string numCoupFen = partiesFen[5];
                    // 2. Nettoyage du coup (pour enlever le "x." si présent)
                    string coupBrut = LogiqueMouvements.ListeCoupsNal[_indexFenCoupActuel];
                    string coupNettoye = coupBrut.Contains('.')
                                         ? coupBrut.Split('.').Last().Trim()
                                         : coupBrut;
                    // 3. Formatage selon le trait
                    string texteAffiche;
                    if (couleurQuiJoue == "w")
                    {   // Au tour des blancs, donc on affiche le dernier coup NOIR
                        texteAffiche = $"Coup noir : {numCoupFen}... {coupNettoye}";
                        InformationPourJoueur.Text = "Trait aux Blancs";
                    }
                    else
                    {   // Au tour des noirs, donc on affiche le dernier coup BLANC
                        texteAffiche = $"Coup blanc : {numCoupFen}. {coupNettoye}";
                        InformationPourJoueur.Text = "Trait aux Noirs";
                    }
                    VarianteMoteurUci1.Text = $"   [ {texteAffiche} ]";
                }
                else
                {
                    VarianteMoteurUci1.Text = "   [ Position initiale ]";
                }
                MiseaZeroParcours();
            }
            else
            {
                KryptonMessageBox.Show("Vous êtes au début de la partie.", "Début de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            AnalysePosition.Enabled = true;    // On réactive le bouton d'analyse de la position    
        }
        private void BoutonSuivant_Click(object sender, EventArgs e)
        {   // On avance d'un coup dans la partie
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
            {
                KryptonMessageBox.Show("Aucun coup à afficher.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_indexFenCoupActuel < ListeCoupsFen.Count - 1)
            {   // On avance seulement si on n'est pas déjà au dernier coup
                NumeroDemiCoup++;
                _indexFenCoupActuel++;
                string fen = ListeCoupsFen[_indexFenCoupActuel];
                LogiqueMouvements.MiseenplaceFen(fen);
                string[] partiesFen = fen.Split(' ');
                string couleurQuiJoue = partiesFen[1];
                string numCoupFen = partiesFen[5];
                // --- NETTOYAGE DU COUP ---
                // On récupère "x. Bc1xf4"
                string coupBrut = LogiqueMouvements.ListeCoupsNal[_indexFenCoupActuel];
                // On ne garde que ce qui est APRÈS le point. Si pas de point, on garde tout.
                string coupNettoye = coupBrut.Contains('.')
                                     ? coupBrut.Split('.').Last().Trim()
                                     : coupBrut;
                string texteFinal;
                if (couleurQuiJoue == "w")
                {   // On vient de jouer NOIR -> format "1... e5"
                    texteFinal = $"Coup noir : {numCoupFen}... {coupNettoye}";
                    InformationPourJoueur.Text = "Trait aux Blancs";
                }
                else
                {   // On vient de jouer BLANC -> format "1. e4"
                    texteFinal = $"Coup blanc : {numCoupFen}. {coupNettoye}";
                    InformationPourJoueur.Text = "Trait aux Noirs";
                }
                VarianteMoteurUci1.Text = $"   [ {texteFinal} ]";
                MiseaZeroParcours();
            }
            else
            {
                if (LogiqueMouvements.EchecetMat)
                {
                    KryptonMessageBox.Show("Il y a échec et mat.", "Terminé : échec et mat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    KryptonMessageBox.Show("Vous êtes à la fin de la partie.", "Fin de partie", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void BoutonDebut_Click(object sender, EventArgs e)
        {   // On va au premier coup joué (premier FEN de la liste)
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
                return;
            LogiqueMouvements.MiseenplaceFen(LogiqueMouvements.ListeCoupsFen[0]);
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(FenDepart));
            NumeroDemiCoup = 0;
            _indexFenCoupActuel = 0;
            VarianteMoteurUci1.Text = "Début de partie";
            MiseaZeroParcours();
        }
        private void BoutonFin_Click(object sender, EventArgs e)
        {   // On va au dernier coup joué (dernier FEN de la liste)
            if (ListeCoupsFen == null || ListeCoupsFen.Count == 0)
                return;
            NumeroDemiCoup = ListeCoupsFen.Count - 1;
            _indexFenCoupActuel = ListeCoupsFen.Count - 1;
            string fen = ListeCoupsFen[_indexFenCoupActuel];
            LogiqueMouvements.MiseenplaceFen(fen);
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(fen));
            string couleurQuiJoue = fen.Split(' ')[1];
            string texteCouleur = couleurQuiJoue == "w" ? "Coup noir" : "Coup blanc";
            InformationPourJoueur.Text = couleurQuiJoue == "w" ? "Trait aux Blancs" : "Trait aux Noirs";
            VarianteMoteurUci1.Text = ($"   [ {texteCouleur} : {LogiqueMouvements.ListeCoupsNal[_indexFenCoupActuel]} ]");
            MiseaZeroParcours();
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
            _positionChargeeDepuisFen = false;
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
            string contenuFen = "";
            _positionChargeeDepuisFen = true;
            ListeParties.Clear();    // On vide la liste des parties 
            ListePartiesPGN.Clear(); // On vide la liste des parties PGN
            ListeCoupsFen.Clear();     // On vide la liste des coups FEN
            ListeCoupsNal.Clear();     // On vide la liste des coups NAL
            ListeCoupsPgnFr.Clear();   // On vide la liste des coups PGN français
            ListeCoupsPgnIntl.Clear();  // On vide la liste des coups PGN international
            ListeCoupsUci.Clear();     // On vide la liste des coups UCI
            InitialisationEchiquier();    // On réinitialise l'échiquier
            AnalysePosition.Enabled = true;    // On veut anlyser la position chargée ...
            if (ChargerPositionFen.ShowDialog() == DialogResult.OK)
            {
                string cheminFichier = ChargerPositionFen.FileName;
                try
                {   // Vérifie et obtient le chemin complet
                    string fullPath = Path.GetFullPath(cheminFichier);
                    Debug.WriteLine("Chemin complet du fichier FEN : " + fullPath);

                    // Lire le contenu du fichier et l'afficher dans la console
                    contenuFen = File.ReadAllText(fullPath);
                    Debug.WriteLine("Contenu du fichier FEN : " + contenuFen);
                    VarianteMoteurUci1.Text = "Fen chargé : " + contenuFen;
                    LogiqueMouvements.MiseenplaceFen(contenuFen);  // Affiche la position FEN sur l'échiquier
                    ListeCoupsFen.Add(contenuFen);  // On ajoute le FEN à la liste des coups FEN 
                    RetourArriere.Enabled = false;    // On ne peut PAS faire un retour arrière sur la position chargée
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Chargement FEN : Erreur lors de la lecture du fichier : " + ex.Message);
                }
            }
            string[] ChampsFen = contenuFen.Split(' ');           // On récupère les 6 champs du FEN dans un tableau
                                                                  // Sécurité
            if (ChampsFen.Length >= 6)
            {   // Couleur au trait
                QuiJoue = (ChampsFen[1] == "w") ? ColorPiece.Blanc : ColorPiece.Noir;
                _couleurHumain = QuiJoue.ToString();    // Par défaut : c'est l'humain qui joue

                _dernierCoupMoteurUci = -1;
                _clickCaseSource = _visuSymbole = true;
                PartieEnCours.CoupsPartiePGN = PartieEnCours.Result = PartieEnCours.CompteDePLy = PartieEnCours.Ronde = "";
                PartieEnCours.Tournoi = "Entrainement";
                PartieEnCours.Lieu = "Maison";
                // NumeroDemiCoup = 0;
                // MiseaZeroAffichages();
                _couleurHumain = "Blancs";      // Douteux, car on ne sait pas encore qui est humain ou ordinateur,
                                                // mais on met une valeur par défaut pour éviter les bugs d'affichage (ex: "Trait aux Blancs" au lieu de "Trait aux ...")
                // QuiJoue = ColorPiece.Blanc;     // Douteux, caar défini plus haut à partir du FEN, donc potentiellemnt incorrect ici !
                PartieEnCours.White = LabelJoueurBlanc.Text = "";
                PartieEnCours.Black = LabelJoueurNoir.Text = "";
                PartieEnCours.WhiteElo = EloBlanc.Text = "";
                PartieEnCours.BlackElo = EloNoir.Text = "";
                OrdinateurJoueNoir = true;
                _humain = OrdinateurJoueBlanc = _analyseEnCours = _partieTerminee = AnalysePosition.Enabled = groupParcoursPartie.Enabled = false;

                LogiqueMouvements.PartieEnCoursMat = LogiqueMouvements.PartieEnCoursPat = false;
                InformationPourJoueur.Text = "Trait aux " + (QuiJoue == ColorPiece.Blanc ? "Blancs" : "Noirs");
                // Droits de roque
                PetitRoqueBlancPossible = ChampsFen[2].Contains('K');
                GrandRoqueBlancPossible = ChampsFen[2].Contains('Q');
                PetitRoqueNoirPossible = ChampsFen[2].Contains('k');
                GrandRoqueNoirPossible = ChampsFen[2].Contains('q');
                // Case en passant
                if (ChampsFen[3] == "-")
                    IndexCaseEnPassant = 0;      // ou -1 selon ta convention
                else
                    IndexCaseEnPassant = RenvoieCaseIndex120(ChampsFen[3]);
                // Compteur des demi-coups (règle des 50 coups)
                SansPrise = int.Parse(ChampsFen[4]);
                // Numéro du coup complet                // Exemple : "1", "23", etc.
                NombreCoupsJoues = float.Parse(ChampsFen[5], System.Globalization.CultureInfo.InvariantCulture);
                // Calcul du numéro de demi-coup (index interne à partir de 0)
                NumeroDemiCoup = ((int)NombreCoupsJoues - 1) * 2;
                // Si ce sont les Noirs au trait, on ajoute 1 demi-coup
                if (QuiJoue == ColorPiece.Noir)
                    NumeroDemiCoup++;
                
                PromotionPiece = TypePiece.Vide;
                PlateauEnable(true);   // On active le plateau pour pouvoir jouer à partir de la position chargée
            }
            _ = AfficherCoupsBibliotheque(PolyglotBibliothèque.CalculeClefPolyglot(contenuFen));
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
                               "Fichier existant", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
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
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            ParcoursPartie(coupsPartie);
        }
        private void ParcoursPartie(string[] suiteCoups)
        {   // Parcourt la partie coup par coup pour l'afficher sur l'échiquier et afficher le résultat à la fin
            bool _couleurTraitBlanc = true;        // Pour commencer avec les Blancs
            groupParcoursPartie.Enabled = _clavierActif = true;
            RetourArriere.Enabled = OrdinateurJoue.Enabled = BoutonBalises.Enabled = SaisiePartieBouton.Enabled = RetourArriere.Enabled = false;
            VarianteMoteurCourante.Text = "";
            PartieEnCoursMat = PartieEnCoursPat = false;     // On réinitialise les indicateurs de fin de partie
            for (int indicecoup = 0; indicecoup < suiteCoups.Length - 1; indicecoup++)  // Parcourir tous les coups de la partie
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
            Thread.Sleep(200);  // pause 0,2 seconde
            LogiqueMouvements.MiseenplaceFen(FenDepart);
            NumeroDemiCoup = _indexFenCoupActuel = 0;       // Remise à zéro des indices de parcours
            BoutonBalises.Enabled = OrdinateurJoue.Enabled = false;
            AnalysePosition.Enabled = true;
            InformationPourJoueur.Text = "Trait aux " + QuiJoue + "s";
            MiseaZeroParcours();
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
                InformationPourJoueur.Text = StatusProgramme.Text = _nomMoteur + " réfléchit ...";
                AnalysePosition.Enabled = OrdinateurJoue.Enabled = RetourArriere.Enabled = false;
                groupParcoursPartie.Enabled = TrackBarTempsReflexion.Enabled = false;
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
        {   // Active ou désactive les cases du plateau de jeu
            if (!(statut && _partieTerminee))
            {
                for (int i = 0; i <= 119; i++)
                    if (PictJeux[i].Visible)
                        PictJeux[i].Enabled = statut;
            }
        }
        private void TourneEchiquier()
        {   // Tourne l'échiquier de 180° pour changer le côté de visualisation
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
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = true;
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
        {   // Avant de lancer le moteur, on désactive les boutons pour éviter de perturber sa réflexion
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;
            SaisiePartieBouton.Enabled = AnalysePosition.Enabled = OrdinateurJoue.Enabled = RetourArriere.Enabled = ListeCoupsBouton.Enabled = false;
        }
        private void LeMoteurARépondu()
        {   // Après que le moteur a répondu pour réactiver les boutons
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = true;
            AnalysePosition.Enabled = OrdinateurJoue.Enabled = RetourArriere.Enabled = ListeCoupsBouton.Enabled = true;
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
