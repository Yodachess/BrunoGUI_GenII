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
using System.Collections.Generic;
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

Charger("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 10 20");
L.ExecutionCoup("e1", "g1");
Verifie("50 coups : le roque compte +1 (une seule fois)", L.SansPrise == 11, "SansPrise = " + L.SansPrise);
Charger("4k3/8/8/3pP3/8/8/8/4K3 w - d6 7 30");
L.ExecutionCoup("e5", "d6");
Verifie("50 coups : remise à zéro sur prise en passant", L.CoupValide && L.SansPrise == 0, "SansPrise = " + L.SansPrise);

Charger("8/R1P4k/8/8/8/8/8/4K3 w - - 0 1");
L.ExecutionCoup("c7", "c8", L.TypePiece.CavalierBlanc);
Verifie("Promotion avec échec à la découverte", DernierCoupPgn().Contains('+'), DernierCoupPgn());

Charger("8/4P2k/8/8/8/8/8/4K3 w - - 0 1");
L.ExecutionCoup("e7", "e8");
Verifie("Promotion sans choix (aucune interface) : dame", L.PiecesEchiquier[L.RenvoieCaseIndex120("e8")] == L.TypePiece.ReineBlanche, DernierCoupPgn());
L.ColorPiece? couleurDemandee = null;
L.ChoixPromotion = couleur => { couleurDemandee = couleur; return L.TypePiece.TourNoire; };
Charger("4K3/8/8/8/8/8/4p2k/8 b - - 0 1");
L.ExecutionCoup("e2", "e1");
Verifie("Promotion : la pièce choisie par l'interface est posée",
    couleurDemandee == L.ColorPiece.Noir && L.PiecesEchiquier[L.RenvoieCaseIndex120("e1")] == L.TypePiece.TourNoire && DernierCoupPgn().StartsWith("e1=R"),
    $"{couleurDemandee} {DernierCoupPgn()}");
Charger("8/4P2k/8/8/8/8/8/4K3 w - - 0 1");
couleurDemandee = null;
L.ExecutionCoup("e7", "e8", L.TypePiece.FouNoir);
Verifie("Promotion imposée : l'interface n'est pas sollicitée, la pièce prend la couleur du pion",
    couleurDemandee == null && L.PiecesEchiquier[L.RenvoieCaseIndex120("e8")] == L.TypePiece.FouBlanc, DernierCoupPgn());
L.ChoixPromotion = null;
Charger("8/4P2k/8/8/8/8/8/4K3 w - - 0 1");
Verifie("Promotion PGN : la pièce du coup est posée",
    GestionPartiePgn.DecodeCoupPartie("e8=N") && L.PiecesEchiquier[L.RenvoieCaseIndex120("e8")] == L.TypePiece.CavalierBlanc, DernierCoupPgn());

L.ColorPiece? campMate = null, campPat = null, campAuTrait = null;
L.AfficheEchecEtMat += c => campMate = c;
L.AffichePat += c => campPat = c;
L.AfficheTour += c => campAuTrait = c;
Charger("7k/8/6K1/8/8/8/8/5Q2 w - - 0 1");
L.ExecutionCoup("f1", "f8");
Verifie("Mat : l'événement donne le camp maté", campMate == L.ColorPiece.Noir && campPat == null && campAuTrait == L.ColorPiece.Noir, $"maté {campMate}, pat {campPat}");
campMate = campPat = null;
Charger("7k/8/6K1/8/8/8/8/5Q2 w - - 0 1");
L.ExecutionCoup("f1", "f7");
Verifie("Pat : l'événement donne le camp pat (pas de mat)", campPat == L.ColorPiece.Noir && campMate == null, $"maté {campMate}, pat {campPat}");

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
    bool relu = GestionPartiePgn.DecodeCoupPartie(coup);
    Verifie(nom + " : relecture PGN", relu && L.RetourneChaineFenActuel() == fenApresCoup, L.RetourneChaineFenActuel());
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

// Numérotation des demandes au moteur ("go" / "bestmove")
var suivi = new SuiviDemandesMoteur();
suivi.DemandeEnvoyee();                                  // demande n° 1
bool reflexion1 = suivi.EnAttenteNonAbandonnee;
int reponse1 = suivi.ReponseRecue();
Verifie("Demandes : réponse normale prise en compte", reflexion1 && reponse1 == 1 && !suivi.EstAbandonnee(1) && !suivi.EnAttente, $"réponse n° {reponse1}");

suivi.DemandeEnvoyee();                                  // demande n° 2
bool stopEnvoye = suivi.Abandonner();                    // ex : retour arrière pendant la réflexion
bool plusEnReflexion = !suivi.EnAttenteNonAbandonnee;
suivi.DemandeEnvoyee();                                  // demande n° 3 (nouvelle position)
int reponse2 = suivi.ReponseRecue();                     // bestmove de la n° 2, arrivé après le "stop"
int reponse3 = suivi.ReponseRecue();
Verifie("Demandes : réponse à une demande abandonnée ignorée, la suivante prise en compte",
    stopEnvoye && plusEnReflexion && reponse2 == 2 && suivi.EstAbandonnee(2) && reponse3 == 3 && !suivi.EstAbandonnee(3),
    $"n° 2 abandonnée : {suivi.EstAbandonnee(2)}, n° 3 abandonnée : {suivi.EstAbandonnee(3)}");
Verifie("Demandes : abandonner sans réflexion en cours ne fait rien (pas de 'stop')", !suivi.Abandonner(), "rien en attente");
suivi.DemandeEnvoyee();
suivi.Abandonner();
Verifie("Demandes : abandonner deux fois n'envoie qu'un seul 'stop'", !suivi.Abandonner() && suivi.EnAttente, "déjà abandonnée");
suivi.Reinitialiser();
Verifie("Demandes : remise à zéro pour un nouveau processus moteur", !suivi.EnAttente && suivi.NumeroEnCours == 1, $"n° en cours : {suivi.NumeroEnCours}");

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
prefs.CouleurMoteur = L.ColorPiece.Blanc;
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
    relus.CaseSombre == "#B58863" && relus.NombreLignesPV == 2 && !relus.ForceMaximale && relus.CouleurMoteur == L.ColorPiece.Blanc && relus.TailleHachageMo == 512 && relus.ForceMoteur == 1850,
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

