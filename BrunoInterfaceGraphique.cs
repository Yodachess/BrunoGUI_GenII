// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Stockfish est développé par Bruno COURTOIS.  Copyright © 2024 █
// █ BrunoGUI_Stockfish est gratuit, sauf s'il est utilisé commercialement  █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
// Informations reflexion moteur - Temps de reflexion - Réglage force moteur
// Gestion par menus - Sauvegarde PGN - Affichage Score - Personnalisation couleurs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static BrunoGUI_Stockfish.GestionPartiePgn;
using static BrunoGUI_Stockfish.LogiqueMouvements;

namespace BrunoGUI_Stockfish
{
    public partial class BrunoInterfaceGraphique : Form
    {
        // Les listes
        private readonly Dictionary<LogiqueMouvements.TypePiece, Bitmap> ListeBitmapsPiece = new Dictionary<LogiqueMouvements.TypePiece, Bitmap>(); // liste des Bitmaps pour les pièces
        private readonly Dictionary<LogiqueMouvements.TypeSymbole, Bitmap> ListeBitmapsSymbole = new Dictionary<LogiqueMouvements.TypeSymbole, Bitmap>(); // liste des Bitmaps pour les symboles
        private readonly List<PictureBox> PictJeux = new List<PictureBox>(); // les 120 cases du jeu
        private readonly List<LogiqueMouvements.TypePiece> ListeNoire = new List<LogiqueMouvements.TypePiece>()          // Reine, Tour, Fou et Cavalier noirs pour promotion
            { LogiqueMouvements.TypePiece.ReineNoire, LogiqueMouvements.TypePiece.TourNoire, LogiqueMouvements.TypePiece.FouNoir, LogiqueMouvements.TypePiece.CavalierNoir };
        private readonly List<LogiqueMouvements.TypePiece> ListeBlanche = new List<LogiqueMouvements.TypePiece>()       // Reine, Tour, Fou et Cavalier blancs pour promotion
            { LogiqueMouvements.TypePiece.ReineBlanche, LogiqueMouvements.TypePiece.TourBlanche, LogiqueMouvements.TypePiece.FouBlanc, LogiqueMouvements.TypePiece.CavalierBlanc };
        private readonly List<Color> CouleurCaseOrigines = new List<Color>(); // Couleurs d'origine des cases
        private readonly List<int> IndiceVisuCoteNoir = new List<int>();

        // les Bitmaps
        private readonly Bitmap PionBlanc = new Bitmap(Properties.Resources.PionBlanc);

        private readonly Bitmap TourBlanche = new Bitmap(Properties.Resources.TourBlanche);
        private readonly Bitmap CavalierBlanc = new Bitmap(Properties.Resources.CavalierBlanc);
        private readonly Bitmap FouBlanc = new Bitmap(Properties.Resources.FouBlanc);
        private readonly Bitmap ReineBlanche = new Bitmap(Properties.Resources.ReineBlanche);
        private readonly Bitmap RoiBlanc = new Bitmap(Properties.Resources.RoiBlanc);
        private readonly Bitmap PionNoir = new Bitmap(Properties.Resources.PionNoir);
        private readonly Bitmap TourNoire = new Bitmap(Properties.Resources.TourNoire);
        private readonly Bitmap CavalierNoir = new Bitmap(Properties.Resources.CavalierNoir);
        private readonly Bitmap FouNoir = new Bitmap(Properties.Resources.FouNoir);
        private readonly Bitmap ReineNoire = new Bitmap(Properties.Resources.ReineNoire);
        private readonly Bitmap RoiNoir = new Bitmap(Properties.Resources.RoiNoir);
        private readonly Bitmap CercleVert = new Bitmap(Properties.Resources.SansPrise); // mouvement autorisé sans prise
        private readonly Bitmap CercleRouge = new Bitmap(Properties.Resources.Menace); // menace pour la pièce sélectionnée
        private readonly Bitmap CercleViolet = new Bitmap(Properties.Resources.Interdit); // mouvement interdit
        private readonly Bitmap CroixPriseVerte = new Bitmap(Properties.Resources.AvecPrise); // mouvement autorisé avec prise

        // les variables
        public static int NumeroDemiCoup { get; set; } = 0;

        private int IndexSource120, ForceMoteurElo, TempsRestant;
        private int DernierCoupMoteurUci;   // dernière case jouée par le moteur UCI
        private int NumeroLigne;    // Indices dans la DataGrid FeuillePartie
        private string CaseSource, CaseDestination, CouleurHumain;
        private string CheminMoteur, MoteurChoisi, NomHumain, joueurElo;
        private string VariationMoteur, MeilleureSuite, ScoreCourant, EvaluationCourante;
        private string[] DonneesUci;        // Données en provenance du Moteur UCI
        private bool ClickCaseSource, VisuSymbole, MontreDonneesBrutesUci, Montre3VariantesUci, AnalyseEnCours, PartieTerminee;
        private bool Humain, OrdinateurJoueBlanc;   // True pour simuler 2 joueurs humains et False pour jouer contre le moteur UCI
        private bool VisuCoteNoir;          // True quand les Noirs sont en bas de l'écran
        private Color CouleurCasesombre, CouleurCaseclaire;
        private LogiqueMouvements.TypePiece SelectionPromotion, PieceSource;
        public bool OrdinateurJoueNoir;
        private Color violetCustom = Color.FromArgb(128, 128, 255);  // Rouge = 128, Vert = 128, Bleu = 255

        // les classes
        public LogiqueMouvements LogiqueMouvements = new LogiqueMouvements();

        public MonoMoteurUci monoMoteurUci = new MonoMoteurUci();
        public GestionPartiePgn GestionPartiePgn = new GestionPartiePgn();
        public PartieForceModule maNouvellePartieForceModule = new PartieForceModule();
        public PartieEchecPGN PartieEnCours = new PartieEchecPGN();
        public ParametresUciStockfish mesParametresUciStockfish = new ParametresUciStockfish();
        public ParametresDeBase mesparametresDeBase;        // mesparametresDeBase est déclarée, mais elle n’est instanciée qu'après "InitializeComponent();"
        private FenetrePartie mafenetrePartie;              // mafenetrePartie est déclarée, mais elle n’est pas encore instanciée. A instancier dans une méthode
        private AffichePgn affichePgn = new AffichePgn();   // affichePgn est à la fois déclarée et instanciée. Prêt à être utilisé dès le début
        private readonly DonneesBrutesUci donneesBrutesUci = new DonneesBrutesUci();

