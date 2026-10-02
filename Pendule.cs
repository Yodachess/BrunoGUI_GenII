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
//              └─ "Restaurer"  (retour arrière : temps d'avant le coup annulé)
// L'heure est donnée par une fonction (un chronomètre dans l'application, un temps simulé dans les tests) :
// la pendule calcule le temps écoulé, elle ne compte pas les tics d'une minuterie.

using System;
using static BrunoGUI_GenII.LogiqueMouvements;

namespace BrunoGUI_GenII
{
    public readonly record struct Cadence(TimeSpan TempsInitial, TimeSpan Increment)
    {
        public static readonly Cadence SansPendule = new(TimeSpan.Zero, TimeSpan.Zero);
        public static Cadence Minutes(int minutes, int incrementSecondes = 0) =>
            new(TimeSpan.FromMinutes(minutes), TimeSpan.FromSeconds(incrementSecondes));

        // Cadences proposées dans l'interface ("Sans pendule" en premier : le moteur a un temps fixe par coup)
        public static readonly Cadence[] Proposees =
            [SansPendule, Minutes(3, 2), Minutes(5, 3), Minutes(10, 5), Minutes(15, 10), Minutes(30)];

        public bool EstSansPendule => TempsInitial <= TimeSpan.Zero;

        public string Nom => EstSansPendule ? "Sans pendule"
            : $"{TempsInitial.TotalMinutes:0} min" + (Increment > TimeSpan.Zero ? $" + {Increment.TotalSeconds:0} s" : "");

        // Balise PGN [TimeControl] et clé du .ini : "300+3" (secondes + bonus), "-" sans pendule
        public string TimeControl => EstSansPendule ? "-"
            : $"{TempsInitial.TotalSeconds:0}" + (Increment > TimeSpan.Zero ? $"+{Increment.TotalSeconds:0}" : "");

        public static Cadence Lire(string texte)
        {   // "300+3", "600" ou "-" ; une valeur illisible donne "Sans pendule"
            string[] parties = (texte ?? "").Trim().Split('+');
            if (parties.Length is < 1 or > 2 || !int.TryParse(parties[0], out int secondes) || secondes <= 0)
                return SansPendule;
            int increment = 0;
            if (parties.Length == 2 && (!int.TryParse(parties[1], out increment) || increment < 0))
                return SansPendule;
            return new(TimeSpan.FromSeconds(secondes), TimeSpan.FromSeconds(increment));
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

        public bool CoupJoue()
        {   // Le camp qui décomptait a joué : son temps est figé, il reçoit le bonus, et c'est à l'adversaire de décompter.
            // Renvoie false (et rien ne change) si la pendule est arrêtée ou si le temps du camp était déjà écoulé
            if (CampQuiDecompte is not ColorPiece camp || TempsEcoule() != null)
                return false;
            Fige();
            if (camp == ColorPiece.Blanc)
                _restantBlancs += Cadence.Increment;
            else
                _restantNoirs += Cadence.Increment;
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