// Position affichée (parcours) : lire une FEN sans toucher à la partie, et convertir une variante sur cette position
Charger("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
L.ExecutionCoup("e2", "e4"); L.ExecutionCoup("e7", "e5"); L.ExecutionCoup("g1", "f3");
string fenPartie = L.RetourneChaineFenActuel();
int dessinsAvantLecture = nombreDessins;
Position passee = L.PositionDepuisFen(L.ListeCoupsFen[0]);      // position après 1. e4
Verifie("PositionDepuisFen : position lue sans toucher à la partie ni dessiner",
    passee.QuiJoue == L.ColorPiece.Noir && passee.Pieces[L.RenvoieCaseIndex120("e4")] == L.TypePiece.PionBlanc && passee.Pieces[L.RenvoieCaseIndex120("e5")] == L.TypePiece.Vide
    && passee.IndexCaseEnPassant == L.RenvoieCaseIndex120("e3") && L.RetourneChaineFenActuel() == fenPartie && nombreDessins == dessinsAvantLecture,
    L.RetourneChaineFenActuel());
string varianteSurPassee = Outils.VarianteUciVersPgn("c7c5 g1f3", L.DemiCoupAvant(passee), false, passee);
Verifie("Variante convertie sur la position affichée (et numérotée d'après elle)", varianteSurPassee.Trim() == "1 ... c5 2. Cf3", varianteSurPassee.Trim());
Verifie("DemiCoupAvant : numéro du dernier demi-coup joué",
    L.DemiCoupAvant(L.PositionDepuisFen(L.FenDepart)) == 0 && L.DemiCoupAvant(passee) == 0 && L.DemiCoupAvant(L.PositionActuelle) == 2,
    $"départ {L.DemiCoupAvant(L.PositionDepuisFen(L.FenDepart))}, après e4 {L.DemiCoupAvant(passee)}, après Cf3 {L.DemiCoupAvant(L.PositionActuelle)}");
int dessinsAvantPosition = nombreDessins;
L.DessinPosition(passee);
Verifie("DessinPosition : 64 cases dessinées, partie inchangée", nombreDessins - dessinsAvantPosition == 64 && L.RetourneChaineFenActuel() == fenPartie, $"{nombreDessins - dessinsAvantPosition} cases");

Position vide = new();
Verifie("Position neuve : 120 cases, 64 vides et 56 bordures",
    vide.Pieces.Count == 120 && vide.Pieces.Count(p => p == L.TypePiece.Vide) == 64 && vide.Pieces.Count(p => p == L.TypePiece.Bordure) == 56,
    $"{vide.Pieces.Count} cases");

// ═══════════════ Partie (déroulement, sans interface) ═══════════════
Console.WriteLine("── Partie ──");

Partie partie = new();
Verifie("Partie neuve : aucune partie, rien à annuler",
    partie.Mode == ModePartie.AucunePartie && !partie.AnnulerDernierCoup() && !partie.MoteurAuTrait && !partie.HumainAuTrait, partie.Mode.ToString());

Charger(L.FenDepart);
partie.Commencer(Joueur.Humain, Joueur.Moteur);
bool humainAuTraitDebut = partie.HumainAuTrait;
L.ExecutionCoup("e2", "e4");
Verifie("Humain (Blancs) contre moteur : au moteur de répondre après le coup de l'humain",
    partie.EnCours && humainAuTraitDebut && partie.MoteurAuTrait && !partie.EntreHumains, $"trait aux {L.QuiJoue}");
L.ExecutionCoup("e7", "e5"); L.ExecutionCoup("e1", "e2");
Verifie("Annuler un coup : trait et droits de roque rétablis (depuis la FEN)",
    partie.AnnulerDernierCoup() && L.QuiJoue == L.ColorPiece.Blanc && L.PetitRoqueBlancPossible && L.GrandRoqueBlancPossible && L.ListeCoups.Count == 2,
    L.RetourneChaineFenActuel());
bool annule2 = partie.AnnulerDernierCoup(), annule3 = partie.AnnulerDernierCoup(), annule4 = partie.AnnulerDernierCoup();
Verifie("Annuler jusqu'au début : position initiale, puis plus rien à annuler",
    annule2 && annule3 && !annule4 && L.ListeCoups.Count == 0 && L.RetourneChaineFenActuel() == L.FenDepart, L.RetourneChaineFenActuel());

// Mat du lion (1. f3 e5 2. g4 Dh4#) : la partie terminée reprend après un retour arrière
Charger(L.FenDepart);
partie.Commencer(Joueur.Moteur, Joueur.Humain);
L.ExecutionCoup("f2", "f3"); L.ExecutionCoup("e7", "e5"); L.ExecutionCoup("g2", "g4"); L.ExecutionCoup("d8", "h4");
bool matAvant = L.EchecetMat;
partie.Terminer();
bool rienAuTrait = !partie.MoteurAuTrait && !partie.HumainAuTrait;
Verifie("Partie terminée par un mat : personne n'est au trait", matAvant && partie.Mode == ModePartie.Terminee && rienAuTrait, $"mat : {matAvant}");
Verifie("Retour arrière après le mat : la partie reprend, plus de mat, Noirs au trait",
    partie.AnnulerDernierCoup() && partie.EnCours && !L.EchecetMat && L.QuiJoue == L.ColorPiece.Noir && L.ListeCoups.Count == 3 && partie.HumainAuTrait,
    L.RetourneChaineFenActuel());

// Partie depuis une position FEN (Noirs au trait) : l'humain joue le camp au trait
string fenNoirsAuTrait = "r1bqkbnr/pppp1ppp/2n5/4p3/4P3/5N2/PPPP1PPP/RNBQKB1R b KQkq - 3 3";
Charger(fenNoirsAuTrait);
L.AjoutePositionDeDepart(fenNoirsAuTrait);
partie.CommencerDepuisPosition();
Verifie("Départ FEN : l'humain a le camp au trait, le moteur l'autre",
    partie.DepuisPosition && partie.Noirs == Joueur.Humain && partie.Blancs == Joueur.Moteur && partie.HumainAuTrait, $"Blancs {partie.Blancs}, Noirs {partie.Noirs}");
L.ExecutionCoup("g8", "f6");
bool annuleFen1 = partie.AnnulerDernierCoup(), annuleFen2 = partie.AnnulerDernierCoup();
Verifie("Départ FEN : retour arrière jusqu'à la position de départ, jamais avant (numéro de coup 3 + 0,5)",
    annuleFen1 && !annuleFen2 && L.RetourneChaineFenActuel() == fenNoirsAuTrait && L.NombreCoupsJoues == 3.5f, $"{L.RetourneChaineFenActuel()} / {L.NombreCoupsJoues}");

partie.MoteurPrendLeTrait();
Verifie("Ordinateur joue : le moteur prend le camp au trait", partie.Noirs == Joueur.Moteur && partie.Blancs == Joueur.Humain && partie.MoteurAuTrait, $"Blancs {partie.Blancs}, Noirs {partie.Noirs}");
partie.Commencer(Joueur.Humain, Joueur.Humain);
Verifie("Entre humains : jamais au moteur", partie.EntreHumains && !partie.MoteurAuTrait && !partie.DepuisPosition, $"Blancs {partie.Blancs}, Noirs {partie.Noirs}");
L.ExecutionCoup("g8", "f6");
partie.PasserEnLectureSeule();
Verifie("Lecture seule (PGN) : pas de retour arrière", !partie.AnnulerDernierCoup() && L.ListeCoups.Count == 2, $"{L.ListeCoups.Count} élément(s)");

// Reprendre la partie d'ici
Charger(L.FenDepart);
partie.Commencer(Joueur.Humain, Joueur.Moteur);
L.ExecutionCoup("e2", "e4"); L.ExecutionCoup("e7", "e5"); L.ExecutionCoup("g1", "f3"); L.ExecutionCoup("b8", "c6");
int supprimesIci = partie.ReprendreDepuis(0);      // position après 1. e4
Verifie("Reprendre ici : coups suivants supprimés, position rétablie, au moteur (Noirs) de jouer",
    supprimesIci == 3 && L.ListeCoups.Count == 1 && L.QuiJoue == L.ColorPiece.Noir && partie.MoteurAuTrait && L.RetourneChaineFenActuel() == L.ListeCoupsFen[0],
    $"{supprimesIci} supprimé(s), {L.RetourneChaineFenActuel()}");
Verifie("Reprendre ici à la dernière position : rien à supprimer", partie.ReprendreDepuis(0) == 0 && L.ListeCoups.Count == 1, $"{L.ListeCoups.Count} coup(s)");
Verifie("Reprendre ici depuis la position initiale (-1) : plus aucun coup", partie.ReprendreDepuis(-1) == 1 && L.ListeCoups.Count == 0 && L.RetourneChaineFenActuel() == L.FenDepart, L.RetourneChaineFenActuel());

// Partie PGN en lecture seule, terminée par un mat : elle devient jouable, l'humain a le camp au trait
Charger(L.FenDepart);
partie.Commencer(Joueur.Humain, Joueur.Humain);
L.ExecutionCoup("f2", "f3"); L.ExecutionCoup("e7", "e5"); L.ExecutionCoup("g2", "g4"); L.ExecutionCoup("d8", "h4");
partie.Terminer();
partie.PasserEnLectureSeule();
int supprimesPgn = partie.ReprendreDepuis(1);      // position après 1... e5 : Blancs au trait
Verifie("Reprendre ici une partie PGN : jouable, l'humain a le camp au trait, plus de mat",
    supprimesPgn == 2 && partie.EnCours && partie.Blancs == Joueur.Humain && partie.Noirs == Joueur.Moteur && partie.HumainAuTrait && !L.EchecetMat,
    $"{supprimesPgn} supprimé(s), Blancs {partie.Blancs}, Noirs {partie.Noirs}");

// Partie PGN reprise à sa position finale : rien n'est supprimé, mais elle devient jouable (humain au camp au trait)
Charger(L.FenDepart);
partie.Commencer(Joueur.Humain, Joueur.Humain);
L.ExecutionCoup("e2", "e4"); L.ExecutionCoup("e7", "e5"); L.ExecutionCoup("g1", "f3");
partie.PasserEnLectureSeule();
int supprimesFin = partie.ReprendreDepuis(L.ListeCoups.Count - 1);
Verifie("Reprendre une partie PGN à sa position finale : jouable, rien de supprimé, humain aux Noirs",
    supprimesFin == 0 && partie.EnCours && L.ListeCoups.Count == 3 && partie.Noirs == Joueur.Humain && partie.Blancs == Joueur.Moteur && partie.HumainAuTrait,
    $"{supprimesFin} supprimé(s), mode {partie.Mode}, Blancs {partie.Blancs}, Noirs {partie.Noirs}");
Verifie("Reprendre à la position finale d'une partie qui n'est pas en lecture seule : rien ne change",
    partie.ReprendreDepuis(L.ListeCoups.Count - 1) == 0 && L.ListeCoups.Count == 3, $"{L.ListeCoups.Count} coup(s)");

// Partie depuis un FEN : on ne remonte jamais avant la position de départ
Charger(fenNoirsAuTrait);
L.AjoutePositionDeDepart(fenNoirsAuTrait);
partie.CommencerDepuisPosition();
L.ExecutionCoup("g8", "f6"); L.ExecutionCoup("f1", "c4");
Verifie("Reprendre ici une partie FEN à sa position de départ (index 0)",
    partie.ReprendreDepuis(0) == 2 && L.ListeCoups.Count == 1 && L.RetourneChaineFenActuel() == fenNoirsAuTrait, L.RetourneChaineFenActuel());

// ═══════════════ Parties PGN (lecture et écriture) ═══════════════
Console.WriteLine("── PGN ──");

string coupsExtraits = ParseurPgn.ExtraireCoups("[Event \"x\"]\n1. e4 (1. d4 d5 (1... Nf6 2. c4) 2. c4) e5 2. Nf3 {commentaire [%clk 0:01:00]} Nc6 ; fin de ligne\n3. Bb5 *");
string[] elementsExtraits = coupsExtraits.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);     // espaces et fins de ligne
Verifie("Variantes imbriquées, commentaires {} et ; ignorés",
    elementsExtraits.SequenceEqual(new[] { "1.", "e4", "e5", "2.", "Nf3", "Nc6", "3.", "Bb5", "*" }), string.Join(" ", elementsExtraits));

