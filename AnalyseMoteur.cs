// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Evaluations et variantes du moteur, sans interface graphique (testé dans Tests/Program.cs)
//  ├─ Structure "Evaluation" : score du point de vue des Blancs, et ses textes (score, symbole, appréciation)
//  ├─ Classe "LigneAnalyse" : une variante du moteur (numéro, évaluation, coups en notation)
//  └─ Classe "SuiviAnalyse" : dernière ligne reçue pour chaque variante de la demande en cours
//              ├─ "Ajouter"            décode une ligne "info" (score et variante) sur la position analysée
//              ├─ "Meilleure"          variante n° 1 (celle du coup du moteur, ou le résultat de l'analyse)
//              └─ "Reinitialiser"      nouvelle demande au moteur
// Le moteur donne ses scores du point de vue du camp au trait ; tout est converti ici, une fois pour toutes,
// du point de vue des Blancs (+ = avantage blanc, "M3" = les Blancs matent en 3, "-M3" = les Noirs matent en 3).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public readonly record struct Evaluation(int? Centipions, int? MatEn)     // du point de vue des Blancs
    {
        public static Evaluation? Depuis(LigneUci ligne, ColorPiece campAuTrait)
        {   // Score d'une ligne "info" (point de vue du camp au trait) converti du point de vue des Blancs ; null si la ligne n'a pas de score
            int sens = campAuTrait == ColorPiece.Blanc ? 1 : -1;
            if (ligne.MatEn is int mat)
                return new Evaluation(null, sens * mat);
            if (ligne.ScoreCentipions is int cp)
                return new Evaluation(sens * cp, null);
            return null;
        }
        public bool EstUnMat => MatEn.HasValue;
        public string Texte => MatEn is int mat
            ? (mat < 0 ? "-M" : "M") + Math.Abs(mat)
            : ((Centipions ?? 0) / 100m).ToString("N2", CultureInfo.InvariantCulture);
        public string Symbole => MatEn is int mat
            ? (mat < 0 ? "#-" : "#+")
            : (Centipions ?? 0) switch
            {
                >= 250 => "+-",
                > 50 => "±",
                <= -250 => "-+",
                < -50 => "∓",
                _ => "="
            };
        public string Appreciation => MatEn is int mat
            ? (mat < 0 ? "Gain Noir (mat)" : "Gain Blanc (mat)")
            : (Centipions ?? 0) switch
            {
                >= 250 => "Gain Blanc (+-)",
                > 50 => "Avantage Blanc (±)",
                <= -250 => "Gain Noir (-+)",
                < -50 => "Avantage Noir (∓)",
                _ => "Égal (=)"
            };
        public string TexteMat => MatEn is int mat ? $"MAT en {Math.Abs(mat)} pour les {(mat < 0 ? "Noirs" : "Blancs")}" : null;
    }

    public class LigneAnalyse
    {
        public int Numero { get; init; }                // numéro de variante (1 = la meilleure ; 1 aussi pour un moteur sans MultiPV)
        public Evaluation? Evaluation { get; init; }    // null si le moteur n'a pas encore donné de score pour cette variante
        public string VariantePgn { get; init; }        // coups en notation (limités à CoupsAffiches), null si la ligne n'a pas de variante
        public string Debut => VariantePgn == null ? "" : string.Join(" ", VariantePgn.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3));
        public string Symbole => Evaluation?.Symbole ?? "?";
        public string TexteScore => Evaluation?.Texte ?? "?";
    }

    public class SuiviAnalyse
    {
        public const int CoupsAffiches = 12;    // nombre de coups de variante convertis et affichés (coups entiers, promotion comprise)
        private readonly Dictionary<int, LigneAnalyse> _lignes = [];

        public void Reinitialiser() => _lignes.Clear();
        public LigneAnalyse Meilleure => _lignes.GetValueOrDefault(1);

        public LigneAnalyse Ajouter(LigneUci ligne, Position position)
        {   // Décode une ligne "info" sur la position analysée ; null si elle n'a ni score ni variante (ex : "info depth 12")
            int numero = ligne.NumeroVariante ?? 1;
            Evaluation? evaluation = Evaluation.Depuis(ligne, position.QuiJoue);
            if (evaluation == null && ligne.Variante == null)
                return null;
            _lignes.TryGetValue(numero, out LigneAnalyse precedente);
            string variantePgn = precedente?.VariantePgn;
            if (ligne.Variante != null)
            {   // Coups entiers seulement (un coup coupé perdrait sa promotion), convertis en notation sur une copie de la position
                string coups = string.Join(" ", ligne.Variante.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(CoupsAffiches));
                variantePgn = Outils.VarianteUciVersPgn(coups, DemiCoupAvant(position), false, position).Trim();
            }
            LigneAnalyse nouvelle = new()
            {
                Numero = numero,
                Evaluation = evaluation ?? precedente?.Evaluation,     // une ligne sans score garde le score de SA variante
                VariantePgn = variantePgn
            };
            _lignes[numero] = nouvelle;
            return nouvelle;
        }
    }
}
