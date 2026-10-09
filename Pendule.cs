// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Pendule d'échecs, sans interface graphique (testée dans Tests/Program.cs)
//  ├─ Structure "Cadence" : temps de la partie et bonus par coup (ex : 5 min + 3 s), ou "Sans pendule"
//  └─ Classe "Pendule"   : temps restant de chaque camp, seul le camp au trait décompte
//              ├─ "Demarrer", "CoupJoue" (bonus ajouté, l'autre camp décompte), "Pause", "Reprendre", "Arreter"
//              ├─ "TempsRestant", "TempsEcoule"
//              ├─ "NoteTemps"  temps restants notés dans le Coup qui vient d'être joué
//              └─ "Restaurer", "RestaurerDepuis"  (retour arrière, "Reprendre ici" : temps notés après le dernier coup restant)
// L'heure est donnée par une fonction (un chronomètre dans l'application, un temps simulé dans les tests) :
// la pendule calcule le temps écoulé, elle ne compte pas les tics d'une minuterie.

using System;
using static BrunoGUI_GenII.LogiqueMouvements;

#nullable enable

namespace BrunoGUI_GenII
{
    // Cadence : temps initial et bonus par coup. Cadence à deux périodes (tournoi) : au coup n° CoupsControle, chaque camp
    // reçoit TempsAjoute (ex : FIDE, 90 min pour 40 coups puis 30 min, avec 30 s par coup depuis le début)
    public readonly record struct Cadence(TimeSpan TempsInitial, TimeSpan Increment, int CoupsControle = 0, TimeSpan TempsAjoute = default)
    {
        public static readonly Cadence SansPendule = new(TimeSpan.Zero, TimeSpan.Zero);
        public static Cadence Minutes(int minutes, int incrementSecondes = 0) =>
            new(TimeSpan.FromMinutes(minutes), TimeSpan.FromSeconds(incrementSecondes));
        public static readonly Cadence Fide = new(TimeSpan.FromMinutes(90), TimeSpan.FromSeconds(30), 40, TimeSpan.FromMinutes(30));

        // Cadences proposées dans l'interface ("Sans pendule" en premier : le moteur a un temps fixe par coup)
        public static readonly Cadence[] Proposees =
            [SansPendule, Minutes(3, 2), Minutes(5), Minutes(5, 3), Minutes(10, 5), Minutes(15, 10), Minutes(30), Fide];

        public bool EstSansPendule => TempsInitial <= TimeSpan.Zero;
        public bool ADeuxPeriodes => CoupsControle > 0 && TempsAjoute > TimeSpan.Zero;
        // Cadences officielles de la FIDE (blitz 3 + 2, rapide 15 + 10, classique) : marquées d'une étoile dorée dans les listes
        public bool EstOfficielle => this == Minutes(3, 2) || this == Minutes(15, 10) || this == Fide;

        private string TexteIncrement(string format) => Increment > TimeSpan.Zero ? string.Format(format, Increment.TotalSeconds) : "";
        // Sans bonus, une seule période : "KO" (le temps est perdu au drapeau, sans rien gagner par coup)
        public string Nom => EstSansPendule ? "Sans pendule"
            : this == Fide ? "90 + 30 min (+30 s) FIDE"
            : ADeuxPeriodes ? $"{TempsInitial.TotalMinutes:0} min/{CoupsControle} coups + {TempsAjoute.TotalMinutes:0} min" + TexteIncrement(" (+{0:0} s)")
            : $"{TempsInitial.TotalMinutes:0} min" + (Increment > TimeSpan.Zero ? TexteIncrement(" + {0:0} s") : " KO") + (EstOfficielle ? " FIDE" : "");

        // Balise PGN [TimeControl] et clé du .ini : "300+3" (secondes + bonus), "-" sans pendule ;
        // deux périodes : "40/5400+30:1800+30" (40 coups en 5400 s, puis 1800 s pour la suite, bonus de 30 s par coup)
        public string TimeControl
        {
            get
            {
                if (EstSansPendule)
                    return "-";
                string periode = $"{TempsInitial.TotalSeconds:0}" + TexteIncrement("+{0:0}");
                return ADeuxPeriodes ? $"{CoupsControle}/{periode}:{TempsAjoute.TotalSeconds:0}" + TexteIncrement("+{0:0}") : periode;
            }
        }

        public static Cadence Lire(string texte)
        {   // "300+3", "600", "40/5400+30:1800+30" ou "-" ; une valeur illisible donne "Sans pendule".
            // (Une seule période ajoutée : "40/7200" se lit comme 2 h pour 40 coups puis 2 h jusqu'à la fin)
            string[] periodes = (texte ?? "").Trim().Split(':');
            if (periodes.Length > 2)
                return SansPendule;
            int coupsControle = 0;
            string premiere = periodes[0];
            if (premiere.Contains('/'))
            {
                string[] coupsEtTemps = premiere.Split('/');
                if (coupsEtTemps.Length != 2 || !int.TryParse(coupsEtTemps[0], out coupsControle) || coupsControle <= 0)
                    return SansPendule;
                premiere = coupsEtTemps[1];
            }
            if (!LirePeriode(premiere, out int secondes, out int increment))
                return SansPendule;
            if (coupsControle == 0)
                return periodes.Length == 1 ? new(TimeSpan.FromSeconds(secondes), TimeSpan.FromSeconds(increment)) : SansPendule;
            int ajout = secondes;
            if (periodes.Length == 2 && !LirePeriode(periodes[1], out ajout, out _))
                return SansPendule;
            return new(TimeSpan.FromSeconds(secondes), TimeSpan.FromSeconds(increment), coupsControle, TimeSpan.FromSeconds(ajout));
        }
        private static bool LirePeriode(string texte, out int secondes, out int increment)
        {   // "5400+30" ou "1800" : temps en secondes et bonus par coup
            secondes = increment = 0;
            string[] parties = texte.Split('+');
            return parties.Length is 1 or 2 && int.TryParse(parties[0], out secondes) && secondes > 0
                && (parties.Length == 1 || (int.TryParse(parties[1], out increment) && increment >= 0));
        }

        public override string ToString() => Nom;     // texte affiché dans une liste de choix
    }

