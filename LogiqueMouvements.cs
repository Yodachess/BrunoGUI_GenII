// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Informations reflexion moteur - Temps de reflexion - Réglage force moteur
// Gestion par menus - Sauvegarde PGN - Affichage Score - Personnalisation couleurs 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BrunoGUI_GenII
{
    public delegate void AffichageCoupJoue(string Coup);
    public delegate void AfficheInfo(string Chaine);
    public delegate void AfficheCouleur(LogiqueMouvements.ColorPiece Couleur);
    public delegate void AffichagePiece(int IndexCase, LogiqueMouvements.TypePiece Caractere);
    public delegate void AffichageSymbole(int IndexCase, LogiqueMouvements.TypeSymbole Symbole);
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

        public static event AffichageCoupJoue? AfficheCoupNoir;
        public static event AffichageCoupJoue? AfficheCoupBlanc;
        public static event AfficheInfo? AfficheInfoEchec;       // message d'échec ou de pat (vide : plus d'échec)
        public static event AfficheCouleur? AfficheEchecEtMat;   // camp maté
        public static event AfficheCouleur? AffichePat;          // camp pat (il n'a plus de coup)
        public static event AfficheCouleur? AfficheTour;         // camp au trait après un coup
        // Pièce choisie par le joueur pour une promotion (l'interface affiche le choix et attend le clic) ;
        // si personne ne répond (tests, fenêtre fermée), c'est une dame
        public static Func<ColorPiece, TypePiece>? ChoixPromotion { get; set; }
        // public static event AfficheInfo AfficheFen;
        public static event AffichagePiece? DessinePiece;
        public static event AffichageSymbole? DessineSymbole;

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
        // Identification des 4 mouvements de roque possibles : permet de savoir si le roi roque et quel roque il joue
        public enum FlagMouvementRoque
        {
            PasDeRoque, PetitRoqueNoir, GrandRoqueNoir, PetitRoqueBlanc, GrandRoqueBlanc
        }
        // ═══ Position de la partie ═══
        // Tout l'état de la position est dans l'objet PositionActuelle. Les propriétés statiques ci-dessous
        // (PiecesEchiquier, QuiJoue, droits de roque, IndexCaseEnPassant, SansPrise, NombreCoupsJoues, Echec, EchecetMat)
        // ne sont que des raccourcis vers cet objet. Pour un calcul "pour voir", utiliser CalculerSurCopie.
        public static Position PositionActuelle { get; private set; } = new();
        public static List<TypePiece> PiecesEchiquier => PositionActuelle.Pieces;
        public static bool PetitRoqueBlancPossible { get => PositionActuelle.PetitRoqueBlancPossible; set => PositionActuelle.PetitRoqueBlancPossible = value; }
        public static bool GrandRoqueBlancPossible { get => PositionActuelle.GrandRoqueBlancPossible; set => PositionActuelle.GrandRoqueBlancPossible = value; }
        public static bool PetitRoqueNoirPossible { get => PositionActuelle.PetitRoqueNoirPossible; set => PositionActuelle.PetitRoqueNoirPossible = value; }
        public static bool GrandRoqueNoirPossible { get => PositionActuelle.GrandRoqueNoirPossible; set => PositionActuelle.GrandRoqueNoirPossible = value; }
        public static bool Echec { get => PositionActuelle.Echec; set => PositionActuelle.Echec = value; }
        public static bool EchecetMat { get => PositionActuelle.EchecetMat; set => PositionActuelle.EchecetMat = value; }

        public static T CalculerSurCopie<T>(Func<T> calcul) => CalculerSur(PositionActuelle, calcul);

        public static T CalculerSur<T>(Position position, Func<T> calcul)
        {   // Exécute un calcul sur une copie de la position donnée (la partie ou une autre, ex : position affichée pendant le parcours) :
            // tout ce que le calcul modifie (pièces, trait, roques, en passant, échec...) est abandonné à la fin, même en cas d'exception
            Position original = PositionActuelle;
            PositionActuelle = position.Copier();
            try
            {
                return calcul();
            }
            finally
            {
                PositionActuelle = original;
            }
        }

        public static bool CoupValide { get; set; }
        public static bool DernierCoupTerminePartie { get; private set; }  // le dernier coup joué a maté ou pat l'adversaire
        public static string FenDepart { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        // Après le coup 1. e4 :        rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1
        // Après le coup 1. ... c5 :    rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2
        // Après le coup 2. Cf3 :       rnbqkbnr/pp1ppppp/8/2p5/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 1 2

        private static readonly Dictionary<int, TypePiece> ListeMouvementsPiece = []; // symboles affichés des déplacements possibles d'une pièce
        private static readonly Dictionary<int, TypePiece> ListeMenacesPiece = [];    // et des menaces sur elle
        private static readonly List<TypePiece> ListPiecesNoires = [TypePiece.TourNoire, TypePiece.CavalierNoir, TypePiece.FouNoir, TypePiece.ReineNoire, TypePiece.RoiNoir, TypePiece.FouNoir, TypePiece.CavalierNoir, TypePiece.TourNoire];
        private static readonly List<TypePiece> ListPiecesBlanches = [TypePiece.TourBlanche, TypePiece.CavalierBlanc, TypePiece.FouBlanc, TypePiece.ReineBlanche, TypePiece.RoiBlanc, TypePiece.FouBlanc, TypePiece.CavalierBlanc, TypePiece.TourBlanche];

        // Les coups de la partie : une seule liste d'objets Coup (toutes les notations d'un coup ensemble).
        // Une partie chargée depuis un FEN commence par un élément "position de départ" (Coup.PositionDeDepart), sans coup.
        // On ne la modifie que par AjouteCoup, AjoutePositionDeDepart, RetireDernierCoup et ViderCoups.
        private static readonly List<Coup> _coups = [];
        public static IReadOnlyList<Coup> ListeCoups => _coups;
        // Demi-coups joués (balise PGN PlyCount) : sans l'élément "position de départ" d'une partie commencée depuis un FEN
        public static int DemiCoupsJoues => _coups.Count(c => !c.EstPositionDeDepart);
        // Vues en lecture seule par notation (même index que ListeCoups, donc toujours alignées)
        public static readonly VueCoups ListeCoupsFen = new(_coups, c => c.Fen);          // https://www.pousseurdebois.fr/cours/notation-fen/
        public static readonly VueCoups ListeCoupsPgnIntl = new(_coups, c => c.PgnIntl);  // https://fr.wikipedia.org/wiki/Portable_Game_Notation
        public static readonly VueCoups ListeCoupsPgnFr = new(_coups, c => c.PgnFr);
        public static readonly VueCoups ListeCoupsNal = new(_coups, c => c.Nal);          // Notation Algébrique longue (NAL)
        public static readonly VueCoups ListeCoupsUci = new(_coups, c => c.Uci);          // Notation protocole UCI

        public static void AjouteCoup(Coup coup) => _coups.Add(coup);
        public static void AjoutePositionDeDepart(string fen) => _coups.Add(Coup.PositionDeDepart(fen));
        public static void ViderCoups() => _coups.Clear();
        public static bool RetireDernierCoup()
        {   // Retire le dernier coup joué (jamais la position de départ) ; retourne false s'il n'y a aucun coup à retirer
            if (_coups.Count == 0 || _coups[^1].EstPositionDeDepart)
                return false;
            _coups.RemoveAt(_coups.Count - 1);
            return true;
        }

        // Information Fen
        public static ColorPiece QuiJoue { get => PositionActuelle.QuiJoue; set => PositionActuelle.QuiJoue = value; }  // le champ 2  : w ou b ( Blanc ou Noir ), indique la couleur qui a le trait
                                                                // le champ 3 (droits de roque KQkq, ou –) correspond aux 4 booléens
                                                                // PetitRoqueBlancPossible, GrandRoqueBlancPossible, PetitRoqueNoirPossible, GrandRoqueNoirPossible
        public static int IndexCaseEnPassant { get => PositionActuelle.IndexCaseEnPassant; set => PositionActuelle.IndexCaseEnPassant = value; }  // le champ 4  : index de la case en passant si elle existe
                                                                // "–" signifie qu’aucune prise en passant n’est possible.
                                                                // Sil y en avait eu une, il aurait suffi de remplacer – par la case de capture en passant exemple f6.
        public static int SansPrise { get => PositionActuelle.SansPrise; set => PositionActuelle.SansPrise = value; }  // le champ 5  : +1 à chaque mouvement sans prise ou mouvement de pion
                                                                // Compteur qui compte le nombre de demi-coups depuis la dernière capture ou le dernier mouvement de pions.
                                                                // Ce compteur sert uniquement pour la règle des 50 coups.
        public static Single NombreCoupsJoues { get => PositionActuelle.NombreCoupsJoues; set => PositionActuelle.NombreCoupsJoues = value; }  //le champ 6  : on avance de 0.5 en 0.5 et on récupère la partie entière
                // Ce champ correspond au nombre de coups complets.
                // C’est le PROCHAIN numéro qui s’affichera dans votre notation de partie.
                // Son incrémentation est de 1 et est actualisé après que Noir ait joué.

        public static void InitialisationEchiquier()
        {   // Met la partie à la position de départ et l'affiche
            QuiJoue = ColorPiece.Blanc;     // détermination du trait ( les blancs commencent )
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

        public static void DessinePiecePlateau(int IndexCase, TypePiece Piece)
        {   // Dessine les piéces sur le plateau à l'initialisation
            PiecesEchiquier[IndexCase] = Piece;
            DessinePiece?.Invoke(IndexCase, Piece);
        }
        public static void DessinPieces() => DessinPosition(PositionActuelle);

        public static void DessinPosition(Position position)
        {   // Dessine les 64 cases d'une position (la partie, ou une position passée affichée), sans rien modifier
            for (int ligne = 2; ligne <= 9; ligne++)
                for (int colonne = 1; colonne <= 8; colonne++)
                    DessinePiece?.Invoke((ligne * 10) + colonne, position.Pieces[(ligne * 10) + colonne]);
        }

        public static void MiseenplaceFen(string Fenautiliser)
        {   // La partie passe à la position FEN (retour arrière, chargement...), qui est dessinée.
            // Pour seulement AFFICHER une position (parcours de la partie), utiliser PositionDepuisFen puis DessinPosition
            Regles.LireFenDans(PositionActuelle, Fenautiliser);
            DessinPieces();
        }

        public static Position PositionDepuisFen(string fen) => Regles.LireFen(fen);   // sans modifier la partie ni rien dessiner

        public static int DemiCoupAvant(Position position)
        {   // Numéro (à partir de 0) du dernier demi-coup joué avant cette position :
            // sert à numéroter les variantes du moteur (Outils.VarianteUciVersPgn). 0 pour la position initiale
            int demiCoupsJoues = (int)Math.Round((position.NombreCoupsJoues - 1) * 2);
            return Math.Max(0, demiCoupsJoues - 1);
        }

        public static void DessineMouvements(string caseSource, bool visu)
        {   // Dessin des mouvements possibles pour une pièce ainsi que les menaces adverses pour cette même pièce
            foreach (string mouvement in Regles.Mouvements(PositionActuelle, caseSource))
            {
                string caseDestination = mouvement[^2..];
                TypeSymbole symbole = !Regles.EstLegal(PositionActuelle, caseSource, caseDestination) ? TypeSymbole.SymboleMouvementInterdit
                                    : mouvement.StartsWith('x') ? TypeSymbole.SymboleMenacePiece        // mouvement possible avec prise
                                    : TypeSymbole.SymboleMouvementSansPrise;
                int indexCase = RenvoieCaseIndex120(caseDestination);
                ListeMouvementsPiece.Add(indexCase, PiecesEchiquier[indexCase]);
                if (visu)
                    DessineSymbole?.Invoke(indexCase, symbole);
            }
            // (les menaces sur la pièce choisie ne sont pas dessinées : ce n'était pas assez clair)
            foreach (string menace in Regles.CasesPiecesMenacantes(PositionActuelle, caseSource))
            {
                int indexCase = RenvoieCaseIndex120(menace);
                ListeMenacesPiece.Add(indexCase, PiecesEchiquier[indexCase]);
            }
        }

        public static void EffaceSymboles(bool visu)
        {   // Efface les symboles sur l'échiquier si ceux-ci sont visibles
            if (visu)
                foreach (KeyValuePair<int, TypePiece> element in ListeMouvementsPiece.Concat(ListeMenacesPiece))
                    DessinePiece?.Invoke(element.Key, element.Value);
            ListeMouvementsPiece.Clear();
            ListeMenacesPiece.Clear();
        }

        public static string RetourneChaineFenActuel() => Regles.Fen(PositionActuelle);   // la FEN de la position de la partie

        public static void ExecutionCoup(string caseSource, string caseDestination, TypePiece promotion = TypePiece.Vide)
        {   // Exécute un coup pour le joueur humain ou le moteur UCI.
            // promotion : pièce choisie si le coup est une promotion (moteur, bibliothèque, PGN) ; Vide : on la demande au joueur
            // (ChoixPromotion) si le coup en est une
            bool AucunCoupJouable = false;      // après le coup : l'adversaire n'a plus de coup (mat s'il est en échec, pat sinon)
            CoupValide = false;
            DernierCoupTerminePartie = false;
            EchecetMat = false;
            string MouvementCoup = string.Empty;
            if (caseSource != caseDestination && Regles.EstLegal(PositionActuelle, caseSource, caseDestination))
            {
                AfficheInfoEchec?.Invoke(string.Empty);
                MouvementCoup = Regles.Notation(PositionActuelle, caseSource, caseDestination).Coup;
                int indexSource = RenvoieCaseIndex120(caseSource), indexDestination = RenvoieCaseIndex120(caseDestination);
                if (promotion == TypePiece.Vide && Regles.EstPromotion(PositionActuelle, indexSource, indexDestination))
                {   // le pion est montré sur sa case d'arrivée pendant que le joueur choisit sa pièce (une dame si personne ne répond)
                    DessinePiece?.Invoke(indexDestination, PiecesEchiquier[indexSource]);
                    DessinePiece?.Invoke(indexSource, TypePiece.Vide);
                    promotion = ChoixPromotion?.Invoke(CouleurCase(indexSource)) ?? TypePiece.Vide;
                }
                List<TypePiece> avant = [.. PiecesEchiquier];
                ResultatCoup resultat = Regles.Jouer(PositionActuelle, caseSource, caseDestination, promotion);
                for (int i = 21; i <= 98; i++)      // les cases changées (avec la tour du roque et le pion pris en passant)
                    if (PiecesEchiquier[i] != avant[i])
                        DessinePiece?.Invoke(i, PiecesEchiquier[i]);
                AfficheTour?.Invoke(QuiJoue);
                Echec = Regles.CampAuTraitEnEchec(PositionActuelle);   // (échec direct, à la découverte, ou par la pièce promue)

                string lettrePromo = resultat.PiecePromue switch
                {   // On rajoute la pièce promue avant de stocker dans le PGN
                    TypePiece.ReineBlanche or TypePiece.ReineNoire => "D",
                    TypePiece.TourBlanche or TypePiece.TourNoire => "T",
                    TypePiece.FouBlanc or TypePiece.FouNoir => "F",
                    TypePiece.CavalierBlanc or TypePiece.CavalierNoir => "C",
                    _ => ""
                };
                MouvementCoup += lettrePromo;

                string ChaineFen = RetourneChaineFenActuel();       // Position après le coup

                string MouvementCoupPgn = NotationInternationale(MouvementCoup);   // PGN international (lettres anglaises des pièces)
                string MouvementCoupNal;

                // Traitement de la notation algébrique longue
                if (MouvementCoupPgn.Contains('x'))
                {
                    if (char.IsLower(MouvementCoupPgn[0]))
                    {   // C'est un coup de Pion
                        MouvementCoupNal = caseSource + 'x' + caseDestination + LettrePromotionUci(lettrePromo).ToUpper();
                    }
                    else
                    {   // C'est un coup de Pièce
                        MouvementCoupNal = MouvementCoupPgn[0] + caseSource + 'x' + caseDestination;   // la case d'arrivée, pas des positions fixes dans le PGN (ex : Cgxe2)
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
                            MouvementCoupNal = MouvementCoupPgn[0] + caseSource + '-' + caseDestination;
                        }
                    }
                }
                // Traitement de la prise en passant
                if (resultat.EnPassant)
                {                                                           // Correction de bug par ajout de caseSource
                    MouvementCoup = caseSource[0] + "x" + MouvementCoup;    // on enlève " e.p"; pas d'indication de la prise en passant en PGN
                    MouvementCoupPgn = caseSource[0] + "x" + MouvementCoupPgn;
                    MouvementCoupNal = MouvementCoupPgn + " e.p";           // en Nal, on indique la prise en passant par "e.p" après le coup
                    if (resultat.SecondPion)
                    {       // on précise la colonne car deux pions peuvent faire la prise en passant
                        MouvementCoup = caseSource[..1] + MouvementCoup;
                        MouvementCoupPgn = caseSource[..1] + MouvementCoupPgn;
                        MouvementCoupNal = caseSource[..1] + MouvementCoupPgn;
                    }
                }

                string[] FenTableau = ChaineFen.Split(' '); // On decoupe les champs du FEN
                string NumeroCoup = FenTableau[5];  // Champ 6 = numéro du coup de la partie (incrémenté à chaque coup des blancs)

                // Fin du coup : "#" si mat, "+" si échec, puis un espace (pas de symbole en UCI)
                AucunCoupJouable = !ResteCoupsValidesJouables();
                DernierCoupTerminePartie = AucunCoupJouable;    // mat ou pat : lu par RaisonNulle pendant les événements du coup
                string Fin = Echec ? (AucunCoupJouable ? "# " : "+ ") : " ";
                // Le numéro n'est écrit que devant un coup blanc (après un coup blanc, c'est aux Noirs de jouer : champ 2 = "b")
                string Numero = FenTableau[1] == "b" ? NumeroCoup + ". " : "";
                AjouteCoup(new Coup
                {
                    Fen = ChaineFen,
                    PgnIntl = Numero + MouvementCoupPgn + Fin,
                    PgnFr = Numero + MouvementCoup + Fin,
                    Nal = Numero + MouvementCoupNal + Fin,
                    Uci = caseSource + caseDestination + LettrePromotionUci(lettrePromo) + " ",
                });

                // Affiche si le roi est en échec
                if (Echec)
                {
                    AfficheInfoEchec?.Invoke(Langue.T("Le roi {0} est en échec", NomCouleur(QuiJoue)));
                }
                if (QuiJoue == ColorPiece.Noir)
                {
                    AfficheCoupBlanc?.Invoke(MouvementCoup);
                }
                else
                {
                    AfficheCoupNoir?.Invoke(MouvementCoup);
                }
            }
            if (MouvementCoup != string.Empty)
            {
                CoupValide = true;
                // EchecetMat n'est positionné qu'ici, après les événements AfficheCoupBlanc/AfficheCoupNoir (l'interface en dépend)
                if (Echec)
                {   // s'il y a échec et plus aucun coup valide à jouer : échec et mat
                    if (AucunCoupJouable)
                    {
                        EchecetMat = true;
                        AfficheEchecEtMat?.Invoke(QuiJoue);
                    }
                }
                else
                {   // sinon on affiche si le joueur qui a la couleur est Pat
                    // ( plus de coups valides jouables + le joueur n'est pas échec )
                    if (AucunCoupJouable)
                    {
                        AfficheInfoEchec?.Invoke(Langue.T("Le joueur {0} est Pat - plus de coup possible", NomCouleur(QuiJoue)));
                        AffichePat?.Invoke(QuiJoue);
                    }
                }
            }
        }

        public static bool TripleRepetition()
        {   // Détection de la triple répétition qui donne partie nulle.
            // Règle FIDE : même placement des pièces, même trait et mêmes droits de roque (champs 1 à 3 du FEN)
            // et même case en passant (champ 4 : ici, elle est notée après toute avance de deux cases, même sans prise possible).
            Dictionary<string, int> positionsVues = [];
            IEnumerable<string> positions = ListeCoupsFen;
            if (_coups.Count == 0 || !_coups[0].EstPositionDeDepart)
                positions = positions.Prepend(FenDepart);       // la position initiale compte aussi
            foreach (string fen in positions)
            {
                string position = string.Join(' ', fen.Split(' ').Take(4));
                positionsVues[position] = positionsVues.GetValueOrDefault(position) + 1;
                if (positionsVues[position] >= 3)
                    return true;
            }
            return false;
        }

        // Les règles appliquées à la position de la partie (voir Regles)
        public static bool MaterielInsuffisant() => Regles.MaterielInsuffisant(PositionActuelle);
        public static bool PeutMater(ColorPiece camp) => Regles.PeutMater(PositionActuelle, camp);   // (pendule : perte au temps ou nulle)
        public static void SimuleCoup(int IndexSource, int IndexDestination) => Regles.SimuleCoup(PositionActuelle, IndexSource, IndexDestination);
        public static bool CampAuTraitEnEchec() => Regles.CampAuTraitEnEchec(PositionActuelle);
        public static bool ResteCoupsValidesJouables() => Regles.ResteCoupsJouables(PositionActuelle);
        public static List<(string Source, string Destination)> CoupsLegaux() => Regles.CoupsLegaux(PositionActuelle);
        public static ColorPiece CouleurCase(int IndexCase) => Regles.CouleurCase(PositionActuelle, IndexCase);

        public static string? RaisonNulle()
        {   // Après un coup : raison de la nulle automatique, ou null si la partie continue.
            // Le mat et le pat du dernier coup sont signalés par leurs propres événements (AfficheEchecEtMat, AffichePat).
            if (DernierCoupTerminePartie)
                return null;
            if (TripleRepetition())
                return Langue.T("Nulle par répétition");
            if (SansPrise >= 100)
                return Langue.T("Nulle (règle des 50 coups)");
            if (MaterielInsuffisant())
                return Langue.T("Nulle (matériel insuffisant)");
            return null;
        }

        public static void CalculeEchecEtMat()
        {   // Le roi du camp qui vient de jouer (adversaire du camp au trait) est-il en échec, et mat ?
            Position adverse = PositionActuelle.Copier();
            adverse.QuiJoue = Adversaire(adverse.QuiJoue);
            Echec = Regles.CampAuTraitEnEchec(adverse);
            EchecetMat = Echec && !Regles.ResteCoupsJouables(adverse);
        }

        public static string CoupNotationAlgebriquePGN(string caseSource, string CaseDestination)
        {   // Le coup (ex : e2e4) en notation algébrique française, sans "+" ni "#" ; Echec dit si le roi adverse sera en échec
            (string coup, bool echec) = Regles.Notation(PositionActuelle, caseSource, CaseDestination);
            Echec = echec;
            return coup;
        }

        public static string NotationInternationale(string coupFrancais)
        {   // Traduit les lettres des pièces du français vers l'anglais : R(oi)→K, D(ame)→Q, T(our)→R, F(ou)→B, C(avalier)→N.
            // Sans ambiguïté : les pièces sont en majuscules, les colonnes (a à h) en minuscules, et le roque "O-O" est identique.
            // Chaque lettre n'est traduite qu'une fois (le "R" issu de T n'est pas retraduit en K)
            return string.Concat(coupFrancais.Select(lettre => lettre switch
            {
                'R' => 'K',
                'D' => 'Q',
                'T' => 'R',
                'F' => 'B',
                'C' => 'N',
                _ => lettre,
            }));
        }
        public static ColorPiece Adversaire(ColorPiece couleur) => couleur == ColorPiece.Blanc ? ColorPiece.Noir : ColorPiece.Blanc;
        public static string NomCouleur(ColorPiece couleur) => couleur == ColorPiece.Blanc ? Langue.T("Blanc") : Langue.T("Noir");     // "le roi Blanc"
        public static string NomCamp(ColorPiece couleur) => couleur == ColorPiece.Blanc ? Langue.T("Blancs") : Langue.T("Noirs");      // "aux Blancs de jouer"
        public static TypePiece PieceDeLaCouleur(TypePiece piece, ColorPiece couleur)
        {   // La même sorte de pièce de promotion, dans la couleur demandée (une dame si la pièce n'est pas une pièce de promotion)
            return piece switch
            {
                TypePiece.TourBlanche or TypePiece.TourNoire => couleur == ColorPiece.Blanc ? TypePiece.TourBlanche : TypePiece.TourNoire,
                TypePiece.FouBlanc or TypePiece.FouNoir => couleur == ColorPiece.Blanc ? TypePiece.FouBlanc : TypePiece.FouNoir,
                TypePiece.CavalierBlanc or TypePiece.CavalierNoir => couleur == ColorPiece.Blanc ? TypePiece.CavalierBlanc : TypePiece.CavalierNoir,
                _ => couleur == ColorPiece.Blanc ? TypePiece.ReineBlanche : TypePiece.ReineNoire
            };
        }
        public static TypePiece PieceDePromotion(char lettreUci, ColorPiece couleur)
        {   // Pièce de promotion d'après la lettre UCI ("q", "r", "b", "n" : e7e8q) ; une dame si la lettre est inconnue
            TypePiece piece = char.ToLower(lettreUci) switch
            {
                'r' => TypePiece.TourBlanche,
                'b' => TypePiece.FouBlanc,
                'n' => TypePiece.CavalierBlanc,
                _ => TypePiece.ReineBlanche
            };
            return PieceDeLaCouleur(piece, couleur);
        }
        private static string LettrePromotionUci(string lettrePromo)
        {   // Convertit la pièce promue (français ou anglais) en notation UCI
            return lettrePromo.ToUpper() switch
            {   // Retourne : q, r, b ou n
                "D" or "Q" => "q",   // Dame / Queen
                "T" or "R" => "r",   // Tour / Rook
                "F" or "B" => "b",   // Fou / Bishop
                "C" or "N" => "n",   // Cavalier / Knight
                _ => ""
            };
        }
        public static string NomCaseAlgebrique(int IndexCase)
        {   // Renvoie le nom d'une case sous la forme e2 ou un caractère vide en cas d'erreur
            if (IndexCase != -1)
                return Convert.ToChar((IndexCase % 10) + 96) + ((IndexCase / 10) - 1).ToString();
            //  Le reste de IndexCase /10 + 96 pour avoir ASCII   + IndexCase / 10 - 1 pour avoir la ligne
            else
                return string.Empty;
        }
        public static int RenvoieCaseIndex120(string NomCase)
        {   // Renvoie l'index d'une case à partir de son nom (sous la forme "e2"), ou -1 si le nom n'est pas une case (ex : "" ou "z9")
            // Index = (rangée x 10) + (colonne : a = 1, b = 2 ... h = 8) + 10  (voir le schéma de l'échiquier 120 cases en tête du fichier)
            if (string.IsNullOrEmpty(NomCase) || NomCase.Length < 2 || NomCase[0] < 'a' || NomCase[0] > 'h' || NomCase[1] < '1' || NomCase[1] > '8')
            {
                Debug.WriteLine($"[Logique] RenvoieCaseIndex120 : case invalide '{NomCase}'");
                return -1;
            }
            return ((NomCase[1] - '0') * 10) + (NomCase[0] - 'a' + 1) + 10;
        }
    }
}
