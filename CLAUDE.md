# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projet

BrunoGUI_GenII : interface graphique d'échecs Windows (C# WinForms, .NET 8, `net8.0-windows`) qui pilote des moteurs UCI (Stockfish par défaut, plus Rodent IV, Sargon 1978, etc.). Licence GPL-3.0-or-later. Tout le code, les identifiants et les commentaires sont en français — garder cette convention.

## Commandes

```bash
dotnet build BrunoGUI_GenII.sln
```

```bash
dotnet run --project BrunoGUI_GenII.csproj
```

Tests de la logique d'échecs (projet console sans framework externe, `Tests/Program.cs`) :

```bash
dotnet run --project Tests
```

```bash
dotnet run --project Tests -- --complet
```

- Les tests couvrent les règles (roque, prise en passant, promotion, 50 coups, lecture FEN, conversion de variantes UCI), la notation PGN des coups ambigus (avec aller-retour écriture/relecture), la liste des coups (alignement, départ FEN, retour arrière, « # » du mat), le décodage des lignes UCI (`LigneUci`), la classe `Position` (copie, `CalculerSurCopie`) et des **perft** (chaque coup y est joué via `CalculerSurCopie`) comparés aux valeurs de référence (position initiale, Kiwipete, positions 3 à 5). `--complet` ajoute les perft profonds (~15 s). Toute modification du générateur de coups doit garder les perft verts.
- Pour ajouter un test : appeler `Charger(fen)`, jouer avec `L.ExecutionCoup(source, destination)`, puis `Verifie(nom, condition, détail)`.
- L'interface graphique n'a pas de tests : vérifier à la main dans l'application tout ce qui touche à `EchiquierPrincipal` ou au moteur.
- La solution compile sans avertissement : garder cet état.
- `CA1416` est désactivé dans le `.csproj`.
- `git` n'est pas dans le PATH de PowerShell sur cette machine : utiliser celui de Visual Studio 2022 (`C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe`).
- GitHub Actions (`.github/workflows/tests.yml`) compile et lance les tests à chaque push. La compilation y passe `-p:SignManifests=false` : le certificat ClickOnce n'existe que sur le PC de développement.
- `.gitattributes` impose CRLF aux fichiers du projet (`.cs`, `.csproj`, `.sln`, `.resx`, `.md`, `.ini`, `.yml`) : écrire les nouveaux fichiers en CRLF.

## Architecture

**Point d'entrée** : `Program.cs` affiche le splash (`EcranDemarrage`), puis lance `EchiquierPrincipal` (palette par défaut de Krypton 95, pas de `KryptonManager`).

**`EchiquierPrincipal`** (~130 Ko) est le formulaire central : il gère l'échiquier, les menus, la feuille de partie, l'analyse et l'orchestration du moteur. Les autres formulaires (`ParametresDeBase`, `ParametresUciStockfish`, `PartieForceModule`, `FenetrePartie`, `AffichePgn`, `FichierPartiePgn`, `DonneesBrutesUci`, `FenetreAide`) sont des boîtes de dialogue secondaires.

**Classe `Position` + façade statique + événements statiques** — c'est le modèle clé à comprendre :
- `Position` (`Position.cs`) regroupe tout l'état d'une position : plateau 120 cases, trait, 4 droits de roque, case en passant, compteur des 50 coups, numéro de coup, `Echec`/`EchecetMat`. `Copier()` en fait une copie indépendante.
- `LogiqueMouvements.PositionActuelle` est la position de la partie. Les propriétés statiques historiques (`PiecesEchiquier`, `QuiJoue`, `PetitRoqueBlancPossible`…, `IndexCaseEnPassant`, `SansPrise`, `NombreCoupsJoues`, `Echec`, `EchecetMat`) ne sont que des raccourcis vers cet objet : le reste du code les utilise sans connaître `Position`. Tout nouvel élément d'état de la position doit être ajouté à `Position` (sinon il ne serait ni copié ni restauré).
- Restent hors de `Position` (état de partie ou de coup, non copié) : la liste des coups (`ListeCoups`), `Pat`, `PartieEnCoursMat/Pat`, `PromotionPiece`, `CoupValide`.
- `LogiqueMouvements` notifie l'UI via des événements statiques (`DessinePiece`, `DessineSymbole`, `AfficheCoupBlanc/Noir`, etc.) abonnés dans `EchiquierPrincipal.BrunoInterfaceGraphique_Load`.
- Les événements statiques sont invoqués avec `?.Invoke(...)` : la logique fonctionne sans interface abonnée (tests). Garder cette forme pour tout nouvel appel d'événement.
- `MoteurUci` lance le moteur comme `Process` avec stdin/stdout redirigés. `ProcOutputDataReceived` s'exécute sur un **thread non-UI** : il décode chaque ligne une seule fois avec `LigneUci.Analyser` (résultat dans `MoteurUci.DerniereLigne`), puis déclenche `AfficheUci`, `AfficheDonneesBrutes`, `AfficheCoupMoteur` ; les gestionnaires côté formulaire doivent passer par `InvokeRequired`/`Invoke` et lire `DerniereLigne` plutôt que redécouper le texte. Les scores UCI sont du point de vue du camp au trait (mat négatif = le camp au trait est maté). Les réponses `bestmove (none)` / `0000` sont traitées comme mat ou pat. Le nombre de variantes est centralisé dans `MoteurUci.NombreLignesPV` (initialisé depuis le `.ini`, modifié via `DefinitMultiPV`) : ne pas envoyer `MultiPV` en dur.
- Réinitialiser une partie passe par `Outils.MiseaZeroListes()`.

**Calculs « pour voir »** : la légalité (`TestMouvementValide`), l'échec après un coup (`CoupNotationAlgebriquePGN`, qui met à jour `Echec` de la position réelle avec le résultat), le mat (`CalculeEchecEtMat`) et les variantes du moteur (`Outils.VarianteUciVersPgn`) s'exécutent dans `LogiqueMouvements.CalculerSurCopie(() => …)`. Cette fonction remplace temporairement `PositionActuelle` par une copie, puis remet l'originale, même en cas d'exception. Dans ce cadre, jouer les coups avec `SimuleCoup` (qui gère la tour du roque et le pion pris en passant, sans affichage), jamais `DeplacementPiece` seul. Tout ce qui est hors de `Position` (listes de coups, `PromotionPiece`, événements d'affichage) n'est **pas** protégé par la copie. Un calcul sur copie ne déclenche aucun redessin, contrairement à `MiseenplaceFen`.

**Droits de roque** : les 4 booléens `PetitRoqueBlancPossible`, `GrandRoqueBlancPossible`, `PetitRoqueNoirPossible`, `GrandRoqueNoirPossible` sont la seule source de vérité (lus/écrits dans la FEN, mis à jour dans `FaireMouvement`). `MiseenplaceFen` rétablit aussi la case en passant et le compteur des 50 coups : le retour arrière et la navigation dans la partie reposent dessus.

**Représentation de l'échiquier** : tableau « mailbox » de **120 cases** (10×12). a1 = index 21, h8 = index 98 ; les cases hors plateau valent `TypePiece.Bordure`. Les `PictureBox` de l'UI (`PictJeux`) sont indexées de la même façon. Voir le schéma en tête de `LogiqueMouvements.cs`. Dans `TypePiece`, les pièces noires ont des valeurs paires et les blanches impaires.

**Liste des coups** (`Coup.cs`) : `LogiqueMouvements.ListeCoups` est une liste d'objets `Coup` qui portent toutes les notations d'un coup (FEN de la position **après** le coup, PGN international, PGN français R/D/T/F/C, NAL, UCI ; le numéro « 12. » n'est écrit que devant les coups blancs, puis « # », « + » ou rien, puis un espace). `ListeCoupsFen`, `ListeCoupsPgnIntl`, `ListeCoupsPgnFr`, `ListeCoupsNal`, `ListeCoupsUci` sont des **vues en lecture seule** (`VueCoups`) de cette liste, toujours alignées : on ne la modifie que par `AjouteCoup`, `AjoutePositionDeDepart`, `RetireDernierCoup` (qui ne retire jamais la position de départ) et `ViderCoups`. Une partie chargée depuis un FEN commence par un élément `Coup.PositionDeDepart` (notations vides) : le code qui parcourt les coups joués doit l'ignorer (`EstPositionDeDepart`). Le « # » du mat est posé par `ExecutionCoup`, qui ne positionne toutefois `EchecetMat` qu'après les événements `AfficheCoupBlanc`/`AfficheCoupNoir` (l'interface en dépend).

**Autres modules** :
- `Bibliotheque.cs` : lecture des livres d'ouvertures Polyglot `.bin` (calcul de clé Zobrist, table Random64, lecture big-endian).
- `GestionPartiePgn.cs` / `FichierPartiePgn.cs` : génération/sauvegarde PGN (lignes ≤ 80 caractères), en-têtes (`SaisieBalises`), et parseur PGN (`ParseurPgn`, machine à états).
- `MiseAJourStockfish.cs` : lancé automatiquement au démarrage (avant `MoteurUci.Start`) et par le bouton de mise à jour. Compare `sf_<n>` (tiré de `id name Stockfish <n>`) au tag de la dernière release GitHub, télécharge le zip Windows adapté (depuis Stockfish 19, un seul binaire `x86-64-universal`) et remplace le `stockfish.exe` **du dossier de sortie** (sauvegarde `.old`, arrêt préalable des seuls processus lancés depuis ce fichier). `ExecuterMiseAJour` retourne `false` si Stockfish est déjà à jour ; une exception signale une vraie erreur. Le `.csproj` copie `stockfish\stockfish.exe` en `PreserveNewest` pour ne pas écraser une version mise à jour à chaque compilation.

## Chemins et fichiers d'exécution

- `Chemins.RepertoireRacine` = `Application.StartupPath` (déploiement portable) : moteurs, livres et `.ini` sont cherchés **à côté de l'exécutable**, pas à la racine du dépôt. Un fichier ajouté dans `Moteurs_UCI\` ou `BibliothèquesPolyglot\` doit donc être déclaré dans le `.csproj` avec `CopyToOutputDirectory` (seuls Rodent IV, Sargon et `stockfish\stockfish.exe` le sont côté moteurs).
- `BrunoGUI.ini` (format `clé = valeur`, `;` pour les commentaires) est lu par `Parametres.ChargerDepuisIni`. Seules les clés présentes dans le `switch` sont lues ; une nouvelle clé doit y être ajoutée. Les couleurs acceptent un nom .NET (`Peru`) ou un code `#RRGGBB` (`Parametres.ConvertitCouleur`), avec le style Lichess par défaut. `NombreCoeursThread` et `TableHachage` sont envoyés au moteur à la réception de `uciok` (`MoteurUci.EnvoieOptionsDemarrage`), une seule fois et seulement si le moteur déclare ces options. À la fermeture, `EchiquierPrincipal.SauvePreferences` enregistre les réglages de l'interface (couleurs, temps de réflexion, choix de la fenêtre « Nouvelle partie », MultiPV, Threads, Hash, bibliothèque) via `Parametres.SauverDansIni`, qui remplace les valeurs sur place en conservant commentaires, ordre et clés inconnues. Le `.csproj` copie le `.ini` en `PreserveNewest` pour ne pas écraser ces préférences à chaque compilation. Les réglages Threads/Hash passent par `MoteurUci.DefinitThreads`/`DefinitHachage` (mémorisés et renvoyés à chaque redémarrage du moteur, par exemple à chaque « Nouvelle partie »).
- `BrunoGUI.ini`, `Bruno_NB.jpg` et `AideBrunoGUI.rtf` sont cherchés via `Chemins.RepertoireRacine` : ne jamais utiliser de chemin relatif au dossier courant (l'application est parfois lancée depuis un raccourci).
- Le dossier `Tests\` est exclu de la compilation de l'application dans `BrunoGUI_GenII.csproj` (le projet racine inclut sinon tous les `.cs` des sous-dossiers).
- `stockfish\src\` et `stockfish\wiki\` sont les sources/doc upstream de Stockfish, fournies pour référence — elles ne font pas partie du build C#.

## UI

Composants Krypton 95 uniquement (`Krypton.Toolkit`, `Krypton.Navigator`, `Krypton.Ribbon`, `Krypton.Workspace`, espace de noms `Krypton.Toolkit`) ; l'ancienne bibliothèque `ComponentFactory.Krypton.Toolkit` a été retirée. `KryptonMessageBox.Show` prend `KryptonMessageBoxButtons` / `KryptonMessageBoxIcon` (pas les énumérations Windows Forms). Les fichiers `*.Designer.cs` sont générés par le designer WinForms : les modifier avec précaution (ou via le designer), en cohérence avec les `.resx`.