    public class Pendule
    {
        private readonly Func<TimeSpan> _maintenant;
        private TimeSpan _restantBlancs, _restantNoirs;     // temps restant au début du décompte en cours
        private TimeSpan _debutDecompte;                    // heure du début du décompte en cours

        public Cadence Cadence { get; }
        public ColorPiece? CampQuiDecompte { get; private set; }   // null : arrêtée (pas encore démarrée, ou partie finie)
        public bool EnPause { get; private set; }                  // ex : analyse demandée pendant la partie
        public bool Tourne => CampQuiDecompte != null && !EnPause;

        public Pendule(Cadence cadence, Func<TimeSpan> maintenant)
        {
            Cadence = cadence;
            _maintenant = maintenant;
            _restantBlancs = _restantNoirs = cadence.TempsInitial;
        }

        public TimeSpan TempsRestant(ColorPiece camp)
        {   // Temps restant du camp à cet instant (jamais négatif)
            TimeSpan restant = camp == ColorPiece.Blanc ? _restantBlancs : _restantNoirs;
            if (Tourne && camp == CampQuiDecompte)
                restant -= _maintenant() - _debutDecompte;
            return restant < TimeSpan.Zero ? TimeSpan.Zero : restant;
        }

        public ColorPiece? TempsEcoule() =>
            // Le camp qui décompte n'a plus de temps (son drapeau est tombé), sinon null
            CampQuiDecompte is ColorPiece camp && TempsRestant(camp) <= TimeSpan.Zero ? camp : null;

        public void Demarrer(ColorPiece auTrait)
        {   // Le camp au trait commence à décompter
            CampQuiDecompte = auTrait;
            EnPause = false;
            _debutDecompte = _maintenant();
        }

