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
using static BrunoGUI_GenII.Langue;
using static BrunoGUI_GenII.LogiqueMouvements;
using static BrunoGUI_GenII.Parametres;

namespace BrunoGUI_GenII
{
    public partial class EchiquierPrincipal
    {
        // ═══ Analyse de partie (voir AnalysePartie.cs) : bouton, barre et bilan sous la feuille ═══
        // Chaque position est analysée DureeAnalyseParPosition ms (réglable, 10 s par défaut) par l'analyse de position habituelle (_pilote.DemanderAnalyse),
        // enchaînée à chaque réponse du moteur. Pendant l'analyse, l'échiquier est bloqué et la pendule en pause ; toute action qui
        // change la partie l'interrompt (AbandonneReflexion) en gardant ce qui est analysé. On peut parcourir la partie pendant ce temps.
        // Puis 2e passage : les positions avant et après chaque coup douteux sont revues DureeApprofondissement ms (30 s par défaut ; un ?? trouvé vite
        // peut n'être qu'une illusion d'une recherche trop courte). Le bilan donne ensuite le coup critique (clic : il est affiché)
        // Durées par position, en secondes (réglables par Options > Durées de l'analyse de partie, préférences
        // DureeAnalyseRapide et DureeAnalyseComplete ; 10 et 30 s par défaut, d'abord 3 et 10 s)
        private int _dureeAnalyseRapide = 10, _dureeAnalyseComplete = 30;
        private int DureeAnalyseParPosition => _dureeAnalyseRapide * 1000;      // en ms : 1er passage
        private int DureeApprofondissement => _dureeAnalyseComplete * 1000;     // en ms : 2e passage (analyse complète)
        private AnalyseDePartie? _analyseDePartie;           // analyse en cours (null : aucune)
        private int _positionEnAnalyse = -1;                // position demandée au moteur (index dans _analyseDePartie.Positions)
        private int? _coupCritique;                         // index dans ListeCoups du coup critique de la dernière analyse (clic sur le bilan)
        private int? _multiPvAvantAnalyse;                  // nombre de variantes à rétablir à la fin de l'analyse
        private FenetreAnalyse? _fenetreAnalyse;            // suivi de l'analyse (position, score, suite, temps restant) ; null : fermée
        private bool _analyseComplete;                      // analyse complète : la vérification des coups douteux suivra le 1er passage
        // 2e Stockfish (celui livré avec BrunoGUI, quel que soit le moteur choisi) : un seul bon coup était-il difficile à trouver ?
        // (voir MoteurAuxiliaire.cs et JugementCoups.ProfondeurDecouverte) ; lancé avec l'analyse, arrêté à la fin
        private MoteurAuxiliaire? _moteurAuxiliaire;

        // Noms des ouvertures (Ouvertures\ouvertures.tsv, table de Lichess) : lus à la première demande ; null : fichier absent ou illisible
        private CatalogueOuvertures? _catalogueOuvertures;
        private bool _catalogueOuverturesLu;

