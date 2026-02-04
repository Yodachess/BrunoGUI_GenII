# BrunoGUI_GenII

Bienvenue dans l’aide de **BrunoGUI_GenII**.

Ce programme permet de jouer aux échecs contre un moteur dont le niveau va de **1350 à plus de 3000 ELO**.

---

## Utilisation

### 1. Fichier
- **Ouvrir fichier PGN** : charge des parties au format PGN pour les parcourir et/ou analyser les positions avec le moteur choisi.  
  **[Affiche liste parties]** ouvre le tableau des parties contenues dans le fichier pour la sélection.
- **Enregistrer partie** : sauvegarde votre partie au format PGN et/ou l’ajoute à un fichier existant.
- **Enregistrer position** : sauvegarde la position au format **FEN**.

---

### 2. Stockfish
**Stockfish** est un moteur d’échecs libre et open source.  
Multiple vainqueur de la ligue **TCEC**, il est considéré comme **le meilleur moteur d’échecs au monde**.

Vous pouvez adapter :
- le niveau de **1350 à 3150 ELO**
- le **temps de réflexion**

Fonctions :
- **Nouvelle partie contre Stockfish** : choix de la couleur, ELO, adversaire, temps de réflexion
- **Paramètres de base** : réglages principaux (optionnels)
- **Paramètres avancés** : réglages fins du moteur

---

### 3. Nouvelle Partie
- **Humain contre ordinateur** : vous avez les Blancs, le moteur joue les Noirs
- **Ordinateur contre humain** : vous avez les Noirs, le moteur joue les Blancs
- **Entre humains (ou saisie)** : le moteur ne joue pas  
  → possibilité d’utiliser **[Analyse Position]**

Lors de la saisie d’une partie :
- affichage des coups avec **[Partie PGN]**
- édition des informations avec **[En-Tête PGN]**

---

### 4. Moteurs / Bibliothèques
- **Sélectionnez un moteur** : permet de jouer contre d’autres moteurs compatibles **UCI**
- **Sélectionnez une bibliothèque** : bibliothèques d’ouvertures au format **Polyglot (*.bin)**  
  La bibliothèque par défaut est généraliste mais complète.

Moteurs fournis :
- **Rodent IV** : moteur original, fort, équilibré et polyvalent
- **Sargon I (1978)** : le tout premier moteur sur micro-ordinateur 😄

---

### 5. Options
- **Personnaliser** : changer la couleur des cases de l’échiquier
- **Partie PGN** : visualisation de la partie (identique à *Partie PGN*)
- **Saisie en-têtes PGN** : édition des en-têtes (identique à *En-Tête PGN*)
- **Aide / Documentation** : ouvre ce fichier

---

### 6. À propos
Version du programme et nom de l’auteur…  
**Votre serviteur Bruno Courtois** 😉
