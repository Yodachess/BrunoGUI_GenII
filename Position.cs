// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// État complet d'une position d'échecs (tout ce que contient un FEN, plus l'état d'échec)
// └─ Classe "Position"
//              ├─ "Position"   (Init : échiquier 120 cases, bordures et 64 cases vides)
//              └─ "Copier"     Copie indépendante, pour les calculs "pour voir" (voir LogiqueMouvements.CalculerSurCopie)

using System.Collections.Generic;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public class Position
    {
        public List<TypePiece> Pieces { get; private set; }     // l'échiquier 120 cases (voir le schéma dans LogiqueMouvements)
        public ColorPiece QuiJoue { get; set; }                 // champ 2 du FEN : couleur qui a le trait
        public bool PetitRoqueBlancPossible { get; set; }       // champ 3 du FEN : droits de roque (K, Q, k, q)
        public bool GrandRoqueBlancPossible { get; set; }
        public bool PetitRoqueNoirPossible { get; set; }
        public bool GrandRoqueNoirPossible { get; set; }
        public int IndexCaseEnPassant { get; set; }             // champ 4 du FEN : index 120 de la case en passant, 0 si aucune
        public int SansPrise { get; set; }                      // champ 5 du FEN : demi-coups depuis la dernière prise ou le dernier coup de pion
        public float NombreCoupsJoues { get; set; }             // champ 6 du FEN : avance de 0.5 en 0.5, la partie entière est le numéro du coup
        public bool Echec { get; set; }                         // le roi est en échec (calculé, absent du FEN)
        public bool EchecetMat { get; set; }                    // le roi est échec et mat (calculé, absent du FEN)

        public Position()
        {   // Echiquier vide : bordures tout autour et 64 cases vides au centre
            Pieces = new List<TypePiece>(120);
            for (int i = 0; i < 120; i++)
            {
                int ligne = i / 10, colonne = i % 10;
                bool caseReelle = ligne >= 2 && ligne <= 9 && colonne >= 1 && colonne <= 8;
                Pieces.Add(caseReelle ? TypePiece.Vide : TypePiece.Bordure);
            }
        }

        public Position Copier()
        {   // Copie indépendante : modifier la copie ne change pas l'original
            Position copie = (Position)MemberwiseClone();
            copie.Pieces = new List<TypePiece>(Pieces);
            return copie;
        }
    }
}
