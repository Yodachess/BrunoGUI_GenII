// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Numérotation des demandes au moteur UCI (utilisée par MoteurUci)
// └─ Classe "SuiviDemandesMoteur"
//              ├─ "DemandeEnvoyee"     Un "go" vient d'être envoyé : il reçoit le numéro suivant
//              ├─ "NumeroEnCours"      Numéro de la demande à laquelle répond la prochaine ligne reçue du moteur
//              ├─ "ReponseRecue"       Un "bestmove" vient d'arriver : numéro de la demande à laquelle il répond
//              ├─ "Abandonner"         Les demandes en attente deviennent périmées (leur réponse sera ignorée)
//              └─ "EstAbandonnee"
// Le protocole UCI ne numérote pas les réponses, mais il garantit exactement un "bestmove" par "go", dans l'ordre d'envoi
// (même après "stop") : le n-ième "bestmove" reçu répond donc au n-ième "go" envoyé.
// Appelée depuis deux threads (interface et lecture du moteur) : toutes les opérations sont protégées par un verrou.

#nullable enable

namespace BrunoGUI_GenII
{
    public class SuiviDemandesMoteur
    {
        private readonly object _verrou = new();
        private int _envoyees;          // nombre de "go" envoyés
        private int _recues;            // nombre de "bestmove" reçus
        private int _abandonneesJusqua; // les demandes de numéro <= cette valeur sont abandonnées

        public void Reinitialiser()
        {   // Nouveau processus moteur : on repart de zéro
            lock (_verrou) { _envoyees = _recues = _abandonneesJusqua = 0; }
        }
        public int DemandeEnvoyee()
        {
            lock (_verrou) { return ++_envoyees; }
        }
        public int NumeroEnCours
        {   // Demande à laquelle répondent les lignes reçues en ce moment ("info", puis le "bestmove")
            get { lock (_verrou) { return _recues + 1; } }
        }
        public int ReponseRecue()
        {
            lock (_verrou) { return ++_recues; }
        }
        public bool EnAttente
        {   // Une demande n'a pas encore reçu son "bestmove" (le moteur réfléchit)
            get { lock (_verrou) { return _recues < _envoyees; } }
        }
        public bool EnAttenteNonAbandonnee
        {   // Le moteur réfléchit à une demande dont la réponse sera prise en compte
            get { lock (_verrou) { return _recues < _envoyees && _abandonneesJusqua < _envoyees; } }
        }
        public bool Abandonner()
        {   // Rend périmées toutes les demandes en attente ; retourne true s'il y en avait (il faut alors envoyer "stop")
            lock (_verrou)
            {
                if (_recues >= _envoyees || _abandonneesJusqua >= _envoyees)
                    return false;
                _abandonneesJusqua = _envoyees;
                return true;
            }
        }
        public bool EstAbandonnee(int numero)
        {
            lock (_verrou) { return numero <= _abandonneesJusqua; }
        }
    }
}