Charger("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
bool roqueZeros = GestionPartiePgn.DecodeCoupPartie("0-0");
bool grandRoqueZeros = GestionPartiePgn.DecodeCoupPartie("0-0-0+");
Verifie("Roque noté avec des zéros (0-0, 0-0-0)",
    roqueZeros && grandRoqueZeros && L.PiecesEchiquier[L.RenvoieCaseIndex120("g1")] == L.TypePiece.RoiBlanc && L.PiecesEchiquier[L.RenvoieCaseIndex120("c8")] == L.TypePiece.RoiNoir,
    L.RetourneChaineFenActuel());

Charger(L.FenDepart);
bool illegal = GestionPartiePgn.DecodeCoupPartie("Nf6");
bool malForme = GestionPartiePgn.DecodeCoupPartie("e9");
bool inconnu = GestionPartiePgn.DecodeCoupPartie("Zz9");
Verifie("Coup illégal ou mal formé : refusé sans exception, rien n'est joué",
    !illegal && !malForme && !inconnu && L.ListeCoups.Count == 0 && L.RetourneChaineFenActuel() == L.FenDepart && !L.EchecetMat,
    $"{illegal} {malForme} {inconnu}, {L.RetourneChaineFenActuel()}");
Verifie("Numéros de coups et résultat ignorés", GestionPartiePgn.DecodeCoupPartie("12.") && GestionPartiePgn.DecodeCoupPartie("1-0") && L.ListeCoups.Count == 0, "");

// Partie FEN commençant par les Noirs : relecture, feuille de partie et écriture PGN (balises SetUp/FEN, "n... coup", "*")
Charger(fenNoirsAuTrait);
L.AjoutePositionDeDepart(fenNoirsAuTrait);
bool noirJoue = GestionPartiePgn.DecodeCoupPartie("Nf6");
bool blancJoue = GestionPartiePgn.DecodeCoupPartie("Bc4");
List<string[]> feuille = FeuilleDePartie.Lignes(L.ListeCoups);
Verifie("Feuille de partie commençant par un coup noir : 3. | ... | Cf6, puis 4. | Fc4",
    noirJoue && blancJoue && feuille.Count == 2 && feuille[0].SequenceEqual(new[] { "3.", "...", "Cf6" }) && feuille[1].SequenceEqual(new[] { "4.", "Fc4", "" })
    && FeuilleDePartie.CommenceParLesNoirs(L.ListeCoups),
    string.Join(" / ", feuille.Select(l => string.Join("|", l))));
string pgnEcrit = GestionPartiePgn.RetourneContenuPgn(new PartieEchecsPGN { Result = "" }, "Intl");
Verifie("PGN écrit : balises SetUp et FEN, premier coup noir numéroté, résultat * pour une partie en cours",
    pgnEcrit.Contains("[SetUp \"1\"]") && pgnEcrit.Contains("[FEN \"" + fenNoirsAuTrait + "\"]") && pgnEcrit.Contains("3... Nf6 4. Bc4") && pgnEcrit.TrimEnd().EndsWith("*")
    && pgnEcrit.Contains("[Result \"*\"]"),
    pgnEcrit.Replace("\n", " "));
PartieEchecsPGN partieRelue = FichierPartiePgn.DecodePartiePGN(pgnEcrit);
Verifie("PGN relu : la balise FEN est retrouvée", partieRelue.Fen == fenNoirsAuTrait, partieRelue.Fen ?? "(aucune)");

// Chargement d'une position FEN et d'une partie PGN complète (ChargementPartie)
Partie partieChargee = new();
Charger(L.FenDepart);
L.ExecutionCoup("e2", "e4");
Verifie("FEN incomplète : refusée, la partie en cours ne change pas",
    !ChargementPartie.ChargerPosition("8/8/8/8 w - -", partieChargee) && L.ListeCoups.Count == 1 && partieChargee.Mode == ModePartie.AucunePartie, $"{L.ListeCoups.Count} coup(s)");
bool positionChargee = ChargementPartie.ChargerPosition("  " + fenNoirsAuTrait + "\r\n", partieChargee);
Verifie("FEN chargée (blancs autour compris) : position de départ, humain au trait (Noirs)",
    positionChargee && L.ListeCoups.Count == 1 && L.ListeCoups[0].EstPositionDeDepart && L.RetourneChaineFenActuel() == fenNoirsAuTrait
    && partieChargee.DepuisPosition && partieChargee.HumainAuTrait && partieChargee.Noirs == Joueur.Humain,
    L.RetourneChaineFenActuel());

PartieEchecsPGN pgnComplet = FichierPartiePgn.DecodePartiePGN(
    "[Event \"x\"]\n[Result \"1-0\"]\n\n1. e4 e5 (1... c5 2. Nf3 (2. c3 d5) 2... d6) 2. Nf3 Nc6 {commentaire} 3. Bb5 a6 4. Ba4 Nf6 5. 0-0 Be7 6. Re1 b5 7. Bb3 d6 8. c3 0-0 1-0");
ResultatChargementPgn chargementComplet = ChargementPartie.ChargerPartiePgn(pgnComplet, partieChargee);
Verifie("PGN complet : 16 demi-coups rejoués (variantes ignorées, roques 0-0), lecture seule",
    chargementComplet.CoupIllisible == null && chargementComplet.DemiCoupsJoues == 16 && L.ListeCoups.Count == 16 && partieChargee.Mode == ModePartie.LectureSeule && !partieChargee.RejeuPgn,
    $"{chargementComplet.DemiCoupsJoues} demi-coups, illisible : {chargementComplet.CoupIllisible ?? "aucun"}, mode {partieChargee.Mode}");
PartieEchecsPGN pgnIllegal = FichierPartiePgn.DecodePartiePGN("[Event \"x\"]\n\n1. e4 e5 2. Nf3 Nc6 3. Qh7 Nf6 0-1");
ResultatChargementPgn chargementIllegal = ChargementPartie.ChargerPartiePgn(pgnIllegal, partieChargee);
Verifie("PGN avec un coup illégal : arrêt avant lui (3. Qh7), 4 demi-coups chargés",
    chargementIllegal.CoupIllisible == "Qh7" && chargementIllegal.DemiCoupsJoues == 4 && L.ListeCoups.Count == 4 && partieChargee.Mode == ModePartie.LectureSeule,
    $"illisible : {chargementIllegal.CoupIllisible}, {chargementIllegal.DemiCoupsJoues} demi-coups");
PartieEchecsPGN pgnFen = FichierPartiePgn.DecodePartiePGN($"[Event \"x\"]\n[SetUp \"1\"]\n[FEN \"{fenNoirsAuTrait}\"]\n\n3... Nf6 4. Bc4 Bc5 5. c3 d6 *");
ResultatChargementPgn chargementFen = ChargementPartie.ChargerPartiePgn(pgnFen, partieChargee);
Verifie("PGN avec balise FEN : rejoué depuis la position (5 demi-coups), départ FEN",
    !chargementFen.FenIncomplete && chargementFen.CoupIllisible == null && chargementFen.DemiCoupsJoues == 5 && L.ListeCoups.Count == 6 && partieChargee.DepuisPosition,
    $"{chargementFen.DemiCoupsJoues} demi-coups, {L.ListeCoups.Count} éléments");
PartieEchecsPGN pgnFenIncomplete = FichierPartiePgn.DecodePartiePGN("[Event \"x\"]\n[FEN \"8/8/8\"]\n\n1. e4 e5 *");
ResultatChargementPgn chargementFenIncomplete = ChargementPartie.ChargerPartiePgn(pgnFenIncomplete, partieChargee);
Verifie("PGN avec balise FEN incomplète : signalée, coups joués depuis la position initiale",
    chargementFenIncomplete.FenIncomplete && chargementFenIncomplete.DemiCoupsJoues == 2 && !partieChargee.DepuisPosition, $"{chargementFenIncomplete.DemiCoupsJoues} demi-coups");

// Fichier PGN en Latin-1 (accents) avec une date incomplète "2024.??.??" et des annotations dans les coups
string fichierLatin1 = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "test_brunogui_latin1.pgn");
System.IO.File.WriteAllText(fichierLatin1,
    "[Event \"Test\"]\n[Date \"2024.??.??\"]\n[White \"Müller\"]\n[Black \"Gaël\"]\n[Result \"*\"]\n\n1. e4! e5?! 2. Nf3 *\n", System.Text.Encoding.Latin1);
