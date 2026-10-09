// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// EchiquierPrincipal (fichier partiel) : Pendule : cadence, pause, chute du drapeau, affichage des temps (voir Pendule.cs)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.GestionPartiePgn;
using static BrunoGUI_GenII.LogiqueMouvements;
using static BrunoGUI_GenII.Parametres;

namespace BrunoGUI_GenII
{
    public partial class EchiquierPrincipal
    {
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Pendule (voir Pendule.cs)
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private Cadence _cadence = Cadence.SansPendule;     // cadence choisie : elle vaut pour la PROCHAINE nouvelle partie
        private Pendule _pendule;                           // pendule de la partie en cours (null : sans pendule, temps fixe par coup)
        private readonly Stopwatch _chrono = Stopwatch.StartNew();      // heure de la pendule (précise, indépendante des tics)
        private readonly System.Windows.Forms.Timer _minuteriePendule = new() { Interval = 100 };   // affichage et chute du drapeau

        private bool _pauseJoueur;      // pause demandée par un clic sur une pendule (à distinguer de la pause pendant une analyse)

        private void Pendule_Click(object sender, EventArgs e)
        {   // Un clic sur l'une des deux pendules met la partie en pause, un autre la reprend (comme le bouton d'une vraie pendule).
            // Sans pendule, ou hors d'une partie en cours, le clic ne fait rien
            if (_pendule == null || !_partie.EnCours)
                return;
            if (_pauseJoueur)
            {
                FinPause();
                InformationsPartie.Text = "Partie reprise";
                if (_partie.MoteurAuTrait && _pilote.Demande == TypeDemande.Aucune)
                    JeuMoteurAvecBibliothèque(LogiqueMouvements.RetourneChaineFenActuel());     // sa réflexion avait été interrompue
            }
            else if (_pendule.Tourne)
            {
                AbandonneReflexion();   // le moteur s'arrête de réfléchir (sinon il jouerait pendant la pause) ; il recommencera à la reprise
                _pendule.Pause();
                _pauseJoueur = true;
                InformationsPartie.Text = "En pause (clic pour reprendre)";     // le cadre vert est court : ~30 caractères
                MetAJourCommandes();
            }
            AffichePendules();
        }
        private void FinPause()
        {   // Fin de la pause du joueur (reprise, ou partie qui change : nouvelle partie, résultat, retour arrière...)
            if (!_pauseJoueur)
                return;
            _pauseJoueur = false;
            _pendule?.Reprendre();
            MetAJourCommandes();
        }

