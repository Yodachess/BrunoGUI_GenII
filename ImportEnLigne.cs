// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Import de ses parties jouées sur Lichess ou Chess.com, par pseudo (sans Windows Forms ; adresses et lecture des réponses
// testées, le réseau non)
//      └─ Classe statique "ImportEnLigne"
// API publiques, sans compte : Lichess /api/games/user/{pseudo} (PGN, les plus récentes d'abord ; une seule requête à la fois),
// Chess.com /pub/player/{pseudo}/games/archives puis une archive PGN par mois (requêtes l'une après l'autre, User-Agent
// reconnaissable avec un moyen de contact : l'adresse du projet). Seul le pseudo est envoyé

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrunoGUI_GenII
{
    public enum SiteEnLigne { Lichess, ChessCom }

    public class ErreurImport(string message) : Exception(message);     // erreur à expliquer telle quelle (joueur inconnu, trop de demandes)

    public static class ImportEnLigne
    {
        public const int MinimumParties = 1, MaximumParties = 100;
        private const int MoisChessComMaximum = 6;      // archives mensuelles lues au plus, de la plus récente vers la plus ancienne
        private static readonly HttpClient Client = CreerClient();

        private static HttpClient CreerClient()
        {
            HttpClient client = new(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }) { Timeout = TimeSpan.FromSeconds(60) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BrunoGUI_GenII (+https://github.com/Yodachess/BrunoGUI_GenII)");
            return client;
        }

        public static string NomDuSite(SiteEnLigne site) => site == SiteEnLigne.Lichess ? "Lichess" : "Chess.com";

        public static bool PseudoValable(string pseudo) => Regex.IsMatch(pseudo, "^[A-Za-z0-9_-]{2,30}$");     // (aussi : rien d'autre dans l'adresse)

        public static Uri AdresseLichess(string pseudo, int nombre) =>
            new($"https://lichess.org/api/games/user/{Uri.EscapeDataString(pseudo)}?max={nombre}&clocks=true&evals=true&opening=true&sort=dateDesc");

        public static Uri AdresseArchivesChessCom(string pseudo) =>
            new($"https://api.chess.com/pub/player/{Uri.EscapeDataString(pseudo.ToLowerInvariant())}/games/archives");

        public static List<string> ArchivesChessCom(string json)
        {   // Les adresses des archives mensuelles, dans l'ordre chronologique : {"archives":["https://api.chess.com/pub/player/x/games/2026/10", …]}
            using JsonDocument document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("archives", out JsonElement archives) || archives.ValueKind != JsonValueKind.Array)
                return [];
            return [.. archives.EnumerateArray().Select(a => a.GetString() ?? "").Where(a => a.StartsWith("https://api.chess.com/pub/player/"))];
        }

        public static List<string> DernieresParties(IEnumerable<string> moisDuPlusRecent, int nombre)
        {   // Les parties des archives de Chess.com (chaque mois dans l'ordre chronologique, les mois du plus récent au plus ancien) :
            // les « nombre » plus récentes, la plus récente d'abord, comme Lichess
            List<string> parties = [];
            foreach (string mois in moisDuPlusRecent)
            {
                parties.AddRange(Enumerable.Reverse(FichierPartiePgn.DecoupeParties(mois)));
                if (parties.Count >= nombre)
                    break;
            }
            return [.. parties.Take(nombre)];
        }

        public static async Task<string> TelechargerPgn(SiteEnLigne site, string pseudo, int nombre)
        {   // Le texte PGN des dernières parties du joueur, les plus récentes d'abord ("" s'il n'en a pas)
            if (site == SiteEnLigne.Lichess)
                return await Lire(AdresseLichess(pseudo, nombre), "application/x-chess-pgn", site, pseudo);
            List<string> archives = ArchivesChessCom(await Lire(AdresseArchivesChessCom(pseudo), "application/json", site, pseudo));
            List<string> mois = [];
            foreach (string archive in Enumerable.Reverse(archives).Take(MoisChessComMaximum))
            {   // l'une après l'autre (demandé par Chess.com), jusqu'à avoir assez de parties
                mois.Add(await Lire(new Uri(archive + "/pgn"), "application/x-chess-pgn", site, pseudo));
                if (DernieresParties(mois, nombre).Count >= nombre)
                    break;
            }
            return string.Join("\n\n", DernieresParties(mois, nombre));
        }

        private static async Task<string> Lire(Uri adresse, string format, SiteEnLigne site, string pseudo)
        {
            using HttpRequestMessage demande = new(HttpMethod.Get, adresse);
            demande.Headers.Accept.ParseAdd(format);
            using HttpResponseMessage reponse = await Client.SendAsync(demande);
            if (reponse.StatusCode == HttpStatusCode.NotFound)
                throw new ErreurImport(Langue.T("Joueur inconnu sur {0} : {1}", NomDuSite(site), pseudo));
            if (reponse.StatusCode == HttpStatusCode.TooManyRequests)
                throw new ErreurImport(Langue.T("{0} demande d'attendre un peu : réessayez dans une minute.", NomDuSite(site)));
            reponse.EnsureSuccessStatusCode();
            return await reponse.Content.ReadAsStringAsync();
        }
    }
}