PartieEchecsPGN partieLatin1 = FichierPartiePgn.DecodePartiePGN(FichierPartiePgn.DecodeFichierPGN(fichierLatin1)[0]);
System.IO.File.Delete(fichierLatin1);
Verifie("PGN en Latin-1 : accents lus, date '??' conservée, annotations retirées des coups",
    partieLatin1.White == "Müller" && partieLatin1.Black == "Gaël" && partieLatin1.Date == "2024.??.??" && partieLatin1.CoupsPartiePGN.StartsWith("1. e4 e5 2. Nf3"),
    $"{partieLatin1.White} / {partieLatin1.Black} / {partieLatin1.Date} / {partieLatin1.CoupsPartiePGN}");

// Choix du coup de bibliothèque : le meilleur poids (au hasard parmi les ex aequo), ou n'importe lequel en mode aléatoire
List<EntréePolyglot> entreesBiblio =
[
    new() { CoupBiblio = 1, Poids = 10 }, new() { CoupBiblio = 2, Poids = 50 }, new() { CoupBiblio = 3, Poids = 50 }, new() { CoupBiblio = 4, Poids = 5 }
];
Random hasardTest = new(1234);
bool toujoursMeilleur = Enumerable.Range(0, 200).Select(_ => PolyglotBibliothèque.ChoisirEntree(entreesBiblio, false, hasardTest)).All(e => e.Poids == 50);
var choixMeilleurs = Enumerable.Range(0, 200).Select(_ => PolyglotBibliothèque.ChoisirEntree(entreesBiblio, false, hasardTest).CoupBiblio).Distinct().Count();
var choixAleatoires = Enumerable.Range(0, 400).Select(_ => PolyglotBibliothèque.ChoisirEntree(entreesBiblio, true, hasardTest).CoupBiblio).Distinct().Count();
Verifie("Bibliothèque : meilleur poids (les deux ex aequo sortent), tous les coups en mode aléatoire, aucun coup si liste vide",
    toujoursMeilleur && choixMeilleurs == 2 && choixAleatoires == 4 && PolyglotBibliothèque.ChoisirEntree([], false, hasardTest) == null,
    $"meilleurs : {choixMeilleurs} coups distincts, aléatoire : {choixAleatoires}");

