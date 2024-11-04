// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_Stockfish est développé par Bruno COURTOIS.  Copyright © 2024 █  
// █ BrunoGUI_Stockfish est gratuit, sauf s'il est utilisé commercialement  █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘
// Informations reflexion moteur - Temps de reflexion - Réglage force moteur
// Gestion par menus - Sauvegarde PGN - Affichage Score - Personnalisation couleurs 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BrunoGUI_Stockfish
{
    public delegate void AffichageCoupJoue(string Coup); 
    public delegate void AfficheInfo(string Chaine); 
    public delegate void AffichagePiece(int IndexCase, LogiqueMouvements.TypePiece Caractere); 
    public delegate void AffichageSymbole(int IndexCase, LogiqueMouvements.TypeSymbole Symbole); 
    public delegate void AffichagePrisePiece(string Couleur, int IndexCase); 
    public class LogiqueMouvements
    {
        // On joue à l'écran sur un échiquier de 64 cases
        // L'échiquier virtuel est un échiquier de 120 cases
        //
        // 110 111 112 113 114 115 116 117 118 119        -1  -1  -1  -1  -1  -1  -1  -1  -1  -1
        // 100 101 102 103 104 105 106 107 108 109        -1  -1  -1  -1  -1  -1  -1  -1  -1  -1
        //  90  91  92  93  94  95  96  97  98  99        -1  a8  b8  c8  d8  e8  f8  g8  h8  -1
        //  80  81  82  83  84  85  86  87  88  89        -1  a7  b7  c7  d7  e7  f7  g7  h7  -1
        //  70  71  72  73  74  75  76  77  78  79        -1  a6  b6  c6  d6  e6  f6  g6  h6  -1
        //  60  61  62  63  64  65  66  67  68  69        -1  a5  b5  c5  d5  e5  f5  g5  h5  -1
        //  50  51  52  53  54  55  56  57  58  59        -1  a4  b4  c4  d4  e4  f4  g4  h4  -1
        //  40  41  42  43  44  45  46  47  48  49        -1  a3  b3  c3  d3  e3  f3  g3  h3  -1
        //  30  31  32  33  34  35  36  37  38  39        -1  a2  b2  c2  d2  e2  f2  g2  h2  -1
        //  20  21  22  23  24  25  26  27  28  29        -1  a1  b1  c1  d1  e1  f1  g1  h1  -1
        //  10  11  12  13  14  15  16  17  18  19        -1  -1  -1  -1  -1  -1  -1  -1  -1  -1
        //   0   1   2   3   4   5   6   7   8   9        -1  -1  -1  -1  -1  -1  -1  -1  -1  -1
        //
        // L'échiquier de gauche représente les indices des Picturebox affichant les cases du jeu à l'écran ( voir Pictjeux comme List(of Picturebox) dans FenetreOutils ) 
        // L'échiquier de droite représente l'échiquier 120 cases avec les 64 cases réelles du jeu.
        // Les cases marquées d'une lettre suivie d'un chiffre (a1 ... h8) sont les 64 cases réelles de l'échiquier 64 cases.
        // La picturebox 21 sur l'échiquier de gauche représente la case 0 de l'échiquier 64 cases soit la case a1 du même échiquier 64 cases .
        // Les cases marquées -1 sur l'échiquier 120 cases sont en dehors de l'échiquier 64 cases et ne sont pas visibles .
        // Ces mêmes cases marquées -1 sont les cases TypePiece.Bordure : elles permettent de savoir si un déplacement déborde de l'échiquier.

        public static event AffichageCoupJoue AfficheCoupNoir;
        public static event AffichageCoupJoue AfficheCoupBlanc;
        public static event AfficheInfo AfficheInfoEchec;
        public static event AfficheInfo AfficheEchecEtMat;
        public static event AfficheInfo AfficheTour;
        public static event AfficheInfo AffichePromotionPion;
        public static event AfficheInfo AfficheFen;
        public static event AffichagePiece DessinePiece;
        public static event AffichageSymbole DessineSymbole;

        // énumération du contenu possible des cases de l'échiquier 120 cases
        // Remarque : les pièces noires sont paires et les pièces blanches sont impaires
        public enum TypePiece 
        {
            // les pièces
            RoiNoir,
            RoiBlanc,
            ReineNoire,
            ReineBlanche,
            FouNoir,
            FouBlanc,
            TourNoire,
            TourBlanche,
            CavalierNoir,
            CavalierBlanc,
            PionNoir,
            PionBlanc,
            // la case vide et la case bordure
            Vide,
            Bordure
        }
        public enum TypeSymbole         // les 4 symboles pour les mouvements et les menaces
        {
            SymboleMouvementInterdit, SymboleMouvementSansPrise, SymboleMouvementAvecPrise, SymboleMenacePiece
        }
        public enum ColorPiece          // Utiliser pour connaître la couleur d'une pièce sur une case
        {
            Vide, Blanc, Noir, BordPlateau
        }

        [Flags]
        // Pour gérer si les roques sont possibles ou non
        public enum FlagEnableRoque
        {
            AucunRoque,
            RoqueBlanc,     // pour le petit et le grand roque du roi blanc
            RoqueNoir       // pour le petit et le grand roque du roi noir 
        }
        // Identification des 4 mouvements de roque possibles : permet de savoir si le roi roque et quel roque il joue
        public enum FlagMouvementRoque
        {
            PasDeRoque, PetitRoqueNoir, GrandRoqueNoir, PetitRoqueBlanc, GrandRoqueBlanc
        }
        public static bool PetitRoqueBlancPossible { get; set; }
        public static bool GrandRoqueBlancPossible { get; set; }
        public static bool PetitRoqueNoirPossible { get; set; }
        public static bool GrandRoqueNoirPossible { get; set; }
        public static bool CoupValide { get; set; }
        public static bool BloquerChoixPromo { get; set; }
        public static bool StatutMoteurUci { get; set; } // true si MoteurUci a démarré
        public static bool Echec { get; set; }
        public static bool EchecetMat { get; set; }
        private static bool TestSecondPion { get; set; }
        private static bool FlagEnPassant { get; set; }
        public static string FenDepart { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        // Après le coup 1. e4 :        rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
        // Après le coup 1. ... c5 :    rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2
        // Après le coup 2. Cf3 :       rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2
        private static string MouvementCoup { get; set; }
        private static string MouvementCoupPgn { get; set; }
        private static string MouvementCoupNal { get; set; }
        private static FlagMouvementRoque Roque { get; set; }
        public static TypePiece PromotionPiece { get; set; }


        private static readonly Dictionary<FlagMouvementRoque, string> ListeCasesTraverseesRoi = new Dictionary<FlagMouvementRoque, string>// liste des cases traversées par le roi lors d'un roque
        {
            { FlagMouvementRoque.PetitRoqueBlanc, "f1" },   // petit roque blanc
            { FlagMouvementRoque.GrandRoqueBlanc, "d1" },   // grand roque blanc
            { FlagMouvementRoque.PetitRoqueNoir, "f8" },    // petit roque noir
            { FlagMouvementRoque.GrandRoqueNoir, "d8" }     // grand roque noir
        };
        private static readonly Dictionary<int, TypePiece> ListeMouvementsPiece = new Dictionary<int, TypePiece>(); // liste des déplacements possibles pour une pièce
        private static readonly Dictionary<int, TypePiece> ListeMenacesPiece = new Dictionary<int, TypePiece>();    // liste des menaces pour une pièce
        public static List<TypePiece> PiecesEchiquier { get; set; } = new List<TypePiece>();
        private static readonly List<string> ListePieces = new List<string> { "k", "K", "q", "Q", "b", "B", "r", "R", "n", "N", "p", "P" }; // minuscule = Noir et majuscule = Blanc
        private static readonly List<int> ListDeplacementsCavalier = new List<int>() { +8, +12, +19, +21, -8, -12, -19, -21 };
        private static readonly List<int> ListDeplacementsFou = new List<int>() { +9, -9, +11, -11 };
        private static readonly List<int> ListDeplacementsTour = new List<int>() { +1, -1, +10, -10 };
        private static readonly List<TypePiece> ListPiecesNoires = new List<TypePiece>() { TypePiece.TourNoire, TypePiece.CavalierNoir, TypePiece.FouNoir, TypePiece.ReineNoire, TypePiece.RoiNoir, TypePiece.FouNoir, TypePiece.CavalierNoir, TypePiece.TourNoire };
        private static readonly List<TypePiece> ListPiecesBlanches = new List<TypePiece>() { TypePiece.TourBlanche, TypePiece.CavalierBlanc, TypePiece.FouBlanc, TypePiece.ReineBlanche, TypePiece.RoiBlanc, TypePiece.FouBlanc, TypePiece.CavalierBlanc, TypePiece.TourBlanche };

        // Contient la liste de coups dans différents formats  
        public static List<string> ListeCoupsFen = new List<string>(); // https://www.pousseurdebois.fr/cours/notation-fen/
        public static List<string> ListeCoupsPgn = new List<string>(); // https://fr.wikipedia.org/wiki/Portable_Game_Notation
        public static List<string> ListeCoupsPgnFr = new List<string>(); // https://fr.wikipedia.org/wiki/Portable_Game_Notation
        public static List<string> ListeCoupsNal = new List<string>(); // Notation Algébrique longue (NAL)  

        // Information Fen
        public static ColorPiece QuiJoue { get; set; }          // le champ 2  : w ou b ( Blanc ou Noir ), indique la couleur qui a le trait
        public static FlagEnableRoque StatutRoque { get; set; } // le champ 3  : pour les 4 possibilités de roque
                                                                // KQkq signifie que les blancs peuvent faire le grand et le petit roque, idem pour les noirs.
                                                                // Si le roque n’est pas possible, marquer –
        public static int IndexCaseEnPassant { get; set; }      // le champ 4  : index de la case en passant si elle existe
                                                                // "–" signifie qu’aucune prise en passant n’est possible.
                                                                // Sil y en avait eu une, il aurait suffi de remplacer – par la case de capture en passant exemple f6.
        public static int SansPrise { get; set; }               // le champ 5  : +1 à chaque mouvement sans prise ou mouvement de pion
                                                                // Compteur qui compte le nombre de demi-coups depuis la dernière capture ou le dernier mouvement de pions.
                                                                // Ce compteur sert uniquement pour la règle des 50 coups.
        public static Single NombreCoupsJoues { get; set; }     //le champ 6  : on avance de 0.5 en 0.5 et on récupère la partie entière


        // Remplit le Fen à partir d'une string FEN au départ de la partie et affiche l'échiquier de départ
        public static void InitialisationEchiquier()
        {
            FlagEnPassant = false;
            TestSecondPion = false;
            PromotionPiece = TypePiece.Vide;
            MouvementCoup = string.Empty;
            QuiJoue = ColorPiece.Blanc;     // détermination du trait ( les blancs commencent )
            StatutRoque = FlagEnableRoque.RoqueBlanc | FlagEnableRoque.RoqueNoir; // détermination des droits au roque au départ ( les 2 rois peuvent roquer )
            PetitRoqueBlancPossible = PetitRoqueNoirPossible = GrandRoqueBlancPossible = GrandRoqueNoirPossible = true; // Tous roques possible 
            IndexCaseEnPassant = 0;         // détermination de la case e.p. ( pas de case en passant au départ )
            SansPrise = 0;                  // détermination du nombre de 1/2 coups sans mouvement de pion, ni prise de pièce
            NombreCoupsJoues = 1;           // détermination du numéro du coup à jouer, augmente de 0.5 en 0.5
            for (int i = 31; i <= 38; i++)
            {
                DessinePiecePlateau(i, TypePiece.PionBlanc);            // les 8 pions blancs
                DessinePiecePlateau(i + 50, TypePiece.PionNoir);        // les 8 pions noirs
                for (int j = 1; j < 5; j++)
                    DessinePiecePlateau(i + (j * 10), TypePiece.Vide);  // les 4 lignes vides
            }
            for (int i = 0; i < 8; i++)     // on place les autres pièces (e.g les "figures")
            {
                DessinePiecePlateau(i + 21, ListPiecesBlanches[i]);
                DessinePiecePlateau(i + 91, ListPiecesNoires[i]);
            }
        }

        // Dessine les piéces sur le plateau à l'initialisation
        public static void DessinePiecePlateau(int IndexCase, TypePiece Piece)
        {
            PiecesEchiquier[IndexCase] = Piece;
            DessinePiece(IndexCase, Piece);
        }
        //  Dessine toutes les pièces
        public static void DessinPieces()
        {
            for (int ligne = 2; ligne <= 9; ligne++)
            {
                for (int colonne = 1; colonne <= 8; colonne++)
                {
                    switch (PiecesEchiquier[(ligne * 10) + colonne])
                    {
                        case TypePiece.RoiBlanc:
                            DessinePiece((ligne * 10) + colonne, TypePiece.RoiBlanc);
                            break;
                        case TypePiece.ReineBlanche:
                            DessinePiece((ligne * 10) + colonne, TypePiece.ReineBlanche);
                            break;
                        case TypePiece.FouBlanc:
                            DessinePiece((ligne * 10) + colonne, TypePiece.FouBlanc);
                            break;
                        case TypePiece.CavalierBlanc:
                            DessinePiece((ligne * 10) + colonne, TypePiece.CavalierBlanc);
                            break;
                        case TypePiece.TourBlanche:
                            DessinePiece((ligne * 10) + colonne, TypePiece.TourBlanche);
                            break;
                        case TypePiece.PionBlanc:
                            DessinePiece((ligne * 10) + colonne, TypePiece.PionBlanc);
                            break;
                        case TypePiece.RoiNoir:
                            DessinePiece((ligne * 10) + colonne, TypePiece.RoiNoir);
                            break;
                        case TypePiece.ReineNoire:
                            DessinePiece((ligne * 10) + colonne, TypePiece.ReineNoire);
                            break;
                        case TypePiece.FouNoir:
                            DessinePiece((ligne * 10) + colonne, TypePiece.FouNoir);
                            break;
                        case TypePiece.CavalierNoir:
                            DessinePiece((ligne * 10) + colonne, TypePiece.CavalierNoir);
                            break;
                        case TypePiece.TourNoire:
                            DessinePiece((ligne * 10) + colonne, TypePiece.TourNoire);
                            break;
                        case TypePiece.PionNoir:
                            DessinePiece((ligne * 10) + colonne, TypePiece.PionNoir);
                            break;
                        case TypePiece.Bordure:
                            DessinePiece((ligne * 10) + colonne, TypePiece.Bordure);
                            break;
                        case TypePiece.Vide:
                            DessinePiece((ligne * 10) + colonne, TypePiece.Vide);
                            break;
                    }
                }
            }
        }

        public static void MiseenplaceFen(string Fenautiliser)     // Préparation du logiciel avec la position FEN
        {
            string[] ChampsFen = Fenautiliser.Split(' ');           // On récupère les 6 champs du FEN dans un tableau

            QuiJoue = ChampsFen[1] == "w" ?
                ColorPiece.Blanc :              // Champ 2 : Couleur au trait (w, c'est aux blancs de jouer)
                ColorPiece.Noir;                // (sinon c'est aux noirs de jouer )

            // Champ 6 : On met à jour le numéro du coup, x si c'est aux Blancs de jouer, x + 0.5 si c'est aux Noirs
            if (QuiJoue == ColorPiece.Blanc)            {
                NombreCoupsJoues = Int32.Parse(ChampsFen[5]);
            }
            else            {
                NombreCoupsJoues = Int32.Parse(ChampsFen[5]) + 0.5f;
            }    //  La lettre f après le nombre 0.5 indique que c'est un nombre à virgule flottante simple précision (un float). 

            // Champ 5 : nombre de demi-coups depuis la dernière capture ou le dernier mouvement de pion. !! Il faudra créer une variable et la mettre àjour !!

            // Champ 4 : mettre à jour IndexCaseEnPassant et FlagEnPassant 
            if (ChampsFen[3] == "-")
            {   // si pas de prise en passant
                IndexCaseEnPassant = 0;
            }
            else
            {   // si prise en passant, on prend l'index de la case de capture en passant 
                IndexCaseEnPassant = RenvoieCaseIndex120(ChampsFen[3]);
            }

            /* Champ 3 : Présence d'une lettre indique que le roque est possible; on utilise  respectivement les lettres 
            K et Q pour petit et grand roque blanc, et les lettres k et q pour les noirs. Si aucun roque n'est possible, on utilise "-" */
            string ChampRoque = ChampsFen[2];
            if (ChampRoque.Contains("-"))
            {
                StatutRoque = FlagEnableRoque.AucunRoque;       // Aucun roque possible
                PetitRoqueNoirPossible = PetitRoqueBlancPossible = GrandRoqueNoirPossible = GrandRoqueBlancPossible = false;
            }
            else if (ChampRoque.Contains("K"))
            {
                PetitRoqueBlancPossible = true;     // Petit roque blanc possible
                StatutRoque |= FlagEnableRoque.RoqueBlanc;
            }
            else if (ChampRoque.Contains("Q"))
            {
                GrandRoqueBlancPossible = true;     // Grand roque blanc possible
                StatutRoque |= FlagEnableRoque.RoqueBlanc;
            }
            else if (ChampRoque.Contains("k"))
            {
                PetitRoqueNoirPossible = true;     // Petit roque noir possible
                StatutRoque |= FlagEnableRoque.RoqueNoir;
            }
            else if (ChampRoque.Contains("q"))
            {
                GrandRoqueNoirPossible = true;     // Grand roque noir possible
                StatutRoque |= FlagEnableRoque.RoqueNoir;
            }

            string[] RangeesFen = ChampsFen[0].Split('/');          // Champ 1 : Décodage du Champ qui décrit la postion
            Array.Reverse(RangeesFen);
            for (int r = 0; r < 8; r++)                             // en 8 rangées qui sont celles de l'échiquier
            {
                char[] CaracteresFen = RangeesFen[r].ToCharArray(); // Décodage de chaque rangée par caractère
                int IndiceCases = 0;
                foreach (char c in CaracteresFen)
                {
                    if (Char.IsDigit(c))    // Si le caractère est un chiffre, 
                    {                       // on doit vider autant de cases que le chiffre
                        int Pasplus = (int)char.GetNumericValue(c);
                        for (int NombreCaseVide = 1; NombreCaseVide <= Pasplus; NombreCaseVide++)
                        {
                            IndiceCases++;
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.Vide;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.Vide);
                        }
                    }
                    else                                // Si le caractère n'est pas un chiffre
                        IndiceCases++;                  // Ce sont les pièces et les pions
                    switch (c)
                    {
                        case 'K':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.RoiBlanc;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.RoiBlanc);
                            break;
                        case 'Q':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.ReineBlanche;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.ReineBlanche);
                            break;
                        case 'B':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.FouBlanc;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.FouBlanc);
                            break;
                        case 'N':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.CavalierBlanc;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.CavalierBlanc);
                            break;
                        case 'R':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.TourBlanche;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.TourBlanche);
                            break;
                        case 'P':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.PionBlanc;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.PionBlanc);
                            break;
                        case 'k':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.RoiNoir;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.RoiNoir);
                            break;
                        case 'q':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.ReineNoire;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.ReineNoire);
                            break;
                        case 'b':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.FouNoir;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.FouNoir);
                            break;
                        case 'n':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.CavalierNoir;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.CavalierNoir);
                            break;
                        case 'r':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.TourNoire;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.TourNoire);
                            break;
                        case 'p':
                            PiecesEchiquier[20 + r * 10 + IndiceCases] = TypePiece.PionNoir;
                            DessinePiece(20 + r * 10 + IndiceCases, TypePiece.PionNoir);
                            break;
                    }
                }
            }
        }

        // Dessin des mouvements possibles pour une pièce ainsi que les menaces adverses pour cette même pièce 
        public static void DessineMouvements(string caseSource, bool visu)
        {
            TypeSymbole Symbole;
            int Indexcase;
            List<string> MouvementsPiece = RetourneMouvements(caseSource);
            if (MouvementsPiece != null)
                if (MouvementsPiece.Count > 0)
                    for (int i = 0; i <= MouvementsPiece.Count - 1; i++)
                    {
                        string CaseDestination = MouvementsPiece[i].Substring(MouvementsPiece[i].Length - 2, 2);
                        if (TestMouvementValide(caseSource, CaseDestination) == false)
                            Symbole = TypeSymbole.SymboleMouvementInterdit;             // Mouvement interdit
                        else
                        {
                            if (MouvementsPiece[i].StartsWith("x") == false)
                                Symbole = TypeSymbole.SymboleMouvementSansPrise; // Mouvement possible sans prise
                            else
                                Symbole = TypeSymbole.SymboleMouvementAvecPrise; // Mouvement possible avec prise
                        }
                        Indexcase = RenvoieCaseIndex120(CaseDestination);
                        ListeMouvementsPiece.Add(Indexcase, PiecesEchiquier[Indexcase]);
                        if (visu)
                            DessineSymbole(Indexcase, Symbole);
                    }
            List<string> Menaces = CasesPiecesMenacantes(caseSource);
            if (Menaces != null)
                if (Menaces.Count > 0)
                    for (int i = 0; i <= Menaces.Count - 1; i++)
                    {
                        Indexcase = RenvoieCaseIndex120(Menaces[i]);
                        ListeMenacesPiece.Add(Indexcase, PiecesEchiquier[Indexcase]);
                        // Pour l'instant, je n'affiche pas les menaces sur la pièce sélectionnée, ce n'est pas assez clair ... ??!!   DEBUG 26/02
                        //if (visu)
                        //    DessineSymbole(Indexcase, TypeSymbole.SymboleMenacePiece); // Menace la pièce sélectionnée
                    }
        }

        // Dessine une case vide pour le pion adverse capturé lors d'un mouvement en passant pour un pion
        public static void DessineCaseVide(int IndexCasePion, TypePiece Vide)
        {
            DessinePiece(IndexCasePion, Vide);
        }
        // Efface les symboles sur l'échiquier si ceux-ci sont visibles
        public static void EffaceSymboles(bool visu)
        {
            if (visu)
            {
                if (ListeMouvementsPiece.Count > 0)
                    EffaceListe(ListeMouvementsPiece); // symbole des mouvements 
                if (ListeMenacesPiece.Count > 0)
                    EffaceListe(ListeMenacesPiece); // symboles des menaces
            }
            ListeMouvementsPiece.Clear();
            ListeMenacesPiece.Clear();
        }
        // Efface les symboles à l'écran
        private static void EffaceListe(Dictionary<int, TypePiece> ListeEfface)
        {
            foreach (KeyValuePair<int, TypePiece> Element in ListeEfface)
                DessinePiece(Element.Key, Element.Value);
        }
        public static string RetourneChaineFenActuel()          // Retourne le FEN correspondant à la position actuelle
        {
            string LigneFen;
            List<string> ListLignesFen = new List<string>();
            int NombreCasesVides;
            for (int i = 90; i >= 20; i -= 10)
            {       // Création des 8 lignes sous la forme "rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R"
                LigneFen = string.Empty;
                NombreCasesVides = 0;
                for (int j = 1; j <= 8; j++)
                {
                    if (PiecesEchiquier[i + j] != TypePiece.Vide)
                    {
                        if (NombreCasesVides > 0)
                        {
                            LigneFen += NombreCasesVides.ToString();
                            NombreCasesVides = 0;
                        }
                        LigneFen += ListePieces[(int)PiecesEchiquier[i + j]];
                    }
                    else
                        NombreCasesVides++;
                }
                if (NombreCasesVides > 0)
                    LigneFen += NombreCasesVides.ToString();
                ListLignesFen.Add(LigneFen);
            }
            // rajoute les infos FEN pour les 5 autres champs
            string ChaineFen = string.Join("/", ListLignesFen) + " " + (QuiJoue == ColorPiece.Blanc ? "w" : "b") + " ";
            if (PetitRoqueBlancPossible)
            {
                ChaineFen += "K";
            }
            if (GrandRoqueBlancPossible)
            {
                ChaineFen += "Q";
            }
            if (PetitRoqueNoirPossible)
            {
                ChaineFen += "k";
            }
            if (GrandRoqueNoirPossible)
            {
                ChaineFen += "q";
            }
            if (StatutRoque == FlagEnableRoque.AucunRoque)
            {
                ChaineFen += "-";
            }
            return ChaineFen + " " + (IndexCaseEnPassant != 0 ? NomCaseAlgebrique(IndexCaseEnPassant) : "-") + " " + SansPrise + " " + Math.Truncate(NombreCoupsJoues).ToString();
        }

        // Exécute un coup pour le joueur humain ou le moteur UCI
        public static void ExecutionCoup(string caseSource, string caseDestination)
        {
            string CouleurEchec = string.Empty;
            CoupValide = false;
            EchecetMat = false;
            MouvementCoup = string.Empty;
            if (caseSource != caseDestination)
            {
                if (TestMouvementValide(caseSource, caseDestination))
                {
                    AfficheInfoEchec(string.Empty);
                    MouvementCoup = CoupNotationAlgebriquePGN(caseSource, caseDestination);
                    Console.WriteLine($"Mouvement Coup Francais joué : {MouvementCoup}, Coup valide : {CoupValide}");                           // 01/02  DEBUG
                    FaireMouvement(caseSource, caseDestination);

                    string ChaineFen = RetourneChaineFenActuel();       // Remplissage de liste de coups FEN
                    ListeCoupsFen.Add(ChaineFen);                       // Mise en liste des FEN

                    char[] decoupe = MouvementCoup.ToCharArray();       // Remplissage de liste de coups PGN
                    for (int i = 0; i < decoupe.Length; i++)
                    {                                                   // On traduit le coup en notation internationale
                        if (decoupe[i] == 'R') decoupe[i] = 'K';
                        if (decoupe[i] == 'D') decoupe[i] = 'Q';
                        if (decoupe[i] == 'T') decoupe[i] = 'R';
                        if (decoupe[i] == 'F') decoupe[i] = 'B';
                        if (decoupe[i] == 'C') decoupe[i] = 'N';
                    }
                    MouvementCoupPgn = new string(decoupe);
                    // Traitement de la notation algébrique longue
                    if (MouvementCoupPgn.Contains('x'))
                    {
                        if (char.IsLower(MouvementCoupPgn[0]))
                        {   // C'est un coup de Pion
                            MouvementCoupNal = caseSource + 'x' + MouvementCoupPgn[2] + MouvementCoupPgn[3];
                        }
                        else
                        {   // C'est un coup de Pièce
                            MouvementCoupNal = MouvementCoupPgn[0] + caseSource + 'x' + MouvementCoupPgn[2] + MouvementCoupPgn[3];
                        }
                    }
                    else
                    {   // Pas de prise
                        if (char.IsLower(MouvementCoupPgn[0]))
                        {   // C'est un coup de Pion
                            MouvementCoupNal = caseSource + '-' + MouvementCoupPgn;
                        }
                        else
                        {   // C'est un coup de Pièce
                            if (MouvementCoupPgn.Contains('O'))
                            {   // C'est un roque
                                MouvementCoupNal = MouvementCoupPgn;
                            }
                            else
                            {   // Ce n'est pas un roque
                                MouvementCoupNal = MouvementCoupPgn[0] + caseSource + '-' + MouvementCoupPgn[1] + MouvementCoupPgn[2];
                            }
                        }
                    }
                    // Traitement de la prise en passant
                    if (FlagEnPassant)
                    {                                                           // Correction de bug par ajout de caseSource
                        MouvementCoup = caseSource[0] + "x" + MouvementCoup;       // on enlève " e.p"; pas d'indication de la prise en passant en PGN
                        MouvementCoupPgn = caseSource[0] + "x" + MouvementCoupPgn;
                        MouvementCoupNal = caseSource + "x" + MouvementCoupPgn + "e.p";
                        if (TestSecondPion)
                        {       // on précise la colonne car deux pions peuvent faire la prise en passant
                            MouvementCoup = caseSource.Substring(0, 1) + MouvementCoup;
                            MouvementCoupPgn = caseSource.Substring(0, 1) + MouvementCoupPgn;
                            MouvementCoupNal = caseSource.Substring(0, 1) + MouvementCoupPgn;
                        }
                    }
                    FlagEnPassant = false;

                    string[] FenTableau = ChaineFen.Split(' '); // On decoupe les champs du FEN
                    string NumeroCoup = FenTableau[5];  // Champ 6 = numéro du coup de la partie (incrémenté à chaque coup des blancs)
                    if (EchecetMat)
                    {
                        if (FenTableau[1] == "b")   // Champ 1 =  couleur au trait: w si c'est aux blancs de jouer, b pour les noirs
                        {
                            ListeCoupsPgn.Add(NumeroCoup + ". " + MouvementCoupPgn + "# "); // C'est mat, il faut mettre le # pour l'indiquer
                            ListeCoupsPgnFr.Add(NumeroCoup + ". " + MouvementCoup + "# ");  // dans les 2 listes de coups, internationale et francaise
                            ListeCoupsNal.Add(NumeroCoup + ". " + MouvementCoupNal + "# ");
                        }
                        if (FenTableau[1] == "w")   // Champ 1 =  couleur au trait: w si c'est aux blancs de jouer, b pour les noirs
                        {
                            ListeCoupsPgn.Add(MouvementCoupPgn + "# ");                     // C'est mat, il faut mettre le # pour l'indiquer
                            ListeCoupsPgnFr.Add(MouvementCoup + "# ");                      // dans les 2 listes de coups, internationale et francaise
                            ListeCoupsNal.Add(MouvementCoupNal + "# ");
                        }
                    }
                    else
                        if (Echec)
                    {
                        if (FenTableau[1] == "b")   // Champ 1 =  couleur au trait: w si c'est aux blancs de jouer, b pour les noirs
                        {
                            ListeCoupsPgn.Add(NumeroCoup + ". " + MouvementCoupPgn + "+ "); // C'est Echec, il faut mettre le + pour l'indiquer
                            ListeCoupsPgnFr.Add(NumeroCoup + ". " + MouvementCoup + "+ ");  // dans les 2 listes de coups, internationale et francaise
                            ListeCoupsNal.Add(NumeroCoup + ". " + MouvementCoupNal + "+ ");
                        }
                        if (FenTableau[1] == "w")   // Champ 1 =  couleur au trait: w si c'est aux blancs de jouer, b pour les noirs
                        {
                            ListeCoupsPgn.Add(MouvementCoupPgn + "+ ");                     // C'est Echec, il faut mettre le + pour l'indiquer
                            ListeCoupsPgnFr.Add(MouvementCoup + "+ ");                      // dans les 2 listes de coups, internationale et francaise
                            ListeCoupsNal.Add(MouvementCoupNal + "+ ");
                        }
                    }
                    else
                    {
                        if (FenTableau[1] == "b")   // Champ 1 =  couleur au trait: w si c'est aux blancs de jouer, b pour les noirs
                        {
                            ListeCoupsPgn.Add(NumeroCoup + ". " + MouvementCoupPgn + " ");  // Ni Echec, ni Mat, il faut mettre un espace
                            ListeCoupsPgnFr.Add(NumeroCoup + ". " + MouvementCoup + " ");   // dans les 2 listes de coups, internationale et francaise
                            ListeCoupsNal.Add(NumeroCoup + ". " + MouvementCoupNal + " ");
                        }
                        if (FenTableau[1] == "w")   // Champ 1 =  couleur au trait: w si c'est aux blancs de jouer, b pour les noirs
                        {
                            ListeCoupsPgn.Add(MouvementCoupPgn + " ");                      // Ni Echec, ni Mat, il faut mettre un espace
                            ListeCoupsPgnFr.Add(MouvementCoup + " ");                       // dans les 2 listes de coups, internationale et francaise
                            ListeCoupsNal.Add(MouvementCoupNal + " ");
                        }
                    }

                    CouleurEchec = QuiJoue == ColorPiece.Noir ? "Noir" : "Blanc";
                    // Affiche si le roi est en échec
                    if (Echec)
                    {
                        AfficheInfoEchec("Le roi " + CouleurEchec + " est en échec");
                    }
                    if (QuiJoue == ColorPiece.Noir)
                    {
                        AfficheCoupBlanc(MouvementCoup);
                    }
                    else
                    {
                        AfficheCoupNoir(MouvementCoup);
                    }
                    // Console.WriteLine($"Execution Coup Pgn International : {ListeCoupsPgn[ListeCoupsPgn.Count - 1]}, Source : {caseSource}, Destination : {caseDestination}");    // 01/02  DEBUG
                }
            }
            if (MouvementCoup != string.Empty)
            {
                CoupValide = true;
                if (Echec)
                {   // si il y a échec on teste s'il reste des coups valides à jouer sinon il y a échec et mat
                    if (ResteCoupsValidesJouables() == false)
                    {
                        EchecetMat = true;
                        AfficheEchecEtMat(CouleurEchec);
                    }
                }
                else
                {   // sinon on affiche si le joueur qui a la couleur est Pat ( plus de coups valides jouables + le joueur n'est pas échec )
                    if (ResteCoupsValidesJouables() == false)
                        AfficheInfoEchec("Le joueur " + CouleurEchec + " est Pat - plus de coup possible ");
                }
            }
        }

        // Retourne la liste des pièces pouvant prendre la pièce sur la case passée en paramètre
        private static List<string> CasesPiecesMenacantes(string CaseEchiquierMenacee)
        {
            ColorPiece CouleurMenace;
            List<string> CaseMenaces = new List<string>();
            int IndexCase = RenvoieCaseIndex120(CaseEchiquierMenacee);
            if (PiecesEchiquier[IndexCase] == TypePiece.Vide)
                CouleurMenace = CouleurAdversaireJoueurCourant(); // on vérifie pour le roque
            else
                CouleurMenace = CouleurAdversaireCase(IndexCase); // on cherche les pièces de la couleur adverse à celle ci
            for (int i = 21; i <= 98; i++)
                if (PiecesEchiquier[i] != TypePiece.Bordure)
                    if (CouleurCase(i) == CouleurMenace) // pour chaque pièce adverse 
                        if (PrisesPossibles(NomCaseAlgebrique(i)).Contains(CaseEchiquierMenacee))
                            CaseMenaces.Add(NomCaseAlgebrique(i));
            return CaseMenaces;
        }

        // Renvoie la liste des prises possibles pour une case 
        private static List<string> PrisesPossibles(string CaseEchiquier)
        {
            List<string> Prises = new List<string>();
            List<string> Mouvements = RetourneMouvements(CaseEchiquier); // récupère l'ensemble des coups pour une case
            if (Mouvements.Count > 0)
            {
                Prises = new List<string>();
                for (int i = 0; i <= Mouvements.Count - 1; i++)
                    if (Mouvements[i].StartsWith("x")) // si c'est une prise
                        Prises.Add(Mouvements[i].Substring(1, 2)); // on ajoute la case sans le x à la liste
            }
            return Prises;
        }

        // Teste la validité d'un coup
        private static bool TestMouvementValide(string caseSource, string caseDestination)
        {
            // reçoit un mouvement du type e2e4
            int IndexSource = RenvoieCaseIndex120(caseSource);
            int IndexDestination = RenvoieCaseIndex120(caseDestination);
            if (CouleurCase(IndexSource) != QuiJoue)
                return false;
            List<string> MouvementsPossibles = RetourneMouvements(caseSource);
            string CaseAtteinte = MouvementsPossibles.Find(x => x.Contains(caseDestination));
            if (CaseAtteinte != null)
            {
                Roque = TestSiRoque(IndexSource, IndexDestination);
                if (Roque != FlagMouvementRoque.PasDeRoque)
                {   // Teste si le roque est un mouvement valide : la case que le roi traverse ne doit pas être contrôlée par une pièce adverse 
                    if (TestMouvementValide(caseSource, ListeCasesTraverseesRoi[Roque]) == false)
                        return false;
                }
                // On teste ensuite si le roi est en échec ou pas suite au déplacement de la pièce jouée
                TypePiece BackupPiece = PiecesEchiquier[IndexDestination];  // sauvegarde de la pièce
                DeplacementPiece(IndexSource, IndexDestination, false);     // on simule le déplacement de la pièce sur l'échiquier
                // On cherche l'emplacement du roi du joueur courant 
                string CaseRoi = QuiJoue == ColorPiece.Noir ? PositionRoi(TypePiece.RoiNoir) : PositionRoi(TypePiece.RoiBlanc);
                // Le roi peut-il être en échec ?
                List<string> Menaces = CasesPiecesMenacantes(CaseRoi);
                DeplacementPiece(IndexDestination, IndexSource, false);     // on remet la pièce à sa place sur l'échiquier
                PiecesEchiquier[IndexDestination] = BackupPiece;            // on récupère la pièce
                return (Menaces.Count == 0);
            }
            else
                return false;
        }
        public static bool TripleRepetition()       // Détection de la triple répétition des coups qui donne partie nulle !
        {
            Dictionary<string, int> positionsVues = new Dictionary<string, int>();
            // Parcourir la liste des FEN pour détecter la triple répétition
            foreach (string fen in ListeCoupsFen)
            {
                // Récupérer la partie de la FEN qui représente l'état du plateau
                string[] elements = fen.Split(' ');
                string positionPlateau = elements[0];
                // Vérifier si la position est déjà dans le dictionnaire
                if (positionsVues.ContainsKey(positionPlateau))
                {
                    positionsVues[positionPlateau]++;
                }
                else
                {
                    positionsVues[positionPlateau] = 1;
                }
                // Vérifier si la position a été vue trois fois
                if (positionsVues[positionPlateau] >= 3)
                {
                    return true; // Triple répétition détectée
                }
            }
            return false; // Pas de triple répétition
        }
        // Déplace une pièce dans le tableau des pièces avec les 2 index des cases
        // Si visu = true le déplacement se fait aussi sur le plateau de jeu à l'écran
        public static void DeplacementPiece(int IndexSource, int IndexDestination, bool visu)
        {
            PiecesEchiquier[IndexDestination] = PiecesEchiquier[IndexSource];
            PiecesEchiquier[IndexSource] = TypePiece.Vide;
            if (visu)
            {
                DessinePiece(IndexDestination, PiecesEchiquier[IndexDestination]);
                DessinePiece(IndexSource, TypePiece.Vide);
            }
        }
        // Retourne tous les déplacements possibles pour une case selon la pièce sur cette case
        public static List<string> RetourneMouvements(string CaseEchiquier)
        {
            int indexCase = RenvoieCaseIndex120(CaseEchiquier);
            switch (PiecesEchiquier[indexCase])
            {
                case TypePiece.ReineBlanche:
                case TypePiece.ReineNoire:
                    return MouvementsReine(indexCase);
                case TypePiece.RoiBlanc:
                case TypePiece.RoiNoir:
                    return MouvementsRoi(indexCase);
                case TypePiece.FouBlanc:
                case TypePiece.FouNoir:
                    return MouvementsPiece(indexCase, ListDeplacementsFou);
                case TypePiece.CavalierBlanc:
                case TypePiece.CavalierNoir:
                    return MouvementsCavalier(indexCase, ListDeplacementsCavalier);
                case TypePiece.TourBlanche:
                case TypePiece.TourNoire:
                    return MouvementsPiece(indexCase, ListDeplacementsTour);
                case TypePiece.PionBlanc:
                case TypePiece.PionNoir:
                    return MouvementsPion(indexCase);
                default:
                    return null;
            }
        }
        // Déplacements du cavalier
        private static List<string> MouvementsCavalier(int IndexCase, List<int> listDeplacementsCavalier)
        {
            List<string> Mouvements = new List<string>();
            for (int i = 0; i <= listDeplacementsCavalier.Count - 1; i++) // les directions du déplacement
                AjouteMouvements(IndexCase, IndexCase + listDeplacementsCavalier[i], Mouvements, true);
            return Mouvements;
        }
        // Déplacements de la reine ( sert aussi pour le Roi mais d'une seule case )
        public static List<string> MouvementsReine(int IndexCase)
        {
            // on combine les Mouvements du fou et de la tour
            List<string> Mouvements = MouvementsPiece(IndexCase, ListDeplacementsFou);
            Mouvements.AddRange(MouvementsPiece(IndexCase, ListDeplacementsTour));
            return Mouvements;
        }
        // Déplacements d'une tour ou d'un fou
        private static List<string> MouvementsPiece(int IndexCase, List<int> ListDeplacements)
        {
            List<string> Mouvements = new List<string>();
            for (int i = 0; i <= ListDeplacements.Count - 1; i++) // les directions du déplacement
                AjouteMouvements(IndexCase, ListDeplacements[i], Mouvements);
            return Mouvements;
        }
        // Ajoute les coups possibles pour le roi, la reine, le fou et la tour 
        public static void AjouteMouvements(int IndexCaseSource, int Direction, List<string> Mouvements)
        {
            int IndexCaseSuivante = IndexCaseSource + Direction;
            while (PiecesEchiquier[IndexCaseSuivante] != TypePiece.Bordure && PiecesEchiquier[IndexCaseSuivante] == TypePiece.Vide)
            {
                Mouvements.Add(NomCaseAlgebrique(IndexCaseSuivante));
                if (PiecesEchiquier[IndexCaseSource] == TypePiece.RoiBlanc || PiecesEchiquier[IndexCaseSource] == TypePiece.RoiNoir)
                    return; // si c'est un roi on s'arrête 
                IndexCaseSuivante += Direction;
            }
            if (CouleurCase(IndexCaseSuivante) == CouleurAdversaireCase(IndexCaseSource))
                Mouvements.Add("x" + NomCaseAlgebrique(IndexCaseSuivante));
        }
        // Ajoute les coups possibles pour le cavalier et les pions
        public static void AjouteMouvements(int IndexCaseSource, int IndexCaseDestination, List<string> Mouvements, bool AvecPrise)
        {
            if (PiecesEchiquier[IndexCaseDestination] != TypePiece.Bordure)
            {
                if (PiecesEchiquier[IndexCaseDestination] == TypePiece.Vide)
                    Mouvements.Add(NomCaseAlgebrique(IndexCaseDestination));
                if (AvecPrise && CouleurCase(IndexCaseDestination) == CouleurAdversaireCase(IndexCaseSource))
                    Mouvements.Add("x" + NomCaseAlgebrique(IndexCaseDestination));
            }
        }
        // Teste si la couleur a encore des coups valides jouables (retourne True si c'est le cas)
        // Utilisation de cette fonction dans 2 cas
        // 1) le roi est en échec et le joueur n'a plus aucun coup valide : le joueur est échec et mat
        // 2) le roi n'est pas en échec mais le joueur n'a plus aucun coup valide : le joueur est donc pat
        private static bool ResteCoupsValidesJouables()
        {
            int NbMouvements = 0;
            for (int i = 21; i <= 98; i++)
                if (PiecesEchiquier[i] != TypePiece.Vide && PiecesEchiquier[i] != TypePiece.Bordure)
                    NbMouvements += RetourneMouvementsValides(NomCaseAlgebrique(i)).Count;
            return (NbMouvements > 0);
        }
        // Retourne les coups valides pour une case
        private static List<string> RetourneMouvementsValides(string CaseEchiquier)
        {
            List<string> MouvementsValides = new List<string>();
            List<string> Mouvements = RetourneMouvements(CaseEchiquier);
            if (Mouvements.Count > 0)
                for (int i = 0; i <= Mouvements.Count - 1; i++)
                    if (TestMouvementValide(CaseEchiquier, Mouvements[i].Substring(Mouvements[i].Length - 2, 2)))
                        MouvementsValides.Add(Mouvements[i]);
            return MouvementsValides;
        }
        // Effectue un mouvement de pièce sans vérifier la validité du mouvement
        private static void FaireMouvement(string caseSource, string caseDestination)
        {
            // reçoit un mouvement du type e2e4
            int IndexSource = RenvoieCaseIndex120(caseSource);  // convertit la case source en index
            int IndexDestination = RenvoieCaseIndex120(caseDestination); // convertit la case destination en index
            {
                // On vérifie que le mouvement n'est pas le roque
                Roque = TestSiRoque(IndexSource, IndexDestination);
                if (Roque != FlagMouvementRoque.PasDeRoque)
                    MouvementsPourRoque(Roque);
                else
                {                       // Le mouvement n'est pas le roque
                    // En passant
                    if (TestEnPassant(IndexSource, IndexDestination))
                        MouvementsEnPassant(IndexSource);
                    // Ajoute la case en passant
                    if (Math.Abs(IndexSource - IndexDestination) == 20 && (PiecesEchiquier[IndexSource] == TypePiece.PionBlanc || PiecesEchiquier[IndexSource] == TypePiece.PionNoir))
                        IndexCaseEnPassant = Convert.ToInt32((IndexSource + IndexDestination) / (double)2);   // la case entre les deux est la case en passant )
                    else
                        IndexCaseEnPassant = 0;
                    // Augmente ou remet à 0 pour les 50 coups
                    if (PiecesEchiquier[IndexSource] == TypePiece.PionBlanc || PiecesEchiquier[IndexDestination] == TypePiece.PionBlanc || PiecesEchiquier[IndexSource] == TypePiece.PionNoir || PiecesEchiquier[IndexDestination] == TypePiece.PionNoir)
                        SansPrise = 0;
                    else
                        SansPrise += 1;

                    // Si le roi blanc peut encore roquer 
                    if (StatutRoque.HasFlag(FlagEnableRoque.RoqueBlanc))
                        if (PiecesEchiquier[IndexSource] == TypePiece.RoiBlanc)
                        {
                            StatutRoque ^= FlagEnableRoque.RoqueBlanc;      // Si le Roi blanc bouge, plus de roque blanc possible
                            PetitRoqueBlancPossible = GrandRoqueBlancPossible = false;
                        }
                    if (PiecesEchiquier[IndexSource] == TypePiece.TourBlanche)
                    {
                        if (IndexSource == 21)                          // Si la tour en a1 bouge, plus de grand roque possiblle
                        {
                            GrandRoqueBlancPossible = false;
                        }
                        if (IndexSource == 28)                                  // Si la tour en h1 bouge, plus de petit roque possiblle
                        {
                            PetitRoqueBlancPossible = false;
                        }
                    }
                    // Si le roi noir peut encore roquer 
                    if (StatutRoque.HasFlag(FlagEnableRoque.RoqueNoir))
                        if (PiecesEchiquier[IndexSource] == TypePiece.RoiNoir)
                        {
                            StatutRoque ^= FlagEnableRoque.RoqueNoir;      // Si le Roi noir bouge, plus de roque noir possible
                            PetitRoqueNoirPossible = GrandRoqueNoirPossible = false;
                        }
                    if (PiecesEchiquier[IndexSource] == TypePiece.TourNoire)
                    {
                        if (IndexSource == 91)                          // Si la tour en a8 bouge, plus de grand roque possiblle
                        {
                            GrandRoqueNoirPossible = false;
                        }
                        if (IndexSource == 98)                          // Si la tour en h8 bouge, plus de petit roque possiblle
                        {
                            PetitRoqueNoirPossible = false;
                        }
                    }
                    // test si promotion d'un pion
                    string CouleurPromotion = string.Empty;
                    if (MouvementCoup.EndsWith("="))
                    {
                        if (IndexDestination < 29)
                            CouleurPromotion = "Noir";
                        if (IndexDestination > 90)
                            CouleurPromotion = "Blanc";
                        if (CouleurPromotion != string.Empty)
                        {
                            if (BloquerChoixPromo == false)
                            {
                                AffichePromotionPion(CouleurPromotion);
                            }
                            // promotion d'un pion noir ou blanc
                            DessinePiece(IndexDestination, PromotionPiece); // On affiche la pièce promue
                            MouvementCoup += NomsPieceLocale(PromotionPiece);
                            PiecesEchiquier[IndexDestination] = PromotionPiece;
                            // test si la pièce promue met en échec le roi adverse
                            Echec = TestEchecPromotionPion(IndexDestination, CouleurPromotion == "Noir" ? TypePiece.RoiBlanc : TypePiece.RoiNoir);
                        }
                        PiecesEchiquier[IndexSource] = TypePiece.Vide;
                        DessinePiece(IndexSource, TypePiece.Vide);          // On efface la case d'origine
                    }
                    else
                        DeplacementPiece(IndexSource, IndexDestination, true);
                }
                // changement de joueur
                QuiJoue = (QuiJoue == ColorPiece.Blanc) ? ColorPiece.Noir : ColorPiece.Blanc;
                AfficheTour((QuiJoue == ColorPiece.Blanc) ? "Blancs" : "Noirs");
                NombreCoupsJoues += Convert.ToSingle(0.5);      // On incrémente d'un demi-coup
                Console.WriteLine($"Nombre coups joues : {NombreCoupsJoues}, Couleur à jouer : {QuiJoue} ");
            }
        }
        // Retourne le nom des pièces françaises ( RNBQR in anglais et TCFDR in français)
        private static string NomsPieceLocale(TypePiece lettreInitiale)
        {
            switch (lettreInitiale)
            {
                case TypePiece.TourBlanche:
                case TypePiece.TourNoire:
                    return "T"; // Tour
                case TypePiece.CavalierBlanc:
                case TypePiece.CavalierNoir:
                    return "C"; // Cavalier
                case TypePiece.FouBlanc:
                case TypePiece.FouNoir:
                    return "F"; // Fou
                case TypePiece.ReineBlanche:
                case TypePiece.ReineNoire:
                    return "D"; // Reine
                case TypePiece.RoiBlanc:
                case TypePiece.RoiNoir:
                    return "R"; // Roi
                default:
                    return string.Empty;
            }
        }
        // Retourne le coup joué par l'humain ou le moteur UCI en notation algébrique
        public static string CoupNotationAlgebriquePGN(string caseSource, string CaseDestination)
        {
            // reçoit un mouvement du type e2e4
            int IndexSource = RenvoieCaseIndex120(caseSource);  // convertit la case source en index
            int IndexDestination = RenvoieCaseIndex120(CaseDestination); // convertit la case destination en index
            TypePiece Piece = PiecesEchiquier[IndexSource];  // récupère la piece qui bouge
            string MouvementSpecifique = string.Empty;
            string MouvementParDefaut = NomsPieceLocale(Piece) + (PiecesEchiquier[IndexDestination] == TypePiece.Vide ? string.Empty : "x") + CaseDestination;
            switch (Piece) // en fonction du type de pièce
            {
                case TypePiece.CavalierBlanc:
                case TypePiece.CavalierNoir:
                case TypePiece.TourBlanche:
                case TypePiece.TourNoire: // si c'est un cavalier ou une tour   
                    string AutrePosition = NomCaseAlgebrique(PositionSecondePiece(IndexSource)); // cherche la position de l'autre cavalier ou de l'autre tour
                    if (AutrePosition != string.Empty)
                    {
                        List<string> AutreCaseDestinations = RetourneMouvements(AutrePosition); // récupère les mouvements possibles de l'autre pièce
                        // on enlève le "x" qui signifie la prise
                        for (int i = 0; i <= AutreCaseDestinations.Count - 1; i++)
                            if (AutreCaseDestinations[i].StartsWith("x"))
                                AutreCaseDestinations[i] = AutreCaseDestinations[i].Substring(1);
                        if (AutreCaseDestinations.Contains(CaseDestination))
                        {
                            if (caseSource.Substring(0, 1) != AutrePosition.Substring(0, 1))
                                // si les deux pièces ne sont pas sur les mêmes colonnes
                                MouvementSpecifique = NomsPieceLocale(Piece) + caseSource.Substring(0, 1) + (PiecesEchiquier[IndexDestination] == TypePiece.Vide ? string.Empty : "x") + CaseDestination;
                            else
                                // sinon il faut rajouter la ligne pour spécifier la bonne pièce car les 2 pièces sont sur la même colonne 
                                MouvementSpecifique = NomsPieceLocale(Piece) + caseSource.Substring(1, 1) + (PiecesEchiquier[IndexDestination] == TypePiece.Vide ? string.Empty : "x") + CaseDestination;
                        }
                    }
                    break;
                case TypePiece.PionBlanc:
                case TypePiece.PionNoir: // Si c'est un pion
                    if (PiecesEchiquier[IndexDestination] == TypePiece.Vide)
                        // supprimer la case de départ en cas de déplacement d'un pion sans prise 
                        MouvementSpecifique = CaseDestination;
                    else
                        // Ajouter la colonne de départ en cas de prise de pion
                        MouvementSpecifique = caseSource.Substring(0, 1) + "x" + CaseDestination;
                    // voir si promotion pion 
                    if (IndexDestination < 29 || IndexDestination > 90)
                        MouvementSpecifique += "=";
                    break;
                case TypePiece.RoiBlanc:
                case TypePiece.RoiNoir: // Si c'est un roi
                                        // Vérifier si le roi roque
                    switch (caseSource + CaseDestination)
                    {
                        case "e8g8":
                        case "e1g1":
                            MouvementSpecifique = "O-O";  // petit roque
                            break;
                        case "e8c8":
                        case "e1c1":
                            MouvementSpecifique = "O-O-O"; // grand roque
                            break;
                    }
                    break;
            }
            TypePiece BackupPiece = PiecesEchiquier[IndexDestination];  // sauvegarde de la pièce de destination
            // test si prise d'une pièce à l'adversaire
            DeplacementPiece(IndexSource, IndexDestination, false); // simulation déplacement pièce
            // Test si le roi est en échec
            string CaseRoi = QuiJoue == ColorPiece.Blanc ? PositionRoi(TypePiece.RoiNoir) : PositionRoi(TypePiece.RoiBlanc);
            List<string> Menaces = CasesPiecesMenacantes(CaseRoi);
            Echec = (Menaces.Count > 0);
            DeplacementPiece(IndexDestination, IndexSource, false); // on remet la pièce déplacée à sa place de départ
            PiecesEchiquier[IndexDestination] = BackupPiece; // on récupère la pièce d'origine
            return MouvementSpecifique == string.Empty ? MouvementParDefaut : MouvementSpecifique;
        }
        // Renvoie la position de la seconde pièce ( -1 si elle n'est pas présente )
        // Valable pour tour et cavalier (pas le fou, car chaque fou est sur une case de couleur différente )
        private static int PositionSecondePiece(int IndexCase)
        {
            TypePiece Piece = PiecesEchiquier[IndexCase];
            if (Piece != TypePiece.Vide)
                for (int i = 0; i <= 99; i++)
                    if (PiecesEchiquier[i] == Piece && i != IndexCase)
                        return i;
            return -1;
        }
        // Renvoie le nom d'une case sous la forme e2 ou un caractère vide en cas d'erreur
        public static string NomCaseAlgebrique(int IndexCase)
        {
            if (IndexCase != -1)
                return Convert.ToChar((IndexCase % 10) + 96) + ((IndexCase / 10) - 1).ToString();
            //  Le reste de IndexCase /10 + 96 pour avoir ASCII   + IndexCase / 10 - 1 pour avoir la ligne
            else
                return string.Empty;
        }
        // Renvoie l'index d'une case à partir de son nom (sous la forme "e2")
        public static int RenvoieCaseIndex120(string NomCase) // Crash quand il y a Mat :-- 'Le format de la chaîne d'entrée est incorrect.'
        {
            try
            {
                return (Convert.ToInt32(NomCase.Substring(1, 1)) * 10) + (Convert.ToInt32(Convert.ToChar(NomCase.Substring(0, 1))) - 96) + 10;
                //            on multiplie le numéro de ligne par 10  :  on convertit le code ASCII en entier, puis -96   : on ajoute 10       
                // Note : Code ASCII de a = 97, code ASCII de e  = 101, d’où le - 96 !! Pour avoir a = 1, b = 2, c= 3,… e = 5
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur s'est produite : {ex.Message}", "dans RenvoieCaseIndex120", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Console.WriteLine($"StackTrace : {ex.StackTrace}");
                return -1;
            }
        }
        // Renvoie la couleur de l'adversaire du joueur courant
        public static ColorPiece CouleurAdversaireJoueurCourant()
        {
            return (QuiJoue == ColorPiece.Blanc ? ColorPiece.Noir : ColorPiece.Blanc);
        }
        // Renvoie la couleur de l'adversaire selon la couleur d'une case
        public static ColorPiece CouleurAdversaireCase(int IndexCase)
        {
            ColorPiece CouleurPiece = CouleurCase(IndexCase);
            if (CouleurPiece != ColorPiece.Vide && CouleurPiece != ColorPiece.BordPlateau)
                // case avec une pièce ( on prend la couleur de l'adversaire )
                return CouleurPiece == ColorPiece.Noir ? ColorPiece.Blanc : ColorPiece.Noir;
            else
                // case vide ou case bordure
                return CouleurPiece;
        }
        // Renvoie la couleur de la pièce sur la case ( 0 : vide , 1 :blanc, 2 : noire , 3 : bord )²
        public static ColorPiece CouleurCase(int IndexCase)
        {
            if (PiecesEchiquier[IndexCase] == TypePiece.Bordure)
                // on est en dehors du plateau de jeu ( sur la bordure )
                return ColorPiece.BordPlateau;
            else if (PiecesEchiquier[IndexCase] == TypePiece.Vide)
                // on est sur une case vide
                return ColorPiece.Vide;
            else
                // on est sur une case avec une pièce noire ou blanche
                return (int)PiecesEchiquier[IndexCase] % 2 == 0 ? ColorPiece.Noir : ColorPiece.Blanc;
        }
        // Vérifie si le déplacement correspond à un roque 
        // Renvoie le roque correspondant ou pas de roque
        private static FlagMouvementRoque TestSiRoque(int IndexSource, int IndexDestination)
        {
            if (IndexSource == 25 && PiecesEchiquier[IndexSource] == TypePiece.RoiBlanc) // case e1 ( roi blanc )
            {
                if (IndexDestination == 27)
                    return FlagMouvementRoque.PetitRoqueBlanc; // petit roque blanc ( case g1 )
                if (IndexDestination == 23)
                    return FlagMouvementRoque.GrandRoqueBlanc; // grand roque blanc ( case c1 )
            }
            if (IndexSource == 95 && PiecesEchiquier[IndexSource] == TypePiece.RoiNoir) // case e8 ( roi noir )
            {
                if (IndexDestination == 97)
                    return FlagMouvementRoque.PetitRoqueNoir; // petit roque noir ( case g8 )
                if (IndexDestination == 93)
                    return FlagMouvementRoque.GrandRoqueNoir; // grand roque noir ( case c8 )
            }
            return FlagMouvementRoque.PasDeRoque;
        }
        // Cherche la case contenant le roi
        private static string PositionRoi(TypePiece Roi)
        {
            return NomCaseAlgebrique(PiecesEchiquier.FindIndex(p => p == Roi));
        }
        // Déplacements du roi avec gestion des roques mais sans vérifier les échecs
        private static List<string> MouvementsRoi(int IndexCase)
        {
            List<string> MouvementsDuRoi = MouvementsReine(IndexCase); // 8 déplacements, comme la reine mais d'une seule case )
            // on traite les 2 petits roques
            if ((IndexCase == 25 && StatutRoque.HasFlag(FlagEnableRoque.RoqueBlanc)) || (IndexCase == 95 && StatutRoque.HasFlag(FlagEnableRoque.RoqueNoir)))
                if (PiecesEchiquier[IndexCase + 1] == TypePiece.Vide && PiecesEchiquier[IndexCase + 2] == TypePiece.Vide)
                    AjouteMouvements(IndexCase, +2, MouvementsDuRoi);
            // idem pour les 2 grands roques
            if ((IndexCase == 25 && StatutRoque.HasFlag(FlagEnableRoque.RoqueBlanc)) || (IndexCase == 95 && StatutRoque.HasFlag(FlagEnableRoque.RoqueNoir)))
                if (PiecesEchiquier[IndexCase - 1] == TypePiece.Vide && PiecesEchiquier[IndexCase - 2] == TypePiece.Vide && PiecesEchiquier[IndexCase - 3] == TypePiece.Vide)
                    AjouteMouvements(IndexCase, -2, MouvementsDuRoi);
            return MouvementsDuRoi;
        }
        // Déplace le roi et la tour pour un roque donné sous forme d'un char du champ FEN
        private static string MouvementsPourRoque(FlagMouvementRoque roque)
        {
            IndexCaseEnPassant = 0;
            SansPrise++;
            switch (roque)
            {
                case FlagMouvementRoque.PetitRoqueBlanc: // petit roque blanc
                    StatutRoque ^= FlagEnableRoque.RoqueBlanc; // plus de roque pour le roi blanc
                    PetitRoqueBlancPossible = GrandRoqueBlancPossible = false;
                    DeplacementPiece(25, 27, true);
                    DeplacementPiece(28, 26, true);
                    return "O-O";
                case FlagMouvementRoque.GrandRoqueBlanc: // grand roque blanc
                    StatutRoque ^= FlagEnableRoque.RoqueBlanc; // plus de roque pour le roi blanc
                    PetitRoqueBlancPossible = GrandRoqueBlancPossible = false;
                    DeplacementPiece(25, 23, true);
                    DeplacementPiece(21, 24, true);
                    return "O-O-O";
                case FlagMouvementRoque.PetitRoqueNoir: // petit roque noir
                    StatutRoque ^= FlagEnableRoque.RoqueNoir; // plus de roque pour le roi noir
                    PetitRoqueNoirPossible = GrandRoqueNoirPossible = false;
                    DeplacementPiece(95, 97, true);
                    DeplacementPiece(98, 96, true);
                    return "O-O";
                case FlagMouvementRoque.GrandRoqueNoir: // grand roque noir
                    StatutRoque ^= FlagEnableRoque.RoqueNoir; // plus de roque pour le roi noir
                    PetitRoqueNoirPossible = GrandRoqueNoirPossible = false;
                    DeplacementPiece(95, 93, true);
                    DeplacementPiece(91, 94, true);
                    return "O-O-O";
                default:
                    return string.Empty;
            }
        }
        // Promotion d'un pion ( retourne si le roi adverse est en échec )
        private static bool TestEchecPromotionPion(int IndexCasePromotion, TypePiece Roi)
        {
            List<string> MouvementsPiecePromue = RetourneMouvements(NomCaseAlgebrique(IndexCasePromotion));
            if (MouvementsPiecePromue.Count > 0)
                // true si le roi est menacé
                return MouvementsPiecePromue.Contains("x" + PositionRoi(Roi));
            else
                return false;
        }
        // Déplacement du pion
        private static List<string> MouvementsPion(int IndexCase)
        {
            List<string> MouvementsDuPion = new List<string>();
            switch (PiecesEchiquier[IndexCase])
            {
                case TypePiece.PionBlanc:
                    // les 4 mouvements possibles pour les pions blancs
                    AjouteMouvements(IndexCase, IndexCase + 10, MouvementsDuPion, false);
                    if ((IndexCase / 10) == 3 && PiecesEchiquier[IndexCase + 10] == TypePiece.Vide)
                        AjouteMouvements(IndexCase, IndexCase + 20, MouvementsDuPion, false);
                    AjoutePrise(IndexCase, IndexCase + 9, MouvementsDuPion);
                    AjoutePrise(IndexCase, IndexCase + 11, MouvementsDuPion);
                    break;
                case TypePiece.PionNoir:
                    // les 4 mouvements possibles pour les pions noirs
                    AjouteMouvements(IndexCase, IndexCase - 10, MouvementsDuPion, false);
                    if ((IndexCase / 10) == 8 && PiecesEchiquier[IndexCase - 10] == TypePiece.Vide)
                        AjouteMouvements(IndexCase, IndexCase - 20, MouvementsDuPion, false);
                    AjoutePrise(IndexCase, IndexCase - 9, MouvementsDuPion);
                    AjoutePrise(IndexCase, IndexCase - 11, MouvementsDuPion);
                    break;
            }
            return MouvementsDuPion;
        }
        // Ajoute une prise en vérifiant que la case de destination est prenable  
        private static void AjoutePrise(int IndexSource, int IndexDestination, List<string> Mouvements)
        {
            if (CouleurCase(IndexDestination) == CouleurAdversaireCase(IndexSource))
                Mouvements.Add("x" + NomCaseAlgebrique(IndexDestination));
            // si la pièce de départ est de la couleur de celui qui doit jouer pour la prise en passant avec un pion
            if (IndexDestination == IndexCaseEnPassant && QuiJoue == CouleurCase(IndexSource))
                Mouvements.Add("x" + NomCaseAlgebrique(IndexDestination));
        }
        private static bool TestEnPassant(int IndexSource, int IndexDestination)
        {   // si on arrive sur la case en passant et si c'est un pion blanc ou noir
            return IndexCaseEnPassant == IndexDestination && (PiecesEchiquier[IndexSource] == TypePiece.PionBlanc || PiecesEchiquier[IndexSource] == TypePiece.PionNoir);
        }
        // Enlève le pion adverse dans un mouvement en passant
        // On teste en même temps si un second pion peut faire la même prise en passant
        private static void MouvementsEnPassant(int IndexSource)
        {
            int indexCasePion;
            int colonne = IndexSource % 10;
            int ligne = (IndexSource / 10) - 1;
            TestSecondPion = false;
            if (PiecesEchiquier[IndexSource] == TypePiece.PionBlanc)
                indexCasePion = IndexCaseEnPassant - 10;
            else
                indexCasePion = IndexCaseEnPassant + 10;
            if (colonne < 7)
            {
                if (ligne == 4)
                    TestSecondPion = (PiecesEchiquier[IndexSource + 2] == TypePiece.PionNoir); // second pion noir à  droite
                if (ligne == 5)
                    TestSecondPion = (PiecesEchiquier[IndexSource + 2] == TypePiece.PionBlanc); // second pion blanc à droite
            }
            if (TestSecondPion == false && colonne > 2)
            {
                if (ligne == 4)
                    TestSecondPion = (PiecesEchiquier[IndexSource - 2] == TypePiece.PionNoir); // second pion noir à gauche
                if (ligne == 5)
                    TestSecondPion = (PiecesEchiquier[IndexSource - 2] == TypePiece.PionBlanc); // second pion blanc à gauche
            }
            PiecesEchiquier[indexCasePion] = TypePiece.Vide;
            DessineCaseVide(indexCasePion, TypePiece.Vide);
            SansPrise = 0;
            IndexCaseEnPassant = 0;
            FlagEnPassant = true;
        }
    }
}