        public bool CoupJoue(int numeroDuCoup = 0)
        {   // Le camp qui décomptait a joué : son temps est figé, il reçoit le bonus, et c'est à l'adversaire de décompter.
            // numeroDuCoup : numéro du coup qui vient d'être joué ; au coup n° CoupsControle d'une cadence à deux périodes,
            // le camp reçoit aussi le temps de la seconde période (ex : FIDE, +30 min au 40e coup).
            // Renvoie false (et rien ne change) si la pendule est arrêtée ou si le temps du camp était déjà écoulé
            if (CampQuiDecompte is not ColorPiece camp || TempsEcoule() != null)
                return false;
            Fige();
            TimeSpan ajout = Cadence.Increment + (Cadence.ADeuxPeriodes && numeroDuCoup == Cadence.CoupsControle ? Cadence.TempsAjoute : TimeSpan.Zero);
            if (camp == ColorPiece.Blanc)
                _restantBlancs += ajout;
            else
                _restantNoirs += ajout;
            CampQuiDecompte = Adversaire(camp);
            return true;
        }

        public void Pause()
        {
            if (!Tourne)
                return;
            Fige();
            EnPause = true;
        }

        public void Reprendre()
        {
            if (!EnPause)
                return;
            EnPause = false;
            _debutDecompte = _maintenant();
        }

        public void Arreter()
        {   // Fin de partie : les temps restent affichés tels quels
            Fige();
            CampQuiDecompte = null;
            EnPause = false;
        }

        public void Restaurer(TimeSpan blancs, TimeSpan noirs, ColorPiece? auTrait)
        {   // Retour arrière : temps d'avant le coup annulé ; le camp au trait décompte (null : pendule arrêtée)
            _restantBlancs = blancs;
            _restantNoirs = noirs;
            CampQuiDecompte = auTrait;
            EnPause = false;
            _debutDecompte = _maintenant();
        }

        public void NoteTemps(Coup coup)
        {   // Le coup vient d'être joué (et la pendule de changer de camp) : on y note les temps restants des deux camps
            coup.TempsBlancs = TempsRestant(ColorPiece.Blanc);
            coup.TempsNoirs = TempsRestant(ColorPiece.Noir);
        }

        public void RestaurerDepuis(System.Collections.Generic.IReadOnlyList<Coup> coups, ColorPiece auTrait)
        {   // Retour arrière ou "Reprendre ici" : la pendule reprend les temps notés après le dernier coup restant.
            // Aucun coup joué (ou coups joués sans pendule) : temps complets, et elle ne repart qu'au prochain coup
            Coup? dernier = null;
            for (int i = coups.Count - 1; i >= 0 && dernier == null; i--)
                if (!coups[i].EstPositionDeDepart)
                    dernier = coups[i];
            if (dernier?.TempsBlancs is TimeSpan blancs && dernier.TempsNoirs is TimeSpan noirs)
                Restaurer(blancs, noirs, auTrait);
            else
                Restaurer(Cadence.TempsInitial, Cadence.TempsInitial, null);
        }

        private void Fige()
        {   // Le temps écoulé depuis le début du décompte est retiré au camp qui décompte
            if (Tourne && CampQuiDecompte is ColorPiece camp)
            {
                TimeSpan restant = TempsRestant(camp);
                if (camp == ColorPiece.Blanc)
                    _restantBlancs = restant;
                else
                    _restantNoirs = restant;
            }
            _debutDecompte = _maintenant();
        }

        public static string Texte(TimeSpan temps)
        {   // Affichage : "1:02:03", "4:57", et les dixièmes sous 20 s ("0:09.4") ; secondes tronquées, comme une pendule
            if (temps < TimeSpan.Zero)
                temps = TimeSpan.Zero;
            if (temps >= TimeSpan.FromHours(1))
                return $"{(int)temps.TotalHours}:{temps.Minutes:00}:{temps.Seconds:00}";
            if (temps < TimeSpan.FromSeconds(20))
                return $"{temps.Minutes}:{temps.Seconds:00}.{temps.Milliseconds / 100}";
            return $"{temps.Minutes}:{temps.Seconds:00}";
        }
    }
}