// Mise à jour de Stockfish : archive Windows adaptée au processeur (publication de Stockfish 19)
string[] archivesSf19 = ["stockfish-android-armv8.tar", "stockfish-ubuntu-x86-64-universal.tar", "stockfish-windows-arm64-universal.zip", "stockfish-windows-x86-64-universal.zip"];
string[] archivesAnciennes = ["stockfish-windows-x86-64-avx2.zip", "stockfish-windows-x86-64.zip", "stockfish-ubuntu-x86-64.tar"];
Verifie("Mise à jour Stockfish : x86-64 universal sur PC classique, arm64 sur PC ARM, rien sans version Windows",
    MiseAJourStockfish.ChoisirArchive(archivesSf19, System.Runtime.InteropServices.Architecture.X64) == "stockfish-windows-x86-64-universal.zip"
    && MiseAJourStockfish.ChoisirArchive(archivesSf19, System.Runtime.InteropServices.Architecture.Arm64) == "stockfish-windows-arm64-universal.zip"
    && MiseAJourStockfish.ChoisirArchive(archivesAnciennes, System.Runtime.InteropServices.Architecture.Arm64) != null
    && MiseAJourStockfish.ChoisirArchive(["stockfish-ubuntu-x86-64.tar"], System.Runtime.InteropServices.Architecture.X64) == null,
    MiseAJourStockfish.ChoisirArchive(archivesSf19, System.Runtime.InteropServices.Architecture.Arm64) ?? "(aucune)");

