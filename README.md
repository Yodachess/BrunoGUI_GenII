# BrunoGUI_GenII - Chess GUI in C# WinForms  
(Copyright (C) 2026 Bruno COURTOIS)

---

## 🇬🇧 English

**BrunoGUI_GenII** is a graphical chess interface allowing users to play, analyze, and replay games using **UCI-compatible engines**, including **Stockfish**.

The application is designed for both casual and advanced players, with configurable playing strength ranging from **1350 to 3000+ ELO**.

### Main features

#### 🎯 Play and analysis
- Human vs engine, engine vs human, or human vs human games
- Position analysis using a chess engine
- Adjustable ELO strength and thinking time
- Color selection (white/black)

#### ♟️ Chess engines
- Native integration of **Stockfish**
- Support for all **UCI-compatible engines**
- Simple and advanced engine configuration
- Included engines:
  - **Rodent IV**
  - **Sargon I (1978)**

#### 📚 PGN / FEN support
- Load **PGN** files
- Save games in **PGN** format
- Save positions in **FEN** format
- Edit PGN headers
- Navigate and analyze existing games

#### 📖 Opening libraries
- Support for **Polyglot (.bin)** opening books
- Default general opening book included
- Support for custom opening libraries

#### ⚙️ Customization
- Board color customization
- Clean interface focused on learning and analysis

---

## 🇫🇷 Français

**BrunoGUI_GenII** est une interface graphique d’échecs permettant de jouer, analyser et rejouer des parties à l’aide de moteurs compatibles **UCI**, dont **Stockfish**.

L’application s’adresse aussi bien aux joueurs débutants qu’aux joueurs avancés, avec un niveau de jeu configurable de **1350 à plus de 3000 ELO**.

### Fonctionnalités principales

#### 🎯 Jeu et analyse
- Parties humain vs moteur, moteur vs humain ou humain vs humain
- Analyse de positions avec moteur d’échecs
- Réglage du niveau ELO et du temps de réflexion
- Choix de la couleur (blanc/noir)

#### ♟️ Moteurs d’échecs
- Intégration native de **Stockfish**
- Support des moteurs compatibles **UCI**
- Configuration simple ou avancée des moteurs
- Moteurs inclus :
  - **Rodent IV**
  - **Sargon I (1978)**

#### 📚 PGN / FEN
- Chargement de fichiers **PGN**
- Sauvegarde des parties au format **PGN**
- Sauvegarde des positions au format **FEN**
- Édition des en-têtes PGN
- Navigation et analyse de parties existantes

#### 📖 Bibliothèques d’ouvertures
- Support des bibliothèques **Polyglot (.bin)**
- Bibliothèque générale incluse
- Possibilité d’utiliser des bibliothèques personnalisées

#### ⚙️ Personnalisation
- Personnalisation des couleurs de l’échiquier
- Interface claire orientée apprentissage et analyse

---

## 🧩 Prérequis / Requirements

- Windows 10 ou 11 (x64)
- Pour utiliser l’application / to run : [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- Pour compiler / to build : [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (ou Visual Studio 2022)
- Moteurs d’échecs compatibles UCI / UCI chess engines (Stockfish est fourni et mis à jour automatiquement au démarrage / Stockfish is included and updated automatically at startup)

---

## 🛠️ Compilation et tests / Build and tests

```bash
dotnet build BrunoGUI_GenII.sln
dotnet run --project BrunoGUI_GenII.csproj
```

Tests de la logique d’échecs (règles, notation PGN, protocole UCI, perft) / Chess logic tests (rules, PGN notation, UCI protocol, perft):

```bash
dotnet run --project Tests
dotnet run --project Tests -- --complet
```

`--complet` ajoute les tests perft profonds (environ 20 s) / adds the deep perft tests (about 20 s).

Les paramètres (moteur, couleurs de l’échiquier, force, temps de réflexion, nombre de variantes…) se règlent dans `BrunoGUI.ini`.
Settings (engine, board colours, strength, thinking time, number of lines…) are in `BrunoGUI.ini`.

---

## 👤 Auteur

Projet développé par **Bruno COURTOIS**  
Conçu pour l’étude, l’analyse et le plaisir du jeu d’échecs.

---

## 📜 Licence

SPDX-License-Identifier: GPL-3.0-or-later
