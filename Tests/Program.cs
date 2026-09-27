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
    L.ViderCoups();
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
Verifie("Mat : '#' posé par la logique dans toutes les notations",
    L.ListeCoupsPgnIntl[^1] == "4. Qxf7# " && L.ListeCoupsPgnFr[^1] == "4. Dxf7# " && L.ListeCoupsNal[^1].Trim().EndsWith('#') && L.ListeCoupsUci[^1] == "h5f7 ",
    $"{L.ListeCoupsPgnIntl[^1]}| {L.ListeCoupsPgnFr[^1]}| {L.ListeCoupsNal[^1]}| {L.ListeCoupsUci[^1]}");

Verifie("Traduction français -> anglais des coups",
    L.NotationInternationale("Tdxe1=D+") == "Rdxe1=Q+" && L.NotationInternationale("Rf1") == "Kf1" && L.NotationInternationale("Cbd7") == "Nbd7"
    && L.NotationInternationale("Fxc4") == "Bxc4" && L.NotationInternationale("O-O-O#") == "O-O-O#" && L.NotationInternationale("exd8=C") == "exd8=N",
    L.NotationInternationale("Tdxe1=D+"));
Verifie("Nom de case invalide : -1 sans plantage ni message",
    L.RenvoieCaseIndex120("a1") == 21 && L.RenvoieCaseIndex120("h8") == 98 && L.RenvoieCaseIndex120("") == -1 && L.RenvoieCaseIndex120("z9") == -1 && L.RenvoieCaseIndex120(null) == -1,
    $"a1={L.RenvoieCaseIndex120("a1")}, h8={L.RenvoieCaseIndex120("h8")}");

// ═══════════════ Nulles ═══════════════
Console.WriteLine("── Nulles ──");

void Joue(params string[] coups)
{   // coups au format "g1f3"
    foreach (string c in coups)
        L.ExecutionCoup(c[..2], c[2..4]);
}

Charger(L.FenDepart);
Joue("g1f3", "g8f6", "f3g1", "f6g8", "g1f3", "g8f6", "f3g1");
string avantRepetition = L.RaisonNulle() ?? "aucune";
Joue("f6g8");   // la position initiale apparaît pour la 3e fois (départ, après 4 et après 8 demi-coups)
Verifie("Répétition : la position initiale compte", avantRepetition == "aucune" && L.RaisonNulle() == "Nulle par répétition", $"avant : {avantRepetition}, après : {L.RaisonNulle()}");

Charger(L.FenDepart);
L.AjouteCoup(new Coup { Fen = "4k3/8/8/8/8/8/8/4K2R w K - 0 1" });
L.AjouteCoup(new Coup { Fen = "4k3/8/8/8/8/8/8/4K2R b K - 0 1" });
L.AjouteCoup(new Coup { Fen = "4k3/8/8/8/8/8/8/4K2R w - - 0 1" });
Verifie("Répétition : trait ou droits de roque différents = positions différentes", !L.TripleRepetition(), "même placement 3 fois, mais pas la même position");

Charger("4k3/8/8/8/8/8/8/R3K3 w - - 99 80");
Joue("a1a2");
Verifie("50 coups : nulle au 100e demi-coup sans prise ni coup de pion", L.RaisonNulle() == "Nulle (règle des 50 coups)", L.RaisonNulle() ?? "aucune");
Charger("4k3/p7/8/8/8/8/P7/R3K3 w - - 99 80");
Joue("a2a3");
Verifie("50 coups : un coup de pion remet le compteur à zéro", L.RaisonNulle() == null, L.RaisonNulle() ?? "aucune");

Charger("4k3/8/8/8/8/8/3n4/2B1K3 w - - 0 1");
Joue("c1d2");
Verifie("Matériel insuffisant : roi et fou contre roi", L.RaisonNulle() == "Nulle (matériel insuffisant)", L.RaisonNulle() ?? "aucune");
bool MaterielInsuffisantDans(string fen) { Charger(fen); return L.MaterielInsuffisant(); }
Verifie("Matériel insuffisant : roi contre roi", MaterielInsuffisantDans("4k3/8/8/8/8/8/8/4K3 w - - 0 1"), "K-K");
Verifie("Matériel insuffisant : fous de même couleur", MaterielInsuffisantDans("4kb2/8/8/8/8/8/8/2B1K3 w - - 0 1"), "c1 et f8 : cases sombres");
Verifie("Mat possible : fous de couleurs opposées", !MaterielInsuffisantDans("4k1b1/8/8/8/8/8/8/2B1K3 w - - 0 1"), "c1 sombre, g8 claire");
Verifie("Mat possible : deux cavaliers", !MaterielInsuffisantDans("4k3/8/8/8/8/8/8/1N2K1N1 w - - 0 1"), "K+C+C contre K");
Verifie("Mat possible : un pion", !MaterielInsuffisantDans("4k3/8/8/8/8/8/4P3/4K3 w - - 0 1"), "K+P contre K");

