// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Tests de la logique d'échecs (LogiqueMouvements, Outils) sans l'interface graphique
//      dotnet run --project Tests               tests rapides
//      dotnet run --project Tests -- --complet  ajoute les perft profonds (plusieurs minutes)
// ├─ Tests de règles : roque, prise en passant, promotion, 50 coups, lecture FEN, variantes UCI
// ├─ Tests de la classe Position : copie indépendante, calcul sur copie
// └─ Perft : nombre de positions atteignables à une profondeur donnée, comparé aux valeurs de référence
//            (https://www.chessprogramming.org/Perft_Results)

using System;
using System.Diagnostics;
using System.Linq;
using BrunoGUI_GenII;
using L = BrunoGUI_GenII.LogiqueMouvements;

bool complet = args.Contains("--complet");
int nombreEchecs = 0;

void Verifie(string nom, bool ok, string detail)
{
    Console.WriteLine($"{(ok ? "OK   " : "ECHEC")} {nom} -> {detail}");
    if (!ok) nombreEchecs++;
}

// La logique fonctionne sans interface (événements sans abonné) ; on compte seulement les dessins de pièces demandés
int nombreDessins = 0;
L.DessinePiece += (i, p) => nombreDessins++;

void Charger(string fen)
{   // Met en place une position FEN et remet à zéro l'état de la partie
    for (int r = 2; r <= 9; r++)
        for (int c = 1; c <= 8; c++)
            L.PiecesEchiquier[r * 10 + c] = L.TypePiece.Vide;
    ViderListes();
    L.Echec = L.EchecetMat = false;
    L.PromotionPiece = L.TypePiece.Vide;
    L.BloquerChoixPromo = false;
    L.MiseenplaceFen(fen);
}

void ViderListes()
{
    L.ListeCoupsFen.Clear(); L.ListeCoupsPgnIntl.Clear(); L.ListeCoupsPgnFr.Clear(); L.ListeCoupsNal.Clear(); L.ListeCoupsUci.Clear();
}

string DernierCoupPgn() => L.ListeCoupsPgnIntl.LastOrDefault() ?? "(aucun)";

// ═══════════════ Tests de règles ═══════════════
Console.WriteLine("── Règles ──");

Charger("5k2/8/8/8/8/8/8/4K2R w K - 0 1");
L.ExecutionCoup("e1", "g1");
Verifie("Roque qui donne échec par la tour", DernierCoupPgn().Contains("O-O+"), DernierCoupPgn());
Verifie("Coup avec échec présent dans la liste UCI", L.ListeCoupsUci.LastOrDefault()?.Trim() == "e1g1", L.ListeCoupsUci.LastOrDefault() ?? "(vide)");

Charger("8/8/8/K2pP2r/8/8/8/7k w - d6 0 1");
L.ExecutionCoup("e5", "d6");
Verifie("Prise en passant qui découvre son roi refusée", !L.CoupValide, "CoupValide = " + L.CoupValide);

Charger("8/8/8/k2pP2R/8/8/8/7K w - d6 0 1");
L.ExecutionCoup("e5", "d6");
Verifie("Prise en passant avec échec à la découverte", L.CoupValide && DernierCoupPgn().Contains('+'), DernierCoupPgn());

Charger("4k3/8/8/8/8/8/3r4/3RK3 w - - 10 30");
L.ExecutionCoup("d1", "d2");
Verifie("50 coups : remise à zéro sur prise d'une pièce", L.SansPrise == 0, "SansPrise = " + L.SansPrise);
Charger("4k3/8/8/8/8/8/3r4/3RK3 w - - 10 30");
L.ExecutionCoup("d1", "c1");
Verifie("50 coups : +1 sans prise ni coup de pion", L.SansPrise == 11, "SansPrise = " + L.SansPrise);

Charger("8/R1P4k/8/8/8/8/8/4K3 w - - 0 1");
L.PromotionPiece = L.TypePiece.CavalierBlanc;
L.BloquerChoixPromo = true;
L.ExecutionCoup("c7", "c8");
Verifie("Promotion avec échec à la découverte", DernierCoupPgn().Contains('+'), DernierCoupPgn());