        private CatalogueOuvertures? CatalogueDesOuvertures()
        {
            if (!_catalogueOuverturesLu)
            {
                _catalogueOuverturesLu = true;
                try
                {
                    _catalogueOuvertures = CatalogueOuvertures.Lire(File.ReadAllText(Chemins.FichierOuvertures));
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {   // sans le fichier, l'analyse se fait sans le nom de l'ouverture
                    Journal.Erreur("Lecture de " + Chemins.FichierOuvertures, ex);
                }
            }
            return _catalogueOuvertures;
        }

        private void CompleteEco()
        {   // Balise [ECO] vide (partie jouée, ou PGN sans elle) : le code de l'ouverture de la partie, d'après la table de Lichess ;
            // celle d'un PGN chargé ou saisie dans "Entête PGN" est gardée
            if (string.IsNullOrWhiteSpace(PartieEnCours.ECO)
                && CatalogueDesOuvertures()?.DerniereConnue(LogiqueMouvements.ListeCoups) is (_, Ouverture ouverture))
                PartieEnCours.ECO = ouverture.Eco;
        }

        private void BoutonAnalysePartie_Click(object? sender, EventArgs e)
        {   // "Analyse rapide" (10 s par position par défaut), ou "Interrompre" si une analyse est en cours (ce bouton occupe alors toute la largeur)
            if (_analyseDePartie != null)
                TermineAnalyseDePartie(interrompue: true);
            else
                LanceAnalyseDePartie(complete: false);
        }
        private void BoutonAnalyseComplete_Click(object? sender, EventArgs e) =>
            // "Analyse complète" : la rapide, puis les coups douteux revus plus longtemps (masqué pendant une analyse)
            LanceAnalyseDePartie(complete: true);
        private void MetAJourInfobullesAnalyse(object? sender, EventArgs e)
        {   // Au survol des boutons : ce que fait chaque analyse et sa durée pour la partie en cours
            int positions = LogiqueMouvements.ListeCoups.Count(c => !c.EstPositionDeDepart) + 1;
            TimeSpan rapide = TimeSpan.FromMilliseconds((double)positions * DureeAnalyseParPosition);
            _infobulleBilan.SetToolTip(BoutonAnalysePartie,
                T("Analyse rapide : {0} s par position, puis bilan et coup critique.\nEnviron {1} pour cette partie ({2} positions).",
                  DureeAnalyseParPosition / 1000, TexteDuree(rapide), positions));
            _infobulleBilan.SetToolTip(BoutonAnalyseComplete,
                T("Analyse complète : l'analyse rapide, puis les positions avant et après chaque coup douteux\n(?!, ?, ??) revues {0} s pour confirmer le jugement.\nEnviron {1}, plus {2} s par coup douteux.",
                  DureeApprofondissement / 1000, TexteDuree(rapide), 2 * DureeApprofondissement / 1000));
        }
        private void LanceAnalyseDePartie(bool complete)
        {
            if (!LogiqueMouvements.ListeCoups.Any(c => !c.EstPositionDeDepart))
            {
                KryptonMessageBox.Show(T("Aucun coup à analyser."), T("Analyse de la partie"), KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            if (_pilote.Demande == TypeDemande.CoupDePartie)
            {
                KryptonMessageBox.Show(T("{0} réfléchit à son coup : attendez qu'il ait joué.", _nomMoteur), T("Analyse de la partie"),
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            AbandonneReflexion();       // une analyse de position en cours est remplacée
            _pendule?.Pause();          // le temps ne compte pas pendant l'analyse (reprise dans MinuteriePendule_Tick à la fin)
            foreach (Coup coup in LogiqueMouvements.ListeCoups)
                coup.EffaceAnalyse();   // une analyse interrompue ne mélange plus l'ancienne et la nouvelle
            _analyseDePartie = new AnalyseDePartie(LogiqueMouvements.ListeCoups, approfondir: complete,
                                                 coupDeBibliotheque: PolyglotBibliotheque.ContientCoup,   // jamais annotés
                                                 ouvertures: CatalogueDesOuvertures());
            CompleteEco();
            _moteurAuxiliaire?.Dispose();
            _moteurAuxiliaire = new MoteurAuxiliaire(CheminStockfish);
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
            InformationsPartie.Text = T("Analyse de la partie...");
            MetAJourCommandes();        // échiquier bloqué
            OuvreFenetreAnalyse(complete);
            AnalysePositionSuivante();
        }
        private void AnalysePositionSuivante()
        {
            if (_analyseDePartie?.PositionSuivante is not int index)
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
            BoutonAnalysePartie.Values.Text = approfondissement ? T("Interrompre (vérif. {0}/{1})", faites + 1, total) : T("Interrompre ({0}/{1})", faites + 1, total);
            InformationsPartie.Text = approfondissement ? T("Vérification des coups douteux ({0} s)...", DureeApprofondissement / 1000) : T("Analyse de la partie...");
            if (_fenetreAnalyse != null)
            {   // Fenêtre de suivi : étape, position examinée, progression et temps restant estimé (au rythme de 3 ou 10 s par position)
                string position = NomDeLaPosition(_analyseDePartie.IndexDansListeCoups(index));
                TimeSpan reste = TimeSpan.FromMilliseconds((double)(total - faites) * (approfondissement ? DureeApprofondissement : DureeAnalyseParPosition));
                // (la durée par position n'est qu'un plafond depuis la profondeur visée : le reste est un maximum)
                string temps = T("Temps restant : au plus {0}", TexteDuree(reste))
                    + (_analyseComplete && !approfondissement ? " " + T("(puis la vérification des coups douteux)") : "");
                _fenetreAnalyse.MontreEtape(position, approfondissement ? T("vérification {0}/{1}", faites + 1, total) : $"{faites + 1}/{total}",
                                            faites, total, temps);
            }
            // (profondeur visée, la durée réglée n'étant plus qu'un plafond : AnalyseDePartie.ProfondeurVisee)
            _pilote.DemanderAnalyse(LogiqueMouvements.PositionDepuisFen(_analyseDePartie.Positions[index].Fen),
                                    approfondissement ? DureeApprofondissement : DureeAnalyseParPosition, _analyseDePartie.ProfondeurVisee(index));
        }
        private string NomDeLaPosition(int indexCoup) =>
            // Pour la fenêtre de suivi : "Position après 25... c5", ou "Position de départ" (position initiale, ou départ FEN)
            indexCoup < 0 || indexCoup >= LogiqueMouvements.ListeCoups.Count || LogiqueMouvements.ListeCoups[indexCoup].EstPositionDeDepart
                ? T("Position de départ") : T("Position après {0}", Notation(LogiqueMouvements.ListeCoups[indexCoup].PgnFrNumerote));
        private async void PositionAnalyseeParLeMoteur(LigneAnalyse? meilleure)
        {   // Réponse du moteur pour la position demandée (null : aucun score) : on l'enregistre et on passe à la suivante
            AnalyseDePartie? analyse = _analyseDePartie;
            if (analyse == null)
                return;
            analyse.Enregistre(_positionEnAnalyse, meilleure, meilleure != null ? _pilote.Lignes.Seconde : null);
            await MesureDifficultes(analyse);
            if (_analyseDePartie != analyse)
                return;         // interrompue pendant la mesure (TermineAnalyseDePartie a déjà tout rangé)
            // Annotations au fur et à mesure : l'analyse allant de la fin vers le début, la position d'après est déjà
            // analysée et le coup joué dans celle-ci peut être jugé tout de suite ; la feuille est redessinée avec la position suivante
            analyse.AppliqueAuxCoups(LogiqueMouvements.ListeCoups);
            AnalysePositionSuivante();
        }
        private async Task MesureDifficultes(AnalyseDePartie analyse)
        {   // Seuls bons coups devenus jugeables : le 2e moteur dit à quelle profondeur il les trouve (quelques dixièmes de seconde
            // chacun, hors du thread de l'interface ; le moteur principal attend, rien ne tourne en même temps)
            if (_moteurAuxiliaire is not MoteurAuxiliaire moteur)
                return;
            foreach ((int index, string fen, string coup) in analyse.DifficultesAMesurer().ToList())
            {
                List<(int, string)> meilleurs = await Task.Run(() => moteur.MeilleursCoupsParProfondeur(fen, JugementCoups.ProfondeurMesureDifficulte));
                if (_analyseDePartie != analyse)
                    return;
                analyse.EnregistreDifficulte(index, JugementCoups.ProfondeurDecouverte(meilleurs, coup));
            }
            // (pièges et menaces : dans les deux analyses depuis le 2026-10-29, ils ne coûtent que quelques secondes par partie)
            foreach ((int index, string fen, string prise) in analyse.PiegesAChercher())
            {   // la prise tentante que le joueur n'a pas jouée : que donnait-elle ? (une ou deux secondes chacune)
                var resultat = await Task.Run(() => moteur.Cherche(fen, prise, JugementCoups.ProfondeurPiege));
                if (_analyseDePartie != analyse)
                    return;
                analyse.EnregistrePiege(index, prise, resultat?.Centipions, resultat?.MatEn, resultat?.Variante);
            }
            foreach ((int index, string fenCoupNul) in analyse.MenacesAChercher())
            {   // la menace du coup joué : ce que son auteur jouerait si l'adversaire ne faisait rien (coup nul, environ 0,5 s)
                var resultat = await Task.Run(() => moteur.Cherche(fenCoupNul, null, JugementCoups.ProfondeurMenace));
                if (_analyseDePartie != analyse)
                    return;
                analyse.EnregistreMenace(index, resultat?.Centipions, resultat?.MatEn, resultat?.Variante);
            }
        }
        private void ChoisitDureesAnalyse()
        {   // Options > Durées de l'analyse de partie : valables dès la position suivante (même pendant une analyse)
            using FenetreDureesAnalyse fenetre = new(_dureeAnalyseRapide, _dureeAnalyseComplete);
            if (fenetre.ShowDialog(this) != DialogResult.OK)
                return;
            _dureeAnalyseRapide = fenetre.DureeRapide;
            _dureeAnalyseComplete = fenetre.DureeComplete;
        }
        private void OuvreFenetreAnalyse(bool complete)
        {   // Analyse de partie : fenêtre de suivi, bouton Interrompre (ce qui est analysé est gardé)
            _analyseComplete = complete;
            OuvreFenetreSuivi(complete ? T("Analyse complète de la partie") : T("Analyse rapide de la partie"), T("Interrompre"),
                              () => TermineAnalyseDePartie(interrompue: true));
        }
        private void OuvreFenetreSuivi(string titre, string texteBouton, Action arret)
        {   // Fenêtre de suivi, non bloquante (la barre sous la feuille était trop discrète) : à droite de la
            // fenêtre principale s'il y a la place à l'écran, sinon par-dessus sa colonne de boutons ; une seule à la fois
            FermeFenetreAnalyse();
            FenetreAnalyse fenetre = new(titre, texteBouton);
            Rectangle ecran = Screen.FromControl(this).WorkingArea;
            int x = Right + 8 + fenetre.Width <= ecran.Right ? Right + 8 : Right - fenetre.Width - 16;
            fenetre.Location = new Point(Math.Max(ecran.Left, x), Math.Max(ecran.Top, Top + 120));
            fenetre.Interrompre += arret;
            fenetre.FormClosed += (s, e) => { if (_fenetreAnalyse == fenetre) _fenetreAnalyse = null; };     // croix : l'analyse continue
            _fenetreAnalyse = fenetre;
            fenetre.Show(this);     // (appartient à la fenêtre principale : reste devant elle)
        }
        private void FermeFenetreAnalyse()
        {
            FenetreAnalyse? fenetre = _fenetreAnalyse;
            _fenetreAnalyse = null;
            fenetre?.Close();
        }
        private readonly ToolTip _infobulleBilan = new() { AutoPopDelay = 20000 };
        private static string TexteCoupCritique(Coup coup) =>
            // Ex : "Coup critique (clic) :" puis "   25... c5??   score -0.23 → 2.73" (évaluation avant, avec le meilleur coup, et après)
            T("Coup critique (clic) :") + "\n   " + Notation(coup.PgnFrNumerote) + coup.Annotation
            + (coup.EvaluationMeilleur is Evaluation avant && coup.EvaluationApres is Evaluation apres ? "   " + T("score {0} → {1}", avant.Texte, apres.Texte) : "");
        private void EffaceBilan()
        {
            BilanAnalyse.Text = "";
            PartieEnCours.Bilan = "";       // (il n'est plus écrit dans le fichier PGN)
            _coupCritique = null;
            BilanAnalyse.Cursor = Cursors.Default;
        }
        private void AfficheBilan(string bilan, int? coupCritique = null)
        {   // Le bilan sous la feuille, gardé dans la partie pour le fichier PGN ({… [%bilan]}, 2026-10-28) ; en tête,
            // le coup critique, cliquable : celui de l'analyse, ou, pour une partie relue, le coup qui a perdu le plus (au moins une erreur)
            PartieEnCours.Bilan = bilan;
            coupCritique ??= LogiqueMouvements.ListeCoups.Select((coup, index) => (coup, index))
                .Where(c => !c.coup.EstPositionDeDepart && c.coup.PerteAnalyse >= JugementCoups.SeuilErreur)
                .OrderByDescending(c => c.coup.PerteAnalyse).Select(c => (int?)c.index).FirstOrDefault();
            if (coupCritique < 0)
                coupCritique = null;        // (-1 : l'analyse n'a pas trouvé de coup critique)
            BilanAnalyse.Text = bilan;
            _coupCritique = coupCritique;
            BilanAnalyse.Cursor = coupCritique != null ? Cursors.Hand : Cursors.Default;
            if (coupCritique is int index)
                BilanAnalyse.Text = TexteCoupCritique(LogiqueMouvements.ListeCoups[index]) + "\n" + bilan;
        }
        private void BilanAnalyse_Click(object? sender, EventArgs e)
        {   // Clic sur le bilan : l'échiquier montre le coup critique (avec son analyse et ses flèches)
            if (_coupCritique is int index && _analyseDePartie == null && index < LogiqueMouvements.ListeCoups.Count)
                AfficheCoupDeLaPartie(index);
        }

        // ═══ Analyse d'un fichier PGN en arrière-plan (voir AnalyseFichier.cs) : menu Analyse ═══
        // Les parties choisies sont analysées une à une par deux Stockfish à part (analyse rapide, durée par position réglée),
        // hors du thread de l'interface : on peut jouer, parcourir ou analyser une autre partie pendant ce temps. Chaque partie
        // finie est ajoutée à « <fichier>_analyse.pgn » ; une interruption (menu, fermeture) garde les précédentes. L'avancement
        // est écrit sur la 1re ligne de variante quand elle est libre (ni parcours ni autre analyse), et dans le menu
        private AnalyseurDeFichier? _analyseurDeFichier;            // null : aucune analyse de fichier en cours
        private CancellationTokenSource? _arretAnalyseFichier;
        private ToolStripMenuItem? _menuAnalyseFichier;
        private string? _texteAnalyseFichier;                      // ce qui a été écrit sur la 1re ligne de variante (effacé à la fin)
        private FenetreAnalyse? _fenetreAnalyseFichier;             // fenêtre de suivi (null : fermée par sa croix, l'analyse continue)

        private void OuvreSuiviAnalyseFichier(string titre)
        {   // Fenêtre de suivi, placée comme celle de l'analyse de partie (à droite de la fenêtre principale s'il y a la place,
            // sinon par-dessus sa colonne de boutons)
            FenetreAnalyse suivi = new(titre, T("Interrompre"));
            Rectangle ecran = Screen.FromControl(this).WorkingArea;
            int x = Right + 8 + suivi.Width <= ecran.Right ? Right + 8 : Right - suivi.Width - 16;
            suivi.Location = new Point(Math.Max(ecran.Left, x), Math.Max(ecran.Top, Top + 120));
            suivi.Interrompre += AnalyseFichier_Click;
            suivi.FormClosed += (s, e) => { if (_fenetreAnalyseFichier == suivi) _fenetreAnalyseFichier = null; };   // croix : l'analyse continue
            _fenetreAnalyseFichier = suivi;
            suivi.Show(this);
        }

        private bool LigneLibrePourAnalyseFichier =>
            // La 1re ligne de variante n'est pas occupée : ni coup regardé (parcours), ni analyse de partie ou de position, ni coup
            // demandé au moteur (sa réflexion s'y affiche) ; un texte plus ancien (dernière variante du moteur…) est remplacé
            _positionAffichee == null && _analyseDePartie == null && _pilote.Demande == TypeDemande.Aucune;

        private async void AnalyseFichier_Click()
        {   // "Analyser un fichier PGN…", ou "Interrompre l'analyse du fichier" pendant qu'elle tourne
            if (_analyseurDeFichier != null)
            {
                _arretAnalyseFichier?.Cancel();
                _analyseurDeFichier.Dispose();      // (la question en cours au moteur s'arrête tout de suite)
                return;
            }
            using OpenFileDialog choix = new() { Filter = T("Fichier PGN (*.pgn)|*.pgn"), Title = T("Analyser un fichier PGN") };
            if (choix.ShowDialog(this) != DialogResult.OK)
                return;
            string fichier = choix.FileName, sortie = AnalyseurDeFichier.FichierDeSortie(fichier);
            List<PartieEchecsPGN> parties;
            HashSet<string> deja;
            try
            {
                parties = [.. FichierPartiePgn.DecodeFichierPGN(fichier).Select(FichierPartiePgn.DecodePartiePGN)];
                deja = AnalyseurDeFichier.PartiesDejaAnalysees(sortie);
            }
            catch (Exception ex)
            {   // fichier illisible ou contenu inattendu (erreurs variées : lecture, décodage) : expliqué et noté dans le journal
                Journal.Erreur("Analyse du fichier PGN " + fichier, ex);
                KryptonMessageBox.Show(T("Impossible de lire ce fichier PGN :\n{0}", ex.Message), T("Analyse d'un fichier PGN"),
                    KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Warning);
                return;
            }
            List<PartieEchecsPGN> aProposer = [.. parties.Where(p => !deja.Contains(AnalyseurDeFichier.Cle(p)))];
            if (aProposer.Count == 0)
            {
                KryptonMessageBox.Show(parties.Count == 0 ? T("Aucune partie dans ce fichier.") : T("Toutes les parties de ce fichier sont déjà analysées dans {0}.", Path.GetFileName(sortie)),
                    T("Analyse d'un fichier PGN"), KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                return;
            }
            // Durée estimée : au plus la durée par position (le moteur s'arrête souvent avant, à la profondeur visée), environ
            // 80 positions par partie
            string Estimation(int nombre) => T("au plus {0}", TexteDuree(TimeSpan.FromMilliseconds(80.0 * nombre * DureeAnalyseParPosition)));
            using FenetreAnalyseFichier fenetre = new(Path.GetFileName(fichier), Path.GetFileName(sortie), aProposer, parties.Count - aProposer.Count, Estimation);
            if (fenetre.ShowDialog(this) != DialogResult.OK)
                return;
            await AnalyseLeFichier(fenetre.PartiesChoisies, sortie);
        }

        private void AfficheAvancementFichier(string texte)
        {
            if (!LigneLibrePourAnalyseFichier)
                return;
            _texteAnalyseFichier = texte;
            VarianteMoteurUci1.Text = texte;
        }

        private async Task AnalyseLeFichier(List<PartieEchecsPGN> parties, string sortie)
        {
            // Threads : ceux de l'utilisateur, au plus la moitié des cœurs (il reste de quoi jouer contre le moteur) ; son hachage
            int threads = Math.Clamp(MoteurUci.NombreThreads ?? 1, 1, Math.Max(1, Environment.ProcessorCount / 2));
            AnalyseurDeFichier analyseur = new(CheminStockfish, threads, MoteurUci.TailleHachageMo ?? 64, DureeAnalyseParPosition,
                                               PolyglotBibliotheque.ContientCoup, CatalogueDesOuvertures());
            CancellationTokenSource arret = new();
            _analyseurDeFichier = analyseur;
            _arretAnalyseFichier = arret;
            OuvreSuiviAnalyseFichier(T("Analyse du fichier {0}", Path.GetFileName(sortie)));
            AfficheAvancementFichier(T("Analyse du fichier : démarrage…"));
            Stopwatch chrono = Stopwatch.StartNew();
            int ajoutees = 0, positionsAvant = 0, partiePrecedente = 0, positionsDeLaPartie = 0;
            void Montre(EtatAnalyseFichier etat)
            {   // Partie et position en cours ; temps restant estimé au rythme des positions déjà faites (80 par partie à venir)
                if (etat.Partie != partiePrecedente)
                {
                    positionsAvant += positionsDeLaPartie;
                    partiePrecedente = etat.Partie;
                }
                positionsDeLaPartie = etat.PositionsTotal;
                int faites = positionsAvant + etat.PositionsFaites;
                int restantes = etat.PositionsTotal - etat.PositionsFaites + 80 * (etat.NombreParties - etat.Partie);
                string reste = faites > 0 ? T("Temps restant : environ {0}", TexteDuree(chrono.Elapsed / faites * restantes)) : "";
                if (_menuAnalyseFichier != null)
                    _menuAnalyseFichier.Text = T("Interrompre l'analyse du fichier ({0}/{1})", etat.Partie, etat.NombreParties);
                AfficheAvancementFichier(T("Analyse du fichier : partie {0}/{1} ({2}), position {3}/{4}", etat.Partie, etat.NombreParties, etat.NomPartie,
                                           Math.Min(etat.PositionsFaites + 1, etat.PositionsTotal), etat.PositionsTotal) + (reste != "" ? " — " + reste : ""));
                _fenetreAnalyseFichier?.MontreEtape(T("Partie {0}/{1} : {2}", etat.Partie, etat.NombreParties, etat.NomPartie), "",
                                                    etat.PositionsFaites, etat.PositionsTotal, reste);
                _fenetreAnalyseFichier?.MontreScore(T("position {0}/{1}", Math.Min(etat.PositionsFaites + 1, etat.PositionsTotal), etat.PositionsTotal), "");
            }
            string? erreur = null;
            bool interrompue;
            try
            {
                ajoutees = await Task.Run(() => analyseur.Analyser(parties, sortie, etat => SurLeThreadInterface(() => Montre(etat)), arret.Token));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {   // fichier de sortie impossible à écrire (verrouillé, protégé...)
                Journal.Erreur("Analyse du fichier : écriture de " + sortie, ex);
                erreur = ex.Message;
            }
            finally
            {   // (toujours : sinon, après une erreur imprévue, le menu resterait sur « Interrompre » et les moteurs tourneraient)
                interrompue = arret.IsCancellationRequested;
                analyseur.Dispose();
                arret.Dispose();
                _analyseurDeFichier = null;
                _arretAnalyseFichier = null;
                if (!IsDisposed && !Disposing)
                {
                    _fenetreAnalyseFichier?.Close();
                    if (_texteAnalyseFichier != null && VarianteMoteurUci1.Text == _texteAnalyseFichier)
                        VarianteMoteurUci1.Text = "";
                    _texteAnalyseFichier = null;
                    if (_menuAnalyseFichier != null)
                        _menuAnalyseFichier.Text = T("Analyser un fichier PGN…");
                }
            }
            if (IsDisposed || Disposing)
                return;     // (application fermée pendant l'analyse)
            string bilan = erreur != null ? T("Analyse du fichier arrêtée : {0}", erreur)
                : interrompue ? T("Analyse du fichier interrompue : {0} partie(s) ajoutée(s) à {1} (la suite reprendra à la prochaine analyse de ce fichier).", ajoutees, Path.GetFileName(sortie))
                : T("Analyse du fichier terminée : {0} partie(s) ajoutée(s) à {1}.", ajoutees, Path.GetFileName(sortie));
            if (ajoutees > 0 && KryptonMessageBox.Show(bilan + "\n\n" + T("Afficher la liste des parties analysées ?"), T("Analyse d'un fichier PGN"),
                                                        KryptonMessageBoxButtons.YesNo, KryptonMessageBoxIcon.Information) == DialogResult.Yes)
                OuvreListePartiesPgn(sortie);
            else if (ajoutees == 0)
                KryptonMessageBox.Show(bilan, T("Analyse d'un fichier PGN"), KryptonMessageBoxButtons.OK,
                    erreur != null ? KryptonMessageBoxIcon.Warning : KryptonMessageBoxIcon.Information);
        }

        private void TermineAnalyseDePartie(bool interrompue)
        {   // Fin (ou interruption) : les résultats vont dans les coups (annotations proposées, plus pâles), puis le bilan
            AnalyseDePartie? analyse = _analyseDePartie;
            if (analyse == null)
                return;
            _analyseDePartie = null;        // avant AbandonneReflexion, qui sinon reviendrait ici
            _moteurAuxiliaire?.Dispose();   // (une mesure en cours s'arrête : son coup garde la règle sans difficulté)
            _moteurAuxiliaire = null;
            FermeFenetreAnalyse();
            _positionEnAnalyse = -1;
            AbandonneReflexion();           // la position en cours d'analyse (interruption) : sa réponse sera ignorée
            if (_multiPvAvantAnalyse is int multiPv && multiPv != MoteurUci.NombreLignesPV)
                MoteurUci.DefinitMultiPV(multiPv);      // nombre de variantes choisi par l'utilisateur, forcé à 2 pendant l'analyse
            _multiPvAvantAnalyse = null;
            analyse.AppliqueAuxCoups(LogiqueMouvements.ListeCoups);
            // (en tête du bilan : le coup critique, le moment où la partie a basculé ; -1 : aucun)
            AfficheBilan(analyse.TexteBilan(), analyse.CoupCritique()?.IndexCoup ?? -1);
            BarreAnalysePartie.Visible = false;
            BoutonAnalysePartie.Values.Text = T("Analyse rapide");
            BoutonAnalysePartie.Width = BoutonAnalyseComplete.Left - 4 - BoutonAnalysePartie.Left;
            BoutonAnalyseComplete.Visible = true;
            RetourPositionCourante();       // l'échiquier, qui suivait l'analyse, revient à la partie
            InformationsPartie.Text = interrompue ? T("Analyse interrompue") : T("Analyse de la partie terminée");
            MetAJourCommandes();
        }
    }
}
