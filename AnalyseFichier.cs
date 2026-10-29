// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Analyse d'un fichier PGN en arrière-plan, sans interface graphique : chaque partie choisie est lue à part
// (ChargementPartie.CoupsDuPgn, la partie affichée n'est jamais touchée), analysée comme par l'analyse rapide (AnalyseDePartie,
// avec la difficulté des bons coups, les pièges et les menaces), puis ajoutée au fichier de sortie avec ses annotations,
// commentaires, évaluations et bilan (RetourneContenuPgn pour le fichier). Deux Stockfish à elle (MoteurAuxiliaire) : un pour
// l'analyse (threads et hachage donnés), un pour les mesures (1 thread) ; on peut continuer à jouer pendant ce temps.
// └─ Classe "AnalyseurDeFichier"
//              ├─ "Analyser"           toutes les parties choisies, une à une (à lancer hors du thread de l'interface)
//              ├─ "AnalyserLaPartie"   une partie : ses coups annotés (null si elle n'a aucun coup)
//              └─ "Cle"                ce qui reconnaît une partie (en-têtes et coups) : une partie déjà dans le fichier de sortie est sautée

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BrunoGUI_GenII
{
    // Où en est l'analyse : partie k sur n (k à partir de 1), ses positions analysées, le nom de la partie
    public record EtatAnalyseFichier(int Partie, int NombreParties, string NomPartie, int PositionsFaites, int PositionsTotal);

    public sealed class AnalyseurDeFichier(string cheminStockfish, int threads, int hachage, int dureeParPositionMs,
                                           Func<string, string, bool>? coupDeBibliotheque, CatalogueOuvertures? ouvertures) : IDisposable
    {
        private readonly MoteurAuxiliaire _moteur = new(cheminStockfish, threads, hachage);
        private readonly MoteurAuxiliaire _mesures = new(cheminStockfish);     // difficulté, pièges, menaces : comme l'analyse de partie

        public static string FichierDeSortie(string fichier) =>
            // "MesParties.pgn" → "MesParties_analyse.pgn", à côté (le fichier d'origine n'est jamais modifié)
            Path.Combine(Path.GetDirectoryName(fichier) ?? "", Path.GetFileNameWithoutExtension(fichier) + "_analyse.pgn");

        public static string Cle(PartieEchecsPGN pgn)
        {   // Ce qui reconnaît une partie, avant et après son analyse : ses en-têtes principaux et ses coups (UCI)
            string coups = string.Join(" ", ChargementPartie.CoupsDuPgn(pgn).Coups.Select(c => c.Uci.Trim()));
            return string.Join("|", pgn.Tournoi, pgn.Date, pgn.Ronde, pgn.White, pgn.Black, pgn.Result, pgn.Fen, coups);
        }

        public static HashSet<string> PartiesDejaAnalysees(string fichierSortie)
        {   // Les parties déjà dans le fichier de sortie (analyse interrompue puis relancée) : elles sont sautées
            HashSet<string> cles = [];
            if (File.Exists(fichierSortie))
                foreach (string texte in FichierPartiePgn.DecodeFichierPGN(fichierSortie))
                    cles.Add(Cle(FichierPartiePgn.DecodePartiePGN(texte)));
            return cles;
        }

        public int Analyser(IReadOnlyList<PartieEchecsPGN> parties, string fichierSortie, Action<EtatAnalyseFichier>? progression,
                            CancellationToken arret)
        {   // Les parties, une à une ; chacune est ajoutée au fichier de sortie dès qu'elle est finie (une interruption garde les
            // précédentes). Renvoie le nombre de parties ajoutées
            int ajoutees = 0;
            for (int k = 0; k < parties.Count && !arret.IsCancellationRequested; k++)
            {
                PartieEchecsPGN pgn = parties[k];
                string nom = $"{pgn.White} – {pgn.Black}";
                List<Coup>? coups = AnalyserLaPartie(pgn, arret, (faites, total) => progression?.Invoke(new(k + 1, parties.Count, nom, faites, total)));
                if (coups == null || arret.IsCancellationRequested)
                    continue;       // (une partie interrompue n'est pas écrite : elle sera reprise la prochaine fois)
                string contenu = GestionPartiePgn.RetourneContenuPgn(pgn, "Intl", pourFichier: true, coups).Replace("\n", "\r\n");
                // (ajoutée sans relire ni réécrire le fichier, comme l'enregistrement d'une partie)
                File.AppendAllText(fichierSortie, (File.Exists(fichierSortie) && new FileInfo(fichierSortie).Length > 0 ? "\r\n\r\n" : "") + contenu);
                ajoutees++;
            }
            return ajoutees;
        }

        public List<Coup>? AnalyserLaPartie(PartieEchecsPGN pgn, CancellationToken arret, Action<int, int>? progression = null)
        {   // Les coups de la partie, analysés comme par l'analyse rapide ; null si elle n'a aucun coup lisible. pgn reçoit le bilan
            // et, si elle est vide, la balise [ECO]
            List<Coup> coups = ChargementPartie.CoupsDuPgn(pgn).Coups;
            if (!coups.Any(c => !c.EstPositionDeDepart))
                return null;
            foreach (Coup coup in coups)
                coup.EffaceAnalyse();   // (une ancienne analyse du fichier est remplacée ; l'annotation et le commentaire du joueur restent)
            AnalyseDePartie analyse = new(coups, approfondir: false, coupDeBibliotheque, ouvertures);
            _moteur.NouvellePartie();
            int total = analyse.NombreAAnalyser, faites = 0;
            while (!arret.IsCancellationRequested && analyse.PositionSuivante is int index)
            {
                progression?.Invoke(faites, total);
                var reponse = _moteur.Analyse(analyse.Positions[index].Fen, analyse.ProfondeurVisee(index), dureeParPositionMs);
                if (reponse == null && arret.IsCancellationRequested)
                    break;
                // (moteur qui ne répond pas : la position est sautée, comme une position sans score)
                analyse.Enregistre(index, reponse?.Meilleure, reponse?.Meilleure != null ? reponse?.Seconde : null);
                Mesures(analyse, arret);
                faites++;
            }
            if (arret.IsCancellationRequested)
                return null;
            progression?.Invoke(total, total);
            analyse.AppliqueAuxCoups(coups);
            pgn.Bilan = analyse.TexteBilan();
            if (string.IsNullOrWhiteSpace(pgn.ECO) && ouvertures?.DerniereConnue(coups) is (_, Ouverture ouverture))
                pgn.ECO = ouverture.Eco;
            return coups;
        }

        private void Mesures(AnalyseDePartie analyse, CancellationToken arret)
        {   // Comme EchiquierPrincipal.MesureDifficultes : profondeur où les bons coups sont trouvés, prises tentantes qui perdaient,
            // menaces (coup nul) ; par le 2e moteur, qui repart de zéro à chaque question
            foreach ((int index, string fen, string coup) in analyse.DifficultesAMesurer().ToList())
            {
                if (arret.IsCancellationRequested)
                    return;
                analyse.EnregistreDifficulte(index, JugementCoups.ProfondeurDecouverte(
                    _mesures.MeilleursCoupsParProfondeur(fen, JugementCoups.ProfondeurMesureDifficulte), coup));
            }
            foreach ((int index, string fen, string prise) in analyse.PiegesAChercher())
            {
                if (arret.IsCancellationRequested)
                    return;
                var resultat = _mesures.Cherche(fen, prise, JugementCoups.ProfondeurPiege);
                analyse.EnregistrePiege(index, prise, resultat?.Centipions, resultat?.MatEn, resultat?.Variante);
            }
            foreach ((int index, string fenCoupNul) in analyse.MenacesAChercher())
            {
                if (arret.IsCancellationRequested)
                    return;
                var resultat = _mesures.Cherche(fenCoupNul, null, JugementCoups.ProfondeurMenace);
                analyse.EnregistreMenace(index, resultat?.Centipions, resultat?.MatEn, resultat?.Variante);
            }
        }

        public void Dispose()
        {   // Arrête les deux moteurs, même au milieu d'une question (elle renvoie null : la partie en cours n'est pas écrite)
            _moteur.Dispose();
            _mesures.Dispose();
        }
    }
}
