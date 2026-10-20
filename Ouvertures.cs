// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Noms des ouvertures (code ECO), sans interface graphique (testé dans Tests/Scenario.cs)
//  ├─ Record "Ouverture"             : code ECO et nom d'une ouverture, en anglais (table de Lichess) ou en français
//  ├─ Classe "CatalogueOuvertures"   : la table Ouvertures\ouvertures.tsv (Lichess, CC0), cherchée par POSITION (transpositions)
//  └─ Classe "TraductionOuvertures"  : nom français ("Défense sicilienne : variante Najdorf, attaque anglaise")
// La table vient de https://github.com/lichess-org/chess-openings (domaine public, CC0) : colonnes eco, name, pgn.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public record Ouverture(string Eco, string NomAnglais, int DemiCoups)
    {
        public string NomFrancais => TraductionOuvertures.Traduit(NomAnglais);
        public string Nom => Langue.EstFrancais ? NomFrancais : NomAnglais;     // dans la langue de l'interface
        public string Texte => $"{Nom} ({Eco})";                                // "Défense sicilienne : variante Najdorf (B90)"
    }

    public class CatalogueOuvertures
    {
        private readonly Dictionary<string, Ouverture> _parPosition = [];
        public int Nombre => _parPosition.Count;
        public int LignesIllisibles { get; private set; }       // lignes dont un coup n'a pas pu être joué (ignorées)

        public static string Cle(string fen) =>
            // Position sans les compteurs ni la case en passant (pièces, trait, roques) : une même position atteinte par un autre
            // ordre de coups (transposition) a la même clé
            string.Join(" ", fen.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3));

        public static CatalogueOuvertures Lire(string texte)
        {   // Le contenu de ouvertures.tsv : "eco<tab>name<tab>pgn<tab>position" par ligne (en-tête "eco" et lignes "#" ignorés) ; une
            // position déjà connue garde son 1er nom. La position (Cle) est calculée d'avance par AjouteCles : rejouer les 3 865 lignes
            // prenait 22 s ; sans elle, la ligne est rejouée
            CatalogueOuvertures catalogue = new();
            foreach (string ligne in texte.Split('\n'))
            {
                string[] colonnes = ligne.TrimEnd('\r').Split('\t');
                if (colonnes.Length < 3 || colonnes[0] == "eco" || colonnes[0].StartsWith('#'))
                    continue;
                string? cle = colonnes.Length >= 4 && colonnes[3] != "" ? colonnes[3] : CleDesCoups(colonnes[2]);
                if (cle == null)
                {
                    catalogue.LignesIllisibles++;
                    continue;
                }
                int demiCoups = colonnes[2].Split(' ', StringSplitOptions.RemoveEmptyEntries).Count(m => !m.EndsWith('.'));
                catalogue._parPosition.TryAdd(cle, new Ouverture(colonnes[0], Simplifie(colonnes[1]), demiCoups));
            }
            return catalogue;
        }

        public static string? CleDesCoups(string coupsPgn)
        {   // La position après les coups ("1. e4 c5 2. Nf3", lettres anglaises), jouée sur une copie de la position initiale (rien
            // n'est modifié ni redessiné) : sa clé, ou null si un coup est illisible ou illégal
            Position initiale = PositionDepuisFen(FenDepart);
            string uci = ChargementPartie.VarianteSanVersUci(coupsPgn, initiale);
            int demiCoups = coupsPgn.Split(' ', StringSplitOptions.RemoveEmptyEntries).Count(m => !m.EndsWith('.'));
            if (demiCoups == 0 || uci.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length != demiCoups)
                return null;
            Position finale = ChargementPartie.PositionsDeLaVariante(initiale, uci, demiCoups)[^1].Position;
            return Cle(CalculerSur(finale, RetourneChaineFenActuel));
        }

        public static string AjouteCles(string texte)
        {   // Le fichier avec la 4e colonne (position) calculée pour chaque ligne : à refaire si la table de Lichess est mise à jour
            List<string> lignes = [];
            foreach (string ligne in texte.Split('\n').Select(l => l.TrimEnd('\r')).Where(l => l != ""))
            {
                string[] colonnes = ligne.Split('\t');
                lignes.Add(colonnes[0].StartsWith('#') || colonnes.Length < 3 ? ligne
                           : colonnes[0] == "eco" ? "eco\tname\tpgn\tposition"
                           : string.Join('\t', colonnes.Take(3)) + "\t" + CleDesCoups(colonnes[2]));
            }
            return string.Join("\r\n", lignes) + "\r\n";
        }

        public Ouverture? Trouver(string fen) => _parPosition.GetValueOrDefault(Cle(fen));

        public (int Index, Ouverture Ouverture)? DerniereConnue(IReadOnlyList<Coup> coups)
        {   // Le dernier coup (place dans la liste) qui mène à une position de la table, et l'ouverture de la partie à ce coup
            // (OuvertureAuCoup). Une partie commencée à une position (FEN) : sa position de départ compte aussi (index 0)
            for (int i = coups.Count - 1; i >= 0; i--)
                if (Trouver(coups[i].Fen) != null && OuvertureAuCoup(coups, i) is Ouverture ouverture)
                    return (i, ouverture);
            return null;
        }

        public Ouverture? OuvertureAuCoup(IReadOnlyList<Coup> coups, int index)
        {   // L'ouverture de la partie après le coup "index" : la dernière position de la table rencontrée, sauf si son nom est moins
            // précis que celui déjà trouvé (la table de Lichess donne "French Defense: Normal Variation" après 2. d4, puis "French
            // Defense" après 2... d5 : le nom revenait en arrière, remarque de Bruno) ; null si aucune position n'est connue
            Ouverture? courante = null;
            for (int i = 0; i <= index && i < coups.Count; i++)
                if (Trouver(coups[i].Fen) is Ouverture ouverture && (courante == null || !EstMoinsPrecis(ouverture, courante)))
                    courante = ouverture;
            return courante;
        }

        private static bool EstMoinsPrecis(Ouverture nouvelle, Ouverture courante) =>
            // "French Defense" est moins précis que "French Defense: Advance Variation" ; un nom égal remplace l'ancien (code ECO
            // de la position la plus récente)
            courante.NomAnglais.Length > nouvelle.NomAnglais.Length && courante.NomAnglais.StartsWith(nouvelle.NomAnglais)
            && courante.NomAnglais[nouvelle.NomAnglais.Length] is ':' or ',';

        public static string Simplifie(string nomAnglais)
        {   // Sans les variantes "normales" ("French Defense: Normal Variation" → "French Defense") : ce mot ne dit rien (remarque
            // de Bruno) ; la famille reste toujours
            int deuxPoints = nomAnglais.IndexOf(':');
            if (deuxPoints < 0)
                return nomAnglais;
            string[] variantes = [.. nomAnglais[(deuxPoints + 1)..].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                                                                  .Where(v => v != "Normal" && !v.StartsWith("Normal "))];
            return variantes.Length == 0 ? nomAnglais[..deuxPoints] : nomAnglais[..deuxPoints] + ": " + string.Join(", ", variantes);
        }
    }

    public static class TraductionOuvertures
    {   // Nom anglais de Lichess → français : la famille ("Sicilian Defense" → "Défense sicilienne"), puis chaque variante
        // ("Najdorf Variation" → "variante Najdorf", "Exchange Variation" → "variante d'échange") ; les noms propres et les mots
        // inconnus restent tels quels (choix de Bruno), la notation des coups passe en français ("4. Bg5" → "4. Fg5")

        private static readonly Dictionary<string, string> Familles = new()
        {
            ["Alekhine Defense"] = "Défense Alekhine", ["Amar Opening"] = "Ouverture Amar", ["Amazon Attack"] = "Attaque amazone",
            ["Amsterdam Attack"] = "Attaque d'Amsterdam", ["Anderssen's Opening"] = "Ouverture Anderssen",
            ["Australian Defense"] = "Défense australienne", ["Barnes Defense"] = "Défense Barnes", ["Barnes Opening"] = "Ouverture Barnes",
            ["Basque Opening"] = "Ouverture basque", ["Benko Gambit"] = "Gambit Benko", ["Benko Gambit Accepted"] = "Gambit Benko accepté",
            ["Benko Gambit Declined"] = "Gambit Benko refusé", ["Benoni Defense"] = "Défense Benoni", ["Bird Opening"] = "Ouverture Bird",
            ["Bishop's Opening"] = "Ouverture du fou", ["Blackmar-Diemer Gambit"] = "Gambit Blackmar-Diemer",
            ["Blackmar-Diemer Gambit Accepted"] = "Gambit Blackmar-Diemer accepté", ["Blackmar-Diemer Gambit Declined"] = "Gambit Blackmar-Diemer refusé",
            ["Blumenfeld Countergambit"] = "Contre-gambit Blumenfeld", ["Blumenfeld Countergambit Accepted"] = "Contre-gambit Blumenfeld accepté",
            ["Bogo-Indian Defense"] = "Défense bogo-indienne", ["Borg Defense"] = "Défense Borg", ["Canard Opening"] = "Ouverture du canard",
            ["Caro-Kann Defense"] = "Défense Caro-Kann", ["Carr Defense"] = "Défense Carr", ["Catalan Opening"] = "Ouverture catalane",
            ["Center Game"] = "Partie du centre", ["Center Game Accepted"] = "Partie du centre acceptée", ["Clemenz Opening"] = "Ouverture Clemenz",
            ["Colle System"] = "Système Colle", ["Czech Defense"] = "Défense tchèque", ["Danish Gambit"] = "Gambit danois",
            ["Danish Gambit Accepted"] = "Gambit danois accepté", ["Danish Gambit Declined"] = "Gambit danois refusé",
            ["Dresden Opening"] = "Ouverture de Dresde", ["Duras Gambit"] = "Gambit Duras", ["Dutch Defense"] = "Défense hollandaise",
            ["East Indian Defense"] = "Défense est-indienne", ["Elephant Gambit"] = "Gambit de l'éléphant", ["English Defense"] = "Défense anglaise",
            ["English Opening"] = "Ouverture anglaise", ["English Orangutan"] = "Orang-outan anglais", ["Englund Gambit"] = "Gambit Englund",
            ["Englund Gambit Declined"] = "Gambit Englund refusé", ["Four Knights Game"] = "Partie des quatre cavaliers",
            ["French Defense"] = "Défense française", ["Goldsmith Defense"] = "Défense Goldsmith", ["Grob Opening"] = "Ouverture Grob",
            ["Grünfeld Defense"] = "Défense Grünfeld", ["Hippopotamus Defense"] = "Défense hippopotame", ["Horwitz Defense"] = "Défense Horwitz",
            ["Hungarian Opening"] = "Ouverture hongroise", ["Indian Defense"] = "Défense indienne", ["Irish Gambit"] = "Gambit irlandais",
            ["Italian Game"] = "Partie italienne", ["Kangaroo Defense"] = "Défense kangourou", ["Kádas Opening"] = "Ouverture Kádas",
            ["King's Gambit"] = "Gambit du roi", ["King's Gambit Accepted"] = "Gambit du roi accepté", ["King's Gambit Declined"] = "Gambit du roi refusé",
            ["King's Indian Attack"] = "Attaque est-indienne", ["King's Indian Defense"] = "Défense est-indienne",
            ["King's Knight Opening"] = "Ouverture du cavalier roi", ["King's Pawn Game"] = "Partie du pion roi",
            ["King's Pawn Opening"] = "Ouverture du pion roi", ["Latvian Gambit"] = "Gambit letton", ["Latvian Gambit Accepted"] = "Gambit letton accepté",
            ["Lion Defense"] = "Défense du lion", ["London System"] = "Système de Londres", ["Mexican Defense"] = "Défense mexicaine",
            ["Mieses Opening"] = "Ouverture Mieses", ["Mikenas Defense"] = "Défense Mikenas", ["Modern Defense"] = "Défense moderne",
            ["Montevideo Defense"] = "Défense de Montevideo", ["Neo-Grünfeld Defense"] = "Défense néo-Grünfeld",
            ["Nimzo-Indian Defense"] = "Défense nimzo-indienne", ["Nimzo-Larsen Attack"] = "Attaque Nimzo-Larsen",
            ["Nimzowitsch Defense"] = "Défense Nimzowitsch", ["Old Indian Defense"] = "Défense vieille-indienne", ["Owen Defense"] = "Défense Owen",
            ["Petrov's Defense"] = "Défense Petrov", ["Philidor Defense"] = "Défense Philidor", ["Pirc Defense"] = "Défense Pirc",
            ["Polish Defense"] = "Défense polonaise", ["Polish Opening"] = "Ouverture polonaise", ["Ponziani Opening"] = "Ouverture Ponziani",
            ["Portuguese Opening"] = "Ouverture portugaise", ["Pseudo Queen's Indian Defense"] = "Défense pseudo-ouest-indienne",
            ["Pterodactyl Defense"] = "Défense ptérodactyle", ["Queen's Gambit"] = "Gambit dame", ["Queen's Gambit Accepted"] = "Gambit dame accepté",
            ["Queen's Gambit Declined"] = "Gambit dame refusé", ["Queen's Indian Accelerated"] = "Défense ouest-indienne accélérée",
            ["Queen's Indian Defense"] = "Défense ouest-indienne", ["Queen's Pawn"] = "Pion dame", ["Queen's Pawn Game"] = "Partie du pion dame",
            ["Rat Defense"] = "Défense du rat", ["Richter-Veresov Attack"] = "Attaque Richter-Veresov", ["Robatsch Defense"] = "Défense Robatsch",
            ["Rubinstein Opening"] = "Ouverture Rubinstein", ["Ruy Lopez"] = "Partie espagnole", ["Réti Opening"] = "Ouverture Réti",
            ["Saragossa Opening"] = "Ouverture de Saragosse", ["Scandinavian Defense"] = "Défense scandinave", ["Scotch Game"] = "Partie écossaise",
            ["Semi-Slav Defense"] = "Défense semi-slave", ["Semi-Slav Defense Accepted"] = "Défense semi-slave acceptée",
            ["Sicilian Defense"] = "Défense sicilienne", ["Slav Defense"] = "Défense slave", ["Slav Indian"] = "Slave indienne",
            ["St. George Defense"] = "Défense Saint-Georges", ["Tarrasch Defense"] = "Défense Tarrasch",
            ["Three Knights Opening"] = "Partie des trois cavaliers", ["Torre Attack"] = "Attaque Torre", ["Trompowsky Attack"] = "Attaque Trompowsky",
            ["Valencia Opening"] = "Ouverture de Valence", ["Van Geet Opening"] = "Ouverture Van Geet", ["Van't Kruijs Opening"] = "Ouverture Van't Kruijs",
            ["Vienna Gambit"] = "Gambit viennois", ["Vienna Game"] = "Partie viennoise", ["Vulture Defense"] = "Défense du vautour",
            ["Ware Defense"] = "Défense Ware", ["Ware Opening"] = "Ouverture Ware", ["Yusupov-Rubinstein System"] = "Système Yusupov-Rubinstein",
            ["Zukertort Defense"] = "Défense Zukertort", ["Zukertort Opening"] = "Ouverture Zukertort",
        };

        // Mot principal d'une variante, placé en tête en français, et son genre (pour accorder les adjectifs)
        private static readonly Dictionary<string, (string Nom, bool Feminin)> Tetes = new()
        {
            ["Variation"] = ("variante", true), ["Attack"] = ("attaque", true), ["Gambit"] = ("gambit", false),
            ["Countergambit"] = ("contre-gambit", false), ["Defense"] = ("défense", true), ["System"] = ("système", false),
            ["Line"] = ("ligne", true), ["Opening"] = ("ouverture", true), ["Game"] = ("partie", true),
            ["Formation"] = ("formation", true), ["Trap"] = ("piège", false), ["Counterattack"] = ("contre-attaque", true),
            ["Dragon"] = ("dragon", false), ["Setup"] = ("dispositif", false), ["Structure"] = ("structure", true),
            ["Invitation"] = ("invitation", true), ["Sacrifice"] = ("sacrifice", false), ["Fianchetto"] = ("fianchetto", false),
            ["Exchange"] = ("échange", false),
        };

        // Adjectifs (masculin, féminin), placés après le nom
        private static readonly Dictionary<string, (string M, string F)> Adjectifs = new()
        {
            ["Classical"] = ("classique", "classique"), ["Modern"] = ("moderne", "moderne"), ["Closed"] = ("fermé", "fermée"),
            ["Open"] = ("ouvert", "ouverte"), ["Normal"] = ("normal", "normale"), ["Main"] = ("principal", "principale"),
            ["Orthodox"] = ("orthodoxe", "orthodoxe"), ["Symmetrical"] = ("symétrique", "symétrique"), ["Accelerated"] = ("accéléré", "accélérée"),
            ["Reversed"] = ("inversé", "inversée"), ["Quiet"] = ("tranquille", "tranquille"), ["Traditional"] = ("traditionnel", "traditionnelle"),
            ["Old"] = ("ancien", "ancienne"), ["Deferred"] = ("différé", "différée"), ["Delayed"] = ("différé", "différée"), ["Declined"] = ("refusé", "refusée"),
            ["Accepted"] = ("accepté", "acceptée"), ["Positional"] = ("positionnel", "positionnelle"), ["Central"] = ("central", "centrale"),
            ["Double"] = ("double", "double"), ["Short"] = ("court", "courte"), ["Long"] = ("long", "longue"),
            ["Advanced"] = ("avancé", "avancée"), ["Improved"] = ("amélioré", "améliorée"), ["Refined"] = ("affiné", "affinée"),
            ["Anti-Marshall"] = ("anti-Marshall", "anti-Marshall"), ["Neo-Classical"] = ("néo-classique", "néo-classique"),
            ["Hypermodern"] = ("hypermoderne", "hypermoderne"), ["Wild"] = ("sauvage", "sauvage"), ["Irregular"] = ("irrégulier", "irrégulière"),
            ["Alternative"] = ("alternatif", "alternative"), ["Original"] = ("original", "originale"), ["Fianchettoed"] = ("en fianchetto", "en fianchetto"),
            ["Poisoned"] = ("empoisonné", "empoisonnée"), ["Neo-Modern"] = ("néo-moderne", "néo-moderne"),
            ["Anglo-Indian"] = ("anglo-indien", "anglo-indienne"), ["Anglo-Scandinavian"] = ("anglo-scandinave", "anglo-scandinave"),
            ["Anglo-Dutch"] = ("anglo-hollandais", "anglo-hollandaise"), ["Anglo-Slav"] = ("anglo-slave", "anglo-slave"),
            ["King's Indian"] = ("est-indien", "est-indienne"), ["Queen's Indian"] = ("ouest-indien", "ouest-indienne"),
            ["King's English"] = ("anglais du roi", "anglaise du roi"), ["Semi-Classical"] = ("semi-classique", "semi-classique"),
            ["Anti-Classical"] = ("anti-classique", "anti-classique"), ["Immediate"] = ("immédiat", "immédiate"),
        };

        // Adjectifs de nationalité ou de famille ("Sicilian Variation" → "variante sicilienne"), placés juste après le nom
        private static readonly Dictionary<string, (string M, string F)> Nationalites = new()
        {
            ["Sicilian"] = ("sicilien", "sicilienne"), ["French"] = ("français", "française"), ["Scandinavian"] = ("scandinave", "scandinave"),
            ["Yugoslav"] = ("yougoslave", "yougoslave"), ["Slav"] = ("slave", "slave"), ["Catalan"] = ("catalan", "catalane"),
            ["Norwegian"] = ("norvégien", "norvégienne"), ["Bulgarian"] = ("bulgare", "bulgare"), ["Hungarian"] = ("hongrois", "hongroise"),
            ["Argentinian"] = ("argentin", "argentine"), ["Venetian"] = ("vénitien", "vénitienne"), ["Moscow"] = ("moscovite", "moscovite"),
            ["Indian"] = ("indien", "indienne"), ["English"] = ("anglais", "anglaise"), ["Dutch"] = ("hollandais", "hollandaise"),
            ["Polish"] = ("polonais", "polonaise"), ["Russian"] = ("russe", "russe"), ["Austrian"] = ("autrichien", "autrichienne"),
            ["Spanish"] = ("espagnol", "espagnole"), ["Italian"] = ("italien", "italienne"), ["Scotch"] = ("écossais", "écossaise"),
            ["Danish"] = ("danois", "danoise"), ["Swiss"] = ("suisse", "suisse"), ["Czech"] = ("tchèque", "tchèque"),
            ["Romanian"] = ("roumain", "roumaine"), ["Belgian"] = ("belge", "belge"), ["Swedish"] = ("suédois", "suédoise"),
            ["German"] = ("allemand", "allemande"), ["Portuguese"] = ("portugais", "portugaise"), ["Vienna"] = ("viennois", "viennoise"),
        };

        // Adjectifs placés en dernier ("Botvinnik System Reversed" → "système Botvinnik inversé")
        private static readonly HashSet<string> AdjectifsEnDernier = ["Reversed", "Deferred", "Delayed", "Accepted", "Declined"];

        // Compléments ("Exchange Variation" → "variante d'échange"), placés après les adjectifs ; d'un ou plusieurs mots
        private static readonly Dictionary<string, string> Complements = new()
        {
            ["Exchange"] = "d'échange", ["Advance"] = "d'avance", ["Fianchetto"] = "du fianchetto", ["Dragon"] = "du dragon",
            ["Knight"] = "du cavalier", ["Bishop"] = "du fou", ["Rook"] = "de la tour", ["Queen"] = "de la dame", ["King"] = "du roi",
            ["Pawn"] = "du pion", ["Center"] = "du centre", ["Wing"] = "de l'aile", ["Hedgehog"] = "du hérisson",
            ["Two Knights"] = "des deux cavaliers", ["Three Knights"] = "des trois cavaliers", ["Four Knights"] = "des quatre cavaliers",
            ["Two Bishops"] = "des deux fous", ["Four Pawns"] = "des quatre pions", ["Three Pawns"] = "des trois pions",
            ["Two Pawns"] = "des deux pions", ["Pawn Storm"] = "de l'avalanche de pions", ["Queen's Knight"] = "du cavalier dame",
            ["King's Knight"] = "du cavalier roi", ["Queen's Bishop"] = "du fou dame", ["King's Bishop"] = "du fou roi",
            ["Queen Pawn"] = "du pion dame", ["King Pawn"] = "du pion roi", ["Queenside"] = "de l'aile dame", ["Kingside"] = "de l'aile roi",
            ["Countergambit"] = "du contre-gambit", ["Gambit"] = "du gambit", ["Stonewall"] = "Stonewall", ["Panno"] = "Panno",
            ["Bishop's"] = "du fou", ["King's"] = "du roi", ["Queen's"] = "de la dame", ["Knight's"] = "du cavalier",
            ["King's Knight's"] = "du cavalier roi", ["King's Pawn"] = "du pion roi", ["Queen's Pawn"] = "du pion dame",
            ["Retreat"] = "de la retraite", ["Knights"] = "des cavaliers", ["Three Pawn"] = "des trois pions",
            ["Poisoned Pawn"] = "du pion empoisonné",
        };

        // Variantes entières sans mot principal
        private static readonly Dictionary<string, string> Expressions = new()
        {
            ["Main Line"] = "ligne principale", ["Accelerated Dragon"] = "dragon accéléré", ["Hyperaccelerated Dragon"] = "dragon hyperaccéléré",
            ["Closed"] = "fermée", ["Open"] = "ouverte", ["Classical"] = "classique", ["Modern"] = "moderne",
            ["Fried Liver Attack"] = "attaque Fried Liver", ["Double Fianchetto"] = "double fianchetto", ["Central"] = "centrale",
            ["Two Knights"] = "deux cavaliers", ["Three Knights"] = "trois cavaliers", ["Four Knights"] = "quatre cavaliers",
            ["Poisoned Pawn"] = "pion empoisonné", ["Poisoned Pawn Accepted"] = "pion empoisonné accepté",
            ["Poisoned Pawn Declined"] = "pion empoisonné refusé",
        };

        public static string Traduit(string nomAnglais)
        {   // "Sicilian Defense: Najdorf Variation, English Attack" → "Défense sicilienne : variante Najdorf, attaque anglaise"
            int deuxPoints = nomAnglais.IndexOf(':');
            string famille = deuxPoints < 0 ? nomAnglais : nomAnglais[..deuxPoints];
            string[] partiesFamille = famille.Split(", ");
            // (famille absente de la liste, souvent une ouverture rare : traduite comme une variante, "Sodium Attack" → "Attaque Sodium")
            string familleFr = Familles.GetValueOrDefault(partiesFamille[0]) ?? Majuscule(TraduitPartie(partiesFamille[0]));
            List<string> famillesFr = [familleFr, .. partiesFamille.Skip(1).Select(TraduitPartie)];
            string resultat = string.Join(", ", famillesFr);
            if (deuxPoints >= 0)
                resultat += " : " + string.Join(", ", nomAnglais[(deuxPoints + 1)..].Split(',', StringSplitOptions.TrimEntries
                                                                                              | StringSplitOptions.RemoveEmptyEntries)
                                                                                       .Select(TraduitPartie));
            return resultat;
        }

        private static string TraduitPartie(string partie)
        {   // Une variante : "Najdorf Variation" → "variante Najdorf", "with e3" → "avec e3", "4. Bg5" → "4. Fg5"
            if (partie.StartsWith("with "))
                return "avec " + TraduitPartie(partie[5..]);
            if (Expressions.TryGetValue(partie, out string? expression))
                return expression;
            List<string> mots = [.. partie.Split(' ', StringSplitOptions.RemoveEmptyEntries)];
            // Mot principal : le dernier mot, avant d'éventuels "Accepted", "Reversed"… ("Marshall Gambit Accepted")
            int placeTete = mots.Count - 1;
            while (placeTete > 0 && AdjectifsEnDernier.Contains(mots[placeTete]))
                placeTete--;
            (string Nom, bool Feminin)? tete = placeTete >= 0 && Tetes.TryGetValue(mots[placeTete], out var trouvee) ? trouvee : null;
            if (tete != null)
                mots.RemoveAt(placeTete);
            bool feminin = tete?.Feminin ?? true;       // sans mot principal ("Reversed Sicilian") : une défense, une variante
            List<string> nationalites = [], nomsPropres = [], adjectifs = [], enDernier = [], complements = [];
            for (int i = 0; i < mots.Count; i++)
            {
                string? deuxMots = i + 1 < mots.Count ? mots[i] + " " + mots[i + 1] : null;
                if (deuxMots != null && Complements.TryGetValue(deuxMots, out string? complementDouble))
                    complements.Add(complementDouble);
                else if (deuxMots != null && Adjectifs.TryGetValue(deuxMots, out var adjectifDouble))
                    adjectifs.Add(feminin ? adjectifDouble.F : adjectifDouble.M);
                else
                {
                    if (Nationalites.TryGetValue(mots[i], out var nationalite))
                        nationalites.Add(feminin ? nationalite.F : nationalite.M);
                    else if (Adjectifs.TryGetValue(mots[i], out var adjectif))
                        (AdjectifsEnDernier.Contains(mots[i]) ? enDernier : adjectifs).Add(feminin ? adjectif.F : adjectif.M);
                    else if (Complements.TryGetValue(mots[i], out string? complement))
                        complements.Add(complement);
                    else
                        nomsPropres.Add(Notation(mots[i]));     // nom propre ("Najdorf"), ou mot inconnu laissé en anglais
                    continue;
                }
                i++;            // (expression de deux mots)
            }
            if (tete == null && (complements.Count > 0 || nationalites.Count + adjectifs.Count + enDernier.Count == 0))
                return Notation(partie);        // rien à traduire, ou trop incertain sans mot principal : laissé en anglais
            IEnumerable<string> debut = tete is (string Nom, bool) t ? [t.Nom] : [];
            return string.Join(" ", debut.Concat(nationalites).Concat(nomsPropres).Concat(adjectifs).Concat(complements).Concat(enDernier));
        }

        private static string Majuscule(string texte) => texte.Length == 0 ? texte : char.ToUpper(texte[0]) + texte[1..];

        private static string Notation(string texte) =>
            // Les coups écrits dans le nom ("4. Bg5", "Bb4+", "Nxe5") passent en notation française ("4. Fg5") ; un coup noir
            // écrit ".. Nc6" par Lichess devient "...Cc6"
            Regex.Replace(texte.Replace(".. ", "..."), @"\b([KQRBN])(?=[a-h1-8]?x?[a-h][1-8])",
                          m => m.Value switch { "K" => "R", "Q" => "D", "R" => "T", "B" => "F", _ => "C" });
    }
}
