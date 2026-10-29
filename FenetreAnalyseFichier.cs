// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre « Analyse d'un fichier PGN » (menu Analyse) : choix des parties à analyser
//      └─ Classe "FenetreAnalyseFichier" (KryptonForm construite en code, boîte de dialogue)
// Les parties du fichier, cochées au départ, filtrables par nom de joueur ; celles qui sont déjà dans le fichier de sortie
// (analyse interrompue puis relancée) ne sont pas proposées. Le résultat va dans « <fichier>_analyse.pgn » (AnalyseurDeFichier)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Krypton.Toolkit;
using static BrunoGUI_GenII.Langue;

namespace BrunoGUI_GenII
{
    public class FenetreAnalyseFichier : KryptonForm
    {
        private readonly List<PartieEchecsPGN> _parties;    // parties proposées (pas encore analysées)
        private readonly bool[] _cochees;                   // une par partie proposée, gardée quand le filtre change
        private readonly List<int> _affichees = [];         // place dans _parties de chaque ligne de la liste
        private readonly KryptonCheckedListBox _liste = new() { Location = new Point(12, 74), Size = new Size(676, 330), CheckOnClick = true };
        private readonly KryptonTextBox _filtre = new() { Location = new Point(130, 40), Width = 250 };
        private readonly KryptonLabel _estimation = new() { Location = new Point(12, 448), AutoSize = true };
        private readonly KryptonButton _analyser;
        private readonly Func<int, string> _texteEstimation;   // durée estimée pour ce nombre de parties
        private bool _miseAJour;                            // la liste est en train d'être remplie (ItemCheck ignoré)

        public List<PartieEchecsPGN> PartiesChoisies => [.. _parties.Where((p, i) => _cochees[i])];

        public FenetreAnalyseFichier(string nomFichier, string nomSortie, List<PartieEchecsPGN> parties, int dejaAnalysees,
                                     Func<int, string> texteEstimation)
        {
            _parties = parties;
            _cochees = [.. parties.Select(p => true)];
            _texteEstimation = texteEstimation;
            Text = T("Analyse d'un fichier PGN");
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(700, 520);
            string entete = T("{0} : {1} partie(s) à analyser (analyse rapide, en arrière-plan : vous pouvez continuer à jouer).", nomFichier, parties.Count)
                + (dejaAnalysees > 0 ? "\n" + T("{0} partie(s) déjà dans {1}, non proposée(s).", dejaAnalysees, nomSortie) : "\n" + T("Résultat : {0}", nomSortie));
            KryptonLabel titre = new() { Text = entete, Location = new Point(12, 4), AutoSize = true };
            KryptonLabel libelleFiltre = new() { Text = T("Joueur :"), Location = new Point(12, 42), AutoSize = true };
            KryptonButton toutCocher = new() { Text = T("Tout cocher"), Location = new Point(400, 38), Size = new Size(140, 28) };
            KryptonButton toutDecocher = new() { Text = T("Tout décocher"), Location = new Point(548, 38), Size = new Size(140, 28) };
            KryptonLabel aide = new() { Text = T("Le fichier d'origine n'est jamais modifié ; une analyse interrompue reprend où elle s'était arrêtée."),
                                        Location = new Point(12, 414), AutoSize = true };
            _analyser = new() { Text = T("Analyser"), Location = new Point(478, 480), Size = new Size(100, 28), DialogResult = DialogResult.OK };
            KryptonButton annuler = new() { Text = T("Annuler"), Location = new Point(588, 480), Size = new Size(100, 28), DialogResult = DialogResult.Cancel };
            toutCocher.Click += (s, e) => CocheLesAffichees(true);
            toutDecocher.Click += (s, e) => CocheLesAffichees(false);
            _filtre.TextChanged += (s, e) => RemplitLaListe();
            _liste.ItemCheck += (s, e) =>
            {
                if (_miseAJour)
                    return;
                _cochees[_affichees[e.Index]] = e.NewValue == CheckState.Checked;
                MetAJourEstimation();
            };
            Controls.AddRange([titre, libelleFiltre, _filtre, toutCocher, toutDecocher, _liste, aide, _estimation, _analyser, annuler]);
            AcceptButton = _analyser;
            CancelButton = annuler;
            RemplitLaListe();
        }

        private static string Description(PartieEchecsPGN p)
        {   // "Duroselle – Courtois   1-0   Nationale IV, ronde 3, 2026.11.08"
            string details = string.Join(", ", new[] { p.Tournoi, string.IsNullOrWhiteSpace(p.Ronde) ? "" : T("ronde {0}", p.Ronde), p.Date }
                                                   .Where(t => !string.IsNullOrWhiteSpace(t) && t != "?" && t != "????.??.??"));
            return $"{p.White} – {p.Black}   {p.Result}   {details}";
        }

        private void RemplitLaListe()
        {   // Les parties dont un joueur contient le texte du filtre (toutes s'il est vide), avec leur case gardée
            string filtre = _filtre.Text.Trim();
            _miseAJour = true;
            _liste.BeginUpdate();
            _liste.Items.Clear();
            _affichees.Clear();
            for (int i = 0; i < _parties.Count; i++)
            {
                PartieEchecsPGN p = _parties[i];
                if (filtre != "" && !p.White.Contains(filtre, StringComparison.CurrentCultureIgnoreCase)
                                 && !p.Black.Contains(filtre, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                _affichees.Add(i);
                _liste.SetItemChecked(_liste.Items.Add(Description(p)), _cochees[i]);
            }
            _liste.EndUpdate();
            _miseAJour = false;
            MetAJourEstimation();
        }

        private void CocheLesAffichees(bool cochee)
        {   // "Tout cocher" / "Tout décocher" : seulement les parties affichées (celles du joueur filtré)
            foreach (int i in _affichees)
                _cochees[i] = cochee;
            RemplitLaListe();
        }

        private void MetAJourEstimation()
        {   // (_cochees suit les cases : ItemCheck, qui arrive avant le changement affiché, l'a déjà mis à jour)
            int nombre = _cochees.Count(c => c);
            _estimation.Text = nombre == 0 ? T("Aucune partie cochée.") : T("{0} partie(s) cochée(s) : {1}", nombre, _texteEstimation(nombre));
            _analyser.Enabled = nombre > 0;
        }
    }
}