Charger("7k/6Q1/6K1/8/8/8/8/8 b - - 0 1");
bool matAuTrait = L.CampAuTraitEnEchec();
string fenAvant = L.RetourneChaineFenActuel();
Charger("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1");
Verifie("Camp au trait en échec : oui si maté, non si pat (position inchangée)",
    matAuTrait && !L.CampAuTraitEnEchec() && fenAvant == "7k/6Q1/6K1/8/8/8/8/8 b - - 0 1", $"mat : {matAuTrait}");

// Un mat au 100e demi-coup reste un mat (pas de nulle signalée)
Charger("6k1/5ppp/8/8/8/8/8/R5K1 w - - 99 80");
Joue("a1a8");
Verifie("Mat prioritaire sur la règle des 50 coups", L.EchecetMat && L.RaisonNulle() == null, $"mat : {L.EchecetMat}, nulle : {L.RaisonNulle() ?? "aucune"}");

// ═══════════════ Liste des coups ═══════════════
Console.WriteLine("── Liste des coups ──");

Charger("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
L.ExecutionCoup("e2", "e4"); L.ExecutionCoup("e7", "e5");
Verifie("Un coup = toutes ses notations au même index",
    L.ListeCoups.Count == 2 && L.ListeCoupsFen[1].StartsWith("rnbqkbnr/pppp1ppp/8/4p3/4P3/") && L.ListeCoupsPgnFr[0] == "1. e4 " && L.ListeCoupsNal[1] == "e7-e5 " && L.ListeCoupsUci[1] == "e7e5 ",
    $"{L.ListeCoupsPgnFr[0]}| {L.ListeCoupsNal[1]}| {L.ListeCoupsUci[1]}");
Verifie("Retour arrière : les notations du dernier coup partent ensemble",
    L.RetireDernierCoup() && L.ListeCoups.Count == 1 && L.ListeCoupsNal.Count == 1 && L.ListeCoupsUci.Count == 1, $"{L.ListeCoups.Count} coup(s)");

// Partie commencée depuis un FEN : la position de départ est un élément sans coup, en tête de liste
string fenDepartPartie = "r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R w KQkq - 2 3";
Charger(fenDepartPartie);
L.AjoutePositionDeDepart(fenDepartPartie);
L.ExecutionCoup("f1", "b5");
Verifie("Départ FEN : notations alignées (le coup est à l'index 1)",
    L.ListeCoups.Count == 2 && L.ListeCoups[0].EstPositionDeDepart && L.ListeCoupsNal[0] == "" && L.ListeCoupsPgnIntl[1] == "3. Bb5 " && L.ListeCoupsFen[0] == fenDepartPartie,
    $"Nal[0]='{L.ListeCoupsNal[0]}', PGN[1]='{L.ListeCoupsPgnIntl[1]}'");
bool retire1 = L.RetireDernierCoup();
bool retire2 = L.RetireDernierCoup();
Verifie("Départ FEN : le retour arrière ne retire jamais la position de départ",
    retire1 && !retire2 && L.ListeCoups.Count == 1 && L.ListeCoupsFen[^1] == fenDepartPartie, $"{L.ListeCoups.Count} élément(s)");

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

// Enregistrement des préférences : commentaires, ordre et clés inconnues conservés, valeurs remplacées, clés manquantes ajoutées
string iniSauve = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BrunoGUI_test_sauve.ini");
System.IO.File.WriteAllLines(iniSauve, [
    "; Mon commentaire",
    "Casesombre = Peru",
    "CleInconnue = gardee",
    "NombrelignesPV = 3",
    "Forcemoteur = abc" ]);         // valeur illisible : ignorée à la lecture
Parametres prefs = new();
prefs.ChargerDepuisIni(iniSauve);
bool lectureTolerante = prefs.ForceMoteur == 1850;
prefs.CaseSombre = Parametres.FormatCouleur(System.Drawing.Color.FromArgb(181, 136, 99));
prefs.NombreLignesPV = 2;
prefs.ForceMaximale = false;
prefs.CouleurMoteur = "Blancs";
prefs.TailleHachageMo = 512;
prefs.SauverPreferences(iniSauve);
string[] lignesSauvees = System.IO.File.ReadAllLines(iniSauve);
Parametres relus = new();
relus.ChargerDepuisIni(iniSauve);
System.IO.File.Delete(iniSauve);
Verifie("Préférences : valeur numérique illisible ignorée", lectureTolerante, "Forcemoteur = abc -> 1850");
Verifie("Préférences : commentaire, ordre et clé inconnue conservés",
    lignesSauvees[0] == "; Mon commentaire" && lignesSauvees[1] == "Casesombre = #B58863" && lignesSauvees[2] == "CleInconnue = gardee" && lignesSauvees[3] == "NombrelignesPV = 2",
    string.Join(" | ", lignesSauvees.Take(4)));
Verifie("Préférences : relecture de toutes les valeurs",
    relus.CaseSombre == "#B58863" && relus.NombreLignesPV == 2 && !relus.ForceMaximale && relus.CouleurMoteur == "Blancs" && relus.TailleHachageMo == 512 && relus.ForceMoteur == 1850,
    $"{relus.CaseSombre}, PV={relus.NombreLignesPV}, max={relus.ForceMaximale}, moteur={relus.CouleurMoteur}, Hash={relus.TailleHachageMo}");

// Deux fichiers : BrunoGUI.ini (valeurs par défaut, jamais écrit) + BrunoGUI.preferences.ini (préférences, par-dessus)
string dossierTest = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "BrunoGUI_test_prefs");
System.IO.Directory.CreateDirectory(dossierTest);
string cheminDefaut = System.IO.Path.Combine(dossierTest, Parametres.FichierParDefaut);
string cheminPrefs = System.IO.Path.Combine(dossierTest, Parametres.FichierPreferences);
System.IO.File.Delete(cheminPrefs);
System.IO.File.WriteAllLines(cheminDefaut, ["Moteur = Stockfish", "Casesombre = Peru", "Forcemoteur = 1950", "EloHumain = 1767", "Palette = Office2010Silver"]);
string defautAvant = System.IO.File.ReadAllText(cheminDefaut);
Parametres p1 = Parametres.Charger(dossierTest);
Verifie("Sans fichier de préférences : valeurs par défaut", p1.CaseSombre == "Peru" && p1.ForceMoteur == 1950 && p1.Palette == "Office2010Silver", $"{p1.CaseSombre}, {p1.ForceMoteur}");
p1.CaseSombre = "#8080FF";
p1.ForceMoteur = 2026;
p1.SauverPreferences(cheminPrefs);
string[] lignesPrefs = System.IO.File.ReadAllLines(cheminPrefs);
Parametres p2 = Parametres.Charger(dossierTest);
Verifie("Préférences créées avec un commentaire d'en-tête, sans les clés du fichier par défaut",
    lignesPrefs[0].StartsWith(';') && lignesPrefs.Contains("Casesombre = #8080FF") && !lignesPrefs.Any(l => l.StartsWith("Moteur") || l.StartsWith("EloHumain") || l.StartsWith("Palette")),
    $"{lignesPrefs.Length} lignes");
