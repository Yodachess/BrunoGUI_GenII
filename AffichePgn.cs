// ┌▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄┐
// █ BrunoGUI_GenII - Interface graphique d'échecs en C# WinForms           █
// █ Copyright (C) 2026 Bruno COURTOIS                                      █
// █ SPDX-License-Identifier: GPL-3.0-or-later                              █
// █ See the LICENSE file in the project root for full license information. █
// └▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀┘

// Fenêtre d'"affichage du pgn (Intl, Fr, algébrique, UCI, FEN) de la partie courante ...
//  └─ Classe "AffichePgn" qui affiche le pgn de la partie courante
//              ├─ "AffichePgn"
//              ├─ "AffichePgnDansZone"  
//              ├─ "AffichePgnIntl_Click"  
//              ├─ "AffichePgnFr_Click"  
//              ├─ "ListeNalAfficheNal_Click"  
//              ├─ "AfficheCoupsUci_Click"  
//              ├─ "ListeFenAffichePgn_Click"  
//              ├─ "ExtraireEntetePgn"  
//              ├─ "MasqueAffichePgn_Click"  
//              └─ "AffichePgn_FormClosing"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BrunoGUI_GenII
{
    public partial class AffichePgn : Form
    {
        private readonly AffichePgn afficheZone;
        private string partieFormatPgnIntl;
        private string partieFormatPgnFr;

        public AffichePgn()
        {
            InitializeComponent();
            afficheZone = this;
        }
        public void AffichePgnDansZone(string contenuPgnIntl, string contenuPgnFr)
        {
            partieFormatPgnIntl = contenuPgnIntl;
            partieFormatPgnFr = contenuPgnFr;
            ZoneAffichage.Text = partieFormatPgnIntl;
        }
        private void AffichePgnIntl_Click(object sender, EventArgs e)
        {   // Affiche la partie au format PGN international
            afficheZone.Show();
            afficheZone.ZoneAffichage.Text = partieFormatPgnIntl;
        }
        private void AffichePgnFr_Click(object sender, EventArgs e)
        {   // Affiche la partie au format PGN francais
            afficheZone.Show();
            afficheZone.ZoneAffichage.Text = partieFormatPgnFr;
        }
        private void ListeNalAfficheNal_Click(object sender, EventArgs e)
        {   // Affiche les coups au format Algébrique long + entête PGN
            afficheZone.Show();
            string contenuNal;
            contenuNal = "Liste de coups au format Nal \nNombre de 1/2 coups = " + LogiqueMouvements.ListeCoupsNal.Count + "\n\n";
            contenuNal = contenuNal + ExtraireEntetePgn(partieFormatPgnIntl) + "\n\n";
            for (int i = 0; i < LogiqueMouvements.ListeCoupsNal.Count; i++)     // Parcours de la liste des Nal
            {
                contenuNal += LogiqueMouvements.ListeCoupsNal[i];
            }
            var matches = Regex.Matches(partieFormatPgnIntl, @"(1-0|0-1|1/2-1/2|\*)(?=\s|$|\)|\])");
            if (matches.Count > 0)
            {   // Récupére le résultat de la partie (1-0, 0-1, 1/2-1/2 ou *) s'il est présent dans le PGN
                contenuNal += " " + matches[^1].Value; // dernier
            }
            afficheZone.ZoneAffichage.Text = contenuNal;
            Debug.WriteLine(contenuNal);
        }
        private void AfficheCoupsUci_Click(object sender, EventArgs e)
        {   // Affiche les coups au format UCI + entête PGN
            afficheZone.Show();
            string contenuUci;
            contenuUci = "Liste de coups au format UCI \nNombre de 1/2 coups = " + LogiqueMouvements.ListeCoupsFen.Count + "\n\n";
            contenuUci = contenuUci + ExtraireEntetePgn(partieFormatPgnIntl) + "\n\n";
            for (int i = 0; i < LogiqueMouvements.ListeCoupsUci.Count; i++)     // Parcours de la liste des Uci
            {
                contenuUci += LogiqueMouvements.ListeCoupsUci[i];
            }
            afficheZone.ZoneAffichage.Text = contenuUci;
            Debug.WriteLine(contenuUci);
        }
        private void ListeFenAffichePgn_Click(object sender, EventArgs e)
        {   // Affiche la liste des FEN de la partie
            afficheZone.Show();
            string contenuFen;
            contenuFen = "Nombre de 1/2 coups = " + LogiqueMouvements.ListeCoupsFen.Count + "\n";
            for (int i = 0; i < LogiqueMouvements.ListeCoupsFen.Count; i++)     // Parcours de la liste des FEN
            {
                contenuFen = contenuFen + "  [" + i + "]: \"" + LogiqueMouvements.ListeCoupsFen[i] + "\"" + " \n";
            }
            afficheZone.ZoneAffichage.Text = contenuFen;
        }
        private static string ExtraireEntetePgn(string pgn)
        {   // Retourne l'entête (ce qui est entre [...]) d'une partie complète en PGN
            var lignes = pgn.Split(["\r\n", "\n"], StringSplitOptions.None);
            var entete = new List<string>();
            foreach (var line in lignes)
            {
                var l = line.Trim();
                if (l.StartsWith('['))
                    entete.Add(line);
                else
                    break;
            }
            return string.Join("\n", entete);
        }
        private void MasqueAffichePgn_Click(object sender, EventArgs e)
        {   // Masque la fenêtre sans la fermer
            this.Hide();
        }
        private void AffichePgn_FormClosing(object sender, FormClosingEventArgs e)
        {   // le formulaire n’est jamais fermé, juste masqué.
            e.Cancel = true;  // Annule la fermeture de la fenêtre
            this.Hide();      // Masque la fenêtre au lieu de la fermer;
        }
    }
}