// Bibliothèque d'ouvertures introuvable : exception claire au chargement, puis aucun coup (et plus d'exception) à la recherche
bool introuvableSignalee = false;
try { new PolyglotBibliothèque().PolyglotBibliothèqueLecture("bibliotheque_introuvable.bin"); }
catch (System.IO.FileNotFoundException) { introuvableSignalee = true; }
Verifie("Bibliothèque introuvable : signalée au chargement, aucun coup et pas d'exception à la recherche",
    introuvableSignalee && !PolyglotBibliothèque.Disponible && !PolyglotBibliothèque.TrouverLesEntrées(0x463b96181691fc9c).Any(),
    $"signalée : {introuvableSignalee}, disponible : {PolyglotBibliothèque.Disponible}");

// ═══════════════ Pilotage du moteur (avec un faux moteur) ═══════════════
Console.WriteLine("── Pilote du moteur ──");

FauxMoteur faux = new();
PiloteMoteur pilote = new(faux);
Charger(L.FenDepart);
var resultatSansBiblio = pilote.DemanderCoup(L.FenDepart, 1000);
Verifie("Sans bibliothèque : la demande part au moteur",
    resultatSansBiblio == ResultatDemandeCoup.EnvoyeAuMoteur && faux.Recherches == 1 && faux.DerniereFen == L.FenDepart && pilote.Demande == TypeDemande.CoupDePartie,
    $"{resultatSansBiblio}, {faux.Recherches} recherche(s)");
faux.Repond();      // le bestmove arrive : le moteur ne réfléchit plus
Verifie("Réponse reçue : c'était un coup de partie, plus rien en cours",
    pilote.ReponseRecue() == TypeDemande.CoupDePartie && pilote.Demande == TypeDemande.Aucune, pilote.Demande.ToString());

pilote.ChoixBibliotheque = fen => "e2e4";
var resultatBiblio = pilote.DemanderCoup(L.FenDepart, 1000);
Verifie("Coup de bibliothèque : joué tout de suite, sans solliciter le moteur",
    resultatBiblio == ResultatDemandeCoup.CoupBibliotheque && faux.Recherches == 1 && L.ListeCoupsUci.LastOrDefault()?.Trim() == "e2e4" && pilote.Demande == TypeDemande.Aucune,
    $"{resultatBiblio}, dernier coup {L.ListeCoupsUci.LastOrDefault()}");
pilote.ChoixBibliotheque = fen => "e2e4";      // illégal : c'est aux Noirs, et e2 est vide
var resultatBiblioIllegal = pilote.DemanderCoup(L.RetourneChaineFenActuel(), 1000);
Verifie("Coup de bibliothèque illégal : la main passe au moteur",
    resultatBiblioIllegal == ResultatDemandeCoup.EnvoyeAuMoteur && faux.Recherches == 2 && L.ListeCoups.Count == 1, $"{resultatBiblioIllegal}, {L.ListeCoups.Count} coup(s)");

Position positionAnalysee = L.PositionDepuisFen(L.FenDepart);
pilote.DemanderAnalyse(positionAnalysee, 2000);
Verifie("Analyse : la demande précédente est abandonnée, la position analysée est une copie",
    faux.Abandons == 1 && pilote.AnalyseEnCours && pilote.PositionAnalysee != positionAnalysee && faux.DerniereFen == L.FenDepart && faux.DerniereDuree == 2000,
    $"{faux.Abandons} abandon(s), FEN envoyée {faux.DerniereFen}");
bool reflechissait = pilote.Abandonner();
Verifie("Abandon : signalé si le moteur réfléchissait, plus d'analyse en cours",
    reflechissait && !pilote.AnalyseEnCours && faux.Abandons == 2 && !pilote.Abandonner() && faux.Abandons == 2, $"{faux.Abandons} abandon(s)");

Charger("8/4P2k/8/8/8/8/8/4K3 w - - 0 1");
Verifie("Coup UCI avec promotion : la pièce demandée est posée (cavalier)",
    PiloteMoteur.JouerCoupUci("e7e8n") && L.PiecesEchiquier[L.RenvoieCaseIndex120("e8")] == L.TypePiece.CavalierBlanc,
    L.RetourneChaineFenActuel());