        public BrunoInterfaceGraphique()
        {
            InitializeComponent();
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
            MonoMoteurUci.AfficheUci += AfficheUci;
            MonoMoteurUci.AfficheDonneesBrutes += AfficheDonneesBrutes;
            MonoMoteurUci.AfficheCoupMoteur += AfficheCoupMoteur;
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

            CouleurCasesombre = violetCustom;           // Color.CornflowerBlue;   // Couleurs cases noires par défaut
            CouleurCaseclaire = Color.Lavender;         // Color.AliceBlue;        // Couleurs cases blanches par défaut
            VisuCoteNoir = OrdinateurJoueBlanc = false; // On commence avec la vue côté Blanc, l'odinateur a les Noirs
            MontreDonneesBrutesUci = AnalyseEnCours = PartieTerminee = false;
            AnalysePosition.Enabled = ListeCoupsBouton.Enabled = false;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;
            OrdinateurJoueNoir = true;
            DateTime Aujourdhui = DateTime.Today;
            PartieEnCours.Date = Aujourdhui.ToString("yyyy.MM.dd");
            PartieEnCours.Lieu = "Maison"; PartieEnCours.Tournament = "Entrainement";
            PartieEnCours.WhiteElo = PartieEnCours.BlackElo = "?"; PartieEnCours.Result = "*";
            /* 
            // Moteur UCI par défaut, remonte de 2 niveaux depuis le répertoire "bin\Debug"
            string cheminProjet = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
            // Construire le chemin complet vers le fichier Stockfish
            MoteurChoisi = Path.Combine(cheminProjet, "stockfish", "stockfish17-windows-x86-64-avx2.exe");
            */
            // Obtenir le répertoire de l'exécutable principal
            MoteurChoisi = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stockfish", "stockfish17-windows-x86-64-avx2.exe");

            PartieEnCours.White = NomHumain = LabelJoueurBlanc.Text = "Bruno"; PartieEnCours.WhiteElo = joueurElo = EloBlanc.Text = "1842";
            PartieEnCours.Black = LabelJoueurNoir.Text = "Stockfish 17 "; PartieEnCours.BlackElo = EloNoir.Text = "3150";

            DessineEchiquier();
            for (int i = 0; i <= 119; i++)
            {   // On place des bords sur tout l'échiquier
                LogiqueMouvements.PiecesEchiquier.Add(LogiqueMouvements.TypePiece.Bordure);
                IndiceVisuCoteNoir.Add(i);      // et on crée la liste de 1 à 120
            }
            IndiceVisuCoteNoir.Reverse();       // On inverse l'ordre pour avoir la liste de 120 à 1 pour la vue côté noir
            LogiqueMouvements.InitialisationEchiquier();
            this.ActiveControl = Plateau;       // Met le focus sur le plateau pour éviter le Bug des radiobutton "Résultat"
            Humain = false;                     // L'opposant est l'ordinateur, à mettre à true pour simuler 2 joueurs humains
            monoMoteurUci.Start(MoteurChoisi);  // Démarrage UCI car jeu contre l'ordinateur par défaut
        }

