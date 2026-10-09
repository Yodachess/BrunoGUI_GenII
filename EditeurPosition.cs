// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Saisie d'une position à la main, sans interface graphique (testée dans Tests/Program.cs ; la fenêtre est SaisiePosition.cs)
// └─ Classe "EditeurPosition" : la position en cours de saisie (un objet Position), et sa FEN
//              ├─ "Vider", "PositionInitiale", "ChargerFen"
//              ├─ "Poser"                      une pièce (ou Vide) sur une case ; un seul roi par camp
//              ├─ "Trait", "NumeroCoup"        champs 2 et 6 de la FEN
//              ├─ "Roque" / "RoquePossible"    droits de roque, possibles seulement si le roi et la tour sont sur leurs cases
//              ├─ "EnPassant", "CasesEnPassantPossibles"
//              └─ "Fen", "Erreur"              la FEN, et la raison pour laquelle elle serait refusée (ChargementPartie.ErreurFen)

using System;
using System.Collections.Generic;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

#nullable enable

namespace BrunoGUI_GenII
{
    public enum DroitRoque { PetitBlanc, GrandBlanc, PetitNoir, GrandNoir }

    public class EditeurPosition
    {
        public Position Position { get; private set; } = new();

        public EditeurPosition(string? fen = null)
        {
            if (fen == null || ChargerFen(fen) != null)
                PositionInitiale();
        }

        public void Vider()
        {   // Échiquier vide, trait aux Blancs, coup n° 1 (pour poser les pièces une à une)
            Position = new Position { QuiJoue = ColorPiece.Blanc, NombreCoupsJoues = 1 };
            _roquesPossiblesAvant = RoquesPossibles();
        }

        public void PositionInitiale()
        {
            Position = PositionDepuisFen(FenDepart);
            _roquesPossiblesAvant = RoquesPossibles();
        }

        public string? ChargerFen(string fen)
        {   // La position d'une FEN (null), ou la raison de son refus (rien n'est changé)
            string normalisee = ChargementPartie.NormaliseFen(fen);
            string? erreur = ChargementPartie.ErreurFen(normalisee);
            if (erreur != null)
                return erreur;
            Position = PositionDepuisFen(normalisee);
            _roquesPossiblesAvant = RoquesPossibles();
            return null;
        }

        public TypePiece Piece(int index) => Position.Pieces[index];

        public void Poser(int index, TypePiece piece)
        {   // Pose une pièce (Vide : vide la case). Un roi posé remplace l'autre roi du même camp, s'il y en avait un
            if (Position.Pieces[index] == TypePiece.Bordure)
                return;
            if (piece is TypePiece.RoiBlanc or TypePiece.RoiNoir)
                for (int i = 21; i <= 98; i++)
                    if (Position.Pieces[i] == piece)
                        Position.Pieces[i] = TypePiece.Vide;
            Position.Pieces[index] = piece;
            MetAJourDroits();
        }

        public ColorPiece Trait
        {
            get => Position.QuiJoue;
            set
            {   // Le numéro du coup ne change pas (NombreCoupsJoues vaut n pour les Blancs, n + 0,5 pour les Noirs)
                int numero = NumeroCoup;
                Position.QuiJoue = value;
                NumeroCoup = numero;
                MetAJourDroits();
            }
        }

        public int NumeroCoup
        {
            get => (int)Math.Truncate(Position.NombreCoupsJoues);
            set => Position.NombreCoupsJoues = Math.Max(1, value) + (Position.QuiJoue == ColorPiece.Noir ? 0.5f : 0f);
        }

        // ═══ Droits de roque ═══
        public bool RoquePossible(DroitRoque roque) => roque switch
        {   // Le roi et la tour de ce roque sur leurs cases d'origine (e1/h1, e1/a1, e8/h8, e8/a8)
            DroitRoque.PetitBlanc => Piece(25) == TypePiece.RoiBlanc && Piece(28) == TypePiece.TourBlanche,
            DroitRoque.GrandBlanc => Piece(25) == TypePiece.RoiBlanc && Piece(21) == TypePiece.TourBlanche,
            DroitRoque.PetitNoir => Piece(95) == TypePiece.RoiNoir && Piece(98) == TypePiece.TourNoire,
            _ => Piece(95) == TypePiece.RoiNoir && Piece(91) == TypePiece.TourNoire
        };

