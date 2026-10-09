// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// EchiquierPrincipal (fichier partiel) : Analyse de partie : boutons, progression, bilan, coup critique (logique : AnalysePartie.cs)

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
        // ═══ Analyse de partie (voir AnalysePartie.cs) : bouton, barre et bilan sous la feuille ═══
        // Chaque position est analysée DureeAnalyseParPosition ms par l'analyse de position habituelle (_pilote.DemanderAnalyse),
        // enchaînée à chaque réponse du moteur. Pendant l'analyse, l'échiquier est bloqué et la pendule en pause ; toute action qui
        // change la partie l'interrompt (AbandonneReflexion) en gardant ce qui est analysé. On peut parcourir la partie pendant ce temps.
        // Puis 2e passage : les positions avant et après chaque coup douteux sont revues DureeApprofondissement ms (un ?? à 3 s
        // peut n'être qu'une illusion d'une recherche trop courte). Le bilan donne ensuite le coup critique (clic : il est affiché)
        private const int DureeAnalyseParPosition = 3000;   // 3 s par position (choix de Bruno)
        private const int DureeApprofondissement = 10000;   // 10 s par position revue
        private AnalyseDePartie _analyseDePartie;           // analyse en cours (null : aucune)
        private int _positionEnAnalyse = -1;                // position demandée au moteur (index dans _analyseDePartie.Positions)
        private int? _coupCritique;                         // index dans ListeCoups du coup critique de la dernière analyse (clic sur le bilan)
        private int? _multiPvAvantAnalyse;                  // nombre de variantes à rétablir à la fin de l'analyse

        private void BoutonAnalysePartie_Click(object sender, EventArgs e)
        {   // "Analyse rapide" (3 s par position), ou "Interrompre" si une analyse est en cours (ce bouton occupe alors toute la largeur)
            if (_analyseDePartie != null)
                TermineAnalyseDePartie(interrompue: true);
            else
                LanceAnalyseDePartie(complete: false);
        }
        private void BoutonAnalyseComplete_Click(object sender, EventArgs e) =>
            // "Analyse complète" : la rapide, puis les coups douteux revus 10 s (masqué pendant une analyse)
            LanceAnalyseDePartie(complete: true);
        private void MetAJourInfobullesAnalyse(object sender, EventArgs e)
        {   // Au survol des boutons : ce que fait chaque analyse et sa durée pour la partie en cours
            int positions = LogiqueMouvements.ListeCoups.Count(c => !c.EstPositionDeDepart) + 1;
            TimeSpan rapide = TimeSpan.FromMilliseconds((double)positions * DureeAnalyseParPosition);
            _infobulleBilan.SetToolTip(BoutonAnalysePartie,
                $"Analyse rapide : {DureeAnalyseParPosition / 1000} s par position, puis bilan et coup critique.\n" +
                $"Environ {TexteDuree(rapide)} pour cette partie ({positions} positions).");
            _infobulleBilan.SetToolTip(BoutonAnalyseComplete,
                $"Analyse complète : l'analyse rapide, puis les positions avant et après chaque coup douteux\n" +
                $"(?!, ?, ??) revues {DureeApprofondissement / 1000} s pour confirmer le jugement.\n" +
                $"Environ {TexteDuree(rapide)}, plus {2 * DureeApprofondissement / 1000} s par coup douteux.");
        }
        private void LanceAnalyseDePartie(bool complete)
        {
            if (!LogiqueMouvements.ListeCoups.Any(c => !c.EstPositionDeDepart))
            {
                KryptonMessageBox.Show("Aucun coup à analyser.", "Analyse de la partie", KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            if (_pilote.Demande == TypeDemande.CoupDePartie)
            {
                KryptonMessageBox.Show(_nomMoteur + " réfléchit à son coup : attendez qu'il ait joué.", "Analyse de la partie",
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            AbandonneReflexion();       // une analyse de position en cours est remplacée
            _pendule?.Pause();          // le temps ne compte pas pendant l'analyse (reprise dans MinuteriePendule_Tick à la fin)
            _analyseDePartie = new AnalyseDePartie(LogiqueMouvements.ListeCoups, approfondir: complete);
            // Au moins 2 variantes pendant l'analyse : la 2e dit si le meilleur coup était le seul bon ("!") ; réglage rétabli à la fin
            _multiPvAvantAnalyse = MoteurUci.NombreLignesPV;
            if (MoteurUci.NombreLignesPV < 2)
                MoteurUci.DefinitMultiPV(2);
            BoutonAnalyseComplete.Visible = false;      // un seul bouton "Interrompre (k/N)", sur toute la largeur
            BoutonAnalysePartie.Width = BoutonAnalyseComplete.Right - BoutonAnalysePartie.Left;
            BarreAnalysePartie.Maximum = Math.Max(1, _analyseDePartie.NombreAAnalyser);
            BarreAnalysePartie.Value = 0;
            BarreAnalysePartie.Visible = true;
            EffaceBilan();
            InformationsPartie.Text = "Analyse de la partie...";
            MetAJourCommandes();        // échiquier bloqué
            AnalysePositionSuivante();
        }
        private void AnalysePositionSuivante()
        {
            if (_analyseDePartie.PositionSuivante is not int index)
            {
                TermineAnalyseDePartie(interrompue: false);
                return;
            }
            _positionEnAnalyse = index;
            AfficheCoupDeLaPartie(_analyseDePartie.IndexDansListeCoups(index));    // l'échiquier montre la position analysée
            bool approfondissement = _analyseDePartie.EnApprofondissement;
            int faites = approfondissement ? _analyseDePartie.NombreApprofondies
                : _analyseDePartie.Positions.Count(p => p.ChancesFinDePartie == null && p.Analysee);
            int total = approfondissement ? _analyseDePartie.NombreAApprofondir : _analyseDePartie.NombreAAnalyser;
            BarreAnalysePartie.Maximum = Math.Max(1, total);
            BarreAnalysePartie.Value = Math.Min(BarreAnalysePartie.Maximum, faites);
            BoutonAnalysePartie.Values.Text = approfondissement ? $"Interrompre (vérif. {faites + 1}/{total})" : $"Interrompre ({faites + 1}/{total})";
            InformationsPartie.Text = approfondissement ? $"Vérification des coups douteux ({DureeApprofondissement / 1000} s)..." : "Analyse de la partie...";
            _pilote.DemanderAnalyse(LogiqueMouvements.PositionDepuisFen(_analyseDePartie.Positions[index].Fen),
                                    approfondissement ? DureeApprofondissement : DureeAnalyseParPosition);
        }
        private void PositionAnalyseeParLeMoteur(LigneAnalyse meilleure)
        {   // Réponse du moteur pour la position demandée (null : aucun score) : on l'enregistre et on passe à la suivante
            _analyseDePartie.Enregistre(_positionEnAnalyse, meilleure, meilleure != null ? _pilote.Lignes.Seconde : null);
            // Annotations au fur et à mesure (choix de Bruno) : l'analyse allant de la fin vers le début, la position d'après est déjà
            // analysée et le coup joué dans celle-ci peut être jugé tout de suite ; la feuille est redessinée avec la position suivante
            _analyseDePartie.AppliqueAuxCoups(LogiqueMouvements.ListeCoups);
            AnalysePositionSuivante();
        }
        private static string TexteBilan(string camp, BilanCamp bilan) =>
            // Ex : "Blancs — précision 87 %" puis "   1 imprécision, 0 erreur, 2 gaffes" (explications dans l'infobulle du bilan)
            $"{camp} — précision {bilan.Precision} %\n   {Pluriel(bilan.Imprecisions, "imprécision")}, {Pluriel(bilan.Erreurs, "erreur")}, {Pluriel(bilan.Gaffes, "gaffe")}";
        private static string Pluriel(int nombre, string mot) => $"{nombre} {mot}{(nombre > 1 ? "s" : "")}";
        private readonly ToolTip _infobulleBilan = new() { AutoPopDelay = 20000 };
        private static string TexteCoupCritique(Coup coup, JugementCoup critique) =>
            // Ex : "Coup critique (clic) :" puis "   25... c5??   score -0.23 → 2.73" (évaluation avant, avec le meilleur coup, et après)
            "Coup critique (clic) :\n   " + coup.PgnFrNumerote + coup.Annotation
            + (critique.EvaluationMeilleur is Evaluation avant && critique.EvaluationApres is Evaluation apres ? $"   score {avant.Texte} → {apres.Texte}" : "");
        private void EffaceBilan()
        {
            BilanAnalyse.Text = "";
            _coupCritique = null;
            BilanAnalyse.Cursor = Cursors.Default;
        }
        private void BilanAnalyse_Click(object sender, EventArgs e)
        {   // Clic sur le bilan : l'échiquier montre le coup critique (avec son analyse et ses flèches)
            if (_coupCritique is int index && _analyseDePartie == null && index < LogiqueMouvements.ListeCoups.Count)
                AfficheCoupDeLaPartie(index);
        }

        private void TermineAnalyseDePartie(bool interrompue)
        {   // Fin (ou interruption) : les résultats vont dans les coups (annotations proposées, plus pâles), puis le bilan
            AnalyseDePartie analyse = _analyseDePartie;
            if (analyse == null)
                return;
            _analyseDePartie = null;        // avant AbandonneReflexion, qui sinon reviendrait ici
            _positionEnAnalyse = -1;
            AbandonneReflexion();           // la position en cours d'analyse (interruption) : sa réponse sera ignorée
            if (_multiPvAvantAnalyse is int multiPv && multiPv != MoteurUci.NombreLignesPV)
                MoteurUci.DefinitMultiPV(multiPv);      // nombre de variantes choisi par l'utilisateur, forcé à 2 pendant l'analyse
            _multiPvAvantAnalyse = null;
            analyse.AppliqueAuxCoups(LogiqueMouvements.ListeCoups);
            BilanCamp blancs = analyse.Bilan(ColorPiece.Blanc), noirs = analyse.Bilan(ColorPiece.Noir);
            BilanAnalyse.Text = TexteBilan("Blancs", blancs) + "\n" + TexteBilan("Noirs", noirs);
            if (analyse.CoupCritique() is JugementCoup critique)
            {   // en tête du bilan : le moment où la partie a basculé
                _coupCritique = critique.IndexCoup;
                BilanAnalyse.Text = TexteCoupCritique(LogiqueMouvements.ListeCoups[critique.IndexCoup], critique) + "\n" + BilanAnalyse.Text;
                BilanAnalyse.Cursor = Cursors.Hand;
            }
            BarreAnalysePartie.Visible = false;
            BoutonAnalysePartie.Values.Text = "Analyse rapide";
            BoutonAnalysePartie.Width = BoutonAnalyseComplete.Left - 4 - BoutonAnalysePartie.Left;
            BoutonAnalyseComplete.Visible = true;
            RetourPositionCourante();       // l'échiquier, qui suivait l'analyse, revient à la partie
            InformationsPartie.Text = interrompue ? "Analyse interrompue" : "Analyse de la partie terminée";
            MetAJourCommandes();
        }
    }
}