        // Le joueur sélectionne la case source ou destination avec la souris
        private void CaseMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is PictureBox CaseClick)
                {
                    int IndexCase120 = Convert.ToInt32(CaseClick.Name.Substring(8)); // Utilise le numéro de la PictureBox comme index
                    if (VisuCoteNoir)
                        IndexCase120 = IndiceVisuCoteNoir[IndexCase120];            // Si on regarde côté noir, il faut inverser l'index par rapport a la vue côté blanc
                    if (CouleurHumain == string.Empty && Humain == false)
                        MessageBox.Show("Veuillez choisir votre couleur\n(Menu Partie / Nouvelle Partie)", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                    {
                        if (ClickCaseSource)     // Permet de savoir si c'est la sélection de la pièce ou le déplacement
                        {        // Sélection d'une pièce
                            IndexSource120 = IndexCase120;
                            CaseSource = LogiqueMouvements.NomCaseAlgebrique(IndexSource120);
                            PieceSource = LogiqueMouvements.PiecesEchiquier[IndexSource120];
                            if (PictJeux[IndexCase120].Image != null)
                            {   // Si la case cliquée contient bien une pièce ou un pion, on va utiliser le thumbnail de la pièce comme curseur :-)
                                using (Bitmap Piece = new Bitmap(PictJeux[IndexCase120].Image))
                                {   // Quand on bouge la souris, on bouge le thumbnail de la pièce comme un curseur :-)
                                    Bitmap thumbnail = (Bitmap)Piece.GetThumbnailImage(88, 88, null, IntPtr.Zero);
                                    Cursor = new Cursor(thumbnail.GetHicon());
                                }
                                DessinePiece(IndexCase120, LogiqueMouvements.TypePiece.Vide);       // On vide la case d'origine car le joueur bouge la pièce ...
                                LogiqueMouvements.DessineMouvements(CaseSource, true);
                                ClickCaseSource = false;
                            }
                        }
                        else
                        {       // Déplacement d'une pièce
                            LogiqueMouvements.Echec = false;
                            Cursor = Cursors.Default;       // On revient au curseur "normal"
                            LogiqueMouvements.EffaceSymboles(true);
                            CaseDestination = LogiqueMouvements.NomCaseAlgebrique(IndexCase120);
                            LogiqueMouvements.ExecutionCoup(CaseSource, CaseDestination);
                            string chaineFen = LogiqueMouvements.RetourneChaineFenActuel();         // UCI : remplacer le FEN par liste de coups ?!
                            if (LogiqueMouvements.CoupValide)
                            {                                               // envoi de la Position Fen au moteur UCI
                                if (Humain == false)
                                {
                                    GestionChronometre(maNouvellePartieForceModule.DureeReflexionSeconde / 1000);
                                    MonoMoteurUci.JeuMoteurUci(chaineFen, maNouvellePartieForceModule.DureeReflexionSeconde);   // Démarre la réflexion du moteur
                                    if (LogiqueMouvements.EchecetMat != true)
                                    {
                                        InformationPourJoueur.Text = StatusProgramme.Text = MoteurChoisi + " réfléchit ...";
                                        RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = false;     // Il faut empêcher tout cela si le moteur réfléchit
                                        BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;
                                    }
                                    Console.WriteLine($"Durée Réflexion envoyé (caseMouseDown) =  {maNouvellePartieForceModule.DureeReflexionSeconde}");
                                    if (LogiqueMouvements.RenvoieCaseIndex120(CaseDestination) == DernierCoupMoteurUci)
                                        DernierCoupMoteurUci = -1;
                                }
                            }
                            else
                            {
                                DessinePiece(IndexSource120, PieceSource);  // Si le coup n'est pas valide, on remet la pièce sur sa case d'origine !
                            }
                            ClickCaseSource = true;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Erreur : sender n'est pas une PictureBox.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur dans CaseMoveDown : " + ex.Message);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }

        private void NouvellePartie_Click(object sender, EventArgs e)
        {   // Affiche la boîte de dialogue nouvelle partie
            Humain = AnalyseEnCours = PartieTerminee = ListeCoupsBouton.Enabled = AnalysePosition.Enabled = false;
            QuiJoue = ColorPiece.Blanc;
            if (maNouvellePartieForceModule.ShowDialog() == DialogResult.OK)
            {   // Utilise les sélections faites par l'utilisateur
                string couleurMoteur = maNouvellePartieForceModule.ChoixCouleur;
                bool forceMaximale = maNouvellePartieForceModule.ForceMaximale;
                ForceMoteurElo = maNouvellePartieForceModule.ForceModule;
                MiseaZeroAffichages();
                if (forceMaximale)
                {   // Moteur à sa force Elo maximale
                    ForceMoteurElo = 3150;
                }
                MonoMoteurUci.ActiveLimiteElo();
                MonoMoteurUci.DefinitLimiteElo(ForceMoteurElo.ToString());
                MonoMoteurUci.StandardInputDataToUci("setoption name MultiPV value " + mesParametresUciStockfish.MultiPV);
                if (couleurMoteur == "Blancs")
                {
                    PartieEnCours.White = LabelJoueurBlanc.Text = MoteurChoisi;
                    PartieEnCours.WhiteElo = EloBlanc.Text = ForceMoteurElo.ToString();
                    PartieEnCours.Black = LabelJoueurNoir.Text = maNouvellePartieForceModule.NomAdversaire;
                    PartieEnCours.BlackElo = EloNoir.Text = joueurElo;
                    OrdinateurJoueNoir = false;
                    CommencerPartie();
                    if (VisuCoteNoir == false)
                        TourneEchiquier();                                          // On met la vue côté Noir
                    ParametresJoueurHumain("Noirs", "Le moteur UCI joue");
                    RetourArriere.Enabled = false;         // Il faut empêcher le retour si le moteur réfléchit
                    BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;
                    MonoMoteurUci.JeuMoteurUci(LogiqueMouvements.FenDepart, maNouvellePartieForceModule.DureeReflexionSeconde); // On fait jouer le moteur, avec le temps de réflexion choisi
                    Console.WriteLine($"Durée Réflexion envoyé (nouvelle partie) =  {maNouvellePartieForceModule.DureeReflexionSeconde}");
                }
                else
                {
                    PartieEnCours.White = LabelJoueurBlanc.Text = maNouvellePartieForceModule.NomAdversaire;
                    PartieEnCours.WhiteElo = EloBlanc.Text = joueurElo;
                    PartieEnCours.Black = LabelJoueurNoir.Text = MoteurChoisi;
                    PartieEnCours.BlackElo = EloNoir.Text = ForceMoteurElo.ToString();
                    OrdinateurJoueNoir = true;
                    if (VisuCoteNoir)
                        TourneEchiquier();
                    CommencerPartie();
                    ParametresJoueurHumain("Blancs", "A vous de jouer");            // On demande à l'humain de jouer
                    PlateauEnable(true);                                            // On lui permet de bouger les pièces
                }
            }
        }

        private void CommencerPartie()              // Début d'une nouvelle partie
        {
            PartieTerminee = false;
            DernierCoupMoteurUci = -1;
            ClickCaseSource = VisuSymbole = true;
            PartieEnCours.CoupsPartiePGN = "";
            for (int i = 0; i <= 119; i++)
            {
                PictJeux[i].Image = null;
                PictJeux[i].BorderStyle = BorderStyle.None;
            }
            MiseaZeroAffichages();
            CouleurHumain = VarianteMoteurUci1.Text = string.Empty;
            NumeroDemiCoup = 0;
            if (Humain == false)
            {
                InformationPourJoueur.Visible = true;
                InformationPourJoueur.Text = "Pour commencer une partie, cliquer \n sur Partie et choisissez votre couleur";
            }
            else
            {
                PartieEnCours.White = PartieEnCours.Black = "Humain";
                PlateauEnable(true);
                InformationPourJoueur.Visible = true;
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux Blancs de jouer";
            }
            GestionChronometre(maNouvellePartieForceModule.DureeReflexionSeconde / 1000);
            RetourArriere.Visible = true;
        }

        private void ParametresJoueurHumain(string Couleur, string Affichage)
        {   // Paramètres selon joueur humain noir ou blanc
            CouleurHumain = Couleur;
            if (Humain == false)
                InformationPourJoueur.Text = StatusProgramme.Text = "(Vous avez les " + Couleur + ")";
            else
            {
                InformationPourJoueur.Visible = true;
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux Blancs de jouer";
            }
            InformationPourJoueur.Visible = true;
            InformationPourJoueur.Text = StatusProgramme.Text = Affichage;
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion du temmps / Chronomètre
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void GestionChronometre(int secondesRestantes)
        {
            TempsRestant = secondesRestantes * 100;             // Réinitialiser le temps
            Decompteur.Text = FormateTemps(TempsRestant);
            ChronometreTempsFixe.Start();  // Démarrer le chronomètre
        }

        private void ChronometreTempsFixe_Tick(object sender, EventArgs e)
        {
            if (TempsRestant > 0)
            {
                TempsRestant -= 10;  // Diminuer le décompteur de 10 car décrément par dixième de seconde
                Decompteur.Text = FormateTemps(TempsRestant);  // Mettre à jour l'UI
            }
            else
            {
                ChronometreTempsFixe.Stop();    // Arrêter le Timer à 0
                Decompteur.Text = "Terminé !";  // Afficher un message
            }
        }

        private string FormateTemps(int centiemeSeconde)
        {   // Méthode pour formater le temps en secondes et centièmes de seconde
            int seconde = centiemeSeconde / 100;    // Convertir en secondes
            int centieme = centiemeSeconde % 100;   // Centièmes de seconde restants
            return $"{seconde:D2}:{centieme:D2}";   // Formater comme "xx:yy"
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
                DonneesUci = MonoMoteurUci.DataUci.Split(' ');                      // Découpage des informations du moteur UCI
            if (string.IsNullOrWhiteSpace(DonneesUci[0]) == false & DonneesUci.Length > 2)
            {                                                   // true si la chaine est " ", "\n", null, ""
                switch (DonneesUci[0])          // identifier le premier mot
                {
                    case "\n":                  // Analyse réponse moteur UCI
                    case " ":
                        break;

                    case "bestmove":            // le moteur UCI propose le meilleur coup
                        VarianteMoteurCourante.Text = "Coup joué : " + DonneesUci[1] + "    (" + "Conseil :     " + DonneesUci[3] + ")";
                        break;

                    case "id":
                        {
                            switch (DonneesUci[1])
                            {
                                case "name":
                                    MoteurChoisi = MonoMoteurUci.DataUci.Substring(8);
                                    break;

                                case "author":
                                    // AuteurProgramme.Text = "     Auteur(s) : \n" + MonoMoteurUci.DataUci.Substring(10);
                                    break;
                            }
                        }
                        break;

                    case "info":                // Infos de réflexion moteur
                        {                       // Parcours des données Uci
                            for (int ucindex = 1; (ucindex < DonneesUci.Length); ucindex++) // Recherche des informations sur la chaine DonneesUci
                                switch (DonneesUci[ucindex])
                                {
                                    case "book":
                                        VarianteMoteurUci1.Invoke(new Action(() =>
                                        {
                                            VarianteMoteurUci1.Text = "    Le moteur est dans sa bibliothèque d'ouvertures";
                                        }));
                                        break;

                                    case "multipv":
                                        numeroVarianteNomBox = "VarianteMoteurUci" + DonneesUci[ucindex + 1];
                                        break;

                                    case "cp":
                                        ScoreCourant = (Decimal.Parse(DonneesUci[ucindex + 1]) / 100).ToString("N2", CultureInfo.InvariantCulture);
                                        if (numeroVarianteNomBox == "VarianteMoteurUci1")
                                        {   // On affiche seulement le score de la meilleure variante
                                            ScoreMoteur.Text = "Score : " + ScoreCourant;
                                            AfficheEvaluation(ScoreCourant);
                                        }
                                        break;

                                    case "mate":
                                        string nombreCoupsMat = "MAT en " + Math.Abs(int.Parse(DonneesUci[ucindex + 1]));
                                        ScoreCourant = "M" + Math.Abs(int.Parse(DonneesUci[ucindex + 1]));
                                        if (numeroVarianteNomBox == "VarianteMoteurUci1")
                                        {   // On affiche le Mat seulement si c'est la meilleure variante
                                            InformationPourJoueur.Text = ScoreMoteur.Text = nombreCoupsMat;
                                        }
                                        break;

                                    case "pv":          // Affichage de la variation principlale
                                        int position = MonoMoteurUci.DataUci.IndexOf(" pv ");
                                        VariationMoteur = MonoMoteurUci.DataUci.Substring(position + 3);
                                        if (VariationMoteur.Length > 60)
                                            VariationMoteur = VariationMoteur.Substring(0, 60);     // On limite la longueur de la variation, pour rester dans le label
                                        VariationMoteur = GestionPartiePgn.AlgebriqueVersPgn(VariationMoteur, NumeroDemiCoup);  // Elle est en algébrique long, il la faut en PGN Fr ...
                                        varianteExaminee = string.Join(" ", VariationMoteur.Split(' ').Take(3));
                                        if (numeroVarianteNomBox != "")     // Par exemple Sargon n'a pas de multipv ?
                                        {
                                            RichTextBox numeroVarianteBox = Controls.Find(numeroVarianteNomBox, true).FirstOrDefault() as RichTextBox;
                                            numeroVarianteBox?.Invoke(new Action(() =>    // Si numeroVarianteBox n'est pas nul
                                            {
                                                numeroVarianteBox.Text = " " + numeroVarianteNomBox[numeroVarianteNomBox.Length - 1] + ". (" + varianteExaminee + ") █[ " +
                                                ScoreCourant + " ]█  " + "[ " + VariationMoteur + " ]";
                                                Console.WriteLine($"Variation : {numeroVarianteBox.Name}, / {VariationMoteur}");
                                            }));
                                        }
                                        else
                                        {                                       // Pour ceux qui n'ont qu'une variante principale (Sargon, ...) ?!
                                            VarianteMoteurUci1.Invoke(new Action(() =>
                                            {  // On n'utilise que la Box VarianteMoteurUci1 ...
                                                VarianteMoteurUci1.Text = " 1. (" + varianteExaminee + ") █[ " + ScoreCourant + " ]█  " + "[ " + VariationMoteur + " ]";
                                                Console.WriteLine($"Seulement une variante !!! : {VarianteMoteurUci1}");
                                            }));
                                        }
                                        break;
                                }
                        }
                        break;
                }
            }
        }

        private void AfficheEvaluation(string ScoreCourant)
        {
            if (!AnalyseEnCours)
            {
                decimal score = Decimal.Parse(ScoreCourant, CultureInfo.InvariantCulture);
                if (OrdinateurJoueNoir)
                {
                    score = -score;
                }
                if (Math.Abs(score) <= 0.5m)
                {
                    EvaluationUci.Text = EvaluationCourante = "Égal";
                }
                else if (score > 0.5m && score < 2.4m)
                {
                    EvaluationUci.Text = EvaluationCourante = "Avantage Blanc";
                }
                else if (score < -0.5m && score > -2.4m)
                {
                    EvaluationUci.Text = EvaluationCourante = "Avantage Noir";
                }
                else if (score >= 2.5m)
                {
                    EvaluationUci.Text = EvaluationCourante = "Gain Blanc";
                }
                else if (score <= -2.5m)
                {
                    EvaluationUci.Text = EvaluationCourante = "Gain Noir";
                }
            }
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
                if (MonoMoteurUci.UciVersGui)
                {
                    if (!MonoMoteurUci.DataUci.Contains("currmove"))    // Inutile d'afficher les currmove, il n'y rien d'intéressant ...
                        donneesBrutesUci.DonneesBrutesVue.AppendText(Environment.NewLine + "[" + MoteurChoisi + "]    " + MonoMoteurUci.DataUci);
                }
                else
                    donneesBrutesUci.DonneesBrutesVue.AppendText(Environment.NewLine + " [BrunoGUI_Stockfish]    " + MonoMoteurUci.DataVersUci);
                donneesBrutesUci.DonneesBrutesVue.ScrollToCaret();   // Pour garder l'affichage dans toute la fenêtre
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
                if (!AnalyseEnCours)
                {   // Si ce n'est pas une analyse ...
                    StatusProgramme.Text = InformationPourJoueur.Text = "A vous de jouer";
                    CaseSource = MonoMoteurUci.CoupUci.Substring(0, 2);                 // CoupUci contient le "best move" sous la forme e2e4
                    CaseDestination = MonoMoteurUci.CoupUci.Substring(2, 2);
                    if (LogiqueMouvements.EchecetMat == false)                          // Note : Si c'est Mat, on n"execute pas de coup
                    {
                        LogiqueMouvements.ExecutionCoup(CaseSource, CaseDestination);   // Exécute un coup du moteur UCI
                    }
                    if (DernierCoupMoteurUci != -1)
                    {       // on redessine la case pour effacer le contour du coup précédent du Moteur UCI
                        PictJeux[DernierCoupMoteurUci].BackColor = CouleurCaseOrigines[DernierCoupMoteurUci];
                        DessinePiece(DernierCoupMoteurUci, LogiqueMouvements.PiecesEchiquier[DernierCoupMoteurUci]);
                    }
                    DernierCoupMoteurUci = LogiqueMouvements.RenvoieCaseIndex120(CaseDestination);
                    TraceContour(DernierCoupMoteurUci);
                    RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = true;      // On réautorise si le moteur a fini de réfléchir
                    if (LogiqueMouvements.EchecetMat == false)
                        BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = true;
                }
                else
                {   // c'est une analyse, on affiche la meilleure variante
                    if (VarianteMoteurUci1.Text != "")
                    {
                        string[] meilleureVariante = VarianteMoteurUci1.Text.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                        string debutVariante = Regex.Match(meilleureVariante[0], @"\((.*?)\)").Groups[1].Value;
                        MeilleureSuite = debutVariante + " Evaluation --- " + meilleureVariante[1] + "(" + EvaluationCourante + ")" + " --- \n" + meilleureVariante[3];
                        _ = MessageBox.Show("La meilleure suite est : " + debutVariante +
                                            "\n Evaluation --- " + meilleureVariante[1] + "(" + EvaluationCourante + ")" + " --- " +
                                            "\n" + meilleureVariante[3], "Analyse Moteur", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    InformationPourJoueur.Text = StatusProgramme.Text = "Analyse terminée... ";
                    RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = true;      // On réautorise si le moteur a fini de réfléchir
                    AnalyseEnCours = false;
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
                PartieEnCours.PlyCount = (LogiqueMouvements.ListeCoupsFen.Count).ToString();
                NumeroLigne++;
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
                PartieEnCours.PlyCount = (LogiqueMouvements.ListeCoupsFen.Count).ToString();
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
            if (Humain)
                InformationPourJoueur.Text = StatusProgramme.Text = "Aux " + Couleur + " de jouer";
            else
                PlateauEnable(Couleur == CouleurHumain); // active les Picturebox si c'est au tour du joueur humain
        }

        private void AfficheEchecEtMat(string couleurRoiMat)   // Affiche l'échec et mat du roi de la couleur en paramètre
        {
            int indexCouleur = couleurRoiMat == "Blanc" ? 2 : 1;
            LogiqueMouvements.ListeCoupsPgn[LogiqueMouvements.ListeCoupsPgn.Count - 1] = (LogiqueMouvements.ListeCoupsPgn[LogiqueMouvements.ListeCoupsPgn.Count - 1].ToString()).Replace("+", "#");
            LogiqueMouvements.ListeCoupsPgnFr[LogiqueMouvements.ListeCoupsPgnFr.Count - 1] = (LogiqueMouvements.ListeCoupsPgnFr[LogiqueMouvements.ListeCoupsPgnFr.Count - 1].ToString()).Replace("+", "#");
            LogiqueMouvements.ListeCoupsNal[LogiqueMouvements.ListeCoupsNal.Count - 1] = (LogiqueMouvements.ListeCoupsNal[LogiqueMouvements.ListeCoupsNal.Count - 1].ToString()).Replace("+", "#");
            string coupMat = (LogiqueMouvements.ListeCoupsPgnFr[LogiqueMouvements.ListeCoupsPgn.Count - 1].ToString()).Replace("+", "#");
            int indexPoint = coupMat.IndexOf('.');  // On enlève le numéro de coup s'il existe
            if (indexPoint != -1)
            {
                coupMat = coupMat.Substring(indexPoint).Replace(".", "");
            }
            if (indexCouleur == 2)
            {       // Couleur du Roi mat = Blanc
                GestionResultat("0-1", " Gain Noir");
            }
            else
            {                               // Couleur du Roi mat = Noir
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
            InformationsPartie.ForeColor = Color.Black;
            if (infoechec.Contains("échec") || infoechec.Contains("Pat"))
                InformationsPartie.ForeColor = Color.Black;
            if (infoechec.Contains("Pat"))
            {
                GestionResultat("1/2-1/2", "Pat (Nulle)");
                PlateauEnable(false); // un des joueurs est pat : fin de la partie
            }
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
            LogiqueMouvements.ListeCoupsPgn.Add(resultat);
            LogiqueMouvements.ListeCoupsPgnFr.Add(resultat);
            LogiqueMouvements.ListeCoupsNal.Add(resultat);
            PartieEnCours.Result = EvaluationUci.Text = resultat;
            ScoreMoteur.Text = vainqueur;
            InformationsPartie.Text = resultat + "  (" + vainqueur + ")";
            StatusProgramme.Text = "Partie terminée";
            PartieTerminee = true;
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;
            RetourArriere.Visible = false;
            PlateauEnable(false);
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion des boutons
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void HumainContreHumain_Click(object sender, EventArgs e)
        {
            PartieEnCours.White = "Humain1";
            PartieEnCours.Black = "Humain2";
            // On demande confirmation car la partie est remise à zéro
            string confirmation = "Vous jouez contre votre ami/partenaire." + "\nToute position précédente sera effacée,\n confirmez avec Oui, sinon Annuler";
            DialogResult Resultat = MessageBox.Show(confirmation, "Vous jouez entre amis, sans ordinateur", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (Resultat == DialogResult.OK)
            {
                Humain = true;
                OrdinateurJoueBlanc = OrdinateurJoueNoir = false;
                StatusProgramme.Text = "Humain contre Humain";
                InformationsPartie.Text = " Bruno vous souhaite une bonne partie !";
                MonoMoteurUci.ActiveLimiteElo();        // Préparation du moteur en cas de demande d'analyse
                MonoMoteurUci.DefinitLimiteElo("3190");
                MonoMoteurUci.StandardInputDataToUci("setoption name MultiPV value 3");
                maNouvellePartieForceModule.DureeReflexionSeconde = 10000;
                CommencerPartie();
            }
        }

        private void AnalysePosition_Click(object sender, EventArgs e)
        {
            InformationPourJoueur.Text = StatusProgramme.Text = "Analyse de la position ...";
            AnalyseEnCours = true;
            if (NumeroDemiCoup <= LogiqueMouvements.ListeCoupsFen.Count - 1)            // on empêche d'analyser sur une case au-delà de la partie ...
            {
                string Fenaenvoyer = LogiqueMouvements.ListeCoupsFen[NumeroDemiCoup];   // Récupère le FEN (position)
                RetourArriere.Enabled = AnalysePosition.Enabled = ListeCoupsBouton.Enabled = false;             // Il faut empêcher le retour si le moteur réfléchit
                BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = false;
                MonoMoteurUci.JeuMoteurUci(Fenaenvoyer, maNouvellePartieForceModule.DureeReflexionSeconde);     // On fait jouer le moteur, avec le temps de réflexion choisi
                Console.WriteLine($"Durée Réflexion (analyse postion) =  {maNouvellePartieForceModule.DureeReflexionSeconde}");
            }
        }

        private void RetourArriere_Click(object sender, EventArgs e)
        {
            if (LogiqueMouvements.ListeCoupsFen.Count <= 1)
                _ = MessageBox.Show("Pas assez de coups joués \nPas de retour arrière possible", "Retour impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                string confirmation = "Le retour arrière ne se fait que pour le dernier coup !" + "\nVous allez être amené à la fin de la partie,\n confirmer avec Oui, sinon Annuler";
                DialogResult resultat = MessageBox.Show(confirmation, "Retour arrière d'un demi coup", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (resultat == DialogResult.OK)
                {
                    NombreCoupsJoues -= Convert.ToSingle(0.5);      // On décrémente d'un demi-coup
                    NumeroDemiCoup--;
                    LogiqueMouvements.ListeCoupsFen.RemoveAt(LogiqueMouvements.ListeCoupsFen.Count - 1);    // On supprime le dernier 1/2 coup
                    LogiqueMouvements.ListeCoupsPgn.RemoveAt(LogiqueMouvements.ListeCoupsPgn.Count - 1);    // pour les 4 listes
                    LogiqueMouvements.ListeCoupsPgnFr.RemoveAt(LogiqueMouvements.ListeCoupsPgnFr.Count - 1);
                    LogiqueMouvements.ListeCoupsNal.RemoveAt(LogiqueMouvements.ListeCoupsNal.Count - 1);
                    // Réactivation du roque si besoin ...  (champ 3 du FEN, mais indice 2 du Split)
                    string[] FenPrecedent = LogiqueMouvements.ListeCoupsFen[LogiqueMouvements.ListeCoupsFen.Count - 1].Split(' ');  // Récupère le dernier FEN découpé
                    if ((FenPrecedent[2]).Contains("-"))
                    {
                        StatutRoque = FlagEnableRoque.AucunRoque;       // Aucun roque possible
                        PetitRoqueNoirPossible = PetitRoqueBlancPossible = GrandRoqueNoirPossible = GrandRoqueBlancPossible = false;
                    }
                    else if ((FenPrecedent[2]).Contains("K"))
                    {   // Petit roque blanc possible
                        PetitRoqueBlancPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueBlanc;
                    }
                    else if ((FenPrecedent[2]).Contains("Q"))
                    {   // Grand roque blanc possible
                        GrandRoqueBlancPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueBlanc;
                    }
                    else if ((FenPrecedent[2]).Contains("k"))
                    {   // Petit roque noir possible
                        PetitRoqueNoirPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueNoir;
                    }
                    else if ((FenPrecedent[2]).Contains("q"))
                    {   // Grand roque noir possible
                        GrandRoqueNoirPossible = true;
                        StatutRoque |= FlagEnableRoque.RoqueNoir;
                    }
                    string Fenaenvoyer = LogiqueMouvements.ListeCoupsFen[LogiqueMouvements.ListeCoupsFen.Count - 1];    // Récupère le dernier FEN (position)
                    LogiqueMouvements.MiseenplaceFen(Fenaenvoyer);              // et on l'affiche sur l'échiquier
                }
            }
        }

        private void ParametresDeBase_Click(object sender, EventArgs e)
        {
            mesparametresDeBase.Show();
        }

        private void ParametresAvances_Click(object sender, EventArgs e)
        {
            mesParametresUciStockfish.Show();
        }

        private void ListeCoupsBouton_Click(object sender, EventArgs e)
        {
            string numeroCoup = "";
            string blancs = "";
            string noirs = "";
            PlateauEnable(false);   // Blocage du plateau car je ne veux pas autoriser de jouer pendant le parcours de la partie ....
            // RetourArriere.Enabled = false;
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
                string dernierElement = LogiqueMouvements.ListeCoupsPgnFr[LogiqueMouvements.ListeCoupsPgnFr.Count - 1].Trim();
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
                        string[] coupBlancs = coup.Split(new[] { ' ' }, 2);
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
            Console.WriteLine($"Nombre coups de la liste Pgn : {nombreCoups}, Coûp valide : {CoupValide}");                           // 01/02  DEBUG
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

        private void visualiserPgn_Click(object sender, EventArgs e)
        {   // Option de menu  pour voir la partie en PGN
            VisualisationPgn_Click(sender, e);
        }

        private void BoutonBalises_Click(object sender, EventArgs e)
        {
            SaisieBalises SaisieBalises = new SaisieBalises(PartieEnCours);
            SaisieBalises.ShowDialog();
            LabelJoueurBlanc.Text = PartieEnCours.White;        // On affiche les noms et ELO des joueurs qui sont dans l'entête PGN
            LabelJoueurNoir.Text = PartieEnCours.Black;
            EloBlanc.Text = PartieEnCours.WhiteElo;
            EloNoir.Text = PartieEnCours.BlackElo;
        }

        private void MontreDonneesUci_Click(object sender, EventArgs e)
        {
            MontreDonneesBrutesUci = !MontreDonneesBrutesUci;       // Affiche ou masque les données UCI à chaque clic
            MontreDonneesUci.Text = MontreDonneesBrutesUci ? "Masque protocole UCI" : "Affiche protocole UCI";
            if (MontreDonneesBrutesUci) donneesBrutesUci.Show();    // On affiche les données brutes UCI
            else donneesBrutesUci.Hide();                       // On masque les données brutes UCI
            donneesBrutesUci.DonneesBrutesVue.ScrollToCaret();      // Pour garder l'affichage dans toute la fenêtre
        }

        private void MontreVariantesUci_Click(object sender, EventArgs e)
        {
            Montre3VariantesUci = !Montre3VariantesUci;       // Affiche ou masque les 3 variantes UCI à chaque clic
            MontreVariantesUci.Text = Montre3VariantesUci ? "Affiche variantes UCI" : "Masque variantes UCI";
            VarianteMoteurUci1.Visible = VarianteMoteurUci2.Visible = VarianteMoteurUci3.Visible = !Montre3VariantesUci;
        }

        private void InverseEchiquier_Click(object sender, EventArgs e)
        {
            PlateauEnable(true);
            TourneEchiquier();
        }

        private void CaseSombre_Click(object sender, EventArgs e)
        {
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
            {
                CouleurCasesombre = CouleurDialogue.Color;          // On récupère la couleur choisie par l'utilisateur
                Color Couleur;
                int index = 0;
                CouleurCaseOrigines.Clear();                        // On nettoie la liste des couleurs d'origine
                // les 120 cases du jeu (seules 64 cases sont visibles : voir la classe ClassEchiquier pour les détails )
                for (int Ligne = 0; Ligne <= 11; Ligne++)           // On parcourt les lignes de l"échiquier
                {
                    Couleur = Ligne % 2 == 0 ? CouleurCaseclaire : CouleurCasesombre; // Couleur des cases de l'échiquier
                    for (int Colonne = 0; Colonne <= 9; Colonne++)              // On parcourt les colonnes de l'échiquier
                    {
                        PictJeux[index].BackColor = Couleur;
                        CouleurCaseOrigines.Add(Couleur);
                        index++;
                        Couleur = Couleur == CouleurCaseclaire ? CouleurCasesombre : CouleurCaseclaire;
                    }
                }
            }
        }

        private void CaseClaire_Click(object sender, EventArgs e)
        {
            if (CouleurDialogue.ShowDialog() == DialogResult.OK)
            {
                CouleurCaseclaire = CouleurDialogue.Color;          // On récupère la couleur choisie par l'utilisateur
                Color Couleur;
                int index = 0;
                CouleurCaseOrigines.Clear();                        // On nettoie la liste des couleurs d'origine
                // les 120 cases du jeu (seules 64 cases sont visibles : voir la classe ClassEchiquier pour les détails )
                for (int ligne = 0; ligne <= 11; ligne++)           // On parcourt les lignes de l"échiquier
                {
                    Couleur = ligne % 2 == 0 ? CouleurCaseclaire : CouleurCasesombre; // Couleur des cases de l'échiquier
                    for (int Colonne = 0; Colonne <= 9; Colonne++)              // On parcourt les colonnes de l'échiquier
                    {
                        PictJeux[index].BackColor = Couleur;
                        CouleurCaseOrigines.Add(Couleur);
                        index++;
                        Couleur = Couleur == CouleurCaseclaire ? CouleurCasesombre : CouleurCaseclaire;
                    }
                }
            }
        }

        //          Gestion de la promotion de Pion
        private void Promo0_Click(object sender, EventArgs e)
        {
            PictureBox Promotion = (PictureBox)sender;
            int indexSelect = Convert.ToInt32(Promotion.Name.Substring(5));
            SelectionPromotion = LogiqueMouvements.QuiJoue == LogiqueMouvements.ColorPiece.Blanc ? ListeBlanche[indexSelect] : ListeNoire[indexSelect];
            LogiqueMouvements.PromotionPiece = SelectionPromotion;
        }

        private void AffichePromotionPion(string Couleur)        // Promotion d'un pion
        {
            SelectionPromotion = LogiqueMouvements.TypePiece.Vide;
            Promo0.Image = Couleur == "Noir" ? ReineNoire : ReineBlanche;
            Promo1.Image = Couleur == "Noir" ? TourNoire : TourBlanche;
            Promo2.Image = Couleur == "Noir" ? FouNoir : FouBlanc;
            Promo3.Image = Couleur == "Noir" ? CavalierNoir : CavalierBlanc;
            GroupPromo.Visible = true;
            while (SelectionPromotion == LogiqueMouvements.TypePiece.Vide)
                Application.DoEvents();
            GroupPromo.Visible = false;
        }

        private void PointArret_Click(object sender, EventArgs e)
        {
            InformationPourJoueur.Text = "Point d'arrêt pour déboguer ...";
        }

        private void StopMoteur_Click(object sender, EventArgs e)
        {
            MonoMoteurUci.StandardInputDataToUci("stop");
            InformationPourJoueur.Text = "Arrêt réflexion Moteur ";
        }

        private void Apropos_Click(object sender, EventArgs e)
        {
            _ = MessageBox.Show("      BrunoGUI Stockfish\n       Version 1.051\n--  Bruno COURTOIS  -- \n Copyright © 2024", "A propos de",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Quitter_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Gestion de fichiers (ouverture/sauvegarde)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
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
                            DialogResult resultat = MessageBox.Show("ATTENTION, le fichier " + Path.GetFileName(cheminPgn) + " existe déjà. \nCliquer Oui pour ajouter la partie à la fin." +
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
                                MessageBox.Show($"Fichier PGN :\n {contenuPgn}", "Affichage fichier PGN", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Une erreur s'est produite : {ex.Message}", "Erreur méthode Enregistrer PGN", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
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

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Routines de Dessin
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void DessineEchiquier()
        {
            Color Couleur;
            int Index = 0;
            // les 120 cases du jeu (seules 64 cases sont visibles : voir la classe ClassEchiquier pour les détails )
            for (int Ligne = 0; Ligne <= 11; Ligne++)
            {
                Couleur = Ligne % 2 == 0 ? CouleurCaseclaire : CouleurCasesombre;       // Couleur des cases de l'échiquier
                for (int Colonne = 0; Colonne <= 9; Colonne++)
                {
                    PictureBox Pict = new PictureBox
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
                    Couleur = Couleur == CouleurCaseclaire ? CouleurCasesombre : CouleurCaseclaire;
                }
            }
        }

        // Dessine une pièce sur l'échiquier avec l'indexSource120 et le type de la Piece
        private void DessinePiece(int IndexCase, LogiqueMouvements.TypePiece Piece)
        {
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
                Console.WriteLine("Erreur dans DessinePiece : " + ex.Message);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }

        // Trace un contour pour la case jouée par le moteur Uci
        private void TraceContour(int IndexCase)
        {
            try
            {
                if (PictJeux[IndexCase].Image != null)
                {
                    Bitmap CaseJeu = new Bitmap(PictJeux[IndexCase].Image);
                    Graphics g = Graphics.FromImage(CaseJeu);
                    Pen Pinceau = new Pen(Color.Black, 5);
                    g.DrawLine(Pinceau, 0, 0, 100, 0);
                    g.DrawLine(Pinceau, 0, 0, 0, 100);
                    g.DrawLine(Pinceau, 85, 0, 85, 100);    // plutôt g.DrawLine(Pinceau, 0, 0, 100, 0);   ??   // Côté droit   DEBUG 26/02
                    g.DrawLine(Pinceau, 0, 85, 100, 85);    // plutôt g.DrawLine(Pinceau, 0, 100, 100, 100); ?? // Côté inférieur
                    PictJeux[IndexCase].Image = CaseJeu;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur dans TraceContour : " + ex.Message);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
            }
        }

        // Dessine un des 4 symboles sur l'échiquier si ceux sont visibles
        private void DessineSymbole(int IndexCase, LogiqueMouvements.TypeSymbole Symbole)
        {
            if (VisuSymbole == true)
            {
                if (PictJeux[IndexCase].Image == null)                          // Si la case est vide,
                    PictJeux[IndexCase].Image = ListeBitmapsSymbole[Symbole];   // On dessine le symbole passé en paramètre
                else
                {                                                               // Si la case n'est pas vide
                    Bitmap CaseJeu = new Bitmap(PictJeux[IndexCase].Image);
                    Graphics g = Graphics.FromImage(CaseJeu);
                    g.DrawImage(ListeBitmapsSymbole[Symbole], 0, 0, 100, 100);
                    PictJeux[IndexCase].Image = CaseJeu;
                }
            }
        }

        public void PlateauEnable(bool statut)                 // Active ou désactive les cases du plateau de jeu
        {
            if (!(statut && PartieTerminee))
            {
                for (int i = 0; i <= 119; i++)
                    if (PictJeux[i].Visible)
                        PictJeux[i].Enabled = statut;
            }
        }

        private void TourneEchiquier()
        {
            PictJeux.Reverse();             // On inverse les liste des PictureBox ce qui revient à faire une rotation à 180°
            Plateau.Image.RotateFlip(RotateFlipType.Rotate180FlipNone);
            Plateau.Refresh();              // On inverse le plateau
            LogiqueMouvements.DessinPieces();       // On dessine les pièces
            VisuCoteNoir = !VisuCoteNoir;   // On inverse le flag de côté de visualisation
        }

        private void MiseaZeroAffichages()
        {
            LogiqueMouvements.InitialisationEchiquier();
            LogiqueMouvements.ListeCoupsPgn.Clear();        // Mise à zéro des liste de coups PGN
            LogiqueMouvements.ListeCoupsPgnFr.Clear();      // Mise à zéro des liste de coups PGN Francais
            LogiqueMouvements.ListeCoupsFen.Clear();        // Mise à zéro des liste de coups FEN
            LogiqueMouvements.ListeCoupsNal.Clear();        // Mise à zéro des liste de coups Notation Algébrique Longue (Itnl)
            LogiqueMouvements.EchecetMat = LogiqueMouvements.Echec = false;
            NumeroLigne = 0;
            NumeroDemiCoup = 0;
            VarianteMoteurUci1.Text = VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = InformationPourJoueur.Text = "...";
            VarianteMoteurCourante.Text = ScoreMoteur.Text = EvaluationUci.Text = "...";
            BoutonGainBlanc.Enabled = BoutonGainNoir.Enabled = BoutonNulle.Enabled = true;
        }
    }
}