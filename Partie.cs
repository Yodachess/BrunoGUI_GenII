// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Déroulement de la partie, sans interface graphique (testé dans Tests/Program.cs)
//  └─ Classe "Partie" : mode de la partie, qui joue quel camp (humain ou moteur), retour arrière
//              ├─ "Commencer", "CommencerDepuisPosition"
//              ├─ "Terminer", "Reprendre", "PasserEnLectureSeule"
//              ├─ "MoteurPrendLeTrait"
//              ├─ "AnnulerDernierCoup"
//              └─ "ReprendreDepuis"    "Reprendre la partie d'ici" (depuis une position du parcours)
// La position et la liste des coups restent dans LogiqueMouvements (PositionActuelle, ListeCoups) ;
// l'interface (EchiquierPrincipal) lit l'état de la partie pour afficher et décider des commandes actives.

using static BrunoGUI_GenII.LogiqueMouvements;

#nullable enable

namespace BrunoGUI_GenII
{
    public enum ModePartie
    {
        AucunePartie,   // au lancement, avant le choix d'une partie
        EnCours,        // partie en cours (contre le moteur, entre humains, ou depuis une position FEN)
        Terminee,       // résultat déclaré (mat, pat, nulle, abandon) : le retour arrière permet de reprendre la partie
        LectureSeule    // partie PGN chargée : parcours et analyse seulement
    }

    public enum Joueur { Humain, Moteur }

    public class Partie
    {
        public ModePartie Mode { get; private set; } = ModePartie.AucunePartie;
        public Joueur Blancs { get; private set; } = Joueur.Humain;     // par défaut : l'humain a les Blancs,
        public Joueur Noirs { get; private set; } = Joueur.Moteur;      // le moteur les Noirs
        public bool DepuisPosition { get; private set; }                // la partie a commencé depuis une position FEN
        public bool RejeuPgn { get; set; }                              // rejeu d'une partie PGN chargée (pas de nulle automatique)

        public bool EnCours => Mode == ModePartie.EnCours;
        public bool EntreHumains => Blancs == Joueur.Humain && Noirs == Joueur.Humain;
        public Joueur JoueurDe(ColorPiece couleur) => couleur == ColorPiece.Blanc ? Blancs : Noirs;
        public Joueur JoueurAuTrait => JoueurDe(QuiJoue);
        public bool MoteurAuTrait => EnCours && JoueurAuTrait == Joueur.Moteur;
        public bool HumainAuTrait => EnCours && JoueurAuTrait == Joueur.Humain;

        public void Commencer(Joueur blancs, Joueur noirs, bool depuisPosition = false)
        {   // Nouvelle partie (la position et la liste des coups sont mises en place par l'appelant) ;
            // depuisPosition : elle commence à une position FEN (ex : partie PGN avec une balise FEN)
            Blancs = blancs;
            Noirs = noirs;
            DepuisPosition = depuisPosition;
            Reprendre();
        }
        public void CommencerDepuisPosition()
        {   // Partie depuis une position FEN : l'humain joue le camp au trait, le moteur lui répond
            HumainPrendLeTrait();
            DepuisPosition = true;
            Reprendre();
        }
        public void Terminer() => Mode = ModePartie.Terminee;           // mat, pat, nulle ou abandon
        public void Reprendre()
        {   // La partie est (de nouveau) en cours
            Mode = ModePartie.EnCours;
        }
        public void PasserEnLectureSeule() => Mode = ModePartie.LectureSeule;
        public void MoteurPrendLeTrait()
        {   // "Ordinateur joue" : le moteur prend le camp au trait, l'humain l'autre camp
            Blancs = QuiJoue == ColorPiece.Blanc ? Joueur.Moteur : Joueur.Humain;
            Noirs = QuiJoue == ColorPiece.Noir ? Joueur.Moteur : Joueur.Humain;
        }
        private void HumainPrendLeTrait()
        {   // L'humain joue le camp au trait, le moteur l'autre camp
            Blancs = QuiJoue == ColorPiece.Blanc ? Joueur.Humain : Joueur.Moteur;
            Noirs = QuiJoue == ColorPiece.Noir ? Joueur.Humain : Joueur.Moteur;
        }

        public int ReprendreDepuis(int index)
        {   // "Reprendre la partie d'ici" : supprime les coups après le coup n° index de ListeCoups (-1 : position initiale)
            // et reprend la partie depuis cette position (une partie terminée reprend ; une partie PGN en lecture seule devient
            // jouable, l'humain ayant le camp au trait). Renvoie le nombre de demi-coups supprimés (0 : rien n'a changé)
            if (Mode == ModePartie.AucunePartie)
                return 0;
            int supprimes = 0;
            while (ListeCoups.Count - 1 > index && RetireDernierCoup())     // jamais la position de départ d'une partie FEN
                supprimes++;
            bool etaitLectureSeule = Mode == ModePartie.LectureSeule;
            if (supprimes == 0 && !etaitLectureSeule)
                return 0;       // (une partie PGN reprise à sa position finale devient jouable sans rien supprimer)
            Reprendre();
            RejeuPgn = false;
            MiseenplaceFen(ListeCoupsFen.Count == 0 ? FenDepart : ListeCoupsFen[^1]);
            EchecetMat = false;             // absent de la FEN : la position rétablie n'est pas un mat
            Echec = CampAuTraitEnEchec();
            if (etaitLectureSeule)
                HumainPrendLeTrait();
            return supprimes;
        }

        public bool AnnulerDernierCoup()
        {   // Retire le dernier demi-coup et rétablit la position d'avant (trait, roques, en passant, 50 coups : tout vient de la FEN).
            // Une partie terminée reprend (le coup annulé a pu mater). Renvoie false s'il n'y a aucun coup à annuler
            if (Mode != ModePartie.EnCours && Mode != ModePartie.Terminee)
                return false;
            if (!RetireDernierCoup())       // jamais la position de départ d'une partie commencée depuis un FEN
                return false;
            if (Mode == ModePartie.Terminee)
                Reprendre();
            MiseenplaceFen(ListeCoupsFen.Count == 0 ? FenDepart : ListeCoupsFen[^1]);
            EchecetMat = false;             // absent de la FEN : la position rétablie n'est pas un mat
            Echec = CampAuTraitEnEchec();
            return true;
        }
    }
}
