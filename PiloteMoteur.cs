// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Pilotage du moteur, sans interface graphique (testé dans Tests/Program.cs avec un faux moteur)
//  ├─ Classe "LimiteTemps" : temps fixe, sans limite, ou temps de la pendule (et la commande "go" correspondante)
//  ├─ Interface "IMoteur" : ce que le pilote attend du moteur (MoteurUci dans l'application)
//  └─ Classe "PiloteMoteur" : ce qui a été demandé au moteur, et ce qu'il faut faire de sa réponse
//              ├─ "DemanderCoup"       coup de la partie : bibliothèque d'ouvertures d'abord, sinon le moteur
//              ├─ "DemanderAnalyse"    analyse d'une position (celle affichée, éventuellement passée)
//              ├─ "Abandonner"         la demande en cours devient périmée (sa réponse sera ignorée par MoteurUci)
//              ├─ "ReponseRecue"       un bestmove valable est arrivé : à quelle demande répond-il ?
//              └─ "JouerCoupUci"       joue un coup au format UCI ("e2e4", "e7e8q"), promotion comprise
// Le formulaire garde l'affichage (minuterie, textes, couleurs des cases, message de fin d'analyse).

using System;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public sealed record LimiteTemps
    {   // Temps donné au moteur pour une recherche : un temps fixe ("go movetime"), sans limite ("go infinite", jusqu'à "stop"),
        // ou les temps de la pendule ("go wtime ... btime ... winc ... binc ...") : le moteur gère alors son temps lui-même
        public int? DureeMilliSecondes { get; private init; }       // temps fixe (null : pendule ou sans limite)
        public bool Infinie { get; private init; }
        public TimeSpan TempsBlancs { get; private init; }          // pendule : temps restant de chaque camp et bonus par coup
        public TimeSpan TempsNoirs { get; private init; }
        public TimeSpan Increment { get; private init; }
        public int? CoupsAvantControle { get; private init; }       // cadence à deux périodes : coups à jouer avant le contrôle ("movestogo")

        public static LimiteTemps Duree(int dureeMilliSecondes) =>
            // Au moins 1 s : "go movetime 0" ferait réfléchir certains moteurs (Stockfish) sans fin
            new() { DureeMilliSecondes = Math.Max(1000, dureeMilliSecondes) };
        public static readonly LimiteTemps SansLimite = new() { Infinie = true };
        public static LimiteTemps ParPendule(TimeSpan tempsBlancs, TimeSpan tempsNoirs, TimeSpan increment, int? coupsAvantControle = null) =>
            new() { TempsBlancs = tempsBlancs, TempsNoirs = tempsNoirs, Increment = increment, CoupsAvantControle = coupsAvantControle };
        public static LimiteTemps DepuisPendule(Pendule pendule, int numeroDuCoupAJouer = 0)
        {   // Temps restants de la pendule ; avant le contrôle d'une cadence à deux périodes, le nombre de coups qui restent
            // (sinon le moteur répartirait tout son temps restant jusqu'à la fin de la partie, sans savoir qu'il en recevra)
            Cadence cadence = pendule.Cadence;
            int? coupsAvantControle = cadence.ADeuxPeriodes && numeroDuCoupAJouer >= 1 && numeroDuCoupAJouer <= cadence.CoupsControle
                ? cadence.CoupsControle - numeroDuCoupAJouer + 1 : null;
            return ParPendule(pendule.TempsRestant(ColorPiece.Blanc), pendule.TempsRestant(ColorPiece.Noir), cadence.Increment, coupsAvantControle);
        }

        public string CommandeGo
        {   // Commande UCI (temps en millisecondes ; jamais 0 avec une pendule : le moteur jouerait sans réfléchir du tout)
            get
            {
                if (Infinie)
                    return "go infinite";
                if (DureeMilliSecondes is int duree)
                    return "go movetime " + duree;
                static long Ms(TimeSpan temps) => Math.Max(1, (long)temps.TotalMilliseconds);
                return $"go wtime {Ms(TempsBlancs)} btime {Ms(TempsNoirs)} winc {(long)Increment.TotalMilliseconds} binc {(long)Increment.TotalMilliseconds}"
                    + (CoupsAvantControle is int coups ? $" movestogo {coups}" : "");
            }
        }
    }

    public interface IMoteur
    {
        // "position fen ..." puis "go" (la demande précédente est abandonnée) ; forceMaximale : sans la limite de force réglée
        // (Elo, niveau), pour une analyse
        void Chercher(string fen, LimiteTemps limite, bool forceMaximale);
        void Abandonner();                                  // la demande en cours devient périmée ("stop")
        bool EnReflexion { get; }                           // le moteur réfléchit à une demande toujours valable
    }

    public enum TypeDemande { Aucune, CoupDePartie, Analyse }

    public enum ResultatDemandeCoup
    {
        CoupBibliotheque,   // le coup de la bibliothèque est déjà joué
        EnvoyeAuMoteur      // le moteur réfléchit : son coup arrivera par ReponseRecue
    }

    public class PiloteMoteur
    {
        private readonly IMoteur _moteur;
        public PiloteMoteur(IMoteur moteur) => _moteur = moteur;

        public TypeDemande Demande { get; private set; } = TypeDemande.Aucune;     // ce que le moteur est en train de chercher
        public Position? PositionAnalysee { get; private set; }                      // position de l'analyse en cours (copie)
        public bool AnalyseEnCours => Demande == TypeDemande.Analyse;
        public string? DernierCoupBibliotheque { get; private set; }                 // dernier coup joué depuis la bibliothèque
        public SuiviAnalyse Lignes { get; } = new();    // variantes et scores reçus pour la demande en cours (voir AnalyseMoteur.cs)

        // Choix dans la bibliothèque d'ouvertures : FEN -> coup UCI, ou null/vide s'il n'y en a pas (null : pas de bibliothèque)
        public Func<string, string?>? ChoixBibliotheque { get; set; }

        public ResultatDemandeCoup DemanderCoup(string fen, LimiteTemps limite)
        {   // Coup de la partie pour le camp au trait : la bibliothèque d'abord (coup joué tout de suite), sinon le moteur
            // (avec un temps fixe, ou les temps de la pendule).
            // Un coup de bibliothèque illégal (bibliothèque qui ne correspond pas à la position) laisse la main au moteur
            Abandonner();
            string? coup = ChoixBibliotheque?.Invoke(fen);
            if (!string.IsNullOrEmpty(coup) && JouerCoupUci(coup))
            {
                DernierCoupBibliotheque = coup;
                return ResultatDemandeCoup.CoupBibliotheque;
            }
            Demande = TypeDemande.CoupDePartie;
            Lignes.Reinitialiser();
            _moteur.Chercher(fen, limite, forceMaximale: false);     // le moteur joue à la force réglée
            return ResultatDemandeCoup.EnvoyeAuMoteur;
        }

        public void DemanderAnalyse(Position position, int dureeMilliSecondes)
        {   // Analyse d'une position (celle affichée) : ses variantes seront converties sur PositionAnalysee.
            // Toujours à pleine force : un moteur limité (Elo, niveau) joue exprès des coups plus faibles, et Stockfish annonce alors
            // ce coup faible comme sa variante principale, souvent avec une variante très courte et un score de 0.00
            Abandonner();
            PositionAnalysee = position.Copier();
            Demande = TypeDemande.Analyse;
            Lignes.Reinitialiser();
            _moteur.Chercher(CalculerSur(position, RetourneChaineFenActuel), LimiteTemps.Duree(dureeMilliSecondes), forceMaximale: true);
        }

        public bool Abandonner()
        {   // La demande en cours devient périmée ; renvoie true si le moteur réfléchissait (l'interface le signale alors)
            bool reflechissait = _moteur.EnReflexion;
            if (reflechissait)
                _moteur.Abandonner();
            Demande = TypeDemande.Aucune;
            return reflechissait;
        }

        public TypeDemande ReponseRecue()
        {   // Un bestmove valable (non périmé) vient d'arriver : il répond à la demande en cours, qui est terminée
            TypeDemande demande = Demande;
            Demande = TypeDemande.Aucune;
            return demande;
        }

        public static bool JouerCoupUci(string? coupUci)
        {   // Joue un coup au format UCI ("e2e4", "e7e8q" : le 5e caractère est la pièce de promotion) ; renvoie false s'il est illégal
            if (string.IsNullOrEmpty(coupUci) || coupUci.Length < 4 || EchecetMat)
                return false;
            // Pièce de promotion imposée (le joueur n'a pas à choisir) : celle du 5e caractère, sinon une dame ;
            // elle ne sert que si le coup est vraiment une promotion, et prend la couleur du pion (PieceDeLaCouleur)
            TypePiece promotion = coupUci.Length >= 5 ? PieceDePromotion(coupUci[4], ColorPiece.Blanc) : TypePiece.ReineBlanche;
            ExecutionCoup(coupUci[..2], coupUci.Substring(2, 2), promotion);
            return CoupValide;
        }
    }
}
