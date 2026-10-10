// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Langue de l'interface (français ou anglais) ...
//      └─ Classe statique "Langue" (sans Windows Forms : utilisable par la logique et les tests)
//                      ├─ "Choisir"    Langue choisie au démarrage (préférence "Langue", sinon celle de Windows)
//                      ├─ "T"          Traduction d'une phrase écrite en français dans le code
//                      └─ "Notation"   Coups en notation française ("Cf3", "Dd8-d7") avec les lettres anglaises (Nf3, Qd8-d7)
// Les phrases restent écrites en français dans le code : T("Aucun coup à analyser.") ; la traduction anglaise est dans
// Langues\en.json (à côté de l'exécutable), sous la forme { "phrase française": "English sentence" }. Une phrase absente du
// fichier reste en français (un test vérifie qu'aucune phrase passée à T n'y manque). Ajouter une langue : un fichier
// Langues\<code>.json de plus. La langue ne change qu'au démarrage (menu Options > Langue, puis redémarrage).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BrunoGUI_GenII
{
    public static class Langue
    {
        public const string Francais = "fr", Anglais = "en";
        public static string Code { get; private set; } = Francais;
        public static bool EstFrancais => Code == Francais;
        private static Dictionary<string, string> _traductions = [];

        // Langue par défaut, sans préférence enregistrée : le français si Windows est en français, sinon l'anglais
        public static string CodeDeWindows => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == Francais ? Francais : Anglais;

        public static string FichierDeLaLangue(string dossier, string code) => Path.Combine(dossier, "Langues", code + ".json");

        public static void Choisir(string? code, string dossier)
        {   // Langue de l'interface : code "fr" ou "en" (vide : celle de Windows). Un fichier de traduction absent ou illisible
            // laisse l'interface en français (noté dans le journal)
            code = string.IsNullOrWhiteSpace(code) ? CodeDeWindows : code.Trim().ToLowerInvariant();
            _traductions = [];
            Code = Francais;
            if (code == Francais)
                return;
            string fichier = FichierDeLaLangue(dossier, code);
            try
            {
                _traductions = LireTraductions(File.ReadAllText(fichier));
                Code = code;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException)
            {
                Journal.Erreur("Traductions de la langue « " + code + " » illisibles : " + fichier, ex);
            }
        }

        public static Dictionary<string, string> LireTraductions(string json)
        {   // { "phrase française": "traduction" } ; les clés qui commencent par "//" sont des commentaires
            Dictionary<string, string> lues = JsonSerializer.Deserialize<Dictionary<string, string>>(json,
                new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) ?? [];
            lues.Keys.Where(cle => cle.StartsWith("//")).ToList().ForEach(cle => lues.Remove(cle));
            return lues;
        }

        public static string T(string francais) =>
            // Phrase dans la langue choisie (la phrase française si elle n'est pas traduite)
            !EstFrancais && _traductions.TryGetValue(francais, out string? traduction) ? traduction : francais;

        public static string T(string francais, params object?[] valeurs) =>
            // Phrase avec des valeurs : T("Trait aux {0}", camp) ; l'ordre des {0}, {1}... peut changer d'une langue à l'autre
            string.Format(CultureInfo.CurrentCulture, T(francais), valeurs);

        // Un coup en notation française : pièce (R, D, T, F, C), éventuellement case de départ ou levée d'ambiguïté, prise,
        // case d'arrivée, promotion (=D), échec ; ou le roque (sans lettre de pièce, rien à changer)
        private static readonly Regex CoupFrancais = new(@"(?<![\p{L}\d])([RDTFC])([a-h]?[1-8]?[-x]?[a-h][1-8])(=[DTFC])?(?![\p{L}\d])");
        private static readonly Regex PromotionFrancaise = new(@"(?<=[a-h][18])=([DTFC])");
        private static char LettreAnglaise(char lettreFrancaise) => lettreFrancaise switch
        {
            'R' => 'K', 'D' => 'Q', 'T' => 'R', 'F' => 'B', 'C' => 'N', _ => lettreFrancaise
        };

        public static string Notation(string? texte)
        {   // Coups écrits en notation française ("12. Cf3", "Dd8-d7", "e8=D", "19 ... Tac8") : en anglais, les lettres des pièces
            // deviennent celles de la notation anglaise (Nf3, Qd8-d7, e8=Q, Rac8) ; le reste du texte ne change pas
            if (string.IsNullOrEmpty(texte) || EstFrancais)
                return texte ?? "";
            string resultat = CoupFrancais.Replace(texte, m => LettreAnglaise(m.Groups[1].Value[0]) + m.Groups[2].Value
                + (m.Groups[3].Success ? "=" + LettreAnglaise(m.Groups[3].Value[1]) : ""));
            return PromotionFrancaise.Replace(resultat, m => "=" + LettreAnglaise(m.Groups[1].Value[0]));
        }

        public static void ChoisirPourLesTests(string code, Dictionary<string, string> traductions)
        {   // Tests : une langue et ses traductions sans fichier
            Code = code;
            _traductions = traductions;
        }
    }
}