Verifie("Coup UCI illégal ou mal formé : refusé", !PiloteMoteur.JouerCoupUci("e1e5") && !PiloteMoteur.JouerCoupUci("e1") && !PiloteMoteur.JouerCoupUci(null), L.RetourneChaineFenActuel());

// ═══════════════ Evaluations et variantes du moteur ═══════════════
Console.WriteLine("── Analyse du moteur ──");

var evalNoirs = Evaluation.Depuis(LigneUci.Analyser("info depth 20 multipv 1 score cp -87 pv g4f3"), L.ColorPiece.Noir);
Verifie("Score converti du point de vue des Blancs (Noirs au trait, -0.87 pour eux)",
    evalNoirs is Evaluation e1 && e1.Texte == "0.87" && e1.Symbole == "±" && e1.Appreciation == "Avantage Blanc (±)",
    $"{evalNoirs?.Texte} {evalNoirs?.Symbole} {evalNoirs?.Appreciation}");
var matNoirsAuTrait = Evaluation.Depuis(LigneUci.Analyser("info score mate -3 pv h7h8"), L.ColorPiece.Noir);
var matBlancsAuTrait = Evaluation.Depuis(LigneUci.Analyser("info score mate -3 pv h7h8"), L.ColorPiece.Blanc);
Verifie("Mat : qui mate, du point de vue des Blancs (mate -3 = le camp au trait est maté)",
    matNoirsAuTrait?.Texte == "M3" && matNoirsAuTrait?.Symbole == "#+" && matNoirsAuTrait?.TexteMat == "MAT en 3 pour les Blancs"
    && matBlancsAuTrait?.Texte == "-M3" && matBlancsAuTrait?.Appreciation == "Gain Noir (mat)",
    $"{matNoirsAuTrait?.TexteMat} / {matBlancsAuTrait?.Appreciation}");
Verifie("Ligne sans score : pas d'évaluation", Evaluation.Depuis(LigneUci.Analyser("info depth 12 pv e2e4"), L.ColorPiece.Blanc) == null, "null");

SuiviAnalyse suiviAnalyse = new();
Position depart = L.PositionDepuisFen(L.FenDepart);
Verifie("Ligne sans score ni variante ignorée", suiviAnalyse.Ajouter(LigneUci.Analyser("info depth 12 nodes 1000"), depart) == null && suiviAnalyse.Meilleure == null, "null");
suiviAnalyse.Ajouter(LigneUci.Analyser("info depth 10 multipv 2 score cp 10 pv d2d4"), depart);
LigneAnalyse l1 = suiviAnalyse.Ajouter(LigneUci.Analyser("info depth 10 multipv 1 score cp 30 pv e2e4 e7e5 g1f3"), depart);
Verifie("Variante convertie en notation, début = 3 premiers éléments",
    l1.VariantePgn == "1. e4 e5 2. Cf3" && l1.Debut == "1. e4 e5" && suiviAnalyse.Meilleure == l1 && l1.TexteScore == "0.30", $"'{l1.VariantePgn}' / '{l1.Debut}'");
LigneAnalyse l2 = suiviAnalyse.Ajouter(LigneUci.Analyser("info depth 11 multipv 2 pv d2d4 d7d5"), depart);
Verifie("Ligne sans score : garde le score de SA variante (pas celui d'une autre)", l2.TexteScore == "0.10" && l2.VariantePgn == "1. d4 d5", $"{l2.TexteScore} '{l2.VariantePgn}'");
string quatorzeCoups = "g1f3 g8f6 f3g1 f6g8 g1f3 g8f6 f3g1 f6g8 g1f3 g8f6 f3g1 f6g8 g1f3 g8f6";
LigneAnalyse longue = suiviAnalyse.Ajouter(LigneUci.Analyser("info multipv 1 score cp 0 pv " + quatorzeCoups), depart);
Verifie($"Variante limitée à {SuiviAnalyse.CoupsAffiches} coups entiers",
    longue.VariantePgn.Split(' ').Count(c => !c.EndsWith('.')) == SuiviAnalyse.CoupsAffiches, longue.VariantePgn);
LigneAnalyse promo = suiviAnalyse.Ajouter(LigneUci.Analyser("info multipv 1 score cp 900 pv e7e8q h7g6"), L.PositionDepuisFen("8/4P2k/8/8/8/8/8/4K3 w - - 0 1"));
Verifie("Variante avec promotion : la pièce est conservée", promo.VariantePgn.StartsWith("1. e8=D"), promo.VariantePgn);
suiviAnalyse.Reinitialiser();
Verifie("Nouvelle demande : plus de variante mémorisée", suiviAnalyse.Meilleure == null, "null");

// ═══════════════ Pendule ═══════════════
Console.WriteLine("── Pendule ──");

Verifie("Cadence : nom, balise TimeControl et relecture",
    Cadence.Minutes(5, 3).Nom == "5 min + 3 s" && Cadence.Minutes(30).Nom == "30 min" && Cadence.SansPendule.Nom == "Sans pendule"
    && Cadence.Minutes(5, 3).TimeControl == "300+3" && Cadence.Minutes(30).TimeControl == "1800" && Cadence.SansPendule.TimeControl == "-"
    && Cadence.Lire("300+3") == Cadence.Minutes(5, 3) && Cadence.Lire("1800") == Cadence.Minutes(30),
    $"{Cadence.Minutes(5, 3).Nom} / {Cadence.Minutes(5, 3).TimeControl}");
Verifie("Cadence illisible : sans pendule",
    Cadence.Lire("-").EstSansPendule && Cadence.Lire("").EstSansPendule && Cadence.Lire(null).EstSansPendule
    && Cadence.Lire("abc").EstSansPendule && Cadence.Lire("300+x").EstSansPendule && Cadence.Lire("0").EstSansPendule, "");
Verifie("Cadences proposées : sans pendule en premier, toutes relues à l'identique",
    Cadence.Proposees[0].EstSansPendule && Cadence.Proposees.All(c => Cadence.Lire(c.TimeControl) == c),
    string.Join(", ", Cadence.Proposees.Select(c => c.TimeControl)));