        public bool Roque(DroitRoque roque) => roque switch
        {
            DroitRoque.PetitBlanc => Position.PetitRoqueBlancPossible,
            DroitRoque.GrandBlanc => Position.GrandRoqueBlancPossible,
            DroitRoque.PetitNoir => Position.PetitRoqueNoirPossible,
            _ => Position.GrandRoqueNoirPossible
        };

        public void DefinitRoque(DroitRoque roque, bool permis)
        {   // (refusé s'il n'est pas possible : le roi ou la tour n'est pas sur sa case)
            permis &= RoquePossible(roque);
            switch (roque)
            {
                case DroitRoque.PetitBlanc: Position.PetitRoqueBlancPossible = permis; break;
                case DroitRoque.GrandBlanc: Position.GrandRoqueBlancPossible = permis; break;
                case DroitRoque.PetitNoir: Position.PetitRoqueNoirPossible = permis; break;
                default: Position.GrandRoqueNoirPossible = permis; break;
            }
        }

        private bool[] _roquesPossiblesAvant = new bool[4];
        private bool[] RoquesPossibles() => [.. Enum.GetValues<DroitRoque>().Select(RoquePossible)];

        private void MetAJourDroits()
        {   // Après une pièce posée ou un changement de trait : un roque devenu impossible est retiré, un roque qui vient de devenir
            // possible (roi et tour remis en place) est accordé d'office ; une case en passant qui n'a plus de sens est retirée
            bool[] possibles = RoquesPossibles();
            foreach (DroitRoque roque in Enum.GetValues<DroitRoque>())
                DefinitRoque(roque, possibles[(int)roque] && (Roque(roque) || !_roquesPossiblesAvant[(int)roque]));
            _roquesPossiblesAvant = possibles;
            if (Position.IndexCaseEnPassant != 0 && !CasesEnPassantPossibles().Contains(NomCaseAlgebrique(Position.IndexCaseEnPassant)))
                Position.IndexCaseEnPassant = 0;
        }

        // ═══ Prise en passant ═══
        public List<string> CasesEnPassantPossibles()
        {   // Cases où le camp au trait pourrait prendre en passant : l'adversaire vient d'avancer un pion de deux cases, donc ce pion
            // est sur sa 4e rangée, et la case qu'il a sautée et sa case de départ sont vides (Blancs au trait : x6, pion noir en x5)
            List<string> cases = [];
            bool blancs = Trait == ColorPiece.Blanc;
            for (int colonne = 1; colonne <= 8; colonne++)
            {
                int saut = (blancs ? 70 : 40) + colonne, pion = (blancs ? 60 : 50) + colonne, depart = (blancs ? 80 : 30) + colonne;
                if (Piece(pion) == (blancs ? TypePiece.PionNoir : TypePiece.PionBlanc) && Piece(saut) == TypePiece.Vide && Piece(depart) == TypePiece.Vide)
                    cases.Add(NomCaseAlgebrique(saut));
            }
            return cases;
        }

        public string EnPassant
        {   // "-" ou la case ("e6")
            get => Position.IndexCaseEnPassant != 0 ? NomCaseAlgebrique(Position.IndexCaseEnPassant) : "-";
            set => Position.IndexCaseEnPassant = CasesEnPassantPossibles().Contains(value) ? RenvoieCaseIndex120(value) : 0;
        }

        // ═══ Résultat ═══
        public string Fen => CalculerSur(Position, RetourneChaineFenActuel);
        public string? Erreur => ChargementPartie.ErreurFen(Fen);   // null : la position peut être jouée
    }
}