Verifie("Préférences lues par-dessus les valeurs par défaut", p2.CaseSombre == "#8080FF" && p2.ForceMoteur == 2026 && p2.Palette == "Office2010Silver" && p2.EloHumain == "1767", $"{p2.CaseSombre}, {p2.ForceMoteur}, {p2.Palette}");
Verifie("Le fichier des valeurs par défaut n'est jamais modifié", System.IO.File.ReadAllText(cheminDefaut) == defautAvant, Parametres.FichierParDefaut);
// Vérification automatique de Stockfish : au plus tous les VerificationMiseAJourJours jours, date gardée dans les préférences
var jour = new DateTime(2026, 9, 27);
Parametres m = new();
bool jamaisFaite = m.VerificationMiseAJourDue(jour);
m.DerniereVerificationMiseAJour = jour.AddDays(-29);
bool apres29Jours = m.VerificationMiseAJourDue(jour);
m.DerniereVerificationMiseAJour = jour.AddDays(-30);
bool apres30Jours = m.VerificationMiseAJourDue(jour);
m.VerificationMiseAJourJours = 0;
bool desactivee = m.VerificationMiseAJourDue(jour);
Verifie("Mise à jour : due si jamais faite ou après 30 jours, jamais si 0",
    jamaisFaite && !apres29Jours && apres30Jours && !desactivee, $"jamais : {jamaisFaite}, 29 j : {apres29Jours}, 30 j : {apres30Jours}, 0 : {desactivee}");
p2.DerniereVerificationMiseAJour = jour;
p2.SauverPreferences(cheminPrefs);
Parametres p3 = Parametres.Charger(dossierTest);
Verifie("Mise à jour : date de la dernière vérification enregistrée et relue",
    p3.DerniereVerificationMiseAJour == jour && System.IO.File.ReadAllLines(cheminPrefs).Contains("DerniereVerificationMiseAJour = 2026-09-27"),
    $"{p3.DerniereVerificationMiseAJour:yyyy-MM-dd}");
System.IO.Directory.Delete(dossierTest, true);
Verifie("FormatCouleur : nom pour une couleur nommée", Parametres.FormatCouleur(System.Drawing.Color.Peru) == "Peru", Parametres.FormatCouleur(System.Drawing.Color.Peru));

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