Charger("4k3/8/8/8/8/8/8/4K3 w - - 0 1");
Charger("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
Verifie("FEN : droits de roque KQkq relus en entier", L.RetourneChaineFenActuel().Contains(" KQkq "), L.RetourneChaineFenActuel());
Charger("r3k2r/8/8/8/8/8/8/R3K2R w Kq - 0 1");
Verifie("FEN : droits de roque partiels (Kq)", L.RetourneChaineFenActuel().Contains(" Kq "), L.RetourneChaineFenActuel());
Charger("4k3/8/8/8/8/8/8/4K3 w - - 7 12");
Verifie("FEN : compteur des 50 coups relu", L.SansPrise == 7, "SansPrise = " + L.SansPrise);

Charger("4k3/8/8/8/8/8/8/4K2R w K - 0 1");
L.ExecutionCoup("h1", "h2"); L.ExecutionCoup("e8", "d8"); L.ExecutionCoup("h2", "h1"); L.ExecutionCoup("d8", "e8");
L.ExecutionCoup("e1", "g1");
Verifie("Roque refusé si la tour a bougé puis est revenue", !L.CoupValide, "CoupValide = " + L.CoupValide);

Charger("4k3/8/8/8/8/8/6b1/4K2R b K - 0 1");
L.ExecutionCoup("g2", "h1");
Verifie("Droit de roque perdu quand la tour est prise dans son coin", !L.RetourneChaineFenActuel().Contains(" K "), L.RetourneChaineFenActuel());
L.ExecutionCoup("e1", "g1");
Verifie("Roque refusé si la tour a été prise", !L.CoupValide, "CoupValide = " + L.CoupValide);

Charger("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 3 17");
L.Echec = true;   // valeur témoin qui doit être restaurée
string fenAvantVariante = L.RetourneChaineFenActuel();
int dessinsAvantVariante = nombreDessins;
string variante = Outils.VarianteUciVersPgn("e1g1 e8c8 f1f7", 0, false);
Verifie("Variante UCI avec roque (la tour passe en f1)", variante.Contains("Tf7"), variante.Trim());
Verifie("Variante UCI : état d'échec restauré", L.Echec, "Echec = " + L.Echec);
Verifie("Variante UCI : position identique (FEN complet)", L.RetourneChaineFenActuel() == fenAvantVariante, L.RetourneChaineFenActuel());
Verifie("Variante UCI : échiquier non redessiné", nombreDessins == dessinsAvantVariante, $"{nombreDessins - dessinsAvantVariante} dessin(s)");

Charger("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");
string conseil = Outils.VarianteUciVersPgn("d7d5 e4d5", 1, true);
Verifie("Conseil du moteur noté après son meilleur coup", conseil == "2. exd5", conseil);

Charger("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
foreach (var (s, d) in new[] { ("e2", "e4"), ("e7", "e5"), ("f1", "c4"), ("b8", "c6"), ("d1", "h5"), ("g8", "f6"), ("h5", "f7") })
    L.ExecutionCoup(s, d);
Verifie("Mat du berger détecté", L.EchecetMat, string.Concat(L.ListeCoupsPgnIntl).Trim());

// ═══════════════ Notation des coups ambigus ═══════════════
Console.WriteLine("── Notation ──");

void TestNotation(string nom, string fen, string source, string destination, string attendu)
{   // Joue le coup et compare sa notation PGN internationale (sans numéro, sans espace),
    // puis vérifie l'aller-retour : relire cette notation (chargement PGN) doit donner la même position
    Charger(fen);
    L.ExecutionCoup(source, destination);
    string fenApresCoup = L.RetourneChaineFenActuel();
    string coup = DernierCoupPgn().Trim();
    coup = coup[(coup.LastIndexOf(' ') + 1)..];     // retire le numéro de coup éventuel ("1. ")
    Verifie(nom, coup == attendu, $"{coup} (attendu {attendu})");

    Charger(fen);
    GestionPartiePgn.DecodeCoupPartie(coup, L.QuiJoue == L.ColorPiece.Blanc);
    Verifie(nom + " : relecture PGN", L.RetourneChaineFenActuel() == fenApresCoup, L.RetourneChaineFenActuel());
}

TestNotation("Deux tours sur la même rangée", "4k3/8/8/8/8/8/4K3/R6R w - - 0 1", "a1", "d1", "Rad1");
TestNotation("Deux tours sur la même colonne", "4k3/8/8/R7/8/8/8/R3K3 w - - 0 1", "a1", "a3", "R1a3");
TestNotation("Deux cavaliers pouvant aller sur la même case", "4k3/8/8/8/8/2N5/8/4K1N1 w - - 0 1", "g1", "e2", "Nge2");
TestNotation("Cavalier cloué : pas d'ambiguïté", "4k3/8/8/8/1b6/2N5/8/4K1N1 w - - 0 1", "g1", "e2", "Ne2");
TestNotation("Deux dames sur la même colonne", "4k3/8/8/8/8/8/Q7/Q3K3 w - - 0 1", "a1", "b2", "Q1b2");
TestNotation("Deux fous de même couleur", "4k3/8/8/8/8/B7/8/2B1K3 w - - 0 1", "c1", "b2", "Bcb2");
TestNotation("Trois dames : colonne et rangée", "4k3/8/8/8/8/Q7/8/Q1Q1K3 w - - 0 1", "a1", "b2", "Qa1b2");
TestNotation("Autre tour qui ne peut pas y aller", "4k3/8/8/8/8/8/8/R3K1R1 w - - 0 1", "g1", "g5", "Rg5");
TestNotation("Prise ambiguë par un cavalier", "4k3/8/8/8/8/2N5/4p3/4K1N1 w - - 0 1", "g1", "e2", "Ngxe2");
Verifie("NAL d'une prise ambiguë", L.ListeCoupsNal.LastOrDefault()?.Trim().EndsWith("Ng1xe2") == true, L.ListeCoupsNal.LastOrDefault() ?? "(vide)");
TestNotation("Prise d'un pion par un pion", "4k3/8/8/3p4/4P3/8/8/4K3 w - - 0 1", "e4", "d5", "exd5");
Verifie("NAL d'une prise de pion", L.ListeCoupsNal.LastOrDefault()?.Trim().EndsWith("e4xd5") == true, L.ListeCoupsNal.LastOrDefault() ?? "(vide)");

// ═══════════════ Décodage des lignes UCI ═══════════════
Console.WriteLine("── Lignes UCI ──");

LigneUci info = LigneUci.Analyser("info depth 22 seldepth 30 multipv 2 score cp -35 nodes 123456 nps 1000000 hashfull 50 tbhits 0 time 120 pv e7e5 g1f3 b8c6");
Verifie("info : variante, score et numéro de variante",
    info.Commande == "info" && info.NumeroVariante == 2 && info.ScoreCentipions == -35 && info.MatEn == null && info.Variante == "e7e5 g1f3 b8c6",
    $"multipv={info.NumeroVariante} cp={info.ScoreCentipions} pv={info.Variante}");

LigneUci mat = LigneUci.Analyser("info depth 12 multipv 1 score mate -3 nodes 999 pv e1d1 d8d1");
Verifie("info : mat contre le camp au trait (signe conservé)", mat.MatEn == -3 && mat.ScoreCentipions == null, $"mate={mat.MatEn}");

LigneUci borne = LigneUci.Analyser("info depth 10 score cp 12 lowerbound nodes 500");
Verifie("info : score avec lowerbound, sans variante", borne.ScoreCentipions == 12 && borne.Variante == null, $"cp={borne.ScoreCentipions}");

LigneUci tronquee = LigneUci.Analyser("info depth 5 multipv");
Verifie("info : ligne tronquée sans plantage", tronquee.NumeroVariante == null, "multipv absent");

LigneUci texte = LigneUci.Analyser("info string NNUE evaluation using nn-1c0000000000.nnue (pv cp mate)");
Verifie("info string : texte libre ignoré", texte.Variante == null && texte.ScoreCentipions == null, texte.Commande);

LigneUci sargon = LigneUci.Analyser("info depth 6 score cp 40 time 900 pv d2d4 d7d5");
Verifie("info sans multipv (Sargon)", sargon.NumeroVariante == null && sargon.ScoreCentipions == 40 && sargon.Variante == "d2d4 d7d5", $"pv={sargon.Variante}");

LigneUci meilleur = LigneUci.Analyser("bestmove e2e4 ponder e7e5");
Verifie("bestmove avec ponder", meilleur.MeilleurCoup == "e2e4" && meilleur.CoupConseil == "e7e5" && !meilleur.AucunCoupLegal, $"{meilleur.MeilleurCoup} / {meilleur.CoupConseil}");
Verifie("bestmove sans ponder", LigneUci.Analyser("bestmove g1f3").CoupConseil == null, "ponder absent");
Verifie("bestmove (none) = aucun coup légal", LigneUci.Analyser("bestmove (none)").AucunCoupLegal && LigneUci.Analyser("bestmove 0000").AucunCoupLegal, "(none) et 0000");

LigneUci nom = LigneUci.Analyser("id name Stockfish 19");
LigneUci auteur = LigneUci.Analyser("id author the Stockfish developers (see AUTHORS file)");
Verifie("id name / id author", nom.NomMoteur == "Stockfish 19" && auteur.AuteurMoteur == "the Stockfish developers (see AUTHORS file)", $"{nom.NomMoteur} / {auteur.AuteurMoteur}");

Verifie("option : nom simple", LigneUci.Analyser("option name UCI_LimitStrength type check default false").NomOption == "UCI_LimitStrength", "UCI_LimitStrength");
Verifie("option : nom avec espace", LigneUci.Analyser("option name Skill Level type spin default 20 min 0 max 20").NomOption == "Skill Level", "Skill Level");
Verifie("ligne vide", LigneUci.Analyser("   ").Commande == "", "commande vide");

// ═══════════════ Paramètres (.ini) ═══════════════
Console.WriteLine("── Paramètres ──");

System.Drawing.Color peru = Parametres.ConvertitCouleur("Peru", Parametres.LichessCaseSombre);
Verifie("Couleur .ini par son nom", peru.R == 205 && peru.G == 133 && peru.B == 63, $"{peru.R},{peru.G},{peru.B}");
System.Drawing.Color hexa = Parametres.ConvertitCouleur("#B58863", Parametres.LichessCaseClaire);
Verifie("Couleur .ini en hexadécimal (Lichess)", hexa.R == 181 && hexa.G == 136 && hexa.B == 99, $"{hexa.R},{hexa.G},{hexa.B}");
System.Drawing.Color illisible = Parametres.ConvertitCouleur("PasUneCouleur", Parametres.LichessCaseClaire);
Verifie("Couleur .ini illisible : valeur par défaut Lichess", illisible.R == 240 && illisible.G == 217 && illisible.B == 181, $"{illisible.R},{illisible.G},{illisible.B}");

string iniTest = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BrunoGUI_test.ini");
System.IO.File.WriteAllLines(iniTest, [
    "; commentaire ignoré",
    "Casesombre = #B58863",
    "NomHumain = Testeur",
    "NombrelignesPV = 2",
    "NombreCoeursThread = 8",
    "TableHachage = 256 min" ]);
Parametres lus = new();
lus.ChargerDepuisIni(iniTest);
System.IO.File.Delete(iniTest);
Verifie("Lecture du .ini (couleur, nom, MultiPV, Threads, Hash)",
    lus.CaseSombre == "#B58863" && lus.NomHumain == "Testeur" && lus.NombreLignesPV == 2 && lus.NombreCoeursThread == 8 && lus.TailleHachageMo == 256,
    $"Hash = {lus.TailleHachageMo} Mo, Threads = {lus.NombreCoeursThread}");
Verifie(".ini sans TableHachage : taille du moteur conservée", new Parametres().TailleHachageMo == null, "null");

// ═══════════════ Classe Position ═══════════════
Console.WriteLine("── Position ──");

Charger("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
Position copie = L.PositionActuelle.Copier();
copie.Pieces[L.RenvoieCaseIndex120("e2")] = L.TypePiece.Vide;
copie.QuiJoue = L.ColorPiece.Noir;
copie.PetitRoqueBlancPossible = false;
Verifie("Copier : la copie est indépendante de l'original",
    L.PiecesEchiquier[L.RenvoieCaseIndex120("e2")] == L.TypePiece.PionBlanc && L.QuiJoue == L.ColorPiece.Blanc && L.PetitRoqueBlancPossible,
    L.RetourneChaineFenActuel());

Position avantCalcul = L.PositionActuelle;
try
{
    L.CalculerSurCopie<int>(() =>
    {
        L.SimuleCoup(L.RenvoieCaseIndex120("e2"), L.RenvoieCaseIndex120("e4"));
        L.QuiJoue = L.ColorPiece.Noir;
        throw new InvalidOperationException("erreur volontaire");
    });
}
catch (InvalidOperationException) { }
Verifie("CalculerSurCopie : position rétablie même après une exception",
    ReferenceEquals(L.PositionActuelle, avantCalcul) && L.RetourneChaineFenActuel() == "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1",
    L.RetourneChaineFenActuel());

Position vide = new();
Verifie("Position neuve : 120 cases, 64 vides et 56 bordures",
    vide.Pieces.Count == 120 && vide.Pieces.Count(p => p == L.TypePiece.Vide) == 64 && vide.Pieces.Count(p => p == L.TypePiece.Bordure) == 56,
    $"{vide.Pieces.Count} cases");

// ═══════════════ Perft ═══════════════
Console.WriteLine("── Perft ──");

L.TypePiece[] PiecesPromotion(bool blanc) => blanc
    ? [L.TypePiece.ReineBlanche, L.TypePiece.TourBlanche, L.TypePiece.FouBlanc, L.TypePiece.CavalierBlanc]
    : [L.TypePiece.ReineNoire, L.TypePiece.TourNoire, L.TypePiece.FouNoir, L.TypePiece.CavalierNoir];

bool EstPromotion(string source, string destination)
{
    L.TypePiece piece = L.PiecesEchiquier[L.RenvoieCaseIndex120(source)];
    return (piece == L.TypePiece.PionBlanc && destination[1] == '8') || (piece == L.TypePiece.PionNoir && destination[1] == '1');
}

long Perft(int profondeur)
{
    var coups = L.CoupsLegaux();
    if (profondeur == 1)
        return coups.Sum(c => EstPromotion(c.Source, c.Destination) ? 4 : 1);
    long total = 0;
    foreach (var (source, destination) in coups)
    {
        bool blanc = L.QuiJoue == L.ColorPiece.Blanc;
        L.TypePiece[] choix = EstPromotion(source, destination) ? PiecesPromotion(blanc) : [L.TypePiece.Vide];
        foreach (L.TypePiece promotion in choix)
        {   // Chaque coup est joué sur une copie : on revient automatiquement à la position de départ
            total += L.CalculerSurCopie(() =>
            {
                L.PromotionPiece = promotion;
                L.BloquerChoixPromo = true;
                L.ExecutionCoup(source, destination);
                if (!L.CoupValide)
                    throw new InvalidOperationException($"Coup légal refusé par ExecutionCoup : {source}{destination}");
                return Perft(profondeur - 1);
            });
            ViderListes();
        }
    }
    return total;
}

void TestPerft(string nom, string fen, int profondeur, long attendu)
{
    Charger(fen);
    var chrono = Stopwatch.StartNew();
    long obtenu = Perft(profondeur);
    Verifie($"Perft {nom} profondeur {profondeur}", obtenu == attendu, $"{obtenu} (attendu {attendu}, {chrono.Elapsed.TotalSeconds:F1} s)");
}

const string Initiale = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
const string Kiwipete = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
const string Position3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
const string Position4 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
const string Position5 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";

TestPerft("position initiale", Initiale, 3, 8902);
TestPerft("Kiwipete", Kiwipete, 2, 2039);
TestPerft("position 3", Position3, 3, 2812);
TestPerft("position 4", Position4, 2, 264);
TestPerft("position 5", Position5, 2, 1486);
if (complet)
{
    TestPerft("position initiale", Initiale, 4, 197281);
    TestPerft("Kiwipete", Kiwipete, 3, 97862);
    TestPerft("position 3", Position3, 4, 43238);
    TestPerft("position 4", Position4, 3, 9467);
    TestPerft("position 5", Position5, 3, 62379);
}

Console.WriteLine(nombreEchecs == 0 ? "\nTous les tests passent." : $"\n{nombreEchecs} test(s) en échec.");
return nombreEchecs == 0 ? 0 : 1;