        private void ChoisitCadence(Cadence cadence)
        {   // Sélectionne la cadence dans la liste (ajoutée si elle n'y est pas, ex : valeur écrite à la main dans le .ini)
            _cadence = cadence;     // avant la sélection : pas de message "à la prochaine partie"
            maNouvellePartieForceModule.ChoixCadence = cadence;
            if (!ListePendule.Items.Contains(cadence))
                ListePendule.Items.Add(cadence);
            ListePendule.SelectedItem = cadence;
        }
        private void ListePendule_SelectedIndexChanged(object sender, EventArgs e)
        {   // Nouvelle cadence : pour la partie suivante (la partie en cours garde la sienne)
            if (ListePendule.SelectedItem is not Cadence cadence || cadence == _cadence)
                return;
            _cadence = cadence;
            maNouvellePartieForceModule.ChoixCadence = cadence;
            InformationsPartie.Text = "Pendule " + cadence.Nom + " : à la prochaine partie";
        }
        private void NouvellePendule()
        {   // Début d'une partie : pendule de la cadence choisie, temps complets affichés. Elle ne démarre qu'au premier coup
            // (voir CoupJoue), comme sur les serveurs : on peut regarder la position avant que le temps file
            _pauseJoueur = false;
            _pendule = _cadence.EstSansPendule ? null : new Pendule(_cadence, () => _chrono.Elapsed);
            PartieEnCours.TimeControl = _pendule?.Cadence.TimeControl ?? "";     // balise PGN [TimeControl] (absente sans pendule)
            if (_pendule != null)
                _minuteriePendule.Start();
            else
                _minuteriePendule.Stop();
            AffichePendules();
        }
        private void SupprimePendule()
        {   // Partie sans pendule (ex : partie PGN chargée)
            _pauseJoueur = false;
            _pendule = null;
            _minuteriePendule.Stop();
            AffichePendules();
        }
        private void MinuteriePendule_Tick(object sender, EventArgs e)
        {   // Tous les dixièmes de seconde : reprise après une analyse, chute du drapeau, affichage
            if (_pendule == null)
                return;
            if (_pendule.EnPause && !_pilote.AnalyseEnCours && !_pauseJoueur && _analyseDePartie == null)
            {   // L'analyse est finie (ou abandonnée) : la pendule repart ; si le moteur devait jouer, l'analyse a interrompu
                // sa réflexion : on lui redemande son coup (sinon son temps s'écoulerait sans qu'il réfléchisse)
                _pendule.Reprendre();
                if (_partie.MoteurAuTrait && _pilote.Demande == TypeDemande.Aucune)
                    JeuMoteurAvecBibliothèque(LogiqueMouvements.RetourneChaineFenActuel());
            }
            // (pendant le choix d'une promotion, le coup n'est pas fini : la chute du drapeau est traitée juste après, par CoupJoue)
            if (_partie.EnCours && !GroupPromo.Visible && _pendule.TempsEcoule() is ColorPiece campSansTemps)
                PerteAuTemps(campSansTemps);
            AffichePendules();
        }
        private void PerteAuTemps(ColorPiece campSansTemps)
        {   // Le temps du camp est écoulé : il perd, sauf si l'adversaire n'a pas de quoi mater (nulle)
            ColorPiece adversaire = Adversaire(campSansTemps);
            string message = "Temps écoulé pour les " + NomCamp(campSansTemps);
            if (LogiqueMouvements.PeutMater(adversaire))
                GestionResultat(adversaire == ColorPiece.Blanc ? "1-0" : "0-1", " Gain " + NomCouleur(adversaire) + " (temps)");
            else
                GestionResultat("1/2-1/2", "Nulle (temps écoulé, matériel insuffisant)");
            InformationPourJoueur.Text = VarianteMoteurCourante.Text = message;
        }
        private void AffichePendules()
        {   // Les deux pendules, toujours affichées : "-:--" sans pendule ; le camp qui décompte sur fond vert, en rouge sous 10 secondes
            PenduleBlanc.Cursor = PenduleNoir.Cursor = _pendule != null ? Cursors.Hand : Cursors.Default;   // cliquables : pause / reprise
            if (_pendule == null)
            {   // Pas de pendule en cours : temps notés dans la partie (ex : PGN chargé avec des [%clk]) à la position affichée
                var (tempsBlancs, tempsNoirs) = TempsDeLaPositionAffichee();
                PenduleBlanc.Text = tempsBlancs is TimeSpan blancs ? Pendule.Texte(blancs) : "-:--";
                PenduleNoir.Text = tempsNoirs is TimeSpan noirs ? Pendule.Texte(noirs) : "-:--";
                PenduleBlanc.BackColor = PenduleNoir.ForeColor = Color.White;
                PenduleNoir.BackColor = PenduleBlanc.ForeColor = Color.Black;
                return;
            }
            AffichePendule(PenduleBlanc, ColorPiece.Blanc, Color.White, Color.Black);
            AffichePendule(PenduleNoir, ColorPiece.Noir, Color.Black, Color.White);
        }
        private (TimeSpan? Blancs, TimeSpan? Noirs) TempsDeLaPositionAffichee()
        {   // Temps notés dans le coup de la position affichée (parcours) ou du dernier coup ; position de départ : temps initial
            // de la cadence (balise TimeControl). Rien si la partie n'a aucun temps noté
            List<Coup> coups = [.. LogiqueMouvements.ListeCoups];
            if (!coups.Any(c => c.TempsBlancs != null || c.TempsNoirs != null))
                return (null, null);
            int index = ParcoursEnCours ? _indexAffiche : coups.Count - 1;
            if (index >= 0 && index < coups.Count && !coups[index].EstPositionDeDepart)
                return (coups[index].TempsBlancs, coups[index].TempsNoirs);
            Cadence cadence = Cadence.Lire(PartieEnCours.TimeControl);
            return cadence.EstSansPendule ? (null, null) : (cadence.TempsInitial, cadence.TempsInitial);
        }
        private void AffichePendule(Label affichage, ColorPiece camp, Color fond, Color texte)
        {
            TimeSpan restant = _pendule.TempsRestant(camp);
            bool decompte = _pendule.Tourne && _pendule.CampQuiDecompte == camp;
            affichage.Text = Pendule.Texte(restant);    // ("1:30:00" : la pendule réduit sa police pour qu'il tienne, voir ReduitPourTenir)
            affichage.BackColor = _pauseJoueur ? Color.Silver : decompte ? Color.LightGreen : fond;     // gris : partie en pause
            affichage.ForeColor = restant < TimeSpan.FromSeconds(10) ? Color.Red : decompte ? Color.Black : texte;
        }
    }
}
