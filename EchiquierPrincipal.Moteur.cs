// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// EchiquierPrincipal (fichier partiel) : Moteur UCI : affichage de ses réponses, coup du moteur et bibliothèque, réflexion à temps fixe

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
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        // Procédures d'affichage diverses
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void AfficheUci()   // Affiche les informations du moteur UCI (la ligne est déjà décodée dans MoteurUci.DerniereLigne)
        {   // ATTENTION : MALGRE LA PRESENCE DU PROTOCOLE UCI, LES MOTEURS ONT DES REPONSES DIFFERENTES !!?? (voir case "info", par ex)
            if (SurLeThreadInterface(AfficheUci))
                return;     // (relancée sur le thread de l'interface, ou ignorée si la fenêtre est fermée)
            LigneUci ligne = MoteurUci.DerniereLigne;
            if ((ligne.Commande == "info" || ligne.Commande == "bestmove") && MoteurUci.LigneAbandonnee)
                return;     // réponse à une demande abandonnée (retour arrière, nouvelle partie...) : on n'affiche rien
            switch (ligne.Commande)     // identifier le premier mot
            {
                case "bestmove":        // **** le moteur UCI propose le meilleur coup ! ****
                    if (ligne.AucunCoupLegal)
                    {   // Cas particulier : le moteur retourne "bestmove (none)" ou "bestmove 0000" => partie terminée (mat ou pat).
                        // Le camp au trait est-il en échec ? (calculé ici, sur la position affichée, pas sur un état d'échec éventuellement ancien)
                        bool mat = LogiqueMouvements.CampAuTraitEnEchec();
                        if (mat)
                            LogiqueMouvements.EchecetMat = true;
                        VarianteMoteurCourante.Text = mat ? T("Aucun coup légal : échec et mat") : T("Aucun coup légal : pat");
                        // la demande est terminée, sans coup à jouer (AfficheCoupMoteur n'est pas appelé) ; analyse de partie : position suivante
                        if (_pilote.ReponseRecue() == TypeDemande.Analyse && _analyseDePartie != null)
                            PositionAnalyseeParLeMoteur(null);
                        break;
                    }
                    if (_analyseDePartie != null)
                        break;      // analyse de partie : aucun coup n'est joué (la suite est dans AfficheCoupMoteur)
                    VarianteMoteurCourante.Text = T("Coup joué : {0}", Notation(Outils.VarianteUciVersPgn(ligne.MeilleurCoup ?? "", LogiqueMouvements.DemiCoupAvant(PositionDesVariantes), false, PositionDesVariantes))) +
                        (ligne.CoupConseil != null ? "   " + T("(Conseil : {0})", Notation(Outils.VarianteUciVersPgn(ligne.MeilleurCoup + " " + ligne.CoupConseil, LogiqueMouvements.DemiCoupAvant(PositionDesVariantes), true, PositionDesVariantes))) : "");  // Le conseil (ponder) se joue après le coup du moteur
                    break;
                case "id":
                    if (ligne.NomMoteur != null)
                    {   // Récupération du nom du moteur (limité à 20 caractères pour l'affichage)
                        _nomMoteur = ligne.NomMoteur[..Math.Min(20, ligne.NomMoteur.Length)];
                        // Le nom va au(x) camp(s) joué(s) par le moteur, jamais à un joueur humain
                        // (ex : partie PGN chargée, ou moteur qui joue les Blancs et redémarre après une mise à jour)
                        AfficheMoteurDansLaPartie();
                    }
                    if (ligne.AuteurMoteur != null)     // Récupération de l'auteur
                        VarianteMoteurUci3.Text = "     " + T("Auteur(s) du moteur {0} = {1}", _nomMoteur, ligne.AuteurMoteur);
                    break;
                case "info":            // **** Infos de réflexion moteur ****
                    AfficheInfoMoteur(ligne);
                    break;
            }
        }

        private void AfficheInfoMoteur(LigneUci ligne)
        {   // Affiche le score et la variante d'une ligne "info" du moteur (décodées par _pilote.Lignes, du point de vue des Blancs)
            if (ligne.DansBibliotheque)
                VarianteMoteurUci1.Text = "    " + T("Le moteur est dans sa bibliothèque d'ouvertures");

            LigneAnalyse? ligneAnalyse = _pilote.Lignes.Ajouter(ligne, PositionDesVariantes);
            if (ligneAnalyse == null)
                return;     // ni score ni variante (ex : "info depth 12")
            // On affiche seulement le score de la meilleure variante (un moteur sans MultiPV, comme Sargon, n'a que celle-là)
            if (ligneAnalyse.Numero == 1 && ligneAnalyse.Evaluation is Evaluation evaluation)
            {
                EvaluationUci.Text = evaluation.Appreciation;
                if (evaluation.EstUnMat)
                {
                    ScoreMoteur.Text = T("MAT en {0}", Math.Abs(evaluation.MatEn ?? 0));
                    InformationPourJoueur.Text = evaluation.TexteMat;      // "MAT en 3 pour les Blancs"
                }
                else
                    ScoreMoteur.Text = T("Score : {0}", evaluation.Texte);
            }
            if (ligne.Variante == null)
                return;

            // Affichage de la variante
            string texteVariante = ligneAnalyse.Symbole + " (" + Notation(ligneAnalyse.Debut) + ") █[ " + ligneAnalyse.TexteScore + " ]█  " + "[ " + Notation(ligneAnalyse.VariantePgn) + " ]";
            if (ligne.NumeroVariante is int numeroVariante)
            {   // Une zone d'affichage par variante (VarianteMoteurUci1, 2, 3) ; les variantes au-delà ne sont pas affichées
                if (Controls.Find("VarianteMoteurUci" + numeroVariante, true).FirstOrDefault() is RichTextBox zoneVariante)
                    zoneVariante.Text = " " + texteVariante;
            }
            else
            {   // Pour ceux qui n'ont qu'une variante principale (Sargon, ...) : on n'utilise que la zone VarianteMoteurUci1
                VarianteMoteurUci1.Text = texteVariante;
                VarianteMoteurUci2.Text = "... " + T("{0} n'affiche qu'une variante", _nomMoteurChoisi) + " ..."; VarianteMoteurUci3.Text = "...";
            }
        }

        private void AfficheDonneesBrutes()
        {   // Affiche les données brutes du moteur UCI, pour les curieux qui veulent voir ce qui se passe "sous le capot" :-)
            // Le texte est pris tout de suite (la ligne suivante remplacera DataUci) ; l'affichage est envoyé à l'interface
            // sans l'attendre (BeginInvoke), pour ne pas ralentir la lecture des lignes du moteur
            string texte;
            if (MoteurUci.UciVersGui)
            {
                if (MoteurUci.DataUci.Contains("currmove"))     // Inutile d'afficher les currmove, il n'y rien d'intéressant ...
                    return;
                texte = "[" + _nomMoteur + "]    " + MoteurUci.DataUci;
            }
            else
                texte = " [BrunoGUI_GenII]    " + MoteurUci.DataVersUci;
            if (!InvokeRequired)
                donneesBrutesUci.AjouteLigne(texte);
            else if (IsHandleCreated && !IsDisposed)
            {
                try
                {
                    BeginInvoke(new MethodInvoker(() => donneesBrutesUci.AjouteLigne(texte)));
                }
                catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
                {   // fenêtre en cours de fermeture : la ligne n'est plus affichée
                    Debug.WriteLine("AfficheDonneesBrutes : " + ex.Message);
                }
            }
        }

        private void AfficheCoupMoteur()
        {   // Le moteur UCI joue son meilleur coup
            if (SurLeThreadInterface(AfficheCoupMoteur))
                return;     // (relancée sur le thread de l'interface, ou ignorée si la fenêtre est fermée)
            else
            {
                if (MoteurUci.LigneAbandonnee)
                    return;     // la demande a été abandonnée entre-temps (vérifié ici, sur le thread de l'interface) : coup ignoré
                MiseaZeroTimer();
                if (_emetUnSon && _analyseDePartie == null)     // (pas de son à chaque position d'une analyse de partie)
                {   // Son pour dire que le coup est joué (son système par défaut si le fichier de Windows est absent)
                    try
                    {
                        SoundPlayer player = new(@"C:\Windows\Media\Windows Notify.wav");
                        player.Play();
                    }
                    catch (Exception ex) when (ex is FileNotFoundException || ex is InvalidOperationException)
                    {
                        SystemSounds.Asterisk.Play();
                    }
                }
                TypeDemande demande = _pilote.ReponseRecue();   // à quelle demande répond ce bestmove ?
                if (demande == TypeDemande.CoupDePartie)
                {   // Coup de la partie : on le joue (promotion comprise)
                    StatusProgramme.Text = InformationPourJoueur.Text = T("A vous de jouer");
                    _caseSource = MoteurUci.CoupAuFormatUci[..2];    // CoupAuFormatUci contient le "best move" sous la forme e2e4
                    _caseDestination = MoteurUci.CoupAuFormatUci.Substring(2, 2);
                    PiloteMoteur.JouerCoupUci(MoteurUci.CoupAuFormatUci);      // (rien n'est joué si la position est déjà un mat)
                    // Cases de départ et d'arrivée du coup colorées (pendant le parcours : seulement au retour à la partie)
                    _vue.MontreDernierCoup(RenvoieCaseIndex120(_caseSource), RenvoieCaseIndex120(_caseDestination));
                    LeMoteurARepondu();      // On réautorise si le moteur a fini de réfléchir
                    AfficheCoupDuMoteur();
                    if (ParcoursEnCours)
                    {   // Le coup est joué dans la partie, mais l'affichage reste sur la position passée que l'utilisateur regarde
                        StatusProgramme.Text = T("Le moteur a joué");
                        InformationsPartie.Text = T("Le moteur a joué : Fin pour revenir");
                    }
                }
                else if (demande == TypeDemande.Analyse && _analyseDePartie != null)
                    PositionAnalyseeParLeMoteur(_pilote.Lignes.Meilleure);     // analyse de partie : position suivante
                else if (demande == TypeDemande.Analyse)
                {   // c'est une analyse : on affiche la meilleure variante (mémorisée par _pilote.Lignes, pas relue dans le texte affiché) :
                    // dans une boîte de message, sur la 1re ligne de variante (centrée, en gras ; les autres variantes restent sur les
                    // lignes 2 et 3), dans la barre d'état, et une flèche verte montre le coup conseillé sur l'échiquier (effacée par
                    // toute action qui change la position)
                    LigneAnalyse? meilleure = _pilote.Lignes.Meilleure;
                    InformationPourJoueur.Text = StatusProgramme.Text = T("Analyse terminée ...");
                    string titre = T("Analyse Moteur ({0} sec.) par {1}", _dureeReflexionMilliSeconde / 1000, _nomMoteur);
                    if (meilleure?.VariantePgn == null)
                    {
                        InformationsPartie.Text = T("Le moteur n'a donné aucune variante.");
                        _ = KryptonMessageBox.Show(T("Le moteur n'a donné aucune variante."), titre, KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    }
                    else
                    {
                        string appreciation = meilleure.Evaluation?.Appreciation ?? T("évaluation inconnue");
                        ScoreMoteur.Text = T("Score = {0}", meilleure.TexteScore);
                        EvaluationUci.Text = appreciation;
                        VarianteMoteurCourante.Text = InformationsPartie.Text = T("Coup suggéré : {0}", Notation(meilleure.Debut));
                        AfficheLigneCentree(VarianteMoteurUci1, T("Meilleure suite ({0}, {1}) : {2}", meilleure.TexteScore, appreciation, Notation(meilleure.VariantePgn)));
                        string? conseil = (meilleure.VarianteUci ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                        if (conseil is { Length: >= 4 } && RenvoieCaseIndex120(conseil[..2]) > 0 && RenvoieCaseIndex120(conseil[2..4]) > 0)
                            _vue.MontreFleches((RenvoieCaseIndex120(conseil[..2]), RenvoieCaseIndex120(conseil[2..4]), CouleurFlecheMeilleurCoup));
                        MetAJourCommandes();
                        _ = KryptonMessageBox.Show(T("La meilleure suite est : {0}\n Evaluation --- {1} --- ({2})\n{3}",
                            Notation(meilleure.Debut), meilleure.TexteScore, appreciation, Notation(meilleure.VariantePgn)), titre, KryptonMessageBoxButtons.OK, KryptonMessageBoxIcon.Information);
                    }
                    MetAJourCommandes();
                }
                // (aucune demande en cours : bestmove après un "stop" sans demande, ignoré)
            }
        }

        private void AfficheCoupDuMoteur()
        {   // Le moteur vient de jouer (réflexion ou bibliothèque) : son coup dans le cadre vert, ex : "Coup joué : 23. Ce6".
            // Pas si la partie vient de finir (le cadre montre alors le résultat)
            if (_partie.EnCours && LogiqueMouvements.ListeCoups.Count > 0 && !LogiqueMouvements.ListeCoups[^1].EstPositionDeDepart)
                InformationsPartie.Text = T("Coup joué : {0}", Notation(LogiqueMouvements.ListeCoups[^1].PgnFrNumerote));
        }
        private void CoupJoue(string coup)
        {   // Un coup (blanc ou noir) vient d'être joué dans la partie (événements AfficheCoupBlanc et AfficheCoupNoir)
            if (LogiqueMouvements.EchecetMat)
            {
                StatusProgramme.Text = T("Partie terminée");
                return;
            }
            PartieEnCours.CompteDePLy = LogiqueMouvements.DemiCoupsJoues.ToString();
            if (_pendule?.CampQuiDecompte == null)
                _pendule?.Demarrer(QuiJoue);    // premier coup de la partie : la pendule part pour l'adversaire (rien n'est décompté avant)
            else if (!_pendule.CoupJoue(LogiqueMouvements.ListeCoups[^1].NumeroDuCoup) && _pendule.TempsEcoule() is ColorPiece campSansTemps)
            {   // Coup joué alors que le temps était déjà écoulé (entre deux tics de la minuterie) : la partie est perdue au temps
                PerteAuTemps(campSansTemps);
                return;
            }
            if (_pendule != null)
                _pendule.NoteTemps(LogiqueMouvements.ListeCoups[^1]);   // pour le retour arrière et "Reprendre ici"
            AffichePendules();
            string? raisonNulle = _partie.RejeuPgn ? null : LogiqueMouvements.RaisonNulle();    // répétition, 50 coups ou matériel insuffisant
            if (raisonNulle != null)
                GestionResultat("1/2-1/2", raisonNulle);
            MetAJourCommandes();    // un coup a été joué : on peut parcourir la partie, la liste des coups, le retour arrière...
        }

        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        //  Bibliothèque d'ouvertures
        // ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
        private void JeuMoteurAvecBibliotheque(string chaineFen)
        {   // Coup du moteur pour la partie : bibliothèque d'ouvertures d'abord (voir _pilote.ChoixBibliotheque), sinon réflexion du moteur
            // Avec une pendule, le moteur reçoit les temps restants et gère son temps ; sinon, un temps fixe par coup
            // (numéro du coup à jouer : pour une cadence à deux périodes, le moteur sait combien de coups restent avant le contrôle)
            LimiteTemps limite = _pendule != null ? LimiteTemps.DepuisPendule(_pendule, (int)Math.Truncate(LogiqueMouvements.NombreCoupsJoues))
                                                  : LimiteTemps.Duree(_dureeReflexionMilliSeconde);
            if (_pilote.DemanderCoup(chaineFen, limite) == ResultatDemandeCoup.CoupBibliotheque)
            {   // Coup trouvé dans la bibliothèque : il est déjà joué
                string? coupChoisiTxt = _pilote.DernierCoupBibliotheque;
                VarianteMoteurUci1.Text = T("Coup bibliothèque {0} exécuté par le moteur -> {1}", Path.GetFileName(_bibliotheque), coupChoisiTxt);
                VarianteMoteurUci2.Text = VarianteMoteurUci3.Text = ".....";
                Debug.WriteLine($"Coup bibliothèque exécuté : {coupChoisiTxt}");
                AfficheCoupsBibliotheque(LogiqueMouvements.RetourneChaineFenActuel());
                AfficheCoupDuMoteur();
                // Barre d'état : comme pour un coup réfléchi (écrit à la réception du bestmove), qu'un coup de bibliothèque n'a pas
                if (LogiqueMouvements.ListeCoups.Count > 0)
                    VarianteMoteurCourante.Text = T("Coup joué : {0} (bibliothèque)", Notation(LogiqueMouvements.ListeCoups[^1].PgnFrNumerote));
                return;
            }
            // Aucun coup dans la bibliothèque ou bibliothèque inactive : le moteur réfléchit
            if (_pendule == null)
                LancerReflexion();  // Décompte le temps de réflexion (avec une pendule, c'est elle qui décompte)
            if (!LogiqueMouvements.EchecetMat)
            {
                InformationPourJoueur.Text = StatusProgramme.Text = T("{0} réfléchit ...", _nomMoteur);
            }
        }

        // ═══ Bibliothèque d'ouvertures : affichage des coups connus, et choix du coup quand le moteur doit jouer ═══
        private static readonly Random _hasard = new();
        private Font? _policeBiblioNormale, _policeBiblioGras;     // créées une seule fois (une police par ligne serait une fuite)
        private string? ChoisirCoupBibliotheque(string fen)
        {   // Le moteur doit jouer : coup choisi dans la bibliothèque (PolyglotBibliotheque.ChoisirEntree), marqué ⭐ dans la liste ;
            // null s'il n'y en a pas
            List<EntreePolyglot> entrees = PolyglotBibliotheque.TrouverLesEntrees(PolyglotBibliotheque.CalculeClefPolyglot(fen)).ToList();
            EntreePolyglot? choisie = PolyglotBibliotheque.ChoisirEntree(entrees, _bibliothequeAleatoire, _hasard);
            AfficheCoupsBibliotheque(entrees, choisie, fen);
            return choisie == null ? null : PolyglotBibliotheque.DecodeCoup(choisie.CoupBiblio, fen);
        }
        private void AfficheCoupsBibliotheque(string fen)
        {   // Coups connus de la bibliothèque pour cette position (sans en choisir aucun)
            AfficheCoupsBibliotheque(PolyglotBibliotheque.TrouverLesEntrees(PolyglotBibliotheque.CalculeClefPolyglot(fen)).ToList(), null, fen);
        }
        private void AfficheCoupsBibliotheque(List<EntreePolyglot> entrees, EntreePolyglot? choisie, string fen)
        {   // Liste des coups, par poids décroissant ; le coup choisi (s'il y en a un) est marqué ⭐ en vert
            _policeBiblioNormale ??= new Font(CoupsBibliothequeBox.Font, FontStyle.Regular);
            _policeBiblioGras ??= new Font(CoupsBibliothequeBox.Font, FontStyle.Bold);
            CoupsBibliothequeBox.Clear();
            CoupsBibliothequeBox.SelectionAlignment = HorizontalAlignment.Center;
            CoupsBibliothequeBox.SelectionColor = Color.Black;
            CoupsBibliothequeBox.SelectionFont = _policeBiblioGras;
            CoupsBibliothequeBox.AppendText(T("Bibliothèque") + "\n--------------\n");
            CoupsBibliothequeBox.SelectionAlignment = HorizontalAlignment.Left;
            if (entrees.Count == 0)
            {
                CoupsBibliothequeBox.SelectionFont = _policeBiblioNormale;
                CoupsBibliothequeBox.AppendText(T("Aucun coup trouvé dans la bibliothèque.") + "\n");
                return;
            }
            foreach (EntreePolyglot entree in entrees.OrderByDescending(e => e.Poids))
            {
                bool estChoisie = entree == choisie;
                CoupsBibliothequeBox.SelectionFont = estChoisie ? _policeBiblioGras : _policeBiblioNormale;
                CoupsBibliothequeBox.SelectionColor = estChoisie ? Color.Green : Color.Black;
                CoupsBibliothequeBox.AppendText($"{(estChoisie ? "⭐ " : " *  ")}{PolyglotBibliotheque.DecodeCoup(entree.CoupBiblio, fen)} ({entree.Poids})\n");
            }
        }

        private void AbandonneReflexion()
        {   // Rend périmée la réflexion en cours (partie ou analyse) : le moteur s'arrête et sa réponse sera ignorée.
            // A appeler avant toute action qui change la partie ou la position (retour arrière, résultat, nouvelle partie, chargement...)
            AnnuleSelectionPiece();     // une pièce prise en main revient sur sa case
            if (_analyseDePartie != null)
                TermineAnalyseDePartie(interrompue: true);  // la partie va changer : on garde ce qui est déjà analysé
            _vue?.EffaceFleches();      // (flèche du coup conseillé par une analyse : elle ne vaut plus ; le parcours remet les siennes)
            if (!_pilote.Abandonner())
                return;     // le moteur ne réfléchissait pas : rien à signaler
            MiseaZeroTimer();
            LeMoteurARepondu();
            InformationPourJoueur.Text = StatusProgramme.Text = T("Réflexion du moteur interrompue");
        }
        private void LeMoteurARepondu()
        {   // Après que le moteur a répondu (ou a été interrompu) : état des commandes
            MetAJourCommandes();
        }

        private TimeSpan _debutReflexion;       // heure (_chrono) du début de la réflexion à temps fixe
        private void LancerReflexion()
        {   // Réflexion à temps fixe (analyse, ou coup du moteur sans pendule) : la barre se remplit pendant ce temps
            // (pendant la réflexion, tout reste possible : les actions qui la rendent inutile l'abandonnent, voir AbandonneReflexion)
            _debutReflexion = _chrono.Elapsed;
            BarreReflexion.Value = 0;
            BarreReflexion.Visible = true;
            timer.Interval = 100;
            timer.Tick -= Timer_Tick;
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {   // Tous les dixièmes de seconde : part du temps de réflexion écoulée (la barre reste pleine jusqu'à la réponse du moteur)
            double ecoule = (_chrono.Elapsed - _debutReflexion).TotalMilliseconds / Math.Max(1, _dureeReflexionMilliSeconde);
            BarreReflexion.Value = (int)Math.Round(Math.Min(1, ecoule) * BarreReflexion.Maximum);
            if (ecoule >= 1)
                timer.Stop();
        }
        private void MiseaZeroTimer()
        {   // Le moteur a répondu (ou la réflexion est abandonnée) : plus de barre
            timer.Stop();
            BarreReflexion.Visible = false;
            BarreReflexion.Value = 0;
            InformationsPartie.Text = "";
        }
    }
}