TimeSpan horloge = TimeSpan.Zero;       // temps simulé : on l'avance à la main
TimeSpan S(double secondes) => TimeSpan.FromSeconds(secondes);
var pendule = new Pendule(Cadence.Minutes(5, 3), () => horloge);
Verifie("Pendule non démarrée : rien ne décompte",
    pendule.CampQuiDecompte == null && !pendule.Tourne && (horloge += S(10)) > S(0) && pendule.TempsRestant(L.ColorPiece.Blanc) == S(300), "");
pendule.Demarrer(L.ColorPiece.Blanc);
horloge += S(12);
Verifie("Seul le camp au trait décompte",
    pendule.TempsRestant(L.ColorPiece.Blanc) == S(288) && pendule.TempsRestant(L.ColorPiece.Noir) == S(300),
    $"{pendule.TempsRestant(L.ColorPiece.Blanc)} / {pendule.TempsRestant(L.ColorPiece.Noir)}");
bool coupAccepte = pendule.CoupJoue();
horloge += S(20);
Verifie("Coup joué : bonus ajouté, l'adversaire décompte",
    coupAccepte && pendule.CampQuiDecompte == L.ColorPiece.Noir && pendule.TempsRestant(L.ColorPiece.Blanc) == S(291) && pendule.TempsRestant(L.ColorPiece.Noir) == S(280),
    $"{pendule.TempsRestant(L.ColorPiece.Blanc)} / {pendule.TempsRestant(L.ColorPiece.Noir)}");
pendule.Pause();
horloge += S(100);
Verifie("Pause : le temps ne décompte plus", pendule.EnPause && pendule.TempsRestant(L.ColorPiece.Noir) == S(280), $"{pendule.TempsRestant(L.ColorPiece.Noir)}");
pendule.Reprendre();
horloge += S(5);
Verifie("Reprise : le décompte repart d'où il était", !pendule.EnPause && pendule.TempsRestant(L.ColorPiece.Noir) == S(275), $"{pendule.TempsRestant(L.ColorPiece.Noir)}");
horloge += S(300);
Verifie("Temps écoulé : le camp qui décompte est signalé, sans temps négatif",
    pendule.TempsEcoule() == L.ColorPiece.Noir && pendule.TempsRestant(L.ColorPiece.Noir) == TimeSpan.Zero, $"{pendule.TempsEcoule()}");
Verifie("Coup joué après la chute du drapeau : refusé, rien ne change",
    !pendule.CoupJoue() && pendule.CampQuiDecompte == L.ColorPiece.Noir && pendule.TempsRestant(L.ColorPiece.Blanc) == S(291), "");
pendule.Restaurer(S(291), S(280), L.ColorPiece.Noir);
horloge += S(1);
Verifie("Retour arrière : temps d'avant le coup, le camp au trait décompte",
    pendule.TempsRestant(L.ColorPiece.Noir) == S(279) && pendule.TempsRestant(L.ColorPiece.Blanc) == S(291) && pendule.TempsEcoule() == null, $"{pendule.TempsRestant(L.ColorPiece.Noir)}");
pendule.Arreter();
horloge += S(60);
Verifie("Pendule arrêtée (fin de partie) : temps figés",
    pendule.CampQuiDecompte == null && pendule.TempsRestant(L.ColorPiece.Noir) == S(279) && !pendule.CoupJoue(), "");
Verifie("Affichage du temps",
    Pendule.Texte(S(297)) == "4:57" && Pendule.Texte(S(3723)) == "1:02:03" && Pendule.Texte(S(9.47)) == "0:09.4"
    && Pendule.Texte(S(20)) == "0:20" && Pendule.Texte(S(-1)) == "0:00.0",
    $"{Pendule.Texte(S(297))} {Pendule.Texte(S(3723))} {Pendule.Texte(S(9.47))} {Pendule.Texte(S(20))}");

bool PeutMaterDans(string fen, L.ColorPiece camp) { Charger(fen); return L.PeutMater(camp); }
Verifie("Temps écoulé : l'adversaire a-t-il de quoi mater ?",
    !PeutMaterDans("4k3/8/8/8/8/8/8/4K3 w - - 0 1", L.ColorPiece.Blanc)
    && !PeutMaterDans("4k3/8/8/8/8/8/8/2B1K3 w - - 0 1", L.ColorPiece.Blanc)
    && !PeutMaterDans("4k3/8/8/8/8/8/8/1N2K3 w - - 0 1", L.ColorPiece.Blanc)
    && PeutMaterDans("4k3/8/8/8/8/8/8/1N2KN2 w - - 0 1", L.ColorPiece.Blanc)
    && PeutMaterDans("4k3/8/8/8/8/8/4P3/4K3 w - - 0 1", L.ColorPiece.Blanc)
    && PeutMaterDans("4k3/8/8/8/8/8/8/R3K3 w - - 0 1", L.ColorPiece.Blanc)
    && !PeutMaterDans("4k3/8/8/8/8/8/8/R3K3 w - - 0 1", L.ColorPiece.Noir), "roi seul, roi + fou, roi + cavalier : pas de mat");

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
                L.ExecutionCoup(source, destination, promotion);
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

// Faux moteur pour tester PiloteMoteur sans lancer de processus : il enregistre les demandes
class FauxMoteur : IMoteur
{
    public int Recherches, Abandons;
    public string DerniereFen;
    public int DerniereDuree;
    public bool EnReflexion { get; private set; }
    public void Chercher(string fen, int dureeMilliSecondes) { Recherches++; DerniereFen = fen; DerniereDuree = dureeMilliSecondes; EnReflexion = true; }
    public void Abandonner() { Abandons++; EnReflexion = false; }
    public void Repond() => EnReflexion = false;     // simule l'arrivée du bestmove
}
